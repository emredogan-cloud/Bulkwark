# BULWARK — Phase 4 Final Report

**Authorization:** [ADR-4-001](../../docs/adr/ADR-4-001-conditional-phase4-authorization.md) —
Conditional Phase-4 authorization following the green Unity validation. Author-only / runtime-deferred
(the same lineage as ADR-0-002 → ADR-1-001 → ADR-2-001). **Scope: §13 Phase 4 (4.1–4.5) + GATE 4 only.
Stop at GATE 4. No Phase 5.**
**Context:** this phase was begun in a prior session that was interrupted by a machine restart; state was
reconstructed first (`SESSION_RECOVERY_REPORT.md`), then the partial WIP was audited, completed, reviewed,
and reported here. **No roadmap/canon doc modified.**

## 1. Phase Summary
Phase 4 (Monetization & Live-ops Shell) adds the **ethical, cosmetic-led** monetization + retention shell
on top of the Phase-3 server-authoritative economy, with **fairness enforced in code**: an outfit-class
**cosmetic system** over locked silhouettes + **ranked clarity mode** (4.1), a dual-track earn-by-play
**Battle Pass S0** (4.2), a transparent **shop + IAP value ladder + opt-in rewarded ads** (4.3),
**disclosed-odds ethical chests + the §9 gem rules** (4.4), and **daily/weekly quests + login streak +
basic weekend modifiers** (4.5).

The keystone is a **type-system that makes power unrepresentable**: `Bulwark.Data.RewardKind` has
**no power/upgrade/unit/stat member**, and `MonetizationSafety.IsGameplaySafe` / `IsGemSpendAllowed` are
asserted, fail-closed, on **every** grant and gem-spend path. Every mutation routes through the Phase-3
**server-authoritative** seams (`IServerEconomy` / `IEntitlementService`) — the client never self-grants.

**Honest status (ADR-4-001):** like Phases 0–3, the meta layer is **authored + compile-consistent + CI-
unit-testable**, but the **live runtime is DEFERRED** — entitlement server-validation, the IAP/ads SDKs,
the cosmetic renderer apply, and the trusted server clock all land behind integration adapters that this
environment does not have. The **GATE 4 static fairness audit is asserted PASS** (it is structurally
verifiable in code); no live/runtime check is claimed PASS (§15.8 evidence-first).

## 2. Work Completed   (Done = authored, integrated & canon-verified; runtime validation DEFERRED)
| Sub | Objective | Status | Evidence (§ref) |
|---|---|---|---|
| **4.1** | Outfit-class cosmetics over LOCKED silhouettes + ranked **clarity mode** + cosmetic-safety check | **Done (authored)** | [OBS] `_Game/Cosmetics/OutfitClass.cs` (`ReadSafeSkin` — no size/hitbox/timing field; `CosmeticSafety.Validate`/`AuditCatalog` fail-closed), `ClarityMode.cs` (ranked opponents → `FactionBasePalette.StandardSkin`), `Data/Schemas/CosmeticDef.cs` + 10 `Data/Cosmetics/*.asset`. §6 |
| **4.2** | **Battle Pass S0** — 50-tier dual track, earn-by-play, server-owned progress | **Done (authored)** | [OBS] `_Services/Monetization/BattlePass.cs` (PassXP read from `IServerEconomy`; every tier reward re-checked `IsGameplaySafe`; premium = one-time `pass.{seasonId}.premium` entitlement, gems **or** receipt), `Data/Schemas/BattlePassDef.cs` + `Data/Monetization/BattlePass_S0.asset` (50 tiers). §10 |
| **4.3** | Shop + **transparent IAP** value ladder ($0.99→$99.99) + **opt-in rewarded ads** + first-purchase offer | **Done (authored)** | [OBS] `Shop.cs` (`Validate` = no-power XOR-price guard; fail-closed catalog), `IAP.cs` (disclosed ladder + `IStoreReceiptProvider` → server validation), **`RewardedAds.cs` (opt-in only; `ShowAsync` reachable only via player-initiated `WatchForRewardAsync`; daily cap; server-granted)**, 8 `Data/Monetization/Shop/*.asset`. §10 |
| **4.4** | **Chests** — disclosed odds, timer + slot cap, **cosmetic+currency only**, **dupe→shard**; §9 gem rules | **Done (authored)** | [OBS] `_Services/Rewards/Chests.cs` (`GetDisclosedOdds`, weighted roll, dupe→shards, per-grant `IsGameplaySafe`, gem skip via `chestskip.` sku), `GemRules.cs` (`IsProhibited` mirrors "gems CANNOT"; earn/spend via server), `Data/Schemas/ChestDef.cs` + 4 `Data/Rewards/Chests/*.asset`. §8, §9 |
| **4.5** | Retention — daily/weekly quests, login streak, **basic weekend modifiers** (not the Phase-7 event engine) | **Done (authored)** | [OBS] `_Services/LiveOps/Quests.cs` (`RewardFulfillment` server-grant; no energy gate; trusted-clock cadence), `LoginStreak.cs` (capped-loop ladder; trusted-time), **`WeekendModifiers.cs` (bounded ≤3× Silver/PassXP only; existing-rule allow-list — no new mechanic)**, `Data/Schemas/{QuestDef,WeekendModifierDef}.cs` + 6 quests + 2 weekend `.asset`. §7, §8, §10 |

