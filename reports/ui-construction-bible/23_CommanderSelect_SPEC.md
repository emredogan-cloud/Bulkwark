# BULWARK — UI CONSTRUCTION SPEC · 23 · Commander Select

Source: design/CommanderSelectDesign.png · 1672×941 (1.78:1) · Analysis-only forensic spec.

> Normalize the source 1672×941 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" values are on-canvas sizes at 1080-tall.

---

## A — SCREEN PURPOSE
The **Commander Select** meta screen — a symmetric **two-commander face-off** chooser. The player picks the
commander that leads their army:
- **LEFT:** Iron Pact — **WARDEN**, "Lord of Stone and Steel".
- **RIGHT:** Ashen Horde — **WARCHIEF**, "Scourge of the Burning Wastes".
- A central **VS** badge separates the two mirrored panels.

Each panel shows the faction crest, faction + commander names, full-body commander art, an **ACTIVE** ability
card and a **PASSIVE** ability card (each with name, description, and—for active—a cooldown line), a
**Commander Level** row with an XP progress bar, and a faction-colored **SELECT** button.

Purpose: communicate each commander's identity + ability kit and let the player commit a choice (server-auth).
Reached from: Main Menu / pre-match flow. Back: top-left chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** epic duel / "choose your champion" — a dark hall split into a **cool cobalt half (left)** and a
  **hot ember half (right)**, the two commanders facing each other across a glowing VS.
- **Split lighting:** left panel lit cool blue (Iron Pact), right panel lit warm orange/red (Ashen); a
  vertical seam of light/embers down the center behind VS.
- **Palette anchors:**
  - BG: near-black `#0a0b0f`; left half cooled toward `#0c1430`, right half warmed toward `#2a0f0c`.
  - Iron Pact chrome/SELECT: cobalt `#1d3a8a → #2b56c8 → #4f8bff`, cyan rim `#7fb0ff`, steel-gold frame.
  - Ashen chrome/SELECT: oxblood/ember `#5a1712 → #7a1f1a → #b5311f`, ember rim `#f0742c`.
  - Gold ornament (title, frames, VS, level numerals): `#6b5320 → #caa04a → #f0d27a`.
  - Parchment/stone ability cards: dark slate `#12141b` wells with gold trim; XP bar fill faction-tinted.
- **Hierarchy:** SELECT COMMANDER title → two commander portraits → VS → ability cards → SELECT buttons →
  currency chips/back.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
CommanderSelectScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_LeftCoolWash (Image, left half cobalt tint)
│  ├─ BG_RightEmberWash (Image, right half ember tint)
│  ├─ BG_CenterSeam (Image, vertical light/ember column behind VS)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ Header
│  │  ├─ Btn_Back (Button) [chevron] top-left
│  │  ├─ Lbl_Title "SELECT COMMANDER" (Text, serif gold) top-center
│  │  ├─ Lbl_Subtitle "Choose your commander. Each leads an army with unique abilities." (Text)
│  │  └─ CurrencyChips (HorizontalLayoutGroup) top-right
│  │     ├─ Chip_Gold   [gold-coin | "12,450" | Btn_Plus]
│  │     └─ Chip_Silver [silver-coin | "1,280" | Btn_Plus]
│  ├─ CommanderPanel_Left (Iron Pact / Warden) [framed]
│  │  ├─ Panel_Frame (Image, gold/cobalt ornate frame)
│  │  ├─ Crest_Faction (Image, Iron Pact lion crest) top-left of panel
│  │  ├─ Lbl_FactionName "IRON PACT" (Text)
│  │  ├─ Lbl_CommanderName "WARDEN" (Text, large serif)
│  │  ├─ Lbl_CommanderEpithet "Lord of Stone and Steel" (Text)
│  │  ├─ Art_Commander (Image, full-body Warden render)
│  │  ├─ AbilityCol (VerticalLayoutGroup)
│  │  │  ├─ AbilityCard_Active
│  │  │  │  ├─ Lbl_Kind "ACTIVE"
│  │  │  │  ├─ Icon_Ability (Image)
│  │  │  │  ├─ Lbl_AbilityName "RALLY"
│  │  │  │  ├─ Lbl_AbilityDesc "Inspire your troops, increasing their Attack and Defense for a short duration."
│  │  │  │  └─ Lbl_Cooldown "⏱ 60s Cooldown"
│  │  │  └─ AbilityCard_Passive
│  │  │     ├─ Lbl_Kind "PASSIVE"
│  │  │     ├─ Icon_Ability (Image)
│  │  │     ├─ Lbl_AbilityName "QUARTERMASTER"
│  │  │     └─ Lbl_AbilityDesc "Reduces resource cost of any upgrades and increases Silver income."
│  │  ├─ LevelRow
│  │  │  ├─ Lbl_LevelTag "COMMANDER LEVEL"
│  │  │  ├─ Badge_Level "12"
│  │  │  └─ XPBar (Slider) [Fill + Lbl "x / x,x00 XP"]
│  │  └─ Btn_Select_Left (Button, cobalt) "SELECT"
│  ├─ VSBadge (Image + Text "VS") center, between panels
│  └─ CommanderPanel_Right (Ashen Horde / Warchief) [mirror of Left]
│     ├─ Panel_Frame (gold/oxblood frame)
│     ├─ Crest_Faction (Ashen skull/horde crest) top-right
│     ├─ Lbl_FactionName "ASHEN HORDE"
│     ├─ Lbl_CommanderName "WARCHIEF"
│     ├─ Lbl_CommanderEpithet "Scourge of the Burning Wastes"
│     ├─ Art_Commander (Warchief render)
│     ├─ AbilityCol
│     │  ├─ AbilityCard_Active [ACTIVE | "WAR ROAR" | "Unleash a war roar, boosting your army's Attack and Spend while intimidating enemies." | "⏱ 60s Cooldown"]
│     │  └─ AbilityCard_Passive [PASSIVE | "BLOOD FORGE" | "Increases troop Health and reduces training time for melee units."]
│     ├─ LevelRow [ "COMMANDER LEVEL" | "9" | XPBar "1,450 / 4,000 XP" ]
│     └─ Btn_Select_Right (Button, oxblood) "SELECT"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | width grows |
| BG_LeftCoolWash | BG_FullBleed | 0 | Image | left-stretch (50%) | 0,.5 | — | full-bleed | follows seam |
| BG_RightEmberWash | BG_FullBleed | 1 | Image | right-stretch (50%) | 1,.5 | — | full-bleed | follows seam |
| BG_CenterSeam | BG_FullBleed | 2 | Image | mid-stretch (vertical) | .5,.5 | — | full-bleed | centered |
| BG_Vignette | BG_FullBleed | 3 | Image(mult) | stretch | .5,.5 | — | full-bleed | corners |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| Lbl_Title | Header | 1 | Text | top-center | .5,1 | center | inside | centered |
| Lbl_Subtitle | Header | 2 | Text | top-center (below title) | .5,1 | center | inside | centered |
| CurrencyChips | Header | 3 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-pinned |
| CommanderPanel_Left | SafeAreaRoot | 1 | Image(framed) | left-stretch | 0,.5 | — | inside | ~46% width, left-anchored |
| CommanderPanel_Right | SafeAreaRoot | 3 | Image(framed) | right-stretch | 1,.5 | — | inside | ~46% width, right-anchored |
| VSBadge | SafeAreaRoot | 2 | Image+Text | mid-center | .5,.5 | center | inside | centered, fixed |
| (per panel) Panel_Frame | Panel | 0 | Image | stretch | .5,.5 | — | inside | scales |
| Crest_Faction | Panel | 1 | Image | top-inner-corner | corner | — | inside | fixed |
| Lbl_FactionName | Panel | 2 | Text | top (beside crest) | side | side | inside | — |
| Lbl_CommanderName | Panel | 3 | Text | top (below faction) | side | side | inside | shrink to fit |
| Lbl_CommanderEpithet | Panel | 4 | Text | top (below name) | side | side | inside | — |
| Art_Commander | Panel | 5 | Image | panel-fill (behind cards) | .5,.5 | — | inside | aspect-locked, may bleed |
| AbilityCol | Panel | 6 | VerticalLayoutGroup | inner column | center | — | inside | fixed inner width |
| AbilityCard_Active/Passive | AbilityCol | 0,1 | Image(framed) | stretch-x | .5,.5 | — | inside | flex height |
| LevelRow | Panel | 7 | RectTransform | bottom (above SELECT) | .5,0 | center | inside | spans inner |
| Badge_Level | LevelRow | 0 | Image+Text | left | 0,.5 | center | inside | fixed |
| XPBar | LevelRow | 1 | Slider(no handle) | right of badge | 0,.5 | mid | inside | flex width |
| Btn_Select | Panel | 8 | Button | bottom-center | .5,0 | center | inside | wide CTA |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Header:** y 0.90 → 1.00.
- Btn_Back: center x 0.030, y 0.955; box ≈0.045w×0.085h.
- Lbl_Title "SELECT COMMANDER": centered x 0.50, baseline y 0.955; cap ≈ 0.055h (≈59px@1080); wide tracking.
- Lbl_Subtitle: centered x 0.50, y 0.905; ≈22px@1080.
- CurrencyChips: right edge x 0.972, y 0.955; two chips ≈0.115w×0.052h, gap 0.010w. **Only two** (Gold, Silver).

