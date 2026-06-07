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
