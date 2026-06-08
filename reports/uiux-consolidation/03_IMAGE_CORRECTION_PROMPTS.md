# 03 — IMAGE CORRECTION PROMPTS (GPT‑Image / DALL·E)
**Goal:** for every offending `/design` screen (Report 02), a prompt that **preserves the composition, camera, lighting, FX and colour grade** and **replaces only the creature layer** with **original stick‑figure** art consistent with Stick Empire Rise.

**Two rules baked into every prompt:**
1. **Original art only** — do not reproduce any existing game's characters, unit designs, logos, or trademarks (see Report 01 §0). Stick‑figure *style* is generic; specific copied assets/marks are not.
2. **Keep UI zones clean** — render scene + characters as **background/key art with the UI areas left as uncluttered space and NO baked text/logos/buttons**. The app overlays live UI on top (Reports 04/05); baked UI is exactly what caused the on‑device doubling. Generate **art layers**, not finished screens.

---

## A. SHARED STYLE BLOCK  *(prepend to every prompt below)*

> **STYLE:** Original 2D cinematic dark‑fantasy game art in a **stick‑figure** character idiom. All characters are **black/charcoal silhouette stick‑men** — round featureless heads, thin clean limbs, simple readable poses — with minimal high‑contrast accents only: faction‑tinted cloth/capes, slim gold‑trimmed armor plates, simple iconic weapons (sword, bow, staff, spear, pickaxe, crossbow), and softly **glowing eyes**. **Iron Pact** = cool steel‑blue accents; **Ashen Horde** = red/ember accents. Painterly atmospheric environments and lighting are encouraged; **every living character must be a stick figure.** Mood: epic, grim, heroic. Landscape 16:9, high detail, crisp silhouettes, strong rim‑lighting.

## B. SHARED NEGATIVE BLOCK  *(append to every prompt)*

> **NEGATIVE / DO NOT INCLUDE:** realistic or muscular human anatomy; realistic faces; orcs, ogres, goblins; dragons or winged beasts; lions or animals as characters; zombies rendered as fleshy realistic monsters (use **stick‑undead** instead); 3D renders or photorealism; any existing‑game logo, wordmark, unit, or trademark; baked UI text, buttons, panels, watermarks, or letterforms; clutter in the reserved UI zones.

---

## C. S3 — fully off‑identity (regenerate character layer entirely)

### SplashScreen
*(style block)* Wide cinematic dusk battlefield overlooking a besieged medieval city; left‑to‑right sky gradient cool‑blue → fiery orange/crimson; strong sunset rim‑light. **Replace** the realistic armored king with a **stick KING hero** standing on the left cliff ledge: black stick body, gold crown, short dark‑red cape, sword in right hand, tall **blue banner** in left, heroic low‑angle silhouette against the sunset. **Remove the dragons** — replace with drifting smoke plumes and distant ember sparks. **Replace** the distant soldier horde with tiny **stick‑army silhouettes**. Keep the empty central ornate plaque zone **clear** for the live logo; keep the lower‑centre "tap" zone clear. **No baked text.** *(negative block)*

### LoadingScreen
*(style)* Symmetric wide shot down a corridor toward a burning dark‑spired fortress at centre (vanishing point); split palette cool‑blue left / warm‑red right; fires dotting the field. **Replace** both foreground realistic warriors with faction champions as **stick figures**: left = blue **stick swordsman** with kite shield + blue banner; right = red **stick heavy‑brute** (bulkier stick, horned‑helm accent, two‑hander). **Remove both dragons** (smoke/embers instead). **Replace** both massed armies with **blue vs red stick armies**. Leave the centred title + progress‑bar band as **clean empty space**. *(negative)*

### LoginAuth
*(style)* Centred symmetric onboarding scene, dark battlefield + burning castle softly blurred behind a centre panel zone; split blue‑left/red‑right light; gold ambience. **Replace** the two flanking realistic knights with **stick warriors** (blue stick swordsman left, red stick spearman right), softly depth‑blurred. **Remove the sky dragon.** Keep the entire central panel area **empty/clean** (live login panel overlays). *(negative)*

### MatchIntro
*(style)* Symmetric VS face‑off, low heroic angle, central diagonal energy seam (blue lightning left, orange fire right) as focal axis; high contrast cool/warm rim light. **Replace** the realistic knight (left) with a **blue stick KING/Commander** — crown, blue cape, raised sword, kite shield with simple crest; **replace** the orc warlord (right) with a **red stick HEAVY‑BRUTE** — bulkier stick body, red war‑cloak, horned‑helm accent, heavy flail/maul. Both in dynamic confrontation poses, glowing eyes. Keep top title banner zone, centre "VS" zone, and bottom nameplate zones **clear**. *(negative)*

