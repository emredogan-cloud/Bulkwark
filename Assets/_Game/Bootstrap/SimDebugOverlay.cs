// BULWARK — SIM DEBUG OVERLAY (TEMPORARY, REMOVABLE, DEBUG-ONLY observability).
//
// PURPOSE: convert simulation status from UNDETERMINED -> PROVEN RUNNING / PROVEN BROKEN by making the
// ECS battle observable, since the build has no render layer yet (no entities.graphics; no scene
// renderers/UI). It READS the default ECS World and reports state via (1) Debug.Log -> logcat, and
// (2) an IMGUI overlay + colored-box "radar" -> on-screen. Camera/URP/art-INDEPENDENT.
//
// INVIOLABLE (this file is debug tooling, NOT gameplay):
//   • READ-ONLY: it only queries/reads ECS components; it NEVER writes, adds, removes, or mutates any
//     sim state, and changes NO gameplay logic or balance (§15). Deleting this one file removes it 100%.
//   • Lives in Bulwark.Bootstrap (guaranteed in the build alongside BattleBootstrap) so it always runs.
//   • No save-state logging (§12): it logs only counts/scalars/enum names, never a profile/save payload.
//
// Proxies (STEP 4): Iron Pact/Team 0 = BLUE, Ashen Horde/Team 1 = RED, Mine = YELLOW, Statue = GRAY.
// SCAFFOLD STATUS: authored here; CI compiles; device run produces the evidence.

