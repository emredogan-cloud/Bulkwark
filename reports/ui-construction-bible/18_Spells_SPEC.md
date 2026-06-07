# BULWARK — UI CONSTRUCTION SPEC · 18 · Spells

Source: design/SpellsScreenDesign.png · 1914×822 (2.33:1) · Analysis-only forensic spec.

> **Inherits the shared shop chrome from `17_Store_SPEC.md`** (Back top-left, tab bar SPELLS·SKINS·CHESTS·
> STORE top-center, Gems+Gold currency chips top-right). This file re-documents the chrome briefly and details
> only the **Spells-specific** body: a left **mage presenter**, four **crystal spell orbs** with gem prices,
> and a right **parchment detail panel** with **BUY**. Here the **SPELLS** tab is selected (gold-lit).

---

## A · SCREEN PURPOSE
The **Spells** shop tab sells **temporary battle spells / boons** purchased with **Gems**. A hooded mage
"presents" the wares; four crystal orbs on a draped table each hold a spell sigil and show a gem price; the
selected orb expands into a parchment scroll on the right describing the spell (name, effect, duration,
flavor) with a prominent **BUY** button repeating the price. Player tasks: browse the 4 orbs, select one to
read its scroll, and BUY with gems. Shown selected: **MINER'S BLESSING** (the leftmost golden-pickaxe orb).

**ADR note (Section L):** Spells are gem-purchased consumables that buff in-match performance. If any spell
grants a competitive power advantage purchasable only with premium gems, it brushes the "gems never buy power"
principle → flag for the economy ADR. Spec'd forensically regardless.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** an arcane wizard's study / spell shop — warm candlelit bookshelves behind, a focal blue magical
  glow on the mage's staff and the orbs. Magic = violet/cyan; commerce = gold.
- **Palette anchors:** amethyst/violet gem prices (`#8a40d8`–`#c07bff`); cyan-blue magical glow on staff &
  glowing eyes (`#3fd0ff`); crystal-orb glass (cool blue-white) with per-spell colored sigil; parchment cream
  scroll (`#d9c79a`–`# efe2bf`); gold frames; the BUY button is **violet/amethyst** (premium spend color),
  gold-edged — the brightest CTA.
- **Lighting:** focal glow on the four orbs (each emits its sigil's color) + a strong cyan key on the mage's
  staff orb; warm bookshelf ambience; vignette; bloom on glass + glow.
- **Background:** full-bleed dim library/study (shelves, candles, props) behind the mage and table; bleeds
  under cutout.
- **Hierarchy:** parchment scroll + BUY (right, the decision) ⟷ selected orb glow → mage presenter (left,
  characterful framing) → orb row (choices) → chrome.

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
SpellsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, wizard-study art)  [OUTSIDE safe area]
   │  └─ Bg_Vignette (Image)
   ├─ TopChrome  ── [SHARED — see 17 §C/§D]
   │  ├─ BackButton (Button: Plate + gold Arrow)
   │  ├─ TabBar (ToggleGroup ShopTabs)
   │  │  ├─ Tab_Spells (Toggle, SELECTED, gold-lit) → Icon(blue spell-orb) + "SPELLS"
   │  │  ├─ Tab_Skins  (Toggle) → Icon(helm) + "SKINS"
   │  │  ├─ Tab_Chests (Toggle) → Icon(chest) + "CHESTS"
   │  │  └─ Tab_Store  (Toggle) → Icon(cart) + "STORE"
   │  └─ CurrencyChips
   │     ├─ GemChip  → violet gem + "1726" + green "+"
   │     └─ GoldChip → silver coin + "48570" + green "+"
   ├─ MagePresenter (Image, hooded mage holding glowing staff, left foreground)
   │  ├─ Mage_Body (Image)
   │  └─ Staff_Glow (Image, cyan orb on staff, additive)
   ├─ OrbTable (group, center-bottom, blue-draped table w/ gold crown motif)
   │  ├─ Table_Cloth (Image, royal-blue cloth, gold crown emblem center)
   │  └─ OrbRow (HorizontalLayoutGroup, 4 orbs)
   │     ├─ Orb_1 (Button, SELECTED) → Globe + Sigil(golden pickaxe) + PriceChip(gem "150")
   │     ├─ Orb_2 (Button) → Globe + Sigil(red flaming sword) + PriceChip(gem "200")
   │     ├─ Orb_3 (Button) → Globe + Sigil(blue tridents/spears) + PriceChip(gem "250")
   │     └─ Orb_4 (Button) → Globe + Sigil(gold shield) + PriceChip(gem "300")
   └─ DetailScroll (Parchment panel, right)
      ├─ Scroll_Frame (Image, gold-capped rolled parchment)
      ├─ Detail_Title (Text "MINER'S BLESSING")
      ├─ Detail_Icon  (Image, golden pickaxe)
      ├─ Detail_Desc  (Text "Increases mining speed significantly for a limited time.")
      ├─ Detail_Stats (Text "Duration: 30 seconds\nEffect: 2x Mining Speed")
      ├─ Detail_Flavor (Text italic "The earth yields its riches to those blessed by magic.")
      └─ BuyButton (Button, violet) → Label "BUY" + PriceRow(gem icon + "150")
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **Shared chrome** — identical anchors/components to `17 §D` (Back top-left 0,1; TabBar top-center 0.5,1;
  CurrencyChips top-right 1,1). Only the **selected** toggle differs (Tab_Spells). Currency values 1726 / 48570.
