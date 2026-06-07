// BULWARK — IN-MATCH CHROME (shared static builder for Bible 09/10 presentation screens). Presentation-only, REMOVABLE.
//
// The Bible's SpellHud (09) and Banner (10) "inherit 08's edge chrome verbatim" at a specific battle moment. The
// LIVE chrome is BattleHud.cs (ECS-wired). These two are presentation/validation screens shown with the mockups'
// static stub values (their live integration — spell casts, sim movement-vector arrows — is GAMEPLAY outside the
// §12 UI boundary, see each spec's Section N). This builder reproduces the 08 chrome (scrims, dual HP troughs +
// crests + centre node, gold/supply/army chips, N unit-train tiles with costs, GARRISON/DEFEND/ATTACK) as
// DISPLAY-ONLY so 09/10 stay DRY and on-identity. NO ECS/gameplay (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Static, display-only reproduction of the Bible-08 in-match chrome for the 09/10 screens.</summary>
    public static class InMatchChrome
    {
        /// <summary>Build the 08 chrome with the given stub values. fullBleed = screen Rect (scrims), safe = SafeContent.</summary>
        public static void Build(Transform fullBleed, Transform safe, System.Action onPause, int gold, string supply, string hpL, string hpR, string army, int[] costs)
        {
            // Scrims (top + bottom legibility gradients, raycast off).
            var st = UiWidgets.Rect("Scrim_Top", fullBleed, new Vector2(0, 0.82f), new Vector2(1, 1), Vector2.zero, Vector2.zero); st.offsetMin = Vector2.zero; st.offsetMax = Vector2.zero;
            var sti = st.gameObject.AddComponent<Image>(); sti.raycastTarget = false; sti.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Vignette, 0.7f), UiTheme.A(UiTheme.Vignette, 0f), 32);
            var sb = UiWidgets.Rect("Scrim_Bottom", fullBleed, new Vector2(0, 0), new Vector2(1, 0.2f), Vector2.zero, Vector2.zero); sb.offsetMin = Vector2.zero; sb.offsetMax = Vector2.zero;
            var sbi = sb.gameObject.AddComponent<Image>(); sbi.raycastTarget = false; sbi.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Vignette, 0f), UiTheme.A(UiTheme.Vignette, 0.7f), 32);

            // Pause (top-left).
            var pause = Chip(safe, new Vector2(0, 1), new Vector2(58, -54), new Vector2(86, 86));
            var pb = pause.gameObject.AddComponent<Button>(); pb.targetGraphic = pause.GetComponent<Image>(); pb.onClick.AddListener(() => { AudioManager.Instance?.Click(); onPause?.Invoke(); });
            UiWidgets.Label(pause, "II", 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.GoldHi);

            // Gold + supply chips (top-left, under pause).
            var g = Chip(safe, new Vector2(0, 1), new Vector2(150, -154), new Vector2(220, 64));
            IconIn(g, UiTheme.Gold); UiWidgets.Label(g, gold.ToString(), 30, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(78, 0), new Vector2(140, 50), TextAnchor.MiddleLeft, Hex("#ffe9a8"));
            var sup = Chip(safe, new Vector2(0, 1), new Vector2(330, -154), new Vector2(170, 64));
            IconIn(sup, UiTheme.Parchment); UiWidgets.Label(sup, supply, 28, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(72, 0), new Vector2(110, 46), TextAnchor.MiddleLeft, Hex("#e8e2cf"));

            // Army chip (top-right).
            var a = Chip(safe, new Vector2(1, 1), new Vector2(-130, -54), new Vector2(210, 64));
            IconIn(a, UiTheme.Ember2); UiWidgets.Label(a, army, 28, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(78, 0), new Vector2(140, 46), TextAnchor.MiddleLeft, Hex("#ffd9cf"));

            // Dual HP bars + crests + centre node.
            HpBar(safe, false, UiTheme.IronBlue, hpL);
            HpBar(safe, true, UiTheme.Ember2, hpR);
            var node = UiWidgets.Rect("CenterNode", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -44), new Vector2(46, 46));
            var ni = node.gameObject.AddComponent<Image>(); ni.raycastTarget = false; ni.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);

            // Unit-train tray (bottom-left), N tiles with costs (display-only).
            for (int i = 0; i < costs.Length; i++)
            {
                var tile = UiWidgets.Rect("UnitBtn_" + i, safe, new Vector2(0, 0), new Vector2(0, 0), new Vector2(40 + 28 + i * 164, 90 + 73), new Vector2(150, 146));
                var ti = tile.gameObject.AddComponent<Image>(); ti.sprite = UiTex.VGradient(UiWidgets.Lighten(UiTheme.IronBlue, 0.12f), UiTheme.Charcoal, 32);
                var tb = tile.gameObject.AddComponent<Button>(); tb.targetGraphic = ti; tb.onClick.AddListener(() => AudioManager.Instance?.Click());
                var fr = UiWidgets.Rect("Frame", tile, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fi = fr.gameObject.AddComponent<Image>(); fi.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fi.type = Image.Type.Sliced; fi.raycastTarget = false;
                var chip = UiWidgets.Rect("CostChip", tile, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(110, 44));
                var ci = chip.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(UiTheme.A(UiTheme.Obsidian, 0.85f), 48);
                IconIn(chip, UiTheme.Gold, 24); UiWidgets.Label(chip, costs[i].ToString(), 24, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(70, 36), TextAnchor.MiddleLeft, Hex("#ffe9a8"));
            }

            // Order cluster (bottom-right): GARRISON / DEFEND / ATTACK (display-only).
            OrderBtn(safe, "GARRISON", Hex("#20242e"), -396 - 256, false);
            OrderBtn(safe, "DEFEND", Hex("#20242e"), -396, false);
            OrderBtn(safe, "ATTACK", UiTheme.Oxblood, -396 + 256, true);
        }

        private static void OrderBtn(Transform parent, string text, Color body, float x, bool primary)
        {
            var rt = UiWidgets.Rect("Btn_" + text, parent, new Vector2(1, 0), new Vector2(1, 0), new Vector2(x, 90 + 56), new Vector2(246, 113));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiWidgets.Lighten(body, 0.25f), UiWidgets.Darken(body, 0.3f), 32);
            if (primary) img.color = new Color(1.18f, 1.18f, 1.18f, 1f);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(() => AudioManager.Instance?.Click());
            var fr = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fi = fr.gameObject.AddComponent<Image>(); fi.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fi.type = Image.Type.Sliced; fi.raycastTarget = false;
            UiWidgets.Label(rt, text, primary ? 30 : 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, primary ? Hex("#ffe6a0") : Hex("#ecd9a6"));
        }

        private static void HpBar(Transform parent, bool right, Color col, string text)
        {
            float innerX = right ? 0.515f : 0.485f;
            var holder = UiWidgets.Rect("HpBar_" + (right ? "R" : "L"), parent, new Vector2(innerX, 1f), new Vector2(innerX, 1f), new Vector2(right ? 351 : -351, -56), new Vector2(702, 46));
            var bg = holder.gameObject.AddComponent<Image>(); bg.raycastTarget = false; bg.sprite = UiTex.VGradient(Hex("#1a140a"), Hex("#0c0e14"), 32);
            var fr = UiWidgets.Rect("Trough", holder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fi = fr.gameObject.AddComponent<Image>(); fi.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fi.type = Image.Type.Sliced; fi.raycastTarget = false;
            var fill = UiWidgets.Rect("Fill", holder, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-12, -12)); var fimg = fill.gameObject.AddComponent<Image>(); fimg.raycastTarget = false;
            fimg.sprite = UiTex.VGradient(UiWidgets.Lighten(col, 0.3f), UiWidgets.Darken(col, 0.2f), 32);
            fimg.type = Image.Type.Filled; fimg.fillMethod = Image.FillMethod.Horizontal; fimg.fillOrigin = right ? 0 : 1; fimg.fillAmount = ParseFill(text);
            UiWidgets.Label(holder, text, 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, right ? Hex("#ffeae6") : Hex("#eaf1ff"));
            float crestX = right ? 720 : -720;
            var crest = UiWidgets.Rect("Crest", holder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(crestX, 0), new Vector2(92, 92));
            var cri = crest.gameObject.AddComponent<Image>(); cri.raycastTarget = false; cri.sprite = UiTex.Disc(UiWidgets.Darken(col, 0.2f), 64);
            var crim = UiWidgets.Rect("CrestRim", crest, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var crm = crim.gameObject.AddComponent<Image>(); crm.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); crm.type = Image.Type.Sliced; crm.raycastTarget = false;
            var cg = UiWidgets.Rect("CrestGlyph", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 44)); var cgi = cg.gameObject.AddComponent<Image>(); cgi.raycastTarget = false; cgi.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
        }

        private static float ParseFill(string text)
        {
            // "8,750 / 10,000" → 0.875 (display approximation).
            var parts = text.Split('/'); if (parts.Length != 2) return 1f;
            float cur = ParseNum(parts[0]), max = ParseNum(parts[1]);
            return max > 0 ? Mathf.Clamp01(cur / max) : 1f;
        }
        private static float ParseNum(string s) { float.TryParse(s.Replace(",", "").Trim(), out var v); return v; }

        private static RectTransform Chip(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = UiWidgets.Rect("Chip", parent, anchor, anchor, pos, size);
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.95f), UiTheme.A(UiTheme.Obsidian, 0.95f), 32);
            var fr = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fi = fr.gameObject.AddComponent<Image>(); fi.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); fi.type = Image.Type.Sliced; fi.raycastTarget = false;
            return rt;
        }

        private static void IconIn(RectTransform chip, Color col, float size = 40f)
        {
            var ic = UiWidgets.Rect("Icon", chip, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(size * 0.9f, 0), new Vector2(size, size));
            var img = ic.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Disc(col, 48);
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
