# BULWARK — Phase 5 Pre-Flight Report

> ## 🔁 RE-AUTHORIZATION PRE-FLIGHT (2026-06-05, after the successful Presentation Pass)
> **New authorization received** ("PHASE 5 AUTHORIZATION") with a stated objective of presentation/
> professionalization (character differentiation, battle HUD, audio/VFX/animation frameworks, polish). Mandatory
> pre-flight re-done: re-read the roadmap (§13 phase plan + gates), all ADRs, Phase 0–4 reports, all pre-Phase-5
> (V0–V2 + GATE1) reports, the Asset Migration reports, and the Presentation/Final-Polish reports.
>
> **Deliverables of this pre-flight:** `reports/phase-5/PHASE5_ENTRY_AUDIT.md` (full state/debt/readiness/risks/
> **contradictions**/stop conditions) and `docs/adr/ADR-5-003-phase5-authorization-after-presentation-pass.md`.
>
> **OUTCOME — STOP at the entry gate (honest, no constraint waived).** The pre-flight surfaces a **material
> contradiction** (detailed in the entry audit §12 + ADR-5-003): the stated objective = **new presentation
> features**, but roadmap **Phase 5 = "Soft Launch & Tuning — *tuning only, NO new features*"** validated by
> **GATE 5 (real D1/D7/D30 LTV/retention)**. Roadmap Phase 5 is **un-executable / un-fabricatable** here (no live
> soft launch; ADR-5-002 LTV floor still PROPOSED/unset), and **GATE 1 (FUN) = FAIL** remains unwaived. Per the
> authorization's own rules ("do not waive any constraint," "no fabricated PASS," "stop at the first required gate
> and report honestly"), **work stops at the Phase-5 entry decision and an owner choice is requested** (re-scope
> the requested work as a *Production Presentation* phase distinct from roadmap Phase 5 / address GATE 1 first /
> accept roadmap Phase 5 as telemetry-blocked readiness-only). **No constraint waived; no PASS fabricated; no
> presentation work mislabeled as roadmap Phase 5.** Inherited debt (GATE 1 FAIL · GATE 2/3 DEFERRED · GATE 4
> PASS-static · GATE 5 DEFERRED) is preserved. The original 2026-06-04 pre-flight (ADR-5-001) follows below.

---

**Date:** 2026-06-04 · **Authorization:** [ADR-5-001](docs/adr/ADR-5-001-conditional-phase5-authorization.md)
(conditional, following the green Phase-4 validation). **Authority (binding):** roadmap §1/§13 Phase 5/§14/§15/§16;
`ROADMAP_CHANGELOG.md`; `PRODUCTION_DECISION_LOG.md`; `docs/execution/PHASE_5_MASTER_EXECUTION_PROMPT.md`;
ADR registry. **Pre-flight per ADR-5-001 — no implementation yet.**

## 0. Canon re-read (done)
- Roadmap **§13 Phase 5** (lines 337–344: 5.1–5.3 + **GATE 5 SCALE-OR-STOP**; "**Tuning only — no new features**"),
  **§14** Phase-5 master prompt (line 372), **§1** philosophy ("Fun-gate before meta spend; **LTV-gate before scale**"),
  **§15** (no invented values; gates binding; no faked success), **§16** (GATE 5 = GD + LP decision).
- `PHASE_5_MASTER_EXECUTION_PROMPT.md` §E ("**No new gameplay/monetization code files; bug-fix patches only**";
  "RC value sets / experiment configs — values only, no new systems"), §G/§J/§K/§L.
