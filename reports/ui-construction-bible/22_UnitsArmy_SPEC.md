# BULWARK — UI CONSTRUCTION SPEC · 22 · Units / Army

Source: design/UnitsArmyDesign.png · 1672×941 (1.78:1) · Analysis-only forensic spec.

> Normalize the source 1672×941 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). All positions below are given as **fractions of 2340×1080** so they scale 1:1 onto the
> mockup and onto any landscape device. Pixel values quoted "@1080" are the on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Army / Units collection + upgrade** meta screen. Three-zone layout:
1. **Header** (title "ARMY", currency chips).
2. **Left/center roster**: faction tabs (Iron Pact / Ashen Horde), a class-filter strip with a "Collected
   48/58" counter, and a **5-column × 2-row card grid** of owned/locked units (10 visible; scrollable for the
   full 58). Each card shows portrait, level badge, name, and an upgrade-material count bar.
3. **Right detail panel** (≈⅓ width): the selected unit's hero portrait, role line ("Frontline · Defensive"),
   **LEVEL 12/20**, three stat bars (Health / Damage / Speed with current value + green "+N" upgrade preview),
   an **Upgrade Progress** tier-node row (10 11 12 13 14), the primary **UPGRADE — 200 [gold]** CTA, and a
   "Hold to upgrade quickly" hint.
4. **Bottom utility bar**: **Rarity** · **Collection** · **Disenchant**.

Purpose: let the player browse the roster per faction, inspect a unit's stats, spend Gold to upgrade a unit's
level (advancing tier nodes), and disenchant duplicates for materials. Server-authoritative meta — the client
displays balances and requests an upgrade; it never mutates a balance locally.

Reached from: Main Menu rail (Units/Army). Back: top-left ornate corner (see §C — a recessed corner bracket;
the source crops it, treat as the standard Back affordance).

---

## B — VISUAL DNA (screen-specific, on top of global)
- **Mood:** armory / war-table. Dark obsidian field, a faint ruined-castle vista bleeding through the right
  detail panel's backdrop, warm gold ornamentation, cobalt selection energy.
- **Palette anchors:**
  - Page background: near-black charcoal `#0a0b0f → #14161e` vertical gradient, vignetted corners.
  - Gold chrome (title, frames, chips, tier nodes): `#6b5320` shadow → `#caa04a` mid → `#f0d27a` highlight.
  - Iron Pact / selection / CTA blue: `#1d3a8a → #2b56c8 → #4f8bff` with cyan rim `#7fb0ff`.
  - Ashen Horde tab (idle): oxblood `#5a1712 → #7a1f1a` with ember edge `#d8452b`.
  - Rarity tints on card bottom bars & class glints: common steel `#9aa3ad`, uncommon green `#4caf50`,
    rare blue `#3d7fe0`, epic violet `#9e6bf0`, legendary gold-orange `#f0a93a`.
  - Stat-bar fills: Health green `#5fd35a`, Damage amber `#f0a93a`, Speed cyan-blue `#4fb0ff`; the green
    "+N" preview text is `#7ff06a`.
- **Lighting:** focal glow behind the selected card and behind the detail portrait; gold rim-light on every
  bevel; soft inner shadow inside card wells; vignette darkens all four corners.
- **Hierarchy:** ARMY title → faction tabs → selected card (blue halo) → detail portrait → UPGRADE CTA →
  currency chips → filters → bottom bar.

---

