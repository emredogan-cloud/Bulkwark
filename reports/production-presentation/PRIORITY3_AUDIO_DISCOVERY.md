# BULWARK — Production Presentation · Priority #3: Audio Discovery (Phase A)

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 Option A — NOT roadmap Phase 5).
**Scope:** audit available audio before importing anything; pick a curated set + a format/loading plan.
**Inputs re-read:** `MIGRATION-AUDIT/`, `reports/asset-migration/*` (ASSET_INVENTORY §A5, IMPORT_RISK §3), and the
extracted audio folders. **Licensing:** every clip below is **© ripped (Stick War: Legacy / Stickman Master /
Age of War) → THROWAWAY DEV PLACEHOLDER ONLY, replace with original/licensed audio before any release.**

---

## 1. Available audio (extracted sets)
| Set | Folder | Count / size | Notes |
|---|---|---|---|
| Stick War — Music | `Audio/Music` | 6 / 22 MB | menu (`Age_run_menu_music` 1ch/44.1k/60.9s), battle (`GatesOfHell`), victory theme, shop; **2 short orchestral-brass stingers** (Positive ~434 KB / Negative ~447 KB) |
| Stick War — Combat SFX | `Audio/Combat` | 267 / 49 MB | hits/blocks/arrow/charge/destroy (multi-variant) |
| Stick War — UI SFX | `Audio/UI` | 135 / 35 MB | click/cast/command (some 8 kHz low-fi) |
| Stickman Master — Music | `audio/music` | 5 | **`Music_Home`, `Music_Win`, `Music_Lose`**, `Music_BossFight`, `Music_Map` |
| Stickman Master — SFX | `audio/sfx` | 73 | **`SFX_Button_Click`, `SFX_Drop_Gold`, `SFX_Hit`, `SFX_Fighter_Impact_Normal_Hit`, `SFX_Fighter_boulder_impact`**, panel open/close |
| Age of War — Audio | `Audio/{UI,Music,Misc}` | ~20 | `button6`, `coin_pickup`, industrial music, ambience |

All source files are **WAV (PCM)**, mostly 44.1 kHz mono/stereo. The Spine/scene metadata is irrelevant to audio.

## 2. Mapping → BULWARK audio slots (curated set, 8 clips)
| Slot | Chosen source (placeholder) | Use | Loop? |
|---|---|---|---|
| `menu_music` | SW `Age_run_menu_music` | Splash / Main Menu / Mode Select | **loop** |
| `battle_music` | SW `GatesOfHell` | Match | **loop** |
| `victory` | SW `MUSIC_EFFECT_Orchestral_Brass_Positive_10` | Victory screen | one-shot stinger |
| `defeat` | SW `MUSIC_EFFECT_Orchestral_Brass_Negative_06` | Defeat screen | one-shot stinger |
| `sfx_click` | SM `SFX_Button_Click` | every uGUI button | one-shot |
| `sfx_train` | SM `SFX_Drop_Gold` | troop trained (enqueue) | one-shot |
| `sfx_hit` | SM `SFX_Hit` | combat hit feedback | one-shot |
| `sfx_death` | SM `SFX_Fighter_boulder_impact` | unit death feedback | one-shot |
> Ambient: out of scope for this first pass (the looping battle music carries atmosphere); noted as a later gap.
> Combat hit/death SFX will be driven from the **presentation layer's** read-only observation of the sim (unit
> count drops / HP-flash), NOT from any sim/gameplay hook — see the framework report (Phase C).

## 3. Format & loading plan (APK-safe, presentation-only)
- **Re-encode WAV → OGG Vorbis** with the available `ffmpeg` (mono; music ~q2 ≈ 80 kbps, SFX ~q3) into
  `Assets/StreamingAssets/bulwark_audio/`. Target: music tracks ≤ ~0.7 MB each, SFX a few KB each → **total well
  under ~2 MB** added to the APK (vs ~106 MB of raw WAV banks — bulk import is rejected per IMPORT_RISK §3).
- **Runtime load (no editor import):** `UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS)` from
  StreamingAssets — same robust pattern as the sprite loader (`PlaceholderAssets`). **Requires adding the engine
  module `com.unity.modules.unitywebrequestaudio`** (currently absent — only base unitywebrequest + assetbundle +
  texture are present). This mirrors the `unitywebrequesttexture` addition for sprites.
- **Playback:** `AudioSource` components — one Music channel (loop, cross-faded on screen change) + one (or a tiny
  pool of) SFX channel(s) (`PlayOneShot`). Pure GameObject/AudioSource — **independent of ECS**.

## 4. Risks (carried to Phase D)
- **APK size** — controlled by OGG + curation (≤ ~2 MB).
- **Module** — `unitywebrequestaudio` must be added or `GetAudioClip` won't compile/resolve (CI will catch).
- **Async load** — clips load over ~1 s; the framework must no-op safely until a clip is `Ready` (no null play).
- **Loop/dup bugs** — the integration must switch music on screen change (stop old, start new) and fire stingers
  exactly once (Phase C rules); the framework must guard against re-triggering.
- **Licensing** — dev-only; replace before release.

## 5. Verdict
Audio coverage is **complete** for the required slots (menu/battle music + victory/defeat stingers + click/train/
hit/death SFX) using small curated placeholders. **Nothing imported yet** (discovery gate satisfied). Proceed to
Phase B (AudioManager / MusicChannel / SfxChannel) → C (flow integration) → D (validation), per
`PRIORITY3_AUDIO_FRAMEWORK_REPORT.md`.
