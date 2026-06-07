# BULWARK — UI CONSTRUCTION SPEC · 20 · Chests

Source: design/ChestsScreenDesign.png · 1914×822 (2.33:1) · Analysis-only forensic spec.

> **Inherits the shared shop chrome from `17_Store_SPEC.md`** (Back top-left, tab bar SPELLS·SKINS·CHESTS·
> STORE top-center, Gems+Gold chips top-right). This file details the **Chests-specific** body: a hooded
> **Keeper** presenter, a central **featured chest** with magical aura on a pedestal, a bottom **chest-slot
> row** with **unlock timers / locks**, and a right **OPEN CHEST** button. **CHESTS** tab selected.
> **ADR-flagged** (loot-box / gacha — see also 21 Chest Open Result).

---

## A · SCREEN PURPOSE
The **Chests** shop tab is the **loot-chest** surface. The player has a row of **chest slots** (earned/found
chests) that **unlock on timers** (or instantly via gems); a **featured chest** sits center-stage glowing,
guarded by a hooded **Keeper**; tapping **OPEN CHEST** (after unlock, or by spending gems to skip) plays the
reveal → routes to **Chest Open Result (21)**. Shown: slot 1 active with a **02:59** timer, slots 2–3 locked,
slot 4 empty; the featured gold chest with amethyst gem glows on its pedestal.

**ADR note (Section L):** timed chests + randomized rewards = a **loot-box / gacha** loop, which collides with
the "no loot boxes/gacha" principled cut → **requires an ADR** (transparent odds + a fair/redesigned model, or
a CUT). Spec'd **exactly as drawn**; ADR governs implementation, not this visual spec.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** a vaulted treasure crypt lit by **violet/amethyst magic** — gold-coin drifts in the dark side
  alcoves, arched stone pillars, a beam of purple light on the focal chest. The most overtly "magical/premium"
  shop tab (violet aura everywhere).
- **Palette anchors:** amethyst/violet aura + light beams (`#7a3fd0`–`#b06bff`); gold chest + gold frames;
  cobalt-blue accent on the active slot's chest gem; **OPEN CHEST button is violet/amethyst**, gold-edged,
  with a **padlock-and-key** icon — the brightest CTA.
- **Lighting:** a focal purple god-beam down onto the featured chest; rim-glow on the chest's gold; the Keeper
  is a dark silhouette with glowing eyes and outstretched arms; heavy vignette; strong bloom on aura + gem.
