# BULWARK — UI CONSTRUCTION SPEC · 05 · Main Menu
Source: design/MainMenuDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

> Note: the mockup shows a placeholder wordmark **"STICK EMPIRE / RISE"** with stick-figure heroes — these are **placeholder art** (the 00/01 context establishes BULWARK as the real product with no real brand text / no stick figures in final art). This forensic spec records the *layout, structure, colors, and treatment exactly as shown*; the wordmark and hero silhouettes are flagged as placeholder content to be replaced by BULWARK branding, but their **position, size, and styling are reproduced faithfully**.

---

## A — SCREEN PURPOSE
The **Main Menu** is the root hub the player lands on after Loading (no login). It is mission control: a large logo, a vertical stack of primary destination buttons, the currency HUD, a right-edge feature rail, and a bottom row of live-ops shortcuts, all over a heroic kingdom backdrop with the player's champions posed left.

- **What it is:** the hub. Center-right **primary button column** (PLAY, CAMPAIGN, ONLINE BATTLE, CHESTS, STORE); **logo** top-center-right; **currency pills** (Gold + Gems) top-right; **right vertical rail** of secondary icons (Quests, Units, Clan, Leaderboard, Settings); **bottom live-ops row** (Daily Reward, Lucky Spin, Free Rewards, Events); **hero characters** posed lower-left over a castle/army backdrop.
- **When it appears:** immediately after Loading; returned to after exiting any sub-screen/match.
- **Emotional state to evoke:** command, ownership, anticipation — "my empire, my army, let's play." Inviting and dense-with-possibility but with one obvious next tap (PLAY).
- **What the player does:** tap PLAY (→ Mode Select) primarily; or branch to any hub destination.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** the brightest, most "daylit hopeful" screen — a sunny besieged-but-holding kingdom under a blue sky (contrast to the dark boot frames). Triumphant, busy, alive.
- **Atmosphere:** bright blue sky with clouds; a sunlit fortress city on the left with a distant ongoing battle/dragons; the player's heroes posed heroically lower-left; warm key light from upper-left.
- **Visual hierarchy:** (1) Logo top → (2) **PLAY** button (brightest, gold/orange, top of the column) → (3) the rest of the button column → (4) currencies top-right → (5) right rail + bottom live-ops → (6) hero characters (flavor, left).
- **Color psychology:** the button column is a deliberate rainbow of function — **gold/orange PLAY** (go/primary), **blue CAMPAIGN** (Iron Pact/story), **green ONLINE BATTLE** (live/competitive), **purple CHESTS** (premium/loot), **red STORE** (spend/urgent). Each color teaches the destination at a glance. Gold currencies + gems top-right.
- **Material identity:** glossy beveled gem-like buttons with gold trim + leading icon; bright matte-painted background; ornate gold currency pills; bronze rail icons in stone tabs.
- **Lighting:** outdoor daylight key + sky bounce; gold rim on the logo and button bevels; soft focal glow on PLAY.
- **Contrast philosophy:** the colored buttons pop off the bright background via gold trim + dark inner shadow; PLAY is the warmest/brightest and sits at the top of the stack as the primary.

---

