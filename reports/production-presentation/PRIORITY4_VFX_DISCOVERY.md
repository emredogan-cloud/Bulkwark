# BULWARK — Production Presentation · Priority #4: VFX Discovery (Phase A)

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 Option A — NOT roadmap Phase 5).
**Scope:** audit extracted VFX before importing; pick a curated set + a build plan. **Inputs re-read:**
`MIGRATION-AUDIT/`, `reports/asset-migration/*` (ASSET_INVENTORY §A3, IMPORT_RISK §5), extracted VFX folders.
**Licensing:** every texture below is **© ripped (Stick War / Stickman Master / Age of War) → THROWAWAY DEV
PLACEHOLDER ONLY, replace before release.**

---

## 1. Available VFX (extracted sets)
| Set | Folder | Content | State |
|---|---|---|---|
| Stick War | `VFX/` (355) | `blood1-14`, `ash`, `sparkle`, `glow`, lightning/heal/magic frames; `ArrowImpact`/`BarricadeDestroySmoke`/`BlazeCaughtFire` **as `.prefab.json`** | **textures OK; particle EMITTERS stripped** (IMPORT_RISK §5) |
| Stickman Master | `fx/effects/` (44) | `Smoke`, `glow`/`glow4`/`Glow_01`, `sparkle2`, `SparkExolosion2`, `fire_flame`, `LightningGlowTexture`, `explosion_spritesheet(_3x3)` | textures OK; sheets need slicing |
| Age of War | `VFX/` | `muzzleFlash`, `aura`, `shine`, `Generals_Glow` | textures OK |

**Reality:** the recovered VFX are **textures only — the emitter/particle-system settings are not recoverable**
(stripped). So the framework **rebuilds effects in-engine** from a few curated textures rather than restoring
emitters.

## 2. Mapping → required VFX slots
| Slot (brief) | Source texture (placeholder) | How produced | Observable read-only? |
|---|---|---|---|
| **hit flash** | *(none — reuse the renderer's existing white sprite-flash on HP-drop)* | tint, already in `SimProxyRenderer` | ✅ unit/statue HP-drop |
| **hit impact** | `vfx_impact` ← SW `sparkle` | brief spark sprite at the hit position | ✅ unit HP-drop |
| **death puff** | `vfx_puff` ← SM `Smoke` | expanding+fading smoke at death | ✅ unit cull |
| **statue damage** | `vfx_glow`/`vfx_spark` ← SM `glow` / SW `sparkle` | flash/glow at the statue on HP-drop | ✅ statue HP-drop |
| **spell cast / spell impact** | `vfx_glow` (cast) / `vfx_spark` (impact) | glow/burst at a position | ◑ **no clean read-only spell event today** → API provided, wired only where observable (see gaps) |
| **projectile trail** | `vfx_spark` streak | a quick streak between two points | ◑ **sim combat is instant-hit** → cosmetic only; API provided, not auto-wired this pass |

## 3. Curated set (4 textures, small)
`vfx_impact` (SW `sparkle`, 256²) · `vfx_blood` (SW `blood1`) · `vfx_puff` (SM `Smoke`) · `vfx_spark` (SM
`SparkExolosion2`) · `vfx_glow` (SM `glow`). Target: a few hundred KB total. (Final selection trimmed to keep the
APK small — see the framework report.)

## 4. Architecture plan (Phase B preview)
- **Code-driven, pooled sprite VFX** — a `VfxManager` spawns short-lived `SpriteRenderer` effects that animate
  scale + alpha over ~0.2–0.5 s, drawn above the battlefield. **No Unity ParticleSystem authoring** (avoids the
  stripped-emitter problem + URP particle-material complexity; deterministic, mobile-cheap, fully removable).
- **Own texture loader** — VFX PNGs in `Assets/StreamingAssets/bulwark_vfx/`, runtime-loaded via
  `UnityWebRequestTexture` (same robust path as the sprite loader; `unitywebrequesttexture` already present).
- **Read-only integration** — driven from `SimProxyRenderer`'s existing read-only observations (unit HP-drop →
  impact; unit cull → death puff; statue HP-drop → statue damage). **No ECS writes, no gameplay dependency.**
- **Pooled + throttled** for performance (cap concurrent effects; reuse objects).

## 5. Risks (carried to Phase D)
- **Emitters broken** → effects rebuilt in code (accepted).
- **Instant-hit sim** → "projectile trails" are cosmetic-only; not auto-wired (would need attacker→target
  positions; documented gap).
- **No clean spell-event hook** → spell cast/impact VFX API provided but only wired where observable; damage
  spells already surface via the hit/impact hooks (gap documented).
- **Performance** — pooling + a concurrent-effect cap + texture curation keep it mobile-safe.
- **APK/memory** — a few small textures + a fixed pool.
- **Licensing** — dev-only; replace before release.

## 6. Verdict
VFX coverage is **sufficient** for the required feedback slots (hit flash/impact, death puff, statue damage, with
a spell/projectile API) using small curated placeholders + code-driven effects. **Nothing imported yet** (gate
satisfied). Proceed to Phase B (`VfxManager`) → C (read-only integration) → D (validation), per
`PRIORITY4_VFX_FRAMEWORK_REPORT.md`.
