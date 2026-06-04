# BULWARK — Phase 4 Pre-Flight Report

**Date:** 2026-06-04 · **Authorization:** ADR-4-001 (conditional, following the green Unity validation).
**Pre-flight per ADR-4-001:** canon re-read (roadmap / changelog / decision log + the Phase-4 prompt);
state reconstructed; scope/objectives/dependencies/risks/gate-impacts below. **No implementation yet.**

## 0. Canon re-read (done)
- `BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` §6 (outfit-class cosmetics + cosmetic-safety + clarity mode),
  §8 (ethical chests), §9 (gem economy + "gems CANNOT"), §10 (IAP value ladder + ethical rules), §13 Phase 4.
- `ROADMAP_CHANGELOG.md` (dispositions: opaque chests/gacha **[CUT]**, interstitials **[CUT]**, P2W **[CUT]**).
- `PRODUCTION_DECISION_LOG.md` §1 (CUT: loot boxes/gacha, interstitials, energy gates, P2W, paid random boxes) — **binding, principled (no revisit trigger)**.
- `docs/execution/PHASE_4_MASTER_EXECUTION_PROMPT.md`.

## 1. Current project state (reconstructed)
**Completed (authored, canon-verified, integration-audited, committed to `bulwark-clean`):**
- **Phase 0** Foundation (ECS sim core, data schemas, 3-tier config resolver, BaaS/analytics seams, CI).
- **Phase 1** Core combat prototype (mine→train→push→statue; Iron Pact 4→6 units; control/possess; influence-map targeting + §4 modifier chain; basic AI).
- **Phase 2** Tactical depth (terrain + flank/back; full 5×4 counter matrix + formations; Ashen Horde → 12 units; 12 spells draft-3; 2 capped commanders; budgeted AI + difficulty; 3 maps).
- **Phase 3** Meta & economy shell — **server-authoritative** 4-currency wallet (`IServerEconomy`/`Wallet`/`DistributedLedgerCache`), capped progression (`Upgrades`/`Progression`), Campaign Act 1 (20) + Endless + async ghost ladder, `EconomyResolver` + analytics `Events`.
- **Infra:** PlayFab adapters authored behind the `PLAYFAB_SDK` define (`IBackendClient`/`IServerEconomy` + CloudScript `economy.js`); **CI/CD GREEN** — compile 0 errors, EditMode 3/3 + PlayMode 4/4, **Android APK built**, artifacts produced (run 26901181289).

**Active blockers (accepted per ADR-4-001):**
- **GATE 1 (FUN) OPEN** — the APK builds, but the game is not yet wired into `MainScene` with content (`BattleBootstrap` inspector refs), so there is no on-device fun verdict.
- **GATE 2 / GATE 3 DEFERRED** — external playtest / server-validated-economy runtime not yet executed.
- **PlayFab live integration DEFERRED** — SDK install + CloudScript/Title Data deploy (owner/runtime).

**Deferred gates:** GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED. Owner accepts this validation debt (ADR-4-001).

## 2. Phase 4 scope
Roadmap §13 **Phase 4 (4.1–4.5) + GATE 4 only** — the **ethical, cosmetic-led monetization + live-ops shell**.
Server-authoritative entitlements (built on the Phase-3 wallet/backend seam). **No gameplay/balance/combat changes.**

## 3. Objectives (1:1 with §13 Phase 4)
| Sub | Objective | Canon |
|---|---|---|
| **4.1** | Outfit-class cosmetics (palette/material/trim/VFX over **locked silhouettes**) + ranked **clarity mode** | §6 |
| **4.2** | **Battle Pass S0** — 50-tier dual track (free + premium), earn-by-play, server-owned progress | §10 |
| **4.3** | Shop + **transparent IAP** value ladder ($0.99→$99.99) + **opt-in rewarded ads** (no interstitials) + first-purchase offer | §10 |
| **4.4** | **Chests** — disclosed odds, open timers + slot cap, **cosmetic+currency only (no power)**, **dupe→shard** protection; §9 gem rules ("gems CANNOT" enforced) | §8, §9 |
| **4.5** | Retention — daily/weekly quests, login streak, **basic weekend modifiers** (not the Phase-7 event engine) | §8, §10 |

## 4. Dependencies
- **Entry (canon):** Phase 3 GATE 3 PASS. **Status:** GATE 3 is DEFERRED, not PASS — entry is **conditionally waived by ADR-4-001** (owner accepts the debt). Phase 4 is authored on the Phase-3 server-authoritative wallet/entitlement seam, which exists.
- Builds on: `Bulwark.Services.Economy` (IServerEconomy/Wallet), `Bulwark.Data` (Currency, schemas), `Bulwark.Services.Config`/`Analytics`. Entitlements validated server-side via the same CloudScript-backed seam (DEFERRED to run, like Phase 3).

## 5. Risks
| Risk | Severity | Mitigation (binding) |
|---|---|---|
| **P2W / sellable-power creep** | CRITICAL (inviolable) | GATE 4 fairness audit + §15 CUT list (hard); every entitlement is cosmetic/convenience-only; automated "no power in chest/shop/pass" assertion. |
| **Readability break via cosmetics** | CRITICAL (inviolable) | §6 silhouette/size/hitbox/animation-timing/VFX-readability/faction-color **lock**; ranked **clarity mode**; cosmetic-safety check. |
| **Loot-box / paid-random-box drift** | HIGH | CUT list — chests are **earned, disclosed-odds, cosmetic-only, dupe-protected**; no paid random boxes; gems skip timers (convenience only). |
| **Dark-pattern / FOMO / interstitials** | HIGH | opt-in rewarded ads only; no interstitials; no energy gates; honest pricing; transparent value ladder. |
| **New currency / currency soup** | MED | **4 MVP currencies ONLY** (no Honor/Event — Phase 7); no new currency introduced. |
| **Pass grindiness** | MED | earn-by-play dual track; §10 caps; data-driven, RC-tunable. |
| Unvalidated runtime (debt) | MED | entitlement server-validation + ads SDK are DEFERRED (like Phase 3); authored against seams, marked DEFERRED. |

## 6. Gate impacts
- **GATE 4 (FAIRNESS AUDIT)** is the Phase-4 exit gate: **zero P2W + readability intact + disclosed odds**. Any monetization touching power/readability is an automatic FAIL → STOP + reject (I cannot self-approve). The static fairness audit can be PASS/FAIL here; live entitlement/ads validation is DEFERRED.
- **GATE 1/2/3** remain as-is (OPEN/DEFERRED) — Phase 4 does not touch combat/economy-runtime; it adds the monetization shell only.
- **No Phase 5** (soft launch) work; Phase 4 stops at GATE 4.

## 7. Execution plan (per ADR-4-001 required behavior)
For each subphase 4.1–4.5: **implement → adversarially verify (vs §6/§8/§9/§10 + CUT list) → integration audit → commit → report**.
Hard stops: a contradiction with roadmap/ADRs/decision-log → **STOP + conflict report**; any power/readability touch → **STOP + reject**. Phase 4 stops at **GATE 4**; **no Phase 5**.

## 8. Constraints checklist (ADR-4-001, acknowledged & binding)
No Phase 5 · no roadmap/canon edits · no unauthorized monetization (fixed by §8/§9/§10) · **no P2W** · **no currencies beyond the canon 4** · no faction expansion · **no commander power-budget changes** · cosmetics gameplay-safe · server-authoritative entitlements.

**Pre-flight complete. Proceeding to Phase 4 implementation (4.1 → 4.5), stopping at GATE 4.**
