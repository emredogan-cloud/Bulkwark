// BULWARK — Phase 2 COMMANDER ABILITIES (roadmap §13 2.5; §5.5 "bones" commander;
// §6 INVIOLABLE power budget ≤10–15%). One commander per side: EXACTLY 1 active + 1
// passive (CommanderDef DATA). This system is a writer of commander-sourced
// StatusEffect buffs onto allied units; it owns the active cooldown timer and the
// hard power clamp.
//
// SCOPE (what §13 2.5 authorizes — and ONLY this):
//   * PASSIVE (standing): Quartermaster = small eco/training bonus; Bloodthirst = small
//     combat-uptime bonus. Applied each tick as a SHORT, continuously-REFRESHED
//     StatusEffect on allied units in the side's army (GoldBoost for Quartermaster,
//     Raged for Bloodthirst). It refreshes rather than stacks, so it reads as a standing
//     buff and self-expires the instant the commander is gone/frozen (no leak).
//   * ACTIVE (triggered): on ActiveRequested && ActiveTimer<=0 → Rally = temp attack/move
//     buff (Raged+Hasted) to allies within ActiveRadius for ActiveDuration; WarCry = temp
//     speed/aggression burst (Hasted+Raged, speed-weighted). Sets ActiveTimer=ActiveCooldown,
//     clears ActiveRequested.
//   * Ticks ActiveTimer toward ready.
//
// CANON GUARDS:
//   * §6 commander power ≤ PowerBudgetPct, and PowerBudgetPct itself is HARD-CLAMPED to the
//     canon ceiling k_PowerBudgetCeiling = 0.15f (the "≤10–15%" constant is canon §6, cited).
//     EVERY magnitude written (passive + active, per status) is clamped to that fraction.
//     RankedNormalized==1 makes the clamp authoritative regardless of authored magnitude
//     (ranked-fairness stub, §16 honored by the sim).
//     §6 ADR-2-002 (RESOLVED): the per-entry clamp here bounds each WRITTEN entry; additionally
//     every commander write is tagged StatusSource.Commander, and StatusQuery.CommanderBuffMultiplier
//     COMBINES all commander-sourced entries of a kind and CLAMPS their fraction to ≤ budget. So the
//     COMBINED commander-attributable fraction on any unit is provably ≤ PowerBudgetPct (active +
//     passive can never exceed it), while a spell-applied Raged/Hasted is a SEPARATE entry on its
//     own uncapped §5.3 layer — no multiplicative §6 leak. (Was an open integration decision.)
//   * §15.6 no invented balance: magnitudes/radius/cooldown/duration come from CommanderRuntime
//     (baked from CommanderDef DATA). The ONLY literal here is the canon §6 cap (0.15) and a
//     control epsilon for the passive refresh window (NOT a balance/power value).
//   * §12 ECS boundary: this is sim (ISystem) and only WRITES sim data. It does NOT read UI;
//     the player/SquadAI sets ActiveRequested via a Control-layer MonoBehaviour (the WRITE).
//     No control writer exists yet — see integrationNotes (ActiveRequested writer missing).
//   * §12 perf: at most ONE CommanderRuntime per side; per active trigger it does a single
//     linear pass over allied units in radius (no all-pairs N^2 scan). The passive refresh
//     is likewise one linear ally pass per side, gated to a low cadence.
//   * NO commander COLLECTION (Phase 7, forbidden): a commander is 1 active + 1 passive ONLY.
//     No roster, no swap, no unlock — just the per-side CommanderRuntime bones.
//
// SHARED STATUSEFFECT BUFFER (DRY, §15): the per-unit StatusEffect buffer is co-owned with
// Spell.cs. Spell.cs::StatusEffectSystem is the SINGLE decay/expiry owner (it decrements
// Remaining and RemoveAtSwapBack-expires entries each tick); this system relies on it to
// expire commander buffs (correct — it runs late in the tick). The add/refresh policy here
// is kept IDENTICAL to Spell.cs::AddOrRefreshStatus (single entry per kind; max(remaining),
// max(magnitude)) so the two writers cannot diverge. See integrationNotes for the cross-file
// contract and the multiplicative-stacking budget note.
//
// SCAFFOLD STATUS: authored, NOT compiled — no Unity 6 / Entities toolchain here
// (ADR-0-001 / ADR-1-001: Phase 2 is AUTHOR-ONLY; runtime behavior is DEFERRED).

