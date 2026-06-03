# Design note — FormationMember wiring plan

**Status:** Plan (authored; implementation is a small follow-up hook — NOT done in Phase 3,
which is meta/economy; this satisfies the ADR-2-001 "document the FormationMember wiring plan"
requirement). Roadmap §13 2.2 (formations), §4 (control), §12 (ECS boundary).

## Problem
Phase 2 authored `FormationSystem` (Line/Tight/Loose) and the bootstrap bakes `SquadFormation`/
`SquadAIState` singletons, but **no step assigns units to squads** — `FormationMember{SquadId,Slot}`
is never written, so `FormationSystem` (which `RequireForUpdate<FormationMember>`) idles. Formations
are authored but do not yet engage.

## Plan (membership assignment) — two complementary writers, both §12-clean
A unit gains `FormationMember` from exactly one of these, at/after spawn:

1. **AI side — at spawn (TrainingSystem / SquadAI):**
   - When `TrainingSystem.SpawnUnit` deploys an AI unit, assign it to the AI side's *current
     active squad* for its role/wave: `FormationMember{ SquadId = <active AI squad>, Slot = next }`.
   - `SquadAISystem` owns the AI squads it already creates (`SquadAIState`/`SquadFormation`): it
     allocates a `SquadId`, picks the `FormationType` from posture (Advance→Line, Hold→Tight,
     Retreat→Loose), and sets the `Slot` ordinal as members join (cap squad size; spill to a new
     squad). This is O(1) per spawn — no scan.

2. **Player side — from control selection (BattleInput, §12 MonoBehaviour writes data):**
   - A box/tap selection forms a transient `Squad{Id}`; a "set formation" UI affordance writes
     `SquadFormation{Type}` and stamps the selected units with `FormationMember{SquadId, Slot}`
     (slot = selection order). Possessed/manually-ordered units keep manual override precedence
     (PossessControl) — formation only drives un-commanded members (already honored by
     `FormationSystem` skipping manual/possess units).

## Slot geometry (already in FormationSystem)
`FormationSystem` computes each member's world offset around `SquadFormation.Anchor` by `Type`
(Line = wide/shallow across rows; Tight = clustered; Loose = dispersed) and writes
`MoveDestination{Value, Active=1}` for non-manual members. No change needed there — only the
membership write is missing.

## Why deferred out of Phase 3
Phase 3 is meta/economy/modes; touching the battle squad-spawn path is combat-layer (Phase 2)
work. The hook is small and localized (a few lines in `TrainingSystem.SpawnUnit` + `SquadAISystem`
squad allocation, plus a player "set formation" affordance in `BattleInput`). It is scheduled as a
**Phase-2 follow-up before the GATE 2 runtime playtest** (formations must engage for the depth
playtest to be valid). Tracked here and in the Phase-2 report §6 / Recommendations.

## Validation (DEFERRED — no Unity)
Once wired: spawn AI units → confirm they assume Line/Tight/Loose around the squad anchor and
reposition on posture change; player selection + set-formation → members hold formation until
manually commanded. Runtime validation DEFERRED with the rest (ADR-1-001/-2-001).
