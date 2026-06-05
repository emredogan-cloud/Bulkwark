# BULWARK — Production Presentation · Priority #5: Animation Discovery (Phase A)

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 Option A — NOT roadmap Phase 5).
**Scope:** audit extracted animation assets; determine what is recoverable; pick an achievable approach.
**Inputs re-read:** `MIGRATION-AUDIT/`, `reports/asset-migration/*` (ASSET_INVENTORY §A1, IMPORT_RISK §4),
extracted asset trees. **Licensing:** all source art is **© ripped → dev-only, replace before release.**

---

## 1. What exists (and its state)
| Animation source | Found | Usable this pass? |
|---|---|---|
| **Spine rigs** (skeletal: json + `.atlas` + page PNG) | ✅ Stick War Universal 4.1 (Swordwrath/Spearton/Magikill/Archidon/Miner skins) + Giant; Age of War Units_Roster/cavalry 2.x; Stickman 3.8 (74 sets) | ❌ **DEFERRED** — needs the **paid `spine-unity` runtime** (NOT in the manifest), and the three sets are **version-split** (4.1 / 3.8 / 2.x) so one runtime can't load all; atlas page-size mismatch (IMPORT_RISK §4) |
| **Pre-rendered unit frame-sheets** (idle/walk/attack/death frames) | ❌ **none** — units are **Spine-composed from per-part sprites** (helm/torso/arm/…), there are no baked per-frame unit animations | ❌ not available |
| **Frame-sheets / flipbooks** | ✅ only for **VFX/environment** (`explosion_spritesheet_3x3`, `lightning_spritesheet`, AoW bases flipbook) | ◑ not unit animation (VFX handled in Priority #4) |
| **Recoverable keyframe data** | Spine `.json` carries bone keyframes | ❌ requires the paid Spine runtime to interpret; re-assembling per-part sprites into frames = re-rigging (out of scope) |
| **Unity animation package** | ❌ no `com.unity.2d.animation` / Spine / Timeline in the manifest | — |

## 2. Required animation states → source
| State (brief) | Skeletal source (deferred) | **Achievable this pass** |
|---|---|---|
| idle | Spine `idle` | procedural gentle breathe/bob |
| move / walk | Spine `walk`/`run` | procedural walk-bounce + slight lean, when the proxy is moving |
| attack | Spine `attack` | procedural lunge/recoil on an observable damage event |
| death | Spine `death` | procedural fall (rotate + shrink + fade) on unit cull |
| cast | Spine `cast` | procedural pulse (API; triggered where observable) |
| **statue states** | crack-overlay sprites | already by HP-driven tint/scale + the Priority-#4 statue-damage VFX (phase-sprite swap = later) |
| **mine states** | — | static (occupancy = later) |

## 3. Decision — procedural sprite animation (this pass)
Because Spine is deferred (paid runtime + version split) and there are **no baked unit frame-sheets**, the framework
animates the **existing static archetype sprites procedurally via transform** (position bob, scale breathe/bounce,
rotation lean, alpha fade). This brings the battlefield to life — idle breathing, walking bounce, hit recoil,
death fall — **with no new assets, no paid runtime, and zero gameplay impact**, consistent with the code-driven
audio/VFX layers. **Skeletal (Spine) animation remains the future production path** (its own ADR + runtime
licensing decision).

## 4. Architecture plan (Phase B preview)
- **`AnimationManager`** (singleton) + a per-proxy **`SpriteAnimator`** component the renderer attaches to each
  unit proxy. `SpriteAnimator` runs in **LateUpdate** and modulates the renderer's per-frame base transform
  (position/scale/rotation/alpha) — no accumulation (the renderer resets the base each Update).
- **Read-only state inference:** moving (proxy position changed) → walk; stationary → idle; recent HP-drop
  (`NotifyHit`) → recoil; cull → `PlayDeath` (the animator orphans the proxy, plays the death tween, self-destructs).
- **No gameplay dependency, no ECS writes** — driven entirely by what `SimProxyRenderer` already observes.

## 5. Risks (carried to Phase D)
- **Spine deferred** → procedural only (units don't have skeletal limbs/attack swings) — accepted.
- **Transform-vs-renderer ordering** → LateUpdate-after-Update, base reset each frame (no runaway) — validate.
- **Death persistence** → the proxy must survive ~0.4 s after cull to play the death tween (orphaned by the
  animator; the entity is already gone in the sim — presentation-only).
- **Performance** → ~50 cheap `SpriteAnimator.LateUpdate`s (a few sin calls each) — mobile-safe; validate.
- **APK/memory** → **zero new assets** (procedural) — ~0 impact.

## 6. Verdict
No baked unit animation is recoverable and Spine is deferred, so the achievable, honest approach is **procedural
transform animation** of the existing sprites (idle/move/attack-react/death/cast), driven read-only. **Nothing
imported.** Proceed to Phase B (`AnimationManager` + `SpriteAnimator`) → C (read-only integration) → D (validation),
per `PRIORITY5_ANIMATION_FRAMEWORK_REPORT.md`.
