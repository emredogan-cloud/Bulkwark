# BULWARK — PHASE V0 (Visualization) Report

**Date:** 2026-06-04 · **Track:** temporary pre-Phase-5 GATE-1 validation (`PREPHASE5_VISUALIZATION_ROADMAP.md`).
**This is NOT Phase 5. No roadmap/canon/balance change.** **Device:** Xiaomi Redmi Note 11R, Android 13,
arm64-v8a. **Builds:** SimProxyRenderer `ab372d3` (v0.0.38) → URP color fix `5febaaa` (v0.0.39), both CI GREEN.
**Method used:** author → independent review → adversarial audit → repair → CI/CD GREEN → device validation.
**Artifacts (local, gitignored):** `runtime/device_validation/screen_color_{16,40}s.png`,
`screen_proxy_*.png`, `proxy_color_logcat.txt`.

---

# ✅ VERDICT: V0 RENDERING DELIVERED & VALIDATED  (exit condition PARTIALLY met — sim-gated)
The simulation is now **visible**: ECS entities render as **team-colored primitives**, correctly positioned and
framed by the camera, on device. The **"units moving and fighting"** part of the exit is **not yet
demonstrable** — not because of the renderer, but because the current sim produces a **single, idle AI unit**
(the AI-economy/targeting stall already documented in `BOOTSTRAP_EXECUTION_REPORT.md` §5). The renderer is
ready to show movement/combat the instant the sim produces them; that battle is what **V1** (player-side
training → an opposing force → combat) is designed to create.

---

## 1. What was delivered (`Assets/_Game/Bootstrap/SimProxyRenderer.cs` — removable, read-only)
A presentation MonoBehaviour (§12) that reads the ECS world read-only and mirrors entities as GameObject
primitives. Auto-spawned; deleting the one file removes it. **Every V0 deliverable:**

| Deliverable | Status | Evidence |
|---|---|---|
| Entities Graphics evaluation | ✅ done | Evaluated + **deferred** (needs per-entity render components + had CI resolution issues) — GameObject proxies chosen. See roadmap §"Entities Graphics evaluation". |
| Capsule rendering | ✅ | red AI-unit capsule visible (`screen_color_40s.png`) |
| Cube rendering | ✅ | two mine cubes visible |
| Cylinder rendering | ✅ | two statue cylinders visible |
| Temporary materials | ✅ | shared URP-compatible material; on device `Shader.Find` resolved **`Sprites/Default`** (URP Unlit/Lit not runtime-findable; fallback chain worked) |
| Team colors | ✅ | **AI = red**, **mines = yellow**, **statues = gray**; **player = blue** reserved (no player units spawn yet) |
| Health visualization | ✅ (coded) | proxy scale + color-lerp by HP fraction (statues full → full size; not yet exercised by damage) |
| Spawn visualization | ✅ | `[PROXY] SPAWN UnitAI/Mine/Statue` logs; proxy appears on spawn |
| Combat visualization | ✅ (coded) | white flash on HP drop + destroy on death + statue shrink/tint — **not triggered** (no combat occurred) |

## 2. The magenta→color fix (the one repair this phase needed)
First device build (`ab372d3`) rendered all primitives **MAGENTA**: `GameObject.CreatePrimitive`'s default
material is the **built-in Standard shader**, which renders magenta under URP and ignores the
`MaterialPropertyBlock` `_BaseColor` tint. Fix (`5febaaa`): assign a real URP-compatible shader
(`Universal Render Pipeline/Unlit → Lit → Sprites/Default → Unlit/Color` fallback) to a shared material and tint
per-proxy via the MPB (`_BaseColor` + `_Color`). Device re-validation confirmed correct colors (`Sprites/Default`
won the fallback and uses `_Color`). *(Cosmetic follow-up: to use URP Lit/Unlit instead of the sprite shader,
add it to "Always Included Shaders" or reference a URP material asset — current colors are correct as-is.)*

## 3. On-device evidence
- `[PROXY] SimProxyRenderer booted` · `[PROXY] camera configured to frame the battlefield (orthographic)` ·
  `[PROXY] proxy material shader = Sprites/Default` · `[PROXY] proxies=5 (rendering 5 entities as primitives)`.
- `screen_color_40s.png`: dark camera background (configured), **2 gray cylinders** (statues, ends),
  **2 yellow cubes** (mines, center), **1 red capsule** (AI unit) — all distinct, positioned, no magenta.
  The `SimDebugOverlay` panel + radar (correct colors) corroborate.
- 30 fps; no crash/exception. Sim state (cross-ref): `Gold P=100/AI=5, Units=1 (AI), Mines=2, Statue 1000/1000
  ×2, AI=Push, Engaged=0` — populated but static.

## 4. Exit-condition assessment — "can the player visually see units moving and fighting?"
- **See units / the world:** **YES** — the hard part (rendering) is done + proven on device.
- **Moving:** **NOT observed.** The single AI unit is at the **same position** at 12 s and 38 s
  (`screen_color_16s.png` vs `screen_color_40s.png`). The unit has no target (no enemy units exist; the player
  statue isn't auto-engaged), so it idles — a **sim/targeting limitation**, not a renderer one.
- **Fighting:** **NOT observed** (`Engaged=0`, statues undamaged) — only one unit exists and the player fields
  none, so there is no combat to render.

**Why:** the current battle is AI-only and stalled (AI trains a combat unit, no miners → 5 gold → 1 unit; the
player is inert; the lone unit doesn't path/target) — the exact follow-ups from `BOOTSTRAP_EXECUTION_REPORT.md`.
These are **gameplay/AI** matters, out of V0 (visualization) scope and not changed here.

## 5. Conclusion + handoff to V1 (NOT started)
**V0's deliverable — a working, team-colored, removable visualization of the live ECS sim — is COMPLETE and
device-validated.** To satisfy the full "moving and fighting" exit, the sim must produce a real battle; that is
**V1 — Playability** (player-side training buttons → the player fields blue units → opposing forces → movement
& combat the renderer will display), plus the AI movement/economy tuning noted above. The renderer requires no
further work to show those once they occur.

**STOPPING after this report per instruction. Do NOT continue to V1. No Phase 5/6. No roadmap/canon/balance
change. `SimProxyRenderer` + `SimDebugOverlay` are read-only, debug-tier, and fully removable.**
