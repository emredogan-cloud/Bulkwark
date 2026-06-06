// BULWARK — UI STUB DATA (UI Implementation). Presentation-only, TEMPORARY, REMOVABLE.
//
// Fake display data for the UI while GATE-1 (Gameplay Recovery) is active and the real meta-services
// (Wallet / ShopService / QuestService / BattlePassService / ProgressionService / ...) are authored but NOT
// yet wired (the "author-only scaffold"). Per the standing rule: a UI WP that needs an unfinished system is
// "switched to the part by stub (fake data)" — never to gameplay code. These are inert numbers/strings for
// layout + flow only; they carry NO economy/balance/gameplay meaning and are replaced by real server-
// authoritative service binding at GATE-3.
//
// FROZEN currency model (02_UI_FINAL_DECISIONS D3): GOLD (soft) + GEMS (hard) only. No Energy (CUT).

namespace Bulwark.Bootstrap
{
    /// <summary>Temporary presentation-only fake data for UI display (no gameplay effect). Replaced at GATE-3.</summary>
    public static class UiStub
    {
        // --- wallet (display only; frozen model = Gold + Gems) ---
        public static int Gold = 48570;
        public static int Gems = 1726;

        // --- battle pass (display only) ---
        public const string SeasonName = "SEASON OF GLORY";
        public static int PassTier = 23;
        public static int PassXp = 650;
        public static int PassXpPerTier = 1000;
        public static int PassTierCount = 50;
        public static bool PassPremiumOwned = false;
        public const int PassPremiumPriceGems = 999;

        // --- quests (display only) ---
        public struct Quest { public string Title; public int Progress; public int Target; public bool Gem; public int Reward; public bool Claimed; }

        /// <summary>Display-only currently-selected commander name (set by Commander Select).</summary>
        public static string SelectedCommander = WardenName;
        public static readonly Quest[] DailyQuests =
        {
            new Quest{ Title="Win 3 Battles",      Progress=3, Target=3,     Gem=true,  Reward=50 },
            new Quest{ Title="Train 50 Units",     Progress=32, Target=50,   Gem=false, Reward=2000 },
            new Quest{ Title="Open 1 Silver Chest",Progress=1, Target=1,     Gem=true,  Reward=30 },
            new Quest{ Title="Deal 10000 Damage",  Progress=7250, Target=10000, Gem=false, Reward=1500 },
            new Quest{ Title="Log In Daily",       Progress=1, Target=1,     Gem=true,  Reward=20 },
        };

        // --- commanders (display only; mirrors canon CommanderDef x2) ---
        public const string WardenName = "WARDEN";
        public const string WardenTitle = "Lord of Stone and Steel";
        public const string WardenActive = "RALLY";
        public const string WardenActiveDesc = "Inspire your troops, increasing Attack and Defense for a short duration.";
        public const string WardenPassive = "QUARTERMASTER";
        public const string WardenPassiveDesc = "Reduces resource cost of unit upgrades and increases Silver income.";
        public static int WardenLevel = 12;

        public const string WarchiefName = "WARCHIEF";
        public const string WarchiefTitle = "Scourge of the Burning Wastes";
        public const string WarchiefActive = "WAR ROAR";
        public const string WarchiefActiveDesc = "Unleash a war roar, boosting Attack and Speed while intimidating enemies.";
        public const string WarchiefPassive = "BLOOD FORGE";
        public const string WarchiefPassiveDesc = "Increases troop Health and reduces training time for melee units.";
        public static int WarchiefLevel = 9;

        // --- store gem packs (display only; mirrors ShopItemDef gem packs) ---
        public struct GemPack { public string Name; public int Gems; public string Price; public bool Best; }
        public static readonly GemPack[] GemPacks =
        {
            new GemPack{ Name="Handful of Gems",  Gems=80,   Price="$0.99",  Best=false },
            new GemPack{ Name="Sack of Gems",     Gems=500,  Price="$4.99",  Best=false },
            new GemPack{ Name="Chest of Gems",    Gems=1200, Price="$9.99",  Best=true  },
            new GemPack{ Name="Vault of Gems",    Gems=2500, Price="$19.99", Best=false },
            new GemPack{ Name="Mountain of Gems", Gems=6500, Price="$49.99", Best=false },
        };

        /// <summary>Spend gems if affordable (display-only — NOT server-authoritative). Returns success.</summary>
        public static bool TrySpendGems(int amount)
        {
            if (amount < 0 || Gems < amount) return false;
            Gems -= amount;
            return true;
        }

        /// <summary>Grant gems (display-only). Used by stubbed reward flows.</summary>
        public static void GrantGems(int amount) { if (amount > 0) Gems += amount; }
        /// <summary>Grant gold (display-only).</summary>
        public static void GrantGold(int amount) { if (amount > 0) Gold += amount; }
    }
}
