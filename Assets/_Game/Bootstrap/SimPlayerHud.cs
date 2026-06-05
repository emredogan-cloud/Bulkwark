// BULWARK — SIM PLAYER HUD (Pre-Phase-5 GATE-1 viz track, PHASE V1 "Playability"). TEMPORARY, REMOVABLE,
// DEBUG-ONLY. This is the §12 CONTROL layer: a MonoBehaviour that translates player input (debug buttons)
// into sim DATA writes the ECS systems already consume — it adds NO gameplay rule and NO balance value.
//
// DELIVERS (V1 focus): gold display · unit-production buttons · train-queue display · player-side training ·
// an Advance (attack-move) command that makes player units march to the enemy statue → first observable combat.
//
// HOW IT WORKS (uses ONLY existing systems/contracts — no sim change):
//   • TRAIN: a button calls Training.EnqueueTrain(em, PlayerTeam, unitIndex) → appends a TrainOrder to the
//     player's TrainQueue; the existing TrainingSystem pays Gold + spawns the unit (data-driven cost/time).
//   • ADVANCE: sets MoveDestination{Value=enemyStatuePos, Active=1} on every player unit. The existing
//     MovementSystem drives any unit with Active==1 toward Value; TargetingSystem/CombatSystem still
//     auto-acquire + fight en route. This is the standard 1.3→1.4 control path (PossessControl/MovementSystem),
//     just issued from a button instead of a touch — an attack-move, not a new mechanic.
//   • PAUSE: toggles Time.timeScale (sim DeltaTime scales with it). Pure debug control.
//
// INVIOLABLE: writes only player INPUT data (TrainOrder queue add + MoveDestination override) — exactly what
// BattleInput.cs is permitted to write (§12). It changes NO sim rule, NO balance number, NO catalog. All ECS
// writes are deferred from OnGUI to Update (safe main-thread structural-change point). Deleting this one file
// removes it 100%. SCAFFOLD STATUS: authored here; CI compiles; device run produces the evidence.

