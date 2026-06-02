# BULWARK — Phase 2 Final Report

**Authorization:** [ADR-1-001](../../docs/adr/ADR-1-001-conditional-phase2-authorization.md) —
Conditional, AUTHOR-ONLY authorization prior to GATE 1 runtime evaluation. No runtime
claims; all runtime-dependent validations DEFERRED; GATE 1 remains OPEN; stop at GATE 2.

## 1. Phase Summary
Phase 2 (Tactical Depth / Vertical Slice) was authored end-to-end: the tactical layer that
distinguishes BULWARK from the original — **terrain + positional flank/back**, the **full 5×4
type×armor counter matrix + formations**, the **second faction (Ashen Horde, 6 units)** with the
§5.2 asymmetry, the **draft-3 spell system** (12 spells, synergy, telegraph+counter), **2 capped
commanders**, a **layered AI with a budgeted scheduler + multi-axis difficulty**, and **3 maps** —
all feeding the SAME §4 combat core authored in Phase 1 (no fork). Both rosters are now at their
canonical 6 (Iron Pact completed with Battlemage + Ironclad; Ashen Horde added) → the §H "12 units".

Built by an orchestrated pipeline: 6 system modules **authored against a fixed ECS contract →
adversarially canon-verified → repaired**; the spell module was caught returning placeholder stubs
and **re-authored in full** (the verifier had been fooled — flagged in §6); then a **cross-module
integration pass** applied every deferred edit (terrain Cover/Choke into the chain, one shared
counter-matrix lookup, status effects honored by Movement/Targeting/Combat, per-unit difficulty),
wired all 26 systems into a cycle-free update order, and rebuilt the bootstrap for two factions +
maps + spells + commanders. Static verification: **zero duplicate type declarations, all files
brace-balanced, all shared symbols resolve, every update-order reference resolves.**

**Honest status (ADR-1-001):** no Unity 6 toolchain/device/playtest — the code is **authored, not
compiled or run**. GATE 2 (external playtest) and all runtime/perf checks are **DEFERRED**, not PASS.

## 2. Work Completed   (Done = authored, integrated & canon-verified; runtime validation DEFERRED)
| Sub | Objective | Status | Evidence (§ref) |
|---|---|---|---|
| **2.1** | Terrain (high ground/choke/cover/hazard) + positional flank(1.5)/back(2.0) | **Done (authored)** | [OBS] `Sim/Systems/Terrain.cs` (FacingSystem, TerrainSystem→TerrainFactor + Hazard DoT, PositionalSystem→flank/back geometry). Cover(target DefenseMult)+Choke(MoveMult) folded into Combat/Movement. §4, §11 |
| **2.2** | 5×4 type×armor matrix + formations (Line/Tight/Loose) | **Done (authored)** | [OBS] `Data/Balance/CounterMatrix.asset` (full 20 cells), `Sim/Systems/CounterMatrix.cs` (one shared `Lookup`, now the sole matrix read in Combat+Spell), `Sim/Systems/Formation.cs` (Line/Tight/Loose layouts, facing). §4 |
| **2.3** | Second faction Ashen Horde (6 units), §5.2 asymmetry | **Done (authored)** | [OBS] `Data/Units/Ashen/*` (Miner/Raider/Slinger/Hexcaster/Razorbeast/Houndmaster — Flanker present, no dedicated Frontline; Razorbeast holds the line) + Iron Pact completed (Battlemage/Ironclad). Faster/cheaper/squishier swarm vs disciplined/shielded. §5.1, §5.2 |
| **2.4** | Spell system: pool 12, draft 3, synergy, telegraph+counter | **Done (authored)** | [OBS] `Sim/Systems/Spell.cs` (Catalog, Cast→Telegraph→Resolve by category, StatusEffect tick, `StatusQuery` shared helper), `Control/DraftAndSpellInput.cs` (draft+cast shell, §12), 12 `Data/Spells/*` — **every spell telegraphTime>0 + counterNote**; Chilled→Shatter ×2 synergy. §5.3 |
| **2.5** | 2 commanders (1/faction), 1 active+1 passive, ≤10–15% | **Done (authored)** | [OBS] `Sim/Systems/CommanderAbility.cs` (active Rally/WarCry, passive Quartermaster/Bloodthirst, **hard clamp to PowerBudgetPct**, ranked-normalization stub), 2 `Data/Commanders/*` (budget 0.12/0.13). §5.5, §6 |
| **2.6** | Squad AI + budgeted scheduler + multi-axis difficulty | **Done (authored)** | [OBS] `Sim/Systems/AIScheduler.cs` (round-robin N agents/frame, frame-stable), `Sim/Systems/SquadAI.cs` (O(1) posture utility above the P1 commander stance; reads `DifficultyAxes` Eco/Aggression/Stats/SpellAccess; reuses shared combat). §4 |
| **2.7** | 3 maps (terrain layouts) | **Done (authored)** | [OBS] 3 `Data/Maps/*` (Open Field / Choke Pass / Ridgeline — all 4 terrain kinds, contestable miner-capped mines, statues at ends), loaded by `BattleBootstrap`. §5.4, §11 |
| **Art** | §6 cosmetic-safety hooks (locked silhouettes) | **Done (doc hook)** | [OBS] `Assets/_Game/Art/README.md` (locked silhouette/size/timing/VFX-readability/faction-color; outfit-class mutables). Art production DEFERRED. §6 |

