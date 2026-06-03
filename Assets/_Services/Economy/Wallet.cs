// BULWARK — CLIENT wallet service (Phase 3 §13 3.1, §9, §12).
// HARD RULE #1 (INVIOLABLE): the client NEVER mutates an authoritative balance locally. This
// service holds a READ-ONLY CACHE of the LAST SERVER-CONFIRMED Silver/Gems/PassXP balances and
// updates that cache ONLY from an IServerEconomy response. Every Spend/Grant round-trips through
// the server seam; on success the cache adopts the server's NewBalance; on failure the cache is
// untouched. There is NO public setter for a balance — SetBalance is impossible by construction
// (any direct-set attempt returns/throws an error). Gold is NOT held here (in-battle, §9).
//
// HARD RULE #2 (INVIOLABLE): NEVER log balances. BalanceChanged carries the currency + new value
// for UI binding only; this service performs NO logging of wallet state, and analytics emission
// (if wired) MUST NOT include balance payloads (§12). We expose only deltas to subscribers.
//
// PURE C# (no UnityEngine) so it is CI-unit-testable like ConfigResolver (§12). It is a Bulwark
// .Services MonoBehaviour-free service (economy/meta layer, §12 boundary — NOT ECS battle-sim).
// SCAFFOLD STATUS: authored, NOT compiled (ADR-0-001 / ADR-2-001). DEFERRED: live server
// round-trip + analytics-land (no backend here).
//
// ADOPT-SNAPSHOT NOTE (review): AdoptSnapshot is a SERVER-WINS adopt. Callers must only pass a
// snapshot obtained from IServerEconomy.GetWalletAsync (RefreshFromServerAsync) or a CloudSync
// reconcile — NEVER a client-fabricated WalletSnapshot — so it can't become a backdoor to set
// balances. This is an integration discipline (see integrationNotes), not a relaxation of RULE #1.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;

namespace Bulwark.Services.Economy
{
    /// <summary>Args for <see cref="Wallet.BalanceChanged"/>. Currency + server-confirmed value
    /// ONLY — enough to refresh UI, never a full wallet payload (§12).</summary>
    public readonly struct BalanceChangedArgs
    {
        public readonly Currency Currency;
        public readonly long NewBalance;
        public BalanceChangedArgs(Currency currency, long newBalance)
        {
            Currency = currency; NewBalance = newBalance;
        }
    }

    /// <summary>
    /// CLIENT-side wallet. Caches the last server-confirmed persistent balances and exposes
    /// getters + a change event for UI. The authority is <see cref="IServerEconomy"/>; this type
    /// is a thin, trust-the-server façade. It refuses to represent Gold and refuses any direct
    /// balance mutation (HARD RULE #1).
    /// </summary>
    public sealed class Wallet
    {
        private readonly IServerEconomy _server;

        // The CACHE: last server-confirmed balances. Private + mutated ONLY via ApplyServerBalance,
        // which is itself only called with a value that CAME FROM the server response. There is no
        // path for client arithmetic to write here.
        private readonly Dictionary<Currency, long> _cache = new Dictionary<Currency, long>
        {
            { Currency.Silver, 0L },
            { Currency.Gems,   0L },
            { Currency.PassXP, 0L },
        };

        /// <summary>Raised after the cache adopts a server-confirmed balance (UI binding). Never
        /// carries a full wallet payload (§12).</summary>
        public event Action<BalanceChangedArgs> BalanceChanged;

