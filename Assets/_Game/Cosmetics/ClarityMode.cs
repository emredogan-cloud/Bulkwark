// BULWARK — RANKED CLARITY MODE (Phase 4 §13 4.1, §6 cosmetic-safety "Fairness limits").
// §6 (INVIOLABLE): "Ranked may enforce a 'clarity mode' (opponents render in standardized read-safe
// skins) so no cosmetic can ever obscure a competitive read." This is the presentation-layer policy that
// guarantees it: in RANKED, an OPPONENT unit is rendered with the STANDARDIZED faction-base skin (the
// read-locked palette), regardless of which outfit class the opponent owns. You still see YOUR OWN
// cosmetics; only the opponents are standardized — so prestige stays visible to its owner while a
// competitive read can never be muddied by an opponent's flashy skin.
//
// Belt-and-suspenders with §6 by construction: cosmetics already cannot change silhouette/size/hitbox/
// timing (CosmeticDef has no such fields; CosmeticSafety enforces it). Clarity mode additionally
// neutralizes the one remaining read vector — PALETTE/VFX confusion — for opponents in ranked.
//
// PURE policy (no MonoBehaviour); UnityEngine.Color only (via Bulwark.Data). CI-unit-testable.
// SCAFFOLD STATUS: authored, NOT compiled here (CI validates). The per-faction standardized palette is
// PROVISIONAL/LSD-owned (§15.6), data/RC-overridable — NOT asserted canon. DEFERRED: the renderer apply.

using UnityEngine;
using Bulwark.Data;

namespace Bulwark.Game.Cosmetics
{
    /// <summary>Who/where a unit is being rendered, so clarity mode can decide whether to standardize it.</summary>
    public readonly struct ClarityContext
    {
        public readonly bool IsRanked;    // ranked match → clarity mode is eligible to apply (§6)
        public readonly bool IsOpponent;  // true for an OPPONENT unit; false for the local player's own unit

        public ClarityContext(bool isRanked, bool isOpponent) { IsRanked = isRanked; IsOpponent = isOpponent; }

        public static ClarityContext OwnUnit(bool isRanked) => new ClarityContext(isRanked, false);
        public static ClarityContext OpponentUnit(bool isRanked) => new ClarityContext(isRanked, true);
    }

    /// <summary>
    /// The standardized, read-locked base skin for a faction's archetype — what an OPPONENT renders as in
    /// ranked clarity mode. It is a ReadSafeSkin (same powerless contract) carrying ONLY the faction base
    /// palette; no per-player cosmetic, no prestige VFX. PROVISIONAL palette (LSD-owned, §15.6).
    /// </summary>
    public static class FactionBasePalette
    {
        // PROVISIONAL clarity-base colors (LSD-owned, RC/SO-overridable). Chosen for high readability +
        // color-blind-safe faction separation (§11 readability) — NOT asserted canon. Iron Pact reads
        // cool steel-blue; Ashen Horde reads warm ash-ember. These are deliberately FLAT (low VFX) so an
        // opponent is maximally legible.
        public static Color Primary(Faction f) => f == Faction.AshenHorde
            ? new Color(0.62f, 0.24f, 0.20f, 1f)   // ash-ember
            : new Color(0.30f, 0.40f, 0.62f, 1f);  // steel-blue

        public static Color Secondary(Faction f) => f == Faction.AshenHorde
            ? new Color(0.35f, 0.16f, 0.14f, 1f)
            : new Color(0.20f, 0.26f, 0.40f, 1f);

        public static Color Trim(Faction f) => new Color(0.85f, 0.85f, 0.85f, 1f); // neutral high-contrast trim
        public static Color Vfx(Faction f) => Primary(f);                          // VFX == primary (no extra read noise)

        /// <summary>The standardized read-safe skin for an archetype+faction (the clarity default).</summary>
        public static ReadSafeSkin StandardSkin(string archetypeId, Faction faction)
            => new ReadSafeSkin(
                archetypeId: archetypeId,
                faction: faction,
                tier: OutfitTier.Standard,          // always the base tier (no prestige VFX for opponents)
                rarity: Rarity.Common,
                variant: CosmeticVariant.ColorScheme,
                primary: Primary(faction),
                secondary: Secondary(faction),
                trim: Trim(faction),
                vfx: Vfx(faction),
                materialVariantId: null);           // base material — never a custom silhouette/material
    }

    /// <summary>
    /// The clarity-mode policy (§6). Decides, per unit, whether to render the owner's chosen cosmetic or
    /// the standardized faction base. Rule: in RANKED, OPPONENT units are standardized; everything else
    /// (non-ranked, or your own unit) renders the chosen cosmetic (falling back to the base if the
    /// cosmetic is missing or fails the §6 safety check — fail-closed).
    /// </summary>
    public sealed class ClarityMode
    {
        /// <summary>Whether clarity mode standardizes opponents in ranked. Canon default = true (§6).
        /// Exposed so a non-ranked mode can disable it; ranked MUST keep it on (inviolable readability).</summary>
        public bool StandardizeOpponentsInRanked { get; }

        public ClarityMode(bool standardizeOpponentsInRanked = true)
        {
            StandardizeOpponentsInRanked = standardizeOpponentsInRanked;
        }

        /// <summary>True iff this unit must be standardized to the read-safe base for the given context.</summary>
        public bool ShouldStandardize(in ClarityContext ctx)
            => StandardizeOpponentsInRanked && ctx.IsRanked && ctx.IsOpponent;

        /// <summary>
        /// Resolve the skin to ACTUALLY render for a unit. <paramref name="archetypeId"/> + <paramref name="faction"/>
        /// identify the LOCKED base (always available, even with no cosmetic). <paramref name="ownedCosmetic"/>
        /// is the owner's chosen CosmeticDef (may be null). Returns:
        ///  • the standardized faction base when ShouldStandardize (ranked opponent) — opponent prestige is hidden;
        ///  • else the owner's read-safe cosmetic when present + §6-safe;
        ///  • else the standardized faction base (fail-closed default — never an unsafe/empty render).
        /// </summary>
        public ReadSafeSkin ResolveSkin(string archetypeId, Faction faction, CosmeticDef ownedCosmetic, in ClarityContext ctx)
        {
            if (ShouldStandardize(in ctx))
                return FactionBasePalette.StandardSkin(archetypeId, faction); // §6: opponents read clean in ranked

            if (ownedCosmetic != null && OutfitClass.TryResolve(ownedCosmetic, out ReadSafeSkin skin))
                return skin; // your own (or non-ranked) cosmetic — prestige shown, still read-safe by construction

            return FactionBasePalette.StandardSkin(archetypeId, faction); // fail-closed base default
        }
    }
}
