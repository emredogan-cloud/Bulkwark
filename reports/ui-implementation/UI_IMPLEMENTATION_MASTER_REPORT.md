# BULWARK — UI IMPLEMENTATION · MASTER REPORT (continuously updated)

**Date:** 2026-06-06 · **Layer:** Presentation only (§12). **Status:** WP-00 → WP-13 **AUTHORED + self/
adversarial-reviewed (PASS / PASS-WITH-NITS, nits repaired).** **PENDING (your pipeline):** Unity compile ·
CI GREEN · Android device validation — I cannot run those here; every WP is authored, reviewed, and reported,
and the project's CI/device runs are the user's pipeline.

> **Self-contained:** this file embeds every WP report (WP-00…WP-13) in full below the summary, so the entire
> UI implementation can be understood from this one document. Authoritative source obeyed:
> `reports/ui-production/99_UI_MASTER_CONSOLIDATED_REPORT.md` (the freeze) + the WP prep packages.

---

# PART 0 — PROGRAM SUMMARY

## 0.1 What was built
A complete **landscape, code-built uGUI front-end** on a new navigation shell, executed continuously WP-00→
WP-13 per the frozen roadmap. **20 new files (~1,607 LOC)** in `Assets/_Game/Bootstrap/`, **4 surgical edits**
to existing files, **0 gameplay/ECS/balance/AI/recovery/backend changes**, **0 scene/prefab changes**.

**Foundation (WP-00 + shared layer):** `UiScaling` (landscape 2340×1080, match-height), `SafeAreaFitter`,
`UiNavigation` (`UiScreen` + `UiRouter` stack/fades/toast), `UiWidgets` (shared builders), `UiStub` (authorized
fake display data while services are unwired), `UiModals` (reusable overlays).

**Screens (WP-01…WP-13):** Splash · Loading · Login · Main Menu · Mode Select (+Commander) · Store · Battle
Pass · Quests · Commander Select · Settings · Reusable modals · Match Intro · Pause · Victory/Defeat.

## 0.2 Navigation graph (as implemented)
```
Splash → Login → Loading → MAIN MENU ─┬─ PLAY → MODE SELECT ─┬─ Classic/Tournament/Endless → MATCH INTRO → MATCH
   (UiRouter shell; sortingOrder 200)  │                      │     (Router.Clear → existing BattleHud + ECS battle,
                                        │                      │      in-match PAUSE overlay) → VICTORY/DEFEAT → CONTINUE → MAIN MENU
                                        │                      ├─ Campaign / Online → "coming soon" (out of this sequence)
                                        │                      └─ COMMANDER → Commander Select
                                        ├─ STORE → Store ─ Battle Pass
                                        ├─ rail: QUESTS → Quests ·  SETTINGS → Settings ·  UNITS/CLAN/LEADERBOARD → "coming soon"
                                        ├─ CHESTS → ADR notice ·  bottom SPIN → ADR notice ·  DAILY/FREE/EVENTS → "coming soon"
                                        └─ currency chips: Gold + Gems (frozen model; no Energy)
PAUSE → Resume / Settings / Surrender(→Defeat).   Modals (Confirm/Reward/Insufficient/NetworkError) float over any screen.
```
The legacy `UiFlow` remains the **match orchestrator** (timeScale + ECS `MatchState` read-only watch); the shell
drives the meta + intro + end and delegates the end screen to itself via a presentation seam
(`PresentationState.RouterOwnsEntry`).

## 0.3 Validation summary
- **Per-WP self-review** + **two independent adversarial reviews** (subagents): WP-00 (PASS-WITH-NITS), WP-01
  (PASS-WITH-NITS), and a **batch review of WP-02→WP-13 (PASS-WITH-NITS)** that cross-checked **every** widget/
  router/stub/audio/match call signature (compile-clean), the nav/match/end flows, z-order, lifecycle guards,
  boot ordering, and regressions.
