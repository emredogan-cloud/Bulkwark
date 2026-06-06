# BULWARK — Gameplay Recovery Master Plan (GATE-1 Rescue)

**Date:** 2026-06-06 · **Status:** Phase A (design) — **no code yet.** · **Source of truth:** the GATE-1 **FAIL**
verdict (`reports/gate1/`). **Goal:** turn the presentation-complete shell into a **functioning RTS match** with a
legitimate GATE-1 verdict. **Inputs re-read:** all five `GATE1_*` reports; the roadmap; ADRs; Phase 0–4 +
Production-Presentation reports; and a 6-agent **read-only** code investigation (verified against the actual sim).

> ## THE BINDING DISCIPLINE — fix LOGIC, not BALANCE
> Every fix below is **control-flow / query-filter / new-system-reusing-existing-values**. **No** cost, HP, damage,
> range, train-time, mining-rate, march-speed, spawn-position, map-size, gold-seed, or commander-cap value is
> changed. **Balance-locked (MUST NOT touch):** `startingGold=100` (`BattleBootstrap.cs:111`), all `UnitDef`
> cost/hp/damage/range/trainSeconds (data/LSD-owned), commander `PowerBudgetPct ≤ 0.15` (ADR-2-002), AI pacing
> knobs `TargetMiners=3`/`MaxQueueLength=5`/`ReevalPeriod` (referenced, not retuned), cosmetic-safety §6, map
> geometry. No new content/assets/monetization. Not roadmap Phase 5.

---

## ⚠️ Honesty correction inherited into this plan (RC-3 measurement fault)
The investigation found that **both** the `[GATE1]` logger and the in-build `[SIMPROOF]` logger measured
"engaged" via **`AttackState.Target`**, a field **no system ever writes** — real target acquisition lives in
**`Targeting.Current`**. So the prior "**0/390 unit-vs-unit combat**" was guaranteed-zero **by construction** and
is **not a valid measurement**. **The GATE-1 FAIL verdict still stands** on the other, sound evidence (AI gold ==5
in 390/390, 0 AI miners, AI stuck at 1 unit, 10/10 probe-decided, deterministic ~116 s). But the combat metric
**must be re-pointed to `Targeting.Current`** before revalidation, or it will read 0 even after combat works. This
correction is built into RC-3 below.

---

## 1. Root-cause graph (RC-1 → RC-6 dependency map)
```
                 ┌────────────────────────────────────────────────────────┐
                 │  STRUCTURAL DEEP ROOT (spans RC-3/RC-5/RC-6):           │
                 │  the march pipeline is unterminated — FormationMember   │
                 │  is never stamped → FormationSystem never marches units;│
                 │  the SimAiDriver scaffold injects movement to hide this.│
                 └───────────────┬────────────────────────────────────────┘
RC-1 AI economy ──────────────┐  │
 (dual train-source race;     │  │
  SquadAI spends opening gold ▼  ▼
  before BasicAI buys a miner) ──►  AI has gold + miners + a GROWING army
RC-2 miner survival ──────────►  both economies stay alive (miners not killed; floor maintained)
        │                         │
        └──────────┬──────────────┘
                   ▼
        RC-3 combat acquisition  = (a) FIX METRIC (Targeting.Current, not AttackState.Target)
                   │               + (b) HALT-to-fight on attack-move when a target is in range
                   │               + (needs armies from RC-1/2 + contact from RC-5/driver)
                   ▼
        RC-4 probe removal       = delete the SimAiDriver victory-latch probe; rely on the REAL
                   │               win condition (statue Health≤0 → MatchState), which already works.
                   ▼
        RC-5 agency / organic march = stamp FormationMember (+ player squads) + a no-target
                   │               march-to-contact fallback in MovementSystem; gate the debug
                   │               auto-drivers so the match runs organically, player can override.
                   ▼
        RC-6 pacing / dead-time  = first-contact lag is mostly a SYMPTOM of the unterminated march
                                   (RC-5). The remaining levers (march speed / spawn pos / map size)
                                   are BALANCE → DEFERRED. RC-6 is "resolved by RC-5 + documented".
```
**Reading:** RC-1 and RC-2 are the true economic roots; RC-3 needs them (armies must exist) plus its own metric +
halt fix; RC-4 needs RC-1/2/3 so matches end organically; RC-5 is the deep march-pipeline fix that makes movement
organic (removing reliance on the scaffold) and creates agency; RC-6 follows from RC-5 (no balance tuning).

## 2. Fix ordering (per the mandate, with dependency notes)
**RC-1 → RC-2 → RC-3 → RC-4 → RC-5 → RC-6.** Notes: movement during RC-1–RC-4 still uses the existing
`SimAiDriver` advance scaffold (so units reach contact); RC-4 removes only the **probe** (not the advance); RC-5
then replaces the advance with the **organic** `FormationMember` march and gates the scaffolds. This keeps each
step independently testable while honoring the order. Each RC ships through the full per-RC workflow (§4).

