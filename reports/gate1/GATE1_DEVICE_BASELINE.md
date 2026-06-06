# BULWARK — GATE 1 · Phase A: Device Baseline

**Date:** 2026-06-06 · **Device:** Xiaomi Redmi Note 11R (Android 13, arm64-v8a). **Build:** `28099ec`
(CI run 27048214222, **GREEN** — latest `main` + the read-only `[GATE1]` telemetry logger).
**Campaign rule:** observation only — no gameplay/feature/asset/UI/audio/VFX/animation/balance change. The only
code added is a **read-only logcat telemetry logger** (no gameplay effect; see `GateTelemetry.cs`).
**Evidence:** `runtime/device_validation/gate1/` (`baseline_*.png`, `metrics_*.txt`, `gate1_m1..10.txt`).

---

## Install
APK installed (`adb install -r -g`), package `com.DefaultCompany.bulwarkclean` launches via
`UnityPlayerGameActivity`. `am start … Status: ok` on all 10 launches.

## Verification checklist (observed on device)
| Item | Result | Evidence |
|---|---|---|
| Launch | ✅ launches; `[ASSETS] 13/13 sprites`, `[VFX]`/`[ANIM]`/`[AUDIO]` boot logs present | logcat |
| Menu flow | ✅ Splash → Main Menu → Mode Select (fade transitions) | `baseline_modeselect.png`; `[UI] screen=` logs |
| Mode selection | ✅ CLASSIC starts a match | `[UI] StartMatch mode=Classic` |
| Match start | ✅ battlefield + sim run on Play | `[GATE1] MATCH n START` |
| Battle HUD | ✅ gold chip, **IRON PACT 1000 / ASHEN HORDE 1000** HP bars, "Units P vs AI", troop buttons (Frontline/Miner/Skirmisher/Heavy/Ranged/Caster + costs), ADVANCE; safe-area applied | `baseline_match1.png` |
| Audio | ✅ framework boots (`[AUDIO] clips ready`) — on-device audibility not machine-verifiable | logcat |
| VFX | ✅ framework boots (`[VFX] textures ready`) | logcat |
| Animation | ✅ framework boots (`[ANIM] AnimationManager booted`) | logcat |

**Presentation layers (Priorities #1–#6) are intact on device.** (`baseline_menu.png` was captured mid fade-in /
pre async-bg and reads near-blank; `baseline_modeselect/match/end` captured cleanly.)

## Runtime metrics
- **Memory:** `dumpsys meminfo` **TOTAL PSS ≈ 341,971 KB (~334 MB)**, RSS ~411 MB — normal range for this Unity/IL2CPP build.
- **Frame timing:** `dumpsys gfxinfo` was inconclusive (only 4 frames in the window — captured near force-stop).
  The in-build `[SIMPROOF]` logger reports a steady **fps ≈ 30** during matches (e.g. `fps=30 sys=33` at t≈120),
  which is the usable frame-rate figure. (No stutter/crash observed across the 10 runs; app stable.)
- **Stability:** 10/10 launches reached a match and a decided end; no crashes/ANRs in logcat.

## What the baseline shows (preview of the gameplay finding)
The app is a **stable, presentation-complete shell** — HUD, sprites, both statue-HP bars, audio/VFX/anim all boot.
But the in-match battlefield is **near-empty** (≈2 player units, ≈1 AI unit; `baseline_match1.png`), which the
Phase-B/C telemetry quantifies. **The presentation works; the contest underneath does not** (see
`GATE1_PLAYTEST_OBSERVATIONS.md`, `GATE1_TELEMETRY.md`).
