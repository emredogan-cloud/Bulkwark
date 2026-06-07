# BULWARK — UI CONSTRUCTION SPEC · 30 · Free Rewards

Source: design/FreeRewardsDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops **opt-in rewarded-ads** screen — a vertical list of 4 offer rows, each a "watch a short video → earn X" action, gated by a daily cap (**3/5 today**) with a reset timer. Canon note (do NOT alter forensic spec): **rewarded ads are ALLOWED when opt-in** (00 §5 / global context) — this is the *compliant* monetization screen; copy already says "Ads are short and optional." No ADR block here; just keep it strictly opt-in (no auto-play, no forced ads).

---

## A · SCREEN PURPOSE
A full-screen (non-modal) hub sub-page that lists rewarded-ad offers. Standard hub chrome on top: **Back** (top-left) + **currency chips with +** (top-right). A **daily-limit bar** shows watches used (3/5) and a reset countdown. The body is a **vertical list of 4 offer rows**; each row = {video thumbnail w/ play overlay, title + description, reward chip, **WATCH** button, per-offer "1/1" availability}. A **RECENT WINS** footer shows recent claimed rewards + a reassurance line. Tapping **WATCH** plays an opt-in rewarded ad and grants the reward. Reached from the bottom-hub "Free" entry; **Back** returns to hub.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** near-black obsidian field with faint vignette + subtle warm top glow; no central hero subject — this is a utilitarian *list* screen, calmer than the wheel/calendar.
- **Header band:** thin top region with gold serif title + subtitle; **Back** is a gold-framed square arrow tile (top-left); currency chips top-right each with a small **+** mint button.
- **Limit bar:** a slim full-width sub-bar (calendar glyph + "DAILY LIMIT 3/5 today" left, clock + "Resets in: 14h 23m" right).
- **Offer rows:** wide dark slate panels with thin **bronze/gold edge**, rounded; left a rectangular **art thumbnail** (faction/loot scene) with a circular **gold play-triangle** overlay; center text block; a small **reward chip**; a **blue gloss WATCH** button at right with a tiny "1/1" counter beneath. Rows are visually identical in structure, varied by art/reward.
- **Footer:** dark strip, "RECENT WINS" left with mini reward chips, reassurance copy right.
- **Mood:** trustworthy, generous, low-pressure; gold accents + blue CTAs on black; the **WATCH** buttons are the brightest interactive objects.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
FreeRewardsScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (dark art, under cutout)
│  ├─ Vignette
│  └─ TopGlow (soft warm, top)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ TopBar
   │  ├─ BackButton (gold-framed ← tile, top-left)
   │  ├─ TitleBlock
   │  │  ├─ Title "FREE REWARDS" (serif gold-bevel UPPER)
   │  │  └─ Subtitle "Watch ads to earn valuable rewards!"
   │  └─ CurrencyChips (top-right, HorizontalLayoutGroup)
   │     ├─ Chip_Gold   {icon, "128,450", PlusBtn}
   │     ├─ Chip_Silver {icon, "87,360",  PlusBtn}
   │     └─ Chip_Gems   {icon, "2,850",   PlusBtn}
   ├─ DailyLimitBar
   │  ├─ Left: CalendarIcon + "DAILY LIMIT" + Count "3/5 today"
   │  └─ Right: ClockIcon + "Resets in:" + Timer "14h 23m"
   ├─ OfferList (VerticalLayoutGroup, 4 rows)  [wrap in ScrollRect if >4]
   │  ├─ OfferRow_Gem
   │  │  ├─ Thumb {art, PlayOverlay}
   │  │  ├─ Text {Title "Gem Cache", Desc "Watch a short video\nto earn Gems!"}
   │  │  ├─ RewardChip {gem icon, "+50", "Gems"}
   │  │  └─ Action {WatchButton "WATCH", AvailTag "1/1"}
   │  ├─ OfferRow_Silver
   │  │  ├─ Thumb {coins art, PlayOverlay}
   │  │  ├─ Text {Title "Silver Stash", Desc "Watch a short video\nto earn Silver!"}
   │  │  ├─ RewardChip {silver icon, "+200", "Silver"}
   │  │  └─ Action {WatchButton "WATCH", "1/1"}
   │  ├─ OfferRow_Chest
   │  │  ├─ Thumb {chest art, PlayOverlay}
   │  │  ├─ Text {Title "Free Chest", Desc "Watch a short video\nto earn a Chest!"}
   │  │  ├─ RewardChip {chest icon, "FREE", "Chest"}
   │  │  └─ Action {WatchButton "WATCH", "1/1"}
   │  └─ OfferRow_Boost
   │     ├─ Thumb {warrior art, PlayOverlay}
   │     ├─ Text {Title "Battle Boost", Desc "Watch a short video\nto earn a 60m Boost!"}
   │     ├─ RewardChip {shield/clock icon, "60m", "Speed Up"}
   │     └─ Action {WatchButton "WATCH", "1/1"}
   └─ FooterStrip
      ├─ Left: "RECENT WINS" + MiniChips ["+50"(gem), "+200"(silver), "+50"(gem), "+200"(silver), chest]
      └─ Right: InfoIcon + "Ads are short and optional. Thanks for your support!"
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| FreeRewardsScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| SafeAreaRoot | screen | 1 | Rect+SafeAreaFitter | stretch-all | .5,.5 | — | insets | drives content |
| TopBar | SafeAreaRoot | 0 | Rect | top-stretch | .5,1 | — | inside safe | height fixed frac |
| BackButton | TopBar | 0 | Button+Image | top-left | 0,1 | — | — | fixed size |
| TitleBlock | TopBar | 1 | VerticalLayoutGroup | top-left (after back) | 0,1 | left | — | — |
| CurrencyChips | TopBar | 2 | HorizontalLayoutGroup | top-right | 1,1 | mid-right | — | right-anchored |
| Chip_x | CurrencyChips | 0..2 | Image+HLG (icon,num,plus) | — | .5,.5 | center | — | — |
| DailyLimitBar | SafeAreaRoot | 1 | Image+HLG (space-between) | top-stretch (below TopBar) | .5,1 | mid spread | inside safe | spans width |
| OfferList | SafeAreaRoot | 2 | **VerticalLayoutGroup** (ctrl-size, spacing) | stretch (mid band) | .5,.5 | top-center | inside safe | rows equal height; **ScrollRect if overflow** |
| OfferRow_x | OfferList | 0..3 | Image + HorizontalLayoutGroup (LayoutElement.preferredHeight) | — | .5,.5 | mid-left→right | — | full width; internal columns fixed frac |
| ↳ Thumb | OfferRow | 0 | Image + PlayOverlay(Button) | left | 0,.5 | — | — | fixed width frac |
| ↳ PlayOverlay | Thumb | 0 | Button+Image (circle ▷) | center | .5,.5 | — | — | centered on thumb |
| ↳ Text | OfferRow | 1 | VerticalLayoutGroup | — | 0,.5 | left | — | flex-grow |
| ↳ RewardChip | OfferRow | 2 | HorizontalLayoutGroup (icon+amount+unit) | — | .5,.5 | center | — | fixed width frac |
| ↳ Action | OfferRow | 3 | VerticalLayoutGroup (button + tag) | right | 1,.5 | center | — | fixed width frac |
| ↳↳ WatchButton | Action | 0 | Button+Image | — | .5,.5 | center | — | fixed |
| ↳↳ AvailTag "1/1" | Action | 1 | Text (on small plate) | — | .5,1 | center | — | — |
| FooterStrip | SafeAreaRoot | 3 | Image+HLG (space-between) | bottom-stretch | .5,0 | mid spread | inside safe | spans width |
| RecentWins (left) | FooterStrip | 0 | HLG (label + mini chips) | left | 0,.5 | mid-left | — | — |
| InfoNote (right) | FooterStrip | 1 | HLG (icon + text) | right | 1,.5 | mid-right | — | — |

