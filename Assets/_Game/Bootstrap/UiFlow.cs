// BULWARK — UI FLOW (Asset Migration / Presentation Pass). TEMPORARY, REMOVABLE, presentation-layer only (§12).
//
// A code-built uGUI front-end that makes the app launch like a real game:
//   Splash → Main Menu → Mode Select (Classic/Tournament/Endless) → [Play] → Match → Victory/Defeat → Menu.
// It OVERLAYS the existing ECS battle (which renders underneath via SimProxyRenderer sprites + the debug HUD).
// It FREEZES the sim (Time.timeScale=0) during menus so the match actually starts when the player presses Play,
// and watches MatchState (read-only) to show Victory/Defeat. Backgrounds/buttons use the StreamingAssets
// placeholder sprites when loaded (fallback to solid colors). Built in code (no prefab YAML) for reliability.
//
// INVIOLABLE: NO ECS/sim/balance/AI/economy change. The ONLY runtime effect on the sim is Time.timeScale (a
// standard pause — the debug HUD already does this), which gates WHEN the match runs, not its rules. Placeholder
// art is © ripped → dev-only, replace before release. Deleting this file removes the front-end 100%.

using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Temporary code-built uGUI presentation flow over the ECS battle.</summary>
    public sealed class UiFlow : MonoBehaviour
    {
        private enum Screen { Splash, Menu, ModeSelect, Match, End }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("UiFlow");
            go.AddComponent<UiFlow>();
            DontDestroyOnLoad(go);
            Debug.Log("[UI] UiFlow booted.");
        }

        private Canvas _canvas;
        private Font _font;
        private Screen _screen;
        private float _splashT;
        private GameObject _panelSplash, _panelMenu, _panelMode, _panelEnd;
        private Text _endTitle, _endSummary;
        private bool _built;
        private World _w;
        private EntityManager _em;

        private static readonly Color IronBlue = new Color(0.20f, 0.45f, 0.95f);
        private static readonly Color AshRed = new Color(0.85f, 0.25f, 0.20f);
        private static readonly Color Dark = new Color(0.08f, 0.09f, 0.12f, 1f);

        private void Start()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // built-in (no font import needed)
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildCanvas();
            Time.timeScale = 0f;       // freeze the battle until the player presses Play
            Show(Screen.Splash);
            _built = true;
        }

        private bool _bgApplied;

        private void Update()
        {
            if (!_built) return;

            // Placeholder sprites load async (~1-2s after Start) — apply backgrounds once ready (one-shot).
            if (!_bgApplied && PlaceholderAssets.Instance != null && PlaceholderAssets.Instance.Ready)
            {
                SetPanelBg(_panelSplash, "bg_menu"); SetPanelBg(_panelMenu, "bg_menu"); SetPanelBg(_panelMode, "bg_menu");
                _bgApplied = true;
            }

            if (_screen == Screen.Splash)
            {
                _splashT += Time.unscaledDeltaTime;
                if (_splashT > 2.0f) Show(Screen.Menu);
                return;
            }
            if (_screen == Screen.Match)
            {
                // Watch the ECS MatchState (read-only) → reveal Victory/Defeat when decided.
                if (TryGetWorld(out var em))
                {
                    using var q = em.CreateEntityQuery(ComponentType.ReadOnly<MatchState>());
                    using var arr = q.ToComponentDataArray<MatchState>(Unity.Collections.Allocator.Temp);
                    if (arr.Length > 0 && arr[0].Outcome != MatchOutcome.Ongoing)
                        ShowEnd(arr[0].Outcome == MatchOutcome.Victory);
                }
            }
        }

        private bool TryGetWorld(out EntityManager em)
        {
            var w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) { em = default; return false; }
            _w = w; _em = w.EntityManager; em = _em; return true;
        }

        // ---------------- screen switching ----------------
        private void Show(Screen s)
        {
            _screen = s;
            if (_panelSplash) _panelSplash.SetActive(s == Screen.Splash);
            if (_panelMenu) _panelMenu.SetActive(s == Screen.Menu);
            if (_panelMode) _panelMode.SetActive(s == Screen.ModeSelect);
            if (_panelEnd) _panelEnd.SetActive(s == Screen.End);
            // Menus freeze the battle; Match/End let it run.
            Time.timeScale = (s == Screen.Match || s == Screen.End) ? 1f : 0f;
            // The textured uGUI BattleHud now provides the in-match UI, so keep the IMGUI debug overlays
            // hidden throughout the front-end (observability remains via logcat); flag Match for BattleHud.
            PresentationState.SuppressDebugHud = true;
            PresentationState.InMatch = (s == Screen.Match);
            Debug.Log("[UI] screen = " + s);
        }

        private void StartMatch(string mode)
        {
            Debug.Log("[UI] StartMatch mode=" + mode);
            Show(Screen.Match); // hides all menu panels; sim unfreezes
        }

        private void ShowEnd(bool victory)
        {
            if (_screen == Screen.End) return;
            _endTitle.text = victory ? "VICTORY" : "DEFEAT";
            _endTitle.color = victory ? new Color(1f, 0.85f, 0.2f) : AshRed;
            _endSummary.text = victory ? "The enemy statue has fallen." : "Your statue has fallen.";
            var img = _panelEnd.GetComponent<Image>();
            if (img != null) { var s = Spr(victory ? "bg_victory" : "bg_defeat"); if (s != null) { img.sprite = s; img.color = Color.white; } }
            Show(Screen.End);
        }

        // ---------------- UI construction (code-built) ----------------
        private void BuildCanvas()
        {
            var cgo = new GameObject("BulwarkCanvas");
            cgo.transform.SetParent(transform, false);
            _canvas = cgo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(transform, false);
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            _panelSplash = BuildSplash();
            _panelMenu = BuildMenu();
            _panelMode = BuildModeSelect();
            _panelEnd = BuildEnd();
        }

        private GameObject FullPanel(string name, string bgSprite, Color fallback)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            var s = Spr(bgSprite);
            if (s != null) { img.sprite = s; img.color = Color.white; img.preserveAspect = false; }
            else img.color = fallback;
            return go;
        }

        private GameObject BuildSplash()
        {
            var p = FullPanel("Splash", "bg_menu", Dark);
            Label(p.transform, "BULWARK", 120, new Vector2(0, 120), new Vector2(900, 200), TextAnchor.MiddleCenter, Color.white);
            Label(p.transform, "a mobile RTS-lite  ·  placeholder build", 36, new Vector2(0, -40), new Vector2(900, 80), TextAnchor.MiddleCenter, new Color(1,1,1,0.8f));
            return p;
        }

        private GameObject BuildMenu()
        {
            var p = FullPanel("MainMenu", "bg_menu", Dark);
            Label(p.transform, "BULWARK", 96, new Vector2(0, 700), new Vector2(900, 160), TextAnchor.MiddleCenter, Color.white);
            Button(p.transform, "PLAY", new Vector2(0, 200), () => Show(Screen.ModeSelect), IronBlue);
            Button(p.transform, "QUIT", new Vector2(0, -40), () => { }, new Color(0.3f,0.3f,0.35f));
            Label(p.transform, "Iron Pact  vs  Ashen Horde", 34, new Vector2(0, -260), new Vector2(900, 80), TextAnchor.MiddleCenter, new Color(1,1,1,0.7f));
            return p;
        }

        private GameObject BuildModeSelect()
        {
            var p = FullPanel("ModeSelect", "bg_menu", Dark);
            Label(p.transform, "CHOOSE MODE", 72, new Vector2(0, 700), new Vector2(900, 140), TextAnchor.MiddleCenter, Color.white);
            Button(p.transform, "CLASSIC", new Vector2(0, 320), () => StartMatch("Classic"), IronBlue);
            Button(p.transform, "TOURNAMENT", new Vector2(0, 140), () => StartMatch("Tournament"), IronBlue);
            Button(p.transform, "ENDLESS", new Vector2(0, -40), () => StartMatch("Endless"), IronBlue);
            Button(p.transform, "BACK", new Vector2(0, -300), () => Show(Screen.Menu), new Color(0.3f,0.3f,0.35f));
            return p;
        }

        private GameObject BuildEnd()
        {
            var p = FullPanel("End", "bg_victory", new Color(0,0,0,0.7f));
            _endTitle = Label(p.transform, "VICTORY", 120, new Vector2(0, 250), new Vector2(900, 200), TextAnchor.MiddleCenter, Color.white);
            _endSummary = Label(p.transform, "", 40, new Vector2(0, 80), new Vector2(900, 100), TextAnchor.MiddleCenter, new Color(1,1,1,0.9f));
            Button(p.transform, "MAIN MENU", new Vector2(0, -200), () => Show(Screen.Menu), IronBlue);
            return p;
        }

        private Text Label(Transform parent, string text, int size, Vector2 pos, Vector2 sz, TextAnchor anchor, Color col)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = sz;
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = anchor; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void Button(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick, Color tint)
        {
            var go = new GameObject("Btn_" + text);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(640, 150);
            var img = go.AddComponent<Image>();
            var s = Spr("button");
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; img.color = tint; }
            else img.color = tint;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            Label(go.transform, text, 48, Vector2.zero, new Vector2(620, 140), TextAnchor.MiddleCenter, Color.white);
        }

        private static void SetPanelBg(GameObject p, string key)
        {
            if (p == null) return;
            var img = p.GetComponent<Image>(); var s = Spr(key);
            if (img != null && s != null) { img.sprite = s; img.color = Color.white; }
        }

        private static Sprite Spr(string key) =>
            PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(key) : null;
    }
}
