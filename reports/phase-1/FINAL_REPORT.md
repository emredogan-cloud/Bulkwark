# BULWARK — Phase 1 Final Report

## 1. Phase Summary
Phase 1 (Core Combat Prototype — the binding **FUN GATE**) was authored end-to-end:
the recovered core loop (**mine → train → push → topple the Statue**) for **one faction
(Iron Pact) with 4 units**, the **squad-command + possess** control hook, **influence-map
targeting** + the **§4 damage modifier chain** with *basic* counters, and a **single-layer
utility AI** — assembled into one in-code battle that resolves to **Victory/Defeat**.
All work is original and IP-free, committed to the clean R1 repo (`bulwark-clean`).

Implementation was produced by an orchestrated pipeline: each subsystem was **authored
against a fixed ECS contract**, **adversarially canon-verified** (every module was flagged
on first pass — real bugs caught: a last-write-wins on mine occupancy, statues unreachable
by targeting, field-name/enum mismatches, a missing AI in the system order), **repaired**,
and then put through a **cross-module integration audit** that removed 2 duplicate type
definitions, closed a control→movement contract gap (`MoveDestination`), and verified the
shared-type references and system order.

**Honest status:** there is **no Unity 6 / Entities toolchain, device, or backend** in the
authoring environment (ADR-0-001/-002), so the code is **authored, not compiled or run**.
Per the evidence-first / no-faked-success rule (§15.8), all runtime functional checks, the
perf budget, and **GATE 1 (the fun verdict)** are reported **DEFERRED**, not PASS. No combat
was played; no "is it fun?" verdict is asserted — that is the owner's call on a real build.

## 2. Work Completed   (Done = authored & canon-verified; runtime validation DEFERRED)
| Sub | Objective | Status | Evidence (§ref) |
|---|---|---|---|
| **1.1** | Economy + objective: capped/contestable mines → Gold; Statue w/ shield + damage states + trickle throttle | **Done (authored)** | [OBS] `Sim/Systems/Mining.cs` (miner-cap, OwnerTeam=-1 contest, O(1) cached assignment, authoritative occupancy recount), `Sim/Systems/StatueDamage.cs` (shield→health, intact→cracked→breaking→destroyed, `TrickleThrottle`, sets `MatchState`). §13 P1.1, §11 |
| **1.2** | One faction, 4 units; train/queue/deploy from Gold | **Done (authored)** | [OBS] `Sim/Systems/Training.cs` (TrainOrder queue, Gold-gated, data-driven spawn), 4 `Data/Units/IronPact_*.asset` (Miner/Shieldman/Legionary/Crossbow → §5.2 roles), `Data/Balance/IronPactBalance.asset`. Stats from DATA, never hardcoded. §13 P1.2, §5.2, §15.6 |
| **1.3** | Control: squad select/command/drag + long-press possess | **Done (authored)** | [OBS] `Control/BattleInput.cs` (MonoBehaviour shell, writes `ManualOrder`/`Possessed`), `Sim/Systems/PossessControl.cs` (consumes orders, manual overrides auto-target, writes `MoveDestination`). §12 boundary honored. §13 P1.3, §4 |
| **1.4** | Influence-map targeting + modifier chain + basic counters | **Done (authored)** | [OBS] `Sim/InfluenceMap.cs` (~4 Hz grid, X-bins × 3 rows), `Sim/Systems/Targeting.cs` (same-row pref + ×1.2 stickiness, row-bucketed — **no O(N²) scan**), `Sim/Systems/Combat.cs` (`round(base×(1+lvl×perLvl))×typeArmor×positional×terrain×difficulty`; neutral slots at P1; statue inbox routing). §13 P1.4, §4, §12 |
| **1.5** | Basic AI: single-layer utility commander + unit layer | **Done (authored)** | [OBS] `Sim/Systems/BasicAI.cs` (O(1) stance utility over Gold/counts/statue HP; paces TrainOrders for a fair composition; units reuse the shared Targeting/Combat — no bespoke combat). §13 P1.5, §5.1 |
| **—** | Assemble one end-to-end battle → Victory/Defeat | **Done (authored)** | [OBS] `Bootstrap/BattleBootstrap.cs` (builds the battle in code: stores, statues, mines, catalog, counter matrix, queues, AI commander, MatchState), `Sim/Systems/SimSystemOrder.cs` (canonical order Mining→Training→InfluenceMap→PossessControl→Targeting→Movement→Combat→StatueDamage→MatchFlow), `Sim/Systems/MatchFlow.cs` (finalizes/freezes outcome). |

## 3. Files Created
**Data layer (schema split + content):** `Data/Schemas/CombatTypes.cs`, `Data/Schemas/UnitDef.cs` (+`.meta`), `Data/Schemas/SpellDef.cs`, `Data/Schemas/BalanceConfig.cs` (+`.meta`); `Data/Units/IronPact_{Miner,Shieldman,Legionary,Crossbow}.asset` + `Data/Units/README.md`; `Data/Balance/IronPactBalance.asset`.
**Sim contract:** `Sim/Components/Phase1Components.cs`.
**Sim systems:** `Sim/Systems/{Mining,StatueDamage,Training,Targeting,Combat,BasicAI,SimSystemOrder,MatchFlow,PossessControl}.cs`, `Sim/InfluenceMap.cs`.
**Control:** `Control/BattleInput.cs`, `Control/Bulwark.Control.asmdef`.
**Bootstrap:** `Bootstrap/BattleBootstrap.cs`, `Bootstrap/Bulwark.Bootstrap.asmdef`.
**Governance:** `docs/adr/ADR-0-002-conditional-phase0-acceptance.md`, this report, the clean-repo `README.md`.