**List note:** OfferList is the canonical list → `VerticalLayoutGroup` with per-row `LayoutElement.preferredHeight`. All 4 fit at 2340×1080, so no scroll is required; **wrap in a vertical `ScrollRect`** if future offers exceed the visible band (keep header/limit-bar/footer pinned outside the viewport). Currency chips + RecentWins mini-chips are small `HorizontalLayoutGroup`s.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar:** height ≈ 0.13·H ≈ 140 px; top inset ≈ 0.02·H.
  - BackButton ⌀/side ≈ 0.075·H ≈ 81 px square; left inset ≈ 0.025·W.
  - TitleBlock left of center, after Back: title cap ≈ 0.06·H, subtitle ≈ 0.028·H below.
  - CurrencyChips: each chip width ≈ 0.13·W; height ≈ 0.05·H; gap ≈ 0.01·W; +button ⌀ ≈ 0.035·H at chip right; right inset ≈ 0.02·W.
- **DailyLimitBar:** full inner width (≈0.95·W), height ≈ 0.055·H ≈ 60 px, y just below TopBar. Left cluster (calendar + "DAILY LIMIT" + "3/5 today") and right cluster (clock + "Resets in:" + "14h 23m") space-between with side insets 0.03·W.
- **OfferList:** occupies the central band from ≈0.20·H (below limit bar) to ≈0.88·H (above footer) = height ≈ 0.68·H ≈ 734 px.
  - 4 rows + 3 gaps. Gap ≈ 0.018·H ≈ 19 px → gaps ≈ 57 px → rows share ≈ 677 px → **row height ≈ 169 px (~0.157·H)**. Row width ≈ 0.94·W ≈ 2200 px.
  - **Row internal columns** (fractions of row width): Thumb 0.00–0.14 (≈ 308 px, ~16:9 inset with padding); Text 0.16–0.52 (flex); RewardChip 0.54–0.70 (≈ 350 px); Action 0.78–0.98 (≈ 440 px). PlayOverlay circle ⌀ ≈ 0.55·thumbH centered on thumb.
  - WatchButton: width ≈ 0.14·W ≈ 328 px, height ≈ 0.085·H ≈ 92 px; AvailTag "1/1" plate ≈ 0.06·W × 0.03·H centered beneath.
  - RewardChip: amount text large; unit label small beneath; icon ⌀ ≈ 0.06·H.