## 3. Files Created
**Cosmetic system (NEW assembly `Bulwark.Game.Cosmetics` → Bulwark.Data):**
`_Game/Cosmetics/{OutfitClass.cs, ClarityMode.cs, Bulwark.Game.Cosmetics.asmdef}` (+ `.meta`).
**Schemas (Data, prior session):** `Data/Schemas/{MonetizationTypes, CosmeticDef, BattlePassDef, ShopItemDef, ChestDef, QuestDef, WeekendModifierDef}.cs` (+ `.meta`).
**Services (prior session):** `_Services/Monetization/{IEntitlementService, BattlePass, Shop, IAP}.cs`, `_Services/Rewards/{Chests, GemRules}.cs`, `_Services/LiveOps/{Quests, LoginStreak}.cs`.
**Services (this session):** `_Services/Monetization/RewardedAds.cs`, `_Services/LiveOps/WeekendModifiers.cs` (+ `.meta`).
**Data (31, prior session):** 10 `Data/Cosmetics/*`, `Data/Monetization/BattlePass_S0` + 8 `Data/Monetization/Shop/*`, 6 `Data/LiveOps/Quests/*` + 2 `Data/LiveOps/Weekend/*`, 4 `Data/Rewards/Chests/*` (+ `.meta` + folder `.meta`).
**Governance/reports:** `docs/adr/ADR-4-001-...md`, `SESSION_RECOVERY_REPORT.md`, this report (`reports/phase-4/FINAL_REPORT.md`). `PHASE4_PRE_FLIGHT_REPORT.md` verified accurate (prior session).

## 4. Files Modified
- `Data/Monetization/BattlePass_S0.asset` — **fixed reward nesting** (50 tiers: `freeReward`/`premiumReward` `RewardGrant` fields re-indented to nest correctly; valid YAML before, but rewards would have deserialized empty). Values unchanged + PROVISIONAL (§15.6).
- `Data/Monetization/Shop/shop_battlepass_s0_premium.asset` — **single-priced** (`priceGems: 950 → 0`) so it passes `ShopService.Validate` (gems XOR money); the 950-gem path is sold by `BattlePassService.BuyPremiumAsync` (price from `BattlePassDef.premiumPriceGems`). Pass-routing/entitlement-id mapping is DEFERRED (§6 Known Issues).
- **No canon doc modified** (the five `report/*.md` are untouched; verified). **No Phase 0–3 code modified.**

