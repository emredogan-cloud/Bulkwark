# BULWARK — UI CONSTRUCTION SPEC · 32 · Online Battle

Source: design/OnlineBattleDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Competitive **async-ghost matchmaking** screen — a faction VS framing (You vs an opponent **REPLAY/ghost**), trophy/league standing, a season-reward trophy track, and a **FIND MATCH** CTA. Canon notes (do NOT alter forensic spec): (1) this is **ASYNC GHOST, NOT real-time PvP** — the art already says "ASYNCHRONOUS MATCHMAKING / No timers. No pressure." and tags the opponent **"REPLAY … 1h ago"**; preserve that framing everywhere. (2) The **58/120 chip** reads as energy/stamina → **canon-CUT**; document, flag, omit at implementation.

---

## A · SCREEN PURPOSE
The hub for ranked async battle. Hub chrome on top (**Back** left, currency chips right) + gold serif title "ONLINE BATTLE" with the **"ASYNCHRONOUS MATCHMAKING"** kicker and a "No timers. No pressure. Just strategy." reassurance line. A central **VS tableau** pits the player (left, **Iron Pact**, blue, league + trophies) against a fetched **opponent ghost** (right, **Ashen Horde**, red, tagged **REPLAY**, league + trophies, "Battle Replay 1h ago"), with a "Season ends in 6d 12h" timer. A **Season Rewards** panel shows a trophy-threshold reward track (5 chest milestones). A large gold **FIND MATCH** CTA dominates the bottom, flanked by utility entries: **Battle Log / Defense Log** (left) and **Leaderboard / Shop** (right). FIND MATCH fetches a new opponent ghost / enters the battle.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific, heavily faction-themed:
- **Backdrop:** dark war-hall / battlefield-at-dusk; heavy vignette; a hot **energy clash** glow in the dead center behind the VS.
- **Faction banners:** two large hanging **cloth war-banners** — left **Iron Pact** (cobalt/steel, wreath-and-shield crest), right **Ashen Horde** (oxblood-red, skull-and-spikes crest) — tattered edges, stitched gold trim, lit by rim light; armored figures faintly flank each side.
- **VS emblem:** central cast-gold ring with "VS" serif, ringed by a **blue↔red energy collision** (cobalt left, ember-red right) with sparks + bloom — the focal subject.
- **League badges:** faceted gem league insignia ("Diamond III/II") with trophy counts (🏆 + number) under each commander.
- **Tags:** small plates — left "You" (gold), right "REPLAY" (red) — making the async/ghost nature explicit.
- **Season Rewards track:** a horizontal dark sub-panel with 5 **chest milestones** pegged to ascending trophy thresholds, the final one a **gold crown trophy** (season finale).
- **FIND MATCH:** a wide brushed-gold CTA, the brightest interactive object; utility buttons are smaller gold-framed icon tiles in the corners.
- **Mood:** ranked-duel gravitas — two heraldic banners, a violent gold VS, prestige league gems; but the copy deliberately de-stresses it (async, no timers).

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
OnlineBattleScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (war-hall art, under cutout)
│  ├─ Vignette
│  ├─ CenterClashGlow (additive, behind VS)
│  └─ GodRays (subtle)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ TopBar
   │  ├─ BackButton (gold ← tile, top-left)
   │  ├─ TitleBlock
   │  │  ├─ Title "ONLINE BATTLE" (serif gold-bevel UPPER)
   │  │  ├─ Kicker "ASYNCHRONOUS MATCHMAKING"
   │  │  └─ Tagline "Compete against other Commanders. No timers. No pressure. Just strategy."
   │  └─ CurrencyChips (top-right, HLG)
   │     ├─ Chip_Gold "128,450" +
   │     ├─ Chip_Gems "2,850"  +
   │     └─ EnergyChip "58/120" +   ⚠️CANON-CUT
   ├─ VSTableau
   │  ├─ PlayerSide (left)
   │  │  ├─ FactionBanner_IronPact (cloth, blue, crest)
   │  │  ├─ YouTag "You" (gold plate)
   │  │  ├─ RoleLabel "COMMANDER"
   │  │  ├─ FactionName "Iron Pact" (shield glyph)
   │  │  ├─ LeagueBadge "Diamond III" (gem insignia)
   │  │  └─ Trophies "🏆 3,420"
   │  ├─ VSEmblem (center: gold ring "VS" + blue/red clash)
   │  ├─ SeasonTimer "Season ends in 6d 12h" (below VS)
   │  └─ OpponentSide (right)
   │     ├─ FactionBanner_AshenHorde (cloth, red, skull crest)
   │     ├─ ReplayTag "REPLAY" (red plate)
   │     ├─ OppName "Ashen Warlord"
   │     ├─ FactionName "Ashen Horde" (skull glyph)
   │     ├─ LeagueBadge "Diamond II"
   │     ├─ Trophies "🏆 3,310"
   │     └─ ReplayNote "Battle Replay · 1h ago"
   ├─ SeasonRewardsPanel
   │  ├─ Header "Season Rewards" + InfoIcon (i)
   │  ├─ Subtitle "Win battles to earn Trophies and unlock season rewards!"
   │  └─ RewardTrack (HorizontalLayoutGroup, 5 milestones)
   │     ├─ Milestone1 {chest, "🏆 1,000"}
   │     ├─ Milestone2 {chest, "🏆 1,600"}
   │     ├─ Milestone3 {chest, "🏆 2,400"}
   │     ├─ Milestone4 {chest, "🏆 3,200"}
   │     └─ Milestone5 {gold crown trophy, "🏆 3,800"}
   ├─ FindMatchButton "FIND MATCH" (wide gold CTA)
   └─ UtilityBar
      ├─ Left: BattleLogBtn ("Battle Log"), DefenseLogBtn ("Defense Log")
      └─ Right: LeaderboardBtn ("Leaderboard"), ShopBtn ("Shop")
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| OnlineBattleScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| CenterClashGlow | backdrop | 1 | Image (additive) | center | .5,.5 | — | — | scales w/ VS |
| SafeAreaRoot | screen | 1 | Rect+SafeAreaFitter | stretch-all | .5,.5 | — | insets | drives content |
| TopBar | SafeAreaRoot | 0 | Rect | top-stretch | .5,1 | — | inside safe | fixed frac |
| BackButton | TopBar | 0 | Button+Image | top-left | 0,1 | — | — | fixed |
| TitleBlock | TopBar | 1 | VerticalLayoutGroup | top-center | .5,1 | center | — | — |
| CurrencyChips | TopBar | 2 | HorizontalLayoutGroup | top-right | 1,1 | mid-right | — | right-anchored |
| VSTableau | SafeAreaRoot | 1 | Rect | center-upper | .5,.5 | — | inside safe | mirror L/R about center |
| PlayerSide | VSTableau | 0 | VerticalLayoutGroup | mid-left | 0,.5 | center | — | left third |
| FactionBanner_IronPact | PlayerSide | 0 | Image | — | .5,.5 | — | — | scales |
| YouTag/RoleLabel/FactionName/LeagueBadge/Trophies | PlayerSide | 1..5 | Image/Text | — | .5,.5 | center | — | stacked |
| VSEmblem | VSTableau | 1 | Image (ring) + Text "VS" + FX | center | .5,.5 | center | — | focal, fixed |
| SeasonTimer | VSTableau | 2 | HLG (clock+text) | center (below VS) | .5,1 | center | — | — |
| OpponentSide | VSTableau | 3 | VerticalLayoutGroup | mid-right | 1,.5 | center | — | right third (mirror) |
| FactionBanner_AshenHorde + tags/labels | OpponentSide | 0..6 | Image/Text | — | .5,.5 | center | — | stacked |
| SeasonRewardsPanel | SafeAreaRoot | 2 | Image + VerticalLayoutGroup | center-lower | .5,.5 | center | inside safe | spans inner width |
| Header+Info / Subtitle | SeasonRewardsPanel | 0,1 | HLG / Text | top | .5,1 | center | — | — |
| RewardTrack | SeasonRewardsPanel | 2 | **HorizontalLayoutGroup** (5 equal) | center | .5,.5 | mid-center | — | milestones equal; connector line behind |
| Milestone_n | RewardTrack | 0..4 | Image+VLG (chest/trophy + threshold) | — | .5,.5 | center | — | equal width |
| FindMatchButton | SafeAreaRoot | 3 | Button+Image | bottom-center | .5,0 | center | inside safe | wide, fixed frac |
| UtilityBar | SafeAreaRoot | 4 | Rect (two clusters) | bottom-stretch | .5,0 | mid spread | inside safe | corners |
| Left cluster (BattleLog, DefenseLog) | UtilityBar | 0 | HLG (icon+label tiles) | bottom-left | 0,0 | mid-left | — | — |
| Right cluster (Leaderboard, Shop) | UtilityBar | 1 | HLG (icon+label tiles) | bottom-right | 1,0 | mid-right | — | — |

