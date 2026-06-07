# BULWARK — UI CONSTRUCTION SPEC · 27 · Campaign Map

Source: design/CampaignMapDesign.png · 1679×937 (1.79:1) · Analysis-only forensic spec.

> Normalize the source 1679×937 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" = on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Campaign Map** level-select meta/mode screen — a painted **world map** with a winding path of numbered
**level nodes** the player progresses along. Layout:
1. **Top-left chrome:** Back chevron; below it a vertical **difficulty toggle** — **NORMAL** (selected) /
   **HARD**.
2. **Top-right currency chips:** **Gems 2,340**, **Silver 12,450**, **Gold 1,850** (each with +).
3. **Map body (full-bleed art):** a glowing **golden path** snaking from bottom-left to the right, studded
   with circular **level nodes** numbered **1 2 3 … 4 5 6 6 7 9 10 11 12 12**, most crowned by **1–3 gold
   stars**. **Node 7** is the **current/active** node (highlighted cobalt, the player's hero standing on it).
   A **locked node** (padlock) sits at the far right. The terrain reads left→right as **dark forest/swamp
   (teal-green) → ruined central castle → volcanic lava (ember-red)**.
4. **Bottom-left:** a treasure-chest icon + **"36/39"** total stars collected.
5. **Bottom-right:** three quick-nav buttons — **Heroes** · **Rewards** · **Quests**.
6. (Implicit) **Level-detail + PLAY:** tapping a node opens a level-detail popup (stars, rewards, **PLAY**).

Purpose: visualize campaign progression, let the player pick/replay an unlocked level (with difficulty), see
star totals, and jump to Heroes/Rewards/Quests. Reached from: Main Menu (Campaign). Back: top-left chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** a sprawling dark-fantasy war map — the screen's art is the hero (full-bleed painting), chrome is
  minimal overlay. Left = haunted swamp (cool teal/green glow), center = colossal ruined castle, right =
  scorched volcanic wastes (ember/orange) — the terrain itself encodes the Iron-Pact→Ashen journey.
- **Palette anchors:**
  - Map art: deep greens/teals `#16302a`, stone greys `#2a2c30`, lava `#7a1f0a → #f0742c` on the right;
    overall dark with luminous focal nodes; vignette.
  - **Golden path:** glowing antique gold `#caa04a → #f0d27a` ribbon with soft bloom.
  - **Node ring (completed/available):** gold `#caa04a` ring, dark center, gold numeral.
  - **Current node (7):** cobalt `#2b56c8 → #4f8bff` glow + cyan rim + hero standing with a blue energy plume.
  - **Stars:** bright gold `#f0d27a`; empty/locked stars dim.
  - **Locked node:** desaturated steel ring + padlock.
  - Currency chips/gold chrome: `#6b5320 → #caa04a → #f0d27a`.
- **Hierarchy:** current node 7 (hero, cobalt focal) → golden path + numbered nodes/stars → top chrome
  (back/difficulty/currencies) → bottom nav (Heroes/Rewards/Quests) + star total.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
CampaignMapScreen (UiScreen root, CanvasGroup)
├─ MapBody_FullBleed (Image — the painted world map; pannable container)
│  ├─ Map_Art (Image, large terrain painting)  // forest→castle→lava
│  ├─ PathLayer (Image / line — glowing golden path)
│  ├─ NodeLayer (RectTransform — all level nodes positioned on the path)
│  │  ├─ Node_1  [ring + "1" + Stars(3)]
│  │  ├─ Node_2  [ring + "2" + Stars(3)]
│  │  ├─ Node_3  [ring + "3" + Stars(3)]
│  │  ├─ Node_4  [ring + "4" + Stars(3)]
│  │  ├─ Node_5  [ring + "5" + Stars(3)]
│  │  ├─ Node_6a [ring + "6" + Stars(3)]
│  │  ├─ Node_6b [ring + "6" + Stars(2)]
│  │  ├─ Node_7  [CURRENT — cobalt glow + "7" + Hero standee]
│  │  ├─ Node_9  [ring + "9" + Stars(3)]
│  │  ├─ Node_10 [ring + "10" + Stars(3)]
│  │  ├─ Node_11 [ring + "11" + Stars(3)]
│  │  ├─ Node_12a[ring + "12" + Stars(3)]
│  │  ├─ Node_12b[ring + "12" + Stars(3)]
│  │  └─ Node_Locked [steel ring + padlock]   // far right
│  │     (each Node:)
│  │     ├─ Node_Ring (Image, gold/cobalt/steel)
│  │     ├─ Node_Number (Text) or Node_Lock (Image)
│  │     ├─ StarRow (HorizontalLayoutGroup, up to 3 stars) above node
│  │     └─ CurrentGlow + HeroStandee (only Node_7)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter — overlay chrome)
│  ├─ TopLeftChrome
│  │  ├─ Btn_Back (Button) [chevron] top-left
│  │  └─ DifficultyToggle (VerticalLayoutGroup)
│  │     ├─ Toggle_Normal (Toggle, SELECTED) [icon | "NORMAL"]
│  │     └─ Toggle_Hard (Toggle) [icon | "HARD"]
│  ├─ CurrencyChips (HorizontalLayoutGroup) top-right
│  │  ├─ Chip_Gems   [gem | "2,340" | Btn_Plus]
│  │  ├─ Chip_Silver [silver-coin | "12,450" | Btn_Plus]
│  │  └─ Chip_Gold   [gold-coin | "1,850" | Btn_Plus]
│  ├─ StarTotal (bottom-left) [chest icon | "36/39"]
│  └─ BottomNav (HorizontalLayoutGroup, bottom-right, 3)
│     ├─ Btn_Heroes  [icon | "Heroes"]
│     ├─ Btn_Rewards [icon | "Rewards"]
│     └─ Btn_Quests  [icon | "Quests"]
├─ LevelDetailPopup (hidden until a node is tapped)   // overlay
│  ├─ Popup_BG (Image dim)
│  ├─ Popup_Panel (Image framed)
│  │  ├─ Lbl_LevelTitle "Level N"
│  │  ├─ StarRow_Earned (up to 3)
│  │  ├─ RewardPreview (icons)
│  │  └─ Btn_Play (Button) "PLAY"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| MapBody_FullBleed | Screen | 0 | RectTransform (pannable) | stretch (overscan) | .5,.5 | — | **ignores** safe area (full-bleed) | can pan/scale; wider on ultrawide |
| Map_Art | MapBody | 0 | Image | stretch | .5,.5 | — | full-bleed | art bleeds under cutout |
| PathLayer | MapBody | 1 | Image/UILineRenderer | stretch | .5,.5 | — | full-bleed | scales with map |
| NodeLayer | MapBody | 2 | RectTransform | stretch | .5,.5 | — | full-bleed | nodes anchored by fraction |
| Node_* | NodeLayer | 0..n | Button | point-anchor (fraction) | .5,.5 | center | follows map | fixed node size, fraction position |
| Node_Ring | Node | 0 | Image | stretch | .5,.5 | — | — | scales |
| Node_Number/Lock | Node | 1 | Text/Image | center | .5,.5 | center | — | — |
| StarRow | Node | 2 | HorizontalLayoutGroup | top-center (above ring) | .5,0 | center | — | — |
| CurrentGlow/HeroStandee | Node_7 | 3 | Image | center/bottom | .5,0 | center | — | scales |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Btn_Back | SafeAreaRoot | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| DifficultyToggle | SafeAreaRoot | 1 | VerticalLayoutGroup | top-left (below back) | 0,1 | center | inside | pinned left |
| Toggle_Normal/Hard | DifficultyToggle | 0,1 | Toggle (group) | — | — | mid | inside | stacked |
| CurrencyChips | SafeAreaRoot | 2 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-pinned |
| Chip_* | CurrencyChips | 0..2 | RectTransform(Image+Text+Button) | — | — | mid | inside | fixed |
| StarTotal | SafeAreaRoot | 3 | RectTransform(icon+Text) | bottom-left | 0,0 | left | inside | pinned BL |
| BottomNav | SafeAreaRoot | 4 | HorizontalLayoutGroup | bottom-right | 1,0 | right | inside | pinned BR |
| Btn_Heroes/Rewards/Quests | BottomNav | 0..2 | Button | — | — | mid | inside | equal |
| LevelDetailPopup | Screen | 2 | RectTransform+CanvasGroup | stretch | .5,.5 | — | overlay | centered modal |
| Popup_Panel | LevelDetailPopup | 1 | Image(framed) | center | .5,.5 | center | inside | clamp size |
| Btn_Play | Popup_Panel | n | Button | bottom-center | .5,0 | center | inside | wide CTA |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Map body:** full-bleed; the painting fills the whole canvas (extends under cutout). If the source art is
narrower than 19.5:9, it is scaled to **cover** (crop top/bottom minimally) and may be **horizontally pannable**
to reach off-screen nodes; default view frames nodes 1→12 as drawn.

