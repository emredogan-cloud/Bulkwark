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
        // Palette aliases → exact Bible hex (UiTheme). Names kept for back-compat with existing screens.
        public static readonly Color IronBlue = UiTheme.IronBlue;
        public static readonly Color AshRed   = UiTheme.Ember2;
        public static readonly Color Gold     = UiTheme.Gold;
        public static readonly Color Gem      = UiTheme.AmethystHi;
        public static readonly Color Purple   = UiTheme.Amethyst;
        public static readonly Color Grey     = new Color(0.28f, 0.30f, 0.36f);
        public static readonly Color PanelCol = UiTheme.Panel;
        public static readonly Color Dark     = UiTheme.Obsidian;

        private static Font _font;
        // Typography swap: the injected display TTF (PirataOne) from Resources, falling back to the builtin font.
        public static Font Font => _font != null ? _font
            : (_font = Resources.Load<Font>("Fonts/StickForge_Display") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

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
            // icon — prefer the real coin/gem sliced from /design (gem when the chip colour is blue/purple-dominant)
            var iconRt = Rect("Icon", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(52, 52));
            valueIcon = iconRt.gameObject.AddComponent<Image>();
            bool isGem = iconColor.b > iconColor.r;
            var real = UiAssets.Instance != null ? UiAssets.Instance.Get(isGem ? "ic_gem" : "ic_coin") : null;
            if (real != null) { valueIcon.sprite = real; valueIcon.color = Color.white; valueIcon.preserveAspect = true; }
            else { var gi = Spr("gold"); if (gi != null) { valueIcon.sprite = gi; valueIcon.color = iconColor; } else valueIcon.color = iconColor; }
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

        // ============================================================================================
        // High-fidelity builders (UI Construction Bible). Use UiTex/UiTheme/UiFx for the prestige look.
        // ============================================================================================

        /// <summary>Full-bleed screen backdrop from the real /design art (UiAssets.Backdrop &lt;screenKey&gt;), with a
        /// late-binder that swaps it in once the async asset load completes; obsidian fallback until then.</summary>
        public static Image Backdrop(Transform fullBleed, string screenKey)
        {
            var img = Stretch("Backdrop_" + screenKey, fullBleed, UiTheme.Obsidian);
            var bd = UiAssets.Instance != null ? UiAssets.Instance.Backdrop(screenKey) : null;
            if (bd != null) { img.sprite = bd; img.color = Color.white; img.preserveAspect = false; }
            img.gameObject.AddComponent<BackdropBinder>().Init(img, screenKey);
            // Scrim over the backdrop: the mockups have baked UI (wrong-brand logos/titles); this dims them so the
            // live BULWARK chrome on top dominates, while the atmospheric art still reads through.
            var scrim = Stretch("BackdropScrim_" + screenKey, fullBleed, new Color(0f, 0f, 0f, 0.45f)); scrim.raycastTarget = false;
            return img;
        }

        /// <summary>A full-bleed radial vignette (transparent centre → near-black corners), non-raycast.
        /// Parent under the full-bleed BG layer; satisfies every spec's "MUST NOT omit the vignette".</summary>
        public static Image Vignette(Transform parent, float strength = 0.55f)
        {
            var go = new GameObject("Vignette", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.Radial(UiTheme.A(UiTheme.Vignette, 0f), UiTheme.A(UiTheme.Vignette, strength), 128, 1.8f);
            return img;
        }

        /// <summary>A soft radial glow (bright centre → transparent edge), non-raycast. God-rays / fire / focal bloom.</summary>
        public static Image Glow(Transform parent, Color col, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, float power = 1.8f)
        {
            var rt = Rect("Glow", parent, aMin, aMax, pos, size);
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.Radial(col, UiTheme.A(col, 0f), 128, power);
            return img;
        }

        /// <summary>An ornate cast-gold beveled frame (UiTex.Frame molding + corner finials) over an obsidian
        /// field. Returns the inner FIELD RectTransform — parent content inside it. inset = molding thickness.</summary>
        public static RectTransform OrnateFrame(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
            Color? fieldColor = null, bool finials = true, float inset = 20f)
        {
            var group = Rect("OrnateFrame", parent, aMin, aMax, pos, size);

            // Prefer the real ornate gold panel sliced from /design (9-slice); fall back to procedural.
            var kitPanel = UiAssets.Instance != null ? UiAssets.Instance.Panel() : null;
            if (kitPanel != null)
            {
                var gimg = group.gameObject.AddComponent<Image>(); gimg.sprite = kitPanel; gimg.type = Image.Type.Sliced; gimg.raycastTarget = false;
                return Rect("Field", group, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-inset * 2f, -inset * 2f));
            }

            // Obsidian field (subtle top-lit vertical gradient), inset inside the molding.
            var field = Rect("Field", group, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-inset * 2f, -inset * 2f));
            var fimg = field.gameObject.AddComponent<Image>();
            fimg.sprite = UiTex.VGradient(UiTheme.FieldTop, fieldColor ?? UiTheme.FieldDark, 64);
            fimg.raycastTarget = false;

            // Cast-gold beveled molding (9-slice border ring), drawn over the field edge.
            var frameImg = group.gameObject.AddComponent<Image>();
            frameImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 16);
            frameImg.type = Image.Type.Sliced; frameImg.raycastTarget = false;

            if (finials)
            {
                Finial(group, new Vector2(0.5f, 1f), new Vector2(0, inset * 0.6f));   // top
                Finial(group, new Vector2(0.5f, 0f), new Vector2(0, -inset * 0.6f));  // bottom
            }
            field.SetAsLastSibling(); // content layer above molding interior
            return field;
        }

        /// <summary>A small gold diamond finial ornament overhanging a frame edge.</summary>
        public static Image Finial(Transform parent, Vector2 anchor, Vector2 pos, float size = 40f)
        {
            var rt = Rect("Finial", parent, anchor, anchor, pos, new Vector2(size, size));
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            return img;
        }

        /// <summary>A prestige UPPERCASE title: gold vertical gradient + dark stroke (Outline) + drop shadow.
        /// Honors the Bible's "serif Trajan-style gold-bevel UPPERCASE" titles (legacy Text stand-in for SDF).</summary>
        public static Text TitleLabel(Transform parent, string text, int size, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sz,
            TextAnchor align, Color? top = null, Color? bottom = null, bool track = true)
        {
            var t = Label(parent, track ? UiTheme.Track(text) : text, size, aMin, aMax, pos, sz, align, Color.white);
            var grad = t.gameObject.AddComponent<UiGradientText>();
            grad.top = top ?? UiTheme.GoldHi; grad.bottom = bottom ?? UiTheme.Gold;
            var ol = t.gameObject.AddComponent<Outline>(); ol.effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f); ol.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        /// <summary>Ornate gold progress bar: recessed obsidian channel + gold-gradient Filled fill + animated
        /// sheen + leading-edge tip glow + gold end-caps. Returns the parts (drive Fill.fillAmount / Tip).</summary>
        public struct BarParts { public RectTransform Group; public Image Track; public Image Fill; public RectTransform Tip; public float TrackWidth; }
        public static BarParts GoldBar(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, float amount, bool caps = true, bool tip = true)
        {
            var group = Rect("GoldBar", parent, aMin, aMax, pos, size);

            // Recessed obsidian channel (carved look via vertical gradient, darkest at top).
            var track = group.gameObject.AddComponent<Image>();
            track.sprite = UiTex.VGradient(UiTheme.ChannelDark, UiTheme.ChannelMid, 32);
            track.raycastTarget = false;

            // Gold gradient fill (Filled, Horizontal, Left origin).
            var fillRt = Rect("Fill", group, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8f, -8f));
            var fill = fillRt.gameObject.AddComponent<Image>();
            fill.sprite = UiTex.VGradient(UiTheme.GoldFillHi, UiTheme.GoldFillLo, 32);
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0;
            fill.fillAmount = Mathf.Clamp01(amount); fill.raycastTarget = false;

            // Sheen highlight band sliding across the fill.
            var sheenRt = Rect("Sheen", fillRt, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(60f, 0f));
            var sheen = sheenRt.gameObject.AddComponent<Image>();
            sheen.sprite = UiTex.HGradient(UiTheme.A(Color.white, 0f), UiTheme.A(Color.white, 0.4f), 32);
            sheen.raycastTarget = false;
            var sh = group.gameObject.AddComponent<Sheen>(); sh.target = sheenRt; sh.minX = -size.x * 0.5f; sh.maxX = size.x * 0.5f; sh.period = 1.4f;

            RectTransform tipRt = null;
            if (tip)
            {
                tipRt = Rect("Tip", group, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(size.x * Mathf.Clamp01(amount), 0), new Vector2(48f, size.y * 2.2f));
                var tg = tipRt.gameObject.AddComponent<Image>(); tg.raycastTarget = false;
                tg.sprite = UiTex.Radial(UiTheme.A(UiTheme.GoldFillHi, 0.9f), UiTheme.A(UiTheme.GoldFillHi, 0f), 64, 1.6f);
                var p = tipRt.gameObject.AddComponent<PulseGraphic>(); p.target = tg; p.min = 0.5f; p.max = 1f; p.period = 1.1f;
            }
            if (caps)
            {
                Finial(group, new Vector2(0, 0.5f), new Vector2(-6f, 0), size.y * 1.8f);
                Finial(group, new Vector2(1, 0.5f), new Vector2(6f, 0), size.y * 1.8f);
            }
            return new BarParts { Group = group, Track = track, Fill = fill, Tip = tipRt, TrackWidth = size.x };
        }

        /// <summary>Reposition a GoldBar's tip glow to follow the current fill amount (call when fillAmount changes).</summary>
        public static void UpdateBarTip(BarParts b)
        {
            if (b.Tip == null || b.Fill == null) return;
            var p = b.Tip.anchoredPosition; p.x = b.TrackWidth * Mathf.Clamp01(b.Fill.fillAmount); b.Tip.anchoredPosition = p;
        }

        public static Color Lighten(Color c, float t) => Color.Lerp(c, Color.white, t);
        public static Color Darken(Color c, float t) => Color.Lerp(c, Color.black, t);

        /// <summary>A glossy beveled "gem" button: vertical-gradient body (top sheen → dark body), cast-gold rim,
        /// leading icon disc, and a left UPPERCASE label. The function-coded primary CTA style (Main Menu etc.).</summary>
        public static Button GemButton(Transform parent, string label, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
            Color body, UnityAction onClick, int fontSize = 40, bool glow = false, bool leadingIcon = true, TextAnchor labelAlign = TextAnchor.MiddleLeft)
        {
            var rt = Rect("GemBtn_" + label, parent, aMin, aMax, pos, size);
            if (glow)
            {
                var gl = Glow(rt, UiTheme.A(Lighten(body, 0.3f), 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.5f);
                var pg = gl.gameObject.AddComponent<PulseGraphic>(); pg.target = gl; pg.min = 0.35f; pg.max = 0.7f; pg.period = 1.8f;
            }
            // Prefer the real gem-button sprite sliced from /design (gold rim + flourishes baked); the live label
            // covers any baked text. Fall back to the procedural gradient body + rim + leading-icon when absent.
            var kitBtn = UiAssets.Instance != null ? UiAssets.Instance.Button(KitButtonKey(body)) : null;
            var bodyImg = rt.gameObject.AddComponent<Image>();
            if (kitBtn != null) { bodyImg.sprite = kitBtn; bodyImg.type = Image.Type.Sliced; bodyImg.color = Color.white; }
            else { bodyImg.sprite = UiTex.VGradient(Lighten(body, 0.28f), Darken(body, 0.34f), 32); }
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = bodyImg;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });

            float pad = 28f;
            if (kitBtn == null)
            {
                var rim = Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
                if (leadingIcon)
                {
                    float d = size.y * 0.62f;
                    var disc = Rect("IconDisc", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(size.y * 0.62f, 0), new Vector2(d, d));
                    var dimg = disc.gameObject.AddComponent<Image>(); dimg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Obsidian, 0.65f), 48); dimg.raycastTarget = false;
                    var gly = Rect("Glyph", disc, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 0.6f, d * 0.6f));
                    var gimg = gly.gameObject.AddComponent<Image>(); gimg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32); gimg.raycastTarget = false;
                    pad = size.y + 24f;
                }
            }
            // Real kit buttons carry symmetric flourishes → centre the label between them; procedural uses labelAlign.
            var align = kitBtn != null ? TextAnchor.MiddleCenter : labelAlign;
            var lbl = Label(rt, label, fontSize, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, align, new Color(0.99f, 0.965f, 0.91f));
            var lrt = (RectTransform)lbl.transform;
            if (kitBtn != null) { lrt.offsetMin = new Vector2(size.x * 0.22f, 0); lrt.offsetMax = new Vector2(-size.x * 0.22f, 0); }
            else { lrt.offsetMin = new Vector2(pad, 0); lrt.offsetMax = new Vector2(-pad * 0.5f, 0); }
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            return btn;
        }

        /// <summary>Map a body colour to the nearest extracted gem-button colour (blue/red/gold/green/purple/dark).</summary>
        public static string KitButtonKey(Color c)
        {
            (string k, Color col)[] pal =
            {
                ("blue", UiTheme.IronBlue), ("blue", UiTheme.IronBlueHi),
                ("red", UiTheme.Oxblood), ("red", UiTheme.Ember2),
                ("gold", UiTheme.Gold), ("gold", UiTheme.Ember), ("gold", UiTheme.GoldHi),
                ("purple", UiTheme.Amethyst), ("purple", UiTheme.AmethystHi),
                ("green", new Color(0.11f, 0.54f, 0.23f)), ("green", new Color(0.27f, 0.60f, 0.27f)),
                ("dark", new Color(0.20f, 0.22f, 0.27f)), ("dark", UiTheme.Charcoal), ("dark", Grey),
            };
            string best = "blue"; float bd = float.MaxValue;
            foreach (var p in pal)
            {
                float d = (c.r - p.col.r) * (c.r - p.col.r) + (c.g - p.col.g) * (c.g - p.col.g) + (c.b - p.col.b) * (c.b - p.col.b);
                if (d < bd) { bd = d; best = p.k; }
            }
            return best;
        }

        /// <summary>A small red notification badge (dot or count) anchored to a parent's top-right corner.</summary>
        public static GameObject NotifyBadge(Transform parent, int count = 0)
        {
            var rt = Rect("Badge", parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -6), new Vector2(40, 40));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.Disc(UiTheme.Ember2, 32); img.raycastTarget = false;
            if (count > 0) Label(rt, count.ToString(), 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            rt.gameObject.AddComponent<PulseScale>();
            return rt.gameObject;
        }

        /// <summary>A round icon tile with a caption beneath (right-rail / live-ops shortcuts). Optional badge.</summary>
        public static Button IconTile(Transform parent, string caption, Vector2 aMin, Vector2 aMax, Vector2 pos, float diameter, Color ring, UnityAction onClick, bool badge = false, int badgeCount = 0)
        {
            var rt = Rect("Tile_" + caption, parent, aMin, aMax, pos, new Vector2(diameter, diameter));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            // gold ring
            var ringRt = Rect("Ring", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ringImg = ringRt.gameObject.AddComponent<Image>(); ringImg.sprite = UiTex.Frame(UiTheme.GoldHi, ring, UiTheme.GoldShadow, 48, 6); ringImg.type = Image.Type.Sliced; ringImg.raycastTarget = false;
            // glyph placeholder
            var gly = Rect("Glyph", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(diameter * 0.42f, diameter * 0.42f));
            var gimg = gly.gameObject.AddComponent<Image>(); gimg.sprite = UiTex.Diamond(UiTheme.A(UiTheme.GoldHi, 0.9f), 32); gimg.raycastTarget = false;
            // caption beneath
            Label(rt, caption, 22, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -22), new Vector2(diameter + 90, 30), TextAnchor.UpperCenter, UiTheme.ParchGold);
            if (badge) NotifyBadge(rt, badgeCount);
            return btn;
        }

        /// <summary>A thin gold horizontal rule (divider).</summary>
        public static Image Divider(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, float width, float thickness = 3f, Color? col = null)
        {
            var rt = Rect("Divider", parent, aMin, aMax, pos, new Vector2(width, thickness));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.HGradient(UiTheme.A(UiTheme.Gold, 0f), col ?? UiTheme.Gold, 32); img.raycastTarget = false;
            return img;
        }

        /// <summary>A left-aligned gold section header with a trailing rule.</summary>
        public static Text SectionHeader(Transform parent, string text, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
            => TitleLabel(parent, text, 36, aMin, aMax, pos, size, TextAnchor.MiddleLeft, UiTheme.GoldHi, UiTheme.Gold, false);

        /// <summary>A dark glass content card (9-slice panel + gold hairline). Returns the card RectTransform.</summary>
        public static RectTransform Card(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color? fill = null)
        {
            var rt = Rect("Card", parent, aMin, aMax, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            var s = Spr("panel"); if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; }
            img.color = fill ?? UiTheme.Panel;
            var rim = Rect("CardRim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 48, 5); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
            return rt;
        }

        /// <summary>A horizontal tab bar. Returns the tab Buttons (index order). Highlights the active tab gold.</summary>
        public static Button[] TabBar(Transform parent, string[] tabs, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, int active, System.Action<int> onSelect)
        {
            var row = Rect("TabBar", parent, aMin, aMax, pos, size);
            var btns = new Button[tabs.Length];
            float w = size.x / Mathf.Max(1, tabs.Length);
            for (int i = 0; i < tabs.Length; i++)
            {
                int idx = i;
                var b = Button(row, tabs[i], new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(w * (i + 0.5f), 0), new Vector2(w - 10, size.y), i == active ? UiTheme.Gold : UiTheme.A(UiTheme.Charcoal, 0.9f), () => onSelect?.Invoke(idx), 32);
                btns[i] = b;
            }
            return btns;
        }

        /// <summary>A row of N star pips (filled gold up to <paramref name="filled"/>, empty dark otherwise).</summary>
        public static void StarRating(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, int total, int filled, float starSize = 48f, float gap = 8f)
        {
            float totalW = total * starSize + (total - 1) * gap;
            var row = Rect("Stars", parent, aMin, aMax, pos, new Vector2(totalW, starSize));
            for (int i = 0; i < total; i++)
            {
                var s = Rect("Star", row, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(starSize * 0.5f + i * (starSize + gap), 0), new Vector2(starSize, starSize));
                var img = s.gameObject.AddComponent<Image>(); img.raycastTarget = false;
                img.sprite = UiTex.Diamond(i < filled ? UiTheme.GoldHi : UiTheme.A(UiTheme.Charcoal, 0.9f), 48);
            }
        }
    }
}