**List/grid note:** RewardTrack is the canonical list → `HorizontalLayoutGroup` (5 equal milestones) with a behind-the-row **connector/progress line** (Image) showing reached vs locked thresholds. VS sides are mirrored `VerticalLayoutGroup`s. CurrencyChips/UtilityBar clusters = small HLGs. No ScrollRect needed (all fixed), but if milestone count grows, wrap RewardTrack in a horizontal ScrollRect.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar:** height ≈ 0.13·H ≈ 140 px (3 stacked title lines), top inset 0.02·H. Back tile ≈ 0.075·H sq, left inset 0.025·W. Title cap ≈ 0.058·H centered; kicker ≈ 0.026·H; tagline ≈ 0.022·H. Chips each ≈ 0.115·W × 0.05·H, right inset 0.02·W.
- **VSTableau:** band y ≈ 0.14·H → 0.58·H (height ≈ 0.44·H ≈ 475 px).
  - **Faction banners:** each width ≈ 0.16·W ≈ 374 px, height ≈ 0.40·H ≈ 432 px; PlayerSide centered ≈ 0.20·W from left, OpponentSide ≈ 0.20·W from right (mirror).
  - Under each banner, the label stack (RoleLabel/FactionName/LeagueBadge/Trophies) centered, total ≈ 0.18·H tall; LeagueBadge gem ⌀ ≈ 0.07·H; Trophy line ≈ 0.03·H.
  - YouTag / ReplayTag: small plate ≈ 0.06·W × 0.03·H, pinned above each commander label block.
  - **VSEmblem:** centered at ≈0.50·W, vertical center ≈ 0.30·H; ring ⌀ ≈ 0.16·H ≈ 173 px; clash glow halo extends ~+60% (≈ 0.26·H); "VS" cap ≈ 0.10·H.
  - **SeasonTimer:** centered ≈ 0.43·H, ≈ 0.18·W × 0.035·H.
