// BULWARK — SERVER-AUTHORITATIVE economy boundary (Phase 3 §13 3.1, §9, §12).
// HARD RULE #1 (INVIOLABLE): currency/progression is SERVER-OWNED. This interface IS the
// server-authority seam: the client NEVER mutates an authoritative balance locally — every
// grant/spend round-trips through here and the client only adopts THIS layer's confirmed
// response (NewBalance / WalletSnapshot / confirmed level). The server (here, the stub) is the
// authority; the client trusts ITS math, not its own (decision-log §3: server-auth via BaaS).
//
// CANONICAL OWNERSHIP (3.1 owns the seam). This module — 3.1-economy-wallet — owns the ONE
// canonical IServerEconomy definition and the ONE canonical UpgradeSpendResult / UpgradeRejectReason.
// Modules 3.2 (Upgrades.cs / UpgradesService) and 3.3 (Progression.cs) CONSUME these. Per the
// shared-seam reconciliation note in Upgrades.cs ("3.1 owns the CANONICAL definition ... the two
// MUST reconcile to ONE definition"), the duplicate interface/types previously declared in
// Upgrades.cs are REMOVED at integration so only THIS declaration remains (see integrationNotes).
// To keep 3.2 compiling against the canonical seam, IServerEconomy here EXPOSES the two members
// UpgradesService depends on: SpendForUpgradeAsync(...) and CachedBalance(...).
//
// SCOPE: Silver / Gems / PassXP ONLY (the persistent, server-owned currencies, §9). GOLD is
// NOT handled here — Gold is in-battle / NON-persistent (the ECS GoldStore) and never enters
// the server wallet (EconomyTypes.cs). Passing Currency.Gold is rejected as a domain error.
//
// HARD RULE #2 (INVIOLABLE): NO save-state logging — this layer never logs wallet/profile
// payloads; EconomyResult / UpgradeSpendResult carry terse CODES / scalars (no balances PII).
// HARD RULE #3 (INVIOLABLE): CAPPED progression — SpendForUpgradeAsync RE-ENFORCES the hard cap
// (hardCapMaxLevel) and the from-level concurrency guard server-side; a client can never advance
// a level past the cap, regardless of what it requests.
// HARD RULE #5: Gems are EARNED only at MVP; this seam grants/spends them but the *sources*
// are campaign first-clear / mode rewards (§9). No IAP/shop path exists (Phase 4).
//
// PURE C# (no UnityEngine) so it is CI-unit-testable like the Phase-0 ConfigResolver (§12).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / no BaaS — ADR-0-001 / ADR-2-001).
// DEFERRED: the REAL server round-trip (BaaS adapter behind IBackendClient), RC retune of
// ServerValidationConfig bounds, and any analytics-land are DEFERRED — no backend here.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Backend;

namespace Bulwark.Services.Economy
{
    /// <summary>
    /// Outcome of a server-authoritative economy op. <see cref="NewBalance"/> is the server's
    /// confirmed balance for the affected currency AFTER the op (the ONLY value the client may
    /// adopt). On failure, Ok=false and <see cref="Error"/> is a terse machine CODE (never a
    /// balance/payload — §12 no save-state logging).
    /// </summary>
    public readonly struct EconomyResult
    {
        public readonly bool Ok;
        public readonly long NewBalance; // server-confirmed post-op balance for the op's currency
        public readonly string Error;    // null on success; a terse code otherwise (no PII/payload)

        private EconomyResult(bool ok, long newBalance, string error)
        {
            Ok = ok; NewBalance = newBalance; Error = error;
        }

        public static EconomyResult Success(long newBalance) => new EconomyResult(true, newBalance, null);
        public static EconomyResult Failure(string error, long lastKnownBalance = 0)
            => new EconomyResult(false, lastKnownBalance, error ?? EconomyError.Unknown);
    }

    /// <summary>Terse, non-sensitive error CODES (no balances/payloads — §12 logging rule).</summary>
    public static class EconomyError
    {
        public const string Unknown            = "unknown";
        public const string GoldNotPersistent  = "gold_not_persistent";   // Gold is in-battle only (§9)
        public const string NonPositiveAmount  = "non_positive_amount";   // amount <= 0
        public const string ImplausibleAmount  = "implausible_amount";    // exceeds per-call sanity cap
        public const string Overflow           = "overflow";              // would overflow long / cap
        public const string InsufficientFunds  = "insufficient_funds";    // spend > balance
        public const string NegativeBalance    = "negative_balance";      // server invariant breached
        public const string NotAuthenticated   = "not_authenticated";     // no server profile
        public const string ServerUnavailable  = "server_unavailable";    // round-trip failed
    }

