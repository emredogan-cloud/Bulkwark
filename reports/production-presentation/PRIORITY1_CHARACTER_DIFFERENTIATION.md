# BULWARK — Production Presentation · Priority #1: Character Visual Differentiation

**Date:** 2026-06-05 · **Phase:** Production Presentation (ADR-5-003 **Option A** — explicitly NOT roadmap Phase 5).
**Build:** `74bf9a7` (CI run 27011917770, **GREEN**). **Method:** author → 2-lens independent review + adversarial
audit → repair → CI/CD GREEN → Android validation. **Inviolable:** presentation-layer only, **READ-ONLY of ECS**;
no gameplay/balance/AI/economy/spawn/canon change; deferred GATE-1 bugs untouched; GATE 1/GATE 5 remain open/binding.

---

## Objective
Priority #1 of the re-scoped phase: make units **visually distinct by role** instead of one sprite per faction —
the first step in "professional-looking RTS."

## What changed (presentation-layer, removable)
- **`SimProxyRenderer.cs`** — each unit's **visual archetype** is derived **read-only** from its existing
  `CombatProfile` (DamageType/ArmorClass) + `MinerTag` (via `_em.HasComponent`/`GetComponentData` — no writes), and
  mapped to a distinct placeholder sprite. The existing per-team tint (cobalt/oxblood) is unchanged, so **team ×
  archetype** now both read. Result: **1 sprite/faction → 5 distinct archetypes/faction** (+ miner).
- **`PlaceholderAssets.cs`** — registered 3 new sprites; **StreamingAssets/bulwark/** gained `u_heavy` (Giant),
  `u_ranged` (Archidon), `u_caster` (Magikill) — © ripped, **dev-only placeholders, replace before release**.

## Classification (verified against the real unit `.asset` data by the review)
| Archetype | Sprite | Iron Pact | Ashen Horde | Key (DamageType / ArmorClass) |
|---|---|---|---|---|
| Shield (frontline) | Spearton (`unit_ai`) | Shieldman | — | ArmorClass = Shielded |
| Heavy | Giant (`u_heavy`) | Legionary, Ironclad | Razorbeast | ArmorClass = Heavy (melee) |
| Ranged | Archidon (`u_ranged`) | Crossbow | Slinger | DamageType = Pierce/Blunt |
| Caster | Magikill (`u_caster`) | Battlemage | Hexcaster | DamageType = Fire/Poison |
| Skirmisher | Swordwrath (`unit_player`) | — | Raider, Houndmaster | else (melee, light) |
| Miner | Miner (`miner`) | Miner | Miner | MinerTag |
(Collisions Legionary/Ironclad→Heavy and Raider/Houndmaster→Skirmisher are same-archetype and acceptable; team
tint still separates the two factions of each archetype.)

## Validation
- **CI/CD:** GREEN — IL2CPP Android build + EditMode/PlayMode compile tests pass on `74bf9a7`.
- **Independent review + adversarial audit (2 lenses, PASS, 0 blockers):**
  - *Compile* — enum/namespace resolution (Bulwark.Data DamageType/ArmorClass, no CS0104), EntityManager APIs,
    UpdateProxy arity (all 3 call sites 6-arg), exhaustive `Arch` switch, brace balance — all confirmed.
  - *Presentation-safety + correctness* — confirmed **zero ECS writes** (read-only), fairness-neutral; verified
    **all 12 units** classify to a distinct-by-archetype sprite **against the actual `.asset` data**; crash-safe
    (`GetComponentData` guarded by `HasComponent`; array alignment unaffected; null→white fallback). One LOW
    comment fix applied.
- **On-device (Android):** ⚠️ **PENDING — test device physically disconnected mid-capture** (Redmi Note 11R;
  `lsusb` shows no device — the same intermittent unplug seen earlier, not a software fault). **No screenshots are
  fabricated.** Prior runs already proved `[ASSETS] 13/13 sprites load` + the sprite battlefield renders on this
  device; the new sprites load by the same path. **To finish:** reconnect the device and I will install `74bf9a7`
  and capture the differentiated battlefield (and, ideally, queue Heavy/Ranged/Caster via the in-match controls to
  show all archetypes side by side).

## Performance / size
- **No per-frame ECS cost** beyond two `HasComponent` + one `GetComponentData` read per unit per frame (unit counts
  are small; reads only). **APK:** +~30 KB (3 small 128² PNGs). Negligible.

## Remaining gaps (for later priorities)
- True per-*role* (not per-archetype) art would need a spawn-side visual-id (a fairness-neutral data tag) — deferred
  to avoid any sim-archetype change this pass; the 5-archetype split is a large readability win without it.
- Still **static sprites** (animation = a later priority); placeholder art is ripped (replace before release).
- Next priorities (owner order): **#2 proper battle HUD**, #3 audio, #4 VFX, #5 animation, #6 polish.

## Verdict
Character visual differentiation is **implemented, CI-GREEN, and review-validated** (correctness checked against
real asset data) — **read-only, zero gameplay change**, the battlefield now shows distinct archetype silhouettes
per faction. The only open item is **on-device screenshot capture**, blocked solely by the test device being
physically disconnected; completable on reconnect. **GATE 1 (FUN) = FAIL and GATE 5 remain open/binding; this is
NOT roadmap Phase 5.** Stopping at this checkpoint for owner direction on Priority #2.
