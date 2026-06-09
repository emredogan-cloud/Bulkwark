// BULWARK — TOURNAMENT LADDER (UI Construction Bible · 33). Presentation-only, REMOVABLE.
//
// Forensic code-built build of design/TournamentLadderDesign.png per spec 33 (Sections A–O): a golden throne-hall
// async-tournament bracket. Header = Back (top-left) + gold-bevel serif "CHAMPIONSHIP TOURNAMENT" + "Ends in: 2d 18h"
// timer + gems/gold chips + Rewards/Rules/Leaderboard utility tiles; a laurel CHAMPION crest apex ("?" shield, 🏆
// 10,000); a symmetric single-elimination bracket (left 8 / right 8 → center FINAL) with the player's gold path +
// YOU★ node highlighted and node pending/winner/loser/next-playable states; a Qualifiers / Tournament(active) /
// Battle Log tab bar. The bracket lives in a horizontal+vertical ScrollRect (build B) so it pans on small screens
// and shows fully at 2340×1080. §12 PRESENTATION-ONLY: NO ECS / NO Unity.Entities / NO gameplay-balance-AI-economy.
// ALL bracket/roster/prize/result data is hard-coded DISPLAY-ONLY local stub (the spec roster + "?" champion +
// 10,000 prize); the screen never mutates a bracket/seed/result/balance — only the PLAY CTA crosses the existing
// presentation seam via MatchPresentation.StartMatch("Tournament"); every other button is display-only (Router.Toast).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-33 async Tournament Ladder (single-elimination bracket). Presentation-only (display-only stub data).</summary>
    public sealed class TournamentLadderScreen : UiScreen
    {
        // Canvas reference units (CanvasScaler 2340×1080, match height — see UiScaling).
        private const float W = 2340f, H = 1080f;

        // ---- Spec hex (Section F/G) not already in UiTheme → Hex(); palette where available → UiTheme. ----
        private static readonly Color StoneTop      = Hex("#221c12"); // throne-hall stone, warm (top of bg gradient)
        private static readonly Color StoneBot      = Hex("#14120e"); // throne-hall stone (bottom)
        private static readonly Color TitleTop      = Hex("#f0d27a"); // title gradient top
        private static readonly Color TitleBot      = Hex("#caa04a"); // title gradient bottom
        private static readonly Color TimerGold     = Hex("#ffe08a"); // "Ends in:" timer
        private static readonly Color ChampLabel    = Hex("#f4e6b0"); // "CHAMPION"
        private static readonly Color ChampQ        = Hex("#f0d27a"); // champion "?"
        private static readonly Color PrizeGold     = Hex("#ffd76a"); // prize "10,000"
        private static readonly Color NodeName      = Hex("#e8e2cf"); // competitor names (faction-tinted in use)
        private static readonly Color YouGold       = Hex("#ffd76a"); // YOU / hot-gold path
        private static readonly Color YouPlateInk   = Hex("#2a1c06"); // "You" path-label ink (on gold plate)
        private static readonly Color PathHot       = Hex("#ffd76a"); // player path connectors (hot gold)
        private static readonly Color PathDim       = Hex("#8a6a30"); // antique-bronze undecided/other connectors
        private static readonly Color NodeFill      = Hex("#11141d"); // competitor tile slate (top of gradient)
        private static readonly Color NodeFill2     = Hex("#1b2030"); // competitor tile slate (bottom)
        private static readonly Color ChampShield   = Hex("#1b2030"); // champion "?" shield steel
        private static readonly Color TabActive     = Hex("#f0d27a"); // active tab label
        private static readonly Color TabMuted      = Hex("#7d7768"); // inactive tab label
        private static readonly Color BrazierCore   = Hex("#ffb04a"); // brazier glow core (additive feel)
        private static readonly Color BrazierEdge   = Hex("#e0742a"); // brazier glow edge / ash motes

        // Faction frame tints (Section G): cobalt / oxblood / violet hints.
        private static readonly Color FacIP  = Hex("#2b56c8");
        private static readonly Color FacAH  = Hex("#7a1f1a");
        private static readonly Color FacN   = Hex("#5a2db0");

        // ---- Node states (Section H). DISPLAY-ONLY — never gameplay; never mutated client-side past the seam. ----
        private enum NodeState { Pending, Winner, Loser, You, NextPlayable }

        private struct Slot { public string Name; public Color Tint; public NodeState State; }

        // ===== DISPLAY-ONLY bracket stub (spec C/M roster verbatim; "?" champion; prize 10,000). =====
        // Round-1 left matchups (M1..M4). YOU is the ★ left competitor of M3 (its node is NextPlayable).
        private static readonly Slot[][] R1L =
        {
            new[]{ S("Frostblade", FacIP, NodeState.Winner),  S("Ironclaw",   FacAH, NodeState.Loser) },     // M1 (decided)
            new[]{ S("Voidwalker", FacN,  NodeState.Pending), S("Shadowbane", FacAH, NodeState.Pending) },   // M2 (pending)
            new[]{ S("YOU",        FacIP, NodeState.You),     S("Stormrider", FacAH, NodeState.NextPlayable) }, // M3 ← player's next match
            new[]{ S("Deathbringer",FacAH,NodeState.Pending), S("Ravenheart", FacN,  NodeState.Pending) },   // M4 (pending)
        };
        // Round-1 right matchups (M5..M8).
        private static readonly Slot[][] R1R =
        {
            new[]{ S("Dragonslayer",FacIP,NodeState.Pending), S("Bloodrage",  FacAH, NodeState.Pending) },   // M5
            new[]{ S("Nightreaper", FacN, NodeState.Pending), S("Firelord",   FacAH, NodeState.Pending) },   // M6
            new[]{ S("Goldenblade", FacIP,NodeState.Pending), S("Dreadlord",  FacAH, NodeState.Pending) },   // M7
            new[]{ S("Thunderfist", FacIP,NodeState.Pending), S("Soulhunter", FacN,  NodeState.Pending) },   // M8
        };

        // ---- Reward tiers per placement (Rewards utility sheet preview — display-only, no balance written). ----
        private struct RewardTier { public string Place; public int Gems; }
        private static readonly RewardTier[] RewardTiers =
        {
            new RewardTier{ Place="Champion",  Gems=10000 },
            new RewardTier{ Place="Finalist",  Gems=4000  },
            new RewardTier{ Place="Semifinal", Gems=1500  },
            new RewardTier{ Place="Quarter",   Gems=600   },
        };

        // Display-only chip values from the mock (spec M: 2,340 gems / 58,420 gold).
        private const int StubGems = 2340;
        private const int StubGold = 58420;

        // Animated layers captured for OnShow entry (Section I timeline).
        private CanvasGroup _topBarCg, _crestCg, _bracketCg, _tabBarCg;
        private RectTransform _crest;
        private readonly System.Collections.Generic.List<Graphic> _pathSweep = new System.Collections.Generic.List<Graphic>(12);
        private readonly System.Collections.Generic.List<CanvasGroup> _nodeCg = new System.Collections.Generic.List<CanvasGroup>(20);

        private static Slot S(string n, Color t, NodeState st) => new Slot { Name = n, Tint = t, State = st };

        protected override void Build()
        {
            BuildBackdrop();
            BuildTopBar();
            BuildChampionCrest();
            BuildBracket();
            BuildTabBar();
        }

        // =====================================================================================================
        // BACKDROP (Section B/G/J): throne-hall stone + warm god-rays + brazier glows + heavy vignette. → Rect.
        // =====================================================================================================
        private void BuildBackdrop()
        {
            // Warm stone base (matte — does NOT bloom).
            var bg = UiLayers.Plate(Rect, "tournament", 0.30f);
            bg.gameObject.AddComponent<KenBurns>().duration = 14f; // slow throne-hall push
            var grade = UiWidgets.Rect("StoneGrade", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var gImg = grade.gameObject.AddComponent<Image>(); gImg.raycastTarget = false;
            gImg.sprite = UiTex.VGradient(UiTheme.A(StoneTop, 0.35f), UiTheme.A(StoneBot, 0.92f), 64);

            // Warm god-rays from the top (bloom).
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#ffcf7a"), 0.30f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(1600, 1000), 1.5f);

            // Brazier glows lining the steps (additive ember, flicker via PulseGraphic) + ash motes.
            BrazierGlow(0.16f, 0.30f); BrazierGlow(0.84f, 0.30f);
            BrazierGlow(0.30f, 0.16f); BrazierGlow(0.70f, 0.16f);
            var motes = UiWidgets.Rect("AshMotes", Rect, new Vector2(0, 0), new Vector2(1, 0.55f), Vector2.zero, Vector2.zero);
            var ef = motes.gameObject.AddComponent<EmberField>(); ef.count = 18; ef.color = BrazierEdge;

            UiWidgets.Vignette(Rect, 0.6f); // heavy throne-hall vignette
        }

        private void BrazierGlow(float fx, float fyFromTop)
        {
            float ay = 1f - fyFromTop;
            var g = UiWidgets.Glow(Rect, UiTheme.A(BrazierCore, 0.55f), new Vector2(fx, ay), new Vector2(fx, ay), Vector2.zero, new Vector2(300, 360), 1.7f);
            var p = g.gameObject.AddComponent<PulseGraphic>(); p.target = g; p.min = 0.35f; p.max = 0.7f; p.period = 0.5f; // flame flicker
        }

        // =====================================================================================================
        // TOP BAR (Section C/E/F): Back + TitleBlock + currency chips + utility tiles. → SafeContent.
        // =====================================================================================================
        private void BuildTopBar()
        {
            var bar = UiWidgets.Rect("TopBar", SafeContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -0.06f * H), new Vector2(0, 0.12f * H));
            _topBarCg = bar.gameObject.AddComponent<CanvasGroup>();

            // Back (top-left) → pop to hub/competitive root.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // TitleBlock: gold-bevel serif title + clock timer beneath (left, after Back).
            UiWidgets.TitleLabel(SafeContent, "CHAMPIONSHIP TOURNAMENT", 54,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(260, -52), new Vector2(1180, 80), TextAnchor.MiddleLeft, TitleTop, TitleBot);
            UiWidgets.Label(SafeContent, "◷ Ends in: 2d 18h", 26,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(266, -104), new Vector2(700, 44), TextAnchor.MiddleLeft, TimerGold);

            // Currency chips (top-right): gold rightmost (idx 0), gems left of it (idx 1). No energy chip (canon).
            UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold,      StubGold, 0, out _);
            UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, StubGems, 1, out _);

            // Utility tiles (right, below the chips): Rewards / Rules / Leaderboard — display-only sheets via Toast.
            float d = 0.06f * H, gap = 0.012f * W;
            float ry = 0.225f; // anchorY beneath the chips
            UtilityTile("LEADERBOARD", 0, d, gap, ry, () => Router.Toast("Tournament Leaderboard — coming soon"));
            UtilityTile("RULES",       1, d, gap, ry, () => Router.Toast("Tournament Rules — coming soon"));
            UtilityTile("REWARDS",     2, d, gap, ry, ShowRewards);
        }

        // Right-anchored utility tile (index 0 = rightmost). Round icon + caption beneath (IconTile).
        private void UtilityTile(string caption, int index, float diameter, float gap, float anchorY, UnityEngine.Events.UnityAction onClick)
        {
            float x = -(0.02f * W + diameter * 0.5f) - index * (diameter + gap);
            UiWidgets.IconTile(SafeContent, caption, new Vector2(1, anchorY), new Vector2(1, anchorY), new Vector2(x, 0), diameter, UiWidgets.Grey, onClick);
        }

        // Rewards sheet preview (display-only): placement → prize tiers as a transient toast (no balance written).
        private void ShowRewards()
        {
            string msg = "REWARDS";
            for (int i = 0; i < RewardTiers.Length; i++) msg += "   " + RewardTiers[i].Place + " " + RewardTiers[i].Gems.ToString("N0") + "◈";
            Router.Toast(msg, 2.6f);
        }

        // =====================================================================================================
        // CHAMPION CREST (Section C/E/F/G): laurel wreath + "?" shield + CHAMPION + 🏆 10,000. Apex bloom. → SafeContent.
        // =====================================================================================================
        private void BuildChampionCrest()
        {
            float wreath = 0.22f * H;                       // ≈238px
            // crest center placed a touch below the wreath-top (fy≈0.155 from top) so the label/prize fit beneath.
            _crest = UiWidgets.Rect("ChampionCrest", SafeContent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -0.155f * H), new Vector2(wreath + 80, wreath + 180));
            _crestCg = _crest.gameObject.AddComponent<CanvasGroup>();

            // Bloom halo (~+40%, strong) behind the wreath.
            UiWidgets.Glow(_crest, UiTheme.A(Hex("#ffe9a0"), 0.6f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(wreath * 1.5f, wreath * 1.5f), 1.5f);

            // Laurel wreath ring (brushed-gold frame ring stands in for the laurel relief) + idle glow pulse.
            var ring = UiWidgets.Rect("LaurelWreath", _crest, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(wreath, wreath));
            var ringImg = ring.gameObject.AddComponent<Image>(); ringImg.raycastTarget = false;
            ringImg.sprite = UiTex.Frame(Hex("#fff2c2"), Hex("#f0d27a"), Hex("#6b5320"), 64, 14); ringImg.type = Image.Type.Sliced;
            ring.gameObject.AddComponent<PulseScale>().period = 2f; // laurel shimmer/glow breathe
            // Laurel side finials (wreath flourish).
            var fl = UiWidgets.Finial(ring, new Vector2(0.5f, 0f), new Vector2(0, -6), 56f); fl.color = UiTheme.GoldHi;

            // Champion "?" shield (undecided) — dark steel field + gold "?".
            float shield = 0.10f * H;
            var sh = UiWidgets.Rect("ChampShield", ring, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(shield, shield * 1.15f));
            var shImg = sh.gameObject.AddComponent<Image>(); shImg.raycastTarget = false;
            shImg.sprite = UiTex.VGradient(UiWidgets.Lighten(ChampShield, 0.12f), UiWidgets.Darken(ChampShield, 0.35f), 32);
            UiWidgets.Label(sh, "?", 64, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, ChampQ);

            // "CHAMPION" label (below wreath).
            UiWidgets.TitleLabel(_crest, "CHAMPION", 34, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(420, 50), TextAnchor.MiddleCenter, ChampLabel, Hex("#caa04a"));

            // Prize: 🏆 trophy glyph + "10,000" (beneath the label). Verbatim spec prize.
            var prize = UiWidgets.Rect("Prize", _crest, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), Vector2.zero, new Vector2(360, 48));
            var trophy = UiWidgets.Rect("Trophy", prize, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-92, 0), new Vector2(40, 40));
            var trImg = trophy.gameObject.AddComponent<Image>(); trImg.raycastTarget = false; trImg.sprite = UiTex.Diamond(PrizeGold, 32);
            UiWidgets.Label(prize, "10,000", 30, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28, 0), new Vector2(240, 48), TextAnchor.MiddleLeft, PrizeGold);
        }

        // =====================================================================================================
        // BRACKET (Section C/D/E/H): symmetric single-elimination tree in a ScrollRect (build B, pans on small
        // screens, full at 2340×1080). Fixed-anchored nodes (build A) + elbow Image connectors; player path = hot
        // gold. DISPLAY-ONLY: no client-side bracket mutation (Section K/L). → SafeContent.
        // =====================================================================================================
        // Bracket band (Section E): y(from top) 0.20→0.88 → height 0.68·H; inner width 0.96·W.
        private const float BandTopFy = 0.20f, BandBotFy = 0.88f;
        private float BandH => (BandBotFy - BandTopFy) * H;        // ≈734px content height
        private float ContentW => 0.96f * W;                       // ≈2246px content width
        private float NodeW => 0.10f * W;                          // ≈234px tile width
        private float NodeH => 0.055f * H;                         // ≈60px tile height

        // Column X centers (content-local px from left). Left half steps inward; right half mirrors.
        private float ColX(int round /*0=R1,1=R2,2=R3*/, bool left)
        {
            // Spec E: R1≈0.02·W, R2≈0.16·W, R3≈0.28·W (left insets). Node centers = inset + half-tile.
            float[] insetFrac = { 0.02f, 0.16f, 0.28f };
            float xLeft = insetFrac[round] * W + NodeW * 0.5f;
            return left ? xLeft : ContentW - xLeft;
        }
        private float FinalX => ContentW * 0.5f; // shared FINAL center column

        private void BuildBracket()
        {
            // ScrollRect host over the bracket band (anchorY = 1 − fy).
            float bandCy = 1f - (BandTopFy + BandBotFy) * 0.5f; // center anchorY of the band
            var host = UiWidgets.Rect("Bracket", SafeContent, new Vector2(0.5f, bandCy), new Vector2(0.5f, bandCy), Vector2.zero, new Vector2(0.96f * W, BandH));
            _bracketCg = host.gameObject.AddComponent<CanvasGroup>();
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = true; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic; scroll.elasticity = 0.1f;
            scroll.inertia = true; scroll.decelerationRate = 0.135f; scroll.scrollSensitivity = 24f;

            // Masked viewport (fills the host; near-invisible mask graphic).
            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0.001f);
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content (center-pivoted; fixed bracket size — shows fully at 1:1, pans if the viewport is smaller).
            var content = UiWidgets.Rect("Content", viewport, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ContentW, BandH));
            content.pivot = new Vector2(0.5f, 0.5f);
            scroll.viewport = viewport; scroll.content = content;

            // Vertical slot centers for the 8 R1 nodes over the band (content-local y, +up). Spec E: 4 matchups,
            // intra-pair gap small, inter-matchup gap large. Build 4 pair-centers evenly across the band.
            float topY = BandH * 0.5f - NodeH * 0.5f - 8f;   // top of usable band
            float botY = -BandH * 0.5f + NodeH * 0.5f + 8f;
            float pairGap = NodeH * 0.5f + 0.005f * H;        // half-offset of the two nodes in a matchup
            var pairCy = new float[4];
            for (int m = 0; m < 4; m++) pairCy[m] = Mathf.Lerp(topY, botY, (m + 0.5f) / 4f);

            // ---- LEFT half ----
            var r2cyL = new float[2]; var r3cyL = new float[1];
            BuildHalf(content, true, pairCy, pairGap, r2cyL, r3cyL);
            // ---- RIGHT half (mirror) ----
            var r2cyR = new float[2]; var r3cyR = new float[1];
            BuildHalf(content, false, pairCy, pairGap, r2cyR, r3cyR);

            // ---- FINAL center (shared) + feed into the ChampionCrest. ----
            float finalCy = 0f; // band center
            // Connectors from each R3 (semifinal) into the FINAL center node.
            ElbowConnect(content, ColX(2, true),  r3cyL[0], FinalX - NodeW * 0.5f, finalCy, PathHot, true);  // player path side (bright)
            ElbowConnect(content, ColX(2, false), r3cyR[0], FinalX + NodeW * 0.5f, finalCy, PathDim, false);
            BuildNode(content, FinalX, finalCy, S("FINAL", UiTheme.Gold, NodeState.Pending), false);
            // Short stub upward from FINAL toward the crest apex (visual feed).
            VLine(content, FinalX, finalCy + NodeH * 0.5f, BandH * 0.5f, PathDim, 4f);
        }

        // Build one half: R1 (4 matchups / 8 nodes) → R2 (2) → R3 (1), with elbow connectors.
        private void BuildHalf(RectTransform content, bool left, float[] pairCy, float pairGap, float[] r2cy, float[] r3cy)
        {
            float x1 = ColX(0, left), x2 = ColX(1, left), x3 = ColX(2, left);
            var slots = left ? R1L : R1R;

            // R1 nodes + their winners feeding R2.
            var r1WinnerCy = new float[4]; // y of the advancing slot in each matchup (for connectors)
            for (int m = 0; m < 4; m++)
            {
                float topNodeY = pairCy[m] + pairGap;
                float botNodeY = pairCy[m] - pairGap;
                BuildNode(content, x1, topNodeY, slots[m][0], left);
                BuildNode(content, x1, botNodeY, slots[m][1], left);
                // The advancing competitor (winner / YOU) → its y is the connector source.
                r1WinnerCy[m] = (slots[m][0].State == NodeState.Winner || slots[m][0].State == NodeState.You) ? topNodeY : botNodeY;
            }

            // R2 (2 nodes), each centered between its two feeder matchups.
            r2cy[0] = (pairCy[0] + pairCy[1]) * 0.5f;
            r2cy[1] = (pairCy[2] + pairCy[3]) * 0.5f;
            // Player path: on the LEFT half, M3 (index 2) carries YOU → its R2 node (index 1) is the "You" path.
            bool[] r2IsPlayer = { false, false };
            if (left) r2IsPlayer[1] = true;

            // Connectors R1 → R2 (per pair) + R2 nodes.
            for (int j = 0; j < 2; j++)
            {
                int mTop = j * 2, mBot = j * 2 + 1;
                bool topIsPlayerFeed = left && (mTop == 2);  // M3 top-feeds R2[1]
                bool botIsPlayerFeed = left && (mBot == 2);
                ElbowConnect(content, x1, r1WinnerCy[mTop], x2 - NodeW * 0.5f, r2cy[j], topIsPlayerFeed ? PathHot : PathDim, topIsPlayerFeed);
                ElbowConnect(content, x1, r1WinnerCy[mBot], x2 - NodeW * 0.5f, r2cy[j], botIsPlayerFeed ? PathHot : PathDim, botIsPlayerFeed);
                var st = r2IsPlayer[j] ? NodeState.You : NodeState.Pending;
                BuildNode(content, x2, r2cy[j], S(r2IsPlayer[j] ? "YOU" : "—", r2IsPlayer[j] ? FacIP : UiTheme.IronSteel, st), left);
                if (r2IsPlayer[j]) PathLabel(content, x1, r1WinnerCy[mTop == 2 ? mTop : mBot], x2, r2cy[j]); // "You" on the won connector
            }

            // R3 (1 semifinal node), centered between the two R2 nodes.
            r3cy[0] = (r2cy[0] + r2cy[1]) * 0.5f;
            bool r3IsPlayer = left; // player's semifinal sits on the left path
            ElbowConnect(content, x2, r2cy[0], x3 - NodeW * 0.5f, r3cy[0], PathDim, false);
            ElbowConnect(content, x2, r2cy[1], x3 - NodeW * 0.5f, r3cy[0], r3IsPlayer ? PathHot : PathDim, r3IsPlayer);
            BuildNode(content, x3, r3cy[0], S(r3IsPlayer ? "YOU" : "—", r3IsPlayer ? FacIP : UiTheme.IronSteel, r3IsPlayer ? NodeState.You : NodeState.Pending), left);
            if (r3IsPlayer) PathLabel(content, x2, r2cy[1], x3, r3cy[0]);
        }

        // -----------------------------------------------------------------------------------------------------
        // CompetitorNode (Section H): slate tile + faction-tinted frame + portrait disc + name; state overlays.
        // Returns the node rect. Captures a CanvasGroup for the staggered reveal.
        // -----------------------------------------------------------------------------------------------------
        private RectTransform BuildNode(RectTransform content, float cx, float cy, Slot slot, bool left)
        {
            var nodeGo = new GameObject("Node_" + slot.Name, typeof(RectTransform), typeof(CanvasGroup));
            var node = (RectTransform)nodeGo.transform; node.SetParent(content, false);
            node.anchorMin = node.anchorMax = new Vector2(0.5f, 0.5f);
            node.sizeDelta = new Vector2(NodeW, NodeH); node.anchoredPosition = new Vector2(cx, cy);
            _nodeCg.Add(nodeGo.GetComponent<CanvasGroup>());

            bool you = slot.State == NodeState.You;
            bool loser = slot.State == NodeState.Loser;
            bool winner = slot.State == NodeState.Winner;
            bool playable = slot.State == NodeState.NextPlayable;

            // Tile body (slate gradient; loser desaturates ~50% toward grey).
            var bodyImg = nodeGo.AddComponent<Image>();
            Color bTop = NodeFill, bBot = NodeFill2;
            if (loser) { bTop = Color.Lerp(bTop, Hex("#3a3a3a"), 0.5f); bBot = Color.Lerp(bBot, Hex("#2a2a2a"), 0.5f); }
            bodyImg.sprite = UiTex.VGradient(bTop, bBot, 32); bodyImg.raycastTarget = true;

            // YOU node glow behind the tile.
            if (you) UiWidgets.Glow(node, UiTheme.A(YouGold, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(NodeW * 1.25f, NodeH * 2.2f), 1.6f);

            // Faction / state frame: YOU = gold, next-playable = cobalt CTA ring, else faction tint (dim if loser).
            Color frameTint = you ? UiTheme.Gold : playable ? FacIP : slot.Tint;
            if (loser) frameTint = UiWidgets.Darken(frameTint, 0.5f);
            var frame = UiWidgets.Rect("Frame", node, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var frImg = frame.gameObject.AddComponent<Image>(); frImg.raycastTarget = false; frImg.type = Image.Type.Sliced;
            frImg.sprite = UiTex.Frame(you ? Hex("#f0d27a") : UiWidgets.Lighten(frameTint, 0.25f), frameTint, UiWidgets.Darken(frameTint, 0.4f), 48, you || playable ? 7 : 5);

            // next-playable cobalt "play" ring + pulse (this is the tappable match).
            if (playable)
            {
                var ring = UiWidgets.Glow(node, UiTheme.A(FacIP, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(NodeW * 1.3f, NodeH * 2.4f), 1.7f);
                var pr = ring.gameObject.AddComponent<PulseGraphic>(); pr.target = ring; pr.min = 0.3f; pr.max = 0.7f; pr.period = 1.4f;
            }

            // Portrait disc (NO portrait art per Negative Rule — a low-key helmeted-bust stand-in disc; faction tint).
            float pd = NodeH * 0.82f;
            var port = UiWidgets.Rect("Portrait", node, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(pd * 0.65f, 0), new Vector2(pd, pd));
            var portImg = port.gameObject.AddComponent<Image>(); portImg.raycastTarget = false;
            portImg.sprite = UiTex.Disc(UiTheme.A(loser ? Hex("#3a3a3a") : UiWidgets.Darken(slot.Tint, 0.25f), 0.95f), 48);
            var bust = UiWidgets.Rect("Bust", port, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(pd * 0.55f, pd * 0.55f));
            var bustImg = bust.gameObject.AddComponent<Image>(); bustImg.raycastTarget = false; bustImg.sprite = UiTex.Diamond(UiTheme.A(NodeName, 0.55f), 32);

            // Name plate (faction-tinted / gold for YOU).
            var name = UiWidgets.Label(node, slot.Name, you ? 24 : 22, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft, you ? YouGold : loser ? UiTheme.A(NodeName, 0.6f) : NodeName);
            var nrt = (RectTransform)name.transform; nrt.offsetMin = new Vector2(pd + 18f, 0); nrt.offsetMax = new Vector2(-12f, 0);

            // State overlays (Section H).
            if (you)
            {
                // gold star badge (top-left), star twinkle.
                var star = UiWidgets.Rect("Star", node, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -2), new Vector2(0.025f * H, 0.025f * H));
                var stImg = star.gameObject.AddComponent<Image>(); stImg.raycastTarget = false; stImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
                star.gameObject.AddComponent<PulseScale>().period = 1.3f;
            }
            else if (winner)
            {
                // gold ✓ tick.
                var tick = UiWidgets.Rect("Tick", node, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -4), new Vector2(28, 28));
                var tImg = tick.gameObject.AddComponent<Image>(); tImg.raycastTarget = false; tImg.sprite = UiTex.Disc(UiTheme.GoldHi, 32);
                UiWidgets.Label(tick, "✓", 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#2a1c06"));
            }
            else if (loser)
            {
                // grey/cracked dim overlay.
                var crack = UiWidgets.Rect("Crack", node, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var cImg = crack.gameObject.AddComponent<Image>(); cImg.raycastTarget = false; cImg.color = new Color(0.1f, 0.1f, 0.12f, 0.45f);
            }
            else if (slot.State == NodeState.Pending)
            {
                // faint pending pulse on the tile body.
                var pp = nodeGo.AddComponent<PulseGraphic>(); pp.target = bodyImg; pp.min = 0.82f; pp.max = 1f; pp.period = 1.8f;
            }

            // ONLY the player's next-pending match is tappable → confirm → battle vs that stored ghost (async).
            if (playable)
            {
                var btn = nodeGo.AddComponent<Button>(); btn.targetGraphic = bodyImg;
                var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); MatchPresentation.StartMatch("Tournament"); });
            }
            return node;
        }

        // -----------------------------------------------------------------------------------------------------
        // Connectors (Section D/E/H): elbow = horizontal stub from source → vertical join → horizontal into the
        // target node's left/right edge. Player path = hot gold (thicker) + captured for the entry sweep.
        // -----------------------------------------------------------------------------------------------------
        private void ElbowConnect(RectTransform content, float fromX, float fromY, float toEdgeX, float toY, Color col, bool player)
        {
            float thick = player ? 5f : 4f;
            float midX = (fromX + toEdgeX) * 0.5f;
            // Source side is the node's inner edge: shift the start to the tile edge facing center.
            float startX = fromX + (toEdgeX > fromX ? NodeW * 0.5f : -NodeW * 0.5f);
            HLine(content, startX, midX, fromY, col, thick);          // horizontal stub
            VLine(content, midX, fromY, toY, col, thick);             // vertical join
            HLine(content, midX, toEdgeX, toY, col, thick);          // horizontal into target
            // (player-path segments are auto-captured for the entry sweep in HLine/VLine when col == PathHot.)
        }

        private void HLine(RectTransform content, float xa, float xb, float y, Color col, float thick)
        {
            float w = Mathf.Abs(xb - xa); float cx = (xa + xb) * 0.5f;
            var line = MakeLine(content, "H", cx, y, w + thick, thick, col);
            if (col == PathHot) _pathSweep.Add(line);
        }
        private void VLine(RectTransform content, float x, float ya, float yb, Color col, float thick)
        {
            float h = Mathf.Abs(yb - ya); float cy = (ya + yb) * 0.5f;
            var line = MakeLine(content, "V", x, cy, thick, h + thick, col);
            if (col == PathHot) _pathSweep.Add(line);
        }
        private Image MakeLine(RectTransform content, string tag, float cx, float cy, float w, float h, Color col)
        {
            var rt = UiWidgets.Rect("Conn_" + tag, content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cx, cy), new Vector2(w, h));
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.color = col;
            rt.SetAsFirstSibling(); // connectors render beneath nodes
            return img;
        }

        // "You" path label (small gold plate with dark ink) sitting on a won connector segment.
        private void PathLabel(RectTransform content, float fromX, float fromY, float toX, float toY)
        {
            float cx = (fromX + toX) * 0.5f, cy = (fromY + toY) * 0.5f;
            var plate = UiWidgets.Rect("YouPlate", content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cx, cy), new Vector2(0.04f * W, 0.022f * H));
            var pImg = plate.gameObject.AddComponent<Image>(); pImg.raycastTarget = false; pImg.sprite = UiTex.VGradient(Hex("#ffe9a8"), Hex("#e9c24a"), 32);
            UiWidgets.Label(plate, "You", 16, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, YouPlateInk);
        }

        // =====================================================================================================
        // TAB BAR (Section C/E/F): Qualifiers / Tournament(active gold + underline) / Battle Log. → SafeContent.
        // =====================================================================================================
        private void BuildTabBar()
        {
            float h = 0.085f * H;                          // ≈92px
            float cy = 0.01f * H + h * 0.5f;               // bottom inset ≈0.01·H
            var bar = UiWidgets.Rect("TabBar", SafeContent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, cy), new Vector2(0, h));
            _tabBarCg = bar.gameObject.AddComponent<CanvasGroup>();
            var barImg = bar.gameObject.AddComponent<Image>(); barImg.raycastTarget = false; barImg.color = UiTheme.A(Hex("#11141d"), 0.92f);

            string[] tabs = { "QUALIFIERS", "TOURNAMENT", "BATTLE LOG" };
            int active = 1;
            float third = 1f / 3f;
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                float cxFrac = third * (i + 0.5f);
                bool isActive = i == active;
                var tabRt = UiWidgets.Rect("Tab_" + tabs[i], bar, new Vector2(cxFrac, 0.5f), new Vector2(cxFrac, 0.5f), Vector2.zero, new Vector2(third * 0.96f * W, h));
                var hit = tabRt.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0.001f);
                var tb = tabRt.gameObject.AddComponent<Button>(); tb.targetGraphic = hit;
                tb.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.Click();
                    if (idx == active) return; // already on Tournament
                    Router.Toast((idx == 0 ? "Qualifiers" : "Battle Log") + " — coming soon");
                });
                UiWidgets.Label(tabRt, UiTheme.Track(tabs[i]), 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, isActive ? TabActive : TabMuted);
                if (isActive)
                {
                    // active crest glyph + underline + glow.
                    var crestGly = UiWidgets.Rect("ActiveCrest", tabRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), new Vector2(26, 26));
                    var cgImg = crestGly.gameObject.AddComponent<Image>(); cgImg.raycastTarget = false; cgImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
                    var underline = UiWidgets.Rect("Underline", tabRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 6), new Vector2(third * 0.5f * W, 5f));
                    var ulImg = underline.gameObject.AddComponent<Image>(); ulImg.raycastTarget = false; ulImg.sprite = UiTex.HGradient(UiTheme.A(TabActive, 0f), TabActive, 32);
                    var ul2 = underline.gameObject.AddComponent<PulseGraphic>(); ul2.target = ulImg; ul2.min = 0.6f; ul2.max = 1f; ul2.period = 1.8f; // active-tab glow breathe
                }
            }
        }

        // =====================================================================================================
        // LIFECYCLE / ENTRY ANIMATION (Section I timeline, all UNSCALED — menus run at Time.timeScale = 0).
        // =====================================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Entry());
        }

        private IEnumerator Entry()
        {
            // Initialize hidden layers.
            SetCg(_topBarCg, 0f); SetCg(_crestCg, 0f); SetCg(_bracketCg, 0f); SetCg(_tabBarCg, 0f);
            for (int i = 0; i < _nodeCg.Count; i++) SetCg(_nodeCg[i], 0f);
            for (int i = 0; i < _pathSweep.Count; i++) if (_pathSweep[i] != null) { var c = _pathSweep[i].color; c.a = 0f; _pathSweep[i].color = c; }

            // 0.05 TopBar slide-down (fade in).
            yield return FadeCg(_topBarCg, 0.20f);
            // 0.15 ChampionCrest drop-in (y −24→0 + scale 0.9→1.0 + α).
            yield return CrestDrop(0.30f);
            // 0.30 Bracket reveal (fade the band in) + node pop-in stagger.
            SetCg(_bracketCg, 1f);
            yield return StaggerNodes(0.03f, 0.15f);
            // 0.55 Player-path highlight: gold connectors sweep up sequentially.
            yield return PathSweep(0.4f);
            // 0.90 TabBar fade-up.
            yield return FadeCg(_tabBarCg, 0.18f);
        }

        private IEnumerator CrestDrop(float d)
        {
            if (_crestCg == null) { yield break; }
            float t = 0f; var rt = _crest;
            Vector2 baseP = rt != null ? rt.anchoredPosition : Vector2.zero;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                float ease = 1f - Mathf.Pow(1f - k, 3f); // ease-out (back-ish)
                _crestCg.alpha = k;
                if (rt != null) { rt.anchoredPosition = baseP + new Vector2(0, Mathf.Lerp(-24f, 0f, ease)); rt.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, ease); }
                yield return null;
            }
            _crestCg.alpha = 1f; if (rt != null) { rt.anchoredPosition = baseP; rt.localScale = Vector3.one; }
        }

        private IEnumerator StaggerNodes(float step, float each)
        {
            for (int i = 0; i < _nodeCg.Count; i++)
            {
                var cg = _nodeCg[i]; if (cg == null) continue;
                StartCoroutine(PopOne(cg, each));
                float w = 0f; while (w < step) { w += Time.unscaledDeltaTime; yield return null; }
            }
        }
        private IEnumerator PopOne(CanvasGroup cg, float d)
        {
            float t = 0f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); cg.alpha = k; cg.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, k); yield return null; }
            if (cg != null) { cg.alpha = 1f; cg.transform.localScale = Vector3.one; }
        }

        private IEnumerator PathSweep(float d)
        {
            float t = 0f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                for (int i = 0; i < _pathSweep.Count; i++)
                {
                    if (_pathSweep[i] == null) continue;
                    // Light each segment up sequentially toward the crest (staggered along k).
                    float a = Mathf.Clamp01((k - (i / (float)Mathf.Max(1, _pathSweep.Count))) * _pathSweep.Count);
                    var c = _pathSweep[i].color; c.a = a; _pathSweep[i].color = c;
                }
                yield return null;
            }
            for (int i = 0; i < _pathSweep.Count; i++) if (_pathSweep[i] != null) { var c = _pathSweep[i].color; c.a = 1f; _pathSweep[i].color = c; }
        }

        private IEnumerator FadeCg(CanvasGroup cg, float d)
        {
            if (cg == null) yield break;
            float t = 0f;
            while (t < d && cg != null) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(t / d); yield return null; }
            if (cg != null) cg.alpha = 1f;
        }

        private static void SetCg(CanvasGroup cg, float a) { if (cg != null) cg.alpha = a; }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
