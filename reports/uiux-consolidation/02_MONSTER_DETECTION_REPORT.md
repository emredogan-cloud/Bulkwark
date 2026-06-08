# 02 — MONSTER / NON‑STICK CREATURE DETECTION
**Goal:** identify every `/design` screen whose **character layer** violates the stick‑figure visual identity, and specify the stick archetype that should replace each.
**Method:** forensic inspection of all 38 mockups (four independent reviewers, viewing each PNG). Classification: `STICK` (on‑identity) vs `NON‑STICK` (realistic human/knight, orc/ogre, dragon, undead/zombie, beast — violation). **No files modified.**

---

## 1. Headline

The intended identity is **stick‑figure warriors** (Iron Pact = blue‑accented sticks, Ashen Horde = red‑accented sticks) in a dark siege war. **The mockups almost entirely betray this:** of 38 screens, **only ~3 use stick figures** (MainMenu hero trio, ModeSelect cards, Skins hero/thumbnails; Spells is a borderline over‑rendered stick mage). Wherever else a character/avatar/unit appears, it is rendered as a **realistic armored human, an orc/ogre, a dragon, undead, or a lion**. This is a **project‑wide art‑identity failure**, not a few stray images.

## 2. The stick archetype vocabulary (replacement targets)

Derived from the game's own roster (`UnitsArmyDesign`) + the on‑identity screens:

| Archetype | Stick description | Faction tint |
|---|---|---|
| **Stick King / Commander** | black stick body, gold crown, short cape, sword + kite shield | blue (Iron Pact) / red (Ashen Horde) |
| **Stick Mage / Caster** | stick body, tall pointed robe + hat silhouette, glowing eyes, staff | purple/arcane accent |
| **Stick Archer / Ranger** | stick body, hood/cloak, longbow | green accent |
| **Stick Swordsman / Shieldman** | stick body, sword + round/kite shield, light helm accent | faction tint |
| **Stick Spearman / Spartan** | stick body, spear + round shield, crested helm | faction tint |
| **Stick Miner / Worker** | stick body, pickaxe, satchel | neutral |
| **Stick Crossbowman** | stick body, crossbow | faction tint |
| **Stick Heavy Guard / Brute** | larger/bulkier stick body, heavy‑armor accents, two‑hander (replaces orc/ogre) | red (Ashen) |
| **Stick Undead** | tattered stick body, bone/green necrotic accents (replaces zombie/undead) | green/red |

**Non‑creature emblems** (dragon crest, lion hub) → replace with **stick‑style heraldry** (crowned‑stick crest, crossed stick‑weapons, faction shield) rather than animal beasts.

## 3. Per‑screen violation matrix

Severity: **S3** entire character layer off‑identity · **S2** prominent creature(s) · **S1** minor/background or gear‑only · **OK** on‑identity · **—** no characters.

