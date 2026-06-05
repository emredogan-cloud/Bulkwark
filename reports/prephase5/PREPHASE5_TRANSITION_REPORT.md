# BULWARK — Pre-Phase-5 Transition Report

**Date:** 2026-06-05 · **Purpose:** close the temporary pre-Phase-5 GATE-1 validation track (V0→V1→V2) and
state what was learned, what works, and what remains **before real art production**. **NOT Phase 5.** No
roadmap/canon/balance change. **Authority:** `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` (§13 GATE 1, §12,
§15). **Inputs:** all `reports/prephase5/*` + the device logs/screens in `runtime/device_validation/`.

---

## 1. What this track did (V0 → V1 → V2)
| Phase | Goal | Outcome |
|---|---|---|
| **V0 Visualization** | make the sim visible | ✅ `SimProxyRenderer` — ECS entities render as team-colored primitives (blue/red capsules, yellow cubes, gray cylinders), camera-framed. `PHASE_V0_REPORT.md`. |
| **V1 Playability** | first playable RTS loop | ✅ `SimPlayerHud` — gold/unit-buttons/queue + player training + attack-move → **first observable combat** (a unit damaged the enemy statue, a kill). `PHASE_V1_REPORT.md`. |
| **V2 GATE-1 validation** | real FUN verdict | ✅ full match resolved (economy→army→push→topple→**Victory**); **GATE 1 = FAIL** (AI collapse → walkover). `GATE1_VALIDATION_REPORT.md`. |
| (parallel) | placeholder-art plan | ✅ `PLACEHOLDER_ART_INTEGRATION_PLAN.md` (advisory). |

Preceded by the diagnostic chain that unblocked everything: `SIMULATION_PROOF_REPORT` (sim runs but world empty)
→ `BOOTSTRAP_EXECUTION_REPORT` (BattleBootstrap was baked inside a SubScene → never ran; moved to MainScene root).

## 2. What was LEARNED (the big insight)
**The ECS simulation was correct all along — every blocker was wiring or missing control input, not broken sim
logic.** In order of discovery:
1. `BattleBootstrap` lived inside the `BattleEnvironment` **SubScene** → DOTS baked it, so its `Start()` never ran
   → empty world. *(Fixed: moved to MainScene root.)*
2. No **render layer** existed → nothing visible. *(Fixed: removable `SimProxyRenderer` primitives.)*
3. Units **never advanced** without a target (MovementSystem has no march-when-idle fallback; squad
   `FormationMember` wiring is unterminated) → no combat. *(Bridged: control-layer attack-move.)*
4. The **player never trained a miner** (mining sim auto-assigns correctly — it just had no miner) → no income.
   *(Fixed: auto-demo trains miners first.)*
5. The **AI economy collapses** (over-queues miners, counting bug) → 1 unit → walkover. *(Identified; fix
   pending — the GATE-1 FAIL blocker.)*

The victory/defeat chain, counter matrix, mining math, training/queue, and targeting were all already implemented
and correct.

## 3. What WORKS now (device-proven)
- Bootstrap builds the world; **33 ECS systems** tick; app installs/launches/runs at 30 fps on the Redmi Note 11R.
- **Economy:** miners auto-assign to mines; gold scales with occupancy (occ 1→4); spend/earn cycle legible.
- **Training + queue:** FIFO, data-driven cost/time, affordable-gating + honest stall display.
- **Combat:** movement-to-range, type×armor chain, deaths, statue damage; **a 9-unit army toppled the enemy
  statue → Victory → sim freeze** (real combat, no probe).
- **Rendering/observability:** team-colored primitive proxies + IMGUI overlay (MatchState/Gold/Units/Miners/
  Statue) + player HUD.

## 4. What REMAINS before real art production
**Immediate (to clear GATE 1):**
1. **Fix the AI economy bug** (`BasicAI.BiasTraining` — count in-flight miners; ~1 line, no balance change) → re-run
   for a *contested* match → **re-gate GATE 1** (target: PASS on a real fight).
2. **Device-confirm Defeat** (flip `SimAiDriver.ProbeTargetTeam`, or let a fixed AI win) — Victory already proven.

**Before/within real art production:**
3. **Proper unit movement** in the sim: terminate the squad→`FormationMember`→`FormationSystem` pipeline (stamp
   `FormationMember` at spawn) so units advance by design, retiring the control-layer advance drivers.
4. **Placeholder art** per `PLACEHOLDER_ART_INTEGRATION_PLAN.md`: a sibling `SimSpriteRenderer` swapping primitives
   for CC0 2D sprites (statue 4-phase first) — presentation-only, no sim change, no `entities.graphics`.
5. **ADR decision (human gate):** canon **2D-Spine** vs the asset audit's **2.5D-3D** — blocks production unit art.
6. **Production render pipeline:** add `com.unity.2d.sprite`/Spine (or resolve `entities.graphics`) with a clean
   CI re-resolution; per-archetype visual id stamped at spawn (data-only).
7. **Retire the debug scaffolds** once real UI/render exist: `SimProxyRenderer`, `SimDebugOverlay`, `SimPlayerHud`,
   `SimAiDriver`, and the `[BOOTSTRAP]` logs are all temporary, read-only/control-only, and **removable by deleting
   their files** (the §12 guarantee).

## 5. Status of the temporary scaffolds (all removable, no balance/rule change)
| File | Role | Removal |
|---|---|---|
| `Assets/_Game/Bootstrap/SimProxyRenderer.cs` | primitive rendering (V0) | delete file |
| `Assets/_Game/Bootstrap/SimDebugOverlay.cs` | stats/radar overlay | delete file |
| `Assets/_Game/Bootstrap/SimPlayerHud.cs` | player HUD + economy auto-demo (V1/V2) | delete file |
| `Assets/_Game/Bootstrap/SimAiDriver.cs` | AI advance + victory probe (V2) | delete file |
| `[BOOTSTRAP]` logs in `BattleBootstrap.cs` | forensic instrumentation | remove log lines |
| `BattleBootstrap` in MainScene root | **permanent fix** (correct placement) | keep |

## 6. Bottom line
The decade-proven core loop **lives and runs on device**: mine → train → push → topple the statue → win. The
remaining work to a fun, shippable GATE-1 is **small and specific** (fix the AI economy, then re-gate), followed
by the art pipeline (placeholder → ADR → production). The sim spine is sound; what's left is opponent quality and
presentation.

**STOPPING at the end of V2 per instruction. Do NOT start Phase 5. No roadmap/canon/balance change.**
