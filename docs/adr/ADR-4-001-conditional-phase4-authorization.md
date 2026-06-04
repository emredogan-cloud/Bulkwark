# ADR-4-001 — Conditional Authorization for Phase 4 Following Successful Unity Validation

- **Status:** Accepted (owner decision, 2026-06-04)
- **Phase:** 3 → 4 transition (Monetization & Live-ops Shell)
- **Relates to:** ADR-0-001/-0-002 (conditional Phase-0), ADR-1-001 (conditional Phase-2), ADR-2-001 (conditional Phase-3), ADR-2-002 (commander/spell stacking)
- **Decided by:** repo owner / Game Director (acceptance of risk); Technical Architect (validation-debt caveat); Live-ops/Product (monetization stays within §9/§10 fairness)
- **Supersedes nothing; extends the conditional-authorization lineage** established by ADR-0-002 → ADR-1-001 → ADR-2-001.

## Context

The Phase-3 FINAL_REPORT withheld authorization to Phase 4 pending (a) a Unity build + BaaS, (b) the
deferred GATE 1/2 validations, and (c) GATE 3 runtime validation. Since then, an evidence-based
**Unity validation milestone** was achieved (the prior phases were author-only; the codebase is now
built and green in CI):

**Recorded successes (evidence):**
- **Successful Unity validation** — `UNITY_VALIDATION_REPORT.md`: compile **0 errors** (CS / SGSG
  source-generator / Burst), Entities IL post-processing + Burst AOT ran over all sim assemblies.
- **Successful tests** — EditMode 3/3 (+ Addressables stub 1/1), **PlayMode 4/4**.
- **Successful APK generation** — full IL2CPP arm64 → `launcher-release.apk`, **37,850,180 B (~38 MB)**,
  published as the `android-build` artifact.
- **Successful CI/CD** — first end-to-end **GREEN** run: `main` **26901181289** (sha `9a487da`),
  conclusion = success; all artifacts produced (APK + test-results + logs).

This converts the Phase 0–3 **compile** debt to PASS and proves the pipeline builds an installable APK.
It does **not** clear the gameplay/runtime gates.

## Acknowledgements (on the record)

- **GATE 1 (FUN) remains OPEN** — the APK builds, but the game is not yet wired into a playable scene
  with content (`BattleBootstrap` inspector refs + a populated SubScene), so there is no on-device fun verdict.
- **GATE 2 remains DEFERRED** — the external-playtest bar was never run.
- **GATE 3 remains DEFERRED** — the server-authoritative economy is server-authoritative *by
  construction* but has not been runtime-validated against a live BaaS.
- **PlayFab live integration remains DEFERRED** — SDK install + `PLAYFAB_SDK` define + CloudScript /
  Title Data deploy are owner/runtime work (adapters authored, excluded behind the define).
- The roadmap's stated **entry condition for Phase 4 is GATE 3 = PASS**; GATE 3 is DEFERRED, not PASS.

## Accepted risks (recorded)

- **Combat fun, tactical readability, and economy server-validation remain runtime-unproven** — Phase 4
  is layered on a core whose FUN/playtest/economy gates are still open/deferred; if a later gate fails or
  pivots, the monetization shell layered above may need rework. **Accepted.**
- **Phase-4 entitlement/ads runtime is unproven** — entitlement server-validation and the rewarded-ads
  SDK are DEFERRED (authored against seams), exactly as Phase 3 deferred its live BaaS round-trip.

## Decision

**Conditionally AUTHORIZE Phase 4 (Monetization & Live-ops Shell) authoring/implementation exactly as
defined in the roadmap** (§13 Phase 4, 4.1–4.5; §6/§8/§9/§10/§12), built on the existing Phase-3
server-authoritative wallet/entitlement seam, under these **binding constraints**:

1. **No Phase 5 work** (no soft-launch/telemetry/scale work). Stop at **GATE 4**.
2. **No roadmap or canon modifications** (the five `report/*.md` are immutable).
3. **No hidden monetization changes** — monetization is fixed by **§8/§9/§10**; any pricing/fairness
   change needs an ADR the agent cannot self-approve.
4. **No P2W / no sellable power** — every entitlement is cosmetic / convenience / currency only.
5. **No new currencies** — the **4 MVP currencies only** (Gold/Silver/Gems/PassXP); Honor/Event are Phase 7.
6. **No commander power-budget changes**, **no faction expansion**, **no new units/spells/maps**.
7. **No future-research content** — the `/future/` track is advisory-only and must never be implemented or touch canon.
8. **Cosmetics gameplay-safe (§6)** — no silhouette/size/hitbox/animation-timing/read change; **ranked clarity mode** enforced.
9. **Server-authoritative entitlements (§12)** — the client never self-grants; gem spends pass the §9 "gems CANNOT" guard.
10. **§15 CUT list is a hard prohibition** — no loot boxes/gacha-for-power, no interstitials, no energy gates, no paid random boxes without disclosed odds + dupe protection.

## Required behavior during Phase 4 (owner-mandated)

- For each sub-phase 4.1–4.5: **implement → adversarial review (vs §6/§8/§9/§10 + CUT list) →
  integration audit → fix → commit → report.**
- **Evidence-first / no faked success (§15.8):** runtime-dependent checks are reported **DEFERRED**, not
  PASS (consistent with Phases 0–3). The **static fairness audit** (GATE 4: zero P2W, readability intact,
  disclosed odds) may be asserted PASS/FAIL because it is structurally verifiable in code.
- **Hard stops:** any contradiction with the roadmap / an ADR / the decision log / canon →
  **STOP + conflict report**; any monetization that touches power or breaks readability →
  **STOP + reject** (inviolable, non-self-approvable).

## Consequences

- Phase 4 proceeds; `reports/phase-4/FINAL_REPORT.md` will assert the **GATE 4 static fairness audit**
  result and mark all runtime-dependent checks (live entitlement server-validation, ads SDK, on-device)
  **DEFERRED**. **Authorization to Phase 5 is not granted by this ADR** — soft launch additionally
  requires the deferred GATE 1/2/3 runtime validations and the **GATE 5 LTV-floor ADR** (§13 Phase 5).
- The inviolable Phase-4 constraints (**FAIRNESS/NO-P2W, READABILITY/clarity mode, SERVER AUTHORITY over
  entitlements, disclosed odds**) are **NOT relaxed** by this ADR — only validation *timing* (live
  entitlement/ads round-trips) is deferred.
- Per the ADR-1-001 §9 rule (carried forward): any Phase-4 discovery contradicting a prior-phase
  assumption is documented immediately via ADR rather than silently resolved.