## C — SCREEN DECOMPOSITION (ASCII node tree — every node)
```
ArmyScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed, extends under cutout)
│  ├─ BG_Gradient (Image, charcoal vertical gradient)
│  └─ BG_Vignette (Image, multiply, darkened corners)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter — ALL content below)
│  ├─ Header
│  │  ├─ Btn_Back (Button) — top-left ornate corner bracket
│  │  │  └─ Icon_BackChevron (Image)
│  │  ├─ Lbl_Title "ARMY" (Text, serif gold bevel, UPPERCASE)
│  │  └─ CurrencyChips (HorizontalLayoutGroup, anchored top-right)
│  │     ├─ Chip_Silver  [icon silver-coin | "12,450" | Btn_Plus]
│  │     ├─ Chip_Gold    [icon gold-coin   | "1,850"  | Btn_Plus]
│  │     └─ Chip_Gems    [icon violet-gem  | "2,340"  | Btn_Plus]
│  ├─ FactionTabs (HorizontalLayoutGroup, 2 tabs, center-left)
│  │  ├─ Tab_IronPact (Toggle, SELECTED) [crest | "IRON PACT"]
│  │  └─ Tab_AshenHorde (Toggle) [crest | "ASHEN HORDE"]
│  ├─ FilterStrip (HorizontalLayoutGroup, left) 
│  │  ├─ Filter_All (Toggle, SELECTED) "All"
│  │  ├─ Filter_Shield (Toggle, icon)         // class: frontline/tank
│  │  ├─ Filter_Sword (Toggle, icon)          // class: melee
│  │  ├─ Filter_Bow (Toggle, icon)            // class: ranged
│  │  ├─ Filter_Magic (Toggle, icon)          // class: caster
│  │  └─ Filter_Special (Toggle, icon)        // class: special
│  ├─ CollectedCounter (right-aligned over grid)
│  │  ├─ Lbl_CollectedTag "Collected"
│  │  └─ Lbl_CollectedVal "48/58"
│  ├─ RosterScroll (ScrollRect, vertical, masked viewport)
│  │  └─ RosterGrid (GridLayoutGroup, 5 cols × N rows)
│  │     ├─ UnitCard_Shieldman   (SELECTED)
│  │     ├─ UnitCard_Sentinel
│  │     ├─ UnitCard_IronArcher
│  │     ├─ UnitCard_HeavyGuard
│  │     ├─ UnitCard_RunicAdept
│  │     ├─ UnitCard_Miner
│  │     ├─ UnitCard_Warden
│  │     ├─ UnitCard_Crossbowman
│  │     ├─ UnitCard_Oathbreaker
│  │     └─ UnitCard_Flamecaller
│  │        (each UnitCard:)
│  │        ├─ Frame_Rarity (Image, rarity-tinted bevel)
│  │        ├─ Portrait (Image, unit render, masked)
│  │        ├─ SelectGlow (Image, blue halo — only when selected)
│  │        ├─ Badge_Level (Image circle + Text e.g. "12")
│  │        ├─ Lbl_Name (Text, UPPERCASE small)
│  │        └─ CountBar (Slider-style) [Fill + Lbl "125 / 300" + ▲ up-arrow if upgradable]
│  ├─ DetailPanel (Image parchment/stone framed, right ⅓)
│  │  ├─ Detail_BG (Image, dark stone + faint vista)
│  │  ├─ Detail_Crest (Image, small unit class crest top-right)
│  │  ├─ Detail_Name "SHIELDMAN" (Text, serif gold)
│  │  ├─ Detail_Role "Frontline • Defensive" (Text)
│  │  ├─ Detail_Portrait (Image, large unit render)
│  │  │  ├─ Btn_PrevUnit (Button, left chevron)
│  │  │  └─ Btn_NextUnit (Button, right chevron)
│  │  ├─ Detail_LevelRow [ "LEVEL " + "12" + "/20" + Btn_Info(ⓘ) ]
│  │  ├─ StatRows (VerticalLayoutGroup)
│  │  │  ├─ Stat_Health  [♥ icon | bar Fill | "3,240" | "+180" green]
│  │  │  ├─ Stat_Damage  [⚔ icon | bar Fill | "210"   | "+12"  green]
│  │  │  └─ Stat_Speed   [» icon | bar Fill | "86"    | "+4"   green]
│  │  ├─ UpgradeProgress
│  │  │  ├─ Lbl_UpgradeProgress "UPGRADE PROGRESS"
│  │  │  └─ TierNodeRow (HorizontalLayoutGroup) [10][11][(12 current)][13][14] + connectors
│  │  ├─ Btn_Upgrade (Button, blue CTA) "UPGRADE — 200" + [gold icon]
│  │  └─ Lbl_HoldHint "Hold to upgrade quickly"
│  └─ BottomBar (HorizontalLayoutGroup, 3 buttons, bottom-center under grid)
│     ├─ Btn_Rarity   [icon | "Rarity"]
│     ├─ Btn_Collection [icon | "Collection"]
│     └─ Btn_Disenchant [icon | "Disenchant"]  (violet accent)
```