        public Wallet(IServerEconomy server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        // ---------------------------------------------------------------- read (cache getters)

        /// <summary>Last server-confirmed Silver (cache). NOT authoritative until next sync.</summary>
        public long Silver => _cache[Currency.Silver];
        /// <summary>Last server-confirmed Gems (cache).</summary>
        public long Gems   => _cache[Currency.Gems];
        /// <summary>Last server-confirmed PassXP (cache).</summary>
        public long PassXP => _cache[Currency.PassXP];

        /// <summary>Cached balance for a persistent currency. Throws for Gold (not held here, §9).</summary>
        public long GetCached(Currency c)
        {
            if (c == Currency.Gold)
                throw new InvalidOperationException(
                    "Gold is in-battle/non-persistent (§9) and is NOT held by the client Wallet.");
            return _cache.TryGetValue(c, out var v) ? v : 0L;
        }

        // ---------------------------------------------------------------- write (server only)

        /// <summary>
        /// FORBIDDEN by HARD RULE #1: the client may never set an authoritative balance directly.
        /// Present only to make the prohibition explicit and self-documenting — it ALWAYS throws.
        /// Balances change exclusively via <see cref="SpendAsync"/>/<see cref="GrantAsync"/>/
        /// <see cref="RefreshFromServerAsync"/>, i.e. only from a server-confirmed response.
        /// </summary>
        public void SetBalance(Currency currency, long amount)
            => throw new InvalidOperationException(
                "Client cannot set a balance directly (HARD RULE #1: server authority over currency). " +
                "Use SpendAsync/GrantAsync/RefreshFromServerAsync — the cache only adopts the server's response.");

        /// <summary>
        /// Spend via the server. On success, adopts the server's NewBalance into the cache and
        /// raises BalanceChanged; on failure, returns the server's EconomyResult unchanged and the
        /// cache is NOT touched. The client performs NO local debit math (HARD RULE #1).
        /// </summary>
        public async Task<EconomyResult> SpendAsync(Currency c, long amount, string reason)
        {
            EconomyResult result = await _server.TrySpendAsync(c, amount, reason);
            if (result.Ok) ApplyServerBalance(c, result.NewBalance);
            return result;
        }

        /// <summary>
        /// Grant via the server (sources are §9 EARNED rewards: campaign first-clear / mode reward).
        /// On success, adopts the server's NewBalance; on failure the cache is untouched.
        /// </summary>
        public async Task<EconomyResult> GrantAsync(Currency c, long amount, string source)
        {
            EconomyResult result = await _server.GrantAsync(c, amount, source);
            if (result.Ok) ApplyServerBalance(c, result.NewBalance);
            return result;
        }

        /// <summary>
        /// Pull the authoritative wallet and overwrite the entire cache from it (server wins). Use
        /// on login / resume / after a reconcile. Raises BalanceChanged per changed currency.
        /// </summary>
        public async Task RefreshFromServerAsync()
        {
            WalletSnapshot snap = await _server.GetWalletAsync();
            AdoptSnapshot(snap);
        }

        /// <summary>
        /// Overwrite the cache from a server snapshot the caller already holds (e.g. one obtained
        /// during a CloudSync reconcile). SERVER WINS — this is a pure adopt, never a merge. Callers
        /// MUST pass only a snapshot from IServerEconomy.GetWalletAsync / a reconcile, never a
        /// client-fabricated one (integration discipline — see header / integrationNotes).
        /// </summary>
        public void AdoptSnapshot(WalletSnapshot snapshot)
        {
            if (snapshot.Balances == null) return;
            ApplyServerBalance(Currency.Silver, snapshot.Get(Currency.Silver));
            ApplyServerBalance(Currency.Gems,   snapshot.Get(Currency.Gems));
            ApplyServerBalance(Currency.PassXP, snapshot.Get(Currency.PassXP));
        }

        // ---------------------------------------------------------------- internal cache adopt

        /// <summary>
        /// The ONLY mutator of the cache. Its `value` argument ALWAYS originates from a server
        /// response (EconomyResult.NewBalance or WalletSnapshot) — never from client arithmetic.
        /// Gold is rejected (not held here). NEVER logs the value (§12).
        /// </summary>
        private void ApplyServerBalance(Currency c, long serverConfirmedValue)
        {
            if (c == Currency.Gold) return; // defensive: Gold is never cached (§9)
            if (!_cache.ContainsKey(c)) return; // only the 3 persistent currencies are cached

            long previous = _cache[c];
            _cache[c] = serverConfirmedValue;
            if (previous != serverConfirmedValue)
                BalanceChanged?.Invoke(new BalanceChangedArgs(c, serverConfirmedValue));
        }
    }
}
