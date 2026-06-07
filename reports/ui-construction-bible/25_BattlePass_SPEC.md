# BULWARK — UI CONSTRUCTION SPEC · 25 · Battle Pass

Source: design/BattlePassDesign.png · 1774×887 (2.0:1) · Analysis-only forensic spec.

> Normalize the source 1774×887 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" = on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Battle Pass** seasonal-track meta/live-ops screen — season **"SEASON OF GLORY"**. Layout:
1. **Header banner:** ornate title "SEASON OF GLORY" on hanging drapes, a season timer **"Ends in: 28d 14h"**,
   currency chips top-right (**Gems 2,450**, **Gold 98,760**).
2. **Progress strip (left):** a season **level badge "23"**, label **"Battle Pass XP"**, an XP bar
   **"650 / 1,000 XP"** with a next-level arrow to **"24"**, and a **"Missions"** button.
3. **Tier track (center):** a horizontally-scrolling two-row reward track — **FREE** (top row) and **PREMIUM**
   (bottom row) — with numbered tier columns **19 20 21 22 23 24 25 …** and a final highlighted **tier 30**
   premium showcase card on the right.
4. **Right showcase card:** a tall **"BATTLE PASS"** card displaying the tier-**30** legendary premium reward
   (locked/featured).
5. **Bottom CTA bar:** **"UNLOCK PREMIUM — Get premium rewards and exclusive perks!"** with three perk chips
   (**+20% XP Boost**, **+20% Gold Bonus**, **Exclusive Rewards**) and the **"UNLOCK PREMIUM 999"** (gems) CTA.

Purpose: show season progress, free vs premium rewards per tier, claim earned rewards, and upsell the premium
pass. Server-authoritative meta. Reached from: Main Menu / live-ops. Back: top-left chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** royal seasonal festival — purple-and-gold regalia, hanging banners/drapes, a treasure-laden track.
  Premium = amethyst/violet luxury; free = steel/blue restraint.
- **Palette anchors:**
  - BG: dark battlefield-at-dusk `#0a0b0f → #1a1320` with a faint ruined-siege vista; vignette.
  - Gold chrome (title frame, track frame, tier numbers): `#6b5320 → #caa04a → #f0d27a`.
  - **Premium violet/amethyst:** `#5a2db0 → #7e3fd6 → #b07cf0`, glow `#c79bff` — drapes, premium row,
    tier-30 card, UNLOCK PREMIUM CTA accents.
  - Free-row steel/blue: `#2b3a5a → #3d6fb0`, cool.
  - Reward icons: gems (violet crystal `#9e6bf0`), gold coins (`#f0c14a`), chests (wood+gold), books/cosmetics.
  - Claimed check: green `#5fd35a`.
- **Hierarchy:** SEASON OF GLORY title → tier track → tier-30 showcase → UNLOCK PREMIUM CTA → progress strip
  → currencies.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
BattlePassScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_SiegeDusk (Image, purple-tinted battlefield vista)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ Header
│  │  ├─ Btn_Back (Button) [chevron] top-left
│  │  ├─ TitleBanner
│  │  │  ├─ Drapes (Image, hanging purple banners)
│  │  │  └─ Lbl_Title "SEASON OF GLORY" (Text, serif gold)
│  │  ├─ Lbl_Timer "Ends in: 28d 14h ⏳" (Text + hourglass icon)
│  │  └─ CurrencyChips (HorizontalLayoutGroup) top-right
│  │     ├─ Chip_Gems [gem | "2,450" | Btn_Plus]
│  │     └─ Chip_Gold [coin | "98,760" | Btn_Plus]
│  ├─ ProgressStrip (left band)
│  │  ├─ Badge_SeasonLevel "23" (Image shield/medallion + Text)
│  │  ├─ Lbl_XPTag "Battle Pass XP"
│  │  ├─ XPBar (Slider) [Fill + Lbl "650 / 1,000 XP"]
│  │  ├─ Lbl_NextLevel "24" (with arrow ➜)
│  │  └─ Btn_Missions (Button) [icon | "Missions"]
│  ├─ TrackScroll (ScrollRect, HORIZONTAL, masked viewport)
│  │  └─ TrackContent (RectTransform)
│  │     ├─ RailLabels (left, fixed/sticky)
│  │     │  ├─ Lbl_Free "FREE" (+ shield icon)
│  │     │  └─ Lbl_Premium "PREMIUM" (+ premium crest, 🔒)
│  │     ├─ TierColumns (HorizontalLayoutGroup)
│  │     │  ├─ TierColumn_19 [ Lbl "19" | FreeReward | PremiumReward ]
│  │     │  ├─ TierColumn_20 [ "20" | gem×25 | coin×100 ]
│  │     │  ├─ TierColumn_21 [ "21" | ✓claimed | reward ]
│  │     │  ├─ TierColumn_22 [ "22" | ×50 | ? mystery ]
│  │     │  ├─ TierColumn_23 [ "23" (CURRENT) | chest×1 | chest ]
│  │     │  ├─ TierColumn_24 [ "24" | coin×15,000 | ×200 ]
│  │     │  └─ TierColumn_25 [ "25" | gem×50 | ×1 ]
│  │     │     (each Reward cell:)
│  │     │     ├─ Cell_Frame (Image, free=steel / premium=violet, 🔒 if locked)
│  │     │     ├─ Reward_Icon (Image)
│  │     │     ├─ Reward_Qty (Text, e.g. "10,000")
│  │     │     └─ Claimed_Check (Image ✓ — when claimed)
│  │     └─ Showcase_Tier30 (tall card, right end)
│  │        ├─ Lbl_BattlePass "BATTLE PASS" (vertical/stacked)
│  │        ├─ Lbl_Tier "30"
│  │        └─ Reward_LegendaryArmor (Image, premium tier-30 reward)
│  └─ BottomCTABar (HorizontalLayoutGroup)
│     ├─ Lbl_UnlockTitle "UNLOCK PREMIUM"
│     ├─ Lbl_UnlockSub "Get premium rewards and exclusive perks!"
│     ├─ PerkChips (HorizontalLayoutGroup)
│     │  ├─ Perk_XP   [icon | "+20% XP Boost"]
│     │  ├─ Perk_Gold [icon | "+20% Gold Bonus"]
│     │  └─ Perk_Excl [icon | "Exclusive Rewards"]
│     └─ Btn_UnlockPremium (Button, gold/violet) "UNLOCK PREMIUM  💎 999"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | grows |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| TitleBanner | Header | 1 | RectTransform | top-center | .5,1 | center | inside | centered |
| Lbl_Timer | Header | 2 | Text | top-center (below title) | .5,1 | center | inside | centered |
| CurrencyChips | Header | 3 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-pinned |
| ProgressStrip | SafeAreaRoot | 1 | RectTransform | upper-stretch (below header) | .5,1 | left | inside | spans width |
| Badge_SeasonLevel | ProgressStrip | 0 | Image+Text | left | 0,.5 | center | inside | fixed |
| Lbl_XPTag | ProgressStrip | 1 | Text | left (right of badge) | 0,.5 | left | inside | — |
| XPBar | ProgressStrip | 2 | Slider(no handle) | center | .5,.5 | mid | inside | flex width |
| Lbl_NextLevel | ProgressStrip | 3 | Text | right of bar | 0,.5 | center | inside | fixed |
| Btn_Missions | ProgressStrip | 4 | Button | right | 1,.5 | center | inside | fixed |
| TrackScroll | SafeAreaRoot | 2 | ScrollRect (horizontal only) | center-stretch | .5,.5 | — | inside | spans width, fixed height |
| TrackContent | TrackScroll/Viewport/Content | 0 | RectTransform | left-stretch (vertical) | 0,.5 | — | inside | width grows with tiers |
| RailLabels | TrackContent | 0 | RectTransform | left-stretch (sticky) | 0,.5 | — | inside | fixed left, optionally pinned |
| TierColumns | TrackContent | 1 | HorizontalLayoutGroup | left-stretch | 0,.5 | upper-left | inside | columns flow right |
| TierColumn_* | TierColumns | 0..n | RectTransform | — | — | center | inside | uniform col width |
| Cell_Frame/Reward_*/Claimed_Check | (cells) | — | Image/Text | stretch(inset) | .5,.5 | center | inside | scales w/ cell |
| Showcase_Tier30 | TrackContent | 2 | RectTransform(framed) | right | 1,.5 | center | inside | taller card, right end |
| BottomCTABar | SafeAreaRoot | 3 | HorizontalLayoutGroup | bottom-stretch | .5,0 | — | inside | spans width |
| Lbl_UnlockTitle/Sub | BottomCTABar | 0,1 | Text | left | 0,.5 | left | inside | left cluster |
| PerkChips | BottomCTABar | 2 | HorizontalLayoutGroup | center | .5,.5 | center | inside | 3 chips |
| Btn_UnlockPremium | BottomCTABar | 3 | Button | right | 1,.5 | center | inside | wide CTA right |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Header:** y 0.86 → 1.00.
- Btn_Back: center x 0.030, y 0.955; ≈0.045w×0.085h.
- TitleBanner "SEASON OF GLORY": centered x 0.50, drapes span x 0.30→0.70 hanging from top; title baseline
  y≈0.945, cap ≈0.060h (≈65px@1080), wide tracking.
