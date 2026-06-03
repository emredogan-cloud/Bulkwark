// BULWARK — Async Ghost Ladder CONTROLLER (meta/mode MonoBehaviour). Roadmap §13 3.4, §7
// ("Tournament / Async Ladder — async vs opponent ghosts/snapshots; climb a ladder; reward = Silver"),
// §3 (recovered async ghost ladder), §9 (4 MVP currencies; Silver is the ladder reward).
// DECISION-LOG §3: NON-deterministic sim + SERVER STAT-SANITY checks; NO deterministic replay.
// §12 boundary: this is the META/MODE layer = a MonoBehaviour service (Bulwark.Game.Modes), NOT the
// ECS battle-sim. It ORCHESTRATES: capture my snapshot → request an opponent ghost → launch an async
// battle vs that ghost (reusing the existing ECS battle via BattleBootstrap, with the ghost as the AI
// side) → submit the result to the SERVER, which applies stat-sanity + updates rank + grants Silver.
//
// INVIOLABLE Phase-3 hard rules honored here:
//   1. SERVER AUTHORITY over currency/rank (INVIOLABLE): this controller NEVER mutates a balance or a
//      rank locally. Rewards (EconomyConfig.silverPerLadderWin) and rank are SERVER-GRANTED; the client
//      only updates its CACHE from the server's confirmed receipt. There is no client-trusted win.
//   2. NO save-state logging (INVIOLABLE): analytics emit event NAMES only — never wallet/profile/
//      snapshot payloads. The IAnalytics seam already refuses to persist payloads (StubAnalytics).
//   3. CAPPED progression / no P2W: the SERVER runs StatSanityValidator (over-cap ghosts are rejected).
//   6. NO deterministic replay: the ladder validates via SERVER STAT-SANITY, not replay verification.
//
// The server round-trip (matchmaking, stat-sanity decision, rank update, Silver grant) is represented
// by a SEAM (IAsyncLadderBackend) + a STUB. The generic IBackendClient deliberately has no ladder
// methods (it is auth/profile/config only); the real BaaS adapter implements BOTH. Every server hop is
// DEFERRED (no backend in this environment) and clearly marked.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / no BaaS — ADR-0-001/-2-001). GATE 3 +
// runtime/playable-mode validation are DEFERRED (ADR-2-001: Phase 3 is AUTHOR-ONLY).

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Bulwark.Data;
using Bulwark.Services.Analytics;

namespace Bulwark.Game.Modes.AsyncLadder
{
    // =====================================================================================
    // SERVER SEAM — the ladder's server contract. Implemented by the BaaS adapter; here a STUB.
    // SERVER-AUTHORITATIVE: matchmaking + stat-sanity DECISION + rank + currency all live here.
    // =====================================================================================

    /// <summary>Server's confirmed response to a submitted ladder result. The CLIENT TRUSTS NOTHING
    /// it computed locally — it reads rank + the granted Silver from THIS receipt and updates its
    /// cache. A rejected (stat-sanity-failed) result grants nothing and does not move rank.</summary>
    public readonly struct LadderResultReceipt
    {
        public readonly bool Accepted;            // server's stat-sanity + result verdict
        public readonly SanityRejectReason RejectReason; // None when Accepted
        public readonly int ServerConfirmedRank;  // authoritative rank AFTER this match (cache target)
        public readonly long SilverGranted;       // server-granted Silver (§7/§9); 0 on loss/reject
        public readonly string Error;             // transport error (non-null ⇒ call failed)

        public LadderResultReceipt(bool accepted, SanityRejectReason rejectReason,
                                   int serverConfirmedRank, long silverGranted, string error)
        {
            Accepted = accepted;
            RejectReason = rejectReason;
            ServerConfirmedRank = serverConfirmedRank;
            SilverGranted = silverGranted;
            Error = error;
        }
    }

