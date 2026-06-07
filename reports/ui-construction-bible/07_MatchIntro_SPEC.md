# BULWARK — UI CONSTRUCTION SPEC · 07 · Match Intro (Pre-Battle VS)

Source: design/MatchIntroDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits the GLOBAL VISUAL DNA and the §12 control boundary from `00_CONTEXT_RECOVERY.md`. This is the
> pre-battle framing screen shown after Mode/Online selection and before the Battle HUD loads. It is a
> full-bleed cinematic card; no live ECS battle runs underneath yet (the sim is loading behind it).

---

## A · SCREEN PURPOSE
A cinematic **"VS" face-off card** that sells the matchup before the fight. It (1) names the **mode/map**
("CLASSIC — STONEHOLD PASS") in a top-center gold banner, (2) presents the two combatants as hero portraits —
left **Iron Pact** blue knight, right **Ashen Horde** red warlord — split by a luminous **"VS"** seam of clashing
blue lightning and ember fire, (3) labels each faction with a heraldic crest + name + motto plate along the
bottom, and (4) shows a single coaching **Tip** line at center-bottom. It is a **transient hand-off screen**:
auto-advances to the Battle HUD when the sim is ready (or on tap). No interactive choices beyond an implicit
"tap/auto-continue"; **no buttons are drawn** in the mockup. Reads nothing from ECS; writes nothing (the match
is configured by the meta layer that launched it).

## B · VISUAL DNA (screen-specific on top of global)
- **Diptych split-screen** composition: the frame is bisected vertically by an energy seam. Left hemisphere =
  **cold** (cobalt/steel, blue rim light, a blue lightning bolt clawing up the seam). Right hemisphere = **hot**
  (oxblood/ember, orange fire licking up the seam, sparks). The seam is the brightest vertical axis on screen.
- **Two full-body hero renders** at roughly 2/3 screen height, facing the seam (left knight faces right, right
  warlord faces left), lit by low-key key-lights from the seam so their inner edges carry a gold/white rim.
- **Top-center ornate banner**: a horizontal cast-gold/bronze plaque with scrolled end-caps and a small central
  finial, holding the serif gold-bevel title "CLASSIC" and a thin tracked sub-label "STONEHOLD PASS".
- **Giant central "VS"** in heavy gold serif with a hot bloom — the focal emblem, sitting over the seam at
  vertical center.
- **Two bottom faction plates**: dark glass lozenges with a circular crest at the inner end, faction name in
  gold serif, and a tracked uppercase motto beneath.
- Strong cinematic **vignette**; warm god-rays bleed from the seam; heavy atmospheric haze/embers right, cold
  mist left. Backgrounds: left = ruined blue-lit fortress wall; right = burning red battlefield/cliff.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
