# BULWARK — UI CONSTRUCTION SPEC · 02 · Splash
Source: design/SplashScreenDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

---

## A — SCREEN PURPOSE
The **Splash** is the very first frame after the Unity boot logo. It establishes the BULWARK brand fantasy in one held cinematic image and acts as the "press-any-key" gate into Loading. In the finalized **no-login** flow it is screen #1 of the boot chain: **Splash → Loading → Main Menu**.

- **What it is:** a full-bleed cinematic key-art establishing shot with a central ornate gold **brand plaque** (where the wordmark/logo will render) and a single low call-to-action line: **"TAP TO BEGIN"**.
- **When it appears:** immediately after the engine splash, before any asset preload UI. It either auto-advances after a short hold or advances on the first tap anywhere on screen.
- **Emotional state to evoke:** awe + foreboding + invitation. The player should feel they are standing on the rim of a vast medieval war about to begin — a lone king on the left, a burning enemy horizon on the right, two empires colliding. Quiet anticipation, not action yet.
- **What the player does:** taps anywhere (or waits) → transition to Loading.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** the most cinematic, "movie poster" frame in the product — pure atmosphere, almost no chrome. A single wide matte-painting vista of a war-torn kingdom at a smoky amber dusk.
- **Atmosphere:** heavy volumetric haze; warm god-rays breaking from the right; a cool desaturated blue-grey LEFT (Iron Pact territory, dawn-cold steel) vs a hot ember-orange RIGHT (Ashen Horde, the burning enemy capital). The image reads as a left-cool / right-warm temperature split across a central dark void where the plaque sits.
- **Visual hierarchy:** (1) central gold plaque (brand) → (2) "TAP TO BEGIN" line → (3) the lone armored king silhouette far left with the blue banner → (4) the burning distant city + dragon silhouettes right → (5) the dark trampled battlefield foreground.
- **Color psychology:** cobalt/steel-blue left = the player's noble faction (order, sanctuary); oxblood/ember right = the threat (danger, the thing you march toward). Gold plaque = the crown/prize. Near-black center = focus rest, lets the future logo pop.
- **Material identity:** matte painted environment (no UI panels except the plaque); the plaque itself is the global cast-gold ornate beveled filigree frame over a near-black obsidian field.
- **Lighting:** low-key dusk; key light is the warm amber blowout on the right horizon; rim light catches the left king's pauldrons and banner pole in cool steel; strong vignette darkens all four corners; the plaque carries a soft warm focal bloom.
- **Contrast philosophy:** the brightest things are the right-horizon amber sky and the gold plaque edge; everything else falls into shadow. The CTA text is the brightest *interactive* affordance at the bottom.

---