- **Structural checks:** braces/parens balanced on all 20 files; every `.cs` has a `.meta` (unique GUIDs);
  **§12 grep clean** (no ECS/sim symbols in any shell file — the only sim touches are the permitted
  `Time.timeScale` + `PresentationState.InMatch`; outcome is read by `UiFlow` read-only).
- **Defects found & repaired:** WP-00 (2 LOW nits), WP-01 (foundation safe-area refinement + 2 LOW),
  WP-08 (self-caught divide-by-zero in quest claim → `Claimed` flag), WP-12 (defensive `Clear()` on end),
  **WP-13 (MED: RETRY bounced to the end screen → made graceful + documented the gameplay blocker)**, plus a
  systemic **z-order fix** (`SafeContent.SetAsLastSibling()` so backgrounds never cover content).

## 0.4 ⚠️ Known blocker (GAMEPLAY, not UI — documented, NOT implemented per the binding rules)
**Rematch / replay is blocked by the sim:** the ECS battle supports **one battle per launch** and exposes no
world-reset, so re-entering a finished match immediately re-resolves (the `MatchState.Outcome` persists). RETRY
and repeated PLAY therefore **degrade gracefully** (notice + return to menu). Resolving this needs a gameplay/
sim **world-reset / new-match API** — out of scope for this UI program (rule: document gameplay needs, don't
implement them). **Surfaced for the gameplay owners.** The single-match flow (start → play → Victory/Defeat →
menu) is complete.

## 0.5 Pending your pipeline (cannot run here)
Unity compile · CI GREEN · Android (Redmi Note 11R): landscape-locked launch; boot flow Splash→Login→Loading→
MainMenu; each meta screen renders safe-area-correct; a match runs via the existing HUD with the in-match Pause;
Victory/Defeat shows and returns to menu. (Existing portrait-tuned legacy panels sit beneath the shell, covered
— intentional migration state.)

## 0.6 Files
**New (20):** `UiScaling, SafeAreaFitter, UiNavigation, UiStub, UiWidgets, UiModals, SplashScreen,
LoadingScreen, LoginScreen, MainMenuScreen, ModeSelectScreen, StoreScreen, BattlePassScreen, QuestsScreen,
CommanderSelectScreen, SettingsScreen, MatchIntroScreen, PauseModal, EndScreen, MatchPresentation` (+ `.meta`).
**Edited (4, presentation/platform only):** `UiFlow.cs` (scaler + flow seams), `BattleHud.cs` (scaler swap
only), `PresentationState.cs` (+`RouterOwnsEntry`), `ProjectSettings.asset` (landscape orientation lock).
**Uncommitted** in the working tree (I commit/push only on request).

## 0.7 Frozen-decision compliance
Landscape mandatory (✓ 2340×1080 match-height + orientation lock); currencies **Gold + Gems**, Energy **cut**
(✓); skins/chests/spells/spin **excluded** (ADR-gated — not built; surfaced as notices) (✓); spells-as-power /
gems-buy-power **not** introduced (✓); §12 boundary held (✓); original-branding pending art (BULWARK wordmark
placeholder text; per-screen art falls back to existing keys) (✓ flagged).

---

# PART 1 — PER-WORK-PACKAGE REPORTS (verbatim)


---

# BULWARK — UI IMPLEMENTATION · WP-00 REPORT · LANDSCAPE MIGRATION FOUNDATION

**Date:** 2026-06-06 · **Layer:** Presentation only (§12) · **Scope:** WP-00 (landscape foundation).
**Status:** **AUTHORED + SELF-VALIDATED (PASS-WITH-NITS, nits repaired).** **PENDING:** Unity compile / CI
GREEN / Android device validation — *the project's pipeline, which I cannot run from this environment.*

> **Operating context (per explicit authorization):** GATE-1 (Gameplay Recovery) is **still active / FAIL**
> (latest revalidation: "FAIL (stalemate)"). The UI/presentation layer was **explicitly authorized to proceed
> anyway**, with the rule: any dependency on incomplete RC/gameplay systems is wired to **stubs/fake data**,
> and **gameplay code is never touched**. WP-00 is pure presentation and has **no gameplay dependency**, so no
> stubs were needed here.

