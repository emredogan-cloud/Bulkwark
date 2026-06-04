# ADR-5-001 — Conditional Authorization for Phase 5 Following Successful Phase 4 Validation

- **Status:** Accepted (owner decision, 2026-06-04)
- **Phase:** 4 → 5 transition (Soft Launch & Tuning — SCALE-OR-STOP)
- **Relates to:** ADR-0-002 / ADR-1-001 / ADR-2-001 / ADR-4-001 (the conditional-authorization lineage)
- **Decided by:** repo owner / Game Director (acceptance of risk); Technical Architect (runtime/validation-debt caveat); Live-ops/Product (monetization KPIs within §9/§10)

## Context

Phase 4 (Monetization & Live-ops Shell) was authored, adversarially audited (6 reviewers + 5 exploit
hunters + 2-lens verification → 3 CRITICAL + HIGH + MEDIUM, all repaired and re-verified RESOLVED), pushed,
and validated **GREEN end-to-end in CI** (compile PASS incl. Phase 4, EditMode+PlayMode 0 failures, Android
APK ~38 MB). **GATE 4 (fairness audit) = PASS (static).**

**Evidence considered:** `PHASE4_ADVERSARIAL_AUDIT.md` (PUSH APPROVED), `PHASE4_REPAIR_REPORT.md`,
`PHASE4_POST_PUSH_VALIDATION_REPORT.md` (CI run 26941891823 / sha a4c7842 = success).

## Current project state (confirmed)
Phases 0–4 COMPLETE (authored + CI compile/tests/APK GREEN). GATE 4 PASS (static).

## Inherited validation debt (PRESERVED — not closed, not fabricated)
Per the owner's explicit instruction, these remain open and are carried forward as standing debt:
- **GATE 1 (FUN) = OPEN** — the APK builds but the game is **not yet wired into a playable scene** with
  content; there is **no on-device fun verdict**.
- **GATE 2 (playtest) = DEFERRED** — external-playtest bar never run.
- **GATE 3 (server-validated economy) = DEFERRED** — no live BaaS; economy is server-authoritative by
  construction only.
- **PlayFab/BaaS live integration = DEFERRED** (owner/runtime).
No runtime evidence is to be fabricated; no deferred gate is to be silently closed.

## Decision

**Conditionally authorize the Phase 5 PRE-FLIGHT and any *legitimately producible* Phase-5 work**, under
the same governance + quality standard that closed Phase 4, with these binding constraints:
1. **Implement Phase 5 exactly as defined** (roadmap §13 Phase 5, §14 Phase-5 prompt). **Tuning only — no new features.**
2. **No roadmap / canon / decision-log modifications.** No future-research adoption; **no import from `/future/*`** unless the roadmap explicitly authorizes it.
3. **No fabricated runtime/telemetry evidence** (§15.8). Real D1/D7/D30 retention + LTV are not assertable without a live soft launch.
4. **Stop at GATE 5** (SCALE-OR-STOP). Do **not** begin Phase 6. Await owner approval at GATE 5.
5. **Stop + conflict report** on any roadmap / ADR / decision-log / canon contradiction.

## Acknowledged prerequisite (roadmap-mandated, STOP-blocking)
Roadmap §13 GATE 5 requires the **exact monetization floor (blended D30 LTV ≥ target CPI) to be set by a
SEPARATE ADR *before* Phase 5 — explicitly "STOP-blocking."** That floor value is a **business decision**
(Live-ops/Product + Game Director, §16) and a concrete value (§15.6 forbids the agent inventing it). **This
ADR-5-001 authorizes the transition but does NOT set that floor** — a distinct LTV-floor ADR (owner/LP-set)
is an unmet prerequisite for GATE 5 evaluation. The Phase-5 pre-flight must surface this.

## Consequences
- The Phase-5 pre-flight (`PHASE5_PRE_FLIGHT_REPORT.md`) proceeds. Because Phase 5's deliverables (limited-geo
  release, live telemetry, RC tuning from live data, perf/stability hardening) and **GATE 5** depend on a
  real soft launch + real-player data that this environment cannot produce — and on a playable build that
  GATE 1 (OPEN) does not yet provide — the pre-flight will determine, with adversarial verification, whether
  any Phase-5 work is legitimately producible here or whether this is a **STOP + conflict-report** condition.
- The inviolable constraints (fairness/no-P2W, readability, server authority, no faked success) are **not**
  relaxed by this ADR — only the prior phases' validation *timing* remains deferred.
- Per ADR-1-001 §9 (carried forward): any discovery contradicting a prior assumption is recorded via ADR/conflict report immediately.