- **SeasonRewardsPanel:** width ≈ 0.78·W ≈ 1825 px, height ≈ 0.16·H ≈ 173 px, vertical center ≈ 0.66·H. Header+info top row ≈ 0.04·H; subtitle ≈ 0.03·H; RewardTrack row ≈ 0.08·H.
  - RewardTrack: 5 milestones equal across ≈ 0.74·W; each ≈ 0.13·W wide; chest ⌀ ≈ 0.06·H, threshold text ≈ 0.022·H beneath; connector line ≈ 4 px behind, gold (reached) → grey (locked).
- **FindMatchButton:** width ≈ **0.42·W ≈ 983 px**, height ≈ **0.10·H ≈ 108 px**, centered, bottom y ≈ 0.86·H (center ≈ 0.81·H).
- **UtilityBar:** bottom y ≈ 0.90·H → 0.99·H. Each utility tile ≈ 0.05·H icon + ≈ 0.02·H label; left cluster two tiles gap ≈ 0.03·W at left inset 0.03·W; right cluster mirror at right inset 0.03·W.

**Tablet (4:3):** match-height keeps banners/VS/CTA; sides gain margin (banners may push toward edges, capped). **Ultrawide (21:9):** more backdrop; banners/utility clusters cap their outward inset so the VS stays centered & balanced; FIND MATCH width capped (~0.35·W). **Notch:** SafeAreaRoot insets all content; full-bleed war-hall + clash glow under cutout; Back/chips/utility never cross inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "ONLINE BATTLE" | serif Trajan display | Black | UPPER | +6% | 1.0 | gold bevel + bloom + 2px stroke | ~62 | #f0d27a→#caa04a; stroke #3a2c0e |
| Kicker "ASYNCHRONOUS MATCHMAKING" | condensed caps | SemiBold | UPPER | +8% | 1.0 | soft glow | ~24 | #cdbf99 |
| Tagline "No timers. No pressure…" | light serif/clean | Reg | Sentence | 0 | 1.1 | shadow | ~22 | #b3ac96 |
| Currency numbers | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |
| "You" tag | condensed caps | Bold | UPPER/Title | +2% | 1.0 | gold plate | ~22 | #2a1c06 on gold |
| "REPLAY" tag | condensed caps | Bold | UPPER | +4% | 1.0 | red plate + glow | ~22 | #ffe0d8 on #7a1f1a |
| RoleLabel "COMMANDER" | condensed caps | SemiBold | UPPER | +6% | 1.0 | shadow | ~24 | #d9c79a |
| OppName "Ashen Warlord" | serif display, fierce | Bold | Title | +2% | 1.0 | red rim glow + shadow | ~40 | #e8c9c2 (red-tinted) |
| FactionName "Iron Pact"/"Ashen Horde" | serif small-caps | SemiBold | Title | +2% | 1.0 | faction tint + glyph | ~28 | IP #acd0ff, AH #f0a89e |
| LeagueBadge "Diamond III"/"II" | condensed caps | Bold | Title | +2% | 1.0 | gem glow | ~26 | #bfe6ff |
| Trophies "3,420"/"3,310" | numeric, bold | Heavy | — | 0 | 1.0 | trophy glyph + glow | ~34 | #ffd76a |
| SeasonTimer "Season ends in 6d 12h" | numeric/caps | SemiBold | Sentence | +1% | 1.0 | clock glyph, soft glow | ~24 | #ffe08a |
| "VS" | serif display, brutal | Black | UPPER | 0 | 1.0 | gold bevel + clash glow + bloom | ~80 | #f4e6b0 |
| "Season Rewards" | serif small-caps | Bold | Title | +2% | 1.0 | shadow | ~28 | #f2e8cf |
| Reward subtitle | clean sans, quiet | Reg | Sentence | 0 | 1.1 | — | ~20 | #a9a28c |
| Milestone thresholds "1,000…3,800" | numeric small | SemiBold | — | 0 | 1.0 | trophy glyph; reached gold/locked grey | ~20 | reached #ffd76a, locked #8a8472 |
| ReplayNote "Battle Replay · 1h ago" | clean sans, quiet | Reg | Sentence | 0 | 1.0 | — | ~18 | #9a8f8a |
| "FIND MATCH" | serif display, action | Heavy | UPPER | +8% | 1.0 | dark engrave + top highlight + glow | ~46 | #2a1c06 on gold |
| Utility labels (Battle/Defense Log, Leaderboard, Shop) | condensed caps | SemiBold | Title | +2% | 1.0 | shadow | ~18 | #cdbf99 |

