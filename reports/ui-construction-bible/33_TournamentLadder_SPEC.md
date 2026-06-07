# BULWARK — UI CONSTRUCTION SPEC · 33 · Tournament Ladder

Source: design/TournamentLadderDesign.png · 1672×941 (1.78:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Competitive **async tournament bracket** — a single-elimination tree (8v8 → CHAMPION) with the player's path highlighted, a champion prize, header timer + utility icons (Rewards/Rules/Leaderboard), and bottom tabs (Qualifiers / Tournament / Battle Log). Canon note (do NOT alter forensic spec): like Online Battle this is **ASYNC GHOST** progression (no real-time PvP); matches resolve vs stored opponent layouts. No energy chip is visible here (good); currency chips are gems + gold only.

---

## A · SCREEN PURPOSE
The bracket view of a seasonal "Championship Tournament". Hub chrome: **Back** (top-left), title + **"Ends in: 2d 18h"** timer, currency chips (top-right), and a utility icon cluster (**Rewards / Rules / Leaderboard**). The center is a **symmetric single-elimination bracket**: 8 competitors down the left in 4 round-1 matchups, narrowing through quarter/semi to the **center FINAL**, mirrored by 8 on the right; the apex is a **CHAMPION** laurel crest (with a "?" placeholder shield) and a **🏆 10,000** prize. **"YOU"** appears on the left (★-marked) and the player's advancement path is rendered in bright gold ("You" labels mark won connectors). A **bottom tab bar** switches Qualifiers / **Tournament** (active) / Battle Log. Tapping the player's next opponent node opens/commits that async match.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** a grand **throne hall** — stone arches, a distant throne, **flaming braziers** lining the steps, warm god-rays from above, heavy vignette; deep amber/gold ambience (more golden/warm than Online Battle's cold clash).
- **Bracket lines:** glowing **gold connector lines** forming the tree; the player's winning path is **brighter/hotter gold** vs the dimmer bronze of undecided/other paths.
- **Competitor nodes:** small dark **portrait tiles** (helmeted hero busts) with a name plate; faction-tinted frames (blue/red/violet hints); the **YOU** node carries a gold star + gold frame + glow.
- **Champion crest:** large **gold laurel wreath** at top-center enclosing a shield with a **"?"** (champion undecided), labeled **CHAMPION** with a trophy + **10,000** prize; strong bloom — the focal apex.
- **Header:** gold serif title "CHAMPIONSHIP TOURNAMENT", a clock "Ends in: 2d 18h", and three gold-framed utility icon tiles.
- **Tab bar:** dark bottom strip; active **Tournament** gold-lit with a crest/underline; Qualifiers + Battle Log muted.
- **Mood:** prestige arena finale — golden throne hall, glowing bracket, laurel champion; the player's gold path is the eye-magnet.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
TournamentLadderScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (throne-hall art, under cutout)
│  ├─ Vignette
│  ├─ GodRays (top, warm)
│  └─ BrazierGlows (additive, along steps)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ TopBar
   │  ├─ BackButton (gold ← tile, top-left)
   │  ├─ TitleBlock {Title "CHAMPIONSHIP TOURNAMENT", Timer "Ends in: 2d 18h"}
   │  ├─ CurrencyChips (HLG) {Chip_Gems "2,340" +, Chip_Gold "58,420" +}
   │  └─ UtilityIcons (HLG) {RewardsBtn(chest "Rewards"), RulesBtn(book "Rules"), LeaderboardBtn(trophy "Leaderboard")}
   ├─ ChampionCrest (top-center apex)
   │  ├─ LaurelWreath (gold)
   │  ├─ ChampShield "?" (undecided)
   │  ├─ Label "CHAMPION"
   │  └─ Prize {trophy, "10,000"}
   ├─ Bracket (center; symmetric tree)
   │  ├─ LeftHalf
   │  │  ├─ Round1_L (4 matchups, 8 competitors)
   │  │  │  ├─ M1 {C:"Frostblade",  C:"Ironclaw"}
   │  │  │  ├─ M2 {C:"Voidwalker",  C:"Shadowbane"}
   │  │  │  ├─ M3 {C:"YOU"★ (gold), C:"Stormrider"}     ← player matchup
   │  │  │  └─ M4 {C:"Deathbringer",C:"Ravenheart"}
   │  │  ├─ Round2_L (2 nodes; M3-winner shows "You")
   │  │  ├─ Round3_L (1 node; semifinal; "You")
   │  │  └─ Connectors_L (gold lines; player path BRIGHT)
   │  ├─ FinalCenter (center node feeding ChampionCrest; current champion-bracket winner portrait)
   │  └─ RightHalf (mirror)
   │     ├─ Round1_R (4 matchups, 8 competitors)
   │     │  ├─ M5 {C:"Dragonslayer", C:"Bloodrage"}
   │     │  ├─ M6 {C:"Nightreaper",  C:"Firelord"}
   │     │  ├─ M7 {C:"Goldenblade",  C:"Dreadlord"}
   │     │  └─ M8 {C:"Thunderfist",  C:"Soulhunter"}
   │     ├─ Round2_R (2 nodes)
   │     ├─ Round3_R (1 node; semifinal)
   │     └─ Connectors_R (bronze lines)
   └─ TabBar (bottom, 3 tabs)
      ├─ Tab_Qualifiers (muted)
      ├─ Tab_Tournament (ACTIVE — crest glyph, gold, underline)
      └─ Tab_BattleLog  (muted)
```
*(Each `C:` = a CompetitorNode {portrait, name plate, win/lose/pending/you state}.)*

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| TournamentLadderScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| BrazierGlows | backdrop | 2 | Image(s) additive | varies | .5,.5 | — | — | scale w/ height |
| SafeAreaRoot | screen | 1 | Rect+SafeAreaFitter | stretch-all | .5,.5 | — | insets | drives content |
| TopBar | SafeAreaRoot | 0 | Rect | top-stretch | .5,1 | — | inside safe | fixed frac |
| BackButton | TopBar | 0 | Button+Image | top-left | 0,1 | — | — | fixed |
| TitleBlock | TopBar | 1 | VerticalLayoutGroup | top-left (after back) | 0,1 | left | — | — |
| CurrencyChips | TopBar | 2 | HorizontalLayoutGroup | top-right | 1,1 | mid-right | — | right-anchored |
| UtilityIcons | TopBar | 3 | HorizontalLayoutGroup | top-right (below/left of chips) | 1,1 | mid-right | — | right-anchored |
| ChampionCrest | SafeAreaRoot | 1 | Image (wreath) + children | top-center | .5,1 | center | inside safe | apex, fixed |
| ChampShield/Label/Prize | ChampionCrest | 0..2 | Image/Text | center/below | .5,.5 | center | — | — |
| **Bracket** | SafeAreaRoot | 2 | Rect (custom layout) **OR** ScrollRect (if overflow) | center | .5,.5 | — | inside safe | symmetric; fit-to-width or pan/zoom |
| LeftHalf / RightHalf | Bracket | 0,2 | Rect | mid-left / mid-right | 0,.5 / 1,.5 | — | — | mirrored |
| Round1_L (matchups) | LeftHalf | 0 | VerticalLayoutGroup (4 matchups) | left | 0,.5 | center | — | even vertical spread |
| Matchup (M1..M8) | RoundN | 0..3 | VerticalLayoutGroup (2 CompetitorNodes) | — | .5,.5 | center | — | pair stacked |
| CompetitorNode | Matchup | 0,1 | Image(frame)+Image(portrait)+Text(name)+StateIcon | — | .5,.5 | center | — | fixed tile |
| Round2_L / Round3_L | LeftHalf | 1,2 | VerticalLayoutGroup | left (stepped right) | 0,.5 | center | — | nodes centered between feeders |
| Connectors_L/_R | Half | 3 | Image/UILineRenderer set | stretch | .5,.5 | — | — | drawn between node anchors |
| FinalCenter | Bracket | 1 | VerticalLayoutGroup | center | .5,.5 | center | — | feeds crest |
| RightHalf rounds (mirror) | RightHalf | 0..3 | (mirror of left) | right | 1,.5 | center | — | mirrored |
| TabBar | SafeAreaRoot | 3 | Image + HorizontalLayoutGroup (3 equal) | bottom-stretch | .5,0 | mid-center | inside safe | spans width |
| Tab_x | TabBar | 0..2 | Toggle/Button+Image+Text | — | .5,.5 | center | — | equal thirds |

**List/tree note:** the **Bracket** is the canonical complex structure. Two viable builds:
- **(A) Fixed-anchored layout (preferred for 1:1):** absolutely position each CompetitorNode at computed bracket coordinates; draw **Connectors** as `Image` elbow segments (or a `UILineRenderer`) between parent/child node anchors; the player path uses a brighter material. Round columns step inward toward `FinalCenter`/`ChampionCrest`.
- **(B) ScrollRect (if it can't fit):** wrap Bracket in a horizontal+vertical `ScrollRect` with pinch-zoom for small screens; header/crest/tabs stay pinned outside the viewport.
Round columns are `VerticalLayoutGroup`s with even spacing; matchups are 2-node `VerticalLayoutGroup`s. TabBar = toggle group. Chips/utility = HLGs.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar:** height ≈ 0.12·H ≈ 130 px, top inset 0.02·H. Back tile ≈ 0.075·H sq, left inset 0.025·W. Title cap ≈ 0.055·H; timer ≈ 0.026·H below. Chips each ≈ 0.10·W × 0.05·H (right inset 0.02·W). UtilityIcons: 3 tiles ⌀ ≈ 0.06·H + label, gap 0.012·W, right-aligned beneath/left of chips.
- **ChampionCrest:** wreath ⌀ ≈ 0.22·H ≈ 238 px, centered horizontally (0.50·W), top y ≈ 0.13·H (just below TopBar); ChampShield ⌀ ≈ 0.10·H inside; Label "CHAMPION" ≈ 0.03·H below wreath; Prize trophy+number ≈ 0.035·H beneath. Bloom halo ~+40%.
- **Bracket band:** y ≈ 0.20·H → 0.88·H (height ≈ 0.68·H ≈ 734 px), full inner width ≈ 0.96·W ≈ 2246 px.
  - **5 columns per side path** collapse toward center: each half has Round1 (4 matchups = 8 nodes), Round2 (2 nodes), Round3 (1 node), then FinalCenter (shared), then ChampionCrest apex.
  - **CompetitorNode tile:** width ≈ 0.10·W ≈ 234 px, height ≈ 0.055·H ≈ 60 px (portrait ⌀ ≈ 0.05·H at left of tile + name plate). 8 nodes stack on each side over the 0.68·H band → per-node slot ≈ 0.085·H; intra-matchup pair gap ≈ 0.01·H, inter-matchup gap ≈ 0.05·H.
  - **Column X (left half):** Round1 nodes at left inset ≈ 0.02·W; Round2 column at ≈ 0.16·W; Round3 (semi) at ≈ 0.28·W; FinalCenter near ≈ 0.40·W→0.50·W. Right half mirrors from the right edge.
  - **Connectors:** horizontal stub from each node (≈ 0.03·W) → vertical join between the two feeders → horizontal into the next-round node; line thickness ≈ 3–4 px; **player path** ≈ 5 px + glow.
  - **YOU node (M3):** gold frame + star badge ⌀ ≈ 0.025·H, +glow; "You" connector labels (small plates ≈ 0.04·W × 0.022·H) sit on the won segments toward Round2/Round3.
- **TabBar:** height ≈ 0.085·H ≈ 92 px, bottom inset ≈ 0.01·H; 3 equal tabs; active Tournament gold crest glyph + label + underline.

**Tablet (4:3):** match-height keeps node sizes; the wide bracket gains side margin — keep symmetric; if it would clip, enable ScrollRect-pan. **Ultrawide (21:9):** more horizontal room → columns can spread for clarity (cap so center stays centered); backdrop fills sides. **Notch:** SafeAreaRoot insets everything; full-bleed throne hall + brazier glows under cutout; Back/chips/utility/tabs never cross inset. **Small screens:** prefer ScrollRect+pinch-zoom on Bracket so node text stays legible.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "CHAMPIONSHIP TOURNAMENT" | serif Trajan display | Black | UPPER | +5% | 1.0 | gold bevel + bloom + 2px stroke | ~52 | #f0d27a→#caa04a; stroke #3a2c0e |
| Timer "Ends in: 2d 18h" | numeric/caps | SemiBold | Sentence | +1% | 1.0 | clock glyph, soft glow | ~24 | #ffe08a |
| Currency numbers | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |
| Utility labels (Rewards/Rules/Leaderboard) | condensed caps small | SemiBold | Title | +2% | 1.0 | shadow | ~16 | #cdbf99 |
| "CHAMPION" | serif display, regal | Black | UPPER | +8% | 1.0 | gold bevel + strong bloom + stroke | ~34 | #f4e6b0; stroke #2a1c06 |
| Champion "?" | serif display | Black | — | 0 | 1.0 | gold, glow | ~64 | #f0d27a |
| Champion prize "10,000" | numeric bold | Heavy | — | 0 | 1.0 | trophy glyph + glow | ~30 | #ffd76a |
| Competitor names | condensed caps, terse | SemiBold | Title | +1% | 1.0 | dark stroke; faction tint | ~22 | #e8e2cf (faction-tinted) |
| "YOU" | condensed caps, bold | Bold | UPPER | +3% | 1.0 | gold + glow + star | ~24 | #ffd76a |
| "You" path labels | condensed caps small | Bold | Title | +2% | 1.0 | gold plate | ~16 | #2a1c06 on gold |
| Tab labels (Qualifiers/Tournament/Battle Log) | condensed caps | SemiBold (active Bold) | UPPER | +4% | 1.0 | active gold + underline; inactive muted | ~22 | active #f0d27a, inactive #7d7768 |

## G · MATERIALS
- **Backdrop:** throne hall stone #14120e→#221c12 warm, matte; **brazier glows** additive ember #e0742a→#ffb04a with flicker + bloom; warm god-rays from top; heavy vignette.
- **ChampionCrest:** brushed gold laurel #6b5320→#f0d27a→#fff2c2, high relief, **strong bloom**; ChampShield dark steel #1b2030 with gold "?"; prize trophy solid gold + bloom.
- **Bracket connectors:** undecided/other = antique bronze #8a6a30 (dim, ~40% glow); **player path = hot gold** #ffd76a with additive glow + slight animated flow.
- **CompetitorNode:** dark slate tile #11141d→#1b2030, thin metal frame (faction-tinted: IP cobalt #2b56c8, AH oxblood #7a1f1a, neutral/violet #5a2db0 hints); portrait = helmeted bust, low-key lit; **YOU** tile = gold frame #caa04a→#f0d27a + glow + star.
- **State overlays:** *winner* = node brightened + small gold ✓/laurel tick; *loser* = node desaturated ~50% + cracked/grey overlay + dim; *pending* = node normal with a faint pulse; *next-for-you* = cobalt CTA-glow ring (this is the playable match).
- **Utility tiles / TabBar:** dark slate #11141d, bronze edge, gold glyphs; active tab gold crest + underline + glow.
- **Bloom:** champion crest/prize, player path, brazier glows, YOU node, active tab, god-rays. Matte stone/slate do not bloom.

## H · COMPONENTS (states)
**CompetitorNode** (per-slot states):
- *pending* (match not yet resolved): both competitors normal, faint pulse; if it's **your** next match, the node pair gets a **cobalt "play" glow ring** + is tappable.
- *winner:* brightened + gold tick/laurel; advances (its name appears in the next round node).
- *loser:* desaturated ~50% + grey/cracked overlay; path beyond it goes dim.
- *you:* always gold-framed + star; your won nodes carry "You" on the outgoing connector.
- *champion (apex):* "?" until decided → resolves to the winner's portrait + name with a coronation flourish.
- *hover/focus:* node lift + frame brighten (tooltip: record/league/replay).
- *pressed (playable only):* scale 0.97 → opens match-commit/confirm.

**Connectors:** static lines; player path animates a subtle gold energy flow; on a new win, the next segment "charges" from the won node to the advanced node (gold sweep).

**ChampionCrest:** idle laurel shimmer + glow pulse; if undecided shows "?"; on tournament finish plays coronation (wreath flare + winner reveal + confetti).

**Utility icons:**
- *RewardsBtn:* opens reward-tier sheet (placements → prizes). *RulesBtn:* opens rules sheet. *LeaderboardBtn:* opens tournament standings (34-style). idle gold glyph; hover brighten; pressed 0.92.

**TabBar (toggle group):** active **Tournament** (gold crest + underline + glow); Qualifiers + Battle Log muted; selecting swaps the body (qualifier seeding view / battle history) while header+tabs persist.

**BackButton:** gold tile; hover brighten; pressed 0.92 → hub/competitive root. **Chip +** → Store.

## I · ANIMATION TIMELINE
**OnShow (~1.0 s):**
- 0.00 backdrop fade + god-rays + brazier flicker start (0.25 s).
- 0.05 TopBar slide-down (0.20 s); chips count-up optional.
- 0.15 **ChampionCrest** drop-in: y −24→0 + scale 0.9→1.0 + α (0.30 s, ease-out-back); wreath glint sweep; prize sparkle.
- 0.30 **Bracket reveal**: connectors draw outward **from center → edges** (or edges → center) over ~0.4 s; CompetitorNodes pop-in stagger by round (Round1 first, then inward), 0.03 s apart, 0.15 s each.
- 0.55 **player path highlight**: the gold path lights up sequentially from YOU node → Round2 → Round3 (gold sweep, 0.4 s) with "You" labels snapping on; YOU node glow + star pop.
- 0.80 next-playable match cobalt ring fades up + pulse begins.
- 0.90 TabBar fade-up; active-tab underline draw-in (0.18 s).

**Idle loops:** champion crest shimmer + glow pulse (2 s); brazier flicker (0.5 s random); player-path gold flow (slow); next-match cobalt ring pulse (1.4 s); active-tab glow breathe; god-ray drift; dust motes.

**OnPlayMatch (see K):** tap your next match node → node scale 0.97 + cobalt flash → confirm/commit → push into battle vs that ghost. On return with a **win**: the won node updates (winner tick), the next connector **charges** (gold sweep, 0.4 s), your advanced node appears + glows, bracket re-renders state; on a **loss**: your path dims, elimination overlay.

**OnTabSwitch:** underline retract + body cross-fade (0.15 s out / 0.20 s in); header/tabs persist.

**OnTournamentResolve:** champion "?" → winner reveal: wreath flare + portrait fade-in + coronation confetti + prize highlight (~1.0 s).

**OnClose:** crest + bracket + tabs fade/contract (~0.30 s); pop screen.

## J · PARTICLE & FX
- **Brazier flames** along the hall (flickering ember particles + light + bloom); warm god-rays; floating ash motes.
- **ChampionCrest:** laurel glint + glow pulse + occasional sparkle; coronation confetti on resolve.
- **Player path:** gold energy flow along connectors + glow; charge-sweep on new wins.
- **YOU node:** star twinkle + frame glow; next-match node cobalt pulse ring.
- **Node state FX:** winner gold-tick sparkle; loser dust/crack puff.
- Vignette; subtle ambient embers. Keep node area legible — concentrate spectacle on crest + braziers + player path.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth bracket state (read-only): seeding, all competitor names/portraits/factions, per-match results (win/lose/pending), the player's position + path, champion status, prize, end timer, tab availability. Render via fixed-anchor layout (or ScrollRect); play entry. (Async — opponents are stored ghosts; the player never battles a live human.)
- **OnPlayMatch:** only the player's **next pending** match node is tappable → confirm → push into battle vs that opponent's ghost/defense; result returns to a ladder/async result (16) and updates the bracket here (advance/eliminate).
- **OnTabSwitch:** Qualifiers ↔ Tournament ↔ Battle Log — swap body (seeding/qualifier view, this bracket, match history) without leaving; persist header+tabs.
- **Utility:** Rewards (placement→prize tiers), Rules (format/odds/schedule), Leaderboard (standings). **Chip +** → Store.
- **Timer:** live countdown to tournament end; on end → resolve/coronation + reward grant (server-side). **Back:** → hub/competitive root.
- **Async/server-auth:** all results, advancement, seeding, and the champion are server-authoritative; client only renders + animates state changes; no client-side bracket mutation.

## L · NEGATIVE RULES
- Do **not** imply **real-time PvP** — tournament matches are **async ghost**; no live brackets/countdowns-to-live-match; results resolve vs stored layouts.
- Do **not** mutate the bracket, results, seeding, or champion client-side — server-authoritative; render only.
- Do **not** make non-player or already-decided nodes "playable"; only the player's next pending match is tappable.
- Do **not** invent competitors/seeds/prize — use exactly the 16 names listed, the champion **"?"** placeholder, and prize **10,000**; YOU is the ★ left-side competitor in matchup M3.
- Do **not** lose the gold-path emphasis (player's advancement) or the laurel CHAMPION apex — they're the core read.
- Do **not** drop the end-timer / utility (Rewards/Rules/Leaderboard) — they frame the competitive context.
- No portrait; keep node text legible (ScrollRect+zoom on small screens rather than shrinking past readability).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Header: Back (left), title "CHAMPIONSHIP TOURNAMENT" gold-bevel serif + "Ends in: 2d 18h"; chips (2,340 / 58,420) + utility icons (Rewards/Rules/Leaderboard) right.
2. ChampionCrest apex: gold laurel wreath + "?" shield + "CHAMPION" + 🏆 10,000.
3. Symmetric single-elimination bracket: left 8 (Frostblade, Ironclaw, Voidwalker, Shadowbane, YOU★, Stormrider, Deathbringer, Ravenheart) in 4 R1 matchups → narrowing; right 8 (Dragonslayer, Bloodrage, Nightreaper, Firelord, Goldenblade, Dreadlord, Thunderfist, Soulhunter) mirrored; center FINAL.
4. Player path highlighted in bright gold with "You" labels on won connectors; YOU node gold-framed + star; node win/lose/pending states distinct.
5. Bottom tabs: Qualifiers / Tournament(active, gold/underline) / Battle Log.
6. Layout within ±2% of fraction math at 2340×1080; bracket symmetric; connectors correctly join feeders→next round; safe-area + full-bleed-under-cutout honored; ScrollRect/zoom fallback on small screens.
7. States correct: node pending/winner/loser/you/next-playable; champion "?"→reveal; tab body swap; timer tick.
8. Throne-hall palette + brazier/crest/path bloom present; matte stone/slate not blooming; async-ghost framing preserved.

## N · IMPLEMENTATION CONFIDENCE
**85/100.** High: exact roster/labels/prize, clear symmetric format, strong canon fit (async), standard tab/utility patterns. Deductions: (−6) the **bracket** is the hardest UI here — precise node coordinates, elbow connector geometry, player-path highlighting, and a responsive fit (fixed-anchor vs ScrollRect+zoom) require careful custom layout to hit 1:1; (−4) 16 hero portraits + laurel/crest + faction-tinted frames are bespoke assets; (−3) match-commit/advance choreography, coronation, and Qualifiers/Battle-Log tab bodies are inferred (not all shown); (−2) connector "charge" + path-flow shaders are approximated.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch + small-screen zoom covered.
- [x] Every visible label/number recorded (all 16 competitors, YOU, champion "?", 10,000 prize, timer, utilities, tabs); nothing invented.
- [x] Bracket typed with two viable builds (fixed-anchor preferred / ScrollRect fallback) + connector strategy; columns VLG; tabs toggle group; chip/utility HLG.
- [x] Component states (node pending/winner/loser/you/next-playable, champion reveal, utilities, tabs, Back) enumerated.
- [x] Animation timeline with timestamps/easing incl. bracket reveal, path highlight, advance/coronation; particle/FX listed.
- [x] Canon framing preserved (ASYNC GHOST; no energy chip present) without altering forensic spec.
- [x] Server-auth (bracket/results/seeding/champion) rules stated; only next-pending match playable; no client mutation.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.
