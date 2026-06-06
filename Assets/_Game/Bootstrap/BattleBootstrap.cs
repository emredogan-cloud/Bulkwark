// BULWARK — Phase-2 BATTLE BOOTSTRAP (control/setup shell). Roadmap §13 Phase 1+2
// (one playable vertical-slice battle: mine→train→push→statue, TWO factions / drafted spells /
// commanders / terrain, vs AI), §11 (statues at ends, contestable miner-capped mines, single
// 3-row front + 2–4 terrain features), §12 (MonoBehaviour CONTROL/SETUP layer ONLY — it WRITES
// entity data the ECS sim then reads; it contains NO gameplay simulation), §3 (preserved core
// loop), §15 (no invented content/balance: all numeric stats come from authored DATA —
// UnitDef[]/MapDef/BalanceConfig/SpellDef[]/CommanderDef).
//
// WHAT THIS DOES (Phase-2 vertical slice)
// Builds ONE battle in code at Start() because no .unity scene is authorable in this environment.
// It constructs the initial entity state for the default ECS World:
//   • per-team GoldStore + control tag (Team 0 PlayerControlled / Team 1 AIControlled),
//   • two statues (StatueTag + StatueState + StatueDamageInbox buffer) at the MAP's statue X's,
//   • the MAP's terrain features (TerrainFeature, multipliers from a PROVISIONAL per-kind table),
//   • the MAP's miner-capped MineNodes,
//   • a TeamSpawnPoint per side,
//   • TWO per-team UnitCatalogTag entities (Iron Pact = playerUnits[], Ashen Horde = aiUnits[]),
//   • the CounterMatrixTag singleton holding 20 CounterCell from counterMatrix.counterMatrix,
//   • a TrainQueueTag entity per side holding an (empty) DynamicBuffer<TrainOrder>,
//   • the SpellCatalogTag singleton (baked from spellPool[]), the SpellCastInboxTag inbox,
//     one SpellLoadoutTag{Team} per side (empty SpellSlot buffer), the DraftState singleton,
//   • one CommanderRuntime per side (from playerCommander / aiCommander),
//   • the AI tactical layer singletons: DifficultyAxes, AISchedulerState, plus a SquadAIState +
//     SquadFormation per AI squad,
//   • an AICommander for Team 1 (§13 1.5), and MatchState{ Ongoing }.
// It ALSO retires the Phase-0 spike systems (documented no-op — the types are deleted).
//
// CANON DISCIPLINE
//   • NO balance numbers invented here. Stat values are COPIED from the authored DATA assets.
//     The ONLY literals are NEUTRAL Phase-1/2 factor defaults (axes = 1.0) and readable LAYOUT
//     positions (field geometry). Terrain per-kind multipliers are a PROVISIONAL, clearly-flagged
//     LSD-owned default table (§15.6) used only where a TerrainPlacement does not carry its own —
//     they are tunable data placeholders, never asserted canon.
//   • §6 commander power: CommanderRuntime.PowerBudgetPct is copied from CommanderDef (the schema
//     already [Range(0,0.15)]-clamps it); CommanderAbilitySystem additionally HARD-CLAMPS every
//     magnitude to 0.15 at runtime. Bootstrap does not raise the cap.
//   • Phase-3+/7 FORBIDDEN: no meta/economy persistence, no upgrades, no commander COLLECTION
//     (exactly 1 active + 1 passive per side), no determinism/replays, no 3rd faction, no biomes.
//   • No save-state logging (§4/§12 inviolable): this never logs/serializes match or save state.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002/-1-001).
// DEFERRED (Unity-editor step): authoring a real Scene/SubScene and ASSIGNING the inspector
// references (map / playerUnits / aiUnits / counterMatrix / spellPool / commanders / camera), plus
// the GATE-2 verdict / on-device feel / perf / end-to-end playthrough — none can run without Unity.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Data;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>
    /// Place on a single GameObject in a near-empty bootstrap scene. Assign the authored MapDef,
    /// the two faction UnitDef sets, the CounterMatrix BalanceConfig, the spell pool, the two
    /// CommanderDefs, and the battle Camera in the inspector. At Start() it builds the whole
    /// Phase-2 vertical-slice battle into the default ECS World.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleBootstrap : MonoBehaviour
    {
        // ----- Required data (DATA-driven; see §12 content-tools rule) -----
        [Header("Map (§13 2.7 / §11 — authored MapDef)")]
        [Tooltip("Battlefield: front length, statue X's, terrain features, mine nodes. PROVISIONAL data.")]
        public MapDef map;

        [Header("Factions (§5.1 — TWO rosters; index is side-relative UnitIndex)")]
        [Tooltip("Player faction units (Iron Pact). Index here is the Team-0 UnitIndex used by TrainOrder/summon.")]
        public UnitDef[] playerUnits;

        [Tooltip("AI faction units (Ashen Horde). Index here is the Team-1 UnitIndex used by TrainOrder/summon.")]
        public UnitDef[] aiUnits;

        [Header("Balance (§4 — counter matrix asset)")]
        [Tooltip("BalanceConfig holding the FULL 5×4 = 20-cell type×armor counter matrix (CounterMatrix.asset).")]
        public BalanceConfig counterMatrix;

        [Header("Spells (§13 2.4 / §5.3 — shared draft pool)")]
        [Tooltip("Shared spell pool baked into the SpellCatalog (index == SpellSlot.SpellIndex). Players draft 3.")]
        public SpellDef[] spellPool;

        [Header("Commanders (§13 2.5 / §5.5/§6 — exactly 1 active + 1 passive per side)")]
        public CommanderDef playerCommander;
        public CommanderDef aiCommander;

        [Tooltip("Battle camera (control/render layer). Positioned to frame the front; the sim never reads it.")]
        public Camera battleCamera;

        // ----- Readable battlefield layout (§11) -----
        // Vertical row spacing is geometry (not balance); the horizontal front + statue X's come
        // from the MapDef. Surfaced for designer readability.
        [Header("Battlefield layout (§11 — geometry, not balance)")]
        [Tooltip("Vertical spacing between the 3 rows of the single front (§11).")]
        public float rowSpacing = 2.5f;

        [Header("Statue objective (§11 — HP/shield are tunable data, not canon)")]
        [Tooltip("Statue max health. PROVISIONAL/LSD-owned; tune via data, never assert as canon.")]
        public float statueMaxHealth = 500f; // GATE1-balance (Option E): 1000→500 (build-2 retune from 700) — breakthroughs resolve in a mobile window (symmetric, both statues).

        [Tooltip("Statue shield pool absorbed before health (§11 shield phase). 0 = no shield.")]
        public float statueShieldHealth = 0f; // GATE1-balance (Option E): 250→0 — the shield was a pure delay buffer (symmetric, both statues).

        [Tooltip("Min interval/threshold before recovered trickle damage applies (§11 statue math).")]
        public float statueTrickleThrottle = 0.5f;

        [Header("Economy start (§13 1.1 — LOCAL battle gold, no persistence)")]
        [Tooltip("Starting Gold for each side (enough to train the first miner).")]
        public int startingGold = 100;

        [Header("AI (§13 1.5 commander stance + 2.6 squad layer / scheduler)")]
        public CommanderStance aiStartingStance = CommanderStance.Push;

        [Tooltip("AI commander re-eval throttle (seconds). Keeps the utility pass O(1)/tick, §12.")]
        public float aiReevalCooldown = 1.0f;

        [Tooltip("Number of AI tactical squads (§13 2.6 SquadAIState/SquadFormation) to create on the AI side.")]
        [Min(1)] public int aiSquadCount = 1;

        [Tooltip("Budgeted AI scheduler: max SquadAIState agents permitted to re-think per frame (§4/§12).")]
        [Min(1)] public int aiSchedulerBudgetPerFrame = 8;

        [Header("Targeting (§13 1.4)")]
        [Tooltip("Same-target stickiness bias applied to the current target's score (×1.2, §13 1.4).")]
        public float targetingStickiness = 1.2f;

        // ---- RESOLVED-UPGRADE BAKE HOOK (Phase-3 integration C2 / §13 3.2) ----
        // The meta layer's IBattleLauncher (Bootstrap) sets this BEFORE the world is built (it must be
        // assigned prior to Start(), e.g. by the launcher on a disabled bootstrap that it then enables).
        // It maps a PLAYER unit's content id (UnitDef.id) → the ADDITIVE per-stat deltas the SERVER-OWNED,
        // cap-clamped upgrade levels produce (computed by the launcher via UpgradeMath.ResolveUpgradedStat,
        // which clamps to maxLevel — so a delta here can NEVER exceed the HARD CAP, §3/§15 no-P2W). When a
        // player unit's catalog stats are baked, the matching deltas are ADDED on top of the authored base
        // (additive, mirroring the §4 base→modifier chain — never multiplicative). Null/absent ⇒ base stats
        // (no upgrades). Only the PLAYER roster is upgraded; the AI/enemy roster is never altered here.
        // This is the explicit hook the launcher writes; the launcher owns the Services-side math so this
        // file keeps its Data/Sim-only dependency surface (no Bulwark.Services reference needed).
        [System.NonSerialized] public IReadOnlyDictionary<string, UnitUpgradeDeltas> playerUpgradeDeltas;

        /// <summary>
        /// Additive, already-cap-clamped stat deltas for one player unit (content id), produced from the
        /// server-owned upgrade levels by the meta launcher (C2). AttackSpeed is modeled as a REDUCTION of
        /// AttackInterval (faster attacks) — see the bake. All fields default 0 (no upgrade).
        /// </summary>
        public struct UnitUpgradeDeltas
        {
            public float Health;          // + MaxHealth
            public float Damage;          // + AttackDamage
            public float MoveSpeed;       // + MoveSpeed
            public float AttackIntervalDelta; // applied to AttackInterval (negative = faster; clamped > 0 at bake)
        }

        private const int PlayerTeam = 0; // human (PlayerControlled)
        private const int AiTeam = 1;     // AI (AIControlled)

        // ---- PROVISIONAL terrain per-kind default multipliers (LSD-owned, §15.6 "provisional") ----
        // Used to populate TerrainFeature.AttackMult/DefenseMult/MoveMult/HazardDps for each
        // TerrainPlacement on the map. These are PLACEHOLDER tuning data — flagged provisional,
        // NOT asserted canon (the §11 terrain features exist in canon; their NUMBERS are LSD-owned).
        // A later content pass moves these into the MapDef/TerrainPlacement schema as authored fields.
        //   HighGround → attacker outgoing-damage bonus (>1); Cover → incoming-damage reduction (<1);
        //   Choke → move-speed reduction (<1); Hazard → damage/sec to occupants (>0).
        private struct TerrainKindDefaults { public float Attack, Defense, Move, Hazard; }

        private static TerrainKindDefaults DefaultsFor(TerrainKind kind)
        {
            // Neutral baseline = all 1.0 / no hazard; each kind perturbs ONE axis (its identity).
            switch (kind)
            {
                case TerrainKind.HighGround: return new TerrainKindDefaults { Attack = 1.25f, Defense = 1f, Move = 1f,    Hazard = 0f };
                case TerrainKind.Cover:      return new TerrainKindDefaults { Attack = 1f,    Defense = 0.75f, Move = 1f, Hazard = 0f };
                case TerrainKind.Choke:      return new TerrainKindDefaults { Attack = 1f,    Defense = 1f, Move = 0.6f,  Hazard = 0f };
                case TerrainKind.Hazard:     return new TerrainKindDefaults { Attack = 1f,    Defense = 1f, Move = 1f,    Hazard = 5f };
                default:                     return new TerrainKindDefaults { Attack = 1f,    Defense = 1f, Move = 1f,    Hazard = 0f };
            }
        }

        // ---- TEMPORARY forensic lifecycle logs (BOOTSTRAP_EXECUTION_REPORT). Removable; no gameplay change. ----
        private void Awake()    { Debug.Log("[BOOTSTRAP] Awake — BattleBootstrap is a LIVE MonoBehaviour (not baked)."); }
        private void OnEnable() { Debug.Log("[BOOTSTRAP] OnEnable."); }

        private void Start()
        {
            Debug.Log("[BOOTSTRAP] Start — beginning world build.");
            if (!ValidateConfig()) { Debug.LogError("[BOOTSTRAP] ValidateConfig FAILED — world NOT built."); return; }

            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                Debug.LogError("[BOOTSTRAP] No default ECS World — cannot build the battle.");
                return;
            }

            EntityManager em = world.EntityManager;

            try
            {
                // Retire the superseded Phase-0 spike BEFORE building state (documented no-op now).
                DisablePhase0SpikeSystems(world);

                BuildMatchSingletons(em);         Debug.Log("[BOOTSTRAP] Created MatchState singleton.");
                BuildUnitCatalogs(em);            Debug.Log("[BOOTSTRAP] Created UnitCatalogs (player + AI).");
                BuildCounterMatrix(em);           Debug.Log("[BOOTSTRAP] Created CounterMatrix.");
                BuildSpellSetup(em);              Debug.Log("[BOOTSTRAP] Created Spell setup.");
                BuildDifficultyAndScheduler(em);  Debug.Log("[BOOTSTRAP] Created DifficultyAxes + AIScheduler.");

                // Statues sit at the MapDef's statue X's (player = team0StatueX, AI = team1StatueX).
                float midRowY = rowSpacing; // centre row of the 3-row front
                float2 playerStatue = new float2(map.team0StatueX, midRowY);
                float2 aiStatue = new float2(map.team1StatueX, midRowY);
                // Spawn anchors sit just inboard of each statue, on the single front (§11).
                float dir = math.sign(map.team1StatueX - map.team0StatueX);
                if (dir == 0f) dir = 1f;
                float2 playerSpawn = new float2(map.team0StatueX + dir * 4f, midRowY);
                float2 aiSpawn = new float2(map.team1StatueX - dir * 4f, midRowY);

                BuildSide(em, PlayerTeam, spawnPos: playerSpawn, statuePos: playerStatue,
                          commander: playerCommander, isPlayer: true);
                Debug.Log("[BOOTSTRAP] Created GoldStore + Statue + TrainQueue (Player).");
                BuildSide(em, AiTeam, spawnPos: aiSpawn, statuePos: aiStatue,
                          commander: aiCommander, isPlayer: false);
                Debug.Log("[BOOTSTRAP] Created GoldStore + Statue + TrainQueue + AICommander (AI).");

                BuildAISquads(em, aiSpawn);       Debug.Log("[BOOTSTRAP] Created AI squads.");
                BuildTerrain(em, midRowY);        Debug.Log("[BOOTSTRAP] Created Terrain.");
                BuildMines(em, midRowY);          Debug.Log("[BOOTSTRAP] Created Mines.");

                FrameCamera();
                Debug.Log("[BOOTSTRAP] World build COMPLETE (units spawn at runtime via TrainingSystem).");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[BOOTSTRAP] EXCEPTION during world build: " + e);
            }
        }

        // ---------------------------------------------------------------- validation

        private bool ValidateConfig()
        {
            if (map == null)
            {
                Debug.LogError("[BattleBootstrap] 'map' is unassigned — assign a MapDef (§13 2.7).");
                return false;
            }
            if (!ValidateRoster(playerUnits, "playerUnits") || !ValidateRoster(aiUnits, "aiUnits"))
                return false;
            if (counterMatrix == null)
            {
                Debug.LogError("[BattleBootstrap] 'counterMatrix' is unassigned — assign the BalanceConfig (CounterMatrix.asset).");
                return false;
            }
            // spellPool / commanders may be null/empty (the slice still runs without spells/commanders);
            // we DEFER those features rather than hard-fail, so a partial scene is still playable.
            return true;
        }

        private static bool ValidateRoster(UnitDef[] roster, string name)
        {
            if (roster == null || roster.Length == 0)
            {
                Debug.LogError($"[BattleBootstrap] '{name}' is empty — assign the faction's UnitDefs (§13 1.2).");
                return false;
            }
            for (int i = 0; i < roster.Length; i++)
            {
                if (roster[i] == null)
                {
                    Debug.LogError($"[BattleBootstrap] {name}[{i}] is null — assign a UnitDef.");
                    return false;
                }
            }
            return true;
        }

        // ---------------------------------------------------------------- phase-0 retirement

        /// <summary>
        /// The Phase-0 spike system TYPES were deleted (SimSystems.cs is empty), so there is nothing
        /// to disable by type here — documented NO-OP. MatchFlowSystem still carries them as string
        /// freeze targets (which match nothing now) defensively. (See SimSystemOrder.RetiredPhase0Systems.)
        /// </summary>
        private void DisablePhase0SpikeSystems(World world)
        {
            // No Phase-0 spike systems exist anymore (types deleted). Intentional no-op.
        }

        // ---------------------------------------------------------------- singletons

        /// <summary>MatchState{Ongoing}. (A5: the old global Difficulty singleton is REMOVED — see below.)</summary>
        private void BuildMatchSingletons(EntityManager em)
        {
            Entity match = em.CreateEntity();
            em.SetName(match, "MatchState");
            em.AddComponentData(match, new MatchState { Outcome = MatchOutcome.Ongoing });

            // A5 (integration): Difficulty is now a PER-UNIT slot (added at spawn by
            // TrainingSystem/SpellSummon, and by EnsurePhase2CombatComponentsSystem as a fallback),
            // which is what CombatSystem's per-unit RefRO<Difficulty> query and SquadAI's per-team
            // Stats-axis write both read. No code reads a GLOBAL Difficulty singleton, so we do NOT
            // bake one here — two instances (global + per-unit) would make GetSingleton<Difficulty>
            // ambiguous. The multi-axis AI bias lives in the DifficultyAxes singleton instead.
        }

        /// <summary>
        /// TWO per-team UnitCatalogTag entities (Phase-2 two-faction change): Team 0 = playerUnits
        /// (Iron Pact), Team 1 = aiUnits (Ashen Horde). Each holds a DynamicBuffer&lt;UnitSpawnStats&gt;
        /// baked from that faction's authored UnitDefs (§15.6 value copy). TrainOrder.UnitIndex and
        /// SpellDef.summonUnitIndex are SIDE-RELATIVE — they index that side's own roster. Sim readers
        /// resolve the right one via UnitCatalog.TryGetForTeam.
        /// </summary>
        private void BuildUnitCatalogs(EntityManager em)
        {
            // ONLY the player roster receives the server-owned resolved-upgrade deltas (C2). The AI/enemy
            // roster is always baked from base authored stats (no meta upgrades on the AI side, §3/§15).
            BuildOneCatalog(em, PlayerTeam, playerUnits, "UnitCatalog_Player", applyUpgrades: true);
            BuildOneCatalog(em, AiTeam, aiUnits, "UnitCatalog_AI", applyUpgrades: false);
        }

        private void BuildOneCatalog(EntityManager em, int team, UnitDef[] roster, string name, bool applyUpgrades)
        {
            Entity catalog = em.CreateEntity();
            em.SetName(catalog, name);
            em.AddComponentData(catalog, new UnitCatalogTag { Team = team });

            DynamicBuffer<UnitSpawnStats> buf = em.AddBuffer<UnitSpawnStats>(catalog);
            buf.Capacity = roster.Length;
            for (int i = 0; i < roster.Length; i++)
            {
                UnitDef u = roster[i];

                // Base authored stats (the ONLY balance source — §15). Deltas (if any) are ADDED below.
                float maxHealth      = u.maxHealth;
                float moveSpeed      = u.moveSpeed;
                float attackDamage   = u.attackDamage;
                float attackInterval = u.attackInterval;

                // RESOLVED-UPGRADE BAKE (C2 / §13 3.2): fold the player's SERVER-OWNED, cap-clamped upgrade
                // deltas onto the base. The deltas were produced by the meta launcher via UpgradeMath
                // (clamped to maxLevel), so this can NEVER exceed the HARD CAP (§3/§15 no-P2W). Additive,
                // mirroring the §4 base→modifier chain. AttackSpeed lowers the interval (faster attacks),
                // floored just above 0 so a delta can never produce a non-positive interval.
                if (applyUpgrades && playerUpgradeDeltas != null && !string.IsNullOrEmpty(u.id)
                    && playerUpgradeDeltas.TryGetValue(u.id, out UnitUpgradeDeltas d))
                {
                    maxHealth      += d.Health;
                    moveSpeed      += d.MoveSpeed;
                    attackDamage   += d.Damage;
                    attackInterval += d.AttackIntervalDelta;
                    if (attackInterval < 0.01f) attackInterval = 0.01f; // never non-positive (sane swing window)
                    if (maxHealth < 0f) maxHealth = 0f;
                    if (moveSpeed < 0f) moveSpeed = 0f;
                    if (attackDamage < 0f) attackDamage = 0f;
                }

                buf.Add(new UnitSpawnStats
                {
                    // Data enums and sim-side mirror enums share identical int values; cast is exact.
                    DamageType       = (DamageTypeId)(int)u.damageType,
                    ArmorClass       = (ArmorClassId)(int)u.armorClass,
                    Role             = (RoleId)(int)u.role,
                    MaxHealth        = maxHealth,
                    MoveSpeed        = moveSpeed,
                    AttackDamage     = attackDamage,
                    AttackRange      = u.attackRange,
                    AttackInterval   = attackInterval,
                    GoldCost         = u.goldCost,
                    TrainSeconds     = u.trainSeconds,
                    MiningRatePerSec = u.miningRatePerSec,
                });
            }
        }

        /// <summary>
        /// CounterMatrixTag singleton + 20 CounterCell from counterMatrix.counterMatrix (the FULL 5×4).
        /// Index = (damageType-1)*4 + (armorClass-1); a 0 cell means "unset → 1.0" downstream.
        /// </summary>
        private void BuildCounterMatrix(EntityManager em)
        {
            Entity matrix = em.CreateEntity();
            em.SetName(matrix, "CounterMatrix");
            em.AddComponent<CounterMatrixTag>(matrix);

            DynamicBuffer<CounterCell> cells = em.AddBuffer<CounterCell>(matrix);
            cells.Length = CounterMatrix.CellCount; // 20 = 5 damage types × 4 armor classes (§4)
            float[] src = counterMatrix.counterMatrix;
            for (int i = 0; i < cells.Length; i++)
            {
                float m = (src != null && i < src.Length) ? src[i] : 0f;
                cells[i] = new CounterCell { Multiplier = m }; // 0 → treated as 1.0 by CounterMatrix.Lookup
            }
        }

        // ---------------------------------------------------------------- spells (§13 2.4 / §5.3)

        /// <summary>
        /// Spell wiring (Spell.cs EDIT 5):
        ///   • SpellCatalogTag singleton + DynamicBuffer&lt;SpellCatalogEntry&gt; baked field-by-field
        ///     from spellPool[] (index == SpellSlot.SpellIndex).
        ///   • SpellCastInboxTag singleton with an empty DynamicBuffer&lt;SpellCastRequest&gt; (the SAME
        ///     inbox the player Control shell + SquadAI append to).
        ///   • One SpellLoadoutTag{Team} per side, each with an empty DynamicBuffer&lt;SpellSlot&gt;
        ///     (filled by the player's DraftAndSpellInput / an AI draft pass).
        ///   • DraftState singleton {PlayerDone=0, AiDone=0} — the §13 2.4 draft gate.
        /// The actual draft (which 3 each side picks) is a Control/AI step (DEFERRED); we only bake
        /// the EMPTY containers + catalog so those writers and the SpellSystem have somewhere to write.
        /// </summary>
        private void BuildSpellSetup(EntityManager em)
        {
            // --- SpellCatalog (DATA value copy of the authored pool, §15.6) ---
            Entity catalog = em.CreateEntity();
            em.SetName(catalog, "SpellCatalog");
            em.AddComponent<SpellCatalogTag>(catalog);
            DynamicBuffer<SpellCatalogEntry> entries = em.AddBuffer<SpellCatalogEntry>(catalog);
            if (spellPool != null)
            {
                entries.Capacity = spellPool.Length;
                for (int i = 0; i < spellPool.Length; i++)
                {
                    SpellDef def = spellPool[i];
                    if (def == null) continue; // skip a null slot rather than fault the whole bake.
                    entries.Add(new SpellCatalogEntry
                    {
                        Category             = def.category,
                        Cooldown             = def.cooldown,
                        Charges              = def.charges,
                        TelegraphTime        = def.telegraphTime,
                        TargetShape          = def.targetShape,
                        Radius               = def.radius,
                        Magnitude            = def.magnitude,
                        Duration             = def.duration,
                        AppliesStatus        = def.appliesStatus,
                        SummonUnitIndex      = def.summonUnitIndex,
                        SynergyBonusVsStatus = def.synergyBonusVsStatus,
                        SynergyMultiplier    = def.synergyMultiplier,
                    });
                }
            }

            // --- Shared cast inbox (empty; player + SquadAI append; SpellCastSystem drains) ---
            Entity inbox = em.CreateEntity();
            em.SetName(inbox, "SpellCastInbox");
            em.AddComponent<SpellCastInboxTag>(inbox);
            em.AddBuffer<SpellCastRequest>(inbox);

            // --- Per-side spell loadouts (empty SpellSlot buffer; drafted later) ---
            BuildLoadout(em, PlayerTeam, "SpellLoadout_Player");
            BuildLoadout(em, AiTeam, "SpellLoadout_AI");

            // --- Draft gate singleton ---
            Entity draft = em.CreateEntity();
            em.SetName(draft, "DraftState");
            em.AddComponentData(draft, new DraftState { PlayerDone = 0, AiDone = 0 });
        }

        private void BuildLoadout(EntityManager em, int team, string name)
        {
            Entity loadout = em.CreateEntity();
            em.SetName(loadout, name);
            em.AddComponentData(loadout, new SpellLoadoutTag { Team = team });
            em.AddBuffer<SpellSlot>(loadout); // empty; ConfirmDraft / AI draft fills it.
        }

        // ---------------------------------------------------------------- difficulty + scheduler (§13 2.6)

        /// <summary>
        /// DifficultyAxes{1,1,1,1} (neutral baseline; SquadAI biases AI pacing off it) + the budgeted
        /// AISchedulerState{Cursor=0, BudgetPerFrame} singleton (§4 frame-stable AI). Both are DATA
        /// singletons read by the §13 2.6 squad layer.
        /// </summary>
        private void BuildDifficultyAndScheduler(EntityManager em)
        {
            Entity axes = em.CreateEntity();
            em.SetName(axes, "DifficultyAxes");
            em.AddComponentData(axes, new DifficultyAxes { Eco = 1f, Aggression = 1f, Stats = 1f, SpellAccess = 1f });

            Entity sched = em.CreateEntity();
            em.SetName(sched, "AISchedulerState");
            em.AddComponentData(sched, new AISchedulerState
            {
                Cursor = 0,
                BudgetPerFrame = math.max(1, aiSchedulerBudgetPerFrame),
            });
        }

        // ---------------------------------------------------------------- per side

        /// <summary>
        /// Builds one side's setup entities: a GoldStore + control tag + TeamSpawnPoint bundle, the
        /// side's statue (+ damage inbox), a TrainQueue entity, a per-side CommanderRuntime (from the
        /// CommanderDef), and (AI side only) an AICommander stance entity.
        /// </summary>
        private void BuildSide(EntityManager em, int team, float2 spawnPos, float2 statuePos,
                               CommanderDef commander, bool isPlayer)
        {
            // --- Side controller entity: per-team GoldStore + control tag + spawn anchor. ---
            Entity side = em.CreateEntity();
            em.SetName(side, isPlayer ? "Side_Player" : "Side_AI");
            em.AddComponentData(side, new GoldStore { Team = team, Amount = startingGold }); // LOCAL battle gold
            em.AddComponentData(side, new TeamSpawnPoint { Team = team, Pos = spawnPos });
            if (isPlayer) em.AddComponent<PlayerControlled>(side);
            else          em.AddComponent<AIControlled>(side);

            // --- Statue objective at this side's end of the front (§11). ---
            Entity statue = em.CreateEntity();
            em.SetName(statue, isPlayer ? "Statue_Player" : "Statue_AI");
            em.AddComponentData(statue, new StatueTag { Team = team });
            em.AddComponentData(statue, new StatueState
            {
                Health          = statueMaxHealth,
                MaxHealth       = statueMaxHealth,
                ShieldHealth    = statueShieldHealth,
                ShieldActive    = statueShieldHealth > 0f,
                Phase           = StatuePhase.Intact,
                TrickleAccum    = 0f,
                TrickleThrottle = statueTrickleThrottle,
            });
            em.AddComponentData(statue, new Position { Value = statuePos });
            em.AddComponentData(statue, new Team { Id = team });
            em.AddBuffer<StatueDamageInbox>(statue); // CombatSystem/SpellSystem append; StatueDamageSystem drains.

            // --- Per-side training queue (holds the TrainOrder buffer). ---
            Entity queue = em.CreateEntity();
            em.SetName(queue, isPlayer ? "TrainQueue_Player" : "TrainQueue_AI");
            em.AddComponentData(queue, new TrainQueueTag { Team = team });
            em.AddBuffer<TrainOrder>(queue); // starts empty; TrainingSystem fills/ticks it.

            // --- Per-side commander (§13 2.5 / §6): exactly 1 active + 1 passive, from DATA. ---
            if (commander != null)
                BuildCommanderRuntime(em, team, commander, isPlayer);

            // --- AI commander stance entity (AI side only, §13 1.5). ---
            if (!isPlayer)
            {
                Entity aiCmd = em.CreateEntity();
                em.SetName(aiCmd, "AICommander");
                em.AddComponentData(aiCmd, new AICommander
                {
                    Team           = team,
                    Stance         = aiStartingStance,
                    ReevalCooldown = aiReevalCooldown,
                });
            }
        }

        /// <summary>
        /// One CommanderRuntime per side, copied field-by-field from the CommanderDef (§13 2.5).
        /// PowerBudgetPct is taken straight from DATA (the schema [Range(0,0.15)] already enforces
        /// the §6 ceiling); CommanderAbilitySystem additionally hard-clamps every magnitude at
        /// runtime, so this bake cannot exceed the budget. ActiveTimer starts at 0 (ready).
        /// </summary>
        private void BuildCommanderRuntime(EntityManager em, int team, CommanderDef def, bool isPlayer)
        {
            Entity cmd = em.CreateEntity();
            em.SetName(cmd, isPlayer ? "CommanderRuntime_Player" : "CommanderRuntime_AI");
            em.AddComponentData(cmd, new CommanderRuntime
            {
                Team            = team,
                Active          = def.active,
                Passive         = def.passive,
                ActiveCooldown  = def.activeCooldown,
                ActiveTimer     = 0f,               // ready at battle start.
                ActiveMagnitude = def.activeMagnitude,
                ActiveRadius    = def.activeRadius,
                ActiveDuration  = def.activeDuration,
                PassiveMagnitude = def.passiveMagnitude,
                PowerBudgetPct  = def.powerBudgetPct,            // §6 cap (schema-clamped; runtime re-clamped).
                RankedNormalized = (byte)(def.rankedNormalized ? 1 : 0),
                ActiveRequested = 0,
            });
        }

        // ---------------------------------------------------------------- AI squads (§13 2.6)

        /// <summary>
        /// Creates the AI side's tactical squads: a SquadAIState (posture utility) + a SquadFormation
        /// (anchor/type) per squad, keyed by a unique SquadId. SquadAI reads/writes these; FormationSystem
        /// lays members out around the anchor. Members (FormationMember) are NOT assigned here — units
        /// spawn from the AI's train queue; assigning them to squads is a §13 2.6 spawn-layer step (DEFERRED).
        /// The anchor seeds at the AI spawn; SquadAI nudges it by posture each reeval.
        /// </summary>
        private void BuildAISquads(EntityManager em, float2 aiSpawn)
        {
            // Squads face the player end of the front (toward team0's statue).
            float2 facing = math.normalizesafe(new float2(map.team0StatueX - map.team1StatueX, 0f), new float2(-1f, 0f));
            for (int i = 0; i < math.max(1, aiSquadCount); i++)
            {
                int squadId = AiTeam * 1000 + i; // stable, side-namespaced squad id.

                Entity sq = em.CreateEntity();
                em.SetName(sq, $"SquadAI_{i}");
                em.AddComponentData(sq, new SquadAIState
                {
                    SquadId        = squadId,
                    Team           = AiTeam,
                    Posture        = SquadPosture.Hold,
                    FocusTarget    = Entity.Null,
                    ReevalCooldown = 0f,
                });

                Entity form = em.CreateEntity();
                em.SetName(form, $"SquadFormation_{i}");
                em.AddComponentData(form, new SquadFormation
                {
                    SquadId = squadId,
                    Type    = FormationType.Line,
                    Anchor  = aiSpawn,
                    Facing  = facing,
                });
            }
        }

        // ---------------------------------------------------------------- terrain (§11 / §4)

        /// <summary>
        /// Spawns the MapDef's terrain features (2–4 per §11). Each TerrainFeature carries its kind,
        /// width, row, and the four effect multipliers. The multipliers come from the PROVISIONAL
        /// per-kind default table (DefaultsFor) — clearly flagged LSD-owned (§15.6) until the
        /// MapDef/TerrainPlacement schema carries them as authored fields. A feature's world center
        /// is its Position.x (TerrainSystem matches occupancy by Position.x ± Width/2 within the row).
        /// </summary>
        private void BuildTerrain(EntityManager em, float midRowY)
        {
            if (map.terrain == null) return;
            for (int i = 0; i < map.terrain.Count; i++)
            {
                TerrainPlacement tp = map.terrain[i];
                TerrainKindDefaults d = DefaultsFor(tp.kind);

                Entity feat = em.CreateEntity();
                em.SetName(feat, $"Terrain_{tp.kind}_{i}");
                em.AddComponentData(feat, new TerrainFeature
                {
                    Kind        = tp.kind,
                    Width       = tp.width,
                    Row         = tp.row,
                    AttackMult  = d.Attack,   // PROVISIONAL (§15.6) — LSD-owned per-kind default.
                    DefenseMult = d.Defense,  // PROVISIONAL — Cover incoming-damage reduction (A1 cover read).
                    MoveMult    = d.Move,     // PROVISIONAL — Choke move-speed reduction (A2 choke read).
                    HazardDps   = d.Hazard,   // PROVISIONAL — Hazard damage/sec.
                });
                // The feature's world center along the front; row Y derived from the map's row band.
                float y = midRowY + (tp.row - 1) * rowSpacing;
                em.AddComponentData(feat, new Position { Value = new float2(tp.x, y) });
            }
        }

        // ---------------------------------------------------------------- mines (§11)

        /// <summary>
        /// Spawns the MapDef's miner-capped, contestable mine nodes (§11). All values (x, capacity,
        /// yield, owner) come from the MinePlacement DATA. If the map authored none, this is a no-op
        /// (the slice still runs, but the economy has no nodes — flagged for the content pass).
        /// </summary>
        private void BuildMines(EntityManager em, float midRowY)
        {
            if (map.mines == null) return;
            for (int i = 0; i < map.mines.Count; i++)
            {
                MinePlacement mp = map.mines[i];

                Entity mine = em.CreateEntity();
                em.SetName(mine, $"Mine_{i}");
                em.AddComponentData(mine, new MineNode
                {
                    Capacity    = math.max(1, mp.capacity),
                    Occupants   = 0,
                    YieldPerSec = mp.yieldPerSec, // PROVISIONAL data (LSD-owned)
                    OwnerTeam   = mp.ownerTeam,    // -1 = contestable / neutral
                });
                em.AddComponentData(mine, new Position { Value = new float2(mp.x, midRowY) });
            }
        }

        // ---------------------------------------------------------------- camera

        /// <summary>
        /// Frames the camera on the front. Pure control/render concern (§12) — the sim never reads
        /// the camera. No-op if no camera was assigned.
        /// </summary>
        private void FrameCamera()
        {
            if (battleCamera == null) return;

            float centerX = 0.5f * (map.team0StatueX + map.team1StatueX);
            float width = math.abs(map.team1StatueX - map.team0StatueX);
            battleCamera.transform.position = new Vector3(centerX, rowSpacing, -10f);
            if (battleCamera.orthographic)
                battleCamera.orthographicSize = math.max(5f, width * 0.6f);
        }
    }
}
