# BULWARK — UI CONSTRUCTION SPEC · 28 · Daily Reward

Source: design/DailyRewardDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops 7-day login-streak calendar. Modal-style ornate panel over a dimmed hub backdrop. Single primary action: **CLAIM** the currently-available day. ADR/canon note (do NOT alter forensic spec): the **5R/120 chip in the inherited top bar reads as an energy/stamina meter → canon-CUT; document, flag, omit at implementation**. Day-7 "EPIC CHEST + Legendary Unit Guaranteed" is loot/gacha-adjacent → note for ADR; it is a *reward depiction*, not a spin, so lower risk than Lucky Spin.

---

## A · SCREEN PURPOSE
A retention/login screen presented as a centered ornate panel floating over the dimmed Main-Menu hub (rail + currency bar + faint right-edge tiles bleed through at low alpha). It shows a horizontal **7-cell streak calendar** (Day 1 → Day 7), the player's reward for each day, claim-state per day (claimed ✓ / claimable-highlighted / locked 🔒), a **streak summary** strip, and one gold **CLAIM** CTA. Exactly one day is "today/claimable" (here Day 3, ring-highlighted) and is the only one whose CLAIM is active. Entry is automatic on first hub load of the day or via the bottom-hub "Daily" entry. Close via top-right **✕** returns to hub.