## C — SCREEN DECOMPOSITION (full node tree)
```
MainMenuScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base            (Image: sunlit kingdom + heroes + distant battle baked)
    │   ├── Vignette_Overlay       (Image: gentle vignette)
    │   ├── GodRay_Overlay         (Image: soft upper-left light shafts)    [FX]
    │   └── Grain_Overlay          (Image: faint grain)                      [FX]
    ├── TopBar_Group               (currency HUD, top-right)
    │   ├── Currency_Gold          (pill: coin icon + "3520" + green "+")
    │   │   ├── GoldIcon  GoldValue("3520")  GoldPlus(green +)
    │   └── Currency_Gems          (pill: gem icon + "48750" + green "+")
    │       ├── GemIcon   GemValue("48750")  GemPlus(green +)
    ├── Logo_Group                 (top-center-right brand)
    │   ├── Logo_Wordmark          (Image/Text: "STICK EMPIRE" placeholder)
    │   └── Logo_RiseBanner        (Image: red "RISE" ribbon under wordmark)
    ├── PrimaryButtons_Column      (vertical stack, center-right)
    │   ├── Btn_Play               (gold/orange: crossed-swords icon + "PLAY")
    │   ├── Btn_Campaign           (blue: book/flag icon + "CAMPAIGN")
    │   ├── Btn_OnlineBattle       (green: crossed-swords/VS icon + "ONLINE BATTLE")
    │   ├── Btn_Chests             (purple: chest icon + "CHESTS")
    │   └── Btn_Store              (red: cart icon + "STORE")
    ├── RightRail_Group            (vertical icon rail, right edge)
    │   ├── Rail_Quests            (icon + "QUESTS")
    │   ├── Rail_Units             (icon + "UNITS")
    │   ├── Rail_Clan              (icon + "CLAN")
    │   ├── Rail_Leaderboard       (icon + "LEADERBOARD")
    │   └── Rail_Settings          (gear icon + "SETTINGS")
    ├── BottomRow_Group            (live-ops shortcuts, bottom-left band)
    │   ├── Live_DailyReward       (gift icon + "DAILY REWARD" + badge)
    │   ├── Live_LuckySpin         (wheel icon + "LUCKY SPIN")
    │   ├── Live_FreeRewards       (play/ad icon + "FREE REWARDS")
    │   └── Live_Events            (VS/banner icon + "EVENTS")
    └── HeroChars_Anchor           (optional FX anchor over baked heroes; idle shimmer)
```

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| MainMenuScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills |
| Root_SafeArea | MainMenuScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds all interactive |
| BG_Layer | MainMenuScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette/GodRay/Grain | BG_Layer | 1–3 | Image | stretch-all / top-left | — | — | ignores | full-bleed |
| TopBar_Group | Root_SafeArea | 0 | RectTransform + HorizontalLayout(right) | top-right | 1,1 | right | inside safe | pinned top-right |
| Currency_Gold | TopBar_Group | 0 | Button(pill) + HLayout | — | 0.5,0.5 | center | — | — |
| Currency_Gems | TopBar_Group | 1 | Button(pill) + HLayout | — | 0.5,0.5 | center | — | — |
| Logo_Group | Root_SafeArea | 1 | RectTransform | top-center (biased right) | 0.5,1 | center | inside safe | scales w/ H |
| Logo_Wordmark | Logo_Group | 0 | Image/Text(TMP) | top-center | 0.5,1 | center | — | — |
| Logo_RiseBanner | Logo_Group | 1 | Image | below wordmark | 0.5,1 | center | — | — |
| PrimaryButtons_Column | Root_SafeArea | 2 | VerticalLayoutGroup + spacing | center-right | 1,0.5 | right | inside safe | right-anchored, height-scaled |
| Btn_Play | PrimaryButtons_Column | 0 | Button + HLayout(icon+label) | stretch-x (within col) | 0.5,0.5 | center | — | fixed col width |
| Btn_Campaign | " | 1 | Button + HLayout | " | 0.5,0.5 | center | — | — |
| Btn_OnlineBattle | " | 2 | Button + HLayout | " | 0.5,0.5 | center | — | — |
| Btn_Chests | " | 3 | Button + HLayout | " | 0.5,0.5 | center | — | — |
| Btn_Store | " | 4 | Button + HLayout | " | 0.5,0.5 | center | — | — |
| RightRail_Group | Root_SafeArea | 3 | VerticalLayoutGroup | mid-right edge | 1,0.5 | right | inside safe | pinned right edge |
| Rail_* (×5) | RightRail_Group | 0–4 | Button + (icon over caption) | — | 0.5,0.5 | center | — | — |
| BottomRow_Group | Root_SafeArea | 4 | HorizontalLayoutGroup | bottom-left | 0,0 | left | inside safe | pinned bottom-left |
| Live_* (×4) | BottomRow_Group | 0–3 | Button + (icon over caption) | — | 0.5,0.5 | center | — | — |
| HeroChars_Anchor | Root_SafeArea | 5 | RectTransform (FX only) | bottom-left | 0,0 | left | — | over baked heroes |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** scale-to-cover; bleeds under cutout. Heroes baked lower-left; fortress left; sky upper.

