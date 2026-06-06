# BULWARK — Gameplay Recovery · Per-RC Validation Log (Phase D evidence)

Running evidence log for each RC fix (author → review → adversarial audit → CI → Android). The final
`GATE1_REVALIDATION_REPORT.md` (Phase E) aggregates the 20-match verdict. **Logic-only; no balance.** Telemetry
files under `runtime/device_validation/`.

---

## RC-1 — AI economy startup · **PASS** (commit `b4a0aba`)
- **Change:** `SquadAI.ApplyEcoAxis` defers combat enqueue while the side's miners (live + in-flight) are below the
  shared `BasicAISystem.TargetMiners` floor; `TargetMiners` promoted `private→internal`. Logic-only, no new literal.
- **Review:** 2-lens PASS (compile/ECS + correctness/no-balance), 0 blockers.
- **CI:** GREEN on re-run (attempt 2). **NOTE:** attempt 1 failed with an **il2cpp `Segmentation fault` (exit 139)**
  during `--convert-to-cpp` — a **toolchain flake** (managed compile tests passed on the same code; same il2cpp
  pipeline was GREEN on the prior commit). Re-running the failed job on the same commit → success. No code change.
- **Device (2 matches, `runtime/device_validation/rc1/`):** AI economy now starts.
  | Metric | Before (GATE-1 FAIL) | After RC-1 |
  |---|---|---|
  | AI gold (max) | **5** (flat, 390/390) | **89** (earns + spends, tracks player) |
  | AI miners (max) | **0** (0.0/10) | **3** (reaches the floor) |
  | AI army (max) | **1** | **4** (early parity/edge vs player's 3) |
  | AI miner #1 timing | never | within the opening (mA≥1 by ~t39 sample) |
- **Acceptance:** MET — `gA>5` and fluctuating; `mA≥1` (peaks at the floor 3); AI fields >1 unit.
- **Residual (expected → RC-2):** AI miners decay `3→0` by ~t78 (combat-targeting death + no replacement). This is
  the RC-2 defect, fixed next; RC-1's scope (economy *starts*) is complete.

## RC-2 — Miner survival · code-validated, device pending (commit pending)
- **Change:** `Targeting.cs` — added `.WithNone<MinerTag>()` to BOTH unit passes (1a candidate-bucketing + pass-2
  acquire), so miners are neither combat TARGETS nor ACQUIRE targets. Logic-only, no balance, no new literal.
- **Review:** 2-lens PASS, 0 blockers. One **HIGH** flagged (miner locomotion) — **investigated + resolved:**
  `MiningSystem` accrues gold **on assignment, position-independent** (Mining.cs:181-200 — assign nearest free node
  then accrue every tick; **no distance gate, no `MoveDestination`, no locomotion**). Miners never moved to nodes
  even pre-RC-2 (they walked toward combat yet still accrued — confirmed by RC-1's rising AI gold). So excluding
  miners from combat targeting does NOT break mining; they now stay off the line and keep accruing → economy
  sustains. (LOW: residual non-combat death vectors remain — e.g. spell AoE — acceptable; combat targeting was the
  primary vector. LOW: harmless capacity over-reservation.)
- **Decision:** ship the exclusion alone (the validated cause); add a player `MinerFloorSystem` ONLY if device shows
  player/AI miners still reaching 0. Mines never deplete (no remaining-yield field), so no mine-depletion despawn.
- **Acceptance to validate on device:** miner count never reaches 0 after the opening on either side (AI holds the
  floor; player holds the auto-demo's 2); no miner ever holds a combat target.
