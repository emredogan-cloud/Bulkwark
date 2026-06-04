# BULWARK — Pre-Phase-5 Visualization Roadmap (temporary GATE-1 validation track)

**Date:** 2026-06-04 · **Status:** TEMPORARY validation track to obtain a real **GATE 1 (FUN)** verdict.
**This is NOT Phase 5.** No roadmap edit, no canon change, no monetization work, no balance change beyond what
a real fun verdict requires (debug-tier only). **Authority unchanged:** `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`
(§13 Phase 1 GATE 1 = "is the combat fun?"), §12 (ECS sim / MonoBehaviour presentation boundary), §15.

## Why this track exists (proven state)
| Proven (evidence) | Source |
|---|---|
| Bootstrap executes; world is created (Gold/Mines/Statues/AICommander); units can spawn; AI decides+spends+queues | `BOOTSTRAP_EXECUTION_REPORT.md` (verdict was NOT_EXECUTING → fixed by moving Bootstrap to MainScene root) |
| ECS systems run (33); sim advances over time | `SIMULATION_PROOF_REPORT.md` |
| Compile/tests/APK GREEN; app installs+launches on device (Redmi Note 11R) | `DEVICE_RUNTIME_VALIDATION_REPORT.md`, CI |
| Monetization shell fair (GATE 4 static PASS) | `reports/phase-4/FINAL_REPORT.md` |
| **No real entity rendering** (no entities.graphics; scenes have no renderers) → game not yet observable/playable enough for a FUN verdict | `SIMULATION_PROOF_REPORT.md` |

**Gate status:** GATE 1 **OPEN** (no on-device fun verdict — nothing visible/playable yet) · GATE 2/3 DEFERRED ·
GATE 4 PASS (static) · GATE 5 DEFERRED. **This track targets GATE 1 only.**

## Method (every phase)
`author → independent review → adversarial audit → repair → CI/CD GREEN → on-device validation → report`
(the same pipeline used for Phase 4 / the overlay / the bootstrap fix). All viz/HUD is **debug-tier, removable,
read-only of the ECS sim, and changes no gameplay logic or balance** (§12/§15). No production art.

---

## PHASE V0 — VISUALIZATION  *(execute now; stop after its report)*
**Goal:** make the simulation visible. **Approach:** a removable presentation MonoBehaviour
(`SimProxyRenderer`) that reads the ECS world read-only and renders **primitive proxies** (the §12 presentation
layer, no sim change). *(Entities.Graphics evaluated — see "Entities Graphics evaluation" below — and
**deferred** in favor of GameObject proxies: lower risk, no per-entity render-component authoring, no package
re-resolution.)*
**Deliver:** primitive rendering + temporary URP materials + team colors + health/spawn/combat visualization +
a camera framed on the front.
- **Player units (Team 0): blue capsules** · **AI units (Team 1): red capsules** · **Mines: yellow cubes** ·
  **Statues: gray cylinders.**
- **Health viz:** proxy scale/tint by HP fraction. **Spawn viz:** proxy appears on spawn. **Combat viz:**
  damage flash + shrink; proxy destroyed on death; statue cylinder shrinks/tints as it takes damage.
- **No art assets, no final models, no animation, no monetization.**
- **Exit:** the player can visually see units (and the world) rendered, moving to the extent the sim moves them.

## PHASE V1 — PLAYABILITY  *(do NOT start automatically)*
**Goal:** the first playable RTS loop. **Deliver (debug-style UI, no final art):** unit-production buttons,
Gold display, train-queue display, pause button, a basic HUD, touch-controls verification, **player-side
training** (so the player can field the opposing force → real combat), and a match-start flow.
**Exit:** the player can train units, spend gold, watch units move, and observe combat.

## PHASE V2 — GATE 1 VALIDATION  *(do NOT start automatically)*
**Goal:** a real FUN verdict — **PASS or FAIL, no DEFERRED, no ambiguity, evidence required.**
**Deliver:** device playtests; balance / combat / economy / AI observations; victory/defeat verification.
**Generate `GATE1_VALIDATION_REPORT.md`** with the verdict.
**Exit:** GATE 1 receives a real PASS/FAIL verdict.

## After V2
Do **not** begin Phase 5. Generate **`PREPHASE5_TRANSITION_REPORT.md`**: what was learned, what is working, and
what remains before real art production.

---

## Entities Graphics evaluation (V0 deliverable)
- `com.unity.entities.graphics` was previously flagged **unresolvable** in CI (`FIRST_COMPILE_REPORT.md` §6 /
  commit `c60d562`); re-adding it risks a red build.
- It renders nothing on its own — it requires **`RenderMeshArray` + `MaterialMeshInfo` authored on every unit/
  statue/mine entity at bake/spawn**, i.e. a change to the **sim/spawn code** (gameplay-adjacent, larger scope).
- **Decision:** use **GameObject primitive proxies** (a read-only presentation MonoBehaviour) for V0 — robust,
  §12-clean, no sim change, no package re-resolution. Full ECS rendering (entities.graphics or an ECS→sprite/
  Spine bridge) is the **production-art task**, deferred to after this GATE-1 track / the real art pass.

## Scope guardrails (binding for this whole track)
No roadmap/canon edits · no monetization/IAP work · no new currencies · no commander-budget change · no
faction/unit/spell/map additions · all viz/HUD is debug-tier + removable + read-only + gameplay-neutral. Stop
points are honored exactly (stop after V0's report; V1/V2 only on explicit go-ahead). Phase 5/6 NOT begun.