**Overlay chrome (inside safe area):**
- **Btn_Back:** center x 0.030, y 0.955; ≈0.045w×0.085h.
- **DifficultyToggle:** top-left under back, x≈0.030, y 0.840 (Normal) & 0.760 (Hard). Each ≈0.060w wide
  stacked chip: an icon (skull/swords) over the label "NORMAL"/"HARD" (≈18px). Normal selected = gold-lit
  frame; Hard idle = dim.
- **CurrencyChips:** right edge x 0.972, y 0.955; **three** chips (Gems, Silver, Gold) ≈0.115w×0.052h, gap
  0.010w.
- **StarTotal:** bottom-left, x≈0.020, y≈0.045; gold chest icon (Ø≈0.05w) + "36/39" (≈30px gold) to its right.
- **BottomNav:** bottom-right, right edge x 0.972, y≈0.060; three icon+label buttons (Heroes, Rewards, Quests)
  each ≈0.075w, gap 0.010w; small gold-rimmed dark chips with an icon over the label (≈22px).

**Node positions (fractions of the map / canvas — approximate, transcribed from the painting; tune to the
final art):** node ring Ø ≈ 0.040w (≈94px@1080); current node (7) ≈ ×1.4 with cobalt glow; stars sit ~0.045h
above the ring.
| Node | x (frac) | y (frac) | Stars | State |
|---|---|---|---|---|
| 1 | 0.115 | 0.135 | 3 | completed |
| 2 | 0.230 | 0.155 | 3 | completed |
| 3 | 0.330 | 0.180 | 3 | completed |
| 4 | 0.315 | 0.300 | 3 | completed |
| 5 | 0.290 | 0.405 | 3 | completed |
| 6a | 0.270 | 0.560 | 3 | completed |
| 6b | 0.310 | 0.700 | 2 | completed |
| 7 (CURRENT) | 0.385 | 0.640 | — | active (hero) |
| 9 | 0.500 | 0.470 | 3 | completed |
| 10 | 0.570 | 0.390 | 3 | completed |
| 11 | 0.640 | 0.255 | 3 | completed |
| 12a | 0.730 | 0.460 | 3 | completed |
| 12b | 0.800 | 0.470 | 3 | completed |
| Locked | 0.880 | 0.420 | — | locked (padlock) |

