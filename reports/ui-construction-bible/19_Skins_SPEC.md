# BULWARK — UI CONSTRUCTION SPEC · 19 · Skins

Source: design/SkinsScreenDesign.png · 1910×823 (2.32:1) · Analysis-only forensic spec.

> **Inherits the shared shop chrome from `17_Store_SPEC.md`** (Back top-left, tab bar SPELLS·SKINS·CHESTS·
> STORE top-center, Gems+Gold chips top-right). This file details the **Skins-specific** body: a left vertical
> **skin-thumbnail rail**, a center **hero preview** of the equipped/selected skin, a bottom **set-pieces row**
> (themed pickaxes with gem prices), and a right **detail panel** listing **stat bonuses** with an **EQUIP
> ALL** button. **SKINS** tab selected. **ADR-flagged** (skins carry gameplay stat modifiers).

---

## A · SCREEN PURPOSE
The **Skins** shop tab is the cosmetic-set collection. The player picks a skin theme from a vertical rail (5
entries), sees a full-body hero preview center-stage, reviews the **set bonuses** in the right panel, browses
the matching **set pieces** (e.g., tiered pickaxes) along the bottom — each with a gem price — and applies the
whole look with **EQUIP ALL**. Shown selected: **LEAF SET** (a nature/green-leaf themed character), with the
green Leaf pickaxe piece selected (300 gems, checkmarked).

**ADR note (Section L):** the right panel shows **gameplay stat modifiers** (+30% Build Speed, +20% Extra
Health, +25% Mining Speed, +15% Unit Regen). This collides with "skins are visual-only cosmetics" + "gems
never buy power" → **requires an ADR** (recommended resolution: make modifiers Gold-only / cosmetic-only or
standardized in ranked). The screen is spec'd **exactly as drawn**; the ADR governs implementation, not this
visual spec.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** a darkened armory / war-camp at dusk — torch-lit posts, a distant battlefield horizon in the
  detail panel art. Moodier and darker than Store/Spells; the glowing **green Leaf hero** is the focal light.
- **Palette anchors:** for LEAF SET, emerald/lime green (`#3fd24a`–`#9bff7a`) on the character + bonus icons;
  gem-violet price chips; gold ornate frames; **EQUIP ALL is cobalt/royal-blue** (Iron-Pact CTA blue),
  gold-edged — distinct from the violet BUY of the Spells tab (equip = apply, not premium-spend).
- **Lighting:** strong rim + inner glow on the selected hero (green), torch flicker on the bg posts, vignette,
  bloom on the glowing skin and the selected piece's frame.
