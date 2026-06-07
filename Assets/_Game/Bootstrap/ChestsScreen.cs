// BULWARK — CHESTS (UI Construction Bible · 20). Presentation-only, REMOVABLE.
//
// Forensic build of design/ChestsScreenDesign.png per Sections A–O: the loot-chest shop tab — shared shop chrome
// (Back TL, STORE/SPELLS/SKINS/CHESTS tab bar TC with CHESTS selected, Gems 1726 + Gold 48570 chips TR), a hooded
// Keeper silhouette with glowing violet eyes/hands behind a center featured gold chest (amethyst gem) standing on
// a runed stone pedestal under an additive violet god-beam + orbiting swirl, a bottom-left 4-slot inventory row
// (Slot_1 active gold-lit with a green "02:59" TimerBar, Slots 2–3 locked w/ padlock, Slot_4 empty socket), and a
// bottom-right violet "OPEN CHEST" CTA with a gold lock-and-key badge. All slots/timer/chest data are DISPLAY-ONLY
// stubs (declared locally; nothing economy/odds invented per §L). ADR note (§L): timed chests + random rewards =
// loot-box/gacha → ADR-gated; the client NEVER rolls loot or mutates balances (§12) — OPEN routes to screen 21,
// which displays the server-authoritative result. Reproduces the VISUAL spec only. NO ECS/gameplay/backend (§12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-20 landscape Chests shop tab. Presentation-only (display-only slots; ADR-gated loot loop, §12).</summary>
    public sealed class ChestsScreen : UiScreen
    {
        // ---- Display-only slot model (NO economy/odds — those live on 21/data per §L). ----
        private enum SlotState { Active, Locked, Empty }
        private struct SlotDef { public SlotState State; public int Seconds; public SlotDef(SlotState s, int sec = 0) { State = s; Seconds = sec; } }

        // Forensic state binding (§L): Slot_1 active w/ "02:59", Slots 2–3 locked, Slot_4 empty.
        private static readonly SlotDef[] Slots =
        {
            new SlotDef(SlotState.Active, 179), // 02:59
            new SlotDef(SlotState.Locked),
            new SlotDef(SlotState.Locked),
            new SlotDef(SlotState.Empty),
        };

        private Text _goldText, _gemText, _timerText;
        private int _timerSeconds = Slots[0].Seconds;
        private bool _opening;

        protected override void Build()
        {
            // ============================ BG_FullBleed: amethyst-lit treasure crypt ============================
            UiWidgets.Stretch("Bg_FullBleed", Rect, Hex("#161019"), "bg_menu"); // crypt base (violet-shifted)
            // Violet ambient fog wash (top→bottom) over the art.
            var fog = UiWidgets.Stretch("Bg_VioletFog", Rect, Color.white);
            fog.sprite = UiTex.VGradient(UiTheme.A(Hex("#3a1f6a"), 0.42f), UiTheme.A(Hex("#0a0710"), 0.62f), 64); fog.raycastTarget = false;
            // Gold-coin hoards glinting in the side alcoves (left + right). Display-only motes.
            HoardGlint(0.085f, 0.30f); HoardGlint(0.915f, 0.30f);
            // Focal purple god-light down onto the chest (high bloom).
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#9a55e6"), 0.40f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(1400, 1100), 1.5f);
            UiWidgets.Vignette(Rect, 0.62f); // heavy vignette (most bloom/vignette-heavy shop tab)
            var bgFog = UiWidgets.Rect("Bg_FogDrift", Rect, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(1700, 900));
            var bff = bgFog.gameObject.AddComponent<EmberField>(); bff.count = 14; bff.color = Hex("#b06bff");

            // =============================== SHARED CHROME (per 17 §C/§D) ===============================
            // Back top-left.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Tab bar top-center — STORE·SPELLS·SKINS·CHESTS, active index 3 = CHESTS.
            UiWidgets.TabBar(SafeContent, new[] { "STORE", "SPELLS", "SKINS", "CHESTS" },
                new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), Vector2.zero, new Vector2(0.44f * 2340f, 84f), 3,
                i => { if (i == 0) Router.Replace<StoreScreen>(); else if (i == 1) Router.Replace<SpellsScreen>(); else if (i == 2) Router.Replace<SkinsScreen>(); });

            // Currency chips top-right — Gold rightmost (idx 0), Gems left of it (idx 1). Values 48570 / 1726.
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);
            _gemText = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);

            // ==================================== KEEPER (behind chest) ====================================
            // Anchor top-center biased, rect x 0.36→0.64 W · y 0.10→0.62 H. Decorative (non-interactive), behind chest.
            BuildKeeper();

            // ==================================== FEATURED CHEST (center) ====================================
            // Anchor center, rect x 0.33→0.67 W · y 0.12→0.78 H. Tap = same as OPEN CHEST.
            BuildFeaturedChest();

            // ================================= CHEST-SLOT ROW (bottom-left) =================================
            // Anchor bottom-left (0,0), rect x 0.020→0.30 W · y 0.02→0.27 H. 4 equal cells.
            BuildSlotRow();

            // =================================== OPEN CHEST (bottom-right) ===================================
            // Anchor bottom-right (1,0), rect x 0.71→0.985 W · y 0.04→0.20 H. Violet CTA + lock-key badge.
            BuildOpenButton();
        }

        // -------------------------------------------------------------------------------------------------
        // KEEPER — near-black hooded silhouette, glowing violet/white eyes + faint hand-glow, rim-lit by aura.
        // -------------------------------------------------------------------------------------------------
        private void BuildKeeper()
        {
            // Center x = 0.50; anchorY center of (0.90,0.38) = 0.64. Size 0.28W × 0.52H.
            var keeper = UiWidgets.Rect("Keeper", SafeContent, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), Vector2.zero, new Vector2(0.28f * 2340f, 0.52f * 1080f));
            var robe = keeper.gameObject.AddComponent<Image>(); robe.raycastTarget = false;
            robe.sprite = UiTex.VGradient(Hex("#1a1622"), Hex("#0a0a12"), 64); // hooded robe, darkest at base
            // Glowing violet/white eyes focal ≈ (0.50 W, 0.22 H from top) → within keeper, near the hood top.
            UiWidgets.Glow(keeper, UiTheme.A(Hex("#cfa6ff"), 0.95f), new Vector2(0.43f, 0.82f), new Vector2(0.43f, 0.82f), Vector2.zero, new Vector2(70, 70), 1.9f);
            UiWidgets.Glow(keeper, UiTheme.A(Hex("#cfa6ff"), 0.95f), new Vector2(0.57f, 0.82f), new Vector2(0.57f, 0.82f), Vector2.zero, new Vector2(70, 70), 1.9f);
            var eyeL = UiWidgets.Rect("EyeL", keeper, new Vector2(0.43f, 0.82f), new Vector2(0.43f, 0.82f), Vector2.zero, new Vector2(26, 26));
            var el = eyeL.gameObject.AddComponent<Image>(); el.raycastTarget = false; el.sprite = UiTex.Disc(Hex("#f3e9ff"), 32);
            var eyeR = UiWidgets.Rect("EyeR", keeper, new Vector2(0.57f, 0.82f), new Vector2(0.57f, 0.82f), Vector2.zero, new Vector2(26, 26));
            var er = eyeR.gameObject.AddComponent<Image>(); er.raycastTarget = false; er.sprite = UiTex.Disc(Hex("#f3e9ff"), 32);
            // Eye-glow flicker (1.6s) on the eye glows.
            var fl = keeper.gameObject.AddComponent<PulseGraphic>(); fl.target = el; fl.min = 0.6f; fl.max = 1f; fl.period = 1.6f;
            var fr = keeper.gameObject.AddComponent<PulseGraphic>(); fr.target = er; fr.min = 0.6f; fr.max = 1f; fr.period = 1.6f;
            // Faint violet hand-glow at ≈ (0.37 W / 0.63 W, 0.45 H) — outstretched arms; map to keeper sides.
            UiWidgets.Glow(keeper, UiTheme.A(Hex("#9e6bf0"), 0.5f), new Vector2(0.04f, 0.42f), new Vector2(0.04f, 0.42f), Vector2.zero, new Vector2(150, 150), 1.7f);
            UiWidgets.Glow(keeper, UiTheme.A(Hex("#9e6bf0"), 0.5f), new Vector2(0.96f, 0.42f), new Vector2(0.96f, 0.42f), Vector2.zero, new Vector2(150, 150), 1.7f);
        }

        // -------------------------------------------------------------------------------------------------
        // FEATURED CHEST — back→front: AuraBeam → Pedestal → Model → AuraSwirl. Tap = OPEN.
        // -------------------------------------------------------------------------------------------------
        private void BuildFeaturedChest()
        {
            // Center x=0.50; anchorY center of (0.88,0.22) = 0.55. Size 0.34W × 0.66H.
            var grp = UiWidgets.Rect("FeaturedChest", SafeContent, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(0.34f * 2340f, 0.66f * 1080f));

            // (1) Chest_AuraBeam — additive vertical violet god-beam through center, behind chest.
            var beam = UiWidgets.Rect("Chest_AuraBeam", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(220, 760));
            var beamImg = beam.gameObject.AddComponent<Image>(); beamImg.raycastTarget = false;
            beamImg.sprite = UiTex.VGradient(UiTheme.A(Hex("#cfa6ff"), 0f), UiTheme.A(Hex("#8a40d8"), 0.55f), 64);
            var beamPulse = beam.gameObject.AddComponent<PulseGraphic>(); beamPulse.target = beamImg; beamPulse.min = 0.75f; beamPulse.max = 1.0f; beamPulse.period = 2.2f; // opacity breathe

            // (2) Chest_Pedestal — dark stone dais (base y ≈0.70→0.78 of screen) with gold + violet rune trim.
            var ped = UiWidgets.Rect("Chest_Pedestal", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(560, 150));
            var pedImg = ped.gameObject.AddComponent<Image>(); pedImg.raycastTarget = false;
            pedImg.sprite = UiTex.VGradient(Hex("#46414e"), Hex("#2a2630"), 64); // worn stone, lighter top
            UiWidgets.Divider(ped, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, 520, 4, UiTheme.Gold); // gold trim line
            // Faint glowing violet runes across the dais face.
            for (int r = 0; r < 5; r++)
            {
                float fx = 0.22f + r * 0.14f;
                var rune = UiWidgets.Rect("Rune", ped, new Vector2(fx, 0.4f), new Vector2(fx, 0.4f), Vector2.zero, new Vector2(34, 34));
                var ri = rune.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Diamond(UiTheme.A(Hex("#b06bff"), 0.85f), 32);
            }

            // (3) Chest_Model — ornate cast-gold chest body (lid + base) with amethyst gem on the front face.
            var bob = UiWidgets.Rect("Chest_Bob", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(440, 360));
            var model = UiWidgets.Rect("Chest_Model", bob, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(400, 300));
            // Warm gold rim glow behind the metal.
            UiWidgets.Glow(model, UiTheme.A(UiTheme.GoldHi, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 460), 1.6f);
            // Base.
            var baseRt = UiWidgets.Rect("ChestBase", model, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(360, 170));
            var baseImg = baseRt.gameObject.AddComponent<Image>(); baseImg.raycastTarget = false; baseImg.sprite = UiTex.VGradient(Hex("#f6d77a"), Hex("#caa04a"), 64);
            var baseRim = UiWidgets.Rect("BaseRim", baseRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var brImg = baseRim.gameObject.AddComponent<Image>(); brImg.raycastTarget = false; brImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 10); brImg.type = Image.Type.Sliced;
            // Lid (rounded dome via gradient cap).
            var lid = UiWidgets.Rect("ChestLid", model, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 178), new Vector2(372, 120));
            var lidImg = lid.gameObject.AddComponent<Image>(); lidImg.raycastTarget = false; lidImg.sprite = UiTex.VGradient(Hex("#f6d77a"), Hex("#b8923f"), 64);
            var lidRim = UiWidgets.Rect("LidRim", lid, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var lrImg = lidRim.gameObject.AddComponent<Image>(); lrImg.raycastTarget = false; lrImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 10); lrImg.type = Image.Type.Sliced;
            // Engraved filigree band across the body seam + corner bosses.
            UiWidgets.Divider(baseRt, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, 360, 6, UiTheme.GoldShadow);
            Boss(baseRt, 0.06f, 0.85f); Boss(baseRt, 0.94f, 0.85f); Boss(baseRt, 0.06f, 0.15f); Boss(baseRt, 0.94f, 0.15f);
            // Amethyst gem on the front face ≈ (0.50 W, 0.52 H of screen) → centered on the base front.
            UiWidgets.Glow(baseRt, UiTheme.A(Hex("#cfa6ff"), 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150), 1.7f);
            var gem = UiWidgets.Rect("AmethystGem", baseRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(88, 88));
            var gemImg = gem.gameObject.AddComponent<Image>(); gemImg.raycastTarget = false; gemImg.sprite = UiTex.Diamond(Hex("#cfa6ff"), 48);
            var gemCore = UiWidgets.Rect("GemCore", gem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50, 50));
            var gcImg = gemCore.gameObject.AddComponent<Image>(); gcImg.raycastTarget = false; gcImg.sprite = UiTex.Diamond(Hex("#7a3fd0"), 48);
            var twinkle = gem.gameObject.AddComponent<PulseGraphic>(); twinkle.target = gemImg; twinkle.min = 0.7f; twinkle.max = 1f; twinkle.period = 1.4f; // gem specular twinkle
            // Gentle idle bob (±5px ~2.6s) on the chest group.
            var bobFx = bob.gameObject.AddComponent<PulseScale>(); bobFx.min = 0.99f; bobFx.max = 1.01f; bobFx.period = 2.6f;

            // (4) Chest_AuraSwirl — additive orbiting violet spark ring + rising sparkles, in front/around.
            var swirl = UiWidgets.Rect("Chest_AuraSwirl", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(520, 520));
            var swirlImg = swirl.gameObject.AddComponent<Image>(); swirlImg.raycastTarget = false;
            swirlImg.sprite = UiTex.Frame(UiTheme.A(Hex("#cfa6ff"), 0f), UiTheme.A(Hex("#b06bff"), 0.55f), UiTheme.A(Hex("#8a40d8"), 0f), 64, 14); swirlImg.type = Image.Type.Sliced;
            var spin = swirl.gameObject.AddComponent<Spin>(); spin.degPerSec = -45f; // slow ring rotate (~8s)
            var sparkRt = UiWidgets.Rect("AmethystSparks", grp, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(460, 600));
            var sparks = sparkRt.gameObject.AddComponent<EmberField>(); sparks.count = 16; sparks.color = Hex("#cfa6ff");

            // Featured-chest tap = same as OPEN CHEST (transparent hit area over the model).
            var hit = UiWidgets.Rect("FeaturedHit", grp, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(440, 420));
            var hitImg = hit.gameObject.AddComponent<Image>(); hitImg.color = new Color(0, 0, 0, 0); // invisible, raycastable
            var hitBtn = hit.gameObject.AddComponent<Button>(); hitBtn.targetGraphic = hitImg;
            hitBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnOpenChest(); });
        }

        // -------------------------------------------------------------------------------------------------
        // CHEST-SLOT ROW — 4 equal framed cells via HorizontalLayoutGroup. Slot_1 active w/ timer.
        // -------------------------------------------------------------------------------------------------
        private void BuildSlotRow()
        {
            float rowW = 0.28f * 2340f, rowH = 0.25f * 1080f;
            // Bottom-left anchor (0,0), pivot (0,0). anchorY range 0.73→0.98 → place at bottom inset.
            var row = UiWidgets.Rect("ChestSlots_Row", SafeContent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.020f * 2340f, 0.02f * 1080f), new Vector2(rowW, rowH));
            row.pivot = new Vector2(0f, 0f);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            for (int i = 0; i < Slots.Length; i++) BuildSlot(row, i, Slots[i]);
        }

        private void BuildSlot(RectTransform row, int index, SlotDef def)
        {
            // Equal cell — LayoutElement lets the HLG size it; we add a fixed flexible weight.
            var cell = UiWidgets.Rect("Slot_" + (index + 1), row, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var le = cell.gameObject.AddComponent<LayoutElement>(); le.flexibleWidth = 1f; le.flexibleHeight = 1f; le.minHeight = 150f;

            switch (def.State)
            {
                case SlotState.Active:
                {
                    // Active gold-lit frame + glow + silver/cobalt chest w/ blue diamond gem + green TimerBar.
                    var btn = UiWidgets.Button(cell, "", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, UiTheme.A(Hex("#1a1622"), 0.95f), () => OnSlotTap(index), 0);
                    var bt = (RectTransform)btn.transform;
                    UiWidgets.Glow(bt, UiTheme.A(UiTheme.GoldHi, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 180), 1.7f);
                    var ring = UiWidgets.Rect("Ring", bt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var ringImg = ring.gameObject.AddComponent<Image>(); ringImg.raycastTarget = false; ringImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); ringImg.type = Image.Type.Sliced;
                    // Chest art — silver/steel + cobalt with a blue diamond gem.
                    var chest = UiWidgets.Rect("ChestArt", bt, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(96, 78));
                    var chImg = chest.gameObject.AddComponent<Image>(); chImg.raycastTarget = false; chImg.sprite = UiTex.VGradient(Hex("#9aa0ad"), Hex("#1e3a8c"), 64);
                    var bgem = UiWidgets.Rect("BlueGem", chest, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(34, 34));
                    var bgImg = bgem.gameObject.AddComponent<Image>(); bgImg.raycastTarget = false; bgImg.sprite = UiTex.Diamond(Hex("#4fa8ff"), 32);
                    // TimerBar at the bottom — green fill + white "02:59".
                    var bar = UiWidgets.Rect("TimerBar", bt, new Vector2(0.5f, 0.02f), new Vector2(0.5f, 0.02f), new Vector2(0, 8), new Vector2(120, 36));
                    var barImg = bar.gameObject.AddComponent<Image>(); barImg.raycastTarget = false; barImg.sprite = UiTex.HGradient(Hex("#2faa3a"), Hex("#46d257"), 64);
                    _timerText = UiWidgets.Label(bar, FormatTime(_timerSeconds), 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#ffffff"));
                    break;
                }
                case SlotState.Locked:
                {
                    // Dim wood chest + centered padlock; non-interactive (tap → info/shake).
                    var btn = UiWidgets.Button(cell, "", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, UiTheme.A(Hex("#14131a"), 0.95f), () => OnSlotTap(index), 0);
                    var bt = (RectTransform)btn.transform;
                    var ring = UiWidgets.Rect("Ring", bt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var ringImg = ring.gameObject.AddComponent<Image>(); ringImg.raycastTarget = false; ringImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.4f), UiTheme.A(UiTheme.Gold, 0.35f), UiTheme.A(UiTheme.GoldShadow, 0.4f), 48, 6); ringImg.type = Image.Type.Sliced;
                    var chest = UiWidgets.Rect("WoodChest", bt, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(96, 74));
                    var chImg = chest.gameObject.AddComponent<Image>(); chImg.raycastTarget = false; chImg.sprite = UiTex.VGradient(Hex("#7a5e34"), Hex("#5a4326"), 64);
                    UiWidgets.Divider(chest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 96, 8, Hex("#3a3a40")); // iron band
                    // Centered padlock glyph (gold/grey diamond stand-in).
                    var lock_ = UiWidgets.Rect("Padlock", bt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52, 52));
                    var li = lock_.gameObject.AddComponent<Image>(); li.raycastTarget = false; li.sprite = UiTex.Diamond(Hex("#cdb887"), 48);
                    break;
                }
                default: // Empty
                {
                    // Dark recessed socket, faint inner gold edge, non-interactive.
                    var sock = UiWidgets.Rect("EmptySocket", cell, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
                    var sImg = sock.gameObject.AddComponent<Image>(); sImg.raycastTarget = false; sImg.sprite = UiTex.VGradient(Hex("#14131a"), Hex("#0a090e"), 64);
                    var ring = UiWidgets.Rect("Ring", sock, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    var ringImg = ring.gameObject.AddComponent<Image>(); ringImg.raycastTarget = false; ringImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.18f), UiTheme.A(UiTheme.Gold, 0.15f), UiTheme.A(UiTheme.GoldShadow, 0.18f), 48, 5); ringImg.type = Image.Type.Sliced;
                    break;
                }
            }
        }

        // -------------------------------------------------------------------------------------------------
        // OPEN CHEST — violet pill, gold ornate frame, lock-and-key badge, label. The brightest CTA.
        // -------------------------------------------------------------------------------------------------
        private void BuildOpenButton()
        {
            float w = 0.275f * 2340f, h = 0.16f * 1080f;
            // Bottom-right anchor (1,0), pivot (1,0). anchorY range 0.80→0.96.
            var cta = UiWidgets.Rect("OpenChestButton", SafeContent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-0.015f * 2340f, 0.04f * 1080f), new Vector2(w, h));
            cta.pivot = new Vector2(1f, 0f);

            // Outer violet glow (ignite).
            var glow = UiWidgets.Glow(cta, UiTheme.A(Hex("#9e6bf0"), 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w * 1.25f, h * 1.6f), 1.7f);
            var gp = glow.gameObject.AddComponent<PulseGraphic>(); gp.target = glow; gp.min = 0.4f; gp.max = 0.7f; gp.period = 1.8f;

            // Amethyst pill body (top bevel → dark base).
            var bodyImg = cta.gameObject.AddComponent<Image>();
            bodyImg.sprite = UiTex.VGradient(Hex("#9e6bf0"), Hex("#6a2db8"), 32);
            var btn = cta.gameObject.AddComponent<Button>(); btn.targetGraphic = bodyImg;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.88f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnOpenChest(); });

            // Gold ornate frame (notched diamond ends via finials).
            var rim = UiWidgets.Rect("Rim", cta, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false; rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 9); rimImg.type = Image.Type.Sliced;
            UiWidgets.Finial(cta, new Vector2(0f, 0.5f), new Vector2(6f, 0), h * 0.6f);   // left notched end
            UiWidgets.Finial(cta, new Vector2(1f, 0.5f), new Vector2(-6f, 0), h * 0.6f);  // right notched end

            // Lock-and-key badge at left ≈0.05 W — gold diamond plate + violet padlock/key.
            var badge = UiWidgets.Rect("LockKeyBadge", cta, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(h * 0.62f, 0), new Vector2(h * 0.66f, h * 0.66f));
            var plate = badge.gameObject.AddComponent<Image>(); plate.raycastTarget = false; plate.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            var key = UiWidgets.Rect("KeyGlyph", badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(h * 0.34f, h * 0.34f));
            var keyImg = key.gameObject.AddComponent<Image>(); keyImg.raycastTarget = false; keyImg.sprite = UiTex.Diamond(Hex("#6a2db8"), 32);

            // Label "OPEN CHEST" — serif gold-bevel on violet, centered-right of the badge.
            var lbl = UiWidgets.TitleLabel(cta, "OPEN CHEST", 34, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#f6e6b8"), Hex("#caa04a"));
            var lrt = (RectTransform)lbl.transform; lrt.offsetMin = new Vector2(h * 1.05f, 0); lrt.offsetMax = new Vector2(-20f, 0);
        }

        // -------------------------------------------------------------------------------------------------
        // EVENT BEHAVIOR (§K) — display-only. Client NEVER rolls loot / mutates balance (§12/§L).
        // -------------------------------------------------------------------------------------------------
        private void OnSlotTap(int index)
        {
            var s = Slots[index];
            if (s.State == SlotState.Active)
                Router.Toast(_timerSeconds <= 0 ? "Chest ready — tap OPEN CHEST" : "Unlocking — unlock now for gems (ADR-gated)");
            else if (s.State == SlotState.Locked)
                Router.Toast("Locked — earn or unlock this slot to fill it");
            // Empty = no-op.
        }

        // OPEN → play the open sequence, then route to Chest Open Result (21). Server-auth RNG; UI displays only.
        private void OnOpenChest()
        {
            if (_opening) return;
            _opening = true;
            StartCoroutine(OpenSequence());
        }

        private IEnumerator OpenSequence()
        {
            // Open burst flash → route under the flash (~1.0s, approximated per §I).
            var flash = UiWidgets.Stretch("OpenFlash", Rect, new Color(1f, 1f, 1f, 0f));
            flash.raycastTarget = true; flash.transform.SetAsLastSibling();
            float t = 0f; const float d = 0.85f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / d);
                // Hold dark, then ramp the violet-white flash in the last 35%.
                float a = k < 0.65f ? 0f : Mathf.InverseLerp(0.65f, 1f, k);
                var c = Color.Lerp(Hex("#cfa6ff"), Color.white, a); c.a = a; flash.color = c;
                yield return null;
            }
            Router.Show<ChestOpenResultScreen>();
        }

        // -------------------------------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------------------------------
        private void Boss(Transform parent, float fx, float fy)
        {
            var b = UiWidgets.Rect("Boss", parent, new Vector2(fx, fy), new Vector2(fx, fy), Vector2.zero, new Vector2(22, 22));
            var img = b.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Disc(UiTheme.GoldHi, 32);
        }

        private void HoardGlint(float fx, float fy)
        {
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Gold, 0.28f), new Vector2(fx, fy), new Vector2(fx, fy), Vector2.zero, new Vector2(360, 260), 1.6f);
            var coins = UiWidgets.Rect("Hoard", Rect, new Vector2(fx, fy), new Vector2(fx, fy), Vector2.zero, new Vector2(220, 200));
            var cf = coins.gameObject.AddComponent<EmberField>(); cf.count = 8; cf.color = UiTheme.GoldHi;
        }

        private static string FormatTime(int sec)
        {
            if (sec < 0) sec = 0;
            return (sec / 60).ToString("00") + ":" + (sec % 60).ToString("00");
        }

        public override void OnShow()
        {
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(TimerTick()); // display-only countdown (synced to a stub end-time; server-auth in prod)
        }

        // Slot_1 TimerBar ticks every 1s ("02:59→02:58…"); at 0 → green→gold flash + "READY" (§I).
        private IEnumerator TimerTick()
        {
            while (_timerSeconds > 0)
            {
                float w = 0f;
                while (w < 1f) { w += Time.unscaledDeltaTime; yield return null; }
                _timerSeconds--;
                if (_timerText != null) _timerText.text = _timerSeconds > 0 ? FormatTime(_timerSeconds) : "READY";
            }
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
