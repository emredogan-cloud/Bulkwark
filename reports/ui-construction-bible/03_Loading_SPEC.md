# BULWARK — UI CONSTRUCTION SPEC · 03 · Loading
Source: design/LoadingScreenDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

---

## A — SCREEN PURPOSE
The **Loading** screen covers asset/scene preload between Splash and the destination. In the finalized **no-login** boot chain it advances **directly to the Main Menu** (Splash → Loading → **Main Menu**); during gameplay it is also reused as the inter-scene loader (e.g., into a battle). It shows a determinate progress bar over dramatic faction-clash key art.

- **What it is:** a full-bleed cinematic of two armies (Iron Pact left, Ashen Horde right) converging on a burning central capital, with a centered **"LOADING"** label, a **gold determinate progress bar**, and a **percentage readout** ("40%" in the mock).
- **When it appears:** after the Splash tap, and on any heavy scene transition. Dismisses automatically when load completes (→ Main Menu by default).
- **Emotional state to evoke:** rising tension and momentum — the war is imminent; the bar filling = the march closing in. Hype, not idleness.
- **What the player does:** nothing (waits); the screen is non-interactive. It auto-advances at 100%.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** the "armies clash" hype frame. Where Splash was a quiet lone king, Loading is the full collision — two opposing hosts foreground-flanking, a burning citadel center-stage under a stormy sky.
- **Atmosphere:** dark, smoky, embered; a central fire glow at the foot of the besieged castle throws warm light up into a cold bruised-grey storm sky. Symmetric framing: **left flank = Iron Pact (cool steel-blue banners)**, **right flank = Ashen Horde (oxblood-red banners)**, converging toward the center vanishing point (the citadel).
- **Visual hierarchy:** (1) central burning citadel + fire glow → (2) "LOADING" + progress bar + % (the functional core, low-center) → (3) the two flanking armies/banners → (4) the storm sky and dragons.
- **Color psychology:** the gold bar is the only bright "progress/hope" accent in an otherwise dark, dangerous field; blue-vs-red flanks restate the faction war; the central fire = the objective/destination heating up.
- **Material identity:** baked matte-painting environment + the global cast-gold ornate bar frame; obsidian bar track.
- **Lighting:** central warm fire uplight; cold ambient storm light on the flanks; rim light on the nearest soldiers' helms/spears; heavy vignette; warm bloom on the bar fill.
- **Contrast philosophy:** brightest = central fire + gold bar fill; the percentage and label sit just under the focal citadel so the eye lands on "how close am I".

---

## C — SCREEN DECOMPOSITION (full node tree)
```
LoadingScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base          (Image: armies-clash matte painting)
    │   ├── Vignette_Overlay     (Image: radial dark vignette, multiply)
    │   ├── FireGlow_Overlay     (Image: warm central glow, additive)        [FX]
    │   ├── Smoke_Overlay        (Image: drifting smoke, low α)              [FX]
    │   └── Grain_Overlay        (Image: faint film grain)                    [FX]
    └── LoadingHUD_Group         (centered-bottom progress cluster)
        ├── Loading_Label        (Text(TMP): "LOADING")
        ├── Label_OrnamentLeft   (Image: small gold flourish left of label)
        ├── Label_OrnamentRight  (Image: small gold flourish right of label)
        ├── ProgressBar_Group
        │   ├── Bar_FrameLeftCap  (Image: ornate gold left end-cap/finial)
        │   ├── Bar_FrameRightCap (Image: ornate gold right end-cap/finial)
        │   ├── Bar_Track         (Image: dark obsidian recessed channel, 9-slice)
        │   ├── Bar_Fill          (Image: gold gradient fill, type=Filled Horizontal, Left origin)
        │   ├── Bar_FillSheen     (Image: bright top highlight line on the fill)   [FX]
        │   └── Bar_FillTipGlow   (Image: bright glow at the fill's leading edge)  [FX]
        └── Percent_Label         (Text(TMP): "40%")
```
> Armies, banners, citadel, dragons, fire are baked into `KeyArt_Base` (one image), not separate sprites.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LoadingScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills canvas |
| Root_SafeArea | LoadingScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds HUD |
| BG_Layer | LoadingScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette_Overlay | BG_Layer | 1 | Image (multiply) | stretch-all | 0.5,0.5 | fill | ignores | full-bleed |
| FireGlow_Overlay | BG_Layer | 2 | Image (additive) | center | 0.5,0.5 | center | ignores | scales with H |
| Smoke_Overlay | BG_Layer | 3 | Image (low α) | stretch-all | 0.5,0.5 | fill | ignores | tiled/scroll |
| Grain_Overlay | BG_Layer | 4 | Image | stretch-all | 0.5,0.5 | fill | ignores | tiled |
| LoadingHUD_Group | Root_SafeArea | 0 | RectTransform + VerticalLayoutGroup | bottom-center | 0.5,0 | center | inside safe | pinned bottom-center |
| Loading_Label | LoadingHUD_Group | 0 | Text(TMP) | top-center | 0.5,0.5 | center | — | — |
| Label_OrnamentLeft/Right | LoadingHUD_Group(or sub-row) | — | Image | flank label | 0.5,0.5 | center | — | — |
| ProgressBar_Group | LoadingHUD_Group | 1 | RectTransform | center | 0.5,0.5 | center | inside safe | width ∝ screen |
| Bar_FrameLeftCap | ProgressBar_Group | 0 | Image | mid-left | 0.5,0.5 | left | — | pinned left end |
| Bar_FrameRightCap | ProgressBar_Group | 1 | Image | mid-right | 0.5,0.5 | right | — | pinned right end |
| Bar_Track | ProgressBar_Group | 2 | Image (9-slice) | stretch-all (inset) | 0.5,0.5 | center | — | stretches between caps |
| Bar_Fill | ProgressBar_Group | 3 | Image **Filled/Horizontal/Left** | stretch-all (inset, matches track) | 0,0.5 | left | — | fillAmount = progress |
| Bar_FillSheen | Bar_Fill | 0 | Image (additive) | top-stretch | 0.5,1 | top | — | follows fill width |
| Bar_FillTipGlow | ProgressBar_Group | 4 | Image (additive) | mid-left, driven | 0.5,0.5 | center | — | x = track.left + fill·width |
| Percent_Label | LoadingHUD_Group | 2 | Text(TMP) | bottom-center | 0.5,0.5 | center | — | — |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** `KeyArt_Base` scale-to-**cover** 2340×1080, centered; bleeds under any cutout. Composition is left-right symmetric about screen center (citadel on the centerline).

