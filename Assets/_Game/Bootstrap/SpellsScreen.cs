// BULWARK — SPELLS (UI Construction Bible · 18). Presentation-only, REMOVABLE.
//
// Forensic build of design/SpellsScreenDesign.png per Sections A–O: a tabbed SHOP HUB (Back top-left, tab bar
// STORE·SPELLS·SKINS·CHESTS top-center with SPELLS gold-lit, Gems+Gold currency chips top-right) over a dim
// wizard-study backdrop; a hooded mage presenter bottom-left with a cyan staff orb; four crystal spell orbs on
// a royal-blue crowned table each holding a colored sigil + amethyst gem price (pickaxe 150 / flaming-sword 200
// / tridents 250 / shield 300); and a right parchment scroll (MINER'S BLESSING + pickaxe icon + forensic desc/
// stats/flavor) with the brightest CTA, a violet gold-edged BUY · 150. Spell list is clearly-display-only local
// stub data; BUY is display-only (UiStub.TrySpendGems + Router.Toast — UI never mutates a real balance). The ADR
// flag (premium-gem buffs vs "gems never buy power") is noted; spec reproduced as drawn. NO ECS/gameplay (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-18 landscape Spells shop tab. Presentation-only (display-only catalog; no balance/ECS, §12).</summary>
    public sealed class SpellsScreen : UiScreen
    {
        // ---- Display-only spell catalog (NOT gameplay/economy — layout/flow data only; replaced by a server-
        //      authoritative ShopService at GATE-3). Copy/prices/sigil colours are forensic, per spec §L. ----
        private struct Spell
        {
            public string Name, Desc, Stats, Flavor;
            public int Price;            // gem price (display only)
            public Color Sigil, Glow;    // per-orb sigil colour + additive glow tint (§G)
        }

        private static readonly Spell[] Spells =
        {
            new Spell{ Name="MINER'S BLESSING",
                       Desc="Increases mining speed significantly for a limited time.",
                       Stats="Duration: 30 seconds\nEffect: 2x Mining Speed",
                       Flavor="The earth yields its riches to those blessed by magic.",
                       Price=150, Sigil=Hex("#f0c050"), Glow=Hex("#f0c050") },
            new Spell{ Name="EMBER STRIKE",
                       Desc="Wreathes your blades in fire for a limited time.",
                       Stats="Duration: 20 seconds\nEffect: Flaming Weapons",
                       Flavor="A spark, a word, and steel remembers the forge.",
                       Price=200, Sigil=Hex("#ff5a2a"), Glow=Hex("#ff5a2a") },
            new Spell{ Name="TIDE'S REACH",
                       Desc="Extends the reach of your spears for a limited time.",
                       Stats="Duration: 25 seconds\nEffect: Extended Reach",
                       Flavor="The deep lends its long arms to the worthy.",
                       Price=250, Sigil=Hex("#4fa8ff"), Glow=Hex("#4fa8ff") },
            new Spell{ Name="AEGIS WARD",
                       Desc="Hardens your defenses with a golden ward for a limited time.",
                       Stats="Duration: 30 seconds\nEffect: Damage Reduction",
                       Flavor="Faith raised, a shield unbroken stands.",
                       Price=300, Sigil=Hex("#e8c25a"), Glow=Hex("#e8c25a") },
        };

        // Crystal-glass globe + parchment hexes not in UiTheme (§G).
        private static readonly Color GlassRim  = Hex("#cfe6ff");
        private static readonly Color GlassBody = Hex("#5a86c8");
        private static readonly Color ClothHi   = Hex("#2c47b0");
        private static readonly Color ClothLo   = Hex("#1c2f7a");
        private static readonly Color ParchEdge = Hex("#c2ac74");
        private static readonly Color InkBrown  = Hex("#3a2c14");
        private static readonly Color InkFaded  = Hex("#5a4a2c");
        private static readonly Color TitleWarmTop = Hex("#8a5a1e");
        private static readonly Color TitleWarmBot = Hex("#5a3a12");
        private static readonly Color BuyBody   = Hex("#6a2db8"); // amethyst pill body
        private static readonly Color BuyHi     = Hex("#9e6bf0");
        private static readonly Color BuyText   = Hex("#f6e6b8");
        private static readonly Color CyanOrb   = Hex("#3fd0ff"); // mage staff orb / glowing eyes
        private static readonly Color RobeTop   = Hex("#4a3a7a");
        private static readonly Color RobeLo    = Hex("#2a1d4a");

        private int _selected = 0; // default-select Orb_1 (Miner's Blessing) per §K
        private Text _goldText, _gemText;

        // Detail-scroll content refs (cross-faded on orb select).
        private Text _detailTitle, _detailDesc, _detailStats, _detailFlavor, _buyPrice;
        private Image _detailIcon, _buyPriceGem;
        private RectTransform _scrollInner;
        private readonly Image[] _orbGlows = new Image[4];
        private readonly RectTransform[] _orbRoots = new RectTransform[4];

        protected override void Build()
        {
            // ---- Bg_FullBleed: dim wizard-study (shelves/candles), bleeds under cutout (§C/§D). FX on Rect. ----
            UiWidgets.Backdrop(Rect, "spells");
            UiWidgets.Stretch("StudyDim", Rect, UiTheme.A(Hex("#1a1424"), 0.55f)); // warm-violet candlelit wash
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.16f), new Vector2(0.18f, 0.55f), new Vector2(0.18f, 0.55f), Vector2.zero, new Vector2(1500, 1100), 1.6f); // bookshelf candle ambience
            UiWidgets.Glow(Rect, UiTheme.A(CyanOrb, 0.10f), new Vector2(0.20f, 0.70f), new Vector2(0.20f, 0.70f), Vector2.zero, new Vector2(900, 900), 1.7f);          // mage-staff focal cool key
            var dust = UiWidgets.Rect("DustMotes", Rect, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(2340, 760));
            var dustFx = dust.gameObject.AddComponent<EmberField>(); dustFx.count = 12; dustFx.color = UiTheme.A(UiTheme.ParchGold, 0.5f); // warm drifting study motes (§J)
            UiWidgets.Vignette(Rect, 0.55f);

            // ---- Mage presenter (foreground character, NOT a button) — bottom-left 0,0; x 0.0→0.30, y 0.10→0.95 ----
            BuildMage();

            // ---- Orb table (blue crowned cloth + 4 orbs) — bottom-center 0.5,0; x 0.18→0.66, y 0.0→0.55 ----
            BuildOrbTable();

            // ---- Detail parchment scroll (right) — 1,1; x 0.665→0.985, y 0.085→0.93 ----
            BuildDetailScroll();

            // ---- SHARED CHROME (drawn last → above body; corners stable on notch/ultrawide, §D/§L) ----
            // Currency chips top-right: Gold rightmost (idx 0), Gems left of it (idx 1). Values 48570 / 1726 (§E).
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);
            _gemText  = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);

            // Tab bar top-center (0.44 W ≈ 1030px), active index 1 = SPELLS (gold-lit). Index routing per task.
            UiWidgets.TabBar(SafeContent, new[] { "STORE", "SPELLS", "SKINS", "CHESTS" },
                new Vector2(0.5f, 0.945f), new Vector2(0.5f, 0.945f), Vector2.zero, new Vector2(1030, 84), 1,
                i => { if (i == 0) Router.Replace<StoreScreen>(); else if (i == 2) Router.Replace<SkinsScreen>(); else if (i == 3) Router.Replace<ChestsScreen>(); });

            // Back top-left.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Bind the default selection (Miner's Blessing) into the scroll + orb highlight.
            SelectOrb(_selected, false);
        }

        // ---------------------------------------------------------------- mage presenter ----
        private void BuildMage()
        {
            // Anchor bottom-left, pivot 0,0; rect x 0.0→0.30 W (≈702px), y 0.10→0.95 H (≈918px) — decorative; may bleed.
            var mage = UiWidgets.Rect("MagePresenter", SafeContent, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(702, 918));
            mage.pivot = new Vector2(0f, 0f);
            mage.anchoredPosition = new Vector2(0f, 0.10f * 1080f); // foot at y 0.10 H

            // Mage_Body — hooded robe (deep indigo→violet vertical gradient, gold-trim stand-in), non-raycast.
            var body = UiWidgets.Rect("Mage_Body", mage, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(20f, 0f), new Vector2(420, 840));
            var bimg = body.gameObject.AddComponent<Image>(); bimg.raycastTarget = false;
            bimg.sprite = UiTex.VGradient(RobeTop, RobeLo, 64);
            body.gameObject.AddComponent<PulseScale>().period = 4.2f; // faint idle sway

            // Glowing cyan eyes (two small additive discs under the hood) (§G).
            for (int e = 0; e < 2; e++)
            {
                var eye = UiWidgets.Rect("Eye", body, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(e == 0 ? -26f : 26f, 0f), new Vector2(22, 22));
                var ei = eye.gameObject.AddComponent<Image>(); ei.raycastTarget = false; ei.sprite = UiTex.Disc(Hex("#5fe0ff"), 32);
            }

            // Staff + cyan staff orb — the brightest single light; focal ≈ (0.20 W, 0.30 H → anchorY 0.70) (§B/§E).
            var staffGlow = UiWidgets.Glow(SafeContent, UiTheme.A(CyanOrb, 0.85f), new Vector2(0.20f, 0.70f), new Vector2(0.20f, 0.70f), Vector2.zero, new Vector2(360, 360), 1.7f);
            var sp = staffGlow.gameObject.AddComponent<PulseGraphic>(); sp.target = staffGlow; sp.min = 0.55f; sp.max = 1f; sp.period = 1.8f; // cyan pulse (§I)
            var orb = UiWidgets.Rect("Staff_Orb", SafeContent, new Vector2(0.20f, 0.70f), new Vector2(0.20f, 0.70f), Vector2.zero, new Vector2(96, 96));
            var oimg = orb.gameObject.AddComponent<Image>(); oimg.raycastTarget = false; oimg.sprite = UiTex.Radial(Hex("#cfeeff"), UiTheme.A(CyanOrb, 0.2f), 128, 1.5f);
            // Floating cyan motes around the staff orb (additive wisps, §J).
            var motes = UiWidgets.Rect("StaffMotes", SafeContent, new Vector2(0.20f, 0.70f), new Vector2(0.20f, 0.70f), Vector2.zero, new Vector2(280, 320));
            var mf = motes.gameObject.AddComponent<EmberField>(); mf.count = 8; mf.color = UiTheme.A(CyanOrb, 0.7f);
        }

        // ---------------------------------------------------------------- orb table + orbs ----
        private void BuildOrbTable()
        {
            // Anchor bottom-center, pivot 0.5,0; rect x 0.18→0.66 W (Δ0.48 ≈1123px), y 0.0→0.55 H (≈594px).
            var table = UiWidgets.Rect("OrbTable", SafeContent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1123, 594));
            table.pivot = new Vector2(0.5f, 0f);
            // Table is centred at x 0.42 W (mid of 0.18→0.66), not 0.5 — offset the group left of screen centre.
            table.anchoredPosition = new Vector2((0.42f - 0.5f) * 2340f, 0f);

            // Table_Cloth — royal-blue velvet, top ≈ y 0.42 H downward (the cloth fills the table's lower band).
            var cloth = UiWidgets.Rect("Table_Cloth", table, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1123, 300));
            var cimg = cloth.gameObject.AddComponent<Image>(); cimg.raycastTarget = false;
            cimg.sprite = UiTex.VGradient(ClothHi, ClothLo, 64);
            // Gold-embroidered crown emblem centred on the cloth (diamond finial stand-in for the crown motif, §G).
            var crown = UiWidgets.Rect("CrownEmblem", cloth, new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), Vector2.zero, new Vector2(120, 120));
            var crimg = crown.gameObject.AddComponent<Image>(); crimg.raycastTarget = false; crimg.sprite = UiTex.Diamond(Hex("#e8c25a"), 48);

            // OrbRow — 4 equal orbs across the cloth top band, gaps ≈0.018 W. Each globe ≈0.10 W (234px).
            const float globe = 220f, gap = 0.018f * 2340f; // ≈42px
            float pitch = globe + gap;
            float startX = -1.5f * pitch; // centre the 4 about table centre
            for (int i = 0; i < Spells.Length; i++)
                BuildOrb(table, Spells[i], i, new Vector2(startX + i * pitch, 0.62f * 594f), globe);
        }

        private void BuildOrb(RectTransform table, Spell s, int index, Vector2 pos, float globe)
        {
            // Orb root = Button (acts like a radio within the row; only one selected, §H).
            var root = UiWidgets.Rect("Orb_" + (index + 1), table, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), pos, new Vector2(globe, globe + 110f));
            _orbRoots[index] = root;
            var hit = root.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0.001f); // raycast surface
            var btn = root.gameObject.AddComponent<Button>(); btn.targetGraphic = hit;
            var cb = btn.colors; cb.highlightedColor = new Color(1.04f, 1.04f, 1.04f, 1f); cb.pressedColor = new Color(0.9f, 0.9f, 0.95f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            int idx = index; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); SelectOrb(idx, true); });

            // Globe stack (bottom-up): gold ring stand → outer bloom → glass globe → inner additive sigil glow → sigil.
            var stand = UiWidgets.Rect("RingStand", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -globe + 18f), new Vector2(globe * 0.9f, 40f));
            var stimg = stand.gameObject.AddComponent<Image>(); stimg.raycastTarget = false; stimg.sprite = UiTex.Disc(UiTheme.Gold, 48);

            // Outer per-sigil bloom (selected → brighter; toggled in SelectOrb).
            var bloom = UiWidgets.Glow(root, UiTheme.A(s.Glow, 0.5f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -globe * 0.5f), new Vector2(globe * 1.5f, globe * 1.5f), 1.7f);
            _orbGlows[index] = bloom;
            var bp = bloom.gameObject.AddComponent<PulseGraphic>(); bp.target = bloom; bp.min = 0.35f; bp.max = 0.6f; bp.period = 2f + index * 0.2f; // sigil glow breathe (§I)

            var glassRt = UiWidgets.Rect("Globe", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -globe * 0.5f), new Vector2(globe, globe));
            var glass = glassRt.gameObject.AddComponent<Image>(); glass.raycastTarget = false; glass.sprite = UiTex.Radial(UiTheme.A(GlassRim, 0.95f), UiTheme.A(GlassBody, 0.45f), 128, 1.4f); // cool blue-white glass

            var sigGlow = UiWidgets.Rect("SigilGlow", glassRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(globe * 0.72f, globe * 0.72f));
            var sgimg = sigGlow.gameObject.AddComponent<Image>(); sgimg.raycastTarget = false; sgimg.sprite = UiTex.Radial(UiTheme.A(s.Glow, 0.8f), UiTheme.A(s.Glow, 0f), 64, 1.5f);
            var sigil = UiWidgets.Rect("Sigil", glassRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(globe * 0.42f, globe * 0.42f));
            var siimg = sigil.gameObject.AddComponent<Image>(); siimg.raycastTarget = false; siimg.sprite = UiTex.Diamond(s.Sigil, 48); // sigil colour stand-in (pickaxe/sword/tridents/shield)
            // Inner swirling particles in the orb's colour (§J).
            var swirl = swirlField(glassRt, s.Glow, globe);

            // Idle float-bob (±6px, staggered phase) on the whole globe column (§H/§I).
            glassRt.gameObject.AddComponent<PulseScale>().period = 2.5f + index * 0.2f;

            // PriceChip directly below the globe (gem icon + count). y ≈0.70→0.76 H band.
            var chip = UiWidgets.Rect("PriceChip", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(140, 56));
            var chBg = chip.gameObject.AddComponent<Image>(); chBg.raycastTarget = false; chBg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Obsidian, 0.7f), UiTheme.A(UiTheme.Obsidian, 0.5f), 16);
            var gem = UiWidgets.Rect("Gem", chip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(30, 30));
            var gemImg = gem.gameObject.AddComponent<Image>(); gemImg.raycastTarget = false; gemImg.sprite = UiTex.Diamond(UiTheme.AmethystHi, 32);
            // Spell price: 28px bold white, dark stroke (§F).
            var pr = UiWidgets.Label(chip, s.Price.ToString(), 28, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(18f, 0f), new Vector2(-10, 40), TextAnchor.MiddleCenter, Color.white);
            pr.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.95f);
        }

        // Sparse swirling motes inside an orb in its sigil colour (additive inner particles, §J).
        private RectTransform swirlField(RectTransform parent, Color col, float globe)
        {
            var rt = UiWidgets.Rect("Swirl", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(globe * 0.7f, globe * 0.7f));
            var f = rt.gameObject.AddComponent<EmberField>(); f.count = 5; f.color = UiTheme.A(col, 0.6f);
            return rt;
        }

        // ---------------------------------------------------------------- detail parchment scroll ----
        private void BuildDetailScroll()
        {
            // Anchor right (1,1), pivot 1,1; rect x 0.665→0.985 W (Δ0.32 ≈749px), y 0.085→0.93 H (Δ0.845 ≈913px).
            var scroll = UiWidgets.Rect("DetailScroll", SafeContent, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(749, 913));
            scroll.pivot = new Vector2(1f, 1f);
            scroll.anchoredPosition = new Vector2(-0.015f * 2340f, -0.085f * 1080f); // right edge at 0.985 W, top at y 0.085

            // Scroll_Frame — aged cream parchment (centre → edge), fibre + curl drop shadow stand-in.
            var parch = scroll.gameObject.AddComponent<Image>(); parch.raycastTarget = false;
            parch.sprite = UiTex.Radial(UiTheme.Parchment, ParchEdge, 128, 1.3f);
            var sh = scroll.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.5f); sh.effectDistance = new Vector2(4, -6);

            // Gold roller caps top & bottom (caa04a→f0d27a) with finial knobs (§G).
            RollerCap(scroll, 1f, 26f);   // top
            RollerCap(scroll, 0f, -26f);  // bottom

            // Content layer (inset inside the rolled body); unrolls/scales on select.
            _scrollInner = UiWidgets.Rect("Scroll_Inner", scroll, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-90, -150));

            // Detail_Title — serif display, UPPER, gold bevel + soft glow, warm dark-brown stroke (§F). y ≈0.135.
            _detailTitle = UiWidgets.TitleLabel(_scrollInner, "MINER'S BLESSING", 34, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), Vector2.zero, new Vector2(560, 60), TextAnchor.MiddleCenter, TitleWarmTop, TitleWarmBot);

            // Detail_Icon — golden pickaxe, matches Orb_1 sigil. y ≈0.22 (left-float).
            var iconRt = UiWidgets.Rect("Detail_Icon", _scrollInner, new Vector2(0.18f, 0.74f), new Vector2(0.18f, 0.74f), Vector2.zero, new Vector2(110, 110));
            _detailIcon = iconRt.gameObject.AddComponent<Image>(); _detailIcon.raycastTarget = false; _detailIcon.sprite = UiTex.Diamond(Spells[0].Sigil, 48);
            UiWidgets.Glow(iconRt, UiTheme.A(Spells[0].Glow, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150), 1.6f);

            // Detail_Desc — light serif ink-brown, slight shadow. y ≈0.33.
            _detailDesc = UiWidgets.Label(_scrollInner, Spells[0].Desc, 22, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(420, 90), TextAnchor.UpperLeft, InkBrown);
            _detailDesc.horizontalOverflow = HorizontalWrapMode.Wrap; ((RectTransform)_detailDesc.transform).anchoredPosition = new Vector2(70f, 0f);

            // Detail_Stats — light serif semibold ink-brown (Duration/Effect). y ≈0.47.
            _detailStats = UiWidgets.Label(_scrollInner, Spells[0].Stats, 21, new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f), Vector2.zero, new Vector2(420, 80), TextAnchor.UpperLeft, InkBrown);
            ((RectTransform)_detailStats.transform).anchoredPosition = new Vector2(70f, 0f);

            // Detail_Flavor — serif italic, muted ink, centred (§F). y ≈0.62.
            _detailFlavor = UiWidgets.Label(_scrollInner, Spells[0].Flavor, 20, new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), Vector2.zero, new Vector2(500, 70), TextAnchor.MiddleCenter, InkFaded);
            _detailFlavor.fontStyle = FontStyle.Italic; _detailFlavor.horizontalOverflow = HorizontalWrapMode.Wrap;

            // BuyButton — large violet gold-edged pill (≈0.24 W × 0.075 H), brightest CTA. y ≈0.78→0.89.
            BuildBuyButton(scroll);
        }

        private void RollerCap(RectTransform scroll, float anchorY, float yOff)
        {
            var cap = UiWidgets.Rect("RollerCap", scroll, new Vector2(0.5f, anchorY), new Vector2(0.5f, anchorY), new Vector2(0f, yOff), new Vector2(790, 56));
            var cimg = cap.gameObject.AddComponent<Image>(); cimg.raycastTarget = false; cimg.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.Gold, 32);
            // finial knobs at each end
            UiWidgets.Finial(cap, new Vector2(0f, 0.5f), new Vector2(-6f, 0f), 56f);
            UiWidgets.Finial(cap, new Vector2(1f, 0.5f), new Vector2(6f, 0f), 56f);
        }

        private void BuildBuyButton(RectTransform scroll)
        {
            // Centred in scroll, width ~0.78 of inner; violet pill, gold rim, outer glow (§D/§G).
            float w = 0.78f * 749f, h = 0.075f * 1080f; // ≈584 × 81
            var root = UiWidgets.Rect("BuyButton", scroll, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(w, h));

            // Outer violet glow (pulse → brightest CTA).
            var glow = UiWidgets.Glow(root, UiTheme.A(BuyHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w * 1.3f, h * 2.2f), 1.7f);
            var gp = glow.gameObject.AddComponent<PulseGraphic>(); gp.target = glow; gp.min = 0.35f; gp.max = 0.65f; gp.period = 1.8f;

            var bodyImg = root.gameObject.AddComponent<Image>();
            bodyImg.sprite = UiTex.VGradient(BuyHi, BuyBody, 32); // bright top bevel → amethyst body
            var btn = root.gameObject.AddComponent<Button>(); btn.targetGraphic = bodyImg;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.88f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); TryBuy(); });

            // Gold edge line (rim).
            var rim = UiWidgets.Rect("Rim", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            // "BUY" (serif display black, UPPER, gold bevel text on violet, glow + dark outline; 36px, §F).
            var buy = UiWidgets.TitleLabel(root, "BUY", 36, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(w, 48), TextAnchor.MiddleCenter, BuyText, UiTheme.Gold);

            // PriceRow (gem icon + "150"), 24px light on violet (§F).
            var priceRow = UiWidgets.Rect("PriceRow", root, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(160, 36));
            var pg = UiWidgets.Rect("Gem", priceRow, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(30, 30));
            _buyPriceGem = pg.gameObject.AddComponent<Image>(); _buyPriceGem.raycastTarget = false; _buyPriceGem.sprite = UiTex.Diamond(UiTheme.AmethystHi, 32);
            _buyPrice = UiWidgets.Label(priceRow, Spells[0].Price.ToString(), 24, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(20f, 0f), new Vector2(-6, 36), TextAnchor.MiddleCenter, Hex("#f3ecff"));

            // Violet sparkle along the pill edge (hover-style ambience, §J).
            var spark = UiWidgets.Rect("BuySparkle", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(w, 30f));
            var sf = spark.gameObject.AddComponent<EmberField>(); sf.count = 6; sf.color = UiTheme.A(BuyHi, 0.6f);
        }

        // ---------------------------------------------------------------- selection / buy ----
        private void SelectOrb(int index, bool animate)
        {
            _selected = Mathf.Clamp(index, 0, Spells.Length - 1);
            var s = Spells[_selected];

            // Orb highlight: selected → scale 1.06 + brightest rim-glow; others → 1.0, dimmer (§H).
            for (int i = 0; i < _orbRoots.Length; i++)
            {
                if (_orbRoots[i] != null) _orbRoots[i].localScale = Vector3.one * (i == _selected ? 1.06f : 1.0f);
                if (_orbGlows[i] != null) { var c = _orbGlows[i].color; c.a = i == _selected ? 0.85f : 0.45f; _orbGlows[i].color = c; }
            }

            // Update DetailScroll content + BUY price (cross-fade flourish via tiny re-unroll bounce, §I).
            if (_detailTitle != null)  _detailTitle.text = s.Name;
            if (_detailDesc != null)   _detailDesc.text = s.Desc;
            if (_detailStats != null)  _detailStats.text = s.Stats;
            if (_detailFlavor != null) _detailFlavor.text = s.Flavor;
            if (_detailIcon != null)   _detailIcon.sprite = UiTex.Diamond(s.Sigil, 48);
            if (_buyPrice != null)     _buyPrice.text = s.Price.ToString();
            // (Click SFX already played by the orb Button before SelectOrb; the scroll cross-fade re-unroll
            //  bounce of §I is the optional flourish noted in §N(c) — content swap above is the load-bearing part.)
        }

        // BUY — DISPLAY-ONLY. The UI never mutates a real balance (§12 / server-auth); GATE-3 binds a real
        // ShopService. Here a stub spend gives layout/flow feedback only (no economy/gameplay meaning).
        private void TryBuy()
        {
            var s = Spells[_selected];
            if (UiStub.TrySpendGems(s.Price))
            {
                if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
                Router.Toast(s.Name + " — purchase pending server (display-only)");
            }
            else
            {
                Router.Toast("Not enough Gems — insufficient (Store gems pending)");
            }
        }

        public override void OnShow()
        {
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