- **MagePresenter** — parent SafeAreaRoot; Image (may be 2 layers: body + additive staff glow). Anchor
  **bottom-left** (0,0) pivot 0,0; rect x 0.0→0.30 W, y 0.10→0.95 H. Drawn as foreground character; NOT a
  button. Staff_Glow child = additive cyan sprite, slow pulse.
- **OrbTable** — parent SafeAreaRoot; anchor **bottom-center** (0.5,0) pivot 0.5,0; rect x 0.18→0.66 W,
  y 0.0→0.55 H. Table_Cloth Image behind; **OrbRow** = `HorizontalLayoutGroup` (4 equal, spacing≈18px@1080,
  childAlignment=MiddleCenter, childForceExpandWidth=true) anchored to the cloth top region.
- **Orb_N** — Button; vertical mini-group: [Globe Image with inner Sigil Image] over [PriceChip
  (HLG: gem icon + count)]. Globe ~0.10 W diameter. Selected orb gets a brighter rim-glow + slight scale 1.06.
- **DetailScroll** — parent SafeAreaRoot; anchor **right** (1,1) pivot 1,1; rect x 0.665→0.985 W,
  y 0.085→0.93 H (tall parchment). Internal `VerticalLayoutGroup` (padding ~24, spacing ~10,
  childControlHeight, childAlignment UpperCenter): Title → Icon (left-float or centered) → Desc → Stats →
  Flavor → spacer → BuyButton (pinned near bottom).
- **BuyButton** — large violet pill, gold-edged; child stack: "BUY" (line 1, big) over PriceRow (gem icon +
  "150"). Anchored bottom of scroll, width ~0.78 of scroll inner.

**Responsive:** chrome to corners (stable). Mage anchored bottom-left, orbs bottom-center, scroll right — on
ultrawide they spread with the revealed bg; on narrower aspect the mage may clip behind the orb table (keep
orbs + scroll always fully in safe area; mage is decorative and may bleed). Scroll keeps fixed fraction; text
auto-sizes within.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Chrome:** identical to 17 §E (top band y 0→0.085; Back 0.045 W sq; TabBar 0.44 W centered; currencies
  right ~0.27 W). Gem chip "1726", gold chip "48570".
- **Mage presenter:** x 0.0→0.30 (Δ0.30 W ≈702px) · y 0.10→0.95. Staff orb focal point ≈ (0.20 W, 0.30 H).
- **Orb table:** x 0.18→0.66 (Δ0.48 W ≈1123px) · cloth top ≈ y 0.42, table base to bottom.
  - OrbRow band: y 0.40→0.70; 4 orbs each ≈0.10 W globe, gaps ≈0.018 W; price chip directly below each globe
    at y ≈0.70→0.76.
  - Crown emblem centered on cloth at ≈(0.42 W, 0.86 H).
- **Detail scroll:** x 0.665→0.985 (Δ0.32 W ≈749px) · y 0.085→0.93 (Δ0.845 H ≈913px).
  - Title y ≈0.135; Icon y ≈0.22; Desc y ≈0.33; Stats y ≈0.47; Flavor y ≈0.62; BuyButton y ≈0.78→0.89
    (violet pill ≈0.24 W × 0.075 H, centered in scroll).
- **Notch/tablet/ultrawide:** SafeAreaFitter insets interactive layer; orbs+scroll guaranteed inside; mage
  bg-character may bleed. On 4:3, scroll narrows slightly and orb globes shrink to keep 4-up.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref. Serif = Trajan/Cinzel display; body = light serif or semi-condensed sans.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tab labels / currencies / Back | (shared, see 17 §F) | — | — | — | — | — | — |
