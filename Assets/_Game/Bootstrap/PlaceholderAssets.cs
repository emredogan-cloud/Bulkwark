// BULWARK — PLACEHOLDER ASSET LOADER (Asset Migration / Presentation Pass). TEMPORARY, REMOVABLE.
//
// Loads the curated TEMPORARY placeholder PNGs from StreamingAssets/bulwark/ at runtime into Sprites, so the
// presentation layer (SimProxyRenderer sprite proxies + the uGUI UiFlow) can show real art instead of debug
// primitives. Runtime PNG load (UnityWebRequestTexture → Texture2D.LoadImage → Sprite.Create) deliberately
// BYPASSES Unity's editor sprite-import pipeline — no hand-authored .meta importer can break it (robust on device).
//
// ⚖️ LICENSING: every PNG under StreamingAssets/bulwark/ is ripped from Stick War: Legacy (© Max Games Studios)
// and is a THROWAWAY DEV PLACEHOLDER ONLY — not shippable, replace with original/licensed art before any release
// (see reports/asset-migration/). This loader + folder are fully removable.
//
// NO gameplay/ECS/balance impact: this only reads PNG files + creates Sprites. It is the presentation layer (§12).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulwark.Bootstrap
{
    /// <summary>Runtime loader for the temporary placeholder sprites (StreamingAssets/bulwark/*.png).</summary>
    public sealed class PlaceholderAssets : MonoBehaviour
    {
        public static PlaceholderAssets Instance { get; private set; }
        public bool Ready { get; private set; }

        // Curated placeholder set copied into StreamingAssets/bulwark/ (see ASSET_INVENTORY.md mapping).
        private static readonly string[] Names =
        {
            // unit archetype sprites (per-role differentiation; team conveyed by tint)
            "unit_player", "unit_ai", "miner", "u_heavy", "u_ranged", "u_caster",
            "mine", "statue", "statue_cracks",
            "bg_battle", "bg_menu", "bg_victory", "bg_defeat", "button", "panel", "gold",
        };

        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("PlaceholderAssets");
            Instance = go.AddComponent<PlaceholderAssets>();
            DontDestroyOnLoad(go);
            Debug.Log("[ASSETS] PlaceholderAssets booted; loading placeholder sprites…");
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            StartCoroutine(LoadAll());
        }

        private IEnumerator LoadAll()
        {
            string baseDir = Application.streamingAssetsPath + "/bulwark/";
            int ok = 0;
            foreach (var n in Names)
            {
                string path = baseDir + n + ".png";
                // Android StreamingAssets is a jar URL already (contains "://"); other platforms need file://.
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
                            _sprites[n] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                            ok++;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[ASSETS] failed to load {n}: {req.error}");
                    }
                }
            }
            Ready = true;
            Debug.Log($"[ASSETS] placeholder sprites ready: {ok}/{Names.Length} loaded.");
        }

        /// <summary>Loaded sprite by key, or null if not (yet) available.</summary>
        public Sprite Get(string key) => _sprites.TryGetValue(key, out var s) ? s : null;

        /// <summary>Convenience: a unit sprite for a team (player=0 → unit_player, else unit_ai).</summary>
        public Sprite UnitFor(int team, bool miner) => miner ? Get("miner") : Get(team == 0 ? "unit_player" : "unit_ai");
    }
}