**Two panels (mirror):**
- Left panel region: x 0.018 → 0.475 (width ≈0.457w ≈1069px@1080), y 0.060 → 0.890.
- Right panel region: x 0.525 → 0.982 (same width), y 0.060 → 0.890.
- VS gap: x 0.475 → 0.525 (center column ≈0.05w).

**Within a panel (Left as reference; Right mirrors horizontally):**
- Crest_Faction: top-inner corner, x≈0.040, y≈0.860, Ø≈0.045w.
- Lbl_FactionName "IRON PACT": x≈0.090 (right of crest), y≈0.870; ≈26px@1080.
- Lbl_CommanderName "WARDEN": x≈0.090, baseline y≈0.835; large serif cap ≈0.052h (≈56px@1080).
- Lbl_CommanderEpithet: x≈0.090, y≈0.805; ≈20px italic.
- Art_Commander: occupies the panel's **left ~45%** (Warden render), region x 0.030→0.230, y 0.180→0.840;
  on Right panel the Warchief art occupies the panel's **right ~45%** (mirror). Art may bleed under cards.
- **AbilityCol** (inner cards, on the inner side of each panel, toward VS):
  - Left panel cards occupy x 0.250 → 0.460, two stacked cards.
  - Card_Active: y 0.620 → 0.790 (height ≈0.170h ≈184px@1080). Card_Passive: y 0.430 → 0.595.
  - Card internals: "ACTIVE"/"PASSIVE" kicker top-left (≈18px gold), ability icon ≈0.055w square at left,
    ability name (≈30px serif) to the right of icon, description (≈19px, 2–3 lines wrapped) below,
    cooldown line "⏱ 60s Cooldown" at card bottom (active only, ≈18px).
- **LevelRow:** y 0.330 → 0.380, spans inner area x 0.090→0.460.
  - Lbl_LevelTag "COMMANDER LEVEL" centered above the bar (y≈0.385, ≈18px tracked).
  - Badge_Level (circle Ø≈0.045w) at left containing "12"; XPBar to its right, x≈0.175→0.460, height ≈0.028h
    (≈30px), gold/cobalt fill, centered text "x / x,x00 XP".
- **Btn_Select:** y 0.250 → 0.320 (height ≈0.070h ≈76px), centered in panel inner width (x≈0.150→0.430),
  faction-colored, label "SELECT" ≈32px white.

**VS badge:** centered x 0.50, y≈0.560; Ø≈0.075w (≈175px@1080), gold ring with "VS" serif.

**Tablet (4:3):** panels widen toward center, VS gap shrinks; keep both panels symmetric, art may clip — keep
ability cards + SELECT fully visible (priority). **Ultrawide (21:9):** extra width widens the VS gutter; keep
panels anchored to their sides, BG washes fill. **Notch:** SafeAreaRoot insets; BG full-bleed under cutout;
back + chips clear of cutout. The seam/VS stays screen-center.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "SELECT COMMANDER" | serif display, regal | Black | UPPER | +8% | gold bevel + bloom + dark drop | 59 | `#f0d27a`→`#caa04a` |
| Subtitle | clean sans, light | Regular | Sentence | +4% | subtle | 22 | `#c9b27a` |
| Currency values | tabular sans | Semibold | — | tabular | 1px stroke | 30 | `#f4e9c8` |
| Faction name "IRON PACT"/"ASHEN HORDE" | strong sans | Bold | UPPER | +6% | drop-shadow | 26 | IP `#9fc0ff`; Ashen `#f0a070` |
| Commander name "WARDEN"/"WARCHIEF" | serif display | Black | UPPER | +5% | heavy gold bevel + bloom | 56 | `#f0d27a`→`#caa04a` |
| Epithet | serif italic, light | Regular | Title | +3% | subtle | 20 | `#cbb98a` |
| "ACTIVE"/"PASSIVE" kicker | sans | Semibold | UPPER | +10% | tinted | 18 | `#caa04a` |
| Ability name (RALLY/QUARTERMASTER/WAR ROAR/BLOOD FORGE) | serif/strong sans | Bold | UPPER | +4% | gold | 30 | `#f0d27a` |
| Ability description | sans, light | Regular | Sentence | +2% | — | 19 | `#cdd2da` |
| Cooldown "⏱ 60s Cooldown" | sans | Medium | Sentence | +2% | clock icon | 18 | `#b9c0c8` |
| "COMMANDER LEVEL" | sans | Medium | UPPER | +10% | dim | 18 | `#9a8a5a` |
| Level numeral (12/9) | tabular serif | Bold | — | — | gold glow | 36 | `#fff4d6` |
| XP "1,450 / 4,000 XP" | tabular sans | Semibold | — | tabular | over-bar | 20 | `#f0e8cf` |
| "SELECT" | strong sans | Bold | UPPER | +6% | white + dark stroke; faction glow | 32 | `#ffffff` |

