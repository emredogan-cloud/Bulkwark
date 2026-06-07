# BULWARK — UI CONSTRUCTION SPEC · 06 · Mode Select
Source: design/ModScreenDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

> Note: the card portraits use placeholder **stick-figure** champions (archer, spartan, king, zombie, dueling pair) — placeholder art per 00/01 context. This spec reproduces the **card layout, frames, themed art tone, label colors, and treatment exactly as shown**; the figures are flagged as placeholders to be swapped for BULWARK champions, but card geometry/styling is faithful.

---

## A — SCREEN PURPOSE
**Mode Select** is the gateway after PLAY: a single horizontal row of **five themed mode cards** the player taps to choose how to play. It is a focused chooser — pick a mode, go.

- **What it is:** five portrait cards left→right — **CLASSIC**, **MISSIONS**, **TOURNAMENT**, **ENDLESS**, **MULTIPLAYER** — each an ornate gold-framed card with a themed character portrait and a colored name plate at its foot, plus a **back button** (top-left). Over a dusk battlefield backdrop.
- **When it appears:** after tapping PLAY on the Main Menu.
- **Emotional state to evoke:** "choose your battle" — five distinct flavors of combat laid out like cards on a war table; curiosity + decisiveness.
- **What the player does:** taps one card → routes to that mode's flow (intro/map/matchmaking); or taps back → Main Menu.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** a "card table of war" — minimal chrome, five hero cards as the entire content, each themed to its mode. Dusk/ember battlefield behind, slightly out of focus so the cards pop.
- **Atmosphere:** warm hazy dusk backdrop with a faint ruined skyline; the five cards are crisp, lit, and evenly spaced like trophies.
- **Visual hierarchy:** the **row of five cards** is the whole show (equal weight, read left→right); the **back button** is the only secondary chrome; the background is pure context.
- **Color psychology — each card's name plate teaches its mode:**
  - **CLASSIC** — green name plate over a **parchment/aged-map** card (the default, "the original").
  - **MISSIONS** — blue name plate, spartan/shield card (structured, Iron-Pact-coded campaign-ish).
  - **TOURNAMENT** — red name plate over a **fiery** card (high-stakes, competitive heat).
  - **ENDLESS** — purple name plate over a **green undead/horde** card (survival/dark/mystic).
  - **MULTIPLAYER** — gold/orange name plate over a **blue-vs-red duel** card (PvP clash).
- **Material identity:** ornate cast-gold beveled card frames; each card a small illustrated scene; gem-like colored name plates with gold trim; bronze back button.
- **Lighting:** each card carries its own internal lighting (fire on Tournament, eerie green on Endless, electric blue/red on Multiplayer); soft top key catches the gold frames; backdrop vignette.
- **Contrast philosophy:** five equally bright cards on a darker backdrop; the colored name plates are the brightest labels; no single card dominates (this is a chooser, not a hierarchy).

---