    /// <summary>Immutable snapshot of the SERVER-CONFIRMED persistent balances (Silver/Gems/PassXP).</summary>
    public readonly struct WalletSnapshot
    {
        public readonly IReadOnlyDictionary<Currency, long> Balances;
        public WalletSnapshot(IReadOnlyDictionary<Currency, long> balances) { Balances = balances; }

        public long Get(Currency c) => (Balances != null && Balances.TryGetValue(c, out var v)) ? v : 0L;
    }

    // =====================================================================================
    // CAPPED-PROGRESSION SPEND result + reason (CANONICAL — owned by 3.1, consumed by 3.2/3.3).
    //
    // These were previously declared in Upgrades.cs (3.2). 3.1 owns the canonical economy seam, so
    // it owns the types that seam returns. They are DEFINED here ONCE and REUSED by UpgradesService
    // (3.2) and ProgressionService (3.3 — which reuses UpgradeRejectReason). The duplicate copies in
    // Upgrades.cs are removed at integration (integrationNotes). NO save-state payload (HARD RULE 2).
    // =====================================================================================

    /// <summary>
    /// The server's confirmed answer to an atomic spend+grant (e.g. a capped upgrade purchase). The
    /// client trusts THIS, never its own guess. Carries NO save-state payload (HARD RULE 2) — only
    /// the two authoritative scalars + a UX reason.
    /// </summary>
    public readonly struct UpgradeSpendResult
    {
        public readonly bool Confirmed;        // true only if the SERVER committed the transaction
        public readonly int  NewLevel;         // server-authoritative new track level (== from on reject)
        public readonly long NewSilverBalance; // server-authoritative Silver balance after the charge
        public readonly UpgradeRejectReason Reason; // why a non-confirm happened (UX only)

        public UpgradeSpendResult(bool confirmed, int newLevel, long newSilverBalance, UpgradeRejectReason reason)
        {
            Confirmed = confirmed;
            NewLevel = newLevel;
            NewSilverBalance = newSilverBalance;
            Reason = reason;
        }

        public static UpgradeSpendResult Ok(int newLevel, long newBalance)
            => new UpgradeSpendResult(true, newLevel, newBalance, UpgradeRejectReason.None);

        public static UpgradeSpendResult Rejected(int currentLevel, long balance, UpgradeRejectReason reason)
            => new UpgradeSpendResult(false, currentLevel, balance, reason);
    }

    /// <summary>Why a (pre-flight or server) capped-progression attempt did not commit. Diagnostics/UX only.</summary>
    public enum UpgradeRejectReason
    {
        None = 0,
        AtHardCap,          // already at/over maxLevel — REFUSED (§3/§15)
        InsufficientSilver, // not enough Silver (server-checked)
        UnknownTrack,       // no UpgradeTrack for this (unitId, stat) in UpgradesConfig
        StaleClientLevel,   // client's from-level disagreed with the server (concurrency)
        ServerRejected,     // generic server-side rejection (validation/transport)
    }

    /// <summary>
    /// THE server-authority boundary (HARD RULE #1). Implemented by a BaaS adapter at integration;
    /// the StubServerEconomy below SIMULATES the server so wiring/tests run without a backend.
    /// Gold is intentionally NOT representable here (in-battle / non-persistent, §9).
    ///
    /// This is the SINGLE canonical IServerEconomy. Besides the wallet ops (TrySpend/Grant/GetWallet)
    /// it exposes the capped-progression spend (SpendForUpgradeAsync) + a read-cache accessor
    /// (CachedBalance) that UpgradesService (3.2) depends on — folded in so 3.2 compiles against THIS
    /// one interface (no duplicate IServerEconomy survives in Upgrades.cs; see integrationNotes).
    /// </summary>
    public interface IServerEconomy
    {
        /// <summary>Authoritatively SPEND `amount` of `c`. Server validates funds/sanity; client
        /// adopts only the returned NewBalance. `reason` is an audit tag (analytics, DEFERRED).</summary>
        Task<EconomyResult> TrySpendAsync(Currency c, long amount, string reason);