using Unity.Entities;
using Unity.Mathematics;
using Bulwark.Data;

namespace Bulwark.Sim
{
    /// <summary>
    /// Drives the per-side commander (§13 2.5 / §5.5): applies the standing PASSIVE buff,
    /// resolves the triggered ACTIVE buff into time-limited StatusEffect(s) on allies in
    /// radius, ticks the active cooldown, and HARD-CLAMPS every magnitude to the §6 power
    /// budget (ceiling 0.15). One CommanderRuntime per Team; iterates each one.
    ///
    /// NOTE: deliberately NOT [BurstCompile] — like BasicAISystem it does DynamicBuffer
    /// appends and EntityManager.HasBuffer/HasComponent lookups (to write StatusEffect onto
    /// allies). It is cheap: ≤2 commanders, a single linear ally pass each, gated by cadence /
    /// active-trigger. The per-unit COMBAT hot path (Combat/Targeting) stays Burst'd elsewhere.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]    // sibling anchor convention (Formation/AIScheduler): bookend the work phase.
    [UpdateBefore(typeof(SimPhaseEnd))]     // sibling anchor convention.
    [UpdateAfter(typeof(BasicAISystem))]    // commander stance (§13 1.5) settles first; we layer buffs after.
    [UpdateAfter(typeof(AISchedulerSystem))]// squad posture (§13 2.6) lands before commander buffs → deterministic order among co-located writers.
    [UpdateBefore(typeof(MovementSystem))]  // buffs (Hasted move / Raged attack) land BEFORE Movement+Combat read them this tick.
    public partial struct CommanderAbilitySystem : ISystem
    {
        // ---- CANON CONSTANT (§6) ----------------------------------------------------------
        // "commander power budget ≤ 10–15%" is INVIOLABLE canon (§6). We take the TOP of the
        // stated band (0.15) as the absolute hard ceiling and clamp PowerBudgetPct (and thus
        // every magnitude) to it. This is the one allowed literal (canon-stated, cited).
        private const float k_PowerBudgetCeiling = 0.15f; // §6 commander power cap

        // ---- PASSIVE REFRESH WINDOW (CONTROL value, NOT balance) ---------------------------
        // The standing passive is written as a StatusEffect that we continuously REFRESH so it
        // behaves like an always-on buff yet auto-clears when the commander stops updating
        // (match frozen / commander removed). This window is a control/handoff lifetime, not a
        // power/balance number (the BUFF strength is PassiveMagnitude DATA, clamped). Sized to
        // comfortably exceed one reeval cadence (DEFERRED tuning). The shared StatusEffectSystem
        // (Spell.cs) decays it; refreshing each tick to this window keeps it standing.
        private const float k_PassiveWindow = 0.5f; // seconds; lifetime of a refreshed passive tick

