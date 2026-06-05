# BULWARK — Asset Inventory (Phase A · gate before import)

**Date:** 2026-06-05 · Source root: `~/Documents/games-assets/` · Companion: `ASSET_MIGRATION_DISCOVERY_REPORT.md`.
**Legend** — *State/Reuse:* READY = drop-in PNG/WAV/TTF · PROCESS = needs tooling (Spine/slice/9-slice) · BROKEN =
not directly usable. *Mobile:* OK = ship-size after OGG/atlas · LARGE = downscale/compress first. **All rows are
© ripped → DEV PLACEHOLDER ONLY (replace before release).** SW=Stick War, AOW=Age of War, SM=Stickman Master.

## A. SUPPLY — reusable extracted assets (BULWARK-relevant)

### A1. Units (sprites + Spine rigs)
| Asset | Src | Path (rel to set) | Type/Format | Res | Anim/Rig | State | Mobile | BULWARK use |
|---|---|---|---|---|---|---|---|---|
| Universal skeleton (5 archetypes) | SW | `RecoveredAssets/Spine/Reconstructed/Universal/` | Spine 4.1.24 (json+atlas+png) | page 2048² | 33 bones/100 anim/19 skins | PROCESS | OK | Frontline/Heavy/Caster/Ranged/Miner via skins; recolor=2nd faction |
| Giant rig | SW | `Spine/Reconstructed/Giant/` | Spine 4.1 | 2048×1024 | 24 bones/18 anim/4 skins | PROCESS | OK | Heavy/Ironclad/Razorbeast |
| Zombie / Meric / Shopkeep / Menu heroes / Chest / Flags / Hand | SW | `Spine/Reconstructed/{Zombie,Meric,ShopMeric,Shopkeep,MenuArcher_Archidon,MenuSpearton_Spearton,Chest,Flags_Sticks,Hand,HandPointer}/` | Spine 4.1 | ≤2048² | varies | PROCESS | OK | enemy line / NPC / menu hero / reward chest / banners / FTUE pointer |
| Per-part unit sprites (Spearton 255, Swordwrath 197, Giant 167, Magikill 156, Archidon 149, Miner 132, Zombie 61) | SW | `RecoveredAssets/Characters/<Unit>/Sprites/` | PNG RGBA (parts) | small | static parts | **READY** | OK | **the drop-in unit art for this pass** (pick a representative body sprite per role/team) |
| Units_Roster (12 skins) / DinoRider / MountedKnight / FutureWarrior | AOW | `Spine/Reconstructed/{Units_Roster,StoneAge_DinoRider,CastleAge_MountedKnight,FutureAge_Warrior}/` | Spine 2.x | ≤1024² | shared+own | PROCESS | OK | alt unit/cavalry/heavy candidates |
| Generals portraits (30) | AOW | `RecoveredAssets/Characters/Generals/` | PNG | ~200² | static | READY | OK | **commander portraits** (re-theme) |
| 74 Spine sets (Fighter/Demon/Witch/Pets×10/…) | SM | `AssetLibrary/animation/spine/` | Spine 3.8 | ≤2048² | 322 anim | PROCESS | OK | alt rigged units / summon pets |

### A2. Statue (win/lose object) + Mine
| Asset | Src | Path | Format | Res | State | Mobile | BULWARK use |
|---|---|---|---|---|---|---|---|
| **Statue set** (base/cracks/death/ash + skins) | SW | `RecoveredAssets/Environment/Buildings/` (`FULL_STATUE.png`,`StatueBase.png` 256×145,`StatueCracks.png` 512²,`statuesash-bone*`, Juggerknight 1024²) | PNG (+prefab json, logic stripped) | mixed | READY (PNG) | OK | **2 statues × 4 `StatuePhase`** (base + crack overlay = damage states) — top priority |
| Statue skin icons (Kai/King/…) | SW | `Environment/Buildings/*__statueicons_assets_all.png` | PNG | icon | READY | OK | statue/faction-skin select icons |
| Tower stack (base/floor/roof) | SM | `AssetLibrary/environment/backgrounds/Bg_Tower_*.png` | PNG | ≤305² | READY | OK | alt statue/tower placeholder |
| `bases` flipbook (5 frames) | AOW | `RecoveredAssets/Animation/Flipbooks/bases/` | PNG seq + frames.json | 481×350 | READY | OK | alt animated objective |
| **Mine** | SW | `Environment/Campaign/Mine.png` 300², `Unknown/Sprites/mineGold.png` 488×348; `VFX/Mine Spark.prefab.json` | PNG (+VFX hook) | mid | READY | OK | **mine node** (replace yellow cube proxy) |

