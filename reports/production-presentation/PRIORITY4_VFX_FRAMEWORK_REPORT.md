# BULWARK — Production Presentation · Priority #4: VFX Framework Report

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 **Option A** — NOT roadmap Phase 5).
**Build:** `1fa732c` (CI run 27027640017, **GREEN**). **Method:** discovery → author → independent review →
adversarial audit → repair → CI/CD GREEN → (Android device capture deferred — owner cannot connect the phone).
**Inviolable:** presentation-only, **NO gameplay dependency, ZERO ECS writes**; no balance/AI/economy/commander-
budget/monetization/canon change; **GATE 1 FAIL / GATE 2 / GATE 3 / GATE 5 all preserved**; deferred GATE-1 bugs
untouched. **Companion:** `PRIORITY4_VFX_DISCOVERY.md` (Phase A).

---

## Objective
Transform BULWARK from an **Audio + HUD** build into an **Audio + HUD + Visual-Feedback** RTS — the first VFX layer
— without touching gameplay.

## Phase A — Discovery (done)
Audited the extracted VFX (Stick War 355 textures: blood/ash/sparkle/glow/lightning/heal/magic; Stickman fx kit:
smoke/glow/spark/explosion/fire; Age of War: muzzle/aura/shine). **Reality: the particle emitters are stripped**
(textures only). Selected 4 small textures + a code-driven build plan. See `PRIORITY4_VFX_DISCOVERY.md`. All
textures are **© ripped → dev-only placeholders, replace before release.**

## Phase B — Architecture (`VfxManager.cs`, new, removable)
- **`VfxManager`** — auto-spawned singleton; a **fixed pool of 48 `SpriteRenderer`s** (built once in `Awake`, never
  instantiated per-effect), each a short-lived effect tweened in `Update` (scale `Lerp` + alpha fade over its
  lifetime, then disabled). `Spawn(key,pos,life,s0,s1,color)` grabs a free slot or recycles round-robin; effects
  draw above the battlefield (`sortingOrder 10`, `z = -0.5`).
- **Own texture loader** — 4 PNGs in `StreamingAssets/bulwark_vfx/`, runtime-loaded via `UnityWebRequestTexture`
  (emitters were unrecoverable → effects rebuilt in code; no Unity ParticleSystem authoring).
- **API:** `Impact` (throttle 0.04 s), `DeathPuff`, `StatueDamage` (throttle 0.10 s), plus `SpellCast` /
  `SpellImpact` / `Trail` provided for later wiring.

## Phase C — Integration (read-only; observes events, never drives gameplay)
Driven entirely from `SimProxyRenderer`'s **existing read-only observations** (the same events the audio layer
uses), Match-gated:
| Observed event (read-only) | Effect |
|---|---|
| Unit HP-drop (the existing damage-flash) | **hit flash** (sprite tint, already present) + **hit impact** spark (`Impact`) |
| Statue HP-drop | **statue damage** burst (`StatueDamage`) |
| Unit-proxy cull (death) | **death puff** smoke (`DeathPuff`, position captured before destroy) |
**No ECS writes, no sim hook** — effects only *react* to what the renderer already reads.

## Phase D — Validation
- **Independent review + adversarial audit (2 lenses, PASS, 0 blockers):**
  - *Compile* — `UnityWebRequestTexture`/`DownloadHandlerTexture` resolve via the present
    `unitywebrequesttexture` module (manifest + lock); `SpriteRenderer`/`Sprite.Create`/tween APIs valid; renderer
    call sites + pool internals + brace/paren balance all confirmed.
  - *Performance / Phase-D* — **presentation-only (zero ECS writes)**; **pool fixed at 48, non-leaking, no
    per-effect instantiate**; `Update` is O(48); concurrent effects bounded (recycle oldest); Impact/StatueDamage
    throttled; crash-safe (null-sprite no-op, `Instance?.` guards, death position captured before `Destroy`).
  - **Memory/APK/Android:** **acceptable** — 4×256² PNGs (~91 KB) runtime-loaded (jar:// path handled); 48 light
    SpriteRenderers + 4 small textures = negligible VRAM.
- **CI/CD:** **GREEN** — IL2CPP Android build + EditMode/PlayMode compile tests pass on `1fa732c`.
- **APK size:** 40.52 → **40.62 MB (+0.10 MB)** — the curated VFX textures.
- **On-device (Android):** **deferred at owner's request** (phone not connectable). No VFX "evidence" fabricated.
  When available: install `1fa732c` and confirm impact sparks on hits, smoke puffs on deaths, and a burst on
  statue damage during a match.

## Remaining gaps (later)
- **Projectile trails** — cosmetic only (sim combat is instant-hit); the `Trail` API exists but is not auto-wired
  (would need attacker→target positions).
- **Spell cast/impact telegraphs** — no clean read-only spell-event hook today; the API is provided, and damage
  spells already surface via the hit/impact hooks. A spell-event observation is a later step.
- **Explosion sheet / fire / lightning** frames not used this pass (single-texture effects only).
- Placeholder textures are © ripped — **replace with original/licensed VFX before any release.**

## Verdict
The first **VFX framework** — pooled hit impacts, death puffs, and statue-damage bursts (plus a spell/projectile
API) — is **implemented, review-validated, and CI-GREEN**, fully **presentation-only with no gameplay/ECS impact**
(GATE 1 FAIL and the deferred gates all preserved; not roadmap Phase 5). The battlefield now has visual feedback to
match the audio + HUD. The only deferred item is on-device confirmation (phone unavailable).

**STOPPING after Priority #4 per instruction. Priority #5 (animation) NOT started.**
