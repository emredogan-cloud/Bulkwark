# ADR-2-001 — Conditional Authorization for Phase 3 Prior to GATE 2 Runtime Validation

- **Status:** Accepted (owner decision, 2026-06-03)
- **Phase:** 2 → 3 transition
- **Relates to:** ADR-0-001, ADR-0-002 (conditional Phase-0), ADR-1-001 (conditional Phase-2)
- **Decided by:** repo owner / Game Director (acceptance of risk); Technical Architect (runtime caveat)

## Context / acknowledgements
The owner acknowledges, on the record:
- **GATE 1 remains OPEN** (the FUN verdict was never runtime-evaluated).
- **GATE 2 remains DEFERRED** (the external-playtest bar was never run).
- Phase 2 has been **authored and canon-verified but not runtime-validated**.
- **FormationMember wiring remains deferred** (formations authored but membership not yet assigned).
- The **commander/spell buff stacking** question requires an LSD decision before GATE 2 sign-off.

## Accepted risks (recorded)
- **Combat readability remains unverified** (no build/playtest).
- **Balance assumptions remain provisional** (all values LSD-owned, untuned).
- **Runtime/performance behavior remains unknown** (no Unity/device).

## Decision
Proceed with **Phase 3 (Meta & Economy Shell) authoring exactly as defined in the roadmap**
(§13 Phase 3, 3.1–3.5; §6/§7/§9/§12), under these binding constraints:
1. **No Phase 4 work** (no monetization/shop/IAP/ads/battle pass/chests).
2. **No monetization expansion beyond roadmap canon.**
3. **No economy inflation** (the 4 MVP currencies only — Gold/Silver/Gems/PassXP; no Honor/Event tokens, those are Phase 7; provisional values stay within the §9 discipline).
4. **No new factions.**
5. **No new commanders.**
6. **No undocumented mechanics.**
7. **No canon modifications.**

## Required actions before Phase 3 completion (owner-mandated)
- **Open an ADR resolving commander/spell buff stacking** → see **ADR-2-002**.
- **Document the FormationMember wiring plan** → see `docs/design/FormationMember_wiring_plan.md`.
- **Preserve all deferred runtime validations** (Phase-0/1/2 + GATE 1/GATE 2 deferrals remain;
  Phase 3 adds its own and asserts no runtime gate as PASS).

## Consequences
- Phase 3 proceeds author-only; `reports/phase-3/FINAL_REPORT.md` will mark GATE 3 and all
  runtime-dependent checks **DEFERRED**, with **authorization to Phase 4 = WITHHELD**.
- The inviolable Phase-3 constraints (SERVER AUTHORITY over currency, NO save-state logging,
  CAPPED upgrades / no P2W) are NOT relaxed by this ADR — only validation *timing* is deferred.
- Per ADR-1-001 §9 (carried forward): any Phase-3 discovery contradicting a prior-phase
  assumption is documented immediately via ADR.
