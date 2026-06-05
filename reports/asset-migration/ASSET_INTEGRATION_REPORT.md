# BULWARK — Asset Integration Report (Phase E/F · final)

**Date:** 2026-06-05 · **Track:** Asset Migration / Presentation Pass (NOT Phase 5). **Goal:** transform the debug
prototype into a visually recognizable, game-like build using temporary placeholder assets — **without changing
any gameplay system.** **Method:** author → independent review → adversarial audit (3-lens, PASS) → repair → CI
GREEN → device validation. **Stop point:** this report.

---

## 0. Status summary
| Item | Status |
|---|---|
| Phases 0–D (discovery → inventory → mapping → gap → architecture → UI plan → risk) | ✅ delivered (`reports/asset-migration/`) |
| Phase E — presentation integration (sprite battlefield + uGUI flow) | ✅ implemented & committed (`c11f48e`, `b218e59`) |
| Pre-push review (compile + presentation-safety + runtime), 3 lenses | ✅ PASS (0 blockers after the metas fix) |
| CI/CD (IL2CPP Android build + compile tests) | ✅ GREEN (run `26993253252`, sha `b218e59`) |
| **On-device (Android) screenshot validation** | ✅ **DONE on reconnect** (run `b218e59`): 13/13 sprites loaded, full Splash→Menu→ModeSelect→Match→Victory flow executed; menus/end look game-like; battlefield sprites render but faint (see §6) |
| Deferred GATE-1 gameplay bugs | ✅ untouched / preserved (documented in `PROJECT_STATE_ANALYSIS.md` §10) |

## 1. Imported assets (curated placeholders — © ripped, DEV-ONLY)
13 PNGs (~1.1 MB) copied into `Assets/StreamingAssets/bulwark/` (loaded at runtime; provenance README in-folder).
Source: **Stick War: Legacy © Max Games Studios — TEMPORARY DEV PLACEHOLDER, replace before any release.**

| File | Source | BULWARK use |
|---|---|---|
| `unit_player.png` (FULL_SWORDWRATH) | SW | player (Team 0) unit sprite |
| `unit_ai.png` (FULL_SPEARTON) | SW | AI (Team 1) unit sprite |
| `miner.png` (FULL_MINER) | SW | miner sprite (loaded; reserved) |
| `mine.png` (Mine) | SW | mine node |
| `statue.png` (StatueBase) + `statue_cracks.png` | SW | statue objective (+ damage overlay, reserved) |
| `bg_battle/bg_menu/bg_victory/bg_defeat.png` | SW | battle bg + menu/loading + victory/defeat screens |
| `button.png`, `panel.png`, `gold.png` | SW | UI button, panel, currency icon |

Not imported (per `ASSET_IMPORT_RISK_REPORT.md`): Spine rigs (deferred — paid runtime, version-split), bulk audio
(size), OBJ/material/shader dumps (no value), foundry-licensed fonts. The 3 GB source tree is **not** in git.

## 2. Code integrated (presentation layer only, removable)
- **`PlaceholderAssets.cs`** (new) — runtime loader: `UnityWebRequestTexture` from StreamingAssets → `Sprite`
  (bypasses the editor sprite-import pipeline → robust; needed `com.unity.modules.unitywebrequesttexture`, added).
- **`SimProxyRenderer.cs`** (changed) — battlefield proxies switched from `GameObject.CreatePrimitive`
  (capsule/cube/cylinder) to **`SpriteRenderer`** with the placeholder sprites (white-square fallback until loaded);
  team tint, HP scale, damage flash; existing orthographic camera framing retained. **Reads ECS read-only.**
- **`UiFlow.cs`** (new) — code-built **uGUI** flow: **Splash → Main Menu → Mode Select (Classic/Tournament/Endless)
  → Match → Victory/Defeat**. Canvas (overlay) + CanvasScaler (1080×2400) + EventSystem; built-in font; placeholder
  backgrounds/buttons. **Freezes the sim (`Time.timeScale=0`) during menus** so the match starts on *Play*, and
  watches `MatchState` (read-only) for the end screen. The only sim-affecting call is `Time.timeScale` (a pause).

## 3. Placeholders replaced
- **Battlefield:** debug primitive proxies (blue/red capsules, yellow cube, gray cylinder) → **Stick War sprites**
  (units, miner, mine, statue). The IMGUI debug overlay/HUD (`SimDebugOverlay`/`SimPlayerHud`) remain available
  underneath (debug); the uGUI front-end is the player-facing layer.
- **App entry:** previously launched straight into the running ECS battle → now **Splash → Main Menu → Mode Select
  → (Play) → Match → Victory/Defeat**.

## 4. Performance / APK impact
- **APK:** 38.09 MB (pre-art baseline, run 26957761497) → **39.15 MB** (integration, run 26993253252) = **+1.06 MB**
  (the curated StreamingAssets payload). Well within mobile budget.
- **Runtime:** sprites load async once (~1-2 s) via 13 small UnityWebRequest calls; SpriteRenderer is cheap; uGUI is
  a small code-built canvas. No ECS/sim cost added (presentation is GameObject/uGUI, independent of the 33 systems).
- **Memory:** 13 small textures (~1 MB) + 1 canvas. Negligible.