**Loading HUD cluster (low-center).** Forensic bounds (normalized):
- **"LOADING" label:** centered, baseline at **fy ≈ 0.74** (≈ 800 px), cap height ≈ 0.034 × 1080 ≈ 37 px. Flanking flourish glyphs ≈ 0.020 × 1080 ≈ 22 px each, ≈ 16 px gap to the text.
- **Progress bar track:** centered horizontally; vertical center at **fy ≈ 0.815** (≈ 880 px); the gold structure spans the rows fy 0.816–0.894 in the source.
  - **Track full width ≈ 0.58 × 2340 ≈ 1357 px** (left edge fx ≈ 0.21 → right edge fx ≈ 0.79).
  - **Track height ≈ 0.030 × 1080 ≈ 32 px** (the recessed channel), with the ornate gold molding adding ≈ 6 px above/below.
  - **End-caps:** ornate gold finials at each end, each ≈ 0.022 × 2340 ≈ 50 px wide, overhanging the track by ≈ 0.006 × 2340 ≈ 14 px.
- **Bar_Fill:** `Image.fillAmount = progress`. In the mock the fill reaches **fx ≈ 0.786** from a left start of ≈ 0.215, i.e. ≈ **40% of the track width filled** — exactly matching the "40%" readout. Fill origin = Left.
- **Bar_FillTipGlow:** bright warm glow centered on the leading edge of the fill (at fx ≈ track.left + 0.40·track.width in the mock); travels right as progress increases.
- **"40%" percentage label:** centered horizontally, baseline at **fy ≈ 0.90** (≈ 972 px), ≈ 0.026 × 1080 ≈ 28 px cap height — directly below the bar center.

**Vertical stack rhythm (top→bottom):** LOADING (fy0.74) → bar (fy0.815) → % (fy0.90). Gaps ≈ 0.04–0.06 H. The whole cluster occupies the lower quarter, leaving the citadel/sky clear above.

**16:9 tablet adaptation:** height-anchored; bar width is a fixed fraction of *screen width* — on a narrower (taller) frame the bar gets proportionally a touch wider relative to the revealed art, but stays centered; label/% unaffected. Background crops width inward.

**Ultrawide:** more art revealed left/right; the HUD cluster keeps its centered fractional width and position; nothing stretches edge-to-edge.

**Notch behavior:** background bleeds under the side cutout; the entire HUD is centered and lives inside `Root_SafeArea`, so a landscape side-notch never overlaps the bar (centered) — but the SafeAreaFitter still guarantees it.

---

