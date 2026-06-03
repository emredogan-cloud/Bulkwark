// BULWARK — Analytics event taxonomy + typed emit wrapper (Phase 3 §13 3.5, §12).
// ONE consolidated analytics pipeline (the original's THREE collapse to one — §12 MODERNIZE).
// PRIVACY (INVIOLABLE): consent (UMP) required upstream; params are MINIMIZED and NON-PII;
// NEVER emit a save-state / wallet / profile payload (§12 "never log save state"); a data-deletion
// path is honored by the backend. Currency events carry a delta + reason CODE + currency NAME
// only — never a balance. Pure C# over the Phase-0 IAnalytics seam (CI-testable).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity / no analytics provider — ADR-0-001/-2-001).
// DEFERRED: events actually LANDING in a provider — no backend here.

using System.Collections.Generic;
using Bulwark.Data;
using Bulwark.Services.Analytics;

namespace Bulwark.Services.Analytics
{
    /// <summary>Stable event names — the consolidated taxonomy (one pipeline). No ad-hoc strings elsewhere.</summary>
    public static class AnalyticsEvents
    {
        public const string AppStart        = "app_start";
        public const string BattleStart     = "battle_start";
        public const string BattleEnd       = "battle_end";       // outcome + duration bucket
        public const string LevelClear      = "level_clear";      // campaign: index, first_clear, star
        public const string DraftComplete   = "draft_complete";   // spell draft choices (indices)
        public const string UpgradePurchase = "upgrade_purchase"; // unit, stat, new_level (capped)
        public const string CommanderLevel  = "commander_level";  // new_level (capped)
        public const string CurrencyGrant   = "currency_grant";   // currency, amount(delta), source
        public const string CurrencySpend   = "currency_spend";   // currency, amount(delta), reason
        public const string EndlessWave     = "endless_wave";     // wave, survived
        public const string LadderSubmit    = "ladder_submit";    // result, rank_bucket
        public const string RcActivated     = "rc_activated";     // config version (live retune landed)
        public const string AdOptIn         = "ad_optin";         // Phase-4 placeholder name only; NOT emitted here
    }

    /// <summary>
    /// Typed wrapper over <see cref="IAnalytics"/> that builds minimized, non-PII parameter maps.
    /// No method here accepts or emits a balance/profile/save payload (§12). One pipeline.
    /// </summary>
    public sealed class GameAnalytics
    {
        private readonly IAnalytics _sink;
        public GameAnalytics(IAnalytics sink) { _sink = sink; }

        private static Dictionary<string, object> P(params (string k, object v)[] kv)
        {
            var d = new Dictionary<string, object>(kv.Length);
            foreach (var (k, v) in kv) d[k] = v;
            return d;
        }

        public void BattleStart(string mode, string mapId)
            => _sink.Emit(AnalyticsEvents.BattleStart, P(("mode", mode), ("map", mapId)));

        public void BattleEnd(string mode, string outcome, int durationSecondsBucket)
            => _sink.Emit(AnalyticsEvents.BattleEnd, P(("mode", mode), ("outcome", outcome), ("dur_bucket", durationSecondsBucket)));

        public void LevelClear(int index, bool firstClear, bool star)
            => _sink.Emit(AnalyticsEvents.LevelClear, P(("index", index), ("first_clear", firstClear), ("star", star)));

        public void UpgradePurchase(string unitId, UpgradeStat stat, int newLevel)
            => _sink.Emit(AnalyticsEvents.UpgradePurchase, P(("unit", unitId), ("stat", stat.ToString()), ("new_level", newLevel)));

        public void CommanderLevel(Faction faction, int newLevel)
            => _sink.Emit(AnalyticsEvents.CommanderLevel, P(("faction", faction.ToString()), ("new_level", newLevel)));

        // Currency events carry the DELTA + currency NAME + reason CODE only — NEVER a balance (§12).
        public void CurrencyGrant(Currency currency, long amount, string source)
            => _sink.Emit(AnalyticsEvents.CurrencyGrant, P(("currency", currency.ToString()), ("amount", amount), ("source", source)));

        public void CurrencySpend(Currency currency, long amount, string reason)
            => _sink.Emit(AnalyticsEvents.CurrencySpend, P(("currency", currency.ToString()), ("amount", amount), ("reason", reason)));

        public void EndlessWave(int wave, bool survived)
            => _sink.Emit(AnalyticsEvents.EndlessWave, P(("wave", wave), ("survived", survived)));

        public void LadderSubmit(string result, int rankBucket)
            => _sink.Emit(AnalyticsEvents.LadderSubmit, P(("result", result), ("rank_bucket", rankBucket)));

        public void RcActivated(string configVersion)
            => _sink.Emit(AnalyticsEvents.RcActivated, P(("version", configVersion)));
    }
}
