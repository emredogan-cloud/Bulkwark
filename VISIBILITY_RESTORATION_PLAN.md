# BULWARK — Visibility Restoration Plan

**Date:** 2026-06-04 · **Goal:** convert simulation status from **UNDETERMINED** → **PROVEN RUNNING** or
**PROVEN BROKEN** with on-device evidence, by restoring observability. **Authority:** roadmap §12 (ECS
boundary; MonoBehaviour for control/render), §15 (no gameplay/balance change). **No Phase 5/6; no roadmap edit.**
**Constraint:** the observability layer is **temporary, removable, debug-only — it does NOT alter gameplay
logic or balance** (it only READS ECS state and DRAWS it).

## 1. Why the screen is brown *(from DEVICE_RUNTIME_VALIDATION_REPORT.md — proven)*
The app launches, il2cpp runs, and the ECS battle SubScene/entities load — but **nothing is rendered**:
- `Packages/manifest.json` has `com.unity.entities` + URP **but not `com.unity.entities.graphics`** → baked ECS entities have **no render bridge** and are never drawn.
- `MainScene` + `BattleEnvironment` contain **0 SpriteRenderer / MeshRenderer / Canvas / Light** — only a Camera + ECS authoring.
- The Camera renders an empty world → a flat dark-brown frame. The brown is **empty-camera output, not game content and not a sim indicator.**

## 2. Why the simulation cannot be observed
- The BULWARK sim is **ECS `ISystem`s** (Mining, Training, Combat, SquadAI, MatchFlow, …) that **emit no `Debug.Log`** and have **no rendered representation**. So from outside the process there is **nothing to see and nothing in logcat** → status is genuinely UNDETERMINED (neither confirmable nor refutable from the prior capture).
- The battle is built at runtime by `BattleBootstrap` (a MonoBehaviour) into the default ECS World (GoldStore, statues, mines, unit catalogs, `AICommander`, `MatchState`), and units spawn over time via `TrainingSystem`. **All of this is observable by READING the World** — we just have no reader.

## 3. How visibility will be restored (the approach)
Add ONE removable debug MonoBehaviour — **`Assets/_Game/Bootstrap/SimDebugOverlay.cs`** (assembly
`Bulwark.Bootstrap`, which is guaranteed in the build because `BattleBootstrap` is wired into `MainScene`,
and already references `Unity.Entities`/`Collections`/`Mathematics`/`Bulwark.Sim`/`Bulwark.Data`). It:
- **Auto-spawns** via `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` → a `DontDestroyOnLoad` GameObject. **No scene edit, no prefab, no art.** Deleting the one file fully removes it.
- **READS the default ECS World every frame** (read-only `EntityQuery` + `ToComponentDataArray`; never writes/mutates sim state) and builds a snapshot: `MatchState`, per-team `GoldStore`, unit counts (total / per-team / alive), miner count, `MineNode` occupants, per-team `StatueState` HP/phase, `AICommander` stance, `TrainOrder` queue lengths, engaged-unit count (`AttackState.Target != Null`), plus frame-to-frame **deltas** (Δalive = spawns/deaths, Δgold, Δtotal-health = combat).
- **Emits evidence two ways, both independent of the missing render layer:**
  1. **`Debug.Log` snapshot ~1×/sec** (tag `Unity` in logcat) — a `[SIMPROOF]` line with all values → **logcat evidence even if nothing draws.**
  2. **`OnGUI` overlay** — a stats panel (FPS, MatchState, Gold, Units, Miners, Mines, Statues, AI stance, Queue, Combat deltas) + a self-projected **"radar"**: colored boxes for each entity (**Iron Pact/Team 0 = blue, Ashen Horde/Team 1 = red, Mine = yellow, Statue = gray**), positioned by mapping `Position` into a GUI viewport (no camera/URP needed) → **on-screen proof + the temporary visual proxies (STEP 4)**.

### Why IMGUI radar instead of `com.unity.entities.graphics` (STEP 3 investigation)
- **Investigated:** `entities.graphics` was previously flagged **unresolvable** in CI (`FIRST_COMPILE_REPORT.md` §6 / commit `c60d562`). Re-adding the package **alone renders nothing** — it requires authoring **`RenderMeshArray` + `MaterialMeshInfo` render components on every unit/statue/mine entity** at bake/spawn, which is a change to the **sim/authoring code** (larger scope + gameplay-adjacent) and re-introduces the resolution risk that previously turned CI red.
- **Decision:** for proof-of-life, use the **IMGUI overlay + radar** (camera/URP/art-independent, removable, zero sim change) — this is exactly the owner-sanctioned STEP-4 proxy fallback ("before real art exists"). Full ECS rendering via `entities.graphics` (+ render components, a 2D/Spine sprite bridge, and a framed camera) is the **production rendering task**, deferred to when art is implemented; the exact steps are recorded in the final report.

## 4. Verdict criteria (evidence-based)
- **PROVEN RUNNING** ⟺ the snapshot shows the sim *advancing over time*: e.g., gold changes, units spawn (count rises) and/or die (alive falls), statue HP changes, `AICommander` active, and/or `MatchState` transitions — observed in **both** logcat and on-screen.
- **PROVEN BROKEN** ⟺ the World is empty / `BattleBootstrap` failed (e.g., `[BattleBootstrap] … unassigned` errors, no GoldStore/statues/catalogs) or all values are **static** across the whole capture (no spawns, no gold change, MatchState never moves) → the core is not simulating.

## 5. Device validation loop
Build (CI) → push → wait for GREEN → install on the connected Redmi Note 11R → `logcat -c` → launch → capture
`full_logcat.txt` (grep `SIMPROOF`) + screenshots of the overlay/radar → record evidence → **`SIMULATION_PROOF_REPORT.md`** with the explicit verdict. **Stop after the report.**