| Spell price (orbs, "150"…"300") | Sans | Bold | — | 0 | 28 | white, dark stroke; violet gem icon left | `#ffffff` |
| Detail title "MINER'S BLESSING" | Serif display | Bold | UPPER | +3% | 34 | gold bevel + soft glow, dark-brown stroke (on parchment) | `#5a3a12`→`#8a5a1e` warm |
| Detail description | Light serif | Regular | Title | 0 | 22 | dark ink on parchment, slight shadow | `#3a2c14` ink-brown |
| Detail stats (Duration/Effect) | Light serif | Semibold | Title | 0 | 21 | ink-brown, label+value | `#3a2c14` |
| Detail flavor (italic) | Serif italic | Regular | Title | +1% | 20 | muted ink, centered, italic | `#5a4a2c` faded |
| BUY (button) | Serif display | Black | UPPER | +4% | 36 | gold bevel text on violet, glow + dark outline | `#f6e6b8` on violet |
| BUY price ("150" + gem) | Sans | Bold | — | 0 | 24 | light, gem icon left | `#f3ecff` on violet |

---

## G · MATERIALS
- **Gold frames / chrome:** as 17 §G.
- **Crystal spell orbs (globes):** cool blue-white glass `#cfe6ff` rim → translucent `#5a86c8` body, strong
  specular highlights, refraction; each holds a glowing colored **sigil**: Orb_1 golden pickaxe (`#f0c050`
  warm glow), Orb_2 red flaming sword (`#ff5a2a` ember), Orb_3 blue tridents/spears (`#4fa8ff` cyan), Orb_4
  gold shield (`#e8c25a`). Orbs sit on small **gold ring stands**. Additive inner glow per sigil + outer
  bloom; selected orb brighter.
- **Mage:** hooded robe deep violet/indigo `#2a1d4a`→`#4a3a7a` with gold trim; glowing **cyan eyes**
  `#5fe0ff`; staff topped by a cyan magical orb `#3fd0ff`→`#cfeeff` (brightest single light).
- **Table cloth:** royal-blue velvet `#1c2f7a`→`#2c47b0` with gold-embroidered **crown** emblem `#e8c25a`,
  tassel/fringe trim; soft sheen.
- **Parchment scroll:** aged cream `#d9c79a` center → `#c2ac74` edges, fiber texture, slight curl + drop
  shadow; **gold roller caps** top & bottom (`#caa04a`→`#f0d27a` with finial knobs); ink-brown text.
- **BUY button:** amethyst pill `#6a2db8`→`#9e6bf0` with bright top bevel + gold edge line + outer glow;
  pressed darkens; the most saturated element after the orbs.
- **Detail icon (pickaxe):** matches Orb_1 sigil — gold-headed pickaxe with warm glow.

---

## H · COMPONENTS (states + feedback)
**Shop tabs / currency chips / Back** — identical states to 17 §H. Here **Tab_Spells = selected** (gold-lit
frame + glow + bright label + full-color orb icon).

**Spell Orb (Button)**
- *idle:* glass globe + sigil + price chip; gentle float bob + sigil glow pulse.
- *hover:* orb rim brightens, scale 1.04, glow +15%.
- *pressed:* scale 0.97, brief flash.
- *selected:* scale 1.06, brightest rim-glow, the parchment scroll updates to this spell; a subtle gold
  underline ring on the stand. Only one selected at a time (acts like a radio within OrbRow).
- *affordable vs not:* if gems < price, price chip text tints red on attempted buy (orb still selectable to
  read).
- *owned/active (if consumable already active):* could show a small "ACTIVE" tag — not shown in mock; omit
  unless data says so.

