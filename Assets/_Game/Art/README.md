# Art hooks — cosmetic-safety contract (§6)

Phase 2 establishes the **art hooks**, not the art. Actual Spine skeletons, sprites,
and VFX are an asset-production task (DEFERRED — no art pipeline in this environment).
What is fixed here is the **cosmetic-safety rule that all art and future cosmetics MUST
obey** (roadmap §6, INVIOLABLE):

## Locked, read-critical (a cosmetic may NEVER change these)
- **Silhouette** — each archetype has one canonical, read-locked silhouette (this is what
  an opponent parses across the front).
- **Unit size / hitbox** — gameplay-identical regardless of skin.
- **Animation timing** — attack/cast windups and recovery frames are fixed (telegraphs
  must stay honest).
- **Ability VFX readability** — a spell/ability read can never be obscured by a cosmetic.
- **Faction-color identity** — Iron Pact vs Ashen Horde must remain instantly distinguishable.

## Cosmetic-mutable (outfit classes — §6)
Palette, material/texture, trim, particle/VFX **color**, idle/victory flourishes only.
Tiers Standard→Veteran→Elite→Legendary→Mythic are recolor/material/trim over the locked base.
Ranked may enforce a **clarity mode** (standardized read-safe skins) so no cosmetic ever
buys a competitive read. **No stat or readability advantage, ever** (P3 fairness).

## Folder layout (hooks, to be populated by the art pipeline)
```
Art/
  Units/<Faction>/<Archetype>/   # locked silhouette + skeleton (base), recolor variants
  Spells/<spell_id>/             # telegraph + effect VFX (readability-locked)
  Terrain/<kind>/                # high-ground/choke/cover/hazard readable tiles
  Commanders/<id>/               # commander portrait/banner (out-of-battle cosmetics)
```
No monetization, store, or cosmetic *content* ships in Phase 2 (that is Phase 4) — these
are gameplay-readability hooks only. **SCAFFOLD STATUS:** documentation hook; art DEFERRED.
