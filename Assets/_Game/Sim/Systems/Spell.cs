// BULWARK — Phase 2.4 SPELL SYSTEM (battle-sim hot path). Roadmap §13 2.4 ("draft 3"),
// §5.3 (the spell bible: cooldown + charges, TELEGRAPH window, COUNTER, SYNERGY tags),
// §4 ("ONE combat core" — spells feed the SAME type×armor modifier chain via
// CounterMatrix.Lookup; NO forked combat path), §12 (ECS boundary: this is pure sim
// ISystem/SystemAPI; input/draft live in the Control MonoBehaviour, which only WRITES data),
// §15 (canon closed — spells stay strictly within the 5 §5.3 categories; no new mechanics).
//
// WHAT THIS FILE OWNS (and only this):
//   1. SpellCatalogEntry / SpellCatalogTag — a Burst-friendly value copy of the SpellDef pool,
//      baked once by BattleBootstrap (index == SpellSlot.SpellIndex). DATA only (§15.6).
//   2. StatusQuery — the SINGLE SOURCE OF TRUTH for status→chain math (Chilled/Hasted move,
//      Raged outgoing dmg, Stunned cannot-act). Movement/Targeting/Combat READ these (the
//      integration pass wires those reads — see the REQUIRED INTEGRATION EDIT block at EOF).
//      Magnitudes are DATA (StatusEffect.Magnitude, derived from SpellDef.magnitude). No
//      invented balance constants live here.
//   3. AddOrRefreshStatus — the canonical status add/refresh policy (ONE entry per kind;
//      max(Remaining)/max(Magnitude) on refresh). CommanderAbility.cs keeps an IDENTICAL
//      private copy and relies on these exact semantics so the two writers cannot diverge.
//   4. SpellSlotCooldownSystem — ticks each side's drafted SpellSlot.Cooldown down.
//   5. SpellCastSystem — consumes SpellCastRequest from the SpellCastInboxTag inbox; validates
//      the slot (off cooldown && charges>0); spends a charge, sets Cooldown from the catalog,
//      spawns an ActiveTelegraph (the §5.3 counterplay window). Clears consumed requests.
//   6. TelegraphResolveSystem — ticks ActiveTelegraph.Remaining; at ≤0 RESOLVES by category /
//      target shape, applying SYNERGY, then destroys the telegraph entity. Offensive damage
//      goes through CounterMatrix.Lookup (the SAME §4 typeArmor read CombatSystem uses) and
//      routes statue hits to StatueDamageInbox (StatueDamageSystem owns shield/phase math).
//   7. StatusEffectSystem — the SINGLE decay/expiry owner of the per-unit StatusEffect buffer:
//      ticks Remaining, removes expired entries, applies Burning/Poisoned DoT to Health (statue
//      → inbox). It does NOT re-implement the read-side semantics (those are StatusQuery).
//
// §5.3 NO UN-COUNTERABLE SPELL: every spell carries telegraphTime>0 (SpellCatalogEntry.TelegraphTime;
// SpellCastSystem refuses a 0 window — see k_MinTelegraph) AND a SpellDef.counterNote (data). The
// telegraph entity is the live, observable counterplay window the opponent dodges/cleanses/spreads.
//
// §12 PERF (~O(1) per agent): cooldown tick is O(slots)=O(3)/side; cast consume is O(requests);
// telegraph resolution iterates units IN RANGE using the existing Position/Team query (a single
// linear pass per resolving telegraph, NOT an all-pairs N^2 scan); status decay is O(1)/unit.
//
// SCAFFOLD STATUS: authored, NOT compiled — no Unity 6 / Entities toolchain here
// (ADR-0-001 / ADR-1-001: Phase 2 is AUTHOR-ONLY; runtime behavior is DEFERRED to a Unity build).

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Bulwark.Data;

namespace Bulwark.Sim
{
    // =====================================================================================
    // 1. SPELL CATALOG (Burst-friendly value copy of the SpellDef pool, §15.6 DATA-only)
    // =====================================================================================
    /// <summary>
    /// Burst-friendly runtime value copy of the SpellDef fields the sim needs (§13 2.4 / §5.3).
    /// One entry per spell in the shared pool; the buffer index == SpellSlot.SpellIndex, so a
    /// drafted slot dereferences its definition in O(1). All values are DATA copied verbatim
    /// from the authored SpellDef (§15.6 no invented balance) — this struct introduces NO
    /// constants. Managed-only SpellDef fields (id / displayName / counterNote string) are NOT
    /// copied: the counter is a §5.3 design/telegraph property, the sim only needs the numbers.
    /// </summary>
    public struct SpellCatalogEntry : IBufferElementData
    {
        public SpellCategory Category;          // §5.3 Offensive/Control/Economy/Summon/Buff
        public float Cooldown;                  // seconds between casts (set onto SpellSlot.Cooldown on cast)
        public int Charges;                     // §5.3 charge count (drafted into SpellSlot.ChargesLeft)
        public float TelegraphTime;             // §5.3 counterplay window (MUST be > 0; cast refuses 0)
        public TargetShape TargetShape;         // Point/Area/Line/Self/AllyArea (resolution geometry)
        public float Radius;                    // area/line radius for telegraph + resolution
        public float Magnitude;                 // damage / gold / DoT-per-sec / buff strength (per category)
        public float Duration;                  // status/buff seconds (0 = instant)
        public StatusKind AppliesStatus;        // status inflicted/granted (e.g. Chilled, Raged)
        public int SummonUnitIndex;             // Summon: index into the caster side's UnitCatalog (-1 = none)
        public StatusKind SynergyBonusVsStatus; // §5.3 synergy: extra effect vs a target already in this status
        public float SynergyMultiplier;         // multiplier applied when the synergy condition holds (e.g. Shatter)
    }

    /// <summary>
    /// Singleton marker for the entity that holds the DynamicBuffer&lt;SpellCatalogEntry&gt;
    /// (the baked spell pool). BattleBootstrap bakes it from the authored SpellDef pool — the
    /// integration pass wires the bake (one BuildSpellCatalog pass mirroring BuildUnitCatalog;
    /// see the REQUIRED INTEGRATION EDIT block at the end of this file). SpellCastSystem and
    /// TelegraphResolveSystem read it; index into the buffer == SpellSlot.SpellIndex.
    /// </summary>
    public struct SpellCatalogTag : IComponentData { }