using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Temporary debug HUD for player-side training + an advance command. Control layer (§12).</summary>
    public sealed class SimPlayerHud : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("SimPlayerHud");
            go.AddComponent<SimPlayerHud>();
            DontDestroyOnLoad(go);
            Debug.Log("[HUD] SimPlayerHud booted (AfterSceneLoad).");
        }

        private const int PlayerTeam = 0, AiTeam = 1;

        private struct Btn { public int Index; public RoleId Role; public int Cost; }
        private readonly List<Btn> _roster = new List<Btn>(8);
        private bool _rosterLoaded;

        // display snapshot (read in Update, drawn in OnGUI)
        private int _goldP = -1;
        private string _queueText = "";
        private int _unitsP, _unitsAI;

        // deferred requests (set in OnGUI, executed in Update)
        private int _trainReq = -1;
        private bool _advanceReq;
        private bool _pauseReq;
        private bool _paused;

        // DEBUG AUTO-DEMO: deterministically exercises the full train→push→combat loop a few seconds after
        // the world is ready, so first-observable-combat is reproducible on-device WITHOUT fragile tap
        // coordinates. It issues the SAME control writes the buttons do (EnqueueTrain + MoveDestination) — no
        // rule/balance change. The manual buttons remain fully functional (player-driven training). Debug-only.
        private bool _autoDemo = true;
        private float _worldReady0 = -1f;
        private bool _autoMinersQueued;
        private float _lastAutoTrain = -100f;
        private float _lastAutoAdvance = -100f;

        private World _w;
        private EntityManager _em;
        private bool _ready;

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
                LoadRoster();
                ReadState();
                // ---- execute deferred player input (safe main-thread point) ----
                if (_trainReq >= 0) { Training.EnqueueTrain(_em, PlayerTeam, _trainReq); Debug.Log($"[HUD] TRAIN requested unit {_trainReq} (player)."); _trainReq = -1; }
                if (_advanceReq) { int n = AdvanceAllPlayerUnits(); Debug.Log($"[HUD] ADVANCE -> {n} player units ordered to the enemy statue."); _advanceReq = false; }
                if (_pauseReq) { _paused = !_paused; Time.timeScale = _paused ? 0f : 1f; Debug.Log($"[HUD] PAUSE = {_paused}."); _pauseReq = false; }

                // ---- DEBUG AUTO-DEMO (V2): drive the FULL economy loop — mine → train → push — reproducibly,
                // using the SAME control writes the buttons do (EnqueueTrain + MoveDestination). No rule/balance change.
                if (_autoDemo && _rosterLoaded)
                {
                    if (_worldReady0 < 0f) _worldReady0 = Time.unscaledTime;
                    float el = Time.unscaledTime - _worldReady0;

                    // 1) ECONOMY: train 2 miners first → MiningSystem auto-assigns them to mines → gold income.
                    if (!_autoMinersQueued && el > 3f)
                    {
                        int mi = MinerIndex();
                        if (mi >= 0) { Training.EnqueueTrain(_em, PlayerTeam, mi); Training.EnqueueTrain(_em, PlayerTeam, mi); Debug.Log($"[HUD] AUTO-DEMO queued 2 miners (#{mi}) for economy."); }
                        else Debug.Log("[HUD] AUTO-DEMO: no miner role in roster.");
                        _autoMinersQueued = true;
                    }

                    // 2) ARMY: once income can flow, train the cheapest affordable combat unit on a cadence.
                    if (el > 8f && (Time.unscaledTime - _lastAutoTrain) > 4f)
                    {
                        int idx = CheapestAffordableCombat();
                        if (idx >= 0) { Training.EnqueueTrain(_em, PlayerTeam, idx); Debug.Log($"[HUD] AUTO-DEMO train combat #{idx} (gold {_goldP})."); }
                        _lastAutoTrain = Time.unscaledTime;
                    }

                    // 3) PUSH: advance all player units toward the enemy statue on a cadence (attack-move).
                    if (el > 14f && (Time.unscaledTime - _lastAutoAdvance) > 5f)
                    {
                        int n = AdvanceAllPlayerUnits();
                        if (n > 0) Debug.Log($"[HUD] AUTO-DEMO advance ({n} player units).");
                        _lastAutoAdvance = Time.unscaledTime;
                    }
                }
            }
            catch (System.Exception e) { Debug.LogError("[HUD] error: " + e.Message); }
        }

        /// <summary>Index of the cheapest NON-miner unit affordable at current gold, or -1. Read-only of the
        /// roster/gold the HUD already cached — picks an existing unit, invents no stat (§15).</summary>
        private int CheapestAffordableCombat()
        {
            int best = -1, bestCost = int.MaxValue;
            for (int i = 0; i < _roster.Count; i++)
            {
                var b = _roster[i];
                if (b.Role == RoleId.Miner) continue;
                if (b.Cost <= _goldP && b.Cost < bestCost) { bestCost = b.Cost; best = b.Index; }
            }
            return best;
        }

        /// <summary>Roster index of the Miner unit (for the economy auto-demo), or -1 if the roster has none.</summary>
        private int MinerIndex()
        {
            for (int i = 0; i < _roster.Count; i++) if (_roster[i].Role == RoleId.Miner) return _roster[i].Index;
            return -1;
        }

        /// <summary>Authored GoldCost for a roster unit index (read-only, for the stall display), or -1.</summary>
        private int CostOf(int unitIndex)
        {
            for (int i = 0; i < _roster.Count; i++) if (_roster[i].Index == unitIndex) return _roster[i].Cost;
            return -1;
        }

        private void LoadRoster()
        {
            if (_rosterLoaded) return;
            if (!UnitCatalog.TryGetForTeam(_em, PlayerTeam, out Entity cat)) return;
            if (!_em.HasBuffer<UnitSpawnStats>(cat)) return;
            var buf = _em.GetBuffer<UnitSpawnStats>(cat, true);
            _roster.Clear();
            for (int i = 0; i < buf.Length; i++)
                _roster.Add(new Btn { Index = i, Role = buf[i].Role, Cost = buf[i].GoldCost });
            _rosterLoaded = _roster.Count > 0;
            if (_rosterLoaded) Debug.Log($"[HUD] player roster loaded: {_roster.Count} units.");
        }

        private void ReadState()
        {
            // gold (player)
            _goldP = -1;
            using (var g = _em.CreateEntityQuery(ComponentType.ReadOnly<GoldStore>()).ToComponentDataArray<GoldStore>(Allocator.Temp))
                for (int i = 0; i < g.Length; i++) if (g[i].Team == PlayerTeam) _goldP = g[i].Amount;

            // unit counts per team
            _unitsP = 0; _unitsAI = 0;
            var uq = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Team>());
            using (var t = uq.ToComponentDataArray<Team>(Allocator.Temp))
                for (int i = 0; i < t.Length; i++) { if (t[i].Id == PlayerTeam) _unitsP++; else if (t[i].Id == AiTeam) _unitsAI++; }

            // player train queue
            var sb = new StringBuilder(64);
            var qq = _em.CreateEntityQuery(ComponentType.ReadOnly<TrainQueueTag>());
            using (var qents = qq.ToEntityArray(Allocator.Temp))
            using (var qtags = qq.ToComponentDataArray<TrainQueueTag>(Allocator.Temp))
                for (int i = 0; i < qents.Length; i++)
                {
                    if (qtags[i].Team != PlayerTeam) continue;
                    if (!_em.HasBuffer<TrainOrder>(qents[i])) continue;
                    var ords = _em.GetBuffer<TrainOrder>(qents[i], true);
                    sb.Append("queue[").Append(ords.Length).Append("]: ");
                    for (int k = 0; k < ords.Length && k < 6; k++)
                    {
                        sb.Append('#').Append(ords[k].UnitIndex);
                        int c = CostOf(ords[k].UnitIndex);
                        if (k == 0 && ords[k].Paid == 0 && c > _goldP) sb.Append("(STALL need ").Append(c).Append("g) ");
                        else sb.Append('(').Append(ords[k].Remaining <= 0f ? "wait" : ords[k].Remaining.ToString("0.0")).Append(") ");
                    }
                }
            _queueText = sb.ToString();
        }

        private int AdvanceAllPlayerUnits()
        {
            // enemy statue position (Team 1) = the attack-move objective
            float2 target = new float2(0f, 0f);
            bool haveTarget = false;
            var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<Position>());
            using (var stag = sq.ToComponentDataArray<StatueTag>(Allocator.Temp))
            using (var spos = sq.ToComponentDataArray<Position>(Allocator.Temp))
                for (int i = 0; i < stag.Length; i++) if (stag[i].Team == AiTeam) { target = spos[i].Value; haveTarget = true; }
            if (!haveTarget) return 0;

            int n = 0;
            var uq = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Team>());
            using (var ents = uq.ToEntityArray(Allocator.Temp))
            using (var team = uq.ToComponentDataArray<Team>(Allocator.Temp))
                for (int i = 0; i < ents.Length; i++)
                {
                    if (team[i].Id != PlayerTeam) continue;
                    if (_em.HasComponent<MinerTag>(ents[i])) continue; // keep miners on the mines (don't send them to die)
                    var dest = new MoveDestination { Value = target, Active = 1 };
                    if (_em.HasComponent<MoveDestination>(ents[i])) _em.SetComponentData(ents[i], dest);
                    else _em.AddComponentData(ents[i], dest);
                    n++;
                }
            return n;
        }

        // ---------------- IMGUI (left column, below the SimDebugOverlay panel) ----------------
        private GUIStyle _lbl, _btn, _bg;
        private Texture2D _bgTex;

        private void EnsureGui()
        {
            if (_btn != null) return;
            _bgTex = new Texture2D(1, 1); _bgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.66f)); _bgTex.Apply();
            _lbl = new GUIStyle { fontSize = 26, richText = true }; _lbl.normal.textColor = Color.white;
            _btn = new GUIStyle(GUI.skin.button) { fontSize = 26, fontStyle = FontStyle.Bold };
        }

        private void OnGUI()
        {
            EnsureGui();
            float w = Mathf.Min(470f, Screen.width * 0.46f);
            float x = 8f;
            float y = Mathf.Min(600f, Screen.height * 0.30f); // start below the overlay panel
            float bh = 64f, gap = 8f;

            GUI.DrawTexture(new Rect(x - 4, y - 4, w + 8, Screen.height - y - 4), _bgTex);

            GUI.Label(new Rect(x, y, w, 30), $"<b>PLAYER HUD (V1)</b>  Gold P0=<b>{_goldP}</b>  Units P{_unitsP}/AI{_unitsAI}", _lbl);
            y += 40f;

            if (!_ready) { GUI.Label(new Rect(x, y, w, 30), "world not ready…", _lbl); return; }
            if (_roster.Count == 0) { GUI.Label(new Rect(x, y, w, 30), "loading roster…", _lbl); return; }

            GUI.Label(new Rect(x, y, w, 26), "Train (spends gold; greyed = can't afford):", _lbl); y += 30f;
            for (int i = 0; i < _roster.Count; i++)
            {
                var b = _roster[i];
                GUI.enabled = b.Cost <= _goldP; // affordable-gating: can't enqueue an unaffordable head that would stall the queue
                if (GUI.Button(new Rect(x, y, w, bh), $"[{b.Index}] {b.Role}  —  {b.Cost}g", _btn)) _trainReq = b.Index;
                GUI.enabled = true;
                y += bh + gap;
            }

            y += 6f;
            if (GUI.Button(new Rect(x, y, w, bh + 10f), "ADVANCE →  (attack-move to enemy statue)", _btn)) _advanceReq = true;
            y += bh + 18f;
            if (GUI.Button(new Rect(x, y, w * 0.5f - 4f, bh), _paused ? "RESUME" : "PAUSE", _btn)) _pauseReq = true;
            y += bh + gap;

            GUI.Label(new Rect(x, y, w, 30), _queueText, _lbl);
        }
    }
}
