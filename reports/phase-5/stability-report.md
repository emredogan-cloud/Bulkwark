# BULWARK — Phase 5.3 Perf/Stability Hardening Report (PLAN + TARGETS — no measured data)

**Status:** **PLAN + TARGETS ONLY.** No live/device run → **no crash-free %, ANR %, or frame-time is
measured.** **No fabricated data.** **Authority:** roadmap §12 (perf budget = hard gate every phase; mid-range
phone target; budgeted AI scheduler; particle caps), §13 Phase 5 (5.3 hardening), §13 GATE 6 (crash-free
≥ ~99% — launch-readiness, later); `PHASE_5_MASTER_EXECUTION_PROMPT.md` §F/§G. **No new code authored — bug-fix
patches only at execution time.**

## 1. Targets (canon-derived; pass/fail TBD on real devices)
| Target | Bar (canon) | Measured |
|---|---|---|
| Crash-free sessions | ≥ target (GATE 5 "crash-free + balanced"; GATE 6 ≥ ~99% for global) | **TBD — DEFERRED** |
| ANR / hang rate | within store/platform limits | **TBD — DEFERRED** |
| Frame budget (mid-range tier) | hold the §12 mid-range-phone frame budget (e.g. p95) under crowd load | **TBD — DEFERRED** |
| 200-unit sim frame-ms | the Phase-0 perf-spike target (§13 P0.2) — still DEFERRED since Phase 0 | **TBD — DEFERRED** |
| Memory / load time | within platform budget | **TBD — DEFERRED** |

## 2. Device matrix (to profile at 5.3 — representative, not exhaustive)
Low / mid / high Android tiers (mid-range is the canonical target, §11/§12). Exact device list TBD by QA;
iOS added when an iOS build channel exists. **No device has been profiled (DEFERRED).**

## 3. Method (binding)
- Hardening = **fix crashes/regressions + hit the perf budget**; balance via telemetry (capped/normalized).
- **No new features/systems** (§J); perf work is optimization + bug-fix patches, within the §12 budgeted AI
  scheduler / instancing / particle-cap discipline (no new mechanics).
- Carries forward the Phase-0 **DEFERRED** 200-unit perf proof (never measured — no editor/device).

## 4. Known stability debt carried in (from prior reports, unchanged)
- Whole tree compiles + builds an APK (CI GREEN), but the game is **not wired into a playable scene**
  (GATE 1 OPEN) → no on-device session to profile.
- ECS sim perf at scale (200 units) and on-device frame budget remain **DEFERRED since Phase 0**.

## 5. Status / DEFERRED
Targets + matrix + method defined; **no profiling/crash data produced** — requires a playable on-device build
(DEFERRED; GATE 1 OPEN / GATE 2 DEFERRED / GATE 3 DEFERRED; GATE 5 DEFERRED). No stability/perf number is
asserted or fabricated (§15.8).

**Inherited validation debt (preserved, NOT closed):** GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED ·
GATE 4 PASS (static) · GATE 5 DEFERRED.
