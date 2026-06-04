// BULWARK — ETHICAL CHEST service (§13 4.4, §8 ethical chests). Phase 4 meta/monetization layer.
//
// §8 (INVIOLABLE): chests are EARNED (per-battle / daily / win-streak — never purchased as a random
// box), cosmetic + currency ONLY (NO power), with an open TIMER + a SLOT CAP for gentle pacing, and
// DISCLOSED ODDS on every drop (regulatory + trust). A DUPLICATE cosmetic converts to SHARDS (dupe
// protection). Gems may ONLY SKIP the open timer (convenience — never buy power, §9). This is NOT a
// paid random box (decision-log §1 CUT: "NO paid random boxes without disclosed odds + dupe
// protection"); the box itself is earned and its odds are queryable BEFORE opening.
//
// FAIRNESS (GATE 4): every rolled reward is asserted MonetizationSafety.IsGameplaySafe BEFORE it is
// granted (fail closed). The drop table (ChestDef.drops / ChestDrop) cannot express power — its
// `kind` is the powerless RewardKind enum — so a chest is power-free BY CONSTRUCTION, and the assert
// catches any future data drift. There is NO RewardKind.Power to roll.
//
// SERVER-AUTHORITATIVE (§12, HARD RULE #1): rewards are GRANTED server-side — currency/shards via
// IServerEconomy.GrantAsync, cosmetics via IEntitlementService.GrantAsync. The chest-skip gem spend
// routes through IEntitlementService.PurchaseWithGemsAsync("chestskip....") (which debits Gems via
// the server economy + records the convenience entitlement). The client never self-grants or
// self-debits.
//
// PURE C# (no UnityEngine) so this is CI-unit-testable (§12 boundary: meta/monetization = services,
// NOT the ECS battle sim). The RNG is injectable for deterministic tests. NO save-state logging (§12).
// SCAFFOLD STATUS: authored, NOT compiled here (no Unity 6 / no BaaS — ADR-0-001 / ADR-2-001).
// DEFERRED: live server grants (IServerEconomy / IEntitlementService) + the real wall-clock open
// timer persistence land behind a BaaS adapter — none here; round-trips are simulated/stubbed.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Economy;
using Bulwark.Services.Monetization;

namespace Bulwark.Services.Rewards
{
    /// <summary>One disclosed odds row for the UI (regulatory/trust): the drop's kind/rarity + its
    /// exact probability (weight / total) + amount/cosmetic. Built from ChestDef.OddsOf — never a
    /// hidden table. Shown BEFORE the player opens (and before any timer-skip spend).</summary>
    public readonly struct DisclosedOdds
    {
        public readonly RewardKind Kind;
        public readonly Rarity Rarity;
        public readonly float Probability; // 0..1, == ChestDef.OddsOf(index)
        public readonly int Amount;        // for currency/shards
        public readonly string CosmeticId; // for kind == Cosmetic

        public DisclosedOdds(RewardKind kind, Rarity rarity, float probability, int amount, string cosmeticId)
        {
            Kind = kind; Rarity = rarity; Probability = probability; Amount = amount; CosmeticId = cosmeticId;
        }
    }

    /// <summary>Why a chest could not be opened (UX/audit; never a payload — §12).</summary>
    public enum ChestOpenReject : int
    {
        None = 0,
        TimerNotElapsed, // open timer still running and no skip was purchased (gentle pacing, §8)
        SlotCapReached,  // too many chests queued (gentle pacing, §8)
        EmptyTable,      // ChestDef has no (positive-weight) drops — config error
        UnsafeReward,    // a rolled grant FAILED MonetizationSafety.IsGameplaySafe — REFUSED (fail closed)
        SkipSpendFailed, // the gem chest-skip spend was rejected (e.g. insufficient gems / not allowed)
        GrantFailed,     // a server grant failed mid-open
    }

