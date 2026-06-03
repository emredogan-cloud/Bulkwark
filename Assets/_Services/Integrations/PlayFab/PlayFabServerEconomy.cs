// BULWARK — PlayFab adapter for IServerEconomy + ICommanderEconomy (Phase-3 §13 3.1/3.2, §9, §12).
// SERVER-AUTHORITATIVE BY CONSTRUCTION: every spend/grant/upgrade/commander-level op is a CALL to
// a PlayFab CloudScript handler (backend/playfab/cloudscript/economy.js) that runs on PlayFab's
// servers with the title secret. The CLIENT never computes or writes a balance/level: it sends the
// INTENT (currency + delta + reason, or unit/stat/from-level/cap) and adopts ONLY the server-
// confirmed result. The server RE-DERIVES costs from server-owned config and RE-CHECKS caps/funds/
// plausibility (anti-tamper) — the client's cost/amount is treated as a request, never trusted.
// Gold is NOT representable here (in-battle / non-persistent, §9). NEVER logs save state (§12).
//
// ACTIVATION: in Bulwark.Services.PlayFab.asmdef (defineConstraints:["PLAYFAB_SDK"]) — excluded
// from the build until the PlayFab SDK + PLAYFAB_SDK define are present (backend/playfab/README.md).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity / no PlayFab SDK here — ADR-0-001/-2-001).
// DEFERRED: real server round-trips run in CI / on device against Title 101A1B + the CloudScript.

#if PLAYFAB_SDK
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using Bulwark.Data;
using Bulwark.Services.Economy;

namespace Bulwark.Services.Integrations.PlayFab
{
    /// <summary>
    /// PlayFab-backed canonical economy seam. Implements BOTH <see cref="IServerEconomy"/> and
    /// <see cref="ICommanderEconomy"/> over CloudScript. Holds a read-only client cache of the last
    /// SERVER-CONFIRMED balances for display; the cache is never authoritative.
    /// </summary>
    public sealed class PlayFabServerEconomy : IServerEconomy, ICommanderEconomy
    {
        // PlayFab Virtual Currency codes (server-owned). Gold is deliberately absent (§9).
        private static string Vc(Currency c) => c switch
        {
            Currency.Silver => "SL",
            Currency.Gems   => "GM",
            Currency.PassXP => "PX",
            _ => null, // Gold (or unknown) — not a server wallet currency
        };

        private readonly Dictionary<Currency, long> _cache = new Dictionary<Currency, long>();

        // ---- CloudScript invocation (server authority lives in the handler) ----
        private async Task<ServerEcoResponse> Exec(string fn, object args)
        {
            var tcs = new TaskCompletionSource<ExecuteCloudScriptResult>();
            PlayFabClientAPI.ExecuteCloudScript(
                new ExecuteCloudScriptRequest { FunctionName = fn, FunctionParameter = args, GeneratePlayStreamEvent = false },
                r => tcs.TrySetResult(r),
                e => tcs.TrySetException(new PlayFabException(e)));
            var res = await tcs.Task;
            if (res.Error != null) return ServerEcoResponse.Fail(EconomyError.ServerUnavailable); // CloudScript runtime error
            // FunctionResult is already a deserialized object; round-trip to the typed DTO.
            var json = JsonWrapper.SerializeObject(res.FunctionResult);
            return JsonWrapper.DeserializeObject<ServerEcoResponse>(json) ?? ServerEcoResponse.Fail(EconomyError.Unknown);
        }

        public long CachedBalance(Currency currency)
            => _cache.TryGetValue(currency, out var v) ? v : 0L;

        public async Task<EconomyResult> TrySpendAsync(Currency c, long amount, string reason)
        {
            if (Vc(c) == null) return EconomyResult.Failure(EconomyError.GoldNotPersistent, CachedBalance(c));
            var r = await Exec("spendCurrency", new { currency = Vc(c), amount, reason });
            if (!r.ok) return EconomyResult.Failure(r.error ?? EconomyError.ServerRejected, CachedBalance(c));
            _cache[c] = r.newBalance;
            return EconomyResult.Success(r.newBalance);
        }

