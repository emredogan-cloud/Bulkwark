// BULWARK — SKINS (UI Construction Bible · 19). Presentation-only, REMOVABLE.
//
// Forensic build of design/SkinsScreenDesign.png per Sections A–O: the cosmetic-set tab of the shared shop hub
// over a dusk armory/battlefield. Shared chrome (Back TL, tab bar STORE·SPELLS·SKINS·CHESTS TC with SKINS
// gold-lit, Gems+Gold chips TR); a left 5-thumb gold-framed skin rail (LEAF top, selected); a center glowing
// green LEAF SET full-body hero on a light pedestal; a bottom gold-framed row of 4 themed pickaxe pieces with
// violet gem prices (100/300/600/900, the green Leaf piece selected w/ frame + ✓); and a right detail panel
// (title + 4 stat-bonus rows + italic flavor + cobalt EQUIP ALL). ADR NOTE (§L): the right-panel stat modifiers
// (+30/+20/+25/+15%) collide with "skins are visual-only" + "gems never buy power" → an ADR governs whether
// they ship; this file reproduces the VISUAL spec EXACTLY (display-only — never altered, never wired to balance).
// Skin/piece data is a clearly-display-only local stub; equip/buy = Router.Toast. NO ECS/gameplay/backend (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-19 landscape Skins shop tab. Presentation-only (display-only cosmetic stub; ADR-flagged).</summary>
    public sealed class SkinsScreen : UiScreen
    {
        // ---- Display-only stub data (NO economy/balance meaning; ADR governs whether bonuses ever ship) ----
        private struct Skin { public string Name; public Color Tone; }   // rail portrait + hero tint
        private struct Piece { public string Price; public Color ArtA, ArtB; public bool Horizontal; public bool Selected; }
        private struct Bonus { public string Text; public Color IconCol; }

        // Rail roster (5) — top (LEAF) selected. Tones approximate the per-theme portrait busts in the mock.
        private static readonly Skin[] Skins =
        {
            new Skin{ Name="LEAF SET", Tone=Hex("#5fd24a") }, // selected — green-leaf hooded figure
            new Skin{ Name="EMBER",    Tone=Hex("#d8452b") }, // red/orange knight-helm
            new Skin{ Name="THORN",    Tone=Hex("#2d5a2b") }, // dark-green hooded
            new Skin{ Name="ARCANE",   Tone=Hex("#9e6bf0") }, // purple wizard-hat
            new Skin{ Name="BONE",     Tone=Hex("#d9d2c2") }, // white skull/bone
        };

        // LEAF SET set-pieces (themed pickaxes), Piece_2 (Leaf) selected.
        private static readonly Piece[] Pieces =
        {
            new Piece{ Price="100", ArtA=Hex("#8a8f98"), ArtB=Hex("#5a4326"), Selected=false }, // plain steel + wood
            new Piece{ Price="300", ArtA=Hex("#9bff7a"), ArtB=Hex("#3fd24a"), Selected=true  }, // green Leaf (selected)
            new Piece{ Price="600", ArtA=Hex("#cfeaff"), ArtB=Hex("#6fb6ff"), Selected=false }, // ice/blue
            new Piece{ Price="900", ArtA=Hex("#ffd07a"), ArtB=Hex("#ff7a2a"), Selected=false }, // fire/orange
        };

        // LEAF SET right-panel bonuses (reproduced EXACTLY as drawn — ADR-gated, visual-only, never wired).
        private static readonly Bonus[] Bonuses =
        {
            new Bonus{ Text="+ 30% Build Speed",  IconCol=Hex("#9bff7a") }, // leaf/gold
            new Bonus{ Text="+ 20% Extra Health", IconCol=Hex("#5fd24a") }, // green heart
            new Bonus{ Text="+ 25% Mining Speed", IconCol=Hex("#caa04a") }, // pickaxe
            new Bonus{ Text="+ 15% Unit Regen",   IconCol=Hex("#bff7a0") }, // sword/plus
        };

        // Exact per-screen hex (Section F/G) not in UiTheme.
        private static readonly Color GreenGoldTop = Hex("#9bff7a"); // detail-title gradient top (green)
        private static readonly Color GreenGoldBot = Hex("#e8d27a"); // detail-title gradient bottom (gold)
        private static readonly Color HeroLeafHi   = Hex("#9bff7a"); // hero foliage highlight
        private static readonly Color HeroLeafBody = Hex("#2faa3a"); // hero foliage body
        private static readonly Color BonusText    = Hex("#eaf3df"); // pale-green-white bonus rows
        private static readonly Color FlavorCol    = Hex("#cdbf9a"); // faded-gold italic flavor
        private static readonly Color CobaltTop    = Hex("#3a63d0"); // EQUIP ALL bevel top
        private static readonly Color CobaltBody   = Hex("#1f3fb0"); // EQUIP ALL body
        private static readonly Color EquipText    = Hex("#f3e6c0"); // EQUIP ALL gold label
        private static readonly Color PanelGlass   = Hex("#0e1118"); // detail panel dark glass
        private static readonly Color CheckGreen    = Hex("#3fd24a"); // selected ✓

        protected override void Build()
        {
            // ---- BG (full-bleed dusk armory/battlefield) + FX → Rect (bleeds under cutout) ----
            UiWidgets.Backdrop(Rect, "skins");
            UiWidgets.Stretch("DuskWash", Rect, UiTheme.A(Hex("#241a22"), 0.5f)); // moodier grey-violet dusk wash
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#ff7a2a"), 0.10f), new Vector2(0.62f, 0.55f), new Vector2(0.62f, 0.55f), Vector2.zero, new Vector2(1500, 900), 1.7f); // ember horizon
            // Torch flicker on the bg camp posts (left + right of the hero).
            Torch(0.215f, 0.40f); Torch(0.245f, 0.55f);
            UiWidgets.Vignette(Rect, 0.6f); // moodier/darker than Store/Spells

            // ============================== SHARED CHROME (17 §C/§D) ==============================
            // Back (top-left).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Tab bar (top-center) — SKINS active (index 2).
            UiWidgets.TabBar(SafeContent, new[] { "STORE", "SPELLS", "SKINS", "CHESTS" },
                new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), Vector2.zero, new Vector2(0.44f * 2340f, 84f), 2,
                i =>
                {
                    if (i == 0) Router.Replace<StoreScreen>();
                    else if (i == 1) Router.Replace<SpellsScreen>();
                    else if (i == 3) Router.Replace<ChestsScreen>();
                });

            // Currency chips (top-right): Gold rightmost (idx 0), Gems left of it (idx 1) → reads Gems · Gold.
            UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);     // 48570
            UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _); // 1726

            // ============================== LEFT SKIN RAIL (5 thumbs) ==============================
            BuildSkinRail();

            // ============================== CENTER HERO PREVIEW ==============================
            BuildHero();

            // ============================== BOTTOM SET-PIECES ROW (4) ==============================
            BuildSetPieces();

            // ============================== RIGHT DETAIL PANEL ==============================
            BuildDetailPanel();
        }

        // ---- Left rail: gold-framed tall column, 5 square portrait thumbs; top (LEAF) gold-lit/selected. ----
        private void BuildSkinRail()
        {
            // Rect x 0.030→0.115 W · y 0.10→0.95 H (anchorY = 1 − fy_from_top).
            var rail = UiWidgets.Rect("SkinRail", SafeContent, new Vector2(0.030f, 0.05f), new Vector2(0.115f, 0.90f), Vector2.zero, Vector2.zero);
            // Ornate gold container frame (returns inner field).
            var field = UiWidgets.OrnateFrame(rail, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UiTheme.FieldDark, false, 12f);

            int n = Skins.Length;
            float cellH = 1f / n;        // 5 equal cells (vertical layout, math reproduced manually)
            float gap = 0.012f;
            for (int i = 0; i < n; i++)
            {
                int idx = i;
                bool selected = i == 0; // LEAF (top) selected
                // Cell center (top→bottom): fy = (i + 0.5) * cellH → anchorY = 1 − fy.
                float fyTop = (i + 0.5f) * cellH;
                float ay = 1f - fyTop;
                var cell = UiWidgets.Rect("Skin_Thumb_" + (i + 1), field,
                    new Vector2(0.5f, ay), new Vector2(0.5f, ay), Vector2.zero, new Vector2(0, 0));
                cell.anchorMin = new Vector2(0.08f, ay - cellH * 0.5f + gap * 0.5f);
                cell.anchorMax = new Vector2(0.92f, ay + cellH * 0.5f - gap * 0.5f);
                cell.offsetMin = Vector2.zero; cell.offsetMax = Vector2.zero;

                // Dim inset portrait (per-theme tone bust) — selected = brighter + glow halo.
                var portrait = cell.gameObject.AddComponent<Image>();
                portrait.sprite = UiTex.VGradient(UiWidgets.Lighten(Skins[i].Tone, selected ? 0.15f : -0.0f),
                                                  UiWidgets.Darken(Skins[i].Tone, selected ? 0.35f : 0.62f), 32);
                if (!selected) portrait.color = new Color(1, 1, 1, 0.62f); // unselected dim
                // Toggle (whole cell selects).
                var btn = cell.gameObject.AddComponent<Button>(); btn.targetGraphic = portrait;
                var cb = btn.colors; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.96f, 0.96f, 0.96f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
                string nm = Skins[i].Name;
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnSelectSkin(nm, idx); });

                // Gold mini-frame (selected = bright gold + glow; unselected = dim gold).
                var rim = UiWidgets.Rect("Frame", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var rimg = rim.gameObject.AddComponent<Image>();
                rimg.sprite = selected
                    ? UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8)
                    : UiTex.Frame(UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.A(UiTheme.GoldShadow, 0.7f), UiTheme.A(UiTheme.GoldShadow, 0.5f), 48, 5);
                rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;

                if (selected)
                {
                    // Warm glow halo + gentle pulse on the selected frame (idle loop 2.5s).
                    var glow = UiWidgets.Glow(cell, UiTheme.A(GreenGoldTop, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 180), 1.7f);
                    var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.25f; pg.max = 0.5f; pg.period = 2.5f;
                }
            }
        }

        // ---- Center: glowing green LEAF SET full-body hero on a faint light pedestal. Decorative (not a button). ----
        private void BuildHero()
        {
            // Rect x 0.27→0.62 W · y 0.10→0.92 H; focal mass ≈ (0.44 W, 0.50 H); pedestal at fy≈0.80.
            var hero = UiWidgets.Rect("HeroPreview", SafeContent, new Vector2(0.27f, 0.08f), new Vector2(0.62f, 0.90f), Vector2.zero, Vector2.zero);

            // Pedestal: faint ground light disc behind, at fy≈0.80 → anchorY 0.20.
            var pedestal = UiWidgets.Glow(hero, UiTheme.A(HeroLeafHi, 0.22f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(560, 220), 1.8f);
            pedestal.transform.SetAsFirstSibling();

            // Focal green inner glow (ignite/breathe).
            var bloom = UiWidgets.Glow(hero, UiTheme.A(HeroLeafBody, 0.45f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(620, 760), 1.6f);
            var bp = bloom.gameObject.AddComponent<PulseGraphic>(); bp.target = bloom; bp.min = 0.30f; bp.max = 0.55f; bp.period = 2f; // green glow breathe (2s)

            // Hero model (full-body silhouette wrapped in glowing emerald foliage) — placeholder vertical-gradient body.
            var model = UiWidgets.Rect("Hero_Model", hero, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(440, 720));
            var mimg = model.gameObject.AddComponent<Image>(); mimg.raycastTarget = false;
            mimg.sprite = UiTex.VGradient(HeroLeafHi, UiTheme.A(HeroLeafBody, 0.85f), 64);
            model.gameObject.AddComponent<PulseScale>().period = 4f; // subtle idle sway/breathe

            // Drifting glowing leaf/spore particles (green additive) over the hero.
            var spores = UiWidgets.Rect("Spores", hero, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(520, 760));
            var ef = spores.gameObject.AddComponent<EmberField>(); ef.count = 12; ef.color = HeroLeafHi;
        }

        // ---- Bottom: gold-framed dark bar, 4 themed pickaxe pieces w/ violet gem price chips; Piece_2 selected. ----
        private void BuildSetPieces()
        {
            // Rect x 0.255→0.665 W · y 0.02→0.27 H (bottom-center).
            var row = UiWidgets.Rect("SetPieces_Row", SafeContent, new Vector2(0.255f, 0.02f), new Vector2(0.665f, 0.27f), Vector2.zero, Vector2.zero);
            var field = UiWidgets.OrnateFrame(row, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UiTheme.FieldDark, false, 14f);

            int n = Pieces.Length;
            for (int i = 0; i < n; i++)
            {
                int idx = i;
                bool selected = Pieces[i].Selected;
                float fx = (i + 0.5f) / n; // 4 equal cells
                var cell = UiWidgets.Rect("Piece_" + (i + 1), field, new Vector2(fx, 0.5f), new Vector2(fx, 0.5f), Vector2.zero, new Vector2(0, 0));
                cell.anchorMin = new Vector2((i + 0.06f) / n, 0.08f);
                cell.anchorMax = new Vector2((i + 0.94f) / n, 0.92f);
                cell.offsetMin = Vector2.zero; cell.offsetMax = Vector2.zero;

                // Dark cell + (selected) green highlight frame.
                var cellImg = cell.gameObject.AddComponent<Image>();
                cellImg.color = new Color(0, 0, 0, 0.001f); // raycast surface
                var btn = cell.gameObject.AddComponent<Button>(); btn.targetGraphic = cellImg;
                var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.97f, 0.97f, 0.97f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
                string price = Pieces[i].Price;
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnSelectPiece(idx, price, selected); });

                if (selected)
                {
                    var grim = UiWidgets.Rect("GreenFrame", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var gimg = grim.gameObject.AddComponent<Image>(); gimg.sprite = UiTex.Frame(GreenGoldTop, CheckGreen, UiWidgets.Darken(CheckGreen, 0.4f), 48, 7); gimg.type = Image.Type.Sliced; gimg.raycastTarget = false;
                    var pg = grim.gameObject.AddComponent<PulseGraphic>(); pg.target = gimg; pg.min = 0.65f; pg.max = 1f; pg.period = 2.5f; // gentle frame glow pulse
                }

                // Pickaxe art (upper ~70%) — themed gradient placeholder (fire = horizontal for ember sweep feel).
                var art = UiWidgets.Rect("Art", cell, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(110, 110));
                var aimg = art.gameObject.AddComponent<Image>(); aimg.raycastTarget = false;
                aimg.sprite = Pieces[i].Horizontal ? UiTex.HGradient(Pieces[i].ArtA, Pieces[i].ArtB, 64) : UiTex.VGradient(Pieces[i].ArtA, Pieces[i].ArtB, 64);

                // Price chip (dark plate + violet gem + white count) at fy≈0.04→0.10 (lower band of the cell).
                var chip = UiWidgets.Rect("PriceChip", cell, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(120, 48));
                var cimg = chip.gameObject.AddComponent<Image>(); var ps = UiWidgets.Spr("panel"); if (ps != null) { cimg.sprite = ps; cimg.type = Image.Type.Sliced; } cimg.color = new Color(0, 0, 0, 0.7f); cimg.raycastTarget = false;
                var gem = UiWidgets.Rect("Gem", chip, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(28, 28));
                var gemImg = gem.gameObject.AddComponent<Image>(); gemImg.sprite = UiTex.Diamond(UiTheme.AmethystHi, 32); gemImg.raycastTarget = false;
                UiWidgets.Label(chip, Pieces[i].Price, 26, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(20, 0), new Vector2(-12, 40), TextAnchor.MiddleCenter, Color.white)
                    .gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);

                // Selected ✓ badge (green check on gold disc), top-right.
                if (selected)
                {
                    var badge = UiWidgets.Rect("Check", cell, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -12), new Vector2(40, 40));
                    var bimg = badge.gameObject.AddComponent<Image>(); bimg.sprite = UiTex.Disc(UiTheme.GoldHi, 48); bimg.raycastTarget = false;
                    UiWidgets.Label(badge, "✓", 30, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CheckGreen);
                }
            }
        }

        // ---- Right: dark glass panel (gold edge, faint battlefield art) — title, 4 bonus rows, flavor, EQUIP ALL. ----
        private void BuildDetailPanel()
        {
            // Rect x 0.745→0.985 W · y 0.10→0.93 H.
            var panel = UiWidgets.Rect("DetailPanel", SafeContent, new Vector2(0.745f, 0.07f), new Vector2(0.985f, 0.90f), Vector2.zero, Vector2.zero);

            // Dark glass body (#0e1118 / 85%) with faint desaturated battlefield-at-dusk art inside.
            var body = panel.gameObject.AddComponent<Image>();
            var ps = UiWidgets.Spr("panel"); if (ps != null) { body.sprite = ps; body.type = Image.Type.Sliced; }
            body.color = UiTheme.A(PanelGlass, 0.85f);
            var artBg = UiWidgets.Stretch("Battlefield", panel, UiTheme.A(Hex("#3a2a22"), 0.25f), "bg_battle"); artBg.raycastTarget = false; // ember-horizon hint
            var ember = UiWidgets.Glow(panel, UiTheme.A(Hex("#ff7a2a"), 0.12f), new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), Vector2.zero, new Vector2(520, 300), 1.7f);
            // Gold edge frame.
            var edge = UiWidgets.Rect("Edge", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var eimg = edge.gameObject.AddComponent<Image>(); eimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 12); eimg.type = Image.Type.Sliced; eimg.raycastTarget = false;

            // Title "LEAF SET" — serif display, green→gold gradient, fy≈0.155 → anchorY 0.845.
            UiWidgets.TitleLabel(panel, "LEAF SET", 38, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(480, 60), TextAnchor.MiddleCenter, GreenGoldTop, GreenGoldBot);

            // 4 bonus rows: y 0.24→0.50 from top → anchorY 0.50→0.76, each ≈0.065 H. Icon (26px) + left text.
            float topY = 0.76f, stepY = 0.075f;
            for (int i = 0; i < Bonuses.Length; i++)
            {
                float ay = topY - i * stepY;
                var iconRt = UiWidgets.Rect("BonusIcon_" + i, panel, new Vector2(0.12f, ay), new Vector2(0.12f, ay), Vector2.zero, new Vector2(26, 26));
                var icon = iconRt.gameObject.AddComponent<Image>(); icon.sprite = UiTex.Diamond(Bonuses[i].IconCol, 32); icon.raycastTarget = false;
                UiWidgets.Label(panel, Bonuses[i].Text, 22, new Vector2(0.20f, ay), new Vector2(0.96f, ay), Vector2.zero, new Vector2(0, 34), TextAnchor.MiddleLeft, BonusText);
            }

            // Flavor (2 lines), serif-italic feel, centered, muted faded-gold, fy≈0.58 → anchorY 0.42.
            UiWidgets.Label(panel, "Harness the power of nature.\nGrow. Protect. Dominate.", 20,
                new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(460, 80), TextAnchor.UpperCenter, FlavorCol);

            // EQUIP ALL — cobalt-blue gold-edged pill, fy 0.80→0.90 → anchorY ~0.15, ≈0.20 W × 0.075 H of screen.
            var equip = UiWidgets.Rect("EquipAllButton", panel, new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), Vector2.zero, new Vector2(420, 96));
            var eGlow = UiWidgets.Glow(equip, UiTheme.A(UiTheme.IronBlueHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 150), 1.7f);
            var eGlowP = eGlow.gameObject.AddComponent<PulseGraphic>(); eGlowP.target = eGlow; eGlowP.min = 0.30f; eGlowP.max = 0.6f; eGlowP.period = 1.8f;
            var eBody = equip.gameObject.AddComponent<Image>(); eBody.sprite = UiTex.VGradient(CobaltTop, UiWidgets.Darken(CobaltBody, 0.2f), 32);
            var eBtn = equip.gameObject.AddComponent<Button>(); eBtn.targetGraphic = eBody;
            var ecb = eBtn.colors; ecb.normalColor = Color.white; ecb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); ecb.pressedColor = new Color(0.82f, 0.82f, 0.88f, 1f); ecb.fadeDuration = 0.08f; eBtn.colors = ecb;
            eBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("EQUIP ALL — display-only (equipped state is server-authoritative)"); });
            var eRim = UiWidgets.Rect("Rim", equip, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var eRimg = eRim.gameObject.AddComponent<Image>(); eRimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); eRimg.type = Image.Type.Sliced; eRimg.raycastTarget = false;
            var eLbl = UiWidgets.Label(equip, UiTheme.Track("EQUIP ALL"), 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, EquipText);
            eLbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
        }

        // A flickering torch glow on a bg camp post.
        private void Torch(float fx, float fy)
        {
            var g = UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.5f), new Vector2(fx, 1f - fy), new Vector2(fx, 1f - fy), Vector2.zero, new Vector2(220, 320), 1.7f);
            var p = g.gameObject.AddComponent<PulseGraphic>(); p.target = g; p.min = 0.3f; p.max = 0.7f; p.period = 0.7f;
        }

        // Selecting a skin in the rail (display-only: re-binds hero/detail/pieces server-side; here a toast).
        private void OnSelectSkin(string name, int idx)
            => Router.Toast(name + " — preview is display-only (skin roster is server-authoritative)");

        // Selecting a set-piece (display-only: equip if owned, else buy → server purchase; here a toast).
        private void OnSelectPiece(int idx, string price, bool owned)
            => Router.Toast(owned ? "Equipped (display-only)" : ("Buy piece for " + price + " gems — display-only (server-authoritative)"));

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        // Exact hex not in UiTheme.
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
