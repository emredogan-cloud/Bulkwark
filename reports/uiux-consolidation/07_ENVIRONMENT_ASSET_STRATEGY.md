# 07 — ENVIRONMENT ASSET STRATEGY
**Goal:** define how the battlefield environment is built — statues, trees, rocks, mines, terrain, props, fore/background — as a coherent, performant, on‑identity 2D asset set.
**Pairs with:** Report 06 (battlefield layers/perf) and Report 05 (clean‑layer discipline).

---

## 1. Visual style

**Flat, bold 2D vector‑style** consistent with the stick‑figure characters: clean silhouettes, limited palette per biome, painterly texturing kept subtle so **characters read against it**. High silhouette contrast between the playfield and the background (units must never get lost in busy art). Faction coding by tint, not by clutter.

## 2. Biome system (reuse > bespoke)

Author **biome kits**, not per‑map art. Each biome = one SpriteAtlas of layered plates + a prop set + a colour grade. Maps are **compositions of a biome kit**, so 12 campaign levels don't need 12 unique art sets.

| Biome | Mood / grade | Signature props |
|---|---|---|
| **Greenfield / Forest** | bright day, green/gold | oaks, bushes, wooden palisade, mill |
| **Siege / Castle** | dusk, slate+ember | castle walls, broken siege engines, banners, rubble |
| **Ashen / Volcanic** | night, red/black, ash | lava cracks, charred trees, bone piles, ember vents |
| **Frost / Haunted** | cold/fog, teal | dead trees, snowdrifts, fog banks, gravestones |

## 3. Asset catalogue & treatment

| Asset class | Layer | Animated? | Treatment | Notes |
|---|---|---|---|---|
| **Statues** (player + enemy) | midground | **Yes** — idle aura glow, damage states, destruction | **Highest priority** — they are the win condition; must read instantly, faction‑tinted aura, 3–4 damage stages + a destruction burst | One rig, two tints |
| **Terrain / ground plane** | playfield | subtle (dust, footstep decals) | Tileable ground strip per biome; the surface units walk on; must be flat enough to read units | Decal pooling |
| **Keeps / spawn structures** | midground | minor (banner flutter, smoke) | Faction barracks/keep flanking each statue | 2 tints |
| **Trees / foliage** | mid + foreground | sway (UV/vertex) | A few variants per biome; foreground ones blurred & in front | Atlas |
| **Rocks / boulders** | mid + foreground | none | Silhouette breakers; cheap | Atlas |
| **Mines / resource nodes** | midground | shimmer/sparkle | Gold/crystal node where stick **Miners** work; gentle glint so it reads as interactive | Tie to economy *display* only (§12) |
| **Decorative props** | mid + foreground | minor | banners, tents, barrels, bones, broken weapons | Scatter set |
| **Foreground elements** | foreground | parallax + slight blur | grass tufts, fences, occasional fly‑through (crows/embers) | Depth/parallax |
| **Background silhouettes** | horizon | slow parallax | castles, mountains, distant armies (as **stick** silhouettes) | 1 plate/biome |

## 4. Pipeline

1. **Author per biome:** one layered source (background plates + prop sheet) → export UI‑free PNG layers → pack into a **per‑biome SpriteAtlas** (ASTC compressed).
2. **Statues & mines** get a small **state machine** (idle / damage stages / destroyed; idle / active) driven by presentation events mirrored from ECS read‑only state — **no ECS writes** (§12).
3. **Scatter system:** props placed via lightweight presentation data (positions/variants per map), not hard‑coded, so maps are cheap to compose.
4. **Performance:** one atlas resident per biome; unload on biome change (addresses OOM); pooled FX; no realtime lights (bake mood into art + global tint per Report 06).

## 5. Animation requirements (minimum viable "alive")

- Statues: idle aura loop + damage transitions + destruction (must‑have).
- Trees/banners/foliage: ambient sway loop.
- Mines: shimmer loop + active sparkle when worked.
- Fires/smoke: pooled particle loops.
- Foreground: parallax + occasional fly‑through.

## 6. Production priority

1. **P0:** Statues (player + enemy) with damage/destruction states — *the objective must read.*
2. **P0:** One complete **Siege/Castle biome kit** (the default battle look) — ground, keeps, 1 horizon plate, core props.
3. **P1:** Greenfield biome (campaign early game) + mines (economy readability).
4. **P1:** Ambient animation pass (sway, shimmer, fires).
5. **P2:** Ashen + Frost biomes; weather overlays (Report 06).
6. **P3:** Extra prop variety, foreground fly‑throughs, decorative density.

## 7. Definitive recommendation

Build **reusable per‑biome 2D layered kits** (atlas‑packed, ASTC, UI‑free) rather than per‑map bespoke art; treat **statues as the top‑priority animated hero‑prop** (win condition); animate the world cheaply (sway/shimmer/pooled FX, no realtime lights); and start with **one Siege/Castle biome + statues** to make the default battle look correct, then expand biome‑by‑biome. This is on‑identity, memory‑safe for the 3.66 GB device, and §12‑clean (all presentation).
