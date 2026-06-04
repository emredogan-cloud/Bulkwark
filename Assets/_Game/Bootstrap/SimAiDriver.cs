// BULWARK — SIM AI DRIVER (Pre-Phase-5 GATE-1 viz track, PHASE V2). TEMPORARY, REMOVABLE, DEBUG-ONLY.
//
// WHY: the AI commander/squad layer (BasicAI/SquadAI) DECIDES correctly (it even queues miners for economy),
// but spawned AI units never MOVE — the squad→FormationMember→FormationSystem pipeline is unterminated (no
// FormationMember is ever stamped on a unit) and MovementSystem has no "march when no target" fallback, so AI
// units idle exactly like untargeted player units. This driver mirrors the player's ADVANCE (SimPlayerHud):
// it periodically sets MoveDestination{playerStatue, Active=1} on Team-1 units so they march to contact and
// the shared Targeting/Combat core produces real TWO-SIDED combat. It also fires a one-shot VICTORY-LATCH
// PROBE late in the match (if still Ongoing) to deterministically validate the
// StatueDamage→MatchState→freeze chain for the GATE-1 report.
//
// INVIOLABLE: this is the §12 CONTROL layer. It writes ONLY input data the existing ECS systems already
// consume — MoveDestination overrides (the same component MovementSystem reads; identical to SimPlayerHud's
// AdvanceAllPlayerUnits) and, for the probe, a StatueDamageInbox entry (the SAME buffer CombatSystem appends to
// and StatueDamageSystem drains). It changes NO sim rule, NO balance number, NO unit/statue stat. Deleting this
// one file removes it 100%. The probe is a clearly-labeled validation aid, not "balance".
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
        // VICTORY-LATCH PROBE: which statue to finish if the match is still Ongoing at ProbeAtSeconds.
        // AiTeam ⇒ destroy the AI statue ⇒ expect Victory. Flip to PlayerTeam to validate Defeat (symmetric path).
        private const int ProbeTargetTeam = AiTeam;
        private const float ProbeAtSeconds = 120f;

        private World _w;
        private EntityManager _em;
        private bool _ready;
        private float _t0 = -1f;
        private float _lastAdvance = -100f;
        private bool _probeFired;

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

                // Victory/defeat LATCH PROBE: if the match has not resolved from real combat by ProbeAtSeconds,
                // deal a decisive blow to the target statue to validate StatueDamage→MatchState→freeze on device.
                if (!_probeFired && el > ProbeAtSeconds)
                {
                    _probeFired = true;
                    if (MatchIsOngoing())
                    {
                        bool ok = ForceFinishStatue(ProbeTargetTeam);
                        Debug.Log(ok
                            ? $"[AIDRV] VICTORY-LATCH PROBE: dealt decisive damage to Team {ProbeTargetTeam} statue (expect {(ProbeTargetTeam == AiTeam ? "Victory" : "Defeat")} + freeze)."
                            : "[AIDRV] VICTORY-LATCH PROBE: target statue not found.");
                    }
                    else Debug.Log("[AIDRV] match already resolved by real combat before the probe — no probe needed.");
                }
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

        private bool MatchIsOngoing()
        {
            using var m = _em.CreateEntityQuery(ComponentType.ReadOnly<MatchState>()).ToComponentDataArray<MatchState>(Allocator.Temp);
            return m.Length == 0 || m[0].Outcome == MatchOutcome.Ongoing;
        }

        /// <summary>Append a decisive StatueDamageInbox entry to the team's statue (the SAME path CombatSystem
        /// uses). StatueDamageSystem then drains it → Health≤0 → Phase=Destroyed → MatchState Victory/Defeat → freeze.</summary>
        private bool ForceFinishStatue(int team)
        {
            var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>());
            using var ents = sq.ToEntityArray(Allocator.Temp);
            using var tags = sq.ToComponentDataArray<StatueTag>(Allocator.Temp);
            for (int i = 0; i < ents.Length; i++)
            {
                if (tags[i].Team != team) continue;
                if (!_em.HasBuffer<StatueDamageInbox>(ents[i])) return false;
                var inbox = _em.GetBuffer<StatueDamageInbox>(ents[i]);
                inbox.Add(new StatueDamageInbox { Amount = 100000f }); // ≫ 250 shield + 1000 health → guaranteed kill
                return true;
            }
            return false;
        }
    }
}
