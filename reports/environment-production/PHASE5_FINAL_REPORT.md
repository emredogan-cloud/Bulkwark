# PHASE 5 — ENVIRONMENT PRODUCTION — FINAL REPORT
**Project:** Stick Empire Rise *(codename Bulwark)* · **Date:** 2026-06-09
**Scope:** turn the validated Phase‑4 parallax battlefield into a **living world** — faction statue state machines, resource‑node reactions, faction keeps/barracks ambience, and biome‑aware scatter props — that react to simulation state while preserving **complete ECS isolation (§12)**. Environment observes; it never controls.

---

## 1. Architecture overview

One new removable presentation MonoBehaviour, **`BattlefieldEnvironment.cs`** (boots `AfterSceneLoad`, active only `PresentationState.InMatch`), reads ECS **read‑only** (`StatueState`, `MineNode`/`Position`, `MinerTag`/`Position`) + the presentation camera and renders four pooled systems under one `ENV_Root`. It loads 11 silhouette sprites once (`env_*` — barracks/watchtower/banner/crack + 7 props) via the same device‑safe `UnityWebRequest` pipeline; all FX (auras, smoke, glints, debris, torch) are procedural `UiTex` (no extra texture cost). **No realtime lights / shadows / post** (painted + additive). It writes nothing to the sim.

A tiny pooled `DebrisBit` mover (gravity + fade) drives statue debris, ore bursts, and chimney smoke.

## 2. Faction statues — 4‑stage state machine

Per statue: `frac = Health / MaxHealth` (read‑only) → exactly four stages, no popping (continuous aura/crack lerps):

| Stage | HP% | Presentation |
|---|---|---|
| **1 IDLE** | 100–76 | Faction aura glow (blue Iron Pact / red Ashen Horde), slow pulse (1.1 Hz), full‑size; majestic/stable |
| **2 MINOR** | 75–36 | + hairline **cracks** fade in (env_crack overlay, alpha ∝ damage), aura dims, pulse 2.2 Hz |
| **3 CRITICAL** | 35–1 | cracks deepen, aura shifts **red** + fast pulse (5 Hz), rising **smoke** puffs (pooled, throttled), instability |
| **4 DESTRUCTION** | 0 | aura/cracks removed + a **debris burst** (10 pooled bits, gravity, fade ≤2.5 s); statue proxy culled by `SimProxyRenderer` |

Statue hits also drive the camera impact‑shake (Phase 4). Faction colour + side are derived from statue x‑position (left=blue, right=red) for team readability + distinct silhouettes.

## 3. Resource nodes

Per mine (`MineNode`): an **idle gold shimmer** (pulsing glint, low/non‑distracting). **Active harvest** is triggered **only by a confirmed miner observation** — a `MinerTag` unit within ~2.2 world units of the node — and emits a small **ore burst** (4 gold diamonds, gravity/fade), **throttled to ≤1 burst / 0.5 s per node**. It never reads or infers economy/harvest cycles; absence of an adjacent miner = idle.

## 4. Faction keeps / barracks

Behind each statue (x ± 3.0), a small civilization cluster: **barracks** + **watchtower** silhouettes (faction‑neutral stone) + a faction‑tinted **banner** + a **torch** glow + a **chimney smoke** source. Ambient behaviour: banner **flutter** (±6° sine), torch **flicker** (alpha pulse), chimney **smoke loop** (pooled puffs rising/fading). Placed once; `sortingOrder` keeps them behind the units (midground band). Subtle, non‑intrusive.

## 5. Scatter prop matrices (biome‑aware)

Props are placed deterministically along the lane (16 candidate slots) and **excluded from the central combat band (±18 % of the lane span)** + min‑spaced, with fore/back depth split, so they **never obscure units, projectiles, mining, statue, or HUD**. Allowed sets per biome:

| Biome | Allowed props |
|---|---|
| **Grasslands** | rock, spear, cart, log *(+ grass tufts from the Phase‑4 foreground)* |
| **Ashlands** | charred debris, rock, log |
| **Snowfields** | log (frozen), rock, debris |
| **Volcanic Wastes** | rock (lava), bone, debris |
| **Deadlands** | grave marker, bone, weathered debris |

