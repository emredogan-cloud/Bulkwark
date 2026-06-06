# BULWARK — GATE-1 Balance/Pacing · 02 · Balance Options (Phase B)

**Date:** 2026-06-06 · Candidate solutions for the **geometric stalemate** (`01_…`). Root cause: units never reach
the enemy statue (access), not economy/HP/numbers. Each option rated for **fairness** (must be symmetric / no P2W),
**RTS readability**, **implementation risk**, **expected outcome variety**, and **BULWARK-vision compatibility**.
Constraints honored throughout: symmetric only; §6 cosmetic-safety + ADR-2-002 (0.15) untouched; no monetization;
no presentation change.

| # | Option | What it changes | Fairness | Readability | Risk | Variety | Vision fit | Addresses ROOT (access)? |
|---|---|---|---|---|---|---|---|---|
| **A** | **Sudden-death escalation** (time-ramped combat damage via `SystemAPI.Time.ElapsedTime`) | new symmetric time factor in the damage chain | ✅ symmetric | ◑ "why is everything dying faster?" | **High** (new mechanic + ramp tuning; no sim clock today) | High (numbers edge tips) | ◑ overtime is fine but adds a system | ❌ **No** — units still don't reach the statue; escalation alone just thins the line faster, still no breakthrough |
| **B** | **Breakthrough / targeting** — make the enemy statue **acquirable from the front** + raise `StatueBonus` so free units commit to it | `Targeting.cs` candidacy + weight | ✅ symmetric | ✅ "units push the base when the lane is clear" | **Low** (query/weight tweak; reuses RC-5 march) | High (numbers/roster/jitter decide who breaks through) | ✅ core RTS "push the base" identity | ✅ **Yes — directly** |
| **C** | **Economy pacing** (more gold / mines) | `map_openfield.asset` / `startingGold` | ✅ symmetric | ✅ | Low | Low | ◑ | ❌ **No** — armies already grow (2.6 v 4.6); gold isn't the bottleneck |
| **D** | **Army-pressure escalation** (rising spawn/queue caps over time) | Training/BasicAI | ✅ symmetric | ◑ | Med | Med | ◑ | ❌ **No** — bigger armies still grind the same line; access unchanged |
| **E** | **Dynamic statue vulnerability** (remove shield buffer / soften HP / decay) | `BattleBootstrap.cs` statue seeds | ✅ symmetric | ✅ | Low | Med | ◑ (statue is a balance value) | ◑ **Partial** — useless ALONE (statue never hit), but makes a B-breakthrough resolve in mobile time |

## Analysis
- **B is the only option that addresses the ROOT cause** (units can't reach the statue). It is also the lowest-risk
  (a targeting candidacy/weight change, reusing the existing RC-5 march), the most readable ("when your lane is
  clear, push the enemy base"), and the most on-vision (an RTS is *about* razing the enemy base). It converts the
  already-present numbers/roster advantages into organic breakthroughs → organic Victory/Defeat with variety.
- **E pairs with B**: removing the 250-shield buffer (a pure delay) and modestly softening HP ensures a B-driven
  breakthrough fells the statue inside a mobile 2–4 min window. On its own E does nothing (statue never reached).
- **A / C / D are rejected as unnecessary**: they do not address access. A (sudden-death) is a higher-risk new
  mechanic that, alone, still leaves the statue unreached; C/D address economy/army size which the telemetry proves
  are NOT the bottleneck. (A is retained only as a fallback guarantee-of-resolution if B+E under-resolve in
  validation.)

## Direction
Proceed with **B (primary) + E (supporting)** as the minimum set — see `03_SELECTED_BALANCE_PLAN.md`. Keep A
(sudden-death) documented as a contingency lever, NOT implemented unless validation shows B+E insufficient.
