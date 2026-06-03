// BULWARK — EconomyConfig ScriptableObject (Data). Phase 3 §13 3.1/3.3/3.5, §9.
// The server-owned, RC-tunable economy defaults: diminishing-returns reward rule, mode reward
// rates, and the Endless adaptive-director curve. Values are the SO/literal tier of the 3-tier
// resolver (RC → SO → literal, §12); RemoteConfig overrides them live (server-owned, §9/§12).
// NO monetization values here (ads/IAP/pass = Phase 4). Provisional, LSD-owned (§16).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-2-001).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "BULWARK/Data/EconomyConfig", order = 7)]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Diminishing-returns first-clear gem rule (§9): max(floor, base − dec×playCount)")]
        public int gemDiminishDecrement = 5; // 'dec'
        public int gemDiminishFloor = 5;     // 'floor' = max(5, …)

        [Header("Mode reward rates (server-granted) — PROVISIONAL")]
        public int silverPerEndlessWave = 8;
        public int passXpPerBattle = 10;
        public int silverPerLadderWin = 40;
        public int gemsPerLadderSeasonReward = 50;

        [Header("Endless adaptive director (§5.1/§7) — PROVISIONAL")]
        public float endlessBaseWaveStrength = 100f;
        public float endlessGrowthPerWave = 0.12f;   // strength multiplier growth/wave
        public float endlessDirectorBand = 0.15f;    // adaptivity band around player performance

        [Header("Async ladder stat-sanity bounds (§3.4 / decision-log §3) — PROVISIONAL")]
        public float ladderMaxPlausibleDps = 1000f;  // server rejects snapshots above plausibility
        public int   ladderMaxUnitsPerBattle = 60;
    }
}