- **Background:** full-bleed crypt/vault (pillars, gold hoards in alcoves); bleeds under cutout.
- **Hierarchy:** featured chest + aura (center focal) → OPEN CHEST (right CTA) → chest-slot row + timers
  (bottom, the inventory) → Keeper (atmosphere) → chrome.

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
ChestsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, amethyst-lit treasure crypt)  [OUTSIDE safe area]
   │  └─ Bg_Vignette (Image)
   ├─ TopChrome  ── [SHARED — see 17 §C/§D]
   │  ├─ BackButton (Button: Plate + gold Arrow)   [note: this screen's arrow is a thinner "<" variant]
   │  ├─ TabBar (ToggleGroup ShopTabs)
   │  │  ├─ Tab_Spells (Toggle) → Icon + "SPELLS"
   │  │  ├─ Tab_Skins  (Toggle) → Icon + "SKINS"
   │  │  ├─ Tab_Chests (Toggle, SELECTED, gold-lit) → Icon(chest) + "CHESTS"
   │  │  └─ Tab_Store  (Toggle) → Icon + "STORE"
   │  └─ CurrencyChips
   │     ├─ GemChip  → violet gem + "1726" + green "+"
   │     └─ GoldChip → silver coin + "48570" + green "+"
   ├─ Keeper (Image, hooded figure, arms outstretched, glowing eyes, behind chest)
   ├─ FeaturedChest (group, center)
   │  ├─ Chest_AuraBeam (Image, vertical violet god-beam, additive)  [behind chest]
   │  ├─ Chest_Pedestal (Image, stone dais with gold/violet rune trim)
   │  ├─ Chest_Model (Image, ornate gold chest w/ amethyst gem on front)
   │  └─ Chest_AuraSwirl (Image, orbiting violet sparks/glow ring)  [in front/around]
   ├─ ChestSlots_Row (bottom-left, HorizontalLayoutGroup, 4 slots)
   │  ├─ Slot_1 (Button, ACTIVE, gold-lit frame) → blue chest + gem; TimerBar(green "02:59")
   │  ├─ Slot_2 (locked) → wood chest + padlock icon
   │  ├─ Slot_3 (locked) → wood chest + padlock icon
   │  └─ Slot_4 (empty, dark recessed slot)
   └─ OpenChestButton (Button, violet, bottom-right) → LockKeyIcon + Label "OPEN CHEST"
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **Shared chrome** — identical to `17 §D`; **Tab_Chests selected** (gold-lit). Currencies 1726 / 48570.
  (Back arrow on this mock is a thinner chevron; treat as the same Back button, art variant only.)
- **Keeper** — parent SafeAreaRoot; Image (dark silhouette w/ additive glowing eyes + faint hand-glow).
  Anchor **top-center** biased behind chest; rect ≈ x 0.36→0.64 W, y 0.10→0.62 H. Decorative (not a button);
  sits BEHIND FeaturedChest in draw order, IN FRONT of Bg.
- **FeaturedChest** — parent SafeAreaRoot; anchor **center** (0.5,0.5); rect ≈ x 0.33→0.67 W, y 0.12→0.78 H.
  Draw order back→front: Chest_AuraBeam (additive vertical beam) → Chest_Pedestal → Chest_Model →
  Chest_AuraSwirl (additive ring/sparks). May be a Button (tap featured to open) or purely visual with the
  OPEN CHEST button as the actuator — implement featured chest tap = same as OPEN CHEST.
- **ChestSlots_Row** — parent SafeAreaRoot; anchor **bottom-left** (0,0) pivot 0,0; rect x 0.020→0.30 W,
  y 0.02→0.27 H. Container + `HorizontalLayoutGroup` (4 equal cells, spacing≈10px, childAlignment
  MiddleCenter). Each Slot = framed cell:
  - **Slot_1 (active):** Button; gold-lit frame; chest art (silver/blue chest w/ blue diamond gem); a
    **TimerBar** at the bottom (green fill + white "02:59" countdown). Tapping = ready-progress info or
    gem-skip.
  - **Slot_2 / Slot_3 (locked):** dim wood-chest art + centered **padlock** icon overlay; non-interactive
    until earned/unlocked.
  - **Slot_4 (empty):** dark recessed empty slot (no chest).
- **OpenChestButton** — parent SafeAreaRoot; anchor **bottom-right** (1,0) pivot 1,0; rect x 0.71→0.985 W,
  y 0.04→0.20 H. Violet pill, gold-edged ornate frame; left **lock-and-key** icon (diamond gold badge) + label
  "OPEN CHEST". The primary CTA.

**Responsive:** chrome to corners. Featured chest centers; slot row bottom-left; CTA bottom-right → stable.
On ultrawide the crypt bg reveals more alcoves; chest stays centered. On 4:3 tablet, slot row + CTA may need a
slightly larger bottom inset; keep both fully in safe area (do not let the CTA overlap slot 4). Slot row can
become a `ScrollRect`/Grid if >4 slots ever exist.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Chrome:** as 17 §E (Back 0.045 W sq TL; TabBar 0.44 W centered TC; currencies right ~0.27 W TR).
- **Keeper:** x 0.36→0.64 (Δ0.28 W ≈655px) · y 0.10→0.62; glowing eyes focal ≈ (0.50 W, 0.22 H); hands at
  ≈ (0.37 W / 0.63 W, 0.45 H).
- **Featured chest:** x 0.33→0.67 (Δ0.34 W ≈796px) · y 0.12→0.78. Chest center ≈ (0.50 W, 0.50 H); amethyst
  gem on front ≈ (0.50 W, 0.52 H); aura beam vertical through center; pedestal base y ≈0.70→0.78.
- **Chest-slot row:** x 0.020→0.30 (Δ0.28 W ≈655px) · y 0.02→0.27 (Δ0.25 H ≈270px). 4 cells each ≈0.065 W,
  gaps ≈0.008 W; Slot_1 active gold-lit. TimerBar within Slot_1 at its bottom ≈0.05 H tall (green) with
  centered "02:59".
- **OPEN CHEST button:** x 0.71→0.985 (Δ0.275 W ≈643px) · y 0.04→0.20 (Δ0.16 H ≈173px). Violet pill; lock-key
  badge at left ≈0.05 W; label centered-right.
- **Notch/tablet/ultrawide:** SafeAreaFitter insets interactive layer; slots + CTA always inside; Keeper +
  chest may bleed at extremes. Bg full-bleed under cutout.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tabs / currencies / Back | (shared, 17 §F) | — | — | — | — | — | — |
| Slot timer "02:59" | Sans (mono-ish) | Bold | — | +2% | 24 | white on green bar, dark stroke | `#ffffff` on `#2faa3a` |
| OPEN CHEST (button) | Serif display | Black | UPPER | +5% | 34 | gold bevel text on violet, glow + dark outline | `#f6e6b8` on violet |
| (Locked slots) | (icon only — padlock) | — | — | — | — | gold/grey padlock glyph | `#cdb887` |

(There is no other body copy on this screen — the chest names/odds appear on the result screen 21, not here.)

---

## G · MATERIALS
- **Gold frames / chrome:** as 17 §G.
- **Featured chest:** ornate **cast-gold** chest `#caa04a`→`#f6d77a` with engraved filigree bands, a large
  **amethyst gem** on the front face `#7a3fd0`→`#cfa6ff` (faceted, glowing); lid clasps + corner bosses gold;
  warm gold rim + violet aura cast onto the metal.
- **Aura beam / swirl:** additive violet `#8a40d8`→`#cfa6ff` vertical god-beam + orbiting spark ring; soft,
  high-bloom, semi-transparent.
- **Pedestal:** dark stone dais `#2a2630`→`#46414e` with gold + violet **rune** trim (glowing faint violet);
  worn edges.
- **Keeper:** near-black hooded robe `#0a0a12`→`#1a1622` silhouette; **glowing violet/white eyes**
  `#cfa6ff`; faint violet hand-glow; rim-lit by the aura.
- **Active slot (Slot_1):** gold-lit frame (bright bevel + glow); chest art = **silver/steel + cobalt** chest
  `#9aa0ad`/`#1e3a8c` with a **blue diamond gem** `#4fa8ff`; green **TimerBar** `#2faa3a`→`#46d257`.
- **Locked slots:** dim **wood** chests `#5a4326`→`#7a5e34` with iron bands `#3a3a40`; centered gold/grey
  **padlock** icon; desaturated.
- **Empty slot:** dark recessed socket `#14131a`, faint inner gold edge.
- **OPEN CHEST button:** amethyst pill `#6a2db8`→`#9e6bf0`, bright top bevel + **gold ornate frame** (notched
  diamond ends), outer violet glow; left **lock-and-key** badge (gold diamond plate + violet padlock/key).
  Pressed darkens.
- **Background:** crypt of pillars + **gold-coin hoards** `#caa04a` glinting in side alcoves, violet ambient
  fog; deep vignette.

---

## H · COMPONENTS (states + feedback)
**Shop tabs / currency / Back** — as 17 §H; **Tab_Chests selected** (gold-lit).

**Chest slot (cell)**
- *active/unlocking (Slot_1):* gold-lit frame, chest art, green TimerBar counting down ("02:59"); tap → info
  / "unlock now for N gems" prompt. When timer hits 0 → "READY" glow + tap opens.
- *ready:* frame pulses gold, chest shakes/glints, "OPEN" affordance; tapping → reveal.
- *locked (Slot_2/3):* dim + padlock; non-interactive (tooltip "earn/unlock to fill"); tap → shake or info.
- *empty (Slot_4):* dark socket, non-interactive.
- *unlock-skip:* spending gems removes the timer → ready (server-auth).

**OPEN CHEST button**
- *idle (ready chest available):* violet pill, lock-key icon + "OPEN CHEST".
- *hover:* +8% glow, scale 1.03; key icon sparkle.
- *pressed:* 0.97 darken.
- *not-ready (only timed chests):* either disabled (45%) OR becomes "OPEN NOW · N💎" gem-skip (governed by
  data/ADR). On click when ready → open animation → route to 21.
- *opening:* button → spinner; featured chest plays the open burst → screen transition to Chest Open Result.
- *insufficient (for gem-skip):* red shake → insufficient modal (37).

**Featured chest (tap = same as OPEN CHEST)**
- *idle:* aura beam + swirl loop, gentle bob, gem glint.
- *hover/press (if interactive):* chest lifts slightly, aura brightens.
- *opening:* lid bursts, violet light-burst → cut to 21.

---

## I · ANIMATION TIMELINE
- **OnShow (0→0.55s):** Bg fade (0.25) → chrome slide-down (0–0.25) → aura beam fade-in (0.1–0.4) → Keeper
  fade-in behind (0.1–0.35) → FeaturedChest scale 0.92→1 + glint (0.15–0.45) → ChestSlots stagger pop
  left→right 40ms (0.2–0.45) → OPEN CHEST slide from right +24px + glow ignite (0.25–0.5).
- **Idle loops:** featured chest bob (±5px, 2.6s) + aura beam opacity breathe (0.75↔1.0, 2.2s) + swirl ring
  rotate (slow, 8s) + gem specular twinkle; Keeper eye glow flicker (1.6s); Slot_1 TimerBar ticks every 1s,
  "02:59→02:58…"; gold-coin glints in bg alcoves (random).
- **Timer countdown:** numeric tick each second; at 0 → green→gold flash + "READY" + chest shake.
- **Open sequence (~1.0s):** chest lifts + aura intensifies (0.0–0.3) → lid bursts open + violet light-burst +
  particle explosion (0.3–0.6) → white/violet flash full-screen wipe (0.6–0.8) → route to Chest Open Result
  (21) under the flash (0.8–1.0).
- **Insufficient (gem-skip):** OPEN CHEST + gem chip red shake (0.3) → modal.

Easing: bob/breathe sine; pops ease-out back 0.2; open burst ease-out 0.3 then sharp flash.

---

## J · PARTICLE & FX
- Featured chest: violet god-beam (additive) + orbiting spark ring + rising amethyst sparkles + gem glint +
  floating dust motes in the beam.
- Keeper: faint violet wisps around the hands; eye-glow bloom.
- Active slot ready: gold sparkle + frame pulse.
- Open burst: large violet/gold particle explosion + radial light rays + screen-flash → transition.
- Bg: drifting violet fog + gold-coin glints in alcoves.
- Global: heavy vignette + strong bloom (the most bloom-heavy shop tab).

---

## K · EVENT BEHAVIOR
- **OnShow:** bind chest inventory (slots[]: state=active/locked/empty, chestType, unlockEndTime), featured
  chest definition, currencies — all server-auth. Start client-side countdown synced to server end-time.
- **OnSlotTap(slot):** if ready → open flow; if unlocking → show "unlock now (N gems)" prompt; if locked/empty
  → info/no-op.
- **OnOpenChest / OnFeaturedTap:** if ready → play open sequence → call server to roll rewards (server-auth
  RNG; client never rolls) → on response route to **Chest Open Result (21)** with the granted rewards.
- **OnGemSkip(slot):** server spends gems, marks ready (client never mutates balance, §12); insufficient →
  modal (37).
- **OnTimerComplete(slot):** flip slot to ready (server-confirmed).
- **OnTabSelect / OnBack / OnAdd "+":** as 17.

---

## L · NEGATIVE RULES
- Forensic state binding: **slot 1 active w/ timer "02:59"**, **slots 2–3 locked (padlock)**, **slot 4 empty**;
  featured **gold chest w/ amethyst gem** on a runed pedestal; hooded **Keeper** behind; **OPEN CHEST** violet
  CTA with lock-key icon; currencies **1726 / 48570**; **CHESTS** tab selected.
- Keep OPEN CHEST **violet** with the **lock-and-key** badge; keep the **4-slot** row order/states as drawn.
- Do NOT move chrome (Back TL, tabs TC, currencies TR). Bg full-bleed under cutout; slots + CTA in safe area.
- **§12 / server-auth RNG:** the **client never rolls loot** and never mutates balances; the server returns
  the result → screen 21 displays it. **No gameplay/ECS/balance change.**
- **ADR flag (do not alter spec):** timed chests + random rewards = loot-box/gacha, colliding with the "no
  loot boxes" cut → ADR required (transparent odds / fair redesign / or CUT). Spec **exactly as drawn**; the
  ADR governs implementation. Do NOT redesign the chest loop in this forensic spec.
- Invent nothing (no fabricated chest names, odds, or rarities on this screen — those live on 21/data).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Shared chrome correct; **CHESTS** tab selected; Gems 1726 / Gold 48570 with "+".
2. Center: gold featured chest with amethyst gem, violet aura beam + swirl, on a runed pedestal.
3. Hooded Keeper silhouette with glowing eyes/arms behind the chest.
4. Bottom-left 4-slot row: Slot_1 active gold-lit w/ green "02:59" timer; Slots 2–3 locked (padlock); Slot_4
   empty.
5. Bottom-right violet **OPEN CHEST** button with lock-and-key icon — brightest CTA.
6. Palette: amethyst aura everywhere + gold chest/frames + crypt bg, heavy vignette/bloom.
7. Fraction layout within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet.
8. Timer countdown, open sequence → route to 21, and insufficient feedback per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**91/100.** Layout, states, timer, and CTA are clear. Minus: (a) the featured chest + aura + Keeper are a
bespoke lit render (rebuild as layered sprites + additive beam/particles); (b) the open→reveal transition is a
multi-stage FX sequence (approximate); (c) exact slot-2/3 chest art + pedestal runes approximated; (d) the
ADR may change the whole loot loop — but the **visual** spec is firm.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (topbar, featured chest, slot row + timer, OPEN CHEST button) before
  writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree incl. shared tab bar + currency chips + back.
- [x] Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Forensic states/timer/colors recorded; nothing invented.
- [x] §12 / server-auth-RNG + **ADR loot-box flag** noted in A/L; spec NOT altered by the flag.
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.
