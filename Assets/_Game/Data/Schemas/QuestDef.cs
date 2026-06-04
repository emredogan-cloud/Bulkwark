// BULWARK — QuestDef ScriptableObject (Data). Phase 4 §13 4.5, §8/§10 retention.
// Daily/weekly quests with gameplay-safe rewards (currency/PassXP/shards — never power).
// No energy gates (decision-log §1 CUT); quests reward play, they don't gate it.
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "QuestDef", menuName = "BULWARK/Data/QuestDef", order = 12)]
    public class QuestDef : ScriptableObject
    {
        public string id;
        public string displayName;
        public QuestCadence cadence = QuestCadence.Daily;
        public string objectiveKey;   // e.g. "win_battles" / "deploy_units" / "cast_spells" (telemetry-driven)
        public int targetCount = 3;
        public RewardGrant reward;     // gameplay-safe (MonetizationSafety.IsGameplaySafe)
    }
}
