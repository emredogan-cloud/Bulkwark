// BULWARK — VFX FRAMEWORK (Production Presentation phase, ADR-5-003 Option A · Priority #4). TEMPORARY/REMOVABLE.
//
// A removable presentation VFX layer: short-lived, code-driven, POOLED sprite effects (scale + alpha tweened) for
// hit impact, death puff, statue damage, and a spell/projectile API. The recovered VFX particle emitters were
// stripped (ASSET_INVENTORY §A3), so effects are rebuilt in-engine from a few curated placeholder textures rather
// than restoring emitters — deterministic, mobile-cheap, no Unity ParticleSystem authoring.
//
// PRESENTATION-ONLY: NO gameplay dependency, ZERO ECS writes. It is driven purely by the renderer's READ-ONLY
// observations (a unit/statue HP-drop, a unit-proxy cull) — exactly the same events the audio layer reacts to. It
// changes no rule, balance, AI, economy, or canon. Deleting this one file (+ the StreamingAssets/bulwark_vfx
// folder) removes all VFX 100%.
//
// LICENSING: textures under StreamingAssets/bulwark_vfx/ are © ripped — THROWAWAY DEV PLACEHOLDER ONLY, replace
// before any release.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulwark.Bootstrap
{
    /// <summary>Pooled code-driven sprite VFX, driven read-only by the renderer. Presentation-only.</summary>
    public sealed class VfxManager : MonoBehaviour
    {
        public static VfxManager Instance { get; private set; }
        public bool Ready { get; private set; }

        private static readonly string[] Names = { "vfx_impact", "vfx_puff", "vfx_glow", "vfx_spark" };
        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        private const int PoolSize = 48;     // bounds concurrent effects (massed combat recycles oldest)
        private const float FxZ = -0.5f;     // in front of the z=0 unit plane (camera looks +Z)

        private sealed class Fx
        {
            public GameObject Go; public SpriteRenderer Sr;
            public float T0, Life, S0, S1; public Color C0; public bool Active;
        }
        private Fx[] _pool;
        private int _next;
        private float _lastImpact = -10f, _lastStatue = -10f; // light throttles so massed combat doesn't spam

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("VfxManager");
            Instance = go.AddComponent<VfxManager>();
            DontDestroyOnLoad(go);
            Debug.Log("[VFX] VfxManager booted; loading textures…");
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _pool = new Fx[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                var go = new GameObject("fx_" + i); go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;        // above units (2) / statue (0) / mine (1) / bg (-100)
                sr.enabled = false;
                _pool[i] = new Fx { Go = go, Sr = sr, Active = false };
            }
            StartCoroutine(LoadAll());
        }

        private IEnumerator LoadAll()
        {
            string baseDir = Application.streamingAssetsPath + "/bulwark_vfx/";
            int ok = 0;
            foreach (var n in Names)
            {
                string path = baseDir + n + ".png";
                string url = path.Contains("://") ? path : "file://" + path;
                using (var req = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        var tex = DownloadHandlerTexture.GetContent(req);
                        if (tex != null)
                        {
                            tex.wrapMode = TextureWrapMode.Clamp;
                            _sprites[n] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                            ok++;
                        }
                    }
                    else Debug.LogWarning($"[VFX] failed to load {n}: {req.error}");
                }
            }
            Ready = true;
            Debug.Log($"[VFX] textures ready: {ok}/{Names.Length}.");
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = 0; i < _pool.Length; i++)
            {
                var fx = _pool[i];
                if (!fx.Active) continue;
                float t = (now - fx.T0) / Mathf.Max(0.0001f, fx.Life);
                if (t >= 1f) { fx.Active = false; fx.Sr.enabled = false; continue; }
                float s = Mathf.Lerp(fx.S0, fx.S1, t);
                fx.Go.transform.localScale = new Vector3(s, s, 1f);
                var c = fx.C0; c.a = fx.C0.a * (1f - t);   // fade out
                fx.Sr.color = c;
            }
        }

        private Sprite Spr(string key) => _sprites.TryGetValue(key, out var s) ? s : null;

        /// <summary>Spawn a pooled one-shot effect (recycles the oldest slot if all are busy).</summary>
        public void Spawn(string key, Vector3 pos, float life, float s0, float s1, Color color)
        {
            var spr = Spr(key);
            if (spr == null) return;             // textures not loaded / missing → no-op (never crash)
            // find a free slot, else recycle round-robin
            int idx = -1;
            for (int k = 0; k < _pool.Length; k++) { int j = (_next + k) % _pool.Length; if (!_pool[j].Active) { idx = j; break; } }
            if (idx < 0) { idx = _next; }
            _next = (idx + 1) % _pool.Length;

            var fx = _pool[idx];
            fx.Sr.sprite = spr;
            fx.Go.transform.position = new Vector3(pos.x, pos.y, FxZ);
            fx.Go.transform.localScale = new Vector3(s0, s0, 1f);
            fx.C0 = color; fx.Sr.color = color;
            fx.T0 = Time.unscaledTime; fx.Life = life; fx.S0 = s0; fx.S1 = s1;
            fx.Sr.enabled = true; fx.Active = true;
        }

        // ---------------- read-only event API (called by SimProxyRenderer) ----------------
        public void Impact(Vector3 pos)
        {
            if (Time.unscaledTime - _lastImpact < 0.04f) return; // throttle massed-combat spam
            _lastImpact = Time.unscaledTime;
            Spawn("vfx_impact", pos, 0.22f, 0.6f, 1.1f, new Color(1f, 0.95f, 0.7f, 0.9f));
        }

        public void DeathPuff(Vector3 pos) => Spawn("vfx_puff", pos, 0.45f, 0.8f, 2.0f, new Color(0.8f, 0.8f, 0.82f, 0.8f));

        public void StatueDamage(Vector3 pos)
        {
            if (Time.unscaledTime - _lastStatue < 0.10f) return;
            _lastStatue = Time.unscaledTime;
            Spawn("vfx_spark", pos, 0.30f, 1.0f, 2.2f, new Color(1f, 0.6f, 0.3f, 0.95f));
        }

        // API provided for later wiring (spell-event hook / projectile streak — see discovery gaps).
        public void SpellCast(Vector3 pos, Color color) => Spawn("vfx_glow", pos, 0.5f, 0.4f, 2.4f, color);
        public void SpellImpact(Vector3 pos, Color color) => Spawn("vfx_spark", pos, 0.35f, 0.8f, 2.6f, color);
        public void Trail(Vector3 from, Vector3 to) => Spawn("vfx_spark", Vector3.Lerp(from, to, 0.5f), 0.15f, 0.6f, 0.9f, new Color(1f, 1f, 0.8f, 0.7f));
    }
}