### CommanderSelect
*(style)* Two symmetric three‑quarter portrait vignettes facing centre, dark stone‑dungeon backdrop, split blue/red + gold light. **Replace** the realistic human "Warden" (left) with a **blue stick Commander** portrait bust: crown/helm, blue cape, gold‑trim shoulder plates, stern glowing‑eye pose. **Replace** the orc "Warchief" (right) with a **red stick Heavy‑Brute** portrait bust: horned‑helm accent, red cloak, jagged pauldrons. Leave ability‑card, name, level, and SELECT zones **clear**. *(negative)*

### Profile
*(style)* Front‑on dashboard; central ornate portrait frame is the focal point; warm key light; dark navy hall backdrop; epic‑purple rarity accents. **Replace** the realistic hero "Thalrion" with a **stick KING hero** portrait inside the frame: black stick body, gold crown, ornate blue‑and‑gold cape, sword at rest, confident glowing‑eye pose. Keep stat tiles, nav rail, gear row, and title zones **clear**. *(negative)*

### BattlePass (tier‑30 capstone)
*(style)* Reward‑grid scene; dark battlefield silhouette backdrop; royal‑purple + gold glow on the capstone reward panel (right). **Replace** the realistic royal‑armored king reward render with a **stick KING in royal regalia**: purple‑and‑gold cape, ornate crown, glowing eyes, triumphant pose, magical purple aura. Keep all reward‑grid/tier/progress zones **clear**. *(negative)*

### BattleHud  *(in‑match key art / unit‑icon source)*
*(style)* High three‑quarter **isometric** battlefield down a horizontal lane; blue keep far‑left, red fortress far‑right; central troop collision lit by small orange fires; cool‑blue left / warm‑red right faction tinting. **Replace** all on‑field troops with **stick armies** — blue **stick swordsmen/archers/spearmen** vs red **stick warriors** — small but crisp silhouettes clashing centre. (Generate a separate clean set of **5 stick unit busts** — swordsman, axeman, archer, cavalry‑stick, crossbowman — for the spawn‑bar icons.) Keep all HUD bar/button zones **clear**. *(negative)*

### InMatchSpellHud
*(style)* Same isometric battlefield, mid‑cast, with a glowing cyan runic ground‑targeting ring at centre as focal point. **Replace** all troops with **stick armies** (blue vs red); generate a clean **stick hero bust** for the bottom‑right portrait ring and **5 stick spawn icons**. Keep HUD/spell‑bar zones **clear**. *(negative)*

### InMatchBanner  *(highest‑severity creature swap)*
*(style)* Same isometric battlefield with an upper event‑banner zone; blue keep defended left, advancing enemy wave right; cyan friendly glows, hot‑red enemy embers; necrotic theme. **Replace** the blue defenders with a **blue stick army**; **replace the realistic army + ogre/monster/undead horde** on the right with a **red STICK‑UNDEAD horde** — tattered stick bodies with **bone‑white/green necrotic accents**, glowing eyes, ragged silhouettes (on‑identity for "The Dead Awaken"). Optionally one larger **stick‑brute** as the wave anchor. Keep the top banner/timer zone and all HUD zones **clear**. *(negative)*

### UnitsArmy  *(heaviest — full roster regeneration)*
*(style)* Dark steel‑blue gloomy castle backdrop; gold‑framed roster grid + right inspector; cold light + warm rim. **Replace the entire roster** with **stick‑figure unit portraits**, one per role, each a clean bust on a neutral vignette: **Shieldman** (sword+kite shield), **Sentinel** (spear+tower shield), **Iron Archer** (bow+hood), **Heavy Guard** (two‑hander, bulky stick), **Runic Adept** (robe+staff, glowing eyes), **Miner** (pickaxe+satchel), **Warden** (sword+cape), **Crossbowman** (crossbow), **Oathbreaker** (red‑accent stick), **Flamecaller** (staff with fire, ember accents). Blue tints for Iron Pact, red for Ashen Horde. Render the large **Shieldman inspector** portrait too. Keep stat/upgrade/tab zones **clear**. *(negative)*

## D. S2 — prominent creature(s) (swap the figures)

### TournamentLadder
*(style)* Torch‑lit cathedral bracket converging on a central gold laurel "Champion" crest; deep one‑point perspective; gold connector lines on near‑black. **Replace** all ~16 competitor avatar busts with **varied stick‑figure busts** (different helms/hoods/weapons, mixed blue/red/neutral tints, a few **stick‑undead** for variety). Keep the centre crest zone and bracket‑label zones **clear**. *(negative)*

### Leaderboard
*(style)* Front‑on ranking table, dark ruined‑castle skyline backdrop, gold trim, league‑coloured badges. **Replace** every player avatar bust with a **distinct stick‑figure bust** (helms/hoods/weapons varied). Keep rank/name/score/badge zones **clear**. *(negative)*

