# BULWARK — UI CONSTRUCTION SPEC · 29 · Lucky Spin

Source: design/LuckySpinDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops **prize wheel** (8-segment) with one **free daily spin** + a paid **×10** spin. **⚠️ GACHA — REQUIRES ADR.** Lucky Spin is randomized-reward loot mechanics colliding with the "no loot boxes/gacha" principled CUT (00 §5). This spec is **forensic only** (records the art exactly); the ADR governs whether/how it ships (e.g., transparent deterministic redesign, posted odds, no paid spins). The **SPIN ×10 / 450** paid action and "Better rewards guaranteed" copy are the highest-risk elements — flag, do not soften the spec.

---

## A · SCREEN PURPOSE
A daily gacha/reward wheel. Layout is a **three-column** composition over a dark hub backdrop: a **left info rail** (next-free-spin countdown + "How it works"), a large **central rotating prize wheel** (lion-head hub + 8 reward segments + a fixed top pointer), and a **right action stack** ("Spin & Win" banner + **SPIN — FREE** CTA + **SPIN ×10 (450)** CTA). A **RECENT WINS** ticker row spans the bottom. The player spins (free once/day, or pays gems for ×10); the wheel decelerates onto a segment and grants that reward. Entry via bottom-hub "Spin" entry; close via top-right **✕**.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** dimmed hub, heavy vignette, warm god-ray behind the wheel; a strong **focal glow halo** behind the wheel rim.
- **Wheel:** the hero subject — a circular ornate **gold/bronze rim** with riveted bevel and inset gem studs, divided into **8 colored pie segments** (alternating jewel tones: amethyst-violet, slate-steel, oxblood/bronze, cobalt-blue, mossy-green, ember-orange), each holding a reward icon + label. A cast-gold **lion-head boss** centerpiece. A **fixed pointer/marker** sits at the top of the rim (gold arrow/crest).
- **Left rail:** dark slate sub-panels with bronze edges; a clock glyph + countdown; a small bulleted "how it works" card.
- **Right stack:** a furled **cobalt banner** ("SPIN & WIN / GREAT REWARDS EVERY TIME"); **blue gloss CTA** (SPIN — FREE) and **gold CTA** (SPIN ×10) with a gem cost.
- **Bottom ticker:** dark strip, "RECENT WINS" centered header, a row of {avatar · name · prize · "Xm ago"} entries.
- **Mood:** "casino of the gods" — opulent gold wheel glowing against black; CTAs are the brightest interactive objects.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
LuckySpinScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (hub art/snapshot, under cutout)
│  ├─ DimScrim (black ~68%)
│  ├─ Vignette
│  ├─ GodRayCone (additive, behind wheel)
│  └─ WheelFocalGlow (radial, behind wheel)
├─ InheritedHubChrome (low-alpha bleed; non-interactive)
│  ├─ TopBar: CurrencyChip_Gold "125,450", CurrencyChip_Gems "2,850"
│  └─ LeftRail icons (Campaign, Army, Commanders, Quests, Store, Alliance, Events)
├─ CloseButton ✕ (top-right)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Header (top-center)
   │  ├─ Title "LUCKY SPIN" (serif gold-bevel UPPER)
   │  └─ Subtitle "Spin the wheel and win amazing rewards!"
   ├─ LeftInfoColumn (vertical)
   │  ├─ NextSpinCard
   │  │  ├─ Label "NEXT FREE SPIN"
   │  │  ├─ ClockIcon
   │  │  └─ Countdown "03:11:00"
   │  └─ HowItWorksCard
   │     ├─ Header "How it works"
   │     └─ Bullets ["Spin daily for free", "Win rewards", "Better rewards on 10x spins!"]
   ├─ WheelGroup (center)
   │  ├─ WheelRimFrame (ornate gold ring + studs)
   │  ├─ WheelDisc (ROTATES — the 8 segments)
   │  │  ├─ Seg0 "EXCLUSIVE AVATAR" (icon avatar)          [slate]
   │  │  ├─ Seg1 "500 GEMS" (purple gems icon)             [amethyst]   ← top in still
   │  │  ├─ Seg2 "100K SILVER" (silver coins icon)          [steel]
   │  │  ├─ Seg3 "EPIC CHEST" (ornate chest icon)           [bronze/gold]
   │  │  ├─ Seg4 "COMMANDER SHARD x10" (shard icon)         [ember-orange]
   │  │  ├─ Seg5 "250 GEMS" (blue gems icon)                [cobalt]
   │  │  ├─ Seg6 "1 DAY SPEED UP" (hourglass/clock icon)    [mossy-green]
   │  │  └─ Seg7 "RARE CHEST" (wood chest icon)             [dark-bronze]
   │  ├─ WheelHub (lion-head gold boss, static, on top)
   │  └─ WheelPointer (fixed top marker/arrow)
   ├─ RightActionColumn (vertical)
   │  ├─ SpinWinBanner (cobalt furled banner: "SPIN & WIN" / "GREAT REWARDS" / "EVERY TIME")
   │  ├─ SpinFreeButton (blue): label "SPIN — FREE" + sub "1 free spin per day"
   │  └─ SpinX10Button (gold): label "SPIN ×10" + cost "450" (gem icon) + sub "Better rewards guaranteed!"
   └─ RecentWinsTicker (bottom strip)
      ├─ TickerHeader "RECENT WINS" (with flank rule lines)
      └─ WinRow (HorizontalLayoutGroup, 6 entries)
         ├─ WinItem{avatar, "ThaneOrlok", "50K Silver", "2m ago"}
         ├─ WinItem{avatar, "LadyMorrigan", "250 Gems", "5m ago"}
         ├─ WinItem{avatar, "Grimblade", "Rare Chest", "8m ago"}
         ├─ WinItem{avatar, "IronWolf", "Exclusive Avatar", "12m ago"}
         ├─ WinItem{avatar, "ValenShield", "100K Silver", "15m ago"}
         └─ WinItem{avatar, "Stormrider", "Commander Shard ×10", "18m ago"}
