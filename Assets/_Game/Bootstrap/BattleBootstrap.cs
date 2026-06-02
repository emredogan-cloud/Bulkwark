// BULWARK — Phase-1 BATTLE BOOTSTRAP (control/setup shell). Roadmap §13 Phase 1
// (one playable micro-battle: mine→train→push→statue, 1 faction / 4 units, vs AI),
// §11 (statues at ends, contestable miner-capped mines, single 3-row front),
// §12 (MonoBehaviour CONTROL/SETUP layer ONLY — it WRITES entity data the ECS sim then
// reads; it contains NO gameplay simulation), §3 (preserved core loop), §15 (no invented
// content/balance: all numeric stats come from the authored UnitDef[]/BalanceConfig DATA).
//
// WHAT THIS DOES
// Builds ONE battle in code at Start() because no .unity scene is authorable in this
// environment. It constructs the initial entity state for the default ECS World:
//   • per-team GoldStore + control tag (Team 0 PlayerControlled / Team 1 AIControlled),
//   • two statues (StatueTag + StatueState + StatueDamageInbox buffer) at opposite ends,
//   • several miner-capped MineNodes mid-field (OwnerTeam = -1, contestable),
//   • a TeamSpawnPoint per side,
//   • the UnitCatalogTag singleton holding a DynamicBuffer<UnitSpawnStats> baked from units[],
//   • the CounterMatrixTag singleton holding 20 CounterCell from balance.counterMatrix,
//   • a TrainQueueTag entity per side holding an (empty) DynamicBuffer<TrainOrder>,
//   • an AICommander for Team 1,
//   • MatchState{ Ongoing } and a Difficulty{ 1.0 } singleton.
// It ALSO retires the Phase-0 spike systems (MoveSystem/AttackSystem) at setup so they
// never co-run with the Phase-1 set (see DisablePhase0SpikeSystems / §12 below).
//
// CANON DISCIPLINE
//   • NO balance numbers invented here. Stat values are COPIED from the authored UnitDef[]
//     and BalanceConfig assets (DATA). The only literals are NEUTRAL Phase-1 factor defaults
//     (Difficulty = 1.0) and readable LAYOUT positions (field geometry, not balance) — and
//     statue HP / mine capacity are surfaced as inspector fields so they remain data-tunable,
//     never hard-coded canon (§15.6).
//   • Phase-1 only: single faction, no terrain/formations/spells/commanders-collection,
//     no meta/economy persistence, no upgrades (§15.2/§15.3). GoldStore is LOCAL battle gold.
//   • No save-state logging (§4/§12 inviolable): this never logs/serializes match or save state.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002).
// DEFERRED (Unity-editor step): authoring a real Scene/SubScene, assigning the inspector
// references, and the GATE-1 fun verdict / on-device control feel / 200-unit perf / end-to-end
// playthrough — none can run without the Unity runtime.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Data;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>
    /// Place on a single GameObject in a near-empty bootstrap scene. Assign the authored
    /// UnitDef set, the BalanceConfig, and the battle Camera in the inspector. At Start()
    /// it builds the whole battle into the default ECS World.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleBootstrap : MonoBehaviour
    {
        // ----- Required data (DATA-driven; see §12 content-tools rule) -----
        [Header("Content (DATA — authored ScriptableObjects)")]
        [Tooltip("The Phase-1 faction's units (Phase 1 = exactly 4: Miner + 3 roles, §13 1.2). " +
                 "Index here is the canonical UnitIndex used by TrainOrder/UnitSpawnStats.")]
        public UnitDef[] units;

        [Tooltip("Counter matrix (20 cells) + roster. Only 'basic counters' are non-1 at P1 (§13 1.4).")]
        public BalanceConfig balance;

        [Tooltip("Battle camera (control/render layer). Positioned to frame the front; the sim never reads it.")]
        public Camera battleCamera;

        // ----- Readable battlefield layout (§11: single horizontal front, statues at ends) -----
        // These are POSITIONS/CAPACITIES (geometry + node config), not combat balance. Surfaced
        // for designer readability; statue/mine numbers stay tunable (not hard-coded canon).
        [Header("Battlefield layout (§11 — geometry, not balance)")]
        [Tooltip("Horizontal distance between the two statues (world units). Front spans this.")]
        public float frontWidth = 60f;

        [Tooltip("Vertical spacing between the 3 rows of the single front (§11).")]
        public float rowSpacing = 2.5f;

        [Header("Statue objective (§11 — HP/shield are tunable data, not canon)")]
        [Tooltip("Statue max health. PROVISIONAL/LSD-owned; tune via data, never assert as canon.")]
        public float statueMaxHealth = 1000f;

        [Tooltip("Statue shield pool absorbed before health (§11 shield phase). 0 = no shield.")]
        public float statueShieldHealth = 250f;

        [Tooltip("Min interval/threshold before recovered trickle damage applies (§11 statue math).")]
        public float statueTrickleThrottle = 0.5f;

        [Header("Gold mines (§11 — fixed, miner-capped, contestable)")]
        [Tooltip("Number of contestable mine nodes spread across mid-field.")]
        [Min(1)] public int mineCount = 3;

        [Tooltip("Miner cap per node (the spatial-contest lever, §11).")]
        [Min(1)] public int mineCapacity = 3;

        [Tooltip("Per-node Gold/sec yield ceiling. PROVISIONAL data (LSD-owned).")]
        public float mineYieldPerSec = 2f;

        [Header("Economy start (§13 1.1 — LOCAL battle gold, no persistence)")]
        [Tooltip("Starting Gold for each side (enough to train the first miner).")]
        public int startingGold = 100;

        [Header("AI (§13 1.5 — single-layer utility commander)")]
        public CommanderStance aiStartingStance = CommanderStance.Push;

        [Tooltip("AI commander re-eval throttle (seconds). Keeps the utility pass O(1)/tick, §12.")]
        public float aiReevalCooldown = 1.0f;

        [Header("Targeting (§13 1.4)")]
        [Tooltip("Same-target stickiness bias applied to the current target's score (×1.2, §13 1.4).")]
        public float targetingStickiness = 1.2f;

        private const int PlayerTeam = 0; // human (PlayerControlled)
        private const int AiTeam = 1;     // AI (AIControlled)

        private void Start()
        {
            if (!ValidateConfig()) return;

            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                Debug.LogError("[BattleBootstrap] No default ECS World — cannot build the battle.");
                return;
            }

            EntityManager em = world.EntityManager;

            // Retire the superseded Phase-0 spike BEFORE building state, so the forbidden
            // O(N^2) MoveSystem/AttackSystem never co-run with the Phase-1 Targeting/Movement/
            // Combat set (§12). Done at setup — not reactively on the decided-match edge.
            DisablePhase0SpikeSystems(world);

            BuildMatchSingletons(em);
            BuildUnitCatalog(em);
            BuildCounterMatrix(em);

            // Statues sit at the two ends of the single horizontal front (§11).
            float half = frontWidth * 0.5f;
            float midRowY = rowSpacing; // centre row of the 3-row front
            BuildSide(em, PlayerTeam, spawnPos: new float2(-half, midRowY), statuePos: new float2(-half - 4f, midRowY), isPlayer: true);
            BuildSide(em, AiTeam, spawnPos: new float2(half, midRowY), statuePos: new float2(half + 4f, midRowY), isPlayer: false);

            BuildMines(em);

            FrameCamera();
        }

        // ---------------------------------------------------------------- validation

        private bool ValidateConfig()
        {
            if (units == null || units.Length == 0)
            {
                Debug.LogError("[BattleBootstrap] 'units' is empty — assign the Phase-1 faction UnitDefs (§13 1.2).");
                return false;
            }
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] == null)
                {
                    Debug.LogError($"[BattleBootstrap] units[{i}] is null — assign a UnitDef.");
                    return false;
                }
            }
            if (balance == null)
            {
                Debug.LogError("[BattleBootstrap] 'balance' is unassigned — assign the BalanceConfig (counter matrix).");
                return false;
            }
            return true;
        }

        // ---------------------------------------------------------------- phase-0 retirement

        /// <summary>
        /// Retire the Phase-0 spike systems (formerly MoveSystem/AttackSystem in SimSystems.cs),
        /// which were SUPERSEDED by the Phase-1 set (Targeting+Movement+Combat) and used
        /// AttackState.Target + an O(N^2) nearest-enemy scan the shared contract forbids in
        /// Phase 1 (§12 O(1) rule).
        ///
        /// RECONCILIATION: those spike system TYPES were intentionally DELETED (SimSystems.cs is
        /// now empty — see its header), so there is nothing to look up/disable here by type; this
        /// is now a documented NO-OP. As belt-and-braces, MatchFlowSystem still carries them in
        /// SimSystemOrder.RetiredPhase0Systems as STRING freeze targets (which match nothing now,
        /// harmlessly) in case a different entry point ever reintroduced a like-named system.
        /// </summary>
        private void DisablePhase0SpikeSystems(World world)
        {
            // No Phase-0 spike systems exist anymore (types deleted). Intentional no-op.
        }

        // ---------------------------------------------------------------- singletons

        /// <summary>MatchState{Ongoing} + Difficulty{1.0}. Both are true ECS singletons.</summary>
        private void BuildMatchSingletons(EntityManager em)
        {
            Entity match = em.CreateEntity();
            em.SetName(match, "MatchState");
            em.AddComponentData(match, new MatchState { Outcome = MatchOutcome.Ongoing });

            Entity diff = em.CreateEntity();
            em.SetName(diff, "Difficulty");
            // Neutral Phase-1 factor slot (multi-axis difficulty = Phase 2.6, forbidden now).
            em.AddComponentData(diff, new Difficulty { Multiplier = 1.0f });
        }

        /// <summary>
        /// UnitCatalogTag singleton + DynamicBuffer&lt;UnitSpawnStats&gt; baked from units[].
        /// Pure value copy of authored UnitDef DATA (§15.6) — no balance is authored here.
        /// </summary>
        private void BuildUnitCatalog(EntityManager em)
        {
            Entity catalog = em.CreateEntity();
            em.SetName(catalog, "UnitCatalog");
            em.AddComponent<UnitCatalogTag>(catalog);

            DynamicBuffer<UnitSpawnStats> roster = em.AddBuffer<UnitSpawnStats>(catalog);
            roster.Capacity = units.Length;
            for (int i = 0; i < units.Length; i++)
            {
                UnitDef u = units[i];
                roster.Add(new UnitSpawnStats
                {
                    // Data enums and sim-side mirror enums share identical int values; cast is exact.
                    DamageType       = (DamageTypeId)(int)u.damageType,
                    ArmorClass       = (ArmorClassId)(int)u.armorClass,
                    Role             = (RoleId)(int)u.role,
                    MaxHealth        = u.maxHealth,
                    MoveSpeed        = u.moveSpeed,
                    AttackDamage     = u.attackDamage,
                    AttackRange      = u.attackRange,
                    AttackInterval   = u.attackInterval,
                    GoldCost         = u.goldCost,
                    TrainSeconds     = u.trainSeconds,
                    MiningRatePerSec = u.miningRatePerSec,
                });
            }
        }

        /// <summary>
        /// CounterMatrixTag singleton + 20 CounterCell from balance.counterMatrix.
        /// Index = (damageType-1)*4 + (armorClass-1); a 0 cell means "unset → 1.0" downstream.
        /// </summary>
        private void BuildCounterMatrix(EntityManager em)
        {
            Entity matrix = em.CreateEntity();
            em.SetName(matrix, "CounterMatrix");
            em.AddComponent<CounterMatrixTag>(matrix);

            DynamicBuffer<CounterCell> cells = em.AddBuffer<CounterCell>(matrix);
            cells.Length = 20; // 5 damage types × 4 armor classes
            float[] src = balance.counterMatrix;
            for (int i = 0; i < 20; i++)
            {
                float m = (src != null && i < src.Length) ? src[i] : 0f;
                cells[i] = new CounterCell { Multiplier = m }; // 0 → treated as 1.0 by Combat (§13 1.4)
            }
        }

        // ---------------------------------------------------------------- per side

        /// <summary>
        /// Builds one side's setup entities: a GoldStore + control tag + TeamSpawnPoint bundle,
        /// the side's statue (+ damage inbox), a TrainQueue entity, and (AI side only) an AICommander.
        /// </summary>
        private void BuildSide(EntityManager em, int team, float2 spawnPos, float2 statuePos, bool isPlayer)
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
            em.AddBuffer<StatueDamageInbox>(statue); // CombatSystem appends; StatueDamageSystem drains.

            // --- Per-side training queue (holds the TrainOrder buffer). ---
            Entity queue = em.CreateEntity();
            em.SetName(queue, isPlayer ? "TrainQueue_Player" : "TrainQueue_AI");
            em.AddComponentData(queue, new TrainQueueTag { Team = team });
            em.AddBuffer<TrainOrder>(queue); // starts empty; TrainingSystem fills/ticks it.

            // --- AI commander (AI side only, §13 1.5). ---
            if (!isPlayer)
            {
                Entity commander = em.CreateEntity();
                em.SetName(commander, "AICommander");
                em.AddComponentData(commander, new AICommander
                {
                    Team           = team,
                    Stance         = aiStartingStance,
                    ReevalCooldown = aiReevalCooldown,
                });
            }
        }

        // ---------------------------------------------------------------- mines

        /// <summary>
        /// Several fixed, miner-capped, contestable mine nodes spread across mid-field (§11).
        /// OwnerTeam = -1 (neutral/contestable). Positions are geometry, not balance.
        /// </summary>
        private void BuildMines(EntityManager em)
        {
            float midRowY = rowSpacing;
            // Evenly distribute mines across the central third of the front.
            float span = frontWidth * 0.5f;       // total spread (centre half of the field)
            float start = -span * 0.5f;
            float step = mineCount > 1 ? span / (mineCount - 1) : 0f;

            for (int i = 0; i < mineCount; i++)
            {
                float x = (mineCount > 1) ? start + step * i : 0f;
                // Alternate mines onto outer rows so the spatial contest spans the front (§11).
                float y = midRowY + ((i % 3) - 1) * rowSpacing;

                Entity mine = em.CreateEntity();
                em.SetName(mine, $"Mine_{i}");
                em.AddComponentData(mine, new MineNode
                {
                    Capacity    = mineCapacity,
                    Occupants   = 0,
                    YieldPerSec = mineYieldPerSec, // PROVISIONAL data (LSD-owned)
                    OwnerTeam   = -1,              // contestable / neutral
                });
                em.AddComponentData(mine, new Position { Value = new float2(x, y) });
            }
        }

        // ---------------------------------------------------------------- camera

        /// <summary>
        /// Frames the camera on the front. Pure control/render concern (§12) — the sim never
        /// reads the camera. No-op if no camera was assigned.
        /// </summary>
        private void FrameCamera()
        {
            if (battleCamera == null) return;

            // Centre on the front; pull back to fit its width with a margin. 2D/orthographic-friendly.
            battleCamera.transform.position = new Vector3(0f, rowSpacing, -10f);
            if (battleCamera.orthographic)
                battleCamera.orthographicSize = math.max(5f, frontWidth * 0.6f);
        }
    }
}
