# BULWARK — Phase 6 Master Execution Prompt: Live Launch & Season 1

**Mission.** Take the validated MVP global and stand up the live-ops cadence: pass launch-readiness (**GATE 6**), ship the global build, deliver Season 1's **single** content slot (one unit *or* commander *or* map), and operate the 8-week season cadence. Only after an owner-granted SCALE decision from GATE 5.
**Scope.** Roadmap §13 **Phase 6 (6.1–6.2)** + **GATE 6** only. Post-launch deferred features are **Phase 7**, not here.
**Inputs (read first).** Roadmap §13 Phase 6 (Entry: GATE 5 PASS; GATE 6 launch-readiness), §7 (mode rules), §8/§10 (live-ops/monetization config), §12 (launch-readiness: crash-free/perf/store-compliance), §16 (governance); `report/PRODUCTION_DECISION_LOG.md` (the per-season **single content slot**: one of {unit | commander | map}).

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; canon docs immutable. **No phase skipping** (GATE 5 owner-granted SCALE required). **No unauthorized features** — Season 1 ships exactly **one** {unit | commander | map} from canon; no second content item, no Phase-7 features (ranked-seasons, clans, 3rd faction, biomes, commander-collection, event-engine, real-time). **No hidden design/monetization changes.** **No canon drift.** **Inviolable constraints in force.** **Stop on ambiguity → ADR. ADR for deviations. Evidence-first. Stop when the cadence is operational; gate any Phase-7 work on its decision-log trigger.**

## A. Context
GATE 5 (owner-decided) authorized scaling. Phase 6 is the controlled global rollout + the first live season. Cadence is **8 weeks** with a **single content slot** (decision log) to remain sustainable for the team size. Anything richer (ranked, clans, etc.) is deferred to Phase 7 and trigger-gated.

## B. Objectives (1:1 with roadmap §13 Phase 6)
1. **6.1** Global launch — pass **GATE 6 (launch-readiness)**: crash-free ≥ ~99%, perf budget met, store compliance; then global rollout.
2. **6.2** Season 1: deliver **one** new {unit | commander | map} (single slot) + operate the **8-week live-ops cadence** (battle pass rotation, shop rotation, weekend modifiers, soft balance patch).

## C. Dependencies
- **Entry:** Phase 5 **GATE 5 = owner-granted SCALE**. Deps: 6.1 ← GATE 5; 6.2 ← 6.1.

## D. Files expected to exist
- A tuned, instrumented, validated MVP (Phases 0–5); live-ops + RC + analytics; all prior FINAL_REPORTs with PASS gates (GATE 5 = SCALE).

## E. Files to create
- `reports/phase-6/{launch-readiness-checklist.md, s1-content-spec.md, liveops-calendar.md}`.
- **One** of: `Assets/_Game/Data/Units/<new>.asset` | `…/Commanders/<new>.asset` | `…/Maps/<new>.asset` (the single S1 slot, within canon caps).
- Season config (battle pass S1, shop rotation) — data only.
- `reports/phase-6/FINAL_REPORT.md`.

## F. Tasks
1. Run the **launch-readiness** checklist (crash-free %, perf on device matrix, store-policy compliance, server capacity) → **GATE 6**; on PASS, execute the global rollout (staged is acceptable) (6.1).
2. Author and ship the **single** S1 content item (respecting §5 caps and §6 cosmetic-safety if applicable); rotate battle pass to S1, refresh shop, schedule weekend modifiers, apply a telemetry-driven soft balance patch (6.2).
3. Operate the 8-week cadence loop; confirm live-ops tooling (RC, analytics, content delivery) supports it without manual store releases.

## G. Validation gates
- **GATE 6 (launch-readiness):** crash-free ≥ ~99%, perf within budget, store-compliant — before global rollout.
- 6.2: exactly one new content item shipped (no more); 8-week cadence operational; no fairness/readability regressions.

## H. Deliverables
Global live build + launch-readiness evidence + S1 single content item + S1 battle pass/shop config + an operating 8-week cadence + `reports/phase-6/FINAL_REPORT.md`.

## I. Risks
- Live-ops sustainability for team size (mitigate: single-slot, 8-week cadence — do not over-commit). Launch stability/server load. Cadence over-commit (forbidden). Scope pressure toward Phase-7 features.

## J. Forbidden actions
- No more than one S1 content item. No Phase-7 features (ranked seasons, clans, 3rd faction, biomes, commander collection, event engine, real-time) — each requires its decision-log trigger (Phase 7). No fairness/monetization changes (inviolable). No canon edits. No 2-week treadmill cadence.

## K. Exit criteria & stop conditions
- **Exit:** GATE 6 PASS + global build live + S1 slot shipped + 8-week cadence operational.
- **Stop conditions:** STOP + Final Report once the cadence is operational; STOP on ambiguity with ADR. **Do not start any Phase-7 feature** without confirming its decision-log revisit-trigger has fired (then use the Phase-7 prompt).

## Escalation rules
- Launch-readiness FAIL → Technical Architect + Game Director (hold rollout). Cadence/content scope → Game Director + Live-ops/Product (single slot is canon). Any Phase-7 feature request → defer; verify the trigger first (§16 / decision log).

## L. Mandatory final report (`reports/phase-6/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 6 Final Report
## 1. Phase Summary
## 2. Work Completed     (6.1–6.2 — evidence + §ref; confirm SINGLE content slot)
## 3. Files Created
## 4. Files Modified      (no canon docs)
## 5. Validation Results  (GATE 6 launch-readiness: crash-free %, perf, store-compliance; cadence operational)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised
## 9. Recommendations     (live-ops health; which Phase-7 trigger(s), if any, are approaching)
## 10. Gate Status        (GATE 6: PASS/FAIL; Live launch: DONE/HELD; S1: shipped; Phase-7 entry: only on a fired trigger)
```
