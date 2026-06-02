# ADR-0-002 — Conditional Phase-0 acceptance due to unavailable Unity/device/BaaS runtime validation

- **Status:** Accepted (owner decision, 2026-06-02)
- **Date:** 2026-06-02
- **Phase:** 0 → 1 transition
- **Supersedes status of:** the BLOCKED gate verdicts in `reports/phase-0/FINAL_REPORT.md`
- **Relates to:** ADR-0-001 (environment & IP/PII blockers)
- **Decided by:** repo owner / Game Director (acceptance); Technical Architect (runtime caveat)

## Context

Phase 0's four validation gates (0.1 CI-green, 0.2 ≥200-unit frame budget, 0.3
in-editor designer-edit→sim reflection, 0.4 BaaS round-trip) require a runtime that
the authoring environment does not have: no Unity 6 editor/build chain, no mid-range
test device, no BaaS project/credentials (ADR-0-001, Blocker A). Per the roadmap's
evidence-first / no-faked-success rule (§15.8), those gates were honestly reported
**BLOCKED**, and authorization to start Phase 1 was withheld pending an owner decision.

The owner has reviewed ADR-0-001, accepted it, chosen remediation **R1** (a clean
BULWARK-only repository, now created), and **directed work to proceed to Phase 1**
with the runtime-blocked validations explicitly treated as **deferred** rather than
as failures or as fabricated passes.

## Decision

1. **Reclassify the Phase-0 runtime-blocked gates from BLOCKED to DEFERRED.** DEFERRED
   means: the deliverable is authored and committed, but its acceptance test depends on
   a runtime not yet available, and the test **must be executed before the gate can be
   asserted PASS**. DEFERRED is *not* PASS. No gate is claimed to have run green.
2. **Grant conditional authorization to proceed to Phase 1.** This is an explicit,
   owner-approved exception to the normal "Exit Phase 0 = all sub-gates PASS" rule
   (§13). It is conditional on the deferred validations being executed once tooling
   exists; if any then FAILs, the affected Phase-0/Phase-1 work is revisited.
3. **Carry a standing validation-debt obligation.** A single consolidated runtime
   validation pass (CI green; 200-unit frame-ms on a mid-range device; in-editor
   resolver proof; BaaS round-trip; Phase-1 functional checks + the GATE 1 fun verdict)
   is owed and tracked. Until it runs, all runtime-dependent gates remain DEFERRED.
4. **No inviolable constraint is waived.** This ADR defers *validation timing only*. It
   does not relax readability, fairness/no-P2W, server-authority, perf budget, the §15
   CUT list, the no-phase-jumping rule, or the canon-closed rule — all remain binding.

## Scope of "deferred" (precise)

| Item | Pre-ADR | Post-ADR |
|---|---|---|
| 0.1 CI green / project opens | BLOCKED | **DEFERRED** (workflow authored; needs a Unity-licensed runner) |
| 0.2 ≥200-unit stable frame | BLOCKED | **DEFERRED** (sim authored; needs editor + device) |
| 0.3 designer-edit→sim; RC override; literal fallback | PARTIAL/BLOCKED | resolver precedence **PASS (unit-tested)**; in-editor half **DEFERRED** |
| 0.4 auth+profile+config+analytics round-trip | BLOCKED | **DEFERRED** (seams authored; needs BaaS project) |
| Exit Phase 0 | BLOCKED | **CONDITIONALLY ACCEPTED** (deferred validations outstanding) |
| Authorize Phase 1 | WITHHELD | **GRANTED (conditional)** |

## Consequences

- Phase 1 may proceed and will be authored under full canon governance, but **GATE 1
  (the binding FUN gate) is itself runtime/owner-dependent** (on-device feel + an owner
  "is it fun?" verdict) and will therefore also be reported **DEFERRED**, with
  authorization to Phase 2 **WITHHELD** until a playable build is run and the owner
  renders the fun verdict. This ADR does not pre-approve passing GATE 1.
- The clean repo (R1) is the canonical project going forward; the RE workspace stays
  local-only and is never pushed.
- Validation debt is real and must be burned down before any soft-launch decision.
