# BULWARK — Phase 7 Master Execution Prompt: Post-Launch Deferred Features (one trigger-gated feature per invocation)

**Mission.** Implement **exactly one** deferred post-launch feature, **only after its decision-log revisit-trigger has fired**, fully integrated under existing canon — no bundling, no scope expansion. This is a **reusable template**: instantiate it once per feature (7.1 … 7.7).
**Scope.** Roadmap §13 **Phase 7** — a single sub-feature per invocation. The deferred set: **7.1** ranked seasons + deterministic sim/replays (+ *Honor* currency) · **7.2** clans / clan-wars · **7.3** 3rd faction · **7.4** biomes · **7.5** commander collection · **7.6** event engine (+ *Event tokens*) · **7.7** real-time skirmish.
**Inputs (read first).** Roadmap §7, §12, §13 Phase 7 + the relevant canon section for the chosen feature (e.g., §6 commanders for 7.5; §8/§10 for event economy; §9 for Honor/Event tokens; §4/§12 for ranked/determinism); **`report/PRODUCTION_DECISION_LOG.md` §1–§3 — the revisit-TRIGGER for the chosen feature (mandatory).**

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; canon docs immutable. **No phase skipping** — Phase 6 live + the chosen feature's **revisit-trigger MUST have fired** (verify in the decision log; if not → STOP). **No unauthorized features** — implement only the ONE named feature; no bundling, no extras. **No hidden design/monetization changes.** **No canon drift** (reuse the one combat core + server-authoritative economy). **Inviolable constraints in force** (readability, fairness/no-P2W, server authority, no save-state logging, perf budget, §15 CUT — note real-time PvP (7.7) is CUT until determinism + audience justify it via ADR). **Stop on ambiguity → ADR. ADR for deviations. Evidence-first. One feature, then STOP.**

## A. Context
These features were deliberately deferred (decision log §2–§3) to keep the MVP focused and the trust/perf model sound. Each carries a **revisit-trigger** (a condition like "MVP retention validated", "deterministic sim landed", "backend capacity ready"). A Phase-7 invocation is legitimate **only** when that specific trigger is satisfied. Real-time skirmish (7.7) additionally requires the deterministic sim (7.1) and an approved ADR.

## B. Objectives (per single chosen feature)
1. Confirm the feature's **revisit-trigger has fired** (cite the decision-log entry + the evidence the trigger is met).
2. Implement that one feature, integrated under canon (examples):
   - **7.1** deterministic (fixed-point/lockstep) sim + **replay-validated ranked seasons/leagues** + the **Honor** currency (§9: earned-only, cosmetic-only, server-authoritative).
   - **7.2** clans + async clan goals/wars (server-backed, moderated).
   - **7.3** a 3rd asymmetric faction (≤ canon caps, balance-tractable).
   - **7.4** biomes with terrain mechanics tying to §4.
   - **7.5** commander collection + talent economy (capped power; cosmetic monetization only).
   - **7.6** event engine + **Event tokens** (§9 rules) + calendar.
   - **7.7** real-time skirmish — **only** on deterministic sim + an approved ADR.

## C. Dependencies
- **Entry:** Phase 6 live **and** the chosen feature's decision-log trigger = MET. Plus any feature-specific prerequisite (e.g., 7.7 ← 7.1 deterministic sim).

## D. Files expected to exist
- The full live game (Phases 0–6) + live-ops; all prior FINAL_REPORTs PASS; the decision-log entry naming the trigger.

## E. Files to create
- Feature-scoped code/data under the appropriate `Assets/_Game` or `Assets/_Services` path (e.g., 7.1 → `Assets/_Game/Sim/Deterministic/`, `Assets/_Services/Ranked/`, Honor in the wallet per §9).
- `reports/phase-7/<feature>/FINAL_REPORT.md`.

## F. Tasks
1. **Trigger verification first** — read the decision log; confirm and cite the fired trigger. If not fired → STOP (do not implement).
2. Implement the single feature under canon; if it introduces a currency (Honor/Event tokens), implement it per §9 (earned-only, cosmetic-only, server-authoritative, never power).
3. Integrate without forking the combat core or weakening any inviolable constraint; respect perf budget.
4. Validate against the feature's own success criteria (e.g., 7.1: replays deterministically reproduce battles + ranked outcomes server-validated).

## G. Validation gates
- Trigger confirmed MET (cited). Feature works to its success criteria. **Fairness + readability + server-authority preserved** (no regressions). Perf within budget. For 7.1: deterministic reproduction verified; ranked outcomes replay-validated server-side.

## H. Deliverables
The one integrated feature + its validation evidence + `reports/phase-7/<feature>/FINAL_REPORT.md`. Nothing else.

## I. Risks
- Determinism complexity (7.1 — the hardest; the prerequisite for 7.7). Balance explosion (7.3/7.4). Moderation/abuse (7.2). Power-creep via commander collection (7.5 — enforce caps). Scope bundling (forbidden). Real-time netcode cost (7.7 — only if justified).

## J. Forbidden actions
- No implementing a feature whose trigger has not fired. No bundling multiple Phase-7 features in one invocation. No P2W via collection/ranked (caps + normalization mandatory). No new currency that buys power. No fairness/readability/server-authority regressions. No canon edits. 7.7 without 7.1 + an approved ADR.

## K. Exit criteria & stop conditions
- **Exit:** the single feature is integrated, validated to its criteria, with no inviolable-constraint regression, and reported.
- **Stop conditions:** STOP immediately if the trigger is not met (with a note); STOP + Final Report after the one feature; STOP on ambiguity with an ADR. Do not chain into another Phase-7 feature.

## Escalation rules
- Trigger ambiguity / readiness → Game Director + the relevant owner (§16). New currency or monetization aspect → Live-ops/Product within §9/§10. Determinism/real-time architecture → Technical Architect (+ ADR for 7.7).

## L. Mandatory final report (`reports/phase-7/<feature>/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 7 (<feature>) Final Report
## 1. Phase Summary       (which single feature; trigger cited as MET)
## 2. Work Completed       (feature tasks — Done/Partial/Blocked — evidence + §ref)
## 3. Files Created
## 4. Files Modified        (no canon docs)
## 5. Validation Results    (trigger confirmation; feature success criteria; fairness/readability/server-authority/perf no-regression; 7.1: determinism + replay validation)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised           (incl. any required for 7.7 real-time)
## 9. Recommendations
## 10. Gate Status          (Feature integrated: YES/NO; inviolable constraints preserved: YES/NO; ONE feature only: confirmed)
```