- Lbl_Timer "Ends in: 28d 14h ⏳": centered x 0.50, y≈0.875, ≈22px.
- CurrencyChips: right edge x 0.972, y 0.955; two chips (Gems, Gold) ≈0.115w×0.052h, gap 0.010w.

**Progress strip:** band y 0.790 → 0.855, spans x 0.030 → 0.760.
- Badge_SeasonLevel "23": left, x≈0.040, a gold shield/medallion Ø≈0.060w (≈140px), "23" centered.
- Lbl_XPTag "Battle Pass XP": x≈0.110, top of bar (≈22px).
- XPBar: x 0.110 → 0.560, y≈0.805 (height ≈0.030h ≈32px), violet/gold fill at 65% (650/1,000); centered text
  "650 / 1,000 XP".
- Lbl_NextLevel "24" with ➜ arrow: just right of the bar, x≈0.575, in a small gold node (Ø≈0.038w).
- Btn_Missions: right, x 0.640 → 0.760, y≈0.805, gold pill with scroll icon + "Missions" (≈26px).

**Tier track:** region y 0.230 → 0.770 (height ≈0.540h ≈583px@1080), x 0.020 → 0.840 (the right ~0.16 is the
tier-30 showcase). Horizontal ScrollRect.
- **RailLabels** (left, sticky): "FREE" (with shield icon) at the vertical center of the top row (y≈0.640),
  "PREMIUM" (with crest + 🔒) at the center of the bottom row (y≈0.380); label column ≈0.075w wide.
- **TierColumns:** start x≈0.110. **7 visible columns (19–25)**. Column width ≈0.090w (≈211px@1080),
  gap ≈0.006w. Each column:
  - Tier number ("19".."25") in a small node at the **top** of the column, y≈0.730 (≈26px gold; current "23"
    highlighted/larger).
  - **Free cell** (top row): centered y≈0.620, cell ≈0.080w × 0.150h (≈187×162@1080), steel-blue frame.
  - **Premium cell** (bottom row): centered y≈0.380, same size, violet frame (🔒 if pass not owned).
  - Each cell: reward icon centered, quantity text at the cell bottom (e.g. "10,000", "25", "50", "15,000",
    "5,000", "100", "200"); a green ✓ overlay if claimed (tiers 21 show ✓ on both rows; 22 premium shows "?"
    mystery).
