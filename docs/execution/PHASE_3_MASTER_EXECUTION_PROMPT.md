# BULWARK — Phase 3 Master Execution Prompt: Meta & Economy Shell

**Mission.** Make BULWARK a complete MVP game: a **server-authoritative** economy (the 4 MVP currencies), capped progression, the Campaign (Act 1) + Endless + async ghost ladder modes, and RemoteConfig live-tuning + analytics — wrapping the Phase-2 combat. Then submit to **GATE 3** (feature-complete MVP).
**Scope.** Roadmap §13 **Phase 3 (3.1–3.5)** + **GATE 3** only. No monetization yet (Phase 4).
**Inputs (read first).** Roadmap §6 (economy/progression, diminishing-returns, modes), §7 (mode specs — Classic/Endless/Tournament-ghost at P0), §9 (gem rules: earned/stored/spent/protected + the "gems CANNOT" list), §12 (server authority, RC resolver, analytics, no save-state logging), §13 Phase 3, §2 GATE 2; `report/PRODUCTION_DECISION_LOG.md` §3 (server-auth via BaaS; replays deferred → ladder uses **stat-sanity checks**, not deterministic validation).

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; canon docs immutable. **No phase skipping** (GATE 2 PASS). **No unauthorized features** (4 MVP currencies only — Gold/Silver/Gems/PassXP; no Honor/Event tokens — those are Phase 7 §9). **No hidden design/monetization changes** (no shop/IAP/ads/chests — Phase 4). **No canon drift.** **Inviolable constraints — especially SERVER AUTHORITY OVER CURRENCY and NO SAVE-STATE LOGGING.** Upgrades MUST be capped (no P2W). **Stop on ambiguity → ADR. ADR for deviations. Evidence-first. Stop at GATE 3.**

## A. Context
Phase 2 has fun, deep combat; Phase 3 gives it a reason to keep playing — persistent (capped) progression and modes — and fixes the original's trust gap by making currency/progression **server-owned** (roadmap §4/§12). The CRDT ledger is a **client cache under server authority** (§9), never the source of truth. No monetization is built here.

## B. Objectives (1:1 with roadmap §13 Phase 3)
1. **3.1** Server-authoritative wallet: the 4 MVP currencies, server validates every grant/spend.
2. **3.2** Progression: **capped** per-unit upgrade tracks + commander levels (no power beyond cap; ranked-normalization hook).
3. **3.3** Campaign **Act 1 (~20 levels)** + **Endless** with the adaptive director (§5.1/§7).
4. **3.4** **Async ghost ladder** (snapshot opponents) validated by **server stat-sanity checks** (replays are Phase 7).
5. **3.5** RemoteConfig live-tuning wired to economy/balance + the analytics event taxonomy.

## C. Dependencies
- **Entry:** Phase 2 GATE 2 = PASS. Uses the Phase-0 BaaS stubs (now made authoritative) and the Phase-2 combat/modes scaffolding.

## D. Files expected to exist
- Phase-2 vertical slice (2 factions, combat, spells, commanders, maps); Phase-0 BaaS client + 3-tier resolver + analytics stub; prior FINAL_REPORTs (gates PASS).

## E. Files to create
- `Assets/_Services/Economy/{Wallet,Upgrades,Progression}.cs` (client) + server-side validation config.
- `Assets/_Services/CloudSync/DistributedLedgerCache.cs` (client cache; **server authoritative**).
- `Assets/_Game/Modes/{Campaign,Endless,AsyncLadder}/` + `Assets/_Game/Data/Campaign/Act1/` (~20 level defs as data).
- `Assets/_Services/RemoteConfig/EconomyResolver.cs` (RC→SO→literal for economy) + `Assets/_Services/Analytics/Events.cs` (taxonomy).
- `reports/phase-3/FINAL_REPORT.md`.

## F. Tasks
1. Implement the server-authoritative wallet (Gold in-battle only/non-persistent; Silver/Gems/PassXP server-owned); every spend/grant validated server-side; client holds an obscured CRDT cache reconciled on sync (3.1, §9).
2. Implement **capped** unit-upgrade tracks (rising Silver cost, hard cap) + commander levels; enforce the cap server-side; stub ranked normalization (3.2).
3. Author Campaign Act 1 (~20 level defs as data) with first-clear **diminishing-returns** gem rewards `max(5, base−5×playCount)`; implement Endless + adaptive director (3.3).
4. Implement the async ghost ladder: capture/serve opponent snapshots; validate outcomes with **server stat-sanity checks** (no deterministic replay) (3.4).
5. Wire RemoteConfig to economy/balance values via the 3-tier resolver; implement the analytics event taxonomy; confirm a value retunes live without an app update (3.5).

## G. Validation gates
- Server validates spend/grant (reject tampered client values); upgrade caps enforced; campaign clearable + replayable with correct diminishing rewards; ladder climbable + stat-checks reject anomalies; RC retunes a value live; analytics events land.
- **GATE 3:** **MVP feature-complete; economy server-validated** (roadmap §13 GATE 3).

## H. Deliverables
Server-authoritative economy + capped progression + Campaign Act 1 + Endless + async ghost ladder + RC/analytics, wrapping Phase-2 combat + `reports/phase-3/FINAL_REPORT.md`.

## I. Risks
- Server-auth bugs / currency desync (mitigate: server is source of truth; cache reconciles). Campaign difficulty tuning. Ladder snapshot fidelity. Scope creep into monetization (forbidden here).

## J. Forbidden actions
- No monetization (no shop/IAP/ads/battle pass/chests — Phase 4). No new currencies beyond the 4 MVP (Honor/Event tokens are Phase 7). No client-authoritative currency. No save-state logging. No deterministic replays. No P2W (caps mandatory). No new units/spells/maps. No canon edits.

## K. Exit criteria & stop conditions
- **Exit:** all 3.x checks PASS and the build is a feature-complete MVP with a server-validated economy.
- **Stop conditions:** STOP + Final Report at GATE 3; STOP on ambiguity with ADR. Do not start monetization.

## Escalation rules
- Currency/server-authority design questions → Technical Architect + Lead Systems Designer (§16). Economy tuning → Lead Systems Designer (within canon). Any P2W pressure (uncap upgrades, sell power) → **reject** (inviolable).

## L. Mandatory final report (`reports/phase-3/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 3 Final Report
## 1. Phase Summary
## 2. Work Completed     (3.1–3.5 — Done/Partial/Blocked — evidence + §ref)
## 3. Files Created
## 4. Files Modified      (no canon docs)
## 5. Validation Results  (server-auth spend/grant, upgrade caps, modes, RC retune, analytics)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised
## 9. Recommendations
## 10. Gate Status        (GATE 3: PASS/FAIL; Authorization to proceed to Phase 4: GRANTED/WITHHELD)
```
