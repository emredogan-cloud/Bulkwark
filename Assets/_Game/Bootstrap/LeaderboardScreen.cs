// BULWARK — LEADERBOARD (UI Construction Bible · 34). Presentation-only, REMOVABLE.
//
// §12 boundary: presentation only — NO ECS / NO Unity.Entities / NO gameplay/balance/AI/economy/backend. This is a
// forensic code-built rebuild of design/LeaderboardScreenDesign.png (Bible 34): an ornate gold-framed competitive
// ranking board over a vignetted dusk-castle skyline — serif "LEADERBOARD" title with crossed-sword corner
// ornaments; GLOBAL/FRIENDS/SEASON tabs (GLOBAL selected, blue) + a season countdown cluster; a left rail
// (LEGENDARY trophy emblem, description, blue "League Rewards", "My Rank 128" / "My Score 2,345,678"); and a
// four-column (RANK/PLAYER/LEAGUE/SCORE) scrollable list with gold/silver/bronze medallions on ranks 1-3, plain
// numerals on 4+, plus a blue-outlined "My Rank" row pinned (never-scrolling) at the bottom and the fine-print
// "Leaderboard updates every 15 minutes." footer. ALL ranking data is clearly DISPLAY-ONLY local stub data; the
// client never computes/edits any rank or score (server-authoritative, §L) — tabs switch + rebuild the list and
// row/CTA taps surface a Router.Toast only. NO wallet chip on this screen (matches source, §L). (§12)

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-34 landscape Leaderboard (ranking board). Presentation-only, server-authoritative display.</summary>
    public sealed class LeaderboardScreen : UiScreen
    {
        // ---- Canvas-fraction helpers (spec E: x = frac×2340, y = frac×1080; anchorY = 1 − fy_from_top). ----
        private const float CW = 2340f, CHt = 1080f;

        // ---- Exact spec hex not in UiTheme (Section F/G). Display-only colours. ----
        private static readonly Color SkylineTop = Hex("#14161e"); // dusk skyline top
        private static readonly Color SkylineBot = Hex("#0a0b0f"); // near-black field bottom
        private static readonly Color BoardFill  = Hex("#0c0e14"); // obsidian board interior (§G)
        private static readonly Color RailFill   = Hex("#0e1018"); // slightly darker rail inset (§G)
        private static readonly Color Cobalt     = Hex("#2b56c8"); // royal cobalt (selected tab / CTA / pin)
        private static readonly Color CobaltHi   = Hex("#4f8bff"); // bright cobalt
        private static readonly Color CaptionCol = Hex("#b8a06a"); // "Season Ends In" / My Rank/Score captions
        private static readonly Color CountdownCol = Hex("#f0d27a"); // countdown value (gold)
        private static readonly Color LeagueDescCol = Hex("#c9bfa6"); // league description body
        private static readonly Color ColHeaderCol = Hex("#9a8a6a"); // RANK/PLAYER/LEAGUE/SCORE small caps
        private static readonly Color NameCol     = Hex("#f3ead2"); // player name
        private static readonly Color GuildCol    = Hex("#8c8270"); // guild subtitle
        private static readonly Color ScoreCol    = Hex("#f0d27a"); // row score (gold tabular)
        private static readonly Color MyRankVal   = Hex("#f0d27a"); // "128"
        private static readonly Color MyScoreVal  = Hex("#e9dcc0"); // "2,345,678"
        private static readonly Color FooterCol   = Hex("#7a7060"); // fine-print footer
        private static readonly Color CtaWhite    = Hex("#ffffff"); // CTA / tab-selected label
        private static readonly Color TabIdleCol  = Hex("#9a8a6a"); // unselected tab label
        // Medallion metals (§G).
        private static readonly Color MedalGoldHi = Hex("#f6dd86"), MedalGoldMid = Hex("#b8862c"), MedalGoldSh = Hex("#6b4a14");
        private static readonly Color MedalSilvHi = Hex("#e6ebf1"), MedalSilvMid = Hex("#b8c0c9"), MedalSilvSh = Hex("#6c747d");
        private static readonly Color MedalBronHi = Hex("#e0a064"), MedalBronMid = Hex("#b9743a"), MedalBronSh = Hex("#6e3f1c");
        private static readonly Color MedalNumeral = Hex("#2a2010"); // debossed numeral on metal
        // League tier text colours (§F).
        private static readonly Color LeagueLeg   = Hex("#b98bff"); // LEGENDARY violet
        private static readonly Color LeagueChamp = Hex("#d8a14a"); // CHAMPION bronze
        private static readonly Color LeagueDia   = Hex("#6fa8ff"); // DIAMOND blue

        // ---- Verbatim copy (§A/§M). Display-only consts. ----
        private const string LeagueName = "LEGENDARY LEAGUE";
        private const string LeagueDesc = "Compete with warriors from around the world and climb the ranks!";
        private const string FooterText = "Leaderboard updates every 15 minutes.";
        private const string SeasonEndsCaption = "Season Ends In";
        // Countdown seed (§M format "Nd Nh Nm"). Display-only; ticks down locally (no server, no gameplay).
        private int _cdDays = 13, _cdHours = 14, _cdMins = 22;

        // ---- My-rank pinned row (§D/§M): 128 / ValiantOne / Silver Wardens / DIAMOND II / 2,345,678. Per-scope. ----
        private struct Self { public string Rank, Name, Guild, League, Score; public LeagueTier Tier; }
        private enum LeagueTier { Legendary, Champion, Diamond }

        // ---- A ranked entry (display-only; client NEVER computes/edits rank or score, §L). ----
        private struct Entry { public int Rank; public string Name, Guild, Score; public LeagueTier Tier; }

        // GLOBAL scope — the six EXACT seed rows the spec mandates (§C/§M criterion 4).
        private static readonly Entry[] GlobalRows =
        {
            new Entry{ Rank=1, Name="BloodReaver",  Guild="Death's Vanguard", Tier=LeagueTier.Legendary, Score="5,824,910" },
            new Entry{ Rank=2, Name="Shadowblade",  Guild="Nightfall Order",  Tier=LeagueTier.Legendary, Score="5,231,780" },
            new Entry{ Rank=3, Name="Grimlord",     Guild="Iron Dominion",    Tier=LeagueTier.Legendary, Score="4,789,450" },
            new Entry{ Rank=4, Name="Frostborne",   Guild="Northern Pact",    Tier=LeagueTier.Champion,  Score="4,125,670" },
            new Entry{ Rank=5, Name="Stormbringer", Guild="Wardens",          Tier=LeagueTier.Champion,  Score="3,876,540" },
            new Entry{ Rank=6, Name="Ravenstrike",  Guild="Silent Talons",    Tier=LeagueTier.Champion,  Score="3,542,180" },
            // Filler entries so the list overflows the viewport and the ScrollRect scrolls (§A/§E "long list").
            // Inert layout-only data — NOT real ranks, never server-truth (§L).
            new Entry{ Rank=7,  Name="Ironhowl",    Guild="Granite Host",     Tier=LeagueTier.Champion,  Score="3,310,220" },
            new Entry{ Rank=8,  Name="Duskwarden",  Guild="Ashen Vow",        Tier=LeagueTier.Champion,  Score="3,104,990" },
            new Entry{ Rank=9,  Name="Emberfang",   Guild="Cinder Clans",     Tier=LeagueTier.Champion,  Score="2,948,710" },
            new Entry{ Rank=10, Name="Thornguard",  Guild="Bramble Pact",     Tier=LeagueTier.Diamond,   Score="2,807,330" },
            new Entry{ Rank=11, Name="Nightspear",  Guild="Hollow Reach",     Tier=LeagueTier.Diamond,   Score="2,694,150" },
            new Entry{ Rank=12, Name="Stoneveil",   Guild="Old Bastion",      Tier=LeagueTier.Diamond,   Score="2,571,640" },
        };

        // FRIENDS scope — social-graph subset (display-only).
        private static readonly Entry[] FriendsRows =
        {
            new Entry{ Rank=1, Name="Stormbringer", Guild="Wardens",          Tier=LeagueTier.Champion,  Score="3,876,540" },
            new Entry{ Rank=2, Name="Thornguard",   Guild="Bramble Pact",     Tier=LeagueTier.Diamond,   Score="2,807,330" },
            new Entry{ Rank=3, Name="ValiantOne",   Guild="Silver Wardens",   Tier=LeagueTier.Diamond,   Score="2,345,678" },
            new Entry{ Rank=4, Name="Stoneveil",    Guild="Old Bastion",      Tier=LeagueTier.Diamond,   Score="2,571,640" },
            new Entry{ Rank=5, Name="Lowtide",      Guild="Driftmark",        Tier=LeagueTier.Diamond,   Score="1,998,210" },
        };

        // SEASON scope — current-season-only board (display-only).
        private static readonly Entry[] SeasonRows =
        {
            new Entry{ Rank=1, Name="Grimlord",     Guild="Iron Dominion",    Tier=LeagueTier.Legendary, Score="1,204,450" },
            new Entry{ Rank=2, Name="BloodReaver",  Guild="Death's Vanguard", Tier=LeagueTier.Legendary, Score="1,180,910" },
            new Entry{ Rank=3, Name="Frostborne",   Guild="Northern Pact",    Tier=LeagueTier.Champion,  Score="1,025,670" },
            new Entry{ Rank=4, Name="Emberfang",    Guild="Cinder Clans",     Tier=LeagueTier.Champion,  Score="948,710" },
            new Entry{ Rank=5, Name="Nightspear",   Guild="Hollow Reach",     Tier=LeagueTier.Diamond,   Score="894,150" },
            new Entry{ Rank=6, Name="ValiantOne",   Guild="Silver Wardens",   Tier=LeagueTier.Diamond,   Score="812,540" },
            new Entry{ Rank=7, Name="Stoneveil",    Guild="Old Bastion",      Tier=LeagueTier.Diamond,   Score="771,640" },
        };

        // My-rank, per scope (its rank/score legitimately differ per scope, §K). Display-only.
        private static readonly Self[] MyByScope =
        {
            new Self{ Rank="128", Name="ValiantOne", Guild="Silver Wardens", League="DIAMOND II", Tier=LeagueTier.Diamond, Score="2,345,678" }, // GLOBAL
            new Self{ Rank="3",   Name="ValiantOne", Guild="Silver Wardens", League="DIAMOND II", Tier=LeagueTier.Diamond, Score="2,345,678" }, // FRIENDS
            new Self{ Rank="6",   Name="ValiantOne", Guild="Silver Wardens", League="DIAMOND II", Tier=LeagueTier.Diamond, Score="812,540"   }, // SEASON
        };

        private static readonly string[] TabNames = { "GLOBAL", "FRIENDS", "SEASON" };

        // ---- ListArea geometry (canvas fractions, §E). Region right of the rail, below the tab row. ----
        // BoardFrame inner ≈ x[0.034,0.966], y[0.04,0.97]; rail occupies the left ~0.205 of the inner width.
        private const float ListXMin = 0.250f, ListXMax = 0.952f; // canvas-x span of the list column area
        private const float ListYTop = 0.140f, ListYBot = 0.060f; // fy_from_top of the list region (header→pinned)
        private const float HeaderFy = 0.255f;                    // column-header band centre (fy)
        private const float RowH = 86f;                           // §E row height @1080
        private const float RowGap = 10f;                         // §E row gap
        private const float RowPitch = RowH + RowGap;

        // ---- Runtime state ----
        private int _scope;                       // 0 GLOBAL · 1 FRIENDS · 2 SEASON
        private RectTransform _listContent;       // ScrollRect content (rows parented here; rebuilt on tab switch)
        private RectTransform _pinHost;           // pinned my-rank row host (rebuilt on tab switch — value is per-scope)
        private ScrollRect _scroll;
        private Text _countdownText;
        private CanvasGroup _boardCg;
        private Button[] _tabBtns;

        // =====================================================================================================
        protected override void Build()
        {
            BuildBackground();
            BuildBoardFrame();
            BuildTopBar();
            BuildTabRow();
            BuildLeftRail();
            BuildListArea();
            BuildFooter();

            PopulateList();
            BuildPinnedRow();
        }

        // ---- BackgroundLayer (full-bleed, under the cutout → Rect): dusk skyline + heavy vignette (§B/§D). ----
        private void BuildBackground()
        {
            var sky = UiWidgets.Stretch("BG_Skyline", Rect, UiTheme.Obsidian, "bg_menu");
            sky.raycastTarget = false;
            // Charcoal dusk grade over the key art (#14161e → #0a0b0f, §B).
            var grade = UiWidgets.Stretch("BG_DuskGrade", Rect, Color.white);
            grade.raycastTarget = false; grade.sprite = UiTex.VGradient(SkylineTop, SkylineBot, 64);
            grade.color = new Color(1f, 1f, 1f, 0.92f);
            UiWidgets.Vignette(Rect, 0.62f); // heavy → the board reads as the focal subject (§B/§D)
        }

        // ---- BoardFrame: ornate gold/bronze frame over an obsidian field (§D/§G). Decorative; content sits on
        //      SafeContent in canvas fractions. Clamp to ~92% width per §E (centred). ----
        private void BuildBoardFrame()
        {
            // Inset ~3.4% L/R, ~3% top, ~4% bottom (§E) → centre + size in px.
            float xMin = 0.034f, xMax = 0.966f, fyTop = 0.030f, fyBot = 0.970f;
            float wPx = Mathf.Min((xMax - xMin) * CW, 0.92f * CW); // §E max-width clamp
            float hPx = (fyBot - fyTop) * CHt;
            var boardGo = new GameObject("BoardFrame", typeof(RectTransform), typeof(CanvasGroup));
            var board = (RectTransform)boardGo.transform; board.SetParent(SafeContent, false);
            board.anchorMin = board.anchorMax = new Vector2(0.5f, 0.5f);
            board.sizeDelta = new Vector2(wPx, hPx); board.anchoredPosition = Vector2.zero;
            _boardCg = boardGo.GetComponent<CanvasGroup>();
            // Ornate cast-gold molding over an obsidian field (returns the inner field; we keep it as backdrop).
            UiWidgets.OrnateFrame(board, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(wPx, hPx), BoardFill, true, 28f);
            // Soft inner drop-shadow on the list area + faint focal bloom upper-centre (§B/§G).
            UiWidgets.Vignette(board, 0.30f);
        }

        // ---- TopBar: BackButton (TL), crossed-sword ornaments flanking the serif "LEADERBOARD" title (§C/§D). ----
        private void BuildTopBar()
        {
            // Top-left back → pop to caller (Main-Menu rail). Task-mandated signature.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Title "LEADERBOARD" centred, fy≈0.075 → anchorY 0.925, serif gold gradient (cap≈64, §F).
            float titleFy = 0.075f, titleAy = 1f - titleFy;
            UiWidgets.TitleLabel(SafeContent, "LEADERBOARD", 64, new Vector2(0.5f, titleAy), new Vector2(0.5f, titleAy),
                Vector2.zero, new Vector2(1100, 96), TextAnchor.MiddleCenter, Hex("#f0d27a"), Hex("#caa04a"));

            // Crossed-sword corner ornaments ±(title half-width + ~40px) (§D). Built from rotated finials.
            CrossedSwords(new Vector2(0.5f, titleAy), new Vector2(-560, 0));
            CrossedSwords(new Vector2(0.5f, titleAy), new Vector2(560, 0));
        }

        // A small crossed-swords flourish (two long rotated gold finials) at an anchor.
        private void CrossedSwords(Vector2 anchor, Vector2 offset)
        {
            var grp = UiWidgets.Rect("TitleOrnament", SafeContent, anchor, anchor, offset, new Vector2(90, 90));
            var a = UiWidgets.Finial(grp, new Vector2(0.5f, 0.5f), Vector2.zero, 78f); a.color = UiTheme.GoldHi; a.transform.localRotation = Quaternion.Euler(0, 0, 42);
            var b = UiWidgets.Finial(grp, new Vector2(0.5f, 0.5f), Vector2.zero, 78f); b.color = UiTheme.Gold;   b.transform.localRotation = Quaternion.Euler(0, 0, -42);
            UiWidgets.Glow(grp, UiTheme.A(UiTheme.GoldHi, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150), 1.6f);
        }

        // ---- TabRow: GLOBAL/FRIENDS/SEASON via UiWidgets.TabBar (left band) + season-timer cluster (right) (§C/§D). ----
        private void BuildTabRow()
        {
            // Three tabs occupy the left band starting at x≈0.20 (§E). TabBar anchors its left edge at aMin.x.
            float tabFy = 0.180f, tabAy = 1f - tabFy;
            float tabAreaXMin = 0.060f;                 // left start (inside frame, below back/title)
            float tabAreaW = 0.620f * CW;               // three 200-wide capsules + gaps band
            _tabBtns = UiWidgets.TabBar(SafeContent, TabNames,
                new Vector2(tabAreaXMin, tabAy), new Vector2(tabAreaXMin, tabAy),
                Vector2.zero, new Vector2(tabAreaW, 0.058f * CHt), _scope, OnSelectTab);
            StyleTabs();

            // SeasonTimerCluster — right-anchored (§D): "Season Ends In" caption | hourglass | countdown value.
            float clX = 0.890f, clAy = tabAy;
            UiWidgets.Label(SafeContent, SeasonEndsCaption, 22, new Vector2(clX, clAy + 0.020f), new Vector2(clX, clAy + 0.020f),
                Vector2.zero, new Vector2(360, 30), TextAnchor.MiddleCenter, CaptionCol);
            // Hourglass icon (diamond stand-in, §N) at the cluster left.
            var hg = UiWidgets.Rect("Icon_Hourglass", SafeContent, new Vector2(clX - 0.060f, clAy - 0.012f), new Vector2(clX - 0.060f, clAy - 0.012f), Vector2.zero, new Vector2(40, 40));
            var hgImg = hg.gameObject.AddComponent<Image>(); hgImg.raycastTarget = false; hgImg.sprite = UiTex.Diamond(CountdownCol, 48);
            _countdownText = UiWidgets.Label(SafeContent, CountdownString(), 28, new Vector2(clX + 0.012f, clAy - 0.012f), new Vector2(clX + 0.012f, clAy - 0.012f),
                Vector2.zero, new Vector2(320, 36), TextAnchor.MiddleLeft, CountdownCol);
        }

        // Recolour TabBar buttons to the spec's blue-selected / ghost-idle states (§B/§H) + selected glow.
        // Re-callable on tab switch: clears prior TabRim/Glow decorations first so they never accumulate.
        private void StyleTabs()
        {
            if (_tabBtns == null) return;
            for (int i = 0; i < _tabBtns.Length; i++)
            {
                var btn = _tabBtns[i]; if (btn == null) continue;
                bool sel = i == _scope;
                // Clear decorations added on a previous styling pass.
                for (int c = btn.transform.childCount - 1; c >= 0; c--)
                {
                    var ch = btn.transform.GetChild(c);
                    if (ch.name == "TabRim" || ch.name == "Glow") Object.Destroy(ch.gameObject);
                }
                var img = btn.targetGraphic as Image;
                if (img != null) img.color = sel ? Cobalt : UiTheme.A(UiTheme.Charcoal, 0.55f); // filled blue vs ghost
                // Selected tab gets a subtle pulsing rim glow (§J) + a thin gold trim; idle gets a faint gold trim.
                var rim = UiWidgets.Rect("TabRim", btn.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var rImg = rim.gameObject.AddComponent<Image>(); rImg.raycastTarget = false;
                rImg.sprite = UiTex.Frame(sel ? CobaltHi : UiTheme.A(UiTheme.Gold, 0.45f), sel ? Cobalt : UiTheme.A(UiTheme.Gold, 0.35f), UiTheme.GoldShadow, 48, 5); rImg.type = Image.Type.Sliced;
                if (sel)
                {
                    var glow = UiWidgets.Glow(btn.transform, UiTheme.A(CobaltHi, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(230, 90), 1.7f);
                    glow.transform.SetAsFirstSibling();
                    var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.3f; pg.max = 0.55f; pg.period = 1.6f;
                }
                // Recolour the TabBar's own label (white selected / muted gold-grey idle).
                var lbl = btn.GetComponentInChildren<Text>(); if (lbl != null) lbl.color = sel ? CtaWhite : TabIdleCol;
            }
        }

        // ---- LeftRail (sub-panel): emblem → league name → desc → blue rewards CTA → My Rank → My Score (§C/§E). ----
        private void BuildLeftRail()
        {
            // Rail region x≈0.045→0.235 of canvas, y inside the frame below the tab row (§E ≈0.205 inner width).
            float xL = 0.045f, xR = 0.235f, fyTop = 0.150f, fyBot = 0.940f;
            float wPx = (xR - xL) * CW, hPx = (fyBot - fyTop) * CHt;
            float cx = (xL + xR) * 0.5f, cy = 1f - (fyTop + fyBot) * 0.5f;

            var rail = UiWidgets.Rect("LeftRail", SafeContent, new Vector2(cx, cy), new Vector2(cx, cy), Vector2.zero, new Vector2(wPx, hPx));
            var bg = rail.gameObject.AddComponent<Image>(); bg.raycastTarget = false;
            bg.sprite = UiTex.VGradient(UiWidgets.Lighten(RailFill, 0.02f), UiWidgets.Darken(RailFill, 0.25f), 64); // darker inset (§G)
            var rim = UiWidgets.Rect("RailRim", rail, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = rim.gameObject.AddComponent<Image>(); rImg.raycastTarget = false;
            rImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.6f), UiTheme.A(UiTheme.Gold, 0.5f), UiTheme.GoldShadow, 48, 6); rImg.type = Image.Type.Sliced;

            // LeagueEmblem (top-centre, ~150px, §E): polished gold trophy/crest stand-in + blue gem inset + focal bloom.
            var emblem = UiWidgets.Rect("LeagueEmblem", rail, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(150, 150));
            UiWidgets.Glow(emblem, UiTheme.A(UiTheme.GoldHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 220), 1.6f); // hero bloom (§J)
            var emImg = emblem.gameObject.AddComponent<Image>(); emImg.raycastTarget = false; emImg.sprite = UiTex.Disc(UiTheme.Gold, 64);
            var emRing = UiWidgets.Rect("EmblemRing", emblem, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var emrImg = emRing.gameObject.AddComponent<Image>(); emrImg.raycastTarget = false; emrImg.sprite = UiTex.Frame(MedalGoldHi, MedalGoldMid, MedalGoldSh, 64, 9); emrImg.type = Image.Type.Sliced;
            var emCrest = UiWidgets.Rect("EmblemCrest", emblem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64, 64));
            var emcImg = emCrest.gameObject.AddComponent<Image>(); emcImg.raycastTarget = false; emcImg.sprite = UiTex.Diamond(MedalGoldHi, 48);
            var emGem = UiWidgets.Rect("EmblemGem", emblem, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(26, 26));
            var emgImg = emGem.gameObject.AddComponent<Image>(); emgImg.raycastTarget = false; emgImg.sprite = UiTex.Diamond(CobaltHi, 32); // blue gem inset (§G)
            emblem.gameObject.AddComponent<PulseScale>().period = 2.4f;

            // Lbl_LeagueName "LEGENDARY LEAGUE" (serif gold, ~32px, §F).
            UiWidgets.TitleLabel(rail, LeagueName, 32, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(wPx - 30, 44), TextAnchor.MiddleCenter, Hex("#f0d27a"), Hex("#caa04a"));

            // Lbl_LeagueDesc (3-line wrap body, ~22px, §F).
            var desc = UiWidgets.Label(rail, LeagueDesc, 22, new Vector2(0.5f, 0.575f), new Vector2(0.5f, 0.575f), Vector2.zero, new Vector2(wPx - 56, 120), TextAnchor.UpperCenter, LeagueDescCol);
            desc.horizontalOverflow = HorizontalWrapMode.Wrap; desc.verticalOverflow = VerticalWrapMode.Truncate;

            // Btn_LeagueRewards (blue capsule, centred, ≥64px, §E/§H) → server-validated detail (display-only Toast, §K).
            BuildBlueRewardsButton(rail, wPx);

            // Block_MyRank ("My Rank" caption over "128", §C/§E/§F).
            UiWidgets.Label(rail, "My Rank", 22, new Vector2(0.5f, 0.250f), new Vector2(0.5f, 0.250f), Vector2.zero, new Vector2(wPx - 40, 30), TextAnchor.MiddleCenter, CaptionCol);
            // Gold focal glow BEHIND the value (parented to the rail before the label so "128" draws on top, §F).
            UiWidgets.Glow(rail, UiTheme.A(UiTheme.GoldHi, 0.28f), new Vector2(0.5f, 0.190f), new Vector2(0.5f, 0.190f), Vector2.zero, new Vector2(170, 100), 1.7f);
            var myRank = UiWidgets.Label(rail, "128", 52, new Vector2(0.5f, 0.190f), new Vector2(0.5f, 0.190f), Vector2.zero, new Vector2(wPx - 40, 64), TextAnchor.MiddleCenter, MyRankVal);
            myRank.gameObject.AddComponent<UiGradientText>().top = MedalGoldHi;

            // Block_MyScore ("My Score" caption over "2,345,678", §C/§E/§F).
            UiWidgets.Label(rail, "My Score", 22, new Vector2(0.5f, 0.105f), new Vector2(0.5f, 0.105f), Vector2.zero, new Vector2(wPx - 40, 30), TextAnchor.MiddleCenter, CaptionCol);
            UiWidgets.Label(rail, "2,345,678", 30, new Vector2(0.5f, 0.055f), new Vector2(0.5f, 0.055f), Vector2.zero, new Vector2(wPx - 40, 40), TextAnchor.MiddleCenter, MyScoreVal);
        }

        private void BuildBlueRewardsButton(RectTransform rail, float wPx)
        {
            float h = 72f, w = wPx - 70f;
            var cta = UiWidgets.Rect("Btn_LeagueRewards", rail, new Vector2(0.5f, 0.420f), new Vector2(0.5f, 0.420f), Vector2.zero, new Vector2(w, h));
            // Idle cobalt gradient + soft outer glow (§G/§H/§J).
            var glow = UiWidgets.Glow(cta, UiTheme.A(CobaltHi, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w + 40, h + 50), 1.7f);
            var gpg = glow.gameObject.AddComponent<PulseGraphic>(); gpg.target = glow; gpg.min = 0.3f; gpg.max = 0.55f; gpg.period = 1.6f;
            var body = cta.gameObject.AddComponent<Image>(); body.sprite = UiTex.VGradient(CobaltHi, UiWidgets.Darken(Cobalt, 0.34f), 32);
            var btn = cta.gameObject.AddComponent<Button>(); btn.targetGraphic = body;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("League Rewards — server-validated (coming soon)"); });
            var rim = UiWidgets.Rect("CtaRim", cta, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = rim.gameObject.AddComponent<Image>(); rImg.raycastTarget = false; rImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); rImg.type = Image.Type.Sliced;
            var lbl = UiWidgets.Label(cta, "League Rewards", 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CtaWhite);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
        }

        // ---- ListArea: column header (fixed top) + ScrollView (fills middle) + pinned row (fixed bottom) (§C/§E). ----
        private void BuildListArea()
        {
            // ColumnHeaderRow (RANK/PLAYER/LEAGUE/SCORE), small caps, fy≈0.255 (§E ~42px band, §F).
            float hAy = 1f - HeaderFy;
            ColumnHeader("RANK",   ColCenterX(Col.Rank),   TextAnchor.MiddleCenter, hAy);
            ColumnHeader("PLAYER", ColCenterX(Col.Player), TextAnchor.MiddleLeft,   hAy);
            ColumnHeader("LEAGUE", ColCenterX(Col.League), TextAnchor.MiddleCenter, hAy);
            ColumnHeader("SCORE",  ColCenterX(Col.Score),  TextAnchor.MiddleRight,  hAy);
            // Thin gold rule beneath the header (§G inner shadow / column separation).
            UiWidgets.Divider(SafeContent, new Vector2((ListXMin + ListXMax) * 0.5f, hAy - 0.022f), new Vector2((ListXMin + ListXMax) * 0.5f, hAy - 0.022f), Vector2.zero, (ListXMax - ListXMin) * CW, 2f, UiTheme.A(UiTheme.Gold, 0.6f));

            // ScrollView region (fy from TOP): below the header (0.300) down to just above the pinned row (0.810).
            // Height = (bottomFy − topFy) × 1080 ≈ 550px → six 86px rows fit; overflow scrolls (§E).
            float scTopFy = 0.300f, scBotFy = 0.810f;     // bottom > top (both measured from screen top)
            float scCx = (ListXMin + ListXMax) * 0.5f;
            float scCy = 1f - (scTopFy + scBotFy) * 0.5f;
            float scW = (ListXMax - ListXMin) * CW, scH = (scBotFy - scTopFy) * CHt; // positive height in px
            var view = UiWidgets.Rect("ScrollView", SafeContent, new Vector2(scCx, scCy), new Vector2(scCx, scCy), Vector2.zero, new Vector2(scW, scH));
            var viewImg = view.gameObject.AddComponent<Image>(); viewImg.color = new Color(0, 0, 0, 0.001f); // raycast surface for drag
            view.gameObject.AddComponent<RectMask2D>();
            _scroll = view.gameObject.AddComponent<ScrollRect>();
            _scroll.horizontal = false; _scroll.vertical = true; _scroll.movementType = ScrollRect.MovementType.Elastic; // elastic overscroll (§H)
            _scroll.elasticity = 0.1f; _scroll.inertia = true; _scroll.scrollSensitivity = 30f; _scroll.viewport = view;

            // Content (top-anchored; height set per scope in PopulateList; rows positioned by anchoredPosition).
            var content = UiWidgets.Rect("Content", view, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, scH));
            content.pivot = new Vector2(0.5f, 1f);
            _listContent = content; _scroll.content = content;

            // MyRankPinnedRow host — fixed at the ListArea bottom (fy≈0.870 → anchorY 0.130), OUTSIDE the scroll
            // content so it never scrolls (§C/§D). ~8px gap above it from the viewport (§E).
            float pinFy = 0.870f, pinAy = 1f - pinFy;
            _pinHost = UiWidgets.Rect("MyRankPinnedRow", SafeContent, new Vector2(scCx, pinAy), new Vector2(scCx, pinAy), Vector2.zero, new Vector2(scW, 92f));
        }

        private void ColumnHeader(string text, float cx, TextAnchor align, float ay)
            => UiWidgets.Label(SafeContent, UiTheme.Track(text), 22, new Vector2(cx, ay), new Vector2(cx, ay), Vector2.zero, new Vector2(360, 30), align, ColHeaderCol);

        // =====================================================================================================
        // LIST POPULATION (rebuilt on tab switch — §I/§K cross-fade + re-stagger)
        // =====================================================================================================
        private Entry[] ScopeRows() => _scope == 1 ? FriendsRows : (_scope == 2 ? SeasonRows : GlobalRows);

        private void PopulateList()
        {
            if (_listContent == null) return;
            for (int i = _listContent.childCount - 1; i >= 0; i--) Object.Destroy(_listContent.GetChild(i).gameObject);

            var rows = ScopeRows();
            float totalH = rows.Length * RowPitch;
            _listContent.sizeDelta = new Vector2(0, totalH); // taller than the viewport → scrolls (§E)
            for (int i = 0; i < rows.Length; i++) BuildRow(_listContent, rows[i], i);
            if (_scroll != null) _scroll.verticalNormalizedPosition = 1f; // reset to top on (re)build
        }

        // Column x-fractions resolved into canvas-x (§E list column fractions, mapped into ListArea span).
        private enum Col { Rank, Player, League, Score }
        private float ColCenterX(Col c)
        {
            // §E fractions are relative to ListArea width — map back to absolute canvas-x.
            float local = c == Col.Rank ? 0.075f : (c == Col.Player ? 0.34f : (c == Col.League ? 0.70f : 0.92f));
            return ListXMin + local * (ListXMax - ListXMin);
        }
        // Avatar/name start (player column begins at local ≈0.16, §E).
        private float PlayerStartX() => ListXMin + 0.16f * (ListXMax - ListXMin);

        private void BuildRow(RectTransform content, Entry e, int index)
        {
            bool medal = e.Rank <= 3; // ranks 1-3 = medallion variant (§C/§D/§M)
            var rowGo = new GameObject("Row_" + e.Rank, typeof(RectTransform), typeof(CanvasGroup));
            var row = (RectTransform)rowGo.transform; row.SetParent(content, false);
            row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1f);
            row.offsetMin = Vector2.zero; row.offsetMax = Vector2.zero;
            row.sizeDelta = new Vector2(0, RowH); row.anchoredPosition = new Vector2(0, -index * RowPitch);
            var cg = rowGo.GetComponent<CanvasGroup>();

            // Row background — very subtle alternating stripe; medallion rows get a faint warm tint (§B/§H).
            var rb = row.gameObject.AddComponent<Image>();
            if (medal) rb.color = UiTheme.A(Hex("#1a140a"), 0.55f);        // faint warm tint
            else rb.color = UiTheme.A(index % 2 == 0 ? Hex("#101218") : Hex("#0c0e14"), 0.5f); // near-black on near-black stripe
            // Whole row tappable → display-only profile peek (§H/§K optional). Hover/press handled by Button colours.
            var btn = row.gameObject.AddComponent<Button>(); btn.targetGraphic = rb;
            var bc = btn.colors; bc.normalColor = Color.white; bc.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f); bc.pressedColor = new Color(0.92f, 0.92f, 0.96f, 1f); bc.fadeDuration = 0.09f; btn.colors = bc;
            var nm = e.Name; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast(nm + " — profile (coming soon)"); });

            FillRowCells(row, e.Rank, e.Name, e.Guild, e.Tier, e.Score, medal, false);
        }

        // Shared cell builder for both list rows and the pinned my-rank row.
        private void FillRowCells(RectTransform row, int rank, string name, string guild, LeagueTier tier, string score, bool medal, bool pinned)
        {
            // Convert absolute canvas-x column centres into row-local anchorX (row spans the ListArea width).
            float rankX  = LocalX(ColCenterX(Col.Rank));
            float playerStartX = LocalX(PlayerStartX());
            float leagueX = LocalX(ColCenterX(Col.League));
            float scoreX = LocalX(ColCenterX(Col.Score));

            // RANK cell — medallion (1-3) or plain numeral (4+) (§D/§F).
            if (medal)
            {
                Color hi, mid, sh;
                if (rank == 1) { hi = MedalGoldHi; mid = MedalGoldMid; sh = MedalGoldSh; }
                else if (rank == 2) { hi = MedalSilvHi; mid = MedalSilvMid; sh = MedalSilvSh; }
                else { hi = MedalBronHi; mid = MedalBronMid; sh = MedalBronSh; }
                var disk = UiWidgets.Rect("RankBadge_Medallion", row, new Vector2(rankX, 0.5f), new Vector2(rankX, 0.5f), Vector2.zero, new Vector2(58, 58));
                if (rank == 1) UiWidgets.Glow(disk, UiTheme.A(MedalGoldHi, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110, 110), 1.6f); // top-1 focal bloom (§J)
                var dImg = disk.gameObject.AddComponent<Image>(); dImg.raycastTarget = false; dImg.sprite = UiTex.VGradient(hi, mid, 64);
                // Rank-1: a slow rotating specular sweep over the disk (§J). A separate spinning child keeps the
                // numeral upright while the highlight rotates.
                if (rank == 1)
                {
                    var sweep = UiWidgets.Rect("Specular", disk, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(58, 58));
                    var swImg = sweep.gameObject.AddComponent<Image>(); swImg.raycastTarget = false;
                    swImg.sprite = UiTex.HGradient(UiTheme.A(Color.white, 0f), UiTheme.A(Color.white, 0.45f), 32);
                    sweep.gameObject.AddComponent<Spin>().degPerSec = 36f; // slow sweep
                }
                var dRing = UiWidgets.Rect("MedalRing", disk, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var drImg = dRing.gameObject.AddComponent<Image>(); drImg.raycastTarget = false; drImg.sprite = UiTex.Frame(hi, mid, sh, 64, 8); drImg.type = Image.Type.Sliced;
                var num = UiWidgets.Label(disk, rank.ToString(), 34, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, MedalNumeral);
                num.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(hi, 0.5f); // embossed relief (§F)
            }
            else
            {
                UiWidgets.Label(row, rank.ToString(), 32, new Vector2(rankX, 0.5f), new Vector2(rankX, 0.5f), Vector2.zero, new Vector2(120, 44), TextAnchor.MiddleCenter, Hex("#e9dcc0"));
            }

            // Avatar (circular portrait + gold ring; medallion rows slightly larger, §D).
            float avD = medal ? 56f : 50f;
            var avatar = UiWidgets.Rect("Avatar", row, new Vector2(playerStartX, 0.5f), new Vector2(playerStartX, 0.5f), Vector2.zero, new Vector2(avD, avD));
            var avImg = avatar.gameObject.AddComponent<Image>(); avImg.raycastTarget = false; avImg.sprite = UiTex.VGradient(Hex("#2a2d36"), Hex("#12141b"), 64);
            var avRing = UiWidgets.Rect("AvatarRing", avatar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var avrImg = avRing.gameObject.AddComponent<Image>(); avrImg.raycastTarget = false; avrImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, medal ? 6 : 4); avrImg.type = Image.Type.Sliced;
            var avGly = UiWidgets.Rect("AvatarGlyph", avatar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(avD * 0.5f, avD * 0.5f));
            var avgImg = avGly.gameObject.AddComponent<Image>(); avgImg.raycastTarget = false; avgImg.sprite = UiTex.Diamond(UiTheme.A(UiTheme.GoldHi, 0.55f), 32);

            // NameBlock — name (bold cream) over guild subtitle (grey), left-aligned starting after the avatar (§D/§F).
            float nameLeftX = playerStartX + 0.020f; // a touch right of the avatar
            UiWidgets.Label(row, name, 30, new Vector2(nameLeftX, 0.66f), new Vector2(nameLeftX, 0.66f), Vector2.zero, new Vector2(520, 36), TextAnchor.MiddleLeft, NameCol);
            UiWidgets.Label(row, guild, 22, new Vector2(nameLeftX, 0.34f), new Vector2(nameLeftX, 0.34f), Vector2.zero, new Vector2(520, 30), TextAnchor.MiddleLeft, GuildCol);

            // LeagueBadge — small tier icon + tier text (tier-coloured, §F), centred in the LEAGUE column.
            BuildLeagueBadge(row, leagueX, tier, pinned);

            // ScoreCell — score (gold tabular) + trophy icon, right-aligned in the SCORE column (§D/§F).
            UiWidgets.Label(row, score, 28, new Vector2(scoreX, 0.5f), new Vector2(scoreX, 0.5f), new Vector2(-30, 0), new Vector2(220, 40), TextAnchor.MiddleRight, ScoreCol);
            var trophy = UiWidgets.Rect("Icon_Trophy", row, new Vector2(scoreX, 0.5f), new Vector2(scoreX, 0.5f), new Vector2(8, 0), new Vector2(30, 30));
            UiWidgets.Glow(trophy, UiTheme.A(UiTheme.GoldHi, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56, 56), 1.7f);
            var trImg = trophy.gameObject.AddComponent<Image>(); trImg.raycastTarget = false; trImg.sprite = UiTex.Disc(UiTheme.Gold, 32);
        }

        private void BuildLeagueBadge(RectTransform row, float cx, LeagueTier tier, bool pinned)
        {
            string txt; Color col; Color glyphCol;
            switch (tier)
            {
                case LeagueTier.Legendary: txt = "LEGENDARY"; col = LeagueLeg;   glyphCol = UiTheme.AmethystHi; break;
                case LeagueTier.Champion:  txt = "CHAMPION";  col = LeagueChamp; glyphCol = Hex("#caa04a");     break;
                default:                   txt = pinned ? "DIAMOND II" : "DIAMOND"; col = LeagueDia; glyphCol = CobaltHi; break;
            }
            var badge = UiWidgets.Rect("LeagueBadge", row, new Vector2(cx, 0.5f), new Vector2(cx, 0.5f), Vector2.zero, new Vector2(220, 50));
            // Tier glyph (left): amethyst crystal = LEGENDARY; shield-ish diamond for CHAMPION/DIAMOND (§G/§N).
            var gly = UiWidgets.Rect("TierIcon", badge, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24, 0), new Vector2(34, 34));
            var glyImg = gly.gameObject.AddComponent<Image>(); glyImg.raycastTarget = false; glyImg.sprite = UiTex.Diamond(glyphCol, 48);
            var tierLbl = UiWidgets.Label(badge, UiTheme.Track(txt, 1), 22, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52, 0), new Vector2(220, 30), TextAnchor.MiddleLeft, col);
            tierLbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(col, 0.25f); // tier-coloured glow (§F)
        }

        // Row-local anchorX from an absolute canvas-x (the row stretches across the ListArea span).
        private float LocalX(float canvasX) => Mathf.Clamp01((canvasX - ListXMin) / (ListXMax - ListXMin));

        // ---- MyRankPinnedRow: same cells as a row, blue outline + brighter fill, never scrolls (§C/§D/§M). ----
        private void BuildPinnedRow()
        {
            if (_pinHost == null) return;
            for (int i = _pinHost.childCount - 1; i >= 0; i--) Object.Destroy(_pinHost.GetChild(i).gameObject);

            var me = MyByScope[Mathf.Clamp(_scope, 0, MyByScope.Length - 1)];

            // Brighter fill (slightly raised vs list rows) + breathing blue outline glow (§H/§J).
            var rb = _pinHost.gameObject.GetComponent<Image>(); if (rb == null) rb = _pinHost.gameObject.AddComponent<Image>();
            rb.sprite = UiTex.VGradient(Hex("#16223e"), Hex("#0d1322"), 32); rb.raycastTarget = true; rb.color = Color.white;
            var outline = UiWidgets.Rect("BlueOutline", _pinHost, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var olImg = outline.gameObject.AddComponent<Image>(); olImg.raycastTarget = false;
            olImg.sprite = UiTex.Frame(CobaltHi, Cobalt, UiWidgets.Darken(Cobalt, 0.3f), 48, 5); olImg.type = Image.Type.Sliced; // ~3px blue outline (§E)
            var olPulse = outline.gameObject.AddComponent<PulseGraphic>(); olPulse.target = olImg; olPulse.min = 0.7f; olPulse.max = 1f; olPulse.period = 2f; // gentle breathing (§J)
            var glow = UiWidgets.Glow(_pinHost, UiTheme.A(CobaltHi, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2((ListXMax - ListXMin) * CW + 40, 150), 1.8f);
            glow.transform.SetAsFirstSibling();

            // Pinned row is a sustained "selected" style; tap → own profile peek (display-only). Reuse the Button
            // across tab-switch rebuilds (clear listeners) so it never duplicates on the persistent host GameObject.
            var btn = _pinHost.gameObject.GetComponent<Button>(); if (btn == null) btn = _pinHost.gameObject.AddComponent<Button>();
            btn.targetGraphic = rb; btn.onClick.RemoveAllListeners();
            var bc = btn.colors; bc.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); bc.pressedColor = new Color(0.9f, 0.9f, 0.95f, 1f); bc.fadeDuration = 0.09f; btn.colors = bc;
            var meRank = me.Rank; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("Your rank: " + meRank + " (" + TabNames[_scope] + ")"); });

            int rankNum; if (!int.TryParse(me.Rank, out rankNum)) rankNum = 999; // pinned rank is always a plain numeral (>3)
            FillRowCells(_pinHost, rankNum, me.Name, me.Guild, me.Tier, me.Score, false, true);
        }

        // ---- FooterNote: fine print, bottom-centre of the board (§C/§E/§L — not prominent). ----
        private void BuildFooter()
        {
            float fy = 0.975f, ay = 1f - fy; // ~24px above frame bottom (§E)
            UiWidgets.Label(SafeContent, FooterText, 22, new Vector2(0.5f, ay), new Vector2(0.5f, ay), Vector2.zero, new Vector2(900, 30), TextAnchor.MiddleCenter, FooterCol);
        }

        // =====================================================================================================
        // Tab switch (display-only): re-style tabs, rebuild the list + pinned row, cross-fade/re-stagger (§I/§K).
        // =====================================================================================================
        private void OnSelectTab(int index)
        {
            if (index == _scope) return;
            _scope = Mathf.Clamp(index, 0, TabNames.Length - 1);
            StyleTabs();          // selected capsule → blue, others ghost
            PopulateList();       // re-query the scope's board (stub) + rebuild rows
            BuildPinnedRow();     // my-rank value may differ per scope (§K)
            StartCoroutine(CrossFadeList());
        }

        // Cross-fade + light re-stagger of the freshly rebuilt rows (§I tab-switch timeline).
        private IEnumerator CrossFadeList()
        {
            if (_listContent == null) yield break;
            var cg = _listContent.GetComponent<CanvasGroup>(); if (cg == null) cg = _listContent.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f; Vector2 home = _listContent.anchoredPosition; _listContent.anchoredPosition = home + new Vector2(0, -10f);
            float t = 0f; const float d = 0.16f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = k; _listContent.anchoredPosition = Vector2.Lerp(home + new Vector2(0, -10f), home, k); yield return null; }
            if (cg != null) cg.alpha = 1f; _listContent.anchoredPosition = home;
        }

        // =====================================================================================================
        // Lifecycle / enter timeline (§I) + local countdown tick (§I — no flashy digits, §L).
        // =====================================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterTimeline());
            StartCoroutine(CountdownTick());
        }

        // Board fade/pop → rows stagger-in (§I). The pinned row carries a continuous breathing blue glow (§J),
        // so it reads as "settled last" without a separate one-shot flash.
        private IEnumerator EnterTimeline()
        {
            // Board scale 0.985→1.0 + fade (~0.20s, ease-out-back lite).
            if (_boardCg != null)
            {
                _boardCg.alpha = 0f; var tr = _boardCg.transform; float t = 0f; const float d = 0.20f;
                while (t < d && _boardCg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _boardCg.alpha = k; tr.localScale = Vector3.one * Mathf.Lerp(0.985f, 1f, k); yield return null; }
                if (_boardCg != null) { _boardCg.alpha = 1f; tr.localScale = Vector3.one; }
            }
            // Rows stagger-in (α 0→1 + slide +12px→0, ~30ms apart).
            if (_listContent != null)
            {
                for (int i = 0; i < _listContent.childCount; i++)
                {
                    var child = _listContent.GetChild(i);
                    var cg = child.GetComponent<CanvasGroup>(); if (cg == null) continue;
                    StartCoroutine(RowIn(cg));
                    float s = 0f; while (s < 0.03f) { s += Time.unscaledDeltaTime; yield return null; }
                }
            }
        }

        private IEnumerator RowIn(CanvasGroup cg)
        {
            var rt = (RectTransform)cg.transform; Vector2 home = rt.anchoredPosition; Vector2 from = home + new Vector2(12f, 0f);
            cg.alpha = 0f; rt.anchoredPosition = from; float t = 0f; const float d = 0.16f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = k; rt.anchoredPosition = Vector2.Lerp(from, home, k); yield return null; }
            if (cg != null) { cg.alpha = 1f; rt.anchoredPosition = home; }
        }

        // Local cosmetic countdown (display-only; NOT server time, NOT gameplay). Recomputes the string each minute.
        private IEnumerator CountdownTick()
        {
            while (true)
            {
                float t = 0f; while (t < 60f) { t += Time.unscaledDeltaTime; yield return null; }
                _cdMins--; if (_cdMins < 0) { _cdMins = 59; _cdHours--; if (_cdHours < 0) { _cdHours = 23; _cdDays = Mathf.Max(0, _cdDays - 1); } }
                if (_countdownText != null) _countdownText.text = CountdownString();
            }
        }

        private string CountdownString() => _cdDays + "d " + _cdHours + "h " + _cdMins + "m"; // §M exact format "Nd Nh Nm"

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
