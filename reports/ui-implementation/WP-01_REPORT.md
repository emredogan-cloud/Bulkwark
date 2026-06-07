# BULWARK — UI IMPLEMENTATION · WP-01 REPORT · SPLASH SCREEN

**Date:** 2026-06-06 · **Layer:** Presentation only (§12) · **Scope:** WP-01 (Splash Screen).
**Status:** **AUTHORED + SELF-VALIDATED (adversarial review PASS-WITH-NITS; no repairs required).**
**PENDING:** Unity compile / CI GREEN / Android device validation — *your pipeline (I cannot run them here).*

> **Operating context:** GATE-1 (Gameplay Recovery) still active/FAIL; UI authorized to proceed with stubs;
> WP-00 accepted **PROVISIONAL PASS** (CI/device continuing in parallel). Per your instruction I did **not**
> wait for WP-00 CI — no WP-00 defect was discovered (the WP-01 foundation refinement below is an
> enhancement, not a defect). If WP-00 CI later fails, I will stop, repair, regenerate the WP-00 report, and
> document the impact here.

---

## 1. Objectives → status
| Objective | Status |
|---|---|
| Exact recreation of the frozen Splash design | ✅ layout/behavior reproduced; **art is the documented placeholder path** (§6) |
| Landscape correctness | ✅ 2340×1080 match-height; full-bleed bg; content in safe area |
| Safe-area correctness | ✅ per-screen `SafeContent` (SafeAreaFitter; re-applies on L↔R rotation) |
| Audio integration | ✅ menu music on show + click on dismiss (`AudioManager`, null-safe) |
| Fade transition into Main Menu through UiRouter | ✅ `Router.PopFaded()` fade-out → reveals Main Menu (see §7) |
| No gameplay interaction / No ECS access / No backend dependency | ✅ verified (grep-clean; §5) |

## 2. Files

### New (presentation-only, `Bulwark.Bootstrap`, with `.meta` + unique GUID)
- **`Assets/_Game/Bootstrap/SplashScreen.cs`** — `SplashScreen : UiScreen`. Boots via
  `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`: sets `PresentationState.RouterOwnsEntry = true` and
  `UiRouter.Instance.Show<SplashScreen>()`. Builds: full-bleed background (`bg_splash`→`bg_menu`→solid),
  ornate frame (`panel` sprite) + **BULWARK** wordmark + faction line, pulsing **TAP TO BEGIN**, and a
  full-screen transparent tap-catcher. Dismiss (tap or 8 s safety timeout) → `Router.PopFaded()`.

### Edited
- **`PresentationState.cs`** — added `RouterOwnsEntry` (presentation-only migration seam).
- **`UiFlow.cs:58`** — `Start` now `Show(PresentationState.RouterOwnsEntry ? Screen.Menu : Screen.Splash)`
  (legacy flow unchanged when the flag is false).

### WP-00 foundation refined (see §3)
- **`UiNavigation.cs`** — `UiScreen` now exposes a **per-screen `SafeContent`** (full-stretch child +
  `SafeAreaFitter`); screens are **full-bleed** under the Canvas; added **`PopFaded()`** (fade-out + pop).

## 3. WP-00 foundation refinement (transparency)
Building the first real screen surfaced a limitation in the initial WP-00 cut: the `UiRouter` parented screens
under a single router-owned SafeArea root, which would **inset full-screen backgrounds** (a splash key art
must fill the screen *under* the side cutout). **Refinement:** safe-area handling moved to a **per-screen
`SafeContent`** child created in `UiScreen.Bind`; the screen root is full-bleed (backgrounds), interactive
content goes in `SafeContent`. Also added `PopFaded()` for the Splash→Menu fade-out. This **supersedes the
"SafeArea root" description in `WP-00_REPORT.md §2.1`** (a short addendum was added there). It is a
presentation-only enhancement, reviewed below; no behavioral regression to WP-00's other deliverables.

## 4. Validation performed
- **Structural (local):** braces balanced (`SplashScreen` 13/13, `UiNavigation` 32/32); **§12 grep clean**
  (no `Unity.Entities`/`EntityManager`/`World`/`MatchState`/`Bulwark.Sim`/`EnqueueTrain`/etc. in the WP-01
  files); seam wired (`RouterOwnsEntry` in SplashScreen/PresentationState/UiFlow); SplashScreen GUID unique.
