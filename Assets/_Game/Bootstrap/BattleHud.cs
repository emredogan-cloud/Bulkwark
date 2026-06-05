// BULWARK — BATTLE HUD (Production Presentation phase, ADR-5-003 Option A · Priority #2). TEMPORARY/REMOVABLE.
//
// A textured uGUI in-match HUD that replaces the IMGUI debug overlays on-screen: a gold indicator, both
// statue-HP bars, and troop-production buttons (+ an Advance command). It is shown ONLY during the uGUI Match
// screen (PresentationState.InMatch, set by UiFlow).
//
// CONTROL BOUNDARY (§12 — identical to the existing SimPlayerHud): it READS the ECS world read-only for display
// (GoldStore, StatueState, UnitTag, the UnitCatalog roster) and, on a button press, writes ONLY player INPUT
// data the ECS systems already consume — Training.EnqueueTrain (append a TrainOrder) and MoveDestination (an
// attack-move). It adds NO gameplay rule, NO balance value, NO catalog/AI/economy change; the TrainingSystem
// still pays the data-driven cost and spawns. All ECS access happens in Update (safe main-thread point).
// Deleting this one file removes the HUD 100%. Placeholder art (gold icon / button) is © ripped → dev-only.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Bulwark.Sim;

namespace Bulwark.Bootstrap
{
    /// <summary>Textured in-match HUD (gold + statue-HP bars + troop buttons). Control layer (§12), read-only of sim.</summary>
    public sealed class BattleHud : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("BattleHud");
            go.AddComponent<BattleHud>();
            DontDestroyOnLoad(go);
            Debug.Log("[BHUD] BattleHud booted.");
        }

        private const int PlayerTeam = 0, AiTeam = 1;
        private static readonly Color IronBlue = new Color(0.20f, 0.45f, 0.95f);
        private static readonly Color AshRed = new Color(0.85f, 0.25f, 0.20f);
        private static readonly Color Gold = new Color(1f, 0.84f, 0.20f);
        private static readonly Color Disabled = new Color(0.30f, 0.30f, 0.34f);

        private Font _font;
        private Sprite _white, _btnSprite, _goldIcon;
        private GameObject _root;
        private bool _built;

        private Text _goldText, _unitText;
        private Image _hpFillP, _hpFillAI;
        private Text _hpTextP, _hpTextAI;
        private RectTransform _btnRow;
        private bool _rosterBuilt;

        private struct Btn { public int Index; public RoleId Role; public int Cost; public Button Button; public Image Bg; }
        private readonly List<Btn> _btns = new List<Btn>(8);

        private int _trainReq = -1;
        private bool _advanceReq;

        private World _w; private EntityManager _em; private bool _ready;
        // display snapshot
        private int _goldP = -1, _unitsP, _unitsAI;
        private float _hpP, _hpMaxP = 1f, _hpAI, _hpMaxAI = 1f;

        private void Start()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _white = MakeWhite();
            BuildCanvas();
            _root.SetActive(false);
            _built = true;
        }

        private void Update()
        {
            if (!_built) return;
            bool inMatch = PresentationState.InMatch;
            if (_root.activeSelf != inMatch) _root.SetActive(inMatch);
            if (!inMatch) return;
            if (!EnsureWorld()) return;

            // Late-bound placeholder sprites (load async) — apply once available.
            if (_btnSprite == null && PlaceholderAssets.Instance != null) { _btnSprite = PlaceholderAssets.Instance.Get("button"); _goldIcon = PlaceholderAssets.Instance.Get("gold"); ApplySprites(); }

            try
            {
                ReadState();
                BuildRosterButtons();                 // one-shot once the catalog is ready
                if (_trainReq >= 0) { Training.EnqueueTrain(_em, PlayerTeam, _trainReq); AudioManager.Instance?.Train(); Debug.Log($"[BHUD] TRAIN unit {_trainReq}."); _trainReq = -1; }
                if (_advanceReq) { int n = AdvanceAllPlayerUnits(); Debug.Log($"[BHUD] ADVANCE -> {n} units."); _advanceReq = false; }
                Refresh();
            }
            catch (System.Exception e) { Debug.LogError("[BHUD] " + e.Message); }
        }

        private bool EnsureWorld()
        {
            World w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) { _ready = false; return false; }
            if (!_ready || w != _w) { _w = w; _em = w.EntityManager; _ready = true; }
            return true;
        }

        // ---------------- ECS reads (read-only) ----------------
        private void ReadState()
        {
            _goldP = -1;
            using (var g = _em.CreateEntityQuery(ComponentType.ReadOnly<GoldStore>()).ToComponentDataArray<GoldStore>(Allocator.Temp))
                for (int i = 0; i < g.Length; i++) if (g[i].Team == PlayerTeam) _goldP = g[i].Amount;

            _unitsP = 0; _unitsAI = 0;
            var uq = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Team>());
            using (var t = uq.ToComponentDataArray<Team>(Allocator.Temp))
                for (int i = 0; i < t.Length; i++) { if (t[i].Id == PlayerTeam) _unitsP++; else if (t[i].Id == AiTeam) _unitsAI++; }

            var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<StatueState>());
            using (var tags = sq.ToComponentDataArray<StatueTag>(Allocator.Temp))
            using (var st = sq.ToComponentDataArray<StatueState>(Allocator.Temp))
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i].Team == PlayerTeam) { _hpP = st[i].Health; _hpMaxP = st[i].MaxHealth > 0f ? st[i].MaxHealth : 1f; }
                    else if (tags[i].Team == AiTeam) { _hpAI = st[i].Health; _hpMaxAI = st[i].MaxHealth > 0f ? st[i].MaxHealth : 1f; }
                }
        }

        private int AdvanceAllPlayerUnits()
        {
            float2 target = float2.zero; bool have = false;
            var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<Position>());
            using (var stag = sq.ToComponentDataArray<StatueTag>(Allocator.Temp))
            using (var spos = sq.ToComponentDataArray<Position>(Allocator.Temp))
                for (int i = 0; i < stag.Length; i++) if (stag[i].Team == AiTeam) { target = spos[i].Value; have = true; }
            if (!have) return 0;
            int n = 0;
            var uq = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Team>());
            using (var ents = uq.ToEntityArray(Allocator.Temp))
            using (var team = uq.ToComponentDataArray<Team>(Allocator.Temp))
                for (int i = 0; i < ents.Length; i++)
                {
                    if (team[i].Id != PlayerTeam) continue;
                    if (_em.HasComponent<MinerTag>(ents[i])) continue; // keep miners on the mines
                    var dest = new MoveDestination { Value = target, Active = 1 };
                    if (_em.HasComponent<MoveDestination>(ents[i])) _em.SetComponentData(ents[i], dest);
                    else _em.AddComponentData(ents[i], dest);
                    n++;
                }
            return n;
        }

        // ---------------- UI build (code-built, original) ----------------
        private void BuildCanvas()
        {
            var cgo = new GameObject("BattleHudCanvas");
            cgo.transform.SetParent(transform, false);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // above the battlefield, below UiFlow's menu canvas (100)
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            // Self-sufficient for clicks even if UiFlow is absent (a debug-only run): ensure an EventSystem.
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(transform, false);
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            _root = NewRect("HudRoot", cgo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // ---- Top bar (gold chip + two statue-HP bars + unit count) ----
            var top = NewRect("TopBar", _root.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20), new Vector2(0, 240));
            var topRt = (RectTransform)top.transform; topRt.pivot = new Vector2(0.5f, 1f);

            // gold chip
            var chip = Panel(top.transform, new Color(0, 0, 0, 0.55f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -16), new Vector2(280, 84));
            ((RectTransform)chip.transform).pivot = new Vector2(0.5f, 1f);
            _goldIconImg = Img(chip.transform, _white, Gold, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 0), new Vector2(56, 56));
            _goldText = Label(chip.transform, "0", 44, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(96, 0), new Vector2(170, 70), TextAnchor.MiddleLeft, Gold);

            // statue HP bars (center)
            _hpFillP = Bar(top.transform, "IRON PACT", IronBlue, new Vector2(0, -24), out _hpTextP);
            _hpFillAI = Bar(top.transform, "ASHEN HORDE", AshRed, new Vector2(0, -84), out _hpTextAI);

            // unit count (top-right)
            _unitText = Label(top.transform, "", 30, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-150, -40), new Vector2(280, 60), TextAnchor.MiddleRight, new Color(1, 1, 1, 0.85f));

            // ---- Bottom bar (troop production buttons + advance) ----
            var bottom = NewRect("BottomBar", _root.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 24), new Vector2(0, 200));
            ((RectTransform)bottom.transform).pivot = new Vector2(0.5f, 0f);

            var rowGo = NewRect("TrainRow", bottom.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(-90, 0), new Vector2(-200, 0));
            _btnRow = (RectTransform)rowGo.transform;
            var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true; hlg.childControlHeight = true;

            _advanceBtn = ActionButton(bottom.transform, "ADVANCE", new Color(0.85f, 0.55f, 0.15f), new Vector2(1, 0.5f), new Vector2(180, 150), () => _advanceReq = true);
        }

        private Image _goldIconImg;
        private Button _advanceBtn;

        private void BuildRosterButtons()
        {
            if (_rosterBuilt) return;
            if (!UnitCatalog.TryGetForTeam(_em, PlayerTeam, out Entity cat)) return;
            if (!_em.HasBuffer<UnitSpawnStats>(cat)) return;
            var buf = _em.GetBuffer<UnitSpawnStats>(cat, true);
            if (buf.Length == 0) return;
            for (int i = 0; i < buf.Length; i++)
            {
                int idx = i; var role = buf[i].Role; int cost = buf[i].GoldCost;
                var go = new GameObject("Train_" + role);
                go.transform.SetParent(_btnRow, false);
                go.AddComponent<RectTransform>();
                var img = go.AddComponent<Image>();
                if (_btnSprite != null) { img.sprite = _btnSprite; img.type = Image.Type.Sliced; }
                img.color = IronBlue;
                var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); _trainReq = idx; });
                Label(go.transform, role + "\n" + cost + "g", 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
                _btns.Add(new Btn { Index = idx, Role = role, Cost = cost, Button = btn, Bg = img });
            }
            _rosterBuilt = true;
            Debug.Log($"[BHUD] roster buttons built: {_btns.Count}.");
        }

        private void Refresh()
        {
            if (_goldText != null) _goldText.text = _goldP < 0 ? "—" : _goldP.ToString();
            if (_unitText != null) _unitText.text = $"Units {_unitsP} vs {_unitsAI}";
            if (_hpFillP != null) _hpFillP.fillAmount = Mathf.Clamp01(_hpP / _hpMaxP);
            if (_hpFillAI != null) _hpFillAI.fillAmount = Mathf.Clamp01(_hpAI / _hpMaxAI);
            if (_hpTextP != null) _hpTextP.text = $"IRON PACT  {Mathf.Max(0, Mathf.RoundToInt(_hpP))}";
            if (_hpTextAI != null) _hpTextAI.text = $"ASHEN HORDE  {Mathf.Max(0, Mathf.RoundToInt(_hpAI))}";
            for (int i = 0; i < _btns.Count; i++)
            {
                bool afford = _btns[i].Cost <= _goldP;
                _btns[i].Button.interactable = afford;
                _btns[i].Bg.color = afford ? IronBlue : Disabled;
            }
        }

        private void ApplySprites()
        {
            if (_goldIcon != null && _goldIconImg != null) { _goldIconImg.sprite = _goldIcon; _goldIconImg.color = Color.white; }
            if (_btnSprite != null && _advanceBtn != null) { var im = _advanceBtn.GetComponent<Image>(); im.sprite = _btnSprite; im.type = Image.Type.Sliced; }
            for (int i = 0; i < _btns.Count; i++) if (_btnSprite != null) { _btns[i].Bg.sprite = _btnSprite; _btns[i].Bg.type = Image.Type.Sliced; }
        }

        // ---------------- tiny uGUI builders ----------------
        private static Sprite MakeWhite()
        {
            var t = new Texture2D(4, 4); var c = new Color[16];
            for (int i = 0; i < 16; i++) c[i] = Color.white; t.SetPixels(c); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
        }

        private GameObject NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
            return go;
        }

        private GameObject Panel(Transform parent, Color col, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = NewRect("Panel", parent, aMin, aMax, pos, size);
            var img = go.AddComponent<Image>(); img.sprite = _white; img.color = col;
            return go;
        }

        private Image Img(Transform parent, Sprite spr, Color col, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = NewRect("Img", parent, aMin, aMax, pos, size);
            var img = go.AddComponent<Image>(); img.sprite = spr; img.color = col;
            return img;
        }

        // A labeled horizontal statue-HP bar, centered in the top bar. Returns the Filled fill Image.
        private Image Bar(Transform parent, string label, Color col, Vector2 offset, out Text valueText)
        {
            var holder = NewRect("Bar_" + label, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), offset, new Vector2(560, 48));
            ((RectTransform)holder.transform).pivot = new Vector2(0.5f, 1f);
            var bg = holder.AddComponent<Image>(); bg.sprite = _white; bg.color = new Color(0, 0, 0, 0.55f);
            var fillGo = NewRect("Fill", holder.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8, -8));
            var fill = fillGo.AddComponent<Image>();
            fill.sprite = _white; fill.color = col;
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 1f;
            valueText = Label(holder.transform, label, 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            return fill;
        }

        private Button ActionButton(Transform parent, string text, Color col, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = NewRect("Btn_" + text, parent, anchor, anchor, new Vector2(-100, 0), size);
            var img = go.AddComponent<Image>(); img.sprite = _white; img.color = col;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            Label(go.transform, text, 30, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            return btn;
        }

        private Text Label(Transform parent, string text, int size, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta, TextAnchor anchor, Color col)
        {
            var go = NewRect("Label", parent, aMin, aMax, pos, sizeDelta);
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = anchor; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = go.AddComponent<Shadow>(); outline.effectColor = new Color(0, 0, 0, 0.8f); outline.effectDistance = new Vector2(2, -2);
            return t;
        }
    }
}