    /// <summary>The result of opening one chest: the (gameplay-safe) grants that landed, plus any
    /// dupe→shard conversions that were applied, or a reject reason. Carries NO balance payload (§12).</summary>
    public readonly struct ChestOpenResult
    {
        public readonly bool Ok;
        public readonly ChestOpenReject Reason;
        public readonly IReadOnlyList<RewardGrant> Grants;   // exactly what was granted (post dupe→shard)
        public readonly int DuplicatesConverted;             // count of cosmetic dupes turned into shards

        public ChestOpenResult(bool ok, ChestOpenReject reason, IReadOnlyList<RewardGrant> grants, int duplicatesConverted)
        {
            Ok = ok; Reason = reason; Grants = grants; DuplicatesConverted = duplicatesConverted;
        }

        public static ChestOpenResult Success(IReadOnlyList<RewardGrant> grants, int dupes)
            => new ChestOpenResult(true, ChestOpenReject.None, grants, dupes);
        public static ChestOpenResult Rejected(ChestOpenReject reason)
            => new ChestOpenResult(false, reason, Array.Empty<RewardGrant>(), 0);
    }

    /// <summary>
    /// Service over a single ChestDef instance. EARNED chests are queued and opened here. Open flow:
    /// (1) respect the open timer + slot cap (data, §8); Gems may SKIP the timer (convenience only);
    /// (2) RollRewards using the DISCLOSED weights (ChestDef.OddsOf); (3) convert DUPLICATE cosmetics
    /// to SHARDS (dupe protection); (4) assert each grant IsGameplaySafe and GRANT it server-side.
    /// GetDisclosedOdds() exposes the full table for the UI. NO power can be rolled (RewardKind has no
    /// power member). This is NOT a paid random box — the chest is earned and its odds are public.
    /// </summary>
    public sealed class ChestService
    {
        private readonly ChestDef _def;
        private readonly IServerEconomy _economy;
        private readonly IEntitlementService _entitlements;
        private readonly Func<int, int> _rollInRange; // [0, exclusiveMax) — injectable for deterministic tests

        // Cosmetic-skip price per tier (convenience, §9). The sku PREFIX "chestskip." is what
        // MonetizationSafety.IsGemSpendAllowed permits — it can NEVER be a power sku.
        public const string SkipSkuPrefix = "chestskip.";

        public ChestService(ChestDef def, IServerEconomy economy, IEntitlementService entitlements,
                            Func<int, int> rollInRange = null)
        {
            _def = def ?? throw new ArgumentNullException(nameof(def));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
            _rollInRange = rollInRange ?? DefaultRoll;
        }

        // ============================================================= disclosed odds (UI / trust)

        /// <summary>
        /// The full DISCLOSED odds table for this chest (§8 regulatory + trust). One row per drop with
        /// its exact probability (ChestDef.OddsOf). The UI shows this BEFORE the player opens and
        /// BEFORE any timer-skip spend — odds are never hidden. Empty when the table has no drops.
        /// </summary>
        public IReadOnlyList<DisclosedOdds> GetDisclosedOdds()
        {
            var rows = new List<DisclosedOdds>(_def.drops != null ? _def.drops.Count : 0);
            if (_def.drops == null) return rows;
            for (int i = 0; i < _def.drops.Count; i++)
            {
                ChestDrop d = _def.drops[i];
                rows.Add(new DisclosedOdds(d.kind, d.rarity, _def.OddsOf(i), d.amount, d.cosmeticId));
            }
            return rows;
        }

        // ============================================================= pacing (timer + slot cap, §8)

        /// <summary>True iff the open timer has elapsed for a chest queued at <paramref name="queuedAtUtc"/>.
        /// Gentle pacing (§8), NOT a paywall — the player may simply WAIT for free, or skip with Gems
        /// (convenience). DEFERRED: the authoritative timer lives server-side; this is a client UX check.</summary>
        public bool IsTimerElapsed(DateTime queuedAtUtc, DateTime nowUtc)
            => nowUtc - queuedAtUtc >= TimeSpan.FromMinutes(Math.Max(0, _def.openMinutes));

        /// <summary>True iff a new chest may be queued given the current count (slot cap, §8 pacing).</summary>
        public bool HasFreeSlot(int currentlyQueued) => currentlyQueued < Math.Max(0, _def.slotCap);

        /// <summary>The disclosed open-timer length (minutes) for UI.</summary>
        public int OpenMinutes => Math.Max(0, _def.openMinutes);

        /// <summary>The disclosed gem chest-skip sku for this chest (convenience-only; never power, §9).
        /// PREFIX is "chestskip." so MonetizationSafety.IsGemSpendAllowed permits it.</summary>
        public string SkipSku => SkipSkuPrefix + (string.IsNullOrEmpty(_def.id) ? _def.tier.ToString().ToLowerInvariant() : _def.id);

