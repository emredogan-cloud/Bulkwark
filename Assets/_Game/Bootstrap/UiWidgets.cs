// BULWARK — UI WIDGETS (UI Implementation · shared code-built widget library). Presentation-only, REMOVABLE.
//
// Completes the WP-00 "shared widget library" objective (deferred from the initial WP-00 cut): one tested set
// of code-built uGUI builders reused by every WP-02+ screen, so each screen is small and consistent. Mirrors
// the proven patterns in UiFlow/SplashScreen (legacy Text + "button"/"panel" placeholder 9-slice sprites,
// shadowed labels, audio-on-click). NO gameplay/ECS impact (presentation §12).

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Bulwark.Bootstrap
{
    /// <summary>Shared code-built uGUI builders for the landscape UI. Presentation-only.</summary>
    public static class UiWidgets
    {
        public static readonly Color IronBlue = new Color(0.20f, 0.45f, 0.95f);
        public static readonly Color AshRed   = new Color(0.85f, 0.25f, 0.20f);
        public static readonly Color Gold     = new Color(1f, 0.84f, 0.20f);
        public static readonly Color Gem      = new Color(0.62f, 0.36f, 0.95f);
        public static readonly Color Purple   = new Color(0.42f, 0.22f, 0.70f);
        public static readonly Color Grey     = new Color(0.28f, 0.30f, 0.36f);
        public static readonly Color PanelCol = new Color(0.10f, 0.11f, 0.15f, 0.92f);
        public static readonly Color Dark     = new Color(0.06f, 0.07f, 0.10f, 1f);

        private static Font _font;
        public static Font Font => _font != null ? _font : (_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public static Sprite Spr(string key) => PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(key) : null;

        /// <summary>An empty RectTransform child.</summary>
        public static RectTransform Rect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        /// <summary>A full-stretch Image (background / overlay). spriteKey null = solid color.</summary>
        public static Image Stretch(string name, Transform parent, Color col, string spriteKey = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            var s = spriteKey != null ? Spr(spriteKey) : null;
            if (s != null) { img.sprite = s; img.color = col; img.preserveAspect = false; }
            else img.color = col;
            return img;
        }

        /// <summary>A 9-slice panel (uses the "panel" placeholder sprite, falls back to solid).</summary>
        public static Image Panel(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color col)
        {
            var rt = Rect("Panel", parent, aMin, aMax, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            var s = Spr("panel");
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            img.color = col;
            return img;
        }

        /// <summary>A shadowed, non-raycast label.</summary>
        public static Text Label(Transform parent, string text, int size, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sz, TextAnchor align, Color col)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = pos; rt.sizeDelta = sz;
            var t = go.GetComponent<Text>();
            t.font = Font; t.text = text; t.fontSize = size; t.alignment = align; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var sh = go.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.8f); sh.effectDistance = new Vector2(2, -2);
            return t;
        }

        /// <summary>Convenience: a centered label positioned by a single anchor point.</summary>
        public static Text LabelAt(Transform parent, string text, int size, Vector2 anchor, Vector2 sz, TextAnchor align, Color col)
            => Label(parent, text, size, anchor, anchor, Vector2.zero, sz, align, col);

        /// <summary>A textured text button (uses the "button" placeholder sprite). Plays a click + onClick.</summary>
        public static Button Button(Transform parent, string text, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color tint, UnityAction onClick, int fontSize = 44)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            var s = Spr("button"); if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            img.color = tint;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            cb.pressedColor = new Color(0.78f, 0.78f, 0.84f, 1f);
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            if (!string.IsNullOrEmpty(text))
                Label(go.transform, text, fontSize, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            return btn;
        }

        /// <summary>Top-left Back arrow.</summary>
        public static Button BackButton(Transform parent, UnityAction onClick)
            => Button(parent, "BACK", new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, -70), new Vector2(180, 96), Grey, onClick, 34);

        /// <summary>A currency chip (icon swatch + value) anchored to the top-right. Returns the value Text
        /// (keep the ref to update it). chipIndex 0 = rightmost; chips stack leftward.</summary>
        public static Text CurrencyChip(Transform parent, Color iconColor, int value, int chipIndex, out Image valueIcon)
        {
            float w = 280f, gap = 16f;
            float x = -(20f + w * 0.5f) - chipIndex * (w + gap);
            var rt = Rect("Chip", parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(x, -64f), new Vector2(w, 80f));
            var bg = rt.gameObject.AddComponent<Image>();
            var ps = Spr("panel"); if (ps != null) { bg.sprite = ps; bg.type = Image.Type.Sliced; }
            bg.color = new Color(0, 0, 0, 0.55f);
            // icon
            var iconRt = Rect("Icon", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(52, 52));
            valueIcon = iconRt.gameObject.AddComponent<Image>();
            var gi = Spr("gold");
            if (gi != null) { valueIcon.sprite = gi; valueIcon.color = iconColor; } else valueIcon.color = iconColor;
            // value
            var t = Label(rt, value.ToString("N0"), 40, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(90, 0), new Vector2(170, 64), TextAnchor.MiddleLeft, Color.white);
            // plus button
            Button(rt, "+", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-34, 0), new Vector2(56, 56), new Color(0.2f, 0.7f, 0.25f), null, 40);
            return t;
        }

        /// <summary>A small square icon button with a caption beneath (rail / feature buttons).</summary>
        public static Button IconButton(Transform parent, string caption, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color tint, UnityAction onClick)
        {
            var btn = Button(parent, "", aMin, aMax, pos, size, tint, onClick, 0);
            Label(btn.transform, caption, 24, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -22), new Vector2(size.x + 60, 30), TextAnchor.UpperCenter, new Color(1, 1, 1, 0.9f));
            return btn;
        }

        /// <summary>A filled progress bar. Returns the fill Image (set fillAmount to update).</summary>
        public static Image ProgressBar(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color fillCol, float amount)
        {
            var bgRt = Rect("Bar", parent, aMin, aMax, pos, size);
            var bg = bgRt.gameObject.AddComponent<Image>(); var ps = Spr("panel"); if (ps != null) { bg.sprite = ps; bg.type = Image.Type.Sliced; } bg.color = new Color(0, 0, 0, 0.6f);
            var fillRt = Rect("Fill", bgRt, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -10));
            var fill = fillRt.gameObject.AddComponent<Image>();
            var bs = Spr("button"); if (bs != null) { fill.sprite = bs; fill.type = Image.Type.Filled; } else fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = Mathf.Clamp01(amount); fill.color = fillCol;
            return fill;
        }
    }
}
