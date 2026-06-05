# BULWARK — Project State Analysis (Asset Migration Track · Phase 0)

**Date:** 2026-06-05 · **Track:** Asset Migration / Presentation Pass (NOT Phase 5). **Purpose:** capture the
exact current state of the project before any asset import, and **document (not fix)** the deferred gameplay
bugs. **Rules honored:** no roadmap/canon/balance change; no gameplay-bug fixes; presentation-layer only.

---

## 1. Current project state (one line)
BULWARK is a **functionally-complete-but-visually-debug** Unity 6 ECS/DOTS RTS-lite that **runs a full
mine→train→push→topple match on a real device** — but it renders only as **debug primitives + IMGUI overlays**
and its **GATE 1 (FUN) verdict is FAIL** (AI economy collapse → not a contested fight). This track adds a
**presentation layer** (temporary placeholder art + menus/HUD) without touching gameplay.

## 2. Completed phases (0–4) and gates
| Phase | Scope | Status | Gate |
|---|---|---|---|
| 0 | ECS spike / core loop spine | COMPLETE | — |
| 1 | Economy, training, targeting, combat, statue, AI (single front, 2 factions) | COMPLETE | GATE 1 OPEN (now FAIL, see §10) |
| 2 | Tactical depth: terrain, formations, counter matrix, positional, spells, commanders, squad AI/scheduler | COMPLETE | GATE 2 DEFERRED |
| 3 | Meta integration hooks (upgrades bake, server-owned, cap-clamped) | COMPLETE (scaffold) | GATE 3 DEFERRED |
| 4 | Monetization shell (battle pass, chests, shop, cosmetics — fair, see-what-you-buy) | COMPLETE | GATE 4 PASS (static) |

CI is GREEN end-to-end (EditMode/PlayMode compile + Android IL2CPP build). App installs, launches, and runs on
the Xiaomi Redmi Note 11R (Android 13, arm64-v8a).

## 3. Completed pre-Phase-5 GATE-1 validation track (V0–V2)
- **V0 Visualization** (`reports/prephase5/PHASE_V0_REPORT.md`): `SimProxyRenderer` — ECS entities render as
  team-colored **primitive proxies** (blue/red capsules = units, yellow cubes = mines, gray cylinders = statues),
  orthographic camera. Device-validated.
- **V1 Playability** (`PHASE_V1_REPORT.md`): `SimPlayerHud` — IMGUI gold/unit-buttons/queue + player training +
  attack-move → **first observable combat** on device.
- **V2 GATE-1 validation** (`GATE1_VALIDATION_REPORT.md`): full match resolves to Victory; **GATE 1 = FAIL** —
  AI economy collapse → walkover. A re-gate (build `fda1c95`) fixed the *identified* counting bug + a roster bug;
  the walkover became a *two-sided* fight but **still FAIL** (both economies collapse → tiny-army stalemate). See §10.
- **PREPHASE5_TRANSITION_REPORT.md**: every blocker to date was wiring/control, not broken sim logic.

## 4. Existing gameplay systems (ECS `ISystem`, `SimulationSystemGroup`, `Assets/_Game/Sim/Systems/`)
Canonical tick order (`SimSystemOrder.cs`): MiningSystem → TrainingSystem → InfluenceMapSystem →
(SpellSlotCooldown, CommanderAbility, AIScheduler, SquadAI, SpellCast) → PossessControl → FormationSystem →
TargetingSystem → BasicAISystem → MovementSystem → (Facing, Terrain, Positional, TelegraphResolve) → CombatSystem
→ StatusEffectSystem → StatueDamageSystem → MatchFlowSystem. All run between `SimPhaseBegin`/`SimPhaseEnd` anchors;
MatchFlow freezes the work set on a decided outcome. **These are unchanged by this track.**

## 5. Rendering systems
- **URP 17.0.3**; **`com.unity.entities.graphics` is NOT in the manifest** (CI-unresolvable; deliberately
  deferred). ECS entities therefore have **no native render bridge**.
- The ONLY renderer is **`SimProxyRenderer`** (`Assets/_Game/Bootstrap/`, removable MonoBehaviour, auto-spawned):
  reads the ECS world read-only and mirrors each entity as a `GameObject.CreatePrimitive` (Capsule/Cube/Cylinder)
  at `(Position.x, Position.y, 0)`, tinted by team via `MaterialPropertyBlock`, framed by an orthographic camera.
- **Presentation-import seam:** swap the primitive in `SimProxyRenderer.CreateProxy` for a `SpriteRenderer`
  (URP 2D) — no ECS change, no `entities.graphics`, no package re-resolution.