---

## 1. Objectives (from the implementation program) → status
| Objective | Status |
|---|---|
| Convert entire UI system to landscape | ✅ done (CanvasScaler migration, see §2.2) |
| SafeArea support | ✅ done (reusable `SafeAreaFitter` + router safe-area root, §2.1) |
| Orientation lock | ✅ done (portrait autorotation disabled, §2.3) |
| CanvasScaler migration | ✅ done (single source of truth `UiScaling`, §2.2) |
| Navigation shell foundation | ✅ done (`UiScreen` + `UiRouter`, inert until WP-01, §2.1) |

## 2. Exact changes

### 2.1 New files (presentation-only, all `namespace Bulwark.Bootstrap`, with `.meta` + unique GUIDs)
- **`Assets/_Game/Bootstrap/UiScaling.cs`** — static single-source-of-truth for the landscape CanvasScaler
  (`ReferenceWidth=2340`, `ReferenceHeight=1080`, `MatchWidthOrHeight=1.0`, `Configure(CanvasScaler)`).
- **`Assets/_Game/Bootstrap/SafeAreaFitter.cs`** — reusable `MonoBehaviour` that insets its `RectTransform`
  to `Screen.safeArea`, re-applying on safe-area / resolution / **orientation (L↔R) change**. Generalizes the
  proven inline `BattleHud.ApplySafeArea`.
- **`Assets/_Game/Bootstrap/UiNavigation.cs`** — the navigation shell: `UiScreen` (abstract base; subclasses
  build in `Build()`) + `UiRouter` (DontDestroyOnLoad screen-stack over one landscape overlay Canvas
  [sortingOrder 200], a `SafeAreaFitter` content root, CanvasGroup fade on unscaled time). **Lazy singleton —
  inert (never instantiated) until a screen is pushed**, so WP-00 adds **zero runtime footprint**. (Multiple
  MonoBehaviours per file follows the existing `AnimationManager.cs` precedent.)

### 2.2 Edits (surgical — 1 line each)
- **`UiFlow.cs:173`** — the 3 inline scaler lines → `UiScaling.Configure(scaler)`.
- **`BattleHud.cs:161`** — the 3 inline scaler lines → `UiScaling.Configure(scaler)`.
- *(Both previously: `1080×2400`, `matchWidthOrHeight 0.5` → now `2340×1080`, match **height** 1.0; and
  `screenMatchMode` is now set explicitly, a latent-correctness improvement.)*

### 2.3 ProjectSettings (orientation lock)
- **`ProjectSettings/ProjectSettings.asset`** — `allowedAutorotateToPortrait: 1→0`,
  `allowedAutorotateToPortraitUpsideDown: 1→0`. `LandscapeLeft/Right` remain `1`; `useOSAutorotation` remains
  `1`; **`defaultScreenOrientation` was already `4` (AutoRotation) and was NOT changed.** Net effect: the app
  auto-rotates **only between the two landscapes**, never portrait — the frozen requirement (D1).

## 3. Validation performed (what I actually did)
- **Structural sanity check (local):** brace balance OK (`UiScaling` 3/3, `SafeAreaFitter` 6/6, `UiNavigation`
  29/29); **no portrait constants remain** in `Assets/_Game/Bootstrap/*.cs`; `UiScaling.Configure` wired at all
  3 sites; 3 `.meta` GUIDs each unique (no collision). `git diff --stat`: 3 files changed (+4/−8) + 6 new
  files (3 `.cs` + 3 `.meta`).
- **Independent adversarial review (subagent, read-only):** verdict **PASS-WITH-NITS** — *no HIGH/MED
  findings; no compile blockers; no §12 violation (grepped clean of `Unity.Entities`/`EntityManager`/
  `EnqueueTrain`/`MatchState`/etc.); no runtime regression; lazy router confirmed truly inert.* It validated
  the trickiest points: AddComponent→Awake-before-Bind ordering (safe — base avoids `Awake`), the lazy-
  singleton/`Instance` getter interaction, `RequireComponent(RectTransform)` (RectTransform exists before the
  component is added), match-HEIGHT correctness for landscape, side-notch + L↔R safe-area behavior, and the
  orientation lock.
