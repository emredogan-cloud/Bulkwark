# BULWARK — Simulation Proof Report

**Date:** 2026-06-04 · **Goal:** convert simulation status from **UNDETERMINED** → **PROVEN RUNNING** or
**PROVEN BROKEN** with on-device evidence, after restoring observability. **Rule honored:** evidence only —
no "probably running." **Device:** Xiaomi **Redmi Note 11R**, Android 13, arm64-v8a (serial `jfzxugsgnnvsrsg6`).
**Build under test:** `com.DefaultCompany.bulwarkclean` **v0.0.34**, CI run **26950331001** (sha `f0bf4ff`,
GREEN), with the `SimDebugOverlay` observability layer. **Artifacts:** `runtime/device_validation/`
(`simproof_logcat.txt` [21,701 lines], `simproof_excerpt.txt` [31 `[SIMPROOF]` lines], `screen_overlay_12s.png`,
`screen_overlay_30s.png`).

---

# ⛔ VERDICT: PROVEN BROKEN
**The ECS runtime and its 33 systems initialize and tick, but the battle world is EMPTY — so the RTS
simulation (economy → units → AI → combat → statue) is NOT running.** Root cause: **`BattleBootstrap` never
executed** (it is not present/active in `MainScene`), and the `BattleEnvironment` SubScene bakes **no** setup
entities. The systems are alive but starved — they have nothing to simulate.

*(This is a wiring/content-population failure, not proof that the systems' internal logic is buggy — their
logic cannot be exercised because they receive no entities. The end-to-end simulation is provably not running.)*

---

## 1. Rendering restoration results (STEP 3)
- **`com.unity.entities.graphics` — investigated, deferred (documented).** Re-adding it (a) was previously
  flagged *unresolvable* in CI (`FIRST_COMPILE_REPORT.md` §6 / `c60d562`) and (b) renders nothing by itself —
  it requires authoring `RenderMeshArray`+`MaterialMeshInfo` on every entity (a sim/authoring change). Deferred
  to the production-art pass (see §10).
- **Chosen path: a camera/URP/art-independent IMGUI observability layer** (the owner-sanctioned STEP-4 proxy
  fallback). It compiled GREEN first try (pre-push review: compile + safety PASS) and **renders on device.**

## 2. Visibility restoration results (STEP 4-11) — SUCCESS
`Assets/_Game/Bootstrap/SimDebugOverlay.cs` (temporary, removable, **read-only**, no gameplay/balance change):
- **On-screen overlay renders** — `screen_overlay_30s.png` is **119,237 B** vs the old flat-brown frame's
  **26,802 B** (4.4×): the stats panel (MatchState, Gold, Units, Miners, Mines, Statues, AI, Queue, Combat,
  FPS, Δalive) is visibly drawn. The colored-box "radar" is empty **because there are no entities to plot.**
- **Logcat `[SIMPROOF]` snapshots emit ~1×/sec** (31 lines over 30 s) — independent of the render layer.
- **Status converted UNDETERMINED → PROVEN.** We can now answer both questions with evidence (§9).

## 3. Log evidence (`simproof_excerpt.txt`)
Every snapshot across the full 30 s is **identical** (one distinct tuple → fully static):
```
[SIMPROOF] SimDebugOverlay booted (AfterSceneLoad).
[SIMPROOF] t=0.0s  f=1  fps=0  sys=33 Match=Ongoing GoldP=-1 GoldAI=-1 Units=0(P0/AI0) Alive=0 Miners=0 Mines=0(occ0) StatueP=-1/0 StatueAI=-1/0 AI=NONE(x0) QueueP=0 QueueAI=0 Engaged=0 dUnits=0 dAlive=0 dGoldP=0 dHealth=0
...
[SIMPROOF] t=29.1s f=846 fps=30 sys=33 Match=Ongoing GoldP=-1 GoldAI=-1 Units=0(P0/AI0) Alive=0 Miners=0 Mines=0(occ0) StatueP=-1/0 StatueAI=-1/0 AI=NONE(x0) QueueP=0 QueueAI=0 Engaged=0 dUnits=0 dAlive=0 dGoldP=0 dHealth=0
```
`grep -oE "Match=… GoldP=… GoldAI=… Units=…"` over the whole log returns **exactly one** line →
`Match=Ongoing GoldP=-1 GoldAI=-1 Units=0` — **nothing changes over 30 s.**

## 4. Screenshot evidence
`screen_overlay_30s.png` shows the live panel: `t=28.2s fps=30 frame=820 systems=33 · MatchState: Ongoing ·
Gold P0=-1 AI=-1 · Units 0 (P0=0/AI=0) alive=0 · Miners=0 Mines=0 · Statue P0=-1/0 AI=-1/0 · AI stance: NONE
(x0) · Train queue P0=0 AI=0 · Engaged(combat)=0 totalHP=0 · Δalive/s=0`. The remainder is the empty
camera (brown) — overlay works; there is simply nothing to render behind it.

