// BULWARK — FREE REWARDS (UI Construction Bible · 30). Presentation-only, REMOVABLE.
//
// §12 boundary: presentation only — NO ECS / NO Unity.Entities / NO gameplay/balance/AI/economy/backend. This is a
// forensic code-built rebuild of design/FreeRewardsDesign.png (Bible 30): the compliant, strictly-opt-in rewarded-ad
// hub — obsidian backdrop + vignette + warm top glow; header (Back tile · "FREE REWARDS" gold-bevel serif title +
// subtitle · 3 currency chips with "+"); a DAILY LIMIT bar (calendar "3/5 today" + clock "Resets in: 14h 23m"); a
// VerticalLayoutGroup-shaped list of exactly 4 offer rows (Gem Cache +50 / Silver Stash +200 / Free Chest FREE /
// Battle Boost 60m Speed Up), each = {thumbnail + gold play-triangle, title + 2-line desc, reward chip, blue WATCH +
// "1/1" tag}; and a RECENT WINS footer + "Ads are short and optional. Thanks for your support!" reassurance line.
// Offers are clearly DISPLAY-ONLY local stub data: WATCH is opt-in + display-only (Router.Toast, decrements a LOCAL
// view counter only) — it never auto-plays an ad and never mutates a real balance (server-authoritative grant; §12 L/K).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-30 landscape Free Rewards (opt-in rewarded-ad offer list). Presentation-only.</summary>
    public sealed class FreeRewardsScreen : UiScreen
    {
        // ============================================================ Layout (Section E) ============================================================
        // fy = fraction from TOP; anchorY = 1 − fy. px: x = frac·2340, y = frac·1080. Sizes in those px units.
        private const float W = 2340f, H = 1080f;

        // OfferList central band: fy 0.20 (below limit bar) → 0.88 (above footer); height ≈ 0.68·H ≈ 734.
        private const float ListXMin = 0.030f, ListXMax = 0.970f;          // row width ≈ 0.94·W ≈ 2200
        private const float ListYMinFy = 0.20f, ListYMaxFy = 0.88f;        // from TOP
        private const float ListYMin = 1f - ListYMaxFy;                    // anchorY 0.12
        private const float ListYMax = 1f - ListYMinFy;                    // anchorY 0.80
        private const float ListW = (ListXMax - ListXMin) * W;             // ≈2200
        private const float RowH = 0.157f * H;                            // ≈169
        private const float RowGap = 0.018f * H;                          // ≈19
        private const float RowPitch = RowH + RowGap;                      // ≈188

        // ============================================================ Exact hex (Sections F/G) ============================================================
        private static readonly Color BackdropTop  = Hex("#14161e"); // obsidian top
        private static readonly Color BackdropBot  = Hex("#0a0b0f"); // obsidian base
        private static readonly Color TopGlowWarm  = Hex("#2a2416"); // faint warm top glow (additive feel)
        private static readonly Color SlateDark    = Hex("#11141d"); // back tile / chips / limit bar slate
        private static readonly Color BronzeEdgeLo = Hex("#7a5f28"); // bronze edge low
        private static readonly Color RowTop       = Hex("#181c27"); // offer row panel top
        private static readonly Color RowBot       = Hex("#10131c"); // offer row panel bottom
        private static readonly Color ThumbTop     = Hex("#3a3326"); // thumbnail loot-art stand-in top
        private static readonly Color ThumbBot     = Hex("#1c1a14"); // thumbnail loot-art stand-in bottom
        private static readonly Color PlayDisc     = Hex("#0a0b0f"); // play-overlay translucent dark disc
        private static readonly Color PlayTri      = Hex("#f0d27a"); // gold play-triangle
        private static readonly Color WatchHi      = Hex("#4f8bff"); // WATCH gloss top (cobalt high)
        private static readonly Color WatchLo      = Hex("#2b56c8"); // WATCH gloss bottom (cobalt deep)
        private static readonly Color WatchDisHi   = Hex("#3a4a6a"); // WATCH disabled grey-blue
        private static readonly Color WatchDisLo   = Hex("#26303f"); // WATCH disabled grey-blue dark
        private static readonly Color AvailPlate   = Hex("#0d1018"); // small dark plate under "1/1"
        private static readonly Color FooterCol    = Hex("#0d0f16"); // footer strip dark

        private static readonly Color C_Subtitle   = Hex("#d9c79a"); // subtitle
        private static readonly Color C_CurrencyNum = Hex("#ffffff"); // currency numbers
        private static readonly Color C_LimitCaps  = Hex("#cdbf99"); // "DAILY LIMIT" / "RECENT WINS"
        private static readonly Color C_Count      = Hex("#ffd76a"); // "3/5"
        private static readonly Color C_CountToday = Hex("#b3ac96"); // "today" / "Resets in:"
        private static readonly Color C_Timer      = Hex("#ffe08a"); // "14h 23m"
        private static readonly Color C_OfferTitle = Hex("#f2e8cf"); // offer title
        private static readonly Color C_OfferDesc  = Hex("#a9a28c"); // offer description
        private static readonly Color C_RewardUnit = Hex("#9a937f"); // reward unit ("Gems"/"Silver"/…)
        private static readonly Color C_WatchTxt   = Hex("#ffffff"); // "WATCH"
        private static readonly Color C_WatchDisTxt = Hex("#6a748c"); // disabled WATCH label
        private static readonly Color C_AvailTag   = Hex("#cfe0ff"); // "1/1"
        private static readonly Color C_InfoNote   = Hex("#8f8872"); // reassurance line

        // Reward accent colours by type (Section F): gem #6f8fff, silver #d6dae2, FREE #5fbf6a, boost #6f8fff.
        private static readonly Color C_RewardGem   = Hex("#6f8fff");
        private static readonly Color C_RewardSilver = Hex("#d6dae2");
        private static readonly Color C_RewardFree  = Hex("#5fbf6a");
        private static readonly Color C_RewardBoost = Hex("#6f8fff");

        // ============================================================ Header / limit-bar copy (display-only consts) ============================================================
        // Currency chip values are verbatim from the mock (Section M) — DISPLAY-ONLY (NOT UiStub.Gold/Gems, which differ).
        private const string ChipGoldVal = "128,450", ChipSilverVal = "87,360", ChipGemsVal = "2,850";
        private const string DailyLimitTitle = "DAILY LIMIT";
        private const int DailyUsed = 3, DailyCap = 5;        // "3/5 today"
        private const string ResetLabel = "Resets in:";
        private const string ResetTimerText = "14h 23m";     // client display of server reset time (no local reset auth)
        private const string ReassuranceText = "Ads are short and optional. Thanks for your support!";

        // ============================================================ Offer stub data (Section A/C/L verbatim — DISPLAY-ONLY) ============================================================
        private enum RewardKind { Gem, Silver, Free, Boost }
        private struct Offer
        {
            public string Title, DescLine2, Amount, Unit;
            public RewardKind Kind;
            public int Avail, AvailMax;   // per-offer availability ("1/1")
        }

        // Exactly the 4 rows + values (Negative-Rule L: never invent extra offers; boost stays a "Speed Up" time-skip).
        private static readonly Offer[] Offers =
        {
            new Offer{ Title="Gem Cache",    DescLine2="to earn Gems!",      Amount="+50",  Unit="Gems",     Kind=RewardKind.Gem,    Avail=1, AvailMax=1 },
            new Offer{ Title="Silver Stash", DescLine2="to earn Silver!",    Amount="+200", Unit="Silver",   Kind=RewardKind.Silver, Avail=1, AvailMax=1 },
            new Offer{ Title="Free Chest",   DescLine2="to earn a Chest!",   Amount="FREE", Unit="Chest",    Kind=RewardKind.Free,   Avail=1, AvailMax=1 },
            new Offer{ Title="Battle Boost", DescLine2="to earn a 60m Boost!", Amount="60m", Unit="Speed Up", Kind=RewardKind.Boost,  Avail=1, AvailMax=1 },
        };
        private const string DescLine1 = "Watch a short video"; // shared first line (2-line desc)

        // RECENT WINS mini-chips (Section C verbatim): +50 gem, +200 silver, +50 gem, +200 silver, chest.
        private struct MiniWin { public string Amount; public Color Col; public bool Diamond; }
        private static readonly MiniWin[] RecentWins =
        {
            new MiniWin{ Amount="+50",  Col=Hex("#6f8fff"), Diamond=true  },
            new MiniWin{ Amount="+200", Col=Hex("#d6dae2"), Diamond=false },
            new MiniWin{ Amount="+50",  Col=Hex("#6f8fff"), Diamond=true  },
            new MiniWin{ Amount="+200", Col=Hex("#d6dae2"), Diamond=false },
            new MiniWin{ Amount="",     Col=Hex("#a05b25"), Diamond=false }, // chest
        };

        // ============================================================ Animation / state handles (presentation-only) ============================================================
        private RectTransform _topBar, _limitBar, _footer;
        private CanvasGroup[] _rowCg;
        private Button[] _watchBtns;
        private Text[] _watchLabels;
        private Image[] _watchBodies;
        private Image[] _playTris;
        private Button[] _playOverlays;
        private RectTransform[] _rewardIcons;
        private Text[] _availTags;
        private int[] _availView;            // LOCAL display-only per-offer availability (NEVER a real balance/grant)
        private int _dailyUsedView;          // LOCAL display-only daily counter (server-authoritative in production)
        private Text _countLabel;

        protected override void Build()
        {
            BuildBackdrop();   // Rect: full-bleed bg + FX
            BuildTopBar();     // SafeContent: Back + title + subtitle + currency chips
            BuildLimitBar();   // SafeContent: DAILY LIMIT + reset timer
            BuildOfferList();  // SafeContent: 4 offer rows (VLG-shaped, ScrollRect-ready)
            BuildFooter();     // SafeContent: RECENT WINS + reassurance line
        }

        // ------------------------------------------------------------ Backdrop (Rect, full-bleed) ------------------------------------------------------------
        private void BuildBackdrop()
        {
            // Obsidian field #0a0b0f→#14161e, matte (no central hero subject — calm utility screen, Section B).
            UiWidgets.Stretch("FullBleedBackdrop", Rect, UiTheme.Obsidian, "bg_menu");
            var grade = UiWidgets.Stretch("BG_ObsidianGrade", Rect, Color.white);
            grade.raycastTarget = false; grade.sprite = UiTex.VGradient(BackdropTop, BackdropBot, 64);
            // Faint warm top glow (Section G: #2a2416 additive feel) — soft, upper-centre.
            UiWidgets.Glow(Rect, UiTheme.A(TopGlowWarm, 0.55f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(1700, 700), 1.5f);
            // Faint vignette (calmer than the wheel/calendar). Matte backdrop does not bloom.
            UiWidgets.Vignette(Rect, 0.45f);
            // Faint dust motes (Section J) over the central band.
            var motes = UiWidgets.Rect("DustMotes", Rect, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(1900, 800));
            var ef = motes.gameObject.AddComponent<EmberField>(); ef.count = 10; ef.color = UiTheme.A(Hex("#cdbf99"), 0.35f);
        }

        // ------------------------------------------------------------ Top bar (Back · title · subtitle · chips) ------------------------------------------------------------
        private void BuildTopBar()
        {
            // TopBar: top-stretch, height ≈ 0.13·H ≈ 140, top inset ≈ 0.02·H.
            _topBar = UiWidgets.Rect("TopBar", SafeContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(0.02f + 0.065f) * H), new Vector2(0, 0.13f * H));
            _topBar.offsetMin = new Vector2(0, _topBar.offsetMin.y); _topBar.offsetMax = new Vector2(0, _topBar.offsetMax.y);

            // Back tile (top-left gold-framed ← tile) — task-mandated signature on SafeContent (parents to safe root, never crosses inset).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // TitleBlock (left of centre, after Back). Title "FREE REWARDS" gold-bevel serif (cap≈64 → ~86px legacy).
            UiWidgets.TitleLabel(SafeContent, "FREE REWARDS", 64, new Vector2(0.165f, 0.940f), new Vector2(0.165f, 0.940f),
                Vector2.zero, new Vector2(760, 96), TextAnchor.MiddleLeft, UiTheme.GoldHi, UiTheme.Gold);
            // Subtitle (~24) below the title.
            UiWidgets.Label(SafeContent, "Watch ads to earn valuable rewards!", 24, new Vector2(0.165f, 0.885f), new Vector2(0.165f, 0.885f),
                Vector2.zero, new Vector2(760, 36), TextAnchor.MiddleLeft, C_Subtitle);

            // CurrencyChips (top-right): Gold | Silver | Gems, each {icon, number, + mint button}. Built right→left.
            // chip width ≈ 0.13·W ≈ 304, height ≈ 0.06·H, gap ≈ 0.012·W. Right inset ≈ 0.02·W.
            float chipW = 0.135f * W, chipH = 0.062f * H, gap = 0.012f * W;
            float rightInset = 0.02f * W;
            // index 0 = rightmost = Gems (mirrors the mock order Gold,Silver,Gems with Gems on the right edge).
            BuildCurrencyChip("Chip_Gems",   ChipGemsVal,   UiTheme.AmethystHi, 0, chipW, chipH, gap, rightInset);
            BuildCurrencyChip("Chip_Silver", ChipSilverVal, Hex("#c8cdd6"),     1, chipW, chipH, gap, rightInset);
            BuildCurrencyChip("Chip_Gold",   ChipGoldVal,   UiTheme.Gold,       2, chipW, chipH, gap, rightInset);
        }

        private void BuildCurrencyChip(string name, string value, Color iconCol, int index, float chipW, float chipH, float gap, float rightInset)
        {
            float x = -(rightInset + chipW * 0.5f) - index * (chipW + gap);
            var chip = UiWidgets.Rect(name, SafeContent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(x, -(0.02f + 0.031f) * H), new Vector2(chipW, chipH));
            var bg = chip.gameObject.AddComponent<Image>();
            var ps = UiWidgets.Spr("panel"); if (ps != null) { bg.sprite = ps; bg.type = Image.Type.Sliced; }
            bg.color = UiTheme.A(SlateDark, 0.92f);
            // Bronze/gold edge.
            var rim = UiWidgets.Rect("ChipRim", chip, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.Gold, BronzeEdgeLo, UiTheme.GoldShadow, 48, 5); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
            // Icon (crystalline swatch).
            var icon = UiWidgets.Rect("Icon", chip, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(46, 46));
            var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false; iImg.sprite = UiTex.Diamond(iconCol, 48);
            // Number (~26 white, SemiBold, shadow already in Label).
            UiWidgets.Label(chip, value, 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(78, 0), new Vector2(chipW - 150, 36), TextAnchor.MiddleLeft, C_CurrencyNum);
            // "+" mint button at chip right (deep-links to Store, Section K) — display-only Toast here.
            var plusD = chipH * 0.58f;
            UiWidgets.Button(chip, "+", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-plusD * 0.5f - 12f, 0), new Vector2(plusD, plusD), Hex("#2f9c43"), () => Router.Toast("Store — coming soon"), 38);
        }

        // ------------------------------------------------------------ Daily-limit bar ------------------------------------------------------------
        private void BuildLimitBar()
        {
            _dailyUsedView = DailyUsed;
            // Full inner width ≈ 0.94·W, height ≈ 0.055·H ≈ 60, y just below TopBar (fy≈0.165 → anchorY 0.835).
            _limitBar = UiWidgets.Rect("DailyLimitBar", SafeContent, new Vector2(0.5f, 0.835f), new Vector2(0.5f, 0.835f), Vector2.zero, new Vector2(0.94f * W, 0.055f * H));
            var bg = _limitBar.gameObject.AddComponent<Image>();
            var ps = UiWidgets.Spr("panel"); if (ps != null) { bg.sprite = ps; bg.type = Image.Type.Sliced; }
            bg.color = UiTheme.A(SlateDark, 0.85f);
            var rim = UiWidgets.Rect("LimitRim", _limitBar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.Gold, 0.7f), UiTheme.A(BronzeEdgeLo, 0.8f), UiTheme.A(UiTheme.GoldShadow, 0.7f), 48, 4); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            float sideInset = 0.03f * W;
            // ---- Left cluster: calendar glyph + "DAILY LIMIT" + count "3/5 today" ----
            var calIcon = UiWidgets.Rect("CalendarIcon", _limitBar, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(sideInset, 0), new Vector2(34, 34));
            var calImg = calIcon.gameObject.AddComponent<Image>(); calImg.raycastTarget = false; calImg.sprite = UiTex.Solid(C_LimitCaps);
            UiWidgets.Label(_limitBar, UiTheme.Track(DailyLimitTitle), 22, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(sideInset + 50, 0), new Vector2(260, 30), TextAnchor.MiddleLeft, C_LimitCaps);
            // Count "3/5" emphatic + " today" muted (two labels so "3/5" reads brighter than "today").
            _countLabel = UiWidgets.Label(_limitBar, _dailyUsedView + "/" + DailyCap, 24, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(sideInset + 310, 0), new Vector2(70, 30), TextAnchor.MiddleLeft, C_Count);
            UiWidgets.Label(_limitBar, "today", 22, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(sideInset + 372, 0), new Vector2(120, 30), TextAnchor.MiddleLeft, C_CountToday);

            // ---- Right cluster: clock glyph + "Resets in:" + timer "14h 23m" ----
            var timerLbl = UiWidgets.Label(_limitBar, ResetTimerText, 24, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-sideInset, 0), new Vector2(160, 30), TextAnchor.MiddleRight, C_Timer);
            timerLbl.gameObject.AddComponent<UiGradientText>(); // soft glow feel on the timer (Section F)
            UiWidgets.Label(_limitBar, ResetLabel, 20, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-sideInset - 170, 0), new Vector2(160, 30), TextAnchor.MiddleRight, C_CountToday);
            var clkIcon = UiWidgets.Rect("ClockIcon", _limitBar, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-sideInset - 340, 0), new Vector2(32, 32));
            var clkImg = clkIcon.gameObject.AddComponent<Image>(); clkImg.raycastTarget = false; clkImg.sprite = UiTex.Disc(C_Timer, 48);
        }

        // ------------------------------------------------------------ Offer list (4 rows; VLG-shaped, ScrollRect-ready) ------------------------------------------------------------
        private void BuildOfferList()
        {
            int n = Offers.Length;
            _rowCg = new CanvasGroup[n];
            _watchBtns = new Button[n];
            _watchLabels = new Text[n];
            _watchBodies = new Image[n];
            _playTris = new Image[n];
            _playOverlays = new Button[n];
            _rewardIcons = new RectTransform[n];
            _availTags = new Text[n];
            _availView = new int[n];
            for (int i = 0; i < n; i++) _availView[i] = Offers[i].Avail;

            // ScrollRect host over the list band (canvas x 0.030→0.970, anchorY 0.12→0.80). Header/limit-bar/footer pinned
            // OUTSIDE this viewport. All 4 fit, so it never scrolls — but it is ScrollRect-ready for future offers (Section D/E).
            var host = UiWidgets.Rect("OfferList", SafeContent, new Vector2(ListXMin, ListYMin), new Vector2(ListXMax, ListYMax), Vector2.zero, Vector2.zero);
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30f;

            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0); vImg.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content: top-anchored, exactly tall enough for 4 rows + 3 gaps (≈753) — equal-height rows like a VLG.
            float totalH = n * RowH + (n - 1) * RowGap;
            var content = UiWidgets.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, totalH));
            content.pivot = new Vector2(0.5f, 1f);
            scroll.viewport = viewport; scroll.content = content;

            for (int i = 0; i < n; i++) BuildOfferRow(content, Offers[i], i);
        }

        private void BuildOfferRow(RectTransform content, Offer o, int i)
        {
            // Row: top-stretch within content, fixed height (equal rows), own CanvasGroup for stagger fade.
            var rowGo = new GameObject("OfferRow_" + o.Title.Replace(" ", ""), typeof(RectTransform), typeof(CanvasGroup));
            var row = (RectTransform)rowGo.transform; row.SetParent(content, false);
            row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1f);
            row.offsetMin = new Vector2(0, 0); row.offsetMax = new Vector2(0, 0);
            row.sizeDelta = new Vector2(0, RowH); row.anchoredPosition = new Vector2(0, -i * RowPitch);
            _rowCg[i] = rowGo.GetComponent<CanvasGroup>();

            // Row panel: #10131c→#181c27 vertical, thin gold-bronze edge, soft inner top sheen.
            var panel = rowGo.AddComponent<Image>(); panel.raycastTarget = false;
            panel.sprite = UiTex.VGradient(RowTop, RowBot, 64);
            var rim = UiWidgets.Rect("RowRim", row, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.Gold, 0.85f), UiTheme.A(BronzeEdgeLo, 0.9f), UiTheme.A(UiTheme.GoldShadow, 0.8f), 48, 4); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            // Row-internal columns are fractions of ROW WIDTH (≈2200). Convert each centre to a 0..1 row-local x.
            // Thumb 0.00–0.14 | Text 0.16–0.52 | RewardChip 0.54–0.70 | Action 0.78–0.98.
            float thumbW = (0.14f - 0.005f) * ListW;                 // inset slightly from the very edge
            float thumbCx = 0.075f;                                   // centre of 0.00–0.14, with left padding
            float rewardCx = 0.62f;                                   // centre of 0.54–0.70
            float actionCx = 0.88f;                                   // centre of 0.78–0.98

            // ---- Thumb (loot/faction art stand-in) + bronze inner frame + PlayOverlay ----
            float thumbH = RowH * 0.78f;
            var thumb = UiWidgets.Rect("Thumb", row, new Vector2(thumbCx, 0.5f), new Vector2(thumbCx, 0.5f), Vector2.zero, new Vector2(thumbW, thumbH));
            var thumbImg = thumb.gameObject.AddComponent<Image>(); thumbImg.raycastTarget = false; thumbImg.sprite = UiTex.VGradient(ThumbTop, ThumbBot, 64);
            // Slight darkening vignette over the art.
            var thumbVig = UiWidgets.Rect("ThumbVig", thumb, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tvImg = thumbVig.gameObject.AddComponent<Image>(); tvImg.raycastTarget = false; tvImg.sprite = UiTex.Radial(UiTheme.A(Color.black, 0f), UiTheme.A(Color.black, 0.45f), 64, 1.6f);
            // Bronze inner frame.
            var thumbRim = UiWidgets.Rect("ThumbRim", thumb, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var trImg = thumbRim.gameObject.AddComponent<Image>(); trImg.sprite = UiTex.Frame(UiTheme.A(BronzeEdgeLo, 0.95f), UiTheme.A(UiTheme.GoldShadow, 0.9f), UiTheme.A(Color.black, 0.7f), 48, 5); trImg.type = Image.Type.Sliced; trImg.raycastTarget = false;

            // PlayOverlay: translucent dark disc + gold play-triangle, centred on the thumb (⌀ ≈ 0.55·thumbH).
            float discD = thumbH * 0.55f;
            var play = UiWidgets.Rect("PlayOverlay", thumb, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(discD, discD));
            var discImg = play.gameObject.AddComponent<Image>(); discImg.sprite = UiTex.Disc(UiTheme.A(PlayDisc, 0.62f), 64);
            var playBtn = play.gameObject.AddComponent<Button>(); playBtn.targetGraphic = discImg;
            var pcb = playBtn.colors; pcb.normalColor = Color.white; pcb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); pcb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); pcb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); pcb.fadeDuration = 0.08f; playBtn.colors = pcb;
            int pidx = i;
            playBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnWatch(pidx); });
            _playOverlays[i] = playBtn;
            // Gold play-triangle glyph (Diamond stand-in for ▷) with a soft glow.
            UiWidgets.Glow(play, UiTheme.A(PlayTri, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(discD * 1.3f, discD * 1.3f), 1.6f);
            var tri = UiWidgets.Rect("PlayTri", play, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(2, 0), new Vector2(discD * 0.46f, discD * 0.46f));
            var triImg = tri.gameObject.AddComponent<Image>(); triImg.raycastTarget = false; triImg.sprite = UiTex.Diamond(PlayTri, 32);
            tri.localRotation = Quaternion.Euler(0, 0, 45); // diamond → triangle-ish play hint
            _playTris[i] = triImg;

            // ---- Text block: Title + 2-line description (flex column 0.16–0.52) ----
            // Anchor the text at row-local x≈0.165, top-aligned title then desc lines.
            UiWidgets.Label(row, o.Title, 32, new Vector2(0.165f, 0.66f), new Vector2(0.165f, 0.66f), Vector2.zero, new Vector2(760, 40), TextAnchor.MiddleLeft, C_OfferTitle);
            UiWidgets.Label(row, DescLine1, 20, new Vector2(0.165f, 0.40f), new Vector2(0.165f, 0.40f), Vector2.zero, new Vector2(760, 28), TextAnchor.MiddleLeft, C_OfferDesc);
            UiWidgets.Label(row, o.DescLine2, 20, new Vector2(0.165f, 0.22f), new Vector2(0.165f, 0.22f), Vector2.zero, new Vector2(760, 28), TextAnchor.MiddleLeft, C_OfferDesc);

            // ---- RewardChip: icon + amount (large, coloured) + unit (small beneath) ----
            Color rewardCol = RewardColor(o.Kind);
            var chip = UiWidgets.Rect("RewardChip", row, new Vector2(rewardCx, 0.5f), new Vector2(rewardCx, 0.5f), Vector2.zero, new Vector2(0.16f * ListW, RowH * 0.8f));
            // Reward icon ⌀ ≈ 0.06·H, crystalline (gem/silver/chest/boost stand-ins).
            float rIconD = 0.06f * H;
            var rIcon = UiWidgets.Rect("Reward_Icon", chip, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), Vector2.zero, new Vector2(rIconD, rIconD));
            var rIconImg = rIcon.gameObject.AddComponent<Image>(); rIconImg.raycastTarget = false;
            rIconImg.sprite = RewardIconSprite(o.Kind, rewardCol);
            _rewardIcons[i] = rIcon;
            // Amount (~34 heavy, coloured-by-reward, glow).
            var amt = UiWidgets.Label(chip, o.Amount, 34, new Vector2(0.62f, 0.62f), new Vector2(0.62f, 0.62f), Vector2.zero, new Vector2(220, 42), TextAnchor.MiddleCenter, rewardCol);
            amt.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.7f);
            // Unit (~18 small beneath).
            UiWidgets.Label(chip, o.Unit, 18, new Vector2(0.62f, 0.26f), new Vector2(0.62f, 0.26f), Vector2.zero, new Vector2(220, 26), TextAnchor.MiddleCenter, C_RewardUnit);

            // ---- Action: WATCH button + "1/1" tag beneath (right column 0.78–0.98) ----
            // WatchButton: width ≈ 0.14·W ≈ 328, height ≈ 0.085·H ≈ 92. Anchor it slightly above centre; tag beneath.
            float watchW = 0.14f * W, watchH = 0.085f * H;
            var watch = UiWidgets.Rect("WatchButton", row, new Vector2(actionCx, 0.60f), new Vector2(actionCx, 0.60f), Vector2.zero, new Vector2(watchW, watchH));
            // Outer glow (rim bloom), pulses on idle.
            var wGlow = UiWidgets.Glow(watch, UiTheme.A(WatchHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(watchW * 1.25f, watchH * 1.7f), 1.7f);
            wGlow.name = "WatchGlow";
            var watchBody = watch.gameObject.AddComponent<Image>(); watchBody.sprite = UiTex.VGradient(WatchHi, WatchLo, 32);
            var watchBtn = watch.gameObject.AddComponent<Button>(); watchBtn.targetGraphic = watchBody;
            var wcb = watchBtn.colors; wcb.normalColor = Color.white; wcb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); wcb.pressedColor = new Color(0.85f, 0.85f, 0.92f, 1f); wcb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); wcb.fadeDuration = 0.08f; watchBtn.colors = wcb;
            int widx = i;
            watchBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnWatch(widx); });
            // Beveled gold/steel trim + top highlight.
            var watchRim = UiWidgets.Rect("WatchRim", watch, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var watchRimImg = watchRim.gameObject.AddComponent<Image>(); watchRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); watchRimImg.type = Image.Type.Sliced; watchRimImg.raycastTarget = false;
            var topHi = UiWidgets.Rect("WatchTopHi", watch, new Vector2(0, 0.6f), new Vector2(1, 1f), Vector2.zero, new Vector2(-14, -8));
            var topHiImg = topHi.gameObject.AddComponent<Image>(); topHiImg.raycastTarget = false; topHiImg.sprite = UiTex.VGradient(UiTheme.A(Color.white, 0.3f), UiTheme.A(Color.white, 0f), 32);
            var watchLbl = UiWidgets.Label(watch, UiTheme.Track("WATCH"), 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, C_WatchTxt);
            watchLbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.8f);
            _watchBtns[i] = watchBtn; _watchLabels[i] = watchLbl; _watchBodies[i] = watchBody;

            // AvailTag "1/1" on a small dark plate, centred beneath the WATCH button.
            var tagPlate = UiWidgets.Rect("AvailPlate", row, new Vector2(actionCx, 0.18f), new Vector2(actionCx, 0.18f), Vector2.zero, new Vector2(0.06f * W, 0.05f * H));
            var tagImg = tagPlate.gameObject.AddComponent<Image>(); tagImg.raycastTarget = false;
            var tps = UiWidgets.Spr("panel"); if (tps != null) { tagImg.sprite = tps; tagImg.type = Image.Type.Sliced; }
            tagImg.color = UiTheme.A(AvailPlate, 0.9f);
            _availTags[i] = UiWidgets.Label(tagPlate, _availView[i] + "/" + o.AvailMax, 18, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, C_AvailTag);

            ApplyOfferState(i);
        }

        // available = full colour, WATCH enabled (blue + glow + breathe), play-triangle bright; used (0/1) or cap-reached →
        // WATCH disabled grey-blue, play-triangle desaturated, row dims (Section H).
        private void ApplyOfferState(int i)
        {
            bool capReached = _dailyUsedView >= DailyCap;
            bool offerUsed = _availView[i] <= 0;
            bool enabled = !offerUsed && !capReached;

            var body = _watchBodies[i];
            var lbl = _watchLabels[i];
            var btn = _watchBtns[i];
            var play = _playOverlays[i];
            var tri = _playTris[i];

            // Clear any prior idle breathe on the WATCH body/glow.
            if (body != null) { var old = body.GetComponent<PulseGraphic>(); if (old != null) Destroy(old); }

            if (enabled)
            {
                if (body != null) body.sprite = UiTex.VGradient(WatchHi, WatchLo, 32);
                if (lbl != null) lbl.color = C_WatchTxt;
                if (tri != null) tri.color = PlayTri;
                if (_rowCg[i] != null) _rowCg[i].alpha = 1f;
                // Buttons stay interactable even when disabled-state (Section H: disabled raycast on → tap = shake + toast);
                // here we keep them interactable and branch inside OnWatch so the toast/shake fires.
                if (btn != null) btn.interactable = true;
                if (play != null) play.interactable = true;
            }
            else
            {
                if (body != null) body.sprite = UiTex.VGradient(WatchDisHi, WatchDisLo, 32);
                if (lbl != null) lbl.color = C_WatchDisTxt;
                if (tri != null) tri.color = UiTheme.A(Hex("#8d8a7e"), 0.8f); // desaturated play-triangle
                if (_rowCg[i] != null) _rowCg[i].alpha = 0.7f;               // row dims to ~70%
                // Keep interactable so a tap still produces feedback (shake + toast) per Section H, but grants nothing.
                if (btn != null) btn.interactable = true;
                if (play != null) play.interactable = true;
            }
        }

        // ------------------------------------------------------------ Footer (RECENT WINS · reassurance) ------------------------------------------------------------
        private void BuildFooter()
        {
            // FooterStrip: bottom-stretch, height ≈ 0.09·H ≈ 97, bottom inset ≈ 0.02·H.
            _footer = UiWidgets.Rect("FooterStrip", SafeContent, new Vector2(0.5f, 0.065f), new Vector2(0.5f, 0.065f), Vector2.zero, new Vector2(0.94f * W, 0.09f * H));
            var bg = _footer.gameObject.AddComponent<Image>(); bg.raycastTarget = false;
            var ps = UiWidgets.Spr("panel"); if (ps != null) { bg.sprite = ps; bg.type = Image.Type.Sliced; }
            bg.color = UiTheme.A(FooterCol, 0.92f);
            UiWidgets.Divider(_footer, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, 0.9f * W, 2f, UiTheme.A(UiTheme.GoldShadow, 0.7f));

            float sideInset = 0.025f * W;
            // ---- Left: "RECENT WINS" + 5 mini chips ----
            UiWidgets.Label(_footer, UiTheme.Track("RECENT WINS"), 20, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(sideInset, 0), new Vector2(280, 30), TextAnchor.MiddleLeft, C_LimitCaps);
            float miniD = 0.05f * H, miniGap = 0.012f * W;
            float startX = sideInset + 260f;
            for (int k = 0; k < RecentWins.Length; k++)
            {
                var win = RecentWins[k];
                float cx = startX + k * (miniD * 2.0f + miniGap);
                var mini = UiWidgets.Rect("MiniWin_" + k, _footer, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(cx, 0), new Vector2(miniD, miniD));
                var mImg = mini.gameObject.AddComponent<Image>(); mImg.raycastTarget = false;
                mImg.sprite = win.Diamond ? UiTex.Diamond(win.Col, 48) : UiTex.Disc(win.Col, 48);
                if (!string.IsNullOrEmpty(win.Amount))
                    UiWidgets.Label(_footer, win.Amount, 18, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(cx + miniD * 0.7f, 0), new Vector2(70, 26), TextAnchor.MiddleLeft, UiTheme.A(win.Col, 1f));
            }

            // ---- Right: info icon + reassurance line (Negative-Rule L: must not drop this) ----
            UiWidgets.Label(_footer, ReassuranceText, 18, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-sideInset, 0), new Vector2(760, 28), TextAnchor.MiddleRight, C_InfoNote);
            var info = UiWidgets.Rect("InfoIcon", _footer, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-sideInset - 770f, 0), new Vector2(30, 30));
            var infoImg = info.gameObject.AddComponent<Image>(); infoImg.raycastTarget = false; infoImg.sprite = UiTex.Disc(UiTheme.A(C_InfoNote, 0.9f), 48);
        }

        // ------------------------------------------------------------ Reward-icon / colour helpers ------------------------------------------------------------
        private static Color RewardColor(RewardKind k)
        {
            switch (k)
            {
                case RewardKind.Gem: return C_RewardGem;
                case RewardKind.Silver: return C_RewardSilver;
                case RewardKind.Free: return C_RewardFree;
                default: return C_RewardBoost;
            }
        }

        private static Sprite RewardIconSprite(RewardKind k, Color col)
        {
            switch (k)
            {
                case RewardKind.Gem:    return UiTex.Diamond(col, 48);                 // crystalline gem
                case RewardKind.Silver: return UiTex.Disc(col, 48);                   // silver coin
                case RewardKind.Free:   return UiTex.Solid(Hex("#a05b25"));           // aged-wood chest stand-in
                default:                return UiTex.Diamond(col, 48);                 // boost shield/clock stand-in
            }
        }

        // ------------------------------------------------------------ Watch (strictly opt-in, DISPLAY-ONLY) ------------------------------------------------------------
        // Section L/K: opt-in only (this WATCH/▷ tap IS the explicit opt-in — nothing auto-plays); the reward is
        // server-authoritative — the UI NEVER mutates a real balance. Here it only updates LOCAL view counters + a Toast.
        private void OnWatch(int i)
        {
            if (_availView == null || i < 0 || i >= _availView.Length) return;

            // Cap-reached → disabled feedback (shake + toast), no grant.
            if (_dailyUsedView >= DailyCap)
            {
                StartCoroutine(Shake(_watchBtns[i] != null ? (RectTransform)_watchBtns[i].transform : null));
                Router.Toast("Daily limit reached — resets in " + ResetTimerText);
                return;
            }
            // Per-offer used → disabled feedback, no grant.
            if (_availView[i] <= 0)
            {
                StartCoroutine(Shake(_watchBtns[i] != null ? (RectTransform)_watchBtns[i].transform : null));
                Router.Toast("Already watched — resets in " + ResetTimerText);
                return;
            }

            // Opt-in rewarded ad would play here via the SDK (external, user-initiated). DISPLAY-ONLY stub:
            // simulate the completion callback path WITHOUT any real ad or balance write.
            _availView[i] = 0;                         // decrement per-offer availability (1/1 → 0/1) — local view only
            _dailyUsedView++;                          // increment daily counter (3/5 → 4/5) — local view only
            if (_availTags[i] != null) _availTags[i].text = _availView[i] + "/" + Offers[i].AvailMax;
            if (_countLabel != null) _countLabel.text = _dailyUsedView + "/" + DailyCap;

            Router.Toast(WatchToast(Offers[i]));        // no real ad, no real balance write (server-authoritative)
            StartCoroutine(RewardPop(i));

            // Re-apply states: this row becomes used; if 5/5 reached, ALL rows disable.
            if (_dailyUsedView >= DailyCap)
                for (int r = 0; r < _availView.Length; r++) ApplyOfferState(r);
            else
                ApplyOfferState(i);
        }

        private static string WatchToast(Offer o)
        {
            switch (o.Kind)
            {
                case RewardKind.Free: return "Reward earned: Free Chest (display-only)";
                case RewardKind.Boost: return "Reward earned: 60m Speed Up (display-only)";
                default: return "Reward earned: " + o.Amount + " " + o.Unit + " (display-only)";
            }
        }

        // Reward-chip pop acknowledgement (Section I: scale 1.0→1.25→1.0). Display-only flourish at the chip.
        private IEnumerator RewardPop(int i)
        {
            if (_rewardIcons == null || i < 0 || i >= _rewardIcons.Length || _rewardIcons[i] == null) yield break;
            var tr = _rewardIcons[i]; float t = 0f; const float d = 0.3f;
            while (t < d && tr != null)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                tr.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            if (tr != null) tr.localScale = Vector3.one;
        }

        // Disabled-tap shake (Section H/K: tap a disabled WATCH → shake + toast, no grant).
        private IEnumerator Shake(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 home = rt.anchoredPosition; float t = 0f; const float d = 0.25f;
            while (t < d && rt != null)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(t / d);
                rt.anchoredPosition = home + new Vector2(Mathf.Sin(t * 60f) * 10f * k, 0f);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = home;
        }

        // ------------------------------------------------------------ Lifecycle / enter timeline (Section I) ------------------------------------------------------------
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterTimeline());
        }

        // ~0.7s: TopBar slide-down → limit bar fade → rows stagger top→bottom → WATCH glow-on + breathe → footer fade-up.
        private IEnumerator EnterTimeline()
        {
            // TopBar slide-down (y +12→0 + α, 0.20s).
            if (_topBar != null) StartCoroutine(SlideFade(_topBar, new Vector2(0, 12f), 0.20f));
            // DailyLimitBar fade-in (0.15s).
            if (_limitBar != null) StartCoroutine(FadeChild(_limitBar, 0.15f));

            // Rows stagger top→bottom (each x −20→0 + α, 0.05s apart, 0.18s each, ease-out).
            if (_rowCg != null)
            {
                for (int i = 0; i < _rowCg.Length; i++)
                {
                    if (_rowCg[i] == null) continue;
                    StartCoroutine(RowIn(i));
                    float s = 0f; while (s < 0.05f) { s += Time.unscaledDeltaTime; yield return null; }
                }
            }

            // FooterStrip fade-up (0.15s).
            if (_footer != null) StartCoroutine(SlideFade(_footer, new Vector2(0, -12f), 0.15f));

            // WATCH buttons glow-on + begin breathe loop; play-triangles subtle pulse (only on enabled offers).
            float w = 0f; while (w < 0.1f) { w += Time.unscaledDeltaTime; yield return null; }
            StartIdleLoops();
        }

        private void StartIdleLoops()
        {
            for (int i = 0; i < (_watchBodies != null ? _watchBodies.Length : 0); i++)
            {
                bool enabled = _availView[i] > 0 && _dailyUsedView < DailyCap;
                if (enabled && _watchBodies[i] != null)
                {
                    // WATCH breathe (1.6s) on the outer glow (find the sibling glow under the button rect).
                    var glowTr = _watchBodies[i].transform.Find("WatchGlow");
                    var glowImg = glowTr != null ? glowTr.GetComponent<Image>() : null;
                    if (glowImg != null) { var pg = glowImg.gameObject.AddComponent<PulseGraphic>(); pg.target = glowImg; pg.min = 0.35f; pg.max = 0.7f; pg.period = 1.6f; }
                }
                // Play-triangle soft pulse (1.4s) on enabled offers.
                if (enabled && _playTris != null && _playTris[i] != null)
                {
                    var pp = _playTris[i].gameObject.AddComponent<PulseGraphic>(); pp.target = _playTris[i]; pp.min = 0.75f; pp.max = 1f; pp.period = 1.4f;
                }
            }
        }

        private IEnumerator RowIn(int i)
        {
            var cg = _rowCg[i]; if (cg == null) yield break;
            float homeAlpha = (_availView[i] > 0 && _dailyUsedView < DailyCap) ? 1f : 0.7f;
            var rt = (RectTransform)cg.transform; Vector2 home = rt.anchoredPosition; Vector2 from = home + new Vector2(-20f, 0f);
            cg.alpha = 0f; rt.anchoredPosition = from; float t = 0f; const float d = 0.18f;
            while (t < d && cg != null)
            {
                t += Time.unscaledDeltaTime; float lin = Mathf.Clamp01(t / d); float k = 1f - (1f - lin) * (1f - lin); // ease-out
                cg.alpha = homeAlpha * k; rt.anchoredPosition = Vector2.Lerp(from, home, k);
                yield return null;
            }
            if (cg != null) { cg.alpha = homeAlpha; rt.anchoredPosition = home; }
        }

        // Slide a RectTransform from an offset to home while fading its CanvasGroup (added on demand).
        private IEnumerator SlideFade(RectTransform rt, Vector2 fromOffset, float duration)
        {
            if (rt == null) yield break;
            var cg = rt.GetComponent<CanvasGroup>(); if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
            Vector2 home = rt.anchoredPosition; Vector2 from = home + fromOffset;
            cg.alpha = 0f; rt.anchoredPosition = from; float t = 0f;
            while (t < duration && rt != null)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / duration);
                cg.alpha = k; rt.anchoredPosition = Vector2.Lerp(from, home, k);
                yield return null;
            }
            if (rt != null) { cg.alpha = 1f; rt.anchoredPosition = home; }
        }

        private IEnumerator FadeChild(RectTransform rt, float duration)
        {
            if (rt == null) yield break;
            var cg = rt.GetComponent<CanvasGroup>(); if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f; float t = 0f;
            while (t < duration && rt != null) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(t / duration); yield return null; }
            if (cg != null) cg.alpha = 1f;
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