- **FooterStrip:** full inner width, height ≈ 0.09·H ≈ 97 px, bottom inset ≈ 0.02·H. Mini chips ⌀ ≈ 0.035·H each, gap ≈ 0.008·W.

**Tablet (4:3):** match-height keeps row heights; rows get more side margin (cap width 0.9·W); thumbnail/columns scale with W. **Ultrawide (21:9):** rows widen but cap max width (~0.8·W centered) so text doesn't stretch absurdly; backdrop fills sides. **Notch:** SafeAreaRoot insets the whole content; full-bleed backdrop under cutout; Back/chips never cross the inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "FREE REWARDS" | serif Trajan display | Black | UPPER | +5% | 1.0 | gold bevel + bloom + 2px stroke | ~64 | #f0d27a→#caa04a; stroke #3a2c0e |
| Subtitle | clean sans/light serif | Reg | Sentence | 0 | 1.1 | shadow | ~24 | #d9c79a |
| Currency numbers | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |
| "DAILY LIMIT" | condensed caps | SemiBold | UPPER | +4% | 1.0 | shadow | ~22 | #cdbf99 |
| Count "3/5 today" | numeric, emphatic | Bold | lower "today" | +1% | 1.0 | "3" brighter | ~24 | "3/5" #ffd76a, "today" #b3ac96 |
| "Resets in:" | clean sans | Reg | Sentence | 0 | 1.0 | — | ~20 | #b3ac96 |
| Timer "14h 23m" | numeric | SemiBold | — | +1% | 1.0 | soft glow | ~24 | #ffe08a |
| Offer Title (e.g. "Gem Cache") | serif small-caps, sturdy | Bold | Title | +2% | 1.0 | shadow | ~32 | #f2e8cf |
| Offer Desc (2 lines) | clean sans, quiet | Reg | Sentence | 0 | 1.15 | — | ~20 | #a9a28c |
| Reward amount "+50"/"+200"/"FREE"/"60m" | numeric/label bold | Heavy | UPPER (FREE) | 0 | 1.0 | glow; color by reward | ~34 | gem #6f8fff, silver #d6dae2, FREE #5fbf6a, boost #6f8fff |
| Reward unit ("Gems"/"Silver"/"Chest"/"Speed Up") | clean sans small | Reg | Title | 0 | 1.0 | — | ~18 | #9a937f |
| "WATCH" | serif display, action | Heavy | UPPER | +6% | 1.0 | top highlight + glow | ~32 | #ffffff on blue |
| AvailTag "1/1" | numeric small | SemiBold | — | 0 | 1.0 | shadow | ~18 | #cfe0ff |
| "RECENT WINS" | condensed caps | SemiBold | UPPER | +5% | 1.0 | — | ~20 | #cdbf99 |
| InfoNote "Ads are short and optional. Thanks for your support!" | clean sans, friendly | Reg | Sentence | 0 | 1.1 | — | ~18 | #8f8872 |

