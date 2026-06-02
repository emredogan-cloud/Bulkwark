// BULWARK — combat type enums (Data layer). Roadmap §4 "type×armor counter matrix (5×4)".
// Canon NAMES are used where the roadmap states them (§5.2: Pierce/Blunt/Fire/Poison;
// Light/Shielded/Heavy). The roadmap declares the COUNTS (5 damage types, 4 armor classes)
// but does not name every member; unnamed slots are flagged PROVISIONAL and are LSD/ADR-
// owned (§15.6: do not fabricate canon — these are explicit placeholders, not asserted canon).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

namespace Bulwark.Data
{
    /// <summary>5 combat damage types (excluding Unset) per §4. Index into the 5×4 counter matrix.</summary>
    public enum DamageType : int
    {
        Unset  = 0,
        Melee  = 1, // PROVISIONAL name for the basic melee/slash type (5th type unnamed in canon; ADR to finalize)
        Pierce = 2, // §5.2 (Crossbow / Iron Pact Ranged)
        Blunt  = 3, // §5.2 (Slinger) — schema-declared for later phases; NO Phase-1 unit uses it
        Fire   = 4, // §5.2 (Battlemage) — schema only
        Poison = 5, // §5.2 (Hexcaster) — schema only
    }

    /// <summary>4 armor classes (excluding Unset) per §4.</summary>
    public enum ArmorClass : int
    {
        Unset     = 0,
        Light     = 1, // §5.2
        Shielded  = 2, // §5.2
        Heavy     = 3, // §5.2
        Unarmored = 4, // PROVISIONAL 4th class (canon states 4; name ADR-owned) — unused by Phase-1 content
    }

    /// <summary>7-role shared archetype palette (§5.2). A faction fields 6 of these.</summary>
    public enum UnitRole : int
    {
        Unset      = 0,
        Miner      = 1,
        Frontline  = 2,
        Skirmisher = 3,
        Ranged     = 4,
        Caster     = 5,
        Heavy      = 6,
        Flanker    = 7,
    }

    // ---------------------------------------------------------------------------
    // Phase 2 additions (§5.1 factions, §5.3 spells). Names are canon-grounded
    // (§5.1 IronPact/AshenHorde; §5.3 spell categories + status effects implied by
    // the named spells); shape labels are implementation. Provisional, LSD-owned.
    // ---------------------------------------------------------------------------

    /// <summary>The 2 MVP factions (§5.1). Full vision adds Arcane/Mechanized (Phase 7) — NOT here.</summary>
    public enum Faction : int
    {
        IronPact   = 0, // disciplined legion: formations, shields, attrition (§5.1)
        AshenHorde  = 1, // swarm aggro: speed, flank, expendable (§5.1)
    }

    /// <summary>
    /// Status effects applied by spells (§5.3) and read by the combat/spell sim. Each is
    /// grounded in a §5.3-named spell (Freeze→Chilled, LightningStorm→Burning, Hexcaster/
    /// poison→Poisoned, Stun→Stunned, GoldRush→GoldBoost, Rage→Raged, Haste→Hasted, Summon).
    /// </summary>
    public enum StatusKind : int
    {
        None = 0, Chilled = 1, Burning = 2, Poisoned = 3, Stunned = 4,
        Hasted = 5, Raged = 6, GoldBoost = 7,
    }

    /// <summary>Spell categories — verbatim from §5.3.</summary>
    public enum SpellCategory : int
    {
        Offensive = 0, Control = 1, Economy = 2, Summon = 3, Buff = 4,
    }

    /// <summary>Spell target shape (implementation; readability via telegraph, §5.3).</summary>
    public enum TargetShape : int
    {
        Point = 0, Area = 1, Line = 2, Self = 3, AllyArea = 4,
    }
}
