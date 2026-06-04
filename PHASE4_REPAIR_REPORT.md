# BULWARK — Phase 4 Repair Report (pre-push)

**Date:** 2026-06-04 · **Input:** the multi-agent adversarial audit (`phase4-adversarial-audit`: 6 reviewers
+ 5 exploit hunters + 2-lens verification; **37 findings, 24 material, 20 confirmed — 3 CRITICAL, 8 HIGH,
13 MEDIUM**). **Authority:** roadmap §6/§8/§9/§10/§13/§15; ADR-4-001. **No canon doc modified.**
**Policy:** fix all **CRITICAL + HIGH**; fix the confirmed **MEDIUM** logic defects; apply cheap **LOW**
hardening that strengthens inviolables; document the rest. Many HIGH findings were the *same* defect
reported independently by up to 6 reviewers (high signal) — collapsed to one fix below.

> **Important fairness note:** the audit confirmed the **zero-P2W invariant is structurally sound** —
> `RewardKind` has no power member, every grant/spend path fails closed through `MonetizationSafety`.
> **None of the defects below is a P2W/fairness violation.** They are *functional* bugs (revenue-blocking,
> paid-goods-not-delivered, a build break, a free-soft-currency inflation, an audit-integrity gap).

---

## 1. CRITICAL (3 — all fixed)

### C1 — Unquoted `: ` in 8 `.asset` YAML scalars → Unity/Addressables parse error → red CI
- **Root cause:** hand-authored string values containing a colon-space (`m_Name: Daily: Win 3`,
  `shownValueNote: Premium track: …`) were left unquoted; a plain YAML scalar may not contain `: `, so
  Unity's asset import / Addressables `BuildPlayerContent` would abort — the exact class as the earlier
  `#`-comment break (UNITY_VALIDATION_REPORT §2).
- **Fix:** single-quoted all 14 offending values across 8 files (6 quest `m_Name`+`displayName`,
  `shop_starter_s0` + `shop_battlepass_s0_premium` `shownValueNote`) — the form Unity's own serializer emits.
- **Risk after:** none; quoting is always-valid YAML. Verified 0 unquoted colon-space scalars remain.

### C2 — Shop purchase granted only `def.id`, never `def.contents` (buyer pays, gets nothing)
- **Root cause:** `ShopService.PurchaseWithGemsAsync`/`PurchaseWithMoneyAsync` granted a single entitlement
  keyed on the item id and never iterated `def.contents` — so gem packs (contents = Gems) and the starter
  bundle (Gems + cosmetic) validated but delivered nothing.
- **Fix:** added `IServerEconomy` to `ShopService`; added `GrantContentsAsync(def, source)` (mirrors
  `BattlePass.GrantRewardAsync`/`Chests.GrantAsync`) that routes each content `RewardGrant` to the correct
  server seam (currency→`IServerEconomy.GrantAsync`, cosmetic→`IEntitlementService.GrantAsync`,
  shards/chest-key→unified ids), re-asserting `IsGameplaySafe` per grant; called on both purchase-success paths.
- **Risk after:** a content grant could fail after the purchase commits (partial delivery) — the real backend
  delivers contents in the **same server transaction as the receipt** (DEFERRED). Documented.

### C3 — Shop premium pass granted an entitlement `BattlePassService` never checks (paid premium unusable)
- **Root cause:** the shop sold the pass as id `shop_battlepass_s0_premium`, but `BattlePassService` checks
  `pass.{seasonId}.premium` (`pass.s0.premium`) → a $9.99 purchase left the premium track locked.
- **Fix:** added `ShopItemDef.entitlementSku` (the canonical id a purchase grants); `Shop` now grants
  `EntitlementSkuOf(def)`; set `shop_battlepass_s0_premium.entitlementSku = pass.s0.premium`. Verified it
  equals `BattlePassService.PremiumEntitlementId` for season `s0`.
- **Risk after:** none structurally; the shop money path and `BattlePassService` now grant the identical id.

---

## 2. HIGH (fixed)