## G · MATERIALS
- **Backdrop:** obsidian #0a0b0f→#14161e, matte, vignette; faint warm top glow #2a2416 additive.
- **Back tile / chips / limit bar:** dark slate #11141d with **bronze/gold edge** #7a5f28→#caa04a; chips have a subtle inner gradient; **+** mint buttons are small gold/green beveled discs.
- **Offer row panel:** #10131c→#181c27 vertical, thin gold-bronze 1–2px edge, rounded corners, soft inner top sheen; faint inner glow on the row whose offer is freshly available.
- **Thumbnail:** full-color loot/faction art, slightly darkened with a vignette; bronze inner frame; **PlayOverlay** = translucent dark disc + gold play-triangle #f0d27a with soft glow.
- **RewardChip:** tiny dark plate or frameless; icons crystalline (gems cobalt/violet inner glow, silver cool specular, chest aged-wood+bronze, boost shield/clock gold-blue).
- **WatchButton:** royal/cobalt #2b56c8→#4f8bff gloss, top highlight, beveled gold/steel trim, soft outer glow; AvailTag on a small dark plate.
- **Bloom:** play-triangles, reward icons, WATCH glows, timer text. Matte panels/backdrop do not bloom.
- **Wear:** subtle worn edges on metal trims; rim-light top-left on bevels.

