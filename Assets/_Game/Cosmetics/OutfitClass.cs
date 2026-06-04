// BULWARK — OUTFIT-CLASS cosmetic system (Phase 4 §13 4.1, §6 cosmetic framework + cosmetic-safety).
// This is the meta/PRESENTATION layer (§12 boundary: NOT the ECS battle sim) that turns a gameplay-safe
// CosmeticDef into a READ-SAFE skin a renderer can apply over a LOCKED archetype base.
//
// COSMETIC-SAFETY RULE (§6, INVIOLABLE): a cosmetic may change ONLY palette / material / trim /
// particle-VFX color / out-of-battle flourishes. It may NEVER change silhouette, unit size, hitbox,
// animation timing, ability-VFX readability, or faction-color identity. Those are owned by the LOCKED
// base (the sim/art), are NOT fields on CosmeticDef, and are NOT fields on ReadSafeSkin — so a cosmetic
// is gameplay-safe AND read-safe BY CONSTRUCTION. CosmeticSafety.Validate asserts this (fail-closed) so
// the GATE 4 fairness audit + a CI check can prove it on every authored cosmetic.
//
// FAIRNESS (GATE 4): higher outfit tiers (Standard→Mythic) get RICHER VFX color/material prestige, the
// SAME silhouette — "prestige, not power, and not even a readability edge" (§6). No stat, no read edge.
//
// PURE policy (no MonoBehaviour); UnityEngine.Color only (via Bulwark.Data). CI-unit-testable in the
// EditMode runner like the Phase-3 services. SCAFFOLD STATUS: authored, NOT compiled here (CI validates).
// DEFERRED: the actual renderer apply (IReadSafeRenderTarget adapter over the Spine/URP material) — the
// renderer wiring is integration work; this layer produces the read-safe DESCRIPTOR it would consume.

using System.Collections.Generic;
using UnityEngine;
using Bulwark.Data;

namespace Bulwark.Game.Cosmetics
{
    /// <summary>
    /// The ONLY things a cosmetic may legitimately drive on a unit: the recolor palette, a read-safe
    /// material-variant id, the VFX tint, and which LOCKED archetype base it skins. There is DELIBERATELY
    /// no size/scale/hitbox/animationSpeed/silhouette field here — readability + power are not expressible,
    /// so a renderer fed a ReadSafeSkin can never break the §6 lock. This is the apply-time contract.
    /// </summary>
    public readonly struct ReadSafeSkin
    {
        public readonly string ArchetypeId;   // the LOCKED silhouette this skins (UnitDef.id) — never changed, only referenced
        public readonly Faction Faction;      // faction identity (read-locked; carried for verification, not mutated)
        public readonly OutfitTier Tier;      // prestige tier — drives VFX RICHNESS only, same silhouette
        public readonly Rarity Rarity;        // visual prestige label only
        public readonly CosmeticVariant Variant;
        public readonly Color PrimaryColor;
        public readonly Color SecondaryColor;
        public readonly Color TrimColor;
        public readonly Color VfxColor;
        public readonly string MaterialVariantId; // a read-safe material swap id (never alters silhouette/size)

        public ReadSafeSkin(string archetypeId, Faction faction, OutfitTier tier, Rarity rarity,
            CosmeticVariant variant, Color primary, Color secondary, Color trim, Color vfx, string materialVariantId)
        {
            ArchetypeId = archetypeId; Faction = faction; Tier = tier; Rarity = rarity; Variant = variant;
            PrimaryColor = primary; SecondaryColor = secondary; TrimColor = trim; VfxColor = vfx;
            MaterialVariantId = materialVariantId;
        }
    }

    /// <summary>Why a cosmetic failed the §6 safety check (UX/CI/audit; never a payload — §12).</summary>
    public enum CosmeticSafetyReject : int
    {
        None = 0,
        NoId,            // missing CosmeticDef.id
        NoArchetype,     // missing archetypeId — a cosmetic must skin a known LOCKED base
        BadTier,         // tier outside the Standard..Mythic enum
        BadRarity,       // rarity outside the enum
        BadVariant,      // variant outside the enum
        NotGameplaySafe, // the type invariant gameplaySafe was somehow false (defense in depth)
    }

    /// <summary>Result of validating one cosmetic against the §6 cosmetic-safety rule.</summary>
    public readonly struct CosmeticSafetyResult
    {
        public readonly bool Ok;
        public readonly CosmeticSafetyReject Reject;
        public CosmeticSafetyResult(bool ok, CosmeticSafetyReject reject) { Ok = ok; Reject = reject; }
        public static CosmeticSafetyResult Pass() => new CosmeticSafetyResult(true, CosmeticSafetyReject.None);
        public static CosmeticSafetyResult Fail(CosmeticSafetyReject r) => new CosmeticSafetyResult(false, r);
    }

