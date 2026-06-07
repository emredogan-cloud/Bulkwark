# BULWARK — UI CONSTRUCTION SPEC · 31 · Events Hub

Source: design/EventsHubDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops events hub: a large **featured event banner** over a grid of **event cards**, with bottom tabs **Events / Calendar / Past Events**. Canon notes (do NOT alter forensic spec): (1) the **58/120 chip** in the top bar reads as an energy/stamina meter → **canon-CUT**; document, flag, omit at implementation. (2) The **"Arena Clash"** card copy says *"Compete against other players in real-time battles"* — BULWARK has **NO real-time PvP** (00 context). Flag: at implementation relabel to **async ghost** ("Compete against other Commanders" / "async"), per the Online-Battle convention. Forensic spec records the art verbatim.

---

## A · SCREEN PURPOSE
A full-screen hub sub-page for limited-time events. Top: hub chrome (**Back** left, currency chips right) + gold serif title "EVENTS" + subtitle. A prominent **FEATURED** banner promotes the current marquee event ("DOUBLE SILVER WEEKEND") with a countdown. Below a "MORE EVENTS" divider sits a **row of 4 event cards**, each with art, name, description, a small **REWARDS** preview row (3 icons), a **PLAY** CTA, and its own end-timer. A **bottom tab bar** switches between **Events** (active), **Calendar**, and **Past Events**. Reached from the bottom-hub "Events" entry; **Back** returns to hub; **PLAY** routes into the event's mode/flow.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** dark obsidian field, vignette, faint warm top glow; the featured banner brings the only large color/art block.
- **Featured banner:** a wide cinematic panel — left a rain-lit battlefield/wagon scene, center huge **two-line gold-bevel serif** title with a blue corner **FEATURED** ribbon, a small **"Ends in 1d 12h"** clock pill, and on the right a **glowing silver "2×" emblem** (silver lion coin in a cobalt energy ring with bloom). Ornate gold frame around the whole banner.
- **Divider:** "MORE EVENTS" centered with flanking gold rule lines.
- **Event cards:** vertical dark slate panels, bronze edge, rounded; top = event art with a soft gradient; some carry a small **NEW** ribbon (Endless Rush); name (gold serif), 2-line description, a **REWARDS** label + 3 reward icons (trophy/shard/chest tiers), a **blue PLAY** CTA, and a bottom **"Ends in …"** clock line. Cards are structurally identical, varied by art/copy/timer.
- **Tab bar:** dark bottom strip; active tab (**Events**) gold-lit with an underline/emblem; inactive tabs (Calendar, Past Events) muted with their glyphs.
- **Mood:** "what's hot right now" — cinematic featured hero + tidy card shelf; PLAY buttons + the 2× emblem are the brightest focal objects.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
EventsHubScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (dark art, under cutout)
│  ├─ Vignette
│  └─ TopGlow
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ TopBar
   │  ├─ BackButton (gold ← tile, top-left)
   │  ├─ TitleBlock {Title "EVENTS", Subtitle "Limited-time events with epic rewards!"}
   │  └─ CurrencyChips (top-right, HLG)
   │     ├─ Chip_Gold   {icon, "128,450", +}
   │     ├─ Chip_Gems   {icon, "2,850",   +}
   │     └─ EnergyChip  {icon, "58/120",  +}   ⚠️CANON-CUT
   ├─ FeaturedBanner (ornate framed wide panel)
   │  ├─ BannerArt (battlefield scene, left/full)
   │  ├─ FeaturedRibbon "FEATURED" (blue corner ribbon, top-left)
   │  ├─ BannerTitle "DOUBLE\nSILVER WEEKEND" (2-line gold serif)
   │  ├─ BannerSubtitle "Earn DOUBLE the Silver in all battles!"
   │  ├─ BannerTimerPill {clock, "Ends in 1d 12h"}
   │  └─ MultiplierEmblem "2×" (silver lion coin + cobalt energy ring + glow)
   ├─ MoreEventsDivider {ruleLeft, "MORE EVENTS", ruleRight}
   ├─ EventCardRow (HorizontalLayoutGroup, 4 cards)  [ScrollRect if >4]
   │  ├─ Card_EndlessRush
   │  │  ├─ Art + NewRibbon "NEW"
   │  │  ├─ Name "Endless Rush"
   │  │  ├─ Desc "Survive endless waves of\nenemies and climb the leaderboard!"
   │  │  ├─ RewardsRow {label "REWARDS", icon trophy, icon shard, icon chest}
   │  │  ├─ PlayButton "PLAY"
   │  │  └─ Timer {clock, "Ends in 1d 12h"}
   │  ├─ Card_HeroTrials
   │  │  ├─ Art
   │  │  ├─ Name "Hero Trials"
   │  │  ├─ Desc "Win with fixed heroes and\nearn exclusive hero shards!"
   │  │  ├─ RewardsRow {"REWARDS", icon(2 badge), shard, chest}
   │  │  ├─ PlayButton "PLAY"
   │  │  └─ Timer {clock, "Ends in 2d 12h"}
   │  ├─ Card_ResourceRun
   │  │  ├─ Art
   │  │  ├─ Name "Resource Run"
   │  │  ├─ Desc "Gather as many resources as\nyou can before time runs out!"
   │  │  ├─ RewardsRow {"REWARDS", icon coin, gem, chest}
   │  │  ├─ PlayButton "PLAY"
   │  │  └─ Timer {clock, "Ends in 12h 45m"}
   │  └─ Card_ArenaClash  ⚠️"real-time" → relabel async
   │     ├─ Art
   │     ├─ Name "Arena Clash"
   │     ├─ Desc "Compete against other players\nin real-time battles for ranked rewards!"  ⚠️
   │     ├─ RewardsRow {"REWARDS", icon rank-gem, shard, chest}
   │     ├─ PlayButton "PLAY"
   │     └─ Timer {clock, "Ends in 3d 12h"}
   └─ TabBar (bottom, 3 tabs)
      ├─ Tab_Events    (ACTIVE — book/scroll glyph, gold-lit, underline)
      ├─ Tab_Calendar  (calendar glyph, muted)
      └─ Tab_PastEvents (clock-history glyph, muted)
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| EventsHubScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| SafeAreaRoot | screen | 1 | Rect+SafeAreaFitter | stretch-all | .5,.5 | — | insets | drives content |
| TopBar | SafeAreaRoot | 0 | Rect | top-stretch | .5,1 | — | inside safe | fixed frac height |
| BackButton | TopBar | 0 | Button+Image | top-left | 0,1 | — | — | fixed |
| TitleBlock | TopBar | 1 | VerticalLayoutGroup | top-center/left | .5,1 | center | — | — |
| CurrencyChips | TopBar | 2 | HorizontalLayoutGroup | top-right | 1,1 | mid-right | — | right-anchored |
| FeaturedBanner | SafeAreaRoot | 1 | Image (9-slice frame) + children | top-stretch (below TopBar) | .5,1 | — | inside safe | width ~full, fixed aspect |
| BannerArt | FeaturedBanner | 0 | Image (masked to frame) | stretch-all | .5,.5 | — | — | cover-fill in frame |
| FeaturedRibbon | FeaturedBanner | 1 | Image+Text | top-left | 0,1 | — | — | corner-pinned |
| BannerTitle | FeaturedBanner | 2 | Text (TMP) | center-left | 0,.5 | left | — | autosize |
| BannerSubtitle | FeaturedBanner | 3 | Text | center-left (below title) | 0,.5 | left | — | — |
| BannerTimerPill | FeaturedBanner | 4 | Image+HLG (clock+text) | center-low | .5,.5 | center | — | — |
| MultiplierEmblem | FeaturedBanner | 5 | Image (glow) + Text "2×" | mid-right | 1,.5 | center | — | right-pinned |
| MoreEventsDivider | SafeAreaRoot | 2 | HLG (rule+text+rule) | top-stretch (below banner) | .5,1 | center | — | spans width |
| EventCardRow | SafeAreaRoot | 3 | **HorizontalLayoutGroup** (4 equal) | stretch (mid-low band) | .5,.5 | mid-center | inside safe | cards equal; **ScrollRect if >4** |
| Card_x | EventCardRow | 0..3 | Image + VerticalLayoutGroup (LayoutElement) | — | .5,.5 | top-center | — | equal width; internal frac rows |
| ↳ Art (+NewRibbon) | Card | 0 | Image (+corner ribbon) | top-stretch | .5,1 | — | — | top band of card |
| ↳ Name | Card | 1 | Text | — | .5,.5 | center | — | — |
| ↳ Desc | Card | 2 | Text (2-line) | — | .5,.5 | center | — | wrap |
| ↳ RewardsRow | Card | 3 | HLG (label + 3 icons) | — | .5,.5 | center | — | — |
| ↳ PlayButton | Card | 4 | Button+Image | — | .5,.5 | center | — | full card width inset |
| ↳ Timer | Card | 5 | HLG (clock+text) | — | .5,0 | center | — | — |
| TabBar | SafeAreaRoot | 4 | Image + HorizontalLayoutGroup (3 equal) | bottom-stretch | .5,0 | mid-center | inside safe | spans width |
| Tab_x | TabBar | 0..2 | Toggle/Button + Image + Text | — | .5,.5 | center | — | equal thirds |

