// BULWARK — CHEST OPEN RESULT (UI Construction Bible · 21). Presentation-only, REMOVABLE.
//
// Forensic build of design/ChestOpenResultDesign.png per Sections A–O: the gacha REWARD-REVEAL overlay shown
// after Chests (20) opens. A near-black crypt with a full-bleed additive violet+gold light-burst behind an open
// gold chest (focal), the "CHEST REWARDS" / "Your treasures await!" header, a left→right row of reward cards
// (SILVER ×500 common · GEMS ×40 rare · COSMETIC SHARDS ×15 rare · a larger gold-radiant LEGENDARY LIONHELM
// / COMMANDER HELMET hero card with NO count), and a single cobalt-blue gold-edged COLLECT pill (the only exit
// → Router.Pop() back to Chests). Staggered rarity-order reveal (legendary last/biggest with a gold ring sweep),
// tap-anywhere fast-forwards, COLLECT is disabled until the cascade completes.
// ADR NOTE (Section L): this is the gacha payoff screen → bound by the same "no loot boxes" ADR as Chests (20);
// spec'd EXACTLY as drawn, the ADR governs the loot model, not these visuals. §12: the granted loot is DISPLAY-
// ONLY stub data — the client never rolls/fabricates rewards (server-authoritative); NO ECS/gameplay/balance.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-21 Chest Open Result reveal overlay. Presentation-only (server-auth loot display; §12).</summary>
    public sealed class ChestOpenResultScreen : UiScreen
    {
        // Reference canvas (CanvasScaler 2340×1080, match height). Layout math is fractions of these (Section E).
        private const float W = 2340f, H = 1080f;

        // ---- Card rarity (display only — drives edge/glow colour; carries NO economy/balance meaning, §12). ----
        private enum Rarity { Common, Rare, Legendary }

        // ---- Display-only stub loot (Section L forensic copy/counts; server-authoritative — client never rolls). ----
        // In the wired flow these arrive via OnShow(rewards[]) from the server roll; here they are inert layout data.
        private struct Loot { public string Name; public int Count; public bool HasCount; public Rarity Rar; }
        private static readonly Loot[] Rewards =
        {
            new Loot{ Name = "SILVER",          Count = 500, HasCount = true,  Rar = Rarity.Common },
            new Loot{ Name = "GEMS",            Count = 40,  HasCount = true,  Rar = Rarity.Rare },
            new Loot{ Name = "COSMETIC SHARDS", Count = 15,  HasCount = true,  Rar = Rarity.Rare },
            new Loot{ Name = "LIONHELM",        Count = 0,   HasCount = false, Rar = Rarity.Legendary }, // unique — NO count
        };

        // Reveal/idle handles.
        private RectTransform _burstRays, _burstCore;
        private CountUp[] _counts;
        private RectTransform[] _cards;
        private RectTransform _legRing;       // legendary one-shot radiant ring sweep
        private CanvasGroup _collectCg;
        private RectTransform _collectRt;
        private bool _resolved;

        protected override void Build()
        {
            _counts = new CountUp[Rewards.Length];
            _cards  = new RectTransform[Rewards.Length];

            // ============================================================================================
            // FULL-BLEED BACKGROUND + LIGHT-BURST (Rect; OUTSIDE safe area → bleeds edge-to-edge under cutout).
            // ============================================================================================
            // Bg_FullBleed: dim crypt (#0a0b0f→#161a24), almost fully vignetted to black (Section G/D).
            var bg = UiWidgets.Stretch("Bg_FullBleed", Rect, UiTheme.Obsidian, "bg_battle");
            var bgGrad = UiWidgets.Rect("Bg_Grade", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgGi = bgGrad.gameObject.AddComponent<Image>(); bgGi.raycastTarget = false;
            bgGi.sprite = UiTex.VGradient(Hex("#161a24"), Hex("#0a0b0f"), 64);
            bgGi.color = UiTheme.A(Color.white, 0.85f);

            // Burst_Rays: large additive violet+gold radial behind the chest, ≥1.2× screen, slow rotation (D/J).
            // Centered on the chest focal ≈ (0.50 W, 0.30 H) → anchorY = 1 − 0.30 = 0.70.
            var raysGo = UiWidgets.Glow(Rect, UiTheme.A(Hex("#8a40d8"), 0.55f),
                new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(W * 1.25f, H * 1.25f), 1.5f);
            _burstRays = (RectTransform)raysGo.transform;
            _burstRays.gameObject.AddComponent<Spin>().degPerSec = 8f;          // slow rotate (Section I idle)
            var raysPulse = _burstRays.gameObject.AddComponent<PulseGraphic>(); raysPulse.target = raysGo; raysPulse.min = 0.40f; raysPulse.max = 0.62f; raysPulse.period = 2.4f; // opacity breathe
            // Warm gold ray-spoke layer over the violet (#e8c25a→#fff0b8), counter-rotating for a richer burst.
            var goldRaysGo = UiWidgets.Glow(Rect, UiTheme.A(Hex("#e8c25a"), 0.40f),
                new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(W * 0.95f, H * 1.0f), 1.9f);
            goldRaysGo.gameObject.AddComponent<Spin>().degPerSec = -5f;

            // Bg_Vignette: strong radial → near-black corners (multiply read), keeps focus on the burst (D/J).
            UiWidgets.Vignette(Rect, 0.78f);
            // Floating motes in the burst (drifting embers/motes, Section I/J).
            var moteHost = UiWidgets.Rect("FX_Motes", Rect, new Vector2(0.18f, 0.30f), new Vector2(0.82f, 1.0f), Vector2.zero, Vector2.zero);
            var mef = moteHost.gameObject.AddComponent<EmberField>(); mef.count = 20; mef.color = Hex("#cfa6ff");

            // ============================================================================================
            // HEADER (SafeContent; top-center). Title "CHEST REWARDS" over subtitle "Your treasures await!".
            // ============================================================================================
            // Title baseline y ≈0.08 H from top → anchorY ≈0.92. Serif gold-bevel #f0cf72→#caa04a (Section F).
            UiWidgets.Glow(SafeContent, UiTheme.A(Hex("#e8c25a"), 0.30f), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(0.55f * W, 0.16f * H), 1.7f);
            UiWidgets.TitleLabel(SafeContent, "CHEST REWARDS", 56, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f),
                Vector2.zero, new Vector2(0.6f * W, 0.10f * H), TextAnchor.MiddleCenter, Hex("#f0cf72"), Hex("#caa04a"));
            // Subtitle y ≈0.13 H from top → anchorY ≈0.87. Soft cream-gold italic stand-in #e3d3a2.
            UiWidgets.Label(SafeContent, "Your treasures await!", 24, new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f),
                Vector2.zero, new Vector2(0.5f * W, 0.05f * H), TextAnchor.MiddleCenter, Hex("#e3d3a2"));

            // ============================================================================================
            // OPEN-CHEST HERO (SafeContent; upper-center). Burst_Core (additive) + Chest_Open. Decorative.
            // ============================================================================================
            // Center ≈ (0.50 W, 0.30 H) → anchorY = 0.70. Envelope ≈ 0.28 W × 0.30 H.
            var hero = UiWidgets.Rect("OpenChest_Hero", SafeContent, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(0.28f * W, 0.30f * H));
            // Burst_Core: violet→white light-core erupting upward (additive, behind/through the chest).
            var coreGo = UiWidgets.Glow(hero, UiTheme.A(Hex("#cfa6ff"), 0.85f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(0.34f * W, 0.40f * H), 1.4f);
            _burstCore = (RectTransform)coreGo.transform;
            var corePulse = _burstCore.gameObject.AddComponent<PulseGraphic>(); corePulse.target = coreGo; corePulse.min = 0.60f; corePulse.max = 0.95f; corePulse.period = 1.8f;
            BuildChest(hero);
            // Upward spark fountain from the open lid (additive motes rising out of the chest, Section J).
            var sparkHost = UiWidgets.Rect("Spark_Fountain", hero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30f), new Vector2(0.18f * W, 0.34f * H));
            var sef = sparkHost.gameObject.AddComponent<EmberField>(); sef.count = 16; sef.color = Hex("#fff0b8");

            // ============================================================================================
            // REWARD CARDS ROW (SafeContent; center, biased lower). Band y 0.46→0.82 H → anchorY 0.18→0.54.
            // Cards 1–3 equal (≈0.165 W × 0.34 H); Card_4 legendary ≈1.25× wider × 0.40 H (taller, right end).
            // ============================================================================================
            float bandCenterY = 1f - 0.64f;                 // band mid (fy 0.64) → anchorY 0.36
            float commonW = 0.165f * W, commonH = 0.34f * H; // ≈386 × 367
            float legW    = 0.205f * W, legH    = 0.40f * H; // ≈480 × 432 (hero rarity card)
            float gap     = 0.022f * W;                      // ≈51

            // Total row width: three common + legendary + three gaps; lay out left→right, centered on the band.
            float rowW = commonW * 3f + legW + gap * 3f;
            float x = -rowW * 0.5f;                          // left edge of the row (relative to band center)
            for (int i = 0; i < Rewards.Length; i++)
            {
                bool legendary = Rewards[i].Rar == Rarity.Legendary;
                float cw = legendary ? legW : commonW;
                float ch = legendary ? legH : commonH;
                var anchor = new Vector2(0.5f, bandCenterY);
                var pos = new Vector2(x + cw * 0.5f, 0f);    // vertically centered on the band (legendary overhangs)
                if (legendary) BuildLegendaryCard(i, anchor, pos, new Vector2(cw, ch));
                else           BuildRewardCard(i, anchor, pos, new Vector2(cw, ch));
                x += cw + gap;
            }

            // ============================================================================================
            // COLLECT BUTTON (SafeContent; bottom-center). Cobalt pill, gold-edged. The ONLY exit (Section L).
            // x 0.33→0.67 (Δ0.34 W ≈796) · y 0.05→0.135 from bottom (pill ≈0.085 H). anchorY 0.09 (pill mid).
            // ============================================================================================
            BuildCollect(new Vector2(0.5f, 0.0925f), new Vector2(0.34f * W, 0.085f * H));

            // NEGATIVE RULES enforced: no tab bar, no top currency chips, no Back — none added (reveal overlay).
        }

        // ---- Open ornate gold chest with the lid raised + violet light pouring from inside (Section G). ----
        private void BuildChest(Transform hero)
        {
            var chest = UiWidgets.Rect("Chest_Open", hero, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(0.2f * W, 0.16f * H));
            // Chest body: cast-gold #caa04a→#f6d77a relief.
            var body = chest.gameObject.AddComponent<Image>(); body.raycastTarget = false;
            body.sprite = UiTex.VGradient(Hex("#f6d77a"), Hex("#caa04a"), 64);
            // Gold beveled rim around the body.
            var rim = UiWidgets.Rect("Chest_Rim", chest, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); ri.type = Image.Type.Sliced;
            // Open mouth: violet inner glow pouring out of the chest.
            var mouth = UiWidgets.Rect("Chest_Mouth", chest, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6f), new Vector2(0.18f * W, 0.06f * H));
            var mg = mouth.gameObject.AddComponent<Image>(); mg.raycastTarget = false; mg.sprite = UiTex.HGradient(Hex("#5a2db0"), Hex("#8a40d8"), 32);
            // Blue-gem accent on the front (matches 20's featured chest).
            var gem = UiWidgets.Rect("Chest_Gem", chest, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(0.03f * W, 0.03f * W));
            var gi = gem.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(Hex("#4f8bff"), 48);
            // Raised lid (tilted back behind the body, hinged at the top).
            var lid = UiWidgets.Rect("Chest_Lid", chest, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0.05f * H), new Vector2(0.2f * W, 0.07f * H));
            var li = lid.gameObject.AddComponent<Image>(); li.raycastTarget = false; li.sprite = UiTex.VGradient(Hex("#f6d77a"), Hex("#b8923f"), 64);
            lid.localRotation = Quaternion.Euler(0, 0, 8f);
            chest.gameObject.AddComponent<PulseScale>().period = 2.6f; // faint idle breathe on the hero chest
        }

        // ---- Common/Rare reward card (1–3): rarity-edged dark slate card, Name → Art → "×N" (Section D/F/G). ----
        private void BuildRewardCard(int i, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var loot = Rewards[i];
            Color edge = loot.Rar == Rarity.Rare ? Hex("#7a3fd0") : Hex("#aab0bc"); // rare violet · common silver

            var card = UiWidgets.Rect("Card_" + (i + 1) + "_" + loot.Name, SafeContent, anchor, anchor, pos, size);
            _cards[i] = card;
            // Card body: dark slate #14161e→#222633.
            var body = card.gameObject.AddComponent<Image>(); body.raycastTarget = false;
            body.sprite = UiTex.VGradient(Hex("#222633"), Hex("#14161e"), 64);
            // Rarity-tinted frame (silver/violet).
            var frame = UiWidgets.Rect("Frame", card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fi = frame.gameObject.AddComponent<Image>(); fi.raycastTarget = false; fi.sprite = UiTex.Frame(UiWidgets.Lighten(edge, 0.3f), edge, UiWidgets.Darken(edge, 0.45f), 48, 7); fi.type = Image.Type.Sliced;
            // Rare cards keep a gentle rarity glow loop (Section H idle).
            if (loot.Rar == Rarity.Rare)
            {
                var g = UiWidgets.Glow(card, UiTheme.A(edge, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.15f, 1.8f);
                var pg = g.gameObject.AddComponent<PulseGraphic>(); pg.target = g; pg.min = 0.25f; pg.max = 0.5f; pg.period = 2.0f;
                g.transform.SetAsFirstSibling();
            }

            // Name (top, fy≈0.50 of card → anchorY 0.50). Serif gold bevel #ecd591.
            int nameSize = loot.Name == "COSMETIC SHARDS" ? 22 : 26; // 2-line for the long name (Section F)
            UiWidgets.TitleLabel(card, loot.Name, nameSize, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f),
                Vector2.zero, new Vector2(0.92f * size.x, 0.34f * size.y), TextAnchor.MiddleCenter, Hex("#f6e2a4"), Hex("#caa04a"));

            // Art (center, fy≈0.62 → anchorY 0.38).
            BuildCardArt(card, loot, new Vector2(0.5f, 0.46f), new Vector2(0.42f * size.x, 0.42f * size.x));

            // Count "×N" (bottom, fy≈0.77 → anchorY 0.23). White, dark stroke (Section F). Counts up on reveal.
            if (loot.HasCount)
            {
                var cnt = UiWidgets.Label(card, "×0", 30, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f),
                    Vector2.zero, new Vector2(0.9f * size.x, 0.18f * size.y), TextAnchor.MiddleCenter, Color.white);
                cnt.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#05060a"), 0.9f);
                int target = loot.Count;
                var cu = card.gameObject.AddComponent<CountUp>(); cu.Bind(cnt, v => "×" + v.ToString("N0"), 0f);
                _counts[i] = cu;
            }

            card.localScale = Vector3.zero; // hidden pre-reveal (Section H), scaled up by the cascade.
        }

        // ---- Legendary card (4): taller gold-radiant frame + sparkle aura; LEGENDARY → LIONHELM → sub → art. ----
        private void BuildLegendaryCard(int i, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var loot = Rewards[i];
            var card = UiWidgets.Rect("Card_" + (i + 1) + "_Legendary", SafeContent, anchor, anchor, pos, size);
            _cards[i] = card;

            // Strongest glow on screen — radiant gold aura behind the card (Section G).
            var aura = UiWidgets.Glow(card, UiTheme.A(Hex("#fff0b8"), 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.45f, 1.6f);
            aura.transform.SetAsFirstSibling();
            var ap = aura.gameObject.AddComponent<PulseGraphic>(); ap.target = aura; ap.min = 0.4f; ap.max = 0.75f; ap.period = 2.0f; // persistent sparkle/aura breathe

            // Card body: dark slate, slightly warmer than commons.
            var body = card.gameObject.AddComponent<Image>(); body.raycastTarget = false;
            body.sprite = UiTex.VGradient(Hex("#262130"), Hex("#161018"), 64);
            // Radiant gold rarity frame #e8c25a→#fff0b8.
            var frame = UiWidgets.Rect("Frame", card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fi = frame.gameObject.AddComponent<Image>(); fi.raycastTarget = false; fi.sprite = UiTex.Frame(Hex("#fff0b8"), Hex("#e8c25a"), Hex("#9a7320"), 48, 9); fi.type = Image.Type.Sliced;
            // Corner light flares (Section J).
            CornerFlare(card, new Vector2(0f, 1f), new Vector2( 10f, -10f));
            CornerFlare(card, new Vector2(1f, 1f), new Vector2(-10f, -10f));
            CornerFlare(card, new Vector2(0f, 0f), new Vector2( 10f,  10f));
            CornerFlare(card, new Vector2(1f, 0f), new Vector2(-10f,  10f));
            // Sparkle aura motes drifting over the card.
            var sparkHost = UiWidgets.Rect("Sparkle", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 0.9f);
            var sef = sparkHost.gameObject.AddComponent<EmberField>(); sef.count = 12; sef.color = Hex("#fff0b8");

            // One-shot radiant ring sweep (hidden until reveal, then expands+fades) — the chase signal (Section I/J).
            var ring = UiWidgets.Glow(card, UiTheme.A(Hex("#fff0b8"), 0.0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 0.3f, 1.4f);
            _legRing = (RectTransform)ring.transform;

            // Rarity_Label "LEGENDARY" (top, fy≈0.48 → anchorY 0.52). Bright gold, condensed black look (Section F).
            UiWidgets.Label(card, UiTheme.Track("LEGENDARY"), 22, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f),
                Vector2.zero, new Vector2(0.92f * size.x, 0.10f * size.y), TextAnchor.MiddleCenter, Hex("#ffdf8a"));
            // Item_Name "LIONHELM" (fy≈0.52 → anchorY 0.48). Serif gold bevel #f3d98a.
            UiWidgets.TitleLabel(card, loot.Name, 30, new Vector2(0.5f, 0.77f), new Vector2(0.5f, 0.77f),
                Vector2.zero, new Vector2(0.92f * size.x, 0.10f * size.y), TextAnchor.MiddleCenter, Hex("#f3d98a"), Hex("#caa04a"));
            // Item_Sub "COMMANDER HELMET" (fy≈0.55 → anchorY 0.45). Muted gold subtitle #cdb887.
            UiWidgets.Label(card, UiTheme.Track("COMMANDER HELMET"), 16, new Vector2(0.5f, 0.69f), new Vector2(0.5f, 0.69f),
                Vector2.zero, new Vector2(0.96f * size.x, 0.07f * size.y), TextAnchor.MiddleCenter, Hex("#cdb887"));
            // Item_Art: gold lion helm with a cobalt-blue plume (fy≈0.66 → anchorY 0.34). NO count (unique item).
            BuildLionHelm(card, new Vector2(0.5f, 0.38f), new Vector2(0.5f * size.x, 0.46f * size.y));

            card.localScale = Vector3.zero; // hidden pre-reveal; biggest flash on the cascade's final beat.
        }

        // ---- Per-card art glyphs (display-only stand-ins for bespoke art, Section G/N). ----
        private void BuildCardArt(Transform card, Loot loot, Vector2 anchor, Vector2 size)
        {
            if (loot.Name == "SILVER")
            {
                // Stacked silver coins #c4c8d0→#eef0f4 with an embossed star.
                for (int k = 0; k < 3; k++)
                {
                    var coin = UiWidgets.Rect("Coin", card, anchor, anchor, new Vector2(0, (k - 1) * size.y * 0.22f), new Vector2(size.x, size.y * 0.3f));
                    var ci = coin.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(Hex("#dfe2e8"), 48);
                }
                var star = UiWidgets.Rect("Star", card, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.4f, size.x * 0.4f));
                var si = star.gameObject.AddComponent<Image>(); si.raycastTarget = false; si.sprite = UiTex.Diamond(Hex("#eef0f4"), 32);
            }
            else if (loot.Name == "GEMS")
            {
                // Glowing amethyst crystal cluster #8a40d8→#cfa6ff.
                var glow = UiWidgets.Glow(card, UiTheme.A(Hex("#9e6bf0"), 0.55f), anchor, anchor, Vector2.zero, size * 1.4f, 1.7f);
                glow.transform.SetAsFirstSibling();
                float[] dx = { -0.22f, 0.22f, 0f }; float[] dy = { -0.1f, -0.1f, 0.18f };
                for (int k = 0; k < 3; k++)
                {
                    var gem = UiWidgets.Rect("Gem", card, anchor, anchor, new Vector2(dx[k] * size.x, dy[k] * size.y), new Vector2(size.x * 0.5f, size.x * 0.5f));
                    var gi = gem.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(k == 2 ? Hex("#cfa6ff") : Hex("#8a40d8"), 48);
                }
            }
            else // COSMETIC SHARDS — gold helm-shard/crest fragment #caa04a→#f6d77a with violet edge glints.
            {
                var shard = UiWidgets.Rect("Shard", card, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.75f, size.y * 0.9f));
                var sh = shard.gameObject.AddComponent<Image>(); sh.raycastTarget = false; sh.sprite = UiTex.VGradient(Hex("#f6d77a"), Hex("#caa04a"), 32);
                shard.localRotation = Quaternion.Euler(0, 0, 18f);
                var glint = UiWidgets.Rect("Glint", shard, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(size.x * 0.3f, size.x * 0.3f));
                var gl = glint.gameObject.AddComponent<Image>(); gl.raycastTarget = false; gl.sprite = UiTex.Diamond(Hex("#9e6bf0"), 32);
            }
        }

        // ---- Lion helm with a cobalt-blue plume crest (gold face, glowing eyes) — bespoke art stand-in. ----
        private void BuildLionHelm(Transform card, Vector2 anchor, Vector2 size)
        {
            // Blue plume crest #1f3fb0→#4f8bff (behind the helm).
            var plume = UiWidgets.Rect("Plume", card, anchor, anchor, new Vector2(0, size.y * 0.34f), new Vector2(size.x * 0.42f, size.y * 0.55f));
            var pl = plume.gameObject.AddComponent<Image>(); pl.raycastTarget = false; pl.sprite = UiTex.VGradient(Hex("#4f8bff"), Hex("#1f3fb0"), 64);
            // Gold helm face #caa04a→#f6d77a.
            var helm = UiWidgets.Rect("Helm", card, anchor, anchor, Vector2.zero, new Vector2(size.x * 0.78f, size.y * 0.8f));
            var hi = helm.gameObject.AddComponent<Image>(); hi.raycastTarget = false; hi.sprite = UiTex.VGradient(Hex("#f6d77a"), Hex("#caa04a"), 64);
            var helmRim = UiWidgets.Rect("HelmRim", helm, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hr = helmRim.gameObject.AddComponent<Image>(); hr.raycastTarget = false; hr.sprite = UiTex.Frame(Hex("#fff0b8"), Hex("#caa04a"), Hex("#6b5320"), 48, 7); hr.type = Image.Type.Sliced;
            // Glowing eyes (two cobalt slits).
            var eyeL = UiWidgets.Rect("EyeL", helm, new Vector2(0.36f, 0.45f), new Vector2(0.36f, 0.45f), Vector2.zero, new Vector2(size.x * 0.1f, size.x * 0.06f));
            var el = eyeL.gameObject.AddComponent<Image>(); el.raycastTarget = false; el.sprite = UiTex.Disc(Hex("#4f8bff"), 32);
            var eyeR = UiWidgets.Rect("EyeR", helm, new Vector2(0.64f, 0.45f), new Vector2(0.64f, 0.45f), Vector2.zero, new Vector2(size.x * 0.1f, size.x * 0.06f));
            var er = eyeR.gameObject.AddComponent<Image>(); er.raycastTarget = false; er.sprite = UiTex.Disc(Hex("#4f8bff"), 32);
        }

        private void CornerFlare(Transform card, Vector2 anchor, Vector2 pos)
        {
            var f = UiWidgets.Glow(card, UiTheme.A(Hex("#fff0b8"), 0.7f), anchor, anchor, pos, new Vector2(80f, 80f), 1.6f);
            var p = f.gameObject.AddComponent<PulseGraphic>(); p.target = f; p.min = 0.35f; p.max = 0.8f; p.period = 1.6f;
        }

        // ---- COLLECT: cobalt-blue gold-edged pill (NOT violet — confirm/bank). The only exit → Router.Pop(). ----
        private void BuildCollect(Vector2 anchor, Vector2 size)
        {
            // Outer glow (gentle; brightens when ready).
            var glow = UiWidgets.Glow(SafeContent, UiTheme.A(Hex("#4f8bff"), 0.5f), anchor, anchor, Vector2.zero, size * 1.35f);
            var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.25f; pg.max = 0.55f; pg.period = 1.8f;

            var rt = UiWidgets.Rect("CollectButton", SafeContent, anchor, anchor, Vector2.zero, size);
            _collectRt = rt;
            // Royal/cobalt body #1f3fb0→#3a63d0 with a bright top bevel.
            var fill = rt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(Hex("#4f74e0"), Hex("#1f3fb0"), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = fill;
            var cb = btn.colors; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Pop(); }); // bank rewards → back to Chests
            // Gold ornate edge (notched-corner read via the beveled frame).
            var rim = UiWidgets.Rect("Collect_Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); ri.type = Image.Type.Sliced;
            // COLLECT label: serif gold-bevel text on blue, dark outline (Section F).
            var lbl = UiWidgets.TitleLabel(rt, "COLLECT", 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#f3e6c0"), Hex("#d8c089"));
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#0a1840"), 0.9f);

            // Disabled/dimmed during the reveal (Section H) — enabled when the cascade completes (or tap-skip).
            _collectCg = rt.gameObject.AddComponent<CanvasGroup>();
            _collectCg.alpha = 0.35f; _collectCg.interactable = false; _collectCg.blocksRaycasts = false;
        }

        // =================== LIFECYCLE / REVEAL ===================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Reveal());
        }

        // Build-up burst → cards reveal left→right (rarest last) → COLLECT enables (Section I). Tap = fast-forward.
        private IEnumerator Reveal()
        {
            // Burst bloom-up (0→0.4): rays + core scale from a tight core to full envelope.
            if (_burstRays != null) _burstRays.localScale = Vector3.one * 0.6f;
            if (_burstCore != null) _burstCore.localScale = Vector3.one * 0.4f;
            float bt = 0f; const float bd = 0.4f;
            while (bt < bd && !_resolved)
            {
                bt += Time.unscaledDeltaTime; float k = Mathf.Clamp01(bt / bd);
                if (_burstRays != null) _burstRays.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, k);
                if (_burstCore != null) _burstCore.localScale = Vector3.one * Mathf.Lerp(0.4f, 1f, EaseOutBack(k));
                yield return null;
            }
            if (_burstRays != null) _burstRays.localScale = Vector3.one;
            if (_burstCore != null) _burstCore.localScale = Vector3.one;

            if (_resolved) yield break;
            yield return Wait(0.15f); // title/subtitle settle beat

            // Card cascade left→right, rarest LAST (Section I): 0.55 / 0.75 / 0.95 / [beat] 1.2 legendary.
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_resolved) break;
                bool legendary = Rewards[i].Rar == Rarity.Legendary;
                if (legendary) yield return Wait(0.25f); // brief "slow-mo" beat before the chase reward
                yield return PopCard(i, legendary);
                if (legendary && _legRing != null) StartCoroutine(RingSweep());
                yield return Wait(legendary ? 0.15f : 0.2f);
            }

            EnableCollect(); // ~end of cascade
        }

        // Card reveal: scale 0→1 ease-out-back + count-up; legendary gets the biggest pop.
        private IEnumerator PopCard(int i, bool legendary)
        {
            var card = _cards[i];
            if (card == null) yield break;
            float over = legendary ? 1.12f : 1.0f;          // legendary overshoots harder (bigger flash)
            float dur  = legendary ? 0.30f : 0.25f;
            float t = 0f;
            while (t < dur && !_resolved)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dur);
                float s = Mathf.Lerp(0f, over, EaseOutBack(k));
                card.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            card.localScale = Vector3.one;
            _counts[i]?.To(Rewards[i].Count, 0.3f);          // count "×N" up quickly (Section H/I)
        }

        // Legendary one-shot radiant ring sweep: expand + fade out.
        private IEnumerator RingSweep()
        {
            if (_legRing == null) yield break;
            var img = _legRing.GetComponent<Image>();
            float t = 0f; const float d = 0.5f;
            float baseSize = _legRing.sizeDelta.x;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                float s = Mathf.Lerp(1f, 3.2f, k);
                _legRing.sizeDelta = new Vector2(baseSize * s, baseSize * s);
                if (img != null) { var c = img.color; c.a = Mathf.Lerp(0.85f, 0f, k); img.color = c; }
                yield return null;
            }
            if (img != null) { var c = img.color; c.a = 0f; img.color = c; }
        }

        // Tap-anywhere during the cascade fast-forwards all reveals + enables COLLECT (Section H/I/K).
        private void Update()
        {
            if (_resolved) return;
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) ResolveAll();
        }

        private void ResolveAll()
        {
            if (_resolved) return;
            _resolved = true;
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] != null) _cards[i].localScale = Vector3.one;
                _counts[i]?.Snap(Rewards[i].Count);
            }
            if (_burstRays != null) _burstRays.localScale = Vector3.one;
            if (_burstCore != null) _burstCore.localScale = Vector3.one;
            EnableCollect();
        }

        private void EnableCollect()
        {
            if (_collectCg == null) return;
            _collectCg.alpha = 1f; _collectCg.interactable = true; _collectCg.blocksRaycasts = true;
            if (_collectRt != null) StartCoroutine(CollectBounce());
        }

        private IEnumerator CollectBounce()
        {
            float t = 0f; const float d = 0.25f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); float s = Mathf.Lerp(0.9f, 1f, EaseOutBack(k)); _collectRt.localScale = new Vector3(s, s, 1f); yield return null; }
            _collectRt.localScale = Vector3.one;
        }

        private IEnumerator Wait(float s) { float t = 0f; while (t < s && !_resolved) { t += Time.unscaledDeltaTime; yield return null; } }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; float p = x - 1f; return 1f + c3 * p * p * p + c1 * p * p; }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