- **Repairs applied from review:** (B) added a `UiScreen.Build()` doc note instructing WP-01 authors to init
  in `Build()` not `Awake()`; (A) this report states the ProjectSettings change accurately (only the 2
  portrait flags). (C) EventSystem-creation style divergence left as-is (review judged it harmless/cleaner).

## 4. Validation PENDING (your pipeline — I cannot run these here)
- [ ] **Unity compile** (no Unity toolchain in this environment; this is C# against `UnityEngine` assemblies).
- [ ] **CI GREEN** (GitHub Actions — your pipeline; e.g. the run-ID flow seen on prior commits).
- [ ] **Android device validation** (Xiaomi Redmi Note 11R per your baseline): confirm the app launches
  **landscape**, is **locked** (no portrait rotation, rotates L↔R), and the existing UiFlow menu + BattleHud
  render at the new scale with **safe-area** clear of the side cutout. *(Existing portrait-tuned anchored
  positions in UiFlow/BattleHud will look off until those screens are re-laid-out in their own WPs — that is
  the expected, documented intermediate state, not a WP-00 defect.)*

## 5. §12 / binding-rules compliance
- **No ECS / sim / AI / economy / balance / combat / match-rule / commander / GATE-1-recovery / backend code
  touched.** Confirmed by the diff and the adversarial grep. Only uGUI presentation + one platform setting.
- **No scenes modified, no prefabs created, no canvases authored into scenes** (all UI remains code-built).
- **No gameplay changes.** No new content/balance.

## 6. Expected PASS criteria (for your explicit PASS)
WP-00 is **PASS** when your pipeline confirms: (1) project **compiles**; (2) **CI GREEN**; (3) on device the
app is **landscape-locked** (L↔R only) and **safe-area-correct**, and the existing UiFlow/BattleHud still run
(re-layout deferred to their WPs). On your PASS, I proceed to **WP-01 (Splash Screen)** — and not before.

## 7. Risks / notes carried forward
- The `UiRouter` shell is foundation; **WP-01 (Splash) is the first consumer** (`UiRouter.Instance.Show<Splash>()`).
- BattleHud keeps its existing inline `ApplySafeArea`; it will be reconciled onto the shared `SafeAreaFitter`
  when the **landscape Battle HUD** is built (program WP-12, "Match Layer").
- UiFlow's existing screens (Splash/Menu/Mode/End) remain portrait-laid-out until rebuilt in their WPs (01,
  04, 05, 13). This is intentional sequencing, not breakage.

---

**STOP — per the one-WP-at-a-time cadence.** WP-00 authored, self-validated (PASS-WITH-NITS, repaired).
Awaiting your **CI GREEN + Android validation + explicit PASS** before WP-01.

---

## ADDENDUM (added during WP-01) — UiRouter safe-area refinement
While building the first screen (WP-01 Splash), the `UiRouter` design in §2.1 was refined: instead of the
router owning a single SafeArea root that screens parent under, **safe-area handling moved to a per-screen
`SafeContent`** child (created in `UiScreen.Bind`), and screens are now **full-bleed** under the Canvas. Reason:
a full-screen background (splash key art) must extend *under* the side cutout, which a router-level SafeArea
root would have prevented. A `PopFaded()` fade-out transition was also added. This supersedes the "SafeArea
root" wording in §2.1 above; everything else in this report stands. Details + adversarial re-review (PASS-WITH
-NITS) in `WP-01_REPORT.md §3–§4`. This was an enhancement discovered during normal work, **not** a WP-00 CI
defect.


---

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


---

