# BULWARK — UI CONSTRUCTION BIBLE (MASTER, self-contained)

**Document type:** Analysis / documentation only — **no code, no scene/prefab/asset changes, no gameplay/ECS
edits, no commits.** **Date:** 2026-06-07. **Author role:** Senior UI Technical Director (forensic
reverse-engineering of `/design`).

**Self-containment guarantee:** this single file embeds, **verbatim**, the full content of **all 40 generated
documents** — `00_CONTEXT_RECOVERY`, `01_DESIGN_INVENTORY`, and the **38 per-screen construction specs**
(`02`…`39`). A reader who has *only this file* possesses the entire specification set; no other file is needed.
The documents also exist individually under `reports/ui-construction-bible/`.

---

## How to use this bible (for the future implementation agent)
1. Read **PART 1 (Context Recovery)** for the accepted architecture, the **landscape-only** + **no-login**
   constraints, the §12 control boundary, and the **GLOBAL VISUAL DNA** every screen inherits.
2. Read **PART 2 (Design Inventory)** for the canonical 38-screen list (IDs, source files, categories).
3. For each screen, build from its **PART `NN` spec** — Sections **A–O**: Purpose · Visual DNA ·
   Decomposition (node tree) · Unity Hierarchy · Layout Mathematics · Typography · Materials · Components ·
   Animation Timeline · Particle/FX · Event Behavior · Negative Rules · Acceptance Criteria · Implementation
   Confidence · Self-Checklist.
4. Build **code-built uGUI** on the `UiRouter` shell at **CanvasScaler 2340×1080, match-height**, with a
   `SafeAreaFitter` on interactive roots. Reconstruct **exactly** what each design shows (forensic; no
   redesign/optimization). Honor §12 (no gameplay/ECS/balance changes).

## Key facts (full detail in PART 1)
- **38 landscape screens.** Boot flow is **no-login**: Splash → Loading → **Main Menu directly** (Stick War
  Legacy onboarding simplicity). `04_Login` is documented but **out-of-flow**.
- **Global Visual DNA:** dark heroic high-fantasy medieval; near-black bases; **brushed gold / antique bronze**
  ornate filigree frames; **royal/cobalt blue** (Iron Pact + CTAs) vs **ember/oxblood red** (Ashen + danger);
  **violet/amethyst** (magic/premium/gems); parchment detail panels; low-key dramatic lighting + god-rays +
  focal glow + vignette + gold rim-light + magical bloom; **serif Trajan-style gold-bevel UPPERCASE** titles.
- **ADR-gated content** (Skins stat-modifiers; Chests / Chest-Open-Result / Lucky-Spin gacha) is spec'd
  forensically; the ADRs govern *implementation*, not the visual spec.

## Table of contents (40 embedded documents)
- **PART 1** — `00_CONTEXT_RECOVERY`
- **PART 2** — `01_DESIGN_INVENTORY`
- **PART 02** Splash · **03** Loading · **04** Login(out-of-flow) · **05** Main Menu · **06** Mode Select
- **07** Match Intro · **08** Battle HUD · **09** In-Match Spell HUD · **10** In-Match Banner · **11** Pause
- **12** Victory · **13** Defeat · **14** Campaign Result · **15** Endless Result · **16** Ladder Result
- **17** Store · **18** Spells · **19** Skins · **20** Chests · **21** Chest Open Result
- **22** Units/Army · **23** Commander Select · **24** Profile · **25** Battle Pass · **26** Quests · **27** Campaign Map
- **28** Daily Reward · **29** Lucky Spin · **30** Free Rewards · **31** Events Hub · **32** Online Battle · **33** Tournament Ladder
- **34** Leaderboard · **35** Clan · **36** Settings · **37** Confirm/Toast/Insufficient/NetErr (sheet) · **38** Reward Grant · **39** Network Error

---



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION BIBLE · 00 · CONTEXT RECOVERY

**Document type:** Analysis / documentation only. **No code, no scene/prefab/asset changes, no commits.**
**Date:** 2026-06-07. **Role:** Senior UI Technical Director — forensic reverse-engineering of `/design`.
**Source of truth:** the **38 images in `/design`** (prior agents' *visual* implementations are NOT assumed
correct; this bible re-derives intent from the art).

---

## 1. Project & accepted architecture (recovered)
- **Game:** BULWARK — Unity **DOTS/ECS** mobile RTS-lite (mine→train→push→topple-statue), factions **Iron
  Pact** (steel + royal/cobalt blue) vs **Ashen Horde** (ember-orange + oxblood red).
- **UI tech (accepted):** **code-built uGUI** — no prefabs/UXML; screens constructed in C# at runtime on a
  **UiRouter screen-stack shell** (`UiScreen` base, `CanvasGroup` fades, `SafeAreaFitter`). MonoBehaviour for
  UI/meta; **ECS only for the battle sim** (intentional boundary).
- **§12 control boundary (inviolable):** UI reads the ECS world read-only and may issue only
  `Training.EnqueueTrain`, `MoveDestination`, `Time.timeScale`. All meta is server-authoritative; the client
  never mutates a balance. **No gameplay/ECS/balance/AI/backend change from UI work.**
- **Font reality:** shipped UI uses Unity legacy `Text` + `LegacyRuntime.ttf`; **Roboto/serif SDF (TMP) is the
  intended upgrade** for the prestige typography the designs demand (see per-screen Section F).

## 2. Landscape-only decision (recovered)
- **Landscape is MANDATORY; portrait is removed.** Orientation locked to landscape (auto-rotate L↔R only).
- **CanvasScaler reference = 2340×1080 (≈19.5:9), `matchWidthOrHeight = 1.0` (match HEIGHT).** Height is the
  stable axis; width reveals more background on wider devices.
- **Safe area:** every interactive root insets to `Screen.safeArea` (side-notch in landscape) via a shared
  fitter; full-bleed backgrounds extend under the cutout, interactive content stays inside the safe area.
- The `/design` mockups are authored at mixed aspect ratios (1.5:1 → 2.33:1 — an art-tool artifact); **all are
  to be normalized to the 2340×1080 production canvas.** Per-screen specs give proportional (fraction-based)
  layout so they scale.

## 3. Finalized navigation (recovered) — **NO LOGIN**
- **Boot flow: Splash → Loading → Main Menu (DIRECTLY).** **No login/auth gate.** ("Stick War Legacy
  onboarding simplicity" — get the player to the hub fast; no account wall.)
- **`LoginAuthDesign.png` exists in `/design` and is therefore specified (Phase-1 rule: no screen skipped),
  but it is OUT OF THE FINALIZED FLOW** (deprecated/optional). Its spec is included for completeness and
  flagged as not-in-boot-flow.
- **Hub (Main Menu)** routes to: Play→Mode Select; Campaign; Online Battle; Chests; Store; rail
  (Quests/Units/Clan/Leaderboard/Settings); bottom (Daily/Spin/Free/Events). Match flow: Mode Select →
  (Match Intro) → Battle (HUD + spell HUD + banner + Pause) → result screen (Victory/Defeat/Campaign/Endless/
  Ladder) → hub. Shop is a tabbed hub (Spells/Skins/Chests/Store). Utility overlays (Confirm/Reward/Network
  error) float over any screen.

## 4. Constraints (recovered, binding for this bible)
1. **No login required.** 2. **Loading → Main Menu directly.** 3. **Stick War Legacy onboarding simplicity**
(minimal friction, fast to play). 4. **Landscape mandatory.** 5. **Existing gameplay untouched** (the battle
sim, BattleHud bindings, balance, AI — all read-only/unchanged; the HUD design is presentation over the same
controls). 6. Currencies displayed = **Gold + Gems** (the in-battle gold is separate, shown in the Battle HUD).

## 5. UI-relevant ADR / decision status (recovered)
- **Skins with gameplay modifiers** (SkinsDesign shows stat bonuses) — collides with the visual-only
  cosmetic-safety canon + "gems never buy power"; **requires an ADR** (recommended: Gold-only modifiers +
  ranked ClarityMode standardization). The screen is still spec'd forensically; the ADR governs *implementation*.
- **Chests + Lucky Spin** (loot-box/gacha) — collide with the "no loot boxes/gacha" principled CUT; **require
  an ADR** or a transparent redesign. Spec'd forensically here; ADR governs implementation.
- **Energy/stamina** — some mockups historically showed it; canon CUTs stamina gates. If a design shows an
  energy meter it is documented but flagged CUT.
- **GATE-1 (FUN) = FAIL** still gates *gameplay implementation*; this bible is design documentation (permitted).
- These ADR notes do **not** reduce forensic fidelity — every screen in `/design` gets a full spec.

## 6. GLOBAL VISUAL DNA (inherited by every per-screen Section B)
The whole UI shares one identity; each screen's Section B adds screen-specific nuance on top of this baseline:
- **Mood/theme:** dark heroic high-fantasy medieval war; prestige + power fantasy; "Stick War: Empire/Rise"
  lineage but original BULWARK (no real brand text, no stick figures in final art — placeholder mockups use
  them).
- **Palette:** near-black charcoal/obsidian bases (#0a0b0f–#14161e); **brushed gold / antique bronze** chrome
  (#caa04a–#f0d27a highlights, #6b5320 shadows) for frames, titles, prestige; **royal/cobalt blue** (#2b56c8–
  #4f8bff) = Iron Pact + primary CTAs; **ember/oxblood red** (#7a1f1a–#d8452b) = Ashen Horde + danger/surrender;
  **violet/amethyst purple** (#5a2db0–#9e6bf0) = magic/premium/gems; **parchment cream** (#d9c79a) for
  scroll/detail panels.
- **Material identity:** ornate cast-gold/bronze beveled frames with engraved filigree; aged stone & dark wood
  panels; parchment scrolls; glass/crystal (gems, spell orbs) with inner glow + specular; cloth banners
  (faction heraldry) with stitched trim; metal with worn edges + rim light.
- **Lighting:** dramatic low-key with warm volumetric god-rays and a focal glow on the hero element; strong
  vignette; gold rim-light on frames; magical bloom on gems/spells/chests.
- **Contrast philosophy:** dark field → luminous focal subject; gold accents reserved for the most important
  elements; faction colors signal allegiance; CTAs are the brightest interactive object on screen.
- **Visual hierarchy:** ornate Title (top-center) → central hero subject → primary CTA (bright, bottom/center)
  → currencies (top-right) → secondary chrome (back top-left, rails, tabs).
- **Typography baseline:** **serif display** (Roman/Trajan-inspired, heavy gold bevel + soft bloom) for
  titles/headers; clean semi-condensed sans or light serif for body/numbers; UPPERCASE for titles/buttons;
  drop-shadow + thin dark stroke for battlefield/over-art legibility.

## 7. Context summary
BULWARK's UI is a **landscape, code-built uGUI, dark-gold high-fantasy** front end over an untouched ECS
battle. The finalized onboarding is **frictionless (Splash→Loading→Main Menu, no login)**. `/design` now holds
**38 screens** (the previously-missing Battle HUD, Campaign Map, Tournament ladder, in-match spell HUD &
banner, and the Campaign/Endless/Ladder result screens have been added; the Clan screen is now a real clan
hub, not a Leaderboard duplicate). This bible treats `/design` as ground truth and produces a forensic
construction spec per screen (Sections A–O) so a future implementation agent can rebuild each at ≥95% fidelity
purely in Unity code, honoring the §12 boundary and the landscape/safe-area rules above.

> Next: `01_DESIGN_INVENTORY.md` (canonical screen list) → per-screen `NN_<Screen>_SPEC.md` → `99` master.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION BIBLE · 01 · DESIGN INVENTORY

**Analysis only.** Date: 2026-06-07. Every image in `/design` (38) is catalogued; **no screen skipped.**
Each has a canonical screen ID, its spec filename, source filename, resolution, aspect ratio, category, and
intended purpose. Mockups are authored at mixed aspect ratios (art-tool artifact) → **all normalize to the
2340×1080 (≈19.5:9) landscape production canvas**; per-screen specs use fraction-based layout.

## Canonical screen table

| # | Spec file | Screen ID | Source `/design` file | Res | Aspect | Category | Purpose |
|---|---|---|---|---|---|---|---|
| 02 | `02_Splash_SPEC.md` | Splash | SplashScreenDesign.png | 1915×821 | 2.33:1 | Boot | Brand title; tap/auto → Loading |
| 03 | `03_Loading_SPEC.md` | Loading | LoadingScreenDesign.png | 1915×821 | 2.33:1 | Boot | Progress + key art; **→ Main Menu directly** |
| 04 | `04_Login_SPEC.md` | Login/Auth | LoginAuthDesign.png | 1672×941 | 1.78:1 | Boot ⚠️**OUT-OF-FLOW** | Guest/social login — **deprecated (no-login constraint)**; spec'd for completeness |
| 05 | `05_MainMenu_SPEC.md` | Main Menu | MainMenuDesign.png | 1915×821 | 2.33:1 | Hub | Root hub: play/campaign/online/chests/store + rail + currencies |
| 06 | `06_ModeSelect_SPEC.md` | Mode Select | ModScreenDesign.png | 1915×821 | 2.33:1 | Mode | Pick a mode (5 cards) |
| 07 | `07_MatchIntro_SPEC.md` | Match Intro | MatchIntroDesign.png | 1536×1024 | 1.5:1 | Mode/Match | Pre-battle VS framing |
| 08 | `08_BattleHud_SPEC.md` | Battle HUD | BattleHudDesign.png | 1672×941 | 1.78:1 | In-Match | In-match HUD: statue HP, gold/pop, train, garrison/defend/attack |
| 09 | `09_InMatchSpellHud_SPEC.md` | In-Match Spell HUD | InMatchSpellHudDesign.png | 1672×941 | 1.78:1 | In-Match | Spell cast slots + cooldown/telegraph |
| 10 | `10_InMatchBanner_SPEC.md` | In-Match Banner | InMatchBannerDesign.png | 1824×862 | 2.12:1 | In-Match | Objective/event/wave banner overlay |
| 11 | `11_Pause_SPEC.md` | Pause | PauseModalDesign.png | 1782×883 | 2.02:1 | In-Match (modal) | Resume/Settings/Surrender |
| 12 | `12_Victory_SPEC.md` | Victory | VictoryScreenDesign.png | 1908×824 | 2.32:1 | Result | Win: reward + time + continue |
| 13 | `13_Defeat_SPEC.md` | Defeat | DefeatScreenDesign.png | 1915×821 | 2.33:1 | Result | Loss: retry/continue |
| 14 | `14_CampaignResult_SPEC.md` | Campaign Result | CampaignResultDesign.png | 1672×941 | 1.78:1 | Result | Campaign level clear: stars/rewards/next |
| 15 | `15_EndlessResult_SPEC.md` | Endless Result | EndlessResultDesign.png | 1672×941 | 1.78:1 | Result | Endless: waves survived/score |
| 16 | `16_LadderResult_SPEC.md` | Ladder Result | LadderResultDesign.png | 1672×941 | 1.78:1 | Result | Async ladder: rank delta/rewards |
| 17 | `17_Store_SPEC.md` | Store | StoreScreenDesign.png | 1672×941 | 1.78:1 | Shop | Gem packs/bundles/IAP + shop tabs |
| 18 | `18_Spells_SPEC.md` | Spells | SpellsScreenDesign.png | 1914×822 | 2.33:1 | Shop | Spell orbs + detail/buy |
| 19 | `19_Skins_SPEC.md` | Skins | SkinsScreenDesign.png | 1910×823 | 2.32:1 | Shop | Cosmetic sets + equip (⚠️stat-modifier ADR) |
| 20 | `20_Chests_SPEC.md` | Chests | ChestsScreenDesign.png | 1914×822 | 2.33:1 | Shop | Chest slots/timers/open (⚠️loot-box ADR) |
| 21 | `21_ChestOpenResult_SPEC.md` | Chest Open Result | ChestOpenResultDesign.png | 1536×1024 | 1.5:1 | Shop/Reward | Loot reveal (⚠️gacha ADR) |
| 22 | `22_UnitsArmy_SPEC.md` | Units / Army | UnitsArmyDesign.png | 1672×941 | 1.78:1 | Meta | Unit collection + upgrade |
| 23 | `23_CommanderSelect_SPEC.md` | Commander Select | CommanderSelectDesign.png | 1672×941 | 1.78:1 | Meta | Warden vs Warchief abilities/select |
| 24 | `24_Profile_SPEC.md` | Profile | ProfileScreenDesign.png | 1783×882 | 2.02:1 | Meta | Player profile/stats/equipped |
| 25 | `25_BattlePass_SPEC.md` | Battle Pass | BattlePassDesign.png | 1774×887 | 2.0:1 | Meta/Live-ops | Seasonal tier track free/premium |
| 26 | `26_Quests_SPEC.md` | Quests | QuestsScreenDesign.png | 1754×897 | 1.96:1 | Meta/Live-ops | Daily/weekly objectives + claim |
| 27 | `27_CampaignMap_SPEC.md` | Campaign Map | CampaignMapDesign.png | 1679×937 | 1.79:1 | Meta/Mode | Level-select world map (nodes/stars) |
| 28 | `28_DailyReward_SPEC.md` | Daily Reward | DailyRewardDesign.png | 1536×1024 | 1.5:1 | Live-ops | Login streak calendar |
| 29 | `29_LuckySpin_SPEC.md` | Lucky Spin | LuckySpinDesign.png | 1536×1024 | 1.5:1 | Live-ops | Prize wheel (⚠️gacha ADR) |
| 30 | `30_FreeRewards_SPEC.md` | Free Rewards | FreeRewardsDesign.png | 1536×1024 | 1.5:1 | Live-ops | Opt-in rewarded-ad offers |
| 31 | `31_EventsHub_SPEC.md` | Events Hub | EventsHubDesign.png | 1536×1024 | 1.5:1 | Live-ops | Limited-time events/modifiers |
| 32 | `32_OnlineBattle_SPEC.md` | Online Battle | OnlineBattleDesign.png | 1536×1024 | 1.5:1 | Competitive | Async ghost matchmaking VS |
| 33 | `33_TournamentLadder_SPEC.md` | Tournament Ladder | TournamentLadderDesign.png | 1672×941 | 1.78:1 | Competitive | Async ladder/bracket progression |
| 34 | `34_Leaderboard_SPEC.md` | Leaderboard | LeaderboardScreenDesign.png | 1782×883 | 2.02:1 | Competitive | Global/Friends/Season ranking |
| 35 | `35_Clan_SPEC.md` | Clan | ClanScreenDesign.png | 1829×860 | 2.13:1 | Social | Clan hub: members/chat/war/chest (now REAL — defect fixed) |
| 36 | `36_Settings_SPEC.md` | Settings | SettingsScreenDesign.png | 1915×821 | 2.33:1 | System | Audio/graphics/account/options |
| 37 | `37_ConfirmModal_SPEC.md` | Confirm/Toast/Insufficient/NetErr (sheet) | ConfirmModalDesign.png | 1536×1024 | 1.5:1 | Utility | 4-in-1 reusable modal sheet |
| 38 | `38_RewardGrant_SPEC.md` | Reward Grant | RewardGrantDesign.png | 1536×1024 | 1.5:1 | Utility | "You received" reward popup |
| 39 | `39_NetworkError_SPEC.md` | Network Error | NetworkErrorDesign.png | 1536×1024 | 1.5:1 | Utility | Connection-lost / retry |

## Notes
- **38 screens total** (02–39). The 9 designs added since the prior freeze: BattleHud, InMatchSpellHud,
  InMatchBanner, CampaignMap, CampaignResult, EndlessResult, LadderResult, TournamentLadder, and a
  **regenerated Clan** (now a real clan hub — the earlier Leaderboard-duplicate defect is resolved, verified).
- **`04_Login`** is documented but **excluded from the finalized boot flow** (no-login constraint:
  Splash→Loading→Main Menu directly). Spec'd per the no-skip rule; flagged out-of-flow.
- **ADR-gated content** (Skins stat-modifiers, Chests/ChestOpenResult/Lucky Spin gacha) is spec'd forensically
  here; the ADRs govern *implementation*, not the visual spec.
- Categories drive grouping; the boot/match/result/shop/meta/live-ops/competitive/social/system/utility split
  matches the navigation in `00 §3`.

> Next: per-screen `NN_<Screen>_SPEC.md` (Sections A–O each), then `99_UI_CONSTRUCTION_BIBLE.md` (embeds all).



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 02 · Splash
Source: design/SplashScreenDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

---

## A — SCREEN PURPOSE
The **Splash** is the very first frame after the Unity boot logo. It establishes the BULWARK brand fantasy in one held cinematic image and acts as the "press-any-key" gate into Loading. In the finalized **no-login** flow it is screen #1 of the boot chain: **Splash → Loading → Main Menu**.

- **What it is:** a full-bleed cinematic key-art establishing shot with a central ornate gold **brand plaque** (where the wordmark/logo will render) and a single low call-to-action line: **"TAP TO BEGIN"**.
- **When it appears:** immediately after the engine splash, before any asset preload UI. It either auto-advances after a short hold or advances on the first tap anywhere on screen.
- **Emotional state to evoke:** awe + foreboding + invitation. The player should feel they are standing on the rim of a vast medieval war about to begin — a lone king on the left, a burning enemy horizon on the right, two empires colliding. Quiet anticipation, not action yet.
- **What the player does:** taps anywhere (or waits) → transition to Loading.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** the most cinematic, "movie poster" frame in the product — pure atmosphere, almost no chrome. A single wide matte-painting vista of a war-torn kingdom at a smoky amber dusk.
- **Atmosphere:** heavy volumetric haze; warm god-rays breaking from the right; a cool desaturated blue-grey LEFT (Iron Pact territory, dawn-cold steel) vs a hot ember-orange RIGHT (Ashen Horde, the burning enemy capital). The image reads as a left-cool / right-warm temperature split across a central dark void where the plaque sits.
- **Visual hierarchy:** (1) central gold plaque (brand) → (2) "TAP TO BEGIN" line → (3) the lone armored king silhouette far left with the blue banner → (4) the burning distant city + dragon silhouettes right → (5) the dark trampled battlefield foreground.
- **Color psychology:** cobalt/steel-blue left = the player's noble faction (order, sanctuary); oxblood/ember right = the threat (danger, the thing you march toward). Gold plaque = the crown/prize. Near-black center = focus rest, lets the future logo pop.
- **Material identity:** matte painted environment (no UI panels except the plaque); the plaque itself is the global cast-gold ornate beveled filigree frame over a near-black obsidian field.
- **Lighting:** low-key dusk; key light is the warm amber blowout on the right horizon; rim light catches the left king's pauldrons and banner pole in cool steel; strong vignette darkens all four corners; the plaque carries a soft warm focal bloom.
- **Contrast philosophy:** the brightest things are the right-horizon amber sky and the gold plaque edge; everything else falls into shadow. The CTA text is the brightest *interactive* affordance at the bottom.

---

## C — SCREEN DECOMPOSITION (full node tree)
```
SplashScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)   ← interactive layer only
    ├── BG_Layer (full-bleed, IGNORES safe area — extends under notch)
    │   ├── KeyArt_Base            (Image: the full matte painting)
    │   ├── Vignette_Overlay       (Image: radial dark vignette, multiply)
    │   ├── GodRay_Overlay         (Image: warm right-side light shafts, additive)  [optional FX layer]
    │   └── Grain_Overlay          (Image: faint film grain, low alpha)             [optional FX layer]
    ├── BrandPlaque_Group          (centered ornate frame cluster)
    │   ├── Plaque_Frame           (Image: ornate cast-gold beveled filigree border)
    │   ├── Plaque_Field           (Image: near-black obsidian interior fill)
    │   ├── Plaque_TopFinial       (Image: gold cross/star finial centered on top edge)
    │   ├── Plaque_BottomFinial    (Image: gold finial centered on bottom edge)
    │   └── Brand_Logo             (Image OR Text(TMP): BULWARK wordmark — placeholder field, empty in mock)
    ├── CTA_Group                  (bottom-center call to action)
    │   ├── CTA_Label              (Text(TMP): "TAP TO BEGIN")
    │   ├── CTA_OrnamentLeft       (Image: small gold flourish glyph left of text)
    │   └── CTA_OrnamentRight      (Image: small gold flourish glyph right of text)
    └── TapCatcher                 (Button, full-screen invisible — advances to Loading)
```
> The two armored figures, banners, dragons, castles, and battlefield are **painted into `KeyArt_Base`**, not separate nodes. Do not attempt to composite them as discrete sprites unless layered source art is provided; the spec treats the vista as one baked image.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Child order | Unity type | Anchor preset | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| SplashScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills canvas |
| Root_SafeArea | SplashScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets to safeArea** | holds CTA + finials |
| BG_Layer | SplashScreen | 0 (behind SafeArea) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES safe area** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover (preserve aspect, crop) |
| Vignette_Overlay | BG_Layer | 1 | Image | stretch-all | 0.5,0.5 | fill | ignores | full-bleed |
| GodRay_Overlay | BG_Layer | 2 | Image (additive mat) | top-right anchored, large | 1,1 | upper-right | ignores | anchored right edge |
| Grain_Overlay | BG_Layer | 3 | Image (tiled, low α) | stretch-all | 0.5,0.5 | fill | ignores | tiled |
| BrandPlaque_Group | Root_SafeArea | 0 | RectTransform | center | 0.5,0.5 | center | inside safe | scales with height |
| Plaque_Frame | BrandPlaque_Group | 0 | Image (9-sliced ornate) | stretch-all | 0.5,0.5 | center | — | 9-slice, no corner stretch |
| Plaque_Field | BrandPlaque_Group | 1 (behind frame edge) | Image | stretch-all (inset) | 0.5,0.5 | center | — | fills inside frame |
| Plaque_TopFinial | BrandPlaque_Group | 2 | Image | top-center | 0.5,0.5 | center | — | pinned top edge |
| Plaque_BottomFinial | BrandPlaque_Group | 3 | Image | bottom-center | 0.5,0.5 | center | — | pinned bottom edge |
| Brand_Logo | BrandPlaque_Group | 4 | Image/Text(TMP) | center | 0.5,0.5 | center | — | scales inside field |
| CTA_Group | Root_SafeArea | 1 | RectTransform + HorizontalLayoutGroup | bottom-center | 0.5,0 | center | inside safe | pinned to safe bottom |
| CTA_OrnamentLeft | CTA_Group | 0 | Image | mid-left | 0.5,0.5 | center | — | — |
| CTA_Label | CTA_Group | 1 | Text(TMP) | center | 0.5,0.5 | center | — | — |
| CTA_OrnamentRight | CTA_Group | 2 | Image | mid-right | 0.5,0.5 | center | — | — |
| TapCatcher | Root_SafeArea | 2 (topmost) | Button (transparent Image, raycast on) | stretch-all | 0.5,0.5 | fill | inside safe | full-screen |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** `KeyArt_Base` scale-to-**cover** the 2340×1080 frame (preserve aspect, crop overflow), centered. Source is 2.33:1 ≈ the production 2.167:1, so minimal crop top/bottom. Background ignores safe area and bleeds fully under any side cutout.

**Brand plaque (central focal):** In the mock the ornate plaque is horizontally centered and sits in the upper-middle. Forensic bounds (normalized):
- **Plaque outer width ≈ 0.41 × 2340 ≈ 960 px**, centered (left edge fx ≈ 0.295, right edge fx ≈ 0.705).
- **Plaque outer height ≈ 0.40 × 1080 ≈ 432 px**, top edge fy ≈ 0.045 (≈ 49 px from top), bottom edge fy ≈ 0.445 (≈ 480 px).
- **Plaque center:** screen-X 0.50, screen-Y ≈ 0.245 (slightly above vertical center — sits in the upper-middle, leaving the lower third for the battlefield and CTA).
- **Frame border thickness:** ≈ 0.018 × 1080 ≈ 19 px of cast-gold filigree on each side (corner ornaments larger, ≈ 34 px).
- **Top & bottom finials:** centered cross/star finials overhang the frame edge by ≈ 0.012 × 1080 ≈ 13 px outward.
- **Plaque_Field inset:** the obsidian interior is inset ≈ 19 px inside the frame on all sides.
- **Brand_Logo box:** centered, ≈ 0.78 × plaque-width × 0.55 × plaque-height (the empty interior where the wordmark renders).

**CTA line:** centered horizontally; baseline at **fy ≈ 0.90** (≈ 972 px), i.e. ≈ 0.10 × 1080 ≈ 108 px above the safe-area bottom. Text height ≈ 0.030 × 1080 ≈ 32 px. Ornament glyphs sit ≈ 14 px to each side of the text block with ≈ 10 px gap, each ≈ 0.022 × 1080 ≈ 24 px tall.

**TapCatcher:** fills the entire safe area (the whole screen is tappable).

**16:9 tablet adaptation:** height stays the dominant axis (matchHeight=1.0). Background reveals less width → side painting (left king / right city) crops inward slightly; plaque + CTA unaffected (height-anchored). Keep plaque centered; never let the side figures be cut by the plaque.

**Ultrawide (21:9+):** more background width revealed left and right; plaque stays centered at fixed proportional width; CTA stays centered. No element stretches.

**Notch behavior:** background ignores the cutout and bleeds under it; plaque and CTA live in `Root_SafeArea` so they never collide with a landscape side-notch. Because both focal elements are horizontally centered, a side cutout never overlaps them.

---

## F — TYPOGRAPHY SPECIFICATION
- **Brand_Logo (placeholder wordmark — empty in mock):** intended serif display, Roman/Trajan-inspired, heavy weight, UPPERCASE, letter-tracking +4%, heavy gold bevel (#f0d27a highlight → #6b5320 shadow) with soft warm outer bloom and a thin dark stroke (≈ 2 px, #1a1206) for legibility on the obsidian field. Cap height ≈ 0.18 × 1080 ≈ 195 px if a single word. (Mock shows an empty plaque; record as placeholder, do not invent brand text.)
- **CTA_Label "TAP TO BEGIN":**
  - Font: light serif display or refined semi-condensed serif, **letter-spaced wide** (tracking +12–16%), **UPPERCASE**.
  - Weight: regular/medium (deliberately understated vs a button).
  - Size: cap height ≈ 0.030 × 1080 ≈ **32 px**.
  - Color: warm parchment gold **#e8d5a8 → #caa04a** subtle vertical gradient.
  - Glow: soft warm outer glow (≈ 6 px, #caa04a @ 35%) for a gentle pulsing affordance; thin dark drop shadow (offset 0,−2, #000 @ 50%) for contrast over the battlefield.
  - No box/pill behind it — it floats over the painting.
- **Hierarchy:** Logo (≈195 px) ≫ CTA (≈32 px). Only two text runs exist on this screen.

---

## G — MATERIAL SPECIFICATION
- **Plaque_Frame (cast gold filigree):** highlight #f0d27a / mid #caa04a / shadow #6b5320; medium-low roughness with crisp specular hits on the bevel ridges; ornate engraved scrollwork on the corners and top/bottom finials; faint warm rim-bloom. Edge treatment: rounded beveled molding, NOT a flat stroke.
- **Plaque_Field (obsidian interior):** near-black **#151614–#181819** (sampled center #181819) with a subtle top-lit vertical gradient (slightly lighter at top), faint inner shadow from the frame, very low sheen.
- **KeyArt_Base sky (left → right):** cool grey-violet dusk left **#39313b**; warming through **#683e41** to a hot ember blowout right **#bb452f** near the burning horizon. Distant right city glow pushes toward #d8452b.
- **Left king / armor:** desaturated steel with cool blue rim; cloak deep cobalt-charcoal (#2b2a35 range under the haze); blue banner cloth muted royal blue with stitched trim.
- **Right enemy:** oxblood/ember silhouettes; the far castle is a dark mass against the amber sky with internal fire glows (#7a1f1a → #d8452b).
- **Battlefield foreground:** trampled near-black earth #211a19 with scattered warm embers; strong corner vignette.
- **Vignette:** radial multiply, transparent center → #05060a at corners, ≈ 55% strength.

---

## H — COMPONENT SPECIFICATION
**TapCatcher (full-screen advance button) — the only interactive element.**
- **Purpose:** advance Splash → Loading on any tap.
- **Structure:** transparent full-screen Image with raycastTarget on, wrapped in a Button; the visible affordance is the pulsing CTA_Label.
- **States:**
  - **Idle:** invisible catcher; CTA_Label gently pulses (see I/J) to signal "tap anywhere".
  - **Hover (pointer, non-touch):** CTA_Label brightens ≈ +12% and its glow widens slightly.
  - **Pressed:** brief CTA_Label flash to near-white (#fff4dc) over ≈ 0.08 s; optional subtle full-screen warm flash.
  - **Disabled:** during the outgoing transition the catcher disables to prevent double-advance.
  - **Selected:** n/a.
- **Visual feedback:** the entire screen begins its exit transition (fade/zoom) the instant the tap registers.

---

## I — ANIMATION TIMELINE (entrance)
All times relative to OnShow t=0. Easing intent in brackets.
- **t=0.00:** CanvasGroup alpha 0; KeyArt slightly zoomed in (scale 1.06).
- **t=0.00 → 0.80 s:** full-screen fade in 0→1 [ease-out]; simultaneously KeyArt slow Ken-Burns push-out 1.06 → 1.00 [linear, continues subtly the whole time].
- **t=0.40 → 1.10 s:** BrandPlaque scales in 0.92 → 1.00 with a soft gold bloom flare on the frame edges [ease-out-back, gentle]; finials catch a quick specular sweep.
- **t=0.90 → 1.30 s:** Brand_Logo (if present) fades/bevels in with a left-to-right gold light sweep across the wordmark.
- **t=1.20 → 1.60 s:** CTA_Group fades in 0→1 [ease-out] and begins its idle pulse loop.
- **t≥1.60 s:** screen idle; auto-advance timer (if used) runs ≈ 3–5 s, or first tap advances.
- **Exit (on tap/auto):** CTA flash (0.08 s) → CanvasGroup fade 1→0 over 0.35 s with a tiny KeyArt zoom-in (1.00 → 1.03) [ease-in]; hand off to Loading.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **CTA pulse:** CTA_Label opacity oscillates 0.78 ↔ 1.00 and glow radius breathes on a slow ≈ 1.6 s sine loop (the "tap me" heartbeat).
- **God-rays:** slow warm volumetric shafts from the right horizon drift/shimmer almost imperceptibly (≈ 8 s loop) over the right third.
- **Embers:** a handful of slow upward-drifting warm ember motes over the burning right horizon and battlefield (very sparse, low alpha).
- **Plaque bloom:** the gold frame edges carry a faint continuous warm bloom; one slow specular glint travels the top edge every ≈ 6 s.
- **Banner sway:** if banners are a separate layer, a very subtle cloth sway (≈ 4 s); otherwise baked.
- **Grain:** faint animated film grain over the whole frame for a cinematic matte-painting feel.
> No gameplay particles. All FX are ambient and looped; none block input.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** play entrance timeline (I); start idle FX (J); start optional auto-advance timer; arm TapCatcher.
- **OnTap / auto-advance fires:** disable TapCatcher; play CTA flash + exit fade; request UiRouter push **Loading**.
- **OnHide:** stop FX loops; release any large key-art texture reference if memory-managed.
- **No back behavior** (this is the first screen); Android back here should quit-confirm or be ignored per platform policy (not depicted in mock).

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use default Unity UI styling (no flat grey Button, no default font on the CTA).
- MUST NOT swap the serif CTA/wordmark to a sans-serif.
- MUST NOT add a visible button box/pill behind "TAP TO BEGIN" — it floats over the art.
- MUST NOT recolor the temperature split (cool-left / warm-right) or symmetrize it.
- MUST NOT place the plaque off-center or resize it so it crops the side figures.
- MUST NOT let interactive content (plaque/CTA) sit outside the safe area, nor let the background respect the safe area (it must bleed full).
- MUST NOT omit the vignette, god-rays, ember drift, or CTA pulse.
- MUST NOT invent brand text inside the plaque — it is an empty gold plaque in the mock; treat the wordmark as a placeholder field.
- MUST NOT add any login/account UI (no-login flow).
- MUST NOT approximate the gold frame as a flat 1-px stroke — it is a beveled ornate molding with corner finials.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs SplashScreenDesign.png at 2340×1080: same cinematic vista, same cool-left/warm-right split, same centered ornate gold plaque in the upper-middle, same single "TAP TO BEGIN" line low-center.
- **Hierarchy preserved:** plaque is the dominant focal object; CTA is the only conspicuous interactive affordance; no extra chrome.
- **Typography:** CTA is wide-tracked UPPERCASE serif in parchment-gold with soft glow + dark shadow; not a default font, not a sans.
- **Safe area:** plaque + CTA inside safe area; background bleeds under the cutout.
- **Eye flow:** plaque → CTA → side figures → burning horizon, matching B.
- **Animation:** fade-in + Ken-Burns + plaque scale-in + CTA pulse all present; tap triggers flash + fade to Loading.
- **Interactive affordance:** tapping anywhere advances; CTA pulses to invite the tap.

---

## N — IMPLEMENTATION CONFIDENCE
**92/100.** The screen is compositionally simple (one baked vista + one plaque + one CTA + one full-screen tap), so layout/animation are highly reproducible. The −8 is because the heavy lift is *art*: the cinematic matte painting and the ornate cast-gold plaque must be supplied as authored textures to hit ≥95% — code-built uGUI can place and animate them precisely but cannot generate the painting. The empty wordmark plaque also leaves the final logo treatment to be confirmed.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base imported and scaled to-cover, centered, bleeding under cutout.
- □ Vignette + god-ray + grain overlays present at correct blend/strength.
- □ Plaque centered at fx0.50 / fy≈0.245, width ≈0.41W, height ≈0.40H, ornate beveled gold frame ≈19 px + corner finials.
- □ Plaque_Field obsidian #151614–#181819 with subtle top-lit gradient.
- □ Top & bottom centered gold finials overhanging the frame.
- □ Brand_Logo placeholder centered in field (no invented text).
- □ CTA "TAP TO BEGIN" centered, baseline fy≈0.90, serif UPPERCASE wide-tracked parchment-gold, glow+shadow, with side flourishes.
- □ Full-screen TapCatcher Button on top, raycast enabled.
- □ Entrance timeline (fade + Ken-Burns + plaque scale-in + logo sweep + CTA fade) implemented.
- □ Idle FX: CTA pulse, god-ray shimmer, ember drift, plaque glint.
- □ Tap/auto → CTA flash + fade → push Loading; catcher disables during exit.
- □ Safe-area fitter on Root_SafeArea; BG_Layer ignores safe area.
- □ No default Unity styling; no sans swap; no login UI.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 03 · Loading
Source: design/LoadingScreenDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

---

## A — SCREEN PURPOSE
The **Loading** screen covers asset/scene preload between Splash and the destination. In the finalized **no-login** boot chain it advances **directly to the Main Menu** (Splash → Loading → **Main Menu**); during gameplay it is also reused as the inter-scene loader (e.g., into a battle). It shows a determinate progress bar over dramatic faction-clash key art.

- **What it is:** a full-bleed cinematic of two armies (Iron Pact left, Ashen Horde right) converging on a burning central capital, with a centered **"LOADING"** label, a **gold determinate progress bar**, and a **percentage readout** ("40%" in the mock).
- **When it appears:** after the Splash tap, and on any heavy scene transition. Dismisses automatically when load completes (→ Main Menu by default).
- **Emotional state to evoke:** rising tension and momentum — the war is imminent; the bar filling = the march closing in. Hype, not idleness.
- **What the player does:** nothing (waits); the screen is non-interactive. It auto-advances at 100%.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** the "armies clash" hype frame. Where Splash was a quiet lone king, Loading is the full collision — two opposing hosts foreground-flanking, a burning citadel center-stage under a stormy sky.
- **Atmosphere:** dark, smoky, embered; a central fire glow at the foot of the besieged castle throws warm light up into a cold bruised-grey storm sky. Symmetric framing: **left flank = Iron Pact (cool steel-blue banners)**, **right flank = Ashen Horde (oxblood-red banners)**, converging toward the center vanishing point (the citadel).
- **Visual hierarchy:** (1) central burning citadel + fire glow → (2) "LOADING" + progress bar + % (the functional core, low-center) → (3) the two flanking armies/banners → (4) the storm sky and dragons.
- **Color psychology:** the gold bar is the only bright "progress/hope" accent in an otherwise dark, dangerous field; blue-vs-red flanks restate the faction war; the central fire = the objective/destination heating up.
- **Material identity:** baked matte-painting environment + the global cast-gold ornate bar frame; obsidian bar track.
- **Lighting:** central warm fire uplight; cold ambient storm light on the flanks; rim light on the nearest soldiers' helms/spears; heavy vignette; warm bloom on the bar fill.
- **Contrast philosophy:** brightest = central fire + gold bar fill; the percentage and label sit just under the focal citadel so the eye lands on "how close am I".

---

## C — SCREEN DECOMPOSITION (full node tree)
```
LoadingScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base          (Image: armies-clash matte painting)
    │   ├── Vignette_Overlay     (Image: radial dark vignette, multiply)
    │   ├── FireGlow_Overlay     (Image: warm central glow, additive)        [FX]
    │   ├── Smoke_Overlay        (Image: drifting smoke, low α)              [FX]
    │   └── Grain_Overlay        (Image: faint film grain)                    [FX]
    └── LoadingHUD_Group         (centered-bottom progress cluster)
        ├── Loading_Label        (Text(TMP): "LOADING")
        ├── Label_OrnamentLeft   (Image: small gold flourish left of label)
        ├── Label_OrnamentRight  (Image: small gold flourish right of label)
        ├── ProgressBar_Group
        │   ├── Bar_FrameLeftCap  (Image: ornate gold left end-cap/finial)
        │   ├── Bar_FrameRightCap (Image: ornate gold right end-cap/finial)
        │   ├── Bar_Track         (Image: dark obsidian recessed channel, 9-slice)
        │   ├── Bar_Fill          (Image: gold gradient fill, type=Filled Horizontal, Left origin)
        │   ├── Bar_FillSheen     (Image: bright top highlight line on the fill)   [FX]
        │   └── Bar_FillTipGlow   (Image: bright glow at the fill's leading edge)  [FX]
        └── Percent_Label         (Text(TMP): "40%")
```
> Armies, banners, citadel, dragons, fire are baked into `KeyArt_Base` (one image), not separate sprites.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LoadingScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills canvas |
| Root_SafeArea | LoadingScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds HUD |
| BG_Layer | LoadingScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette_Overlay | BG_Layer | 1 | Image (multiply) | stretch-all | 0.5,0.5 | fill | ignores | full-bleed |
| FireGlow_Overlay | BG_Layer | 2 | Image (additive) | center | 0.5,0.5 | center | ignores | scales with H |
| Smoke_Overlay | BG_Layer | 3 | Image (low α) | stretch-all | 0.5,0.5 | fill | ignores | tiled/scroll |
| Grain_Overlay | BG_Layer | 4 | Image | stretch-all | 0.5,0.5 | fill | ignores | tiled |
| LoadingHUD_Group | Root_SafeArea | 0 | RectTransform + VerticalLayoutGroup | bottom-center | 0.5,0 | center | inside safe | pinned bottom-center |
| Loading_Label | LoadingHUD_Group | 0 | Text(TMP) | top-center | 0.5,0.5 | center | — | — |
| Label_OrnamentLeft/Right | LoadingHUD_Group(or sub-row) | — | Image | flank label | 0.5,0.5 | center | — | — |
| ProgressBar_Group | LoadingHUD_Group | 1 | RectTransform | center | 0.5,0.5 | center | inside safe | width ∝ screen |
| Bar_FrameLeftCap | ProgressBar_Group | 0 | Image | mid-left | 0.5,0.5 | left | — | pinned left end |
| Bar_FrameRightCap | ProgressBar_Group | 1 | Image | mid-right | 0.5,0.5 | right | — | pinned right end |
| Bar_Track | ProgressBar_Group | 2 | Image (9-slice) | stretch-all (inset) | 0.5,0.5 | center | — | stretches between caps |
| Bar_Fill | ProgressBar_Group | 3 | Image **Filled/Horizontal/Left** | stretch-all (inset, matches track) | 0,0.5 | left | — | fillAmount = progress |
| Bar_FillSheen | Bar_Fill | 0 | Image (additive) | top-stretch | 0.5,1 | top | — | follows fill width |
| Bar_FillTipGlow | ProgressBar_Group | 4 | Image (additive) | mid-left, driven | 0.5,0.5 | center | — | x = track.left + fill·width |
| Percent_Label | LoadingHUD_Group | 2 | Text(TMP) | bottom-center | 0.5,0.5 | center | — | — |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** `KeyArt_Base` scale-to-**cover** 2340×1080, centered; bleeds under any cutout. Composition is left-right symmetric about screen center (citadel on the centerline).

**Loading HUD cluster (low-center).** Forensic bounds (normalized):
- **"LOADING" label:** centered, baseline at **fy ≈ 0.74** (≈ 800 px), cap height ≈ 0.034 × 1080 ≈ 37 px. Flanking flourish glyphs ≈ 0.020 × 1080 ≈ 22 px each, ≈ 16 px gap to the text.
- **Progress bar track:** centered horizontally; vertical center at **fy ≈ 0.815** (≈ 880 px); the gold structure spans the rows fy 0.816–0.894 in the source.
  - **Track full width ≈ 0.58 × 2340 ≈ 1357 px** (left edge fx ≈ 0.21 → right edge fx ≈ 0.79).
  - **Track height ≈ 0.030 × 1080 ≈ 32 px** (the recessed channel), with the ornate gold molding adding ≈ 6 px above/below.
  - **End-caps:** ornate gold finials at each end, each ≈ 0.022 × 2340 ≈ 50 px wide, overhanging the track by ≈ 0.006 × 2340 ≈ 14 px.
- **Bar_Fill:** `Image.fillAmount = progress`. In the mock the fill reaches **fx ≈ 0.786** from a left start of ≈ 0.215, i.e. ≈ **40% of the track width filled** — exactly matching the "40%" readout. Fill origin = Left.
- **Bar_FillTipGlow:** bright warm glow centered on the leading edge of the fill (at fx ≈ track.left + 0.40·track.width in the mock); travels right as progress increases.
- **"40%" percentage label:** centered horizontally, baseline at **fy ≈ 0.90** (≈ 972 px), ≈ 0.026 × 1080 ≈ 28 px cap height — directly below the bar center.

**Vertical stack rhythm (top→bottom):** LOADING (fy0.74) → bar (fy0.815) → % (fy0.90). Gaps ≈ 0.04–0.06 H. The whole cluster occupies the lower quarter, leaving the citadel/sky clear above.

**16:9 tablet adaptation:** height-anchored; bar width is a fixed fraction of *screen width* — on a narrower (taller) frame the bar gets proportionally a touch wider relative to the revealed art, but stays centered; label/% unaffected. Background crops width inward.

**Ultrawide:** more art revealed left/right; the HUD cluster keeps its centered fractional width and position; nothing stretches edge-to-edge.

**Notch behavior:** background bleeds under the side cutout; the entire HUD is centered and lives inside `Root_SafeArea`, so a landscape side-notch never overlaps the bar (centered) — but the SafeAreaFitter still guarantees it.

---

## F — TYPOGRAPHY SPECIFICATION
- **Loading_Label "LOADING":**
  - Font: serif display (Roman/Trajan-inspired), **UPPERCASE**, tracking **+14%** (notably wide-spaced as in the mock).
  - Weight: medium; cap height ≈ **37 px**.
  - Color: brushed-gold vertical gradient **#f0d27a → #caa04a**.
  - FX: soft warm outer glow (≈ 6 px, #caa04a @ 40%) + thin dark stroke (≈ 1.5 px, #1a1206) + drop shadow (0,−2, #000 @ 50%) for contrast over the dark art.
- **Percent_Label "40%":**
  - Font: same serif family OR a clean tabular semi-condensed face for the numerals; **tabular figures** so width doesn't jitter as it counts up.
  - Weight: medium/semibold; cap height ≈ **28 px**.
  - Color: warm gold **#caa04a → #e8d5a8**, with a subtle glow + dark shadow.
  - The "%" sign slightly smaller (≈ 80%) than the digits.
- **Hierarchy:** LOADING (37 px) > 40% (28 px). Both gold, both glowing, both centered.

---

## G — MATERIAL SPECIFICATION
- **Bar_Track (obsidian channel):** very dark **#060507–#121318** (sampled #121318/#181b1e under the fill, #060507 at the empty right), recessed with an inner shadow (top edge darkest) so it reads as a carved channel; faint cool inner highlight along the bottom lip.
- **Bar_Fill (gold):** vertical gradient — top highlight **#ffe9a8**, body **#e9c24a / #caa04a**, bottom shadow **#9a7320**; warm bloom; a brighter sheen line near the top.
- **Bar end-caps & molding (cast gold):** #f0d27a highlight / #caa04a mid / #6b5320 shadow; ornate beveled finials with a small central jewel/boss; crisp specular on the bevel.
- **KeyArt sky:** cold bruised storm grey **#302725** up top, lightening toward the central glow; smoke desaturates the upper third.
- **Central fire glow:** warm core #ffb04a → #d8452b falloff, additive, seated at the citadel base (fy ≈ 0.50, center).
- **Left flank (Iron Pact):** steel armor with cool blue rim; muted royal-blue banners (#2b3a6a range under the haze) with stitched gold trim.
- **Right flank (Ashen Horde):** dark armor with warm rim; oxblood-red banners (#5a1c1a → #7a1f1a).
- **Vignette:** radial multiply, transparent center → #05060a corners, ≈ 60% strength (heavier than Splash; this is the darkest boot frame).

---

## H — COMPONENT SPECIFICATION
This screen has **no interactive components** — it is a passive loader. The "component" is the progress bar as a *display* element:

**ProgressBar (display only):**
- **Purpose:** communicate determinate load progress 0→100%.
- **Driven value:** `Bar_Fill.fillAmount` and `Percent_Label.text` bound to the loader's normalized progress.
- **States (by progress, not input):**
  - **0%:** track empty (all obsidian), tip glow at far left, % reads "0%".
  - **In-progress (e.g., 40%):** fill covers 40% from left, tip glow rides the leading edge, sheen animates, % counts up with tabular figures.
  - **100%:** fill spans full track to the right cap, tip glow reaches the right finial and flares; brief completion shimmer before auto-advance.
- **Visual feedback:** the fill never snaps backward; if the underlying loader jumps, tween the fill smoothly (≈ 0.2 s catch-up) so it reads as continuous progress.
> No buttons, no tap-to-skip in the mock. Do not add a Cancel/Skip control.

---

## I — ANIMATION TIMELINE (entrance + progress)
- **t=0.00:** CanvasGroup alpha 0; KeyArt scale 1.05; bar fill at incoming progress (often 0).
- **t=0.00 → 0.50 s:** full-screen fade in 0→1 [ease-out]; KeyArt slow push-out 1.05 → 1.00 (continues subtly throughout).
- **t=0.30 → 0.70 s:** LOADING label + flourishes fade/scale in 0.95 → 1.00 [ease-out] with a gold light sweep across the letters.
- **t=0.50 → 0.90 s:** progress bar frame/caps fade in; track appears; a quick specular glint sweeps the gold molding.
- **t=0.70 s →:** Bar_Fill begins tracking real progress; FillSheen + TipGlow loops start; Percent_Label counts up in step with the fill (tabular, smooth).
- **On 100%:** TipGlow reaches the right cap and flares (≈ 0.25 s); a soft full-bar bloom pulse; then exit.
- **Exit:** CanvasGroup fade 1→0 over 0.35 s with a small KeyArt zoom-in (1.00 → 1.03) [ease-in]; hand off to Main Menu (or the requested scene).

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **Fill sheen:** a bright highlight band slides left→right along the gold fill on a ≈ 1.4 s loop (the classic "loading shimmer").
- **Tip glow pulse:** the leading-edge glow gently pulses and emits sparse warm sparks while progress advances.
- **Central fire:** the citadel fire glow flickers on a fast irregular loop; warm uplight subtly modulates.
- **Smoke:** slow upward smoke drift over the upper third (≈ 10 s scroll), low alpha.
- **Embers:** sparse warm ember motes rising from the central fire across the lower-mid frame.
- **Banner sway:** subtle cloth sway on the flank banners if separated (else baked).
- **Grain + vignette breathe:** faint film grain; vignette can breathe ≈ ±3% very slowly for life.
> No input-driven FX; all loop independently of progress except the sheen/tip which key off the fill.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** play entrance (I); start FX (J); bind progress source; begin counting.
- **OnProgress(p):** smoothly tween fill + percentage toward p; reposition tip glow.
- **OnComplete (100%):** play completion flare → exit fade → request the destination screen (default **Main Menu** in the boot flow; or the loaded scene/battle).
- **OnHide:** stop FX loops; release the large key-art texture if memory-managed; unbind progress.
- **Non-interactive:** ignore all taps; Android back is ignored during load.

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use Unity's default progress/slider visuals or default font.
- MUST NOT render the bar as a plain rectangle — it has ornate gold end-caps + beveled molding over a recessed obsidian channel.
- MUST NOT swap the serif LOADING/percentage to a generic sans (percentage may use a tabular face but keep the prestige treatment).
- MUST NOT let the fill snap or jitter; smooth it; never run it backward.
- MUST NOT add a Skip/Cancel button, tap-to-continue, or any control not in the mock.
- MUST NOT route Loading anywhere but its destination (default Main Menu) — no login screen between.
- MUST NOT break the left-blue / right-red flank symmetry or recolor the central fire.
- MUST NOT omit vignette, fire glow, smoke, fill sheen, or tip glow.
- MUST NOT let the HUD leave the safe area, nor let the background respect the safe area (bleed it).
- MUST NOT approximate bar geometry — track ≈ 0.58 W centered at fy ≈ 0.815, % at fy ≈ 0.90.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs LoadingScreenDesign.png at 2340×1080: same clashing-armies vista, central burning citadel, left-blue/right-red flanks, centered LOADING + gold ornate bar + percentage low-center.
- **Hierarchy preserved:** citadel/fire focal; bar+label+% the functional core; flanks framing.
- **Progress fidelity:** at 40% the fill covers ≈40% of the track from the left and the readout shows "40%" — the two always agree.
- **Typography:** LOADING wide-tracked UPPERCASE serif gold w/ glow; percentage tabular gold; no default font.
- **Safe area:** HUD inside safe area; background bleeds under the cutout.
- **Animation:** fade-in, label sweep, fill sheen + tip glow loops, smooth count-up, 100% flare → exit to destination.
- **Non-interactive:** no controls; auto-advances at 100%.

---

## N — IMPLEMENTATION CONFIDENCE
**93/100.** Layout, the filled-image progress bar, the count-up binding, and all loops are straightforward and exactly measurable, so code-built fidelity is high. The −7 is purely the authored art (clashing-armies matte painting + ornate gold bar finials) which must be provided as textures; uGUI places/animates them precisely but cannot generate them. The fill+percentage agreement and shimmer/tip-glow are fully reproducible in code.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base scaled to-cover, centered, bleeding under the cutout; left-blue/right-red symmetry preserved.
- □ Vignette (≈60%), central fire glow, smoke, grain overlays present at correct blend.
- □ LOADING label centered, baseline fy≈0.74, serif UPPERCASE +14% tracking gold w/ glow + flank flourishes.
- □ Progress bar centered, track ≈0.58 W, vertical center fy≈0.815, obsidian channel + beveled gold molding + ornate end-caps.
- □ Bar_Fill = Filled/Horizontal/Left, gold gradient, fillAmount bound to progress (40% in static mock).
- □ Fill sheen band + leading-edge tip glow animating.
- □ Percentage "40%" centered, baseline fy≈0.90, tabular gold, agrees with fill.
- □ Entrance timeline + smooth count-up + 100% flare + exit-to-Main-Menu implemented.
- □ No interactive controls; taps/back ignored.
- □ SafeAreaFitter on Root_SafeArea; BG_Layer ignores safe area.
- □ No default Unity styling; no sans swap on LOADING; no Skip button.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 04 · Login / Auth
Source: design/LoginAuthDesign.png · 1672×941 (1.78:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

> ⚠️ **OUT-OF-FLOW / DEPRECATED.** The finalized BULWARK boot is **NO-LOGIN**: Splash → Loading → **Main Menu directly** (Stick War Legacy onboarding simplicity; no account wall). `LoginAuthDesign.png` exists in `/design`, so per the **no-screen-skipped** rule it is fully spec'd here for completeness, **but it is not in the shipped boot chain.** Any implementation agent MUST treat this screen as optional/disabled: it must NOT be inserted between Loading and Main Menu. It may later be repurposed as an *optional* "Link Account" sheet reachable from Settings, but that is out of scope for the boot flow.

---

## A — SCREEN PURPOSE
A guest/social **account gate** offering frictionless "Play as Guest" plus optional social sign-in (Google / Facebook / Apple), with consent + legal acknowledgement.

- **What it is (as designed):** an ornate central parchment-and-gold panel headed **"WELCOME, WARRIOR"** containing a primary **PLAY AS GUEST** button, an **"OR CONTINUE WITH"** divider, three social-login rows (Google, Facebook, Apple), a "your progress is safe and secure" reassurance line, and a **ToS/Privacy consent checkbox**. Corner utilities: **Support** (bottom-left), **Language** (bottom-left), **Account Recovery** (bottom-right).
- **When it WOULD appear (if enabled):** between Loading and Main Menu, or as an optional account-link sheet. **In the finalized flow it does NOT appear.**
- **Emotional state to evoke:** safe, welcoming, prestigious arrival — "you belong here, jump in instantly" (guest is the bright primary; social is secondary).
- **What the player does (if shown):** tap **Play as Guest** to enter instantly, or pick a social provider; tick consent.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** a heraldic "audience chamber" — the player is greeted at the gate by an ornate banner-draped gold frame, flanked by the two faction champions (blue knight left, red knight right).
- **Atmosphere:** warm hazy battlefield backdrop (out of focus) framing a crisp foreground panel; a **blue heraldic banner drapes over the panel's left/top shoulder**; an amethyst gem crowns the top of the frame.
- **Visual hierarchy:** (1) gem-crowned ornate frame + "WELCOME, WARRIOR" → (2) **PLAY AS GUEST** (brightest CTA, cobalt) → (3) the three social rows → (4) reassurance + consent → (5) corner utilities.
- **Color psychology:** cobalt Play-as-Guest = Iron Pact / primary action / trust; the social buttons keep each brand's canonical color (Google white, Facebook blue, Apple black) for instant recognition; gold frame = prestige welcome; amethyst gem = premium spark.
- **Material identity:** ornate cast-gold beveled frame with a violet gem finial; near-black panel interior; cloth banner drape; brand-accurate pill buttons with leading brand glyph; small bronze utility icons.
- **Lighting:** focal warm light on the panel; cool rim on the left knight, warm rim on the right; background vignette pushes attention to the panel.
- **Contrast philosophy:** Play-as-Guest is the single brightest interactive object; social rows are calmer; legal text is the quietest.

---

## C — SCREEN DECOMPOSITION (full node tree)
```
LoginScreen (UiScreen root, CanvasGroup)   ⚠️ DISABLED in boot flow
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base            (Image: hazy battlefield w/ flanking knights baked)
    │   ├── Vignette_Overlay       (Image: radial dark vignette)
    │   └── Grain_Overlay          (Image: faint grain)                 [FX]
    ├── Panel_Group                (central ornate auth card)
    │   ├── BannerDrape            (Image: blue heraldic cloth over top-left shoulder)
    │   ├── Panel_Frame            (Image: ornate cast-gold beveled frame, 9-slice)
    │   ├── Panel_Field            (Image: near-black interior fill)
    │   ├── GemFinial_Top          (Image: amethyst gem + gold setting, top-center of frame)
    │   ├── Title_Welcome          (Text(TMP): "WELCOME, WARRIOR")
    │   ├── Btn_PlayGuest          (Button: cobalt, leading person icon + "PLAY AS GUEST")
    │   │   ├── GuestIcon          (Image: person/silhouette glyph)
    │   │   └── GuestLabel         (Text(TMP))
    │   ├── Divider_OrContinue     (row)
    │   │   ├── Divider_LineLeft   (Image: thin gold rule)
    │   │   ├── Divider_Label      (Text(TMP): "OR CONTINUE WITH")
    │   │   └── Divider_LineRight  (Image: thin gold rule)
    │   ├── Btn_Google             (Button: white pill)
    │   │   ├── GoogleIcon         (Image: G mark)
    │   │   └── GoogleLabel        (Text(TMP): "Continue with Google")
    │   ├── Btn_Facebook           (Button: Facebook-blue pill)
    │   │   ├── FacebookIcon       (Image: f mark)
    │   │   └── FacebookLabel      (Text(TMP): "Continue with Facebook")
    │   ├── Btn_Apple              (Button: black pill)
    │   │   ├── AppleIcon          (Image:  mark)
    │   │   └── AppleLabel         (Text(TMP): "Continue with Apple")
    │   ├── Reassurance_Row
    │   │   ├── ShieldIcon         (Image: small gold shield/lock)
    │   │   └── Reassurance_Label  (Text(TMP): "Your progress is safe and secure")
    │   └── Consent_Row
    │       ├── Consent_Checkbox   (Toggle: checked in mock)
    │       └── Consent_Label      (Text(TMP): "I have read and agree to the Terms of Service and Privacy Policy")
    │                                (with "Terms of Service" + "Privacy Policy" as link runs)
    ├── Util_SupportBtn            (Button: headset icon + "SUPPORT", bottom-left)
    ├── Util_LanguageBtn           (Button: globe icon + "LANGUAGE", bottom-left)
    └── Util_AccountRecoveryBtn    (Button: document icon + "ACCOUNT RECOVERY", bottom-right)
```
> Flanking knights are baked into `KeyArt_Base`. The `BannerDrape` is drawn as a foreground cloth element over the panel's shoulder.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LoginScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills canvas |
| Root_SafeArea | LoginScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds panel + utils |
| BG_Layer | LoginScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette/Grain | BG_Layer | 1–2 | Image | stretch-all | 0.5,0.5 | fill | ignores | full-bleed |
| Panel_Group | Root_SafeArea | 0 | RectTransform | center | 0.5,0.5 | center | inside safe | scales w/ H; capped max width |
| BannerDrape | Panel_Group | 0 | Image | top-left (overhang) | 0.5,1 | upper-left | — | pinned to panel shoulder |
| Panel_Frame | Panel_Group | 1 | Image (9-slice) | stretch-all | 0.5,0.5 | center | — | 9-slice |
| Panel_Field | Panel_Group | 2 (behind frame edge) | Image | stretch-all (inset) | 0.5,0.5 | center | — | fills interior |
| GemFinial_Top | Panel_Group | 3 | Image | top-center (overhang) | 0.5,0.5 | center | — | pinned top edge |
| Content_VLayout | Panel_Field | 0 | VerticalLayoutGroup + padding | stretch-top | 0.5,1 | center | — | stacks rows |
| Title_Welcome | Content_VLayout | 0 | Text(TMP) | top-center | 0.5,0.5 | center | — | — |
| Btn_PlayGuest | Content_VLayout | 1 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full panel-width |
| Divider_OrContinue | Content_VLayout | 2 | RectTransform + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Btn_Google | Content_VLayout | 3 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Btn_Facebook | Content_VLayout | 4 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Btn_Apple | Content_VLayout | 5 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Reassurance_Row | Content_VLayout | 6 | HorizontalLayout | center | 0.5,0.5 | center | — | — |
| Consent_Row | Content_VLayout | 7 | HorizontalLayout | top-left | 0,0.5 | left | — | — |
| Util_SupportBtn | Root_SafeArea | 1 | Button + vertical icon+label | bottom-left | 0,0 | center | inside safe | pinned bottom-left |
| Util_LanguageBtn | Root_SafeArea | 2 | Button + vertical icon+label | bottom-left | 0,0 | center | inside safe | right of Support |
| Util_AccountRecoveryBtn | Root_SafeArea | 3 | Button + vertical icon+label | bottom-right | 1,0 | center | inside safe | pinned bottom-right |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** scale-to-cover; bleeds under cutout. Flanking knights baked.

**Central panel.** Forensic bounds (normalized; source panel gold bbox fx 0.250–0.749, fy 0.032–0.947):
- **Panel outer:** width ≈ **0.42 × 2340 ≈ 980 px**, height ≈ **0.85 × 1080 ≈ 918 px** (a tall portrait card), centered horizontally; **top edge fy ≈ 0.04**, bottom edge fy ≈ 0.89 (the card nearly spans the full height).
- **Frame thickness:** ≈ 0.016 × 1080 ≈ 17 px beveled gold, with larger ornate corners (≈ 30 px) and a top-center gem finial overhanging ≈ 0.03 × 1080 ≈ 32 px above the top edge.
- **BannerDrape:** blue cloth draped over the top-left shoulder, overhanging the frame by ≈ 0.04 W to the left and ≈ 0.05 H above; it tucks behind the gem finial.
- **Content padding:** interior content inset ≈ 0.035 × panel-width left/right; rows stacked with ≈ 0.018 H vertical gaps.

**Row metrics (within the panel, normalized to screen):**
- **Title "WELCOME, WARRIOR":** centered, baseline fy ≈ 0.30, cap height ≈ 0.034 × 1080 ≈ 37 px.
- **Btn_PlayGuest (cobalt):** full content-width pill; vertical center fy ≈ 0.375; cobalt band spans fx ≈ 0.401–0.608 (≈ 0.21 W, ≈ 485 px); height ≈ 0.075 × 1080 ≈ 81 px (the tallest/primary button). Leading person icon at the left inset.
- **Divider "OR CONTINUE WITH":** centered, fy ≈ 0.47; gold rules flank the centered label.
- **Btn_Google (white):** full-width pill, vertical center fy ≈ 0.55 (white band fx 0.372–0.625); height ≈ 0.063 × 1080 ≈ 68 px.
- **Btn_Facebook (blue):** fy ≈ 0.62 (band fx 0.374–0.625); same height.
- **Btn_Apple (black):** fy ≈ 0.71; same height. (Social buttons share width fx ≈ 0.372–0.625 ≈ 0.253 W ≈ 592 px — slightly wider than the guest pill, with leading brand glyph + centered label.)
- **Reassurance_Row:** centered, fy ≈ 0.80, small gold shield + caption ≈ 0.020 × 1080 ≈ 22 px.
- **Consent_Row:** left-aligned within panel, fy ≈ 0.855; checkbox ≈ 0.026 × 1080 ≈ 28 px square (checked) + wrapping label with link runs.

**Corner utilities (outside the panel, on the background):**
- **Support:** bottom-left, icon center fx ≈ 0.045, fy ≈ 0.90; vertical icon-over-label, label ≈ 16 px.
- **Language:** to the right of Support, fx ≈ 0.135, fy ≈ 0.90.
- **Account Recovery:** bottom-right, fx ≈ 0.955, fy ≈ 0.90.
Each utility ≈ 0.05 × 1080 ≈ 54 px icon + small caption beneath.

**16:9 tablet:** panel keeps its proportional width (cap max ≈ 1040 px so it doesn't dominate on near-square); height-anchored; utilities pinned to safe corners. Background crops width.

**Ultrawide:** panel stays centered at fixed width; more background revealed; utilities pinned to safe-area corners (move inward with the safe inset).

**Notch behavior:** background bleeds under the cutout; panel is centered (clear of a side-notch); utilities sit inside `Root_SafeArea` so a side-notch pushes them inward rather than under the cutout.

---

## F — TYPOGRAPHY SPECIFICATION
- **Title "WELCOME, WARRIOR":** serif display, UPPERCASE, tracking +6%, gold bevel #f0d27a→#caa04a, soft glow + thin dark stroke; cap height ≈ **37 px**.
- **GuestLabel "PLAY AS GUEST":** semi-condensed bold sans **or** light serif, UPPERCASE, tracking +4%, near-white **#f4f6ff** with a thin dark shadow; cap height ≈ **30 px** (largest button label). Left-padded after the person icon, optically centered.
- **Divider_Label "OR CONTINUE WITH":** small caps, tracking +16%, muted parchment **#b9a877**, ≈ **18 px**.
- **Social labels "Continue with Google/Facebook/Apple":** clean medium sans (brand-style), sentence case, ≈ **24 px**. Colors: Google label dark **#3c4043** on white; Facebook label white **#ffffff** on blue; Apple label white **#ffffff** on black.
- **Reassurance "Your progress is safe and secure":** light sans, sentence case, muted gold/parchment **#c9b888**, ≈ **20 px**.
- **Consent label:** light sans, ≈ **18 px**, parchment grey **#b7ad95**; the runs **"Terms of Service"** and **"Privacy Policy"** are link-styled (cobalt **#4f8bff**, slightly brighter, underlined).
- **Utility captions (SUPPORT / LANGUAGE / ACCOUNT RECOVERY):** small caps, tracking +8%, muted gold ≈ **16 px**.
- **Hierarchy:** Title 37 > Guest 30 > social 24 > reassurance 20 > consent/divider/util 16–18.

---

## G — MATERIAL SPECIFICATION
- **Panel_Frame:** cast gold #f0d27a/#caa04a/#6b5320, beveled, ornate corners; top gem setting in gold.
- **GemFinial_Top:** faceted **amethyst** crystal #9e6bf0 core → #5a2db0 deep facets, inner glow + specular highlight, gold claw setting.
- **Panel_Field:** near-black **#1a1f29–#212630** (sampled interior #212630) with a subtle top-lit gradient and inner shadow from the frame.
- **BannerDrape:** royal/cobalt blue cloth #2b56c8→#1a347a with stitched gold trim and soft folds; faint cloth specular.
- **Btn_PlayGuest:** cobalt body #2b56c8→#1c3f9e with a brighter top sheen, gold/steel beveled rim, inner glow; the brightest button.
- **Btn_Google:** white #ffffff→#ececec, subtle grey 1-px border, soft drop shadow; multi-color G glyph.
- **Btn_Facebook:** Facebook blue #1877f2→#1057de, white f glyph.
- **Btn_Apple:** near-black #1c1c1e→#000000, subtle edge highlight, white  glyph.
- **ShieldIcon:** small gold shield/lock #caa04a.
- **Utility icons:** antique bronze #b08a3e line icons on transparent.
- **Vignette:** radial multiply → #06070b corners ≈ 55%.

---

## H — COMPONENT SPECIFICATION
**Btn_PlayGuest (primary CTA, cobalt):**
- Purpose: enter as guest instantly.
- States — Idle: cobalt with top sheen + soft glow; Hover: brighten +10%, glow widens; Pressed: dip −8% + scale 0.98 + brief inner flash; Disabled: desaturate to slate, glow off; Selected: n/a.
- Structure: leading person icon + centered UPPERCASE label on a beveled cobalt pill.

**Btn_Google / Btn_Facebook / Btn_Apple (social pills):**
- Purpose: OAuth with the respective provider.
- States — Idle: brand color; Hover: subtle elevation + +6% brightness; Pressed: scale 0.98 + slight dim; Disabled: 50% opacity; Selected: n/a.
- Structure: leading brand glyph (left-aligned) + label (centered or left-of-center per brand guidelines); brand-accurate colors must be preserved exactly.

**Consent_Checkbox (Toggle):**
- Purpose: gate sign-in on legal acknowledgement.
- States — Unchecked: empty gold-bordered box; Checked (mock state): gold/cobalt fill + check glyph; Hover: border brighten; Disabled: muted.
- Behavior: social/guest sign-in disabled until checked (per typical compliance; mock shows it checked).

**Util buttons (Support / Language / Account Recovery):**
- States — Idle: bronze icon + caption; Hover: icon brighten + caption glow; Pressed: scale 0.95.

---

## I — ANIMATION TIMELINE (entrance)
- **t=0.00:** CanvasGroup 0; panel scale 0.94, offset +20 px down.
- **t=0.00 → 0.45 s:** background fade in + slight Ken-Burns.
- **t=0.20 → 0.70 s:** Panel_Group scales/slides to 1.0 [ease-out-back]; gem finial catches a specular flash; banner drape settles with a tiny cloth ripple.
- **t=0.55 → 0.80 s:** Title fades/bevels in with a gold sweep.
- **t=0.70 → 1.10 s:** buttons cascade in top→bottom (Guest → divider → Google → Facebook → Apple), each fade+rise over ≈0.12 s, staggered ≈0.06 s [ease-out]; Guest gets an extra glow pulse to mark it primary.
- **t=1.10 → 1.30 s:** reassurance + consent fade in.
- **t=0.30 → 0.60 s (parallel):** corner utilities fade in.
- **Exit:** reverse-ish — panel scale 0.96 + fade 1→0 over 0.30 s.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **Gem sparkle:** the amethyst finial emits occasional tiny sparkles + a slow inner-glow breathe.
- **Guest button glow breathe:** the cobalt CTA's outer glow gently pulses to mark it primary.
- **Banner sway:** subtle cloth sway on the blue drape (≈ 4 s).
- **Background:** faint god-ray drift + grain; vignette steady.
- **Frame glint:** a slow specular glint travels the gold frame top every ≈ 6 s.
> No gameplay particles; all ambient.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow (if ever enabled):** entrance (I); FX (J); focus none (no keyboard).
- **OnPlayGuest:** if consent checked → enter; create/reuse a guest session (server-authoritative, stubbed) → route to Main Menu. If consent unchecked → nudge the checkbox (shake + highlight).
- **OnSocial(provider):** if consent checked → launch provider OAuth (stubbed) → on success route to Main Menu; on cancel/fail → return to this screen with a toast.
- **OnConsentToggle:** enable/disable sign-in buttons.
- **OnLink taps (ToS/Privacy):** open the respective document (web view / overlay).
- **OnSupport / OnLanguage / OnAccountRecovery:** open the respective utility flow.
- **OnHide:** stop FX; release art.
- **⚠️ Boot-flow note:** in the shipped flow none of the above fires because the screen is never pushed.

---

## L — NEGATIVE RULES (MUST NEVER)
- **MUST NEVER insert this screen between Loading and Main Menu** (no-login flow). If implemented, it is opt-in from Settings only.
- MUST NOT alter the brand colors/glyphs of Google/Facebook/Apple buttons (compliance + recognition).
- MUST NOT use default Unity Button/Toggle visuals or default font.
- MUST NOT demote "Play as Guest" below the social buttons in prominence — it is the brightest, largest, topmost action.
- MUST NOT omit the consent checkbox or the ToS/Privacy link runs.
- MUST NOT drop the gem finial, banner drape, vignette, or gem sparkle.
- MUST NOT let the panel exceed the safe area or the corner utilities sit under the cutout.
- MUST NOT swap the serif title to a sans.
- MUST NOT invent extra providers or fields (no email/password row — not in the mock).

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs LoginAuthDesign.png at 2340×1080: gem-crowned banner-draped gold panel, "WELCOME, WARRIOR", cobalt Play-as-Guest, OR-divider, three brand-accurate social pills, reassurance + consent, three corner utilities.
- **Out-of-flow respected:** the screen is NOT in the boot chain; verifiable by routing Loading → Main Menu with no auth stop.
- **Hierarchy preserved:** Guest brightest/largest; social calmer; legal quietest.
- **Brand integrity:** Google white / Facebook blue / Apple black exact.
- **Typography:** serif title; correct size ladder.
- **Safe area:** panel + utilities inside safe; background bleeds.
- **Animation:** entrance cascade + gem sparkle + guest glow present.
- **Affordance:** consent gates sign-in; unchecked nudges.

---

## N — IMPLEMENTATION CONFIDENCE
**88/100.** Layout/components are standard and measurable, and the cascade animation is easy. −12 because (a) it is **deprecated/out-of-flow**, so its real value is uncertain and an implementer must resist wiring it into boot; (b) brand-compliant social buttons + the ornate gem/banner panel art must be supplied; (c) live OAuth is stubbed/server-authoritative, so only the presentation is in scope here.

---

## O — SELF-CHECKLIST
- □ **Screen flagged DISABLED / not pushed in boot flow** (Loading → Main Menu verified to skip it).
- □ KeyArt_Base scale-to-cover w/ flanking knights; vignette + grain; bleeds under cutout.
- □ Central panel fx0.25–0.75, fy0.04–0.89, ornate gold frame + amethyst gem finial + blue banner drape.
- □ Title "WELCOME, WARRIOR" serif UPPERCASE gold, fy≈0.30.
- □ Btn_PlayGuest cobalt, fy≈0.375, largest/brightest, person icon + label.
- □ "OR CONTINUE WITH" divider with gold rules, fy≈0.47.
- □ Google(white)/Facebook(blue)/Apple(black) pills, brand-exact, fy≈0.55/0.62/0.71.
- □ Reassurance row (shield + caption) fy≈0.80; consent checkbox + ToS/Privacy links fy≈0.855.
- □ Corner utilities: Support + Language (bottom-left), Account Recovery (bottom-right), inside safe area.
- □ Entrance cascade + gem sparkle + guest glow breathe.
- □ Consent gates sign-in; unchecked → nudge.
- □ SafeAreaFitter on Root_SafeArea; BG_Layer ignores safe area.
- □ No default styling; no brand-color changes; no serif→sans swap; no email/password invented.



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 06 · Mode Select
Source: design/ModScreenDesign.png · 1915×821 (2.33:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

> Note: the card portraits use placeholder **stick-figure** champions (archer, spartan, king, zombie, dueling pair) — placeholder art per 00/01 context. This spec reproduces the **card layout, frames, themed art tone, label colors, and treatment exactly as shown**; the figures are flagged as placeholders to be swapped for BULWARK champions, but card geometry/styling is faithful.

---

## A — SCREEN PURPOSE
**Mode Select** is the gateway after PLAY: a single horizontal row of **five themed mode cards** the player taps to choose how to play. It is a focused chooser — pick a mode, go.

- **What it is:** five portrait cards left→right — **CLASSIC**, **MISSIONS**, **TOURNAMENT**, **ENDLESS**, **MULTIPLAYER** — each an ornate gold-framed card with a themed character portrait and a colored name plate at its foot, plus a **back button** (top-left). Over a dusk battlefield backdrop.
- **When it appears:** after tapping PLAY on the Main Menu.
- **Emotional state to evoke:** "choose your battle" — five distinct flavors of combat laid out like cards on a war table; curiosity + decisiveness.
- **What the player does:** taps one card → routes to that mode's flow (intro/map/matchmaking); or taps back → Main Menu.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** a "card table of war" — minimal chrome, five hero cards as the entire content, each themed to its mode. Dusk/ember battlefield behind, slightly out of focus so the cards pop.
- **Atmosphere:** warm hazy dusk backdrop with a faint ruined skyline; the five cards are crisp, lit, and evenly spaced like trophies.
- **Visual hierarchy:** the **row of five cards** is the whole show (equal weight, read left→right); the **back button** is the only secondary chrome; the background is pure context.
- **Color psychology — each card's name plate teaches its mode:**
  - **CLASSIC** — green name plate over a **parchment/aged-map** card (the default, "the original").
  - **MISSIONS** — blue name plate, spartan/shield card (structured, Iron-Pact-coded campaign-ish).
  - **TOURNAMENT** — red name plate over a **fiery** card (high-stakes, competitive heat).
  - **ENDLESS** — purple name plate over a **green undead/horde** card (survival/dark/mystic).
  - **MULTIPLAYER** — gold/orange name plate over a **blue-vs-red duel** card (PvP clash).
- **Material identity:** ornate cast-gold beveled card frames; each card a small illustrated scene; gem-like colored name plates with gold trim; bronze back button.
- **Lighting:** each card carries its own internal lighting (fire on Tournament, eerie green on Endless, electric blue/red on Multiplayer); soft top key catches the gold frames; backdrop vignette.
- **Contrast philosophy:** five equally bright cards on a darker backdrop; the colored name plates are the brightest labels; no single card dominates (this is a chooser, not a hierarchy).

---

## C — SCREEN DECOMPOSITION (full node tree)
```
ModeSelectScreen (UiScreen root, CanvasGroup)
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base        (Image: dusk battlefield/ruins, soft-focus)
    │   ├── Vignette_Overlay   (Image: radial vignette)
    │   ├── GodRay_Overlay     (Image: soft warm shafts)        [FX]
    │   └── Grain_Overlay      (Image: faint grain)              [FX]
    ├── BackButton_Group       (top-left)
    │   ├── Back_Frame         (Image: ornate gold square button)
    │   └── Back_Arrow         (Image: gold left-chevron glyph)
    └── CardRow_Group          (HorizontalLayoutGroup, 5 equal cards, centered)
        ├── Card_Classic
        │   ├── Card_Frame     (Image: ornate gold beveled card border, 9-slice)
        │   ├── Card_Art       (Image: parchment/map + archer portrait)
        │   ├── Card_ArtMask   (Mask/RectMask2D clipping art to frame)
        │   ├── Card_TopEmblem (Image: small emblem top-center of card)   [if present]
        │   ├── NamePlate      (Image: green gem plate w/ gold trim)
        │   └── NameLabel      (Text(TMP): "CLASSIC")
        ├── Card_Missions      (same structure; blue plate; "MISSIONS"; spartan art)
        ├── Card_Tournament    (same; red plate; "TOURNAMENT"; fiery art)
        ├── Card_Endless       (same; purple plate; "ENDLESS"; green-undead art)
        └── Card_Multiplayer   (same; gold/orange plate; "MULTIPLAYER"; blue-vs-red duel art)
```
> Each card is an identical node template (Frame + Art + Mask + NamePlate + NameLabel), differing only in art texture, plate color, and label text.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| ModeSelectScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills |
| Root_SafeArea | ModeSelectScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds back + cards |
| BG_Layer | ModeSelectScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette/GodRay/Grain | BG_Layer | 1–3 | Image | stretch-all / top | — | — | ignores | full-bleed |
| BackButton_Group | Root_SafeArea | 0 | Button | top-left | 0,1 | center | inside safe | pinned top-left |
| Back_Frame | BackButton_Group | 0 | Image | stretch-all | 0.5,0.5 | center | — | square |
| Back_Arrow | BackButton_Group | 1 | Image | center | 0.5,0.5 | center | — | — |
| CardRow_Group | Root_SafeArea | 1 | HorizontalLayoutGroup (spacing, center, child-force-expand off) | center | 0.5,0.5 | center | inside safe | scales w/ H; centered |
| Card_Classic … Card_Multiplayer | CardRow_Group | 0–4 | Button + RectTransform | — | 0.5,0.5 | center | — | fixed aspect, equal width |
| Card_Frame | (each Card) | 0 | Image (9-slice ornate) | stretch-all | 0.5,0.5 | center | — | 9-slice |
| Card_Art | (each Card) | 1 (behind frame edge) | Image | stretch-all (inset) | 0.5,0.5 | center | — | clipped by mask |
| Card_ArtMask | (each Card) | wraps Art | RectMask2D | stretch-all (inset) | 0.5,0.5 | center | — | clips art to inner rect |
| Card_TopEmblem | (each Card) | 2 | Image | top-center | 0.5,1 | center | — | small |
| NamePlate | (each Card) | 3 | Image | bottom-center | 0.5,0 | center | — | overlaps card foot |
| NameLabel | NamePlate | 0 | Text(TMP) | center | 0.5,0.5 | center | — | — |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** scale-to-cover; bleeds under cutout; soft-focus dusk ruins.

**Card row (the whole content).** Five equal portrait cards, evenly spaced, **horizontally centered** as a group, **vertically centered** (slightly above center). Forensic measurements (normalized):
- **Card vertical extent:** frame top fy ≈ **0.202**, frame bottom fy ≈ **0.816** → **card height ≈ 0.614 × 1080 ≈ 663 px**.
- **Card vertical center:** fy ≈ **0.51** (essentially screen-centered, very slightly low).
- **Card centers (X):** the five cards sit at fx ≈ **0.18 / 0.345 / 0.51 / 0.655 / 0.82** (centers ≈ 0.165 W apart, evenly distributed across the middle ~0.82 of the width).
- **Card width:** each ≈ **0.135 × 2340 ≈ 316 px** (portrait aspect ≈ 316:663 ≈ **0.48 : 1**, a tall card).
- **Inter-card gap:** ≈ 0.03 × 2340 ≈ 70 px between adjacent card edges (centers 0.165 apart − card width 0.135 = 0.03 gap).
- **Row outer span:** left edge of card 1 ≈ fx 0.112, right edge of card 5 ≈ fx 0.888 → the row occupies the central ≈ 0.776 W, leaving ≈ 0.11 W margins each side.
- **Frame thickness:** ornate gold border ≈ 0.012 × 2340 ≈ 28 px (corner ornaments larger).

**Name plates (foot of each card):**
- Each colored gem plate overlaps the card's lower portion; vertical band fy ≈ **0.65–0.805** (forensic: Tournament plate y 0.650–0.805) — i.e. the plate sits over the **bottom ~25%** of the card, centered, slightly wider than the card art so it reads as a banner clasped across the foot.
- **Plate height ≈ 0.155 × 1080 ≈ 167 px** including the gold trim; the label is vertically centered in the plate's upper portion.
- **Plate colors (forensic samples):** CLASSIC green **#2d4211/#293d11**; MISSIONS blue **#0e3863/#11386a**; TOURNAMENT red **#5d0f09/#610b08**; ENDLESS purple **#443053/#422a53**; MULTIPLAYER gold/orange **#864f02/#764701**.

**Back button (top-left):**
- Ornate gold square; forensic bounds fx **0.033–0.072**, fy **0.048–0.136** → ≈ **0.04 × 2340 ≈ 94 px** square (height ≈ 0.088 × 1080 ≈ 95 px), centered at fx ≈ 0.052 / fy ≈ 0.092. Gold left-chevron glyph centered inside.

**16:9 tablet:** cards are height-anchored (height = 0.614 H), so they keep their size; the row stays centered; on a narrower frame the inter-card gaps compress slightly but cards never overlap; back button pins to the top-left safe corner. Background crops width.

**Ultrawide:** the centered row keeps its fixed fractional layout; more background revealed at the sides; gaps may widen proportionally; back button pins to the (inset) top-left safe corner.

**Notch behavior:** background bleeds under the cutout; the back button (top-left) and the leftmost card live in `Root_SafeArea` so a left-side landscape notch insets them clear; the centered row never collides with a side cutout.

---

## F — TYPOGRAPHY SPECIFICATION
- **NameLabels (CLASSIC / MISSIONS / TOURNAMENT / ENDLESS / MULTIPLAYER):**
  - Font: bold serif display OR heavy slab, **UPPERCASE**, tracking +4%.
  - Weight: heavy; cap height ≈ **0.030 × 1080 ≈ 32 px** (auto-fit down for the longer words MULTIPLAYER/TOURNAMENT so all five share a consistent visual size and fit their plates).
  - Color: warm cream/gold **#f0e0bb → #e8d5a8** letters with a **thin dark stroke** (≈ 1.5 px, #1a0f06) and a soft drop shadow, so the same light type reads on every plate color.
  - Each label is centered on its plate.
- **Back_Arrow:** glyph, not text (gold chevron).
- **Hierarchy:** the five labels are intentionally **equal** in size/treatment (no dominant card). Only one text run type on this screen.

---

## G — MATERIAL SPECIFICATION
- **Card_Frame (cast gold):** #f0d27a highlight / #caa04a mid / #6b5320 shadow; ornate beveled molding with engraved corner scrollwork + small top-center emblem boss; crisp specular on the bevel; faint warm rim.
- **Card_Art (per mode):**
  - CLASSIC: aged **parchment/map** tone #c9b27a with a green-cloaked archer; soft warm light.
  - MISSIONS: stone/steel spartan with **blue** rim + shield; cool light.
  - TOURNAMENT: **fire** scene — embers/flame #d8452b→#7a1f1a around a crowned warrior; hot uplight.
  - ENDLESS: eerie **green** undead #5f9841/#2d4211 mausoleum tone; cold green glow.
  - MULTIPLAYER: **split blue-vs-red** duel #2b56c8 vs #c8351f with electric energy; high contrast.
- **NamePlate (gem banner):** colored body (per E) with a brighter top sheen + **gold beveled trim** clasps at the ends; inner glow tinted to the plate color.
- **Card_TopEmblem:** small gold faction/mode emblem on the frame's top center (where shown).
- **Back_Frame:** ornate cast-gold square button matching the frame language; dark inset behind the chevron.
- **Background:** soft-focus dusk battlefield — warm grey-violet sky #4d3442, ruined skyline, dust haze; vignette → #07060a corners ≈ 55%.

---

## H — COMPONENT SPECIFICATION
**Mode cards (×5).** Each whole card is a Button.
- **Idle:** lit themed card, gold frame, glowing colored name plate; a very gentle idle breathe/float (see I/J).
- **Hover:** card scales 1.04 + frame rim glow brightens + the card's themed FX intensifies (e.g., Tournament flames flare, Multiplayer sparks crackle) + name plate brightens.
- **Pressed:** scale 0.97 + brief inner flash + plate flash; card "presses into the table".
- **Disabled (locked mode):** card desaturated/greyed with a small **gold padlock** overlay + a requirement caption (e.g., "Unlocks at level X"); frame dull, FX off. (Not shown unlocked in mock, but the template must support a locked state.)
- **Selected:** on tap, the chosen card flares and lifts as the screen transitions out.
- **Feedback:** press → route to the mode (CLASSIC→classic match/intro; MISSIONS→Campaign Map/missions; TOURNAMENT→Tournament Ladder; ENDLESS→endless setup; MULTIPLAYER→Online Battle matchmaking).

**Back button:**
- **Idle:** gold square + chevron.
- **Hover:** rim glow + chevron brighten + scale 1.05.
- **Pressed:** scale 0.92 + dip.
- **Feedback:** → Main Menu.

---

## I — ANIMATION TIMELINE (entrance)
- **t=0.00:** CanvasGroup 0; background scale 1.04; cards offset +24 px down + scale 0.9.
- **t=0.00 → 0.40 s:** background fade in + slow Ken-Burns.
- **t=0.20 → 0.85 s:** the five cards **deal in** left→right like dealt cards — each fades + rises + scales 0.9→1.0 over ≈0.16 s, staggered ≈0.09 s [ease-out-back]; on each card's arrival its themed FX ignites (flame/spark/green-glow) and its name plate clasps snap with a small gold glint.
- **t=0.30 → 0.60 s (parallel):** back button drops/scales in [ease-out].
- **t≥0.9 s:** idle — cards gently float/breathe; themed FX loop.
- **Exit (on select):** chosen card flares + lifts/scales 1.08 while the others fade slightly; CanvasGroup 1→0 over 0.28 s [ease-in]; route to the mode. Exit (on back): simple fade 0.25 s → Main Menu.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **Per-card themed loops:**
  - TOURNAMENT: flickering flames + rising embers within the card.
  - ENDLESS: drifting eerie green mist + occasional spark.
  - MULTIPLAYER: small electric arcs along the blue/red split.
  - CLASSIC: gentle dust motes / warm light over the parchment.
  - MISSIONS: subtle steel glint + faint banner sway.
- **Frame glint:** a slow specular glint travels each gold frame on a staggered loop.
- **Name plate glow breathe:** each colored plate's inner glow gently pulses in its own hue.
- **Card float:** each card bobs ±2 px on a slow, slightly phase-offset sine (so the row feels alive, not rigid).
- **Background:** warm god-ray drift + dust haze + grain; vignette steady.
> No gameplay particles; all ambient and looped per card.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow:** entrance deal-in (I); start per-card FX (J); query lock/unlock state per mode (server-authoritative, read-only) and apply locked visuals where applicable.
- **OnCardTap(mode):** if unlocked → flare + lift → route to that mode's flow. If locked → shake + show the unlock requirement tooltip (no navigation).
- **OnBack:** fade → Main Menu (also Android back).
- **OnReturn (from a mode flow):** re-show with a quick fade; re-evaluate lock states; light re-cascade or snap.
- **OnHide:** stop FX loops; keep card art if returning soon.

---

## L — NEGATIVE RULES (MUST NEVER)
- MUST NOT use default Unity Button visuals or default font — cards are ornate gold-framed illustrated cards with gem name plates.
- MUST NOT make the cards unequal in size or give one visual dominance — this is an equal-weight chooser (only hover/selection may scale one).
- MUST NOT recolor a mode's name plate (green Classic, blue Missions, red Tournament, purple Endless, gold/orange Multiplayer) — the plate color identifies the mode.
- MUST NOT change a card's themed art tone (parchment / steel-blue / fire / green-undead / blue-vs-red duel).
- MUST NOT omit the per-card themed FX, the frame glint, or the plate glow.
- MUST NOT drop the back button or move it off the top-left safe corner.
- MUST NOT let cards overlap, exceed the safe area, or let the background respect the safe area (bleed it).
- MUST NOT swap the serif/slab name labels to a default sans, and MUST keep all five labels the same size/treatment.
- MUST NOT treat the placeholder stick figures as final — but MUST preserve card geometry/frames/plates when swapping in BULWARK champions.
- MUST NOT add more or fewer than five cards.

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs ModScreenDesign.png at 2340×1080: five equal gold-framed portrait cards centered in a row (CLASSIC/MISSIONS/TOURNAMENT/ENDLESS/MULTIPLAYER) with the correct themed art + colored name plates, over a soft-focus dusk backdrop, with a gold back button top-left.
- **Layout:** card height ≈0.614 H, centers at fx≈0.18/0.345/0.51/0.655/0.82, ≈0.135 W wide, equal gaps; row centered.
- **Plate colors exact:** green/blue/red/purple/gold-orange per mode.
- **Hierarchy:** five equal-weight cards; back button the only secondary chrome.
- **Typography:** uniform UPPERCASE serif/slab cream-gold labels with dark stroke, auto-fit to plates; no default font.
- **Safe area:** back button + leftmost/rightmost cards inset from notches; background bleeds.
- **Animation:** left→right deal-in with themed FX ignition; idle float + per-card FX loops; tap flares + routes.
- **Affordance:** each card has idle/hover/pressed (+locked) states and routes to its mode; back → Main Menu.

---

## N — IMPLEMENTATION CONFIDENCE
**91/100.** The screen is structurally clean — five instances of one card template in a centered HorizontalLayoutGroup, plus a back button — and every dimension is measured, so the layout, deal-in animation, and feedback are highly reproducible in code. −9: the five themed card illustrations, ornate gold frames, colored gem name plates, and the per-card looping FX (flame/mist/arcs) must be supplied as authored art/particles (placeholder stick figures need replacing); lock-state data is server-authoritative (read-only). Geometry, layout, and interaction are fully code-buildable.

---

## O — SELF-CHECKLIST
- □ KeyArt_Base soft-focus dusk battlefield, scale-to-cover, bleeds under cutout; vignette + god-rays + grain.
- □ Five equal cards in a centered HorizontalLayoutGroup: height ≈0.614 H, width ≈0.135 W, centers fx≈0.18/0.345/0.51/0.655/0.82, ≈0.03 W gaps.
- □ Card template: ornate gold 9-slice frame + masked themed art + top emblem + foot name plate + label.
- □ Themed art per card: CLASSIC parchment/archer, MISSIONS steel/spartan, TOURNAMENT fire, ENDLESS green-undead, MULTIPLAYER blue-vs-red duel.
- □ Name plates colored per mode (green/blue/red/purple/gold-orange) with gold trim; labels uniform cream-gold UPPERCASE serif + dark stroke, auto-fit.
- □ Back button top-left, ornate gold square + chevron, ≈94 px, fx≈0.052/fy≈0.092.
- □ Entrance: left→right deal-in (ease-out-back) with themed-FX ignition + plate glints; back button drops in.
- □ Idle FX: per-card flame/mist/arc/dust loops, frame glints, plate glow breathe, gentle card float.
- □ Card states: idle/hover(scale+FX flare)/pressed/locked(padlock+requirement); tap → route to mode; locked → shake+tooltip.
- □ Back → Main Menu (and Android back).
- □ SafeAreaFitter insets back button + edge cards from notches; BG ignores safe area.
- □ No default styling; equal-weight cards; plate-color language intact; serif labels; exactly five cards; placeholder figures flagged.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 07 · Match Intro (Pre-Battle VS)

Source: design/MatchIntroDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits the GLOBAL VISUAL DNA and the §12 control boundary from `00_CONTEXT_RECOVERY.md`. This is the
> pre-battle framing screen shown after Mode/Online selection and before the Battle HUD loads. It is a
> full-bleed cinematic card; no live ECS battle runs underneath yet (the sim is loading behind it).

---

## A · SCREEN PURPOSE
A cinematic **"VS" face-off card** that sells the matchup before the fight. It (1) names the **mode/map**
("CLASSIC — STONEHOLD PASS") in a top-center gold banner, (2) presents the two combatants as hero portraits —
left **Iron Pact** blue knight, right **Ashen Horde** red warlord — split by a luminous **"VS"** seam of clashing
blue lightning and ember fire, (3) labels each faction with a heraldic crest + name + motto plate along the
bottom, and (4) shows a single coaching **Tip** line at center-bottom. It is a **transient hand-off screen**:
auto-advances to the Battle HUD when the sim is ready (or on tap). No interactive choices beyond an implicit
"tap/auto-continue"; **no buttons are drawn** in the mockup. Reads nothing from ECS; writes nothing (the match
is configured by the meta layer that launched it).

## B · VISUAL DNA (screen-specific on top of global)
- **Diptych split-screen** composition: the frame is bisected vertically by an energy seam. Left hemisphere =
  **cold** (cobalt/steel, blue rim light, a blue lightning bolt clawing up the seam). Right hemisphere = **hot**
  (oxblood/ember, orange fire licking up the seam, sparks). The seam is the brightest vertical axis on screen.
- **Two full-body hero renders** at roughly 2/3 screen height, facing the seam (left knight faces right, right
  warlord faces left), lit by low-key key-lights from the seam so their inner edges carry a gold/white rim.
- **Top-center ornate banner**: a horizontal cast-gold/bronze plaque with scrolled end-caps and a small central
  finial, holding the serif gold-bevel title "CLASSIC" and a thin tracked sub-label "STONEHOLD PASS".
- **Giant central "VS"** in heavy gold serif with a hot bloom — the focal emblem, sitting over the seam at
  vertical center.
- **Two bottom faction plates**: dark glass lozenges with a circular crest at the inner end, faction name in
  gold serif, and a tracked uppercase motto beneath.
- Strong cinematic **vignette**; warm god-rays bleed from the seam; heavy atmospheric haze/embers right, cold
  mist left. Backgrounds: left = ruined blue-lit fortress wall; right = burning red battlefield/cliff.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
MatchIntroScreen (UiScreen, CanvasGroup, full-bleed)
└─ Root (RectTransform, stretch-all)
   ├─ BG_FullBleed (Image, stretch-all)                     // composited cinematic backdrop (under cutout)
   │  ├─ BG_Left_ColdField (Image)                          // blue ruined fortress wall + cold mist
   │  ├─ BG_Right_HotField (Image)                          // burning red cliff/battlefield + embers
   │  ├─ Seam_EnergySplit (Image, vertical)                 // blue-lightning↔orange-fire clash band
   │  │  ├─ Seam_LightningBlue (Image)                      // left-leaning electric arc (cold)
   │  │  └─ Seam_FireOrange (Image)                         // right-leaning flame plume (hot)
   │  ├─ Vignette (Image, stretch-all, multiply)
   │  └─ GodRayGlow (Image, additive, centered on seam)
   ├─ SafeArea (RectTransform + SafeAreaFitter)             // all framed content insets here
   │  ├─ Hero_Left_IronPact (Image)                         // blue knight: sword + kite shield (blue heraldry)
   │  ├─ Hero_Right_AshenHorde (Image)                      // red warlord: spiked armor + mace
   │  ├─ TopBanner_ModeMap (Container, top-center)
   │  │  ├─ Banner_Frame_Gold (Image)                       // ornate plaque + scroll end-caps + finial
   │  │  ├─ Banner_Title_Mode (Text "CLASSIC")
   │  │  └─ Banner_Sub_Map (Text "STONEHOLD PASS")
   │  ├─ VS_Emblem (Container, center)
   │  │  ├─ VS_Glow (Image, additive)                       // hot bloom behind glyphs
   │  │  └─ VS_Text (Text "VS")                             // giant gold serif
   │  ├─ FactionPlate_Left (Container, bottom-left)
   │  │  ├─ Plate_BG_Glass_L (Image)                        // dark glass lozenge
   │  │  ├─ Crest_IronPact (Image)                          // round blue+gold shield crest
   │  │  ├─ Faction_Name_L (Text "IRON PACT")
   │  │  └─ Faction_Motto_L (Text "STRENGTH. HONOR. UNITY.")
   │  ├─ FactionPlate_Right (Container, bottom-right)
   │  │  ├─ Plate_BG_Glass_R (Image)                        // dark glass lozenge
   │  │  ├─ Crest_AshenHorde (Image)                        // round red skull crest
   │  │  ├─ Faction_Name_R (Text "ASHEN HORDE")
   │  │  └─ Faction_Motto_R (Text "STRENGTH IN CHAOS.\nGLORY IN CONQUEST.")
   │  └─ Tip_Line (Container, bottom-center)
   │     └─ Tip_Text (Text "Tip: Upgrade your Units and Commanders\nto dominate the battlefield.")
   └─ TapToContinue_Catcher (Button, transparent, stretch-all)  // implicit advance; invisible
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Screen | 0 | RectTransform | stretch-all (0,0)-(1,1) | 0.5,0.5 | offsets 0 | n/a | fills canvas |
| BG_FullBleed | Root | 0 | Image | stretch-all | 0.5,0.5 | drawn UNDER cutout | **no** | full-bleed |
| BG_Left_ColdField | BG_FullBleed | 0 | Image | left-half (0,0)-(0.5,1) | 0.5,0.5 | — | no | reveals more on widen |
| BG_Right_HotField | BG_FullBleed | 1 | Image | right-half (0.5,0)-(1,1) | 0.5,0.5 | — | no | reveals more on widen |
| Seam_EnergySplit | BG_FullBleed | 2 | Image | vertical strip x∈[0.46,0.54], y full | 0.5,0.5 | centered | no | stays seam-centered |
| Seam_LightningBlue | Seam_EnergySplit | 0 | Image | center | 0.5,0.5 | additive | no | — |
| Seam_FireOrange | Seam_EnergySplit | 1 | Image | center | 0.5,0.5 | additive | no | — |
| Vignette | BG_FullBleed | 3 | Image | stretch-all | 0.5,0.5 | multiply, on top of fields | no | full-bleed |
| GodRayGlow | BG_FullBleed | 4 | Image | center | 0.5,0.5 | additive | no | seam-centered |
| SafeArea | Root | 1 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | insets to Screen.safeArea | **yes** | all content here |
| Hero_Left_IronPact | SafeArea | 0 | Image | left-anchored, bottom | 0.5,0 | inner edge faces seam | partial | scale w/ height |
| Hero_Right_AshenHorde | SafeArea | 1 | Image | right-anchored, bottom | 0.5,0 | inner edge faces seam | partial | scale w/ height |
| TopBanner_ModeMap | SafeArea | 2 | Container | top-center (0.5,1) | 0.5,1 | hangs from safe top | yes | fixed-height, center |
| Banner_Frame_Gold | TopBanner | 0 | Image (sliced) | stretch-all (of banner) | 0.5,0.5 | 9-slice end-caps | yes | — |
| Banner_Title_Mode | TopBanner | 1 | Text | center | 0.5,0.5 | "CLASSIC" | yes | — |
| Banner_Sub_Map | TopBanner | 2 | Text | below title | 0.5,1 | "STONEHOLD PASS" | yes | — |
| VS_Emblem | SafeArea | 3 | Container | center (0.5,0.5) | 0.5,0.5 | over seam | yes | center-locked |
| VS_Glow | VS_Emblem | 0 | Image | center | 0.5,0.5 | additive behind glyphs | yes | — |
| VS_Text | VS_Emblem | 1 | Text | center | 0.5,0.5 | "VS" | yes | — |
| FactionPlate_Left | SafeArea | 4 | Container | bottom-left (0,0) | 0,0 | — | yes | pin to safe BL |
| Crest_IronPact | Plate_L | 1 | Image | left-inside plate | 0.5,0.5 | round crest | yes | — |
| Faction_Name_L | Plate_L | 2 | Text | left, top row | 0,0.5 | "IRON PACT" | yes | — |
| Faction_Motto_L | Plate_L | 3 | Text | left, bottom row | 0,0.5 | motto | yes | — |
| FactionPlate_Right | SafeArea | 5 | Container | bottom-right (1,0) | 1,0 | mirror of left | yes | pin to safe BR |
| Crest_AshenHorde | Plate_R | 1 | Image | right-inside plate | 0.5,0.5 | round skull crest | yes | — |
| Faction_Name_R | Plate_R | 2 | Text | right, top row | 1,0.5 | "ASHEN HORDE" (right-align) | yes | — |
| Faction_Motto_R | Plate_R | 3 | Text | right, bottom row | 1,0.5 | 2-line motto (right-align) | yes | — |
| Tip_Line | SafeArea | 6 | Container | bottom-center (0.5,0) | 0.5,0 | between the two plates | yes | center |
| Tip_Text | Tip_Line | 0 | Text | center | 0.5,0.5 | 2-line tip | yes | wrap |
| TapToContinue_Catcher | Root | 2 | Button (Image α0) | stretch-all | 0.5,0.5 | invisible advance | no | full screen |

**Child-order rationale:** BG (and its seam/vignette/glow) renders first (deepest); heroes next so their inner
rim sits over the seam glow; banner/VS/plates/tip are the readable foreground; the transparent tap-catcher is
top-most so a tap anywhere advances.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Seam axis:** vertical center, x = 0.50. Seam strip width ≈ **0.08W** (x∈[0.46,0.54]); glow halo ≈ 0.22W wide.
- **TopBanner_ModeMap:** width ≈ **0.30W** (≈702 px), height ≈ **0.115H** (≈124 px); top edge at y≈0.955 (its
  top ≈ 0.045H below the safe top). Centered x=0.50. Title cap-height ≈ 0.055H; sub-label ≈ 0.020H, sitting
  ≈ 0.012H below the title baseline.
- **VS_Emblem:** glyph cap-height ≈ **0.165H** (≈178 px) — the largest type on screen. Centered at (0.50, 0.50).
  Glow halo ≈ 1.6× the glyph box.
- **Hero_Left:** occupies x∈[0.02,0.44], bottom-anchored, height ≈ **0.78H**; its weapon (sword tip) reaches up
  to ≈0.80H. **Hero_Right:** mirror, x∈[0.56,0.98], height ≈ **0.80H** (mace head reaches ≈0.84H). Inner edges
  stop ≈0.06W shy of the seam so the energy stays visible between them.
- **Faction plates:** each ≈ **0.40W** wide × **0.135H** tall. Left plate pinned to safe BL with its left edge at
  x≈0.015, bottom at y≈0.045H; right plate mirrored (right edge x≈0.985). The crest circle Ø ≈ **0.085H**,
  inset ≈0.018W from the plate's inner-bottom corner (left crest at the plate's left; right crest at plate's
  right). Faction name cap-height ≈ 0.040H; motto cap-height ≈ 0.018H, ≈0.010H below the name.
- **Tip_Line:** centered x=0.50, bottom at y≈0.050H, max width ≈ **0.34W** (sits in the gap between the two
  plates, slightly higher than them — its baseline ≈0.075H so it clears the plate tops). Tip cap-height ≈ 0.020H,
  two centered lines, line-spacing ×1.15.
- **Tablet (4:3 / 1.33:1):** seam + heroes + VS stay centered; the two faction plates pull inward (reduce their
  width to ≈0.34W each) so they don't overlap the heroes; banner unchanged. **Ultrawide (21:9):** background
  fields and seam glow widen to fill; heroes, banner, VS, plates and tip stay at the same fractions and remain
  center-clustered (extra width is scenic margin). **Notch:** full-bleed BG passes under the cutout; banner,
  plates and tip live inside SafeArea so a side-notch in landscape never clips text or crests.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Line-sp | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|---|
| Banner_Title_Mode | CLASSIC | Trajan serif, gold bevel | Heavy | UPPER | +6% | — | soft gold bloom + 1px #2a1d07 stroke + drop-shadow (0,2,#000 60%) | ~59 | gradient **#f7e7b0→#caa04a** (top-light bevel) |
| Banner_Sub_Map | STONEHOLD PASS | refined serif, small-caps feel | Medium | UPPER | +14% | — | thin dark stroke; faint shadow | ~22 | #cdb784 (muted parchment-gold) |
| VS_Text | VS | massive heroic serif | Black | UPPER | -2% (tight) | — | hot inner-glow + outer gold bloom + dark drop-shadow (0,4,#000 70%) | ~178 | gradient **#fff2c0→#e2a93a**, hot-orange spill at base |
| Faction_Name_L | IRON PACT | regal serif | Bold | UPPER | +8% | — | gold bevel, 1px #1a1206 stroke, soft shadow | ~43 | gradient **#f3e2a6→#c79a44** |
| Faction_Motto_L | STRENGTH. HONOR. UNITY. | clean small-caps | Medium | UPPER | +12% | — | thin dark stroke for legibility | ~19 | #b9c6e6 (cool steel-blue tint) |
| Faction_Name_R | ASHEN HORDE | regal serif | Bold | UPPER | +8% | — | gold bevel, dark stroke, soft shadow | ~43 | gradient **#f3e2a6→#c79a44** |
| Faction_Motto_R | STRENGTH IN CHAOS. / GLORY IN CONQUEST. | clean small-caps | Medium | UPPER | +12% | ×1.15 | thin dark stroke | ~19 | #e3b6a4 (warm ember tint) |
| Tip_Text | Tip: Upgrade your Units and Commanders / to dominate the battlefield. | informational serif | Regular | Title/sentence case | +2% | ×1.15 | subtle dark shadow for over-art legibility | ~21 | #d9c79a (parchment) |

Notes: titles/names use the serif SDF (TMP) upgrade; mottos & tip may render in the same family at lighter
weight. Faction-name fill is identical gold for both sides (parity); only the **motto tint** carries the cool
vs warm faction hue. The VS is the single brightest, largest glyph — eye-magnet.

## G · MATERIALS
- **Banner frame (cast gold/bronze):** base #8a6a2c, highlight #f0d27a, deep shadow #5a4318; satin-metal
  roughness ≈0.35; engraved filigree on the face; polished beveled rim with a sharp gold specular along the
  top edge; scrolled end-caps + small central finial; subtle gold rim-bloom.
- **VS glyph metal:** the most polished gold on screen (roughness ≈0.2) with an additive hot core; reads like
  molten/charged metal where the seam energy meets it.
- **Faction crest — Iron Pact:** disc of dark steel ringed in gold; centered blue heraldic device (chevron/tower
  motif) on a cobalt field; cool rim-light, slight gloss.
- **Faction crest — Ashen Horde:** disc of blackened iron ringed in tarnished gold; a red **skull/horned**
  emblem on an oxblood field; warm rim-light, soot-worn edges.
- **Plate glass lozenges:** near-black translucent glass (#0c0e14 @ ~78% opacity) with a thin gold/bronze hairline
  edge and a faint inner top-edge sheen; very low reflection; slight blur of the art behind.
- **Hero armor — left:** brushed/blued steel with cobalt cloth and a gold-trimmed kite shield bearing the blue
  device; cool rim-light from the seam; weathered scratches.
- **Hero armor — right:** blackened spiked plate with oxblood leather and bone/horn accents, ember rim-light;
  heavy soot + battle-wear; the mace is dark iron with glinting spikes.
- **Backgrounds:** left = blue-lit ruined stone fortress, cold haze (#10131c→#1d2740 blues); right = burning
  cliff/battlefield, ember orange (#1a0d09→#5a1e12 reds) with floating sparks.
- **Seam:** electric **blue** plasma (#5fa8ff core, white-hot center) on the left lean vs **orange** flame
  (#ff8a2a→#ffd27a) on the right lean, meeting in a white-gold collision at center; strong additive bloom.

## H · COMPONENTS (interactive)
1. **TapToContinue_Catcher** — *purpose:* advance to Battle HUD on player tap (and a watchdog auto-advance).
   - *States:* **idle** = invisible, accepting input; **pressed** = optional 0.05 brightness pulse of the whole
     card (CanvasGroup or a brief additive flash) to acknowledge; **disabled** = during the 0.0–0.35s intro
     build-in, taps are swallowed (no advance) until the card is fully assembled; there is no hover/selected on
     a transient full-screen catcher.
   - *Structure:* a single stretch-all transparent `Button` over everything; raycast target on, no graphic.
   - *Feedback:* on accept, plays the OnHide transition (E/I below) then the router pushes BattleHud. A faint
     "tap to continue" affordance is **not** drawn in the mockup, so none is added (negative rule L).

*(No other interactive elements exist in this mockup — banner, VS, crests, plates, and tip are all
non-interactive display.)*

## I · ANIMATION TIMELINE (OnShow build-in; t in seconds)
| t (s) | Element | Action | Duration | Easing | Emphasis |
|---|---|---|---|---|---|
| 0.00 | BG_FullBleed + Vignette | fade 0→1 | 0.25 | ease-out | scene establishes |
| 0.05 | Seam_EnergySplit | ignite: scale-Y 0.6→1, α 0→1 | 0.30 | ease-out | the clash lights up |
| 0.10 | Hero_Left | slide in from x-0.06 + α 0→1 | 0.30 | ease-out-cubic | knight strides in |
| 0.10 | Hero_Right | slide in from x+0.06 + α 0→1 | 0.30 | ease-out-cubic | warlord strides in (mirror) |
| 0.28 | VS_Emblem | punch-in scale 1.6→1.0, α 0→1; VS_Glow flash | 0.22 | back-out | hero "VS" slam (peak emphasis) |
| 0.30 | TopBanner | drop from y+0.03 + α 0→1 | 0.22 | ease-out | mode/map title settles |
| 0.40 | FactionPlate_Left | slide up from y-0.04 + α 0→1 | 0.22 | ease-out | left allegiance reveal |
| 0.46 | FactionPlate_Right | slide up from y-0.04 + α 0→1 | 0.22 | ease-out | right allegiance reveal |
| 0.60 | Tip_Text | α 0→1 | 0.25 | linear | coaching line fades in |
| 0.85 | TapToContinue | becomes enabled | — | — | input now accepted |
| loop | Seam, VS_Glow | seam flicker (±4% α, ~0.9s) + VS bloom breathe (±3%, ~2.0s) | cont. | sine | living energy |
| loop | Hero rim-lights | slow specular shimmer | cont. | sine | metallic life |

**OnHide (advance):** VS_Glow quick flash (0.08s) → whole card scale 1→1.04 + α 1→0 over 0.22s ease-in →
router pushes BattleHud. **Auto-advance watchdog:** if no tap, auto-continue once the sim reports ready
(target ≈ 2.5–3.5 s minimum dwell so the card is read).

## J · PARTICLE & FX (passive — describe only)
- **Seam plasma/fire:** continuous additive electric arcs (left, cool) and rising flame tongues + drifting
  sparks (right, hot) along the seam; gentle flicker.
- **Embers (right field):** slow upward-drifting orange motes, parallax with the haze.
- **Cold mist (left field):** slow drifting blue fog wisps.
- **God-rays:** soft warm volumetric shafts from behind the VS/seam, faint pulse.
- **Hero rim shimmer:** subtle moving specular on inner-edge armor (cool left / warm right).
- **VS bloom:** soft halo breathing around the glyphs.
- All FX are **decorative**, low-cost, and must never animate over the readable banner/plate text enough to
  reduce legibility.

## K · EVENT BEHAVIOR
- **OnShow:** router pushes MatchIntro over the loading sim; CanvasGroup fades in; the build-in timeline (I)
  plays; input gated until t≈0.85. Player experience: a punchy, hype "fight card" reveal.
- **While shown:** purely ambient (looping seam/ember/bloom). No data polling — the matchup (mode, map, the two
  factions, mottos) is passed in by the caller at construction; nothing is read from ECS.
- **OnAdvance (tap or sim-ready):** OnHide transition → BattleHud screen is pushed; MatchIntro is destroyed/
  pooled. Player experience: the card "snaps" into the live battle.
- **OnHide/Back:** there is **no Back** from the intro (you can only go forward into the match); the hardware
  back button is ignored or also advances. No persistent state.

## L · NEGATIVE RULES (must-never)
- **Never** draw the seam, heroes, embers, or vignette **inside** SafeArea or let them clip the banner/plate
  text — FX live in the full-bleed BG layer only.
- **Never** add buttons, score, currencies, HP bars, or a visible "TAP TO CONTINUE" label that the mockup does
  not show (this is a clean cinematic card).
- **Never** mismatch faction colors: left is **always** Iron Pact cobalt-blue; right is **always** Ashen Horde
  ember-red. Do not swap sides.
- **Never** let the VS glyph stop being the single largest, brightest element, and never animate it so it
  obscures the heroes' faces.
- **Never** read or write ECS here; never block advance forever (the auto-advance watchdog must fire).
- **Never** stretch the hero renders or banner non-uniformly (preserve aspect; scale by height).
- **Never** use the cool motto tint on the right plate or the warm tint on the left.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% visual fidelity** to `MatchIntroDesign.png`: diptych split, blue-left/red-right heroes, central
  blue-lightning↔orange-fire seam, giant gold "VS", top "CLASSIC / STONEHOLD PASS" banner, two bottom faction
  plates (crest + name + motto), and the centered 2-line Tip — all present and correctly placed by the §E
  fractions (±2%).
- **Hierarchy:** node tree (§C) reproduced; BG/seam under heroes under foreground text; tap-catcher top-most.
- **Typography:** exact strings; VS cap-height ≥ 0.16H and largest on screen; both faction names render identical
  gold; mottos carry cool(L)/warm(R) tints; serif gold-bevel titles with stroke+shadow.
- **Safe area:** banner, plates, tip, crests fully inside `Screen.safeArea` on a notched landscape device;
  background bleeds under the cutout.
- **Eye flow:** VS → mode banner → heroes → faction plates → tip (verified by emphasis order in §I).
- **Animation:** build-in plays in the §I order with the VS slam as the peak; input gated until ≥0.85s; advance
  transition plays before BattleHud appears; auto-advance fires if untouched.
- **Affordance:** a tap anywhere advances; no dead time where the card is fully built but input is ignored
  beyond the documented gate.

## N · IMPLEMENTATION CONFIDENCE
**90 / 100.** Layout, copy, faction semantics, and the seam/VS focal treatment are unambiguous and fully
fraction-specified. The −10 is art-dependent: the exact hero renders, the painted blue/orange seam VFX, and the
crest devices must be authored as textures/shaders to hit the cinematic look — the uGUI structure and animation
are deterministic, but the final fidelity rides on those art assets and a convincing seam particle/shader.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ All sections A–O present and substantive, in order.
- □ Every visible string captured verbatim (CLASSIC / STONEHOLD PASS / VS / IRON PACT / STRENGTH. HONOR. UNITY. /
  ASHEN HORDE / STRENGTH IN CHAOS. GLORY IN CONQUEST. / the Tip line).
- □ Faction sides locked (blue=left=Iron Pact, red=right=Ashen Horde).
- □ Fraction-based layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Full-bleed BG under cutout; framed content in SafeArea.
- □ Hex values + materials given for gold/glass/crests/seam/heroes/bg.
- □ §12 boundary respected (no ECS read/write; no gameplay touched).
- □ No invented buttons/labels; transient auto-advance documented.
- □ Header + Source line in required format.



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 09 · In-Match Spell HUD

Source: design/InMatchSpellHudDesign.png · 1672×941 (1.78:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 boundary from `00_CONTEXT_RECOVERY.md`. This is the **Battle HUD in its
> spell-casting state**: the same edge chrome as `08_BattleHud`, PLUS a bottom-right **spell-slot row** with
> per-slot **cooldown timers**, a large round **commander/hero portrait** (ultimate), and — when a targeted
> spell is being aimed — a big circular **targeting telegraph** ring rendered over the battlefield. Casting is a
> presentation-level interaction; the actual spell effect is the meta/sim's concern. UI stays read-only except
> the §12-permitted writes.

---

## A · SCREEN PURPOSE
Extends the in-match HUD with **spell command**. It shows: (1) the same top/edge readouts as the Battle HUD —
gold "**150**", supply "**2/8**", left Iron Pact HP "**10,000 / 10,000**", right Ashen Horde HP
"**8,750 / 10,000**", army "**15/50**", and the 5 unit-train tiles (costs **40 / 60 / 50 / 80 / 120**) with
GARRISON / DEFEND / ATTACK; (2) a **spell-slot row** of three castable spells above the order cluster —
**Lightning (12s)**, **Heal/Restore (5s)**, **Arrow-volley (8s)** — each a framed icon with a remaining-cooldown
label; (3) a large round **commander portrait** with an ornate gold ring at the far bottom-right (the hero/
ultimate button); and (4) a **targeting telegraph** — a glowing blue concentric ring with a cardinal compass-
cross — drawn on the battlefield center while a ground-targeted spell is being aimed (the active state captured
in this mockup). Purpose: let the player fire spells/ultimate and place ground-targeted effects without leaving
the battle.

## B · VISUAL DNA (screen-specific delta from 08)
- **Same edge frame** (gold-rim chrome, top HP bars + crests, resource chips, unit tray, GARRISON/DEFEND/ATTACK)
  — re-use 08's DNA verbatim for those.
- **NEW — Spell-slot row:** three smaller gold-framed **square spell tiles** sitting just **left of the
  commander portrait** and **above the order cluster**, each showing a glowing elemental icon (electric-blue
  bolt; green life-rune; orange arrow-fan) and a small **cooldown time** beneath ("12s", "5s", "8s"). Tiles read
  as **arcane glass** (inner colored glow) rather than the steel of the unit tiles.
- **NEW — Commander/Ultimate orb:** a large **circular portrait** of the player's commander, framed by a thick
  ornate **gold ring** with a subtle glow, anchored at the bottom-right corner above ATTACK — clearly the
  highest-value cast (the "ultimate").
- **NEW — Targeting telegraph:** a large **blue concentric-circle** reticle with an inner compass cross and a
  soft radial glow, centered on the battlefield, indicating "choose a target location" for a ground spell. It is
  a **transient aiming overlay**, not permanent chrome.
- This screen's unit-tile **costs differ** from 08 (40/60/50/80/120) and the right HP is mid-fight (8,750), i.e.
  it is the same HUD at a different battle moment with the spell layer engaged.

## C · SCREEN DECOMPOSITION (ASCII node tree — delta nodes in **bold**)
```
InMatchSpellHudScreen (UiScreen, CanvasGroup, overlays live ECS battle)
└─ Root (stretch-all)
   ├─ Scrim_Top / Scrim_Bottom (Image, gradients, raycast off)        // as 08
   ├─ **TargetTelegraph (Container, battlefield-center, transient)**  // shown only while aiming
   │  ├─ **Telegraph_Glow (Image, additive)**                         // soft radial blue glow
   │  ├─ **Telegraph_RingOuter (Image)**                              // large circle
   │  ├─ **Telegraph_RingMid (Image)**                                // mid circle
   │  ├─ **Telegraph_RingInner (Image)**                              // inner circle
   │  └─ **Telegraph_Compass (Image)**                                // N/E/S/W cross + center pip
   ├─ SafeArea (RectTransform + SafeAreaFitter)
   │  ├─ TopBar (Container)                                           // pause + HP bars + chips  (see 08)
   │  │  ├─ PauseButton ( ❚❚ )
   │  │  ├─ ResChip_Gold ("150") · ResChip_Supply ("2/8")            // top-left cluster
   │  │  ├─ HpBar_Left_IronPact  (crest + trough + blue fill + "10,000 / 10,000")
   │  │  ├─ HpBar_Right_AshenHorde (crest + trough + red fill + "8,750 / 10,000")
   │  │  └─ ResChip_Army ("15/50")                                    // top-right
   │  ├─ UnitTray (Horizontal, bottom-left)                          // 5 tiles, costs 40/60/50/80/120
   │  │  └─ UnitBtn_0..4 (Portrait + CostChip + CooldownVeil + AffordTint)
   │  ├─ **SpellRow (Horizontal, bottom, left of commander)**
   │  │  ├─ **SpellBtn_Lightning (Button)** ├ ArcaneFrame + Icon_Bolt   + CdLabel "12s" + CdVeil
   │  │  ├─ **SpellBtn_Heal     (Button)** ├ ArcaneFrame + Icon_Life   + CdLabel "5s"  + CdVeil
   │  │  └─ **SpellBtn_Arrows   (Button)** └ ArcaneFrame + Icon_Arrows + CdLabel "8s"  + CdVeil
   │  ├─ **CommanderOrb (Button, bottom-right corner)**
   │  │  ├─ **Orb_Ring_Gold (Image)** · **Orb_Glow (Image)**
   │  │  ├─ **Orb_Portrait (Image, circular mask)**
   │  │  └─ **Orb_ReadyFlash / Orb_CdVeil (Image, Radial360)**
   │  └─ OrderCluster (Horizontal, bottom-right, above/left as laid out)
   │     ├─ Btn_Garrison (shield + "GARRISON")
   │     ├─ Btn_Defend   (swords + "DEFEND")
   │     └─ Btn_Attack   (chevrons + "ATTACK")                        // primary
   └─ (live ECS battlefield beneath the canvas)
```

## D · UNITY HIERARCHY SPEC (delta + shared)
*Shared nodes (Scrim, TopBar, PauseButton, HP bars, ResChips, UnitTray, OrderCluster) follow **08 §D exactly**.*
New/changed nodes:
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| TargetTelegraph | Root | 2 (above scrims, below SafeArea chrome) | Container | center (0.5,0.5) | 0.5,0.5 | follows the cursor/finger ground point while aiming | **no** (battlefield space) | scales w/ camera, not safe area |
| Telegraph_Glow | TargetTelegraph | 0 | Image | center | 0.5,0.5 | additive radial | no | — |
| Telegraph_RingOuter/Mid/Inner | TargetTelegraph | 1/2/3 | Image | center | 0.5,0.5 | concentric, slow counter-rotate | no | — |
| Telegraph_Compass | TargetTelegraph | 4 | Image | center | 0.5,0.5 | N/E/S/W cross + pip | no | — |
| SpellRow | SafeArea | 2 | HorizontalLayoutGroup | bottom, anchored right-of-tray / left-of-orb | 0,0 | 3 arcane tiles | yes | pin to lower band |
| SpellBtn_Lightning/Heal/Arrows | SpellRow | 0/1/2 | Button | layout element | 0.5,0.5 | square arcane tiles | yes | fixed cell |
| ArcaneFrame | SpellBtn | 0 | Image (sliced) | stretch-all | 0.5,0.5 | gold rim + arcane glass fill | yes | — |
| Icon_(Bolt/Life/Arrows) | SpellBtn | 1 | Image | center (inset) | 0.5,0.5 | glowing elemental glyph | yes | — |
| CdLabel | SpellBtn | 2 | Text | bottom-center | 0.5,0 | "12s"/"5s"/"8s" | yes | — |
| CdVeil | SpellBtn | 3 | Image (Filled, Radial360, origin Top) | stretch-all | 0.5,0.5 | dark sweep while on cooldown | yes | — |
| CommanderOrb | SafeArea | 3 | Button | bottom-right (1,0) | 1,0 | large round; rightmost bottom element | yes | pin BR (above ATTACK row) |
| Orb_Glow | CommanderOrb | 0 | Image | center | 0.5,0.5 | additive gold glow when ready | yes | — |
| Orb_Ring_Gold | CommanderOrb | 1 | Image | stretch-all | 0.5,0.5 | thick ornate ring | yes | — |
| Orb_Portrait | CommanderOrb | 2 | Image (circular mask) | stretch (inset) | 0.5,0.5 | commander bust | yes | — |
| Orb_CdVeil / Orb_ReadyFlash | CommanderOrb | 3 | Image (Radial360) | stretch-all | 0.5,0.5 | ult cooldown / ready pulse | yes | — |
| OrderCluster | SafeArea | 4 | HorizontalLayoutGroup | bottom-right (1,0) | 1,0 | shifts left of the orb | yes | pin BR |

**Order note:** SpellRow and CommanderOrb sit in the bottom band between the UnitTray (BL) and ATTACK (BR). The
CommanderOrb is the rightmost, largest bottom element; the SpellRow is immediately to its left; the
GARRISON/DEFEND/ATTACK cluster sits below/inline at the far right corner (matching the mockup, where ATTACK's
chevrons read at the very corner and the orb sits just above the order row).

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Shared chrome (top HP bars, pause, chips, unit tray, order cluster):** identical to **08 §E** (same band
  geometry). **Difference:** unit-tile costs are **40/60/50/80/120**; right HP text is **8,750 / 10,000**. In
  this mockup the gold "150"+supply "2/8" chips and the left HP bar are clustered top-LEFT (HP bar reads just
  below the chips), army "15/50" top-RIGHT — reproduce 08's chip/bar fractions; if the left HP bar visually sits
  beneath the chip row, anchor it at y≈0.90 left-of-center rather than spanning (this mockup shows the left HP
  bar lower-left under the chips). Keep both bars ≈0.30W where shown.
- **Clear battlefield zone:** same inviolable center x∈[0.20,0.80], y∈[0.20,0.82]; the telegraph is allowed to
  render here (it's an aiming overlay, not blocking chrome — it doesn't consume the eventual confirm tap; the
  confirm happens on release at the ground point).
- **TargetTelegraph:** outer ring Ø ≈ **0.42H** (≈454 px) when shown at default range; centered on the aim point
  (the mockup shows it near screen-center, ≈(0.42, 0.52)). Mid ring ≈0.62× outer, inner ≈0.34× outer; compass
  cross spans the mid ring; soft glow halo ≈1.4× outer. Stroke widths: outer ≈4px, mid ≈3px, inner ≈2px @1080.
- **SpellRow:** 3 tiles each ≈ **0.072W × 0.108H** (≈168×117 px), spacing ≈0.010W. The row sits in the bottom-
  right region, its right edge ≈0.06W to the **left** of the CommanderOrb, bottom aligned ≈0.165H above safe
  bottom (so it sits **above** the GARRISON/DEFEND/ATTACK row). CdLabel cap-height ≈0.020H on the tile's lower
  edge.
- **CommanderOrb:** Ø ≈ **0.165H** (≈178 px) including the ring; centered at ≈(0.945, 0.30 from bottom), i.e.
  right edge ≈0.985W, vertical center ≈0.30H up from safe bottom — clearly the largest bottom-right element,
  sitting just above the ATTACK button. Gold ring thickness ≈0.018H; portrait inner Ø ≈0.13H.
- **OrderCluster:** as 08 (3 buttons ≈0.105W×0.105H), pinned bottom-right corner; the cluster sits **below** the
  orb/spell row so all three groups stack without overlap in the lower-right quadrant.
- **Tablet / Ultrawide / Notch:** identical strategy to 08 — corner-pinned groups, center widens on ultrawide,
  SafeArea protects pause/army/orb/spell row from notches; the telegraph scales with the camera/world, not the
  safe area, and is clamped to stay on-screen.

## F · TYPOGRAPHY (per text)
*HP / gold / supply / army / cost / GARRISON-DEFEND-ATTACK typography = **08 §F** (only the cost values change to
40/60/50/80/120 and right HP to 8,750 / 10,000).* New text:
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| CdLabel (Lightning) | 12s | tabular numerals | Bold | lower 's' | 0 | 1px #06121c stroke + soft blue inner glow + drop-shadow | ~22 | #cfe6ff (icy blue) |
| CdLabel (Heal) | 5s | tabular numerals | Bold | lower 's' | 0 | 1px #07140a stroke + soft green glow | ~22 | #cdf3cf (life green) |
| CdLabel (Arrows) | 8s | tabular numerals | Bold | lower 's' | 0 | 1px #1a0c05 stroke + soft amber glow | ~22 | #ffe1b0 (amber) |

Cooldown labels render in their spell's elemental tint so the player parses element + readiness at a glance. When
a spell is **ready**, the CdLabel hides (no "0s"); when on cooldown it counts down whole seconds.

## G · MATERIALS
*Shared chrome materials = **08 §G**.* New materials:
- **Arcane spell tiles:** gold/bronze rim (as 08 frames) over a **dark arcane-glass** core (#0b0d16 @ ~80%) that
  carries the spell's inner colored glow: Lightning = electric blue #4aa8ff core with white sparks; Heal =
  verdant #46d06a with soft motes; Arrows = amber #ffae4a. Glass has a faint specular and a slight inner bloom;
  on cooldown the core desaturates and the CdVeil paints a dark radial sweep.
- **Spell icons:** Lightning = jagged electric **bolt**; Heal = a **life-rune/leaf-cross** (green); Arrows = a
  **fan of three arrows** (amber). Each glows additively.
- **Commander orb:** thick ornate **cast-gold ring** (base #8a6a2c, highlight #f6dd86, shadow #574114) with
  engraved filigree and a polished bevel; a soft gold **ready-glow** halo behind it; the inner **portrait** is a
  painted commander bust (faction-tinted, low-key lit) circularly masked; a thin inner gold liner separates ring
  from portrait. When charging, a dark radial veil overlays the portrait; when ready, the ring brightens + the
  halo pulses.
- **Targeting telegraph:** translucent **cobalt-blue** rings (#3f8bff @ ~70%, additive) with a brighter inner
  ring, a faint cross-grid compass, and a soft radial floor-glow that fakes a projected light circle on the
  ground; gentle rotation gives it "active scanning" life. Color stays **blue** (the player/Iron Pact aiming
  color) regardless of spell element, matching the mockup's blue reticle.

## H · COMPONENTS (each interactive)
*PauseButton, HP bars, ResChips, UnitBtn tiles, Garrison/Defend/Attack = **08 §H** (same behavior; only costs/
values differ).* New components:
1. **SpellBtn_Lightning / Heal / Arrows** — *purpose:* cast that spell. Lightning & Arrows are **ground-targeted**
   (tap arms → telegraph appears → tap/drag on the battlefield to place → release confirms); Heal may be
   **instant/self** (tap fires immediately). *States:* **idle/ready** = arcane frame, glowing icon, no CdLabel;
   **hover/focus** = frame + icon brighten; **armed** (ground-targeted, after first tap) = persistent selection
   ring + the TargetTelegraph shown on the field; **pressed/cast** = bright elemental flash + icon punch; **on
   cooldown** = CdVeil radial sweep + dimmed icon + CdLabel counting down (e.g. "12s"→…→hidden at ready);
   **disabled/unaffordable** = desat + (if a mana/gold gate exists) tint, raycast on for a deny shake.
   *Structure:* ArcaneFrame, Icon, CdLabel, CdVeil. *Feedback:* arm = soft hum; cast = elemental SFX + screen
   micro-flash; cooldown start = veil wipe. *(Spell effect is meta/sim; UI issues the cast request via the
   permitted control path; no balance written client-side.)*
2. **CommanderOrb (Ultimate)** — *purpose:* fire the commander's ultimate/ability. *States:* **idle/charging** =
   ring dim, Orb_CdVeil radial showing charge, portrait slightly desat; **ready** = ring bright + Orb_Glow
   pulsing halo + portrait fully lit (strong "use me" affordance); **hover/focus** = halo intensifies; **pressed/
   cast** = big gold flash + ring spin burst; **disabled** = greyed. *Structure:* glow + gold ring + circular
   portrait + cd/ready veil. *Feedback:* cast = heroic SFX + camera/field flash; then recharge veil restarts.
3. **TargetTelegraph** — *purpose:* show the chosen ground location while a targeted spell is armed. *States:*
   **hidden** (default); **aiming** = visible, follows the finger/cursor ground point, rings rotate; **valid** =
   blue (placeable); **invalid** = tinted red (out of range / blocked); **confirm** = quick bright pulse +
   collapse on release. *Structure:* glow + 3 rings + compass. *Feedback:* purely visual aiming aid; the actual
   placement/confirm is the cast tap on the battlefield. It is **not** a blocking button.

## I · ANIMATION TIMELINE
**OnShow:** identical edge-chrome slide-in to **08 §I**, PLUS:
| t (s) | Element | Action | Dur | Easing |
|---|---|---|---|---|
| 0.16 | SpellRow | slide up + stagger tiles 0.03s | 0.25 | ease-out |
| 0.20 | CommanderOrb | scale 0.85→1 + glow fade-in | 0.25 | back-out |
| 0.34 | CommanderOrb (if ready) | one-shot ready halo pulse | 0.40 | ease-in-out |

**Reactive:**
- Spell arm: tap → selection ring fades in (0.12s) + TargetTelegraph fades/scales in (rings from 0.6→1.0 over
  0.18s) at the aim point.
- While aiming: outer/mid rings counter-rotate (~6s/rev), inner pip breathes ±4% (~1.2s).
- Cast: elemental flash on tile (0.12s) + telegraph quick bright pulse then collapse (0.15s) → CdVeil begins a
  full radial sweep over the spell's cooldown (12s/5s/8s), CdLabel counts down.
- Commander ready: ring brightness +20% + halo breathe (±6%, ~1.6s) continuously until cast; cast = gold burst
  + spin then recharge veil from 0→full over the ult cooldown.
- Cooldown finish: CdVeil vanishes with a quick gold ring-flash; CdLabel hides.

**OnHide:** as 08 (chrome retracts); any active telegraph is force-hidden.

## J · PARTICLE & FX (passive — describe only)
- **Arcane tile cores:** slow drifting motes in each element's color (blue sparks / green life-motes / amber
  dust); faint inner bloom.
- **Commander orb:** soft rotating gold rim-glint + a gentle ready-halo pulse.
- **Telegraph:** rotating ring shimmer + soft projected floor-glow + tiny inward-drifting blue particles toward
  the center pip (reads as "channeling here").
- Shared 08 passives (gold rim-bloom, HP glows) still apply.
- No persistent particles clutter the battlefield center except the transient telegraph while aiming.

## K · EVENT BEHAVIOR
- **OnShow:** same as 08 plus spell/ult readiness subscribed (read-only) and rendered as cooldown veils/labels.
- **OnSpellTap (targeted):** arm → show telegraph → on battlefield release, issue the cast at the ground point →
  start cooldown. **OnSpellTap (instant, Heal):** fire immediately → cooldown. If on cooldown/disabled: deny
  shake.
- **OnCommanderTap:** if ready, fire ultimate → recharge; else pulse "not ready."
- **OnOrder / OnTrain / OnPause / battlefield taps:** exactly as **08 §K** (the spell layer is additive; the unit
  tray, order cluster, pause, and read-only readouts behave identically).
- **Cancel aim:** tapping the armed spell again, or a back/cancel gesture, hides the telegraph without casting.
- **OnHide:** retract chrome; release bindings; hide telegraph.

## L · NEGATIVE RULES (must-never)
- **Never** leave the TargetTelegraph on-screen when no spell is armed; it is transient.
- **Never** make the telegraph a raycast-blocking element that eats the battlefield confirm tap.
- **Never** show "0s" on a ready spell — hide the CdLabel at ready.
- **Never** recolor the telegraph by element — it stays the player's **blue** aiming color (per mockup).
- **Never** let the SpellRow/CommanderOrb overlap the GARRISON/DEFEND/ATTACK cluster or the central battlefield
  zone; stack them cleanly in the lower-right.
- **Never** violate §12: spell/ult casts go through the permitted control path; no client-side balance/effect
  mutation; everything else read-only.
- **Never** drop the shared 08 rules (faction color sides, clear center, unaffordable train state, single-select
  stance group).
- **Never** invent spell counts beyond the **three** slots + **one** commander orb shown.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `InMatchSpellHudDesign.png`: all shared 08 chrome present with this mockup's values
  (costs 40/60/50/80/120; right HP 8,750/10,000; gold 150; supply 2/8; army 15/50); a 3-slot SpellRow with
  Lightning "12s" / Heal "5s" / Arrows "8s"; a large gold-ringed CommanderOrb at bottom-right; and the blue
  concentric targeting telegraph over the battlefield — all per §E (±2%).
- **Hierarchy:** §C tree (telegraph above scrims; spell row + orb in SafeArea; veils/labels per spell).
- **Typography:** cooldown labels in elemental tints, tabular; hidden at ready; shared text matches 08.
- **States:** spells arm/cast/cooldown correctly; targeted spells show the telegraph; orb shows charging vs ready;
  telegraph is transient + non-blocking.
- **Clear center + safe area + §12:** inherited from 08 and verified (orb/spell row inside safeArea; telegraph in
  world space, clamped on-screen; only permitted writes).
- **Eye flow:** CommanderOrb (largest, glowing) → SpellRow → ATTACK → unit tray → top readouts.

## N · IMPLEMENTATION CONFIDENCE
**85 / 100.** The additive spell layer is structurally clear and fraction-specified, and the cooldown/telegraph
patterns are standard. The −15: (a) spell/ultimate art + the painted telegraph shader are assets to build;
(b) the cast/aim flow (arm→telegraph→confirm) and what control path the cast uses must be reconciled with the
real spell system within the §12 boundary (the mockup only shows the aiming state); (c) the exact left-HP-bar
placement in this frame (it reads lower-left, beneath the chip row) is slightly ambiguous vs 08's top-spanning
bars and is called out as an anchor decision.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O present, in order, substantive; deltas vs 08 clearly marked.
- □ Numbers captured: costs 40/60/50/80/120; HP 10,000/10,000 & 8,750/10,000; gold 150; supply 2/8; army 15/50;
  spell cooldowns 12s/5s/8s.
- □ Three spell slots + one commander orb (no invented extras).
- □ Telegraph is transient, non-blocking, stays blue, clamped on-screen.
- □ Shared chrome reuses 08 (sides locked, center clear, train-affordability, single-select stance).
- □ §12 respected for casting; everything else read-only.
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Hex + materials for arcane tiles, commander orb, telegraph.
- □ Header + Source line in required format.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 10 · In-Match Banner (Objective / Wave / Event)

Source: design/InMatchBannerDesign.png · 1824×862 (2.12:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 boundary from `00_CONTEXT_RECOVERY.md`. This is a **transient announcement
> overlay** that drops over the live Battle HUD (08) + Spell HUD (09) to call out a **wave / objective / event**
> ("WAVE 12: THE DEAD AWAKEN") with a **countdown timer** ("02:14"). The full in-match HUD stays visible and
> functional beneath it; the banner is a top-center heraldic title strip with flanking cloth banners. It reads
> the event/timer from the sim **read-only** and writes nothing.

---

## A · SCREEN PURPOSE
A **dramatic event banner** that announces the current wave/objective during a fight without leaving the HUD. It
shows (1) a large **ornate title strip** across the top-center reading the event name — here **"WAVE 12: THE DEAD
AWAKEN"** — topped by a small horned-skull finial and flanked by **hanging red cloth banners**; (2) a
**countdown timer** "**02:14**" with a small hourglass/clock glyph directly beneath the title (time until the
wave hits / objective deadline); all while (3) the **entire live HUD remains visible and usable** beneath it —
gold "**450**", supply "**3/8**", left Iron Pact HP "**10,000 / 10,000**", right Ashen Horde HP
"**8,300 / 10,000**", army "**42/50**", the 5 unit-train tiles (costs **60 / 90 / 75 / 120 / 150**), the
3-slot spell row (**12s / 8s / 15s**) + commander orb, and GARRISON / DEFEND / ATTACK. The battlefield also
shows blue (player) and red (enemy) **movement-path arrows**. Purpose: communicate the stakes of the moment with
high drama, then auto-dismiss so play continues.

## B · VISUAL DNA (screen-specific)
- **Top-center heraldic title strip:** a long horizontal **ornate gold/bronze plaque** with scrolled, pointed
  end-caps and engraved filigree, carrying the event title in big serif gold-bevel UPPERCASE. A small **horned
  skull** crest sits at the top-center apex (this wave is undead-themed → ember/oxblood accent on the frame).
- **Flanking cloth banners:** two **hanging red (oxblood) cloth pennants** with gold trim and a faint sigil,
  draping down from the strip's outer ends — they frame the title and reinforce the Ashen/undead threat.
- **Countdown plate:** a smaller dark gold-rim lozenge directly under the title with a clock/hourglass glyph + a
  **MM:SS timer** in glowing numerals.
- **Banner is an overlay, not full chrome:** behind it the **complete Battle/Spell HUD** is rendered (top HP
  bars + crests, resource chips, unit tray, spell row + commander orb, order cluster) — all dimmed slightly by a
  soft top scrim so the title pops, but still readable and interactive.
- **Battlefield movement arrows:** stylized **blue** (player advance) and **red** (enemy advance) curved arrow
  paths overlaid on the field, indicating troop movement vectors — part of this in-match moment's storytelling.
- Mood: ominous, ember-lit, "the horde is coming" — danger red dominates the banner while the HUD keeps its
  gold/blue/red faction language.

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
InMatchBannerOverlay (UiScreen OR additive overlay layer above BattleHud/SpellHud)
└─ Root (stretch-all)
   ├─ Scrim_BannerTop (Image, top gradient, soft, raycast OFF)        // gently dims HUD top for title pop
   ├─ **BannerGroup (Container, top-center, transient)**
   │  ├─ **Banner_Cloth_Left (Image)**                                // hanging red pennant (left)
   │  ├─ **Banner_Cloth_Right (Image)**                               // hanging red pennant (right)
   │  ├─ **Banner_Frame_Gold (Image, sliced)**                        // ornate plaque + scroll caps
   │  ├─ **Banner_SkullFinial (Image)**                               // horned-skull crest at apex
   │  ├─ **Banner_Title (Text "WAVE 12: THE DEAD AWAKEN")**
   │  └─ **Timer_Plate (Container)**
   │     ├─ **Timer_Frame (Image)** · **Timer_ClockIcon (Image)**
   │     └─ **Timer_Text (Text "02:14")**
   ├─ **PathArrows_Layer (Container, battlefield space, raycast OFF)** // movement vectors
   │  ├─ **Arrow_Player_Blue (Image, ×N)**
   │  └─ **Arrow_Enemy_Red (Image, ×N)**
   └─ [BENEATH] BattleHud (08) + SpellHud (09) chrome — rendered & interactive:
      TopBar{Pause, HP_L"10,000/10,000"+crest, HP_R"8,300/10,000"+crest, Gold"450", Supply"3/8", Army"42/50"},
      UnitTray{costs 60,90,75,120,150}, SpellRow{12s,8s,15s}, CommanderOrb, OrderCluster{GARRISON,DEFEND,ATTACK}
```

## D · UNITY HIERARCHY SPEC (per node — banner-specific; HUD beneath follows 08/09)
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Overlay | 0 | RectTransform | stretch-all | 0.5,0.5 | sits above HUD canvas (higher sort order) | n/a | fills |
| Scrim_BannerTop | Root | 0 | Image | top stretch (0,0.7)-(1,1) | 0.5,1 | soft dark→clear, raycast OFF (HUD stays tappable) | no | full width |
| BannerGroup | Root | 1 | Container | top-center (0.5,1) | 0.5,1 | hangs from safe top | **yes** | center, fixed height |
| Banner_Cloth_Left | BannerGroup | 0 | Image | left end, hanging down | 0.5,1 | red pennant | yes | — |
| Banner_Cloth_Right | BannerGroup | 1 | Image | right end, hanging down | 0.5,1 | red pennant (mirror) | yes | — |
| Banner_Frame_Gold | BannerGroup | 2 | Image (sliced) | stretch-all (of group strip) | 0.5,0.5 | 9-slice w/ scroll caps | yes | width grows with title |
| Banner_SkullFinial | BannerGroup | 3 | Image | top-center apex | 0.5,0 | above the strip | yes | center-locked |
| Banner_Title | BannerGroup | 4 | Text | center of strip | 0.5,0.5 | event name | yes | auto-size to fit strip |
| Timer_Plate | BannerGroup | 5 | Container | below strip, center | 0.5,1 | hangs under title | yes | center |
| Timer_Frame | Timer_Plate | 0 | Image | stretch-all | 0.5,0.5 | dark gold-rim lozenge | yes | — |
| Timer_ClockIcon | Timer_Plate | 1 | Image | left-inside | 0.5,0.5 | hourglass/clock | yes | — |
| Timer_Text | Timer_Plate | 2 | Text | center/right | 0.5,0.5 | "02:14" | yes | — |
| PathArrows_Layer | Root | 2 | Container | center (battlefield) | 0.5,0.5 | over field, raycast OFF | **no** | world-aligned |
| Arrow_Player_Blue | PathArrows_Layer | 0..n | Image | per-vector | varies | blue curved arrow | no | follows sim vectors |
| Arrow_Enemy_Red | PathArrows_Layer | 0..n | Image | per-vector | varies | red curved arrow | no | follows sim vectors |
| (HUD beneath) | separate canvas/lower sort | — | — | — | — | unchanged from 08/09 | yes | unchanged |

**Sort/order rationale:** the banner overlay renders **above** the HUD (higher canvas sortingOrder or a child
appended last) so the title sits over the top of the screen, but its scrim and arrow layers are **raycast OFF**
so the HUD beneath (pause, train, spells, orders, battlefield) stays fully interactive while the banner is up.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Banner strip:** width ≈ **0.46W** (≈1076 px) × height ≈ **0.11H** (≈119 px); centered x=0.50, the strip's
  vertical center at ≈y=0.90 (top edge ≈0.045H below safe top). Title cap-height ≈ **0.060H** (≈65 px),
  horizontally fit/auto-sized within the strip's inner width (≈0.40W usable).
- **Skull finial:** Ø ≈ **0.055H**, centered x=0.50, its base touching the strip's top edge (apex at ≈y=0.965).
- **Cloth pennants:** each ≈ **0.06W** wide × **0.16H** tall, hanging from the strip's outer ends (left pennant
  centered x≈0.27, right x≈0.73), their tops behind the strip ends, draping down to ≈y=0.74. Slight outward
  splay.
- **Timer plate:** ≈ **0.115W × 0.052H**, centered x=0.50, its top touching the strip's bottom (vertical center
  ≈y=0.825). Clock icon Ø≈0.032H at the plate's left; "02:14" cap-height ≈ **0.030H**, right of the icon.
- **Clear battlefield zone:** the banner + timer occupy only the top strip (y≳0.78); the central battlefield
  x∈[0.20,0.80], y∈[0.20,0.78] stays clear of blocking chrome (the banner overlay is non-blocking anyway).
- **Path arrows:** stylized curved arrows ≈0.10–0.18W long, drawn between unit groups on the field (blue from the
  player's left mass toward the center/right; red from the enemy's right mass toward the center/left). They are
  decorative-over-sim indicators, positioned by the sim's movement vectors, clamped to the field area.
- **HUD beneath:** all fractions from **08 §E / 09 §E** (this mockup shows the full spell-HUD variant). Values:
  gold **450**, supply **3/8**, HP_L **10,000/10,000**, HP_R **8,300/10,000**, army **42/50**, unit costs
  **60/90/75/120/150**, spell cooldowns **12s/8s/15s**, GARRISON/DEFEND/ATTACK present (ATTACK at far corner).
- **Tablet (1.33:1):** strip width grows to ≈0.55W so the long title still fits at the same cap-height; pennants
  pull in slightly. **Ultrawide (21:9):** strip stays ≈0.46W centered (extra width is margin); HUD corner groups
  unchanged. **Notch:** the whole BannerGroup is inside SafeArea so a top/side cutout never clips the title,
  skull, or timer; the soft scrim passes under.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| Banner_Title | WAVE 12: THE DEAD AWAKEN | epic Trajan serif, gold bevel | **Black** | UPPER | +5% | gold bevel + 1.5px #1f1405 stroke + drop-shadow (0,3,#000 70%) + faint ember outer glow | ~65 | gradient **#f8e9b4→#caa04a**, slight ember warmth at the lower edge |
| Timer_Text | 02:14 | tabular numerals, urgent | Bold | — | +2% | 1px #2a1305 stroke + soft amber/ember inner glow + drop-shadow | ~32 | #ffd98a (urgent gold); shifts toward **#ff8a5a** when <0:30 |

The title is the **single largest, brightest** element while the banner is up (it temporarily out-ranks the
HUD's ATTACK). The timer reads as urgent gold and warms toward ember as it runs low. HUD text typography = 08/09
§F unchanged.

## G · MATERIALS
- **Banner plaque (gold/bronze, ember-accented):** base #8a6a2c, highlight #f3d680, shadow #523c14; satin metal
  (roughness ≈0.35) with deeply **engraved filigree** and scrolled/pointed end-caps; because the event is
  undead-themed the frame carries faint **ember/oxblood** inlays (#7a1f1a) along its channels and a warm rim-
  bloom; light edge wear.
- **Skull finial:** aged bone/iron horned skull, blackened metal mounts, faint red eye-glow; oxblood accent.
- **Cloth pennants:** **oxblood red** woven cloth (#5a1714→#8a241c) with **gold trim** and stitched edges, a
  faint darker sigil woven in; soft cloth roughness, gentle drape folds, subtle bottom fray; low specular.
- **Timer plate:** dark glass core (#0c0e14 @ ~85%) with a gold/bronze rim; the clock/hourglass icon is brushed
  gold; numerals carry an inner amber glow.
- **Path arrows:** translucent additive ribbons — **blue** (#3f8bff core, white edge) for the player, **red**
  (#d8452b core) for the enemy; soft outer glow, slight animated flow along their length.
- **Scrim:** soft vertical alpha gradient #05060a (≈0→0.45) at the top only — just enough to lift the title.
- HUD beneath uses 08/09 §G materials unchanged.

## H · COMPONENTS (interactive)
The banner overlay is **almost entirely non-interactive display**; the interactivity lives in the HUD beneath
(which keeps **08 §H / 09 §H** behavior verbatim — pause, train tiles, spells, commander orb, order cluster, HP
read-outs). Banner-specific elements:
1. **Banner_Title / Banner_Frame / Skull / Cloth** — *purpose:* announce the event. *States:* value states only
   — **enter** (dramatic build-in), **hold** (steady, subtle cloth sway + frame rim breathe), **exit** (slide
   up / fade). No idle/hover/pressed (not a button). *Feedback:* none (display).
2. **Timer_Text** — *purpose:* live countdown to the wave/deadline (read-only from sim). *States:* **normal**
   (gold), **urgent** (<0:30 → warms to ember + a soft per-second pulse), **expired** (00:00 → quick flash, then
   the banner exits as the wave triggers). *Structure:* clock icon + MM:SS. *Feedback:* visual urgency only.
3. **PathArrows (blue/red)** — *purpose:* visualize movement vectors (read-only). *States:* animated flow; appear/
   fade with the relevant orders/waves. *Feedback:* display; never interactive.

*(If product wants a "tap to dismiss" the banner could accept a tap, but the mockup shows no dismiss affordance;
default is **auto-dismiss by timer/animation**, not a button — see L.)*

## I · ANIMATION TIMELINE (banner enter → hold → exit; t in s)
| t (s) | Element | Action | Dur | Easing | Emphasis |
|---|---|---|---|---|---|
| 0.00 | Scrim_BannerTop | α 0→0.45 | 0.20 | linear | top dims slightly |
| 0.02 | Banner_Frame_Gold | drop from y+0.05 + scale-X 0.7→1 + α 0→1 | 0.30 | back-out | the plaque slams in |
| 0.10 | Banner_Cloth_L/R | unfurl: scale-Y 0.4→1 from top + α 0→1 | 0.30 | ease-out | pennants drop/unroll |
| 0.14 | Banner_SkullFinial | pop scale 0.5→1 + α | 0.22 | back-out | crest seats at apex |
| 0.20 | Banner_Title | type/reveal: α 0→1 + scale 1.08→1 + ember glow flash | 0.28 | ease-out | **peak** — event name |
| 0.36 | Timer_Plate | drop from y+0.03 + α 0→1 | 0.22 | ease-out | countdown appears |
| hold | Cloth, Frame | cloth sway ±2° (~2.2s), frame rim-bloom breathe (~2.0s) | cont. | sine | living banner |
| hold | Timer_Text | tick down; pulse each second when urgent (<0:30) | cont. | — | urgency |
| exit (≈2.5–4s, or on expiry) | BannerGroup | slide up to y+0.06 + α 1→0 (cloth furls back) | 0.30 | ease-in | clears for play |
| exit | Scrim_BannerTop | α →0 | 0.25 | linear | HUD un-dims |

**Path arrows:** flowing dash animation along each arrow (~1.0s loop) while shown; fade in/out with their order.

## J · PARTICLE & FX (passive — describe only)
- **Ember motes** rising around the skull finial / banner top (undead theme); faint warm bloom on the frame.
- **Cloth sway** + soft cast shadow under the pennants.
- **Timer urgency:** a subtle ember pulse-ring around the timer plate when <0:30.
- **Path arrows:** traveling light-dash flow (blue / red) along each vector.
- HUD passives (08/09 §J) continue beneath. No FX intrudes on the readable battlefield center.

## K · EVENT BEHAVIOR
- **OnEvent (wave/objective fires):** the sim/meta raises an event with {title, optional duration}; the overlay
  is pushed **above** the live HUD; the enter timeline (I) plays. Player: a dramatic "WAVE 12: THE DEAD AWAKEN"
  announcement without losing control of the battle.
- **While shown:** Timer_Text counts down from the supplied duration (read-only); cloth sways; the **HUD beneath
  stays fully interactive** (the player can keep training, casting, ordering — the banner's scrim/arrow layers are
  raycast-off). Path arrows reflect current movement vectors.
- **OnExpire / OnTimeout:** at 00:00 (or after a min dwell with no timer) the exit timeline plays and the overlay
  is removed; the wave/objective proceeds in the sim.
- **OnHide:** overlay destroyed/pooled; HUD un-dims; no persistent state. The banner never pauses the game
  (unlike the Pause modal) — play continues throughout.

## L · NEGATIVE RULES (must-never)
- **Never** block the HUD or battlefield: the banner's scrim and arrow layers are **raycast OFF**; the player can
  always pause/train/cast/order while it's up.
- **Never** pause the sim for the banner (it is non-modal; only the Pause modal sets timeScale 0).
- **Never** clip the title/skull/timer under a notch — BannerGroup stays in SafeArea.
- **Never** mismatch arrow colors: blue = player advance, red = enemy advance.
- **Never** leave the banner up indefinitely — it must auto-exit on timer expiry or after its dwell.
- **Never** add a dismiss button the mockup doesn't show (auto-dismiss only) unless product later requires it.
- **Never** let the banner's drama reduce HUD legibility below readable (scrim ≤0.45 alpha, top only).
- **Never** read/write beyond the §12 boundary; the banner consumes a read-only event + timer, nothing more.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `InMatchBannerDesign.png`: top-center ornate banner reading **"WAVE 12: THE DEAD AWAKEN"**
  with a horned-skull finial and two hanging red pennants; a "**02:14**" countdown plate beneath; the full live
  HUD visible behind with this mockup's values (gold 450, supply 3/8, HP_L 10,000/10,000, HP_R 8,300/10,000,
  army 42/50, unit costs 60/90/75/120/150, spell row 12s/8s/15s + commander orb, GARRISON/DEFEND/ATTACK); plus
  blue/red battlefield path arrows — all per §E (±2%).
- **Non-modal overlay:** HUD beneath stays fully interactive; scrim/arrows raycast-off; sim NOT paused.
- **Hierarchy:** §C tree; banner renders above HUD; cloth→frame→skull→title→timer order.
- **Typography:** exact title string + "02:14"; title is largest/brightest while shown; timer warms when urgent.
- **Animation:** dramatic enter (frame slam → cloth unfurl → skull pop → title reveal → timer drop), hold sway,
  auto-exit on timer expiry.
- **Safe area + §12:** banner inside safeArea on a notched device; only a read-only event/timer consumed.

## N · IMPLEMENTATION CONFIDENCE
**87 / 100.** The overlay is structurally simple, fraction-specified, and the non-modal "announce over a live
HUD" pattern is well understood. The −13: (a) the ornate plaque art, cloth pennants, and skull finial are assets
to author; (b) the path-arrow rendering must be wired to the sim's movement vectors (the mockup only shows a
snapshot) and kept read-only; (c) banner dwell/exit timing and the event/timer source must be hooked to the
real wave system within §12.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O present, in order, substantive.
- □ Strings/numbers captured: "WAVE 12: THE DEAD AWAKEN", "02:14", gold 450, supply 3/8, HP 10,000/10,000 &
  8,300/10,000, army 42/50, costs 60/90/75/120/150, spell cooldowns 12s/8s/15s.
- □ Overlay is non-modal & non-blocking (raycast-off scrim/arrows; sim not paused).
- □ HUD beneath reuses 08/09; banner renders above; title is the temporary focal.
- □ Arrow colors locked (blue=player, red=enemy); banner inside SafeArea.
- □ Auto-exit on timer expiry; no invented dismiss button.
- □ §12 respected (read-only event/timer).
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Hex + materials for plaque/cloth/skull/timer/arrows.
- □ Header + Source line in required format.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 11 · Pause Modal (In-Match)

Source: design/PauseModalDesign.png · 1782×883 (2.02:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 boundary from `00_CONTEXT_RECOVERY.md`. This is the **in-match Pause
> modal**: a centered ornate gold-framed panel over a **dimmed, frozen battlefield**, offering **Resume /
> Settings / Surrender**. Opening it sets `Time.timeScale = 0` (the one permitted write here); closing restores
> it. The HUD beneath stays rendered but inert under the modal scrim.

---

## A · SCREEN PURPOSE
The pause overlay. When the player taps the HUD's Pause (❚❚) button, the sim freezes (`Time.timeScale = 0`) and
this **modal panel** appears centered over a darkened battlefield. It presents exactly three stacked actions:
(1) **RESUME** (primary, blue) — close the modal and unfreeze; (2) **SETTINGS** (neutral/dark) — open the
in-match settings; (3) **SURRENDER** (danger, oxblood-red) — concede the match (routes to a defeat/result flow,
typically via a confirm). A serif **"PAUSED"** title sits in the panel's gold header crowned by a small **gem
finial**. It is a true modal: input is captured by the panel + a dimming scrim; the battle is paused beneath.

## B · VISUAL DNA (screen-specific)
- **Centered ornate panel:** a near-black rounded-rectangle plate bordered by a thick **cast-gold/bronze frame**
  with engraved filigree and **decorative corner bosses/cartouches** at all four corners; a small **violet/blue
  gem** set into a gold mount crowns the top-center of the frame (the prestige finial).
- **"PAUSED" title:** serif gold-bevel UPPERCASE in the panel's upper region, with shadow + stroke.
- **Three stacked buttons**, full panel-width, generous vertical gaps, each a beveled gold-rimmed capsule:
  - **RESUME** — **royal/cobalt blue** fill (the brightest, primary CTA), small gold flourishes/diamond accents
    flanking the label.
  - **SETTINGS** — **neutral dark steel** fill (secondary), same gold rim + flourishes.
  - **SURRENDER** — **oxblood/ember red** fill (danger), same rim + flourishes.
- **Dimmed battlefield behind:** a frozen war scene — a fortress/keep upper-left, **blue (Iron Pact) banners** on
  the left, **red (Ashen Horde) banners + fire** on the right, troops mid-field — pushed dark/desaturated by a
  full-screen scrim so the panel is the clear focus. Strong vignette; faint warm fire glow at the right edge.
- Mood: solemn "the war waits" interlude — calm, regal, with the danger of Surrender clearly color-coded.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
PauseModalScreen (UiScreen, CanvasGroup, MODAL over frozen battlefield)
└─ Root (stretch-all)
   ├─ Scrim_Dim (Image, stretch-all, raycast ON)            // dims battlefield + captures outside taps
   ├─ BG_BattlefieldFrozen (visual reference only — the live HUD/field rendered beneath, inert)
   │     // NOT a child of this modal; shown here for context: keep/fortress UL, blue banners L,
   │     // red banners + fire R, troops mid-field — all dimmed by Scrim_Dim
   └─ SafeArea (RectTransform + SafeAreaFitter)
      └─ Panel (Container, centered)
         ├─ Panel_Frame_Gold (Image, sliced)                // ornate gold/bronze border + corner bosses
         ├─ Panel_BG_Dark (Image)                           // near-black inner plate
         ├─ Panel_GemFinial (Image)                         // gem in gold mount, top-center apex
         ├─ Title_Paused (Text "PAUSED")
         └─ ButtonStack (VerticalLayoutGroup)
            ├─ Btn_Resume (Button)                          // PRIMARY blue
            │  ├─ Resume_Frame (Image) · Resume_Fill (Image, blue)
            │  ├─ Resume_FlourishL (Image) · Resume_FlourishR (Image)
            │  └─ Resume_Label (Text "RESUME")
            ├─ Btn_Settings (Button)                        // neutral dark
            │  ├─ Settings_Frame (Image) · Settings_Fill (Image, dark steel)
            │  ├─ Settings_FlourishL/R (Image)
            │  └─ Settings_Label (Text "SETTINGS")
            └─ Btn_Surrender (Button)                       // danger red
               ├─ Surrender_Frame (Image) · Surrender_Fill (Image, oxblood)
               ├─ Surrender_FlourishL/R (Image)
               └─ Surrender_Label (Text "SURRENDER")
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Root | Screen | 0 | RectTransform | stretch-all | 0.5,0.5 | offsets 0 | n/a | fills |
| Scrim_Dim | Root | 0 | Image | stretch-all | 0.5,0.5 | #05060a @ ~0.62; **raycast ON** (eats outside taps) | **no** (covers full bleed incl. under cutout) | full-bleed |
| SafeArea | Root | 1 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | insets to Screen.safeArea | **yes** | panel centers here |
| Panel | SafeArea | 0 | Container | center (0.5,0.5) | 0.5,0.5 | the modal | yes | center-locked |
| Panel_Frame_Gold | Panel | 0 | Image (sliced) | stretch-all | 0.5,0.5 | 9-slice, corner bosses preserved | yes | scales w/ panel |
| Panel_BG_Dark | Panel | 1 | Image | stretch (inset) | 0.5,0.5 | inner plate, slight inset from frame | yes | — |
| Panel_GemFinial | Panel | 2 | Image | top-center (0.5,1) | 0.5,0.5 | overhangs the frame top edge | yes | center-locked |
| Title_Paused | Panel | 3 | Text | top-center region | 0.5,1 | below the gem | yes | — |
| ButtonStack | Panel | 4 | VerticalLayoutGroup | center/lower | 0.5,0.5 | 3 buttons, equal spacing, child-force-expand width | yes | — |
| Btn_Resume | ButtonStack | 0 | Button | layout element | 0.5,0.5 | full-width capsule | yes | width = stack |
| Resume_Frame | Btn_Resume | 0 | Image (sliced) | stretch-all | 0.5,0.5 | gold rim | yes | — |
| Resume_Fill | Btn_Resume | 1 | Image | stretch (inset) | 0.5,0.5 | blue gradient | yes | — |
| Resume_FlourishL/R | Btn_Resume | 2/3 | Image | left/right inside | 0/1,0.5 | small gold diamond accents | yes | — |
| Resume_Label | Btn_Resume | 4 | Text | center | 0.5,0.5 | "RESUME" | yes | — |
| Btn_Settings | ButtonStack | 1 | Button | layout element | 0.5,0.5 | full-width capsule | yes | width = stack |
| Settings_Frame/Fill/FlourishL/R/Label | Btn_Settings | 0..4 | Image/Text | as Resume | — | dark-steel fill; "SETTINGS" | yes | — |
| Btn_Surrender | ButtonStack | 2 | Button | layout element | 0.5,0.5 | full-width capsule | yes | width = stack |
| Surrender_Frame/Fill/FlourishL/R/Label | Btn_Surrender | 0..4 | Image/Text | as Resume | — | oxblood fill; "SURRENDER" | yes | — |

**Child-order rationale:** Scrim_Dim first (dims + captures outside taps); then the Panel (frame→inner plate→gem
→title→buttons). Each button is frame→fill→flourishes→label so the label reads on top of the colored fill.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim_Dim:** full-bleed (extends under cutout), alpha ≈ **0.62** of #05060a.
- **Panel:** width ≈ **0.34W** (≈796 px) × height ≈ **0.66H** (≈713 px); centered at (0.50, 0.50). The gold frame
  border thickness ≈ **0.022W**; corner bosses ≈0.05W square. The inner dark plate insets ≈0.018W from the frame.
- **Gem finial:** Ø ≈ **0.05H**, centered x=0.50, overhanging so its center sits on the frame's top edge
  (≈y=0.83 in screen space given the panel's top at ≈y=0.83).
- **Title "PAUSED":** cap-height ≈ **0.075H** (≈81 px), centered x=0.50, baseline in the panel's upper third
  (≈0.70H screen / ≈0.78 of the panel height from its bottom). Clear gap below the gem.
- **ButtonStack:** occupies the panel's lower ~62%, centered; each button width ≈ **0.265W** (≈620 px, i.e. panel
  inner width) × height ≈ **0.105H** (≈113 px); vertical spacing between buttons ≈ **0.045H** (≈49 px). The stack
  is vertically centered in the lower region with the title clearing above it. Button corner radius ≈0.02H;
  flourish diamonds ≈0.025H inset ≈0.02W from each label end. Label cap-height ≈ **0.040H** (≈43 px).
- **Vertical rhythm (top→bottom inside panel):** gem (apex) → ~0.10H gap → "PAUSED" → ~0.10H gap → RESUME →
  spacing → SETTINGS → spacing → SURRENDER → ~0.06H bottom margin.
- **Tablet (1.33:1):** panel width grows to ≈0.42W (it's a fixed-aspect modal — scale by height, keep buttons
  full panel-width). **Ultrawide (21:9):** panel stays ≈0.34W centered; the extra width just shows more dimmed
  battlefield. **Notch:** Scrim_Dim covers the full bleed (incl. under the cutout) but the Panel + all text/
  buttons sit inside SafeArea so nothing clips; the modal stays centered in the safe rect.

## F · TYPOGRAPHY (per text)
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| Title_Paused | PAUSED | Trajan serif, gold bevel | Heavy | UPPER | +8% | gold bevel + 1.5px #1f1405 stroke + drop-shadow (0,3,#000 70%) + soft bloom | ~81 | gradient **#f7e7b0→#caa04a** |
| Resume_Label | RESUME | clean serif/condensed | Bold | UPPER | +10% | 1px #08142e stroke + drop-shadow; on bright-blue fill | ~43 | #ffffff → #eaf2ff (cool white) |
| Settings_Label | SETTINGS | clean serif/condensed | Bold | UPPER | +10% | 1px #0a0c12 stroke + drop-shadow; on dark fill | ~43 | #ecd9a6 (warm gold-cream) |
| Surrender_Label | SURRENDER | clean serif/condensed | Bold | UPPER | +10% | 1px #2a0908 stroke + drop-shadow; on oxblood fill | ~43 | #ffe6df (warm white) |

"PAUSED" is the largest, gold-bevel focal of the panel. Button labels share weight/tracking; only fill color
encodes role (blue primary / dark neutral / red danger). The three labels are equal in size — color, not size,
ranks them.

## G · MATERIALS
- **Panel frame (cast gold/bronze):** base #8a6a2c, highlight #f3d680, shadow #523c14; satin metal (roughness
  ≈0.35); engraved filigree along the border; polished bevel with sharp top specular; **ornate corner bosses/
  cartouches** at each corner; soft gold rim-bloom; minor edge wear.
- **Panel inner plate:** near-black brushed obsidian #0c0e14 with a very subtle top-edge sheen and faint inner
  vignette so the gold frame pops.
- **Gem finial:** faceted **violet/blue amethyst-sapphire** (#5a2db0↔#3f6fff) in a gold claw mount, inner glow +
  bright specular spark; soft outer bloom (the panel's prestige jewel).
- **RESUME button:** royal/cobalt **blue** fill gradient #2b56c8→#4f8bff with a glossy top highlight #bcd3ff,
  gold-rim frame, small gold diamond flourishes; the brightest/most saturated button (primary affordance).
- **SETTINGS button:** **dark steel** fill #20242e→#2c313d (neutral), gold-rim frame + flourishes, faint top
  sheen — clearly secondary.
- **SURRENDER button:** **oxblood/ember red** fill #7a1f1a→#b6342a with a warm top highlight #e2735a, gold-rim
  frame + flourishes — danger.
- **Battlefield behind (dimmed):** keep/fortress stone upper-left; **blue cloth banners** (Iron Pact) left; **red
  cloth banners + open fire** (Ashen Horde) right; troops mid-field; the whole scene pushed dark/desaturated by
  Scrim_Dim with a strong vignette and a faint warm fire glow bleeding from the right edge.

## H · COMPONENTS (each interactive)
1. **Btn_Resume (PRIMARY)** — *purpose:* close the modal and **unfreeze** (`Time.timeScale = 1`, the saved
   value). *States:* **idle** = bright blue, gold rim; **hover/focus** = fill +10% brightness + rim glow;
   **pressed** = scale 0.96 + inner flash + label dip; **disabled** (n/a) = desat; **selected/default** = it is
   the **default focus** (controller/Enter triggers it) with a subtle persistent glow. *Structure:* frame + blue
   fill + flourishes + label. *Feedback:* click → close animation + resume SFX + restore timeScale. *(Write:
   Time.timeScale.)*
2. **Btn_Settings** — *purpose:* open in-match Settings (the game stays paused beneath). *States:* idle dark
   steel; hover fill lighten + rim glow; pressed scale 0.96 + flash; disabled desat; no default. *Structure:*
   frame + dark fill + flourishes + label. *Feedback:* click → push Settings overlay; on its close, return to
   this Pause modal (still paused). *(No sim write; navigates UI.)*
3. **Btn_Surrender (DANGER)** — *purpose:* concede the match. *States:* idle oxblood; hover fill +10% + warm rim
   glow; pressed scale 0.96 + flash; disabled desat; no default (deliberately not the default, to avoid
   accidental concede). *Structure:* frame + red fill + flourishes + label. *Feedback:* click → **confirm
   prompt** ("Surrender?") via the shared Confirm modal (recommended), then route to the defeat/result flow.
   *(Outcome is a match result; the surrender decision is sent through the permitted control path; no client-side
   balance mutation.)*
4. **Scrim_Dim** — *purpose:* modal backdrop. *States:* fades in/out with the modal; **raycast ON** so taps
   outside the panel are **absorbed** (do NOT pass to the battlefield while paused). *Optional:* a tap on the
   scrim may be treated as **Resume** (common pattern) — but since the mockup gives an explicit Resume button,
   default is **no dismiss-on-scrim** to prevent accidental unpause; if enabled, it mirrors Resume exactly.

## I · ANIMATION TIMELINE
**OnShow (open):**
| t (s) | Element | Action | Dur | Easing | Emphasis |
|---|---|---|---|---|---|
| 0.00 | (sim) | `Time.timeScale = 0` (freeze) | inst | — | battle stops |
| 0.00 | Scrim_Dim | α 0→0.62 | 0.18 | linear | field dims |
| 0.04 | Panel | scale 0.90→1.0 + α 0→1 (slight drop from y+0.02) | 0.22 | back-out | panel seats |
| 0.16 | Panel_GemFinial | spark/scale 0.6→1 + glint | 0.18 | back-out | jewel catches light |
| 0.18 | Title_Paused | α 0→1 + scale 1.06→1 | 0.20 | ease-out | "PAUSED" reads |
| 0.22 | Btn_Resume | slide up from y-0.02 + α, then default-focus glow | 0.18 | ease-out | primary first |
| 0.27 | Btn_Settings | slide up + α | 0.18 | ease-out | — |
| 0.32 | Btn_Surrender | slide up + α | 0.18 | ease-out | danger last |
| hold | Resume glow, gem | primary breathe (±5%, ~1.8s) + gem sparkle | cont. | sine | gentle life |

**OnHide (Resume / close):** buttons + title quick α→0 (0.10s) → Panel scale 1→0.94 + α→0 (0.18s, ease-in) →
Scrim_Dim α→0 (0.18s) → **restore** `Time.timeScale` to its pre-pause value → HUD live again. **Surrender:**
plays a brief panel pulse, then transitions out toward the result flow (after confirm).

## J · PARTICLE & FX (passive — describe only)
- **Gem finial:** slow rotating inner glint + soft violet/blue bloom + occasional sparkle.
- **Gold frame:** gentle rim-bloom breathe; faint specular travel along the top bevel.
- **RESUME:** soft primary under-glow pulse to draw the eye to the default action.
- **Battlefield behind:** the only motion is the dimmed scene's residual fire flicker at the right edge (very
  subdued under the scrim) — everything else is frozen (paused).
- No particles overlap the button labels enough to hurt legibility.

## K · EVENT BEHAVIOR
- **OnShow:** triggered by the HUD Pause (❚❚); immediately `Time.timeScale = 0`; Scrim_Dim + Panel animate in
  (I); Resume takes default focus. Player: the war freezes; a calm, regal pause panel demands a choice.
- **Resume:** restore timeScale → close → back to the live HUD exactly where it was.
- **Settings:** open Settings over the still-paused game; returning re-shows this modal (game stays frozen until
  Resume).
- **Surrender:** confirm → concede → route to defeat/result. (Until confirmed, nothing is committed.)
- **Back / hardware-back:** treated as **Resume** (safe, non-destructive) — never as Surrender.
- **OnHide:** Panel + scrim animate out; timeScale restored (for Resume) or handed to the result flow (for
  Surrender). No leftover scrim; HUD interactivity restored.

## L · NEGATIVE RULES (must-never)
- **Never** leave `Time.timeScale = 0` after closing via Resume — always restore the pre-pause value.
- **Never** make **Surrender** the default focus or trigger it on hardware-back (must be deliberate; confirm-
  gated).
- **Never** let outside/scrim taps reach the (paused) battlefield — Scrim_Dim raycast is ON and absorbs them.
- **Never** rank the buttons by size — they are equal height; **color** encodes role (blue primary / dark neutral
  / red danger). Don't recolor: Resume stays blue, Surrender stays oxblood.
- **Never** clip the Panel/title/buttons under a notch — Panel lives in SafeArea (scrim may bleed under).
- **Never** write anything beyond `Time.timeScale` from this modal (the surrender outcome goes through the
  permitted control path; no balance/ECS mutation).
- **Never** drop the gem finial or corner bosses — they are signature prestige elements of the panel.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `PauseModalDesign.png`: a centered ornate gold-framed dark panel with a gem finial at the
  top-center, serif "**PAUSED**" title, and three full-width stacked buttons — **RESUME** (blue, top, primary),
  **SETTINGS** (dark, middle), **SURRENDER** (oxblood-red, bottom) — over a dimmed battlefield (keep UL, blue
  banners L, red banners + fire R) — all per §E (±2%).
- **Modal behavior:** opening sets timeScale 0; Scrim_Dim absorbs outside taps; Resume restores timeScale;
  Surrender is confirm-gated and never the default/back action.
- **Hierarchy:** §C tree; scrim → panel(frame→plate→gem→title→stack); each button frame→fill→flourish→label.
- **Typography:** "PAUSED" largest/gold-bevel; three equal-size labels color-coded by role; exact strings.
- **Safe area:** panel + content inside safeArea on a notched device; scrim full-bleed under the cutout.
- **Animation:** panel back-out seat, gem spark, title pop, staggered buttons (Resume→Settings→Surrender),
  default-focus on Resume; clean close that restores the live HUD.

## N · IMPLEMENTATION CONFIDENCE
**93 / 100.** This is a clean, self-contained modal with unambiguous copy, color-coded roles, and a fully
specified layout/animation; it maps directly onto the §12-permitted `Time.timeScale` write and the existing
Pause trigger. The −7 is purely art-dependent: the ornate frame with corner bosses, the gem finial, the button
bevels/flourishes, and the painted dimmed-battlefield backdrop must be authored to hit the prestige look — the
uGUI structure, states, and behavior are deterministic.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O present, in order, substantive.
- □ Strings captured: "PAUSED", "RESUME", "SETTINGS", "SURRENDER".
- □ Button roles/colors locked (blue primary / dark neutral / oxblood danger), equal size.
- □ Opening sets Time.timeScale 0; Resume restores it; Surrender confirm-gated & not default/back.
- □ Scrim raycast ON (absorbs outside taps; battlefield stays frozen/inert).
- □ Gem finial + corner bosses preserved; "PAUSED" is the focal title.
- □ Panel + content inside SafeArea; scrim full-bleed under cutout.
- □ Only Time.timeScale written (§12 respected; no balance/ECS mutation).
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Hex + materials for frame/plate/gem/buttons/backdrop.
- □ Header + Source line in required format.



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 13 · Defeat

Source: design/DefeatScreenDesign.png · 1915×821 (≈2.33:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas; all geometry is **fractions of 2340×1080**. This is a **POST-MATCH RESULT** screen shown when the standard match resolves against the player (the player's own statue/objective has fallen). Verdict comes from ECS `MatchState.Outcome == Defeat` (read-only). **No code here** — construction spec only.

---

## A · SCREEN PURPOSE
The **Defeat** result screen is the somber counterpart to Victory. It must (1) communicate the loss clearly but without humiliating the player (dignified, cold, "the war goes on" tone — not punishing), (2) give the player two clear recovery paths — **RETRY** (re-attempt the same match immediately) and **CONTINUE** (return to the hub) — and (3) keep dwell short. Crucially there is **no reward chest, no gem/time stat row** here (you lost — nothing to celebrate or grant); the focus is purely verdict + two routing actions. Source of truth = ECS `MatchState.Outcome == Defeat`.

State machine position: `Battle (HUD) → [own statue falls] → Defeat → {RETRY → Match Intro/Battle | CONTINUE → Main Menu}`. Two forward actions, no celebratory reveal.

---

## B · VISUAL DNA (screen-specific)
- **Verdict colour = SOMBER, COLD, DESATURATED.** This is the "you lost" palette: a grey-steel field, cold blue-grey haze, muted gold frame (less bloom than Victory), and a single deep **oxblood-red** accent reserved for the **RETRY** button (the emotional "try again" / Ashen-coded action). No warm god-rays, no gold sparkle storm — the lighting is overcast and low-energy.
- **Hero ornament:** a smaller, more austere **gold crown** crest centered on the top frame edge (a fallen/diminished crown vs Victory's crown+swords trophy). Optionally tarnished/dimmer gold.
- **Right-side mood prop:** a **defeated / kneeling armored warrior** (your fallen champion) is rendered into the right portion of the background, slumped, in cold steel armor — a storytelling silhouette that sells the loss.
- **Background:** full-bleed bleak battlefield at dusk/overcast — broken banners, a tattered flag drooping left, smoke, ruined ground, cold grey-blue tones (`#3a3f48` haze). Heavily vignetted and desaturated; pushed back behind the panel.
- **Panel:** same architectural language as Victory (obsidian interior + ornate gold beveled frame, filigree corners) but **cooler and dimmer** — the inner glow is faint/cold rather than warm-gold; the frame catches less rim-light.
- **Mood:** dignified defeat, cold, quiet, resolute. Contrast is lower than Victory (no luminous focal prize); the two buttons are the brightest objects, with RETRY (red) slightly hotter than CONTINUE (blue).

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
DefeatScreen (UiScreen root; CanvasGroup; full-rect)
└── FullBleedRoot (RectTransform, stretch-stretch, ignores safe area)
    ├── BG_Battlefield (Image — bleak overcast battlefield, full-bleed)
    ├── BG_FallenWarrior (Image — kneeling defeated champion, right side; may be baked into BG)
    ├── BG_Desaturate (Image/Material — cold desaturation + blue-grey grade overlay)
    ├── BG_Vignette (Image — radial dark vignette, multiply)
    └── BG_DarkenScrim (Image — black ~0.40, focuses panel)
    SafeAreaRoot (RectTransform — Screen.safeArea inset)
    └── ResultPanel (Container; anchored center; ~0.46w × ~0.86h)
        ├── Panel_Frame (Image — ornate gold beveled 9-slice frame, dimmer)
        │   └── Panel_Interior (Image — near-black obsidian, faint cold inner glow)
        ├── Crest_Group (anchored top-center, overlaps frame top edge)
        │   ├── Crest_BannerCloth_L (Image — muted banner fan)
        │   ├── Crest_BannerCloth_R (Image — muted banner fan)
        │   └── Crest_Crown (Image — austere gold crown, top-most)
        ├── Title_Defeat (Text "DEFEAT" — gold-bevel serif, desaturated/cooler)
        │   └── Title_Glow (Image — faint gold/grey bloom, low intensity)
        ├── Subtitle_Ribbon (Image — dark engraved ribbon strip)
        │   └── Subtitle_Text (Text "Your statue has fallen.")
        └── Buttons_Row (Horizontal container — two equal CTAs side by side)
            ├── CTA_Retry (Button — oxblood-red fill, gold frame)
            │   ├── Retry_Frame (Image — gold beveled frame)
            │   ├── Retry_Fill (Image — oxblood-red gradient + inner red rim)
            │   └── Retry_Label (Text "RETRY")
            └── CTA_Continue (Button — royal-blue fill, gold frame)
                ├── Continue_Frame (Image — gold beveled frame)
                ├── Continue_Fill (Image — royal-blue gradient + inner blue rim)
                └── Continue_Label (Text "CONTINUE")
```

---

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| DefeatScreen | Canvas | — | UiScreen+CanvasGroup | stretch/stretch | .5,.5 | — | root | fade in/out |
| FullBleedRoot | DefeatScreen | 0 | RectTransform | stretch/stretch | .5,.5 | — | **ignores** | full-bleed under notch |
| BG_Battlefield | FullBleedRoot | 0 | Image | stretch/stretch | .5,.5 | center-crop | no | AspectFill, focal center |
| BG_FallenWarrior | FullBleedRoot | 1 | Image | right/center | 1,.5 | right | no | anchored to right edge; clips off-screen on narrow |
| BG_Desaturate | FullBleedRoot | 2 | Image/Mat | stretch/stretch | .5,.5 | — | no | full-screen grade |
| BG_Vignette | FullBleedRoot | 3 | Image | stretch/stretch | .5,.5 | center | no | radial |
| BG_DarkenScrim | FullBleedRoot | 4 | Image | stretch/stretch | .5,.5 | — | no | flat black α0.40 |
| SafeAreaRoot | DefeatScreen | 1 | RectTransform+SafeAreaFitter | stretch/stretch | .5,.5 | — | **applies** | insets on notch sides |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center/center | .5,.5 | center | yes | width = min(0.46·W, height-cap); see §E |
| Panel_Frame | ResultPanel | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | 9-slice |
| Panel_Interior | Panel_Frame | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | inset by frame thickness |
| Crest_Group | ResultPanel | 1 | RectTransform | top/center | .5,.5 | center | yes | mid sits on frame top edge |
| Crest_BannerCloth_L/R | Crest_Group | 0,1 | Image | center/center | .5,.5 | center | yes | mirror scale.x=±1 |
| Crest_Crown | Crest_Group | 2 | Image | top/center | .5,1 | center | yes | top z |
| Title_Defeat | ResultPanel | 2 | Text (TMP) | top/center | .5,1 | center | yes | auto-size capped |
| Title_Glow | Title_Defeat | 0 | Image | center/center | .5,.5 | center | yes | behind glyphs, faint |
| Subtitle_Ribbon | ResultPanel | 3 | Image (9-slice) | top/center | .5,1 | center | yes | width ≈ 0.78 panel interior |
| Subtitle_Text | Subtitle_Ribbon | 0 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | one line auto-fit |
| Buttons_Row | ResultPanel | 4 | HorizontalLayoutGroup | bottom/center | .5,0 | middle-center | yes | two equal cells + center gap; pinned above bottom frame |
| CTA_Retry | Buttons_Row | 0 | Button | — | .5,.5 | center | yes | ≈0.46 of row width |
| Retry_Frame | CTA_Retry | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | gold frame |
| Retry_Fill | CTA_Retry | 1 | Image | stretch/stretch | .5,.5 | — | yes | inset; red gradient |
| Retry_Label | CTA_Retry | 2 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | auto-size capped |
| CTA_Continue | Buttons_Row | 1 | Button | — | .5,.5 | center | yes | ≈0.46 of row width |
| Continue_Frame | CTA_Continue | 0 | Image (9-slice) | stretch/stretch | .5,.5 | — | yes | gold frame |
| Continue_Fill | CTA_Continue | 1 | Image | stretch/stretch | .5,.5 | — | yes | inset; blue gradient |
| Continue_Label | CTA_Continue | 2 | Text (TMP) | stretch/stretch | .5,.5 | center | yes | auto-size capped |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
W=2340, H=1080, match HEIGHT, fractions from top-left.

**Background (full-bleed):** BG_Battlefield 0,0→1,1 AspectFill center. BG_FallenWarrior anchored to the **right** edge (1,.5), occupying roughly the right 0.26·W × 0.70·H of the field; on narrow/notched devices its outer edge clips off-screen (acceptable — it is mood, not content). BG_Desaturate + BG_Vignette + BG_DarkenScrim (α≈0.40) full-rect.

**ResultPanel:** width = 0.46·W ≈ 1076 px, centered (x=0.50·W). Height ≈ **0.86·H** ≈ 929 px (shorter than Victory's 0.92 — fewer internal rows, so the card is less tall), vertically centered. Cap: width = `min(0.46·W, 1.30·panelHeight)` (the Defeat card is a touch wider-to-tall than Victory because its content is title + subtitle + a wide two-button row).

**Internal vertical rhythm (fractions of PANEL height, 0=top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Crest (crown apex above frame) | ~ -0.03 to 0.07 | 0.16 |
| Title "DEFEAT" | 0.30 | 0.18 |
| Subtitle ribbon | 0.48 | 0.07 |
| Buttons row | 0.80 | 0.16 |

(Note: with no stat row and no reward, the content sits higher/sparser; the title is large and dominant, then the two buttons anchor the lower third — a deliberately empty middle that reads as somber.)

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.82, centered.
- Subtitle ribbon width ≈ 0.78, centered.
- Buttons row width ≈ 0.86, centered; each button ≈ 0.42 of panel width, center gap ≈ 0.04; button height ≈ 0.13·panelH. RETRY is the **left** cell, CONTINUE the **right** cell (matches source).

**Notch/safe-area:** identical rule to Victory — panel centered in the *safe* rect; background art bleeds under the cutout. **Tablet/ultrawide:** same cap behavior; ultrawide reveals more bleak battlefield and pushes the fallen-warrior prop further right.

---

## F · TYPOGRAPHY (per text)
Sizes px@1080. Same families as Victory but **cooler/dimmer** treatment on the title (less bloom, slightly desaturated gold).

| Text | Content | Family / personality | Weight | Caps | Tracking | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|
| Title_Defeat | `DEFEAT` | Trajan-style serif, heavy, solemn | Black/ExtraBold | ALL-CAPS | +6% | desaturated gold gradient, **low** bloom, faceted bevel | ~120–130 (slightly larger than Victory — it is the lone focal) | gradient `#e8cf86` (top) → `#a98a45` (bottom), cooler/greyer than Victory's hot gold; cores `#f3ead2` | dark stroke `#2a2008` ~3px + shadow `#000` α0.65 (0,4) |
| Subtitle_Text | `Your statue has fallen.` | clean serif, quiet | Medium | Title-case (as shown) | +1% | subtle, no glow | ~30–34 | cool parchment `#d8d0bc` (slightly grey) | dark stroke `#241a06` ~1.5px, shadow α0.5 (0,2) |
| Retry_Label | `RETRY` | serif display, urgent | Bold | ALL-CAPS | +8% | bright clean, faint red-tinged glow | ~42–46 | warm white `#fff3ec` | dark red stroke `#3a0a06` ~2px + shadow α0.6 (0,3) |
| Continue_Label | `CONTINUE` | serif display, authoritative | Bold | ALL-CAPS | +8% | bright clean, faint blue glow | ~42–46 | warm white `#fffbf0` | dark blue stroke `#0a1f44` ~2px + shadow α0.6 (0,3) |

Both button labels share the same size so neither visually outranks the other (RETRY and CONTINUE are co-equal in weight; the *color* differentiates them).

---

## G · MATERIALS
- **Gold frame / crown / title:** same brushed antique-gold family as Victory but **tarnished/cooler**: highlights `#d8bf78`, mid `#a98a45`, shadow `#5a4720`–`#2a2008`. Lower bloom; rim-light is dimmer (overcast scene). Crown reads slightly tarnished.
- **Panel interior:** obsidian `#0a0b0f`–`#14161e`, matte; inner glow is **cold/faint** (a low blue-grey radial `#2a3340` α≈0.12) rather than warm — reinforces the somber read.
- **Subtitle ribbon:** dark engraved bronze/stone `#2a2210`–`#3a3018`, recessed carved channel.
- **RETRY fill (oxblood-red):** vertical gradient deep `#5a1410` (bottom) → `#b84130` (top), with an inner red rim-glow `#e0604a` α≈0.4; satin, slightly matte (less glossy than Victory's blue to keep the mood restrained). Edges catch dim gold from the frame.
- **CONTINUE fill (royal-blue):** vertical gradient `#0d2a66` → `#2f6fd6`, inner cobalt rim-glow `#5aa0ff` α≈0.4 — same construction as Victory's CTA but at slightly lower glow intensity to match the muted scene.
- **Background grade:** desaturate toward grey-blue; haze `#3a3f48`; banners/cloth muted; the fallen warrior in cold steel `#4a525c` with weak rim-light.
- **Bloom budget:** minimal. Only the two button top-edges and the title carry slight bloom; everything else is matte/overcast. No focal-prize bloom (there is no prize).

---

## H · COMPONENTS (states + feedback)
**CTA_Retry (oxblood-red Button):**
- **Idle:** red gradient fill, gold frame, faint inner red rim; full opacity.
- **Hover:** frame +8%, inner red glow +25%, scale 1.0→1.03 (120 ms ease-out).
- **Pressed:** scale →0.97 (60 ms), fill darkens ~10%, brief red flash, "blade-draw" SFX.
- **Disabled:** only if retry is unavailable for this mode (rare); then fill desaturates to grey `#4a4038`, label `#8a8278`, non-interactive.
- **Confirm:** quick red ring flash → exit to Match Intro/Battle.

**CTA_Continue (royal-blue Button):**
- **Idle/Hover/Pressed:** identical interaction model to Retry but blue; "anvil" click SFX.
- **Disabled:** N/A (always available).
- **Confirm:** blue ring flash → exit to Main Menu.

**Co-equal emphasis:** neither button "breathes"/pulses by default (this is a calm decision screen, not a celebratory funnel). Optionally a very subtle slow glow on RETRY only (it is the encouraged "try again" path), but keep it understated.

**Focus order / input:** default selected control = **CONTINUE** (the safe/neutral path) on gamepad — but RETRY is one step left. Back/B → CONTINUE (return to hub; non-destructive). Tab/D-pad cycles between the two. No tap-anywhere shortcut (must choose deliberately).

---

## I · ANIMATION TIMELINE
The Defeat reveal is **slower, heavier, and quieter** than Victory — no celebratory burst, no count-up.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.45 | FullBleedRoot CanvasGroup | fade 0→1 (bleak field + scrim in) — slower than Victory | linear |
| 0.10 | 0.50 | BG_Desaturate | grade ramps from neutral→full cold desaturation | ease-in |
| 0.20 | 0.45 | ResultPanel | scale 0.92→1.0 + α 0→1 (settles, **no overshoot** — a heavy, dignified entrance) | ease-out |
| 0.35 | 0.40 | Crest_Group | crown lowers/settles onto frame (slow, slight downward "weight") | ease-out |
| 0.45 | 0.55 | Title_Defeat | α 0→1 + scale 1.06→1.0; faint bloom rises then settles low; optional slow desaturate-in of the gold | ease-out |
| 0.55 | 0.25 | FX_TitleDust | one-shot faint grey/ash dust drift across the title (somber, not sparkle) | linear |
| 0.85 | 0.35 | Subtitle_Ribbon + text | α 0→1 + slide up 8px | ease-out |
| 1.15 | 0.35 | CTA_Continue | α 0→1 + scale 0.94→1.0 | ease-out |
| 1.20 | 0.35 | CTA_Retry | α 0→1 + scale 0.94→1.0 (appears just after / together) | ease-out |
| 1.55 | (idle) | both CTAs | settle to idle; RETRY may carry a very subtle slow red glow (period ~2.4 s) | sine in-out (optional) |

**OnRetry (exit):** 60 ms press dip → 120 ms red ring flash → panel α 1→0 + scale →0.96 (180 ms) while bg fades → route to Match Intro (or directly re-enter Battle for the same mode/seed). 
**OnContinue (exit):** same but blue flash → route to Main Menu. 
**Skip rule:** a tap during reveal snaps all tweens to end-state and enables both buttons (no celebratory content to protect, so skipping is fully allowed).

---

## J · PARTICLE & FX (passive)
- **Ash/smoke drift (background):** slow upward grey smoke + a few falling ash flecks across the bleak field; low opacity; reinforces overcast loss tone. Stays behind the scrim.
- **Title dust (one-shot at reveal):** faint ashen motes (NOT gold sparkles) — desaturated grey/`#9a948a`.
- **Faint cold inner panel glow:** very low, static.
- **Optional RETRY glow:** subtle slow red breathing (understated).
- **No god-rays, no gold sparkle storm, no halo, no prize bloom** — the absence is intentional and part of the somber DNA.

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload may be `{ mode, canRetry:bool }` (no reward/time values needed). Verify ECS `MatchState.Outcome == Defeat`. Run the slow §I reveal. Battle input disabled.
- **OnRetry:** the encouraged recovery CTA. If `canRetry`, fires exit → re-enter the same match (Match Intro or direct Battle re-init for the same mode/level). Debounced. Issues only navigation + a fresh match start request — **no balance mutation** (retry is free per design unless an energy/ticket system exists, which canon CUTs — see 00 §5; if a future ticket gate is added it is server-authoritative, not decided here).
- **OnContinue:** returns to Main Menu (`UiRouter` pop/replace). Debounced.
- **OnBack (B/Esc):** aliased to CONTINUE (non-destructive return to hub).
- **No mutation / read-only:** the screen issues no ECS commands beyond requesting a new match on Retry; it never edits balance/state (§12).
- **Idempotency:** re-entry settles to end-state immediately.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** show any reward (no chest, no gems, no XP, no count-up) — defeat grants nothing here. 
2. Do **not** use warm/triumphant gold bloom, god-rays, or sparkle FX — the scene is somber/cold/desaturated. 
3. Do **not** make RETRY and CONTINUE different sizes — they are co-equal; only color differentiates (RETRY red, CONTINUE blue). 
4. Do **not** add a stat row, time, or reward slots (those belong to other variants). 
5. Do **not** put interactive content under the notch; only background art bleeds there. 
6. Do **not** mutate currency/state client-side (§12); Retry only requests a new match. 
7. Do **not** auto-dismiss or auto-retry — the player must choose. 
8. Do **not** stretch frame filigree corners (9-slice borders fixed). 
9. Do **not** humiliate (no taunting copy, no enemy gloating) — copy is exactly `DEFEAT` / `Your statue has fallen.`. 
10. Do **not** hard-code the loss — gated by ECS `MatchState.Outcome == Defeat`. 
11. Do **not** use portrait or assume width is stable; match HEIGHT, fraction-based.

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] Cold, desaturated full-bleed bleak battlefield with a **kneeling/defeated armored warrior** on the right, vignetted + scrimmed.
- [ ] Centered obsidian card with a **dimmer/tarnished** ornate gold frame, width ≈ 0.46·W, height ≈ 0.86·H.
- [ ] An austere **gold crown** crest (no swords) breaks the top frame edge, with muted banner cloth behind.
- [ ] `DEFEAT` renders as a heavy, solemn, slightly **desaturated** gold-bevel serif, ALL-CAPS, ~120–130 px@1080, low bloom, dark stroke + shadow.
- [ ] Subtitle reads exactly `Your statue has fallen.` in a recessed dark ribbon, cool-parchment text.
- [ ] Exactly **two co-equal buttons**: **RETRY** (left, oxblood-red `#5a1410`→`#b84130`, gold frame) and **CONTINUE** (right, royal-blue `#0d2a66`→`#2f6fd6`, gold frame), same size, white serif labels.
- [ ] The empty middle band (no stat/reward content) is preserved — sparse, somber composition.
- [ ] Reveal is slow/heavy/quiet (no overshoot, no count-up, no gold sparkles); ash/smoke drifts in the background; tap-to-skip works.
- [ ] Both buttons show idle/hover/pressed feedback; RETRY routes to re-match, CONTINUE routes to hub.
- [ ] No reward, no warm FX, no client balance mutation.
- [ ] Side-by-side with `DefeatScreenDesign.png`, positions within ±2% of panel dims, colors within stated ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**94 / 100.** The simplest of the five result screens (title + subtitle + two buttons, no reward economy), with unambiguous copy and a clear two-button layout, so structural + color fidelity is very high. Deductions: (a) the bespoke fallen-warrior background art and the tarnished crown crest must be authored/sourced to match (~ -4); (b) the exact desaturation/grade strength and ash-drift tuning are interpretive within the stated somber budget (~ -2). No structural ambiguity remains.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/DefeatScreenDesign.png`, 1915×821) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header has number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed stated.
- [x] Verbatim labels recorded (`DEFEAT`, `Your statue has fallen.`, `RETRY`, `CONTINUE`) — nothing invented.
- [x] Forensic hex ranges from the art.
- [x] Full ASCII tree + per-node Unity table.
- [x] Somber reveal timeline (no count-up/sparkle) with tap-to-skip.
- [x] §12 boundary honored (no balance mutation; Retry only requests a new match).
- [x] Negative rules + ≥95% acceptance + confidence rationale.
- [x] No code/asset/scene/prefab/gameplay changes, no commit.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 14 · Campaign Result

Source: design/CampaignResultDesign.png · 1672×941 (≈1.78:1) · Analysis-only forensic spec.

> Normalized to the **2340×1080** landscape production canvas; geometry is **fractions of 2340×1080**. This is a **POST-MATCH RESULT** screen shown when a **campaign level** is cleared. The clear verdict, the star count, the clear-time, and the reward values originate from ECS `MatchState`/level evaluation (read-only) + server-authoritative reward grant (display-only). **No code here** — construction spec only.

---

## A · SCREEN PURPOSE
The **Campaign Result** ("Level Cleared") screen rewards completing a campaign level and drives progression to the **next** level. It must (1) celebrate the clear with a **3-star rating** (stars earned vs available — the core campaign mastery loop), (2) show the **clear time** as a performance stat (green = good/under target), (3) present the level's **rewards** as two distinct currency slots (coins + gems), and (4) offer two routing actions — **NEXT LEVEL** (primary progression) and **REPLAY** (re-attempt for more stars / better time). Tone is warmer/lighter than the standard Victory — it uses a **parchment quest-scroll** aesthetic (campaign = story/journey), with blue Iron-Pact heraldic banners framing the scroll. Source of truth: ECS level-clear result (stars, time) + server reward grant.

State machine position: `Campaign Battle → Campaign Result → {NEXT LEVEL → next level intro/battle | REPLAY → same level}` (and an implicit back-to-Campaign-Map via REPLAY's sibling or system back).

---

## B · VISUAL DNA (screen-specific)
- **Aesthetic = WARM PARCHMENT QUEST-SCROLL on a dark map.** Unlike the obsidian Victory/Defeat cards, the campaign result panel is a **light parchment/cream scroll** (`#c8a96a`–`#e2c98a`) with a torn/aged paper texture, bordered by an ornate gold frame and flanked by **blue heraldic war-banners** (Iron Pact, with a gold lion/heraldic crest) draped over the top-left and top-right corners.
- **Hero element = the 3-STAR ARC.** Three large gold five-point stars sit on a curved gold banner ribbon spanning the top of the panel (breaking the top frame). The **center star is largest** and sits highest; the two side stars are slightly smaller/lower (classic 3-star arc). Earned stars are **filled hot gold** with bloom; unearned stars are **dark/hollow outlines** (`#23180f` with a gold rim). Source shows **2 of 3 filled** (left + center gold, right empty).
- **Background:** a dark, dimmed **campaign world map / battlefield** (the level just played), heavily vignetted to near-black at the edges (`#010101`), with faint troop silhouettes — pushed far back so the bright parchment scroll pops.
- **Performance stat = GREEN clear-time.** "Clear Time" with a gold clock/stopwatch icon and the time value in **vivid lime-green** (`#93bf37`) — green signals "good / under par." 
- **Rewards = two ornate slot cards.** Side-by-side dark slot tiles (gold-framed) on the lower parchment: left = **silver coins** stack icon + amount; right = **purple gem** cluster icon + amount.
- **CTAs:** **NEXT LEVEL** (royal-blue, primary progression) and **REPLAY** (dark stone/iron, secondary), separated by a small **gold sword + shield heraldic emblem** divider.
- **Mood:** triumphant but warm, adventurous, "chapter complete." Gold + parchment + Iron-Pact blue; green accent for the time; purple for gems. Brightest interactive object = NEXT LEVEL.

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
CampaignResultScreen (UiScreen root; CanvasGroup)
└── FullBleedRoot (stretch-stretch, ignores safe area)
    ├── BG_CampaignMap (Image — dimmed world-map/battlefield, full-bleed)
    ├── BG_Vignette (Image — heavy radial vignette → near-black edges)
    └── BG_DarkenScrim (Image — black ~0.45)
    SafeAreaRoot (RectTransform — Screen.safeArea inset)
    └── ResultPanel (Container; anchored center; ~0.58w × ~0.84h)
        ├── Banner_Left (Image — blue Iron-Pact war-banner w/ gold crest, draped top-left)
        ├── Banner_Right (Image — blue Iron-Pact war-banner, draped top-right)
        ├── Panel_Frame (Image — ornate gold beveled 9-slice frame)
        │   └── Panel_Parchment (Image — aged cream parchment fill, 9-slice/texture)
        ├── Stars_Group (anchored top-center, on a gold ribbon arc, breaks top frame)
        │   ├── Star_Ribbon (Image — curved gold banner the stars sit on)
        │   ├── Star_L (Image — star, filled gold OR hollow)
        │   ├── Star_R (Image — star, filled gold OR hollow)
        │   └── Star_C (Image — center star, largest, highest, filled gold OR hollow)
        ├── Title_Cleared (Text "LEVEL 7 CLEARED" — dark serif on parchment)
        ├── ClearTime_Group
        │   ├── ClearTime_Label (Text "Clear Time")
        │   ├── ClearTime_Divider (Image — thin gold filigree rule, optional)
        │   ├── Icon_Clock (Image — gold stopwatch)
        │   └── ClearTime_Value (Text "02:45" — GREEN)
        ├── Rewards_Group
        │   ├── Rewards_Label (Text "Rewards")
        │   ├── Reward_Slot_Coins (Container — gold-framed dark tile)
        │   │   ├── Slot_Frame (Image)
        │   │   ├── Icon_Coins (Image — silver coin stack)
        │   │   └── Coins_Value (Text "12,450")
        │   └── Reward_Slot_Gems (Container — gold-framed dark tile)
        │       ├── Slot_Frame (Image)
        │       ├── Icon_Gems (Image — purple gem cluster)
        │       └── Gems_Value (Text "60")
        └── Buttons_Group (anchored bottom)
            ├── CTA_NextLevel (Button — royal-blue, gold frame)
            │   ├── Next_Frame (Image)
            │   ├── Next_Fill (Image — blue gradient + inner blue rim)
            │   └── Next_Label (Text "NEXT LEVEL")
            ├── Buttons_Divider (Image — gold sword+shield heraldic emblem)
            └── CTA_Replay (Button — dark stone/iron, gold frame)
                ├── Replay_Frame (Image)
                ├── Replay_Fill (Image — dark stone gradient)
                └── Replay_Label (Text "REPLAY")
```

---

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Order | Type | AP | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| CampaignResultScreen | Canvas | — | UiScreen+CanvasGroup | stretch | .5,.5 | — | root | fade |
| FullBleedRoot | screen | 0 | RectTransform | stretch | .5,.5 | — | ignores | full-bleed |
| BG_CampaignMap | FullBleedRoot | 0 | Image | stretch | .5,.5 | center-crop | no | AspectFill |
| BG_Vignette | FullBleedRoot | 1 | Image | stretch | .5,.5 | center | no | radial, heavy |
| BG_DarkenScrim | FullBleedRoot | 2 | Image | stretch | .5,.5 | — | no | α0.45 |
| SafeAreaRoot | screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | applies | insets |
| ResultPanel | SafeAreaRoot | 0 | RectTransform | center | .5,.5 | center | yes | width=min(0.58·W, height-cap) |
| Banner_Left | ResultPanel | 0 | Image | top-left | 0,1 | — | yes | anchored to panel top-left corner, hangs down |
| Banner_Right | ResultPanel | 1 | Image | top-right | 1,1 | — | yes | mirror; top-right corner |
| Panel_Frame | ResultPanel | 2 | Image (9-slice) | stretch | .5,.5 | — | yes | 9-slice |
| Panel_Parchment | Panel_Frame | 0 | Image | stretch | .5,.5 | — | yes | inset; aged texture (tiled or large) |
| Stars_Group | ResultPanel | 3 | RectTransform | top/center | .5,.5 | center | yes | sits on top frame edge, breaks upward |
| Star_Ribbon | Stars_Group | 0 | Image | center/center | .5,.5 | center | yes | curved gold banner behind stars |
| Star_L / Star_R | Stars_Group | 1,2 | Image | center/center | .5,.5 | center | yes | side stars, smaller, lower |
| Star_C | Stars_Group | 3 | Image | center/center | .5,.5 | center | yes | center, largest, highest, top z |
| Title_Cleared | ResultPanel | 4 | Text (TMP) | top/center | .5,1 | center | yes | auto-size capped |
| ClearTime_Label | ResultPanel | 5 | Text (TMP) | top/center | .5,1 | center | yes | small caps |
| Icon_Clock | ResultPanel | 6 | Image | center/center | .5,.5 | left-of-value | yes | square |
| ClearTime_Value | ResultPanel | 7 | Text (TMP) | center/center | .5,.5 | center | yes | tabular, green |
| Rewards_Label | ResultPanel | 8 | Text (TMP) | center/center | .5,1 | center | yes | small caps |
| Rewards_Group | ResultPanel | 9 | HorizontalLayoutGroup | center/center | .5,.5 | middle-center | yes | two equal slot tiles + gap |
| Reward_Slot_Coins / _Gems | Rewards_Group | 0,1 | RectTransform | — | .5,.5 | center | yes | each ≈0.30 of panel width |
| Slot_Frame | slot | 0 | Image (9-slice) | stretch | .5,.5 | — | yes | gold frame |
| Icon_Coins / Icon_Gems | slot | 1 | Image | top/center | .5,1 | center | yes | icon in upper area of tile |
| Coins_Value / Gems_Value | slot | 2 | Text (TMP) | bottom/center | .5,0 | center | yes | small currency icon + number row at tile bottom |
| Buttons_Group | ResultPanel | 10 | HorizontalLayoutGroup | bottom/center | .5,0 | middle-center | yes | NextLevel + emblem + Replay; pinned above bottom frame |
| CTA_NextLevel | Buttons_Group | 0 | Button | — | .5,.5 | center | yes | ≈0.40 panel width |
| Next_Frame/Fill/Label | CTA_NextLevel | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | blue CTA |
| Buttons_Divider | Buttons_Group | 1 | Image | center/center | .5,.5 | center | yes | gold sword+shield emblem |
| CTA_Replay | Buttons_Group | 2 | Button | — | .5,.5 | center | yes | ≈0.40 panel width |
| Replay_Frame/Fill/Label | CTA_Replay | 0,1,2 | Image/Image/Text | stretch/stretch/stretch | .5,.5 | center | yes | dark-stone CTA |

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
W=2340, H=1080, match HEIGHT.

**Background (full-bleed):** BG_CampaignMap 0,0→1,1 AspectFill; BG_Vignette heavy (corners→near-black `#010101`); BG_DarkenScrim α≈0.45.

**ResultPanel:** width ≈ **0.58·W ≈ 1357 px**, centered (x=0.50·W). Height ≈ **0.84·H ≈ 907 px**, vertically centered (sits a touch low to leave room for the star arc above). Aspect ≈ 1.50:1 (a wide landscape scroll). Cap: width = `min(0.58·W, 1.55·panelHeight)`.

**Banners:** Banner_Left anchored to panel top-left corner (pivot 0,1), width ≈ 0.10·panelW, hanging down ≈ 0.55·panelH; Banner_Right mirrored at top-right. They overlap the frame and extend slightly **outside** the parchment to the left/right (draped look).

**Star arc (Stars_Group):** spans the top of the panel, breaking the top frame edge. Star_Ribbon width ≈ 0.55·panelW centered at top. Center star (Star_C) at x=0.50·panelW, its center Y ≈ **-0.06·panelH** (above the frame top); diameter ≈ 0.16·panelH. Side stars at x≈0.38 and 0.62·panelW, center Y ≈ 0.00·panelH, diameter ≈ 0.13·panelH (slightly smaller + lower than center).

**Internal vertical rhythm (fractions of PANEL height, 0=panel top):**
| Element | Center Y (of panel) | Height (of panel) |
|---|---|---|
| Title "LEVEL 7 CLEARED" | 0.16 | 0.12 |
| "Clear Time" label | 0.30 | 0.05 |
| Clock icon + green time | 0.40 | 0.09 |
| "Rewards" label | 0.52 | 0.05 |
| Reward slot tiles | 0.66 | 0.22 |
| Buttons (Next/Replay) | 0.89 | 0.12 |

**Horizontal (fractions of PANEL width):**
- Title width ≈ 0.60, centered.
- Clear-time row centered; clock icon ≈ 0.06·panelW square just left of the value.
- Rewards row width ≈ 0.66; each slot tile ≈ 0.30·panelW × 0.20·panelH, gap ≈ 0.04 centered. Within a tile: icon in upper ~0.60, and a bottom row "small currency glyph + number" left-aligned-ish/centered.
- Buttons row width ≈ 0.82; NEXT LEVEL ≈ 0.40·panelW (left), REPLAY ≈ 0.40·panelW (right), gold emblem divider ≈ 0.06·panelW centered; button height ≈ 0.10·panelH.

**Notch/safe-area:** panel centered in safe rect; map bleeds under cutout. **Tablet (4:3):** the wide 1.5:1 scroll is height-fit; the cap keeps it ≤ ~0.7 of a narrow width. **Ultrawide:** more map revealed; banners/panel stay centered at absolute size.

---

## F · TYPOGRAPHY (per text)
Sizes px@1080. On **parchment**, titles/labels are **dark engraved serif** (not gold-on-dark) for contrast against the light scroll.

| Text | Content | Family / personality | Weight | Caps | Tracking | Effects | Size (px@1080) | Fill hex | Stroke/Shadow |
|---|---|---|---|---|---|---|---|---|---|
| Title_Cleared | `LEVEL 7 CLEARED` | serif display, embossed-into-parchment | Bold | ALL-CAPS | +4% | letterpress: dark fill with a faint light bottom-edge highlight (debossed) | ~58–64 | dark warm brown `#3a2a12` | light highlight `#f0e2b8` α0.5 offset (0,1) below; soft inner shadow above |
| ClearTime_Label | `Clear Time` | serif small-caps, label | SemiBold | small-caps (Title-case shown) | +8% | engraved | ~26–28 | muted brown `#5a4424` | light highlight (0,1) |
| ClearTime_Value | `02:45` | semi-condensed, tabular | Bold | n/a | 0% | **green glow**, slight emboss | ~46–52 | vivid lime-green `#93bf37` (cores brighter `#b6e35a`) | dark green stroke `#2c4a0c` ~1.5px + soft green outer glow α0.4 + shadow `#000` α0.4 (0,2) |
| Rewards_Label | `Rewards` | serif small-caps, label | SemiBold | small-caps (Title-case shown) | +8% | engraved | ~26–28 | muted brown `#5a4424` | light highlight (0,1) |
| Coins_Value | `12,450` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp on dark tile | ~34–38 | warm white `#f3ead2` | shadow `#000` α0.6 (0,2) |
| Gems_Value | `60` | semi-condensed, tabular | SemiBold | n/a | 0% | crisp on dark tile | ~34–38 | warm white `#f3ead2` | shadow α0.6 (0,2) |
| Next_Label | `NEXT LEVEL` | serif display, authoritative | Bold | ALL-CAPS | +6% | bright, faint blue glow | ~36–40 | warm white `#fffbf0` | dark-blue stroke `#0a1f44` ~2px + shadow α0.6 (0,3) |
| Replay_Label | `REPLAY` | serif display | Bold | ALL-CAPS | +6% | bright on dark stone | ~36–40 | warm white `#f3ecdc` | dark stroke `#1a140a` ~2px + shadow α0.6 (0,3) |

Note the **two text regimes**: dark-engraved serif on the parchment (title/labels/reward numbers sit on dark tiles so those are light) vs the green performance value vs the white CTA labels.

---

## G · MATERIALS
- **Parchment panel:** aged cream paper — base `#d6bd86`, lighter center `#e2c98a`, darker aged edges/stains `#b08a4e`–`#916330`; subtle fiber/grain texture; slightly uneven (torn-edge) border under the gold frame; soft inner shadow from the frame.
- **Gold frame + star ribbon + emblem + slot frames:** brushed antique gold — highlights `#f0d27a`, mid `#caa04a`, shadow `#6b5320`; engraved filigree; bloom on stars and star-ribbon highlights.
- **Stars (filled):** hot gold gradient `#ffe27a` (top) → `#e0a83a` (bottom) with a bright spec highlight and **bloom**; beveled 5-point with a slight inner facet line. **Stars (empty):** dark hollow `#23180f` interior with a thin gold rim `#a07a3a`, no bloom (clearly "not earned").
- **Blue banners:** Iron-Pact royal blue cloth `#1f3e78`–`#2b56c8` with stitched gold trim and a gold heraldic crest (lion/sigil) `#caa04a`; soft cloth folds + drop shadow onto the parchment; slight desaturation (background-tier).
- **Clock/stopwatch icon:** gold ring `#caa04a`, pale face `#e8dcc0`, dark hands; small.
- **Reward slot tiles:** dark recessed interior `#130f0a`–`#1c160e` (so light reward art/numbers pop) inside a gold bevel frame `#af874e`; inner shadow top.
- **Coin icon:** stacked silver/steel coins `#9aa0a8` highlights, `#5a6068` shadow, with a faint embossed sigil on the face. **Gem icon:** clustered violet crystals — highlights `#c89bff`, body `#7a3fd0`–`#5a2db0`, hot spec; magical inner glow.
- **NEXT LEVEL fill (blue):** royal-blue gradient `#0d2a66`→`#2f6fd6`, inner cobalt rim-glow `#5aa0ff` α≈0.45; satin sheen — the brightest button.
- **REPLAY fill (dark stone):** muted iron/stone gradient `#2a2620`→`#3a342a` with a faint top highlight; clearly secondary (matte, low glow). Gold frame matches.
- **Sword+shield emblem divider:** small gold heraldic crest (crossed/upright sword over a shield) with bevel + slight bloom, bridging the two buttons.
- **Bloom budget:** stars, the green time value, the gem cluster, and the NEXT LEVEL top edge carry bloom; parchment and REPLAY stay matte.

---

## H · COMPONENTS (states + feedback)
**Stars (display, animated):** each star reveals/pops in sequence (see §I). Earned = filled gold + pop + small sparkle burst + chime; unearned = remains a dark hollow outline (no pop, no sound). Optional: earned stars have a gentle persistent glint.

**Reward slots (display):** non-interactive; numbers may count-up (see §I). On reveal each tile does a brief scale-pop + the icon shimmers once.

**CTA_NextLevel (primary Button):**
- **Idle:** blue gradient, gold frame, inner blue rim, white label; subtle slow glow pulse (it is the encouraged progression).
- **Hover:** frame +8%, glow +25%, scale →1.03.
- **Pressed:** scale →0.97, fill darkens ~10%, glow flash, click SFX.
- **Disabled:** if there is **no next level** (last level of chapter/campaign), it becomes "WORLD MAP"/disabled-grey — but per the source the label is `NEXT LEVEL`; when unavailable, desaturate to grey `#3a342a` + label `#8a8278` + non-interactive (the implementation may instead swap to a "Map" action; that is an ADR/flow decision, not a redesign of this spec).
- **Confirm:** blue ring flash → next level intro/battle.

**CTA_Replay (secondary Button):**
- **Idle:** dark-stone fill, gold frame, white label; no glow pulse (secondary).
- **Hover:** frame +8%, slight fill lighten, scale →1.03.
- **Pressed:** scale →0.97, click SFX.
- **Disabled:** N/A (replay always available).
- **Confirm:** soft flash → re-enter the same level.

**Focus order:** default selected = **NEXT LEVEL**. Back/B → Campaign Map (system back; non-destructive). D-pad cycles Next ↔ Replay.

---

## I · ANIMATION TIMELINE (rich campaign reveal)
The campaign reveal is the richest of the result screens because of the **star rating ceremony**.

| t (s) | Duration | Element | Motion | Easing |
|---|---|---|---|---|
| 0.00 | 0.30 | FullBleedRoot CG | fade 0→1 (map + scrim) | linear |
| 0.10 | 0.40 | ResultPanel | scale 0.9→1.0 + α 0→1 (parchment unfurls/scales in) | back-ease-out (overshoot 1.02→1.0) |
| 0.20 | 0.35 | Banner_Left/Right | unfurl/drop down from corners (slight cloth sway settle) | ease-out + small bounce |
| 0.40 | 0.30 | Title_Cleared | α 0→1 + slide up 10px | ease-out |
| 0.70 | 0.30 | Star_Ribbon | α 0→1 + scale 0.8→1.0 (the banner the stars land on) | ease-out |
| 0.85 | 0.30 | Star_L | drop+pop into filled gold + sparkle burst + chime (if earned) | back-ease-out |
| 1.10 | 0.30 | Star_C | drop+pop, LARGER pop + brighter sparkle + higher chime (center, earned) | back-ease-out |
| 1.35 | 0.30 | Star_R | reveal: if earned → pop gold+chime; **if unearned → settle as a dark hollow outline, no pop/sound** | ease-out |
| 1.65 | 0.30 | ClearTime_Label + icon | α 0→1 + slide up 8px | ease-out |
| 1.75 | 0.70 | ClearTime_Value | **count-up time toward `02:45`** OR type-in reveal with a green flash on settle | ease-out |
| 2.10 | 0.25 | Rewards_Label | α 0→1 | ease-out |
| 2.25 | 0.30 | Reward_Slot_Coins | tile scale-pop in + icon shimmer | back-ease-out |
| 2.30 | 0.80 | Coins_Value | **count-up 0 → 12,450** (tabular, tick SFX) | ease-out |
| 2.35 | 0.30 | Reward_Slot_Gems | tile scale-pop in + gem shimmer | back-ease-out |
| 2.40 | 0.60 | Gems_Value | **count-up 0 → 60** | ease-out |
| 2.70 | 0.30 | CTA_NextLevel | α 0→1 + scale 0.94→1.0 + glow ignite | back-ease-out |
| 2.75 | 0.30 | Buttons_Divider + CTA_Replay | α 0→1 + scale 0.94→1.0 | ease-out |
| 3.05 | loop | CTA_NextLevel glow | breathing pulse (period ~1.8 s) | sine in-out |
| 3.05 | loop | earned stars | gentle glint every ~3 s | linear |

**OnNextLevel (exit):** press dip 60 ms → blue ring flash 120 ms → panel α→0 + scale→0.96 (180 ms) + bg fade → route to next level. **OnReplay:** soft flash → re-enter same level. 
**Skip rule:** tap during reveal snaps all tweens (including star pops and count-ups) to end-state and enables both CTAs. The **star ceremony still resolves to the correct earned/unearned end-state** on skip.

---

## J · PARTICLE & FX (passive)
- **Star sparkle bursts:** one-shot gold sparkle per earned star at its pop; persistent gentle glint afterward.
- **Star-ribbon shimmer:** subtle gold shimmer traveling along the banner.
- **Green time glow:** soft persistent green bloom on the clear-time value.
- **Gem cluster glow:** soft violet inner glow + occasional spec twinkle on the gem icon.
- **Coin tile glint:** occasional silver glint sweep on the coin stack.
- **Dust motes:** faint warm dust drifting in front of the parchment (campaign/adventure ambiance), very low opacity.
- **NEXT LEVEL glow:** persistent breathing cobalt rim (see §I).

---

## K · EVENT BEHAVIOR
- **OnShow(payload):** payload = `{ levelNumber:int, starsEarned:int (0–3), clearTimeSeconds:int, coins:int, gems:int, hasNextLevel:bool }` from ECS level evaluation + server reward grant (all **display-only**). Title formats `LEVEL {n} CLEARED`. Run the §I ceremony. The **number of filled stars is driven by `starsEarned`** — never hard-coded (source shows 2/3 as the example).
- **OnReward (count-ups):** coins → `12,450`, gems → `60`, clear-time settles to `02:45` (`MM:SS`). All display-only; the actual grant is server-authoritative (§12) — the UI never writes a balance.
- **OnNextLevel:** if `hasNextLevel`, route to the next level's intro/battle (or Campaign Map advanced to the next node). If not, the button is unavailable/relabeled per §H. Debounced.
- **OnReplay:** re-enter the same level (fresh attempt). Debounced. No balance mutation.
- **OnBack (B/Esc):** route to Campaign Map (non-destructive). 
- **Idempotency / re-entry:** settles to end-state immediately; no re-grant, no re-count, stars shown at their earned value.
- **No mutation:** issues only navigation + (Replay) a new-level request; never edits balance/progress client-side.

---

## L · NEGATIVE RULES (must NOT)
1. Do **not** hard-code 3 (or 2) filled stars — `starsEarned` (0–3) drives fills; unearned stars are dark hollow outlines. 
2. Do **not** put the title/labels in gold-on-dark — on parchment they are **dark engraved serif**; only the dark reward tiles carry light text. 
3. Do **not** color the clear-time anything but **green** (performance-good signal) as shown; do not invent a red/over-par variant unless the data supplies it (this spec records the green `02:45` case). 
4. Do **not** swap currency identities — left slot = silver coins, right slot = purple gems, in that order. 
5. Do **not** make REPLAY compete with NEXT LEVEL — REPLAY is dark/secondary/matte; NEXT LEVEL is the bright blue primary. 
6. Do **not** drop the blue Iron-Pact banners or the sword+shield emblem divider (signature campaign chrome). 
7. Do **not** put interactive content under the notch; only the map bleeds there. 
8. Do **not** mutate currency/progress client-side (§12); values are display-only. 
9. Do **not** stretch the gold frame/banner crest filigree (9-slice fixed). 
10. Do **not** skip the star ceremony resolution on tap-skip — it must still land on the correct earned/unearned end-state. 
11. Do **not** use portrait or assume width is stable; match HEIGHT, fraction-based.

---

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
- [ ] A **light parchment scroll** panel (cream `#d6bd86`–`#e2c98a`, aged texture) with an ornate gold frame, flanked by **blue Iron-Pact war-banners** draped over the top-left and top-right corners, centered over a dimmed, heavily vignetted campaign map.
- [ ] A **3-star arc** on a gold ribbon breaks the top frame: center star largest+highest, two side stars smaller+lower; **2 filled hot-gold + 1 dark hollow** (driven by `starsEarned`).
- [ ] Title reads exactly `LEVEL 7 CLEARED` in dark engraved serif on the parchment.
- [ ] `Clear Time` label + gold stopwatch icon + `02:45` in **vivid lime-green** with a green glow.
- [ ] `Rewards` label + two gold-framed dark slot tiles: left **silver coin stack + `12,450`**, right **purple gem cluster + `60`**.
- [ ] Two buttons: **NEXT LEVEL** (royal-blue primary, glowing) and **REPLAY** (dark-stone secondary), separated by a **gold sword+shield emblem** divider.
- [ ] Reveal ceremony plays in order (panel → banners → title → star ribbon → L/C/R star pops → time count → reward tiles + count-ups → CTAs), with tap-to-skip that resolves the correct star end-state.
- [ ] NEXT LEVEL shows idle/hover/pressed + breathing glow and routes forward; REPLAY re-enters the level; back → Campaign Map.
- [ ] Background bleeds under the notch; content respects safe area; layout fraction-based, match-height.
- [ ] No client balance/progress mutation; star count and values are data-driven, not hard-coded.
- [ ] Side-by-side with `CampaignResultDesign.png`, positions within ±2% of panel dims and colors within stated ranges → **≥95%**.

---

## N · IMPLEMENTATION CONFIDENCE
**90 / 100.** Layout, copy, currencies, and the star/green-time/reward structure are all directly legible, so structural + color fidelity is high. Deductions: (a) the star **rating ceremony** (per-star pop/sparkle/chime + correct earned/unearned resolution under skip) is the most logic-rich reveal of the five and must be built carefully (~ -3); (b) bespoke art — the blue heraldic banners with gold crest, the sword+shield emblem divider, parchment texture, filled/hollow star art, coin & gem clusters — must be authored/sourced to match (~ -4); (c) the green clear-time presumably has thresholds (green = under target) that may need data the spec can't fully define from a single example, and the NEXT-LEVEL-when-no-next behavior is an ADR/flow detail (~ -3). No structural ambiguity in the shown state.

---

## O · SELF-CHECKLIST
- [x] Read source PNG (`design/CampaignResultDesign.png`, 1672×941) before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header has number, screen ID, source, resolution, "Analysis-only forensic spec."
- [x] Geometry as **fractions of 2340×1080**, match-HEIGHT, safe-area + full-bleed stated.
- [x] Verbatim labels recorded (`LEVEL 7 CLEARED`, `Clear Time`, `02:45`, `Rewards`, `12,450`, `60`, `NEXT LEVEL`, `REPLAY`) — nothing invented.
- [x] Forensic hex ranges from the art (parchment, green `#93bf37`, gem purple `#8c07cc`, gold).
- [x] Full ASCII tree + per-node Unity table.
- [x] Rich star-ceremony + count-up reveal timeline with tap-to-skip that resolves star state.
- [x] §12 boundary honored (display-only values; no balance/progress mutation).
- [x] Negative rules + ≥95% acceptance + confidence rationale.
- [x] No code/asset/scene/prefab/gameplay changes, no commit.



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 17 · Store

Source: design/StoreScreenDesign.png · 1672×941 (1.78:1) · Analysis-only forensic spec.

> **Shop-chrome anchor file.** This screen is the canonical reference for the **shared shop chrome** — top
> TAB BAR (SPELLS · SKINS · CHESTS · STORE), top-right currency chips (Gems + Gold, each with a green "+"),
> and top-left Back. Specs 18–20 (Spells/Skins/Chests) **inherit Sections C/D/E/F/H for the chrome verbatim**
> and only re-document deltas. Read this file first when building any shop tab.

---

## A · SCREEN PURPOSE
The **Store** is the hard-currency / IAP storefront tab of the shop hub. It sells **Gems** (premium currency),
**resource bundles**, and cross-promotes the **Battle Pass**. Player tasks: (1) buy the hero **Legendary
Starter Bundle** (a one-time, time-limited value bundle), (2) buy one of **five gem packs** at ascending
price tiers, (3) jump to the **Battle Pass** via a side promo, (4) re-filter the catalog via **five bottom
sub-tabs** (FEATURED / GEMS / RESOURCES / OFFERS / DAILY DEALS). The screen shown is the **FEATURED** sub-tab.

This is the only shop tab that spends **real money** ($ prices), so the visual hierarchy pushes the high-value
bundle and the "BEST SELLER" pack hardest. A red notification dot on the STORE tab signals a new/featured offer.

**ADR note (Section L):** Store itself is monetization-clean (direct-price gem packs + bundle, no randomness).
The right-rail **Battle Pass** promo and the **CHESTS** tab it sits beside are the gacha surfaces — see 20/21.

---

## B · VISUAL DNA (screen-specific, on top of the global baseline)
- **Mood:** opulent royal treasury / merchant hall. Near-black obsidian field; brushed-gold ornate frames;
  the whole screen is denser and brighter than the meta screens because it must read as "premium store."
- **Palette anchors:** gem-violet / amethyst (`#7a3fd0`–`#b06bff`) dominates (gems are the product); gold/bronze
  frames (`#caa04a`–`#f0d27a` hi, `#6b5320` shadow); cobalt accents on the bundle chest & BP art; **gold price
  ribbon** for the bundle ($9.99) and **violet price chips** for the gem packs ($-labels on cobalt-violet).
- **Lighting:** warm focal glow on the central Starter-Bundle chest (volumetric god-ray behind it); softer rim
  on each pack card; vignette darkens the four screen corners; bloom on every gem pile.
- **Background:** full-bleed stone-vault wall with hanging gold lantern / treasure props at far left & right
  (these bleed under the safe area on wide devices; never let UI overlap them).
- **Hierarchy:** Starter Bundle (top-left hero block, biggest) → BEST SELLER pack (center, lifted) → other
  packs → Battle Pass promo (right rail) → tab bar / currencies / sub-tabs (chrome).

---

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
StoreScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, stone-vault art, extends UNDER cutout)
   │  └─ Bg_Vignette (Image, radial dark overlay)
   ├─ TopChrome  ── [SHARED — anchor definition]
   │  ├─ BackButton (Button)
   │  │  ├─ Back_Plate (Image, blue beveled gold-edged plate)
   │  │  └─ Back_Arrow (Image, gold curved left-arrow)
   │  ├─ TabBar (HorizontalLayoutGroup container, gold-framed bar)
   │  │  ├─ Tab_Spells   (Toggle) → Icon(blue spell-orb) + Label "SPELLS"
   │  │  ├─ Tab_Skins    (Toggle) → Icon(warrior helm)   + Label "SKINS"
   │  │  ├─ Tab_Chests   (Toggle) → Icon(treasure chest) + Label "CHESTS"
   │  │  └─ Tab_Store    (Toggle, SELECTED) → Icon(shopping cart) + Label "STORE"
   │  │     └─ Tab_NotifyDot (Image, red dot, top-right of Store tab)
   │  └─ CurrencyChips (HorizontalLayoutGroup)
   │     ├─ GemChip  (container) → Icon(violet faceted gem) + Value "1746" + AddBtn(green "+")
   │     └─ GoldChip (container) → Icon(silver star-coin)   + Value "58,420" + AddBtn(green "+")
   ├─ Content (safe-area inner; three columns: bundle stack | pack row | BP rail)
   │  ├─ StarterBundle_Card (large panel, top-left/center)
   │  │  ├─ Bundle_Frame (Image, ornate gold frame)
   │  │  ├─ Bundle_Title  (Text "LEGENDARY\nSTARTER BUNDLE")
   │  │  ├─ Bundle_Subtitle (Text "Best value!" + "Limited time offer!")  [two-color]
   │  │  ├─ Bundle_Hero (Image, cobalt war-chest spilling gems, god-ray)
   │  │  ├─ Bundle_ContentsBar (HorizontalLayoutGroup, dark rounded strip)
   │  │  │  ├─ Item_Gems   (Icon violet gem  + "2000")
   │  │  │  ├─ Item_Gold   (Icon silver coin + "50000")
   │  │  │  ├─ Item_Keys   (Icon blue key    + "5")
   │  │  │  └─ Item_Potion (Icon violet vial + "10")
   │  │  ├─ Bundle_ValueBadge (Image+Text "400%\nVALUE", angled gold burst)
   │  │  └─ Bundle_PriceBtn (Button, gold ribbon, "$9.99")
   │  ├─ GemPack_Row (HorizontalLayoutGroup, 5 cards)
   │  │  ├─ Pack_1 "HANDFUL OF GEMS"  → gem-pile art + "80"   + PriceBtn "$0.99"
   │  │  ├─ Pack_2 "SACK OF GEMS"     → gem-pile art + "500"  + PriceBtn "$4.99"
   │  │  ├─ Pack_3 "CHEST OF GEMS"    → [BEST SELLER ribbon] gem-chest art + "1200" + PriceBtn "$9.99"
   │  │  ├─ Pack_4 "VAULT OF GEMS"    → gem-vault art + "2500" + PriceBtn "$19.99"
   │  │  └─ Pack_5 "MOUNTAIN OF GEMS" → gem-pyramid art + "6500" + PriceBtn "$49.99"
   │  └─ BattlePass_Promo (panel, right rail)
   │     ├─ BP_Frame (Image, gold frame)
   │     ├─ BP_Title (Text "BATTLE PASS") + BP_InfoDot (ⓘ)
   │     ├─ BP_Season (Text "— SEASON 1 —")
   │     ├─ BP_Name (Text "GLORY OF KINGS")
   │     ├─ BP_ValueBadge (Text "400%\nVALUE", angled, left edge)
   │     ├─ BP_Art (Image, dark-knight key art)
   │     ├─ BP_RewardStrip (Row of 3 reward thumbs: gold token, violet chest, blue gem)
   │     ├─ BP_Caption (Text "UNLOCK PREMIUM REWARDS!")
   │     └─ BP_GoBtn (Button "GO NOW" + crown icon, gold)
   └─ SubTabBar (bottom, HorizontalLayoutGroup, 5 toggles)
      ├─ SubTab_Featured (SELECTED) → gold star + "FEATURED"
      ├─ SubTab_Gems        → violet gem + "GEMS"
      ├─ SubTab_Resources   → silver coin + "RESOURCES"
      ├─ SubTab_Offers      → red gift   + "OFFERS"
      └─ SubTab_DailyDeals  → hourglass  + "DAILY DEALS"
```

---

## D · UNITY HIERARCHY SPEC (per node)

### Shared chrome (canonical — 18/19/20 inherit)
- **StoreScreen** — parent: UiRouter content; type: GameObject + `UiScreen`, `CanvasGroup`. Anchors stretch
  full. Built in code; no prefab.
- **SafeAreaRoot** — parent StoreScreen; `SafeAreaFitter`; anchors stretch 0..1; offsets 0. All interactive
  chrome parents here. **Bg_FullBleed is OUTSIDE** safe area (true 0..1 of canvas) so art bleeds under notch.
- **Bg_FullBleed** — parent StoreScreen (not SafeAreaRoot); Image; anchor stretch full; pivot .5,.5;
  `preserveAspect`=false (cover). **Bg_Vignette** child, same rect, multiply overlay.
- **BackButton** — parent SafeAreaRoot; Button; anchor **top-left** (0,1) pivot 0,1; pos ≈ (+x 1.0%, −y 1.5%)
  inset. Square ~ 0.045 W. Children Back_Plate (fill) + Back_Arrow (centered, ~70% of plate).
- **TabBar** — parent SafeAreaRoot; container Image (gold bar) + `HorizontalLayoutGroup`
  (spacing≈8px@1080, childForceExpandWidth=false, childAlignment=MiddleCenter, padding L/R≈14). Anchor **top-
  center** (0.5,1) pivot 0.5,1; y inset −1.5%. Width ≈ 0.44 W (centered between Back and currencies).
  Children: 4 `Toggle` (shared ToggleGroup `ShopTabs`). Each tab = HorizontalLayoutGroup(Icon 28px + Label).
  Child order = Spells, Skins, Chests, Store.
- **Tab_NotifyDot** — parent Tab_Store; Image (red circle ~10px@1080) anchor top-right (1,1) pivot .5,.5,
  small outward offset.
- **CurrencyChips** — parent SafeAreaRoot; `HorizontalLayoutGroup` (spacing≈14px). Anchor **top-right** (1,1)
  pivot 1,1; pos inset (−x 1.0%, −y 1.5%). Children GemChip, GoldChip (left→right). Each chip = HLG of
  [Icon 30px][Value Text right-aligned][AddBtn 30px green "+"].

### Store-specific content
- **Content** — parent SafeAreaRoot; empty RectTransform; anchor stretch with insets: top ≈ 9% (below
  chrome), bottom ≈ 9% (above sub-tabs), left/right ≈ 1.5%. NOT a layout group (free 3-column placement via
  child anchors).
- **StarterBundle_Card** — parent Content; anchor top-left region; pivot 0,1; rect ≈ x 0.07→0.555 W,
  y top 0.135→0.34 H (a wide landscape banner). Internal: Title top-left, Hero center-right (chest art),
  ContentsBar a dark rounded strip lower-left, ValueBadge near chest, PriceBtn bottom-right corner.
- **Bundle_ContentsBar** — `HorizontalLayoutGroup`, 4 items even, childAlignment MiddleCenter; each item is a
  vertical mini-group [Icon over Value] OR icon-left/value-right (art shows icon-above-number; treat as
  vertical mini-group, spacing≈4).
- **GemPack_Row** — parent Content; `HorizontalLayoutGroup` (childForceExpandWidth=true, spacing≈12px@1080,
  childAlignment=MiddleCenter). Anchor: x 0.07→0.79 W, y 0.40→0.86 (the main pack band). 5 equal cards.
  Pack_3 (BEST SELLER) is scaled ~1.05 and lifted ~−12px (see H).
- **Each Pack card** — vertical stack: [optional Ribbon] → Title (1–2 lines) → ArtIcon → GemRow(icon+count)
  → PriceBtn. Use VerticalLayoutGroup inside the card with controlled spacing.
- **BattlePass_Promo** — parent Content; anchor **right** column; pivot 1,1; rect ≈ x 0.79→0.985 W,
  y 0.10→0.875 H (tall narrow rail). Vertical internal stack (Title→Season→Name→Art→RewardStrip→Caption→GoBtn).
- **SubTabBar** — parent SafeAreaRoot; container Image (gold bar) + `HorizontalLayoutGroup` (5 toggles,
  childForceExpandWidth=true, childAlignment MiddleCenter). Anchor **bottom-center** (0.5,0) pivot 0.5,0;
  y inset +1.0%; width ≈ 0.66 W centered. Each sub-tab = vertical mini-group [Icon over Label] (art shows
  icon above text).

**Responsive:** chrome anchored to its own corner/edge → stable on any aspect. Content uses fraction rects;
on ultrawide the bg reveals more side props and the 3 columns keep their fractions (extra gutter splits
evenly). On 4:3 tablet, content insets grow; GemPack_Row stays 5-up until <2.0:1 then may wrap to 3+2 (allow
GridLayoutGroup fallback, constraint=FixedColumnCount 5, switch to 3 below threshold).

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
All Y measured from top. Canvas 2340 W × 1080 H.

**Chrome band (shared):**
- Top chrome occupies y 0 → ~0.085 H (≈92px). Back, TabBar, CurrencyChips vertically centered in it.
- BackButton: ~0.045 W square (≈105px) at x 0.010 W, y −0.015 H.
- TabBar: width 0.44 W (≈1030px) centered at x 0.5; height ≈0.062 H (≈67px). 4 tabs share it equally
  (~0.11 W each incl. spacing).
- CurrencyChips: right-anchored, total width ≈0.27 W; gem chip ~0.12 W, gold chip ~0.13 W (gold value is
  longer "58,420"), gap 0.006 W.
- SubTabBar: width 0.66 W (≈1545px) centered; height ≈0.075 H (≈81px); bottom inset +0.01 H. Each of 5 cells
  ≈0.132 W.

**Starter Bundle card:** x 0.07→0.555 (Δ0.485 W ≈1135px) · y 0.135→0.345 (Δ0.21 H ≈227px).
- Title block left ≈x 0.085, two serif lines.
- ContentsBar: dark strip x 0.085→0.34, y 0.255→0.315 (4 items each ≈0.062 W).
- PriceBtn ribbon: x 0.485→0.555, y 0.275→0.325 (gold ribbon ≈0.07 W × 0.05 H).
- ValueBadge "400% VALUE": small angled burst near the chest, x ≈0.50, y ≈0.16.

**Gem pack row:** band x 0.07→0.79 (Δ0.72 W ≈1685px) · y 0.40→0.86 (Δ0.46 H ≈497px).
- 5 cards, gaps ≈0.011 W → each card ≈0.135 W (≈316px) × 0.46 H. Card_3 scaled 1.05 → ~0.142 W and lifted.
- Inside a card: Title at top 12%, Art icon center 45%, GemRow ~70%, PriceBtn bottom 86–98% (price chip
  ≈0.10 W × 0.045 H).

**Battle Pass rail:** x 0.79→0.985 (Δ0.195 W ≈456px) · y 0.10→0.875 (Δ0.775 H ≈837px).
- Title y 0.12, Season y 0.18, Name y 0.225, ValueBadge angled at left edge y ≈0.27, Art y 0.30→0.66,
  RewardStrip y 0.70 (3 thumbs each ≈0.05 W), Caption y 0.78, GoBtn y 0.82→0.87 (gold button full rail width).

**Notch/tablet:** SafeAreaFitter insets the whole interactive layer; Bg_FullBleed unaffected. Ultrawide
(21:9+) → side props reveal; keep TabBar ≤0.44 W so it never collides with currencies.

---

## F · TYPOGRAPHY (per text element)
Sizes given at 1080-tall reference. Serif = Trajan/Cinzel-style display (TMP SDF target; legacy Text fallback).

| Element | Face / personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tab labels (idle) | Serif display | Semibold | UPPER | +4% | 26 | soft inner shadow | `#c9b27a` muted gold |
| Tab label (selected STORE) | Serif display | Bold | UPPER | +4% | 27 | gold bevel + faint glow | `#f3d98a` |
| Currency value (Gems/Gold) | Semi-condensed sans | Bold | — | 0 | 28 | dark stroke 1px + drop-shadow | `#f4ead0` near-white gold |
| Bundle title "LEGENDARY / STARTER BUNDLE" | Serif display | Black | UPPER | +2% | 46 (line1 36 small-caps "LEGENDARY", line2 52) | heavy gold bevel + bloom + outer dark stroke | `#f0cf72`→`#caa04a` grad |
| Bundle "Best value!" | Serif italic | Semibold | Title | 0 | 20 | shadow | `#e7dcc0` cream |
| Bundle "Limited time offer!" | Serif italic | Semibold | Title | 0 | 20 | shadow | `#e0432a` ember (highlight) |
| Bundle contents numbers (2000/50000/5/10) | Sans | Bold | — | 0 | 24 | dark stroke + shadow | `#f4ead0` |
| Value badge "400% VALUE" | Sans condensed | Black | UPPER | 0 | 22 (two lines) | red→gold burst fill, white text, dark outline | text `#fff8e6` on `#c12a20`/gold burst |
| Bundle price "$9.99" | Serif | Bold | — | 0 | 30 | on gold ribbon, dark-brown text | `#3a2a10` on gold ribbon |
| Pack titles ("HANDFUL OF GEMS" …) | Serif display | Semibold | UPPER | +3% | 24 (2 lines, ~20 each) | gold bevel + shadow | `#e9cf86` |
| "BEST SELLER" / "BEST VALUE!" ribbon | Sans condensed | Black | UPPER | +2% | 18 | on gold tab, dark text | `#2c1f08` on `#e8b94a` |
| Pack gem counts (80/500/1200/2500/6500) | Sans | Bold | — | 0 | 30 | white, dark stroke; gem icon to left | `#ffffff` |
| Pack $ prices ($0.99 …) | Sans | Bold | — | 0 | 26 | on cobalt-violet chip, light text | `#f3ecff` on `#3a2c7a`/violet |
| BP "BATTLE PASS" | Serif display | Bold | UPPER | +4% | 30 | gold bevel + glow | `#f3d98a` |
| BP "SEASON 1" | Sans | Semibold | UPPER | +6% | 18 | between two rule lines | `#cdb887` |
| BP "GLORY OF KINGS" | Serif display | Bold | UPPER | +2% | 26 | gold + bloom | `#f0cf72` |
| BP "UNLOCK PREMIUM REWARDS!" | Sans condensed | Bold | UPPER | +2% | 18 | shadow | `#e7dcc0` |
| BP "GO NOW" | Serif | Bold | UPPER | +3% | 26 | on gold button, dark text + crown | `#2c1f08` on gold |
| Sub-tab labels | Serif display | Semibold | UPPER | +4% | 22 (selected 23 brighter) | bevel + shadow; selected gold-lit | idle `#b9a36e` / sel `#f3d98a` |

---

## G · MATERIALS (hex ranges, finish, wear, edges, bloom)
- **Gold/bronze frames (all chrome + cards):** base `#caa04a`, highlight `#f0d27a`–`#fff0b8` on top bevel,
  shadow `#6b5320`–`#3a2a10` in grooves; roughness mid (satin brushed), engraved filigree on outer rail;
  worn micro-scratches on edges; gentle rim-bloom on lit frames (selected tab/active slot).
- **Dark panel bases (tab bar, contents bar, pack interiors):** obsidian `#0c0e14`–`#161a24` vertical
  gradient, ~85% opaque, inner top-edge sheen; subtle noise.
- **Back plate:** cobalt `#1c3a8e`→`#274aa6` enamel with gold bevel edge; arrow `#f0cf72` with shadow.
- **Gems / amethyst (product hero):** crystal `#7a3fd0` core → `#b06bff` faces → `#e7ccff` specular hot-spots;
  high reflection, strong inner glow, additive bloom; cut facets with sharp white speculars. Gem piles read
  as hundreds of faceted crystals with violet glow + warm gold underlight.
- **Silver coin (Gold currency):** brushed silver `#b9bcc4`→`#eef0f4` rim, dark center star emboss `#6a6e78`,
  cool specular.
- **Blue key icon (bundle):** steel-blue `#3f6fd8`→`#9fc0ff`, ornate bow, gold-ish teeth highlight.
- **Violet potion vial:** glass `#2a1840` body, liquid `#8a40d8`→`#c07bff`, cork gold cap, inner glow.
- **Cobalt war-chest (bundle hero):** royal-blue lacquered chest `#1e3a8c`→`#3a63d0`, gold fittings, a blue
  gem on the lid, spilling violet gems; lit by a warm god-ray from upper-back.
- **Price ribbon (gold, bundle):** cast-gold banner with notched ends, `#e8b94a`→`#f6d77a`, dark engraved
  text; faint specular sweep.
- **Price chips (gem packs):** cobalt-violet plate `#2c2370`→`#4a3aa6`, thin gold/edge line, light text.
- **BP key art:** desaturated dark-knight scene, cool blues + ember rim; framed in gold; slight inner vignette.
- **Sub-tab bar:** same gold-frame + obsidian base; selected cell gets a gold up-light + brighter icon.

---

## H · COMPONENTS (states + feedback)

**Shop Tab (Toggle, shared) — SPELLS/SKINS/CHESTS/STORE**
- *idle:* obsidian cell, muted-gold icon + label, no glow.
- *hover (pad/cursor):* +6% brightness, faint gold under-glow.
- *pressed:* −4% scale, brighter.
- *selected:* gold-lit frame around the cell, glow halo, label brightens to `#f3d98a`, icon full-color; an
  upward gold light-bleed (see Spells/Skins/Chests where the active tab clearly glows gold). Only one selected
  (ToggleGroup). STORE is selected here.
- *disabled:* 45% opacity, desaturated.
- *notify:* red dot (Tab_Store here) — pulse 1.0–1.15 scale loop 1.2s.

**Currency chip + AddBtn**
- chip static (display). **AddBtn** green "+" (`#2faa3a`→`#46d257`, gold edge): idle / hover +8% / pressed
  −6% scale → routes to Store GEMS or RESOURCES sub-tab (OnAddCurrency).

**Gem-pack card (Button)**
- *idle:* gold-framed card, art + count + price chip.
- *hover:* lift −6px, frame brightens, art bloom +10%.
- *pressed:* −3% scale, price chip flashes.
- *BEST SELLER (Pack_3):* persistent gold "BEST SELLER" ribbon top-center, card scaled 1.05 + raised, extra
  rim-glow → strongest pack. (Bundle uses "Best value!" copy instead of a ribbon.)
- *purchasing:* price chip → spinner; on success: gem-count fly-to GemChip + chip "+N" tick-up (count-up
  ~0.6s) + sparkle burst on card; on fail: red shake + insufficient/IAP-error modal.
- *owned/one-time (Bundle):* after purchase → "PURCHASED" stamp + button disabled (45%).

**Bundle price button** — gold ribbon `$9.99`; hover brighten, pressed −3%, success → purchase flow + bundle
contents fly to currency chips (gems→GemChip, gold→GoldChip, keys/potions→inventory toast).

**Battle Pass GO NOW button** — gold pill + crown; hover brighten + crown sparkle; pressed −3%; OnClick →
route to Battle Pass screen (25).

**Sub-tab (Toggle)** — vertical icon+label; selected = gold up-light + brighter; switching re-filters the
catalog (cross-fade the pack/bundle area).

---

## I · ANIMATION TIMELINE
- **OnShow (0.00→0.45s):** Bg fade-in 0→1 (0.25s) → chrome slides from top −20px ease-out (0.0–0.25) →
  StarterBundle scales 0.96→1 + fade (0.10–0.35) → GemPack_Row cards stagger-in left→right, each
  0.92→1 scale + fade, 40ms stagger (0.15–0.45) → BP rail slides from right +24px (0.20–0.40).
- **Idle loops:** Bundle chest god-ray slow pulse (3s, opacity 0.8↔1.0); gem piles slow sparkle (random
  specular twinkles); BEST SELLER ribbon faint gold shimmer (2.5s); STORE notify dot pulse (1.2s); BP art
  subtle ember drift.
- **Tab switch (out 0.12s / in 0.18s):** current content CanvasGroup fade 1→0 + −8px, new content fade 0→1
  + 8px→0; tab gold-glow snaps to new tab (0.10s). Easing ease-in-out.
- **Sub-tab switch:** pack/bundle band cross-fade 0.18s + 6px slide.
- **Buy success:** card sparkle burst (0.4s) → currency icons arc to chip (0.5s, ease-in) → chip count-up
  (0.6s) → chip bump 1.0→1.12→1.0 (0.18s).
- **Buy fail:** card + price chip red shake (x ±6px, 0.3s, 3 cycles) → insufficient/IAP-error modal fade-in.

Easing: UI moves ease-out/in-out 0.12–0.25s; celebratory bursts ease-out 0.4–0.6s.

---

## J · PARTICLE & FX
- Bundle chest: warm volumetric god-ray (soft additive cone) + slow floating dust motes + gem-pile sparkle.
- Gem packs: per-card faint violet bloom + occasional white specular twinkle on the gem art.
- BEST SELLER: thin gold sweep across the ribbon (2.5s loop).
- Buy success: 12–18 gold/violet spark particles burst from card, then currency-icon projectiles to chip.
- BP rail: subtle ember/ash particles drifting up over the key art; crown on GO NOW emits a tiny sparkle on
  hover.
- Vignette + global bloom post applied to whole screen (treasury glow).

---

## K · EVENT BEHAVIOR
- **OnShow:** load catalog (server-auth prices/SKUs); bind currency values from wallet; resolve STORE notify
  dot from "has new offer" flag; default sub-tab = FEATURED.
- **OnTabSelect(tab):** UiRouter swaps shop tab content (Spells/Skins/Chests/Store) — same chrome persists.
- **OnSubTabSelect(filter):** re-query catalog filter; re-fill bundle/pack band.
- **OnBuyPack(sku) / OnBuyBundle:** invoke IAP/server purchase (client never mutates balance — §12); on
  success server returns new wallet → animate count-up + grant toast; one-time bundle flips to PURCHASED.
- **OnInsufficient / OnPurchaseError:** show ConfirmModal "Insufficient"/"Purchase failed" sheet (37).
- **OnAddCurrency(gem/gold "+"):** route to GEMS / RESOURCES sub-tab (or platform store).
- **OnBattlePassGo:** route to Battle Pass screen (25).
- **OnBack:** UiRouter pop → previous hub.

---

## L · NEGATIVE RULES
- Do NOT redesign or rename anything; copy/prices are forensic: bundle **$9.99** with **2000 gems / 50000
  gold / 5 keys / 10 potions** and a **400% VALUE** badge; packs **$0.99/80, $4.99/500, $9.99/1200 (BEST
  SELLER), $19.99/2500, $49.99/6500**; BP **SEASON 1 — GLORY OF KINGS, 400% VALUE, GO NOW**; sub-tabs
  **FEATURED/GEMS/RESOURCES/OFFERS/DAILY DEALS**; currencies **Gems 1746 / Gold 58,420**.
- Do NOT move currencies off top-right, Back off top-left, or the shop tab bar off top-center.
- Do NOT let interactive UI cross into the full-bleed side props; keep content in safe area.
- Bg_FullBleed stays OUTSIDE SafeAreaFitter; all interactive nodes INSIDE it.
- **§12 / server-auth:** UI never mutates a balance; purchases go through server/IAP and reflect the returned
  wallet. **No gameplay/ECS/balance change.**
- **ADR flag:** Store packs/bundle are direct-price (clean). The neighboring **CHESTS** tab and the **Battle
  Pass** are the monetization surfaces requiring the loot-box / value-claim ADRs (see 20/21) — do not "fix"
  them here; this is the visual spec only.
- Invent nothing not in the PNG (no extra packs, no fabricated timers).

## M · ACCEPTANCE CRITERIA (≥95% fidelity)
1. Shared chrome present & positioned: Back top-left, tab bar (4 tabs, STORE selected + red dot) top-center,
   Gems 1746 + Gold 58,420 chips with green "+" top-right.
2. Starter Bundle: two-line gold title, "Best value!" + ember "Limited time offer!", 4-item contents bar
   (2000/50000/5/10 with correct icons), 400% VALUE badge, gold $9.99 ribbon — all in the top-left banner.
3. Five gem packs in order with correct names, gem counts, $ prices; Pack_3 carries BEST SELLER ribbon and is
   visually lifted/largest.
4. Battle Pass rail on the right: SEASON 1 / GLORY OF KINGS / 400% VALUE / reward strip / UNLOCK PREMIUM
   REWARDS! / gold GO NOW with crown.
5. Five bottom sub-tabs with correct icons/labels, FEATURED selected.
6. Palette: amethyst gems dominate, gold frames, cobalt accents, vignette+bloom treasury mood.
7. Layout matches fraction math within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet.
8. States (tab selected, BEST SELLER, buy success count-up, insufficient shake) behave per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**93/100.** Layout, copy, prices, icon set, and chrome are crisp and fully legible at zoom. Minus points:
(a) exact gem-pile art per pack is bespoke 2.5D render (approximate with layered gem sprites + glow);
(b) the 400% VALUE burst shape and BP key-art are unique illustrations (rebuild as framed image + badge);
(c) precise inner-padding of the bundle contents bar is estimated. None affect interactive fidelity.

## O · SELF-CHECKLIST
- [x] Read source PNG (+ zoom crops: top bar, currency, bundle, packs, BP, sub-tabs) before writing.
- [x] All A–O sections present and substantive, in order.
- [x] Full node tree incl. shared tab bar + currency chips + back (anchor file).
- [x] Fraction-based layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Forensic copy/prices/counts/colors recorded; nothing invented.
- [x] §12 / server-auth + ADR pointers noted in A/L (no spec alteration).
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 18 · Spells

Source: design/SpellsScreenDesign.png · 1914×822 (2.33:1) · Analysis-only forensic spec.

> **Inherits the shared shop chrome from `17_Store_SPEC.md`** (Back top-left, tab bar SPELLS·SKINS·CHESTS·
> STORE top-center, Gems+Gold currency chips top-right). This file re-documents the chrome briefly and details
> only the **Spells-specific** body: a left **mage presenter**, four **crystal spell orbs** with gem prices,
> and a right **parchment detail panel** with **BUY**. Here the **SPELLS** tab is selected (gold-lit).

---

## A · SCREEN PURPOSE
The **Spells** shop tab sells **temporary battle spells / boons** purchased with **Gems**. A hooded mage
"presents" the wares; four crystal orbs on a draped table each hold a spell sigil and show a gem price; the
selected orb expands into a parchment scroll on the right describing the spell (name, effect, duration,
flavor) with a prominent **BUY** button repeating the price. Player tasks: browse the 4 orbs, select one to
read its scroll, and BUY with gems. Shown selected: **MINER'S BLESSING** (the leftmost golden-pickaxe orb).

**ADR note (Section L):** Spells are gem-purchased consumables that buff in-match performance. If any spell
grants a competitive power advantage purchasable only with premium gems, it brushes the "gems never buy power"
principle → flag for the economy ADR. Spec'd forensically regardless.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** an arcane wizard's study / spell shop — warm candlelit bookshelves behind, a focal blue magical
  glow on the mage's staff and the orbs. Magic = violet/cyan; commerce = gold.
- **Palette anchors:** amethyst/violet gem prices (`#8a40d8`–`#c07bff`); cyan-blue magical glow on staff &
  glowing eyes (`#3fd0ff`); crystal-orb glass (cool blue-white) with per-spell colored sigil; parchment cream
  scroll (`#d9c79a`–`# efe2bf`); gold frames; the BUY button is **violet/amethyst** (premium spend color),
  gold-edged — the brightest CTA.
- **Lighting:** focal glow on the four orbs (each emits its sigil's color) + a strong cyan key on the mage's
  staff orb; warm bookshelf ambience; vignette; bloom on glass + glow.
- **Background:** full-bleed dim library/study (shelves, candles, props) behind the mage and table; bleeds
  under cutout.
- **Hierarchy:** parchment scroll + BUY (right, the decision) ⟷ selected orb glow → mage presenter (left,
  characterful framing) → orb row (choices) → chrome.

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
SpellsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, wizard-study art)  [OUTSIDE safe area]
   │  └─ Bg_Vignette (Image)
   ├─ TopChrome  ── [SHARED — see 17 §C/§D]
   │  ├─ BackButton (Button: Plate + gold Arrow)
   │  ├─ TabBar (ToggleGroup ShopTabs)
   │  │  ├─ Tab_Spells (Toggle, SELECTED, gold-lit) → Icon(blue spell-orb) + "SPELLS"
   │  │  ├─ Tab_Skins  (Toggle) → Icon(helm) + "SKINS"
   │  │  ├─ Tab_Chests (Toggle) → Icon(chest) + "CHESTS"
   │  │  └─ Tab_Store  (Toggle) → Icon(cart) + "STORE"
   │  └─ CurrencyChips
   │     ├─ GemChip  → violet gem + "1726" + green "+"
   │     └─ GoldChip → silver coin + "48570" + green "+"
   ├─ MagePresenter (Image, hooded mage holding glowing staff, left foreground)
   │  ├─ Mage_Body (Image)
   │  └─ Staff_Glow (Image, cyan orb on staff, additive)
   ├─ OrbTable (group, center-bottom, blue-draped table w/ gold crown motif)
   │  ├─ Table_Cloth (Image, royal-blue cloth, gold crown emblem center)
   │  └─ OrbRow (HorizontalLayoutGroup, 4 orbs)
   │     ├─ Orb_1 (Button, SELECTED) → Globe + Sigil(golden pickaxe) + PriceChip(gem "150")
   │     ├─ Orb_2 (Button) → Globe + Sigil(red flaming sword) + PriceChip(gem "200")
   │     ├─ Orb_3 (Button) → Globe + Sigil(blue tridents/spears) + PriceChip(gem "250")
   │     └─ Orb_4 (Button) → Globe + Sigil(gold shield) + PriceChip(gem "300")
   └─ DetailScroll (Parchment panel, right)
      ├─ Scroll_Frame (Image, gold-capped rolled parchment)
      ├─ Detail_Title (Text "MINER'S BLESSING")
      ├─ Detail_Icon  (Image, golden pickaxe)
      ├─ Detail_Desc  (Text "Increases mining speed significantly for a limited time.")
      ├─ Detail_Stats (Text "Duration: 30 seconds\nEffect: 2x Mining Speed")
      ├─ Detail_Flavor (Text italic "The earth yields its riches to those blessed by magic.")
      └─ BuyButton (Button, violet) → Label "BUY" + PriceRow(gem icon + "150")
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **Shared chrome** — identical anchors/components to `17 §D` (Back top-left 0,1; TabBar top-center 0.5,1;
  CurrencyChips top-right 1,1). Only the **selected** toggle differs (Tab_Spells). Currency values 1726 / 48570.
- **MagePresenter** — parent SafeAreaRoot; Image (may be 2 layers: body + additive staff glow). Anchor
  **bottom-left** (0,0) pivot 0,0; rect x 0.0→0.30 W, y 0.10→0.95 H. Drawn as foreground character; NOT a
  button. Staff_Glow child = additive cyan sprite, slow pulse.
- **OrbTable** — parent SafeAreaRoot; anchor **bottom-center** (0.5,0) pivot 0.5,0; rect x 0.18→0.66 W,
  y 0.0→0.55 H. Table_Cloth Image behind; **OrbRow** = `HorizontalLayoutGroup` (4 equal, spacing≈18px@1080,
  childAlignment=MiddleCenter, childForceExpandWidth=true) anchored to the cloth top region.
- **Orb_N** — Button; vertical mini-group: [Globe Image with inner Sigil Image] over [PriceChip
  (HLG: gem icon + count)]. Globe ~0.10 W diameter. Selected orb gets a brighter rim-glow + slight scale 1.06.
- **DetailScroll** — parent SafeAreaRoot; anchor **right** (1,1) pivot 1,1; rect x 0.665→0.985 W,
  y 0.085→0.93 H (tall parchment). Internal `VerticalLayoutGroup` (padding ~24, spacing ~10,
  childControlHeight, childAlignment UpperCenter): Title → Icon (left-float or centered) → Desc → Stats →
  Flavor → spacer → BuyButton (pinned near bottom).
- **BuyButton** — large violet pill, gold-edged; child stack: "BUY" (line 1, big) over PriceRow (gem icon +
  "150"). Anchored bottom of scroll, width ~0.78 of scroll inner.

**Responsive:** chrome to corners (stable). Mage anchored bottom-left, orbs bottom-center, scroll right — on
ultrawide they spread with the revealed bg; on narrower aspect the mage may clip behind the orb table (keep
orbs + scroll always fully in safe area; mage is decorative and may bleed). Scroll keeps fixed fraction; text
auto-sizes within.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Chrome:** identical to 17 §E (top band y 0→0.085; Back 0.045 W sq; TabBar 0.44 W centered; currencies
  right ~0.27 W). Gem chip "1726", gold chip "48570".
- **Mage presenter:** x 0.0→0.30 (Δ0.30 W ≈702px) · y 0.10→0.95. Staff orb focal point ≈ (0.20 W, 0.30 H).
- **Orb table:** x 0.18→0.66 (Δ0.48 W ≈1123px) · cloth top ≈ y 0.42, table base to bottom.
  - OrbRow band: y 0.40→0.70; 4 orbs each ≈0.10 W globe, gaps ≈0.018 W; price chip directly below each globe
    at y ≈0.70→0.76.
  - Crown emblem centered on cloth at ≈(0.42 W, 0.86 H).
- **Detail scroll:** x 0.665→0.985 (Δ0.32 W ≈749px) · y 0.085→0.93 (Δ0.845 H ≈913px).
  - Title y ≈0.135; Icon y ≈0.22; Desc y ≈0.33; Stats y ≈0.47; Flavor y ≈0.62; BuyButton y ≈0.78→0.89
    (violet pill ≈0.24 W × 0.075 H, centered in scroll).
- **Notch/tablet/ultrawide:** SafeAreaFitter insets interactive layer; orbs+scroll guaranteed inside; mage
  bg-character may bleed. On 4:3, scroll narrows slightly and orb globes shrink to keep 4-up.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref. Serif = Trajan/Cinzel display; body = light serif or semi-condensed sans.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tab labels / currencies / Back | (shared, see 17 §F) | — | — | — | — | — | — |
| Spell price (orbs, "150"…"300") | Sans | Bold | — | 0 | 28 | white, dark stroke; violet gem icon left | `#ffffff` |
| Detail title "MINER'S BLESSING" | Serif display | Bold | UPPER | +3% | 34 | gold bevel + soft glow, dark-brown stroke (on parchment) | `#5a3a12`→`#8a5a1e` warm |
| Detail description | Light serif | Regular | Title | 0 | 22 | dark ink on parchment, slight shadow | `#3a2c14` ink-brown |
| Detail stats (Duration/Effect) | Light serif | Semibold | Title | 0 | 21 | ink-brown, label+value | `#3a2c14` |
| Detail flavor (italic) | Serif italic | Regular | Title | +1% | 20 | muted ink, centered, italic | `#5a4a2c` faded |
| BUY (button) | Serif display | Black | UPPER | +4% | 36 | gold bevel text on violet, glow + dark outline | `#f6e6b8` on violet |
| BUY price ("150" + gem) | Sans | Bold | — | 0 | 24 | light, gem icon left | `#f3ecff` on violet |

---

## G · MATERIALS
- **Gold frames / chrome:** as 17 §G.
- **Crystal spell orbs (globes):** cool blue-white glass `#cfe6ff` rim → translucent `#5a86c8` body, strong
  specular highlights, refraction; each holds a glowing colored **sigil**: Orb_1 golden pickaxe (`#f0c050`
  warm glow), Orb_2 red flaming sword (`#ff5a2a` ember), Orb_3 blue tridents/spears (`#4fa8ff` cyan), Orb_4
  gold shield (`#e8c25a`). Orbs sit on small **gold ring stands**. Additive inner glow per sigil + outer
  bloom; selected orb brighter.
- **Mage:** hooded robe deep violet/indigo `#2a1d4a`→`#4a3a7a` with gold trim; glowing **cyan eyes**
  `#5fe0ff`; staff topped by a cyan magical orb `#3fd0ff`→`#cfeeff` (brightest single light).
- **Table cloth:** royal-blue velvet `#1c2f7a`→`#2c47b0` with gold-embroidered **crown** emblem `#e8c25a`,
  tassel/fringe trim; soft sheen.
- **Parchment scroll:** aged cream `#d9c79a` center → `#c2ac74` edges, fiber texture, slight curl + drop
  shadow; **gold roller caps** top & bottom (`#caa04a`→`#f0d27a` with finial knobs); ink-brown text.
- **BUY button:** amethyst pill `#6a2db8`→`#9e6bf0` with bright top bevel + gold edge line + outer glow;
  pressed darkens; the most saturated element after the orbs.
- **Detail icon (pickaxe):** matches Orb_1 sigil — gold-headed pickaxe with warm glow.

---

## H · COMPONENTS (states + feedback)
**Shop tabs / currency chips / Back** — identical states to 17 §H. Here **Tab_Spells = selected** (gold-lit
frame + glow + bright label + full-color orb icon).

**Spell Orb (Button)**
- *idle:* glass globe + sigil + price chip; gentle float bob + sigil glow pulse.
- *hover:* orb rim brightens, scale 1.04, glow +15%.
- *pressed:* scale 0.97, brief flash.
- *selected:* scale 1.06, brightest rim-glow, the parchment scroll updates to this spell; a subtle gold
  underline ring on the stand. Only one selected at a time (acts like a radio within OrbRow).
- *affordable vs not:* if gems < price, price chip text tints red on attempted buy (orb still selectable to
  read).
- *owned/active (if consumable already active):* could show a small "ACTIVE" tag — not shown in mock; omit
  unless data says so.

**BUY button**
- *idle:* violet pill, "BUY" + price.
- *hover:* +8% glow, slight scale 1.03.
- *pressed:* 0.97 scale, darken.
- *disabled (can't afford):* desaturate to grey-violet 50%, "+" suggestion → tapping opens insufficient modal.
- *purchasing:* label → spinner; *success:* gem count flies from GemChip down (or gem-spend animation),
  scroll flashes gold, success toast/RewardGrant; *fail:* red shake + insufficient modal.

---

## I · ANIMATION TIMELINE
- **OnShow (0→0.5s):** Bg fade (0.25) → chrome slide-down (0–0.25) → Mage slide-in from left −24px + fade
  (0.1–0.4) → OrbRow orbs stagger pop 0.9→1 with glow ignite, 50ms stagger (0.15–0.45) → DetailScroll
  unrolls/scales 0.95→1 + fade (0.2–0.45).
- **Idle loops:** each orb float-bob (±6px, 2.5–3.2s, staggered phase) + sigil glow breathe (2s); mage staff
  orb cyan pulse (1.8s) + faint floating motes; parchment edge subtle sway.
- **Orb select (0.18s):** previous orb scale→1.0 dim, new orb scale→1.06 brighten; DetailScroll content
  cross-fades (out 0.1 / in 0.15) + tiny re-unroll bounce; BUY price updates.
- **Buy success:** gem-spend sparkle at GemChip (count-down) + scroll gold flash (0.3) + RewardGrant/toast.
- **Buy fail:** BUY + price red shake (0.3, 3 cycles) → insufficient modal.

Easing: floats sine in-out; UI moves ease-out 0.12–0.25; ignites ease-out 0.3.

---

## J · PARTICLE & FX
- Per-orb: additive sigil glow + slow swirling inner particles in the orb's color; occasional spark twinkle on
  the glass specular.
- Mage staff: cyan magical wisps / floating motes around the orb (additive).
- Selected orb: a faint rising sparkle column.
- BUY hover: small violet sparkle along the pill edge.
- Buy success: gold+violet burst from the scroll; gem icon dissolve at GemChip.
- Global: vignette + bloom; warm dust motes drifting in the study light.

---

## K · EVENT BEHAVIOR
- **OnShow:** bind catalog of spells (id, name, desc, duration, effect, flavor, gemPrice — server-auth);
  default-select Orb_1 (Miner's Blessing); bind currencies.
- **OnSelectOrb(spell):** update DetailScroll (title/icon/desc/stats/flavor) + BUY price; highlight orb.
- **OnBuy(spell):** server/wallet purchase (client never mutates balance, §12); on success grant + count-down
  gems + toast; on insufficient → modal (37).
- **OnTabSelect:** swap to Skins/Chests/Store (chrome persists). **OnBack:** UiRouter pop.
- **OnAddGem/Gold "+":** route to Store gems/resources.

---

## L · NEGATIVE RULES
- Forensic copy is binding: **MINER'S BLESSING** · "Increases mining speed significantly for a limited time."
  · "Duration: 30 seconds" · "Effect: 2x Mining Speed" · flavor "The earth yields its riches to those blessed
  by magic." · **BUY / 150**. Orb prices **150 / 200 / 250 / 300** gems; sigils pickaxe / flaming-sword /
  tridents / shield. Currencies **1726 / 48570**.
- Keep exactly **four** orbs; do not add/remove. Keep BUY violet (premium spend), not blue.
- Do NOT move chrome (Back TL, tabs TC, currencies TR). Bg full-bleed under cutout; orbs+scroll in safe area.
- **§12 / server-auth:** UI never mutates a balance; purchase via server. **No gameplay/ECS/balance change.**
- **ADR flag (do not alter spec):** premium-gem spell buffs may collide with "gems never buy power" → economy
  ADR. Spec'd as drawn.
- Invent nothing (no extra spell stats, no fabricated cooldowns beyond the drawn "Duration/Effect").

## M · ACCEPTANCE CRITERIA (≥95%)
1. Shared chrome correct; **SPELLS** tab selected (gold-lit); Gems 1726 / Gold 48570 with "+".
2. Hooded mage presenter bottom-left with glowing cyan staff orb.
3. Four crystal orbs on the blue crowned table, correct sigils + prices (150/200/250/300) below each.
4. Parchment scroll on right with MINER'S BLESSING title, pickaxe icon, exact description/stats/flavor text.
5. Violet gold-edged **BUY** button with "150" + gem icon — brightest CTA.
6. Palette: cyan magic + amethyst gems + cream parchment + gold frames; vignette/bloom study mood.
7. Fraction layout within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet.
8. Orb select updates scroll + BUY; buy/insufficient feedback per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**93/100.** All copy/prices/icons legible; chrome inherited. Minus: (a) crystal-orb refraction + per-sigil
glow is a custom shader/sprite stack (approximate with layered globe sprite + additive sigil + glow);
(b) mage character + study bg are bespoke art (use provided sprites); (c) parchment unroll is a stylistic
flourish (optional). No interactive-fidelity risk.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (topbar, orbs, orb1 pickaxe, mage, parchment) before writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree incl. shared tab bar + currency chips + back.
- [x] Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Forensic copy/prices/colors recorded; nothing invented.
- [x] §12/server-auth + ADR flag noted; spec not altered by the flag.
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 19 · Skins

Source: design/SkinsScreenDesign.png · 1910×823 (2.32:1) · Analysis-only forensic spec.

> **Inherits the shared shop chrome from `17_Store_SPEC.md`** (Back top-left, tab bar SPELLS·SKINS·CHESTS·
> STORE top-center, Gems+Gold chips top-right). This file details the **Skins-specific** body: a left vertical
> **skin-thumbnail rail**, a center **hero preview** of the equipped/selected skin, a bottom **set-pieces row**
> (themed pickaxes with gem prices), and a right **detail panel** listing **stat bonuses** with an **EQUIP
> ALL** button. **SKINS** tab selected. **ADR-flagged** (skins carry gameplay stat modifiers).

---

## A · SCREEN PURPOSE
The **Skins** shop tab is the cosmetic-set collection. The player picks a skin theme from a vertical rail (5
entries), sees a full-body hero preview center-stage, reviews the **set bonuses** in the right panel, browses
the matching **set pieces** (e.g., tiered pickaxes) along the bottom — each with a gem price — and applies the
whole look with **EQUIP ALL**. Shown selected: **LEAF SET** (a nature/green-leaf themed character), with the
green Leaf pickaxe piece selected (300 gems, checkmarked).

**ADR note (Section L):** the right panel shows **gameplay stat modifiers** (+30% Build Speed, +20% Extra
Health, +25% Mining Speed, +15% Unit Regen). This collides with "skins are visual-only cosmetics" + "gems
never buy power" → **requires an ADR** (recommended resolution: make modifiers Gold-only / cosmetic-only or
standardized in ranked). The screen is spec'd **exactly as drawn**; the ADR governs implementation, not this
visual spec.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** a darkened armory / war-camp at dusk — torch-lit posts, a distant battlefield horizon in the
  detail panel art. Moodier and darker than Store/Spells; the glowing **green Leaf hero** is the focal light.
- **Palette anchors:** for LEAF SET, emerald/lime green (`#3fd24a`–`#9bff7a`) on the character + bonus icons;
  gem-violet price chips; gold ornate frames; **EQUIP ALL is cobalt/royal-blue** (Iron-Pact CTA blue),
  gold-edged — distinct from the violet BUY of the Spells tab (equip = apply, not premium-spend).
- **Lighting:** strong rim + inner glow on the selected hero (green), torch flicker on the bg posts, vignette,
  bloom on the glowing skin and the selected piece's frame.
- **Background:** full-bleed dusk armory/battlefield; bleeds under cutout.
- **Hierarchy:** detail bonuses + EQUIP ALL (right, the decision) ⟷ hero preview (center focal) → set-pieces
  row (choices) → thumbnail rail (set switch) → chrome.

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
SkinsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, dusk armory/battlefield)  [OUTSIDE safe area]
   │  └─ Bg_Vignette (Image)
   ├─ TopChrome  ── [SHARED — see 17 §C/§D]
   │  ├─ BackButton (Button: Plate + gold Arrow)
   │  ├─ TabBar (ToggleGroup ShopTabs)
   │  │  ├─ Tab_Spells (Toggle) → Icon + "SPELLS"
   │  │  ├─ Tab_Skins  (Toggle, SELECTED, gold-lit) → Icon(helm) + "SKINS"
   │  │  ├─ Tab_Chests (Toggle) → Icon + "CHESTS"
   │  │  └─ Tab_Store  (Toggle) → Icon + "STORE"
   │  └─ CurrencyChips
   │     ├─ GemChip  → violet gem + "1726" + green "+"
   │     └─ GoldChip → silver coin + "48570" + green "+"
   ├─ SkinRail (left, gold-framed container + VerticalLayoutGroup or ScrollRect, 5 thumbs)
   │  ├─ Rail_Frame (Image, ornate gold border container)
   │  ├─ Skin_Thumb_1 (Toggle, SELECTED, gold-lit) → portrait: green-leaf hooded figure
   │  ├─ Skin_Thumb_2 (Toggle) → red/orange knight-helm figure
   │  ├─ Skin_Thumb_3 (Toggle) → dark green hooded figure
   │  ├─ Skin_Thumb_4 (Toggle) → purple wizard-hat figure
   │  └─ Skin_Thumb_5 (Toggle) → white skull/bone figure
   ├─ HeroPreview (center stage)
   │  ├─ Hero_Pedestal (Image, faint ground/light disc)
   │  └─ Hero_Model (Image, full-body LEAF SET character, glowing green, holding leaf-blades)
   ├─ SetPieces_Row (bottom, gold-framed container + HorizontalLayoutGroup, 4 pieces)
   │  ├─ Piece_1 (Button) → stone/plain pickaxe + PriceChip(gem "100")
   │  ├─ Piece_2 (Button, SELECTED, green frame + ✓) → green Leaf pickaxe + PriceChip(gem "300")
   │  ├─ Piece_3 (Button) → blue/ice pickaxe + PriceChip(gem "600")
   │  └─ Piece_4 (Button) → orange/fire pickaxe + PriceChip(gem "900")
   └─ DetailPanel (right)
      ├─ Detail_Frame (Image, dark panel + gold edge, battlefield bg inside)
      ├─ Detail_Title (Text "LEAF SET")
      ├─ Detail_Bonuses (VerticalLayoutGroup, 4 rows: icon + text)
      │  ├─ "+ 30% Build Speed"   (leaf/gold icon)
      │  ├─ "+ 20% Extra Health"  (green heart icon)
      │  ├─ "+ 25% Mining Speed"  (pickaxe icon)
      │  └─ "+ 15% Unit Regen"    (sword/plus icon)
      ├─ Detail_Flavor (Text "Harness the power of nature.\nGrow. Protect. Dominate.")
      └─ EquipAllButton (Button, cobalt-blue) → Label "EQUIP ALL"
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **Shared chrome** — identical to `17 §D`; **Tab_Skins selected** (gold-lit). Currencies 1726 / 48570.
- **SkinRail** — parent SafeAreaRoot; anchor **left** (0,1) pivot 0,1; rect x 0.030→0.115 W, y 0.10→0.95 H
  (tall narrow column beside Back). Rail_Frame Image (ornate gold container) + inner `VerticalLayoutGroup`
  (spacing≈10px, 5 equal cells) — wrap in a `ScrollRect` (vertical) if the roster can exceed 5. Each cell =
  `Toggle` (ToggleGroup `SkinList`) with a square portrait Image; selected = gold-lit frame + glow.
- **HeroPreview** — parent SafeAreaRoot; anchor **center** (0.5,0.5) biased toward center-left of the open
  area; rect ≈ x 0.27→0.62 W, y 0.10→0.92 H. Hero_Pedestal behind (faint disc), Hero_Model foreground (full
  body). Decorative (not a button); selection happens via rail/pieces.
- **SetPieces_Row** — parent SafeAreaRoot; anchor **bottom-center** (0.5,0) pivot 0.5,0; rect x 0.255→0.665 W,
  y 0.02→0.27 H. Container Image (gold-framed dark bar) + `HorizontalLayoutGroup` (4 equal, spacing≈16px,
  childAlignment MiddleCenter, childForceExpandWidth=true). Each Piece = Button = vertical mini-group [Art
  Image over PriceChip(gem+count)]. Selected piece = green highlight frame + ✓ badge (top-right).
- **DetailPanel** — parent SafeAreaRoot; anchor **right** (1,1) pivot 1,1; rect x 0.745→0.985 W,
  y 0.10→0.93 H. Detail_Frame (dark panel, gold edge, faint battlefield art inside). Internal
  `VerticalLayoutGroup` (padding ~24, spacing ~12): Title → Bonuses(sub-VLG of 4 icon+text rows) → Flavor →
  spacer → EquipAllButton (pinned bottom).
- **Detail_Bonuses rows** — each = `HorizontalLayoutGroup` [Icon 26px][Text left-aligned]. Icons themed
  (leaf, heart, pickaxe, sword).
- **EquipAllButton** — cobalt-blue pill, gold-edged; width ~0.80 of panel inner; anchored bottom.

**Responsive:** chrome to corners. Rail anchored left edge, panel right edge → stable; hero centers in the
gap. On ultrawide the gap widens (hero stays centered, bg reveals more camp). On 4:3 tablet, rail/panel narrow
slightly; set-pieces stay 4-up (GridLayoutGroup fallback 4 cols → 2×2 below ~1.7:1). ScrollRect handles >5
skins.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Chrome:** as 17 §E (Back 0.045 W sq TL; TabBar 0.44 W centered TC; currencies right ~0.27 W TR).
- **Skin rail:** x 0.030→0.115 (Δ0.085 W ≈199px) · y 0.10→0.95 (Δ0.85 H ≈918px). 5 cells each ≈0.155 H tall
  ≈ square portraits ~0.075 W; gaps ≈0.012 H. Selected (top) gold-lit.
- **Hero preview:** x 0.27→0.62 (Δ0.35 W ≈819px) · y 0.10→0.92. Focal mass centered ≈ (0.44 W, 0.50 H);
  pedestal disc at y ≈0.80.
- **Set-pieces row:** x 0.255→0.665 (Δ0.41 W ≈959px) · y 0.02→0.27 (Δ0.25 H ≈270px). 4 pieces each ≈0.092 W,
  gaps ≈0.015 W; price chip below each at y ≈0.04→0.10. Selected (Piece_2) green frame + ✓.
- **Detail panel:** x 0.745→0.985 (Δ0.24 W ≈562px) · y 0.10→0.93 (Δ0.83 H ≈896px).
  - Title y ≈0.155; 4 bonus rows y 0.24→0.50 (each ≈0.065 H); Flavor y ≈0.58 (2 lines); EQUIP ALL y
    0.80→0.90 (blue pill ≈0.20 W × 0.075 H).
- **Notch/tablet/ultrawide:** SafeAreaFitter insets interactive layer; rail + panel + pieces always inside;
  hero may bleed slightly. Bg full-bleed under cutout.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tabs / currencies / Back | (shared, 17 §F) | — | — | — | — | — | — |
| Detail title "LEAF SET" | Serif display | Bold | UPPER | +4% | 38 | gold bevel + glow, dark stroke | `#9bff7a`→`#e8d27a` (green-gold) |
| Bonus rows ("+ 30% Build Speed"…) | Sans / light serif | Semibold | Title | +1% | 22 | light text, dark shadow; "+nn%" emphasized | `#eaf3df` pale-green-white |
| Bonus % value emphasis | Sans | Bold | — | 0 | 23 | brighter | `#bff7a0` lime |
| Detail flavor (2 lines) | Serif italic | Regular | Title | +1% | 20 | muted, centered | `#cdbf9a` faded gold |
| Set-piece prices (100/300/600/900) | Sans | Bold | — | 0 | 26 | white, dark stroke; violet gem icon left | `#ffffff` |
| EQUIP ALL (button) | Serif display | Black | UPPER | +5% | 32 | gold bevel text on cobalt, glow + outline | `#f3e6c0` on blue |
| Selected ✓ badge | (icon) | — | — | — | — | green check on gold disc | `#3fd24a` |

---

## G · MATERIALS
- **Gold frames / chrome:** as 17 §G (rail container, set-pieces bar, detail-panel edge all share it).
- **Skin rail portraits:** dark inset thumbnails, ornate gold mini-frames; selected = gold-lit frame +
  warm glow halo; unselected = dim. Portraits are small character busts (per theme color).
- **LEAF SET hero:** dark silhouette body wrapped in **glowing emerald foliage** `#2faa3a`→`#9bff7a`,
  leaf-blade weapons, glowing white-green eyes; strong inner glow + rim light + soft bloom; stands on a faint
  light disc.
- **Set-piece pickaxes:** themed materials —
  - Piece_1 plain: weathered steel `#8a8f98` + wood haft `#5a4326`, low shine.
  - Piece_2 Leaf (selected): living green `#3fd24a`→`#9bff7a`, glowing, organic — green highlight frame + ✓.
  - Piece_3 ice/blue: frosted blue `#6fb6ff`→`#cfeaff`, crystalline, cold specular.
  - Piece_4 fire/orange: molten `#ff7a2a`→`#ffd07a`, ember glow, smoking edges.
- **Price chips:** dark plate + violet gem icon + white count (like Spells orbs).
- **Detail panel:** dark glass panel `#0e1118`/85% with gold edge; a faint **battlefield-at-dusk** image
  inside (desaturated, ember horizon); bonus icons small gold/colored glyphs (leaf, green heart, pickaxe,
  sword-plus).
- **EQUIP ALL button:** royal/cobalt blue `#1f3fb0`→`#3a63d0` with bright top bevel + gold edge line + outer
  glow; pressed darkens. (Distinct from Spells' violet BUY.)

---

## H · COMPONENTS (states + feedback)
**Shop tabs / currency / Back** — as 17 §H; **Tab_Skins selected** (gold-lit).

**Skin rail thumb (Toggle, ToggleGroup SkinList)**
- *idle:* dim portrait in gold mini-frame.
- *hover:* +8% brightness, faint glow.
- *pressed:* 0.96 scale.
- *selected:* gold-lit frame + glow halo; hero preview + detail panel + set-pieces switch to this set.
- *locked/unowned:* could show a lock overlay (not visible in mock; include only if data says locked).

**Set-piece (Button)**
- *idle:* art + price chip in dark cell.
- *hover:* lift −5px, art bloom +10%.
- *pressed:* 0.97 scale, price chip flash.
- *selected (equipped piece):* green highlight frame + ✓ badge (Piece_2) — denotes the currently equipped
  piece of that slot.
- *owned vs purchasable:* owned → ✓ / "EQUIP"; not owned → gem price acts as buy. (Mock shows prices on all;
  treat price = buy-then-equip; ✓ = owned+equipped.)
- *can't afford:* price tints red on buy attempt → insufficient modal.

**EQUIP ALL button**
- *idle:* cobalt pill "EQUIP ALL".
- *hover:* +8% glow, scale 1.03.
- *pressed:* 0.97 darken.
- *already fully equipped:* label → "EQUIPPED" + disabled (45%).
- *partial-owned set:* tapping equips owned pieces + prompts purchase for missing (or disabled until owned —
  governed by ADR/data); on success → green flash + "EQUIPPED" + hero updates.

---

## I · ANIMATION TIMELINE
- **OnShow (0→0.5s):** Bg fade (0.25) → chrome slide-down (0–0.25) → SkinRail slides in from left −20px +
  fade (0.1–0.4) → HeroPreview fades + scales 0.96→1 with a green glow ignite (0.15–0.45) → SetPieces stagger
  pop left→right 40ms (0.2–0.45) → DetailPanel slides from right +24px (0.2–0.45).
- **Idle loops:** hero green glow breathe (2s) + slow foliage shimmer + idle sway (subtle ±2°); torch flicker
  on bg posts; selected rail-frame + piece-frame gentle glow pulse (2.5s).
- **Skin switch (0.25s):** old hero fade/scale-down → new hero fade/scale-up + glow ignite; rail glow + detail
  panel + set-pieces cross-fade (out 0.12 / in 0.15); EQUIP ALL state recomputes.
- **Piece select (0.15s):** previous green frame fades, new piece green frame + ✓ pops (scale 0.8→1.1→1.0);
  hero's matching slot art swaps.
- **Equip all success:** hero flash gold-green + sparkle ring + "EQUIPPED"; pieces all show ✓.
- **Buy/insufficient:** price chip red shake (0.3) → insufficient modal.

Easing: hero swaps ease-in-out 0.25; pops ease-out back 0.2; glows sine.

---

## J · PARTICLE & FX
- LEAF hero: drifting glowing leaf/spore particles (green additive) + rim shimmer; pedestal soft light pool.
- Selected piece: small green sparkle on the ✓ badge.
- Bg: torch ember particles on the camp posts.
- Equip-all success: green-gold burst from hero + ring sweep.
- Global: vignette + bloom; dusk haze.

---

## K · EVENT BEHAVIOR
- **OnShow:** bind skin roster (id, name, bonuses[], flavor, pieces[], owned/equipped flags, gemPrices —
  server-auth); default-select LEAF SET; bind currencies.
- **OnSelectSkin(set):** update hero preview + detail (title/bonuses/flavor) + set-pieces row + EQUIP ALL
  state.
- **OnSelectPiece(piece):** equip/highlight that piece (✓); if unowned, treat price as buy → server purchase.
- **OnEquipAll(set):** apply set (server-auth equipped state); update hero + ✓s; success flash.
- **OnBuyPiece / OnInsufficient:** server purchase; insufficient → modal (37).
- **OnTabSelect / OnBack / OnAdd "+":** as 17 (swap tab / pop / route to store).

---

## L · NEGATIVE RULES
- Forensic copy binding: **LEAF SET**; bonuses **+30% Build Speed, +20% Extra Health, +25% Mining Speed,
  +15% Unit Regen**; flavor "Harness the power of nature. Grow. Protect. Dominate."; **EQUIP ALL**; piece
  prices **100 / 300 / 600 / 900** gems (Piece_2 green ✓ selected); currencies **1726 / 48570**; rail = **5**
  skins.
- Keep EQUIP ALL **cobalt-blue** (apply/equip), NOT violet (that's Spells' premium BUY).
- Do NOT move chrome (Back TL, tabs TC, currencies TR). Bg full-bleed under cutout; rail/hero/pieces/panel in
  safe area.
- **§12 / server-auth:** equipped/owned state and balances are server-authoritative; UI never mutates them.
  **No gameplay/ECS/balance change.**
- **ADR flag (do not alter spec):** the stat-modifier bonuses collide with visual-only-cosmetic +
  "gems-never-buy-power" canon → ADR required. Record the modifiers **exactly as drawn**; the ADR governs
  whether they ship as Gold-only/cosmetic/ranked-standardized. Do NOT remove or "fix" the bonuses in this
  forensic spec.
- Invent nothing (no extra pieces, no fabricated rarities/percentages).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Shared chrome correct; **SKINS** tab selected; Gems 1726 / Gold 48570 with "+".
2. Left rail: 5 gold-framed skin portraits, top (LEAF) gold-lit/selected.
3. Center: glowing green LEAF SET full-body hero on a light pedestal.
4. Bottom: 4 themed pickaxe pieces with prices 100/300/600/900; green Leaf piece selected (frame + ✓).
5. Right panel: "LEAF SET" + exactly the 4 bonuses with icons + flavor + cobalt **EQUIP ALL**.
6. Palette: emerald hero + violet price chips + gold frames + dusk-armory bg, vignette/bloom.
7. Fraction layout within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet; rail scrolls if >5.
8. Skin/piece selection + equip-all + insufficient feedback per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**92/100.** Copy/bonuses/prices/icons legible; chrome inherited. Minus: (a) glowing-foliage hero + themed
pickaxe renders are bespoke art (use sprites); (b) per-skin portrait set is custom; (c) exact bonus-icon
glyphs approximated; (d) ADR may change whether bonuses are shown as power — but the **visual** spec is firm.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (topbar, thumb rail, hero, set-pieces, detail panel) before writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree incl. shared tab bar + currency chips + back.
- [x] Fraction layout normalized to 2340×1080; tablet/ultrawide/notch + ScrollRect covered.
- [x] Forensic copy/bonuses/prices/colors recorded; nothing invented.
- [x] §12/server-auth + **ADR stat-modifier flag** noted in A/L; spec NOT altered by the flag.
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 20 · Chests

Source: design/ChestsScreenDesign.png · 1914×822 (2.33:1) · Analysis-only forensic spec.

> **Inherits the shared shop chrome from `17_Store_SPEC.md`** (Back top-left, tab bar SPELLS·SKINS·CHESTS·
> STORE top-center, Gems+Gold chips top-right). This file details the **Chests-specific** body: a hooded
> **Keeper** presenter, a central **featured chest** with magical aura on a pedestal, a bottom **chest-slot
> row** with **unlock timers / locks**, and a right **OPEN CHEST** button. **CHESTS** tab selected.
> **ADR-flagged** (loot-box / gacha — see also 21 Chest Open Result).

---

## A · SCREEN PURPOSE
The **Chests** shop tab is the **loot-chest** surface. The player has a row of **chest slots** (earned/found
chests) that **unlock on timers** (or instantly via gems); a **featured chest** sits center-stage glowing,
guarded by a hooded **Keeper**; tapping **OPEN CHEST** (after unlock, or by spending gems to skip) plays the
reveal → routes to **Chest Open Result (21)**. Shown: slot 1 active with a **02:59** timer, slots 2–3 locked,
slot 4 empty; the featured gold chest with amethyst gem glows on its pedestal.

**ADR note (Section L):** timed chests + randomized rewards = a **loot-box / gacha** loop, which collides with
the "no loot boxes/gacha" principled cut → **requires an ADR** (transparent odds + a fair/redesigned model, or
a CUT). Spec'd **exactly as drawn**; ADR governs implementation, not this visual spec.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** a vaulted treasure crypt lit by **violet/amethyst magic** — gold-coin drifts in the dark side
  alcoves, arched stone pillars, a beam of purple light on the focal chest. The most overtly "magical/premium"
  shop tab (violet aura everywhere).
- **Palette anchors:** amethyst/violet aura + light beams (`#7a3fd0`–`#b06bff`); gold chest + gold frames;
  cobalt-blue accent on the active slot's chest gem; **OPEN CHEST button is violet/amethyst**, gold-edged,
  with a **padlock-and-key** icon — the brightest CTA.
- **Lighting:** a focal purple god-beam down onto the featured chest; rim-glow on the chest's gold; the Keeper
  is a dark silhouette with glowing eyes and outstretched arms; heavy vignette; strong bloom on aura + gem.
- **Background:** full-bleed crypt/vault (pillars, gold hoards in alcoves); bleeds under cutout.
- **Hierarchy:** featured chest + aura (center focal) → OPEN CHEST (right CTA) → chest-slot row + timers
  (bottom, the inventory) → Keeper (atmosphere) → chrome.

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
ChestsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, amethyst-lit treasure crypt)  [OUTSIDE safe area]
   │  └─ Bg_Vignette (Image)
   ├─ TopChrome  ── [SHARED — see 17 §C/§D]
   │  ├─ BackButton (Button: Plate + gold Arrow)   [note: this screen's arrow is a thinner "<" variant]
   │  ├─ TabBar (ToggleGroup ShopTabs)
   │  │  ├─ Tab_Spells (Toggle) → Icon + "SPELLS"
   │  │  ├─ Tab_Skins  (Toggle) → Icon + "SKINS"
   │  │  ├─ Tab_Chests (Toggle, SELECTED, gold-lit) → Icon(chest) + "CHESTS"
   │  │  └─ Tab_Store  (Toggle) → Icon + "STORE"
   │  └─ CurrencyChips
   │     ├─ GemChip  → violet gem + "1726" + green "+"
   │     └─ GoldChip → silver coin + "48570" + green "+"
   ├─ Keeper (Image, hooded figure, arms outstretched, glowing eyes, behind chest)
   ├─ FeaturedChest (group, center)
   │  ├─ Chest_AuraBeam (Image, vertical violet god-beam, additive)  [behind chest]
   │  ├─ Chest_Pedestal (Image, stone dais with gold/violet rune trim)
   │  ├─ Chest_Model (Image, ornate gold chest w/ amethyst gem on front)
   │  └─ Chest_AuraSwirl (Image, orbiting violet sparks/glow ring)  [in front/around]
   ├─ ChestSlots_Row (bottom-left, HorizontalLayoutGroup, 4 slots)
   │  ├─ Slot_1 (Button, ACTIVE, gold-lit frame) → blue chest + gem; TimerBar(green "02:59")
   │  ├─ Slot_2 (locked) → wood chest + padlock icon
   │  ├─ Slot_3 (locked) → wood chest + padlock icon
   │  └─ Slot_4 (empty, dark recessed slot)
   └─ OpenChestButton (Button, violet, bottom-right) → LockKeyIcon + Label "OPEN CHEST"
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **Shared chrome** — identical to `17 §D`; **Tab_Chests selected** (gold-lit). Currencies 1726 / 48570.
  (Back arrow on this mock is a thinner chevron; treat as the same Back button, art variant only.)
- **Keeper** — parent SafeAreaRoot; Image (dark silhouette w/ additive glowing eyes + faint hand-glow).
  Anchor **top-center** biased behind chest; rect ≈ x 0.36→0.64 W, y 0.10→0.62 H. Decorative (not a button);
  sits BEHIND FeaturedChest in draw order, IN FRONT of Bg.
- **FeaturedChest** — parent SafeAreaRoot; anchor **center** (0.5,0.5); rect ≈ x 0.33→0.67 W, y 0.12→0.78 H.
  Draw order back→front: Chest_AuraBeam (additive vertical beam) → Chest_Pedestal → Chest_Model →
  Chest_AuraSwirl (additive ring/sparks). May be a Button (tap featured to open) or purely visual with the
  OPEN CHEST button as the actuator — implement featured chest tap = same as OPEN CHEST.
- **ChestSlots_Row** — parent SafeAreaRoot; anchor **bottom-left** (0,0) pivot 0,0; rect x 0.020→0.30 W,
  y 0.02→0.27 H. Container + `HorizontalLayoutGroup` (4 equal cells, spacing≈10px, childAlignment
  MiddleCenter). Each Slot = framed cell:
  - **Slot_1 (active):** Button; gold-lit frame; chest art (silver/blue chest w/ blue diamond gem); a
    **TimerBar** at the bottom (green fill + white "02:59" countdown). Tapping = ready-progress info or
    gem-skip.
  - **Slot_2 / Slot_3 (locked):** dim wood-chest art + centered **padlock** icon overlay; non-interactive
    until earned/unlocked.
  - **Slot_4 (empty):** dark recessed empty slot (no chest).
- **OpenChestButton** — parent SafeAreaRoot; anchor **bottom-right** (1,0) pivot 1,0; rect x 0.71→0.985 W,
  y 0.04→0.20 H. Violet pill, gold-edged ornate frame; left **lock-and-key** icon (diamond gold badge) + label
  "OPEN CHEST". The primary CTA.

**Responsive:** chrome to corners. Featured chest centers; slot row bottom-left; CTA bottom-right → stable.
On ultrawide the crypt bg reveals more alcoves; chest stays centered. On 4:3 tablet, slot row + CTA may need a
slightly larger bottom inset; keep both fully in safe area (do not let the CTA overlap slot 4). Slot row can
become a `ScrollRect`/Grid if >4 slots ever exist.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Chrome:** as 17 §E (Back 0.045 W sq TL; TabBar 0.44 W centered TC; currencies right ~0.27 W TR).
- **Keeper:** x 0.36→0.64 (Δ0.28 W ≈655px) · y 0.10→0.62; glowing eyes focal ≈ (0.50 W, 0.22 H); hands at
  ≈ (0.37 W / 0.63 W, 0.45 H).
- **Featured chest:** x 0.33→0.67 (Δ0.34 W ≈796px) · y 0.12→0.78. Chest center ≈ (0.50 W, 0.50 H); amethyst
  gem on front ≈ (0.50 W, 0.52 H); aura beam vertical through center; pedestal base y ≈0.70→0.78.
- **Chest-slot row:** x 0.020→0.30 (Δ0.28 W ≈655px) · y 0.02→0.27 (Δ0.25 H ≈270px). 4 cells each ≈0.065 W,
  gaps ≈0.008 W; Slot_1 active gold-lit. TimerBar within Slot_1 at its bottom ≈0.05 H tall (green) with
  centered "02:59".
- **OPEN CHEST button:** x 0.71→0.985 (Δ0.275 W ≈643px) · y 0.04→0.20 (Δ0.16 H ≈173px). Violet pill; lock-key
  badge at left ≈0.05 W; label centered-right.
- **Notch/tablet/ultrawide:** SafeAreaFitter insets interactive layer; slots + CTA always inside; Keeper +
  chest may bleed at extremes. Bg full-bleed under cutout.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Tabs / currencies / Back | (shared, 17 §F) | — | — | — | — | — | — |
| Slot timer "02:59" | Sans (mono-ish) | Bold | — | +2% | 24 | white on green bar, dark stroke | `#ffffff` on `#2faa3a` |
| OPEN CHEST (button) | Serif display | Black | UPPER | +5% | 34 | gold bevel text on violet, glow + dark outline | `#f6e6b8` on violet |
| (Locked slots) | (icon only — padlock) | — | — | — | — | gold/grey padlock glyph | `#cdb887` |

(There is no other body copy on this screen — the chest names/odds appear on the result screen 21, not here.)

---

## G · MATERIALS
- **Gold frames / chrome:** as 17 §G.
- **Featured chest:** ornate **cast-gold** chest `#caa04a`→`#f6d77a` with engraved filigree bands, a large
  **amethyst gem** on the front face `#7a3fd0`→`#cfa6ff` (faceted, glowing); lid clasps + corner bosses gold;
  warm gold rim + violet aura cast onto the metal.
- **Aura beam / swirl:** additive violet `#8a40d8`→`#cfa6ff` vertical god-beam + orbiting spark ring; soft,
  high-bloom, semi-transparent.
- **Pedestal:** dark stone dais `#2a2630`→`#46414e` with gold + violet **rune** trim (glowing faint violet);
  worn edges.
- **Keeper:** near-black hooded robe `#0a0a12`→`#1a1622` silhouette; **glowing violet/white eyes**
  `#cfa6ff`; faint violet hand-glow; rim-lit by the aura.
- **Active slot (Slot_1):** gold-lit frame (bright bevel + glow); chest art = **silver/steel + cobalt** chest
  `#9aa0ad`/`#1e3a8c` with a **blue diamond gem** `#4fa8ff`; green **TimerBar** `#2faa3a`→`#46d257`.
- **Locked slots:** dim **wood** chests `#5a4326`→`#7a5e34` with iron bands `#3a3a40`; centered gold/grey
  **padlock** icon; desaturated.
- **Empty slot:** dark recessed socket `#14131a`, faint inner gold edge.
- **OPEN CHEST button:** amethyst pill `#6a2db8`→`#9e6bf0`, bright top bevel + **gold ornate frame** (notched
  diamond ends), outer violet glow; left **lock-and-key** badge (gold diamond plate + violet padlock/key).
  Pressed darkens.
- **Background:** crypt of pillars + **gold-coin hoards** `#caa04a` glinting in side alcoves, violet ambient
  fog; deep vignette.

---

## H · COMPONENTS (states + feedback)
**Shop tabs / currency / Back** — as 17 §H; **Tab_Chests selected** (gold-lit).

**Chest slot (cell)**
- *active/unlocking (Slot_1):* gold-lit frame, chest art, green TimerBar counting down ("02:59"); tap → info
  / "unlock now for N gems" prompt. When timer hits 0 → "READY" glow + tap opens.
- *ready:* frame pulses gold, chest shakes/glints, "OPEN" affordance; tapping → reveal.
- *locked (Slot_2/3):* dim + padlock; non-interactive (tooltip "earn/unlock to fill"); tap → shake or info.
- *empty (Slot_4):* dark socket, non-interactive.
- *unlock-skip:* spending gems removes the timer → ready (server-auth).

**OPEN CHEST button**
- *idle (ready chest available):* violet pill, lock-key icon + "OPEN CHEST".
- *hover:* +8% glow, scale 1.03; key icon sparkle.
- *pressed:* 0.97 darken.
- *not-ready (only timed chests):* either disabled (45%) OR becomes "OPEN NOW · N💎" gem-skip (governed by
  data/ADR). On click when ready → open animation → route to 21.
- *opening:* button → spinner; featured chest plays the open burst → screen transition to Chest Open Result.
- *insufficient (for gem-skip):* red shake → insufficient modal (37).

**Featured chest (tap = same as OPEN CHEST)**
- *idle:* aura beam + swirl loop, gentle bob, gem glint.
- *hover/press (if interactive):* chest lifts slightly, aura brightens.
- *opening:* lid bursts, violet light-burst → cut to 21.

---

## I · ANIMATION TIMELINE
- **OnShow (0→0.55s):** Bg fade (0.25) → chrome slide-down (0–0.25) → aura beam fade-in (0.1–0.4) → Keeper
  fade-in behind (0.1–0.35) → FeaturedChest scale 0.92→1 + glint (0.15–0.45) → ChestSlots stagger pop
  left→right 40ms (0.2–0.45) → OPEN CHEST slide from right +24px + glow ignite (0.25–0.5).
- **Idle loops:** featured chest bob (±5px, 2.6s) + aura beam opacity breathe (0.75↔1.0, 2.2s) + swirl ring
  rotate (slow, 8s) + gem specular twinkle; Keeper eye glow flicker (1.6s); Slot_1 TimerBar ticks every 1s,
  "02:59→02:58…"; gold-coin glints in bg alcoves (random).
- **Timer countdown:** numeric tick each second; at 0 → green→gold flash + "READY" + chest shake.
- **Open sequence (~1.0s):** chest lifts + aura intensifies (0.0–0.3) → lid bursts open + violet light-burst +
  particle explosion (0.3–0.6) → white/violet flash full-screen wipe (0.6–0.8) → route to Chest Open Result
  (21) under the flash (0.8–1.0).
- **Insufficient (gem-skip):** OPEN CHEST + gem chip red shake (0.3) → modal.

Easing: bob/breathe sine; pops ease-out back 0.2; open burst ease-out 0.3 then sharp flash.

---

## J · PARTICLE & FX
- Featured chest: violet god-beam (additive) + orbiting spark ring + rising amethyst sparkles + gem glint +
  floating dust motes in the beam.
- Keeper: faint violet wisps around the hands; eye-glow bloom.
- Active slot ready: gold sparkle + frame pulse.
- Open burst: large violet/gold particle explosion + radial light rays + screen-flash → transition.
- Bg: drifting violet fog + gold-coin glints in alcoves.
- Global: heavy vignette + strong bloom (the most bloom-heavy shop tab).

---

## K · EVENT BEHAVIOR
- **OnShow:** bind chest inventory (slots[]: state=active/locked/empty, chestType, unlockEndTime), featured
  chest definition, currencies — all server-auth. Start client-side countdown synced to server end-time.
- **OnSlotTap(slot):** if ready → open flow; if unlocking → show "unlock now (N gems)" prompt; if locked/empty
  → info/no-op.
- **OnOpenChest / OnFeaturedTap:** if ready → play open sequence → call server to roll rewards (server-auth
  RNG; client never rolls) → on response route to **Chest Open Result (21)** with the granted rewards.
- **OnGemSkip(slot):** server spends gems, marks ready (client never mutates balance, §12); insufficient →
  modal (37).
- **OnTimerComplete(slot):** flip slot to ready (server-confirmed).
- **OnTabSelect / OnBack / OnAdd "+":** as 17.

---

## L · NEGATIVE RULES
- Forensic state binding: **slot 1 active w/ timer "02:59"**, **slots 2–3 locked (padlock)**, **slot 4 empty**;
  featured **gold chest w/ amethyst gem** on a runed pedestal; hooded **Keeper** behind; **OPEN CHEST** violet
  CTA with lock-key icon; currencies **1726 / 48570**; **CHESTS** tab selected.
- Keep OPEN CHEST **violet** with the **lock-and-key** badge; keep the **4-slot** row order/states as drawn.
- Do NOT move chrome (Back TL, tabs TC, currencies TR). Bg full-bleed under cutout; slots + CTA in safe area.
- **§12 / server-auth RNG:** the **client never rolls loot** and never mutates balances; the server returns
  the result → screen 21 displays it. **No gameplay/ECS/balance change.**
- **ADR flag (do not alter spec):** timed chests + random rewards = loot-box/gacha, colliding with the "no
  loot boxes" cut → ADR required (transparent odds / fair redesign / or CUT). Spec **exactly as drawn**; the
  ADR governs implementation. Do NOT redesign the chest loop in this forensic spec.
- Invent nothing (no fabricated chest names, odds, or rarities on this screen — those live on 21/data).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Shared chrome correct; **CHESTS** tab selected; Gems 1726 / Gold 48570 with "+".
2. Center: gold featured chest with amethyst gem, violet aura beam + swirl, on a runed pedestal.
3. Hooded Keeper silhouette with glowing eyes/arms behind the chest.
4. Bottom-left 4-slot row: Slot_1 active gold-lit w/ green "02:59" timer; Slots 2–3 locked (padlock); Slot_4
   empty.
5. Bottom-right violet **OPEN CHEST** button with lock-and-key icon — brightest CTA.
6. Palette: amethyst aura everywhere + gold chest/frames + crypt bg, heavy vignette/bloom.
7. Fraction layout within ±2% at 2340×1080; chrome stable on notch/ultrawide/tablet.
8. Timer countdown, open sequence → route to 21, and insufficient feedback per H/I/K.

## N · IMPLEMENTATION CONFIDENCE
**91/100.** Layout, states, timer, and CTA are clear. Minus: (a) the featured chest + aura + Keeper are a
bespoke lit render (rebuild as layered sprites + additive beam/particles); (b) the open→reveal transition is a
multi-stage FX sequence (approximate); (c) exact slot-2/3 chest art + pedestal runes approximated; (d) the
ADR may change the whole loot loop — but the **visual** spec is firm.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (topbar, featured chest, slot row + timer, OPEN CHEST button) before
  writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree incl. shared tab bar + currency chips + back.
- [x] Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Forensic states/timer/colors recorded; nothing invented.
- [x] §12 / server-auth-RNG + **ADR loot-box flag** noted in A/L; spec NOT altered by the flag.
- [x] Landscape, full-bleed-under-cutout, content-in-safe-area enforced.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 21 · Chest Open Result

Source: design/ChestOpenResultDesign.png · 1536×1024 (1.5:1) · Analysis-only forensic spec.

> **Reward-reveal overlay** that follows **Chests (20)** after an open animation. Unlike the four shop tabs,
> this screen has **no shop tab bar and no top currency chips** — it is a focused full-screen "you got…"
> celebration with a single **COLLECT** CTA. It still belongs to the shop/reward family and shares the global
> dark-gold + amethyst DNA. **ADR-flagged** (gacha loot reveal — see 20).

---

## A · SCREEN PURPOSE
The **Chest Open Result** reveals the loot rolled from a chest. An open gold chest erupts with a violet light
burst at the top; below, the **reward cards** (here four) display each granted item with its **count**; the
rarest/headline reward (a **LEGENDARY** cosmetic) is presented as a larger, gold-rarity-bordered hero card on
the right. A single **COLLECT** button banks the rewards and returns the player to the shop/hub. The screen's
job is the dopamine beat: build-up burst → cards reveal one-by-one → COLLECT.

Shown rewards: **Silver ×500**, **Gems ×40**, **Cosmetic Shards ×15**, and **LEGENDARY — LIONHELM (Commander
Helmet)** (the headline, no count).

**ADR note (Section L):** this is the **payoff screen of the loot-box/gacha loop** → bound by the same "no
loot boxes" ADR as Chests (20). The reward content is **server-authoritative** (the client never rolls). Spec'd
**exactly as drawn**; ADR governs implementation.

---

## B · VISUAL DNA (screen-specific)
- **Mood:** triumphant reveal in the treasure crypt — a **violet/gold light explosion** from the open chest is
  the hero light; everything else is near-black so the burst + cards pop. Celebration > navigation.
- **Palette anchors:** amethyst/violet burst (`#7a3fd0`–`#cfa6ff`) + warm gold rays (`#e8c25a`–`#fff0b8`);
  rarity-coded cards (common silver/grey, rare violet, legendary **gold radiant**); **COLLECT button is
  cobalt/royal-blue**, gold-edged (bank/confirm, not premium-spend).
- **Lighting:** central radial light-burst behind the chest + god-rays; per-card rim light; the LEGENDARY card
  has a strong gold rarity glow + sparkle aura; deep vignette around the edges; heavy bloom on the burst.
- **Background:** full-bleed dim crypt (same world as 20), almost fully darkened by the vignette so the burst
  dominates; bleeds under cutout.
- **Hierarchy:** Title "CHEST REWARDS" (top) → open-chest light-burst (focal) → reward cards (the payoff, with
  the LEGENDARY card emphasized) → COLLECT (bottom CTA).

---

## C · SCREEN DECOMPOSITION (ASCII node tree)
```
ChestOpenResultScreen (UiScreen root / overlay, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Bg_FullBleed (Image, dark crypt, heavily vignetted)  [OUTSIDE safe area]
   │  ├─ Bg_Vignette (Image, strong radial)
   │  └─ Burst_Rays (Image, radial god-ray/light-burst, additive)  [behind chest]
   ├─ Header (top-center)
   │  ├─ Title    (Text "CHEST REWARDS")
   │  └─ Subtitle (Text "Your treasures await!")
   ├─ OpenChest_Hero (group, upper-center)
   │  ├─ Chest_Open (Image, ornate gold chest, lid open)
   │  └─ Burst_Core (Image, violet light-core + sparks erupting upward, additive)  [in front]
   ├─ RewardCards_Row (HorizontalLayoutGroup, 4 cards)
   │  ├─ Card_1 "SILVER"          (common)    → silver-coin-stack icon + "×500"
   │  ├─ Card_2 "GEMS"            (rare)      → amethyst-crystals icon + "×40"
   │  ├─ Card_3 "COSMETIC SHARDS" (rare)      → gold-shard icon + "×15"
   │  └─ Card_4 (LEGENDARY, larger, gold-radiant frame)
   │      ├─ Rarity_Label (Text "LEGENDARY")
   │      ├─ Item_Name    (Text "LIONHELM")
   │      ├─ Item_Sub     (Text "COMMANDER HELMET")
   │      └─ Item_Art     (Image, blue-plumed gold lion helm)   [no count — unique item]
   └─ CollectButton (Button, cobalt-blue, bottom-center) → Label "COLLECT"
```

---

## D · UNITY HIERARCHY SPEC (per node)
- **ChestOpenResultScreen** — parent UiRouter content (pushed as a result/overlay over Chests); `UiScreen` +
  `CanvasGroup`. Anchors stretch full. **No tab bar, no currency chips, no Back** (the only exit is COLLECT;
  optionally a tap-anywhere-to-skip-reveal). Built in code.
- **SafeAreaRoot** — `SafeAreaFitter`; interactive content parents here. **Bg_FullBleed + Burst_Rays OUTSIDE**
  safe area (true 0..1) so the burst bleeds edge-to-edge under the notch.
- **Burst_Rays** — parent Bg (not SafeAreaRoot); Image; anchor center, large (≥1.2× screen); additive; slow
  rotate. **Bg_Vignette** on top of Bg, multiply.
- **Header** — parent SafeAreaRoot; anchor **top-center** (0.5,1) pivot 0.5,1; y inset −0.04 H. Vertical
  mini-group: Title (big serif) over Subtitle (small).
- **OpenChest_Hero** — parent SafeAreaRoot; anchor **upper-center** (0.5,1) pivot 0.5,1; rect centered at
  ≈ (0.5 W, 0.30 H), size ≈ 0.28 W × 0.30 H. Draw order: Burst_Core (additive, behind/through chest) +
  Chest_Open. Decorative.
- **RewardCards_Row** — parent SafeAreaRoot; anchor **center** (0.5,0.5) biased lower; rect x 0.10→0.90 W,
  y 0.42→0.82 H. `HorizontalLayoutGroup` (spacing≈18px@1080, childAlignment MiddleCenter,
  childForceExpandWidth=false — the LEGENDARY card is wider). Card_1..3 equal width; **Card_4 ≈1.25× width**
  and slightly taller (hero rarity card). For ScrollRect safety if a chest can yield >4 cards, wrap row in a
  horizontal `ScrollRect` (disabled when ≤4, centered).
- **Common/Rare card (1–3)** — vertical stack: [Rarity-tinted Frame] → Name (top) → Art (center) → CountRow
  ("×N", bottom). VerticalLayoutGroup inside, controlled spacing.
- **Legendary card (4)** — taller frame with gold radiant border + sparkle; stack: Rarity_Label "LEGENDARY"
  (top) → Item_Name "LIONHELM" → Item_Sub "COMMANDER HELMET" → Item_Art (lion helm, large). **No count.**
- **CollectButton** — parent SafeAreaRoot; anchor **bottom-center** (0.5,0) pivot 0.5,0; y inset +0.05 H;
  rect width ≈0.34 W × 0.085 H. Cobalt pill, gold-edged. Primary (and effectively only) CTA.

**Responsive:** header top-center, chest upper-center, cards center, COLLECT bottom-center — all centered →
trivially stable on any aspect. The burst bg fills/bleeds. On ultrawide the cards row keeps fixed card sizes
and centers (extra gutter splits). On 4:3 tablet the 4 cards may grow; if too tight, allow 2×2 wrap
(GridLayoutGroup fallback) with the LEGENDARY card spanning/centered. SafeAreaFitter insets cards + COLLECT;
burst unaffected.

---

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
Source mock is 1.5:1 (portrait-ish); **normalize to 2340×1080 landscape** — the layout is a vertical stack
(Title → chest → cards → COLLECT) that maps cleanly to a centered landscape column with the cards spread wider.

- **Header:** Title baseline at y ≈0.08 H (top-center); Subtitle y ≈0.13 H. Title block width ≈0.5 W centered.
- **Open-chest hero:** center ≈ (0.50 W, 0.30 H); chest+burst envelope ≈ x 0.36→0.64 W, y 0.13→0.42 H. Burst
  rays radiate from ≈ (0.50 W, 0.30 H).
- **Reward cards row:** band x 0.10→0.90 (Δ0.80 W ≈1872px) · y 0.46→0.82 (Δ0.36 H ≈389px).
  - Cards 1–3 each ≈0.165 W (≈386px) × 0.34 H; gaps ≈0.022 W.
  - **Card_4 (legendary)** ≈0.205 W (≈480px) × 0.40 H (taller), right end of the row, vertically centered on
    the band (slightly overhangs top/bottom of the common cards).
  - Inside common card: Name y ≈0.50; Art center ≈0.62; "×N" ≈0.77.
  - Inside legendary: LEGENDARY y ≈0.48; LIONHELM y ≈0.52; COMMANDER HELMET y ≈0.55; helm art center ≈0.66.
- **COLLECT button:** x 0.33→0.67 (Δ0.34 W ≈796px) centered · y 0.05→0.135 H from bottom (pill ≈0.085 H tall).
- **Notch/tablet/ultrawide:** SafeAreaFitter insets the centered column; burst bg bleeds full. Everything is
  center-anchored → no edge collisions.

---

## F · TYPOGRAPHY (per text)
Sizes @1080-ref.

| Element | Personality | Weight | Caps | Tracking | px@1080 | Treatment | Hex |
|---|---|---|---|---|---|---|---|
| Title "CHEST REWARDS" | Serif display | Black | UPPER | +4% | 56 | heavy gold bevel + bloom + outer dark stroke | `#f0cf72`→`#caa04a` grad |
| Subtitle "Your treasures await!" | Serif italic | Regular | Title | +2% | 24 | soft gold, slight glow | `#e3d3a2` cream-gold |
| Card name (SILVER/GEMS/COSMETIC SHARDS) | Serif display | Semibold | UPPER | +3% | 26 (2-line for "COSMETIC SHARDS" ~22) | gold bevel + shadow | `#ecd591` |
| Card count "×500 / ×40 / ×15" | Sans | Bold | — | +2% | 30 | white, dark stroke; "×" slightly smaller | `#ffffff` |
| Legendary "LEGENDARY" | Sans condensed | Black | UPPER | +6% | 22 | bright gold, glow (rarity tag) | `#ffdf8a` |
| Legendary "LIONHELM" | Serif display | Bold | UPPER | +2% | 30 | gold bevel + bloom | `#f3d98a` |
| Legendary "COMMANDER HELMET" | Sans condensed | Semibold | UPPER | +4% | 16 | muted gold subtitle | `#cdb887` |
| COLLECT (button) | Serif display | Bold | UPPER | +5% | 40 | gold bevel text on cobalt, glow + outline | `#f3e6c0` on blue |

---

## G · MATERIALS
- **Burst rays / core:** additive radial **violet→white** light-burst `#8a40d8`→`#cfa6ff`→`#ffffff` plus warm
  **gold ray spokes** `#e8c25a`→`#fff0b8`; high bloom, soft edges, slow rotation; the dominant light.
- **Open gold chest:** ornate **cast-gold** `#caa04a`→`#f6d77a` with filigree, lid open, blue-gem accent on
  the front (matches 20's featured chest); violet light pours from inside; gold rim + violet inner glow.
- **Common card (Silver):** dark slate card `#14161e`→`#222633` with a cool **silver** rarity edge `#aab0bc`;
  art = stacked **silver coins** w/ embossed star `#c4c8d0`→`#eef0f4`, cool specular.
- **Rare cards (Gems, Cosmetic Shards):** dark card with a **violet** rarity edge `#7a3fd0`; Gems art =
  **amethyst crystal cluster** `#8a40d8`→`#cfa6ff` glowing; Shards art = **gold helm-shard / crest** fragment
  `#caa04a`→`#f6d77a` with violet edge glints.
- **Legendary card:** dark card with a **radiant gold rarity frame** `#e8c25a`→`#fff0b8` + animated sparkle
  aura + corner light flares; art = **gold lion helm with a cobalt-blue plume** `#1f3fb0`→`#4f8bff` crest, gold
  face `#caa04a`→`#f6d77a`, glowing eyes; strongest glow on screen. (No count — unique cosmetic.)
- **COLLECT button:** royal/cobalt blue `#1f3fb0`→`#3a63d0`, bright top bevel + **gold ornate edge** (notched
  corners), outer glow; pressed darkens.
- **Background:** dim crypt `#0a0b0f`→`#161a24`, almost fully vignetted to black at the edges; subtle gold
  hoard glints far behind the burst.

---

## H · COMPONENTS (states + feedback)
This screen is mostly **presentational** (rewards are already decided server-side); interactivity = reveal
pacing + COLLECT.

**Reward card (display, with reveal state)**
- *hidden (pre-reveal):* card scaled 0 / face-down or behind the burst.
- *revealing:* flips/scales up 0→1 with a rarity-colored flash; count "×N" counts up quickly; rarer cards get
  a bigger flash + sparkle.
- *revealed/idle:* card settled; rare/legendary cards keep a gentle glow loop; legendary keeps sparkle aura +
  slow frame shimmer.
- *(non-interactive otherwise; optional hover tooltip = item description.)*

**Legendary card** — same reveal but with the biggest flash, a gold radiant ring sweep, a brief slow-mo beat,
and a persistent sparkle aura (signals the chase reward).

**COLLECT button**
- *disabled (during reveal):* dimmed until all cards revealed (or tap-to-skip reveals instantly then enables).
- *idle (ready):* cobalt pill "COLLECT", gentle glow.
- *hover:* +8% glow, scale 1.03.
- *pressed:* 0.97 darken.
- *on collect:* rewards fly to their HUD destinations (silver/gems → currency, items → inventory) + sparkle;
  screen fades out → returns to Chests/hub; wallet reflects server-confirmed grant.

**Tap-anywhere (optional):** during the staggered reveal, a tap **fast-forwards** all cards to revealed and
enables COLLECT (standard reward-screen affordance).

---

## I · ANIMATION TIMELINE
- **OnShow / open burst (0→0.6s):** inherits the flash from Chests' open sequence → Bg + heavy vignette in
  (0–0.2) → Burst_Rays bloom up + rotate begins (0.0–0.4) → Chest_Open pops in with Burst_Core eruption
  (0.1–0.4) → Title "CHEST REWARDS" scales 0.9→1 + bloom + Subtitle fade (0.2–0.5).
- **Card reveal cascade (0.5→1.6s):** cards reveal left→right, **rarest last** — Card_1 (0.55), Card_2 (0.75),
  Card_3 (0.95), then a beat, **Card_4 LEGENDARY (1.2)** with a bigger flash + gold ring sweep + brief
  slow-mo + sparkle aura. Each: scale 0→1 ease-out-back (0.25) + rarity flash (0.15) + count-up (0.3).
- **COLLECT enable (~1.6s):** button fade from dim→bright + slight bounce; (a tap before this fast-forwards
  the cascade).
- **Idle loops (post-reveal):** Burst_Rays slow rotate + opacity breathe (2.4s); legendary sparkle aura +
  frame shimmer (2s); rare cards faint glow breathe; floating motes in the burst.
- **OnCollect (0→0.5s):** reward icons arc to destinations (0.0–0.35, ease-in) + sparkle + COLLECT bump
  (1.0→1.1→1.0) → screen CanvasGroup fade-out (0.3–0.5) → pop back to Chests/hub.

Easing: reveals ease-out-back 0.25; burst ease-out; collect arcs ease-in 0.35; fades 0.3–0.5.

---

## J · PARTICLE & FX
- Central: large additive violet+gold **light-burst** + rotating ray spokes + upward **spark fountain** from
  the open chest + drifting embers/motes; high global bloom.
- Per-card reveal: rarity-colored flash ring + small sparkle puff (bigger for legendary).
- Legendary: persistent gold sparkle aura + corner light flares + a one-shot radiant ring sweep on reveal.
- OnCollect: reward-icon projectiles with trailing sparkles toward HUD; final twinkle.
- Edge vignette keeps focus on the burst/cards.

---

## K · EVENT BEHAVIOR
- **OnShow(rewards[]):** receives the **server-authoritative** reward list from the chest open (id, type,
  count, rarity, art) — the **client never rolls**. Play burst + staggered reveal in rarity order.
- **OnTapSkip:** fast-forward all reveals to final state; enable COLLECT.
- **OnRevealComplete:** enable COLLECT.
- **OnCollect:** confirm grant with server (already granted at roll-time; this banks/acks) → animate rewards to
  destinations → update wallet/inventory from server state → fade out → return to Chests (20) / hub.
- **No Back, no tab bar, no currency "+":** COLLECT (or tap-skip then COLLECT) is the only exit.
- **OnReconnect/already-collected:** if re-entered, show as collected / dismiss gracefully (idempotent).

---

## L · NEGATIVE RULES
- Forensic copy/counts binding: Title **"CHEST REWARDS"**, subtitle **"Your treasures await!"**; cards
  **SILVER ×500**, **GEMS ×40**, **COSMETIC SHARDS ×15**, **LEGENDARY / LIONHELM / COMMANDER HELMET** (no
  count); CTA **COLLECT**.
- The legendary card has **no ×count** (it's a unique item) — do not add one. Keep card **rarity order/emphasis
  as drawn** (legendary largest, right end, gold-radiant).
- **No tab bar, no top currency chips, no Back** on this screen — do not add shop chrome here.
- Keep **COLLECT cobalt-blue** (confirm/bank), not violet.
- Bg + burst full-bleed under cutout; cards + COLLECT in safe area, centered.
- **§12 / server-auth RNG:** the **client never rolls or fabricates rewards**; it only **displays** the
  server-returned list and acks COLLECT. Wallet/inventory update from server state. **No gameplay/ECS/balance
  change.**
- **ADR flag (do not alter spec):** this is the gacha payoff screen → bound by the same loot-box ADR as Chests
  (20). Spec **exactly as drawn**; the ADR governs the loot model, not this reveal's visuals.
- Invent nothing (no extra reward cards, no fabricated counts/rarities beyond the four shown).

## M · ACCEPTANCE CRITERIA (≥95%)
1. Top: gold "CHEST REWARDS" title + "Your treasures await!" subtitle.
2. Upper-center: open gold chest with a violet+gold light-burst / ray fountain (focal).
3. Four reward cards in order: SILVER ×500, GEMS ×40, COSMETIC SHARDS ×15, and a larger gold-radiant LEGENDARY
   LIONHELM (Commander Helmet) card with **no count**.
4. Bottom-center cobalt **COLLECT** button (gold-edged) — the only exit; **no tab bar / currencies / Back**.
5. Palette: violet+gold burst on near-black, rarity-coded cards, deep vignette + heavy bloom.
6. Fraction layout within ±2% at 2340×1080 (centered column); stable on notch/ultrawide/tablet.
7. Staggered rarity-order reveal (legendary last/biggest), tap-skip, COLLECT-banks-then-returns per H/I/K.
8. Rewards are server-authoritative display-only (client never rolls).

## N · IMPLEMENTATION CONFIDENCE
**93/100.** Copy, counts, rarity treatment, and layout are crisp. Minus: (a) the central light-burst + ray
fountain is a custom additive/particle composite (approximate with rays sprite + particle systems + bloom);
(b) the LEGENDARY lion-helm art + radiant frame are bespoke (use sprite + animated gold frame); (c) exact
reveal timing/slow-mo is a feel choice (values given are a faithful estimate). No interactive-fidelity risk.

## O · SELF-CHECKLIST
- [x] Read source PNG + zoom crops (title, 4 cards, legendary card, COLLECT) before writing.
- [x] All A–O present, substantive, in order.
- [x] Node tree documented; explicitly notes the **absence** of the shared tab bar / currency chips / back
  (this is a reveal overlay, not a shop tab).
- [x] Fraction layout normalized to 2340×1080 (1.5:1 source → centered landscape column); tablet/ultrawide/
  notch covered.
- [x] Forensic copy/counts/rarities/colors recorded; nothing invented (legendary has no count).
- [x] §12 / server-auth-RNG (display-only) + **ADR gacha flag** noted in A/L; spec NOT altered by the flag.
- [x] Landscape, full-bleed-burst-under-cutout, content-in-safe-area enforced.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 22 · Units / Army

Source: design/UnitsArmyDesign.png · 1672×941 (1.78:1) · Analysis-only forensic spec.

> Normalize the source 1672×941 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). All positions below are given as **fractions of 2340×1080** so they scale 1:1 onto the
> mockup and onto any landscape device. Pixel values quoted "@1080" are the on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Army / Units collection + upgrade** meta screen. Three-zone layout:
1. **Header** (title "ARMY", currency chips).
2. **Left/center roster**: faction tabs (Iron Pact / Ashen Horde), a class-filter strip with a "Collected
   48/58" counter, and a **5-column × 2-row card grid** of owned/locked units (10 visible; scrollable for the
   full 58). Each card shows portrait, level badge, name, and an upgrade-material count bar.
3. **Right detail panel** (≈⅓ width): the selected unit's hero portrait, role line ("Frontline · Defensive"),
   **LEVEL 12/20**, three stat bars (Health / Damage / Speed with current value + green "+N" upgrade preview),
   an **Upgrade Progress** tier-node row (10 11 12 13 14), the primary **UPGRADE — 200 [gold]** CTA, and a
   "Hold to upgrade quickly" hint.
4. **Bottom utility bar**: **Rarity** · **Collection** · **Disenchant**.

Purpose: let the player browse the roster per faction, inspect a unit's stats, spend Gold to upgrade a unit's
level (advancing tier nodes), and disenchant duplicates for materials. Server-authoritative meta — the client
displays balances and requests an upgrade; it never mutates a balance locally.

Reached from: Main Menu rail (Units/Army). Back: top-left ornate corner (see §C — a recessed corner bracket;
the source crops it, treat as the standard Back affordance).

---

## B — VISUAL DNA (screen-specific, on top of global)
- **Mood:** armory / war-table. Dark obsidian field, a faint ruined-castle vista bleeding through the right
  detail panel's backdrop, warm gold ornamentation, cobalt selection energy.
- **Palette anchors:**
  - Page background: near-black charcoal `#0a0b0f → #14161e` vertical gradient, vignetted corners.
  - Gold chrome (title, frames, chips, tier nodes): `#6b5320` shadow → `#caa04a` mid → `#f0d27a` highlight.
  - Iron Pact / selection / CTA blue: `#1d3a8a → #2b56c8 → #4f8bff` with cyan rim `#7fb0ff`.
  - Ashen Horde tab (idle): oxblood `#5a1712 → #7a1f1a` with ember edge `#d8452b`.
  - Rarity tints on card bottom bars & class glints: common steel `#9aa3ad`, uncommon green `#4caf50`,
    rare blue `#3d7fe0`, epic violet `#9e6bf0`, legendary gold-orange `#f0a93a`.
  - Stat-bar fills: Health green `#5fd35a`, Damage amber `#f0a93a`, Speed cyan-blue `#4fb0ff`; the green
    "+N" preview text is `#7ff06a`.
- **Lighting:** focal glow behind the selected card and behind the detail portrait; gold rim-light on every
  bevel; soft inner shadow inside card wells; vignette darkens all four corners.
- **Hierarchy:** ARMY title → faction tabs → selected card (blue halo) → detail portrait → UPGRADE CTA →
  currency chips → filters → bottom bar.

---

## C — SCREEN DECOMPOSITION (ASCII node tree — every node)
```
ArmyScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed, extends under cutout)
│  ├─ BG_Gradient (Image, charcoal vertical gradient)
│  └─ BG_Vignette (Image, multiply, darkened corners)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter — ALL content below)
│  ├─ Header
│  │  ├─ Btn_Back (Button) — top-left ornate corner bracket
│  │  │  └─ Icon_BackChevron (Image)
│  │  ├─ Lbl_Title "ARMY" (Text, serif gold bevel, UPPERCASE)
│  │  └─ CurrencyChips (HorizontalLayoutGroup, anchored top-right)
│  │     ├─ Chip_Silver  [icon silver-coin | "12,450" | Btn_Plus]
│  │     ├─ Chip_Gold    [icon gold-coin   | "1,850"  | Btn_Plus]
│  │     └─ Chip_Gems    [icon violet-gem  | "2,340"  | Btn_Plus]
│  ├─ FactionTabs (HorizontalLayoutGroup, 2 tabs, center-left)
│  │  ├─ Tab_IronPact (Toggle, SELECTED) [crest | "IRON PACT"]
│  │  └─ Tab_AshenHorde (Toggle) [crest | "ASHEN HORDE"]
│  ├─ FilterStrip (HorizontalLayoutGroup, left) 
│  │  ├─ Filter_All (Toggle, SELECTED) "All"
│  │  ├─ Filter_Shield (Toggle, icon)         // class: frontline/tank
│  │  ├─ Filter_Sword (Toggle, icon)          // class: melee
│  │  ├─ Filter_Bow (Toggle, icon)            // class: ranged
│  │  ├─ Filter_Magic (Toggle, icon)          // class: caster
│  │  └─ Filter_Special (Toggle, icon)        // class: special
│  ├─ CollectedCounter (right-aligned over grid)
│  │  ├─ Lbl_CollectedTag "Collected"
│  │  └─ Lbl_CollectedVal "48/58"
│  ├─ RosterScroll (ScrollRect, vertical, masked viewport)
│  │  └─ RosterGrid (GridLayoutGroup, 5 cols × N rows)
│  │     ├─ UnitCard_Shieldman   (SELECTED)
│  │     ├─ UnitCard_Sentinel
│  │     ├─ UnitCard_IronArcher
│  │     ├─ UnitCard_HeavyGuard
│  │     ├─ UnitCard_RunicAdept
│  │     ├─ UnitCard_Miner
│  │     ├─ UnitCard_Warden
│  │     ├─ UnitCard_Crossbowman
│  │     ├─ UnitCard_Oathbreaker
│  │     └─ UnitCard_Flamecaller
│  │        (each UnitCard:)
│  │        ├─ Frame_Rarity (Image, rarity-tinted bevel)
│  │        ├─ Portrait (Image, unit render, masked)
│  │        ├─ SelectGlow (Image, blue halo — only when selected)
│  │        ├─ Badge_Level (Image circle + Text e.g. "12")
│  │        ├─ Lbl_Name (Text, UPPERCASE small)
│  │        └─ CountBar (Slider-style) [Fill + Lbl "125 / 300" + ▲ up-arrow if upgradable]
│  ├─ DetailPanel (Image parchment/stone framed, right ⅓)
│  │  ├─ Detail_BG (Image, dark stone + faint vista)
│  │  ├─ Detail_Crest (Image, small unit class crest top-right)
│  │  ├─ Detail_Name "SHIELDMAN" (Text, serif gold)
│  │  ├─ Detail_Role "Frontline • Defensive" (Text)
│  │  ├─ Detail_Portrait (Image, large unit render)
│  │  │  ├─ Btn_PrevUnit (Button, left chevron)
│  │  │  └─ Btn_NextUnit (Button, right chevron)
│  │  ├─ Detail_LevelRow [ "LEVEL " + "12" + "/20" + Btn_Info(ⓘ) ]
│  │  ├─ StatRows (VerticalLayoutGroup)
│  │  │  ├─ Stat_Health  [♥ icon | bar Fill | "3,240" | "+180" green]
│  │  │  ├─ Stat_Damage  [⚔ icon | bar Fill | "210"   | "+12"  green]
│  │  │  └─ Stat_Speed   [» icon | bar Fill | "86"    | "+4"   green]
│  │  ├─ UpgradeProgress
│  │  │  ├─ Lbl_UpgradeProgress "UPGRADE PROGRESS"
│  │  │  └─ TierNodeRow (HorizontalLayoutGroup) [10][11][(12 current)][13][14] + connectors
│  │  ├─ Btn_Upgrade (Button, blue CTA) "UPGRADE — 200" + [gold icon]
│  │  └─ Lbl_HoldHint "Hold to upgrade quickly"
│  └─ BottomBar (HorizontalLayoutGroup, 3 buttons, bottom-center under grid)
│     ├─ Btn_Rarity   [icon | "Rarity"]
│     ├─ Btn_Collection [icon | "Collection"]
│     └─ Btn_Disenchant [icon | "Disenchant"]  (violet accent)
```

---

## D — UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor preset | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| ArmyScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | .5,.5 | — | — | fills canvas |
| BG_FullBleed | ArmyScreen | 0 | Image | stretch-all | .5,.5 | — | **ignores** safe area (full-bleed) | width grows on ultrawide |
| BG_Gradient | BG_FullBleed | 0 | Image | stretch-all | .5,.5 | — | full-bleed | tile-safe |
| BG_Vignette | BG_FullBleed | 1 | Image (mult) | stretch-all | .5,.5 | — | full-bleed | corners scale |
| SafeAreaRoot | ArmyScreen | 1 | RectTransform + SafeAreaFitter | stretch-all | .5,.5 | — | **defines** inset | insets to Screen.safeArea |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans safe width |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | fixed size, pinned TL |
| Lbl_Title | Header | 1 | Text | top-left (offset right of back) | 0,1 | left | inside | left-anchored |
| CurrencyChips | Header | 2 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-anchored, fixed |
| Chip_* | CurrencyChips | 0..2 | RectTransform (Image+Text+Button) | — | — | mid | inside | fixed widths |
| FactionTabs | SafeAreaRoot | 1 | HorizontalLayoutGroup | top-left (below header) | 0,1 | left | inside | left cluster |
| Tab_IronPact / Tab_AshenHorde | FactionTabs | 0,1 | Toggle (ToggleGroup) | — | — | mid | inside | equal width pill |
| FilterStrip | SafeAreaRoot | 2 | HorizontalLayoutGroup | top-left (below tabs) | 0,1 | left | inside | left cluster |
| Filter_* | FilterStrip | 0..5 | Toggle (ToggleGroup) | — | — | mid | inside | square chips |
| CollectedCounter | SafeAreaRoot | 3 | RectTransform | top (right of grid area) | 1,1 | right | inside | right-anchored |
| RosterScroll | SafeAreaRoot | 4 | ScrollRect (vertical only) | top-left | 0,1 | — | inside | width = left ⅔; height to bottom bar |
| RosterGrid | RosterScroll/Viewport/Content | 0 | GridLayoutGroup | top-stretch | .5,1 | upper-left | inside | 5 fixed cols, rows grow |
| UnitCard_* | RosterGrid | 0..n | RectTransform (Button) | grid cell | .5,.5 | — | inside | uniform cell |
| Frame_Rarity | UnitCard | 0 | Image | stretch-all | .5,.5 | — | inside | scales w/ card |
| Portrait | UnitCard | 1 | Image (masked) | stretch (inset) | .5,.5 | — | inside | scales w/ card |
| SelectGlow | UnitCard | 2 | Image | stretch-all (overscan) | .5,.5 | — | inside | only on selected |
| Badge_Level | UnitCard | 3 | Image+Text | bottom-left | 0,0 | mid | inside | fixed |
| Lbl_Name | UnitCard | 4 | Text | bottom-stretch (above bar) | .5,0 | center | inside | shrinks to fit |
| CountBar | UnitCard | 5 | Slider(no handle)+Text | bottom-stretch | .5,0 | mid | inside | full card width |
| DetailPanel | SafeAreaRoot | 5 | Image (framed) | right-stretch (vertical) | 1,.5 | — | inside | fixed fraction width, full height |
| Detail_BG | DetailPanel | 0 | Image | stretch-all | .5,.5 | — | inside | fills panel |
| Detail_Crest | DetailPanel | 1 | Image | top-right | 1,1 | — | inside | fixed |
| Detail_Name | DetailPanel | 2 | Text | top-center | .5,1 | center | inside | shrink to fit |
| Detail_Role | DetailPanel | 3 | Text | top-center (below name) | .5,1 | center | inside | — |
| Detail_Portrait | DetailPanel | 4 | Image | top-center (below role) | .5,1 | center | inside | aspect-locked |
| Btn_PrevUnit / Btn_NextUnit | Detail_Portrait | 0,1 | Button | mid-left / mid-right | .5,.5 | — | inside | fixed |
| Detail_LevelRow | DetailPanel | 5 | RectTransform | center-stretch | .5,.5 | center | inside | — |
| StatRows | DetailPanel | 6 | VerticalLayoutGroup | center-stretch | .5,.5 | — | inside | full panel width (inset) |
| Stat_* | StatRows | 0..2 | RectTransform (icon+Slider+2 Text) | stretch-x | .5,.5 | — | inside | bar flexes |
| UpgradeProgress | DetailPanel | 7 | RectTransform | center-stretch | .5,.5 | center | inside | — |
| TierNodeRow | UpgradeProgress | 1 | HorizontalLayoutGroup | center-stretch | .5,.5 | center | inside | 5 nodes + connectors |
| Btn_Upgrade | DetailPanel | 8 | Button | bottom-stretch | .5,0 | center | inside | wide CTA |
| Lbl_HoldHint | DetailPanel | 9 | Text | bottom-center | .5,0 | center | inside | — |
| BottomBar | SafeAreaRoot | 6 | HorizontalLayoutGroup | bottom-left (under grid) | .5,0 | center | inside | spans left ⅔ |
| Btn_Rarity/Collection/Disenchant | BottomBar | 0..2 | Button | — | — | mid | inside | equal width |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Global safe-area margin:** treat ~3.0% (≈70px) inset on all sides as the design's working frame.

**Header band:** y from 0.93 → 1.00 (top 7%).
- Btn_Back: x-center 0.030, y-center 0.965; box ≈ 0.045w × 0.085h (≈105×92@1080).
- Lbl_Title "ARMY": left edge x≈0.075, baseline y≈0.955; cap-height ≈ 0.052h (≈56px@1080).
- CurrencyChips: right edge x=0.970, y-center 0.965. Each chip ≈ 0.115w × 0.052h (≈269×56@1080); gap 0.010w.
  Order L→R: Silver, Gold, Gems. Each = [round icon Ø≈0.030w][value text][+ button Ø≈0.026w].

**Faction tabs:** band y 0.855 → 0.915.
- Two pills starting x≈0.020. Each pill ≈ 0.165w × 0.052h (≈386×56@1080), gap 0.008w.
- Iron Pact pill spans x 0.020→0.185 (selected — blue fill). Ashen pill x 0.193→0.358.

**Filter strip:** band y 0.790 → 0.845.
- "All" pill x≈0.020, ≈0.050w wide. Then 5 square class chips ≈0.038w each (≈89px), gap 0.008w, running to
  x≈0.40. (The chips read as: shield, crossed-swords, bow, magic-bolt, special/star.)
- CollectedCounter: anchored to grid right edge (x≈0.625), top y≈0.840; "Collected" tag small over "48/58".

**Roster grid (left ⅔ of screen):**
- Grid region: x 0.018 → 0.630 (width 0.612w ≈ 1432px@1080), y 0.130 → 0.775 (height 0.645h ≈ 697px@1080).
- **5 columns × 2 visible rows.** Cell size: with 4 inter-column gaps of 0.012w (≈28px) →
  cellW = (0.612 − 4·0.012)/5 = 0.1128w ≈ **264px@1080**; cellH ≈ 0.300h ≈ **324px@1080** (portrait taller
  than wide, ~0.81:1). Row gap 0.030h (≈32px). GridLayoutGroup: cell (264,324), spacing (28,32),
  startCorner Upper-Left, constraint Fixed-Column-Count = 5.
- **Within each card:**
  - Frame inset 0 (frame = card bounds). Portrait inset ≈ 0.10·cell on all sides.
  - Badge_Level: bottom-left, circle Ø ≈ 0.34·cellW (≈90px), inset ~6px from BL corner; number centered.
  - Lbl_Name: horizontal band at ~78% card height, centered, cap ≈ 18px@1080 UPPERCASE.
  - CountBar: bottom strip, full card width inset 6px, height ≈ 0.075·cellH (≈24px); centered text
    "125 / 300"; small ▲ up-arrow glyph at right end when upgrade material available (Shieldman shows ▲).

**Detail panel (right ⅓):**
- Panel region: x 0.660 → 0.982 (width 0.322w ≈ 753px@1080), y 0.060 → 0.945 (full height inside safe).
- Detail_Name baseline y≈0.905; cap ≈ 0.040h (≈43px). Detail_Role y≈0.875.
- Detail_Portrait: centered, region y 0.640→0.860, width ≈ 0.20w; Prev/Next chevrons at its vertical mid,
  pinned to panel inner left/right (x≈0.675 / x≈0.965).
- Detail_LevelRow: y≈0.610; "LEVEL" + big "12" + "/20" + ⓘ.
- StatRows: y 0.460 → 0.585, 3 rows evenly spaced (row pitch ≈0.042h). Each row: icon (Ø≈0.022w) at left,
  bar from x≈0.700→0.870 (track), value right-aligned x≈0.905, "+N" green to its right x≈0.945.
- UpgradeProgress: label y≈0.430; TierNodeRow y≈0.370 → 0.405. 5 nodes across panel width: node Ø ≈ 0.038w
  (≈89px); current node (12) slightly larger (×1.15) and brighter; connectors are short gold bars between.
  Node labels under each: 10,11,12,13,14 (12 highlighted gold).
- Btn_Upgrade: y 0.250→0.320 (height ≈0.070h ≈76px), spans panel inner width (x 0.672→0.972). Text
  "UPGRADE — 200" + gold-coin icon, centered.
- Lbl_HoldHint: y≈0.225, centered, small italic.

**Bottom bar:** band y 0.025 → 0.085, spans grid width (x 0.018→0.630). 3 equal buttons ≈0.195w each, gaps
0.012w. Order: Rarity, Collection, Disenchant.

**Tablet (4:3 ≈ 1440×1080):** width compresses to 1.0 match-height baseline; grid may drop to 4 columns if
cellW < 240 (keep ≥4); detail panel keeps fixed 0.322 fraction → on 4:3 it occupies more relative width, so
clamp detail panel to max 380px and reflow grid. **Ultrawide (21:9 / 2.33:1 ≈ 2520×1080):** extra width pads
the gutter between grid and detail; keep grid left-anchored, detail right-anchored, BG full-bleed fills.
**Notch (landscape side cutout):** SafeAreaRoot insets; full-bleed BG slides under; back button and chips never
enter the cutout.

---

## F — TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| "ARMY" title | serif display, regal | Bold/Black | UPPER | +6% (wide) | — | gold bevel + soft outer bloom + 2px dark drop | 56 | grad `#f0d27a`→`#caa04a` |
| Currency values "12,450/1,850/2,340" | clean tabular sans | Semibold | — | tabular | — | thin 1px dark stroke | 30 | `#f4e9c8` |
| Tab labels "IRON PACT"/"ASHEN HORDE" | strong sans-serif | Bold | UPPER | +4% | — | drop-shadow; selected = bright | 26 | sel `#eaf2ff`; idle `#c9b27a` |
| "All" + filter labels | sans | Medium | Title | 0 | — | — | 22 | `#d9c79a` |
| "Collected" tag | sans | Regular | Title | +2% | — | — | 20 | `#b9c0c8` |
| "48/58" value | tabular sans | Semibold | — | tabular | — | — | 26 | `#f4e9c8` |
| Card name (e.g. "SHIELDMAN") | condensed sans | Semibold | UPPER | +3% | — | 1px dark stroke | 18 | `#e8e2cf` |
| Card count "125 / 300" | tabular sans | Medium | — | tabular | — | — | 17 | full `#e8f0c0`, low `#cf6b5a` |
| Card level badge "12" | sans | Bold | — | — | — | dark stroke for legibility | 24 | `#fff4d6` |
| Detail name "SHIELDMAN" | serif display | Bold | UPPER | +5% | — | gold bevel + bloom | 43 | `#f0d27a`→`#caa04a` |
| Detail role "Frontline • Defensive" | sans, light | Regular | Title | +6% | — | subtle | 22 | `#c9b27a` |
| "LEVEL" / "12" / "/20" | serif label / tabular | Semibold/Bold | UPPER | — | — | "12" glows gold | LEVEL 24, 12 → 40, /20 → 24 | `#f0d27a` / `#fff4d6` / `#9a8a5a` |
| Stat labels (icons only, no text) | — | — | — | — | — | — | — | — |
| Stat values "3,240 / 210 / 86" | tabular sans | Semibold | — | tabular | — | — | 26 | `#f4f0e2` |
| Stat preview "+180 / +12 / +4" | tabular sans | Bold | — | tabular | — | green glow | 22 | `#7ff06a` |
| "UPGRADE PROGRESS" | sans | Medium | UPPER | +8% (tracked) | — | dim | 18 | `#9a8a5a` |
| Tier node numbers "10..14" | tabular sans | Semibold | — | — | — | current=gold glow | 20 | cur `#f0d27a`, others `#9aa3ad` |
| CTA "UPGRADE — 200" | strong sans | Bold | UPPER | +4% | — | white text + dark stroke; gold icon | 32 | `#ffffff` |
| "Hold to upgrade quickly" | sans italic | Light | Title | +2% | — | subtle | 18 | `#9aa3ad` |
| Bottom bar labels | sans | Medium | Title | +2% | — | icon-led | 22 | `#d9c79a`; Disenchant `#c8a6ff` |

Font: TMP SDF Roboto for body/numbers; serif SDF (Trajan/Cinzel-like) for ARMY + SHIELDMAN + LEVEL display.

---

## G — MATERIALS
- **Page BG:** flat charcoal gradient `#0a0b0f`(top) → `#14161e`(bottom), 0 roughness (matte), radial vignette
  to `#05060a` corners (≈45% darken). No reflection.
- **Gold frames/title/chips/tier nodes:** brushed antique gold — base `#a8842f`, shadow `#6b5320`, highlight
  `#f0d27a`; medium-high specular along top bevel; light micro-scratch wear on long bars; 1px inner dark line
  separating bevel from fill; faint bloom on highlights only.
- **Card frames:** dark cast-iron well `#16181f` with a rarity-tinted inner edge glow (see palette). Selected
  card: full **cobalt halo** `#4f8bff` (outer soft glow ~12px) + brighter gold frame; portrait sits in a
  recessed shadowed well (inner shadow ~6px).
- **Tabs:** Iron Pact selected = polished cobalt enamel `#2b56c8` with cyan rim + gold crest; Ashen idle =
  matte oxblood `#7a1f1a` with ember rim, slightly desaturated (unselected).
- **Stat bars:** dark groove track `#1b1d24`; fills are glassy with a top specular line — Health `#5fd35a`,
  Damage `#f0a93a`, Speed `#4fb0ff`; the "+N" portion can render as a brighter lighter cap on the fill.
- **Detail panel:** dark stone slab `#101218` with a faint desaturated castle vista (≈20% opacity) and a gold
  filigree frame; soft inner vignette.
- **CTA (Upgrade):** cobalt button `#2f5fd6` with lighter top edge `#5f8bff` and dark bottom `#1b367f`,
  rounded ~14px, gold thin trim, white label; gold-coin icon inset; subtle bloom on idle.
- **Disenchant accent:** violet `#9e6bf0` glyph/edge to read as "convert to materials".
- **Bloom budget:** restrained — title, selected halo, CTA, tier-current node, stat-fill speculars only.

---

## H — COMPONENTS (states + feedback)
**UnitCard** (Button):
- *idle (owned):* rarity-tinted frame, lit portrait, level badge, count bar with fill.
- *hover/focus (gamepad):* gold frame brightens +15%, slight scale 1.03, soft glow.
- *pressed:* scale 0.97, inner darken.
- *selected:* persistent cobalt halo + brighter gold frame + portrait fully lit (the SHIELDMAN card state).
- *upgradable:* small ▲ green up-arrow on the count bar (material ≥ threshold) — Shieldman, Miner show it.
- *locked/unowned:* portrait silhouetted dark, frame desaturated, level badge replaced by 🔒, no count bar
  (or "0/—"); tapping opens "how to unlock" (the 58 roster includes locked entries beyond the 10 shown).
- *maxed:* badge shows max level, count bar hidden or "MAX".

**FactionTab** (Toggle, single-select group):
- *selected:* filled faction color, bright label, crest lit, slight raised look.
- *unselected:* darkened/desaturated, hover brightens; click swaps the whole roster + detail to that faction.

**FilterChip** (Toggle group, "All" default):
- *selected:* gold ring + lit icon; *idle:* dim; filters the grid by class. Multi vs single = single (radio).

**TierNode** (Upgrade Progress):
- *passed (≤ current):* filled gold, check-style.
- *current (12):* enlarged, bright gold glow ring.
- *future (>current):* dim steel outline.
- Connectors between nodes fill gold up to current.

**Stat row:** animated fill bar; on hovering a future upgrade, the "+N" green segment highlights to preview
the post-upgrade value.

**CTA_Upgrade** (Button):
- *idle:* cobalt, white "UPGRADE — 200" + gold coin, soft glow.
- *hover:* +10% brightness, glow grows.
- *pressed:* scale 0.97 + brief inner flash; **hold** = repeat-upgrade ("Hold to upgrade quickly" — long-press
  ramps repeat rate).
- *disabled (insufficient gold or maxed):* desaturated grey-blue, label dim, lock/coin-grey; tap → shake +
  "Not enough Gold" toast (routes to Store via chip +).
- *affordable feedback:* coin count on chip flashes when spent.

**BottomBar buttons** (Rarity / Collection / Disenchant): standard icon+label buttons; Disenchant opens a
multi-select disenchant flow (duplicates → materials); violet accent.

---

## I — ANIMATION TIMELINE
**OnShow (screen enter, ~0.45s):**
- 0.00s BG + vignette fade in (0.20s).
- 0.05s Header slides down 16px + fade (0.20s, ease-out).
- 0.10s Faction tabs + filters fade/slide from left (0.18s).
- 0.12s **Card grid staggers in**: each card scale 0.92→1.0 + fade, 0.018s stagger left-to-right, top row then
  bottom row; ease-out-back (slight overshoot 1.02). Full grid settled by ~0.40s.
- 0.20s Detail panel slides in from right 24px + fade (0.22s).
- 0.30s **Stat bars fill** left→right (0.45s ease-out) to current value; the "+N" preview segment pulses once.
- 0.34s Tier nodes pop sequentially up to current (0.04s each), current node glow-ring scales 1.0→1.15→1.05.
- 0.38s CTA fades in with a single bloom sweep.

**OnSelectCard (~0.25s):** previously-selected halo fades (0.12s); new card halo expands 0→1 (0.15s ease-out);
detail panel content cross-fades (name/role/portrait swap 0.18s); stat bars re-fill to the new unit (0.40s);
tier nodes re-pop.

**OnUpgrade (~0.6s):** CTA flash → coin chip decrements with a -200 tick; affected stat bar(s) extend by the
"+N" amount with a green flash (0.35s); LEVEL number ticks 12→13 with a scale pop; current tier node advances
(connector fills, node slides) ; small gold sparkle burst at the node. If hold-repeat, loop at ~0.25s cadence.

**OnFactionSwap (~0.35s):** grid cards fade/slide out left (0.15s), new faction's cards stagger in (0.20s);
tabs swap fill color.

---

## J — PARTICLE & FX
- **Selected card:** soft cobalt halo + 2–3 slow rising spark motes; faint pulsing (period ~2s).
- **Rarity shimmer:** legendary/epic cards get a slow diagonal specular sweep across the frame (~6s loop);
  rarer = brighter sweep. Common = none.
- **Detail portrait:** gentle volumetric backlight + 1–2 dust motes drifting.
- **Tier current node:** continuous soft gold glow-ring pulse.
- **CTA idle:** subtle bloom breathing; on press, a quick gold spark ring.
- **Upgrade success:** gold sparkle burst at the stat bar + node; brief screen-edge warm flash (very subtle).
- **Currency spend:** small coin-poof at the chip.
Budget: pooled, capped; disable rarity sweeps on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load roster for current faction (default Iron Pact); request balances (read-only); select first
  owned unit (Shieldman) → populate detail; run enter timeline.
- **OnFactionTab(faction):** swap roster + reset filter to All + reselect first owned + refresh counter.
- **OnFilter(class):** filter grid; keep selection if still visible else select first match.
- **OnSelectCard(unit):** populate detail (name, role, portrait, level, stats, tier row, upgrade cost,
  upgradable arrows); play select anim.
- **OnPrev/NextUnit:** move selection within current filtered list (wraps); same as select.
- **OnUpgrade (tap):** if affordable & not maxed → send upgrade request (server-auth); on success apply stat
  deltas, level++, tier advance, decrement Gold; else disabled-shake + toast. **Hold:** repeat while held and
  affordable.
- **OnInfo (ⓘ):** open unit stat-breakdown tooltip/sheet.
- **OnPlus (currency chip):** route to Store (Gold/Gems purchase).
- **OnRarity:** open rarity legend/sort. **OnCollection:** collection/progress view. **OnDisenchant:** open
  multi-select disenchant → materials (server-auth).
- **OnBack:** pop screen → Main Menu.
- **§12:** all upgrade/disenchant are server-authoritative requests; UI only displays and requests. No local
  balance mutation, no ECS write.

---

## L — NEGATIVE RULES
- Do **not** invent unit stats, costs, or rarities beyond what's drawn (Shieldman Lv12/20, Health 3,240 +180,
  Damage 210 +12, Speed 86 +4, Upgrade 200, count 125/300, Collected 48/58, currencies 12,450/1,850/2,340).
- Do **not** add a 6th currency, a search box, sort dropdowns, or extra tabs not shown.
- Do **not** add stick figures or real brand text. No portrait-orientation variant.
- Do **not** change grid to anything but 5-wide on the reference canvas (rows may scroll).
- Do **not** let UI mutate balances/ECS; upgrades are requests only.
- Do **not** recolor faction identity (Iron Pact cobalt, Ashen oxblood) or rarity tints.
- Canon note: BULWARK canon = 12 units; this screen draws **58 collected-of total** and 10 visible cards →
  spec exactly as drawn and flag the roster-count discrepancy to design (do not "correct" it here).

---

## M — ACCEPTANCE CRITERIA (≥95% fidelity)
1. 5×2 visible card grid with correct cell aspect, gaps, rarity frames, level badges, count bars, and the ▲
   upgradable arrow on the right units.
2. Shieldman card shows the **cobalt selected halo**; detail panel mirrors Shieldman exactly.
3. Header: ARMY title left, three currency chips (silver/gold/gems) with +buttons, correct values, right-pinned.
4. Faction tabs (Iron Pact selected blue / Ashen oxblood) + filter strip (All + 5 class chips) + "Collected
   48/58".
5. Detail: name, role, portrait with prev/next chevrons, LEVEL 12/20 + ⓘ, three stat bars with exact values
   and green "+N" previews, Upgrade Progress nodes 10–14 (12 current/highlighted), UPGRADE — 200 CTA, hold hint.
6. Bottom bar Rarity/Collection/Disenchant.
7. Stat-bar fill, card stagger, select cross-fade, and upgrade tick animations present.
8. Safe-area inset honored; BG full-bleed; layout holds on 4:3, 19.5:9, 21:9, and notched landscape.
9. All hex/typography within the ranges in F/G; CTA is the brightest interactive element after the selected card.

## N — IMPLEMENTATION CONFIDENCE
**90/100.** High confidence on layout, the 5×2 grid, detail panel, stats, tier nodes, CTA, and color identity
(all clearly legible). Minor uncertainty: exact pixel radii/bevel widths, the precise class icons in the filter
strip (read as shield/swords/bow/magic/special), whether locked roster entries render identically to the 10
shown, and the exact "+N" fill rendering (separate cap vs ghost segment). The 58-unit roster vs 12-unit canon
is a design discrepancy, not a fidelity blocker.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present and substantive, in order.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 22 · Units / Army".
- [x] Fraction-based layout normalized to 2340×1080; grid cell sizes + gaps given.
- [x] ScrollRect + GridLayoutGroup specified for the roster.
- [x] States for cards/tabs/filters/tier-nodes/CTA (idle/hover/pressed/disabled/selected/owned/locked).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; no invented numbers; discrepancy flagged.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — no code/assets/scenes; only this .md written.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 23 · Commander Select

Source: design/CommanderSelectDesign.png · 1672×941 (1.78:1) · Analysis-only forensic spec.

> Normalize the source 1672×941 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" values are on-canvas sizes at 1080-tall.

---

## A — SCREEN PURPOSE
The **Commander Select** meta screen — a symmetric **two-commander face-off** chooser. The player picks the
commander that leads their army:
- **LEFT:** Iron Pact — **WARDEN**, "Lord of Stone and Steel".
- **RIGHT:** Ashen Horde — **WARCHIEF**, "Scourge of the Burning Wastes".
- A central **VS** badge separates the two mirrored panels.

Each panel shows the faction crest, faction + commander names, full-body commander art, an **ACTIVE** ability
card and a **PASSIVE** ability card (each with name, description, and—for active—a cooldown line), a
**Commander Level** row with an XP progress bar, and a faction-colored **SELECT** button.

Purpose: communicate each commander's identity + ability kit and let the player commit a choice (server-auth).
Reached from: Main Menu / pre-match flow. Back: top-left chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** epic duel / "choose your champion" — a dark hall split into a **cool cobalt half (left)** and a
  **hot ember half (right)**, the two commanders facing each other across a glowing VS.
- **Split lighting:** left panel lit cool blue (Iron Pact), right panel lit warm orange/red (Ashen); a
  vertical seam of light/embers down the center behind VS.
- **Palette anchors:**
  - BG: near-black `#0a0b0f`; left half cooled toward `#0c1430`, right half warmed toward `#2a0f0c`.
  - Iron Pact chrome/SELECT: cobalt `#1d3a8a → #2b56c8 → #4f8bff`, cyan rim `#7fb0ff`, steel-gold frame.
  - Ashen chrome/SELECT: oxblood/ember `#5a1712 → #7a1f1a → #b5311f`, ember rim `#f0742c`.
  - Gold ornament (title, frames, VS, level numerals): `#6b5320 → #caa04a → #f0d27a`.
  - Parchment/stone ability cards: dark slate `#12141b` wells with gold trim; XP bar fill faction-tinted.
- **Hierarchy:** SELECT COMMANDER title → two commander portraits → VS → ability cards → SELECT buttons →
  currency chips/back.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
CommanderSelectScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_LeftCoolWash (Image, left half cobalt tint)
│  ├─ BG_RightEmberWash (Image, right half ember tint)
│  ├─ BG_CenterSeam (Image, vertical light/ember column behind VS)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ Header
│  │  ├─ Btn_Back (Button) [chevron] top-left
│  │  ├─ Lbl_Title "SELECT COMMANDER" (Text, serif gold) top-center
│  │  ├─ Lbl_Subtitle "Choose your commander. Each leads an army with unique abilities." (Text)
│  │  └─ CurrencyChips (HorizontalLayoutGroup) top-right
│  │     ├─ Chip_Gold   [gold-coin | "12,450" | Btn_Plus]
│  │     └─ Chip_Silver [silver-coin | "1,280" | Btn_Plus]
│  ├─ CommanderPanel_Left (Iron Pact / Warden) [framed]
│  │  ├─ Panel_Frame (Image, gold/cobalt ornate frame)
│  │  ├─ Crest_Faction (Image, Iron Pact lion crest) top-left of panel
│  │  ├─ Lbl_FactionName "IRON PACT" (Text)
│  │  ├─ Lbl_CommanderName "WARDEN" (Text, large serif)
│  │  ├─ Lbl_CommanderEpithet "Lord of Stone and Steel" (Text)
│  │  ├─ Art_Commander (Image, full-body Warden render)
│  │  ├─ AbilityCol (VerticalLayoutGroup)
│  │  │  ├─ AbilityCard_Active
│  │  │  │  ├─ Lbl_Kind "ACTIVE"
│  │  │  │  ├─ Icon_Ability (Image)
│  │  │  │  ├─ Lbl_AbilityName "RALLY"
│  │  │  │  ├─ Lbl_AbilityDesc "Inspire your troops, increasing their Attack and Defense for a short duration."
│  │  │  │  └─ Lbl_Cooldown "⏱ 60s Cooldown"
│  │  │  └─ AbilityCard_Passive
│  │  │     ├─ Lbl_Kind "PASSIVE"
│  │  │     ├─ Icon_Ability (Image)
│  │  │     ├─ Lbl_AbilityName "QUARTERMASTER"
│  │  │     └─ Lbl_AbilityDesc "Reduces resource cost of any upgrades and increases Silver income."
│  │  ├─ LevelRow
│  │  │  ├─ Lbl_LevelTag "COMMANDER LEVEL"
│  │  │  ├─ Badge_Level "12"
│  │  │  └─ XPBar (Slider) [Fill + Lbl "x / x,x00 XP"]
│  │  └─ Btn_Select_Left (Button, cobalt) "SELECT"
│  ├─ VSBadge (Image + Text "VS") center, between panels
│  └─ CommanderPanel_Right (Ashen Horde / Warchief) [mirror of Left]
│     ├─ Panel_Frame (gold/oxblood frame)
│     ├─ Crest_Faction (Ashen skull/horde crest) top-right
│     ├─ Lbl_FactionName "ASHEN HORDE"
│     ├─ Lbl_CommanderName "WARCHIEF"
│     ├─ Lbl_CommanderEpithet "Scourge of the Burning Wastes"
│     ├─ Art_Commander (Warchief render)
│     ├─ AbilityCol
│     │  ├─ AbilityCard_Active [ACTIVE | "WAR ROAR" | "Unleash a war roar, boosting your army's Attack and Spend while intimidating enemies." | "⏱ 60s Cooldown"]
│     │  └─ AbilityCard_Passive [PASSIVE | "BLOOD FORGE" | "Increases troop Health and reduces training time for melee units."]
│     ├─ LevelRow [ "COMMANDER LEVEL" | "9" | XPBar "1,450 / 4,000 XP" ]
│     └─ Btn_Select_Right (Button, oxblood) "SELECT"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | width grows |
| BG_LeftCoolWash | BG_FullBleed | 0 | Image | left-stretch (50%) | 0,.5 | — | full-bleed | follows seam |
| BG_RightEmberWash | BG_FullBleed | 1 | Image | right-stretch (50%) | 1,.5 | — | full-bleed | follows seam |
| BG_CenterSeam | BG_FullBleed | 2 | Image | mid-stretch (vertical) | .5,.5 | — | full-bleed | centered |
| BG_Vignette | BG_FullBleed | 3 | Image(mult) | stretch | .5,.5 | — | full-bleed | corners |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| Lbl_Title | Header | 1 | Text | top-center | .5,1 | center | inside | centered |
| Lbl_Subtitle | Header | 2 | Text | top-center (below title) | .5,1 | center | inside | centered |
| CurrencyChips | Header | 3 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-pinned |
| CommanderPanel_Left | SafeAreaRoot | 1 | Image(framed) | left-stretch | 0,.5 | — | inside | ~46% width, left-anchored |
| CommanderPanel_Right | SafeAreaRoot | 3 | Image(framed) | right-stretch | 1,.5 | — | inside | ~46% width, right-anchored |
| VSBadge | SafeAreaRoot | 2 | Image+Text | mid-center | .5,.5 | center | inside | centered, fixed |
| (per panel) Panel_Frame | Panel | 0 | Image | stretch | .5,.5 | — | inside | scales |
| Crest_Faction | Panel | 1 | Image | top-inner-corner | corner | — | inside | fixed |
| Lbl_FactionName | Panel | 2 | Text | top (beside crest) | side | side | inside | — |
| Lbl_CommanderName | Panel | 3 | Text | top (below faction) | side | side | inside | shrink to fit |
| Lbl_CommanderEpithet | Panel | 4 | Text | top (below name) | side | side | inside | — |
| Art_Commander | Panel | 5 | Image | panel-fill (behind cards) | .5,.5 | — | inside | aspect-locked, may bleed |
| AbilityCol | Panel | 6 | VerticalLayoutGroup | inner column | center | — | inside | fixed inner width |
| AbilityCard_Active/Passive | AbilityCol | 0,1 | Image(framed) | stretch-x | .5,.5 | — | inside | flex height |
| LevelRow | Panel | 7 | RectTransform | bottom (above SELECT) | .5,0 | center | inside | spans inner |
| Badge_Level | LevelRow | 0 | Image+Text | left | 0,.5 | center | inside | fixed |
| XPBar | LevelRow | 1 | Slider(no handle) | right of badge | 0,.5 | mid | inside | flex width |
| Btn_Select | Panel | 8 | Button | bottom-center | .5,0 | center | inside | wide CTA |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Header:** y 0.90 → 1.00.
- Btn_Back: center x 0.030, y 0.955; box ≈0.045w×0.085h.
- Lbl_Title "SELECT COMMANDER": centered x 0.50, baseline y 0.955; cap ≈ 0.055h (≈59px@1080); wide tracking.
- Lbl_Subtitle: centered x 0.50, y 0.905; ≈22px@1080.
- CurrencyChips: right edge x 0.972, y 0.955; two chips ≈0.115w×0.052h, gap 0.010w. **Only two** (Gold, Silver).

**Two panels (mirror):**
- Left panel region: x 0.018 → 0.475 (width ≈0.457w ≈1069px@1080), y 0.060 → 0.890.
- Right panel region: x 0.525 → 0.982 (same width), y 0.060 → 0.890.
- VS gap: x 0.475 → 0.525 (center column ≈0.05w).

**Within a panel (Left as reference; Right mirrors horizontally):**
- Crest_Faction: top-inner corner, x≈0.040, y≈0.860, Ø≈0.045w.
- Lbl_FactionName "IRON PACT": x≈0.090 (right of crest), y≈0.870; ≈26px@1080.
- Lbl_CommanderName "WARDEN": x≈0.090, baseline y≈0.835; large serif cap ≈0.052h (≈56px@1080).
- Lbl_CommanderEpithet: x≈0.090, y≈0.805; ≈20px italic.
- Art_Commander: occupies the panel's **left ~45%** (Warden render), region x 0.030→0.230, y 0.180→0.840;
  on Right panel the Warchief art occupies the panel's **right ~45%** (mirror). Art may bleed under cards.
- **AbilityCol** (inner cards, on the inner side of each panel, toward VS):
  - Left panel cards occupy x 0.250 → 0.460, two stacked cards.
  - Card_Active: y 0.620 → 0.790 (height ≈0.170h ≈184px@1080). Card_Passive: y 0.430 → 0.595.
  - Card internals: "ACTIVE"/"PASSIVE" kicker top-left (≈18px gold), ability icon ≈0.055w square at left,
    ability name (≈30px serif) to the right of icon, description (≈19px, 2–3 lines wrapped) below,
    cooldown line "⏱ 60s Cooldown" at card bottom (active only, ≈18px).
- **LevelRow:** y 0.330 → 0.380, spans inner area x 0.090→0.460.
  - Lbl_LevelTag "COMMANDER LEVEL" centered above the bar (y≈0.385, ≈18px tracked).
  - Badge_Level (circle Ø≈0.045w) at left containing "12"; XPBar to its right, x≈0.175→0.460, height ≈0.028h
    (≈30px), gold/cobalt fill, centered text "x / x,x00 XP".
- **Btn_Select:** y 0.250 → 0.320 (height ≈0.070h ≈76px), centered in panel inner width (x≈0.150→0.430),
  faction-colored, label "SELECT" ≈32px white.

**VS badge:** centered x 0.50, y≈0.560; Ø≈0.075w (≈175px@1080), gold ring with "VS" serif.

**Tablet (4:3):** panels widen toward center, VS gap shrinks; keep both panels symmetric, art may clip — keep
ability cards + SELECT fully visible (priority). **Ultrawide (21:9):** extra width widens the VS gutter; keep
panels anchored to their sides, BG washes fill. **Notch:** SafeAreaRoot insets; BG full-bleed under cutout;
back + chips clear of cutout. The seam/VS stays screen-center.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "SELECT COMMANDER" | serif display, regal | Black | UPPER | +8% | gold bevel + bloom + dark drop | 59 | `#f0d27a`→`#caa04a` |
| Subtitle | clean sans, light | Regular | Sentence | +4% | subtle | 22 | `#c9b27a` |
| Currency values | tabular sans | Semibold | — | tabular | 1px stroke | 30 | `#f4e9c8` |
| Faction name "IRON PACT"/"ASHEN HORDE" | strong sans | Bold | UPPER | +6% | drop-shadow | 26 | IP `#9fc0ff`; Ashen `#f0a070` |
| Commander name "WARDEN"/"WARCHIEF" | serif display | Black | UPPER | +5% | heavy gold bevel + bloom | 56 | `#f0d27a`→`#caa04a` |
| Epithet | serif italic, light | Regular | Title | +3% | subtle | 20 | `#cbb98a` |
| "ACTIVE"/"PASSIVE" kicker | sans | Semibold | UPPER | +10% | tinted | 18 | `#caa04a` |
| Ability name (RALLY/QUARTERMASTER/WAR ROAR/BLOOD FORGE) | serif/strong sans | Bold | UPPER | +4% | gold | 30 | `#f0d27a` |
| Ability description | sans, light | Regular | Sentence | +2% | — | 19 | `#cdd2da` |
| Cooldown "⏱ 60s Cooldown" | sans | Medium | Sentence | +2% | clock icon | 18 | `#b9c0c8` |
| "COMMANDER LEVEL" | sans | Medium | UPPER | +10% | dim | 18 | `#9a8a5a` |
| Level numeral (12/9) | tabular serif | Bold | — | — | gold glow | 36 | `#fff4d6` |
| XP "1,450 / 4,000 XP" | tabular sans | Semibold | — | tabular | over-bar | 20 | `#f0e8cf` |
| "SELECT" | strong sans | Bold | UPPER | +6% | white + dark stroke; faction glow | 32 | `#ffffff` |

---

## G — MATERIALS
- **BG:** charcoal base; left half a cool cobalt gradient wash, right half a warm ember wash; central seam =
  bright vertical light shaft (left side) blending to drifting embers (right side); strong corner vignette.
- **Panel frames:** ornate brushed gold/bronze (`#6b5320`→`#f0d27a`) with engraved filigree; the **left**
  frame carries cobalt inner glow `#4f8bff`, the **right** frame carries ember inner glow `#f0742c`.
- **Commander art:** painterly high-detail; Warden = polished steel armor + blue cloth, rim-lit cobalt;
  Warchief = blackened plate + ember/oxblood, rim-lit orange.
- **Ability cards:** dark slate wells `#12141b` with thin gold trim and a faint inner vignette; ability icon
  in a small recessed gold-rimmed frame.
- **XP bar:** dark groove `#1b1d24`; fill faction-tinted glassy (left cobalt, right ember/gold) with top
  specular; partial fill (~0.36 for Warchief = 1450/4000).
- **VS badge:** cast-gold ring with beveled "VS", soft bloom, slight metallic specular; sits over the seam.
- **SELECT buttons:** left = cobalt enamel (`#2b56c8`, lighter top edge, dark bottom, gold trim);
  right = oxblood/ember enamel (`#7a1f1a`→`#b5311f`, gold trim). Both rounded ~14px, white label, idle glow.

---

## H — COMPONENTS (states)
**CommanderPanel:** a selectable region; hover/focus brightens its frame glow and slightly lifts the art.
On a device with a single committed commander, the **currently-equipped** commander's panel shows an "EQUIPPED"
ribbon/check and its SELECT reads "SELECTED" (disabled/owned style).

**AbilityCard:**
- *idle:* shown as drawn.
- *hover:* gold trim brightens, icon glows; tooltip may expand full description.
- Active card always shows the cooldown line; passive card omits it.

**XPBar:** read-only progress; on level-up, animates fill to full then resets with level numeral tick.

**Btn_Select (per faction):**
- *idle:* faction enamel, white "SELECT", soft glow.
- *hover:* +10% brightness, glow grows, slight scale 1.02.
- *pressed:* scale 0.97, inner flash.
- *selected/owned:* label "SELECTED", check mark, locked-bright (this commander is active) — the other panel's
  SELECT returns to idle.
- *locked (commander not yet unlocked):* desaturated, 🔒, label "LOCKED" / "Level N to unlock"; tap → toast.
- *insufficient (if a cost gates it):* disabled grey; tap → shake + toast. (No explicit cost shown on the
  buttons in the mockup → default: free selection of an unlocked commander.)

**Back button:** standard; hover brighten, pressed scale.

---

## I — ANIMATION TIMELINE
**OnShow (~0.6s):**
- 0.00s BG washes + seam fade in (0.25s); embers begin drifting on the right.
- 0.05s Header (title scale 0.96→1.0 + fade, subtitle fade) 0.22s.
- 0.12s **Left panel slides in from left** (24px) + fade (0.25s ease-out); Warden art settles.
- 0.12s **Right panel slides in from right** (24px) + fade (0.25s) — symmetric.
- 0.30s VS badge pops (scale 0→1.15→1.0, 0.20s ease-out-back) + bloom flash.
- 0.34s Ability cards fade/slide up within each panel (0.18s, active then passive, 0.05s stagger).
- 0.42s XP bars fill left→right to current value (0.40s ease-out).
- 0.48s SELECT buttons fade in with a single glow sweep.

**OnHoverPanel:** frame glow +20%, art lifts 4px, ability icons shimmer (0.15s).

**OnSelect(faction):** chosen panel pulses faction glow, SELECT → "SELECTED" with a check pop (0.25s); other
panel's SELECT dims to idle; brief confirm bloom on the chosen crest. If selection commits + routes onward,
short fade-out (0.25s).

**Idle ambient:** VS badge faint pulse; embers loop on the right; cobalt motes loop on the left.

---

## J — PARTICLE & FX
- **Center seam:** continuous light-shaft shimmer (left) + rising embers (right) meeting at VS.
- **VS badge:** soft pulsing gold bloom; on hover of either panel, a faint spark toward that side.
- **Warden side:** slow cobalt dust motes + steel rim glints.
- **Warchief side:** ember sparks + heat-shimmer near the art base.
- **Ability icons:** subtle idle shimmer; active-ability icon has a faint cooldown-clock tick motif.
- **Select confirm:** faction-colored sparkle burst around the chosen SELECT + crest.
Budget pooled/capped; reduce ember/mote counts on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load both commanders' data (names, abilities, level, XP) read-only; mark currently-equipped if
  any; run enter timeline.
- **OnHoverPanel(side):** highlight; optional expand ability tooltips.
- **OnAbilityCard tap:** open detailed ability tooltip/sheet (full numbers if available).
- **OnSelect(side):** if commander unlocked → send "set active commander" request (server-auth); on success
  mark SELECTED + update other panel; if locked → toast unlock requirement; if gated by cost → confirm modal.
- **OnPlus (chip):** route to Store.
- **OnBack:** pop → previous screen (Main Menu / pre-match).
- **§12:** selection is a server-authoritative meta write request; UI never mutates state locally; no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn strings: Warden "Lord of Stone and Steel"; abilities RALLY (Active, 60s Cooldown,
  "Inspire your troops, increasing their Attack and Defense for a short duration.") and QUARTERMASTER (Passive,
  "Reduces resource cost of any upgrades and increases Silver income."); Warchief "Scourge of the Burning
  Wastes"; WAR ROAR (Active, 60s Cooldown, "Unleash a war roar, boosting your army's Attack and Spend while
  intimidating enemies.") and BLOOD FORGE (Passive, "Increases troop Health and reduces training time for
  melee units."). Warden Level 12; Warchief Level 9 (1,450 / 4,000 XP). Currencies 12,450 (gold) / 1,280
  (silver).
- Only **two** currency chips here (Gold + Silver) — do not add Gems.
- Do not add a third commander, extra ability slots, or a cost label not drawn.
- Keep the left=cobalt / right=ember split; do not swap faction colors.
- No portrait variant, no stick figures, no real brand text.
- No local balance/ECS mutation; SELECT is a request only.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Symmetric two-panel layout with a centered VS badge over a cool/warm split background.
2. Left = Iron Pact WARDEN (cobalt), Right = Ashen Horde WARCHIEF (ember), correct crests, names, epithets.
3. Each panel: ACTIVE + PASSIVE ability cards with the exact names/descriptions; active cards show "60s
   Cooldown".
4. Commander Level rows with badge (12 / 9) and XP bars (Warchief 1,450 / 4,000 XP), faction-tinted fill.
5. Faction-colored SELECT buttons (cobalt / oxblood), brightest interactive elements.
6. Header: SELECT COMMANDER title + subtitle centered, two currency chips top-right, back top-left.
7. Enter stagger, VS pop, XP fill, and select-confirm animations present.
8. Safe-area honored; BG full-bleed; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges.

## N — IMPLEMENTATION CONFIDENCE
**91/100.** Strong confidence: symmetric layout, ability cards, level/XP rows, SELECT buttons, split lighting,
and all visible copy are clearly legible. Minor uncertainty: exact ability-icon art, precise frame filigree,
whether selecting costs anything (none shown → assumed free for unlocked), and the exact Warden XP fraction
(its bar reads near-full but the numerator is partly obscured — Warchief's 1,450/4,000 is clear).

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 23 · Commander Select".
- [x] Fraction-based layout normalized to 2340×1080; symmetric panel math given.
- [x] Ability cards, level/XP bars, SELECT states (idle/hover/pressed/selected/owned/locked/disabled).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; nothing invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 24 · Profile

Source: design/ProfileScreenDesign.png · 1783×882 (2.02:1) · Analysis-only forensic spec.

> Normalize the source 1783×882 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" = on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Player Profile** meta screen. Three columns under a centered "PROFILE" title:
1. **Left vertical tab rail:** Overview (selected) · Heroes · Match History · Stats · Achievements ·
   Customization, with the faction crest anchored at the rail's bottom.
2. **Center identity column:** large framed avatar portrait, a **level badge "45"** on the portrait base, an
   **XP bar** ("x,xxx / x,000 XP"), the player name **THALRION**, and a clan badge **SILVERWARDENS — "Knights
   of the Realm"**.
3. **Right content (Overview tab):** three top **stat blocks** — **BATTLES 1,248**, **WINS 842**, **WIN RATE
   67.5%** — an **Equipped** row of **5 cosmetic slots** (Galvanhelm, Lionheart Plate, Dawnbreaker, Royal
   Cloak, Warden's Banner — each "Epic"), a **Title** panel ("REALM CHAMPION"), and a footer with **Player ID
   #7A4B3C9E** (copy button) and **Joined 2024-01-15**.

Purpose: present player identity, progression, lifetime stats, equipped cosmetics, and account metadata; the
tab rail switches the right content. Reached from: Main Menu / avatar tap. Back: top-left double-chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** prestige character sheet / hall-of-honor. Dark stone hall, warm gold framing, a heroic backlit
  avatar; calm and stately (less "combat", more "trophy room").
- **Palette anchors:**
  - BG: obsidian `#0a0b0f → #14161e`, faint stone-hall texture, vignette.
  - Gold chrome (title, frames, stat icons, dividers): `#6b5320 → #caa04a → #f0d27a`.
  - Iron Pact cobalt accents (selected tab, clan crest, XP fill): `#2b56c8 → #4f8bff`.
  - Slot wells / panels: dark slate `#101218 → #161922` with gold trim.
  - Rarity "Epic" label/edge: violet `#9e6bf0`.
  - Win-rate / positive numerals: warm gold `#f0d27a`; neutral text `#cdd2da`.
- **Hierarchy:** PROFILE title → avatar + name → stat blocks → equipped slots → title/ID footer → tab rail.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
ProfileScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_StoneHall (Image, dark stone texture)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ Header
│  │  ├─ Btn_Back (Button) [double-chevron] top-left
│  │  └─ Lbl_Title "PROFILE" (Text, serif gold) top-center
│  ├─ TabRail (VerticalLayoutGroup, left)  [framed rail]
│  │  ├─ Tab_Overview (Toggle, SELECTED) [icon | "Overview"]
│  │  ├─ Tab_Heroes (Toggle) [icon | "Heroes"]
│  │  ├─ Tab_MatchHistory (Toggle) [icon | "Match History"]
│  │  ├─ Tab_Stats (Toggle) [icon | "Stats"]
│  │  ├─ Tab_Achievements (Toggle) [icon | "Achievements"]
│  │  ├─ Tab_Customization (Toggle) [icon | "Customization"]
│  │  └─ Crest_Faction (Image, Iron Pact crest at rail bottom)
│  ├─ IdentityColumn (center)
│  │  ├─ Avatar_Frame (Image, ornate portrait frame)
│  │  │  ├─ Avatar_Portrait (Image, hero render, masked)
│  │  │  └─ Badge_Level (Image circle + Text "45") at frame base
│  │  ├─ XPBar (Slider) [Fill + Lbl "x,xxx / x,000 XP"]
│  │  ├─ Lbl_PlayerName "THALRION" (Text, large serif gold)
│  │  └─ ClanBadge
│  │     ├─ Clan_Crest (Image)
│  │     ├─ Lbl_ClanName "SILVERWARDENS"
│  │     └─ Lbl_ClanMotto "Knights of the Realm"
│  └─ ContentPane_Overview (right)
│     ├─ StatBlocks (HorizontalLayoutGroup, 3)
│     │  ├─ Stat_Battles  [icon crossed-swords | "BATTLES" | "1,248"]
│     │  ├─ Stat_Wins     [icon laurel/wreath  | "WINS"    | "842"]
│     │  └─ Stat_WinRate  [icon banner         | "WIN RATE"| "67.5%"]
│     ├─ EquippedSection
│     │  ├─ Lbl_EquippedTitle "Equipped"
│     │  └─ EquippedSlots (HorizontalLayoutGroup, 5)
│     │     ├─ Slot_Helm    [item img | "Galvanhelm"     | "Epic"]
│     │     ├─ Slot_Armor   [item img | "Lionheart Plate"| "Epic"]
│     │     ├─ Slot_Weapon  [item img | "Dawnbreaker"    | "Epic"]
│     │     ├─ Slot_Cloak   [item img | "Royal Cloak"    | "Epic"]
│     │     └─ Slot_Banner  [item img | "Warden's Banner"| "Epic"]
│     └─ FooterRow
│        ├─ TitlePanel [ Lbl "Title" | Value "REALM CHAMPION" ]
│        ├─ Lbl_PlayerIDTag "Player ID"
│        ├─ Lbl_PlayerIDVal "#7A4B3C9E"
│        ├─ Btn_CopyID (Button, copy icon)
│        ├─ Lbl_JoinedTag "Joined"
│        └─ Lbl_JoinedVal "2024-01-15"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | grows |
| BG_StoneHall | BG_FullBleed | 0 | Image | stretch | .5,.5 | — | full-bleed | tile-safe |
| BG_Vignette | BG_FullBleed | 1 | Image(mult) | stretch | .5,.5 | — | full-bleed | corners |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| Lbl_Title | Header | 1 | Text | top-center | .5,1 | center | inside | centered |
| TabRail | SafeAreaRoot | 1 | VerticalLayoutGroup | left-stretch | 0,.5 | top | inside | fixed width, full height |
| Tab_* | TabRail | 0..5 | Toggle (group) | stretch-x | .5,1 | left | inside | uniform row |
| Crest_Faction | TabRail | 6 | Image | bottom-center | .5,0 | center | inside | fixed |
| IdentityColumn | SafeAreaRoot | 2 | RectTransform | left-center (right of rail) | .5,.5 | center | inside | fixed fraction |
| Avatar_Frame | IdentityColumn | 0 | Image | top-center | .5,1 | center | inside | aspect-locked |
| Avatar_Portrait | Avatar_Frame | 0 | Image(masked) | stretch(inset) | .5,.5 | — | inside | scales |
| Badge_Level | Avatar_Frame | 1 | Image+Text | bottom-center | .5,0 | center | inside | fixed |
| XPBar | IdentityColumn | 1 | Slider(no handle) | center (below frame) | .5,1 | mid | inside | column width |
| Lbl_PlayerName | IdentityColumn | 2 | Text | center (below XP) | .5,1 | center | inside | shrink to fit |
| ClanBadge | IdentityColumn | 3 | RectTransform | center (below name) | .5,1 | center | inside | — |
| ContentPane_Overview | SafeAreaRoot | 3 | RectTransform | right-stretch | 1,.5 | — | inside | flex width, right-anchored |
| StatBlocks | ContentPane | 0 | HorizontalLayoutGroup | top-stretch | .5,1 | center | inside | 3 equal blocks |
| Stat_* | StatBlocks | 0..2 | RectTransform(icon+2 Text) | — | — | center | inside | equal |
| EquippedSection | ContentPane | 1 | RectTransform | center-stretch | .5,.5 | — | inside | full width |
| Lbl_EquippedTitle | EquippedSection | 0 | Text | top-center | .5,1 | center | inside | — |
| EquippedSlots | EquippedSection | 1 | HorizontalLayoutGroup | center-stretch | .5,.5 | center | inside | 5 equal slots |
| Slot_* | EquippedSlots | 0..4 | RectTransform(img+2 Text) | — | — | center | inside | uniform |
| FooterRow | ContentPane | 2 | RectTransform | bottom-stretch | .5,0 | center | inside | spans |
| TitlePanel | FooterRow | 0 | RectTransform | bottom-center | .5,0 | center | inside | centered panel |
| Lbl_PlayerID* / Btn_CopyID | FooterRow | 1..3 | Text/Button | bottom-right | 1,0 | right | inside | right cluster |
| Lbl_Joined* | FooterRow | 4,5 | Text | bottom-right (below ID) | 1,0 | right | inside | right cluster |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Header:** y 0.91 → 1.00.
- Btn_Back: center x 0.030, y 0.955; box ≈0.045w×0.085h (double-chevron «).
- Lbl_Title "PROFILE": centered x 0.50, baseline y 0.955; cap ≈ 0.055h (≈59px@1080), wide tracking.

**Left tab rail:** x 0.016 → 0.215 (width ≈0.199w ≈466px@1080), y 0.060 → 0.900.
- 6 tab rows top-aligned, each ≈0.085h tall (≈92px), pitch ≈0.105h. Row = icon (Ø≈0.030w) at left +
  label. Selected (Overview) = cobalt-filled pill + lit icon + small left active-marker.
- Crest_Faction at rail bottom, centered, Ø≈0.085w, y≈0.085.

**Center identity column:** x 0.230 → 0.520 (width ≈0.290w ≈679px@1080), vertically centered.
- Avatar_Frame: centered x 0.375, top y≈0.880; frame ≈0.185w × 0.420h (≈433×454@1080), ornate gold portrait
  frame (slightly arched top). Portrait inset ~0.07·frame.
- Badge_Level "45": at frame base center, x 0.375, y≈0.470; circle Ø≈0.060w (≈140px), gold ring, "45" inside.
- XPBar: x 0.270→0.480, y≈0.430 (height ≈0.024h ≈26px), cobalt fill, centered "x,xxx / x,000 XP".
- Lbl_PlayerName "THALRION": centered x 0.375, baseline y≈0.380; large serif cap ≈0.055h (≈59px).
- ClanBadge: centered x 0.375, y≈0.300; pill with clan crest (left) + "SILVERWARDENS" (≈26px) over
  "Knights of the Realm" (≈18px).

**Right content pane (Overview):** x 0.535 → 0.984 (width ≈0.449w ≈1050px@1080), y 0.075 → 0.900.
- **StatBlocks:** top band y 0.770 → 0.890. Three equal blocks across the pane (each ≈0.140w, gap 0.014w).
  Each block: gold icon top (Ø≈0.040w), label ("BATTLES"/"WINS"/"WIN RATE") ≈18px, big value
  ("1,248"/"842"/"67.5%") ≈40px below.
- **EquippedSection:** "Equipped" title centered at y≈0.700 (≈24px). Slots band y 0.500 → 0.680.
  Five square slots across the pane: slotW = (0.449 − 4·0.014)/5 = 0.0786w ≈ **184px@1080**, square-ish
  (height ≈0.150h ≈162px for the art well) + label rows below. Each slot: item render in a gold-rimmed dark
  well, item name beneath (≈18px, e.g. "Galvanhelm"), rarity "Epic" (≈16px violet) beneath that.
- **FooterRow:** y 0.110 → 0.260.
  - TitlePanel: centered horizontally in the pane, y≈0.230; a wide parchment/gold pill with small "Title"
    kicker above and value "REALM CHAMPION" (≈30px serif gold) inside; pill ≈0.30w × 0.075h.
  - Player ID cluster: right-aligned, y≈0.230 (or split): "Player ID" tag (≈18px) + "#7A4B3C9E" value
    (≈26px mono) + copy icon button; below it "Joined" tag + "2024-01-15" value (≈22px). (Reads as a
    right-side two-line metadata block beside the centered Title pill.)

**Tablet (4:3):** pane narrows — equipped slots may wrap to 5-across still (priority) but stat blocks can stack
2+1 if width < threshold; keep rail fixed width. **Ultrawide (21:9):** extra width pads between identity column
and content pane; keep rail left, content right. **Notch:** SafeAreaRoot insets; BG full-bleed under cutout;
back/title and rail clear of cutout.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "PROFILE" title | serif display | Black | UPPER | +8% | gold bevel + bloom + drop | 59 | `#f0d27a`→`#caa04a` |
| Tab labels | sans | Medium | Title | +2% | selected bright | 24 | sel `#eaf2ff`; idle `#c9b27a` |
| Level "45" | tabular serif | Black | — | — | gold glow + dark stroke | 48 | `#fff4d6` |
| XP "x,xxx / x,000 XP" | tabular sans | Semibold | — | tabular | over-bar | 20 | `#f0e8cf` |
| "THALRION" name | serif display | Bold | UPPER | +5% | heavy gold bevel + bloom | 59 | `#f0d27a`→`#caa04a` |
| "SILVERWARDENS" | strong sans | Semibold | UPPER | +4% | drop-shadow | 26 | `#9fc0ff` |
| "Knights of the Realm" | serif italic, light | Regular | Title | +3% | subtle | 18 | `#cbb98a` |
| Stat labels (BATTLES/WINS/WIN RATE) | sans | Medium | UPPER | +8% | dim | 18 | `#9a8a5a` |
| Stat values (1,248/842/67.5%) | tabular serif | Bold | — | tabular | gold glow | 40 | `#f4e9c8` |
| "Equipped" | sans | Semibold | Title | +4% | gold | 24 | `#caa04a` |
| Slot item names | sans | Medium | Title | +2% | — | 18 | `#e8e2cf` |
| "Epic" rarity | sans | Semibold | Title | +6% | violet glow | 16 | `#c8a6ff` |
| "Title" kicker | sans | Regular | Title | +4% | dim | 18 | `#9a8a5a` |
| "REALM CHAMPION" | serif display | Bold | UPPER | +5% | gold bevel + bloom | 30 | `#f0d27a` |
| "Player ID" / "Joined" tags | sans | Regular | Title | +2% | dim | 18 | `#9a8a5a` |
| "#7A4B3C9E" | mono/tabular sans | Semibold | UPPER | tabular | — | 26 | `#cdd2da` |
| "2024-01-15" | tabular sans | Medium | — | tabular | — | 22 | `#cdd2da` |

Font: serif SDF for PROFILE/THALRION/REALM CHAMPION + stat numerals; Roboto SDF for body/IDs/dates.

---

## G — MATERIALS
- **BG:** dark stone-hall texture, low contrast, strong vignette; subtle warm key from upper area.
- **Gold chrome:** brushed antique gold (`#6b5320`→`#f0d27a`), engraved filigree on the avatar frame and
  panel edges; medium specular on top bevels; light wear.
- **Avatar frame:** ornate cast-gold portrait frame (arched top), inner dark recess with the hero render
  rim-lit gold/cobalt; soft inner shadow around the portrait.
- **Level badge:** gold ring medallion, dark center, "45" embossed; faint bloom.
- **XP bar:** dark groove `#1b1d24`, cobalt glassy fill with top specular.
- **Clan badge:** small gold-rimmed crest + dark pill background.
- **Stat blocks:** flat dark panels (or borderless over BG) with gold icons; thin gold divider lines between.
- **Equipped slots:** dark slate wells `#101218` with gold rim; item renders (helm/plate/sword/cloak/banner)
  lit; a thin **violet (Epic)** edge/underglow on each slot to signal rarity.
- **Title pill:** parchment/dark with gold frame; "REALM CHAMPION" embossed gold.
- **Copy button:** small gold/steel icon button.

---

## H — COMPONENTS (states)
**TabRail tab (Toggle, single-select):**
- *selected (Overview):* cobalt-filled pill, bright label, lit icon, left active marker.
- *idle:* transparent/dark, muted gold label; *hover:* label brightens + faint pill; click switches the right
  content pane (Overview/Heroes/Match History/Stats/Achievements/Customization).
- *disabled (if a tab is locked):* greyed + 🔒.

**EquippedSlot:**
- *filled:* item render + name + "Epic" rarity edge.
- *hover:* gold rim brightens, slight scale 1.03, tooltip with item details.
- *pressed:* scale 0.97 → opens item detail / equip-swap (routes to Customization).
- *empty:* dark well + "+" / "Empty" placeholder.

**Btn_CopyID:** *idle* gold icon; *pressed* flash + "Copied!" micro-toast; copies "#7A4B3C9E".

**StatBlock:** static display; on tab=Stats, may expand into a fuller breakdown (out of this Overview scope).

**Back button:** standard hover/pressed.

---

## I — ANIMATION TIMELINE
**OnShow (~0.55s):**
- 0.00s BG + vignette fade (0.20s).
- 0.05s Header (title scale 0.96→1.0 + fade) 0.22s.
- 0.10s Tab rail slides in from left + fade (0.20s); tabs cascade 0.03s each.
- 0.14s Avatar frame scales 0.94→1.0 + fade (0.25s ease-out-back); portrait backlight ramps.
- 0.24s Level badge pops (0.18s), XP bar fills left→right to current (0.40s).
- 0.28s Name + clan badge fade up (0.18s).
- 0.30s **Stat blocks** count-up tick to their values (1,248 / 842 / 67.5%) over 0.5s; icons glint.
- 0.36s **Equipped slots** stagger in (scale 0.92→1.0 + fade, 0.04s each, left→right).
- 0.46s Footer (Title pill + ID/Joined) fade in (0.18s).

**OnTabSwitch (~0.25s):** right content pane cross-fades/slides 16px (0.22s); selected pill marker slides to
the new tab.

**OnHoverSlot:** gold rim +20%, scale 1.03, tooltip fade (0.12s).
**OnCopyID:** copy-icon flash + "Copied!" toast pop (0.3s) then fade.

---

## J — PARTICLE & FX
- **Avatar:** gentle volumetric backlight + 1–2 dust motes; faint gold rim shimmer on the frame.
- **Level badge:** soft gold glow pulse.
- **Equipped slots:** subtle violet (Epic) underglow shimmer; on hover a brief sparkle.
- **Title pill:** faint gold bloom on "REALM CHAMPION".
- **Stat icons:** single glint on enter.
Budget pooled/capped; reduce motes on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load profile (name, level, XP, clan, stats, equipped cosmetics, title, ID, join date) read-only;
  default tab Overview; run enter timeline + stat count-up.
- **OnTab(tab):** swap right content pane (Heroes → roster summary; Match History → recent matches; Stats →
  full stats; Achievements → grid; Customization → cosmetic equip).
- **OnSlot tap:** open cosmetic detail / route to Customization to swap.
- **OnCopyID:** copy "#7A4B3C9E" to clipboard + toast.
- **OnBack:** pop → Main Menu.
- **§12:** all data read-only/server-authoritative; cosmetic equip is a meta request via Customization; no
  local mutation, no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn values: name THALRION, level 45, clan SILVERWARDENS / "Knights of the Realm",
  BATTLES 1,248, WINS 842, WIN RATE 67.5%, equipped = Galvanhelm / Lionheart Plate / Dawnbreaker / Royal Cloak
  / Warden's Banner (all Epic), Title "REALM CHAMPION", Player ID #7A4B3C9E, Joined 2024-01-15.
- Exactly **6** tabs in the rail and **5** equipped slots — do not add/remove.
- No currency chips are drawn on this screen → do **not** add them.
- Do not invent additional stats on the Overview pane beyond the three blocks.
- No portrait variant, no stick figures, no real brand text.
- Equip changes are meta requests only; no local/ECS mutation.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Left rail with 6 tabs (Overview selected, cobalt) + faction crest at bottom.
2. Center: ornate avatar frame, level "45" badge, XP bar, "THALRION", clan badge SILVERWARDENS / "Knights of
   the Realm".
3. Right: 3 stat blocks (BATTLES 1,248 / WINS 842 / WIN RATE 67.5%) with correct icons.
4. Equipped row of 5 slots with the exact item names + "Epic" rarity edges.
5. Footer: Title "REALM CHAMPION" pill, Player ID #7A4B3C9E + copy button, Joined 2024-01-15.
6. Header: PROFILE title centered, back double-chevron top-left.
7. Enter stagger, stat count-up, XP fill, slot stagger, tab cross-fade present.
8. Safe-area honored; BG full-bleed; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges.

## N — IMPLEMENTATION CONFIDENCE
**92/100.** Strong confidence: 3-column structure, tab rail, avatar/level/XP, stat blocks, 5 equipped slots,
and all visible copy are clearly legible. Minor uncertainty: exact avatar XP numerator (partly obscured under
the badge — render as the drawn "x,xxx / x,000 XP" placeholder), precise stat-block icon art, and the exact
arrangement of the footer (Title pill centered vs left, ID/Joined right) — the spec uses the most consistent
reading.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 24 · Profile".
- [x] Fraction-based layout normalized to 2340×1080; rail/slots/stat-block math given.
- [x] Tab rail + equipped slots + copy button states (idle/hover/pressed/selected/empty/disabled).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; nothing invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 25 · Battle Pass

Source: design/BattlePassDesign.png · 1774×887 (2.0:1) · Analysis-only forensic spec.

> Normalize the source 1774×887 art to the **2340×1080** production canvas (CanvasScaler ref 2340×1080,
> matchHeight=1.0). Positions are **fractions of 2340×1080**; "@1080" = on-canvas size at 1080-tall.

---

## A — SCREEN PURPOSE
The **Battle Pass** seasonal-track meta/live-ops screen — season **"SEASON OF GLORY"**. Layout:
1. **Header banner:** ornate title "SEASON OF GLORY" on hanging drapes, a season timer **"Ends in: 28d 14h"**,
   currency chips top-right (**Gems 2,450**, **Gold 98,760**).
2. **Progress strip (left):** a season **level badge "23"**, label **"Battle Pass XP"**, an XP bar
   **"650 / 1,000 XP"** with a next-level arrow to **"24"**, and a **"Missions"** button.
3. **Tier track (center):** a horizontally-scrolling two-row reward track — **FREE** (top row) and **PREMIUM**
   (bottom row) — with numbered tier columns **19 20 21 22 23 24 25 …** and a final highlighted **tier 30**
   premium showcase card on the right.
4. **Right showcase card:** a tall **"BATTLE PASS"** card displaying the tier-**30** legendary premium reward
   (locked/featured).
5. **Bottom CTA bar:** **"UNLOCK PREMIUM — Get premium rewards and exclusive perks!"** with three perk chips
   (**+20% XP Boost**, **+20% Gold Bonus**, **Exclusive Rewards**) and the **"UNLOCK PREMIUM 999"** (gems) CTA.

Purpose: show season progress, free vs premium rewards per tier, claim earned rewards, and upsell the premium
pass. Server-authoritative meta. Reached from: Main Menu / live-ops. Back: top-left chevron.

---

## B — VISUAL DNA (screen-specific)
- **Mood:** royal seasonal festival — purple-and-gold regalia, hanging banners/drapes, a treasure-laden track.
  Premium = amethyst/violet luxury; free = steel/blue restraint.
- **Palette anchors:**
  - BG: dark battlefield-at-dusk `#0a0b0f → #1a1320` with a faint ruined-siege vista; vignette.
  - Gold chrome (title frame, track frame, tier numbers): `#6b5320 → #caa04a → #f0d27a`.
  - **Premium violet/amethyst:** `#5a2db0 → #7e3fd6 → #b07cf0`, glow `#c79bff` — drapes, premium row,
    tier-30 card, UNLOCK PREMIUM CTA accents.
  - Free-row steel/blue: `#2b3a5a → #3d6fb0`, cool.
  - Reward icons: gems (violet crystal `#9e6bf0`), gold coins (`#f0c14a`), chests (wood+gold), books/cosmetics.
  - Claimed check: green `#5fd35a`.
- **Hierarchy:** SEASON OF GLORY title → tier track → tier-30 showcase → UNLOCK PREMIUM CTA → progress strip
  → currencies.

---

## C — SCREEN DECOMPOSITION (ASCII node tree)
```
BattlePassScreen (UiScreen root, CanvasGroup)
├─ BG_FullBleed (Image, full-bleed)
│  ├─ BG_SiegeDusk (Image, purple-tinted battlefield vista)
│  └─ BG_Vignette (Image, multiply)
├─ SafeAreaRoot (RectTransform + SafeAreaFitter)
│  ├─ Header
│  │  ├─ Btn_Back (Button) [chevron] top-left
│  │  ├─ TitleBanner
│  │  │  ├─ Drapes (Image, hanging purple banners)
│  │  │  └─ Lbl_Title "SEASON OF GLORY" (Text, serif gold)
│  │  ├─ Lbl_Timer "Ends in: 28d 14h ⏳" (Text + hourglass icon)
│  │  └─ CurrencyChips (HorizontalLayoutGroup) top-right
│  │     ├─ Chip_Gems [gem | "2,450" | Btn_Plus]
│  │     └─ Chip_Gold [coin | "98,760" | Btn_Plus]
│  ├─ ProgressStrip (left band)
│  │  ├─ Badge_SeasonLevel "23" (Image shield/medallion + Text)
│  │  ├─ Lbl_XPTag "Battle Pass XP"
│  │  ├─ XPBar (Slider) [Fill + Lbl "650 / 1,000 XP"]
│  │  ├─ Lbl_NextLevel "24" (with arrow ➜)
│  │  └─ Btn_Missions (Button) [icon | "Missions"]
│  ├─ TrackScroll (ScrollRect, HORIZONTAL, masked viewport)
│  │  └─ TrackContent (RectTransform)
│  │     ├─ RailLabels (left, fixed/sticky)
│  │     │  ├─ Lbl_Free "FREE" (+ shield icon)
│  │     │  └─ Lbl_Premium "PREMIUM" (+ premium crest, 🔒)
│  │     ├─ TierColumns (HorizontalLayoutGroup)
│  │     │  ├─ TierColumn_19 [ Lbl "19" | FreeReward | PremiumReward ]
│  │     │  ├─ TierColumn_20 [ "20" | gem×25 | coin×100 ]
│  │     │  ├─ TierColumn_21 [ "21" | ✓claimed | reward ]
│  │     │  ├─ TierColumn_22 [ "22" | ×50 | ? mystery ]
│  │     │  ├─ TierColumn_23 [ "23" (CURRENT) | chest×1 | chest ]
│  │     │  ├─ TierColumn_24 [ "24" | coin×15,000 | ×200 ]
│  │     │  └─ TierColumn_25 [ "25" | gem×50 | ×1 ]
│  │     │     (each Reward cell:)
│  │     │     ├─ Cell_Frame (Image, free=steel / premium=violet, 🔒 if locked)
│  │     │     ├─ Reward_Icon (Image)
│  │     │     ├─ Reward_Qty (Text, e.g. "10,000")
│  │     │     └─ Claimed_Check (Image ✓ — when claimed)
│  │     └─ Showcase_Tier30 (tall card, right end)
│  │        ├─ Lbl_BattlePass "BATTLE PASS" (vertical/stacked)
│  │        ├─ Lbl_Tier "30"
│  │        └─ Reward_LegendaryArmor (Image, premium tier-30 reward)
│  └─ BottomCTABar (HorizontalLayoutGroup)
│     ├─ Lbl_UnlockTitle "UNLOCK PREMIUM"
│     ├─ Lbl_UnlockSub "Get premium rewards and exclusive perks!"
│     ├─ PerkChips (HorizontalLayoutGroup)
│     │  ├─ Perk_XP   [icon | "+20% XP Boost"]
│     │  ├─ Perk_Gold [icon | "+20% Gold Bonus"]
│     │  └─ Perk_Excl [icon | "Exclusive Rewards"]
│     └─ Btn_UnlockPremium (Button, gold/violet) "UNLOCK PREMIUM  💎 999"
```

---

## D — UNITY HIERARCHY SPEC
| Node | Parent | Order | Type | Anchor | Pivot | Align | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| Screen | Canvas | 0 | RectTransform+CanvasGroup | stretch | .5,.5 | — | — | fills |
| BG_FullBleed | Screen | 0 | Image | stretch | .5,.5 | — | full-bleed | grows |
| SafeAreaRoot | Screen | 1 | RectTransform+SafeAreaFitter | stretch | .5,.5 | — | defines inset | insets |
| Header | SafeAreaRoot | 0 | RectTransform | top-stretch | .5,1 | — | inside | spans |
| Btn_Back | Header | 0 | Button | top-left | 0,1 | — | inside | pinned TL |
| TitleBanner | Header | 1 | RectTransform | top-center | .5,1 | center | inside | centered |
| Lbl_Timer | Header | 2 | Text | top-center (below title) | .5,1 | center | inside | centered |
| CurrencyChips | Header | 3 | HorizontalLayoutGroup | top-right | 1,1 | right | inside | right-pinned |
| ProgressStrip | SafeAreaRoot | 1 | RectTransform | upper-stretch (below header) | .5,1 | left | inside | spans width |
| Badge_SeasonLevel | ProgressStrip | 0 | Image+Text | left | 0,.5 | center | inside | fixed |
| Lbl_XPTag | ProgressStrip | 1 | Text | left (right of badge) | 0,.5 | left | inside | — |
| XPBar | ProgressStrip | 2 | Slider(no handle) | center | .5,.5 | mid | inside | flex width |
| Lbl_NextLevel | ProgressStrip | 3 | Text | right of bar | 0,.5 | center | inside | fixed |
| Btn_Missions | ProgressStrip | 4 | Button | right | 1,.5 | center | inside | fixed |
| TrackScroll | SafeAreaRoot | 2 | ScrollRect (horizontal only) | center-stretch | .5,.5 | — | inside | spans width, fixed height |
| TrackContent | TrackScroll/Viewport/Content | 0 | RectTransform | left-stretch (vertical) | 0,.5 | — | inside | width grows with tiers |
| RailLabels | TrackContent | 0 | RectTransform | left-stretch (sticky) | 0,.5 | — | inside | fixed left, optionally pinned |
| TierColumns | TrackContent | 1 | HorizontalLayoutGroup | left-stretch | 0,.5 | upper-left | inside | columns flow right |
| TierColumn_* | TierColumns | 0..n | RectTransform | — | — | center | inside | uniform col width |
| Cell_Frame/Reward_*/Claimed_Check | (cells) | — | Image/Text | stretch(inset) | .5,.5 | center | inside | scales w/ cell |
| Showcase_Tier30 | TrackContent | 2 | RectTransform(framed) | right | 1,.5 | center | inside | taller card, right end |
| BottomCTABar | SafeAreaRoot | 3 | HorizontalLayoutGroup | bottom-stretch | .5,0 | — | inside | spans width |
| Lbl_UnlockTitle/Sub | BottomCTABar | 0,1 | Text | left | 0,.5 | left | inside | left cluster |
| PerkChips | BottomCTABar | 2 | HorizontalLayoutGroup | center | .5,.5 | center | inside | 3 chips |
| Btn_UnlockPremium | BottomCTABar | 3 | Button | right | 1,.5 | center | inside | wide CTA right |

---

## E — LAYOUT MATHEMATICS (fractions of 2340×1080)
**Header:** y 0.86 → 1.00.
- Btn_Back: center x 0.030, y 0.955; ≈0.045w×0.085h.
- TitleBanner "SEASON OF GLORY": centered x 0.50, drapes span x 0.30→0.70 hanging from top; title baseline
  y≈0.945, cap ≈0.060h (≈65px@1080), wide tracking.
- Lbl_Timer "Ends in: 28d 14h ⏳": centered x 0.50, y≈0.875, ≈22px.
- CurrencyChips: right edge x 0.972, y 0.955; two chips (Gems, Gold) ≈0.115w×0.052h, gap 0.010w.

**Progress strip:** band y 0.790 → 0.855, spans x 0.030 → 0.760.
- Badge_SeasonLevel "23": left, x≈0.040, a gold shield/medallion Ø≈0.060w (≈140px), "23" centered.
- Lbl_XPTag "Battle Pass XP": x≈0.110, top of bar (≈22px).
- XPBar: x 0.110 → 0.560, y≈0.805 (height ≈0.030h ≈32px), violet/gold fill at 65% (650/1,000); centered text
  "650 / 1,000 XP".
- Lbl_NextLevel "24" with ➜ arrow: just right of the bar, x≈0.575, in a small gold node (Ø≈0.038w).
- Btn_Missions: right, x 0.640 → 0.760, y≈0.805, gold pill with scroll icon + "Missions" (≈26px).

**Tier track:** region y 0.230 → 0.770 (height ≈0.540h ≈583px@1080), x 0.020 → 0.840 (the right ~0.16 is the
tier-30 showcase). Horizontal ScrollRect.
- **RailLabels** (left, sticky): "FREE" (with shield icon) at the vertical center of the top row (y≈0.640),
  "PREMIUM" (with crest + 🔒) at the center of the bottom row (y≈0.380); label column ≈0.075w wide.
- **TierColumns:** start x≈0.110. **7 visible columns (19–25)**. Column width ≈0.090w (≈211px@1080),
  gap ≈0.006w. Each column:
  - Tier number ("19".."25") in a small node at the **top** of the column, y≈0.730 (≈26px gold; current "23"
    highlighted/larger).
  - **Free cell** (top row): centered y≈0.620, cell ≈0.080w × 0.150h (≈187×162@1080), steel-blue frame.
  - **Premium cell** (bottom row): centered y≈0.380, same size, violet frame (🔒 if pass not owned).
  - Each cell: reward icon centered, quantity text at the cell bottom (e.g. "10,000", "25", "50", "15,000",
    "5,000", "100", "200"); a green ✓ overlay if claimed (tiers 21 show ✓ on both rows; 22 premium shows "?"
    mystery).
- **Showcase_Tier30** (right end): tall card x 0.850 → 0.982, y 0.235 → 0.765 (spans both rows' height),
  violet ornate frame; "BATTLE PASS" stacked at the top, "30" tier number, a legendary armored-king reward
  render filling the card; strong premium glow (featured/locked).

(Free-row sample contents L→R: 19=10,000 gold; 20=25 gems; 21=✓; 22=50; 23=chest×1; 24=15,000 gold; 25=50 gems.
Premium-row L→R: 19=5,000; 20=100; 21=✓; 22=? mystery; 23=chest; 24=200; 25=×1. Transcribe icons/qty as drawn;
where a glyph is ambiguous render the icon shown and the legible number.)

**Bottom CTA bar:** band y 0.020 → 0.110, spans x 0.020 → 0.982; dark gold-framed bar.
- Lbl_UnlockTitle "UNLOCK PREMIUM": left, x≈0.060, y≈0.085 (≈34px serif gold).
- Lbl_UnlockSub "Get premium rewards and exclusive perks!": x≈0.060, y≈0.045 (≈20px).
- PerkChips: centered cluster x 0.470 → 0.760, three chips each ≈0.090w: "+20% XP Boost" (book/star icon),
  "+20% Gold Bonus" (coin icon), "Exclusive Rewards" (chest icon).
- Btn_UnlockPremium: right, x 0.790 → 0.972, y≈0.065 (height ≈0.075h ≈81px); gold/violet CTA, label
  "UNLOCK PREMIUM" + gem icon + "999".

**Tablet (4:3):** fewer tier columns visible per viewport (scroll reveals more); progress strip wraps Missions
under the bar if width < threshold; CTA bar stacks title/sub left, perks may wrap. **Ultrawide (21:9):** more
tier columns visible at once; showcase stays right-anchored; BG full-bleed. **Notch:** SafeAreaRoot insets;
BG/drapes full-bleed under cutout; back/chips clear.

---

## F — TYPOGRAPHY
| Text | Personality | Weight | Caps | Kerning | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|
| "SEASON OF GLORY" | serif display, ornate | Black | UPPER | +8% | heavy gold bevel + bloom + drop | 65 | `#f0d27a`→`#caa04a` |
| "Ends in: 28d 14h" | sans | Medium | Sentence | +2% | hourglass icon, dim | 22 | `#cbb98a` |
| Currency values | tabular sans | Semibold | — | tabular | 1px stroke | 30 | `#f4e9c8` |
| Season level "23" | tabular serif | Black | — | — | gold glow + stroke | 44 | `#fff4d6` |
| "Battle Pass XP" | sans | Medium | Title | +2% | dim | 22 | `#c9b27a` |
| XP "650 / 1,000 XP" | tabular sans | Semibold | — | tabular | over-bar | 20 | `#f0e8cf` |
| Next level "24" | tabular serif | Bold | — | — | gold | 26 | `#f0d27a` |
| "Missions" | sans | Semibold | Title | +2% | gold | 26 | `#f0e8cf` |
| "FREE" / "PREMIUM" rail | strong sans | Bold | UPPER | +8% | FREE steel, PREMIUM violet glow | 24 | FREE `#9fb6d8`; PREM `#c79bff` |
| Tier numbers 19–25 | tabular sans | Bold | — | — | current glows | 26 | cur `#f0d27a`; others `#d9c79a` |
| Reward qty (10,000/25/50/200…) | tabular sans | Semibold | — | tabular | 1px stroke | 20 | `#f4e9c8` |
| "?" mystery | serif | Bold | — | — | violet glow | 30 | `#c79bff` |
| "BATTLE PASS" (showcase) | serif display | Black | UPPER | +6% (stacked) | gold bevel + violet bloom | 30 | `#f0d27a` |
| Showcase tier "30" | tabular serif | Black | — | — | strong gold glow | 48 | `#fff4d6` |
| "UNLOCK PREMIUM" (bar title) | serif/strong | Black | UPPER | +6% | gold bevel + bloom | 34 | `#f0d27a` |
| Unlock sub | sans, light | Regular | Sentence | +2% | dim | 20 | `#cbb98a` |
| Perk labels (+20% …) | sans | Medium | Title | +2% | icon-led | 19 | `#e8e2cf` |
| CTA "UNLOCK PREMIUM 999" | strong sans | Bold | UPPER | +4% | white + dark stroke; gem icon | 30 | `#ffffff` |

---

## G — MATERIALS
- **BG:** dark dusk battlefield, violet-shifted, faint siege silhouettes; strong vignette.
- **Drapes/banners:** rich amethyst velvet `#5a2db0`→`#7e3fd6` with gold trim + tassels; soft cloth shading.
- **Gold chrome:** brushed antique gold filigree on title frame, track frame, tier nodes, CTA edges.
- **Free cells:** steel-blue beveled frames `#2b3a5a`→`#3d6fb0`, dark recess, gold corner ticks.
- **Premium cells:** amethyst beveled frames `#5a2db0`→`#b07cf0` with violet inner glow; a small 🔒 padlock
  overlay when the pass isn't owned; richer/brighter than free cells.
- **Reward icons:** glassy gems, shiny coins, wooden+gold chests, bound spellbooks, painted cosmetics — each
  in a recessed well; claimed cells show a green ✓ medallion.
- **Tier-30 showcase:** ornate violet+gold card, inner glow, a high-detail legendary armored-king render;
  premium "featured" bloom; clearly the track's apex.
- **XP bar:** dark groove, violet/gold glassy fill (~65%) with top specular.
- **CTA UNLOCK PREMIUM:** gold-rimmed violet/gold gradient button, white label, gem icon, idle bloom; the
  brightest interactive element.

---

## H — COMPONENTS (states)
**TierColumn reward Cell (Free & Premium):**
- *unclaimed-locked (tier > current):* frame dim, faint padlock; reward greyed.
- *claimable (tier ≤ current, not yet claimed):* frame lit + gentle pulse; tap → claim flow.
- *claimed:* green ✓ medallion, frame slightly dim/settled.
- *premium-locked (pass not owned):* premium cell shows 🔒 + "Unlock Premium" gating; tapping routes to the
  UNLOCK PREMIUM CTA.
- *mystery ("?"):* hidden reward until claimed/revealed.

**Showcase_Tier30:** featured/locked state with strong glow; tap → detail of the tier-30 reward.

**XPBar:** read-only; fills to 650/1,000; on level-up animates to full → resets, level badge ticks 23→24.

**Btn_Missions:** standard gold pill; opens the pass missions/quests list.

**Btn_UnlockPremium (CTA):**
- *idle:* gold/violet, "UNLOCK PREMIUM" + gem 999, bloom.
- *hover:* +10% brightness, glow grows.
- *pressed:* scale 0.97 + flash.
- *owned (premium already purchased):* hidden/replaced by "PREMIUM ACTIVE" badge; premium cells unlock.
- *insufficient gems:* tap → confirm/insufficient modal → Store.

**Perk chips:** static informational; hover shows tooltip detail.

**Back / chip + buttons:** standard.

---

## I — ANIMATION TIMELINE
**OnShow (~0.7s):**
- 0.00s BG + vignette fade (0.20s); drapes settle (subtle sway-in 0.30s).
- 0.05s Header (title scale 0.96→1.0 + bloom, timer fade) 0.25s.
- 0.12s Progress strip fades in; **XP bar fills** left→right to 650/1,000 (0.45s ease-out); level badge pop.
- 0.20s **Tier columns stagger in** from left (each scale 0.94→1.0 + fade, 0.03s stagger); claimable cells
  begin a gentle pulse; current tier (23) node glows.
- 0.40s Tier-30 showcase scales 0.96→1.0 + strong bloom sweep (0.30s).
- 0.50s Bottom CTA bar slides up + fade (0.22s); UNLOCK PREMIUM gets a single glow sweep; perk chips cascade.

**OnClaim(tier):** cell flash → green ✓ stamps with a pop (0.25s) + sparkle; reward icon flies to the
currency chip / inventory; chip count ticks up.

**OnUnlockPremium:** CTA flash → premium cells' padlocks unlock in a left-to-right cascade (0.04s each) with
violet sparkle; "PREMIUM ACTIVE" badge appears; perk chips glow.

**Idle ambient:** drapes faint sway; showcase card slow shimmer; claimable cells pulse.

---

## J — PARTICLE & FX
- **Drapes/title:** gentle gold dust + soft bloom on the title.
- **Premium cells & showcase:** violet shimmer sweep (rarer = brighter); tier-30 card has rising amethyst
  motes + a slow specular sweep.
- **Claimable cells:** soft pulsing glow + occasional sparkle.
- **Claim:** gold/violet sparkle burst + reward fly-to-chip; chip coin-poof on arrival.
- **Unlock premium:** cascade of violet unlock sparks across the premium row.
- **CTA idle:** bloom breathing; press = gem-spark ring.
Budget pooled/capped; reduce sweeps/motes on low-end.

---

## K — EVENT BEHAVIOR
- **OnShow:** load season data (name, end time, current tier 23, XP 650/1,000, free/premium reward states,
  premium-owned flag) read-only; run enter timeline; start timer countdown.
- **OnScrollTrack:** horizontal scroll reveals earlier/later tiers; tier-30 showcase pinned at the right end.
- **OnClaim(tier,row):** if claimable & (premium row → pass owned) → send claim request (server-auth); on
  success stamp ✓, grant reward, tick currency; else gate (route to UNLOCK PREMIUM or show locked).
- **OnMissions:** open the battle-pass missions list (XP sources).
- **OnUnlockPremium:** if affordable (gems ≥ 999) → purchase request (server-auth) → unlock premium row;
  else → insufficient/confirm modal → Store.
- **OnPlus (chip):** route to Store.
- **OnBack:** pop → Main Menu.
- **§12:** all claims/purchases are server-authoritative requests; UI displays + requests only; no local
  balance mutation; no ECS write.

---

## L — NEGATIVE RULES
- Use exactly the drawn values: season "SEASON OF GLORY", "Ends in: 28d 14h", level 23, XP "650 / 1,000 XP",
  next "24", visible tiers 19–25 + showcase 30, currencies Gems 2,450 / Gold 98,760, perks "+20% XP Boost" /
  "+20% Gold Bonus" / "Exclusive Rewards", CTA "UNLOCK PREMIUM 999". Reward quantities as drawn
  (10,000 / 25 / 50 / 15,000 / 5,000 / 100 / 200 / 1 / "?").
- Two rows only: **FREE** (top) and **PREMIUM** (bottom). Do not add a third track.
- Only two currency chips (Gems + Gold). Do not add Silver here.
- Do not invent tier rewards beyond what's legible; render the icon shown for ambiguous cells.
- No portrait variant, no stick figures, no real brand text.
- Claims/purchases are requests only; no local/ECS mutation.

## M — ACCEPTANCE CRITERIA (≥95%)
1. Ornate "SEASON OF GLORY" banner with drapes + "Ends in: 28d 14h" timer; Gems 2,450 / Gold 98,760 chips.
2. Progress strip: level "23" badge, "Battle Pass XP", XP bar 650/1,000 (~65% fill) → "24", Missions button.
3. Horizontally-scrolling two-row track (FREE top / PREMIUM bottom) with tier columns 19–25 + numbered nodes,
   correct reward icons/quantities, claimed ✓ on tier 21, "?" mystery on tier 22 premium.
4. Premium cells carry violet frames + padlocks (pass-locked); free cells steel-blue.
5. Tier-30 "BATTLE PASS" showcase card (violet, legendary reward) pinned right.
6. Bottom CTA bar: UNLOCK PREMIUM title + sub + 3 perk chips + "UNLOCK PREMIUM 999" gem CTA (brightest CTA).
7. XP fill, tier stagger, claim ✓ pop, premium-unlock cascade animations present.
8. Safe-area honored; BG/drapes full-bleed; track scrolls; holds on 4:3 / 19.5:9 / 21:9 / notched landscape.
9. Hex/typography within F/G ranges.

## N — IMPLEMENTATION CONFIDENCE
**88/100.** High confidence on the overall structure (banner, progress strip, two-row scrolling track,
showcase, CTA bar) and all major copy/numbers. Lower-than-others uncertainty on a few **per-cell reward
icons/quantities** (some glyphs are small/overlapping — e.g. tier 22/25 premium contents) and whether RailLabels
should be sticky vs scroll with the track (spec recommends sticky-left). These are cell-detail risks, not
structural; render ambiguous cells as the icon+legible number shown.

## O — SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Header matches "# BULWARK — UI CONSTRUCTION SPEC · 25 · Battle Pass".
- [x] Fraction-based layout normalized to 2340×1080; track column sizes + gaps given.
- [x] ScrollRect (horizontal) + HorizontalLayoutGroup specified for the tier track.
- [x] Tier-cell / CTA / Missions / chip states (locked/claimable/claimed/mystery/owned/insufficient).
- [x] Animation + particle timelines with timestamps/easing.
- [x] §12 boundary + server-auth respected; exact copy transcribed; ambiguous cells flagged not invented.
- [x] Landscape/safe-area/tablet/ultrawide/notch covered.
- [x] Analysis-only — only this .md written.



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 28 · Daily Reward

Source: design/DailyRewardDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops 7-day login-streak calendar. Modal-style ornate panel over a dimmed hub backdrop. Single primary action: **CLAIM** the currently-available day. ADR/canon note (do NOT alter forensic spec): the **5R/120 chip in the inherited top bar reads as an energy/stamina meter → canon-CUT; document, flag, omit at implementation**. Day-7 "EPIC CHEST + Legendary Unit Guaranteed" is loot/gacha-adjacent → note for ADR; it is a *reward depiction*, not a spin, so lower risk than Lucky Spin.

---

## A · SCREEN PURPOSE
A retention/login screen presented as a centered ornate panel floating over the dimmed Main-Menu hub (rail + currency bar + faint right-edge tiles bleed through at low alpha). It shows a horizontal **7-cell streak calendar** (Day 1 → Day 7), the player's reward for each day, claim-state per day (claimed ✓ / claimable-highlighted / locked 🔒), a **streak summary** strip, and one gold **CLAIM** CTA. Exactly one day is "today/claimable" (here Day 3, ring-highlighted) and is the only one whose CLAIM is active. Entry is automatic on first hub load of the day or via the bottom-hub "Daily" entry. Close via top-right **✕** returns to hub.

## B · VISUAL DNA
Inherits the global dark heroic high-fantasy DNA (00 §6). Screen-specific:
- **Backdrop:** the live hub behind, darkened by a near-black scrim (~70% black) + vignette; a soft warm god-ray cone descends behind the panel top, and a faint focal glow sits behind the central highlighted day.
- **Panel:** large near-black obsidian plate (#0c0e15→#161a24 vertical) with an **ornate cast-gold/antique-bronze double frame** — outer beveled gold rail + inner thin filigree line, scrolled corner cartouches, and a **crown/gem crest centerpiece** straddling the top edge (cobalt gem in a gold sunburst).
- **Day cells:** dark slate rounded-rect tiles in a row; the **claimable day (Day 3)** is enlarged, brighter, and wrapped in a glowing **gold ring/halo**; the **premium day (Day 7)** uses a **violet/amethyst** frame + body tint (premium signal) vs the gold/blue of the others.
- **CLAIM CTA:** brushed-gold pill (the single brightest interactive object), beveled, inner gradient, dark serif label.
- **Mood:** "reward ritual" — gold-on-black opulence; the highlighted day is the luminous focal subject.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
DailyRewardScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (hub-snapshot or static art, full-bleed, behind cutout)
│  ├─ DimScrim (black ~70%)
│  ├─ Vignette (radial, edges dark)
│  └─ GodRayCone (additive, top-center, behind panel)
├─ InheritedHubChrome (low-alpha bleed; NON-interactive here — see L)
│  ├─ TopBar
│  │  ├─ AvatarPortrait + Name "Warden" + "Level 32"
│  │  ├─ CurrencyChip_Gold   icon + "128,450"
│  │  ├─ CurrencyChip_Silver icon + "87,560"
│  │  ├─ CurrencyChip_Gems   icon + "2,850"
│  │  ├─ EnergyChip          icon + "5R/120"   ⚠️CANON-CUT
│  │  ├─ MailButton (envelope)
│  │  └─ MenuButton (hamburger)
│  ├─ LeftRail (icon+label stack: Campaign, Army, Commanders, Quests, Store, Alliance, Events)
│  └─ RightEdgeTiles (faint: "EVENT Ends in 2d 14h", "BATTLE Chapter 9-12")
└─ SafeAreaRoot (SafeAreaFitter; all interactive content)
   └─ RewardPanel (ornate framed plate, center)
      ├─ PanelFrame (gold double-frame + corner cartouches)
      ├─ TopCrest (crown/gem centerpiece, overlaps top edge)
      ├─ CloseButton ✕ (top-right, on/over frame)
      ├─ Header
      │  ├─ Title "DAILY REWARD" (serif gold-bevel UPPERCASE)
      │  └─ Subtitle "Log in every day to earn valuable rewards!"
      ├─ DayRow (HorizontalLayoutGroup, 7 children)
      │  ├─ DayCell_1 [CLAIMED]   {Header "DAY 1", Icon gold-coins, Amount "5,000", State ✓}
      │  ├─ DayCell_2 [CLAIMED]   {Header "DAY 2", Icon silver-bars, Amount "10,000", State ✓}
      │  ├─ DayCell_3 [CLAIMABLE] {Header "DAY 3", Icon blue-crystal, Amount "100", State ✓ + GOLD HALO RING}
      │  ├─ DayCell_4 [LOCKED]    {Header "DAY 4", Icon chest, Amount "1", State 🔒}
      │  ├─ DayCell_5 [LOCKED]    {Header "DAY 5", Icon shield/sigil, Amount "50", State 🔒}
      │  ├─ DayCell_6 [LOCKED]    {Header "DAY 6", Icon blue-gems, Amount "200", State 🔒}
      │  └─ DayCell_7 [LOCKED·PREMIUM] {Header "DAY 7" (gold), Icon ornate-chest, Label "EPIC CHEST",
      │                                  Sub "+ Legendary Unit Guaranteed!", State 🔒, VIOLET frame}
      └─ FooterStrip (rounded sub-panel inside frame, full width)
         ├─ FlameIcon (ember glow)
         ├─ StreakLine "STREAK: 3 DAYS"  (label + bold value)
         ├─ StreakSub  "Keep the streak going to earn better rewards!"
         └─ ClaimButton  "CLAIM"  (gold pill, right side)
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| DailyRewardScreen | Canvas | 0 | RectTransform+CanvasGroup | stretch-all | .5,.5 | — | root | fade in/out |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | extends UNDER cutout | cover-fill, no inset |
| DimScrim | backdrop | 0 | Image (black α.70) | stretch-all | .5,.5 | — | full-bleed | — |
| GodRayCone | backdrop | 2 | Image (additive) | top-center | .5,1 | — | — | scale w/ height |
| InheritedHubChrome | screen | 1 | RectTransform (α≈.25, raycast OFF) | stretch-all | .5,.5 | — | inside safe | non-interactive |
| SafeAreaRoot | screen | 2 | RectTransform+SafeAreaFitter | stretch-all | .5,.5 | — | insets to safeArea | drives all content |
| RewardPanel | SafeAreaRoot | 0 | Image (9-slice) | center | .5,.5 | — | inside safe | fixed aspect, scale-to-fit |
| PanelFrame | RewardPanel | 0 | Image (9-slice ornate) | stretch-all | .5,.5 | — | — | borders fixed px |
| TopCrest | RewardPanel | 1 | Image | top-center | .5,.5 | — | — | overlaps top edge |
| CloseButton | RewardPanel | 2 | Button+Image | top-right | 1,1 | — | — | offset fixed |
| Header | RewardPanel | 3 | VerticalLayoutGroup | top-stretch | .5,1 | center | — | — |
| Title | Header | 0 | Text (TMP) | — | .5,.5 | center | — | autosize cap |
| Subtitle | Header | 1 | Text (TMP) | — | .5,.5 | center | — | — |
| DayRow | RewardPanel | 4 | HorizontalLayoutGroup (ctrl-size, spacing) | center | .5,.5 | mid-center | — | cells flex equally; Day3/7 wider |
| DayCell_n | DayRow | 0..6 | Image + VerticalLayoutGroup (LayoutElement) | — | .5,.5 | top-center | — | min/pref width; Day3 ×~1.12 |
| ↳ DayHeader | DayCell | 0 | Text (banner sub-strip) | top-stretch | .5,1 | center | — | — |
| ↳ DayIcon | DayCell | 1 | Image | center | .5,.5 | center | — | square |
| ↳ DayAmount | DayCell | 2 | Text | — | .5,.5 | center | — | — |
| ↳ DayStateBadge | DayCell | 3 | Image (✓ or 🔒) on disc | bottom-center | .5,0 | center | — | — |
| ↳ HaloRing (Day3 only) | DayCell | -1 (behind) | Image (additive glow) | stretch-all | .5,.5 | — | — | pulses |
| FooterStrip | RewardPanel | 5 | Image + HorizontalLayoutGroup | bottom-stretch | .5,0 | mid-left→right | — | spans inner width |
| FlameIcon | FooterStrip | 0 | Image | left | 0,.5 | — | — | — |
| StreakText (line+sub) | FooterStrip | 1 | VerticalLayoutGroup | left | 0,.5 | left | — | flex grow |
| ClaimButton | FooterStrip | 2 | Button+Image | right | 1,.5 | center | — | fixed min width |

**List/grid note:** DayRow is the canonical "wheel/list" structure here → `HorizontalLayoutGroup` with `childForceExpandWidth=false`, per-cell `LayoutElement.preferredWidth`. Not a ScrollRect (all 7 fit; no scroll). If a future variant exceeds 7, wrap DayRow in a horizontal ScrollRect.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
Normalize the 1.5:1 source onto 2340×1080; panel is centered and height-bounded.
- **RewardPanel:** width ≈ **0.80·W = 1872 px**, height ≈ **0.78·H = 842 px**; centered (origin offset 0). Outer gold frame thickness ≈ 0.012·H ≈ 13 px; inner filigree inset ≈ 0.008·H.
- **TopCrest:** width ≈ 0.10·W ≈ 234 px, centered on top edge, vertical center on the frame line (≈ half above the edge).
- **CloseButton:** ⌀ ≈ 0.045·H ≈ 49 px; center ≈ (panelRight − 0.03·W, panelTop − 0.03·H).
- **Header block:** top inset ≈ 0.07·panelH. Title cap-height region ≈ 0.10·H. Subtitle baseline ≈ 0.045·H below title.
- **DayRow:** vertical center ≈ 0.50·H of panel; row height ≈ 0.42·panelH. Inner usable width ≈ panelW − 2·(0.04·W) ≈ 1685 px.
  - 7 cells with 6 gaps. Gap ≈ 0.012·W ≈ 28 px → total gaps ≈ 168 px → cells share ≈ 1517 px.
  - **Base cell width** ≈ 1517 / (6 + 1.12 + 1.0) [Day3 ×1.12, Day7 ×1.0 same as base; Day7 slightly taller not wider] ≈ **≈ 198 px** for standard cells; **Day3 ≈ 222 px** (×1.12) and visually elevated.
  - Standard cell aspect ≈ 0.55 W:H → cell height ≈ 360 px (~0.33·H). Day3 height ≈ ×1.10.
  - Internal cell layout (fractions of cell H): DayHeader strip 0–0.18; DayIcon 0.18–0.62 (square, side ≈ 0.6·cellW); DayAmount 0.62–0.80; StateBadge disc ⌀ ≈ 0.22·cellW centered at 0.90.
- **FooterStrip:** spans inner width; height ≈ 0.16·panelH ≈ 135 px; bottom inset ≈ 0.05·panelH. FlameIcon ⌀ ≈ 0.07·panelH. ClaimButton width ≈ 0.26·panelW ≈ 487 px, height ≈ 0.11·panelH ≈ 93 px, right-anchored with 0.04·W right inset.

**Tablet (4:3, 2048×1536-class):** match-height keeps panel height; extra side margin → panel may grow to 0.86·W; cells widen, gaps scale with W. **Ultrawide (21:9):** more backdrop revealed; panel stays 1872 px (cap max width 0.62·W so it doesn't stretch); content unchanged. **Notch/landscape:** SafeAreaRoot insets; full-bleed backdrop stays under cutout; panel never crosses safe inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "DAILY REWARD" | serif Trajan-display, prestige | Black/Heavy | UPPER | +6% | 1.0 | gold bevel + soft outer bloom + 2px dark stroke + drop-shadow | ~74 | fill #f0d27a→#caa04a gradient; stroke #3a2c0e |
| Subtitle | light serif/clean sans, calm | Regular | Sentence | 0 | 1.1 | subtle shadow | ~26 | #d9c79a |
| DayHeader "DAY n" | condensed sans-caps banner | SemiBold | UPPER | +4% | 1.0 | dark stroke | ~24 | #e8dcc0 (Day7 gold #f0d27a) |
| DayAmount "5,000"/"100" | numeric, clean | Bold | — | 0 | 1.0 | shadow; highlighted day +glow | ~32 (Day3 ~36) | #ffffff; Day3 #eaf2ff |
| Day7 "EPIC CHEST" | serif small-caps, premium | Bold | UPPER | +4% | 1.0 | violet glow + stroke | ~26 | #c9a6ff |
| Day7 sub "+ Legendary Unit Guaranteed!" | clean sans | Italic/Reg | Sentence | 0 | 1.05 | shadow | ~18 | #b9a7d8 |
| StreakLine "STREAK: 3 DAYS" | condensed caps, emphatic | Bold | UPPER | +2% | 1.0 | shadow; "3 DAYS" brighter/larger | ~30 | label #d9c79a, value #ffd76a |
| StreakSub | clean sans, quiet | Regular | Sentence | 0 | 1.1 | — | ~20 | #9a937f |
| ClaimButton "CLAIM" | serif display, action | Heavy | UPPER | +8% | 1.0 | dark engrave + top highlight | ~40 | #2a1c06 on gold |
| Currency numbers (top bar) | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |

## G · MATERIALS (hex ranges, roughness, wear, edges, reflection/bloom)
- **Panel body:** obsidian #0c0e15→#161a24, roughness high (matte), faint engraved vertical sheen; subtle inner top-edge dark gradient.
- **Gold frame/CTA:** brushed antique gold #6b5320 (shadow) → #caa04a (mid) → #f0d27a (highlight) → #fff2c2 (specular pip); low roughness on bevel crests, worn micro-nicks on outer rail; warm rim-light top-left; **bloom on highlight pips**.
- **Standard day cell:** dark slate #11141d→#1b2030, thin bronze edge #7a5f28; claimable Day3 cell brighter #1a2336 with **gold halo ring** (additive #ffd16a, soft 18px blur, animated pulse) + cobalt inner glow.
- **Premium Day7 cell:** violet #1a1130→#2a1c52 body, **amethyst frame** #5a2db0→#9e6bf0, inner magic glow #b07bff + bloom.
- **State discs:** dark disc #14161e w/ bronze ring; ✓ = gold #f0d27a engraved; 🔒 = desaturated steel #8a8f9c with the whole cell at ~0.7 brightness + slight grey overlay.
- **Icons:** coins warm gold; silver bars cool #c8ccd6 specular; blue crystal/gems cobalt #2b56c8→#4f8bff with inner glow + crystalline specular; chests aged wood + bronze bands + lock.
- **Reflections/bloom:** gold + gems + halo carry bloom; matte panel/body do not.

## H · COMPONENTS (states)
**DayCell** — three logical states (visual), interaction only on the claimable one:
- *CLAIMED* (Day1,2): full color, ✓ gold disc, no halo, raycast off (informational).
- *CLAIMABLE* (Day3): brightened body + **pulsing gold halo ring** + cobalt focal glow; ✓-style disc; this is the visual "next reward"; it is NOT a separate button (claiming uses the footer CLAIM). On show it gently scales 1.00→1.04→1.00.
- *LOCKED* (Day4-7): ~70% brightness, grey overlay, 🔒 steel disc; raycast off. Day7 keeps its violet identity but dimmed under lock.
- *(future hover/pressed n/a — cells aren't pressable in this layout)*.

**ClaimButton** (the only interactive control besides ✕):
- *idle/enabled:* gold pill, bevel, soft outer glow, label #2a1c06; subtle 1.0→1.02 breathing glow.
- *hover/focus (gamepad):* +8% brightness, glow radius +.
- *pressed:* scale 0.96, inner shadow deepens, highlight dims 1f.
- *disabled* (no claim available / already claimed today): desaturate to grey-gold #7d7355, label #4a4636, glow off, raycast still on (tapping → small shake + toast "Come back tomorrow!").
- *success (post-claim):* flash white→gold, then label morphs to "CLAIMED" greyed; the claimable cell plays a collect burst.

**CloseButton ✕:** dark disc + bronze ring; hover brighten; pressed scale 0.92; closes screen.

**FooterStrip:** static info container; FlameIcon has a looping ember flicker; StreakLine value can count-up on first show.

## I · ANIMATION TIMELINE (timestamps, durations, order, easing)
**OnShow (entry, total ~0.85 s):**
- 0.00 backdrop scrim fade 0→.70 (0.20 s, linear) + god-ray fade-in (0.30 s).
- 0.06 RewardPanel: scale 0.92→1.00 + α 0→1 (0.28 s, ease-out-back small).
- 0.18 TopCrest drop-in: y −20→0 + α (0.22 s, ease-out) + crest gem glint sweep.
- 0.22 Title: α + slight 1.04→1.00 (0.20 s).
- 0.28 DayRow cells stagger in L→R, each y +16→0 + α, 0.04 s apart, 0.18 s each (ease-out).
- 0.55 Day3 halo ring fades up + starts pulse loop (period 1.6 s).
- 0.62 FooterStrip slide up + α (0.20 s); StreakLine value count-up 0→3 (0.4 s, ease-out) if dynamic.
- 0.70 ClaimButton pop 0.9→1.0 + glow on (0.18 s, ease-out-back); begin breathing-glow loop.

**Idle loops:** Day3 halo pulse (scale 1.00↔1.06 α 0.6↔1.0, 1.6 s sine); CLAIM glow breathe (1.6 s); flame flicker (0.5 s random); crest gem slow shimmer (3 s).

**OnClaim (see K):** CLAIM press 0.96 (0.08 s) → white flash (0.10 s) → Day3 cell "collect" burst (icon scale 1.2 + particles, 0.4 s) → reward fly-to-currency (0.5 s) → cell✓ locks, CLAIM → disabled "CLAIMED" (0.2 s). Optional: highlight advances to Day4 (halo migrates, 0.3 s) if same-session preview.

**OnClose:** reverse of entry, ~0.30 s (panel scale 1.0→0.94 + α→0, scrim fade out).

## J · PARTICLE & FX
- God-ray cone (soft additive, slow drift) behind panel top.
- Day3 **halo ring**: additive radial gold glow + faint orbiting sparkles (2–3 motes).
- Gem/crystal icons: tiny inner twinkle sparkles (1–2, slow).
- FlameIcon: small ember particle puff loop + flicker light.
- CLAIM glow: soft pulsing rim bloom; on success a one-shot **gold coin/spark burst** + screen-space sparkle from the claimed cell to the top-bar currency chip.
- TopCrest gem: occasional specular glint streak.
- Vignette + subtle dust motes drifting in the dark field (very low alpha).

## K · EVENT BEHAVIOR
- **OnShow:** query server-auth streak state (read-only) → set per-day {claimed/claimable/locked}, current streak count, today index; play entry timeline; if today already claimed → all played cells ✓, CLAIM = disabled "CLAIMED". (Client never computes rewards; it renders server state.)
- **OnClaim:** disable button immediately; send claim request (stub/server-auth); on ack: mark today ✓, grant reward (currency count-up handled by hub), play collect burst + fly-to-chip, set CLAIM→"CLAIMED" disabled, persist; on failure: re-enable + toast via shared error sheet (39).
- **OnClaim when none available:** shake + toast "Already claimed — come back tomorrow!" (no request).
- **OnClose / ✕ / back:** fade out, pop screen from UiRouter stack → return to hub.
- **Day7 special:** EPIC CHEST reward routes to Chest-Open-Result (21) reveal flow when its day is claimed (ADR-gated content; behavior noted, not altered here).
- **No timers tick visually** except the inherited right-edge EVENT/BATTLE tiles (decorative bleed). Streak reset logic is server-side; client only displays.

## L · NEGATIVE RULES
- Do **not** make claimed/locked day cells tappable; only CLAIM and ✕ are interactive.
- Do **not** let the player claim more than one day per server-day; never client-authoritative on rewards/streak.
- Do **not** render the inherited **EnergyChip (5R/120)** as a live system — it is **canon-CUT**; at implementation omit/replace (kept here only as forensic record of the source art).
- Do **not** treat the hub bleed (rail/tiles/top bar) as functional while this screen is open — raycast OFF, alpha low.
- Do **not** restyle Day7 to gold — its **violet/amethyst** premium identity is load-bearing.
- Do **not** invent extra days, numbers, or rewards beyond the 7 shown; values are exactly: 5,000 / 10,000 / 100 / 1 / 50 / 200 / EPIC CHEST.
- No portrait layout; no real-time anything.

## M · ACCEPTANCE CRITERIA (≥95%)
1. 7 day-cells in one row, correct order & exact values/labels; Day3 highlighted with gold halo; Days1-2 ✓; Days4-7 🔒; Day7 violet/premium with "EPIC CHEST + Legendary Unit Guaranteed!".
2. Ornate gold double-frame + top crest + ✕; title "DAILY REWARD" gold-bevel serif; subtitle exact text.
3. Footer strip: flame + "STREAK: 3 DAYS" + sub line + gold **CLAIM** pill right-aligned.
4. Layout matches fraction math within ±2% at 2340×1080; safe-area respected; full-bleed backdrop under cutout.
5. Entry/idle/claim animations present with correct order & easing; CLAIM disabled state correct.
6. Palette hexes within specified ranges; gold/gem/halo bloom present, matte panel not blooming.
7. Energy chip flagged/omitted; no extra interactivity on day cells.

## N · IMPLEMENTATION CONFIDENCE
**92/100.** High: clean modal, finite known content, exact visible values, standard HLG layout. Deductions: (−3) exact corner-cartouche/crest geometry & frame filigree are art-asset dependent (need matching 9-slice sprites); (−3) precise halo/glow radii and bloom thresholds are eyeballed; (−2) Day7 chest→reveal hand-off and streak-advance micro-animation are inferred, not shown in a static frame.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction-based math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded; nothing invented.
- [x] List structure (DayRow) typed (HLG, not ScrollRect) with rationale.
- [x] Component states (DayCell, CLAIM, ✕) enumerated incl. disabled.
- [x] Animation timeline with timestamps/easing; particle/FX listed.
- [x] ADR/canon flags noted (energy CUT; Day7 reward gacha-adjacent) without altering forensic spec.
- [x] Landscape, safe-area, full-bleed-under-cutout rules applied.
- [x] No code/asset/scene changes; spec is documentation only.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 29 · Lucky Spin

Source: design/LuckySpinDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops **prize wheel** (8-segment) with one **free daily spin** + a paid **×10** spin. **⚠️ GACHA — REQUIRES ADR.** Lucky Spin is randomized-reward loot mechanics colliding with the "no loot boxes/gacha" principled CUT (00 §5). This spec is **forensic only** (records the art exactly); the ADR governs whether/how it ships (e.g., transparent deterministic redesign, posted odds, no paid spins). The **SPIN ×10 / 450** paid action and "Better rewards guaranteed" copy are the highest-risk elements — flag, do not soften the spec.

---

## A · SCREEN PURPOSE
A daily gacha/reward wheel. Layout is a **three-column** composition over a dark hub backdrop: a **left info rail** (next-free-spin countdown + "How it works"), a large **central rotating prize wheel** (lion-head hub + 8 reward segments + a fixed top pointer), and a **right action stack** ("Spin & Win" banner + **SPIN — FREE** CTA + **SPIN ×10 (450)** CTA). A **RECENT WINS** ticker row spans the bottom. The player spins (free once/day, or pays gems for ×10); the wheel decelerates onto a segment and grants that reward. Entry via bottom-hub "Spin" entry; close via top-right **✕**.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** dimmed hub, heavy vignette, warm god-ray behind the wheel; a strong **focal glow halo** behind the wheel rim.
- **Wheel:** the hero subject — a circular ornate **gold/bronze rim** with riveted bevel and inset gem studs, divided into **8 colored pie segments** (alternating jewel tones: amethyst-violet, slate-steel, oxblood/bronze, cobalt-blue, mossy-green, ember-orange), each holding a reward icon + label. A cast-gold **lion-head boss** centerpiece. A **fixed pointer/marker** sits at the top of the rim (gold arrow/crest).
- **Left rail:** dark slate sub-panels with bronze edges; a clock glyph + countdown; a small bulleted "how it works" card.
- **Right stack:** a furled **cobalt banner** ("SPIN & WIN / GREAT REWARDS EVERY TIME"); **blue gloss CTA** (SPIN — FREE) and **gold CTA** (SPIN ×10) with a gem cost.
- **Bottom ticker:** dark strip, "RECENT WINS" centered header, a row of {avatar · name · prize · "Xm ago"} entries.
- **Mood:** "casino of the gods" — opulent gold wheel glowing against black; CTAs are the brightest interactive objects.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
LuckySpinScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (hub art/snapshot, under cutout)
│  ├─ DimScrim (black ~68%)
│  ├─ Vignette
│  ├─ GodRayCone (additive, behind wheel)
│  └─ WheelFocalGlow (radial, behind wheel)
├─ InheritedHubChrome (low-alpha bleed; non-interactive)
│  ├─ TopBar: CurrencyChip_Gold "125,450", CurrencyChip_Gems "2,850"
│  └─ LeftRail icons (Campaign, Army, Commanders, Quests, Store, Alliance, Events)
├─ CloseButton ✕ (top-right)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ Header (top-center)
   │  ├─ Title "LUCKY SPIN" (serif gold-bevel UPPER)
   │  └─ Subtitle "Spin the wheel and win amazing rewards!"
   ├─ LeftInfoColumn (vertical)
   │  ├─ NextSpinCard
   │  │  ├─ Label "NEXT FREE SPIN"
   │  │  ├─ ClockIcon
   │  │  └─ Countdown "03:11:00"
   │  └─ HowItWorksCard
   │     ├─ Header "How it works"
   │     └─ Bullets ["Spin daily for free", "Win rewards", "Better rewards on 10x spins!"]
   ├─ WheelGroup (center)
   │  ├─ WheelRimFrame (ornate gold ring + studs)
   │  ├─ WheelDisc (ROTATES — the 8 segments)
   │  │  ├─ Seg0 "EXCLUSIVE AVATAR" (icon avatar)          [slate]
   │  │  ├─ Seg1 "500 GEMS" (purple gems icon)             [amethyst]   ← top in still
   │  │  ├─ Seg2 "100K SILVER" (silver coins icon)          [steel]
   │  │  ├─ Seg3 "EPIC CHEST" (ornate chest icon)           [bronze/gold]
   │  │  ├─ Seg4 "COMMANDER SHARD x10" (shard icon)         [ember-orange]
   │  │  ├─ Seg5 "250 GEMS" (blue gems icon)                [cobalt]
   │  │  ├─ Seg6 "1 DAY SPEED UP" (hourglass/clock icon)    [mossy-green]
   │  │  └─ Seg7 "RARE CHEST" (wood chest icon)             [dark-bronze]
   │  ├─ WheelHub (lion-head gold boss, static, on top)
   │  └─ WheelPointer (fixed top marker/arrow)
   ├─ RightActionColumn (vertical)
   │  ├─ SpinWinBanner (cobalt furled banner: "SPIN & WIN" / "GREAT REWARDS" / "EVERY TIME")
   │  ├─ SpinFreeButton (blue): label "SPIN — FREE" + sub "1 free spin per day"
   │  └─ SpinX10Button (gold): label "SPIN ×10" + cost "450" (gem icon) + sub "Better rewards guaranteed!"
   └─ RecentWinsTicker (bottom strip)
      ├─ TickerHeader "RECENT WINS" (with flank rule lines)
      └─ WinRow (HorizontalLayoutGroup, 6 entries)
         ├─ WinItem{avatar, "ThaneOrlok", "50K Silver", "2m ago"}
         ├─ WinItem{avatar, "LadyMorrigan", "250 Gems", "5m ago"}
         ├─ WinItem{avatar, "Grimblade", "Rare Chest", "8m ago"}
         ├─ WinItem{avatar, "IronWolf", "Exclusive Avatar", "12m ago"}
         ├─ WinItem{avatar, "ValenShield", "100K Silver", "15m ago"}
         └─ WinItem{avatar, "Stormrider", "Commander Shard ×10", "18m ago"}
```
*(Segment reward set is fixed at 8; the order above is the still's clockwise reading from the violet "500 GEMS" segment at top.)*

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LuckySpinScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| WheelFocalGlow | backdrop | 3 | Image (additive) | center | .5,.5 | — | — | scale w/ wheel |
| InheritedHubChrome | screen | 1 | Rect (α≈.25, raycast OFF) | stretch | .5,.5 | — | inside safe | non-interactive |
| CloseButton | screen | 2 | Button+Image | top-right | 1,1 | — | inside safe | fixed offset |
| SafeAreaRoot | screen | 3 | Rect+SafeAreaFitter | stretch | .5,.5 | — | insets | drives content |
| Header | SafeAreaRoot | 0 | VerticalLayoutGroup | top-center | .5,1 | center | — | — |
| LeftInfoColumn | SafeAreaRoot | 1 | VerticalLayoutGroup | mid-left | 0,.5 | top-left | — | fixed width frac |
| NextSpinCard | LeftInfoColumn | 0 | Image+VLG | — | .5,.5 | center | — | — |
| HowItWorksCard | LeftInfoColumn | 1 | Image+VLG | — | 0,.5 | left | — | — |
| WheelGroup | SafeAreaRoot | 2 | Rect (square, aspect-fit) | center | .5,.5 | center | — | size = min(0.5·W,0.78·H) |
| WheelRimFrame | WheelGroup | 0 | Image (circular) | stretch-all | .5,.5 | — | — | scales w/ group |
| **WheelDisc** | WheelGroup | 1 | **Image (radial sprite) OR 8× pie Image** + rotation driver | stretch-all | .5,.5 (center pivot!) | — | — | **rotates about center** |
| Seg_n content | WheelDisc | 0..7 | (Icon+Text) parented at 45° increments, radius offset | center+rotated | varies | center | — | rotates with disc |
| WheelHub | WheelGroup | 2 | Image (lion boss) | center | .5,.5 | center | — | static overlay |
| WheelPointer | WheelGroup | 3 | Image (arrow) | top-center | .5,1 | — | — | static, marks landing |
| RightActionColumn | SafeAreaRoot | 3 | VerticalLayoutGroup | mid-right | 1,.5 | center | — | fixed width frac |
| SpinWinBanner | RightActionColumn | 0 | Image+Text | — | .5,.5 | center | — | — |
| SpinFreeButton | RightActionColumn | 1 | Button+Image+VLG | — | .5,.5 | center | — | full column width |
| SpinX10Button | RightActionColumn | 2 | Button+Image+HLG(cost) | — | .5,.5 | center | — | full column width |
| RecentWinsTicker | SafeAreaRoot | 4 | Image+VLG | bottom-stretch | .5,0 | center | — | spans inner width |
| TickerHeader | RecentWinsTicker | 0 | Text + 2 rule Images | top-center | .5,1 | center | — | — |
| WinRow | RecentWinsTicker | 1 | **HorizontalLayoutGroup** (6 items, equal) | center | .5,.5 | mid-center | — | items flex; overflow → marquee/scroll |
| WinItem_n | WinRow | 0..5 | Image+VLG (avatar,name,prize,time) | — | .5,.5 | center | — | — |

**List/wheel note:**
- **WheelDisc** is the canonical rotating element → a single radial sprite (preferred for crisp segment art) OR 8 pie `Image`s under one pivot-centered RectTransform; rotation animated via `localEulerAngles.z`. Segment content (icon+label) is parented at 45° steps, radius ≈ 0.62·R, each rotated so text faces outward/upright at rest.
- **WinRow** is a horizontal list → `HorizontalLayoutGroup`; if entries exceed the strip, convert to a **looping marquee** (auto-scroll) or horizontal `ScrollRect`.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Header:** top inset 0.05·H; title cap region ~0.09·H centered; subtitle below.
- **WheelGroup:** square, side = **min(0.50·W, 0.80·H) ≈ 864 px** (height-bound), centered horizontally at ≈0.50·W, vertical center ≈0.52·H (slightly low to clear header). Radius R ≈ 432 px.
  - Rim frame thickness ≈ 0.10·R ≈ 43 px; gem studs ≈ 0.05·R spaced every 45°.
  - 8 segments × 45°. Segment label ring radius ≈ 0.62·R; icon centered ≈ 0.55·R (icon ⌀ ≈ 0.20·R), label arc/baseline ≈ 0.78·R.
  - WheelHub (lion boss) ⌀ ≈ 0.34·R ≈ 147 px, centered.
  - WheelPointer at 12 o'clock, height ≈ 0.16·R, tip touching rim inner edge.
- **LeftInfoColumn:** width ≈ 0.20·W ≈ 468 px; left inset ≈ 0.03·W; vertical center ≈ 0.52·H. NextSpinCard height ≈ 0.16·H; HowItWorksCard height ≈ 0.26·H; gap 0.03·H. Countdown text ≈ 0.05·H tall.
- **RightActionColumn:** width ≈ 0.22·W ≈ 515 px; right inset ≈ 0.03·W; vertical center ≈ 0.50·H. SpinWinBanner height ≈ 0.20·H; SpinFreeButton height ≈ 0.12·H; SpinX10Button height ≈ 0.13·H (slightly taller, has cost row); gaps 0.025·H. Button sub-text strip ≈ 0.035·H beneath each.
- **RecentWinsTicker:** spans inner width (≈0.94·W), height ≈ 0.16·H, bottom inset ≈ 0.03·H. Header rule lines flank centered text. WinRow: 6 items, gap ≈ 0.01·W; each item width ≈ (rowW − 5·gap)/6; avatar ⌀ ≈ 0.045·H, name/prize/time stacked.

**Tablet (4:3):** match-height keeps wheel size; side columns gain margin, may widen slightly; wheel stays centered. **Ultrawide (21:9):** more backdrop; columns push toward edges but cap inset so they don't drift off-balance; wheel unchanged. **Notch:** SafeAreaRoot insets the three columns + ticker; full-bleed backdrop/glow remain under cutout.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "LUCKY SPIN" | serif Trajan display | Black | UPPER | +6% | 1.0 | gold bevel + bloom + 2px stroke | ~72 | #f0d27a→#caa04a; stroke #3a2c0e |
| Subtitle | light serif/clean | Reg | Sentence | 0 | 1.1 | shadow | ~24 | #d9c79a |
| "NEXT FREE SPIN" | condensed caps | SemiBold | UPPER | +4% | 1.0 | shadow | ~22 | #cdbf99 |
| Countdown "03:11:00" | mono/numeric, tense | Bold | — | +2% | 1.0 | soft glow | ~40 | #ffe08a |
| "How it works" | serif small-caps | SemiBold | Title | +2% | 1.0 | shadow | ~24 | #e8dcc0 |
| Bullets | clean sans | Reg | Sentence | 0 | 1.2 | — | ~20 | #c7bfa8 |
| Segment labels (e.g. "500 GEMS","EPIC CHEST") | condensed caps, punchy | Bold | UPPER | +2% | 0.95 | dark stroke + slight glow; color by tier | ~22–24 | white #fff / gold #f0d27a / gem-tint per segment |
| Banner "SPIN & WIN / GREAT REWARDS / EVERY TIME" | serif display, hype | Heavy | UPPER | +4% | 1.0 | gold-on-cobalt, bevel | line1 ~30, l2 ~26, l3 ~24 | #f4e6b0 on #1d3a8a |
| "SPIN — FREE" | serif display, action | Heavy | UPPER | +6% | 1.0 | top highlight + glow | ~38 | #ffffff on blue |
| "1 free spin per day" | clean sans | Reg | Sentence | 0 | 1.0 | shadow | ~18 | #b9c6e6 |
| "SPIN ×10" | serif display, action | Heavy | UPPER | +6% | 1.0 | dark engrave on gold | ~36 | #2a1c06 on gold |
| Cost "450" | numeric bold | Bold | — | 0 | 1.0 | shadow; gem icon left | ~30 | #ffffff |
| "Better rewards guaranteed!" | clean sans, italic | Italic | Sentence | 0 | 1.0 | shadow | ~18 | #d9c79a |
| "RECENT WINS" | condensed caps | SemiBold | UPPER | +6% | 1.0 | flank rules | ~22 | #cdbf99 |
| Win name | clean sans | SemiBold | — | 0 | 1.0 | shadow | ~18 | #e8e2cf |
| Win prize | clean sans | Reg | — | 0 | 1.0 | tint by reward | ~16 | gem #6f8fff / silver #c8ccd6 / chest #d9b06a |
| Win time "Xm ago" | clean sans, quiet | Reg | — | 0 | 1.0 | — | ~14 | #8a8472 |

## G · MATERIALS
- **Wheel rim:** brushed antique gold/bronze #6b5320→#caa04a→#f0d27a→#fff2c2, beveled, riveted studs (gem-blue/red insets) with specular pips + **bloom**; worn edges; warm rim-light.
- **Segments (jewel pie):** semi-glossy radial gradients — amethyst #4a2a7a→#9e6bf0 (500 GEMS), steel #3a414f→#7c8696 (100K SILVER), bronze/gold #5a431c→#caa04a (EPIC CHEST), ember #7a3a14→#e0742a (COMMANDER SHARD), cobalt #1f356f→#4f8bff (250 GEMS), moss #294a2c→#5aa05f (1 DAY SPEED UP), dark-bronze #3a2c14→#8a6a30 (RARE CHEST), slate #2a2f3a→#5a6272 (EXCLUSIVE AVATAR); thin gold divider spokes between segments.
- **Lion hub:** cast gold, high relief, glossy, strong specular + bloom; small inset gem.
- **Pointer:** gold arrow/crest, beveled, slight glow at tip.
- **Left/ticker panels:** obsidian #0c0e15→#161a24, bronze edges, matte; clock glyph gold.
- **SpinFreeButton:** royal/cobalt #2b56c8→#4f8bff gloss, top highlight, beveled gold trim, soft outer glow.
- **SpinX10Button:** brushed gold (as CTA), beveled, gem icon crystalline violet.
- **Banner:** cobalt cloth #16306e→#244a9c with stitched gold trim, soft folds.
- **Icons:** gems crystalline cobalt/violet with inner glow; silver cool specular; chests aged wood + bronze; shard glowing orange crystal; hourglass gold; avatar framed bust.
- **Bloom:** wheel rim, hub, gems, CTA glows, focal halo. Matte panels do not bloom.

## H · COMPONENTS (states)
**WheelDisc** — *rest:* segment labels upright, slow idle shimmer on rim. *spinning:* fast rotation, motion-blur streaks on rim, segment labels blur. *decelerating:* ease-out into target. *landed:* target segment flashes/pulses + pointer recoils + win burst. The wheel is driven, not directly draggable in this design (button-initiated).

**SpinFreeButton:**
- *idle/enabled:* blue gloss, glow, breathing pulse (free spin available).
- *hover/focus:* +brightness, glow+.
- *pressed:* scale 0.96, inner shadow.
- *disabled* (free spin used → countdown running): desaturate to grey-blue #3a4a6a, label #6a748c, sub shows "Next free spin in 03:11:00", raycast on → tap = shake + toast.
- *spinning:* both spin buttons lock (disabled-look + spinner) until result resolves.

**SpinX10Button:**
- *idle/enabled (affordable):* gold gloss, gem cost "450", glow.
- *hover/pressed:* as gold CTA.
- *disabled-unaffordable* (gems < 450): cost text turns red #d8452b, button greyed, tap → "Not enough Gems" via shared insufficient sheet (37) with Store deep-link.
- *confirm:* tapping ×10 opens a confirm sheet (37) "Spend 450 Gems for 10 spins?" before charging (server-auth).

**CloseButton ✕:** dark disc + bronze ring; hover brighten; pressed 0.92.

**WinItem:** static; newest can slide in from left when a real win posts.

**NextSpinCard countdown:** ticks down; at 00:00:00 → free spin re-enabled, SpinFreeButton flips to enabled + glow pop.

## I · ANIMATION TIMELINE
**OnShow (~0.9 s):**
- 0.00 scrim fade-in + god-ray + focal glow (0.25 s).
- 0.05 WheelGroup scale 0.90→1.00 + α (0.30 s, ease-out-back); rim glint sweep.
- 0.10 wheel **idle slow rotation** begins (very slow, ±, ambient) OR gentle rock; lion hub specular shimmer loop (3 s).
- 0.20 Header α + 1.04→1.0 (0.20 s).
- 0.28 LeftInfoColumn slide-in from left (x −24→0 + α, 0.22 s); countdown starts ticking.
- 0.34 RightActionColumn slide-in from right (0.22 s); banner unfurl (scaleY 0.85→1.0, 0.25 s).
- 0.55 SpinFreeButton pop + glow on (0.18 s), begin breathe loop.
- 0.62 RecentWinsTicker fade-up; WinRow stagger L→R (0.04 s apart).

**OnSpin (free or ×10) — core sequence:**
- t0 button press 0.96; both buttons → disabled/locked; SFX.
- t0+0.05 **wind-up**: wheel briefly rotates *backward* ~8° (0.15 s, ease-in).
- t0+0.20 **accelerate**: spin forward, easing-in to max angular velocity over ~0.6 s; rim motion-blur on; segment labels blur.
- t0+0.80 **cruise** at max velocity ~1.0–1.4 s (constant), pointer ticks (per-segment SFX click).
- ~t0+2.2 **decelerate**: ease-out (cubic/quint) over ~2.0–2.6 s onto the server-decided target segment angle (client never picks the prize); add a tiny overshoot+settle (±2°, 0.25 s) for "click into place".
- on settle: pointer recoil bounce; **target segment flash** (white→tier color, 0.3 s) + radial burst from segment; lion hub roar-glint.
- +0.3 **reward reveal**: hand off to Reward Grant (38) / Chest-Open-Result (21) for chest/avatar; currency rewards count-up at top bar with fly-to-chip.
- ×10 variant: either spin 10× rapid sequential settles, or a single spin → a 10-item reward summary panel (implementation/ADR choice; reveal via 38 multi-grant).
- after reveal: re-enable buttons per availability (free → disabled+countdown; ×10 → enabled if gems remain).

**Idle loops:** rim shimmer; hub specular; free-button breathe; countdown tick; focal glow slow pulse; occasional WinItem slide-in.

**OnClose:** wheel stops, panel scale→0.94 + α→0, scrim out (~0.30 s).

## J · PARTICLE & FX
- Focal radial glow + god-ray behind wheel (additive, slow).
- Rim/hub specular glints (periodic streaks); gem studs twinkle.
- During spin: motion-blur smear on rim, faint sparks trailing pointer ticks.
- On landing: **win burst** (gold sparks + colored confetti tinted to the reward) radiating from the winning segment; pointer impact spark; screen-space sparkle fly to currency chip for gem/silver wins.
- CTA glows (blue breathe, gold pulse); banner subtle cloth sway.
- Dust motes drifting in dark field; vignette.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth state (read-only): free-spin availability + countdown, gem balance, recent-wins feed, segment table. Render; play entry. (Odds/segment weights are server-side and must be disclosed per ADR.)
- **OnSpin (FREE):** if available → lock buttons, request spin → server returns target segment + reward; play spin→decel-to-target→reveal; mark free used, start countdown, persist; if not available → shake + toast.
- **OnSpin (×10):** open confirm sheet (37) "Spend 450 Gems for 10 spins?"; on confirm → charge via server-auth (client never mutates balance), run ×10 sequence/summary, reveal rewards, deduct gems w/ count-down at chip; on insufficient → insufficient sheet (37) → Store.
- **Reward grant:** all grants are server-authoritative; client only animates the returned results (38/21). No client-side RNG for the prize.
- **OnClose / ✕ / back:** if a spin is in progress, ignore close until resolved (or queue); else fade out, pop screen.
- **Countdown reaches 0:** enable free spin + glow pop, no reload.
- **ADR hooks (note, do not alter spec):** posted-odds panel, no-paid-spin or pity/transparent variant, and "guaranteed" copy review all live in the ADR, not this forensic record.

## L · NEGATIVE RULES
- **⚠️ Do not ship the paid/random mechanic without the ADR.** This spec documents the art; it does not authorize gacha. Keep the flag visible to implementers.
- Do **not** let the **client pick the winning segment** — server-authoritative result only; the wheel must decelerate to the server's chosen angle.
- Do **not** charge gems before the ×10 confirm sheet; never mutate balance client-side.
- Do **not** allow spinning while a spin is unresolved (lock both buttons).
- Do **not** invent segments/odds — exactly the 8 rewards listed; no implied probabilities in the UI unless the ADR adds a posted-odds panel.
- Do **not** make the wheel free-drag/flick in this design (button-initiated only) unless ADR/UX adds it.
- Do **not** treat hub bleed as interactive (raycast off, low alpha).
- No portrait; no real-time PvP implications (this is solo gacha).

## M · ACCEPTANCE CRITERIA (≥95%)
1. 8-segment gold-rim wheel with lion-head hub + fixed top pointer; exact reward set/labels/colors as listed; segments at 45° with upright labels.
2. Left column: "NEXT FREE SPIN" + clock + "03:11:00" countdown + "How it works" with the 3 exact bullets.
3. Right column: cobalt "SPIN & WIN / GREAT REWARDS / EVERY TIME" banner; blue **SPIN — FREE** (+"1 free spin per day"); gold **SPIN ×10** with gem cost **450** (+"Better rewards guaranteed!").
4. Bottom "RECENT WINS" ticker with the 6 exact entries (name/prize/time).
5. Title "LUCKY SPIN" gold-bevel serif + subtitle; ✕ top-right; currency chips 125,450 / 2,850.
6. Spin animation = wind-up → accel → cruise → ease-out decel → settle-into-segment → win burst → reward reveal; server-authoritative landing; both buttons lock during spin.
7. Disabled states correct (free→countdown; ×10→unaffordable red); ADR gacha flag present in doc.
8. Layout within ±2% of fraction math at 2340×1080; safe-area + full-bleed-under-cutout honored; palette within ranges; wheel/CTA bloom present.

## N · IMPLEMENTATION CONFIDENCE
**88/100.** High: clear three-column comp, finite known content, standard CTA/list patterns. Deductions: (−4) the rotating wheel needs precise segment art (radial sprite or 8 pies) + correct pivot/rotation math and per-segment label orientation — fiddly to hit 1:1; (−3) the ×10 flow (sequential vs summary) and exact decel curve/overshoot are inferred from a static frame; (−3) ornate rim/hub/pointer geometry & gem studs are art-asset dependent; (−2) ADR may change mechanics/odds-panel, shifting the build. Forensic *visual* fidelity is high; the variability is in motion + ADR.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (segments, CTAs, countdown, 6 win rows); nothing invented.
- [x] Wheel typed as rotating radial/pie under center pivot; WinRow typed as HLG/marquee with rationale.
- [x] Component states (wheel, both spin buttons incl. disabled/unaffordable, ✕) enumerated.
- [x] Full spin animation timeline (wind-up→accel→cruise→decel→settle→reveal) with easings; particles/FX listed.
- [x] **GACHA ADR flag** raised prominently (header, K, L, N) without altering forensic spec.
- [x] Server-authoritative result/charge rules stated; no client RNG, no client balance mutation.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 30 · Free Rewards

Source: design/FreeRewardsDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops **opt-in rewarded-ads** screen — a vertical list of 4 offer rows, each a "watch a short video → earn X" action, gated by a daily cap (**3/5 today**) with a reset timer. Canon note (do NOT alter forensic spec): **rewarded ads are ALLOWED when opt-in** (00 §5 / global context) — this is the *compliant* monetization screen; copy already says "Ads are short and optional." No ADR block here; just keep it strictly opt-in (no auto-play, no forced ads).

---

## A · SCREEN PURPOSE
A full-screen (non-modal) hub sub-page that lists rewarded-ad offers. Standard hub chrome on top: **Back** (top-left) + **currency chips with +** (top-right). A **daily-limit bar** shows watches used (3/5) and a reset countdown. The body is a **vertical list of 4 offer rows**; each row = {video thumbnail w/ play overlay, title + description, reward chip, **WATCH** button, per-offer "1/1" availability}. A **RECENT WINS** footer shows recent claimed rewards + a reassurance line. Tapping **WATCH** plays an opt-in rewarded ad and grants the reward. Reached from the bottom-hub "Free" entry; **Back** returns to hub.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** near-black obsidian field with faint vignette + subtle warm top glow; no central hero subject — this is a utilitarian *list* screen, calmer than the wheel/calendar.
- **Header band:** thin top region with gold serif title + subtitle; **Back** is a gold-framed square arrow tile (top-left); currency chips top-right each with a small **+** mint button.
- **Limit bar:** a slim full-width sub-bar (calendar glyph + "DAILY LIMIT 3/5 today" left, clock + "Resets in: 14h 23m" right).
- **Offer rows:** wide dark slate panels with thin **bronze/gold edge**, rounded; left a rectangular **art thumbnail** (faction/loot scene) with a circular **gold play-triangle** overlay; center text block; a small **reward chip**; a **blue gloss WATCH** button at right with a tiny "1/1" counter beneath. Rows are visually identical in structure, varied by art/reward.
- **Footer:** dark strip, "RECENT WINS" left with mini reward chips, reassurance copy right.
- **Mood:** trustworthy, generous, low-pressure; gold accents + blue CTAs on black; the **WATCH** buttons are the brightest interactive objects.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
FreeRewardsScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (dark art, under cutout)
│  ├─ Vignette
│  └─ TopGlow (soft warm, top)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ TopBar
   │  ├─ BackButton (gold-framed ← tile, top-left)
   │  ├─ TitleBlock
   │  │  ├─ Title "FREE REWARDS" (serif gold-bevel UPPER)
   │  │  └─ Subtitle "Watch ads to earn valuable rewards!"
   │  └─ CurrencyChips (top-right, HorizontalLayoutGroup)
   │     ├─ Chip_Gold   {icon, "128,450", PlusBtn}
   │     ├─ Chip_Silver {icon, "87,360",  PlusBtn}
   │     └─ Chip_Gems   {icon, "2,850",   PlusBtn}
   ├─ DailyLimitBar
   │  ├─ Left: CalendarIcon + "DAILY LIMIT" + Count "3/5 today"
   │  └─ Right: ClockIcon + "Resets in:" + Timer "14h 23m"
   ├─ OfferList (VerticalLayoutGroup, 4 rows)  [wrap in ScrollRect if >4]
   │  ├─ OfferRow_Gem
   │  │  ├─ Thumb {art, PlayOverlay}
   │  │  ├─ Text {Title "Gem Cache", Desc "Watch a short video\nto earn Gems!"}
   │  │  ├─ RewardChip {gem icon, "+50", "Gems"}
   │  │  └─ Action {WatchButton "WATCH", AvailTag "1/1"}
   │  ├─ OfferRow_Silver
   │  │  ├─ Thumb {coins art, PlayOverlay}
   │  │  ├─ Text {Title "Silver Stash", Desc "Watch a short video\nto earn Silver!"}
   │  │  ├─ RewardChip {silver icon, "+200", "Silver"}
   │  │  └─ Action {WatchButton "WATCH", "1/1"}
   │  ├─ OfferRow_Chest
   │  │  ├─ Thumb {chest art, PlayOverlay}
   │  │  ├─ Text {Title "Free Chest", Desc "Watch a short video\nto earn a Chest!"}
   │  │  ├─ RewardChip {chest icon, "FREE", "Chest"}
   │  │  └─ Action {WatchButton "WATCH", "1/1"}
   │  └─ OfferRow_Boost
   │     ├─ Thumb {warrior art, PlayOverlay}
   │     ├─ Text {Title "Battle Boost", Desc "Watch a short video\nto earn a 60m Boost!"}
   │     ├─ RewardChip {shield/clock icon, "60m", "Speed Up"}
   │     └─ Action {WatchButton "WATCH", "1/1"}
   └─ FooterStrip
      ├─ Left: "RECENT WINS" + MiniChips ["+50"(gem), "+200"(silver), "+50"(gem), "+200"(silver), chest]
      └─ Right: InfoIcon + "Ads are short and optional. Thanks for your support!"
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| FreeRewardsScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| SafeAreaRoot | screen | 1 | Rect+SafeAreaFitter | stretch-all | .5,.5 | — | insets | drives content |
| TopBar | SafeAreaRoot | 0 | Rect | top-stretch | .5,1 | — | inside safe | height fixed frac |
| BackButton | TopBar | 0 | Button+Image | top-left | 0,1 | — | — | fixed size |
| TitleBlock | TopBar | 1 | VerticalLayoutGroup | top-left (after back) | 0,1 | left | — | — |
| CurrencyChips | TopBar | 2 | HorizontalLayoutGroup | top-right | 1,1 | mid-right | — | right-anchored |
| Chip_x | CurrencyChips | 0..2 | Image+HLG (icon,num,plus) | — | .5,.5 | center | — | — |
| DailyLimitBar | SafeAreaRoot | 1 | Image+HLG (space-between) | top-stretch (below TopBar) | .5,1 | mid spread | inside safe | spans width |
| OfferList | SafeAreaRoot | 2 | **VerticalLayoutGroup** (ctrl-size, spacing) | stretch (mid band) | .5,.5 | top-center | inside safe | rows equal height; **ScrollRect if overflow** |
| OfferRow_x | OfferList | 0..3 | Image + HorizontalLayoutGroup (LayoutElement.preferredHeight) | — | .5,.5 | mid-left→right | — | full width; internal columns fixed frac |
| ↳ Thumb | OfferRow | 0 | Image + PlayOverlay(Button) | left | 0,.5 | — | — | fixed width frac |
| ↳ PlayOverlay | Thumb | 0 | Button+Image (circle ▷) | center | .5,.5 | — | — | centered on thumb |
| ↳ Text | OfferRow | 1 | VerticalLayoutGroup | — | 0,.5 | left | — | flex-grow |
| ↳ RewardChip | OfferRow | 2 | HorizontalLayoutGroup (icon+amount+unit) | — | .5,.5 | center | — | fixed width frac |
| ↳ Action | OfferRow | 3 | VerticalLayoutGroup (button + tag) | right | 1,.5 | center | — | fixed width frac |
| ↳↳ WatchButton | Action | 0 | Button+Image | — | .5,.5 | center | — | fixed |
| ↳↳ AvailTag "1/1" | Action | 1 | Text (on small plate) | — | .5,1 | center | — | — |
| FooterStrip | SafeAreaRoot | 3 | Image+HLG (space-between) | bottom-stretch | .5,0 | mid spread | inside safe | spans width |
| RecentWins (left) | FooterStrip | 0 | HLG (label + mini chips) | left | 0,.5 | mid-left | — | — |
| InfoNote (right) | FooterStrip | 1 | HLG (icon + text) | right | 1,.5 | mid-right | — | — |

**List note:** OfferList is the canonical list → `VerticalLayoutGroup` with per-row `LayoutElement.preferredHeight`. All 4 fit at 2340×1080, so no scroll is required; **wrap in a vertical `ScrollRect`** if future offers exceed the visible band (keep header/limit-bar/footer pinned outside the viewport). Currency chips + RecentWins mini-chips are small `HorizontalLayoutGroup`s.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar:** height ≈ 0.13·H ≈ 140 px; top inset ≈ 0.02·H.
  - BackButton ⌀/side ≈ 0.075·H ≈ 81 px square; left inset ≈ 0.025·W.
  - TitleBlock left of center, after Back: title cap ≈ 0.06·H, subtitle ≈ 0.028·H below.
  - CurrencyChips: each chip width ≈ 0.13·W; height ≈ 0.05·H; gap ≈ 0.01·W; +button ⌀ ≈ 0.035·H at chip right; right inset ≈ 0.02·W.
- **DailyLimitBar:** full inner width (≈0.95·W), height ≈ 0.055·H ≈ 60 px, y just below TopBar. Left cluster (calendar + "DAILY LIMIT" + "3/5 today") and right cluster (clock + "Resets in:" + "14h 23m") space-between with side insets 0.03·W.
- **OfferList:** occupies the central band from ≈0.20·H (below limit bar) to ≈0.88·H (above footer) = height ≈ 0.68·H ≈ 734 px.
  - 4 rows + 3 gaps. Gap ≈ 0.018·H ≈ 19 px → gaps ≈ 57 px → rows share ≈ 677 px → **row height ≈ 169 px (~0.157·H)**. Row width ≈ 0.94·W ≈ 2200 px.
  - **Row internal columns** (fractions of row width): Thumb 0.00–0.14 (≈ 308 px, ~16:9 inset with padding); Text 0.16–0.52 (flex); RewardChip 0.54–0.70 (≈ 350 px); Action 0.78–0.98 (≈ 440 px). PlayOverlay circle ⌀ ≈ 0.55·thumbH centered on thumb.
  - WatchButton: width ≈ 0.14·W ≈ 328 px, height ≈ 0.085·H ≈ 92 px; AvailTag "1/1" plate ≈ 0.06·W × 0.03·H centered beneath.
  - RewardChip: amount text large; unit label small beneath; icon ⌀ ≈ 0.06·H.
- **FooterStrip:** full inner width, height ≈ 0.09·H ≈ 97 px, bottom inset ≈ 0.02·H. Mini chips ⌀ ≈ 0.035·H each, gap ≈ 0.008·W.

**Tablet (4:3):** match-height keeps row heights; rows get more side margin (cap width 0.9·W); thumbnail/columns scale with W. **Ultrawide (21:9):** rows widen but cap max width (~0.8·W centered) so text doesn't stretch absurdly; backdrop fills sides. **Notch:** SafeAreaRoot insets the whole content; full-bleed backdrop under cutout; Back/chips never cross the inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "FREE REWARDS" | serif Trajan display | Black | UPPER | +5% | 1.0 | gold bevel + bloom + 2px stroke | ~64 | #f0d27a→#caa04a; stroke #3a2c0e |
| Subtitle | clean sans/light serif | Reg | Sentence | 0 | 1.1 | shadow | ~24 | #d9c79a |
| Currency numbers | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |
| "DAILY LIMIT" | condensed caps | SemiBold | UPPER | +4% | 1.0 | shadow | ~22 | #cdbf99 |
| Count "3/5 today" | numeric, emphatic | Bold | lower "today" | +1% | 1.0 | "3" brighter | ~24 | "3/5" #ffd76a, "today" #b3ac96 |
| "Resets in:" | clean sans | Reg | Sentence | 0 | 1.0 | — | ~20 | #b3ac96 |
| Timer "14h 23m" | numeric | SemiBold | — | +1% | 1.0 | soft glow | ~24 | #ffe08a |
| Offer Title (e.g. "Gem Cache") | serif small-caps, sturdy | Bold | Title | +2% | 1.0 | shadow | ~32 | #f2e8cf |
| Offer Desc (2 lines) | clean sans, quiet | Reg | Sentence | 0 | 1.15 | — | ~20 | #a9a28c |
| Reward amount "+50"/"+200"/"FREE"/"60m" | numeric/label bold | Heavy | UPPER (FREE) | 0 | 1.0 | glow; color by reward | ~34 | gem #6f8fff, silver #d6dae2, FREE #5fbf6a, boost #6f8fff |
| Reward unit ("Gems"/"Silver"/"Chest"/"Speed Up") | clean sans small | Reg | Title | 0 | 1.0 | — | ~18 | #9a937f |
| "WATCH" | serif display, action | Heavy | UPPER | +6% | 1.0 | top highlight + glow | ~32 | #ffffff on blue |
| AvailTag "1/1" | numeric small | SemiBold | — | 0 | 1.0 | shadow | ~18 | #cfe0ff |
| "RECENT WINS" | condensed caps | SemiBold | UPPER | +5% | 1.0 | — | ~20 | #cdbf99 |
| InfoNote "Ads are short and optional. Thanks for your support!" | clean sans, friendly | Reg | Sentence | 0 | 1.1 | — | ~18 | #8f8872 |

## G · MATERIALS
- **Backdrop:** obsidian #0a0b0f→#14161e, matte, vignette; faint warm top glow #2a2416 additive.
- **Back tile / chips / limit bar:** dark slate #11141d with **bronze/gold edge** #7a5f28→#caa04a; chips have a subtle inner gradient; **+** mint buttons are small gold/green beveled discs.
- **Offer row panel:** #10131c→#181c27 vertical, thin gold-bronze 1–2px edge, rounded corners, soft inner top sheen; faint inner glow on the row whose offer is freshly available.
- **Thumbnail:** full-color loot/faction art, slightly darkened with a vignette; bronze inner frame; **PlayOverlay** = translucent dark disc + gold play-triangle #f0d27a with soft glow.
- **RewardChip:** tiny dark plate or frameless; icons crystalline (gems cobalt/violet inner glow, silver cool specular, chest aged-wood+bronze, boost shield/clock gold-blue).
- **WatchButton:** royal/cobalt #2b56c8→#4f8bff gloss, top highlight, beveled gold/steel trim, soft outer glow; AvailTag on a small dark plate.
- **Bloom:** play-triangles, reward icons, WATCH glows, timer text. Matte panels/backdrop do not bloom.
- **Wear:** subtle worn edges on metal trims; rim-light top-left on bevels.

## H · COMPONENTS (states)
**OfferRow** (informational container; interactive children = PlayOverlay + WatchButton, which trigger the same action):
- *available* (offer's "1/1" unused, under daily cap): full color, WATCH enabled (blue, glow), play-triangle bright.
- *used* (offer's per-day count exhausted, e.g. "0/1"): row dims to ~70%, WATCH → disabled grey-blue, AvailTag "0/1", play-triangle desaturated; tag may show next-reset.
- *cap-reached* (global 3/5 → 5/5): ALL WATCH buttons disabled grey, a banner/tooltip "Daily limit reached — resets in 14h 23m"; rows dim.
- *loading-ad:* WATCH shows spinner + "Loading…", row locks; on no-fill → toast "No ad available, try again" (re-enable).

**WatchButton:**
- *idle/enabled:* blue gloss + glow + subtle breathe.
- *hover/focus:* +brightness, glow+.
- *pressed:* scale 0.96 + inner shadow.
- *disabled:* desaturate grey-blue #3a4a6a, label #6a748c, glow off, raycast on → tap = shake + toast.
- *success (post-watch):* flash white→blue, reward chip pops + flies to currency, AvailTag decrements (1/1→0/1), DailyLimit count increments (3/5→4/5).

**PlayOverlay:** mirrors WatchButton state (same action); hover = triangle brighten + disc darken; disabled when offer used.

**BackButton:** gold tile; hover brighten; pressed 0.92; returns to hub.

**CurrencyChip + / DailyLimit count / Timer:** chips' **+** deep-links to Store; count + timer are live displays (timer ticks; count updates on each successful watch; at reset → counts restore, rows re-enable).

## I · ANIMATION TIMELINE
**OnShow (~0.7 s):**
- 0.00 backdrop fade + top glow (0.20 s).
- 0.05 TopBar slide-down (y +12→0 + α, 0.20 s); chips count-up optional.
- 0.15 DailyLimitBar fade-in (0.15 s); timer starts ticking.
- 0.22 OfferList rows stagger top→bottom: each x −20→0 + α, 0.05 s apart, 0.18 s each (ease-out).
- 0.50 WATCH buttons glow-on + begin breathe loop; play-triangles subtle pulse.
- 0.58 FooterStrip fade-up (0.15 s).

**Idle loops:** WATCH breathe (1.6 s); play-triangle soft pulse (1.4 s); timer tick; reward-icon twinkle (slow); subtle row inner-glow on the top available offer.

**OnWatch (per row, see K):**
- press 0.96 (0.08 s) → WATCH→"Loading…" spinner; row locks.
- ad SDK plays (external) → on complete: return to screen, WATCH flash white→blue, **reward chip pop** (scale 1.0→1.25→1.0, 0.3 s) + sparkle → **fly-to-currency** at top bar (0.5 s) with count-up; AvailTag 1/1→0/1 (cross-fade); DailyLimit 3/5→4/5 (number flip 0.2 s).
- if 5/5 reached → all WATCH dim (0.2 s) + show "limit reached" note.
- on ad dismissed-early/no-reward: re-enable WATCH, no grant, toast.

**OnClose:** rows + bars fade/slide out (~0.25 s); pop screen.

## J · PARTICLE & FX
- Soft warm top glow + vignette + faint dust motes.
- Play-triangle glow pulse; reward-icon twinkles (gem/silver sparkle).
- On successful watch: **reward burst** (small gold/colored sparks) from the row's reward chip + a screen-space sparkle trail flying to the matching top-bar currency chip; count-flip flash.
- WATCH button breathing rim bloom; chip **+** subtle glint.
- No heavy FX — this is a calm utility screen.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth state (read-only): per-offer availability (x/1), global daily count (n/5), reset timer, balances, recent-wins feed. Render rows + states; play entry.
- **OnWatch (PlayOverlay or WATCH):** if offer available AND under cap → set Loading, request rewarded ad via SDK (opt-in, user-initiated); on **reward-earned callback** → server-auth grants reward (client never mutates balance), animate chip→currency, decrement offer avail, increment daily count, persist; on **no-fill / closed-early / error** → re-enable, toast (shared error sheet 39 if needed), no grant.
- **Cap reached (n/5 → 5/5):** disable all WATCH, show limit note; re-enable at reset (timer→0 restores counts via server).
- **Chip +:** deep-link to Store (17). **Back:** fade out, pop to hub.
- **Strictly opt-in:** no ad auto-plays; nothing watches without an explicit WATCH/▷ tap (canon-allowed condition). Reward only granted on genuine completion callback.
- **Timer tick:** client displays server reset time; does not authorize resets locally.

## L · NEGATIVE RULES
- Do **not** auto-play or pre-roll ads — **opt-in only** (the canon allowance depends on this); never grant without the SDK completion callback.
- Do **not** grant rewards client-side or mutate balances on the client — server-authoritative.
- Do **not** exceed the daily cap (5) or a per-offer cap (1) regardless of UI taps; enforce server-side.
- Do **not** invent extra offers/rewards — exactly the 4 rows + values: +50 Gems, +200 Silver, FREE Chest, 60m Speed Up.
- Do **not** restyle the 60m Boost reward as energy/stamina (the canon-cut meter) — it's a time-skip "Speed Up" item, render as such (document if it later collides with cut systems).
- Do **not** drop the reassurance line / "DAILY LIMIT 3/5" / reset timer — they are part of the compliant, transparent presentation.
- No portrait; no real-time anything.

## M · ACCEPTANCE CRITERIA (≥95%)
1. Header: Back tile (top-left), title "FREE REWARDS" gold-bevel serif + subtitle, 3 currency chips with + (top-right) showing 128,450 / 87,360 / 2,850.
2. DailyLimit bar: calendar + "DAILY LIMIT 3/5 today" (left) + clock + "Resets in: 14h 23m" (right).
3. Exactly 4 offer rows in order with correct art-role, titles, 2-line descriptions, reward chips (+50 Gems / +200 Silver / FREE Chest / 60m Speed Up), blue WATCH + "1/1" tag each; play-triangle on each thumbnail.
4. Footer: "RECENT WINS" + 5 mini chips + "Ads are short and optional. Thanks for your support!".
5. Layout within ±2% of fraction math at 2340×1080; rows equal height; VLG (ScrollRect-ready); safe-area + full-bleed-under-cutout honored.
6. States correct: available/used/cap-reached/loading + disabled WATCH; success animation (chip→currency, count increments) present.
7. Palette within ranges; play-triangle/reward/WATCH bloom present; matte backdrop not blooming.
8. Strictly opt-in behavior documented; server-auth grant; no client balance mutation.

## N · IMPLEMENTATION CONFIDENCE
**93/100.** High: classic offer-row list, finite known content, exact labels/values, standard layout groups + CTA states. Deductions: (−3) thumbnail art + reward icons are asset-dependent (need matching sprites); (−2) exact ad-SDK integration / reward-callback wiring is platform-specific (out of pure UI scope) though the UI states are fully specified; (−2) precise reset/cap restore choreography and row inner-glow radii are inferred from a static frame.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (4 offers, caps, timer, chips, footer); nothing invented.
- [x] List typed as VLG (ScrollRect-ready) with rationale; chip rows typed as HLG.
- [x] Component states (rows available/used/cap, WATCH idle/hover/pressed/disabled/loading/success, Back, chips) enumerated.
- [x] Animation timeline with timestamps/easing; particle/FX listed.
- [x] Canon note recorded (rewarded ads ALLOWED iff opt-in) without altering forensic spec; boost-vs-energy nuance flagged.
- [x] Server-auth + strictly-opt-in rules stated; no client balance mutation; reward only on completion.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 31 · Events Hub

Source: design/EventsHubDesign.png · 1536×1024 (1.5:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Live-ops events hub: a large **featured event banner** over a grid of **event cards**, with bottom tabs **Events / Calendar / Past Events**. Canon notes (do NOT alter forensic spec): (1) the **58/120 chip** in the top bar reads as an energy/stamina meter → **canon-CUT**; document, flag, omit at implementation. (2) The **"Arena Clash"** card copy says *"Compete against other players in real-time battles"* — BULWARK has **NO real-time PvP** (00 context). Flag: at implementation relabel to **async ghost** ("Compete against other Commanders" / "async"), per the Online-Battle convention. Forensic spec records the art verbatim.

---

## A · SCREEN PURPOSE
A full-screen hub sub-page for limited-time events. Top: hub chrome (**Back** left, currency chips right) + gold serif title "EVENTS" + subtitle. A prominent **FEATURED** banner promotes the current marquee event ("DOUBLE SILVER WEEKEND") with a countdown. Below a "MORE EVENTS" divider sits a **row of 4 event cards**, each with art, name, description, a small **REWARDS** preview row (3 icons), a **PLAY** CTA, and its own end-timer. A **bottom tab bar** switches between **Events** (active), **Calendar**, and **Past Events**. Reached from the bottom-hub "Events" entry; **Back** returns to hub; **PLAY** routes into the event's mode/flow.

## B · VISUAL DNA
Inherits global DNA (00 §6). Screen-specific:
- **Backdrop:** dark obsidian field, vignette, faint warm top glow; the featured banner brings the only large color/art block.
- **Featured banner:** a wide cinematic panel — left a rain-lit battlefield/wagon scene, center huge **two-line gold-bevel serif** title with a blue corner **FEATURED** ribbon, a small **"Ends in 1d 12h"** clock pill, and on the right a **glowing silver "2×" emblem** (silver lion coin in a cobalt energy ring with bloom). Ornate gold frame around the whole banner.
- **Divider:** "MORE EVENTS" centered with flanking gold rule lines.
- **Event cards:** vertical dark slate panels, bronze edge, rounded; top = event art with a soft gradient; some carry a small **NEW** ribbon (Endless Rush); name (gold serif), 2-line description, a **REWARDS** label + 3 reward icons (trophy/shard/chest tiers), a **blue PLAY** CTA, and a bottom **"Ends in …"** clock line. Cards are structurally identical, varied by art/copy/timer.
- **Tab bar:** dark bottom strip; active tab (**Events**) gold-lit with an underline/emblem; inactive tabs (Calendar, Past Events) muted with their glyphs.
- **Mood:** "what's hot right now" — cinematic featured hero + tidy card shelf; PLAY buttons + the 2× emblem are the brightest focal objects.

## C · SCREEN DECOMPOSITION (ASCII node tree — every node)
```
EventsHubScreen (UiScreen, CanvasGroup)
├─ FullBleedBackdrop (dark art, under cutout)
│  ├─ Vignette
│  └─ TopGlow
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ TopBar
   │  ├─ BackButton (gold ← tile, top-left)
   │  ├─ TitleBlock {Title "EVENTS", Subtitle "Limited-time events with epic rewards!"}
   │  └─ CurrencyChips (top-right, HLG)
   │     ├─ Chip_Gold   {icon, "128,450", +}
   │     ├─ Chip_Gems   {icon, "2,850",   +}
   │     └─ EnergyChip  {icon, "58/120",  +}   ⚠️CANON-CUT
   ├─ FeaturedBanner (ornate framed wide panel)
   │  ├─ BannerArt (battlefield scene, left/full)
   │  ├─ FeaturedRibbon "FEATURED" (blue corner ribbon, top-left)
   │  ├─ BannerTitle "DOUBLE\nSILVER WEEKEND" (2-line gold serif)
   │  ├─ BannerSubtitle "Earn DOUBLE the Silver in all battles!"
   │  ├─ BannerTimerPill {clock, "Ends in 1d 12h"}
   │  └─ MultiplierEmblem "2×" (silver lion coin + cobalt energy ring + glow)
   ├─ MoreEventsDivider {ruleLeft, "MORE EVENTS", ruleRight}
   ├─ EventCardRow (HorizontalLayoutGroup, 4 cards)  [ScrollRect if >4]
   │  ├─ Card_EndlessRush
   │  │  ├─ Art + NewRibbon "NEW"
   │  │  ├─ Name "Endless Rush"
   │  │  ├─ Desc "Survive endless waves of\nenemies and climb the leaderboard!"
   │  │  ├─ RewardsRow {label "REWARDS", icon trophy, icon shard, icon chest}
   │  │  ├─ PlayButton "PLAY"
   │  │  └─ Timer {clock, "Ends in 1d 12h"}
   │  ├─ Card_HeroTrials
   │  │  ├─ Art
   │  │  ├─ Name "Hero Trials"
   │  │  ├─ Desc "Win with fixed heroes and\nearn exclusive hero shards!"
   │  │  ├─ RewardsRow {"REWARDS", icon(2 badge), shard, chest}
   │  │  ├─ PlayButton "PLAY"
   │  │  └─ Timer {clock, "Ends in 2d 12h"}
   │  ├─ Card_ResourceRun
   │  │  ├─ Art
   │  │  ├─ Name "Resource Run"
   │  │  ├─ Desc "Gather as many resources as\nyou can before time runs out!"
   │  │  ├─ RewardsRow {"REWARDS", icon coin, gem, chest}
   │  │  ├─ PlayButton "PLAY"
   │  │  └─ Timer {clock, "Ends in 12h 45m"}
   │  └─ Card_ArenaClash  ⚠️"real-time" → relabel async
   │     ├─ Art
   │     ├─ Name "Arena Clash"
   │     ├─ Desc "Compete against other players\nin real-time battles for ranked rewards!"  ⚠️
   │     ├─ RewardsRow {"REWARDS", icon rank-gem, shard, chest}
   │     ├─ PlayButton "PLAY"
   │     └─ Timer {clock, "Ends in 3d 12h"}
   └─ TabBar (bottom, 3 tabs)
      ├─ Tab_Events    (ACTIVE — book/scroll glyph, gold-lit, underline)
      ├─ Tab_Calendar  (calendar glyph, muted)
      └─ Tab_PastEvents (clock-history glyph, muted)
```

## D · UNITY HIERARCHY SPEC (per node)
| Node | Parent | Child order | Object type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| EventsHubScreen | Canvas | 0 | Rect+CanvasGroup | stretch-all | .5,.5 | — | root | fade |
| FullBleedBackdrop | screen | 0 | RawImage/Image | stretch-all | .5,.5 | — | under cutout | cover-fill |
| SafeAreaRoot | screen | 1 | Rect+SafeAreaFitter | stretch-all | .5,.5 | — | insets | drives content |
| TopBar | SafeAreaRoot | 0 | Rect | top-stretch | .5,1 | — | inside safe | fixed frac height |
| BackButton | TopBar | 0 | Button+Image | top-left | 0,1 | — | — | fixed |
| TitleBlock | TopBar | 1 | VerticalLayoutGroup | top-center/left | .5,1 | center | — | — |
| CurrencyChips | TopBar | 2 | HorizontalLayoutGroup | top-right | 1,1 | mid-right | — | right-anchored |
| FeaturedBanner | SafeAreaRoot | 1 | Image (9-slice frame) + children | top-stretch (below TopBar) | .5,1 | — | inside safe | width ~full, fixed aspect |
| BannerArt | FeaturedBanner | 0 | Image (masked to frame) | stretch-all | .5,.5 | — | — | cover-fill in frame |
| FeaturedRibbon | FeaturedBanner | 1 | Image+Text | top-left | 0,1 | — | — | corner-pinned |
| BannerTitle | FeaturedBanner | 2 | Text (TMP) | center-left | 0,.5 | left | — | autosize |
| BannerSubtitle | FeaturedBanner | 3 | Text | center-left (below title) | 0,.5 | left | — | — |
| BannerTimerPill | FeaturedBanner | 4 | Image+HLG (clock+text) | center-low | .5,.5 | center | — | — |
| MultiplierEmblem | FeaturedBanner | 5 | Image (glow) + Text "2×" | mid-right | 1,.5 | center | — | right-pinned |
| MoreEventsDivider | SafeAreaRoot | 2 | HLG (rule+text+rule) | top-stretch (below banner) | .5,1 | center | — | spans width |
| EventCardRow | SafeAreaRoot | 3 | **HorizontalLayoutGroup** (4 equal) | stretch (mid-low band) | .5,.5 | mid-center | inside safe | cards equal; **ScrollRect if >4** |
| Card_x | EventCardRow | 0..3 | Image + VerticalLayoutGroup (LayoutElement) | — | .5,.5 | top-center | — | equal width; internal frac rows |
| ↳ Art (+NewRibbon) | Card | 0 | Image (+corner ribbon) | top-stretch | .5,1 | — | — | top band of card |
| ↳ Name | Card | 1 | Text | — | .5,.5 | center | — | — |
| ↳ Desc | Card | 2 | Text (2-line) | — | .5,.5 | center | — | wrap |
| ↳ RewardsRow | Card | 3 | HLG (label + 3 icons) | — | .5,.5 | center | — | — |
| ↳ PlayButton | Card | 4 | Button+Image | — | .5,.5 | center | — | full card width inset |
| ↳ Timer | Card | 5 | HLG (clock+text) | — | .5,0 | center | — | — |
| TabBar | SafeAreaRoot | 4 | Image + HorizontalLayoutGroup (3 equal) | bottom-stretch | .5,0 | mid-center | inside safe | spans width |
| Tab_x | TabBar | 0..2 | Toggle/Button + Image + Text | — | .5,.5 | center | — | equal thirds |

**List note:** EventCardRow is the canonical card list → `HorizontalLayoutGroup` with equal `LayoutElement` widths; all 4 fit at 2340×1080. **Wrap in a horizontal `ScrollRect`** if events exceed 4 (banner/divider/tabs stay pinned outside the viewport). TabBar = a toggle group (single-select). CurrencyChips + RewardsRow = small HLGs.

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar:** height ≈ 0.11·H ≈ 119 px; top inset ≈ 0.02·H. Back tile ≈ 0.075·H square, left inset 0.025·W. Title cap ≈ 0.055·H centered; subtitle below ≈ 0.026·H. Chips: each ≈ 0.115·W × 0.05·H, gap 0.008·W, right inset 0.02·W.
- **FeaturedBanner:** width ≈ **0.95·W ≈ 2223 px**, height ≈ **0.30·H ≈ 324 px**; top y ≈ below TopBar (≈0.13·H). Gold frame ≈ 0.012·H thick.
  - FeaturedRibbon: top-left corner, length ≈ 0.10·W, angled 0° banded (or 45° corner) — here a horizontal tab ≈ 0.10·W × 0.045·H.
  - BannerTitle: left-center, 2 lines, cap ≈ 0.075·H each, left inset ≈ 0.06·bannerW.
  - BannerSubtitle: below title, ≈ 0.03·H.
  - BannerTimerPill: low-center area, ≈ 0.14·W × 0.045·H.
  - MultiplierEmblem: right ~0.20·bannerW zone, "2×" cap ≈ 0.16·H, coin/ring ⌀ ≈ 0.22·H, right inset ≈ 0.05·bannerW; bloom halo extends ~+30%.
- **MoreEventsDivider:** y ≈ 0.46·H; rule lines flank centered text; text cap ≈ 0.026·H.
- **EventCardRow:** band y ≈ 0.49·H → 0.90·H (height ≈ 0.41·H ≈ 443 px), width ≈ 0.95·W.
  - 4 cards + 3 gaps; gap ≈ 0.015·W ≈ 35 px → gaps ≈ 105 px → cards share ≈ 2118 px → **card width ≈ 530 px (~0.226·W)**. Card height ≈ full band ≈ 443 px → card aspect ≈ 0.55:1 (portrait).
  - Card internal (fractions of card H): Art 0.00–0.34; Name 0.36–0.44; Desc 0.45–0.60 (2 lines); RewardsRow 0.62–0.74 (label + 3 icons ⌀ ≈ 0.07·cardW each); PlayButton 0.76–0.90 (width ≈ 0.84·cardW, height ≈ 0.12·cardH); Timer 0.92–1.00.
  - NewRibbon (Endless Rush): top-left of Art, ≈ 0.30·cardW × 0.07·cardH.
- **TabBar:** height ≈ 0.085·H ≈ 92 px, bottom inset ≈ 0.01·H; 3 equal tabs; active tab gold underline ≈ 0.4·tabW × 4 px, glyph ⌀ ≈ 0.05·H above label.

**Tablet (4:3):** match-height keeps banner/card heights; cards gain side margin (cap row width 0.9·W). **Ultrawide (21:9):** banner caps max width (~0.8·W centered); cards may show a 5th if present (ScrollRect) or center with margins; backdrop fills sides. **Notch:** SafeAreaRoot insets all content; full-bleed backdrop under cutout; Back/chips/tabs never cross inset.

## F · TYPOGRAPHY (per text)
| Text | Personality | Weight | Caps | Kerning | Line | FX | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "EVENTS" | serif Trajan display | Black | UPPER | +6% | 1.0 | gold bevel + bloom + 2px stroke | ~64 | #f0d27a→#caa04a; stroke #3a2c0e |
| Subtitle | clean sans/light serif | Reg | Sentence | 0 | 1.1 | shadow | ~24 | #d9c79a |
| Currency numbers | numeric | SemiBold | — | 0 | 1.0 | shadow | ~26 | #ffffff |
| "FEATURED" ribbon | condensed caps | Bold | UPPER | +6% | 1.0 | on cobalt, slight bevel | ~22 | #eaf2ff on #2b56c8 |
| BannerTitle "DOUBLE / SILVER WEEKEND" | serif display, epic | Black | UPPER | +4% | 0.95 | strong gold/silver bevel + bloom + stroke | line1 ~58, line2 ~64 | #e8e2cf→#cfd3dc (silvery gold); stroke #2a2c33 |
| BannerSubtitle | light serif/clean | Reg | Sentence | 0 | 1.1 | shadow | ~26 | #d9c79a |
| BannerTimerPill "Ends in 1d 12h" | numeric/caps | SemiBold | Sentence | +1% | 1.0 | on dark pill, soft glow | ~22 | #ffe08a |
| "2×" emblem | serif display, huge | Black | — | 0 | 1.0 | gold bevel + cobalt glow + bloom | ~120 | #f4e6b0 |
| "MORE EVENTS" | condensed caps | SemiBold | UPPER | +6% | 1.0 | flank rules | ~24 | #cdbf99 |
| "NEW" ribbon | condensed caps | Bold | UPPER | +4% | 1.0 | on blue/gold tag | ~18 | #ffffff |
| Card Name | serif small-caps, sturdy | Bold | Title | +2% | 1.0 | shadow | ~30 | #f2e8cf |
| Card Desc (2 lines) | clean sans, quiet | Reg | Sentence | 0 | 1.15 | — | ~18 | #a9a28c |
| "REWARDS" | condensed caps small | SemiBold | UPPER | +5% | 1.0 | — | ~16 | #b3ac96 |
| "PLAY" | serif display, action | Heavy | UPPER | +6% | 1.0 | top highlight + glow | ~30 | #ffffff on blue |
| Card Timer "Ends in …" | numeric small | Reg | Sentence | 0 | 1.0 | clock glyph left | ~18 | #cdbf99 |
| Tab labels (Events/Calendar/Past Events) | condensed caps | SemiBold (active Bold) | UPPER | +4% | 1.0 | active gold + underline; inactive muted | ~22 | active #f0d27a, inactive #7d7768 |

## G · MATERIALS
- **Backdrop:** obsidian #0a0b0f→#14161e, matte, vignette; warm top glow #2a2416 additive.
- **Banner frame:** brushed gold/bronze #6b5320→#caa04a→#f0d27a→#fff2c2, beveled, worn edges, rim-light; **bloom** on highlights.
- **Banner art:** full-color rainy battlefield/wagon scene, darkened toward edges + inner vignette so title reads; cool blue ambient.
- **FeaturedRibbon:** cobalt #1f356f→#2b56c8 cloth/metal tab, stitched/gold trim.
- **Multiplier emblem:** silver lion coin #b8bcc6→#e8ebf0 high specular, set in a **cobalt energy ring** #2b56c8→#4f8bff with additive glow + arc sparks + bloom.
- **TimerPill:** dark translucent plate #11141d α.8, bronze edge, gold text.
- **Event card panel:** #10131c→#181c27 vertical, thin gold-bronze edge, rounded; soft inner top sheen; card art with bottom gradient fade into panel.
- **NewRibbon:** small blue/gold beveled tag.
- **Reward icons:** trophy gold, shards glowing crystal (orange/violet), chests aged-wood+bronze, gems cobalt/violet, coins gold/silver — all with slight bloom.
- **PlayButton:** royal/cobalt #2b56c8→#4f8bff gloss, top highlight, beveled gold/steel trim, outer glow.
- **TabBar:** dark slate #0c0e15→#161a24, bronze top edge; active tab gold glyph + label + thin gold underline + faint glow; inactive desaturated.
- **Bloom:** banner frame/emblem/title, reward icons, PLAY glows, active tab. Matte panels/backdrop do not bloom.

## H · COMPONENTS (states)
**FeaturedBanner:** quasi-button (tap → featured event detail/play). *idle:* art parallax-still, emblem glow pulse, timer ticking. *hover/focus:* frame brighten + slight scale 1.01. *pressed:* scale 0.99. *expired* (timer→0): emblem dims, "ENDED" overlay, tap disabled until refresh.

**EventCard:**
- *active:* full color, PLAY enabled (blue glow), timer ticking; NEW ribbon shown where applicable.
- *hover/focus:* card lift (y −6) + edge brighten + art subtle zoom.
- *pressed:* scale 0.98.
- *ending-soon* (timer < threshold): timer text turns ember #e0742a + subtle pulse.
- *expired:* card dims ~60%, PLAY → disabled grey, "ENDED" stamp; removed/moved to Past tab on refresh.
- *locked* (level/req not met, if any): PLAY → "LOCKED" + lock glyph, tap → requirement tooltip.

**PlayButton:** idle blue gloss + glow + breathe; hover +brightness; pressed 0.96; disabled grey-blue (expired/locked) → tap shake + tooltip.

**TabBar tabs (toggle group):** active = gold glyph+label+underline+glow (Events); inactive = muted, hover brighten; selecting Calendar/Past Events cross-fades the body content (banner+cards swap for a calendar list / past-events list) while header+tabs persist.

**BackButton:** gold tile; hover brighten; pressed 0.92 → hub.

**CurrencyChip + / timers:** + → Store; all timers tick live (banner + per-card); on expiry trigger the expired states above. EnergyChip flagged CUT.

## I · ANIMATION TIMELINE
**OnShow (~0.85 s):**
- 0.00 backdrop fade + top glow (0.20 s).
- 0.05 TopBar slide-down (0.20 s); chips count-up optional.
- 0.12 FeaturedBanner: scale 0.97→1.00 + α (0.30 s, ease-out); BannerArt subtle ken-burns drift starts; FeaturedRibbon snap-in.
- 0.30 BannerTitle α + slight 1.03→1.0 (0.20 s); MultiplierEmblem pop (0.9→1.0) + glow ring spin-up + bloom (0.30 s).
- 0.40 TimerPill fade-in; timer ticking.
- 0.46 MoreEventsDivider rule lines wipe outward from center (0.20 s) + text fade.
- 0.52 EventCardRow cards stagger L→R: each y +18→0 + α, 0.05 s apart, 0.18 s each (ease-out); art subtle zoom-settle.
- 0.78 PLAY buttons glow-on + breathe; NEW ribbons pop.
- 0.80 TabBar fade-up; active tab underline draw-in (0.18 s).

**Idle loops:** emblem ring slow rotation + arc-spark twinkle + glow pulse (2 s); banner art ken-burns (slow, looping); PLAY breathe (1.6 s); ending-soon timer pulse; active-tab faint glow breathe; dust motes.

**OnPlay (see K):** PLAY press 0.96 → card flash white→blue → screen transition (push) into the event mode (handoff to mode/match flow).

**OnTabSwitch:** old tab underline retract + body cross-fade out (0.15 s) → new body cross-fade in + stagger (0.20 s) + new underline draw-in; header/tabs persist.

**OnClose:** banner+cards+tabs fade/slide out (~0.30 s); pop screen.

## J · PARTICLE & FX
- Banner: rain/ambient in art (baked), **emblem energy ring** sparks + glow + bloom, occasional gold glint sweep across frame.
- Card art: subtle floating embers/dust per theme (baked or light particles); NEW ribbon shimmer.
- Reward icons: small twinkles (slow).
- PLAY glows (blue breathe); active-tab glow.
- Divider wipe spark; dust motes in dark field; vignette.
- Keep FX restrained on cards (legibility); concentrate spectacle on the featured banner.

## K · EVENT BEHAVIOR
- **OnShow:** read server-auth event config (read-only): featured event (title/art/multiplier/timer), active event list (name/desc/rewards/timer/flags: NEW/locked), tab availability. Render; play entry. (Client never authors events; it renders the live-ops payload.)
- **OnPlay (card or banner):** validate availability/requirements → push into the event's mode/flow (mode-select/match). For ranked/competitive events the result routes to the relevant async result screen.
- **OnTabSwitch:** Events ↔ Calendar ↔ Past Events — swap body content (cards / calendar schedule / past-events log) without leaving the screen; persist header+tabs.
- **Timer ticks:** live countdowns; on expiry → expired card/banner state; refresh may move expired events to Past Events.
- **Chip +:** Store deep-link. **Back:** fade out → hub.
- **⚠️ Arena Clash relabel (note, do not alter art):** at implementation the "real-time" copy must become **async ghost** wording to match canon (no real-time PvP); behavior routes to the async-battle pipeline (32). EnergyChip omitted/CUT.

## L · NEGATIVE RULES
- Do **not** render the **EnergyChip (58/120)** as a live system — **canon-CUT**; omit/replace at implementation (kept here only as forensic record).
- Do **not** ship "Arena Clash" as **real-time PvP** — BULWARK is **async ghost**; relabel + route async (the art text is recorded verbatim but flagged).
- Do **not** author events client-side or fabricate timers/rewards — render the server-auth live-ops payload only.
- Do **not** invent extra cards/rewards — exactly the featured banner + 4 cards with the listed names/descriptions/timers shown.
- Do **not** let expired events remain playable; gate via server state.
- Do **not** make Calendar/Past Events forget the header/tab chrome (they're body swaps, not new screens).
- No portrait; concentrate spectacle on the banner, keep cards legible.

## M · ACCEPTANCE CRITERIA (≥95%)
1. Header: Back (left), title "EVENTS" gold-bevel serif + subtitle, 3 chips (128,450 / 2,850 / 58/120) right.
2. Featured banner: ornate frame, blue "FEATURED" ribbon, 2-line "DOUBLE / SILVER WEEKEND", subtitle "Earn DOUBLE the Silver in all battles!", "Ends in 1d 12h" pill, glowing silver **2×** emblem right.
3. "MORE EVENTS" divider, then exactly 4 cards in order (Endless Rush[NEW] / Hero Trials / Resource Run / Arena Clash) with correct names, 2-line descriptions, REWARDS row (3 icons), blue PLAY, and per-card "Ends in …" timer.
4. Bottom tab bar: Events(active, gold/underline) / Calendar / Past Events with glyphs.
5. Layout within ±2% of fraction math at 2340×1080; cards equal width (HLG, ScrollRect-ready); safe-area + full-bleed-under-cutout honored.
6. States correct: card active/hover/ending-soon/expired/locked; PLAY disabled; tab switching body cross-fade; expired banner.
7. Palette within ranges; banner/emblem/PLAY/active-tab bloom present; matte panels not blooming.
8. Energy chip flagged/omitted; Arena-Clash async relabel flagged.

## N · IMPLEMENTATION CONFIDENCE
**90/100.** High: standard hub sub-page (header + hero banner + card shelf + tabs), finite known content, exact labels/timers. Deductions: (−4) the featured-banner art + glowing 2× emblem ring need bespoke assets/shader for 1:1 (bloom/spark/ken-burns); (−3) Calendar/Past-Events tab bodies are not shown in this frame — their layouts are inferred (will need their own sub-specs); (−2) reward-icon sets per card are asset-dependent; (−1) Arena-Clash relabel is a content/canon edit governed outside this forensic record.

## O · SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, in order, substantive.
- [x] Fraction math normalized to 2340×1080; tablet/ultrawide/notch covered.
- [x] Every visible label/number/colour recorded (banner, 4 cards, timers, tabs, chips); nothing invented.
- [x] Card list typed as HLG (ScrollRect-ready); tab bar typed as toggle group; chip/reward rows as HLG.
- [x] Component states (banner, cards active/hover/ending/expired/locked, PLAY, tabs, Back) enumerated.
- [x] Animation timeline with timestamps/easing incl. tab-switch; particle/FX listed.
- [x] Canon flags raised (EnergyChip CUT; Arena-Clash real-time→async) without altering forensic spec.
- [x] Server-auth live-ops render rules stated; no client-authored events.
- [x] Landscape, safe-area, full-bleed-under-cutout applied; documentation only, no code/asset changes.



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

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



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 35 · Clan
Source: design/ClanScreenDesign.png · 1829×860 (≈2.13:1) · Analysis-only forensic spec.

> Normalize to 2340×1080 (≈19.5:9). CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). FRACTION-BASED layout; px quoted at 1080-tall height. Full-bleed BG under cutout; interactive content inside `Screen.safeArea`.

---

## A. SCREEN PURPOSE
The **real Clan hub** (resolves the prior "Clan == Leaderboard duplicate" defect). A three-column social home for the player's clan **DRAGONFORGE**: (LEFT) clan identity — crest/banner, name, motto, vital stats; (CENTER) tabbed content **MEMBERS / Activity / Clan War / Clan Chest** with the member roster (rank, avatar, name, level, role, trophies, online status) and a bottom action bar **CLAN WAR · DONATE · CLAN SHOP · MANAGE · LEAVE CLAN**; (RIGHT) a live **CHAT / Announcements** panel with a message composer. Server-authoritative social data; the client never edits roles/trophies. Reached from the Main-Menu rail. No gameplay/ECS interaction.

## B. VISUAL DNA (screen-specific, inherits GLOBAL DNA)
- Dark heroic high-fantasy; near-black charcoal field (#0a0b0f→#14161e) over a faint dusk **castle wall** backdrop, vignetted.
- **Brushed gold/antique bronze ornate frames** divide the three columns; thin gold hairline dividers between sections.
- **Faction banner identity:** the left column is dominated by a tall **royal/cobalt-blue heraldic banner** with a **gold dragon crest** and stitched gold trim (Iron-Pact-leaning palette for this clan).
- **Royal blue** = selected tab, online accents and the chat send affordance area; **ember/oxblood red** = the **LEAVE CLAN** danger button only. **Gold** = roles emphasis (Leader), trophy values, headers. **Green** (#3fd07a) = "Online" status dot.
- Role pills are tinted chips: **Leader** (gold), **Officer** (blue/steel), **Veteran** (bronze/grey), **Member** (muted grey). The **(You)** member row is highlighted (blue tint + outline).
- Low-key lighting; focal glow on the dragon crest; gold rim-light on frames; subtle banner cloth shading.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
ClanScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ BackgroundLayer (full-bleed)
   │  ├─ BG_CastleWall (Image)
   │  └─ BG_Vignette (Image)
   ├─ TopBar
   │  ├─ BackButton (gold square + left chevron)
   │  ├─ Title_ClanName ("DRAGONFORGE", serif gold)
   │  └─ TopRightCluster
   │     ├─ MemberCountChip (Icon_People + "48/50")
   │     ├─ ClanGoldChip (Icon_Coin + "125,680")
   │     ├─ LanguagePill ("English")
   │     └─ SettingsGear (Button)
   ├─ ContentRow (3 columns)
   │  ├─ LeftColumn_Identity (panel)
   │  │  ├─ ClanBanner (tall blue banner Image)
   │  │  │  ├─ DragonCrest (gold dragon emblem)
   │  │  │  └─ Banner_ClanName ("DRAGONFORGE")
   │  │  ├─ Lbl_Motto ("United in Fire, Forged in Glory.")
   │  │  └─ StatsList
   │  │     ├─ Stat_ClanLevel ("Clan Level" / "15")  [+ progress bar fragment ~9,xxx/?]
   │  │     ├─ Stat_ClanType ("Clan Type" / "Open")
   │  │     ├─ Stat_RequiredLevel ("Required Level" / "30")
   │  │     ├─ Stat_ClanRegion ("Clan Region" / "North America")
   │  │     └─ Stat_Created ("Created" / "2024-02-14")
   │  ├─ CenterColumn_Roster (panel)
   │  │  ├─ TabRow
   │  │  │  ├─ Tab_MEMBERS (selected)
   │  │  │  ├─ Tab_Activity
   │  │  │  ├─ Tab_ClanWar
   │  │  │  └─ Tab_ClanChest
   │  │  ├─ RosterHeaderRow
   │  │  │  ├─ Lbl_MembersCount ("MEMBERS 48/50")
   │  │  │  ├─ Col_ROLE
   │  │  │  └─ Col_TROPHIES
   │  │  ├─ ScrollView (vertical)
   │  │  │  └─ Content (VerticalLayoutGroup)
   │  │  │     ├─ MemberRow_1 (RankBadge1 · Thalion · L60 · Leader(gold) · 24,560 · Online)
   │  │  │     ├─ MemberRow_2 (RankBadge2 · Valyra · L58 · Officer · 21,340 · Online)
   │  │  │     ├─ MemberRow_3 (RankBadge3 · Ragnor · L57 · Officer · 19,870 · In Battle)
   │  │  │     ├─ MemberRow_4 (4 · Eldric · L55 · Veteran · 18,450 · Online)
   │  │  │     ├─ MemberRow_5 (5 · Seraphine · L54 · Veteran · 16,720 · 1h ago)
   │  │  │     ├─ MemberRow_You (12 · Aric Stormblade (You) · L52 · Member · 12,680)  [highlighted]
   │  │  │     └─ MemberRow_7 (13 · Mortis · L51 · Member · 11,240 · 2h ago)
   │  │  └─ ActionBar
   │  │     ├─ Btn_ClanWar (icon + "CLAN WAR")
   │  │     ├─ Btn_Donate (icon + "DONATE")
   │  │     ├─ Btn_ClanShop (icon + "CLAN SHOP")
   │  │     ├─ Btn_Manage (icon + "MANAGE")
   │  │     └─ Btn_LeaveClan (red, "LEAVE CLAN")
   │  └─ RightColumn_Chat (panel)
   │     ├─ ChatTabRow
   │     │  ├─ Tab_Chat (selected)
   │     │  └─ Tab_Announcements
   │     ├─ ChatScrollView (vertical)
   │     │  └─ Content (VerticalLayoutGroup)
   │     │     ├─ ChatMsg (Thalion · "Welcome to Dragonforge! Let's conquer together!" · ts)
   │     │     ├─ ChatMsg (Valyra · "Great war tonight! 🔥" · ts)
   │     │     ├─ ChatMsg (Eldric · "Thanks everyone! GG all around." · ts)
   │     │     ├─ ChatMsg (Ragnor · "Let's keep the momentum going!" · ts)
   │     │     └─ ChatMsg (Seraphine · "I donated to the Clan Chest. Let's unlock those rewards!" · ts)
   │     └─ ChatComposer
   │        ├─ Input_Message ("Tap to message...")
   │        ├─ Btn_Emoji (😊)
   │        └─ Btn_Send ("SEND")
```

## D. UNITY HIERARCHY SPEC (per node)
- **ClanScreen** — parent UiRouter canvas. Empty `RectTransform` + `CanvasGroup`. Stretch-all. Full screen (NOT a modal).
- **SafeAreaRoot** — `SafeAreaFitter`, stretch-all. Children: Background, TopBar, ContentRow.
- **BackgroundLayer** (BG_CastleWall, BG_Vignette) — parent ClanScreen (outside safe area → full-bleed). Stretch-all, `Image`, raycast off. Vignette later sibling.
- **TopBar** — parent SafeAreaRoot. Anchor top-stretch (0,1)-(1,1) pivot 0.5,1, fixed height (E). BackButton top-left; Title left-of-center after the back button; TopRightCluster top-right `HorizontalLayoutGroup` (chips + gear).
- **BackButton** — `Button`, anchor (0,1) pivot 0,1, ≥88×88.
- **Title_ClanName** — `Text` serif gold, anchor left (after back btn), pivot 0,1, left-aligned.
- **TopRightCluster** — `HorizontalLayoutGroup` (spacing ~16), anchor (1,1) pivot 1,1. Children: MemberCountChip, ClanGoldChip, LanguagePill, SettingsGear. Each chip = `Image` capsule + icon + `Text`.
- **ContentRow** — parent SafeAreaRoot. Anchor stretch-all below TopBar (min (0, 0) max (1, topBarBottomFrac)) pivot 0.5. `HorizontalLayoutGroup` is possible but FIXED fractional widths via three anchored panels is preferred (the columns differ in width). Child order: LeftColumn, CenterColumn, RightColumn.
- **LeftColumn_Identity** — anchor left band (E). `Image` panel (dark inset + gold frame). `VerticalLayoutGroup` top-aligned: Banner (large), Motto, StatsList.
  - **ClanBanner** — `Image` tall blue banner (9-slice or bespoke), anchor top-center, pivot 0.5,1, `preserveAspect`. **DragonCrest** child `Image` centered on banner upper-third; **Banner_ClanName** `Text` overlaid lower on the banner.
  - **Lbl_Motto** — `Text` italic-leaning cream, center, below banner.
  - **StatsList** — `VerticalLayoutGroup` of label/value rows; each row = `HorizontalLayoutGroup` (caption left grey, value right gold). ClanLevel row may include a small `Image` fill bar (progress) under it.
- **CenterColumn_Roster** — anchor center band (E), widest column. `Image` panel. Child order: TabRow (top), RosterHeaderRow, ScrollView (fills), ActionBar (bottom).
  - **TabRow** — `HorizontalLayoutGroup` / `ToggleGroup`, top-stretch, fixed height. MEMBERS selected.
  - **RosterHeaderRow** — top-stretch under tabs, fixed ~42 px. Left `Text` "MEMBERS 48/50"; right two column labels ROLE, TROPHIES aligned to the row's role/trophy x-fractions.
  - **ScrollView** — `ScrollRect` vertical, `RectMask2D` viewport, Content `VerticalLayoutGroup` (spacing ~8) + `ContentSizeFitter`.
  - **MemberRow_N** — `RectTransform` + `Button` (tap → member context, optional). Internal `HorizontalLayoutGroup`-style positioned by x-fractions: RankBadge | Avatar | Name+LevelChip | RolePill | TrophyValue | StatusTag. Fixed height (E). The **(You)** row uses a blue-tinted bg `Image` + 2px blue `Outline`.
  - **RankBadge** — ranks 1-3 medallion `Image` (gold/silver/bronze) + numeral; 4+ plain numeral `Text`.
  - **LevelChip** — small dark capsule `Image` + level `Text` (e.g. "60").
  - **RolePill** — capsule `Image` tinted by role + role `Text`.
  - **StatusTag** — green dot `Image` + `Text` ("Online") OR amber dot + "In Battle" OR grey + "1h ago"/"2h ago"; the (You) row has no status tag in source (omit).
  - **ActionBar** — bottom-stretch, fixed height. `HorizontalLayoutGroup`: four icon+label buttons (ClanWar, Donate, ClanShop, Manage) sharing equal width, then **Btn_LeaveClan** (red) wider/right-weighted.
- **RightColumn_Chat** — anchor right band (E). `Image` panel. Child order: ChatTabRow (top), ChatScrollView (fills), ChatComposer (bottom, fixed height).
  - **ChatTabRow** — two tabs (Chat selected, Announcements). `ToggleGroup`.
  - **ChatScrollView** — `ScrollRect` vertical; Content `VerticalLayoutGroup` (spacing ~12) + `ContentSizeFitter`; auto-scroll to bottom on new message.
  - **ChatMsg** — `VerticalLayoutGroup` bubble: header row (small Avatar `Image` + sender `Text` gold + timestamp `Text` grey right) over body `Text` (wrapping cream).
  - **ChatComposer** — bottom-stretch `HorizontalLayoutGroup`: Input_Message (`InputField`/TMP_InputField, flexible width, placeholder "Tap to message..."), Btn_Emoji (square), Btn_Send (blue/gold capsule "SEND").
- **Responsive:** match-height keeps row/message heights stable. On ultrawide, the **center roster** flexes wider (extra width goes to PLAYER/name area); left and right columns keep min widths. On narrower-than-ref (rare in landscape), right chat column can collapse to a toggle drawer (note as fallback). Notch handled by SafeAreaRoot; BG full-bleed.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar height:** ≈ 0.095 (~103 px). BackButton ≈ 88×88, left-inset ~1.4%. Title baseline centered to bar. TopRightCluster right-inset ~1.4%; chips ~190–230 px wide each, gear ~72×72.
- **ContentRow:** y from ~0.10 to ~0.99 of canvas (below TopBar to bottom inset ~1%).
- **Three-column widths (fractions of canvas width, with ~1.2% gutters):**
  - LeftColumn_Identity: ≈ 0.205 (~480 px), x≈[0.012, 0.217]
  - CenterColumn_Roster: ≈ 0.475 (~1112 px), x≈[0.229, 0.704]
  - RightColumn_Chat: ≈ 0.272 (~636 px), x≈[0.716, 0.988]
- **LeftColumn internal:** Banner occupies top ~0.42 of the column height (tall, ~aspect 0.55:1); crest centered at ~0.30 of banner height; motto one line below banner; StatsList = 5 rows, each ~0.07 of column height, caption left / value right, with a thin divider between rows.
- **CenterColumn internal:** TabRow band height ≈ 64 px (4 tabs equal width across the column, ~24% each). RosterHeaderRow ≈ 42 px. ActionBar height ≈ 92 px. ScrollView fills the remainder.
  - **Roster row x-fractions (relative to center column width):** RANK center ≈ 0.045 (w 0.09) · Avatar center ≈ 0.135 (Ø ~52 px) · Name block starts ≈ 0.18 with LevelChip immediately right of the name · RolePill center ≈ 0.62 (w ~0.16) · TrophyValue (icon+number) center ≈ 0.83 right-aligned · StatusTag center ≈ 0.95 right-aligned.
  - **Roster row height:** ≈ 80 px; gap ~8 px; seven seed rows; scroll for more.
  - **ActionBar:** five buttons; ClanWar/Donate/ClanShop/Manage each ≈ 0.155 of column width (icon over/left of label), LeaveClan ≈ 0.22 width, gap ~12 px; LeaveClan right-aligned and red.
- **RightColumn internal:** ChatTabRow height ≈ 56 px (two tabs ~50% each). ChatComposer height ≈ 80 px (Input flexible, Emoji ~64×64, Send ~120×64). ChatScrollView fills the rest. Message bubble: avatar Ø ~36 px; sender+timestamp header ~28 px; body wraps; bubble vertical padding ~10 px.
- **Tablet 4:3:** columns keep fractions; extra height grows the scroll areas. **Ultrawide 21:9:** clamp overall content to max-width ~0.97·2340 and grow CenterColumn. **Notch:** inside SafeAreaRoot; BG bleeds.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF (Cinzel/Trajan-like) for clan name/headers; Roboto-Medium SDF for body/chat/numbers. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title clan name "DRAGONFORGE" (topbar) | prestige serif | Heavy | UPPER | +6% | 1.0 | gold bevel + bloom + dark stroke | ~52 | #f0d27a / stroke #3a2c0e |
| Banner clan name (on banner) | heraldic serif | Bold | UPPER | +4% | 1.0 | gold + soft shadow on cloth | ~40 | #f4dd8c |
| Motto "United in Fire, Forged in Glory." | flavor | Regular/italic | Title | +2% | 1.1 | none | ~24 | #cdbf9e |
| Stat captions ("Clan Level" etc.) | label | Medium | Title | +1% | 1.0 | none | ~22 | #9a8a6a |
| Stat values ("15","Open","North America"...) | data | Bold | Title | 0 | 1.0 | faint shadow | ~24 | #e9dcc0 |
| Member/Gold chips ("48/50","125,680") | data | Bold | — | +1% | 1.0 | shadow | ~26 | #f0d27a |
| Language pill ("English") | utility | Medium | Title | +2% | 1.0 | none | ~24 | #d8cdb2 |
| Tab labels (MEMBERS/Activity/Clan War/Clan Chest) | sans caps | Bold | UPPER(MEMBERS)/Title | +3% | 1.0 | sel white glow / unsel gold-grey | ~28 | sel #ffffff / unsel #9a8a6a |
| Roster header "MEMBERS 48/50" | heading | Bold | UPPER | +4% | 1.0 | gold | ~26 | #f0d27a |
| Column labels (ROLE/TROPHIES) | small caps | Medium | UPPER | +6% | 1.0 | none | ~22 | #9a8a6a |
| Member name ("Thalion"…) | name | Bold | Title | +1% | 1.0 | shadow | ~28 | #f3ead2 |
| "(You)" suffix on Aric | emphasis | Bold | Title | 0 | 1.0 | blue tint | ~24 | #9fc0ff |
| Level chip number ("60") | data | Bold | — | 0 | 1.0 | shadow | ~22 | #e9dcc0 |
| Role pill text (Leader/Officer/Veteran/Member) | label | Bold | Title | +2% | 1.0 | role-tinted, faint glow | ~22 | Leader #f0d27a / Officer #8fb3ff / Vet #cdb88a / Member #b8b0a0 |
| Trophy value ("24,560") | data | Bold | — | +1% tabular | 1.0 | shadow | ~26 | #f0d27a |
| Status tag (Online/In Battle/1h ago) | status | Medium | Title | +1% | 1.0 | dot-colored | ~22 | Online #3fd07a / InBattle #f0a23a / ago #8c8270 |
| Action button labels (CLAN WAR/DONATE/CLAN SHOP/MANAGE) | CTA caps | Bold | UPPER | +3% | 1.0 | gold on dark, shadow | ~24 | #f0d27a |
| LEAVE CLAN label | danger CTA | Bold | UPPER | +3% | 1.0 | white on red + shadow | ~24 | #ffffff |
| Chat tab labels (Chat/Announcements) | sans caps | Bold | UPPER | +3% | 1.0 | sel white / unsel grey | ~24 | sel #ffffff / unsel #9a8a6a |
| Chat sender name | name | Bold | Title | +1% | 1.0 | gold | ~24 | #e7cf94 |
| Chat timestamp | meta | Regular | — | 0 | 1.0 | none | ~20 | #7a7060 |
| Chat body text | body | Regular | Sentence | 0 | 1.2 | none | ~24 | #d9d2c2 |
| Composer placeholder "Tap to message..." | placeholder | Regular/italic | Sentence | 0 | 1.0 | none | ~24 | #6f685a |
| SEND label | CTA | Bold | UPPER | +3% | 1.0 | white/gold | ~24 | #ffffff |

## G. MATERIALS
- **Frames/dividers:** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin roughness, worn edges, engraved filigree on column rails; thin gold hairlines between sections.
- **Panel fills:** obsidian #0c0e14→#14161e gradient, low reflectivity, inner shadow at frame edges.
- **Clan banner:** royal-blue cloth (#1d2f7a core, #2b56c8 mid, #0f1b4a shadow) with stitched gold trim and a subtle cloth weave/fold shading; bottom edge swallow-tail cut. **DragonCrest:** polished gold dragon relief (#f6dd86 hi, #b8862c mid, #6b4a14 sh) with a small blue gem and focal bloom.
- **Medallion rank badges:** gold/silver/bronze cast disks (same palette as spec 34).
- **Role pills:** Leader = gold gradient chip (glossy); Officer = steel-blue chip; Veteran = bronze/grey chip; Member = muted dark-grey chip; all with thin metal rim.
- **Avatars:** semi-realistic portrait sprites in beveled gold rings.
- **Status dots:** Online emissive green (#3fd07a, faint glow); In Battle amber (#f0a23a); ago dim grey.
- **Trophy icon:** gold cup, satin.
- **Action buttons:** dark stone/metal capsules with gold icon+label; **LeaveClan** = oxblood/ember red gradient (#7a1f1a→#d8452b), glossy, danger glow.
- **Chat panel:** slightly darker inset; message bubbles nearly transparent dark with a faint left gold accent on the sender row; **Send** button blue or gold gloss; **Input** field a dark recessed pill with inner shadow.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **BackButton / SettingsGear:** idle gold on dark; hover +brightness+glow; pressed scale 0.94; SFX click.
- **TopRightCluster chips / LanguagePill:** mostly display; LanguagePill is a `Button` (opens language picker) — hover faint glow, pressed scale 0.97.
- **Roster tabs (MEMBERS/Activity/Clan War/Clan Chest):** selected = blue fill + white label + raise/glow; idle = ghost capsule + gold-grey label; hover brightens; pressed 0.97; disabled 40%. Switching cross-fades the center content (roster vs activity feed vs war panel vs chest panel).
- **Member rows:** idle subtle stripe; hover faint gold tint + 1px edge; pressed 0.99; the **(You)** row is sustained blue tint + outline; tap (optional) opens member actions (promote/kick if permitted — gated by role, server-validated).
- **Action buttons (ClanWar/Donate/ClanShop/Manage):** idle stone capsule + gold; hover +brightness+glow; pressed scale 0.96 + inset; disabled desaturated @50% (e.g., Manage disabled for non-officers). **LeaveClan (danger):** idle red gradient; hover brighter red + glow; pressed darken+0.96; always triggers a Confirm modal (spec 37) before acting.
- **Chat tabs (Chat/Announcements):** same toggle pattern; Announcements may be read-only (no composer) for non-officers.
- **Chat composer:** Input idle recessed; focused = gold/blue ring + caret; Emoji button opens picker; **Send** idle blue, hover brighter, pressed 0.96, **disabled when input empty** (desaturated). Sending appends a bubble and clears input.
- **ScrollViews:** elastic overscroll; chat auto-scrolls to newest; momentum flick.
- **Feedback:** ≥88px touch; pressed ≤80ms scale; hover only on pointer platforms.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms CanvasGroup α 0→1 over 180 ms. 40 ms TopBar slide −16px→0 over 200 ms. 80 ms LeftColumn slide −24px→0 + α over 220 ms (ease-out); banner crest soft bloom pulse one-shot at 220 ms. 120–420 ms roster rows stagger-in (α + slide +12px), 30 ms apart, 160 ms each. 140 ms RightColumn chat slides +24px→0 + α over 220 ms; latest chat bubbles pop subtly. 260 ms ActionBar fades/raises in (180 ms). 300 ms (You) row blue-glow flash (220 ms).
- **Tab switch (center):** old content α 1→0 + slide −10px (120 ms); new content α 0→1 + slide +10px→0 (160 ms); selected capsule glides (140 ms). Re-stagger rows 15 ms apart.
- **Chat tab switch:** cross-fade lists 140 ms.
- **New chat message arrives:** bubble α 0→1 + slide +10px→0 + scale 0.98→1 over 180 ms; auto-scroll to bottom (160 ms ease-out).
- **Send pressed:** button scale 0.96 (80 ms), input clears, bubble appended with the arrive anim.
- **LeaveClan pressed:** triggers Confirm modal slide-in (per spec 37 I).
- **OnHide:** CanvasGroup α 1→0 over 140 ms; columns slight outward slide.
- **Easing:** ease-out enters, ease-in exits, gentle back on banner/crest pop.

## J. PARTICLE & FX
- **DragonCrest:** steady focal bloom + slow specular sweep + 1–2 sparkle motes (low rate).
- **ClanBanner:** very subtle **cloth sway** (1–2° rotation at the free edge, ~3 s sine loop) + soft shadow shift — keep gentle.
- **Online status dots:** faint pulsing green glow (±10%, 1.8 s).
- **Selected tab / Send button:** subtle pulsing rim glow.
- **(You) row:** breathing blue outline glow (±8%, 2 s).
- **LeaveClan:** faint ember flicker on hover only. Keep all FX low-key — roster/chat legibility first.

## K. EVENT BEHAVIOR
- **OnShow:** fetch clan summary (name, crest, motto, stats, member count, clan gold), the member roster, and recent chat/announcements from the server-auth clan service; bind region/language; subscribe to chat stream.
- **OnTabChanged(MEMBERS/Activity/Clan War/Clan Chest):** load+show that sub-view (Activity feed, Clan War status/bracket, Clan Chest progress/contribution); MEMBERS is default.
- **OnChatTabChanged(Chat/Announcements):** swap stream; hide composer for read-only Announcements when not permitted.
- **OnSend:** validate non-empty → send message to server → optimistic append → reconcile on ack; clear input; auto-scroll.
- **OnEmoji:** open emoji picker; insert glyph into input.
- **OnMemberTap:** open member context (view profile; promote/demote/kick if the local player's role permits — all server-validated).
- **OnAction:**
  - **ClanWar** → open Clan War screen/flow.
  - **Donate** → open donation requests (contribute units/resources; server-validated, never client-mint).
  - **ClanShop** → open clan shop (spend clan currency).
  - **Manage** → clan settings (officer+ only; permission-gated/disabled otherwise).
  - **LeaveClan** → Confirm modal (spec 37); on confirm, call server `leaveClan`, then route to the clan-finder/join screen.
- **OnSettingsGear:** open clan notification/preference settings (or clan-level admin if permitted).
- **OnLanguagePill:** open language/region filter for the clan.
- **OnBack:** pop screen → Main Menu.
- **Failure:** network failure → NetworkError (spec 39) or inline retry; never fabricate roster/chat.

## L. NEGATIVE RULES
- Do NOT let the client edit roles, trophies, member count, or clan gold — **display only**, server-authoritative; role/permission gating must be enforced server-side too.
- Do NOT skip the **Confirm modal** before LEAVE CLAN (destructive).
- Do NOT replace this with a modal; it's a full screen.
- Do NOT use real brand text or stick figures; keep palette within DNA. Faction-blue banner is fine (clan identity).
- Do NOT enable **Send** with an empty input; do NOT show the composer on read-only Announcements without permission.
- Do NOT stretch avatars/banner on ultrawide — grow the center column instead.
- Do NOT over-animate the banner sway or status glow (legibility first).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel serif clan name well — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Three-column layout (Identity / Roster / Chat) with widths within ±2% of the E fractions; gold frames + dividers.
2. TopBar: back chevron + "DRAGONFORGE" + member chip "48/50" + gold "125,680" + "English" pill + gear, in source order.
3. Left column: blue dragon banner + "DRAGONFORGE" + motto "United in Fire, Forged in Glory." + the five stats (Clan Level 15, Clan Type Open, Required Level 30, Clan Region North America, Created 2024-02-14).
4. Center: tabs MEMBERS(selected)/Activity/Clan War/Clan Chest; header "MEMBERS 48/50" + ROLE/TROPHIES columns; the seven seed member rows with EXACT names/levels/roles/trophies/status; ranks 1-3 medallions; the (You) row (12 · Aric Stormblade · L52 · Member · 12,680) highlighted.
5. Action bar: CLAN WAR · DONATE · CLAN SHOP · MANAGE (gold icon buttons) + LEAVE CLAN (red); Leave triggers Confirm.
6. Right: CHAT(selected)/Announcements tabs; the five seed chat messages with senders/timestamps/text; composer "Tap to message..." + emoji + SEND; Send disabled when empty.
7. Role pills color-coded (Leader gold/Officer blue/Veteran bronze/Member grey); Online green / In Battle amber / "Nh ago" grey statuses.
8. Colors within DNA hex ranges; fraction-based + match-height stable; safe-area respected; BG full-bleed; animations per Section I.

## N. IMPLEMENTATION CONFIDENCE
**90/100.** High: tri-column structure, roster columns, role/status taxonomy, action bar, chat layout all read clearly. Risks: bespoke banner/dragon-crest + medallion + role-pill artwork (-4); gold-bevel serif needs TMP SDF (-2); live chat stream + permission gating is engineering beyond the visual (-2); exact stat progress-bar fragment under "Clan Level" is partially occluded/interpretive (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Member-list AND chat layout specified (Section D/E call-out per the SOCIAL batch).
- [x] Fraction-based layout → 2340×1080; match-height; safe-area; full-bleed BG.
- [x] Exact strings/numbers (clan name, motto, stats, all 7 members, all 5 chat messages, action labels) recorded.
- [x] Typography table + hex; materials (crest/banner) with hex/finish.
- [x] States, animation, FX (banner sway), events (OnSend/OnLeave/OnShow…), negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 36 · Settings
Source: design/SettingsScreenDesign.png · 1915×821 (≈2.33:1) · Analysis-only forensic spec.

> Normalize to 2340×1080 (≈19.5:9). CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). FRACTION-BASED layout; px quoted at 1080-tall height. Full-bleed BG under cutout; interactive content inside `Screen.safeArea`.

---

## A. SCREEN PURPOSE
The **System / Settings** screen. A **left tab rail** (General · Audio · Graphics · Controls · Account · Language · Support) selects content panels in the body. The **General** tab (shown) surfaces four grouped panels — **AUDIO** (Music/Sound Effects/Voice sliders + per-channel mute), **GRAPHICS** (Quality/Resolution/Frame Rate segmented selectors + Shadows/Bloom toggles), **ACCOUNT** (avatar/name/level/XP + Change Name + Link Account), **OTHER** (Vibration/Push Notifications/Battery Saver toggles + a faction emblem) — plus a bottom action bar **LOGOUT · PRIVACY POLICY · RESET SETTINGS**. **IMPLEMENTATION REALITY: only the mute toggle is functional today; every other control is a placeholder** (wire visually, no-op/stub the rest — see A/L). Reached from the Main-Menu rail and from the in-match Pause modal. No gameplay/ECS interaction.

## B. VISUAL DNA (screen-specific, inherits GLOBAL DNA)
- Dark heroic high-fantasy; near-black field over a faint **torch-lit dungeon/armory** backdrop (warm braziers, dark stone), heavily vignetted.
- **Brushed gold/antique bronze ornate title plate** "SETTINGS" centered top with **crown + wing/laurel** ornaments; gold frames around each grouped panel.
- **Left tab rail** = dark vertical bronze-framed strip; selected tab = warm gold highlight bar; others muted.
- **Violet/amethyst** = the gem currency (top-right) + a faint magical accent. **Royal blue** = the ACCOUNT action buttons (Change Name / Link Account) and selected segmented options. **Ember red** = the **LOGOUT** danger button. **Green** (#3fd07a) accents on ON toggles. **Gold** = section headers, slider fills, selected segment.
- Panel headers are small gold serif caps centered with flanking flourishes. Low-key lighting; warm focal glow from the background braziers; gold rim-light on frames.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
SettingsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ BackgroundLayer (full-bleed)
   │  ├─ BG_Dungeon (Image — torch-lit armory)
   │  └─ BG_Vignette (Image)
   ├─ TopBar
   │  ├─ BackButton (gold square + left chevron)
   │  ├─ TitleOrnamentLeft (crown/wing flourish)
   │  ├─ Title_SETTINGS (Text serif gold)
   │  ├─ TitleOrnamentRight (flourish)
   │  └─ CurrencyCluster
   │     ├─ GemChip (Icon_Gem(violet) + "1746" + "+")
   │     └─ GoldChip (Icon_Coin + "48570" + "+")
   ├─ BodyRow
   │  ├─ TabRail (left vertical)
   │  │  ├─ Tab_General (selected)  (icon + "General")
   │  │  ├─ Tab_Audio
   │  │  ├─ Tab_Graphics
   │  │  ├─ Tab_Controls
   │  │  ├─ Tab_Account
   │  │  ├─ Tab_Language
   │  │  └─ Tab_Support
   │  └─ ContentArea (General tab content)
   │     ├─ Panel_AUDIO
   │     │  ├─ Header_AUDIO
   │     │  ├─ Row_Music    (Icon_Speaker + "Music"         + Slider[100%] + "100%" + MuteToggle)
   │     │  ├─ Row_SFX      (Icon_Speaker + "Sound Effects" + Slider[100%] + "100%" + MuteToggle)
   │     │  └─ Row_Voice    (Icon_Speaker + "Voice"         + Slider[80%]  + "80%"  + MuteToggle)
   │     ├─ Panel_GRAPHICS
   │     │  ├─ Header_GRAPHICS
   │     │  ├─ Row_Quality    ("Quality"    + Seg[LOW|MEDIUM|HIGH|ULTRA*])
   │     │  ├─ Row_Resolution ("Resolution" + Seg[50%|75%|100%*])
   │     │  ├─ Row_FrameRate  ("Frame Rate" + Seg[30 FPS|60 FPS*|120 FPS])
   │     │  ├─ Row_Shadows    ("Shadows"    + Toggle[ON])
   │     │  └─ Row_Bloom      ("Bloom"      + Toggle[ON])
   │     ├─ Panel_ACCOUNT
   │     │  ├─ Header_ACCOUNT
   │     │  ├─ AccountIdentity (Avatar + "StickKing" + "Level 45" + XPBar["25600"])
   │     │  ├─ Btn_ChangeName (blue)
   │     │  └─ Btn_LinkAccount (blue)
   │     ├─ Panel_OTHER
   │     │  ├─ Header_OTHER
   │     │  ├─ Row_Vibration         ("Vibration" + Toggle[ON])
   │     │  ├─ Row_PushNotifications  ("Push Notifications" + Toggle[ON])
   │     │  ├─ Row_BatterySaver       ("Battery Saver Mode" + Toggle[ON])
   │     │  └─ FactionEmblem (crossed-swords shield art)
   │     └─ BottomActionBar
   │        ├─ Btn_LOGOUT (red)
   │        ├─ Btn_PRIVACY_POLICY
   │        └─ Btn_RESET_SETTINGS
```

## D. UNITY HIERARCHY SPEC (per node)
- **SettingsScreen** — parent UiRouter canvas. Empty `RectTransform` + `CanvasGroup`. Stretch-all. Full screen (NOT a modal; though it can be pushed over Pause).
- **SafeAreaRoot** — `SafeAreaFitter`, stretch-all. Children: Background, TopBar, BodyRow.
- **BackgroundLayer** (BG_Dungeon, BG_Vignette) — parent SettingsScreen (outside safe area). Stretch-all `Image`, raycast off.
- **TopBar** — parent SafeAreaRoot. Anchor top-stretch, pivot 0.5,1, fixed height (E). BackButton top-left; Title+ornaments top-center; CurrencyCluster top-right `HorizontalLayoutGroup`.
- **BackButton** — `Button`, (0,1) pivot 0,1, ≥88×88.
- **Title_SETTINGS** — `Text` serif gold, top-center, alignment center; ornaments anchored to its measured edges.
- **CurrencyCluster** — `HorizontalLayoutGroup` (spacing ~16), (1,1) pivot 1,1. GemChip + GoldChip; each = capsule `Image` + icon + value `Text` + "+" `Button` (opens Store).
- **BodyRow** — parent SafeAreaRoot. Stretch below TopBar (min (0,0) max (1, topBarBottomFrac)) pivot 0.5. Two regions: TabRail (left, fixed fractional width) and ContentArea (fills rest).
- **TabRail** — anchor left-stretch (0,0)-(0,1) pivot 0,0.5, fixed width (E). `Image` dark bronze-framed strip. `VerticalLayoutGroup` (spacing ~6) of 7 tabs; `ToggleGroup` (exclusive). Each **Tab_X** = `Toggle` with child `Image` (icon) + `Text` (label), left-aligned; selected shows a gold highlight bar `Image`.
- **ContentArea** — anchor: fills right of rail (min (railWidthFrac,0) max (1,1)) pivot 0.5. Uses a **2×2 grid of grouped panels** + a bottom bar. Layout via anchored panels (not a single LayoutGroup) because the four panels differ in height. Child order: Panel_AUDIO (top-left), Panel_GRAPHICS (top-right), Panel_ACCOUNT (bottom-left), Panel_OTHER (bottom-right), BottomActionBar (very bottom, spanning).
- **Panel_AUDIO / GRAPHICS / ACCOUNT / OTHER** — each `Image` gold-framed sub-panel with a centered **Header_X** `Text` (gold caps + flourishes) at top and a `VerticalLayoutGroup` of rows below.
- **Audio Row_X** — `HorizontalLayoutGroup`: Icon_Speaker `Image` | label `Text` (fixed width) | **Slider** (flexible) | percent `Text` (fixed) | **MuteToggle** (speaker `Toggle`). The **Slider** = uGUI `Slider` (0–100) with gold fill + round gold knob.
- **Graphics Row_Quality/Resolution/FrameRate** — `HorizontalLayoutGroup`: label `Text` | **SegmentedControl** = a `ToggleGroup` of N option `Toggle`s in a horizontal capsule (selected option = gold fill, others ghost). Quality has 4 options (ULTRA selected), Resolution 3 (100% selected), Frame Rate 3 (60 FPS selected).
- **Graphics Row_Shadows/Bloom** — `HorizontalLayoutGroup`: label `Text` | **Toggle** (pill ON/OFF, ON = green/gold knob right).
- **Panel_ACCOUNT → AccountIdentity** — `HorizontalLayoutGroup`/composite: circular Avatar `Image` (faction ring) | vertical block: name `Text` "StickKing", "Level 45" `Text`, **XPBar** = `Slider`/`Image` fill labeled "25600". Below identity: **Btn_ChangeName** and **Btn_LinkAccount** (blue `Button`s, stacked or side-by-side).
- **Panel_OTHER → Row_X** — `HorizontalLayoutGroup`: label `Text` | **Toggle** (ON). **FactionEmblem** = decorative `Image` (crossed-swords shield) anchored in the panel's right area; `raycastTarget=false`.
- **BottomActionBar** — parent ContentArea (or SettingsScreen body), anchor bottom-stretch pivot 0.5,0, fixed height. `HorizontalLayoutGroup`: **Btn_LOGOUT** (red), **Btn_PRIVACY_POLICY**, **Btn_RESET_SETTINGS** — left-weighted/grouped as in source (Logout leftmost).
- **Responsive:** match-height keeps row/slider heights stable. On ultrawide, the 2×2 grid keeps fractions; extra width pads between panels. Tab rail keeps min width. Notch handled by SafeAreaRoot; BG full-bleed.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar height:** ≈ 0.105 (~113 px). BackButton ≈ 88×88 left-inset ~1.4%. Title center; ornaments ±(title half-width + ~50 px). CurrencyCluster right-inset ~1.4%; each chip ~210 px incl. "+".
- **BodyRow:** y from ~0.105 to ~0.99.
- **TabRail width:** ≈ 0.165 of canvas width (~386 px), x≈[0.012, 0.177]. 7 tabs equally spaced over the rail height; each tab row height ≈ 0.115 of rail (~108 px), icon ~36px left + label. Selected highlight bar spans the tab width.
- **ContentArea:** x≈[0.19, 0.99], the 2×2 panel grid:
  - Column split inside content: left panels ≈ 0.46 of content width, right panels ≈ 0.46, gutter ~0.04 (Account/Audio narrower-left as in source; Graphics/Other on the right).
  - Row split: top panels (AUDIO, GRAPHICS) occupy ~0.46 of body height; bottom panels (ACCOUNT, OTHER) ~0.40; BottomActionBar ~0.12; small gutters ~0.02.
  - **Panel_AUDIO** top-left ≈ x[0.19,0.55] y[top]. **Panel_GRAPHICS** top-right ≈ x[0.57,0.99]. **Panel_ACCOUNT** bottom-left. **Panel_OTHER** bottom-right.
- **Audio row metrics:** row height ≈ 64 px; gap ~12. Speaker icon Ø ~32; label width ~0.28 of panel; slider track width ~0.40 of panel, knob Ø ~28; percent text width ~0.10; mute speaker toggle ~40×40 right.
- **Graphics row metrics:** row height ≈ 64 px. Segmented control: each option ~70–90 px wide in a shared capsule ~0.55 of panel width; selected option gold-filled. Toggle pill ~64×32.
- **Account panel:** Avatar Ø ~96 px; XP bar height ~16 px width ~0.7 of panel; ChangeName/LinkAccount buttons height ~56 px each.
- **Other panel:** three toggle rows height ~56 px; FactionEmblem ~140×140 right-area, decorative.
- **BottomActionBar:** height ≈ 0.10 of body (~104 px); three buttons ~0.20 width each, height ~64 px; LOGOUT leftmost (red), then PRIVACY POLICY, RESET SETTINGS; gap ~16.
- **Tablet 4:3 / ultrawide:** keep the 2×2 grid fractions; clamp content max-width ~0.97·2340 on ultrawide. **Notch:** inside SafeAreaRoot; BG bleeds.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for title/headers; Roboto-Medium SDF for body/labels/values. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "SETTINGS" | prestige serif | Heavy | UPPER | +6% | 1.0 | gold bevel + bloom + dark stroke | ~60 | #f0d27a / stroke #3a2c0e |
| Gem value "1746" | data | Bold | — | +1% | 1.0 | violet-tinged shadow | ~28 | #d8cdf0 |
| Gold value "48570" | data | Bold | — | +1% | 1.0 | shadow | ~28 | #f0d27a |
| Tab labels (General…Support) | menu sans | Bold | Title | +2% | 1.0 | sel gold-glow / unsel grey | ~28 | sel #f4dd8c / unsel #9a8a6a |
| Panel headers (AUDIO/GRAPHICS/ACCOUNT/OTHER) | section serif caps | Bold | UPPER | +6% | 1.0 | gold + flourishes | ~30 | #f0d27a |
| Audio labels (Music/Sound Effects/Voice) | label | Medium | Title | +1% | 1.0 | none | ~26 | #d9d2c2 |
| Percent values (100%/80%) | data | Bold | — | 0 | 1.0 | shadow | ~24 | #f0d27a |
| Graphics labels (Quality/Resolution/Frame Rate/Shadows/Bloom) | label | Medium | Title | +1% | 1.0 | none | ~26 | #d9d2c2 |
| Segmented options (LOW/MEDIUM/HIGH/ULTRA, 50%/75%/100%, 30/60/120 FPS) | option | Bold | UPPER | +2% | 1.0 | sel dark-on-gold / unsel grey | ~22 | sel #2a2010 / unsel #b8b0a0 |
| Toggle ON/OFF text (if shown) | state | Bold | UPPER | +2% | 1.0 | ON green / OFF grey | ~20 | ON #3fd07a / OFF #8c8270 |
| Account name "StickKing" | name | Bold | Title | +1% | 1.0 | shadow | ~30 | #f3ead2 |
| "Level 45" | meta | Medium | Title | +1% | 1.0 | none | ~24 | #c9bfa6 |
| XP value "25600" | data | Medium | — | 0 | 1.0 | none | ~22 | #d9d2c2 |
| Change Name / Link Account labels | CTA | Bold | Title | +2% | 1.0 | white on blue + shadow | ~24 | #ffffff |
| Other labels (Vibration/Push Notifications/Battery Saver Mode) | label | Medium | Title | +1% | 1.0 | none | ~26 | #d9d2c2 |
| LOGOUT label | danger CTA | Bold | UPPER | +3% | 1.0 | white on red + shadow | ~26 | #ffffff |
| PRIVACY POLICY / RESET SETTINGS labels | CTA | Bold | UPPER | +3% | 1.0 | gold/cream on dark | ~24 | #e9dcc0 |

## G. MATERIALS
- **Title plate / frames:** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin, worn edges, engraved filigree; crown/wing ornaments polished gold with bloom.
- **Panel fills:** obsidian #0c0e14→#14161e gradient, low reflectivity, inner shadow at frame edge.
- **Tab rail:** darker bronze-framed strip; **selected highlight** = warm gold gradient bar (#caa04a→#f0d27a) with soft glow; tab icons gold line-art.
- **Sliders:** track = recessed dark groove (#1a1c24) with a **gold fill** (#caa04a→#f0d27a) up to value; **knob** = round gold cabochon with specular + faint glow; mute speaker icon gold (lit) / grey (muted).
- **Segmented controls:** dark capsule with thin gold rim; **selected option** = gold gradient fill with dark text; unselected = transparent with grey text; thin separators.
- **Toggles (Shadows/Bloom/Vibration/Push/Battery):** pill track dark when OFF / **green-gold when ON** (#2e8a52→#3fd07a tint), gold knob; slight glow when ON.
- **Account avatar:** portrait sprite in beveled gold ring (faction-tinted); **XP bar** = gold fill in a dark groove.
- **Blue buttons (Change Name/Link Account):** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft glow.
- **FactionEmblem:** crossed-swords over a steel/blue shield, cast-metal relief, faint focal glow; decorative only.
- **LOGOUT:** oxblood/ember red gradient (#7a1f1a→#d8452b) gloss, danger glow. **PRIVACY/RESET:** dark stone capsules with gold rim + gold/cream label; RESET may carry a small refresh glyph.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
> **Functional-status note:** Per implementation reality, ONLY the **per-channel mute toggle** is live today; **all other controls render their states but no-op/stub** until wired. Treat every below as visually complete, behaviorally stubbed except the mute toggle.
- **BackButton:** idle gold chevron; hover +brightness+glow; pressed scale 0.94; SFX click.
- **Currency "+" buttons:** idle gold "+"; hover glow; pressed 0.96 → (stub) would open Store.
- **Tab rail tabs (Toggle group):** selected = gold highlight bar + bright label + glow; idle = no bar + muted label; hover = label brightens + faint bar; pressed = 0.97; disabled = 40%. Switching swaps ContentArea (General shown; other tabs reveal their own panels — stubbed today).
- **Sliders (Music/SFX/Voice):** idle gold fill + knob; hover knob +glow; **drag** = knob follows, fill updates, percent label live-updates; released = settle; disabled = desaturated. (Stub: value changes are visual only until audio mixer is wired.)
- **MuteToggle (FUNCTIONAL):** unmuted = lit gold speaker (sound waves); muted = grey speaker with slash; toggling **actually mutes that channel** (the one live behavior). Pressed scale 0.94; subtle SFX.
- **Segmented controls (Quality/Resolution/Frame Rate):** selected option gold-filled (dark text); others ghost; hover (unselected) brightens; pressed 0.96 → moves selection (stub: stored but not applied yet). ULTRA / 100% / 60 FPS are the seeded selections.
- **Toggles (Shadows/Bloom/Vibration/Push/Battery):** ON = knob-right + green-gold track + glow; OFF = knob-left + dark track; pressed = knob slides 120 ms; disabled = desaturated. (Stub today.)
- **Account buttons (Change Name/Link Account):** idle blue gloss; hover brighter+glow; pressed darken+0.96; disabled desaturated. (Stub → would open rename/link flow.)
- **BottomActionBar:** **LOGOUT (danger)** idle red, hover brighter red+glow, pressed darken+0.96 → triggers Confirm modal (spec 37) [stub action]. **PRIVACY POLICY** → opens URL/legal view (stub). **RESET SETTINGS** → Confirm modal then reset-to-defaults (stub). All ≥88px touch.
- **Feedback:** hover only on pointer platforms; pressed ≤80 ms scale; toggles/sliders give immediate visual response even when behaviorally stubbed.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms CanvasGroup α 0→1 over 180 ms. 40 ms title plate scale 0.985→1.0 over 200 ms (gentle back). 80 ms TabRail slide −20px→0 + α over 200 ms; tabs stagger 25 ms apart. 140–460 ms the four panels fade/scale-in (0.98→1) staggered 60 ms apart (AUDIO→GRAPHICS→ACCOUNT→OTHER). 260 ms BottomActionBar fades/raises in (180 ms). 220 ms FactionEmblem soft bloom one-shot.
- **Tab switch:** old content α 1→0 + slide −10px (120 ms); new content α 0→1 + slide +10px→0 (160 ms); selected highlight bar glides vertically to the new tab (140 ms ease-out).
- **Slider drag:** fill/percent track the knob in real time; release knob settle (80 ms, ease-out).
- **Toggle flip:** knob slides + track color crossfade over 120 ms (ease-out); ON adds a brief glow pop.
- **Segment select:** gold fill slides to the new option (120 ms ease-out).
- **LOGOUT/RESET pressed:** Confirm modal slide-in (per spec 37 I).
- **OnHide:** CanvasGroup α 1→0 over 140 ms; panels slight scale-down.
- **Easing:** ease-out enters, ease-in exits, gentle back on title/panel pops.

## J. PARTICLE & FX
- **Title ornaments / FactionEmblem:** steady focal bloom + occasional single sparkle.
- **Background braziers:** subtle warm flicker (low-amplitude brightness loop) + faint drifting embers (very low rate) — atmospheric, never over the controls.
- **Selected tab highlight & ON toggles:** subtle pulsing glow (±10%, 1.6 s).
- **Slider knobs:** faint glow on hover/drag only. Keep all FX low-key; control legibility first.

## K. EVENT BEHAVIOR
- **OnShow:** load current settings (audio levels, mute flags, graphics tier, toggles) from local prefs; bind account identity (name/level/XP) from profile; default tab = General.
- **OnTabChanged(General/Audio/Graphics/Controls/Account/Language/Support):** swap ContentArea to that tab's panels (today only General is fully populated; others stubbed/empty-state).
- **OnSliderChanged(channel,value):** update the percent label + (when wired) the audio mixer; persist to prefs. (Stub today — visual only.)
- **OnMuteToggled(channel) [FUNCTIONAL]:** mute/unmute that audio channel immediately; persist flag.
- **OnSegmentChanged / OnToggleChanged:** store the new value (stub: apply graphics/system change when wired); persist.
- **OnChangeName:** open rename flow (server-validated uniqueness). **OnLinkAccount:** open account-link (social/platform). [stubs]
- **OnLogout:** Confirm modal (spec 37) → on confirm, sign out / return to boot. [stub action]
- **OnPrivacyPolicy:** open legal/privacy view (URL or in-app). **OnResetSettings:** Confirm modal → reset prefs to defaults → refresh controls. [stubs]
- **OnCurrencyPlus:** open Store. **OnBack:** persist + pop screen → caller (Main Menu or Pause).

## L. NEGATIVE RULES
- **Do NOT imply non-mute controls are live** — only the per-channel mute toggle functions today; render the rest but stub/no-op (this is a known reality, surface it in code comments/empty-states; do not fake applied effects).
- Do NOT skip Confirm before **LOGOUT** and **RESET SETTINGS** (destructive).
- Do NOT mutate any server balance; account actions are server-validated; the client never changes XP/level.
- Do NOT replace this with a modal; it's a full screen (it may be pushed over Pause).
- Do NOT add real brand text or stick figures ("StickKing" is the seed username, keep it; no stick-figure art). Keep palette within DNA.
- Do NOT stretch panels/avatar on ultrawide — pad gutters and re-center.
- Do NOT make background braziers/embers distract from the controls.
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel serif title/headers well — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Title "SETTINGS" + crown/wing ornaments centered top; back chevron top-left; gem "1746" + gold "48570" chips top-right.
2. Left tab rail with all seven tabs (General selected, gold highlight): General/Audio/Graphics/Controls/Account/Language/Support, each with an icon.
3. AUDIO panel: Music (100%), Sound Effects (100%), Voice (80%) each with speaker icon + slider + percent + mute toggle; fills match values.
4. GRAPHICS panel: Quality segmented (ULTRA selected) / Resolution (100% selected) / Frame Rate (60 FPS selected) / Shadows (ON) / Bloom (ON).
5. ACCOUNT panel: avatar + "StickKing" + "Level 45" + XP bar "25600" + Change Name (blue) + Link Account (blue).
6. OTHER panel: Vibration (ON), Push Notifications (ON), Battery Saver Mode (ON) + the faction shield emblem.
7. Bottom bar: LOGOUT (red), PRIVACY POLICY, RESET SETTINGS; Logout & Reset trigger Confirm.
8. **Only the mute toggle is functional**; all other controls render correct states but are stubbed (verifiable: dragging a slider/flipping a graphics toggle changes visuals but not real settings yet).
9. Colors within DNA hex ranges; fraction-based + match-height stable; safe-area; BG full-bleed; animations per Section I.

## N. IMPLEMENTATION CONFIDENCE
**93/100.** High: rail+2×2 grid, slider/segment/toggle taxonomy, exact seeded values, action bar all read clearly. Risks: bespoke gold ornament/emblem/icon artwork (-3); gold-bevel serif needs TMP SDF (-2); the "only mute is functional" constraint means careful stubbing that's easy to over-implement (-1); exact XP-bar fill ratio for "25600" is interpretive (-1).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Left tab rail specified; "only mute toggle functional" reality flagged in A, H, K, L, M.
- [x] Fraction-based layout → 2340×1080; match-height; safe-area; full-bleed BG.
- [x] Exact strings/values recorded (tabs, audio %s, graphics selections, account name/level/XP, toggles, action labels, currencies).
- [x] Sliders/toggles/tabs/segmented components with states; typography + hex; materials with hex/finish.
- [x] Animation, FX, events, negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 37 · Confirm / Toast / Insufficient-Gems / Connection-Lost (4-in-1 modal sheet)
Source: design/ConfirmModalDesign.png · 1536×1024 (≈1.5:1) · Analysis-only forensic spec.

> **This source is a COMPONENT SHEET** — a 2×2 grid presenting FOUR reusable utility components, each with a numbered label: ①Confirm Modal, ②Toast Notification, ③Insufficient Gems Modal, ④Connection Lost Modal. The grid is a documentation layout ONLY; in production each component is an **independent overlay** that floats over the current screen. This spec specifies **all four** sub-components.
> Normalize to 2340×1080. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). All modals are CENTERED over a full-screen dim scrim that BLOCKS input to the screen beneath. FRACTION-BASED sizing; px quoted at 1080-tall height.

---

## A. SCREEN PURPOSE
A **reusable utility-overlay kit** used across the whole game:
- **①Confirm Modal** — generic destructive/spend confirmation ("Spend 150 gems?") with **CONFIRM / CANCEL**. Used by spend actions, Leave Clan, Logout, Reset Settings, surrender, etc.
- **②Toast Notification** — transient non-blocking success/info pill ("Equipped!") that auto-dismisses; does NOT block input.
- **③Insufficient Gems Modal** — blocks a purchase the player can't afford; states the shortfall ("You need 250 more gems…"), shows the deficit amount, and routes to **BUY MORE**.
- **④Connection Lost Modal** — compact network-failure overlay ("Unable to connect to the server…") with a single **RETRY**. (The full-screen connection-lost variant is spec 39; this is the compact modal form.)
All are **server/stub-aware** chrome; they never mutate balances themselves.

## B. VISUAL DNA (inherits GLOBAL DNA)
- Dark heroic high-fantasy; each modal is a **near-black panel** (#0c0e14→#14161e) inside a **brushed gold/antique bronze ornate frame** with **corner filigree flourishes** and a small **gold/blue gem finial** centered on the top edge.
- **Serif gold-bevel UPPERCASE titles**; the **primary CTA is royal/cobalt blue** and brightest; CANCEL is a ghost/outline secondary.
- Insufficient/Connection modals add a small **crest/shield emblem** above the title (amber/bronze) and an **amber-gold title** (warning tone) rather than pure gold.
- Toast is the odd one out: a **horizontal rounded bar** with a **green success glow** (#3fd07a) and a green check medallion — no scrim, no buttons.
- **Violet/amethyst** = the gem icon in ③. Low-key; focal glow on the CTA and the gem finial; gentle vignette implied by the scrim.

## C. SCREEN DECOMPOSITION (ASCII node trees — one sub-tree per component)
```
=== Shared overlay wrapper (used by ①③④; NOT ②) ===
ModalOverlay (UiScreen overlay root, CanvasGroup) — pushed ON TOP of current screen
└─ Scrim (Image, full-screen, semi-transparent black, raycastTarget=TRUE  ← blocks input)
   └─ ModalPanel (Image, ornate gold frame, centered)
      ├─ TopGemFinial (Image — gold/blue gem on top edge)
      ├─ CornerFlourish_TL / TR / BL / BR (Image x4)
      └─ <component-specific content>

=== ① Confirm Modal ===
ModalOverlay
└─ Scrim
   └─ ModalPanel (Confirm)
      ├─ TopGemFinial · CornerFlourish x4
      ├─ Title_CONFIRM (Text "CONFIRM")
      ├─ Body_Prompt (Text "Spend 150 [gem] gems?")   ← inline violet gem glyph between "150" and "gems"
      └─ ButtonRow
         ├─ Btn_CONFIRM (blue, primary)
         └─ Btn_CANCEL (outline, secondary)

=== ② Toast Notification (NO scrim, non-blocking) ===
ToastRoot (overlay, CanvasGroup, raycastTarget=FALSE on root)
└─ ToastBar (Image — rounded gold-rimmed pill, green glow)
   ├─ Icon_CheckMedallion (green ✓ circle)
   └─ Toast_Text (Text "Equipped!")

=== ③ Insufficient Gems Modal ===
ModalOverlay
└─ Scrim
   └─ ModalPanel (InsufficientGems)
      ├─ TopGemFinial · CornerFlourish x4
      ├─ Crest_Shield (amber/bronze shield emblem, top-center)
      ├─ Title_INSUFFICIENT_GEMS (Text "INSUFFICIENT GEMS", amber-gold)
      ├─ Body_Need (Text "You need 250 more gems to complete this purchase.")
      ├─ DeficitCluster
      │  ├─ Icon_Gem (large violet gem)
      │  └─ Val_Deficit (Text "100")
      ├─ Body_Hint (Text "Purchase more gems to continue.")
      └─ Btn_BUY_MORE (blue, primary)

=== ④ Connection Lost Modal (compact) ===
ModalOverlay
└─ Scrim
   └─ ModalPanel (ConnectionLost)
      ├─ TopGemFinial · CornerFlourish x4
      ├─ Crest_Shield (amber/bronze shield emblem, top-center)
      ├─ Title_CONNECTION_LOST (Text "CONNECTION LOST", amber-gold)
      ├─ Body_Msg (Text "Unable to connect to the server.\nPlease check your connection and try again.")
      ├─ Icon_WifiError (wifi bars + red ✕)
      └─ Btn_RETRY (blue, primary)
```

## D. UNITY HIERARCHY SPEC (per node)
**Shared (①③④):**
- **ModalOverlay** — parent: UiRouter canvas, pushed as an OVERLAY above the current screen (does NOT replace it). Empty `RectTransform` + `CanvasGroup`. Stretch-all. High sorting order.
- **Scrim** — parent ModalOverlay. Anchor stretch-all (full-bleed, ignores safe area so it dims the notch too). `Image` solid black @ ~55–65% alpha. **`raycastTarget = true`** → blocks all input to the screen beneath. Tapping the scrim = Cancel/Dismiss for ① (configurable; ③④ may require explicit button).
- **ModalPanel** — parent Scrim. Anchor center (0.5,0.5) pivot 0.5,0.5. `Image` 9-slice ornate gold frame; sized per E. Sits inside safe area (clamp center so the panel never hides under a notch). `raycastTarget=true`.
- **TopGemFinial** — parent ModalPanel. Anchor top-center (0.5,1) pivot 0.5,0.5 (overhangs the top edge). `Image`, raycast off.
- **CornerFlourish_TL/TR/BL/BR** — parent ModalPanel, anchored to each corner (0,1)/(1,1)/(0,0)/(1,0) with matching pivots. `Image`, raycast off.
- **Title_X** — `Text` serif, top-center under finial/crest, alignment center.
- **Body_X** — `Text`, center-aligned, wrapping, under title.
- **ButtonRow** (① only) — `HorizontalLayoutGroup` (spacing ~24), bottom-center of panel; two equal buttons.
- **Btn_CONFIRM / Btn_BUY_MORE / Btn_RETRY (primary blue)** — `Button` + label `Text`; cobalt gradient; min height 64 px; the brightest element.
- **Btn_CANCEL (secondary)** — `Button` + label; transparent fill with gold/grey outline; same height as Confirm.

**② Toast (independent, non-blocking):**
- **ToastRoot** — parent UiRouter canvas (overlay layer, above content but a toast can coexist with input). `CanvasGroup`; **root `raycastTarget=false`** so it never blocks. Anchor: top-center or above-center band (see E). Pivot 0.5,1 (slides from top) — or bottom-center per app convention; source shows a free-floating pill (use top-center default).
- **ToastBar** — `Image` rounded pill (gold rim, dark fill, green glow `Image` behind). `HorizontalLayoutGroup`: Icon_CheckMedallion + Toast_Text.
- **Icon_CheckMedallion** — `Image` green circle + check.
- **Toast_Text** — `Text`, left-aligned after the icon.

**Component-specific:**
- **Crest_Shield (③④)** — `Image` amber/bronze shield, anchor top-center, pivot 0.5,1 (above title), raycast off.
- **DeficitCluster (③)** — `HorizontalLayoutGroup` center: large gem `Image` + deficit `Text` "100".
- **Icon_WifiError (④)** — `Image` (wifi arcs + red ✕), centered between body and button.
- **Inline gem glyph (①)** — in "Spend 150 gems?", the violet gem appears between the number and word; implement via a TMP inline sprite or a small `Image` laid into the text flow (sprite asset in the TMP sprite sheet).

**Responsive:** modals are centered & size-clamped — on ultrawide they stay the same size (do NOT stretch); on small screens clamp to ≤90% width / ≤85% height. Scrim always full-bleed. Toast width hugs its content (min/max clamp). All panels keep their center inside safe area.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim:** full 2340×1080 (bleeds under cutout), alpha ~0.58.
- **① Confirm panel:** ≈ 0.30 W × 0.40 H (~700×432 px), centered. Frame border ~24 px. Title baseline ~0.20 from panel top; body ~0.45; ButtonRow centered ~0.78 from top. Each button ≈ 0.42 of panel width × 64 px; gap ~24 px. Gem finial overhang ~28 px above top edge.
- **② Toast bar:** height ≈ 80 px; width hugs content, clamp ~360–640 px (~0.15–0.27 W). Check medallion Ø ~48 px left; text padding ~20 px. Default anchor: top-center, ~0.12 from top (slides down to ~0.10). Green glow halo extends ~16 px beyond the bar.
- **③ Insufficient Gems panel:** ≈ 0.33 W × 0.52 H (~770×562 px), centered (taller). Crest shield Ø ~80 px overlapping top frame. Title ~0.20 from top; Body_Need (2 lines) ~0.36; DeficitCluster centered ~0.54 (gem Ø ~96 px + value); Body_Hint ~0.70; Btn_BUY_MORE ~0.85, ≈ 0.55 W × 64 px.
- **④ Connection Lost panel:** ≈ 0.33 W × 0.50 H (~770×540 px), centered. Crest shield top. Title ~0.20; Body_Msg (2 lines) ~0.38; Icon_WifiError centered ~0.58 (Ø ~96 px); Btn_RETRY ~0.84, ≈ 0.48 W × 64 px.
- **Documentation-grid note:** in the SOURCE sheet the four sit in a 2×2 at roughly: ① top-left, ② top-right, ③ bottom-left, ④ bottom-right, each ~0.40 W of the sheet with the numbered caption above-left. Reproduce sizes/contents, NOT the grid placement, in production (each is centered & solo).
- **Tablet/ultrawide:** fixed panel sizes, re-centered; never stretch. **Notch:** clamp panel center within safe area; scrim full-bleed.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for titles; Roboto-Medium SDF for body/buttons; TMP inline sprite for the gem glyph. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| ① Title "CONFIRM" | prestige serif | Heavy | UPPER | +6% | 1.0 | gold bevel + bloom + dark stroke | ~48 | #f0d27a / stroke #3a2c0e |
| ① Body "Spend 150 gems?" | prompt | Regular | Sentence | 0 | 1.15 | none (inline violet gem glyph) | ~28 | #d9d2c2 |
| ① CONFIRM label | primary CTA | Bold | UPPER/Title | +3% | 1.0 | white on blue + shadow | ~28 | #ffffff |
| ① CANCEL label | secondary | Bold | UPPER/Title | +3% | 1.0 | gold/grey on outline | ~28 | #cdbf9e |
| ② Toast text "Equipped!" | success | Bold | Title | +1% | 1.0 | faint green glow | ~30 | #eafff0 |
| ③ Title "INSUFFICIENT GEMS" | warning serif | Heavy | UPPER | +5% | 1.05 | amber-gold bevel + bloom + stroke | ~44 | #f0b24a / stroke #3a2607 |
| ③ Body "You need 250 more gems to complete this purchase." | body | Regular | Sentence | 0 | 1.2 | none | ~26 | #d9d2c2 |
| ③ Deficit value "100" | data | Black | — | 0 | 1.0 | violet-gold glow + shadow | ~40 | #e9dcc0 |
| ③ Hint "Purchase more gems to continue." | hint | Regular | Sentence | 0 | 1.15 | none | ~24 | #b8b0a0 |
| ③ BUY MORE label | primary CTA | Bold | UPPER | +3% | 1.0 | white on blue + shadow | ~28 | #ffffff |
| ④ Title "CONNECTION LOST" | warning serif | Heavy | UPPER | +5% | 1.05 | amber-gold bevel + bloom + stroke | ~44 | #f0b24a / stroke #3a2607 |
| ④ Body "Unable to connect to the server. Please check your connection and try again." | body | Regular | Sentence | 0 | 1.2 | none | ~26 | #d9d2c2 |
| ④ RETRY label | primary CTA | Bold | UPPER | +3% | 1.0 | white on blue + shadow | ~28 | #ffffff |

## G. MATERIALS
- **Frames (①③④):** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin, worn edges, engraved corner filigree; gold rim-light; **TopGemFinial** = faceted blue/gold gem with bloom.
- **Panel fills:** obsidian #0c0e14→#14161e gradient, low reflectivity, soft inner shadow at frame; faint vignette.
- **Crest shields (③④):** amber/bronze cast shield (#caa04a hi, #8a6a28 mid, #4a3410 sh) with a small exclamation/heraldic glyph and a warm glow — signals "attention/warning".
- **Primary blue buttons (CONFIRM/BUY MORE/RETRY):** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft outer glow (brightest object).
- **CANCEL (outline):** transparent dark fill with a gold/grey beveled rim, no glow.
- **② Toast:** rounded pill, dark fill (#10131a) with a **gold hairline rim** and a **green emissive glow** (#3fd07a) bleeding outward; **check medallion** = green disc (#2e8a52→#3fd07a) with white ✓ and faint bloom.
- **③ Gem icon:** faceted violet amethyst (#9e6bf0 core, #5a2db0 shadow, white specular) with bloom.
- **④ WifiError icon:** steel/grey wifi arcs with a bright **red ✕** (#d8452b) overlay + faint danger glow.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **Scrim:** static input-blocker; tap = Cancel for ① (if `cancelOnScrim`), ignored for ③④ (must use a button) — configurable per call. Fades with the modal.
- **Primary blue buttons (CONFIRM/BUY MORE/RETRY):** idle cobalt gloss+glow; hover brighter+glow grows; pressed darken+scale 0.96+inset; disabled desaturated grey @50% (e.g., RETRY briefly disabled while a retry is in flight → spinner). Feedback: confirm SFX.
- **CANCEL (secondary outline):** idle ghost; hover rim brightens + faint fill; pressed scale 0.96; gives a soft "back" SFX.
- **② Toast:** no interactive states (non-blocking, auto-dismiss); optional tap-to-dismiss-early.
- **RETRY in-flight:** show a small rotating spinner inside the button + disable until the network call resolves; on failure keep the modal, on success close it.
- **Feedback:** all buttons ≥88px touch; pressed ≤80 ms scale; primary always visually dominant over secondary.

## I. ANIMATION TIMELINE
- **Modal show (①③④):** 0 ms Scrim α 0→0.58 over 140 ms (ease-out). 60 ms ModalPanel scale 0.92→1.0 + α 0→1 over 200 ms (ease-out-back, overshoot ≤4%) — a "slide+pop" (optionally also +20px→0 vertical). 160 ms TopGemFinial + corner flourishes glint (one-shot sparkle). 180 ms primary CTA glow pulse begins (loop). For ③ the deficit gem does a soft bloom pulse at 200 ms; for ④ the wifi ✕ does a single shake (±4px, 180 ms).
- **Modal dismiss (OnConfirm/OnCancel/OnRetry-success):** ModalPanel scale 1→0.95 + α 1→0 over 140 ms (ease-in); Scrim α 0.58→0 over 160 ms; then pop overlay.
- **② Toast in:** α 0→1 + slide from top −24px→0 + scale 0.96→1 over 200 ms (ease-out-back); **hold ~1.6 s**; **out:** α 1→0 + slide +12px + scale 1→0.98 over 220 ms (ease-in). Green glow pulses once on entry. Total lifetime ~2.0 s (configurable).
- **Easing:** ease-out-back for entries (gentle), ease-in for exits.

## J. PARTICLE & FX
- **TopGemFinial / corner flourishes:** one-shot glint on show + steady faint bloom.
- **Primary CTA:** subtle pulsing rim glow (±10%, 1.6 s loop).
- **③ Gem:** soft amethyst bloom + occasional sparkle.
- **④ WifiError ✕:** faint red danger glow + the single entry shake (no looping jitter — keep calm).
- **② Toast:** green success glow flare on entry, then steady soft glow during hold; subtle outward shimmer. Keep all FX restrained — these are functional overlays.

## K. EVENT BEHAVIOR
- **①Confirm — OnShow(prompt, confirmLabel, cancelLabel, cost, cancelOnScrim):** render the prompt (with inline gem glyph if a gem cost is supplied). **OnConfirm:** invoke the caller's confirm callback (e.g., spend, leave clan, logout, reset) — the actual mutation is server/stub-validated by the caller, NOT by the modal — then dismiss. **OnCancel / OnScrim(if enabled) / OnBackKey:** dismiss with no action.
- **②Toast — OnToast(message, [icon], [duration]):** spawn, play in→hold→out, auto-destroy; never blocks input; multiple toasts queue/stack.
- **③Insufficient Gems — OnShow(needed, deficit):** display the shortfall text and deficit amount. **OnBuyMore:** route to the Store (gem packs); dismiss this modal. **OnCancel/OnScrim/OnBackKey:** dismiss (purchase aborted). Never auto-deducts.
- **④Connection Lost (compact) — OnShow(message):** display. **OnRetry:** disable button + spinner → re-attempt the failed network call; on success dismiss and resume; on failure keep the modal (optionally update message). **OnBackKey:** configurable (usually blocked until resolved, or routes to Main Menu via the full-screen variant spec 39).
- **General:** these overlays are **stateless/reusable** — created on demand, parameterized by the caller, and popped on resolution. They never persist game state.

## L. NEGATIVE RULES
- Do NOT let any modal mutate a currency/balance itself — the modal only invokes the caller's server/stub-validated callback; ③ never deducts, it routes to Store.
- Do NOT make the scrim transparent or non-blocking for ①③④ (input MUST be blocked beneath them); conversely the **Toast MUST NOT block input** (root raycast off).
- Do NOT reproduce the 2×2 documentation grid in production — each component is a solo, centered overlay.
- Do NOT stretch panels on ultrawide; keep fixed sizes, re-centered; keep centers inside safe area.
- Do NOT make CANCEL/secondary brighter than the primary blue CTA.
- Do NOT auto-dismiss ①③④ (they require a choice); do NOT make the Toast persistent (it must auto-expire).
- Do NOT add real brand text/stick figures; keep palette within DNA; amber title tone reserved for warning modals (③④).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render gold-bevel serif titles or the inline gem sprite well — **flag the TMP SDF upgrade** (and TMP sprite atlas for the gem glyph); don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. All four components specified & buildable as independent overlays; the doc-grid is not reproduced in production.
2. ①: gold-frame panel, "CONFIRM" title, "Spend 150 [violet gem] gems?" body, CONFIRM (blue, primary) + CANCEL (outline) — Confirm is the brightest element; scrim blocks input.
3. ②: rounded green-glow pill with check medallion + "Equipped!"; non-blocking; auto-dismisses (~2 s) with in/out anim.
4. ③: shield crest + amber "INSUFFICIENT GEMS" + "You need 250 more gems to complete this purchase." + big violet gem with "100" + "Purchase more gems to continue." + BUY MORE (blue) → routes to Store; no auto-deduct.
5. ④: shield crest + amber "CONNECTION LOST" + "Unable to connect to the server. Please check your connection and try again." + wifi-with-red-✕ icon + RETRY (blue) with in-flight spinner.
6. Every modal has the top gem finial + four corner flourishes; centered & size-clamped; scrim full-bleed at ~55–65% alpha.
7. Colors within DNA hex ranges; titles serif gold/amber; primary CTA cobalt brightest; animations per Section I.
8. No modal mutates a balance; toast never blocks; ①③④ always block.

## N. IMPLEMENTATION CONFIDENCE
**95/100.** Very high: simple, well-bounded reusable overlays; all text, structure, button roles, and the blocking/non-blocking distinction are unambiguous. Risks: bespoke frame/crest/finial/icon artwork + the TMP inline gem sprite (-3); gold/amber-bevel serif needs TMP SDF (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] ALL FOUR sub-components specified, each with its own ASCII sub-tree (per the 4-in-1 sheet rule).
- [x] Modal pattern = full-screen scrim Image[raycast block] + centered panel; Toast = non-blocking exception.
- [x] Fraction-based sizing → 2340×1080; centered & clamped; safe-area; scrim full-bleed.
- [x] Exact strings/numbers recorded (Confirm/Spend 150/Equipped!/Insufficient Gems/250/100/Connection Lost/Retry/Buy More).
- [x] Typography + hex; materials with hex/finish; states; animation (modal slide+scrim, toast in/out); events (OnConfirm/OnCancel/OnDismiss/OnToast/OnRetry); negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 38 · Reward Grant
Source: design/RewardGrantDesign.png · 1536×1024 (≈1.5:1) · Analysis-only forensic spec.

> Normalize to 2340×1080. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). This is a CENTERED modal overlay floating over the current screen on a dim scrim that BLOCKS input. FRACTION-BASED sizing; px quoted at 1080-tall height.

---

## A. SCREEN PURPOSE
The generic **"REWARD!" grant popup** — a reusable celebratory overlay shown whenever the player is awarded items (quest/daily/season/league/event/clan rewards, etc.). It announces the grant ("You have received the following:"), displays one or more **reward icons with amounts** inside a radiant burst, and ends with a single **COLLECT** action that acknowledges the grant and dismisses. Source shows two rewards: a **violet gem cluster "+40"** and a **silver coin stack "+500"**. The grant itself is **server-authoritative** (already applied or applied on COLLECT-ack server-side); this overlay is presentation + acknowledgement only. Floats over any screen.

## B. VISUAL DNA (inherits GLOBAL DNA)
- Dark heroic high-fantasy; a **near-black panel** (#0c0e14→#16131e, faintly warm) inside a **brushed gold/antique bronze ornate frame** with corner filigree and a small **blue/gold gem finial** centered on the top edge.
- Background (behind the panel/scrim) is a dim **battlefield at dusk with faction banners** and scattered braziers, heavily darkened so the panel pops.
- **Serif gold-bevel UPPERCASE title** "REWARD!" — large, celebratory, with strong bloom.
- The reward icons sit in a **warm radiant gold burst** (god-ray sunburst) — the focal celebration. **Violet/amethyst** gems + **silver** coins are the two reward materials shown.
- **Royal/cobalt blue COLLECT** CTA at the bottom — the brightest interactive element. A thin gold divider separates the rewards from the button. Low-key field → luminous focal burst.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
RewardGrantOverlay (UiScreen overlay root, CanvasGroup) — pushed ON TOP of current screen
└─ Scrim (Image, full-screen, semi-transparent black, raycastTarget=TRUE ← blocks input)
   ├─ BG_Ambience (Image — dim dusk battlefield + banners; optional, behind panel, above scrim)
   └─ RewardPanel (Image, ornate gold frame, centered, ~square)
      ├─ TopGemFinial (Image — blue/gold gem on top edge)
      ├─ CornerFlourish_TL / TR / BL / BR (Image x4)
      ├─ Title_REWARD ("REWARD!", serif gold)
      ├─ Subtitle_Received ("You have received the following:")
      ├─ RewardBurst (Image — radiant gold sunburst behind the items)
      ├─ RewardRow (HorizontalLayoutGroup)
      │  ├─ RewardItem_Gems
      │  │  ├─ Icon_GemCluster (violet crystals)
      │  │  └─ AmountChip_Gems (Text "+40" on dark capsule)
      │  └─ RewardItem_Coins
      │     ├─ Icon_CoinStack (silver star-stamped coins)
      │     └─ AmountChip_Coins (Text "+500" on dark capsule)
      ├─ Divider (Image — thin gold rule)
      └─ Btn_COLLECT (blue, primary)
```

## D. UNITY HIERARCHY SPEC (per node)
- **RewardGrantOverlay** — parent: UiRouter canvas, pushed as an OVERLAY above the current screen (does NOT replace it). Empty `RectTransform` + `CanvasGroup`. Stretch-all. High sorting order.
- **Scrim** — parent overlay. Anchor stretch-all (full-bleed, ignores safe area to dim the notch). `Image` solid black @ ~58% alpha. **`raycastTarget=true`** → blocks input beneath. Scrim tap is **ignored** here (must press COLLECT) — `cancelOnScrim=false`.
- **BG_Ambience** — parent Scrim (child, drawn above the flat scrim but behind the panel). Anchor stretch-all, `Image` dim battlefield, raycast off. Optional; if omitted, the flat scrim alone is fine.
- **RewardPanel** — parent Scrim. Anchor center (0.5,0.5) pivot 0.5,0.5. `Image` 9-slice ornate gold frame, ~square (see E). Center clamped inside safe area. `raycastTarget=true`.
- **TopGemFinial** — parent RewardPanel, anchor top-center (0.5,1) pivot 0.5,0.5 (overhangs). `Image`, raycast off.
- **CornerFlourish_TL/TR/BL/BR** — parent RewardPanel, anchored to each corner, raycast off.
- **Title_REWARD** — `Text` serif gold, anchor top-center pivot 0.5,1, alignment center, near panel top under finial.
- **Subtitle_Received** — `Text`, center-aligned, under title.
- **RewardBurst** — `Image` radiant sunburst, anchor center-upper (behind the reward row), pivot 0.5,0.5, raycast off, additive/screen-blend look. Drawn BEFORE RewardRow (earlier sibling).
- **RewardRow** — `HorizontalLayoutGroup` (spacing ~64, center-aligned), anchor center pivot 0.5,0.5. Holds N **RewardItem** children (here 2; layout must support 1–4 gracefully).
- **RewardItem_X** — `VerticalLayoutGroup` (center): big icon `Image` (`preserveAspect`) over an **AmountChip** (`Image` dark capsule + `Text` "+N", gold).
- **Icon_GemCluster** — `Image` violet amethyst crystal cluster, focal bloom. **Icon_CoinStack** — `Image` silver star-stamped coin stack.
- **AmountChip_X** — `Image` rounded dark capsule + centered `Text` "+40"/"+500" gold.
- **Divider** — `Image` thin gold rule, anchor horizontal-center below the reward row, raycast off.
- **Btn_COLLECT** — parent RewardPanel, anchor bottom-center pivot 0.5,0, `Button` cobalt gradient + label `Text` "COLLECT"; the brightest element; min height 72 px.
- **Responsive:** panel is centered & size-clamped — on ultrawide it stays fixed size (do NOT stretch); on small screens clamp ≤88% width / ≤90% height. RewardRow auto-centers for 1–4 items (wrap to a 2×2 if >4 — note as extension). Scrim full-bleed; panel center inside safe area.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim:** full 2340×1080, alpha ~0.58. BG_Ambience same bounds, darkened.
- **RewardPanel:** ≈ 0.345 W × 0.62 H (~807×670 px), **near-square**, centered. Frame border ~26 px 9-slice. Gem finial overhang ~30 px above top.
- **Title_REWARD:** baseline ~0.16 from panel top; large.
- **Subtitle_Received:** ~0.27 from top.
- **RewardBurst:** centered on the reward row at ~0.50 panel height, radius ~0.42 of panel width (extends slightly behind both icons).
- **RewardRow:** vertical center ~0.50 from top; two items with ~64 px gap; each item column ~0.30 of panel width. Icon Ø ~0.24 of panel width (~190 px); AmountChip below: ~0.20 W × 56 px.
- **Divider:** ~0.74 from top, width ~0.72 of panel, ~2 px tall.
- **Btn_COLLECT:** centered ~0.87 from top, ≈ 0.62 of panel width × 72 px.
- **Item count adaptivity:** 1 item → centered; 2 → as shown; 3 → equal thirds; 4 → tighter gaps or 2×2 grid; keep icons ≥0.18 W. Burst radius scales to span the row.
- **Tablet/ultrawide:** fixed panel size, re-centered, never stretched. **Notch:** clamp panel center within safe area; scrim/ambience full-bleed.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for title; Roboto-Medium SDF for subtitle/amounts/button. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "REWARD!" | celebratory prestige serif | Black | UPPER | +5% | 1.0 | heavy gold bevel + strong bloom + dark stroke + drop-shadow | ~72 | #f2d885 / stroke #3a2c0e |
| Subtitle "You have received the following:" | announce | Regular | Sentence | +1% | 1.1 | faint shadow | ~28 | #d9d2c2 |
| Amount "+40" (gems) | reward data | Black | — | 0 | 1.0 | gold + soft glow + shadow on capsule | ~36 | #f0d27a |
| Amount "+500" (coins) | reward data | Black | — | 0 | 1.0 | gold + soft glow + shadow on capsule | ~36 | #f0d27a |
| COLLECT label | primary CTA | Bold | UPPER | +4% | 1.0 | white on blue + shadow | ~32 | #ffffff |

## G. MATERIALS
- **Frame:** brushed gold/antique bronze (base #8a6a28, hi #f2d885, sh #5a3f12), satin, worn edges, engraved corner filigree; strong gold rim-light; **TopGemFinial** = faceted blue/gold gem with bloom.
- **Panel fill:** obsidian #0c0e14→#16131e (faintly warm) gradient, low reflectivity, soft inner shadow; subtle vignette toward corners.
- **BG_Ambience:** dim dusk battlefield, deep-blue faction banners at the sides, scattered warm braziers; heavily darkened/blurred.
- **RewardBurst:** warm **radiant gold sunburst / god-rays** (#ffdca0 core → #caa04a → transparent), additive/screen blend, soft bloom — the celebration light source behind the loot.
- **Icon_GemCluster:** faceted **violet amethyst** crystals (#a06ff2 core, #5a2db0 shadow, white specular), inner glow + bloom + sparkle.
- **Icon_CoinStack:** **silver/pewter** coins (#e6ebf1 hi, #aab2bb mid, #6c747d sh) with a **star** stamp on the face, metallic specular, faint glow.
- **AmountChips:** rounded near-black capsules (#10131a) with a thin gold rim; gold "+N" text.
- **Divider:** thin gold gradient rule (#caa04a→#f0d27a→fade).
- **Btn_COLLECT:** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft outer glow (brightest element).

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **Scrim:** static input-blocker; tap ignored (`cancelOnScrim=false`) — player must COLLECT. Fades with the overlay.
- **Btn_COLLECT (primary blue):** idle cobalt gloss + steady glow; hover brighter + glow grows (pointer platforms); pressed darken + scale 0.96 + inset; disabled (briefly, during the collect-ack round-trip) desaturated + small spinner. The only interactive element; ≥88px touch; satisfying confirm SFX + reward chime.
- **Reward icons / amount chips:** non-interactive display (no states), but they animate on entry (see I).
- **Feedback:** COLLECT press → reward chime, optional coin/gem fly-to-wallet micro-FX toward the top-right currency HUD of the underlying screen, then dismiss.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms Scrim α 0→0.58 over 150 ms (ease-out); BG_Ambience α 0→1 with it. 70 ms RewardPanel scale 0.90→1.0 + α 0→1 over 220 ms (ease-out-back, overshoot ≤5%). 160 ms RewardBurst scale 0.7→1.0 + α 0→1 over 260 ms with a slow continuous rotation begun (god-rays). 220 ms Title "REWARD!" pop: scale 0.85→1.0 + bloom flare over 200 ms. 300 ms each RewardItem staggers in (80 ms apart): scale 0.6→1.0 (ease-out-back) + α, with a sparkle burst on arrival; AmountChip counts up 0→value over ~400 ms (ease-out) [+40 / +500]. 520 ms Divider wipes in (left→right, 180 ms). 560 ms COLLECT fades/raises in (+16px→0, 180 ms) and begins its glow pulse.
- **OnCollect:** COLLECT scale 0.96 (80 ms); optional fly-to-wallet streaks (coins/gems arc to the top-right HUD over ~400 ms); then RewardPanel scale 1→0.96 + α 1→0 over 160 ms (ease-in), burst fades, Scrim α →0 over 180 ms; pop overlay.
- **Idle loops:** RewardBurst slow rotation (~12 s/rev) + gentle pulse (±6%, 2.4 s); COLLECT rim glow (±10%, 1.6 s); occasional sparkle on the gem cluster.
- **Easing:** ease-out-back entries (celebratory), ease-in exit.

## J. PARTICLE & FX
- **RewardBurst:** rotating god-ray sunburst + drifting gold dust motes (low rate) + a one-shot radial sparkle flare when items land.
- **Icon_GemCluster:** amethyst bloom + intermittent sparkle (1–2 motes).
- **Icon_CoinStack:** metallic glint sweep + tiny coin-shine sparkles.
- **TopGemFinial / corner flourishes:** one-shot glint on show + faint steady bloom.
- **COLLECT:** pulsing rim glow; on press, optional coin/gem fly-to-wallet streak particles toward the currency HUD. Celebratory but not seizure-y — bloom and sparkle, no harsh strobing.

## K. EVENT BEHAVIOR
- **OnShow(rewards[]):** receive a server-authoritative list of `{type, icon, amount}` (here gems+40, coins+500); build RewardItems dynamically (1–4); play the entry timeline; the grant is recorded server-side (either already applied, or applied on COLLECT-ack — caller decides; the overlay does NOT mint currency).
- **OnCollect:** disable button + spinner → send COLLECT/ack to the server-auth meta service → on success, play fly-to-wallet, refresh the underlying screen's wallet, dismiss the overlay; on failure, re-enable + surface NetworkError (spec 39) and keep the grant pending.
- **OnBackKey:** treated as COLLECT (acknowledge) — never silently discard a reward; if a round-trip is required, route through OnCollect.
- **Reusability:** parameterized & stateless; spawned on demand by any reward source; popped on acknowledgement.

## L. NEGATIVE RULES
- Do NOT let the client mint/add currency — amounts are display; the actual grant is server-authoritative (applied/acked server-side). The fly-to-wallet is cosmetic; the real wallet refresh comes from the server.
- Do NOT allow dismissal without acknowledgement — no scrim-tap close, no silent discard; COLLECT (or BackKey→Collect) only.
- Do NOT replace the underlying screen; this floats over it on a blocking scrim.
- Do NOT stretch the panel/icons on ultrawide; fixed size, re-centered; center inside safe area.
- Do NOT exceed 4 inline items without switching to a grid (keeps icons legible); do NOT shrink icons below ~0.18 panel width.
- Do NOT add real brand text/stick figures; keep palette within DNA (violet gems, silver coins, gold burst, cobalt CTA).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel "REWARD!" convincingly — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Centered near-square gold-framed panel with top gem finial + four corner flourishes, on a ~58% blocking scrim over a dim battlefield ambience.
2. Title "REWARD!" (large serif gold, strong bloom) + subtitle "You have received the following:".
3. Two reward items inside a radiant gold burst: violet gem cluster with "+40" and silver star-stamped coin stack with "+500", each on a dark amount chip.
4. Thin gold divider, then a single cobalt **COLLECT** button (brightest element, ≥72px tall, centered).
5. Layout supports 1–4 items centered; icons ≥0.18 panel width; amounts count up on entry.
6. COLLECT acknowledges (server-acked) and dismisses; no scrim-tap close; back key = collect.
7. Colors within DNA hex ranges; animations per Section I (panel pop, burst rotation, item stagger, count-up, collect fly-to-wallet); client never mints currency.

## N. IMPLEMENTATION CONFIDENCE
**95/100.** Very high: a single, well-bounded reusable popup; all text, structure, two reward types, and the COLLECT flow are unambiguous. Risks: bespoke gem-cluster/coin-stack/sunburst/frame artwork + sparkle FX (-3); gold-bevel "REWARD!" needs TMP SDF (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Modal pattern = full-screen blocking scrim Image[raycast] + centered panel.
- [x] Fraction-based sizing → 2340×1080; centered & clamped; safe-area; scrim full-bleed; adaptive 1–4 items.
- [x] Exact strings/numbers recorded (REWARD!, subtitle, +40 gems, +500 coins, COLLECT).
- [x] Typography + hex; materials (gem/coin/burst) with hex/finish; states; animation (count-up, burst, collect); FX; events (OnShow/OnCollect); negative rules (no minting, no silent dismiss).
- [x] No code/assets/scenes; analysis-only; invented nothing.



<div style="page-break-after: always;"></div>

===============================================================================

# BULWARK — UI CONSTRUCTION SPEC · 39 · Network Error (full-screen Connection Lost)
Source: design/NetworkErrorDesign.png · 1536×1024 (≈1.5:1) · Analysis-only forensic spec.

> Normalize to 2340×1080. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). This is the FULL-SCREEN connection-lost overlay: a centered gold-frame panel on a dim blocking scrim over the (frozen, dimmed) current screen — distinct from the compact Connection-Lost modal in spec 37 ④. FRACTION-BASED sizing; px quoted at 1080-tall height.

---

## A. SCREEN PURPOSE
The **full-screen "CONNECTION LOST"** overlay shown on a hard network failure (lost socket, failed required fetch, server unreachable). It (a) blocks all interaction with the dimmed screen beneath, (b) explains the failure and lists **possible causes** (unstable internet / weak signal / server temporarily unavailable), and (c) offers two recoveries: **RETRY** (re-attempt the failed operation / reconnect) and **MAIN MENU** (bail out to the hub). The background visibly shows a dimmed Main-Menu-like screen (left rail Quests/Heroes/Armory/Battle Pass/Store, right Events/Arena/Battle, top currencies) — confirming this floats over the live screen rather than replacing it. Server/connection state only; mutates nothing.

## B. VISUAL DNA (inherits GLOBAL DNA)
- Dark heroic high-fantasy; a **large near-black panel** (#0c0e14→#14161e) inside a **brushed gold/antique bronze ornate frame** with corner filigree and a **blue/gold gem finial** centered on the top edge.
- Behind it, the **dimmed previous screen** (Main-Menu chrome) is visible through/around the scrim — strongly darkened, desaturated, unblurred-to-lightly-blurred.
- **Serif gold-bevel UPPERCASE title** "CONNECTION LOST" (warm gold, not amber-warning here — larger/heroic, this is the dedicated full-screen treatment).
- A **distressed metal shield** with **wifi bars + a red crack/lightning bolt** is the hero glyph (left of the body text) — signals a broken connection. **Ember/oxblood red** = the crack accent. **Royal/cobalt blue** = the primary **RETRY** CTA (brightest). **MAIN MENU** is a gold/dark outline secondary.
- "Possible causes" is a small bullet list with line-icons (wifi / signal-bars / globe). Low-key field → focal panel + shield; gold rim-light on frame.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
NetworkErrorOverlay (UiScreen overlay root, CanvasGroup) — pushed ON TOP of current screen
└─ Scrim (Image, full-screen, semi-transparent black, raycastTarget=TRUE ← blocks input)
   ├─ (the previous screen renders beneath, dimmed)
   └─ ErrorPanel (Image, ornate gold frame, centered, landscape rectangle)
      ├─ TopGemFinial (Image — blue/gold gem on top edge)
      ├─ CornerFlourish_TL / TR / BL / BR (Image x4)
      ├─ Title_CONNECTION_LOST ("CONNECTION LOST", serif gold)
      ├─ LeftCluster
      │  └─ ShieldGlyph (Image — metal shield + wifi bars + red crack)
      ├─ RightCluster
      │  ├─ Body_Msg (Text "The connection to the server was lost. Please check your network and try again.")
      │  ├─ Lbl_PossibleCauses ("Possible causes:")
      │  └─ CausesList (VerticalLayoutGroup)
      │     ├─ Cause_Wifi   (Icon_Wifi   + "Unstable internet connection")
      │     ├─ Cause_Signal (Icon_Bars   + "Network signal is weak")
      │     └─ Cause_Server (Icon_Globe  + "Server temporarily unavailable")
      └─ ButtonRow
         ├─ Btn_RETRY (blue, primary; refresh icon + "RETRY")
         └─ Btn_MAIN_MENU (gold/dark outline, secondary; home icon + "MAIN MENU")
```

## D. UNITY HIERARCHY SPEC (per node)
- **NetworkErrorOverlay** — parent: UiRouter canvas, pushed as an OVERLAY above the current screen (does NOT replace it; the dimmed screen stays rendered beneath). Empty `RectTransform` + `CanvasGroup`. Stretch-all. **Highest sorting order** (it must sit above every other overlay too, since connection loss is global).
- **Scrim** — parent overlay. Anchor stretch-all (full-bleed, ignores safe area → dims notch). `Image` solid black @ ~60% alpha. **`raycastTarget=true`** → blocks all input to the screen beneath. Scrim tap ignored (must press a button) — `cancelOnScrim=false`.
- **ErrorPanel** — parent Scrim. Anchor center (0.5,0.5) pivot 0.5,0.5. `Image` 9-slice ornate gold frame; **landscape rectangle** (wider than tall, see E). Center clamped inside safe area. `raycastTarget=true`.
- **TopGemFinial** — parent ErrorPanel, anchor top-center (0.5,1) pivot 0.5,0.5 (overhangs). `Image`, raycast off.
- **CornerFlourish_TL/TR/BL/BR** — parent ErrorPanel, anchored to each corner, raycast off.
- **Title_CONNECTION_LOST** — `Text` serif gold, anchor top-center pivot 0.5,1, alignment center, near panel top under finial.
- **LeftCluster** — parent ErrorPanel, anchor center-left, pivot 0,0.5. Holds the ShieldGlyph (the hero art, left half of the body region).
  - **ShieldGlyph** — `Image` distressed metal shield with wifi arcs + a red crack/lightning; `preserveAspect`; focal glow.
- **RightCluster** — parent ErrorPanel, anchor center-right, pivot 1,0.5. `VerticalLayoutGroup` left-aligned: Body_Msg, Lbl_PossibleCauses, CausesList.
  - **Body_Msg** — `Text`, left-aligned, wrapping (2 lines).
  - **Lbl_PossibleCauses** — `Text` small label (gold), with a thin flanking gold rule as in source.
  - **CausesList** — `VerticalLayoutGroup` (spacing ~8); each **Cause_X** = `HorizontalLayoutGroup`: line-icon `Image` + `Text`.
- **ButtonRow** — parent ErrorPanel, anchor bottom-center pivot 0.5,0. `HorizontalLayoutGroup` (spacing ~28): Btn_RETRY then Btn_MAIN_MENU (a small ornamental separator dot may sit between, decorative).
  - **Btn_RETRY (primary blue)** — `Button` cobalt gradient + refresh `Icon` + label "RETRY"; brightest; min height 72 px.
  - **Btn_MAIN_MENU (secondary)** — `Button` dark/gold outline + home `Icon` + label "MAIN MENU"; same height.
- **Responsive:** panel centered & size-clamped — on ultrawide it stays fixed size (do NOT stretch); on small/tall screens clamp ≤92% width / ≤80% height. Left/right clusters keep their split; on very narrow (rare in landscape) stack shield above text as a fallback. Scrim full-bleed; the dimmed underlying screen + panel center stay inside safe area.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim:** full 2340×1080, alpha ~0.60 (the underlying Main-Menu chrome shows through, darkened).
- **ErrorPanel:** ≈ 0.50 W × 0.58 H (~1170×626 px), **landscape rectangle**, centered. Frame border ~26 px 9-slice. Gem finial overhang ~30 px above top.
- **Title_CONNECTION_LOST:** baseline ~0.16 from panel top, centered.
- **Body region split (below title, ~0.30–0.72 of panel height):**
  - **LeftCluster / ShieldGlyph:** left ~0.40 of panel width; shield Ø/height ~0.34 of panel height (~210 px), vertically centered in the body region.
  - **RightCluster:** right ~0.55 of panel width (x≈0.42–0.97). Body_Msg (2 lines) at top; "Possible causes:" label below with flanking rule; CausesList of 3 rows, each ~32 px tall, icon Ø ~26 px + text.
- **ButtonRow:** centered ~0.86 from panel top; two buttons each ≈ 0.40 of panel width × 72 px, ~28 px gap; RETRY left (blue), MAIN MENU right (outline). Optional center separator dot.
- **Tablet 4:3 / ultrawide:** fixed panel size, re-centered, never stretched (the underlying screen reflows on its own). **Notch:** clamp panel center within safe area; scrim full-bleed.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for title; Roboto-Medium SDF for body/causes/buttons. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "CONNECTION LOST" | heroic prestige serif | Black | UPPER | +5% | 1.0 | gold bevel + bloom + dark stroke + drop-shadow | ~64 | #f0d27a / stroke #3a2c0e |
| Body "The connection to the server was lost. Please check your network and try again." | body | Regular | Sentence | +1% | 1.25 | none | ~28 | #d9d2c2 |
| "Possible causes:" | label | Medium | Sentence | +2% | 1.0 | gold, flanked by thin rule | ~24 | #cdb474 |
| Cause items (Unstable internet connection / Network signal is weak / Server temporarily unavailable) | list | Regular | Sentence | 0 | 1.15 | none (line-icon left) | ~24 | #c9bfa6 |
| RETRY label | primary CTA | Bold | UPPER | +4% | 1.0 | white on blue + shadow | ~32 | #ffffff |
| MAIN MENU label | secondary CTA | Bold | UPPER | +3% | 1.0 | gold/cream on outline + shadow | ~30 | #e9dcc0 |

## G. MATERIALS
- **Frame:** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin, worn edges, engraved corner filigree; strong gold rim-light; **TopGemFinial** = faceted blue/gold gem with bloom.
- **Panel fill:** obsidian #0c0e14→#14161e gradient, low reflectivity, soft inner shadow at the frame; faint vignette.
- **Underlying screen (through scrim):** the live screen (Main-Menu chrome here) rendered then darkened ~60% + slightly desaturated; optional light blur. It is NON-interactive while this overlay is up.
- **ShieldGlyph:** distressed/worn **steel shield** (#cdd3da hi, #8a929c mid, #3a3f47 sh) bearing **wifi arcs**; a jagged **red crack / lightning bolt** (#d8452b→#7a1f1a) splits it with a faint danger glow + small embers at the fracture; subtle rust at the edges.
- **Cause line-icons:** thin gold/cream line-art — wifi waves, signal bars, globe; muted (#cdb474).
- **Btn_RETRY:** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft outer glow (brightest); refresh/circular-arrow icon in white.
- **Btn_MAIN_MENU:** dark stone capsule with a gold beveled rim + gold/cream label + home icon; no glow (secondary).

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **Scrim:** static input-blocker over the dimmed screen; tap ignored (`cancelOnScrim=false`) — must choose RETRY or MAIN MENU. Fades with the overlay.
- **Btn_RETRY (primary blue):** idle cobalt gloss + steady glow; hover brighter + glow grows (pointer); pressed darken + scale 0.96 + inset; **in-flight** = disabled + the refresh icon spins + small spinner (while re-attempting). On retry success → dismiss + resume; on failure → re-enable, keep the panel (optionally update body to reflect repeated failure). ≥88px touch; retry SFX.
- **Btn_MAIN_MENU (secondary outline):** idle dark/gold outline; hover rim brightens + faint fill; pressed scale 0.96; routes to the hub (tears down the failed flow). Soft "back" SFX.
- **CausesList / ShieldGlyph:** non-interactive display.
- **Feedback:** primary RETRY always visually dominant over MAIN MENU; both ≥88px touch; pressed ≤80 ms scale.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms Scrim α 0→0.60 over 150 ms (ease-out), underlying screen dims/desaturates with it. 70 ms ErrorPanel scale 0.92→1.0 + α 0→1 over 220 ms (ease-out-back, overshoot ≤4%). 150 ms ShieldGlyph entry: scale 0.85→1.0 + α over 200 ms, then a **single shake** (±5px horizontal, 200 ms) and the red crack glow flares once. 200 ms Title "CONNECTION LOST" pop (scale 0.9→1.0 + bloom, 180 ms). 260 ms RightCluster text fades/raises in (+12px→0, 180 ms); causes stagger 40 ms apart. 360 ms ButtonRow fades/raises in (+16px→0, 180 ms); RETRY begins its glow pulse.
- **Retry pressed:** RETRY scale 0.96 (80 ms) → disabled + icon spin (continuous ~0.8 s/rev) while the network call runs; on success → panel scale 1→0.96 + α 1→0 over 160 ms (ease-in) + scrim α →0 (180 ms), underlying screen restores, pop overlay; on failure → stop spin, re-enable, brief red flash on the shield crack (160 ms).
- **Main Menu pressed:** MAIN MENU scale 0.96 → panel/scrim fade out (160/180 ms) → route to hub.
- **Idle loops:** RETRY rim glow (±10%, 1.6 s); faint ember flicker at the shield crack (subtle). 
- **Easing:** ease-out-back entries, ease-in exits.

## J. PARTICLE & FX
- **ShieldGlyph crack:** faint red danger glow + a few small embers/sparks at the fracture (very low rate) + the one-shot flare on show and on retry-failure. No looping violent jitter — one entry shake only.
- **TopGemFinial / corner flourishes:** one-shot glint on show + steady faint bloom.
- **Btn_RETRY:** pulsing cobalt rim glow; spinning refresh icon while retrying. Keep FX restrained — this is a reassuring recovery screen, not an alarm strobe.

## K. EVENT BEHAVIOR
- **OnShow(context):** triggered by the network layer on a hard failure; capture the failed operation/route so RETRY can re-attempt it; render; block input globally.
- **OnRetry:** disable + spin → re-attempt the captured operation (reconnect socket / re-fire the failed request); on success → dismiss + resume the original flow; on failure → re-enable + keep the panel (optionally increment a retry counter / adjust copy); never fabricate success.
- **OnMainMenu:** abandon the failed flow → route to the Main-Menu hub (clean teardown of any in-progress match/meta call); dismiss the overlay.
- **OnBackKey:** map to MAIN MENU (safe bail) — never silently dismiss without resolving.
- **Auto-recover (optional):** if the network layer reconnects on its own while the panel is up, auto-invoke the success path (dismiss + resume) so the player isn't stranded.
- **Relationship to spec 37④:** use THIS full-screen variant for hard/global connection loss with two recoveries; use the compact 37④ modal for a single-RETRY in-context failure. Don't show both at once.

## L. NEGATIVE RULES
- Do NOT replace the underlying screen — it stays rendered (dimmed) beneath; this is an overlay so RETRY can resume exactly where the player was.
- Do NOT allow scrim-tap dismissal or any close without RETRY/MAIN MENU (BackKey → Main Menu).
- Do NOT fake a successful reconnect; RETRY must actually re-attempt and only dismiss on real success.
- Do NOT mutate any game/server state from this screen; it is recovery chrome only.
- Do NOT stretch the panel/shield on ultrawide; fixed size, re-centered; center inside safe area; scrim full-bleed.
- Do NOT make MAIN MENU brighter than RETRY (primary must dominate).
- Do NOT add real brand text/stick figures; keep palette within DNA (red crack accent, cobalt RETRY).
- Do NOT over-animate the shield (one entry shake + subtle ember flicker only — reassuring, not alarming).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel serif title well — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Full-screen ~60% blocking scrim over the dimmed/desaturated previous screen (Main-Menu chrome visible beneath); centered landscape gold-frame panel with top gem finial + four corner flourishes.
2. Title "CONNECTION LOST" (large serif gold) centered near the top.
3. Left: distressed metal shield with wifi bars + red crack/lightning (hero glyph) with focal glow.
4. Right: body "The connection to the server was lost. Please check your network and try again." + "Possible causes:" + three line-icon causes — "Unstable internet connection", "Network signal is weak", "Server temporarily unavailable".
5. Bottom: RETRY (blue primary, refresh icon, brightest) + MAIN MENU (gold/dark outline secondary, home icon); RETRY spins/disables in-flight and only dismisses on real reconnect; MAIN MENU routes to hub.
6. No scrim-tap close; back key = Main Menu; nothing mutated.
7. Colors within DNA hex ranges; panel sized ~0.50W×0.58H centered & clamped; safe-area; scrim full-bleed; animations per Section I.

## N. IMPLEMENTATION CONFIDENCE
**94/100.** High: layout (shield-left / text-right / two buttons), copy, causes, and recovery flow are unambiguous; the overlay-over-dimmed-screen pattern is clear from the visible Main-Menu chrome. Risks: bespoke cracked-shield/wifi + cause line-icon + frame artwork (-3); gold-bevel serif needs TMP SDF (-2); robust "re-attempt the exact failed operation" wiring is engineering beyond the visual (-1).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Full-screen overlay = blocking scrim Image[raycast] over the dimmed live screen + centered panel (distinguished from compact 37④).
- [x] Fraction-based sizing → 2340×1080; centered & clamped; safe-area; scrim full-bleed.
- [x] Exact strings recorded (CONNECTION LOST, body, Possible causes + 3 items, RETRY, MAIN MENU).
- [x] Typography + hex; materials (cracked shield) with hex/finish; states (RETRY in-flight spinner); animation; FX; events (OnRetry/OnMainMenu/OnBack/auto-recover); negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.
