# BULWARK — UI CONSTRUCTION SPEC · 17 · Store

Source: design/StoreScreenDesign.png · 1672×941 (1.78:1) · Analysis-only forensic spec.

> **Shop-chrome anchor file.** This screen is the canonical reference for the **shared shop chrome** — top
> TAB BAR (SPELLS · SKINS · CHESTS · STORE), top-right currency chips (Gems + Gold, each with a green "+"),
> and top-left Back. Specs 18–20 (Spells/Skins/Chests) **inherit Sections C/D/E/F/H for the chrome verbatim**
> and only re-document deltas. Read this file first when building any shop tab.

---

## A · SCREEN PURPOSE
The **Store** is the hard-currency / IAP storefront tab of the shop hub. It sells **Gems** (premium currency),
**resource bundles**, and cross-promotes the **Battle Pass**. Player tasks: (1) buy the hero **Legendary
Starter Bundle** (a one-time, time-limited value bundle), (2) buy one of **five gem packs** at ascending
price tiers, (3) jump to the **Battle Pass** via a side promo, (4) re-filter the catalog via **five bottom
sub-tabs** (FEATURED / GEMS / RESOURCES / OFFERS / DAILY DEALS). The screen shown is the **FEATURED** sub-tab.

This is the only shop tab that spends **real money** ($ prices), so the visual hierarchy pushes the high-value
bundle and the "BEST SELLER" pack hardest. A red notification dot on the STORE tab signals a new/featured offer.

**ADR note (Section L):** Store itself is monetization-clean (direct-price gem packs + bundle, no randomness).
The right-rail **Battle Pass** promo and the **CHESTS** tab it sits beside are the gacha surfaces — see 20/21.

---

## B · VISUAL DNA (screen-specific, on top of the global baseline)
- **Mood:** opulent royal treasury / merchant hall. Near-black obsidian field; brushed-gold ornate frames;
  the whole screen is denser and brighter than the meta screens because it must read as "premium store."
- **Palette anchors:** gem-violet / amethyst (`#7a3fd0`–`#b06bff`) dominates (gems are the product); gold/bronze
  frames (`#caa04a`–`#f0d27a` hi, `#6b5320` shadow); cobalt accents on the bundle chest & BP art; **gold price
  ribbon** for the bundle ($9.99) and **violet price chips** for the gem packs ($-labels on cobalt-violet).
- **Lighting:** warm focal glow on the central Starter-Bundle chest (volumetric god-ray behind it); softer rim
  on each pack card; vignette darkens the four screen corners; bloom on every gem pile.
- **Background:** full-bleed stone-vault wall with hanging gold lantern / treasure props at far left & right
  (these bleed under the safe area on wide devices; never let UI overlap them).