---

## D — UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor preset | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| ArmyScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | .5,.5 | — | — | fills canvas |
| BG_FullBleed | ArmyScreen | 0 | Image | stretch-all | .5,.5 | — | **ignores** safe area (full-bleed) | width grows on ultrawide |
| BG_Gradient | BG_FullBleed | 0 | Image | stretch-all | .5,.5 | — | full-bleed | tile-safe |
| BG_Vignette | BG_FullBleed | 1 | Image (mult) | stretch-all | .5,.5 | — | full-bleed | corners scale |
| SafeAreaRoot | ArmyScreen | 1 | RectTransform + SafeAreaFitter | stretch-all | .5,.5 | — | **defines** inset | insets to Screen.safeArea |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans safe width |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | fixed size, pinned TL |
| Lbl_Title | Header | 1 | Text | top-left (offset right of back) | 0,1 | left | inside | left-anchored |
| CurrencyChips | Header | 2 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-anchored, fixed |
| Chip_* | CurrencyChips | 0..2 | RectTransform (Image+Text+Button) | — | — | mid | inside | fixed widths |
| FactionTabs | SafeAreaRoot | 1 | HorizontalLayoutGroup | top-left (below header) | 0,1 | left | inside | left cluster |
| Tab_IronPact / Tab_AshenHorde | FactionTabs | 0,1 | Toggle (ToggleGroup) | — | — | mid | inside | equal width pill |
| FilterStrip | SafeAreaRoot | 2 | HorizontalLayoutGroup | top-left (below tabs) | 0,1 | left | inside | left cluster |
| Filter_* | FilterStrip | 0..5 | Toggle (ToggleGroup) | — | — | mid | inside | square chips |
| CollectedCounter | SafeAreaRoot | 3 | RectTransform | top (right of grid area) | 1,1 | right | inside | right-anchored |
| RosterScroll | SafeAreaRoot | 4 | ScrollRect (vertical only) | top-left | 0,1 | — | inside | width = left ⅔; height to bottom bar |
| RosterGrid | RosterScroll/Viewport/Content | 0 | GridLayoutGroup | top-stretch | .5,1 | upper-left | inside | 5 fixed cols, rows grow |
| UnitCard_* | RosterGrid | 0..n | RectTransform (Button) | grid cell | .5,.5 | — | inside | uniform cell |
| Frame_Rarity | UnitCard | 0 | Image | stretch-all | .5,.5 | — | inside | scales w/ card |
| Portrait | UnitCard | 1 | Image (masked) | stretch (inset) | .5,.5 | — | inside | scales w/ card |
| SelectGlow | UnitCard | 2 | Image | stretch-all (overscan) | .5,.5 | — | inside | only on selected |
| Badge_Level | UnitCard | 3 | Image+Text | bottom-left | 0,0 | mid | inside | fixed |
| Lbl_Name | UnitCard | 4 | Text | bottom-stretch (above bar) | .5,0 | center | inside | shrinks to fit |
| CountBar | UnitCard | 5 | Slider(no handle)+Text | bottom-stretch | .5,0 | mid | inside | full card width |
| DetailPanel | SafeAreaRoot | 5 | Image (framed) | right-stretch (vertical) | 1,.5 | — | inside | fixed fraction width, full height |
| Detail_BG | DetailPanel | 0 | Image | stretch-all | .5,.5 | — | inside | fills panel |
| Detail_Crest | DetailPanel | 1 | Image | top-right | 1,1 | — | inside | fixed |
| Detail_Name | DetailPanel | 2 | Text | top-center | .5,1 | center | inside | shrink to fit |
| Detail_Role | DetailPanel | 3 | Text | top-center (below name) | .5,1 | center | inside | — |
| Detail_Portrait | DetailPanel | 4 | Image | top-center (below role) | .5,1 | center | inside | aspect-locked |
| Btn_PrevUnit / Btn_NextUnit | Detail_Portrait | 0,1 | Button | mid-left / mid-right | .5,.5 | — | inside | fixed |
| Detail_LevelRow | DetailPanel | 5 | RectTransform | center-stretch | .5,.5 | center | inside | — |
| StatRows | DetailPanel | 6 | VerticalLayoutGroup | center-stretch | .5,.5 | — | inside | full panel width (inset) |
| Stat_* | StatRows | 0..2 | RectTransform (icon+Slider+2 Text) | stretch-x | .5,.5 | — | inside | bar flexes |
| UpgradeProgress | DetailPanel | 7 | RectTransform | center-stretch | .5,.5 | center | inside | — |
| TierNodeRow | UpgradeProgress | 1 | HorizontalLayoutGroup | center-stretch | .5,.5 | center | inside | 5 nodes + connectors |
| Btn_Upgrade | DetailPanel | 8 | Button | bottom-stretch | .5,0 | center | inside | wide CTA |
| Lbl_HoldHint | DetailPanel | 9 | Text | bottom-center | .5,0 | center | inside | — |
| BottomBar | SafeAreaRoot | 6 | HorizontalLayoutGroup | bottom-left (under grid) | .5,0 | center | inside | spans left ⅔ |
| Btn_Rarity/Collection/Disenchant | BottomBar | 0..2 | Button | — | — | mid | inside | equal width |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Global safe-area margin:** treat ~3.0% (≈70px) inset on all sides as the design's working frame.

