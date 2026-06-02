# BULWARK — Phase Execution Prompt System

**Purpose.** This is the **execution layer** for the BULWARK project. It turns each canonical roadmap phase into a self-contained, production-grade execution brief so that a Claude CLI agent can execute a phase using **only (1) the canon documents and (2) the corresponding phase prompt** — with no interpretation drift, hallucination, feature creep, or phase skipping.

**These prompts are subordinate to canon.** They operationalize; they never override. If a prompt and a canon document ever disagree, **canon wins** and the agent files an ADR (see below). The following remain **canonical and MUST NOT be modified** by execution work:
- `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` (the constitution — §-numbers cited throughout)
- `report/ROADMAP_CHANGELOG.md`
- `report/NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`
- `report/PRODUCTION_DECISION_LOG.md`

**Index of phase prompts (this folder):**
| File | Phase | One-line |
|---|---|---|
| `PHASE_0_MASTER_EXECUTION_PROMPT.md` | 0 — Foundation & Canon Lock | scaffold + ECS sim spike + data/config + backend stub + CI |
| `PHASE_1_MASTER_EXECUTION_PROMPT.md` | 1 — Core Combat Prototype | the loop + control + targeting + basic AI → **FUN GATE** |
| `PHASE_2_MASTER_EXECUTION_PROMPT.md` | 2 — Tactical Depth | terrain/formations/counters/spells/commander/AI layers → vertical slice |
| `PHASE_3_MASTER_EXECUTION_PROMPT.md` | 3 — Meta & Economy Shell | server-auth economy, progression, campaign/endless/ladder, RC |
| `PHASE_4_MASTER_EXECUTION_PROMPT.md` | 4 — Monetization & Live-ops Shell | cosmetics, pass, shop/IAP/ads, chests/gems, retention → fairness audit |
| `PHASE_5_MASTER_EXECUTION_PROMPT.md` | 5 — Soft Launch & Tuning | telemetry/tuning/hardening → **SCALE-OR-STOP GATE** |
| `PHASE_6_MASTER_EXECUTION_PROMPT.md` | 6 — Live Launch & Season 1 | global launch + S1 + live-ops cadence |
| `PHASE_7_MASTER_EXECUTION_PROMPT.md` | 7 — Post-launch (deferred) | one trigger-gated feature per invocation |

---

## How an agent runs a phase (deterministic procedure)

1. **Confirm entry.** Verify the prior phase's **Exit criteria** and any **Entry gate** are met and reported as PASS. If not → STOP, do not start.
2. **Open exactly one phase prompt** (the current phase) and read it fully.
3. **Read the canon documents it lists** (the prompt names the precise roadmap §sections).
4. **Execute only that phase's Tasks** to produce the listed **Files to create / Deliverables**.
5. **Run the Validation gate(s)** with the stated method and record results as evidence.
6. **Produce the Mandatory Final Report** (template below) and **STOP at the validation gate** for human/owner review. Do not begin the next phase.
7. **On any ambiguity / missing spec / desire to change canon → STOP and emit an ADR request.** Never invent.

---

## Universal preamble (canonical — inherited verbatim by every phase prompt)