### H1 — Every gem-priced shop SKU was unbuyable (id never matched the §9 gem-spend allow-list)  *(6 reviewers)*
- **Root cause:** `IsGemSpendAllowed` accepts only the **dotted** prefixes (`cosmetic.`/`pass.`/…), but the
  authored cosmetic/banner ids use the **underscore** form (`cosmetic_…`) and shop ids use `shop_…`; `Shop`
  forwarded the raw id, so `StubEntitlementService.PurchaseWithGemsAsync` rejected every gem spend.
- **Fix (3 parts):** (a) `MonetizationSafety.IsGemSpendAllowed` now also admits `cosmetic_` (covers skins,
  `cosmetic_banner_*`, `cosmetic_emote_*` — all category-safe; power remains unrepresentable); (b) gem-priced
  shop assets set `entitlementSku` to the **cosmetic id** (so the spend both passes the gate AND grants the
  cosmetic under the *same id* pass/chest grants use — unified ownership/dupe-protection); (c) `Shop.Validate`
  now **fails a gem-priced item whose `EntitlementSkuOf` isn't allow-listed** (`BadGemSku`) so a future
  mis-authored gem SKU surfaces in the rejected list instead of being silently dead.
- **Risk after:** the allow-list is looser by one category prefix; re-verified it still cannot admit any
  non-cosmetic/power sku. Crimson (800💎) and banner (150💎) are now purchasable + delivered.

### H2 — Chest duplicate→shard payout was a client-supplied, uncapped parameter (free soft-currency inflation)
- **Root cause:** `ChestService.OpenAsync(..., int dupeToShardAmount = 10)` took the dupe→shard value as a
  caller argument used verbatim (`Math.Max(1, dupeToShardAmount)`) → a caller could mint up to `int.MaxValue`
  shards per open.
