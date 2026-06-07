# BULWARK — UI CONSTRUCTION SPEC · 12 · Victory

Source: design/VictoryScreenDesign.png · 1908×824 (≈2.32:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas. All geometry is given as **fractions of 2340×1080** so it scales across aspect ratios. This is a **POST-MATCH RESULT** screen: the win/loss verdict and the displayed reward/time originate from ECS `MatchState` (read-only) and a server-authoritative reward grant (display-only on client). **No code in this document** — construction spec only.

---

## A · SCREEN PURPOSE
The **Victory** result screen is shown when the standard (non-campaign / non-endless / non-ladder) match resolves in the player's favour — the enemy statue/objective has been toppled. It must (1) deliver an immediate, triumphant emotional payoff (gold + royal-blue, crown-and-swords heraldry, god-rays, glowing reward chest), (2) summarise the result in two glanceable stats (**gems earned**, **match time**), (3) present the reward as a single focal **chest** (the loot the player won), and (4) funnel the player forward with one bright, unmistakable **CONTINUE** CTA back to the hub. It is celebratory, short-dwell, and single-exit. Source of truth for the verdict = ECS `MatchState.Outcome == Victory`; the gem value and time are read-only display values handed to the screen at OnShow.

State machine position: `Battle (HUD) → [statue falls] → Victory → Main Menu (hub)`. There is exactly one forward action (CONTINUE). No back button, no rematch button on this variant (rematch/retry lives on Defeat & Endless variants).

---

## B · VISUAL DNA (screen-specific, inherits GLOBAL DNA)
- **Verdict colour = TRIUMPHANT GOLD + ROYAL/COBALT BLUE.** This is the warm, "you won" palette. Gold dominates (title, crown, swords, panel frame, chest, REWARD label, god-rays); royal blue is reserved for the **CONTINUE** CTA only (so the eye is pulled to it last as the action).
- **Hero ornament:** a **golden crown flanked by two crossed swords** sits centred on the top edge of the panel, breaking the frame line (a "trophy crest"). Behind/around it, faint blue heraldic banner cloth fans out left and right.
- **Focal subject = the REWARD CHEST**, a wooden + iron-banded treasure chest, **closed**, dead-centre, wrapped in a hot radial **gold glow halo** with sparkle motes — the brightest non-CTA element, the "prize."
- **Background:** a full-bleed dusk battlefield (warm purple-mauve sky `#3d2f3c`, distant burning hills, a fallen giant stone skull/ruins on the right, a friendly **blue banner** planted left, scattered silhouetted combatants along the bottom). Heavily vignetted and pushed back (darkened + slightly blurred) so the central panel reads as the subject.
- **Panel:** a near-black obsidian interior framed by an **ornate cast-gold/antique-bronze beveled frame** with engraved filigree corners and a subtle inner warm glow (the panel is lit from within by the chest).
- **Lighting:** warm volumetric **god-rays** descend behind the title; a focal gold glow blooms from the chest; strong vignette; gold rim-light on the frame; bloom on the gold title and chest.
- **Mood:** heroic, regal, victorious, warm. High contrast: dark field → luminous gold focal → single blue CTA.

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
VictoryScreen (UiScreen root; CanvasGroup; full-rect)
└── FullBleedRoot (RectTransform, stretch-stretch, ignores safe area — art bleeds under cutout)
    ├── BG_Battlefield (Image — dusk battlefield key art, full-bleed)
    ├── BG_Vignette (Image — radial dark vignette overlay, multiply)
    ├── FX_GodRays (Image/ParticleSystem — warm volumetric rays behind title, additive)
    └── BG_DarkenScrim (Image — 35–45% black, focuses the panel)
    SafeAreaRoot (RectTransform — Screen.safeArea inset; all content below lives here)
    └── ResultPanel (Container; anchored center; ~0.46w × ~0.92h)
        ├── Panel_Frame (Image — ornate gold beveled 9-slice frame)
        │   └── Panel_Interior (Image — near-black obsidian fill, inner warm glow)
        ├── Crest_Group (anchored top-center, overlaps frame top edge)
        │   ├── Crest_BannerCloth_L (Image — blue banner fan, behind crown)
        │   ├── Crest_BannerCloth_R (Image — blue banner fan, behind crown)
        │   ├── Crest_SwordL (Image — crossed sword, gold)
        │   ├── Crest_SwordR (Image — crossed sword, gold)
        │   └── Crest_Crown (Image — gold crown, top-most, focal crest)
        ├── Title_Victory (Text "VICTORY" — gold-bevel serif, hero)
        │   └── Title_Glow (Image/Outline — soft gold bloom behind glyphs)
        ├── Subtitle_Ribbon (Container — dark engraved ribbon strip)
        │   └── Subtitle_Text (Text "The enemy statue has fallen!")
        ├── Stats_Row (Horizontal container, 2 cells split by a thin gold divider)
        │   ├── Stat_Gem (cell)
        │   │   ├── Icon_Gem (Image — violet faceted gem)
        │   │   └── Value_Gem (Text "1746")
        │   ├── Stats_Divider (Image — vertical gold filigree separator)
        │   └── Stat_Time (cell)
        │       ├── Icon_Clock (Image — gold pocket-watch/clock)
        │       └── Value_Time (Text "05:28")
        ├── Reward_Group (anchored center-lower)
        │   ├── Reward_GlowHalo (Image — radial gold glow, additive, behind chest)
        │   ├── FX_Sparkles (ParticleSystem — rising gold motes around chest)
        │   ├── Reward_Chest (Image — closed wood+iron treasure chest, focal)
        │   └── Reward_Label (Text "REWARD")
        └── CTA_Continue (Button — royal-blue, gold chevron frame, hero CTA)
            ├── CTA_Frame (Image — gold beveled frame w/ pointed chevron ends + corner flourishes)
            ├── CTA_Fill (Image — royal-blue vertical gradient + inner blue rim glow)
            └── CTA_Label (Text "CONTINUE")
```

---

## D · UNITY HIERARCHY SPEC (per node)
Conventions: **AP** = anchor preset, **Pivot** = (x,y), order = sibling index (0 = first/back).

| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| VictoryScreen | Canvas | — | UiScreen+CanvasGroup | stretch/stretch | .5,.5 | — | root | fade in/out via CanvasGroup |
| FullBleedRoot | VictoryScreen | 0 | RectTransform | stretch/stretch | .5,.5 | — | **ignores** safe area | full-bleed, may extend under notch |
| BG_Battlefield | FullBleedRoot | 0 | Image | stretch/stretch | .5,.5 | center-crop | no | `preserveAspect` via AspectFill; anchor focal to center |
| BG_Vignette | FullBleedRoot | 1 | Image | stretch/stretch | .5,.5 | center | no | radial; scales with screen |
| FX_GodRays | FullBleedRoot | 2 | Image/PS | top/center | .5,1 | top-center | no | width tracks panel; additive |
| BG_DarkenScrim | FullBleedRoot | 3 | Image | stretch/stretch | .5,.5 | — | no | flat color, constant |
| SafeAreaRoot | VictoryScreen | 1 | RectTransform+SafeAreaFitter | stretch/stretch | .5,.5 | — | **applies** Screen.safeArea | insets on notch sides |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center/center | .5,.5 | center | yes | width = min(0.46·W, height-driven cap); see §E |
| Panel_Frame | ResultPanel | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | 9-slice; corner filigree non-stretch via sliced borders |
| Panel_Interior | Panel_Frame | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | inset from frame by frame thickness |
| Crest_Group | ResultPanel | 1 | RectTransform | top/center | .5,.5 | center | yes | y so its vertical mid sits on frame top edge |
| Crest_BannerCloth_L/R | Crest_Group | 0,1 | Image | center/center | .5,.5 | center | yes | mirror via scale.x = ±1 |
| Crest_SwordL/R | Crest_Group | 2,3 | Image | center/center | .5,.5 | center | yes | mirror via scale.x = ±1 |
| Crest_Crown | Crest_Group | 4 | Image | top/center | .5,1 | center | yes | top-most z |
| Title_Victory | ResultPanel | 2 | Text (TMP) | top/center | .5,1 | center | yes | auto-size capped; tracks panel width |
| Title_Glow | Title_Victory | 0 | Image | center/center | .5,.5 | center | yes | behind glyphs (negative z / first child) |
| Subtitle_Ribbon | ResultPanel | 3 | Image (9-slice) | top/center | .5,1 | center | yes | width ≈ 0.78 of panel interior |
| Subtitle_Text | Subtitle_Ribbon | 0 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | auto-fit one line |
| Stats_Row | ResultPanel | 4 | HorizontalLayoutGroup | top/center | .5,1 | middle-center | yes | two equal cells + center divider |
| Stat_Gem / Stat_Time | Stats_Row | 0 / 2 | RectTransform (HLG cell) | — | .5,.5 | left-icon→right-value | yes | each ~0.46 of row |
| Icon_Gem / Icon_Clock | cell | 0 | Image | left/center | 0,.5 | left | yes | square, height-driven |
| Value_Gem / Value_Time | cell | 1 | Text (TMP) | center/center | .5,.5 | center-left | yes | tabular numbers |
| Stats_Divider | Stats_Row | 1 | Image | center/center | .5,.5 | center | yes | thin vertical filigree |
| Reward_Group | ResultPanel | 5 | RectTransform | center/center | .5,.5 | center | yes | centered slightly below mid |
| Reward_GlowHalo | Reward_Group | 0 | Image | center/center | .5,.5 | center | yes | additive; pulses (see §I) |
| FX_Sparkles | Reward_Group | 1 | ParticleSystem | center/center | .5,.5 | center | yes | local-space emitter |
| Reward_Chest | Reward_Group | 2 | Image | center/center | .5,.5 | center | yes | square-ish; focal |
| Reward_Label | Reward_Group | 3 | Text (TMP) | bottom/center | .5,1 | center | yes | beneath chest |
| CTA_Continue | ResultPanel | 6 | Button | bottom/center | .5,0 | center | yes | width ≈ 0.74 panel interior; pinned above bottom frame |
| CTA_Frame | CTA_Continue | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | chevron ends are sliced caps |
| CTA_Fill | CTA_Continue | 1 | Image | stretch/stretch | .5,.5 | — | yes | inset inside frame |
| CTA_Label | CTA_Continue | 2 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | auto-size capped |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
**Canvas:** 2340×1080, CanvasScaler match HEIGHT (=1.0). Origin top-left for fractions below; W=2340, H=1080.

**Background (full-bleed, ignores safe area):**
- BG_Battlefield: anchored 0,0→1,1; AspectFill, focal point = center. On ultrawide the extra width reveals more battlefield (left blue banner / right ruins drift outward). On tablet (4:3) the art is height-fit and cropped on the sides.
- BG_Vignette: full-rect, radial darkening, opacity peaks at corners (~0.7) → center (~0.0).
- BG_DarkenScrim: flat black at α≈0.38 over the whole field.
- FX_GodRays: anchored top-center, width ≈ 0.50·W, height ≈ 0.55·H, fanning down from behind the crest/title.

**ResultPanel (the central card):**
- Reference width in source ≈ 0.46·W → **panel width = 0.46·W ≈ 1076 px**, centered horizontally (x-center = 0.50·W).
- Panel height ≈ 0.92·H ≈ 994 px, vertically centered (y-center = 0.50·H). It nearly fills the safe-area height with small top/bottom margins (~0.04·H each).
- **Cap rule:** panel width = `min(0.46·W, 1.18·panelHeight)` so on ultrawide it does not become squat; on tall/narrow safe areas it shrinks with height. Aspect of the card ≈ 1.08:1 (slightly portrait card in a landscape field).

**Internal vertical rhythm (fractions of PANEL height, top→bottom; 0 = panel top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Crest (crown apex above frame) | ~ -0.04 to 0.06 | 0.18 |
| Title "VICTORY" | 0.16 | 0.13 |
| Subtitle ribbon | 0.27 | 0.06 |
| Stats row | 0.37 | 0.10 |
| Reward chest (center) | 0.58 | 0.30 |
| REWARD label | 0.74 | 0.04 |
| CONTINUE CTA | 0.89 | 0.10 |

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.86, centered.
- Subtitle ribbon width ≈ 0.80, centered.
- Stats row width ≈ 0.84, centered; each stat cell ≈ 0.40, divider ≈ 0.02 centered; within a cell the icon sits at left (~0.10–0.22) and value text fills the remaining right.
- Reward chest width ≈ 0.30 of panel; glow halo ≈ 0.55 of panel (overflows chest).
- CONTINUE width ≈ 0.74 of panel, centered; height ≈ 0.085·panelH.

**Notch/safe-area:** SafeAreaRoot insets to `Screen.safeArea`; in landscape the cutout is on a short side → panel shifts inward from that side automatically (it is centered in the *safe* rect, not the screen rect). Background art stays full-bleed beneath.

**Tablet (4:3, e.g. 1440×1080):** match-height keeps panel height the same; panel width fraction of the *narrower* screen makes the card relatively larger and more centered — acceptable, card remains ≤ 0.55 of width via the cap. **Ultrawide (21:9, 2520×1080):** panel keeps absolute size; more battlefield revealed at the sides; god-rays/vignette re-center on the panel.

---

## F · TYPOGRAPHY (per text)
Sizes are **px at 1080-tall canvas**. Recommended family: heavy serif display (Trajan/Cinzel-style) for titles; semi-condensed serif/sans for body & numbers. Shipped fallback = legacy `Text`; intended = TMP SDF with bevel/glow materials.

| Text | Content | Family / personality | Weight | Caps | Tracking | Line | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|---|
| Title_Victory | `VICTORY` | Trajan-style serif, regal, gold-bevel | Black/ExtraBold | ALL-CAPS | +6% (wide, monumental) | n/a (1 line) | gold metallic gradient + soft bloom + inner highlight; faceted bevel | ~112–120 | gradient `#fded61` (hot top) → `#caa04a` (deep bottom); hot core highlights `#fefff2` | dark stroke `#3a2a08` ~3px + drop shadow `#000` α0.6, offset (0,4), blur ~6 |
| Subtitle_Text | `The enemy statue has fallen!` | clean serif, understated herald | Medium | Title-case (as shown) | +1% | 1 line | subtle warm glow | ~30–34 | parchment `#e6cf9a` | dark stroke `#2a1602` ~1.5px, shadow α0.5 (0,2) |
| Value_Gem | `1746` | semi-condensed, tabular numerals | SemiBold | n/a | 0% | 1 line | crisp, slight glow | ~40–44 | near-white `#f3ead2` | shadow `#000` α0.55 (0,2) |
| Value_Time | `05:28` | same as Value_Gem | SemiBold | n/a | 0% (tabular, colon centered) | 1 line | crisp | ~40–44 | near-white `#f3ead2` | shadow as above |
| Reward_Label | `REWARD` | serif small-caps, engraved | SemiBold | ALL-CAPS | +14% (spaced label) | 1 line | engraved gold, low glow | ~22–24 | antique gold `#c8a55a` | inner dark `#1a1206`, no bloom |
| CTA_Label | `CONTINUE` | serif display, authoritative | Bold | ALL-CAPS | +8% | 1 line | bright, clean, slight outer glow | ~44–48 | warm white `#fffbf0` | dark blue stroke `#0a1f44` ~2px + shadow `#001230` α0.6 (0,3) |

Numbers (`1746`, `05:28`) MUST use tabular/monospaced figures so count-up animation does not jitter width.

---

## G · MATERIALS (hex ranges, finish, wear, edges, reflection/bloom)
- **Gold frame / crest / swords / title:** brushed antique gold. Highlights `#f0d27a`–`#fded61`; mid `#caa04a`; shadow/recess `#6b5320`–`#3a2a08`. Finish: semi-gloss metal with engraved filigree; **worn edges** (slightly darker, micro-nicks) at corners; strong **gold rim-light**; **bloom** on the brightest bevel edges. Crown gem accents (if any) read as small red/blue cabochons.
- **Panel interior:** obsidian/charcoal `#0a0b0f`–`#14161e`; matte; lit from within by a warm radial glow centered on the chest (additive overlay `#3a2409` α≈0.25 fading outward). Subtle vertical brushed texture.
- **Subtitle ribbon:** dark engraved bronze/stone strip `#2a1c0a`–`#3a2810`; recessed (inner shadow top, highlight bottom) so text sits in a carved channel.
- **Reward chest:** wood body `#7a4a20`–`#a05b25` (warm oak), iron bands/lock `#5a5048`–`#9aa0a8` (cool steel) with worn highlights; closed lid; receives the strongest bloom (it is the prize). Edges catch hot gold rim from the halo.
- **Reward glow halo:** additive radial gradient, hot core `#ffe9a0` → `#f0b840` → transparent; bloom-heavy.
- **Gem icon:** faceted violet crystal — highlights `#c89bff`/`#9e6bf0`, body `#7a3fd0`–`#5a2db0`, hot spec dot near-white; inner glow + specular.
- **Clock icon:** gold pocket-watch — gold ring `#caa04a`, pale face `#e8dcc0`, dark hands `#2a1c0a`.
- **CTA royal-blue fill:** vertical gradient deep `#0d2a66` (bottom) → bright `#2f6fd6`/`#388ee8` (top); satin sheen; **inner cobalt rim-glow** `#5aa0ff` α≈0.5 hugging the inside of the frame; faint diagonal specular streak.
- **CTA gold chevron frame:** same gold material as panel frame; **pointed chevron caps** on left/right ends and small ornamental corner flourishes; bevel highlight on top edge.
- **Reflection/bloom budget:** bloom is concentrated on (1) title gold, (2) chest + halo, (3) CTA top edge. Everything else is matte/low-key so these three read as the luminous hierarchy.

---

## H · COMPONENTS (states + feedback)
**CTA_Continue (primary Button):**
- **Idle:** royal-blue gradient fill, gold frame, soft inner blue rim-glow, white label; resting at full opacity. A slow "breathing" glow pulse (see §I) draws attention.
- **Hover/Highlighted (gamepad/pointer):** frame brightens +8%, inner rim-glow intensity +25%, scale 1.0→1.03 (120 ms ease-out), subtle gold sparkle on the chevron tips.
- **Pressed:** scale 1.03→0.97 (60 ms), fill darkens ~10% (deeper blue), inner glow flashes brighter for 80 ms, soft "anvil" click SFX.
- **Disabled:** N/A in normal flow (CONTINUE always enabled once reveal completes). During the reveal sequence before it appears it is simply not yet instantiated/visible (CanvasGroup α0, non-interactive).
- **Released/Confirm:** brief gold ring flash from the button, then screen transition.

**Stat cells (non-interactive display):** no states; only the count-up animation (§I). 

**Reward chest (non-interactive display in this variant):** idle = closed chest with pulsing halo + sparkles. (Opening/loot reveal is handled by the separate `ChestOpenResult` screen if the flow routes there; on this Victory screen the chest stays closed as a trophy.) Optional micro-feedback: a single gold "shine sweep" across the lid every ~3 s.

**Focus order / input:** default selected control = CTA_Continue. Back/B (gamepad) is bound to the same CONTINUE action (no destructive alternative). Tap anywhere outside controls does nothing (prevents accidental skips of the reveal); after reveal completes, the whole panel can optionally accept a tap as CONTINUE.

---

## I · ANIMATION TIMELINE (results-screen reveal sequence)
All times relative to OnShow t=0. Easing noted per step. The sequence is a **staged triumphant reveal** culminating in the CTA.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.30 | FullBleedRoot CanvasGroup | fade 0→1 (battlefield + scrim in) | linear |
| 0.05 | 0.35 | FX_GodRays | fade 0→1 + slow vertical drift begin | ease-out |
| 0.15 | 0.40 | ResultPanel | scale 0.86→1.0 + α 0→1 (card "slams" in) | back-ease-out (slight overshoot to 1.02→1.0) |
| 0.30 | 0.45 | Crest_Group | crown + swords drop from above frame, settle; banners unfurl L/R | ease-out + small bounce |
| 0.45 | 0.40 | Title_Victory | scale 1.15→1.0 + α 0→1; gold bloom flares then settles | ease-out; bloom decay |
| 0.50 | 0.20 | FX_TitleImpact | one-shot gold sparkle burst + light shockwave ring behind title | ease-out |
| 0.70 | 0.30 | Subtitle_Ribbon + text | slide up 12px + α 0→1 | ease-out |
| 0.95 | 0.30 | Stats_Row container | α 0→1 + slide up 10px | ease-out |
| 1.05 | 0.90 | Value_Gem | **count-up 0 → 1746** (eased, tabular) with tick SFX every ~6 frames | ease-out (fast→slow) |
| 1.05 | 0.40 | Value_Time | reveal final `05:28` (no count-up — it is a measured time; type-in or fade) | ease-out |
| 1.30 | 0.40 | Reward_GlowHalo | α 0→1 + scale 0.7→1.0 (glow blooms) | ease-out |
| 1.35 | 0.45 | Reward_Chest | scale 0.6→1.0 + α 0→1 + small rotational settle; one heavy "thud" SFX on land | back-ease-out |
| 1.40 | loop | FX_Sparkles | begins emitting rising gold motes (continuous) | — |
| 1.55 | 0.30 | Reward_Label `REWARD` | α 0→1 + letter-spacing settle (from +20% to +14%) | ease-out |
| 1.85 | 0.35 | CTA_Continue | α 0→1 + scale 0.9→1.0 + inner glow ignite | back-ease-out |
| 2.20 | loop | CTA inner rim-glow | breathing pulse: intensity 0.4↔0.7, period ~1.6 s | sine in-out |
| 2.20 | loop | Reward_GlowHalo | gentle pulse scale 1.0↔1.04 + intensity, period ~2.0 s | sine in-out |
| 2.20 | every ~3 s | Reward_Chest | one-shot diagonal "shine sweep" across lid | linear |

**OnContinue (exit):** CTA pressed → 60 ms press dip → 120 ms gold ring flash on CTA → whole panel α 1→0 + scale 1.0→0.96 (180 ms ease-in) while FullBleedRoot fades 1→0 (200 ms) → route to Main Menu. Total exit ≈ 220 ms.

**Skip rule:** a tap during the reveal (before t≈2.2 s) fast-forwards all in-progress tweens to their end-state instantly (snap), then immediately enables the CTA — so impatient players are never blocked.

---

## J · PARTICLE & FX (passive)
- **FX_GodRays:** soft additive light shafts fanning down behind the crest/title, slow opacity shimmer (period ~4 s), slight horizontal sway. Color `#f4dca0` α≈0.18.
- **FX_Sparkles (chest):** small additive gold motes spawning in a ring around the chest base, rising 60–120 px and fading; rate ~12/s; size 4–10 px; color `#ffe9a0`→transparent. Gives the prize a "magical loot" shimmer.
- **Title bloom:** persistent soft gold bloom on the `VICTORY` glyph edges (post-reveal, low intensity).
- **CTA rim-glow:** persistent breathing cobalt glow (see §I loop).
- **Halo bloom:** persistent radial gold bloom around chest (breathing).
- **Dust/embers (optional, very subtle):** a few slow warm embers drifting upward in the background battlefield to keep the dusk scene alive; must stay behind the scrim, very low opacity, never compete with the chest.

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload = `{ gemsEarned:int, matchTimeSeconds:int }` derived from ECS `MatchState` (verdict) + server reward grant (display values). Verify verdict == Victory (else this screen must not be shown). Initialize all values to hidden/zero, then run the §I reveal. Pause/ignore battle input; battle sim already concluded.
- **OnReward (count-up):** `Value_Gem` animates 0 → `gemsEarned`. The number is **display-only** — the actual currency was granted server-side; the UI never writes a balance (§12). `Value_Time` formats `matchTimeSeconds` → `MM:SS`.
- **OnContinue:** the single CTA. Fires the exit animation (§I), then `UiRouter.Pop`-to-hub (or `Replace` to Main Menu). Must be idempotent (debounce double-tap). No network write required for "continue."
- **OnBack (gamepad B / Esc):** aliased to OnContinue (no destructive path).
- **Idempotency / re-entry:** if the screen is re-shown (e.g., resumed), it must reach the settled end-state immediately (no re-grant, no re-count).
- **No mutation:** this screen issues no ECS commands and no balance mutations; it only reads display values and requests navigation.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** add a RETRY/REMATCH button here — Victory has a single CONTINUE (rematch lives on Defeat/Endless variants). 
2. Do **not** open the chest on this screen (it is a closed trophy; loot reveal is `ChestOpenResult`, a separate screen). 
3. Do **not** use red anywhere as a primary accent (red = defeat/Ashen; this is a gold/blue win). 
4. Do **not** let the CTA be anything but the single brightest interactive object; no other button competes. 
5. Do **not** put interactive content under the notch — only background art bleeds there. 
6. Do **not** mutate currency/balance client-side; values are display-only (§12). 
7. Do **not** block the player during the reveal without a tap-to-skip. 
8. Do **not** stretch the gold frame filigree corners (9-slice borders stay fixed; only the center tiles). 
9. Do **not** let god-rays/sparkles/embers exceed the chest or CTA in brightness/bloom. 
10. Do **not** hard-code the win (`VICTORY`) — it is gated by ECS `MatchState.Outcome`. 
11. Do **not** use portrait layout or assume width is stable; match HEIGHT, fraction-based. 
12. Do **not** count-up the **time** value (it is a fixed measurement; only the gem reward counts up).

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] Panel is a near-black obsidian card with an ornate gold beveled frame, centered in the safe area, width ≈ 0.46·W (capped), aspect ≈ 1.08:1.
- [ ] **Crown + crossed swords** crest breaks the top frame edge, with faint blue banner cloth behind it.
- [ ] `VICTORY` renders as a heavy gold-bevel serif, ALL-CAPS, with gradient `#fded61`→`#caa04a`, dark stroke + shadow + bloom, ~112–120 px@1080.
- [ ] Subtitle reads exactly `The enemy statue has fallen!` in a dark recessed ribbon, parchment-cream text.
- [ ] Two stats: **violet gem icon + `1746`** (left) and **gold clock icon + `05:28`** (right), split by a vertical gold divider; numbers are tabular.
- [ ] A **closed wood+iron chest** sits dead-center-lower inside a hot radial gold glow halo with rising sparkles, labelled `REWARD` beneath in spaced small-caps gold.
- [ ] Exactly one CTA: **CONTINUE**, royal-blue fill (`#0d2a66`→`#388ee8`), gold chevron-ended frame, inner cobalt rim-glow, white serif label; it is the brightest interactive element.
- [ ] Reveal sequence plays in order (bg → card → crest → title+burst → subtitle → stats+count-up → halo → chest thud → label → CTA ignite), with a working tap-to-skip.
- [ ] CTA shows idle/hover/pressed feedback and a breathing glow; pressing it runs the exit and returns to the hub.
- [ ] Background battlefield is full-bleed under the notch, vignetted + scrimmed; content respects safe area.
- [ ] No red primary accents; no extra buttons; no client balance mutation.
- [ ] Side-by-side with `VictoryScreenDesign.png`, element positions match within ±2% of panel dimensions and colors within the stated hex ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**93 / 100.** The screen is compositionally simple and unambiguous (one card, six stat/reward/CTA zones, one CTA), and all text/colors are directly legible in the source, so layout + palette fidelity is high. Deductions: (a) the exact crest geometry (crown + sword overlap, banner fan angles) and the chest art are bespoke assets that must be authored/sourced to fully match (~ -4); (b) god-ray and sparkle particle tuning is interpretive within the stated budget (~ -2); (c) the precise count-up SFX/curve and the "shine sweep" cadence are inferred, not measured (~ -1). None affect structural fidelity; all are art/polish variables.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/VictoryScreenDesign.png`, 1908×824) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header includes spec number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry expressed as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed rules stated.
- [x] Every visible label/number recorded verbatim (`VICTORY`, `The enemy statue has fallen!`, `1746`, `05:28`, `REWARD`, `CONTINUE`) — nothing invented.
- [x] Colors given as forensic hex ranges sampled from the art.
- [x] Full ASCII node tree + per-node Unity table (parent/order/type/anchor/pivot/alignment/safe-area/responsive).
- [x] Rich results-screen reveal timeline with count-up + tap-to-skip.
- [x] §12 boundary honored (UI read-only; reward display-only; no balance mutation).
- [x] Negative rules + ≥95% acceptance criteria + confidence rationale included.
- [x] No code, no asset/scene/prefab changes, no gameplay/ECS edits, no commit.