| Screen | Severity | Non‑stick creatures (location) | Why it violates | Stick replacement | Conf. |
|---|---|---|---|---|---|
| **UnitsArmyDesign** | **S3** | Full roster of 10 realistic units + large "Shieldman" inspector (grid + right panel) | The actual army — every unit — is realistic knights/mages/humans | Map 1:1 to the stick archetype table (stick Shieldman, Archer, Mage, Miner, Crossbowman, Heavy Guard…) | High |
| **InMatchBannerDesign** | **S3** | Realistic armies + **ogre/monster + undead horde** (right wave) | Worst HUD: live army *and* explicit monsters ("The Dead Awaken") | Blue stick army vs red **stick‑undead** wave | High |
| **BattleHudDesign** | **S3** | Realistic blue+red armies on field + 5 realistic unit icons | The on‑field gameplay army is realistic, not sticks | Stick armies (blue vs red) + stick unit icons | High |
| **InMatchSpellHudDesign** | **S3** | Realistic armies + 5 spawn icons + hero portrait | Same as BattleHud + realistic hero bust | Stick armies + stick hero portrait | High |
| **SplashScreenDesign** | **S3** | Realistic armored king (foreground), **dragons** (sky), realistic soldier horde | Title key art is fully realistic + dragons | Stick king hero foreground; remove/replace dragons with smoke/banners | High |
| **LoadingScreenDesign** | **S3** | 2 realistic warriors, **2 dragons**, 2 realistic armies | Fully realistic clash + dragons | Blue vs red stick armies clashing; drop dragons | High |
| **LoginAuthDesign** | **S3** | 2 realistic knights, **dragon** (sky), siege army | Fully realistic flanking figures | Stick warriors flanking; drop dragon | High |
| **MatchIntroDesign** | **S3** | Realistic knight (Iron Pact) + **orc/warlord brute** (Ashen Horde) | VS champions are realistic + an orc | Blue stick king vs red **stick heavy‑brute** | High |
| **CommanderSelectDesign** | **S3** | Realistic human "Warden" + **orc/ogre "Warchief"** | Both commander portraits off‑identity | Stick Commander (blue) vs stick Heavy‑Brute (red) | High |
| **ProfileDesign** | **S3** | Realistic human hero "Thalrion" (large central portrait) | Dominant hero portrait realistic | Stick King hero portrait | High |
| **TournamentLadderDesign** | **S2** | ~16 realistic/undead competitor avatar busts | Whole bracket roster off‑identity | Stick‑figure avatar busts (varied helms/weapons) | High |
| **LeaderboardScreenDesign** | **S2** | Realistic‑human avatar busts (every row) | All player avatars realistic | Stick‑figure avatar busts | High |
| **ClanScreenDesign** | **S2** | **Dragon** clan crest + realistic member/chat avatars | Dragon beast + realistic avatars | Stick‑style clan crest (crowned‑stick) + stick avatars | High |
| **EventsHubDesign** | **S2** | Army silhouettes + **monster skull** (Endless Rush) + knight (Hero Trials) + warriors (Arena Clash) | Multiple card arts off‑identity incl. a monster | Stick army + stick‑undead + stick warriors per card | High |
| **OnlineBattleDesign** | **S2** | 2 realistic armored knight figures flanking VS | Foreground champions realistic | Blue stick vs red stick champions | High |
| **StoreScreenDesign** | **S2** | Battle‑Pass knight/king render + horned crown in bundle | Featured character render realistic | Stick king render; stick‑style crown | Med‑High |
| **LuckySpinDesign** | **S2** | **Lion** emblem (wheel hub) + realistic "Exclusive Avatar" segment | Beast emblem + realistic avatar | Stick‑king/crossed‑weapons crest; stick avatar | High |
| **MainMenuDesign** | **S1** (partial) | **Dragons** (sky) + realistic cavalry (right) — *hero trio is OK* | Stray dragons/cavalry pollute an on‑identity screen | Remove dragons (smoke/banners); stick cavalry or omit | High |
| **ModeSelectDesign** | **S1** (partial) | **Green zombie/undead** on "Endless" card — *other 4 cards OK* | One card breaks the set | Stick‑undead head | High |
| **ChestsScreenDesign** | **S1** | Hooded robed humanoid looming behind chest | Robed humanoid, not stick | Stick mage/king silhouette, or remove | Med |
| **ChestOpenResultDesign** | **S1** | Realistic "Lionhelm" commander helmet (reward card) | Realistic gear art (no body) off‑style | Stick‑style helm icon | Med |
| **FreeRewardsDesign** | **S1** | "Battle Boost" realistic knight thumbnail | Small realistic figure | Stick warrior thumbnail | Med |
| **CampaignMapDesign** | **S1** | Realistic armored avatar token (node 7) + HEROES icon | Map avatar realistic | Stick‑figure map token | Med |
| **PauseModalDesign** | **S1** | Blurred realistic soldier backdrop | Out‑of‑focus realistic combatants | Blurred stick army backdrop | Med |
| **VictoryScreenDesign** | **S1** | Realistic flag‑bearer + background soldiers | Background figures realistic | Stick victors/banner | Med |
| **DefeatScreenDesign** | **S1** | Prominent foreground realistic kneeling knight | Strong realistic figure | Kneeling stick warrior | Med‑High |
| **CampaignResultDesign** | **S1** | Blurred background soldier silhouettes | Faint realistic figures | Blurred stick army | Low‑Med |
| **SkinsScreenDesign** | **OK** | Stick hero + stick skin thumbnails (bg soldiers ambiguous) | On‑identity | — (verify bg soldiers) | High |
| **SpellsScreenDesign** | **OK*** | Borderline over‑rendered stick mage | On‑intent but heavier render than pure stick | Tighten to true stick silhouette | Med |
| **QuestsScreenDesign** | **—** | None (heraldic icons only) | No characters | — | High |
| **DailyRewardDesign** | **—** | None ("Legendary Unit" implied, not shown) | No visible character | — | High |
| **EndlessResultDesign** | **—** | None (dark abstract field) | No legible characters | — | Med |
| **LadderResultDesign** | **—** | None (crests/architecture) | No characters | — | High |
| **ConfirmModalDesign** | **—** | None | UI components only | — | High |
| **RewardGrantDesign** | **—** | None (reward objects) | No characters | — | High |
| **NetworkErrorDesign** | **—** | None (icon only) | No characters | — | High |
| **SettingsScreenDesign** | **—** | Tiny "StickKing" avatar (stick‑consistent) | No violation | — | Med |

**Totals:** S3 = 10 · S2 = 7 · S1 = 9 · OK/clean/no‑char = 12.

## 4. Creature taxonomy (what recurs)

| Creature | Screens | Replace with |
|---|---|---|
| **Realistic armored humans / knights** | Splash, Loading, Login, MatchIntro, Commander, Profile, BattlePass, BattleHud, SpellHud, Banner, Units, Store, Online, Tournament, Leaderboard, Clan, Events, FreeRewards, ChestOpen, CampaignMap, Pause, Victory, Defeat, CampaignResult | Stick warriors (faction‑tinted) per archetype table |
| **Orc / ogre warlord** | MatchIntro, CommanderSelect, InMatchBanner | Red‑accented **stick Heavy‑Brute** (bulkier stick, war‑paint/horned‑helm accent — *not* a green orc) |
| **Dragons** | Splash, Loading, Login, MainMenu, Clan (crest) | Remove (smoke/embers/banners) or stick‑heraldry crest |
| **Undead / zombie** | ModeSelect (Endless), InMatchBanner, Events (Endless Rush), Tournament | **Stick‑undead** (tattered stick + bone/green accents — on‑identity for Endless mode) |
| **Lion (beast emblem)** | LuckySpin | Stick‑king / crossed‑stick‑weapons heraldic crest |

## 5. Confidence & caveats

- **High confidence** where a figure is clearly rendered (foreground knights, orcs, dragons, the Units roster, the Endless zombie, the Clan dragon, the LuckySpin lion).
- **Medium/low** for small, blurred, or background silhouettes (Pause/Victory/CampaignResult backdrops, CampaignMap token) — flagged for art‑director confirmation, not auto‑replacement.
- **IP note (cross‑cutting):** reviewers did **not** find a confirmed 1:1 asset rip of a single game; the style reads as generic AAA dark‑fantasy / AI key art. The closest IP echoes are MainMenu/ModeSelect (stick art + "Stick Empire: Rise" logo, *Stick War* lineage) and the **"statue has fallen"** win/lose concept (Victory/Defeat). This reinforces Report 01 §0: build **original** stick art (Report 03 prompts) rather than reusing these mockups as final assets.
