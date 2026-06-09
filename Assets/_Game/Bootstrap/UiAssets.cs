// BULWARK — UI ASSET LOADER (real art extracted from /design). Presentation-only, REMOVABLE.
//
// Loads the production UI sprites sliced from the original /design mockups (Assets/StreamingAssets/bulwark_ui/)
// at runtime — the same robust UnityWebRequest→Texture2D→Sprite pipeline as PlaceholderAssets (bypasses the
// editor sprite-import pipeline; works on device). Provides: per-screen atmospheric BACKDROPS (bd_*), the
// reusable ornate gold 9-slice PANEL (kit_panel_ornate), six colour gem BUTTONS (kit_btn_*), the gem FINIAL,
// and an ICON set. 9-slice border (Vector4) is assigned here so panels/buttons stretch with fixed ornate
// corners/caps. UiWidgets prefers these real sprites and falls back to the procedural UiTex set when absent.
// NO gameplay/ECS impact (§12): it only reads PNG/JPG files + creates Sprites.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulwark.Bootstrap
{
    /// <summary>Runtime loader for the real UI sprites extracted from /design. Presentation-only.</summary>
    public sealed class UiAssets : MonoBehaviour
    {
        public static UiAssets Instance { get; private set; }
        public bool Ready { get; private set; }

        // Per-screen backdrops (JPG) — keys mirror the screen class roots.
        private static readonly string[] Backdrops =
        {
            "splash","loading","login","mainmenu","modeselect","matchintro","battlehud","spellhud","banner",
            "pause","victory","defeat","campaignresult","endlessresult","ladderresult","store","spells","skins",
            "chests","chestopen","units","commander","profile","battlepass","quests","campaignmap","daily",
            "luckyspin","freerewards","events","onlinebattle","tournament","leaderboard","clan","settings",
            "confirm","rewardgrant","networkerror",
        };
        // 9-slice chrome (PNG): key -> (leftRightFrac, topBottomFrac) of the loaded texture.
        private static readonly (string key, float lr, float tb)[] Sliced =
        {
            ("kit_panel_ornate", 0.20f, 0.20f),
            ("kit_btn_blue", 0.17f, 0.22f), ("kit_btn_dark", 0.17f, 0.22f), ("kit_btn_red", 0.17f, 0.22f),
            ("kit_btn_gold", 0.17f, 0.22f), ("kit_btn_green", 0.17f, 0.22f), ("kit_btn_purple", 0.17f, 0.22f),
        };
        // plain sprites (PNG): finial + icons.
        private static readonly string[] Plain = { "kit_finial", "ic_coin", "ic_gem" };
        // LAYER 0 — clean UI-free background plates (JPG). No baked text/UI (Phase-3 architecture).
        private static readonly string[] Plates =
        { "splash", "loading", "mainmenu", "modeselect", "campaignmap", "battlehud", "victory", "defeat",
          // Phase-7 meta-screen clean plates (replace the baked-UI mockup backdrops)
          "store", "chestopen", "rewardgrant", "leaderboard", "profile", "clan", "quests", "daily",
          "tournament", "endlessresult", "settings", "about" };
        // LAYER 1 — stick-figure characters + army strips (PNG, transparent).
        private static readonly string[] CharPngs =
        {
            "char_king", "char_mage", "char_archer", "char_sword_blue", "char_sword_red",
            "char_spear_blue", "char_spear_red", "char_brute_red", "char_kneel", "char_cheer",
            "army_blue", "army_red",
        };

        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>(64);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("UiAssets");
            Instance = go.AddComponent<UiAssets>();
            DontDestroyOnLoad(go);
            Debug.Log("[UIART] UiAssets booted; loading /design-extracted UI sprites…");
        }

        private void Awake() { if (Instance == null) Instance = this; StartCoroutine(LoadAll()); }

        private IEnumerator LoadAll()
        {
            string baseDir = Application.streamingAssetsPath + "/bulwark_ui/";
            int total = 0;
            var ok = new int[1];
            foreach (var k in Plates)   { total++; yield return Step(baseDir, "plate_" + k, ".jpg", Vector4.zero, ok); }
            foreach (var k in CharPngs) { total++; yield return Step(baseDir, k, ".png", Vector4.zero, ok); }
            foreach (var s in Sliced)   { total++; yield return Step(baseDir, s.key, ".png", new Vector4(s.lr, s.tb, s.lr, s.tb), ok); }
            foreach (var k in Plain)    { total++; yield return Step(baseDir, k, ".png", Vector4.zero, ok); }
            foreach (var k in Backdrops) { total++; yield return Step(baseDir, "bd_" + k, ".jpg", Vector4.zero, ok); }
            Ready = true;
            Debug.Log($"[UIART] UI sprites ready: {ok[0]}/{total} loaded.");
        }

        private IEnumerator Step(string dir, string key, string ext, Vector4 borderFrac, int[] ok)
        {
            string path = dir + key + ext;
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
                        var rect = new Rect(0, 0, tex.width, tex.height);
                        Vector4 border = Vector4.zero;
                        if (borderFrac != Vector4.zero)
                            border = new Vector4(tex.width * borderFrac.x, tex.height * borderFrac.y, tex.width * borderFrac.z, tex.height * borderFrac.w);
                        _sprites[key] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                        ok[0]++;
                        yield break;
                    }
                }
                Debug.LogWarning($"[UIART] failed to load {key}{ext}: {req.error}");
            }
        }

        /// <summary>Loaded sprite by key, or null.</summary>
        public Sprite Get(string key) => _sprites.TryGetValue(key, out var s) ? s : null;
        /// <summary>Per-screen backdrop (bd_&lt;key&gt;), or null if absent.</summary>
        public Sprite Backdrop(string screenKey) => Get("bd_" + screenKey);
        /// <summary>The ornate gold 9-slice panel.</summary>
        public Sprite Panel() => Get("kit_panel_ornate");
        /// <summary>A colour gem button 9-slice (blue/dark/red/gold/green/purple).</summary>
        public Sprite Button(string color) => Get("kit_btn_" + color);
        /// <summary>The gem finial ornament.</summary>
        public Sprite Finial() => Get("kit_finial");
        /// <summary>LAYER 0 — clean UI-free background plate (plate_&lt;screenKey&gt;), or null.</summary>
        public Sprite Plate(string screenKey) => Get("plate_" + screenKey);
        /// <summary>LAYER 1 — stick-figure character (char_&lt;archetype&gt;), or null.</summary>
        public Sprite Character(string archetype) => Get("char_" + archetype);
        /// <summary>LAYER 1 — stick army silhouette strip (army_&lt;faction&gt;), or null.</summary>
        public Sprite Army(string faction) => Get("army_" + faction);
    }
}