- `ROADMAP_CHANGELOG.md` line 77 (GATE 5 "blended D30 LTV ≥ target CPI"; exact value via ADR, STOP-blocking).
- `PRODUCTION_DECISION_LOG.md` §6 (the two binding kill/rescope checkpoints: fun-gate + **soft-launch LTV gate**).
- ADR registry: ADR-0-001/-0-002/-1-001/-2-001/-2-002/-4-001/**-5-001**. **No ADR sets a numeric LTV/CPI floor.**

## 1. Current project state (reconstructed)
Phases 0–4 **COMPLETE** (authored + **CI GREEN**: compile 0 errors incl. Phase 4, EditMode+PlayMode 0 failures,
Android APK ~38 MB — run 26941891823 / sha a4c7842). **GATE 4 (fairness audit) = PASS (static).**

## 2. Scope (roadmap §13 Phase 5)
**Soft Launch & Tuning — SCALE-OR-STOP.** 5.1 limited-geo release + telemetry/funnel dashboards; 5.2
retention/monetization tuning via RemoteConfig; 5.3 balance + perf/stability hardening. **No new features —
tuning/telemetry/hardening only.** Exit = **GATE 5**.

## 3. Objectives (1:1 with §13 Phase 5)
| Sub | Objective | Deliverable (roadmap) | Exit (roadmap) |
|---|---|---|---|
| **5.1** | Limited-geo release + telemetry | **live build**, funnel dashboards | **data flowing** |
| **5.2** | Retention/monetization tuning (RC) | **tuned curves** | thresholds approached |
| **5.3** | Balance + perf/stability hardening | **stable, balanced build** | **crash-free + balanced** |
| **GATE 5** | SCALE-OR-STOP | — | **D1≥~40%, D7≥~18% AND blended D30 LTV ≥ target CPI → scale; else stop/rescope** |

**Every deliverable's exit is a runtime STATE measured from real players — not an authorable artifact.**

## 4. Dependencies
- **Entry:** GATE 4 PASS — **met (static)**. Deps: 5.1 ← GATE 4; 5.2,5.3 ← 5.1.
- **Operationally requires:** a **playable build**, a **live BaaS** (server-authoritative economy round-trip),
  an **app-store / limited-geo release channel**, and **real-player telemetry**. Builds on the Phase-3 §3.5
  analytics taxonomy (`Events.cs`) + the 3-tier RC resolver (`EconomyResolver`) — which already exist.

## 5. Inherited validation debt (PRESERVED — not closed, not fabricated)
- **GATE 1 (FUN) = OPEN** — APK builds but the game is **not wired into a playable scene with content**; no on-device fun verdict.
- **GATE 2 (playtest) = DEFERRED** · **GATE 3 (server-validated economy) = DEFERRED** · **PlayFab/BaaS = DEFERRED**.
- **GATE 4 = PASS (static)**; live entitlement/ads runtime DEFERRED.

## 6. Risks
| Risk | Severity | Note |
|---|---|---|
| **LTV below the gate** | CRITICAL (commercial) | the top roadmap risk; STOP/rescope is a valid honest outcome |
| **Faking metrics to pass GATE 5** | INVIOLABLE (forbidden) | §15.8 + §J + owner directive — never |
| **Soft-launching a non-playable / backend-less shell** | CRITICAL | GATE 1 OPEN + GATE 3 DEFERRED → no valid retention/LTV |
| **Over-tuning chasing metrics** | MED | n/a until real data |
| **Telemetry gaps undermining the decision** | MED | dashboards/funnels are definitions only here |

## 7. Gate implications
- **GATE 5 is the binding SCALE-OR-STOP checkpoint; the verdict is the OWNER's (GD+LP), informed by data** (§16, §G).
- **GATE 5 cannot be evaluated without real D1/D7/D30 retention + blended-D30 LTV** — which this environment
  cannot produce and which **must not be fabricated**.
- **Roadmap-mandated, STOP-blocking prerequisite is UNMET:** the **exact LTV floor (target CPI) must be set by a
  separate ADR *before* P5** (§13 line 344). No such ADR exists; its value is a **business decision** the agent
  is forbidden to invent (§15.6, §16). ADR-5-001 authorizes the transition but does **not** set the floor.
- GATE 1/2/3 remain OPEN/DEFERRED — Phase 5 does not touch them and must not silently close them.

## 8. Feasibility finding (adversarially verified)
Phase 5 is **categorically unlike Phases 0–4.** Those delivered **authorable artifacts** (ECS sim, services,
schemas, data) with only runtime *validation* deferred. Phase 5's deliverables **are the runtime outcomes**
(a live release, live telemetry, data-tuned curves, runtime-profiled hardening), and **GATE 5 is real-player
data** — none of which is authorable or fabricable here. A 4-lens independent review
(`phase5-feasibility-gate`) returned **3× PROCEED_SCAFFOLD_ONLY, 1× STOP_CONFLICT** — unanimous that **Phase 5
cannot be implemented to completion / GATE 5 cannot be reached** in this environment, and that the maximum
legitimately producible output is a **non-runtime readiness SCAFFOLD** (telemetry/funnel **definitions**, RC
experiment-config **templates** [values only], the report skeletons as **plans**, and the **LTV-floor ADR
request**) with the soft launch + live tuning + GATE 5 **DEFERRED** and **Phase 5 NOT marked complete**.

The Phase-5 execution prompt's standard pipeline (adversarial audit → exploit audit → repair → push → CI
failure-loop → GATE-5 validation) **does not apply** here: there is essentially no new code to audit/exploit
(§E "no new code files"), no CI gate to turn green for a docs/RC-values scaffold, and **no GATE 5 to validate**
without live data.

## 9. Conclusion — STOP + conflict report (per ADR-5-001 cl.5 / owner directive)
Proceeding to "implement Phase 5 to completion" would require either **fabricating runtime/telemetry evidence**
or **silently closing the deferred gates** — both explicitly forbidden — and depends on a **missing
STOP-blocking LTV-floor ADR** (owner's to set) and a **playable build that does not yet exist (GATE 1 OPEN)**.
This is the "roadmap/ADR contradiction → STOP + conflict report" condition. **See
`PHASE5_CONFLICT_REPORT.md`** for the precise conflict, options, and the decision required from the owner.
**No Phase-5 implementation, GATE-5 evaluation, soft launch, telemetry, or completion is asserted. Phase 6 NOT begun.**