        // ============================================================= roll (disclosed weights)

        /// <summary>
        /// Pick ONE drop using the DISCLOSED weights (the same weights GetDisclosedOdds shows — no
        /// hidden second table). Returns the chosen drop materialized as a gameplay-safe RewardGrant.
        /// Returns false on an empty/zero-weight table. Power is unrepresentable here (RewardKind has
        /// no power member), so the result is safe by construction — the caller still asserts it.
        /// </summary>
        public bool RollRewards(out RewardGrant grant)
        {
            grant = default;
            if (_def.drops == null || _def.drops.Count == 0) return false;

            int total = 0;
            for (int i = 0; i < _def.drops.Count; i++)
                total += _def.drops[i].weight < 0 ? 0 : _def.drops[i].weight;
            if (total <= 0) return false;

            int pick = _rollInRange(total); // [0, total)
            int acc = 0;
            for (int i = 0; i < _def.drops.Count; i++)
            {
                int w = _def.drops[i].weight < 0 ? 0 : _def.drops[i].weight;
                acc += w;
                if (pick < acc) { grant = ToGrant(_def.drops[i]); return true; }
            }
            // Fallback (shouldn't happen): last positive-weight drop.
            for (int i = _def.drops.Count - 1; i >= 0; i--)
                if (_def.drops[i].weight > 0) { grant = ToGrant(_def.drops[i]); return true; }
            return false;
        }

        /// <summary>Materialize a disclosed drop into a RewardGrant (no power kind exists to produce).</summary>
        private static RewardGrant ToGrant(in ChestDrop d)
            => new RewardGrant { kind = d.kind, amount = d.amount, cosmeticId = d.cosmeticId };

        // ============================================================= open flow (earn -> grant)

        /// <summary>
        /// Open ONE earned chest. Enforces, in order: slot/timer pacing (§8) with an optional Gems
        /// SKIP (convenience-only, §9); a single disclosed-weight roll; DUPLICATE cosmetic → SHARD
        /// conversion (dupe protection, §8); a per-grant IsGameplaySafe assert (fail closed — GATE 4);
        /// and a SERVER-AUTHORITATIVE grant (§12). NO power can be granted. NOT a paid random box.
        ///
        /// <paramref name="timerElapsed"/>: whether the open timer has already elapsed for FREE (compute
        ///   via IsTimerElapsed). If false, <paramref name="paySkipWithGems"/>=true buys the convenience
        ///   skip via IEntitlementService.PurchaseWithGemsAsync(SkipSku, skipGemPrice).
        /// <paramref name="currentlyQueued"/>: queue size for the slot-cap check.
        /// DEFERRED: live server grants + authoritative timer/queue persistence (BaaS adapter).
        /// </summary>
        public async Task<ChestOpenResult> OpenAsync(
            bool timerElapsed, int currentlyQueued, bool paySkipWithGems, long skipGemPrice,
            DupeOwnershipCheck ownsCosmetic = null, int dupeToShardAmount = 10)
        {
            // --- pacing: slot cap (§8) -------------------------------------------------------
            // Opening frees a slot; we only reject if the queue is so full the chest can't exist.
            if (currentlyQueued > Math.Max(0, _def.slotCap))
                return ChestOpenResult.Rejected(ChestOpenReject.SlotCapReached);

            // --- pacing: open timer (§8) + optional Gems SKIP (convenience only, §9) ---------
            if (!timerElapsed)
            {
                if (!paySkipWithGems)
                    return ChestOpenResult.Rejected(ChestOpenReject.TimerNotElapsed); // wait for free, or skip

                // The skip sku PREFIX guarantees MonetizationSafety.IsGemSpendAllowed; the spend
                // debits Gems via the server economy (PurchaseWithGemsAsync) — never buys power.
                PurchaseResult skip = await _entitlements.PurchaseWithGemsAsync(SkipSku, skipGemPrice);
                if (!skip.Ok) return ChestOpenResult.Rejected(ChestOpenReject.SkipSpendFailed);
            }

            // --- roll using the DISCLOSED weights (§8) ---------------------------------------
            if (!RollRewards(out RewardGrant rolled))
                return ChestOpenResult.Rejected(ChestOpenReject.EmptyTable);

            // --- duplicate cosmetic -> shards (dupe protection, §8) --------------------------
            int dupes = 0;
            RewardGrant grant = rolled;
            if (_def.dupeToShards && rolled.kind == RewardKind.Cosmetic && !string.IsNullOrEmpty(rolled.cosmeticId)
                && ownsCosmetic != null)
            {
                bool alreadyOwned = await ownsCosmetic(rolled.cosmeticId);
                if (alreadyOwned)
                {
                    // Convert the duplicate into CosmeticShards (craft currency) — never a power refund.
                    grant = new RewardGrant
                    {
                        kind = RewardKind.CosmeticShards,
                        amount = Math.Max(1, dupeToShardAmount),
                        cosmeticId = null,
                    };
                    dupes = 1;
                }
            }

            // --- GATE 4: assert gameplay-safe BEFORE granting (fail closed) ------------------
            if (!MonetizationSafety.IsGameplaySafe(in grant))
                return ChestOpenResult.Rejected(ChestOpenReject.UnsafeReward);

            // --- SERVER-AUTHORITATIVE grant (§12, HARD RULE #1) ------------------------------
            bool granted = await GrantAsync(grant);
            if (!granted) return ChestOpenResult.Rejected(ChestOpenReject.GrantFailed);

            return ChestOpenResult.Success(new[] { grant }, dupes);
        }

