// BULWARK — LUCKY SPIN (UI Construction Bible · 29). Presentation-only, REMOVABLE.
//
// §12: NO ECS / Unity.Entities / gameplay / balance / AI / economy / backend. This is a code-built uGUI screen on
// the existing UiRouter shell — it only builds + animates Images/Text. The prize wheel, segment rewards, countdown,
// and "recent wins" are DISPLAY-ONLY local stub data; the client NEVER writes a balance or implies a server roll.
//
// ADR SUMMARY (Bible 29 §A/§L · 00 §5): Lucky Spin is randomized-reward GACHA colliding with the principled "no loot
// boxes" CUT, so it is ⚠️ ADR-GATED — this file reproduces the *visual* spec ONLY (the art exactly as drawn) and does
// NOT authorize shipping the paid/random mechanic. Per task: SPIN is display-only — it eases to a random PRESENTATION
// segment, then toasts the prize (optional UiStub.GrantGems, a fake wallet); in production the landing is server-
// authoritative (client never picks the prize), gems are never charged before the ×10 confirm sheet (37), and the
// "Better rewards guaranteed!" copy + posted odds live in the ADR, not here. The flag is kept visible to implementers.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-29 landscape Lucky Spin prize wheel. Presentation-only, ADR-gated gacha (display-only stub data; §12).</summary>
    public sealed class LuckySpinScreen : UiScreen
    {
        // Reference canvas (CanvasScaler 2340×1080, match height). Layout math is fractions of these (Section E).
        // anchorY = 1 − fy_from_top ; px = fraction × (W | H).
        private const float W = 2340f, H = 1080f;

        // Wheel geometry (Section E): side = min(0.50·W, 0.80·H) = 864 (height-bound); R = 432.
        private const float WheelSide = 864f;
        private const float R = WheelSide * 0.5f;     // 432
        private const int Segs = 8;                   // 8 segments × 45°
        private const float SegDeg = 360f / Segs;     // 45

        // ---- Segment reward set (Section C/G) — DISPLAY-ONLY stub. Fixed at 8; NO odds/probabilities invented (§L). ----
        // Clockwise reading from the violet "500 GEMS" at top in the still. Carries NO economy/balance meaning (§12).
        private struct Prize { public string Label; public Color Top; public Color Bottom; public Color Ink; public bool GrantsGems; public int Gems; }
        private static readonly Prize[] Prizes =
        {
            // Seg0 — amethyst "500 GEMS"  (top in the still)            #4a2a7a→#9e6bf0
            new Prize{ Label="500\nGEMS",          Top=Hex("#9e6bf0"), Bottom=Hex("#4a2a7a"), Ink=Hex("#f4ecff"), GrantsGems=true, Gems=500 },
            // Seg1 — steel "100K SILVER"                                #3a414f→#7c8696
            new Prize{ Label="100K\nSILVER",       Top=Hex("#7c8696"), Bottom=Hex("#3a414f"), Ink=Hex("#eef1f6") },
            // Seg2 — bronze/gold "EPIC CHEST"                           #5a431c→#caa04a
            new Prize{ Label="EPIC\nCHEST",        Top=Hex("#caa04a"), Bottom=Hex("#5a431c"), Ink=Hex("#fff4d2") },
            // Seg3 — ember "COMMANDER SHARD ×10"                        #7a3a14→#e0742a
            new Prize{ Label="CMDR\nSHARD ×10",    Top=Hex("#e0742a"), Bottom=Hex("#7a3a14"), Ink=Hex("#fff0e2") },
            // Seg4 — cobalt "250 GEMS"                                  #1f356f→#4f8bff
            new Prize{ Label="250\nGEMS",          Top=Hex("#4f8bff"), Bottom=Hex("#1f356f"), Ink=Hex("#eaf2ff"), GrantsGems=true, Gems=250 },
            // Seg5 — moss "1 DAY SPEED UP"                              #294a2c→#5aa05f
            new Prize{ Label="1 DAY\nSPEED UP",    Top=Hex("#5aa05f"), Bottom=Hex("#294a2c"), Ink=Hex("#ecf7ec") },
            // Seg6 — dark-bronze "RARE CHEST"                           #3a2c14→#8a6a30
            new Prize{ Label="RARE\nCHEST",        Top=Hex("#8a6a30"), Bottom=Hex("#3a2c14"), Ink=Hex("#f6ead0") },
            // Seg7 — slate "EXCLUSIVE AVATAR"                           #2a2f3a→#5a6272
            new Prize{ Label="EXCLUSIVE\nAVATAR",  Top=Hex("#5a6272"), Bottom=Hex("#2a2f3a"), Ink=Hex("#eef1f6") },
        };

        // ---- "How it works" exact bullets + recent-wins feed (Section C) — DISPLAY-ONLY stub. ----
        private static readonly string[] HowBullets = { "Spin daily for free", "Win rewards", "Better rewards on 10x spins!" };
        private struct Win { public string Name; public string Reward; public string Ago; public Color Tint; }
        private static readonly Win[] RecentWins =
        {
            new Win{ Name="ThaneOrlok",   Reward="50K Silver",          Ago="2m ago",  Tint=Hex("#c8ccd6") },
            new Win{ Name="LadyMorrigan", Reward="250 Gems",            Ago="5m ago",  Tint=Hex("#6f8fff") },
            new Win{ Name="Grimblade",    Reward="Rare Chest",          Ago="8m ago",  Tint=Hex("#d9b06a") },
            new Win{ Name="IronWolf",     Reward="Exclusive Avatar",    Ago="12m ago", Tint=Hex("#c8ccd6") },
            new Win{ Name="ValenShield",  Reward="100K Silver",         Ago="15m ago", Tint=Hex("#c8ccd6") },
            new Win{ Name="Stormrider",   Reward="Commander Shard ×10", Ago="18m ago", Tint=Hex("#d9b06a") },
        };

        private const int X10Cost = 450; // gem cost of the paid ×10 spin (Section C/M).

        // Runtime handles.
        private Text _goldText, _gemText, _countdownText, _freeSubText;
        private RectTransform _discRt;          // the rotating disc (segments + labels + spokes)
        private RectTransform _pointerRt;        // fixed top marker (recoils on settle)
        private Image _hubGlow;                  // lion-hub specular pulse
        private Button _freeBtn, _x10Btn;
        private CanvasGroup _freeCg, _x10Cg;
        private Text _x10CostText;
        private int _countdownSeconds = 3 * 3600 + 11 * 60; // 03:11:00 (Section C/F) — display-only
        private bool _freeAvailable = true;
        private bool _spinning;

        protected override void Build()
        {
            // ============================================================================================
            // FULL-BLEED BACKDROP (Rect; OUTSIDE safe area → bleeds edge-to-edge under the cutout, Section D).
            // DimScrim → Vignette → GodRayCone → WheelFocalGlow, all non-interactive.
            // ============================================================================================
            UiWidgets.Stretch("Bg_FullBleed", Rect, UiTheme.Obsidian, "bg_menu");          // dimmed hub backdrop
            UiWidgets.Stretch("DimScrim", Rect, new Color(0f, 0f, 0f, 0.68f));             // black ~68% (Section C)
            UiWidgets.Vignette(Rect, 0.7f);                                                // heavy vignette (Section B)
            // GodRayCone — warm additive cone behind the wheel (slow), centered on the wheel focal (0.50 W, 0.52 H).
            var cone = UiWidgets.Glow(Rect, UiTheme.A(Hex("#f4dca0"), 0.30f), new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), new Vector2(0, 80), new Vector2(1200, 1120), 1.7f);
            var conePulse = cone.gameObject.AddComponent<PulseGraphic>(); conePulse.target = cone; conePulse.min = 0.22f; conePulse.max = 0.38f; conePulse.period = 3.2f; // focal glow slow pulse (Section I)
            // WheelFocalGlow — strong radial halo behind the wheel rim (Section B/D).
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#ffd98a"), 0.34f), new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), Vector2.zero, new Vector2(WheelSide * 1.35f, WheelSide * 1.35f), 1.5f);
            // Dust motes drifting in the dark field (Section J).
            var moteHost = UiWidgets.Rect("FX_Motes", Rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1700, 1000));
            var mef = moteHost.gameObject.AddComponent<EmberField>(); mef.count = 16; mef.color = Hex("#ffe0a0");

            // ============================================================================================
            // INHERITED HUB CHROME (low-alpha bleed; raycast OFF; non-interactive — Section C/D, Negative-Rule L).
            // Currency chips Gold 125,450 / Gems 2,850 (Section C/M). Built first so the close ✕ sits above.
            // ============================================================================================
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);
            _gemText  = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);

            // ============================================================================================
            // SAFE-AREA CONTENT (SafeContent is already the safe-area-inset root, Section D).
            // ============================================================================================
            // Top-left BACK (required navigation seam) → Router.Pop().
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            // CloseButton ✕ top-right (Section C/D · H) — dark disc + bronze ring; also pops (ignored mid-spin, §K).
            BuildCloseButton();

            // ---- Header (top-center): Title "LUCKY SPIN" + subtitle (Section C/E/F). ----
            // Title cap region ~0.09·H, top inset ~0.05·H → anchorY ≈ 0.95. Gold-bevel serif (#f0d27a→#caa04a).
            UiWidgets.Glow(SafeContent, UiTheme.A(Hex("#f4dca0"), 0.28f), new Vector2(0.5f, 0.945f), new Vector2(0.5f, 0.945f), Vector2.zero, new Vector2(900, 200), 1.7f);
            UiWidgets.TitleLabel(SafeContent, "LUCKY SPIN", 72, new Vector2(0.5f, 0.945f), new Vector2(0.5f, 0.945f), Vector2.zero, new Vector2(1100, 130), TextAnchor.MiddleCenter, Hex("#f0d27a"), Hex("#caa04a"));
            UiWidgets.Label(SafeContent, "Spin the wheel and win amazing rewards!", 24, new Vector2(0.5f, 0.875f), new Vector2(0.5f, 0.875f), Vector2.zero, new Vector2(1200, 44), TextAnchor.MiddleCenter, Hex("#d9c79a"));

            // ---- Three-column composition + bottom ticker (Section A/C/E). ----
            BuildLeftColumn();
            BuildWheel();
            BuildRightColumn();
            BuildRecentWins();
        }

        // =================================================================================================
        // CLOSE ✕ (top-right) — dark disc + bronze ring; hover brighten (Section H). Pops the screen.
        // =================================================================================================
        private void BuildCloseButton()
        {
            var rt = UiWidgets.Rect("CloseButton", SafeContent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-90, -90), new Vector2(96, 96));
            var disc = rt.gameObject.AddComponent<Image>(); disc.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = disc;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); if (!_spinning) Router.Pop(); }); // ignore close mid-spin (§K)
            var ring = UiWidgets.Rect("Ring", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = ring.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); ri.type = Image.Type.Sliced;
            UiWidgets.Label(rt, "✕", 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#d9c79a"));
        }

        // =================================================================================================
        // LEFT INFO COLUMN — NextSpinCard (label + clock + countdown) over HowItWorksCard (header + 3 bullets).
        // Width ≈0.20·W=468; left inset ≈0.03·W → center x≈0.13; vertical center anchorY 0.48 (Section E).
        // =================================================================================================
        private void BuildLeftColumn()
        {
            const float colW = 0.20f * W;       // ≈468
            const float cx = 0.13f;             // column center (left inset 0.03 + half width 0.10)

            // NextSpinCard — height ≈0.16·H=173. Top of card area: anchorY ≈ 0.60.
            var nextCard = UiWidgets.Card(SafeContent, new Vector2(cx, 0.60f), new Vector2(cx, 0.60f), Vector2.zero, new Vector2(colW, 0.16f * H), UiTheme.A(Hex("#0c0e15"), 0.94f));
            // "NEXT FREE SPIN" condensed caps (Section F).
            UiWidgets.Label(nextCard, UiTheme.Track("NEXT FREE SPIN"), 22, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero, new Vector2(colW - 40, 34), TextAnchor.MiddleCenter, Hex("#cdbf99"));
            // ClockIcon (gold disc + hands stand-in) left of the countdown.
            var clk = UiWidgets.Rect("ClockIcon", nextCard, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(-128, 0), new Vector2(56, 56));
            var clkImg = clk.gameObject.AddComponent<Image>(); clkImg.raycastTarget = false; clkImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Gold, 0.9f), 48);
            UiWidgets.Divider(clk, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 22, 4, UiTheme.Obsidian);           // minute hand
            var hand2 = UiWidgets.Rect("HandV", clk, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4, 18)); // hour hand
            var h2i = hand2.gameObject.AddComponent<Image>(); h2i.raycastTarget = false; h2i.color = UiTheme.Obsidian;
            // Countdown "03:11:00" — mono/numeric, soft glow (Section F).
            UiWidgets.Glow(nextCard, UiTheme.A(Hex("#ffe08a"), 0.25f), new Vector2(0.5f, 0.40f), new Vector2(0.56f, 0.40f), new Vector2(20, 0), new Vector2(300, 90), 1.7f);
            _countdownText = UiWidgets.Label(nextCard, FormatHms(_countdownSeconds), 40, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(28, 0), new Vector2(280, 56), TextAnchor.MiddleLeft, Hex("#ffe08a"));

            // HowItWorksCard — height ≈0.26·H=281; gap 0.03·H below NextSpinCard → anchorY ≈ 0.385.
            var howCard = UiWidgets.Card(SafeContent, new Vector2(cx, 0.385f), new Vector2(cx, 0.385f), Vector2.zero, new Vector2(colW, 0.26f * H), UiTheme.A(Hex("#0c0e15"), 0.94f));
            UiWidgets.Label(howCard, "How it works", 24, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(colW - 40, 36), TextAnchor.MiddleCenter, Hex("#e8dcc0"));
            UiWidgets.Divider(howCard, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero, colW - 70, 3, UiTheme.A(UiTheme.Gold, 0.7f));
            // 3 exact bullets (Section C/F).
            float by = 0.64f;
            for (int i = 0; i < HowBullets.Length; i++)
            {
                var dot = UiWidgets.Rect("Bullet", howCard, new Vector2(0.10f, by), new Vector2(0.10f, by), Vector2.zero, new Vector2(16, 16));
                var di = dot.gameObject.AddComponent<Image>(); di.raycastTarget = false; di.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
                UiWidgets.Label(howCard, HowBullets[i], 20, new Vector2(0.16f, by), new Vector2(0.16f, by), new Vector2(0, 0), new Vector2(colW - 90, 60), TextAnchor.MiddleLeft, Hex("#c7bfa8"));
                by -= 0.21f;
            }
        }

        // =================================================================================================
        // WHEEL GROUP (center) — RimFrame → Disc (8 segments + spokes + labels, ROTATES) → Hub (lion boss) →
        // Pointer (fixed top). Square, side 864; center x 0.50, vertical center anchorY 0.47 (Section D/E).
        // =================================================================================================
        private void BuildWheel()
        {
            var grp = UiWidgets.Rect("WheelGroup", SafeContent, new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), Vector2.zero, new Vector2(WheelSide, WheelSide));

            // --- WheelDisc (rotates about center pivot) — built FIRST so the rim + hub overlay it. ---
            var disc = UiWidgets.Rect("WheelDisc", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(WheelSide, WheelSide));
            disc.pivot = new Vector2(0.5f, 0.5f); // center pivot (rotation about center)
            _discRt = disc;
            // Disc base plate (dark, fills gaps between the radial segment wedges).
            var plate = disc.gameObject.AddComponent<Image>(); plate.raycastTarget = false; plate.sprite = UiTex.Disc(Hex("#14131a"), 64);

            // 8 pie segments arranged radially around center at 45° steps. Each is a tinted wedge (diamond rotated
            // to read as a pie slice) + an upright reward label at ≈0.62·R. NOTE: the still has the violet "500
            // GEMS" segment at TOP (12 o'clock), so segment i sits centered at angle (i × 45°) measured clockwise
            // from the top. In Unity z-rotation is CCW, so localEulerAngles.z = −(i × 45°).
            for (int i = 0; i < Segs; i++)
            {
                float midDeg = i * SegDeg;                       // clockwise from top (still reading)
                var p = Prizes[i];

                // Wedge: a diamond silhouette with its bottom tip pinned at center, rotated to point outward along the
                // segment mid-angle. Tiling 8 of these around the center reads as a segmented pie (the dark base plate
                // fills micro-gaps; the gold spokes draw the dividing lines). The diamond's lower half is the visible
                // pie slice; tint it with the segment's rim jewel tone (Section G material per segment).
                var wedge = UiWidgets.Rect("Seg" + i + "_Wedge", disc, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(R * 1.0f, R * 2.0f));
                wedge.pivot = new Vector2(0.5f, 0f);             // bottom tip at center
                wedge.anchoredPosition = Vector2.zero;
                wedge.localRotation = Quaternion.Euler(0, 0, -midDeg);
                var wi = wedge.gameObject.AddComponent<Image>(); wi.raycastTarget = false; wi.sprite = UiTex.Diamond(Color.white, 64); wi.color = p.Top;
                // A darker inner triangle toward the hub suggests the radial rich→dark jewel gradient (Section G).
                var wInner = UiWidgets.Rect("Seg" + i + "_Inner", wedge, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(R * 0.5f, R * 1.0f));
                wInner.pivot = new Vector2(0.5f, 0f);
                var wii = wInner.gameObject.AddComponent<Image>(); wii.raycastTarget = false; wii.sprite = UiTex.Diamond(Color.white, 48); wii.color = p.Bottom;

                // Gold divider spoke between segments (thin radial rule from center to rim).
                var spoke = UiWidgets.Rect("Spoke" + i, disc, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, R));
                spoke.pivot = new Vector2(0.5f, 0f);
                spoke.anchoredPosition = Vector2.zero;
                spoke.localRotation = Quaternion.Euler(0, 0, -(midDeg + SegDeg * 0.5f));
                var spi = spoke.gameObject.AddComponent<Image>(); spi.raycastTarget = false; spi.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Gold, 0.2f), UiTheme.GoldHi, 32);

                // Reward icon (≈0.20·R) centered ≈0.55·R out along the segment mid-angle.
                Vector2 iconPos = Polar(midDeg, 0.55f * R);
                var icon = UiWidgets.Rect("Seg" + i + "_Icon", disc, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), iconPos, new Vector2(0.20f * R, 0.20f * R));
                var ici = icon.gameObject.AddComponent<Image>(); ici.raycastTarget = false; ici.sprite = UiTex.Diamond(UiWidgets.Lighten(p.Top, 0.35f), 48);

                // Segment label at ≈0.74·R, rotated so the text faces outward AND is upright at rest. The disc rests
                // unrotated (idle is rim shimmer, not disc spin — see OnShow), so an upright label keeps acceptance
                // criterion #1 ("upright labels"); rotation just orients each ring item to its radius.
                Vector2 labelPos = Polar(midDeg, 0.74f * R);
                var lbl = UiWidgets.Label(disc, p.Label, 22, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), labelPos, new Vector2(170, 80), TextAnchor.MiddleCenter, p.Ink);
                lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
                // Orient text upright relative to the segment: small tilt toward its mid-angle for the radial read,
                // clamped so labels never read upside-down (top/side segments stay legible).
                float tilt = NormalizeTilt(midDeg);
                lbl.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            }

            // --- WheelRimFrame (ornate gold ring + studs) — overlays the disc edge. ---
            var rim = UiWidgets.Rect("WheelRimFrame", grp, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false;
            rimImg.sprite = UiTex.Frame(Hex("#fff2c2"), Hex("#caa04a"), Hex("#6b5320"), 64, 14); rimImg.type = Image.Type.Sliced; // brushed antique gold/bronze (Section G)
            // Gem studs every 45°, ≈0.05·R, just inside the rim. Twinkle (Section J).
            for (int i = 0; i < Segs; i++)
            {
                Vector2 studPos = Polar(i * SegDeg + SegDeg * 0.5f, R - 22f);
                var stud = UiWidgets.Rect("Stud" + i, grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), studPos, new Vector2(0.05f * R, 0.05f * R));
                var sti = stud.gameObject.AddComponent<Image>(); sti.raycastTarget = false; sti.sprite = UiTex.Disc(i % 2 == 0 ? Hex("#4f8bff") : Hex("#d8452b"), 32);
                var stp = stud.gameObject.AddComponent<PulseGraphic>(); stp.target = sti; stp.min = 0.55f; stp.max = 1f; stp.period = 1.4f + i * 0.07f; // gem studs twinkle
            }

            // --- WheelHub (lion-head gold boss, static, on top) — ⌀≈0.34·R=147 (Section E). ---
            _hubGlow = UiWidgets.Glow(grp, UiTheme.A(UiTheme.GoldHi, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.5f * R, 0.5f * R), 1.6f);
            var hubp = _hubGlow.gameObject.AddComponent<PulseGraphic>(); hubp.target = _hubGlow; hubp.min = 0.45f; hubp.max = 0.8f; hubp.period = 3f; // hub specular shimmer loop (Section I)
            var hub = UiWidgets.Rect("WheelHub", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.34f * R, 0.34f * R));
            var hubImg = hub.gameObject.AddComponent<Image>(); hubImg.raycastTarget = false; hubImg.sprite = UiTex.Disc(Hex("#caa04a"), 64);
            var hubRim = UiWidgets.Rect("HubRim", hub, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hri = hubRim.gameObject.AddComponent<Image>(); hri.raycastTarget = false; hri.sprite = UiTex.Frame(Hex("#fff2c2"), Hex("#caa04a"), Hex("#6b5320"), 48, 7); hri.type = Image.Type.Sliced;
            // Lion-boss stand-in: cast-gold relief face (diamond muzzle + two cobalt eyes + inset gem).
            var muzzle = UiWidgets.Rect("LionMuzzle", hub, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(0.5f * R * 0.55f, 0.5f * R * 0.55f));
            var mzi = muzzle.gameObject.AddComponent<Image>(); mzi.raycastTarget = false; mzi.sprite = UiTex.Diamond(Hex("#f6d77a"), 48);
            var eyeL = UiWidgets.Rect("LionEyeL", hub, new Vector2(0.38f, 0.60f), new Vector2(0.38f, 0.60f), Vector2.zero, new Vector2(14, 14));
            var eli = eyeL.gameObject.AddComponent<Image>(); eli.raycastTarget = false; eli.sprite = UiTex.Disc(Hex("#2b56c8"), 32);
            var eyeR = UiWidgets.Rect("LionEyeR", hub, new Vector2(0.62f, 0.60f), new Vector2(0.62f, 0.60f), Vector2.zero, new Vector2(14, 14));
            var eri = eyeR.gameObject.AddComponent<Image>(); eri.raycastTarget = false; eri.sprite = UiTex.Disc(Hex("#2b56c8"), 32);

            // --- WheelPointer (fixed top marker/crest, tip touching the rim inner edge) — height ≈0.16·R (Section E). ---
            var pointer = UiWidgets.Rect("WheelPointer", grp, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -R * 0.04f), new Vector2(0.16f * R, 0.16f * R));
            _pointerRt = pointer;
            var pImg = pointer.gameObject.AddComponent<Image>(); pImg.raycastTarget = false; pImg.sprite = UiTex.Diamond(Hex("#f0d27a"), 48);
            pointer.localRotation = Quaternion.Euler(0, 0, 45f); // diamond → downward-pointing arrowhead at 12 o'clock
            UiWidgets.Glow(pointer, UiTheme.A(UiTheme.GoldHi, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60), 1.6f); // slight glow at tip (Section G)
        }

        // =================================================================================================
        // RIGHT ACTION COLUMN — SpinWinBanner (cobalt) over SPIN — FREE (blue) over SPIN ×10 (gold + cost).
        // Width ≈0.22·W=515; right inset ≈0.03·W → center x≈0.86; vertical center anchorY 0.50 (Section E).
        // =================================================================================================
        private void BuildRightColumn()
        {
            const float colW = 0.22f * W;       // ≈515
            const float cx = 0.86f;             // column center (right inset 0.03 + half width 0.11)

            // SpinWinBanner — cobalt furled banner, height ≈0.20·H=216. Top of stack → anchorY ≈ 0.70.
            var banner = UiWidgets.Rect("SpinWinBanner", SafeContent, new Vector2(cx, 0.70f), new Vector2(cx, 0.70f), Vector2.zero, new Vector2(colW, 0.20f * H));
            var bImg = banner.gameObject.AddComponent<Image>(); bImg.raycastTarget = false; bImg.sprite = UiTex.VGradient(Hex("#244a9c"), Hex("#16306e"), 64); // cobalt cloth (Section G)
            var bRim = UiWidgets.Rect("BannerRim", banner, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bri = bRim.gameObject.AddComponent<Image>(); bri.raycastTarget = false; bri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); bri.type = Image.Type.Sliced; // stitched gold trim
            // 3 lines, gold-on-cobalt bevel (Section F): "SPIN & WIN" / "GREAT REWARDS" / "EVERY TIME".
            UiWidgets.TitleLabel(banner, "SPIN & WIN", 30, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(colW - 30, 44), TextAnchor.MiddleCenter, Hex("#f4e6b0"), Hex("#e3c878"));
            UiWidgets.TitleLabel(banner, "GREAT REWARDS", 26, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), Vector2.zero, new Vector2(colW - 30, 40), TextAnchor.MiddleCenter, Hex("#f4e6b0"), Hex("#e3c878"));
            UiWidgets.TitleLabel(banner, "EVERY TIME", 24, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(colW - 30, 38), TextAnchor.MiddleCenter, Hex("#f4e6b0"), Hex("#e3c878"));

            // SpinFreeButton (blue) — height ≈0.12·H=130; gap 0.025·H below banner → anchorY ≈ 0.50.
            BuildSpinFree(new Vector2(cx, 0.50f), new Vector2(colW, 0.12f * H));

            // SpinX10Button (gold) — height ≈0.13·H=140 (taller, has cost row); gap below → anchorY ≈ 0.345.
            BuildSpinX10(new Vector2(cx, 0.345f), new Vector2(colW, 0.13f * H));
        }

        // SPIN — FREE (blue gloss; breathing pulse when available). Sub: "1 free spin per day".
        private void BuildSpinFree(Vector2 anchor, Vector2 size)
        {
            var glow = UiWidgets.Glow(SafeContent, UiTheme.A(Hex("#4f8bff"), 0.5f), anchor, anchor, new Vector2(0, 14), size * 1.25f);
            var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.3f; pg.max = 0.6f; pg.period = 1.6f; // free-button breathe (Section I/H)

            var rt = UiWidgets.Rect("SpinFreeButton", SafeContent, anchor, anchor, Vector2.zero, size);
            var fill = rt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(Hex("#4f8bff"), Hex("#1f3fb0"), 32); // royal/cobalt gloss (Section G)
            _freeBtn = rt.gameObject.AddComponent<Button>(); _freeBtn.targetGraphic = fill;
            var cb = _freeBtn.colors; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; _freeBtn.colors = cb;
            _freeBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnSpinFree(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); ri.type = Image.Type.Sliced; // beveled gold trim
            // "SPIN — FREE" heavy serif white-on-blue (Section F).
            var lbl = UiWidgets.Label(rt, UiTheme.Track("SPIN — FREE"), 38, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(size.x - 20, 50), TextAnchor.MiddleCenter, Color.white);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#0a1840"), 0.9f);
            _freeSubText = UiWidgets.Label(rt, "1 free spin per day", 18, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), Vector2.zero, new Vector2(size.x - 20, 30), TextAnchor.MiddleCenter, Hex("#b9c6e6"));
            // CanvasGroup drives the disabled (countdown-running) look (Section H).
            _freeCg = rt.gameObject.AddComponent<CanvasGroup>();
        }

        // SPIN ×10 (gold gloss; gem cost "450"). Sub: "Better rewards guaranteed!" (⚠️ highest-risk copy — §A/§L).
        private void BuildSpinX10(Vector2 anchor, Vector2 size)
        {
            var glow = UiWidgets.Glow(SafeContent, UiTheme.A(UiTheme.GoldHi, 0.45f), anchor, anchor, new Vector2(0, 14), size * 1.25f);
            var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.28f; pg.max = 0.55f; pg.period = 2.0f; // gold pulse (Section I)

            var rt = UiWidgets.Rect("SpinX10Button", SafeContent, anchor, anchor, Vector2.zero, size);
            var fill = rt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(Hex("#f0d27a"), Hex("#9a7320"), 32); // brushed gold CTA (Section G)
            _x10Btn = rt.gameObject.AddComponent<Button>(); _x10Btn.targetGraphic = fill;
            var cb = _x10Btn.colors; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.82f, 0.7f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; _x10Btn.colors = cb;
            _x10Btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnSpinX10(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(Hex("#fff2c2"), UiTheme.Gold, UiTheme.GoldShadow, 48, 7); ri.type = Image.Type.Sliced;
            // "SPIN ×10" heavy serif dark-engrave on gold (Section F).
            var lbl = UiWidgets.Label(rt, UiTheme.Track("SPIN ×10"), 36, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), new Vector2(20, 0), new Vector2(size.x - 40, 50), TextAnchor.MiddleCenter, Hex("#2a1c06"));
            // Cost row: crystalline-violet gem icon + "450" (white; turns red #d8452b when unaffordable, §H).
            var gem = UiWidgets.Rect("CostGem", rt, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(-44, 0), new Vector2(36, 36));
            var gi = gem.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48);
            _x10CostText = UiWidgets.Label(rt, X10Cost.ToString(), 30, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(8, 0), new Vector2(160, 40), TextAnchor.MiddleLeft, Color.white);
            // Sub: "Better rewards guaranteed!" clean italic (Section F) — flagged copy, reproduced verbatim per spec.
            var sub = UiWidgets.Label(rt, "Better rewards guaranteed!", 18, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(size.x - 20, 30), TextAnchor.MiddleCenter, Hex("#5a431c"));
            sub.fontStyle = FontStyle.Italic;
            _x10Cg = rt.gameObject.AddComponent<CanvasGroup>();
        }

        // =================================================================================================
        // RECENT WINS TICKER (bottom strip) — header with flank rules + 6 win items via HorizontalLayoutGroup.
        // Spans inner width ≈0.94·W=2199, height ≈0.16·H=173, bottom inset ≈0.03·H → anchorY ≈0.105 (Section E).
        // =================================================================================================
        private void BuildRecentWins()
        {
            float stripW = 0.94f * W, stripH = 0.16f * H;
            var strip = UiWidgets.Card(SafeContent, new Vector2(0.5f, 0.105f), new Vector2(0.5f, 0.105f), Vector2.zero, new Vector2(stripW, stripH), UiTheme.A(Hex("#0c0e15"), 0.94f));

            // TickerHeader "RECENT WINS" centered with flank rule lines (Section C/F).
            UiWidgets.Label(strip, UiTheme.Track("RECENT WINS"), 22, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(400, 32), TextAnchor.MiddleCenter, Hex("#cdbf99"));
            UiWidgets.Divider(strip, new Vector2(0.30f, 0.86f), new Vector2(0.30f, 0.86f), Vector2.zero, 220, 3, UiTheme.A(UiTheme.Gold, 0.7f));
            var rR = UiWidgets.Divider(strip, new Vector2(0.70f, 0.86f), new Vector2(0.70f, 0.86f), Vector2.zero, 220, 3, UiTheme.A(UiTheme.Gold, 0.7f));
            rR.transform.localRotation = Quaternion.Euler(0, 0, 180f); // mirror so the fade points outward

            // WinRow — HorizontalLayoutGroup, 6 equal items (Section D/E). gap ≈0.01·W.
            var row = UiWidgets.Rect("WinRow", strip, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(stripW - 80, stripH * 0.6f));
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0.01f * W; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            for (int i = 0; i < RecentWins.Length; i++) BuildWinItem(row, RecentWins[i]);
        }

        // WinItem — avatar disc + name + prize (tinted) + "Xm ago". Equal cell sized by the HLG (Section D/F).
        private void BuildWinItem(RectTransform row, Win w)
        {
            var cell = UiWidgets.Rect("WinItem_" + w.Name, row, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var le = cell.gameObject.AddComponent<LayoutElement>(); le.flexibleWidth = 1f; le.flexibleHeight = 1f;
            // Avatar framed bust (⌀≈0.045·H).
            var av = UiWidgets.Rect("Avatar", cell, new Vector2(0.13f, 0.5f), new Vector2(0.13f, 0.5f), Vector2.zero, new Vector2(0.045f * H, 0.045f * H));
            var avi = av.gameObject.AddComponent<Image>(); avi.raycastTarget = false; avi.sprite = UiTex.Disc(Hex("#3a4150"), 48);
            var avRim = UiWidgets.Rect("AvRim", av, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var avr = avRim.gameObject.AddComponent<Image>(); avr.raycastTarget = false; avr.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.6f), UiTheme.A(UiTheme.Gold, 0.5f), UiTheme.A(UiTheme.GoldShadow, 0.5f), 48, 5); avr.type = Image.Type.Sliced;
            var bust = UiWidgets.Rect("Bust", av, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(0.03f * H, 0.03f * H));
            var bu = bust.gameObject.AddComponent<Image>(); bu.raycastTarget = false; bu.sprite = UiTex.Diamond(Hex("#9aa0ad"), 32);
            // Name / prize / time stacked (right of avatar).
            UiWidgets.Label(cell, w.Name, 18, new Vector2(0.26f, 0.74f), new Vector2(0.26f, 0.74f), new Vector2(0, 0), new Vector2(220, 26), TextAnchor.MiddleLeft, Hex("#e8e2cf"));
            UiWidgets.Label(cell, w.Reward, 16, new Vector2(0.26f, 0.48f), new Vector2(0.26f, 0.48f), new Vector2(0, 0), new Vector2(240, 24), TextAnchor.MiddleLeft, w.Tint);
            UiWidgets.Label(cell, w.Ago, 14, new Vector2(0.26f, 0.24f), new Vector2(0.26f, 0.24f), new Vector2(0, 0), new Vector2(200, 22), TextAnchor.MiddleLeft, Hex("#8a8472"));
        }

        // =================================================================================================
        // LIFECYCLE — entry reveal (Section I) + idle loops + display-only countdown.
        // =================================================================================================
        public override void OnShow()
        {
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
            RefreshFreeState();
            RefreshX10Affordability();
            StartCoroutine(EntryReveal());
            StartCoroutine(CountdownTick());
        }

        // OnShow (~0.9s, Section I): wheel scale-in (ease-out-back); a soft glint sweep is implicit via the idle
        // rim-stud twinkle + hub shimmer already running. Subsequent columns are already placed (built in Build()).
        private IEnumerator EntryReveal()
        {
            if (_discRt == null) yield break;
            var grp = (RectTransform)_discRt.parent; // WheelGroup
            if (grp == null) yield break;
            float t = 0f; const float d = 0.32f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                float s = Mathf.Lerp(0.90f, 1f, EaseOutBack(k));
                grp.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            grp.localScale = Vector3.one;
        }

        // =================================================================================================
        // SPIN — display-only (per task). Eases to a random PRESENTATION segment, then toasts the prize. The
        // landing is NOT a server roll and writes NO real balance; the optional UiStub.GrantGems mutates a FAKE
        // wallet only. In production the segment + reward are server-authoritative (client never picks, §K/§L).
        // =================================================================================================
        private void OnSpinFree()
        {
            if (_spinning) return;
            if (!_freeAvailable) { StartCoroutine(ShakeFree()); Router.Toast("Next free spin in " + FormatHms(_countdownSeconds)); return; }
            _freeAvailable = false;
            RefreshFreeState();
            StartCoroutine(SpinSequence(false));
        }

        private void OnSpinX10()
        {
            if (_spinning) return;
            // Unaffordable → red cost + insufficient toast (shared insufficient sheet 37 / Store deep-link in prod, §H).
            if (UiStub.Gems < X10Cost) { if (_x10CostText != null) _x10CostText.color = Hex("#d8452b"); StartCoroutine(ShakeX10()); Router.Toast("Not enough Gems — visit the Store (server-authoritative)"); return; }
            // Affordable → in production this opens the ×10 CONFIRM sheet (37) BEFORE any charge (§K/§L). Here it is
            // a display-only affordance: surface the confirm intent via a toast; do NOT charge gems pre-confirm (§L).
            Router.Toast("Spend " + X10Cost + " Gems for 10 spins? (confirm sheet — server-authoritative)");
            StartCoroutine(SpinSequence(true));
        }

        // wind-up → accelerate → cruise → ease-out decelerate → settle-into-segment → win burst → reveal (Section I).
        private IEnumerator SpinSequence(bool isX10)
        {
            if (_discRt == null) yield break;
            _spinning = true;
            SetButtonsLocked(true); // both spin buttons lock during a spin (Negative-Rule L)

            float start = _discRt.localEulerAngles.z;

            // Presentation-only target segment (random; NOT a server roll, §L). Land the chosen segment under the
            // top pointer: a segment i is centered under 12 o'clock when the disc z = +(i × 45°) (CCW in Unity).
            int target = Random.Range(0, Segs);
            float baseTarget = target * SegDeg;
            // Total travel = several full turns + the delta to the target resting angle (decelerating).
            int turns = isX10 ? 7 : 5; // ×10 spins a little longer/flashier
            float normStart = Mathf.Repeat(start, 360f);
            float deltaToTarget = Mathf.Repeat(baseTarget - normStart, 360f);
            float total = turns * 360f + deltaToTarget;

            // (1) wind-up: rotate backward ~8° (0.15s, ease-in).
            yield return EaseRotate(start, start - 8f, 0.15f, EaseInQuad);
            float windEnd = start - 8f;

            // (2)+(3)+(4) accelerate → cruise → decelerate into the target, as one ease-out-quint over the whole
            // forward travel (clean settle without per-phase seams; pointer 'ticks' approximated by the studs).
            float spinDur = isX10 ? 4.6f : 4.0f;
            yield return EaseRotate(windEnd, windEnd + total + 8f, spinDur, EaseOutQuint);

            // (5) tiny overshoot + settle (±2°, 0.25s) — "click into place".
            float settled = windEnd + total + 8f;
            yield return EaseRotate(settled, settled + 3f, 0.12f, EaseOutQuad);
            yield return EaseRotate(settled + 3f, settled, 0.13f, EaseOutQuad);
            _discRt.localEulerAngles = new Vector3(0, 0, Mathf.Repeat(settled, 360f));

            // Pointer recoil bounce on settle (Section I/H).
            StartCoroutine(PointerRecoil());
            // Win burst from the winning segment + hub roar-glint (Section J) — gold sparks radiating at the rim.
            SpawnWinBurst(target);

            yield return WaitUnscaled(0.3f);

            // (6) reward reveal — display-only. Toast the prize; gem prizes count up the FAKE wallet (UiStub), never
            // a server write (§12). ×10 would hand to a 10-item summary / multi-grant (38) in production.
            var p = Prizes[target];
            string prizeName = p.Label.Replace("\n", " ");
            if (isX10)
            {
                Router.Toast("×10 result — 10 rewards granted (summary via Reward Grant 38, server-authoritative)");
            }
            else
            {
                if (p.GrantsGems) { UiStub.GrantGems(p.Gems); if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0"); }
                Router.Toast("You won: " + prizeName + "!");
            }
            RefreshX10Affordability();

            _spinning = false;
            SetButtonsLocked(false);
            RefreshFreeState(); // free → disabled+countdown after use; ×10 → enabled if gems remain
        }

        // Rotate the disc's z from `a` to `b` over `dur` (unscaled) using easing `e`.
        private IEnumerator EaseRotate(float a, float b, float dur, System.Func<float, float> e)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dur);
                float z = Mathf.Lerp(a, b, e(k));
                _discRt.localEulerAngles = new Vector3(0, 0, z);
                yield return null;
            }
            _discRt.localEulerAngles = new Vector3(0, 0, b);
        }

        private IEnumerator PointerRecoil()
        {
            if (_pointerRt == null) yield break;
            float t = 0f; const float d = 0.22f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                float kick = Mathf.Sin(k * Mathf.PI) * 12f; // kick out then return
                _pointerRt.localEulerAngles = new Vector3(0, 0, 45f + kick);
                yield return null;
            }
            _pointerRt.localEulerAngles = new Vector3(0, 0, 45f);
        }

        // Gold/colored spark burst + brief segment flash radiating from the winning segment at the rim (Section J).
        private void SpawnWinBurst(int target)
        {
            var grp = _discRt != null ? (RectTransform)_discRt.parent : null;
            if (grp == null) return;
            Vector2 at = Polar(target * SegDeg, R * 0.8f);
            // Flash: white→tier color radial that pops then fades.
            var flash = UiWidgets.Glow(grp, UiTheme.A(Color.white, 0.0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), at, new Vector2(280, 280), 1.5f);
            StartCoroutine(FlashOut(flash, Prizes[target].Top));
            // Spark fountain (colored confetti tinted to the reward).
            var sparkHost = UiWidgets.Rect("WinBurst", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), at, new Vector2(260, 260));
            var ef = sparkHost.gameObject.AddComponent<EmberField>(); ef.count = 18; ef.color = UiWidgets.Lighten(Prizes[target].Top, 0.3f);
            Destroy(sparkHost.gameObject, 2.0f);
            // Hub roar-glint pop.
            if (_hubGlow != null) StartCoroutine(HubGlint());
        }

        private IEnumerator FlashOut(Image flash, Color tier)
        {
            if (flash == null) yield break;
            float t = 0f; const float d = 0.4f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                Color c = Color.Lerp(Color.white, tier, k); c.a = Mathf.Lerp(0.9f, 0f, k);
                flash.color = c;
                yield return null;
            }
            if (flash != null) Destroy(flash.gameObject);
        }

        private IEnumerator HubGlint()
        {
            var pulse = _hubGlow.GetComponent<PulseGraphic>(); if (pulse != null) pulse.enabled = false;
            float t = 0f; const float d = 0.5f; var c = _hubGlow.color;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                c.a = Mathf.Lerp(1f, 0.6f, k); _hubGlow.color = c;
                yield return null;
            }
            if (pulse != null) pulse.enabled = true;
        }

        private IEnumerator ShakeFree() { yield return Shake(_freeBtn != null ? (RectTransform)_freeBtn.transform : null); }
        private IEnumerator ShakeX10()  { yield return Shake(_x10Btn  != null ? (RectTransform)_x10Btn.transform  : null); }
        private IEnumerator Shake(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 home = rt.anchoredPosition;
            float t = 0f; const float d = 0.3f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = t / d;
                rt.anchoredPosition = home + new Vector2(Mathf.Sin(k * 40f) * (1f - k) * 14f, 0f);
                yield return null;
            }
            rt.anchoredPosition = home;
        }

        // =================================================================================================
        // STATES (Section H) — free availability, ×10 affordability, lock both during spin.
        // =================================================================================================
        private void RefreshFreeState()
        {
            // Disabled (free spin used → countdown running) reads as a desaturated/dimmed button (alpha), but raycast
            // stays on so a tap = shake + countdown toast (Section H). The _freeAvailable guard in OnSpinFree blocks
            // an actual spin; we keep the Button interactable (unless spinning) so the tap-to-toast path fires.
            if (_freeCg != null) _freeCg.alpha = _freeAvailable ? 1f : 0.55f;
            if (_freeBtn != null && !_spinning) _freeBtn.interactable = true;
            if (_freeSubText != null) _freeSubText.text = _freeAvailable ? "1 free spin per day" : "Next free spin in " + FormatHms(_countdownSeconds);
        }

        private void RefreshX10Affordability()
        {
            bool afford = UiStub.Gems >= X10Cost;
            if (_x10CostText != null) _x10CostText.color = afford ? Color.white : Hex("#d8452b");
            if (_x10Cg != null) _x10Cg.alpha = afford ? 1f : 0.7f;
        }

        private void SetButtonsLocked(bool locked)
        {
            if (_freeBtn != null) _freeBtn.interactable = !locked; // both spin buttons lock during a spin (Negative-Rule L)
            if (_x10Btn != null) _x10Btn.interactable = !locked;
            if (locked)
            {
                if (_freeCg != null) _freeCg.alpha = 0.6f;
                if (_x10Cg != null) _x10Cg.alpha = 0.6f;
            }
            else
            {
                RefreshFreeState();
                RefreshX10Affordability();
            }
        }

        // Display-only countdown tick (synced to a stub end-time; server-authoritative in production, §K).
        // At 00:00:00 → free spin re-enabled + sub flips (no reload, §H/§K).
        private IEnumerator CountdownTick()
        {
            while (true)
            {
                float w = 0f;
                while (w < 1f) { w += Time.unscaledDeltaTime; yield return null; }
                if (!_freeAvailable && _countdownSeconds > 0)
                {
                    _countdownSeconds--;
                    if (_countdownText != null) _countdownText.text = FormatHms(_countdownSeconds);
                    if (_freeSubText != null) _freeSubText.text = "Next free spin in " + FormatHms(_countdownSeconds);
                    if (_countdownSeconds <= 0) { _freeAvailable = true; _countdownSeconds = 3 * 3600 + 11 * 60; if (_countdownText != null) _countdownText.text = FormatHms(_countdownSeconds); RefreshFreeState(); }
                }
            }
        }

        // =================================================================================================
        // Helpers
        // =================================================================================================
        // Polar offset (degrees measured CLOCKWISE from the top / 12 o'clock) → anchoredPosition.
        // top = (0, +r); clockwise increases x first: x = sin(deg)·r, y = cos(deg)·r.
        private static Vector2 Polar(float deg, float radius)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(a) * radius, Mathf.Cos(a) * radius);
        }

        // Keep a segment label readable: tilt toward its mid-angle but never beyond ±90° (no upside-down text).
        private static float NormalizeTilt(float midDeg)
        {
            float z = -midDeg; // match the disc's CCW convention
            while (z > 90f) z -= 180f;
            while (z < -90f) z += 180f;
            return z;
        }

        private static string FormatHms(int sec)
        {
            if (sec < 0) sec = 0;
            int h = sec / 3600, m = (sec % 3600) / 60, s = sec % 60;
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; float p = x - 1f; return 1f + c3 * p * p * p + c1 * p * p; }
        private static float EaseOutQuint(float x) { float p = 1f - x; return 1f - p * p * p * p * p; }
        private static float EaseOutQuad(float x) { return 1f - (1f - x) * (1f - x); }
        private static float EaseInQuad(float x) { return x * x; }

        private IEnumerator WaitUnscaled(float s) { float t = 0f; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
