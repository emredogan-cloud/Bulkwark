# ADR-5-002 — GATE-5 Monetization Floor (Blended D30 LTV ≥ Target CPI) — DECISION REQUEST

- **Status:** **PROPOSED — PENDING OWNER/LP DECISION** (not yet Accepted; no value set)
- **Phase:** 5 (Soft Launch & Tuning) — **prerequisite to GATE 5**
- **Relates to:** ADR-5-001 (conditional Phase-5 authorization); roadmap §13 GATE 5; §10 (KPIs); §16 (governance)
- **Owner of this decision:** **Live-ops/Product + Game Director** (§16). The production agent **must NOT set the value** (§15.6 — no invented values).

## Inherited validation debt (preserved — context for the gate)
GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED · GATE 4 PASS (static) · GATE 5 DEFERRED. None is closed by
this ADR; GATE 5 additionally requires this floor to be Accepted **and** real soft-launch data.

## Why this ADR exists (roadmap-mandated)
Roadmap §13 GATE 5 (line 344) makes the monetization floor **"exact floor set by an ADR before P5;
STOP-blocking."** (Reinforced: `ROADMAP_CHANGELOG.md` line 77; `PHASE_5_MASTER_EXECUTION_PROMPT.md` §F/§G.)
No prior ADR sets it. **GATE 5 cannot be evaluated until this ADR is Accepted with concrete values.** This
document frames the decision and leaves the numbers to the owner/LP.

## The decision to make (fill in the **TBD**s — owner/LP)
GATE 5 SCALE recommendation requires **all** of:
| Metric | Roadmap reference bar | **Floor to ratify (TBD — owner/LP)** |
|---|---|---|
| **D1 retention** | ~40% | `TBD%` |
| **D7 retention** | ~18% | `TBD%` |
| **Blended D30 LTV ≥ target CPI** | the STOP-blocking monetization floor | **target CPI = `$TBD`; required blended D30 LTV = `$TBD`** |

Additional parameters the owner/LP should pin (so the gate is unambiguous + un-gameable):
- **CPI definition** (which channels/geos; blended vs per-channel) and the **measurement window** (D30 cohort).
- **LTV model** (realized D30 vs predicted-D30; the prediction method if used).
- **Minimum cohort size / soft-launch geo set / minimum live duration** before the gate is read.
- **Confidence bar** (e.g. the metric must hold on ≥ N cohorts / a stated CI), to avoid a noise-driven pass.

## Constraints (binding, regardless of the values chosen)
- The floor governs **SCALE-vs-STOP only**; it **does not** authorize any fairness/P2W change (§J, inviolable)
  — a below-floor result is resolved by **rescope/STOP or more cosmetic-content velocity + retention tuning**,
  **never** by selling power.
- **No faking metrics to pass the gate** (§15.8, exec-prompt §J). A below-floor outcome is a valid, honest STOP.
- The agent records the owner/LP's values here verbatim; it neither proposes nor defaults a number.

## Consequences
- Until this ADR is **Accepted with values**, GATE 5 remains **un-evaluable** (a STOP-blocking prerequisite),
  and Phase 5 cannot reach its exit even once a live soft launch exists.
- When the owner supplies the values, this ADR is updated to **Accepted**, and the GATE-5 evaluation in
  `reports/phase-5/FINAL_REPORT.md` references these exact floors.

> **ACTION REQUIRED (owner/LP):** ratify the D1/D7/CPI/LTV floor values + definitions above. Reply with the
> numbers (or "Set the GATE-5 LTV floor" with values) and I will record them and flip this ADR to Accepted.
