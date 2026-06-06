# BULWARK — GATE-1 Balance/Pacing · 01 · Stalemate Root-Cause Analysis (Phase A)

**Date:** 2026-06-06 · **Program:** GATE-1 Balance & Pacing Recovery (gameplay-design; balance changes authorized).
**Basis:** 20-match device campaign (`reports/gameplay-recovery/GATE1_REVALIDATION_REPORT.md` +
`runtime/device_validation/revalidation/m*.txt`, 900 `[GATE1]` samples) + a 3-agent read-only code investigation
(unit data, combat/clock seams, telemetry curves). **No edits made in this phase.**

## Headline
The stalemate is **GEOMETRIC / TARGETING — not economy, not damage-magnitude, not statue-HP.** Two real armies
form and fight, but **units almost never reach the enemy statue**, so no base takes meaningful damage and no match
resolves. **Fixing access (reach + attack the statue), not numbers, is the lever.**

## Quantified evidence
- **Armies grow and plateau (NOT pinned):** steady-state (t≥60 s, mean of 20) **player 2.62 / AI 4.57 units**;
  plateau by ~t63 s. The AI even holds a standing ~2-unit edge — yet still cannot break through.
- **Reinforcement ≈ attrition (equilibrium):** spawns P 0.065/s, A 0.066/s; deaths P 0.043/s, A 0.029/s → every
  casualty is replaced, the line never collapses. Combat is low-intensity: engaged ≤ **1 unit/side**, `atkP==atkA`
  in **99.9 %** of samples — symmetric mutual nibbling at the dead-center mine line (~x18–22).
- **Statues are never reached:** **`sA=1000` in 900/900 samples** (AI statue hit ZERO times); player statue rate
  **0.094 HP/s → 222 min** to kill (only the rare lone leaker; worst dip `sP=939`). To resolve in 120–240 s needs
  **5.2–10.4 HP/s = 55–110× current** delivered statue damage.
- **It's access, not HP:** **one melee unit AT the statue kills it in ~75–80 s** (Legionary 16 dmg/1.0 s; Ironclad
  30/1.8 s). Statue = 1000 HP + 250 shield = 1250 effective, **no regen** (no health-increase path exists).
- **Economy is not the bottleneck:** miners constant all match (mP=2, mA=3); income ~5.3–6.9 g/s/side; gold a
  sawtooth 9↔104, funding a steady unit drip. More gold would not change the targeting geometry.
- **Why units don't reach the statue (the mechanism):** TargetingSystem registers each statue **only at its own
  X-bin** (`Targeting.cs:123-139`) and weights it `StatueBonus=0.01` (`Targeting.cs:54`); units search only
  `SearchBinRadius=4` bins (~5 world units, `Targeting.cs:48`). A frontline unit at x≈20 never has the enemy statue
  (x=0/40, ~18–20 units away) in its search set, and the 0.01 weight means it's chosen only when no other target
  exists — which, at a continuous line, never happens. RC-5's no-target march only triggers when `Targeting.Current
  == Null`, but at the dense line a unit almost always has a nearby enemy → it never "frees up" to march.

## Seams available for a minimal fix (from the code)
- **Targeting candidacy + weight:** `Targeting.cs` statue bucketing (123-139) + `StatueBonus` (54) + search radius
  (48) — the direct access lever.
- **Statue durability:** `BattleBootstrap.cs` Health=1000 (:101), Shield=250 (:104), TrickleThrottle=0.5 (:107) —
  the shield is a pure delay buffer; the throttle does NOT cap real hits.
- **Time/sudden-death:** **no sim match-clock exists** (MatchState has only `Outcome`); a time-based lever would
  need `SystemAPI.Time.ElapsedTime` read in CombatSystem (already used by 13 systems) — available but a new mechanic.
- **Balance-LOCKED (do NOT touch):** §6 cosmetic-safety (render-only), ADR-2-002 commander cap **0.15**
  (`Combat.cs:62/222`), the shared §4 counter matrix. Unit/statue/economy values are PROVISIONAL/LSD-owned (§15.6).

## Smallest-intervention conclusion
The root cause is **statue unreachability**. The smallest fix makes frontline units **acquire + commit to the
enemy statue** (so the existing numbers/position advantages convert into breakthroughs), plus a **minor statue
softening** so the breakthrough resolves within a mobile window. Economy, unit DPS/HP, sudden-death escalation, and
army-size are NOT required to change. Options + selection follow in `02_…` and `03_…`.
