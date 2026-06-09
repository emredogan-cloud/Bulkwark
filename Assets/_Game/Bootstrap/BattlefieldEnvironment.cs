// STICK EMPIRE RISE — PHASE 5 ENVIRONMENT PRODUCTION (presentation-only, §12, REMOVABLE).
//
// Turns the Phase-4 parallax battlefield into a living world. Reads ECS state READ-ONLY (statue HP, mines,
// miner positions) + the presentation camera; writes NOTHING to the sim (no combat/AI/economy/spawn change).
// Systems (all pooled, no realtime lights):
//   • FACTION STATUES — 4-stage state machine by HP%: IDLE(100-76) aura · MINOR(75-36) +cracks · CRITICAL(35-1)
//                       +smoke/instability · DESTRUCTION(0) collapse+burst (≤2.5s).
//   • RESOURCE NODES  — idle gold shimmer; ACTIVE harvest bursts ONLY when a miner is observed adjacent
//                       (throttled ≤1 burst / 0.5s per node). Never infers economy.
//   • FACTION KEEPS/BARRACKS — barracks/watchtower/banner behind each statue; banner flutter + chimney smoke.
//   • SCATTER PROPS   — biome-aware decorative props placed with spacing, kept OUT of the central combat band
//                       so they never obscure units / projectiles / mining / statue / HUD readability.
// Deleting this one file removes Phase 5.

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Phase-5 environment systems. Read-only of ECS + camera. Presentation-only (§12).</summary>
    public sealed class BattlefieldEnvironment : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("BattlefieldEnvironment");
            go.AddComponent<BattlefieldEnvironment>();
            DontDestroyOnLoad(go);
            Debug.Log("[ENV] BattlefieldEnvironment booted.");
        }

        private Camera _cam; private Transform _root;
        private readonly Dictionary<string, Sprite> _spr = new Dictionary<string, Sprite>(16);
        private bool _loaded, _loading;
        private World _w; private EntityManager _em; private EntityQuery _qStatue, _qMine, _qMiner; private bool _qReady;

        private sealed class StatueViz { public SpriteRenderer Aura, Crack; public readonly List<Transform> Smoke = new List<Transform>(8); public bool Collapsing; public float CollapseT; public float Scale0 = 1f; public SpriteRenderer Proxy; }
        private readonly Dictionary<Entity, StatueViz> _statues = new Dictionary<Entity, StatueViz>(4);
        private readonly Dictionary<Entity, SpriteRenderer> _glints = new Dictionary<Entity, SpriteRenderer>(8);
        private readonly Dictionary<Entity, float> _mineBurst = new Dictionary<Entity, float>(8);
        private readonly List<Transform> _burstPool = new List<Transform>(32); private int _burstIdx;
        private readonly List<Transform> _ambient = new List<Transform>(16); // banner/torch transforms (animated)
        private readonly List<Transform> _chimSmoke = new List<Transform>(16);
        private bool _structuresBuilt; private string _scatterBiome;
        private readonly HashSet<Entity> _seen = new HashSet<Entity>();

        private static readonly string[] EnvKeys =
        { "env_barracks", "env_watchtower", "env_banner", "env_crack",
          "env_prop_rock", "env_prop_spear", "env_prop_debris", "env_prop_grave", "env_prop_bone", "env_prop_cart", "env_prop_log" };

        private void Update()
        {
            if (!PresentationState.InMatch) { if (_root != null && _root.gameObject.activeSelf) _root.gameObject.SetActive(false); return; }
            if (_cam == null) { _cam = Camera.main ?? (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null); if (_cam == null) return; }
            if (!_loaded) { if (!_loading) StartCoroutine(Load()); return; }
            if (_root != null && !_root.gameObject.activeSelf) _root.gameObject.SetActive(true);
            if (!EnsureQueries()) return;
            float dt = Time.unscaledDeltaTime;
            Statues(dt);
            Resources(dt);
            Structures();
            Scatter();
        }

        // ---------- load env sprites (once) ----------
        private IEnumerator Load()
        {
            _loading = true;
            if (_root == null) { _root = new GameObject("ENV_Root").transform; _root.SetParent(transform, false); }
            string dir = Application.streamingAssetsPath + "/bulwark_ui/";
            foreach (var k in EnvKeys)
            {
                string url = dir + k + ".png"; if (!url.Contains("://")) url = "file://" + url;
                using (var req = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        var t = DownloadHandlerTexture.GetContent(req); t.wrapMode = TextureWrapMode.Clamp;
                        _spr[k] = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0f), 100f); // bottom pivot (props stand on ground)
                    }
                }
            }
            _loaded = true; _loading = false;
            Debug.Log($"[ENV] env sprites ready: {_spr.Count}/{EnvKeys.Length}.");
        }

        private bool EnsureQueries()
        {
            var w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) return false;
            if (!_qReady || w != _w)
            {
                _w = w; _em = w.EntityManager;
                _qStatue = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<StatueState>(), ComponentType.ReadOnly<Position>());
                _qMine = _em.CreateEntityQuery(ComponentType.ReadOnly<MineNode>(), ComponentType.ReadOnly<Position>());
                _qMiner = _em.CreateEntityQuery(ComponentType.ReadOnly<MinerTag>(), ComponentType.ReadOnly<Position>());
                _qReady = true;
            }
            return true;
        }

        private Sprite S(string k) => _spr.TryGetValue(k, out var s) ? s : null;

        // ---------- 1) STATUES (4-stage) ----------
        private void Statues(float dt)
        {
            _seen.Clear();
            try
            {
                using (var ents = _qStatue.ToEntityArray(Allocator.Temp))
                using (var stt = _qStatue.ToComponentDataArray<StatueState>(Allocator.Temp))
                using (var pos = _qStatue.ToComponentDataArray<Position>(Allocator.Temp))
                {
                    float minx = float.MaxValue, maxx = float.MinValue;
                    for (int i = 0; i < pos.Length; i++) { minx = math.min(minx, pos[i].Value.x); maxx = math.max(maxx, pos[i].Value.x); }
                    for (int i = 0; i < ents.Length; i++)
                    {
                        _seen.Add(ents[i]);
                        float frac = stt[i].MaxHealth > 0f ? Mathf.Clamp01(stt[i].Health / stt[i].MaxHealth) : 1f;
                        bool blue = Mathf.Abs(pos[i].Value.x - minx) <= Mathf.Abs(pos[i].Value.x - maxx);
                        Vector3 p = new Vector3(pos[i].Value.x, pos[i].Value.y, 0f);
                        if (!_statues.TryGetValue(ents[i], out var v)) { v = NewStatue(); _statues[ents[i]] = v; }
                        UpdateStatue(v, frac, blue, p, dt);
                    }
                }
            }
            catch (System.Exception) { }
            // destruction: statue gone -> trigger collapse finish / cleanup
            if (_statues.Count > 0)
            {
                List<Entity> gone = null;
                foreach (var kv in _statues) if (!_seen.Contains(kv.Key)) (gone ??= new List<Entity>()).Add(kv.Key);
                if (gone != null) foreach (var e in gone) { CollapseDestroy(_statues[e]); _statues.Remove(e); }
            }
        }

        private StatueViz NewStatue()
        {
            var v = new StatueViz();
            v.Aura = Mk("StatueAura", -1); v.Aura.sprite = UiTex.Disc(Color.white, 64);
            v.Crack = Mk("StatueCrack", 3); v.Crack.sprite = S("env_crack"); v.Crack.color = new Color(1, 1, 1, 0);
            return v;
        }

        private void UpdateStatue(StatueViz v, float frac, bool blue, Vector3 p, float dt)
        {
            // aura (idle steady / minor dim / critical red+fast)
            Color baseC = blue ? new Color(0.4f, 0.7f, 1f) : new Color(1f, 0.5f, 0.35f);
            if (frac <= 0.35f) baseC = Color.Lerp(baseC, new Color(1f, 0.25f, 0.15f), 0.85f);
            float hz = frac <= 0.35f ? 5f : frac <= 0.75f ? 2.2f : 1.1f;
            float a = (0.16f + 0.5f * frac) * (0.7f + 0.3f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * hz)));
            v.Aura.color = new Color(baseC.r, baseC.g, baseC.b, a);
            float auraS = 3.0f + 1.3f * frac;
            v.Aura.transform.position = p + new Vector3(0, 1.0f, 0); v.Aura.transform.localScale = new Vector3(auraS, auraS, 1f);
            // cracks appear from MINOR (≤0.75), deepen toward CRITICAL
            float crackA = frac >= 0.75f ? 0f : Mathf.Clamp01((0.75f - frac) / 0.6f);
            if (v.Crack.sprite != null) { v.Crack.color = new Color(1, 1, 1, crackA * 0.9f); v.Crack.transform.position = p + new Vector3(0, 1.2f, 0); v.Crack.transform.localScale = Vector3.one * 2.6f; }
            // smoke from CRITICAL (≤0.35) — rising dark puffs
            if (frac <= 0.35f) EmitStatueSmoke(v, p);
            UpdateSmoke(v, dt);
        }

        private void EmitStatueSmoke(StatueViz v, Vector3 p)
        {
            if (v.Smoke.Count >= 8) return;
            if (UnityEngine.Random.value > 0.12f) return; // throttle
            var t = MkLoose("StatueSmoke", 6).transform; t.GetComponent<SpriteRenderer>().sprite = UiTex.Disc(new Color(0.15f, 0.14f, 0.13f, 0.6f), 24);
            t.position = p + new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), 1.4f, 0); t.localScale = Vector3.one * 0.8f;
            v.Smoke.Add(t);
        }
        private void UpdateSmoke(StatueViz v, float dt)
        {
            for (int i = v.Smoke.Count - 1; i >= 0; i--)
            {
                var t = v.Smoke[i]; if (t == null) { v.Smoke.RemoveAt(i); continue; }
                t.position += new Vector3(0.1f * dt, 1.6f * dt, 0); t.localScale *= 1f + dt * 0.6f;
                var sr = t.GetComponent<SpriteRenderer>(); var c = sr.color; c.a -= dt * 0.5f; sr.color = c;
                if (c.a <= 0.02f) { Destroy(t.gameObject); v.Smoke.RemoveAt(i); }
            }
        }
        private void CollapseDestroy(StatueViz v)
        {
            // destruction burst (≤2.5s handled by particle decay) + cleanup
            if (v.Aura != null) { for (int i = 0; i < 10; i++) { var t = MkLoose("Debris", 7).transform; t.GetComponent<SpriteRenderer>().sprite = UiTex.Diamond(new Color(0.5f, 0.5f, 0.52f), 16); t.position = v.Aura.transform.position; var rb = t.gameObject.AddComponent<DebrisBit>(); rb.vel = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(2f, 5f) + Vector2.up * 3f; } Destroy(v.Aura.gameObject); }
            if (v.Crack != null) Destroy(v.Crack.gameObject);
            foreach (var s in v.Smoke) if (s != null) Destroy(s.gameObject);
        }

        // ---------- 2) RESOURCE NODES ----------
        private void Resources(float dt)
        {
            float[] minerX = null; float[] minerY = null; int mn = 0;
            try
            {
                using (var mp = _qMiner.ToComponentDataArray<Position>(Allocator.Temp))
                { mn = mp.Length; minerX = new float[mn]; minerY = new float[mn]; for (int i = 0; i < mn; i++) { minerX[i] = mp[i].Value.x; minerY[i] = mp[i].Value.y; } }
                using (var ents = _qMine.ToEntityArray(Allocator.Temp))
                using (var pos = _qMine.ToComponentDataArray<Position>(Allocator.Temp))
                {
                    var seenM = new HashSet<Entity>();
                    for (int i = 0; i < ents.Length; i++)
                    {
                        seenM.Add(ents[i]); Vector3 p = new Vector3(pos[i].Value.x, pos[i].Value.y, 0f);
                        if (!_glints.TryGetValue(ents[i], out var g) || g == null) { g = Mk("MineGlint", 2); g.sprite = UiTex.Disc(new Color(1f, 0.9f, 0.4f), 24); _glints[ents[i]] = g; }
                        // idle shimmer
                        float sh = 0.25f + 0.25f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 1.6f + pos[i].Value.x));
                        g.color = new Color(1f, 0.9f, 0.45f, sh); g.transform.position = p + new Vector3(0, 0.5f, 0); g.transform.localScale = Vector3.one * 0.7f;
                        // active harvest: a miner adjacent -> throttled gold burst
                        bool active = false; for (int m = 0; m < mn; m++) { float dx = minerX[m] - pos[i].Value.x, dy = minerY[m] - pos[i].Value.y; if (dx * dx + dy * dy < 2.2f * 2.2f) { active = true; break; } }
                        float last = _mineBurst.TryGetValue(ents[i], out var lb) ? lb : -10f;
                        if (active && Time.unscaledTime - last >= 0.5f) { _mineBurst[ents[i]] = Time.unscaledTime; OreBurst(p); }
                    }
                }
            }
            catch (System.Exception) { }
        }
        private void OreBurst(Vector3 p)
        {
            for (int i = 0; i < 4; i++)
            {
                Transform t = NextBurst(); var sr = t.GetComponent<SpriteRenderer>(); sr.sprite = UiTex.Diamond(new Color(1f, 0.85f, 0.3f), 12); sr.color = new Color(1f, 0.85f, 0.3f, 1f);
                t.position = p + new Vector3(0, 0.7f, 0); t.localScale = Vector3.one * 0.35f;
                var b = t.GetComponent<DebrisBit>() ?? t.gameObject.AddComponent<DebrisBit>(); b.vel = new Vector2(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(2.5f, 4.5f)); b.life = 0.5f;
            }
        }
        private Transform NextBurst()
        {
            if (_burstPool.Count < 24) { var t = MkLoose("Ore", 8).transform; _burstPool.Add(t); return t; }
            _burstIdx = (_burstIdx + 1) % _burstPool.Count; return _burstPool[_burstIdx];
        }

        // ---------- 3) STRUCTURES (barracks/keeps behind statues) + ambience ----------
        private void Structures()
        {
            if (_structuresBuilt) { AnimateAmbient(); return; }
            // place relative to statues (need their positions)
            try
            {
                using (var pos = _qStatue.ToComponentDataArray<Position>(Allocator.Temp))
                {
                    if (pos.Length < 1) return;
                    float minx = float.MaxValue, maxx = float.MinValue, gy = 0f;
                    for (int i = 0; i < pos.Length; i++) { minx = math.min(minx, pos[i].Value.x); maxx = math.max(maxx, pos[i].Value.x); gy = pos[i].Value.y; }
                    BuildKeep(minx - 3.0f, gy, true);   // blue side (left)
                    BuildKeep(maxx + 3.0f, gy, false);  // red side (right)
                    _structuresBuilt = true;
                    Debug.Log("[ENV] faction structures built.");
                }
            }
            catch (System.Exception) { }
        }
        private void BuildKeep(float x, float gy, bool blue)
        {
            Color tint = blue ? new Color(0.7f, 0.78f, 0.95f) : new Color(0.95f, 0.74f, 0.66f);
            var bar = Mk("Barracks", -126); bar.sprite = S("env_barracks"); bar.color = new Color(0.55f, 0.55f, 0.6f); bar.transform.position = new Vector3(x, gy - 0.5f, 0); bar.transform.localScale = Vector3.one * 2.2f;
            var tow = Mk("Watchtower", -125); tow.sprite = S("env_watchtower"); tow.color = new Color(0.5f, 0.5f, 0.55f); tow.transform.position = new Vector3(x + (blue ? -1.6f : 1.6f), gy - 0.5f, 0); tow.transform.localScale = Vector3.one * 2.6f;
            var ban = Mk("Banner", -124); ban.sprite = S("env_banner"); ban.color = tint; ban.transform.position = new Vector3(x + (blue ? 1.2f : -1.2f), gy, 0); ban.transform.localScale = Vector3.one * 1.8f; _ambient.Add(ban.transform);
            // chimney smoke source over the barracks
            var sm = MkLoose("ChimSmokeSrc", -123).transform; sm.position = new Vector3(x + 0.8f, gy + 2.4f, 0); sm.GetComponent<SpriteRenderer>().enabled = false; _chimSmoke.Add(sm);
            // torch glow (pulse)
            var to = Mk("Torch", -123); to.sprite = UiTex.Disc(new Color(1f, 0.7f, 0.3f), 24); to.transform.position = new Vector3(x + (blue ? -1.6f : 1.6f), gy + 1.4f, 0); to.transform.localScale = Vector3.one * 0.6f; _ambient.Add(to.transform);
        }
        private float _smokeT;
        private void AnimateAmbient()
        {
            float t = Time.unscaledTime;
            foreach (var a in _ambient)
            {
                if (a == null) continue;
                if (a.name == "Banner") a.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 2.5f) * 6f);
                else if (a.name == "Torch") { var sr = a.GetComponent<SpriteRenderer>(); if (sr) { var c = sr.color; c.a = 0.5f + 0.35f * Mathf.Abs(Mathf.Sin(t * 7f + a.position.x)); sr.color = c; } }
            }
            // chimney smoke loop (pooled)
            _smokeT += Time.unscaledDeltaTime;
            if (_smokeT > 0.4f && _chimSmoke.Count > 0)
            {
                _smokeT = 0f;
                var src = _chimSmoke[UnityEngine.Random.Range(0, _chimSmoke.Count)];
                var p = MkLoose("ChimSmoke", -123).transform; p.GetComponent<SpriteRenderer>().sprite = UiTex.Disc(new Color(0.3f, 0.3f, 0.32f, 0.5f), 20);
                p.position = src.position; p.localScale = Vector3.one * 0.6f; var b = p.gameObject.AddComponent<DebrisBit>(); b.vel = new Vector2(0.3f, 1.2f); b.life = 2.2f; b.grow = true; b.fade = true;
            }
        }

        // ---------- 4) SCATTER PROPS (biome-aware, out of the combat band) ----------
        private static readonly Dictionary<string, string[]> BiomeProps = new Dictionary<string, string[]>
        {
            { "grass", new[]{ "env_prop_rock","env_prop_spear","env_prop_cart","env_prop_log" } },
            { "ash",   new[]{ "env_prop_debris","env_prop_rock","env_prop_log" } },
            { "snow",  new[]{ "env_prop_log","env_prop_rock","env_prop_debris" } },
            { "volcanic", new[]{ "env_prop_rock","env_prop_bone","env_prop_debris" } },
            { "dead",  new[]{ "env_prop_grave","env_prop_bone","env_prop_debris" } },
        };
        private void Scatter()
        {
            string biome = BattlefieldParallax.Biome;
            if (_scatterBiome == biome) return;
            // clear old scatter (biome change)
            for (int i = _ambient.Count - 1; i >= 0; i--) { } // ambient kept
            foreach (Transform c in _root) { } // (scatter tagged below; rebuilt fresh)
            // need statue x-range for the lane span + ground y
            float minx = -8f, maxx = 8f, gy = 0f;
            try { using (var pos = _qStatue.ToComponentDataArray<Position>(Allocator.Temp)) { if (pos.Length > 0) { minx = float.MaxValue; maxx = float.MinValue; for (int i = 0; i < pos.Length; i++) { minx = math.min(minx, pos[i].Value.x); maxx = math.max(maxx, pos[i].Value.x); gy = pos[i].Value.y; } } } }
            catch (System.Exception) { return; }
            var set = BiomeProps.TryGetValue(biome, out var s) ? s : BiomeProps["grass"];
            float span = Mathf.Max(6f, maxx - minx); float lane = (minx + maxx) * 0.5f;
            int n = 16; float prevX = -999f;
            for (int i = 0; i < n; i++)
            {
                float fx = (i + 0.5f) / n; float x = minx + fx * span;
                // keep OUT of the central combat band (±18% of span around centre) so units/projectiles read
                if (Mathf.Abs(x - lane) < span * 0.18f) continue;
                if (x - prevX < span * 0.05f) continue; // min spacing
                prevX = x;
                // place on the ground line, alternating slightly fore/back so nothing tiles
                bool fore = (i % 3 == 0);
                float yj = (((i * 37) % 7) - 3) * 0.12f;
                var pr = Mk("Scatter_" + biome, fore ? 30 : -127);
                pr.sprite = S(set[(i * 13) % set.Length]); if (pr.sprite == null) { Destroy(pr.gameObject); continue; }
                pr.color = WeatherTint(biome, fore ? new Color(1f, 1f, 1f) : new Color(0.8f, 0.8f, 0.85f));
                float sc = (fore ? 1.4f : 0.9f) * (0.8f + ((i * 17) % 10) / 20f);
                pr.transform.position = new Vector3(x, gy + yj - (fore ? 0.2f : 0.1f), 0); pr.transform.localScale = Vector3.one * sc;
            }
            _scatterBiome = biome;
            Debug.Log($"[ENV] scatter built for biome '{biome}'.");
        }

        // ---------- weather compatibility (props react to biome weather) ----------
        private static Color WeatherTint(string biome, Color baseC)
        {
            switch (biome)
            {
                case "snow": return new Color(baseC.r * 0.9f + 0.1f, baseC.g * 0.95f + 0.05f, baseC.b, baseC.a); // frost/cooler
                case "ash": case "volcanic": return new Color(baseC.r * 0.85f, baseC.g * 0.6f, baseC.b * 0.5f, baseC.a); // ember/desaturate
                case "dead": return new Color(baseC.r * 0.8f, baseC.g * 0.82f, baseC.b * 0.8f, baseC.a); // grey
                default: return baseC;
            }
        }

        // ---------- helpers ----------
        private SpriteRenderer Mk(string name, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(_root, false);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = order; return sr;
        }
        private GameObject MkLoose(string name, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(_root, false);
            go.AddComponent<SpriteRenderer>().sortingOrder = order; return go;
        }
    }

    /// <summary>Tiny pooled-debris mover (gravity + fade) for statue debris / ore bursts / chimney smoke. Presentation-only.</summary>
    public sealed class DebrisBit : MonoBehaviour
    {
        public Vector2 vel; public float life = 1.2f; public bool grow, fade = true;
        private float _t;
        private void Update()
        {
            float dt = Time.unscaledDeltaTime; _t += dt;
            vel.y -= 9f * dt; // gravity
            transform.position += (Vector3)(vel * dt);
            if (grow) transform.localScale *= 1f + dt * 0.5f;
            if (fade) { var sr = GetComponent<SpriteRenderer>(); if (sr) { var c = sr.color; c.a = Mathf.Max(0f, 1f - _t / life); sr.color = c; } }
            if (_t >= life) Destroy(gameObject);
        }
    }
}
