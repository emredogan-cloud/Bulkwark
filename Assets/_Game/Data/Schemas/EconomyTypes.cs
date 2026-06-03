// BULWARK — economy/progression enums (Data layer). Phase 3 §13 3.1–3.2, §9.
// The 4 MVP currencies ONLY (§9; Honor/Event tokens are Phase 7 — NOT here). Upgrade
// stats are the cappable per-unit tracks (§3 "capped per-unit upgrade tracks", no P2W).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002/-1-001/-2-001).

namespace Bulwark.Data
{
    /// <summary>The 4 MVP currencies (§9). Gold is in-battle/non-persistent; the rest are server-owned.</summary>
    public enum Currency : int
    {
        Gold   = 0, // in-battle only, NON-persistent (the ECS GoldStore); never in the server wallet
        Silver = 1, // meta soft currency (server-owned)
        Gems   = 2, // premium currency (server-owned); earned + (Phase 4) purchasable; NEVER buys power (§9)
        PassXP = 3, // battle-pass track xp (server-owned)
    }

    /// <summary>Per-unit upgrade tracks — each is CAPPED (§3, no P2W). Stats mirror UnitDef combat fields.</summary>
    public enum UpgradeStat : int
    {
        Health      = 0,
        Damage      = 1,
        MoveSpeed   = 2,
        AttackSpeed = 3, // lowers AttackInterval
    }
}
