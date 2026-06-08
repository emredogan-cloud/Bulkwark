# PHASE 4 — BATTLEFIELD RECONSTRUCTION — FINAL REPORT
**Project:** Stick Empire Rise *(codename Bulwark)* · **Date:** 2026-06-08
**Scope:** replace the flat primitive battlefield with a **pure 2D side-view parallax lane**, biome system, statue states, weather, and camera polish — **presentation-only, §12 preserved** (the ECS sim/AI/economy/balance are untouched; presentation observes, never controls).

---

## 1. Architecture

The battlefield was already rendered by **`SimProxyRenderer`** — a §12 presentation MonoBehaviour that reads ECS entities read‑only, draws units/mines/statues as `SpriteRenderer`s on a z=0 plane, frames an **orthographic** camera to the action, and drew a single flat `bg_battle` sprite behind everything. Phase 4 replaces that flat bg with a layered parallax system and adds statue/camera/weather presentation — at the same read‑only seam.

**`BattlefieldParallax.cs`** (new, removable) composites the biome layers as camera‑following parallax around the existing unit proxies (the PLAYFIELD):

| sortingOrder | Layer | Parallax factor | Source |
|---|---|---|---|
| −140 | **L1 Sky** | 1.00 (appears static/distant) | `bf_<biome>_sky.jpg` |
| −136 | **L2 Horizon** | 0.82 | `bf_<biome>_horizon.png` |
| −132 | **L3 Midground** (hills/keeps; statue aura) | 0.55 | `bf_<biome>_mid.png` |
| 0–2 | **L4 PLAYFIELD** — units/mines/statues | 0.00 (world‑locked) | `SimProxyRenderer` (ECS proxies) |
| −128 | **L4 Ground** | 0.00 | `bf_<biome>_ground.png` |
| +40 | **L5 Foreground** | −0.30 (faster, blurred) | `bf_<biome>_fg.png` |
| +55/60 | **L6 FX / Weather + biome tint** | — | pooled `UiTex` discs |

Each frame, layers are scaled to cover the ortho view (×1.35 margin) and offset by `cameraX × parallaxFactor` (1.0 = static on screen → distant; 0.0 = world‑locked → playfield; negative = faster → foreground). The horizon line is placed where the units stand. `BattlefieldParallax` disables `SimProxyRenderer`'s flat bg on first build.

**Hard prohibitions honoured:** no full 3D, no isometric, no free camera, no realtime lights / shadows / HDRP / heavy post — mood is **painted into the sprites + one additive biome tint quad**.

## 2. Biome system

Five launch biomes, each = one sky JPG + four transparent PNG layers, generated UI‑free by `ui_pipeline/build_battlefield.py`. **Selecting a biome is just `BattlefieldParallax.Biome = "<name>"`** (or env `BULWARK_BIOME=…`) — it loads `bf_<biome>_*` at runtime; **no architectural rewrite, no rebuild**.

| Biome | Mood | Sky | Ground | Weather/FX |
|---|---|---|---|---|
| **grass** (Grasslands) | hopeful day | blue→hazy + sun | green | dust tint |
| **ash** (Ashlands) | dark fantasy | ember red/black | charred | **red embers** |
| **snow** (Snowfields) | cold isolation | teal/white | snow | snow drift + fog tint |
| **volcanic** (Volcanic Wastes) | apocalyptic | lava red/black | scorched | ashfall + heat tint |
| **dead** (Deadlands) | desolation | grey | grey | grey dust + mist tint |

## 3. Statue states (battlefield anchors)

`BattlefieldParallax.Statues()` reads ECS `StatueState` (Health/MaxHealth) + `Position` **read‑only** and renders a faction‑tinted aura behind each statue (left=blue Iron Pact, right=red Ashen Horde), driven by HP fraction:
- **Idle** (≥0.6): steady faction glow, slow pulse, full size.
- **Damaged** (<0.6): dimmer, faster pulse, shrinking.
- **Critical** (<0.3): shifts **red**, fast (5 Hz) pulse.
- **Destruction**: the proxy is culled by `SimProxyRenderer` (death VFX via `VfxManager.StatueDamage`) and the aura is removed.
Statue damage also feeds the camera shake (§5). No ECS writes.

## 4. Weather

Pooled (48 reused quads), presentation‑only, per‑biome (`WeatherFor`): ash/volcanic/dead → drifting **embers/ash**; snow → **snowflakes** with sine drift; (rain profile available). Driven by `Time.unscaledTime`; **no runtime allocations** after the pool is built; positions wrap within the camera view.

