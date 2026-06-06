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

## RC-2 — Miner survival · **PASS** (commit `9aa1dde`, CI GREEN)
- **Device (2 matches, `runtime/device_validation/rc2/`):** miner attrition FIXED.
  | Metric | Before (RC-1) | After RC-2 |
  |---|---|---|
  | AI miners held | 3 → **0 by t78** | **3, entire match** (min 2, max 3) |
  | Player miners held | → 0 | **2, entire match** |
  | AI army (max) | 4 | **7** (a real, scaling opponent) |
  | AI gold | 89 then starved | **sustained** (spent down on the army) |
- **Acceptance:** MET — neither side's miner count reaches 0 after the opening; no `MinerFloorSystem` needed (the
  exclusion alone sustains both economies; mines don't deplete). Device install fought an MIUI USB-install
  restriction (`INSTALL_FAILED_USER_RESTRICTED`) — device-side flake, succeeded on retry.

## RC-3 — Combat acquisition + metric · **PASS** (commit `9e3e807`, CI GREEN)
- **Change:** (a) `Combat.cs` MovementSystem override branch now HALTS an attack-move when `Targeting.Current` is
  within the unit's existing `AttackState.Range` (CombatSystem then swings); (b) re-pointed the engaged metric in
  `GateTelemetry`/`SimDebugOverlay` from the never-written `AttackState.Target` to `Targeting.Current` pointing at
  an enemy UNIT. Logic-only, no balance.
- **Review:** correctness/no-balance PASS; compile/ECS caught 1 CRITICAL (a prior failed edit left `_qEngaged`
  defined as `UnitTag+AttackState` but consumed as `Targeting`) → FIXED.
- **Device (3 matches, `runtime/device_validation/rc3/`):** real unit-vs-unit combat now occurs.
  | Metric | Before | After RC-3 |
  |---|---|---|
  | Engaged (unit-vs-unit) | **0 / 390** | **>0 — 27 combat samples / 3 matches** (peak 4/side) |
  | Statue HP at t=78 | chipped (units walked past) | **1000/1000** — units halt + fight each other first |
- **Acceptance:** MET — `Engaged>0` when armies meet; a front line forms (statues no longer chipped early).
- **Residual (→ RC-4/RC-5):** matches still end at ~t116 by the probe; statues now untouched at t=78 (units tied up
  fighting). Removing the probe (RC-4) + organic march of survivors (RC-5) makes the *win* organic next.

## RC-4 — Probe removal · code+review PASS (commit `ed6040d`)
- **Change:** deleted the `SimAiDriver` victory-latch probe entirely; matches resolve ONLY via real statue
  destruction (Combat→StatueDamageInbox→StatueDamageSystem→MatchState→MatchFlow, verified intact). Logic-only.
- **Review:** PASS, 0 blockers; documented stalemate risk (no timeout fallback by design — watched in Phase E).
- **Validation:** folded into the RC-5/6 final build + the 20-match revalidation (its isolated device build was
  cancelled to stay within the 2-build budget; RC-4 code ships + is exercised in the final build).

## RC-5 — Organic march + agency · RC-6 — Pacing · code+review PASS (final build)
- **RC-5 change:** `Combat.cs` MovementSystem — when a unit has no acquired target it now ORGANICALLY marches
  toward the ENEMY statue at its own MoveSpeed (StepToward, stopDist=AttackState.Range; miners excluded; statue
  anchors pre-scanned). Both scaffold advances GATED (`SimAiDriver.UseScaffoldAdvance=false`,
  `SimPlayerHud.UseAutoAdvance=false`); auto-TRAIN kept so the unattended campaign still builds armies; the player
  may still ADVANCE manually via the HUD (agency). Logic-only, no new constant.
- **RC-6:** subsumed — units advance from spawn (less dead time); further pacing (march speed / spawn / map) is
  BALANCE → DEFERRED.
- **Review:** 2-lens PASS, 0 blockers — Burst-safe (same multi-query pattern as Targeting/Mining), correct march
  direction, no balance, no no-movement deadlock (statues always exist).
- **Validation:** the 20-match Phase-E revalidation (`GATE1_REVALIDATION_REPORT.md`) runs on this build.

### RC-2 detail (original analysis)
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