## C — SCREEN DECOMPOSITION (full node tree)
```
ModeSelectScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base        (Image: dusk battlefield/ruins, soft-focus)
    │   ├── Vignette_Overlay   (Image: radial vignette)
    │   ├── GodRay_Overlay     (Image: soft warm shafts)        [FX]
    │   └── Grain_Overlay      (Image: faint grain)              [FX]
    ├── BackButton_Group       (top-left)
    │   ├── Back_Frame         (Image: ornate gold square button)
    │   └── Back_Arrow         (Image: gold left-chevron glyph)
    └── CardRow_Group          (HorizontalLayoutGroup, 5 equal cards, centered)
        ├── Card_Classic
        │   ├── Card_Frame     (Image: ornate gold beveled card border, 9-slice)
        │   ├── Card_Art       (Image: parchment/map + archer portrait)
        │   ├── Card_ArtMask   (Mask/RectMask2D clipping art to frame)
        │   ├── Card_TopEmblem (Image: small emblem top-center of card)   [if present]
        │   ├── NamePlate      (Image: green gem plate w/ gold trim)
        │   └── NameLabel      (Text(TMP): "CLASSIC")
        ├── Card_Missions      (same structure; blue plate; "MISSIONS"; spartan art)
        ├── Card_Tournament    (same; red plate; "TOURNAMENT"; fiery art)
        ├── Card_Endless       (same; purple plate; "ENDLESS"; green-undead art)
        └── Card_Multiplayer   (same; gold/orange plate; "MULTIPLAYER"; blue-vs-red duel art)
```
> Each card is an identical node template (Frame + Art + Mask + NamePlate + NameLabel), differing only in art texture, plate color, and label text.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| ModeSelectScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills |
| Root_SafeArea | ModeSelectScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds back + cards |
| BG_Layer | ModeSelectScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette/GodRay/Grain | BG_Layer | 1–3 | Image | stretch-all / top | — | — | ignores | full-bleed |
| BackButton_Group | Root_SafeArea | 0 | Button | top-left | 0,1 | center | inside safe | pinned top-left |
| Back_Frame | BackButton_Group | 0 | Image | stretch-all | 0.5,0.5 | center | — | square |
| Back_Arrow | BackButton_Group | 1 | Image | center | 0.5,0.5 | center | — | — |
| CardRow_Group | Root_SafeArea | 1 | HorizontalLayoutGroup (spacing, center, child-force-expand off) | center | 0.5,0.5 | center | inside safe | scales w/ H; centered |
| Card_Classic … Card_Multiplayer | CardRow_Group | 0–4 | Button + RectTransform | — | 0.5,0.5 | center | — | fixed aspect, equal width |
| Card_Frame | (each Card) | 0 | Image (9-slice ornate) | stretch-all | 0.5,0.5 | center | — | 9-slice |
| Card_Art | (each Card) | 1 (behind frame edge) | Image | stretch-all (inset) | 0.5,0.5 | center | — | clipped by mask |
| Card_ArtMask | (each Card) | wraps Art | RectMask2D | stretch-all (inset) | 0.5,0.5 | center | — | clips art to inner rect |
| Card_TopEmblem | (each Card) | 2 | Image | top-center | 0.5,1 | center | — | small |
| NamePlate | (each Card) | 3 | Image | bottom-center | 0.5,0 | center | — | overlaps card foot |
| NameLabel | NamePlate | 0 | Text(TMP) | center | 0.5,0.5 | center | — | — |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** scale-to-cover; bleeds under cutout; soft-focus dusk ruins.

**Card row (the whole content).** Five equal portrait cards, evenly spaced, **horizontally centered** as a group, **vertically centered** (slightly above center). Forensic measurements (normalized):
- **Card vertical extent:** frame top fy ≈ **0.202**, frame bottom fy ≈ **0.816** → **card height ≈ 0.614 × 1080 ≈ 663 px**.
- **Card vertical center:** fy ≈ **0.51** (essentially screen-centered, very slightly low).
- **Card centers (X):** the five cards sit at fx ≈ **0.18 / 0.345 / 0.51 / 0.655 / 0.82** (centers ≈ 0.165 W apart, evenly distributed across the middle ~0.82 of the width).
- **Card width:** each ≈ **0.135 × 2340 ≈ 316 px** (portrait aspect ≈ 316:663 ≈ **0.48 : 1**, a tall card).
- **Inter-card gap:** ≈ 0.03 × 2340 ≈ 70 px between adjacent card edges (centers 0.165 apart − card width 0.135 = 0.03 gap).
- **Row outer span:** left edge of card 1 ≈ fx 0.112, right edge of card 5 ≈ fx 0.888 → the row occupies the central ≈ 0.776 W, leaving ≈ 0.11 W margins each side.
- **Frame thickness:** ornate gold border ≈ 0.012 × 2340 ≈ 28 px (corner ornaments larger).

