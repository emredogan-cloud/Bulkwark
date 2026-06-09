# STICK EMPIRE RISE — CHARACTER BIBLE
**The permanent character standard.** Every unit is the **shared master stick rig** (`CharacterRig.cs`) + equipment overlays. Faction identity is **silhouette‑first** (weapon/head profile) — it survives grayscale; colour is an accent only. Animation **mirrors ECS** (driven by `SimProxyRenderer` from read‑only reads); it never leads the sim (§12).

The ECS already classifies each unit into one of six archetypes (`SimProxyRenderer.ClassifyArch` from `CombatProfile` + `MinerTag`); the rig equips off that classification — so adding the visual layer required **no sim change**.

---

## ACTIVE ARCHETYPES (launch)

### Swordsman — `Arch.Shield`
- **Role identity:** reliable frontline.
- **Silhouette:** upright, balanced; **iron helm** dome + **round shield** offhand + short **sword**.
- **Recognition cues (≤0.3 s):** shield disc on the lead arm; helm dome; compact upright posture.
- **Equipment:** WeaponSlot `ce_sword` · OffhandSlot `ce_shield` · head `ce_helm_iron` · CapeAnchor `ce_cape` (faction).
- **Animation:** idle/walk/**melee_attack** (overhead arc)/hit/death.
- **Faction:** blue steel shield boss + cape (Iron Pact) / ember‑crimson (Ashen Horde).

### Archer — `Arch.Ranged`
- **Role identity:** precision ranged.
- **Silhouette:** **hood** point on the head + a tall **curved bow** held in the offhand; lean stance.
- **Recognition cues:** the bow arc (largest single shape) + the pointed hood; no shield.
- **Equipment:** OffhandSlot `ce_bow` · head `ce_hood`.
- **Animation:** idle/walk/**ranged_attack** (draw + release)/hit/death.
- **Faction:** hood accent tint; bow stays wood (silhouette carries identity).

### Spearman — `Arch.Heavy` *(reach role in the launch set)*
- **Role identity:** reach and control.
- **Silhouette:** **crested helm** plume + a very **long spear** projecting well past the body — the tallest weapon shape on the field.
- **Recognition cues:** the long vertical spear line + the crest; reach silhouette.
- **Equipment:** WeaponSlot `ce_spear` · head `ce_helm_crested` · CapeAnchor `ce_cape` (faction).
- **Animation:** idle/walk/melee_attack (thrust via the same arc)/hit/death.
- **Faction:** crest + cape tint.

### Miner — `Arch.Miner`
- **Role identity:** economic backbone.
- **Silhouette:** **leather satchel** at the hip + a stout **heavy pickaxe**; working stance.
- **Recognition cues:** the pickaxe head curve + the satchel bulge; no helm.
- **Equipment:** WeaponSlot `ce_pickaxe` · AccessorySlot `ce_satchel`.
- **Animation:** idle/walk/**mine_swing** (repeating downward swing when stationary)/hit/death.
- **Faction:** neutral (economy unit); team read via the body accent.

### Mage — `Arch.Caster`
- **Role identity:** arcane support.
- **Silhouette:** tall **pointed wizard hat** + a **crystal staff** with a glowing orb; slender robe stance.
- **Recognition cues:** the hat cone (unmistakable head profile) + the orb glow on the staff.
- **Equipment:** WeaponSlot `ce_staff` · head `ce_hat_wizard`.
- **Animation:** idle/walk/**cast** (staff raise + FX burst)/hit/death.
- **Faction:** hat + staff orb accent (purple base, faction‑tinted glow).

*(A sixth, `Arch.Skirmisher` = sword + iron helm only, is the default fallback classification.)*

---

## RESERVED ARCHETYPES (prepared; not yet implemented)

All inherit the **master rig** — no new skeleton — and add equipment/scale/accent. Documented per the governance rule (§3): a new unique skeleton requires an explicit ADR.

| Archetype | Silhouette strategy | Equipment needs | Rig compatibility | Future animation |
|---|---|---|---|---|
| **Commander** | Larger scale + cape + crown/plume; commanding posture | crown/helm, sword, banner offhand | ✅ master rig + scale 1.15 | rally/`celebrate_victory`, aura FX |
| **Heavy Guard** | Bulkier limbs (limb width ↑), tower shield, great‑helm | great‑helm, tower shield, maul | ✅ master rig + wider limb scale | slow heavy melee |
| **Crossbowman** | Ranged stance + **crossbow** (horizontal vs bow's vertical) | crossbow, light helm | ✅ master rig | reload + `ranged_attack` |
| **Assassin** | Slim + hood + twin daggers; crouched | hood, 2× dagger | ✅ master rig | quick dash/strike |
| **Brute** (Ashen) | Oversized scale, no helm, club; hunched aggressive | club/maul, war‑paint accent | ✅ master rig + scale 1.3 | heavy slam |
| **Necromancer** | Robe + skull‑staff + tattered cape; hunched | skull staff, hood | ✅ master rig (Mage variant) | summon `cast` |
| **Giant** (boss) | Very large scale (≥2.5×), club; LOD‑sensitive | club, partial armour | ✅ master rig + scale; **flag for perf** | ground‑pound |
| **Elite variants** | Existing archetype + gold trim accent + plume | accent overlay only | ✅ pure overlay | reuse base clips |
| **Boss variants** | Unique head accessory + scale + FX aura | bespoke head/weapon overlay | ✅ master rig + FX | reuse + signature move |

**Expansion note:** any future role that genuinely cannot inherit the master rig (e.g., a quadruped or a non‑humanoid construct) must **halt expansion and file an ADR** documenting why a new skeleton is required — per §3 governance.

---

## EQUIPMENT MATRIX (modular overlays, runtime‑swappable)

| Slot | Options |
|---|---|
| **WeaponSlot** (RightHand) | sword · spear · pickaxe · staff *(crossbow/club/dagger reserved)* |
| **OffhandSlot** (LeftHand) | shield · bow *(tower shield/2nd dagger reserved)* |
| **Head** | iron helm · crested helm · hood · wizard hat *(great‑helm/crown reserved)* |
| **AccessorySlot** (hip) | satchel *(quiver/pouch reserved)* |
| **CapeAnchor** (shoulders) | cape (faction‑tinted) |

Swapping = set the slot sprite at spawn (or runtime) — **no rig duplication, shared material**.

## ANIMATION MATRIX (shared vocabulary — author once, reuse forever)

| Clip | Movement | Combat | Reactions | Meta | Utility |
|---|---|---|---|---|---|
| **implemented** | idle, walk *(run = faster walk)* | melee_attack, ranged_attack (draw), cast | hit_flinch, death | celebrate_victory | mine_swing |

All archetypes share these procedural clips on the same bones; equipment rides the hands/head bones, so a new archetype inherits every animation for free.

## FACTION VARIATIONS

| | Iron Pact | Ashen Horde |
|---|---|---|
| Accent | cool steel‑blue | ember‑crimson |
| Silhouette tone | disciplined/upright | aggressive (reserved brutes hunch/oversize) |
| Method | material colour on accents (cape/shield boss/crest/orb) — **no texture duplication** | same, red |
| Grayscale test | passes — identity is in the **weapon/head silhouette**, not colour | passes |
| Facing | rig mirrors by faction (face the enemy) | mirrored |