## B · VISUAL DNA
Inherits the global dark heroic high-fantasy DNA (00 §6). Screen-specific:
- **Backdrop:** the live hub behind, darkened by a near-black scrim (~70% black) + vignette; a soft warm god-ray cone descends behind the panel top, and a faint focal glow sits behind the central highlighted day.
- **Panel:** large near-black obsidian plate (#0c0e15→#161a24 vertical) with an **ornate cast-gold/antique-bronze double frame** — outer beveled gold rail + inner thin filigree line, scrolled corner cartouches, and a **crown/gem crest centerpiece** straddling the top edge (cobalt gem in a gold sunburst).
- **Day cells:** dark slate rounded-rect tiles in a row; the **claimable day (Day 3)** is enlarged, brighter, and wrapped in a glowing **gold ring/halo**; the **premium day (Day 7)** uses a **violet/amethyst** frame + body tint (premium signal) vs the gold/blue of the others.
- **CLAIM CTA:** brushed-gold pill (the single brightest interactive object), beveled, inner gradient, dark serif label.
- **Mood:** "reward ritual" — gold-on-black opulence; the highlighted day is the luminous focal subject.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
DailyRewardScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (hub-snapshot or static art, full-bleed, behind cutout)
│  ├─ DimScrim (black ~70%)
│  ├─ Vignette (radial, edges dark)
│  └─ GodRayCone (additive, top-center, behind panel)
├─ InheritedHubChrome (low-alpha bleed; NON-interactive here — see L)
│  ├─ TopBar
│  │  ├─ AvatarPortrait + Name "Warden" + "Level 32"
│  │  ├─ CurrencyChip_Gold   icon + "128,450"
│  │  ├─ CurrencyChip_Silver icon + "87,560"
│  │  ├─ CurrencyChip_Gems   icon + "2,850"
│  │  ├─ EnergyChip          icon + "5R/120"   ⚠️CANON-CUT
│  │  ├─ MailButton (envelope)
│  │  └─ MenuButton (hamburger)
│  ├─ LeftRail (icon+label stack: Campaign, Army, Commanders, Quests, Store, Alliance, Events)
│  └─ RightEdgeTiles (faint: "EVENT Ends in 2d 14h", "BATTLE Chapter 9-12")
└─ SafeAreaRoot (SafeAreaFitter; all interactive content)
   └─ RewardPanel (ornate framed plate, center)
      ├─ PanelFrame (gold double-frame + corner cartouches)
      ├─ TopCrest (crown/gem centerpiece, overlaps top edge)
      ├─ CloseButton ✕ (top-right, on/over frame)
      ├─ Header
      │  ├─ Title "DAILY REWARD" (serif gold-bevel UPPERCASE)
      │  └─ Subtitle "Log in every day to earn valuable rewards!"
      ├─ DayRow (HorizontalLayoutGroup, 7 children)
      │  ├─ DayCell_1 [CLAIMED]   {Header "DAY 1", Icon gold-coins, Amount "5,000", State ✓}
      │  ├─ DayCell_2 [CLAIMED]   {Header "DAY 2", Icon silver-bars, Amount "10,000", State ✓}
      │  ├─ DayCell_3 [CLAIMABLE] {Header "DAY 3", Icon blue-crystal, Amount "100", State ✓ + GOLD HALO RING}
      │  ├─ DayCell_4 [LOCKED]    {Header "DAY 4", Icon chest, Amount "1", State 🔒}
      │  ├─ DayCell_5 [LOCKED]    {Header "DAY 5", Icon shield/sigil, Amount "50", State 🔒}
      │  ├─ DayCell_6 [LOCKED]    {Header "DAY 6", Icon blue-gems, Amount "200", State 🔒}
      │  └─ DayCell_7 [LOCKED·PREMIUM] {Header "DAY 7" (gold), Icon ornate-chest, Label "EPIC CHEST",
      │                                  Sub "+ Legendary Unit Guaranteed!", State 🔒, VIOLET frame}
      └─ FooterStrip (rounded sub-panel inside frame, full width)
         ├─ FlameIcon (ember glow)
         ├─ StreakLine "STREAK: 3 DAYS"  (label + bold value)
         ├─ StreakSub  "Keep the streak going to earn better rewards!"
         └─ ClaimButton  "CLAIM"  (gold pill, right side)
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| DailyRewardScreen | Canvas | 0 | RectTransform+CanvasGroup | stretch-all | .5,.5 | — | root | fade in/out |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | extends UNDER cutout | cover-fill, no inset |
| DimScrim | backdrop | 0 | Image (black α.70) | stretch-all | .5,.5 | — | full-bleed | — |
| GodRayCone | backdrop | 2 | Image (additive) | top-center | .5,1 | — | — | scale w/ height |
| InheritedHubChrome | screen | 1 | RectTransform (α≈.25, raycast OFF) | stretch-all | .5,.5 | — | inside safe | non-interactive |
| SafeAreaRoot | screen | 2 | RectTransform+SafeAreaFitter | stretch-all | .5,.5 | — | insets to safeArea | drives all content |
| RewardPanel | SafeAreaRoot | 0 | Image (9-slice) | center | .5,.5 | — | inside safe | fixed aspect, scale-to-fit |
| PanelFrame | RewardPanel | 0 | Image (9-slice ornate) | stretch-all | .5,.5 | — | — | borders fixed px |
| TopCrest | RewardPanel | 1 | Image | top-center | .5,.5 | — | — | overlaps top edge |
| CloseButton | RewardPanel | 2 | Button+Image | top-right | 1,1 | — | — | offset fixed |
| Header | RewardPanel | 3 | VerticalLayoutGroup | top-stretch | .5,1 | center | — | — |
| Title | Header | 0 | Text (TMP) | — | .5,.5 | center | — | autosize cap |
| Subtitle | Header | 1 | Text (TMP) | — | .5,.5 | center | — | — |
| DayRow | RewardPanel | 4 | HorizontalLayoutGroup (ctrl-size, spacing) | center | .5,.5 | mid-center | — | cells flex equally; Day3/7 wider |
| DayCell_n | DayRow | 0..6 | Image + VerticalLayoutGroup (LayoutElement) | — | .5,.5 | top-center | — | min/pref width; Day3 ×~1.12 |
| ↳ DayHeader | DayCell | 0 | Text (banner sub-strip) | top-stretch | .5,1 | center | — | — |
| ↳ DayIcon | DayCell | 1 | Image | center | .5,.5 | center | — | square |
| ↳ DayAmount | DayCell | 2 | Text | — | .5,.5 | center | — | — |
| ↳ DayStateBadge | DayCell | 3 | Image (✓ or 🔒) on disc | bottom-center | .5,0 | center | — | — |
| ↳ HaloRing (Day3 only) | DayCell | -1 (behind) | Image (additive glow) | stretch-all | .5,.5 | — | — | pulses |
| FooterStrip | RewardPanel | 5 | Image + HorizontalLayoutGroup | bottom-stretch | .5,0 | mid-left→right | — | spans inner width |
| FlameIcon | FooterStrip | 0 | Image | left | 0,.5 | — | — | — |
| StreakText (line+sub) | FooterStrip | 1 | VerticalLayoutGroup | left | 0,.5 | left | — | flex grow |
| ClaimButton | FooterStrip | 2 | Button+Image | right | 1,.5 | center | — | fixed min width |

**List/grid note:** DayRow is the canonical "wheel/list" structure here → `HorizontalLayoutGroup` with `childForceExpandWidth=false`, per-cell `LayoutElement.preferredWidth`. Not a ScrollRect (all 7 fit; no scroll). If a future variant exceeds 7, wrap DayRow in a horizontal ScrollRect.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
Normalize the 1.5:1 source onto 2340×1080; panel is centered and height-bounded.
- **RewardPanel:** width ≈ **0.80·W = 1872 px**, height ≈ **0.78·H = 842 px**; centered (origin offset 0). Outer gold frame thickness ≈ 0.012·H ≈ 13 px; inner filigree inset ≈ 0.008·H.
- **TopCrest:** width ≈ 0.10·W ≈ 234 px, centered on top edge, vertical center on the frame line (≈ half above the edge).
- **CloseButton:** ⌀ ≈ 0.045·H ≈ 49 px; center ≈ (panelRight − 0.03·W, panelTop − 0.03·H).
- **Header block:** top inset ≈ 0.07·panelH. Title cap-height region ≈ 0.10·H. Subtitle baseline ≈ 0.045·H below title.
- **DayRow:** vertical center ≈ 0.50·H of panel; row height ≈ 0.42·panelH. Inner usable width ≈ panelW − 2·(0.04·W) ≈ 1685 px.
  - 7 cells with 6 gaps. Gap ≈ 0.012·W ≈ 28 px → total gaps ≈ 168 px → cells share ≈ 1517 px.
  - **Base cell width** ≈ 1517 / (6 + 1.12 + 1.0) [Day3 ×1.12, Day7 ×1.0 same as base; Day7 slightly taller not wider] ≈ **≈ 198 px** for standard cells; **Day3 ≈ 222 px** (×1.12) and visually elevated.
  - Standard cell aspect ≈ 0.55 W:H → cell height ≈ 360 px (~0.33·H). Day3 height ≈ ×1.10.
  - Internal cell layout (fractions of cell H): DayHeader strip 0–0.18; DayIcon 0.18–0.62 (square, side ≈ 0.6·cellW); DayAmount 0.62–0.80; StateBadge disc ⌀ ≈ 0.22·cellW centered at 0.90.
- **FooterStrip:** spans inner width; height ≈ 0.16·panelH ≈ 135 px; bottom inset ≈ 0.05·panelH. FlameIcon ⌀ ≈ 0.07·panelH. ClaimButton width ≈ 0.26·panelW ≈ 487 px, height ≈ 0.11·panelH ≈ 93 px, right-anchored with 0.04·W right inset.

**Tablet (4:3, 2048×1536-class):** match-height keeps panel height; extra side margin → panel may grow to 0.86·W; cells widen, gaps scale with W. **Ultrawide (21:9):** more backdrop revealed; panel stays 1872 px (cap max width 0.62·W so it doesn't stretch); content unchanged. **Notch/landscape:** SafeAreaRoot insets; full-bleed backdrop stays under cutout; panel never crosses safe inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "DAILY REWARD" | serif Trajan-display, prestige | Black/Heavy | UPPER | +6% | 1.0 | gold bevel + soft outer bloom + 2px dark stroke + drop-shadow | ~74 | fill #f0d27a→#caa04a gradient; stroke #3a2c0e |
| Subtitle | light serif/clean sans, calm | Regular | Sentence | 0 | 1.1 | subtle shadow | ~26 | #d9c79a |
| DayHeader "DAY n" | condensed sans-caps banner | SemiBold | UPPER | +4% | 1.0 | dark stroke | ~24 | #e8dcc0 (Day7 gold #f0d27a) |
| DayAmount "5,000"/"100" | numeric, clean | Bold | — | 0 | 1.0 | shadow; highlighted day +glow | ~32 (Day3 ~36) | #ffffff; Day3 #eaf2ff |
| Day7 "EPIC CHEST" | serif small-caps, premium | Bold | UPPER | +4% | 1.0 | violet glow + stroke | ~26 | #c9a6ff |
| Day7 sub "+ Legendary Unit Guaranteed!" | clean sans | Italic/Reg | Sentence | 0 | 1.05 | shadow | ~18 | #b9a7d8 |
| StreakLine "STREAK: 3 DAYS" | condensed caps, emphatic | Bold | UPPER | +2% | 1.0 | shadow; "3 DAYS" brighter/larger | ~30 | label #d9c79a, value #ffd76a |
| StreakSub | clean sans, quiet | Regular | Sentence | 0 | 1.1 | — | ~20 | #9a937f |
| ClaimButton "CLAIM" | serif display, action | Heavy | UPPER | +8% | 1.0 | dark engrave + top highlight | ~40 | #2a1c06 on gold |
| Currency numbers (top bar) | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |

## G · MATERIALS (hex ranges, roughness, wear, edges, reflection/bloom)
- **Panel body:** obsidian #0c0e15→#161a24, roughness high (matte), faint engraved vertical sheen; subtle inner top-edge dark gradient.
- **Gold frame/CTA:** brushed antique gold #6b5320 (shadow) → #caa04a (mid) → #f0d27a (highlight) → #fff2c2 (specular pip); low roughness on bevel crests, worn micro-nicks on outer rail; warm rim-light top-left; **bloom on highlight pips**.
- **Standard day cell:** dark slate #11141d→#1b2030, thin bronze edge #7a5f28; claimable Day3 cell brighter #1a2336 with **gold halo ring** (additive #ffd16a, soft 18px blur, animated pulse) + cobalt inner glow.
- **Premium Day7 cell:** violet #1a1130→#2a1c52 body, **amethyst frame** #5a2db0→#9e6bf0, inner magic glow #b07bff + bloom.
- **State discs:** dark disc #14161e w/ bronze ring; ✓ = gold #f0d27a engraved; 🔒 = desaturated steel #8a8f9c with the whole cell at ~0.7 brightness + slight grey overlay.
- **Icons:** coins warm gold; silver bars cool #c8ccd6 specular; blue crystal/gems cobalt #2b56c8→#4f8bff with inner glow + crystalline specular; chests aged wood + bronze bands + lock.
- **Reflections/bloom:** gold + gems + halo carry bloom; matte panel/body do not.

## H · COMPONENTS (states)
**DayCell** — three logical states (visual), interaction only on the claimable one:
- *CLAIMED* (Day1,2): full color, ✓ gold disc, no halo, raycast off (informational).
- *CLAIMABLE* (Day3): brightened body + **pulsing gold halo ring** + cobalt focal glow; ✓-style disc; this is the visual "next reward"; it is NOT a separate button (claiming uses the footer CLAIM). On show it gently scales 1.00→1.04→1.00.
- *LOCKED* (Day4-7): ~70% brightness, grey overlay, 🔒 steel disc; raycast off. Day7 keeps its violet identity but dimmed under lock.
- *(future hover/pressed n/a — cells aren't pressable in this layout)*.

**ClaimButton** (the only interactive control besides ✕):
- *idle/enabled:* gold pill, bevel, soft outer glow, label #2a1c06; subtle 1.0→1.02 breathing glow.
- *hover/focus (gamepad):* +8% brightness, glow radius +.
- *pressed:* scale 0.96, inner shadow deepens, highlight dims 1f.
- *disabled* (no claim available / already claimed today): desaturate to grey-gold #7d7355, label #4a4636, glow off, raycast still on (tapping → small shake + toast "Come back tomorrow!").
- *success (post-claim):* flash white→gold, then label morphs to "CLAIMED" greyed; the claimable cell plays a collect burst.

**CloseButton ✕:** dark disc + bronze ring; hover brighten; pressed scale 0.92; closes screen.

**FooterStrip:** static info container; FlameIcon has a looping ember flicker; StreakLine value can count-up on first show.

## I · ANIMATION TIMELINE (timestamps, durations, order, easing)
**OnShow (entry, total ~0.85 s):**
- 0.00 backdrop scrim fade 0→.70 (0.20 s, linear) + god-ray fade-in (0.30 s).
- 0.06 RewardPanel: scale 0.92→1.00 + α 0→1 (0.28 s, ease-out-back small).
- 0.18 TopCrest drop-in: y −20→0 + α (0.22 s, ease-out) + crest gem glint sweep.
- 0.22 Title: α + slight 1.04→1.00 (0.20 s).
- 0.28 DayRow cells stagger in L→R, each y +16→0 + α, 0.04 s apart, 0.18 s each (ease-out).
- 0.55 Day3 halo ring fades up + starts pulse loop (period 1.6 s).
- 0.62 FooterStrip slide up + α (0.20 s); StreakLine value count-up 0→3 (0.4 s, ease-out) if dynamic.
- 0.70 ClaimButton pop 0.9→1.0 + glow on (0.18 s, ease-out-back); begin breathing-glow loop.

**Idle loops:** Day3 halo pulse (scale 1.00↔1.06 α 0.6↔1.0, 1.6 s sine); CLAIM glow breathe (1.6 s); flame flicker (0.5 s random); crest gem slow shimmer (3 s).

**OnClaim (see K):** CLAIM press 0.96 (0.08 s) → white flash (0.10 s) → Day3 cell "collect" burst (icon scale 1.2 + particles, 0.4 s) → reward fly-to-currency (0.5 s) → cell✓ locks, CLAIM → disabled "CLAIMED" (0.2 s). Optional: highlight advances to Day4 (halo migrates, 0.3 s) if same-session preview.

**OnClose:** reverse of entry, ~0.30 s (panel scale 1.0→0.94 + α→0, scrim fade out).

## J · PARTICLE & FX
- God-ray cone (soft additive, slow drift) behind panel top.
- Day3 **halo ring**: additive radial gold glow + faint orbiting sparkles (2–3 motes).
- Gem/crystal icons: tiny inner twinkle sparkles (1–2, slow).
- FlameIcon: small ember particle puff loop + flicker light.
- CLAIM glow: soft pulsing rim bloom; on success a one-shot **gold coin/spark burst** + screen-space sparkle from the claimed cell to the top-bar currency chip.
- TopCrest gem: occasional specular glint streak.
- Vignette + subtle dust motes drifting in the dark field (very low alpha).

## K · EVENT BEHAVIOR
- **OnShow:** query server-auth streak state (read-only) → set per-day {claimed/claimable/locked}, current streak count, today index; play entry timeline; if today already claimed → all played cells ✓, CLAIM = disabled "CLAIMED". (Client never computes rewards; it renders server state.)
- **OnClaim:** disable button immediately; send claim request (stub/server-auth); on ack: mark today ✓, grant reward (currency count-up handled by hub), play collect burst + fly-to-chip, set CLAIM→"CLAIMED" disabled, persist; on failure: re-enable + toast via shared error sheet (39).
- **OnClaim when none available:** shake + toast "Already claimed — come back tomorrow!" (no request).
- **OnClose / ✕ / back:** fade out, pop screen from UiRouter stack → return to hub.
- **Day7 special:** EPIC CHEST reward routes to Chest-Open-Result (21) reveal flow when its day is claimed (ADR-gated content; behavior noted, not altered here).
- **No timers tick visually** except the inherited right-edge EVENT/BATTLE tiles (decorative bleed). Streak reset logic is server-side; client only displays.

## L · NEGATIVE RULES
- Do **not** make claimed/locked day cells tappable; only CLAIM and ✕ are interactive.
- Do **not** let the player claim more than one day per server-day; never client-authoritative on rewards/streak.
- Do **not** render the inherited **EnergyChip (5R/120)** as a live system — it is **canon-CUT**; at implementation omit/replace (kept here only as forensic record of the source art).
- Do **not** treat the hub bleed (rail/tiles/top bar) as functional while this screen is open — raycast OFF, alpha low.
- Do **not** restyle Day7 to gold — its **violet/amethyst** premium identity is load-bearing.
- Do **not** invent extra days, numbers, or rewards beyond the 7 shown; values are exactly: 5,000 / 10,000 / 100 / 1 / 50 / 200 / EPIC CHEST.
- No portrait layout; no real-time anything.

## M · ACCEPTANCE CRITERIA (≥95%)
1. 7 day-cells in one row, correct order & exact values/labels; Day3 highlighted with gold halo; Days1-2 ✓; Days4-7 🔒; Day7 violet/premium with "EPIC CHEST + Legendary Unit Guaranteed!".
2. Ornate gold double-frame + top crest + ✕; title "DAILY REWARD" gold-bevel serif; subtitle exact text.
3. Footer strip: flame + "STREAK: 3 DAYS" + sub line + gold **CLAIM** pill right-aligned.
4. Layout matches fraction math within ±2% at 2340×1080; safe-area respected; full-bleed backdrop under cutout.
5. Entry/idle/claim animations present with correct order & easing; CLAIM disabled state correct.
6. Palette hexes within specified ranges; gold/gem/halo bloom present, matte panel not blooming.
7. Energy chip flagged/omitted; no extra interactivity on day cells.

## N · IMPLEMENTATION CONFIDENCE
**92/100.** High: clean modal, finite known content, exact visible values, standard HLG layout. Deductions: (−3) exact corner-cartouche/crest geometry & frame filigree are art-asset dependent (need matching 9-slice sprites); (−3) precise halo/glow radii and bloom thresholds are eyeballed; (−2) Day7 chest→reveal hand-off and streak-advance micro-animation are inferred, not shown in a static frame.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction-based math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded; nothing invented.
- [x] List structure (DayRow) typed (HLG, not ScrollRect) with rationale.
- [x] Component states (DayCell, CLAIM, ✕) enumerated incl. disabled.
- [x] Animation timeline with timestamps/easing; particle/FX listed.
- [x] ADR/canon flags noted (energy CUT; Day7 reward gacha-adjacent) without altering forensic spec.
- [x] Landscape, safe-area, full-bleed-under-cutout rules applied.
- [x] No code/asset/scene changes; spec is documentation only.