- **Hierarchy:** Starter Bundle (top-left hero block, biggest) → BEST SELLER pack (center, lifted) → other
  packs → Battle Pass promo (right rail) → tab bar / currencies / sub-tabs (chrome).

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
StoreScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, stone-vault art, extends UNDER cutout)
   │  └─ Bg_Vignette (Image, radial dark overlay)
   ├─ TopChrome  ── [SHARED — anchor definition]
   │  ├─ BackButton (Button)
   │  │  ├─ Back_Plate (Image, blue beveled gold-edged plate)
   │  │  └─ Back_Arrow (Image, gold curved left-arrow)
   │  ├─ TabBar (HorizontalLayoutGroup container, gold-framed bar)
   │  │  ├─ Tab_Spells   (Toggle) → Icon(blue spell-orb) + Label "SPELLS"
   │  │  ├─ Tab_Skins    (Toggle) → Icon(warrior helm)   + Label "SKINS"
   │  │  ├─ Tab_Chests   (Toggle) → Icon(treasure chest) + Label "CHESTS"
   │  │  └─ Tab_Store    (Toggle, SELECTED) → Icon(shopping cart) + Label "STORE"
   │  │     └─ Tab_NotifyDot (Image, red dot, top-right of Store tab)
   │  └─ CurrencyChips (HorizontalLayoutGroup)
   │     ├─ GemChip  (container) → Icon(violet faceted gem) + Value "1746" + AddBtn(green "+")
   │     └─ GoldChip (container) → Icon(silver star-coin)   + Value "58,420" + AddBtn(green "+")
   ├─ Content (safe-area inner; three columns: bundle stack | pack row | BP rail)
   │  ├─ StarterBundle_Card (large panel, top-left/center)
   │  │  ├─ Bundle_Frame (Image, ornate gold frame)
   │  │  ├─ Bundle_Title  (Text "LEGENDARY\nSTARTER BUNDLE")
   │  │  ├─ Bundle_Subtitle (Text "Best value!" + "Limited time offer!")  [two-color]
   │  │  ├─ Bundle_Hero (Image, cobalt war-chest spilling gems, god-ray)
   │  │  ├─ Bundle_ContentsBar (HorizontalLayoutGroup, dark rounded strip)
   │  │  │  ├─ Item_Gems   (Icon violet gem  + "2000")
   │  │  │  ├─ Item_Gold   (Icon silver coin + "50000")
   │  │  │  ├─ Item_Keys   (Icon blue key    + "5")
   │  │  │  └─ Item_Potion (Icon violet vial + "10")
   │  │  ├─ Bundle_ValueBadge (Image+Text "400%\nVALUE", angled gold burst)
   │  │  └─ Bundle_PriceBtn (Button, gold ribbon, "$9.99")
   │  ├─ GemPack_Row (HorizontalLayoutGroup, 5 cards)
   │  │  ├─ Pack_1 "HANDFUL OF GEMS"  → gem-pile art + "80"   + PriceBtn "$0.99"
   │  │  ├─ Pack_2 "SACK OF GEMS"     → gem-pile art + "500"  + PriceBtn "$4.99"
   │  │  ├─ Pack_3 "CHEST OF GEMS"    → [BEST SELLER ribbon] gem-chest art + "1200" + PriceBtn "$9.99"
   │  │  ├─ Pack_4 "VAULT OF GEMS"    → gem-vault art + "2500" + PriceBtn "$19.99"
   │  │  └─ Pack_5 "MOUNTAIN OF GEMS" → gem-pyramid art + "6500" + PriceBtn "$49.99"
   │  └─ BattlePass_Promo (panel, right rail)
   │     ├─ BP_Frame (Image, gold frame)
   │     ├─ BP_Title (Text "BATTLE PASS") + BP_InfoDot (ⓘ)
   │     ├─ BP_Season (Text "— SEASON 1 —")
   │     ├─ BP_Name (Text "GLORY OF KINGS")
   │     ├─ BP_ValueBadge (Text "400%\nVALUE", angled, left edge)
   │     ├─ BP_Art (Image, dark-knight key art)
   │     ├─ BP_RewardStrip (Row of 3 reward thumbs: gold token, violet chest, blue gem)
   │     ├─ BP_Caption (Text "UNLOCK PREMIUM REWARDS!")
   │     └─ BP_GoBtn (Button "GO NOW" + crown icon, gold)
   └─ SubTabBar (bottom, HorizontalLayoutGroup, 5 toggles)
      ├─ SubTab_Featured (SELECTED) → gold star + "FEATURED"
      ├─ SubTab_Gems        → violet gem + "GEMS"
      ├─ SubTab_Resources   → silver coin + "RESOURCES"
      ├─ SubTab_Offers      → red gift   + "OFFERS"
      └─ SubTab_DailyDeals  → hourglass  + "DAILY DEALS"