## 3. Files Created
**Systems:** `Sim/Systems/{Terrain,Formation,CounterMatrix,Spell,CommanderAbility,SquadAI,AIScheduler}.cs`, `Control/DraftAndSpellInput.cs`.
**Contract:** `Sim/Components/Phase2Components.cs`.
**Schemas:** `Data/Schemas/CommanderDef.cs` (+`.meta`), `Data/Schemas/MapDef.cs` (+`.meta`), `Data/Schemas/SpellDef.cs.meta`.
**Data (26):** `Data/Units/IronPact_{Battlemage,Ironclad}.asset`, `Data/Units/Ashen/Ashen_{Miner,Raider,Slinger,Hexcaster,Razorbeast,Houndmaster}.asset`, `Data/Balance/CounterMatrix.asset`, `Data/Spells/*.asset` (12), `Data/Commanders/*.asset` (2), `Data/Maps/*.asset` (3), `Data/Units/README.md` (updated).
**Art + governance:** `Assets/_Game/Art/README.md`, `docs/adr/ADR-1-001-...md`, this report.

## 4. Files Modified
- `Data/Schemas/CombatTypes.cs` — added `Faction`/`StatusKind`/`SpellCategory`/`TargetShape`/`TerrainKind` enums.
- `Data/Schemas/UnitDef.cs` — added `faction` field. `Data/Schemas/SpellDef.cs` — extended to the full draft-spell schema.
- `Sim/Systems/Combat.cs` — Cover (target) + Raged into the §4 chain; stun gate; Chilled/Hasted/Choke move-scaling; switched to the shared `CounterMatrix.Lookup` (deleted the private dup).
- `Sim/Systems/Targeting.cs` — stunned units skip re-acquire.
- `Sim/Systems/Training.cs` — spawn adds `Difficulty`+`StatusEffect`; summon shares `SpellSummon.SpawnFromCatalog`.
- `Sim/Systems/SimSystemOrder.cs` — wired the full Phase-2 update order; `UnitCatalogTag` gained `Team` (per-faction catalogs).
- `Bootstrap/BattleBootstrap.cs` — rebuilt for 2 factions, map load, full matrix, spell catalog/inbox/loadout/draft, commanders, difficulty axes, scheduler, squad AI.
- **No canon doc modified** (the five `report/*.md` are untouched; verified).

## 5. Validation Results
**Static/authoring validation (executed here):**
- **Canon compliance:** PASS — adversarial review per module + integration audit; only the §5 roster (12 units / 12 spells / 3 maps / 2 commanders); ONE §4 combat core (no fork — verified the single `dmg = ...` expression); commander magnitudes hard-clamped ≤ `PowerBudgetPct` (≤0.15, §6); every spell `telegraphTime>0` (no un-counterable spell); AI uses the budgeted scheduler; no meta/economy persistence, monetization, commander collection, determinism, or 3rd faction.
- **Integration coherence:** PASS — 0 duplicate type declarations; all files brace-balanced; shared symbols (`StatusQuery.*`, `CounterMatrix.Lookup`, `SpellCatalog*`, `SpellSummon.*`, `CommanderRuntime`, per-team `UnitCatalog`) resolve; the 26-system order is cycle-free (topologically sorted) with every `[UpdateBefore/After]` target present.
- **§12 boundary + perf rule:** STATIC PASS — sim in ISystem; draft/cast input is MonoBehaviour writing data; AI/targeting O(1)/agent under the budgeted scheduler; no all-pairs scans.