**Header band:** y from 0.93 → 1.00 (top 7%).
- Btn_Back: x-center 0.030, y-center 0.965; box ≈ 0.045w × 0.085h (≈105×92@1080).
- Lbl_Title "ARMY": left edge x≈0.075, baseline y≈0.955; cap-height ≈ 0.052h (≈56px@1080).
- CurrencyChips: right edge x=0.970, y-center 0.965. Each chip ≈ 0.115w × 0.052h (≈269×56@1080); gap 0.010w.
  Order L→R: Silver, Gold, Gems. Each = [round icon Ø≈0.030w][value text][+ button Ø≈0.026w].

**Faction tabs:** band y 0.855 → 0.915.
- Two pills starting x≈0.020. Each pill ≈ 0.165w × 0.052h (≈386×56@1080), gap 0.008w.
- Iron Pact pill spans x 0.020→0.185 (selected — blue fill). Ashen pill x 0.193→0.358.

**Filter strip:** band y 0.790 → 0.845.
- "All" pill x≈0.020, ≈0.050w wide. Then 5 square class chips ≈0.038w each (≈89px), gap 0.008w, running to
  x≈0.40. (The chips read as: shield, crossed-swords, bow, magic-bolt, special/star.)
- CollectedCounter: anchored to grid right edge (x≈0.625), top y≈0.840; "Collected" tag small over "48/58".

**Roster grid (left ⅔ of screen):**
- Grid region: x 0.018 → 0.630 (width 0.612w ≈ 1432px@1080), y 0.130 → 0.775 (height 0.645h ≈ 697px@1080).
- **5 columns × 2 visible rows.** Cell size: with 4 inter-column gaps of 0.012w (≈28px) →
  cellW = (0.612 − 4·0.012)/5 = 0.1128w ≈ **264px@1080**; cellH ≈ 0.300h ≈ **324px@1080** (portrait taller
  than wide, ~0.81:1). Row gap 0.030h (≈32px). GridLayoutGroup: cell (264,324), spacing (28,32),
  startCorner Upper-Left, constraint Fixed-Column-Count = 5.
- **Within each card:**
  - Frame inset 0 (frame = card bounds). Portrait inset ≈ 0.10·cell on all sides.
  - Badge_Level: bottom-left, circle Ø ≈ 0.34·cellW (≈90px), inset ~6px from BL corner; number centered.
  - Lbl_Name: horizontal band at ~78% card height, centered, cap ≈ 18px@1080 UPPERCASE.
  - CountBar: bottom strip, full card width inset 6px, height ≈ 0.075·cellH (≈24px); centered text
    "125 / 300"; small ▲ up-arrow glyph at right end when upgrade material available (Shieldman shows ▲).