    /// <summary>What the client submits after an async battle. The server RE-VALIDATES everything
    /// (it does not trust the reported outcome for rank/currency); the client-side fields exist so the
    /// server can correlate the match, then applies its OWN stat-sanity + result logic.</summary>
    public readonly struct LadderResultSubmission
    {
        public readonly string PlayerId;          // opaque server id (from AuthResult)
        public readonly string OpponentSnapshotId; // which ghost was fought
        public readonly LadderBattleOutcome Outcome; // player-perspective outcome (server re-checks)
        public readonly GhostSnapshot PlayerSnapshot; // the player's OWN snapshot from this battle

        public LadderResultSubmission(string playerId, string opponentSnapshotId,
                                      LadderBattleOutcome outcome, GhostSnapshot playerSnapshot)
        {
            PlayerId = playerId;
            OpponentSnapshotId = opponentSnapshotId;
            Outcome = outcome;
            PlayerSnapshot = playerSnapshot;
        }
    }

    /// <summary>
    /// The async-ladder server contract (matchmaking + submission). SERVER-AUTHORITATIVE. The BaaS
    /// adapter implements this alongside IBackendClient. ALL methods are server round-trips (DEFERRED).
    /// </summary>
    public interface IAsyncLadderBackend
    {
        /// <summary>Server-side matchmaking: returns an opponent ghost near the player's rank. The
        /// snapshot it returns has ALREADY passed server stat-sanity at ingestion, so the client can
        /// trust its SHAPE (not its claimed result). DEFERRED (stub returns a synthetic ghost).</summary>
        Task<GhostSnapshot> RequestOpponentGhostAsync(string playerId);

        /// <summary>Persist the player's own snapshot for OTHERS to fight. The server runs
        /// StatSanityValidator at ingestion and drops cheats. DEFERRED. Returns server accept/verdict.</summary>
        Task<SanityResult> SubmitOwnSnapshotAsync(GhostSnapshot mySnapshot);

        /// <summary>Submit an async battle result. The SERVER applies stat-sanity + result logic,
        /// updates rank, and GRANTS Silver on a clean win (§7). Returns the authoritative receipt the
        /// client caches. DEFERRED.</summary>
        Task<LadderResultReceipt> SubmitResultAsync(LadderResultSubmission submission);
    }

    /// <summary>
    /// No-op-ish STUB so the orchestration is exercisable without a backend (mirrors StubBackendClient).
    /// It returns deterministic placeholder data and does the stat-sanity + grant locally ONLY to model
    /// the server's job — in production this code path lives on the SERVER, not the client. The client
    /// still treats the receipt as authoritative (it never grants on its own). DEFERRED → real BaaS.
    /// </summary>
    public sealed class StubAsyncLadderBackend : IAsyncLadderBackend
    {
        private readonly LadderSanityBounds _bounds;
        private readonly UpgradeCapTable _caps;
        private readonly long _silverPerWin;
        private int _rank;

        /// <param name="bounds">Resolved EconomyConfig ladder bounds (server-supplied in production).</param>
        /// <param name="caps">Resolved UpgradesConfig caps (server-supplied in production).</param>
        /// <param name="silverPerWin">EconomyConfig.silverPerLadderWin (server-owned reward rate).</param>
        /// <param name="startingRank">Placeholder starting rank for the stub.</param>
        public StubAsyncLadderBackend(LadderSanityBounds bounds, UpgradeCapTable caps,
                                      long silverPerWin, int startingRank = 1000)
        {
            _bounds = bounds;
            _caps = caps;
            _silverPerWin = silverPerWin;
            _rank = startingRank;
        }

        public Task<GhostSnapshot> RequestOpponentGhostAsync(string playerId)
        {
            // Synthetic, plausible opponent (in-bounds) so a local async battle can stand up.
            // Real matchmaking (rank-banded snapshot selection) is DEFERRED to the server.
            var ghost = new GhostSnapshot
            {
                snapshotId = "stub-ghost",
                ownerPlayerId = "stub-opponent",
                schemaVersion = 1,
                factionId = (int)Faction.AshenHorde,
                units = new List<GhostUnitEntry>
                {
                    new GhostUnitEntry
                    {
                        unitId = "stub_unit", count = 5,
                        upgradeLevels = new Dictionary<int, int>(),
                        baseAttackDamage = 10f, baseAttackInterval = 1f,
                    },
                },
                draftedSpellIds = new List<string>(),
                commanderId = null,
                commanderLevel = 0,
                totalUnitsFielded = 5,
                capturedOutcome = LadderBattleOutcome.Win,
                capturedUtcTicks = 0L,
            };
            return Task.FromResult(ghost);
        }

