# BULWARK — Infrastructure Bring-Up Report

**Date:** 2026-06-03 · **Outcome:** ⛔ **BLOCKED at Step 1 (Infrastructure Audit) — STOPPED per the gate.**
**Authority (binding, unchanged):** `BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`, `ROADMAP_CHANGELOG.md`,
`PRODUCTION_DECISION_LOG.md`, Phase-3 canon. No canon changes were made. No gate is marked PASS.

> This serves as the **Infrastructure Blocker Report** required by Step 1 *and* the
> `INFRASTRUCTURE_BRINGUP_REPORT.md` deliverable named in Step 5. Steps 2–4 were **not executed**:
> Step 1 found missing requirements and instructed "Do not continue." Nothing was faked.

---

## Why this stopped (one sentence)
The provisioned infrastructure (PlayFab title, GitHub Secrets, Unity creds) lives in **GitHub's
execution context**, but the BULWARK code lives **only in a local repository (`bulwark-clean`) that
has never been pushed to GitHub** — and this authoring environment has **no Unity toolchain and no
access to the GitHub-scoped secrets** — so none of the bring-up validations (CI build, authenticated
PlayFab, server-auth economy) can be executed from here, and the repo cannot be pushed from here
(`gh` absent, no remote, no push credentials).

---

## Step 1 — Infrastructure Audit (evidence-based)

| Requirement | Result | Evidence |
|---|---|---|
| **Existing repo build structure** | ✅ **PRESENT** | `ProjectSettings/ProjectVersion.txt` = Unity **6000.0.23f1**; `Packages/manifest.json` pins Entities 1.3.5 / URP 17.0.3 / Addressables 2.3.1; 7 asmdefs; `.github/workflows/ci.yml` present. (Structure exists; **not compiled/built** here.) |
| **PlayFab connectivity** | ⚠️ **REACHABLE, NOT AUTH-VALIDATED** | Unauthenticated probe `POST https://101A1B.playfabapi.com/Client/GetTitleData` → **HTTP 401**. Proves outbound network + the title endpoint responds; it is **not** an authenticated validation (login/economy) — those need the secret, which is correctly **not** available locally. |
| **Title ID accessibility (101A1B)** | ⚠️ **ENDPOINT RESOLVES, NOT AUTH-CONFIRMED** | Same probe — the `101A1B.playfabapi.com` API responded (401), so the title's API is reachable; title config/economy is **unconfirmed** without an authenticated call. |
| **Secret availability** | ⚠️ **CI-only (correct), not local** | `PLAYFAB_SECRET_KEY` / `UNITY_EMAIL` / `UNITY_PASSWORD` are **not** in the local env (verified, values not printed). This is correct — they belong in GitHub Secrets, available only to Actions runners. **No local validation can use them.** |
| **GitHub Actions availability** | ❌ **BLOCKED** | `bulwark-clean` has **no git remote** (4 commits, local-only `main`); **`gh` CLI absent**. Actions cannot run on a repo that isn't on GitHub. |
| **Unity build capability** | ❌ **ABSENT locally** | No `unity` binary, no Unity install dirs, no `dotnet`. A local build is impossible; the intended path is **game-ci in Actions**, which is blocked on the push above. |

**Gate result:** requirements are **missing** for executing the bring-up validations → **STOP** (Step 1).

---

## Step 2 — Backend Integration (PlayFab) — ⏸ NOT STARTED (gated by Step 1)
The PlayFab adapters for `IBackendClient` + `IServerEconomy` (preserving server-authoritative wallet,
server-side validation, cap enforcement, anti-tamper, grant/upgrade validation — no client-authoritative
shortcuts) are **authorable offline** and ready to be written **once the repo is on GitHub** (so the
adapter can be CI-compiled and exercised against title `101A1B`). Not authored in this turn because the
Step-1 gate said "Do not continue." **The seams already exist** (`IServerEconomy`/`IBackendClient` from
Phase 0/3) — the PlayFab implementation slots in behind them with no canon change.

