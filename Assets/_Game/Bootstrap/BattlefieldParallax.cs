// STICK EMPIRE RISE — PHASE 4 BATTLEFIELD PARALLAX (presentation-only, §12, REMOVABLE).
//
// Pure 2D SIDE-VIEW PARALLAX LANE. Reads ONLY the presentation camera (which SimProxyRenderer frames from the
// ECS bounds) and the ECS statue state READ-ONLY — it never writes ECS, never touches sim/AI/economy/balance.
// It composites the biome layer sprites (bf_<biome>_*) around the existing unit proxies (the PLAYFIELD, z=0):
//
//   sortingOrder  layer
//   -140          L1 SKY        (parallax 1.00 — appears static/distant)
//   -136          L2 HORIZON    (parallax 0.82 — distant silhouettes)
//   -132          L3 MIDGROUND  (parallax 0.55 — hills/keeps; statue aura sits here)
//   -128          L4 GROUND     (parallax 0.00 — world-locked; units stand on it)
//   [0..2]        UNITS/MINES/STATUES  (SimProxyRenderer)
//   +40           L5 FOREGROUND (parallax -0.30 — near props, slight blur)
//   +60           L6 FX / WEATHER (pooled additive)
//
// No realtime lights / shadows / post (Phase-4 atmospheric rule): mood is painted into the sprites + an additive
// fullscreen tint. Disables SimProxyRenderer's flat battlefield_bg. Deleting this one file removes Phase 4.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulwark.Bootstrap
{
    /// <summary>2D side-view parallax battlefield presentation. Read-only of camera + ECS statues. §12-safe.</summary>
    public sealed class BattlefieldParallax : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("BattlefieldParallax");
            go.AddComponent<BattlefieldParallax>();
            DontDestroyOnLoad(go);
            Debug.Log("[BFX] BattlefieldParallax booted.");
        }

        // --- biome (default Grasslands; battle modes can set this via SetBiome before/at match start) ---
        public static string Biome = "grass";

        private struct Layer { public SpriteRenderer Sr; public float Parallax; public float Anchor; public int Order; }
        private Layer _sky, _horizon, _mid, _ground, _fg;
        private readonly Dictionary<string, Sprite> _spr = new Dictionary<string, Sprite>(8);
        private string _loadedBiome;
        private bool _loading, _built;
        private Camera _cam;
        private Transform _root;
        private SpriteRenderer _tint;          // additive/dark biome grade quad
        private readonly List<Transform> _weather = new List<Transform>(64);
        private int _weatherIdx;

        private void Update()
        {
            if (!PresentationState.InMatch) { Show(false); return; }
            FindCamera();
            if (_cam == null) return;
            if (_loadedBiome != Biome && !_loading) StartCoroutine(LoadBiome(Biome));
            if (!_built) { if (_spr.Count >= 5) Build(); else return; }
            DisableFlatBg();
            LayoutAndParallax();
            Weather();
        }

        // ---------------- asset loading (own UnityWebRequest, like UiAssets) ----------------
        private IEnumerator LoadBiome(string biome)
        {
            _loading = true; _built = false; _spr.Clear();
            string dir = Application.streamingAssetsPath + "/bulwark_ui/";
            string[] keys = { "sky", "horizon", "mid", "ground", "fg" };
            foreach (var k in keys)
            {
                string ext = k == "sky" ? ".jpg" : ".png";
                string url = "file://" + dir + "bf_" + biome + "_" + k + ext;
                if (url.Contains("://://")) url = url.Replace("://://", "://");
                if (dir.Contains("://")) url = dir + "bf_" + biome + "_" + k + ext; // jar on Android
                using (var req = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        var tex = DownloadHandlerTexture.GetContent(req); tex.wrapMode = TextureWrapMode.Clamp;
                        _spr[k] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                    else Debug.LogWarning("[BFX] load fail " + k + ": " + req.error);
                }
            }
            _loadedBiome = biome; _loading = false;
            Debug.Log($"[BFX] biome '{biome}' loaded ({_spr.Count}/5 layers).");
        }

        // ---------------- build the layer renderers ----------------
        private SpriteRenderer MakeSr(string name, string key, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(_root, false);
            var sr = go.AddComponent<SpriteRenderer>();
            if (_spr.TryGetValue(key, out var s)) sr.sprite = s;
            sr.sortingOrder = order;
            return sr;
        }

        private void Build()
        {
            if (_root == null) { _root = new GameObject("BFX_Root").transform; _root.SetParent(transform, false); }
            _sky     = new Layer { Sr = MakeSr("L1_Sky", "sky", -140),       Parallax = 1.00f, Anchor = 0f };
            _horizon = new Layer { Sr = MakeSr("L2_Horizon", "horizon", -136), Parallax = 0.82f, Anchor = 1f };
            _mid     = new Layer { Sr = MakeSr("L3_Mid", "mid", -132),       Parallax = 0.55f, Anchor = 1f };
            _ground  = new Layer { Sr = MakeSr("L4_Ground", "ground", -128), Parallax = 0.00f, Anchor = -1f };
            _fg      = new Layer { Sr = MakeSr("L5_Fg", "fg", 40),           Parallax = -0.30f, Anchor = -1f };
            // biome grade tint (subtle, painted mood — no realtime light)
            var tgo = new GameObject("L_Tint"); tgo.transform.SetParent(_root, false);
            _tint = tgo.AddComponent<SpriteRenderer>(); _tint.sprite = UiTex.Solid(BiomeTint(Biome)); _tint.sortingOrder = 55;
            _built = true;
            Debug.Log("[BFX] layers built.");
        }

        // ---------------- per-frame layout + parallax (Anchor: 0=cover-centre, 1=base-on-horizon-up, -1=top-on-horizon-down) ----------------
        private void LayoutAndParallax()
        {
            float size = _cam.orthographicSize, aspect = _cam.aspect > 0.01f ? _cam.aspect : 1.8f;
            float viewH = size * 2f, viewW = viewH * aspect;
            float cx = _cam.transform.position.x, cy = _cam.transform.position.y;
            float horizonY = cy + viewH * 0.06f; // the ground line where units stand (slightly above centre)

            Place(_sky, cx, cy, viewW * 1.35f, viewH * 1.2f, horizonY);
            Place(_horizon, cx, cy, viewW * 1.35f, viewH * 0.55f, horizonY);
            Place(_mid, cx, cy, viewW * 1.35f, viewH * 0.5f, horizonY);
            Place(_ground, cx, cy, viewW * 1.35f, viewH * 0.62f, horizonY);
            Place(_fg, cx, cy, viewW * 1.4f, viewH * 0.28f, horizonY);
            if (_tint != null) { _tint.transform.position = new Vector3(cx, cy, 0f); _tint.transform.localScale = new Vector3(viewW * 1.4f, viewH * 1.4f, 1f); }
        }

        private void Place(Layer L, float cx, float cy, float targetW, float targetH, float horizonY)
        {
            if (L.Sr == null || L.Sr.sprite == null) return;
            var b = L.Sr.sprite.bounds.size; if (b.x < 0.01f) b.x = 1f; if (b.y < 0.01f) b.y = 1f;
            L.Sr.transform.localScale = new Vector3(targetW / b.x, targetH / b.y, 1f);
            float px = cx * L.Parallax; // parallax: 1=static-on-screen, 0=world-locked, negative=faster
            float py;
            if (L.Anchor > 0.5f) py = horizonY + targetH * 0.5f - targetH * 0.18f;   // silhouette base near horizon, extends up
            else if (L.Anchor < -0.5f) py = horizonY - targetH * 0.5f;               // top edge at horizon, extends down
            else py = cy;                                                            // cover-centre (sky/tint)
            L.Sr.transform.position = new Vector3(px, py, 0f);
        }

        // ---------------- weather (pooled, presentation-only) ----------------
        private void Weather()
        {
            string w = WeatherFor(Biome);
            if (w == null) return;
            // lazily build a small pool of drifting particle quads
            if (_weather.Count == 0)
            {
                for (int i = 0; i < 48; i++)
                {
                    var go = new GameObject("wx"); go.transform.SetParent(_root, false);
                    var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = 60;
                    sr.sprite = UiTex.Disc(WeatherColor(w), 8);
                    _weather.Add(go.transform);
                }
            }
            float size = _cam.orthographicSize, aspect = _cam.aspect > 0.01f ? _cam.aspect : 1.8f;
            float viewH = size * 2f, viewW = viewH * aspect;
            float cx = _cam.transform.position.x, cy = _cam.transform.position.y;
            float t = Time.unscaledTime;
            for (int i = 0; i < _weather.Count; i++)
            {
                float seed = i * 0.137f;
                float fall = w == "snow" ? 0.6f : w == "rain" ? 3.2f : 0.9f;
                float fx = Mathf.Repeat(seed * viewW + t * (w == "rain" ? 0.4f : 0.15f) * viewW, viewW) - viewW * 0.5f;
                float fy = Mathf.Repeat(seed * viewH + t * fall, viewH);
                float drift = w == "snow" ? Mathf.Sin(t * 0.8f + seed * 6f) * viewW * 0.02f : (w == "rain" ? viewW * -0.04f : 0f);
                var tr = _weather[i];
                tr.position = new Vector3(cx + fx + drift, cy + viewH * 0.5f - fy, 0f);
                tr.localScale = w == "rain" ? new Vector3(0.06f, 0.5f, 1f) : new Vector3(0.18f, 0.18f, 1f);
            }
        }

        // ---------------- helpers ----------------
        private void FindCamera()
        {
            if (_cam != null) return;
            _cam = Camera.main;
            if (_cam == null) { var c = Camera.allCameras; if (c != null && c.Length > 0) _cam = c[0]; }
        }

        private bool _flatBgKilled;
        private void DisableFlatBg()
        {
            if (_flatBgKilled) return;
            var bg = GameObject.Find("battlefield_bg");
            if (bg != null) { bg.SetActive(false); _flatBgKilled = true; Debug.Log("[BFX] flat battlefield_bg disabled (parallax owns the bg)."); }
        }

        private void Show(bool on)
        {
            if (_root != null && _root.gameObject.activeSelf != on) _root.gameObject.SetActive(on);
        }

        private static Color BiomeTint(string b)
        {
            switch (b)
            {
                case "ash": return new Color(0.5f, 0.18f, 0.12f, 0.12f);
                case "snow": return new Color(0.6f, 0.7f, 0.8f, 0.10f);
                case "volcanic": return new Color(0.7f, 0.25f, 0.10f, 0.14f);
                case "dead": return new Color(0.4f, 0.42f, 0.4f, 0.14f);
                default: return new Color(0.9f, 0.85f, 0.6f, 0.05f); // grass: warm, faint
            }
        }
        private static string WeatherFor(string b)
        {
            switch (b) { case "ash": return "ash"; case "snow": return "snow"; case "volcanic": return "ash"; case "dead": return "ash"; default: return null; }
        }
        private static Color WeatherColor(string w)
        {
            switch (w) { case "snow": return new Color(0.95f, 0.97f, 1f, 0.8f); case "rain": return new Color(0.7f, 0.8f, 0.95f, 0.6f); default: return new Color(1f, 0.55f, 0.25f, 0.55f); }
        }
    }
}