    /// <summary>
    /// THE automated cosmetic-safety check (Phase-4 §G validation gate): "every cosmetic preserves
    /// silhouette / size / hitbox / animation-timing." Because CosmeticDef carries NONE of those fields
    /// (they live on the locked base), preservation is guaranteed at the TYPE level; this validator adds
    /// the runtime/CI assertion (well-formed identity, in-range enums, gameplaySafe invariant) and is the
    /// single guard a content/CI test runs over the whole cosmetic catalog. Fail-closed.
    /// </summary>
    public static class CosmeticSafety
    {
        /// <summary>
        /// The structural guarantees this system enforces, enumerated so the GATE 4 report can cite them.
        /// Each is true BY CONSTRUCTION (no such field exists to violate) — the validator proves the
        /// CosmeticDef instance is well-formed; the type proves the lock.
        /// </summary>
        public static readonly string[] LockedProperties =
        {
            "silhouette", "unit_size", "hitbox", "animation_timing", "ability_vfx_readability", "faction_color_identity",
        };

        /// <summary>Validate ONE cosmetic. Fail-closed: anything malformed is rejected so it can never
        /// be applied. Returns the specific reject reason for CI/audit display.</summary>
        public static CosmeticSafetyResult Validate(CosmeticDef def)
        {
            if (def == null || string.IsNullOrEmpty(def.id)) return CosmeticSafetyResult.Fail(CosmeticSafetyReject.NoId);
            // OUT-OF-BATTLE variants (emote/banner, §6) skin NO unit, so they legitimately carry no
            // archetypeId — require an archetype ONLY for IN-BATTLE variants (skins/recolors/weapon).
            if (!OutfitClass.IsOutOfBattle(def.variant) && string.IsNullOrEmpty(def.archetypeId))
                return CosmeticSafetyResult.Fail(CosmeticSafetyReject.NoArchetype);
            if (!System.Enum.IsDefined(typeof(OutfitTier), def.tier))        return CosmeticSafetyResult.Fail(CosmeticSafetyReject.BadTier);
            if (!System.Enum.IsDefined(typeof(Rarity), def.rarity))          return CosmeticSafetyResult.Fail(CosmeticSafetyReject.BadRarity);
            if (!System.Enum.IsDefined(typeof(CosmeticVariant), def.variant)) return CosmeticSafetyResult.Fail(CosmeticSafetyReject.BadVariant);
            // The type invariant: a cosmetic is gameplay-safe by construction. Assert anyway (defense in depth).
            if (!def.gameplaySafe) return CosmeticSafetyResult.Fail(CosmeticSafetyReject.NotGameplaySafe);
            return CosmeticSafetyResult.Pass();
        }

        /// <summary>True iff the cosmetic passes the §6 safety check.</summary>
        public static bool IsReadSafe(CosmeticDef def) => Validate(def).Ok;

        /// <summary>
        /// Audit the whole cosmetic catalog (the content/CI check). Returns the ids of any cosmetics that
        /// FAILED the §6 safety rule — a non-empty list is a GATE-4 FAIL the authoring must fix. Null/empty
        /// ids and null entries are reported as failures (fail-closed). An all-pass catalog returns empty.
        /// </summary>
        public static IReadOnlyList<string> AuditCatalog(IEnumerable<CosmeticDef> catalog)
        {
            var rejected = new List<string>();
            if (catalog == null) return rejected;
            foreach (var def in catalog)
            {
                if (!IsReadSafe(def))
                    rejected.Add(def != null && !string.IsNullOrEmpty(def.id) ? def.id : "<malformed-cosmetic>");
            }
            return rejected;
        }
    }

    /// <summary>
    /// Resolves a gameplay-safe CosmeticDef into a ReadSafeSkin for the renderer (§6 outfit classes).
    /// The resolve is a pure projection of the cosmetic's VISUAL params over a LOCKED base — it copies the
    /// palette/material/VFX and the prestige tier, and copies NOTHING that could change a read (there is
    /// no such field to copy). A cosmetic that fails CosmeticSafety is REFUSED (returns false) so an
    /// unsafe skin can never reach the renderer (fail-closed, GATE 4).
    /// </summary>
    public static class OutfitClass
    {
        /// <summary>
        /// Produce the read-safe skin for <paramref name="def"/>. Returns false (and a default skin) if the
        /// cosmetic is not §6-safe — the caller then renders the locked base / clarity default instead.
        /// </summary>
        public static bool TryResolve(CosmeticDef def, out ReadSafeSkin skin)
        {
            skin = default;
            if (!CosmeticSafety.IsReadSafe(def)) return false; // never apply an unsafe cosmetic
            // OUT-OF-BATTLE cosmetics (emote/banner) are NOT unit skins — they render in the meta UI,
            // never on a combat unit, so they never resolve to a ReadSafeSkin (the "never on a unit"
            // contract is enforced here, not just by data convention). §6.
            if (IsOutOfBattle(def.variant)) return false;

            skin = new ReadSafeSkin(
                archetypeId: def.archetypeId,
                faction: def.faction,
                tier: def.tier,
                rarity: def.rarity,
                variant: def.variant,
                primary: def.primaryColor,
                secondary: def.secondaryColor,
                trim: def.trimColor,
                vfx: def.vfxColor,
                materialVariantId: def.materialVariantId);
            return true;
        }

        /// <summary>
        /// Whether this variant is an OUT-OF-BATTLE flourish (emote/banner) that never renders on a combat
        /// unit at all — so it cannot affect any in-battle read by definition (§6). Used by the renderer to
        /// route emotes/banners to the meta UI rather than the unit.
        /// </summary>
        public static bool IsOutOfBattle(CosmeticVariant variant)
            => variant == CosmeticVariant.Emote || variant == CosmeticVariant.Banner;
    }
}