**Detail panel (right ⅓):**
- Panel region: x 0.660 → 0.982 (width 0.322w ≈ 753px@1080), y 0.060 → 0.945 (full height inside safe).
- Detail_Name baseline y≈0.905; cap ≈ 0.040h (≈43px). Detail_Role y≈0.875.
- Detail_Portrait: centered, region y 0.640→0.860, width ≈ 0.20w; Prev/Next chevrons at its vertical mid,
  pinned to panel inner left/right (x≈0.675 / x≈0.965).
- Detail_LevelRow: y≈0.610; "LEVEL" + big "12" + "/20" + ⓘ.
- StatRows: y 0.460 → 0.585, 3 rows evenly spaced (row pitch ≈0.042h). Each row: icon (Ø≈0.022w) at left,
  bar from x≈0.700→0.870 (track), value right-aligned x≈0.905, "+N" green to its right x≈0.945.
- UpgradeProgress: label y≈0.430; TierNodeRow y≈0.370 → 0.405. 5 nodes across panel width: node Ø ≈ 0.038w
  (≈89px); current node (12) slightly larger (×1.15) and brighter; connectors are short gold bars between.
  Node labels under each: 10,11,12,13,14 (12 highlighted gold).
- Btn_Upgrade: y 0.250→0.320 (height ≈0.070h ≈76px), spans panel inner width (x 0.672→0.972). Text
  "UPGRADE — 200" + gold-coin icon, centered.
- Lbl_HoldHint: y≈0.225, centered, small italic.

**Bottom bar:** band y 0.025 → 0.085, spans grid width (x 0.018→0.630). 3 equal buttons ≈0.195w each, gaps
0.012w. Order: Rarity, Collection, Disenchant.

**Tablet (4:3 ≈ 1440×1080):** width compresses to 1.0 match-height baseline; grid may drop to 4 columns if
cellW < 240 (keep ≥4); detail panel keeps fixed 0.322 fraction → on 4:3 it occupies more relative width, so
clamp detail panel to max 380px and reflow grid. **Ultrawide (21:9 / 2.33:1 ≈ 2520×1080):** extra width pads
the gutter between grid and detail; keep grid left-anchored, detail right-anchored, BG full-bleed fills.
**Notch (landscape side cutout):** SafeAreaRoot insets; full-bleed BG slides under; back button and chips never
enter the cutout.

---

## F — TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| "ARMY" title | serif display, regal | Bold/Black | UPPER | +6% (wide) | — | gold bevel + soft outer bloom + 2px dark drop | 56 | grad `#f0d27a`→`#caa04a` |
| Currency values "12,450/1,850/2,340" | clean tabular sans | Semibold | — | tabular | — | thin 1px dark stroke | 30 | `#f4e9c8` |
| Tab labels "IRON PACT"/"ASHEN HORDE" | strong sans-serif | Bold | UPPER | +4% | — | drop-shadow; selected = bright | 26 | sel `#eaf2ff`; idle `#c9b27a` |
| "All" + filter labels | sans | Medium | Title | 0 | — | — | 22 | `#d9c79a` |
| "Collected" tag | sans | Regular | Title | +2% | — | — | 20 | `#b9c0c8` |
| "48/58" value | tabular sans | Semibold | — | tabular | — | — | 26 | `#f4e9c8` |
| Card name (e.g. "SHIELDMAN") | condensed sans | Semibold | UPPER | +3% | — | 1px dark stroke | 18 | `#e8e2cf` |
| Card count "125 / 300" | tabular sans | Medium | — | tabular | — | — | 17 | full `#e8f0c0`, low `#cf6b5a` |
| Card level badge "12" | sans | Bold | — | — | — | dark stroke for legibility | 24 | `#fff4d6` |
| Detail name "SHIELDMAN" | serif display | Bold | UPPER | +5% | — | gold bevel + bloom | 43 | `#f0d27a`→`#caa04a` |
| Detail role "Frontline • Defensive" | sans, light | Regular | Title | +6% | — | subtle | 22 | `#c9b27a` |
| "LEVEL" / "12" / "/20" | serif label / tabular | Semibold/Bold | UPPER | — | — | "12" glows gold | LEVEL 24, 12 → 40, /20 → 24 | `#f0d27a` / `#fff4d6` / `#9a8a5a` |
| Stat labels (icons only, no text) | — | — | — | — | — | — | — | — |
| Stat values "3,240 / 210 / 86" | tabular sans | Semibold | — | tabular | — | — | 26 | `#f4f0e2` |
| Stat preview "+180 / +12 / +4" | tabular sans | Bold | — | tabular | — | green glow | 22 | `#7ff06a` |
| "UPGRADE PROGRESS" | sans | Medium | UPPER | +8% (tracked) | — | dim | 18 | `#9a8a5a` |
| Tier node numbers "10..14" | tabular sans | Semibold | — | — | — | current=gold glow | 20 | cur `#f0d27a`, others `#9aa3ad` |
| CTA "UPGRADE — 200" | strong sans | Bold | UPPER | +4% | — | white text + dark stroke; gold icon | 32 | `#ffffff` |
| "Hold to upgrade quickly" | sans italic | Light | Title | +2% | — | subtle | 18 | `#9aa3ad` |
| Bottom bar labels | sans | Medium | Title | +2% | — | icon-led | 22 | `#d9c79a`; Disenchant `#c8a6ff` |