**List note:** EventCardRow is the canonical card list → `HorizontalLayoutGroup` with equal `LayoutElement` widths; all 4 fit at 2340×1080. **Wrap in a horizontal `ScrollRect`** if events exceed 4 (banner/divider/tabs stay pinned outside the viewport). TabBar = a toggle group (single-select). CurrencyChips + RewardsRow = small HLGs.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar:** height ≈ 0.11·H ≈ 119 px; top inset ≈ 0.02·H. Back tile ≈ 0.075·H square, left inset 0.025·W. Title cap ≈ 0.055·H centered; subtitle below ≈ 0.026·H. Chips: each ≈ 0.115·W × 0.05·H, gap 0.008·W, right inset 0.02·W.
- **FeaturedBanner:** width ≈ **0.95·W ≈ 2223 px**, height ≈ **0.30·H ≈ 324 px**; top y ≈ below TopBar (≈0.13·H). Gold frame ≈ 0.012·H thick.
  - FeaturedRibbon: top-left corner, length ≈ 0.10·W, angled 0° banded (or 45° corner) — here a horizontal tab ≈ 0.10·W × 0.045·H.
  - BannerTitle: left-center, 2 lines, cap ≈ 0.075·H each, left inset ≈ 0.06·bannerW.
  - BannerSubtitle: below title, ≈ 0.03·H.
  - BannerTimerPill: low-center area, ≈ 0.14·W × 0.045·H.
  - MultiplierEmblem: right ~0.20·bannerW zone, "2×" cap ≈ 0.16·H, coin/ring ⌀ ≈ 0.22·H, right inset ≈ 0.05·bannerW; bloom halo extends ~+30%.
