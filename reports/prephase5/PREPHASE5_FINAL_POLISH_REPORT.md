# BULWARK — Pre-Phase-5 Final Polish Report

**Date:** 2026-06-05 · **Track:** pre-Phase-5 presentation **polish only** (NOT Phase 5). **Device:** Xiaomi
Redmi Note 11R, Android 13, arm64-v8a. **Build:** `28eb1ff` (CI run 27006274054, **GREEN**). **Method:** author →
independent review (2-lens) → adversarial audit → repair → CI/CD GREEN → Android validation.
**Inviolable:** no gameplay/balance/AI/economy/canon/roadmap change; deferred GATE-1 bugs untouched.
**Evidence:** before = `runtime/device_validation/pres_*.png` (prior integration run), after = `pol_*.png` + `pol_logcat.txt`.

---

## Summary
The three polish issues from `ASSET_INTEGRATION_REPORT.md` §6 are fixed and **device-validated**: the menus/end
screens are now **clean** (no debug-overlay bleed), the battlefield has a **real background**, and units are
**larger/more readable**. All presentation-layer, removable, zero gameplay impact. **APK +0.19 MB**, no per-frame
cost.

## Task 1 — Remove debug bleed ✅
- **Change:** new `PresentationState.SuppressDebugHud` (static flag); `UiFlow.Show()` sets it so the IMGUI debug
  overlays (`SimDebugOverlay` + `SimPlayerHud`) draw **only during Match** and are hidden on Splash / Main Menu /
  Mode Select / Victory / Defeat. The debug **systems are not removed** (observability preserved — they still run
  and still emit logcat); only their `OnGUI` visibility is gated. Default = shown, so debug-only runs are unchanged.
- **Result (device):** `pol_2_menu.png` = a **clean Main Menu** ("BULWARK", PLAY/QUIT, "Iron Pact vs Ashen Horde")
  with **no debug overlay**; `pol_6_victory.png` = a **clean VICTORY** screen ("…enemy statue has fallen." + MAIN
  MENU) with no debug overlay. (Before: `pres_2_modeselect.png` showed the IMGUI stats panel + train buttons
  plastered over the menu.) Debug correctly reappears in Match (`pol_4_match.png`), per spec.

## Task 2 — Real battlefield background ✅
- **Inspection:** reviewed all extracted backgrounds (Stick War `Environment/Backgrounds` + `Terrain`, Age of War
  `bg_art`, Stickman `Bg_Map`). **Selected `background day.png` (1024×1024, 206 KB)** — Stick War's actual in-game
  **day battlefield** scene (clear, recognizable, mobile-sized). Replaced the near-blank 3 KB `bg_battle.png`.
- **Change:** `SimProxyRenderer.ConfigureCamera` now renders the bg as a `SpriteRenderer` **stretched to cover the
  orthographic view**, at z=5 (behind the z=0 unit plane; camera looks +Z), `sortingOrder=-100`. Null-safe (skips
  until loaded). Mobile: one extra static sprite, no per-frame allocation.
- **Result (device):** `pol_4_match.png`/`pol_5_match2.png` show a **blue "day" battlefield backdrop** instead of
  the prior flat dark frame (`pres_3_match.png`). `[ASSETS] 13/13 loaded`, `[PROXY] camera configured`.

## Task 3 — Unit visibility pass ◑ (improved)
- **Reviewed:** camera framing (orthographic, frames the whole front incl. both statues), sprite scale, readability,
  team color. **Change:** bumped sprite target world-heights (units 1.3→**1.7**, statue 2.6→**3.0**, mine 1.1→**1.3**)
  for prominence. Team read = distinct sprites (player Swordwrath / AI Spearton) + light team tint (cobalt/oxblood)
  + white damage-flash, retained.
- **Result:** units are larger and read better against the new bg. **Honest limit:** because the camera frames the
  *entire* wide front (statue-to-statue) on a portrait screen, units are still relatively small on-screen; tightening
  the camera would risk units leaving the view as they march, so framing was left full-front (deferred, see gaps).
  No gameplay change (purely sprite size + existing camera logic).

## Performance impact
- **No per-frame ECS/CPU cost added:** Task 1 is a boolean check; Task 2 is one static background sprite sized once
  in `ConfigureCamera`; Task 3 is a constant change. The ECS sim (33 systems) is untouched. App ran at the same
  cadence; full match resolved (`Match=Victory`, 22 frames latched) as before.
- **Memory:** +1 background texture (206 KB) replacing a 3 KB one; negligible.

## APK size impact
| Build | android-build artifact | Δ |
|---|---|---|
| Pre-art baseline (ab372d3) | 38.09 MB | — |
| Asset integration (b218e59) | 39.15 MB | +1.06 MB |
| **Final polish (28eb1ff)** | **39.34 MB** | **+0.19 MB** (the real 206 KB battle bg vs the old 3 KB) |

## Before vs after (screenshots)
| Screen | Before (`pres_*`) | After (`pol_*`) |
|---|---|---|
| Main Menu / Mode Select | debug stats panel + train buttons drawn over the menu | **clean** — BULWARK title + PLAY/QUIT + textured bg, no debug |
| Match | flat dark frame, faint tiny sprites, debug overlay | **blue day battlefield bg**, larger unit/statue/mine sprites, debug HUD (allowed in Match) |
| Victory | "VICTORY" with debug overlay bleeding on top | **clean** "VICTORY" + "enemy statue has fallen." + MAIN MENU |
(Files: `runtime/device_validation/{pres_*,pol_*}.png` — local, gitignored.)

## Presentation improvements delivered
1. Clean, game-like menu/end screens (debug overlays gated to Match only).
2. A recognizable battlefield backdrop (no more blank dark frame).
3. Larger, more readable battlefield sprites.
4. All on top of the already-working Splash → Main Menu → Mode Select → Match → Victory flow.

## Remaining presentation gaps (deferred — not in scope here)
- **Units still small** on the wide full-front framing (a tighter/dynamic camera or a dedicated battle-view would
  help, but risks units off-screen — needs design, deferred).
- **In-match HUD is still IMGUI** (intentionally shown in Match for observability); a textured uGUI battle HUD
  (bars/buttons from the asset sets) is a future step (`UI_RECREATION_PLAN.md`).
- **Single static backdrop** (no parallax/terrain/props); **statue phase-swap, VFX, audio** not yet wired.
- **Per-archetype unit sprites** still need a spawn-side visual-id (units share one team sprite).
- **Replay** shows the prior result instantly (needs a scene reload) — unchanged.
- **Licensing:** every placeholder is © ripped — **dev-only, replace with original/licensed art before any release
  or public playtest** (unchanged, binding).

## Verdict
The presentation polish is **implemented, CI-GREEN, review-PASS, and device-validated**: the app now presents
**clean menus + end screens and a recognizable battlefield**, completing the visual transformation from debug
prototype to a game-like build — with **zero gameplay/balance/AI/economy/canon change and the deferred GATE-1 bugs
preserved.**

**STOPPING after this report per instruction. No Phase 5. No gameplay/canon/roadmap change. Deferred GATE-1 bugs
untouched. Placeholder assets remain dev-only (replace before release).**