**Name plates (foot of each card):**
- Each colored gem plate overlaps the card's lower portion; vertical band fy ≈ **0.65–0.805** (forensic: Tournament plate y 0.650–0.805) — i.e. the plate sits over the **bottom ~25%** of the card, centered, slightly wider than the card art so it reads as a banner clasped across the foot.
- **Plate height ≈ 0.155 × 1080 ≈ 167 px** including the gold trim; the label is vertically centered in the plate's upper portion.
- **Plate colors (forensic samples):** CLASSIC green **#2d4211/#293d11**; MISSIONS blue **#0e3863/#11386a**; TOURNAMENT red **#5d0f09/#610b08**; ENDLESS purple **#443053/#422a53**; MULTIPLAYER gold/orange **#864f02/#764701**.

**Back button (top-left):**
- Ornate gold square; forensic bounds fx **0.033–0.072**, fy **0.048–0.136** → ≈ **0.04 × 2340 ≈ 94 px** square (height ≈ 0.088 × 1080 ≈ 95 px), centered at fx ≈ 0.052 / fy ≈ 0.092. Gold left-chevron glyph centered inside.

**16:9 tablet:** cards are height-anchored (height = 0.614 H), so they keep their size; the row stays centered; on a narrower frame the inter-card gaps compress slightly but cards never overlap; back button pins to the top-left safe corner. Background crops width.

**Ultrawide:** the centered row keeps its fixed fractional layout; more background revealed at the sides; gaps may widen proportionally; back button pins to the (inset) top-left safe corner.

**Notch behavior:** background bleeds under the cutout; the back button (top-left) and the leftmost card live in `Root_SafeArea` so a left-side landscape notch insets them clear; the centered row never collides with a side cutout.

---

## F — TYPOGRAPHY SPECIFICATION
- **NameLabels (CLASSIC / MISSIONS / TOURNAMENT / ENDLESS / MULTIPLAYER):**
  - Font: bold serif display OR heavy slab, **UPPERCASE**, tracking +4%.
  - Weight: heavy; cap height ≈ **0.030 × 1080 ≈ 32 px** (auto-fit down for the longer words MULTIPLAYER/TOURNAMENT so all five share a consistent visual size and fit their plates).
  - Color: warm cream/gold **#f0e0bb → #e8d5a8** letters with a **thin dark stroke** (≈ 1.5 px, #1a0f06) and a soft drop shadow, so the same light type reads on every plate color.
  - Each label is centered on its plate.
- **Back_Arrow:** glyph, not text (gold chevron).
- **Hierarchy:** the five labels are intentionally **equal** in size/treatment (no dominant card). Only one text run type on this screen.

---

## G — MATERIAL SPECIFICATION
- **Card_Frame (cast gold):** #f0d27a highlight / #caa04a mid / #6b5320 shadow; ornate beveled molding with engraved corner scrollwork + small top-center emblem boss; crisp specular on the bevel; faint warm rim.
- **Card_Art (per mode):**
  - CLASSIC: aged **parchment/map** tone #c9b27a with a green-cloaked archer; soft warm light.
  - MISSIONS: stone/steel spartan with **blue** rim + shield; cool light.
  - TOURNAMENT: **fire** scene — embers/flame #d8452b→#7a1f1a around a crowned warrior; hot uplight.
  - ENDLESS: eerie **green** undead #5f9841/#2d4211 mausoleum tone; cold green glow.
  - MULTIPLAYER: **split blue-vs-red** duel #2b56c8 vs #c8351f with electric energy; high contrast.
- **NamePlate (gem banner):** colored body (per E) with a brighter top sheen + **gold beveled trim** clasps at the ends; inner glow tinted to the plate color.
- **Card_TopEmblem:** small gold faction/mode emblem on the frame's top center (where shown).
- **Back_Frame:** ornate cast-gold square button matching the frame language; dark inset behind the chevron.
- **Background:** soft-focus dusk battlefield — warm grey-violet sky #4d3442, ruined skyline, dust haze; vignette → #07060a corners ≈ 55%.

