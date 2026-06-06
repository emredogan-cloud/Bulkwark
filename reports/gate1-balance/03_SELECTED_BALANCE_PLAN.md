# BULWARK — GATE-1 Balance/Pacing · 03 · Selected Plan (Phase C)

**Date:** 2026-06-06 · **Selected: Option B (breakthrough/targeting) + Option E (statue vulnerability)** — the
minimum change set that makes matches resolve organically. All changes **symmetric (fair, no P2W)**, all
**PROVISIONAL/LSD tuning values** (§15.6), **§6 cosmetic-safety + ADR-2-002 commander cap (0.15) untouched**, no
monetization, no presentation change, no new system. Targets every PASS condition:

| PASS condition | How this plan achieves it |
|---|---|
| A) Organic victories | a side that wins/clears its lane sends free units to the now-reachable enemy statue → razes it (real combat, no probe) |
| B) Organic defeats | symmetric — the same happens to the player's statue when the AI breaks through (AI has a standing numbers edge) |
| C) Match variety | which side breaks through depends on roster asymmetry (Iron Pact tanky vs Ashen fast/cheap) + device frame-jitter → varying winners/durations |
| D) Agency | the player can still train + manually ADVANCE via the HUD to force/deny a breakthrough; composition choices now matter (they convert to base damage) |
| E) Mobile match length | shield removed + HP softened → a breakthrough fells the statue in ~20–60 s once reached (one unit ~45 s, a few ~20 s) → matches resolve in a ~2–4 min window |
| F) Identity preserved | same units, same roster, same economy, same map, same combat rules; only the statue's targetability/weight + its shield/HP change |

## The change set (exactly these, nothing more)
1. **Targeting — make the enemy statue reachable + commit to it (Option B).** `Assets/_Game/Sim/Systems/Targeting.cs`:
   - register each statue as a candidate **in every X-bin of its rows** (not only its own X-bin) so a frontline
     unit's neighborhood search always includes the enemy statue;
   - raise **`StatueBonus` 0.01 → 25.0**. Score is `weight/(1+distSq)`, so a NEAR enemy (tiny distSq) is always
     preferred and units still fight the line; the higher weight only lets a unit commit to the (far) enemy base
     over a DISTANT enemy. Gradient: at mid-field a unit still engages enemies within ~5 units; once it pushes past
     the sparse 2.6-unit line, the shrinking statue distance makes it commit harder → breakthrough. (1.0 was
     insufficient — with the distance term it only beats *nothing*; the AI's standing row-coverage edge needs the
     statue to out-score a *distant* straggler to convert into a push.)
   - Net effect: a unit with a clear lane targets the enemy statue and (via the existing auto-target StepToward /
     RC-5 march) advances on it; the numbers/roster-superior side breaks through. **No new constant beyond the one
     re-weighted tuning value; no combat-stat change.**
2. **Statue vulnerability (Option E).** `Assets/_Game/Bootstrap/BattleBootstrap.cs`:
   - **ShieldHealth 250 → 0** (the shield was a pure delay buffer that the first breakthrough must chew through);
   - **Health 1000 → 700** (still a substantial siege objective; with breakthrough DPS ~16–60 it falls in ~12–45 s
     once reached). Symmetric — both statues identical.

## Explicitly NOT doing
- No economy/mine/startingGold change (armies already grow — not the bottleneck).
- No unit HP/damage/range/cost/trainTime change (preserve combat identity + the §4 counter matrix).
- No sudden-death/time-escalation, no army-pressure ramp (Option A/D) — unnecessary if access is fixed; **retained
  as a documented contingency** only if validation shows B+E under-resolve.
- No monetization, no presentation, no commander-cap, no cosmetic-safety change.

## Validation gate (Phase E)
20 Android matches on the implementation build. **PASS requires:** organic economy + armies + real combat (already
proven) **AND** organic endings (statue destroyed by combat, no probe), **both Victory and Defeat observed**,
agency demonstrable, and match durations in a mobile-suitable range. Implement → review → adversarial → CI →
20-match device validation (`04_…`, `05_…`).