**BUY button**
- *idle:* violet pill, "BUY" + price.
- *hover:* +8% glow, slight scale 1.03.
- *pressed:* 0.97 scale, darken.
- *disabled (can't afford):* desaturate to grey-violet 50%, "+" suggestion → tapping opens insufficient modal.
- *purchasing:* label → spinner; *success:* gem count flies from GemChip down (or gem-spend animation),
  scroll flashes gold, success toast/RewardGrant; *fail:* red shake + insufficient modal.

---

## I · ANIMATION TIMELINE
- **OnShow (0→0.5s):** Bg fade (0.25) → chrome slide-down (0–0.25) → Mage slide-in from left −24px + fade
  (0.1–0.4) → OrbRow orbs stagger pop 0.9→1 with glow ignite, 50ms stagger (0.15–0.45) → DetailScroll
  unrolls/scales 0.95→1 + fade (0.2–0.45).
- **Idle loops:** each orb float-bob (±6px, 2.5–3.2s, staggered phase) + sigil glow breathe (2s); mage staff
  orb cyan pulse (1.8s) + faint floating motes; parchment edge subtle sway.
- **Orb select (0.18s):** previous orb scale→1.0 dim, new orb scale→1.06 brighten; DetailScroll content
  cross-fades (out 0.1 / in 0.15) + tiny re-unroll bounce; BUY price updates.
- **Buy success:** gem-spend sparkle at GemChip (count-down) + scroll gold flash (0.3) + RewardGrant/toast.
- **Buy fail:** BUY + price red shake (0.3, 3 cycles) → insufficient modal.

Easing: floats sine in-out; UI moves ease-out 0.12–0.25; ignites ease-out 0.3.

---

## J · PARTICLE & FX
- Per-orb: additive sigil glow + slow swirling inner particles in the orb's color; occasional spark twinkle on
  the glass specular.
- Mage staff: cyan magical wisps / floating motes around the orb (additive).
- Selected orb: a faint rising sparkle column.
- BUY hover: small violet sparkle along the pill edge.
- Buy success: gold+violet burst from the scroll; gem icon dissolve at GemChip.
- Global: vignette + bloom; warm dust motes drifting in the study light.

---

## K · EVENT BEHAVIOR
- **OnShow:** bind catalog of spells (id, name, desc, duration, effect, flavor, gemPrice — server-auth);
  default-select Orb_1 (Miner's Blessing); bind currencies.
- **OnSelectOrb(spell):** update DetailScroll (title/icon/desc/stats/flavor) + BUY price; highlight orb.
- **OnBuy(spell):** server/wallet purchase (client never mutates balance, §12); on success grant + count-down
  gems + toast; on insufficient → modal (37).
- **OnTabSelect:** swap to Skins/Chests/Store (chrome persists). **OnBack:** UiRouter pop.
- **OnAddGem/Gold "+":** route to Store gems/resources.

---

## L · NEGATIVE RULES
- Forensic copy is binding: **MINER'S BLESSING** · "Increases mining speed significantly for a limited time."
  · "Duration: 30 seconds" · "Effect: 2x Mining Speed" · flavor "The earth yields its riches to those blessed
  by magic." · **BUY / 150**. Orb prices **150 / 200 / 250 / 300** gems; sigils pickaxe / flaming-sword /
  tridents / shield. Currencies **1726 / 48570**.
- Keep exactly **four** orbs; do not add/remove. Keep BUY violet (premium spend), not blue.
- Do NOT move chrome (Back TL, tabs TC, currencies TR). Bg full-bleed under cutout; orbs+scroll in safe area.
- **§12 / server-auth:** UI never mutates a balance; purchase via server. **No gameplay/ECS/balance change.**
- **ADR flag (do not alter spec):** premium-gem spell buffs may collide with "gems never buy power" → economy
  ADR. Spec'd as drawn.
- Invent nothing (no extra spell stats, no fabricated cooldowns beyond the drawn "Duration/Effect").

## M · ACCEPTANCE CRITERIA (≥95%)
1. Shared chrome correct; **SPELLS** tab selected (gold-lit); Gems 1726 / Gold 48570 with "+".
2. Hooded mage presenter bottom-left with glowing cyan staff orb.
3. Four crystal orbs on the blue crowned table, correct sigils + prices (150/200/250/300) below each.
4. Parchment scroll on right with MINER'S BLESSING title, pickaxe icon, exact description/stats/flavor text.
5. Violet gold-edged **BUY** button with "150" + gem icon — brightest CTA.
6. Palette: cyan magic + amethyst gems + cream parchment + gold frames; vignette/bloom study mood.
7. Fraction layout within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet.
8. Orb select updates scroll + BUY; buy/insufficient feedback per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**93/100.** All copy/prices/icons legible; chrome inherited. Minus: (a) crystal-orb refraction + per-sigil
glow is a custom shader/sprite stack (approximate with layered globe sprite + additive sigil + glow);
(b) mage character + study bg are bespoke art (use provided sprites); (c) parchment unroll is a stylistic
flourish (optional). No interactive-fidelity risk.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (topbar, orbs, orb1 pickaxe, mage, parchment) before writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree incl. shared tab bar + currency chips + back.
- [x] Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Forensic copy/prices/colors recorded; nothing invented.
- [x] §12/server-auth + ADR flag noted; spec not altered by the flag.
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.
