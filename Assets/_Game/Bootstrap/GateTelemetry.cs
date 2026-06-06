// BULWARK — GATE-1 DISCOVERY INSTRUMENTATION (Fun Validation Campaign). READ-ONLY, REMOVABLE.
//
// This is NOT a gameplay feature / UI / asset / audio / VFX / animation. It is a logcat-only telemetry logger
// that periodically samples EXISTING ECS state (read-only) during a match so the GATE-1 campaign can MEASURE
// (Phase C) instead of fabricating numbers. It performs ZERO ECS writes and changes NO rule / balance / AI /
// economy / canon. Deleting this one file removes it 100%. (Added solely to produce real evidence per the
// campaign's "no fabrication" rule; it does not alter what the game does.)

using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Read-only per-match telemetry to logcat (GATE-1 discovery). No gameplay effect.</summary>
    public sealed class GateTelemetry : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("GateTelemetry");
            go.AddComponent<GateTelemetry>();
            DontDestroyOnLoad(go);
            Debug.Log("[GATE1] telemetry logger booted (read-only, logcat only).");
        }

        private const int P0 = 0, P1 = 1;
        private World _w; private EntityManager _em; private bool _ready;
        private bool _wasInMatch, _ended; private float _matchT, _logTimer; private int _matchNo;

        private bool EnsureWorld()
        {
            var w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) { _ready = false; return false; }
            if (!_ready || w != _w) { _w = w; _em = w.EntityManager; _ready = true; }
            return true;
        }

        private void Update()
        {
            bool inMatch = PresentationState.InMatch;
            if (inMatch && !_wasInMatch) { _matchT = 0f; _logTimer = 0f; _ended = false; _matchNo++; Debug.Log($"[GATE1] MATCH {_matchNo} START"); }
            _wasInMatch = inMatch;
            if (!inMatch || !EnsureWorld()) return;

            _matchT += Time.deltaTime;
            _logTimer += Time.deltaTime;
            string outcome = ReadOutcome();
            if (outcome != "Ongoing" && !_ended)
            {
                _ended = true; Sample(outcome);
                Debug.Log($"[GATE1] MATCH {_matchNo} END outcome={outcome} dur={_matchT:0.0}s");
                return;
            }
            if (_logTimer >= 3f) { _logTimer = 0f; Sample(outcome); }
        }

        private string ReadOutcome()
        {
            try
            {
                using var q = _em.CreateEntityQuery(ComponentType.ReadOnly<MatchState>());
                using var a = q.ToComponentDataArray<MatchState>(Allocator.Temp);
                if (a.Length > 0) return a[0].Outcome.ToString();
            }
            catch { }
            return "Ongoing";
        }

        private void Sample(string outcome)
        {
            int gP = -1, gA = -1, uP = 0, uA = 0, mP = 0, mA = 0, atkP = 0, atkA = 0; float sP = -1, sA = -1;
            try
            {
                using (var g = _em.CreateEntityQuery(ComponentType.ReadOnly<GoldStore>()).ToComponentDataArray<GoldStore>(Allocator.Temp))
                    for (int i = 0; i < g.Length; i++) { if (g[i].Team == P0) gP = g[i].Amount; else if (g[i].Team == P1) gA = g[i].Amount; }

                var uq = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Team>());
                using (var t = uq.ToComponentDataArray<Team>(Allocator.Temp))
                    for (int i = 0; i < t.Length; i++) { if (t[i].Id == P0) uP++; else if (t[i].Id == P1) uA++; }

                var mq = _em.CreateEntityQuery(ComponentType.ReadOnly<MinerTag>(), ComponentType.ReadOnly<Team>());
                using (var t = mq.ToComponentDataArray<Team>(Allocator.Temp))
                    for (int i = 0; i < t.Length; i++) { if (t[i].Id == P0) mP++; else if (t[i].Id == P1) mA++; }

                var aq = _em.CreateEntityQuery(ComponentType.ReadOnly<AttackState>(), ComponentType.ReadOnly<Team>());
                using (var at = aq.ToComponentDataArray<AttackState>(Allocator.Temp))
                using (var t = aq.ToComponentDataArray<Team>(Allocator.Temp))
                    for (int i = 0; i < at.Length; i++) if (at[i].Target != Entity.Null) { if (t[i].Id == P0) atkP++; else if (t[i].Id == P1) atkA++; }

                var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<StatueState>());
                using (var tg = sq.ToComponentDataArray<StatueTag>(Allocator.Temp))
                using (var st = sq.ToComponentDataArray<StatueState>(Allocator.Temp))
                    for (int i = 0; i < tg.Length; i++) { if (tg[i].Team == P0) sP = st[i].Health; else if (tg[i].Team == P1) sA = st[i].Health; }
            }
            catch (System.Exception e) { Debug.LogWarning("[GATE1] sample error: " + e.Message); return; }

            // CSV-ish line: match, time, gold P0/AI, units P0/AI, miners P0/AI, engaged P0/AI, statueHP P0/AI, outcome
            Debug.Log($"[GATE1] m{_matchNo} t={_matchT:0} gP={gP} gA={gA} uP={uP} uA={uA} mP={mP} mA={mA} atkP={atkP} atkA={atkA} sP={sP:0} sA={sA:0} out={outcome}");
        }
    }
}