## 5. Rendering verification (CI + review; device pending)
- **Compile/build:** CI GREEN — IL2CPP Android build + EditMode/PlayMode compile tests pass on `b218e59`.
- **Adversarial review (3 lenses, PASS):** compile-correctness (UnityEngine.UI auto-resolves; networking/sprite
  APIs valid), presentation-safety (read-only ECS; only `Time.timeScale`; removable; no balance/AI/economy change),
  runtime-correctness (SpriteRenderer renders under the project's pipeline; menu flow interactive; `Camera.main`-null
  handled via the `allCameras` fallback; bg-refresh-on-ready works).
- **On-device:** see §6 (could not be captured this run — device disconnected).

## 6. Device validation — DONE (Xiaomi Redmi Note 11R, on reconnect)
Installed the CI-GREEN APK (`b218e59`) on the reconnected device and drove the full flow via taps. **Evidence**
(`runtime/device_validation/pres_*.png` + `pres_logcat.txt`):
- **Asset load:** `[ASSETS] placeholder sprites ready: 13/13 loaded.` — all StreamingAssets placeholders loaded at
  runtime on Android (the UnityWebRequestTexture path works on-device).
- **Flow:** `[UI]` log shows `UiFlow booted → Splash → Menu → ModeSelect → StartMatch mode=Classic → Match → End`;
  the match resolved (`Match=Victory`, 37 frames) and the End screen displayed.
- **UI screenshots (game-like):** `pres_2_modeselect.png` = a real **"CHOOSE MODE"** screen (textured background +
  CLASSIC/TOURNAMENT/ENDLESS/BACK buttons); `pres_5_match3.png`/`pres_6_end.png` = a **"VICTORY"** end screen
  (victory background + "…statue has fallen." + MAIN MENU button). The Splash→Menu→ModeSelect→Match→Victory flow
  is real and navigable by touch.
- **Battlefield:** `[PROXY]` shows the sprite renderer active (camera framed; proxies 4→7); units/mine/statue
  render as **sprites** (not primitives). `pres_3_match.png`/`pres_4_match2.png` show the in-match battlefield.

**Honest visual findings (polish gaps, NOT failures):** (1) the **IMGUI debug overlays** (`SimDebugOverlay` /
`SimPlayerHud`) **bleed on top of the uGUI menus/end screens** (OnGUI always draws over uGUI) — the menus read
clearly but look un-clean; they should be hidden while the uGUI flow is in a menu/end state. (2) The **battle
background placeholder (`bg_battle`) is near-blank/dark** (3 KB source) and units are small/few, so the
battlefield is faint vs the strong menu/end screens — swap for a real battlefield background + optionally hide the
debug HUD in-match. Both are quick presentation-layer follow-ups (no gameplay impact). The core objective —
**launches and plays through a recognizable game flow with sprite art instead of debug primitives** — is met.

## 7. Remaining gaps (placeholder → production)
- **On-device screenshots** (above) — pending device reconnect.
- **Per-archetype unit sprites:** all team units currently share one team sprite (the entity exposes only Team +
  `MinerTag`); distinct Shieldman/Crossbow/etc. art needs a spawn-side visual-id (MEDIUM, `ASSET_GAP_ANALYSIS.md`).
- **Animation:** Spine rigs deferred (static sprites only) — units don't animate.
- **Statue phases / shield, terrain, VFX, audio:** loaded art or rebuild deferred (statue uses one sprite scaled by
  HP; `statue_cracks`/audio reserved, not yet wired).
- **Full screen set:** Profile/Settings and a fully-textured Battle HUD (bars/buttons) are designed
  (`UI_RECREATION_PLAN.md`) but not yet built; the IMGUI HUD remains the in-match control surface.
- **Replay:** after a match decides, `MatchState` persists → "Main Menu → Play" would show the stale result; a clean
  rematch needs a scene reload (deferred; first-run flow is the deliverable).
- **9-slice / branding / crests / licensing:** placeholder buttons aren't 9-sliced; BULWARK branding/crests are
  BUILD items; **all ripped assets must be replaced with original/licensed art before any release or public
  playtest** (legal blocker — repeated from every prior report).

## 8. Preservation of gameplay (the binding rule)
**No ECS system, balance number, AI logic, economy, or canon was changed; the deferred GATE-1 bugs (BasicAI vs
SquadAI; miner targeting death; miner replacement) remain exactly as documented and were NOT fixed.** The entire
pass is removable (delete `PlaceholderAssets.cs` + `UiFlow.cs` + `StreamingAssets/bulwark/`, revert
`SimProxyRenderer.cs`, drop the texture module).

## 9. Verdict
The presentation pass is **implemented, committed, CI-GREEN, review-validated, and DEVICE-VALIDATED**: on the
Redmi Note 11R the build loads 13/13 placeholder sprites and launches through a real game flow (Splash → Main
Menu → Mode Select → Match → Victory) with a **sprite battlefield** instead of debug primitives — transforming the
debug prototype into a visually recognizable, navigable game **without touching gameplay** (no ECS/sim/balance/AI/
economy change; deferred GATE-1 bugs preserved; fully removable). The menu and end screens read as a real game;
two **presentation-only polish follow-ups** remain (hide the IMGUI debug overlays during the uGUI flow; swap the
near-blank battle background) — neither affects gameplay, and both are quick. All ripped placeholders are
**dev-only and must be replaced with original/licensed art before any release or public playtest.**

**STOPPING after this report per instruction. No Phase 5. No roadmap/canon/balance change. Deferred gameplay bugs
preserved. Placeholder assets are dev-only and must be replaced before release.**
