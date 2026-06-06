# BULWARK — GATE-1 Balance/Pacing · 04 · Implementation Log (Phase D)

**Date:** 2026-06-06 · Implements the minimum set from `03_SELECTED_BALANCE_PLAN.md` (Option B + Option E). All
changes **symmetric (fair)**, **PROVISIONAL/LSD tuning** (§15.6); §6 cosmetic-safety + ADR-2-002 commander cap
(0.15) untouched; no monetization; no presentation change; no new system. Workflow: author → independent review →
adversarial review → repair → CI → 20-match Android validation (`05_…`).

## Changes (3 edits, 2 files)
### Option B — make the enemy statue reachable + commit to it (`Assets/_Game/Sim/Systems/Targeting.cs`)
1. **Statue registered in EVERY x-bin of every row** (1b bucketing) — was only its own x-bin, so a frontline unit's
   `SearchBinRadius=4` (~5 units) neighbourhood never included the statue (~18–20 units behind the line). Now the
   enemy statue is always a candidate. The entry keeps the statue's TRUE `Pos`, so march/attack distance is correct
   regardless of which bin matched. Capacity reservation updated `unitCount + statueCount*rows` →
   `…*rows*xbins` so the `NativeParallelMultiHashMap` never reallocates. (Removed the now-unused `xb` local.)
2. **`StatueBonus` 0.01 → 40.0.** Scoring is `weight/(1+distSq)`. A near enemy (tiny distSq) is still always
   preferred (units fight the line — a unit holds even a point-blank enemy until the statue is within ~6 u); the
   higher weight only lets a unit commit to the far enemy base over a *distant* enemy, once its lane is clear →
   breakthrough. Only ENEMY statues are candidates (the acquire loop skips `et==myTeam`); miners never acquire
   (RC-2 `WithNone<MinerTag>`), so neither change affects miners or own-base. (Initially set 25.0; the adversarial
   review judged 25 in-band but flagged **"too timid" as the likelier residual risk** and recommended nudging UP
   to 40–60 — so under the one-build budget it was set to **40.0**, the conservative end of that band.)

### Option E — statue vulnerability (`Assets/_Game/Bootstrap/BattleBootstrap.cs`)
3. **`statueMaxHealth` 1000 → 700** and **`statueShieldHealth` 250 → 0** (the shield was a pure delay buffer). Both
   statues identical → symmetric. Verified no scene/prefab serializes `BattleBootstrap` and no code assigns these
   fields, so the code defaults are authoritative at runtime (the prior 20-match campaign used Health=1000 from
   here). One melee unit fells 700 HP in ~45 s; a small breakthrough force in ~15–20 s → mobile-suitable.

## Not changed (per plan)
Economy / mines / startingGold; unit HP/damage/range/cost/trainTime; §4 counter matrix; commander cap;
cosmetic-safety; monetization; presentation. Sudden-death/escalation (Option A) NOT implemented — kept as a
documented contingency only if validation under-resolves.

## Build-1 result → Build-2 retune (the "repair" step)
**Build-1** (`52e1504`, StatueBonus 40, statue 700/0): 20-match device validation = **1/20 organic** (m1 Defeat,
145 s, `sP_min=-14` = a REAL organic statue kill, **0 probe spikes**) — the mechanism is proven, but **19/20 still
Ongoing** (too timid, as the adversarial review warned). The AI broke through (it fielded **6 units vs the
player's 4** — its 3-miner floor vs the player's 2); the player never did (`sA_min=1000`). First-combat 48 s
(line combat preserved). **Diagnosis:** (a) breakthroughs too rare → push `StatueBonus` harder; (b) statue still
slow to fall → soften more; (c) economic asymmetry → only the AI wins → symmetrize for BOTH outcomes.
**Build-2 retune (all symmetric):**
- `StatueBonus` **40 → 120** (commit to the base over enemies beyond ~2.4 u at mid-field → frequent breakthroughs;
  match length still floored by the ~48 s march, so not a trivial race).
- `statueMaxHealth` **700 → 500** (breakthroughs resolve faster, mobile window).
- `SimPlayerHud` player auto-economy **2 → 3 miners** (symmetrize with the AI's `TargetMiners=3` floor → equal mine
  occupancy/income → equal armies → BOTH Victory and Defeat possible). This is gameplay-input (the player's
  auto-stand-in), not a render/presentation change.

## Build-2 result → Build-3 pivot (Option A escalation)
**Build-2** (`cabb94c`, StatueBonus 120, statue 500/0, symmetric 3-miner economy): 20-match device validation =
**0/20 organic** (both statues untouched, `sP_min=sA_min=1000`; player 5.3 ≈ AI 5.3 units). **Worse than build-1's
1/20** — and the cause is instructive: build-1's lone resolution came from the AI's *asymmetric* numbers edge
(6 v 4); symmetrizing the economy produced a PERFECT balanced line that never opens. **Proven on device:
targeting/access (Option B) alone CANNOT break a balanced, continuously-reinforced contact line** — every unit
always has an enemy in attack range, so the `StatueBonus` lever never gets the clear lane it needs. The real
blocker is the **reinforcement = attrition equilibrium** itself.

**Build-3 = add Option A (sudden-death escalation)** — the documented contingency, now necessary. In
`Combat.cs` CombatSystem: after `kEscalationStartSec=40 s`, ALL combat damage ramps `+8%/s` (cap `10×`),
**symmetric for both teams** (`dmg *= matchEscalation`, applied after the §13 P1.4 chain, before the unit/statue
branch). Casualties then outpace gold-limited reinforcement → the balanced line thins → units (high StatueBonus)
break through → a statue falls; the marginally-stronger side (jitter/position) wins → BOTH outcomes; match length
bounded (~110–140 s). Per-match clock via `SystemAPI.Time.ElapsedTime` (≈0 at match start — frozen during the
`timeScale=0` menus; each validation match is a fresh app launch). Kept: StatueBonus 120, statue 500/0, symmetric
economy. NOT a balance edge (no side favored). Independent + adversarial review before build.

## Build/validation budget
One CI build; the 20-match campaign runs on that same APK. il2cpp segfault = transient flake → auto-retry the
same commit (never a new build). Result + verdict → `05_…` then `GATE1_PASS_REPORT.md` / `GATE1_FAIL_REPORT.md`.

## Review status
- Independent + adversarial review: **PASS, 0 blockers.** Compile/Burst/perf clean (capacity exact 202, no
  realloc; ~270 extra candidate-comparisons/tick — within §3 budget). `StatueDamage` with shield=0 SAFE
  (`ShieldActive=false` at seed → damage to Health, no div-by-zero, destroy on Health≤0). HP=700 has no
  hardcoded-1000 dependency (all normalization reads `MaxHealth` dynamically). Fully symmetric/fair (commander cap
  + counter matrix untouched). Adversarial: `StatueBonus` is in the resolving band, line combat stays dominant;
  residual risk is "too timid" → hedged 25→40. 2 LOW notes: ranged units commit to the statue from ~their range
  (desired breakthrough, not a bug); commit balance edits separately from the unrelated working-tree UI files
  (done — only `Targeting.cs` + `BattleBootstrap.cs` + these reports are committed for an attributable build).
- CI: pending. Android 20-match validation: pending.