- **MoreEventsDivider:** y ≈ 0.46·H; rule lines flank centered text; text cap ≈ 0.026·H.
- **EventCardRow:** band y ≈ 0.49·H → 0.90·H (height ≈ 0.41·H ≈ 443 px), width ≈ 0.95·W.
  - 4 cards + 3 gaps; gap ≈ 0.015·W ≈ 35 px → gaps ≈ 105 px → cards share ≈ 2118 px → **card width ≈ 530 px (~0.226·W)**. Card height ≈ full band ≈ 443 px → card aspect ≈ 0.55:1 (portrait).
  - Card internal (fractions of card H): Art 0.00–0.34; Name 0.36–0.44; Desc 0.45–0.60 (2 lines); RewardsRow 0.62–0.74 (label + 3 icons ⌀ ≈ 0.07·cardW each); PlayButton 0.76–0.90 (width ≈ 0.84·cardW, height ≈ 0.12·cardH); Timer 0.92–1.00.
  - NewRibbon (Endless Rush): top-left of Art, ≈ 0.30·cardW × 0.07·cardH.
- **TabBar:** height ≈ 0.085·H ≈ 92 px, bottom inset ≈ 0.01·H; 3 equal tabs; active tab gold underline ≈ 0.4·tabW × 4 px, glyph ⌀ ≈ 0.05·H above label.

