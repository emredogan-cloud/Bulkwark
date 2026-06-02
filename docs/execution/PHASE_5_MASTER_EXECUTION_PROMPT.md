# BULWARK — Phase 5 Master Execution Prompt: Soft Launch & Tuning (SCALE-OR-STOP GATE)

**Mission.** Release the MVP to limited geos, instrument it, and **tune retention/monetization/balance with real data + harden stability/perf** — then submit to the binding **SCALE-OR-STOP gate (GATE 5)**. This phase ships **no new features** — tuning, telemetry, and hardening only.
**Scope.** Roadmap §13 **Phase 5 (5.1–5.3)** + **GATE 5** only.
**Inputs (read first).** Roadmap §13 Phase 5 (Entry: GATE 4 PASS) + GATE 5 thresholds, §10 (monetization KPIs), §12 (analytics, RC, perf), §16 (governance — the SCALE-OR-STOP decision owner); `report/PRODUCTION_DECISION_LOG.md` (the scale-or-stop gate is a defined kill/scale checkpoint).

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; canon docs immutable. **No phase skipping** (GATE 4 PASS). **NO NEW FEATURES** — this phase is tuning/telemetry/hardening only; new content/mechanics/monetization are forbidden. **No hidden design/monetization changes** beyond RC-tunable values already in canon. **No canon drift.** **Inviolable constraints remain in force.** **Stop on ambiguity → ADR. ADR for deviations. Evidence-first (data-backed). Stop at GATE 5 — the scale/stop decision is the owner's, not the agent's.**

## A. Context
The MVP is feature-complete (GATE 3) and ethically monetized (GATE 4). Phase 5 answers the two binding commercial/retention questions with real players. Per the roadmap, the monetization model is fair-by-design, so retention + conversion must clear a bar to justify global scale. The agent **tunes within canon** (RC values, balance, perf) and **reports**, but the **scale-or-stop verdict is made by the owner** (§16).

## B. Objectives (1:1 with roadmap §13 Phase 5)
1. **5.1** Limited-geo release + telemetry/funnel dashboards (Entry: GATE 4 PASS).
2. **5.2** Retention/monetization tuning via RemoteConfig (no app updates for value changes).
3. **5.3** Balance + perf/stability hardening (crash-free, frame budget).

## C. Dependencies
- **Entry:** Phase 4 **GATE 4 = PASS** (fairness audit). Deps: 5.1 ← GATE 4; 5.2, 5.3 ← 5.1.

## D. Files expected to exist
- A feature-complete, ethically-monetized MVP (Phases 0–4 outputs); RC + analytics pipeline; prior FINAL_REPORTs (all gates PASS).

## E. Files to create
- `reports/phase-5/{telemetry-dashboards.md, tuning-log.md, balance-changelog.md, stability-report.md}`.
- RC value sets / experiment configs (server-side; **values only**, no new systems).
- `reports/phase-5/FINAL_REPORT.md`.
- (No new gameplay/monetization code files; bug-fix patches only.)

## F. Tasks
1. Ship to limited geos; stand up funnel/retention/monetization dashboards; verify event integrity (5.1).
2. Tune retention curves, reward pacing, ad-cadence, and battle-pass pacing **via RemoteConfig** (server-side A/B); record every change in `tuning-log.md` (5.2).
3. Harden: fix crashes/regressions, hit the perf budget on the device matrix, balance via telemetry (capped/normalized per canon) (5.3).
4. Continuously measure against GATE 5: D1, D7, and the monetization floor (blended D30 LTV ≥ target CPI — the exact floor is set by an ADR before P5 per §13 GATE 5).

## G. Validation gates
- Telemetry trustworthy (events complete, dashboards accurate); RC changes take effect without app updates; crash-free ≥ target; perf within budget on the matrix.
- **GATE 5 (SCALE-OR-STOP, binding):** **D1 ≥ ~40%, D7 ≥ ~18% AND blended D30 LTV ≥ target CPI → recommend SCALE; else recommend STOP/RESCOPE.** Present the data; the **owner decides** (§16).

## H. Deliverables
Limited-geo live build + telemetry dashboards + tuning/balance/stability reports + a GATE-5 data package + `reports/phase-5/FINAL_REPORT.md` with the measured metrics and a scale/stop **recommendation** (not a unilateral decision).

## I. Risks
- **LTV below the gate** (top commercial risk — may trigger STOP/rescope; that is a valid, honest outcome). Over-tuning chasing metrics. Perf regressions. Telemetry gaps undermining the decision.

## J. Forbidden actions
- No new features/content/mechanics/monetization. No design changes beyond RC-tunable canon values. No fairness/P2W changes (inviolable). No global scale before GATE 5 is decided by the owner. No faking metrics to pass the gate. No canon edits.

## K. Exit criteria & stop conditions
- **Exit:** stable, tuned, instrumented limited-geo build + a complete GATE-5 data package and recommendation.
- **Stop conditions:** STOP + Final Report at GATE 5 and await the owner's scale/stop decision; STOP on ambiguity/telemetry-integrity issues with an ADR. Do not initiate global launch.

## Escalation rules
- **GATE 5 is a Game-Director + Live-ops/Product decision** (§16), informed by this data. Below-gate metrics auto-escalate to those owners with a rescope/stop ADR. Perf/stability blockers → Technical Architect.

## L. Mandatory final report (`reports/phase-5/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 5 Final Report
## 1. Phase Summary
## 2. Work Completed     (5.1–5.3 — tuning/hardening — evidence + §ref; NO new features)
## 3. Files Created      (reports/configs only)
## 4. Files Modified      (patches/RC values; no canon docs)
## 5. Validation Results  (telemetry integrity, RC-live changes, crash-free %, perf; GATE 5 metrics: D1 / D7 / D30-LTV vs CPI)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised        (incl. the GATE-5 monetization-floor ADR)
## 9. Recommendations    (SCALE / STOP / RESCOPE — with the data justification)
## 10. Gate Status        (GATE 5: SCALE-recommended / STOP-recommended — OWNER DECISION PENDING; Authorization to proceed to Phase 6: owner-granted only)
```
