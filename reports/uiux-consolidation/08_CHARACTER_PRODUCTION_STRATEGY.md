# 08 — CHARACTER PRODUCTION STRATEGY (the stick army)
**Problem:** the game has no real characters — only ECS primitives in play and realistic, off‑identity figures in the mockups (Report 02). Define how to produce the **stick‑figure** cast.
**Constraints:** stick identity; §12 (presentation only); 3.66 GB device; two factions (Iron Pact blue / Ashen Horde red).

---

## 1. The production‑method decision

| Method | Fit for stick figures | Memory | Flexibility (reskin/equip) | Cost | Verdict |
|---|---|---|---|---|---|
| **Sprite sheets** (pre‑rendered frames per unit per anim) | OK | **High** (frames × units × anims) | Low (re‑render per change) | High (art per unit) | ❌ Memory + cost explode with roster size |
| **2.5D / 3D** | Off‑identity, overkill | High | Med | Very high | ❌ Wrong look, wrong budget |
| **2D skeletal — Spine** | **Excellent** | Low | **Excellent** (mesh deform, skins) | Med + licence | ★ Premium choice |
| **2D skeletal — Unity 2D Animation + 2D IK** (in‑engine) | **Excellent** | Low | **Excellent** (bones + sprite swap) | Low (free, Unity‑native) | ★ **Default recommendation** |

**Decision: 2D skeletal/bone animation.** Stick figures are the *ideal* case for it — a stick body is essentially a bone rig with thin limbs. **One shared humanoid stick rig** is authored once; **every archetype reuses it** with swappable equipment sprites + a faction tint. This is the Stick War approach and it is dramatically cheaper than sprite sheets at roster scale.

**Tooling recommendation:** ship on **Unity's 2D Animation package (bone‑based) + 2D IK** by default (zero extra licence, Unity‑native, atlas‑friendly); treat **Spine** as an optional premium upgrade if mesh‑deform quality demands it later. FX stays sprite‑sheet/particle.

### Why this is the key unlock
Because all humanoid units share one rig, you **animate the locomotion/combat set ONCE** and the entire roster inherits it. New units = a new weapon + accent sprite + tint (hours), not a new animation set (weeks). This collapses the "no characters" problem into a tractable, scalable pipeline.

## 2. Shared rig + animation set (author once)

**Rig:** humanoid stick skeleton — head, torso, 2 arms (upper/fore/hand), 2 legs (thigh/shin/foot), optional cape/cloak bone. Thin clean limbs, round head, glowing‑eye sprite.

**Shared animation set (inherited by all):** `idle`, `march/walk`, `run`, `melee_attack` (A/B), `ranged_attack`, `cast`, `hit/flinch`, `death`, `cheer/victory`. Mirror these from ECS read‑only state (presentation only — **no ECS writes**, §12).

**Variation layers on top of the shared rig:**
- **Equipment overlay** — weapon + armor‑accent sprites parented to hand/body bones (sword, bow, staff, spear, pickaxe, crossbow, shield, helm, cape).
- **Faction tint** — material colour (blue Iron Pact / red Ashen Horde) on accents.
- **Body scale** — bulkier rig variant for Heavy/Brute; tattered for Undead.

## 3. Unit catalogue (maps to the roster in `UnitsArmyDesign`)

| Unit | Silhouette / equipment | Extra anims / VFX | Difficulty | Priority |
|---|---|---|---|---|
| **Swordsman / Shieldman** | stick + sword + kite shield + light helm | shield‑block; clang spark | Low (shared rig) | **P0** |
| **Archer (Iron Archer)** | stick + hood + longbow | draw/loose; **arrow projectile** (pooled) | Low | **P0** |
| **Spearman / Sentinel** | stick + spear + round/tower shield + crest | thrust; brace | Low | **P0** |
| **Miner** | stick + pickaxe + satchel | mine‑swing; **ore sparkle** | Low | **P0** (economy reads) |
| **Mage / Runic Adept** | stick + robe/hat silhouette + staff, glowing eyes | cast; **spell VFX** (particle) | Med (FX) | **P0** (caster) |
| **Crossbowman** | stick + crossbow | reload/loose; bolt projectile | Low | P1 |
| **Heavy Guard** | bulkier stick + two‑hander, heavy plates | heavy swing; ground‑thud | Low‑Med (scale variant) | P1 |
| **Warden** | stick + sword + cape (officer) | rally pose | Low | P1 |
| **King / Commander (hero)** | stick + crown + cape + sword + shield | rally; hero ability; **aura VFX** | Med (hero anims) | P1 |
| **Flamecaller** | stick + staff + ember accents | fire cast; **flame VFX** | Med (FX) | P2 |
| **Oathbreaker** | red‑accent stick warrior | melee | Low | P2 |
| **Heavy‑Brute (Ashen)** | large bulky stick, horned‑helm accent, maul (replaces orc) | heavy slam; roar | Med (scale) | P2 |
| **Stick‑Undead (Endless)** | tattered stick + bone/green necrotic accents | shamble; rise; crumble death | Med (reskin + anims) | P2 |

## 4. VFX & projectiles
- **Projectiles** (arrows, bolts, spells): pooled sprite/particle objects, presentation‑only.
- **Impacts/death:** sprite‑sheet poofs, dust, blood‑optional sparks (pooled).
- **Hero/ability auras:** additive sprite + particle (no realtime lights).
- **Faction read:** blue vs red accent + tinted FX so allegiance is instant at small scale.

## 5. Cosmetics / skins (already a designed feature — `SkinsScreenDesign`)
Skins are **equipment/accent swaps + tints on the shared rig** (e.g., "Leaf Set"). The skeletal approach makes cosmetics nearly free — a skin is a sprite set, not a new rig — which aligns with the existing Skins/Store monetization screens (display‑only; economy stays in ECS/services, §12).

## 6. Production priority (what makes the army exist)

1. **P0 — shared stick rig + the full shared animation set** (the foundation; one‑time).
2. **P0 — the five visible core units** (Swordsman, Archer, Spearman, Miner, Mage) — this is what the player sees fighting.
3. **P1 — remaining common roster** (Crossbowman, Heavy Guard, Warden) + **King/Commander hero**.
4. **P2 — Flamecaller, Oathbreaker, Heavy‑Brute, Stick‑Undead** (Ashen/Endless content) + first cosmetic skins.
5. **P3 — extended cosmetics, emotes, alt heroes.**

## 7. Definitive recommendation

Adopt **2D skeletal/bone animation on a single shared humanoid stick rig** (Unity 2D Animation + 2D IK by default; Spine as a premium option), with **equipment‑overlay sprites + faction tint + scale variants** to express the whole roster, and **pooled sprite/particle FX** for projectiles and abilities. Author the rig + shared animation set **once**, then ship the **five core on‑field units first**. This is the only approach that scales to a full RTS roster + cosmetics on a 3.66 GB device, is perfectly on‑identity for stick figures, and stays entirely presentation‑side (§12). It also makes the Skins/Store cosmetic features cheap to feed.
