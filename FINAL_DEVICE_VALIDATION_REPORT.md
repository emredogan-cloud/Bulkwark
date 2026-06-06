# BULWARK — FINAL DEVICE VALIDATION REPORT

**Date:** 2026-06-06 · **Method:** real on-device run (adb + logcat + screencap). **Evidence-only — no
fabrication.** All evidence saved under `runtime/device_validation/final_validation/` (full logcat, 12
screenshots, apk_info, launch/gate1/rematch metrics, ci_poll log).

---

## 1. Build under test
| Field | Value |
|---|---|
| **HEAD commit** | `d8111797a84a5a0c88859c830ad75c53292574ba` (origin/main) |
| HEAD = | **UI shell `cdfa056` (WP-00..WP-13)** + **user's `gate1-balance build-3`** (sudden-death escalation, `Combat.cs`) |
| **CI run** | **27072097535** — conclusion **success** (Android build ✅ + EditMode/PlayMode compile-validation ✅) |
| Earlier UI run | `cdfa056` run 27071839893 — compile-validation **success**, Android build cancelled (superseded by build-3's push) |
| **APK** | `BULWARK.apk` (CI artifact `android-build` id 7457633753), **versionName 0.0.84 / versionCode 84** |
| **APK sha256** | `894ee7fe54458f9a4d3f1485cb229b11ffb4d30fd1d3e167092bf2cd756558ac` (42,276,202 bytes) |
| Provenance | downloaded from the CI run for HEAD; HEAD tree confirmed to contain the new UI files |

## 2. Device
| Field | Value |
|---|---|
| Model | **Xiaomi 22095RA98C = Redmi Note 11R** |
| OS | Android **13** (SDK 33), ABI arm64-v8a |
| Install | clean uninstall + install of the CI APK; firstInstall=lastUpdate 2026-06-06 23:32:47 |
| Engine | Unity **6000.0.75f1**, IL2CPP, Release, locale tr-TR |

## 3. Repository / CI state (workflow steps 1–3)
- The **UI implementation** was committed + pushed to `main` (`cabb94c → cdfa056`); the user then pushed
  **`d811179` (build-3)** on top. `origin/main == d811179` (verified).
- **GATE-1 balance commits are present** (`52e1504`, `cabb94c` build-2, `d811179` build-3) — all on HEAD.
- **CI GREEN achieved** on HEAD `d811179` (run 27072097535, both jobs success). Boot from the resulting APK.
- *Note:* my `cdfa056` run's Android-build job was **cancelled by concurrency** when build-3 was pushed ~11 min
  later; its compile-validation had already passed (UI compiles). The green HEAD build supersedes it.

## 4. Boot
| Item | Result |
|---|---|
| Cold launch | `am start -W` → **LaunchState COLD, TotalTime 459 ms, WaitTime 462 ms** |
| Orientation | **Landscape-locked** (surface 2408×1080; portrait disabled) ✅ |
| Splash | ✅ renders — "BULWARK" wordmark in frame, "IRON PACT · ASHEN HORDE", TAP TO BEGIN (`10_boot_splash.png`) |
| Asset load | `[ASSETS] placeholder sprites ready: 16/16 loaded` ✅ |
| Shell seam | `[UI] SplashScreen (WP-01) booted via UiRouter` + `[UI] screen = Menu` (legacy splash suppressed) ✅ |
| Login | ✅ renders — WELCOME WARRIOR, PLAY AS GUEST, social placeholders, ToS (`02`, `13`) |
| Loading | flow worked (Login→Loading→Menu); no isolated clean screenshot captured |
| Crashes | **none** — no FATAL/AndroidRuntime/SIGSEGV for the app; only unrelated MIUI system noise |

## 5. Meta flow (UI validation)
**8 shell screens verified rendering correctly on device, landscape:**

| Screen | Result | Evidence |
|---|---|---|
| Splash | ✅ | `10_boot_splash.png` |
| Login / Auth | ✅ | `02_current.png`, `13_store.png` |
| Main Menu | ✅ currency chips (Gems 1726 / Gold 48570), PLAY/CAMPAIGN/ONLINE/CHESTS/STORE, rail, bottom bar | `03_after_guest.png` |
| Mode Select | ✅ 5 cards (Classic/Tournament/Endless = PLAY; Campaign/Online = SOON), Commander | `04_modeselect.png` |
| Commander Select | ✅ Warden vs Warchief, abilities, levels, SELECT | `05_commander.png` |
| Quests | ✅ Daily/Weekly, 5 quest rows + progress + CLAIM | `11_quests.png` |
| Settings | ✅ tabs, real SOUND toggle, placeholder volume, toggles, Logout/Privacy/Reset | `12_settings.png` |
| Battle HUD (landscape) | ✅ gold, both statue-HP bars, 6 train buttons + ADVANCE, **pause "II" overlay** | `01_splash.png`, `14_match_start.png` |

**Not cleanly screenshotted (honest gap):** **Store, Battle Pass, Match Intro, Victory/Defeat.** Reason: a
human was using the device concurrently and the app **cold-relaunched ~5×** during the session, so two Store
navigation attempts landed on the freshly-relaunched **Login** screen (`06`, `13`) instead of Store. These
screens are CI-compiled (green) and built on the same verified `UiScreen`/`UiWidgets` pattern, and the
**EndScreen path is confirmed in logcat** (`MatchPresentation:OnContinue()` fired → an end screen's CONTINUE
was used), but they lack a direct screenshot this session. Recommend a follow-up capture on a free device.

**Currency display:** Gold + Gems chips render with stub values (frozen model; no Energy). ✅

## 6. Match flow (build-3 gameplay)
- Match start via the shell: `[UI] StartMatch mode=CLASSIC` → `MatchPresentation:Begin()` → `[GATE1] MATCH 1
  START` ✅ (the new UI drives the existing battle through the documented seam).
- **Battle HUD** (landscape) + **in-match Pause overlay** render and the match runs (`14_match_start.png`).
- Audio: menu/battle music + click are invoked in code; not independently audio-captured.
- VFX / animations: the sprite battlefield renders (`[PROXY] proxies=4 … camera configured`); no per-frame
  VFX assessment captured.

## 7. GATE-1 validation (does build-3 change the verdict?)
**Longest uninterrupted match observed: ~156 s** (read-only `[GATE1]`/`[SIMPROOF]` telemetry):

| Dimension | Result |
|---|---|
| Economy | ✅ alive — miners 3/side, mines occupied, gold cycling, units produced |
| Army | ✅ ~4–5 units/side sustained |
| **Statue pressure** | ◑ **partial + asymmetric** — player statue **1000 → 813** then **plateaus at 813 for 50+ s**; enemy statue **1000 (zero damage)** all session |
| Combat | ◑ minimal (Engaged peaked 1) |
| **Resolution** | ❌ **NONE** — `out=Ongoing` for **all ~995+ samples**; **0 Victory, 0 Defeat** |
| Outcome variety | ❌ none |

**Updated GATE-1 verdict: still FAIL — matches do not resolve.** build-3 is a **real improvement over
build-2** (statue damage now occurs at all, vs the pure 1000/1000 stalemate), but a **new equilibrium forms**
(the player statue is dented to 813 then protected; the enemy statue is never touched) and **no Victory/Defeat
is reached within 156 s**. *Caveat:* full uninterrupted runs were limited by ~5 concurrent cold relaunches;
the 156 s window is past the prior 135 s benchmark and was plateaued (no trend toward resolution).

## 8. Rematch / "one-battle-per-launch" blocker
**Status: UNCHANGED (documented; not refuted on device).** Every match logged was `[GATE1] MATCH 1 START` of
a fresh launch (~5 cold launches); **no in-session `MATCH 2`** was observed, so an in-session rematch was not
independently exercised this session. Code-level (WP-13): a true rematch needs a sim **world-reset**
(`MatchState→Ongoing`, respawn) that does not exist; `MatchPresentation.OnRetry` degrades to return-to-menu.
This is a **gameplay** dependency, out of UI scope.

## 9. Remaining blockers
| # | Blocker | Type | Owner |
|---|---|---|---|
| 1 | **GATE-1 (fun): matches do not resolve** (build-3 dents one statue then plateaus; no outcome) | gameplay/balance | GATE-1 owners (build-4+) |
| 2 | **Rematch** needs an ECS world-reset (one battle per launch) | gameplay | gameplay owners |
| 3 | Live backend (store/persistence/leaderboard) compile-gated off | backend (GATE-3) | backend |
| 4 | Clean screenshots of Store / Battle Pass / Match Intro / Victory-Defeat (device was concurrently in use) | validation | follow-up capture |
| 5 | Placeholder/ripped art + "Stick" asset sources still installed; final BULWARK art pending | art/legal | art |

## 10. Recommendation
> **UI layer: VALIDATED / READY.** The landscape UI shell compiles in CI (green), installs, cold-boots in
> ~0.46 s, is landscape-locked, renders correctly across 8 verified screens, drives the existing battle + HUD +
> pause through the §12 seam, and ran with **no app crashes**. No gameplay/ECS/balance code was changed by the
> UI work.
>
> **Overall: `BLOCKED`** — on **GATE-1 (fun)**, the master gate: build-3 did **not** make matches resolve
> (no Victory/Defeat in a 156 s match; asymmetric statue dent then plateau). The next gate cannot be entered
> until matches resolve with outcome variety. This is a **gameplay/balance** blocker (build-4+), **not** a UI
> blocker.

**Evidence:** `runtime/device_validation/final_validation/` — `full_logcat.txt` (135,471 lines),
`01..15_*.png` (12 screenshots), `apk_info.txt`, `launch_metrics.txt`, `gate1_metrics.txt`,
`rematch_validation.txt`, `ci_poll.log`.