## 6. UI systems
- **No production UI.** Two **IMGUI debug** MonoBehaviours: `SimDebugOverlay` (MatchState/Gold/Units/Miners/
  Statue stats + radar) and `SimPlayerHud` (gold display, per-unit train buttons, queue, ADVANCE, pause; affordable-
  gating). Plus `SimAiDriver` (AI advance + victory-latch probe). All temporary, removable, auto-spawned.
- **No uGUI Canvas, no menus, no splash/loading/victory/defeat screens, no fonts/icons/buttons art.**

## 7. ECS architecture
- Components in `Assets/_Game/Sim/Components/` (Position float2, Health, Team{0 player/1 AI}, UnitTag, MinerTag,
  MiningState, MineNode, GoldStore, StatueTag/StatueState{Phase}, AICommander, TrainQueueTag/TrainOrder,
  Targeting, AttackState, MoveDestination, MatchState{MatchOutcome}, etc.). Default ECS World; entities live on a
  2D plane (z=0). Player=Team 0, AI=Team 1.
- `BattleBootstrap` (MonoBehaviour in MainScene **root** — moved out of the SubScene earlier this session) builds
  the world at `Start()` from authored Data assets (UnitDef/MapDef/BalanceConfig/SpellDef/CommanderDef).

## 8. Current scenes & prefabs
- **Scenes:** `Assets/MainScene.unity` (root = Camera + `BattleEnvironment` SubScene holder + `Bootstrap`
  GameObject) and `Assets/MainScene/BattleEnvironment.unity` (now an empty SubScene — bakes nothing).
- **Prefabs:** effectively **none** for presentation — all visuals are primitives created at runtime by
  `SimProxyRenderer`. `Assets/_Game/Art/` is documentation hooks only (no art).
- Active map: **`map_openfield`** (2 contestable mines, capacity 3 each, yield 5).

## 9. Blockers / integration constraints / technical debt
- **Constraints (binding for this track):** presentation-layer only — no ECS/sim/balance/AI/economy change; no
  GATE-1 bug fixes; reuse the `SimProxyRenderer` seam (SpriteRenderer proxies, **not** `entities.graphics`); keep
  scaffolds removable; **ripped assets are TEMPORARY dev placeholders** (© their studios — must be replaced before
  any release; cosmetic-safety/silhouette/faction-color readability still applies).
- **Tech debt:** debug-IMGUI UI (no uGUI); no prefabs/atlases; `entities.graphics` deferred; squad→`FormationMember`
  pipeline unterminated (units move only via control-layer drivers); debug auto-demo/AI-driver stand in for real
  input/AI; one-shot miner training in the demo.

## 10. ⚠️ DEFERRED GATE-1 GAMEPLAY ISSUES — DOCUMENTED, **NOT FIXED** (preserve as-is)
> **All three are DEFERRED UNTIL AFTER THE PRESENTATION PASS. This track does NOT touch them.**

1. **BasicAI vs SquadAI conflict.** Two AI systems append `TrainOrder`s to the same Team-1 queue —
   `BasicAISystem` (economy-first) and `SquadAISystem` (Phase-2.6, `SquadAI.cs:560` via the Eco axis). An expensive
   SquadAI combat order can reach the FIFO **head**, drain `GoldAI 100→5`, and **block the cheaper miner behind it**
   → the AI never establishes mining (`MinersAI=0`, `GoldAI=5` flat on device). The two layers are uncoordinated.
   *Status: DEFERRED — do not fix.*
2. **Miner targeting issue.** `TargetingSystem` (`Targeting.cs` `WithAll<UnitTag,Health>`) does **not exclude
   `MinerTag`**, so miners auto-acquire targets, walk into combat, and **die**. On device the player economy rose
   (`GoldP→81`, 2 miners) then **collapsed** (`Miners 2→0`, `GoldP→0`) when its miners were killed.
   *Status: DEFERRED — do not fix.*
3. **Miner replacement issue.** When miners die the economy does not recover: the AI doesn't re-establish miners
   (see #1), and the debug auto-demo trains player miners only once (`SimPlayerHud._autoMinersQueued` one-shot).
   No system maintains a live miner floor against attrition. *Status: DEFERRED — do not fix.*

**Net GATE-1 status: FAIL (deferred).** The presentation pass changes none of this — it makes the existing
(imperfect) match *look* like a real game; the FUN re-gate remains a separate, later gameplay task.

## 11. What this track will/won't do
- **Will:** add a temporary presentation layer — placeholder sprites for units/statue/mine/environment (via the
  `SimProxyRenderer` seam), a uGUI menu/HUD/victory-defeat flow, fonts/icons/buttons/audio — so the app launches
  and plays like a recognizable game. Discovery → inventory → mapping → gap → architecture → risk → safe import →
  device validation, per the phase plan.
- **Won't:** change any ECS system, balance number, AI logic, economy, or canon; won't fix the §10 deferred bugs;
  won't start Phase 5; won't ship ripped assets (they are flagged placeholder-only throughout).