        /// <summary>Authoritatively GRANT `amount` of `c` from `source` (e.g. campaign first-clear,
        /// mode reward — §9). Server validates sanity/overflow; client adopts only NewBalance.</summary>
        Task<EconomyResult> GrantAsync(Currency c, long amount, string source);

        /// <summary>Read the server-confirmed persistent balances (Silver/Gems/PassXP).</summary>
        Task<WalletSnapshot> GetWalletAsync();

        /// <summary>
        /// Atomic, SERVER-VALIDATED upgrade purchase (consumed by UpgradesService, §13 3.2): charge
        /// <paramref name="costSilver"/> Silver AND advance the (unitId, stat) track from
        /// <paramref name="expectedFromLevel"/> to <paramref name="expectedFromLevel"/>+1, in ONE
        /// transaction. The server re-validates the cap (<paramref name="hardCapMaxLevel"/>), the cost,
        /// sufficient balance, and the from-level (optimistic-concurrency guard against a stale client),
        /// then returns the CONFIRMED new level + Silver balance. On any rejection nothing is mutated
        /// server-side and Confirmed is false. The client trusts only this answer (HARD RULE #1/#3).
        /// </summary>
        Task<UpgradeSpendResult> SpendForUpgradeAsync(
            string unitId, UpgradeStat stat, long costSilver, int expectedFromLevel, int hardCapMaxLevel);

        /// <summary>Last server-confirmed balance for read-cache display (server-owned, may be stale).
        /// Gold returns 0 (not held here, §9). Used by UpgradesService/ProgressionService for UX only.</summary>
        long CachedBalance(Currency currency);
    }

    /// <summary>
    /// Server-side validation bounds (the "plausibility" envelope the AUTHORITY enforces). These are
    /// SERVER-OWNED and RC-tunable in the same RC → SO → literal spirit as the rest of the economy
    /// (§12); here they are literal defaults until a backend/RemoteConfig supplies them (DEFERRED).
    /// They exist to reject implausible grants/spends — a stat-sanity style guard (decision-log §3),
    /// NOT client trust. Caps here are anti-abuse envelopes; the no-P2W progression cap (§3) is a
    /// SEPARATE server-enforced rule applied where balances are *consumed* (UpgradesConfig).
    /// </summary>
    public readonly struct ServerValidationConfig
    {
        public readonly long MaxGrantPerCall;   // reject a single grant above this (anti-abuse)
        public readonly long MaxSpendPerCall;    // reject a single spend above this (anti-abuse)
        public readonly long MaxBalancePerCurrency; // hard ceiling per currency (overflow / hoard guard)

        public ServerValidationConfig(long maxGrantPerCall, long maxSpendPerCall, long maxBalancePerCurrency)
        {
            MaxGrantPerCall = maxGrantPerCall;
            MaxSpendPerCall = maxSpendPerCall;
            MaxBalancePerCurrency = maxBalancePerCurrency;
        }

        /// <summary>Provisional literal defaults (LSD-owned, RC-overridable). DEFERRED: RC retune.
        /// EconomyConfig.cs has no matching fields today, so these stay literal until a backend/
        /// RemoteConfig supplies REAL server-side bounds — they are NOT client-trusted at MVP.</summary>
        public static ServerValidationConfig Default => new ServerValidationConfig(
            maxGrantPerCall: 100_000,
            maxSpendPerCall: 100_000,
            maxBalancePerCurrency: 1_000_000_000L);
    }