## Step 3 — CI/CD (`.github/workflows/main.yml`) — ⏸ NOT STARTED (gated)
The game-ci `unity-builder` workflow (compile-on-push → Android build → publish artifact + compile logs
+ test results) is **authorable offline**, but it is **inert until the repo is on GitHub** with the
Unity/PlayFab secrets. There is an existing `ci.yml` (test + Android build via game-ci) authored in
Phase 0; `main.yml` would supersede/extend it. Not authored this turn (gated).

## Step 4 — Validation — ⏸ NOT POSSIBLE in this environment
- **Compile validation:** ❌ no local Unity/dotnet; needs CI (blocked on push).
- **PlayFab connectivity:** ⚠️ network reachability only (401 probe); **authenticated** validation needs CI + secret.
- **Economy authority / server grant validation:** ❌ needs the live title + secret in CI; cannot run locally.
None executed; **none marked PASS** (no evidence available).

---

## Step 5 — Status Summary

| Area | Status |
|---|---|
| **PlayFab** | Network-reachable (HTTP 401 to `101A1B.playfabapi.com`); **authenticated/economy validation: DEFERRED** (no local secret, by design). |
| **CI/CD** | **BLOCKED** — `bulwark-clean` not on GitHub; `gh` absent. Workflows cannot run. |
| **Build** | **NOT BUILT** — no local Unity toolchain; CI build blocked on push. |
| **Test** | **NOT RUN** — same. |
| **Compile** | **UNVERIFIED** — all C# remains authored-not-compiled (Phases 0–3). |

### Remaining blockers (ranked)
1. **[PRIMARY] The clean repo is not on GitHub.** `bulwark-clean` has no remote; `gh` is absent and there
   are no push credentials in this environment, so **I cannot push it from here.** Until it is on GitHub,
   CI, the Unity build, and every secret-backed validation are unreachable.
2. **Probable secret↔repo mismatch.** The secrets were stored in *GitHub Secrets* — almost certainly on the
   pre-existing **`emredogan-cloud/Stick-War-Advanced`** GitHub repo. But that repo's history contains the
   **reverse-engineering IP/PII** this project deliberately kept out (ADR-0-001, R1), and the clean BULWARK
   code (Phases 1–3) was **never pushed anywhere**. The code and the secrets are currently in **different
   places**. An owner decision is required (below).
3. **No local Unity / BaaS runtime.** Local compile/build/authenticated-PlayFab validation is impossible in
   this sandbox by nature; the intended path is GitHub Actions (game-ci) + the live title — both gated by #1.

### Deferred items (unchanged, preserved per the standing ADRs)
- GATE 1 (FUN) — **OPEN**; GATE 2 (depth playtest) — **DEFERRED**; GATE 3 (server-validated MVP) — **DEFERRED**.
- All Phase 0–3 runtime/perf/playtest/server-round-trip validations remain **DEFERRED**.
- FormationMember wiring (plan documented); ADR-2-002 implemented (not runtime-validated).

### Updated gate status
- **Infrastructure bring-up:** **BLOCKED at Step 1.** No new gate transitions. **Nothing marked PASS.**

---

## Required owner decision + remediation (single highest-leverage unblock)
To proceed, the clean BULWARK code must reach a GitHub repo that carries the three secrets, on a Unity-
CI-capable runner. Choose one (consistent with R1 / ADR-0-001 — never publish the RE IP/PII):

- **(Recommended) Fresh clean repo.** Create an **empty GitHub repo** for BULWARK, add the 3 secrets
  (`PLAYFAB_SECRET_KEY`, `UNITY_EMAIL`, `UNITY_PASSWORD`) to **it**, and give me the remote URL (and a
  push path — e.g. a token, or push it yourself). I push `bulwark-clean` (no IP/PII). Then I author the
  PlayFab adapter + `main.yml`, push, and CI runs the deferred validations against title `101A1B`.
- **(Alt) Reuse `Stick-War-Advanced`** only after **purging the RE IP/PII from its history** (filter-repo/BFG)
  and making it private; then push the clean code there (it already holds the secrets). Heavier; risks
  exposure if history isn't fully scrubbed.

Once a clean repo-with-secrets exists, re-run this bring-up: Steps 2–4 become executable and this report is
superseded by an evidence-bearing one (CI green, build artifact, authenticated PlayFab + server-grant validation).

**STOPPING here per Step 1. Did not continue to Steps 2–5. No additional roadmap phase started.**
