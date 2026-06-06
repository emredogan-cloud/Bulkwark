// BULWARK — UI NAVIGATION SHELL (UI Implementation · WP-00 Landscape Foundation; refined in WP-01).
// Presentation-only, REMOVABLE.
//
// The navigation-shell foundation the WP-01+ screens are built on (reports/ui-production/packages/
// WP-00_FOUNDATION.md / WP-01_NAVIGATION.md). Two presentation-only types in one file (mirrors the existing
// AnimationManager.cs multi-MonoBehaviour precedent):
//
//   • UiScreen  — abstract base for a full-screen panel. Its root RectTransform is FULL-BLEED (full-screen
//                 backgrounds parent there and may extend under a side cutout); interactive content parents
//                 under SafeContent, a safe-area-inset child (SafeAreaFitter). Subclasses build in Build().
//   • UiRouter  — a DontDestroyOnLoad screen-stack over ONE landscape overlay Canvas (CanvasScaler via
//                 UiScaling, sortingOrder 200 — above UiFlow's 100 and BattleHud's 50). CanvasGroup fade
//                 transitions on UNSCALED time (menus run at Time.timeScale = 0; mirrors UiFlow.FadeIn).
//                 Lazy singleton: INERT until a screen is shown (zero footprint until WP-01 uses it).
//
// WP-01 REFINEMENT (vs the initial WP-00 cut): safe-area handling moved from a single router-owned SafeArea
// root to a PER-SCREEN SafeContent child, so each screen can have a full-bleed background (splash key art must
// fill the screen under the cutout) while keeping interactive content inside the safe area. Added PopFaded()
// for a fade-out transition (e.g. Splash → Main Menu).
//
// NO gameplay/ECS/balance/AI/economy impact (presentation §12): it only builds/animates uGUI. Screens bind to
// game services only through their own code (and, while GATE-1 recovery is active, to STUBS — never gameplay).
// Deleting this file removes the shell 100%.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Bulwark.Bootstrap
{
    /// <summary>Base class for a full-screen UI panel managed by <see cref="UiRouter"/>. Presentation-only.</summary>
    public abstract class UiScreen : MonoBehaviour
    {
        /// <summary>The router that owns this screen (set on Bind).</summary>
        protected UiRouter Router { get; private set; }
        /// <summary>This screen's CanvasGroup (used for fade transitions).</summary>
        public CanvasGroup Group { get; private set; }
        /// <summary>Full-bleed screen root — parent FULL-SCREEN BACKGROUNDS here (may extend under the cutout).</summary>
        protected RectTransform Rect { get; private set; }
        /// <summary>Safe-area-inset content root — parent INTERACTIVE / important content here.</summary>
        protected RectTransform SafeContent { get; private set; }

        /// <summary>Called once by the router after the GameObject + RectTransform + CanvasGroup exist.</summary>
        internal void Bind(UiRouter router)
        {
            Router = router;
            Rect = transform as RectTransform;
            Group = GetComponent<CanvasGroup>();
            if (Group == null) Group = gameObject.AddComponent<CanvasGroup>();

            // Safe-area content root (full-stretch child of the full-bleed screen root).
            var saGo = new GameObject("SafeContent", typeof(RectTransform));
            SafeContent = (RectTransform)saGo.transform;
            SafeContent.SetParent(Rect, false);
            SafeContent.anchorMin = Vector2.zero;
            SafeContent.anchorMax = Vector2.one;
            SafeContent.offsetMin = Vector2.zero;
            SafeContent.offsetMax = Vector2.zero;
            saGo.AddComponent<SafeAreaFitter>();

            Build();
            // Keep interactive content above any full-bleed background art a screen added under Rect in Build()
            // (SafeContent is created before Build, so it would otherwise sit beneath the background).
            SafeContent.SetAsLastSibling();
        }

        /// <summary>Build the screen's uGUI here (called once, after <see cref="Bind"/>). Subclass responsibility.
        /// NOTE for WP-01+ authors: do screen setup in Build(), NOT in Awake() — Router/Group/Rect/SafeContent
        /// are not bound until Bind() runs (right after AddComponent), so they are null during a subclass Awake().</summary>
        protected abstract void Build();

        /// <summary>Invoked when this screen becomes the active top of the stack.</summary>
        public virtual void OnShow() { }

        /// <summary>Invoked when this screen is covered by a push or removed by a pop.</summary>
        public virtual void OnHide() { }
    }

    /// <summary>Screen-stack navigation shell over one landscape overlay Canvas. Presentation-only.</summary>
    public sealed class UiRouter : MonoBehaviour
    {
        private const int ShellSortingOrder = 200; // above UiFlow menu canvas (100) and BattleHud (50)
        private const float FadeDuration = 0.2f;   // unscaled (menus run at Time.timeScale = 0)

        private static UiRouter _instance;

        /// <summary>The shared router. Lazily created on first access (inert until a screen is shown).</summary>
        public static UiRouter Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("UiRouter");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<UiRouter>(); // Awake builds the canvas
                }
                return _instance;
            }
        }

        /// <summary>True if a router instance already exists (without creating one).</summary>
        public static bool Exists => _instance != null;

        /// <summary>The shell's overlay Canvas.</summary>
        public Canvas Canvas { get; private set; }
        /// <summary>The current top screen, or null if the stack is empty.</summary>
        public UiScreen Top => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        private readonly List<UiScreen> _stack = new List<UiScreen>(8);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            BuildCanvas();
        }

        // ---------------- canvas ----------------
        private void BuildCanvas()
        {
            Canvas = gameObject.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = ShellSortingOrder;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            UiScaling.Configure(scaler); // landscape 2340x1080, match height (WP-00 / freeze D1)

            gameObject.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var es = new GameObject("EventSystem");
            DontDestroyOnLoad(es);
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ---------------- stack operations ----------------
        /// <summary>Create + push a screen of type <typeparamref name="T"/> (full-bleed under the Canvas) and fade it in.</summary>
        public T Show<T>() where T : UiScreen
        {
            var go = new GameObject("Screen_" + typeof(T).Name, typeof(RectTransform), typeof(CanvasGroup));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false); // full-bleed under the Canvas root
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Full-screen screens cover the one below — hide it while it is not the top.
            var prev = Top;
            if (prev != null) { prev.OnHide(); prev.gameObject.SetActive(false); }

            var screen = go.AddComponent<T>();
            _stack.Add(screen);
            screen.Bind(this);   // builds the screen's uGUI
            screen.OnShow();
            StartCoroutine(FadeIn(screen.Group));
            return screen;
        }

        /// <summary>Pop the top screen (destroying it) and reveal the one beneath (if any).</summary>
        public void Pop()
        {
            if (_stack.Count == 0) return;
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            top.OnHide();
            Destroy(top.gameObject);

            var now = Top;
            if (now != null)
            {
                now.gameObject.SetActive(true);
                now.OnShow();
                StartCoroutine(FadeIn(now.Group));
            }
        }

        /// <summary>Fade the top screen out, then pop it. Reveals whatever is beneath — another shell screen, or
        /// a lower-sortingOrder canvas (e.g. the legacy UiFlow menu during the screen-by-screen migration).</summary>
        public void PopFaded()
        {
            if (Top == null) return;
            StartCoroutine(FadeOutThenPop());
        }

        /// <summary>Replace the top screen with a new screen of type <typeparamref name="T"/>.</summary>
        public T Replace<T>() where T : UiScreen
        {
            if (_stack.Count > 0)
            {
                var top = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
                top.OnHide();
                Destroy(top.gameObject);
            }
            return Show<T>();
        }

        /// <summary>Pop every screen (clears the stack).</summary>
        public void Clear()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                var s = _stack[i];
                if (s != null) { s.OnHide(); Destroy(s.gameObject); }
            }
            _stack.Clear();
        }

        /// <summary>Show a transient bottom toast message (auto-fades). Used for "coming soon" / quick feedback.
        /// A lightweight foundation toast; the full WP-11 reusable component set adds richer modals.</summary>
        public void Toast(string message, float seconds = 2.0f)
        {
            var go = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 180f); rt.sizeDelta = new Vector2(1000f, 96f);
            var bg = go.GetComponent<Image>();
            var ps = UiWidgets.Spr("panel"); if (ps != null) { bg.sprite = ps; bg.type = Image.Type.Sliced; }
            bg.color = new Color(0f, 0f, 0f, 0.82f); bg.raycastTarget = false;
            UiWidgets.Label(rt, message, 36, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            StartCoroutine(ToastRoutine(go.GetComponent<CanvasGroup>(), go, seconds));
        }

        private IEnumerator ToastRoutine(CanvasGroup cg, GameObject go, float seconds)
        {
            if (cg != null) cg.alpha = 0f;
            float t = 0f;
            while (t < FadeDuration && cg != null) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(t / FadeDuration); yield return null; }
            float hold = 0f;
            while (hold < seconds) { hold += Time.unscaledDeltaTime; yield return null; }
            t = 0f;
            while (t < FadeDuration && cg != null) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(1f - t / FadeDuration); yield return null; }
            if (go != null) Destroy(go);
        }

        // Fade a screen's CanvasGroup 0 -> 1 on UNSCALED time (menus run at timeScale = 0). Mirrors UiFlow.FadeIn.
        private IEnumerator FadeIn(CanvasGroup cg)
        {
            if (cg == null) yield break;
            float t = 0f;
            cg.alpha = 0f;
            while (t < FadeDuration && cg != null)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(t / FadeDuration);
                yield return null;
            }
            if (cg != null) cg.alpha = 1f;
        }

        // Fade the current top out (unscaled), then Pop it.
        private IEnumerator FadeOutThenPop()
        {
            var screen = Top;
            var cg = screen != null ? screen.Group : null;
            float t = 0f;
            while (t < FadeDuration && cg != null)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(1f - t / FadeDuration);
                yield return null;
            }
            if (cg != null) cg.alpha = 0f;
            // Pop only if it is still the top (nothing else was pushed during the fade).
            if (Top == screen) Pop();
        }
    }
}
