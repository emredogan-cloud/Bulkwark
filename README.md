# BULWARK

**BULWARK** is an original mobile tactical RTS-lite (Unity 6, Android/iOS): a
direct-control *mine → train → push → topple-the-statue* loop with readable lane
combat, asymmetric factions, a type×armor counter system, a draft-3 spell layer,
and ethical, cosmetic-led monetization (no pay-to-win, no loot boxes, no energy gates).

This repository is the **BULWARK forward project only** — design canon + the Unity
implementation. It contains **no third-party game binaries, decompiled code, or
runtime data**; those reverse-engineering artifacts were part of a separate, local,
educational analysis and are deliberately kept out of this repository (see
`docs/adr/ADR-0-001-environment-and-ip-blockers.md`).

## Authority order (the canon "is law")
1. `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` — production constitution / phase system
2. `report/ROADMAP_CHANGELOG.md` — lineage & disposition ledger
3. `report/PRODUCTION_DECISION_LOG.md` — binding decisions (e.g., MVP non-deterministic sim, BaaS)
4. `report/NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`, `report/NEXTGEN_RTS_SUCCESSOR_REPORT.md` — plan & vision

Canon docs are **immutable** to implementation work. Any change to closed canon
requires an ADR (`docs/adr/`) approved per the §16 governance hierarchy.

## Layout
| Path | Purpose |
|---|---|
| `report/` | Design canon (roadmap, changelog, blueprint, successor report, decision log) |
| `docs/execution/` | Per-phase Claude-CLI execution prompts (Phase 0–7) |
| `docs/adr/` | Architecture/Design Decision Records |
| `Assets/_Game/Sim/` | ECS/DOTS battle-sim hot path (§12 boundary) |
| `Assets/_Game/Data/` | ScriptableObject schemas + content (units/spells/balance) |
| `Assets/_Game/Control/` | MonoBehaviour input/control shell |
| `Assets/_Services/` | Config resolver (RC→SO→literal), BaaS + analytics seams |
| `.github/workflows/` | CI (build + test gate) |
| `reports/phase-N/` | Per-phase final reports & gate verdicts |

## Build status (honest)
Phase 0 (foundation) and Phase 1 (core combat prototype) are **authored**. They have
**not** been compiled, run, or perf-validated in the authoring environment, which has
no Unity 6 editor, test device, or BaaS project. Validation gates that require that
runtime are reported **DEFERRED** (see `docs/adr/ADR-0-002-...` and
`reports/phase-*/FINAL_REPORT.md`) — not falsely marked PASS. Open in Unity 6 LTS
(`6000.0.x`) to compile and validate.

## Engine
Unity 6 LTS · URP-2D · Entities/DOTS (battle sim only) · Addressables · server-authoritative
economy via managed BaaS for MVP.
