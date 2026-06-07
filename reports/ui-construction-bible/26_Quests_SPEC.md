# BULWARK — UI CONSTRUCTION SPEC · 26 · Quests

Source: design/QuestsScreenDesign.png · 1754×897 (1.96:1) · Analysis-only forensic spec.

> Normalize the source 1754×897 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" = on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Quests** meta/live-ops screen — **"DAILY QUESTS"** on a large parchment scroll panel. Layout:
1. **Header:** title "DAILY QUESTS", a **reset timer "Reset In: 13h 46m 21s"** (with chest/timer icon).
2. **Tabs:** **Daily** (selected, blue, with a red unread dot) · **Weekly**.
3. **Quest list:** **5 quest rows**, each = circular emblem icon · name + one-line description · a progress bar
   with "current/target" · a reward chip (gem or coin + quantity) · a **CLAIM** button. The five rows:
   - **Win 3 Battles** — "Win any 3 battles in Campaign or Multiplayer." — 3/3 — **50 [gem]** — CLAIM
   - **Train 50 Units** — "Train a total of 50 units." — 32/50 — **2000 [coin]** — CLAIM
   - **Open 1 Silver Chest** — "Open 1 Silver Chest." — 1/1 — **30 [gem]** — CLAIM
   - **Deal 10,000 Damage** — "Deal a total of 10,000 damage in battles." — 7,250/10,000 — **1500 [coin]** — CLAIM
   - **Log In Daily** — "Log in to the game." — 1/1 — **20 [gem]** — CLAIM
4. **Footer:** **"Complete all Daily Quests to earn bonus rewards!"** with a completion meter **"4/5"** and a
   bonus chest icon.

Purpose: present daily/weekly objectives, show progress, and let the player claim per-quest rewards plus a
"complete all" bonus. Server-authoritative meta. Reached from: Main Menu rail. Back: top-left chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** a war-camp quest board on aged parchment — the **lightest-field screen** in the set. Warm cream
  scroll over a dark dusk battlefield; gold frame; blue CTAs pop against the parchment.
- **Palette anchors:**
  - BG (behind scroll): dark battlefield dusk `#0a0b0f → #1c1510`, faint banner on the right; vignette.
  - **Parchment panel:** aged cream `#d9c79a → #c3ad79`, darker edges `#9c855a`, subtle fiber texture, gold
    rolled-edge frame.
  - Gold chrome (frame, title, dividers, row plates): `#6b5320 → #caa04a → #f0d27a`.
  - **CLAIM CTA + selected tab:** cobalt `#2b56c8 → #4f8bff` with gold trim.
  - Reward chips: gems violet crystal `#9e6bf0`, coins gold `#f0c14a`.
  - Progress bar fill: warm gold/green over a dark groove; completed = full bright.
  - Row text on parchment: dark brown `#3a2e1c` (names) / `#5a4a30` (descriptions).