    /// <summary>
    /// SIMULATED server authority backed by <see cref="IBackendClient"/>. It performs the
    /// validation a real server would (reject Gold, non-positive, implausible, overflow,
    /// insufficient funds, negative invariant, cap/from-level for upgrades) and treats the BACKEND
    /// PROFILE as the record of truth — reading the wallet/progress, computing the new state, writing
    /// it back, and returning the confirmed result. The client trusts THIS response, never its own
    /// arithmetic (HARD RULE #1).
    ///
    /// The backend wallet is keyed by Currency NAME (string) to match PlayerProfile.Wallet
    /// (IReadOnlyDictionary&lt;string,long&gt;); Gold is never written into it. Upgrade levels live in
    /// PlayerProfile.Progress under the SAME `upg.{unitId}.{stat}` key UpgradesService reads (so the
    /// stub and 3.2 agree). EVERY profile write is a READ-MODIFY-WRITE that PRESERVES the full
    /// Progress map — it never clobbers the upgrade/commander progression keys (the data-loss risk
    /// flagged at review when 3.1 and 3.2 share one backend profile).
    /// NEVER logs the wallet/profile (HARD RULE #2). Pure C#; the only async is the (stubbed)
    /// round-trip — DEFERRED to a real BaaS adapter.
    ///
    /// ONE COHERENT SEAM (integration A3): this stub implements BOTH the canonical
    /// <see cref="IServerEconomy"/> (used by Wallet / UpgradesService / the mode reward facade) AND
    /// <see cref="ICommanderEconomy"/> (Progression.cs, 3.3). Commander leveling shares the SAME
    /// simulated server wallet + Progress map (so a commander level-up and an upgrade purchase can
    /// never clobber one another), under the canonical commander key cmd.{commanderId}.level that
    /// ProgressionService.ProgressKey reads. A single instance therefore serves the whole economy
    /// seam in this environment (DEFERRED: a real BaaS adapter implements both contracts likewise).
    /// </summary>
    public sealed class StubServerEconomy : IServerEconomy, ICommanderEconomy
    {
        private readonly IBackendClient _backend;
        private readonly ServerValidationConfig _rules;
        private string _playerId;

        // In-stub authoritative store (stands in for the server profile until a BaaS adapter
        // lands). Keyed by Currency name to mirror PlayerProfile.Wallet. NOT a client cache —
        // this is the simulated SERVER side. DEFERRED: replace with real GetProfile/WriteProfile.
        private readonly Dictionary<string, long> _serverWallet = new Dictionary<string, long>();

        // The simulated server-side Progress map (upgrade/commander levels etc.). Seeded from the
        // profile on auth and read-modify-written on every commit so a WALLET write never erases a
        // PROGRESSION key (data-loss guard — review integration issue #2). This is the authority's
        // copy, NOT a client cache. DEFERRED: real server transaction over GetProfile/WriteProfile.
        private readonly Dictionary<string, string> _serverProgress = new Dictionary<string, string>(StringComparer.Ordinal);

        public StubServerEconomy(IBackendClient backend, ServerValidationConfig? rules = null)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _rules = rules ?? ServerValidationConfig.Default;
        }

        public async Task<EconomyResult> TrySpendAsync(Currency c, long amount, string reason)
        {
            var guard = ValidatePersistentCurrency(c);
            if (guard != null) return EconomyResult.Failure(guard);
            if (amount <= 0) return EconomyResult.Failure(EconomyError.NonPositiveAmount);
            if (amount > _rules.MaxSpendPerCall) return EconomyResult.Failure(EconomyError.ImplausibleAmount);

            if (!await EnsureAuthenticatedAsync())
                return EconomyResult.Failure(EconomyError.NotAuthenticated);

            long current = ReadServer(c);
            if (amount > current)
                return EconomyResult.Failure(EconomyError.InsufficientFunds, current);

            long next = current - amount;
            if (next < 0) // server invariant: a balance can never go negative
                return EconomyResult.Failure(EconomyError.NegativeBalance, current);

            _serverWallet[Key(c)] = next;
            if (!await CommitProfileAsync())
            {
                _serverWallet[Key(c)] = current; // roll back the in-stub store on a failed write
                return EconomyResult.Failure(EconomyError.ServerUnavailable, current);
            }

            // `reason` is an audit/analytics tag (DEFERRED) — deliberately NOT logged here (§12).
            return EconomyResult.Success(next);
        }

        public async Task<EconomyResult> GrantAsync(Currency c, long amount, string source)
        {
            var guard = ValidatePersistentCurrency(c);
            if (guard != null) return EconomyResult.Failure(guard);
            if (amount <= 0) return EconomyResult.Failure(EconomyError.NonPositiveAmount);
            if (amount > _rules.MaxGrantPerCall) return EconomyResult.Failure(EconomyError.ImplausibleAmount);

            if (!await EnsureAuthenticatedAsync())
                return EconomyResult.Failure(EconomyError.NotAuthenticated);

            long current = ReadServer(c);
            // Overflow / hard-ceiling guard (anti-abuse, never silently wrap).
            if (amount > _rules.MaxBalancePerCurrency - current)
                return EconomyResult.Failure(EconomyError.Overflow, current);

            long next = current + amount;
            _serverWallet[Key(c)] = next;
            if (!await CommitProfileAsync())
            {
                _serverWallet[Key(c)] = current; // roll back the in-stub store on a failed write
                return EconomyResult.Failure(EconomyError.ServerUnavailable, current);
            }

            // `source` (campaign first-clear / mode reward, §9) is an audit tag (DEFERRED), NOT logged.
            return EconomyResult.Success(next);
        }