---

## H — COMPONENT SPECIFICATION
**Mode cards (×5).** Each whole card is a Button.
- **Idle:** lit themed card, gold frame, glowing colored name plate; a very gentle idle breathe/float (see I/J).
- **Hover:** card scales 1.04 + frame rim glow brightens + the card's themed FX intensifies (e.g., Tournament flames flare, Multiplayer sparks crackle) + name plate brightens.
- **Pressed:** scale 0.97 + brief inner flash + plate flash; card "presses into the table".
- **Disabled (locked mode):** card desaturated/greyed with a small **gold padlock** overlay + a requirement caption (e.g., "Unlocks at level X"); frame dull, FX off. (Not shown unlocked in mock, but the template must support a locked state.)
- **Selected:** on tap, the chosen card flares and lifts as the screen transitions out.
- **Feedback:** press → route to the mode (CLASSIC→classic match/intro; MISSIONS→Campaign Map/missions; TOURNAMENT→Tournament Ladder; ENDLESS→endless setup; MULTIPLAYER→Online Battle matchmaking).

**Back button:**
- **Idle:** gold square + chevron.
- **Hover:** rim glow + chevron brighten + scale 1.05.
- **Pressed:** scale 0.92 + dip.
- **Feedback:** → Main Menu.

---

## I — ANIMATION TIMELINE (entrance)
- **t=0.00:** CanvasGroup 0; background scale 1.04; cards offset +24 px down + scale 0.9.
- **t=0.00 → 0.40 s:** background fade in + slow Ken-Burns.
- **t=0.20 → 0.85 s:** the five cards **deal in** left→right like dealt cards — each fades + rises + scales 0.9→1.0 over ≈0.16 s, staggered ≈0.09 s [ease-out-back]; on each card's arrival its themed FX ignites (flame/spark/green-glow) and its name plate clasps snap with a small gold glint.
- **t=0.30 → 0.60 s (parallel):** back button drops/scales in [ease-out].
- **t≥0.9 s:** idle — cards gently float/breathe; themed FX loop.
- **Exit (on select):** chosen card flares + lifts/scales 1.08 while the others fade slightly; CanvasGroup 1→0 over 0.28 s [ease-in]; route to the mode. Exit (on back): simple fade 0.25 s → Main Menu.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **Per-card themed loops:**
  - TOURNAMENT: flickering flames + rising embers within the card.
  - ENDLESS: drifting eerie green mist + occasional spark.
  - MULTIPLAYER: small electric arcs along the blue/red split.
  - CLASSIC: gentle dust motes / warm light over the parchment.
  - MISSIONS: subtle steel glint + faint banner sway.