(Positions are read off the mockup; the **golden path** connects them in number order 1→2→3→4→5→6→6→7→9→10→11
→12→12→(locked). The current node 7 carries a tall **cobalt energy plume + hero standee** and is the brightest
focal point. Node numbering as drawn skips 8 and repeats 6 and 12 — transcribe **exactly as drawn** and flag
the numbering anomaly to design.)

**StarRow per node:** up to 3 small gold stars, centered above the ring, total width ≈0.060w; earned stars
bright, missing stars dim (6b shows 2/3).

**LevelDetailPopup (on node tap):** centered modal ≈0.42w × 0.52h, dark dim behind; shows "Level N", earned
stars (up to 3), a small reward preview, and a wide cobalt **PLAY** CTA at the bottom (≈0.30w × 0.070h, white
"PLAY"). Replay-able for unlocked nodes; locked nodes instead show "Locked — clear Level N-1".

**Tablet (4:3):** map covers (more top/bottom visible); chrome stays pinned to corners; node fractions hold
relative to the art (anchor nodes to the art container, not the screen, so they track when the art is letter/
pillar-cropped). **Ultrawide (21:9):** more of the map's width shows; chrome corner-pinned; pan range reduces.
**Notch:** SafeAreaRoot insets all chrome; the map art slides under the cutout; back/chips/nav never enter it.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| Node numbers (1..12) | tabular serif | Bold | — | — | gold + dark stroke for legibility over art | 30 | `#fff4d6` |
| Current node "7" | tabular serif | Black | — | — | cobalt glow + white stroke | 34 | `#ffffff` |
| "NORMAL" / "HARD" | strong sans | Bold | UPPER | +6% | selected gold; idle dim; dark stroke | 18 | sel `#f0d27a`; idle `#9aa3ad` |
| Currency values | tabular sans | Semibold | — | tabular | 1px stroke | 30 | `#f4e9c8` |
| "36/39" star total | tabular serif | Bold | — | tabular | gold glow + dark stroke | 30 | `#f0d27a` |
| Bottom nav labels | sans | Medium | Title | +2% | dark stroke over art | 22 | `#e8e2cf` |
| Popup "Level N" | serif display | Bold | UPPER | +5% | gold bevel + bloom | 40 | `#f0d27a` |
| Popup "PLAY" | strong sans | Bold | UPPER | +6% | white + dark stroke | 32 | `#ffffff` |

