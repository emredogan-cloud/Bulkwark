// BULWARK — END FLOW: VICTORY (12) / DEFEAT (13) (UI Construction Bible). Presentation-only, REMOVABLE.
//
// Post-match result. The verdict is the REAL ECS MatchState.Outcome (read read-only by UiFlow, handed in via
// MatchPresentation.OnMatchDecided → PendingVictory) — or a presentation-only Surrender (Defeat). This builds
// BOTH Bible variants forensically:
//   • VICTORY (12): gold+royal-blue, crown+crossed-swords crest, "VICTORY", subtitle, gem+time stat row, a
//     closed reward chest in a gold glow halo + "REWARD", single blue CONTINUE. (Reward/time are DISPLAY-ONLY
//     stubs — the grant is server-authoritative; the UI never writes a balance, §12.)
//   • DEFEAT (13): somber/cold/desaturated, austere crown, "DEFEAT", subtitle, NO reward, two co-equal buttons
//     RETRY (oxblood) + CONTINUE (blue). RETRY degrades to a notice+menu (battle-reset is a gameplay blocker).
// NO ECS/gameplay/backend (outcome consumed read-only; CONTINUE/RETRY go through the existing presentation seam).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-12/13 Victory & Defeat result. Presentation-only (outcome read-only via MatchState).</summary>
    public sealed class EndScreen : UiScreen
    {
        /// <summary>Set by MatchPresentation before this screen is shown (true = Victory).</summary>
        public static bool PendingVictory = true;

        private bool _victory;
        private CanvasGroup _panelCg;

        protected override void Build()
        {
            _victory = PendingVictory;

            // ---- Full-bleed background ----
            UiWidgets.Stretch("BG_Battlefield", Rect, UiTheme.Obsidian, _victory ? "bg_victory" : "bg_defeat");
            if (!_victory) UiWidgets.Stretch("BG_Desaturate", Rect, UiTheme.A(new Color(0.23f, 0.25f, 0.28f), 0.5f)); // cold grey-blue grade
            if (_victory) UiWidgets.Glow(Rect, UiTheme.A(Hex("#f4dca0"), 0.3f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(1200, 900), 1.4f); // god-rays
            UiWidgets.Vignette(Rect, 0.6f);
            UiWidgets.Stretch("BG_DarkenScrim", Rect, new Color(0, 0, 0, _victory ? 0.38f : 0.42f));

            // ---- Result panel (centred card) ----
            float panelH = _victory ? 994 : 929;
            var panelGo = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.sizeDelta = new Vector2(1076, panelH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1076, panelH), _victory ? Hex("#14110d") : Hex("#0f1116"), false, 26f);

            BuildCrest(panel);

            // ---- Title + subtitle ----
            if (_victory)
            {
                UiWidgets.TitleLabel(field, "VICTORY", 116, new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(940, 150), TextAnchor.MiddleCenter, Hex("#fded61"), UiTheme.Gold);
                Ribbon(field, "The enemy statue has fallen!", 0.735f);
                BuildStats(field);
                BuildReward(field);
                CtaWide(field, "CONTINUE", UiTheme.IronBlue, 0.11f, MatchPresentation.OnContinue);
            }
            else
            {
                UiWidgets.TitleLabel(field, "DEFEAT", 124, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(940, 180), TextAnchor.MiddleCenter, Hex("#e8cf86"), Hex("#a98a45"));
                Ribbon(field, "Your statue has fallen.", 0.50f);
                CtaHalf(field, "RETRY", UiTheme.Oxblood, 0.27f, 0.20f, MatchPresentation.OnRetry, Hex("#fff3ec"));
                CtaHalf(field, "CONTINUE", UiTheme.IronBlue, 0.73f, 0.20f, MatchPresentation.OnContinue, Hex("#fffbf0"));
            }
        }

        private void BuildCrest(Transform panel)
        {
            var crest = UiWidgets.Rect("Crest_Group", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 10), new Vector2(360, 180));
            if (_victory)
            {
                var bl = UiWidgets.Rect("BannerL", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-90, -10), new Vector2(160, 120)); var bli = bl.gameObject.AddComponent<Image>(); bli.raycastTarget = false; bli.sprite = UiTex.VGradient(UiTheme.IronBlue, Hex("#1a347a"), 64); bl.localRotation = Quaternion.Euler(0, 0, 24);
                var br = UiWidgets.Rect("BannerR", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(90, -10), new Vector2(160, 120)); var bri = br.gameObject.AddComponent<Image>(); bri.raycastTarget = false; bri.sprite = UiTex.VGradient(UiTheme.IronBlue, Hex("#1a347a"), 64); br.localRotation = Quaternion.Euler(0, 0, -24);
                var sl = UiWidgets.Finial(crest, new Vector2(0.5f, 0.5f), new Vector2(-70, 0), 90f); sl.color = UiTheme.GoldHi; sl.transform.localRotation = Quaternion.Euler(0, 0, 35);
                var sr = UiWidgets.Finial(crest, new Vector2(0.5f, 0.5f), new Vector2(70, 0), 90f); sr.color = UiTheme.GoldHi; sr.transform.localRotation = Quaternion.Euler(0, 0, -35);
            }
            var crown = UiWidgets.Rect("Crown", crest, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(110, 110)); var ci = crown.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Diamond(_victory ? UiTheme.GoldHi : Hex("#c7ad6a"), 48);
            UiWidgets.Glow(crest, UiTheme.A(_victory ? UiTheme.GoldHi : Hex("#8a7a4a"), 0.4f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(260, 220));
        }

        private void Ribbon(Transform field, string text, float y)
        {
            var rib = UiWidgets.Card(field, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(840, 70), UiTheme.A(Hex("#2a1c0a"), 0.95f));
            UiWidgets.Label(rib, text, 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, _victory ? Hex("#e6cf9a") : Hex("#d8d0bc"));
        }

        private void BuildStats(Transform field)
        {
            // Display-only stub values from the mock (server-authoritative grant; UI never writes balance §12).
            var gemIcon = UiWidgets.Rect("Icon_Gem", field, new Vector2(0.34f, 0.62f), new Vector2(0.34f, 0.62f), new Vector2(-70, 0), new Vector2(56, 56)); var gi = gemIcon.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48);
            UiWidgets.Label(field, "1746", 42, new Vector2(0.37f, 0.62f), new Vector2(0.37f, 0.62f), new Vector2(30, 0), new Vector2(200, 60), TextAnchor.MiddleLeft, Hex("#f3ead2"));
            UiWidgets.Divider(field, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, 4, 70, UiTheme.Gold); // vertical-ish divider (thin)
            var clk = UiWidgets.Rect("Icon_Clock", field, new Vector2(0.62f, 0.62f), new Vector2(0.62f, 0.62f), new Vector2(-70, 0), new Vector2(56, 56)); var cli = clk.gameObject.AddComponent<Image>(); cli.raycastTarget = false; cli.sprite = UiTex.Disc(UiTheme.Gold, 48);
            UiWidgets.Label(field, "05:28", 42, new Vector2(0.65f, 0.62f), new Vector2(0.65f, 0.62f), new Vector2(30, 0), new Vector2(200, 60), TextAnchor.MiddleLeft, Hex("#f3ead2"));
        }

        private void BuildReward(Transform field)
        {
            var grp = UiWidgets.Rect("Reward_Group", field, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(420, 320));
            UiWidgets.Glow(grp, UiTheme.A(Hex("#ffe9a0"), 0.6f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(420, 360), 1.5f);
            var spark = UiWidgets.Rect("Sparkles", grp, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(360, 320)); var ef = spark.gameObject.AddComponent<EmberField>(); ef.count = 12; ef.color = Hex("#ffe9a0");
            var chest = UiWidgets.Rect("Reward_Chest", grp, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(230, 190));
            var chImg = chest.gameObject.AddComponent<Image>(); chImg.raycastTarget = false; chImg.sprite = UiTex.VGradient(Hex("#a05b25"), Hex("#7a4a20"), 64);
            var band = UiWidgets.Rect("Band", chest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(230, 30)); var bi = band.gameObject.AddComponent<Image>(); bi.raycastTarget = false; bi.color = Hex("#9aa0a8");
            chest.gameObject.AddComponent<PulseScale>().period = 2f;
            UiWidgets.Label(grp, UiTheme.Track("REWARD"), 24, new Vector2(0.5f, 0.02f), new Vector2(0.5f, 0.02f), Vector2.zero, new Vector2(300, 40), TextAnchor.MiddleCenter, Hex("#c8a55a"));
        }

        private void CtaWide(Transform field, string label, Color body, float y, UnityEngine.Events.UnityAction onClick)
            => UiWidgets.GemButton(field, label, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(800, 100), body, onClick, 46, true, false, TextAnchor.MiddleCenter);

        private void CtaHalf(Transform field, string label, Color body, float x, float y, UnityEngine.Events.UnityAction onClick, Color textCol)
            => UiWidgets.GemButton(field, label, new Vector2(x, y), new Vector2(x, y), Vector2.zero, new Vector2(440, 120), body, onClick, 44, false, false, TextAnchor.MiddleCenter);

        public override void OnShow()
        {
            AudioManager.Instance?.PlayEndStinger(_victory); // single fire (guarded; reset on match start)
            StartCoroutine(Reveal());
        }

        private IEnumerator Reveal()
        {
            if (_panelCg == null) yield break;
            _panelCg.alpha = 0f; float t = 0f; const float d = 0.4f;
            float from = _victory ? 0.86f : 0.92f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(from, 1f, k); yield return null; }
            _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one;
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
