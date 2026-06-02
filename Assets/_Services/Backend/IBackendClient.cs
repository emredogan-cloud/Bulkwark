// BULWARK — Backend (BaaS) client stub (Phase 0.4 scaffold).
// Roadmap §12 + decision-log §3: economy is SERVER-AUTHORITATIVE; MVP uses a managed
// BaaS (PlayFab/Nakama), NOT a custom backend. This interface is the seam; a concrete
// PlayFab/Nakama adapter is wired when a BaaS project + secrets exist.
// Roadmap REPLACE rule: NEVER log save state; the server profile is the record of truth.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6, no BaaS project — ADR-0-001).
// GATE 0.4 (auth + profile read/write + config fetch + analytics round-trip) is
// reported BLOCKED — no backend/credentials in this environment.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Services.Backend
{
    public readonly struct AuthResult
    {
        public readonly bool Success;
        public readonly string PlayerId;   // opaque server id; no device PII persisted client-side
        public readonly string Error;
        public AuthResult(bool ok, string playerId, string error) { Success = ok; PlayerId = playerId; Error = error; }
    }

    /// <summary>Server-authoritative profile (currencies/progression live on the server, §9/§12).</summary>
    public readonly struct PlayerProfile
    {
        public readonly string PlayerId;
        public readonly IReadOnlyDictionary<string, long> Wallet;       // server-owned balances
        public readonly IReadOnlyDictionary<string, string> Progress;   // server-owned progression
        public PlayerProfile(string id, IReadOnlyDictionary<string, long> wallet, IReadOnlyDictionary<string, string> progress)
        { PlayerId = id; Wallet = wallet; Progress = progress; }
    }

    /// <summary>The MVP backend seam. Implemented by a BaaS adapter; never by custom servers at MVP.</summary>
    public interface IBackendClient
    {
        Task<AuthResult> AuthenticateAsync();                              // device/platform auth
        Task<PlayerProfile> GetProfileAsync(string playerId);             // server read
        Task<bool> WriteProfileAsync(PlayerProfile profile);             // server write (authoritative)
        Task<IReadOnlyDictionary<string, string>> FetchRemoteConfigAsync(); // tier-1 config delivery (feeds ConfigResolver)
    }

    /// <summary>
    /// No-op stub so the app/CI compile and round-trip wiring is exercisable in tests.
    /// Returns deterministic placeholder data; replaced by a BaaS adapter in integration.
    /// </summary>
    public sealed class StubBackendClient : IBackendClient
    {
        public Task<AuthResult> AuthenticateAsync()
            => Task.FromResult(new AuthResult(true, "stub-player", null));

        public Task<PlayerProfile> GetProfileAsync(string playerId)
            => Task.FromResult(new PlayerProfile(playerId,
                new Dictionary<string, long>(),     // empty wallet — NO currencies invented at P0 (§15)
                new Dictionary<string, string>()));

        public Task<bool> WriteProfileAsync(PlayerProfile profile) => Task.FromResult(true);

        public Task<IReadOnlyDictionary<string, string>> FetchRemoteConfigAsync()
            => Task.FromResult((IReadOnlyDictionary<string, string>)new Dictionary<string, string>());
    }
}
