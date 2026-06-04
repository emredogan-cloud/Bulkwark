# BULWARK — Placeholder-Art Integration Plan (advisory; presentation-layer only)

**Date:** 2026-06-04 · **Track:** pre-Phase-5 GATE-1 validation (parallel deliverable). **Status: ADVISORY
PLAN — no code, no canon change, no asset import in this document.** **Authority:** roadmap §"Visual philosophy"
(canon) + §12 (ECS sim / MonoBehaviour presentation boundary) + §6/§15 (cosmetic-safety, no invented content).
**Inputs:** `future/000-assets-roadmap/BULWARK_ASSET_MASTER_AUDIT_TR.md` (advisory research, summarized),
`Assets/_Game/Art/README.md`, the V0/V1 renderer (`SimProxyRenderer.cs`). **This plan does NOT adopt `/future/`
content into canon** (per the future-research-track rule) — it only *references* the audit to inform placeholder
sourcing.

---

## 0. Goal
Replace the temporary GameObject **primitive proxies** (capsule/cube/cylinder) with **placeholder 2D art** so a
GATE-1 readability/fun read reflects real silhouettes — **without touching the ECS simulation** (§12). This is a
*placeholder* pass (pre-production-art), not the final art.

## 1. Approved art direction (canon — binding constraints)
- **Canon visual philosophy** (`report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` §"Visual philosophy"): *"Clean
  stylized 2D, bold readable silhouettes, **Spine skeletal animation**; readability over detail."* Shared
  skeleton per archetype + faction recolor is the content-velocity engine (§10), ~9 units / 18 spells.
- **Cosmetic-safety LOCKS** (`Assets/_Game/Art/README.md`, roadmap §6 INVIOLABLE): silhouette, unit size/hitbox,
  animation timing, ability-VFX readability, and **faction-color identity** may **never** change for power/clarity
  reasons. Faction palettes: **Iron Pact = steel/cobalt**, **Ashen Horde = ember/oxblood**. Damage-type VFX
  color code (Slash=steel, Pierce=white, Blunt=dust, Magic=purple, Fire=orange, Poison=green). Placeholder art
  must already honor silhouette + faction color so it does not mislead the GATE-1 read.
- **Addressable folder hooks already authored** (`Assets/_Game/Art/README.md`): `Art/Units/<Faction>/<Archetype>/`,
  `Art/Spells/<id>/`, `Art/Terrain/<kind>/`, `Art/Commanders/<id>/` — currently empty ("art DEFERRED").

## 2. Asset-audit summary (advisory `future/000-assets-roadmap`, in English)
- **Zero art exists today** (audit KB-6, confirmed by repo scan).
- **Pipeline fork (KB-1):** the audit's headline recommendation is a **2.5D low-poly 3D** path (orthographic
  camera; Synty / POLYGON / Quaternius free/paid packs). **This CONTRADICTS canon "Spine 2D"** and therefore
  **requires a GD/TA-approved ADR (§15/§16)** — *an agent cannot make that disposition change alone.* It is a
  **human decision gate.** This plan presents both and does **not** pick for the owner.
- **If canon stays (Path A = 2D Spine):** source from 2D-sprite/Spine-ready packs — **Kenney (CC0)** primary,
  OpenGameArt **CC0** backup (audit flags the license-diligence trap: avoid GPL/CC-BY-SA). Synty/Quaternius
  become *reference-only*.
- **Decision framework (audit Phase 4):** **BUY+KITBASH** units/env/props/VFX (shared archetype skeleton +
  faction reskin); **BUILD** only IP-critical (statues = 2 factions × 4 `StatuePhase` + shield overlay, faction
  crests, branded UI, music, portraits, telegraph timing); **GENERATE** cosmetic recolor tiers (palette/material
  /mask only — never silhouette).

## 3. Current rendering = the single integration seam
`Assets/_Game/Bootstrap/SimProxyRenderer.cs` is the **entire** renderer (removable, read-only of the ECS world,
§12). It keeps a `Dictionary<Entity,Proxy>`, runs 3 read-only queries (units = `UnitTag+Position+Team+Health`;
mines = `MineNode+Position`; statues = `StatueTag+StatueState+Position`), and per entity: `CreateProxy` (a
`GameObject.CreatePrimitive`) + `UpdateProxy` (position on the z=0 plane, scale by HP, tint a
`MaterialPropertyBlock` by team/kind). The camera is already **orthographic** (`ConfigureCamera`) — ideal for
sprites. It does **not** use `entities.graphics` (deliberately deferred; CI-unresolvable, and it would need
per-entity render components = a sim/spawn change).