```

---

## D · UNITY HIERARCHY SPEC (per node)

### Shared chrome (canonical — 18/19/20 inherit)
- **StoreScreen** — parent: UiRouter content; type: GameObject + `UiScreen`, `CanvasGroup`. Anchors stretch
  full. Built in code; no prefab.
- **SafeAreaRoot** — parent StoreScreen; `SafeAreaFitter`; anchors stretch 0..1; offsets 0. All interactive
  chrome parents here. **Bg_FullBleed is OUTSIDE** safe area (true 0..1 of canvas) so art bleeds under notch.
- **Bg_FullBleed** — parent StoreScreen (not SafeAreaRoot); Image; anchor stretch full; pivot .5,.5;
  `preserveAspect`=false (cover). **Bg_Vignette** child, same rect, multiply overlay.
- **BackButton** — parent SafeAreaRoot; Button; anchor **top-left** (0,1) pivot 0,1; pos ≈ (+x 1.0%, −y 1.5%)
  inset. Square ~ 0.045 W. Children Back_Plate (fill) + Back_Arrow (centered, ~70% of plate).
- **TabBar** — parent SafeAreaRoot; container Image (gold bar) + `HorizontalLayoutGroup`
  (spacing≈8px@1080, childForceExpandWidth=false, childAlignment=MiddleCenter, padding L/R≈14). Anchor **top-
  center** (0.5,1) pivot 0.5,1; y inset −1.5%. Width ≈ 0.44 W (centered between Back and currencies).
  Children: 4 `Toggle` (shared ToggleGroup `ShopTabs`). Each tab = HorizontalLayoutGroup(Icon 28px + Label).
  Child order = Spells, Skins, Chests, Store.
- **Tab_NotifyDot** — parent Tab_Store; Image (red circle ~10px@1080) anchor top-right (1,1) pivot .5,.5,
  small outward offset.
- **CurrencyChips** — parent SafeAreaRoot; `HorizontalLayoutGroup` (spacing≈14px). Anchor **top-right** (1,1)
  pivot 1,1; pos inset (−x 1.0%, −y 1.5%). Children GemChip, GoldChip (left→right). Each chip = HLG of
  [Icon 30px][Value Text right-aligned][AddBtn 30px green "+"].

### Store-specific content
- **Content** — parent SafeAreaRoot; empty RectTransform; anchor stretch with insets: top ≈ 9% (below
  chrome), bottom ≈ 9% (above sub-tabs), left/right ≈ 1.5%. NOT a layout group (free 3-column placement via
  child anchors).
- **StarterBundle_Card** — parent Content; anchor top-left region; pivot 0,1; rect ≈ x 0.07→0.555 W,
  y top 0.135→0.34 H (a wide landscape banner). Internal: Title top-left, Hero center-right (chest art),
  ContentsBar a dark rounded strip lower-left, ValueBadge near chest, PriceBtn bottom-right corner.
- **Bundle_ContentsBar** — `HorizontalLayoutGroup`, 4 items even, childAlignment MiddleCenter; each item is a
  vertical mini-group [Icon over Value] OR icon-left/value-right (art shows icon-above-number; treat as
  vertical mini-group, spacing≈4).
- **GemPack_Row** — parent Content; `HorizontalLayoutGroup` (childForceExpandWidth=true, spacing≈12px@1080,
  childAlignment=MiddleCenter). Anchor: x 0.07→0.79 W, y 0.40→0.86 (the main pack band). 5 equal cards.
  Pack_3 (BEST SELLER) is scaled ~1.05 and lifted ~−12px (see H).
- **Each Pack card** — vertical stack: [optional Ribbon] → Title (1–2 lines) → ArtIcon → GemRow(icon+count)
  → PriceBtn. Use VerticalLayoutGroup inside the card with controlled spacing.
- **BattlePass_Promo** — parent Content; anchor **right** column; pivot 1,1; rect ≈ x 0.79→0.985 W,
  y 0.10→0.875 H (tall narrow rail). Vertical internal stack (Title→Season→Name→Art→RewardStrip→Caption→GoBtn).
- **SubTabBar** — parent SafeAreaRoot; container Image (gold bar) + `HorizontalLayoutGroup` (5 toggles,
  childForceExpandWidth=true, childAlignment MiddleCenter). Anchor **bottom-center** (0.5,0) pivot 0.5,0;
  y inset +1.0%; width ≈ 0.66 W centered. Each sub-tab = vertical mini-group [Icon over Label] (art shows
  icon above text).

**Responsive:** chrome anchored to its own corner/edge → stable on any aspect. Content uses fraction rects;
on ultrawide the bg reveals more side props and the 3 columns keep their fractions (extra gutter splits
evenly). On 4:3 tablet, content insets grow; GemPack_Row stays 5-up until <2.0:1 then may wrap to 3+2 (allow
GridLayoutGroup fallback, constraint=FixedColumnCount 5, switch to 3 below threshold).

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
All Y measured from top. Canvas 2340 W × 1080 H.

**Chrome band (shared):**
- Top chrome occupies y 0 → ~0.085 H (≈92px). Back, TabBar, CurrencyChips vertically centered in it.
- BackButton: ~0.045 W square (≈105px) at x 0.010 W, y −0.015 H.
- TabBar: width 0.44 W (≈1030px) centered at x 0.5; height ≈0.062 H (≈67px). 4 tabs share it equally
  (~0.11 W each incl. spacing).
- CurrencyChips: right-anchored, total width ≈0.27 W; gem chip ~0.12 W, gold chip ~0.13 W (gold value is
  longer "58,420"), gap 0.006 W.
- SubTabBar: width 0.66 W (≈1545px) centered; height ≈0.075 H (≈81px); bottom inset +0.01 H. Each of 5 cells
  ≈0.132 W.

**Starter Bundle card:** x 0.07→0.555 (Δ0.485 W ≈1135px) · y 0.135→0.345 (Δ0.21 H ≈227px).
- Title block left ≈x 0.085, two serif lines.
- ContentsBar: dark strip x 0.085→0.34, y 0.255→0.315 (4 items each ≈0.062 W).
- PriceBtn ribbon: x 0.485→0.555, y 0.275→0.325 (gold ribbon ≈0.07 W × 0.05 H).
- ValueBadge "400% VALUE": small angled burst near the chest, x ≈0.50, y ≈0.16.

**Gem pack row:** band x 0.07→0.79 (Δ0.72 W ≈1685px) · y 0.40→0.86 (Δ0.46 H ≈497px).
- 5 cards, gaps ≈0.011 W → each card ≈0.135 W (≈316px) × 0.46 H. Card_3 scaled 1.05 → ~0.142 W and lifted.
- Inside a card: Title at top 12%, Art icon center 45%, GemRow ~70%, PriceBtn bottom 86–98% (price chip
  ≈0.10 W × 0.045 H).

**Battle Pass rail:** x 0.79→0.985 (Δ0.195 W ≈456px) · y 0.10→0.875 (Δ0.775 H ≈837px).
- Title y 0.12, Season y 0.18, Name y 0.225, ValueBadge angled at left edge y ≈0.27, Art y 0.30→0.66,
  RewardStrip y 0.70 (3 thumbs each ≈0.05 W), Caption y 0.78, GoBtn y 0.82→0.87 (gold button full rail width).

**Notch/tablet:** SafeAreaFitter insets the whole interactive layer; Bg_FullBleed unaffected. Ultrawide
(21:9+) → side props reveal; keep TabBar ≤0.44 W so it never collides with currencies.

---

## F · TYPOGRAPHY (per text element)
Sizes given at 1080-tall reference. Serif = Trajan/Cinzel-style display (TMP SDF target; legacy Text fallback).

| Element | Face / personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tab labels (idle) | Serif display | Semibold | UPPER | +4% | 26 | soft inner shadow | `#c9b27a` muted gold |
| Tab label (selected STORE) | Serif display | Bold | UPPER | +4% | 27 | gold bevel + faint glow | `#f3d98a` |
| Currency value (Gems/Gold) | Semi-condensed sans | Bold | — | 0 | 28 | dark stroke 1px + drop-shadow | `#f4ead0` near-white gold |
| Bundle title "LEGENDARY / STARTER BUNDLE" | Serif display | Black | UPPER | +2% | 46 (line1 36 small-caps "LEGENDARY", line2 52) | heavy gold bevel + bloom + outer dark stroke | `#f0cf72`→`#caa04a` grad |
| Bundle "Best value!" | Serif italic | Semibold | Title | 0 | 20 | shadow | `#e7dcc0` cream |
| Bundle "Limited time offer!" | Serif italic | Semibold | Title | 0 | 20 | shadow | `#e0432a` ember (highlight) |
| Bundle contents numbers (2000/50000/5/10) | Sans | Bold | — | 0 | 24 | dark stroke + shadow | `#f4ead0` |
| Value badge "400% VALUE" | Sans condensed | Black | UPPER | 0 | 22 (two lines) | red→gold burst fill, white text, dark outline | text `#fff8e6` on `#c12a20`/gold burst |
| Bundle price "$9.99" | Serif | Bold | — | 0 | 30 | on gold ribbon, dark-brown text | `#3a2a10` on gold ribbon |
| Pack titles ("HANDFUL OF GEMS" …) | Serif display | Semibold | UPPER | +3% | 24 (2 lines, ~20 each) | gold bevel + shadow | `#e9cf86` |
| "BEST SELLER" / "BEST VALUE!" ribbon | Sans condensed | Black | UPPER | +2% | 18 | on gold tab, dark text | `#2c1f08` on `#e8b94a` |
| Pack gem counts (80/500/1200/2500/6500) | Sans | Bold | — | 0 | 30 | white, dark stroke; gem icon to left | `#ffffff` |
| Pack $ prices ($0.99 …) | Sans | Bold | — | 0 | 26 | on cobalt-violet chip, light text | `#f3ecff` on `#3a2c7a`/violet |
| BP "BATTLE PASS" | Serif display | Bold | UPPER | +4% | 30 | gold bevel + glow | `#f3d98a` |
| BP "SEASON 1" | Sans | Semibold | UPPER | +6% | 18 | between two rule lines | `#cdb887` |
| BP "GLORY OF KINGS" | Serif display | Bold | UPPER | +2% | 26 | gold + bloom | `#f0cf72` |
| BP "UNLOCK PREMIUM REWARDS!" | Sans condensed | Bold | UPPER | +2% | 18 | shadow | `#e7dcc0` |
| BP "GO NOW" | Serif | Bold | UPPER | +3% | 26 | on gold button, dark text + crown | `#2c1f08` on gold |
| Sub-tab labels | Serif display | Semibold | UPPER | +4% | 22 (selected 23 brighter) | bevel + shadow; selected gold-lit | idle `#b9a36e` / sel `#f3d98a` |