- **Hierarchy:** DAILY QUESTS title → quest rows → CLAIM buttons → reset timer → tabs → footer bonus.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
QuestsScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_BattlefieldDusk (Image, dark vista + faint banner right)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ ScrollPanel (Image, parchment + gold rolled-edge frame)  ← main container
│  │  ├─ Header
│  │  │  ├─ Btn_Back (Button) [chevron] top-left (over/near frame)
│  │  │  ├─ Lbl_Title "DAILY QUESTS" (Text, serif gold) top-center
│  │  │  └─ ResetTimer [ icon | "Reset In:" | "13h 46m 21s" ] top-right
│  │  ├─ Tabs (HorizontalLayoutGroup, 2, centered)
│  │  │  ├─ Tab_Daily (Toggle, SELECTED) "Daily" + RedDot(unread)
│  │  │  └─ Tab_Weekly (Toggle) "Weekly"
│  │  ├─ QuestList (ScrollRect vertical / or VerticalLayoutGroup, 5 rows)
│  │  │  ├─ QuestRow_Win3
│  │  │  ├─ QuestRow_Train50
│  │  │  ├─ QuestRow_OpenChest
│  │  │  ├─ QuestRow_Deal10k
│  │  │  └─ QuestRow_Login
│  │  │     (each QuestRow:)
│  │  │     ├─ Row_Plate (Image, inset parchment/wood plate)
│  │  │     ├─ Icon_Emblem (Image, circular gold-rimmed quest icon)
│  │  │     ├─ Lbl_QuestName (Text, e.g. "Win 3 Battles")
│  │  │     ├─ Lbl_QuestDesc (Text, e.g. "Win any 3 battles in Campaign or Multiplayer.")
│  │  │     ├─ ProgressBar (Slider) [Fill + Lbl "3 / 3"]
│  │  │     ├─ RewardChip [ Icon (gem/coin) | Qty (e.g. "50") ]
│  │  │     └─ Btn_Claim (Button, cobalt) "CLAIM"
│  │  └─ Footer
│  │     ├─ Lbl_BonusText "Complete all Daily Quests to earn bonus rewards!"
│  │     ├─ BonusMeter [ Fill + Lbl "4/5" ]
│  │     └─ Icon_BonusChest (Image)
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | grows |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| ScrollPanel | SafeAreaRoot | 0 | Image(framed) | center (near-stretch) | .5,.5 | — | inside | centered panel, max width clamp |
| Header | ScrollPanel | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans panel |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| Lbl_Title | Header | 1 | Text | top-center | .5,1 | center | inside | centered |
| ResetTimer | Header | 2 | RectTransform(icon+2 Text) | top-right | 1,1 | right | inside | right-pinned |
| Tabs | ScrollPanel | 1 | HorizontalLayoutGroup | top-center (below header) | .5,1 | center | inside | centered pair |
| Tab_Daily / Tab_Weekly | Tabs | 0,1 | Toggle (group) | — | — | mid | inside | equal pill |
| QuestList | ScrollPanel | 2 | ScrollRect(vert) / VerticalLayoutGroup | center-stretch | .5,.5 | top | inside | spans panel width |
| QuestRow_* | QuestList content | 0..4 | RectTransform | stretch-x | .5,1 | mid | inside | full width, uniform height |
| Row_Plate | QuestRow | 0 | Image | stretch | .5,.5 | — | inside | scales |
| Icon_Emblem | QuestRow | 1 | Image | left | 0,.5 | center | inside | fixed |
| Lbl_QuestName | QuestRow | 2 | Text | left (right of icon) | 0,1 | left | inside | flex |
| Lbl_QuestDesc | QuestRow | 3 | Text | left (below name) | 0,1 | left | inside | flex |
| ProgressBar | QuestRow | 4 | Slider(no handle)+Text | left (below desc) | 0,0 | mid | inside | flex width |
| RewardChip | QuestRow | 5 | RectTransform(icon+Text) | right (left of CLAIM) | 1,.5 | center | inside | fixed |
| Btn_Claim | QuestRow | 6 | Button | right | 1,.5 | center | inside | fixed CTA |
| Footer | ScrollPanel | 3 | RectTransform | bottom-stretch | .5,0 | center | inside | spans panel |
| Lbl_BonusText | Footer | 0 | Text | bottom-left/center | .5,0 | center | inside | — |
| BonusMeter | Footer | 1 | Slider(no handle)+Text | bottom-center | .5,0 | center | inside | — |
| Icon_BonusChest | Footer | 2 | Image | bottom-right | 1,0 | center | inside | fixed |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**ScrollPanel (main parchment container):** x 0.080 → 0.920 (width ≈0.840w ≈1966px@1080), y 0.060 → 0.945
(height ≈0.885h ≈956px@1080). Centered; gold rolled-edge frame ~16px; the BG extends full-bleed behind it.