## C — SCREEN DECOMPOSITION (full node tree)
```
SplashScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)   ← interactive layer only
    ├── BG_Layer (full-bleed, IGNORES safe area — extends under notch)
    │   ├── KeyArt_Base            (Image: the full matte painting)
    │   ├── Vignette_Overlay       (Image: radial dark vignette, multiply)
    │   ├── GodRay_Overlay         (Image: warm right-side light shafts, additive)  [optional FX layer]
    │   └── Grain_Overlay          (Image: faint film grain, low alpha)             [optional FX layer]
    ├── BrandPlaque_Group          (centered ornate frame cluster)
    │   ├── Plaque_Frame           (Image: ornate cast-gold beveled filigree border)
    │   ├── Plaque_Field           (Image: near-black obsidian interior fill)
    │   ├── Plaque_TopFinial       (Image: gold cross/star finial centered on top edge)
    │   ├── Plaque_BottomFinial    (Image: gold finial centered on bottom edge)
    │   └── Brand_Logo             (Image OR Text(TMP): BULWARK wordmark — placeholder field, empty in mock)
    ├── CTA_Group                  (bottom-center call to action)
    │   ├── CTA_Label              (Text(TMP): "TAP TO BEGIN")
    │   ├── CTA_OrnamentLeft       (Image: small gold flourish glyph left of text)
    │   └── CTA_OrnamentRight      (Image: small gold flourish glyph right of text)
    └── TapCatcher                 (Button, full-screen invisible — advances to Loading)
```
> The two armored figures, banners, dragons, castles, and battlefield are **painted into `KeyArt_Base`**, not separate nodes. Do not attempt to composite them as discrete sprites unless layered source art is provided; the spec treats the vista as one baked image.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Child order | Unity type | Anchor preset | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| SplashScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills canvas |
| Root_SafeArea | SplashScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets to safeArea** | holds CTA + finials |
| BG_Layer | SplashScreen | 0 (behind SafeArea) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES safe area** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover (preserve aspect, crop) |
| Vignette_Overlay | BG_Layer | 1 | Image | stretch-all | 0.5,0.5 | fill | ignores | full-bleed |
| GodRay_Overlay | BG_Layer | 2 | Image (additive mat) | top-right anchored, large | 1,1 | upper-right | ignores | anchored right edge |
| Grain_Overlay | BG_Layer | 3 | Image (tiled, low α) | stretch-all | 0.5,0.5 | fill | ignores | tiled |
| BrandPlaque_Group | Root_SafeArea | 0 | RectTransform | center | 0.5,0.5 | center | inside safe | scales with height |
| Plaque_Frame | BrandPlaque_Group | 0 | Image (9-sliced ornate) | stretch-all | 0.5,0.5 | center | — | 9-slice, no corner stretch |
| Plaque_Field | BrandPlaque_Group | 1 (behind frame edge) | Image | stretch-all (inset) | 0.5,0.5 | center | — | fills inside frame |
| Plaque_TopFinial | BrandPlaque_Group | 2 | Image | top-center | 0.5,0.5 | center | — | pinned top edge |
| Plaque_BottomFinial | BrandPlaque_Group | 3 | Image | bottom-center | 0.5,0.5 | center | — | pinned bottom edge |
| Brand_Logo | BrandPlaque_Group | 4 | Image/Text(TMP) | center | 0.5,0.5 | center | — | scales inside field |
| CTA_Group | Root_SafeArea | 1 | RectTransform + HorizontalLayoutGroup | bottom-center | 0.5,0 | center | inside safe | pinned to safe bottom |
| CTA_OrnamentLeft | CTA_Group | 0 | Image | mid-left | 0.5,0.5 | center | — | — |
| CTA_Label | CTA_Group | 1 | Text(TMP) | center | 0.5,0.5 | center | — | — |
| CTA_OrnamentRight | CTA_Group | 2 | Image | mid-right | 0.5,0.5 | center | — | — |
| TapCatcher | Root_SafeArea | 2 (topmost) | Button (transparent Image, raycast on) | stretch-all | 0.5,0.5 | fill | inside safe | full-screen |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** `KeyArt_Base` scale-to-**cover** the 2340×1080 frame (preserve aspect, crop overflow), centered. Source is 2.33:1 ≈ the production 2.167:1, so minimal crop top/bottom. Background ignores safe area and bleeds fully under any side cutout.

**Brand plaque (central focal):** In the mock the ornate plaque is horizontally centered and sits in the upper-middle. Forensic bounds (normalized):
- **Plaque outer width ≈ 0.41 × 2340 ≈ 960 px**, centered (left edge fx ≈ 0.295, right edge fx ≈ 0.705).
- **Plaque outer height ≈ 0.40 × 1080 ≈ 432 px**, top edge fy ≈ 0.045 (≈ 49 px from top), bottom edge fy ≈ 0.445 (≈ 480 px).
- **Plaque center:** screen-X 0.50, screen-Y ≈ 0.245 (slightly above vertical center — sits in the upper-middle, leaving the lower third for the battlefield and CTA).
- **Frame border thickness:** ≈ 0.018 × 1080 ≈ 19 px of cast-gold filigree on each side (corner ornaments larger, ≈ 34 px).
- **Top & bottom finials:** centered cross/star finials overhang the frame edge by ≈ 0.012 × 1080 ≈ 13 px outward.
- **Plaque_Field inset:** the obsidian interior is inset ≈ 19 px inside the frame on all sides.
- **Brand_Logo box:** centered, ≈ 0.78 × plaque-width × 0.55 × plaque-height (the empty interior where the wordmark renders).

**CTA line:** centered horizontally; baseline at **fy ≈ 0.90** (≈ 972 px), i.e. ≈ 0.10 × 1080 ≈ 108 px above the safe-area bottom. Text height ≈ 0.030 × 1080 ≈ 32 px. Ornament glyphs sit ≈ 14 px to each side of the text block with ≈ 10 px gap, each ≈ 0.022 × 1080 ≈ 24 px tall.

**TapCatcher:** fills the entire safe area (the whole screen is tappable).

**16:9 tablet adaptation:** height stays the dominant axis (matchHeight=1.0). Background reveals less width → side painting (left king / right city) crops inward slightly; plaque + CTA unaffected (height-anchored). Keep plaque centered; never let the side figures be cut by the plaque.

**Ultrawide (21:9+):** more background width revealed left and right; plaque stays centered at fixed proportional width; CTA stays centered. No element stretches.

**Notch behavior:** background ignores the cutout and bleeds under it; plaque and CTA live in `Root_SafeArea` so they never collide with a landscape side-notch. Because both focal elements are horizontally centered, a side cutout never overlaps them.

---