**Tablet (4:3):** match-height keeps banner/card heights; cards gain side margin (cap row width 0.9·W). **Ultrawide (21:9):** banner caps max width (~0.8·W centered); cards may show a 5th if present (ScrollRect) or center with margins; backdrop fills sides. **Notch:** SafeAreaRoot insets all content; full-bleed backdrop under cutout; Back/chips/tabs never cross inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "EVENTS" | serif Trajan display | Black | UPPER | +6% | 1.0 | gold bevel + bloom + 2px stroke | ~64 | #f0d27a→#caa04a; stroke #3a2c0e |
| Subtitle | clean sans/light serif | Reg | Sentence | 0 | 1.1 | shadow | ~24 | #d9c79a |
| Currency numbers | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |
| "FEATURED" ribbon | condensed caps | Bold | UPPER | +6% | 1.0 | on cobalt, slight bevel | ~22 | #eaf2ff on #2b56c8 |
| BannerTitle "DOUBLE / SILVER WEEKEND" | serif display, epic | Black | UPPER | +4% | 0.95 | strong gold/silver bevel + bloom + stroke | line1 ~58, line2 ~64 | #e8e2cf→#cfd3dc (silvery gold); stroke #2a2c33 |
| BannerSubtitle | light serif/clean | Reg | Sentence | 0 | 1.1 | shadow | ~26 | #d9c79a |
| BannerTimerPill "Ends in 1d 12h" | numeric/caps | SemiBold | Sentence | +1% | 1.0 | on dark pill, soft glow | ~22 | #ffe08a |
| "2×" emblem | serif display, huge | Black | — | 0 | 1.0 | gold bevel + cobalt glow + bloom | ~120 | #f4e6b0 |
| "MORE EVENTS" | condensed caps | SemiBold | UPPER | +6% | 1.0 | flank rules | ~24 | #cdbf99 |
| "NEW" ribbon | condensed caps | Bold | UPPER | +4% | 1.0 | on blue/gold tag | ~18 | #ffffff |
| Card Name | serif small-caps, sturdy | Bold | Title | +2% | 1.0 | shadow | ~30 | #f2e8cf |
| Card Desc (2 lines) | clean sans, quiet | Reg | Sentence | 0 | 1.15 | — | ~18 | #a9a28c |
| "REWARDS" | condensed caps small | SemiBold | UPPER | +5% | 1.0 | — | ~16 | #b3ac96 |
| "PLAY" | serif display, action | Heavy | UPPER | +6% | 1.0 | top highlight + glow | ~30 | #ffffff on blue |
| Card Timer "Ends in …" | numeric small | Reg | Sentence | 0 | 1.0 | clock glyph left | ~18 | #cdbf99 |
| Tab labels (Events/Calendar/Past Events) | condensed caps | SemiBold (active Bold) | UPPER | +4% | 1.0 | active gold + underline; inactive muted | ~22 | active #f0d27a, inactive #7d7768 |

## G · MATERIALS
- **Backdrop:** obsidian #0a0b0f→#14161e, matte, vignette; warm top glow #2a2416 additive.
- **Banner frame:** brushed gold/bronze #6b5320→#caa04a→#f0d27a→#fff2c2, beveled, worn edges, rim-light; **bloom** on highlights.
- **Banner art:** full-color rainy battlefield/wagon scene, darkened toward edges + inner vignette so title reads; cool blue ambient.
- **FeaturedRibbon:** cobalt #1f356f→#2b56c8 cloth/metal tab, stitched/gold trim.
- **Multiplier emblem:** silver lion coin #b8bcc6→#e8ebf0 high specular, set in a **cobalt energy ring** #2b56c8→#4f8bff with additive glow + arc sparks + bloom.
- **TimerPill:** dark translucent plate #11141d α.8, bronze edge, gold text.
- **Event card panel:** #10131c→#181c27 vertical, thin gold-bronze edge, rounded; soft inner top sheen; card art with bottom gradient fade into panel.
- **NewRibbon:** small blue/gold beveled tag.
- **Reward icons:** trophy gold, shards glowing crystal (orange/violet), chests aged-wood+bronze, gems cobalt/violet, coins gold/silver — all with slight bloom.
- **PlayButton:** royal/cobalt #2b56c8→#4f8bff gloss, top highlight, beveled gold/steel trim, outer glow.
- **TabBar:** dark slate #0c0e15→#161a24, bronze top edge; active tab gold glyph + label + thin gold underline + faint glow; inactive desaturated.
- **Bloom:** banner frame/emblem/title, reward icons, PLAY glows, active tab. Matte panels/backdrop do not bloom.

