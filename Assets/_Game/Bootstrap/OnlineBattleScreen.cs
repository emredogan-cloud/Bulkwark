// BULWARK — ONLINE BATTLE (UI Construction Bible · 32). Presentation-only, REMOVABLE.
//
// §12 summary: forensic code-built uGUI of design/OnlineBattleDesign.png (Sections A–O) — the ranked ASYNC-GHOST
// matchmaking hub: hub chrome + gold "ONLINE BATTLE" title (kicker "ASYNCHRONOUS MATCHMAKING" + "No timers."
// tagline), a mirrored VS tableau (You / Iron Pact blue LEFT vs a REPLAY ghost / Ashen Horde oxblood RIGHT) with
// a central blue↔red clash VS emblem, a season timer, a 5-milestone Season-Rewards trophy track, a wide gold
// FIND MATCH CTA, and four utility tiles. ALL opponent/league/trophy/threshold values are DISPLAY-ONLY local
// stubs (server-authoritative in production; the client never computes/mutates rank, trophies, rewards, or
// matchmaking). NO ECS / NO Unity.Entities / NO gameplay/balance/economy/backend. FIND MATCH routes through the
// existing §12-safe seam MatchPresentation.StartMatch("Online"); every other button is display-only (Toast).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-32 landscape Online Battle (async-ghost matchmaking) hub. Presentation-only; rank/trophies/
    /// rewards/opponent are display-only stubs (server-authoritative in production, §12).</summary>
    public sealed class OnlineBattleScreen : UiScreen
    {
        // ===== Display-only stub data (§K/§L: server-authoritative in production; the UI NEVER writes these). =====
        // Verbatim from the forensic spec (Negative Rule 4 — do NOT invent opponents/leagues/thresholds).
        private const string PlayerFaction = "Iron Pact";
        private const string PlayerLeague  = "Diamond III";
        private const int    PlayerTrophies = 3420;

        private const string OppName     = "Ashen Warlord";
        private const string OppFaction  = "Ashen Horde";
        private const string OppLeague   = "Diamond II";
        private const int    OppTrophies = 3310;
        private const string ReplayNote  = "Battle Replay · 1h ago"; // async-ghost tag — Negative Rule 1

        private const string SeasonTimer = "Season ends in 6d 12h";  // display string only (no live match countdown)

        // Reward-track trophy thresholds (Negative Rule 4) — first 4 chests, final = crown trophy.
        private static readonly int[] Thresholds = { 1000, 1600, 2400, 3200, 3800 };

        // Currency chips (display-only). Gold / Gems verbatim; EnergyChip 58/120 is CANON-CUT (Negative Rule 2) —
        // documented here and OMITTED below (frozen wallet model = Gold + Gems only, UiStub).
        private const int GoldValue = 128450;
        private const int GemValue  = 2850;

        // ===== Forensic hex (§F/§G) not present in UiTheme — via the Hex() helper. =====
        private static readonly Color KickerCol   = Hex("#cdbf99");
        private static readonly Color TaglineCol   = Hex("#b3ac96");
        private static readonly Color IronClothHi  = Hex("#2b56c8"); // Iron Pact cloth #16306e→#2b56c8
        private static readonly Color IronClothLo  = Hex("#16306e");
        private static readonly Color AshClothHi   = Hex("#7a1f1a"); // Ashen Horde cloth #4a1410→#7a1f1a
        private static readonly Color AshClothLo   = Hex("#4a1410");
        private static readonly Color IronName     = Hex("#acd0ff"); // FactionName IP tint
        private static readonly Color AshName      = Hex("#f0a89e"); // FactionName AH tint
        private static readonly Color OppNameCol   = Hex("#e8c9c2"); // red-tinted serif
        private static readonly Color RoleCol      = Hex("#d9c79a");
        private static readonly Color LeagueCol    = Hex("#bfe6ff"); // gem-glow caps
        private static readonly Color GemFacet     = Hex("#bfe6ff");
        private static readonly Color TrophyCol    = Hex("#ffd76a");
        private static readonly Color TimerCol     = Hex("#ffe08a");
        private static readonly Color YouPlateTxt  = Hex("#2a1c06"); // "You" on gold
        private static readonly Color ReplayTxt    = Hex("#ffe0d8"); // "REPLAY" on oxblood
        private static readonly Color SeasonHdr    = Hex("#f2e8cf");
        private static readonly Color SeasonSub    = Hex("#a9a28c");
        private static readonly Color ThreshGold   = Hex("#ffd76a"); // reached
        private static readonly Color ThreshGrey   = Hex("#8a8472"); // locked
        private static readonly Color ConnLocked   = Hex("#3a3a3a"); // connector grey (locked)
        private static readonly Color ReplayNoteCol = Hex("#9a8f8a");
        private static readonly Color CtaEngrave   = Hex("#2a1c06"); // FIND MATCH label engrave
        private static readonly Color UtilCol      = Hex("#cdbf99");
        private static readonly Color ClashBlue    = Hex("#2b56c8");
        private static readonly Color ClashRed     = Hex("#d8452b");
        private static readonly Color PanelTop     = Hex("#161a24"); // SeasonRewards obsidian #0c0e15→#161a24
        private static readonly Color PanelBot     = Hex("#0c0e15");
        private static readonly Color VsCore       = Hex("#f4e6b0");

        // ===== Live refs (count-ups, searching-state swap). =====
        private Text _goldText, _gemText, _playerTrophyText, _oppTrophyText, _ctaLabel;
        private CountUp _playerTrophyCount, _oppTrophyCount;
        private Image _clashGlow;            // intensifies while "searching"
        private Button _findBtn;
        private RectTransform _oppStack;     // cross-fades on re-roll
        private bool _searching;

        // ---------------------------------------------------------------------------------------------------------
        // BUILD
        // ---------------------------------------------------------------------------------------------------------
        protected override void Build()
        {
            BuildBackdrop();   // §C FullBleedBackdrop — full-bleed art + FX live on Rect, never inside SafeContent.
            BuildTopBar();     // §C/§D TopBar — Back, title block, currency chips (energy CUT).
            BuildVSTableau();  // §C/§D VSTableau — mirrored banners + VS emblem + season timer.
            BuildSeasonRewards();
            BuildFindMatch();
            BuildUtilityBar();
        }

        // ===== §C FullBleedBackdrop: dark war-hall at dusk + heavy vignette + central blue/red clash glow. =====
        private void BuildBackdrop()
        {
            UiWidgets.Backdrop(Rect, "onlinebattle"); // war-hall #0a0b0f→#16161e
            var grade = UiWidgets.Stretch("DuskGrade", Rect, Color.white);
            grade.sprite = UiTex.VGradient(UiTheme.A(Hex("#16161e"), 0.0f), UiTheme.A(Hex("#06070b"), 0.7f), 64); grade.raycastTarget = false;

            // Armored figures faintly flank each side (cold motes left, embers right) — §B/§J, decorative.
            UiWidgets.Glow(Rect, UiTheme.A(ClashBlue, 0.14f), new Vector2(0.16f, 0.40f), new Vector2(0.16f, 0.40f), Vector2.zero, new Vector2(520, 720), 1.7f);
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Oxblood, 0.16f), new Vector2(0.84f, 0.40f), new Vector2(0.84f, 0.40f), Vector2.zero, new Vector2(520, 720), 1.7f);
            var embers = UiWidgets.Rect("HordeEmbers", Rect, new Vector2(0.84f, 0.30f), new Vector2(0.84f, 0.30f), Vector2.zero, new Vector2(420, 620));
            var ef = embers.gameObject.AddComponent<EmberField>(); ef.count = 12; ef.color = UiTheme.Ember2;

            // CenterClashGlow (§C/§G/§J) — additive blue↔red collision behind the VS, white-hot core + bloom.
            UiWidgets.Glow(Rect, UiTheme.A(ClashBlue, 0.5f), new Vector2(0.465f, 0.70f), new Vector2(0.465f, 0.70f), Vector2.zero, new Vector2(560, 560), 1.5f);
            UiWidgets.Glow(Rect, UiTheme.A(ClashRed,  0.55f), new Vector2(0.535f, 0.70f), new Vector2(0.535f, 0.70f), Vector2.zero, new Vector2(560, 560), 1.5f);
            _clashGlow = UiWidgets.Glow(Rect, UiTheme.A(Hex("#fff2d8"), 0.4f), new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(620, 620), 1.7f);
            Pulse(_clashGlow, 0.3f, 0.62f, 1.8f); // §I idle clash pulse (1.8s)

            // GodRays (subtle) + heavy vignette (§C/§J).
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.GoldHi, 0.16f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(1500, 900), 1.6f);
            UiWidgets.Vignette(Rect, 0.6f);
        }

        // ===== §C/§D TopBar: Back (left) + TitleBlock (center) + CurrencyChips (right). =====
        private void BuildTopBar()
        {
            // Back top-left → hub (Negative-Rule-safe Pop).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // TitleBlock — three stacked lines, top-center (§E: title cap ~62, kicker ~24, tagline ~22).
            UiWidgets.Glow(SafeContent, UiTheme.A(UiTheme.GoldHi, 0.18f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), Vector2.zero, new Vector2(1100, 240), 1.7f);
            var title = UiWidgets.TitleLabel(SafeContent, "ONLINE BATTLE", 62, PA(0.5f, 0.045f), PA(0.5f, 0.045f), Vector2.zero, new Vector2(1300, 90),
                TextAnchor.MiddleCenter, Hex("#f0d27a"), Hex("#caa04a")); // gold bevel #f0d27a→#caa04a
            var tol = title.gameObject.GetComponent<Outline>(); if (tol != null) { tol.effectColor = UiTheme.A(Hex("#3a2c0e"), 0.95f); tol.effectDistance = new Vector2(2, -2); }

            UiWidgets.Label(SafeContent, UiTheme.Track("ASYNCHRONOUS MATCHMAKING"), 24, PA(0.5f, 0.092f), PA(0.5f, 0.092f), Vector2.zero, new Vector2(1300, 36),
                TextAnchor.MiddleCenter, KickerCol); // kicker — preserves the async framing (Negative Rule 1)
            UiWidgets.Label(SafeContent, "Compete against other Commanders. No timers. No pressure. Just strategy.", 22,
                PA(0.5f, 0.126f), PA(0.5f, 0.126f), Vector2.zero, new Vector2(1500, 32), TextAnchor.MiddleCenter, TaglineCol); // reassurance line (Negative Rule 6)

            // CurrencyChips top-right: Gold rightmost (idx 0), Gems left of it (idx 1).
            // EnergyChip "58/120" is CANON-CUT (Negative Rule 2) → intentionally NOT created.
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, GoldValue, 0, out _);
            _gemText  = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, GemValue, 1, out _);
        }

        // ===== §C/§D VSTableau: mirrored sides about center, VS emblem, season timer. =====
        private void BuildVSTableau()
        {
            // PlayerSide (left, Iron Pact blue, "You" gold tag).
            BuildCommanderSide(left: true,  centerFx: 0.20f, clothHi: IronClothHi, clothLo: IronClothLo,
                tagText: "You", tagBody: UiTheme.Gold, tagTxt: YouPlateTxt, role: "COMMANDER",
                factionName: PlayerFaction, factionTint: IronName, league: PlayerLeague, trophies: PlayerTrophies,
                nameCol: IronName, replayNote: null, out _playerTrophyText, out _playerTrophyCount, out _);

            // OpponentSide (right, Ashen Horde oxblood, "REPLAY" red tag, ghost note).
            BuildCommanderSide(left: false, centerFx: 0.80f, clothHi: AshClothHi, clothLo: AshClothLo,
                tagText: "REPLAY", tagBody: UiTheme.Oxblood, tagTxt: ReplayTxt, role: OppName,
                factionName: OppFaction, factionTint: AshName, league: OppLeague, trophies: OppTrophies,
                nameCol: OppNameCol, replayNote: ReplayNote, out _oppTrophyText, out _oppTrophyCount, out _oppStack);

            BuildVSEmblem();

            // SeasonTimer below the VS (§E center ≈0.43H → fy 0.43). Clock glyph + soft glow. Display string only.
            UiWidgets.Glow(SafeContent, UiTheme.A(TimerCol, 0.2f), PA(0.5f, 0.43f), PA(0.5f, 0.43f), Vector2.zero, new Vector2(460, 90), 1.7f);
            var clock = UiWidgets.Rect("TimerClock", SafeContent, PA(0.5f, 0.43f), PA(0.5f, 0.43f), new Vector2(-0.085f * 2340f, 0), new Vector2(30, 30));
            var ci = clock.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(UiTheme.A(TimerCol, 0.9f), 32);
            UiWidgets.Label(SafeContent, SeasonTimer, 24, PA(0.5f, 0.43f), PA(0.5f, 0.43f), new Vector2(20, 0), new Vector2(0.20f * 2340f, 34), TextAnchor.MiddleCenter, TimerCol);
        }

        // One mirrored commander side: hanging cloth war-banner (crest) + tag + COMMANDER + faction + league + trophies.
        private void BuildCommanderSide(bool left, float centerFx, Color clothHi, Color clothLo,
            string tagText, Color tagBody, Color tagTxt, string role, string factionName, Color factionTint,
            string league, int trophies, Color nameCol, string replayNote,
            out Text trophyText, out CountUp trophyCount, out RectTransform stack)
        {
            float bw = 0.16f * 2340f;  // banner width ≈374px (§E)
            float bh = 0.40f * 1080f;  // banner height ≈432px (§E)
            float bannerFy = 0.30f;    // banner vertical center within the VS band (top ≈0.14H, band ≈0.44H)
            string sideName = left ? "PlayerSide" : "OpponentSide";

            // ---- FactionBanner (cloth, faction tint, stitched gold trim, crest) ----
            UiWidgets.Glow(SafeContent, UiTheme.A(left ? UiTheme.IronBlueHi : UiTheme.Ember2, 0.28f), PA(centerFx, bannerFy), PA(centerFx, bannerFy), Vector2.zero, new Vector2(bw * 1.4f, bh * 1.2f), 1.7f); // rim light
            var banner = UiWidgets.Rect("FactionBanner_" + (left ? "IronPact" : "AshenHorde"), SafeContent, PA(centerFx, bannerFy), PA(centerFx, bannerFy), Vector2.zero, new Vector2(bw, bh));
            var cloth = banner.gameObject.AddComponent<Image>(); cloth.raycastTarget = false; cloth.sprite = UiTex.VGradient(clothHi, clothLo, 64); // matte cloth (does NOT bloom — §G)
            // stitched gold trim down the cloth edges
            var trim = UiWidgets.Rect("Trim", banner, new Vector2(0.5f, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(bw - 12, 0));
            var ti = trim.gameObject.AddComponent<Image>(); ti.raycastTarget = false; ti.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 8); ti.type = Image.Type.Sliced;
            // tattered lower edge (notched diamond cut)
            var tail = UiWidgets.Rect("Tail", banner, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(bw, bw * 0.55f));
            var tli = tail.gameObject.AddComponent<Image>(); tli.raycastTarget = false; tli.color = Hex("#0a0b0f"); tli.sprite = UiTex.Diamond(Hex("#0a0b0f"), 48); tail.localRotation = Quaternion.Euler(0, 0, 45);
            // crest — wreath-and-shield (Pact) / skull-and-spikes (Horde) stand-in: disc + gold rim + glyph
            float cd = bw * 0.52f;
            var crest = UiWidgets.Rect("Crest", banner, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(cd, cd));
            var cimg = crest.gameObject.AddComponent<Image>(); cimg.raycastTarget = false; cimg.sprite = UiTex.Disc(UiWidgets.Darken(clothLo, 0.25f), 64);
            var crim = UiWidgets.Rect("CrestRim", crest, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var crimg = crim.gameObject.AddComponent<Image>(); crimg.raycastTarget = false; crimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); crimg.type = Image.Type.Sliced;
            var gly = UiWidgets.Rect("CrestGlyph", crest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(cd * 0.5f, cd * 0.5f));
            var gimg = gly.gameObject.AddComponent<Image>(); gimg.raycastTarget = false; gimg.sprite = UiTex.Diamond(left ? GemFacet : Hex("#1a0d0b"), 48);

            // ---- Tag plate above the label block ("You" gold / "REPLAY" oxblood) ----
            float tagFy = bannerFy + bh / 1080f * 0.5f + 0.012f; // just under the banner
            var tag = UiWidgets.Rect((left ? "YouTag" : "ReplayTag"), SafeContent, PA(centerFx, tagFy), PA(centerFx, tagFy), Vector2.zero, new Vector2(0.07f * 2340f, 0.034f * 1080f));
            var tagImg = tag.gameObject.AddComponent<Image>(); tagImg.raycastTarget = false;
            tagImg.sprite = UiTex.VGradient(UiWidgets.Lighten(tagBody, 0.2f), UiWidgets.Darken(tagBody, 0.15f), 32);
            if (!left) UiWidgets.Glow(tag, UiTheme.A(UiTheme.Ember2, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.10f * 2340f, 0.06f * 1080f), 1.7f); // red glow signals ghost
            var trim2 = UiWidgets.Rect("TagRim", tag, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tr2 = trim2.gameObject.AddComponent<Image>(); tr2.raycastTarget = false; tr2.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.GoldShadow, 48, 4); tr2.type = Image.Type.Sliced;
            UiWidgets.Label(tag, UiTheme.Track(tagText), 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, tagTxt);

            // ---- Label stack under the tag: RoleLabel/OppName · FactionName · LeagueBadge · Trophies ----
            // RoleLabel "COMMANDER" (player) OR the opponent name "Ashen Warlord" (red-tinted serif).
            float rowFy = tagFy + 0.040f;
            stack = UiWidgets.Rect((left ? "PlayerLabels" : "OpponentLabels"), SafeContent, PA(centerFx, rowFy), PA(centerFx, rowFy), Vector2.zero, new Vector2(0.26f * 2340f, 0.18f * 1080f));
            if (left)
            {
                UiWidgets.Label(stack, UiTheme.Track(role), 24, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(0.26f * 2340f, 30), TextAnchor.MiddleCenter, RoleCol);
            }
            else
            {
                UiWidgets.Glow(stack, UiTheme.A(UiTheme.Ember2, 0.28f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(0.24f * 2340f, 70), 1.7f); // red rim glow
                UiWidgets.Label(stack, role, 40, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -22), new Vector2(0.26f * 2340f, 50), TextAnchor.MiddleCenter, OppNameCol);
            }

            // FactionName (serif small-caps, faction tint, shield/skull glyph).
            var fac = UiWidgets.Rect("FactionNameRow", stack, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, left ? -52 : -66), new Vector2(0.26f * 2340f, 40));
            var facGlyph = UiWidgets.Rect("FactionGlyph", fac, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.085f * 2340f, 0), new Vector2(26, 26));
            var fg = facGlyph.gameObject.AddComponent<Image>(); fg.raycastTarget = false; fg.sprite = UiTex.Diamond(factionTint, 32);
            UiWidgets.Label(fac, factionName, 28, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16, 0), new Vector2(0.24f * 2340f, 36), TextAnchor.MiddleCenter, factionTint);

            // LeagueBadge — faceted gem insignia (⌀ ≈0.07H) + caps label, gem glow.
            float gemD = 0.07f * 1080f;
            var badge = UiWidgets.Rect("LeagueBadge", stack, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, left ? -118 : -132), new Vector2(gemD, gemD));
            UiWidgets.Glow(badge, UiTheme.A(GemFacet, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(gemD * 1.6f, gemD * 1.6f), 1.8f);
            var gemImg = badge.gameObject.AddComponent<Image>(); gemImg.raycastTarget = false; gemImg.sprite = UiTex.Diamond(GemFacet, 48);
            var gemCore = UiWidgets.Rect("GemCore", badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(gemD * 0.5f, gemD * 0.5f));
            var gcImg = gemCore.gameObject.AddComponent<Image>(); gcImg.raycastTarget = false; gcImg.sprite = UiTex.Diamond(Color.white, 48);
            var shimmer = badge.gameObject.AddComponent<PulseGraphic>(); shimmer.target = gemImg; shimmer.min = 0.7f; shimmer.max = 1f; shimmer.period = 3f; // gem shimmer (3s, §I)
            UiWidgets.Label(stack, league, 26, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, left ? -118 - gemD * 0.5f - 20 : -132 - gemD * 0.5f - 20), new Vector2(0.26f * 2340f, 32), TextAnchor.MiddleCenter, LeagueCol);

            // Trophies "🏆 N,NNN" (trophy glyph + count-up number, glow).
            float trophyFy = rowFy + 0.20f;
            var trow = UiWidgets.Rect((left ? "PlayerTrophies" : "OpponentTrophies"), SafeContent, PA(centerFx, trophyFy), PA(centerFx, trophyFy), Vector2.zero, new Vector2(0.24f * 2340f, 0.04f * 1080f));
            UiWidgets.Glow(trow, UiTheme.A(TrophyCol, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 80), 1.7f);
            var tg2 = UiWidgets.Rect("TrophyGlyph", trow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-70, 0), new Vector2(34, 34));
            var tgi = tg2.gameObject.AddComponent<Image>(); tgi.raycastTarget = false; tgi.sprite = UiTex.Diamond(TrophyCol, 48);
            trophyText = UiWidgets.Label(trow, trophies.ToString("N0"), 34, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(24, 0), new Vector2(0.2f * 2340f, 44), TextAnchor.MiddleLeft, TrophyCol);
            trophyText.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.GoldShadow, 0.7f);
            trophyCount = gameObject.AddComponent<CountUp>(); trophyCount.Bind(trophyText, v => v.ToString("N0"), 0f); // §I count-up on show

            // ReplayNote ("Battle Replay · 1h ago") — the async-ghost link (display-only) below the opponent block.
            if (!string.IsNullOrEmpty(replayNote))
            {
                var note = UiWidgets.Button(SafeContent, replayNote, PA(centerFx, trophyFy + 0.042f), PA(centerFx, trophyFy + 0.042f), Vector2.zero, new Vector2(0.2f * 2340f, 0.03f * 1080f), new Color(0, 0, 0, 0), () => Router.Toast("Opponent replay (read-only) — coming soon"), 18);
                var nl = note.GetComponentInChildren<Text>(); if (nl != null) nl.color = ReplayNoteCol;
            }

            // Idle banner sway (slight rotation+drift, 2–3s sine) — §H/§I.
            var sway = banner.gameObject.AddComponent<PulseScale>(); sway.min = 0.99f; sway.max = 1.0f; sway.period = left ? 2.6f : 2.9f;
        }

        // ===== §C VSEmblem: central cast-gold ring + "VS" + blue↔red clash bloom (the focal subject). =====
        private void BuildVSEmblem()
        {
            float emblemFy = 0.30f; // §E vertical center ≈0.30H
            float ringD = 0.16f * 1080f; // ⌀ ≈173px

            // clash halo (extends ~+60% of the ring) — bloom around the focal emblem.
            UiWidgets.Glow(SafeContent, UiTheme.A(ClashBlue, 0.4f), PA(0.485f, emblemFy), PA(0.485f, emblemFy), Vector2.zero, new Vector2(ringD * 1.8f, ringD * 1.8f), 1.6f);
            UiWidgets.Glow(SafeContent, UiTheme.A(ClashRed,  0.42f), PA(0.515f, emblemFy), PA(0.515f, emblemFy), Vector2.zero, new Vector2(ringD * 1.8f, ringD * 1.8f), 1.6f);
            UiWidgets.Glow(SafeContent, UiTheme.A(VsCore, 0.4f), PA(0.5f, emblemFy), PA(0.5f, emblemFy), Vector2.zero, new Vector2(ringD * 1.5f, ringD * 1.5f), 1.7f);

            // cast-gold ring.
            var ring = UiWidgets.Rect("VSRing", SafeContent, PA(0.5f, emblemFy), PA(0.5f, emblemFy), Vector2.zero, new Vector2(ringD, ringD));
            var ri = ring.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(Hex("#fff2c2"), Hex("#f0d27a"), Hex("#6b5320"), 96, 18); ri.type = Image.Type.Sliced; // #6b5320→#f0d27a→#fff2c2
            // orbiting clash sparks (§J) — a slow-spinning faint ring of motes inside the halo.
            var sparkRing = UiWidgets.Rect("ClashSparks", SafeContent, PA(0.5f, emblemFy), PA(0.5f, emblemFy), Vector2.zero, new Vector2(ringD * 1.5f, ringD * 1.5f));
            var sr = sparkRing.gameObject.AddComponent<EmberField>(); sr.count = 12; sr.color = Hex("#ffd27a");
            sparkRing.gameObject.AddComponent<Spin>().degPerSec = 18f;

            // "VS" serif glyph (gold bevel + clash glow + bloom), cap ~80 (§F/§E).
            var vs = UiWidgets.TitleLabel(ring, "VS", 80, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ringD, ringD * 0.8f), TextAnchor.MiddleCenter, VsCore, Hex("#e2a93a"), false);
            var vol = vs.gameObject.GetComponent<Outline>(); if (vol != null) { vol.effectColor = UiTheme.A(Hex("#3a2c0e"), 0.95f); vol.effectDistance = new Vector2(2, -2); }
            ring.gameObject.AddComponent<PulseScale>().period = 2.2f; // gentle focal breathe
        }

        // ===== §C/§D SeasonRewardsPanel: header + (i) + subtitle + 5-milestone HLG track (ScrollRect-ready). =====
        private void BuildSeasonRewards()
        {
            float pw = 0.78f * 2340f;  // ≈1825px
            float ph = 0.16f * 1080f;  // ≈173px
            float panelFy = 0.66f;     // vertical center ≈0.66H (§E)

            var panel = UiWidgets.Rect("SeasonRewardsPanel", SafeContent, PA(0.5f, panelFy), PA(0.5f, panelFy), Vector2.zero, new Vector2(pw, ph));
            var pImg = panel.gameObject.AddComponent<Image>(); pImg.raycastTarget = false; pImg.sprite = UiTex.VGradient(PanelTop, PanelBot, 64); // obsidian, matte (no bloom — §G)
            var pRim = UiWidgets.Rect("PanelRim", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var pr = pRim.gameObject.AddComponent<Image>(); pr.raycastTarget = false; pr.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.6f), UiTheme.A(UiTheme.Gold, 0.5f), UiTheme.GoldShadow, 48, 6); pr.type = Image.Type.Sliced; // bronze edge

            // Header "Season Rewards" + Info (i) icon (top row).
            UiWidgets.Label(panel, "Season Rewards", 28, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -28), new Vector2(pw, 36), TextAnchor.MiddleCenter, SeasonHdr);
            var info = UiWidgets.Rect("InfoIcon", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(150, -28), new Vector2(36, 36));
            var infoImg = info.gameObject.AddComponent<Image>(); infoImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 48);
            var infoBtn = info.gameObject.AddComponent<Button>(); infoBtn.targetGraphic = infoImg;
            infoBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("Win Trophies to unlock season rewards (server-authoritative)"); });
            UiWidgets.Label(info, "i", 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.ParchGold);

            // Subtitle.
            UiWidgets.Label(panel, "Win battles to earn Trophies and unlock season rewards!", 20, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -66), new Vector2(pw, 28), TextAnchor.MiddleCenter, SeasonSub);

            // RewardTrack — HorizontalLayoutGroup (5 equal milestones), with a connector/progress line BEHIND the row.
            float trackW = 0.74f * 2340f;
            var trackHost = UiWidgets.Rect("RewardTrackHost", panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 0.045f * 1080f), new Vector2(trackW, 0.08f * 1080f));

            // connector line behind the row: gold up to the player's reached progress, grey beyond (§D/§H).
            var connBg = UiWidgets.Rect("Connector", trackHost, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(trackW * 0.86f, 4f));
            var cbg = connBg.gameObject.AddComponent<Image>(); cbg.raycastTarget = false; cbg.color = ConnLocked;
            float reachedFrac = Mathf.Clamp01(Mathf.InverseLerp(Thresholds[0], Thresholds[Thresholds.Length - 1], PlayerTrophies));
            var connFill = UiWidgets.Rect("ConnectorFill", connBg, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2((trackW * 0.86f) * reachedFrac, 4f));
            connFill.pivot = new Vector2(0, 0.5f); connFill.anchoredPosition = new Vector2(-(trackW * 0.86f) * 0.5f, 0);
            var cf = connFill.gameObject.AddComponent<Image>(); cf.raycastTarget = false; cf.sprite = UiTex.HGradient(UiTheme.Gold, UiTheme.GoldHi, 32);

            // the milestone row (HLG, 5 equal cells).
            var row = UiWidgets.Rect("RewardTrack", trackHost, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            // determine "next" (first unreached) milestone for the pulse cue.
            int nextIdx = -1;
            for (int i = 0; i < Thresholds.Length; i++) if (PlayerTrophies < Thresholds[i]) { nextIdx = i; break; }

            for (int i = 0; i < Thresholds.Length; i++)
            {
                bool reached = PlayerTrophies >= Thresholds[i];
                bool crown = (i == Thresholds.Length - 1);
                BuildMilestone(row, i, Thresholds[i], reached, crown, isNext: i == nextIdx);
            }

            // progress marker: the player's current trophy position riding the connector (§H).
            var marker = UiWidgets.Rect("ProgressMarker", connBg, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-(trackW * 0.86f) * 0.5f + (trackW * 0.86f) * reachedFrac, 0), new Vector2(22, 22));
            var mk = marker.gameObject.AddComponent<Image>(); mk.raycastTarget = false; mk.sprite = UiTex.Diamond(Hex("#fff2c2"), 32);
            UiWidgets.Glow(marker, UiTheme.A(UiTheme.GoldHi, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70, 70), 1.8f);
        }

        private void BuildMilestone(RectTransform row, int index, int threshold, bool reached, bool crown, bool isNext)
        {
            var cell = UiWidgets.Rect("Milestone" + (index + 1), row, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            cell.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            float chestD = 0.06f * 1080f; // chest ⌀ ≈65px (§E)
            var icon = UiWidgets.Rect(crown ? "CrownTrophy" : "Chest", cell, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(chestD, chestD));
            if (reached) UiWidgets.Glow(icon, UiTheme.A(UiTheme.GoldHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(chestD * 1.7f, chestD * 1.7f), 1.7f); // reached = bright + bloom
            var ic = icon.gameObject.AddComponent<Image>(); ic.raycastTarget = false;
            if (crown)
                ic.sprite = UiTex.Diamond(reached ? Hex("#ffd76a") : ThreshGrey, 48); // gold crown trophy (final), solid gold + bloom
            else
                ic.sprite = UiTex.VGradient(reached ? Hex("#caa04a") : Hex("#3a352c"), reached ? Hex("#7a5e34") : Hex("#23211b"), 64); // aged-wood + bronze / dim
            // chest gold rim/edge
            var rim = UiWidgets.Rect("Rim", icon, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false;
            ri.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, reached ? 0.9f : 0.3f), UiTheme.A(UiTheme.Gold, reached ? 0.8f : 0.25f), UiTheme.GoldShadow, 48, 5); ri.type = Image.Type.Sliced;

            // locked → faint padlock overlay (§H).
            if (!reached)
            {
                var lck = UiWidgets.Rect("Lock", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(chestD * 0.45f, chestD * 0.45f));
                var li = lck.gameObject.AddComponent<Image>(); li.raycastTarget = false; li.sprite = UiTex.Diamond(UiTheme.A(Hex("#cdb887"), 0.7f), 32);
            }
            // next milestone subtly pulses (§H).
            if (isNext) { var p = icon.gameObject.AddComponent<PulseScale>(); p.min = 0.96f; p.max = 1.06f; p.period = 1.2f; }

            // threshold "🏆 N,NNN" beneath — reached gold / locked grey.
            var th = UiWidgets.Rect("ThresholdRow", cell, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 18), new Vector2(0.13f * 2340f, 28));
            var tgl = UiWidgets.Rect("ThGlyph", th, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-52, 0), new Vector2(22, 22));
            var tgi = tgl.gameObject.AddComponent<Image>(); tgi.raycastTarget = false; tgi.sprite = UiTex.Diamond(reached ? ThreshGold : ThreshGrey, 32);
            UiWidgets.Label(th, threshold.ToString("N0"), 20, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14, 0), new Vector2(0.12f * 2340f, 26), TextAnchor.MiddleLeft, reached ? ThreshGold : ThreshGrey);
        }

        // ===== §C/§D FindMatchButton: wide brushed-gold CTA (the brightest interactive object). =====
        private void BuildFindMatch()
        {
            float cw = 0.42f * 2340f;  // ≈983px (§E)
            float ch = 0.10f * 1080f;  // ≈108px
            float ctaFy = 0.81f;       // center ≈0.81H (§E)

            // breathing outer glow (§I/§J).
            var glow = UiWidgets.Glow(SafeContent, UiTheme.A(UiTheme.GoldHi, 0.5f), PA(0.5f, ctaFy), PA(0.5f, ctaFy), Vector2.zero, new Vector2(cw * 1.2f, ch * 2.4f), 1.7f);
            Pulse(glow, 0.35f, 0.7f, 1.6f); // FIND MATCH breathe (1.6s)

            _findBtn = UiWidgets.GemButton(SafeContent, "FIND MATCH", PA(0.5f, ctaFy), PA(0.5f, ctaFy), Vector2.zero, new Vector2(cw, ch),
                UiTheme.Gold, OnFindMatch, 46, false, false, TextAnchor.MiddleCenter);
            // re-skin body to brushed gold (top highlight → body) per §G.
            var body = _findBtn.GetComponent<Image>(); if (body != null) body.sprite = UiTex.VGradient(Hex("#ffe9a8"), Hex("#caa04a"), 32);
            // dark engrave label + top highlight (§F: ~46, +8% track, #2a1c06 on gold).
            _ctaLabel = _findBtn.GetComponentInChildren<Text>();
            if (_ctaLabel != null)
            {
                _ctaLabel.text = UiTheme.Track("FIND MATCH");
                _ctaLabel.color = CtaEngrave;
                var g = _ctaLabel.gameObject.GetComponent<UiGradientText>(); if (g != null) { g.top = Hex("#3a2c0e"); g.bottom = Hex("#1a1206"); }
                var ol = _ctaLabel.gameObject.GetComponent<Outline>(); if (ol != null) ol.effectColor = UiTheme.A(Hex("#fff2c2"), 0.5f); // top highlight engrave
            }
            _findBtn.gameObject.AddComponent<PulseScale>().period = 1.6f; // idle breathe loop
        }

        // ===== §C/§D UtilityBar: Battle Log + Defense Log (left), Leaderboard + Shop (right). =====
        private void BuildUtilityBar()
        {
            float y = 0.06f;  // anchorY (bottom band fy ≈0.90→0.99 → anchorY ≈0.10→0.01; tiles centered ~0.06)
            // Left cluster (bottom-left).
            UtilTile("Battle Log",  0.085f, y, () => Router.Toast("Battle Log (your offenses) — coming soon"));
            UtilTile("Defense Log", 0.205f, y, () => Router.Toast("Defense Log (attacks against you) — coming soon"));
            // Right cluster (bottom-right) — Leaderboard → 34, Shop → 17.
            UtilTile("Leaderboard", 0.795f, y, () => Router.Toast("Leaderboard — coming soon"));
            UtilTile("Shop",        0.915f, y, () => Router.Show<StoreScreen>());
        }

        private void UtilTile(string label, float fx, float fy, UnityEngine.Events.UnityAction onClick)
            => UiWidgets.IconTile(SafeContent, label, new Vector2(fx, fy), new Vector2(fx, fy), Vector2.zero, 0.05f * 1080f * 1.6f, UiWidgets.Grey, onClick);

        // ---------------------------------------------------------------------------------------------------------
        // EVENT BEHAVIOR (§K) — display-only. The opponent is a stored GHOST/replay; trophies/league/rewards/
        // matchmaking are server-authoritative; the client never mutates them.
        // ---------------------------------------------------------------------------------------------------------
        // FIND MATCH (§H/§K): press → "FINDING…" + clash intensifies → (scout) → commit into the §12-safe battle
        // seam vs the ghost's defense layout. We model the brief async "searching" beat, then hand off.
        private void OnFindMatch()
        {
            if (_searching) return;
            _searching = true;
            StartCoroutine(FindSequence());
        }

        private IEnumerator FindSequence()
        {
            // searching state: lock CTA, swap label to FINDING…, intensify the clash glow, dim opponent ("Scouting…").
            if (_findBtn != null) _findBtn.interactable = false;
            if (_oppStack != null) { var g = _oppStack.gameObject.GetComponent<CanvasGroup>(); if (g == null) g = _oppStack.gameObject.AddComponent<CanvasGroup>(); g.alpha = 0.5f; }
            var clashPulse = _clashGlow != null ? _clashGlow.GetComponent<PulseGraphic>() : null;
            if (clashPulse != null) { clashPulse.min = 0.55f; clashPulse.max = 0.95f; clashPulse.period = 0.5f; } // spike on FIND MATCH

            float t = 0f; const float d = 1.1f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime;
                if (_ctaLabel != null)
                {
                    int dots = 1 + (int)(Time.unscaledTime * 3f) % 3; // animated "FINDING." dots
                    _ctaLabel.text = UiTheme.Track("FINDING") + new string('.', dots);
                }
                yield return null;
            }
            // commit → the existing §12-safe battle seam (Mode Select → Match Intro → battle).
            MatchPresentation.StartMatch("Online");
        }

        // ---------------------------------------------------------------------------------------------------------
        public override void OnShow()
        {
            if (_goldText != null) _goldText.text = GoldValue.ToString("N0");
            if (_gemText != null)  _gemText.text  = GemValue.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
            // §I entry: trophy numbers count-up (client animates server-auth values; never resolves them).
            _playerTrophyCount?.To(PlayerTrophies, 0.5f);
            _oppTrophyCount?.To(OppTrophies, 0.5f);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------------------------------------
        // Point-anchor using the spec's vertical rhythm: fy = fraction from the TOP → anchorY = 1 − fy.
        private static Vector2 PA(float fx, float fyFromTop) => new Vector2(fx, 1f - fyFromTop);

        // Attach a documented PulseGraphic (sine alpha breathe, unscaled time) to a Graphic.
        private static void Pulse(Graphic g, float min, float max, float period)
        {
            var p = g.gameObject.AddComponent<PulseGraphic>();
            p.target = g; p.min = min; p.max = max; p.period = period;
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
