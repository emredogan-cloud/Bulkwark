# BULWARK — UI CONSTRUCTION SPEC · 15 · Endless Result

Source: design/EndlessResultDesign.png · 1672×941 (≈1.78:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas; geometry is **fractions of 2340×1080**. This is a **POST-MATCH RESULT** screen shown when an **Endless / survival** run ends (the player is finally overrun). Waves-survived, score, best-flag, and rewards originate from ECS run-state (read-only) + server-authoritative best-score + reward grant (display-only). **No code here** — construction spec only.

---

## A · SCREEN PURPOSE
The **Endless Result** screen closes out a survival run. Endless has no "win" — the run always ends in being overrun — so the framing is **"how far did you get?"** rather than victory/defeat. It must (1) headline the run outcome with the thematic title **"THE HORDE PREVAILS"** (the Ashen Horde always wins eventually), (2) show the two performance numbers that define an endless run — **Waves Survived** (the hero stat, big ember number) and **Score** — plus a **"NEW BEST!"** celebration ribbon when the score is a personal record, (3) grant run **rewards** (coins + battle-pass XP), and (4) offer **RETRY** (run again immediately — the dominant endless loop) and **MAIN MENU** (exit to hub). Tone is dark, ember/oxblood, ominous-but-rewarding (a horde-mode aesthetic). Source of truth: ECS run-state (waves, score) + server best/reward.

State machine position: `Endless run → Endless Result → {RETRY → new run | MAIN MENU → hub}`.

---

## B · VISUAL DNA (screen-specific)
- **Theme = ASHEN HORDE / EMBER + OXBLOOD on near-black.** This screen wears the **enemy faction's** colors because the horde "prevails." Palette: near-black charcoal panel, **oxblood-red** draped banners, **ember-orange/red** for the hero number and title, cold steel for chrome, gold reserved for the small "NEW BEST!" ribbon and reward frames. It is the darkest, most ominous of the result screens.
- **Frame = SPIKED IRON, not smooth gold.** The panel frame is a darker, **jagged/spiked black-iron** ornament (cruel, horde-themed) with subtle ember-red glints and a **jagged crown-spike crest** at the top center (a tribal/horde sigil rather than a regal crown).
- **Banners:** **oxblood-red cloth war-banners** draped over the top-left and top-right corners (the Ashen Horde's heraldry), with dark/iron trim — the red counterpart to Campaign's blue Iron-Pact banners.
- **Hero number = WAVES SURVIVED in giant EMBER.** "Waves Survived:" label above a very large **ember-orange/red** number (`#db2b01`, e.g. `34`) — the single biggest, hottest element on the panel, glowing.
- **Score + NEW BEST:** "Score:" label + a large cream/white number (`145,200`); when it is a record, a **gold ribbon banner reading "NEW BEST!"** (flanked by small gold crowns) sits just beneath the score, glowing — the only warm-gold celebratory element.
- **Rewards:** two dark gold-framed slot tiles — left **silver coins**, right a **battle-pass "XP" badge** (a blue shield with gold wings + "XP") labelled `Pass XP` + amount.
- **CTAs:** **RETRY** (oxblood-red, primary — the dominant "run again" endless loop) and **MAIN MENU** (dark stone/iron, secondary).
- **Background:** a dark, ember-lit ruined battlefield / horde encampment, near-black with faint red rim-glows and embers, heavily vignetted; pushed far back.
- **Mood:** ominous, intense, "you held the line a long time, now run it again." High contrast: black field → hot ember number → red CTA.

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
EndlessResultScreen (UiScreen root; CanvasGroup)
└── FullBleedRoot (stretch-stretch, ignores safe area)
    ├── BG_HordeField (Image — dark ember-lit ruined battlefield, full-bleed)
    ├── BG_Vignette (Image — heavy radial vignette → near-black edges)
    ├── FX_EmberDrift (ParticleSystem — slow rising red/orange embers, background)
    └── BG_DarkenScrim (Image — black ~0.45)
    SafeAreaRoot (RectTransform — Screen.safeArea inset)
    └── ResultPanel (Container; anchored center; ~0.50w × ~0.90h)
        ├── Banner_Left (Image — oxblood war-banner, draped top-left)
        ├── Banner_Right (Image — oxblood war-banner, draped top-right)
        ├── Panel_Frame (Image — spiked black-iron 9-slice frame, ember glints)
        │   └── Panel_Interior (Image — near-black charcoal fill, faint red inner glow)
        ├── Crest_Spike (Image — jagged crown-spike horde sigil, top-center, breaks frame)
        ├── Title_Horde (Text "THE HORDE PREVAILS" — oxblood/ember serif)
        ├── Waves_Group
        │   ├── Waves_Label (Text "Waves Survived:")
        │   └── Waves_Value (Text "34" — giant EMBER)
        ├── Score_Group
        │   ├── Score_Label (Text "Score:")
        │   └── Score_Value (Text "145,200" — cream)
        ├── NewBest_Ribbon (Container — gold banner; shown only on record)
        │   ├── Ribbon_CrownL (Image — small gold crown)
        │   ├── Ribbon_Banner (Image — gold ribbon)
        │   ├── NewBest_Text (Text "NEW BEST!")
        │   └── Ribbon_CrownR (Image — small gold crown)
        ├── Rewards_Group
        │   ├── Rewards_Label (Text "Rewards")
        │   ├── Reward_Slot_Coins (Container — gold-framed dark tile)
        │   │   ├── Slot_Frame (Image)
        │   │   ├── Icon_Coins (Image — silver coin stack)
        │   │   └── Coins_Value (Text "12,450")
        │   └── Reward_Slot_XP (Container — gold-framed dark tile)
        │       ├── Slot_Frame (Image)
        │       ├── Icon_XPBadge (Image — blue shield + gold wings + "XP")
        │       ├── XP_SubLabel (Text "Pass XP")
        │       └── XP_Value (Text "2,350")
        └── Buttons_Group (anchored bottom)
            ├── CTA_Retry (Button — oxblood-red, iron/gold frame)
            │   ├── Retry_Frame (Image)
            │   ├── Retry_Fill (Image — red gradient + inner red rim)
            │   └── Retry_Label (Text "RETRY")
            └── CTA_MainMenu (Button — dark stone/iron, frame)
                ├── Menu_Frame (Image)
                ├── Menu_Fill (Image — dark stone gradient)
                └── Menu_Label (Text "Main Menu")
```

---

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| EndlessResultScreen | Canvas | — | UiScreen+CanvasGroup | stretch | .5,.5 | — | root | fade |
| FullBleedRoot | screen | 0 | RectTransform | stretch | .5,.5 | — | ignores | full-bleed |
| BG_HordeField | FullBleedRoot | 0 | Image | stretch | .5,.5 | center-crop | no | AspectFill |
| BG_Vignette | FullBleedRoot | 1 | Image | stretch | .5,.5 | center | no | radial, heavy |
| FX_EmberDrift | FullBleedRoot | 2 | ParticleSystem | stretch | .5,.5 | — | no | screen-space-ish, behind scrim |
| BG_DarkenScrim | FullBleedRoot | 3 | Image | stretch | .5,.5 | — | no | α0.45 |
| SafeAreaRoot | screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | applies | insets |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center | .5,.5 | center | yes | width=min(0.50·W, height-cap) |
| Banner_Left/Right | ResultPanel | 0,1 | Image | top-left / top-right | 0,1 / 1,1 | — | yes | draped corners, mirror |
| Panel_Frame | ResultPanel | 2 | Image (9-slice) | stretch | .5,.5 | — | yes | spiked iron 9-slice |
| Panel_Interior | Panel_Frame | 0 | Image (9-slice) | stretch | .5,.5 | — | yes | inset; charcoal |
| Crest_Spike | ResultPanel | 3 | Image | top/center | .5,1 | center | yes | breaks top frame, top z |
| Title_Horde | ResultPanel | 4 | Text (TMP) | top/center | .5,1 | center | yes | auto-size capped |
| Waves_Label | ResultPanel | 5 | Text (TMP) | top/center | .5,1 | center | yes | small caps |
| Waves_Value | ResultPanel | 6 | Text (TMP) | top/center | .5,1 | center | yes | giant ember, auto-size capped |
| Score_Label | ResultPanel | 7 | Text (TMP) | center/center | .5,1 | center | yes | small caps |
| Score_Value | ResultPanel | 8 | Text (TMP) | center/center | .5,1 | center | yes | cream, tabular |
| NewBest_Ribbon | ResultPanel | 9 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | crown+banner+crown; conditional |
| Ribbon_CrownL/R | NewBest_Ribbon | 0,3 | Image | center/center | .5,.5 | center | yes | small gold crowns |
| Ribbon_Banner | NewBest_Ribbon | 1 | Image | center/center | .5,.5 | center | yes | gold ribbon (behind text) |
| NewBest_Text | Ribbon_Banner | 0 | Text (TMP) | stretch | .5,.5 | center | yes | on ribbon |
| Rewards_Label | ResultPanel | 10 | Text (TMP) | center/center | .5,1 | center | yes | small caps |
| Rewards_Group | ResultPanel | 11 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | two tiles + gap |
| Reward_Slot_Coins / _XP | Rewards_Group | 0,1 | RectTransform | — | .5,.5 | center | yes | each ≈0.30 panel width |
| Slot_Frame | slot | 0 | Image (9-slice) | stretch | .5,.5 | — | yes | gold frame |
| Icon_Coins / Icon_XPBadge | slot | 1 | Image | top/center | .5,1 | center | yes | icon upper area |
| Coins_Value | coins slot | 2 | Text (TMP) | bottom/center | .5,0 | center | yes | currency glyph + number |
| XP_SubLabel | xp slot | 2 | Text (TMP) | bottom/center | .5,0 | center-left | yes | "Pass XP" small |
| XP_Value | xp slot | 3 | Text (TMP) | bottom/center | .5,0 | center-right | yes | number |
| Buttons_Group | ResultPanel | 12 | HorizontalLayoutGroup | bottom/center | .5,0 | middle-center | yes | Retry + MainMenu; above bottom frame |
| CTA_Retry | Buttons_Group | 0 | Button | — | .5,.5 | center | yes | ≈0.42 panel width |
| Retry_Frame/Fill/Label | CTA_Retry | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | red CTA |
| CTA_MainMenu | Buttons_Group | 1 | Button | — | .5,.5 | center | yes | ≈0.42 panel width |
| Menu_Frame/Fill/Label | CTA_MainMenu | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | dark-stone CTA |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
W=2340, H=1080, match HEIGHT.

**Background:** BG_HordeField 0,0→1,1 AspectFill; BG_Vignette heavy → near-black corners; FX_EmberDrift full-field behind scrim; BG_DarkenScrim α≈0.45.

**ResultPanel:** width ≈ **0.50·W ≈ 1170 px**, centered. Height ≈ **0.90·H ≈ 972 px**, centered. Aspect ≈ 1.20:1. Cap: width = `min(0.50·W, 1.25·panelHeight)`. (Taller card than Campaign because it stacks more rows: title, waves, score, best, rewards, buttons.)

**Banners:** oxblood banners anchored to top-left/top-right corners (pivot 0,1 / 1,1), width ≈ 0.11·panelW, hang down ≈ 0.50·panelH, overlapping frame and draping slightly outside.

**Internal vertical rhythm (fractions of PANEL height, 0=panel top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Crest spike (apex above frame) | ~ -0.03 to 0.05 | 0.12 |
| Title "THE HORDE PREVAILS" | 0.14 | 0.09 |
| "Waves Survived:" label | 0.25 | 0.05 |
| Waves value `34` (giant) | 0.34 | 0.13 |
| "Score:" label | 0.44 | 0.04 |
| Score value `145,200` | 0.50 | 0.06 |
| "NEW BEST!" ribbon | 0.57 | 0.06 |
| "Rewards" label | 0.65 | 0.04 |
| Reward slot tiles | 0.77 | 0.16 |
| Buttons (Retry/MainMenu) | 0.91 | 0.10 |

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.78, centered.
- Waves value centered; very large (auto-size up to ≈ 0.13·panelH cap).
- NEW BEST ribbon width ≈ 0.46·panelW centered, with small crowns at its ends (~0.05·panelW each).
- Rewards row width ≈ 0.66; each tile ≈ 0.30·panelW × 0.14·panelH, gap ≈ 0.04.
- Buttons row width ≈ 0.84; RETRY ≈ 0.42·panelW (left), MAIN MENU ≈ 0.42·panelW (right), gap ≈ 0.02; button height ≈ 0.085·panelH. (No center emblem divider on this variant — the buttons sit closer, simple gap.)

**Notch/safe-area:** panel centered in safe rect; field bleeds under cutout. **Tablet/ultrawide:** cap keeps the card proportionate; ultrawide reveals more ember battlefield.

---

## F · TYPOGRAPHY (per text)
Sizes px@1080. Dark panel → mostly **light/ember text**.

| Text | Content | Family / personality | Weight | Caps | Tracking | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|
| Title_Horde | `THE HORDE PREVAILS` | serif display, ominous/regal-cruel | Bold/ExtraBold | ALL-CAPS | +5% | oxblood/ember gradient with low red glow + faceted bevel | ~52–58 | gradient `#d8452b` (top) → `#7a1f1a` (bottom); cores `#f08050` | dark stroke `#2a0a06` ~2.5px + shadow `#000` α0.65 (0,3) + faint red outer glow α0.3 |
| Waves_Label | `Waves Survived:` | clean serif, label | Medium | Title-case (as shown) | +2% | subtle | ~30–34 | cool parchment `#d8cfbc` | shadow α0.5 (0,2) |
| Waves_Value | `34` | heavy condensed display, the hero number | Black | n/a | 0% | **strong ember glow**, faceted, bloom | ~150–170 (auto-size; the single biggest glyph on screen) | hot ember `#db2b01` (cores `#ff6a2a`, deep edges `#8a1c0a`) | dark stroke `#2a0a04` ~3px + strong red/orange outer glow α0.55 + shadow α0.6 (0,4) |
| Score_Label | `Score:` | clean serif, label | Medium | Title-case (as shown) | +2% | subtle | ~28–30 | cool parchment `#d8cfbc` | shadow α0.5 (0,2) |
| Score_Value | `145,200` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp, faint glow | ~52–58 | cream/white `#f3ead2` | shadow `#000` α0.6 (0,3) |
| NewBest_Text | `NEW BEST!` | serif display, celebratory | Bold | ALL-CAPS | +6% | gold, bright, bloom | ~30–34 | warm gold `#f0d27a` (cores `#fff0c0`) | dark stroke `#3a2a08` ~2px + gold glow α0.4 + shadow α0.5 (0,2) |
| Rewards_Label | `Rewards` | serif small-caps, label | SemiBold | small-caps (Title-case shown) | +8% | engraved gold | ~24–26 | antique gold `#c8a55a` | inner dark, low glow |
| Coins_Value | `12,450` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp on dark tile | ~32–36 | warm white `#f3ead2` | shadow α0.6 (0,2) |
| XP_SubLabel | `Pass XP` | small caps, label | Medium | small-caps (Title-case shown) | +4% | muted | ~20–22 | muted gold/grey `#b8a878` | shadow α0.4 (0,1) |
| XP_Value | `2,350` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp | ~30–34 | warm white `#f3ead2` | shadow α0.6 (0,2) |
| Retry_Label | `RETRY` | serif display, urgent | Bold | ALL-CAPS | +8% | bright, faint red glow | ~38–42 | warm white `#fff3ec` | dark-red stroke `#3a0a06` ~2px + shadow α0.6 (0,3) |
| Menu_Label | `Main Menu` | serif display | Bold | Title-case (as shown) | +4% | bright on dark stone | ~36–40 | warm white `#f3ecdc` | dark stroke `#1a140a` ~2px + shadow α0.6 (0,3) |

The `34` waves value is the typographic hero — by far the largest, hottest, most-bloomed glyph; everything else is subordinate.

---

## G · MATERIALS
- **Panel interior:** charcoal/obsidian `#0a0b0f`–`#16120f` (slightly warm-black from ember light), matte; a **faint red inner glow** at the bottom/center (`#3a1410` α≈0.15) — the horde-fire ambiance.
- **Spiked iron frame + crest spike:** dark cast iron `#1a1814`–`#2a241c` with worn metal highlights `#5a5048` and **ember-red glints** `#b8401c` catching the spike tips; jagged/cruel silhouette (not smooth gold). The crest spike is a tribal/horde sigil (jagged crown of blades/horns).
- **Oxblood banners:** deep red cloth `#5a1410`–`#8a241c` with dark iron trim and a torn/ragged hem; subtle cloth folds + drop shadow; an Ashen sigil (jagged emblem) in darker red/black.
- **Waves ember number:** hot ember/lava material — gradient `#ff6a2a` (cores) → `#db2b01` → `#8a1c0a` (deep), with bloom and a faint heat-shimmer; the brightest text element.
- **Score number:** cream metal `#f3ead2`, subtle bevel, low glow.
- **NEW BEST ribbon:** brushed gold banner `#caa04a`–`#f0d27a` with bloom (the lone warm-gold celebratory chrome), small gold crowns at the ends; gentle shimmer.
- **Reward slot tiles:** dark recessed interior `#130f0a` inside gold bevel frames `#af874e`; inner shadow.
- **Coin icon:** silver/steel coin stack `#9aa0a8`/`#5a6068`. **XP badge:** royal-blue heraldic shield `#1f3e78`–`#2b56c8` with **gold wings** `#caa04a` and a gold `XP` monogram, soft glow (battle-pass identity, deliberately blue/gold to read as "progression," contrasting the red field).
- **RETRY fill (oxblood-red):** gradient `#5a1410`→`#b84130`, inner red rim-glow `#e0604a` α≈0.45; satin; the primary CTA (matches the horde theme — running again is the loop).
- **MAIN MENU fill (dark stone):** muted iron/stone `#2a2620`→`#3a342a`, faint top highlight; secondary/matte. Frame is the same iron/gold as the panel.
- **Embers (background):** drifting red/orange motes `#ff5a1e`→transparent.
- **Bloom budget:** the `34` ember number first, then the NEW BEST ribbon, the XP badge, and the RETRY top edge; the rest matte. Endless is intentionally the **least gold-bloomed, most red-glowing** result screen.

---

## H · COMPONENTS (states + feedback)
**Waves/Score values (display, animated):** count-up (see §I); the waves number has a heavier "impact" settle.

**NewBest_Ribbon (conditional display):** only instantiated/shown when `isNewBest == true`. On reveal it pops in with a gold flare + a triumphant sting; persistent gentle shimmer + crown glints. If not a record, it is **absent** (not greyed) and the layout closes the gap.

**Reward slots (display):** non-interactive; numbers count-up; brief scale-pop + shimmer on reveal.

**CTA_Retry (oxblood-red primary):**
- **Idle:** red gradient, iron/gold frame, inner red rim, white label; subtle slow glow (the encouraged "run again").
- **Hover:** frame +8%, glow +25%, scale →1.03.
- **Pressed:** scale →0.97, fill darkens ~10%, red flash, "war-drum/blade" SFX.
- **Disabled:** N/A (retry always available).
- **Confirm:** red ring flash → new run.

**CTA_MainMenu (dark-stone secondary):**
- **Idle:** dark-stone fill, frame, white label; no glow pulse.
- **Hover:** frame +8%, slight lighten, scale →1.03.
- **Pressed:** scale →0.97, click SFX.
- **Confirm:** soft flash → hub.

**Focus order:** default selected = **RETRY** (the dominant endless loop). Back/B → MAIN MENU (exit to hub). D-pad cycles Retry ↔ Main Menu.

---

## I · ANIMATION TIMELINE
Endless reveal is **dark, building, ominous** — the ember number "ignites," and NEW BEST is the gold payoff.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.35 | FullBleedRoot CG | fade 0→1 (ember field + scrim) | linear |
| 0.05 | loop | FX_EmberDrift | embers begin rising | — |
| 0.15 | 0.40 | ResultPanel | scale 0.9→1.0 + α 0→1 | back-ease-out (slight overshoot) |
| 0.25 | 0.30 | Banner_Left/Right | unfurl from corners (cloth settle) | ease-out |
| 0.35 | 0.30 | Crest_Spike | drop/settle onto top frame (slight downward weight) | ease-out |
| 0.45 | 0.45 | Title_Horde | α 0→1 + scale 1.06→1.0; low red glow rises then settles | ease-out |
| 0.85 | 0.30 | Waves_Label | α 0→1 + slide up 8px | ease-out |
| 1.10 | 0.80 | Waves_Value | **count-up 0 → 34** with rising ember glow that **peaks then settles** on the final value; one heavy "impact" + ember burst on land | ease-out (fast→slow) |
| 1.55 | 0.25 | Score_Label | α 0→1 | ease-out |
| 1.70 | 0.80 | Score_Value | **count-up 0 → 145,200** (tabular, tick SFX) | ease-out |
| 2.45 | 0.35 | NewBest_Ribbon | (if record) pop in: scale 0.7→1.0 + gold flare + triumphant sting + crown glints | back-ease-out |
| 2.75 | 0.25 | Rewards_Label | α 0→1 | ease-out |
| 2.90 | 0.30 | Reward_Slot_Coins | tile pop + shimmer | back-ease-out |
| 2.95 | 0.70 | Coins_Value | **count-up 0 → 12,450** | ease-out |
| 3.00 | 0.30 | Reward_Slot_XP | tile pop + badge shimmer | back-ease-out |
| 3.05 | 0.60 | XP_Value | **count-up 0 → 2,350** | ease-out |
| 3.35 | 0.30 | CTA_Retry | α 0→1 + scale 0.94→1.0 + glow ignite | back-ease-out |
| 3.40 | 0.30 | CTA_MainMenu | α 0→1 + scale 0.94→1.0 | ease-out |
| 3.70 | loop | CTA_Retry glow | breathing pulse (period ~1.8 s) | sine in-out |
| 3.70 | loop | NewBest_Ribbon | gentle gold shimmer + crown glints (period ~2.5 s) | sine in-out |
| 3.70 | loop | Waves_Value | faint ember heat-shimmer on the glyph | noise |

**OnRetry (exit):** press dip → red ring flash → panel α→0 + scale→0.96 + bg fade → new run. **OnMainMenu:** soft flash → hub. 
**Skip rule:** tap during reveal snaps all count-ups + the NEW BEST pop to end-state and enables both CTAs. If a record, the NEW BEST ribbon still appears (snapped) on skip.

---

## J · PARTICLE & FX (passive)
- **FX_EmberDrift (background):** slow rising red/orange embers across the whole field, low opacity, behind the scrim — the horde-fire ambiance.
- **Waves ember glow + heat-shimmer:** persistent strong ember bloom + subtle noise-driven shimmer on the `34`.
- **NEW BEST shimmer + crown glints:** persistent gentle gold shimmer along the ribbon and occasional sparkle on the crowns.
- **XP badge glow:** soft blue/gold inner glow + occasional wing glint.
- **Coin glint:** occasional silver sweep on the coin stack.
- **Red inner panel glow:** faint, static, bottom-weighted.
- **RETRY glow:** persistent breathing red rim (see §I).

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload = `{ wavesSurvived:int, score:int, isNewBest:bool, coins:int, passXP:int }` from ECS run-state + server best/reward (all **display-only**). Title is the fixed string `THE HORDE PREVAILS` (endless always ends overrun). Run the §I reveal. NEW BEST ribbon shown iff `isNewBest`.
- **OnReward (count-ups):** waves → `34`, score → `145,200`, coins → `12,450`, passXP → `2,350`. All display-only; the best-score and rewards are server-authoritative (§12) — the UI never writes them. Battle-pass XP grant is also server-side; this only shows the earned amount.
- **OnRetry:** start a new endless run (the dominant loop). Debounced. Issues only a new-run request — no balance mutation.
- **OnMainMenu:** route to hub. Debounced.
- **OnBack (B/Esc):** aliased to MAIN MENU.
- **Idempotency / re-entry:** settles to end-state immediately; no re-grant/re-count; NEW BEST reflects the stored record.
- **No mutation:** navigation + (Retry) new-run request only; never edits balance/best/XP client-side.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** frame this as "Victory" or "Defeat" — the fixed title is `THE HORDE PREVAILS`; endless has no win/loss verdict, only "how far." 
2. Do **not** demote the `Waves Survived` number — it is the hero glyph: the largest, hottest, most-bloomed ember element. 
3. Do **not** show the **NEW BEST!** ribbon when `isNewBest == false` — omit it and close the gap (never grey it). 
4. Do **not** use blue/gold as the dominant theme — this screen is **Ashen ember/oxblood on near-black** (gold only on NEW BEST + reward frames; blue only on the XP badge). 
5. Do **not** swap reward identities — left = silver coins, right = battle-pass **XP** badge (`Pass XP`). 
6. Do **not** make MAIN MENU compete with RETRY — RETRY is the red primary; MAIN MENU is dark/secondary/matte. 
7. Do **not** use a smooth regal gold crown crest — the crest is a **jagged horde spike** sigil; the frame is spiked iron, not smooth gold. 
8. Do **not** put interactive content under the notch; only the field bleeds there. 
9. Do **not** mutate balance/best/XP client-side (§12); values are display-only. 
10. Do **not** stretch frame/banner ornament (9-slice fixed). 
11. Do **not** use portrait or assume width is stable; match HEIGHT, fraction-based.

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] Dark, ember-lit, heavily vignetted full-bleed horde battlefield with drifting embers; near-black charcoal panel with a **spiked black-iron frame** and a **jagged crown-spike crest** at the top center.
- [ ] **Oxblood-red war-banners** draped over the top-left and top-right corners.
- [ ] Title reads exactly `THE HORDE PREVAILS` in an oxblood/ember serif with a low red glow.
- [ ] `Waves Survived:` label above a **giant ember `34`** that is the single largest, hottest, most-bloomed glyph on screen.
- [ ] `Score:` label + `145,200` in cream; a **gold `NEW BEST!` ribbon flanked by small gold crowns** beneath it (shown only on a record).
- [ ] `Rewards` label + two gold-framed dark tiles: left **silver coin stack + `12,450`**, right **blue/gold winged XP badge + `Pass XP` + `2,350`**.
- [ ] Two buttons: **RETRY** (oxblood-red primary, glowing) and **Main Menu** (dark-stone secondary).
- [ ] Reveal builds in order (panel → banners → crest → title → waves count-up w/ ember peak → score count-up → NEW BEST pop → rewards + count-ups → CTAs); tap-to-skip resolves all values + NEW BEST.
- [ ] RETRY shows idle/hover/pressed + breathing glow and starts a new run; Main Menu routes to hub; back → Main Menu.
- [ ] Field bleeds under the notch; content respects safe area; fraction-based, match-height.
- [ ] No client balance/best/XP mutation; values and NEW BEST are data-driven.
- [ ] Side-by-side with `EndlessResultDesign.png`, positions within ±2% of panel dims and colors within stated ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**90 / 100.** Copy, the two-number performance structure, the conditional NEW BEST, and the dual rewards are all directly legible, so structural fidelity is high. Deductions: (a) the **giant ember number with an ignite/peak/settle count-up + heat-shimmer** is a distinctive effect that needs careful tuning to match the hero treatment (~ -3); (b) bespoke horde art — spiked iron frame, jagged crown-spike crest, oxblood banners with Ashen sigil, winged XP badge — must be authored/sourced (~ -4); (c) the conditional NEW BEST presentation + battle-pass XP semantics (display vs grant timing) are flow details that depend on data the spec infers from one example (~ -3). No structural ambiguity in the shown state.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/EndlessResultDesign.png`, 1672×941) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header has number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed stated.
- [x] Verbatim labels recorded (`THE HORDE PREVAILS`, `Waves Survived:`, `34`, `Score:`, `145,200`, `NEW BEST!`, `Rewards`, `12,450`, `Pass XP`, `2,350`, `RETRY`, `Main Menu`) — nothing invented.
- [x] Forensic hex ranges from the art (ember `#db2b01`, oxblood, near-black, gold ribbon).
- [x] Full ASCII tree + per-node Unity table.
- [x] Ominous build reveal with ember count-up + conditional NEW BEST + tap-to-skip.
- [x] §12 boundary honored (display-only; no balance/best/XP mutation).
- [x] Negative rules + ≥95% acceptance + confidence rationale.
- [x] No code/asset/scene/prefab/gameplay changes, no commit.
