# BULWARK — UI CONSTRUCTION SPEC · 21 · Chest Open Result

Source: design/ChestOpenResultDesign.png · 1536×1024 (1.5:1) · Analysis-only forensic spec.

> **Reward-reveal overlay** that follows **Chests (20)** after an open animation. Unlike the four shop tabs,
> this screen has **no shop tab bar and no top currency chips** — it is a focused full-screen "you got…"
> celebration with a single **COLLECT** CTA. It still belongs to the shop/reward family and shares the global
> dark-gold + amethyst DNA. **ADR-flagged** (gacha loot reveal — see 20).

---

## A · SCREEN PURPOSE
The **Chest Open Result** reveals the loot rolled from a chest. An open gold chest erupts with a violet light
burst at the top; below, the **reward cards** (here four) display each granted item with its **count**; the
rarest/headline reward (a **LEGENDARY** cosmetic) is presented as a larger, gold-rarity-bordered hero card on
the right. A single **COLLECT** button banks the rewards and returns the player to the shop/hub. The screen's
job is the dopamine beat: build-up burst → cards reveal one-by-one → COLLECT.

Shown rewards: **Silver ×500**, **Gems ×40**, **Cosmetic Shards ×15**, and **LEGENDARY — LIONHELM (Commander
Helmet)** (the headline, no count).

**ADR note (Section L):** this is the **payoff screen of the loot-box/gacha loop** → bound by the same "no
loot boxes" ADR as Chests (20). The reward content is **server-authoritative** (the client never rolls). Spec'd
**exactly as drawn**; ADR governs implementation.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** triumphant reveal in the treasure crypt — a **violet/gold light explosion** from the open chest is
  the hero light; everything else is near-black so the burst + cards pop. Celebration > navigation.
- **Palette anchors:** amethyst/violet burst (`#7a3fd0`–`#cfa6ff`) + warm gold rays (`#e8c25a`–`#fff0b8`);
  rarity-coded cards (common silver/grey, rare violet, legendary **gold radiant**); **COLLECT button is
  cobalt/royal-blue**, gold-edged (bank/confirm, not premium-spend).
- **Lighting:** central radial light-burst behind the chest + god-rays; per-card rim light; the LEGENDARY card
  has a strong gold rarity glow + sparkle aura; deep vignette around the edges; heavy bloom on the burst.
- **Background:** full-bleed dim crypt (same world as 20), almost fully darkened by the vignette so the burst
  dominates; bleeds under cutout.
- **Hierarchy:** Title "CHEST REWARDS" (top) → open-chest light-burst (focal) → reward cards (the payoff, with
  the LEGENDARY card emphasized) → COLLECT (bottom CTA).

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
ChestOpenResultScreen (UiScreen root / overlay, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, dark crypt, heavily vignetted)  [OUTSIDE safe area]
   │  ├─ Bg_Vignette (Image, strong radial)
   │  └─ Burst_Rays (Image, radial god-ray/light-burst, additive)  [behind chest]
   ├─ Header (top-center)
   │  ├─ Title    (Text "CHEST REWARDS")
   │  └─ Subtitle (Text "Your treasures await!")
   ├─ OpenChest_Hero (group, upper-center)
   │  ├─ Chest_Open (Image, ornate gold chest, lid open)
   │  └─ Burst_Core (Image, violet light-core + sparks erupting upward, additive)  [in front]
   ├─ RewardCards_Row (HorizontalLayoutGroup, 4 cards)
   │  ├─ Card_1 "SILVER"          (common)    → silver-coin-stack icon + "×500"
   │  ├─ Card_2 "GEMS"            (rare)      → amethyst-crystals icon + "×40"
   │  ├─ Card_3 "COSMETIC SHARDS" (rare)      → gold-shard icon + "×15"
   │  └─ Card_4 (LEGENDARY, larger, gold-radiant frame)
   │      ├─ Rarity_Label (Text "LEGENDARY")
   │      ├─ Item_Name    (Text "LIONHELM")
   │      ├─ Item_Sub     (Text "COMMANDER HELMET")
   │      └─ Item_Art     (Image, blue-plumed gold lion helm)   [no count — unique item]
   └─ CollectButton (Button, cobalt-blue, bottom-center) → Label "COLLECT"
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **ChestOpenResultScreen** — parent UiRouter content (pushed as a result/overlay over Chests); `UiScreen` +
  `CanvasGroup`. Anchors stretch full. **No tab bar, no currency chips, no Back** (the only exit is COLLECT;
  optionally a tap-anywhere-to-skip-reveal). Built in code.