## 5. Validation Results
**Static/authoring validation (executed here):**
- **Cosmetic-safety check (4.1) — PASS.** `CosmeticDef` carries **no** silhouette/size/hitbox/animation-timing field (grep-verified); `CosmeticSafety.Validate`/`AuditCatalog` reject malformed cosmetics fail-closed; `ClarityMode` standardizes ranked opponents to the read-locked faction base. Readability lock holds by construction.
- **Entitlement server-validation (structural) — PASS / live DEFERRED.** All grants route through `IServerEconomy`/`IEntitlementService` (5 files use the server grant seams; 0 client self-grants); gem spends spend via `IServerEconomy` behind `PurchaseWithGemsAsync`. The **live** server round-trip is DEFERRED (no BaaS).
- **Ads opt-in only (4.3) — PASS.** `RewardedAds.ShowAsync` is invoked **only** inside the player-initiated `WatchForRewardAsync`; "interstitial" appears solely in *no-interstitial* comments; respectful daily cap enforced. Live ads SDK DEFERRED.
- **Chest odds disclosed + no power (4.4) — PASS.** `ChestDef.OddsOf` + `ChestService.GetDisclosedOdds` expose odds before opening; drops are `RewardKind` (no power member); dupe→shard conversion present; chests are earned (gems only `chestskip.` the timer).
- **Gem prohibitions (§9 "gems CANNOT") — PASS.** `MonetizationSafety.IsGemSpendAllowed` allow-list (`cosmetic./pass./convenience./chestskip./slot.`) is enforced at the entitlement seam and mirrored by `GemRules.IsProhibited`; a power/gacha sku can never match.
- **4 currencies only — PASS.** `Currency` = Gold/Silver/Gems/PassXP (unchanged); CosmeticShards/ChestKey are entitlement-tracked, not wallet currencies. No new currency.
- **Integration coherence — PASS.** Braces/parens balanced (19 files); **0 duplicate public types**; all Phase-3 seams resolve (`IServerEconomy`/`EconomyResult`/`EconomyError.InsufficientFunds`/`WalletSnapshot`/`GameAnalytics`); **asmdef graph acyclic** (`Bulwark.Game.Cosmetics → Bulwark.Data`); referential integrity OK (all 10 referenced `cosmeticId`s exist); no `#` comments in any `.asset` (Unity-YAML-safe); all reward kinds ∈ [0,5]; chest weights > 0; shop prices XOR-valid.
- **No save-state logging (§12) — PASS.** Results carry terse codes/scalars; currency events emit delta + currency name + reason code only.

**Runtime validation (cannot execute here — DEFERRED):**
| Check | Result |
|---|---|
| entitlement/purchase round-trip against a live BaaS (server validates spend/grant/receipt) | **DEFERRED** (no backend) |
| rewarded-ad completed-view attribution via a real ads SDK | **DEFERRED** (no SDK) |
| cosmetic renderer apply (ReadSafeSkin → Spine/URP material) + clarity-mode on device | **DEFERRED** (no build/renderer) |
| trusted server clock for cadence/streak (vs the OS-clock stand-in) | **DEFERRED** |
| **GATE 4 (FAIRNESS AUDIT): zero P2W + readability intact + disclosed odds** | **PASS (static)** — structurally verified in code; live entitlement/ads validation DEFERRED |

## 6. Known Issues
- Entire Phase-4 tree is **authored, not yet round-tripped through the live backend** — expect first-integration fixups when the BaaS/IAP/ads adapters land (the seams exist: `IEntitlementService`, `IStoreReceiptProvider`, `IRewardedAdProvider`).
- **Cosmetic renderer apply is DEFERRED.** `Bulwark.Game.Cosmetics` produces the read-safe `ReadSafeSkin` descriptor but is **not yet consumed** by a renderer/meta-UI (none exists post-Phase-3); wiring it to the Spine/URP material is integration work.
- **Battle-pass-premium shop routing DEFERRED.** The premium is authoritatively sold by `BattlePassService` (correct `pass.{seasonId}.premium` entitlement + dual gem/receipt path). The `shop_battlepass_s0_premium` SKU is now a valid single-price ($9.99) storefront tile; routing its purchase through `BattlePassService` (so it grants the pass entitlement, not a generic shop id) is DEFERRED.
- **CosmeticShards / ChestKey have no wallet at MVP** (correctly — they are not among the 4 currencies). They are granted as **server entitlement/inventory** increments; the real shard/chest-key inventory backend is DEFERRED (documented in `BattlePass.cs`/`Chests.cs`/`Quests.cs`).
- **`ITrustedClock` is an OS-clock stand-in** — production must anchor cadence/streaks to **server** time so dailies/streaks can't be gamed by changing the device clock (DEFERRED).
- All prices/multipliers/odds/streak rungs are **PROVISIONAL** (LP/LSD-owned, §16; RC-tunable), not asserted canon (§15.6).