Scatter rebuilds on biome change. *(Asset slots exist for the full directive lists — burnt banners, ice shards, ember piles, broken standards — as additional `env_prop_*` silhouettes dropped into the same per‑biome arrays.)*

## 6. Weather compatibility

Props are tinted by the active biome's weather profile (`WeatherTint`): **snow** → cooler/frost tones; **ash/volcanic** → desaturated + ember‑warm darkening; **dead** → grey. Integrates with the Phase‑4 pooled weather (embers/snow/ash) — no gameplay change.

## 7. Memory & performance

| System | Budget | This implementation |
|---|---|---|
| Statue systems | ≤2 MB | procedural FX + 1 shared crack sprite (~50 KB) — **well under** |
| Prop atlases | ≤6 MB | 11 silhouette PNGs (~0.5 MB total) — **well under** |
| Particle pools | ≤2 MB | procedural discs/diamonds, pooled (ore ≤24, smoke ≤8/statue) — **well under** |
| Smoke/ambient CPU | ≤0.5 ms each | pooled, throttled (chimney 0.4 s, statue‑smoke 12 % gated) |
| Environment total | ≤1.5 ms | 3 small ECS read queries/frame + pooled sprite updates |

No duplicated textures, no per‑frame allocations after pools warm (a per‑frame miner‑position temp array is the only allocation; can be cached next pass). *Static estimates; on‑device Profiler `≤1.5 ms` measurement pending the install unblock (§9).*

## 8. Validation evidence (Unity runtime)

Validated via the established **Linux standalone + auto‑match** pipeline (`LocalBuild.BuildLinux`, `ValidationAutoMatch`, `validate_standalone.py`) — device install is intermittently MIUI‑locked.

| Item | Status |
|---|---|
| Compile | **PASS** (Roslyn 66/0/0) |
| Runtime init | **PASS** — `[ENV] env sprites ready: 11/11` · `faction structures built` · `scatter built for biome 'grass'` |
| Statue IDLE aura (faction blue/red) | **PASS** (carried from Phase‑4 validation; same read) |
| Statue MINOR/CRITICAL/DESTRUCTION | **IMPLEMENTED** — auto‑match statues stay full‑HP, so damaged‑stage visuals are code‑verified (trigger on HP); not screenshot‑capturable without sim damage (which §12 forbids forcing) |
| Resource idle shimmer / harvest burst | **IMPLEMENTED** — shimmer captured; bursts depend on a miner reaching a node mid‑run |
| Faction keeps/barracks + ambience | **PASS** — barracks + watchtower + banner behind each statue (`rc_p5_linux/env_22.png`) |
| Scatter props (biome‑aware, out of combat band) | **PASS** — carts/logs/rocks along the lane; central combat band kept clear (`env_22.png`) |
| Weather tint | **IMPLEMENTED** (per‑biome) |
| ECS integrity (no sim regression) | **PASS** — presentation‑only; reads only; `[PROXY]` proxy counts unchanged |
| OOM | no new OOM on the standalone (PC RAM); device measurement pending |

## 9. Remaining blockers / next steps

- **On‑device capture + Profiler** (statue damage stages, perf `≤1.5 ms`, OOM): gated on the recurring MIUI "Install via USB" re‑lock — re‑enable in Developer Options, or continue on the standalone path.
- **Damaged‑stage screenshots**: require a statue at low HP; capturable from a real played match on device (the auto‑match keeps statues healthy). The stage logic is code‑verified and continuous (no popping).
- **Full prop lists per biome** (burnt banners, ice shards, ember piles, etc.): additional `env_prop_*` silhouettes into the existing per‑biome arrays — no code change.

## 10. Verdict

The battlefield is now a **believable world**: faction statues that read their own danger via a 4‑stage state machine, resource nodes that react to actual mining, faction keeps with living ambience, and biome‑aware scatter that respects gameplay readability — all **pooled, mobile‑budgeted, and §12‑isolated** (the ECS sim is untouched; environment only observes). This establishes the definitive environmental standard; authored art can replace any procedural/silhouette element in the same slots without code change.