using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Temporary debug-only observability overlay. Auto-spawned; reads the ECS World read-only.</summary>
    public sealed class SimDebugOverlay : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("SimDebugOverlay");
            go.AddComponent<SimDebugOverlay>();
            DontDestroyOnLoad(go);
            Debug.Log("[SIMPROOF] SimDebugOverlay booted (AfterSceneLoad).");
        }

        // ---- snapshot of the current world state (rebuilt each frame; rendered by OnGUI) ----
        private struct Snap
        {
            public bool WorldOk;
            public string Error;
            public int MatchOutcome;   // -1 = no MatchState singleton; else 0 Ongoing /1 Victory /2 Defeat
            public int GoldP, GoldAI;  // -1 = no GoldStore for that team
            public int Units, UnitsP, UnitsAI, Alive;
            public int Miners, MinersAI, Mines, MineOccupants;
            public int StatueHpP, StatueHpAI, StatueMaxP, StatueMaxAI; // -1 = none
            public int AiStance;       // -1 none; else CommanderStance
            public int AiCount;
            public int QueueP, QueueAI;
            public int Engaged;        // units with a live AttackState.Target
            public float TotalHealth;
            public int Systems;
            // radar points (capped)
            public int PtCount;
            public float2[] Pts;
            public byte[] Kind;        // 0 unit-T0, 1 unit-T1, 2 mine, 3 statue
        }

        private Snap _s;
        private float _logTimer;
        private int _frame;
        private float _startTime;
        // deltas vs previous logged snapshot (proof of CHANGE over time)
        private int _prevAlive = int.MinValue, _prevGoldP = int.MinValue, _prevUnits = int.MinValue;
        private float _prevTotalHealth = float.NaN;
        private float _fps;

        // cached queries (rebuilt if the World changes)
        private World _w;
        private EntityManager _em;
        private EntityQuery _qMatch, _qGold, _qUnit, _qMiner, _qMine, _qStatue, _qAi, _qQueue, _qEngaged;
        private bool _queriesReady;

        private const int PlayerTeam = 0, AiTeam = 1, MaxPts = 600;

        private void Update()
        {
            _frame++;
            _startTime = _startTime == 0f ? Time.realtimeSinceStartup : _startTime;
            _fps = _fps <= 0f ? 1f / Mathf.Max(1e-4f, Time.unscaledDeltaTime)
                              : Mathf.Lerp(_fps, 1f / Mathf.Max(1e-4f, Time.unscaledDeltaTime), 0.1f);

            try { _s = ReadWorld(); }
            catch (System.Exception e) { _s = new Snap { WorldOk = false, Error = e.GetType().Name + ": " + e.Message }; }

            _logTimer += Time.unscaledDeltaTime;
            if (_logTimer >= 1.0f)
            {
                _logTimer = 0f;
                LogSnapshot();
            }
        }

        private bool EnsureQueries()
        {
            World w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) { _queriesReady = false; return false; }
            if (!_queriesReady || w != _w)
            {
                _w = w;
                _em = w.EntityManager;
                _qMatch   = _em.CreateEntityQuery(ComponentType.ReadOnly<MatchState>());
                _qGold    = _em.CreateEntityQuery(ComponentType.ReadOnly<GoldStore>());
                _qUnit    = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Position>(), ComponentType.ReadOnly<Team>(), ComponentType.ReadOnly<Health>());
                _qMiner   = _em.CreateEntityQuery(ComponentType.ReadOnly<MinerTag>(), ComponentType.ReadOnly<Team>());
                _qMine    = _em.CreateEntityQuery(ComponentType.ReadOnly<MineNode>(), ComponentType.ReadOnly<Position>());
                _qStatue  = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<StatueState>(), ComponentType.ReadOnly<Position>());
                _qAi      = _em.CreateEntityQuery(ComponentType.ReadOnly<AICommander>());
                _qQueue   = _em.CreateEntityQuery(ComponentType.ReadOnly<TrainQueueTag>());
                _qEngaged = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<AttackState>());
                _queriesReady = true;
            }
            return true;
        }

        private Snap ReadWorld()
        {
            var s = new Snap { MatchOutcome = -1, GoldP = -1, GoldAI = -1, StatueHpP = -1, StatueHpAI = -1, AiStance = -1, Pts = _s.Pts, Kind = _s.Kind };
            if (!EnsureQueries()) { s.WorldOk = false; s.Error = "No default ECS World"; return s; }
            s.WorldOk = true;
            using (var sysArr = _w.Unmanaged.GetAllUnmanagedSystems(Allocator.Temp)) s.Systems = sysArr.Length;

            // MatchState singleton
            using (var m = _qMatch.ToComponentDataArray<MatchState>(Allocator.Temp))
                if (m.Length > 0) s.MatchOutcome = (int)m[0].Outcome;

            // Gold per team
            using (var g = _qGold.ToComponentDataArray<GoldStore>(Allocator.Temp))
                for (int i = 0; i < g.Length; i++)
                {
                    if (g[i].Team == PlayerTeam) s.GoldP = g[i].Amount;
                    else if (g[i].Team == AiTeam) s.GoldAI = g[i].Amount;
                }

            // Units (pos+team+health) — counts, alive, total health, radar points
            if (s.Pts == null || s.Pts.Length < MaxPts) { s.Pts = new float2[MaxPts]; s.Kind = new byte[MaxPts]; }
            else if (s.Kind == null || s.Kind.Length < MaxPts) { s.Kind = new byte[MaxPts]; }
            int pc = 0;
            using (var pos = _qUnit.ToComponentDataArray<Position>(Allocator.Temp))
            using (var team = _qUnit.ToComponentDataArray<Team>(Allocator.Temp))
            using (var hp = _qUnit.ToComponentDataArray<Health>(Allocator.Temp))
            {
                s.Units = pos.Length;
                for (int i = 0; i < pos.Length; i++)
                {
                    bool p = team[i].Id == PlayerTeam;
                    if (p) s.UnitsP++; else s.UnitsAI++;
                    if (hp[i].Current > 0f) s.Alive++;
                    s.TotalHealth += hp[i].Current;
                    if (pc < MaxPts) { s.Pts[pc] = pos[i].Value; s.Kind[pc] = (byte)(p ? 0 : 1); pc++; }
                }
            }

            using (var mt = _qMiner.ToComponentDataArray<Team>(Allocator.Temp))
            { s.Miners = mt.Length; for (int i = 0; i < mt.Length; i++) if (mt[i].Id == AiTeam) s.MinersAI++; }

            using (var mn = _qMine.ToComponentDataArray<MineNode>(Allocator.Temp))
            using (var mp = _qMine.ToComponentDataArray<Position>(Allocator.Temp))
            {
                s.Mines = mn.Length;
                for (int i = 0; i < mn.Length; i++)
                {
                    s.MineOccupants += mn[i].Occupants;
                    if (pc < MaxPts) { s.Pts[pc] = mp[i].Value; s.Kind[pc] = 2; pc++; }
                }
            }

            using (var stag = _qStatue.ToComponentDataArray<StatueTag>(Allocator.Temp))
            using (var sst = _qStatue.ToComponentDataArray<StatueState>(Allocator.Temp))
            using (var sp = _qStatue.ToComponentDataArray<Position>(Allocator.Temp))
                for (int i = 0; i < stag.Length; i++)
                {
                    if (stag[i].Team == PlayerTeam) { s.StatueHpP = (int)sst[i].Health; s.StatueMaxP = (int)sst[i].MaxHealth; }
                    else if (stag[i].Team == AiTeam) { s.StatueHpAI = (int)sst[i].Health; s.StatueMaxAI = (int)sst[i].MaxHealth; }
                    if (pc < MaxPts) { s.Pts[pc] = sp[i].Value; s.Kind[pc] = 3; pc++; }
                }

            s.AiCount = _qAi.CalculateEntityCount();
            using (var ai = _qAi.ToComponentDataArray<AICommander>(Allocator.Temp))
                if (ai.Length > 0) s.AiStance = (int)ai[0].Stance;

            using (var qents = _qQueue.ToEntityArray(Allocator.Temp))
            using (var qtags = _qQueue.ToComponentDataArray<TrainQueueTag>(Allocator.Temp))
                for (int i = 0; i < qents.Length; i++)
                {
                    int len = _em.HasBuffer<TrainOrder>(qents[i]) ? _em.GetBuffer<TrainOrder>(qents[i], true).Length : 0;
                    if (qtags[i].Team == PlayerTeam) s.QueueP = len; else if (qtags[i].Team == AiTeam) s.QueueAI = len;
                }

            using (var atk = _qEngaged.ToComponentDataArray<AttackState>(Allocator.Temp))
                for (int i = 0; i < atk.Length; i++)
                    if (atk[i].Target != Entity.Null) s.Engaged++;

            s.PtCount = pc;
            return s;
        }

        private static string OutcomeName(int o) => o == 0 ? "Ongoing" : o == 1 ? "Victory" : o == 2 ? "Defeat" : "NONE";
        private static string StanceName(int st) => st == 0 ? "Defend" : st == 1 ? "Push" : st == 2 ? "Harass" : "NONE";

        private void LogSnapshot()
        {
            if (!_s.WorldOk) { Debug.Log($"[SIMPROOF] t={Now():0.0}s frame={_frame} WORLD-NOT-READY: {_s.Error}"); return; }
            int dAlive = _prevAlive == int.MinValue ? 0 : _s.Alive - _prevAlive;
            int dGoldP = _prevGoldP == int.MinValue ? 0 : _s.GoldP - _prevGoldP;
            int dUnits = _prevUnits == int.MinValue ? 0 : _s.Units - _prevUnits;
            float dHp = float.IsNaN(_prevTotalHealth) ? 0f : _s.TotalHealth - _prevTotalHealth;
            Debug.Log(
                $"[SIMPROOF] t={Now():0.0}s f={_frame} fps={_fps:0} sys={_s.Systems} " +
                $"Match={OutcomeName(_s.MatchOutcome)} GoldP={_s.GoldP} GoldAI={_s.GoldAI} " +
                $"Units={_s.Units}(P{_s.UnitsP}/AI{_s.UnitsAI}) Alive={_s.Alive} Miners={_s.Miners}(AI{_s.MinersAI}) " +
                $"Mines={_s.Mines}(occ{_s.MineOccupants}) StatueP={_s.StatueHpP}/{_s.StatueMaxP} StatueAI={_s.StatueHpAI}/{_s.StatueMaxAI} " +
                $"AI={StanceName(_s.AiStance)}(x{_s.AiCount}) QueueP={_s.QueueP} QueueAI={_s.QueueAI} Engaged={_s.Engaged} " +
                $"dUnits={dUnits} dAlive={dAlive} dGoldP={dGoldP} dHealth={dHp:0.0}");
            _prevAlive = _s.Alive; _prevGoldP = _s.GoldP; _prevUnits = _s.Units; _prevTotalHealth = _s.TotalHealth;
        }

        private float Now() => Time.realtimeSinceStartup - _startTime;

        // ---------------- IMGUI ----------------
        private GUIStyle _box, _lbl;
        private Texture2D _texBlue, _texRed, _texYellow, _texGray, _texBg;

        private static Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t;
        }

        private void EnsureGui()
        {
            if (_texBlue != null) return;
            _texBlue = Solid(new Color(0.25f, 0.5f, 1f)); _texRed = Solid(new Color(1f, 0.3f, 0.25f));
            _texYellow = Solid(new Color(1f, 0.85f, 0.1f)); _texGray = Solid(new Color(0.7f, 0.7f, 0.72f));
            _texBg = Solid(new Color(0f, 0f, 0f, 0.62f));
            _lbl = new GUIStyle { fontSize = 26, richText = true };
            _lbl.normal.textColor = Color.white;
            _box = new GUIStyle { fontSize = 30, fontStyle = FontStyle.Bold };
            _box.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            if (PresentationState.SuppressDebugHud) return; // hidden while a uGUI menu/end screen is up
            EnsureGui();

            // ---- stats panel (top-left) ----
            float pw = Mathf.Min(560f, Screen.width * 0.5f), ph = 560f;
            GUI.DrawTexture(new Rect(8, 8, pw, ph), _texBg);
            var sb = new StringBuilder(512);
            sb.AppendLine("<b>BULWARK SIM PROOF (debug overlay)</b>");
            if (!_s.WorldOk) { sb.AppendLine("WORLD: <color=#ff6>NOT READY</color>"); sb.AppendLine(_s.Error); }
            else
            {
                int dAlive = _prevAlive == int.MinValue ? 0 : _s.Alive - _prevAlive;
                sb.AppendLine($"t={Now():0.0}s  fps={_fps:0}  frame={_frame}  systems={_s.Systems}");
                sb.AppendLine($"MatchState: <b>{OutcomeName(_s.MatchOutcome)}</b>");
                sb.AppendLine($"Gold  P0={_s.GoldP}   AI={_s.GoldAI}");
                sb.AppendLine($"Units {_s.Units}  (P0={_s.UnitsP} / AI={_s.UnitsAI})  alive={_s.Alive}");
                sb.AppendLine($"Miners={_s.Miners} (AI {_s.MinersAI})   Mines={_s.Mines} (occ {_s.MineOccupants})");
                sb.AppendLine($"Statue P0={_s.StatueHpP}/{_s.StatueMaxP}   AI={_s.StatueHpAI}/{_s.StatueMaxAI}");
                sb.AppendLine($"AI stance: {StanceName(_s.AiStance)} (x{_s.AiCount})");
                sb.AppendLine($"Train queue  P0={_s.QueueP}  AI={_s.QueueAI}");
                sb.AppendLine($"Engaged(combat)={_s.Engaged}  totalHP={_s.TotalHealth:0}");
                sb.AppendLine($"Δalive/s={dAlive}  (spawns/deaths -> sim advancing)");
                sb.AppendLine("proxies: <color=#7af>P0 blue</color> <color=#f55>AI red</color> <color=#fd1>mine</color> <color=#bbb>statue</color>");
            }
            GUI.Label(new Rect(20, 18, pw - 24, ph - 16), sb.ToString(), _lbl);

            // ---- radar (right) : self-projected colored boxes (no camera/URP needed) ----
            if (!_s.WorldOk || _s.PtCount <= 0) return;
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < _s.PtCount; i++)
            {
                float2 p = _s.Pts[i];
                if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y; if (p.y > maxY) maxY = p.y;
            }
            float spanX = Mathf.Max(1f, maxX - minX), spanY = Mathf.Max(1f, maxY - minY);
            Rect radar = new Rect(Screen.width * 0.42f, 8, Screen.width * 0.56f, Screen.height - 16);
            GUI.DrawTexture(radar, _texBg);
            for (int i = 0; i < _s.PtCount; i++)
            {
                float2 p = _s.Pts[i];
                float nx = (p.x - minX) / spanX, ny = (p.y - minY) / spanY;
                float sx = radar.x + nx * (radar.width - 28f) + 8f;
                float sy = radar.y + (1f - ny) * (radar.height - 28f) + 8f;
                byte k = _s.Kind[i];
                Texture2D t = k == 0 ? _texBlue : k == 1 ? _texRed : k == 2 ? _texYellow : _texGray;
                float sz = k == 3 ? 26f : k == 2 ? 18f : 12f;
                GUI.DrawTexture(new Rect(sx - sz * 0.5f, sy - sz * 0.5f, sz, sz), t);
            }
        }
    }
}
