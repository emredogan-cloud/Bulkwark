# BULWARK — Infrastructure Bring-Up Report V2

**Date:** 2026-06-03 · **Outcome:** Steps 1–3 done; **Step 4 executed against live CI** → the
pipeline runs but **fails at Unity license activation** (before compilation). **No gate marked PASS.**
**Authority (binding, unchanged):** roadmap / changelog / decision-log / Phase-3 canon. No canon
changes. ADRs honored: 0-001, 0-002, 1-001, 2-001, 2-002. *(Note: the authorization message cited
"ADR-3-001", which does not exist — the Phase-3 authorization is **ADR-2-001**, plus **ADR-2-002**;
flagged for the record, no action taken.)*

---

## Step 1 — Re-audit (previous blockers cleared)
| Requirement | V1 | V2 | Evidence |
|---|---|---|---|
| Clean repo on GitHub | ❌ not pushed | ✅ **on GitHub** | `origin → github.com/emredogan-cloud/Bulkwark.git`; `origin/main` == local `1481b59`; 0/0 divergence. |
| Repo reachable / pushable | ❌ no remote, gh absent | ✅ **reachable + pushable** | github.com/api → HTTP 200; stored credential valid; `git push` succeeded (`97cf00d..1481b59`). |
| Secrets present (CI) | ❌ | ✅ (owner-provisioned) | `UNITY_EMAIL`/`UNITY_PASSWORD`/`PLAYFAB_SECRET_KEY` referenced by CI; **`UNITY_LICENSE` is empty** (see Step 4). |
| GitHub Actions | ❌ blocked | ✅ **runs** | 2 workflow runs executed (evidence below). |
| PlayFab `101A1B` reachable | ⚠️ 401 | ⚠️ 401 (unchanged) | unauth probe → HTTP 401 (reachable; not auth-validated). |
| Local Unity toolchain | ❌ | ❌ (unchanged) | no `unity`/`dotnet` locally — build runs in CI, not here. |

**Step 1 verdict:** the V1 primary blocker (code not on GitHub) is **cleared** → proceeded to Steps 2–4.

## Step 2 — PlayFab backend integration — ✅ AUTHORED (uncompiled)
Implemented behind a `PLAYFAB_SDK` define-constrained asmdef (excluded from the build until the
SDK is installed — cannot break baseline CI):
- **`PlayFabBackendClient`** (`IBackendClient`): device auth + **server-owned reads only** (Virtual
  Currency wallet, read-only progression, Title Data config). `WriteProfileAsync` returns `false`
  **by design** — the client cannot write authoritative state.
- **`PlayFabServerEconomy`** (`IServerEconomy` + `ICommanderEconomy`): every spend/grant/upgrade/
  commander-level op is a **CloudScript call**; the client sends intent and adopts only the
  server-confirmed result; a read-only cache backs `CachedBalance`.
- **`backend/playfab/cloudscript/economy.js`** (the authority): re-derives costs from server-owned
  Title Data (**anti-tamper**), re-enforces **hard caps** (no P2W), optimistic concurrency on level,
  single-claim diminishing first-clear grants. Handlers: `spendCurrency`/`grantCurrency`/
  `spendForUpgrade`/`spendForCommanderLevel`.
- **`backend/playfab/README.md`**: SDK install, `PLAYFAB_SDK` define, VC codes (`SL`/`GM`/`PX`;
  Gold excluded), Title Data upload, CloudScript deploy.
**Preserved:** server-authoritative wallet, server-side validation, cap enforcement, anti-tamper,
grant validation, upgrade validation — **no client-authoritative shortcuts**. **Not compiled here.**

## Step 3 — CI/CD — ✅ CREATED & LIVE
`.github/workflows/main.yml` (game-ci `unity-test-runner` + `unity-builder`): compile-on-push,
EditMode+PlayMode tests, Android build, publish artifacts + compile logs + test results. The
superseded Phase-0 `ci.yml` was removed (avoids dual Unity-license activation). The workflow
**triggered, ran, and published artifacts** — it is correctly wired.