### ClanScreen
*(style)* Three‑panel guild dashboard, dark castle backdrop, gold filigree frames. **Replace the dragon clan crest** with a **stick‑style heraldic emblem** (a crowned **stick‑king silhouette** or **crossed stick‑weapons** on a faction shield). **Replace** all member/chat avatar busts with **stick‑figure busts**. Keep roster/chat/tab zones **clear**. *(negative)*

### EventsHub
*(style)* Rainy night siege hub; featured top banner + four event cards; gold frames, cool‑blue rim over warm embers. **Replace**: banner army → **stick army** before a wagon in rain; "Endless Rush" monster skull → **stick‑undead head** with green necrotic glow; "Hero Trials" knight → **stick warrior** in a fiery scene; "Arena Clash" warriors → **two dueling stick fighters** (blue vs red). Keep title/card‑text/button zones **clear**. *(negative)*

### OnlineBattle
*(style)* Symmetric async‑PvP VS scene, dark war‑hall, explosive blue‑vs‑red "VS" disc focal centre, split side lighting. **Replace** the two flanking realistic knights with **stick champions** (blue stick Commander left, red stick Brute right), depth‑blurred behind the banners. Keep VS/panel/FIND‑MATCH zones **clear**. *(negative)*

### Store (featured bundle + Battle‑Pass art)
*(style)* Torch‑lit treasury hall; central glowing chest+gem hero spot; gold‑on‑black opulent framing; warm torch + cool gem glow. **Replace** the Battle‑Pass knight/king render (right) with a **stick KING** hero in royal blue‑gold regalia; **replace** the horned crown atop the bundle with a **stick‑style crown/helm** silhouette. Keep all card/price/tab zones **clear**. *(negative)*

### LuckySpin
*(style)* Casino prize‑wheel on near‑black; warm gold rim glow; multicolour segment fills. **Replace the golden lion hub emblem** with a **stick‑king or crossed‑stick‑weapons gold crest**; **replace** the "Exclusive Avatar" segment portrait with a **stick‑figure avatar**. Keep wheel‑segment label and button zones **clear**. *(negative)*

## E. S1 — minor / background / gear‑only (lighter swaps)

> For these, regenerate only the small/background creature element in stick style; everything else (environment, lighting, UI zones) is preserved unchanged.

- **MainMenu:** keep the **stick hero trio** (king/mage/archer) — they are correct. **Remove the sky dragons** (replace with smoke/birds/banners) and **convert the right‑side realistic cavalry** to **stick cavalry** or omit. Keep logo/currency/button zones clear. *(style + negative)*
- **ModeSelect:** keep four stick cards; **replace the "Endless" card's green zombie** with a **stick‑undead head** (green necrotic accents). *(style + negative)*
- **Chests:** **replace the hooded robed humanoid** behind the chest with a **stick mage/king silhouette** (or remove for a clean arcane vault). *(style + negative)*
- **ChestOpenResult:** **replace the realistic "Lionhelm" helmet** reward art with a **stick‑style helm icon** (simple gold‑trim crest helm). *(style + negative)*
- **FreeRewards:** **replace the "Battle Boost" knight thumbnail** with a **stick warrior** thumbnail. *(style + negative)*
- **CampaignMap:** **replace the node‑7 realistic avatar token + HEROES icon** with a **stick‑figure token/icon**; keep the green→lava biome map unchanged. *(style + negative)*
- **Pause:** **replace the blurred realistic soldier backdrop** with a **blurred stick‑army** battlefield (same banners/fires/blur). *(style + negative)*
- **Victory:** **replace the flag‑bearer + background soldiers** with **stick victors** raising a blue banner; keep the dusk god‑ray reward composition. *(style + negative)*
- **Defeat:** **replace the foreground kneeling realistic knight** with a **kneeling stick warrior** (tattered cape, dropped sword), same bleak storm composition. *(style + negative)*
- **CampaignResult:** **replace blurred background soldier silhouettes** with **blurred stick‑army** silhouettes. *(style + negative)*

## F. Production notes

- **Output as layers where the tool allows** (character layer separate from background) so the live UI and the corrected characters composite cleanly without re‑introducing baked UI.
- **Aspect ratio:** generate at the mockup's ratio (≈16:9 landscape) at ≥2048 px wide; downscale in the asset pipeline.
- **Consistency pass:** generate the **archetype sheet** (Report 08) FIRST, then reuse those exact stick designs across all screens so the king/mage/archer/etc. look identical everywhere.
- **Validation:** every regenerated image must pass the Report 02 classifier (zero `NON‑STICK` characters) before it enters the asset pipeline.