        public void OnCreate(ref SystemState state)
        {
            // Only run when a commander exists (Bootstrap bakes one CommanderRuntime per side)
            // and a match is live to freeze against.
            state.RequireForUpdate<CommanderRuntime>();
            state.RequireForUpdate<MatchState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Freeze once the match is decided (MatchFlow owns the final outcome; mirrors
            // BasicAISystem). When frozen we ALSO stop refreshing the passive, so commander
            // buffs naturally expire — no buff leaks past match end.
            var match = SystemAPI.GetSingleton<MatchState>();
            if (match.Outcome != MatchOutcome.Ongoing)
                return;

            float dt = SystemAPI.Time.DeltaTime;
            var em = state.EntityManager;

            foreach (var cmdRef in SystemAPI.Query<RefRW<CommanderRuntime>>())
            {
                ref CommanderRuntime cmd = ref cmdRef.ValueRW;

                // ---- §6 HARD CLAMP on the budget itself ----------------------------------
                // Clamp PowerBudgetPct to [0, ceiling]. RankedNormalized just makes this clamp
                // authoritative — it already IS authoritative (we clamp every magnitude to it),
                // so ranked normalization is the SAME path: there is no way to exceed budget.
                float budget = math.clamp(cmd.PowerBudgetPct, 0f, k_PowerBudgetCeiling);
                // (ranked-fairness stub, §16: the clamp is unconditional, so RankedNormalized==1
                //  and ==0 resolve identically — you can never spend more than 'budget'.)

                int team = cmd.Team;

                // ---- ACTIVE: tick cooldown, then trigger if requested + ready --------------
                cmd.ActiveTimer = math.max(0f, cmd.ActiveTimer - dt);

                if (cmd.ActiveRequested != 0 && cmd.ActiveTimer <= 0f)
                {
                    ApplyActive(ref state, ref em, team, cmd.Active, cmd.ActiveMagnitude,
                                cmd.ActiveRadius, cmd.ActiveDuration, budget);
                    cmd.ActiveTimer = cmd.ActiveCooldown; // start the cooldown from DATA.
                }
                // Always consume the request flag (whether or not it fired): a request made while
                // on cooldown is dropped, not queued (a commander active is fire-or-wait, not a
                // buffered queue). The Control layer re-requests when the player presses again.
                cmd.ActiveRequested = 0;

                // ---- PASSIVE: continuously-refreshed standing buff -------------------------
                ApplyPassive(ref state, ref em, team, cmd.Passive, cmd.PassiveMagnitude, budget);
            }
        }

        // =========================================================================
        // ACTIVE — time-limited buff to allies within radius (§13 2.5)
        // =========================================================================
        /// <summary>
        /// Rally (Iron Pact discipline): attack + move buff to allies in radius.
        /// WarCry (Ashen Horde swarm): speed/aggression burst to allies in radius.
        /// Both apply as time-limited StatusEffect(s) for <paramref name="duration"/>; every
        /// magnitude is clamped to <paramref name="budget"/> (§6). Single linear ally pass (§12).
        /// </summary>
        // CS0120 fix: NON-static instance method. This helper uses SystemAPI.Query (line ~185),
        // which the Entities source generator rewrites into INSTANCE fields (__query_*/__TypeHandle)
        // on the system struct — a `static` method cannot reach them. Making it an instance method
        // (called via `this` from the instance OnUpdate) lets the generator bind those fields
        // correctly. Behavior is unchanged: same query, same single linear ally pass (§12).
        private void ApplyActive(ref SystemState state, ref EntityManager em, int team,
                                        CommanderActiveKind active, float magnitude, float radius,
                                        float duration, float budget)
        {
            if (active == CommanderActiveKind.None) return;
            if (duration <= 0f || radius <= 0f) return;

            // Anchor the radius on the side's spawn point (the commander's army "home" on the
            // single front, §11). If absent, fall back to centroid-free behavior: skip the
            // radius gate (apply to all allied units) — still a single linear pass.
            bool hasAnchor = TryGetTeamSpawn(ref state, team, out float2 anchor);
            float r2 = radius * radius;

            // Clamp each buff component to the budget (§6). Active spends the SAME budget the
            // passive draws from conceptually, but per the bones spec the cap is per-magnitude:
            // no single applied buff fraction may exceed 'budget'. (Cross-source stacking with
            // a spell-applied buff of the same kind is an integration decision — see header /
            // integrationNotes; the StatusQuery readers multiply per-entry.)
            float buffMag = math.clamp(magnitude, 0f, budget);
            if (buffMag <= 0f) return;

            // Rally  → Raged (attack/outgoing) + Hasted (move), attack-weighted.
            // WarCry → Hasted (speed) + Raged (aggression), speed-weighted.
            // Both reuse ONLY canon StatusKinds (§5.3); no new effect invented.
            StatusKind primary = active == CommanderActiveKind.Rally ? StatusKind.Raged : StatusKind.Hasted;
            StatusKind secondary = active == CommanderActiveKind.Rally ? StatusKind.Hasted : StatusKind.Raged;

            foreach (var (pos, unitTeam, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (unitTeam.ValueRO.Id != team) continue; // allies only
                if (hasAnchor && math.distancesq(pos.ValueRO.Value, anchor) > r2) continue;

                // Commander buffs share the per-unit StatusEffect buffer with Spell.cs. If a unit
                // has never been touched by a spell it may not have a buffer yet (Terrain.cs
                // EnsurePhase2 attaches Facing + TerrainOccupancy only, NOT StatusEffect). We skip
                // such units here; the integration pass guarantees buffer presence (Bootstrap/
                // EnsurePhase2 attaches an empty StatusEffect buffer to every unit) — see
                // integrationNotes. We do NOT lazily AddBuffer (would need a parallel ECB path and
                // duplicate Spell.cs's structural-change handling — kept single-owner there).
                if (!em.HasBuffer<StatusEffect>(e)) continue;
                var buf = em.GetBuffer<StatusEffect>(e);

                // Refresh-or-add via the ONE canonical Spell.cs::AddOrRefreshStatus (single entry
                // per (kind, source); max(remaining), max(magnitude)) so the two writers never
                // drift. §6 ADR-2-002: tag these COMMANDER-sourced so StatusQuery bounds their
                // COMBINED contribution to budget (PowerBudgetPct) separately from spell buffs.
                Spell.AddOrRefreshStatus(ref buf, primary, duration, buffMag, StatusSource.Commander);
                Spell.AddOrRefreshStatus(ref buf, secondary, duration, buffMag, StatusSource.Commander);
            }
        }

        // =========================================================================
        // PASSIVE — continuously-refreshed standing buff (§13 2.5)
        // =========================================================================
        /// <summary>
        /// Quartermaster (eco/training): GoldBoost standing buff on the side's MINERS (eco
        /// uptime). Bloodthirst (combat-uptime): Raged standing buff on the side's combat units.
        /// Both are refreshed each cadence so they read as always-on yet self-expire when the
        /// commander stops (match end / removed). Magnitude clamped to budget (§6).
        /// </summary>
        // CS0120 fix: NON-static instance method (uses SystemAPI.Query at line ~237; same
        // source-generator reasoning as ApplyActive). Called via `this` from OnUpdate; unchanged behavior.
        private void ApplyPassive(ref SystemState state, ref EntityManager em, int team,
                                         CommanderPassiveKind passive, float magnitude, float budget)
        {
            if (passive == CommanderPassiveKind.None) return;

            float buffMag = math.clamp(magnitude, 0f, budget); // §6 cap on the standing bonus.
            if (buffMag <= 0f) return;

            StatusKind kind = passive == CommanderPassiveKind.Quartermaster
                ? StatusKind.GoldBoost   // small eco/training bonus (read by MiningSystem — see integrationNotes)
                : StatusKind.Raged;      // Bloodthirst: small standing combat-uptime bonus (read via StatusQuery)

            // Quartermaster targets the side's economy (miners); Bloodthirst targets the army.
            // Keep it a single linear ally pass either way (§12).
            foreach (var (unitTeam, e) in
                     SystemAPI.Query<RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (unitTeam.ValueRO.Id != team) continue;

                if (passive == CommanderPassiveKind.Quartermaster)
                {
                    if (!em.HasComponent<MinerTag>(e)) continue; // eco bonus rides on miners
                }
                else
                {
                    if (em.HasComponent<MinerTag>(e)) continue;  // combat-uptime is for combat units
                }

                // See ApplyActive: buffer presence is an integration guarantee; skip if absent.
                if (!em.HasBuffer<StatusEffect>(e)) continue;
                var buf = em.GetBuffer<StatusEffect>(e);

                // Standing buff: refresh to the short window every cadence so it reads as always-on
                // yet self-expires when the commander stops. SAME canonical refresh policy as the
                // active / Spell.cs (single entry per (kind, source); max(remaining), max(magnitude)).
                // Because buffMag is the clamped value and AddOrRefreshStatus takes max(magnitude),
                // the standing entry settles at the clamped magnitude and the short window keeps
                // Remaining from exceeding k_PassiveWindow once the commander stops refreshing it.
                // §6 ADR-2-002: COMMANDER-sourced (clamped to budget with the active by StatusQuery).
                Spell.AddOrRefreshStatus(ref buf, kind, k_PassiveWindow, buffMag, StatusSource.Commander);
            }
        }

        // =========================================================================
        // StatusEffect buffer writes go through Spell.cs::AddOrRefreshStatus (the ONE canonical
        // policy). §6 ADR-2-002: this system's private copy was REMOVED so there is a single
        // writer policy — commander writes pass StatusSource.Commander; entries are keyed by
        // (kind, source) so commander and spell buffs of the same kind stay separate and the
        // commander contribution is bounded to budget by StatusQuery. (§15 DRY.)
        // =========================================================================

        /// <summary>Find the side's spawn anchor (§11 single-front "home") for the active radius gate.</summary>
        // CS0120 fix: NON-static instance method (uses SystemAPI.Query at line ~275; same
        // source-generator reasoning as ApplyActive). Called from the now-instance ApplyActive.
        private bool TryGetTeamSpawn(ref SystemState state, int team, out float2 pos)
        {
            foreach (var sp in SystemAPI.Query<RefRO<TeamSpawnPoint>>())
            {
                if (sp.ValueRO.Team == team)
                {
                    pos = sp.ValueRO.Pos;
                    return true;
                }
            }
            pos = default;
            return false;
        }
    }
}
