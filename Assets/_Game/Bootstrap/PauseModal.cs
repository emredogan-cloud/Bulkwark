// BULWARK — PAUSE MODAL (UI Construction Bible · 11). Presentation-only, REMOVABLE.
//
// In-match pause over the frozen battlefield (design/PauseModalDesign.png). Forensic build per Sections A–O: a
// raycast-absorbing dim scrim + a centred ornate gold-framed dark panel with corner bosses + a gem finial, a
// serif gold-bevel "PAUSED" title, and three full-width stacked capsules — RESUME (blue, primary, default glow),
// SETTINGS (dark steel), SURRENDER (oxblood, danger). Opening sets Time.timeScale=0 (the one permitted §12 write);
// Resume restores it. Surrender is a presentation-only abandonment (→ Defeat) and writes NOTHING to the sim.
// NO ECS/gameplay/balance change.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-11 in-match pause modal. Presentation-only (§12: Time.timeScale only).</summary>
    public sealed class PauseModal : UiScreen
    {
        private CanvasGroup _panelCg;

        protected override void Build()
        {
            // Scrim: full-bleed, raycast ON (absorbs outside taps; battlefield stays frozen beneath).
            UiWidgets.Stretch("Scrim_Dim", Rect, UiTheme.A(UiTheme.Vignette, 0.62f));

            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.sizeDelta = new Vector2(796, 713); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();

            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(796, 713), Hex("#0c0e14"), false, 26f);
            CornerBosses(panel, 796, 713);
            // Gem finial (violet/blue) crowning the top-centre.
            var gem = UiWidgets.Rect("Panel_GemFinial", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 4), new Vector2(70, 70));
            var gimg = gem.gameObject.AddComponent<Image>(); gimg.raycastTarget = false; gimg.sprite = UiTex.Diamond(Hex("#6f5bff"), 48); gem.gameObject.AddComponent<PulseScale>();
            UiWidgets.Glow(gem, UiTheme.A(Hex("#6f5bff"), 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150));

            UiWidgets.TitleLabel(field, "PAUSED", 100, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), Vector2.zero, new Vector2(700, 130), TextAnchor.MiddleCenter, Hex("#f7e7b0"), UiTheme.Gold);

            CapsuleButton(field, "RESUME", 0.55f, UiTheme.IronBlue, Hex("#eaf2ff"), Resume, true);
            CapsuleButton(field, "SETTINGS", 0.37f, Hex("#20242e"), Hex("#ecd9a6"), () => Router.Show<SettingsScreen>(), false);
            CapsuleButton(field, "SURRENDER", 0.19f, UiTheme.Oxblood, Hex("#ffe6df"), Surrender, false);
        }

        private void CapsuleButton(Transform parent, string label, float anchorY, Color body, Color textCol, UnityEngine.Events.UnityAction onClick, bool primary)
        {
            var rt = UiWidgets.Rect("Btn_" + label, parent, new Vector2(0.5f, anchorY), new Vector2(0.5f, anchorY), Vector2.zero, new Vector2(620, 113));
            if (primary)
            {
                var gl = UiWidgets.Glow(rt, UiTheme.A(UiTheme.IronBlueHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 190));
                var pg = gl.gameObject.AddComponent<PulseGraphic>(); pg.target = gl; pg.min = 0.3f; pg.max = 0.65f; pg.period = 1.8f;
            }
            var bimg = rt.gameObject.AddComponent<Image>(); bimg.sprite = UiTex.VGradient(UiWidgets.Lighten(body, 0.28f), UiWidgets.Darken(body, 0.3f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = bimg;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var rimg = rim.gameObject.AddComponent<Image>(); rimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;
            var fl = UiWidgets.Finial(rt, new Vector2(0, 0.5f), new Vector2(46, 0), 26f); fl.color = UiTheme.GoldHi; fl.raycastTarget = false;
            var fr = UiWidgets.Finial(rt, new Vector2(1, 0.5f), new Vector2(-46, 0), 26f); fr.color = UiTheme.GoldHi; fr.raycastTarget = false;
            var lbl = UiWidgets.Label(rt, label, 43, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, textCol);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
        }

        private void CornerBosses(Transform parent, float w, float h)
        {
            Vector2[] corners = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            foreach (var c in corners)
            {
                var b = UiWidgets.Rect("Boss", parent, c, c, new Vector2((c.x < 0.5f ? 1 : -1) * 18, (c.y < 0.5f ? 1 : -1) * 18), new Vector2(54, 54));
                var img = b.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            }
        }

        public override void OnShow()
        {
            Time.timeScale = 0f; // freeze (the one permitted §12 write)
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(SeatPanel());
        }

        private IEnumerator SeatPanel()
        {
            if (_panelCg == null) yield break;
            float t = 0f; const float d = 0.22f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, EaseOutBack(k)); yield return null; }
            _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one;
        }

        private void Resume()
        {
            Time.timeScale = 1f; // restore (never leave it at 0 after Resume)
            Router.Pop();
        }

        // Confirm-gated surrender (Bible-11: deliberate, never the default/back action) via the shared modal sheet.
        private void Surrender()
        {
            ConfirmModalScreen.Mode = ConfirmModalScreen.Variant.Confirm;
            ConfirmModalScreen.Title = "SURRENDER";
            ConfirmModalScreen.Message = "Surrender this battle? Your statue will fall.";
            ConfirmModalScreen.ConfirmText = "SURRENDER";
            ConfirmModalScreen.CancelText = "CANCEL";
            ConfirmModalScreen.OnConfirm = MatchPresentation.Surrender; // presentation-only abandonment (§12)
            Router.Show<ConfirmModalScreen>();
        }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f); }
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