# WP-02 REPORT — LOADING SCREEN

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `LoadingScreen : UiScreen` (design `LoadingScreenDesign.png`) — full-bleed key art (`bg_menu` fallback), "LOADING" label, progress bar + percent. Fills over 1.6 s (presentation stub for "world/assets ready"; no real load gate under GATE-1), then `Replace<MainMenuScreen>`. In the boot flow it sits Login→Loading→MainMenu.
- **Files changed:** `Assets/_Game/Bootstrap/LoadingScreen.cs` (+`.meta`). Uses shared `UiWidgets`/`UiScreen`.
- **Validation performed:** structural (braces 5/5, parens 25/25; meta present; §12 grep clean — no ECS); batch adversarial review (independent subagent) covering this screen.
- **Review findings:** none specific to Loading; `_done` one-shot guard confirmed (no repeated `Replace`).
- **Repairs applied:** none required.
- **Remaining risks:** the fill duration is a stub (no real readiness gate yet — gated work is GATE-1/world-build); final loading art (`bg_loading`) pending (falls back to `bg_menu`).
- **Stable:** yes (pending CI/device compile run on your pipeline).


---

# WP-03 REPORT — LOGIN / AUTH

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `LoginScreen : UiScreen` (design `LoginAuthDesign.png`) — "WELCOME, WARRIOR" card with **PLAY AS GUEST** (→ Loading), social sign-in **placeholders** (Google/Facebook/Apple → notice + proceed as guest), and a Terms/Privacy line.
- **Files changed:** `Assets/_Game/Bootstrap/LoginScreen.cs` (+`.meta`).
- **Validation performed:** structural (braces 4/4; meta; §12 clean); batch adversarial review.
- **Review findings:** none specific. Confirmed no Services-assembly/backend call from Bootstrap (guest proceed is the stub path).
- **Repairs applied:** none.
- **Remaining risks:** real auth (`IBackendClient.AuthenticateAsync`) + Google/FB/Apple SDKs are a **GATE-3 / SDK integration** (deferred while GATE-1 recovery is active) — social buttons are placeholders by design.
- **Stable:** yes (pending CI/device).


---

# WP-04 REPORT — MAIN MENU / HUB

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `MainMenuScreen : UiScreen` (design `MainMenuDesign.png`) — currency chips (**Gold + Gems**, frozen model; no Energy), BULWARK wordmark, center stack (PLAY/CAMPAIGN/ONLINE BATTLE/CHESTS/STORE), right rail (QUESTS/UNITS/CLAN/LEADERBOARD/SETTINGS), bottom feature bar (DAILY/SPIN/FREE/EVENTS). Currency from `UiStub` (display-only); refreshed in `OnShow`. Built destinations route to their screens (PLAY→ModeSelect, STORE→Store, QUESTS→Quests, SETTINGS→Settings); destinations **not in this build sequence or ADR-gated/deferred** (Campaign/Online→ModeSelect; Chests/Spin→ADR notice; Units/Clan/Leaderboard/Daily/Free/Events→"coming soon") surface a toast — never dead/invented.
- **Files changed:** `MainMenuScreen.cs` (+`.meta`). Also relies on `UiStub`/`UiWidgets` (shared layer).
- **Validation performed:** structural (braces 4/4; meta; §12 clean); batch adversarial review (nav graph traced from hub).
- **Review findings:** loop-closure capture for rail/feature buttons verified correct; currency `OnShow` refresh confirmed.
- **Repairs applied:** none.
- **Remaining risks:** the legacy UiFlow menu sits beneath the shell (covered) — intentional migration state; Units/Clan/Leaderboard/Daily/Spin/Free/Events are out of this sequence (toasts); final hero/wordmark art pending.
- **Stable:** yes (pending CI/device).


---