- **SafeAreaRoot** — `SafeAreaFitter`; interactive content parents here. **Bg_FullBleed + Burst_Rays OUTSIDE**
  safe area (true 0..1) so the burst bleeds edge-to-edge under the notch.
- **Burst_Rays** — parent Bg (not SafeAreaRoot); Image; anchor center, large (≥1.2× screen); additive; slow
  rotate. **Bg_Vignette** on top of Bg, multiply.
- **Header** — parent SafeAreaRoot; anchor **top-center** (0.5,1) pivot 0.5,1; y inset −0.04 H. Vertical
  mini-group: Title (big serif) over Subtitle (small).
- **OpenChest_Hero** — parent SafeAreaRoot; anchor **upper-center** (0.5,1) pivot 0.5,1; rect centered at
  ≈ (0.5 W, 0.30 H), size ≈ 0.28 W × 0.30 H. Draw order: Burst_Core (additive, behind/through chest) +
  Chest_Open. Decorative.
- **RewardCards_Row** — parent SafeAreaRoot; anchor **center** (0.5,0.5) biased lower; rect x 0.10→0.90 W,
  y 0.42→0.82 H. `HorizontalLayoutGroup` (spacing≈18px@1080, childAlignment MiddleCenter,
  childForceExpandWidth=false — the LEGENDARY card is wider). Card_1..3 equal width; **Card_4 ≈1.25× width**
  and slightly taller (hero rarity card). For ScrollRect safety if a chest can yield >4 cards, wrap row in a
  horizontal `ScrollRect` (disabled when ≤4, centered).
- **Common/Rare card (1–3)** — vertical stack: [Rarity-tinted Frame] → Name (top) → Art (center) → CountRow
  ("×N", bottom). VerticalLayoutGroup inside, controlled spacing.
- **Legendary card (4)** — taller frame with gold radiant border + sparkle; stack: Rarity_Label "LEGENDARY"
  (top) → Item_Name "LIONHELM" → Item_Sub "COMMANDER HELMET" → Item_Art (lion helm, large). **No count.**
- **CollectButton** — parent SafeAreaRoot; anchor **bottom-center** (0.5,0) pivot 0.5,0; y inset +0.05 H;
  rect width ≈0.34 W × 0.085 H. Cobalt pill, gold-edged. Primary (and effectively only) CTA.

**Responsive:** header top-center, chest upper-center, cards center, COLLECT bottom-center — all centered →
trivially stable on any aspect. The burst bg fills/bleeds. On ultrawide the cards row keeps fixed card sizes
and centers (extra gutter splits). On 4:3 tablet the 4 cards may grow; if too tight, allow 2×2 wrap
(GridLayoutGroup fallback) with the LEGENDARY card spanning/centered. SafeAreaFitter insets cards + COLLECT;
burst unaffected.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
Source mock is 1.5:1 (portrait-ish); **normalize to 2340×1080 landscape** — the layout is a vertical stack
(Title → chest → cards → COLLECT) that maps cleanly to a centered landscape column with the cards spread wider.

- **Header:** Title baseline at y ≈0.08 H (top-center); Subtitle y ≈0.13 H. Title block width ≈0.5 W centered.
- **Open-chest hero:** center ≈ (0.50 W, 0.30 H); chest+burst envelope ≈ x 0.36→0.64 W, y 0.13→0.42 H. Burst
  rays radiate from ≈ (0.50 W, 0.30 H).