Font: TMP SDF Roboto for body/numbers; serif SDF (Trajan/Cinzel-like) for ARMY + SHIELDMAN + LEVEL display.

---

## G — MATERIALS
- **Page BG:** flat charcoal gradient `#0a0b0f`(top) → `#14161e`(bottom), 0 roughness (matte), radial vignette
  to `#05060a` corners (≈45% darken). No reflection.
- **Gold frames/title/chips/tier nodes:** brushed antique gold — base `#a8842f`, shadow `#6b5320`, highlight
  `#f0d27a`; medium-high specular along top bevel; light micro-scratch wear on long bars; 1px inner dark line
  separating bevel from fill; faint bloom on highlights only.
- **Card frames:** dark cast-iron well `#16181f` with a rarity-tinted inner edge glow (see palette). Selected
  card: full **cobalt halo** `#4f8bff` (outer soft glow ~12px) + brighter gold frame; portrait sits in a
  recessed shadowed well (inner shadow ~6px).
- **Tabs:** Iron Pact selected = polished cobalt enamel `#2b56c8` with cyan rim + gold crest; Ashen idle =
  matte oxblood `#7a1f1a` with ember rim, slightly desaturated (unselected).
- **Stat bars:** dark groove track `#1b1d24`; fills are glassy with a top specular line — Health `#5fd35a`,
  Damage `#f0a93a`, Speed `#4fb0ff`; the "+N" portion can render as a brighter lighter cap on the fill.
- **Detail panel:** dark stone slab `#101218` with a faint desaturated castle vista (≈20% opacity) and a gold
  filigree frame; soft inner vignette.
- **CTA (Upgrade):** cobalt button `#2f5fd6` with lighter top edge `#5f8bff` and dark bottom `#1b367f`,
  rounded ~14px, gold thin trim, white label; gold-coin icon inset; subtle bloom on idle.
- **Disenchant accent:** violet `#9e6bf0` glyph/edge to read as "convert to materials".
- **Bloom budget:** restrained — title, selected halo, CTA, tier-current node, stat-fill speculars only.

---

## H — COMPONENTS (states + feedback)
**UnitCard** (Button):
- *idle (owned):* rarity-tinted frame, lit portrait, level badge, count bar with fill.
- *hover/focus (gamepad):* gold frame brightens +15%, slight scale 1.03, soft glow.
- *pressed:* scale 0.97, inner darken.
- *selected:* persistent cobalt halo + brighter gold frame + portrait fully lit (the SHIELDMAN card state).
- *upgradable:* small ▲ green up-arrow on the count bar (material ≥ threshold) — Shieldman, Miner show it.
- *locked/unowned:* portrait silhouetted dark, frame desaturated, level badge replaced by 🔒, no count bar
  (or "0/—"); tapping opens "how to unlock" (the 58 roster includes locked entries beyond the 10 shown).
- *maxed:* badge shows max level, count bar hidden or "MAX".

