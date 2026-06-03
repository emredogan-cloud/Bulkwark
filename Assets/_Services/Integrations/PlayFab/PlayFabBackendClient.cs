// BULWARK — PlayFab adapter for IBackendClient (Phase-3 backend integration, §12).
// SERVER-AUTHORITATIVE: this CLIENT adapter performs auth + SERVER-OWNED READS only. It NEVER
// writes an authoritative balance or progression value from the client — those mutations go
// through CloudScript (server-validated) via PlayFabServerEconomy. The wallet is read from
// PlayFab Virtual Currency (server-owned, client-readable); progression from server-owned
// read-only player data; config from server-owned Title Data. NEVER logs save state (§12).
//
// ACTIVATION: this file lives in Bulwark.Services.PlayFab.asmdef, which is
// defineConstraints:["PLAYFAB_SDK"] — it is EXCLUDED from the build until the PlayFab Unity SDK
// is installed AND the PLAYFAB_SDK scripting define is added (see backend/playfab/README.md).
// Until then it has ZERO effect on the baseline CI compile (and cannot break it).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity / no PlayFab SDK here — ADR-0-001/-2-001).
// DEFERRED: real auth/read round-trips run in CI / on device against Title 101A1B.

#if PLAYFAB_SDK
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Bulwark.Services.Backend;

namespace Bulwark.Services.Integrations.PlayFab
{
    /// <summary>
    /// PlayFab-backed <see cref="IBackendClient"/>. Auth + server-owned reads; authoritative
    /// writes are refused here and routed through CloudScript (server authority is structural).
    /// </summary>
    public sealed class PlayFabBackendClient : IBackendClient
    {
        private readonly string _customId; // device-stable, non-PII opaque id (no raw device PII persisted)

        public PlayFabBackendClient(string titleId, string customId)
        {
            if (!string.IsNullOrEmpty(titleId)) PlayFabSettings.TitleId = titleId; // e.g. "101A1B"
            _customId = customId;
        }

        // ---- bridge PlayFab's callback API to Task ----
        private static Task<TResult> Call<TRequest, TResult>(
            TRequest req, Action<TRequest, Action<TResult>, Action<PlayFabError>, object, Dictionary<string, string>> api)
        {
            var tcs = new TaskCompletionSource<TResult>();
            api(req, r => tcs.TrySetResult(r), e => tcs.TrySetException(new PlayFabException(e)), null, null);
            return tcs.Task;
        }

        public async Task<AuthResult> AuthenticateAsync()
        {
            try
            {
                var res = await Call<LoginWithCustomIDRequest, LoginResult>(
                    new LoginWithCustomIDRequest { CustomId = _customId, CreateAccount = true },
                    PlayFabClientAPI.LoginWithCustomID);
                return new AuthResult(true, res.PlayFabId, null);
            }
            catch (PlayFabException ex) { return new AuthResult(false, null, ex.Code); } // terse code only — no PII
        }

        /// <summary>Reads the SERVER-OWNED wallet (Virtual Currency) + progression (read-only data).</summary>
        public async Task<PlayerProfile> GetProfileAsync(string playerId)
        {
            var inv = await Call<GetUserInventoryRequest, GetUserInventoryResult>(
                new GetUserInventoryRequest(), PlayFabClientAPI.GetUserInventory);
            var wallet = new Dictionary<string, long>();
            if (inv.VirtualCurrency != null)
                foreach (var kv in inv.VirtualCurrency) wallet[kv.Key] = kv.Value; // VC are server-owned

            var ro = await Call<GetUserDataRequest, GetUserDataResult>(
                new GetUserDataRequest { PlayFabId = playerId }, PlayFabClientAPI.GetUserReadOnlyData);
            var progress = new Dictionary<string, string>();
            if (ro.Data != null)
                foreach (var kv in ro.Data) progress[kv.Key] = kv.Value.Value; // server-owned, client-READ-only

            return new PlayerProfile(playerId, wallet, progress);
        }

        /// <summary>
        /// SERVER AUTHORITY: the client may NOT write authoritative balances/progression. This refuses
        /// such writes; authoritative changes go through CloudScript (PlayFabServerEconomy). Returns
        /// false so callers never assume a client write succeeded. (No save-state payload is logged.)
        /// </summary>
        public Task<bool> WriteProfileAsync(PlayerProfile profile)
            => Task.FromResult(false);

        public async Task<IReadOnlyDictionary<string, string>> FetchRemoteConfigAsync()
        {
            var td = await Call<GetTitleDataRequest, GetTitleDataResult>(
                new GetTitleDataRequest(), PlayFabClientAPI.GetTitleData);
            return (IReadOnlyDictionary<string, string>)(td.Data ?? new Dictionary<string, string>());
        }
    }

    /// <summary>Wraps a PlayFabError as an exception carrying only its terse code (no PII/payload).</summary>
    public sealed class PlayFabException : Exception
    {
        public string Code { get; }
        public PlayFabException(PlayFabError e) : base(e != null ? e.Error.ToString() : "playfab_error")
        { Code = e != null ? e.Error.ToString() : "playfab_error"; }
    }
}
#endif