# WP-05 REPORT — MODE SELECT

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `ModeSelectScreen : UiScreen` (design `ModScreenDesign.png`) — 5 cards **Classic / Campaign / Tournament / Endless / Online** (frozen relabel Missions→Campaign, Multiplayer→Online) + a **Commander** button (→ CommanderSelect). Per the freeze, Classic/Tournament/Endless launch the same skirmish (mode rules deferred) via `MatchPresentation.StartMatch`; Campaign/Online surface "coming soon" (their dedicated flows are out of this sequence). Back → Pop.
- **Files changed:** `ModeSelectScreen.cs` (+`.meta`).
- **Validation performed:** structural (braces 13/13; meta; §12 clean); batch adversarial review (card→handoff traced).
- **Review findings:** loop-capture of `mode`/`playable` verified; match handoff is via the presentation seam only (no gameplay change).
- **Repairs applied:** none.
- **Remaining risks:** mode differentiation (waves/rules) is gameplay-deferred (all playable cards launch the base skirmish); **replay is gameplay-blocked** (see WP-13 — sim supports one battle/launch); per-mode card art pending.
- **Stable:** yes (pending CI/device).


---

# WP-06 REPORT — STORE

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `StoreScreen : UiScreen` (design `StoreScreenDesign.png`) — shared shop tab bar (SPELLS/SKINS/CHESTS/STORE; only STORE built — others "not in this build sequence"), currency chips, Starter Bundle banner, **Battle Pass promo → BattlePassScreen**, the 5 gem-pack cards (`UiStub.GemPacks`), and Featured/Gems/Resources/Offers/Daily-Deals sub-tab labels. Purchases surface a "real-money store pending (GATE-3)" notice.
- **Files changed:** `StoreScreen.cs` (+`.meta`).
- **Validation performed:** structural (braces 9/9; meta; §12 clean); batch adversarial review.
- **Review findings:** tab-loop closure capture correct; frozen rule honored (no gems-buy-power item; gem packs are real-money, deterministic).
- **Repairs applied:** none.
- **Remaining risks:** live IAP/receipts = **GATE-3** (`ShopService`/`IapService` exist, stub today); Spells/Skins/Chests tabs are ADR-gated/out-of-sequence (notice).
- **Stable:** yes (pending CI/device).


---

# WP-07 REPORT — BATTLE PASS

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `BattlePassScreen : UiScreen` (design `BattlePassDesign.png`) — season header, current tier + XP bar, a Free/Premium tier track sampled around the current tier, and **UNLOCK PREMIUM** (stub gem spend → on success rebuild; on failure `UiModals.Insufficient`). Binds to `UiStub` (display-only).
- **Files changed:** `BattlePassScreen.cs` (+`.meta`). Wires the WP-11 `UiModals.Insufficient` reusable component.
- **Validation performed:** structural (braces 6/6; meta; §12 clean); batch adversarial review.
- **Review findings:** `Insufficient(needed)` arg is non-negative in the reached path and is clamped anyway (LOW, no action).
- **Repairs applied:** switched the failure path from a toast to `UiModals.Insufficient` (reusable-component integration).
- **Remaining risks:** real `BattlePassService` binding + server-authoritative claims/purchase = **GATE-3**; premium is cosmetic/convenience only (frozen — never power).
- **Stable:** yes (pending CI/device).


---

# WP-08 REPORT — QUESTS

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `QuestsScreen : UiScreen` (design `QuestsScreenDesign.png`) — Daily/Weekly sub-tabs (Weekly → "coming soon"), quest rows (title, progress bar, reward chip, CLAIM/CLAIMED/"…" state) from `UiStub.DailyQuests`, reset timer. CLAIM grants the reward to the stub wallet and refreshes.
- **Files changed:** `QuestsScreen.cs` (+`.meta`); `UiStub.cs` (added `Quest.Claimed`).
- **Validation performed:** structural (braces 7/7; meta; §12 clean); batch adversarial review.
- **Review findings:** **self-review caught a divide-by-zero** (original Claim zeroed `Target` → NaN fill on rebuild).
- **Repairs applied:** replaced the zero-Target approach with a `Claimed` flag + guarded progress fraction (`Target>0 ? … : 1f`); CLAIM now sets `Claimed` and `Replace<QuestsScreen>` to refresh. Struct-array element mutation confirmed legal.
- **Remaining risks:** real `QuestService` binding + server claims = **GATE-3**; claimed-state is in-session (display-only) until persistence lands.
- **Stable:** yes (pending CI/device).