## 4. Files Modified
- `Sim/Systems/SimSystems.cs` — Phase-0 O(N²) MoveSystem/AttackSystem spike **retired** (replaced by Targeting/Movement/Combat/InfluenceMap); kept as a documented empty stub.
- `Sim/Bulwark.Sim.asmdef` — added `Bulwark.Data` reference (sim reads combat enums from data).
- `Data/Schemas/DataSchemas.cs` — **deleted**; split into per-class files (Unity requires `filename == ScriptableObject classname` so the `.asset` instances resolve).
- `reports/phase-0/FINAL_REPORT.md` — gate verdicts reclassified BLOCKED → DEFERRED per ADR-0-002.
- **No canon doc was modified** (the five `report/*.md` design docs are untouched; verified).

## 5. Validation Results
**Static/authoring validation (executed here):**
- **Canon compliance:** PASS — every module adversarially reviewed against §4/§11/§12/§15; no Phase-2 feature implemented (terrain/flank geometry/formations/full matrix/spells/commanders/2nd faction/persistence/progression/determinism are absent; factor slots are neutral 1.0/level 0; SpellDef is empty schema only).
- **§12 ECS boundary:** PASS — battle sim is ISystem/Burst under `_Game/Sim`; input is MonoBehaviour under `_Game/Control` writing data only.
- **No hardcoded balance:** PASS — all stats sourced from `UnitDef`/`BalanceConfig` data; the only code constants are neutral defaults + a movement arrival epsilon.
- **Integration coherence:** PASS — 0 duplicate type declarations; all shared types declared once; system-order references resolve; control→movement handoff wired.
- **Perf rule (§12 O(1)/unit):** STATIC PASS — targeting/AI use influence-map/row buckets, not all-pairs scans; the Phase-0 O(N²) spike was retired. (Measured 200-unit frame time = DEFERRED.)

**Runtime validation (cannot execute here — DEFERRED):**
| Check | Result |
|---|---|
| 1.1 mining yields Gold; statue states read | **DEFERRED** (no runtime) |
| 1.2 units train/deploy from Gold | **DEFERRED** |
| 1.3 possess/command feels good **on device** | **DEFERRED** (on-device only) |
| 1.4 front line reads; targeting stable | **DEFERRED** |
| 1.5 AI provides a fair fight | **DEFERRED** |
| Perf: ≥200 units stable frame on mid-range | **DEFERRED** |
| **GATE 1 (FUN): "is the combat fun? one more game?"** | **DEFERRED** — binding owner verdict on a playable build; not asserted |

## 6. Known Issues
- Entire C# tree is **authored, not compiled**; expect first-compile fixups in Unity 6 (package resolves, `.meta` generation, Burst/`SystemAPI` API drift in Entities 1.x).
- Hand-authored `.asset`/`.meta` use fixed GUIDs (`...beef0001/0002`); verify import / let Unity regenerate `.meta`.
- `BasicAISystem` and `PossessControlSystem` carry `[UpdateInGroup]` but **not** explicit pairwise `[UpdateAfter]/[UpdateBefore]` attributes; the canonical order is documented in `SimSystemOrder.cs` but not fully enforced by attributes (runtime scheduling — DEFERRED). Recommend adding the attributes before perf/feel testing.
- All unit/counter **values are PROVISIONAL** (LSD-owned, §16) — first-pass only, to be tuned by the fun pass + telemetry; not asserted canon (§15.6).

## 7. Risks
- **Combat not fun (CRITICAL — this is the gate).** Unmitigable on paper; requires a playable build + owner verdict. The whole prototype exists to answer it; it remains open until DEFERRED → run.
- **Validation debt compounding:** Phase-0 deferrals (CI/perf/BaaS) + Phase-1 deferrals (functional/feel/perf/fun) must all be burned down before any meta build. Tracked via ADR-0-002.
- **Touch-control feel** (§I): possess/drag tuning is device-dependent; deferred.
- **Uncompiled code drift:** the longer it sits un-built, the more first-compile churn; recommend opening in Unity 6 soon.

## 8. ADRs Raised
- **ADR-0-002** — Conditional Phase-0 acceptance (runtime validations deferred); grants conditional authorization that enabled this phase. No new ADR was required during Phase 1 (no canon deviation; all mechanics trace to §3/§4/§11/§13).

## 9. Recommendations
1. **Open `bulwark-clean` in Unity 6 LTS** and run the first-compile pass; fix import nits; generate `.meta`.
2. **Execute the deferred validations** (Phase 0 + Phase 1) on a device: CI green, 200-unit frame-ms, in-editor resolver proof, BaaS round-trip, then the **GATE 1 fun playtest**.
3. Add explicit system-ordering attributes to `BasicAISystem`/`PossessControlSystem`; build a minimal Scene/SubScene around `BattleBootstrap`.
4. **Do NOT begin Phase 2** (tactical depth) until GATE 1's fun verdict is rendered PASS by the owner — terrain/formations/2nd faction/spells/commanders stay forbidden until then (§J, §15).

## 10. Gate Status
- **GATE 1 (FUN — combat fun?):** **DEFERRED** — the prototype is authored and canon-verified, but the binding fun verdict requires a playable on-device build that this environment cannot produce. **Not PASS, not FAIL.** No fun claim is fabricated.
- **Authorization to proceed to Phase 2:** **WITHHELD** — pending (a) a successful build, (b) the deferred runtime validations, and (c) the owner's GATE 1 fun verdict = PASS.