**Primary button column (center-right).** Forensic vertical scan (single column at fx≈0.62) gives the band centers/colors:
- **Btn_Play (gold/orange):** band fy ≈ **0.339–0.378** (center ≈ 0.355); fill #e8a31f/#eba621 (orange-gold). Width fx ≈ **0.185–0.751** (≈ 0.566 W ≈ 1324 px). This is the **widest/topmost** button. Leading **crossed-swords** icon, label "PLAY".
- **Btn_Campaign (blue):** band fy ≈ **0.441–0.519** (center ≈ 0.49); fill #0048bc. The button is **right-aligned** to the column's right edge and **narrower than PLAY**, stepping inward at the left (its left edge fx ≈ 0.314, right edge fx ≈ 0.840). Leading book/flag icon, "CAMPAIGN".
- **Btn_OnlineBattle (green):** band fy ≈ **0.548–0.626** (center ≈ 0.59); fill #0d4310/#145119 (deep green). Right-aligned, similar narrower width. Leading crossed-swords/VS icon, "ONLINE BATTLE".
- **Btn_Chests (purple):** band fy ≈ **0.655–0.724** (center ≈ 0.69); fill #6427ae/#521b91 (amethyst). Leading chest icon, "CHESTS".
- **Btn_Store (red):** band fy ≈ **0.753–0.831** (center ≈ 0.79); fill #a72216 (oxblood-red). Leading cart icon, "STORE".
- **Button height:** each ≈ **0.078 × 1080 ≈ 84 px** tall; **inter-button gap ≈ 0.022 × 1080 ≈ 24 px** (bands above show ≈0.06 center-to-center spacing).
- **Column layout note:** PLAY is full-width and centered-ish; the four below are **right-aligned and ~7% narrower**, creating a subtle "PLAY is primary, hero of the stack" emphasis. All buttons share the same **right edge ≈ fx 0.84** except PLAY which extends further left to ≈ 0.185.

**Logo (top-center-right):**
- **Logo_Wordmark:** centered over the button column's horizontal span, top edge fy ≈ 0.06, the wordmark block spans fx ≈ 0.44–0.78, height ≈ 0.14 × 1080 ≈ 151 px (two-line lockup).
- **Logo_RiseBanner:** red ribbon centered under the wordmark, fy ≈ 0.22–0.27, width ≈ 0.16 W.