---

## G · MATERIALS (hex ranges, finish, wear, edges, bloom)
- **Gold/bronze frames (all chrome + cards):** base `#caa04a`, highlight `#f0d27a`–`#fff0b8` on top bevel,
  shadow `#6b5320`–`#3a2a10` in grooves; roughness mid (satin brushed), engraved filigree on outer rail;
  worn micro-scratches on edges; gentle rim-bloom on lit frames (selected tab/active slot).
- **Dark panel bases (tab bar, contents bar, pack interiors):** obsidian `#0c0e14`–`#161a24` vertical
  gradient, ~85% opaque, inner top-edge sheen; subtle noise.
- **Back plate:** cobalt `#1c3a8e`→`#274aa6` enamel with gold bevel edge; arrow `#f0cf72` with shadow.
- **Gems / amethyst (product hero):** crystal `#7a3fd0` core → `#b06bff` faces → `#e7ccff` specular hot-spots;
  high reflection, strong inner glow, additive bloom; cut facets with sharp white speculars. Gem piles read
  as hundreds of faceted crystals with violet glow + warm gold underlight.
- **Silver coin (Gold currency):** brushed silver `#b9bcc4`→`#eef0f4` rim, dark center star emboss `#6a6e78`,
  cool specular.
- **Blue key icon (bundle):** steel-blue `#3f6fd8`→`#9fc0ff`, ornate bow, gold-ish teeth highlight.
- **Violet potion vial:** glass `#2a1840` body, liquid `#8a40d8`→`#c07bff`, cork gold cap, inner glow.
- **Cobalt war-chest (bundle hero):** royal-blue lacquered chest `#1e3a8c`→`#3a63d0`, gold fittings, a blue
  gem on the lid, spilling violet gems; lit by a warm god-ray from upper-back.
