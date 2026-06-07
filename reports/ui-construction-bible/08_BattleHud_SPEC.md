# BULWARK — UI CONSTRUCTION SPEC · 08 · Battle HUD (In-Match)

Source: design/BattleHudDesign.png · 1672×941 (1.78:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 control boundary from `00_CONTEXT_RECOVERY.md`. This is the **primary
> in-match HUD** overlaying the live ECS battlefield (top-down medieval armies). It MUST keep the battlefield
> center clear, read the sim **read-only**, and route only the permitted writes (`Training.EnqueueTrain`,
> `MoveDestination` via the order buttons, `Time.timeScale` via Pause).

---

## A · SCREEN PURPOSE
The combat dashboard. It frames the live battle with: (1) a **top bar** showing both statues' **HP** as faction
bars (left Iron Pact blue "10,000 / 10,000", right Ashen Horde red "9,200 / 10,000") flanked by crests, with a
**Pause** button at top-left; (2) the player's **resource readouts** — in-battle **Gold "150"** and a
**population/supply** counter "**2/8**" (current/cap) at top-left, and the **enemy/army** counter "**15/50**" at
top-right; (3) a bottom-left **unit-train tray** of **5 portrait buttons** each with a **gold cost** (60, 90, 75,
120, 150) that enqueue training; and (4) a bottom-right **order cluster** of three CTAs — **GARRISON** (shield),
**DEFEND** (crossed swords), **ATTACK** (chevrons) — that set the army's stance/move order. The HUD never covers
the battlefield's center third. It is the screen the player lives on during a fight.

## B · VISUAL DNA (screen-specific)
- **Edge-hugging frame, open center.** All chrome clings to the top strip and the two bottom corners; the middle
  of the screen is the playable battlefield (dirt field, blue army left, red army right, scattered fires).
- **Top HP bar** = two long horizontal gauges meeting under a small central node, each capped on its outer end by
  a faction **crest medallion** (blue tower-shield left, red horned-skull right) on an ornate gold mount. Bars
  are gold-framed troughs; fill is faction-colored with a glossy top sheen and a numeric "current / max" label.
- **Resource chips** = small dark gold-rimmed lozenges with an icon + number (gold coin "150"; a banner/supply
  icon "2/8"; a unit/helmet icon "15/50").
- **Pause** = a round/squircle dark gold-rimmed button with a "❚❚" glyph, top-left corner.
- **Unit tray** = 5 square-ish gold-framed **portrait tiles** (armored unit busts: spearman, swordsman, archer,
  cavalry, ballista/siege) in a row, each with a small gold-coin cost chip on its lower edge.
- **Order cluster** = 3 wide dark gold-rimmed buttons in a row, each with an icon above/left of an UPPERCASE
  label; ATTACK is the brightest (primary), rendered with bold gold chevrons.
- Heavy bottom + top **scrim gradient** so chrome stays legible over the bright battlefield; gold rim-light on all
  frames; faction-tinted glows on the HP fills.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
BattleHudScreen (UiScreen, CanvasGroup, overlays live ECS battle)
└─ Root (RectTransform, stretch-all)
   ├─ Scrim_Top (Image, top gradient, raycast off)
   ├─ Scrim_Bottom (Image, bottom gradient, raycast off)
   ├─ SafeArea (RectTransform + SafeAreaFitter)            // ALL interactive chrome insets here
   │  ├─ TopBar (Container, top stretch)
   │  │  ├─ PauseButton (Button)                           // ❚❚ , top-left
   │  │  │  ├─ Pause_Frame (Image)
   │  │  │  └─ Pause_Glyph (Image "❚❚")
   │  │  ├─ HpBar_Left_IronPact (Container)
   │  │  │  ├─ Hp_Crest_L (Image)                          // blue tower-shield medallion
   │  │  │  ├─ Hp_Trough_L (Image)                         // gold-framed empty trough
   │  │  │  ├─ Hp_Fill_L (Image, filled, blue)             // depletes right→left toward center
   │  │  │  └─ Hp_Text_L (Text "10,000 / 10,000")
   │  │  ├─ HpBar_CenterNode (Image)                       // small gold junction finial
   │  │  ├─ HpBar_Right_AshenHorde (Container)
   │  │  │  ├─ Hp_Crest_R (Image)                          // red horned-skull medallion
   │  │  │  ├─ Hp_Trough_R (Image)
   │  │  │  ├─ Hp_Fill_R (Image, filled, red)              // depletes left→right toward center
   │  │  │  └─ Hp_Text_R (Text "9,200 / 10,000")
   │  │  ├─ ResChip_Gold (Container, under pause)
   │  │  │  ├─ Res_Icon_Coin (Image)
   │  │  │  └─ Res_Val_Gold (Text "150")
   │  │  ├─ ResChip_Supply (Container, right of gold)
   │  │  │  ├─ Res_Icon_Supply (Image)                     // banner/standard glyph
   │  │  │  └─ Res_Val_Supply (Text "2/8")
   │  │  └─ ResChip_Army (Container, top-right)
   │  │     ├─ Res_Icon_Helmet (Image)                     // unit/helmet glyph
   │  │     └─ Res_Val_Army (Text "15/50")
   │  ├─ UnitTray (HorizontalLayout, bottom-left)
   │  │  ├─ UnitBtn_0 (Button)  ├ Portrait + CostChip("60")   // spearman
   │  │  ├─ UnitBtn_1 (Button)  ├ Portrait + CostChip("90")   // swordsman
   │  │  ├─ UnitBtn_2 (Button)  ├ Portrait + CostChip("75")   // archer
   │  │  ├─ UnitBtn_3 (Button)  ├ Portrait + CostChip("120")  // cavalry
   │  │  └─ UnitBtn_4 (Button)  └ Portrait + CostChip("150")  // ballista/siege
   │  │     (each UnitBtn_n: TileFrame, UnitPortrait, CostChip{CoinIcon,CostText}, CooldownVeil, AffordTint)
   │  └─ OrderCluster (HorizontalLayout, bottom-right)
   │     ├─ Btn_Garrison (Button)  ├ ShieldIcon + Label "GARRISON"
   │     ├─ Btn_Defend   (Button)  ├ CrossedSwordsIcon + Label "DEFEND"
   │     └─ Btn_Attack   (Button)  └ ChevronsIcon + Label "ATTACK"   // primary/brightest
   └─ (battlefield is NOT a UI node — live ECS render beneath the canvas)
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Screen | 0 | RectTransform | stretch-all | 0.5,0.5 | offsets 0 | n/a | fills |
| Scrim_Top | Root | 0 | Image | top stretch (0,~0.82)-(1,1) | 0.5,1 | dark→clear downward, raycast off | no | full width |
| Scrim_Bottom | Root | 1 | Image | bottom stretch (0,0)-(1,~0.2) | 0.5,0 | dark→clear upward, raycast off | no | full width |
| SafeArea | Root | 2 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | insets to Screen.safeArea | **yes** | chrome anchors here |
| TopBar | SafeArea | 0 | Container | top stretch (0,~0.86)-(1,1) | 0.5,1 | hangs from safe top | yes | width-stretch |
| PauseButton | TopBar | 0 | Button | top-left (0,1) | 0,1 | squircle | yes | fixed px |
| Pause_Frame | PauseButton | 0 | Image | stretch-all | 0.5,0.5 | gold-rim dark fill | yes | — |
| Pause_Glyph | PauseButton | 1 | Image | center | 0.5,0.5 | "❚❚" | yes | — |
| HpBar_Left_IronPact | TopBar | 1 | Container | top, left-of-center | 1,1 | inner edge → center | yes | width scales w/ safe |
| Hp_Crest_L | HpBar_L | 0 | Image | outer-left end | 0.5,0.5 | blue medallion | yes | fixed |
| Hp_Trough_L | HpBar_L | 1 | Image (sliced) | stretch (within bar) | 0.5,0.5 | gold frame trough | yes | — |
| Hp_Fill_L | HpBar_L | 2 | Image (Filled, Horizontal, origin=Right) | inside trough | 1,0.5 | blue fill, anchored to inner/center end | yes | fillAmount=hp/max |
| Hp_Text_L | HpBar_L | 3 | Text | center of bar | 0.5,0.5 | "10,000 / 10,000" | yes | — |
| HpBar_CenterNode | TopBar | 2 | Image | top-center (0.5,1) | 0.5,1 | small gold finial | yes | center-locked |
| HpBar_Right_AshenHorde | TopBar | 3 | Container | top, right-of-center | 0,1 | mirror of left | yes | width scales |
| Hp_Crest_R | HpBar_R | 0 | Image | outer-right end | 0.5,0.5 | red medallion | yes | fixed |
| Hp_Trough_R | HpBar_R | 1 | Image (sliced) | stretch | 0.5,0.5 | gold frame trough | yes | — |
| Hp_Fill_R | HpBar_R | 2 | Image (Filled, Horizontal, origin=Left) | inside trough | 0,0.5 | red fill, anchored to inner/center end | yes | fillAmount=hp/max |
| Hp_Text_R | HpBar_R | 3 | Text | center of bar | 0.5,0.5 | "9,200 / 10,000" | yes | — |
| ResChip_Gold | TopBar | 4 | Container | left, below pause | 0,1 | offset down ~0.10H | yes | fixed |
| ResChip_Supply | TopBar | 5 | Container | right of gold chip | 0,1 | same row as gold | yes | fixed |
| ResChip_Army | TopBar | 6 | Container | top-right (1,1) | 1,1 | mirrors pause corner | yes | fixed |
| UnitTray | SafeArea | 1 | HorizontalLayoutGroup | bottom-left (0,0) | 0,0 | 5 tiles, spacing const | yes | pin BL |
| UnitBtn_0..4 | UnitTray | 0..4 | Button | layout element | 0.5,0.5 | square tiles | yes | fixed cell |
| TileFrame | UnitBtn | 0 | Image (sliced) | stretch-all | 0.5,0.5 | gold frame | yes | — |
| UnitPortrait | UnitBtn | 1 | Image | stretch (inset) | 0.5,0.5 | unit bust, masked | yes | — |
| CostChip | UnitBtn | 2 | Container | bottom-center of tile | 0.5,0 | coin + number | yes | — |
| CooldownVeil | UnitBtn | 3 | Image (Filled, Radial360) | stretch-all | 0.5,0.5 | dark radial wipe (training/CD) | yes | — |
| AffordTint | UnitBtn | 4 | Image | stretch-all | 0.5,0.5 | red/desat overlay when gold<cost | yes | — |
| OrderCluster | SafeArea | 2 | HorizontalLayoutGroup | bottom-right (1,0) | 1,0 | 3 buttons | yes | pin BR |
| Btn_Garrison | OrderCluster | 0 | Button | layout element | 0.5,0.5 | shield + label | yes | fixed |
| Btn_Defend | OrderCluster | 1 | Button | layout element | 0.5,0.5 | swords + label | yes | fixed |
| Btn_Attack | OrderCluster | 2 | Button | layout element | 0.5,0.5 | chevrons + label (primary) | yes | fixed |

**Child-order rationale:** scrims first (behind chrome, raycast off so the battlefield receives the empty-center
taps for `MoveDestination`); TopBar, then trays. Within HP bars, trough→fill→text. Within unit tiles,
frame→portrait→cost→cooldown veil→afford tint (veil & tint paint over the portrait but under no text by being
semi-transparent).

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Clear battlefield zone (inviolable):** the central rectangle x∈[0.20,0.80], y∈[0.20,0.82] carries **no
  raycast-blocking chrome** so orders/selection land on the sim.
- **TopBar band:** y∈[0.86,1.0] (height ≈0.14H). **Scrim_Top** y∈[0.82,1.0]; **Scrim_Bottom** y∈[0,0.20].
- **PauseButton:** squircle ≈ **0.066H** (≈71 px) per side; top-left, its top ≈0.025H below safe top, left
  ≈0.012W from safe left.
- **HP bars:** each bar length ≈ **0.30W** (≈702 px) × height ≈ **0.040H** (≈43 px). Left bar inner end at
  x≈0.485, extending left to ≈0.185; right bar inner end at x≈0.515, extending right to ≈0.815. **CenterNode**
  Ø≈0.05H at x=0.50, y≈0.965. **Crest medallions** Ø≈ **0.085H**: left crest centered at x≈0.165, right at
  x≈0.835, both at the bar's vertical center (y≈0.945). HP numeric text centered on each bar, cap-height ≈0.022H.
  Fill direction: left fills from its **right (inner)** edge leftward; right fills from its **left (inner)** edge
  rightward — i.e., both deplete toward the center node.
- **ResChip_Gold:** ≈ **0.085W × 0.052H**, left edge x≈0.012, top ≈0.10H below safe top (directly under pause).
  Coin icon Ø≈0.04H at the chip's left; number right of it. **ResChip_Supply:** same height, ≈0.075W wide,
  immediately right of the gold chip (gap ≈0.012W). **ResChip_Army:** ≈ **0.085W × 0.052H**, right edge x≈0.988,
  top ≈0.025H below safe top (top-right corner, mirroring pause).
- **UnitTray:** 5 tiles, each ≈ **0.085W × 0.135H** (≈199×146 px), spacing ≈0.012W. Tray left edge x≈0.012,
  bottom ≈0.045H above safe bottom. Total tray width ≈0.473W (ends at x≈0.485 — stays left of center). Each
  **CostChip** ≈0.055W × 0.034H centered on the tile's bottom edge, overlapping the frame by ~40%.
- **OrderCluster:** 3 buttons, each ≈ **0.105W × 0.105H** (≈246×113 px), spacing ≈0.010W. Cluster right edge
  x≈0.988, bottom ≈0.045H above safe bottom. Total width ≈0.335W (starts at x≈0.653 — stays right of center).
  Icon sits above (or left of) the label; label cap-height ≈0.024H.
- **Tablet (1.33:1):** HP bars shorten to ≈0.26W each (keep the center gap), chips/buttons keep px sizing; tray
  and cluster stay corner-pinned. **Ultrawide (21:9):** the empty center widens (good); HP bars keep ≈0.30W and
  stay flanking the center node; tray/cluster stay corner-pinned — extra width becomes more battlefield. **Notch:**
  pause, army chip, and the bar crests inset via SafeArea so a side-notch never clips them; scrims pass under.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| Hp_Text_L | 10,000 / 10,000 | tabular numerals, clean | Semibold | — | +2% | 1px #08101e stroke + drop-shadow (0,1,#000) | ~24 | #eaf1ff (cool white) |
| Hp_Text_R | 9,200 / 10,000 | tabular numerals | Semibold | — | +2% | 1px #1e0808 stroke + drop-shadow | ~24 | #ffeae6 (warm white) |
| Res_Val_Gold | 150 | tabular numerals | Bold | — | 0 | thin dark stroke + soft gold inner glow | ~28 | #ffe9a8 (gold) |
| Res_Val_Supply | 2/8 | tabular numerals | Bold | — | 0 | thin dark stroke | ~26 | #e8e2cf (parchment) |
| Res_Val_Army | 15/50 | tabular numerals | Bold | — | 0 | thin dark stroke | ~26 | #ffd9cf (warm — enemy/army) |
| CostText (×5) | 60 / 90 / 75 / 120 / 150 | tabular numerals | Bold | — | 0 | 1px #1a1206 stroke + drop-shadow; turns red when unaffordable | ~22 | #ffe9a8 (gold); #ff7a6a if gold<cost |
| Label GARRISON | GARRISON | sturdy serif/condensed | Bold | UPPER | +6% | dark stroke + soft shadow | ~26 | #ecd9a6 (warm gold) |
| Label DEFEND | DEFEND | sturdy serif/condensed | Bold | UPPER | +6% | dark stroke + soft shadow | ~26 | #ecd9a6 |
| Label ATTACK | ATTACK | sturdy serif/condensed | **Black** | UPPER | +6% | dark stroke + brighter gold bloom (primary) | ~28 | #ffe6a0 (brightest) |

Numbers use a **tabular** SDF so digits don't jitter as values tick. ATTACK is the heaviest, brightest label =
primary CTA. Faction HP text carries a faint cool/warm tint to reinforce sides.

## G · MATERIALS
- **Gold/bronze frames (pause, chips, HP troughs, tiles, order buttons):** base #8a6a2c, highlight #f0d27a,
  shadow #5a4318; satin metal (roughness ≈0.35), engraved bevel, sharp top specular, gold rim-bloom; corners
  show light wear.
- **Dark fills (button/chip interiors):** near-black glass #0c0e14 @ ~85% with a faint inner top sheen.
- **HP fill — Iron Pact (blue):** gradient #2b56c8→#4f8bff with a glossy top highlight #bcd3ff and a soft blue
  outer glow; the trough behind is #14110a (dark) under gold frame.
- **HP fill — Ashen Horde (red):** gradient #7a1f1a→#d8452b with top highlight #ff9d7a and a soft ember glow.
- **Crest medallions:** circular gold mounts; **left** holds a cobalt tower/shield device; **right** a oxblood
  horned-skull device; both with rim-light + slight gloss + soot on the red one.
- **Unit portrait tiles:** painted armored busts (steel/blue tint to read as the player's faction), masked into
  the tile with a subtle dark vignette; gold inner frame line; the cost coin is bright minted gold.
- **Order icons:** **GARRISON** = gold/steel **shield**; **DEFEND** = **crossed swords**; **ATTACK** = bold
  **double/triple chevrons** in bright gold (the only icon that reads as "go").
- **Scrims:** vertical alpha gradients of #05060a (≈0→0.7), no texture, purely for legibility.

## H · COMPONENTS (each interactive)
1. **PauseButton** — *purpose:* open the Pause modal (sets `Time.timeScale = 0`). *States:* **idle** gold-rim
   dark; **hover/focus** rim brightens +12%; **pressed** scale 0.94 + inner flash; **disabled** (n/a in normal
   play) desat 50%; no selected. *Structure:* frame + "❚❚" glyph. *Feedback:* click → push `11_Pause` modal +
   pause SFX. *(Write: Time.timeScale.)*
2. **HpBar_Left / HpBar_Right** — *purpose:* display each statue's HP (read-only). *States:* not a button — it
   has **value states:** healthy (full faction color), **damaged** (on hit: 0.12s white flash on the fill +
   a "chip" ghost-fill that lags 0.4s before catching down), **critical** (≤15%: fill pulses, faint red edge).
   *Structure:* crest + trough + filled image + numeric text. *Feedback:* purely visual; **no input**.
   *(Read-only from ECS statue HP.)*
3. **ResChip_Gold / Supply / Army** — *purpose:* live readouts of in-battle gold, supply (used/cap), army
   (count/cap). *States:* value states only — **tick-up** (number rolls + brief gold glow on increase),
   **insufficient** (gold chip flashes red when a purchase fails), **cap-reached** (supply/army text turns amber
   when used==cap). *Structure:* icon + tabular number. *Feedback:* non-interactive display. *(Read-only.)*
4. **UnitBtn_0..4 (train tiles)** — *purpose:* enqueue training of that unit for its gold cost
   (`Training.EnqueueTrain`). *States:* **idle** = framed portrait + gold cost; **hover/focus** = frame brighten +
   portrait +5% scale; **pressed** = scale 0.94 + gold spark, cost coin pops; **disabled/unaffordable** =
   `AffordTint` red-desat overlay + `CostText` turns #ff7a6a + raycast still on (tap → "not enough gold" shake);
   **training/cooldown** = `CooldownVeil` radial wipe with a small remaining-time look + dimmed; **selected** =
   (if the design uses tap-to-arm) a gold selection ring — *not shown in this mockup, so omit unless the build
   needs it.* *Structure:* TileFrame, UnitPortrait, CostChip{coin,cost}, CooldownVeil, AffordTint. *Feedback:*
   successful enqueue = coin-spend chime + cost flash + veil starts. *(Write: EnqueueTrain; read gold to gate.)*
5. **Btn_Garrison** — *purpose:* set army stance to **hold/garrison** (pull back to the statue/structure).
   *States:* idle gold-rim; hover brighten; pressed scale 0.94 + flash; **selected/active** = persistent gold
   under-glow + shield icon lit (current stance); disabled desat. *Feedback:* tap → stance change + horn SFX.
   *(Write: issues the corresponding `MoveDestination`/stance order.)*
6. **Btn_Defend** — *purpose:* set **defensive** stance (hold the line at the current front). Same state model;
   **selected** shows the active under-glow. *Feedback:* tap → stance change + SFX. *(Write: MoveDestination.)*
7. **Btn_Attack** — *purpose:* **primary** order — push/advance toward the enemy statue. *States:* idle is the
   brightest of the three (chevrons + label glow); hover brighten further; pressed scale 0.94 + strong gold
   flash; **selected/active** = animated chevrons (subtle forward shimmer) + under-glow; disabled desat.
   *Feedback:* tap → advance order + battle-cry SFX. *(Write: MoveDestination toward enemy.)*

The three order buttons are a **mutually-exclusive stance group** (only one active under-glow at a time).

## I · ANIMATION TIMELINE
**OnShow (HUD slide-in, t in s):**
| t | Element | Action | Dur | Easing |
|---|---|---|---|---|
| 0.00 | Scrim_Top/Bottom | α 0→1 | 0.20 | linear |
| 0.05 | TopBar (pause+HP+chips) | slide down from y+0.04 + α 0→1 | 0.25 | ease-out |
| 0.10 | UnitTray | slide up from y-0.05 + stagger tiles 0.03s each | 0.25 | ease-out |
| 0.14 | OrderCluster | slide up from y-0.05 + α 0→1 | 0.25 | ease-out |
| 0.30 | Btn_Attack | one-shot gold pulse (draw attention to primary) | 0.30 | ease-in-out |

**Continuous/reactive:**
- HP fill change tweens over 0.4s ease-out; **hit-flash** 0.12s white on the fill; critical (≤15%) pulses ±6% at
  ~0.8s.
- Resource numbers **roll** on change (0.3s) with a brief icon glow.
- Train tap: tile scale 0.94→1.0 (0.12s back-out) + cost-coin pop; CooldownVeil radial fills over the unit's
  train time.
- Unaffordable tap: tile X-shake ±4px, 0.18s; CostText red flash.
- Stance change: outgoing button under-glow fades (0.15s), incoming fades in (0.15s); ATTACK chevrons get a
  forward shimmer loop while active.

**OnHide (to result/pause-as-overlay):** for a result screen, HUD chrome slides back to edges + α→0 over 0.20s;
the Pause modal does **not** hide the HUD (it dims it via its own scrim).

## J · PARTICLE & FX (passive — describe only)
- Soft **gold rim-bloom** breathing on the frames; **HP fill** inner-glow.
- **Coin sparkle** on the gold chip when it ticks up.
- **ATTACK** chevrons: faint forward-traveling light streak while the stance is active.
- **Critical HP**: a faint red vignette pulse at the corresponding screen edge (left/right) when that side is low.
- No particles over the battlefield center (keep it clean for the sim's own FX).

## K · EVENT BEHAVIOR
- **OnShow:** router pushes BattleHud over the running ECS world; slide-in (I) plays; the HUD subscribes
  (read-only) to statue HP, gold, supply, army-count. Player: "the battle UI snaps to the edges, field is clear."
- **Per-frame:** poll/observe ECS read-only → update HP fills+text, gold/supply/army chips, and per-tile
  affordability (gate train buttons by current gold) and cooldown veils.
- **OnPause tap:** `Time.timeScale=0`; push Pause modal (HUD stays visible, dimmed by the modal's scrim).
- **OnTrain tap:** if affordable & supply available → `Training.EnqueueTrain(unitId)`; else shake + deny SFX.
- **OnOrder tap (Garrison/Defend/Attack):** set the stance and issue the matching `MoveDestination`; update the
  active under-glow. Player: clear, single-tap army command.
- **Battlefield taps (empty center):** pass through the raycast-off scrims to the sim for selection/move (the HUD
  must not eat them).
- **OnHide:** to a result screen, chrome retracts; bindings released.

## L · NEGATIVE RULES (must-never)
- **Never** place raycast-blocking chrome inside the central battlefield zone x∈[0.20,0.80], y∈[0.20,0.82].
- **Never** let UI write anything beyond `Training.EnqueueTrain`, `MoveDestination`, `Time.timeScale` (§12).
- **Never** swap faction colors: left HP/crest = Iron Pact blue, right = Ashen Horde red.
- **Never** show a train tile as buyable when gold<cost — it must render the unaffordable state (and never
  silently enqueue on a denied tap).
- **Never** have more than one order button show the active under-glow simultaneously.
- **Never** animate HP fill the wrong direction — both deplete **toward the center node**.
- **Never** invent extra HUD widgets (minimap, chat, abilities) not present in this mockup (spells live in the
  separate `09_InMatchSpellHud`).
- **Never** let scrims become opaque enough to hide the battlefield; they are legibility gradients only.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `BattleHudDesign.png`: top dual faction HP bars (blue-left "10,000 / 10,000",
  red-right "9,200 / 10,000") with outer crests + center node; pause top-left; gold "150" + supply "2/8"
  top-left; army "15/50" top-right; 5 unit tiles with costs **60/90/75/120/150** bottom-left; GARRISON/DEFEND/
  ATTACK bottom-right — all placed per §E (±2%).
- **Clear center:** no blocking chrome in the inviolable battlefield rectangle; empty-center taps reach the sim.
- **Hierarchy:** §C tree reproduced; trough→fill→text on bars; tile frame→portrait→cost→veil→tint order.
- **Typography:** exact strings & numbers; tabular numerals; ATTACK is the brightest/heaviest label.
- **States:** train tiles gate on gold (unaffordable state verified); order buttons are a single-select stance
  group; pause sets timeScale 0.
- **Safe area:** pause, army chip, crests inside safeArea on a notched device; scrims pass under the cutout.
- **§12:** only the three permitted writes occur; everything else read-only.

## N · IMPLEMENTATION CONFIDENCE
**88 / 100.** The HUD is structurally deterministic and fully fraction-specified, and it maps cleanly onto the
existing BattleHud bindings (HP, gold, supply, train, stance). The −12: (a) the exact unit-portrait art and crest
devices are assets to author; (b) the precise HP-bar trough/fill 9-slice and the "chip damage lag" need tuning to
match the painted look; (c) the cooldown-veil/affordability behaviors are inferred from RTS convention (the
static mockup can't show them) and must be reconciled with the real `Training` API.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O all present, in order, substantive.
- □ Every number captured: HP 10,000/10,000 & 9,200/10,000; gold 150; supply 2/8; army 15/50; costs 60/90/75/120/150.
- □ Faction sides locked (blue=left, red=right); both fills deplete toward center.
- □ Central battlefield kept clear; scrims raycast-off so sim taps pass through.
- □ Only §12 writes used; everything else read-only.
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ States for pause / train tiles / order group fully specified.
- □ Hex + materials for gold/HP fills/crests/portraits/scrims.
- □ Header + Source line in required format.
