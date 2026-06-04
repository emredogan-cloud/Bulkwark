// BULWARK — WEEKEND MODIFIERS (Phase 4 §13 4.5, §7 "basic weekend modifiers" ONLY).
//
// SCOPE (§7 / decision-log §2, BINDING): this is the BASIC weekend-modifier lever — NOT the full event
// engine (that is Phase 7, DEFERRED). A weekend modifier may ONLY (a) tune bounded REWARD pacing
// (Silver / PassXP multipliers) or (b) toggle an EXISTING combat rule by key. It introduces NO new
// mechanic and NO pay-for-power. "Modifiers are data, not new systems" (§7).
//
// FAIRNESS (GATE 4): multipliers are bounded (schema [Range(1,3)]; re-clamped fail-closed here) and apply
// ONLY to Silver / PassXP — never to Gems and never to anything power-bearing (there is no power reward).
// The existingRuleKey is validated against an ALLOW-LIST of KNOWN existing rules; an unknown key is
// IGNORED (fail-closed) so a weekend can never smuggle in a NEW mechanic (§7). No new currency (§9).
//
// SERVER-AUTHORITATIVE (§12): WHICH modifier is active for the current weekend is SERVER-OWNED (issued
// against trusted server time, never the device clock). Reward multipliers are SERVER-APPLIED on the
// grant; this client service computes the DISPLAY/advisory value and the server re-applies + re-validates
// the bound. No client-authored reward inflation.
//
// PURE C# (no UnityEngine in the logic) so it is CI-unit-testable like the other live-ops services
// (§12 boundary: meta/live-ops = services, NOT the ECS battle sim). SCAFFOLD STATUS: authored, NOT
// compiled here (CI validates). DEFERRED: the server-issued active-weekend feed + server-applied grants.

using System;
using System.Collections.Generic;
using Bulwark.Data;

namespace Bulwark.Services.LiveOps
{
    /// <summary>
    /// Basic weekend-modifier service (§13 4.5). Holds the SERVER-issued active <see cref="WeekendModifierDef"/>
    /// for the current weekend, exposes its BOUNDED reward multipliers (Silver/PassXP only) and its
    /// validated existing-rule toggle, and applies the multiplier to a base reward amount for display
    /// (the server re-applies authoritatively). Introduces no new mechanic, no new currency, no power.
    /// </summary>
    public sealed class WeekendModifierService
    {
        /// <summary>The bound on any reward multiplier (matches WeekendModifierDef [Range(1,3)]). A modifier
        /// can at most TRIPLE a soft reward — never more (anti reward-inflation), never less than 1× (a
        /// weekend never PENALIZES rewards). Re-clamped here so a bad data value can't exceed the bound.</summary>
        public const float MinMultiplier = 1f;
        public const float MaxMultiplier = 3f;

        /// <summary>
        /// The ALLOW-LIST of EXISTING combat/mode rules a weekend may toggle (§7 "no new systems"). A
        /// modifier's existingRuleKey is honored ONLY if it appears here; anything else is ignored
        /// (fail-closed) so a weekend can never introduce a new mechanic. Each entry MUST map to a rule
        /// that already exists in the sim/modes:
        ///   • "endless_faster_waves" → the EXISTING Endless AdaptiveDirector wave-pacing knob (Phase 3).
        /// LSD may extend this list ONLY by adding keys for rules that ALREADY exist (never a new mechanic).
        /// </summary>
        // Typed HashSet (not IReadOnlyCollection) so membership uses the O(1) INSTANCE Contains —
        // IReadOnlyCollection<string> has no Contains, and without a System.Linq import the compiler
        // mis-binds ".Contains" to the span MemoryExtensions.Contains (CS7036). HashSet avoids both.
        public static readonly HashSet<string> KnownExistingRuleKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "endless_faster_waves",
        };

        private WeekendModifierDef _active; // server-issued active modifier; null = no weekend modifier running

        /// <summary>Adopt the SERVER-issued active weekend modifier (trusted-time owned). null clears it.
        /// SERVER WINS — the client never decides which weekend is live (anti device-clock gaming).</summary>
        public void SetActive(WeekendModifierDef active) => _active = active;

        /// <summary>True iff a weekend modifier is currently active.</summary>
        public bool IsActive => _active != null;

        /// <summary>The active modifier's id (for UI/analytics), or null when none is active.</summary>
        public string ActiveId => _active != null ? _active.id : null;

        /// <summary>The active modifier's display name, or null when none is active.</summary>
        public string ActiveDisplayName => _active != null ? _active.displayName : null;

        /// <summary>The bounded Silver reward multiplier (1× when no modifier / out of range). Server-applied.</summary>
        public float SilverRewardMultiplier => _active != null ? Clamp(_active.silverRewardMultiplier) : 1f;

        /// <summary>The bounded PassXP reward multiplier (1× when no modifier / out of range). Server-applied.</summary>
        public float PassXpMultiplier => _active != null ? Clamp(_active.passXpMultiplier) : 1f;

        /// <summary>
        /// The active EXISTING-rule toggle, or null if none / not on the allow-list. A key the modifier
        /// declares but that is NOT a KnownExistingRuleKey is IGNORED (fail-closed) — a weekend can only
        /// toggle rules that already exist, never add one (§7). Modes query this to flip an existing knob.
        /// </summary>
        public string ActiveExistingRuleKey
        {
            get
            {
                if (_active == null) return null;
                string key = _active.existingRuleKey;
                return IsKnownExistingRule(key) ? key : null; // unknown ⇒ ignored (no new mechanic)
            }
        }

        /// <summary>True iff <paramref name="ruleKey"/> names an existing rule a weekend is allowed to toggle.</summary>
        public static bool IsKnownExistingRule(string ruleKey)
            => !string.IsNullOrEmpty(ruleKey) && KnownExistingRuleKeys.Contains(ruleKey);

        /// <summary>
        /// The DISPLAY/advisory multiplied amount for a base reward under the active weekend (the SERVER
        /// re-applies + re-validates authoritatively on grant). ONLY Silver and PassXP are scaled — Gems
        /// and every other kind pass through UNCHANGED (no gem inflation; no power exists to scale). The
        /// result is clamped to a non-negative int with an overflow guard. A non-positive base returns 0.
        /// </summary>
        public int ApplyRewardMultiplier(RewardKind kind, int baseAmount)
        {
            if (baseAmount <= 0) return baseAmount < 0 ? 0 : 0;

            float mult;
            switch (kind)
            {
                case RewardKind.Silver: mult = SilverRewardMultiplier; break;
                case RewardKind.PassXP: mult = PassXpMultiplier;       break;
                default: return baseAmount; // Gems / shards / cosmetics / chest-keys: NEVER weekend-scaled
            }

            if (mult <= 1f) return baseAmount; // 1× (or clamped) → unchanged

            double scaled = (double)baseAmount * mult;
            if (scaled >= int.MaxValue) return int.MaxValue;       // overflow guard
            return (int)Math.Round(scaled, MidpointRounding.AwayFromZero);
        }

        /// <summary>Clamp a raw multiplier into [Min, Max]; a NaN / sub-1 value falls back to 1× (no penalty).</summary>
        private static float Clamp(float m)
        {
            if (float.IsNaN(m) || m < MinMultiplier) return MinMultiplier;
            return m > MaxMultiplier ? MaxMultiplier : m;
        }
    }
}
