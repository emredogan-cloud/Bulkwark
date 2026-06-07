# BULWARK — UI CONSTRUCTION SPEC · 16 · Ladder Result

Source: design/LadderResultDesign.png · 1672×941 (≈1.78:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas; geometry is **fractions of 2340×1080**. This is a **POST-MATCH RESULT** screen shown when an **async (ghost) ladder/ranked** match resolves. The win/loss verdict comes from ECS `MatchState` (read-only); the **rank tier, points delta, rank-up flag, and rewards are server-authoritative** (display-only on client). **No code here** — construction spec only.

---

## A · SCREEN PURPOSE
The **Ladder Result** screen closes an asynchronous ranked match (you vs an opponent's "ghost"). Beyond the simple win/loss, its job is **competitive progression feedback**: (1) headline the match result (`VICTORY` shown — the win variant), (2) present the player's **rank tier emblem** (a laurel-wreathed heraldic crest, e.g. `Gold Tier III`) with a **"Rank up!"** callout when the result advances the tier, (3) show the head-to-head **You vs Opponent** framing with faction shields, (4) show the **points delta** (`+24 Points`, green for a gain), (5) grant ranked **rewards** (coins), and (6) funnel forward with one **CONTINUE** CTA. This is the most "esports/ranked" of the result screens — gold + Iron-Pact blue, a prestigious rank crest as the hero, and progression numbers. Source of truth: ECS verdict + server rank/points/reward.

State machine position: `Online/Ladder match → Ladder Result → CONTINUE → hub (or ladder screen)`.

---

## B · VISUAL DNA (screen-specific)
- **Theme = PRESTIGE RANKED: GOLD + IRON-PACT ROYAL BLUE on dark.** Closest in palette to the standard Victory (gold title, blue CTA) but with a **competitive/heraldic** layer: a rank-tier crest, faction VS shields, and a points delta. Loss variant would re-skin to somber/red, but the source is the **win/`VICTORY`** state and that is what this spec captures forensically (with a loss-variant note in §K).
- **Hero element = the RANK TIER EMBLEM.** Directly beneath the title sits a large **gold laurel wreath** encircling a **royal-blue heraldic shield** that bears a **gold gladiator/helm sigil**, with **radiating gold light rays** behind it (a "rank medallion"). Below it, the tier label `Gold Tier III`. This crest is the focal subject (the prize is *status*, not loot).
- **Title:** `VICTORY` in the same gold-bevel serif DNA as screen 12, top-center, breaking the frame, flanked by blue banners with **eagle/winged finials** at the top corners.
- **VS framing:** a horizontal row — **You** (a royal-blue Iron-Pact shield banner with a gold cross) on the left, a gold **VS** monogram in the center, **Opponent** (an oxblood Ashen shield banner with a jagged sigil) on the right.
- **Progression number = GREEN points delta.** `+24 Points` in **green** (gain) with a small green up-indicator/coin glyph — green = "you gained rating," mirroring the campaign green-time convention.
- **Rewards:** a single reward row — **silver coins** stack + amount (`12,450`) under a small "Rewards" label.
- **CTA:** one **CONTINUE** (royal-blue, gold frame) — single forward action, like Victory.
- **Background:** a dark, dramatic **enemy-capital/castle skyline at dusk/night** (the ranked arena), with faint red opponent banners in the distance, heavily vignetted; pushed back behind an ornate gold-framed panel.
- **Mood:** prestigious, competitive, triumphant, heraldic. Gold + royal blue; green for the rating gain; red only on the opponent shield. Brightest interactive = CONTINUE; brightest *display* = the rank crest.

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
LadderResultScreen (UiScreen root; CanvasGroup)
└── FullBleedRoot (stretch-stretch, ignores safe area)
    ├── BG_RankedArena (Image — dark castle/capital skyline at dusk, full-bleed)
    ├── BG_Vignette (Image — heavy radial vignette → near-black edges)
    ├── FX_GodRays (Image/PS — warm gold rays behind the rank crest, additive)
    └── BG_DarkenScrim (Image — black ~0.45)
    SafeAreaRoot (RectTransform — Screen.safeArea inset)
    └── ResultPanel (Container; anchored center; ~0.54w × ~0.92h)
        ├── Banner_Left (Image — blue Iron-Pact banner w/ eagle finial, top-left)
        ├── Banner_Right (Image — blue Iron-Pact banner w/ eagle finial, top-right)
        ├── Panel_Frame (Image — ornate gold beveled 9-slice frame)
        │   └── Panel_Interior (Image — near-black obsidian fill, faint warm glow)
        ├── Title_Victory (Text "VICTORY" — gold-bevel serif, breaks frame)
        │   └── Title_Glow (Image — soft gold bloom)
        ├── Rank_Group (anchored upper-center, the hero crest)
        │   ├── Rank_Rays (Image — radiating gold light rays, additive)
        │   ├── Rank_Wreath (Image — gold laurel wreath)
        │   ├── Rank_Shield (Image — royal-blue heraldic shield)
        │   ├── Rank_Sigil (Image — gold gladiator helm/sigil on shield)
        │   └── Rank_TierLabel (Text "Gold Tier III")
        ├── RankUp_Text (Text "Rank up!" — white serif; shown only on tier-up)
        ├── VS_Group (Horizontal — You | VS | Opponent)
        │   ├── Side_You (Container)
        │   │   ├── You_Shield (Image — blue Iron-Pact shield w/ gold cross)
        │   │   └── You_Label (Text "You")
        │   ├── VS_Mark (Image/Text — gold "VS")
        │   └── Side_Opponent (Container)
        │       ├── Opp_Shield (Image — oxblood Ashen shield w/ jagged sigil)
        │       └── Opp_Label (Text "Opponent")
        ├── Points_Group
        │   ├── Points_Icon (Image — small green up-arrow / point coin)
        │   └── Points_Value (Text "+24 Points" — GREEN)
        ├── Rewards_Group
        │   ├── Rewards_Label (Text "Rewards")
        │   ├── Icon_Coins (Image — silver coin stack)
        │   └── Coins_Value (Text "12,450")
        └── CTA_Continue (Button — royal-blue, gold frame)
            ├── Continue_Frame (Image)
            ├── Continue_Fill (Image — blue gradient + inner blue rim)
            └── Continue_Label (Text "CONTINUE")
```

---

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LadderResultScreen | Canvas | — | UiScreen+CanvasGroup | stretch | .5,.5 | — | root | fade |
| FullBleedRoot | screen | 0 | RectTransform | stretch | .5,.5 | — | ignores | full-bleed |
| BG_RankedArena | FullBleedRoot | 0 | Image | stretch | .5,.5 | center-crop | no | AspectFill |
| BG_Vignette | FullBleedRoot | 1 | Image | stretch | .5,.5 | center | no | radial, heavy |
| FX_GodRays | FullBleedRoot | 2 | Image/PS | top/center | .5,1 | center | no | width tracks crest, additive |
| BG_DarkenScrim | FullBleedRoot | 3 | Image | stretch | .5,.5 | — | no | α0.45 |
| SafeAreaRoot | screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | applies | insets |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center | .5,.5 | center | yes | width=min(0.54·W, height-cap) |
| Banner_Left/Right | ResultPanel | 0,1 | Image | top-left / top-right | 0,1 / 1,1 | — | yes | draped corners w/ eagle finials, mirror |
| Panel_Frame | ResultPanel | 2 | Image (9-slice) | stretch | .5,.5 | — | yes | gold 9-slice |
| Panel_Interior | Panel_Frame | 0 | Image (9-slice) | stretch | .5,.5 | — | yes | inset; obsidian |
| Title_Victory | ResultPanel | 3 | Text (TMP) | top/center | .5,1 | center | yes | breaks frame, auto-size capped |
| Title_Glow | Title_Victory | 0 | Image | center/center | .5,.5 | center | yes | behind glyphs |
| Rank_Group | ResultPanel | 4 | RectTransform | top/center | .5,1 | center | yes | upper-center hero crest |
| Rank_Rays | Rank_Group | 0 | Image | center/center | .5,.5 | center | yes | additive, behind crest |
| Rank_Wreath | Rank_Group | 1 | Image | center/center | .5,.5 | center | yes | laurel ring |
| Rank_Shield | Rank_Group | 2 | Image | center/center | .5,.5 | center | yes | blue shield |
| Rank_Sigil | Rank_Group | 3 | Image | center/center | .5,.5 | center | yes | gold helm on shield, top z |
| Rank_TierLabel | Rank_Group | 4 | Text (TMP) | bottom/center | .5,1 | center | yes | beneath crest |
| RankUp_Text | ResultPanel | 5 | Text (TMP) | top/center | .5,1 | center | yes | conditional (tier-up) |
| VS_Group | ResultPanel | 6 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | You | VS | Opponent |
| Side_You / Side_Opponent | VS_Group | 0 / 2 | RectTransform | — | .5,.5 | center | yes | each ≈0.34 panel width |
| You_Shield / Opp_Shield | side | 0 | Image | top/center | .5,1 | center | yes | faction shield banner |
| You_Label / Opp_Label | side | 1 | Text (TMP) | bottom/center | .5,0 | center | yes | small caps |
| VS_Mark | VS_Group | 1 | Image/Text | center/center | .5,.5 | center | yes | gold "VS" |
| Points_Group | ResultPanel | 7 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | icon + value |
| Points_Icon | Points_Group | 0 | Image | center/center | .5,.5 | center | yes | green up indicator |
| Points_Value | Points_Group | 1 | Text (TMP) | center/center | .5,.5 | center | yes | green, tabular |
| Rewards_Label | ResultPanel | 8 | Text (TMP) | center/center | .5,1 | center | yes | small caps |
| Rewards_Group | ResultPanel | 9 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | coin icon + value (single reward) |
| Icon_Coins | Rewards_Group | 0 | Image | center/center | .5,.5 | center | yes | silver stack |
| Coins_Value | Rewards_Group | 1 | Text (TMP) | center/center | .5,.5 | center | yes | tabular |
| CTA_Continue | ResultPanel | 10 | Button | bottom/center | .5,0 | center | yes | width ≈0.74 panel interior; above bottom frame |
| Continue_Frame/Fill/Label | CTA_Continue | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | blue CTA |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
W=2340, H=1080, match HEIGHT.

**Background:** BG_RankedArena 0,0→1,1 AspectFill (castle skyline focal center); BG_Vignette heavy → near-black corners; FX_GodRays top-center behind the rank crest (width ≈ 0.45·W, height ≈ 0.50·H); BG_DarkenScrim α≈0.45.

**ResultPanel:** width ≈ **0.54·W ≈ 1264 px**, centered. Height ≈ **0.92·H ≈ 994 px**, centered. Aspect ≈ 1.27:1. Cap: width = `min(0.54·W, 1.30·panelHeight)`. (Tall card: title + crest + rank-up + VS + points + rewards + CTA is a deep stack.)

**Banners:** blue Iron-Pact banners anchored top-left/top-right (pivot 0,1 / 1,1), width ≈ 0.11·panelW, hang down ≈ 0.55·panelH, with **eagle/winged finials** at the very top corners (the finials sit at/above the frame top).

**Internal vertical rhythm (fractions of PANEL height, 0=panel top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Title "VICTORY" (breaks frame) | 0.05 | 0.11 |
| Rank crest (rays+wreath+shield+sigil) | 0.24 | 0.22 |
| `Gold Tier III` label | 0.35 | 0.04 |
| "Rank up!" | 0.43 | 0.06 |
| VS row (You / VS / Opponent) | 0.56 | 0.16 |
| `+24 Points` | 0.70 | 0.05 |
| "Rewards" label | 0.76 | 0.04 |
| Coins icon + `12,450` | 0.82 | 0.06 |
| CONTINUE CTA | 0.92 | 0.09 |

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.62, centered.
- Rank crest: wreath diameter ≈ 0.30·panelW, centered; shield ≈ 0.16·panelW within it; rays ≈ 0.50·panelW (overflow). 
- VS row width ≈ 0.72; You side ≈ 0.32·panelW (left), Opponent side ≈ 0.32·panelW (right), VS mark ≈ 0.08·panelW centered. Each faction shield ≈ 0.16·panelW wide.
- Points row centered; small green icon ≈ 0.04·panelW left of the value.
- Rewards row centered; coin icon ≈ 0.06·panelW left of `12,450`.
- CONTINUE width ≈ 0.74·panelW, centered; height ≈ 0.075·panelH.

**Notch/safe-area:** panel centered in safe rect; arena bleeds under cutout. **Tablet/ultrawide:** cap keeps proportion; ultrawide reveals more skyline; god-rays/crest stay centered.

---

## F · TYPOGRAPHY (per text)
Sizes px@1080. Dark obsidian panel → light/gold/green text.

| Text | Content | Family / personality | Weight | Caps | Tracking | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|
| Title_Victory | `VICTORY` | Trajan-style serif, regal, gold-bevel | Black/ExtraBold | ALL-CAPS | +6% | gold gradient + bloom + faceted bevel (same as screen 12) | ~96–104 (smaller than std Victory — shares space with the crest below) | gradient `#fded61`→`#caa04a`; cores `#fefff2` | dark stroke `#3a2a08` ~3px + shadow α0.6 (0,4) + bloom |
| Rank_TierLabel | `Gold Tier III` | serif small-caps, prestige | SemiBold | small-caps (Title-case shown) | +10% | engraved gold, low glow | ~28–32 | warm gold `#e0bf6a` | dark stroke `#3a2a08` ~1.5px + shadow α0.5 (0,2) |
| RankUp_Text | `Rank up!` | serif display, celebratory | Bold | Title-case (as shown) | +4% | bright white, faint gold glow | ~46–52 | warm white `#fffbf0` | dark stroke `#1a140a` ~2px + shadow α0.6 (0,3) + faint gold glow α0.3 |
| You_Label | `You` | serif small-caps, label | Medium | Title-case (as shown) | +6% | subtle | ~24–28 | cool parchment `#d8cfbc` | shadow α0.5 (0,2) |
| Opp_Label | `Opponent` | serif small-caps, label | Medium | Title-case (as shown) | +6% | subtle | ~24–28 | cool parchment `#d8cfbc` | shadow α0.5 (0,2) |
| VS_Mark | `VS` | serif display, bold monogram | ExtraBold | ALL-CAPS | +4% | gold, beveled, slight glow | ~40–46 | gold `#e0bf6a` (cores `#fff0c0`) | dark stroke `#3a2a08` ~2px + shadow α0.6 (0,3) |
| Points_Value | `+24 Points` | semi-condensed, tabular for the number | SemiBold | Title-case "Points" | 0% (number) | **green glow** | ~34–40 | vivid green `#7ad13a` (number cores `#b6e35a`); "Points" word slightly dimmer green `#8ab84a` | dark green stroke `#2c4a0c` ~1.5px + green glow α0.4 + shadow α0.4 (0,2) |
| Rewards_Label | `Rewards` | serif small-caps, label | SemiBold | small-caps (Title-case shown) | +8% | engraved gold | ~24–26 | antique gold `#c8a55a` | inner dark, low glow |
| Coins_Value | `12,450` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp, faint glow | ~34–38 | warm white `#f3ead2` | shadow α0.6 (0,2) |
| Continue_Label | `CONTINUE` | serif display, authoritative | Bold | ALL-CAPS | +8% | bright, faint blue glow | ~42–46 | warm white `#fffbf0` | dark-blue stroke `#0a1f44` ~2px + shadow α0.6 (0,3) |

The number in `+24 Points` is the green focal stat; the rank crest is the visual hero (art, not text).

---

## G · MATERIALS
- **Panel interior:** obsidian/charcoal `#0a0b0f`–`#14161e`, matte; a faint warm radial glow behind the rank crest (`#3a2a0e` α≈0.18) lit by the god-rays.
- **Gold frame + title + wreath + VS + finials:** brushed antique gold — highlights `#f0d27a`–`#fded61`, mid `#caa04a`, shadow `#6b5320`; engraved filigree; bloom on the wreath, title, and ray highlights.
- **Blue Iron-Pact banners + eagle finials:** royal-blue cloth `#1f3e78`–`#2b56c8` with stitched gold trim; the top finials are gold eagle/wing ornaments `#caa04a` with bloom; soft folds + drop shadow.
- **Rank crest:**
  - **Rays:** additive radial gold light `#ffe9a0`→`#f0b840`→transparent behind the wreath; the strongest bloom on the panel.
  - **Wreath:** gold laurel `#caa04a`–`#f0d27a`, beveled leaves, rim-light.
  - **Shield:** royal-blue heraldic shield `#214365`–`#2b56c8` with a beveled gold border and a subtle inner sheen.
  - **Sigil:** a gold gladiator/helm emblem `#caa04a`–`#e8c46a` centered on the shield, beveled, slight bloom.
- **You shield (Iron-Pact):** royal-blue `#214365`–`#2b56c8` shield-banner with a **gold cross** `#caa04a` and gold trim; clean, heroic.
- **Opponent shield (Ashen):** oxblood-red `#5a1410`–`#8a241c` shield-banner with a **jagged dark sigil** and iron trim; slightly desaturated (the rival).
- **VS mark:** beveled gold monogram with a slight outer glow, sitting between the two shields.
- **Points indicator:** small green up-arrow / point token `#7ad13a`–`#b6e35a` with a soft green glow.
- **Coin icon:** silver/steel stack `#9aa0a8`/`#5a6068`, faint embossed face.
- **CONTINUE fill (royal-blue):** gradient `#0d2a66`→`#2f6fd6`/`#388ee8`, inner cobalt rim-glow `#5aa0ff` α≈0.45; satin sheen — the brightest interactive object.
- **Bloom budget:** rank-crest rays + wreath first, then the title, the green points number, the VS mark, and the CONTINUE top edge; the rest matte. The crest is the luminous *display* hero; CONTINUE is the luminous *action*.

---

## H · COMPONENTS (states + feedback)
**Rank crest (display, animated):** the hero. On reveal: rays bloom out, wreath + shield assemble, sigil pops, and (on tier-up) the crest does a celebratory flare + a "rank-up" sting. Persistent gentle ray shimmer + sigil glint afterward. If the result is **not** a tier-up, the crest still shows the current tier but skips the flare and the `Rank up!` line is absent.

**VS shields (display):** non-interactive; on reveal the two shields slide in from their sides toward center, the VS mark snaps in with a small clash spark. The **winner's** shield (You, in this victory state) sits slightly brighter/forward; the opponent's is slightly dimmer/recessed.

**Points value (display, animated):** count-up from `+0` to `+24` (green), with a soft upward "tick" and a green glow pulse on settle. (On a loss variant this would be a red `-N` with a downward indicator — see §K.)

**Reward (display):** coins count-up; brief glint.

**CTA_Continue (primary Button):**
- **Idle:** blue gradient, gold frame, inner blue rim, white label; subtle breathing glow.
- **Hover:** frame +8%, glow +25%, scale →1.03.
- **Pressed:** scale →0.97, fill darkens ~10%, glow flash, "anvil" click SFX.
- **Disabled:** N/A (always enabled post-reveal).
- **Confirm:** blue ring flash → hub/ladder.

**Focus order:** default selected = **CONTINUE** (single CTA). Back/B → same CONTINUE action (non-destructive). After reveal, a tap anywhere may act as CONTINUE.

---

## I · ANIMATION TIMELINE (prestige rank reveal)
The Ladder reveal centers on the **rank-crest ceremony** and the **points count-up**.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.30 | FullBleedRoot CG | fade 0→1 (arena + scrim) | linear |
| 0.05 | 0.40 | FX_GodRays | fade 0→1 + slow drift | ease-out |
| 0.15 | 0.40 | ResultPanel | scale 0.9→1.0 + α 0→1 | back-ease-out (overshoot) |
| 0.25 | 0.30 | Banner_Left/Right | unfurl from corners (eagle finials settle) | ease-out |
| 0.40 | 0.40 | Title_Victory | α 0→1 + scale 1.12→1.0 + gold bloom flare→settle | ease-out |
| 0.50 | 0.20 | FX_TitleImpact | one-shot gold sparkle burst behind title | ease-out |
| 0.75 | 0.35 | Rank_Rays | bloom out (scale 0.7→1.0 + α) | ease-out |
| 0.80 | 0.35 | Rank_Wreath + Shield | assemble: wreath scales in + shield drops into center | back-ease-out |
| 1.05 | 0.25 | Rank_Sigil | pop onto shield + glint | back-ease-out |
| 1.10 | 0.30 | (if tier-up) crest flare | celebratory gold flare + "rank-up" sting | ease-out |
| 1.20 | 0.25 | Rank_TierLabel | α 0→1 (`Gold Tier III`) | ease-out |
| 1.45 | 0.30 | RankUp_Text | (if tier-up) α 0→1 + scale 1.1→1.0 (`Rank up!`) | back-ease-out |
| 1.75 | 0.35 | VS shields | You slides in from left, Opponent from right toward center | ease-out |
| 2.05 | 0.20 | VS_Mark | snap in + small clash spark | back-ease-out |
| 2.25 | 0.60 | Points_Value | **count-up +0 → +24** (green), upward tick, green glow on settle | ease-out |
| 2.65 | 0.25 | Rewards_Label | α 0→1 | ease-out |
| 2.80 | 0.70 | Coins_Value | **count-up 0 → 12,450** + coin glint | ease-out |
| 3.10 | 0.30 | CTA_Continue | α 0→1 + scale 0.94→1.0 + glow ignite | back-ease-out |
| 3.40 | loop | CTA glow | breathing pulse (period ~1.8 s) | sine in-out |
| 3.40 | loop | Rank crest | gentle ray shimmer + sigil glint (period ~2.5 s) | sine in-out |

**OnContinue (exit):** press dip → blue ring flash → panel α→0 + scale→0.96 + bg fade → hub/ladder. 
**Skip rule:** tap during reveal snaps all tweens (crest assembly, VS slide, points + coin count-ups, rank-up flare) to end-state and enables CONTINUE; the correct tier + points + rank-up presence are resolved on skip.

---

## J · PARTICLE & FX (passive)
- **FX_GodRays:** warm gold light shafts behind the rank crest, slow shimmer + sway (the prestige halo).
- **Rank-crest rays + wreath bloom:** persistent gold bloom; gentle shimmer.
- **Sigil glint:** occasional gold spec sweep across the helm sigil.
- **Title bloom:** persistent soft gold bloom on `VICTORY`.
- **Green points glow:** soft persistent green bloom on `+24 Points`.
- **VS clash spark:** one-shot small spark when the VS mark snaps in.
- **Coin glint:** occasional silver sweep.
- **CONTINUE rim-glow:** persistent breathing cobalt (see §I).
- **Background banners:** faint distant red opponent banners barely lit; very low opacity behind the scrim.

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload = `{ outcome:Win|Loss, tierName:string, tierUp:bool, pointsDelta:int, coins:int, opponentFaction }` from ECS verdict + **server-authoritative** rank/points/reward (all **display-only**). 
  - **Win variant (this source):** title `VICTORY` (gold), You shield highlighted, points delta **green `+24 Points`**, optional `Rank up!` if `tierUp`. 
  - **Loss variant (note):** title `DEFEAT` (desaturated gold/red per screen 13 DNA), Opponent shield highlighted, points delta **red `-N`** with a downward indicator, `Rank up!` replaced by an optional `Rank down`/none, somber grade. The structure is identical; only color/copy/highlight swap by `outcome`. This spec captures the win state as shown.
- **OnReward (count-ups):** points → `+24`, coins → `12,450`. All display-only; rank/points/rewards are computed server-side (§12) — the UI never writes them.
- **OnRankUp:** if `tierUp`, play the crest flare + `Rank up!` reveal + sting; else skip both (crest still shows current tier).
- **OnContinue:** single CTA → route to hub (or back to the ladder/online screen). Debounced. No network write for "continue."
- **OnBack (B/Esc):** aliased to CONTINUE (non-destructive).
- **Idempotency / re-entry:** settles to end-state immediately; no re-grant/re-count; tier + points reflect stored server values.
- **No mutation:** navigation only; never edits rank/points/balance client-side.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** hard-code `Gold Tier III`, `+24`, or `Rank up!` — tier/points/tier-up are **server-authoritative** data; the screen displays whatever it is given. 
2. Do **not** show `Rank up!` (or the crest flare) when `tierUp == false` — omit them. 
3. Do **not** color the points delta green on a loss — green = gain only; a loss is red `-N` with a downward indicator (loss variant). 
4. Do **not** drop the **You vs Opponent** framing or the faction shields (blue Iron-Pact left, oxblood Ashen right) — it is the ladder's competitive signature. 
5. Do **not** swap the reward identity — this variant grants **silver coins** only (`12,450`); do not invent extra reward slots. 
6. Do **not** add a RETRY here — ranked/async has a single CONTINUE (no instant rematch on the result screen). 
7. Do **not** let CONTINUE be outshone — it is the single brightest interactive object (the rank crest is the brightest *display* element, not a button). 
8. Do **not** put interactive content under the notch; only the arena bleeds there. 
9. Do **not** mutate rank/points/balance client-side (§12); all are display-only. 
10. Do **not** stretch frame/banner/wreath ornament (9-slice fixed). 
11. Do **not** use portrait or assume width is stable; match HEIGHT, fraction-based. 
12. Do **not** confuse this with standard Victory (12): this variant adds the rank crest, VS shields, points delta, and tier label, and grants coins (not a chest).

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] Centered obsidian card with an ornate gold frame over a dark, vignetted **castle/capital skyline**, with **blue Iron-Pact banners + gold eagle finials** at the top corners and warm god-rays behind the crest.
- [ ] `VICTORY` gold-bevel serif title breaks the top frame (same DNA as screen 12, slightly smaller).
- [ ] A **rank-tier crest**: gold laurel **wreath** encircling a **royal-blue shield** bearing a **gold helm/gladiator sigil**, with **radiating gold rays**, labelled `Gold Tier III` beneath — the brightest display element.
- [ ] `Rank up!` in white serif appears beneath the tier label (shown on tier-up).
- [ ] A **You vs Opponent** row: blue Iron-Pact shield + `You` (left), gold `VS` (center), oxblood Ashen shield + `Opponent` (right).
- [ ] `+24 Points` in **green** with a small green up-indicator.
- [ ] `Rewards` label + **silver coin stack + `12,450`** (single reward row).
- [ ] Exactly one CTA: **CONTINUE** (royal-blue `#0d2a66`→`#388ee8`, gold frame, inner cobalt glow, white serif) — the brightest interactive object.
- [ ] Reveal ceremony plays in order (panel → banners → title+burst → crest assembly + tier-up flare → tier label → `Rank up!` → VS slide-in + clash → points count-up → reward count-up → CONTINUE ignite); tap-to-skip resolves tier/points/rank-up.
- [ ] CONTINUE shows idle/hover/pressed + breathing glow and routes to hub/ladder.
- [ ] Arena bleeds under the notch; content respects safe area; fraction-based, match-height.
- [ ] Tier/points/rank-up/rewards are data-driven (server-auth), not hard-coded; no client mutation.
- [ ] Side-by-side with `LadderResultDesign.png`, positions within ±2% of panel dims and colors within stated ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**88 / 100.** The richest result screen (title + rank crest + VS + points + rewards + CTA), but every element and label is directly legible, so the structural map is solid. Deductions: (a) it has the most bespoke art of the five — laurel wreath, blue shield + gold helm sigil, radiating rays, eagle-finial banners, two distinct faction VS shields, green point token (~ -5); (b) the **rank-crest assembly + tier-up flare** and the **VS slide/clash** make this the most logic- and animation-rich reveal, needing careful sequencing + a conditional tier-up branch (~ -4); (c) the **win/loss variant swap** (color/copy/highlight/points-sign by `outcome`) is described but only the win state is in the source, so the loss skin is inferred from the global DNA (~ -3). No structural ambiguity in the shown (win) state.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/LadderResultDesign.png`, 1672×941) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header has number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed stated.
- [x] Verbatim labels recorded (`VICTORY`, `Gold Tier III`, `Rank up!`, `You`, `VS`, `Opponent`, `+24 Points`, `Rewards`, `12,450`, `CONTINUE`) — nothing invented.
- [x] Forensic hex ranges from the art (gold title, blue shields, oxblood opponent, green points, blue CTA).
- [x] Full ASCII tree + per-node Unity table.
- [x] Prestige rank-crest + VS + count-up reveal timeline with conditional tier-up and tap-to-skip.
- [x] Win/loss variant behavior noted; §12 boundary honored (server-auth rank/points/reward; display-only; no mutation).
- [x] Negative rules + ≥95% acceptance + confidence rationale.
- [x] No code/asset/scene/prefab/gameplay changes, no commit.