        public async Task<WalletSnapshot> GetWalletAsync()
        {
            await EnsureAuthenticatedAsync();
            // Project the simulated server wallet into a typed Currency map (persistent currencies only).
            var balances = new Dictionary<Currency, long>
            {
                { Currency.Silver, ReadServer(Currency.Silver) },
                { Currency.Gems,   ReadServer(Currency.Gems) },
                { Currency.PassXP, ReadServer(Currency.PassXP) },
            };
            return new WalletSnapshot(balances);
        }

        /// <summary>
        /// Atomic, SERVER-VALIDATED upgrade purchase (HARD RULE #1/#3). Simulates what a BaaS
        /// transaction would do: re-validate the hard cap and from-level concurrency guard, charge
        /// Silver only if funds suffice, advance the level by exactly ONE, and persist BOTH the new
        /// Silver balance AND the new level under the canonical Progress key — in a single profile
        /// read-modify-write that preserves all other progression keys. The client never advances a
        /// level itself; it adopts only the returned UpgradeSpendResult. NO payload logged (§12).
        /// </summary>
        public async Task<UpgradeSpendResult> SpendForUpgradeAsync(
            string unitId, UpgradeStat stat, long costSilver, int expectedFromLevel, int hardCapMaxLevel)
        {
            if (string.IsNullOrEmpty(unitId))
                return UpgradeSpendResult.Rejected(0, CachedBalance(Currency.Silver), UpgradeRejectReason.UnknownTrack);

            if (!await EnsureAuthenticatedAsync())
                return UpgradeSpendResult.Rejected(
                    expectedFromLevel < 0 ? 0 : expectedFromLevel, CachedBalance(Currency.Silver), UpgradeRejectReason.ServerRejected);

            int cap = hardCapMaxLevel < 0 ? 0 : hardCapMaxLevel;
            long silver = ReadServer(Currency.Silver);

            // SERVER-authoritative current level (record of truth), independent of the client's claim.
            int serverLevel = ReadServerLevel(UpgradeProgressKey(unitId, stat), cap);

            // Optimistic-concurrency guard: the client's from-level must match the server's (anti-stale).
            if (expectedFromLevel != serverLevel)
                return UpgradeSpendResult.Rejected(serverLevel, silver, UpgradeRejectReason.StaleClientLevel);

            // HARD CAP re-enforcement (§3/§15): never advance at/over the cap, regardless of request.
            if (serverLevel >= cap)
                return UpgradeSpendResult.Rejected(serverLevel, silver, UpgradeRejectReason.AtHardCap);

            // Cost sanity + sufficient funds (the client's cost is advisory; server holds the balance).
            if (costSilver < 0 || costSilver > _rules.MaxSpendPerCall)
                return UpgradeSpendResult.Rejected(serverLevel, silver, UpgradeRejectReason.ServerRejected);
            if (costSilver > silver)
                return UpgradeSpendResult.Rejected(serverLevel, silver, UpgradeRejectReason.InsufficientSilver);

            int newLevel = serverLevel + 1;
            long newSilver = silver - costSilver;
            if (newSilver < 0)
                return UpgradeSpendResult.Rejected(serverLevel, silver, UpgradeRejectReason.InsufficientSilver);

            // Stage the atomic mutation (charge + level advance), then commit the FULL profile.
            _serverWallet[Key(Currency.Silver)] = newSilver;
            _serverProgress[UpgradeProgressKey(unitId, stat)] = newLevel.ToString();

            if (!await CommitProfileAsync())
            {
                // Roll back the staged mutation on a failed write (DEFERRED: real server txn).
                _serverWallet[Key(Currency.Silver)] = silver;
                _serverProgress[UpgradeProgressKey(unitId, stat)] = serverLevel.ToString();
                return UpgradeSpendResult.Rejected(serverLevel, silver, UpgradeRejectReason.ServerRejected);
            }

            return UpgradeSpendResult.Ok(newLevel, newSilver);
        }