---

# WP-09 REPORT — COMMANDER SELECT

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `CommanderSelectScreen : UiScreen` (design `CommanderSelectDesign.png`) — Iron Pact **WARDEN** vs Ashen **WARCHIEF**, each with active/passive ability cards, title, commander level, and **SELECT** (records the display-only choice + Pop). Mirrors canon `CommanderDef`×2 via `UiStub`. Reached from Mode Select (pre-battle).
- **Files changed:** `CommanderSelectScreen.cs` (+`.meta`); `UiStub.cs` (added `SelectedCommander`).
- **Validation performed:** structural (braces 5/5; meta; §12 clean); batch adversarial review.
- **Review findings:** none specific; `BuildCommander` closure capture (`n`) correct.
- **Repairs applied:** none.
- **Remaining risks:** real `CommanderDef`/`ProgressionService` binding (levels, ≤15% power budget) = **GATE-3**; portrait art pending.
- **Stable:** yes (pending CI/device).


---

# WP-10 REPORT — SETTINGS

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `SettingsScreen : UiScreen` (design `SettingsScreenDesign.png`) — left tab rail (General active; others "coming soon"), Audio panel with a **real SOUND mute toggle** (drives `AudioManager.ToggleMute`/`Muted`), placeholder volume row, in-session toggles (Vibration/Push/Battery), and LOGOUT/PRIVACY/RESET actions + version string.
- **Files changed:** `SettingsScreen.cs` (+`.meta`).
- **Validation performed:** structural (braces 9/9; meta; §12 clean); batch adversarial review.
- **Review findings:** mute toggle label-update via `GetComponentInChildren<Text>()` + a second `onClick` listener confirmed; per-toggle closure (`on`) correct.
- **Repairs applied:** none.
- **Remaining risks:** only mute is functional; volume/graphics/account/persistence are **placeholders** (a settings-persistence store is GATE-3) — labelled as such per the objective.
- **Stable:** yes (pending CI/device).


---

# WP-11 REPORT — REUSABLE COMPONENTS

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `UiModals` (designs `ConfirmModalDesign.png` 4-in-1 + `RewardGrantDesign.png` + `NetworkErrorDesign.png`) — **Confirm**, **Reward**, **Insufficient** (→ Store), **NetworkError** (Retry/Main Menu) as dim-scrim overlays that float over the current screen (built on the router canvas, not the screen stack, so the screen beneath stays visible but click-blocked). A lightweight **Toast** already lives on `UiRouter.Toast` (foundation). 
- **Files changed:** `UiModals.cs` (+`.meta`). Integrated into `BattlePassScreen` (Insufficient).
- **Validation performed:** structural (braces 12/12; meta; §12 clean); batch adversarial review.
- **Review findings:** `Scaffold(out GameObject, out Transform)` + scrim raycast-block confirmed; modals destroy their root on action.
- **Repairs applied:** none.
- **Note on order:** built alongside the screens that depend on it (Toast/Insufficient are used by earlier screens) — a dependency-driven reorder; the freeze foundation (WP-01 Navigation) groups these utilities in the shell, so this is consistent, not a skip.
- **Remaining risks:** none material (presentation-only). Reward/Confirm/NetworkError are available for wiring as their call sites are built (e.g. real IAP confirm at GATE-3).
- **Stable:** yes (pending CI/device).


---