## Step 4 — Validation (executed against live CI; honest results)
| Validation | Result | Evidence |
|---|---|---|
| **CI pipeline status** | ✅ **LIVE** (currently red) | 2 runs executed; `main` run = [run 26879097560](https://github.com/emredogan-cloud/Bulkwark/actions/runs/26879097560). Both jobs reached the Unity step; artifacts/logs/test-results steps succeeded. |
| **Compile validation** | ⛔ **NOT REACHED** (UNVERIFIED) | CI failed at activation **before compiling**: `##[error]Missing Unity License File and no Serial was found.` (`UNITY_LICENSE` empty). **No compile evidence exists yet** — the blocker is the license, not the code. |
| **Android build** | ❌ **NOT PRODUCED** | `unity-builder` failed at the same activation step. |
| **PlayFab connectivity** | ⚠️ **reachable only** | unauth probe → HTTP 401. |
| **Authenticated PlayFab access** | ⛔ **DEFERRED** | needs the SDK installed + a CI/runtime step using `PLAYFAB_SECRET_KEY`; the build never ran. |
| **Economy authority / server grant validation** | ⛔ **DEFERRED** | needs CloudScript + Title Data deployed to Title `101A1B` and a live call; not exercised. |

**No gate marked PASS.** The single, precise thing standing between us and real compile/build/test
evidence is the **Unity license secret**.

## Step 5 — Status Summary
| Area | Status |
|---|---|
| **PlayFab integration** | Adapters + CloudScript **authored** (server-authoritative, uncompiled); connectivity reachable (401); auth/economy validation **DEFERRED**. |
| **CI/CD** | **LIVE** — runs on push, publishes artifacts/logs/tests; **red** at Unity activation. |
| **Build** | **Not produced** (activation blocker). |
| **Compile** | **Unverified** — CI never reached compilation; still no first-compile evidence. |
| **Validation results** | CI-execution: PASS (it runs). Unity activation: FAIL (empty `UNITY_LICENSE`). Compile/build/test/PlayFab-auth: not yet obtained. |

### Remaining blockers (ranked, actionable)
1. **[PRIMARY] `UNITY_LICENSE` (or `UNITY_SERIAL`) is empty.** game-ci needs, for a **personal**
   license, the `.ulf` activation-file contents in the `UNITY_LICENSE` secret (one-time: request an
   `.alf` via `game-ci/unity-request-activation-file`, activate at id.unity.com → `.ulf` → paste into
   `UNITY_LICENSE`); for **Plus/Pro**, set `UNITY_SERIAL` (+ the existing email/password). Email +
   password **alone** do not activate. → Until set, **no compile/build/test evidence is possible.**
2. **PlayFab live validation prerequisites:** install the PlayFab Unity SDK + add the `PLAYFAB_SDK`
   define; create VCs `SL`/`GM`/`PX`; upload `UpgradesConfig`/`EconomyConfig` to Title Data; deploy
   `economy.js` CloudScript. (Per `backend/playfab/README.md`.) Then authenticated PlayFab + server
   grant/upgrade validation become runnable.
3. **No local Unity** (unchanged) — all build/compile validation is CI-side by design.

### Updated gate status
- **GATE 1 (FUN):** OPEN. **GATE 2 (depth):** DEFERRED. **GATE 3 (server-validated MVP):** DEFERRED.
- **Infrastructure:** CI/CD **established and executing**; **compile/build PASS not yet earned** (license blocker). No gate transitions; nothing marked PASS.

### Validation debt status (what each blocker unlocks)
- **Set `UNITY_LICENSE`/`UNITY_SERIAL`** → unlocks: compile validation, Android build artifact, EditMode/PlayMode test results (the first real compile feedback for Phases 0–3; expect iterative compile fixups).
- **Deploy PlayFab SDK + CloudScript + Title Data** → unlocks: authenticated PlayFab access, economy authority, server grant/upgrade validation against Title `101A1B`.
- Still independently DEFERRED (need a built, playable client): GATE 1 fun verdict, GATE 2 playtest, GATE 3 server-validated-MVP runtime, perf, FormationMember runtime.

---

**STOPPING per instruction.** No Phase 4 work started. No roadmap/canon/monetization/feature changes.
Provide `UNITY_LICENSE`/`UNITY_SERIAL` (and, for economy validation, deploy the PlayFab SDK +
CloudScript + Title Data) and I will re-run the bring-up to convert the deferred compile/build/test
and PlayFab validations into evidence-bearing PASS/FAIL results.