Note: all map-overlaid text uses a **thin dark stroke + soft shadow** for legibility over the busy painting.

---

## G — MATERIALS
- **Map art:** painterly high-fantasy terrain — left swamp/forest (cool teal/green, eerie glow, dead trees),
  center colossal ruined castle (cold stone, blue moon-glow), right volcanic wastes (lava cracks, ember haze,
  oxblood rock). Low-key with luminous focal accents; strong vignette toward edges.
- **Golden path:** a worn-gold/parchment ribbon (`#caa04a`→`#f0d27a`) laid on the terrain with soft outer
  bloom and faint footstep/cobble texture; brighter near the current node.
- **Node rings (completed/available):** cast-gold rings with dark centers + a beveled rim and subtle bloom;
  completed nodes feel "lit". 
- **Current node (7):** cobalt glowing ring + cyan rim + a vertical blue energy plume; a small hero standee
  (Iron Pact warrior) on the node; the map's brightest element.
- **Stars:** small faceted gold stars with bloom; missing stars are dim grey outlines.
- **Locked node:** desaturated steel ring with a gold padlock; no glow.
- **Currency chips / chrome:** brushed gold-rimmed dark chips (matching the rest of the meta UI).
- **Difficulty toggle:** dark gold-rimmed chips with skull/sword icons; selected = gold-lit, idle = dim.
- **Bottom nav:** dark gold-rimmed chips with icons (helm/heroes, chest/rewards, scroll/quests).
- **PLAY CTA (popup):** cobalt enamel + gold trim, white label, idle glow.

---

## H — COMPONENTS (states)
**Level Node (Button):**
- *completed:* gold ring lit, number bright, 1–3 gold stars above (per earned).
- *available/next (not the current focal but unlocked):* gold ring, may pulse gently to invite.
- *current (7):* cobalt glow + hero standee + plume; tapping opens the detail/PLAY for the next playable level.
- *hover/focus:* ring brightens +15%, slight scale 1.05, soft glow.
- *pressed:* scale 0.95 → opens LevelDetailPopup.
- *locked:* steel ring + padlock, desaturated; tap → "Locked — clear Level N-1" toast (no popup PLAY).
- Stars: filled gold (earned) vs dim (unearned), e.g. node 6b shows 2/3.

**DifficultyToggle (Normal/Hard, single-select):**
- *selected:* gold-lit chip; *idle:* dim; switching reloads node states/star counts for that difficulty.
  (Hard may show fewer stars earned / locked-until-Normal-cleared.)

**CurrencyChips / BottomNav buttons:** standard meta chips/buttons — hover brighten, pressed scale; chip +
routes to Store; nav routes to Heroes/Rewards/Quests screens.

**LevelDetailPopup / Btn_Play:**
- *PLAY idle:* cobalt, white "PLAY", glow; *hover:* +10% bright; *pressed:* scale 0.97 → launch match-intro
  for that level+difficulty.
- *locked level:* popup replaced by a locked message (no PLAY).

**Map pan:** drag to pan horizontally within bounds (if art exceeds viewport); momentum + clamp.

**Back button:** standard.

---

## I — ANIMATION TIMELINE
**OnShow (~0.7s):**
- 0.00s Map art + vignette fade in (0.25s); lava/ember and swamp glows begin ambient loops.
- 0.10s **Golden path draws on** from node 1 toward the current node (a light sweeps along the ribbon, ~0.45s).
- 0.15s **Nodes pop in** in number order (each scale 0→1.1→1.0 + fade, 0.04s stagger) as the path reaches
  them; stars above each node pop a beat after their node (0.03s each).
- 0.30s **Current node 7** gets a stronger arrival: cobalt glow blooms, hero standee fades in, energy plume
  rises (0.30s).
- 0.20s Top chrome (back, difficulty, chips) fades/slides in (0.20s).
- 0.40s StarTotal counts up to "36/39"; bottom nav fades/slides up (0.20s).

**OnSelectNode (~0.25s):** node press scale-down; LevelDetailPopup dim-in + panel scale 0.94→1.0 (0.22s);
earned stars in the popup pop sequentially; PLAY gets a glow sweep.
**OnPlay:** PLAY flash → popup fades, screen transitions to Match Intro (0.30s).
**OnDifficultySwitch (~0.35s):** nodes/stars cross-update (re-pop changed states); path tint may shift.
**Idle ambient:** current node plume pulses; lava embers + swamp wisps loop; available nodes gently pulse.

