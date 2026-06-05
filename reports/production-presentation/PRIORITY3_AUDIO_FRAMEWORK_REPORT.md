# BULWARK — Production Presentation · Priority #3: Audio Framework Report

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 **Option A** — NOT roadmap Phase 5).
**Build:** `25db99c` (CI run 27023971289, **GREEN**). **Method:** discovery → author → independent review →
adversarial audit → repair → CI/CD GREEN → (Android device capture deferred — owner cannot connect the phone).
**Inviolable:** presentation-only, **NO gameplay dependency, ZERO ECS writes**; no balance/AI/economy/commander-
budget/monetization/canon change; **GATE 1 FAIL / GATE 2 / GATE 3 / GATE 5 all preserved**; deferred GATE-1 bugs untouched.
**Companion:** `PRIORITY3_AUDIO_DISCOVERY.md` (Phase A).

---

## Objective
Transform BULWARK from a **silent prototype** into an **RTS with feedback and atmosphere** — the first complete
audio layer — without touching gameplay.

## Phase A — Discovery (done)
Audited the extracted audio (Stick War 6 music / 267 combat / 135 UI SFX; Stickman Win/Lose/Home + 73 SFX; Age of
War ~20). Selected a curated **8-clip** set and a format plan (re-encode WAV→OGG, runtime-load). See
`PRIORITY3_AUDIO_DISCOVERY.md`. All clips are **© ripped → dev-only placeholders, replace before release.**

## Phase B — Architecture (`AudioManager.cs`, new, removable)
A removable presentation audio layer with the required systems:
- **`AudioManager`** — auto-spawned singleton; coroutine-loads 8 OGGs from `StreamingAssets/bulwark_audio/` via
  `UnityWebRequestMultimedia.GetAudioClip(..., OGGVORBIS)` (no editor import — same robust path as the sprite
  loader). Public façade: `PlayMenuMusic / PlayBattleMusic / StopMusic / ResetStingers / PlayEndStinger(bool) /
  Click / Train / Hit / Death`.
- **`MusicChannel`** — one looping `AudioSource`; `Play(key, clip)` **no-ops if that key is already playing**
  (prevents restart + duplicate playback); null clip → stop.
- **`SfxChannel`** — one `AudioSource` for `PlayOneShot`.
- Curated clips: `menu_music`, `battle_music` (loop) · `victory`, `defeat` (one-shot stingers) · `sfx_click`,
  `sfx_train`, `sfx_hit`, `sfx_death`. **No gameplay dependency** — it reads nothing from and writes nothing to ECS.

## Phase C — Integration (flow rules honored)
| Trigger | Sound | Rule enforced |
|---|---|---|
| Splash / Main Menu / Mode Select (`UiFlow.Show`) | `menu_music` (loop) | **menu music only in menus** |
| Match (`UiFlow.Show`) | `battle_music` (loop) + `ResetStingers` | **battle music only in matches** |
| Victory / Defeat (`UiFlow.ShowEnd`) | `victory`/`defeat` stinger; music stopped | **stinger fires exactly once** (double-guarded: `ShowEnd` `if(_screen==End)return` + `_victoryFired/_defeatFired`) |
| Any uGUI button (UiFlow + BattleHud) | `sfx_click` | — |
| Troop trained (BattleHud) | `sfx_train` | — |
| Unit HP-drop (SimProxyRenderer, read-only) | `sfx_hit` | throttled 0.12 s, Match-gated |
| Unit death/cull (SimProxyRenderer, read-only) | `sfx_death` | throttled 0.15 s, Match-gated, units only |
**No looping bug / no duplicate playback:** single `MusicChannel` with no-restart-on-same-key; menu↔match↔end↔menu
transitions verified (end stops the loop and does not resume; rematch re-arms the stingers). Combat SFX are
**read-only presentation reactions** to the renderer's existing HP-flash / cull — **no sim hook**.

## Phase D — Validation
- **Independent review + adversarial audit (2 lenses, PASS, 0 blockers):**
  - *Compile* — `UnityWebRequestMultimedia/DownloadHandlerAudioClip/AudioType.OGGVORBIS` + `AudioSource/AudioClip/
    PlayOneShot` verified against the editor module DLLs; both modules present in `manifest.json` **and**
    `packages-lock.json`; all call sites map to public members; braces/parens balanced.
  - *Runtime / Phase-D* — **presentation-only confirmed (zero ECS writes, no gameplay dep)**; crash-safe (async
    clips null-guarded end-to-end, failed loads degrade to silence); flow correctness (no double-play/stuck loop,
    stinger-once); combat SFX cannot machine-gun (≤ ~8 hits/s + ~6.6 deaths/s on one channel) or stall.
  - **Memory/APK/Android:** **acceptable** — 1.1 MB OGG in StreamingAssets (runtime-loaded, copied verbatim into
    the APK; jar:// Android path handled). One **LOW** (non-blocking): the two music tracks are decompressed to PCM
    in RAM for the app lifetime (a few MB) — fine at 2 tracks; revisit (stream/compressed-in-memory) only if more/
    longer tracks are added.
- **CI/CD:** **GREEN** — IL2CPP Android build + EditMode/PlayMode compile tests pass on `25db99c`.
- **APK size:** 39.40 → **40.52 MB (+1.12 MB)** — the curated OGG audio.
- **On-device (Android):** **deferred at owner's request** (phone not connectable). No audio "evidence" fabricated.
  When available: install `25db99c` and confirm menu↔battle music switches, button/train/hit/death SFX fire, and
  the victory/defeat stinger plays once.

## Remaining gaps (later)
- Ambient bed not added (the looping battle music carries atmosphere this pass).
- Music held as PCM in RAM (LOW; fine at this scale).
- No volume/mute settings UI yet (the channels expose volume; a Settings screen is later polish).
- Placeholder audio is © ripped — **replace with original/licensed audio before any release.**

## Verdict
The first **complete audio framework** — menu/battle music, one-shot victory/defeat stingers, and UI/combat SFX
(click, train, hit, death) — is **implemented, review-validated, and CI-GREEN**, fully **presentation-only with no
gameplay/ECS impact** (GATE 1 FAIL and the deferred gates all preserved; not roadmap Phase 5). The only deferred
item is on-device audio confirmation (phone unavailable).

**STOPPING after Priority #3 per instruction. Priority #4 (VFX) NOT started.**