## 3. Per-RC plan (code locations · systems · side effects · validation · acceptance)

### RC-1 — AI economy startup
- **Root:** dual TrainOrder authority; `SquadAISystem.ApplyEcoAxis` (`SquadAI.cs:519-563`) is miner-blind and runs
  first (baked `SquadAIState.ReevalCooldown=0`), spending the opening 100 g on a ~95 g combat unit before
  `BasicAISystem.BiasTraining` (`BasicAI.cs:273-312`, the only miner-first path) ever runs (gated by
  `AICommander.ReevalCooldown=1.0`). Gold strands at 5; affordability gate then blocks everything.
- **Fix (logic):** in `SquadAI.cs::ApplyEcoAxis`, **gate the combat enqueue behind the miner floor** — count this
  side's live `MinerTag` + in-flight miner `TrainOrder`s (reuse `FindRoleIndex(RoleId.Miner)` + the existing
  in-flight queue-walk); if below `BasicAI.TargetMiners`, **return without enqueuing** (defer to BasicAI's
  miner-first opening). Make BasicAI the single economy authority. No balance constant touched.
- **Impacted:** `SquadAISystem`, `BasicAISystem`, `TrainingSystem` (unchanged, gets correct order), `MiningSystem`.
- **Side effects:** AI buys miners first → income flows → AI funds army; SquadAI combat pacing resumes once floor met. Player side untouched.
- **Validation:** `[GATE1]` telemetry — AI gold rises >5 and cycles; AI miners ≥1; AI units >1 sustained.
- **Acceptance:** `mA≥1` within the opening; `gA>5` and fluctuating in the majority of samples; AI fields >1 unit (vs current 0.0 miners / gA==5 / 1 unit).

### RC-2 — Miner survival + replacement
- **Root:** `TargetingSystem` (`Targeting.cs:99-103`, `:141-144`) selects `WithAll<UnitTag,Health>` with **no
  `MinerTag` exclusion** → miners are valid targets AND acquire targets → walk into combat and die
  (`Combat.cs:294 DestroyEntity`); no live-miner-floor maintainer for the player side.
- **Fix (logic):** add **`.WithNone<MinerTag>()`** to both Targeting queries (miners neither targeted nor
  acquiring); add a **MinerFloorSystem** (sim) that, per team, tops miners to the existing `TargetMiners` floor via
  the existing `EnqueueTrain` path (data-driven cost; no new constant) — applies to **both** sides.
- **Impacted:** `TargetingSystem`, new `MinerFloorSystem`, `MiningSystem` (handles idle miners already), `TrainingSystem`.
- **Side effects:** miners stop dying to combat + are replaced; wasted swings at miners vanish; no effect on real combat.
- **Validation:** telemetry — per-team live miner count stays ≥1 (AI ~3) all match; no miner holds a non-null target.
- **Acceptance:** miner count never hits 0 after the opening on either side; 0 miners recorded as combat targets.

### RC-3 — Combat acquisition / unit engagement
- **Root:** (a) **measurement fault** — metric reads dead `AttackState.Target`; real field is `Targeting.Current`;
  (b) **engagement fault** — an active attack-move (`MoveDestination.Active==1`) keeps stepping toward the statue
  even when a valid enemy is within `AttackState.Range`, so columns interpenetrate without fighting.
- **Fix (logic):** (a) re-point the engaged metric to `Targeting.Current != Entity.Null` in **`GateTelemetry.cs`**
  + **`SimDebugOverlay.cs`** (instrumentation only); (b) in `MovementSystem` (`Combat.cs` attack-move branch),
  **halt this tick** if `Targeting.Current` is a valid enemy already within `AttackState.Range` (reuse the same
  Range the CombatSystem already gates on — no range tuning).
- **Impacted:** `MovementSystem`, `CombatSystem` (now gets to resolve swings), the two loggers.
- **Side effects:** front lines form; statue-chip drops (units tie up in melee); units die from real combat.
- **Validation:** corrected `Engaged>0` whenever opposing units are in range; unit `Health` decreases from combat; unit deaths attributable to combat (not statue).
- **Acceptance:** ≥1 pair halts at range and trades damage; a unit dies from unit-combat; `Engaged>0` (corrected metric) in matches where armies meet.

### RC-4 — Probe removal + real win condition
- **Root:** `SimAiDriver` victory-latch **probe** (`ProbeAtSeconds=120` → `StatueDamageInbox 100000`) force-ends
  every match; the **real** win condition (statue `Health≤0` → `MatchState.Outcome`, via `StatueDamage`/`MatchFlow`)
  is sound but never reached because nothing organically destroys a statue (pre RC-1/2/3).
