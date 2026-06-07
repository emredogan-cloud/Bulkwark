# BULWARK — UI CONSTRUCTION SPEC · 11 · Pause Modal (In-Match)

Source: design/PauseModalDesign.png · 1782×883 (2.02:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 boundary from `00_CONTEXT_RECOVERY.md`. This is the **in-match Pause
> modal**: a centered ornate gold-framed panel over a **dimmed, frozen battlefield**, offering **Resume /
> Settings / Surrender**. Opening it sets `Time.timeScale = 0` (the one permitted write here); closing restores
> it. The HUD beneath stays rendered but inert under the modal scrim.

---

## A · SCREEN PURPOSE
The pause overlay. When the player taps the HUD's Pause (❚❚) button, the sim freezes (`Time.timeScale = 0`) and
this **modal panel** appears centered over a darkened battlefield. It presents exactly three stacked actions:
(1) **RESUME** (primary, blue) — close the modal and unfreeze; (2) **SETTINGS** (neutral/dark) — open the
in-match settings; (3) **SURRENDER** (danger, oxblood-red) — concede the match (routes to a defeat/result flow,
typically via a confirm). A serif **"PAUSED"** title sits in the panel's gold header crowned by a small **gem
finial**. It is a true modal: input is captured by the panel + a dimming scrim; the battle is paused beneath.

## B · VISUAL DNA (screen-specific)
- **Centered ornate panel:** a near-black rounded-rectangle plate bordered by a thick **cast-gold/bronze frame**
  with engraved filigree and **decorative corner bosses/cartouches** at all four corners; a small **violet/blue
  gem** set into a gold mount crowns the top-center of the frame (the prestige finial).
- **"PAUSED" title:** serif gold-bevel UPPERCASE in the panel's upper region, with shadow + stroke.
- **Three stacked buttons**, full panel-width, generous vertical gaps, each a beveled gold-rimmed capsule:
  - **RESUME** — **royal/cobalt blue** fill (the brightest, primary CTA), small gold flourishes/diamond accents
    flanking the label.
  - **SETTINGS** — **neutral dark steel** fill (secondary), same gold rim + flourishes.
  - **SURRENDER** — **oxblood/ember red** fill (danger), same rim + flourishes.
- **Dimmed battlefield behind:** a frozen war scene — a fortress/keep upper-left, **blue (Iron Pact) banners** on
  the left, **red (Ashen Horde) banners + fire** on the right, troops mid-field — pushed dark/desaturated by a
  full-screen scrim so the panel is the clear focus. Strong vignette; faint warm fire glow at the right edge.
- Mood: solemn "the war waits" interlude — calm, regal, with the danger of Surrender clearly color-coded.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
PauseModalScreen (UiScreen, CanvasGroup, MODAL over frozen battlefield)
└─ Root (stretch-all)
   ├─ Scrim_Dim (Image, stretch-all, raycast ON)            // dims battlefield + captures outside taps
   ├─ BG_BattlefieldFrozen (visual reference only — the live HUD/field rendered beneath, inert)
   │     // NOT a child of this modal; shown here for context: keep/fortress UL, blue banners L,
   │     // red banners + fire R, troops mid-field — all dimmed by Scrim_Dim
   └─ SafeArea (RectTransform + SafeAreaFitter)
      └─ Panel (Container, centered)
         ├─ Panel_Frame_Gold (Image, sliced)                // ornate gold/bronze border + corner bosses
         ├─ Panel_BG_Dark (Image)                           // near-black inner plate
         ├─ Panel_GemFinial (Image)                         // gem in gold mount, top-center apex
         ├─ Title_Paused (Text "PAUSED")
         └─ ButtonStack (VerticalLayoutGroup)
            ├─ Btn_Resume (Button)                          // PRIMARY blue
            │  ├─ Resume_Frame (Image) · Resume_Fill (Image, blue)
            │  ├─ Resume_FlourishL (Image) · Resume_FlourishR (Image)
            │  └─ Resume_Label (Text "RESUME")
            ├─ Btn_Settings (Button)                        // neutral dark
            │  ├─ Settings_Frame (Image) · Settings_Fill (Image, dark steel)
            │  ├─ Settings_FlourishL/R (Image)
            │  └─ Settings_Label (Text "SETTINGS")
            └─ Btn_Surrender (Button)                       // danger red
               ├─ Surrender_Frame (Image) · Surrender_Fill (Image, oxblood)
               ├─ Surrender_FlourishL/R (Image)
               └─ Surrender_Label (Text "SURRENDER")
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Screen | 0 | RectTransform | stretch-all | 0.5,0.5 | offsets 0 | n/a | fills |
| Scrim_Dim | Root | 0 | Image | stretch-all | 0.5,0.5 | #05060a @ ~0.62; **raycast ON** (eats outside taps) | **no** (covers full bleed incl. under cutout) | full-bleed |
| SafeArea | Root | 1 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | insets to Screen.safeArea | **yes** | panel centers here |
| Panel | SafeArea | 0 | Container | center (0.5,0.5) | 0.5,0.5 | the modal | yes | center-locked |
| Panel_Frame_Gold | Panel | 0 | Image (sliced) | stretch-all | 0.5,0.5 | 9-slice, corner bosses preserved | yes | scales w/ panel |
| Panel_BG_Dark | Panel | 1 | Image | stretch (inset) | 0.5,0.5 | inner plate, slight inset from frame | yes | — |
| Panel_GemFinial | Panel | 2 | Image | top-center (0.5,1) | 0.5,0.5 | overhangs the frame top edge | yes | center-locked |
| Title_Paused | Panel | 3 | Text | top-center region | 0.5,1 | below the gem | yes | — |
| ButtonStack | Panel | 4 | VerticalLayoutGroup | center/lower | 0.5,0.5 | 3 buttons, equal spacing, child-force-expand width | yes | — |
| Btn_Resume | ButtonStack | 0 | Button | layout element | 0.5,0.5 | full-width capsule | yes | width = stack |
| Resume_Frame | Btn_Resume | 0 | Image (sliced) | stretch-all | 0.5,0.5 | gold rim | yes | — |
| Resume_Fill | Btn_Resume | 1 | Image | stretch (inset) | 0.5,0.5 | blue gradient | yes | — |
| Resume_FlourishL/R | Btn_Resume | 2/3 | Image | left/right inside | 0/1,0.5 | small gold diamond accents | yes | — |
| Resume_Label | Btn_Resume | 4 | Text | center | 0.5,0.5 | "RESUME" | yes | — |
| Btn_Settings | ButtonStack | 1 | Button | layout element | 0.5,0.5 | full-width capsule | yes | width = stack |
| Settings_Frame/Fill/FlourishL/R/Label | Btn_Settings | 0..4 | Image/Text | as Resume | — | dark-steel fill; "SETTINGS" | yes | — |
| Btn_Surrender | ButtonStack | 2 | Button | layout element | 0.5,0.5 | full-width capsule | yes | width = stack |
| Surrender_Frame/Fill/FlourishL/R/Label | Btn_Surrender | 0..4 | Image/Text | as Resume | — | oxblood fill; "SURRENDER" | yes | — |

**Child-order rationale:** Scrim_Dim first (dims + captures outside taps); then the Panel (frame→inner plate→gem
→title→buttons). Each button is frame→fill→flourishes→label so the label reads on top of the colored fill.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim_Dim:** full-bleed (extends under cutout), alpha ≈ **0.62** of #05060a.
- **Panel:** width ≈ **0.34W** (≈796 px) × height ≈ **0.66H** (≈713 px); centered at (0.50, 0.50). The gold frame
  border thickness ≈ **0.022W**; corner bosses ≈0.05W square. The inner dark plate insets ≈0.018W from the frame.
- **Gem finial:** Ø ≈ **0.05H**, centered x=0.50, overhanging so its center sits on the frame's top edge
  (≈y=0.83 in screen space given the panel's top at ≈y=0.83).
- **Title "PAUSED":** cap-height ≈ **0.075H** (≈81 px), centered x=0.50, baseline in the panel's upper third
  (≈0.70H screen / ≈0.78 of the panel height from its bottom). Clear gap below the gem.
- **ButtonStack:** occupies the panel's lower ~62%, centered; each button width ≈ **0.265W** (≈620 px, i.e. panel
  inner width) × height ≈ **0.105H** (≈113 px); vertical spacing between buttons ≈ **0.045H** (≈49 px). The stack
  is vertically centered in the lower region with the title clearing above it. Button corner radius ≈0.02H;
  flourish diamonds ≈0.025H inset ≈0.02W from each label end. Label cap-height ≈ **0.040H** (≈43 px).
- **Vertical rhythm (top→bottom inside panel):** gem (apex) → ~0.10H gap → "PAUSED" → ~0.10H gap → RESUME →
  spacing → SETTINGS → spacing → SURRENDER → ~0.06H bottom margin.
- **Tablet (1.33:1):** panel width grows to ≈0.42W (it's a fixed-aspect modal — scale by height, keep buttons
  full panel-width). **Ultrawide (21:9):** panel stays ≈0.34W centered; the extra width just shows more dimmed
  battlefield. **Notch:** Scrim_Dim covers the full bleed (incl. under the cutout) but the Panel + all text/
  buttons sit inside SafeArea so nothing clips; the modal stays centered in the safe rect.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| Title_Paused | PAUSED | Trajan serif, gold bevel | Heavy | UPPER | +8% | gold bevel + 1.5px #1f1405 stroke + drop-shadow (0,3,#000 70%) + soft bloom | ~81 | gradient **#f7e7b0→#caa04a** |
| Resume_Label | RESUME | clean serif/condensed | Bold | UPPER | +10% | 1px #08142e stroke + drop-shadow; on bright-blue fill | ~43 | #ffffff → #eaf2ff (cool white) |
| Settings_Label | SETTINGS | clean serif/condensed | Bold | UPPER | +10% | 1px #0a0c12 stroke + drop-shadow; on dark fill | ~43 | #ecd9a6 (warm gold-cream) |
| Surrender_Label | SURRENDER | clean serif/condensed | Bold | UPPER | +10% | 1px #2a0908 stroke + drop-shadow; on oxblood fill | ~43 | #ffe6df (warm white) |

"PAUSED" is the largest, gold-bevel focal of the panel. Button labels share weight/tracking; only fill color
encodes role (blue primary / dark neutral / red danger). The three labels are equal in size — color, not size,
ranks them.

## G · MATERIALS
- **Panel frame (cast gold/bronze):** base #8a6a2c, highlight #f3d680, shadow #523c14; satin metal (roughness
  ≈0.35); engraved filigree along the border; polished bevel with sharp top specular; **ornate corner bosses/
  cartouches** at each corner; soft gold rim-bloom; minor edge wear.