        public Task<SanityResult> SubmitOwnSnapshotAsync(GhostSnapshot mySnapshot)
            => Task.FromResult(StatSanityValidator.Validate(in mySnapshot, in _bounds, _caps));

        public Task<LadderResultReceipt> SubmitResultAsync(LadderResultSubmission submission)
        {
            // SERVER's job (modeled here): re-run stat-sanity on the player's submitted snapshot; on a
            // clean WIN, move rank + grant Silver. A reject grants nothing and does not move rank.
            SanityResult sanity = StatSanityValidator.Validate(in submission.PlayerSnapshot, in _bounds, _caps);
            if (!sanity.Accepted)
                return Task.FromResult(new LadderResultReceipt(false, sanity.Reason, _rank, 0L, null));

            long silver = 0L;
            if (submission.Outcome == LadderBattleOutcome.Win)
            {
                _rank += 1;            // placeholder ladder math; real ELO/rank is server-owned (DEFERRED).
                silver = _silverPerWin; // server-granted Silver (§7).
            }
            return Task.FromResult(new LadderResultReceipt(true, SanityRejectReason.None, _rank, silver, null));
        }
    }

    // =====================================================================================
    // BATTLE-LAUNCH SEAM — how the controller stands up the async battle (reuses BattleBootstrap).
    // Declared as an interface so the meta module does NOT hard-reference the ECS sim/bootstrap
    // assembly (§12 boundary). The Bootstrap/Control layer implements this against BattleBootstrap.
    // =====================================================================================

    /// <summary>
    /// Adapter the controller uses to LAUNCH an async battle with a ghost as the AI side and to learn
    /// the outcome — without the meta module referencing Bulwark.Bootstrap/Bulwark.Sim directly.
    /// A thin implementation in the Bootstrap/Control assembly maps the ghost onto BattleBootstrap's
    /// AI-side inputs (aiUnits/aiCommander/spell loadout) and reports the resolved MatchOutcome.
    /// See integrationNotes for the exact BattleBootstrap wiring. The launch itself is DEFERRED
    /// (no Unity runtime here).
    /// </summary>
    public interface IAsyncBattleLauncher
    {
        /// <summary>Configure + launch one async battle: PLAYER is Team 0, the GHOST is the AI side
        /// (Team 1). Resolves to the player-perspective outcome when the battle ends.</summary>
        Task<LadderBattleOutcome> LaunchVsGhostAsync(GhostSnapshot opponentGhost);

        /// <summary>Capture the PLAYER's own army snapshot from the just-finished battle (faction,
        /// composition with applied capped-upgrade levels, drafted spells, commander+level, outcome).
        /// Pure read of authored DATA + the player's server-confirmed upgrade levels — NO save payload.</summary>
        GhostSnapshot CapturePlayerSnapshot(LadderBattleOutcome outcome);
    }

    // =====================================================================================
    // THE CONTROLLER — orchestrates the loop. MonoBehaviour (meta layer, §12).
    // =====================================================================================

