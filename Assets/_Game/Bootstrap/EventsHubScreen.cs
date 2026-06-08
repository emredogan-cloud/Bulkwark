// BULWARK — EVENTS HUB (UI Construction Bible · 31). Presentation-only, REMOVABLE.
//
// §12 boundary: presentation only — NO ECS / NO Unity.Entities / NO gameplay/balance/AI/economy/backend. This is a
// forensic code-built rebuild of design/EventsHubDesign.png (Bible 31): hub chrome (Back TL · "EVENTS" gold-bevel
// serif title + subtitle · currency chips TR) over a dark obsidian field, a wide ornate-framed FEATURED banner
// ("DOUBLE / SILVER WEEKEND" + blue ribbon, "Ends in 1d 12h" pill, glowing silver 2× emblem), a "MORE EVENTS"
// divider, a HorizontalLayoutGroup row of 4 event cards (art/NEW · name · 2-line desc · REWARDS row · blue PLAY ·
// per-card timer), and a bottom 3-tab toggle bar (Events active · Calendar · Past Events). All event data are
// clearly DISPLAY-ONLY local stubs (the live-ops payload is server-authoritative in prod, never client-authored);
// PLAY/banner/tabs/chip-+ are display-only (Router.Toast). Canon flags honored (§L): the 58/120 EnergyChip is
// CANON-CUT (omitted; recorded only), and "Arena Clash" is relabeled ASYNC (no real-time PvP). NO ECS/gameplay (§12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-31 landscape Events Hub (featured banner + card shelf + tab bar). Presentation-only.</summary>
    public sealed class EventsHubScreen : UiScreen
    {
        // ---- Canvas constants (CanvasScaler 2340×1080, match-height). fy = fraction from TOP; anchorY = 1 − fy. ----
        private const float W = 2340f, H = 1080f;

        // ---- Exact spec hex not present in UiTheme (Section F/G verbatim). Display-only colours. ----
        private static readonly Color SubGold    = Hex("#d9c79a"); // subtitle / "Ends in …" card timer / MORE EVENTS-ish
        private static readonly Color RibbonBlue  = Hex("#2b56c8"); // FEATURED ribbon cloth (= IronBlue)
        private static readonly Color RibbonText  = Hex("#eaf2ff"); // "FEATURED" / "PLAY" text on cobalt
        private static readonly Color BannerSilA  = Hex("#e8e2cf"); // banner-title silvery-gold top
        private static readonly Color BannerSilB  = Hex("#cfd3dc"); // banner-title silvery-gold bottom
        private static readonly Color BannerStrk   = Hex("#2a2c33"); // banner-title stroke
        private static readonly Color PillBg       = Hex("#11141d"); // timer pill plate
        private static readonly Color PillText     = Hex("#ffe08a"); // "Ends in 1d 12h" gold
        private static readonly Color EmblemGold   = Hex("#f4e6b0"); // "2×" gold
        private static readonly Color SilverCoinA  = Hex("#e8ebf0"); // silver lion coin highlight
        private static readonly Color SilverCoinB  = Hex("#b8bcc6"); // silver lion coin body
        private static readonly Color RingBlue     = Hex("#4f8bff"); // cobalt energy ring bright
        private static readonly Color MoreEvents   = Hex("#cdbf99"); // "MORE EVENTS"
        private static readonly Color CardTop      = Hex("#181c27"); // card panel top
        private static readonly Color CardBot      = Hex("#10131c"); // card panel bottom
        private static readonly Color CardName     = Hex("#f2e8cf"); // card name serif
        private static readonly Color CardDesc     = Hex("#a9a28c"); // card 2-line desc
        private static readonly Color RewardsLbl   = Hex("#b3ac96"); // "REWARDS"
        private static readonly Color PlayBlueA    = Hex("#4f8bff"); // PLAY gloss top
        private static readonly Color PlayBlueB    = Hex("#2b56c8"); // PLAY gloss bottom
        private static readonly Color NewRibbonCol = Hex("#2b56c8"); // NEW tag (blue/gold)
        private static readonly Color EmberWarn    = Hex("#e0742a"); // ending-soon timer ember (Section H)
        private static readonly Color TabActive    = Hex("#f0d27a"); // active tab label/glyph
        private static readonly Color TabIdle      = Hex("#7d7768"); // inactive tab label/glyph
        private static readonly Color TopGlowCol   = Hex("#2a2416"); // warm additive top glow

        // ---- Reward-icon tiers (Section C/G): display-only chips. trophy/shard/chest/coin/gem/rank — colour stand-ins. ----
        private enum Reward { Trophy, Shard, Chest, Coin, Gem, Rank }

        // ---- Display-only event card model (server-auth live-ops in prod — NEVER client-authored, §L). ----
        private struct EventDef
        {
            public string Name, Desc, Timer;
            public Color ArtA, ArtB;          // card-art gradient tone (theme stand-in for bespoke art)
            public bool New;                   // NEW ribbon (Endless Rush only)
            public bool EndingSoon;            // timer < threshold → ember pulse (Resource Run "12h 45m")
            public Reward R0, R1, R2;          // 3 REWARDS-row icons
            public System.Action Go;           // PLAY route (display-only / MatchPresentation seam)
        }

        // Animation handles (presentation-only; no gameplay state).
        private CanvasGroup _bannerCg;
        private CanvasGroup[] _cardCg;
        private RectTransform _tabUnderline;

        protected override void Build()
        {
            // ============================ FullBleedBackdrop: dark obsidian field + FX (→ Rect) ============================
            UiWidgets.Backdrop(Rect, "events");
            var grade = UiWidgets.Stretch("Backdrop_Grade", Rect, Color.white);
            grade.raycastTarget = false; grade.sprite = UiTex.VGradient(Hex("#14161e"), Hex("#0a0b0f"), 64); // #0a0b0f→#14161e matte
            // Warm top glow (#2a2416 additive) — faint.
            UiWidgets.Glow(Rect, UiTheme.A(TopGlowCol, 0.5f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 60), new Vector2(1900, 700), 1.6f);
            UiWidgets.Vignette(Rect, 0.55f); // dark field vignette
            // Dust motes in the dark field (Section J).
            var motes = UiWidgets.Rect("DustMotes", Rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1900, 1000));
            var mf = motes.gameObject.AddComponent<EmberField>(); mf.count = 12; mf.color = UiTheme.A(Hex("#cdbf99"), 0.5f);

            // ================================= TopBar (Back · TitleBlock · CurrencyChips) =================================
            // Back top-left (task-mandated signature) on SafeContent.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Title "EVENTS" — gold-bevel serif, centred, cap ≈64 → fy ≈0.030 (TitleBlock at top, cap 0.055·H). anchorY 0.965.
            UiWidgets.TitleLabel(SafeContent, "EVENTS", 64, new Vector2(0.5f, 0.965f), new Vector2(0.5f, 0.965f),
                Vector2.zero, new Vector2(900, 86), TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);
            // Subtitle — below title (≈0.026·H), parchment-gold (Section A/M verbatim).
            UiWidgets.Label(SafeContent, "Limited-time events with epic rewards!", 24,
                new Vector2(0.5f, 0.915f), new Vector2(0.5f, 0.915f), Vector2.zero, new Vector2(1000, 32), TextAnchor.MiddleCenter, SubGold);

            // Currency chips top-right — Gold rightmost (idx 0) "128,450", Gems left of it (idx 1) "2,850".
            // ⚠️ Section L NEGATIVE RULE: the EnergyChip (58/120) is CANON-CUT — recorded in the spec, OMITTED here (no 3rd chip).
            BuildChip(EmblemGold, "128,450", 0, UiTheme.Gold);     // gold
            BuildChip(RingBlue,   "2,850",   1, UiTheme.AmethystHi); // gems

            // ===================================== FeaturedBanner (ornate framed wide panel) =====================================
            // width 0.95·W ≈2223, height 0.30·H ≈324, top y ≈0.13·H → centre fy = 0.13 + 0.15 = 0.28 → anchorY 0.72.
            BuildFeaturedBanner();

            // ========================================= MoreEventsDivider (rule · text · rule) =========================================
            // fy ≈0.46 → anchorY 0.54.
            BuildMoreEventsDivider();

            // ============================================ EventCardRow (HLG, 4 cards) ============================================
            // band fy 0.49→0.90 → centre fy 0.695 → anchorY 0.305; height 0.41·H ≈443, width 0.95·W.
            BuildCardRow();

            // ============================================ TabBar (bottom, 3 tabs) ============================================
            // height 0.085·H ≈92, bottom inset 0.01·H → centre fy ≈0.9475 → anchorY 0.0525.
            BuildTabBar();
        }

        // -------------------------------------------------------------------------------------------------
        // Currency chip — gold/gems pill (top-right). EnergyChip CUT (§L). Display-only "+" → Store toast.
        // -------------------------------------------------------------------------------------------------
        private void BuildChip(Color iconColor, string value, int chipIndex, Color iconBody)
        {
            float w = 280f, gap = 16f;
            float x = -(20f + w * 0.5f) - chipIndex * (w + gap);
            var rt = UiWidgets.Rect("Chip_" + chipIndex, SafeContent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(x, -64f), new Vector2(w, 80f));
            var bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UiTex.VGradient(Hex("#1a1c26"), Hex("#0c0e15"), 32); bg.color = new Color(1, 1, 1, 0.92f);
            var rim = UiWidgets.Rect("ChipRim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false;
            rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.55f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 48, 5); rimImg.type = Image.Type.Sliced;
            // Icon disc (gold coin / gem stand-in).
            var iconRt = UiWidgets.Rect("Icon", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(52, 52));
            var iconImg = iconRt.gameObject.AddComponent<Image>(); iconImg.raycastTarget = false;
            iconImg.sprite = chipIndex == 1 ? UiTex.Diamond(iconBody, 48) : UiTex.Disc(iconBody, 48);
            // Value (white, SemiBold ~26 → 40px legacy).
            UiWidgets.Label(rt, value, 40, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(90, 0), new Vector2(170, 64), TextAnchor.MiddleLeft, Color.white);
            // "+" → Store deep-link (display-only).
            UiWidgets.Button(rt, "+", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-34, 0), new Vector2(56, 56), new Color(0.2f, 0.7f, 0.25f), () => Router.Toast("Store — coming soon"), 40);
        }

        // -------------------------------------------------------------------------------------------------
        // FeaturedBanner — ornate gold frame · battlefield art · FEATURED ribbon · 2-line title · subtitle ·
        // timer pill · glowing silver 2× emblem. Quasi-button (tap → featured event; display-only toast).
        // -------------------------------------------------------------------------------------------------
        private void BuildFeaturedBanner()
        {
            float bw = 0.95f * W, bh = 0.30f * H; // ≈2223 × 324

            // Group owns a CanvasGroup for the OnShow scale+fade.
            var grpGo = new GameObject("FeaturedBanner", typeof(RectTransform), typeof(CanvasGroup));
            var grp = (RectTransform)grpGo.transform; grp.SetParent(SafeContent, false);
            grp.anchorMin = grp.anchorMax = new Vector2(0.5f, 0.72f); grp.sizeDelta = new Vector2(bw, bh); grp.anchoredPosition = Vector2.zero;
            _bannerCg = grpGo.GetComponent<CanvasGroup>();

            // Whole-banner button (tap → featured detail; idle hover brighten via Button colors).
            var hit = grpGo.AddComponent<Image>();
            hit.sprite = UiTex.VGradient(Hex("#141821"), Hex("#0c0f16"), 32); // banner base under the art
            var btn = grpGo.AddComponent<Button>(); btn.targetGraphic = hit;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.04f, 1.04f, 1.04f, 1f); cb.pressedColor = new Color(0.97f, 0.97f, 0.99f, 1f); cb.fadeDuration = 0.1f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnFeaturedTap(); });

            // BannerArt (left/full) — rainy battlefield/wagon scene (cool-blue ambient gradient stand-in), darkened toward edges.
            var art = UiWidgets.Rect("BannerArt", grp, Vector2.zero, new Vector2(0.62f, 1f), Vector2.zero, Vector2.zero);
            var artImg = art.gameObject.AddComponent<Image>(); artImg.raycastTarget = false;
            artImg.sprite = UiTex.VGradient(Hex("#2a3650"), Hex("#0e1118"), 64); // rain-lit cool battlefield
            art.gameObject.AddComponent<KenBurns>().duration = 9f; // subtle ken-burns drift (Section I)
            // Inner vignette over the art so the title reads (right side fades to the dark base).
            var artFade = UiWidgets.Rect("Art_Fade", grp, Vector2.zero, new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            var afImg = artFade.gameObject.AddComponent<Image>(); afImg.raycastTarget = false;
            afImg.sprite = UiTex.HGradient(UiTheme.A(Hex("#0c0f16"), 0f), UiTheme.A(Hex("#0c0f16"), 0.85f), 64);

            // Ornate gold/bronze frame around the whole banner (≈0.012·H thick).
            var frame = UiWidgets.Rect("Banner_Frame", grp, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var frImg = frame.gameObject.AddComponent<Image>(); frImg.raycastTarget = false;
            frImg.sprite = UiTex.Frame(Hex("#fff2c2"), UiTheme.Gold, Hex("#6b5320"), 64, 14); frImg.type = Image.Type.Sliced;
            UiWidgets.Finial(grp, new Vector2(0f, 0.5f), new Vector2(2, 0), 30f);
            UiWidgets.Finial(grp, new Vector2(1f, 0.5f), new Vector2(-2, 0), 30f);

            // FeaturedRibbon "FEATURED" — cobalt corner tab top-left (≈0.10·W × 0.045·H).
            var ribbon = UiWidgets.Rect("FeaturedRibbon", grp, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.05f * W * 0.5f + 16, -16), new Vector2(0.10f * W, 0.045f * H));
            var ribImg = ribbon.gameObject.AddComponent<Image>(); ribImg.raycastTarget = false;
            ribImg.sprite = UiTex.VGradient(RibbonBlue, Hex("#1f356f"), 32); // cobalt cloth/metal tab
            var ribRim = UiWidgets.Rect("RibbonRim", ribbon, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ribRimImg = ribRim.gameObject.AddComponent<Image>(); ribRimImg.raycastTarget = false; ribRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 4); ribRimImg.type = Image.Type.Sliced;
            UiWidgets.Label(ribbon, UiTheme.Track("FEATURED"), 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, RibbonText);

            // BannerTitle "DOUBLE / SILVER WEEKEND" — 2-line silvery-gold serif (line1 ~58, line2 ~64), centre-left.
            var titleBlock = UiWidgets.Rect("BannerTitleBlock", grp, new Vector2(0.30f, 0.5f), new Vector2(0.30f, 0.5f), new Vector2(0, 18), new Vector2(820, 240));
            var t1 = UiWidgets.TitleLabel(titleBlock, "DOUBLE", 84, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(820, 120), TextAnchor.MiddleCenter, BannerSilA, BannerSilB);
            t1.gameObject.GetComponent<Outline>().effectColor = UiTheme.A(BannerStrk, 0.95f);
            var t2 = UiWidgets.TitleLabel(titleBlock, "SILVER WEEKEND", 96, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(840, 130), TextAnchor.MiddleCenter, BannerSilA, BannerSilB);
            t2.gameObject.GetComponent<Outline>().effectColor = UiTheme.A(BannerStrk, 0.95f);
            // Title bloom behind it.
            UiWidgets.Glow(grp, UiTheme.A(BannerSilA, 0.18f), new Vector2(0.30f, 0.5f), new Vector2(0.30f, 0.5f), new Vector2(0, 18), new Vector2(900, 280), 1.7f).transform.SetAsFirstSibling();

            // BannerSubtitle "Earn DOUBLE the Silver in all battles!" — below title.
            UiWidgets.Label(grp, "Earn DOUBLE the Silver in all battles!", 26, new Vector2(0.30f, 0.5f), new Vector2(0.30f, 0.5f), new Vector2(0, -78), new Vector2(820, 34), TextAnchor.MiddleCenter, SubGold);

            // BannerTimerPill {clock, "Ends in 1d 12h"} — low-centre (≈0.14·W × 0.045·H).
            BuildTimerPill(grp, new Vector2(0.30f, 0.5f), new Vector2(0, -132), new Vector2(0.14f * W, 0.05f * H), "Ends in 1d 12h", PillText, 22);

            // MultiplierEmblem "2×" — right zone: silver lion coin + cobalt energy ring + glow + "2×".
            BuildMultiplierEmblem(grp);
        }

        // Silver lion coin in a cobalt energy ring with bloom + "2×" (Section C/G). The brightest focal object.
        private void BuildMultiplierEmblem(RectTransform banner)
        {
            float dia = 0.22f * H; // coin/ring ⌀ ≈238
            var emblem = UiWidgets.Rect("MultiplierEmblem", banner, new Vector2(0.86f, 0.5f), new Vector2(0.86f, 0.5f), Vector2.zero, new Vector2(dia * 1.5f, dia * 1.5f));

            // Outer cobalt bloom halo (extends ~+30%).
            var bloom = UiWidgets.Glow(emblem, UiTheme.A(RingBlue, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia * 1.45f, dia * 1.45f), 1.6f);
            var bp = bloom.gameObject.AddComponent<PulseGraphic>(); bp.target = bloom; bp.min = 0.45f; bp.max = 0.75f; bp.period = 2f; // glow pulse (2s)

            // Cobalt energy ring (slow rotation + arc-spark stand-in via Frame ring).
            var ring = UiWidgets.Rect("EnergyRing", emblem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia, dia));
            var ringImg = ring.gameObject.AddComponent<Image>(); ringImg.raycastTarget = false;
            ringImg.sprite = UiTex.Frame(UiTheme.A(RingBlue, 0f), UiTheme.A(RingBlue, 0.9f), UiTheme.A(RibbonBlue, 0f), 64, 12); ringImg.type = Image.Type.Sliced;
            ring.gameObject.AddComponent<Spin>().degPerSec = -40f; // ring slow rotation
            // Arc sparks twinkling around the ring.
            var sparks = UiWidgets.Rect("RingSparks", emblem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia * 0.95f, dia * 0.95f));
            var sf = sparks.gameObject.AddComponent<EmberField>(); sf.count = 10; sf.color = Hex("#bcd6ff");

            // Silver lion coin disc (high specular stand-in).
            var coin = UiWidgets.Rect("SilverCoin", emblem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia * 0.72f, dia * 0.72f));
            var coinImg = coin.gameObject.AddComponent<Image>(); coinImg.raycastTarget = false; coinImg.sprite = UiTex.Disc(SilverCoinB, 64);
            var coinFace = UiWidgets.Rect("CoinFace", coin, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia * 0.58f, dia * 0.58f));
            var faceImg = coinFace.gameObject.AddComponent<Image>(); faceImg.raycastTarget = false; faceImg.sprite = UiTex.VGradient(SilverCoinA, SilverCoinB, 64);
            // Lion glyph stand-in (gold diamond emblem on the coin face).
            var lion = UiWidgets.Rect("LionGlyph", coinFace, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia * 0.30f, dia * 0.30f));
            var lionImg = lion.gameObject.AddComponent<Image>(); lionImg.raycastTarget = false; lionImg.sprite = UiTex.Diamond(Hex("#8b8f99"), 48);

            // "2×" — huge gold serif over the right edge (cap ≈0.16·H → ~120px legacy), gold bevel + cobalt glow.
            UiWidgets.Glow(emblem, UiTheme.A(EmblemGold, 0.35f), new Vector2(0.78f, 0.42f), new Vector2(0.78f, 0.42f), Vector2.zero, new Vector2(220, 220), 1.7f);
            var two = UiWidgets.TitleLabel(emblem, "2×", 150, new Vector2(0.78f, 0.42f), new Vector2(0.78f, 0.42f), Vector2.zero, new Vector2(260, 200), TextAnchor.MiddleCenter, Hex("#fff2c2"), UiTheme.Gold, false);
            two.gameObject.GetComponent<Outline>().effectColor = UiTheme.A(Hex("#3a2c0e"), 0.95f);
        }

        // -------------------------------------------------------------------------------------------------
        // MoreEventsDivider — centred "MORE EVENTS" flanked by gold rule lines.
        // -------------------------------------------------------------------------------------------------
        private void BuildMoreEventsDivider()
        {
            // Left rule wipes outward (fade toward centre → gold at the outer end via the Divider gradient direction).
            UiWidgets.Divider(SafeContent, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(-560, 0), 760, 3f, UiTheme.A(UiTheme.Gold, 0.85f));
            UiWidgets.Divider(SafeContent, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(560, 0), 760, 3f, UiTheme.A(UiTheme.Gold, 0.85f));
            UiWidgets.Label(SafeContent, UiTheme.Track("MORE EVENTS"), 24, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), Vector2.zero, new Vector2(420, 34), TextAnchor.MiddleCenter, MoreEvents);
        }

        // -------------------------------------------------------------------------------------------------
        // EventCardRow — HorizontalLayoutGroup, 4 equal cards (ScrollRect-ready). Banner/divider/tabs stay pinned.
        // -------------------------------------------------------------------------------------------------
        private void BuildCardRow()
        {
            float rowW = 0.95f * W, rowH = 0.41f * H; // ≈2223 × 443

            // Display-only event payload — exactly the featured banner + 4 cards (no extras, §L).
            var events = new[]
            {
                new EventDef{
                    Name="Endless Rush", Desc="Survive endless waves of\nenemies and climb the leaderboard!", Timer="Ends in 1d 12h",
                    ArtA=Hex("#5a2417"), ArtB=Hex("#1a0f0a"), New=true, EndingSoon=false,
                    R0=Reward.Trophy, R1=Reward.Shard, R2=Reward.Chest,
                    // Endless is a real battle mode in BULWARK → route through the existing MatchPresentation seam (presentation-only).
                    Go=() => MatchPresentation.StartMatch("Event") },
                new EventDef{
                    Name="Hero Trials", Desc="Win with fixed heroes and\nearn exclusive hero shards!", Timer="Ends in 2d 12h",
                    ArtA=Hex("#23304f"), ArtB=Hex("#0d1118"), New=false, EndingSoon=false,
                    R0=Reward.Trophy, R1=Reward.Shard, R2=Reward.Chest,
                    Go=() => Router.Toast("Hero Trials — coming soon") },
                new EventDef{
                    Name="Resource Run", Desc="Gather as many resources as\nyou can before time runs out!", Timer="Ends in 12h 45m",
                    ArtA=Hex("#4a3a16"), ArtB=Hex("#16110a"), New=false, EndingSoon=true, // <threshold → ember timer (Section H)
                    R0=Reward.Coin, R1=Reward.Gem, R2=Reward.Chest,
                    Go=() => Router.Toast("Resource Run — coming soon") },
                new EventDef{
                    // ⚠️ Section L: art reads "real-time" but BULWARK has NO real-time PvP → relabel ASYNC ghost; route async (32).
                    Name="Arena Clash", Desc="Compete against other Commanders\nin async ghost battles for ranked rewards!", Timer="Ends in 3d 12h",
                    ArtA=Hex("#4a1814"), ArtB=Hex("#160a09"), New=false, EndingSoon=false,
                    R0=Reward.Rank, R1=Reward.Shard, R2=Reward.Chest,
                    Go=() => Router.Toast("Arena Clash (async ghost) — coming soon") },
            };

            var rowGo = new GameObject("EventCardRow", typeof(RectTransform));
            var row = (RectTransform)rowGo.transform; row.SetParent(SafeContent, false);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.305f); row.sizeDelta = new Vector2(rowW, rowH); row.anchoredPosition = Vector2.zero;
            var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0.015f * W; // ≈35 px gap
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            _cardCg = new CanvasGroup[events.Length];
            for (int i = 0; i < events.Length; i++) BuildCard(row, events[i], i);
        }

        private void BuildCard(RectTransform row, EventDef e, int index)
        {
            // Equal cell (HLG-sized). Card root carries an Image panel + a CanvasGroup for the stagger-in.
            var cardGo = new GameObject("Card_" + e.Name.Replace(" ", ""), typeof(RectTransform), typeof(CanvasGroup));
            var card = (RectTransform)cardGo.transform; card.SetParent(row, false);
            var le = cardGo.AddComponent<LayoutElement>(); le.flexibleWidth = 1f; le.flexibleHeight = 1f; le.minWidth = 300f; // card width ≈530 at 2340
            _cardCg[index] = cardGo.GetComponent<CanvasGroup>();

            // Panel (vertical dark slate #10131c→#181c27) + bronze edge + soft inner top sheen.
            var panel = cardGo.AddComponent<Image>(); panel.sprite = UiTex.VGradient(CardTop, CardBot, 64);
            var pBtn = cardGo.AddComponent<Button>(); pBtn.targetGraphic = panel; // card lift / press (whole-card focus)
            var pcb = pBtn.colors; pcb.normalColor = Color.white; pcb.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f); pcb.pressedColor = new Color(0.96f, 0.96f, 0.98f, 1f); pcb.fadeDuration = 0.08f; pBtn.colors = pcb;
            var go = e.Go; pBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); go?.Invoke(); });
            var rim = UiWidgets.Rect("Card_Edge", card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false;
            rimImg.sprite = UiTex.Frame(Hex("#caa04a"), Hex("#8a6f30"), Hex("#4a3a18"), 48, 5); rimImg.type = Image.Type.Sliced; // bronze edge

            // Internal frac rows (fractions of card H, Section E). anchorY = 1 − fy.
            // Art 0.00–0.34 → top band of the card.
            var art = UiWidgets.Rect("Art", card, new Vector2(0.04f, 0.66f), new Vector2(0.96f, 0.985f), Vector2.zero, Vector2.zero);
            var artImg = art.gameObject.AddComponent<Image>(); artImg.raycastTarget = false; artImg.sprite = UiTex.VGradient(e.ArtA, e.ArtB, 64);
            // Bottom gradient fade of the art into the panel.
            var artFade = UiWidgets.Rect("Art_Fade", art, new Vector2(0, 0), new Vector2(1, 0.5f), Vector2.zero, Vector2.zero);
            var afImg = artFade.gameObject.AddComponent<Image>(); afImg.raycastTarget = false; afImg.sprite = UiTex.VGradient(UiTheme.A(CardBot, 0.9f), UiTheme.A(CardBot, 0f), 32);
            // Subtle floating embers/dust per theme (restrained, Section J).
            var emb = UiWidgets.Rect("Art_Embers", art, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(360, 200));
            var ef = emb.gameObject.AddComponent<EmberField>(); ef.count = 5; ef.color = UiTheme.A(UiWidgets.Lighten(e.ArtA, 0.3f), 0.6f);

            // NewRibbon (Endless Rush) — top-left of Art (≈0.30·cardW × 0.07·cardH), blue/gold tag.
            if (e.New)
            {
                var nr = UiWidgets.Rect("NewRibbon", art, new Vector2(0, 1), new Vector2(0, 1), new Vector2(58, -22), new Vector2(112, 40));
                var nrImg = nr.gameObject.AddComponent<Image>(); nrImg.raycastTarget = false; nrImg.sprite = UiTex.VGradient(UiWidgets.Lighten(NewRibbonCol, 0.2f), UiWidgets.Darken(NewRibbonCol, 0.2f), 32);
                var nrRim = UiWidgets.Rect("NewRim", nr, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var nrRimImg = nrRim.gameObject.AddComponent<Image>(); nrRimImg.raycastTarget = false; nrRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 4); nrRimImg.type = Image.Type.Sliced;
                UiWidgets.Label(nr, UiTheme.Track("NEW"), 18, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
                nr.gameObject.AddComponent<PulseScale>().period = 1.8f; // NEW ribbon shimmer/pop
            }

            // Name 0.36–0.44 — gold serif small-caps (cap ≈30).
            UiWidgets.TitleLabel(card, e.Name, 34, new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.60f), Vector2.zero, new Vector2(500, 40), TextAnchor.MiddleCenter, Hex("#f6ecd2"), Hex("#caa04a"));

            // Desc 0.45–0.60 (2 lines) — clean quiet sans, wrap. Use a wrapping Text (not the Overflow Label).
            BuildWrapText(card, e.Desc, 20, new Vector2(0.06f, 0.42f), new Vector2(0.94f, 0.555f), CardDesc);

            // RewardsRow 0.62–0.74 — "REWARDS" label + 3 icons (⌀ ≈0.07·cardW).
            var rewards = UiWidgets.Rect("RewardsRow", card, new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(480, 70));
            UiWidgets.Label(rewards, UiTheme.Track("REWARDS"), 16, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 2), new Vector2(300, 22), TextAnchor.MiddleCenter, RewardsLbl);
            float ic = 0.072f * 530f; // ≈38 icon ⌀
            float gap = 22f, startX = -(ic + gap);
            BuildRewardIcon(rewards, e.R0, new Vector2(startX, -14), ic);
            BuildRewardIcon(rewards, e.R1, new Vector2(0, -14), ic);
            BuildRewardIcon(rewards, e.R2, new Vector2(-startX, -14), ic);

            // PlayButton 0.76–0.90 — blue gloss CTA (width ≈0.84·cardW, height ≈0.12·cardH).
            BuildPlayButton(card, e);

            // Timer 0.92–1.00 — clock glyph + "Ends in …". Ending-soon → ember + pulse (Section H).
            var timerCol = e.EndingSoon ? EmberWarn : SubGold;
            var timer = UiWidgets.Rect("Timer", card, new Vector2(0.5f, 0.045f), new Vector2(0.5f, 0.045f), Vector2.zero, new Vector2(360, 34));
            var clk = UiWidgets.Rect("ClockGlyph", timer, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(26, 26));
            var clkImg = clk.gameObject.AddComponent<Image>(); clkImg.raycastTarget = false; clkImg.sprite = UiTex.Disc(UiTheme.A(timerCol, 0.85f), 32);
            var tLbl = UiWidgets.Label(timer, e.Timer, 18, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(64, 0), new Vector2(280, 28), TextAnchor.MiddleLeft, timerCol);
            if (e.EndingSoon) { var tp = timer.gameObject.AddComponent<PulseGraphic>(); tp.target = tLbl; tp.min = 0.6f; tp.max = 1f; tp.period = 1.2f; }
        }

        // Blue gloss PLAY CTA + outer glow + breathe (Section G/H). Display-only / seam route via card model.
        private void BuildPlayButton(RectTransform card, EventDef e)
        {
            var play = UiWidgets.Rect("PlayButton", card, new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), Vector2.zero, new Vector2(0.84f * 530f, 0.12f * 443f));
            // Outer cobalt glow (breathe).
            var glow = UiWidgets.Glow(play, UiTheme.A(PlayBlueA, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.84f * 530f * 1.2f, 80f), 1.7f);
            var gp = glow.gameObject.AddComponent<PulseGraphic>(); gp.target = glow; gp.min = 0.35f; gp.max = 0.65f; gp.period = 1.6f;
            // Body (royal/cobalt gloss).
            var body = play.gameObject.AddComponent<Image>(); body.sprite = UiTex.VGradient(PlayBlueA, PlayBlueB, 32);
            var btn = play.gameObject.AddComponent<Button>(); btn.targetGraphic = body;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.88f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            var go = e.Go; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); go?.Invoke(); });
            // Beveled gold/steel trim.
            var rim = UiWidgets.Rect("PlayRim", play, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false; rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); rimImg.type = Image.Type.Sliced;
            // Top highlight band.
            var hi = UiWidgets.Rect("PlayHi", play, new Vector2(0, 0.55f), new Vector2(1, 1), Vector2.zero, new Vector2(-12, 0));
            var hiImg = hi.gameObject.AddComponent<Image>(); hiImg.raycastTarget = false; hiImg.sprite = UiTex.VGradient(UiTheme.A(Color.white, 0.25f), UiTheme.A(Color.white, 0f), 32);
            // Label "PLAY" — heavy UPPERCASE white on blue + glow.
            var lbl = UiWidgets.Label(play, UiTheme.Track("PLAY"), 30, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, RibbonText);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#0e1a3a"), 0.9f);
        }

        // A single reward icon (tier-coloured diamond/disc + bloom). Display-only tier stand-in.
        private void BuildRewardIcon(RectTransform parent, Reward r, Vector2 pos, float dia)
        {
            Color c; bool diamond;
            switch (r)
            {
                case Reward.Trophy: c = Hex("#f0c14a"); diamond = false; break; // trophy gold
                case Reward.Shard:  c = Hex("#d8742b"); diamond = true;  break; // glowing crystal shard (orange)
                case Reward.Chest:  c = Hex("#a0682c"); diamond = false; break; // aged-wood + bronze chest
                case Reward.Coin:   c = Hex("#e8d59a"); diamond = false; break; // gold/silver coin
                case Reward.Gem:    c = Hex("#9e6bf0"); diamond = true;  break; // cobalt/violet gem
                default:            c = Hex("#7a3fd0"); diamond = true;  break; // rank-gem (violet)
            }
            var icon = UiWidgets.Rect("Reward", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(dia, dia));
            UiWidgets.Glow(icon, UiTheme.A(c, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(dia * 1.5f, dia * 1.5f), 1.6f); // slight bloom (twinkle)
            var img = icon.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = diamond ? UiTex.Diamond(c, 48) : UiTex.Disc(c, 48);
            var tw = icon.gameObject.AddComponent<PulseGraphic>(); tw.target = img; tw.min = 0.78f; tw.max = 1f; tw.period = 2.4f; // slow twinkle
        }

        // -------------------------------------------------------------------------------------------------
        // TimerPill — dark translucent plate + bronze edge + clock glyph + gold text (banner + reused style).
        // -------------------------------------------------------------------------------------------------
        private void BuildTimerPill(RectTransform parent, Vector2 anchor, Vector2 pos, Vector2 size, string text, Color textCol, int fontSize)
        {
            var pill = UiWidgets.Rect("BannerTimerPill", parent, anchor, anchor, pos, size);
            var bg = pill.gameObject.AddComponent<Image>(); bg.raycastTarget = false; bg.color = UiTheme.A(PillBg, 0.8f);
            var rim = UiWidgets.Rect("PillRim", pill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false; rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.6f), UiTheme.A(UiTheme.Gold, 0.5f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 48, 4); rimImg.type = Image.Type.Sliced;
            var clk = UiWidgets.Rect("ClockGlyph", pill, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(34, 0), new Vector2(28, 28));
            var clkImg = clk.gameObject.AddComponent<Image>(); clkImg.raycastTarget = false; clkImg.sprite = UiTex.Disc(UiTheme.A(textCol, 0.9f), 32);
            UiWidgets.Label(pill, text, fontSize, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(20, 0), new Vector2(-20, 0), TextAnchor.MiddleCenter, textCol);
        }

        // -------------------------------------------------------------------------------------------------
        // TabBar (bottom toggle group): Events(active gold + underline) · Calendar · Past Events (muted).
        // Tab switch = body cross-fade (banner+cards swap) — header+tabs persist (§L). Display-only here.
        // -------------------------------------------------------------------------------------------------
        private void BuildTabBar()
        {
            float barH = 0.085f * H; // ≈92
            var bar = UiWidgets.Rect("TabBar", SafeContent, new Vector2(0.5f, 0.0525f), new Vector2(0.5f, 0.0525f), Vector2.zero, new Vector2(0.95f * W, barH));
            var barImg = bar.gameObject.AddComponent<Image>(); barImg.raycastTarget = false; barImg.sprite = UiTex.VGradient(Hex("#161a24"), Hex("#0c0e15"), 32); // dark slate
            // Bronze top edge.
            UiWidgets.Divider(bar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, 0.95f * W, 3f, UiTheme.A(UiTheme.Gold, 0.5f));

            string[] tabs = { "EVENTS", "CALENDAR", "PAST EVENTS" };
            float third = (0.95f * W) / 3f;
            for (int i = 0; i < tabs.Length; i++)
            {
                int idx = i;
                bool active = i == 0;
                float cx = -0.95f * W * 0.5f + third * (i + 0.5f);
                var tab = UiWidgets.Rect("Tab_" + tabs[i].Replace(" ", ""), bar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cx, 0), new Vector2(third - 10, barH));
                var tImg = tab.gameObject.AddComponent<Image>(); tImg.color = new Color(0, 0, 0, 0.001f); // raycast surface
                var tBtn = tab.gameObject.AddComponent<Button>(); tBtn.targetGraphic = tImg;
                var cb = tBtn.colors; cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f); cb.fadeDuration = 0.08f; tBtn.colors = cb;
                tBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnTab(idx); });

                Color glyphCol = active ? TabActive : TabIdle;
                // Glyph above label (book/scroll · calendar · clock-history stand-in).
                var gly = UiWidgets.Rect("Glyph", tab, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-86, 4), new Vector2(0.05f * H, 0.05f * H));
                var gImg = gly.gameObject.AddComponent<Image>(); gImg.raycastTarget = false;
                gImg.sprite = i == 1 ? UiTex.Frame(glyphCol, glyphCol, UiTheme.A(glyphCol, 0.4f), 48, 8) : UiTex.Diamond(glyphCol, 48);
                if (i == 1) gImg.type = Image.Type.Sliced;
                if (active) { var lbl = UiWidgets.Label(tab, UiTheme.Track(tabs[i]), 22, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(26, 0), new Vector2(300, 30), TextAnchor.MiddleLeft, TabActive); lbl.gameObject.AddComponent<UiGradientText>(); }
                else UiWidgets.Label(tab, UiTheme.Track(tabs[i]), 22, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(26, 0), new Vector2(300, 30), TextAnchor.MiddleLeft, TabIdle);

                // Active tab gold underline (≈0.4·tabW × 4) + faint glow (breathe).
                if (active)
                {
                    _tabUnderline = UiWidgets.Rect("ActiveUnderline", tab, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(0.4f * third, 4f));
                    var uImg = _tabUnderline.gameObject.AddComponent<Image>(); uImg.raycastTarget = false; uImg.sprite = UiTex.HGradient(UiTheme.A(UiTheme.GoldHi, 0.2f), UiTheme.GoldHi, 32);
                    var ug = UiWidgets.Glow(tab, UiTheme.A(TabActive, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(third * 0.7f, barH), 1.7f);
                    var ugp = ug.gameObject.AddComponent<PulseGraphic>(); ugp.target = ug; ugp.min = 0.2f; ugp.max = 0.45f; ugp.period = 2f;
                }
            }
        }

        // -------------------------------------------------------------------------------------------------
        // EVENT BEHAVIOR (§K) — all display-only. Client NEVER authors events / mutates balance (§12/§L).
        // -------------------------------------------------------------------------------------------------
        private void OnFeaturedTap() => Router.Toast("Double Silver Weekend — featured event");

        private void OnTab(int idx)
        {
            // Section L: Calendar/Past Events are body SWAPS (cross-fade cards out/in) — NOT new screens.
            // The alternate bodies are not in this forensic frame (Section N) → display-only toast stub.
            if (idx == 0) return; // already Events (active)
            Router.Toast(idx == 1 ? "Calendar — coming soon" : "Past Events — coming soon");
        }

        // A wrapping (non-overflow) Text for the 2-line card description. Mirrors UiWidgets.Label but wraps.
        private void BuildWrapText(Transform parent, string text, int size, Vector2 aMin, Vector2 aMax, Color col)
        {
            var go = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = UiWidgets.Font; t.text = text; t.fontSize = size; t.alignment = TextAnchor.UpperCenter; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.lineSpacing = 1.05f;
            t.raycastTarget = false;
            var sh = go.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.7f); sh.effectDistance = new Vector2(1, -1);
        }

        // -------------------------------------------------------------------------------------------------
        // Lifecycle / enter timeline (Section I, ~0.85s).
        // -------------------------------------------------------------------------------------------------
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterTimeline());
        }

        private IEnumerator EnterTimeline()
        {
            // 0.12 FeaturedBanner: scale 0.97→1.00 + α (0.30s ease-out).
            if (_bannerCg != null)
            {
                _bannerCg.alpha = 0f; var tr = _bannerCg.transform; float t = 0f; const float d = 0.30f;
                while (t < d && _bannerCg != null)
                {
                    t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                    float e = 1f - (1f - k) * (1f - k); // ease-out
                    _bannerCg.alpha = k; tr.localScale = Vector3.one * Mathf.Lerp(0.97f, 1f, e);
                    yield return null;
                }
                if (_bannerCg != null) { _bannerCg.alpha = 1f; tr.localScale = Vector3.one; }
            }

            // 0.52 EventCardRow: cards stagger L→R (each y +18→0 + α, 0.05s apart, 0.18s each, ease-out).
            if (_cardCg != null)
            {
                for (int i = 0; i < _cardCg.Length; i++)
                {
                    if (_cardCg[i] == null) continue;
                    StartCoroutine(CardIn(i));
                    float s = 0f; while (s < 0.05f) { s += Time.unscaledDeltaTime; yield return null; }
                }
            }

            // 0.80 TabBar underline draw-in (0.18s).
            if (_tabUnderline != null)
            {
                float full = _tabUnderline.sizeDelta.x; var p = _tabUnderline.sizeDelta; p.x = 0f; _tabUnderline.sizeDelta = p;
                float t = 0f; const float d = 0.18f;
                while (t < d && _tabUnderline != null)
                {
                    t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                    var s = _tabUnderline.sizeDelta; s.x = Mathf.Lerp(0f, full, k); _tabUnderline.sizeDelta = s;
                    yield return null;
                }
                if (_tabUnderline != null) { var s = _tabUnderline.sizeDelta; s.x = full; _tabUnderline.sizeDelta = s; }
            }
        }

        // Cards live inside a HorizontalLayoutGroup (it drives anchoredPosition), so the "y +18→0" rise is
        // expressed HLG-safely as a localPosition.y offset on the cell transform (the layout sets anchoredPosition,
        // not localPosition.y directly each frame) plus a small scale settle — alpha carries the fade.
        private IEnumerator CardIn(int i)
        {
            var cg = _cardCg[i]; if (cg == null) yield break;
            var tr = cg.transform; Vector3 baseScale = Vector3.one;
            cg.alpha = 0f; tr.localScale = baseScale * 0.96f;
            Vector3 home = tr.localPosition; Vector3 from = home + new Vector3(0f, -18f, 0f);
            tr.localPosition = from;
            float t = 0f; const float d = 0.18f;
            while (t < d && cg != null)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                float e = 1f - (1f - k) * (1f - k); // ease-out
                cg.alpha = k; tr.localScale = baseScale * Mathf.Lerp(0.96f, 1f, e);
                tr.localPosition = Vector3.Lerp(from, home, e);
                yield return null;
            }
            if (cg != null) { cg.alpha = 1f; tr.localScale = baseScale; tr.localPosition = home; }
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