    // =====================================================================================
    // 2. StatusQuery — SINGLE SOURCE OF TRUTH for status → §4 modifier-chain math (§5.3)
    // =====================================================================================
    /// <summary>
    /// Pure, Burst-safe read-side helpers over a unit's DynamicBuffer&lt;StatusEffect&gt;. These
    /// are the ONLY place status effects are translated into chain multipliers / gates, so the
    /// movement / targeting / combat systems all agree (DRY, §15). They are READ-ONLY: they
    /// never mutate the buffer (decay/expiry is StatusEffectSystem's job).
    ///
    /// Magnitudes come straight from StatusEffect.Magnitude (data-derived from SpellDef.magnitude
    /// and the §6-clamped CommanderRuntime magnitudes) — NO invented balance constants. A status
    /// magnitude is interpreted as a FRACTION: Chilled 0.4 ⇒ −40% move; Hasted 0.4 ⇒ +40% move;
    /// Raged 0.3 ⇒ +30% outgoing damage. Multiple entries of the same kind cannot occur
    /// (AddOrRefreshStatus keeps one per kind), so each multiplier reads at most one entry/kind.
    ///
    /// CONTRACT WITH CommanderAbility.cs: that file's header names
    /// StatusQuery.OutgoingDamageMultiplier and StatusQuery.MoveSpeedMultiplier as the readers
    /// that multiply commander-applied Raged/Hasted/etc. Those exact method names are provided
    /// below so the cross-file reference resolves.
    ///
    /// §6 ADR-2-002 (commander/spell buff-stacking): the buff multipliers are SPLIT BY SOURCE.
    /// The no-budget overloads (MoveSpeedMultiplier(buf) / OutgoingDamageMultiplier(buf)) read
    /// SPELL-sourced entries ONLY — the §5.3 tactical layer, intentionally stacking, uncapped by
    /// §6. The commander contribution is a SEPARATE factor: CommanderBuffMultiplier(buf, kind,
    /// budget) COMBINES all Commander-sourced entries of a kind and CLAMPS their extra fraction to
    /// ≤ budget (PowerBudgetPct), so the commander-attributable buff on any unit is provably ≤
    /// budget no matter how many commander entries (active + passive) exist. The budget-taking
    /// overloads return spellMult × commanderClampedMult — the NET multiplier a call site applies.
    /// </summary>
    [BurstCompile]
    public static class StatusQuery
    {
        /// <summary>
        /// SPELL-only move-speed multiplier from statuses (§5.3 Control/Buff): Chilled SLOWS
        /// (×(1−mag), floored at 0), Hasted SPEEDS (×(1+mag)) — SPELL-sourced entries only (§6
        /// ADR-2-002; the commander Hasted contribution is added separately, clamped). Other kinds
        /// are neutral here. Returns 1.0 on an empty/absent buffer. (Chilled has no commander
        /// source — only spells slow — so its full effect stays here.)
        /// </summary>
        public static float MoveSpeedMultiplier(in DynamicBuffer<StatusEffect> buf)
        {
            float mult = 1f;
            for (int i = 0; i < buf.Length; i++)
            {
                StatusEffect se = buf[i];
                if (se.Remaining <= 0f) continue; // expired-but-not-yet-swept entry: ignore.
                if (se.Source != StatusSource.Spell) continue; // §6 ADR-2-002: spell layer only.
                if (se.Kind == StatusKind.Chilled) mult *= math.max(0f, 1f - se.Magnitude);
                else if (se.Kind == StatusKind.Hasted) mult *= (1f + se.Magnitude);
            }
            return mult;
        }

        /// <summary>
        /// NET move-speed multiplier = SPELL-only mult × CLAMPED commander Hasted contribution
        /// (§6 ADR-2-002). <paramref name="commanderBudget"/> is the unit-commander's
        /// PowerBudgetPct: the combined Commander-sourced Hasted fraction is capped to ≤ that, so
        /// the commander-attributable speed buff can never exceed the §6 budget while spell Hasted/
        /// Chilled remain their own uncapped layer. Read by MovementSystem.
        /// </summary>
        public static float MoveSpeedMultiplier(in DynamicBuffer<StatusEffect> buf, float commanderBudget)
            => MoveSpeedMultiplier(in buf)
             * CommanderBuffMultiplier(in buf, StatusKind.Hasted, commanderBudget);

        /// <summary>
        /// SPELL-only OUTGOING damage multiplier from statuses (§5.3 Buff): Raged boosts outgoing
        /// damage (×(1+mag)) — SPELL-sourced entries only (§6 ADR-2-002; the commander Raged
        /// contribution is added separately, clamped to budget). Multiplies INTO the §4 chain, it
        /// does not fork it. This is the exact name CommanderAbility.cs references. Returns 1.0 on
        /// an empty/absent buffer.
        /// </summary>
        public static float OutgoingDamageMultiplier(in DynamicBuffer<StatusEffect> buf)
        {
            float mult = 1f;
            for (int i = 0; i < buf.Length; i++)
            {
                StatusEffect se = buf[i];
                if (se.Remaining <= 0f) continue;
                if (se.Source != StatusSource.Spell) continue; // §6 ADR-2-002: spell layer only.
                if (se.Kind == StatusKind.Raged) mult *= (1f + se.Magnitude);
            }
            return mult;
        }

        /// <summary>
        /// NET outgoing-damage multiplier = SPELL-only mult × CLAMPED commander Raged contribution
        /// (§6 ADR-2-002). <paramref name="commanderBudget"/> is the unit-commander's
        /// PowerBudgetPct: the combined Commander-sourced Raged fraction is capped to ≤ that, so the
        /// commander-attributable damage buff can never exceed the §6 budget while spell Raged
        /// remains its own uncapped tactical layer. Read by CombatSystem (joins the §4 chain).
        /// </summary>
        public static float OutgoingDamageMultiplier(in DynamicBuffer<StatusEffect> buf, float commanderBudget)
            => OutgoingDamageMultiplier(in buf)
             * CommanderBuffMultiplier(in buf, StatusKind.Raged, commanderBudget);

        /// <summary>
        /// §6 ADR-2-002 — the COMMANDER-attributable buff multiplier for one <paramref name="kind"/>:
        /// SUMS the magnitudes of ALL live Commander-sourced entries of that kind, CLAMPS the
        /// combined extra fraction to ≤ <paramref name="budget"/> (PowerBudgetPct, the §6 cap), and
        /// returns 1 + that clamped fraction. Thus commander active + passive of the same kind can
        /// NEVER push the commander-attributable fraction past the budget on any unit (the §6
        /// fairness leak is closed). Spell-sourced entries are ignored here (they are a separate,
        /// uncapped layer). A non-positive budget ⇒ neutral 1.0. Pure/Burst-safe (read-only buffer).
        /// </summary>
        public static float CommanderBuffMultiplier(in DynamicBuffer<StatusEffect> buf,
                                                    StatusKind kind, float budget)
        {
            if (budget <= 0f) return 1f; // no commander budget ⇒ no commander-attributable buff.
            float sum = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                StatusEffect se = buf[i];
                if (se.Remaining <= 0f) continue;
                if (se.Source != StatusSource.Commander || se.Kind != kind) continue;
                sum += se.Magnitude; // COMBINE all commander entries of this kind (active + passive).
            }
            return 1f + math.min(sum, budget); // CLAMP the combined commander fraction to ≤ budget.
        }

        /// <summary>
        /// Alias of <see cref="OutgoingDamageMultiplier"/> under the shorter name the Phase-2.4
        /// brief uses ("DamageMultiplier"). Same single source of truth — kept so either name
        /// resolves to identical math (no divergence). Prefer OutgoingDamageMultiplier in new code.
        /// </summary>
        public static float DamageMultiplier(in DynamicBuffer<StatusEffect> buf)
            => OutgoingDamageMultiplier(in buf);

        /// <summary>
        /// INCOMING damage multiplier from statuses. No §5.3 category currently shifts incoming
        /// damage (Chilled/Burning/Poisoned/Stunned/Hasted/Raged/GoldBoost are move/dot/gate/
        /// outgoing only), so this is neutral 1.0 today. Provided as the single hook the
        /// integration pass would extend if a future DATA spell ever applies a damage-taken
        /// status — keeping that read in ONE place, never re-derived in Combat. (§15: no new
        /// mechanic added now; this stays neutral until DATA drives it.)
        /// </summary>
        public static float IncomingDamageMultiplier(in DynamicBuffer<StatusEffect> buf)
        {
            // Intentionally neutral: iterate so the signature is honest about reading the buffer,
            // but no current StatusKind modifies incoming damage (canon closed, §15).
            for (int i = 0; i < buf.Length; i++)
            {
                // No-op by design; placeholder loop body avoids "unused param" while staying 1.0.
                if (buf[i].Kind == StatusKind.None) { /* unreachable: None is never stored */ }
            }
            return 1f;
        }

