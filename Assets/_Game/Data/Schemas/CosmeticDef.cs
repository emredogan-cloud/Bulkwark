// BULWARK — CosmeticDef ScriptableObject (Data). Phase 4 §13 4.1, §6 cosmetic-safety (INVIOLABLE).
// A cosmetic changes ONLY palette/material/trim/particle-color/flourishes over a LOCKED base.
// It may NEVER change: silhouette, unit size, hitbox, animation timing, ability-VFX readability,
// or faction-color identity — NONE of those are fields here (the locked base/sim owns them), so a
// cosmetic is gameplay-safe BY CONSTRUCTION. Ranked clarity mode can override to read-safe skins.
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "CosmeticDef", menuName = "BULWARK/Data/CosmeticDef", order = 8)]
    public class CosmeticDef : ScriptableObject
    {
        [Header("Identity (§6)")]
        public string id;                 // e.g. cosmetic.ironpact_shieldman.elite_crimson
        public string displayName;
        public string archetypeId;        // which unit's LOCKED silhouette this skins (UnitDef.id)
        public Faction faction = Faction.IronPact;
        public OutfitTier tier = OutfitTier.Standard;
        public Rarity rarity = Rarity.Common;
        public CosmeticVariant variant = CosmeticVariant.ColorScheme;

        [Header("Visual-only params (recolor/material/VFX color) — NO gameplay fields exist here")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.gray;
        public Color trimColor = Color.yellow;
        public Color vfxColor = Color.cyan;
        public string materialVariantId;  // a read-safe material swap id; never alters silhouette/size

        // INVARIANT (asserted by ClarityMode/OutfitClass at apply time): cosmetics are read-safe.
        // There is intentionally no size/scale/hitbox/animationSpeed field — power/readability are
        // not expressible here. gameplaySafe is always true; the type carries the proof.
        public bool gameplaySafe => true;
    }
}