MatchIntroScreen (UiScreen, CanvasGroup, full-bleed)
└─ Root (RectTransform, stretch-all)
   ├─ BG_FullBleed (Image, stretch-all)                     // composited cinematic backdrop (under cutout)
   │  ├─ BG_Left_ColdField (Image)                          // blue ruined fortress wall + cold mist
   │  ├─ BG_Right_HotField (Image)                          // burning red cliff/battlefield + embers
   │  ├─ Seam_EnergySplit (Image, vertical)                 // blue-lightning↔orange-fire clash band
   │  │  ├─ Seam_LightningBlue (Image)                      // left-leaning electric arc (cold)
   │  │  └─ Seam_FireOrange (Image)                         // right-leaning flame plume (hot)
   │  ├─ Vignette (Image, stretch-all, multiply)
   │  └─ GodRayGlow (Image, additive, centered on seam)
   ├─ SafeArea (RectTransform + SafeAreaFitter)             // all framed content insets here
   │  ├─ Hero_Left_IronPact (Image)                         // blue knight: sword + kite shield (blue heraldry)
   │  ├─ Hero_Right_AshenHorde (Image)                      // red warlord: spiked armor + mace
   │  ├─ TopBanner_ModeMap (Container, top-center)
   │  │  ├─ Banner_Frame_Gold (Image)                       // ornate plaque + scroll end-caps + finial
   │  │  ├─ Banner_Title_Mode (Text "CLASSIC")
   │  │  └─ Banner_Sub_Map (Text "STONEHOLD PASS")
   │  ├─ VS_Emblem (Container, center)
   │  │  ├─ VS_Glow (Image, additive)                       // hot bloom behind glyphs
   │  │  └─ VS_Text (Text "VS")                             // giant gold serif
   │  ├─ FactionPlate_Left (Container, bottom-left)
   │  │  ├─ Plate_BG_Glass_L (Image)                        // dark glass lozenge
   │  │  ├─ Crest_IronPact (Image)                          // round blue+gold shield crest
   │  │  ├─ Faction_Name_L (Text "IRON PACT")
   │  │  └─ Faction_Motto_L (Text "STRENGTH. HONOR. UNITY.")
   │  ├─ FactionPlate_Right (Container, bottom-right)
   │  │  ├─ Plate_BG_Glass_R (Image)                        // dark glass lozenge
   │  │  ├─ Crest_AshenHorde (Image)                        // round red skull crest
   │  │  ├─ Faction_Name_R (Text "ASHEN HORDE")
   │  │  └─ Faction_Motto_R (Text "STRENGTH IN CHAOS.\nGLORY IN CONQUEST.")
   │  └─ Tip_Line (Container, bottom-center)
   │     └─ Tip_Text (Text "Tip: Upgrade your Units and Commanders\nto dominate the battlefield.")
   └─ TapToContinue_Catcher (Button, transparent, stretch-all)  // implicit advance; invisible
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Screen | 0 | RectTransform | stretch-all (0,0)-(1,1) | 0.5,0.5 | offsets 0 | n/a | fills canvas |
| BG_FullBleed | Root | 0 | Image | stretch-all | 0.5,0.5 | drawn UNDER cutout | **no** | full-bleed |
| BG_Left_ColdField | BG_FullBleed | 0 | Image | left-half (0,0)-(0.5,1) | 0.5,0.5 | — | no | reveals more on widen |
| BG_Right_HotField | BG_FullBleed | 1 | Image | right-half (0.5,0)-(1,1) | 0.5,0.5 | — | no | reveals more on widen |
| Seam_EnergySplit | BG_FullBleed | 2 | Image | vertical strip x∈[0.46,0.54], y full | 0.5,0.5 | centered | no | stays seam-centered |
| Seam_LightningBlue | Seam_EnergySplit | 0 | Image | center | 0.5,0.5 | additive | no | — |
| Seam_FireOrange | Seam_EnergySplit | 1 | Image | center | 0.5,0.5 | additive | no | — |
| Vignette | BG_FullBleed | 3 | Image | stretch-all | 0.5,0.5 | multiply, on top of fields | no | full-bleed |
| GodRayGlow | BG_FullBleed | 4 | Image | center | 0.5,0.5 | additive | no | seam-centered |
| SafeArea | Root | 1 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | insets to Screen.safeArea | **yes** | all content here |
| Hero_Left_IronPact | SafeArea | 0 | Image | left-anchored, bottom | 0.5,0 | inner edge faces seam | partial | scale w/ height |
| Hero_Right_AshenHorde | SafeArea | 1 | Image | right-anchored, bottom | 0.5,0 | inner edge faces seam | partial | scale w/ height |
| TopBanner_ModeMap | SafeArea | 2 | Container | top-center (0.5,1) | 0.5,1 | hangs from safe top | yes | fixed-height, center |
| Banner_Frame_Gold | TopBanner | 0 | Image (sliced) | stretch-all (of banner) | 0.5,0.5 | 9-slice end-caps | yes | — |
| Banner_Title_Mode | TopBanner | 1 | Text | center | 0.5,0.5 | "CLASSIC" | yes | — |
| Banner_Sub_Map | TopBanner | 2 | Text | below title | 0.5,1 | "STONEHOLD PASS" | yes | — |
| VS_Emblem | SafeArea | 3 | Container | center (0.5,0.5) | 0.5,0.5 | over seam | yes | center-locked |
| VS_Glow | VS_Emblem | 0 | Image | center | 0.5,0.5 | additive behind glyphs | yes | — |
| VS_Text | VS_Emblem | 1 | Text | center | 0.5,0.5 | "VS" | yes | — |
| FactionPlate_Left | SafeArea | 4 | Container | bottom-left (0,0) | 0,0 | — | yes | pin to safe BL |
| Crest_IronPact | Plate_L | 1 | Image | left-inside plate | 0.5,0.5 | round crest | yes | — |
| Faction_Name_L | Plate_L | 2 | Text | left, top row | 0,0.5 | "IRON PACT" | yes | — |
| Faction_Motto_L | Plate_L | 3 | Text | left, bottom row | 0,0.5 | motto | yes | — |
| FactionPlate_Right | SafeArea | 5 | Container | bottom-right (1,0) | 1,0 | mirror of left | yes | pin to safe BR |
| Crest_AshenHorde | Plate_R | 1 | Image | right-inside plate | 0.5,0.5 | round skull crest | yes | — |
| Faction_Name_R | Plate_R | 2 | Text | right, top row | 1,0.5 | "ASHEN HORDE" (right-align) | yes | — |
| Faction_Motto_R | Plate_R | 3 | Text | right, bottom row | 1,0.5 | 2-line motto (right-align) | yes | — |
| Tip_Line | SafeArea | 6 | Container | bottom-center (0.5,0) | 0.5,0 | between the two plates | yes | center |
| Tip_Text | Tip_Line | 0 | Text | center | 0.5,0.5 | 2-line tip | yes | wrap |
| TapToContinue_Catcher | Root | 2 | Button (Image α0) | stretch-all | 0.5,0.5 | invisible advance | no | full screen |

