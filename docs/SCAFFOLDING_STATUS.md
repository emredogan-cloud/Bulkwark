# Phase 0 scaffolding — status

All code/config under `Assets/`, `Packages/`, `ProjectSettings/`, and
`.github/workflows/` was **authored** during Phase 0 but **not compiled, opened, or
run** — this environment has no Unity 6 editor, Android build chain, test device, or
BaaS project (see `docs/adr/ADR-0-001-environment-and-ip-blockers.md`).

What that means:
- It is a faithful, minimal Unity 6 / DOTS skeleton intended to open and build in a
  real Unity 6 LTS install, but first-compile fixups (package version resolves,
  `.meta` generation, namespace nits) are expected and normal.
- The 4 Phase-0 validation gates are reported **BLOCKED**, not PASS, in
  `reports/phase-0/FINAL_REPORT.md`. Nothing here claims to have run green.
- The pure-C# `ConfigResolver` precedence is unit-tested by construction; everything
  requiring the editor/device/backend awaits the tooling called out in ADR-0-001.

Do **not** interpret the presence of these files as a passing build.