**FactionTab** (Toggle, single-select group):
- *selected:* filled faction color, bright label, crest lit, slight raised look.
- *unselected:* darkened/desaturated, hover brightens; click swaps the whole roster + detail to that faction.

**FilterChip** (Toggle group, "All" default):
- *selected:* gold ring + lit icon; *idle:* dim; filters the grid by class. Multi vs single = single (radio).

**TierNode** (Upgrade Progress):
- *passed (≤ current):* filled gold, check-style.
- *current (12):* enlarged, bright gold glow ring.
- *future (>current):* dim steel outline.
- Connectors between nodes fill gold up to current.

**Stat row:** animated fill bar; on hovering a future upgrade, the "+N" green segment highlights to preview
the post-upgrade value.

**CTA_Upgrade** (Button):
- *idle:* cobalt, white "UPGRADE — 200" + gold coin, soft glow.
- *hover:* +10% brightness, glow grows.
- *pressed:* scale 0.97 + brief inner flash; **hold** = repeat-upgrade ("Hold to upgrade quickly" — long-press
  ramps repeat rate).
- *disabled (insufficient gold or maxed):* desaturated grey-blue, label dim, lock/coin-grey; tap → shake +
  "Not enough Gold" toast (routes to Store via chip +).
- *affordable feedback:* coin count on chip flashes when spent.

**BottomBar buttons** (Rarity / Collection / Disenchant): standard icon+label buttons; Disenchant opens a
multi-select disenchant flow (duplicates → materials); violet accent.

---

## I — ANIMATION TIMELINE
**OnShow (screen enter, ~0.45s):**
- 0.00s BG + vignette fade in (0.20s).
- 0.05s Header slides down 16px + fade (0.20s, ease-out).
- 0.10s Faction tabs + filters fade/slide from left (0.18s).
- 0.12s **Card grid staggers in**: each card scale 0.92→1.0 + fade, 0.018s stagger left-to-right, top row then
  bottom row; ease-out-back (slight overshoot 1.02). Full grid settled by ~0.40s.
- 0.20s Detail panel slides in from right 24px + fade (0.22s).
- 0.30s **Stat bars fill** left→right (0.45s ease-out) to current value; the "+N" preview segment pulses once.
- 0.34s Tier nodes pop sequentially up to current (0.04s each), current node glow-ring scales 1.0→1.15→1.05.
- 0.38s CTA fades in with a single bloom sweep.

**OnSelectCard (~0.25s):** previously-selected halo fades (0.12s); new card halo expands 0→1 (0.15s ease-out);
detail panel content cross-fades (name/role/portrait swap 0.18s); stat bars re-fill to the new unit (0.40s);
tier nodes re-pop.

**OnUpgrade (~0.6s):** CTA flash → coin chip decrements with a -200 tick; affected stat bar(s) extend by the
"+N" amount with a green flash (0.35s); LEVEL number ticks 12→13 with a scale pop; current tier node advances
(connector fills, node slides) ; small gold sparkle burst at the node. If hold-repeat, loop at ~0.25s cadence.

**OnFactionSwap (~0.35s):** grid cards fade/slide out left (0.15s), new faction's cards stagger in (0.20s);
tabs swap fill color.

---

## J — PARTICLE & FX
- **Selected card:** soft cobalt halo + 2–3 slow rising spark motes; faint pulsing (period ~2s).
- **Rarity shimmer:** legendary/epic cards get a slow diagonal specular sweep across the frame (~6s loop);
  rarer = brighter sweep. Common = none.
- **Detail portrait:** gentle volumetric backlight + 1–2 dust motes drifting.
- **Tier current node:** continuous soft gold glow-ring pulse.
- **CTA idle:** subtle bloom breathing; on press, a quick gold spark ring.
- **Upgrade success:** gold sparkle burst at the stat bar + node; brief screen-edge warm flash (very subtle).
- **Currency spend:** small coin-poof at the chip.
Budget: pooled, capped; disable rarity sweeps on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load roster for current faction (default Iron Pact); request balances (read-only); select first
  owned unit (Shieldman) → populate detail; run enter timeline.
