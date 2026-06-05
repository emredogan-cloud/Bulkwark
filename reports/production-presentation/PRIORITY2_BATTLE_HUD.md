# BULWARK — Production Presentation · Priority #2: Battle HUD

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 **Option A** — NOT roadmap Phase 5).
**Build:** `b35ff98` (CI run 27020141998, **GREEN**). **Method:** author → 2-lens independent review + adversarial
audit → repair → CI/CD GREEN → (Android device capture deferred — owner cannot connect the phone right now).
**Inviolable:** presentation/§12-control layer only; no gameplay-rule/balance/AI/economy/catalog/canon change;
deferred GATE-1 bugs untouched; GATE 1 (FUN) / GATE 5 remain open/binding.

---

## Objective
Priority #2: a **proper, professional in-match HUD** — a gold indicator and troop-production buttons drawn on
screen (plus statue-HP bars), replacing the IMGUI debug overlays during a match.

## What was built (`BattleHud.cs`, new — code-built uGUI, removable)
Shown only during the uGUI Match screen (`PresentationState.InMatch`, set by `UiFlow`):
- **Gold indicator** — gold icon (placeholder sprite) + live amount (top-left chip).
- **Statue-HP bars** — two filled bars, **Iron Pact (blue)** and **Ashen Horde (red)**, each filling to
  `Health / MaxHealth` (top-center) with the faction name + value.
- **Troop-production buttons** — built from the **live `UnitCatalog` roster** (one per unit), each showing the
  role + gold cost, **affordability-gated every frame** (disabled + greyed when unaffordable; fail-safe before gold
  is read). A bottom row, auto-laid-out.
- **Unit count** readout + an **ADVANCE** (attack-move) button.
- IMGUI debug overlays (`SimDebugOverlay`/`SimPlayerHud`) are now hidden throughout the front-end (replaced
  on-screen; observability remains via logcat). The debug *systems* still run (the V2 auto-demo still drives a match).

## Control boundary (the binding rule — verified, no gameplay change)
`BattleHud` reads the ECS world **read-only** for display (`GoldStore`, `StatueTag`+`StatueState`, `UnitTag`+`Team`,
the `UnitSpawnStats` roster) and, on a button press, writes **only player INPUT data the systems already consume** —
`Training.EnqueueTrain` (append a `TrainOrder`; the existing `TrainingSystem` still pays the data-driven cost +
spawns) and `MoveDestination` (attack-move, miners excluded). The review confirmed this write set is **byte-for-byte
identical to the proven `SimPlayerHud`** — **no balance/cost/AI/economy/catalog/canon mutation, no new gameplay rule.**

## Validation
- **CI/CD:** GREEN — IL2CPP Android build + EditMode/PlayMode compile tests pass on `b35ff98`.
- **Independent review + adversarial audit (2 lenses, PASS, 0 blockers):**
  - *Compile* — UnityEngine.UI (Canvas/CanvasScaler/GraphicRaycaster/Image/Button/HorizontalLayoutGroup/Shadow/Text,
    `Image.Type.Filled`) resolves; every ECS read/write member cross-checked against `SimPlayerHud`; braces/parens balanced.
  - *Control-boundary safety* — only `EnqueueTrain` + `MoveDestination` writes (grep-confirmed no other
    `SetComponentData`/`AddComponentData`/`CreateEntity`/`DestroyEntity`); all display reads read-only + leak-free
    (`using` + `Allocator.Temp`); crash-safe (world/roster guards, `MaxHealth>0`, `try/catch`, null-sprite fallback);
    miners excluded from Advance; no soft-lock / no double-input hazard (auto-demo + manual both only append orders).
  - Two non-blocking notes applied: BattleHud now creates its own `EventSystem` if absent (self-sufficient input),
    and the unit-count glyph is ASCII (LegacyRuntime font safety).
- **On-device (Android):** **deferred at owner's request** (device not connectable now). No screenshots fabricated.
  When the device is available: install `b35ff98`, enter a match, and confirm the gold chip updates, both HP bars
  drain, and the troop buttons train (greying when unaffordable). The HUD reads/writes via the same path
  `SimPlayerHud` already proved on device.

## Performance / size
- **No per-frame ECS cost** beyond the small read queries already done by the debug HUD; one extra overlay canvas.
- **APK:** 39.34 → **39.40 MB (+0.06 MB)** — code only, no new art (reuses the existing gold/button placeholders).

## Remaining gaps (later priorities)
- The HUD uses the built-in font + placeholder button/gold sprites (a 9-sliced UI kit + TMP font is later polish).
- The V2 auto-demo still trains/advances alongside the manual buttons (left intact — removing it is a behavior
  change, out of scope here).
- Next in owner order: **#3 audio framework**, #4 VFX, #5 animation, #6 polish.

## Verdict
The professional in-match HUD — **gold indicator, statue-HP bars, and troop-production buttons** — is
**implemented, CI-GREEN, and review-validated** within the established control boundary (**no gameplay change**;
GATE 1/GATE 5 remain open/binding; not roadmap Phase 5). On-device screenshot capture is the only deferred item
(device unavailable). Stopping at this checkpoint for owner direction (Priority #3 = audio, and/or device capture
once the phone is connectable).