- **Price ribbon (gold, bundle):** cast-gold banner with notched ends, `#e8b94a`→`#f6d77a`, dark engraved
  text; faint specular sweep.
- **Price chips (gem packs):** cobalt-violet plate `#2c2370`→`#4a3aa6`, thin gold/edge line, light text.
- **BP key art:** desaturated dark-knight scene, cool blues + ember rim; framed in gold; slight inner vignette.
- **Sub-tab bar:** same gold-frame + obsidian base; selected cell gets a gold up-light + brighter icon.

---

## H · COMPONENTS (states + feedback)

**Shop Tab (Toggle, shared) — SPELLS/SKINS/CHESTS/STORE**
- *idle:* obsidian cell, muted-gold icon + label, no glow.
- *hover (pad/cursor):* +6% brightness, faint gold under-glow.
- *pressed:* −4% scale, brighter.
- *selected:* gold-lit frame around the cell, glow halo, label brightens to `#f3d98a`, icon full-color; an
  upward gold light-bleed (see Spells/Skins/Chests where the active tab clearly glows gold). Only one selected
  (ToggleGroup). STORE is selected here.
- *disabled:* 45% opacity, desaturated.
- *notify:* red dot (Tab_Store here) — pulse 1.0–1.15 scale loop 1.2s.