## G · MATERIALS
- **Backdrop:** dark war-hall/dusk #0a0b0f→#16161e, matte, heavy vignette; **CenterClashGlow** = additive blue#2b56c8 ↔ red#d8452b lobes with white-hot core + bloom; subtle god-rays.
- **Iron Pact banner:** cobalt/steel cloth #16306e→#2b56c8, tattered lower edge, stitched **gold trim**, wreath-and-shield crest in brushed steel+gold, rim-light.
- **Ashen Horde banner:** oxblood cloth #4a1410→#7a1f1a, tattered, gold/bronze trim, skull-and-spikes crest in blackened iron + ember glow, rim-light.
- **VS ring:** cast gold #6b5320→#f0d27a→#fff2c2 beveled, worn; clash energy sparks orbit.
- **League gems:** faceted crystal (diamond) #bfe6ff→#ffffff specular + inner glow + bloom.
- **Tags:** "You" = gold plate; "REPLAY" = oxblood plate #7a1f1a with red glow (signals ghost).
- **SeasonRewardsPanel:** obsidian #0c0e15→#161a24, bronze edge, matte; connector line gold(reached)→#3a3a3a(locked); chests aged-wood+bronze with bloom; final crown-trophy solid gold + bloom.
- **FindMatchButton:** brushed gold (primary CTA), beveled, inner gradient, soft outer glow, worn edges, rim-light.
- **Utility tiles:** dark slate #11141d, bronze edge, gold glyphs; subtle.
- **Bloom:** clash glow, VS ring, league gems, trophy numbers, FIND MATCH glow, chest highlights. Matte panels/banner cloth do not bloom (only their gold trim does).

## H · COMPONENTS (states)
**VSTableau / OpponentSide:** the opponent is a **fetched ghost** (REPLAY tag persistent). *idle:* banners sway gently, clash glow pulses, league gems shimmer. *re-roll (on new FIND MATCH/refresh):* opponent side cross-fades to the next ghost (name/faction/league/trophies/replay-note update) with a quick swipe. The "Battle Replay · 1h ago" is a link → opens the opponent's replay (read-only).