**Header (within panel):** y 0.870 → 0.940 (panel-relative top band).
- Btn_Back: just outside/at the panel's top-left, center x 0.060, y 0.915; ≈0.045w×0.085h.
- Lbl_Title "DAILY QUESTS": centered x 0.50, baseline y 0.910; cap ≈0.052h (≈56px@1080), wide tracking, gold.
- ResetTimer: right, x≈0.880, y 0.910; chest/timer icon + "Reset In:" small over/before "13h 46m 21s"
  (≈24px tabular).

**Tabs:** centered, y 0.795 → 0.855. Two pills each ≈0.150w × 0.055h (≈351×59@1080), gap 0.010w, centered
about x 0.50. Daily (left) selected = cobalt fill + bright label + **red unread dot** at its top-right corner;
Weekly (right) idle parchment/gold.

**Quest list:** region x 0.110 → 0.890 (inner panel width ≈0.780w ≈1825px@1080), y 0.150 → 0.780
(height ≈0.630h ≈680px@1080). **5 rows**, each ≈0.118h tall (≈127px@1080), row gap ≈0.008h.
- **Within a row** (left→right):
  - Icon_Emblem: circular gold-rimmed emblem, x-center ≈0.150, Ø≈0.075h (≈81px@1080).
  - Lbl_QuestName: x start ≈0.205, top of the text block, ≈28px dark-brown.
  - Lbl_QuestDesc: x start ≈0.205, below name, ≈20px lighter brown.
  - ProgressBar: x ≈0.205 → 0.560, below desc (y ≈ row-bottom + 0.020), height ≈0.022h (≈24px); dark groove
    with warm-gold fill; centered text "3 / 3" (or "32 / 50", "7,250 / 10,000"); filled bars (3/3, 1/1) read
    full/bright.
  - RewardChip: x-center ≈0.640, gem/coin icon (Ø≈0.030w) + quantity text (≈26px) — "50", "2000", "30",
    "1500", "20".
  - Btn_Claim: right, x 0.760 → 0.870 (width ≈0.110w ≈257px), height ≈0.070h (≈76px); cobalt "CLAIM" (≈28px
    white). (All five rows show CLAIM available in the mockup; in-progress quests would show it disabled.)

**Footer:** y 0.070 → 0.140, spans inner panel width.
- Lbl_BonusText "Complete all Daily Quests to earn bonus rewards!": left/center, x≈0.130, y≈0.100 (≈22px).
- BonusMeter "4/5": center, a small meter + text (≈24px) at x≈0.620.
- Icon_BonusChest: right, x≈0.860, a gold bonus chest icon Ø≈0.06w.

**Tablet (4:3):** panel keeps clamped max width; rows keep full layout; if width tight, description may
truncate (name + progress + reward + CLAIM are priority). **Ultrawide (21:9):** panel stays centered with a
max width; extra width = more parchment/BG margin. **Notch:** SafeAreaRoot insets; BG full-bleed under cutout;
panel + back/timer stay inside safe area.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "DAILY QUESTS" title | serif display | Black | UPPER | +8% | gold bevel + bloom + dark drop | 56 | `#f0d27a`→`#caa04a` |
| "Reset In:" tag | sans | Regular | Title | +2% | dim | 18 | `#6b5a3a` |
| "13h 46m 21s" | tabular sans | Semibold | — | tabular | — | 24 | `#3a2e1c` |
| Tab labels "Daily"/"Weekly" | strong sans | Bold | Title | +3% | selected white; idle brown | 28 | sel `#eaf2ff`; idle `#5a4a30` |
| Quest name (e.g. "Win 3 Battles") | strong sans/serif | Semibold | Title | +2% | subtle emboss on parchment | 28 | `#3a2e1c` |
| Quest description | sans, light | Regular | Sentence | +1% | — | 20 | `#5a4a30` |
| Progress "3 / 3" etc. | tabular sans | Semibold | — | tabular | over-bar (light on dark groove) | 18 | `#f4e9c8` |
| Reward qty (50/2000/30/1500/20) | tabular sans | Bold | — | tabular | 1px stroke | 26 | `#3a2e1c` |
| "CLAIM" | strong sans | Bold | UPPER | +6% | white + dark stroke | 28 | `#ffffff` |
| Bonus text | sans | Regular | Sentence | +2% | dim | 22 | `#5a4a30` |
| "4/5" bonus meter | tabular sans | Semibold | — | tabular | — | 24 | `#3a2e1c` |

