# BULWARK — Production Presentation · Priority #5: Animation Framework Report

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 **Option A** — NOT roadmap Phase 5).
**Build:** `268763f` (CI run 27030674626, **GREEN**). **Method:** discovery → author → independent review →
adversarial audit → repair → CI/CD GREEN → (Android device capture deferred — owner request).
**Inviolable:** presentation-only, **NO gameplay dependency, ZERO ECS writes**; no balance/AI/economy/commander-
budget/monetization/canon change; **GATE 1 FAIL / GATE 2 / GATE 3 / GATE 5 all preserved**; deferred GATE-1 bugs
untouched. **Companion:** `PRIORITY5_ANIMATION_DISCOVERY.md` (Phase A).

---

## Objective
Transform BULWARK into an **Animated RTS** — units that breathe, walk, recoil, and fall — without touching gameplay.

## Phase A — Discovery (done)
Skeletal **Spine** rigs exist (Stick War 4.1 / Stickman 3.8 / Age of War 2.x) but are **DEFERRED** (paid
`spine-unity` runtime not in the manifest; version-split sets can't share one runtime). There are **no baked unit
frame-sheets** (units are Spine-composed per-part). So the achievable approach is **procedural transform
animation** of the existing static archetype sprites — **zero new assets, no paid runtime.** Spine remains the
future production path. See `PRIORITY5_ANIMATION_DISCOVERY.md`.

## Phase B — Architecture (`AnimationManager.cs`, new, removable)
- **`SpriteAnimator`** (per-unit-proxy MonoBehaviour) — in **LateUpdate** it reads the renderer's per-frame BASE
  transform and applies a procedural modulation: **idle** breathe + gentle bob; **walk** bounce (hops up) + slight
  lean, when the proxy is moving; **hit recoil** (a brief scale punch via `NotifyHit`); **death** fall — sink +
  topple + shrink + alpha-fade over 0.45 s via `PlayDeath`, then self-destruct.
- **`AnimationManager`** — auto-spawned singleton; `Attach(proxy, sprite)` factory.
- **No accumulation/drift:** the renderer re-establishes the clean base every `Update`, the animator modulates in
  `LateUpdate` (Unity runs all `Update`s before any `LateUpdate`); rotation is owned solely by the animator.

## Phase C — Integration (read-only; never drives gameplay)
Driven entirely from `SimProxyRenderer`'s read-only observations:
| Observed state (read-only) | Animation |
|---|---|
| proxy position changed (speed > threshold) | **walk** bounce + lean |
| stationary | **idle** breathe/bob |
| HP-drop (the existing damage event) → `NotifyHit` | **hit recoil** |
| unit cull → `PlayDeath` | **death** fall/topple/fade, then destroy |
Attached to **unit proxies only** (statue/mine keep their HP-driven scale + VFX). A lazy-attach guard covers any
boot-order race. **No ECS writes, no sim hook.**

## Phase D — Validation
- **Independent review + adversarial audit (2 lenses, PASS, 0 blockers):**
  - *Compile* — only `using UnityEngine;` needed; both classes resolve; renderer hooks type-check; null-guards
    correct; braces/parens balanced.
  - *Performance / Phase-D* — **presentation-only (zero ECS writes)**; transform ordering correct (no drift, first-
    frame + dt-guard sound); **death lifecycle leak-free & crash-free** (proxy removed from the live set; PlayDeath
    vs Destroy mutually exclusive; `Time.unscaledTime` guarantees the tween reaches `Destroy`; death SFX/VFX still
    fire); **mobile-safe** (~50 cheap `LateUpdate`s, value-type math, **zero per-frame GC**); fixed to one applied
    LOW (lazy-attach hardening).
- **CI/CD:** **GREEN** — IL2CPP Android build + EditMode/PlayMode compile tests pass on `268763f`.
- **APK size:** **40.62 MB (unchanged)** — animation is fully procedural (no new assets).
- **On-device (Android):** **deferred at owner's request** (phone not connectable). No animation "evidence"
  fabricated. When available: install `268763f` and confirm idle breathing, walk bounce, hit recoil, and death falls.

## Remaining gaps (later)
- **Skeletal (Spine) animation** — the production path; needs a runtime-licensing ADR + version reconciliation.
- **Attack/cast** are approximated by the hit recoil + VFX; true attack swings/cast poses need skeletal rigs (or
  baked frames). `cast` API stub noted for a future spell-event hook.
- **Statue/mine** animation (phase sprites/occupancy) not added (HP-driven scale + VFX cover damage feedback).
- Static sprites are still © ripped placeholders — replace before release.

## Verdict
The first **animation framework** — procedural idle/walk/hit-recoil/death on the unit sprites — is **implemented,
review-validated, and CI-GREEN**, fully **presentation-only with no gameplay/ECS impact** (GATE 1 FAIL and the
deferred gates all preserved; not roadmap Phase 5). The battlefield now has motion to match the audio + HUD + VFX.
The only deferred item is on-device confirmation (phone unavailable).

**STOPPING after Priority #5 per instruction.** (Priority #6 — presentation polish — authorized separately by the
owner; proceeding next.)