> **BULWARK PRODUCTION AGENT — UNIVERSAL PREAMBLE.**
> You are executing **one** BULWARK development phase. Your governing authority is `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` (**the roadmap is law**); `report/PRODUCTION_DECISION_LOG.md` is **binding** (its cuts/deferrals/triggers are non-negotiable); `report/ROADMAP_CHANGELOG.md` and `report/NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md` are supporting canon. **You may not modify any of these four files.**
> **Hard rules:**
> 1. **No phase skipping** — confirm the prior phase's exit/entry gate is PASS before starting; implement only the current phase.
> 2. **No unauthorized features** — canon (roadmap §2–§12) is closed. Do not invent units, currencies, mechanics, systems, modes, or content. Anything not authorized by a cited roadmap section does not ship.
> 3. **No hidden design changes** and **no monetization changes** — design/economy/monetization are fixed by canon; altering balance philosophy, currency set, pricing, or fairness rules requires an approved ADR (you cannot self-approve).
> 4. **No canon drift** — match established conventions; reuse the one combat core and shared systems; no bespoke one-off mechanics.
> 5. **Inviolable constraints (auto-reject any work that breaks them):** readability (silhouette/clarity), fairness / no-P2W, server authority over currency, no save-state logging, perf budget, and the roadmap §15 **CUT list** (no loot boxes/gacha-for-power, no interstitials, no energy gates, no sellable power, no paid random boxes without disclosed odds + dupe protection, no client-authoritative currency, no later-phase/deferred features early).
> 6. **Stop on ambiguity** — if any instruction is unclear, under-specified, or would require a canon change, **STOP and emit an ADR request** describing the gap and your recommended options. **Never guess or fill gaps with invention.**
> 7. **ADR required for deviations** — any departure from canon (new mechanic, exception to a constraint, scope change) requires a written ADR approved per the roadmap §16 decision hierarchy. Inviolable constraints are **non-overridable even by an ADR**.
> 8. **Evidence-first reporting** — every "done" claim must cite the authorizing roadmap §section AND the concrete artifact/test result that proves it. No unverified or aspirational claims.
> 9. **Stop at the gate** — produce only this phase's deliverables, run the validation gate, file the Mandatory Final Report, then **STOP**. Do not proceed to the next phase.

---

## Conventions

**ADR (Architecture/Design Decision Record).** When STOP-on-ambiguity or a deviation occurs, create `docs/adr/ADR-<phase>-<NNN>-<slug>.md` with: Context · Decision needed · Options (with tradeoffs) · Recommendation · Owner to approve (per roadmap §16) · Status (Proposed). Do **not** implement the change until Status = Approved.

**Evidence format.** `[OBS]` reproducible artifact/test (cite path/command/result) · `[INF]` reasoned (state confidence) · `[BLOCKED]` cannot proceed (state why + ADR ref).

**Engineering structure** (implementation scaffolding — NOT design canon, so the agent may create it): `Assets/_Game/` (ECS sim + gameplay), `Assets/_Services/` (meta/UI/backend clients), `Assets/_Game/Sim/{Components,Systems,Authoring}` (ECS), `ProjectSettings/`, `Packages/`, `.github/workflows/` (CI), `docs/adr/`, `reports/phase-<n>/`. The roadmap §12 boundary (ECS = battle sim only; MonoBehaviour/UGUI = UI/meta) is **canon** and must be honored.

**Reporting location.** Each phase writes its Mandatory Final Report to `reports/phase-<n>/FINAL_REPORT.md` and prints a copy to the agent's final message.

---

## Mandatory final report template (canonical — used by every phase)

```
# BULWARK — Phase <N> Final Report

## 1. Phase Summary
   (1–3 sentences: what this phase was, outcome, gate status headline)

## 2. Work Completed
   - <task> — status (Done/Partial/Blocked) — evidence [OBS]/[INF]/[BLOCKED] + roadmap §ref

## 3. Files Created
   - <path> — purpose

## 4. Files Modified
   - <path> — what changed (NOTE: canon docs must NOT appear here)

## 5. Validation Results
   - <gate/criterion> — PASS/FAIL — method + measured result (evidence)

## 6. Known Issues
   - <issue> — severity — impact

## 7. Risks
   - <risk> — likelihood/impact — mitigation/owner

## 8. ADRs Raised
   - <ADR id> — topic — status (Proposed/Approved/Rejected)

## 9. Recommendations
   - <next action / readiness for next phase / rescope advice>

## 10. Gate Status
   - <GATE n>: PASS / FAIL / BLOCKED — against the phase's exit criteria
   - Authorization to proceed to Phase <N+1>: GRANTED / WITHHELD (+reason)
```

A phase is **not complete** until its Final Report exists with an explicit Gate Status. A **FAIL/BLOCKED** gate halts the pipeline and escalates per the phase's Escalation rules.

---

*This system file documents the layer. The per-phase prompts are self-contained (they inline the preamble and report template) so an agent never needs this file to execute — it needs only the canon documents and the one phase prompt. Documentation only; no implementation, no design changes.*