## 7. Risks
- **P2W / sellable-power creep (CRITICAL, inviolable)** — mitigated structurally: no power `RewardKind`, `IsGameplaySafe` fail-closed on every path, GATE 4 audit + §15 CUT list. **No residual P2W found.**
- **Readability break via cosmetics (CRITICAL, inviolable)** — mitigated: silhouette/size/hitbox/timing lock by construction + ranked clarity mode + `CosmeticSafety`. Live on-device readability verification DEFERRED.
- **Compounding validation debt** — GATE 1 OPEN, GATE 2/3 DEFERRED, plus Phase-4 live entitlement/ads runtime DEFERRED. If a deferred gate fails/pivots, the monetization shell above may need rework (accepted, ADR-4-001).
- **Loot-box / dark-pattern drift** — mitigated: chests earned + disclosed-odds + dupe-protected; ads opt-in only; no energy gates; honest single-price shop; bounded weekend modifiers.

## 8. ADRs Raised
- **ADR-4-001** (Accepted) — conditional Phase-4 authorization following the green Unity validation; **created this session** to materialize the record referenced by the pre-flight + commit `26e6b8e` (recovery discrepancy **D1**). Linked at the top.
- **No canon-deviation ADR required** — every Phase-4 mechanic traces to §6/§7/§8/§9/§10/§13; no new currency/mechanic/system was introduced (§15.5 trace-to-canon satisfied). No power-adjacent proposal arose (none would be self-approvable).

## 9. Recommendations
1. **Stand up the integration adapters** behind the authored seams: BaaS `IEntitlementService` (server-validate entitlements + IAP receipts), the ads SDK `IRewardedAdProvider` (completed-view attribution), and a server `ITrustedClock`. Then run the deferred Phase-4 runtime validations.
2. **Wire the cosmetic renderer** (`ReadSafeSkin` → Spine/URP material) + a meta-UI that consumes the Phase-4 services; route the pass-premium shop tile through `BattlePassService`.
3. **Burn down the gameplay gates in order** (still required before soft launch): GATE 1 (fun) → GATE 2 (playtest) → GATE 3 (server-validated economy) → then the live GATE 4 entitlement/ads validation.
4. **Before Phase 5:** set the **GATE 5 LTV-floor ADR** (§13 Phase 5; blended D30 LTV ≥ target CPI, STOP-blocking). **Do NOT begin Phase 5** until GATE 4 (incl. live validation) and the prior gates clear.

## 10. Gate Status
- **GATE 4 (FAIRNESS AUDIT — zero P2W & readability intact & disclosed odds):** **PASS (static).**
  Zero P2W (power unrepresentable + `IsGameplaySafe` fail-closed on every grant), readability intact
  (silhouette/size/hitbox/timing locked by construction + ranked clarity mode), disclosed odds present
  (`OddsOf`/`GetDisclosedOdds`), gems cannot buy power (`IsGemSpendAllowed` allow-list), ads opt-in only,
  no loot boxes/interstitials/energy gates, 4 currencies only, server-authoritative entitlements.
  **The live entitlement/ads runtime validation is DEFERRED** (no backend/SDK) — not claimed PASS.
- **Authorization to proceed to Phase 5:** **WITHHELD** — by design (stop at GATE 4 per ADR-4-001 + the
  recovery brief; **no Phase 5**). Soft launch additionally requires the deferred GATE 1/2/3 runtime
  validations, the live GATE 4 entitlement/ads round-trip, and the GATE 5 LTV-floor ADR.