        /// <summary>
        /// True if any LIVE Stunned status is present (§5.3 Control): a stunned unit CANNOT ACT —
        /// TargetingSystem skips re-acquire and CombatSystem skips the swing (integration edit at
        /// EOF). Movement may also halt (read in MovementSystem). False on an empty/absent buffer.
        /// </summary>
        public static bool IsStunned(in DynamicBuffer<StatusEffect> buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                StatusEffect se = buf[i];
                if (se.Kind == StatusKind.Stunned && se.Remaining > 0f)
                    return true;
            }
            return false;
        }
    }

    // =====================================================================================
    // 3. AddOrRefreshStatus — canonical add/refresh policy (shared with CommanderAbility.cs)
    // =====================================================================================
    /// <summary>
    /// Static helpers Spell.cs shares with the rest of the sim. Kept here (not nested in a
    /// system) so any writer can reach the ONE canonical status-write policy (§15 DRY).
    /// </summary>
    public static class Spell
    {
        /// <summary>
        /// Refresh-or-add a single StatusEffect of <paramref name="kind"/> (§5.3). Policy
        /// (the ONE canonical add/refresh — CommanderAbility.cs now calls THIS, no private copy):
        /// keep exactly ONE entry per (kind, <paramref name="source"/>); on a matching entry re-up
        /// it to max(Remaining) / max(Magnitude) so a recast EXTENDS rather than stacks (no stacking
        /// exploit); otherwise append a fresh entry. §6 ADR-2-002: entries are keyed by (Kind,
        /// Source), so a Commander-sourced entry is NEVER merged with a Spell-sourced entry of the
        /// same kind — the two layers stay SEPARATE so StatusQuery can bound the commander
        /// contribution to PowerBudgetPct while spell buffs remain their own (uncapped) layer.
        /// Burst-safe: only DynamicBuffer mutation + math.max. The caller guarantees the buffer
        /// exists (creation/decay is owned by StatusEffectSystem).
        /// </summary>
        public static void AddOrRefreshStatus(ref DynamicBuffer<StatusEffect> buf, StatusKind kind,
                                              float remaining, float magnitude,
                                              StatusSource source = StatusSource.Spell)
        {
            if (kind == StatusKind.None) return; // None is never stored (it is the "no status" sentinel).
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i].Kind != kind || buf[i].Source != source) continue; // key by (Kind, Source).
                StatusEffect se = buf[i];
                se.Remaining = math.max(se.Remaining, remaining);
                se.Magnitude = math.max(se.Magnitude, magnitude);
                buf[i] = se;
                return;
            }
            buf.Add(new StatusEffect { Kind = kind, Remaining = remaining, Magnitude = magnitude, Source = source });
        }
    }

    // =====================================================================================
    // 4. SpellSlotCooldownSystem — tick each side's drafted SpellSlot.Cooldown down (§13 2.4)
    // =====================================================================================
    /// <summary>
    /// Decrements every drafted SpellSlot.Cooldown by dt (floored at 0) for both sides. Runs
    /// early in the tick (right after the phase-begin anchor) so SquadAI (§5.3 AI cast axis) and
    /// the player Control shell both observe an up-to-date "is this slot ready?" state THIS tick,
    /// and SpellCastSystem (below) validates against the freshly-ticked cooldown. O(slots)=O(3)
    /// per side (§12). Burst-compiled: pure buffer math, no structural changes.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateBefore(typeof(SquadAISystem))] // AI reads slot readiness this tick; tick cooldowns first.
    public partial struct SpellSlotCooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // One SpellSlot buffer per side, hung on the SpellLoadoutTag singleton-per-side.
            foreach (var (loadout, e) in
                     SystemAPI.Query<RefRO<SpellLoadoutTag>>().WithEntityAccess())
            {
                if (!SystemAPI.HasBuffer<SpellSlot>(e)) continue;
                DynamicBuffer<SpellSlot> slots = SystemAPI.GetBuffer<SpellSlot>(e);
                for (int i = 0; i < slots.Length; i++)
                {
                    SpellSlot s = slots[i];
                    if (s.Cooldown > 0f)
                    {
                        s.Cooldown = math.max(0f, s.Cooldown - dt);
                        slots[i] = s;
                    }
                }
            }
        }
    }

    // =====================================================================================
    // 5. SpellCastSystem — consume SpellCastRequest → validate → spend → spawn telegraph (§5.3)
    // =====================================================================================
    /// <summary>
    /// Drains the shared SpellCastInboxTag inbox (appended by the player Control shell §12 and by
    /// SquadAI's §5.3 cast axis — the SAME path for both). For each request:
    ///   • locate the requesting side's SpellLoadoutTag → SpellSlot buffer;
    ///   • validate SlotIndex in range, slot OFF cooldown (Cooldown<=0) and ChargesLeft>0;
    ///   • read the spell's SpellCatalogEntry (index == SpellSlot.SpellIndex);
    ///   • REFUSE an un-counterable spell: TelegraphTime MUST be > 0 (§5.3) — else skip;
    ///   • spend one charge, set Cooldown from the catalog (DATA, §15.6);
    ///   • spawn an ActiveTelegraph{SpellIndex, CasterTeam, Pos, Radius, Remaining=TelegraphTime,
    ///     TargetEntity} — the live counterplay window TelegraphResolveSystem later resolves.
    /// Then CLEARS the inbox (every request is consumed exactly once). Structural changes (the
    /// telegraph spawn) go through an ECB. Not [BurstCompile]: it walks singletons + per-side
    /// buffers and issues a structural create — mirrors TrainingSystem. O(requests) (§12).
    ///
    /// Runs AFTER SquadAISystem (so AI requests made THIS tick are honored same-tick) and BEFORE
    /// TelegraphResolveSystem (a freshly-cast telegraph ticks its window before it can resolve).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(SpellSlotCooldownSystem))] // validate against freshly-ticked cooldowns.
    [UpdateAfter(typeof(SquadAISystem))]           // see AI casts queued this tick.
    public partial struct SpellCastSystem : ISystem
    {
        // §5.3 INVIOLABLE: no un-counterable spell. A telegraph window at/under this epsilon is
        // treated as "no window" and the cast is REFUSED. This is a correctness guard, not a
        // balance value — the actual window length is DATA (SpellCatalogEntry.TelegraphTime).
        private const float k_MinTelegraph = 1e-4f;

        public void OnUpdate(ref SystemState state)
        {
            // Need both the inbox (cast requests) and the catalog (to read cooldown/telegraph).
            if (!SystemAPI.HasSingleton<SpellCastInboxTag>()) return;
            if (!SystemAPI.HasSingleton<SpellCatalogTag>()) return;

            Entity inboxEntity = SystemAPI.GetSingletonEntity<SpellCastInboxTag>();
            if (!SystemAPI.HasBuffer<SpellCastRequest>(inboxEntity)) return;
            DynamicBuffer<SpellCastRequest> inbox = SystemAPI.GetBuffer<SpellCastRequest>(inboxEntity);
            if (inbox.Length == 0) return;

            Entity catalogEntity = SystemAPI.GetSingletonEntity<SpellCatalogTag>();
            if (!SystemAPI.HasBuffer<SpellCatalogEntry>(catalogEntity)) { inbox.Clear(); return; }
            DynamicBuffer<SpellCatalogEntry> catalog = SystemAPI.GetBuffer<SpellCatalogEntry>(catalogEntity);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            for (int r = 0; r < inbox.Length; r++)
            {
                SpellCastRequest req = inbox[r];

                // Resolve the requesting side's drafted SpellSlot buffer (SpellLoadoutTag per side).
                if (!TryGetLoadout(ref state, req.Team, out Entity loadoutEntity))
                    continue;
                DynamicBuffer<SpellSlot> slots = SystemAPI.GetBuffer<SpellSlot>(loadoutEntity);

                if (req.SlotIndex < 0 || req.SlotIndex >= slots.Length)
                    continue; // bad slot index — drop the request.

                SpellSlot slot = slots[req.SlotIndex];

                // Validate readiness (same gate SquadAI pre-checks; this is the AUTHORITATIVE one).
                if (slot.Cooldown > 0f || slot.ChargesLeft <= 0)
                    continue;

                if (slot.SpellIndex < 0 || slot.SpellIndex >= catalog.Length)
                    continue; // slot points at no valid spell — drop.

                SpellCatalogEntry def = catalog[slot.SpellIndex];

                // §5.3 every spell counterable: a non-positive telegraph window is illegal —
                // refuse the cast (do NOT spend a charge) so no un-counterable spell ever resolves.
                if (def.TelegraphTime <= k_MinTelegraph)
                    continue;

                // ---- SPEND: one charge, set cooldown from DATA (§15.6) ----
                slot.ChargesLeft -= 1;
                slot.Cooldown = def.Cooldown;
                slots[req.SlotIndex] = slot;

                // ---- SPAWN the telegraph (the live §5.3 counterplay window) ----
                // For a Self/AllyArea spell with no explicit aim, the request's TargetPos still
                // anchors the telegraph; resolution re-derives the caster-relative center as needed.
                Entity tele = ecb.CreateEntity();
                ecb.AddComponent(tele, new ActiveTelegraph
                {
                    SpellIndex = slot.SpellIndex,
                    CasterTeam = req.Team,
                    Pos = req.TargetPos,
                    Radius = def.Radius,
                    Remaining = def.TelegraphTime,
                    TargetEntity = req.TargetEntity,
                });
            }

            inbox.Clear(); // every request consumed exactly once (player + AI share this inbox).

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>Locate the SpellLoadoutTag singleton-per-side that carries this team's SpellSlot buffer.</summary>
        private bool TryGetLoadout(ref SystemState state, int team, out Entity loadoutEntity)
        {
            foreach (var (tag, e) in SystemAPI.Query<RefRO<SpellLoadoutTag>>().WithEntityAccess())
            {
                if (tag.ValueRO.Team != team) continue;
                if (!SystemAPI.HasBuffer<SpellSlot>(e)) continue;
                loadoutEntity = e;
                return true;
            }
            loadoutEntity = Entity.Null;
            return false;
        }
    }

    // =====================================================================================
    // 6. TelegraphResolveSystem — tick window; at ≤0 resolve by category + synergy (§5.3, §4)
    // =====================================================================================
    /// <summary>
    /// Ticks every ActiveTelegraph.Remaining; when a window expires (≤0) it RESOLVES the spell at
    /// its Pos/Radius according to the spell's category + target shape, applies §5.3 SYNERGY, and
    /// DESTROYS the telegraph entity. Resolution per category (all within the §5.3 set; §15):
    ///   • Offensive → AoE damage to ENEMY units in radius via the §4 type×armor read
    ///     (CounterMatrix.Lookup — the SAME factor CombatSystem uses; NOT a forked chain). The
    ///     spell's damage type is its SpellCatalogEntry-implied type; we use the caster-side
    ///     offensive interpretation: base = Magnitude, typeArmor by target ArmorClass. Statue
    ///     targets route to StatueDamageInbox (StatueDamageSystem owns shield/phase math).
    ///   • Control → AddOrRefreshStatus(AppliesStatus) on units in radius (Chilled slow / Stunned
    ///     gate / Poisoned DoT seed). Enemy-targeted (control is offensive utility).
    ///   • Economy → add Magnitude Gold to the caster team's GoldStore (LOCAL battle economy only,
    ///     §J — NO persistence), or a GoldBoost status if the spell so specifies.
    ///   • Summon → spawn SummonUnitIndex unit(s) for the caster team reusing the SAME spawn path
    ///       Training uses (UnitCatalog → SpellSummon.SpawnFromCatalog).
    ///   • Buff (AllyArea) → AddOrRefreshStatus(Raged/Hasted/…) on ALLIES in radius.
    /// SYNERGY (§5.3): if a target already carries the spell's SynergyBonusVsStatus, the effect on
    /// THAT target is multiplied by SynergyMultiplier (e.g. Shatter does ×N vs an already-Chilled
    /// target). Applied per-target so it is data-driven, not a global flag.
    ///
    /// Runs AFTER SpellCastSystem (so a just-spawned telegraph is not resolved the same tick it is
    /// cast — its window must elapse) and BEFORE CombatSystem (applied status/damage is visible to
    /// the SAME tick's combat read). Structural changes (damage kills, summon spawns, telegraph
    /// destroy) go through an ECB. Not [BurstCompile]: structural changes + buffer lookups across
    /// many target entities, like TrainingSystem/CommanderAbilitySystem.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(SpellCastSystem))]   // telegraph must exist + age before it can resolve.
    [UpdateBefore(typeof(CombatSystem))]     // applied status/damage lands before this tick's combat.
    public partial struct TelegraphResolveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Catalog is required to resolve any telegraph (it carries the effect numbers).
            if (!SystemAPI.HasSingleton<SpellCatalogTag>()) return;
            Entity catalogEntity = SystemAPI.GetSingletonEntity<SpellCatalogTag>();
            if (!SystemAPI.HasBuffer<SpellCatalogEntry>(catalogEntity)) return;
            DynamicBuffer<SpellCatalogEntry> catalog = SystemAPI.GetBuffer<SpellCatalogEntry>(catalogEntity);

            // Shared §4 type×armor matrix (CounterMatrix.Lookup falls back to neutral 1.0 if absent).
            DynamicBuffer<CounterCell> matrix = default;
            if (SystemAPI.TryGetSingletonEntity<CounterMatrixTag>(out var matrixEntity) &&
                SystemAPI.HasBuffer<CounterCell>(matrixEntity))
                matrix = SystemAPI.GetBuffer<CounterCell>(matrixEntity);

            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (teleRef, teleEntity) in
                     SystemAPI.Query<RefRW<ActiveTelegraph>>().WithEntityAccess())
            {
                ref ActiveTelegraph tele = ref teleRef.ValueRW;

                // Tick the counterplay window; not yet expired → leave it visible to dodge/counter.
                tele.Remaining -= dt;
                if (tele.Remaining > 0f)
                    continue;

                // Window elapsed → RESOLVE. Guard a bad SpellIndex (drop the telegraph cleanly).
                if (tele.SpellIndex < 0 || tele.SpellIndex >= catalog.Length)
                {
                    ecb.DestroyEntity(teleEntity);
                    continue;
                }

                SpellCatalogEntry def = catalog[tele.SpellIndex];

                switch (def.Category)
                {
                    case SpellCategory.Offensive:
                        ResolveOffensive(ref state, ref em, ref ecb, in def, in tele, matrix);
                        break;
                    case SpellCategory.Control:
                        ResolveControl(ref state, ref em, in def, in tele);
                        break;
                    case SpellCategory.Economy:
                        ResolveEconomy(ref state, in def, in tele);
                        break;
                    case SpellCategory.Summon:
                        ResolveSummon(ref state, ref ecb, in def, in tele);
                        break;
                    case SpellCategory.Buff:
                        ResolveBuff(ref state, ref em, in def, in tele);
                        break;
                }

                // Telegraph consumed — destroy it (the window is single-shot; charges/cooldown
                // already spent at cast time in SpellCastSystem).
                ecb.DestroyEntity(teleEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        // ----------------------------------------------------------------- Offensive (§4 chain)
        /// <summary>
        /// AoE damage to ENEMY units within Radius of the telegraph center. Damage is the §4
        /// type×armor read applied to the spell's Magnitude base: dmg = Magnitude × typeArmor,
        /// where typeArmor = CounterMatrix.Lookup(matrix, spellDamageType, targetArmor). The
        /// spell damage type is derived from the status it applies where meaningful (Fire→Burning,
        /// Poison→Poisoned) and defaults to Melee for a pure-physical blast — this keeps the spell
        /// on the SAME 5×4 matrix as units (no forked chain, §4). Statue targets route to the
        /// StatueDamageInbox; units take it on Health (destroyed via ECB at ≤0). SYNERGY: a target
        /// already carrying SynergyBonusVsStatus takes ×SynergyMultiplier (e.g. Shatter vs Chilled).
        /// Single linear pass over units in range (§12) — no all-pairs scan.
        /// </summary>
        private void ResolveOffensive(ref SystemState state, ref EntityManager em,
                                      ref EntityCommandBuffer ecb, in SpellCatalogEntry def,
                                      in ActiveTelegraph tele, DynamicBuffer<CounterCell> matrix)
        {
            float r2 = def.Radius * def.Radius;
            Bulwark.Data.DamageType spellType = SpellDamageType(def);

            foreach (var (pos, team, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (team.ValueRO.Id == tele.CasterTeam) continue;       // enemies only
                if (def.Radius > 0f &&
                    math.distancesq(pos.ValueRO.Value, tele.Pos) > r2) continue;

                // §4 type×armor via the ONE shared lookup (CounterMatrix.Lookup).
                Bulwark.Data.ArmorClass armor = ResolveArmor(ref state, e);
                float typeArmor = CounterMatrix.Lookup(matrix, spellType, armor);

                float dmg = def.Magnitude * typeArmor;

                // §5.3 SYNERGY: extra damage vs a target already in the synergy status.
                dmg *= SynergyFactor(ref state, e, in def);
                if (dmg < 0f) dmg = 0f;

                ApplyDamage(ref state, ref em, ref ecb, e, dmg);

                // A spell that ALSO applies a status (e.g. a Fire blast leaving Burning) seeds it.
                if (def.AppliesStatus != StatusKind.None && def.Duration > 0f)
                    AddStatusTo(ref em, e, def.AppliesStatus, def.Duration, def.Magnitude, in def, ref state);
            }

            // The telegraph center may itself be the enemy statue (TargetEntity) — route directly.
            TryDamageStatueTarget(ref state, ref em, ref ecb, in def, in tele, matrix, spellType);
        }

        /// <summary>If the telegraph's TargetEntity is the enemy statue, append the spell hit to its inbox.</summary>
        private void TryDamageStatueTarget(ref SystemState state, ref EntityManager em,
                                           ref EntityCommandBuffer ecb, in SpellCatalogEntry def,
                                           in ActiveTelegraph tele, DynamicBuffer<CounterCell> matrix,
                                           Bulwark.Data.DamageType spellType)
        {
            Entity t = tele.TargetEntity;
            if (t == Entity.Null || !em.Exists(t)) return;
            if (!SystemAPI.HasComponent<StatueTag>(t)) return;
            if (SystemAPI.GetComponent<StatueTag>(t).Team == tele.CasterTeam) return; // enemy statue only

            float dmg = def.Magnitude * CounterMatrix.Lookup(matrix, spellType, Bulwark.Data.ArmorClass.Heavy);
            if (dmg <= 0f) return;
            AppendStatueDamage(ref em, ref ecb, t, dmg);
        }

        // ----------------------------------------------------------------- Control (status apply)
        /// <summary>
        /// Apply the spell's AppliesStatus (Chilled/Stunned/Poisoned/…) to ENEMY units in radius
        /// for Duration at Magnitude (§5.3 Control). Poisoned/Burning magnitudes are read by
        /// StatusEffectSystem as DoT-per-sec; Chilled/Stunned by StatusQuery. SYNERGY multiplies
        /// the applied magnitude vs an already-synergy-status target (data-driven). Single pass (§12).
        /// </summary>
        private void ResolveControl(ref SystemState state, ref EntityManager em,
                                    in SpellCatalogEntry def, in ActiveTelegraph tele)
        {
            if (def.AppliesStatus == StatusKind.None) return;
            float r2 = def.Radius * def.Radius;

            foreach (var (pos, team, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (team.ValueRO.Id == tele.CasterTeam) continue;   // control hits enemies
                if (def.Radius > 0f &&
                    math.distancesq(pos.ValueRO.Value, tele.Pos) > r2) continue;

                AddStatusTo(ref em, e, def.AppliesStatus, def.Duration, def.Magnitude, in def, ref state);
            }
        }

        // ----------------------------------------------------------------- Economy (local gold)
        /// <summary>
        /// Add Magnitude Gold to the caster team's GoldStore (LOCAL battle economy only — NO meta
        /// persistence, §J). If the spell instead grants a GoldBoost status (AppliesStatus ==
        /// GoldBoost), apply that to the side's miners so MiningSystem reads it (mirrors the
        /// Quartermaster passive path). One of the two — driven by DATA (§15.6).
        /// </summary>
        private void ResolveEconomy(ref SystemState state, in SpellCatalogEntry def, in ActiveTelegraph tele)
        {
            if (def.AppliesStatus == StatusKind.GoldBoost && def.Duration > 0f)
            {
                // Standing eco buff on the caster side's miners (read by MiningSystem — see
                // CommanderAbility's Quartermaster integration note; SAME GoldBoost status).
                var em = state.EntityManager;
                foreach (var (team, e) in
                         SystemAPI.Query<RefRO<Team>>().WithAll<UnitTag, MinerTag>().WithEntityAccess())
                {
                    if (team.ValueRO.Id != tele.CasterTeam) continue;
                    if (!em.HasBuffer<StatusEffect>(e)) continue;
                    var buf = em.GetBuffer<StatusEffect>(e);
                    Spell.AddOrRefreshStatus(ref buf, StatusKind.GoldBoost, def.Duration, def.Magnitude);
                }
                return;
            }

            // Direct one-shot gold injection into the side's local battle balance.
            int amount = (int)math.round(math.max(0f, def.Magnitude));
            if (amount <= 0) return;
            foreach (var store in SystemAPI.Query<RefRW<GoldStore>>())
            {
                if (store.ValueRO.Team != tele.CasterTeam) continue;
                store.ValueRW.Amount += amount;
                return;
            }
        }

        // ----------------------------------------------------------------- Summon (reuse spawn path)
        /// <summary>
        /// Spawn SummonUnitIndex unit(s) for the caster team reusing the SAME catalog-driven spawn
        /// the TrainingSystem uses (SpellSummon.SpawnFromCatalog → builds the identical unit
        /// archetype from UnitSpawnStats). Count is Magnitude (rounded, ≥1). No new unit invented —
        /// it must reference a baked UnitCatalog index (§15). Spawns at the caster's TeamSpawnPoint.
        /// </summary>
        private void ResolveSummon(ref SystemState state, ref EntityCommandBuffer ecb,
                                   in SpellCatalogEntry def, in ActiveTelegraph tele)
        {
            if (def.SummonUnitIndex < 0) return;
            // PHASE-2 TWO-FACTION: summon from the CASTER SIDE's per-team catalog (SpellDef
            // summonUnitIndex is side-relative — it indexes that faction's roster).
            if (!UnitCatalog.TryGetForTeam(state.EntityManager, tele.CasterTeam, out Entity catalogEntity)) return;
            if (!SystemAPI.HasBuffer<UnitSpawnStats>(catalogEntity)) return;
            DynamicBuffer<UnitSpawnStats> unitCatalog = SystemAPI.GetBuffer<UnitSpawnStats>(catalogEntity);

            if (def.SummonUnitIndex >= unitCatalog.Length) return;
            UnitSpawnStats stats = unitCatalog[def.SummonUnitIndex];

            int count = (int)math.max(1f, math.round(def.Magnitude <= 0f ? 1f : def.Magnitude));
            float2 spawnPos = ResolveSpawnPos(ref state, tele.CasterTeam);

            for (int i = 0; i < count; i++)
            {
                // Row-cycle across the 3 rows so summons spread like trained units (§13 P1.4),
                // deterministically from the summon index — no shared static (Burst/multi-world safe).
                int row = i % 3;
                SpellSummon.SpawnFromCatalog(ref ecb, in stats, tele.CasterTeam, row, spawnPos);
            }
        }

        // ----------------------------------------------------------------- Buff (ally area)
        /// <summary>
        /// Apply the spell's AppliesStatus (Raged/Hasted/…) to ALLIES in radius for Duration at
        /// Magnitude (§5.3 Buff, TargetShape.AllyArea). For a Self shape the caster's own units at
        /// the anchor are buffed (same pass; radius gate). SYNERGY may amplify vs a target already
        /// in the synergy status (data-driven). Single linear ally pass (§12).
        /// </summary>
        private void ResolveBuff(ref SystemState state, ref EntityManager em,
                                 in SpellCatalogEntry def, in ActiveTelegraph tele)
        {
            if (def.AppliesStatus == StatusKind.None) return;
            float r2 = def.Radius * def.Radius;
            bool gateByRadius = def.Radius > 0f && def.TargetShape != TargetShape.Self;

            foreach (var (pos, team, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (team.ValueRO.Id != tele.CasterTeam) continue;  // buffs hit allies
                if (gateByRadius && math.distancesq(pos.ValueRO.Value, tele.Pos) > r2) continue;

                AddStatusTo(ref em, e, def.AppliesStatus, def.Duration, def.Magnitude, in def, ref state);
            }
        }

        // ----------------------------------------------------------------- shared helpers

        /// <summary>
        /// Add/refresh a status on a target, applying §5.3 SYNERGY: if the target already carries
        /// the spell's SynergyBonusVsStatus, the magnitude is scaled by SynergyMultiplier. Skips
        /// targets with no StatusEffect buffer (buffer presence is an integration guarantee — see
        /// the REQUIRED INTEGRATION EDIT block; we do not lazily structural-add here, matching
        /// CommanderAbility.cs's single-owner discipline).
        /// </summary>
        private void AddStatusTo(ref EntityManager em, Entity target, StatusKind kind,
                                 float duration, float magnitude, in SpellCatalogEntry def,
                                 ref SystemState state)
        {
            if (kind == StatusKind.None || duration <= 0f) return;
            if (!em.HasBuffer<StatusEffect>(target)) return;
            var buf = em.GetBuffer<StatusEffect>(target);

            float mag = magnitude * SynergyFactor(ref state, target, in def);
            Spell.AddOrRefreshStatus(ref buf, kind, duration, mag);
        }

        /// <summary>
        /// §5.3 synergy factor for a target: SynergyMultiplier if the target currently carries the
        /// spell's SynergyBonusVsStatus (e.g. Shatter ×N vs an already-Chilled target), else 1.0.
        /// Reads the target's StatusEffect buffer; neutral if it has none.
        /// </summary>
        private float SynergyFactor(ref SystemState state, Entity target, in SpellCatalogEntry def)
        {
            if (def.SynergyBonusVsStatus == StatusKind.None || def.SynergyMultiplier == 1f)
                return 1f;
            var em = state.EntityManager;
            if (!em.HasBuffer<StatusEffect>(target)) return 1f;
            var buf = em.GetBuffer<StatusEffect>(target);
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i].Kind == def.SynergyBonusVsStatus && buf[i].Remaining > 0f)
                    return def.SynergyMultiplier;
            }
            return 1f;
        }

        /// <summary>
        /// Map a spell to its §4 damage type so it shares the unit 5×4 matrix. A spell that leaves
        /// Burning is Fire; Poisoned is Poison; otherwise a physical blast defaults to Melee. NO
        /// new type invented — these are the canon DamageType members (§4/§5.2).
        /// </summary>
        private static Bulwark.Data.DamageType SpellDamageType(in SpellCatalogEntry def)
        {
            switch (def.AppliesStatus)
            {
                case StatusKind.Burning: return Bulwark.Data.DamageType.Fire;
                case StatusKind.Poisoned: return Bulwark.Data.DamageType.Poison;
                default: return Bulwark.Data.DamageType.Melee;
            }
        }

        /// <summary>Armor class of an entity if it has a CombatProfile; Unset (⇒ neutral 1.0) otherwise.</summary>
        private static Bulwark.Data.ArmorClass ResolveArmor(ref SystemState state, Entity e)
        {
            if (SystemAPI.HasComponent<CombatProfile>(e))
                return SystemAPI.GetComponent<CombatProfile>(e).ArmorClass;
            return Bulwark.Data.ArmorClass.Unset;
        }

        /// <summary>
        /// Apply spell damage to a target the SAME way CombatSystem does: a StatueTag target routes
        /// to its StatueDamageInbox (StatueDamageSystem owns shield/phase math, §11); a unit takes
        /// it on Health and is destroyed via ECB at ≤0. Keeps statue/unit damage in ONE place.
        /// </summary>
        private void ApplyDamage(ref SystemState state, ref EntityManager em,
                                 ref EntityCommandBuffer ecb, Entity target, float dmg)
        {
            if (dmg <= 0f) return;
            if (SystemAPI.HasComponent<StatueTag>(target))
            {
                AppendStatueDamage(ref em, ref ecb, target, dmg);
                return;
            }
            if (SystemAPI.HasComponent<Health>(target))
            {
                var hp = SystemAPI.GetComponent<Health>(target);
                hp.Current -= dmg;
                ecb.SetComponent(target, hp);
                if (hp.Current <= 0f)
                    ecb.DestroyEntity(target);
            }
        }

        /// <summary>Append a damage element to a statue's inbox (creating it via ECB if not yet present).</summary>
        private static void AppendStatueDamage(ref EntityManager em, ref EntityCommandBuffer ecb,
                                               Entity statue, float dmg)
        {
            if (em.HasBuffer<StatueDamageInbox>(statue))
            {
                var inbox = em.GetBuffer<StatueDamageInbox>(statue);
                inbox.Add(new StatueDamageInbox { Amount = dmg });
            }
            else
            {
                var added = ecb.AddBuffer<StatueDamageInbox>(statue);
                added.Add(new StatueDamageInbox { Amount = dmg });
            }
        }

        /// <summary>Caster side's spawn anchor (§11 single-front home) for summons; origin if none baked.</summary>
        private float2 ResolveSpawnPos(ref SystemState state, int team)
        {
            foreach (var sp in SystemAPI.Query<RefRO<TeamSpawnPoint>>())
            {
                if (sp.ValueRO.Team == team)
                    return sp.ValueRO.Pos;
            }
            return float2.zero;
        }
    }

    // =====================================================================================
    // Summon spawn helper — reuses Training's catalog-driven archetype (DRY, §15)
    // =====================================================================================
    /// <summary>
    /// Builds a combat unit from a UnitSpawnStats catalog entry — the SAME archetype/fields
    /// TrainingSystem.SpawnUnit produces (Hard-Rule 4: all stats from DATA; only NEUTRAL factor
    /// slots / Level 0 / stickiness 1.2 are literals). Kept as a small public static so both the
    /// (private) TrainingSystem and the spell Summon path build IDENTICAL units. The integration
    /// pass MAY refactor TrainingSystem.SpawnUnit to call this to remove the remaining duplication
    /// (see the REQUIRED INTEGRATION EDIT block); for now Summon spawns through here independently
    /// so Training.cs is not edited by this pass.
    /// </summary>
    public static class SpellSummon
    {
        public static void SpawnFromCatalog(ref EntityCommandBuffer ecb, in UnitSpawnStats stats,
                                            int team, int row, float2 pos)
        {
            Entity e = ecb.CreateEntity();

            ecb.AddComponent<UnitTag>(e);
            ecb.AddComponent(e, new Team { Id = team });
            ecb.AddComponent(e, new Position { Value = pos });
            ecb.AddComponent(e, new Row { Index = row });
            ecb.AddComponent(e, new Health { Current = stats.MaxHealth, Max = stats.MaxHealth });
            ecb.AddComponent(e, new Movement { Speed = stats.MoveSpeed });

            ecb.AddComponent(e, new AttackState
            {
                Range = stats.AttackRange,
                Damage = stats.AttackDamage,
                Interval = stats.AttackInterval,
                Cooldown = 0f,
            });

            ecb.AddComponent(e, new CombatProfile
            {
                DamageType = (Bulwark.Data.DamageType)(int)stats.DamageType,
                ArmorClass = (Bulwark.Data.ArmorClass)(int)stats.ArmorClass,
                Level = 0,
                PerLevel = 0f,
            });

            // Neutral factor slots (1.0): positional/terrain geometry is populated by the Phase-2
            // PositionalSystem/TerrainSystem on the unit's first tick (slots, not literals — §13 P1.4).
            ecb.AddComponent(e, new Positional { Multiplier = 1f });
            ecb.AddComponent(e, new TerrainFactor { Multiplier = 1f });

            // Difficulty slot defaults neutral; the global difficulty pass owns it (not per-unit).
            ecb.AddComponent(e, new Difficulty { Multiplier = 1f });

            ecb.AddComponent(e, new Targeting
            {
                Current = Entity.Null,
                ReevalCooldown = 0f,
                Stickiness = 1.2f, // §13 P1.4 same-target stickiness
            });

            // Summoned combat units carry an empty StatusEffect buffer so spell/commander buffs can
            // be applied without a lazy structural add on the hot path (matches the integration
            // guarantee that every unit has this buffer — see the REQUIRED INTEGRATION EDIT block).
            ecb.AddBuffer<StatusEffect>(e);

            if (stats.Role == RoleId.Miner)
            {
                ecb.AddComponent<MinerTag>(e);
                ecb.AddComponent(e, new MiningState
                {
                    Mine = Entity.Null,
                    Accum = 0f,
                    RatePerSec = stats.MiningRatePerSec,
                });
            }
        }
    }

    // =====================================================================================
    // 7. StatusEffectSystem — SINGLE decay/expiry owner + Burning/Poisoned DoT (§5.3)
    // =====================================================================================
    /// <summary>
    /// The ONE owner of per-unit StatusEffect lifetime (shared with CommanderAbility.cs, which
    /// only WRITES via the identical AddOrRefreshStatus policy). Each tick, for every unit's
    /// StatusEffect buffer:
    ///   • decrement Remaining by dt;
    ///   • apply Burning/Poisoned DoT to Health at Magnitude (per-sec × dt) — statue targets route
    ///     to StatueDamageInbox (StatueDamageSystem owns shield/phase math, §11);
    ///   • remove expired entries (Remaining ≤ 0) via swap-back so the buffer stays compact.
    /// It does NOT re-implement Chilled/Hasted/Stunned/Raged effects — those are READ by
    /// Movement/Targeting/Combat through StatusQuery (the single source of truth). Runs LATE in
    /// the tick (after Combat) so it observes a fully-resolved tick before decaying buffs — this
    /// is exactly what CommanderAbility.cs relies on to expire its refreshed passive (no leak).
    /// Not [BurstCompile]: it may DestroyEntity / structural-edit via ECB on a lethal DoT and
    /// touches statue inbox buffers, like CombatSystem.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(CombatSystem))] // decay after the tick's combat has read the statuses.
    public partial struct StatusEffectSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (buf, e) in
                     SystemAPI.Query<DynamicBuffer<StatusEffect>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (buf.Length == 0) continue;

                float dotThisTick = 0f; // accumulated Burning/Poisoned damage this tick (per-sec × dt)

                // Walk back-to-front so RemoveAtSwapBack does not skip elements.
                for (int i = buf.Length - 1; i >= 0; i--)
                {
                    StatusEffect se = buf[i];

                    // DoT statuses bleed Health at Magnitude/sec while live (§5.3 Burning/Poisoned).
                    if ((se.Kind == StatusKind.Burning || se.Kind == StatusKind.Poisoned) &&
                        se.Remaining > 0f && se.Magnitude > 0f)
                    {
                        dotThisTick += se.Magnitude * dt;
                    }

                    se.Remaining -= dt;
                    if (se.Remaining <= 0f)
                        buf.RemoveAtSwapBack(i); // expired — compact out.
                    else
                        buf[i] = se;
                }

                if (dotThisTick > 0f)
                    ApplyDot(ref state, ref em, ref ecb, e, dotThisTick);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Apply accumulated DoT to a target: a StatueTag routes to its StatueDamageInbox (statue
        /// math owned by StatueDamageSystem); a unit takes it on Health and is destroyed at ≤0.
        /// (Units don't normally carry StatueTag, but the routing keeps DoT consistent with the
        /// §4 damage application path used everywhere else.)
        /// </summary>
        private void ApplyDot(ref SystemState state, ref EntityManager em,
                              ref EntityCommandBuffer ecb, Entity target, float dmg)
        {
            if (SystemAPI.HasComponent<StatueTag>(target))
            {
                if (em.HasBuffer<StatueDamageInbox>(target))
                    em.GetBuffer<StatueDamageInbox>(target).Add(new StatueDamageInbox { Amount = dmg });
                else
                    ecb.AddBuffer<StatueDamageInbox>(target).Add(new StatueDamageInbox { Amount = dmg });
                return;
            }
            if (SystemAPI.HasComponent<Health>(target))
            {
                var hp = SystemAPI.GetComponent<Health>(target);
                hp.Current -= dmg;
                ecb.SetComponent(target, hp);
                if (hp.Current <= 0f)
                    ecb.DestroyEntity(target);
            }
        }
    }
}