**Runtime validation (cannot execute here — DEFERRED):**
| Check | Result |
|---|---|
| terrain changes outcomes readably; counters teachable | **DEFERRED** |
| factions asymmetric yet fair; every spell counterable in play | **DEFERRED** |
| commanders within budget in play; AI smart + frame-stable; 3 maps load | **DEFERRED** |
| perf within budget under the scheduler | **DEFERRED** |
| **GATE 2: external playtest — ≥~40% session-2 return AND majority "readable & fun"** | **DEFERRED** — requires a build + external testers; not asserted |

## 6. Known Issues
- Entire tree **authored, not compiled** — expect first-compile fixups in Unity 6 (Entities 1.x API drift, `.meta` generation, Burst constraints).
- **Spell module first-pass was a placeholder stub** (the author returned "see repo file" comments; the per-module verifier was fooled). Caught in the cross-module census and **re-authored in full** (`Spell.cs` 1054 lines). Lesson logged: verify file substance, not just contract-name usage.
- **`FormationMember` assignment is not yet wired** — `FormationSystem` idles until a unit→squad membership step runs; the bootstrap bakes `SquadFormation`/`SquadAIState` but not membership. Formations are authored but won't engage until this DEFERRED spawn-layer hook is added.
- **Commander + spell same-kind buff stacking** — each written status entry is clamped ≤ budget, but `StatusQuery` multiplies every matching `Raged`/`Hasted` entry, so a commander buff + a spell buff of the same kind stack multiplicatively; the commander-attributable fraction is not airtight-bounded. **§6 fairness open question → needs an LSD decision** (per-entry vs per-unit-from-commander cap) before GATE 2 sign-off. Not a current canon violation (each entry is within budget) but flagged.
- One-tick terrain latency in the Choke move-read (steady-state fine). Provisional per-`TerrainKind` effect table lives in the bootstrap (flagged LSD-owned) pending migration into `MapDef`.
- All unit/spell/commander/terrain/matrix **values are PROVISIONAL** (LSD-owned, §16), to be tuned by playtest/telemetry; not asserted canon (§15.6).

## 7. Risks
- **Depth not readable / not fun (GATE 2, critical)** — unprovable on paper; needs a build + external playtest. Mitigations authored (telegraphs, clarity, counter matrix, formations) but unvalidated.
- **Compounding validation debt** — GATE 1 still OPEN (ADR-1-001) + all Phase-2 runtime/perf/playtest checks DEFERRED. If GATE 1 fails/pivots, depth layered on the core may need rework (accepted risk, ADR-1-001).
- **Combinatorial balance** — 12 units × 5×4 matrix × terrain × 12 spells × 2 commanders is a large space; provisional values only. Telemetry/RC tuning intended (do not over-tune now, §I).
- **Commander power creep** — mitigated by the hard ≤15% clamp; the stacking nuance above must be closed by LSD.

## 8. ADRs Raised
- **ADR-1-001** (Accepted) — conditional author-only Phase-2 authorization + accepted risk; **linked at the top of this report**.
- No new ADR was required during authoring (no canon deviation; all mechanics trace to §4/§5/§6/§11/§13). The commander-buff-stacking item (§6) is flagged for an **LSD decision/ADR before GATE 2 sign-off**; per ADR-1-001 §9 it is documented here rather than silently resolved.

## 9. Recommendations
1. **Open `bulwark-clean` in Unity 6 LTS**, first-compile pass, generate `.meta`, wire a Scene/SubScene around `BattleBootstrap` and assign the inspector refs (map, both factions' `UnitDef[]`, `spellPool`, commanders, `CounterMatrix`).
2. **Add the `FormationMember` assignment hook** (unit→squad membership) so formations engage.
3. **LSD: resolve the commander+spell same-kind buff stacking** (cap commander-attributable buff fraction) → ADR + tune.
4. **Burn down the deferred validations** (GATE 1 fun verdict first, then the GATE 2 external playtest + perf-under-scheduler).
5. **Do NOT begin Phase 3** (meta/economy/server wallet) until GATE 2 passes the playtest bar — currencies/persistence stay forbidden until then (§J, §15).

## 10. Gate Status
- **GATE 2 (vertical slice — readable & fun playtest):** **DEFERRED** — the vertical slice is authored, integrated, and canon-verified, but the binding external-playtest bar (≥~40% session-2 return AND majority "readable & fun") + perf-under-scheduler require a build + testers this environment cannot produce. **Not PASS, not FAIL.** No playtest metric fabricated.
- **Authorization to proceed to Phase 3:** **WITHHELD** — pending (a) a build, (b) the deferred Phase-1 GATE 1 fun verdict (still OPEN, ADR-1-001), and (c) the GATE 2 playtest bar.
