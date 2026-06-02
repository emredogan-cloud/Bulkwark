# ADR-0-001 — Phase 0 environment & repository blockers

- **Status:** Open (escalated to Technical Architect & Game Director, roadmap §16)
- **Date:** 2026-06-02
- **Phase:** 0 (Foundation & Canon Lock)
- **Author:** Build agent (Claude CLI)
- **Decision needed from:** repo owner / Technical Architect

## Context

Phase 0 (per `docs/execution/PHASE_0_MASTER_EXECUTION_PROMPT.md` and roadmap §13)
requires a building Unity 6 project (0.1), an ECS sim spike proven at **≥200 units
on a mid-range phone** (0.2), a designer-editable data + 3-tier resolver pipeline
proven in-editor (0.3), and a BaaS auth/profile/config/analytics **round-trip** (0.4).
The roadmap mandates **evidence-first reporting** and forbids faked success: a gate
is PASS only with measured evidence (§0/§15).

Two hard blockers were found in this execution environment.

### Blocker A — no Unity/build/device/BaaS toolchain
This environment has **no Unity 6 editor, no Android build chain, no mid-range
device, and no BaaS project/credentials**. Therefore:
- 0.1 CI cannot be observed green (needs a Unity license on a runner + a push).
- 0.2 frame-time at 200 units cannot be measured (no editor, no device).
- 0.3 cannot be proven in-editor (the pure-C# resolver precedence *is* unit-tested,
  but the "designer SO edit changes live sim" half needs Unity).
- 0.4 cannot round-trip (no backend/credentials).

Per the no-faked-success rule, gates 0.1–0.4 are reported **BLOCKED**, not PASS.
What is deliverable without Unity — the project skeleton, ECS sim core, data
schemas, 3-tier resolver + tests, BaaS/analytics seams, and CI definition — has
been authored and is committed, clearly marked "authored, not compiled."

### Blocker B — reverse-engineering IP/PII already tracked in this git repo
The workspace `~/Downloads/BULWARK` is an existing git repo (`main`, remote
`github.com/emredogan-cloud/Stick-War-Advanced.git`) that **already tracks ~18,800
files of third-party IP and PII**: the Stick War: Legacy **APK + asset packs**, the
**decompiled game** (`dump.cs`, `global-metadata.dat`, `jadx-out/`, `unpacked/`),
and **runtime logs** containing device identifiers, save state, and recovered
Firebase/AdMob credentials. The entire RE track was scoped as **local, educational,
non-redistribution** analysis.

**Pushing this repo as-is would publish copyrighted third-party software + secrets/PII** —
a copyright and privacy violation and a direct breach of project scope. The build
agent therefore **withholds the push** pending an owner decision. `.gitignore` now
excludes all such paths going forward, but it does **not** untrack files already in
history; only a clean repo or a history rewrite removes them from what a push exposes.

## Decision

1. **Do not push the current repository.** (Done — push withheld.)
2. Add `.gitignore` excluding all RE/IP/PII artifacts. (Done.)
3. Author all Unity-independent Phase 0 scaffolding and commit it locally. (Done.)
4. Report gates 0.1–0.4 as **BLOCKED** with honest evidence, not PASS. (Done — see
   `reports/phase-0/FINAL_REPORT.md`.)
5. **Escalate** the two blockers for owner decision before any remote publish.

## Options for the owner (Blocker B remediation)

- **(R1, recommended) Clean successor repo.** Publish BULWARK from a fresh repo /
  orphan history containing only the IP-free forward project (design docs +
  scaffolding). Keep the RE artifacts strictly local. Cleanest; zero IP/PII exposure.
- **(R2) History rewrite + private.** Make the repo private, rewrite history
  (git filter-repo / BFG) to purge APK/decompiled/logs/secrets, rotate any exposed
  keys, then push. Heavier; private still risks accidental exposure.
- **(R3) Status quo.** Do nothing and do not push. RE stays local-only.

## Options for the owner (Blocker A remediation)

- Provide a Unity 6 environment + mid-range device + BaaS project (or a game-ci
  runner with a Unity license) so gates 0.1–0.4 can be executed and re-reported, OR
  accept BLOCKED gates as the Phase 0 exit state and authorize a follow-up validation
  pass once tooling exists.

## Consequences

- Phase 0 exit gate is **BLOCKED**, so **authorization to start Phase 1 is WITHHELD**
  until the gates are executed (Blocker A) — consistent with §13 "gates can kill or
  rescope" and the stop-on-blocker rule.
- No copyrighted material or PII is published. No canon docs were modified.
