# BULWARK — Phase 5.3 Balance Changelog (TEMPLATE — no entries yet)

**Status:** **METHODOLOGY + EMPTY CHANGELOG.** No live/playtest telemetry → **zero balance changes recorded.**
**No fabricated results.** **Authority:** roadmap §13 Phase 5 ("Tuning only — no new features"), §4 (combat
core — single shared chain, no fork), §5 (roster caps), §6 (commander power budget), §15 (no invented values;
no new mechanics); `PHASE_5_MASTER_EXECUTION_PROMPT.md` §F. Balance is **data-value tuning only** (UnitDef /
BalanceConfig / CounterMatrix / SpellDef / CommanderDef / EconomyConfig values via RC) — **no new units,
spells, maps, mechanics, or code**, and **no commander-power-budget change** (ADR-5-001 constraints).

## 1. Method (binding)
- Changes are **telemetry/playtest-driven value edits** to existing canon data, delivered via RC where the
  resolver supports it (`EconomyResolver`) or as SO value patches otherwise — **never** structural/mechanic changes.
- **Inviolable:** commander effects stay **≤ the §6 power budget** (the ADR-2-002 clamp is not relaxed);
  upgrades stay **capped** (§3); ranked normalization stays on; the §4 chain is **not forked**.
- Every entry records the **win-rate / usage / counter-balance signal** that motivated it (from telemetry) and
  the **guardrail** (no dominant unit/faction; readability intact).

## 2. Watch list (what 5.3 would balance from data)
Per-unit win/pick rates by faction · counter-matrix cell outcomes (5×4) · spell pick/impact + telegraph/counter
efficacy · commander pick/impact (within budget) · map-side win-rate skew · economy pacing (Gold/Silver/upgrade
costs). **All targets/changes TBD from telemetry.**

## 3. Balance changelog (chronological — EMPTY)
| Date | Asset (existing data) | Field | Old → New | Telemetry signal | Guardrail (must hold) | Result |
|---|---|---|---|---|---|---|
| — | — | — | — | — | — | **none — DEFERRED (no telemetry/playtest)** |

## 4. Status / DEFERRED
Method defined; **no balance change performed** — requires real session/playtest telemetry on a playable build
(DEFERRED; blocked by GATE 1 OPEN / GATE 2 DEFERRED / GATE 3 DEFERRED; GATE 5 DEFERRED). No value is changed or
invented without data (§15.6/§15.8). All Phase-1–3 balance values remain **PROVISIONAL/LSD-owned** as previously reported.

**Inherited validation debt (preserved, NOT closed):** GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED ·
GATE 4 PASS (static) · GATE 5 DEFERRED.
