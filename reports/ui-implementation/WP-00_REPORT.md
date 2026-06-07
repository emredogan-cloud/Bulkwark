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
