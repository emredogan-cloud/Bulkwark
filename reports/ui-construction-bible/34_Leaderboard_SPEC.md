# BULWARK — UI CONSTRUCTION SPEC · 34 · Leaderboard
Source: design/LeaderboardScreenDesign.png · 1782×883 (≈2.02:1) · Analysis-only forensic spec.

> Normalize to the 2340×1080 (≈19.5:9) landscape production canvas. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). All layout below is FRACTION-BASED so it scales; px values are quoted at the 1080-tall production height. Full-bleed background extends under the cutout; all interactive content sits inside `Screen.safeArea`.

---

## A. SCREEN PURPOSE
Competitive **global/social ranking** board. Lets the player (a) browse the top of the ranked ladder, (b) switch ranking scope via tabs **GLOBAL / FRIENDS / SEASON**, (c) see the seasonal countdown, (d) read their own pinned rank/score regardless of scroll, and (e) inspect each ranked player's league tier, guild, and score. It is a **read-only meta screen** (server-authoritative data; client never mutates rank). The only mutating-adjacent affordance is **League Rewards** (opens a rewards detail — claim is server-validated). Reached from the Main-Menu rail. No gameplay/ECS interaction.

## B. VISUAL DNA (screen-specific, inherits GLOBAL DNA)
- Dark heroic high-fantasy; **near-black charcoal field** (#0a0b0f→#14161e) over a faint dusk **castle-ruins skyline** background, heavily vignetted so the board reads as the focal subject.
- **Brushed gold / antique bronze ornate frame** around the whole board panel; **serif gold-bevel UPPERCASE title** "LEADERBOARD" centered top with **crossed-sword corner ornaments** flanking it.
- Top-3 ranks get **prestige medallion badges** (gold #f0d27a, silver #cdd3da, bronze #c8884a) — circular cast-metal disks with the rank numeral; ranks 4+ are plain gold serif numerals.
- **Royal/cobalt blue** = the selected tab + the "League Rewards" CTA + the **my-rank pinned row** highlight. **Violet/amethyst** = the LEGENDARY league badge crystal. Champion/Diamond league badges are bronze/blue shield glyphs.
- Alternating row striping is extremely subtle (near-black on near-black). Focal glow sits on the top-1 medallion. Gold rim-light on the outer frame; soft inner drop-shadow on the list area.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
LeaderboardScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ BackgroundLayer (full-bleed, under cutout)
   │  ├─ BG_Skyline (Image — dusk castle ruins)
   │  └─ BG_Vignette (Image — radial dark overlay)
   ├─ BoardFrame (Image — ornate gold/bronze 9-slice frame; the main panel)
   │  ├─ TopBar
   │  │  ├─ BackButton (gold square + left chevron)
   │  │  ├─ TitleOrnamentLeft (crossed-swords flourish)
   │  │  ├─ Title_LEADERBOARD (Text, serif gold)
   │  │  └─ TitleOrnamentRight (crossed-swords flourish)
   │  ├─ TabRow
   │  │  ├─ Tab_GLOBAL (selected)
   │  │  ├─ Tab_FRIENDS
   │  │  ├─ Tab_SEASON
   │  │  └─ SeasonTimerCluster
   │  │     ├─ Lbl_SeasonEndsIn ("Season Ends In")
   │  │     ├─ Icon_Hourglass
   │  │     └─ Val_Countdown ("13d 14h 22m")
   │  ├─ LeftRail (sub-panel)
   │  │  ├─ LeagueEmblem (large golden trophy/crest)
   │  │  ├─ Lbl_LeagueName ("LEGENDARY LEAGUE")
   │  │  ├─ Lbl_LeagueDesc ("Compete with warriors from around the world and climb the ranks!")
   │  │  ├─ Btn_LeagueRewards (blue)
   │  │  ├─ Block_MyRank
   │  │  │  ├─ Lbl_MyRankCaption ("My Rank")
   │  │  │  └─ Val_MyRank ("128")
   │  │  └─ Block_MyScore
   │  │     ├─ Lbl_MyScoreCaption ("My Score")
   │  │     └─ Val_MyScore ("2,345,678")
   │  ├─ ListArea
   │  │  ├─ ColumnHeaderRow
   │  │  │  ├─ Col_RANK
   │  │  │  ├─ Col_PLAYER
   │  │  │  ├─ Col_LEAGUE
   │  │  │  └─ Col_SCORE
   │  │  ├─ ScrollView (Viewport + Content, vertical)
   │  │  │  └─ Content (VerticalLayoutGroup)
   │  │  │     ├─ Row_1 (medallion variant — gold)
   │  │  │     │  ├─ RankBadge_Medallion (numeral 1)
   │  │  │     │  ├─ Avatar (circular portrait + ring)
   │  │  │     │  ├─ NameBlock (Name "BloodReaver" + Guild "Death's Vanguard")
   │  │  │     │  ├─ LeagueBadge ("LEGENDARY" violet)
   │  │  │     │  └─ ScoreCell (Icon_Trophy + "5,824,910")
   │  │  │     ├─ Row_2 (silver medallion — "Shadowblade"/"Nightfall Order"/5,231,780)
   │  │  │     ├─ Row_3 (bronze medallion — "Grimlord"/"Iron Dominion"/4,789,450)
   │  │  │     ├─ Row_4 (plain — "Frostborne"/"Northern Pact"/CHAMPION/4,125,670)
   │  │  │     ├─ Row_5 (plain — "Stormbringer"/"Wardens"/CHAMPION/3,876,540)
   │  │  │     └─ Row_6 (plain — "Ravenstrike"/"Silent Talons"/CHAMPION/3,542,180)
   │  │  └─ MyRankPinnedRow (sticky, blue-outlined — "128 ValiantOne"/"Silver Wardens"/DIAMOND II/2,345,678)
   │  └─ FooterNote (Text — "Leaderboard updates every 15 minutes.")
```

## D. UNITY HIERARCHY SPEC (per node)
- **LeaderboardScreen** — parent: UiRouter canvas. Type: empty `RectTransform` + `CanvasGroup` (fade in/out). Anchor: stretch-all (0,0)-(1,1). Pivot 0.5,0.5. Pushed onto the screen-stack; replaces previous screen (NOT a modal).
- **SafeAreaRoot** — parent: LeaderboardScreen. Anchor: stretch-all. `SafeAreaFitter` insets to `Screen.safeArea`. Child order: Background then BoardFrame.
- **BackgroundLayer / BG_Skyline / BG_Vignette** — parent: LeaderboardScreen (NOT SafeAreaRoot, so it bleeds under the notch). Anchor stretch-all, pivot 0.5. `Image`, `raycastTarget=false`. Vignette on top of skyline (later sibling).
- **BoardFrame** — parent: SafeAreaRoot. Anchor stretch-all with inset margins (see E). Pivot 0.5. `Image` 9-slice gold frame; `raycastTarget=true` (background click does nothing but blocks). Child order: TopBar, TabRow, LeftRail, ListArea, FooterNote.
- **BackButton** — parent: TopBar. Anchor top-left (0,1) pivot 0,1. `Button` + child `Image` (chevron). Min touch 88×88 px.
- **Title_LEADERBOARD** — parent: TopBar. Anchor top-center (0.5,1) pivot 0.5,1. `Text`, alignment center. Ornaments anchored left/right of the title's measured width.
- **TabRow** — parent: BoardFrame. Anchor top-stretch (0,1)-(1,1) pivot 0.5,1, below TopBar. Horizontal `LayoutGroup` for the 3 tabs (left-of-center); SeasonTimerCluster anchored top-right (1,1) pivot 1,1, vertically centered to the tab row.
- **Tab_GLOBAL/FRIENDS/SEASON** — `Button` (`Toggle` in a `ToggleGroup` is preferred for exclusive selection). Anchor within TabRow's horizontal group, pivot 0.5,0.5. Selected = filled blue capsule; others = ghost.
- **SeasonTimerCluster** — horizontal layout: caption (small), hourglass icon, countdown value. Right-aligned.
- **LeftRail** — parent: BoardFrame. Anchor left-stretch (0,0)-(0,1) pivot 0,0.5, below TabRow. Fixed fractional width (see E). `VerticalLayoutGroup` center-aligned, `LayoutElement` spacing.
- **LeagueEmblem** — top of LeftRail, anchor top-center, pivot 0.5,1. `Image`, `preserveAspect=true`.
- **Btn_LeagueRewards** — `Button` blue capsule, centered in rail. Min height 64 px.
- **Block_MyRank / Block_MyScore** — vertical caption+value pairs, centered, near rail bottom.
- **ListArea** — parent: BoardFrame. Anchor: fills the region right of LeftRail and below TabRow (anchors min (railWidthFrac, 0) max (1, tabRowBottomFrac)). Pivot 0.5. Child order: ColumnHeaderRow (top, fixed height), ScrollView (fills middle), MyRankPinnedRow (bottom, fixed height, OUTSIDE the scroll content), FooterNote sits below in BoardFrame.
- **ColumnHeaderRow** — anchor top-stretch within ListArea, pivot 0.5,1, fixed height ~42 px. Four `Text` cells aligned to the row column x-fractions (see E).
- **ScrollView** — `ScrollRect` vertical-only, `movementType=Elastic`, scrollbar hidden/auto. Viewport `Mask`(RectMask2D). Content has `VerticalLayoutGroup` (spacing ~10 px) + `ContentSizeFitter` (vertical preferred).
- **Row_N** — `RectTransform` + `Button` (tappable → profile peek, optional). Fixed height (see E). Internal layout uses the SAME column x-fractions as the header. `Image` row background (very subtle stripe; medallion rows have a faint warm tint).
- **RankBadge_Medallion** vs **plain rank** — variant by data: ranks 1-3 = `Image` medallion (gold/silver/bronze) with overlaid numeral `Text`; ranks 4+ = `Text` numeral only, gold, right/center-aligned in the RANK column.
- **Avatar** — `Image` circular (alpha-mask or rounded sprite) + ring `Image` overlay. Medallion rows get a slightly larger ring.
- **NameBlock** — `VerticalLayoutGroup`: Name (`Text`, bold gold/cream) over Guild subtitle (`Text`, small grey).
- **LeagueBadge** — horizontal: small league-tier icon `Image` + tier `Text` (LEGENDARY violet, CHAMPION bronze, DIAMOND blue). Center-aligned in LEAGUE column.
- **ScoreCell** — horizontal: score `Text` (gold, tabular) + trophy `Icon` `Image`, right-aligned in SCORE column.
- **MyRankPinnedRow** — same internal layout as Row_N but with a **blue outline `Image`/Outline** and slightly brighter fill; pinned to ListArea bottom, never scrolls. Rank "128", name "ValiantOne", guild "Silver Wardens", league "DIAMOND II" (blue), score "2,345,678".
- **FooterNote** — parent BoardFrame, anchor bottom-center (0.5,0) pivot 0.5,0. Small italic-ish grey `Text`, centered.
- **Responsive:** match-height keeps row heights stable; on wider screens (21:9/ultrawide) BoardFrame's stretch margins reveal more background at the sides — clamp BoardFrame to a max width (~92% of 2340) and re-center. On notch devices the SafeAreaFitter pulls the frame in; BG stays full-bleed.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **BoardFrame:** inset ~3.4% L/R and ~3% top, ~4% bottom → x≈[0.034, 0.966], y≈[0.04, 0.97]; ≈2170×1004 px. Corner radius/frame thickness ≈ 28 px 9-slice border.
- **TopBar height:** ≈ 0.11 of frame (~110 px). BackButton ≈ 92×92 px, left-inset ~1.5% of canvas. Title centered, ornaments ±(title half-width + ~40 px).
- **TabRow:** top ≈ 0.11–0.20 of frame height (band ~95 px). Three tabs occupy the left ~52% of the inner width starting at x≈0.20 (frame-relative); each tab capsule ≈ 200×60 px with ~24 px gap; **GLOBAL** is the leftmost and selected. SeasonTimerCluster right-anchored, occupying ~x[0.80,0.99] of the inner width.
- **LeftRail width:** ≈ 0.205 of frame inner width (~430 px). Vertical content order top→bottom: Emblem (~150 px tall) → LeagueName (~34 px) → LeagueDesc (3 lines wrap ~84 px) → LeagueRewards button (~64 px) → gap → MyRank block (~80 px) → MyScore block (~80 px). Center-aligned.
- **ListArea:** occupies x≈[0.205, 1.0] of frame inner width, y from below TabRow (~0.21) to frame bottom.
- **List column x-fractions (relative to ListArea width):**
  - RANK: center ≈ 0.075 (left), width ~0.13
  - PLAYER: starts ≈ 0.16 (avatar) then name block; spans ~0.16–0.56
  - LEAGUE: center ≈ 0.70, width ~0.18
  - SCORE: right-aligned, center ≈ 0.92, width ~0.16
- **ColumnHeaderRow height:** ≈ 42 px.
- **Row height:** ≈ 86 px each; row gap ≈ 8–10 px. Six visible rows fit between header and pinned row; ScrollRect scrolls if more exist. Medallion (rows 1-3) avatar Ø ≈ 56 px; plain rows avatar Ø ≈ 50 px.
- **MyRankPinnedRow:** height ≈ 92 px (slightly taller than list rows), pinned at ListArea bottom with ~8 px gap above it from the scroll viewport; blue outline thickness ~3 px.
- **FooterNote:** baseline ~24 px above frame bottom; font ~22 px.
- **Tablet 4:3 / ultrawide:** clamp BoardFrame max-width 0.92·2340; on very wide, keep rail and columns proportional (don't stretch text). **Notch:** all the above already inside SafeAreaRoot; BG layers ignore safe area.

## F. TYPOGRAPHY (per text)
> Intended: serif display (Trajan/Cinzel-inspired SDF, heavy gold bevel + soft bloom) for titles; clean semi-condensed sans for body/numbers (Roboto-Medium SDF). Shipped fallback: legacy `Text` + LegacyRuntime.ttf (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-spacing | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "LEADERBOARD" | prestige serif display | Heavy/Black | UPPER | +6% (wide) | 1.0 | gold bevel + outer bloom + 2px dark stroke + soft drop-shadow | ~64 | fill #f0d27a, stroke #3a2c0e |
| Tab labels (GLOBAL/FRIENDS/SEASON) | confident sans caps | Bold | UPPER | +4% | 1.0 | selected: white #f5f7fa w/ subtle glow; unselected: muted #b9a familiar gold-grey #9a8a6a | ~30 | sel #ffffff / unsel #9a8a6a |
| "Season Ends In" caption | utility | Medium | Title-case | 0 | 1.0 | none | ~22 | #b8a06a |
| Countdown "13d 14h 22m" | data | Bold | — | +2% | 1.0 | faint shadow | ~28 | #f0d27a |
| "LEGENDARY LEAGUE" | heading serif | Bold | UPPER | +3% | 1.05 | gold + soft glow | ~32 | #f0d27a |
| League description | body | Regular | Sentence | 0 | 1.15 | none | ~22 | #c9bfa6 |
| "League Rewards" btn label | CTA | Bold | Title/UPPER | +3% | 1.0 | white on blue + faint shadow | ~26 | #ffffff |
| "My Rank"/"My Score" captions | label | Medium | Title-case | +2% | 1.0 | none | ~22 | #b8a06a |
| My Rank value "128" | data display | Black | — | 0 | 1.0 | gold glow + shadow | ~52 | #f0d27a |
| My Score value "2,345,678" | data | Bold | — | +1% (tabular) | 1.0 | shadow | ~30 | #e9dcc0 |
| Column headers (RANK/PLAYER/LEAGUE/SCORE) | small caps label | Medium | UPPER | +6% | 1.0 | none | ~22 | #9a8a6a |
| Rank numerals (medallion) | display | Black | — | 0 | 1.0 | embossed dark stroke on metal | ~34 | #2a2010 on disk |
| Rank numerals (4+) | data | Bold | — | 0 | 1.0 | shadow | ~32 | #e9dcc0 |
| Player name | name | Bold | Title-case | +1% | 1.0 | shadow | ~30 | #f3ead2 |
| Guild subtitle | sub | Regular | Title-case | 0 | 1.0 | none | ~22 | #8c8270 |
| League tier text (LEGENDARY/CHAMPION/DIAMOND II) | label | Bold | UPPER | +4% | 1.0 | tier-colored glow | ~22 | LEG #b98bff / CHAMP #d8a14a / DIA #6fa8ff |
| Score value (rows) | data | Bold | — | +1% tabular | 1.0 | shadow | ~28 | #f0d27a |
| Footer note | fine print | Regular/italic | Sentence | 0 | 1.0 | none | ~22 | #7a7060 |

## G. MATERIALS
- **Outer frame:** brushed cast gold/antique bronze; base #8a6a28, highlight #f0d27a, shadow #5a3f12; roughness mid (satin), worn edges with micro-nicks, engraved filigree along the rails; **gold rim-light** top-left; subtle bloom on corner ornaments.
- **Panel fill (board interior):** near-black obsidian #0c0e14 → #14161e vertical gradient, very low reflectivity, faint inner shadow at the frame edge.
- **LeftRail sub-panel:** slightly darker inset with a thin gold hairline divider separating it from the list.
- **Medallions:** rank-1 gold (#f6dd86 highlight, #b8862c mid, #6b4a14 shadow) with bloom; rank-2 silver (#e6ebf1 / #b8c0c9 / #6c747d); rank-3 bronze (#e0a064 / #b9743a / #6e3f1c). Cast-disk relief, beveled rim, numeral debossed.
- **LeagueEmblem (trophy/crest):** polished gold with a small blue gem inset; strong focal bloom (it's the rail's hero).
- **League badges:** LEGENDARY = faceted **violet amethyst** crystal (#9e6bf0 core, #5a2db0 shadow, specular highlight); CHAMPION = bronze shield glyph; DIAMOND = blue/steel shield glyph; all with thin metal rim.
- **Avatars:** semi-realistic portrait sprites inside a beveled gold ring; top-3 ring slightly thicker/brighter.
- **Trophy icon (score):** small gold cup, satin metal, faint glow.
- **Blue CTA (League Rewards / pinned-row outline):** royal cobalt #2b56c8→#4f8bff gradient, glossy, inner highlight, soft outer glow.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **BackButton:** idle gold chevron on dark square; hover +8% brightness + faint glow; pressed scale 0.94 + inset; disabled n/a. Feedback: light "click" SFX, route pop.
- **Tabs (Toggle group):** selected = filled blue capsule, white label, slight raise + glow; idle/unselected = transparent/ghost capsule, muted gold-grey label; hover (unselected) = label brightens, faint underline; pressed = scale 0.97; disabled = 40% alpha. Switching tabs cross-fades the list content (see I/K).
- **Btn_LeagueRewards (primary-blue secondary):** idle cobalt gradient + glow; hover brighter + glow grows; pressed darken + scale 0.96; disabled desaturated grey @50%.
- **List rows:** idle subtle stripe; hover (pointer platforms) faint gold tint + 1px gold edge; pressed scale 0.99 + brief tint; selected (if tap-to-inspect) sustained faint blue tint; medallion rows have a permanent warm tint. **My-rank pinned row** is a sustained "selected" style (blue outline + brighter fill) regardless of pointer.
- **ScrollView:** elastic overscroll bounce; momentum flick; optional thin auto-hiding gold scrollbar.
- **Feedback rules:** all interactive elements ≥88px touch; hover only on pointer platforms; pressed gives ≤80 ms tactile scale.

## I. ANIMATION TIMELINE (timestamps, durations, easing)
- **OnShow (screen enter):** 0 ms CanvasGroup α 0→1 over 180 ms (ease-out). 60 ms BoardFrame scale 0.985→1.0 over 200 ms (ease-out-back lite). 120–360 ms list rows stagger-in: each row α 0→1 + slide +12px→0, 30 ms apart, 160 ms each (ease-out). 200 ms LeftRail emblem soft bloom pulse (one shot). 240 ms my-rank pinned row settles last with a brief blue glow flash (220 ms).
- **Countdown:** ticks every second; recompute string; subtle "::" no flash (avoid distraction).
- **Tab switch:** 0 ms old list α 1→0 + slide −10px over 120 ms; 120 ms new list α 0→1 + slide +10px→0 over 160 ms; selected capsule slides/fades to new tab over 140 ms (ease-out). Re-stagger rows lightly (15 ms apart).
- **Row hover (pointer):** tint/edge fade 90 ms.
- **OnHide:** CanvasGroup α 1→0 over 140 ms (ease-in), frame scale 1→0.99.
- **Easing defaults:** ease-out (Quad) for enters, ease-in for exits, ease-out-back (gentle, overshoot ≤3%) for the frame pop.

## J. PARTICLE & FX
- **Rank-1 medallion:** slow rotating specular sweep + faint sparkle motes (2–3 particles, low rate) and a steady focal bloom.
- **LeagueEmblem:** soft volumetric glow + occasional single sparkle on the gem inset.
- **Selected tab / blue CTA:** subtle pulsing rim glow (±10% intensity, 1.6 s loop).
- **My-rank pinned row:** gentle breathing blue outline glow (±8%, 2 s loop).
- **Background:** very slow drifting haze/dust over the skyline (optional, low alpha). Keep all FX low-key — the data must stay legible.

## K. EVENT BEHAVIOR
- **OnShow:** request leaderboard page for the active tab (default GLOBAL) from the server-auth meta service; show skeleton rows while loading; populate; compute & start the season countdown from server `seasonEndUtc`; bind my-rank/my-score from the player profile; pin my-rank row.
- **OnTabChanged(GLOBAL/FRIENDS/SEASON):** re-query the corresponding scope (FRIENDS = social graph; SEASON = current-season-only board); cross-fade list; keep my-rank pinned (its value may differ per scope).
- **OnRowTap (optional):** open a player-profile peek modal (read-only).
- **OnLeagueRewards:** open League Rewards detail/claim (server-validated grant; client shows result via RewardGrant popup, spec 38).
- **OnBack:** pop screen → return to caller (Main Menu rail).
- **OnRefreshTick:** every 15 min (or server push) silently refresh the visible page; show "updated" micro-toast optional.
- **Failure:** if the page fetch fails → show the full-screen NetworkError (spec 39) or an inline retry strip; never fabricate ranks.

## L. NEGATIVE RULES
- Do NOT let the client compute or edit any rank/score — **display only**, server-authoritative.
- Do NOT replace this screen with a modal; it is a full screen (modals float over it).
- Do NOT add real brand text, stick figures, or non-DNA colors. Trophy = score metaphor only.
- Do NOT stretch text or avatars on ultrawide — re-center the clamped frame instead.
- Do NOT make the footer note prominent; it is fine print.
- Do NOT animate the countdown digits with flashy effects (legibility > flourish).
- Shipped reality: legacy `Text`/LegacyRuntime.ttf will not render the gold-bevel serif convincingly — **flag the TMP SDF upgrade** for ≥95% title fidelity; do not block on it.
- Currency top-right is NOT part of this screen (Leaderboard shows no wallet) — do not add one (matches source).

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Title, crossed-sword ornaments, and frame match source placement within ±2% of canvas.
2. Three tabs present with GLOBAL selected (blue); season countdown cluster top-right with the exact format `Nd Nh Nm`.
3. Left rail shows emblem, "LEGENDARY LEAGUE", the description string, "League Rewards" blue button, "My Rank 128", "My Score 2,345,678".
4. List shows the four columns (RANK/PLAYER/LEAGUE/SCORE) and the six seed rows with EXACT names/guilds/leagues/scores; ranks 1-3 use gold/silver/bronze medallions, 4-6 plain numerals.
5. My-rank pinned row (128 / ValiantOne / Silver Wardens / DIAMOND II / 2,345,678) is blue-outlined and pinned at the bottom, not scrolling.
6. Footer reads "Leaderboard updates every 15 minutes." (fine print).
7. Colors within the DNA hex ranges; layout fraction-based and stable under match-height; safe-area respected; BG full-bleed.
8. Enter/tab-switch animations behave per Section I; no client-side rank mutation.

## N. IMPLEMENTATION CONFIDENCE
**92/100.** High: layout, columns, tab model, my-rank pin, color/material reads are unambiguous from the art. Risks: exact medallion relief and league-badge glyph artwork need bespoke sprites (-3); gold-bevel serif requires the TMP SDF upgrade not yet shipped (-3); subtle row-stripe alpha and FX intensities are interpretive (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present and substantive, in order.
- [x] Fraction-based layout normalized to 2340×1080; match-height; safe-area; full-bleed BG.
- [x] Exact visible strings/numbers recorded (names, guilds, leagues, scores, countdown, footer).
- [x] Per-text typography table + hex; materials with hex/finish.
- [x] Component states + animation timeline + FX + events + negative rules.
- [x] No code/assets/scenes; analysis-only; no invented content.
