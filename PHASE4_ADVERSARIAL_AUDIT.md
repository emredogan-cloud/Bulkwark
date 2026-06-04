# BULWARK — Phase 4 Adversarial Pre-Push Audit

**Date:** 2026-06-04 · **Scope:** the committed Phase-4 monetization & live-ops shell (commits `dcb5d67`,
`860c07f`) **before** any push. **Method:** the Phase 1–3 verification standard (`PHASE4_REVIEW_METHODOLOGY.md`)
executed as two multi-agent workflows — **(1)** 6 independent reviewers + 5 adversarial exploit hunters +
2-lens (prosecutor/defender) verification of every material finding (**59 agents, ~3.4M tokens**); **(2)** a
focused re-verification of every repair + compile/fairness/data regression scans (**13 agents**).
**Authority:** roadmap §6/§8/§9/§10/§13/§15; ADR-4-001. **No canon doc modified during the audit or repair.**

---

## 1. Reviewer findings (round 1)

| Reviewer | Lens | Verdict | n |
|---|---|---|---|
| A — Canon | roadmap/ADR/decision-log compliance | CONCERNS | 4 |
| B — Monetization | P2W/reward/pass/shop/currency exploits | CONCERNS | 4 |
| C — Economy | grant/reward/gem/currency abuse, overflow | CONCERNS | 5 |
| D — Fairness | cosmetics/clarity/readability/silhouette | CONCERNS | 3 |
| E — Integration | refs/serialization/asmdef/build risks | CONCERNS | 3 |
| F — CI/Build | compile/test/Android/Addressables | **FAIL** | 1 |

**Exploit hunters:** Free-rewards **CONCERNS** (4) · Duplicate-rewards **CONCERNS** (3) · **P2W PASS** (2) ·
Broken-shop-state **FAIL** (5) · Invalid-battle-pass **PASS** (3).

**Totals:** 37 findings → 37 deduped → **24 material (≥MEDIUM)** → **20 confirmed** after 2-lens verification.
**By severity (deduped): 3 CRITICAL · 8 HIGH · 13 MEDIUM · 8 LOW · 5 INFO.**

### Headline result — the fairness invariant held
The **P2W** and **invalid-battle-pass** exploit hunters returned **PASS**, and every reviewer independently
confirmed the zero-P2W core is **structurally sound**: `RewardKind` has no power member (power is
*unrepresentable*), and every grant/spend path fails closed through `MonetizationSafety`. **No P2W,
sellable-power, loot-box, interstitial, energy-gate, or readability violation was found.** Every confirmed
finding was a **functional** defect, not a fairness one.

## 2. Confirmed exploit / defect findings (2-lens verified) → all repaired

| Sev | Finding | Source (independent) | Fix (see PHASE4_REPAIR_REPORT.md) |
|---|---|---|---|
| **CRITICAL** | Unquoted `: ` in 8 `.asset` scalars → Unity/Addressables YAML parse → red CI | F-ci-build | C1: quoted 14 scalars |
| **CRITICAL** | Shop grants `def.id`, never `def.contents` → buyer pays, gets nothing | broken-shop | C2: `GrantContentsAsync` + `IServerEconomy` |
| **CRITICAL** | Shop premium pass grants id `BattlePassService` never checks → paid premium locked | broken-shop | C3: `entitlementSku=pass.s0.premium` |
| **HIGH** | Every gem-priced shop SKU unbuyable (id ≠ allow-list prefix) | A, B, C, E, broken-shop, P2W (6×) | H1: allow `cosmetic_` + `entitlementSku` + `Validate` |
| **HIGH** | Chest dupe→shard payout a client-supplied uncapped param | free-rewards | H2: `ChestDef.dupeShardValue` (data-owned) |
| **MEDIUM** | Banner/emote fail `CosmeticSafety.Validate` → GATE-4 AuditCatalog FAIL | A, D, E | M1: out-of-battle exemption |
| **MEDIUM** | Chest gem-skip charged before roll can fail → gem loss | C-economy | M2: reorder (roll/safety before charge) |
| **MEDIUM** | 3 incompatible shard/chest-key id schemes → non-additive shards | C-economy | M3: unified `cosmetic.shards`/`convenience.chestkey` |
| **MEDIUM** | Shop contents use non-canonical list YAML | E-integration | M4: normalized to inline |
| **LOW** | Faction-color identity not checked vs the unit (§6) | D-fairness | L1: identity match guard |
| **MEDIUM→LOW** | Rewarded-ad daily-cap TOCTOU (overlapping watches) | duplicate-rewards | L2: `_watchInFlight` guard |

The remaining LOW/INFO findings (IAP-ladder vs shop-asset divergence, `amount==0` handling, client-supplied
chest `timerElapsed`, minimal auto-generated metas) were judged **provisional / not-exploitable / DEFERRED**
by the 2-lens verification and are documented in `PHASE4_REPAIR_REPORT.md` §5 (not changed).

## 3. Repair re-verification (round 2)

Every repair was re-verified against the **current** code by an independent verifier (is it actually closed?
any regression?), plus fresh compile/fairness/data regression scans:

- **Re-verify: 10/10 RESOLVED.** `openOrRegressed: []`. **No real regressions.**
- **Compile scan: CLEAN** (9 files; `using Bulwark.Services.Economy;` present in `Shop`; all symbols resolve;
  removed `OpenAsync` params have no callers; no dangling `ShardKey`/paramref).
- **Fairness scan: CLEAN** (5; `RewardKind` still powerless; the loosened `IsGemSpendAllowed` still cannot
  admit a non-cosmetic/power sku; every grant path still asserts `IsGameplaySafe`; no client self-grant).
- **Data scan: CLEAN** (7; referential integrity intact; `entitlementSku` correct; contents normalized;
  kinds in range; chest weights >0; 0 unquoted colon-space; m_Script GUIDs match metas).
- **Only non-INFO note:** one **LOW** robustness item — `Shop.GrantContentsAsync` ignores a per-content
  server-grant failure (no rollback). This is the **DEFERRED single-server-transaction** concern already
  documented (the real backend delivers contents atomically with the receipt). Not a fairness/build issue.

## 4. Remaining risks (post-repair)
- **DEFERRED runtime (accepted, ADR-4-001):** live entitlement/IAP/ads validation, the cosmetic renderer,
  the trusted server clock, and the additive shard ledger are not present in this environment — author-only,
  same posture as Phases 0–3. The static GATE-4 fairness audit is asserted; live validation is DEFERRED.
- **LOW:** partial-delivery window in `Shop.GrantContentsAsync` / chest skip-then-grant — both resolved by a
  real server transaction at integration (DEFERRED, documented in-code).
- **Provisional values** (prices/odds/multipliers/streak rungs) remain LP/LSD-owned, RC-tunable (§15.6).

## 5. VERDICT

**Inputs to the gate:** initial CRITICAL = 3, HIGH = 8 (the HIGH set was largely one gem-spend defect found
6×). **After repair + adversarial re-verification:** CRITICAL open = **0**, HIGH open = **0**, regressions =
**0**, compile/fairness/data scans = **CLEAN**, GATE-4 zero-P2W + readability + disclosed-odds invariants =
**intact** (never breached; confirmed by the P2W/pass hunters' PASS and the fairness re-scan).

# ✅ PUSH APPROVED

No CRITICAL or HIGH findings remain; no regression was introduced; the inviolable fairness/readability
invariants are intact. Proceed to push `origin/main` and observe CI to GREEN (STEP 7–8); deliver
`PHASE4_POST_PUSH_VALIDATION_REPORT.md` once CI is green. **Do NOT begin Phase 5.**
