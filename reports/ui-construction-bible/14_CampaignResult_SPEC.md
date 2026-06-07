# BULWARK — UI CONSTRUCTION SPEC · 14 · Campaign Result

Source: design/CampaignResultDesign.png · 1672×941 (≈1.78:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas; geometry is **fractions of 2340×1080**. This is a **POST-MATCH RESULT** screen shown when a **campaign level** is cleared. The clear verdict, the star count, the clear-time, and the reward values originate from ECS `MatchState`/level evaluation (read-only) + server-authoritative reward grant (display-only). **No code here** — construction spec only.

---

## A · SCREEN PURPOSE
The **Campaign Result** ("Level Cleared") screen rewards completing a campaign level and drives progression to the **next** level. It must (1) celebrate the clear with a **3-star rating** (stars earned vs available — the core campaign mastery loop), (2) show the **clear time** as a performance stat (green = good/under target), (3) present the level's **rewards** as two distinct currency slots (coins + gems), and (4) offer two routing actions — **NEXT LEVEL** (primary progression) and **REPLAY** (re-attempt for more stars / better time). Tone is warmer/lighter than the standard Victory — it uses a **parchment quest-scroll** aesthetic (campaign = story/journey), with blue Iron-Pact heraldic banners framing the scroll. Source of truth: ECS level-clear result (stars, time) + server reward grant.

State machine position: `Campaign Battle → Campaign Result → {NEXT LEVEL → next level intro/battle | REPLAY → same level}` (and an implicit back-to-Campaign-Map via REPLAY's sibling or system back).

---

## B · VISUAL DNA (screen-specific)
- **Aesthetic = WARM PARCHMENT QUEST-SCROLL on a dark map.** Unlike the obsidian Victory/Defeat cards, the campaign result panel is a **light parchment/cream scroll** (`#c8a96a`–`#e2c98a`) with a torn/aged paper texture, bordered by an ornate gold frame and flanked by **blue heraldic war-banners** (Iron Pact, with a gold lion/heraldic crest) draped over the top-left and top-right corners.
- **Hero element = the 3-STAR ARC.** Three large gold five-point stars sit on a curved gold banner ribbon spanning the top of the panel (breaking the top frame). The **center star is largest** and sits highest; the two side stars are slightly smaller/lower (classic 3-star arc). Earned stars are **filled hot gold** with bloom; unearned stars are **dark/hollow outlines** (`#23180f` with a gold rim). Source shows **2 of 3 filled** (left + center gold, right empty).
- **Background:** a dark, dimmed **campaign world map / battlefield** (the level just played), heavily vignetted to near-black at the edges (`#010101`), with faint troop silhouettes — pushed far back so the bright parchment scroll pops.
- **Performance stat = GREEN clear-time.** "Clear Time" with a gold clock/stopwatch icon and the time value in **vivid lime-green** (`#93bf37`) — green signals "good / under par." 
- **Rewards = two ornate slot cards.** Side-by-side dark slot tiles (gold-framed) on the lower parchment: left = **silver coins** stack icon + amount; right = **purple gem** cluster icon + amount.
- **CTAs:** **NEXT LEVEL** (royal-blue, primary progression) and **REPLAY** (dark stone/iron, secondary), separated by a small **gold sword + shield heraldic emblem** divider.
- **Mood:** triumphant but warm, adventurous, "chapter complete." Gold + parchment + Iron-Pact blue; green accent for the time; purple for gems. Brightest interactive object = NEXT LEVEL.

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
CampaignResultScreen (UiScreen root; CanvasGroup)
└── FullBleedRoot (stretch-stretch, ignores safe area)
    ├── BG_CampaignMap (Image — dimmed world-map/battlefield, full-bleed)
    ├── BG_Vignette (Image — heavy radial vignette → near-black edges)
    └── BG_DarkenScrim (Image — black ~0.45)
    SafeAreaRoot (RectTransform — Screen.safeArea inset)
    └── ResultPanel (Container; anchored center; ~0.58w × ~0.84h)
        ├── Banner_Left (Image — blue Iron-Pact war-banner w/ gold crest, draped top-left)
        ├── Banner_Right (Image — blue Iron-Pact war-banner, draped top-right)
        ├── Panel_Frame (Image — ornate gold beveled 9-slice frame)
        │   └── Panel_Parchment (Image — aged cream parchment fill, 9-slice/texture)
        ├── Stars_Group (anchored top-center, on a gold ribbon arc, breaks top frame)
        │   ├── Star_Ribbon (Image — curved gold banner the stars sit on)
        │   ├── Star_L (Image — star, filled gold OR hollow)
        │   ├── Star_R (Image — star, filled gold OR hollow)
        │   └── Star_C (Image — center star, largest, highest, filled gold OR hollow)
        ├── Title_Cleared (Text "LEVEL 7 CLEARED" — dark serif on parchment)
        ├── ClearTime_Group
        │   ├── ClearTime_Label (Text "Clear Time")
        │   ├── ClearTime_Divider (Image — thin gold filigree rule, optional)
        │   ├── Icon_Clock (Image — gold stopwatch)
        │   └── ClearTime_Value (Text "02:45" — GREEN)
        ├── Rewards_Group
        │   ├── Rewards_Label (Text "Rewards")
        │   ├── Reward_Slot_Coins (Container — gold-framed dark tile)
        │   │   ├── Slot_Frame (Image)
        │   │   ├── Icon_Coins (Image — silver coin stack)
        │   │   └── Coins_Value (Text "12,450")
        │   └── Reward_Slot_Gems (Container — gold-framed dark tile)
        │       ├── Slot_Frame (Image)
        │       ├── Icon_Gems (Image — purple gem cluster)
        │       └── Gems_Value (Text "60")
        └── Buttons_Group (anchored bottom)
            ├── CTA_NextLevel (Button — royal-blue, gold frame)
            │   ├── Next_Frame (Image)
            │   ├── Next_Fill (Image — blue gradient + inner blue rim)
            │   └── Next_Label (Text "NEXT LEVEL")
            ├── Buttons_Divider (Image — gold sword+shield heraldic emblem)
            └── CTA_Replay (Button — dark stone/iron, gold frame)
                ├── Replay_Frame (Image)
                ├── Replay_Fill (Image — dark stone gradient)
                └── Replay_Label (Text "REPLAY")
```

---

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| CampaignResultScreen | Canvas | — | UiScreen+CanvasGroup | stretch | .5,.5 | — | root | fade |
| FullBleedRoot | screen | 0 | RectTransform | stretch | .5,.5 | — | ignores | full-bleed |
| BG_CampaignMap | FullBleedRoot | 0 | Image | stretch | .5,.5 | center-crop | no | AspectFill |
| BG_Vignette | FullBleedRoot | 1 | Image | stretch | .5,.5 | center | no | radial, heavy |
| BG_DarkenScrim | FullBleedRoot | 2 | Image | stretch | .5,.5 | — | no | α0.45 |
| SafeAreaRoot | screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | applies | insets |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center | .5,.5 | center | yes | width=min(0.58·W, height-cap) |
| Banner_Left | ResultPanel | 0 | Image | top-left | 0,1 | — | yes | anchored to panel top-left corner, hangs down |
| Banner_Right | ResultPanel | 1 | Image | top-right | 1,1 | — | yes | mirror; top-right corner |
| Panel_Frame | ResultPanel | 2 | Image (9-slice) | stretch | .5,.5 | — | yes | 9-slice |
| Panel_Parchment | Panel_Frame | 0 | Image | stretch | .5,.5 | — | yes | inset; aged texture (tiled or large) |
| Stars_Group | ResultPanel | 3 | RectTransform | top/center | .5,.5 | center | yes | sits on top frame edge, breaks upward |
| Star_Ribbon | Stars_Group | 0 | Image | center/center | .5,.5 | center | yes | curved gold banner behind stars |
| Star_L / Star_R | Stars_Group | 1,2 | Image | center/center | .5,.5 | center | yes | side stars, smaller, lower |
| Star_C | Stars_Group | 3 | Image | center/center | .5,.5 | center | yes | center, largest, highest, top z |
| Title_Cleared | ResultPanel | 4 | Text (TMP) | top/center | .5,1 | center | yes | auto-size capped |
| ClearTime_Label | ResultPanel | 5 | Text (TMP) | top/center | .5,1 | center | yes | small caps |
| Icon_Clock | ResultPanel | 6 | Image | center/center | .5,.5 | left-of-value | yes | square |
| ClearTime_Value | ResultPanel | 7 | Text (TMP) | center/center | .5,.5 | center | yes | tabular, green |
| Rewards_Label | ResultPanel | 8 | Text (TMP) | center/center | .5,1 | center | yes | small caps |
| Rewards_Group | ResultPanel | 9 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | two equal slot tiles + gap |
| Reward_Slot_Coins / _Gems | Rewards_Group | 0,1 | RectTransform | — | .5,.5 | center | yes | each ≈0.30 of panel width |
| Slot_Frame | slot | 0 | Image (9-slice) | stretch | .5,.5 | — | yes | gold frame |
| Icon_Coins / Icon_Gems | slot | 1 | Image | top/center | .5,1 | center | yes | icon in upper area of tile |
| Coins_Value / Gems_Value | slot | 2 | Text (TMP) | bottom/center | .5,0 | center | yes | small currency icon + number row at tile bottom |
| Buttons_Group | ResultPanel | 10 | HorizontalLayoutGroup | bottom/center | .5,0 | middle-center | yes | NextLevel + emblem + Replay; pinned above bottom frame |
| CTA_NextLevel | Buttons_Group | 0 | Button | — | .5,.5 | center | yes | ≈0.40 panel width |
| Next_Frame/Fill/Label | CTA_NextLevel | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | blue CTA |
| Buttons_Divider | Buttons_Group | 1 | Image | center/center | .5,.5 | center | yes | gold sword+shield emblem |
| CTA_Replay | Buttons_Group | 2 | Button | — | .5,.5 | center | yes | ≈0.40 panel width |
| Replay_Frame/Fill/Label | CTA_Replay | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | dark-stone CTA |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
W=2340, H=1080, match HEIGHT.

**Background (full-bleed):** BG_CampaignMap 0,0→1,1 AspectFill; BG_Vignette heavy (corners→near-black `#010101`); BG_DarkenScrim α≈0.45.

**ResultPanel:** width ≈ **0.58·W ≈ 1357 px**, centered (x=0.50·W). Height ≈ **0.84·H ≈ 907 px**, vertically centered (sits a touch low to leave room for the star arc above). Aspect ≈ 1.50:1 (a wide landscape scroll). Cap: width = `min(0.58·W, 1.55·panelHeight)`.

**Banners:** Banner_Left anchored to panel top-left corner (pivot 0,1), width ≈ 0.10·panelW, hanging down ≈ 0.55·panelH; Banner_Right mirrored at top-right. They overlap the frame and extend slightly **outside** the parchment to the left/right (draped look).

**Star arc (Stars_Group):** spans the top of the panel, breaking the top frame edge. Star_Ribbon width ≈ 0.55·panelW centered at top. Center star (Star_C) at x=0.50·panelW, its center Y ≈ **-0.06·panelH** (above the frame top); diameter ≈ 0.16·panelH. Side stars at x≈0.38 and 0.62·panelW, center Y ≈ 0.00·panelH, diameter ≈ 0.13·panelH (slightly smaller + lower than center).

**Internal vertical rhythm (fractions of PANEL height, 0=panel top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Title "LEVEL 7 CLEARED" | 0.16 | 0.12 |
| "Clear Time" label | 0.30 | 0.05 |
| Clock icon + green time | 0.40 | 0.09 |
| "Rewards" label | 0.52 | 0.05 |
| Reward slot tiles | 0.66 | 0.22 |
| Buttons (Next/Replay) | 0.89 | 0.12 |

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.60, centered.
- Clear-time row centered; clock icon ≈ 0.06·panelW square just left of the value.
- Rewards row width ≈ 0.66; each slot tile ≈ 0.30·panelW × 0.20·panelH, gap ≈ 0.04 centered. Within a tile: icon in upper ~0.60, and a bottom row "small currency glyph + number" left-aligned-ish/centered.
- Buttons row width ≈ 0.82; NEXT LEVEL ≈ 0.40·panelW (left), REPLAY ≈ 0.40·panelW (right), gold emblem divider ≈ 0.06·panelW centered; button height ≈ 0.10·panelH.

**Notch/safe-area:** panel centered in safe rect; map bleeds under cutout. **Tablet (4:3):** the wide 1.5:1 scroll is height-fit; the cap keeps it ≤ ~0.7 of a narrow width. **Ultrawide:** more map revealed; banners/panel stay centered at absolute size.

---

## F · TYPOGRAPHY (per text)
Sizes px@1080. On **parchment**, titles/labels are **dark engraved serif** (not gold-on-dark) for contrast against the light scroll.

| Text | Content | Family / personality | Weight | Caps | Tracking | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|
| Title_Cleared | `LEVEL 7 CLEARED` | serif display, embossed-into-parchment | Bold | ALL-CAPS | +4% | letterpress: dark fill with a faint light bottom-edge highlight (debossed) | ~58–64 | dark warm brown `#3a2a12` | light highlight `#f0e2b8` α0.5 offset (0,1) below; soft inner shadow above |
| ClearTime_Label | `Clear Time` | serif small-caps, label | SemiBold | small-caps (Title-case shown) | +8% | engraved | ~26–28 | muted brown `#5a4424` | light highlight (0,1) |
| ClearTime_Value | `02:45` | semi-condensed, tabular | Bold | n/a | 0% | **green glow**, slight emboss | ~46–52 | vivid lime-green `#93bf37` (cores brighter `#b6e35a`) | dark green stroke `#2c4a0c` ~1.5px + soft green outer glow α0.4 + shadow `#000` α0.4 (0,2) |
| Rewards_Label | `Rewards` | serif small-caps, label | SemiBold | small-caps (Title-case shown) | +8% | engraved | ~26–28 | muted brown `#5a4424` | light highlight (0,1) |
| Coins_Value | `12,450` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp on dark tile | ~34–38 | warm white `#f3ead2` | shadow `#000` α0.6 (0,2) |
| Gems_Value | `60` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp on dark tile | ~34–38 | warm white `#f3ead2` | shadow α0.6 (0,2) |
| Next_Label | `NEXT LEVEL` | serif display, authoritative | Bold | ALL-CAPS | +6% | bright, faint blue glow | ~36–40 | warm white `#fffbf0` | dark-blue stroke `#0a1f44` ~2px + shadow α0.6 (0,3) |
| Replay_Label | `REPLAY` | serif display | Bold | ALL-CAPS | +6% | bright on dark stone | ~36–40 | warm white `#f3ecdc` | dark stroke `#1a140a` ~2px + shadow α0.6 (0,3) |

Note the **two text regimes**: dark-engraved serif on the parchment (title/labels/reward numbers sit on dark tiles so those are light) vs the green performance value vs the white CTA labels.

---

## G · MATERIALS
- **Parchment panel:** aged cream paper — base `#d6bd86`, lighter center `#e2c98a`, darker aged edges/stains `#b08a4e`–`#916330`; subtle fiber/grain texture; slightly uneven (torn-edge) border under the gold frame; soft inner shadow from the frame.
- **Gold frame + star ribbon + emblem + slot frames:** brushed antique gold — highlights `#f0d27a`, mid `#caa04a`, shadow `#6b5320`; engraved filigree; bloom on stars and star-ribbon highlights.
- **Stars (filled):** hot gold gradient `#ffe27a` (top) → `#e0a83a` (bottom) with a bright spec highlight and **bloom**; beveled 5-point with a slight inner facet line. **Stars (empty):** dark hollow `#23180f` interior with a thin gold rim `#a07a3a`, no bloom (clearly "not earned").
- **Blue banners:** Iron-Pact royal blue cloth `#1f3e78`–`#2b56c8` with stitched gold trim and a gold heraldic crest (lion/sigil) `#caa04a`; soft cloth folds + drop shadow onto the parchment; slight desaturation (background-tier).
- **Clock/stopwatch icon:** gold ring `#caa04a`, pale face `#e8dcc0`, dark hands; small.
- **Reward slot tiles:** dark recessed interior `#130f0a`–`#1c160e` (so light reward art/numbers pop) inside a gold bevel frame `#af874e`; inner shadow top.
- **Coin icon:** stacked silver/steel coins `#9aa0a8` highlights, `#5a6068` shadow, with a faint embossed sigil on the face. **Gem icon:** clustered violet crystals — highlights `#c89bff`, body `#7a3fd0`–`#5a2db0`, hot spec; magical inner glow.
- **NEXT LEVEL fill (blue):** royal-blue gradient `#0d2a66`→`#2f6fd6`, inner cobalt rim-glow `#5aa0ff` α≈0.45; satin sheen — the brightest button.
- **REPLAY fill (dark stone):** muted iron/stone gradient `#2a2620`→`#3a342a` with a faint top highlight; clearly secondary (matte, low glow). Gold frame matches.
- **Sword+shield emblem divider:** small gold heraldic crest (crossed/upright sword over a shield) with bevel + slight bloom, bridging the two buttons.
- **Bloom budget:** stars, the green time value, the gem cluster, and the NEXT LEVEL top edge carry bloom; parchment and REPLAY stay matte.

---

## H · COMPONENTS (states + feedback)
**Stars (display, animated):** each star reveals/pops in sequence (see §I). Earned = filled gold + pop + small sparkle burst + chime; unearned = remains a dark hollow outline (no pop, no sound). Optional: earned stars have a gentle persistent glint.

**Reward slots (display):** non-interactive; numbers may count-up (see §I). On reveal each tile does a brief scale-pop + the icon shimmers once.

**CTA_NextLevel (primary Button):**
- **Idle:** blue gradient, gold frame, inner blue rim, white label; subtle slow glow pulse (it is the encouraged progression).
- **Hover:** frame +8%, glow +25%, scale →1.03.
- **Pressed:** scale →0.97, fill darkens ~10%, glow flash, click SFX.
- **Disabled:** if there is **no next level** (last level of chapter/campaign), it becomes "WORLD MAP"/disabled-grey — but per the source the label is `NEXT LEVEL`; when unavailable, desaturate to grey `#3a342a` + label `#8a8278` + non-interactive (the implementation may instead swap to a "Map" action; that is an ADR/flow decision, not a redesign of this spec).
- **Confirm:** blue ring flash → next level intro/battle.

**CTA_Replay (secondary Button):**
- **Idle:** dark-stone fill, gold frame, white label; no glow pulse (secondary).
- **Hover:** frame +8%, slight fill lighten, scale →1.03.
- **Pressed:** scale →0.97, click SFX.
- **Disabled:** N/A (replay always available).
- **Confirm:** soft flash → re-enter the same level.

**Focus order:** default selected = **NEXT LEVEL**. Back/B → Campaign Map (system back; non-destructive). D-pad cycles Next ↔ Replay.

---

## I · ANIMATION TIMELINE (rich campaign reveal)
The campaign reveal is the richest of the result screens because of the **star rating ceremony**.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.30 | FullBleedRoot CG | fade 0→1 (map + scrim) | linear |
| 0.10 | 0.40 | ResultPanel | scale 0.9→1.0 + α 0→1 (parchment unfurls/scales in) | back-ease-out (overshoot 1.02→1.0) |
| 0.20 | 0.35 | Banner_Left/Right | unfurl/drop down from corners (slight cloth sway settle) | ease-out + small bounce |
| 0.40 | 0.30 | Title_Cleared | α 0→1 + slide up 10px | ease-out |
| 0.70 | 0.30 | Star_Ribbon | α 0→1 + scale 0.8→1.0 (the banner the stars land on) | ease-out |
| 0.85 | 0.30 | Star_L | drop+pop into filled gold + sparkle burst + chime (if earned) | back-ease-out |
| 1.10 | 0.30 | Star_C | drop+pop, LARGER pop + brighter sparkle + higher chime (center, earned) | back-ease-out |
| 1.35 | 0.30 | Star_R | reveal: if earned → pop gold+chime; **if unearned → settle as a dark hollow outline, no pop/sound** | ease-out |
| 1.65 | 0.30 | ClearTime_Label + icon | α 0→1 + slide up 8px | ease-out |
| 1.75 | 0.70 | ClearTime_Value | **count-up time toward `02:45`** OR type-in reveal with a green flash on settle | ease-out |
| 2.10 | 0.25 | Rewards_Label | α 0→1 | ease-out |
| 2.25 | 0.30 | Reward_Slot_Coins | tile scale-pop in + icon shimmer | back-ease-out |
| 2.30 | 0.80 | Coins_Value | **count-up 0 → 12,450** (tabular, tick SFX) | ease-out |
| 2.35 | 0.30 | Reward_Slot_Gems | tile scale-pop in + gem shimmer | back-ease-out |
| 2.40 | 0.60 | Gems_Value | **count-up 0 → 60** | ease-out |
| 2.70 | 0.30 | CTA_NextLevel | α 0→1 + scale 0.94→1.0 + glow ignite | back-ease-out |
| 2.75 | 0.30 | Buttons_Divider + CTA_Replay | α 0→1 + scale 0.94→1.0 | ease-out |
| 3.05 | loop | CTA_NextLevel glow | breathing pulse (period ~1.8 s) | sine in-out |
| 3.05 | loop | earned stars | gentle glint every ~3 s | linear |

**OnNextLevel (exit):** press dip 60 ms → blue ring flash 120 ms → panel α→0 + scale→0.96 (180 ms) + bg fade → route to next level. **OnReplay:** soft flash → re-enter same level. 
**Skip rule:** tap during reveal snaps all tweens (including star pops and count-ups) to end-state and enables both CTAs. The **star ceremony still resolves to the correct earned/unearned end-state** on skip.

---

## J · PARTICLE & FX (passive)
- **Star sparkle bursts:** one-shot gold sparkle per earned star at its pop; persistent gentle glint afterward.
- **Star-ribbon shimmer:** subtle gold shimmer traveling along the banner.
- **Green time glow:** soft persistent green bloom on the clear-time value.
- **Gem cluster glow:** soft violet inner glow + occasional spec twinkle on the gem icon.
- **Coin tile glint:** occasional silver glint sweep on the coin stack.
- **Dust motes:** faint warm dust drifting in front of the parchment (campaign/adventure ambiance), very low opacity.
- **NEXT LEVEL glow:** persistent breathing cobalt rim (see §I).

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload = `{ levelNumber:int, starsEarned:int (0–3), clearTimeSeconds:int, coins:int, gems:int, hasNextLevel:bool }` from ECS level evaluation + server reward grant (all **display-only**). Title formats `LEVEL {n} CLEARED`. Run the §I ceremony. The **number of filled stars is driven by `starsEarned`** — never hard-coded (source shows 2/3 as the example).
- **OnReward (count-ups):** coins → `12,450`, gems → `60`, clear-time settles to `02:45` (`MM:SS`). All display-only; the actual grant is server-authoritative (§12) — the UI never writes a balance.
- **OnNextLevel:** if `hasNextLevel`, route to the next level's intro/battle (or Campaign Map advanced to the next node). If not, the button is unavailable/relabeled per §H. Debounced.
- **OnReplay:** re-enter the same level (fresh attempt). Debounced. No balance mutation.
- **OnBack (B/Esc):** route to Campaign Map (non-destructive). 
- **Idempotency / re-entry:** settles to end-state immediately; no re-grant, no re-count, stars shown at their earned value.
- **No mutation:** issues only navigation + (Replay) a new-level request; never edits balance/progress client-side.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** hard-code 3 (or 2) filled stars — `starsEarned` (0–3) drives fills; unearned stars are dark hollow outlines. 
2. Do **not** put the title/labels in gold-on-dark — on parchment they are **dark engraved serif**; only the dark reward tiles carry light text. 
3. Do **not** color the clear-time anything but **green** (performance-good signal) as shown; do not invent a red/over-par variant unless the data supplies it (this spec records the green `02:45` case). 
4. Do **not** swap currency identities — left slot = silver coins, right slot = purple gems, in that order. 
5. Do **not** make REPLAY compete with NEXT LEVEL — REPLAY is dark/secondary/matte; NEXT LEVEL is the bright blue primary. 
6. Do **not** drop the blue Iron-Pact banners or the sword+shield emblem divider (signature campaign chrome). 
7. Do **not** put interactive content under the notch; only the map bleeds there. 
8. Do **not** mutate currency/progress client-side (§12); values are display-only. 
9. Do **not** stretch the gold frame/banner crest filigree (9-slice fixed). 
10. Do **not** skip the star ceremony resolution on tap-skip — it must still land on the correct earned/unearned end-state. 
11. Do **not** use portrait or assume width is stable; match HEIGHT, fraction-based.

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] A **light parchment scroll** panel (cream `#d6bd86`–`#e2c98a`, aged texture) with an ornate gold frame, flanked by **blue Iron-Pact war-banners** draped over the top-left and top-right corners, centered over a dimmed, heavily vignetted campaign map.
- [ ] A **3-star arc** on a gold ribbon breaks the top frame: center star largest+highest, two side stars smaller+lower; **2 filled hot-gold + 1 dark hollow** (driven by `starsEarned`).
- [ ] Title reads exactly `LEVEL 7 CLEARED` in dark engraved serif on the parchment.
- [ ] `Clear Time` label + gold stopwatch icon + `02:45` in **vivid lime-green** with a green glow.
- [ ] `Rewards` label + two gold-framed dark slot tiles: left **silver coin stack + `12,450`**, right **purple gem cluster + `60`**.
- [ ] Two buttons: **NEXT LEVEL** (royal-blue primary, glowing) and **REPLAY** (dark-stone secondary), separated by a **gold sword+shield emblem** divider.
- [ ] Reveal ceremony plays in order (panel → banners → title → star ribbon → L/C/R star pops → time count → reward tiles + count-ups → CTAs), with tap-to-skip that resolves the correct star end-state.
- [ ] NEXT LEVEL shows idle/hover/pressed + breathing glow and routes forward; REPLAY re-enters the level; back → Campaign Map.
- [ ] Background bleeds under the notch; content respects safe area; layout fraction-based, match-height.
- [ ] No client balance/progress mutation; star count and values are data-driven, not hard-coded.
- [ ] Side-by-side with `CampaignResultDesign.png`, positions within ±2% of panel dims and colors within stated ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**90 / 100.** Layout, copy, currencies, and the star/green-time/reward structure are all directly legible, so structural + color fidelity is high. Deductions: (a) the star **rating ceremony** (per-star pop/sparkle/chime + correct earned/unearned resolution under skip) is the most logic-rich reveal of the five and must be built carefully (~ -3); (b) bespoke art — the blue heraldic banners with gold crest, the sword+shield emblem divider, parchment texture, filled/hollow star art, coin & gem clusters — must be authored/sourced to match (~ -4); (c) the green clear-time presumably has thresholds (green = under target) that may need data the spec can't fully define from a single example, and the NEXT-LEVEL-when-no-next behavior is an ADR/flow detail (~ -3). No structural ambiguity in the shown state.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/CampaignResultDesign.png`, 1672×941) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header has number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed stated.
- [x] Verbatim labels recorded (`LEVEL 7 CLEARED`, `Clear Time`, `02:45`, `Rewards`, `12,450`, `60`, `NEXT LEVEL`, `REPLAY`) — nothing invented.
- [x] Forensic hex ranges from the art (parchment, green `#93bf37`, gem purple `#8c07cc`, gold).
- [x] Full ASCII tree + per-node Unity table.
- [x] Rich star-ceremony + count-up reveal timeline with tap-to-skip that resolves star state.
- [x] §12 boundary honored (display-only values; no balance/progress mutation).
- [x] Negative rules + ≥95% acceptance + confidence rationale.
- [x] No code/asset/scene/prefab/gameplay changes, no commit.
