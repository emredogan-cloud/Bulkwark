# BULWARK — Phase 4 Review Methodology (reconstructed from Phases 1–3)

**Purpose.** Reproduce, for Phase 4 **before any push**, the exact verification standard used in the
strongest Phase 1–3 reviews. Source of truth: `reports/phase-1/FINAL_REPORT.md`,
`reports/phase-2/FINAL_REPORT.md`, `reports/phase-3/FINAL_REPORT.md`, and the roadmap's own
adversarial-verification precedent (`report/ROADMAP_CHANGELOG.md` §6, a **6-critic pass**).

---

## 1. The verification workflow (what every prior phase did)
A repeatable **orchestrated pipeline**, identical in shape across Phases 1–3:

1. **Author against a fixed contract/seam** — each module is built against a frozen interface
   (Phase 1/2: the ECS component contract; Phase 3: the Phase-0 service seams). No module invents shared types.
2. **Per-module adversarial canon-verification** — every module is reviewed *assuming it is wrong*.
   Evidence it works: **"every module was flagged on first pass"** (Phase 1) — real bugs were caught
   (last-write-wins mine occupancy, statues unreachable by targeting, field/enum mismatches, missing AI in order).
3. **Repair** — fix what the adversarial pass found; re-verify.
4. **Cross-module integration audit** — reconcile shared seams to ONE canonical definition, wire update
   order cycle-free, and run static checks (below).
5. **Inviolable-constraint verification** — server authority over currency, no save-state logging, capped/no-P2W,
   readability — each explicitly re-checked.
6. **Honest gate report** — runtime-dependent checks marked **DEFERRED** (never faked); the gate verdict
   states PASS/FAIL/DEFERRED with evidence (§15.8 evidence-first / no faked success).

## 2. Reviewer roles (the critic lenses)
The roadmap itself was hardened by a **6-critic adversarial pass** (changelog §6): **canon-consistency,
internal-contradiction, hallucination/scope, phase-soundness, prompt-governance, spec-completeness.**
The phase reports applied the same spirit per module (canon compliance + integration coherence + §12
boundary + perf). Phase 4 maps these onto **six independent reviewers**:

| Reviewer | Lens | Heritage |
|---|---|---|
| **A — Canon** | roadmap/ADR/decision-log compliance; no phase-jumping; trace-to-canon; CUT list | changelog §6 canon-consistency + hallucination/scope + phase-soundness |
| **B — Monetization** | P2W / reward / pass / shop / currency exploits | Phase-4 GATE-4 fairness audit surface (§8/§9/§10/§15) |
| **C — Economy** | grant/reward paths, progression/gem/currency abuse, overflow/negative | Phase-3 server-authority audit (HARD RULE #1/#2/#3) |
| **D — Fairness** | cosmetics, clarity mode, readability, silhouette safety, ranked integrity | §6 cosmetic-safety (INVIOLABLE) |
| **E — Integration** | references, serialization/YAML, asmdefs, dependencies, build risks | Phase-1/2/3 integration audits (dup types, brace, symbols, acyclic) |
| **F — CI/Build** | compile/test/Android/Addressables risks; prior-failure regressions | FIRST_COMPILE_REPORT + UNITY_VALIDATION_REPORT taxonomies |

## 3. Audit methodology
- **Assume the implementation is wrong.** The bar is *finding* defects, not confirming success.
- **Verify SUBSTANCE, not names.** Phase 2's spell module returned plausible **placeholder stubs with good
  comments** and *fooled the per-module verifier*; it was caught only in the cross-module census and
  re-authored in full. **Lesson (binding): read the actual logic; comment claims can lie.**
- **Per-module + cross-module.** A finding must cite file + line + a concrete impact/exploit, not a vibe.
- **Trace-to-canon (§15.5).** Every feature must cite the roadmap § that authorizes it; if it can't, it's a defect.

## 4. Integration methodology (static checks every prior phase ran)
Zero duplicate type declarations · all files brace-balanced · all shared symbols resolve · **acyclic asmdef
graph** · update/dependency order coherent · shared seams deduped to ONE canonical definition · referential
integrity of data (every referenced id exists) · serialization correctness (nested-struct YAML indentation,
list-of-struct shape) · no `#` comments in `.asset` (invalid Unity YAML — a real prior CI break).

## 5. Repair methodology
Classify each confirmed finding **CRITICAL / HIGH / MEDIUM / LOW**. **Fix CRITICAL + HIGH** (optionally
MEDIUM). For each fix document **root cause → fix → residual risk**, then **re-verify** the affected area.
Precedents: Phase-3 deduped a duplicate `IServerEconomy` to one canonical seam; added a read-modify-write
**data-loss guard** so a wallet write never clobbers progression keys; Phase-3 module 3.5 failed to return
and was **authored directly** rather than left missing.

## 6. Honesty standard (non-negotiable)
Evidence-first; **no faked success** (§15.8). Runtime-dependent validation (live BaaS, IAP/ads SDK, device,
renderer) is reported **DEFERRED**, not PASS. Static/structural properties (the GATE-4 fairness invariants)
*may* be asserted PASS because they are verifiable in code. A gate is never marked PASS without evidence.

## 7. Lessons carried forward into the Phase 4 audit
- **Stub-fooling (Phase 2):** verify file substance → the Phase-4 audit reviewers + a 2-lens verification
  (prosecutor *constructs* the exploit; defender *refutes* it) must read real code, not comments.
- **Seam dedup + data-loss guard (Phase 3):** integration reviewer re-checks shared-seam usage + grant atomicity.
- **YAML / config breaks (Unity validation):** CI reviewer re-checks every `.asset` for the prior failure classes.

## 8. Application to Phase 4 (this pre-push pass)
Six reviewers (A–F) **plus five adversarial exploit hunters** (free rewards · duplicate rewards · P2W path ·
broken shop state · invalid battle-pass state) run **independently** over the committed Phase-4 code; every
**material finding (≥ MEDIUM)** is then **adversarially verified from two opposing lenses** before any
CRITICAL/HIGH is repaired. Outputs: `PHASE4_REPAIR_REPORT.md`, `PHASE4_ADVERSARIAL_AUDIT.md` (with an
explicit **PUSH APPROVED / BLOCKED** verdict), then — only if approved with no CRITICAL — push + CI
observation to GREEN + `PHASE4_POST_PUSH_VALIDATION_REPORT.md`.