- **Fix (logic):** **delete only the probe block** in `SimAiDriver.Update` (+ unused `ForceFinishStatue`/probe
  fields); keep the advance injector for now (removed in RC-5). Rely on the real win condition.
- **Impacted:** `SimAiDriver` (Bootstrap debug scaffold; not sim canon), `StatueDamageSystem`/`MatchFlow` (unchanged).
- **Side effects:** sequenced **after** RC-1/2/3 so matches still end (organically). A headless safety-net probe may be retained **batchmode-only** (RC-5) to prevent infinite CI runs.
- **Validation:** no `VICTORY-LATCH PROBE` log; statue HP never spikes to ≈ −99,900; `MatchState` set only by real damage.
- **Acceptance:** `grep` finds no `ProbeAtSeconds/ForceFinishStatue/VICTORY-LATCH`; ≥5 matches end via real statue death; outcomes/durations vary.

### RC-5 — Agency + organic march (the deep structural fix)
- **Root:** `FormationMember` is **never stamped** on spawned units (`SpellSummon.SpawnFromCatalog`,
  `Spell.cs:867-925`) → `FormationSystem` (the canonical march owner) never acts → units stand still unless a debug
  scaffold injects `MoveDestination`. Player has no `SquadFormation` at all.
- **Fix (structural, allowed):** (1) stamp `FormationMember` on every spawned non-miner combat unit (side squad id
  matching `BuildAISquads`); (2) build a **player** `SquadFormation` (+ symmetric scaffolding); (3) add a
  **no-target march-to-contact fallback** in `MovementSystem` (advance toward the enemy front, derived from
  existing `StatueTag`/spawn positions — no speed/position constant added); (4) **agency:** gate the
  `SimPlayerHud` auto-demo and the `SimAiDriver` advance to **headless** (CI) so on-device the organic pipeline
  drives and the player can override via the BattleHud; keep auto-train for validation.
- **Impacted:** `SpellSummon.SpawnFromCatalog`, `BattleBootstrap` (player squads), `FormationSystem`,
  `MovementSystem`, `SimPlayerHud`, `SimAiDriver`. **Care:** `FormationSystem` skips units with a live `Active=1`
  override, so manual ADVANCE still works.
- **Side effects:** AI/player units march and reach contact **organically** (scaffold no longer needed); manual
  orders contest formation slots (handled by the existing override skip).
- **Validation:** with `SimAiDriver` advance disabled, AI units still move + reach combat; ≥1 match ends Victory **or** Defeat organically; player HUD orders change unit movement.
- **Acceptance:** AI units move + engage with the scaffold off; both outcomes reachable; player input demonstrably alters the match.

### RC-6 — Pacing / dead-time
- **Root:** ~63 s to first contact is a **symptom** of the unterminated march (RC-5). The direct levers (march
  speed, spawn anchor inboard, map width) are **BALANCE → DEFERRED**.
- **Fix:** **none beyond RC-5** (organic march-to-contact shortens dead time structurally). Any further reduction
  is a **balance-tuning task deferred** to a later, separately-authorized pass; documented, not done.
- **Validation/acceptance:** first meaningful action (combat or statue contact) occurs and the match is **engaging
  from contact onward**; we **measure** time-to-first-combat in revalidation but do **not** tune balance to hit a target.

## 4. Validation strategy (per RC + overall)
Per RC: **author → independent review → adversarial audit → repair → CI/CD GREEN → Android device validation**,
with evidence = **screenshots + telemetry + logcat + before/after metrics** (no claim without evidence). The
read-only `[GATE1]`/`[SIMPROOF]` loggers are the measurement spine (with the RC-3 metric correction). Overall: a
fresh **≥20-match** device campaign (Phase E) → `GATE1_REVALIDATION_REPORT.md` → unconditional **PASS/FAIL**.

## 5. Rollback strategy
- Each RC is a **small, isolated** logic change on its own commit → revert that commit to roll back one RC.
- Git branch discipline: work on `main` per the established flow; every RC commit is independently revertible; CI
  gates each. If an RC regresses (CI red or device worse on metrics), **revert and re-plan** before proceeding.
- The debug scaffolds (`SimAiDriver`, auto-demo) are retained until RC-5 proves the organic path, so there is
  always a known-good fallback to compare against.
- No balance values are touched, so there is **no balance to roll back** — only logic commits.

## 6. Success metrics (measured via the corrected telemetry, n≥20)
1. **AI economy:** AI miners ≥1 sustained; `gA` non-flat (earns+spends) — vs 0 miners / `gA==5`.
2. **Armies:** AI peak units > 1 sustained (vs 1).
3. **Real combat:** `Engaged>0` (corrected `Targeting.Current` metric); ≥1 unit death from **unit combat** per match.
4. **Organic ends:** matches end by real statue destruction (no probe); statue HP never spikes to ≈ −99,900.
5. **Variance:** outcomes + durations **vary** across the 20 matches (not 100% identical); **both** Victory and Defeat observed.
6. **Agency:** player train/advance decisions change the result (demonstrated).
7. **Stability/perf:** unchanged or better (no crashes; fps not regressed materially).