**FindMatchButton (primary):**
- *idle/enabled:* gold gloss, beveled, strong outer glow, slow breathe (1.6 s).
- *hover/focus:* +brightness, glow radius +.
- *pressed:* scale 0.96 + inner shadow.
- *searching:* label → "FINDING…" + spinner/animated dots; button locks; clash glow intensifies; opponent side may show "Scouting…" shimmer.
- *disabled* (e.g. mid-season lockout / requirement): grey-gold, glow off, tap → tooltip.

**LeagueBadge / Trophies:** display only; on trophy change (post-match) animate count delta (+Δ green / −Δ red) and possibly a league promote/demote flourish.

**RewardTrack milestones:**
- *reached* (player trophies ≥ threshold): chest bright + gold connector to it + ✓/claimable glow.
- *locked* (below threshold): chest dim + grey connector + faint lock; threshold grey.
- *next:* the immediate upcoming milestone subtly pulses; a progress marker (player's current trophy position) sits on the connector line.
- Info (i): tap → tooltip/sheet explaining trophy→reward rules.

**Utility tiles (Battle Log / Defense Log / Leaderboard / Shop):** idle gold-glyph; hover brighten; pressed 0.92; each routes to its sub-screen (Defense Log = attacks against you; Leaderboard → 34; Shop → 17).

**BackButton:** gold tile; hover brighten; pressed 0.92 → hub. **Chip +** → Store. EnergyChip flagged CUT.

## I · ANIMATION TIMELINE
**OnShow (~0.95 s):**
- 0.00 backdrop fade + vignette; CenterClashGlow fade-in + pulse start (0.30 s).
- 0.05 TopBar slide-down (0.20 s).
- 0.15 **banners drop/unfurl**: PlayerSide banner scaleY 0.85→1.0 + α from left, OpponentSide mirrored from right (0.30 s, ease-out), then gentle sway loop begins.
- 0.30 commander label stacks fade-up under each banner (0.18 s); league gems glint; trophy numbers count-up (0.4 s).
- 0.40 YouTag / ReplayTag snap-in (0.12 s).
- 0.45 **VSEmblem slam-in**: scale 1.3→1.0 + clash flash (blue/red burst) + bloom + small screen shake (0.25 s, ease-out-back); "VS" settles.
- 0.55 SeasonTimer fade-in; ticking.
- 0.62 SeasonRewardsPanel slide-up + α (0.22 s); RewardTrack connector draws L→R + milestones pop stagger (0.04 s apart); progress marker slides to current.
- 0.85 FindMatchButton pop 0.9→1.0 + glow on (0.18 s, ease-out-back) → breathe loop.
- 0.90 UtilityBar tiles fade-up (0.15 s).

**Idle loops:** banner sway (2–3 s sine, slight rotation+drift); clash glow pulse (1.8 s); league gem shimmer (3 s); FIND MATCH breathe (1.6 s); next-milestone pulse; embers from Horde crest.

**OnFindMatch (see K):** press 0.96 → "FINDING…" + clash intensify (0.4–1.2 s) → opponent side **swipe-swap** to new ghost (0.3 s) → either auto-advance to battle (push transition) or settle on the new matchup with FIND MATCH re-enabled (design supports "scout then commit").

**OnClose:** banners retract, VS scale→0.8 + α, panel/CTA fade (~0.30 s); pop screen.

## J · PARTICLE & FX
- **CenterClashGlow:** blue/red energy collision with crackling sparks + white-hot core + bloom; continuous slow churn, spikes on FIND MATCH.
- Banner cloth sway + dust/embers (Horde side embers, Pact side cold motes); torch-flicker rim light.
- VSEmblem: orbiting sparks + impact burst on slam-in; periodic glint.
- League gems: faceted twinkle; trophy numbers: small sparkle on count-up.
- FIND MATCH: breathing rim bloom; on press a charge-up ring + spark burst.
- RewardTrack: gold connector glow on reached segments; next-milestone pulse.
- Vignette + god-rays.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth state (read-only): player faction/league/trophies, current **opponent ghost** (name/faction/league/trophies/replay timestamp), season timer, reward-track thresholds + player progress. Render; play entry. (Strictly async — the opponent is a stored ghost/replay, never a live player.)
- **OnFindMatch:** request a new opponent ghost from matchmaking (async); swipe-swap opponent side; then commit → push into the battle/match flow vs that ghost's defense layout. Result returns to a ladder/async result screen (16) and updates trophies/league here.
- **Replay link ("Battle Replay · 1h ago"):** open read-only replay of the opponent's last battle.
- **Utility:** Battle Log (your offenses), Defense Log (attacks against you), Leaderboard (34), Shop (17).
- **Season timer:** live countdown; at season end → season-reset flow (rewards granted server-side; track resets). 
- **Trophy/league updates** are server-authoritative; client only animates deltas. **Chip +** → Store.
- **Back:** fade out → hub. EnergyChip omitted/CUT.

## L · NEGATIVE RULES
- Do **not** present or imply **real-time PvP** — it is **async ghost**; keep "ASYNCHRONOUS / No timers / REPLAY / Battle Replay · 1h ago" framing intact; no live-opponent matchmaking, no live countdown-to-match.
- Do **not** render the **EnergyChip (58/120)** as a live gate — **canon-CUT**; omit/replace at implementation.
- Do **not** compute trophies/league/rewards client-side — server-authoritative; client animates results only.
- Do **not** invent opponents, leagues, or thresholds — use exactly: You = Iron Pact / Diamond III / 3,420; Opp = Ashen Warlord / Ashen Horde / Diamond II / 3,310 (REPLAY 1h ago); thresholds 1,000 / 1,600 / 2,400 / 3,200 / 3,800 (final = crown trophy).
- Do **not** swap the faction color coding (Iron Pact = cobalt/blue, Ashen Horde = oxblood/red) — it's load-bearing.
- Do **not** drop the "No timers. No pressure." reassurance line (it communicates the async design).
- No portrait.

## M · ACCEPTANCE CRITERIA (≥95%)
1. Header: Back (left), title "ONLINE BATTLE" gold-bevel serif + "ASYNCHRONOUS MATCHMAKING" kicker + tagline; 3 chips right (128,450 / 2,850 / 58/120).
2. VS tableau: left Iron Pact banner with "You", "COMMANDER", "Iron Pact", "Diamond III", "🏆 3,420"; right Ashen Horde banner with "REPLAY", "Ashen Warlord", "Ashen Horde", "Diamond II", "🏆 3,310", "Battle Replay · 1h ago"; central glowing gold **VS** with blue/red clash; "Season ends in 6d 12h".
3. Season Rewards panel: header + (i) + subtitle + 5-milestone trophy track (chests at 1,000/1,600/2,400/3,200, crown-trophy at 3,800) with reached/locked connector + progress marker.
4. Wide gold **FIND MATCH** CTA bottom-center; utility tiles Battle Log + Defense Log (left), Leaderboard + Shop (right).
5. Layout within ±2% of fraction math at 2340×1080; VS sides mirrored; RewardTrack HLG (ScrollRect-ready); safe-area + full-bleed-under-cutout honored.
6. States correct: FIND MATCH idle/hover/pressed/searching/disabled; opponent ghost re-roll; milestone reached/locked/next; trophy-delta animation hooks.
7. Faction palettes + clash glow + VS/gem/trophy/CTA bloom present; matte cloth/panels not blooming.
8. Async-ghost framing preserved; energy chip flagged/omitted; server-auth rules documented.

## N · IMPLEMENTATION CONFIDENCE
**89/100.** High: clear symmetric VS comp, exact labels/numbers, standard CTA/track/utility patterns, strong canon alignment (async framing already in art). Deductions: (−4) faction banners + crests + VS clash FX need bespoke assets/shaders for 1:1 (cloth, bloom, energy collision, slam-in); (−3) FIND MATCH flow (scout-then-commit vs auto-enter) and opponent re-roll choreography are inferred from a static frame; (−2) league-gem geometry + reward-track progress-marker behavior are asset/logic dependent; (−2) replay-link + Defense Log sub-flows are referenced but not shown.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (both commanders, leagues, trophies, 5 thresholds, timers, tags, utilities); nothing invented.
- [x] RewardTrack typed as HLG (ScrollRect-ready) + connector; VS sides mirrored VLGs; chip/utility rows HLG.
- [x] Component states (FIND MATCH incl. searching/disabled, opponent re-roll, milestones, trophy deltas, utilities, Back) enumerated.
- [x] Animation timeline with timestamps/easing incl. VS slam + opponent swap; particle/FX listed.
- [x] Canon flags raised (ASYNC GHOST framing preserved; EnergyChip CUT) without altering forensic spec.
- [x] Server-auth (trophies/league/rewards/matchmaking) rules stated; strictly async; no client mutation.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.