- **Panel inner plate:** near-black brushed obsidian #0c0e14 with a very subtle top-edge sheen and faint inner
  vignette so the gold frame pops.
- **Gem finial:** faceted **violet/blue amethyst-sapphire** (#5a2db0↔#3f6fff) in a gold claw mount, inner glow +
  bright specular spark; soft outer bloom (the panel's prestige jewel).
- **RESUME button:** royal/cobalt **blue** fill gradient #2b56c8→#4f8bff with a glossy top highlight #bcd3ff,
  gold-rim frame, small gold diamond flourishes; the brightest/most saturated button (primary affordance).
- **SETTINGS button:** **dark steel** fill #20242e→#2c313d (neutral), gold-rim frame + flourishes, faint top
  sheen — clearly secondary.
- **SURRENDER button:** **oxblood/ember red** fill #7a1f1a→#b6342a with a warm top highlight #e2735a, gold-rim
  frame + flourishes — danger.
- **Battlefield behind (dimmed):** keep/fortress stone upper-left; **blue cloth banners** (Iron Pact) left; **red
  cloth banners + open fire** (Ashen Horde) right; troops mid-field; the whole scene pushed dark/desaturated by
  Scrim_Dim with a strong vignette and a faint warm fire glow bleeding from the right edge.

## H · COMPONENTS (each interactive)
1. **Btn_Resume (PRIMARY)** — *purpose:* close the modal and **unfreeze** (`Time.timeScale = 1`, the saved
   value). *States:* **idle** = bright blue, gold rim; **hover/focus** = fill +10% brightness + rim glow;
   **pressed** = scale 0.96 + inner flash + label dip; **disabled** (n/a) = desat; **selected/default** = it is
   the **default focus** (controller/Enter triggers it) with a subtle persistent glow. *Structure:* frame + blue
   fill + flourishes + label. *Feedback:* click → close animation + resume SFX + restore timeScale. *(Write:
   Time.timeScale.)*
2. **Btn_Settings** — *purpose:* open in-match Settings (the game stays paused beneath). *States:* idle dark
   steel; hover fill lighten + rim glow; pressed scale 0.96 + flash; disabled desat; no default. *Structure:*
   frame + dark fill + flourishes + label. *Feedback:* click → push Settings overlay; on its close, return to
   this Pause modal (still paused). *(No sim write; navigates UI.)*
3. **Btn_Surrender (DANGER)** — *purpose:* concede the match. *States:* idle oxblood; hover fill +10% + warm rim
   glow; pressed scale 0.96 + flash; disabled desat; no default (deliberately not the default, to avoid
   accidental concede). *Structure:* frame + red fill + flourishes + label. *Feedback:* click → **confirm
   prompt** ("Surrender?") via the shared Confirm modal (recommended), then route to the defeat/result flow.
   *(Outcome is a match result; the surrender decision is sent through the permitted control path; no client-side
   balance mutation.)*
4. **Scrim_Dim** — *purpose:* modal backdrop. *States:* fades in/out with the modal; **raycast ON** so taps
   outside the panel are **absorbed** (do NOT pass to the battlefield while paused). *Optional:* a tap on the
   scrim may be treated as **Resume** (common pattern) — but since the mockup gives an explicit Resume button,
   default is **no dismiss-on-scrim** to prevent accidental unpause; if enabled, it mirrors Resume exactly.

## I · ANIMATION TIMELINE
**OnShow (open):**
| t (s) | Element | Action | Dur | Easing | Emphasis |
|---|---|---|---|---|---|
| 0.00 | (sim) | `Time.timeScale = 0` (freeze) | inst | — | battle stops |
| 0.00 | Scrim_Dim | α 0→0.62 | 0.18 | linear | field dims |
| 0.04 | Panel | scale 0.90→1.0 + α 0→1 (slight drop from y+0.02) | 0.22 | back-out | panel seats |
| 0.16 | Panel_GemFinial | spark/scale 0.6→1 + glint | 0.18 | back-out | jewel catches light |
| 0.18 | Title_Paused | α 0→1 + scale 1.06→1 | 0.20 | ease-out | "PAUSED" reads |
| 0.22 | Btn_Resume | slide up from y-0.02 + α, then default-focus glow | 0.18 | ease-out | primary first |
| 0.27 | Btn_Settings | slide up + α | 0.18 | ease-out | — |
| 0.32 | Btn_Surrender | slide up + α | 0.18 | ease-out | danger last |
| hold | Resume glow, gem | primary breathe (±5%, ~1.8s) + gem sparkle | cont. | sine | gentle life |

**OnHide (Resume / close):** buttons + title quick α→0 (0.10s) → Panel scale 1→0.94 + α→0 (0.18s, ease-in) →
Scrim_Dim α→0 (0.18s) → **restore** `Time.timeScale` to its pre-pause value → HUD live again. **Surrender:**
plays a brief panel pulse, then transitions out toward the result flow (after confirm).

## J · PARTICLE & FX (passive — describe only)
- **Gem finial:** slow rotating inner glint + soft violet/blue bloom + occasional sparkle.
- **Gold frame:** gentle rim-bloom breathe; faint specular travel along the top bevel.
- **RESUME:** soft primary under-glow pulse to draw the eye to the default action.
- **Battlefield behind:** the only motion is the dimmed scene's residual fire flicker at the right edge (very
  subdued under the scrim) — everything else is frozen (paused).
- No particles overlap the button labels enough to hurt legibility.

## K · EVENT BEHAVIOR
- **OnShow:** triggered by the HUD Pause (❚❚); immediately `Time.timeScale = 0`; Scrim_Dim + Panel animate in
  (I); Resume takes default focus. Player: the war freezes; a calm, regal pause panel demands a choice.
- **Resume:** restore timeScale → close → back to the live HUD exactly where it was.
- **Settings:** open Settings over the still-paused game; returning re-shows this modal (game stays frozen until
  Resume).
- **Surrender:** confirm → concede → route to defeat/result. (Until confirmed, nothing is committed.)
- **Back / hardware-back:** treated as **Resume** (safe, non-destructive) — never as Surrender.
- **OnHide:** Panel + scrim animate out; timeScale restored (for Resume) or handed to the result flow (for
  Surrender). No leftover scrim; HUD interactivity restored.

## L · NEGATIVE RULES (must-never)
- **Never** leave `Time.timeScale = 0` after closing via Resume — always restore the pre-pause value.
- **Never** make **Surrender** the default focus or trigger it on hardware-back (must be deliberate; confirm-
  gated).
- **Never** let outside/scrim taps reach the (paused) battlefield — Scrim_Dim raycast is ON and absorbs them.
- **Never** rank the buttons by size — they are equal height; **color** encodes role (blue primary / dark neutral
  / red danger). Don't recolor: Resume stays blue, Surrender stays oxblood.
- **Never** clip the Panel/title/buttons under a notch — Panel lives in SafeArea (scrim may bleed under).
- **Never** write anything beyond `Time.timeScale` from this modal (the surrender outcome goes through the
  permitted control path; no balance/ECS mutation).
- **Never** drop the gem finial or corner bosses — they are signature prestige elements of the panel.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `PauseModalDesign.png`: a centered ornate gold-framed dark panel with a gem finial at the
  top-center, serif "**PAUSED**" title, and three full-width stacked buttons — **RESUME** (blue, top, primary),
  **SETTINGS** (dark, middle), **SURRENDER** (oxblood-red, bottom) — over a dimmed battlefield (keep UL, blue
  banners L, red banners + fire R) — all per §E (±2%).
- **Modal behavior:** opening sets timeScale 0; Scrim_Dim absorbs outside taps; Resume restores timeScale;
  Surrender is confirm-gated and never the default/back action.
- **Hierarchy:** §C tree; scrim → panel(frame→plate→gem→title→stack); each button frame→fill→flourish→label.
- **Typography:** "PAUSED" largest/gold-bevel; three equal-size labels color-coded by role; exact strings.
- **Safe area:** panel + content inside safeArea on a notched device; scrim full-bleed under the cutout.
- **Animation:** panel back-out seat, gem spark, title pop, staggered buttons (Resume→Settings→Surrender),
  default-focus on Resume; clean close that restores the live HUD.

## N · IMPLEMENTATION CONFIDENCE
**93 / 100.** This is a clean, self-contained modal with unambiguous copy, color-coded roles, and a fully
specified layout/animation; it maps directly onto the §12-permitted `Time.timeScale` write and the existing
Pause trigger. The −7 is purely art-dependent: the ornate frame with corner bosses, the gem finial, the button
bevels/flourishes, and the painted dimmed-battlefield backdrop must be authored to hit the prestige look — the
uGUI structure, states, and behavior are deterministic.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O present, in order, substantive.
- □ Strings captured: "PAUSED", "RESUME", "SETTINGS", "SURRENDER".
- □ Button roles/colors locked (blue primary / dark neutral / oxblood danger), equal size.
- □ Opening sets Time.timeScale 0; Resume restores it; Surrender confirm-gated & not default/back.
- □ Scrim raycast ON (absorbs outside taps; battlefield stays frozen/inert).
- □ Gem finial + corner bosses preserved; "PAUSED" is the focal title.
- □ Panel + content inside SafeArea; scrim full-bleed under cutout.
- □ Only Time.timeScale written (§12 respected; no balance/ECS mutation).
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Hex + materials for frame/plate/gem/buttons/backdrop.
- □ Header + Source line in required format.