```
*(Segment reward set is fixed at 8; the order above is the still's clockwise reading from the violet "500 GEMS" segment at top.)*

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LuckySpinScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| WheelFocalGlow | backdrop | 3 | Image (additive) | center | .5,.5 | — | — | scale w/ wheel |
| InheritedHubChrome | screen | 1 | Rect (α≈.25, raycast OFF) | stretch | .5,.5 | — | inside safe | non-interactive |
| CloseButton | screen | 2 | Button+Image | top-right | 1,1 | — | inside safe | fixed offset |
| SafeAreaRoot | screen | 3 | Rect+SafeAreaFitter | stretch | .5,.5 | — | insets | drives content |
| Header | SafeAreaRoot | 0 | VerticalLayoutGroup | top-center | .5,1 | center | — | — |
| LeftInfoColumn | SafeAreaRoot | 1 | VerticalLayoutGroup | mid-left | 0,.5 | top-left | — | fixed width frac |
| NextSpinCard | LeftInfoColumn | 0 | Image+VLG | — | .5,.5 | center | — | — |
| HowItWorksCard | LeftInfoColumn | 1 | Image+VLG | — | 0,.5 | left | — | — |
| WheelGroup | SafeAreaRoot | 2 | Rect (square, aspect-fit) | center | .5,.5 | center | — | size = min(0.5·W,0.78·H) |
| WheelRimFrame | WheelGroup | 0 | Image (circular) | stretch-all | .5,.5 | — | — | scales w/ group |
| **WheelDisc** | WheelGroup | 1 | **Image (radial sprite) OR 8× pie Image** + rotation driver | stretch-all | .5,.5 (center pivot!) | — | — | **rotates about center** |
| Seg_n content | WheelDisc | 0..7 | (Icon+Text) parented at 45° increments, radius offset | center+rotated | varies | center | — | rotates with disc |
| WheelHub | WheelGroup | 2 | Image (lion boss) | center | .5,.5 | center | — | static overlay |
| WheelPointer | WheelGroup | 3 | Image (arrow) | top-center | .5,1 | — | — | static, marks landing |
| RightActionColumn | SafeAreaRoot | 3 | VerticalLayoutGroup | mid-right | 1,.5 | center | — | fixed width frac |
| SpinWinBanner | RightActionColumn | 0 | Image+Text | — | .5,.5 | center | — | — |
| SpinFreeButton | RightActionColumn | 1 | Button+Image+VLG | — | .5,.5 | center | — | full column width |
| SpinX10Button | RightActionColumn | 2 | Button+Image+HLG(cost) | — | .5,.5 | center | — | full column width |
| RecentWinsTicker | SafeAreaRoot | 4 | Image+VLG | bottom-stretch | .5,0 | center | — | spans inner width |
| TickerHeader | RecentWinsTicker | 0 | Text + 2 rule Images | top-center | .5,1 | center | — | — |
| WinRow | RecentWinsTicker | 1 | **HorizontalLayoutGroup** (6 items, equal) | center | .5,.5 | mid-center | — | items flex; overflow → marquee/scroll |
| WinItem_n | WinRow | 0..5 | Image+VLG (avatar,name,prize,time) | — | .5,.5 | center | — | — |

**List/wheel note:**
- **WheelDisc** is the canonical rotating element → a single radial sprite (preferred for crisp segment art) OR 8 pie `Image`s under one pivot-centered RectTransform; rotation animated via `localEulerAngles.z`. Segment content (icon+label) is parented at 45° steps, radius ≈ 0.62·R, each rotated so text faces outward/upright at rest.
- **WinRow** is a horizontal list → `HorizontalLayoutGroup`; if entries exceed the strip, convert to a **looping marquee** (auto-scroll) or horizontal `ScrollRect`.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Header:** top inset 0.05·H; title cap region ~0.09·H centered; subtitle below.
- **WheelGroup:** square, side = **min(0.50·W, 0.80·H) ≈ 864 px** (height-bound), centered horizontally at ≈0.50·W, vertical center ≈0.52·H (slightly low to clear header). Radius R ≈ 432 px.
  - Rim frame thickness ≈ 0.10·R ≈ 43 px; gem studs ≈ 0.05·R spaced every 45°.
  - 8 segments × 45°. Segment label ring radius ≈ 0.62·R; icon centered ≈ 0.55·R (icon ⌀ ≈ 0.20·R), label arc/baseline ≈ 0.78·R.
  - WheelHub (lion boss) ⌀ ≈ 0.34·R ≈ 147 px, centered.
  - WheelPointer at 12 o'clock, height ≈ 0.16·R, tip touching rim inner edge.
- **LeftInfoColumn:** width ≈ 0.20·W ≈ 468 px; left inset ≈ 0.03·W; vertical center ≈ 0.52·H. NextSpinCard height ≈ 0.16·H; HowItWorksCard height ≈ 0.26·H; gap 0.03·H. Countdown text ≈ 0.05·H tall.
- **RightActionColumn:** width ≈ 0.22·W ≈ 515 px; right inset ≈ 0.03·W; vertical center ≈ 0.50·H. SpinWinBanner height ≈ 0.20·H; SpinFreeButton height ≈ 0.12·H; SpinX10Button height ≈ 0.13·H (slightly taller, has cost row); gaps 0.025·H. Button sub-text strip ≈ 0.035·H beneath each.
- **RecentWinsTicker:** spans inner width (≈0.94·W), height ≈ 0.16·H, bottom inset ≈ 0.03·H. Header rule lines flank centered text. WinRow: 6 items, gap ≈ 0.01·W; each item width ≈ (rowW − 5·gap)/6; avatar ⌀ ≈ 0.045·H, name/prize/time stacked.

**Tablet (4:3):** match-height keeps wheel size; side columns gain margin, may widen slightly; wheel stays centered. **Ultrawide (21:9):** more backdrop; columns push toward edges but cap inset so they don't drift off-balance; wheel unchanged. **Notch:** SafeAreaRoot insets the three columns + ticker; full-bleed backdrop/glow remain under cutout.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "LUCKY SPIN" | serif Trajan display | Black | UPPER | +6% | 1.0 | gold bevel + bloom + 2px stroke | ~72 | #f0d27a→#caa04a; stroke #3a2c0e |
| Subtitle | light serif/clean | Reg | Sentence | 0 | 1.1 | shadow | ~24 | #d9c79a |
| "NEXT FREE SPIN" | condensed caps | SemiBold | UPPER | +4% | 1.0 | shadow | ~22 | #cdbf99 |
| Countdown "03:11:00" | mono/numeric, tense | Bold | — | +2% | 1.0 | soft glow | ~40 | #ffe08a |
| "How it works" | serif small-caps | SemiBold | Title | +2% | 1.0 | shadow | ~24 | #e8dcc0 |
| Bullets | clean sans | Reg | Sentence | 0 | 1.2 | — | ~20 | #c7bfa8 |
| Segment labels (e.g. "500 GEMS","EPIC CHEST") | condensed caps, punchy | Bold | UPPER | +2% | 0.95 | dark stroke + slight glow; color by tier | ~22–24 | white #fff / gold #f0d27a / gem-tint per segment |
| Banner "SPIN & WIN / GREAT REWARDS / EVERY TIME" | serif display, hype | Heavy | UPPER | +4% | 1.0 | gold-on-cobalt, bevel | line1 ~30, l2 ~26, l3 ~24 | #f4e6b0 on #1d3a8a |
| "SPIN — FREE" | serif display, action | Heavy | UPPER | +6% | 1.0 | top highlight + glow | ~38 | #ffffff on blue |
| "1 free spin per day" | clean sans | Reg | Sentence | 0 | 1.0 | shadow | ~18 | #b9c6e6 |
| "SPIN ×10" | serif display, action | Heavy | UPPER | +6% | 1.0 | dark engrave on gold | ~36 | #2a1c06 on gold |
| Cost "450" | numeric bold | Bold | — | 0 | 1.0 | shadow; gem icon left | ~30 | #ffffff |
| "Better rewards guaranteed!" | clean sans, italic | Italic | Sentence | 0 | 1.0 | shadow | ~18 | #d9c79a |
| "RECENT WINS" | condensed caps | SemiBold | UPPER | +6% | 1.0 | flank rules | ~22 | #cdbf99 |
| Win name | clean sans | SemiBold | — | 0 | 1.0 | shadow | ~18 | #e8e2cf |
| Win prize | clean sans | Reg | — | 0 | 1.0 | tint by reward | ~16 | gem #6f8fff / silver #c8ccd6 / chest #d9b06a |
| Win time "Xm ago" | clean sans, quiet | Reg | — | 0 | 1.0 | — | ~14 | #8a8472 |

## G · MATERIALS
- **Wheel rim:** brushed antique gold/bronze #6b5320→#caa04a→#f0d27a→#fff2c2, beveled, riveted studs (gem-blue/red insets) with specular pips + **bloom**; worn edges; warm rim-light.
- **Segments (jewel pie):** semi-glossy radial gradients — amethyst #4a2a7a→#9e6bf0 (500 GEMS), steel #3a414f→#7c8696 (100K SILVER), bronze/gold #5a431c→#caa04a (EPIC CHEST), ember #7a3a14→#e0742a (COMMANDER SHARD), cobalt #1f356f→#4f8bff (250 GEMS), moss #294a2c→#5aa05f (1 DAY SPEED UP), dark-bronze #3a2c14→#8a6a30 (RARE CHEST), slate #2a2f3a→#5a6272 (EXCLUSIVE AVATAR); thin gold divider spokes between segments.
- **Lion hub:** cast gold, high relief, glossy, strong specular + bloom; small inset gem.
- **Pointer:** gold arrow/crest, beveled, slight glow at tip.
- **Left/ticker panels:** obsidian #0c0e15→#161a24, bronze edges, matte; clock glyph gold.
- **SpinFreeButton:** royal/cobalt #2b56c8→#4f8bff gloss, top highlight, beveled gold trim, soft outer glow.
- **SpinX10Button:** brushed gold (as CTA), beveled, gem icon crystalline violet.
- **Banner:** cobalt cloth #16306e→#244a9c with stitched gold trim, soft folds.
- **Icons:** gems crystalline cobalt/violet with inner glow; silver cool specular; chests aged wood + bronze; shard glowing orange crystal; hourglass gold; avatar framed bust.
- **Bloom:** wheel rim, hub, gems, CTA glows, focal halo. Matte panels do not bloom.

## H · COMPONENTS (states)
**WheelDisc** — *rest:* segment labels upright, slow idle shimmer on rim. *spinning:* fast rotation, motion-blur streaks on rim, segment labels blur. *decelerating:* ease-out into target. *landed:* target segment flashes/pulses + pointer recoils + win burst. The wheel is driven, not directly draggable in this design (button-initiated).

**SpinFreeButton:**
- *idle/enabled:* blue gloss, glow, breathing pulse (free spin available).
- *hover/focus:* +brightness, glow+.
- *pressed:* scale 0.96, inner shadow.
- *disabled* (free spin used → countdown running): desaturate to grey-blue #3a4a6a, label #6a748c, sub shows "Next free spin in 03:11:00", raycast on → tap = shake + toast.
- *spinning:* both spin buttons lock (disabled-look + spinner) until result resolves.

**SpinX10Button:**
- *idle/enabled (affordable):* gold gloss, gem cost "450", glow.
- *hover/pressed:* as gold CTA.
- *disabled-unaffordable* (gems < 450): cost text turns red #d8452b, button greyed, tap → "Not enough Gems" via shared insufficient sheet (37) with Store deep-link.
- *confirm:* tapping ×10 opens a confirm sheet (37) "Spend 450 Gems for 10 spins?" before charging (server-auth).

**CloseButton ✕:** dark disc + bronze ring; hover brighten; pressed 0.92.

**WinItem:** static; newest can slide in from left when a real win posts.

**NextSpinCard countdown:** ticks down; at 00:00:00 → free spin re-enabled, SpinFreeButton flips to enabled + glow pop.

## I · ANIMATION TIMELINE
**OnShow (~0.9 s):**
- 0.00 scrim fade-in + god-ray + focal glow (0.25 s).
- 0.05 WheelGroup scale 0.90→1.00 + α (0.30 s, ease-out-back); rim glint sweep.
- 0.10 wheel **idle slow rotation** begins (very slow, ±, ambient) OR gentle rock; lion hub specular shimmer loop (3 s).
- 0.20 Header α + 1.04→1.0 (0.20 s).
- 0.28 LeftInfoColumn slide-in from left (x −24→0 + α, 0.22 s); countdown starts ticking.
- 0.34 RightActionColumn slide-in from right (0.22 s); banner unfurl (scaleY 0.85→1.0, 0.25 s).
- 0.55 SpinFreeButton pop + glow on (0.18 s), begin breathe loop.
- 0.62 RecentWinsTicker fade-up; WinRow stagger L→R (0.04 s apart).

**OnSpin (free or ×10) — core sequence:**
- t0 button press 0.96; both buttons → disabled/locked; SFX.
- t0+0.05 **wind-up**: wheel briefly rotates *backward* ~8° (0.15 s, ease-in).
- t0+0.20 **accelerate**: spin forward, easing-in to max angular velocity over ~0.6 s; rim motion-blur on; segment labels blur.
- t0+0.80 **cruise** at max velocity ~1.0–1.4 s (constant), pointer ticks (per-segment SFX click).
- ~t0+2.2 **decelerate**: ease-out (cubic/quint) over ~2.0–2.6 s onto the server-decided target segment angle (client never picks the prize); add a tiny overshoot+settle (±2°, 0.25 s) for "click into place".
- on settle: pointer recoil bounce; **target segment flash** (white→tier color, 0.3 s) + radial burst from segment; lion hub roar-glint.
- +0.3 **reward reveal**: hand off to Reward Grant (38) / Chest-Open-Result (21) for chest/avatar; currency rewards count-up at top bar with fly-to-chip.
- ×10 variant: either spin 10× rapid sequential settles, or a single spin → a 10-item reward summary panel (implementation/ADR choice; reveal via 38 multi-grant).
- after reveal: re-enable buttons per availability (free → disabled+countdown; ×10 → enabled if gems remain).

**Idle loops:** rim shimmer; hub specular; free-button breathe; countdown tick; focal glow slow pulse; occasional WinItem slide-in.

**OnClose:** wheel stops, panel scale→0.94 + α→0, scrim out (~0.30 s).

## J · PARTICLE & FX
- Focal radial glow + god-ray behind wheel (additive, slow).
- Rim/hub specular glints (periodic streaks); gem studs twinkle.
- During spin: motion-blur smear on rim, faint sparks trailing pointer ticks.
- On landing: **win burst** (gold sparks + colored confetti tinted to the reward) radiating from the winning segment; pointer impact spark; screen-space sparkle fly to currency chip for gem/silver wins.
- CTA glows (blue breathe, gold pulse); banner subtle cloth sway.
- Dust motes drifting in dark field; vignette.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth state (read-only): free-spin availability + countdown, gem balance, recent-wins feed, segment table. Render; play entry. (Odds/segment weights are server-side and must be disclosed per ADR.)
- **OnSpin (FREE):** if available → lock buttons, request spin → server returns target segment + reward; play spin→decel-to-target→reveal; mark free used, start countdown, persist; if not available → shake + toast.
- **OnSpin (×10):** open confirm sheet (37) "Spend 450 Gems for 10 spins?"; on confirm → charge via server-auth (client never mutates balance), run ×10 sequence/summary, reveal rewards, deduct gems w/ count-down at chip; on insufficient → insufficient sheet (37) → Store.
- **Reward grant:** all grants are server-authoritative; client only animates the returned results (38/21). No client-side RNG for the prize.
- **OnClose / ✕ / back:** if a spin is in progress, ignore close until resolved (or queue); else fade out, pop screen.
- **Countdown reaches 0:** enable free spin + glow pop, no reload.
- **ADR hooks (note, do not alter spec):** posted-odds panel, no-paid-spin or pity/transparent variant, and "guaranteed" copy review all live in the ADR, not this forensic record.

## L · NEGATIVE RULES
- **⚠️ Do not ship the paid/random mechanic without the ADR.** This spec documents the art; it does not authorize gacha. Keep the flag visible to implementers.
- Do **not** let the **client pick the winning segment** — server-authoritative result only; the wheel must decelerate to the server's chosen angle.
- Do **not** charge gems before the ×10 confirm sheet; never mutate balance client-side.
- Do **not** allow spinning while a spin is unresolved (lock both buttons).
- Do **not** invent segments/odds — exactly the 8 rewards listed; no implied probabilities in the UI unless the ADR adds a posted-odds panel.
- Do **not** make the wheel free-drag/flick in this design (button-initiated only) unless ADR/UX adds it.
- Do **not** treat hub bleed as interactive (raycast off, low alpha).
- No portrait; no real-time PvP implications (this is solo gacha).

## M · ACCEPTANCE CRITERIA (≥95%)
1. 8-segment gold-rim wheel with lion-head hub + fixed top pointer; exact reward set/labels/colors as listed; segments at 45° with upright labels.
2. Left column: "NEXT FREE SPIN" + clock + "03:11:00" countdown + "How it works" with the 3 exact bullets.
3. Right column: cobalt "SPIN & WIN / GREAT REWARDS / EVERY TIME" banner; blue **SPIN — FREE** (+"1 free spin per day"); gold **SPIN ×10** with gem cost **450** (+"Better rewards guaranteed!").
4. Bottom "RECENT WINS" ticker with the 6 exact entries (name/prize/time).
5. Title "LUCKY SPIN" gold-bevel serif + subtitle; ✕ top-right; currency chips 125,450 / 2,850.
6. Spin animation = wind-up → accel → cruise → ease-out decel → settle-into-segment → win burst → reward reveal; server-authoritative landing; both buttons lock during spin.
7. Disabled states correct (free→countdown; ×10→unaffordable red); ADR gacha flag present in doc.
8. Layout within ±2% of fraction math at 2340×1080; safe-area + full-bleed-under-cutout honored; palette within ranges; wheel/CTA bloom present.

## N · IMPLEMENTATION CONFIDENCE
**88/100.** High: clear three-column comp, finite known content, standard CTA/list patterns. Deductions: (−4) the rotating wheel needs precise segment art (radial sprite or 8 pies) + correct pivot/rotation math and per-segment label orientation — fiddly to hit 1:1; (−3) the ×10 flow (sequential vs summary) and exact decel curve/overshoot are inferred from a static frame; (−3) ornate rim/hub/pointer geometry & gem studs are art-asset dependent; (−2) ADR may change mechanics/odds-panel, shifting the build. Forensic *visual* fidelity is high; the variability is in motion + ADR.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (segments, CTAs, countdown, 6 win rows); nothing invented.
- [x] Wheel typed as rotating radial/pie under center pivot; WinRow typed as HLG/marquee with rationale.
- [x] Component states (wheel, both spin buttons incl. disabled/unaffordable, ✕) enumerated.
- [x] Full spin animation timeline (wind-up→accel→cruise→decel→settle→reveal) with easings; particles/FX listed.
- [x] **GACHA ADR flag** raised prominently (header, K, L, N) without altering forensic spec.
- [x] Server-authoritative result/charge rules stated; no client RNG, no client balance mutation.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.