- **Background:** full-bleed dusk armory/battlefield; bleeds under cutout.
- **Hierarchy:** detail bonuses + EQUIP ALL (right, the decision) ⟷ hero preview (center focal) → set-pieces
  row (choices) → thumbnail rail (set switch) → chrome.

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
SkinsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, dusk armory/battlefield)  [OUTSIDE safe area]
   │  └─ Bg_Vignette (Image)
   ├─ TopChrome  ── [SHARED — see 17 §C/§D]
   │  ├─ BackButton (Button: Plate + gold Arrow)
   │  ├─ TabBar (ToggleGroup ShopTabs)
   │  │  ├─ Tab_Spells (Toggle) → Icon + "SPELLS"
   │  │  ├─ Tab_Skins  (Toggle, SELECTED, gold-lit) → Icon(helm) + "SKINS"
   │  │  ├─ Tab_Chests (Toggle) → Icon + "CHESTS"
   │  │  └─ Tab_Store  (Toggle) → Icon + "STORE"
   │  └─ CurrencyChips
   │     ├─ GemChip  → violet gem + "1726" + green "+"
   │     └─ GoldChip → silver coin + "48570" + green "+"
   ├─ SkinRail (left, gold-framed container + VerticalLayoutGroup or ScrollRect, 5 thumbs)
   │  ├─ Rail_Frame (Image, ornate gold border container)
   │  ├─ Skin_Thumb_1 (Toggle, SELECTED, gold-lit) → portrait: green-leaf hooded figure
   │  ├─ Skin_Thumb_2 (Toggle) → red/orange knight-helm figure
   │  ├─ Skin_Thumb_3 (Toggle) → dark green hooded figure
   │  ├─ Skin_Thumb_4 (Toggle) → purple wizard-hat figure
   │  └─ Skin_Thumb_5 (Toggle) → white skull/bone figure
   ├─ HeroPreview (center stage)
   │  ├─ Hero_Pedestal (Image, faint ground/light disc)
   │  └─ Hero_Model (Image, full-body LEAF SET character, glowing green, holding leaf-blades)
   ├─ SetPieces_Row (bottom, gold-framed container + HorizontalLayoutGroup, 4 pieces)
   │  ├─ Piece_1 (Button) → stone/plain pickaxe + PriceChip(gem "100")
   │  ├─ Piece_2 (Button, SELECTED, green frame + ✓) → green Leaf pickaxe + PriceChip(gem "300")
   │  ├─ Piece_3 (Button) → blue/ice pickaxe + PriceChip(gem "600")
   │  └─ Piece_4 (Button) → orange/fire pickaxe + PriceChip(gem "900")
   └─ DetailPanel (right)
      ├─ Detail_Frame (Image, dark panel + gold edge, battlefield bg inside)
      ├─ Detail_Title (Text "LEAF SET")
      ├─ Detail_Bonuses (VerticalLayoutGroup, 4 rows: icon + text)
      │  ├─ "+ 30% Build Speed"   (leaf/gold icon)
      │  ├─ "+ 20% Extra Health"  (green heart icon)
      │  ├─ "+ 25% Mining Speed"  (pickaxe icon)
      │  └─ "+ 15% Unit Regen"    (sword/plus icon)
      ├─ Detail_Flavor (Text "Harness the power of nature.\nGrow. Protect. Dominate.")
      └─ EquipAllButton (Button, cobalt-blue) → Label "EQUIP ALL"
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **Shared chrome** — identical to `17 §D`; **Tab_Skins selected** (gold-lit). Currencies 1726 / 48570.
- **SkinRail** — parent SafeAreaRoot; anchor **left** (0,1) pivot 0,1; rect x 0.030→0.115 W, y 0.10→0.95 H
  (tall narrow column beside Back). Rail_Frame Image (ornate gold container) + inner `VerticalLayoutGroup`
  (spacing≈10px, 5 equal cells) — wrap in a `ScrollRect` (vertical) if the roster can exceed 5. Each cell =
  `Toggle` (ToggleGroup `SkinList`) with a square portrait Image; selected = gold-lit frame + glow.
- **HeroPreview** — parent SafeAreaRoot; anchor **center** (0.5,0.5) biased toward center-left of the open
  area; rect ≈ x 0.27→0.62 W, y 0.10→0.92 H. Hero_Pedestal behind (faint disc), Hero_Model foreground (full
  body). Decorative (not a button); selection happens via rail/pieces.
- **SetPieces_Row** — parent SafeAreaRoot; anchor **bottom-center** (0.5,0) pivot 0.5,0; rect x 0.255→0.665 W,
  y 0.02→0.27 H. Container Image (gold-framed dark bar) + `HorizontalLayoutGroup` (4 equal, spacing≈16px,
  childAlignment MiddleCenter, childForceExpandWidth=true). Each Piece = Button = vertical mini-group [Art
  Image over PriceChip(gem+count)]. Selected piece = green highlight frame + ✓ badge (top-right).
- **DetailPanel** — parent SafeAreaRoot; anchor **right** (1,1) pivot 1,1; rect x 0.745→0.985 W,
  y 0.10→0.93 H. Detail_Frame (dark panel, gold edge, faint battlefield art inside). Internal
  `VerticalLayoutGroup` (padding ~24, spacing ~12): Title → Bonuses(sub-VLG of 4 icon+text rows) → Flavor →
  spacer → EquipAllButton (pinned bottom).