### A3. Projectiles & VFX (textures only — emitters BROKEN, rebuild in-engine)
| Asset | Src | Path | Format | State | BULWARK use |
|---|---|---|---|---|---|
| Arrows/Spears | SW | `Environment/Campaign/{Arrow.png 539×256, Spears.png 757×164}` | PNG | READY | cosmetic projectiles (sim is instant-hit) |
| Bullets atlas + cutouts | AOW | `VFX/bullets.png` 1024² (+ slices) | PNG sheet | PROCESS (slice) | cosmetic projectiles |
| Combat/gore VFX (blood1-14, ash, sparkle, glow) | SW | `VFX/` (354 files/5.4 MB) | PNG flipbooks | READY | hit/blood/spark feedback |
| Magic/spell VFX (lightning, heal ~70f, orb, portal) | SW | `VFX/{spell-lightning,spell-heal,magic_orb2,portal_glow,healingEffectMc*}` | PNG frames | READY | spell/caster VFX |
| FX kit (glow/smoke/explosion sheet) | SM | `AssetLibrary/fx/effects/` (44) | PNG (256-512) | READY | statue aura/mine glow/death smoke |
| Muzzle/aura/comet/shine/shadows | AOW | `VFX/` (48) | PNG | READY | hit/muzzle/glow + **unit ground shadows** |

### A4. UI — buttons / icons / panels / bars / fonts / screens
| Asset | Src | Path | Count/Res | State | Mobile | BULWARK use |
|---|---|---|---|---|---|---|
| Buttons (+ pressed states) | SW | `UI/Buttons/` | 149 (e.g. BlueButton1 500×150) | READY (9-slice = re-author) | OK | menu/HUD buttons |
| Icons (gem/gold/units/achv) | SW | `UI/Icons/` | 273 / 17 MB | READY | OK | currency, unit cards, HUD |
| Menus/panels/banners/chest/loading | SW | `UI/Menus/` | 244 / 18.5 MB (store_panel 351×767, titlebar, loading-circle, chest1-3) | READY | OK | menu/shop/chest screens |
| **Bars** (HP/charge/EXP/progress) | SM | `AssetLibrary/ui/bars/` | 23 (Img_Hpbar 222×21) | READY (9-slice) | OK | **statue-HP bar, unit HP, build progress** |
| Frames/dialogs/banners | SM | `AssetLibrary/ui/frames/` | 18 (dialog_bg 64² 9-slice) | READY | OK | popups/dialogs |
| Buttons/icons (attack/back/tab/skill) | SM | `AssetLibrary/ui/{buttons 107,icons 309}` | per-sprite | READY | OK | HUD action buttons, ability/skill icons |
| Sliders/toggles | AOW | `UI/{Sliders,Toggles}/` | 21 (tiny 9-slice) | READY (9-slice) | OK | settings sliders/mutes |
| Loading/Victory/Defeat backgrounds | SW | `Environment/Backgrounds/{BackgroundLoading,BackgroundVictory*,BackgroundDefeat}` | PNG full-screen | READY | OK | end-of-match + loading (replace logo) |
| Loading bgs (1080p + _Low) | SM | `environment/backgrounds/Img_Bg_Loading*` | 1920×1080 (+_Low) | READY | LARGE→use _Low | splash/loading/menu bg |
| **Fonts** | SW/AOW/SM | `UI/Fonts/` | SW 9, AOW 5, SM 6 | READY | OK | **`Roboto-Regular/Medium` (SM, Apache-2.0 = the safe pick)**; others = placeholder-only (foundry-licensed) |