**Child-order rationale:** BG (and its seam/vignette/glow) renders first (deepest); heroes next so their inner
rim sits over the seam glow; banner/VS/plates/tip are the readable foreground; the transparent tap-catcher is
top-most so a tap anywhere advances.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Seam axis:** vertical center, x = 0.50. Seam strip width ≈ **0.08W** (x∈[0.46,0.54]); glow halo ≈ 0.22W wide.
- **TopBanner_ModeMap:** width ≈ **0.30W** (≈702 px), height ≈ **0.115H** (≈124 px); top edge at y≈0.955 (its
  top ≈ 0.045H below the safe top). Centered x=0.50. Title cap-height ≈ 0.055H; sub-label ≈ 0.020H, sitting
  ≈ 0.012H below the title baseline.
- **VS_Emblem:** glyph cap-height ≈ **0.165H** (≈178 px) — the largest type on screen. Centered at (0.50, 0.50).
  Glow halo ≈ 1.6× the glyph box.
- **Hero_Left:** occupies x∈[0.02,0.44], bottom-anchored, height ≈ **0.78H**; its weapon (sword tip) reaches up
  to ≈0.80H. **Hero_Right:** mirror, x∈[0.56,0.98], height ≈ **0.80H** (mace head reaches ≈0.84H). Inner edges
  stop ≈0.06W shy of the seam so the energy stays visible between them.
- **Faction plates:** each ≈ **0.40W** wide × **0.135H** tall. Left plate pinned to safe BL with its left edge at
  x≈0.015, bottom at y≈0.045H; right plate mirrored (right edge x≈0.985). The crest circle Ø ≈ **0.085H**,
  inset ≈0.018W from the plate's inner-bottom corner (left crest at the plate's left; right crest at plate's
  right). Faction name cap-height ≈ 0.040H; motto cap-height ≈ 0.018H, ≈0.010H below the name.
- **Tip_Line:** centered x=0.50, bottom at y≈0.050H, max width ≈ **0.34W** (sits in the gap between the two
  plates, slightly higher than them — its baseline ≈0.075H so it clears the plate tops). Tip cap-height ≈ 0.020H,
  two centered lines, line-spacing ×1.15.