## H · COMPONENTS (states)
**FeaturedBanner:** quasi-button (tap → featured event detail/play). *idle:* art parallax-still, emblem glow pulse, timer ticking. *hover/focus:* frame brighten + slight scale 1.01. *pressed:* scale 0.99. *expired* (timer→0): emblem dims, "ENDED" overlay, tap disabled until refresh.

**EventCard:**
- *active:* full color, PLAY enabled (blue glow), timer ticking; NEW ribbon shown where applicable.
- *hover/focus:* card lift (y −6) + edge brighten + art subtle zoom.
- *pressed:* scale 0.98.
- *ending-soon* (timer < threshold): timer text turns ember #e0742a + subtle pulse.
- *expired:* card dims ~60%, PLAY → disabled grey, "ENDED" stamp; removed/moved to Past tab on refresh.
- *locked* (level/req not met, if any): PLAY → "LOCKED" + lock glyph, tap → requirement tooltip.

**PlayButton:** idle blue gloss + glow + breathe; hover +brightness; pressed 0.96; disabled grey-blue (expired/locked) → tap shake + tooltip.

**TabBar tabs (toggle group):** active = gold glyph+label+underline+glow (Events); inactive = muted, hover brighten; selecting Calendar/Past Events cross-fades the body content (banner+cards swap for a calendar list / past-events list) while header+tabs persist.

**BackButton:** gold tile; hover brighten; pressed 0.92 → hub.

**CurrencyChip + / timers:** + → Store; all timers tick live (banner + per-card); on expiry trigger the expired states above. EnergyChip flagged CUT.

## I · ANIMATION TIMELINE
**OnShow (~0.85 s):**
- 0.00 backdrop fade + top glow (0.20 s).
- 0.05 TopBar slide-down (0.20 s); chips count-up optional.
- 0.12 FeaturedBanner: scale 0.97→1.00 + α (0.30 s, ease-out); BannerArt subtle ken-burns drift starts; FeaturedRibbon snap-in.
- 0.30 BannerTitle α + slight 1.03→1.0 (0.20 s); MultiplierEmblem pop (0.9→1.0) + glow ring spin-up + bloom (0.30 s).
- 0.40 TimerPill fade-in; timer ticking.
- 0.46 MoreEventsDivider rule lines wipe outward from center (0.20 s) + text fade.
- 0.52 EventCardRow cards stagger L→R: each y +18→0 + α, 0.05 s apart, 0.18 s each (ease-out); art subtle zoom-settle.
- 0.78 PLAY buttons glow-on + breathe; NEW ribbons pop.
- 0.80 TabBar fade-up; active tab underline draw-in (0.18 s).

**Idle loops:** emblem ring slow rotation + arc-spark twinkle + glow pulse (2 s); banner art ken-burns (slow, looping); PLAY breathe (1.6 s); ending-soon timer pulse; active-tab faint glow breathe; dust motes.

**OnPlay (see K):** PLAY press 0.96 → card flash white→blue → screen transition (push) into the event mode (handoff to mode/match flow).

**OnTabSwitch:** old tab underline retract + body cross-fade out (0.15 s) → new body cross-fade in + stagger (0.20 s) + new underline draw-in; header/tabs persist.

**OnClose:** banner+cards+tabs fade/slide out (~0.30 s); pop screen.