---

## J — PARTICLE & FX
- **Current node 7:** cobalt energy plume + rising spark motes + soft pulsing glow (the focal beacon).
- **Golden path:** a slow light glint travels along it (~6s loop); brighter segment near the current node.
- **Terrain ambient:** swamp wisps/fireflies (left, teal), drifting embers + heat shimmer (right, lava),
  faint dust around the central castle.
- **Completed nodes:** subtle gold shimmer on stars.
- **Node select:** gold sparkle burst on tap.
- **PLAY:** cobalt spark ring on press.
Budget pooled/capped; reduce wisps/embers/motes on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load campaign progress (per-node completion + stars, current/active node, locked nodes) for the
  active difficulty (default Normal) read-only; compute star total (36/39); run enter timeline.
- **OnDifficulty(Normal/Hard):** reload node states/stars/star-total for that difficulty.
- **OnNodeTap(level):** if unlocked → open LevelDetailPopup (stars, rewards, PLAY); if locked → toast unlock
  requirement.
- **OnPlay(level,difficulty):** route to Match Intro → battle for that level (server-auth level entry).
- **OnHeroes/Rewards/Quests:** route to those screens.
- **OnPlus(chip):** route to Store.
- **OnPanMap:** scroll the map within bounds.
- **OnBack:** pop → Main Menu.
- **§12:** progression is server-authoritative/read-only; launching a level is a meta navigation request; UI
  never mutates progress or balances; no ECS write.

---

## L — NEGATIVE RULES
- Transcribe nodes **exactly as drawn**: numbers 1,2,3,4,5,6,6,7,9,10,11,12,12 + one locked node; node 7 is
  the cobalt current node with the hero; star total "36/39"; difficulty NORMAL (selected)/HARD; currencies
  Gems 2,340 / Silver 12,450 / Gold 1,850; bottom nav Heroes / Rewards / Quests.
- **Flag, do not fix,** the numbering anomaly (skipped 8, repeated 6 and 12) — keep as drawn; note to design.
- Three currency chips here (Gems + Silver + Gold).
- Do not redesign the map art, path shape, or terrain split; the painting is the hero.
- Do not add a level list/grid alternative on this screen (it is a map).
- No portrait variant, no stick figures, no real brand text.
- No local progress/balance/ECS mutation; PLAY is a navigation request.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Full-bleed painted world map (forest→castle→lava) with a glowing golden path connecting numbered nodes in
   order.
2. Nodes 1–12 (as drawn, incl. repeats) with 1–3 gold stars each (6b = 2/3); node 7 = cobalt current node with
   hero standee + plume; far-right locked padlock node.
3. Top-left back + NORMAL/HARD difficulty toggle (Normal selected); top-right Gems/Silver/Gold chips.
4. Bottom-left chest + "36/39"; bottom-right Heroes/Rewards/Quests nav.
5. Tapping a node opens a level-detail popup with stars + PLAY (cobalt); locked nodes show a locked message.
6. Path-draw, node-pop stagger, current-node bloom, star count-up animations present.
7. All overlaid text uses dark stroke/shadow for legibility over the art.
8. Safe-area honored for chrome; map full-bleed under cutout; holds on 4:3 / 19.5:9 / 21:9 / notched landscape;
   node anchors track the art container.
9. Hex/typography within F/G ranges; node 7 is the brightest focal element.

## N — IMPLEMENTATION CONFIDENCE
**85/100.** High confidence on chrome (back, difficulty toggle, three chips, star total, bottom nav), the
current-node-7 focal treatment, stars, locked node, and the terrain/path identity — all clearly legible. The
main uncertainty is **exact node coordinates** (read approximately off the painting; table values must be
tuned to the final art asset) and the **level-detail popup contents** (the popup isn't shown in the mockup —
its stars/rewards/PLAY composition is inferred from genre + the bible's CTA conventions). The node-numbering
anomaly is a design issue, transcribed not corrected.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 27 · Campaign Map".
- [x] Fraction-based layout normalized to 2340×1080; node coordinate table + sizes given.
- [x] Map pan/full-bleed + node anchoring (to art container) specified; popup/PLAY covered.
- [x] Node / difficulty / nav / PLAY states (completed/available/current/locked/selected/idle).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; numbering anomaly flagged not fixed.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.