## 5. ECS runtime evidence
- **`sys=33`** unmanaged systems exist in the default World → the ECS system set **is created and ticking**.
- `MatchState=Ongoing` exists — created by **`MatchFlowSystem.OnCreate`'s fallback** ("create Ongoing if
  missing"), **not** by `BattleBootstrap` (which would also have created Gold/statues/etc.). This is itself
  evidence the systems run while the bootstrap did not.
- Unity engine: `Built from 6000.0/staging … 6000.0.75f1 … il2cpp … Release`; `SimDebugOverlay booted`; overlay
  reached **30 fps** → the app + main loop + overlay are healthy. **No app crash/exception** (logcat errors are
  all MIUI/other-process noise).

## 6. Economy evidence (STEP 7) — NOT RUNNING
`GoldP=-1 GoldAI=-1` (the `-1` sentinel = **no `GoldStore` entity** for either team), `Miners=0`,
`Mines=0 (occ 0)`. **No economy entities exist** → `MiningSystem` has nothing to mine; gold never changes.

## 7. Unit evidence (STEP 8) — NOT RUNNING
`Units=0 (P0=0/AI=0)`, `Alive=0`, `QueueP=0 QueueAI=0`. **No units, no train queues, zero spawns** across 30 s
(`dUnits=0`, `dAlive=0`) → `TrainingSystem` has nothing to spawn (and no `AICommander` to pace orders).

## 8. AI evidence (STEP 9) — NOT EXECUTING
`AI=NONE(x0)` → **no `AICommander` entity exists** (`BattleBootstrap` creates it; it didn't run). The AI
systems are among `sys=33` but have no commander/units to act on → AI does not execute meaningfully.

## 9. Combat evidence (STEP 10) — NOT RUNNING
`Engaged(combat)=0`, `totalHP=0`, `StatueP=-1/0 StatueAI=-1/0` (no statues). **No units → no targets → no
damage/deaths** (`dHealth=0`, `dAlive=0`). `CombatSystem`/`StatueDamageSystem` have nothing to process.

## 10. Root cause + recommended fix
**Root cause (proven):** `MainScene` streams the `BattleEnvironment` SubScene (`EntityScenes/e607781938….0.entities`,
869 ms) but it bakes **no** battle-setup entities (it is effectively empty — `BattleEnvironment.unity` = 1
GameObject, 0 renderers), **and there is no active `BattleBootstrap`** in the scene (zero `[BattleBootstrap]`
log lines — not even its `'… unassigned'` validation errors, which it *would* emit if it ran with missing
refs). So the 33 systems boot against an empty World.

**Fix (one of):**
1. **Add a configured `BattleBootstrap` to `MainScene`** — a GameObject with the `BattleBootstrap` component
   and its inspector refs assigned: **`map`** (a `MapDef`), **`playerUnits[]`** (Iron Pact `UnitDef`s),
   **`aiUnits[]`** (Ashen Horde `UnitDef`s), **`counterMatrix`** (`CounterMatrix` `BalanceConfig`); optionally
   `spellPool` / commanders / `battleCamera`. At `Start()` it builds GoldStore + statues + mines + unit
   catalogs + `AICommander` + `MatchState` → the 33 systems then simulate (miners train → mine gold → push →
   combat → statue damage → `MatchState` → Victory/Defeat).
2. **OR** author the same setup entities **into the `BattleEnvironment` SubScene** (authoring components that
   bake to `GoldStore`/`MineNode`/`StatueState`/`AICommander`/catalogs) — currently it bakes nothing.

This is a **Unity-editor wiring step** (assign scene refs) — owner/runtime work (the long-DEFERRED
`BattleBootstrap`-inspector-refs item from Phases 1–4 / GATE 1).

**Re-validation:** the `SimDebugOverlay` is left in the build. After the fix, re-run this exact loop; the
overlay will then show **gold rising, units spawning, alive changing, statue HP dropping, AI stance active,
and `MatchState` transitioning** — which flips the verdict to **PROVEN RUNNING**.

## 11. Success-condition answers (evidence-based)
- **"Can the player actually SEE the simulation?"** → The **observability layer now renders on-screen**
  (proven: 119 KB overlay frame), so state *would* be visible — but **there is currently nothing to see**
  (empty world). Spatial entity rendering (entities.graphics / sprites) remains a separate production task.
- **"Can we prove the RTS core is running?"** → **The ECS systems run; the simulation does NOT.** Proven by
  evidence: 33 systems tick, but 0 gold / 0 units / 0 mines / 0 statues / 0 AI commander, fully static for 30 s.

---

**STOPPING after this report per instruction. No Phase 5 / Phase 6 work. The read-only `SimDebugOverlay` is
left installed for re-validation; it is fully removable (delete `Assets/_Game/Bootstrap/SimDebugOverlay.cs`).**
