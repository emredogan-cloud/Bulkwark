# BULWARK — Phase 1 Master Execution Prompt: Core Combat Prototype (FUN GATE)

**Mission.** Build the smallest fun, complete micro-battle: the recovered core loop (mine → train → push → topple the statue) with the signature direct-control hook, readable targeting, and a basic AI opponent — then submit it to the **binding FUN GATE**. No meta, economy, monetization, or content beyond one faction's 4 units.
**Scope.** Roadmap §13 **Phase 1 (1.1–1.5)** + **GATE 1** only. Combat-core only.
**Inputs (read first).** Roadmap §3 (PRESERVE: loop, direct control, readable lane combat), §4 (combat spec: control model, influence-map targeting, modifier chain, roles), §11 (world: statue, mines, battlefield), §13 Phase 1, §0 Phase-0 exit; `report/PRODUCTION_DECISION_LOG.md` §3 (non-deterministic sim OK).

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; the four canon docs are immutable. **No phase skipping** (Phase 0 must be PASS first; nothing from Phase 2+). **No unauthorized features** (canon §2–§12 closed; no new units/currencies/mechanics/modes). **No hidden design or monetization changes.** **No canon drift.** **Inviolable constraints:** readability, fairness/no-P2W, server authority over currency (n/a yet — do not add currency persistence), no save-state logging, perf budget, §15 CUT list. **Stop on ambiguity → ADR; never guess.** **ADR for any deviation.** **Evidence-first.** **Stop at GATE 1.**

## A. Context
This is the make-or-break fun check. The original's strength was *agency + a legible loop* (roadmap §3); Phase 1 must reproduce that feel before any meta is built. The full tactical layer (terrain/formations/counters) is **Phase 2** — do not pull it forward; Phase 1 ships only "basic counters" inside the modifier chain.

## B. Objectives (1:1 with roadmap §13 Phase 1)
1. **1.1** Economy + objective: mines (miner-capped), miners mine Gold, the **Statue** with readable damage states (per §11).
2. **1.2** One faction, **4 units** (Miner + 3 roles from the §5.2 palette), with train/queue/deploy.
3. **1.3** Control model: **squad command + drag + possess** (the agency hook, §4).
4. **1.4** **Influence-map targeting** + the **damage modifier chain** with *basic* counters (§4).
5. **1.5** Basic AI: utility commander + unit layer (§4/§5.1 — single-layer, O(1)).

## C. Dependencies
- **Entry:** Phase 0 Exit = PASS (ECS sim core, data pipeline, CI). Build on the Phase-0 ECS systems.

## D. Files expected to exist
- Phase-0 outputs: building project, `Assets/_Game/Sim/*`, SO schemas + 3-tier resolver, CI, `reports/phase-0/FINAL_REPORT.md` (Gate PASS).

## E. Files to create
- `Assets/_Game/Sim/Systems/{Mining,Training,Targeting,Combat,StatueDamage,BasicAI}.cs` (ECS).
- `Assets/_Game/Data/Units/` (4 UnitDef instances — engineering data populated to roadmap-authorized roles; **no new archetypes**).
- `Assets/_Game/Control/` (squad-select/command/possess input — MonoBehaviour shell per §12).
- `Assets/_Game/Sim/InfluenceMap.cs` (grid threat/value, ~4 Hz).
- `reports/phase-1/FINAL_REPORT.md`.

## F. Tasks
1. Implement mine nodes (miner cap), Gold income, and the Statue with intact→cracked→breaking→destroyed states + trickle-damage throttle (§11) (1.1).
2. Author the 4 units to authorized roles (Miner + Frontline + Skirmisher + Ranged, Iron Pact) with train/queue/deploy from Gold (1.2). Values come from SO data; do not hardcode balance.
3. Implement squad select/command/drag and long-press **possess** with smart auto-target + manual override (1.3).
4. Build the influence-map targeting (same-row preference + ×1.2 stickiness) and the damage modifier chain (round(base×(1+lvl×perLvl))×typeArmor×positional×terrain[neutral now]×difficulty); wire *basic* counters only (1.4).
5. Implement single-layer utility-AI opponent (commander stance + unit targeting) for a fair fight (1.5).
6. Assemble one playable end-to-end battle to Victory/Defeat.

## G. Validation gates
- Per-sub functional checks (mining yields Gold; units train/deploy; possess works on device; targeting reads as a front line; AI provides a fair fight).
- **GATE 1 (FUN — BINDING):** internal verdict "is the combat fun? would a player want one more game?" Combat-feel approved by the owner. **If not fun → STOP and file a pivot ADR; do not proceed to meta.**

## H. Deliverables
Playable single-battle prototype (1 faction/4 units, control + targeting + statue + basic AI) + perf still within budget + `reports/phase-1/FINAL_REPORT.md` with the GATE 1 verdict.

## I. Risks
- **Combat not fun** (critical — this is the gate). Touch-control feel on device. Targeting jitter. Mitigate: iterate control/feel before declaring the gate.

## J. Forbidden actions
- No second faction, terrain, formations, full counter matrix, spells, or commanders (all **Phase 2**). No meta/economy persistence, no monetization, no cosmetics. No new units beyond the 4 authorized. No balance invention (values from SO data). No canon edits.

## K. Exit criteria & stop conditions
- **Exit:** all 1.x functional checks PASS **and** GATE 1 fun verdict = PASS.
- **Stop conditions:** STOP + Final Report at GATE 1 (await authorization), OR STOP immediately on ambiguity/blocker with an ADR, OR STOP+pivot-ADR if combat isn't fun.

## Escalation rules
- Fun-gate FAIL → escalate to the **Game Director** (§16) with a pivot ADR. Perf regression → Technical Architect. Any request for Phase-2 features → reject.

## L. Mandatory final report (`reports/phase-1/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 1 Final Report
## 1. Phase Summary
## 2. Work Completed     (1.1–1.5 — Done/Partial/Blocked — evidence + §ref)
## 3. Files Created
## 4. Files Modified      (no canon docs)
## 5. Validation Results  (per-sub checks + GATE 1 fun verdict + perf)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised
## 9. Recommendations
## 10. Gate Status        (GATE 1: PASS/FAIL — combat fun?; Authorization to proceed to Phase 2: GRANTED/WITHHELD)
```
