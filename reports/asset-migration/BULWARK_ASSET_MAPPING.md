# BULWARK — Asset Mapping (Phase B)

**Date:** 2026-06-05 · Maps every BULWARK object → a usable extracted (placeholder) asset **or** MISSING.
Inputs: `ASSET_INVENTORY.md`. **Placeholder-only / © ripped — replace before release.** SW=Stick War,
AOW=Age of War, SM=Stickman Master. **Action:** SPRITE = drop-in static PNG via the `SimProxyRenderer` seam ·
UI = uGUI Image/Button · AUDIO = AudioSource · SPINE = deferred (rig exists) · BUILD = author original (gap).

## 1. Units (battlefield sprites — Team 0 blue / Team 1 red)
| BULWARK unit | Role | Mapped placeholder | Src/path | Fit | Action |
|---|---|---|---|---|---|
| IronPact Shieldman | Frontline/Shield | Spearton (shield) sprite | SW `Characters/Spearton/Sprites/` | ✅ strong (shield archetype) | SPRITE (blue) |
| IronPact Legionary | Skirmisher | Swordwrath sprite | SW `Characters/Swordwrath/Sprites/` | ✅ | SPRITE (blue) |
| IronPact Ironclad | Heavy | Giant sprite | SW `Characters/Giant/Sprites/` | ✅ (bulky) | SPRITE (blue) |
| IronPact Crossbow | Ranged | Archidon (archer) sprite | SW `Characters/Archidon/Sprites/` | ✅ | SPRITE (blue) |
| IronPact Battlemage | Caster/Fire | Magikill sprite | SW `Characters/Magikill/Sprites/` | ✅ | SPRITE (blue) |
| IronPact Miner | Miner | Miner sprite | SW `Characters/Miner/Sprites/` | ✅ exact | SPRITE (blue, `MinerTag`) |
| Ashen Raider | Skirmisher | Swordwrath **red recolor** / Zombie var | SW skins / Zombie | ✅ | SPRITE (red) |
| Ashen Razorbeast | Heavy | Giant **Giant_Red** skin | SW `Giant` (Giant_Red) | ✅ | SPRITE (red) |
| Ashen Slinger | Ranged | Archidon recolor | SW | ✅ | SPRITE (red) |
| Ashen Houndmaster | Flanker | Swordwrath/Zombie var | SW | ◑ ok (no exact hound) | SPRITE (red) |
| Ashen Hexcaster | Caster/Poison | Magikill recolor | SW | ✅ | SPRITE (red) |
| Ashen Miner | Miner | Miner recolor | SW | ✅ | SPRITE (red, `MinerTag`) |
> Animation: SW **Universal/Giant Spine rigs** map to these for a later animated pass (SPINE, deferred).

## 2. Statue, Mine, Terrain, Environment
| BULWARK object | Mapped placeholder | Src/path | Fit | Action |
|---|---|---|---|---|
| Statue (Iron Pact, 4 phases) | StatueBase + StatueCracks overlay (Intact→Cracked→Breaking→Destroyed) + ash | SW `Environment/Buildings/{StatueBase,StatueCracks,statuesash-bone*,FULL_STATUE}` | ✅ direct (crack overlay = damage states) | SPRITE (+phase swap by `StatueState.Phase`) |
| Statue (Ashen Horde, 4 phases) | same set, red/oxblood tint or alt statue skin | SW statue skins (Kai/King) | ✅ | SPRITE (tint) |
| Statue shield overlay | glow/aura texture | SM `fx/effects/Glow_*`, SW `VFX/glow4` | ◑ (overlay when `ShieldActive`) | SPRITE/VFX |
| Mine node | Mine.png / mineGold.png | SW `Environment/Campaign/Mine.png`, `Unknown/Sprites/mineGold.png` | ✅ | SPRITE (replaces yellow cube) |
| Terrain (HighGround/Choke/Cover/Hazard) | terrain tiles | SW `Environment/Terrain/` | ◑ generic (re-theme) | SPRITE (decor, optional) |
| Map background (OpenField/ChokePass/Ridgeline) | parallax bg | SW `Environment/Backgrounds/`, AOW `bg_art`, SM `Bg_Map*` | ✅ | UI/world quad backdrop |
| Base/barracks structure | armory tent / buildings | SW `Environment/Buildings/` | ◑ optional | SPRITE (decor) |