        public async Task<EconomyResult> GrantAsync(Currency c, long amount, string source)
        {
            if (Vc(c) == null) return EconomyResult.Failure(EconomyError.GoldNotPersistent, CachedBalance(c));
            // The server re-derives the LEGITIMATE grant for `source` (e.g. a campaign first-clear,
            // checking the server-owned play count + diminishing rule) and CAPS the client's amount.
            var r = await Exec("grantCurrency", new { currency = Vc(c), amount, source });
            if (!r.ok) return EconomyResult.Failure(r.error ?? EconomyError.ServerRejected, CachedBalance(c));
            _cache[c] = r.newBalance;
            return EconomyResult.Success(r.newBalance);
        }

        public async Task<WalletSnapshot> GetWalletAsync()
        {
            var tcs = new TaskCompletionSource<GetUserInventoryResult>();
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                r => tcs.TrySetResult(r), e => tcs.TrySetException(new PlayFabException(e)));
            var inv = await tcs.Task;
            var balances = new Dictionary<Currency, long>();
            foreach (var c in new[] { Currency.Silver, Currency.Gems, Currency.PassXP })
            {
                long v = 0;
                if (inv.VirtualCurrency != null && inv.VirtualCurrency.TryGetValue(Vc(c), out var iv)) v = iv;
                balances[c] = v; _cache[c] = v;
            }
            return new WalletSnapshot(balances);
        }

        public async Task<UpgradeSpendResult> SpendForUpgradeAsync(
            string unitId, UpgradeStat stat, long costSilver, int expectedFromLevel, int hardCapMaxLevel)
        {
            // Server RE-DERIVES the cost from the server-owned UpgradesConfig (ignoring costSilver),
            // re-checks the hard cap + the from-level (optimistic concurrency), and commits atomically.
            var r = await Exec("spendForUpgrade", new
            {
                unitId, stat = (int)stat, clientCost = costSilver, expectedFromLevel, hardCapMaxLevel
            });
            if (r.ok) { _cache[Currency.Silver] = r.newBalance; return UpgradeSpendResult.Ok(r.newLevel, r.newBalance); }
            return UpgradeSpendResult.Rejected(r.newLevel, CachedBalance(Currency.Silver), MapReason(r.reason));
        }

        public async Task<CommanderLevelResult> SpendForCommanderLevelAsync(
            string commanderId, long costSilver, int expectedFromLevel, int hardCapMaxLevel)
        {
            var r = await Exec("spendForCommanderLevel", new
            {
                commanderId, clientCost = costSilver, expectedFromLevel, hardCapMaxLevel
            });
            if (r.ok) { _cache[Currency.Silver] = r.newBalance; return new CommanderLevelResult(true, r.newLevel, r.newBalance, UpgradeRejectReason.None); }
            return new CommanderLevelResult(false, r.newLevel, CachedBalance(Currency.Silver), MapReason(r.reason));
        }

        private static UpgradeRejectReason MapReason(string reason) => reason switch
        {
            "at_hard_cap"        => UpgradeRejectReason.AtHardCap,
            "insufficient_silver"=> UpgradeRejectReason.InsufficientSilver,
            "unknown_track"      => UpgradeRejectReason.UnknownTrack,
            "stale_client_level" => UpgradeRejectReason.StaleClientLevel,
            _                    => UpgradeRejectReason.ServerRejected,
        };

        /// <summary>Typed mirror of the CloudScript JSON response (server is the source of truth).</summary>
        private sealed class ServerEcoResponse
        {
            public bool ok;
            public long newBalance;
            public int newLevel;
            public string reason;
            public string error;
            public static ServerEcoResponse Fail(string err) => new ServerEcoResponse { ok = false, error = err };
        }
    }
}
#endif
