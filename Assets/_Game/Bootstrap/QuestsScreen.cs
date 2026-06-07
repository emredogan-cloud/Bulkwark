// BULWARK — QUESTS (UI Construction Bible · 26). Presentation-only, REMOVABLE.
//
// §12 boundary: presentation only — NO ECS / NO Unity.Entities / NO gameplay/balance/AI/economy/backend. This is
// a forensic code-built rebuild of design/QuestsScreenDesign.png (Bible 26): a "DAILY QUESTS" parchment scroll
// panel over a dark battlefield dusk, with a reset timer, Daily/Weekly tabs (Daily selected, red unread dot),
// five quest rows (emblem · name · description · progress bar · reward chip · CLAIM), and a "complete all" footer
// bonus meter. Rows bind to UiStub.DailyQuests (display-only stub data); CLAIM is display-only (Toast + visually
// mark CLAIMED) — claims are server-authoritative requests, so the UI never mutates any balance/ECS state (L/K §12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-26 landscape Quests (parchment scroll board). Presentation-only.</summary>
    public sealed class QuestsScreen : UiScreen
    {
        // ---- Canvas-fraction layout anchors (fy = fraction from TOP; anchorY = 1 − fy). Section E. ----
        // ScrollPanel: x 0.080→0.920, y 0.060→0.945 (centred parchment container).
        private const float PanelCx = 0.5f, PanelCy = 0.4975f;     // 1 − (0.060+0.945)/2
        private const float PanelW = 0.840f * 2340f, PanelH = 0.885f * 1080f; // ≈1966 × 956

        // Quest list region: x 0.110→0.890 (width 0.780w ≈1825), y fy 0.150→0.780 (anchorY 0.850→0.220).
        private const float ListXMin = 0.110f, ListXMax = 0.890f;
        private const float ListYMin = 0.220f, ListYMax = 0.850f;   // anchorY (1 − fy)
        private const float ListW = (ListXMax - ListXMin) * 2340f;  // ≈1825
        private const float RowH = 0.118f * 1080f;                  // ≈127
        private const float RowGap = 0.008f * 1080f;                // ≈8.6
        private const float RowPitch = RowH + RowGap;               // ≈136

        // Exact spec hex not in UiTheme.
        private static readonly Color ParchTop  = Hex("#d9c79a"); // parchment centre/top
        private static readonly Color ParchBot  = Hex("#c3ad79"); // parchment edge
        private static readonly Color ParchEdge = Hex("#9c855a"); // darkened parchment edge / plate
        private static readonly Color PlateCol  = Hex("#cfb87e"); // inset row plate
        private static readonly Color Cobalt    = Hex("#2b56c8"); // CLAIM / selected tab body
        private static readonly Color NameCol   = Hex("#3a2e1c"); // quest name / qty / "13h.." / "4/5"
        private static readonly Color DescCol   = Hex("#5a4a30"); // description / idle tab / bonus text
        private static readonly Color TimerTag  = Hex("#6b5a3a"); // "Reset In:"
        private static readonly Color TabSelTxt = Hex("#eaf2ff"); // selected tab label
        private static readonly Color BarText   = Hex("#f4e9c8"); // "3 / 3" over groove
        private static readonly Color CoinCol   = Hex("#f0c14a"); // coin reward icon
        private static readonly Color GemCol     = Hex("#9e6bf0"); // gem reward icon (AmethystHi)
        private static readonly Color GrooveCol  = Hex("#2a2418"); // progress groove
        private static readonly Color ClaimWhite = Hex("#ffffff"); // CLAIM label

        // Reset timer + bonus copy (display-only consts; Section E/L verbatim — stub lacks these).
        private const string ResetTimerText = "13h 46m 21s";
        private const string BonusText = "Complete all Daily Quests to earn bonus rewards!";
        private const int BonusDone = 4, BonusTotal = 5; // "4/5" meter (Section L: verbatim).

        // Per-quest one-line descriptions (Section A/L verbatim) — indexed to UiStub.DailyQuests. Display-only.
        private static readonly string[] QuestDescs =
        {
            "Win any 3 battles in Campaign or Multiplayer.",
            "Train a total of 50 units.",
            "Open 1 Silver Chest.",
            "Deal a total of 10,000 damage in battles.",
            "Log in to the game.",
        };

        // Animation/state handles (presentation-only, no gameplay).
        private CanvasGroup _panelCg;
        private CanvasGroup[] _rowCg;
        private Image[] _barFill;
        private float[] _barTarget;
        private Button[] _claimBtns;
        private Text[] _claimLabels;
        private bool[] _claimedView;        // local display-only claimed flags (NEVER mutates the stub)
        private RectTransform[] _rewardIcon; // reward-chip icon (claim pop target — no header chip on this screen)
        private Image _bonusFill;
        private Text _bonusLabel;
        private int _bonusView;

        protected override void Build()
        {
            // ---- BG_FullBleed: dark battlefield dusk #0a0b0f → #1c1510, faint banner right, strong vignette. ----
            UiWidgets.Stretch("BG_BattlefieldDusk", Rect, UiTheme.Obsidian, "bg_battle");
            var dusk = UiWidgets.Stretch("BG_DuskGrade", Rect, Color.white);
            dusk.raycastTarget = false; dusk.sprite = UiTex.VGradient(Hex("#1c1510"), Hex("#0a0b0f"), 64);
            // Faint faction banner on the right.
            var banner = UiWidgets.Rect("BG_Banner", Rect, new Vector2(0.88f, 0.5f), new Vector2(0.88f, 0.5f), Vector2.zero, new Vector2(260, 900));
            var bImg = banner.gameObject.AddComponent<Image>(); bImg.raycastTarget = false;
            bImg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.IronBanner, 0.22f), UiTheme.A(UiTheme.IronBanner, 0f), 64);
            UiWidgets.Vignette(Rect, 0.62f); // strong → focus the parchment

            // ---- ScrollPanel: parchment field + gold rolled-edge frame (manual: keep the cream field, do NOT darken). ----
            var panelGo = new GameObject("ScrollPanel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(PanelCx, PanelCy);
            panel.sizeDelta = new Vector2(PanelW, PanelH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();
            // Parchment field (aged cream centre → darker edge), matte.
            var field = panelGo.AddComponent<Image>(); field.raycastTarget = false;
            field.sprite = UiTex.VGradient(ParchTop, ParchBot, 64);
            // Edge-darkening ring (subtle) + gold rolled-edge frame molding over the field edge.
            var edge = UiWidgets.Rect("Panel_Edge", panel, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -10));
            var eImg = edge.gameObject.AddComponent<Image>(); eImg.raycastTarget = false;
            eImg.sprite = UiTex.Frame(UiTheme.A(ParchEdge, 0f), UiTheme.A(ParchEdge, 0f), UiTheme.A(ParchEdge, 0.7f), 64, 28); eImg.type = Image.Type.Sliced;
            var frame = UiWidgets.Rect("Panel_GoldFrame", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fImg = frame.gameObject.AddComponent<Image>(); fImg.raycastTarget = false;
            fImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 16); fImg.type = Image.Type.Sliced;
            // Rolled top/bottom-end finials.
            UiWidgets.Finial(panel, new Vector2(0.5f, 1f), new Vector2(0, 6), 48f);
            UiWidgets.Finial(panel, new Vector2(0.5f, 0f), new Vector2(0, -6), 48f);

            // ---- Header ----
            // Back (top-left chevron) — task-mandated signature on SafeContent.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Title "DAILY QUESTS" — centred, fy 0.910 → anchorY 0.090, gold serif gradient (cap≈56).
            UiWidgets.TitleLabel(SafeContent, "DAILY QUESTS", 56, new Vector2(0.5f, 0.090f), new Vector2(0.5f, 0.090f),
                Vector2.zero, new Vector2(1000, 90), TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);

            // ResetTimer — right (x≈0.880, fy 0.910): chest/timer icon | "Reset In:" | "13h 46m 21s".
            var timer = UiWidgets.Rect("ResetTimer", SafeContent, new Vector2(0.880f, 0.090f), new Vector2(0.880f, 0.090f), Vector2.zero, new Vector2(360, 64));
            var tIcon = UiWidgets.Rect("Timer_Icon", timer, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(46, 46));
            var tiImg = tIcon.gameObject.AddComponent<Image>(); tiImg.raycastTarget = false; tiImg.sprite = UiTex.Disc(UiTheme.GoldShadow, 48);
            UiWidgets.Label(timer, "Reset In:", 18, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(64, 14), new Vector2(150, 26), TextAnchor.MiddleLeft, TimerTag);
            UiWidgets.Label(timer, ResetTimerText, 24, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(64, -12), new Vector2(220, 30), TextAnchor.MiddleLeft, NameCol);

            // ---- Tabs (Daily selected cobalt + red unread dot · Weekly idle). Two only. ----
            // Pills 0.150w × 0.055h (≈351×59), gap 0.010w, centred about x 0.50, fy 0.825 → anchorY 0.175.
            Vector2 pillSize = new Vector2(0.150f * 2340f, 0.055f * 1080f);
            BuildTab("Daily", 0.420f, pillSize, true, () => Router.Toast("Daily quests"));
            BuildTab("Weekly", 0.580f, pillSize, false, () => Router.Toast("Weekly quests — coming soon"));

            // ---- Quest list (ScrollRect over the list region; content holds the 5 rows). ----
            BuildQuestList();

            // ---- Footer: bonus text + "4/5" meter + bonus chest (fy 0.070→0.140). ----
            BuildFooter();
        }

        // ---------------------------------------------------------------- Tabs ----
        private void BuildTab(string label, float x, Vector2 size, bool selected, UnityEngine.Events.UnityAction onClick)
        {
            var rt = UiWidgets.Rect("Tab_" + label, SafeContent, new Vector2(x, 0.175f), new Vector2(x, 0.175f), Vector2.zero, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = selected ? UiTex.VGradient(UiWidgets.Lighten(Cobalt, 0.28f), UiWidgets.Darken(Cobalt, 0.3f), 32)
                                   : UiTex.VGradient(ParchTop, ParchEdge, 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            // Gold trim.
            var rim = UiWidgets.Rect("TabRim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = rim.gameObject.AddComponent<Image>(); rImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); rImg.type = Image.Type.Sliced; rImg.raycastTarget = false;
            UiWidgets.Label(rt, label, 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, selected ? TabSelTxt : DescCol);
            // Daily selected → red unread dot at its top-right corner (unclaimed rewards exist).
            if (selected) { var dot = UiWidgets.Rect("RedDot", rt, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -6), new Vector2(26, 26)); var dImg = dot.gameObject.AddComponent<Image>(); dImg.raycastTarget = false; dImg.sprite = UiTex.Disc(UiTheme.Ember2, 32); dot.gameObject.AddComponent<PulseScale>(); }
        }

        // ---------------------------------------------------------------- Quest list ----
        private void BuildQuestList()
        {
            var quests = UiStub.DailyQuests;
            int n = quests.Length;
            _rowCg = new CanvasGroup[n];
            _barFill = new Image[n];
            _barTarget = new float[n];
            _claimBtns = new Button[n];
            _claimLabels = new Text[n];
            _claimedView = new bool[n];
            _rewardIcon = new RectTransform[n];

            // ScrollRect host over the list region (canvas x 0.110→0.890, anchorY 0.220→0.850).
            var host = UiWidgets.Rect("QuestList", SafeContent, new Vector2(ListXMin, ListYMin), new Vector2(ListXMax, ListYMax), Vector2.zero, Vector2.zero);
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30f;

            // Viewport (masked).
            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0); vImg.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content (top-anchored; tall enough for all rows — scrolls only if it overflows the viewport).
            float totalH = n * RowPitch;
            var content = UiWidgets.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, totalH));
            content.pivot = new Vector2(0.5f, 1f);

            scroll.viewport = viewport; scroll.content = content;

            for (int i = 0; i < n; i++) BuildQuestRow(content, quests[i], i);
        }

        private void BuildQuestRow(RectTransform content, UiStub.Quest q, int i)
        {
            // Row: top-stretch within content, fixed height, slight side inset; own CanvasGroup for stagger fade.
            var rowGo = new GameObject("QuestRow_" + i, typeof(RectTransform), typeof(CanvasGroup));
            var row = (RectTransform)rowGo.transform; row.SetParent(content, false);
            row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1f);
            row.offsetMin = new Vector2(0, 0); row.offsetMax = new Vector2(0, 0);
            row.sizeDelta = new Vector2(0, RowH); row.anchoredPosition = new Vector2(0, -i * RowPitch);
            _rowCg[i] = rowGo.GetComponent<CanvasGroup>();

            // Row_Plate (inset parchment plate) + thin gold divider beneath.
            var plate = UiWidgets.Rect("Row_Plate", row, new Vector2(0, 0.06f), new Vector2(1, 0.96f), Vector2.zero, Vector2.zero);
            var pImg = plate.gameObject.AddComponent<Image>(); pImg.raycastTarget = false;
            pImg.sprite = UiTex.VGradient(PlateCol, UiWidgets.Darken(PlateCol, 0.12f), 32);
            UiWidgets.Divider(row, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, ListW * 0.96f, 2f, UiTheme.A(UiTheme.GoldShadow, 0.8f));

            // Row-local x fractions = (canvasX − 0.110) / 0.780.
            const float fxIcon = 0.0513f, fxText = 0.1218f, fxReward = 0.6795f, fxClaim = 0.9038f;
            const float barCenterX = 0.3494f; // (0.205→0.560) mapped into the row
            float barW = (0.560f - 0.205f) / (ListXMax - ListXMin) * ListW; // ≈830

            // Icon_Emblem — circular gold-rimmed medallion (glyph placeholder), Ø≈81 (0.075h), v-centred.
            var icon = UiWidgets.Rect("Icon_Emblem", row, new Vector2(fxIcon, 0.5f), new Vector2(fxIcon, 0.5f), Vector2.zero, new Vector2(81, 81));
            var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false; iImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.GoldShadow, 0.9f), 64);
            var iRing = UiWidgets.Rect("Emblem_Ring", icon, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var iRingImg = iRing.gameObject.AddComponent<Image>(); iRingImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); iRingImg.type = Image.Type.Sliced; iRingImg.raycastTarget = false;
            var iGly = UiWidgets.Rect("Emblem_Glyph", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40, 40));
            var iGlyImg = iGly.gameObject.AddComponent<Image>(); iGlyImg.raycastTarget = false; iGlyImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);

            // Lbl_QuestName (28px dark-brown, row-local top) — stub Title.
            UiWidgets.Label(row, q.Title, 28, new Vector2(fxText, 0.74f), new Vector2(fxText, 0.74f), Vector2.zero, new Vector2(620, 36), TextAnchor.MiddleLeft, NameCol);
            // Lbl_QuestDesc (20px lighter brown, below name) — verbatim description.
            string desc = (i >= 0 && i < QuestDescs.Length) ? QuestDescs[i] : "";
            UiWidgets.Label(row, desc, 20, new Vector2(fxText, 0.50f), new Vector2(fxText, 0.50f), Vector2.zero, new Vector2(620, 30), TextAnchor.MiddleLeft, DescCol);

            // ProgressBar (UiWidgets.ProgressBar) — groove + warm-gold fill; complete bars read full/bright.
            bool complete = q.Progress >= q.Target;
            _barTarget[i] = q.Target > 0 ? Mathf.Clamp01((float)q.Progress / q.Target) : 1f;
            Color fillCol = complete ? UiTheme.GoldFillHi : UiTheme.GoldFill;
            var bar = UiWidgets.ProgressBar(row, new Vector2(barCenterX, 0.24f), new Vector2(barCenterX, 0.24f), Vector2.zero, new Vector2(barW, 24f), fillCol, 0f);
            // Recolour the ProgressBar groove to the spec channel #2a2418.
            var barRt = bar.transform.parent as RectTransform; var grooveImg = barRt != null ? barRt.GetComponent<Image>() : null; if (grooveImg != null) grooveImg.color = GrooveCol;
            _barFill[i] = bar; // fill Image (animated in OnShow)
            // Over-bar tabular value "3 / 3" / "32 / 50" / "7,250 / 10,000" (light on dark groove).
            UiWidgets.Label(bar.transform.parent, q.Progress.ToString("N0") + " / " + q.Target.ToString("N0"), 18,
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, BarText);

            // RewardChip — gem (violet) or coin (gold) icon + tabular quantity (dark brown, 1px stroke).
            var chip = UiWidgets.Rect("RewardChip", row, new Vector2(fxReward, 0.5f), new Vector2(fxReward, 0.5f), Vector2.zero, new Vector2(160, 70));
            var rIcon = UiWidgets.Rect("Reward_Icon", chip, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(35, 0), new Vector2(0.030f * 2340f, 0.030f * 2340f));
            var rIconImg = rIcon.gameObject.AddComponent<Image>(); rIconImg.raycastTarget = false;
            rIconImg.sprite = q.Gem ? UiTex.Diamond(GemCol, 48) : UiTex.Disc(CoinCol, 48);
            _rewardIcon[i] = rIcon;
            var qty = UiWidgets.Label(chip, q.Reward.ToString(), 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(86, 0), new Vector2(120, 36), TextAnchor.MiddleLeft, NameCol);
            qty.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(ParchTop, 0.9f); // 1px stroke

            // Btn_Claim — cobalt CTA, width ≈257 (0.110w) × height ≈76 (0.070h). States: claimable / in-progress / claimed.
            var claimRt = UiWidgets.Rect("Btn_Claim", row, new Vector2(fxClaim, 0.5f), new Vector2(fxClaim, 0.5f), Vector2.zero, new Vector2(0.110f * 2340f, 0.070f * 1080f));
            var claimImg = claimRt.gameObject.AddComponent<Image>();
            var btn = claimRt.gameObject.AddComponent<Button>(); btn.targetGraphic = claimImg;
            var ccb = btn.colors; ccb.normalColor = Color.white; ccb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); ccb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); ccb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); ccb.fadeDuration = 0.08f; btn.colors = ccb;
            int idx = i;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnClaim(idx); });
            var claimRim = UiWidgets.Rect("ClaimRim", claimRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var claimRimImg = claimRim.gameObject.AddComponent<Image>(); claimRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); claimRimImg.type = Image.Type.Sliced; claimRimImg.raycastTarget = false;
            var claimLbl = UiWidgets.Label(claimRt, "CLAIM", 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, ClaimWhite);
            claimLbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            _claimBtns[i] = btn; _claimLabels[i] = claimLbl;
            _claimedView[i] = q.Claimed;
            ApplyClaimVisual(i, complete);
        }

        // claimable = bright cobalt + pulse; in-progress = desaturated grey-blue, dim, non-interactable; claimed = "CLAIMED" + check, grey, row dims.
        private void ApplyClaimVisual(int i, bool complete)
        {
            var btn = _claimBtns[i]; if (btn == null) return;
            var img = btn.targetGraphic as Image;
            // Clear any prior claimable pulse.
            var oldPulse = btn.GetComponent<PulseGraphic>(); if (oldPulse != null) Destroy(oldPulse);

            if (_claimedView[i])
            {
                if (img != null) img.sprite = UiTex.VGradient(Hex("#5a6068"), Hex("#3a3e44"), 32);
                btn.interactable = false;
                if (_claimLabels[i] != null) { _claimLabels[i].text = "CLAIMED"; _claimLabels[i].color = Hex("#cfd6df"); }
                if (_rowCg[i] != null) _rowCg[i].alpha = Mathf.Min(_rowCg[i].alpha, 0.7f); // row dims slightly
            }
            else if (complete)
            {
                if (img != null) img.sprite = UiTex.VGradient(UiWidgets.Lighten(Cobalt, 0.28f), UiWidgets.Darken(Cobalt, 0.34f), 32);
                btn.interactable = true;
                if (_claimLabels[i] != null) { _claimLabels[i].text = "CLAIM"; _claimLabels[i].color = ClaimWhite; }
                var pg = btn.gameObject.AddComponent<PulseGraphic>(); pg.target = img; pg.min = 0.78f; pg.max = 1f; pg.period = 1.4f; // gentle CTA pulse
            }
            else // in-progress: desaturated grey-blue, dim, not interactable (tap → nudge handled in OnClaim).
            {
                if (img != null) img.sprite = UiTex.VGradient(Hex("#3a4658"), Hex("#262d38"), 32);
                btn.interactable = false;
                if (_claimLabels[i] != null) { _claimLabels[i].text = "CLAIM"; _claimLabels[i].color = Hex("#9aa6b8"); }
            }
        }

        // ---------------------------------------------------------------- Footer ----
        private void BuildFooter()
        {
            // Bonus text — left/centre (x≈0.130, fy 0.100 → anchorY 0.900).
            UiWidgets.Label(SafeContent, BonusText, 22, new Vector2(0.130f, 0.100f), new Vector2(0.130f, 0.100f), new Vector2(0, 0), new Vector2(760, 32), TextAnchor.MiddleLeft, DescCol);

            // BonusMeter "4/5" — centre (x≈0.620).
            _bonusView = BonusDone;
            _bonusFill = UiWidgets.ProgressBar(SafeContent, new Vector2(0.620f, 0.100f), new Vector2(0.620f, 0.100f), new Vector2(-150, 0), new Vector2(220, 24f), UiTheme.GoldFillHi, 0f);
            var bgGroove = _bonusFill.transform.parent != null ? _bonusFill.transform.parent.GetComponent<Image>() : null; if (bgGroove != null) bgGroove.color = GrooveCol;
            _bonusLabel = UiWidgets.Label(SafeContent, _bonusView + "/" + BonusTotal, 24, new Vector2(0.620f, 0.100f), new Vector2(0.620f, 0.100f), new Vector2(30, 0), new Vector2(120, 32), TextAnchor.MiddleLeft, NameCol);

            // Icon_BonusChest — right (x≈0.860), gold chest icon Ø≈0.06w (≈140) with a faint glow.
            var chest = UiWidgets.Rect("Icon_BonusChest", SafeContent, new Vector2(0.860f, 0.100f), new Vector2(0.860f, 0.100f), Vector2.zero, new Vector2(0.06f * 2340f, 0.06f * 2340f));
            UiWidgets.Glow(chest, UiTheme.A(UiTheme.GoldHi, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 180), 1.6f);
            var cImg = chest.gameObject.AddComponent<Image>(); cImg.raycastTarget = false; cImg.sprite = UiTex.VGradient(Hex("#a05b25"), Hex("#7a4a20"), 64);
            var cBand = UiWidgets.Rect("Chest_Band", chest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 20));
            var cBandImg = cBand.gameObject.AddComponent<Image>(); cBandImg.raycastTarget = false; cBandImg.color = UiTheme.GoldHi;
            // 5/5 → bonus chest becomes claimable (glows/pulses).
            if (_bonusView >= BonusTotal) chest.gameObject.AddComponent<PulseScale>();
        }

        // ---------------------------------------------------------------- Claim (display-only) ----
        private void OnClaim(int i)
        {
            if (_claimBtns == null || i < 0 || i >= _claimBtns.Length) return;
            var q = UiStub.DailyQuests[i];
            if (_claimedView[i]) { Router.Toast("Already claimed"); return; }
            bool complete = q.Progress >= q.Target;
            if (!complete) { Router.Toast("Keep going!"); return; } // in-progress nudge

            // §12: claim is a server-authoritative REQUEST. UI displays + requests only — NO wallet/ECS mutation here.
            _claimedView[i] = true;
            ApplyClaimVisual(i, true);
            Router.Toast("Claimed " + q.Reward + (q.Gem ? " Gems" : " Gold"));
            StartCoroutine(ClaimPop(i));

            // "complete all" bonus meter increments toward 5/5 (display-only visual).
            if (_bonusView < BonusTotal)
            {
                _bonusView++;
                if (_bonusLabel != null) _bonusLabel.text = _bonusView + "/" + BonusTotal;
                if (_bonusFill != null) _bonusFill.fillAmount = Mathf.Clamp01((float)_bonusView / BonusTotal);
            }
        }

        // Reward-icon pop acknowledgement (no header currency chip on this screen → pop in place, J: claim sparkle).
        private IEnumerator ClaimPop(int i)
        {
            if (_rewardIcon == null || i < 0 || i >= _rewardIcon.Length || _rewardIcon[i] == null) yield break;
            var tr = _rewardIcon[i]; float t = 0f; const float d = 0.35f;
            while (t < d && tr != null)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                tr.localScale = Vector3.one * (1f + 0.5f * Mathf.Sin(k * Mathf.PI)); // punch up then settle
                yield return null;
            }
            if (tr != null) tr.localScale = Vector3.one;
        }

        // ---------------------------------------------------------------- Lifecycle / enter timeline ----
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterTimeline());
        }

        // Section I (~0.55s): panel unroll → rows stagger in (slide 12px from right + fade) → progress bars fill.
        private IEnumerator EnterTimeline()
        {
            // Panel unroll: scale 0.96→1.0 + fade (0.30s).
            if (_panelCg != null)
            {
                _panelCg.alpha = 0f; var ptr = _panelCg.transform; float t = 0f; const float d = 0.30f;
                while (t < d && _panelCg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; ptr.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, k); yield return null; }
                if (_panelCg != null) { _panelCg.alpha = 1f; ptr.localScale = Vector3.one; }
            }

            // Rows stagger in top→bottom (each slide 12px from right + fade, 0.05s stagger).
            if (_rowCg != null)
            {
                for (int i = 0; i < _rowCg.Length; i++)
                {
                    if (_rowCg[i] == null) continue;
                    StartCoroutine(RowIn(i));
                    float s = 0f; while (s < 0.05f) { s += Time.unscaledDeltaTime; yield return null; }
                }
            }

            // Progress bars fill left→right to value (0.40s ease-out).
            float ft = 0f; const float fd = 0.40f;
            while (ft < fd)
            {
                ft += Time.unscaledDeltaTime; float k = 1f - (1f - Mathf.Clamp01(ft / fd)) * (1f - Mathf.Clamp01(ft / fd)); // ease-out
                if (_barFill != null) for (int i = 0; i < _barFill.Length; i++) if (_barFill[i] != null) _barFill[i].fillAmount = Mathf.Lerp(0f, _barTarget[i], k);
                if (_bonusFill != null) _bonusFill.fillAmount = Mathf.Lerp(0f, (float)_bonusView / BonusTotal, k);
                yield return null;
            }
            if (_barFill != null) for (int i = 0; i < _barFill.Length; i++) if (_barFill[i] != null) _barFill[i].fillAmount = _barTarget[i];
            if (_bonusFill != null) _bonusFill.fillAmount = Mathf.Clamp01((float)_bonusView / BonusTotal);
        }

        private IEnumerator RowIn(int i)
        {
            var cg = _rowCg[i]; if (cg == null) yield break;
            var rt = (RectTransform)cg.transform; Vector2 home = rt.anchoredPosition; Vector2 from = home + new Vector2(12f, 0f);
            cg.alpha = 0f; rt.anchoredPosition = from; float t = 0f; const float d = 0.18f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = _claimedView[i] ? k * 0.7f : k; rt.anchoredPosition = Vector2.Lerp(from, home, k); yield return null; }
            if (cg != null) { cg.alpha = _claimedView[i] ? 0.7f : 1f; rt.anchoredPosition = home; }
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