- **Detail_Bonuses rows** — each = `HorizontalLayoutGroup` [Icon 26px][Text left-aligned]. Icons themed
  (leaf, heart, pickaxe, sword).
- **EquipAllButton** — cobalt-blue pill, gold-edged; width ~0.80 of panel inner; anchored bottom.

**Responsive:** chrome to corners. Rail anchored left edge, panel right edge → stable; hero centers in the
gap. On ultrawide the gap widens (hero stays centered, bg reveals more camp). On 4:3 tablet, rail/panel narrow
slightly; set-pieces stay 4-up (GridLayoutGroup fallback 4 cols → 2×2 below ~1.7:1). ScrollRect handles >5
skins.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Chrome:** as 17 §E (Back 0.045 W sq TL; TabBar 0.44 W centered TC; currencies right ~0.27 W TR).
- **Skin rail:** x 0.030→0.115 (Δ0.085 W ≈199px) · y 0.10→0.95 (Δ0.85 H ≈918px). 5 cells each ≈0.155 H tall
  ≈ square portraits ~0.075 W; gaps ≈0.012 H. Selected (top) gold-lit.
- **Hero preview:** x 0.27→0.62 (Δ0.35 W ≈819px) · y 0.10→0.92. Focal mass centered ≈ (0.44 W, 0.50 H);
  pedestal disc at y ≈0.80.
- **Set-pieces row:** x 0.255→0.665 (Δ0.41 W ≈959px) · y 0.02→0.27 (Δ0.25 H ≈270px). 4 pieces each ≈0.092 W,
  gaps ≈0.015 W; price chip below each at y ≈0.04→0.10. Selected (Piece_2) green frame + ✓.
- **Detail panel:** x 0.745→0.985 (Δ0.24 W ≈562px) · y 0.10→0.93 (Δ0.83 H ≈896px).
  - Title y ≈0.155; 4 bonus rows y 0.24→0.50 (each ≈0.065 H); Flavor y ≈0.58 (2 lines); EQUIP ALL y
    0.80→0.90 (blue pill ≈0.20 W × 0.075 H).
- **Notch/tablet/ultrawide:** SafeAreaFitter insets interactive layer; rail + panel + pieces always inside;
  hero may bleed slightly. Bg full-bleed under cutout.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tabs / currencies / Back | (shared, 17 §F) | — | — | — | — | — | — |
| Detail title "LEAF SET" | Serif display | Bold | UPPER | +4% | 38 | gold bevel + glow, dark stroke | `#9bff7a`→`#e8d27a` (green-gold) |
| Bonus rows ("+ 30% Build Speed"…) | Sans / light serif | Semibold | Title | +1% | 22 | light text, dark shadow; "+nn%" emphasized | `#eaf3df` pale-green-white |
| Bonus % value emphasis | Sans | Bold | — | 0 | 23 | brighter | `#bff7a0` lime |
| Detail flavor (2 lines) | Serif italic | Regular | Title | +1% | 20 | muted, centered | `#cdbf9a` faded gold |
| Set-piece prices (100/300/600/900) | Sans | Bold | — | 0 | 26 | white, dark stroke; violet gem icon left | `#ffffff` |
| EQUIP ALL (button) | Serif display | Black | UPPER | +5% | 32 | gold bevel text on cobalt, glow + outline | `#f3e6c0` on blue |
| Selected ✓ badge | (icon) | — | — | — | — | green check on gold disc | `#3fd24a` |

---

## G · MATERIALS
- **Gold frames / chrome:** as 17 §G (rail container, set-pieces bar, detail-panel edge all share it).
- **Skin rail portraits:** dark inset thumbnails, ornate gold mini-frames; selected = gold-lit frame +
  warm glow halo; unselected = dim. Portraits are small character busts (per theme color).
- **LEAF SET hero:** dark silhouette body wrapped in **glowing emerald foliage** `#2faa3a`→`#9bff7a`,
  leaf-blade weapons, glowing white-green eyes; strong inner glow + rim light + soft bloom; stands on a faint
  light disc.
