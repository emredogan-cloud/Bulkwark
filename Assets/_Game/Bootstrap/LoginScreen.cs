// BULWARK — LOGIN / AUTH SCREEN (UI Implementation · WP-03). Presentation-only, REMOVABLE.
//
// Landscape first-run login on the UiRouter shell (design/LoginAuthDesign.png): WELCOME WARRIOR card with
// PLAY AS GUEST + social-sign-in placeholders + a Terms/Privacy line. Guest proceeds to Loading. Real auth
// (IBackendClient.AuthenticateAsync / Google/FB/Apple SDKs) is a GATE-3 / SDK integration deferred while
// GATE-1 recovery is active — so social buttons are PLACEHOLDERS that proceed as guest with a notice.
// NO ECS/gameplay/backend call (the Services assembly is not bound here; guest proceed is the stub path).

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-03 landscape login/auth. Presentation-only (guest + social placeholders).</summary>
    public sealed class LoginScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");

            var panel = UiWidgets.Panel(SafeContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1040, 760), UiWidgets.PanelCol);
            var p = panel.transform;

            UiWidgets.LabelAt(p, "WELCOME, WARRIOR", 56, new Vector2(0.5f, 0.87f), new Vector2(920, 80), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.Button(p, "PLAY AS GUEST", new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(780, 118), UiWidgets.IronBlue, Proceed, 46);
            UiWidgets.LabelAt(p, "— or continue with —", 30, new Vector2(0.5f, 0.55f), new Vector2(700, 50), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.6f));
            UiWidgets.Button(p, "Continue with Google",   new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(780, 92), new Color(0.92f, 0.92f, 0.94f), SocialPlaceholder, 32);
            UiWidgets.Button(p, "Continue with Facebook", new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(780, 92), new Color(0.23f, 0.35f, 0.60f), SocialPlaceholder, 32);
            UiWidgets.Button(p, "Continue with Apple",    new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), Vector2.zero, new Vector2(780, 92), new Color(0.16f, 0.16f, 0.18f), SocialPlaceholder, 32);
            UiWidgets.LabelAt(p, "By continuing you agree to the Terms of Service  &  Privacy Policy", 24, new Vector2(0.5f, 0.06f), new Vector2(940, 40), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.5f));
        }

        private void Proceed() => Router.Replace<LoadingScreen>();

        private void SocialPlaceholder()
        {
            Router.Toast("Social sign-in coming soon — continuing as guest");
            Proceed();
        }
    }
}
