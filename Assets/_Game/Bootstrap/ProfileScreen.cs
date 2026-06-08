// BULWARK — PROFILE (UI Construction Bible · 24). Presentation-only, REMOVABLE.
//
// §12: NO ECS / NO Unity.Entities / NO gameplay-balance-AI-economy-backend. Pure code-built uGUI on the
// existing UiRouter shell. Every profile value (name THALRION, level 45, clan SILVERWARDENS, BATTLES/WINS/
// WIN RATE, equipped cosmetics, title, ID, join date) is a clearly DISPLAY-ONLY local stub (read-only / server-
// authoritative in production); nothing here mutates state. Forensic build of design/ProfileScreenDesign.png per
// 24_Profile_SPEC.md Sections A–O: the prestige player character-sheet — a 6-tab left rail (Overview selected,
// cobalt) + faction crest, a center identity column (ornate avatar frame, level-45 badge, XP bar, name, clan
// badge), and the right Overview pane (3 stat blocks, 5 Epic equipped slots, Title pill + ID/Joined footer).
// Buttons are display-only (Router.Toast); Back pops to Main Menu.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-24 landscape Player Profile (Overview tab). Presentation-only (all data display-only stub).</summary>
    public sealed class ProfileScreen : UiScreen
    {
        // ---- Exact per-screen hex (Section F/G) not already in UiTheme ----
        private static readonly Color StoneTop    = Hex("#14161e"); // BG stone-hall top
        private static readonly Color StoneBottom = Hex("#0a0b0f"); // BG obsidian base
        private static readonly Color SlateWell   = Hex("#101218"); // equipped-slot / panel well
        private static readonly Color SlateWellHi = Hex("#161922"); // slot well top-light
        private static readonly Color XpGroove    = Hex("#1b1d24"); // XP bar recessed groove
        private static readonly Color TitleGoldTop = Hex("#f0d27a"); // PROFILE/THALRION/REALM gradient top
        private static readonly Color TitleGoldBot = Hex("#caa04a"); // …gradient bottom
        private static readonly Color TabSelText  = Hex("#eaf2ff"); // selected tab label
        private static readonly Color TabIdleText = Hex("#c9b27a"); // idle tab label
        private static readonly Color LevelText   = Hex("#fff4d6"); // "45"
        private static readonly Color XpText      = Hex("#f0e8cf"); // "x,xxx / x,000 XP"
        private static readonly Color ClanName    = Hex("#9fc0ff"); // SILVERWARDENS
        private static readonly Color ClanMotto   = Hex("#cbb98a"); // "Knights of the Realm"
        private static readonly Color StatLabel   = Hex("#9a8a5a"); // BATTLES/WINS/WIN RATE + kickers/tags
        private static readonly Color StatValue   = Hex("#f4e9c8"); // 1,248 / 842 / 67.5%
        private static readonly Color SlotName    = Hex("#e8e2cf"); // item names
        private static readonly Color EpicViolet  = Hex("#c8a6ff"); // "Epic" rarity text
        private static readonly Color EpicEdge    = Hex("#9e6bf0"); // violet rarity edge/underglow
        private static readonly Color IdValue     = Hex("#cdd2da"); // #7A4B3C9E / 2024-01-15

        // ---- Display-only profile stub (NO gameplay/economy meaning; server-authoritative in production) ----
        private const string PlayerName = "THALRION";
        private const int    PlayerLevel = 45;
        private const string XpDisplay  = "4,250 / 6,000 XP"; // drawn "x,xxx / x,000 XP" placeholder (Section N)
        private const float  XpFraction = 4250f / 6000f;      // XP-bar fill (display-only)
        private const string ClanNameStr = "SILVERWARDENS";
        private const string ClanMottoStr = "Knights of the Realm";
        private const string PlayerId   = "#7A4B3C9E";
        private const string JoinedDate = "2024-01-15";
        private const string PlayerTitle = "REALM CHAMPION";

        // Exactly 6 tabs (Overview selected) — Section L: do not add/remove.
        private static readonly string[] Tabs =
            { "Overview", "Heroes", "Match History", "Stats", "Achievements", "Customization" };

        // The 3 Overview stat blocks (count-up targets are integers; win-rate counts tenths → "67.5%").
        private struct Stat { public string Label; public int Target; public bool Percent; public Color Icon; }
        private static readonly Stat[] Stats =
        {
            new Stat{ Label="BATTLES",  Target=1248, Percent=false, Icon=Hex("#caa04a") }, // crossed swords
            new Stat{ Label="WINS",     Target=842,  Percent=false, Icon=Hex("#f0d27a") }, // laurel/wreath
            new Stat{ Label="WIN RATE", Target=675,  Percent=true,  Icon=Hex("#e9c24a") }, // banner → "67.5%"
        };

        // Exactly 5 equipped slots, all "Epic" — Section L: exact item names, do not add/remove.
        private struct Slot { public string Name; public Color ArtTop, ArtBot; }
        private static readonly Slot[] EquippedSlots =
        {
            new Slot{ Name="Galvanhelm",      ArtTop=Hex("#d7dbe2"), ArtBot=Hex("#6e7682") }, // helm — steel
            new Slot{ Name="Lionheart Plate", ArtTop=Hex("#e6c878"), ArtBot=Hex("#9a7320") }, // armor — gold
            new Slot{ Name="Dawnbreaker",     ArtTop=Hex("#fff0c0"), ArtBot=Hex("#caa04a") }, // weapon — sword
            new Slot{ Name="Royal Cloak",     ArtTop=Hex("#4f8bff"), ArtBot=Hex("#2b3a6a") }, // cloak — cobalt
            new Slot{ Name="Warden's Banner", ArtTop=Hex("#6f9bff"), ArtBot=Hex("#2b56c8") }, // banner — iron
        };

        // Animated refs (wired in Build, driven in OnShow / Reveal coroutine).
        private CanvasGroup _bgCg, _headerCg, _railCg, _avatarCg, _nameCg, _footerCg;
        private readonly CanvasGroup[] _slotCg = new CanvasGroup[5];
        private readonly CountUp[] _statCounts = new CountUp[3];
        private Image _xpFill;

        protected override void Build()
        {
            // ============================== BG (full-bleed dark stone hall) + FX → Rect ==============================
            // Background art + vignette/glow parent under the full-bleed Rect (may extend under a notch cutout).
            var bg = UiWidgets.Backdrop(Rect, "profile");
            bg.color = UiTheme.A(Color.white, 0.55f);                                   // dim the daylight key-art toward a stone hall
            var grade = UiWidgets.Stretch("BG_Grade", Rect, UiTheme.A(StoneBottom, 0.62f)); // obsidian→stone wash
            grade.sprite = UiTex.VGradient(UiTheme.A(StoneTop, 0.55f), UiTheme.A(StoneBottom, 0.85f), 64);
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#caa04a"), 0.18f),                       // faint warm upper key (hall light)
                new Vector2(0.375f, 0.92f), new Vector2(0.375f, 0.92f), Vector2.zero, new Vector2(1200, 760), 1.6f);
            UiWidgets.Vignette(Rect, 0.6f);                                              // strong vignette (Section B/G)

            // A CanvasGroup over the whole BG so OnShow can fade it (0.20s, Section I @0.00).
            _bgCg = bg.gameObject.AddComponent<CanvasGroup>();

            // ============================== HEADER (y 0.91 → 1.00) ==============================
            var header = UiWidgets.Rect("Header", SafeContent, new Vector2(0, 0.91f), new Vector2(1, 1f), Vector2.zero, Vector2.zero);
            _headerCg = header.gameObject.AddComponent<CanvasGroup>();
            // Back (double-chevron) — top-left, pops to Main Menu (Section C/E/K).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            // "PROFILE" — centered x 0.50, baseline y≈0.955; serif gold-gradient UPPERCASE, wide tracking (59px@1080).
            UiWidgets.TitleLabel(header, "PROFILE", 59, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(900, 90), TextAnchor.MiddleCenter, TitleGoldTop, TitleGoldBot);

            // ============================== LEFT TAB RAIL ==============================
            BuildTabRail();

            // ============================== CENTER IDENTITY COLUMN ==============================
            BuildIdentityColumn();

            // ============================== RIGHT CONTENT PANE (Overview) ==============================
            BuildStatBlocks();
            BuildEquippedSection();
            BuildFooter();
        }

        // ---- Left rail: framed column x 0.016→0.215, y 0.060→0.900; 6 top-aligned tabs + faction crest at bottom. ----
        private void BuildTabRail()
        {
            var rail = UiWidgets.Rect("TabRail", SafeContent,
                new Vector2(0.016f, 1f - 0.900f), new Vector2(0.215f, 1f - 0.060f), Vector2.zero, Vector2.zero);
            _railCg = rail.gameObject.AddComponent<CanvasGroup>();
            // OrnateFrame returns the inner FIELD (its dark plate is drawn last) — parent rail content INTO it.
            var field = UiWidgets.OrnateFrame(rail, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UiTheme.FieldDark, false, 14f);

            int n = Tabs.Length;          // 6 rows top-aligned, each ≈0.085h (pitch ≈0.105h within the upper rail span)
            const float rowH = 0.115f;    // fraction of the rail height per tab cell (top region; crest lives below)
            const float gap = 0.018f;
            for (int i = 0; i < n; i++)
            {
                int idx = i;
                bool selected = i == 0; // Overview
                // Top-aligned within the rail field: row i center (from rail top) = top margin + (i+0.5)*rowH.
                float top = 0.035f + (i + 0.5f) * rowH;        // fraction from rail TOP
                float ay = 1f - top;                            // rail-local anchorY
                var cell = UiWidgets.Rect("Tab_" + Tabs[i].Replace(" ", ""), field,
                    new Vector2(0.5f, ay), new Vector2(0.5f, ay), Vector2.zero, Vector2.zero);
                cell.anchorMin = new Vector2(0.06f, ay - rowH * 0.5f + gap * 0.5f);
                cell.anchorMax = new Vector2(0.94f, ay + rowH * 0.5f - gap * 0.5f);
                cell.offsetMin = Vector2.zero; cell.offsetMax = Vector2.zero;

                // Pill: selected = cobalt fill; idle = transparent dark (Section H).
                var pill = cell.gameObject.AddComponent<Image>();
                if (selected) { pill.sprite = UiTex.VGradient(UiTheme.IronBlueHi, UiTheme.IronBlue, 32); pill.color = Color.white; }
                else { pill.color = UiTheme.A(UiTheme.Charcoal, 0.55f); }
                var btn = cell.gameObject.AddComponent<Button>(); btn.targetGraphic = pill;
                var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.9f, 0.9f, 0.94f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
                string label = Tabs[i];
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); if (idx != 0) Router.Toast(label + " — coming soon"); });

                if (selected)
                {
                    // Gold rim + left active marker (Section H).
                    var rim = UiWidgets.Rect("Rim", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var rimg = rim.gameObject.AddComponent<Image>();
                    rimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;
                    var marker = UiWidgets.Rect("ActiveMarker", cell, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(8, 0));
                    marker.anchorMin = new Vector2(0, 0.2f); marker.anchorMax = new Vector2(0, 0.8f); marker.offsetMin = new Vector2(4, 0); marker.offsetMax = new Vector2(12, 0);
                    var mimg = marker.gameObject.AddComponent<Image>(); mimg.color = UiTheme.GoldHi; mimg.raycastTarget = false;
                }

                // Icon disc (left) + label.
                var icon = UiWidgets.Rect("Icon", cell, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(42, 0), new Vector2(44, 44));
                var iimg = icon.gameObject.AddComponent<Image>(); iimg.raycastTarget = false;
                iimg.sprite = UiTex.Diamond(selected ? UiTheme.GoldHi : UiTheme.A(UiTheme.Gold, 0.8f), 32);
                UiWidgets.Label(cell, Tabs[i], 24, new Vector2(0, 0), new Vector2(1, 1), new Vector2(40, 0), new Vector2(-40, 0),
                    TextAnchor.MiddleLeft, selected ? TabSelText : TabIdleText);
            }

            // Faction crest (Iron Pact) at rail bottom — centered, Ø≈0.085w, y≈0.085 → near rail base.
            var crest = UiWidgets.Rect("Crest_Faction", field, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 90), new Vector2(150, 150));
            UiWidgets.Glow(crest, UiTheme.A(UiTheme.IronBlueHi, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(190, 190), 1.6f);
            var crestImg = crest.gameObject.AddComponent<Image>(); crestImg.raycastTarget = false;
            crestImg.sprite = UiTex.Diamond(UiTheme.IronBlue, 48);
            var crestRing = UiWidgets.Rect("CrestRing", crest, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var crImg = crestRing.gameObject.AddComponent<Image>(); crImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); crImg.type = Image.Type.Sliced; crImg.raycastTarget = false;
        }

        // ---- Center identity column: x 0.230→0.520, centered. Avatar frame → level badge → XP bar → name → clan. ----
        private void BuildIdentityColumn()
        {
            var col = UiWidgets.Rect("IdentityColumn", SafeContent,
                new Vector2(0.230f, 0f), new Vector2(0.520f, 1f), Vector2.zero, Vector2.zero);
            _avatarCg = col.gameObject.AddComponent<CanvasGroup>();

            // Avatar frame — centered x 0.375 (col-local 0.5), top y≈0.880; frame ≈433×454@1080. Ornate gold frame.
            var frameRect = UiWidgets.Rect("Avatar_Frame", col, new Vector2(0.5f, 1f - 0.880f - 0.210f), new Vector2(0.5f, 1f - 0.880f), Vector2.zero, new Vector2(0, 0));
            // Anchor the frame by its center at fy≈0.670 (midpoint of 0.880 top over a 0.420h frame).
            float frameCY = 1f - (0.880f - 0.420f * 0.5f);
            frameRect.anchorMin = new Vector2(0.5f, frameCY); frameRect.anchorMax = new Vector2(0.5f, frameCY);
            frameRect.sizeDelta = new Vector2(433, 454); frameRect.anchoredPosition = Vector2.zero;
            // Backlight bloom behind the portrait (Section J volumetric backlight).
            UiWidgets.Glow(frameRect, UiTheme.A(Hex("#f4dca0"), 0.30f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(520, 540), 1.5f);
            var field = UiWidgets.OrnateFrame(frameRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Hex("#0c0d12"), true, 24f);
            // Masked hero render placeholder (rim-lit gold/cobalt) — inset ~0.07·frame.
            var portrait = UiWidgets.Rect("Avatar_Portrait", field, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 0));
            portrait.anchorMin = new Vector2(0.07f, 0.07f); portrait.anchorMax = new Vector2(0.93f, 0.93f); portrait.offsetMin = Vector2.zero; portrait.offsetMax = Vector2.zero;
            var pimg = portrait.gameObject.AddComponent<Image>(); pimg.raycastTarget = false;
            pimg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.IronBlueHi, 0.55f), UiTheme.A(Hex("#1a1206"), 0.95f), 64);
            // Faint drifting dust motes over the portrait (Section J).
            var motes = UiWidgets.Rect("Motes", field, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 420));
            var ef = motes.gameObject.AddComponent<EmberField>(); ef.count = 6; ef.color = Hex("#f4dca0");

            // Level badge "45" — at frame base center, y≈0.470; gold ring medallion, Ø≈140px.
            var badge = UiWidgets.Rect("Badge_Level", col, new Vector2(0.5f, 1f - 0.470f), new Vector2(0.5f, 1f - 0.470f), Vector2.zero, new Vector2(140, 140));
            UiWidgets.Glow(badge, UiTheme.A(UiTheme.GoldHi, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 200), 1.6f);
            var bimg = badge.gameObject.AddComponent<Image>(); bimg.raycastTarget = false; bimg.sprite = UiTex.Disc(Hex("#15110a"), 64);
            var bring = UiWidgets.Rect("BadgeRing", badge, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var brimg = bring.gameObject.AddComponent<Image>(); brimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); brimg.type = Image.Type.Sliced; brimg.raycastTarget = false;
            var lvl = UiWidgets.Label(badge, PlayerLevel.ToString(), 48, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, LevelText);
            lvl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
            badge.gameObject.AddComponent<PulseScale>().period = 2.4f; // soft gold-glow pulse beat (Section J)

            // XP bar — x 0.270→0.480 (col-local), y≈0.430, cobalt fill, centered "x,xxx / x,000 XP".
            var xpRect = UiWidgets.Rect("XPBar", col, new Vector2(0.14f, 1f - 0.430f - 0.012f), new Vector2(0.86f, 1f - 0.430f + 0.012f), Vector2.zero, Vector2.zero);
            var groove = xpRect.gameObject.AddComponent<Image>(); groove.raycastTarget = false; groove.sprite = UiTex.VGradient(XpGroove, Hex("#0e1016"), 32);
            var xpFillRect = UiWidgets.Rect("XP_Fill", xpRect, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-6, -6));
            _xpFill = xpFillRect.gameObject.AddComponent<Image>();
            _xpFill.sprite = UiTex.VGradient(UiTheme.IronBlueHi, UiTheme.IronBlue, 32);
            _xpFill.type = Image.Type.Filled; _xpFill.fillMethod = Image.FillMethod.Horizontal; _xpFill.fillOrigin = 0; _xpFill.fillAmount = 0f; _xpFill.raycastTarget = false;
            UiWidgets.Label(xpRect, XpDisplay, 20, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, XpText);

            // Player name "THALRION" — centered, baseline y≈0.380; large serif gold-gradient cap.
            var nameRect = UiWidgets.Rect("NameGroup", col, new Vector2(0.5f, 1f - 0.380f), new Vector2(0.5f, 1f - 0.380f), Vector2.zero, new Vector2(640, 90));
            _nameCg = nameRect.gameObject.AddComponent<CanvasGroup>();
            UiWidgets.TitleLabel(nameRect, PlayerName, 59, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, TitleGoldTop, TitleGoldBot);

            // Clan badge — centered y≈0.300: crest (left) + "SILVERWARDENS" over "Knights of the Realm" on a dark pill.
            var clan = UiWidgets.Rect("ClanBadge", nameRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -120), new Vector2(440, 84));
            var clanBg = clan.gameObject.AddComponent<Image>(); clanBg.sprite = UiTex.VGradient(SlateWellHi, SlateWell, 32); clanBg.color = UiTheme.A(Color.white, 0.92f); clanBg.raycastTarget = false;
            var clanRim = UiWidgets.Rect("ClanRim", clan, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var clanRimg = clanRim.gameObject.AddComponent<Image>(); clanRimg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.8f), UiTheme.A(UiTheme.Gold, 0.7f), UiTheme.A(UiTheme.GoldShadow, 0.7f), 48, 5); clanRimg.type = Image.Type.Sliced; clanRimg.raycastTarget = false;
            var clanCrest = UiWidgets.Rect("Clan_Crest", clan, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 0), new Vector2(54, 54));
            var ccImg = clanCrest.gameObject.AddComponent<Image>(); ccImg.raycastTarget = false; ccImg.sprite = UiTex.Diamond(UiTheme.IronBlueHi, 48);
            UiWidgets.Label(clan, ClanNameStr, 26, new Vector2(0, 0.5f), new Vector2(1, 1f), new Vector2(90, -4), new Vector2(-20, 0), TextAnchor.LowerLeft, ClanName);
            UiWidgets.Label(clan, ClanMottoStr, 18, new Vector2(0, 0f), new Vector2(1, 0.5f), new Vector2(90, 4), new Vector2(-20, 0), TextAnchor.UpperLeft, ClanMotto);
        }

        // ---- Right Overview pane: x 0.535→0.984. Top band = 3 stat blocks (y 0.770→0.890). ----
        private void BuildStatBlocks()
        {
            const float paneL = 0.535f, paneR = 0.984f, paneW = paneR - paneL;     // 0.449w
            const float gap = 0.014f;
            float blockW = (paneW - 2f * gap) / 3f;                                 // 3 equal blocks
            for (int i = 0; i < Stats.Length; i++)
            {
                float l = paneL + i * (blockW + gap);
                var block = UiWidgets.Rect("Stat_" + Stats[i].Label.Replace(" ", ""), SafeContent,
                    new Vector2(l, 1f - 0.890f), new Vector2(l + blockW, 1f - 0.770f), Vector2.zero, Vector2.zero);

                // Gold stat icon (top, Ø≈0.040w ≈94px).
                var icon = UiWidgets.Rect("Icon", block, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero, new Vector2(72, 72));
                var iimg = icon.gameObject.AddComponent<Image>(); iimg.raycastTarget = false; iimg.sprite = UiTex.Diamond(Stats[i].Icon, 48);

                // Label ("BATTLES" / "WINS" / "WIN RATE") — 18px dim gold.
                UiWidgets.Label(block, UiTheme.Track(Stats[i].Label), 18, new Vector2(0, 0.42f), new Vector2(1, 0.56f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, StatLabel);

                // Big value (count-up target) — 40px tabular gold.
                var val = UiWidgets.Label(block, Format(0, Stats[i].Percent), 40, new Vector2(0, 0.05f), new Vector2(1, 0.42f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, StatValue);
                val.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.8f);
                bool pct = Stats[i].Percent;
                var cu = val.gameObject.AddComponent<CountUp>(); cu.Bind(val, v => Format(v, pct), 0f);
                _statCounts[i] = cu;

                // Thin gold divider between blocks (Section G).
                if (i < Stats.Length - 1)
                    UiWidgets.Divider(SafeContent, new Vector2(l + blockW + gap * 0.5f, 1f - 0.830f), new Vector2(l + blockW + gap * 0.5f, 1f - 0.830f), Vector2.zero, 3, 96, UiTheme.A(UiTheme.Gold, 0.7f));
            }
        }

        // ---- "Equipped" title (y≈0.700) + 5 square Epic slots (band y 0.500→0.680). ----
        private void BuildEquippedSection()
        {
            const float paneL = 0.535f, paneR = 0.984f, paneW = paneR - paneL;     // 0.449w
            const float gap = 0.014f;
            float slotW = (paneW - 4f * gap) / 5f;                                  // 0.0786w ≈184px@1080

            // "Equipped" title — centered in pane at y≈0.700, 24px gold.
            float paneCx = (paneL + paneR) * 0.5f;
            UiWidgets.Label(SafeContent, UiTheme.Track("Equipped", 1), 24,
                new Vector2(paneCx, 1f - 0.700f), new Vector2(paneCx, 1f - 0.700f), Vector2.zero, new Vector2(360, 40), TextAnchor.MiddleCenter, UiTheme.Gold);

            for (int i = 0; i < EquippedSlots.Length; i++)
            {
                float l = paneL + i * (slotW + gap);
                var slot = UiWidgets.Rect("Slot_" + EquippedSlots[i].Name.Replace(" ", "").Replace("'", ""), SafeContent,
                    new Vector2(l, 1f - 0.680f), new Vector2(l + slotW, 1f - 0.500f), Vector2.zero, Vector2.zero);
                _slotCg[i] = slot.gameObject.AddComponent<CanvasGroup>();

                // Art well (top square ≈162px@1080) — dark slate with gold rim + violet Epic edge.
                var well = UiWidgets.Rect("Well", slot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -84), new Vector2(0, 0));
                well.anchorMin = new Vector2(0.04f, 0.34f); well.anchorMax = new Vector2(0.96f, 1f); well.offsetMin = Vector2.zero; well.offsetMax = Vector2.zero;
                var wbg = well.gameObject.AddComponent<Image>(); wbg.sprite = UiTex.VGradient(SlateWellHi, SlateWell, 32);
                var wbtn = well.gameObject.AddComponent<Button>(); wbtn.targetGraphic = wbg;
                var cb = wbtn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.96f, 0.96f, 0.96f, 1f); cb.fadeDuration = 0.08f; wbtn.colors = cb;
                string item = EquippedSlots[i].Name;
                wbtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast(item + " — Customization coming soon"); });

                // Violet (Epic) underglow shimmer (Section J).
                var glow = UiWidgets.Glow(well, UiTheme.A(EpicEdge, 0.40f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 200), 1.7f);
                glow.transform.SetAsFirstSibling();
                var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.22f; pg.max = 0.45f; pg.period = 2.6f;

                // Item render placeholder (lit).
                var art = UiWidgets.Rect("Item", well, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 0));
                art.anchorMin = new Vector2(0.18f, 0.18f); art.anchorMax = new Vector2(0.82f, 0.82f); art.offsetMin = Vector2.zero; art.offsetMax = Vector2.zero;
                var aimg = art.gameObject.AddComponent<Image>(); aimg.raycastTarget = false; aimg.sprite = UiTex.VGradient(EquippedSlots[i].ArtTop, EquippedSlots[i].ArtBot, 64);

                // Gold rim over the well edge.
                var rim = UiWidgets.Rect("Rim", well, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var rimg = rim.gameObject.AddComponent<Image>(); rimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;
                // Thin violet rarity edge inside the gold rim.
                var vEdge = UiWidgets.Rect("EpicEdge", well, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -10));
                var vImg = vEdge.gameObject.AddComponent<Image>(); vImg.sprite = UiTex.Frame(UiTheme.A(EpicEdge, 0.9f), UiTheme.A(EpicEdge, 0.6f), UiTheme.A(EpicEdge, 0.3f), 48, 3); vImg.type = Image.Type.Sliced; vImg.raycastTarget = false;

                // Item name (18px) + "Epic" rarity (16px violet) below the well.
                UiWidgets.Label(slot, EquippedSlots[i].Name, 18, new Vector2(0, 0.16f), new Vector2(1, 0.30f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, SlotName);
                UiWidgets.Label(slot, "Epic", 16, new Vector2(0, 0.02f), new Vector2(1, 0.16f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, EpicViolet);
            }
        }

        // ---- Footer (y 0.110→0.260): centered Title pill + right ID/Joined metadata cluster. ----
        private void BuildFooter()
        {
            const float paneL = 0.535f, paneR = 0.984f;
            float paneCx = (paneL + paneR) * 0.5f;
            var footer = UiWidgets.Rect("FooterRow", SafeContent,
                new Vector2(paneL, 1f - 0.260f), new Vector2(paneR, 1f - 0.110f), Vector2.zero, Vector2.zero);
            _footerCg = footer.gameObject.AddComponent<CanvasGroup>();

            // Title pill — centered horizontally in the pane, y≈0.230; parchment/gold pill ≈0.30w×0.075h.
            float pillCxLocal = (paneCx - paneL) / (paneR - paneL);                 // map pane-Cx to footer-local
            var pill = UiWidgets.Rect("TitlePanel", footer, new Vector2(pillCxLocal, 0.78f), new Vector2(pillCxLocal, 0.78f), Vector2.zero, new Vector2(0.30f * 2340f, 0.075f * 1080f));
            UiWidgets.Glow(pill, UiTheme.A(UiTheme.GoldHi, 0.30f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.30f * 2340f + 120f, 0.075f * 1080f + 80f), 1.6f);
            var pillBg = pill.gameObject.AddComponent<Image>(); pillBg.sprite = UiTex.VGradient(Hex("#2a2113"), Hex("#15110a"), 32); pillBg.raycastTarget = false;
            var pillRim = UiWidgets.Rect("PillRim", pill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var prImg = pillRim.gameObject.AddComponent<Image>(); prImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); prImg.type = Image.Type.Sliced; prImg.raycastTarget = false;
            UiWidgets.Label(pill, UiTheme.Track("Title", 1), 18, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -14), new Vector2(300, 28), TextAnchor.MiddleCenter, StatLabel);
            UiWidgets.TitleLabel(pill, PlayerTitle, 30, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(0.30f * 2340f - 40f, 50), TextAnchor.MiddleCenter, TitleGoldTop, TitleGoldTop);

            // Right metadata cluster: "Player ID" + "#7A4B3C9E" + copy button (upper), "Joined" + "2024-01-15" (lower).
            // Player ID tag.
            UiWidgets.Label(footer, "Player ID", 18, new Vector2(1, 0.62f), new Vector2(1, 0.62f), new Vector2(-360, 0), new Vector2(220, 30), TextAnchor.MiddleRight, StatLabel);
            // Player ID value (26px mono-ish tabular).
            UiWidgets.Label(footer, PlayerId, 26, new Vector2(1, 0.62f), new Vector2(1, 0.62f), new Vector2(-150, 0), new Vector2(200, 36), TextAnchor.MiddleRight, IdValue);
            // Copy button (small gold/steel icon button; font-safe "COPY" stand-in) → display-only "Copied!" toast.
            UiWidgets.Button(footer, "COPY", new Vector2(1, 0.62f), new Vector2(1, 0.62f), new Vector2(-55, 0), new Vector2(90, 50), UiTheme.A(UiTheme.GoldShadow, 0.9f),
                () => Router.Toast("Copied! " + PlayerId), 22);
            // Joined tag + value (below).
            UiWidgets.Label(footer, "Joined", 18, new Vector2(1, 0.24f), new Vector2(1, 0.24f), new Vector2(-360, 0), new Vector2(220, 30), TextAnchor.MiddleRight, StatLabel);
            UiWidgets.Label(footer, JoinedDate, 22, new Vector2(1, 0.24f), new Vector2(1, 0.24f), new Vector2(-150, 0), new Vector2(220, 34), TextAnchor.MiddleRight, IdValue);
        }

        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Reveal()); // enter timeline (Section I): cascade + stat count-up + XP fill + slot stagger.
        }

        // Enter timeline (~0.55s, Section I): BG fade → header → rail → avatar → name → stat count-up → slots → footer.
        private IEnumerator Reveal()
        {
            // Start everything hidden (so the cascade reads), then fade groups in on unscaled time.
            SetAlpha(_bgCg, 0f); SetAlpha(_headerCg, 0f); SetAlpha(_railCg, 0f);
            SetAlpha(_avatarCg, 0f); SetAlpha(_nameCg, 0f); SetAlpha(_footerCg, 0f);
            for (int i = 0; i < _slotCg.Length; i++) { if (_slotCg[i] != null) { SetAlpha(_slotCg[i], 0f); _slotCg[i].transform.localScale = Vector3.one * 0.92f; } }

            yield return FadeGroup(_bgCg, 0.20f);                 // 0.00s BG + vignette
            yield return Wait(0.05f);
            StartCoroutine(FadeGroup(_headerCg, 0.22f));          // 0.05s header
            yield return Wait(0.05f);
            StartCoroutine(FadeGroup(_railCg, 0.20f));            // 0.10s rail slides/fades in
            yield return Wait(0.04f);
            StartCoroutine(PopGroup(_avatarCg, 0.94f, 0.25f));    // 0.14s avatar scale + fade (ease-out-back)
            yield return Wait(0.10f);
            // 0.24s XP bar fills left→right to current (0.40s).
            StartCoroutine(FillXp(0.40f));
            yield return Wait(0.04f);
            StartCoroutine(FadeGroup(_nameCg, 0.18f));            // 0.28s name + clan
            yield return Wait(0.02f);
            for (int i = 0; i < _statCounts.Length; i++)          // 0.30s stat count-up to values (~0.5s)
                _statCounts[i]?.To(Stats[i].Target, 0.5f);
            yield return Wait(0.06f);
            for (int i = 0; i < _slotCg.Length; i++)              // 0.36s equipped slots stagger left→right (0.04s each)
            {
                StartCoroutine(PopGroup(_slotCg[i], 0.92f, 0.18f));
                yield return Wait(0.04f);
            }
            yield return Wait(0.06f);
            StartCoroutine(FadeGroup(_footerCg, 0.18f));          // 0.46s footer
        }

        // ---- Small unscaled-time tween helpers (mirror EndScreen/result-screen Reveal idioms) ----
        private static void SetAlpha(CanvasGroup cg, float a) { if (cg != null) cg.alpha = a; }

        private IEnumerator FadeGroup(CanvasGroup cg, float d)
        {
            if (cg == null) yield break;
            float t = 0f; cg.alpha = Mathf.Max(cg.alpha, 0f);
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(t / d); yield return null; }
            if (cg != null) cg.alpha = 1f;
        }

        private IEnumerator PopGroup(CanvasGroup cg, float from, float d)
        {
            if (cg == null) yield break;
            var tr = cg.transform; float t = 0f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = k; tr.localScale = Vector3.one * Mathf.Lerp(from, 1f, EaseOutBack(k)); yield return null; }
            if (cg != null) { cg.alpha = 1f; tr.localScale = Vector3.one; }
        }

        private IEnumerator FillXp(float d)
        {
            if (_xpFill == null) yield break;
            float t = 0f;
            while (t < d && _xpFill != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _xpFill.fillAmount = Mathf.Lerp(0f, XpFraction, k); yield return null; }
            if (_xpFill != null) _xpFill.fillAmount = XpFraction;
        }

        private IEnumerator Wait(float seconds)
        {
            float t = 0f; while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }

        private static float EaseOutBack(float k)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float p = k - 1f; return 1f + c3 * p * p * p + c1 * p * p;
        }

        // Format a stat value: percent stats store tenths (675 → "67.5%"); plain stats use grouped thousands.
        private static string Format(int v, bool percent)
            => percent ? (v / 10) + "." + (v % 10) + "%" : v.ToString("N0");

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
