// STICK EMPIRE RISE — 4-LAYER UI COMPOSITION HELPER (Phase 3, Report 05 architecture). Presentation-only.
//
// Enforces the clean layered model that makes the "broken UI over art" failure (Report 04) impossible:
//   LAYER 0  Plate(...)      — UI-FREE, TEXT-FREE background plate (plate_*). Never a finished mockup.
//   LAYER 1  Character/Army  — stick-figure assets (char_*/army_*) placed deliberately.
//   LAYER 2  (UiWidgets kit) — clean text-free ornate panels / gem buttons / cards (built by callers).
//   LAYER 3  (UiWidgets text)— live labels/numbers (the ONLY text source on screen).
// Backgrounds/characters here carry NO text and NO baked UI, so live UI on top can never double a baked element.
// NO gameplay/ECS impact (§12): pure presentation (Sprites + RectTransforms).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Builders for the clean 4-layer screen composition. Presentation-only.</summary>
    public static class UiLayers
    {
        /// <summary>LAYER 0 — full-bleed clean background plate + optional legibility scrim. Falls back to a
        /// dark obsidian gradient (never the baked mockup) so a missing plate degrades cleanly, not into doubling.</summary>
        public static Image Plate(Transform fullBleed, string screenKey, float scrim = 0.32f)
        {
            var img = UiWidgets.Stretch("L0_Plate_" + screenKey, fullBleed, UiTheme.Obsidian);
            var p = UiAssets.Instance != null ? UiAssets.Instance.Plate(screenKey) : null;
            if (p != null) { img.sprite = p; img.color = Color.white; img.type = Image.Type.Simple; img.preserveAspect = false; }
            else { img.sprite = UiTex.VGradient(UiTheme.A(UiTheme.IronBlue, 0.25f), UiTheme.Obsidian, 64); } // clean fallback, no text
            img.raycastTarget = false;
            img.gameObject.AddComponent<UiAssetBinder>().Init(img, a => a.Plate(screenKey), false);
            if (scrim > 0f)
            {
                var s = UiWidgets.Stretch("L0_Scrim_" + screenKey, fullBleed, new Color(0f, 0f, 0f, scrim));
                s.raycastTarget = false;
            }
            return img;
        }

        /// <summary>LAYER 1 — a stick-figure character placed at an anchor, sized by height (aspect-preserved).</summary>
        public static Image Character(Transform parent, string archetype, Vector2 anchor, Vector2 posPx, float heightPx, bool flip = false)
        {
            var rt = UiWidgets.Rect("L1_Char_" + archetype, parent, anchor, anchor, posPx, new Vector2(heightPx * 0.583f, heightPx));
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false; img.preserveAspect = true;
            var s = UiAssets.Instance != null ? UiAssets.Instance.Character(archetype) : null;
            if (s != null) img.sprite = s; else img.color = new Color(0, 0, 0, 0);
            img.gameObject.AddComponent<UiAssetBinder>().Init(img, a => a.Character(archetype), true);
            if (flip) rt.localScale = new Vector3(-1f, 1f, 1f);
            return img;
        }

        /// <summary>LAYER 1 — a horizontal stick-army silhouette band (faction = "blue"/"red").</summary>
        public static Image Army(Transform parent, string faction, Vector2 anchorMin, Vector2 anchorMax, float yPx, float hPx)
        {
            var rt = UiWidgets.Rect("L1_Army_" + faction, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            rt.offsetMin = new Vector2(0, yPx); rt.offsetMax = new Vector2(0, yPx + hPx);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false; img.preserveAspect = true;
            var s = UiAssets.Instance != null ? UiAssets.Instance.Army(faction) : null;
            if (s != null) img.sprite = s; else img.color = new Color(0, 0, 0, 0);
            img.gameObject.AddComponent<UiAssetBinder>().Init(img, a => a.Army(faction), true);
            return img;
        }
    }
}
