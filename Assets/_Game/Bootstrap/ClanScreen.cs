// BULWARK — CLAN (UI Construction Bible · 35). Presentation-only, REMOVABLE.
//
// §12 boundary: presentation only — NO ECS / NO Unity.Entities / NO gameplay/balance/AI/economy/backend. This is
// a forensic code-built rebuild of design/ClanScreenDesign.png (Bible 35): the REAL Clan hub for "DRAGONFORGE" —
// a three-column social home (LEFT identity: blue dragon banner + name + motto + five vital stats; CENTER tabs
// MEMBERS/Activity/Clan War/Clan Chest with a scrolling member roster + action bar CLAN WAR·DONATE·CLAN SHOP·
// MANAGE·LEAVE CLAN; RIGHT a CHAT/Announcements panel + composer). All clan/member/chat data is local DISPLAY-ONLY
// stub data; roles/trophies/member-count/clan-gold are never edited by the client (server-authoritative, L/K §12)
// and every clan action is display-only (Router.Toast / Confirm-stub) — the UI never mutates any balance/ECS state.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-35 landscape Clan hub (identity / roster / chat). Presentation-only.</summary>
    public sealed class ClanScreen : UiScreen
    {
        // ============================================================================================
        // Canvas-fraction layout (Section E). fy = fraction from TOP; anchorY = 1 − fy. px = frac×2340/×1080.
        // ============================================================================================
        // TopBar height ≈0.095 (~103 px) → content band fy 0.10→0.99 ⇒ anchorY 0.01→0.90.
        private const float ContentTop = 0.90f;                // anchorY top of the column band (fy 0.10)
        private const float ContentBot = 0.01f;                // anchorY bottom (fy 0.99)

        // Three-column x bands (fractions of 2340w, ~1.2% gutters) — Section E.
        private const float LeftXMin = 0.012f, LeftXMax = 0.217f;   // ≈0.205w (~480)
        private const float CtrXMin  = 0.229f, CtrXMax  = 0.704f;   // ≈0.475w (~1112)
        private const float RgtXMin  = 0.716f, RgtXMax  = 0.988f;   // ≈0.272w (~636)

        // ============================================================================================
        // Exact spec hex not in UiTheme (Section F/G). Display-only.
        // ============================================================================================
        private static readonly Color TitleGold  = Hex("#f0d27a"); // clan name / roster header / trophies
        private static readonly Color BannerName  = Hex("#f4dd8c"); // clan name on banner cloth
        private static readonly Color MottoCream  = Hex("#cdbf9e"); // motto
        private static readonly Color StatCap     = Hex("#9a8a6a"); // stat captions / column labels / idle tab
        private static readonly Color StatVal     = Hex("#e9dcc0"); // stat values / level chip number
        private static readonly Color LangCream   = Hex("#d8cdb2"); // language pill
        private static readonly Color TabSel      = Hex("#ffffff"); // selected tab label
        private static readonly Color NameCream   = Hex("#f3ead2"); // member name
        private static readonly Color YouBlue     = Hex("#9fc0ff"); // "(You)" suffix / you-row tint
        private static readonly Color ActionGold  = Hex("#f0d27a"); // action-button labels
        private static readonly Color DangerWhite = Hex("#ffffff"); // LEAVE CLAN / SEND label
        private static readonly Color SenderGold  = Hex("#e7cf94"); // chat sender name
        private static readonly Color ChatTime    = Hex("#7a7060"); // chat timestamp
        private static readonly Color ChatBody    = Hex("#d9d2c2"); // chat body
        private static readonly Color Placeholder = Hex("#6f685a"); // composer placeholder

        // Role-pill text + chip colours (Section F/G).
        private static readonly Color RoleLeaderTxt  = Hex("#f0d27a"), RoleLeaderChip  = Hex("#caa04a");
        private static readonly Color RoleOfficerTxt = Hex("#8fb3ff"), RoleOfficerChip = Hex("#2b3a6a");
        private static readonly Color RoleVetTxt     = Hex("#cdb88a"), RoleVetChip     = Hex("#5a4a30");
        private static readonly Color RoleMemberTxt  = Hex("#b8b0a0"), RoleMemberChip  = Hex("#2a2a30");

        // Status taxonomy (Section F/G): Online green / In Battle amber / "Nh ago" grey.
        private static readonly Color OnlineGreen = Hex("#3fd07a");
        private static readonly Color InBattleAmb = Hex("#f0a23a");
        private static readonly Color AgoGrey     = Hex("#8c8270");

        // Banner cloth + crest (Section G).
        private static readonly Color BannerCore = Hex("#1d2f7a"), BannerMid = Hex("#2b56c8"), BannerSh = Hex("#0f1b4a");
        private static readonly Color CrestHi = Hex("#f6dd86"), CrestMid = Hex("#b8862c"), CrestSh = Hex("#6b4a14");
        private static readonly Color LeaveTop = Hex("#d8452b"), LeaveBot = Hex("#7a1f1a"); // oxblood→ember danger
        private static readonly Color PanelTop = Hex("#14161e"), PanelBot = Hex("#0c0e14"); // obsidian field

        // ============================================================================================
        // DISPLAY-ONLY local stub data (Section A/M verbatim). NEVER gameplay/economy; server-authoritative.
        // ============================================================================================
        private const string ClanName = "DRAGONFORGE";
        private const string ClanMotto = "United in Fire, Forged in Glory.";
        private const string MemberCount = "48/50";       // member-count chip + roster header (display-only)
        private const string ClanGold = "125,680";        // clan gold chip (display-only)

        private struct Stat { public string Cap, Val; public Stat(string c, string v) { Cap = c; Val = v; } }
        private static readonly Stat[] Stats =
        {
            new Stat("Clan Level",    "15"),
            new Stat("Clan Type",     "Open"),
            new Stat("Required Level","30"),
            new Stat("Clan Region",   "North America"),
            new Stat("Created",       "2024-02-14"),
        };

        private enum Role { Leader, Officer, Veteran, Member }
        private enum Status { Online, InBattle, Ago, None }
        private struct Member
        {
            public int Rank; public string Name; public int Level; public Role Role; public string Trophies;
            public Status Status; public string StatusText; public bool You;
            public Member(int rank, string name, int lvl, Role role, string tro, Status st, string sText, bool you = false)
            { Rank = rank; Name = name; Level = lvl; Role = role; Trophies = tro; Status = st; StatusText = sText; You = you; }
        }
        // The seven seed rows (Section C/M verbatim — ranks/names/levels/roles/trophies/status exact).
        private static readonly Member[] Members =
        {
            new Member(1,  "Thalion",   60, Role.Leader,  "24,560", Status.Online,   "Online"),
            new Member(2,  "Valyra",    58, Role.Officer, "21,340", Status.Online,   "Online"),
            new Member(3,  "Ragnor",    57, Role.Officer, "19,870", Status.InBattle, "In Battle"),
            new Member(4,  "Eldric",    55, Role.Veteran, "18,450", Status.Online,   "Online"),
            new Member(5,  "Seraphine", 54, Role.Veteran, "16,720", Status.Ago,      "1h ago"),
            new Member(12, "Aric Stormblade", 52, Role.Member, "12,680", Status.None, "", true), // (You) — no status tag
            new Member(13, "Mortis",    51, Role.Member,  "11,240", Status.Ago,      "2h ago"),
        };

        private struct ChatMsg { public string Sender, Body, Ts; public ChatMsg(string s, string b, string t) { Sender = s; Body = b; Ts = t; } }
        // The five seed chat messages (Section C/M verbatim).
        private static readonly ChatMsg[] SeedChat =
        {
            new ChatMsg("Thalion",   "Welcome to Dragonforge! Let's conquer together!", "10:24"),
            new ChatMsg("Valyra",    "Great war tonight! \U0001F525",                    "10:31"),
            new ChatMsg("Eldric",    "Thanks everyone! GG all around.",                  "10:38"),
            new ChatMsg("Ragnor",    "Let's keep the momentum going!",                   "10:45"),
            new ChatMsg("Seraphine", "I donated to the Clan Chest. Let's unlock those rewards!", "10:52"),
        };

        // ---- Animation/state handles (presentation-only) ----
        private RectTransform _leftCol, _centerCol, _rightCol;
        private CanvasGroup _leftCg, _centerCg, _rightCg;
        private CanvasGroup[] _rowCg;       // member-row stagger handles
        private CanvasGroup _youRowCg;      // (You) row blue-glow flash target
        private RectTransform _actionBar; private CanvasGroup _actionCg;
        private RectTransform _tabIndicator; // selected center-tab capsule (glides)
        private float[] _tabCenterX;         // x of each center-tab centre (for the gliding indicator)
        private Text[] _tabLabels; private Image[] _tabFills;
        private int _activeTab;              // 0 MEMBERS · 1 Activity · 2 Clan War · 3 Clan Chest
        private RectTransform[] _tabPanels; private CanvasGroup[] _tabPanelCg;
        private Text[] _chatTabLabels; private Image[] _chatTabFills; private int _activeChatTab; // 0 Chat · 1 Announcements
        private ScrollRect _chatScroll; private RectTransform _chatContent; private float _chatY; private int _chatCount;
        private InputField _composerInput; private Button _sendBtn; private Image _sendImg; private RectTransform _composerRoot;

        // ============================================================================================
        // Build
        // ============================================================================================
        protected override void Build()
        {
            BuildBackground();
            BuildTopBar();
            BuildLeftColumn();
            BuildCenterColumn();
            BuildRightColumn();
        }

        // ---- BackgroundLayer (full-bleed, under cutout) → Rect. ----
        private void BuildBackground()
        {
            UiLayers.Plate(Rect, "clan", 0.30f); // faint dusk castle wall
            var grade = UiWidgets.Stretch("BG_Grade", Rect, Color.white); grade.raycastTarget = false;
            grade.sprite = UiTex.VGradient(Hex("#14161e"), Hex("#0a0b0f"), 64);
            UiWidgets.Vignette(Rect, 0.6f); // BG_Vignette (later sibling)
        }

        // ============================================================================================
        // TopBar — back chevron · "DRAGONFORGE" · member chip · gold chip · English pill · gear (source order).
        // ============================================================================================
        private void BuildTopBar()
        {
            // Back (top-left) — task-mandated signature on SafeContent.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Title_ClanName — serif gold, left-of-centre after the back button (anchorY for TopBar ≈0.952).
            UiWidgets.TitleLabel(SafeContent, ClanName, 52, new Vector2(0.0f, 0.952f), new Vector2(0.0f, 0.952f),
                new Vector2(330, 0), new Vector2(560, 80), TextAnchor.MiddleLeft, TitleGold, UiTheme.Gold);

            // TopRightCluster — chips (display-only) + LanguagePill (Button) + SettingsGear (Button), right-anchored.
            // Laid out manually right→left to mirror the source HorizontalLayoutGroup (spacing ~16).
            float y = 0.952f, gap = 16f, x = -20f;
            // SettingsGear ~72×72 (rightmost of the cluster per source order it is last → sits far right).
            float gearD = 72f;
            var gear = UiWidgets.Rect("SettingsGear", SafeContent, new Vector2(1, y), new Vector2(1, y), new Vector2(x - gearD * 0.5f, 0), new Vector2(gearD, gearD));
            var gImg = gear.gameObject.AddComponent<Image>(); gImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            var gBtn = gear.gameObject.AddComponent<Button>(); gBtn.targetGraphic = gImg;
            var gcb = gBtn.colors; gcb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); gcb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); gcb.fadeDuration = 0.08f; gBtn.colors = gcb;
            gBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("Clan settings — coming soon"); });
            RimImage(gear, 6);
            var gGly = UiWidgets.Rect("Gear_Glyph", gear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(gearD * 0.5f, gearD * 0.5f));
            var gGlyImg = gGly.gameObject.AddComponent<Image>(); gGlyImg.raycastTarget = false; gGlyImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
            x -= gearD + gap;

            // LanguagePill ("English") — Button.
            float langW = 190f;
            var lang = UiWidgets.Rect("LanguagePill", SafeContent, new Vector2(1, y), new Vector2(1, y), new Vector2(x - langW * 0.5f, 0), new Vector2(langW, 64f));
            var lImg = lang.gameObject.AddComponent<Image>(); lImg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.95f), UiTheme.A(UiTheme.Obsidian, 0.95f), 32);
            var lBtn = lang.gameObject.AddComponent<Button>(); lBtn.targetGraphic = lImg;
            var lcb = lBtn.colors; lcb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); lcb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); lcb.fadeDuration = 0.08f; lBtn.colors = lcb;
            lBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("Language / region — coming soon"); });
            RimImage(lang, 5);
            UiWidgets.Label(lang, "English", 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, LangCream);
            x -= langW + gap;

            // ClanGoldChip (Icon_Coin + "125,680") — display-only capsule.
            x = TopChip(x, gap, "ClanGoldChip", UiTex.Disc(TitleGold, 48), ClanGold);
            // MemberCountChip (Icon_People + "48/50") — display-only capsule (leftmost of the cluster).
            x = TopChip(x, gap, "MemberCountChip", UiTex.Disc(Hex("#9fb0c8"), 48), MemberCount);
        }

        // A display-only top-right capsule (icon + value). Returns the next x to the left.
        private float TopChip(float x, float gap, string name, Sprite icon, string value)
        {
            float w = 215f, y = 0.952f;
            var chip = UiWidgets.Rect(name, SafeContent, new Vector2(1, y), new Vector2(1, y), new Vector2(x - w * 0.5f, 0), new Vector2(w, 64f));
            var bg = chip.gameObject.AddComponent<Image>(); bg.raycastTarget = false;
            bg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.9f), UiTheme.A(UiTheme.Obsidian, 0.92f), 32);
            RimImage(chip, 5);
            var ic = UiWidgets.Rect("Icon", chip, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(34, 0), new Vector2(40, 40));
            var icImg = ic.gameObject.AddComponent<Image>(); icImg.raycastTarget = false; icImg.sprite = icon;
            UiWidgets.Label(chip, value, 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(66, 0), new Vector2(w - 76, 36), TextAnchor.MiddleLeft, TitleGold);
            return x - w - gap;
        }

        // ============================================================================================
        // LeftColumn_Identity — banner + clan name + motto + 5 stats.
        // ============================================================================================
        private void BuildLeftColumn()
        {
            _leftCol = ColumnPanel("LeftColumn_Identity", LeftXMin, LeftXMax, out _leftCg);
            float colW = (LeftXMax - LeftXMin) * 2340f; // ≈480

            // ClanBanner — tall royal-blue cloth, top-center (occupies the top ~0.42 of the column).
            float bannerW = colW * 0.62f, bannerH = bannerW / 0.55f; // aspect ~0.55:1 (Section E)
            var bannerPivot = UiWidgets.Rect("BannerPivot", _leftCol, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -26), new Vector2(bannerW, bannerH));
            // Cloth sway pivots at the top edge (gentle 1–2° sine loop, Section J) → pivot the cloth child, anchored to top.
            var banner = UiWidgets.Rect("ClanBanner", bannerPivot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(bannerW, bannerH));
            banner.pivot = new Vector2(0.5f, 1f);
            var bImg = banner.gameObject.AddComponent<Image>(); bImg.raycastTarget = false;
            bImg.sprite = UiTex.VGradient(BannerCore, BannerSh, 64); // cloth shading (core top → shadow bottom)
            // Mid cloth highlight band (the #2b56c8 mid sample) for a folded-cloth read.
            var bMid = UiWidgets.Rect("Banner_Mid", banner, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(bannerW * 0.6f, bannerH * 0.45f));
            var bMidImg = bMid.gameObject.AddComponent<Image>(); bMidImg.raycastTarget = false;
            bMidImg.sprite = UiTex.VGradient(UiTheme.A(BannerMid, 0.55f), UiTheme.A(BannerMid, 0f), 64);
            RimImage(banner, 8); // stitched gold trim
            // Very subtle cloth "sway"/breathe, low-key (Section J/L): reuse the existing PulseScale FX (no new
            // MonoBehaviour type per the kit). Tiny amplitude + slow period keep legibility first.
            var sway = banner.gameObject.AddComponent<PulseScale>(); sway.min = 0.995f; sway.max = 1.006f; sway.period = 3f;
            // Swallow-tail bottom cut (two dark notches at the free edge).
            var notchL = UiWidgets.Rect("TailL", banner, new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0, 14), new Vector2(bannerW * 0.5f, 28));
            var nlImg = notchL.gameObject.AddComponent<Image>(); nlImg.raycastTarget = false; nlImg.color = PanelBot;
            notchL.localRotation = Quaternion.Euler(0, 0, 18);
            var notchR = UiWidgets.Rect("TailR", banner, new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0, 14), new Vector2(bannerW * 0.5f, 28));
            var nrImg = notchR.gameObject.AddComponent<Image>(); nrImg.raycastTarget = false; nrImg.color = PanelBot;
            notchR.localRotation = Quaternion.Euler(0, 0, -18);

            // DragonCrest — gold dragon relief centred on the banner upper-third (~0.30 of banner height from top).
            var crest = UiWidgets.Rect("DragonCrest", banner, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(bannerW * 0.62f, bannerW * 0.62f));
            UiWidgets.Glow(crest, UiTheme.A(CrestHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(bannerW * 0.9f, bannerW * 0.9f), 1.5f); // focal bloom
            // Dragon relief: dark base (#6b4a14 sh) → mid (#b8862c) → bright (#f6dd86 hi) stacked diamonds.
            var crestImg = crest.gameObject.AddComponent<Image>(); crestImg.raycastTarget = false; crestImg.sprite = UiTex.Diamond(CrestSh, 48);
            var crestMidR = UiWidgets.Rect("Crest_Mid", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(bannerW * 0.5f, bannerW * 0.5f));
            var crestMidImg = crestMidR.gameObject.AddComponent<Image>(); crestMidImg.raycastTarget = false; crestMidImg.sprite = UiTex.Diamond(CrestMid, 48);
            var crestHi = UiWidgets.Rect("Crest_Hi", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(bannerW * 0.34f, bannerW * 0.34f));
            var crestHiImg = crestHi.gameObject.AddComponent<Image>(); crestHiImg.raycastTarget = false; crestHiImg.sprite = UiTex.Diamond(CrestHi, 48);
            var gem = UiWidgets.Rect("Crest_Gem", crest, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), Vector2.zero, new Vector2(bannerW * 0.1f, bannerW * 0.1f));
            var gemImg = gem.gameObject.AddComponent<Image>(); gemImg.raycastTarget = false; gemImg.sprite = UiTex.Disc(UiTheme.IronBlueHi, 48); // small blue gem
            crest.gameObject.AddComponent<PulseScale>().period = 2.4f; // gentle specular/bloom breathe

            // Banner_ClanName — heraldic serif overlaid lower on the banner cloth.
            UiWidgets.TitleLabel(banner, ClanName, 32, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero,
                new Vector2(bannerW + 40, 50), TextAnchor.MiddleCenter, BannerName, UiTheme.Gold);

            // Lbl_Motto — italic-leaning cream, one line below the banner. Top-anchored in pixels for determinism:
            // the banner top sits 26 px below the column top and extends bannerH downward.
            float bannerBottomY = -(26f + bannerH); // anchoredPosition.y under a top anchor (0.5,1)
            UiWidgets.Label(_leftCol, ClanMotto, 22, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, bannerBottomY - 24f), new Vector2(colW - 24, 30), TextAnchor.MiddleCenter, MottoCream);

            // StatsList — 5 caption/value rows beneath the motto, with thin gold dividers between rows.
            BuildStatsList(colW, bannerBottomY - 64f);
        }

        // listTopY = anchoredPosition.y (under a top anchor 0.5,1) where the first row's top sits.
        private void BuildStatsList(float colW, float listTopY)
        {
            float rowH = 64f, rowGap = 14f; // px
            int n = Stats.Length;
            for (int i = 0; i < n; i++)
            {
                float yTop = listTopY - i * (rowH + rowGap);
                float yc = yTop - rowH * 0.5f;
                // Caption (left grey) / value (right gold) — top-anchored row, fixed pixel height.
                var row = UiWidgets.Rect("Stat_" + Stats[i].Cap.Replace(" ", ""), _leftCol, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, yc), new Vector2(colW * 0.86f, rowH));
                UiWidgets.Label(row, Stats[i].Cap, 22, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 8), new Vector2(colW * 0.55f, 28), TextAnchor.MiddleLeft, StatCap);
                UiWidgets.Label(row, Stats[i].Val, 24, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, 8), new Vector2(colW * 0.7f, 28), TextAnchor.MiddleRight, StatVal);
                if (i < n - 1)
                    UiWidgets.Divider(_leftCol, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, yTop - rowH - rowGap * 0.5f), colW * 0.82f, 2f, UiTheme.A(UiTheme.GoldShadow, 0.7f));
                // Clan Level row carries a small progress-bar fragment beneath it (Section C/E — partially-occluded fragment).
                if (i == 0)
                    UiWidgets.ProgressBar(_leftCol, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, yc - rowH * 0.42f), new Vector2(colW * 0.8f, 12f), UiTheme.GoldFill, 0.62f);
            }
        }

        // ============================================================================================
        // CenterColumn_Roster — tabs · roster header · scroll (4 tab panels) · action bar.
        // ============================================================================================
        private void BuildCenterColumn()
        {
            _centerCol = ColumnPanel("CenterColumn_Roster", CtrXMin, CtrXMax, out _centerCg);
            float colW = (CtrXMax - CtrXMin) * 2340f; // ≈1112

            // TabRow — 4 tabs equal width across the column, top band ≈64 px. MEMBERS selected.
            string[] tabs = { "MEMBERS", "Activity", "Clan War", "Clan Chest" };
            _tabLabels = new Text[tabs.Length]; _tabFills = new Image[tabs.Length]; _tabCenterX = new float[tabs.Length];
            float tabH = 64f, tabGap = 8f;
            var tabRow = UiWidgets.Rect("TabRow", _centerCol, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, tabH));
            tabRow.offsetMin = new Vector2(10, tabRow.offsetMin.y); tabRow.offsetMax = new Vector2(-10, tabRow.offsetMax.y);
            float innerW = colW - 20f; float tw = (innerW - tabGap * (tabs.Length - 1)) / tabs.Length;
            // Selected-tab indicator capsule (glides under the active tab) — built first so labels sit above.
            _tabIndicator = UiWidgets.Rect("TabIndicator", tabRow, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(tw, tabH));
            var indImg = _tabIndicator.gameObject.AddComponent<Image>(); indImg.raycastTarget = false;
            indImg.sprite = UiTex.VGradient(UiWidgets.Lighten(UiTheme.IronBlue, 0.28f), UiWidgets.Darken(UiTheme.IronBlue, 0.3f), 32);
            var indGlow = _tabIndicator.gameObject.AddComponent<PulseGraphic>(); indGlow.target = indImg; indGlow.min = 0.82f; indGlow.max = 1f; indGlow.period = 1.8f; // subtle rim glow (Section J)
            for (int i = 0; i < tabs.Length; i++)
            {
                int idx = i;
                float cx = tw * (i + 0.5f) + tabGap * i;
                _tabCenterX[i] = cx;
                var tab = UiWidgets.Rect("Tab_" + tabs[i].Replace(" ", ""), tabRow, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(cx, 0), new Vector2(tw, tabH));
                var tImg = tab.gameObject.AddComponent<Image>(); tImg.color = new Color(0, 0, 0, 0); // ghost (indicator carries selected fill)
                _tabFills[i] = tImg;
                var tBtn = tab.gameObject.AddComponent<Button>(); tBtn.targetGraphic = tImg;
                var cb = tBtn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; tBtn.colors = cb;
                tBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); SelectTab(idx); });
                _tabLabels[i] = UiWidgets.Label(tab, tabs[i], 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, i == 0 ? TabSel : StatCap);
            }
            _tabIndicator.anchoredPosition = new Vector2(_tabCenterX[0], 0); _tabIndicator.SetAsFirstSibling();

            // The four center sub-views (stacked; only the active one is visible/interactable). Region below tabs,
            // above the action bar: anchorY ~0.10 (top of action bar) to ~ (1 − tab band).
            float bodyTop = 1f - (tabH + 16f) / (((ContentTop - ContentBot)) * 1080f);
            float bodyBot = 0.10f; // action bar sits below
            _tabPanels = new RectTransform[4]; _tabPanelCg = new CanvasGroup[4];
            for (int i = 0; i < 4; i++)
            {
                var goPanel = new GameObject("TabPanel_" + i, typeof(RectTransform), typeof(CanvasGroup));
                var p = (RectTransform)goPanel.transform; p.SetParent(_centerCol, false);
                p.anchorMin = new Vector2(0.02f, bodyBot); p.anchorMax = new Vector2(0.98f, bodyTop);
                p.offsetMin = Vector2.zero; p.offsetMax = Vector2.zero;
                _tabPanels[i] = p; _tabPanelCg[i] = goPanel.GetComponent<CanvasGroup>();
            }
            BuildRosterPanel(_tabPanels[0], colW);     // MEMBERS (default)
            BuildActivityPanel(_tabPanels[1]);          // Activity feed
            BuildClanWarPanel(_tabPanels[2]);           // Clan War status
            BuildClanChestPanel(_tabPanels[3]);         // Clan Chest progress/contribute
            for (int i = 1; i < 4; i++) { _tabPanelCg[i].alpha = 0f; _tabPanelCg[i].blocksRaycasts = false; }

            // ActionBar — bottom band ≈92 px: 4 gold icon+label buttons + LEAVE CLAN (red, right-weighted).
            BuildActionBar(colW);
        }

        // ---- MEMBERS panel: roster header + scrolling member rows ----
        private void BuildRosterPanel(RectTransform panel, float colW)
        {
            // RosterHeaderRow ≈42 px (top): "MEMBERS 48/50" left · ROLE/TROPHIES column labels right.
            var header = UiWidgets.Rect("RosterHeaderRow", panel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -4), new Vector2(0, 42f));
            UiWidgets.Label(header, "MEMBERS " + MemberCount, 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(420, 34), TextAnchor.MiddleLeft, TitleGold);
            // Column labels aligned to the row's role/trophy x-fractions (ROLE center ≈0.62, TROPHIES center ≈0.83).
            UiWidgets.Label(header, "ROLE", 22, new Vector2(0.62f, 0.5f), new Vector2(0.62f, 0.5f), Vector2.zero, new Vector2(200, 30), TextAnchor.MiddleCenter, StatCap);
            UiWidgets.Label(header, "TROPHIES", 22, new Vector2(0.83f, 0.5f), new Vector2(0.83f, 0.5f), Vector2.zero, new Vector2(220, 30), TextAnchor.MiddleRight, StatCap);

            // ScrollRect over the rest of the panel (below the header).
            var host = UiWidgets.Rect("ScrollView", panel, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            host.offsetMax = new Vector2(0, -50f); // below header
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Elastic; scroll.elasticity = 0.1f; scroll.scrollSensitivity = 32f;
            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0); vImg.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            float rowH = 80f, gap = 8f, pitch = rowH + gap; // Section E: row ≈80, gap ~8
            int n = Members.Length; float totalH = n * pitch;
            var content = UiWidgets.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, totalH));
            content.pivot = new Vector2(0.5f, 1f);
            scroll.viewport = viewport; scroll.content = content;

            _rowCg = new CanvasGroup[n];
            for (int i = 0; i < n; i++) BuildMemberRow(content, Members[i], i, rowH, pitch);
        }

        private void BuildMemberRow(RectTransform content, Member m, int i, float rowH, float pitch)
        {
            var rowGo = new GameObject("MemberRow_" + (m.You ? "You" : (i + 1).ToString()), typeof(RectTransform), typeof(CanvasGroup));
            var row = (RectTransform)rowGo.transform; row.SetParent(content, false);
            row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1f);
            row.offsetMin = new Vector2(0, 0); row.offsetMax = new Vector2(0, 0);
            row.sizeDelta = new Vector2(0, rowH); row.anchoredPosition = new Vector2(0, -i * pitch);
            _rowCg[i] = rowGo.GetComponent<CanvasGroup>();

            // Row background: subtle stripe; the (You) row is blue-tinted with a 2px blue outline (sustained).
            var bg = UiWidgets.Rect("Row_BG", row, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-6, -4));
            var bgImg = bg.gameObject.AddComponent<Image>();
            if (m.You)
            {
                bgImg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.IronBlue, 0.34f), UiTheme.A(UiTheme.IronBlue, 0.18f), 32);
                var outline = UiWidgets.Rect("You_Outline", row, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-6, -4));
                var olImg = outline.gameObject.AddComponent<Image>(); olImg.raycastTarget = false;
                olImg.sprite = UiTex.Frame(UiTheme.IronBlueHi, UiTheme.IronBlue, UiTheme.A(UiTheme.IronBlue, 0.8f), 48, 4); olImg.type = Image.Type.Sliced;
                outline.gameObject.AddComponent<PulseGraphic>().target = olImg; // breathing blue outline glow (Section J)
                _youRowCg = _rowCg[i];
            }
            else bgImg.color = UiTheme.A((i % 2 == 0) ? Hex("#161821") : Hex("#10121a"), 0.85f);
            // Tap → member context (display-only; promote/kick is role-gated + server-validated, L/K §12).
            var btn = row.gameObject.AddComponent<Button>(); btn.targetGraphic = bgImg;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.92f, 0.92f, 0.95f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            string who = m.Name; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast(who + " — member actions (server-validated)"); });

            // Row x-fractions (Section E, relative to center column width): RANK 0.045 · Avatar 0.135 · Name 0.18 ·
            // RolePill 0.62 · Trophy 0.83 (right) · Status 0.95 (right).
            // RankBadge — ranks 1-3 medallion (gold/silver/bronze) + numeral; 4+ plain numeral.
            var rank = UiWidgets.Rect("RankBadge", row, new Vector2(0.045f, 0.5f), new Vector2(0.045f, 0.5f), Vector2.zero, new Vector2(54, 54));
            if (m.Rank <= 3)
            {
                Color medal = m.Rank == 1 ? Hex("#f0d27a") : m.Rank == 2 ? Hex("#cfd3da") : Hex("#c08a4a");
                var medImg = rank.gameObject.AddComponent<Image>(); medImg.raycastTarget = false; medImg.sprite = UiTex.Disc(medal, 48);
                RimImage(rank, 5);
                UiWidgets.Label(rank, m.Rank.ToString(), 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#2a2008"));
            }
            else UiWidgets.Label(rank, m.Rank.ToString(), 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, StatCap);

            // Avatar — semi-realistic portrait in a beveled gold ring (placeholder disc + ring).
            var av = UiWidgets.Rect("Avatar", row, new Vector2(0.135f, 0.5f), new Vector2(0.135f, 0.5f), Vector2.zero, new Vector2(52, 52));
            var avImg = av.gameObject.AddComponent<Image>(); avImg.raycastTarget = false; avImg.sprite = UiTex.Disc(Hex("#3a3340"), 48);
            var avGly = UiWidgets.Rect("Av_Glyph", av, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26, 26));
            var avGlyImg = avGly.gameObject.AddComponent<Image>(); avGlyImg.raycastTarget = false; avGlyImg.sprite = UiTex.Disc(Hex("#6a6270"), 48);
            RimImage(av, 5);

            // Name + LevelChip (chip immediately right of the name). (You) gets a blue "(You)" suffix.
            UiWidgets.Label(row, m.Name, 28, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), new Vector2(0, 0), new Vector2(420, 36), TextAnchor.MiddleLeft, NameCream);
            // Approx pixel advance to place the level chip just past the name (legacy Text has no measured width here).
            float colW = (CtrXMax - CtrXMin) * 2340f;
            float nameStartX = 0.18f * colW;
            float nameW = Mathf.Min(m.Name.Length * 15f + 16f, 360f);
            if (m.You)
            {
                UiWidgets.Label(row, "(You)", 24, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(nameStartX + nameW, 0), new Vector2(120, 32), TextAnchor.MiddleLeft, YouBlue);
                nameW += 110f;
            }
            var chip = UiWidgets.Rect("LevelChip", row, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(nameStartX + nameW + 24, 0), new Vector2(70, 38));
            var chipImg = chip.gameObject.AddComponent<Image>(); chipImg.raycastTarget = false; chipImg.sprite = UiTex.VGradient(Hex("#22242c"), Hex("#14161c"), 32);
            RimImage(chip, 4);
            UiWidgets.Label(chip, m.Level.ToString(), 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, StatVal);

            // RolePill — capsule tinted by role + role text.
            BuildRolePill(row, m.Role);

            // TrophyValue — gold cup icon + tabular number, right-aligned at ≈0.83.
            var trophy = UiWidgets.Rect("TrophyValue", row, new Vector2(0.83f, 0.5f), new Vector2(0.83f, 0.5f), Vector2.zero, new Vector2(180, 40));
            var cup = UiWidgets.Rect("Trophy_Icon", trophy, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(30, 30));
            var cupImg = cup.gameObject.AddComponent<Image>(); cupImg.raycastTarget = false; cupImg.sprite = UiTex.Disc(TitleGold, 48);
            UiWidgets.Label(trophy, m.Trophies, 26, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-6, 0), new Vector2(130, 34), TextAnchor.MiddleRight, TitleGold);

            // StatusTag — dot + label (omit for the (You) row per source).
            if (m.Status != Status.None)
            {
                Color dotCol = m.Status == Status.Online ? OnlineGreen : m.Status == Status.InBattle ? InBattleAmb : AgoGrey;
                Color txtCol = dotCol;
                var status = UiWidgets.Rect("StatusTag", row, new Vector2(0.95f, 0.5f), new Vector2(0.95f, 0.5f), Vector2.zero, new Vector2(150, 30));
                var dot = UiWidgets.Rect("Dot", status, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(16, 16));
                var dotImg = dot.gameObject.AddComponent<Image>(); dotImg.raycastTarget = false; dotImg.sprite = UiTex.Disc(dotCol, 32);
                if (m.Status == Status.Online) // faint pulsing green glow (±10%, Section J)
                { var pg = dot.gameObject.AddComponent<PulseGraphic>(); pg.target = dotImg; pg.min = 0.7f; pg.max = 1f; pg.period = 1.8f; }
                UiWidgets.Label(status, m.StatusText, 22, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(140, 28), TextAnchor.MiddleLeft, txtCol);
            }
        }

        private void BuildRolePill(RectTransform row, Role role)
        {
            string txt; Color chipCol, txtCol;
            switch (role)
            {
                case Role.Leader:  txt = "Leader";  chipCol = RoleLeaderChip;  txtCol = RoleLeaderTxt;  break;
                case Role.Officer: txt = "Officer"; chipCol = RoleOfficerChip; txtCol = RoleOfficerTxt; break;
                case Role.Veteran: txt = "Veteran"; chipCol = RoleVetChip;     txtCol = RoleVetTxt;     break;
                default:           txt = "Member";  chipCol = RoleMemberChip;  txtCol = RoleMemberTxt;  break;
            }
            var pill = UiWidgets.Rect("RolePill", row, new Vector2(0.62f, 0.5f), new Vector2(0.62f, 0.5f), Vector2.zero, new Vector2(150, 44));
            var pImg = pill.gameObject.AddComponent<Image>(); pImg.raycastTarget = false;
            pImg.sprite = UiTex.VGradient(UiWidgets.Lighten(chipCol, 0.22f), UiWidgets.Darken(chipCol, 0.25f), 32);
            RimImage(pill, 4);
            UiWidgets.Label(pill, txt, 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, txtCol);
        }

        // ---- Activity panel (alternate center sub-view) ----
        private void BuildActivityPanel(RectTransform panel)
        {
            string[] feed =
            {
                "Thalion promoted Valyra to Officer.",
                "Eldric won a Clan War battle (+18 stars).",
                "Seraphine contributed to the Clan Chest.",
                "Mortis joined the clan.",
                "Ragnor donated 12 units to a request.",
            };
            var host = ScrollHost(panel, out var content, feed.Length * 84f);
            for (int i = 0; i < feed.Length; i++)
            {
                var card = UiWidgets.Card(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -i * 84f - 6), new Vector2(-8, 72f));
                card.pivot = new Vector2(0.5f, 1f); card.anchoredPosition = new Vector2(0, -i * 84f - 6);
                var dot = UiWidgets.Rect("ActDot", card, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(26, 0), new Vector2(16, 16));
                var dImg = dot.gameObject.AddComponent<Image>(); dImg.raycastTarget = false; dImg.sprite = UiTex.Disc(UiTheme.IronBlueHi, 32);
                UiWidgets.Label(card, feed[i], 24, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(30, 0), new Vector2(-60, 36), TextAnchor.MiddleLeft, ChatBody);
            }
        }

        // ---- Clan War panel (alternate center sub-view) ----
        private void BuildClanWarPanel(RectTransform panel)
        {
            UiWidgets.TitleLabel(panel, "CLAN WAR", 40, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(500, 56), TextAnchor.MiddleCenter, TitleGold, UiTheme.Gold);
            UiWidgets.Label(panel, "Battle Day — 06h 12m remaining", 24, new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.83f), Vector2.zero, new Vector2(600, 32), TextAnchor.MiddleCenter, MottoCream);
            // Two faction war cards (DRAGONFORGE vs rival) with star scores (display-only).
            WarCard(panel, "DRAGONFORGE", "32", true, 0.30f);
            WarCard(panel, "IRONCLAD",    "28", false, 0.70f);
            UiWidgets.Label(panel, "VS", 36, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(120, 50), TextAnchor.MiddleCenter, TitleGold);
            UiWidgets.GemButton(panel, "ENTER WAR", new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(420, 92), UiTheme.IronBlue, () => Router.Toast("Clan War — coming soon"), 36, false, false, TextAnchor.MiddleCenter);
        }

        private void WarCard(RectTransform panel, string name, string stars, bool ours, float xc)
        {
            var card = UiWidgets.Card(panel, new Vector2(xc, 0.50f), new Vector2(xc, 0.50f), Vector2.zero, new Vector2(300, 300), UiTheme.A(ours ? UiTheme.IronBanner : UiTheme.AshBanner, 0.5f));
            UiWidgets.TitleLabel(card, name, 28, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(280, 40), TextAnchor.MiddleCenter, BannerName, UiTheme.Gold);
            var cup = UiWidgets.Rect("WarCup", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90, 90));
            var cupImg = cup.gameObject.AddComponent<Image>(); cupImg.raycastTarget = false; cupImg.sprite = UiTex.Disc(TitleGold, 48);
            UiWidgets.Label(card, stars + " ★", 34, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(260, 44), TextAnchor.MiddleCenter, TitleGold);
        }

        // ---- Clan Chest panel (alternate center sub-view) ----
        private void BuildClanChestPanel(RectTransform panel)
        {
            UiWidgets.TitleLabel(panel, "CLAN CHEST", 40, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(560, 56), TextAnchor.MiddleCenter, TitleGold, UiTheme.Gold);
            var chest = UiWidgets.Rect("ChestArt", panel, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(260, 200));
            UiWidgets.Glow(chest, UiTheme.A(UiTheme.GoldHi, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 320), 1.5f);
            var chImg = chest.gameObject.AddComponent<Image>(); chImg.raycastTarget = false; chImg.sprite = UiTex.VGradient(Hex("#a05b25"), Hex("#7a4a20"), 64);
            var band = UiWidgets.Rect("Chest_Band", chest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 28));
            var bandImg = band.gameObject.AddComponent<Image>(); bandImg.raycastTarget = false; bandImg.color = UiTheme.GoldHi;
            // Tier progress (display-only) + "Tier 6 / 10".
            UiWidgets.ProgressBar(panel, new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), Vector2.zero, new Vector2(640, 30f), UiTheme.GoldFillHi, 0.6f);
            UiWidgets.Label(panel, "Tier 6 / 10 — 9,240 / 15,000", 24, new Vector2(0.5f, 0.27f), new Vector2(0.5f, 0.27f), Vector2.zero, new Vector2(600, 32), TextAnchor.MiddleCenter, StatVal);
            // CONTRIBUTE — display-only (never client-mint, L/K §12).
            UiWidgets.GemButton(panel, "CONTRIBUTE", new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(420, 92), UiTheme.IronBlue, () => Router.Toast("Contribute — server-validated (display-only)"), 36, false, false, TextAnchor.MiddleCenter);
        }

        // ---- ActionBar: CLAN WAR · DONATE · CLAN SHOP · MANAGE (gold) + LEAVE CLAN (red) ----
        private void BuildActionBar(float colW)
        {
            float barH = 92f;
            var goBar = new GameObject("ActionBar", typeof(RectTransform), typeof(CanvasGroup));
            _actionBar = (RectTransform)goBar.transform; _actionBar.SetParent(_centerCol, false);
            _actionBar.anchorMin = new Vector2(0.02f, 0f); _actionBar.anchorMax = new Vector2(0.98f, 0f);
            _actionBar.pivot = new Vector2(0.5f, 0f); _actionBar.anchoredPosition = new Vector2(0, 8); _actionBar.sizeDelta = new Vector2(0, barH);
            _actionCg = goBar.GetComponent<CanvasGroup>();

            float innerW = colW * 0.96f - 16f;
            float gap = 12f;
            float wMain = innerW * 0.155f;  // each of the 4 gold buttons (≈0.155 of column width, Section E)
            float wLeave = innerW * 0.22f;  // LEAVE CLAN wider, right-weighted
            float x = 0f;
            ActionButton("CLAN WAR",  x + wMain * 0.5f, wMain, barH, ActionGold, false, () => Router.Toast("Clan War — coming soon")); x += wMain + gap;
            ActionButton("DONATE",    x + wMain * 0.5f, wMain, barH, ActionGold, false, () => Router.Toast("Donate / requests — server-validated (display-only)")); x += wMain + gap;
            ActionButton("CLAN SHOP", x + wMain * 0.5f, wMain, barH, ActionGold, false, () => Router.Toast("Clan Shop — coming soon")); x += wMain + gap;
            ActionButton("MANAGE",    x + wMain * 0.5f, wMain, barH, ActionGold, false, () => Router.Toast("Manage — officer+ only (server-validated)")); x += wMain + gap;
            // LEAVE CLAN — right-aligned (red danger), always triggers a Confirm before acting (Section L).
            ActionButton("LEAVE CLAN", innerW - wLeave * 0.5f, wLeave, barH, DangerWhite, true, ConfirmLeave);
        }

        private void ActionButton(string label, float cx, float w, float h, Color labelCol, bool danger, UnityEngine.Events.UnityAction onClick)
        {
            // Anchored to the left edge of the bar's inner span; cx is the centre offset from that edge.
            var rt = UiWidgets.Rect("Btn_" + label.Replace(" ", ""), _actionBar, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8 + cx, 0), new Vector2(w, h - 8));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = danger ? UiTex.VGradient(LeaveTop, LeaveBot, 32)              // oxblood→ember danger gradient
                                : UiTex.VGradient(Hex("#2a2c34"), Hex("#161820"), 32); // dark stone/metal capsule
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            RimImage(rt, danger ? 7 : 6);
            // Icon over/left of the label.
            var icon = UiWidgets.Rect("Icon", rt, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(34, 34));
            var icImg = icon.gameObject.AddComponent<Image>(); icImg.raycastTarget = false; icImg.sprite = UiTex.Diamond(danger ? DangerWhite : ActionGold, 32);
            var lbl = UiWidgets.Label(rt, label, 24, new Vector2(0, 0.0f), new Vector2(1, 0.42f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, labelCol);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            if (danger) { var pg = rt.gameObject.AddComponent<PulseGraphic>(); pg.target = img; pg.min = 0.9f; pg.max = 1f; pg.period = 2.2f; } // faint danger glow (kept low)
        }

        // ============================================================================================
        // RightColumn_Chat — chat tabs · chat scroll · composer.
        // ============================================================================================
        private void BuildRightColumn()
        {
            _rightCol = ColumnPanel("RightColumn_Chat", RgtXMin, RgtXMax, out _rightCg);

            // ChatTabRow — two tabs (Chat selected, Announcements), top band ≈56 px.
            string[] tabs = { "Chat", "Announcements" };
            _chatTabLabels = new Text[2]; _chatTabFills = new Image[2];
            float tabH = 56f;
            var tabRow = UiWidgets.Rect("ChatTabRow", _rightCol, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, tabH));
            tabRow.offsetMin = new Vector2(10, tabRow.offsetMin.y); tabRow.offsetMax = new Vector2(-10, tabRow.offsetMax.y);
            for (int i = 0; i < 2; i++)
            {
                int idx = i;
                var tab = UiWidgets.Rect("ChatTab_" + tabs[i], tabRow, new Vector2(i * 0.5f, 0), new Vector2((i + 1) * 0.5f, 1), Vector2.zero, Vector2.zero);
                tab.offsetMin = new Vector2(i == 0 ? 0 : 4, 0); tab.offsetMax = new Vector2(i == 0 ? -4 : 0, 0);
                var tImg = tab.gameObject.AddComponent<Image>();
                tImg.sprite = i == 0 ? UiTex.VGradient(UiWidgets.Lighten(UiTheme.IronBlue, 0.28f), UiWidgets.Darken(UiTheme.IronBlue, 0.3f), 32)
                                     : UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.9f), UiTheme.A(UiTheme.Obsidian, 0.9f), 32);
                _chatTabFills[i] = tImg;
                var tBtn = tab.gameObject.AddComponent<Button>(); tBtn.targetGraphic = tImg;
                var cb = tBtn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; tBtn.colors = cb;
                tBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); SelectChatTab(idx); });
                RimImage(tab, 4);
                _chatTabLabels[i] = UiWidgets.Label(tab, tabs[i], 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, i == 0 ? TabSel : StatCap);
            }

            // ChatScrollView — vertical; content holds the seed bubbles; new messages append + auto-scroll.
            var host = UiWidgets.Rect("ChatScrollView", _rightCol, new Vector2(0.02f, 0), new Vector2(0.98f, 1), Vector2.zero, Vector2.zero);
            host.offsetMin = new Vector2(host.offsetMin.x, 88f); // above composer (≈80 + pad)
            host.offsetMax = new Vector2(host.offsetMax.x, -(tabH + 16f)); // below tabs
            _chatScroll = host.gameObject.AddComponent<ScrollRect>();
            _chatScroll.horizontal = false; _chatScroll.vertical = true; _chatScroll.movementType = ScrollRect.MovementType.Elastic; _chatScroll.elasticity = 0.1f; _chatScroll.scrollSensitivity = 32f;
            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0); vImg.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            _chatContent = UiWidgets.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 0));
            _chatContent.pivot = new Vector2(0.5f, 1f);
            _chatScroll.viewport = viewport; _chatScroll.content = _chatContent;

            _chatY = 0f; _chatCount = 0;
            for (int i = 0; i < SeedChat.Length; i++) AppendChat(SeedChat[i].Sender, SeedChat[i].Body, SeedChat[i].Ts, false);

            // ChatComposer — Input (flexible) + Emoji (square) + Send (capsule, disabled when empty).
            BuildComposer(tabH);
        }

        private void BuildComposer(float tabH)
        {
            float h = 80f;
            var goC = new GameObject("ChatComposer", typeof(RectTransform));
            _composerRoot = (RectTransform)goC.transform; _composerRoot.SetParent(_rightCol, false);
            _composerRoot.anchorMin = new Vector2(0.02f, 0f); _composerRoot.anchorMax = new Vector2(0.98f, 0f);
            _composerRoot.pivot = new Vector2(0.5f, 0f); _composerRoot.anchoredPosition = new Vector2(0, 6); _composerRoot.sizeDelta = new Vector2(0, h);

            float emojiW = 64f, sendW = 120f, gap = 10f;
            // Input_Message — dark recessed pill, flexible width (fills minus emoji + send).
            var inputRt = UiWidgets.Rect("Input_Message", _composerRoot, new Vector2(0, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(0, h - 12));
            inputRt.offsetMin = new Vector2(0, inputRt.offsetMin.y); inputRt.offsetMax = new Vector2(-(emojiW + sendW + gap * 2f), inputRt.offsetMax.y);
            var inImg = inputRt.gameObject.AddComponent<Image>(); inImg.sprite = UiTex.VGradient(Hex("#0c0d12"), Hex("#08090d"), 32);
            RimImage(inputRt, 4);
            _composerInput = inputRt.gameObject.AddComponent<InputField>();
            var textRt = UiWidgets.Rect("Text", inputRt, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-30, -10));
            var textCmp = textRt.gameObject.AddComponent<Text>(); textCmp.font = UiWidgets.Font; textCmp.fontSize = 24; textCmp.color = ChatBody; textCmp.supportRichText = false;
            textCmp.alignment = TextAnchor.MiddleLeft; textCmp.horizontalOverflow = HorizontalWrapMode.Overflow; textCmp.verticalOverflow = VerticalWrapMode.Truncate;
            var phRt = UiWidgets.Rect("Placeholder", inputRt, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-30, -10));
            var phCmp = phRt.gameObject.AddComponent<Text>(); phCmp.font = UiWidgets.Font; phCmp.fontSize = 24; phCmp.color = Placeholder; phCmp.fontStyle = FontStyle.Italic;
            phCmp.text = "Tap to message..."; phCmp.alignment = TextAnchor.MiddleLeft; phCmp.raycastTarget = false;
            _composerInput.textComponent = textCmp; _composerInput.placeholder = phCmp; _composerInput.targetGraphic = inImg;
            _composerInput.lineType = InputField.LineType.SingleLine; _composerInput.characterLimit = 200;
            _composerInput.onValueChanged.AddListener(_ => RefreshSendState());
            _composerInput.onEndEdit.AddListener(s => { if (!string.IsNullOrEmpty(s) && Input.GetKeyDown(KeyCode.Return)) OnSend(); });

            // Btn_Emoji — square, opens picker (display-only).
            var emoji = UiWidgets.Rect("Btn_Emoji", _composerRoot, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-(sendW + gap) - emojiW * 0.5f, 0), new Vector2(emojiW, h - 12));
            var eImg = emoji.gameObject.AddComponent<Image>(); eImg.sprite = UiTex.VGradient(Hex("#22242c"), Hex("#14161c"), 32);
            var eBtn = emoji.gameObject.AddComponent<Button>(); eBtn.targetGraphic = eImg;
            var ecb = eBtn.colors; ecb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); ecb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); ecb.fadeDuration = 0.08f; eBtn.colors = ecb;
            eBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnEmoji(); });
            RimImage(emoji, 4);
            UiWidgets.Label(emoji, "\U0001F60A", 30, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, TitleGold);

            // Btn_Send — blue capsule "SEND"; disabled (desaturated) when input empty (Section H/L).
            var send = UiWidgets.Rect("Btn_Send", _composerRoot, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-sendW * 0.5f, 0), new Vector2(sendW, h - 12));
            _sendImg = send.gameObject.AddComponent<Image>(); _sendImg.sprite = UiTex.VGradient(UiWidgets.Lighten(UiTheme.IronBlue, 0.28f), UiWidgets.Darken(UiTheme.IronBlue, 0.3f), 32);
            _sendBtn = send.gameObject.AddComponent<Button>(); _sendBtn.targetGraphic = _sendImg;
            var scb = _sendBtn.colors; scb.normalColor = Color.white; scb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); scb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); scb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.55f); scb.fadeDuration = 0.08f; _sendBtn.colors = scb;
            _sendBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnSend(); });
            RimImage(send, 5);
            UiWidgets.Label(send, "SEND", 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, DangerWhite);
            var sendPulse = send.gameObject.AddComponent<PulseGraphic>(); sendPulse.target = _sendImg; sendPulse.min = 0.86f; sendPulse.max = 1f; sendPulse.period = 1.8f; // subtle send rim glow
            RefreshSendState();
        }

        // Append a chat bubble: header (avatar + sender gold + timestamp grey-right) over wrapping body.
        private void AppendChat(string sender, string body, string ts, bool animate)
        {
            float padV = 10f, headerH = 28f, avatar = 36f;
            // Estimate body height from wrap width (legacy Text has no pre-measure; conservative line estimate).
            float bubbleW = (RgtXMax - RgtXMin) * 2340f * 0.94f - 20f;
            int approxCharsPerLine = Mathf.Max(10, Mathf.FloorToInt((bubbleW - 16f) / 12f));
            int lines = Mathf.Max(1, Mathf.CeilToInt(body.Length / (float)approxCharsPerLine));
            float bodyH = lines * 30f;
            float bubbleH = padV * 2f + headerH + 6f + bodyH;

            var goB = new GameObject("ChatMsg_" + _chatCount, typeof(RectTransform), typeof(CanvasGroup));
            var b = (RectTransform)goB.transform; b.SetParent(_chatContent, false);
            b.anchorMin = new Vector2(0, 1); b.anchorMax = new Vector2(1, 1); b.pivot = new Vector2(0.5f, 1f);
            b.offsetMin = new Vector2(0, 0); b.offsetMax = new Vector2(0, 0);
            b.sizeDelta = new Vector2(0, bubbleH); b.anchoredPosition = new Vector2(0, -_chatY);
            var cg = goB.GetComponent<CanvasGroup>();

            // Bubble fill: nearly-transparent dark with a faint left gold accent on the sender row.
            var fill = UiWidgets.Rect("Bubble", b, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8, -4));
            var fImg = fill.gameObject.AddComponent<Image>(); fImg.raycastTarget = false; fImg.color = UiTheme.A(Hex("#0e0f16"), 0.7f);
            var accent = UiWidgets.Rect("Accent", fill, new Vector2(0, 0), new Vector2(0, 1), new Vector2(3, 0), new Vector2(4, 0));
            var aImg = accent.gameObject.AddComponent<Image>(); aImg.raycastTarget = false; aImg.color = UiTheme.A(UiTheme.Gold, 0.6f);

            // Header: avatar + sender (gold) + timestamp (grey, right).
            var av = UiWidgets.Rect("MsgAvatar", fill, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8 + avatar * 0.5f, -(padV + avatar * 0.5f)), new Vector2(avatar, avatar));
            var avImg = av.gameObject.AddComponent<Image>(); avImg.raycastTarget = false; avImg.sprite = UiTex.Disc(Hex("#3a3340"), 48); RimImage(av, 4);
            UiWidgets.Label(fill, sender, 24, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8 + avatar + 12, -(padV + headerH * 0.5f)), new Vector2(300, headerH), TextAnchor.MiddleLeft, SenderGold);
            UiWidgets.Label(fill, ts, 20, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-14, -(padV + headerH * 0.5f)), new Vector2(120, headerH), TextAnchor.MiddleRight, ChatTime);

            // Body (wrapping cream).
            var bodyLbl = UiWidgets.Label(fill, body, 24, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, padV + bodyH * 0.5f), new Vector2(-32, bodyH), TextAnchor.UpperLeft, ChatBody);
            bodyLbl.horizontalOverflow = HorizontalWrapMode.Wrap; bodyLbl.verticalOverflow = VerticalWrapMode.Overflow;
            var brt = (RectTransform)bodyLbl.transform; brt.offsetMin = new Vector2(16, padV); brt.offsetMax = new Vector2(-16, padV + bodyH);

            _chatY += bubbleH + 12f; // spacing ~12 (Section E)
            _chatContent.sizeDelta = new Vector2(0, _chatY);
            _chatCount++;
            if (animate) StartCoroutine(BubbleArrive(cg, b));
        }

        // ============================================================================================
        // Tab switching (center) — cross-fade + gliding indicator + re-stagger rows.
        // ============================================================================================
        private void SelectTab(int idx)
        {
            if (idx == _activeTab) return;
            int old = _activeTab; _activeTab = idx;
            for (int i = 0; i < _tabLabels.Length; i++) if (_tabLabels[i] != null) _tabLabels[i].color = (i == idx) ? TabSel : StatCap;
            StartCoroutine(GlideIndicator(_tabCenterX[idx]));
            StartCoroutine(CrossfadePanels(old, idx));
            if (idx == 0) StartCoroutine(StaggerRows(0.015f)); // re-stagger roster (Section I)
        }

        private void SelectChatTab(int idx)
        {
            if (idx == _activeChatTab) return;
            _activeChatTab = idx;
            for (int i = 0; i < _chatTabLabels.Length; i++)
            {
                if (_chatTabLabels[i] != null) _chatTabLabels[i].color = (i == idx) ? TabSel : StatCap;
                if (_chatTabFills[i] != null)
                    _chatTabFills[i].sprite = (i == idx) ? UiTex.VGradient(UiWidgets.Lighten(UiTheme.IronBlue, 0.28f), UiWidgets.Darken(UiTheme.IronBlue, 0.3f), 32)
                                                         : UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.9f), UiTheme.A(UiTheme.Obsidian, 0.9f), 32);
            }
            // Announcements is read-only for non-officers → hide the composer (Section H/L).
            bool readOnly = idx == 1;
            if (_composerRoot != null) _composerRoot.gameObject.SetActive(!readOnly);
            // Adjust the chat scroll bottom inset to reclaim the composer space when hidden.
            var host = _chatScroll != null ? (RectTransform)_chatScroll.transform : null;
            if (host != null) host.offsetMin = new Vector2(host.offsetMin.x, readOnly ? 8f : 88f);
            Router.Toast(readOnly ? "Announcements (read-only)" : "Clan chat");
        }

        // ============================================================================================
        // Composer actions (display-only; OnSend optimistically appends, never server-authoritative)
        // ============================================================================================
        private void RefreshSendState()
        {
            bool has = _composerInput != null && !string.IsNullOrEmpty(_composerInput.text.Trim());
            if (_sendBtn != null) _sendBtn.interactable = has; // Send disabled when empty (Section L)
        }

        private void OnSend()
        {
            if (_composerInput == null) return;
            string msg = _composerInput.text.Trim();
            if (string.IsNullOrEmpty(msg)) { RefreshSendState(); return; } // never send empty
            // §12: send is a server-authoritative request; the UI optimistically appends a local bubble only.
            AppendChat("Aric Stormblade", msg, "now", true);
            _composerInput.text = "";
            RefreshSendState();
            StartCoroutine(PressPop(_sendImg != null ? (RectTransform)_sendImg.transform : null));
            StartCoroutine(ScrollChatToBottom());
        }

        private void OnEmoji()
        {
            // Open picker → insert a glyph (display-only).
            if (_composerInput != null) { _composerInput.text += "\U0001F525"; RefreshSendState(); }
            Router.Toast("Emoji picker — coming soon");
        }

        // LEAVE CLAN → Confirm modal stub (never skip the confirm — Section L). Confirm is display-only.
        private void ConfirmLeave()
        {
            // Spec 37 Confirm modal is a separate screen (not yet built) — surface the confirm step as a toast
            // so the destructive action is never performed inline/without confirmation (§12 + Section L).
            Router.Toast("Leave Clan? Confirm required (spec 37) — coming soon");
        }

        // ============================================================================================
        // Shared builders
        // ============================================================================================
        // A column panel: obsidian field + gold ornate frame, anchored to its x band over the content vertical band.
        private RectTransform ColumnPanel(string name, float xMin, float xMax, out CanvasGroup cg)
        {
            var goP = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var p = (RectTransform)goP.transform; p.SetParent(SafeContent, false);
            p.anchorMin = new Vector2(xMin, ContentBot); p.anchorMax = new Vector2(xMax, ContentTop);
            p.offsetMin = Vector2.zero; p.offsetMax = Vector2.zero;
            cg = goP.GetComponent<CanvasGroup>();
            var field = goP.GetComponent<Image>(); field.raycastTarget = true; // panel catches stray taps
            field.sprite = UiTex.VGradient(PanelTop, PanelBot, 64); field.color = UiTheme.A(Color.white, 0.96f);
            // Gold ornate frame molding over the field edge.
            var frame = UiWidgets.Rect("Frame", p, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fImg = frame.gameObject.AddComponent<Image>(); fImg.raycastTarget = false;
            fImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 14); fImg.type = Image.Type.Sliced;
            return p;
        }

        // A masked vertical ScrollRect host inside a panel; returns the host and outputs the content rect.
        private RectTransform ScrollHost(RectTransform panel, out RectTransform content, float totalH)
        {
            var host = UiWidgets.Rect("ScrollView", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Elastic; scroll.elasticity = 0.1f; scroll.scrollSensitivity = 32f;
            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0); vImg.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            content = UiWidgets.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, totalH));
            content.pivot = new Vector2(0.5f, 1f);
            scroll.viewport = viewport; scroll.content = content;
            return host;
        }

        // A 9-slice gold rim over a rect (matches the GemButton/Card rim treatment).
        private static void RimImage(RectTransform rt, int border)
        {
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = rim.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.85f), UiTheme.A(UiTheme.Gold, 0.75f), UiTheme.A(UiTheme.GoldShadow, 0.75f), 48, border); img.type = Image.Type.Sliced;
        }

        // ============================================================================================
        // Lifecycle / enter timeline (Section I)
        // ============================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterTimeline());
        }

        // OnShow: TopBar slide-in (handled by router fade) → Left slide+fade → roster rows stagger → Right
        // slide+fade → ActionBar raise → (You) row blue-glow flash. All on UNSCALED time.
        private IEnumerator EnterTimeline()
        {
            // Left column: slide −24px + fade (220 ms, ease-out).
            if (_leftCol != null && _leftCg != null) StartCoroutine(SlideFade(_leftCg, _leftCol, new Vector2(-24, 0), 0.22f, 0.08f));
            // Right column: slide +24px + fade (220 ms).
            if (_rightCol != null && _rightCg != null) StartCoroutine(SlideFade(_rightCg, _rightCol, new Vector2(24, 0), 0.22f, 0.14f));
            // Center: present immediately (its children animate).
            yield return null;

            // Roster rows stagger-in (α + slide +12px), ~30 ms apart, 160 ms each.
            yield return StaggerRows(0.03f);

            // ActionBar fades/raises in (180 ms) at ~260 ms.
            if (_actionBar != null && _actionCg != null)
            {
                _actionCg.alpha = 0f; Vector2 home = _actionBar.anchoredPosition; Vector2 from = home + new Vector2(0, -16);
                float t = 0f; const float d = 0.18f;
                while (t < d && _actionCg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _actionCg.alpha = k; _actionBar.anchoredPosition = Vector2.Lerp(from, home, k); yield return null; }
                if (_actionCg != null) { _actionCg.alpha = 1f; _actionBar.anchoredPosition = home; }
            }

            // (You) row blue-glow flash (220 ms) at ~300 ms.
            if (_youRowCg != null) StartCoroutine(YouFlash());
        }

        private IEnumerator SlideFade(CanvasGroup cg, RectTransform rt, Vector2 offset, float dur, float delay)
        {
            float w = 0f; while (w < delay) { w += Time.unscaledDeltaTime; yield return null; }
            if (cg == null) yield break;
            Vector2 home = rt.anchoredPosition; Vector2 from = home + offset; cg.alpha = 0f; rt.anchoredPosition = from;
            float t = 0f;
            while (t < dur && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dur); float e = 1f - (1f - k) * (1f - k); cg.alpha = k; rt.anchoredPosition = Vector2.Lerp(from, home, e); yield return null; }
            if (cg != null) { cg.alpha = 1f; rt.anchoredPosition = home; }
        }

        private IEnumerator StaggerRows(float step)
        {
            if (_rowCg == null) yield break;
            for (int i = 0; i < _rowCg.Length; i++)
            {
                if (_rowCg[i] == null) continue;
                StartCoroutine(RowIn(i));
                float s = 0f; while (s < step) { s += Time.unscaledDeltaTime; yield return null; }
            }
        }

        private IEnumerator RowIn(int i)
        {
            var cg = _rowCg[i]; if (cg == null) yield break;
            var rt = (RectTransform)cg.transform; Vector2 home = rt.anchoredPosition; Vector2 from = home + new Vector2(0, -12f);
            cg.alpha = 0f; rt.anchoredPosition = from; float t = 0f; const float d = 0.16f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = k; rt.anchoredPosition = Vector2.Lerp(from, home, k); yield return null; }
            if (cg != null) { cg.alpha = 1f; rt.anchoredPosition = home; }
        }

        private IEnumerator YouFlash()
        {
            var cg = _youRowCg; if (cg == null) yield break;
            float t = 0f; const float d = 0.22f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Sin(Mathf.Clamp01(t / d) * Mathf.PI); cg.transform.localScale = Vector3.one * (1f + 0.03f * k); yield return null; }
            if (cg != null) cg.transform.localScale = Vector3.one;
        }

        private IEnumerator GlideIndicator(float toX)
        {
            if (_tabIndicator == null) yield break;
            Vector2 p = _tabIndicator.anchoredPosition; float fromX = p.x; float t = 0f; const float d = 0.14f;
            while (t < d && _tabIndicator != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); p.x = Mathf.Lerp(fromX, toX, k); _tabIndicator.anchoredPosition = p; yield return null; }
            if (_tabIndicator != null) { p.x = toX; _tabIndicator.anchoredPosition = p; }
        }

        private IEnumerator CrossfadePanels(int oldIdx, int newIdx)
        {
            var oldP = _tabPanelCg[oldIdx]; var newP = _tabPanelCg[newIdx];
            var oldRt = _tabPanels[oldIdx]; var newRt = _tabPanels[newIdx];
            if (oldP != null) oldP.blocksRaycasts = false;
            if (newP != null) { newP.blocksRaycasts = true; newP.alpha = 0f; }
            Vector2 oldHome = oldRt != null ? oldRt.anchoredPosition : Vector2.zero;
            Vector2 newHome = newRt != null ? newRt.anchoredPosition : Vector2.zero;
            // Old: α 1→0 + slide −10px (120 ms).
            float t = 0f; const float dOut = 0.12f;
            while (t < dOut) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dOut); if (oldP != null) oldP.alpha = 1f - k; if (oldRt != null) oldRt.anchoredPosition = oldHome + new Vector2(-10f * k, 0); yield return null; }
            if (oldP != null) oldP.alpha = 0f; if (oldRt != null) oldRt.anchoredPosition = oldHome;
            // New: α 0→1 + slide +10px→0 (160 ms).
            t = 0f; const float dIn = 0.16f;
            if (newRt != null) newRt.anchoredPosition = newHome + new Vector2(10f, 0);
            while (t < dIn) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dIn); if (newP != null) newP.alpha = k; if (newRt != null) newRt.anchoredPosition = Vector2.Lerp(newHome + new Vector2(10f, 0), newHome, k); yield return null; }
            if (newP != null) newP.alpha = 1f; if (newRt != null) newRt.anchoredPosition = newHome;
        }

        // New chat bubble: α 0→1 + slide +10px→0 + scale 0.98→1 over 180 ms.
        private IEnumerator BubbleArrive(CanvasGroup cg, RectTransform rt)
        {
            if (cg == null) yield break;
            Vector2 home = rt.anchoredPosition; Vector2 from = home + new Vector2(10f, 0);
            cg.alpha = 0f; rt.anchoredPosition = from; rt.localScale = Vector3.one * 0.98f;
            float t = 0f; const float d = 0.18f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = k; rt.anchoredPosition = Vector2.Lerp(from, home, k); rt.localScale = Vector3.one * Mathf.Lerp(0.98f, 1f, k); yield return null; }
            if (cg != null) { cg.alpha = 1f; rt.anchoredPosition = home; rt.localScale = Vector3.one; }
        }

        private IEnumerator ScrollChatToBottom()
        {
            if (_chatScroll == null) yield break;
            float t = 0f; const float d = 0.16f; float from = _chatScroll.verticalNormalizedPosition;
            // Force a layout pass so content size is up to date before easing.
            yield return null;
            while (t < d && _chatScroll != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _chatScroll.verticalNormalizedPosition = Mathf.Lerp(from, 0f, k); yield return null; }
            if (_chatScroll != null) _chatScroll.verticalNormalizedPosition = 0f;
        }

        private IEnumerator PressPop(RectTransform rt)
        {
            if (rt == null) yield break;
            float t = 0f; const float d = 0.08f;
            while (t < d && rt != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); rt.localScale = Vector3.one * (1f - 0.04f * Mathf.Sin(k * Mathf.PI)); yield return null; }
            if (rt != null) rt.localScale = Vector3.one;
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