        /// <summary>
        /// Atomic, SERVER-VALIDATED commander level-up (ICommanderEconomy, 3.3 — HARD RULE #1/#3).
        /// Mirrors <see cref="SpendForUpgradeAsync"/> but for a commander track keyed
        /// cmd.{commanderId}.level: re-validate the hard cap + the from-level concurrency guard,
        /// charge Silver only if funds suffice, advance the level by exactly ONE, and persist BOTH the
        /// new Silver balance AND the new commander level in ONE profile read-modify-write that
        /// preserves all other progression keys (the upgrade keys 3.2 owns are untouched). The client
        /// never advances a level itself; it adopts only the returned CommanderLevelResult. Leveling
        /// realises more of the §6 budget but NEVER raises the ceiling (that clamp lives in the pure
        /// CommanderProgressionMath, applied where the fraction is consumed). NO payload logged (§12).
        /// </summary>
        public async Task<CommanderLevelResult> SpendForCommanderLevelAsync(
            string commanderId, long costSilver, int expectedFromLevel, int hardCapMaxLevel)
        {
            if (string.IsNullOrEmpty(commanderId))
                return new CommanderLevelResult(false, 0, CachedBalance(Currency.Silver), UpgradeRejectReason.UnknownTrack);

            if (!await EnsureAuthenticatedAsync())
                return new CommanderLevelResult(
                    false, expectedFromLevel < 0 ? 0 : expectedFromLevel, // == from-level on a non-confirm
                    CachedBalance(Currency.Silver), UpgradeRejectReason.ServerRejected);

            int cap = hardCapMaxLevel < 0 ? 0 : hardCapMaxLevel;
            long silver = ReadServer(Currency.Silver);

            // SERVER-authoritative current level (record of truth), independent of the client's claim.
            int serverLevel = ReadServerLevel(CommanderProgressKey(commanderId), cap);

            // Optimistic-concurrency guard: the client's from-level must match the server's (anti-stale).
            if (expectedFromLevel != serverLevel)
                return new CommanderLevelResult(false, serverLevel, silver, UpgradeRejectReason.StaleClientLevel);

            // HARD CAP re-enforcement (§3/§6/§15): never advance at/over the cap, regardless of request.
            if (serverLevel >= cap)
                return new CommanderLevelResult(false, serverLevel, silver, UpgradeRejectReason.AtHardCap);

            // Cost sanity + sufficient funds (the client's cost is advisory; server holds the balance).
            if (costSilver < 0 || costSilver > _rules.MaxSpendPerCall)
                return new CommanderLevelResult(false, serverLevel, silver, UpgradeRejectReason.ServerRejected);
            if (costSilver > silver)
                return new CommanderLevelResult(false, serverLevel, silver, UpgradeRejectReason.InsufficientSilver);

            int newLevel = serverLevel + 1;
            long newSilver = silver - costSilver;
            if (newSilver < 0)
                return new CommanderLevelResult(false, serverLevel, silver, UpgradeRejectReason.InsufficientSilver);

            // Stage the atomic mutation (charge + level advance), then commit the FULL profile.
            _serverWallet[Key(Currency.Silver)] = newSilver;
            _serverProgress[CommanderProgressKey(commanderId)] = newLevel.ToString();

            if (!await CommitProfileAsync())
            {
                // Roll back the staged mutation on a failed write (DEFERRED: real server txn).
                _serverWallet[Key(Currency.Silver)] = silver;
                _serverProgress[CommanderProgressKey(commanderId)] = serverLevel.ToString();
                return new CommanderLevelResult(false, serverLevel, silver, UpgradeRejectReason.ServerRejected);
            }

            return new CommanderLevelResult(true, newLevel, newSilver, UpgradeRejectReason.None);
        }

        /// <summary>Last server-confirmed balance from the in-stub store (read-cache display, §9).
        /// Gold is not held here and returns 0. NEVER logged (§12). Satisfies BOTH the
        /// IServerEconomy and ICommanderEconomy CachedBalance members (identical contract).</summary>
        public long CachedBalance(Currency currency)
        {
            if (currency == Currency.Gold) return 0L; // Gold is in-battle/non-persistent (§9)
            return ReadServer(currency);
        }

