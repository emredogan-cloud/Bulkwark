# BULWARK — Asset Import Risk Report (Phase D)

**Date:** 2026-06-05 · Per-class import risk for the presentation pass. **No asset may be imported without
appearing here.** Targets: Unity 6000.0.75f1, URP 17.0.3, IL2CPP, Android arm64-v8a, ECS/DOTS. Inputs:
`ASSET_INVENTORY.md`, `ASSET_GAP_ANALYSIS.md`. Verdict scale: ✅ safe · ⚠️ caution (mitigate) · ⛔ avoid this pass.

## 0. Cross-cutting
- **Licensing (⛔ for ship):** all placeholders are © ripped — dev only, replace before release. Keep them under a
  clearly-named `Assets/_Game/Art/_Placeholder/` (and `_Audio/_Placeholder/`) so they are easy to purge.
- **Source location:** assets live OUTSIDE the repo (`~/Documents/games-assets/`). Import = **copy a curated
  subset** into `Assets/` (with `.meta`). Do **not** add the 3 GB tree to git — copy only the few MB actually used.
- **ECS compatibility (✅):** presentation is GameObject/uGUI/AudioSource — **independent of ECS**. The sprite
  battlefield reads the ECS world **read-only** via the existing `SimProxyRenderer` seam; no entity component is
  added, no system changed. `entities.graphics` stays OUT (CI-unresolvable; not needed for SpriteRenderer).
- **CI risk:** the IL2CPP Android build can go red on a bad `.meta`/import. Mitigation: import a **tiny** curated
  set first, keep `.meta` minimal/valid, validate via the author→review→audit→CI→device workflow (Phase F).

## 1. Sprite PNG (units, statue, mine, UI, backgrounds) — ✅ (primary path)
- **Dependencies:** none (no shader/material/anim needed for `SpriteRenderer`/uGUI `Image`).
- **Shaders/materials:** uses built-in **Sprites/Default** + URP 2D default (already proven to render in the V0
  pass via `Sprites/Default` fallback). No custom shader. UI `Image` uses UI/Default.
- **Texture import:** each PNG needs a `.meta` with `textureType: 8` (Sprite), `spriteMode: 1` (Single),
  `alphaIsTransparency: 1`, sensible `spritePixelsToUnits`, mipmaps **off** for UI, `ASTC` compression for Android.
  9-slice buttons/panels need `spriteBorder` set (lost from source → re-author).
- **URP/Android:** ✅ fully compatible. **Memory/APK:** the controlling factor — **curate hard**. A handful of unit
  sprites + statue phases + mine + ~20 UI sprites + 1-2 backgrounds ≈ a few MB. Downscale large bgs (use SM `_Low`
  1024×512, not 1080p). Atlas UI sprites later to cut draw calls. **Addressables:** not required this pass (place in
  Resources/scene refs); Addressables grouping is a production optimization (deferred).
- **Risk:** low. Watch APK bloat → import only mapped sprites, ASTC, downscale backgrounds.

## 2. Fonts (TTF/OTF) — ✅ Roboto only
- **Use `Roboto-Regular/Medium` (Apache-2.0, Stickman set).** All other bundled fonts (Myriad/Minion Pro, Arial,
  Bookman, Blambot/Comicraft, UVF_Assassin) carry **foundry licenses → ⛔ do not import even as placeholder.**
- **Import:** TTF → generate a **TMP SDF font asset** (one atlas, dynamic or static). **Dependency:** TextMeshPro
  (`com.unity.ugui` includes TMP in Unity 6 — verify in manifest; add TMP Essentials if needed). **URP/Android:**
  ✅. **Memory/APK:** ~0.5-1 MB per SDF atlas; fine.
- **Risk:** low (Roboto). Licensing trap on the others — avoid.

## 3. Audio (WAV → OGG) — ⚠️ size
- **Dependencies:** none (AudioSource/AudioClip). **URP/Android:** ✅.
- **Risk:** WAV banks are **huge** (SW Combat 47 MB, UI 34 MB, Misc 86 MB; mono; some 8 kHz). Importing raw WAV
  bloats the APK badly. **Mitigation:** curate ~10-20 SFX + 3-4 music tracks; **re-encode to OGG/Vorbis** (or set
  Unity import to Vorbis + load-type Compressed-in-memory/Streaming for music); skip the 86 MB Misc bucket; audition
  8 kHz clips. **Memory:** stream music, decompress-on-load short SFX.
- **Verdict:** ✅ after curation+compression; ⛔ to bulk-import raw.

## 4. Spine rigs — ⛔ this pass (deferred)
- **Dependencies:** Spine Editor + **paid `spine-unity` runtime** package; the three sets are **version-split**
  (4.1 / 3.8 / 2.x) → can't share one runtime. **ECS:** Spine is GameObject-based (would parent to the sprite proxy
  seam, fine) but the import/runtime cost + version split + atlas-size mismatch (SW 4096-declared/2048-actual) make
  it high-risk for now. **APK/memory:** runtime + atlases add weight.
- **Verdict:** **deferred** — static sprites first (Phase E). Animation is a later, separate pass with the ADR
  decision. Not "import without docs" — explicitly excluded here.

## 5. VFX textures — ✅ texture / ⚠️ emitters
- **Textures:** PNG flipbooks/particles import as Sprites/textures (✅, same as §1). **Emitter settings are BROKEN**
  (stripped) → particle systems must be **authored in-engine** using the textures. **URP:** use URP Particles/Unlit.
- **Risk:** low to import textures; emitter authoring is effort (MEDIUM) → keep VFX minimal this pass.

## 6. UI 9-slice / atlases — ⚠️ author borders
- Buttons/panels/bars need `spriteBorder` re-authored at import (source border data lost). Master atlases (AOW
  `Buttons.png`, SM tower atlas) need **manual slicing** (sub-rects) — prefer the already-cropped per-sprite PNGs
  to avoid slicing. **Risk:** medium (manual border setup); mitigate by using cropped singles + setting borders.

## 7. OBJ / Materials / Shaders / Scenes / data dumps — ⛔ skip
- OBJ = flat Spine quads / primitives (no value, BULWARK is 2D). Materials/Animations/Shaders/Scenes JSON = logic-
  stripped reference only, not importable. IL2CPP `_MonoScripts.json` = names only. **Do not import.**

## 8. Import policy (binding for Phase E)
1. Copy ONLY mapped, curated assets into `Assets/_Game/Art/_Placeholder/` + `_Audio/_Placeholder/` (a few MB).
2. Author valid minimal `.meta` (Sprite/ASTC/borders; TMP for font; Vorbis for audio).
3. Keep `entities.graphics` OUT; sprites via `SimProxyRenderer`/uGUI only.
4. Stage small → CI GREEN → device-validate (Phase F) → expand.
5. Never commit the 3 GB source tree; document every imported file in `ASSET_INTEGRATION_REPORT.md`.
6. All imports are placeholder (© ripped) — replace before any release; purge path is the `_Placeholder/` folders.

## Risk verdict
The **sprite + Roboto-font + curated-OGG-audio** path is **low-risk and Android/URP/ECS-safe**. The controllable
risks are **APK size** (curate + ASTC + OGG + downscale) and **9-slice authoring** (use cropped singles + set
borders). **Spine and bulk audio are excluded** this pass. Proceed to Phase E with this policy.