- **Independent adversarial review (subagent, read-only): VERDICT PASS-WITH-NITS.** No compile-breakers;
  nothing that stops the splash showing, dismissing, or catching input. It specifically cleared:
  **boot ordering** (all `AfterSceneLoad` hooks run before any `Start`, and UiFlow reads the flag only in
  `Start` — so the flag is always set in time; UiFlow has no `Awake`); **z-order/raycast** (Bg
  `SetAsFirstSibling` behind content; full-screen TapCatcher topmost; frame/labels `raycastTarget=false` —
  tap-anywhere works, nothing blocks); **AddComponent→Awake-before-Bind** (screen builds in `Build()`, not
  `Awake`); **AudioManager methods exist** (`PlayMenuMusic`/`Click`); **PopFaded** correctness incl. empty
  stack + race re-check; **EventSystem** single-creation; **§12** clean; **landscape/safe-area** correct on
  2340×1080 + side-notch; **no regression** (legacy flow byte-identical when flag false; splash can't get
  stuck — 8 s safety timeout).
- **Nits (LOW/INFO — no repair):** (a) dead `Arial.ttf` fallback — intentionally mirrors the existing
  `UiFlow.cs:53`/`BattleHud` idiom (kept for codebase consistency; `LegacyRuntime.ttf` always resolves);
  (b) `bg_splash` not in `PlaceholderAssets.Names` → falls back to `bg_menu` (intended placeholder path);
  (c) change-hygiene: the WP-00 scaler refactor rides along in the uncommitted working tree (§9).

## 5. §12 / binding-rules compliance
No ECS/sim/AI/economy/balance/combat/match-rule/commander/recovery/backend code touched. No scenes, no
prefabs, no canvases authored into scenes (all code-built). The `UiFlow` edit only changes **which `Screen`
enum value is shown at Start** — no gameplay/sim/timeScale logic altered. SplashScreen touches only
`PlaceholderAssets`, `AudioManager`, `UiRouter`, and the `PresentationState` flag. **No gameplay change.**

## 6. Design fidelity & the placeholder-art path (honest "as close as practical")
`design/SplashScreenDesign.png` = epic landscape key art + an **empty ornate gold frame (logo-ready)** +
centered gold "TAP TO BEGIN". The implementation reproduces the **composition and behavior** exactly
(full-bleed key-art bg, centered frame at y≈0.60, wordmark inside, gold tap prompt near the bottom, landscape,
safe-area, fade-to-menu). Two elements are the **expected placeholder path**, not exact-pixel matches, because
the final assets do not exist yet:
- **Background:** no `bg_splash` sprite exists → falls back to the `bg_menu` placeholder (and the mockup art is
  itself "Stick Empire" placeholder style). Final BULWARK splash art drops in by adding `bg_splash.png` to
  `StreamingAssets/bulwark/` + `PlaceholderAssets.Names` (per the Design Import Contract) — no code change.
- **Wordmark:** no logo asset → a styled **BULWARK** text stands in (the mockup frame is deliberately empty).
This is the established placeholder→final-art path from the freeze; it needs no approval. *If pixel-exact
splash art is required before WP-02, that is an asset-production task (generate `bg_splash` + logo), which I
flag here rather than improvise.*

## 7. The Splash → Main Menu transition (current seam)
On dismiss, `Router.PopFaded()` fades the splash out (unscaled) and pops it, emptying the shell stack and
**revealing the Main Menu beneath** (the shell Canvas is sortingOrder 200; the legacy UiFlow menu is 100).
Because `RouterOwnsEntry` makes UiFlow start on `Screen.Menu`, the menu is present to reveal. **Today that is
the legacy (portrait-tuned) UiFlow menu rendered in landscape — it will look off until WP-04 rebuilds it**;
that is the expected migration state. When WP-04 lands a `MainMenuScreen : UiScreen`, this transition becomes
`Router.Replace<MainMenuScreen>()` (cross-fade), a one-line change in `Dismiss()`.

## 8. Validation PENDING (your pipeline)
- [ ] Unity compile · CI GREEN.
- [ ] Android (Redmi Note 11R): app launches **landscape-locked**; the **Splash shows** with safe-area clear
  of the side cutout; **TAP TO BEGIN** (or the 8 s timeout) **fades to the Main Menu**; menu music plays; no
  double-splash; back-rotation L↔R keeps safe-area correct.

## 9. Notes / change hygiene
- The working tree currently contains **both** WP-00 and WP-01 changes (WP-00 is provisional-pass and
  uncommitted). If you commit per-WP, WP-00 = {`UiScaling`, `SafeAreaFitter`, `UiNavigation`, the 2 scaler
  edits, ProjectSettings} and WP-01 = {`SplashScreen`, `PresentationState` flag, the `UiFlow:58` one-liner,
  the `UiNavigation` refinement}. Still **uncommitted** (I don't commit/push unless you ask).
- BattleHud's inline safe-area is unchanged; it reconciles onto `SafeAreaFitter` in WP-12 (Match Layer).

---

**STOP — per the one-WP-at-a-time cadence.** WP-01 authored, self-validated (PASS-WITH-NITS, no repairs).
Awaiting your **CI GREEN + Android validation + explicit PASS** before WP-02 (Loading Screen).