- **OnFactionTab(faction):** swap roster + reset filter to All + reselect first owned + refresh counter.
- **OnFilter(class):** filter grid; keep selection if still visible else select first match.
- **OnSelectCard(unit):** populate detail (name, role, portrait, level, stats, tier row, upgrade cost,
  upgradable arrows); play select anim.
- **OnPrev/NextUnit:** move selection within current filtered list (wraps); same as select.
- **OnUpgrade (tap):** if affordable & not maxed → send upgrade request (server-auth); on success apply stat
  deltas, level++, tier advance, decrement Gold; else disabled-shake + toast. **Hold:** repeat while held and
  affordable.
- **OnInfo (ⓘ):** open unit stat-breakdown tooltip/sheet.
- **OnPlus (currency chip):** route to Store (Gold/Gems purchase).
- **OnRarity:** open rarity legend/sort. **OnCollection:** collection/progress view. **OnDisenchant:** open
  multi-select disenchant → materials (server-auth).
- **OnBack:** pop screen → Main Menu.
- **§12:** all upgrade/disenchant are server-authoritative requests; UI only displays and requests. No local
  balance mutation, no ECS write.

---

## L — NEGATIVE RULES
- Do **not** invent unit stats, costs, or rarities beyond what's drawn (Shieldman Lv12/20, Health 3,240 +180,
  Damage 210 +12, Speed 86 +4, Upgrade 200, count 125/300, Collected 48/58, currencies 12,450/1,850/2,340).
- Do **not** add a 6th currency, a search box, sort dropdowns, or extra tabs not shown.
- Do **not** add stick figures or real brand text. No portrait-orientation variant.
- Do **not** change grid to anything but 5-wide on the reference canvas (rows may scroll).
- Do **not** let UI mutate balances/ECS; upgrades are requests only.
- Do **not** recolor faction identity (Iron Pact cobalt, Ashen oxblood) or rarity tints.
- Canon note: BULWARK canon = 12 units; this screen draws **58 collected-of total** and 10 visible cards →
  spec exactly as drawn and flag the roster-count discrepancy to design (do not "correct" it here).

---

## M — ACCEPTANCE CRITERIA (≥95% fidelity)
1. 5×2 visible card grid with correct cell aspect, gaps, rarity frames, level badges, count bars, and the ▲
   upgradable arrow on the right units.
2. Shieldman card shows the **cobalt selected halo**; detail panel mirrors Shieldman exactly.
3. Header: ARMY title left, three currency chips (silver/gold/gems) with +buttons, correct values, right-pinned.
4. Faction tabs (Iron Pact selected blue / Ashen oxblood) + filter strip (All + 5 class chips) + "Collected
   48/58".
5. Detail: name, role, portrait with prev/next chevrons, LEVEL 12/20 + ⓘ, three stat bars with exact values
   and green "+N" previews, Upgrade Progress nodes 10–14 (12 current/highlighted), UPGRADE — 200 CTA, hold hint.
6. Bottom bar Rarity/Collection/Disenchant.
7. Stat-bar fill, card stagger, select cross-fade, and upgrade tick animations present.
8. Safe-area inset honored; BG full-bleed; layout holds on 4:3, 19.5:9, 21:9, and notched landscape.
9. All hex/typography within the ranges in F/G; CTA is the brightest interactive element after the selected card.

## N — IMPLEMENTATION CONFIDENCE
**90/100.** High confidence on layout, the 5×2 grid, detail panel, stats, tier nodes, CTA, and color identity
(all clearly legible). Minor uncertainty: exact pixel radii/bevel widths, the precise class icons in the filter
strip (read as shield/swords/bow/magic/special), whether locked roster entries render identically to the 10
shown, and the exact "+N" fill rendering (separate cap vs ghost segment). The 58-unit roster vs 12-unit canon
is a design discrepancy, not a fidelity blocker.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present and substantive, in order.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 22 · Units / Army".
- [x] Fraction-based layout normalized to 2340×1080; grid cell sizes + gaps given.
- [x] ScrollRect + GridLayoutGroup specified for the roster.
- [x] States for cards/tabs/filters/tier-nodes/CTA (idle/hover/pressed/disabled/selected/owned/locked).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; no invented numbers; discrepancy flagged.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — no code/assets/scenes; only this .md written.