    /// <summary>
    /// Drives one async-ladder cycle: request a ghost, fight it (player vs ghost-as-AI via the launcher),
    /// capture the player's own snapshot, and submit the result to the SERVER (which validates + grants).
    /// Holds NO authoritative state: <see cref="CachedRank"/> / <see cref="CachedSilverDelta"/> are a
    /// DISPLAY CACHE updated ONLY from the server receipt (§12 "client cache under server authority").
    /// Assign the EconomyConfig + UpgradesConfig (for bounds/caps the SERVER would resolve), the backend
    /// seam, the launcher, and the analytics seam in the inspector / via Initialize().
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AsyncLadderController : MonoBehaviour
    {
        [Header("Economy / caps source (§13 3.1/3.2 — server-owned; here for bound/cap resolution)")]
        [Tooltip("EconomyConfig: ladderMaxPlausibleDps / ladderMaxUnitsPerBattle / silverPerLadderWin. " +
                 "The SERVER resolves these via the 3-tier resolver (RC→SO→literal); this SO is the SO/literal tier.")]
        public EconomyConfig economyConfig;

        [Tooltip("UpgradesConfig: per-track maxLevel caps + commanderMaxLevel — the HARD caps the server " +
                 "enforces in stat-sanity (§3, no P2W).")]
        public UpgradesConfig upgradesConfig;

        // ---- Seams (injected; not authored in the inspector since they are interfaces) ----
        private IAsyncLadderBackend _backend;   // SERVER seam (BaaS adapter; stub here). DEFERRED.
        private IAsyncBattleLauncher _launcher; // reuses BattleBootstrap for the actual fight.
        private IAnalytics _analytics;          // event-NAME-only emit (no payloads, §2).

        // ---- DISPLAY CACHE ONLY (never authoritative) — reconciled to the server receipt (§12) ----
        /// <summary>Last server-confirmed rank (cache for UI). NOT trusted for anything authoritative.</summary>
        public int CachedRank { get; private set; }

        /// <summary>Last server-GRANTED Silver delta (cache for a reward popup). The wallet itself is
        /// server-owned; this is display only.</summary>
        public long CachedSilverDelta { get; private set; }

        /// <summary>True once a cycle has reconciled at least one server receipt.</summary>
        public bool HasServerState { get; private set; }

        private bool _busy;

        /// <summary>
        /// Inject the seams. In production: the BaaS adapter (IAsyncLadderBackend), a BattleBootstrap-backed
        /// IAsyncBattleLauncher, and the consolidated IAnalytics. In tests/local: the stubs. The bounds +
        /// caps the SERVER uses are derived here from the assigned configs (in production the SERVER
        /// resolves them itself via RemoteConfig — this client-side derivation is for the stub backend only).
        /// </summary>
        public void Initialize(IAsyncLadderBackend backend, IAsyncBattleLauncher launcher, IAnalytics analytics)
        {
            _backend = backend;
            _launcher = launcher;
            _analytics = analytics ?? new StubAnalytics();
        }

        /// <summary>
        /// Run ONE async-ladder cycle. Safe to call from a button; no-ops if a cycle is in flight or the
        /// seams are not wired. Every server hop is awaited and is DEFERRED (stub) in this environment.
        /// </summary>
        public async Task RunLadderCycleAsync(string playerId)
        {
            if (_busy) return;
            if (_backend == null || _launcher == null)
            {
                Debug.LogError("[AsyncLadderController] Not initialized — call Initialize() with backend + launcher seams.");
                return;
            }
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogError("[AsyncLadderController] playerId is empty — authenticate first (IBackendClient.AuthenticateAsync).");
                return;
            }

            _busy = true;
            try
            {
                Emit("ladder_cycle_start"); // NAME ONLY — no payload (§2 no save-state logging).

                // 1) Request an opponent ghost (SERVER matchmaking). DEFERRED → stub.
                GhostSnapshot opponent = await _backend.RequestOpponentGhostAsync(playerId);
                Emit("ladder_ghost_received");

                // 2) Launch the async battle: PLAYER (Team 0) vs the GHOST as the AI side (Team 1),
                //    reusing the existing ECS battle through BattleBootstrap. DEFERRED (no Unity runtime).
                LadderBattleOutcome outcome = await _launcher.LaunchVsGhostAsync(opponent);
                Emit(outcome == LadderBattleOutcome.Win ? "ladder_battle_win" : "ladder_battle_loss");

                // 3) Capture the PLAYER's own snapshot for others to fight (composition + capped levels).
                GhostSnapshot mySnapshot = _launcher.CapturePlayerSnapshot(outcome);

                //    Persist my snapshot (server runs stat-sanity at ingestion). DEFERRED → stub.
                SanityResult ingest = await _backend.SubmitOwnSnapshotAsync(mySnapshot);
                if (!ingest.Accepted)
                {
                    // My OWN snapshot failed server sanity (would only happen if the client were tampered).
                    // We never block the player's result on this; record + continue. Server decides all.
                    Emit("ladder_own_snapshot_rejected");
                }

                // 4) Submit the result. The SERVER applies stat-sanity + result logic, updates rank, and
                //    GRANTS Silver on a clean win (§7). The client CACHES the receipt — it never grants.
                var submission = new LadderResultSubmission(playerId, opponent.snapshotId, outcome, mySnapshot);
                LadderResultReceipt receipt = await _backend.SubmitResultAsync(submission);

                if (!string.IsNullOrEmpty(receipt.Error))
                {
                    Emit("ladder_submit_error"); // NAME only — never the error payload/save state.
                    return; // leave the cache untouched; the server remains the source of truth.
                }

                // 5) RECONCILE the display cache to the server's confirmed receipt (§12). This is the ONLY
                //    place rank/reward enter the client — read from the server, never computed locally.
                ReconcileFromServer(receipt);
                Emit(receipt.Accepted ? "ladder_result_accepted" : "ladder_result_rejected");
            }
            catch (Exception)
            {
                // Never log the exception payload (it could carry profile/snapshot data) — NAME only (§2).
                Emit("ladder_cycle_exception");
            }
            finally
            {
                _busy = false;
            }
        }