## J · PARTICLE & FX
- Banner: rain/ambient in art (baked), **emblem energy ring** sparks + glow + bloom, occasional gold glint sweep across frame.
- Card art: subtle floating embers/dust per theme (baked or light particles); NEW ribbon shimmer.
- Reward icons: small twinkles (slow).
- PLAY glows (blue breathe); active-tab glow.
- Divider wipe spark; dust motes in dark field; vignette.
- Keep FX restrained on cards (legibility); concentrate spectacle on the featured banner.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth event config (read-only): featured event (title/art/multiplier/timer), active event list (name/desc/rewards/timer/flags: NEW/locked), tab availability. Render; play entry. (Client never authors events; it renders the live-ops payload.)
- **OnPlay (card or banner):** validate availability/requirements → push into the event's mode/flow (mode-select/match). For ranked/competitive events the result routes to the relevant async result screen.
- **OnTabSwitch:** Events ↔ Calendar ↔ Past Events — swap body content (cards / calendar schedule / past-events log) without leaving the screen; persist header+tabs.
- **Timer ticks:** live countdowns; on expiry → expired card/banner state; refresh may move expired events to Past Events.
- **Chip +:** Store deep-link. **Back:** fade out → hub.
- **⚠️ Arena Clash relabel (note, do not alter art):** at implementation the "real-time" copy must become **async ghost** wording to match canon (no real-time PvP); behavior routes to the async-battle pipeline (32). EnergyChip omitted/CUT.

## L · NEGATIVE RULES
- Do **not** render the **EnergyChip (58/120)** as a live system — **canon-CUT**; omit/replace at implementation (kept here only as forensic record).
- Do **not** ship "Arena Clash" as **real-time PvP** — BULWARK is **async ghost**; relabel + route async (the art text is recorded verbatim but flagged).
- Do **not** author events client-side or fabricate timers/rewards — render the server-auth live-ops payload only.
- Do **not** invent extra cards/rewards — exactly the featured banner + 4 cards with the listed names/descriptions/timers shown.
- Do **not** let expired events remain playable; gate via server state.
- Do **not** make Calendar/Past Events forget the header/tab chrome (they're body swaps, not new screens).
- No portrait; concentrate spectacle on the banner, keep cards legible.

## M · ACCEPTANCE CRITERIA (≥95%)
1. Header: Back (left), title "EVENTS" gold-bevel serif + subtitle, 3 chips (128,450 / 2,850 / 58/120) right.
2. Featured banner: ornate frame, blue "FEATURED" ribbon, 2-line "DOUBLE / SILVER WEEKEND", subtitle "Earn DOUBLE the Silver in all battles!", "Ends in 1d 12h" pill, glowing silver **2×** emblem right.
3. "MORE EVENTS" divider, then exactly 4 cards in order (Endless Rush[NEW] / Hero Trials / Resource Run / Arena Clash) with correct names, 2-line descriptions, REWARDS row (3 icons), blue PLAY, and per-card "Ends in …" timer.
4. Bottom tab bar: Events(active, gold/underline) / Calendar / Past Events with glyphs.
5. Layout within ±2% of fraction math at 2340×1080; cards equal width (HLG, ScrollRect-ready); safe-area + full-bleed-under-cutout honored.
6. States correct: card active/hover/ending-soon/expired/locked; PLAY disabled; tab switching body cross-fade; expired banner.
7. Palette within ranges; banner/emblem/PLAY/active-tab bloom present; matte panels not blooming.
8. Energy chip flagged/omitted; Arena-Clash async relabel flagged.

## N · IMPLEMENTATION CONFIDENCE
**90/100.** High: standard hub sub-page (header + hero banner + card shelf + tabs), finite known content, exact labels/timers. Deductions: (−4) the featured-banner art + glowing 2× emblem ring need bespoke assets/shader for 1:1 (bloom/spark/ken-burns); (−3) Calendar/Past-Events tab bodies are not shown in this frame — their layouts are inferred (will need their own sub-specs); (−2) reward-icon sets per card are asset-dependent; (−1) Arena-Clash relabel is a content/canon edit governed outside this forensic record.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (banner, 4 cards, timers, tabs, chips); nothing invented.
- [x] Card list typed as HLG (ScrollRect-ready); tab bar typed as toggle group; chip/reward rows as HLG.
- [x] Component states (banner, cards active/hover/ending/expired/locked, PLAY, tabs, Back) enumerated.
- [x] Animation timeline with timestamps/easing incl. tab-switch; particle/FX listed.
- [x] Canon flags raised (EnergyChip CUT; Arena-Clash real-time→async) without altering forensic spec.
- [x] Server-auth live-ops render rules stated; no client-authored events.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.
