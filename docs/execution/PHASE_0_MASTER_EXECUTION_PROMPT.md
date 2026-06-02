# BULWARK — Phase 0 Master Execution Prompt: Foundation & Canon Lock

**Mission.** Stand up the BULWARK technical foundation: a building Unity 6 project, the ECS battle-sim core proven at perf budget, the data-driven content + 3-tier RemoteConfig resolver, backend service stubs, and CI — with **zero game-design or content work**. This phase makes later phases buildable; it ships no gameplay.
**Scope.** Roadmap §13 **Phase 0 (0.1–0.4)** only, under the §12 technical canon. Nothing from Phase 1+.
**Inputs (read before acting).** `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` §0(authority), §12(tech canon — hard constraints), §13 Phase 0, §9(currency/config resolver pattern); `report/NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md` §9(tech table); `report/PRODUCTION_DECISION_LOG.md` §3(GATE: determinism deferred, BaaS for MVP).

> **UNIVERSAL PREAMBLE (binding).** The roadmap is law; the decision log is binding; you may NOT modify the four canon docs. **No phase skipping** (this is Phase 0; nothing later). **No unauthorized features** — canon §2–§12 is closed; invent no units/currencies/mechanics/systems. **No hidden design or monetization changes.** **No canon drift** — honor the §12 ECS-vs-MonoBehaviour boundary. **Inviolable constraints:** readability, fairness/no-P2W, server authority over currency, no save-state logging, perf budget, §15 CUT list. **Stop on ambiguity → emit an ADR request (`docs/adr/`); never guess.** **ADR required for any deviation** (inviolable constraints are non-overridable). **Evidence-first reporting.** **Stop at the validation gate**; do not start Phase 1.

## A. Context
BULWARK's two technical upgrades over the recovered original are an **ECS/DOTS deterministic-capable battle sim** and a **server-authoritative economy** (roadmap §4, §12). Phase 0 lays only the foundation for these; per decision-log §3 the sim ships **non-deterministic** for MVP (replays deferred to Phase 7) and the backend uses a **managed BaaS** (PlayFab/Nakama). No gameplay, art, or balance exists yet.

## B. Objectives (map 1:1 to roadmap §13 Phase 0)
1. **0.1** Reproducible Unity 6 LTS + URP-2D project building on CI.
2. **0.2** ECS battle-sim core spike: one unit moves and attacks another; proven to run **≥200 units at a stable frame on a mid-range phone**.
3. **0.3** Data model (ScriptableObject schema for units/spells/balance) + the **3-tier RemoteConfig resolver** (RC → typed SO → literal), designer-editable, reflected by the sim.
4. **0.4** Backend service stubs via BaaS: auth, profile read/write, config delivery; analytics event stub.

## C. Dependencies
- **Entry:** none (first phase). Confirm the four canon docs exist and are readable.

## D. Files expected to exist
- The four canon docs under `report/`. No prior build artifacts.

## E. Files to create (engineering scaffolding — not design canon)
- `ProjectSettings/`, `Packages/manifest.json` (Unity 6 LTS, URP, Entities/DOTS, Addressables).
- `Assets/_Game/Sim/{Components,Systems,Authoring}/` (ECS sim core).
- `Assets/_Game/Data/{Schemas}/` (ScriptableObject schemas: UnitDef, SpellDef, BalanceConfig — schemas only, **no populated content**).
- `Assets/_Services/{Config,Backend,Analytics}/` (3-tier resolver, BaaS client, analytics client — MonoBehaviour side per §12).
- `.github/workflows/ci.yml` (build + test gate).
- `docs/adr/` (empty, ready), `reports/phase-0/FINAL_REPORT.md`.

## F. Tasks
1. Initialize the Unity 6 LTS / URP-2D project; pin packages; commit; make CI build it green (0.1).
2. Implement a minimal ECS sim (Components: Position, Health, Team, AttackState; Systems: Move, Attack) with a headless harness that spawns N units; **measure frame time at N=200 on a mid-range device profile** (0.2).
3. Define the SO schemas (UnitDef/SpellDef/BalanceConfig) as **empty-of-content typed containers**; implement the **3-tier resolver** so a value resolves RC→SO→literal; prove a designer-edited SO value changes sim behavior, and an RC override supersedes it (0.3).
4. Integrate BaaS: auth (device/platform), profile read/write, server-delivered config; stub an analytics event emit. Confirm a round-trip (0.4).
5. Honor §12: ECS only in `Assets/_Game/Sim`; UI/meta/services in MonoBehaviour/`_Services`. **Do not** add gameplay content, currencies, or design.

## G. Validation gates
- **0.1 gate:** CI build green; project opens.
- **0.2 gate:** ≥200 units at a stable frame on the mid-range target (record FPS/frame-ms).
- **0.3 gate:** designer SO edit → sim reflects it; RC override → supersedes SO; literal fallback when both absent.
- **0.4 gate:** auth + profile read/write + config fetch + analytics emit all round-trip.
- **Exit Phase 0 gate:** all sub-gates PASS.

## H. Deliverables
Building project + CI; ECS sim spike + perf evidence; SO schemas + working 3-tier resolver; BaaS auth/profile/config + analytics stub; `reports/phase-0/FINAL_REPORT.md`.

## I. Risks
- ECS over-architecting (mitigate: minimal components/systems only). Perf miss at 200 units (mitigate: profile early; ADR if budget needs revisiting). BaaS lock-in (acceptable for MVP per decision log).

## J. Forbidden actions
- No gameplay/content/art/balance. No units, spells, currencies, or maps. No determinism/replays (deferred §7). No custom backend (BaaS only for MVP). No UI/meta features. No canon edits. No deviation from the §12 ECS boundary without an ADR.

## K. Exit criteria & stop conditions
- **Exit:** sim core + data pipeline + backend stub + CI all present and all sub-gates PASS (roadmap §13 "Exit Phase 0").
- **Stop conditions:** STOP and file the Final Report when all sub-gates pass (then await authorization for Phase 1), OR STOP immediately on any ambiguity/blocker with an ADR. Do not start Phase 1.

## Escalation rules
- Perf-budget infeasibility, determinism question, or BaaS choice → ADR to the **Technical Architect** (§16). Any pressure to add gameplay/content → reject; it belongs to later phases.

## L. Mandatory final report (write to `reports/phase-0/FINAL_REPORT.md` and print)
```
# BULWARK — Phase 0 Final Report
## 1. Phase Summary
## 2. Work Completed         (task — Done/Partial/Blocked — evidence [OBS]/[INF] + §ref)
## 3. Files Created
## 4. Files Modified         (canon docs MUST NOT appear)
## 5. Validation Results     (0.1/0.2/0.3/0.4 — PASS/FAIL — method + measured result, incl. 200-unit frame time)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised
## 9. Recommendations
## 10. Gate Status           (Exit Phase 0: PASS/FAIL/BLOCKED; Authorization to proceed to Phase 1: GRANTED/WITHHELD)
```