Font: serif SDF for the DAILY QUESTS title; Roboto SDF for rows/numbers (legacy Text fallback acceptable).

---

## G — MATERIALS
- **BG:** dark dusk battlefield, low contrast, a faint faction banner on the right; strong vignette to focus
  the parchment.
- **Parchment scroll panel:** aged cream paper with subtle fiber + edge-darkening (`#d9c79a` center →
  `#9c855a` edges), matte (high roughness), faint stains; a **gold rolled-edge frame** (brushed gold
  `#6b5320`→`#f0d27a`) with corner ornaments and small rolled top/bottom ends.
- **Row plates:** slightly inset parchment/aged-wood plates with thin gold dividers between rows; a soft inner
  shadow gives each row a recessed feel.
- **Quest emblems:** circular gold-rimmed medallions with class-themed glyphs (swords, units, chest, damage,
  login-star), lightly lit.
- **Progress bars:** dark groove `#2a2418` with a warm-gold/green glassy fill + top specular; completed bars
  fully filled & brighter.
- **Reward chips:** small dark-rimmed chips with a glassy gem or shiny coin icon + dark-brown quantity.
- **CLAIM buttons:** cobalt enamel `#2b56c8` (lighter top edge, dark bottom, gold trim), rounded ~12px, white
  label, idle glow — the brightest interactive elements against the parchment.
- **Footer bonus chest:** gold-trimmed wooden chest icon with a faint glow.

---

## H — COMPONENTS (states)
**QuestRow / Btn_Claim:**
- *complete + claimable* (progress full, e.g. 3/3, 1/1): row plate normal, progress bar full, **CLAIM** bright
  cobalt + gentle pulse.
- *in-progress* (e.g. 32/50, 7,250/10,000): progress bar partial; **CLAIM disabled** = desaturated grey-blue,
  label dim, not interactable (tap → subtle nudge "Keep going!"). (In the mockup all show CLAIM; this is the
  general state model.)
- *claimed:* CLAIM → "CLAIMED" with a check, button settles disabled/grey; row may dim slightly; reward flies
  to the currency chip.
- *hover (CLAIM):* +10% brightness, glow grows; *pressed:* scale 0.97 + flash.

**Tabs (Daily/Weekly Toggle group):**
- *selected:* cobalt fill, bright label; Daily shows a **red unread dot** when unclaimed rewards exist.
- *idle:* parchment/brown; *hover:* brightens; click swaps the quest list to that cadence.

**RewardChip:** static; gem vs coin per quest; hover tooltip optional.

**ResetTimer:** live countdown; on hitting 0 → quests refresh (list reload, dot resets).

**BonusMeter:** "4/5" progress toward the all-complete bonus; when 5/5, the bonus chest becomes claimable
(glows) → tapping claims the bonus.

**Back button:** standard hover/pressed.

---

## I — ANIMATION TIMELINE
**OnShow (~0.55s):**
- 0.00s BG + vignette fade (0.20s).
- 0.05s **Scroll panel unrolls**/scales in (0.30s ease-out) — or fade+scale 0.96→1.0; gold frame settles.
- 0.12s Header (title scale 0.96→1.0 + fade; timer fade) 0.22s.
- 0.16s Tabs fade in; red dot pops on Daily.
- 0.20s **Quest rows stagger in** top→bottom (each slide 12px from right + fade, 0.05s stagger).
- 0.32s **Progress bars fill** left→right to their values (0.40s ease-out); complete bars flash at full.
- 0.40s CLAIM buttons fade in; claimable ones get a single glow sweep.
- 0.46s Footer fades in; bonus meter fills to 4/5.

