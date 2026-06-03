// BULWARK — EconomyResolver (Phase 3 §13 3.5, §12). A thin TYPED wrapper over the Phase-0
// 3-tier ConfigResolver (RemoteConfig → typed ScriptableObject → literal). Economy/balance
// values are SERVER-OWNED and RC-tunable: a server/RC edit retunes them live WITHOUT an app
// update (§12; server-side A/B — no client-random). The SO tier is fed from EconomyConfig at
// the UnityEngine boundary (an IScriptableConfigStore adapter); this class stays PURE C# so it
// is CI-unit-testable like ConfigResolver itself. The literal defaults below are the final
// (code) tier and mirror EconomyConfig's provisional defaults (LSD-owned, §16).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity / no RC backend — ADR-0-001/-2-001).
// DEFERRED: a live RC retune (RC store wired to the BaaS) round-trip — no backend here.

using Bulwark.Services.Config;

namespace Bulwark.Services.RemoteConfig
{
    /// <summary>Stable RemoteConfig keys for the economy (server-owned). One place; no magic strings elsewhere.</summary>
    public static class EconomyKeys
    {
        public const string GemDiminishDecrement   = "econ.gem_diminish_decrement";
        public const string GemDiminishFloor       = "econ.gem_diminish_floor";
        public const string SilverPerEndlessWave   = "econ.silver_per_endless_wave";
        public const string PassXpPerBattle        = "econ.passxp_per_battle";
        public const string SilverPerLadderWin     = "econ.silver_per_ladder_win";
        public const string GemsPerLadderSeason    = "econ.gems_per_ladder_season";
        public const string EndlessBaseWaveStrength= "econ.endless_base_wave_strength";
        public const string EndlessGrowthPerWave   = "econ.endless_growth_per_wave";
        public const string EndlessDirectorBand    = "econ.endless_director_band";
        public const string LadderMaxPlausibleDps  = "econ.ladder_max_plausible_dps";
        public const string LadderMaxUnitsPerBattle= "econ.ladder_max_units_per_battle";
        public const string UpgradeCostMultiplier  = "econ.upgrade_cost_multiplier"; // live economy lever
    }

    /// <summary>
    /// Typed economy getters resolving RC → SO(EconomyConfig) → literal. Each getter returns the
    /// resolved value; <see cref="LastSource"/> exposes which tier won (telemetry/QA). Pure C#.
    /// </summary>
    public sealed class EconomyResolver
    {
        private readonly ConfigResolver _resolver;
        public ConfigSource LastSource { get; private set; } = ConfigSource.Literal;

        public EconomyResolver(ConfigResolver resolver)
        {
            _resolver = resolver;
        }

        private int I(string key, int literal)
        {
            var r = _resolver.ResolveInt(key, literal);
            LastSource = r.Source;
            return r.Value;
        }
        private float F(string key, float literal)
        {
            var r = _resolver.ResolveFloat(key, literal);
            LastSource = r.Source;
            return r.Value;
        }

        // Literal defaults mirror EconomyConfig's provisional defaults (the code/final tier).
        public int   GemDiminishDecrement    => I(EconomyKeys.GemDiminishDecrement, 5);
        public int   GemDiminishFloor        => I(EconomyKeys.GemDiminishFloor, 5);
        public int   SilverPerEndlessWave    => I(EconomyKeys.SilverPerEndlessWave, 8);
        public int   PassXpPerBattle         => I(EconomyKeys.PassXpPerBattle, 10);
        public int   SilverPerLadderWin      => I(EconomyKeys.SilverPerLadderWin, 40);
        public int   GemsPerLadderSeason     => I(EconomyKeys.GemsPerLadderSeason, 50);
        public float EndlessBaseWaveStrength => F(EconomyKeys.EndlessBaseWaveStrength, 100f);
        public float EndlessGrowthPerWave    => F(EconomyKeys.EndlessGrowthPerWave, 0.12f);
        public float EndlessDirectorBand     => F(EconomyKeys.EndlessDirectorBand, 0.15f);
        public float LadderMaxPlausibleDps   => F(EconomyKeys.LadderMaxPlausibleDps, 1000f);
        public int   LadderMaxUnitsPerBattle => I(EconomyKeys.LadderMaxUnitsPerBattle, 60);
        public float UpgradeCostMultiplier   => F(EconomyKeys.UpgradeCostMultiplier, 1f);

        /// <summary>The §9 diminishing first-clear gem reward: max(floor, base − dec×playCount).</summary>
        public int FirstClearGems(int firstClearGemBase, int playCount)
        {
            int dec = GemDiminishDecrement, floor = GemDiminishFloor;
            int v = firstClearGemBase - dec * (playCount < 0 ? 0 : playCount);
            return v < floor ? floor : v;
        }
    }
}
