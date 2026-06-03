// BULWARK — Mode reward seam (Phase 3 §13 3.3, §6, §9, §12).
// WHY THIS FILE EXISTS
//   Campaign + Endless need to GRANT currency (Gems/Silver/PassXP) and to READ the
//   SERVER-OWNED per-level playCount that feeds the diminishing-returns gem rule
//   (§9: first-clear gem = max(floor, base − dec×playCount)). The SERVER-AUTHORITATIVE
//   currency economy is module 3.1's deliverable — Assets/_Services/Economy/Progression.cs
//   + Upgrades.cs, which expose the canonical IServerEconomy / ICommanderEconomy seams
//   (Bulwark.Services.Economy). There is NO Wallet.cs; the canonical currency-mutation
//   contract is IServerEconomy (declared in Upgrades.cs, owned by 3.1).
//
//   To keep the modes layer self-contained AND to make the inviolable rule structurally
//   true, the mode layer talks ONLY to this narrow CLIENT-FACING facade (IModeRewardWallet).
//   The facade is a thin adapter over 3.1's IServerEconomy — it NEVER reaches a local
//   balance and NEVER does client-side arithmetic into a wallet write.
//
// SEAM RECONCILIATION (integration contract — RESOLVED THIS PASS)
//   IModeRewardWallet is the AGREED narrow client-facing reward facade. It does NOT define a
//   second server-economy contract: its GrantAsync DELEGATES to 3.1's canonical
//   IServerEconomy.GrantAsync (the atomic, server-validated grant the SERVER derives and
//   commits — ServerEconomy.cs). An earlier draft named a non-existent
//   IServerEconomy.GrantCurrencyAsync returning a CurrencyGrantResult; at integration this is
//   ROUTED to the canonical GrantAsync(Currency, long, string) → EconomyResult, mapped into the
//   modes-facing RewardResult below. The previous version did a CLIENT read-modify-write of the
//   raw wallet via IBackendClient.WriteProfileAsync — that is removed. The client now passes a
//   DELTA + reason; the SERVER computes current+amount, validates plausibility/caps, and returns
//   the confirmed balance. GetCampaignPlayCountAsync remains a READ of the server-owned profile.
//
// INVIOLABLE — SERVER AUTHORITY OVER CURRENCY (§9/§12, decision-log §3, ADR-2-001)
//   • The client NEVER mutates an authoritative balance and NEVER writes a raw wallet value.
//     Every Grant is a DELTA request through 3.1's IServerEconomy; the SERVER derives + commits
//     the new balance and the caller only learns the SERVER-CONFIRMED result.
//   • playCount is SERVER-OWNED: read from PlayerProfile.Progress (the server-owned progression
//     dict), NEVER counted/trusted client-side. The diminishing FORMULA runs on the client only
//     to PREVIEW/REQUEST a delta; the server re-derives + validates the actual grant.
//   • The CRDT/obscured cache (3.1) is a display/anti-tamper cache reconciled to the server —
//     it is NOT the source of truth, and it is NOT mutated here.
//
// NO SAVE-STATE LOGGING (§4/§12 inviolable): nothing here logs wallet/profile/save payloads.
// CAPPED PROGRESSION (§3): rewards are Gems/Silver/PassXP only — never power, never upgrades
//   past a cap. This seam grants currency; it has no path to raise a stat or a cap.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / no BaaS — ADR-0-001/-2-001).
// DEFERRED: the real BaaS round-trip (server-side grant validation/commit via a Cloud Script,
//   the playCount increment) — represented by 3.1's IServerEconomy seam + the IBackendClient
//   stub below. Until the Cloud Script exists, the grant is a REQUEST the server re-derives.

using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Analytics;
using Bulwark.Services.Backend;
using Bulwark.Services.Economy; // IServerEconomy, EconomyResult (module 3.1's canonical seam)

namespace Bulwark.Game.Modes.Economy
{
    /// <summary>
    /// Result of a server-validated reward grant. The client treats <see cref="Confirmed"/> as the
    /// ONLY truth: a false value means the server rejected/failed the grant and NO balance changed.
    /// <see cref="NewBalance"/> is the server's post-grant balance for the granted currency (cache update only).
    /// This is the modes-facing projection of 3.1's <see cref="EconomyResult"/>.
    /// </summary>
    public readonly struct RewardResult
    {
        public readonly bool Confirmed;     // server accepted the grant (false ⇒ nothing changed)
        public readonly Currency Currency;  // which currency was granted
        public readonly long Granted;        // amount the server actually granted (may be 0)
        public readonly long NewBalance;     // server-confirmed balance AFTER the grant (cache value)
        public readonly string Error;        // diagnostic only; never a save-state payload

        public RewardResult(bool confirmed, Currency currency, long granted, long newBalance, string error)
        {
            Confirmed = confirmed; Currency = currency; Granted = granted; NewBalance = newBalance; Error = error;
        }
    }

    /// <summary>
    /// Narrow client-facing reward FACADE consumed by Campaign/Endless. It is NOT a second
    /// server-economy contract: it DELEGATES every grant to module 3.1's canonical
    /// <see cref="IServerEconomy"/> (Assets/_Services/Economy/Upgrades.cs). The modes layer depends
    /// ONLY on this facade so it can never reach a local balance (server-authority is structural).
    /// At integration this facade is implemented by (or over) 3.1's server-authoritative economy;
    /// <see cref="GetCampaignPlayCountAsync"/> + <see cref="GrantAsync"/> map onto 3.1's server RPCs.
    /// </summary>
    public interface IModeRewardWallet
    {
        /// <summary>
        /// Request a SERVER-VALIDATED grant of <paramref name="amount"/> of <paramref name="currency"/>.
        /// The client passes a DELTA + <paramref name="reason"/>; the SERVER derives current+amount,
        /// validates plausibility, and commits. The returned <see cref="RewardResult"/> reflects what the
        /// SERVER actually granted. Persistent currencies only (Silver/Gems/PassXP) — Gold is
        /// in-battle/non-persistent and is rejected.
        /// </summary>
        Task<RewardResult> GrantAsync(Currency currency, long amount, string reason);