### A5. Audio (PCM WAV — re-encode to OGG; mono)
| Asset | Src | Path | Count/Size | State | BULWARK use |
|---|---|---|---|---|---|
| Music (battle/menu/shop/victory + stingers) | SW | `Audio/Music/` | 6 / 21.8 MB | READY | match/menu music |
| Combat SFX (hits/blocks/arrow/charge/destroy) | SW | `Audio/Combat/` | 267 / 47 MB | READY | full combat sound layer (multi-variant) |
| UI/callout SFX (click/cast/attack/defend/mine) | SW | `Audio/UI/` | 135 / 34 MB | READY (some 8 kHz = low-fi) | HUD + command feedback |
| Music (Home/Boss/Map/**Win/Lose**) | SM | `audio/music/` | 5 / 12.9 MB | READY | menu/battle + win/lose stingers |
| SFX (button/panel/gold-drop/impacts) | SM | `audio/sfx/` | 73 | READY | UI + economy SFX |
| Combat/UI SFX | AOW | `Audio/{Combat 16,UI}/` | ~20 | READY | extra attack/click/coin |

### A6. Environment / background / terrain / buildings
| Asset | Src | Path | State | BULWARK use |
|---|---|---|---|---|
| Battle backgrounds + terrain (parallax, canyon) | SW | `Environment/{Backgrounds,Terrain}/` (125 terrain) | READY | lane battlefield parallax |
| Buildings/barracks/armory tent | SW | `Environment/Buildings/` (90) | READY | base/train structures |
| Campaign/map art | SW | `Environment/Campaign/` (334) | READY (some SW-specific) | level/map-select meta |
| Battlefield bg + dirt tile | AOW | `Environment/{bg_art 1431×1191, dirt}` | READY | side-scroller bg |
| Region/temple/rock/ground props | SM | `environment/{backgrounds,props,terrain}` | READY | set-dressing, alt shrine |

### A7. Low/zero value (documented, skip for this pass)
OBJ meshes (all sets: flat Spine quads / Unity primitives) — niche; Materials/Animations/Shaders/Scenes JSON dumps
(reference only, logic stripped); AOW `UI/Screens/` baked screenshots (reference); `Unknown/` buckets (hand-sort;
`mineGold.png`, AOW `Award_medal` salvageable); IL2CPP `_MonoScripts.json` (names only, no values).

---

## B. DEMAND — BULWARK objects needing assets (from `Assets/_Game/Data/` + sim)
| BULWARK object | Count | Needs | Best supply (placeholder) |
|---|---|---|---|
| Units — Iron Pact (Shieldman/Legionary/Ironclad/Crossbow/Battlemage/Miner) | 6 | sprite (idle), team=blue | SW Swordwrath/Spearton/Magikill/Archidon/Miner parts |
| Units — Ashen Horde (Raider/Razorbeast/Slinger/Houndmaster/Hexcaster/Miner) | 6 | sprite, team=red | SW recolor skins / Giant / Zombie variants |
| Commanders (Iron Warden, Ashen Warchief) | 2 | portrait + crest + ability icon | AOW Generals portraits; SW UI |
| Statues (per faction × 4 `StatuePhase` + shield) | 2×4+ | phase sprites + shield overlay | **SW statue set (base + StatueCracks)** |
| Mine | ~6-8 inst | node sprite + occupancy VFX | **SW Mine.png / mineGold.png** |
| Spells (12: ArrowStorm/Lightning/Shatter/Freeze/Poison/Stun/GoldRush/RaiseGold/Haste/Rage/SummonGiant/SummonPouncer) | 12 | telegraph decal + effect VFX | SW magic/lightning/heal VFX + FX kits |
| Status overlays (Chilled/Burning/Poisoned/Stunned/Hasted/Raged/GoldBoost) | 7 | per-unit overlay + icon | SW/SM glow/aura/smoke + icons |
| Damage-type hit VFX (Melee/Pierce/Blunt/Fire/Poison) | 5 | impact VFX (color-coded) | SW blood/spark + magic |
| Terrain kinds (HighGround/Choke/Cover/Hazard) | 4 | tile art | SW Terrain set |
| Map backgrounds (Open Field/Choke Pass/Ridgeline) | 3 | parallax bg | SW/AOW/SM backgrounds |
| UI screens (Splash/Loading/MainMenu/ModeSelect/Classic/Tournament/Endless/Profile/Settings/BattleHUD/Victory/Defeat) | 13 | uGUI canvases | SW menus/backgrounds + SM bars + Roboto |
| UI buttons / icons / fonts | many | styled sprites | SW/SM buttons+icons; **Roboto** |
| Music / SFX | full set | menu/battle/win/lose + combat/UI | SW + SM audio |
| Faction crests / cosmetic tiers | 2 + tiers | crest art / recolor masks | BUILD/GENERATE (not ripped) |

## C. Inventory verdict
Supply **covers nearly all demand** with drop-in PNG/WAV/TTF for a presentation pass; the only true gaps are
**IP-critical BUILD items** (BULWARK faction crests/branding, final statue identity, cosmetic-recolor masks) and
**animation** (Spine deferred). Detailed mapping + gap classification follow in `BULWARK_ASSET_MAPPING.md` and
`ASSET_GAP_ANALYSIS.md`. **No assets imported yet** — discovery/inventory gate satisfied.
