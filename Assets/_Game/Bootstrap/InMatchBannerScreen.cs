// BULWARK — IN-MATCH BANNER (UI Construction Bible · 10). Presentation-only, REMOVABLE.
//
// Forensic build of design/InMatchBannerDesign.png per Sections A–O: a transient, NON-MODAL event banner over the
// live HUD — a top-centre ornate gold plaque "WAVE 12: THE DEAD AWAKEN" with a horned-skull finial + two hanging
// red cloth pennants, a "02:14" countdown plate, and blue/red battlefield path arrows — with the full Bible-08/09
// chrome (gold 450, supply 3/8, HP 10,000/10,000 & 8,300/10,000, army 42/50, costs 60/90/75/120/150) visible
// beneath (scrim/arrows raycast-OFF; the sim is NOT paused). Presentation/validation screen with the mock's static
// values; the live event/timer + sim-vector arrows are gameplay reconciled outside the §12 UI boundary (Section N).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-10 in-match event banner (presentation/validation; non-modal). Presentation-only.</summary>
    public sealed class InMatchBannerScreen : UiScreen
    {
        private Text _timer;
        private float _seconds = 134f; // 02:14 (presentation countdown)

        protected override void Build()
        {
            UiWidgets.Backdrop(Rect, "banner");

            // Full HUD chrome beneath (this mockup's spell-HUD-variant values).
            InMatchChrome.Build(Rect, SafeContent, () => Router.Pop(), 450, "3/8", "10,000 / 10,000", "8,300 / 10,000", "42/50", new[] { 60, 90, 75, 120, 150 });

            // Path arrows (blue = player advance, red = enemy advance) — raycast off, over the field.
            PathArrow(new Vector2(0.32f, 0.45f), 18f, UiTheme.IronBlueHi);
            PathArrow(new Vector2(0.40f, 0.38f), 8f, UiTheme.IronBlueHi);
            PathArrow(new Vector2(0.68f, 0.45f), 162f, UiTheme.Ember2);
            PathArrow(new Vector2(0.60f, 0.38f), 172f, UiTheme.Ember2);

            // Top soft scrim (raycast off) so the title pops without blocking the HUD.
            var scrim = UiWidgets.Rect("Scrim_BannerTop", Rect, new Vector2(0, 0.7f), new Vector2(1, 1), Vector2.zero, Vector2.zero); scrim.offsetMin = Vector2.zero; scrim.offsetMax = Vector2.zero;
            var si = scrim.gameObject.AddComponent<Image>(); si.raycastTarget = false; si.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Vignette, 0.45f), UiTheme.A(UiTheme.Vignette, 0f), 32);

            // Cloth pennants (hanging from strip ends).
            Pennant(0.27f); Pennant(0.73f);

            // Banner strip (ornate plaque + skull finial + title), top-centre.
            var strip = UiWidgets.Rect("BannerGroup", SafeContent, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), Vector2.zero, new Vector2(1076, 119));
            UiWidgets.OrnateFrame(strip, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1076, 119), Hex("#1a120c"), true, 16f);
            var skull = UiWidgets.Rect("SkullFinial", strip, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 14), new Vector2(60, 60)); var ski = skull.gameObject.AddComponent<Image>(); ski.raycastTarget = false; ski.sprite = UiTex.Diamond(Hex("#cdbfa0"), 48);
            UiWidgets.Glow(skull, UiTheme.A(UiTheme.Oxblood, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120));
            UiWidgets.TitleLabel(strip, "WAVE 12: THE DEAD AWAKEN", 60, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(940, 90), TextAnchor.MiddleCenter, Hex("#f8e9b4"), UiTheme.Gold, false);

            // Timer plate beneath the strip.
            var tp = UiWidgets.Card(SafeContent, new Vector2(0.5f, 0.825f), new Vector2(0.5f, 0.825f), Vector2.zero, new Vector2(270, 56), UiTheme.A(Hex("#0c0e14"), 0.85f));
            var clock = UiWidgets.Rect("Clock", tp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(36, 36)); var ci = clock.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(UiTheme.Gold, 48);
            _timer = UiWidgets.Label(tp, "02:14", 32, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(30, 0), Vector2.zero, TextAnchor.MiddleCenter, Hex("#ffd98a"));

            // A back affordance for this standalone presentation screen.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
        }

        private void Pennant(float fx)
        {
            var p = UiWidgets.Rect("Banner_Cloth", SafeContent, new Vector2(fx, 0.945f), new Vector2(fx, 0.945f), new Vector2(0, -90), new Vector2(140, 360));
            var img = p.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.VGradient(Hex("#8a241c"), Hex("#5a1714"), 64);
            var trim = UiWidgets.Rect("Trim", p, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var ti = trim.gameObject.AddComponent<Image>(); ti.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 4); ti.type = Image.Type.Sliced; ti.raycastTarget = false;
            p.gameObject.AddComponent<PulseScale>().period = 2.2f;
        }

        private void PathArrow(Vector2 anchor, float angle, Color col)
        {
            var rt = UiWidgets.Rect("PathArrow", Rect, anchor, anchor, Vector2.zero, new Vector2(300, 26));
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.HGradient(UiTheme.A(col, 0f), UiTheme.A(col, 0.7f), 64);
            var head = UiWidgets.Rect("Head", rt, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(8, 0), new Vector2(36, 36)); var hi = head.gameObject.AddComponent<Image>(); hi.raycastTarget = false; hi.sprite = UiTex.Diamond(col, 32);
        }

        public override void OnShow() => AudioManager.Instance?.PlayBattleMusic();

        private void Update()
        {
            if (_timer == null) return;
            _seconds = Mathf.Max(0f, _seconds - Time.unscaledDeltaTime);
            int s = Mathf.CeilToInt(_seconds);
            _timer.text = $"{s / 60:00}:{s % 60:00}";
            if (_seconds <= 30f) { var c = Hex("#ff8a5a"); c.a = 0.7f + 0.3f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)); _timer.color = c; }
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
