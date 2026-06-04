# BULWARK — Bootstrap Execution Forensics Report

**Date:** 2026-06-04 · **Question:** why is the ECS world never created (Gold=-1/Units=0/AI=0 forever)
despite BattleBootstrap existing with refs assigned? **Rule honored:** evidence only. **Device:** Xiaomi
Redmi Note 11R, Android 13, arm64-v8a. **Artifacts:** `runtime/device_validation/bootstrap_logcat.txt`,
`screen_bootstrap_{15,38}s.png` (gitignored, local).

---

# ⛔ VERDICT: BOOTSTRAP_NOT_EXECUTING  →  FIXED & VALIDATED
**Original state:** `BattleBootstrap` **did not execute** — its GameObject was inside the `BattleEnvironment`
**SubScene**, which DOTS **bakes to entities at build time and never instantiates as a live MonoBehaviour**;
with no Baker, its `Start()` (the code that builds the entire battle world) **never ran** → empty world.
**Fix (smallest):** moved the Bootstrap GameObject out of the SubScene to **MainScene root**. **On-device
re-validation proves it now executes and creates the world.**

---

## 1. Does BattleBootstrap execute?
- **Original (in SubScene): NO.** Zero `[BattleBootstrap]`/`[BOOTSTRAP]` log lines across the prior 30 s
  capture (`SIMULATION_PROOF_REPORT.md`), and the world was empty — yet the 33 ECS systems ticked. A
  SubScene-baked MonoBehaviour without a Baker never gets `Awake/OnEnable/Start`.
- **After the fix (in MainScene root): YES — proven on device.** The full lifecycle fired:
  ```
  [BOOTSTRAP] Awake — BattleBootstrap is a LIVE MonoBehaviour (not baked).
  [BOOTSTRAP] OnEnable.
  [BOOTSTRAP] Start — beginning world build.
  ```

## 2. Does it create the world?
- **Original: NO** (Gold=-1, Units=0, Mines=0, Statues=-1, AI=NONE — empty World).
- **After the fix: YES — proven** by both the per-step `[BOOTSTRAP] Created …` logs and the `[SIMPROOF]` snapshot:
  ```
  [BOOTSTRAP] Created MatchState singleton.
  [BOOTSTRAP] Created UnitCatalogs (player + AI).
  [BOOTSTRAP] Created CounterMatrix.
  [BOOTSTRAP] Created Spell setup.
  [BOOTSTRAP] Created DifficultyAxes + AIScheduler.
  [BOOTSTRAP] Created GoldStore + Statue + TrainQueue (Player).
  [BOOTSTRAP] Created GoldStore + Statue + TrainQueue + AICommander (AI).
  [BOOTSTRAP] Created AI squads.  /  Created Terrain.  /  Created Mines.
  [BOOTSTRAP] World build COMPLETE (units spawn at runtime via TrainingSystem).
  ```
  `[SIMPROOF] t=0` now reads: `Gold P=100 AI=100 · Mines=2 · Statue 1000/1000 ×2 · AI=Push(x1)` — the world
  is populated (vs `-1/0` before). The radar (`screen_bootstrap_38s.png`) renders **2 gray statues, 2 yellow
  mines, 1 red AI unit** at their positions. **No exception** was logged during the build (try/catch clean).

## 3. Root cause
`BattleBootstrap`'s GameObject ("Bootstrap") lived in **`Assets/MainScene/BattleEnvironment.unity`** — i.e.
**inside the SubScene** referenced by `MainScene`. In Unity DOTS, SubScene GameObjects are **converted (baked)
to entities at import/build time and are NOT present as live GameObjects at runtime**. `BattleBootstrap` is a
plain world-building MonoBehaviour with a `Start()` and **no `Baker`**, so baking ignored it and the runtime
never instantiated it → **`Start()` never executed → the world was never built.** (Confirmed statically: the
script GUID `2e09375272f091b30963933b085d849d` was in `BattleEnvironment.unity`, not `MainScene.unity`.)

## 4. Smallest fix
**Move the Bootstrap GameObject from the SubScene to `MainScene` root** so it is a live MonoBehaviour whose
`Start()` runs. Applied:
- `Assets/MainScene.unity`: added the Bootstrap GameObject/MonoBehaviour/Transform (refs intact) and registered
  it in `SceneRoots`; wired `battleCamera` to the MainScene camera so `FrameCamera()` frames the front.
- `Assets/MainScene/BattleEnvironment.unity`: emptied (the SubScene now bakes nothing; harmless).
- CI GREEN (run 26954003685, sha `8fa772c`); device re-validation confirms execution + world creation.
*(Alternative not chosen: give BattleBootstrap a Baker — that converts it to an entity, it still wouldn't run
`Start()`; moving it to a normal scene is the correct, minimal fix for a runtime world-builder.)*

---

## 5. Post-fix simulation state (bonus — now PROVEN RUNNING, with follow-ups)
The RTS core is now **executing** (proven): the **AI commander made a decision** (`Push`), **spent gold**
(`GoldAI 100→5`), **queued a train order** (`QueueAI=1`), and **a unit spawned** (`Units=1 (AI 1), alive=1`,
`totalHP=60`) — 3 distinct world states over the run. **`MatchState=Ongoing`** with both statues `1000/1000`.

**Follow-up findings (OUT OF SCOPE here — gameplay/AI behavior, not bootstrap; not fixed; not Phase 5/6):**
- **Economy stalls:** the AI trained a **combat unit, no miner** (`Miners=0`, `Mines=2 occ 0`) → gold stuck at
  5 → cannot train more → a single AI unit. The AI's opening should build economy (miners) first.
- **Player side inert:** `GoldP=100`, `QueueP=0`, `UnitsP=0` — no human input and no player-side AI commander,
  so the player never acts (expected for an unattended run).
- **No combat engagement yet:** `Engaged=0`, statues undamaged — the lone AI unit hasn't reached/engaged a
  target in the window. Worth a separate look (movement/targeting toward the enemy statue).

These are AI-opening / input / targeting tuning items for a later, in-scope pass — they do **not** affect the
bootstrap-execution verdict (the world is built and the sim advances).

---

## 6. Status of the instrumentation
- **Fix (Bootstrap in MainScene root):** permanent, correct — keep.
- **`[BOOTSTRAP]` logs:** temporary forensic instrumentation in `BattleBootstrap.cs` (no gameplay/balance
  change); removable. **`SimDebugOverlay`** remains for observability (read-only, removable).

**STOPPING after this report per instruction. No Phase 5 / Phase 6 work. No roadmap/canon/balance change.**