---

## G — MATERIALS
- **BG:** charcoal base; left half a cool cobalt gradient wash, right half a warm ember wash; central seam =
  bright vertical light shaft (left side) blending to drifting embers (right side); strong corner vignette.
- **Panel frames:** ornate brushed gold/bronze (`#6b5320`→`#f0d27a`) with engraved filigree; the **left**
  frame carries cobalt inner glow `#4f8bff`, the **right** frame carries ember inner glow `#f0742c`.
- **Commander art:** painterly high-detail; Warden = polished steel armor + blue cloth, rim-lit cobalt;
  Warchief = blackened plate + ember/oxblood, rim-lit orange.
- **Ability cards:** dark slate wells `#12141b` with thin gold trim and a faint inner vignette; ability icon
  in a small recessed gold-rimmed frame.
- **XP bar:** dark groove `#1b1d24`; fill faction-tinted glassy (left cobalt, right ember/gold) with top
  specular; partial fill (~0.36 for Warchief = 1450/4000).
- **VS badge:** cast-gold ring with beveled "VS", soft bloom, slight metallic specular; sits over the seam.
- **SELECT buttons:** left = cobalt enamel (`#2b56c8`, lighter top edge, dark bottom, gold trim);
  right = oxblood/ember enamel (`#7a1f1a`→`#b5311f`, gold trim). Both rounded ~14px, white label, idle glow.

---

## H — COMPONENTS (states)
**CommanderPanel:** a selectable region; hover/focus brightens its frame glow and slightly lifts the art.
On a device with a single committed commander, the **currently-equipped** commander's panel shows an "EQUIPPED"
ribbon/check and its SELECT reads "SELECTED" (disabled/owned style).

**AbilityCard:**
- *idle:* shown as drawn.
- *hover:* gold trim brightens, icon glows; tooltip may expand full description.
- Active card always shows the cooldown line; passive card omits it.

**XPBar:** read-only progress; on level-up, animates fill to full then resets with level numeral tick.

**Btn_Select (per faction):**
- *idle:* faction enamel, white "SELECT", soft glow.
- *hover:* +10% brightness, glow grows, slight scale 1.02.
- *pressed:* scale 0.97, inner flash.
- *selected/owned:* label "SELECTED", check mark, locked-bright (this commander is active) — the other panel's
  SELECT returns to idle.
- *locked (commander not yet unlocked):* desaturated, 🔒, label "LOCKED" / "Level N to unlock"; tap → toast.
- *insufficient (if a cost gates it):* disabled grey; tap → shake + toast. (No explicit cost shown on the
  buttons in the mockup → default: free selection of an unlocked commander.)

**Back button:** standard; hover brighten, pressed scale.

---

## I — ANIMATION TIMELINE
**OnShow (~0.6s):**
- 0.00s BG washes + seam fade in (0.25s); embers begin drifting on the right.
- 0.05s Header (title scale 0.96→1.0 + fade, subtitle fade) 0.22s.
- 0.12s **Left panel slides in from left** (24px) + fade (0.25s ease-out); Warden art settles.
- 0.12s **Right panel slides in from right** (24px) + fade (0.25s) — symmetric.
- 0.30s VS badge pops (scale 0→1.15→1.0, 0.20s ease-out-back) + bloom flash.
- 0.34s Ability cards fade/slide up within each panel (0.18s, active then passive, 0.05s stagger).
- 0.42s XP bars fill left→right to current value (0.40s ease-out).
- 0.48s SELECT buttons fade in with a single glow sweep.