- **Reward cards row:** band x 0.10→0.90 (Δ0.80 W ≈1872px) · y 0.46→0.82 (Δ0.36 H ≈389px).
  - Cards 1–3 each ≈0.165 W (≈386px) × 0.34 H; gaps ≈0.022 W.
  - **Card_4 (legendary)** ≈0.205 W (≈480px) × 0.40 H (taller), right end of the row, vertically centered on
    the band (slightly overhangs top/bottom of the common cards).
  - Inside common card: Name y ≈0.50; Art center ≈0.62; "×N" ≈0.77.
  - Inside legendary: LEGENDARY y ≈0.48; LIONHELM y ≈0.52; COMMANDER HELMET y ≈0.55; helm art center ≈0.66.
- **COLLECT button:** x 0.33→0.67 (Δ0.34 W ≈796px) centered · y 0.05→0.135 H from bottom (pill ≈0.085 H tall).
- **Notch/tablet/ultrawide:** SafeAreaFitter insets the centered column; burst bg bleeds full. Everything is
  center-anchored → no edge collisions.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Title "CHEST REWARDS" | Serif display | Black | UPPER | +4% | 56 | heavy gold bevel + bloom + outer dark stroke | `#f0cf72`→`#caa04a` grad |
| Subtitle "Your treasures await!" | Serif italic | Regular | Title | +2% | 24 | soft gold, slight glow | `#e3d3a2` cream-gold |
| Card name (SILVER/GEMS/COSMETIC SHARDS) | Serif display | Semibold | UPPER | +3% | 26 (2-line for "COSMETIC SHARDS" ~22) | gold bevel + shadow | `#ecd591` |
| Card count "×500 / ×40 / ×15" | Sans | Bold | — | +2% | 30 | white, dark stroke; "×" slightly smaller | `#ffffff` |
| Legendary "LEGENDARY" | Sans condensed | Black | UPPER | +6% | 22 | bright gold, glow (rarity tag) | `#ffdf8a` |
| Legendary "LIONHELM" | Serif display | Bold | UPPER | +2% | 30 | gold bevel + bloom | `#f3d98a` |
| Legendary "COMMANDER HELMET" | Sans condensed | Semibold | UPPER | +4% | 16 | muted gold subtitle | `#cdb887` |
| COLLECT (button) | Serif display | Bold | UPPER | +5% | 40 | gold bevel text on cobalt, glow + outline | `#f3e6c0` on blue |

---

## G · MATERIALS
- **Burst rays / core:** additive radial **violet→white** light-burst `#8a40d8`→`#cfa6ff`→`#ffffff` plus warm
  **gold ray spokes** `#e8c25a`→`#fff0b8`; high bloom, soft edges, slow rotation; the dominant light.
