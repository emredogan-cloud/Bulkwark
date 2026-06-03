// BULWARK — Endless adaptive director (Phase 3 §13 3.3, §5.1 GetWaveRating, §7 Endless).
// WHAT THIS IS
//   The pure-C# wave-strength curve + adaptive director for Endless (survival). It MODERNIZES the
//   original EndlessDeads GetWaveRating(day) scaler (§3/§5.1): a base geometric growth curve, ADAPTED
//   within a band around the player's RECENT performance so the run self-tunes toward a target challenge.
//
// WHY PURE C# (no UnityEngine)
//   §12 says pure-C# services should avoid UnityEngine so they are CI-unit-testable like the Phase-0
//   ConfigResolver. This holds NO Unity references and NO ECS — it is just the math. EndlessController
//   (the MonoBehaviour) owns the Unity/ECS side and calls into this.
//
// CANON THIS OBEYS
//   • §5.1/§7 curve: baseStrength = endlessBaseWaveStrength × (1 + endlessGrowthPerWave)^wave.
//   • Adaptation band (§5.1 modern GetWaveRating): the actual emitted strength is the base curve
//     nudged toward the player's recent rating, but CLAMPED to ±endlessDirectorBand of the base.
//     The director can never run away from the authored curve — the band is a hard fence (anti-spike,
//     keeps difficulty legible per §3 readability).
//   • NO balance invented: every coefficient comes from the EconomyConfig DATA (passed in), never literals
//     (the only constants here are neutral math identities like 1.0 / clamp bounds).
//
// SCAFFOLD STATUS: authored, NOT compiled (no toolchain — ADR-0-001/-2-001), but UNIT-TESTABLE (pure C#).
// DEFERRED: difficulty-tuning verdict (do the band/curve values feel right) — needs runtime playtest.

namespace Bulwark.Game.Modes.Endless
{
    /// <summary>
    /// Pure-C# Endless wave scaler + adaptive director. Construct with the authored curve params
    /// (from EconomyConfig), then ask for each wave's strength; feed back the player's per-wave
    /// performance so the next waves adapt within the band. No UnityEngine / no ECS dependency.
    /// </summary>
    public sealed class AdaptiveDirector
    {
        private readonly float _baseWaveStrength;   // EconomyConfig.endlessBaseWaveStrength
        private readonly float _growthPerWave;       // EconomyConfig.endlessGrowthPerWave (e.g. 0.12)
        private readonly float _band;                // EconomyConfig.endlessDirectorBand (e.g. 0.15)
        private readonly int _ratingWindow;          // how many recent waves feed the rating (smoothing)

        // Rolling player-performance rating in [0,1]: 0 = struggling (ease off), 1 = dominating (ramp up).
        // It is a smoothed average of the recent per-wave ratings the controller reports.
        private float _rating;
        private int _samples;

        /// <param name="baseWaveStrength">EconomyConfig.endlessBaseWaveStrength (curve scale).</param>
        /// <param name="growthPerWave">EconomyConfig.endlessGrowthPerWave (geometric growth/wave).</param>
        /// <param name="directorBand">EconomyConfig.endlessDirectorBand (max ± deviation from the base curve).</param>
        /// <param name="ratingWindow">Smoothing window for the recent-performance rating (≥1).</param>
        public AdaptiveDirector(float baseWaveStrength, float growthPerWave, float directorBand, int ratingWindow = 3)
        {
            _baseWaveStrength = baseWaveStrength;
            _growthPerWave = growthPerWave;
            _band = directorBand < 0f ? 0f : directorBand; // a negative band would invert the fence — clamp to 0
            _ratingWindow = ratingWindow < 1 ? 1 : ratingWindow;
            _rating = 0.5f;   // start neutral (target challenge) until we have real data
            _samples = 0;
        }

        /// <summary>Current smoothed performance rating in [0,1] (0.5 = on target). Exposed for telemetry/tests.</summary>
        public float Rating => _rating;

        /// <summary>
        /// The UN-adapted base strength of a wave: baseWaveStrength × (1 + growthPerWave)^wave.
        /// <paramref name="wave"/> is 0-based (wave 0 = baseWaveStrength). Negative inputs clamp to 0.
        /// </summary>
        public float BaseWaveStrength(int wave)
        {
            int w = wave < 0 ? 0 : wave;
            float g = 1f + _growthPerWave;
            return _baseWaveStrength * Pow(g, w);
        }

        /// <summary>
        /// The ADAPTED strength to actually emit for <paramref name="wave"/>: the base curve scaled by a
        /// director factor derived from the player's recent rating, then CLAMPED to [1−band, 1+band] of the
        /// base. Rating &gt; 0.5 (player dominating) pushes toward 1+band; &lt; 0.5 (struggling) toward 1−band.
        /// The clamp is the §5.1 fence: the director cannot escape ±band of the authored curve.
        /// </summary>
        public float WaveStrength(int wave)
        {
            float baseStrength = BaseWaveStrength(wave);

            // Map rating [0,1] → director factor [1−band, 1+band]. rating 0.5 ⇒ factor 1.0 (no nudge).
            float factor = 1f + (_rating - 0.5f) * 2f * _band;
            factor = Clamp(factor, 1f - _band, 1f + _band); // hard fence (legibility / anti-spike, §3)
            return baseStrength * factor;
        }

        /// <summary>
        /// Convenience: the §5.1 "GetWaveRating"-style scaler as a MULTIPLE of the base strength
        /// (1.0 = on the authored curve). Equivalent to WaveStrength(wave) / BaseWaveStrength(wave),
        /// guarded against a zero base. Useful for telemetry/tests and for callers that scale ECS spawns.
        /// </summary>
        public float GetWaveRating(int wave)
        {
            float b = BaseWaveStrength(wave);
            if (b <= 0f) return 1f;
            return WaveStrength(wave) / b;
        }

        /// <summary>
        /// Feed the player's performance on the wave they just survived/failed. <paramref name="waveRating"/>
        /// is the controller's normalized read of how comfortably the player handled the wave, in [0,1]
        /// (0 = nearly wiped, 1 = trivial). It is smoothed into the rolling rating that drives WaveStrength.
        /// Values are clamped to [0,1] so a bad controller read can never break the band fence.
        /// </summary>
        public void ReportWavePerformance(float waveRating)
        {
            float r = Clamp(waveRating, 0f, 1f);
            if (_samples == 0)
            {
                _rating = r;
                _samples = 1;
                return;
            }
            // Exponential smoothing with weight 1/window — recent waves dominate but no single spike whips it.
            float alpha = 1f / _ratingWindow;
            _rating = _rating * (1f - alpha) + r * alpha;
            if (_samples < _ratingWindow) _samples++;
        }

        // -------- tiny math helpers (kept local so this stays a zero-dependency, CI-testable file) --------

        private static float Clamp(float v, float lo, float hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

        // Integer power (waves are non-negative integers): avoids System.Math import for a single call,
        // keeps the file dependency-free. O(exp) — exp is a wave index, trivially small.
        private static float Pow(float baseValue, int exp)
        {
            float result = 1f;
            for (int i = 0; i < exp; i++) result *= baseValue;
            return result;
        }
    }
}