**OnHoverPanel:** frame glow +20%, art lifts 4px, ability icons shimmer (0.15s).

**OnSelect(faction):** chosen panel pulses faction glow, SELECT → "SELECTED" with a check pop (0.25s); other
panel's SELECT dims to idle; brief confirm bloom on the chosen crest. If selection commits + routes onward,
short fade-out (0.25s).

**Idle ambient:** VS badge faint pulse; embers loop on the right; cobalt motes loop on the left.

---

## J — PARTICLE & FX
- **Center seam:** continuous light-shaft shimmer (left) + rising embers (right) meeting at VS.
- **VS badge:** soft pulsing gold bloom; on hover of either panel, a faint spark toward that side.
- **Warden side:** slow cobalt dust motes + steel rim glints.
- **Warchief side:** ember sparks + heat-shimmer near the art base.
- **Ability icons:** subtle idle shimmer; active-ability icon has a faint cooldown-clock tick motif.
- **Select confirm:** faction-colored sparkle burst around the chosen SELECT + crest.
Budget pooled/capped; reduce ember/mote counts on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load both commanders' data (names, abilities, level, XP) read-only; mark currently-equipped if
  any; run enter timeline.
- **OnHoverPanel(side):** highlight; optional expand ability tooltips.
- **OnAbilityCard tap:** open detailed ability tooltip/sheet (full numbers if available).
- **OnSelect(side):** if commander unlocked → send "set active commander" request (server-auth); on success
  mark SELECTED + update other panel; if locked → toast unlock requirement; if gated by cost → confirm modal.
- **OnPlus (chip):** route to Store.
- **OnBack:** pop → previous screen (Main Menu / pre-match).
- **§12:** selection is a server-authoritative meta write request; UI never mutates state locally; no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn strings: Warden "Lord of Stone and Steel"; abilities RALLY (Active, 60s Cooldown,
  "Inspire your troops, increasing their Attack and Defense for a short duration.") and QUARTERMASTER (Passive,
  "Reduces resource cost of any upgrades and increases Silver income."); Warchief "Scourge of the Burning
  Wastes"; WAR ROAR (Active, 60s Cooldown, "Unleash a war roar, boosting your army's Attack and Spend while
  intimidating enemies.") and BLOOD FORGE (Passive, "Increases troop Health and reduces training time for
  melee units."). Warden Level 12; Warchief Level 9 (1,450 / 4,000 XP). Currencies 12,450 (gold) / 1,280
  (silver).
- Only **two** currency chips here (Gold + Silver) — do not add Gems.
- Do not add a third commander, extra ability slots, or a cost label not drawn.
- Keep the left=cobalt / right=ember split; do not swap faction colors.
- No portrait variant, no stick figures, no real brand text.
- No local balance/ECS mutation; SELECT is a request only.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Symmetric two-panel layout with a centered VS badge over a cool/warm split background.
2. Left = Iron Pact WARDEN (cobalt), Right = Ashen Horde WARCHIEF (ember), correct crests, names, epithets.
3. Each panel: ACTIVE + PASSIVE ability cards with the exact names/descriptions; active cards show "60s
   Cooldown".
4. Commander Level rows with badge (12 / 9) and XP bars (Warchief 1,450 / 4,000 XP), faction-tinted fill.
5. Faction-colored SELECT buttons (cobalt / oxblood), brightest interactive elements.
6. Header: SELECT COMMANDER title + subtitle centered, two currency chips top-right, back top-left.
7. Enter stagger, VS pop, XP fill, and select-confirm animations present.
8. Safe-area honored; BG full-bleed; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges.

## N — IMPLEMENTATION CONFIDENCE
**91/100.** Strong confidence: symmetric layout, ability cards, level/XP rows, SELECT buttons, split lighting,
and all visible copy are clearly legible. Minor uncertainty: exact ability-icon art, precise frame filigree,
whether selecting costs anything (none shown → assumed free for unlocked), and the exact Warden XP fraction
(its bar reads near-full but the numerator is partly obscured — Warchief's 1,450/4,000 is clear).

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 23 · Commander Select".
- [x] Fraction-based layout normalized to 2340×1080; symmetric panel math given.
- [x] Ability cards, level/XP bars, SELECT states (idle/hover/pressed/selected/owned/locked/disabled).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; nothing invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.