// =========================================================================================
// REQUIRED INTEGRATION EDIT NOTES (for the integration pass — NOT applied here, §ADR-1-001)
// -----------------------------------------------------------------------------------------
// This pass owns ONLY Spell.cs + DraftAndSpellInput.cs and must not edit existing Phase-1/2
// system files. The following precise edits make Movement/Targeting/Combat honor statuses and
// wire the catalog/buffer bakes. Each is the exact file + location + code to apply.
//
// ── EDIT 1: MovementSystem honors Chilled (slow) / Hasted (speed) / Stunned (halt) ──
//   FILE:     Assets/_Game/Sim/Systems/Combat.cs  (MovementSystem.OnUpdate)
//   LOCATION: inside the auto-target movement branch, where it currently computes the step:
//                 float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;
//                 StepToward(ref pos.ValueRW.Value, targetPos, move.ValueRO.Speed, dt,
//                            stopDist: atk.ValueRO.Range);
//   CHANGE:   scale the move speed by the status multiplier, and halt while Stunned. Add
//             RefRO<StatusEffect> is not possible (it's a buffer) — fetch the buffer by entity:
//                 float spd = move.ValueRO.Speed;
//                 if (SystemAPI.HasBuffer<StatusEffect>(e))
//                 {
//                     var sbuf = SystemAPI.GetBuffer<StatusEffect>(e);
//                     if (Bulwark.Sim.StatusQuery.IsStunned(in sbuf)) continue; // stunned: no move this tick
//                     spd *= Bulwark.Sim.StatusQuery.MoveSpeedMultiplier(in sbuf, k_CommanderBudgetCeiling);
//                 }
//                 StepToward(ref pos.ValueRW.Value, targetPos, spd, dt, stopDist: atk.ValueRO.Range);
//   §6 ADR-2-002: the budget-taking overload returns spellMult × CLAMPED commander-Hasted (≤ the
//   §6 ceiling 0.15), so the commander-attributable speed buff is bounded; spell Chilled/Hasted
//   stay their own uncapped §5.3 layer. (Applied in Combat.cs; this note reflects the wired form.)
//   ALSO:     apply the SAME spd scaling + stun-halt to the manual MoveDestination branch above
//             (so a stunned/chilled possessed unit is affected identically). NOTE: MovementSystem
//             is [BurstCompile] and StatusQuery is [BurstCompile]+pure, so the calls are Burst-safe.
//
// ── EDIT 2: TargetingSystem skips re-acquire while Stunned ──
//   FILE:     Assets/_Game/Sim/Systems/Targeting.cs  (TargetingSystem.OnUpdate, per-unit loop)
//   LOCATION: at the TOP of the per-unit body, before the re-eval/acquire logic.
//   CHANGE:   if (SystemAPI.HasBuffer<StatusEffect>(e))
//             {
//                 var sbuf = SystemAPI.GetBuffer<StatusEffect>(e);
//                 if (Bulwark.Sim.StatusQuery.IsStunned(in sbuf)) continue; // cannot act: keep/skip target
//             }
//   (A stunned unit holds its current target but does not spend cost re-acquiring; combat also
//    skips its swing per EDIT 3, so the stun fully gates the unit's actions.)
//
// ── EDIT 3: CombatSystem applies Raged (outgoing ×) and skips the swing while Stunned ──
//   FILE:     Assets/_Game/Sim/Systems/Combat.cs  (CombatSystem.OnUpdate, per-unit loop)
//   LOCATION (stun gate): right after the cooldown tick, before reading the target:
//                 atk.ValueRW.Cooldown = math.max(0f, atk.ValueRO.Cooldown - dt);
//                 if (SystemAPI.HasBuffer<StatusEffect>(e))
//                 {
//                     var sbufA = SystemAPI.GetBuffer<StatusEffect>(e);
//                     if (Bulwark.Sim.StatusQuery.IsStunned(in sbufA)) continue; // stunned: no swing
//                 }
//   LOCATION (raged factor): in the §13 P1.4 chain where 'dmg' is composed, multiply IN the
//             outgoing-status factor (it joins the SAME chain — no fork, §4):
//                 float ragedMult = 1f;
//                 if (SystemAPI.HasBuffer<StatusEffect>(e))
//                 {
//                     var sbufB = SystemAPI.GetBuffer<StatusEffect>(e);
//                     ragedMult = Bulwark.Sim.StatusQuery.OutgoingDamageMultiplier(in sbufB, k_CommanderBudgetCeiling);
//                 }
//                 float dmg = rounded * typeArmor
//                             * positional.ValueRO.Multiplier
//                             * terrain.ValueRO.Multiplier
//                             * difficulty.ValueRO.Multiplier
//                             * ragedMult;
//   §6 ADR-2-002: the budget-taking overload returns spellMult × CLAMPED commander-Raged (≤ the
//   §6 ceiling 0.15), so the commander-attributable damage buff is bounded; spell Raged stays its
//   own uncapped §5.3 layer. (Applied in Combat.cs; this note reflects the wired form.)
//   (CombatSystem is [BurstCompile]; StatusQuery is Burst-safe so this is legal in the job.)
//
// ── EDIT 4: every unit carries a StatusEffect buffer (buffer-presence guarantee) ──
//   FILE:     Assets/_Game/Sim/Systems/Terrain.cs  (EnsurePhase2CombatComponentsSystem)
//   CHANGE:   in the lazy "ensure Phase-2 components" pass that already attaches Facing /
//             TerrainOccupancy to units lacking them, ALSO attach an empty StatusEffect buffer:
//                 if (!SystemAPI.HasBuffer<StatusEffect>(e)) ecb.AddBuffer<StatusEffect>(e);
//             This makes the "skip units without a StatusEffect buffer" guards in Spell.cs and
//             CommanderAbility.cs unnecessary in steady state (they remain as safe fallbacks).
//             (TrainingSystem.SpawnUnit may ALSO add the empty buffer at spawn for immediacy;
//             SpellSummon.SpawnFromCatalog above already does so for summoned units.)
//
// ── EDIT 5: BattleBootstrap bakes the SpellCatalog (index == SpellSlot.SpellIndex) ──
//   FILE:     Assets/_Game/Bootstrap/BattleBootstrap.cs  (battle setup, alongside BuildUnitCatalog)
//   CHANGE:   add a BuildSpellCatalog pass mirroring BuildUnitCatalog: create one singleton entity,
//             AddComponent<SpellCatalogTag>, AddBuffer<SpellCatalogEntry>, and for each SpellDef in
//             the authored pool (in pool order) append:
//                 buf.Add(new SpellCatalogEntry {
//                     Category = def.category, Cooldown = def.cooldown, Charges = def.charges,
//                     TelegraphTime = def.telegraphTime, TargetShape = def.targetShape,
//                     Radius = def.radius, Magnitude = def.magnitude, Duration = def.duration,
//                     AppliesStatus = def.appliesStatus, SummonUnitIndex = def.summonUnitIndex,
//                     SynergyBonusVsStatus = def.synergyBonusVsStatus,
//                     SynergyMultiplier = def.synergyMultiplier });
//             ALSO create the SpellCastInboxTag singleton with an empty DynamicBuffer<SpellCastRequest>
//             (SpellCastSystem/SquadAI/DraftAndSpellInput all assume this inbox exists), and create
//             one SpellLoadoutTag{Team} singleton per side, each with an empty DynamicBuffer<SpellSlot>,
//             plus the DraftState singleton {PlayerDone=0, AiDone=0}. The AI side's SpellSlot buffer
//             is filled by an AI draft pass (or the bootstrap) and DraftState.AiDone set; the player
//             side's is filled by DraftAndSpellInput (it sets DraftState.PlayerDone). The §13-2.4
//             draft GATE (sim waits for both PlayerDone && AiDone) is enforced wherever the match-
//             start gate lives (MatchFlow/Bootstrap) — out of scope for this pass.
//
// ── EDIT 6 (optional DRY): TrainingSystem.SpawnUnit may delegate to SpellSummon.SpawnFromCatalog ──
//   FILE:     Assets/_Game/Sim/Systems/Training.cs  (TrainingSystem.SpawnUnit)
//   CHANGE:   replace the body of SpawnUnit with a call to SpellSummon.SpawnFromCatalog(ref ecb,
//             in stats, team, row, pos) so trained and summoned units are built by ONE code path.
//             (Note: SpawnFromCatalog also adds an empty StatusEffect buffer + a neutral Difficulty
//             slot, which is a superset of the current SpawnUnit — harmless and desirable.)
// =========================================================================================
