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
}