- **Frame glint:** a slow specular glint travels each gold frame on a staggered loop.
- **Name plate glow breathe:** each colored plate's inner glow gently pulses in its own hue.
- **Card float:** each card bobs ±2 px on a slow, slightly phase-offset sine (so the row feels alive, not rigid).
- **Background:** warm god-ray drift + dust haze + grain; vignette steady.
> No gameplay particles; all ambient and looped per card.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** entrance deal-in (I); start per-card FX (J); query lock/unlock state per mode (server-authoritative, read-only) and apply locked visuals where applicable.
- **OnCardTap(mode):** if unlocked → flare + lift → route to that mode's flow. If locked → shake + show the unlock requirement tooltip (no navigation).
- **OnBack:** fade → Main Menu (also Android back).
- **OnReturn (from a mode flow):** re-show with a quick fade; re-evaluate lock states; light re-cascade or snap.
- **OnHide:** stop FX loops; keep card art if returning soon.

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use default Unity Button visuals or default font — cards are ornate gold-framed illustrated cards with gem name plates.
- MUST NOT make the cards unequal in size or give one visual dominance — this is an equal-weight chooser (only hover/selection may scale one).
- MUST NOT recolor a mode's name plate (green Classic, blue Missions, red Tournament, purple Endless, gold/orange Multiplayer) — the plate color identifies the mode.
- MUST NOT change a card's themed art tone (parchment / steel-blue / fire / green-undead / blue-vs-red duel).
- MUST NOT omit the per-card themed FX, the frame glint, or the plate glow.
- MUST NOT drop the back button or move it off the top-left safe corner.
- MUST NOT let cards overlap, exceed the safe area, or let the background respect the safe area (bleed it).
- MUST NOT swap the serif/slab name labels to a default sans, and MUST keep all five labels the same size/treatment.
- MUST NOT treat the placeholder stick figures as final — but MUST preserve card geometry/frames/plates when swapping in BULWARK champions.
- MUST NOT add more or fewer than five cards.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs ModScreenDesign.png at 2340×1080: five equal gold-framed portrait cards centered in a row (CLASSIC/MISSIONS/TOURNAMENT/ENDLESS/MULTIPLAYER) with the correct themed art + colored name plates, over a soft-focus dusk backdrop, with a gold back button top-left.
- **Layout:** card height ≈0.614 H, centers at fx≈0.18/0.345/0.51/0.655/0.82, ≈0.135 W wide, equal gaps; row centered.
- **Plate colors exact:** green/blue/red/purple/gold-orange per mode.
- **Hierarchy:** five equal-weight cards; back button the only secondary chrome.
- **Typography:** uniform UPPERCASE serif/slab cream-gold labels with dark stroke, auto-fit to plates; no default font.
- **Safe area:** back button + leftmost/rightmost cards inset from notches; background bleeds.
- **Animation:** left→right deal-in with themed FX ignition; idle float + per-card FX loops; tap flares + routes.
- **Affordance:** each card has idle/hover/pressed (+locked) states and routes to its mode; back → Main Menu.

---

## N — IMPLEMENTATION CONFIDENCE
**91/100.** The screen is structurally clean — five instances of one card template in a centered HorizontalLayoutGroup, plus a back button — and every dimension is measured, so the layout, deal-in animation, and feedback are highly reproducible in code. −9: the five themed card illustrations, ornate gold frames, colored gem name plates, and the per-card looping FX (flame/mist/arcs) must be supplied as authored art/particles (placeholder stick figures need replacing); lock-state data is server-authoritative (read-only). Geometry, layout, and interaction are fully code-buildable.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base soft-focus dusk battlefield, scale-to-cover, bleeds under cutout; vignette + god-rays + grain.
- □ Five equal cards in a centered HorizontalLayoutGroup: height ≈0.614 H, width ≈0.135 W, centers fx≈0.18/0.345/0.51/0.655/0.82, ≈0.03 W gaps.
- □ Card template: ornate gold 9-slice frame + masked themed art + top emblem + foot name plate + label.
- □ Themed art per card: CLASSIC parchment/archer, MISSIONS steel/spartan, TOURNAMENT fire, ENDLESS green-undead, MULTIPLAYER blue-vs-red duel.
- □ Name plates colored per mode (green/blue/red/purple/gold-orange) with gold trim; labels uniform cream-gold UPPERCASE serif + dark stroke, auto-fit.
- □ Back button top-left, ornate gold square + chevron, ≈94 px, fx≈0.052/fy≈0.092.
- □ Entrance: left→right deal-in (ease-out-back) with themed-FX ignition + plate glints; back button drops in.
- □ Idle FX: per-card flame/mist/arc/dust loops, frame glints, plate glow breathe, gentle card float.
- □ Card states: idle/hover(scale+FX flare)/pressed/locked(padlock+requirement); tap → route to mode; locked → shake+tooltip.
- □ Back → Main Menu (and Android back).
- □ SafeAreaFitter insets back button + edge cards from notches; BG ignores safe area.
- □ No default styling; equal-weight cards; plate-color language intact; serif labels; exactly five cards; placeholder figures flagged.