## F — TYPOGRAPHY SPECIFICATION
- **Loading_Label "LOADING":**
  - Font: serif display (Roman/Trajan-inspired), **UPPERCASE**, tracking **+14%** (notably wide-spaced as in the mock).
  - Weight: medium; cap height ≈ **37 px**.
  - Color: brushed-gold vertical gradient **#f0d27a → #caa04a**.
  - FX: soft warm outer glow (≈ 6 px, #caa04a @ 40%) + thin dark stroke (≈ 1.5 px, #1a1206) + drop shadow (0,−2, #000 @ 50%) for contrast over the dark art.
- **Percent_Label "40%":**
  - Font: same serif family OR a clean tabular semi-condensed face for the numerals; **tabular figures** so width doesn't jitter as it counts up.
  - Weight: medium/semibold; cap height ≈ **28 px**.
  - Color: warm gold **#caa04a → #e8d5a8**, with a subtle glow + dark shadow.
  - The "%" sign slightly smaller (≈ 80%) than the digits.
- **Hierarchy:** LOADING (37 px) > 40% (28 px). Both gold, both glowing, both centered.

---

## G — MATERIAL SPECIFICATION
- **Bar_Track (obsidian channel):** very dark **#060507–#121318** (sampled #121318/#181b1e under the fill, #060507 at the empty right), recessed with an inner shadow (top edge darkest) so it reads as a carved channel; faint cool inner highlight along the bottom lip.
- **Bar_Fill (gold):** vertical gradient — top highlight **#ffe9a8**, body **#e9c24a / #caa04a**, bottom shadow **#9a7320**; warm bloom; a brighter sheen line near the top.
- **Bar end-caps & molding (cast gold):** #f0d27a highlight / #caa04a mid / #6b5320 shadow; ornate beveled finials with a small central jewel/boss; crisp specular on the bevel.
- **KeyArt sky:** cold bruised storm grey **#302725** up top, lightening toward the central glow; smoke desaturates the upper third.
- **Central fire glow:** warm core #ffb04a → #d8452b falloff, additive, seated at the citadel base (fy ≈ 0.50, center).
- **Left flank (Iron Pact):** steel armor with cool blue rim; muted royal-blue banners (#2b3a6a range under the haze) with stitched gold trim.
- **Right flank (Ashen Horde):** dark armor with warm rim; oxblood-red banners (#5a1c1a → #7a1f1a).
- **Vignette:** radial multiply, transparent center → #05060a corners, ≈ 60% strength (heavier than Splash; this is the darkest boot frame).

---

## H — COMPONENT SPECIFICATION
This screen has **no interactive components** — it is a passive loader. The "component" is the progress bar as a *display* element:

**ProgressBar (display only):**
- **Purpose:** communicate determinate load progress 0→100%.
- **Driven value:** `Bar_Fill.fillAmount` and `Percent_Label.text` bound to the loader's normalized progress.
- **States (by progress, not input):**
  - **0%:** track empty (all obsidian), tip glow at far left, % reads "0%".
  - **In-progress (e.g., 40%):** fill covers 40% from left, tip glow rides the leading edge, sheen animates, % counts up with tabular figures.
  - **100%:** fill spans full track to the right cap, tip glow reaches the right finial and flares; brief completion shimmer before auto-advance.
- **Visual feedback:** the fill never snaps backward; if the underlying loader jumps, tween the fill smoothly (≈ 0.2 s catch-up) so it reads as continuous progress.
> No buttons, no tap-to-skip in the mock. Do not add a Cancel/Skip control.

---

## I — ANIMATION TIMELINE (entrance + progress)
- **t=0.00:** CanvasGroup alpha 0; KeyArt scale 1.05; bar fill at incoming progress (often 0).
- **t=0.00 → 0.50 s:** full-screen fade in 0→1 [ease-out]; KeyArt slow push-out 1.05 → 1.00 (continues subtly throughout).
- **t=0.30 → 0.70 s:** LOADING label + flourishes fade/scale in 0.95 → 1.00 [ease-out] with a gold light sweep across the letters.
- **t=0.50 → 0.90 s:** progress bar frame/caps fade in; track appears; a quick specular glint sweeps the gold molding.
- **t=0.70 s →:** Bar_Fill begins tracking real progress; FillSheen + TipGlow loops start; Percent_Label counts up in step with the fill (tabular, smooth).
- **On 100%:** TipGlow reaches the right cap and flares (≈ 0.25 s); a soft full-bar bloom pulse; then exit.
- **Exit:** CanvasGroup fade 1→0 over 0.35 s with a small KeyArt zoom-in (1.00 → 1.03) [ease-in]; hand off to Main Menu (or the requested scene).

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **Fill sheen:** a bright highlight band slides left→right along the gold fill on a ≈ 1.4 s loop (the classic "loading shimmer").
- **Tip glow pulse:** the leading-edge glow gently pulses and emits sparse warm sparks while progress advances.
- **Central fire:** the citadel fire glow flickers on a fast irregular loop; warm uplight subtly modulates.
- **Smoke:** slow upward smoke drift over the upper third (≈ 10 s scroll), low alpha.
- **Embers:** sparse warm ember motes rising from the central fire across the lower-mid frame.
- **Banner sway:** subtle cloth sway on the flank banners if separated (else baked).
- **Grain + vignette breathe:** faint film grain; vignette can breathe ≈ ±3% very slowly for life.
> No input-driven FX; all loop independently of progress except the sheen/tip which key off the fill.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** play entrance (I); start FX (J); bind progress source; begin counting.
- **OnProgress(p):** smoothly tween fill + percentage toward p; reposition tip glow.
- **OnComplete (100%):** play completion flare → exit fade → request the destination screen (default **Main Menu** in the boot flow; or the loaded scene/battle).
- **OnHide:** stop FX loops; release the large key-art texture if memory-managed; unbind progress.
- **Non-interactive:** ignore all taps; Android back is ignored during load.

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use Unity's default progress/slider visuals or default font.
- MUST NOT render the bar as a plain rectangle — it has ornate gold end-caps + beveled molding over a recessed obsidian channel.
- MUST NOT swap the serif LOADING/percentage to a generic sans (percentage may use a tabular face but keep the prestige treatment).
- MUST NOT let the fill snap or jitter; smooth it; never run it backward.
- MUST NOT add a Skip/Cancel button, tap-to-continue, or any control not in the mock.
- MUST NOT route Loading anywhere but its destination (default Main Menu) — no login screen between.
- MUST NOT break the left-blue / right-red flank symmetry or recolor the central fire.
- MUST NOT omit vignette, fire glow, smoke, fill sheen, or tip glow.
- MUST NOT let the HUD leave the safe area, nor let the background respect the safe area (bleed it).
- MUST NOT approximate bar geometry — track ≈ 0.58 W centered at fy ≈ 0.815, % at fy ≈ 0.90.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs LoadingScreenDesign.png at 2340×1080: same clashing-armies vista, central burning citadel, left-blue/right-red flanks, centered LOADING + gold ornate bar + percentage low-center.
- **Hierarchy preserved:** citadel/fire focal; bar+label+% the functional core; flanks framing.
- **Progress fidelity:** at 40% the fill covers ≈40% of the track from the left and the readout shows "40%" — the two always agree.
- **Typography:** LOADING wide-tracked UPPERCASE serif gold w/ glow; percentage tabular gold; no default font.
- **Safe area:** HUD inside safe area; background bleeds under the cutout.
- **Animation:** fade-in, label sweep, fill sheen + tip glow loops, smooth count-up, 100% flare → exit to destination.
- **Non-interactive:** no controls; auto-advances at 100%.

---

## N — IMPLEMENTATION CONFIDENCE
**93/100.** Layout, the filled-image progress bar, the count-up binding, and all loops are straightforward and exactly measurable, so code-built fidelity is high. The −7 is purely the authored art (clashing-armies matte painting + ornate gold bar finials) which must be provided as textures; uGUI places/animates them precisely but cannot generate them. The fill+percentage agreement and shimmer/tip-glow are fully reproducible in code.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base scaled to-cover, centered, bleeding under the cutout; left-blue/right-red symmetry preserved.
- □ Vignette (≈60%), central fire glow, smoke, grain overlays present at correct blend.
- □ LOADING label centered, baseline fy≈0.74, serif UPPERCASE +14% tracking gold w/ glow + flank flourishes.
- □ Progress bar centered, track ≈0.58 W, vertical center fy≈0.815, obsidian channel + beveled gold molding + ornate end-caps.
- □ Bar_Fill = Filled/Horizontal/Left, gold gradient, fillAmount bound to progress (40% in static mock).
- □ Fill sheen band + leading-edge tip glow animating.
- □ Percentage "40%" centered, baseline fy≈0.90, tabular gold, agrees with fill.
- □ Entrance timeline + smooth count-up + 100% flare + exit-to-Main-Menu implemented.
- □ No interactive controls; taps/back ignored.
- □ SafeAreaFitter on Root_SafeArea; BG_Layer ignores safe area.
- □ No default Unity styling; no sans swap on LOADING; no Skip button.
