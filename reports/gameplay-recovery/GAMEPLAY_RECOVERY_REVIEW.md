# BULWARK — Gameplay Recovery · Phase B: Independent Review

**Date:** 2026-06-06 · 4 parallel independent reviews of `GAMEPLAY_RECOVERY_MASTER_PLAN.md`, each verified against
the actual sim/Bootstrap code. **Outcome: all four = APPROVE_WITH_CHANGES (no REJECT).** Concerns **converge** —
there is **no reviewer-vs-reviewer disagreement**, so the "stop on disagreement" condition does **not** trigger.
The required changes below are **folded into the plan (v2 addendum)** before implementation.

| Reviewer | Lens | Verdict | Concerns |
|---|---|---|---|
| A | Architecture | APPROVE_WITH_CHANGES | 7 |
| B | Gameplay | APPROVE_WITH_CHANGES | 7 |
| C | DOTS/ECS | APPROVE_WITH_CHANGES | 10 |
| D | Adversarial | APPROVE_WITH_CHANGES | 9 |

## Verified-correct premises
- **RC-3 measurement fault confirmed (D):** `AttackState.Target` is written exactly once as `Entity.Null`
  (`UnitAuthoring.cs:36`) and **never again** anywhere in `Assets/_Game` — real acquisition is `Targeting.Current`.
  The plan's honesty correction is sound; the metric must be re-pointed.
- The overall decomposition, the logic-only discipline, the rollback/validation strategy, and the GATE-1 criteria
  framing are all accepted.

## Required changes (consolidated, blocking — folded into the plan)
1. **RC-1 mechanism prose is wrong (A/C, HIGH).** In-frame order is **BasicAI → AIScheduler → SquadAI** (not
   "SquadAI first"). The real race is a **cooldown-bake asymmetry**: `AICommander.ReevalCooldown=1.0`
   (`BattleBootstrap.cs:543`) gates BasicAI's miner-first opening ~1 s, while `SquadAIState.ReevalCooldown=0`
   (`:600`) lets SquadAI enqueue a ~95 g combat unit on frame 1. **The fix (gate SquadAI's combat enqueue behind
   the miner floor) is unchanged and correct** — only the prose is corrected. Also: a ~1 s opening window where
   *neither* enqueues is expected/benign; **BasicAI is the sole opening-miner authority** (acceptance: miner #1
   queued within ~1–2 s).
2. **`TargetMiners` is `private const` (`BasicAI.cs:63`) (D, HIGH).** Promote to **`internal`** (single shared
   source) so the SquadAI guard reads the same value. **No new numeric literal introduced.**
3. **RC-2 double-enqueue overlap (C, HIGH).** BasicAI already maintains the **AI-side** miner floor
   (`BasicAI.cs:286-300`). The new **`MinerFloorSystem` applies to the PLAYER side only (team 0)**; BasicAI stays
   the sole AI-side miner authority.
4. **RC-5 CRITICAL — spawned units have no `MoveDestination` (C, CRITICAL; B, HIGH).** `FormationSystem` writes the
   move override only `if (hasDest)` (`Formation.cs:171`). Stamping `FormationMember` alone is inert. **Also add
   `MoveDestination{Active=0}`** to every spawned non-miner combat unit in `SpellSummon.SpawnFromCatalog` (same
   ECB, structural-safe).
5. **RC-5 both sides + squad policy (A, MEDIUM).** Stamp `FormationMember` on **both** AI **and** player combat
   units, and build a **player `SquadFormation`**. `SpawnFromCatalog` is the single spawn path with no squad
   context → assign SquadId/Slot by **round-robin** (mirror the `RowCursor` pattern, `Training.cs:186-199`).
6. **RC-3 halt location (A/C/D, HIGH).** The **auto-target** branch already halts at range
   (`Combat.cs:128-129`). The interpenetration bug is specific to the **`Active==1` manual/override branch**
   (`Combat.cs:100-118`). RC-3(b)'s in-range halt **must live in the override branch** so it applies to ANY
   `Active=1` mover (SimAiDriver advance, HUD advance, **and** RC-5 formation slots). Re-test RC-3 acceptance again
   at the end of RC-5 with the scaffold off.
7. **RC-5 auto-demo split (D, HIGH — device-campaign breaker).** The device 20-match campaign is **not** batchmode;
   gating the whole `SimPlayerHud` auto-demo to headless would kill the player economy/army on device. **Keep
   auto-TRAIN always on** (economy + combat queue — the only unattended player army source, both device + CI);
   **gate only the auto-ADVANCE/push** so push-timing becomes organic/manual (agency) while economy runs unattended.
8. **RC-6 pacing reframe (B, HIGH; honesty).** "organic march shortens dead time" is **not** supported — once RC-5
   routes movement through `FormationSystem`, forward progress is throttled by SquadAI's anchor-step constant
   (balance). **The structural distance-closer is the per-unit `MoveSpeed` no-target march-to-contact fallback**
   (units advance at their own data-driven speed toward the enemy front when they have no target and no live
   formation order) — uses existing per-unit speed, **no new constant**. Any further pacing tuning (anchor-step,
   march-speed, spawn pos, map size) is **BALANCE → DEFERRED**. RC-6 is "structural fallback + documented defer."
9. **Criterion (E) variance vs determinism (B/D, HIGH — affects the PASS bar).** The sim has **no stochastic
   source** and gold-seed/balance are frozen, so **identical inputs → identical matches** (determinism is correct
   for an RTS, not a bug). Variance + "both Victory and Defeat" therefore **cannot come from RNG** — it must come
   from **varied player decisions**. **(E) is reframed:** across the 20-match campaign, **vary the player strategy**
   (push timing / composition via the HUD / auto-demo params) and show that **different decisions → different
   outcomes** (this jointly evidences agency (F) and variety (E)). Identical inputs yielding identical results is
   acceptable and expected.

## Other accepted concerns (non-blocking, folded as notes)
- RC-1 "single economy authority" clarified to "sole *opening-miner* authority; SquadAI resumes combat once the
  floor is met." (B)
- RC-3(b) is a newly-isolated root that **compounds** the gate1 RCA's "sparse armies" framing, not a replacement. (A)
- `MinerFloorSystem` ordering vs `TrainingSystem`/`MiningSystem` + in-flight counting must avoid a double-enqueue
  race; reuse the existing `CountQueuedRole`/`FindRoleIndex` helpers. (C)
- `FormationSystem` re-asserting `Active=1` every tick means the no-target fallback is dead for *formation* units →
  the fallback is for units **without** a live formation order (early/ungrouped), and formation march is via the
  anchor; keep both paths consistent with the override-branch halt (#6). (C)

## Decision
**No blocking disagreement among reviewers; unanimous APPROVE_WITH_CHANGES.** The 9 required changes are folded
into the master plan (see its "Phase-B revision" addendum). **Proceeding to Phase C implementation**, RC-1 first,
under the per-RC workflow. **Owner flag:** criterion (E)/variance is now input-driven, not RNG (item 9) — raised
here transparently; the PASS bar reflects it.
