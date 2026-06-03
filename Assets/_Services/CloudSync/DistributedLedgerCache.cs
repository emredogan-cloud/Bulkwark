// BULWARK — obscured CLIENT CRDT ledger CACHE (Phase 3 §13 3.1, §9, §12).
//
// !!! NOT THE SOURCE OF TRUTH (HARD RULE #1, §9). !!!
// Currency is SERVER-OWNED. This is a CLIENT-SIDE, per-device CRDT cache used ONLY for:
//   (a) offline / pre-sync DISPLAY of a best-effort balance, and
//   (b) tamper-DETERRENCE (a redundant, commutatively-merged shadow ledger that makes naive
//       single-field save edits inconsistent — deterrence, NOT prevention/validation).
// It NEVER decides a balance. The server's WalletSnapshot always WINS: ReconcileToServer
// OVERWRITES this cache from the server (server-authoritative reconciliation). Nothing reads
// this cache as truth; the authoritative path is IServerEconomy → Wallet (server response only).
//
// WIRING NOTE (review): this cache is FED by the composition root, not by itself. After a
// CONFIRMED Wallet.GrantAsync/SpendAsync, call NoteConfirmedGrant/NoteConfirmedSpend; after a
// RefreshFromServerAsync (or any server read), call ReconcileToServer(snapshot). That wiring is an
// integration step (see integrationNotes) — these methods exist precisely so the cache is fed and
// reconciled at the seam, never ahead of the server.
//
// NO PERSISTENCE-AS-TRUTH: even if persisted to disk for offline display, the persisted value is
// a CACHE, discarded/overwritten on the next reconcile. NO SAVE-STATE LOGGING (HARD RULE #2):
// this type never logs ledger contents / balances / device ids.
//
// CRDT shape: a per-currency PN-counter (a state-based CvRDT). Each device owns two grow-only
// maps — positive (grants) and negative (spends) — keyed by device id. Merge takes the
// element-wise MAX per device (idempotent, commutative, associative), so out-of-order / duplicated
// per-device updates converge without coordination. The displayed value = Σpositive − Σnegative.
// This is for DISPLAY/DETERRENCE convergence only — it does not authorize spend/grant.
//
// PURE C# (no UnityEngine) so it is CI-unit-testable like ConfigResolver (§12).
// SCAFFOLD STATUS: authored, NOT compiled (ADR-0-001 / ADR-2-001). DEFERRED: real device-id
// provisioning, the cross-device gossip/merge transport, and the obscuring/at-rest encoding pass.

using System;
using System.Collections.Generic;
using Bulwark.Data;
using Bulwark.Services.Economy;

namespace Bulwark.Services.CloudSync
{
    /// <summary>
    /// Per-currency PN-counter (positive = grants, negative = spends), keyed by device id.
    /// Grow-only maps merged by per-key MAX — a standard CvRDT. Value = Σpos − Σneg, clamped ≥ 0.
    /// </summary>
    internal sealed class PnCounter
    {
        private readonly Dictionary<string, long> _positive = new Dictionary<string, long>();
        private readonly Dictionary<string, long> _negative = new Dictionary<string, long>();

        /// <summary>This device records a grant of `amount` (monotone increment of its P entry).</summary>
        public void Increment(string deviceId, long amount)
        {
            if (amount <= 0) return;
            _positive.TryGetValue(deviceId, out var p);
            _positive[deviceId] = p + amount;
        }

        /// <summary>This device records a spend of `amount` (monotone increment of its N entry).</summary>
        public void Decrement(string deviceId, long amount)
        {
            if (amount <= 0) return;
            _negative.TryGetValue(deviceId, out var n);
            _negative[deviceId] = n + amount;
        }

        /// <summary>CRDT merge: per-device MAX of P and of N (idempotent/commutative/associative).</summary>
        public void Merge(PnCounter other)
        {
            if (other == null) return;
            MergeMap(_positive, other._positive);
            MergeMap(_negative, other._negative);
        }

        private static void MergeMap(Dictionary<string, long> into, Dictionary<string, long> from)
        {
            foreach (var kv in from)
            {
                if (!into.TryGetValue(kv.Key, out var cur) || kv.Value > cur)
                    into[kv.Key] = kv.Value;
            }
        }

