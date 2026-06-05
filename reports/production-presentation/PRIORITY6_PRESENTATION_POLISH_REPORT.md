# BULWARK — Production Presentation · Priority #6: Presentation Polish Report

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 **Option A** — NOT roadmap Phase 5).
**Build:** `3a7c7d9` (CI run 27033293761, **GREEN**). **Method:** author → independent review → adversarial audit →
repair → CI/CD GREEN → (Android device capture deferred — owner request). **Inviolable:** presentation-only,
**NO ECS writes**; no gameplay/balance/AI/economy/commander-budget/monetization/canon change; **GATE 1 FAIL /
GATE 2 / GATE 3 / GATE 5 all preserved**; deferred GATE-1 bugs untouched.

---

## Objective
A cohesion/finish pass over the now-built presentation stack (sprites + HUD + audio + VFX + animation) — make the
front-end feel finished and mobile-correct, without touching gameplay.

## What was polished (all presentation-layer, removable, no new assets)
| Item | Change |
|---|---|
| **Screen transitions** | Each UiFlow panel gets a `CanvasGroup`; `Show()` fades the incoming screen in over 0.22 s using **unscaled** time (menus run at `Time.timeScale=0`). Splash→Menu→Mode Select→Victory/Defeat now fade instead of snapping; Match activates no panel (battlefield stays clear). |
| **Button feedback** | `btn.colors` normal/highlighted/**pressed**/disabled (+ fade) on every uGUI button — tactile press response; each button's tint is preserved at normal state. |
| **Audio settings affordance** | `AudioManager.ToggleMute` + `SfxChannel.SetVolume`; a **SOUND: ON/OFF** toggle on the Main Menu (mutes both music + SFX). |
| **Quit** | Main-menu **QUIT** now calls `Application.Quit()` (was a no-op). |
| **HUD safe-area** | `BattleHud.ApplySafeArea` insets the HUD root to `Screen.safeArea` (notch / nav-bar) so the gold chip, statue-HP bars, and troop buttons stay reachable; re-applied on change; div-by-zero guarded; cannot collapse or clip. |

## Validation
- **Independent review + adversarial audit (2 lenses, PASS, 0 blockers):**
  - *Compile* — `CanvasGroup`/`Button.colors` (ColorBlock)/`StartCoroutine`+`IEnumerator`/`Application.Quit`/
    `Screen.safeArea` (Rect `!=`) all resolve; `Button()` returning `Text` is back-compatible with the bare-statement
    call sites; the `Button` method-name vs `UnityEngine.UI.Button` type and the nested `Screen` enum vs
    `UnityEngine.Screen` were both confirmed unambiguous; braces/parens balanced.
  - *Presentation-safety + correctness* — **zero ECS writes / no gameplay change** confirmed across all three files;
    **fade uses unscaled time** (no hang at `timeScale=0`) and always terminates at α=1 (no stuck-fade/soft-lock;
    Match never blacked); **mute** flips both channels (null-guarded, label consistent); **safe-area** cannot make
    the HUD unreachable/zero-sized (full-screen safe area → original full-screen root); per-button tint preserved
    and the BattleHud train-button affordability colors still work. One LOW (the >1 hover-brighten is a no-op on
    touch — harmless).
- **CI/CD:** **GREEN** — IL2CPP Android build + EditMode/PlayMode compile tests pass on `3a7c7d9`.
- **APK size:** 40.62 → **40.63 MB (+0.01 MB)** — code only, no new assets.
- **On-device (Android):** **deferred at owner's request** (phone not connectable). No screenshots fabricated.

## Remaining presentation gaps (out of scope for #6)
- No full Settings screen (a single mute toggle covers the audio affordance; volume sliders are later).
- Menus inset only their content via centering (backgrounds intentionally fill the screen); a full safe-area
  content container for menus is a later refinement (the HUD — where edges matter most — is safe-area-correct).
- 9-sliced UI kit / TMP font / branded art still placeholder; skeletal animation still deferred (Spine).
- All placeholder art/audio is © ripped — **replace with original/licensed assets before any release.**

## Verdict
The presentation polish — **fade transitions, button press feedback, an audio mute toggle, a working Quit, and
HUD safe-area** — is **implemented, review-validated, and CI-GREEN**, fully **presentation-only with no
gameplay/ECS impact** (GATE 1 FAIL and the deferred gates all preserved; not roadmap Phase 5). The front-end now
reads as a finished, mobile-correct game shell over the sprite/HUD/audio/VFX/animation layers.

**STOPPING after Priority #6 per instruction.** (Priority #7 — remaining Phase-5 roadmap deliverables — is a
separate authorization and is NOT started.)

---

## Production Presentation phase — summary (Priorities #1–#6, all CI-GREEN + review-validated)
| # | Deliverable | Build | Report |
|---|---|---|---|
| 1 | Character visual differentiation (5 archetypes/faction, read-only) | `74bf9a7` | PRIORITY1_CHARACTER_DIFFERENTIATION.md |
| 2 | Battle HUD (gold + statue-HP bars + troop buttons) | `b35ff98` | PRIORITY2_BATTLE_HUD.md |
| 3 | Audio framework (music + stingers + UI/combat SFX) | `25db99c` | PRIORITY3_AUDIO_FRAMEWORK_REPORT.md |
| 4 | VFX framework (impact / death / statue, pooled) | `1fa732c` | PRIORITY4_VFX_FRAMEWORK_REPORT.md |
| 5 | Animation framework (procedural idle/walk/hit/death) | `268763f` | PRIORITY5_ANIMATION_FRAMEWORK_REPORT.md |
| 6 | Presentation polish (fades/feedback/mute/safe-area) | `3a7c7d9` | this report |

**All six are presentation-only, removable, with zero gameplay/balance/AI/economy/canon change; GATE 1 FAIL +
GATE 2/3/5 preserved; not roadmap Phase 5.** Six on-device captures (differentiated battlefield, HUD, audio, VFX,
animation, polish) are queued for when the device is reconnectable. Placeholder assets remain dev-only (replace
before release).
