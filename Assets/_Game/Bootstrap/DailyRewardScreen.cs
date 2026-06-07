// BULWARK — DAILY REWARD (UI Construction Bible · 28). Presentation-only, REMOVABLE.
//
// §12 boundary: pure presentation. NO ECS / Unity.Entities, NO gameplay / balance / AI / economy / backend.
// The 7-day login-streak, per-day claim states, and streak count are DISPLAY-ONLY local stub data (server-
// authoritative in the real flow — the client never computes rewards or streak). CLAIM is a display-only
// affordance (Router.Toast + visual "claimed" mark + UiStub.GrantGems/GrantGold for the day's stub currency);
// it never writes a real balance. The inherited "5R/120" Energy chip is canon-CUT (Section L) — omitted here.
// Deleting this file removes the screen 100%.
//
// Forensic build of design/DailyRewardDesign.png per Construction-Bible 28 (Sections A–O): a modal ornate
// near-black plate floating over a dimmed hub backdrop (scrim + vignette + god-ray cone, low-alpha hub bleed);
// a gold double-frame + top crest + ✕ close; "DAILY REWARD" gold-bevel title + subtitle; a single-row 7-cell
// streak calendar (Days 1–2 CLAIMED ✓ · Day 3 CLAIMABLE with a pulsing gold halo · Days 4–7 LOCKED 🔒, Day 7
// VIOLET/premium "EPIC CHEST + Legendary Unit Guaranteed!"); and a footer strip (flame + "STREAK: 3 DAYS" +
// sub line + gold CLAIM pill). Entry/idle/claim animations via UiFx + local coroutines (Sections I/J).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-28 landscape Daily Reward login-streak modal. Presentation-only (stub streak; server-auth meta, §12).</summary>
    public sealed class DailyRewardScreen : UiScreen
    {
        // Reference canvas (CanvasScaler 2340×1080, match height). Layout math is fractions of these (Section E);
        // Section-E fy values are measured from the TOP → converted to Unity anchorY via (1f − fy).
        private const float W = 2340f, H = 1080f;

        // ---- Screen-local display-only palette (Section B/G hex not in UiTheme). Cosmetic only; no economy. ----
        private static readonly Color CPanelTop   = Hex("#161a24"); // obsidian plate top
        private static readonly Color CPanelBot   = Hex("#0c0e15"); // obsidian plate base
        private static readonly Color CCellTop    = Hex("#1b2030"); // standard day-cell slate top
        private static readonly Color CCellBot    = Hex("#11141d"); // standard day-cell slate base
        private static readonly Color CCellBronze = Hex("#7a5f28"); // thin bronze cell edge
        private static readonly Color CClaimTop   = Hex("#1a2336"); // claimable Day-3 brighter body
        private static readonly Color CClaimBot   = Hex("#12172a"); // claimable Day-3 base
        private static readonly Color CHalo       = Hex("#ffd16a"); // additive gold halo ring
        private static readonly Color CCobalt     = Hex("#2b56c8"); // blue crystal / cobalt inner glow
        private static readonly Color CCobaltHi   = Hex("#4f8bff"); // bright cobalt
        private static readonly Color CVioTop     = Hex("#2a1c52"); // premium Day-7 body top
        private static readonly Color CVioBot     = Hex("#1a1130"); // premium Day-7 body base
        private static readonly Color CAmethyst   = Hex("#5a2db0"); // amethyst frame base
        private static readonly Color CAmethystHi = Hex("#9e6bf0"); // amethyst frame highlight
        private static readonly Color CMagicGlow  = Hex("#b07bff"); // Day-7 inner magic glow
        private static readonly Color CDiscBody   = Hex("#14161e"); // state-disc body
        private static readonly Color CCheck      = Hex("#f0d27a"); // ✓ gold engrave
        private static readonly Color CLockSteel  = Hex("#8a8f9c"); // 🔒 desaturated steel
        private static readonly Color CSilver     = Hex("#c8ccd6"); // silver bars
        private static readonly Color CChestWood  = Hex("#a05b25"); // chest aged wood
        private static readonly Color CChestBand  = Hex("#9aa0a8"); // chest bronze/steel band
        private static readonly Color CDayHdr     = Hex("#e8dcc0"); // "DAY n" header text
        private static readonly Color CDayHdrGold = Hex("#f0d27a"); // Day-7 gold header
        private static readonly Color CAmount     = Hex("#ffffff"); // day amount
        private static readonly Color CAmountHi   = Hex("#eaf2ff"); // Day-3 amount (cool glow)
        private static readonly Color CEpic       = Hex("#c9a6ff"); // "EPIC CHEST" violet small-caps
        private static readonly Color CEpicSub    = Hex("#b9a7d8"); // Day-7 sub line
        private static readonly Color CSubtitle   = Hex("#d9c79a"); // subtitle
        private static readonly Color CStreakLbl  = Hex("#d9c79a"); // "STREAK:" label
        private static readonly Color CStreakVal  = Hex("#ffd76a"); // "3 DAYS" value
        private static readonly Color CStreakSub  = Hex("#9a937f"); // streak sub line
        private static readonly Color CFlame      = Hex("#ffb04a"); // flame ember core
        private static readonly Color CClaimGrey  = Hex("#7d7355"); // disabled CLAIM grey-gold
        private static readonly Color CClaimLbl   = Hex("#2a1c06"); // CLAIM label (on gold)
        private static readonly Color CTitleTop   = Hex("#f0d27a"); // title gradient top
        private static readonly Color CTitleBot   = Hex("#caa04a"); // title gradient bottom

        // ---- Per-day reward state (display only — drives icon/colour/state; NO economy/balance meaning, §12). ----
        private enum DayState { Claimed, Claimable, Locked }
        private enum DayIcon  { Gold, Silver, Crystal, Chest, Sigil, Gems, EpicChest }
        // Stub currency the day grants (display-only mark on claim; never a server write). None = no chip nudge.
        private enum Grant    { None, Gold, Gems }

        private struct Day
        {
            public string Header;   // "DAY n"
            public DayIcon Icon;
            public string Amount;   // exact drawn value (Section L)
            public DayState State;
            public Grant   Give;    // display-only currency stub the cell depicts
            public int     GiveN;   // stub amount for the GrantGems/GrantGold display nudge
            public bool    Premium; // Day 7 — violet identity (load-bearing, Section L)
        }

        // Section-L EXACT drawn values: 5,000 / 10,000 / 100 / 1 / 50 / 200 / EPIC CHEST. Day 3 is today/claimable.
        private static readonly Day[] Days =
        {
            new Day{ Header="DAY 1", Icon=DayIcon.Gold,      Amount="5,000",  State=DayState.Claimed,   Give=Grant.Gold, GiveN=5000 },
            new Day{ Header="DAY 2", Icon=DayIcon.Silver,    Amount="10,000", State=DayState.Claimed,   Give=Grant.Gold, GiveN=10000 },
            new Day{ Header="DAY 3", Icon=DayIcon.Crystal,   Amount="100",    State=DayState.Claimable, Give=Grant.Gems, GiveN=100 },
            new Day{ Header="DAY 4", Icon=DayIcon.Chest,     Amount="1",      State=DayState.Locked,    Give=Grant.None },
            new Day{ Header="DAY 5", Icon=DayIcon.Sigil,     Amount="50",     State=DayState.Locked,    Give=Grant.Gems, GiveN=50 },
            new Day{ Header="DAY 6", Icon=DayIcon.Gems,      Amount="200",    State=DayState.Locked,    Give=Grant.Gems, GiveN=200 },
            new Day{ Header="DAY 7", Icon=DayIcon.EpicChest, Amount="",       State=DayState.Locked,    Give=Grant.None, Premium=true },
        };
        private const int StreakDays      = 3;   // STREAK: 3 DAYS (Section L, display only)
        private const int ClaimableIndex  = 2;   // Day 3 is today/claimable

        // ---- Panel geometry (Section E). Panel ≈ 0.80W × 0.78H, centered; molding ≈ 0.012H. ----
        private const float PanelW   = 0.80f * W;  // 1872
        private const float PanelH   = 0.78f * H;  // 842.4
        private const float Molding  = 14f;        // ≈ 0.012H cast-gold frame thickness

        // ---- Live handles for the entry/idle/claim timeline (Section I). ----
        private CanvasGroup _panelCg;
        private RectTransform _crest;
        private RectTransform[] _cellRt;
        private CanvasGroup[]  _cellCg;
        private RectTransform _haloRt;        // Day-3 pulsing halo (idle loop after entry)
        private RectTransform _footerRt;
        private CanvasGroup _footerCg;
        private Text _streakValue;
        private RectTransform _claimRt;
        private CanvasGroup _claimCg;
        private Button _claimBtn;
        private Image _claimFill;
        private Text _claimLabel;
        private bool _claimed;

        protected override void Build()
        {
            _cellRt = new RectTransform[Days.Length];
            _cellCg = new CanvasGroup[Days.Length];

            // ============================================================================================
            // FULL-BLEED BACKDROP + SCRIM + GOD-RAY (Rect; OUTSIDE safe area → under the cutout). Section C/D.
            // ============================================================================================
            // FullBleedBackdrop: a static hub snapshot stand-in (the live hub bleeds through dimmed).
            UiWidgets.Stretch("FullBleedBackdrop", Rect, UiTheme.Charcoal, "bg_menu");
            // DimScrim: near-black ~70% over the hub (Section B/D).
            UiWidgets.Stretch("DimScrim", Rect, new Color(0f, 0f, 0f, 0.70f));
            // Vignette: radial → dark edges (Section D/J).
            UiWidgets.Vignette(Rect, 0.6f);
            // GodRayCone: soft warm additive cone descending behind the panel top (Section B/D/J).
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#f4dca0"), 0.22f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40f), new Vector2(1100f, 900f), 1.5f);
            // Faint focal glow behind the central highlighted day (Section B).
            UiWidgets.Glow(Rect, UiTheme.A(CHalo, 0.16f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 500f), 1.8f);
            // Very low-alpha dust motes drifting in the dark field (Section J).
            var moteHost = UiWidgets.Rect("FX_DustMotes", Rect, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
            var mef = moteHost.gameObject.AddComponent<EmberField>(); mef.count = 14; mef.color = UiTheme.A(Hex("#cbb98a"), 0.5f);

            // ============================================================================================
            // INHERITED HUB CHROME (low-alpha, NON-interactive bleed; Section C/L). raycast OFF, α≈.22.
            // Forensic record only — NOT functional while this modal is open (Section L negative rule).
            // The Energy "5R/120" chip is canon-CUT (Section L) → intentionally NOT drawn.
            // ============================================================================================
            BuildHubBleed();

            // ============================================================================================
            // REWARD PANEL (SafeContent; centered). Ornate gold double-frame over an obsidian plate. Section D/E.
            // ============================================================================================
            var panelGo = new GameObject("RewardPanel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(PanelW, PanelH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();

            // PanelFrame: cast-gold beveled molding + obsidian field (returns the inner FIELD rect → parent in it).
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(PanelW, PanelH), CPanelBot, true, Molding);
            // Recolour the field to the obsidian #0c0e15→#161a24 plate (Section B/G) over the frame's default fill.
            var plate = UiWidgets.Rect("PlateGradient", field, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var plateImg = plate.gameObject.AddComponent<Image>(); plateImg.raycastTarget = false;
            plateImg.sprite = UiTex.VGradient(CPanelTop, CPanelBot, 64);
            plate.SetAsFirstSibling(); // behind all panel content, above the frame's own field fill
            // Inner thin gold filigree line just inside the molding (Section B/E "inner filigree").
            var filigree = UiWidgets.Rect("InnerFiligree", field, Vector2.zero, Vector2.one, new Vector2(0, 0), new Vector2(-18f, -18f));
            var filImg = filigree.gameObject.AddComponent<Image>(); filImg.raycastTarget = false;
            filImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.55f), UiTheme.A(UiTheme.Gold, 0.45f), UiTheme.A(UiTheme.GoldShadow, 0.5f), 48, 3);
            filImg.type = Image.Type.Sliced;

            // Scrolled corner cartouches (gold diamond ornaments at the four inner corners; Section B/D).
            CornerCartouche(field, new Vector2(0f, 1f), new Vector2( 34f, -34f));
            CornerCartouche(field, new Vector2(1f, 1f), new Vector2(-34f, -34f));
            CornerCartouche(field, new Vector2(0f, 0f), new Vector2( 34f,  34f));
            CornerCartouche(field, new Vector2(1f, 0f), new Vector2(-34f,  34f));

            BuildTopCrest(panel);   // crown/gem crest straddling the top edge (Section D)
            BuildCloseButton(panel); // ✕ top-right on the frame (Section D)

            // ---- Header: title "DAILY REWARD" + subtitle (Section C/E/F). Top inset ≈ 0.07·panelH. ----
            // Title cap region high in the panel; baseline ≈ fy 0.10 of full H above panel center mapped into field.
            UiWidgets.Glow(field, UiTheme.A(Hex("#e8c25a"), 0.22f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f),
                Vector2.zero, new Vector2(0.5f * PanelW, 140f), 1.7f);
            UiWidgets.TitleLabel(field, "DAILY REWARD", 74, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f),
                Vector2.zero, new Vector2(0.8f * PanelW, 110f), TextAnchor.MiddleCenter, CTitleTop, CTitleBot);
            UiWidgets.Label(field, "Log in every day to earn valuable rewards!", 26,
                new Vector2(0.5f, 0.79f), new Vector2(0.5f, 0.79f), Vector2.zero, new Vector2(0.8f * PanelW, 40f),
                TextAnchor.MiddleCenter, CSubtitle);

            // ---- DayRow: 7 cells in one row, centered, Day3 wider/taller (Section C/E). Vertical center ≈ field mid. ----
            BuildDayRow(field);

            // ---- FooterStrip: flame + STREAK line + sub + CLAIM pill (Section C/E). ----
            BuildFooter(field);
        }

        // ===================================================================================================
        // HUB BLEED (Section C/L) — faint, non-interactive forensic record of the inherited hub chrome.
        // ===================================================================================================
        private void BuildHubBleed()
        {
            var bleed = UiWidgets.Rect("InheritedHubChrome", SafeContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bleedCg = bleed.gameObject.AddComponent<CanvasGroup>();
            bleedCg.alpha = 0.22f; bleedCg.interactable = false; bleedCg.blocksRaycasts = false; // non-functional bleed

            // TopBar: avatar + name/level + currency chips (Gold/Silver/Gems). Energy chip CANON-CUT → omitted.
            var avatar = UiWidgets.Rect("AvatarPortrait", bleed, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -64f), new Vector2(80f, 80f));
            var avImg = avatar.gameObject.AddComponent<Image>(); avImg.raycastTarget = false; avImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            UiWidgets.Label(bleed, "Warden", 28, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -48f), new Vector2(220f, 36f), TextAnchor.MiddleLeft, UiTheme.ParchGold);
            UiWidgets.Label(bleed, "Level 32", 22, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -80f), new Vector2(220f, 32f), TextAnchor.MiddleLeft, CSubtitle);
            // Currency chips (top-right): Gold rightmost (idx 0), Silver (idx 1), Gems (idx 2). Forensic values.
            UiWidgets.CurrencyChip(bleed, UiTheme.Gold,       128450, 0, out _);
            UiWidgets.CurrencyChip(bleed, CSilver,            87560,  1, out _);
            UiWidgets.CurrencyChip(bleed, UiTheme.AmethystHi, 2850,   2, out _);

            // LeftRail: icon+label stack (Campaign, Army, Commanders, Quests, Store, Alliance, Events) — faint bleed.
            string[] rail = { "CAMPAIGN", "ARMY", "COMMANDERS", "QUESTS", "STORE", "ALLIANCE", "EVENTS" };
            for (int i = 0; i < rail.Length; i++)
            {
                float y = 0.80f - i * 0.105f;
                var t = UiWidgets.Rect("Rail_" + rail[i], bleed, new Vector2(0.04f, y), new Vector2(0.04f, y), Vector2.zero, new Vector2(70f, 70f));
                var ti = t.gameObject.AddComponent<Image>(); ti.raycastTarget = false; ti.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.9f), 48);
                UiWidgets.Label(bleed, rail[i], 16, new Vector2(0.04f, y - 0.045f), new Vector2(0.04f, y - 0.045f), Vector2.zero, new Vector2(180f, 24f), TextAnchor.UpperCenter, CSubtitle);
            }

            // RightEdgeTiles: faint event/battle tiles bleeding at the right edge (Section C/K decorative).
            EdgeTile(bleed, 0.72f, "EVENT", "Ends in 2d 14h");
            EdgeTile(bleed, 0.55f, "BATTLE", "Chapter 9-12");
        }

        private void EdgeTile(Transform parent, float y, string head, string sub)
        {
            var card = UiWidgets.Card(parent, new Vector2(0.93f, y), new Vector2(0.93f, y), Vector2.zero, new Vector2(300f, 120f), UiTheme.A(Hex("#10111a"), 0.9f));
            UiWidgets.Label(card, head, 24, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(280f, 30f), TextAnchor.MiddleCenter, UiTheme.ParchGold);
            UiWidgets.Label(card, sub, 18, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), Vector2.zero, new Vector2(280f, 28f), TextAnchor.MiddleCenter, CSubtitle);
        }

        // ===================================================================================================
        // TOP CREST (Section D/E) — crown/gem centerpiece straddling the top edge: cobalt gem in a gold sunburst.
        // ===================================================================================================
        private void BuildTopCrest(Transform panel)
        {
            // Width ≈ 0.10W ≈ 234; vertical center on the frame line (≈ half above the edge).
            _crest = UiWidgets.Rect("TopCrest", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 18f), new Vector2(234f, 150f));
            // Gold sunburst glow.
            UiWidgets.Glow(_crest, UiTheme.A(UiTheme.GoldHi, 0.5f), new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(260f, 220f), 1.6f);
            // Crown wings (two gold finials flanking the gem).
            var wl = UiWidgets.Finial(_crest, new Vector2(0.5f, 0.5f), new Vector2(-78f, -6f), 84f); wl.color = UiTheme.GoldHi; wl.transform.localRotation = Quaternion.Euler(0, 0, 32f);
            var wr = UiWidgets.Finial(_crest, new Vector2(0.5f, 0.5f), new Vector2( 78f, -6f), 84f); wr.color = UiTheme.GoldHi; wr.transform.localRotation = Quaternion.Euler(0, 0, -32f);
            // Gold disc base of the crest.
            var disc = UiWidgets.Rect("CrestDisc", _crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(118f, 118f));
            var di = disc.gameObject.AddComponent<Image>(); di.raycastTarget = false; di.sprite = UiTex.Disc(UiTheme.Gold, 64);
            var discRim = UiWidgets.Rect("CrestRim", disc, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var dri = discRim.gameObject.AddComponent<Image>(); dri.raycastTarget = false; dri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); dri.type = Image.Type.Sliced;
            // Cobalt gem in the centre (Section B/G).
            var gem = UiWidgets.Rect("CrestGem", disc, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));
            var gi = gem.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(CCobaltHi, 48);
            // Occasional specular glint streak on the gem (slow shimmer, Section I/J).
            var glint = gem.gameObject.AddComponent<PulseGraphic>(); glint.target = gi; glint.min = 0.7f; glint.max = 1f; glint.period = 3f;
        }

        // ===================================================================================================
        // CLOSE BUTTON ✕ (Section D/H) — dark disc + bronze ring, top-right on the frame. Closes the modal.
        // ===================================================================================================
        private void BuildCloseButton(Transform panel)
        {
            // ⌀ ≈ 0.045H ≈ 49; center ≈ (panelRight − 0.03W, panelTop − 0.03H) → inset from the top-right corner.
            var rt = UiWidgets.Rect("CloseButton", panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -26f), new Vector2(56f, 56f));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.Disc(CDiscBody, 48);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.86f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Pop(); }); // close → return to hub
            // Bronze ring.
            var ring = UiWidgets.Rect("CloseRing", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = ring.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, CCellBronze, UiTheme.GoldShadow, 48, 5); ri.type = Image.Type.Sliced;
            // ✕ glyph (two crossed bars).
            var bar1 = UiWidgets.Rect("X1", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 5f));
            var b1 = bar1.gameObject.AddComponent<Image>(); b1.raycastTarget = false; b1.color = UiTheme.GoldHi; bar1.localRotation = Quaternion.Euler(0, 0, 45f);
            var bar2 = UiWidgets.Rect("X2", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 5f));
            var b2 = bar2.gameObject.AddComponent<Image>(); b2.raycastTarget = false; b2.color = UiTheme.GoldHi; bar2.localRotation = Quaternion.Euler(0, 0, -45f);
        }

        // ===================================================================================================
        // DAY ROW (Section C/E/H) — 7 cells, single row, centered. Day3 wider/taller (claimable, halo);
        // Day7 violet/premium. Walks left→right summing widths/gaps (mirrors the proven row-layout pattern).
        // ===================================================================================================
        private void BuildDayRow(Transform field)
        {
            // Inner usable width ≈ panelW − 2·(0.04W); 7 cells + 6 gaps. Base cell ≈ 198; Day3 ≈ 222 (×1.12).
            float gap     = 0.012f * W;            // ≈ 28
            float baseW   = 198f;
            float baseH   = 360f;                  // standard cell height (~0.33H)
            float claimW  = baseW * 1.12f;         // Day3 wider
            float claimH  = baseH * 1.10f;         // Day3 taller
            float bandY   = 0.50f;                 // vertical center ≈ field mid (anchorY)

            // Total row width and left edge (relative to field center).
            float rowW = 0f;
            for (int i = 0; i < Days.Length; i++) rowW += (i == ClaimableIndex ? claimW : baseW) + (i < Days.Length - 1 ? gap : 0f);
            float x = -rowW * 0.5f;

            for (int i = 0; i < Days.Length; i++)
            {
                bool claimable = i == ClaimableIndex;
                float cw = claimable ? claimW : baseW;
                float ch = claimable ? claimH : baseH;
                var pos = new Vector2(x + cw * 0.5f, 0f);
                BuildDayCell(field, i, new Vector2(0.5f, bandY), pos, new Vector2(cw, ch));
                x += cw + gap;
            }
        }

        private void BuildDayCell(Transform field, int i, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var d = Days[i];
            var cell = UiWidgets.Rect("DayCell_" + (i + 1) + (d.Premium ? "_Premium" : ""), field, anchor, anchor, pos, size);
            _cellRt[i] = cell;
            var cg = cell.gameObject.AddComponent<CanvasGroup>(); _cellCg[i] = cg;
            // CLAIMED/LOCKED cells are informational — raycast off (Section H/L: only CLAIM & ✕ are interactive).
            cg.interactable = false; cg.blocksRaycasts = false;

            // HaloRing (Day3 only) — behind the cell body, additive pulsing gold glow (Section D/H/J).
            if (d.State == DayState.Claimable)
            {
                var halo = UiWidgets.Glow(cell, UiTheme.A(CHalo, 0.85f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.35f, 1.5f);
                halo.transform.SetAsFirstSibling();
                _haloRt = (RectTransform)halo.transform;
                // Cobalt inner focal glow under the claimable cell (Section G).
                var inner = UiWidgets.Glow(cell, UiTheme.A(CCobalt, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 0.9f, 1.7f);
                inner.transform.SetAsFirstSibling();
            }

            // Cell body — standard slate / claimable brighter / premium violet (Section G).
            var body = cell.gameObject.AddComponent<Image>(); body.raycastTarget = false;
            if (d.Premium)            body.sprite = UiTex.VGradient(CVioTop, CVioBot, 64);
            else if (d.State == DayState.Claimable) body.sprite = UiTex.VGradient(CClaimTop, CClaimBot, 64);
            else                      body.sprite = UiTex.VGradient(CCellTop, CCellBot, 64);

            // Cell frame — bronze edge (standard/claimable) / amethyst (premium Day7, load-bearing Section L).
            var frame = UiWidgets.Rect("Frame", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fi = frame.gameObject.AddComponent<Image>(); fi.raycastTarget = false;
            if (d.Premium) fi.sprite = UiTex.Frame(CAmethystHi, CAmethyst, UiWidgets.Darken(CAmethyst, 0.4f), 48, 7);
            else           fi.sprite = UiTex.Frame(UiWidgets.Lighten(CCellBronze, 0.35f), CCellBronze, UiWidgets.Darken(CCellBronze, 0.45f), 48, 5);
            fi.type = Image.Type.Sliced;

            // Premium Day7 inner magic glow + bloom (Section G).
            if (d.Premium)
            {
                var mg = UiWidgets.Glow(cell, UiTheme.A(CMagicGlow, 0.45f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, size * 0.95f, 1.7f);
                mg.transform.SetSiblingIndex(1);
                var mp = mg.gameObject.AddComponent<PulseGraphic>(); mp.target = mg; mp.min = 0.3f; mp.max = 0.55f; mp.period = 2.2f;
            }

            // DayHeader strip (top 0–0.18 of cell): "DAY n" banner (Day7 gold). Section E/F.
            UiWidgets.Label(cell, d.Header, 24, new Vector2(0.5f, 0.91f), new Vector2(0.5f, 0.91f), Vector2.zero,
                new Vector2(0.96f * size.x, 0.16f * size.y), TextAnchor.MiddleCenter, d.Premium ? CDayHdrGold : CDayHdr);

            // DayIcon (0.18–0.62, square ≈ 0.6·cellW). Section E.
            float iconSide = 0.6f * size.x;
            BuildDayIcon(cell, d, new Vector2(0.5f, 0.58f), new Vector2(iconSide, iconSide));

            // DayAmount / premium label block (0.62–0.80). Section E/F.
            if (d.Premium)
            {
                // "EPIC CHEST" violet small-caps + "+ Legendary Unit Guaranteed!" sub (Section C/F/L).
                UiWidgets.Label(cell, UiTheme.Track("EPIC CHEST"), 24, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), Vector2.zero,
                    new Vector2(0.98f * size.x, 0.12f * size.y), TextAnchor.MiddleCenter, CEpic);
                UiWidgets.Label(cell, "+ Legendary Unit\nGuaranteed!", 17, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero,
                    new Vector2(0.98f * size.x, 0.16f * size.y), TextAnchor.MiddleCenter, CEpicSub);
            }
            else
            {
                bool hi = d.State == DayState.Claimable;
                var amt = UiWidgets.Label(cell, d.Amount, hi ? 36 : 32, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), Vector2.zero,
                    new Vector2(0.96f * size.x, 0.18f * size.y), TextAnchor.MiddleCenter, hi ? CAmountHi : CAmount);
                if (hi) { var ag = amt.gameObject.AddComponent<Outline>(); ag.effectColor = UiTheme.A(CCobaltHi, 0.6f); ag.effectDistance = new Vector2(0f, 0f); }
            }

            // DayStateBadge disc (⌀ ≈ 0.22·cellW centered at 0.90 from top → anchorY ≈ 0.10). Section E/G.
            float discD = 0.22f * size.x;
            BuildStateBadge(cell, d, new Vector2(0.5f, 0.04f), discD);

            // LOCKED dim: ~70% brightness + grey overlay (Section G/H).
            if (d.State == DayState.Locked)
            {
                var grey = UiWidgets.Rect("LockOverlay", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var gi = grey.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.color = new Color(0.05f, 0.06f, 0.08f, 0.42f);
            }
        }

        // ---- Per-day reward icon (display-only stand-ins for bespoke art, Section G/N). ----
        private void BuildDayIcon(Transform cell, Day d, Vector2 anchor, Vector2 size)
        {
            switch (d.Icon)
            {
                case DayIcon.Gold: // stacked warm-gold coins with an embossed star
                    for (int k = 0; k < 3; k++)
                    {
                        var coin = UiWidgets.Rect("Coin", cell, anchor, anchor, new Vector2(0, (k - 1) * size.y * 0.2f), new Vector2(size.x, size.y * 0.32f));
                        var ci = coin.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(Hex("#f0c14a"), 48);
                    }
                    var gStar = UiWidgets.Rect("Star", cell, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.38f, size.x * 0.38f));
                    var gsi = gStar.gameObject.AddComponent<Image>(); gsi.raycastTarget = false; gsi.sprite = UiTex.Diamond(Hex("#fff2c2"), 32);
                    break;

                case DayIcon.Silver: // cool silver bars (specular), stacked
                    for (int k = 0; k < 3; k++)
                    {
                        var bar = UiWidgets.Rect("Bar", cell, anchor, anchor, new Vector2((k - 1) * size.x * 0.16f, (k - 1) * size.y * 0.14f), new Vector2(size.x * 0.7f, size.y * 0.26f));
                        var bi = bar.gameObject.AddComponent<Image>(); bi.raycastTarget = false; bi.sprite = UiTex.VGradient(Hex("#eef0f4"), CSilver, 32);
                    }
                    break;

                case DayIcon.Crystal: // cobalt blue crystal with inner glow + crystalline specular (Day3 focal)
                    var cGlow = UiWidgets.Glow(cell, UiTheme.A(CCobaltHi, 0.6f), anchor, anchor, Vector2.zero, size * 1.3f, 1.7f); cGlow.transform.SetAsFirstSibling();
                    var crys = UiWidgets.Rect("Crystal", cell, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.6f, size.y * 0.85f));
                    var cri = crys.gameObject.AddComponent<Image>(); cri.raycastTarget = false; cri.sprite = UiTex.VGradient(CCobaltHi, CCobalt, 64);
                    crys.localRotation = Quaternion.Euler(0, 0, 45f);
                    var twk = UiWidgets.Rect("Twinkle", crys, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(size.x * 0.22f, size.x * 0.22f));
                    var twi = twk.gameObject.AddComponent<Image>(); twi.raycastTarget = false; twi.sprite = UiTex.Diamond(Color.white, 32);
                    var twp = twk.gameObject.AddComponent<PulseGraphic>(); twp.target = twi; twp.min = 0.3f; twp.max = 1f; twp.period = 1.8f;
                    break;

                case DayIcon.Chest: // aged-wood chest + bronze bands (Day4)
                    var ch = UiWidgets.Rect("Chest", cell, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.86f, size.y * 0.7f));
                    var chi = ch.gameObject.AddComponent<Image>(); chi.raycastTarget = false; chi.sprite = UiTex.VGradient(UiWidgets.Lighten(CChestWood, 0.15f), CChestWood, 64);
                    var chRim = UiWidgets.Rect("ChestRim", ch, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var chr = chRim.gameObject.AddComponent<Image>(); chr.raycastTarget = false; chr.sprite = UiTex.Frame(UiWidgets.Lighten(CChestBand, 0.2f), CChestBand, UiWidgets.Darken(CChestBand, 0.4f), 48, 6); chr.type = Image.Type.Sliced;
                    var band = UiWidgets.Rect("Band", ch, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x * 0.86f, size.y * 0.14f));
                    var bni = band.gameObject.AddComponent<Image>(); bni.raycastTarget = false; bni.color = CChestBand;
                    break;

                case DayIcon.Sigil: // shield / sigil (Day5)
                    var shield = UiWidgets.Rect("Shield", cell, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.72f, size.y * 0.86f));
                    var shi = shield.gameObject.AddComponent<Image>(); shi.raycastTarget = false; shi.sprite = UiTex.VGradient(UiTheme.IronBlueHi, UiTheme.IronBlue, 64);
                    var shRim = UiWidgets.Rect("ShieldRim", shield, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var shr = shRim.gameObject.AddComponent<Image>(); shr.raycastTarget = false; shr.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); shr.type = Image.Type.Sliced;
                    var crest = UiWidgets.Rect("Sigil", shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x * 0.3f, size.x * 0.3f));
                    var cre = crest.gameObject.AddComponent<Image>(); cre.raycastTarget = false; cre.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
                    break;

                case DayIcon.Gems: // blue gem cluster (Day6)
                    var gGlow = UiWidgets.Glow(cell, UiTheme.A(CCobaltHi, 0.5f), anchor, anchor, Vector2.zero, size * 1.25f, 1.7f); gGlow.transform.SetAsFirstSibling();
                    float[] dx = { -0.22f, 0.22f, 0f }; float[] dy = { -0.12f, -0.12f, 0.18f };
                    for (int k = 0; k < 3; k++)
                    {
                        var gem = UiWidgets.Rect("Gem", cell, anchor, anchor, new Vector2(dx[k] * size.x, dy[k] * size.y), new Vector2(size.x * 0.46f, size.x * 0.46f));
                        var gmi = gem.gameObject.AddComponent<Image>(); gmi.raycastTarget = false; gmi.sprite = UiTex.Diamond(k == 2 ? CCobaltHi : CCobalt, 48);
                    }
                    break;

                case DayIcon.EpicChest: // ornate gold/violet chest (Day7 premium)
                    var ec = UiWidgets.Rect("EpicChest", cell, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.9f, size.y * 0.72f));
                    var eci = ec.gameObject.AddComponent<Image>(); eci.raycastTarget = false; eci.sprite = UiTex.VGradient(Hex("#f6d77a"), UiTheme.Gold, 64);
                    var ecRim = UiWidgets.Rect("EpicRim", ec, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var ecr = ecRim.gameObject.AddComponent<Image>(); ecr.raycastTarget = false; ecr.sprite = UiTex.Frame(CAmethystHi, CAmethyst, UiWidgets.Darken(CAmethyst, 0.4f), 48, 7); ecr.type = Image.Type.Sliced;
                    var ecGem = UiWidgets.Rect("EpicGem", ec, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(size.x * 0.26f, size.x * 0.26f));
                    var ecg = ecGem.gameObject.AddComponent<Image>(); ecg.raycastTarget = false; ecg.sprite = UiTex.Diamond(CMagicGlow, 48);
                    break;
            }
        }

        // ---- State badge disc: ✓ gold (claimed/claimable) or 🔒 steel (locked). Section E/G. ----
        private void BuildStateBadge(Transform cell, Day d, Vector2 anchor, float diameter)
        {
            var disc = UiWidgets.Rect("DayStateBadge", cell, anchor, anchor, Vector2.zero, new Vector2(diameter, diameter));
            var di = disc.gameObject.AddComponent<Image>(); di.raycastTarget = false; di.sprite = UiTex.Disc(CDiscBody, 48);
            var ring = UiWidgets.Rect("BadgeRing", disc, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = ring.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, CCellBronze, UiTheme.GoldShadow, 48, 4); ri.type = Image.Type.Sliced;

            if (d.State == DayState.Locked)
            {
                // 🔒 desaturated steel — body + shackle.
                var body = UiWidgets.Rect("LockBody", disc, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(diameter * 0.42f, diameter * 0.34f));
                var lb = body.gameObject.AddComponent<Image>(); lb.raycastTarget = false; lb.color = CLockSteel;
                var sh = UiWidgets.Rect("LockShackle", disc, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(diameter * 0.26f, diameter * 0.26f));
                var shi = sh.gameObject.AddComponent<Image>(); shi.raycastTarget = false; shi.sprite = UiTex.Frame(CLockSteel, CLockSteel, UiWidgets.Darken(CLockSteel, 0.4f), 48, 5); shi.type = Image.Type.Sliced;
            }
            else
            {
                // ✓ gold engraved (claimed + claimable).
                var c1 = UiWidgets.Rect("Check1", disc, new Vector2(0.42f, 0.4f), new Vector2(0.42f, 0.4f), Vector2.zero, new Vector2(diameter * 0.16f, diameter * 0.42f));
                var ci1 = c1.gameObject.AddComponent<Image>(); ci1.raycastTarget = false; ci1.color = CCheck; c1.localRotation = Quaternion.Euler(0, 0, 45f);
                var c2 = UiWidgets.Rect("Check2", disc, new Vector2(0.56f, 0.46f), new Vector2(0.56f, 0.46f), Vector2.zero, new Vector2(diameter * 0.16f, diameter * 0.64f));
                var ci2 = c2.gameObject.AddComponent<Image>(); ci2.raycastTarget = false; ci2.color = CCheck; c2.localRotation = Quaternion.Euler(0, 0, -45f);
            }
        }

        // ===================================================================================================
        // FOOTER STRIP (Section C/E/H) — rounded sub-panel inside the frame: flame + STREAK line + sub + CLAIM.
        // ===================================================================================================
        private void BuildFooter(Transform field)
        {
            // Height ≈ 0.16·panelH ≈ 135; bottom inset ≈ 0.05·panelH; spans inner width.
            float footH = 0.16f * PanelH;
            _footerRt = UiWidgets.Rect("FooterStrip", field, new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.0f), new Vector2(0f, 0.075f * PanelH), new Vector2(0.9f * PanelW, footH));
            _footerCg = _footerRt.gameObject.AddComponent<CanvasGroup>();
            // Rounded sub-panel container (dark glass + gold hairline).
            var strip = _footerRt.gameObject.AddComponent<Image>();
            var ps = UiWidgets.Spr("panel"); if (ps != null) { strip.sprite = ps; strip.type = Image.Type.Sliced; }
            strip.color = UiTheme.A(Hex("#10131c"), 0.92f); strip.raycastTarget = false;
            var stripRim = UiWidgets.Rect("FooterRim", _footerRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var sri = stripRim.gameObject.AddComponent<Image>(); sri.raycastTarget = false; sri.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 48, 5); sri.type = Image.Type.Sliced;

            // FlameIcon (left): ember glow + flicker (Section H/J). ⌀ ≈ 0.07·panelH.
            float flameD = 0.07f * PanelH;
            var flame = UiWidgets.Rect("FlameIcon", _footerRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(flameD * 0.9f + 16f, 0f), new Vector2(flameD, flameD));
            var fGlow = UiWidgets.Glow(flame, UiTheme.A(CFlame, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(flameD * 1.6f, flameD * 1.8f), 1.5f);
            var fp = fGlow.gameObject.AddComponent<PulseGraphic>(); fp.target = fGlow; fp.min = 0.5f; fp.max = 1f; fp.period = 0.5f; // flicker
            var fi = flame.gameObject.AddComponent<Image>(); fi.raycastTarget = false; fi.sprite = UiTex.Diamond(CFlame, 48);
            var fef = flame.gameObject.AddComponent<EmberField>(); fef.count = 6; fef.color = CFlame;

            // StreakText (line + sub), left, flex grow. "STREAK: 3 DAYS" (label + brighter/larger value) + sub.
            float textX = flameD + 40f;
            var streakRow = UiWidgets.Rect("StreakLine", _footerRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(textX, 26f), new Vector2(620f, 44f));
            UiWidgets.Label(streakRow, "STREAK: ", 30, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(180f, 44f), TextAnchor.MiddleLeft, CStreakLbl);
            // "3 DAYS" value — brighter + larger (count-up on first show, Section I).
            _streakValue = UiWidgets.Label(streakRow, "0 DAYS", 34, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(168f, 0f), new Vector2(280f, 48f), TextAnchor.MiddleLeft, CStreakVal);
            UiWidgets.Label(_footerRt, "Keep the streak going to earn better rewards!", 20,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(textX + 4f, -22f), new Vector2(720f, 30f), TextAnchor.MiddleLeft, CStreakSub);

            // ClaimButton (right): gold pill (the single brightest interactive object). Section C/E/H.
            BuildClaimButton(_footerRt);
        }

        // ---- CLAIM: brushed-gold pill, beveled, dark serif label. The only live CTA besides ✕. Section H/K. ----
        private void BuildClaimButton(RectTransform footer)
        {
            float claimW = 0.26f * PanelW;   // ≈ 487
            float claimH = 0.11f * PanelH;   // ≈ 93
            // Right-anchored with 0.04W right inset (inside the footer's inner edge).
            _claimRt = UiWidgets.Rect("ClaimButton", footer, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(claimW * 0.5f) - 28f, 0f), new Vector2(claimW, claimH));
            // Outer breathing glow (Section H/I/J).
            var glow = UiWidgets.Glow(_claimRt, UiTheme.A(UiTheme.GoldHi, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(claimW, claimH) * 1.4f, 1.8f);
            var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.3f; pg.max = 0.6f; pg.period = 1.6f;
            glow.transform.SetAsFirstSibling();
            // Brushed-gold body (top sheen → dark) with inner gradient.
            _claimFill = _claimRt.gameObject.AddComponent<Image>();
            _claimFill.sprite = UiTex.VGradient(Hex("#f6e0a0"), Hex("#caa04a"), 32);
            _claimBtn = _claimRt.gameObject.AddComponent<Button>(); _claimBtn.targetGraphic = _claimFill;
            var cb = _claimBtn.colors; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.8f, 1f); cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f); cb.fadeDuration = 0.08f; _claimBtn.colors = cb;
            _claimBtn.onClick.AddListener(OnClaim);
            // Cast-gold beveled rim.
            var rim = UiWidgets.Rect("ClaimRim", _claimRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); ri.type = Image.Type.Sliced;
            // "CLAIM" label — dark serif engrave on gold, top highlight (Section F).
            _claimLabel = UiWidgets.Label(_claimRt, UiTheme.Track("CLAIM"), 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CClaimLbl);
            _claimLabel.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#fff2c2"), 0.5f);

            _claimCg = _claimRt.gameObject.AddComponent<CanvasGroup>();
        }

        // ===================================================================================================
        // CLAIM (display-only, Section K) — never a server write. Toast + visually mark Day3 claimed + a stub
        // currency nudge (UiStub.GrantGems for the 100-gem Day3) + collect burst; CLAIM → "CLAIMED" disabled.
        // ===================================================================================================
        private void OnClaim()
        {
            if (_claimed)
            {
                // Already claimed today: shake + toast (no request). Section H/K.
                StartCoroutine(Shake(_claimRt));
                Router.Toast("Already claimed — come back tomorrow!");
                return;
            }
            _claimed = true;
            AudioManager.Instance?.Click();

            // Display-only currency stub for the claimable day (Day3 = 100 gems). NEVER a server-authoritative write.
            var d = Days[ClaimableIndex];
            if (d.Give == Grant.Gems) UiStub.GrantGems(d.GiveN);
            else if (d.Give == Grant.Gold) UiStub.GrantGold(d.GiveN);

            Router.Toast("Daily reward claimed!");
            StartCoroutine(ClaimSequence());
        }

        // ===================================================================================================
        // LIFECYCLE — entry timeline (Section I) + idle loops.
        // ===================================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Entry());
        }

        // Entry (~0.85s, Section I): panel pop → crest drop → cells stagger L→R → halo up → footer slide → CLAIM pop.
        private IEnumerator Entry()
        {
            // Pre-state: panel scaled down + transparent; cells hidden + nudged down; crest up; footer/CLAIM hidden.
            if (_panelCg != null) { _panelCg.alpha = 0f; _panelCg.transform.localScale = Vector3.one * 0.92f; }
            if (_crest != null) _crest.anchoredPosition += new Vector2(0f, -20f);
            for (int i = 0; i < _cellCg.Length; i++) { if (_cellCg[i] != null) _cellCg[i].alpha = 0f; if (_cellRt[i] != null) _cellRt[i].anchoredPosition += new Vector2(0f, -16f); }
            if (_footerCg != null) { _footerCg.alpha = 0f; _footerRt.anchoredPosition += new Vector2(0f, -24f); }
            if (_claimCg != null) { _claimCg.alpha = 0f; _claimRt.localScale = Vector3.one * 0.9f; }
            if (_haloRt != null) { var hi = _haloRt.GetComponent<Image>(); if (hi != null) { var c = hi.color; c.a = 0f; hi.color = c; } }

            // 0.06 panel scale 0.92→1.00 + α (ease-out-back small, 0.28s).
            yield return Tween(0.28f, k =>
            {
                float e = EaseOutBack(k);
                if (_panelCg != null) { _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, e); }
            });
            if (_panelCg != null) { _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one; }

            // 0.18 crest drop-in (y −20→0 + glint already looping).
            var crestStart = _crest != null ? _crest.anchoredPosition : Vector2.zero;
            yield return Tween(0.22f, k => { if (_crest != null) _crest.anchoredPosition = crestStart + new Vector2(0f, 20f * EaseOut(k)); });
            if (_crest != null) _crest.anchoredPosition = crestStart + new Vector2(0f, 20f);

            // 0.28 cells stagger L→R, each y +16→0 + α, 0.04s apart, 0.18s each (ease-out).
            for (int i = 0; i < _cellRt.Length; i++)
            {
                StartCoroutine(CellIn(i));
                yield return Wait(0.04f);
            }
            yield return Wait(0.18f);

            // 0.55 halo fades up + starts the idle pulse loop (period 1.6s).
            if (_haloRt != null)
            {
                var hImg = _haloRt.GetComponent<Image>();
                yield return Tween(0.25f, k => { if (hImg != null) { var c = hImg.color; c.a = Mathf.Lerp(0f, 0.85f, k); hImg.color = c; } });
                var hp = _haloRt.gameObject.AddComponent<PulseGraphic>(); hp.target = hImg; hp.min = 0.55f; hp.max = 0.95f; hp.period = 1.6f;
                var hs = _haloRt.gameObject.AddComponent<PulseScale>(); hs.min = 1.0f; hs.max = 1.06f; hs.period = 1.6f;
                // Claimable cell gentle one-shot scale 1.00→1.04→1.00 (Section H).
                if (_cellRt[ClaimableIndex] != null) StartCoroutine(CellBreath(_cellRt[ClaimableIndex]));
            }

            // 0.62 footer slide up + α; streak value count-up 0→3.
            var footStart = _footerRt != null ? _footerRt.anchoredPosition : Vector2.zero;
            yield return Tween(0.20f, k =>
            {
                if (_footerCg != null) _footerCg.alpha = k;
                if (_footerRt != null) _footerRt.anchoredPosition = footStart + new Vector2(0f, 24f * EaseOut(k));
            });
            if (_footerCg != null) _footerCg.alpha = 1f;
            if (_footerRt != null) _footerRt.anchoredPosition = footStart + new Vector2(0f, 24f);
            StartCoroutine(StreakCountUp());

            // 0.70 CLAIM pop 0.9→1.0 + glow on; breathing-glow loop already running.
            yield return Tween(0.18f, k =>
            {
                float e = EaseOutBack(k);
                if (_claimCg != null) _claimCg.alpha = k;
                if (_claimRt != null) _claimRt.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, e);
            });
            if (_claimCg != null) _claimCg.alpha = 1f;
            if (_claimRt != null) _claimRt.localScale = Vector3.one;
        }

        // Per-cell entry: y +16→0 + α over 0.18s (ease-out).
        private IEnumerator CellIn(int i)
        {
            var rt = _cellRt[i]; var cg = _cellCg[i];
            if (rt == null) yield break;
            var start = rt.anchoredPosition; // already nudged −16
            yield return Tween(0.18f, k =>
            {
                float e = EaseOut(k);
                if (cg != null) cg.alpha = k;
                rt.anchoredPosition = start + new Vector2(0f, 16f * e);
            });
            rt.anchoredPosition = start + new Vector2(0f, 16f);
            if (cg != null) cg.alpha = 1f;
        }

        // Claimable cell one-shot 1.00→1.04→1.00 settle (Section H).
        private IEnumerator CellBreath(RectTransform rt)
        {
            yield return Tween(0.5f, k => { float s = 1f + 0.04f * Mathf.Sin(k * Mathf.PI); rt.localScale = Vector3.one * s; });
            rt.localScale = Vector3.one;
        }

        // Streak value count-up 0→3 (Section I) — display only.
        private IEnumerator StreakCountUp()
        {
            if (_streakValue == null) yield break;
            yield return Tween(0.4f, k =>
            {
                int v = Mathf.RoundToInt(Mathf.Lerp(0f, StreakDays, EaseOut(k)));
                _streakValue.text = v + (v == 1 ? " DAY" : " DAYS");
            });
            _streakValue.text = StreakDays + " DAYS";
        }

        // OnClaim sequence (Section I/K): CLAIM press → white flash → Day3 collect burst → currency fly-to-chip →
        // cell stays ✓ (already drawn) + CLAIM → disabled "CLAIMED".
        private IEnumerator ClaimSequence()
        {
            // CLAIM press 0.96 (0.08s).
            if (_claimRt != null)
            {
                yield return Tween(0.08f, k => _claimRt.localScale = Vector3.one * Mathf.Lerp(1f, 0.96f, k));
            }
            if (_claimRt != null) _claimRt.localScale = Vector3.one;

            // White flash → gold (0.10s): a brief white overlay on the pill fades out (Section I success beat).
            if (_claimRt != null)
            {
                var flash = UiWidgets.Rect("ClaimFlash", _claimRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var fImg = flash.gameObject.AddComponent<Image>(); fImg.raycastTarget = false; fImg.color = new Color(1f, 1f, 1f, 0.9f);
                yield return Tween(0.10f, k => { var c = fImg.color; c.a = Mathf.Lerp(0.9f, 0f, k); fImg.color = c; });
                if (flash != null) Destroy(flash.gameObject);
            }

            // Day3 collect burst: icon scale 1.2 + a spark burst from the claimable cell (Section I/J).
            if (_cellRt[ClaimableIndex] != null)
            {
                var cell = _cellRt[ClaimableIndex];
                var burstHost = UiWidgets.Rect("CollectBurst", cell, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, cell.sizeDelta * 0.9f);
                var bef = burstHost.gameObject.AddComponent<EmberField>(); bef.count = 18; bef.color = CHalo;
                yield return Tween(0.4f, k =>
                {
                    float s = 1f + 0.2f * Mathf.Sin(k * Mathf.PI);
                    cell.localScale = Vector3.one * s;
                });
                cell.localScale = Vector3.one;
            }

            // Currency fly-to-chip: a gold spark streaks from the cell toward the top-right currency bar (Section J).
            yield return FlyToChip();

            // CLAIM → disabled "CLAIMED" greyed (Section H).
            MarkClaimed();
        }

        // A single gold spark flies from the claimable cell up to the top-right currency chips (decorative, Section J).
        private IEnumerator FlyToChip()
        {
            var spark = UiWidgets.Rect("FlySpark", SafeContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f));
            var si = spark.gameObject.AddComponent<Image>(); si.raycastTarget = false; si.sprite = UiTex.Disc(CHalo, 32);
            // From panel center-ish (claimable cell) to top-right (currency bar region).
            var from = new Vector2(0f, 0f);
            var to = new Vector2(0.42f * W, 0.42f * H);
            yield return Tween(0.5f, k =>
            {
                float e = EaseOut(k);
                spark.anchoredPosition = Vector2.Lerp(from, to, e);
                var c = si.color; c.a = Mathf.Lerp(1f, 0f, k); si.color = c;
                spark.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.4f, k);
            });
            if (spark != null) Destroy(spark.gameObject);
        }

        // CLAIM → disabled grey-gold "CLAIMED" (Section H disabled/success state).
        private void MarkClaimed()
        {
            if (_claimFill != null) _claimFill.sprite = UiTex.VGradient(UiWidgets.Lighten(CClaimGrey, 0.12f), UiWidgets.Darken(CClaimGrey, 0.18f), 32);
            if (_claimLabel != null) { _claimLabel.text = UiTheme.Track("CLAIMED"); _claimLabel.color = Hex("#4a4636"); }
            if (_claimBtn != null) _claimBtn.interactable = false; // raycast remains via the disabled colour; no further claim
        }

        // ===================================================================================================
        // SMALL HELPERS
        // ===================================================================================================
        private IEnumerator Shake(RectTransform rt)
        {
            if (rt == null) yield break;
            var basePos = rt.anchoredPosition; float t = 0f; const float d = 0.3f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = t / d; rt.anchoredPosition = basePos + new Vector2(Mathf.Sin(k * 40f) * 10f * (1f - k), 0f); yield return null; }
            rt.anchoredPosition = basePos;
        }

        // Tween driver on UNSCALED time (menus run at Time.timeScale = 0). step(k) with k in [0,1]; final k=1 forced.
        private IEnumerator Tween(float duration, System.Action<float> step)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, duration));
                step?.Invoke(k);
                yield return null;
            }
            step?.Invoke(1f);
        }

        private IEnumerator Wait(float s) { float t = 0f; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }

        // Corner cartouche: small gold diamond ornament at a frame corner (Section B/D).
        private void CornerCartouche(Transform field, Vector2 anchor, Vector2 pos)
        {
            var f = UiWidgets.Finial(field, anchor, pos, 30f);
            f.color = UiTheme.A(UiTheme.GoldHi, 0.9f);
        }

        private static float EaseOut(float x) => 1f - (1f - x) * (1f - x);
        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; float p = x - 1f; return 1f + c3 * p * p * p + c1 * p * p; }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
