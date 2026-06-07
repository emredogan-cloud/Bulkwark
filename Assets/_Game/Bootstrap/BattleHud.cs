// BULWARK — BATTLE HUD (UI Construction Bible · 08). Presentation-only restyle, REMOVABLE.
//
// The primary in-match HUD overlaying the live ECS battlefield. This file's VISUALS are rebuilt to the Bible's
// 08 spec (edge-hugging gold chrome, dual faction HP troughs + crests + centre node, gold/supply/army chips,
// gold-framed unit-train tiles with cost + affordability state, GARRISON/DEFEND/ATTACK order cluster, top/bottom
// scrims, clear battlefield centre). The CONTROL BOUNDARY is unchanged and inviolable (§12 — identical to before):
// it READS the ECS world read-only (GoldStore, StatueState, UnitTag/Team, the UnitCatalog roster) and writes ONLY
// the permitted player INPUT — Training.EnqueueTrain and MoveDestination (the three order buttons all issue
// MoveDestination to different target points; nothing else). NO new gameplay rule / balance / catalog / AI /
// economy. All ECS access stays in Update (main thread). Deleting this one file removes the HUD 100%. Placeholder
// portrait/crest art is code-built (UiTex) pending authored assets (Section N).

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
    /// <summary>Bible-08 textured in-match HUD. Control layer (§12): read-only of sim + EnqueueTrain/MoveDestination.</summary>
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
        private static readonly Color IronBlue = UiTheme.IronBlue;
        private static readonly Color AshRed = UiTheme.Ember2;
        private static readonly Color Gold = UiTheme.Gold;
        private static readonly Color Disabled = new Color(0.30f, 0.30f, 0.34f);

        private Font _font;
        private Sprite _white, _btnSprite, _goldIcon;
        private GameObject _root;
        private bool _built;
        private Rect _lastSafe;

        private Text _goldText, _supplyText, _armyText;
        private Image _hpFillP, _hpFillAI;
        private Text _hpTextP, _hpTextAI;
        private RectTransform _btnRow;
        private bool _rosterBuilt;
        private Image _goldIconImg;
        private Button _garrisonBtn, _defendBtn, _attackBtn;
        private int _activeStance = 2; // 0=garrison 1=defend 2=attack (single-select; default attack)

        private struct Btn { public int Index; public RoleId Role; public int Cost; public Button Button; public Image Bg; public Text CostText; }
        private readonly List<Btn> _btns = new List<Btn>(8);

        private int _trainReq = -1;
        private int _orderReq = -1; // 0=garrison 1=defend 2=attack

        private World _w; private EntityManager _em; private bool _ready;
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
            if (inMatch && Screen.safeArea != _lastSafe) ApplySafeArea();
            if (!inMatch) return;
            if (!EnsureWorld()) return;

            if (_btnSprite == null && PlaceholderAssets.Instance != null) { _btnSprite = PlaceholderAssets.Instance.Get("button"); _goldIcon = PlaceholderAssets.Instance.Get("gold"); ApplySprites(); }

            try
            {
                ReadState();
                BuildRosterButtons();
                if (_trainReq >= 0) { Training.EnqueueTrain(_em, PlayerTeam, _trainReq); AudioManager.Instance?.Train(); Debug.Log($"[BHUD] TRAIN unit {_trainReq}."); _trainReq = -1; }
                if (_orderReq >= 0) { IssueOrder(_orderReq); _orderReq = -1; }
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

        // Single statue position lookup (read-only) for the order-cluster MoveDestination targets.
        private bool StatuePos(int team, out float2 pos)
        {
            pos = float2.zero; bool have = false;
            var sq = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<Position>());
            using (var stag = sq.ToComponentDataArray<StatueTag>(Allocator.Temp))
            using (var spos = sq.ToComponentDataArray<Position>(Allocator.Temp))
                for (int i = 0; i < stag.Length; i++) if (stag[i].Team == team) { pos = spos[i].Value; have = true; }
            return have;
        }

        // GARRISON/DEFEND/ATTACK = the same permitted MoveDestination write to different points (§12). No new system.
        private void IssueOrder(int stance)
        {
            float2 target; bool have;
            if (stance == 0) have = StatuePos(PlayerTeam, out target);              // GARRISON → fall back to own statue
            else if (stance == 1)                                                    // DEFEND → midpoint between statues
            {
                have = StatuePos(PlayerTeam, out var a) & StatuePos(AiTeam, out var b);
                target = (a + b) * 0.5f;
            }
            else have = StatuePos(AiTeam, out target);                               // ATTACK → enemy statue
            if (!have) return;
            int n = MoveAllPlayerUnits(target);
            _activeStance = stance;
            UpdateStanceGlow();
            Debug.Log($"[BHUD] ORDER {stance} -> {n} units.");
        }

        private int MoveAllPlayerUnits(float2 target)
        {
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

        // ---------------- UI build (Bible-08 restyle) ----------------
        private void BuildCanvas()
        {
            var cgo = new GameObject("BattleHudCanvas");
            cgo.transform.SetParent(transform, false);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // above the battlefield, below UiFlow's menu canvas (100) and the router (200)
            var scaler = cgo.AddComponent<CanvasScaler>();
            UiScaling.Configure(scaler);
            cgo.AddComponent<GraphicRaycaster>();
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(transform, false);
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // Scrims (raycast off so empty-centre taps reach the sim) — built on the canvas, OUTSIDE the safe-area root.
            ScrimImg("Scrim_Top", cgo.transform, new Vector2(0, 0.82f), new Vector2(1, 1), true);
            ScrimImg("Scrim_Bottom", cgo.transform, new Vector2(0, 0), new Vector2(1, 0.2f), false);

            _root = NewRect("HudRoot", cgo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ApplySafeArea();

            BuildTopBar();
            BuildUnitTray();
            BuildOrderCluster();
        }

        private void BuildTopBar()
        {
            var top = NewRect("TopBar", _root.transform, new Vector2(0, 0.86f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            ((RectTransform)top.transform).offsetMin = Vector2.zero; ((RectTransform)top.transform).offsetMax = Vector2.zero;

            // Pause (top-left squircle) → opens the Pause modal (Time.timeScale handled there).
            var pause = ChipFrame(top.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(58, -50), new Vector2(86, 86));
            var pbtn = pause.gameObject.AddComponent<Button>(); pbtn.targetGraphic = pause.GetComponent<Image>();
            pbtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); MatchPresentation.ShowPause(); });
            Label(pause.transform, "II", 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.GoldHi);

            // Gold chip (under pause) + supply chip (right of gold).
            var goldChip = ChipFrame(top.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -150), new Vector2(220, 64));
            _goldIconImg = Img(goldChip.transform, _white, Gold, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(44, 44));
            _goldText = Label(goldChip.transform, "0", 30, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(76, 0), new Vector2(150, 50), TextAnchor.MiddleLeft, Hex("#ffe9a8"));
            var supChip = ChipFrame(top.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(330, -150), new Vector2(170, 64));
            Img(supChip.transform, _white, UiTheme.Parchment, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(40, 40));
            _supplyText = Label(supChip.transform, "0", 28, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(110, 46), TextAnchor.MiddleLeft, Hex("#e8e2cf"));

            // Army chip (top-right).
            var armyChip = ChipFrame(top.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-130, -50), new Vector2(210, 64));
            Img(armyChip.transform, _white, AshRed, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(44, 44));
            _armyText = Label(armyChip.transform, "0", 28, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(76, 0), new Vector2(140, 46), TextAnchor.MiddleLeft, Hex("#ffd9cf"));

            // Dual HP troughs + crests + centre node.
            _hpFillP = HpBar(top.transform, false, IronBlue, out _hpTextP);
            _hpFillAI = HpBar(top.transform, true, AshRed, out _hpTextAI);
            var node = NewRect("CenterNode", top.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(46, 46));
            var nImg = node.AddComponent<Image>(); nImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 48); nImg.raycastTarget = false;
        }

        // A faction HP bar: gold-framed trough + faction fill (depletes toward centre) + crest + "cur / max".
        private Image HpBar(Transform parent, bool right, Color col, out Text valueText)
        {
            float innerX = right ? 0.515f : 0.485f;
            var holder = NewRect("HpBar_" + (right ? "R" : "L"), parent, new Vector2(innerX, 1f), new Vector2(innerX, 1f), new Vector2(right ? 351 : -351, -52), new Vector2(702, 46));
            var bg = holder.AddComponent<Image>(); bg.sprite = UiTex.VGradient(Hex("#1a140a"), Hex("#0c0e14"), 32); bg.raycastTarget = false;
            var frame = NewRect("Trough", holder.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fr = frame.AddComponent<Image>(); fr.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fr.type = Image.Type.Sliced; fr.raycastTarget = false;
            var fillGo = NewRect("Fill", holder.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-12, -12));
            var fill = fillGo.AddComponent<Image>(); fill.sprite = UiTex.VGradient(UiWidgets.Lighten(col, 0.3f), UiWidgets.Darken(col, 0.2f), 32); fill.raycastTarget = false;
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = right ? 0 : 1; fill.fillAmount = 1f; // both deplete toward centre
            valueText = Label(holder.transform, "10,000 / 10,000", 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, right ? Hex("#ffeae6") : Hex("#eaf1ff"));
            // crest medallion at the outer end
            float crestX = right ? 720 : -720;
            var crest = NewRect("Crest", holder.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(crestX, 0), new Vector2(92, 92));
            var cImg = crest.AddComponent<Image>(); cImg.sprite = UiTex.Disc(UiWidgets.Darken(col, 0.2f), 64); cImg.raycastTarget = false;
            var crim = NewRect("CrestRim", crest.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var crm = crim.AddComponent<Image>(); crm.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); crm.type = Image.Type.Sliced; crm.raycastTarget = false;
            var cg = NewRect("CrestGlyph", crest.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 44)); var cgi = cg.AddComponent<Image>(); cgi.sprite = UiTex.Diamond(UiTheme.GoldHi, 32); cgi.raycastTarget = false;
            return fill;
        }

        private void BuildUnitTray()
        {
            var bottom = NewRect("UnitTray", _root.transform, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(0, 150));
            var rowGo = NewRect("TrainRow", bottom.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(28, 0), new Vector2(-40, 0));
            _btnRow = (RectTransform)rowGo.transform;
            var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14; hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
        }

        private void BuildOrderCluster()
        {
            var cluster = NewRect("OrderCluster", _root.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-396, 90), new Vector2(770, 130));
            _garrisonBtn = OrderButton(cluster.transform, "GARRISON", Hex("#20242e"), new Vector2(0, 0.5f), new Vector2(8 + 123, 0), () => _orderReq = 0, false);
            _defendBtn = OrderButton(cluster.transform, "DEFEND", Hex("#20242e"), new Vector2(0, 0.5f), new Vector2(8 + 123 + 256, 0), () => _orderReq = 1, false);
            _attackBtn = OrderButton(cluster.transform, "ATTACK", UiTheme.Oxblood, new Vector2(0, 0.5f), new Vector2(8 + 123 + 512, 0), () => _orderReq = 2, true);
            UpdateStanceGlow();
        }

        private Button OrderButton(Transform parent, string text, Color body, Vector2 anchor, Vector2 pos, UnityEngine.Events.UnityAction onClick, bool primary)
        {
            var rt = NewRect("Btn_" + text, parent, anchor, anchor, pos, new Vector2(246, 113));
            ((RectTransform)rt.transform).pivot = new Vector2(0.5f, 0.5f);
            var img = rt.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiWidgets.Lighten(body, 0.25f), UiWidgets.Darken(body, 0.3f), 32);
            var btn = rt.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var frame = NewRect("Rim", rt.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fr = frame.AddComponent<Image>(); fr.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fr.type = Image.Type.Sliced; fr.raycastTarget = false;
            Label(rt.transform, text, primary ? 30 : 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, primary ? Hex("#ffe6a0") : Hex("#ecd9a6"));
            return btn;
        }

        private void UpdateStanceGlow()
        {
            SetStance(_garrisonBtn, _activeStance == 0);
            SetStance(_defendBtn, _activeStance == 1);
            SetStance(_attackBtn, _activeStance == 2);
        }
        private static void SetStance(Button b, bool active)
        {
            if (b == null) return;
            var img = b.GetComponent<Image>(); if (img != null) img.color = active ? new Color(1.18f, 1.18f, 1.18f, 1f) : Color.white;
        }

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
                var le = go.AddComponent<LayoutElement>(); le.preferredWidth = 150; le.preferredHeight = 146;
                var img = go.AddComponent<Image>();
                img.sprite = UiTex.VGradient(UiWidgets.Lighten(IronBlue, 0.15f), UiTheme.Charcoal, 32);
                var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); _trainReq = idx; });
                // gold frame
                var frame = NewRect("TileFrame", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fr = frame.AddComponent<Image>(); fr.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fr.type = Image.Type.Sliced; fr.raycastTarget = false;
                // portrait glyph placeholder + role label
                Label(go.transform, role.ToString(), 22, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(150, 40), TextAnchor.MiddleCenter, Color.white);
                // cost chip on the lower edge
                var costChip = NewRect("CostChip", go.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(110, 44));
                var cc = costChip.AddComponent<Image>(); cc.sprite = UiTex.Disc(UiTheme.A(UiTheme.Obsidian, 0.85f), 48); cc.raycastTarget = false;
                Img(costChip.transform, _white, Gold, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(26, 0), new Vector2(26, 26));
                var costText = Label(costChip.transform, cost.ToString(), 24, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(80, 36), TextAnchor.MiddleLeft, Hex("#ffe9a8"));
                _btns.Add(new Btn { Index = idx, Role = role, Cost = cost, Button = btn, Bg = img, CostText = costText });
            }
            _rosterBuilt = true;
            Debug.Log($"[BHUD] roster buttons built: {_btns.Count}.");
        }

        private void Refresh()
        {
            if (_goldText != null) _goldText.text = _goldP < 0 ? "—" : _goldP.ToString();
            if (_supplyText != null) _supplyText.text = _unitsP.ToString();
            if (_armyText != null) _armyText.text = _unitsAI.ToString();
            if (_hpFillP != null) _hpFillP.fillAmount = Mathf.Clamp01(_hpP / _hpMaxP);
            if (_hpFillAI != null) _hpFillAI.fillAmount = Mathf.Clamp01(_hpAI / _hpMaxAI);
            if (_hpTextP != null) _hpTextP.text = $"{Mathf.Max(0, Mathf.RoundToInt(_hpP)):N0} / {Mathf.RoundToInt(_hpMaxP):N0}";
            if (_hpTextAI != null) _hpTextAI.text = $"{Mathf.Max(0, Mathf.RoundToInt(_hpAI)):N0} / {Mathf.RoundToInt(_hpMaxAI):N0}";
            for (int i = 0; i < _btns.Count; i++)
            {
                bool afford = _btns[i].Cost <= _goldP;
                _btns[i].Bg.color = afford ? Color.white : Disabled; // tile stays tappable (deny-shake handled by gate); tint shows affordability
                if (_btns[i].CostText != null) _btns[i].CostText.color = afford ? Hex("#ffe9a8") : Hex("#ff7a6a");
            }
        }

        private void ApplySprites()
        {
            if (_goldIcon != null && _goldIconImg != null) { _goldIconImg.sprite = _goldIcon; _goldIconImg.color = Color.white; }
        }

        // ---------------- tiny uGUI builders ----------------
        private void ApplySafeArea()
        {
            if (_root == null) return;
            var sa = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = new Vector2(sa.xMin / w, sa.yMin / h);
            rt.anchorMax = new Vector2(sa.xMax / w, sa.yMax / h);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _lastSafe = sa;
        }

        private void ScrimImg(string name, Transform parent, Vector2 aMin, Vector2 aMax, bool top)
        {
            var go = NewRect(name, parent, aMin, aMax, Vector2.zero, Vector2.zero);
            ((RectTransform)go.transform).offsetMin = Vector2.zero; ((RectTransform)go.transform).offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = top ? UiTex.VGradient(UiTheme.A(UiTheme.Vignette, 0.7f), UiTheme.A(UiTheme.Vignette, 0f), 32)
                             : UiTex.VGradient(UiTheme.A(UiTheme.Vignette, 0f), UiTheme.A(UiTheme.Vignette, 0.7f), 32);
        }

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

        private RectTransform ChipFrame(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = NewRect("Chip", parent, aMin, aMax, pos, size);
            ((RectTransform)go.transform).pivot = new Vector2(0.5f, 0.5f);
            var img = go.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.95f), UiTheme.A(UiTheme.Obsidian, 0.95f), 32);
            var frame = NewRect("Rim", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fr = frame.AddComponent<Image>(); fr.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); fr.type = Image.Type.Sliced; fr.raycastTarget = false;
            return (RectTransform)go.transform;
        }

        private Image Img(Transform parent, Sprite spr, Color col, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = NewRect("Img", parent, aMin, aMax, pos, size);
            var img = go.AddComponent<Image>(); img.sprite = spr; img.color = col; img.raycastTarget = false;
            return img;
        }

        private Text Label(Transform parent, string text, int size, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta, TextAnchor anchor, Color col)
        {
            var go = NewRect("Label", parent, aMin, aMax, pos, sizeDelta);
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = anchor; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow; t.raycastTarget = false;
            var outline = go.AddComponent<Shadow>(); outline.effectColor = new Color(0, 0, 0, 0.8f); outline.effectDistance = new Vector2(2, -2);
            return t;
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