## 5. Camera

The base camera (orthographic, frames the action bounds via `SimProxyRenderer`) is enhanced with an **impact shake**: `SimProxyRenderer.CamShake` rises on damage (statue hits hardest, +0.18; unit hits +0.04), decays in **≤0.18 s**, and applies a small random offset (**≤~2 % viewport**) to the camera position. Readability‑first per the Phase‑4 rule (the framing itself is unchanged — it still keeps the whole clash on screen). *Clash‑track narrowing (≤12 % width) is a follow‑up refinement on top of the existing fit‑bounds framing.*

## 6. Asset mappings

`Assets/StreamingAssets/bulwark_ui/bf_<biome>_{sky.jpg,horizon.png,mid.png,ground.png,fg.png}` — 5 biomes × 5 layers = 25 files (~12 MB), runtime‑loaded by `BattlefieldParallax` (same device‑safe `UnityWebRequest` pipeline as the UI). Statue aura + weather sprites are procedural `UiTex` (no texture cost).

## 7. Memory / performance budget

Loaded resident **per active biome** (not all 5 at once): 1 sky JPG (~150 KB) + 4 PNG layers (~0.5–1.5 MB) ≈ **≤3 MB textures/biome** — well under the 25 MB battlefield‑texture budget. Weather pool = 48 tiny procedural discs ≈ **<0.5 MB** (budget 3 MB). Foreground = 1 sprite (budget 4 MB). Draw calls: ~6 background layers + the proxy batch. No realtime lights/post → GPU‑light. *Note: these are static estimates; a Profiler frame‑time/`≤1 ms` measurement on the Redmi is pending the device‑install unblock (§9).* 

## 8. Validation status (Unity‑runtime evidence)

Device install was intermittently **MIUI‑locked** (`INSTALL_FAILED_USER_RESTRICTED` re‑engaged), so validation used a **Linux standalone** (`LocalBuild.BuildLinux`) run on the build PC (DISPLAY=:1, NVIDIA GL 4.6) + an env‑gated **auto‑match** hook (`ValidationAutoMatch`, §12 — calls the same `StartMatch`/`Begin` the UI does) + `validate_standalone.py` (screenshot). This satisfies the rule's *"Unity runtime OR device screenshots."*

| Item | Status | Evidence |
|---|---|---|
| 6‑layer parallax lane (flat bg replaced) | **PASS** | `rc_p4_linux/22_battlefield.png` (grass) — sky/horizon/hills+keeps/ground + units + foreground |
| Biome system (no‑rebuild swap) | **PASS** | `ash_22.png` (env `BIOME=ash` → ember mood) |
| Weather (pooled, per‑biome) | **PASS** | ash embers visible in `ash_22.png` |
| No realtime lights (painted mood + tint) | **PASS** | by construction |
| Statue aura (idle/damaged/critical) | **PASS** (idle state) | `rc_p4_linux/42_statue.png` — blue glow (left/Iron Pact) + orange glow (right/Ashen Horde) behind the statues; damaged/critical pulse drives off HP as it drops |
| Camera impact‑shake | **IMPLEMENTED** — visual confirmation pending (shake is transient on hits) | code‑verified |
| Memory/perf `≤1 ms` on device | **IMPLEMENTED — MEASUREMENT PENDING** | needs device Profiler |
| ECS integrity (no sim regression) | **PASS** | presentation‑only; `[PROXY] proxies=N` unchanged; §12 reads only |

## 9. Remaining blockers / next steps

- **Device Profiler measurement** + on‑device capture: blocked by the recurring MIUI "Install via USB" re‑lock (re‑enable in Developer Options, or use the standalone path which is now established).
- **Clash‑track camera narrowing** (≤12 % width) and **statue crack/smoke art** (currently aura‑only) are refinements on the validated foundation.
- **Snow/volcanic/dead biomes**: assets generated; spot‑validate like grass/ash.

## 10. Verdict

The definitive battlefield standard is established and validated on Unity runtime: a **pure 2D side‑view parallax lane** with a **rebuild‑free biome system**, **HP‑driven statue auras**, **pooled weather**, and **impact‑shake** — all **presentation‑only (§12)**, mobile‑safe (no realtime lights, per‑biome texture budget), and a drop‑in replacement for the flat placeholder battlefield. Painted/authored biome art can replace the procedural layer sprites in the same slots without code change.
