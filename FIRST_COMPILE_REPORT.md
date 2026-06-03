# BULWARK — First Compile Report

**Date:** 2026-06-03 · **Milestone:** ✅ **First evidence-based COMPILE PASS for Phases 0–3** (Unity
6000.0.75f1 in CI). Build/tests fail **post-compile** on Unity-editor project configuration (not code).
**Authority (binding, unchanged):** roadmap / changelog / decision-log; ADR-0-001/-0-002/-1-001/-2-001/-2-002.
No roadmap/canon/feature/monetization changes. No gate marked PASS without CI evidence.

**Evidence run:** `main` run **26884262912** (sha `363076e`) — https://github.com/emredogan-cloud/Bulkwark/actions/runs/26884262912

---

## 1. Unity activation result — ✅ PASS
After `UNITY_LICENSE` was provisioned, game-ci activates cleanly: `Activation complete. / Activation successful`
on both jobs (Unity 6000.0.75f1). The earlier blockers (empty license; deprecated activation workflow) are resolved.

## 2. Compile result — ✅ PASS (stable, 0 errors)
The full Phase 0–3 C# codebase **compiles clean** in CI — **0 errors across all codes** (CS, SGSG source-generator,
Burst). Verified: the Entities IL post-processors + Burst IL post-processor ran over `Bulwark.Sim` and the dependent
assemblies successfully. This was reached over several CI iterations, each fixing a category (see §6).

## 3. Build result — ❌ FAIL (post-compile; Unity-editor project config, not code)
The Android `unity-builder` job **compiles and Burst-AOT/Entities-IL-post-processes the sim assemblies successfully**,
then fails at **player-build prep** (exit 101). Root causes are all **missing Unity-editor project configuration**
(the repo tracks no scenes / URP-global-settings / Addressables config — these are editor-generated):
- `SystemException: Entities currently cannot build a Scene that has not yet been saved for the first time.`
- Addressables `BuildPlayerContent` runs with no Addressables groups configured.
- URP global settings: `RenderPipelineGlobalSettingsUtils.TryEnsure<UniversalRenderPipelineGlobalSettings>` (asset absent).
- Android SDK: `Failed to find package 'platforms;android-'` (no Android target API level set).
**The APK artifact was not produced.** None of these are compile or code defects — they are the one-time
Unity-editor setup long flagged as DEFERRED across the phase reports.

## 4. Test result — ❌ FAIL (post-compile; runner aborted at project setup)
The `unity-test-runner` job compiles clean, then the runner exits non-zero during **setup** (same URP-global-settings /
unsaved-scene prep path as the build). The authored EditMode `ConfigResolver` tests **compiled** but **did not execute**
(the runner aborted before running tests), so no pass/fail results were produced. This is the same editor-config blocker,
not a test logic failure.

## 5. Artifact result — ✅ PARTIAL PASS
The publish steps work: **`build-logs`, `test-logs`, and `test-results` artifacts uploaded successfully** every run
(evidence in the run). The **Android `.apk` artifact was not produced** (build failed before the APK stage).

## 6. Compile errors fixed (the full iteration log)
| Round (sha) | Category | Fix |
|---|---|---|
| c60d562 / 3f53710 | **Build-enabling config** | Removed unresolvable `com.unity.entities.graphics@1.3.5` (DOTS rendering bridge, unused); bumped Unity `6000.0.23f1 → 6000.0.75f1` (game-ci has no Docker image for .23f1); added a **free-disk-space** step (the ~10 GB Android editor image exhausted runner disk → docker exit 125). |
| c60d562 | **CS0308** (×12→1 unique) | `UnitAuthoring`'s nested baker class was named `Baker`, shadowing `Unity.Entities.Baker<T>` so `Baker<UnitAuthoring>` resolved to itself → renamed to `UnitAuthoringBaker`. |
| 0bcb0ef | **CS0246** | After the rename, `Baker<>` couldn't be found — the baking/authoring types live in `Unity.Entities.Hybrid`, which `Bulwark.Sim.asmdef` didn't reference → added the reference. |
| 2a8077e | **CS0120** (×19) | `SystemAPI` called from **static** helpers (the source generator injects *instance* `__TypeHandle`/`__query` fields) in Combat/CommanderAbility/Spell → switched to `state.EntityManager.*` (kept static) or converted helpers to instance methods. |
| 2a8077e | **CS0029** (×1) | `MatchFlow`: `SystemState.DebugName` (`NativeText.ReadOnly`) → `FixedString128Bytes` via `CopyFromTruncated(.ToString())`. |
| 2a8077e | **CS1654** (×1) | `Spell`: mutating a `foreach` iteration variable → indexed/mutable-local rewrite (DynamicBuffer is a handle struct). |
| 363076e | **SGSG0002** (×6) | `Training`/`SquadAI` helpers used `SystemAPI.*` without a `ref SystemState` parameter (the generator needs it to update query handles) → threaded `ref SystemState state` through the methods + callers. |
| **363076e (final)** | — | **0 compile errors (all codes). Compile STABLE.** |

Notably, the bulk of the hand-authored ECS sim, services, modes, and bootstrap compiled without changes — the
fixes were concentrated in a handful of authoring/source-generator idioms.

## 7. Remaining blockers (all post-compile — one-time Unity-editor setup)
1. **URP Global Settings asset** missing → create via the editor (Graphics settings auto-generates it).
2. **At least one saved Scene in Build Settings** (Entities cannot build an unsaved scene); wire the ECS battle
   Scene/SubScene around `BattleBootstrap` (the DEFERRED step from Phases 1–3).
3. **Addressables** configured (groups/settings) or disable build-addressables-on-player-build — the project pins
   Addressables but has no config.
4. **Android Player Settings + SDK target API level** (`platforms;android-` shows none set).
These require opening the project once in **Unity 6 (6000.0.75f1)**, which generates global settings, lets scenes be
saved, and exposes Addressables/Android settings. They are project configuration, **not** code/compile work.

## 8. Updated gate status
| Item | Status | Evidence |
|---|---|---|
| Unity activation | **PASS** | "Activation successful" (run 26884262912) |
| **Compile (Phases 0–3)** | **PASS** (stable, 0 errors) | run 26884262912 — 0 errors all codes; EILPP + Burst ran |
| Android build / APK | **FAIL** (post-compile config) → DEFERRED | exit 101: unsaved-scene / Addressables / URP-global-settings / Android SDK |
| EditMode/PlayMode tests | **DID NOT EXECUTE** (runner aborted at setup) → DEFERRED | same project-config cause |
| Artifact generation | **PASS** (logs/test-results); APK not produced | artifacts uploaded each run |
| GATE 1 (FUN) / GATE 2 / GATE 3 | OPEN / DEFERRED / DEFERRED (unchanged) | need a configured, playable build + playtest + live backend |

### Validation debt
- **Burned down:** the Phase 0–3 **compile** debt — the codebase now compiles in CI (first real evidence).
- **Remaining:** the one-time Unity-editor project configuration (§7) to unblock the Android build + test execution;
  then the gameplay/runtime gates (GATE 1 fun, GATE 2 playtest, GATE 3 server-validated economy) and the PlayFab
  live integration (SDK + CloudScript/Title Data deploy) remain DEFERRED as before.

---

**STOPPING per instruction** (compile stable + report generated). No Phase 4 work; no feature/monetization/canon/roadmap
changes. Next, opening the project once in Unity 6 to apply the §7 editor configuration converts the Android build +
test execution from DEFERRED into evidence-bearing results.