        // ---------------------------------------------------------------- server-side helpers

        /// <summary>Reject Gold (in-battle / non-persistent, §9) and any unknown enum value.</summary>
        private static string ValidatePersistentCurrency(Currency c)
        {
            if (c == Currency.Gold) return EconomyError.GoldNotPersistent;
            if (c != Currency.Silver && c != Currency.Gems && c != Currency.PassXP)
                return EconomyError.Unknown;
            return null;
        }

        private async Task<bool> EnsureAuthenticatedAsync()
        {
            if (!string.IsNullOrEmpty(_playerId)) return true;
            AuthResult auth = await _backend.AuthenticateAsync();
            if (!auth.Success || string.IsNullOrEmpty(auth.PlayerId)) return false;
            _playerId = auth.PlayerId;

            // Seed the simulated server state from the server profile (record of truth). We copy the
            // persistent currencies (Gold is never present) AND the full Progress map, so subsequent
            // wallet writes preserve progression keys owned by 3.2/3.3.
            PlayerProfile profile = await _backend.GetProfileAsync(_playerId);
            if (profile.Wallet != null)
            {
                foreach (var kv in profile.Wallet)
                {
                    if (TryParseCurrency(kv.Key, out var cur) && cur != Currency.Gold)
                        _serverWallet[Key(cur)] = kv.Value < 0 ? 0 : kv.Value; // clamp any bad seed
                }
            }
            if (profile.Progress != null)
            {
                foreach (var kv in profile.Progress)
                    _serverProgress[kv.Key] = kv.Value;
            }
            return true;
        }

        private long ReadServer(Currency c)
            => _serverWallet.TryGetValue(Key(c), out var v) ? v : 0L;

        /// <summary>Read a server-owned upgrade level from the Progress map, clamped to [0, cap].
        /// Unparseable/garbage is treated as level 0 (a bad profile value cannot grant power).</summary>
        private int ReadServerLevel(string progressKey, int cap)
        {
            int lvl = 0;
            if (_serverProgress.TryGetValue(progressKey, out string raw) && int.TryParse(raw, out int parsed))
                lvl = parsed;
            if (lvl < 0) return 0;
            int c = cap < 0 ? 0 : cap;
            return lvl > c ? c : lvl;
        }

        /// <summary>
        /// Persist the CURRENT simulated server state to the (stubbed) backend as ONE self-consistent
        /// profile: the full persistent wallet AND the full Progress map. READ-MODIFY-WRITE — the
        /// Progress map is carried forward intact so a wallet-only change never erases the upgrade/
        /// commander progression keys (review integration issue #2). A real adapter would do this as
        /// an atomic server-side transaction. We do NOT log this payload (§12).
        /// </summary>
        private async Task<bool> CommitProfileAsync()
        {
            var wallet = new Dictionary<string, long>
            {
                { Key(Currency.Silver), ReadServer(Currency.Silver) },
                { Key(Currency.Gems),   ReadServer(Currency.Gems) },
                { Key(Currency.PassXP), ReadServer(Currency.PassXP) },
            };
            // Carry the entire Progress map forward (preserve upg.* / cmd.* keys we don't own here).
            var progress = new Dictionary<string, string>(_serverProgress, StringComparer.Ordinal);

            var profile = new PlayerProfile(_playerId, wallet, progress);
            return await _backend.WriteProfileAsync(profile);
        }

        private static string Key(Currency c) => c.ToString();

        /// <summary>The canonical Progress key for an upgrade track — MUST match UpgradesService
        /// .ProgressKey (3.2): "upg.{unitId}.{(int)stat}". Kept identical so stub and service agree.</summary>
        private static string UpgradeProgressKey(string unitId, UpgradeStat stat) => $"upg.{unitId}.{(int)stat}";

        /// <summary>The canonical Progress key for a commander level — MUST match ProgressionService
        /// .ProgressKey (3.3): "cmd.{commanderId}.level". Kept identical so stub and service agree.</summary>
        private static string CommanderProgressKey(string commanderId) => $"cmd.{commanderId}.level";

        private static bool TryParseCurrency(string name, out Currency c)
            => Enum.TryParse(name, ignoreCase: false, result: out c) && Enum.IsDefined(typeof(Currency), c);
    }
}
