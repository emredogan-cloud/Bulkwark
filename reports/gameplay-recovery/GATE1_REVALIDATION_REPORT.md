# BULWARK — GATE 1 Revalidation Report (Gameplay Recovery · Phase E)

**Date:** 2026-06-06 · **Build:** `1475ce8` (CI run 27056026588, **GREEN** first attempt), all of RC-1→RC-6.
**Device:** Xiaomi Redmi Note 11R. **Sample:** **20 real matches**, 900 read-only `[GATE1]` telemetry samples
(45/match). **Method:** the full recovery (author → 4-reviewer design review → per-RC review/adversarial-audit →
CI → device) then this fresh campaign. **Discipline:** every fix logic-only — **no balance/content/assets touched.**
**Answer with evidence only — no conditional wording, no optimism, no fabrication.**

---

## VERDICT: **FAIL**

The recovery **fixed every targeted logic defect** — the AI now runs a real economy, miners survive, and genuine
unit-vs-unit combat happens in **20/20** matches. But **0/20 matches reach an organic decision**: the two armies
lock into a **perpetual front-line stalemate** and **no statue is destroyed**, so no match ends in Victory or
Defeat. A game that never resolves is not a complete, fun micro-battle. **GATE 1 = FAIL.**

## Evidence (n = 20, every figure parsed from `runtime/device_validation/revalidation/m*.txt`)
| GATE-1 criterion (master-plan §7) | Result | Evidence |
|---|---|---|
| (A) Two-sided **economy** | ✅ PASS | AI economy alive (`gA>5`) **20/20**; AI miners ≥1 **20/20** |
| (B) Two-sided **army** | ✅ PASS | peak units AI **avg 5.0** / player **avg 4.0** (was AI 1) |
| (C) Real **unit-vs-unit combat** | ✅ PASS | `Engaged>0` in **20/20** (corrected `Targeting.Current` metric; baseline 0/390) |
| (D) **Organic resolution** | ❌ **FAIL** | **0/20** organic ENDs; outcome = **Ongoing 20/20**; **0** probe spikes (probe removed) |
| (E) **Variety / both outcomes** | ❌ FAIL | no outcome variety — all 20 Ongoing; neither Victory nor Defeat ever reached |
| (G) Pacing / legibility | ◑ partial | combat happens, but statues barely touched: **`sA_min=1000` (AI statue never hit), `sP_min=939`** over 135 s |
| (F) Agency | — not isolated | matches were auto-driven (train); manual HUD control exists but was not the test variable |

## Before → after (this recovery vs the original GATE-1 FAIL)
| Dimension | Original FAIL | After RC-1→RC-6 |
|---|---|---|
| AI economy | dead (`gA==5`, 0 miners, 20/20) | **alive** (`gA>5`, miners ≥1, 20/20) |
| AI army | 1 unit | **~5 units** |
| Unit-vs-unit combat | none (metric was a dead field) | **real, 20/20** |
| Match end | probe-decided walkover (10/10) | **no probe; 0 artificial ends** |
| Match resolution | fake "Victory" | **none — perpetual stalemate (0/20 resolve)** |
**The three deferred GATE-1 bugs are fixed** (AI economy collapse; miner death; miner replacement) and combat is
real. The failure mode has changed from *"there is no contest"* to *"the contest never ends."*

## Root cause of the residual FAIL (stalemate) — and why it is OUT OF SCOPE here
- **Mechanism:** both economies are now healthy and **reinforce the front line continuously**; opposing units meet
  in the middle and fight (RC-3 halt), but neither side wins the local clash decisively, so **no survivor breaks
  through** to reach + destroy a statue. The AI statue is **never even touched** (`sA_min=1000`); the player statue
  loses only ~6 % (`sP_min=939`) in 135 s. At that rate a statue would take ~tens of minutes to fall — effectively
  never within a match. This is exactly the risk the RC-4 review flagged.
- **Why it is not fixable under this program's rules:** breaking a stalemate requires **balance/design** levers —
  a sudden-death/match timer, an economy or unit-strength asymmetry, breakthrough/push-through mechanics, lane
  geometry, or spawn/march tuning. **All of these are balance or new mechanics, which the Gameplay Recovery
  Program explicitly forbade** ("No balancing yet. No new content."). The recovery was scoped to **logic fixes**,
  and within that scope it is **complete** — it cannot, by its own rules, add the tuning that would force a result.

## What this means
- **The recovery succeeded at its mandate** (make a real RTS match *exist*): a two-sided economy, growing armies,
  and real front-line combat now run on device, with no probe and no balance change. That is a genuine
  transformation from the presentation-complete-but-hollow shell.
- **GATE 1 still FAILS** because *fun* requires the match to **resolve** (Victory/Defeat) and vary — and it does
  not. Honest verdict: **FAIL.**

## Recommendation (NOT executed — out of scope; needs separate authorization)
A focused, separately-authorized **balance/pacing pass** is required to make matches resolve, e.g. one or more of:
a sudden-death timer / escalating statue-damage over time; a breakthrough rule (units commit past a thinning
line); or a small, fair economy/strength asymmetry — each a **balance/design** change that GATE-1 owners must
approve (and that must respect §6 cosmetic-safety / fairness / the ADR-2-002 cap). With matches resolving, this
exact 20-match campaign can be re-run for a clean PASS/FAIL.

## Validation integrity
All numbers come from the read-only `[GATE1]` logger (no gameplay/balance change to obtain them); the corrected
combat metric reads `Targeting.Current` (the real field). No runtime evidence is fabricated; the verdict is
unconditional.

---

**STOP — per the program exit condition. Verdict delivered (FAIL). GATE 2 not started. Roadmap Phase 5 not
started. No further gameplay changed.** The logic-only recovery (RC-1→RC-6) is complete and device-validated; the
remaining blocker (stalemate) is a balance/design matter for a future, separately-authorized pass.