## 3. Projectiles, VFX, Spells, Status, Damage-type
| BULWARK object | Mapped placeholder | Src/path | Fit | Action |
|---|---|---|---|---|
| Projectiles (cosmetic — sim is instant-hit) | Arrow/Spears/bullets | SW `Campaign/Arrow.png,Spears.png`; AOW `VFX/bullets.png` | ◑ cosmetic only | VFX (optional) |
| Damage-type hit VFX (Melee=steel/Pierce=white/Blunt=dust/Fire=orange/Poison=green) | blood/spark + magic textures (color-tinted) | SW `VFX/` + SM `fx/` | ◑ (rebuild emitters) | VFX |
| Spells: ArrowStorm/LightningStorm/Shatter | lightning + magic VFX + ground decal | SW `VFX/spell-lightning,magic_orb2` | ◑ | VFX (telegraph BUILD) |
| Spells: Freeze/PoisonCloud/Stun | cloud/glow + status overlay | SW `VFX/cloud_magic`, SM `fx/Smoke*` | ◑ | VFX |
| Spells: GoldRush/RaiseGold/Haste/Rage | UI/buff flourish + aura | SM `fx/glow*`, SW gold icons | ◑ | VFX/UI |
| Spells: SummonGiant/SummonPouncer | summon circle + reuse unit sprite | SW VFX + unit sprites | ◑ | VFX |
| Status overlays (8) | aura/glow/smoke + small icons | SW/SM glow/smoke + `UI/Icons` | ◑ | VFX/icon |

## 4. UI (screens, buttons, icons, bars, fonts) + Audio
| BULWARK object | Mapped placeholder | Src/path | Fit | Action |
|---|---|---|---|---|
| Splash/Main Menu/Loading bg | menu + loading backgrounds | SW `Environment/Backgrounds/BackgroundLoading`; SM `Img_Bg_Loading_Low` | ✅ (replace logo) | UI |
| Victory / Defeat screens | victory/defeat backgrounds + stingers | SW `Environment/Backgrounds/BackgroundVictory/Defeat` + SW/SM win/lose music | ✅ | UI + AUDIO |
| Mode Select / Profile / Settings / Shop panels | panels/banners/store frames | SW `UI/Menus/` (store_panel, titlebar, banner) | ✅ | UI |
| Buttons (train/advance/pause/nav) | button sprites + states | SW `UI/Buttons/`, SM `ui/buttons/` | ✅ (re-author 9-slice) | UI Button |
| Icons (gold/role/spell/status/commander) | icon library | SW `UI/Icons/`, SM `ui/icons/` | ✅ | UI Image |
| **Statue-HP / unit-HP / build bars** | HP/charge/progress bars | SM `AssetLibrary/ui/bars/` (Img_Hpbar) | ✅ direct | UI (9-slice fill) |
| Sliders / toggles (settings) | sliders/toggles | AOW `UI/{Sliders,Toggles}/` | ✅ | UI |
| Commander portraits | generals portraits | AOW `Characters/Generals/` | ◑ (re-theme) | UI Image |
| Font (UI/HUD/title) | **Roboto-Regular/Medium (Apache-2.0)** | SM `ui/fonts/Roboto-*.ttf` | ✅ safe | TMP font |
| Music (menu/battle/victory/defeat) | tracks + stingers | SW `Audio/Music/`, SM `audio/music/{Win,Lose,Home,BossFight}` | ✅ | AUDIO (→OGG) |
| SFX (attack/hit/death/mine/gold/UI/statue) | combat + UI SFX | SW `Audio/{Combat,UI}/`, SM `audio/sfx/` | ✅ | AUDIO (→OGG) |
| Reward chest (ChestDef) | chest art / chest Spine | SW `UI/Menus/chest1-3`, `Spine/Chest` | ✅ | UI / SPINE(later) |

## 5. MISSING / must-BUILD (no acceptable ripped placeholder — see gap analysis)
- **BULWARK faction crests/branding** (Iron Pact steel-cobalt / Ashen ember-oxblood) — IP-critical, BUILD.
- **Final statue identity** per faction (ripped Stick War statue is placeholder; real BULWARK statue = BUILD).
- **Cosmetic-recolor masks/tiers** (Standard→Mythic) — GENERATE over locked silhouette (gated on production art).
- **Branded splash/logo** (must not show Stick War branding) — BUILD.
- **Licensed/original everything** before release (the whole ripped set is replace-before-ship).
- **Animation** (Spine) — rigs exist but integration deferred (version-split, paid runtime) → not "missing", deferred.

## 6. Mapping verdict
Every gameplay-visible BULWARK object has a **usable drop-in placeholder** (units→SW sprites, statue→SW
statue+cracks, mine→SW Mine, HUD bars→SM, screens→SW backgrounds+menus, font→Roboto, audio→SW/SM). The only true
authoring gaps are **IP-critical branding/crests/final-statue/cosmetics** (BUILD/GENERATE) and **skeletal
animation** (deferred). This is sufficient to make BULWARK look and play like a recognizable game in Phase E.