**Key seam fact:** the renderer distinguishes only **Team** (blue/red) and **Kind** (unit/mine/statue), plus
`MinerTag`. Spawned unit entities carry **no per-archetype role component** (role lives only in the
`UnitSpawnStats` buffer / `TrainOrder.UnitIndex` at spawn). So per-archetype sprites need a small spawn-side
visual id (see Risks); the placeholder pass can start with team + `MinerTag` → 5 sprites.

## 4. Cleanest swap path (presentation-only, no sim change)
Add a **sibling removable MonoBehaviour `SimSpriteRenderer`** (or a flagged branch in `SimProxyRenderer`) that
reuses the exact same `Dictionary<Entity,Proxy>`, the same read-only queries, the same z=0 placement / HP-scale /
faction-tint, and the same orthographic camera — but `CreateProxy` builds a `GameObject` with a **`SpriteRenderer`
(URP 2D)** instead of `CreatePrimitive`. Faction tint maps to the same `_BaseColor`/sprite color. **No ECS
components added, no sim writes, no package re-resolution** if it stays on built-in `SpriteRenderer` (URP renders
sprites without `entities.graphics`). Deleting the file removes it 100% (the §12 guarantee). Toggle proxies↔sprites
with one flag for A/B readability comparison during GATE-1.

## 5. Phased steps + risks
- **Step A — ADR decision (HUMAN GATE):** resolve the KB-1 fork (canon 2D-Spine **vs** audit's 2.5D-3D). **Do not
  author unit art until resolved** — it determines whether sourcing is 2D sprite packs or Synty/Quaternius 3D.
- **Step B — render package:** add `com.unity.2d.sprite` (+ `2d.animation` if frame-animating). **Leave
  `entities.graphics` OUT** (keeps CI green — it previously took the build red). *Risk:* any package add can
  re-trigger CI resolution failure → gate on a clean resolve in a branch.
- **Step C — placeholder atlases:** author CC0 placeholder sprites under the existing `Art/Units/<Faction>/
  <Archetype>/` hooks; **one atlas per faction-set** (ASTC, SRP-batcher-friendly) for draw-call budget (audit
  Phase 11).
- **Step D — `SimSpriteRenderer`:** start with the 5 distinctions already on entities — **player-unit (blue),
  AI-unit (red), miner (`MinerTag`), mine, statue.** The **statue** is the highest-value placeholder: map
  `StatueState.Phase` (Intact/Cracked/Breaking/Destroyed) → 4 damage sprites + a shield overlay when
  `ShieldActive` — it's the win/lose object and the data already exists. **Defer per-archetype unit sprites**
  (Shieldman vs Crossbow vs Battlemage) to the production pass that adds a spawn-side visual id.
- **Step E — animation states (read-only, derived):** idle/walk from `Movement`/position delta; attack from
  `AttackState` (cooldown/target); death from the existing `CullStale` (entity disappears); the HP-drop flash
  already exists. No sim flag needed.

**Risks to record:** (i) `entities.graphics` CI instability — avoid for placeholders; (ii) per-archetype art
needs a small, removable **spawn-side visual-id `IComponentData`** (e.g. carry `TrainOrder.UnitIndex`/`RoleId`
onto the unit) — this crosses the strict presentation boundary, so keep it **data-only, §15.6-clean**; (iii)
overdraw/atlas size on mid-range devices (audit Phase 11 budgets); (iv) **cosmetic-safety** — placeholder
sprites must preserve locked silhouette + faction color so GATE-1 readability isn't skewed.

## 6. What this unblocks / what stays gated
- **Unblocks now (no human gate):** a `SimSpriteRenderer` using CC0 2D placeholders for the **5 current
  distinctions** + the **statue 4-phase** sprite — a real readability uplift over primitives, presentation-only,
  reversible.
- **Stays gated (human):** the 2D-Spine vs 2.5D-3D ADR (KB-1); production Spine rigging; per-archetype art;
  any `entities.graphics`/package adoption; final cosmetics tiers.

## 7. Cross-references
`reports/prephase5/PREPHASE5_VISUALIZATION_ROADMAP.md` (entities.graphics deferral rationale) · `PHASE_V0_REPORT.md`
(primitive renderer) · `Assets/_Game/Art/README.md` (folder hooks + cosmetic-safety) ·
`future/000-assets-roadmap/BULWARK_ASSET_MASTER_AUDIT_TR.md` (advisory audit, Phases 4/11).

**This is a plan only. No canon change, no `/future/` adoption, no asset import, no Phase 5. The 2.5D-vs-2D fork
is a human ADR decision.**