**OnClaim(row):** CLAIM flash → reward icon pops + flies to the currency chip (0.35s); chip count ticks up;
row's CLAIM → "CLAIMED" check; bonus meter increments (4/5→5/5 if applicable, then bonus chest glows).
**OnTabSwitch:** list cross-fades/slides 16px (0.22s); selected pill swaps; red dot updates.
**OnReset (timer→0):** list fades, refreshes with new quests (stagger back in), dot resets.

---

## J — PARTICLE & FX
- **Scroll panel:** faint warm dust on enter; subtle gold frame shimmer.
- **Claimable CLAIM buttons:** soft cobalt glow pulse.
- **Claim:** reward sparkle burst + fly-to-chip; chip coin/gem poof on arrival.
- **Quest emblems:** single gold glint on enter.
- **Bonus chest (when 5/5):** glow pulse + sparkle invite.
Budget pooled/capped; reduce on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load quests for the active tab (default Daily) read-only: name, desc, progress current/target,
  reward, claim/claimed state; compute bonus meter; run enter timeline; start reset countdown.
- **OnTab(Daily/Weekly):** swap the quest list + bonus meter for that cadence; update red dot.
- **OnClaim(quest):** if complete & not claimed → send claim request (server-auth); on success grant reward,
  tick currency, mark CLAIMED, update bonus meter; else (in-progress) ignore/nudge.
- **OnBonusClaim (5/5):** claim the all-complete bonus (server-auth).
- **OnResetTimer→0:** reload quests (server refresh).
- **OnBack:** pop → Main Menu.
- **§12:** all claims are server-authoritative requests; UI displays + requests only; no local balance
  mutation; no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn rows/values: Win 3 Battles (3/3, 50 gem), Train 50 Units (32/50, 2000 coin),
  Open 1 Silver Chest (1/1, 30 gem), Deal 10,000 Damage (7,250/10,000, 1500 coin), Log In Daily (1/1, 20 gem);
  reset "13h 46m 21s"; bonus "4/5"; descriptions verbatim.
- Two tabs only: **Daily** / **Weekly**. Do not add more.
- No currency chips are drawn in the header here (only the reset timer) → do **not** add Gold/Gem chips up top.
- Do not invent extra quests beyond the 5 shown (the list may scroll for more, but spec the 5 as drawn).
- Keep the parchment field (do not darken it to match other screens); CLAIM stays cobalt.
- No portrait variant, no stick figures, no real brand text.
- Claims are requests only; no local/ECS mutation.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Parchment scroll panel with gold rolled-edge frame over a dark battlefield BG.
2. Header: "DAILY QUESTS" title + "Reset In: 13h 46m 21s" timer.
3. Tabs Daily (selected cobalt, red unread dot) / Weekly.
4. Five quest rows with correct emblem, name, description, progress bar+value, reward chip (gem/coin+qty),
   and CLAIM button — exact strings/numbers.
5. Footer: "Complete all Daily Quests to earn bonus rewards!" + "4/5" meter + bonus chest.
6. Panel unroll, row stagger, progress fill, claim fly-to-chip animations present.
7. Disabled-CLAIM state defined for in-progress quests; claimed state defined.
8. Safe-area honored; BG full-bleed; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges (parchment dark-brown text, cobalt CLAIM).

## N — IMPLEMENTATION CONFIDENCE
**93/100.** High confidence: this is a clean, legible parchment list — title, timer, tabs, five fully-readable
quest rows (names, descriptions, progress, rewards, CLAIM), and footer are all clear. Minor uncertainty: exact
quest emblem glyphs, whether the list scrolls beyond 5, and the precise footer layout (bonus text vs meter vs
chest spacing). No structural risk.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 26 · Quests".
- [x] Fraction-based layout normalized to 2340×1080; row sizes + gaps given.
- [x] ScrollRect/VerticalLayoutGroup specified for the quest list.
- [x] Quest-row / CLAIM / tab / bonus states (claimable/in-progress/claimed/selected/idle).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; nothing invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.