- **Showcase_Tier30** (right end): tall card x 0.850 → 0.982, y 0.235 → 0.765 (spans both rows' height),
  violet ornate frame; "BATTLE PASS" stacked at the top, "30" tier number, a legendary armored-king reward
  render filling the card; strong premium glow (featured/locked).

(Free-row sample contents L→R: 19=10,000 gold; 20=25 gems; 21=✓; 22=50; 23=chest×1; 24=15,000 gold; 25=50 gems.
Premium-row L→R: 19=5,000; 20=100; 21=✓; 22=? mystery; 23=chest; 24=200; 25=×1. Transcribe icons/qty as drawn;
where a glyph is ambiguous render the icon shown and the legible number.)

**Bottom CTA bar:** band y 0.020 → 0.110, spans x 0.020 → 0.982; dark gold-framed bar.
- Lbl_UnlockTitle "UNLOCK PREMIUM": left, x≈0.060, y≈0.085 (≈34px serif gold).
- Lbl_UnlockSub "Get premium rewards and exclusive perks!": x≈0.060, y≈0.045 (≈20px).
- PerkChips: centered cluster x 0.470 → 0.760, three chips each ≈0.090w: "+20% XP Boost" (book/star icon),
  "+20% Gold Bonus" (coin icon), "Exclusive Rewards" (chest icon).
- Btn_UnlockPremium: right, x 0.790 → 0.972, y≈0.065 (height ≈0.075h ≈81px); gold/violet CTA, label
  "UNLOCK PREMIUM" + gem icon + "999".

**Tablet (4:3):** fewer tier columns visible per viewport (scroll reveals more); progress strip wraps Missions
under the bar if width < threshold; CTA bar stacks title/sub left, perks may wrap. **Ultrawide (21:9):** more
tier columns visible at once; showcase stays right-anchored; BG full-bleed. **Notch:** SafeAreaRoot insets;
BG/drapes full-bleed under cutout; back/chips clear.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "SEASON OF GLORY" | serif display, ornate | Black | UPPER | +8% | heavy gold bevel + bloom + drop | 65 | `#f0d27a`→`#caa04a` |
| "Ends in: 28d 14h" | sans | Medium | Sentence | +2% | hourglass icon, dim | 22 | `#cbb98a` |
| Currency values | tabular sans | Semibold | — | tabular | 1px stroke | 30 | `#f4e9c8` |
| Season level "23" | tabular serif | Black | — | — | gold glow + stroke | 44 | `#fff4d6` |
| "Battle Pass XP" | sans | Medium | Title | +2% | dim | 22 | `#c9b27a` |
| XP "650 / 1,000 XP" | tabular sans | Semibold | — | tabular | over-bar | 20 | `#f0e8cf` |
| Next level "24" | tabular serif | Bold | — | — | gold | 26 | `#f0d27a` |
| "Missions" | sans | Semibold | Title | +2% | gold | 26 | `#f0e8cf` |
| "FREE" / "PREMIUM" rail | strong sans | Bold | UPPER | +8% | FREE steel, PREMIUM violet glow | 24 | FREE `#9fb6d8`; PREM `#c79bff` |
| Tier numbers 19–25 | tabular sans | Bold | — | — | current glows | 26 | cur `#f0d27a`; others `#d9c79a` |
| Reward qty (10,000/25/50/200…) | tabular sans | Semibold | — | tabular | 1px stroke | 20 | `#f4e9c8` |
| "?" mystery | serif | Bold | — | — | violet glow | 30 | `#c79bff` |
| "BATTLE PASS" (showcase) | serif display | Black | UPPER | +6% (stacked) | gold bevel + violet bloom | 30 | `#f0d27a` |
| Showcase tier "30" | tabular serif | Black | — | — | strong gold glow | 48 | `#fff4d6` |
| "UNLOCK PREMIUM" (bar title) | serif/strong | Black | UPPER | +6% | gold bevel + bloom | 34 | `#f0d27a` |
| Unlock sub | sans, light | Regular | Sentence | +2% | dim | 20 | `#cbb98a` |
| Perk labels (+20% …) | sans | Medium | Title | +2% | icon-led | 19 | `#e8e2cf` |
| CTA "UNLOCK PREMIUM 999" | strong sans | Bold | UPPER | +4% | white + dark stroke; gem icon | 30 | `#ffffff` |

---

## G — MATERIALS
- **BG:** dark dusk battlefield, violet-shifted, faint siege silhouettes; strong vignette.
- **Drapes/banners:** rich amethyst velvet `#5a2db0`→`#7e3fd6` with gold trim + tassels; soft cloth shading.
- **Gold chrome:** brushed antique gold filigree on title frame, track frame, tier nodes, CTA edges.
- **Free cells:** steel-blue beveled frames `#2b3a5a`→`#3d6fb0`, dark recess, gold corner ticks.
- **Premium cells:** amethyst beveled frames `#5a2db0`→`#b07cf0` with violet inner glow; a small 🔒 padlock
  overlay when the pass isn't owned; richer/brighter than free cells.
- **Reward icons:** glassy gems, shiny coins, wooden+gold chests, bound spellbooks, painted cosmetics — each
  in a recessed well; claimed cells show a green ✓ medallion.
- **Tier-30 showcase:** ornate violet+gold card, inner glow, a high-detail legendary armored-king render;
  premium "featured" bloom; clearly the track's apex.
- **XP bar:** dark groove, violet/gold glassy fill (~65%) with top specular.
- **CTA UNLOCK PREMIUM:** gold-rimmed violet/gold gradient button, white label, gem icon, idle bloom; the
  brightest interactive element.

---

## H — COMPONENTS (states)
**TierColumn reward Cell (Free & Premium):**
- *unclaimed-locked (tier > current):* frame dim, faint padlock; reward greyed.
- *claimable (tier ≤ current, not yet claimed):* frame lit + gentle pulse; tap → claim flow.
- *claimed:* green ✓ medallion, frame slightly dim/settled.
- *premium-locked (pass not owned):* premium cell shows 🔒 + "Unlock Premium" gating; tapping routes to the
  UNLOCK PREMIUM CTA.
- *mystery ("?"):* hidden reward until claimed/revealed.

**Showcase_Tier30:** featured/locked state with strong glow; tap → detail of the tier-30 reward.

**XPBar:** read-only; fills to 650/1,000; on level-up animates to full → resets, level badge ticks 23→24.

**Btn_Missions:** standard gold pill; opens the pass missions/quests list.

**Btn_UnlockPremium (CTA):**
- *idle:* gold/violet, "UNLOCK PREMIUM" + gem 999, bloom.
- *hover:* +10% brightness, glow grows.
- *pressed:* scale 0.97 + flash.
- *owned (premium already purchased):* hidden/replaced by "PREMIUM ACTIVE" badge; premium cells unlock.
- *insufficient gems:* tap → confirm/insufficient modal → Store.

**Perk chips:** static informational; hover shows tooltip detail.

**Back / chip + buttons:** standard.

---

## I — ANIMATION TIMELINE
**OnShow (~0.7s):**
- 0.00s BG + vignette fade (0.20s); drapes settle (subtle sway-in 0.30s).
- 0.05s Header (title scale 0.96→1.0 + bloom, timer fade) 0.25s.
- 0.12s Progress strip fades in; **XP bar fills** left→right to 650/1,000 (0.45s ease-out); level badge pop.
- 0.20s **Tier columns stagger in** from left (each scale 0.94→1.0 + fade, 0.03s stagger); claimable cells
  begin a gentle pulse; current tier (23) node glows.
- 0.40s Tier-30 showcase scales 0.96→1.0 + strong bloom sweep (0.30s).
- 0.50s Bottom CTA bar slides up + fade (0.22s); UNLOCK PREMIUM gets a single glow sweep; perk chips cascade.

**OnClaim(tier):** cell flash → green ✓ stamps with a pop (0.25s) + sparkle; reward icon flies to the
currency chip / inventory; chip count ticks up.

**OnUnlockPremium:** CTA flash → premium cells' padlocks unlock in a left-to-right cascade (0.04s each) with
violet sparkle; "PREMIUM ACTIVE" badge appears; perk chips glow.

**Idle ambient:** drapes faint sway; showcase card slow shimmer; claimable cells pulse.

---

## J — PARTICLE & FX
- **Drapes/title:** gentle gold dust + soft bloom on the title.
- **Premium cells & showcase:** violet shimmer sweep (rarer = brighter); tier-30 card has rising amethyst
  motes + a slow specular sweep.
- **Claimable cells:** soft pulsing glow + occasional sparkle.
- **Claim:** gold/violet sparkle burst + reward fly-to-chip; chip coin-poof on arrival.
- **Unlock premium:** cascade of violet unlock sparks across the premium row.
- **CTA idle:** bloom breathing; press = gem-spark ring.
Budget pooled/capped; reduce sweeps/motes on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load season data (name, end time, current tier 23, XP 650/1,000, free/premium reward states,
  premium-owned flag) read-only; run enter timeline; start timer countdown.
- **OnScrollTrack:** horizontal scroll reveals earlier/later tiers; tier-30 showcase pinned at the right end.
- **OnClaim(tier,row):** if claimable & (premium row → pass owned) → send claim request (server-auth); on
  success stamp ✓, grant reward, tick currency; else gate (route to UNLOCK PREMIUM or show locked).
- **OnMissions:** open the battle-pass missions list (XP sources).
- **OnUnlockPremium:** if affordable (gems ≥ 999) → purchase request (server-auth) → unlock premium row;
  else → insufficient/confirm modal → Store.
- **OnPlus (chip):** route to Store.
- **OnBack:** pop → Main Menu.
- **§12:** all claims/purchases are server-authoritative requests; UI displays + requests only; no local
  balance mutation; no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn values: season "SEASON OF GLORY", "Ends in: 28d 14h", level 23, XP "650 / 1,000 XP",
  next "24", visible tiers 19–25 + showcase 30, currencies Gems 2,450 / Gold 98,760, perks "+20% XP Boost" /
  "+20% Gold Bonus" / "Exclusive Rewards", CTA "UNLOCK PREMIUM 999". Reward quantities as drawn
  (10,000 / 25 / 50 / 15,000 / 5,000 / 100 / 200 / 1 / "?").
- Two rows only: **FREE** (top) and **PREMIUM** (bottom). Do not add a third track.
- Only two currency chips (Gems + Gold). Do not add Silver here.
- Do not invent tier rewards beyond what's legible; render the icon shown for ambiguous cells.
- No portrait variant, no stick figures, no real brand text.
- Claims/purchases are requests only; no local/ECS mutation.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Ornate "SEASON OF GLORY" banner with drapes + "Ends in: 28d 14h" timer; Gems 2,450 / Gold 98,760 chips.
2. Progress strip: level "23" badge, "Battle Pass XP", XP bar 650/1,000 (~65% fill) → "24", Missions button.
3. Horizontally-scrolling two-row track (FREE top / PREMIUM bottom) with tier columns 19–25 + numbered nodes,
   correct reward icons/quantities, claimed ✓ on tier 21, "?" mystery on tier 22 premium.
4. Premium cells carry violet frames + padlocks (pass-locked); free cells steel-blue.
5. Tier-30 "BATTLE PASS" showcase card (violet, legendary reward) pinned right.
6. Bottom CTA bar: UNLOCK PREMIUM title + sub + 3 perk chips + "UNLOCK PREMIUM 999" gem CTA (brightest CTA).
7. XP fill, tier stagger, claim ✓ pop, premium-unlock cascade animations present.
8. Safe-area honored; BG/drapes full-bleed; track scrolls; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges.

## N — IMPLEMENTATION CONFIDENCE
**88/100.** High confidence on the overall structure (banner, progress strip, two-row scrolling track,
showcase, CTA bar) and all major copy/numbers. Lower-than-others uncertainty on a few **per-cell reward
icons/quantities** (some glyphs are small/overlapping — e.g. tier 22/25 premium contents) and whether RailLabels
should be sticky vs scroll with the track (spec recommends sticky-left). These are cell-detail risks, not
structural; render ambiguous cells as the icon+legible number shown.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 25 · Battle Pass".
- [x] Fraction-based layout normalized to 2340×1080; track column sizes + gaps given.
- [x] ScrollRect (horizontal) + HorizontalLayoutGroup specified for the tier track.
- [x] Tier-cell / CTA / Missions / chip states (locked/claimable/claimed/mystery/owned/insufficient).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; ambiguous cells flagged not invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.
