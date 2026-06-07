// BULWARK — LOGIN / AUTH (UI Construction Bible · 04). Presentation-only, REMOVABLE.
//
// ⚠️ OUT-OF-FLOW / DEPRECATED. The finalized boot is NO-LOGIN (Splash → Loading → Main Menu directly). This
// screen is fully spec'd per the no-screen-skipped rule but is NEVER pushed in the boot chain (SplashScreen
// advances to LoadingScreen, not here). It may later be repurposed as an opt-in "Link Account" sheet from
// Settings. Forensic build of design/LoginAuthDesign.png: a gem-crowned banner-draped ornate gold panel headed
// "WELCOME, WARRIOR" with a cobalt PLAY AS GUEST primary, an OR-divider, three brand-accurate social pills
// (Google/Facebook/Apple), a reassurance line, a consent checkbox, and three corner utilities. NO ECS/backend
// (OAuth is stubbed/server-authoritative; §12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-04 login/auth card. OUT-OF-FLOW (never pushed in boot). Presentation-only.</summary>
    public sealed class LoginScreen : UiScreen
    {
        private bool _consent = true; // mock shows it checked
        private Image _check;
        private RectTransform _field;

        protected override void Build()
        {
            UiWidgets.Stretch("KeyArt_Base", Rect, UiTheme.Obsidian, "bg_menu");
            UiWidgets.Vignette(Rect, 0.55f);

            // ---- Central ornate panel (fx0.25–0.75, fy0.04–0.89 → centred, 980×918) ----
            var panel = UiWidgets.Rect("Panel_Group", SafeContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(980, 918));
            var drape = UiWidgets.Rect("BannerDrape", panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, 40), new Vector2(260, 360));
            var dimg = drape.gameObject.AddComponent<Image>(); dimg.raycastTarget = false; dimg.sprite = UiTex.VGradient(UiTheme.IronBlue, Hex("#1a347a"), 64);
            _field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980, 918), Hex("#212630"), false, 22f);
            var gem = UiWidgets.Rect("GemFinial", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 6), new Vector2(80, 80));
            var gimg = gem.gameObject.AddComponent<Image>(); gimg.raycastTarget = false; gimg.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48);
            gem.gameObject.AddComponent<PulseScale>();
            UiWidgets.Glow(gem, UiTheme.A(UiTheme.AmethystHi, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160, 160));

            // ---- Content rows (panel-fraction positions) ----
            UiWidgets.TitleLabel(_field, "WELCOME, WARRIOR", 50, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(860, 80), TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);
            UiWidgets.GemButton(_field, "PLAY AS GUEST", new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(640, 92), UiTheme.IronBlue, OnGuest, 40, true);

            UiWidgets.Divider(_field, new Vector2(0.5f, 0.635f), new Vector2(0.5f, 0.635f), new Vector2(-230, 0), 300);
            UiWidgets.Divider(_field, new Vector2(0.5f, 0.635f), new Vector2(0.5f, 0.635f), new Vector2(230, 0), 300);
            UiWidgets.Label(_field, UiTheme.Track("OR CONTINUE WITH", 1), 22, new Vector2(0.5f, 0.635f), new Vector2(0.5f, 0.635f), Vector2.zero, new Vector2(400, 40), TextAnchor.MiddleCenter, Hex("#b9a877"));

            SocialPill("Continue with Google",   0.535f, Color.white,    Hex("#3c4043"));
            SocialPill("Continue with Facebook", 0.435f, Hex("#1877f2"), Color.white);
            SocialPill("Continue with Apple",    0.335f, Hex("#1c1c1e"), Color.white);

            var shield = UiWidgets.Rect("Shield", _field, new Vector2(0.5f, 0.23f), new Vector2(0.5f, 0.23f), new Vector2(-250, 0), new Vector2(34, 34)); var sh = shield.gameObject.AddComponent<Image>(); sh.raycastTarget = false; sh.sprite = UiTex.Diamond(UiTheme.Gold, 32);
            UiWidgets.Label(_field, "Your progress is safe and secure", 22, new Vector2(0.5f, 0.23f), new Vector2(0.5f, 0.23f), new Vector2(20, 0), new Vector2(560, 40), TextAnchor.MiddleCenter, Hex("#c9b888"));
            BuildConsent();

            Util("SUPPORT", new Vector2(0.045f, 0.10f));
            Util("LANGUAGE", new Vector2(0.135f, 0.10f));
            Util("ACCOUNT RECOVERY", new Vector2(0.955f, 0.10f));
        }

        // Brand-accurate social pill: body colour preserved exactly; leading brand-glyph disc + label.
        private void SocialPill(string label, float y, Color body, Color textCol)
        {
            var btn = UiWidgets.Button(_field, "", new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(640, 72), body, () => OnSocial(label), 0);
            var disc = UiWidgets.Rect("Glyph", btn.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(54, 0), new Vector2(44, 44));
            var dimg = disc.gameObject.AddComponent<Image>(); dimg.raycastTarget = false; dimg.sprite = UiTex.Disc(UiWidgets.Darken(body, 0.15f), 48);
            UiWidgets.Label(btn.transform, label, 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, textCol);
        }

        private void BuildConsent()
        {
            var box = UiWidgets.Rect("Consent_Checkbox", _field, new Vector2(0.5f, 0.135f), new Vector2(0.5f, 0.135f), new Vector2(-330, 0), new Vector2(34, 34));
            var bimg = box.gameObject.AddComponent<Image>(); bimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 32, 5); bimg.type = Image.Type.Sliced;
            var btn = box.gameObject.AddComponent<Button>(); btn.targetGraphic = bimg;
            var checkRt = UiWidgets.Rect("Check", box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22, 22));
            _check = checkRt.gameObject.AddComponent<Image>(); _check.sprite = UiTex.Diamond(UiTheme.IronBlueHi, 32); _check.raycastTarget = false; _check.enabled = _consent;
            btn.onClick.AddListener(() => { _consent = !_consent; if (_check != null) _check.enabled = _consent; AudioManager.Instance?.Click(); });
            UiWidgets.Label(_field, "I have read and agree to the Terms of Service and Privacy Policy", 18, new Vector2(0.5f, 0.135f), new Vector2(0.5f, 0.135f), new Vector2(20, 0), new Vector2(620, 50), TextAnchor.MiddleLeft, Hex("#b7ad95"));
        }

        private void Util(string caption, Vector2 anchor)
            => UiWidgets.IconTile(SafeContent, caption, anchor, anchor, Vector2.zero, 56f, UiWidgets.Grey, () => Router.Toast(caption));

        private void OnGuest()
        {
            if (!_consent) { Router.Toast("Please accept the Terms of Service to continue"); return; }
            Router.Replace<MainMenuScreen>(); // only if ever enabled (never reached in boot)
        }
        private void OnSocial(string provider)
        {
            if (!_consent) { Router.Toast("Please accept the Terms of Service to continue"); return; }
            Router.Toast(provider + " — sign-in stubbed (out-of-flow)");
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
