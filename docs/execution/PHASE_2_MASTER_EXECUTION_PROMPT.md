# BULWARK — Phase 2 Master Execution Prompt: Tactical Depth (Vertical Slice)

**Mission.** Add the tactical layer that distinguishes BULWARK from the original — terrain, formations, the type×armor counter matrix, the draft-3 spell system, a capped commander per faction, layered AI, and 3 maps — producing a vertical slice that proves the depth pillars. Then submit to **GATE 2**.
**Scope.** Roadmap §13 **Phase 2 (2.1–2.7)** + **GATE 2** only. No meta backend (stubs only), no monetization.
**Inputs (read first).** Roadmap §4 (combat modernization: terrain, positional mults, formations, type×armor matrix, spell synergy/telegraph, AI layers), §5 (content bible — the exact rosters/spell pool/maps), §6 (cosmetic-safety, for art hooks only), §13 Phase 2, §1 GATE 1.

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; canon docs immutable. **No phase skipping** (GATE 1 must be PASS). **No unauthorized features** — use only the §5 roster (Iron Pact + Ashen Horde, 6 units each from the 7-archetype palette), the ~12 spell pool, the 3 maps; invent nothing. **No hidden design/monetization changes. No canon drift** (one combat core; §12 ECS boundary). **Inviolable constraints** (readability, fairness/no-P2W, no save-state logging, perf budget, §15 CUT list). **Stop on ambiguity → ADR. ADR for deviations. Evidence-first. Stop at GATE 2.**

## A. Context
GATE 1 proved the loop is fun; Phase 2 proves it has *depth*. Per §4 the clean modifier chain from Phase 1 is now fed by position/terrain/armor-type, and spells become a synergistic draft. Commanders ship as **system bones only** (1/faction, capped power per §6) — the collectible roster is Phase 7. Economy/meta remain stubbed.

## B. Objectives (1:1 with roadmap §13 Phase 2)
1. **2.1** Terrain (high ground/choke/cover/hazard) + positional flank/back multipliers.
2. **2.2** Type×armor **counter matrix** (§4) + **formations** (Line/Tight/Loose).
3. **2.3** Second faction (**Ashen Horde**, 6 units) with the §5.2 asymmetry (Flanker, no dedicated Frontline).
4. **2.4** **Spell system**: pool ~12, draft 3, synergy tags, telegraph + counter (no un-counterable spell).
5. **2.5** **Commander bones**: 1/faction, 1 active + 1 passive, ≤10–15% power budget (§6).
6. **2.6** AI layering: squad layer + **budgeted tick scheduler** + multi-axis difficulty (§4).
7. **2.7** 3 maps (terrain layouts).

## C. Dependencies
- **Entry:** Phase 1 GATE 1 = PASS. Builds on the Phase-1 combat core, targeting, control, and basic AI.

## D. Files expected to exist
- Phase-1 outputs: combat core, influence-map targeting, modifier chain, control model, 1 faction/4 units, basic AI; `reports/phase-1/FINAL_REPORT.md` (GATE 1 PASS).

## E. Files to create
- `Assets/_Game/Sim/Systems/{Terrain,Formation,CounterMatrix,Spell,CommanderAbility,SquadAI,AIScheduler}.cs`.
- `Assets/_Game/Data/{Units/Ashen/*, Spells/* (~12), Maps/* (3), Commanders/* (2), CounterMatrix.asset}` (data instances for the §5-authorized content only).
- `Assets/_Game/Art/` hooks honoring §6 cosmetic-safety (locked silhouettes).
- `reports/phase-2/FINAL_REPORT.md`.

## F. Tasks
1. Implement terrain features + occupancy effects and positional flank(1.5)/back(2.0) multipliers feeding the §4 modifier chain (2.1).
2. Implement the 5×4 type×armor matrix as data (`CounterMatrix.asset`) and the 3 formations (facing tracked) (2.2).
3. Author Ashen Horde's 6 units to the §5.2 palette mapping (Flanker via Houndmaster; Razorbeast holds the line) — asymmetric, fair (2.3).
4. Implement the spell system: ~12 SO spell defs, a draft-3 pre-battle step, synergy tags (e.g., Chilled→Shatter), and a telegraph+counter for each (2.4).
5. Implement 2 commanders (1/faction): one active + one passive, within the ≤10–15% power budget; ranked-normalization hook stubbed (2.5).
6. Add the squad AI layer and a **budgeted AI scheduler** (N agents/frame) + multi-axis difficulty (eco/aggression/stats/spell-access) (2.6).
7. Build 3 terrain-layout maps (2.7).

## G. Validation gates
- Functional: terrain changes outcomes readably; counters are teachable; factions feel asymmetric yet fair; every spell is counterable; commanders stay within budget; AI is smart and frame-stable; 3 maps load.
- **GATE 2:** external playtest — **≥~40% session-2 return AND a majority of testers rate combat "readable & fun" on a fixed rubric** (roadmap §13 GATE 2). Perf within budget under the scheduler.

## H. Deliverables
Vertical slice: 2 factions (12 units), terrain/formations/counter matrix, spell-draft, 2 commanders, layered AI + scheduler, 3 maps + playtest evidence + `reports/phase-2/FINAL_REPORT.md`.

## I. Risks
- Depth not readable (mitigate: telegraphs, clarity, rubric). Combinatorial balance (mitigate: SO-data + later telemetry; do not over-tune now). Commander power creep (enforce budget). Scheduler perf.

## J. Forbidden actions
- No meta/economy backend (stubs only), no monetization, no cosmetics-store, no chests, no currencies persistence. No units/spells/maps beyond §5. No commander *collection* (Phase 7). No deterministic replays. No canon edits. No new mechanics (ADR if tempted).

## K. Exit criteria & stop conditions
- **Exit:** all 2.x functional checks PASS **and** GATE 2 playtest bar met.
- **Stop conditions:** STOP + Final Report at GATE 2; STOP on ambiguity with ADR; if depth tests as unreadable/not-fun, STOP and raise an ADR rather than papering over.

## Escalation rules
- GATE 2 FAIL (depth unreadable) → Game Director (rescope/iterate ADR). Balance disputes → Lead Systems Designer (within canon). Perf → Technical Architect.

## L. Mandatory final report (`reports/phase-2/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 2 Final Report
## 1. Phase Summary
## 2. Work Completed     (2.1–2.7 — Done/Partial/Blocked — evidence + §ref)
## 3. Files Created
## 4. Files Modified      (no canon docs)
## 5. Validation Results  (per-sub + GATE 2 playtest metrics + perf)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised
## 9. Recommendations
## 10. Gate Status        (GATE 2: PASS/FAIL; Authorization to proceed to Phase 3: GRANTED/WITHHELD)
```
