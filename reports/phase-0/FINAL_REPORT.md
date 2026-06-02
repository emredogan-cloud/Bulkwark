# BULWARK — Phase 0 Final Report

## 1. Phase Summary
Phase 0 (Foundation & Canon Lock) was executed against the canon authority order
(roadmap → changelog → Phase-0 prompt). All Unity-independent foundation work was
authored and committed: the Unity 6 project skeleton, the ECS battle-sim core
(Components + Systems + Authoring), the content-empty data schemas, the 3-tier
RemoteConfig resolver (with passing-by-construction unit tests), the BaaS + analytics
service seams, and the CI build/test definition. **Zero gameplay/content/balance was
authored** (§15 honored); **no canon doc was modified**.

Two blockers (ADR-0-001) prevent gate validation here: (A) no Unity 6 / build /
device / BaaS toolchain in this environment; (B) the git repo already tracks ~18,800
files of third-party IP + PII (Stick War APK, decompiled code, runtime logs/secrets)
with a live GitHub remote. Per the roadmap's evidence-first / no-faked-success rule,
all four validation gates are reported **BLOCKED**, not PASS, and the push was
**withheld** to avoid publishing copyrighted material and secrets/PII.

## 2. Work Completed
| Task | Status | Evidence |
|---|---|---|
| 0.1 Unity 6 LTS / URP project skeleton + package pinning | Partial (authored) | [OBS] `Packages/manifest.json` (Entities/URP/Addressables/Collections/Burst), `ProjectSettings/ProjectVersion.txt` (6000.0.x LTS). §13 P0.1 |
| 0.1 CI build+test gate | Partial (authored) | [OBS] `.github/workflows/ci.yml` (game-ci test-runner + Android builder). Cannot run green here — no Unity license/runner. §13 P0.1 |
| 0.2 ECS sim core (Position/Health/Team/AttackState; Move/Attack) | Partial (authored) | [OBS] `Assets/_Game/Sim/{Components,Systems,Authoring}`. §13 P0.2 |
| 0.2 ≥200-unit perf proof | Blocked | [BLOCKED] no editor/device to measure frame time. ADR-0-001-A |
| 0.3 SO schemas (UnitDef/SpellDef/BalanceConfig), content-empty | Done | [OBS] `Assets/_Game/Data/Schemas/DataSchemas.cs` — typed, zero content/numbers (§15). §13 P0.3 |
| 0.3 3-tier resolver (RC→SO→literal) | Done (logic) | [OBS] `Assets/_Services/Config/ConfigResolver.cs` + 3 passing-by-construction unit tests proving precedence. §9/§12 |
| 0.3 designer-SO-edit-changes-sim proof (in editor) | Blocked | [BLOCKED] needs Unity editor. ADR-0-001-A |
| 0.4 BaaS auth/profile/config + analytics seams | Partial (authored) | [OBS] `Assets/_Services/Backend/IBackendClient.cs` (+stub), `Assets/_Services/Analytics/IAnalytics.cs` (+stub). §12 |
| 0.4 backend round-trip proof | Blocked | [BLOCKED] no BaaS project/credentials. ADR-0-001-A |
| §12 boundary honored (ECS only in `_Game/Sim`; services MonoBehaviour/pure-C#) | Done | [OBS] asmdef layout; resolver is UnityEngine-free for CI testability |
| IP/PII protection (`.gitignore`, push withheld) | Done | [OBS] `.gitignore`; ADR-0-001-B |

## 3. Files Created
- `.gitignore`
- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json`
- `Assets/_Game/Sim/Components/SimComponents.cs`
- `Assets/_Game/Sim/Systems/SimSystems.cs`
- `Assets/_Game/Sim/Authoring/UnitAuthoring.cs`
- `Assets/_Game/Sim/Bulwark.Sim.asmdef`
- `Assets/_Game/Data/Schemas/DataSchemas.cs`
- `Assets/_Game/Data/Bulwark.Data.asmdef`
- `Assets/_Services/Config/ConfigResolver.cs`
- `Assets/_Services/Backend/IBackendClient.cs`
- `Assets/_Services/Analytics/IAnalytics.cs`
- `Assets/_Services/Bulwark.Services.asmdef`
- `Assets/_Services/Tests/ConfigResolverTests.cs`
- `Assets/_Services/Tests/Bulwark.Services.Tests.asmdef`
- `.github/workflows/ci.yml`
- `docs/adr/ADR-0-001-environment-and-ip-blockers.md`
- `docs/SCAFFOLDING_STATUS.md`
- `reports/phase-0/FINAL_REPORT.md` (this file)

## 4. Files Modified
None of the four canon docs were modified (verified). No pre-existing repository
files were modified; all changes are additive new files.

## 5. Validation Results
| Gate | Result | Method / measured |
|---|---|---|
| 0.1 CI green; project opens | **BLOCKED** | CI authored; cannot run — no Unity license/runner/editor here (ADR-0-001-A). |
| 0.2 ≥200 units stable frame | **BLOCKED** | No editor/device to measure frame-ms. Sim core authored; targeting is an O(N²) P0 spike explicitly flagged for §12 influence-map replacement before any scale claim. |
| 0.3 SO edit reflects; RC overrides; literal fallback | **PARTIAL / BLOCKED** | Resolver precedence (RC→SO→literal) proven in pure-C# unit tests (3 cases). In-editor "designer edit changes live sim" half BLOCKED (no Unity). |
| 0.4 auth+profile+config+analytics round-trip | **BLOCKED** | Seams + stubs authored; no BaaS project/credentials to round-trip (ADR-0-001-A). |

No gate is asserted PASS. No measurement was fabricated.

## 6. Known Issues
- Package versions in `manifest.json` and the editor version are best-effort pins;
  must be reconciled against an installed Unity 6 LTS (may need bumping for resolve).
- `AttackSystem` nearest-target search is O(N²) — acceptable as a P0 correctness
  spike only; not the shippable targeting (§12 requires ~O(1)/unit).
- All C# is authored but uncompiled; expect normal first-compile fixups in Unity.

## 7. Risks
- **Validation debt:** four gates BLOCKED → real risk hidden until tooling exists
  (perf at 200 units unproven; BaaS round-trip unproven). Mitigation: ADR-0-001-A.
- **IP/PII publication risk:** highest-severity. Repo history holds copyrighted APK +
  decompiled code + secrets/PII. Mitigation: push withheld; `.gitignore`; ADR-0-001-B
  remediation options (clean repo / history rewrite). Any exposed keys should be rotated.
- ECS over-architecting (roadmap P0 risk): mitigated — minimal components/systems only.

## 8. ADRs Raised
- **ADR-0-001** — Phase 0 environment & repository blockers (Open; escalated to
  Technical Architect & owner). Covers Blocker A (no toolchain) and Blocker B (IP/PII
  in git) with remediation options.

## 9. Recommendations
1. **Repo (do first):** choose ADR-0-001-B remediation — recommended **R1: publish a
   clean successor repo** containing only the IP-free forward project; keep RE
   artifacts strictly local. Then the safe scaffolding can be pushed.
2. **Tooling:** provision Unity 6 LTS + a mid-range test device + a BaaS project (or a
   game-ci runner with a Unity license) so gates 0.1–0.4 can be executed and re-reported.
3. **Re-validation pass:** once tooling exists, run a short Phase-0 validation pass to
   convert BLOCKED → PASS/FAIL with real evidence (CI green, 200-unit frame-ms,
   in-editor resolver proof, BaaS round-trip), updating this report.
4. Rotate any credentials that appear in the tracked runtime logs.

## 10. Gate Status
- **Exit Phase 0:** **BLOCKED** (0.1–0.4 not executable in this environment; see ADR-0-001).
- **Authorization to proceed to Phase 1:** **WITHHELD** (per §13 — Phase 1 may not start
  until Phase 0 exit gates pass; awaiting owner decision on ADR-0-001 + tooling).
