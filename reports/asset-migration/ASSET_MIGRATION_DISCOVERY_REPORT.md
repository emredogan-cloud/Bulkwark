# BULWARK — Asset Migration Discovery Report (Phase A)

**Date:** 2026-06-05 · **Track:** Asset Migration / Presentation Pass (NOT Phase 5). **Method:** parallel
discovery workflow over all 3 extracted asset sets + BULWARK's needs + the canon art roadmap, each agent reading
the per-set audit material and verifying on disk. **Rule:** nothing is imported until discovery + inventory are
complete (this report + `ASSET_INVENTORY.md`). **Companion:** `ASSET_INVENTORY.md` (the row-level inventory).

---

## 0. ⚖️ LICENSING — READ FIRST (blocking for any release)
**All three asset sets are ripped, third-party-copyrighted game assets** (Stick War: Legacy & Age of War © Max
Games Studios; Stickman Master © Unimob). Their own bundled reports scope them to **study/modding/personal,
non-distribution use only**. **For BULWARK they are TEMPORARY DEV/PLACEHOLDER assets ONLY** — usable to make the
prototype *look* like a game during development, **never shippable and never in a public playtest build.** Every
imported asset must be replaced with original or properly-licensed art before any release (consistent with the
roadmap's cosmetic-monetization + GATE-4 fairness stance, and the `PLACEHOLDER_ART_INTEGRATION_PLAN.md`).
Additional sharper traps: several bundled **fonts** carry their own foundry licenses (Adobe Myriad/Minion Pro;
Comicraft/Blambot; Arial; Bookman) — do not ship even as placeholder. **`Roboto` (Apache-2.0, in the Stickman set)
is the one license-safe font** and is the recommended dev font. This pass treats canon as unbreached (§15): no new
unit/faction/mechanic is invented — the assets only *skin* BULWARK's existing 12-unit/2-faction/statue/mine line.

## 1. Sources (verified on disk at `~/Documents/games-assets/`)
| Set | Source game | Engine / Spine | Size | Key counts | BULWARK fit |
|---|---|---|---|---|---|
| **`stick-war-assets`** | **Stick War: Legacy** (Max Games) | Unity 6000.0.59f2 IL2CPP · **Spine 4.1.24** | 1.8 GB | 2,956 PNG · 424 WAV · 9 TTF/OTF · 418 OBJ · 49 Spine files (13 packages) | **PRIMARY — near 1:1** (miners→train→push→topple-statue, same archetypes) |
| `age-of-war-assets` | Age of War (Max Games) | Unity 6000.0.60f1 IL2CPP · Spine 2.x | 553 MB | 1,790 PNG · ~100 WAV · 5 TTF · 4 Spine rigs | Secondary (turrets, generals portraits, bullets, UI, base-flipbook) |
| `Stickman-Master-assets` | Stickman Master (Unimob) | Unity 2022.3.62f2 IL2CPP · Spine 3.8 | 662 MB | 3,654 PNG · 160 WAV · 6 fonts (incl. **Roboto/Apache**) · 74 Spine sets | Secondary (Tower stack=statue, HP/charge **bars**, frames, FX, win/lose music, safe font) |

Each set ships its own audit suite (`ENGINEER_START_HERE.md`, `MASTER_ASSET_INDEX`, `ASSET_REUSABILITY_REPORT`,
`CHARACTER_INDEX`, `SPINE_RECONSTRUCTION_REPORT`, `manifest.jsonl`) — these are the "MIGRATION-AUDIT" material;
spot-checks (PNG dims, WAV headers, atlas files, counts) confirmed they are accurate.

## 2. Format reality (decides what this pass can use)
- **DROP-IN (no tooling):** raw **PNG** (units' body-part sprites, statue/mine/projectile/VFX textures, UI
  buttons/icons/panels/backgrounds), **PCM WAV** audio (re-encode to OGG for size), **TTF/OTF** fonts (build a TMP
  atlas). This is the path the presentation pass uses.
- **PROCESS-HEAVY (deferred this pass):** **Spine** rigs are the only *animated* units, but they need the Spine
  Editor + the **paid `spine-unity` runtime**, and the three sets are **version-split** (4.1 / 2.x / 3.8) so they
  can't share one runtime. The BULWARK render seam (`SimProxyRenderer` → `SpriteRenderer`) is **sprite-based, not
  Spine** — so Phase E uses **static sprites**, and full skeletal animation is explicitly deferred (matches the
  `PLACEHOLDER_ART_INTEGRATION_PLAN.md` "start with sprites" guidance and avoids the `entities.graphics`/CI risk).
- **BROKEN / unrecoverable:** all gameplay logic, balance numbers, ScriptableObject/MonoBehaviour data (IL2CPP-
  stripped), **particle emitter settings** (only VFX *textures* survived → rebuild systems in-engine), editable
  `.unity` scenes, and **9-slice border data** (UI atlas JSONs are PathID metadata, not pixel borders → re-author
  9-slice at import). Two gotchas: Stick War Spine **atlas declares 4096 but pages are 2048** (UV rescale needed);
  some UI SFX are **8000 Hz** (low-fi — audition before reuse).

## 3. Highest-value finds for BULWARK (the "it looks like a real game" set)
- **Units (sprites):** Stick War per-part PNG sprite sets for Swordwrath/Spearton/Magikill/Archidon/Miner/Giant
  (+ recolor skins = free 2nd-faction art) → map onto BULWARK's Frontline/Heavy/Caster/Ranged/Miner/Heavy. Spine
  *rigs* exist for later animation.
- **Statue (the win/lose object):** Stick War `Environment/Buildings/` `FULL_STATUE`/`StatueBase`/**`StatueCracks`
  (damage overlay)**/death+ash variants + per-faction statue **skins** — a *direct* match for BULWARK's 4-phase
  `StatuePhase` (Intact/Cracked/Breaking/Destroyed) + shield. (Age-of-War `bases` flipbook + Stickman `Bg_Tower`
  stack are alternates.)
- **Mine:** Stick War `Mine.png` (300×300) + `mineGold.png` (488×348) + Mine-spark VFX.
- **Projectiles/VFX:** arrows/spears (Stick War), bullets atlas (Age of War); blood/spark/ash/heal/lightning/magic
  textures (all three) — note: **BULWARK's sim has no projectile entity** (ranged is instant-hit), so projectiles
  are cosmetic-only.
- **UI:** Stick War **149 buttons / 273 icons / 244 panels** + gold/gem currency icons + loading/victory/defeat
  backgrounds; Stickman **HP/charge/progress bars** + frames; Age-of-War sliders/toggles. Fonts: **Roboto
  (Apache-safe)** + display faces (placeholder-only).
- **Audio:** Stick War music (battle/menu/shop/victory) + **267 combat SFX** + 135 UI/callout SFX; Stickman
  win/lose/boss music. Re-encode to OGG; prune the 86 MB "Misc" bucket.

## 4. BULWARK side (the demand)
BULWARK has a **complete, device-proven ECS sim but ZERO media** — a repo-wide scan found no PNG/WAV/TTF/prefab/
material/Spine in `Assets/`. It needs art/audio for: **12 units** (Iron Pact: Shieldman/Legionary/Ironclad/
Crossbow/Battlemage/Miner; Ashen Horde: Raider/Razorbeast/Slinger/Houndmaster/Hexcaster/Miner), **2 commanders**,
**2 statues × 4 phases + shield**, **mine**, **12 spells** (telegraph + effect VFX), **8 status overlays**, **5
damage-type hit VFX**, **4 terrain kinds**, **3 map backgrounds**, **13 UI screens**, buttons/icons/fonts, music/
SFX, faction crests, cosmetic-recolor tiers. (Full demand list in `ASSET_INVENTORY.md` §B.) The data `.asset`
files hold stats only — **no art-reference fields exist** (schemas have no Sprite/icon bindings), so presentation
binds by **runtime convention** (Team 0/1 + `MinerTag`), via the `SimProxyRenderer` seam — not by data edits.

## 5. Integration seam + constraints (unchanged from Phase 0)
- The single renderer is **`SimProxyRenderer`** (primitive proxies per ECS entity at `Position` on z=0). The clean
  swap is **primitive → `SpriteRenderer`** (URP 2D) in that one removable MonoBehaviour — no ECS/sim/balance change,
  no `entities.graphics`. UI is greenfield (only IMGUI debug today) → add a uGUI Canvas flow.
- Coordinate facts for sprite placement: entities on the **z=0 plane**, Team 0 = player/blue, Team 1 = AI/red,
  3-row lane (rows 0..2), statues at the front ends, orthographic camera (configured by `SimProxyRenderer`).

## 6. Discovery verdict
The migration is **viable and high-value**: Stick War alone covers nearly every BULWARK presentation need with
**drop-in PNG/WAV/TTF**, and the genre match means silhouettes/roles line up. **Spine animation is deferred** (too
heavy/version-split for this pass; sprites first). The **binding caveat is licensing** — everything is a throwaway
dev placeholder to be replaced before release. Proceed to mapping (`BULWARK_ASSET_MAPPING.md`) and gap analysis
(`ASSET_GAP_ANALYSIS.md`); **no import until** the inventory (`ASSET_INVENTORY.md`) is committed.