        /// <summary>
        /// Grant a single gameplay-safe reward via the correct SERVER seam (§12): currency/shards/PassXP
        /// through IServerEconomy.GrantAsync; a cosmetic (or chest-key) through IEntitlementService.
        /// GrantAsync. The client never self-grants. Re-asserts IsGameplaySafe (defense in depth).
        /// DEFERRED: the live server round-trip (BaaS adapter).
        /// </summary>
        private async Task<bool> GrantAsync(RewardGrant g)
        {
            if (!MonetizationSafety.IsGameplaySafe(in g)) return false; // never grant an unsafe reward

            switch (g.kind)
            {
                case RewardKind.Silver:
                {
                    var r = await _economy.GrantAsync(Currency.Silver, g.amount, "chest:" + SafeId());
                    return r.Ok;
                }
                case RewardKind.Gems:
                {
                    var r = await _economy.GrantAsync(Currency.Gems, g.amount, "chest:" + SafeId());
                    return r.Ok;
                }
                case RewardKind.PassXP:
                {
                    var r = await _economy.GrantAsync(Currency.PassXP, g.amount, "chest:" + SafeId());
                    return r.Ok;
                }
                case RewardKind.CosmeticShards:
                {
                    // Shards are a server-owned craft currency. There is no Currency member for shards
                    // (the 4-currency rule, §9), so shards are an ENTITLEMENT-tracked balance: grant a
                    // shard entitlement increment server-side. DEFERRED: the BaaS shard ledger.
                    var r = await _entitlements.GrantAsync("shards." + ShardKey(g.amount), "chest:" + SafeId());
                    return r.Ok;
                }
                case RewardKind.Cosmetic:
                {
                    var r = await _entitlements.GrantAsync(g.cosmeticId, "chest:" + SafeId());
                    return r.Ok;
                }
                case RewardKind.ChestKey:
                {
                    // A convenience chest-key entitlement (opens/queues another earned chest — no power).
                    var r = await _entitlements.GrantAsync("convenience.chestkey." + SafeId(), "chest:" + SafeId());
                    return r.Ok;
                }
                default:
                    return false; // unknown kind ⇒ fail closed (protects GATE 4)
            }
        }

        // ============================================================= helpers

        private string SafeId() => string.IsNullOrEmpty(_def.id) ? _def.tier.ToString().ToLowerInvariant() : _def.id;

        // Encodes the shard amount in the entitlement key so the server ledger can apply the increment
        // without a payload beyond the count (no PII — §12). The amount is also carried by the grant.
        private static string ShardKey(int amount) => amount.ToString();

        private static readonly Random _rng = new Random();
        private static int DefaultRoll(int exclusiveMax) => exclusiveMax <= 0 ? 0 : _rng.Next(exclusiveMax);
    }

    /// <summary>
    /// Server-authoritative ownership probe for dupe protection (§8): returns true iff the player
    /// already owns the given cosmetic id. Backed by IEntitlementService.OwnsAsync at integration —
    /// passed in so ChestService stays a pure policy object. DEFERRED: live server read.
    /// </summary>
    public delegate Task<bool> DupeOwnershipCheck(string cosmeticId);
}
