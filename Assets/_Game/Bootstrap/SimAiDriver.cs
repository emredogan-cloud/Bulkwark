// BULWARK — SIM AI DRIVER (Pre-Phase-5 GATE-1 viz track, PHASE V2). TEMPORARY, REMOVABLE, DEBUG-ONLY.
//
// WHY: the AI commander/squad layer (BasicAI/SquadAI) DECIDES correctly (it even queues miners for economy),
// but spawned AI units never MOVE — the squad→FormationMember→FormationSystem pipeline is unterminated (no
// FormationMember is ever stamped on a unit) and MovementSystem has no "march when no target" fallback, so AI
// units idle exactly like untargeted player units. This driver mirrors the player's ADVANCE (SimPlayerHud):
// it periodically sets MoveDestination{playerStatue, Active=1} on Team-1 units so they march to contact and
// the shared Targeting/Combat core produces real TWO-SIDED combat. RC-4: the old VICTORY-LATCH PROBE has been
// REMOVED — matches now resolve ONLY from real statue destruction (no artificial force-finish).
//
// INVIOLABLE: this is the §12 CONTROL layer. It writes ONLY input data the existing ECS systems already
// consume — MoveDestination overrides (the same component MovementSystem reads; identical to SimPlayerHud's
// AdvanceAllPlayerUnits). It changes NO sim rule, NO balance number, NO unit/statue stat. Deleting this
// one file removes it 100%.
// SCAFFOLD STATUS: authored here; CI compiles; device run produces the evidence.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Temporary debug driver: advances AI units (mirror of the player ADVANCE) + a victory-latch probe.</summary>
    public sealed class SimAiDriver : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("SimAiDriver");
            go.AddComponent<SimAiDriver>();
            DontDestroyOnLoad(go);
            Debug.Log("[AIDRV] SimAiDriver booted (AfterSceneLoad).");
        }

        private const int PlayerTeam = 0, AiTeam = 1;
        // RC-4: the VICTORY-LATCH PROBE has been REMOVED. Matches now end ONLY by real statue destruction
        // (CombatSystem → StatueDamageInbox → StatueDamageSystem → Health≤0 → MatchState). No artificial force-finish.

        private World _w;
        private EntityManager _em;
        private bool _ready;
        private float _t0 = -1f;
        private float _lastAdvance = -100f;

        private bool EnsureWorld()
        {
            World w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) { _ready = false; return false; }
            if (!_ready || w != _w) { _w = w; _em = w.EntityManager; _ready = true; }
            return true;
        }

        private void Update()
        {
            if (!EnsureWorld()) return;
            try
            {
                if (_t0 < 0f) _t0 = Time.unscaledTime;
                float el = Time.unscaledTime - _t0;

                // Advance AI units toward the player statue (two-sided combat). Cadence mirrors the player push.
                if (el > 8f && (Time.unscaledTime - _lastAdvance) > 5f)
                {
                    int n = AdvanceTeamTowardEnemyStatue(AiTeam, PlayerTeam);
                    if (n > 0) Debug.Log($"[AIDRV] advance {n} AI units toward the player statue.");
                    _lastAdvance = Time.unscaledTime;
                }
                // RC-4: no victory-latch probe — the match resolves only from real statue destruction.
            }
            catch (System.Exception e) { Debug.LogError("[AIDRV] error: " + e.Message); }
        }

        /// <summary>Set MoveDestination{enemyStatue, Active=1} on every unit of <paramref name="team"/>
        /// (add-if-missing). Same control write as SimPlayerHud.AdvanceAllPlayerUnits — MovementSystem consumes it.</summary>
        private int AdvanceTeamTowardEnemyStatue(int team, int enemyTeam)
        {
            if (!TryGetStatuePos(enemyTeam, out float2 target)) return 0;
            int n = 0;
            var uq = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Team>());
            using (var ents = uq.ToEntityArray(Allocator.Temp))
            using (var tm = uq.ToComponentDataArray<Team>(Allocator.Temp))
                for (int i = 0; i < ents.Length; i++)
                {
                    if (tm[i].Id != team) continue;
                    if (_em.HasComponent<MinerTag>(ents[i])) continue; // keep AI miners on the mines (sustain economy)
                    var dest = new MoveDestination { Value = target, Active = 1 };
                    if (_em.HasComponent<MoveDestination>(ents[i])) _em.SetComponentData(ents[i], dest);
                    else _em.AddComponentData(ents[i], dest);
                    n++;
                }
            return n;
        }

        private bool TryGetStatuePos(int team, out float2 pos)
        {
            pos = float2.zero;
            bool found = false;
            var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<Position>());
            using (var stag = sq.ToComponentDataArray<StatueTag>(Allocator.Temp))
            using (var sp = sq.ToComponentDataArray<Position>(Allocator.Temp))
                for (int i = 0; i < stag.Length; i++) if (stag[i].Team == team) { pos = sp[i].Value; found = true; }
            return found;
        }

    }
}