# WP-12 REPORT — MATCH LAYER

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:**
  - `MatchIntroScreen` (design `MatchIntroDesign.png`) — VS framing (mode + Iron Pact vs Ashen + tip); auto-advance/tap → `MatchPresentation.Begin`.
  - `PauseModal` (design `PauseModalDesign.png`) — dim scrim over the frozen battlefield + RESUME/SETTINGS/SURRENDER; pausing toggles `Time.timeScale` (a permitted §12 control the legacy debug HUD already used — no rule change).
  - `MatchPresentation` (bridge) — sequences the shell around the **existing** battle: clears the shell so the battlefield + **existing BattleHud** show, calls `UiFlow.BeginMatchFromShell`, leaves a **safe-area in-match Pause overlay** (so BattleHud is left 100% untouched), and routes the end via the UiFlow delegation.
  - **Landscape Battle HUD:** the existing `BattleHud` already renders landscape via the shared `UiScaling` (WP-00) and is **unedited** (bindings untouched, per the objective); the Pause entry is added as a shell overlay rather than by modifying the HUD.
- **Files changed:** `MatchIntroScreen.cs`, `PauseModal.cs`, `MatchPresentation.cs` (+metas); `UiFlow.cs` (presentation-flow seams only: `Instance`, `BeginMatchFromShell`, `ReturnToMenuFromShell`, end-delegation when `RouterOwnsEntry`). **`BattleHud.cs` NOT edited** (no gameplay logic touched).
- **Validation performed:** structural (braces balanced; meta; §12 clean — only `Time.timeScale`/`PresentationState.InMatch` touched; outcome read by UiFlow read-only); batch adversarial review (full match-flow trace, timeScale coherence, pause overlay lifecycle).
- **Review findings:** defensive — `OnMatchDecided` did not `Clear()` the stack before showing the end screen.
- **Repairs applied:** added `UiRouter.Instance.Clear()` at the top of `OnMatchDecided` (mirrors `Surrender`).
- **Remaining risks:** the in-match Pause is a shell overlay (intentional, to avoid editing BattleHud); deeper landscape HUD re-layout (HP bars to corners) is optional polish — the HUD is functional landscape via the scaler. Match start/end coordinates with the legacy UiFlow orchestrator (kept as the proven sim-control owner).
- **Stable:** yes (pending CI/device).


---

# WP-13 REPORT — END FLOW (Victory / Defeat)

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS, 1 MED repaired); PENDING your CI/device.**

- **What was implemented:** `EndScreen : UiScreen` (designs `VictoryScreenDesign.png` + `DefeatScreenDesign.png`) — Victory (title + "enemy statue has fallen!" + **display-only** gem reward + match-time + reward-chest placeholder + CONTINUE) / Defeat (title + "your statue has fallen." + RETRY + CONTINUE). The outcome is the **real ECS `MatchState.Outcome`** (read read-only by UiFlow, which delegates to `MatchPresentation.OnMatchDecided` and sets `PendingVictory`) — or a presentation-only Surrender (Defeat). End stinger fires once in `OnShow`. CONTINUE → Main Menu; RETRY → see blocker.
- **Files changed:** `EndScreen.cs` (+`.meta`). (End delegation seam lives in `UiFlow.cs`, see WP-12.)
- **Validation performed:** structural (braces 5/5; meta; §12 clean — outcome consumed read-only; no sim write); batch adversarial review.
- **Review findings (MED):** **RETRY bounced straight back to the end screen** — the ECS `MatchState` persists the outcome, so re-entering a finished battle immediately re-resolves.
- **Repairs applied:** `MatchPresentation.OnRetry` now degrades to a notice + return-to-menu instead of re-entering the finished battle.
- **⚠️ KNOWN BLOCKER (GAMEPLAY, not UI) — documented, not implemented (per binding rules):** a true **rematch/replay** requires the ECS battle **world to reset** (`MatchState`→Ongoing, units/statues respawned). **The sim supports one battle per launch and exposes no reset.** This also means repeated PLAY after a finished match would re-resolve. Resolving it is a gameplay/sim feature (a world-reset / new-match API) that **must not** be added in this UI program. RETRY and replay therefore degrade gracefully until that gameplay feature exists. **Surfaced for the gameplay owners.**
- **Remaining risks:** the victory reward/time are display-only placeholders (reward economy + match-timer surfacing pending); replay blocked as above.
- **Stable:** yes for a single match end (pending CI/device); replay is gameplay-blocked.
