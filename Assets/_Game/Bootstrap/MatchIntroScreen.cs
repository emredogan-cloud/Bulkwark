// BULWARK — MATCH INTRO / VS (UI Construction Bible · 07). Presentation-only, REMOVABLE.
//
// Forensic build of design/MatchIntroDesign.png per Sections A–O: a cinematic diptych "fight card" — cold blue
// Iron Pact hero LEFT vs hot red Ashen Horde hero RIGHT, split by a luminous blue-lightning↔orange-fire seam; a
// top-centre ornate gold banner with the mode + map ("CLASSIC / STONEHOLD PASS"); a giant gold "VS" focal emblem;
// two bottom faction plates (crest + name + motto, cool-left / warm-right tints); and a centred coaching Tip.
// A transient hand-off card: tap anywhere (input gated until the build-in finishes) or auto-advance → the existing
// battle via MatchPresentation.Begin(). Reads/writes NOTHING from ECS (the match is configured by the caller §12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-07 landscape pre-battle VS card. Presentation-only.</summary>
    public sealed class MatchIntroScreen : UiScreen
    {
        private const float Dwell = 3.0f;     // min read time before auto-advance (Bible: 2.5–3.5 s)
        private const float InputGate = 0.85f; // taps swallowed until the card is assembled
        private float _t;
        private bool _started;

        protected override void Build()
        {
            // ---- BG_FullBleed: cold-left / hot-right fields + seam (FX live here, never inside SafeArea) ----
            var coldRt = UiWidgets.Rect("BG_Left_ColdField", Rect, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            coldRt.anchorMin = Vector2.zero; coldRt.anchorMax = new Vector2(0.5f, 1f); coldRt.offsetMin = Vector2.zero; coldRt.offsetMax = Vector2.zero;
            var cold = coldRt.gameObject.AddComponent<Image>(); cold.raycastTarget = false; cold.sprite = UiTex.HGradient(Hex("#10131c"), Hex("#1d2740"), 64);
            var hotRt = UiWidgets.Rect("BG_Right_HotField", Rect, new Vector2(0.5f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            hotRt.anchorMin = new Vector2(0.5f, 0f); hotRt.anchorMax = Vector2.one; hotRt.offsetMin = Vector2.zero; hotRt.offsetMax = Vector2.zero;
            var hot = hotRt.gameObject.AddComponent<Image>(); hot.raycastTarget = false; hot.sprite = UiTex.HGradient(Hex("#5a1e12"), Hex("#1a0d09"), 64);

            // Hero placeholders (blue knight left, red warlord right) — flagged placeholder art (00/01).
            Hero("Hero_Left_IronPact", "unit_player", UiTheme.IronBlueHi, 0.0f, 0.23f);
            Hero("Hero_Right_AshenHorde", "unit_ai", UiTheme.Ember2, 1.0f, 0.77f);

            // Seam (blue plasma ↔ orange fire collision at centre — brightest vertical axis).
            var seam = UiWidgets.Rect("Seam_EnergySplit", Rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 1080));
            var seamImg = seam.gameObject.AddComponent<Image>(); seamImg.raycastTarget = false; seamImg.sprite = UiTex.HGradient(UiTheme.A(Hex("#5fa8ff"), 0.0f), UiTheme.A(Hex("#ff8a2a"), 0.0f), 64);
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#5fa8ff"), 0.5f), new Vector2(0.47f, 0.5f), new Vector2(0.47f, 0.5f), Vector2.zero, new Vector2(420, 1100), 1.4f);
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#ff9a3a"), 0.55f), new Vector2(0.53f, 0.5f), new Vector2(0.53f, 0.5f), Vector2.zero, new Vector2(420, 1100), 1.4f);
            var sparkHost = UiWidgets.Rect("SeamSparks", Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 360), new Vector2(360, 720)); sparkHost.gameObject.AddComponent<EmberField>().count = 14;
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.GoldHi, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 900), 1.6f); // god-ray bloom on seam
            UiWidgets.Vignette(Rect, 0.55f);

            // ---- TopBanner (mode / map) ----
            var banner = UiWidgets.Rect("TopBanner", SafeContent, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), Vector2.zero, new Vector2(720, 150));
            UiWidgets.OrnateFrame(banner, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 150), UiTheme.FieldDark, true, 16f);
            UiWidgets.TitleLabel(banner, MatchPresentation.Mode.ToUpperInvariant(), 60, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(640, 80), TextAnchor.MiddleCenter, Hex("#f7e7b0"), UiTheme.Gold);
            UiWidgets.Label(banner, UiTheme.Track("STONEHOLD PASS", 2), 22, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), Vector2.zero, new Vector2(640, 40), TextAnchor.MiddleCenter, Hex("#cdb784"));

            // ---- Giant VS (largest, brightest glyph) ----
            UiWidgets.Glow(SafeContent, UiTheme.A(UiTheme.Ember, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 420), 1.6f);
            var vs = UiWidgets.TitleLabel(SafeContent, "VS", 178, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 260), TextAnchor.MiddleCenter, Hex("#fff2c0"), Hex("#e2a93a"), false);
            vs.gameObject.AddComponent<PulseScale>().period = 2f;

            // ---- Faction plates (cool-left / warm-right) ----
            FactionPlate(false, "IRON PACT", "STRENGTH. HONOR. UNITY.", UiTheme.IronBlue, Hex("#b9c6e6"));
            FactionPlate(true, "ASHEN HORDE", "STRENGTH IN CHAOS.\nGLORY IN CONQUEST.", UiTheme.Oxblood, Hex("#e3b6a4"));

            // ---- Tip (centred, between the plates) ----
            UiWidgets.Label(SafeContent, "Tip: Upgrade your Units and Commanders\nto dominate the battlefield.", 21, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), Vector2.zero, new Vector2(796, 80), TextAnchor.MiddleCenter, UiTheme.Parchment);

            // ---- Tap catcher (top-most, full-bleed; invisible — no drawn "tap" label) ----
            var tap = UiWidgets.Stretch("TapToContinue", Rect, new Color(0, 0, 0, 0));
            var btn = tap.gameObject.AddComponent<Button>(); btn.transition = Selectable.Transition.None; btn.targetGraphic = tap;
            btn.onClick.AddListener(() => { if (_t >= InputGate) Go(); });
        }

        private void Hero(string name, string sprite, Color rim, float anchorX, float pivotless)
        {
            float w = 0.42f * 2340f, h = 0.78f * 1080f;
            var rt = UiWidgets.Rect(name, SafeContent, new Vector2(anchorX, 0f), new Vector2(anchorX, 0f), new Vector2(anchorX < 0.5f ? w * 0.5f + 40 : -(w * 0.5f + 40), h * 0.5f), new Vector2(w, h));
            UiWidgets.Glow(rt, UiTheme.A(rim, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h), 1.7f);
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.preserveAspect = true;
            var s = Spr(sprite); if (s != null) { img.sprite = s; img.color = UiWidgets.Lighten(rim, 0.1f); } else img.color = UiTheme.A(rim, 0.5f);
        }

        private void FactionPlate(bool right, string nameText, string motto, Color body, Color mottoTint)
        {
            float w = 0.40f * 2340f, h = 0.135f * 1080f;
            float ax = right ? 0.985f : 0.015f;
            var plate = UiWidgets.Card(SafeContent, new Vector2(ax, 0.11f), new Vector2(ax, 0.11f), new Vector2(right ? -w * 0.5f : w * 0.5f, 0), new Vector2(w, h), UiTheme.A(Hex("#0c0e14"), 0.78f));
            // crest disc at inner end
            float cd = 0.085f * 1080f;
            float crestX = right ? w * 0.5f - cd * 0.7f : -w * 0.5f + cd * 0.7f;
            var crest = UiWidgets.Rect("Crest", plate, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(crestX, 0), new Vector2(cd, cd));
            var cimg = crest.gameObject.AddComponent<Image>(); cimg.raycastTarget = false; cimg.sprite = UiTex.Disc(UiWidgets.Darken(body, 0.3f), 64);
            var crim = UiWidgets.Rect("CrestRim", crest, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var crimg = crim.gameObject.AddComponent<Image>(); crimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); crimg.type = Image.Type.Sliced; crimg.raycastTarget = false;
            var crestGly = UiWidgets.Rect("CrestGlyph", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(cd * 0.5f, cd * 0.5f)); var cg = crestGly.gameObject.AddComponent<Image>(); cg.raycastTarget = false; cg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
            // name + motto (text on the far side from the crest)
            var align = right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            float textInset = cd + 40f;
            var nameRt = UiWidgets.TitleLabel(plate, nameText, 43, new Vector2(0, 0.62f), new Vector2(1, 0.62f), Vector2.zero, Vector2.zero, align, Hex("#f3e2a6"), Hex("#c79a44"), false);
            var nrt = (RectTransform)nameRt.transform; nrt.offsetMin = new Vector2(right ? 30 : textInset, 0); nrt.offsetMax = new Vector2(right ? -textInset : -30, 0);
            var mottoLbl = UiWidgets.Label(plate, motto, 19, new Vector2(0, 0.28f), new Vector2(1, 0.28f), Vector2.zero, Vector2.zero, align, mottoTint);
            var mrt = (RectTransform)mottoLbl.transform; mrt.offsetMin = new Vector2(right ? 30 : textInset, 0); mrt.offsetMax = new Vector2(right ? -textInset : -30, 0);
        }

        public override void OnShow() => AudioManager.Instance?.PlayBattleMusic();

        private void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_t >= Dwell) Go();
        }

        private void Go()
        {
            if (_started) return;
            _started = true;
            StartCoroutine(Advance());
        }

        // OnHide: VS flash → card scale-up + fade → start the existing battle.
        private IEnumerator Advance()
        {
            float t = 0f; const float fd = 0.22f;
            while (t < fd && Group != null) { t += Time.unscaledDeltaTime; Group.alpha = 1f - Mathf.Clamp01(t / fd); if (Rect != null) Rect.localScale = Vector3.one * Mathf.Lerp(1f, 1.04f, t / fd); yield return null; }
            MatchPresentation.Begin();
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
        private static Sprite Spr(string key) => PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(key) : null;
    }
}