## 7. New GATE-1 pass criteria (seeded from roadmap §13 1.1–1.5 + GATE 1 FUN)
A **PASS** requires ALL of:
- **(A) Two-sided economy:** both sides run mine→train→push; AI keeps ≥1 miner all match and `gA` is non-flat.
- **(B) Two-sided army:** both sides field a growing force (AI > 1 unit sustained).
- **(C) Real combat:** unit-vs-unit engagement occurs (corrected `Engaged>0`) and units die from combat; a front line reads.
- **(D) Organic resolution:** matches end because a statue is destroyed by real combat damage — **no probe**.
- **(E) Variety:** across ≥20 matches, outcomes and durations vary; **both** Victory and Defeat occur.
- **(F) Agency:** player decisions (composition / when to push) measurably affect outcomes.
- **(G) Legibility/pacing:** the battle is readable and engaging from first contact (time-to-first-combat measured,
  not balance-tuned).
Anything short of all of (A)–(F) (G measured) = **FAIL** (no conditional wording).

## 7b. Phase-B revision (folded from `GAMEPLAY_RECOVERY_REVIEW.md` — 4 reviewers, unanimous APPROVE_WITH_CHANGES)
These code-verified corrections **supersede** the relevant text above and are binding on implementation:
1. **RC-1 mechanism (corrected):** in-frame order is **BasicAI → AIScheduler → SquadAI**; the race is a
   **cooldown-bake asymmetry** (`AICommander.ReevalCooldown=1.0` gates BasicAI ~1 s; `SquadAIState.ReevalCooldown=0`
   lets SquadAI buy a ~95 g combat unit on frame 1). The fix is unchanged (gate SquadAI's combat enqueue behind the
   miner floor). BasicAI is the **sole opening-miner authority**; a ~1 s "neither enqueues" window is expected —
   **acceptance adds: miner #1 queued within ~1–2 s.**
2. **`TargetMiners`** (`BasicAI.cs:63`) promoted `private`→**`internal`** (single shared source); **no new numeric literal.**
3. **`MinerFloorSystem` = PLAYER side (team 0) ONLY**; BasicAI remains sole AI-side miner authority (no double-enqueue).
4. **RC-5 (CRITICAL):** also add **`MoveDestination{Active=0}`** to every spawned non-miner combat unit in
   `SpellSummon.SpawnFromCatalog` (same ECB) — `FormationSystem` writes the override only `if (hasDest)`, so
   `FormationMember` alone is inert.
5. **RC-5 both sides:** stamp `FormationMember` on **AI and player** combat units; build a **player `SquadFormation`**;
   assign SquadId/Slot by **round-robin** at spawn (mirror `RowCursor`, `Training.cs:186-199`).
6. **RC-3(b) halt location (corrected):** the **auto-target** branch already halts at range (`Combat.cs:128-129`);
   the bug is the **`Active==1` override branch** (`Combat.cs:100-118`). Put the in-range halt **there** so it
   covers SimAiDriver advance, HUD advance, AND RC-5 formation slots. Re-test RC-3 acceptance after RC-5 (scaffold off).
7. **RC-5 auto-demo split (device-campaign critical):** keep **auto-TRAIN always on** (device + CI; the only
   unattended player army source); gate **only auto-ADVANCE/push** to make push-timing organic/manual (agency).
8. **RC-6 (honest reframe):** the structural distance-closer is the **per-unit `MoveSpeed` no-target
   march-to-contact fallback** (no new constant); anchor-step / march-speed / spawn / map levers are **BALANCE →
   DEFERRED**. RC-6 = structural fallback + documented defer (not "march shortens dead time").
9. **Criterion (E) variance (reframed):** the sim is **deterministic (no RNG)** → identical inputs give identical
   matches (correct). Variance + both-outcomes must come from **varied player decisions**, so the 20-match campaign
   will **vary player strategy** (push timing / composition) to show different decisions → different outcomes
   (jointly evidencing agency (F) + variety (E)). **Owner flag:** the PASS bar's "variety" is input-driven, not RNG.

## 8. Stop condition
This plan is **design only**. Implementation begins **only after Phase B review approval** (now obtained:
unanimous APPROVE_WITH_CHANGES, changes folded in §7b). The program stops
after `GATE1_REVALIDATION_REPORT.md` (PASS/FAIL). No GATE 2, no roadmap Phase 5, no future-research, no new
features/content/assets/monetization, no balance tuning.