- **Fix:** removed the parameter; added **`ChestDef.dupeShardValue`** (DATA/SERVER-owned); `OpenAsync` uses
  `Math.Max(1, _def.dupeShardValue)`. (Also moved the gem **skip price** to `ChestDef.skipGemPrice` so the
  client can't set its own convenience price.)
- **Risk after:** none — the payout is now data/server-owned. `OpenAsync` has no callers yet (DEFERRED meta-UI), so the signature change is safe.

---

## 3. MEDIUM (confirmed logic defects — fixed)

### M1 — Banner/emote cosmetics failed `CosmeticSafety.Validate` (empty `archetypeId`) → broke the GATE-4 AuditCatalog claim
- **Root cause:** my `CosmeticSafety.Validate` unconditionally required a non-empty `archetypeId`, but
  out-of-battle variants (banner/emote, §6) legitimately skin **no unit** → 2/10 authored cosmetics failed.
- **Fix:** `Validate` now exempts out-of-battle variants from the archetype requirement (via `IsOutOfBattle`);
  in-battle variants still require it. `OutfitClass.TryResolve` now returns `false` for out-of-battle variants
  (enforcing "never rendered on a unit" by construction, not just by data convention).
- **Risk after:** none — `AuditCatalog` over all 10 cosmetics now returns empty (PASS), and the GATE-4
  cosmetic-safety claim is honest.

### M2 — Chest gem-skip charged before the roll/grant could fail (currency loss, no refund)
- **Root cause:** `OpenAsync` debited the gem skip first, then rolled — an `EmptyTable`/`UnsafeReward`
  outcome lost the gems with nothing granted.
- **Fix:** reordered — `RollRewards` + dupe + `IsGameplaySafe` now run **before** the skip charge; gems are
  spent only once a grantable reward is guaranteed.
- **Risk after:** a narrow residual window remains (skip ok → grant fails); the real fix is a single
  skip+grant **server transaction** (DEFERRED, documented in-code).

### M3 — CosmeticShards/ChestKey grants used 3 incompatible entitlement-id schemes (non-additive shards)
- **Root cause:** `BattlePass` used `inv.cosmeticshards.{source}`, `Chests` used `shards.{amount}`, `Quests`
  used `cosmetic.shards` — so an additive craft currency was modeled three ways; the chest scheme keyed by
  amount collapsed repeated awards.
- **Fix:** unified all four services (`BattlePass`, `Chests`, `Shop.GrantContentsAsync`, `Quests` already
  canonical) to the stable id **`cosmetic.shards`** / **`convenience.chestkey`** with the quantity carried in
  the audit source, so the server **adds** to the running balance (§8 "no dead pulls").
- **Risk after:** the additive shard ledger itself is DEFERRED (no quantity-bearing entitlement at MVP), but the id scheme is now coherent.

### M4 — Shop content assets used a non-canonical list-of-struct YAML form
- **Root cause:** gem-pack/starter `contents` used a `-` (dash) on its own line then indented fields — valid
  YAML but used nowhere else in the repo (a deserialization risk given Unity's strict parser).
- **Fix:** normalized to the inline `- kind:` form Unity itself emits (matching chest `drops`/pass `tiers`).
  (Also cleared the now-redundant `contents` on the crimson/banner items, which deliver via `entitlementSku`.)
- **Risk after:** none.

---

## 4. LOW (cheap hardening of inviolables — applied)

- **L1 — Faction-color-identity lock (§6 inviolable):** `ClarityMode.ResolveSkin` now requires the owned
  cosmetic's `archetypeId` **and** `faction` to match the unit being skinned, else fails closed to the
  standardized base — so a mis-authored cosmetic can never project the wrong faction read onto a unit
  (the lock is now enforced, not merely a data-consistency accident).
- **L2 — Rewarded-ad daily-cap TOCTOU:** `RewardedAdService.WatchForRewardAsync` now holds a `_watchInFlight`
  re-entrancy guard (cleared in `finally`) so overlapping opt-in watches can't each pass the cap check before
  either increments — the respectful daily cap can't be exceeded by double-tapping.

---

## 5. Documented / DEFERRED (intentionally not changed)

| Item | Disposition |
|---|---|
| IAP `DefaultLadder` ids/amounts diverge from the shop gem-pack assets | The ladder is explicitly **illustrative/provisional** (RC/SO-overridable, §15.6); the shop SO assets are authoritative at integration. Documented, not force-aligned. |
| `IsGameplaySafe` permits `amount==0` → a 0-amount currency reward no-ops/fails | Not exploitable (no power; no negative); benign. Noted. |
| `ChestService.OpenAsync(timerElapsed, …)` takes the timer decision as a caller bool | The authoritative open-timer is **server-owned** (DEFERRED), same posture as Phase 3; the skip *price* is now data-owned. Documented. |
| Residual chest skip-then-grant-fail window | Needs a single server transaction (DEFERRED). Documented in-code. |
| 5 minimal auto-generated `.asset.meta` (no importer block) | Unity regenerates the importer block on import; the GUID (the only load-bearing part) is preserved. Not a defect. |

---

## 6. Files changed in the repair pass
**Code:** `Assets/_Game/Data/Schemas/{MonetizationTypes,ShopItemDef,ChestDef}.cs`,
`Assets/_Services/Monetization/{Shop,BattlePass,RewardedAds}.cs`, `Assets/_Services/Rewards/Chests.cs`,
`Assets/_Game/Cosmetics/{OutfitClass,ClarityMode}.cs`.
**Data:** 8 `.asset` (quoted scalars: 6 quests + 2 shop); `shop_{cosmetic_crimson,banner_ironpact_s0,
battlepass_s0_premium}.asset` (entitlementSku + contents); 5 gem-pack/starter `.asset` (contents normalized).
**No Phase 0–3 code touched; no canon doc touched.**

## 7. Post-repair static verification (executed)
Brace balance OK on all 9 edited C# files · no callers of the changed `ShopService`/`OpenAsync` signatures ·
no stale param refs · gem allow-list admits `cosmetic_` · 3 shop `entitlementSku` set + pass id matches
`PremiumEntitlementId` · 0 unquoted colon-space scalars · shop contents normalized. **Adversarial
re-verification (workflow `phase4-repair-reverify`) result is recorded in `PHASE4_ADVERSARIAL_AUDIT.md`.**