## H · COMPONENTS (states)
**OfferRow** (informational container; interactive children = PlayOverlay + WatchButton, which trigger the same action):
- *available* (offer's "1/1" unused, under daily cap): full color, WATCH enabled (blue, glow), play-triangle bright.
- *used* (offer's per-day count exhausted, e.g. "0/1"): row dims to ~70%, WATCH → disabled grey-blue, AvailTag "0/1", play-triangle desaturated; tag may show next-reset.
- *cap-reached* (global 3/5 → 5/5): ALL WATCH buttons disabled grey, a banner/tooltip "Daily limit reached — resets in 14h 23m"; rows dim.
- *loading-ad:* WATCH shows spinner + "Loading…", row locks; on no-fill → toast "No ad available, try again" (re-enable).

**WatchButton:**
- *idle/enabled:* blue gloss + glow + subtle breathe.
- *hover/focus:* +brightness, glow+.
- *pressed:* scale 0.96 + inner shadow.
- *disabled:* desaturate grey-blue #3a4a6a, label #6a748c, glow off, raycast on → tap = shake + toast.
- *success (post-watch):* flash white→blue, reward chip pops + flies to currency, AvailTag decrements (1/1→0/1), DailyLimit count increments (3/5→4/5).

**PlayOverlay:** mirrors WatchButton state (same action); hover = triangle brighten + disc darken; disabled when offer used.

**BackButton:** gold tile; hover brighten; pressed 0.92; returns to hub.

**CurrencyChip + / DailyLimit count / Timer:** chips' **+** deep-links to Store; count + timer are live displays (timer ticks; count updates on each successful watch; at reset → counts restore, rows re-enable).

## I · ANIMATION TIMELINE
**OnShow (~0.7 s):**
- 0.00 backdrop fade + top glow (0.20 s).
- 0.05 TopBar slide-down (y +12→0 + α, 0.20 s); chips count-up optional.
- 0.15 DailyLimitBar fade-in (0.15 s); timer starts ticking.
- 0.22 OfferList rows stagger top→bottom: each x −20→0 + α, 0.05 s apart, 0.18 s each (ease-out).
- 0.50 WATCH buttons glow-on + begin breathe loop; play-triangles subtle pulse.
- 0.58 FooterStrip fade-up (0.15 s).

**Idle loops:** WATCH breathe (1.6 s); play-triangle soft pulse (1.4 s); timer tick; reward-icon twinkle (slow); subtle row inner-glow on the top available offer.

**OnWatch (per row, see K):**
- press 0.96 (0.08 s) → WATCH→"Loading…" spinner; row locks.
- ad SDK plays (external) → on complete: return to screen, WATCH flash white→blue, **reward chip pop** (scale 1.0→1.25→1.0, 0.3 s) + sparkle → **fly-to-currency** at top bar (0.5 s) with count-up; AvailTag 1/1→0/1 (cross-fade); DailyLimit 3/5→4/5 (number flip 0.2 s).
- if 5/5 reached → all WATCH dim (0.2 s) + show "limit reached" note.
- on ad dismissed-early/no-reward: re-enable WATCH, no grant, toast.

**OnClose:** rows + bars fade/slide out (~0.25 s); pop screen.

## J · PARTICLE & FX
- Soft warm top glow + vignette + faint dust motes.
- Play-triangle glow pulse; reward-icon twinkles (gem/silver sparkle).
- On successful watch: **reward burst** (small gold/colored sparks) from the row's reward chip + a screen-space sparkle trail flying to the matching top-bar currency chip; count-flip flash.
- WATCH button breathing rim bloom; chip **+** subtle glint.
- No heavy FX — this is a calm utility screen.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth state (read-only): per-offer availability (x/1), global daily count (n/5), reset timer, balances, recent-wins feed. Render rows + states; play entry.
- **OnWatch (PlayOverlay or WATCH):** if offer available AND under cap → set Loading, request rewarded ad via SDK (opt-in, user-initiated); on **reward-earned callback** → server-auth grants reward (client never mutates balance), animate chip→currency, decrement offer avail, increment daily count, persist; on **no-fill / closed-early / error** → re-enable, toast (shared error sheet 39 if needed), no grant.
- **Cap reached (n/5 → 5/5):** disable all WATCH, show limit note; re-enable at reset (timer→0 restores counts via server).
- **Chip +:** deep-link to Store (17). **Back:** fade out, pop to hub.
- **Strictly opt-in:** no ad auto-plays; nothing watches without an explicit WATCH/▷ tap (canon-allowed condition). Reward only granted on genuine completion callback.
- **Timer tick:** client displays server reset time; does not authorize resets locally.

## L · NEGATIVE RULES
- Do **not** auto-play or pre-roll ads — **opt-in only** (the canon allowance depends on this); never grant without the SDK completion callback.
- Do **not** grant rewards client-side or mutate balances on the client — server-authoritative.
- Do **not** exceed the daily cap (5) or a per-offer cap (1) regardless of UI taps; enforce server-side.
- Do **not** invent extra offers/rewards — exactly the 4 rows + values: +50 Gems, +200 Silver, FREE Chest, 60m Speed Up.
- Do **not** restyle the 60m Boost reward as energy/stamina (the canon-cut meter) — it's a time-skip "Speed Up" item, render as such (document if it later collides with cut systems).
- Do **not** drop the reassurance line / "DAILY LIMIT 3/5" / reset timer — they are part of the compliant, transparent presentation.
- No portrait; no real-time anything.

## M · ACCEPTANCE CRITERIA (≥95%)
1. Header: Back tile (top-left), title "FREE REWARDS" gold-bevel serif + subtitle, 3 currency chips with + (top-right) showing 128,450 / 87,360 / 2,850.
2. DailyLimit bar: calendar + "DAILY LIMIT 3/5 today" (left) + clock + "Resets in: 14h 23m" (right).
3. Exactly 4 offer rows in order with correct art-role, titles, 2-line descriptions, reward chips (+50 Gems / +200 Silver / FREE Chest / 60m Speed Up), blue WATCH + "1/1" tag each; play-triangle on each thumbnail.
4. Footer: "RECENT WINS" + 5 mini chips + "Ads are short and optional. Thanks for your support!".
5. Layout within ±2% of fraction math at 2340×1080; rows equal height; VLG (ScrollRect-ready); safe-area + full-bleed-under-cutout honored.
6. States correct: available/used/cap-reached/loading + disabled WATCH; success animation (chip→currency, count increments) present.
7. Palette within ranges; play-triangle/reward/WATCH bloom present; matte backdrop not blooming.
8. Strictly opt-in behavior documented; server-auth grant; no client balance mutation.

## N · IMPLEMENTATION CONFIDENCE
**93/100.** High: classic offer-row list, finite known content, exact labels/values, standard layout groups + CTA states. Deductions: (−3) thumbnail art + reward icons are asset-dependent (need matching sprites); (−2) exact ad-SDK integration / reward-callback wiring is platform-specific (out of pure UI scope) though the UI states are fully specified; (−2) precise reset/cap restore choreography and row inner-glow radii are inferred from a static frame.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (4 offers, caps, timer, chips, footer); nothing invented.
- [x] List typed as VLG (ScrollRect-ready) with rationale; chip rows typed as HLG.
- [x] Component states (rows available/used/cap, WATCH idle/hover/pressed/disabled/loading/success, Back, chips) enumerated.
- [x] Animation timeline with timestamps/easing; particle/FX listed.
- [x] Canon note recorded (rewarded ads ALLOWED iff opt-in) without altering forensic spec; boost-vs-energy nuance flagged.
- [x] Server-auth + strictly-opt-in rules stated; no client balance mutation; reward only on completion.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.
