# BULWARK — UI CONSTRUCTION SPEC · 13 · Defeat

Source: design/DefeatScreenDesign.png · 1915×821 (≈2.33:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas; all geometry is **fractions of 2340×1080**. This is a **POST-MATCH RESULT** screen shown when the standard match resolves against the player (the player's own statue/objective has fallen). Verdict comes from ECS `MatchState.Outcome == Defeat` (read-only). **No code here** — construction spec only.

---

## A · SCREEN PURPOSE
The **Defeat** result screen is the somber counterpart to Victory. It must (1) communicate the loss clearly but without humiliating the player (dignified, cold, "the war goes on" tone — not punishing), (2) give the player two clear recovery paths — **RETRY** (re-attempt the same match immediately) and **CONTINUE** (return to the hub) — and (3) keep dwell short. Crucially there is **no reward chest, no gem/time stat row** here (you lost — nothing to celebrate or grant); the focus is purely verdict + two routing actions. Source of truth = ECS `MatchState.Outcome == Defeat`.

State machine position: `Battle (HUD) → [own statue falls] → Defeat → {RETRY → Match Intro/Battle | CONTINUE → Main Menu}`. Two forward actions, no celebratory reveal.

---

## B · VISUAL DNA (screen-specific)
- **Verdict colour = SOMBER, COLD, DESATURATED.** This is the "you lost" palette: a grey-steel field, cold blue-grey haze, muted gold frame (less bloom than Victory), and a single deep **oxblood-red** accent reserved for the **RETRY** button (the emotional "try again" / Ashen-coded action). No warm god-rays, no gold sparkle storm — the lighting is overcast and low-energy.
- **Hero ornament:** a smaller, more austere **gold crown** crest centered on the top frame edge (a fallen/diminished crown vs Victory's crown+swords trophy). Optionally tarnished/dimmer gold.
- **Right-side mood prop:** a **defeated / kneeling armored warrior** (your fallen champion) is rendered into the right portion of the background, slumped, in cold steel armor — a storytelling silhouette that sells the loss.
- **Background:** full-bleed bleak battlefield at dusk/overcast — broken banners, a tattered flag drooping left, smoke, ruined ground, cold grey-blue tones (`#3a3f48` haze). Heavily vignetted and desaturated; pushed back behind the panel.
- **Panel:** same architectural language as Victory (obsidian interior + ornate gold beveled frame, filigree corners) but **cooler and dimmer** — the inner glow is faint/cold rather than warm-gold; the frame catches less rim-light.
- **Mood:** dignified defeat, cold, quiet, resolute. Contrast is lower than Victory (no luminous focal prize); the two buttons are the brightest objects, with RETRY (red) slightly hotter than CONTINUE (blue).

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
DefeatScreen (UiScreen root; CanvasGroup; full-rect)
└── FullBleedRoot (RectTransform, stretch-stretch, ignores safe area)
    ├── BG_Battlefield (Image — bleak overcast battlefield, full-bleed)
    ├── BG_FallenWarrior (Image — kneeling defeated champion, right side; may be baked into BG)
    ├── BG_Desaturate (Image/Material — cold desaturation + blue-grey grade overlay)
    ├── BG_Vignette (Image — radial dark vignette, multiply)
    └── BG_DarkenScrim (Image — black ~0.40, focuses panel)
    SafeAreaRoot (RectTransform — Screen.safeArea inset)
    └── ResultPanel (Container; anchored center; ~0.46w × ~0.86h)
        ├── Panel_Frame (Image — ornate gold beveled 9-slice frame, dimmer)
        │   └── Panel_Interior (Image — near-black obsidian, faint cold inner glow)
        ├── Crest_Group (anchored top-center, overlaps frame top edge)
        │   ├── Crest_BannerCloth_L (Image — muted banner fan)
        │   ├── Crest_BannerCloth_R (Image — muted banner fan)
        │   └── Crest_Crown (Image — austere gold crown, top-most)
        ├── Title_Defeat (Text "DEFEAT" — gold-bevel serif, desaturated/cooler)
        │   └── Title_Glow (Image — faint gold/grey bloom, low intensity)
        ├── Subtitle_Ribbon (Image — dark engraved ribbon strip)
        │   └── Subtitle_Text (Text "Your statue has fallen.")
        └── Buttons_Row (Horizontal container — two equal CTAs side by side)
            ├── CTA_Retry (Button — oxblood-red fill, gold frame)
            │   ├── Retry_Frame (Image — gold beveled frame)
            │   ├── Retry_Fill (Image — oxblood-red gradient + inner red rim)
            │   └── Retry_Label (Text "RETRY")
            └── CTA_Continue (Button — royal-blue fill, gold frame)
                ├── Continue_Frame (Image — gold beveled frame)
                ├── Continue_Fill (Image — royal-blue gradient + inner blue rim)
                └── Continue_Label (Text "CONTINUE")
```

---

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| DefeatScreen | Canvas | — | UiScreen+CanvasGroup | stretch/stretch | .5,.5 | — | root | fade in/out |
| FullBleedRoot | DefeatScreen | 0 | RectTransform | stretch/stretch | .5,.5 | — | **ignores** | full-bleed under notch |
| BG_Battlefield | FullBleedRoot | 0 | Image | stretch/stretch | .5,.5 | center-crop | no | AspectFill, focal center |
| BG_FallenWarrior | FullBleedRoot | 1 | Image | right/center | 1,.5 | right | no | anchored to right edge; clips off-screen on narrow |
| BG_Desaturate | FullBleedRoot | 2 | Image/Mat | stretch/stretch | .5,.5 | — | no | full-screen grade |
| BG_Vignette | FullBleedRoot | 3 | Image | stretch/stretch | .5,.5 | center | no | radial |
| BG_DarkenScrim | FullBleedRoot | 4 | Image | stretch/stretch | .5,.5 | — | no | flat black α0.40 |
| SafeAreaRoot | DefeatScreen | 1 | RectTransform+SafeAreaFitter | stretch/stretch | .5,.5 | — | **applies** | insets on notch sides |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center/center | .5,.5 | center | yes | width = min(0.46·W, height-cap); see §E |
| Panel_Frame | ResultPanel | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | 9-slice |
| Panel_Interior | Panel_Frame | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | inset by frame thickness |
| Crest_Group | ResultPanel | 1 | RectTransform | top/center | .5,.5 | center | yes | mid sits on frame top edge |
| Crest_BannerCloth_L/R | Crest_Group | 0,1 | Image | center/center | .5,.5 | center | yes | mirror scale.x=±1 |
| Crest_Crown | Crest_Group | 2 | Image | top/center | .5,1 | center | yes | top z |
| Title_Defeat | ResultPanel | 2 | Text (TMP) | top/center | .5,1 | center | yes | auto-size capped |
| Title_Glow | Title_Defeat | 0 | Image | center/center | .5,.5 | center | yes | behind glyphs, faint |
| Subtitle_Ribbon | ResultPanel | 3 | Image (9-slice) | top/center | .5,1 | center | yes | width ≈ 0.78 panel interior |
| Subtitle_Text | Subtitle_Ribbon | 0 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | one line auto-fit |
| Buttons_Row | ResultPanel | 4 | HorizontalLayoutGroup | bottom/center | .5,0 | middle-center | yes | two equal cells + center gap; pinned above bottom frame |
| CTA_Retry | Buttons_Row | 0 | Button | — | .5,.5 | center | yes | ≈0.46 of row width |
| Retry_Frame | CTA_Retry | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | gold frame |
| Retry_Fill | CTA_Retry | 1 | Image | stretch/stretch | .5,.5 | — | yes | inset; red gradient |
| Retry_Label | CTA_Retry | 2 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | auto-size capped |
| CTA_Continue | Buttons_Row | 1 | Button | — | .5,.5 | center | yes | ≈0.46 of row width |
| Continue_Frame | CTA_Continue | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | gold frame |
| Continue_Fill | CTA_Continue | 1 | Image | stretch/stretch | .5,.5 | — | yes | inset; blue gradient |
| Continue_Label | CTA_Continue | 2 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | auto-size capped |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
W=2340, H=1080, match HEIGHT, fractions from top-left.

**Background (full-bleed):** BG_Battlefield 0,0→1,1 AspectFill center. BG_FallenWarrior anchored to the **right** edge (1,.5), occupying roughly the right 0.26·W × 0.70·H of the field; on narrow/notched devices its outer edge clips off-screen (acceptable — it is mood, not content). BG_Desaturate + BG_Vignette + BG_DarkenScrim (α≈0.40) full-rect.

**ResultPanel:** width = 0.46·W ≈ 1076 px, centered (x=0.50·W). Height ≈ **0.86·H** ≈ 929 px (shorter than Victory's 0.92 — fewer internal rows, so the card is less tall), vertically centered. Cap: width = `min(0.46·W, 1.30·panelHeight)` (the Defeat card is a touch wider-to-tall than Victory because its content is title + subtitle + a wide two-button row).

**Internal vertical rhythm (fractions of PANEL height, 0=top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Crest (crown apex above frame) | ~ -0.03 to 0.07 | 0.16 |
| Title "DEFEAT" | 0.30 | 0.18 |
| Subtitle ribbon | 0.48 | 0.07 |
| Buttons row | 0.80 | 0.16 |

(Note: with no stat row and no reward, the content sits higher/sparser; the title is large and dominant, then the two buttons anchor the lower third — a deliberately empty middle that reads as somber.)

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.82, centered.
- Subtitle ribbon width ≈ 0.78, centered.
- Buttons row width ≈ 0.86, centered; each button ≈ 0.42 of panel width, center gap ≈ 0.04; button height ≈ 0.13·panelH. RETRY is the **left** cell, CONTINUE the **right** cell (matches source).

**Notch/safe-area:** identical rule to Victory — panel centered in the *safe* rect; background art bleeds under the cutout. **Tablet/ultrawide:** same cap behavior; ultrawide reveals more bleak battlefield and pushes the fallen-warrior prop further right.

---

## F · TYPOGRAPHY (per text)
Sizes px@1080. Same families as Victory but **cooler/dimmer** treatment on the title (less bloom, slightly desaturated gold).

| Text | Content | Family / personality | Weight | Caps | Tracking | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|
| Title_Defeat | `DEFEAT` | Trajan-style serif, heavy, solemn | Black/ExtraBold | ALL-CAPS | +6% | desaturated gold gradient, **low** bloom, faceted bevel | ~120–130 (slightly larger than Victory — it is the lone focal) | gradient `#e8cf86` (top) → `#a98a45` (bottom), cooler/greyer than Victory's hot gold; cores `#f3ead2` | dark stroke `#2a2008` ~3px + shadow `#000` α0.65 (0,4) |
| Subtitle_Text | `Your statue has fallen.` | clean serif, quiet | Medium | Title-case (as shown) | +1% | subtle, no glow | ~30–34 | cool parchment `#d8d0bc` (slightly grey) | dark stroke `#241a06` ~1.5px, shadow α0.5 (0,2) |
| Retry_Label | `RETRY` | serif display, urgent | Bold | ALL-CAPS | +8% | bright clean, faint red-tinged glow | ~42–46 | warm white `#fff3ec` | dark red stroke `#3a0a06` ~2px + shadow α0.6 (0,3) |
| Continue_Label | `CONTINUE` | serif display, authoritative | Bold | ALL-CAPS | +8% | bright clean, faint blue glow | ~42–46 | warm white `#fffbf0` | dark blue stroke `#0a1f44` ~2px + shadow α0.6 (0,3) |

Both button labels share the same size so neither visually outranks the other (RETRY and CONTINUE are co-equal in weight; the *color* differentiates them).

---

## G · MATERIALS
- **Gold frame / crown / title:** same brushed antique-gold family as Victory but **tarnished/cooler**: highlights `#d8bf78`, mid `#a98a45`, shadow `#5a4720`–`#2a2008`. Lower bloom; rim-light is dimmer (overcast scene). Crown reads slightly tarnished.
- **Panel interior:** obsidian `#0a0b0f`–`#14161e`, matte; inner glow is **cold/faint** (a low blue-grey radial `#2a3340` α≈0.12) rather than warm — reinforces the somber read.
- **Subtitle ribbon:** dark engraved bronze/stone `#2a2210`–`#3a3018`, recessed carved channel.
- **RETRY fill (oxblood-red):** vertical gradient deep `#5a1410` (bottom) → `#b84130` (top), with an inner red rim-glow `#e0604a` α≈0.4; satin, slightly matte (less glossy than Victory's blue to keep the mood restrained). Edges catch dim gold from the frame.
- **CONTINUE fill (royal-blue):** vertical gradient `#0d2a66` → `#2f6fd6`, inner cobalt rim-glow `#5aa0ff` α≈0.4 — same construction as Victory's CTA but at slightly lower glow intensity to match the muted scene.
- **Background grade:** desaturate toward grey-blue; haze `#3a3f48`; banners/cloth muted; the fallen warrior in cold steel `#4a525c` with weak rim-light.
- **Bloom budget:** minimal. Only the two button top-edges and the title carry slight bloom; everything else is matte/overcast. No focal-prize bloom (there is no prize).

---

## H · COMPONENTS (states + feedback)
**CTA_Retry (oxblood-red Button):**
- **Idle:** red gradient fill, gold frame, faint inner red rim; full opacity.
- **Hover:** frame +8%, inner red glow +25%, scale 1.0→1.03 (120 ms ease-out).
- **Pressed:** scale →0.97 (60 ms), fill darkens ~10%, brief red flash, "blade-draw" SFX.
- **Disabled:** only if retry is unavailable for this mode (rare); then fill desaturates to grey `#4a4038`, label `#8a8278`, non-interactive.
- **Confirm:** quick red ring flash → exit to Match Intro/Battle.

**CTA_Continue (royal-blue Button):**
- **Idle/Hover/Pressed:** identical interaction model to Retry but blue; "anvil" click SFX.
- **Disabled:** N/A (always available).
- **Confirm:** blue ring flash → exit to Main Menu.

**Co-equal emphasis:** neither button "breathes"/pulses by default (this is a calm decision screen, not a celebratory funnel). Optionally a very subtle slow glow on RETRY only (it is the encouraged "try again" path), but keep it understated.

**Focus order / input:** default selected control = **CONTINUE** (the safe/neutral path) on gamepad — but RETRY is one step left. Back/B → CONTINUE (return to hub; non-destructive). Tab/D-pad cycles between the two. No tap-anywhere shortcut (must choose deliberately).

---

## I · ANIMATION TIMELINE
The Defeat reveal is **slower, heavier, and quieter** than Victory — no celebratory burst, no count-up.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.45 | FullBleedRoot CanvasGroup | fade 0→1 (bleak field + scrim in) — slower than Victory | linear |
| 0.10 | 0.50 | BG_Desaturate | grade ramps from neutral→full cold desaturation | ease-in |
| 0.20 | 0.45 | ResultPanel | scale 0.92→1.0 + α 0→1 (settles, **no overshoot** — a heavy, dignified entrance) | ease-out |
| 0.35 | 0.40 | Crest_Group | crown lowers/settles onto frame (slow, slight downward "weight") | ease-out |
| 0.45 | 0.55 | Title_Defeat | α 0→1 + scale 1.06→1.0; faint bloom rises then settles low; optional slow desaturate-in of the gold | ease-out |
| 0.55 | 0.25 | FX_TitleDust | one-shot faint grey/ash dust drift across the title (somber, not sparkle) | linear |
| 0.85 | 0.35 | Subtitle_Ribbon + text | α 0→1 + slide up 8px | ease-out |
| 1.15 | 0.35 | CTA_Continue | α 0→1 + scale 0.94→1.0 | ease-out |
| 1.20 | 0.35 | CTA_Retry | α 0→1 + scale 0.94→1.0 (appears just after / together) | ease-out |
| 1.55 | (idle) | both CTAs | settle to idle; RETRY may carry a very subtle slow red glow (period ~2.4 s) | sine in-out (optional) |

**OnRetry (exit):** 60 ms press dip → 120 ms red ring flash → panel α 1→0 + scale →0.96 (180 ms) while bg fades → route to Match Intro (or directly re-enter Battle for the same mode/seed). 
**OnContinue (exit):** same but blue flash → route to Main Menu. 
**Skip rule:** a tap during reveal snaps all tweens to end-state and enables both buttons (no celebratory content to protect, so skipping is fully allowed).

---

## J · PARTICLE & FX (passive)
- **Ash/smoke drift (background):** slow upward grey smoke + a few falling ash flecks across the bleak field; low opacity; reinforces overcast loss tone. Stays behind the scrim.
- **Title dust (one-shot at reveal):** faint ashen motes (NOT gold sparkles) — desaturated grey/`#9a948a`.
- **Faint cold inner panel glow:** very low, static.
- **Optional RETRY glow:** subtle slow red breathing (understated).
- **No god-rays, no gold sparkle storm, no halo, no prize bloom** — the absence is intentional and part of the somber DNA.

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload may be `{ mode, canRetry:bool }` (no reward/time values needed). Verify ECS `MatchState.Outcome == Defeat`. Run the slow §I reveal. Battle input disabled.
- **OnRetry:** the encouraged recovery CTA. If `canRetry`, fires exit → re-enter the same match (Match Intro or direct Battle re-init for the same mode/level). Debounced. Issues only navigation + a fresh match start request — **no balance mutation** (retry is free per design unless an energy/ticket system exists, which canon CUTs — see 00 §5; if a future ticket gate is added it is server-authoritative, not decided here).
- **OnContinue:** returns to Main Menu (`UiRouter` pop/replace). Debounced.
- **OnBack (B/Esc):** aliased to CONTINUE (non-destructive return to hub).
- **No mutation / read-only:** the screen issues no ECS commands beyond requesting a new match on Retry; it never edits balance/state (§12).
- **Idempotency:** re-entry settles to end-state immediately.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** show any reward (no chest, no gems, no XP, no count-up) — defeat grants nothing here. 
2. Do **not** use warm/triumphant gold bloom, god-rays, or sparkle FX — the scene is somber/cold/desaturated. 
3. Do **not** make RETRY and CONTINUE different sizes — they are co-equal; only color differentiates (RETRY red, CONTINUE blue). 
4. Do **not** add a stat row, time, or reward slots (those belong to other variants). 
5. Do **not** put interactive content under the notch; only background art bleeds there. 
6. Do **not** mutate currency/state client-side (§12); Retry only requests a new match. 
7. Do **not** auto-dismiss or auto-retry — the player must choose. 
8. Do **not** stretch frame filigree corners (9-slice borders fixed). 
9. Do **not** humiliate (no taunting copy, no enemy gloating) — copy is exactly `DEFEAT` / `Your statue has fallen.`. 
10. Do **not** hard-code the loss — gated by ECS `MatchState.Outcome == Defeat`. 
11. Do **not** use portrait or assume width is stable; match HEIGHT, fraction-based.

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] Cold, desaturated full-bleed bleak battlefield with a **kneeling/defeated armored warrior** on the right, vignetted + scrimmed.
- [ ] Centered obsidian card with a **dimmer/tarnished** ornate gold frame, width ≈ 0.46·W, height ≈ 0.86·H.
- [ ] An austere **gold crown** crest (no swords) breaks the top frame edge, with muted banner cloth behind.
- [ ] `DEFEAT` renders as a heavy, solemn, slightly **desaturated** gold-bevel serif, ALL-CAPS, ~120–130 px@1080, low bloom, dark stroke + shadow.
- [ ] Subtitle reads exactly `Your statue has fallen.` in a recessed dark ribbon, cool-parchment text.
- [ ] Exactly **two co-equal buttons**: **RETRY** (left, oxblood-red `#5a1410`→`#b84130`, gold frame) and **CONTINUE** (right, royal-blue `#0d2a66`→`#2f6fd6`, gold frame), same size, white serif labels.
- [ ] The empty middle band (no stat/reward content) is preserved — sparse, somber composition.
- [ ] Reveal is slow/heavy/quiet (no overshoot, no count-up, no gold sparkles); ash/smoke drifts in the background; tap-to-skip works.
- [ ] Both buttons show idle/hover/pressed feedback; RETRY routes to re-match, CONTINUE routes to hub.
- [ ] No reward, no warm FX, no client balance mutation.
- [ ] Side-by-side with `DefeatScreenDesign.png`, positions within ±2% of panel dims, colors within stated ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**94 / 100.** The simplest of the five result screens (title + subtitle + two buttons, no reward economy), with unambiguous copy and a clear two-button layout, so structural + color fidelity is very high. Deductions: (a) the bespoke fallen-warrior background art and the tarnished crown crest must be authored/sourced to match (~ -4); (b) the exact desaturation/grade strength and ash-drift tuning are interpretive within the stated somber budget (~ -2). No structural ambiguity remains.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/DefeatScreenDesign.png`, 1915×821) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header has number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed stated.
- [x] Verbatim labels recorded (`DEFEAT`, `Your statue has fallen.`, `RETRY`, `CONTINUE`) — nothing invented.
- [x] Forensic hex ranges from the art.
- [x] Full ASCII tree + per-node Unity table.
- [x] Somber reveal timeline (no count-up/sparkle) with tap-to-skip.
- [x] §12 boundary honored (no balance mutation; Retry only requests a new match).
- [x] Negative rules + ≥95% acceptance + confidence rationale.
- [x] No code/asset/scene/prefab/gameplay changes, no commit.
