# BULWARK — UI CONSTRUCTION SPEC · 24 · Profile

Source: design/ProfileScreenDesign.png · 1783×882 (2.02:1) · Analysis-only forensic spec.

> Normalize the source 1783×882 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" = on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Player Profile** meta screen. Three columns under a centered "PROFILE" title:
1. **Left vertical tab rail:** Overview (selected) · Heroes · Match History · Stats · Achievements ·
   Customization, with the faction crest anchored at the rail's bottom.
2. **Center identity column:** large framed avatar portrait, a **level badge "45"** on the portrait base, an
   **XP bar** ("x,xxx / x,000 XP"), the player name **THALRION**, and a clan badge **SILVERWARDENS — "Knights
   of the Realm"**.
3. **Right content (Overview tab):** three top **stat blocks** — **BATTLES 1,248**, **WINS 842**, **WIN RATE
   67.5%** — an **Equipped** row of **5 cosmetic slots** (Galvanhelm, Lionheart Plate, Dawnbreaker, Royal
   Cloak, Warden's Banner — each "Epic"), a **Title** panel ("REALM CHAMPION"), and a footer with **Player ID
   #7A4B3C9E** (copy button) and **Joined 2024-01-15**.

Purpose: present player identity, progression, lifetime stats, equipped cosmetics, and account metadata; the
tab rail switches the right content. Reached from: Main Menu / avatar tap. Back: top-left double-chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** prestige character sheet / hall-of-honor. Dark stone hall, warm gold framing, a heroic backlit
  avatar; calm and stately (less "combat", more "trophy room").
- **Palette anchors:**
  - BG: obsidian `#0a0b0f → #14161e`, faint stone-hall texture, vignette.
  - Gold chrome (title, frames, stat icons, dividers): `#6b5320 → #caa04a → #f0d27a`.
  - Iron Pact cobalt accents (selected tab, clan crest, XP fill): `#2b56c8 → #4f8bff`.
  - Slot wells / panels: dark slate `#101218 → #161922` with gold trim.
  - Rarity "Epic" label/edge: violet `#9e6bf0`.
  - Win-rate / positive numerals: warm gold `#f0d27a`; neutral text `#cdd2da`.
- **Hierarchy:** PROFILE title → avatar + name → stat blocks → equipped slots → title/ID footer → tab rail.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
ProfileScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_StoneHall (Image, dark stone texture)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ Header
│  │  ├─ Btn_Back (Button) [double-chevron] top-left
│  │  └─ Lbl_Title "PROFILE" (Text, serif gold) top-center
│  ├─ TabRail (VerticalLayoutGroup, left)  [framed rail]
│  │  ├─ Tab_Overview (Toggle, SELECTED) [icon | "Overview"]
│  │  ├─ Tab_Heroes (Toggle) [icon | "Heroes"]
│  │  ├─ Tab_MatchHistory (Toggle) [icon | "Match History"]
│  │  ├─ Tab_Stats (Toggle) [icon | "Stats"]
│  │  ├─ Tab_Achievements (Toggle) [icon | "Achievements"]
│  │  ├─ Tab_Customization (Toggle) [icon | "Customization"]
│  │  └─ Crest_Faction (Image, Iron Pact crest at rail bottom)
│  ├─ IdentityColumn (center)
│  │  ├─ Avatar_Frame (Image, ornate portrait frame)
│  │  │  ├─ Avatar_Portrait (Image, hero render, masked)
│  │  │  └─ Badge_Level (Image circle + Text "45") at frame base
│  │  ├─ XPBar (Slider) [Fill + Lbl "x,xxx / x,000 XP"]
│  │  ├─ Lbl_PlayerName "THALRION" (Text, large serif gold)
│  │  └─ ClanBadge
│  │     ├─ Clan_Crest (Image)
│  │     ├─ Lbl_ClanName "SILVERWARDENS"
│  │     └─ Lbl_ClanMotto "Knights of the Realm"
│  └─ ContentPane_Overview (right)
│     ├─ StatBlocks (HorizontalLayoutGroup, 3)
│     │  ├─ Stat_Battles  [icon crossed-swords | "BATTLES" | "1,248"]
│     │  ├─ Stat_Wins     [icon laurel/wreath  | "WINS"    | "842"]
│     │  └─ Stat_WinRate  [icon banner         | "WIN RATE"| "67.5%"]
│     ├─ EquippedSection
│     │  ├─ Lbl_EquippedTitle "Equipped"
│     │  └─ EquippedSlots (HorizontalLayoutGroup, 5)
│     │     ├─ Slot_Helm    [item img | "Galvanhelm"     | "Epic"]
│     │     ├─ Slot_Armor   [item img | "Lionheart Plate"| "Epic"]
│     │     ├─ Slot_Weapon  [item img | "Dawnbreaker"    | "Epic"]
│     │     ├─ Slot_Cloak   [item img | "Royal Cloak"    | "Epic"]
│     │     └─ Slot_Banner  [item img | "Warden's Banner"| "Epic"]
│     └─ FooterRow
│        ├─ TitlePanel [ Lbl "Title" | Value "REALM CHAMPION" ]
│        ├─ Lbl_PlayerIDTag "Player ID"
│        ├─ Lbl_PlayerIDVal "#7A4B3C9E"
│        ├─ Btn_CopyID (Button, copy icon)
│        ├─ Lbl_JoinedTag "Joined"
│        └─ Lbl_JoinedVal "2024-01-15"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | grows |
| BG_StoneHall | BG_FullBleed | 0 | Image | stretch | .5,.5 | — | full-bleed | tile-safe |
| BG_Vignette | BG_FullBleed | 1 | Image(mult) | stretch | .5,.5 | — | full-bleed | corners |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| Lbl_Title | Header | 1 | Text | top-center | .5,1 | center | inside | centered |
| TabRail | SafeAreaRoot | 1 | VerticalLayoutGroup | left-stretch | 0,.5 | top | inside | fixed width, full height |
| Tab_* | TabRail | 0..5 | Toggle (group) | stretch-x | .5,1 | left | inside | uniform row |
| Crest_Faction | TabRail | 6 | Image | bottom-center | .5,0 | center | inside | fixed |
| IdentityColumn | SafeAreaRoot | 2 | RectTransform | left-center (right of rail) | .5,.5 | center | inside | fixed fraction |
| Avatar_Frame | IdentityColumn | 0 | Image | top-center | .5,1 | center | inside | aspect-locked |
| Avatar_Portrait | Avatar_Frame | 0 | Image(masked) | stretch(inset) | .5,.5 | — | inside | scales |
| Badge_Level | Avatar_Frame | 1 | Image+Text | bottom-center | .5,0 | center | inside | fixed |
| XPBar | IdentityColumn | 1 | Slider(no handle) | center (below frame) | .5,1 | mid | inside | column width |
| Lbl_PlayerName | IdentityColumn | 2 | Text | center (below XP) | .5,1 | center | inside | shrink to fit |
| ClanBadge | IdentityColumn | 3 | RectTransform | center (below name) | .5,1 | center | inside | — |
| ContentPane_Overview | SafeAreaRoot | 3 | RectTransform | right-stretch | 1,.5 | — | inside | flex width, right-anchored |
| StatBlocks | ContentPane | 0 | HorizontalLayoutGroup | top-stretch | .5,1 | center | inside | 3 equal blocks |
| Stat_* | StatBlocks | 0..2 | RectTransform(icon+2 Text) | — | — | center | inside | equal |
| EquippedSection | ContentPane | 1 | RectTransform | center-stretch | .5,.5 | — | inside | full width |
| Lbl_EquippedTitle | EquippedSection | 0 | Text | top-center | .5,1 | center | inside | — |
| EquippedSlots | EquippedSection | 1 | HorizontalLayoutGroup | center-stretch | .5,.5 | center | inside | 5 equal slots |
| Slot_* | EquippedSlots | 0..4 | RectTransform(img+2 Text) | — | — | center | inside | uniform |
| FooterRow | ContentPane | 2 | RectTransform | bottom-stretch | .5,0 | center | inside | spans |
| TitlePanel | FooterRow | 0 | RectTransform | bottom-center | .5,0 | center | inside | centered panel |
| Lbl_PlayerID* / Btn_CopyID | FooterRow | 1..3 | Text/Button | bottom-right | 1,0 | right | inside | right cluster |
| Lbl_Joined* | FooterRow | 4,5 | Text | bottom-right (below ID) | 1,0 | right | inside | right cluster |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Header:** y 0.91 → 1.00.
- Btn_Back: center x 0.030, y 0.955; box ≈0.045w×0.085h (double-chevron «).
- Lbl_Title "PROFILE": centered x 0.50, baseline y 0.955; cap ≈ 0.055h (≈59px@1080), wide tracking.

**Left tab rail:** x 0.016 → 0.215 (width ≈0.199w ≈466px@1080), y 0.060 → 0.900.
- 6 tab rows top-aligned, each ≈0.085h tall (≈92px), pitch ≈0.105h. Row = icon (Ø≈0.030w) at left +
  label. Selected (Overview) = cobalt-filled pill + lit icon + small left active-marker.
- Crest_Faction at rail bottom, centered, Ø≈0.085w, y≈0.085.

**Center identity column:** x 0.230 → 0.520 (width ≈0.290w ≈679px@1080), vertically centered.
- Avatar_Frame: centered x 0.375, top y≈0.880; frame ≈0.185w × 0.420h (≈433×454@1080), ornate gold portrait
  frame (slightly arched top). Portrait inset ~0.07·frame.
- Badge_Level "45": at frame base center, x 0.375, y≈0.470; circle Ø≈0.060w (≈140px), gold ring, "45" inside.
- XPBar: x 0.270→0.480, y≈0.430 (height ≈0.024h ≈26px), cobalt fill, centered "x,xxx / x,000 XP".
- Lbl_PlayerName "THALRION": centered x 0.375, baseline y≈0.380; large serif cap ≈0.055h (≈59px).
- ClanBadge: centered x 0.375, y≈0.300; pill with clan crest (left) + "SILVERWARDENS" (≈26px) over
  "Knights of the Realm" (≈18px).

**Right content pane (Overview):** x 0.535 → 0.984 (width ≈0.449w ≈1050px@1080), y 0.075 → 0.900.
- **StatBlocks:** top band y 0.770 → 0.890. Three equal blocks across the pane (each ≈0.140w, gap 0.014w).
  Each block: gold icon top (Ø≈0.040w), label ("BATTLES"/"WINS"/"WIN RATE") ≈18px, big value
  ("1,248"/"842"/"67.5%") ≈40px below.
- **EquippedSection:** "Equipped" title centered at y≈0.700 (≈24px). Slots band y 0.500 → 0.680.
  Five square slots across the pane: slotW = (0.449 − 4·0.014)/5 = 0.0786w ≈ **184px@1080**, square-ish
  (height ≈0.150h ≈162px for the art well) + label rows below. Each slot: item render in a gold-rimmed dark
  well, item name beneath (≈18px, e.g. "Galvanhelm"), rarity "Epic" (≈16px violet) beneath that.
- **FooterRow:** y 0.110 → 0.260.
  - TitlePanel: centered horizontally in the pane, y≈0.230; a wide parchment/gold pill with small "Title"
    kicker above and value "REALM CHAMPION" (≈30px serif gold) inside; pill ≈0.30w × 0.075h.
  - Player ID cluster: right-aligned, y≈0.230 (or split): "Player ID" tag (≈18px) + "#7A4B3C9E" value
    (≈26px mono) + copy icon button; below it "Joined" tag + "2024-01-15" value (≈22px). (Reads as a
    right-side two-line metadata block beside the centered Title pill.)

**Tablet (4:3):** pane narrows — equipped slots may wrap to 5-across still (priority) but stat blocks can stack
2+1 if width < threshold; keep rail fixed width. **Ultrawide (21:9):** extra width pads between identity column
and content pane; keep rail left, content right. **Notch:** SafeAreaRoot insets; BG full-bleed under cutout;
back/title and rail clear of cutout.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "PROFILE" title | serif display | Black | UPPER | +8% | gold bevel + bloom + drop | 59 | `#f0d27a`→`#caa04a` |
| Tab labels | sans | Medium | Title | +2% | selected bright | 24 | sel `#eaf2ff`; idle `#c9b27a` |
| Level "45" | tabular serif | Black | — | — | gold glow + dark stroke | 48 | `#fff4d6` |
| XP "x,xxx / x,000 XP" | tabular sans | Semibold | — | tabular | over-bar | 20 | `#f0e8cf` |
| "THALRION" name | serif display | Bold | UPPER | +5% | heavy gold bevel + bloom | 59 | `#f0d27a`→`#caa04a` |
| "SILVERWARDENS" | strong sans | Semibold | UPPER | +4% | drop-shadow | 26 | `#9fc0ff` |
| "Knights of the Realm" | serif italic, light | Regular | Title | +3% | subtle | 18 | `#cbb98a` |
| Stat labels (BATTLES/WINS/WIN RATE) | sans | Medium | UPPER | +8% | dim | 18 | `#9a8a5a` |
| Stat values (1,248/842/67.5%) | tabular serif | Bold | — | tabular | gold glow | 40 | `#f4e9c8` |
| "Equipped" | sans | Semibold | Title | +4% | gold | 24 | `#caa04a` |
| Slot item names | sans | Medium | Title | +2% | — | 18 | `#e8e2cf` |
| "Epic" rarity | sans | Semibold | Title | +6% | violet glow | 16 | `#c8a6ff` |
| "Title" kicker | sans | Regular | Title | +4% | dim | 18 | `#9a8a5a` |
| "REALM CHAMPION" | serif display | Bold | UPPER | +5% | gold bevel + bloom | 30 | `#f0d27a` |
| "Player ID" / "Joined" tags | sans | Regular | Title | +2% | dim | 18 | `#9a8a5a` |
| "#7A4B3C9E" | mono/tabular sans | Semibold | UPPER | tabular | — | 26 | `#cdd2da` |
| "2024-01-15" | tabular sans | Medium | — | tabular | — | 22 | `#cdd2da` |

Font: serif SDF for PROFILE/THALRION/REALM CHAMPION + stat numerals; Roboto SDF for body/IDs/dates.

---

## G — MATERIALS
- **BG:** dark stone-hall texture, low contrast, strong vignette; subtle warm key from upper area.
- **Gold chrome:** brushed antique gold (`#6b5320`→`#f0d27a`), engraved filigree on the avatar frame and
  panel edges; medium specular on top bevels; light wear.
- **Avatar frame:** ornate cast-gold portrait frame (arched top), inner dark recess with the hero render
  rim-lit gold/cobalt; soft inner shadow around the portrait.
- **Level badge:** gold ring medallion, dark center, "45" embossed; faint bloom.
- **XP bar:** dark groove `#1b1d24`, cobalt glassy fill with top specular.
- **Clan badge:** small gold-rimmed crest + dark pill background.
- **Stat blocks:** flat dark panels (or borderless over BG) with gold icons; thin gold divider lines between.
- **Equipped slots:** dark slate wells `#101218` with gold rim; item renders (helm/plate/sword/cloak/banner)
  lit; a thin **violet (Epic)** edge/underglow on each slot to signal rarity.
- **Title pill:** parchment/dark with gold frame; "REALM CHAMPION" embossed gold.
- **Copy button:** small gold/steel icon button.

---

## H — COMPONENTS (states)
**TabRail tab (Toggle, single-select):**
- *selected (Overview):* cobalt-filled pill, bright label, lit icon, left active marker.
- *idle:* transparent/dark, muted gold label; *hover:* label brightens + faint pill; click switches the right
  content pane (Overview/Heroes/Match History/Stats/Achievements/Customization).
- *disabled (if a tab is locked):* greyed + 🔒.

**EquippedSlot:**
- *filled:* item render + name + "Epic" rarity edge.
- *hover:* gold rim brightens, slight scale 1.03, tooltip with item details.
- *pressed:* scale 0.97 → opens item detail / equip-swap (routes to Customization).
- *empty:* dark well + "+" / "Empty" placeholder.

**Btn_CopyID:** *idle* gold icon; *pressed* flash + "Copied!" micro-toast; copies "#7A4B3C9E".

**StatBlock:** static display; on tab=Stats, may expand into a fuller breakdown (out of this Overview scope).

**Back button:** standard hover/pressed.

---

## I — ANIMATION TIMELINE
**OnShow (~0.55s):**
- 0.00s BG + vignette fade (0.20s).
- 0.05s Header (title scale 0.96→1.0 + fade) 0.22s.
- 0.10s Tab rail slides in from left + fade (0.20s); tabs cascade 0.03s each.
- 0.14s Avatar frame scales 0.94→1.0 + fade (0.25s ease-out-back); portrait backlight ramps.
- 0.24s Level badge pops (0.18s), XP bar fills left→right to current (0.40s).
- 0.28s Name + clan badge fade up (0.18s).
- 0.30s **Stat blocks** count-up tick to their values (1,248 / 842 / 67.5%) over 0.5s; icons glint.
- 0.36s **Equipped slots** stagger in (scale 0.92→1.0 + fade, 0.04s each, left→right).
- 0.46s Footer (Title pill + ID/Joined) fade in (0.18s).

**OnTabSwitch (~0.25s):** right content pane cross-fades/slides 16px (0.22s); selected pill marker slides to
the new tab.

**OnHoverSlot:** gold rim +20%, scale 1.03, tooltip fade (0.12s).
**OnCopyID:** copy-icon flash + "Copied!" toast pop (0.3s) then fade.

---

## J — PARTICLE & FX
- **Avatar:** gentle volumetric backlight + 1–2 dust motes; faint gold rim shimmer on the frame.
- **Level badge:** soft gold glow pulse.
- **Equipped slots:** subtle violet (Epic) underglow shimmer; on hover a brief sparkle.
- **Title pill:** faint gold bloom on "REALM CHAMPION".
- **Stat icons:** single glint on enter.
Budget pooled/capped; reduce motes on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load profile (name, level, XP, clan, stats, equipped cosmetics, title, ID, join date) read-only;
  default tab Overview; run enter timeline + stat count-up.
- **OnTab(tab):** swap right content pane (Heroes → roster summary; Match History → recent matches; Stats →
  full stats; Achievements → grid; Customization → cosmetic equip).
- **OnSlot tap:** open cosmetic detail / route to Customization to swap.
- **OnCopyID:** copy "#7A4B3C9E" to clipboard + toast.
- **OnBack:** pop → Main Menu.
- **§12:** all data read-only/server-authoritative; cosmetic equip is a meta request via Customization; no
  local mutation, no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn values: name THALRION, level 45, clan SILVERWARDENS / "Knights of the Realm",
  BATTLES 1,248, WINS 842, WIN RATE 67.5%, equipped = Galvanhelm / Lionheart Plate / Dawnbreaker / Royal Cloak
  / Warden's Banner (all Epic), Title "REALM CHAMPION", Player ID #7A4B3C9E, Joined 2024-01-15.
- Exactly **6** tabs in the rail and **5** equipped slots — do not add/remove.
- No currency chips are drawn on this screen → do **not** add them.
- Do not invent additional stats on the Overview pane beyond the three blocks.
- No portrait variant, no stick figures, no real brand text.
- Equip changes are meta requests only; no local/ECS mutation.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Left rail with 6 tabs (Overview selected, cobalt) + faction crest at bottom.
2. Center: ornate avatar frame, level "45" badge, XP bar, "THALRION", clan badge SILVERWARDENS / "Knights of
   the Realm".
3. Right: 3 stat blocks (BATTLES 1,248 / WINS 842 / WIN RATE 67.5%) with correct icons.
4. Equipped row of 5 slots with the exact item names + "Epic" rarity edges.
5. Footer: Title "REALM CHAMPION" pill, Player ID #7A4B3C9E + copy button, Joined 2024-01-15.
6. Header: PROFILE title centered, back double-chevron top-left.
7. Enter stagger, stat count-up, XP fill, slot stagger, tab cross-fade present.
8. Safe-area honored; BG full-bleed; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges.

## N — IMPLEMENTATION CONFIDENCE
**92/100.** Strong confidence: 3-column structure, tab rail, avatar/level/XP, stat blocks, 5 equipped slots,
and all visible copy are clearly legible. Minor uncertainty: exact avatar XP numerator (partly obscured under
the badge — render as the drawn "x,xxx / x,000 XP" placeholder), precise stat-block icon art, and the exact
arrangement of the footer (Title pill centered vs left, ID/Joined right) — the spec uses the most consistent
reading.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 24 · Profile".
- [x] Fraction-based layout normalized to 2340×1080; rail/slots/stat-block math given.
- [x] Tab rail + equipped slots + copy button states (idle/hover/pressed/selected/empty/disabled).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; nothing invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.
