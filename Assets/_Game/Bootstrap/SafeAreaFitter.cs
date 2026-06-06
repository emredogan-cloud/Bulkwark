// BULWARK — SAFE AREA FITTER (UI Implementation · WP-00 Landscape Foundation). Presentation-only, REMOVABLE.
//
// Reusable component that insets its RectTransform to the device safe area (Screen.safeArea), so UI content
// stays clear of notches / rounded corners / camera cutouts / gesture bars. In LANDSCAPE the cutout moves to a
// SIDE (and flips between LandscapeLeft/Right), so a safe-area inset is required on every interactive root.
// This generalizes the inline logic already proven in BattleHud.ApplySafeArea() into a drop-in component the
// UiRouter and every WP-01+ screen reuse. It re-applies whenever safe area / orientation / resolution changes.
//
// USAGE (code-built UI): make a child RectTransform stretched to its parent, AddComponent<SafeAreaFitter>(),
// and parent INTERACTIVE content under it. Full-bleed backgrounds stay OUTSIDE it (they may extend under the
// cutout). NO gameplay/ECS impact (presentation §12).

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>Insets a RectTransform to Screen.safeArea, re-applying on change. Presentation-only (WP-00).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafe;
        private int _lastW, _lastH;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rt = transform as RectTransform;
            Apply();
        }

        private void OnEnable() { Apply(); }

        private void Update()
        {
            // Re-apply on rotation (left<->right landscape), resolution / multi-window, or safe-area change.
            if (Screen.safeArea != _lastSafe || Screen.width != _lastW || Screen.height != _lastH
                || Screen.orientation != _lastOrientation)
                Apply();
        }

        /// <summary>Inset the RectTransform to the current safe area (anchors expressed in [0,1] of the screen).</summary>
        public void Apply()
        {
            if (_rt == null) _rt = transform as RectTransform;
            if (_rt == null) return;

            Rect sa = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width);
            float h = Mathf.Max(1, Screen.height);

            _rt.anchorMin = new Vector2(sa.xMin / w, sa.yMin / h);
            _rt.anchorMax = new Vector2(sa.xMax / w, sa.yMax / h);
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;

            _lastSafe = sa;
            _lastW = Screen.width;
            _lastH = Screen.height;
            _lastOrientation = Screen.orientation;
        }
    }
}
