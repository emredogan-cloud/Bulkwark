# BULWARK — UI CONSTRUCTION SPEC · 10 · In-Match Banner (Objective / Wave / Event)

Source: design/InMatchBannerDesign.png · 1824×862 (2.12:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 boundary from `00_CONTEXT_RECOVERY.md`. This is a **transient announcement
> overlay** that drops over the live Battle HUD (08) + Spell HUD (09) to call out a **wave / objective / event**
> ("WAVE 12: THE DEAD AWAKEN") with a **countdown timer** ("02:14"). The full in-match HUD stays visible and
> functional beneath it; the banner is a top-center heraldic title strip with flanking cloth banners. It reads
> the event/timer from the sim **read-only** and writes nothing.

---

## A · SCREEN PURPOSE
A **dramatic event banner** that announces the current wave/objective during a fight without leaving the HUD. It
shows (1) a large **ornate title strip** across the top-center reading the event name — here **"WAVE 12: THE DEAD
AWAKEN"** — topped by a small horned-skull finial and flanked by **hanging red cloth banners**; (2) a
**countdown timer** "**02:14**" with a small hourglass/clock glyph directly beneath the title (time until the
wave hits / objective deadline); all while (3) the **entire live HUD remains visible and usable** beneath it —
gold "**450**", supply "**3/8**", left Iron Pact HP "**10,000 / 10,000**", right Ashen Horde HP
"**8,300 / 10,000**", army "**42/50**", the 5 unit-train tiles (costs **60 / 90 / 75 / 120 / 150**), the
3-slot spell row (**12s / 8s / 15s**) + commander orb, and GARRISON / DEFEND / ATTACK. The battlefield also
shows blue (player) and red (enemy) **movement-path arrows**. Purpose: communicate the stakes of the moment with
high drama, then auto-dismiss so play continues.

## B · VISUAL DNA (screen-specific)
- **Top-center heraldic title strip:** a long horizontal **ornate gold/bronze plaque** with scrolled, pointed
  end-caps and engraved filigree, carrying the event title in big serif gold-bevel UPPERCASE. A small **horned
  skull** crest sits at the top-center apex (this wave is undead-themed → ember/oxblood accent on the frame).
- **Flanking cloth banners:** two **hanging red (oxblood) cloth pennants** with gold trim and a faint sigil,
  draping down from the strip's outer ends — they frame the title and reinforce the Ashen/undead threat.
- **Countdown plate:** a smaller dark gold-rim lozenge directly under the title with a clock/hourglass glyph + a
  **MM:SS timer** in glowing numerals.
- **Banner is an overlay, not full chrome:** behind it the **complete Battle/Spell HUD** is rendered (top HP
  bars + crests, resource chips, unit tray, spell row + commander orb, order cluster) — all dimmed slightly by a
  soft top scrim so the title pops, but still readable and interactive.
- **Battlefield movement arrows:** stylized **blue** (player advance) and **red** (enemy advance) curved arrow
  paths overlaid on the field, indicating troop movement vectors — part of this in-match moment's storytelling.
- Mood: ominous, ember-lit, "the horde is coming" — danger red dominates the banner while the HUD keeps its
  gold/blue/red faction language.

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
InMatchBannerOverlay (UiScreen OR additive overlay layer above BattleHud/SpellHud)
└─ Root (stretch-all)
   ├─ Scrim_BannerTop (Image, top gradient, soft, raycast OFF)        // gently dims HUD top for title pop
   ├─ **BannerGroup (Container, top-center, transient)**
   │  ├─ **Banner_Cloth_Left (Image)**                                // hanging red pennant (left)
   │  ├─ **Banner_Cloth_Right (Image)**                               // hanging red pennant (right)
   │  ├─ **Banner_Frame_Gold (Image, sliced)**                        // ornate plaque + scroll caps
   │  ├─ **Banner_SkullFinial (Image)**                               // horned-skull crest at apex
   │  ├─ **Banner_Title (Text "WAVE 12: THE DEAD AWAKEN")**
   │  └─ **Timer_Plate (Container)**
   │     ├─ **Timer_Frame (Image)** · **Timer_ClockIcon (Image)**
   │     └─ **Timer_Text (Text "02:14")**
   ├─ **PathArrows_Layer (Container, battlefield space, raycast OFF)** // movement vectors
   │  ├─ **Arrow_Player_Blue (Image, ×N)**
   │  └─ **Arrow_Enemy_Red (Image, ×N)**
   └─ [BENEATH] BattleHud (08) + SpellHud (09) chrome — rendered & interactive:
      TopBar{Pause, HP_L"10,000/10,000"+crest, HP_R"8,300/10,000"+crest, Gold"450", Supply"3/8", Army"42/50"},
      UnitTray{costs 60,90,75,120,150}, SpellRow{12s,8s,15s}, CommanderOrb, OrderCluster{GARRISON,DEFEND,ATTACK}
```

## D · UNITY HIERARCHY SPEC (per node — banner-specific; HUD beneath follows 08/09)
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Overlay | 0 | RectTransform | stretch-all | 0.5,0.5 | sits above HUD canvas (higher sort order) | n/a | fills |
| Scrim_BannerTop | Root | 0 | Image | top stretch (0,0.7)-(1,1) | 0.5,1 | soft dark→clear, raycast OFF (HUD stays tappable) | no | full width |
| BannerGroup | Root | 1 | Container | top-center (0.5,1) | 0.5,1 | hangs from safe top | **yes** | center, fixed height |
| Banner_Cloth_Left | BannerGroup | 0 | Image | left end, hanging down | 0.5,1 | red pennant | yes | — |
| Banner_Cloth_Right | BannerGroup | 1 | Image | right end, hanging down | 0.5,1 | red pennant (mirror) | yes | — |
| Banner_Frame_Gold | BannerGroup | 2 | Image (sliced) | stretch-all (of group strip) | 0.5,0.5 | 9-slice w/ scroll caps | yes | width grows with title |
| Banner_SkullFinial | BannerGroup | 3 | Image | top-center apex | 0.5,0 | above the strip | yes | center-locked |
| Banner_Title | BannerGroup | 4 | Text | center of strip | 0.5,0.5 | event name | yes | auto-size to fit strip |
| Timer_Plate | BannerGroup | 5 | Container | below strip, center | 0.5,1 | hangs under title | yes | center |
| Timer_Frame | Timer_Plate | 0 | Image | stretch-all | 0.5,0.5 | dark gold-rim lozenge | yes | — |
| Timer_ClockIcon | Timer_Plate | 1 | Image | left-inside | 0.5,0.5 | hourglass/clock | yes | — |
| Timer_Text | Timer_Plate | 2 | Text | center/right | 0.5,0.5 | "02:14" | yes | — |
| PathArrows_Layer | Root | 2 | Container | center (battlefield) | 0.5,0.5 | over field, raycast OFF | **no** | world-aligned |
| Arrow_Player_Blue | PathArrows_Layer | 0..n | Image | per-vector | varies | blue curved arrow | no | follows sim vectors |
| Arrow_Enemy_Red | PathArrows_Layer | 0..n | Image | per-vector | varies | red curved arrow | no | follows sim vectors |
| (HUD beneath) | separate canvas/lower sort | — | — | — | — | unchanged from 08/09 | yes | unchanged |

**Sort/order rationale:** the banner overlay renders **above** the HUD (higher canvas sortingOrder or a child
appended last) so the title sits over the top of the screen, but its scrim and arrow layers are **raycast OFF**
so the HUD beneath (pause, train, spells, orders, battlefield) stays fully interactive while the banner is up.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Banner strip:** width ≈ **0.46W** (≈1076 px) × height ≈ **0.11H** (≈119 px); centered x=0.50, the strip's
  vertical center at ≈y=0.90 (top edge ≈0.045H below safe top). Title cap-height ≈ **0.060H** (≈65 px),
  horizontally fit/auto-sized within the strip's inner width (≈0.40W usable).
- **Skull finial:** Ø ≈ **0.055H**, centered x=0.50, its base touching the strip's top edge (apex at ≈y=0.965).
- **Cloth pennants:** each ≈ **0.06W** wide × **0.16H** tall, hanging from the strip's outer ends (left pennant
  centered x≈0.27, right x≈0.73), their tops behind the strip ends, draping down to ≈y=0.74. Slight outward
  splay.
- **Timer plate:** ≈ **0.115W × 0.052H**, centered x=0.50, its top touching the strip's bottom (vertical center
  ≈y=0.825). Clock icon Ø≈0.032H at the plate's left; "02:14" cap-height ≈ **0.030H**, right of the icon.
- **Clear battlefield zone:** the banner + timer occupy only the top strip (y≳0.78); the central battlefield
  x∈[0.20,0.80], y∈[0.20,0.78] stays clear of blocking chrome (the banner overlay is non-blocking anyway).
- **Path arrows:** stylized curved arrows ≈0.10–0.18W long, drawn between unit groups on the field (blue from the
  player's left mass toward the center/right; red from the enemy's right mass toward the center/left). They are
  decorative-over-sim indicators, positioned by the sim's movement vectors, clamped to the field area.
- **HUD beneath:** all fractions from **08 §E / 09 §E** (this mockup shows the full spell-HUD variant). Values:
  gold **450**, supply **3/8**, HP_L **10,000/10,000**, HP_R **8,300/10,000**, army **42/50**, unit costs
  **60/90/75/120/150**, spell cooldowns **12s/8s/15s**, GARRISON/DEFEND/ATTACK present (ATTACK at far corner).
- **Tablet (1.33:1):** strip width grows to ≈0.55W so the long title still fits at the same cap-height; pennants
  pull in slightly. **Ultrawide (21:9):** strip stays ≈0.46W centered (extra width is margin); HUD corner groups
  unchanged. **Notch:** the whole BannerGroup is inside SafeArea so a top/side cutout never clips the title,
  skull, or timer; the soft scrim passes under.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| Banner_Title | WAVE 12: THE DEAD AWAKEN | epic Trajan serif, gold bevel | **Black** | UPPER | +5% | gold bevel + 1.5px #1f1405 stroke + drop-shadow (0,3,#000 70%) + faint ember outer glow | ~65 | gradient **#f8e9b4→#caa04a**, slight ember warmth at the lower edge |
| Timer_Text | 02:14 | tabular numerals, urgent | Bold | — | +2% | 1px #2a1305 stroke + soft amber/ember inner glow + drop-shadow | ~32 | #ffd98a (urgent gold); shifts toward **#ff8a5a** when <0:30 |

The title is the **single largest, brightest** element while the banner is up (it temporarily out-ranks the
HUD's ATTACK). The timer reads as urgent gold and warms toward ember as it runs low. HUD text typography = 08/09
§F unchanged.

## G · MATERIALS
- **Banner plaque (gold/bronze, ember-accented):** base #8a6a2c, highlight #f3d680, shadow #523c14; satin metal
  (roughness ≈0.35) with deeply **engraved filigree** and scrolled/pointed end-caps; because the event is
  undead-themed the frame carries faint **ember/oxblood** inlays (#7a1f1a) along its channels and a warm rim-
  bloom; light edge wear.
- **Skull finial:** aged bone/iron horned skull, blackened metal mounts, faint red eye-glow; oxblood accent.
- **Cloth pennants:** **oxblood red** woven cloth (#5a1714→#8a241c) with **gold trim** and stitched edges, a
  faint darker sigil woven in; soft cloth roughness, gentle drape folds, subtle bottom fray; low specular.
- **Timer plate:** dark glass core (#0c0e14 @ ~85%) with a gold/bronze rim; the clock/hourglass icon is brushed
  gold; numerals carry an inner amber glow.
- **Path arrows:** translucent additive ribbons — **blue** (#3f8bff core, white edge) for the player, **red**
  (#d8452b core) for the enemy; soft outer glow, slight animated flow along their length.
- **Scrim:** soft vertical alpha gradient #05060a (≈0→0.45) at the top only — just enough to lift the title.
- HUD beneath uses 08/09 §G materials unchanged.

## H · COMPONENTS (interactive)
The banner overlay is **almost entirely non-interactive display**; the interactivity lives in the HUD beneath
(which keeps **08 §H / 09 §H** behavior verbatim — pause, train tiles, spells, commander orb, order cluster, HP
read-outs). Banner-specific elements:
1. **Banner_Title / Banner_Frame / Skull / Cloth** — *purpose:* announce the event. *States:* value states only
   — **enter** (dramatic build-in), **hold** (steady, subtle cloth sway + frame rim breathe), **exit** (slide
   up / fade). No idle/hover/pressed (not a button). *Feedback:* none (display).
2. **Timer_Text** — *purpose:* live countdown to the wave/deadline (read-only from sim). *States:* **normal**
   (gold), **urgent** (<0:30 → warms to ember + a soft per-second pulse), **expired** (00:00 → quick flash, then
   the banner exits as the wave triggers). *Structure:* clock icon + MM:SS. *Feedback:* visual urgency only.
3. **PathArrows (blue/red)** — *purpose:* visualize movement vectors (read-only). *States:* animated flow; appear/
   fade with the relevant orders/waves. *Feedback:* display; never interactive.

*(If product wants a "tap to dismiss" the banner could accept a tap, but the mockup shows no dismiss affordance;
default is **auto-dismiss by timer/animation**, not a button — see L.)*

## I · ANIMATION TIMELINE (banner enter → hold → exit; t in s)
| t (s) | Element | Action | Dur | Easing | Emphasis |
|---|---|---|---|---|---|
| 0.00 | Scrim_BannerTop | α 0→0.45 | 0.20 | linear | top dims slightly |
| 0.02 | Banner_Frame_Gold | drop from y+0.05 + scale-X 0.7→1 + α 0→1 | 0.30 | back-out | the plaque slams in |
| 0.10 | Banner_Cloth_L/R | unfurl: scale-Y 0.4→1 from top + α 0→1 | 0.30 | ease-out | pennants drop/unroll |
| 0.14 | Banner_SkullFinial | pop scale 0.5→1 + α | 0.22 | back-out | crest seats at apex |
| 0.20 | Banner_Title | type/reveal: α 0→1 + scale 1.08→1 + ember glow flash | 0.28 | ease-out | **peak** — event name |
| 0.36 | Timer_Plate | drop from y+0.03 + α 0→1 | 0.22 | ease-out | countdown appears |
| hold | Cloth, Frame | cloth sway ±2° (~2.2s), frame rim-bloom breathe (~2.0s) | cont. | sine | living banner |
| hold | Timer_Text | tick down; pulse each second when urgent (<0:30) | cont. | — | urgency |
| exit (≈2.5–4s, or on expiry) | BannerGroup | slide up to y+0.06 + α 1→0 (cloth furls back) | 0.30 | ease-in | clears for play |
| exit | Scrim_BannerTop | α →0 | 0.25 | linear | HUD un-dims |

**Path arrows:** flowing dash animation along each arrow (~1.0s loop) while shown; fade in/out with their order.

## J · PARTICLE & FX (passive — describe only)
- **Ember motes** rising around the skull finial / banner top (undead theme); faint warm bloom on the frame.
- **Cloth sway** + soft cast shadow under the pennants.
- **Timer urgency:** a subtle ember pulse-ring around the timer plate when <0:30.
- **Path arrows:** traveling light-dash flow (blue / red) along each vector.
- HUD passives (08/09 §J) continue beneath. No FX intrudes on the readable battlefield center.

## K · EVENT BEHAVIOR
- **OnEvent (wave/objective fires):** the sim/meta raises an event with {title, optional duration}; the overlay
  is pushed **above** the live HUD; the enter timeline (I) plays. Player: a dramatic "WAVE 12: THE DEAD AWAKEN"
  announcement without losing control of the battle.
- **While shown:** Timer_Text counts down from the supplied duration (read-only); cloth sways; the **HUD beneath
  stays fully interactive** (the player can keep training, casting, ordering — the banner's scrim/arrow layers are
  raycast-off). Path arrows reflect current movement vectors.
- **OnExpire / OnTimeout:** at 00:00 (or after a min dwell with no timer) the exit timeline plays and the overlay
  is removed; the wave/objective proceeds in the sim.
- **OnHide:** overlay destroyed/pooled; HUD un-dims; no persistent state. The banner never pauses the game
  (unlike the Pause modal) — play continues throughout.

## L · NEGATIVE RULES (must-never)
- **Never** block the HUD or battlefield: the banner's scrim and arrow layers are **raycast OFF**; the player can
  always pause/train/cast/order while it's up.
- **Never** pause the sim for the banner (it is non-modal; only the Pause modal sets timeScale 0).
- **Never** clip the title/skull/timer under a notch — BannerGroup stays in SafeArea.
- **Never** mismatch arrow colors: blue = player advance, red = enemy advance.
- **Never** leave the banner up indefinitely — it must auto-exit on timer expiry or after its dwell.
- **Never** add a dismiss button the mockup doesn't show (auto-dismiss only) unless product later requires it.
- **Never** let the banner's drama reduce HUD legibility below readable (scrim ≤0.45 alpha, top only).
- **Never** read/write beyond the §12 boundary; the banner consumes a read-only event + timer, nothing more.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `InMatchBannerDesign.png`: top-center ornate banner reading **"WAVE 12: THE DEAD AWAKEN"**
  with a horned-skull finial and two hanging red pennants; a "**02:14**" countdown plate beneath; the full live
  HUD visible behind with this mockup's values (gold 450, supply 3/8, HP_L 10,000/10,000, HP_R 8,300/10,000,
  army 42/50, unit costs 60/90/75/120/150, spell row 12s/8s/15s + commander orb, GARRISON/DEFEND/ATTACK); plus
  blue/red battlefield path arrows — all per §E (±2%).
- **Non-modal overlay:** HUD beneath stays fully interactive; scrim/arrows raycast-off; sim NOT paused.
- **Hierarchy:** §C tree; banner renders above HUD; cloth→frame→skull→title→timer order.
- **Typography:** exact title string + "02:14"; title is largest/brightest while shown; timer warms when urgent.
- **Animation:** dramatic enter (frame slam → cloth unfurl → skull pop → title reveal → timer drop), hold sway,
  auto-exit on timer expiry.
- **Safe area + §12:** banner inside safeArea on a notched device; only a read-only event/timer consumed.

## N · IMPLEMENTATION CONFIDENCE
**87 / 100.** The overlay is structurally simple, fraction-specified, and the non-modal "announce over a live
HUD" pattern is well understood. The −13: (a) the ornate plaque art, cloth pennants, and skull finial are assets
to author; (b) the path-arrow rendering must be wired to the sim's movement vectors (the mockup only shows a
snapshot) and kept read-only; (c) banner dwell/exit timing and the event/timer source must be hooked to the
real wave system within §12.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O present, in order, substantive.
- □ Strings/numbers captured: "WAVE 12: THE DEAD AWAKEN", "02:14", gold 450, supply 3/8, HP 10,000/10,000 &
  8,300/10,000, army 42/50, costs 60/90/75/120/150, spell cooldowns 12s/8s/15s.
- □ Overlay is non-modal & non-blocking (raycast-off scrim/arrows; sim not paused).
- □ HUD beneath reuses 08/09; banner renders above; title is the temporary focal.
- □ Arrow colors locked (blue=player, red=enemy); banner inside SafeArea.
- □ Auto-exit on timer expiry; no invented dismiss button.
- □ §12 respected (read-only event/timer).
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Hex + materials for plaque/cloth/skull/timer/arrows.
- □ Header + Source line in required format.
