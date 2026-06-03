# BULWARK — Unity Validation Report

**Date:** 2026-06-03 · **Result:** ✅ **First successful end-to-end Unity validation — pipeline GREEN.**
Compile clean, all tests pass, **Android APK built**, all artifacts produced.
**Authority (unchanged):** roadmap / changelog / decision-log; ADR-0-001/-0-002/-1-001/-2-001/-2-002.
No roadmap/canon/feature/monetization/gameplay changes. No gate marked PASS without CI evidence.

**Green run:** `main` **26901181289** (sha `9a487da`) — https://github.com/emredogan-cloud/Bulkwark/actions/runs/26901181289

---

## 1. CI runs executed (this validation session)
| Run | sha | Result | What it showed / fixed |
|---|---|---|---|
| 26896532536 | `1b1b12b` (owner editor config) | failure | Compile clean; **EditMode tests pass 3/3**; build reached `BuildResult`. Surfaced: Addressables YAML parse error + Android SDK levels. |
| 26897465806 | `b74afee` | failure | **Android build SUCCEEDED → APK**; tests pass (EditMode 3/3+1/1, **PlayMode 4/4**). Test job red only on a check-run permission error. |
| 26900399545 | `d9db777` | failure | Android success + **38 MB APK artifact**; test job red on **docker exit 125** (disk exhaustion at image pull). |
| **26901181289** | **`9a487da`** | **✅ success** | **Both jobs green.** Tests success, Android build success, all artifacts (APK + test-results + logs). |

## 2. Issues found → fixed (categorized)
| # | Category | Issue (evidence) | Fix | Commit |
|---|---|---|---|---|
| 1 | **Addressables** / Unity-config | `Unable to parse Assets/_Game/Data/Balance/{CounterMatrix,IronPactBalance}.asset [Expect ':' ...]` — hand-authored `.asset` files had inline `#` comments (invalid in Unity YAML) → Addressables `BuildPlayerContent` failed. | Stripped all `#` comments; the 20 `counterMatrix` values unchanged. | `b74afee` |
| 2 | **Android build** / Unity-config | `CommandInvokationFailure: Unable to install … 'platforms;android-37' / 'android-0'` — `AndroidMinSdkVersion: 37` (too high), `AndroidTargetSdkVersion: 0` (Auto). | Set `min=24`, `target=34` (Play-compliant, Unity-6-bundled). | `b74afee` |
| 3 | **CI/CD** | Test job: `##[error]Resource not accessible by integration` — `game-ci/unity-test-runner` publishing a check-run via `GITHUB_TOKEN` without `checks:write`. | Added a workflow `permissions` block (`contents:read`, `checks:write`) + dropped the optional `githubToken` (results still upload as the `test-results` artifact). | `d9db777` |
| 4 | **CI/CD** | Test job: `docker … exit code 125` — disk exhaustion at the Unity image pull (the now-large `Library` cache + image overran the runner disk); the test job lacked a free-disk step (only the build job had it). | Added the same free-disk-space step to the test job. | `9a487da` |

**By the owner's taxonomy:** compile — none; **Unity configuration** — #1, #2; **Android build** — #2; **Addressables** — #1; **URP** — none (the owner's URP global settings resolved it); **ECS** — none (Burst AOT + Entities IL post-processing succeeded); **CI/CD** — #3, #4; **PlayFab** — none in-pipeline (adapters are excluded behind the `PLAYFAB_SDK` define); **test execution** — none (tests passed; the job-level reds were CI/CD, not tests).

## 3. Issues remaining
- **None blocking the pipeline** — it is green end-to-end.
- **Non-blocking / informational:** the game-ci/`actions/*` steps emit a **Node.js 20 deprecation warning** (GitHub will force Node 24 by 2026-06-16; bump action versions before then). The **PlayFab live integration** (SDK install + `PLAYFAB_SDK` define + CloudScript/Title Data deploy) remains **DEFERRED** — it is intentionally excluded from this build and is owner/runtime work, not a pipeline failure.

## 4. Compile status — ✅ PASS
0 errors across all codes (CS / source-generator SGSG / Burst). Entities IL post-processing + Burst AOT ran over all sim assemblies.

## 5. Android build status — ✅ PASS
Full IL2CPP arm64 build → `libil2cpp.so` → Gradle → **`launcher-release.apk`**. Published as the **`android-build`** artifact (**37,850,180 bytes ≈ 38 MB**).

## 6. Test status — ✅ PASS
- **EditMode:** `Bulwark.Services.Tests.ConfigResolverTests` **3/3 passed**; Addressables doc-stub **1/1 passed**.
- **PlayMode:** **4/4 passed** — *"Run succeeded, no failures occurred."*
- Results published as the **`test-results`** artifact.

## 7. Artifact status — ✅ PASS (all produced)
| Artifact | Size | Contents |
|---|---|---|
| `android-build` | 37,850,180 B | the release APK |
| `test-results` | 113,320 B | EditMode + PlayMode NUnit XML |
| `test-logs` | 111,491 B | Unity test logs |
| `build-logs` | 49,500 B | Android build logs |

## 8. Updated gate status
| Item | Status | Evidence |
|---|---|---|
| Unity activation | **PASS** | run 26901181289 |
| Compile (Phases 0–3) | **PASS** | 0 errors |
| EditMode + PlayMode tests | **PASS** | 4/4 + 4 (job success) |
| **Android build / APK** | **PASS** | `android-build` 38 MB |
| Artifact generation | **PASS** | 4 artifacts |
| **CI/CD pipeline (end-to-end)** | **✅ GREEN** | run conclusion = success |
| GATE 1 (FUN) | **OPEN** | the APK builds, but the game isn't yet wired into a playable scene with content (BattleBootstrap needs inspector refs + a populated scene); no on-device fun verdict |
| GATE 2 / GATE 3 | **DEFERRED** | external playtest / server-validated economy |
| PlayFab live integration | **DEFERRED** | SDK + CloudScript/Title Data deploy (owner/runtime) |

## 9. Recommendation
The **CI/CD validation infrastructure is fully operational**: every push compiles, tests, and builds an installable Android APK with published artifacts — the first evidence-based end-to-end Unity validation. Recommended next steps (each is owner-approved, non–Phase-4 work):
1. **Make the APK playable** — wire `BattleBootstrap` into `MainScene` (assign the `UnitDef[]`/`SpellDef[]`/`CommanderDef`/`MapDef`/`BalanceConfig` inspector refs + a Camera, set up the SubScene). This is the remaining step before the **GATE 1 fun verdict** is possible.
2. **Deploy PlayFab** — install the SDK + add the `PLAYFAB_SDK` define + upload `economy.js` CloudScript + Title Data, to enable the **server-economy validation** (GATE 3 path).
3. **CI hygiene** — bump `game-ci`/`actions/*` to Node-24-compatible versions before 2026-06-16.
4. Then, **with explicit approval**, proceed to the deferred gameplay gates (GATE 1 → 2 → 3). **No Phase 4 (monetization) until GATE 3.**

---

**STOPPING per instruction** (first successful end-to-end validation achieved + report generated).
No Phase 4 work started; no roadmap/canon/feature/monetization/gameplay changes.