        /// <summary>
        /// Update the DISPLAY CACHE from the server receipt. The CRDT/distributed cache is reconciled to
        /// THIS — the server is the source of truth (§12 / hard rule 1). No local currency mutation: we
        /// store what the server GRANTED for display; the authoritative wallet lives on the server and is
        /// re-read via IBackendClient.GetProfileAsync when the meta UI needs the true balance.
        /// </summary>
        private void ReconcileFromServer(in LadderResultReceipt receipt)
        {
            CachedRank = receipt.ServerConfirmedRank;
            CachedSilverDelta = receipt.Accepted ? receipt.SilverGranted : 0L;
            HasServerState = true;
            // NOTE: we do NOT write any balance anywhere. The reward is already on the server wallet;
            // CachedSilverDelta is only for a "+N Silver" popup. A balance refresh is a server READ.
        }

        /// <summary>Resolve the ladder sanity bounds from EconomyConfig (SO/literal tier). In production
        /// the SERVER resolves these (RC→SO→literal); exposed so the stub backend can be wired locally.</summary>
        public LadderSanityBounds ResolveBounds()
        {
            if (economyConfig == null)
                return new LadderSanityBounds(1000f, 60); // literal fallback = EconomyConfig defaults.
            return new LadderSanityBounds(economyConfig.ladderMaxPlausibleDps, economyConfig.ladderMaxUnitsPerBattle);
        }

        /// <summary>Build the server's cap table from UpgradesConfig (per-track maxLevel + commander cap).
        /// In production this is SERVER-SIDE; exposed for local stub wiring.</summary>
        public UpgradeCapTable ResolveCaps()
        {
            var trackCaps = new Dictionary<(string, int), int>();
            int commanderMax = 10; // literal fallback = UpgradesConfig.commanderMaxLevel default.
            if (upgradesConfig != null)
            {
                commanderMax = upgradesConfig.commanderMaxLevel;
                if (upgradesConfig.tracks != null)
                {
                    for (int i = 0; i < upgradesConfig.tracks.Count; i++)
                    {
                        UpgradeTrack t = upgradesConfig.tracks[i];
                        if (string.IsNullOrEmpty(t.unitId)) continue;
                        trackCaps[(t.unitId, (int)t.stat)] = t.maxLevel;
                    }
                }
            }
            return new UpgradeCapTable(trackCaps, commanderMax);
        }

        /// <summary>Convenience: build a local STUB backend from the assigned configs (no BaaS here).
        /// Production swaps in the BaaS adapter via Initialize(). DEFERRED for the real server.</summary>
        public IAsyncLadderBackend CreateStubBackend()
        {
            long silverPerWin = economyConfig != null ? economyConfig.silverPerLadderWin : 40L;
            return new StubAsyncLadderBackend(ResolveBounds(), ResolveCaps(), silverPerWin);
        }

        private void Emit(string eventName)
        {
            // Event NAME only — NO parameters, NO wallet/profile/snapshot payload (hard rule 2).
            _analytics?.Emit(eventName);
        }
    }
}