- **Tablet (4:3 / 1.33:1):** seam + heroes + VS stay centered; the two faction plates pull inward (reduce their
  width to ≈0.34W each) so they don't overlap the heroes; banner unchanged. **Ultrawide (21:9):** background
  fields and seam glow widen to fill; heroes, banner, VS, plates and tip stay at the same fractions and remain
  center-clustered (extra width is scenic margin). **Notch:** full-bleed BG passes under the cutout; banner,
  plates and tip live inside SafeArea so a side-notch in landscape never clips text or crests.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Line-sp | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|---|
| Banner_Title_Mode | CLASSIC | Trajan serif, gold bevel | Heavy | UPPER | +6% | — | soft gold bloom + 1px #2a1d07 stroke + drop-shadow (0,2,#000 60%) | ~59 | gradient **#f7e7b0→#caa04a** (top-light bevel) |
| Banner_Sub_Map | STONEHOLD PASS | refined serif, small-caps feel | Medium | UPPER | +14% | — | thin dark stroke; faint shadow | ~22 | #cdb784 (muted parchment-gold) |
| VS_Text | VS | massive heroic serif | Black | UPPER | -2% (tight) | — | hot inner-glow + outer gold bloom + dark drop-shadow (0,4,#000 70%) | ~178 | gradient **#fff2c0→#e2a93a**, hot-orange spill at base |
| Faction_Name_L | IRON PACT | regal serif | Bold | UPPER | +8% | — | gold bevel, 1px #1a1206 stroke, soft shadow | ~43 | gradient **#f3e2a6→#c79a44** |
| Faction_Motto_L | STRENGTH. HONOR. UNITY. | clean small-caps | Medium | UPPER | +12% | — | thin dark stroke for legibility | ~19 | #b9c6e6 (cool steel-blue tint) |
| Faction_Name_R | ASHEN HORDE | regal serif | Bold | UPPER | +8% | — | gold bevel, dark stroke, soft shadow | ~43 | gradient **#f3e2a6→#c79a44** |
| Faction_Motto_R | STRENGTH IN CHAOS. / GLORY IN CONQUEST. | clean small-caps | Medium | UPPER | +12% | ×1.15 | thin dark stroke | ~19 | #e3b6a4 (warm ember tint) |
| Tip_Text | Tip: Upgrade your Units and Commanders / to dominate the battlefield. | informational serif | Regular | Title/sentence case | +2% | ×1.15 | subtle dark shadow for over-art legibility | ~21 | #d9c79a (parchment) |

Notes: titles/names use the serif SDF (TMP) upgrade; mottos & tip may render in the same family at lighter
weight. Faction-name fill is identical gold for both sides (parity); only the **motto tint** carries the cool
vs warm faction hue. The VS is the single brightest, largest glyph — eye-magnet.

## G · MATERIALS
- **Banner frame (cast gold/bronze):** base #8a6a2c, highlight #f0d27a, deep shadow #5a4318; satin-metal
  roughness ≈0.35; engraved filigree on the face; polished beveled rim with a sharp gold specular along the
  top edge; scrolled end-caps + small central finial; subtle gold rim-bloom.
- **VS glyph metal:** the most polished gold on screen (roughness ≈0.2) with an additive hot core; reads like
  molten/charged metal where the seam energy meets it.
- **Faction crest — Iron Pact:** disc of dark steel ringed in gold; centered blue heraldic device (chevron/tower
  motif) on a cobalt field; cool rim-light, slight gloss.
- **Faction crest — Ashen Horde:** disc of blackened iron ringed in tarnished gold; a red **skull/horned**
  emblem on an oxblood field; warm rim-light, soot-worn edges.
- **Plate glass lozenges:** near-black translucent glass (#0c0e14 @ ~78% opacity) with a thin gold/bronze hairline
  edge and a faint inner top-edge sheen; very low reflection; slight blur of the art behind.
- **Hero armor — left:** brushed/blued steel with cobalt cloth and a gold-trimmed kite shield bearing the blue
  device; cool rim-light from the seam; weathered scratches.
- **Hero armor — right:** blackened spiked plate with oxblood leather and bone/horn accents, ember rim-light;
  heavy soot + battle-wear; the mace is dark iron with glinting spikes.
- **Backgrounds:** left = blue-lit ruined stone fortress, cold haze (#10131c→#1d2740 blues); right = burning
  cliff/battlefield, ember orange (#1a0d09→#5a1e12 reds) with floating sparks.
- **Seam:** electric **blue** plasma (#5fa8ff core, white-hot center) on the left lean vs **orange** flame
  (#ff8a2a→#ffd27a) on the right lean, meeting in a white-gold collision at center; strong additive bloom.

## H · COMPONENTS (interactive)
1. **TapToContinue_Catcher** — *purpose:* advance to Battle HUD on player tap (and a watchdog auto-advance).
   - *States:* **idle** = invisible, accepting input; **pressed** = optional 0.05 brightness pulse of the whole
     card (CanvasGroup or a brief additive flash) to acknowledge; **disabled** = during the 0.0–0.35s intro
     build-in, taps are swallowed (no advance) until the card is fully assembled; there is no hover/selected on
     a transient full-screen catcher.
   - *Structure:* a single stretch-all transparent `Button` over everything; raycast target on, no graphic.
   - *Feedback:* on accept, plays the OnHide transition (E/I below) then the router pushes BattleHud. A faint
     "tap to continue" affordance is **not** drawn in the mockup, so none is added (negative rule L).

*(No other interactive elements exist in this mockup — banner, VS, crests, plates, and tip are all
non-interactive display.)*

## I · ANIMATION TIMELINE (OnShow build-in; t in seconds)
| t (s) | Element | Action | Duration | Easing | Emphasis |
|---|---|---|---|---|---|
| 0.00 | BG_FullBleed + Vignette | fade 0→1 | 0.25 | ease-out | scene establishes |
| 0.05 | Seam_EnergySplit | ignite: scale-Y 0.6→1, α 0→1 | 0.30 | ease-out | the clash lights up |
| 0.10 | Hero_Left | slide in from x-0.06 + α 0→1 | 0.30 | ease-out-cubic | knight strides in |
| 0.10 | Hero_Right | slide in from x+0.06 + α 0→1 | 0.30 | ease-out-cubic | warlord strides in (mirror) |
| 0.28 | VS_Emblem | punch-in scale 1.6→1.0, α 0→1; VS_Glow flash | 0.22 | back-out | hero "VS" slam (peak emphasis) |
| 0.30 | TopBanner | drop from y+0.03 + α 0→1 | 0.22 | ease-out | mode/map title settles |
| 0.40 | FactionPlate_Left | slide up from y-0.04 + α 0→1 | 0.22 | ease-out | left allegiance reveal |
| 0.46 | FactionPlate_Right | slide up from y-0.04 + α 0→1 | 0.22 | ease-out | right allegiance reveal |
| 0.60 | Tip_Text | α 0→1 | 0.25 | linear | coaching line fades in |
| 0.85 | TapToContinue | becomes enabled | — | — | input now accepted |
| loop | Seam, VS_Glow | seam flicker (±4% α, ~0.9s) + VS bloom breathe (±3%, ~2.0s) | cont. | sine | living energy |
| loop | Hero rim-lights | slow specular shimmer | cont. | sine | metallic life |

**OnHide (advance):** VS_Glow quick flash (0.08s) → whole card scale 1→1.04 + α 1→0 over 0.22s ease-in →
router pushes BattleHud. **Auto-advance watchdog:** if no tap, auto-continue once the sim reports ready
(target ≈ 2.5–3.5 s minimum dwell so the card is read).

## J · PARTICLE & FX (passive — describe only)
- **Seam plasma/fire:** continuous additive electric arcs (left, cool) and rising flame tongues + drifting
  sparks (right, hot) along the seam; gentle flicker.
- **Embers (right field):** slow upward-drifting orange motes, parallax with the haze.
- **Cold mist (left field):** slow drifting blue fog wisps.
- **God-rays:** soft warm volumetric shafts from behind the VS/seam, faint pulse.
- **Hero rim shimmer:** subtle moving specular on inner-edge armor (cool left / warm right).
- **VS bloom:** soft halo breathing around the glyphs.
- All FX are **decorative**, low-cost, and must never animate over the readable banner/plate text enough to
  reduce legibility.

## K · EVENT BEHAVIOR
- **OnShow:** router pushes MatchIntro over the loading sim; CanvasGroup fades in; the build-in timeline (I)
  plays; input gated until t≈0.85. Player experience: a punchy, hype "fight card" reveal.
- **While shown:** purely ambient (looping seam/ember/bloom). No data polling — the matchup (mode, map, the two
  factions, mottos) is passed in by the caller at construction; nothing is read from ECS.
- **OnAdvance (tap or sim-ready):** OnHide transition → BattleHud screen is pushed; MatchIntro is destroyed/
  pooled. Player experience: the card "snaps" into the live battle.
- **OnHide/Back:** there is **no Back** from the intro (you can only go forward into the match); the hardware
  back button is ignored or also advances. No persistent state.

## L · NEGATIVE RULES (must-never)
- **Never** draw the seam, heroes, embers, or vignette **inside** SafeArea or let them clip the banner/plate
  text — FX live in the full-bleed BG layer only.
- **Never** add buttons, score, currencies, HP bars, or a visible "TAP TO CONTINUE" label that the mockup does
  not show (this is a clean cinematic card).
- **Never** mismatch faction colors: left is **always** Iron Pact cobalt-blue; right is **always** Ashen Horde
  ember-red. Do not swap sides.
- **Never** let the VS glyph stop being the single largest, brightest element, and never animate it so it
  obscures the heroes' faces.
- **Never** read or write ECS here; never block advance forever (the auto-advance watchdog must fire).
- **Never** stretch the hero renders or banner non-uniformly (preserve aspect; scale by height).
- **Never** use the cool motto tint on the right plate or the warm tint on the left.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% visual fidelity** to `MatchIntroDesign.png`: diptych split, blue-left/red-right heroes, central
  blue-lightning↔orange-fire seam, giant gold "VS", top "CLASSIC / STONEHOLD PASS" banner, two bottom faction
  plates (crest + name + motto), and the centered 2-line Tip — all present and correctly placed by the §E
  fractions (±2%).
- **Hierarchy:** node tree (§C) reproduced; BG/seam under heroes under foreground text; tap-catcher top-most.
- **Typography:** exact strings; VS cap-height ≥ 0.16H and largest on screen; both faction names render identical
  gold; mottos carry cool(L)/warm(R) tints; serif gold-bevel titles with stroke+shadow.
- **Safe area:** banner, plates, tip, crests fully inside `Screen.safeArea` on a notched landscape device;
  background bleeds under the cutout.
- **Eye flow:** VS → mode banner → heroes → faction plates → tip (verified by emphasis order in §I).
- **Animation:** build-in plays in the §I order with the VS slam as the peak; input gated until ≥0.85s; advance
  transition plays before BattleHud appears; auto-advance fires if untouched.
- **Affordance:** a tap anywhere advances; no dead time where the card is fully built but input is ignored
  beyond the documented gate.

## N · IMPLEMENTATION CONFIDENCE
**90 / 100.** Layout, copy, faction semantics, and the seam/VS focal treatment are unambiguous and fully
fraction-specified. The −10 is art-dependent: the exact hero renders, the painted blue/orange seam VFX, and the
crest devices must be authored as textures/shaders to hit the cinematic look — the uGUI structure and animation
are deterministic, but the final fidelity rides on those art assets and a convincing seam particle/shader.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ All sections A–O present and substantive, in order.
- □ Every visible string captured verbatim (CLASSIC / STONEHOLD PASS / VS / IRON PACT / STRENGTH. HONOR. UNITY. /
  ASHEN HORDE / STRENGTH IN CHAOS. GLORY IN CONQUEST. / the Tip line).
- □ Faction sides locked (blue=left=Iron Pact, red=right=Ashen Horde).
- □ Fraction-based layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Full-bleed BG under cutout; framed content in SafeArea.
- □ Hex values + materials given for gold/glass/crests/seam/heroes/bg.
- □ §12 boundary respected (no ECS read/write; no gameplay touched).
- □ No invented buttons/labels; transient auto-advance documented.
- □ Header + Source line in required format.
