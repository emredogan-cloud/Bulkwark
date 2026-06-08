# 06 — BATTLEFIELD VISUAL STRATEGY
**Problem:** the current in‑match battlefield (ECS primitives over a flat/placeholder background) is unacceptable. Define the definitive replacement.
**Constraints:** stick‑figure identity; §12 boundary (presentation only — no ECS/gameplay change); 3.66 GB‑RAM reference device (must hit 60 fps / no OOM); win condition = **destroy the enemy statue**.

---

## 1. Reference analysis

| Reference | What to take | What to avoid |
|---|---|---|
| **Stick War: Legacy** | The native idiom: **2D side‑view lane**, two statues, parallax depth, stick armies — exactly our identity & win condition | Don't copy its specific art/assets (IP, Report 01 §0) |
| **Kingdom Rush** | Layered painterly parallax, readable silhouettes, ambient micro‑animation, weather accents | Tower‑defense fixed camera (we pan a lane) |
| **Clash Royale** | Clean readability at small scale, punchy FX, performance discipline on mobile | 3D/iso arena (off‑identity for sticks) |
| **The mockups (`BattleHud`)** | The blue‑keep‑left vs red‑fortress‑right framing, faction colour split, central clash | The realistic iso troops + realistic units (Report 02) |

**Decision:** a **2D orthographic side‑view parallax lane** — Stick War‑native, on‑identity, cheapest to animate, and the most performant choice for a low‑RAM device. (The mockups' iso look is abandoned: it conflicts with the stick identity and is far costlier to animate.)

## 2. Layer model (UI‑free, per Report 05 invariants)

```
SKY        far gradient + drifting cloud band (slow parallax)        depth 1.0
HORIZON    distant castle/mountain silhouettes, faction-tinted        depth 0.8
MIDGROUND  rolling terrain, forest line, the two STATUES + keeps      depth 0.5
PLAYFIELD  the ground plane where stick units march & fight           depth 0.0  ← gameplay
FOREGROUND grass tufts, rocks, banners, occasional fly-through        depth -0.3 (in front, blurred)
FX/AMBIENCE embers, dust motes, smoke, weather                        additive overlays
```

The **PLAYFIELD** is the only gameplay layer (ECS units render here). Every other layer is **presentation** — pure art + particles, no ECS coupling (§12 safe).

## 3. Animation & ambience (all unscaled/presentation)

- **Parallax on camera pan:** as the camera tracks the front line, layers scroll at depth‑scaled rates → strong depth illusion from flat sprites.
- **Idle motion:** drifting clouds, swaying trees/banners (vertex/UV scroll), shimmering water, the **statues' idle aura glow** (faction‑tinted) so the objective always reads.
- **Combat ambience:** ground dust at the clash line, ember/spark bursts on impacts, smoke columns from fires, screen‑space heat shimmer near fires (cheap shader).
- **Camera treatment:** smooth ease toward the active front line; subtle impact shake on big hits / statue damage; a brief push‑in on statue destruction (victory beat). Camera is presentation‑only.

## 4. Weather & time‑of‑day

- **Weather** = additive particle overlay + a global tint, toggled per map: **rain** (streaks + darken + occasional lightning flash), **snow** (drift + cool tint + fog), **dust/ash** (Ashen biomes), **fog** (depth haze band). Implemented as pooled particle systems + one full‑screen tint quad — negligible cost.
- **Time‑of‑day:** ship **fixed per‑map time‑of‑day** (day / dusk / night) via a global colour grade + light‑direction tint per biome (Report 07). **A live day/night *cycle* is explicitly deferred** (P3) — it adds cost/complexity for little gameplay value on this device.

## 5. Performance budget (3.66 GB device, 60 fps target)

- **Backgrounds:** 5–7 large parallax sprites per biome, packed in **one SpriteAtlas per biome**; reuse across maps of the same biome. No per‑map unique 4K plates.
- **Lighting:** **no realtime lights** — bake mood into the art + a global tint. (Realtime lights are the classic mobile perf/OOM trap.)
- **Particles:** GPU particles, hard caps per system, **object‑pooled**; weather is one capped emitter + tint.
- **No heavy post‑processing** (no bloom stacks/DoF on this tier); fake glow via additive sprites.
- **Memory:** atlas streaming per biome; unload the previous biome on map change. Backgrounds as compressed textures (ASTC). This directly addresses the observed `lowmemorykiller` OOMs.
- **Draw calls:** atlas batching keeps the background at a handful of draw calls.

## 6. Definitive recommendation

Build the battlefield as a **2D side‑view parallax lane** with **5–7 UI‑free layered sprites per biome** (sky / horizon / midground+statues / playfield / foreground) + **pooled additive FX** for ambience, weather, and impacts, **fixed per‑map time‑of‑day** via global tint, **no realtime lights / no heavy post‑FX**, atlas‑batched and biome‑streamed for the 3.66 GB device. Camera pans/eases to the front line with light impact shake and a statue‑destruction push‑in. This is on‑identity (Stick War lineage), the cheapest path to a "alive" battlefield, and stays entirely within §12 (presentation only). Statues get a persistent faction aura so the win objective always reads.
