# ADR-1-001 — Conditional Authorization for Phase 2 Prior to GATE 1 Runtime Evaluation

- **Status:** Accepted (owner decision, 2026-06-03)
- **Phase:** 1 → 2 transition
- **Relates to:** ADR-0-001 (environment/IP blockers), ADR-0-002 (conditional Phase-0 acceptance)
- **Decided by:** repo owner / Game Director (acceptance of risk); Technical Architect (runtime caveat)

## Context
GATE 1 (FUN) is the binding fun-check for the core combat prototype. It requires a
playable on-device build and an owner "is it fun? one more game?" verdict. The authoring
environment has **no Unity 6 runtime, no device pipeline, no BaaS** (ADR-0-001), so GATE 1
**cannot be evaluated** and was reported **DEFERRED** (not PASS/FAIL) in
`reports/phase-1/FINAL_REPORT.md`. Phase 1's *implementation* is, however, complete for
authoring purposes (authored, adversarially canon-verified, integration-audited).

The owner has elected to keep the build/validation track moving by authoring the next
phase ahead of the runtime check, accepting the associated risk explicitly.

## Decision
Authorization is granted to proceed with **Phase 2 (Tactical Depth / Vertical Slice)**
under these binding constraints:
1. Phase 2 may be **AUTHORED only**.
2. **No runtime claims** may be made.
3. **No gate may be marked PASS without evidence.**
4. **All runtime-dependent validations remain DEFERRED.**
5. **No Phase 3 work** is authorized.
6. Phase 2 must **stop at GATE 2**.
7. **GATE 1 remains OPEN and unresolved.**
8. A future **Unity/device validation pass is still mandatory**.
9. Any discovery that **invalidates Phase 1 assumptions** must be documented immediately via ADR.

Execution authorities (unchanged, binding): `BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`,
`ROADMAP_CHANGELOG.md`, `PRODUCTION_DECISION_LOG.md`, `PHASE_2_MASTER_EXECUTION_PROMPT.md`.
Execution rules: no Phase-3 features; no factions beyond roadmap scope (Iron Pact + Ashen
Horde only); no monetization expansion; no scope creep; no canon changes; no undocumented
design decisions; deliverables must remain fully roadmap-compliant.

## Accepted risk (recorded)
The owner **accepts** that:
- Phase 2 is built on **Phase-1 assumptions that have not been runtime-validated** — the
  core loop, control feel, targeting, and AI fairness are authored but unproven on device,
  and **GATE 1's fun verdict is still open**. If GATE 1 later FAILs or requires a pivot, a
  portion of Phase-2 work (depth layered on that core) **may need rework or be discarded**.
- The compounding **validation debt** (Phase-0 + Phase-1 + now Phase-2 runtime/perf/playtest
  checks) grows until a Unity/device pass burns it down. Phase-2's GATE 2 (external playtest:
  ≥~40% session-2 return AND majority "readable & fun") is **also DEFERRED** under this ADR.
- This risk is taken deliberately to parallelize authoring against an unavailable runtime; it
  is **not** a waiver of any inviolable constraint (readability, fairness/no-P2W, server
  authority, perf budget, §15 CUT list) and **not** a pre-approval of passing any gate.

## Consequences
- Phase 2 proceeds author-only; `reports/phase-2/FINAL_REPORT.md` will mark GATE 2 and all
  runtime checks **DEFERRED**, with **authorization to Phase 3 = WITHHELD**.
- This ADR is **linked from the Phase 2 report** (per the owner's instruction).
- Constraint 9 stands: a Phase-2 finding that contradicts a Phase-1 assumption → immediate ADR.