- **Open gold chest:** ornate **cast-gold** `#caa04a`→`#f6d77a` with filigree, lid open, blue-gem accent on
  the front (matches 20's featured chest); violet light pours from inside; gold rim + violet inner glow.
- **Common card (Silver):** dark slate card `#14161e`→`#222633` with a cool **silver** rarity edge `#aab0bc`;
  art = stacked **silver coins** w/ embossed star `#c4c8d0`→`#eef0f4`, cool specular.
- **Rare cards (Gems, Cosmetic Shards):** dark card with a **violet** rarity edge `#7a3fd0`; Gems art =
  **amethyst crystal cluster** `#8a40d8`→`#cfa6ff` glowing; Shards art = **gold helm-shard / crest** fragment
  `#caa04a`→`#f6d77a` with violet edge glints.
- **Legendary card:** dark card with a **radiant gold rarity frame** `#e8c25a`→`#fff0b8` + animated sparkle
  aura + corner light flares; art = **gold lion helm with a cobalt-blue plume** `#1f3fb0`→`#4f8bff` crest, gold
  face `#caa04a`→`#f6d77a`, glowing eyes; strongest glow on screen. (No count — unique cosmetic.)
- **COLLECT button:** royal/cobalt blue `#1f3fb0`→`#3a63d0`, bright top bevel + **gold ornate edge** (notched
  corners), outer glow; pressed darkens.
- **Background:** dim crypt `#0a0b0f`→`#161a24`, almost fully vignetted to black at the edges; subtle gold
  hoard glints far behind the burst.

---

## H · COMPONENTS (states + feedback)
This screen is mostly **presentational** (rewards are already decided server-side); interactivity = reveal
pacing + COLLECT.

**Reward card (display, with reveal state)**
- *hidden (pre-reveal):* card scaled 0 / face-down or behind the burst.
- *revealing:* flips/scales up 0→1 with a rarity-colored flash; count "×N" counts up quickly; rarer cards get
  a bigger flash + sparkle.
- *revealed/idle:* card settled; rare/legendary cards keep a gentle glow loop; legendary keeps sparkle aura +
  slow frame shimmer.
- *(non-interactive otherwise; optional hover tooltip = item description.)*

**Legendary card** — same reveal but with the biggest flash, a gold radiant ring sweep, a brief slow-mo beat,
and a persistent sparkle aura (signals the chase reward).

**COLLECT button**
- *disabled (during reveal):* dimmed until all cards revealed (or tap-to-skip reveals instantly then enables).
- *idle (ready):* cobalt pill "COLLECT", gentle glow.
- *hover:* +8% glow, scale 1.03.
- *pressed:* 0.97 darken.
- *on collect:* rewards fly to their HUD destinations (silver/gems → currency, items → inventory) + sparkle;
  screen fades out → returns to Chests/hub; wallet reflects server-confirmed grant.

**Tap-anywhere (optional):** during the staggered reveal, a tap **fast-forwards** all cards to revealed and
enables COLLECT (standard reward-screen affordance).

---

## I · ANIMATION TIMELINE
- **OnShow / open burst (0→0.6s):** inherits the flash from Chests' open sequence → Bg + heavy vignette in
  (0–0.2) → Burst_Rays bloom up + rotate begins (0.0–0.4) → Chest_Open pops in with Burst_Core eruption
  (0.1–0.4) → Title "CHEST REWARDS" scales 0.9→1 + bloom + Subtitle fade (0.2–0.5).
- **Card reveal cascade (0.5→1.6s):** cards reveal left→right, **rarest last** — Card_1 (0.55), Card_2 (0.75),
  Card_3 (0.95), then a beat, **Card_4 LEGENDARY (1.2)** with a bigger flash + gold ring sweep + brief
  slow-mo + sparkle aura. Each: scale 0→1 ease-out-back (0.25) + rarity flash (0.15) + count-up (0.3).
- **COLLECT enable (~1.6s):** button fade from dim→bright + slight bounce; (a tap before this fast-forwards
  the cascade).
- **Idle loops (post-reveal):** Burst_Rays slow rotate + opacity breathe (2.4s); legendary sparkle aura +
  frame shimmer (2s); rare cards faint glow breathe; floating motes in the burst.
- **OnCollect (0→0.5s):** reward icons arc to destinations (0.0–0.35, ease-in) + sparkle + COLLECT bump
  (1.0→1.1→1.0) → screen CanvasGroup fade-out (0.3–0.5) → pop back to Chests/hub.

Easing: reveals ease-out-back 0.25; burst ease-out; collect arcs ease-in 0.35; fades 0.3–0.5.

---

## J · PARTICLE & FX
- Central: large additive violet+gold **light-burst** + rotating ray spokes + upward **spark fountain** from
  the open chest + drifting embers/motes; high global bloom.
- Per-card reveal: rarity-colored flash ring + small sparkle puff (bigger for legendary).
- Legendary: persistent gold sparkle aura + corner light flares + a one-shot radiant ring sweep on reveal.
- OnCollect: reward-icon projectiles with trailing sparkles toward HUD; final twinkle.
- Edge vignette keeps focus on the burst/cards.

---

## K · EVENT BEHAVIOR
- **OnShow(rewards[]):** receives the **server-authoritative** reward list from the chest open (id, type,
  count, rarity, art) — the **client never rolls**. Play burst + staggered reveal in rarity order.
- **OnTapSkip:** fast-forward all reveals to final state; enable COLLECT.
- **OnRevealComplete:** enable COLLECT.
- **OnCollect:** confirm grant with server (already granted at roll-time; this banks/acks) → animate rewards to
  destinations → update wallet/inventory from server state → fade out → return to Chests (20) / hub.
- **No Back, no tab bar, no currency "+":** COLLECT (or tap-skip then COLLECT) is the only exit.
- **OnReconnect/already-collected:** if re-entered, show as collected / dismiss gracefully (idempotent).

---

## L · NEGATIVE RULES
- Forensic copy/counts binding: Title **"CHEST REWARDS"**, subtitle **"Your treasures await!"**; cards
  **SILVER ×500**, **GEMS ×40**, **COSMETIC SHARDS ×15**, **LEGENDARY / LIONHELM / COMMANDER HELMET** (no
  count); CTA **COLLECT**.
- The legendary card has **no ×count** (it's a unique item) — do not add one. Keep card **rarity order/emphasis
  as drawn** (legendary largest, right end, gold-radiant).
- **No tab bar, no top currency chips, no Back** on this screen — do not add shop chrome here.
- Keep **COLLECT cobalt-blue** (confirm/bank), not violet.
- Bg + burst full-bleed under cutout; cards + COLLECT in safe area, centered.
- **§12 / server-auth RNG:** the **client never rolls or fabricates rewards**; it only **displays** the
  server-returned list and acks COLLECT. Wallet/inventory update from server state. **No gameplay/ECS/balance
  change.**
- **ADR flag (do not alter spec):** this is the gacha payoff screen → bound by the same loot-box ADR as Chests
  (20). Spec **exactly as drawn**; the ADR governs the loot model, not this reveal's visuals.
- Invent nothing (no extra reward cards, no fabricated counts/rarities beyond the four shown).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Top: gold "CHEST REWARDS" title + "Your treasures await!" subtitle.
2. Upper-center: open gold chest with a violet+gold light-burst / ray fountain (focal).
3. Four reward cards in order: SILVER ×500, GEMS ×40, COSMETIC SHARDS ×15, and a larger gold-radiant LEGENDARY
   LIONHELM (Commander Helmet) card with **no count**.
4. Bottom-center cobalt **COLLECT** button (gold-edged) — the only exit; **no tab bar / currencies / Back**.
5. Palette: violet+gold burst on near-black, rarity-coded cards, deep vignette + heavy bloom.
6. Fraction layout within ±2% at 2340×1080 (centered column); stable on notch/ultrawide/tablet.
7. Staggered rarity-order reveal (legendary last/biggest), tap-skip, COLLECT-banks-then-returns per H/I/K.
8. Rewards are server-authoritative display-only (client never rolls).

## N · IMPLEMENTATION CONFIDENCE
**93/100.** Copy, counts, rarity treatment, and layout are crisp. Minus: (a) the central light-burst + ray
fountain is a custom additive/particle composite (approximate with rays sprite + particle systems + bloom);
(b) the LEGENDARY lion-helm art + radiant frame are bespoke (use sprite + animated gold frame); (c) exact
reveal timing/slow-mo is a feel choice (values given are a faithful estimate). No interactive-fidelity risk.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (title, 4 cards, legendary card, COLLECT) before writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree documented; explicitly notes the **absence** of the shared tab bar / currency chips / back
  (this is a reveal overlay, not a shop tab).
- [x] Fraction layout normalized to 2340×1080 (1.5:1 source → centered landscape column); tablet/ultrawide/
  notch covered.
- [x] Forensic copy/counts/rarities/colors recorded; nothing invented (legendary has no count).
- [x] §12 / server-auth-RNG (display-only) + **ADR gacha flag** noted in A/L; spec NOT altered by the flag.
- [x] Landscape, full-bleed-burst-under-cutout, content-in-safe-area enforced.