## F — TYPOGRAPHY SPECIFICATION
- **Brand_Logo (placeholder wordmark — empty in mock):** intended serif display, Roman/Trajan-inspired, heavy weight, UPPERCASE, letter-tracking +4%, heavy gold bevel (#f0d27a highlight → #6b5320 shadow) with soft warm outer bloom and a thin dark stroke (≈ 2 px, #1a1206) for legibility on the obsidian field. Cap height ≈ 0.18 × 1080 ≈ 195 px if a single word. (Mock shows an empty plaque; record as placeholder, do not invent brand text.)
- **CTA_Label "TAP TO BEGIN":**
  - Font: light serif display or refined semi-condensed serif, **letter-spaced wide** (tracking +12–16%), **UPPERCASE**.
  - Weight: regular/medium (deliberately understated vs a button).
  - Size: cap height ≈ 0.030 × 1080 ≈ **32 px**.
  - Color: warm parchment gold **#e8d5a8 → #caa04a** subtle vertical gradient.
  - Glow: soft warm outer glow (≈ 6 px, #caa04a @ 35%) for a gentle pulsing affordance; thin dark drop shadow (offset 0,−2, #000 @ 50%) for contrast over the battlefield.
  - No box/pill behind it — it floats over the painting.
- **Hierarchy:** Logo (≈195 px) ≫ CTA (≈32 px). Only two text runs exist on this screen.

---

## G — MATERIAL SPECIFICATION
- **Plaque_Frame (cast gold filigree):** highlight #f0d27a / mid #caa04a / shadow #6b5320; medium-low roughness with crisp specular hits on the bevel ridges; ornate engraved scrollwork on the corners and top/bottom finials; faint warm rim-bloom. Edge treatment: rounded beveled molding, NOT a flat stroke.
- **Plaque_Field (obsidian interior):** near-black **#151614–#181819** (sampled center #181819) with a subtle top-lit vertical gradient (slightly lighter at top), faint inner shadow from the frame, very low sheen.
- **KeyArt_Base sky (left → right):** cool grey-violet dusk left **#39313b**; warming through **#683e41** to a hot ember blowout right **#bb452f** near the burning horizon. Distant right city glow pushes toward #d8452b.
- **Left king / armor:** desaturated steel with cool blue rim; cloak deep cobalt-charcoal (#2b2a35 range under the haze); blue banner cloth muted royal blue with stitched trim.
- **Right enemy:** oxblood/ember silhouettes; the far castle is a dark mass against the amber sky with internal fire glows (#7a1f1a → #d8452b).
- **Battlefield foreground:** trampled near-black earth #211a19 with scattered warm embers; strong corner vignette.
- **Vignette:** radial multiply, transparent center → #05060a at corners, ≈ 55% strength.

---

## H — COMPONENT SPECIFICATION
**TapCatcher (full-screen advance button) — the only interactive element.**
- **Purpose:** advance Splash → Loading on any tap.
- **Structure:** transparent full-screen Image with raycastTarget on, wrapped in a Button; the visible affordance is the pulsing CTA_Label.
- **States:**
  - **Idle:** invisible catcher; CTA_Label gently pulses (see I/J) to signal "tap anywhere".
  - **Hover (pointer, non-touch):** CTA_Label brightens ≈ +12% and its glow widens slightly.
  - **Pressed:** brief CTA_Label flash to near-white (#fff4dc) over ≈ 0.08 s; optional subtle full-screen warm flash.
  - **Disabled:** during the outgoing transition the catcher disables to prevent double-advance.
  - **Selected:** n/a.
- **Visual feedback:** the entire screen begins its exit transition (fade/zoom) the instant the tap registers.

---

## I — ANIMATION TIMELINE (entrance)
All times relative to OnShow t=0. Easing intent in brackets.
- **t=0.00:** CanvasGroup alpha 0; KeyArt slightly zoomed in (scale 1.06).
- **t=0.00 → 0.80 s:** full-screen fade in 0→1 [ease-out]; simultaneously KeyArt slow Ken-Burns push-out 1.06 → 1.00 [linear, continues subtly the whole time].
- **t=0.40 → 1.10 s:** BrandPlaque scales in 0.92 → 1.00 with a soft gold bloom flare on the frame edges [ease-out-back, gentle]; finials catch a quick specular sweep.
- **t=0.90 → 1.30 s:** Brand_Logo (if present) fades/bevels in with a left-to-right gold light sweep across the wordmark.
- **t=1.20 → 1.60 s:** CTA_Group fades in 0→1 [ease-out] and begins its idle pulse loop.
- **t≥1.60 s:** screen idle; auto-advance timer (if used) runs ≈ 3–5 s, or first tap advances.
- **Exit (on tap/auto):** CTA flash (0.08 s) → CanvasGroup fade 1→0 over 0.35 s with a tiny KeyArt zoom-in (1.00 → 1.03) [ease-in]; hand off to Loading.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **CTA pulse:** CTA_Label opacity oscillates 0.78 ↔ 1.00 and glow radius breathes on a slow ≈ 1.6 s sine loop (the "tap me" heartbeat).
- **God-rays:** slow warm volumetric shafts from the right horizon drift/shimmer almost imperceptibly (≈ 8 s loop) over the right third.
- **Embers:** a handful of slow upward-drifting warm ember motes over the burning right horizon and battlefield (very sparse, low alpha).
- **Plaque bloom:** the gold frame edges carry a faint continuous warm bloom; one slow specular glint travels the top edge every ≈ 6 s.
- **Banner sway:** if banners are a separate layer, a very subtle cloth sway (≈ 4 s); otherwise baked.
- **Grain:** faint animated film grain over the whole frame for a cinematic matte-painting feel.
> No gameplay particles. All FX are ambient and looped; none block input.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** play entrance timeline (I); start idle FX (J); start optional auto-advance timer; arm TapCatcher.
- **OnTap / auto-advance fires:** disable TapCatcher; play CTA flash + exit fade; request UiRouter push **Loading**.
- **OnHide:** stop FX loops; release any large key-art texture reference if memory-managed.
- **No back behavior** (this is the first screen); Android back here should quit-confirm or be ignored per platform policy (not depicted in mock).

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use default Unity UI styling (no flat grey Button, no default font on the CTA).
- MUST NOT swap the serif CTA/wordmark to a sans-serif.
- MUST NOT add a visible button box/pill behind "TAP TO BEGIN" — it floats over the art.
- MUST NOT recolor the temperature split (cool-left / warm-right) or symmetrize it.
- MUST NOT place the plaque off-center or resize it so it crops the side figures.
- MUST NOT let interactive content (plaque/CTA) sit outside the safe area, nor let the background respect the safe area (it must bleed full).
- MUST NOT omit the vignette, god-rays, ember drift, or CTA pulse.
- MUST NOT invent brand text inside the plaque — it is an empty gold plaque in the mock; treat the wordmark as a placeholder field.
- MUST NOT add any login/account UI (no-login flow).
- MUST NOT approximate the gold frame as a flat 1-px stroke — it is a beveled ornate molding with corner finials.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs SplashScreenDesign.png at 2340×1080: same cinematic vista, same cool-left/warm-right split, same centered ornate gold plaque in the upper-middle, same single "TAP TO BEGIN" line low-center.
- **Hierarchy preserved:** plaque is the dominant focal object; CTA is the only conspicuous interactive affordance; no extra chrome.
- **Typography:** CTA is wide-tracked UPPERCASE serif in parchment-gold with soft glow + dark shadow; not a default font, not a sans.
- **Safe area:** plaque + CTA inside safe area; background bleeds under the cutout.
- **Eye flow:** plaque → CTA → side figures → burning horizon, matching B.
- **Animation:** fade-in + Ken-Burns + plaque scale-in + CTA pulse all present; tap triggers flash + fade to Loading.
- **Interactive affordance:** tapping anywhere advances; CTA pulses to invite the tap.

---

## N — IMPLEMENTATION CONFIDENCE
**92/100.** The screen is compositionally simple (one baked vista + one plaque + one CTA + one full-screen tap), so layout/animation are highly reproducible. The −8 is because the heavy lift is *art*: the cinematic matte painting and the ornate cast-gold plaque must be supplied as authored textures to hit ≥95% — code-built uGUI can place and animate them precisely but cannot generate the painting. The empty wordmark plaque also leaves the final logo treatment to be confirmed.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base imported and scaled to-cover, centered, bleeding under cutout.
- □ Vignette + god-ray + grain overlays present at correct blend/strength.
- □ Plaque centered at fx0.50 / fy≈0.245, width ≈0.41W, height ≈0.40H, ornate beveled gold frame ≈19 px + corner finials.
- □ Plaque_Field obsidian #151614–#181819 with subtle top-lit gradient.
- □ Top & bottom centered gold finials overhanging the frame.
- □ Brand_Logo placeholder centered in field (no invented text).
- □ CTA "TAP TO BEGIN" centered, baseline fy≈0.90, serif UPPERCASE wide-tracked parchment-gold, glow+shadow, with side flourishes.
- □ Full-screen TapCatcher Button on top, raycast enabled.
- □ Entrance timeline (fade + Ken-Burns + plaque scale-in + logo sweep + CTA fade) implemented.
- □ Idle FX: CTA pulse, god-ray shimmer, ember drift, plaque glint.
- □ Tap/auto → CTA flash + fade → push Loading; catcher disables during exit.
- □ Safe-area fitter on Root_SafeArea; BG_Layer ignores safe area.
- □ No default Unity styling; no sans swap; no login UI.
