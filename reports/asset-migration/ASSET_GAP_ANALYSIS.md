# BULWARK — Asset Gap Analysis (Phase B.5)

**Date:** 2026-06-05 · Classifies every gap (missing or must-author asset/capability) for the presentation pass.
Input: `BULWARK_ASSET_MAPPING.md`. **Severity:** CRITICAL = blocks gameplay visualization · HIGH = blocks
playable presentation · MEDIUM = quality improvement · LOW = future production content. Effort: S/M/L.

## Summary
The extracted sets supply **drop-in placeholders for every gameplay-visible object**, so there are **no CRITICAL
asset gaps** — the battle *can* be visualized. The real gaps are **capabilities/wiring** (a sprite-render path, a
uGUI menu/HUD framework, 9-slice/atlas authoring, audio re-encode) and **IP-critical BUILD items** (branding/
crests/final statue/cosmetics) that are out of scope for a throwaway placeholder pass.

## CRITICAL — blocks gameplay visualization
| Gap | Why critical | Recommended source | Strategy | Effort | Integration complexity |
|---|---|---|---|---|---|
| **Sprite render path** (entity → on-screen sprite) | Without it the battle stays primitives | the existing `SimProxyRenderer` seam | Swap `CreatePrimitive` → `SpriteRenderer` (URP 2D); load PNGs as `Sprite` (Resources or copied into `Assets/`) — **no new asset, a code change** | M | Low (one MonoBehaviour; no ECS/sim change) |
| **Drop-in unit/statue/mine sprites in-project** | Sprites must live in `Assets/` with sprite import meta | SW `Characters/*`, `Environment/Buildings`, `Campaign/Mine` | Copy a *small curated subset* (1 sprite per role/team + statue phases + mine) into `Assets/_Game/Art/_Placeholder/`, generate `.meta` with `textureType: 8` (Sprite) | S | Low |
> Both are *capability* gaps, not missing art — the art exists. Closing them = Phase E core.

## HIGH — blocks playable (game-like) presentation
| Gap | Why | Source | Strategy | Effort | Complexity |
|---|---|---|---|---|---|
| **uGUI menu/HUD framework** (13 screens: Splash→Menu→ModeSelect→…→Victory/Defeat) | only IMGUI debug today | SW menus/backgrounds + SM bars/frames + Roboto | Author uGUI Canvas scenes/prefabs (Image/Button/TMP); start with Splash + Main Menu + Mode Select + Battle HUD + Victory/Defeat (the required flow) | L | Med (Canvas YAML/prefabs, safe-area, scaling) |
| **Branded splash / logo** (no Stick War branding) | legal + identity | BUILD (placeholder text logo ok) | Temporary text/wordmark "BULWARK" on a generic bg; real logo BUILD | S | Low |
| **9-slice authoring for buttons/panels** | ripped atlases lost border data | SW/SM/AOW button+panel PNGs | Re-author sprite borders at import (set `borders` in sprite meta) for stretchy buttons/panels | M | Med (per-sprite borders) |
| **TMP font setup** | text uses default IMGUI font | **Roboto (Apache, SM)** | Import Roboto TTF + generate a TMP SDF font asset | S | Low |
| **Statue phase swap visual** | win/lose readability | SW StatueBase + StatueCracks | Drive sprite/overlay from `StatueState.Phase` in the renderer (read-only) | S | Low |
| **Audio playback layer** | no audio at all | SW/SM music + SFX | Add an AudioSource + a tiny play-on-event hook (menu music, victory/defeat stinger, click); re-encode WAV→OGG | M | Low |

## MEDIUM — quality improvements
| Gap | Source | Strategy | Effort | Complexity |
|---|---|---|---|---|
| **Per-archetype unit sprites** (Shieldman ≠ Crossbow visually) | SW per-unit sprites | Needs a spawn-side **visual-id** on the entity (today only Team + `MinerTag` are exposed) — a small data-only `IComponentData` stamped at spawn; until then use team-colored generic + miner sprite | M | Med (touches spawn path → schedule carefully, data-only) |
| **Battlefield parallax background** | SW/AOW/SM backgrounds | World-space quad(s) behind the z=0 plane; multi-layer parallax later | S | Low |
| **Combat/spell VFX** (emitters lost) | SW/SM VFX textures | Rebuild particle systems in-engine using the recovered textures; map damage-type colors | M | Med |
| **HUD bars** (statue HP, unit HP) | SM `ui/bars` | uGUI filled Image driven read-only from `StatueState`/`Health` | S | Low |
| **Audio compression** (WAV 47-86 MB banks) | all sets | Re-encode to OGG, prune Misc; curate ~10-20 SFX + 3-4 tracks | M | Low |
| **Terrain / building decor** | SW terrain/buildings | Optional set-dressing sprites on the battlefield | S | Low |

## LOW — future production content (defer)
| Gap | Source/strategy | Note |
|---|---|---|
| **Skeletal animation (Spine)** | rigs exist (SW Universal 4.1; SM 3.8; AOW 2.x) | Needs paid `spine-unity` + Spine Editor; **version-split** across sets; deferred (sprites first) |
| **Commander out-of-battle screens** | AOW Generals portraits (re-theme) | Meta UI, post-presentation |
| **Cosmetic-recolor tiers** (Standard→Mythic) | GENERATE masks over locked silhouette | Gated on production art + cosmetic-safety (§6) |
| **Campaign / level-select meta** | SW Campaign art | Not in the required flow |
| **Chest-open / reward animations** | SW/SM chest Spine + chest PNGs | Monetization-screen polish |

## IP / replace-before-ship gaps (apply to ALL of the above)
Every imported placeholder is © its source studio → **BUILD originals** for: faction crests/branding, final
statue identity, splash/logo, branded UI kit, music, and cosmetics — before any release or public playtest.
Per the roadmap, this is the BUY+KITBASH (licensed) / BUILD (IP-critical) / GENERATE (cosmetics) framework; the
2D-Spine-vs-2.5D-3D ADR remains a **human decision gate** for production unit art.

## Gap verdict
**No CRITICAL art is missing** — the pass is unblocked. Closing the two CRITICAL *capability* gaps (sprite render
path + curated drop-in sprites) plus the HIGH uGUI flow gives a recognizable, playable-looking game. Everything
else is quality/future. Proceed to `PRESENTATION_ARCHITECTURE.md` + `UI_RECREATION_PLAN.md`.