**Currency HUD (top-right):**
- Two pills right-anchored; the Gold pill left of the Gems pill.
- **Gold pill:** coin icon + value **"3520"** + a small green "+"; center fy ≈ 0.06, the pill right edge ≈ fx 0.73.
- **Gems pill:** purple-crystal icon + value **"48750"** + green "+"; right edge ≈ fx 0.875 (green + sampled #2a7c29 confirms the plus badge).
- Each pill ≈ **0.055 × 1080 ≈ 60 px** tall; value text ≈ 0.030 × 1080 ≈ 32 px.

**Right rail (right edge, vertical):**
- Five stacked icon-tabs (Quests, Units, Clan, Leaderboard, Settings), top→bottom, centered on fx ≈ **0.955**, spanning fy ≈ 0.15–0.62, each ≈ 0.075 H tall (icon ≈ 0.055 H + tiny caption). Icons read as bronze/stone tabs (sampled greys/steel #7890b2/#66697a confirm metal tabs).

**Bottom live-ops row (bottom-left band):**
- Four shortcuts left→right: **Daily Reward, Lucky Spin, Free Rewards, Events**, centered on fy ≈ 0.90, at fx ≈ 0.05 / 0.16 / 0.27 / 0.38. Each = round/badged icon (≈ 0.09 × 1080 ≈ 97 px) over a small caption; Daily Reward may carry a red notification badge. (Free Rewards sampled purple #8b23f3 = its play/gem accent.)

**16:9 tablet:** column + rail + currencies are height-anchored and edge-pinned, so they hold; the bright background crops width (less of the left fortress shown). Keep PLAY widest.

**Ultrawide:** background reveals more left/right; the button column stays right-of-center at fixed fractional width; rail pins to the (inset) right safe edge; bottom row pins to the left safe edge; currencies to the top-right safe corner.

**Notch behavior:** background bleeds under the cutout. The **right rail and currencies are the elements nearest a right-side landscape notch** — they live in `Root_SafeArea` so the SafeAreaFitter insets them clear of the cutout (critical here). Bottom row insets from the left.

---

## F — TYPOGRAPHY SPECIFICATION
- **Logo_Wordmark "STICK EMPIRE" (placeholder):** heavy serif display, UPPERCASE, dimensional gold bevel #f0d27a→#6b5320 with a dark outline + soft bloom; two-tier lockup. Cap height ≈ 0.09 × 1080 ≈ 97 px. (Replace text with BULWARK branding; keep treatment + position.)
- **Logo_RiseBanner "RISE":** UPPERCASE serif on a red ribbon, cream/gold letters #e8d5a8 with red ribbon #7a1f1a behind; ≈ 0.04 × 1080 ≈ 43 px.
- **Primary button labels (PLAY / CAMPAIGN / ONLINE BATTLE / CHESTS / STORE):** bold semi-condensed sans or heavy slab, **UPPERCASE**, tracking +4%, near-white **#fdf6e8** with a thin dark stroke + drop shadow for contrast on the colored fills; cap height ≈ **0.040 × 1080 ≈ 43 px** (PLAY may be ≈ +10% larger as the primary). Left-padded after each leading icon.
- **Currency values "3520" / "48750":** clean bold tabular numerals, near-white **#ffffff** with a thin dark shadow; ≈ **32 px**. The "+" badges are green **#2fae3a** on a small disc.
- **Rail captions (QUESTS/UNITS/CLAN/LEADERBOARD/SETTINGS):** tiny caps, tracking +6%, parchment-gold ≈ **16 px**, under each icon.
- **Live-ops captions (DAILY REWARD/LUCKY SPIN/FREE REWARDS/EVENTS):** tiny caps ≈ **16 px**, cream, under each icon.
- **Hierarchy:** Logo 97 > button labels 43 > currency 32 > captions 16. PLAY label is the largest button label.

---

## G — MATERIAL SPECIFICATION
- **Buttons (glossy gem-beveled):** each has a vivid top-sheen, a darker lower body, and a **gold/bronze beveled rim** with engraved corner rivets, plus a dark inner shadow that lifts it off the bright bg.
  - PLAY: orange-gold #f5af15→#e0860 1 body, brightest sheen, strongest focal glow.
  - CAMPAIGN: royal blue #056fdb→#003a9e.
  - ONLINE BATTLE: emerald #1c8a3a→#0d4310.
  - CHESTS: amethyst #7a3fd0→#521b91.
  - STORE: oxblood red #c8351f→#7a1f1a.
- **Leading icons:** gold/cream glyphs on a small dark inset disc within each button.
- **Currency pills:** dark stone capsule #1a1f29 with a gold rim; gold coin (#f0d27a) / amethyst gem (#9e6bf0) icon; green "+" disc.
- **Rail tabs:** aged stone/bronze tabs #3a3f4a with bronze line icons #b08a3e and a subtle bevel.
- **Live-ops icons:** colorful rounded badges (gift = red/gold, spin = multicolor wheel, free = green/purple play, events = red VS) each with a gold ring.
- **Background:** bright daylight matte painting — sky #4f8bff→#9fc4ff up top, sunlit stone fortress, green-brown terrain, distant smoke; gentle vignette only (this is the brightest screen).
- **Logo:** dimensional cast gold with red ribbon; heavy bevel + bloom.

---

## H — COMPONENT SPECIFICATION
**Primary buttons (×5).** Shared structure: leading icon disc + UPPERCASE label on a beveled colored gem-pill.
- **Idle:** vivid color, top sheen, gold rim; PLAY also has a soft pulsing focal glow.
- **Hover:** brightness +8%, rim glow widens, slight scale 1.02.
- **Pressed:** scale 0.97 + body dip −10% + brief inner flash; icon nudges.
- **Disabled:** desaturate to grey-tinted, rim dull, glow off (e.g., a locked mode).
- **Selected:** n/a (navigational, not toggles).
- **Feedback:** press → route to destination (PLAY→Mode Select; CAMPAIGN→Campaign Map; ONLINE BATTLE→Online Battle; CHESTS→Chests; STORE→Store).

**Currency pills (×2):**
- Purpose: show balance + shortcut to acquire (the green "+").
- Idle/Hover/Pressed: pill brightens on hover; the "+" disc has its own hover/press; tapping "+" → Store (gold/gem tab).
- Values update with animated roll-up when balance changes.

**Right rail (×5) & Live-ops (×4):**
- Idle: icon + caption; some carry **notification badges** (red dot/number) when content is available (e.g., Quests, Daily Reward).
- Hover: icon brighten + caption glow + slight scale.
- Pressed: scale 0.95.
- Feedback: route to the respective screen; badge clears on visit.

---

## I — ANIMATION TIMELINE (entrance)
- **t=0.00:** CanvasGroup 0; background scale 1.04.
- **t=0.00 → 0.45 s:** background fade in + slow Ken-Burns push-out.
- **t=0.25 → 0.65 s:** Logo drops/scales in 0.92→1.00 [ease-out-back] with a gold light sweep; RISE ribbon snaps in just after with a tiny flag flutter.
- **t=0.45 → 1.05 s:** primary buttons cascade in from the right, top→bottom (PLAY→CAMPAIGN→ONLINE→CHESTS→STORE), each slide-from-right + fade over ≈0.14 s, staggered ≈0.07 s [ease-out]; PLAY gets an extra glow flare on arrival.
- **t=0.40 → 0.70 s (parallel):** currency pills slide in from top-right; values roll up from 0 to "3520"/"48750".
- **t=0.60 → 1.10 s (parallel):** right rail items fade/slide in from the right edge, staggered; badges pop after.
- **t=0.80 → 1.20 s (parallel):** bottom live-ops icons pop in left→right with a small bounce; any badge pulses.
- **t≥1.2 s:** idle; PLAY glow breathes; heroes idle-shimmer.
- **Exit (on navigate):** quick fade + the chosen button flares; CanvasGroup 1→0 over 0.25 s.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **PLAY glow breathe:** the primary button's focal glow pulses slowly to keep the eye on it.
- **Logo bloom + glint:** gold logo carries a soft bloom; a specular glint sweeps it every ≈ 6 s; the RISE ribbon flutters subtly.
- **Hero shimmer/idle:** the baked heroes get a faint rim-light shimmer / occasional ambient sparkle (e.g., mage staff gem twinkle).
- **Background life:** drifting clouds, soft god-rays from upper-left, distant battle smoke wisps, rare dragon silhouette pass.
- **Currency sparkle:** tiny sparkle on the gold coin / gem icons occasionally.
- **Badge pulse:** notification badges gently pulse to draw attention.
- **Grain:** faint film grain over the whole frame.
> No gameplay particles; all ambient and looped.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** entrance (I); start FX (J); fetch/refresh balances + badge states (server-authoritative, read-only); play any pending "+currency" rollups.
- **OnPlay:** flare PLAY → push **Mode Select**.
- **OnCampaign / OnOnlineBattle / OnChests / OnStore:** push the respective screen.
- **OnRail tap:** push Quests / Units / Clan / Leaderboard / Settings; clear that badge.
- **OnLiveOps tap:** push Daily Reward / Lucky Spin / Free Rewards / Events.
- **OnCurrency "+" tap:** push Store to the matching tab.
- **OnReturn (back from a sub-screen):** re-show with a soft fade; refresh balances/badges; no full re-cascade (snap or quick fade).
- **Android back:** quit-confirm (this is the hub root).
- **OnHide:** stop FX; keep art if returning soon.

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use default Unity Button visuals or default font — buttons are beveled colored gem-pills with gold trim + leading icon.
- MUST NOT recolor the function-coded button palette (PLAY gold/orange, CAMPAIGN blue, ONLINE green, CHESTS purple, STORE red) — the colors are the navigation language.
- MUST NOT demote PLAY — it stays the topmost, widest, brightest, glowing button.
- MUST NOT drop the leading icon on any primary button, rail item, or live-ops item.
- MUST NOT omit the green "+" on the currency pills, nor the notification badges.
- MUST NOT move the rail or currencies outside the safe area (notch-critical on the right) or let the background respect the safe area.
- MUST NOT simplify the hierarchy (remove the rail, the live-ops row, or merge buttons).
- MUST NOT swap the serif logo / slab button labels to a default sans.
- MUST NOT treat the placeholder "STICK EMPIRE/RISE" or stick heroes as final — but MUST preserve their position/size/treatment when substituting BULWARK art.
- MUST NOT darken the background — this is the bright daylight hub frame.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs MainMenuDesign.png at 2340×1080: bright kingdom bg + heroes left; top logo + red RISE ribbon; gold/blue/green/purple/red primary column center-right with PLAY widest/topmost; Gold+Gems pills top-right with green "+"; five-icon right rail; four live-ops icons bottom-left.
- **Color language preserved:** the five button colors exactly as specified.
- **Hierarchy:** Logo → PLAY → column → currencies → rail/live-ops → heroes.
- **Typography:** serif logo, slab/condensed UPPERCASE button labels, tabular currency numerals; no default font.
- **Safe area:** rail + currencies inset clear of a right notch; bottom row inset from left; background bleeds.
- **Animation:** bg fade + logo drop + button cascade-from-right + currency rollup + rail/live-ops stagger; PLAY glow breathe at idle.
- **Affordance:** every button/icon has idle/hover/pressed feedback and routes correctly; badges present.

---

## N — IMPLEMENTATION CONFIDENCE
**90/100.** This is the densest of the five but every region is measured (button bands, widths, currency, rail, live-ops positions), and the components are conventional uGUI with layout groups — highly reproducible in code. −10: the beveled gem-button skins, colorful live-ops badges, ornate currency pills, and the bright kingdom matte painting + final BULWARK logo must be supplied as authored art (placeholder wordmark/heroes need replacement); the badge/balance data is server-authoritative (read-only here). Layout, cascade, and feedback are fully code-buildable.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base bright kingdom + heroes baked, scale-to-cover, bleeds under cutout; gentle vignette + god-rays + grain.
- □ Primary column center-right: PLAY(orange, fy≈0.355, widest fx0.185–0.751) → CAMPAIGN(blue, fy≈0.49) → ONLINE BATTLE(green, fy≈0.59) → CHESTS(purple, fy≈0.69) → STORE(red, fy≈0.79); ~84 px tall, ~24 px gaps; PLAY widest, four below right-aligned & narrower.
- □ Each button: beveled colored gem-pill + gold rim + leading icon + UPPERCASE label; PLAY glows.
- □ Logo top-center-right (placeholder "STICK EMPIRE"), red "RISE" ribbon below; serif gold bevel + bloom.
- □ Currency pills top-right: Gold "3520" + green "+", Gems "48750" + green "+"; tabular numerals.
- □ Right rail fx≈0.955: Quests/Units/Clan/Leaderboard/Settings, bronze/stone icon-tabs + captions + badges.
- □ Bottom live-ops fy≈0.90: Daily Reward(+badge)/Lucky Spin/Free Rewards/Events, badged icons + captions.
- □ Entrance: bg fade + logo drop + button cascade-from-right + currency rollup + rail/live-ops stagger.
- □ Idle FX: PLAY glow breathe, logo glint, hero shimmer, clouds/god-rays, badge pulse.
- □ Routing: PLAY→Mode Select, others→their screens; "+"→Store; rail/live-ops→screens; badges clear on visit.
- □ SafeAreaFitter insets rail + currencies clear of right notch; bottom row from left; BG ignores safe area.
- □ No default styling; function-color palette intact; PLAY primary; serif logo; placeholder art flagged.