        /// <summary>
        /// Read the SERVER-OWNED play count for a campaign level (how many times it has been cleared/played),
        /// used by the §9 diminishing-returns gem rule. NEVER counted client-side. Returns 0 if unknown.
        /// </summary>
        Task<int> GetCampaignPlayCountAsync(string campaignLevelId);
    }

    /// <summary>
    /// Routed reward wallet: a THIN ADAPTER that turns a reward request into a SERVER round-trip via
    /// module 3.1's canonical <see cref="IServerEconomy"/>. It does NOT read-modify-write the wallet on
    /// the client: it passes the DELTA to <see cref="IServerEconomy.GrantAsync"/>, and the SERVER
    /// derives current+amount, validates caps/plausibility, commits, and returns the CONFIRMED balance.
    /// The client cache is updated ONLY from that confirmed result — never optimistically, never from a
    /// client-computed balance.
    ///
    /// The play-count READ uses <see cref="IBackendClient"/> to read the server-owned profile (a pure read,
    /// no mutation). The server-owned playCount INCREMENT is a server (BaaS Cloud Script) op — DEFERRED.
    ///
    /// NOTE: With the StubBackendClient / the StubServerEconomy this is a no-op round-trip (the stub returns
    /// the request unchanged and re-reads an empty profile); that is the DEFERRED seam. A real BaaS adapter
    /// (PlayFab/Nakama Cloud Script) MUST back GrantAsync with a server-side grant that re-derives the
    /// balance from the delta and persists it, so the confirmed balance is the true post-grant value.
    /// </summary>
    public sealed class ServerRoutedRewardWallet : IModeRewardWallet
    {
        private const string PlayCountProgressPrefix = "campaign.playcount."; // server-owned key namespace
                                                                              // (MUST match the Cloud Script's key)

        private readonly IServerEconomy _economy;  // 3.1's canonical server-authoritative grant seam
        private readonly IBackendClient _backend;  // server-owned profile READ (playCount); never a grant path
        private readonly IAnalytics _analytics;
        private readonly string _playerId;

        public ServerRoutedRewardWallet(IServerEconomy economy, IBackendClient backend, IAnalytics analytics, string playerId)
        {
            _economy = economy;
            _backend = backend;
            _analytics = analytics;
            _playerId = playerId;
        }

        public async Task<RewardResult> GrantAsync(Currency currency, long amount, string reason)
        {
            if (_economy == null)
                return new RewardResult(false, currency, 0, 0, "no-server-economy");
            if (currency == Currency.Gold)
                return new RewardResult(false, currency, 0, 0, "gold-non-persistent"); // §9: Gold is in-battle only
            if (amount <= 0)
                // Nothing to grant — surface the server's CURRENT balance (cache), no mutation requested.
                return new RewardResult(true, currency, 0, _economy.CachedBalance(currency), null);

            // SERVER-AUTHORITATIVE grant: pass the DELTA; the SERVER derives current+amount, validates
            // plausibility, commits, and returns the confirmed balance. The client never writes a balance.
            // Routed to the canonical IServerEconomy.GrantAsync (ServerEconomy.cs) → EconomyResult.
            EconomyResult result = await _economy.GrantAsync(currency, amount, reason);

            if (!result.Ok)
                // On failure NewBalance carries the server's last-known balance (lastKnownBalance) and
                // Error is a terse non-PII code — surfaced for diagnostics, never a save-state payload.
                return new RewardResult(false, currency, 0, result.NewBalance, result.Error ?? "server-rejected");

            // The server confirmed the full delta (the canonical seam grants the requested amount or fails
            // atomically — there is no partial grant), so the granted amount is `amount`.
            long granted = amount;

            // Analytics: event name + non-PII numeric fields ONLY — never the wallet/profile payload (§4/§12).
            _analytics?.Emit("reward_granted", new System.Collections.Generic.Dictionary<string, object>
            {
                { "currency", currency.ToString() },
                { "amount", granted },
                { "reason", reason ?? "unspecified" },
            });

            // The cache reflects the SERVER-CONFIRMED balance (from the server's response, not a client guess).
            return new RewardResult(true, currency, granted, result.NewBalance, null);
        }

        public async Task<int> GetCampaignPlayCountAsync(string campaignLevelId)
        {
            if (_backend == null || string.IsNullOrEmpty(_playerId) || string.IsNullOrEmpty(campaignLevelId))
                return 0;
            // Pure READ of the server-owned progression dict. The INCREMENT is a server op (Cloud Script),
            // DEFERRED — with the stub (empty Progress) this is always 0 until the server increments it.
            PlayerProfile profile = await _backend.GetProfileAsync(_playerId);
            if (profile.Progress != null &&
                profile.Progress.TryGetValue(PlayCountProgressPrefix + campaignLevelId, out string raw) &&
                int.TryParse(raw, out int n) && n > 0)
                return n;
            return 0; // server-owned; absent ⇒ never played
        }
    }
}