**Currency chip + AddBtn**
- chip static (display). **AddBtn** green "+" (`#2faa3a`→`#46d257`, gold edge): idle / hover +8% / pressed
  −6% scale → routes to Store GEMS or RESOURCES sub-tab (OnAddCurrency).

**Gem-pack card (Button)**
- *idle:* gold-framed card, art + count + price chip.
- *hover:* lift −6px, frame brightens, art bloom +10%.
- *pressed:* −3% scale, price chip flashes.
- *BEST SELLER (Pack_3):* persistent gold "BEST SELLER" ribbon top-center, card scaled 1.05 + raised, extra
  rim-glow → strongest pack. (Bundle uses "Best value!" copy instead of a ribbon.)
- *purchasing:* price chip → spinner; on success: gem-count fly-to GemChip + chip "+N" tick-up (count-up
  ~0.6s) + sparkle burst on card; on fail: red shake + insufficient/IAP-error modal.
- *owned/one-time (Bundle):* after purchase → "PURCHASED" stamp + button disabled (45%).

**Bundle price button** — gold ribbon `$9.99`; hover brighten, pressed −3%, success → purchase flow + bundle
contents fly to currency chips (gems→GemChip, gold→GoldChip, keys/potions→inventory toast).

**Battle Pass GO NOW button** — gold pill + crown; hover brighten + crown sparkle; pressed −3%; OnClick →
route to Battle Pass screen (25).

**Sub-tab (Toggle)** — vertical icon+label; selected = gold up-light + brighter; switching re-filters the
catalog (cross-fade the pack/bundle area).

---

## I · ANIMATION TIMELINE
- **OnShow (0.00→0.45s):** Bg fade-in 0→1 (0.25s) → chrome slides from top −20px ease-out (0.0–0.25) →
  StarterBundle scales 0.96→1 + fade (0.10–0.35) → GemPack_Row cards stagger-in left→right, each
  0.92→1 scale + fade, 40ms stagger (0.15–0.45) → BP rail slides from right +24px (0.20–0.40).
- **Idle loops:** Bundle chest god-ray slow pulse (3s, opacity 0.8↔1.0); gem piles slow sparkle (random
  specular twinkles); BEST SELLER ribbon faint gold shimmer (2.5s); STORE notify dot pulse (1.2s); BP art
  subtle ember drift.
- **Tab switch (out 0.12s / in 0.18s):** current content CanvasGroup fade 1→0 + −8px, new content fade 0→1
  + 8px→0; tab gold-glow snaps to new tab (0.10s). Easing ease-in-out.
- **Sub-tab switch:** pack/bundle band cross-fade 0.18s + 6px slide.
- **Buy success:** card sparkle burst (0.4s) → currency icons arc to chip (0.5s, ease-in) → chip count-up
  (0.6s) → chip bump 1.0→1.12→1.0 (0.18s).
- **Buy fail:** card + price chip red shake (x ±6px, 0.3s, 3 cycles) → insufficient/IAP-error modal fade-in.

Easing: UI moves ease-out/in-out 0.12–0.25s; celebratory bursts ease-out 0.4–0.6s.

---

## J · PARTICLE & FX
- Bundle chest: warm volumetric god-ray (soft additive cone) + slow floating dust motes + gem-pile sparkle.
- Gem packs: per-card faint violet bloom + occasional white specular twinkle on the gem art.
- BEST SELLER: thin gold sweep across the ribbon (2.5s loop).
- Buy success: 12–18 gold/violet spark particles burst from card, then currency-icon projectiles to chip.
- BP rail: subtle ember/ash particles drifting up over the key art; crown on GO NOW emits a tiny sparkle on
  hover.
- Vignette + global bloom post applied to whole screen (treasury glow).

---

## K · EVENT BEHAVIOR
- **OnShow:** load catalog (server-auth prices/SKUs); bind currency values from wallet; resolve STORE notify
  dot from "has new offer" flag; default sub-tab = FEATURED.
