# ADR-2-002 — Commander/Spell buff-stacking resolution (§6 power budget)

- **Status:** Accepted (resolution adopted; implementation in Phase 3 integration)
- **Date:** 2026-06-03
- **Owners:** Lead Systems Designer (balance authority within canon, §16) + Technical Architect (impl)
- **Relates to:** ADR-1-001 / Phase-2 report §6 (the surfaced open question), roadmap §6 (commander cap), §5.3 (spells)
- **Required by:** ADR-2-001 ("Open ADR for commander/spell stacking resolution")

## Problem
Phase 2's `CommanderAbilitySystem` clamps each *written* status entry to ≤ `PowerBudgetPct`
(≤10–15%, §6). But `StatusQuery` multiplies **every** matching `Raged`/`Hasted` entry on a
unit, so two same-kind buffs stack multiplicatively. Two cases:
1. **Commander active + commander passive** of the same kind → the *commander-attributable*
   fraction on a unit can exceed the §6 budget. **This is a fairness leak** (§6 is INVIOLABLE).
2. **Commander buff + spell buff** of the same kind (e.g., commander Rally + spell Rage) →
   combined effect exceeds the commander budget *number*, but the spell portion is the §5.3
   tactical layer, not commander power.

## Decision
Resolve by **scoping the §6 budget to COMMANDER-sourced effects only, and bounding their
combined contribution per unit** — spell-sourced buffs are a separate, intentionally-stacking
tactical layer bounded by their own telegraph/counter/cooldown (§5.3), not by §6.

Concretely:
1. **Tag the source** of each buff: add `StatusSource Source { Spell, Commander }` to
   `StatusEffect` (default `Spell`). `CommanderAbilitySystem` writes `Commander`; `SpellSystem`
   writes `Spell`.
2. **Bound commander-attributable buffs**: when computing a unit's buff multiplier,
   `StatusQuery` sums/combines **commander-sourced** entries of a kind and **clamps their
   combined contribution to ≤ `PowerBudgetPct`** (the active+passive of one commander can never
   exceed the budget on any unit), then applies spell-sourced entries separately.
3. **Spell buffs remain uncapped by §6** (they are not commander power); their balance is held
   by cooldown/telegraph/counter and LSD tuning — consistent with §5.3 "no un-counterable spell".
4. **Ranked normalization** (the existing `RankedNormalized` hook) continues to clamp the
   commander-attributable contribution to the budget regardless, in ranked.

This keeps the §6 commander-power cap **airtight** while preserving the spell layer's design.

## Implementation plan (applied in the Phase-3 integration pass)
- `Phase2Components.cs`: add `enum StatusSource { Spell = 0, Commander = 1 }` and
  `StatusSource Source` to `StatusEffect` (default 0 = Spell, so existing writers are unchanged
  in meaning).
- `Spell.cs` (`AddOrRefreshStatus`): accept a `StatusSource` (default `Spell`); `StatusQuery`
  gains `CommanderBuffMultiplier(buffer, budget)` (clamped, commander-sourced only) and keeps the
  spell-buff multiplier separate; `Combat`/`Movement` combine `spellMult * commanderMult`.
- `CommanderAbilitySystem`: write its statuses with `Source = Commander`, pass the unit's
  `PowerBudgetPct` so the clamp is data-driven.
- No canon edit; this realizes the existing §6 cap. Runtime behavior DEFERRED (no Unity).

## Consequences
- Closes the §6 fairness leak; the commander-attributable buff on any unit is provably ≤ budget.
- Spell stacking remains a deliberate tactical lever (counterable), not P2W.
- This ADR's implementation is part of Phase 3; it is **not** a canon change (it enforces §6).