- **Set-piece pickaxes:** themed materials —
  - Piece_1 plain: weathered steel `#8a8f98` + wood haft `#5a4326`, low shine.
  - Piece_2 Leaf (selected): living green `#3fd24a`→`#9bff7a`, glowing, organic — green highlight frame + ✓.
  - Piece_3 ice/blue: frosted blue `#6fb6ff`→`#cfeaff`, crystalline, cold specular.
  - Piece_4 fire/orange: molten `#ff7a2a`→`#ffd07a`, ember glow, smoking edges.
- **Price chips:** dark plate + violet gem icon + white count (like Spells orbs).
- **Detail panel:** dark glass panel `#0e1118`/85% with gold edge; a faint **battlefield-at-dusk** image
  inside (desaturated, ember horizon); bonus icons small gold/colored glyphs (leaf, green heart, pickaxe,
  sword-plus).
- **EQUIP ALL button:** royal/cobalt blue `#1f3fb0`→`#3a63d0` with bright top bevel + gold edge line + outer
  glow; pressed darkens. (Distinct from Spells' violet BUY.)

---

## H · COMPONENTS (states + feedback)
**Shop tabs / currency / Back** — as 17 §H; **Tab_Skins selected** (gold-lit).

**Skin rail thumb (Toggle, ToggleGroup SkinList)**
- *idle:* dim portrait in gold mini-frame.
- *hover:* +8% brightness, faint glow.
- *pressed:* 0.96 scale.
- *selected:* gold-lit frame + glow halo; hero preview + detail panel + set-pieces switch to this set.
- *locked/unowned:* could show a lock overlay (not visible in mock; include only if data says locked).

**Set-piece (Button)**
- *idle:* art + price chip in dark cell.
- *hover:* lift −5px, art bloom +10%.
- *pressed:* 0.97 scale, price chip flash.
- *selected (equipped piece):* green highlight frame + ✓ badge (Piece_2) — denotes the currently equipped
  piece of that slot.
- *owned vs purchasable:* owned → ✓ / "EQUIP"; not owned → gem price acts as buy. (Mock shows prices on all;
  treat price = buy-then-equip; ✓ = owned+equipped.)
- *can't afford:* price tints red on buy attempt → insufficient modal.

**EQUIP ALL button**
- *idle:* cobalt pill "EQUIP ALL".
- *hover:* +8% glow, scale 1.03.
- *pressed:* 0.97 darken.
- *already fully equipped:* label → "EQUIPPED" + disabled (45%).
- *partial-owned set:* tapping equips owned pieces + prompts purchase for missing (or disabled until owned —
  governed by ADR/data); on success → green flash + "EQUIPPED" + hero updates.

---

## I · ANIMATION TIMELINE
- **OnShow (0→0.5s):** Bg fade (0.25) → chrome slide-down (0–0.25) → SkinRail slides in from left −20px +
  fade (0.1–0.4) → HeroPreview fades + scales 0.96→1 with a green glow ignite (0.15–0.45) → SetPieces stagger
  pop left→right 40ms (0.2–0.45) → DetailPanel slides from right +24px (0.2–0.45).
- **Idle loops:** hero green glow breathe (2s) + slow foliage shimmer + idle sway (subtle ±2°); torch flicker
  on bg posts; selected rail-frame + piece-frame gentle glow pulse (2.5s).
- **Skin switch (0.25s):** old hero fade/scale-down → new hero fade/scale-up + glow ignite; rail glow + detail
  panel + set-pieces cross-fade (out 0.12 / in 0.15); EQUIP ALL state recomputes.
- **Piece select (0.15s):** previous green frame fades, new piece green frame + ✓ pops (scale 0.8→1.1→1.0);
  hero's matching slot art swaps.
- **Equip all success:** hero flash gold-green + sparkle ring + "EQUIPPED"; pieces all show ✓.
- **Buy/insufficient:** price chip red shake (0.3) → insufficient modal.

Easing: hero swaps ease-in-out 0.25; pops ease-out back 0.2; glows sine.

---

## J · PARTICLE & FX
- LEAF hero: drifting glowing leaf/spore particles (green additive) + rim shimmer; pedestal soft light pool.
- Selected piece: small green sparkle on the ✓ badge.
- Bg: torch ember particles on the camp posts.
- Equip-all success: green-gold burst from hero + ring sweep.
- Global: vignette + bloom; dusk haze.

---

## K · EVENT BEHAVIOR
- **OnShow:** bind skin roster (id, name, bonuses[], flavor, pieces[], owned/equipped flags, gemPrices —
  server-auth); default-select LEAF SET; bind currencies.
- **OnSelectSkin(set):** update hero preview + detail (title/bonuses/flavor) + set-pieces row + EQUIP ALL
  state.
- **OnSelectPiece(piece):** equip/highlight that piece (✓); if unowned, treat price as buy → server purchase.
- **OnEquipAll(set):** apply set (server-auth equipped state); update hero + ✓s; success flash.
- **OnBuyPiece / OnInsufficient:** server purchase; insufficient → modal (37).
- **OnTabSelect / OnBack / OnAdd "+":** as 17 (swap tab / pop / route to store).

---

## L · NEGATIVE RULES
- Forensic copy binding: **LEAF SET**; bonuses **+30% Build Speed, +20% Extra Health, +25% Mining Speed,
  +15% Unit Regen**; flavor "Harness the power of nature. Grow. Protect. Dominate."; **EQUIP ALL**; piece
  prices **100 / 300 / 600 / 900** gems (Piece_2 green ✓ selected); currencies **1726 / 48570**; rail = **5**
  skins.
- Keep EQUIP ALL **cobalt-blue** (apply/equip), NOT violet (that's Spells' premium BUY).
- Do NOT move chrome (Back TL, tabs TC, currencies TR). Bg full-bleed under cutout; rail/hero/pieces/panel in
  safe area.
- **§12 / server-auth:** equipped/owned state and balances are server-authoritative; UI never mutates them.
  **No gameplay/ECS/balance change.**
- **ADR flag (do not alter spec):** the stat-modifier bonuses collide with visual-only-cosmetic +
  "gems-never-buy-power" canon → ADR required. Record the modifiers **exactly as drawn**; the ADR governs
  whether they ship as Gold-only/cosmetic/ranked-standardized. Do NOT remove or "fix" the bonuses in this
  forensic spec.
- Invent nothing (no extra pieces, no fabricated rarities/percentages).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Shared chrome correct; **SKINS** tab selected; Gems 1726 / Gold 48570 with "+".
2. Left rail: 5 gold-framed skin portraits, top (LEAF) gold-lit/selected.
3. Center: glowing green LEAF SET full-body hero on a light pedestal.
4. Bottom: 4 themed pickaxe pieces with prices 100/300/600/900; green Leaf piece selected (frame + ✓).
5. Right panel: "LEAF SET" + exactly the 4 bonuses with icons + flavor + cobalt **EQUIP ALL**.
6. Palette: emerald hero + violet price chips + gold frames + dusk-armory bg, vignette/bloom.
7. Fraction layout within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet; rail scrolls if >5.
8. Skin/piece selection + equip-all + insufficient feedback per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**92/100.** Copy/bonuses/prices/icons legible; chrome inherited. Minus: (a) glowing-foliage hero + themed
pickaxe renders are bespoke art (use sprites); (b) per-skin portrait set is custom; (c) exact bonus-icon
glyphs approximated; (d) ADR may change whether bonuses are shown as power — but the **visual** spec is firm.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (topbar, thumb rail, hero, set-pieces, detail panel) before writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree incl. shared tab bar + currency chips + back.
- [x] Fraction layout normalized to 2340×1080; tablet/ultrawide/notch + ScrollRect covered.
- [x] Forensic copy/bonuses/prices/colors recorded; nothing invented.
- [x] §12/server-auth + **ADR stat-modifier flag** noted in A/L; spec NOT altered by the flag.
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.