        /// <summary>Best-effort DISPLAY value (Σpos − Σneg), clamped ≥ 0. NOT authoritative.</summary>
        public long Value()
        {
            long pos = 0, neg = 0;
            foreach (var v in _positive.Values) pos += v;
            foreach (var v in _negative.Values) neg += v;
            long v2 = pos - neg;
            return v2 < 0 ? 0 : v2;
        }

        /// <summary>Server-wins RESET: clear the CRDT history and seed it so Value() == authoritative.
        /// The seed lands under a synthetic "server" key so a later reconcile can overwrite it again.</summary>
        public void OverwriteFromServer(long authoritativeBalance)
        {
            _positive.Clear();
            _negative.Clear();
            _positive[ServerKey] = authoritativeBalance < 0 ? 0 : authoritativeBalance;
        }

        internal const string ServerKey = "@server"; // reserved device key for reconciled truth
    }

    /// <summary>
    /// The obscured CLIENT CRDT cache for the persistent currencies (Silver/Gems/PassXP). Gold is
    /// excluded (in-battle / non-persistent, §9). Offline DISPLAY + tamper-DETERRENCE ONLY —
    /// reconciled to (and overwritten by) the server. NOT the source of truth (§9).
    /// </summary>
    public sealed class DistributedLedgerCache
    {
        private readonly string _deviceId;
        private readonly Dictionary<Currency, PnCounter> _ledgers = new Dictionary<Currency, PnCounter>
        {
            { Currency.Silver, new PnCounter() },
            { Currency.Gems,   new PnCounter() },
            { Currency.PassXP, new PnCounter() },
        };

        /// <param name="deviceId">Opaque per-device id (NOT PII; never logged). DEFERRED: real provisioning.</param>
        public DistributedLedgerCache(string deviceId)
        {
            _deviceId = string.IsNullOrEmpty(deviceId) ? "device-local" : deviceId;
        }

        /// <summary>
        /// Record a server-CONFIRMED grant into the cache, so offline display stays roughly current
        /// between syncs. Call ONLY after the authoritative grant succeeded — this cache must never
        /// lead the server. Gold is ignored (§9). Does not authorize anything.
        /// </summary>
        public void NoteConfirmedGrant(Currency c, long amount)
        {
            if (!_ledgers.TryGetValue(c, out var pn)) return; // Gold / unknown → ignored
            pn.Increment(_deviceId, amount);
        }

        /// <summary>
        /// Record a server-CONFIRMED spend into the cache (mirror of NoteConfirmedGrant). Call ONLY
        /// after the authoritative spend succeeded. Gold is ignored (§9).
        /// </summary>
        public void NoteConfirmedSpend(Currency c, long amount)
        {
            if (!_ledgers.TryGetValue(c, out var pn)) return;
            pn.Decrement(_deviceId, amount);
        }

        /// <summary>
        /// CRDT merge with another device's cache (e.g. multi-device same account). Commutative /
        /// idempotent — order and duplicates don't matter; it converges. Display/deterrence only.
        /// </summary>
        public void MergeFrom(DistributedLedgerCache other)
        {
            if (other == null) return;
            foreach (var kv in _ledgers)
                if (other._ledgers.TryGetValue(kv.Key, out var theirs))
                    kv.Value.Merge(theirs);
        }

        /// <summary>
        /// Best-effort cached DISPLAY balance for offline/pre-sync UI. NOT authoritative — the
        /// server (Wallet) value supersedes this the moment a sync completes. Gold throws (§9).
        /// </summary>
        public long DisplayBalance(Currency c)
        {
            if (c == Currency.Gold)
                throw new InvalidOperationException(
                    "Gold is in-battle/non-persistent (§9) — not represented in the ledger cache.");
            return _ledgers.TryGetValue(c, out var pn) ? pn.Value() : 0L;
        }

        /// <summary>
        /// SERVER WINS. Overwrite the cache from the authoritative <see cref="WalletSnapshot"/> — a
        /// hard reset of every CRDT to the server's value, discarding any local drift (HARD RULE #1,
        /// §9). This is the reconcile seam called after a server read. NEVER logs the snapshot (§12).
        /// </summary>
        public void ReconcileToServer(WalletSnapshot serverTruth)
        {
            if (serverTruth.Balances == null) return;
            foreach (var kv in _ledgers)
                kv.Value.OverwriteFromServer(serverTruth.Get(kv.Key));
        }
    }
}