- **OnTabSelect(tab):** UiRouter swaps shop tab content (Spells/Skins/Chests/Store) — same chrome persists.
- **OnSubTabSelect(filter):** re-query catalog filter; re-fill bundle/pack band.
- **OnBuyPack(sku) / OnBuyBundle:** invoke IAP/server purchase (client never mutates balance — §12); on
  success server returns new wallet → animate count-up + grant toast; one-time bundle flips to PURCHASED.
- **OnInsufficient / OnPurchaseError:** show ConfirmModal "Insufficient"/"Purchase failed" sheet (37).
- **OnAddCurrency(gem/gold "+"):** route to GEMS / RESOURCES sub-tab (or platform store).
- **OnBattlePassGo:** route to Battle Pass screen (25).
- **OnBack:** UiRouter pop → previous hub.

---

## L · NEGATIVE RULES
- Do NOT redesign or rename anything; copy/prices are forensic: bundle **$9.99** with **2000 gems / 50000
  gold / 5 keys / 10 potions** and a **400% VALUE** badge; packs **$0.99/80, $4.99/500, $9.99/1200 (BEST
  SELLER), $19.99/2500, $49.99/6500**; BP **SEASON 1 — GLORY OF KINGS, 400% VALUE, GO NOW**; sub-tabs
  **FEATURED/GEMS/RESOURCES/OFFERS/DAILY DEALS**; currencies **Gems 1746 / Gold 58,420**.
- Do NOT move currencies off top-right, Back off top-left, or the shop tab bar off top-center.
- Do NOT let interactive UI cross into the full-bleed side props; keep content in safe area.
- Bg_FullBleed stays OUTSIDE SafeAreaFitter; all interactive nodes INSIDE it.
- **§12 / server-auth:** UI never mutates a balance; purchases go through server/IAP and reflect the returned
  wallet. **No gameplay/ECS/balance change.**
- **ADR flag:** Store packs/bundle are direct-price (clean). The neighboring **CHESTS** tab and the **Battle
  Pass** are the monetization surfaces requiring the loot-box / value-claim ADRs (see 20/21) — do not "fix"
  them here; this is the visual spec only.
- Invent nothing not in the PNG (no extra packs, no fabricated timers).

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
1. Shared chrome present & positioned: Back top-left, tab bar (4 tabs, STORE selected + red dot) top-center,
   Gems 1746 + Gold 58,420 chips with green "+" top-right.
2. Starter Bundle: two-line gold title, "Best value!" + ember "Limited time offer!", 4-item contents bar
   (2000/50000/5/10 with correct icons), 400% VALUE badge, gold $9.99 ribbon — all in the top-left banner.
3. Five gem packs in order with correct names, gem counts, $ prices; Pack_3 carries BEST SELLER ribbon and is
   visually lifted/largest.
4. Battle Pass rail on the right: SEASON 1 / GLORY OF KINGS / 400% VALUE / reward strip / UNLOCK PREMIUM
   REWARDS! / gold GO NOW with crown.
5. Five bottom sub-tabs with correct icons/labels, FEATURED selected.
6. Palette: amethyst gems dominate, gold frames, cobalt accents, vignette+bloom treasury mood.
7. Layout matches fraction math within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet.
8. States (tab selected, BEST SELLER, buy success count-up, insufficient shake) behave per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**93/100.** Layout, copy, prices, icon set, and chrome are crisp and fully legible at zoom. Minus points:
(a) exact gem-pile art per pack is bespoke 2.5D render (approximate with layered gem sprites + glow);
(b) the 400% VALUE burst shape and BP key-art are unique illustrations (rebuild as framed image + badge);
(c) precise inner-padding of the bundle contents bar is estimated. None affect interactive fidelity.

## O · SELF-CHECKLIST
- [x] Read source PNG (+ zoom crops: top bar, currency, bundle, packs, BP, sub-tabs) before writing.
- [x] All A–O sections present and substantive, in order.
- [x] Full node tree incl. shared tab bar + currency chips + back (anchor file).
- [x] Fraction-based layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Forensic copy/prices/counts/colors recorded; nothing invented.
- [x] §12 / server-auth + ADR pointers noted in A/L (no spec alteration).
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.
