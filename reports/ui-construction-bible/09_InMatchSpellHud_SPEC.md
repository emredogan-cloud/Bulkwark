# BULWARK — UI CONSTRUCTION SPEC · 09 · In-Match Spell HUD

Source: design/InMatchSpellHudDesign.png · 1672×941 (1.78:1, normalize to 2340×1080) · Analysis-only forensic spec.

> Inherits GLOBAL VISUAL DNA + the §12 boundary from `00_CONTEXT_RECOVERY.md`. This is the **Battle HUD in its
> spell-casting state**: the same edge chrome as `08_BattleHud`, PLUS a bottom-right **spell-slot row** with
> per-slot **cooldown timers**, a large round **commander/hero portrait** (ultimate), and — when a targeted
> spell is being aimed — a big circular **targeting telegraph** ring rendered over the battlefield. Casting is a
> presentation-level interaction; the actual spell effect is the meta/sim's concern. UI stays read-only except
> the §12-permitted writes.

---

## A · SCREEN PURPOSE
Extends the in-match HUD with **spell command**. It shows: (1) the same top/edge readouts as the Battle HUD —
gold "**150**", supply "**2/8**", left Iron Pact HP "**10,000 / 10,000**", right Ashen Horde HP
"**8,750 / 10,000**", army "**15/50**", and the 5 unit-train tiles (costs **40 / 60 / 50 / 80 / 120**) with
GARRISON / DEFEND / ATTACK; (2) a **spell-slot row** of three castable spells above the order cluster —
**Lightning (12s)**, **Heal/Restore (5s)**, **Arrow-volley (8s)** — each a framed icon with a remaining-cooldown
label; (3) a large round **commander portrait** with an ornate gold ring at the far bottom-right (the hero/
ultimate button); and (4) a **targeting telegraph** — a glowing blue concentric ring with a cardinal compass-
cross — drawn on the battlefield center while a ground-targeted spell is being aimed (the active state captured
in this mockup). Purpose: let the player fire spells/ultimate and place ground-targeted effects without leaving
the battle.

## B · VISUAL DNA (screen-specific delta from 08)
- **Same edge frame** (gold-rim chrome, top HP bars + crests, resource chips, unit tray, GARRISON/DEFEND/ATTACK)
  — re-use 08's DNA verbatim for those.
- **NEW — Spell-slot row:** three smaller gold-framed **square spell tiles** sitting just **left of the
  commander portrait** and **above the order cluster**, each showing a glowing elemental icon (electric-blue
  bolt; green life-rune; orange arrow-fan) and a small **cooldown time** beneath ("12s", "5s", "8s"). Tiles read
  as **arcane glass** (inner colored glow) rather than the steel of the unit tiles.
- **NEW — Commander/Ultimate orb:** a large **circular portrait** of the player's commander, framed by a thick
  ornate **gold ring** with a subtle glow, anchored at the bottom-right corner above ATTACK — clearly the
  highest-value cast (the "ultimate").
- **NEW — Targeting telegraph:** a large **blue concentric-circle** reticle with an inner compass cross and a
  soft radial glow, centered on the battlefield, indicating "choose a target location" for a ground spell. It is
  a **transient aiming overlay**, not permanent chrome.
- This screen's unit-tile **costs differ** from 08 (40/60/50/80/120) and the right HP is mid-fight (8,750), i.e.
  it is the same HUD at a different battle moment with the spell layer engaged.

## C · SCREEN DECOMPOSITION (ASCII node tree — delta nodes in **bold**)
```
InMatchSpellHudScreen (UiScreen, CanvasGroup, overlays live ECS battle)
└─ Root (stretch-all)
   ├─ Scrim_Top / Scrim_Bottom (Image, gradients, raycast off)        // as 08
   ├─ **TargetTelegraph (Container, battlefield-center, transient)**  // shown only while aiming
   │  ├─ **Telegraph_Glow (Image, additive)**                         // soft radial blue glow
   │  ├─ **Telegraph_RingOuter (Image)**                              // large circle
   │  ├─ **Telegraph_RingMid (Image)**                                // mid circle
   │  ├─ **Telegraph_RingInner (Image)**                              // inner circle
   │  └─ **Telegraph_Compass (Image)**                                // N/E/S/W cross + center pip
   ├─ SafeArea (RectTransform + SafeAreaFitter)
   │  ├─ TopBar (Container)                                           // pause + HP bars + chips  (see 08)
   │  │  ├─ PauseButton ( ❚❚ )
   │  │  ├─ ResChip_Gold ("150") · ResChip_Supply ("2/8")            // top-left cluster
   │  │  ├─ HpBar_Left_IronPact  (crest + trough + blue fill + "10,000 / 10,000")
   │  │  ├─ HpBar_Right_AshenHorde (crest + trough + red fill + "8,750 / 10,000")
   │  │  └─ ResChip_Army ("15/50")                                    // top-right
   │  ├─ UnitTray (Horizontal, bottom-left)                          // 5 tiles, costs 40/60/50/80/120
   │  │  └─ UnitBtn_0..4 (Portrait + CostChip + CooldownVeil + AffordTint)
   │  ├─ **SpellRow (Horizontal, bottom, left of commander)**
   │  │  ├─ **SpellBtn_Lightning (Button)** ├ ArcaneFrame + Icon_Bolt   + CdLabel "12s" + CdVeil
   │  │  ├─ **SpellBtn_Heal     (Button)** ├ ArcaneFrame + Icon_Life   + CdLabel "5s"  + CdVeil
   │  │  └─ **SpellBtn_Arrows   (Button)** └ ArcaneFrame + Icon_Arrows + CdLabel "8s"  + CdVeil
   │  ├─ **CommanderOrb (Button, bottom-right corner)**
   │  │  ├─ **Orb_Ring_Gold (Image)** · **Orb_Glow (Image)**
   │  │  ├─ **Orb_Portrait (Image, circular mask)**
   │  │  └─ **Orb_ReadyFlash / Orb_CdVeil (Image, Radial360)**
   │  └─ OrderCluster (Horizontal, bottom-right, above/left as laid out)
   │     ├─ Btn_Garrison (shield + "GARRISON")
   │     ├─ Btn_Defend   (swords + "DEFEND")
   │     └─ Btn_Attack   (chevrons + "ATTACK")                        // primary
   └─ (live ECS battlefield beneath the canvas)
```

## D · UNITY HIERARCHY SPEC (delta + shared)
*Shared nodes (Scrim, TopBar, PauseButton, HP bars, ResChips, UnitTray, OrderCluster) follow **08 §D exactly**.*
New/changed nodes:
| Node | Parent | Order | Type | Anchor preset | Pivot | Alignment / notes | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| TargetTelegraph | Root | 2 (above scrims, below SafeArea chrome) | Container | center (0.5,0.5) | 0.5,0.5 | follows the cursor/finger ground point while aiming | **no** (battlefield space) | scales w/ camera, not safe area |
| Telegraph_Glow | TargetTelegraph | 0 | Image | center | 0.5,0.5 | additive radial | no | — |
| Telegraph_RingOuter/Mid/Inner | TargetTelegraph | 1/2/3 | Image | center | 0.5,0.5 | concentric, slow counter-rotate | no | — |
| Telegraph_Compass | TargetTelegraph | 4 | Image | center | 0.5,0.5 | N/E/S/W cross + pip | no | — |
| SpellRow | SafeArea | 2 | HorizontalLayoutGroup | bottom, anchored right-of-tray / left-of-orb | 0,0 | 3 arcane tiles | yes | pin to lower band |
| SpellBtn_Lightning/Heal/Arrows | SpellRow | 0/1/2 | Button | layout element | 0.5,0.5 | square arcane tiles | yes | fixed cell |
| ArcaneFrame | SpellBtn | 0 | Image (sliced) | stretch-all | 0.5,0.5 | gold rim + arcane glass fill | yes | — |
| Icon_(Bolt/Life/Arrows) | SpellBtn | 1 | Image | center (inset) | 0.5,0.5 | glowing elemental glyph | yes | — |
| CdLabel | SpellBtn | 2 | Text | bottom-center | 0.5,0 | "12s"/"5s"/"8s" | yes | — |
| CdVeil | SpellBtn | 3 | Image (Filled, Radial360, origin Top) | stretch-all | 0.5,0.5 | dark sweep while on cooldown | yes | — |
| CommanderOrb | SafeArea | 3 | Button | bottom-right (1,0) | 1,0 | large round; rightmost bottom element | yes | pin BR (above ATTACK row) |
| Orb_Glow | CommanderOrb | 0 | Image | center | 0.5,0.5 | additive gold glow when ready | yes | — |
| Orb_Ring_Gold | CommanderOrb | 1 | Image | stretch-all | 0.5,0.5 | thick ornate ring | yes | — |
| Orb_Portrait | CommanderOrb | 2 | Image (circular mask) | stretch (inset) | 0.5,0.5 | commander bust | yes | — |
| Orb_CdVeil / Orb_ReadyFlash | CommanderOrb | 3 | Image (Radial360) | stretch-all | 0.5,0.5 | ult cooldown / ready pulse | yes | — |
| OrderCluster | SafeArea | 4 | HorizontalLayoutGroup | bottom-right (1,0) | 1,0 | shifts left of the orb | yes | pin BR |

**Order note:** SpellRow and CommanderOrb sit in the bottom band between the UnitTray (BL) and ATTACK (BR). The
CommanderOrb is the rightmost, largest bottom element; the SpellRow is immediately to its left; the
GARRISON/DEFEND/ATTACK cluster sits below/inline at the far right corner (matching the mockup, where ATTACK's
chevrons read at the very corner and the orb sits just above the order row).

## E · LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Shared chrome (top HP bars, pause, chips, unit tray, order cluster):** identical to **08 §E** (same band
  geometry). **Difference:** unit-tile costs are **40/60/50/80/120**; right HP text is **8,750 / 10,000**. In
  this mockup the gold "150"+supply "2/8" chips and the left HP bar are clustered top-LEFT (HP bar reads just
  below the chips), army "15/50" top-RIGHT — reproduce 08's chip/bar fractions; if the left HP bar visually sits
  beneath the chip row, anchor it at y≈0.90 left-of-center rather than spanning (this mockup shows the left HP
  bar lower-left under the chips). Keep both bars ≈0.30W where shown.
- **Clear battlefield zone:** same inviolable center x∈[0.20,0.80], y∈[0.20,0.82]; the telegraph is allowed to
  render here (it's an aiming overlay, not blocking chrome — it doesn't consume the eventual confirm tap; the
  confirm happens on release at the ground point).
- **TargetTelegraph:** outer ring Ø ≈ **0.42H** (≈454 px) when shown at default range; centered on the aim point
  (the mockup shows it near screen-center, ≈(0.42, 0.52)). Mid ring ≈0.62× outer, inner ≈0.34× outer; compass
  cross spans the mid ring; soft glow halo ≈1.4× outer. Stroke widths: outer ≈4px, mid ≈3px, inner ≈2px @1080.
- **SpellRow:** 3 tiles each ≈ **0.072W × 0.108H** (≈168×117 px), spacing ≈0.010W. The row sits in the bottom-
  right region, its right edge ≈0.06W to the **left** of the CommanderOrb, bottom aligned ≈0.165H above safe
  bottom (so it sits **above** the GARRISON/DEFEND/ATTACK row). CdLabel cap-height ≈0.020H on the tile's lower
  edge.
- **CommanderOrb:** Ø ≈ **0.165H** (≈178 px) including the ring; centered at ≈(0.945, 0.30 from bottom), i.e.
  right edge ≈0.985W, vertical center ≈0.30H up from safe bottom — clearly the largest bottom-right element,
  sitting just above the ATTACK button. Gold ring thickness ≈0.018H; portrait inner Ø ≈0.13H.
- **OrderCluster:** as 08 (3 buttons ≈0.105W×0.105H), pinned bottom-right corner; the cluster sits **below** the
  orb/spell row so all three groups stack without overlap in the lower-right quadrant.
- **Tablet / Ultrawide / Notch:** identical strategy to 08 — corner-pinned groups, center widens on ultrawide,
  SafeArea protects pause/army/orb/spell row from notches; the telegraph scales with the camera/world, not the
  safe area, and is clamped to stay on-screen.

## F · TYPOGRAPHY (per text)
*HP / gold / supply / army / cost / GARRISON-DEFEND-ATTACK typography = **08 §F** (only the cost values change to
40/60/50/80/120 and right HP to 8,750 / 10,000).* New text:
| Text | Content | Personality | Weight | Caps | Tracking | Glow/Stroke/Shadow | px@1080 | Fill hex |
|---|---|---|---|---|---|---|---|---|
| CdLabel (Lightning) | 12s | tabular numerals | Bold | lower 's' | 0 | 1px #06121c stroke + soft blue inner glow + drop-shadow | ~22 | #cfe6ff (icy blue) |
| CdLabel (Heal) | 5s | tabular numerals | Bold | lower 's' | 0 | 1px #07140a stroke + soft green glow | ~22 | #cdf3cf (life green) |
| CdLabel (Arrows) | 8s | tabular numerals | Bold | lower 's' | 0 | 1px #1a0c05 stroke + soft amber glow | ~22 | #ffe1b0 (amber) |

Cooldown labels render in their spell's elemental tint so the player parses element + readiness at a glance. When
a spell is **ready**, the CdLabel hides (no "0s"); when on cooldown it counts down whole seconds.

## G · MATERIALS
*Shared chrome materials = **08 §G**.* New materials:
- **Arcane spell tiles:** gold/bronze rim (as 08 frames) over a **dark arcane-glass** core (#0b0d16 @ ~80%) that
  carries the spell's inner colored glow: Lightning = electric blue #4aa8ff core with white sparks; Heal =
  verdant #46d06a with soft motes; Arrows = amber #ffae4a. Glass has a faint specular and a slight inner bloom;
  on cooldown the core desaturates and the CdVeil paints a dark radial sweep.
- **Spell icons:** Lightning = jagged electric **bolt**; Heal = a **life-rune/leaf-cross** (green); Arrows = a
  **fan of three arrows** (amber). Each glows additively.
- **Commander orb:** thick ornate **cast-gold ring** (base #8a6a2c, highlight #f6dd86, shadow #574114) with
  engraved filigree and a polished bevel; a soft gold **ready-glow** halo behind it; the inner **portrait** is a
  painted commander bust (faction-tinted, low-key lit) circularly masked; a thin inner gold liner separates ring
  from portrait. When charging, a dark radial veil overlays the portrait; when ready, the ring brightens + the
  halo pulses.
- **Targeting telegraph:** translucent **cobalt-blue** rings (#3f8bff @ ~70%, additive) with a brighter inner
  ring, a faint cross-grid compass, and a soft radial floor-glow that fakes a projected light circle on the
  ground; gentle rotation gives it "active scanning" life. Color stays **blue** (the player/Iron Pact aiming
  color) regardless of spell element, matching the mockup's blue reticle.

## H · COMPONENTS (each interactive)
*PauseButton, HP bars, ResChips, UnitBtn tiles, Garrison/Defend/Attack = **08 §H** (same behavior; only costs/
values differ).* New components:
1. **SpellBtn_Lightning / Heal / Arrows** — *purpose:* cast that spell. Lightning & Arrows are **ground-targeted**
   (tap arms → telegraph appears → tap/drag on the battlefield to place → release confirms); Heal may be
   **instant/self** (tap fires immediately). *States:* **idle/ready** = arcane frame, glowing icon, no CdLabel;
   **hover/focus** = frame + icon brighten; **armed** (ground-targeted, after first tap) = persistent selection
   ring + the TargetTelegraph shown on the field; **pressed/cast** = bright elemental flash + icon punch; **on
   cooldown** = CdVeil radial sweep + dimmed icon + CdLabel counting down (e.g. "12s"→…→hidden at ready);
   **disabled/unaffordable** = desat + (if a mana/gold gate exists) tint, raycast on for a deny shake.
   *Structure:* ArcaneFrame, Icon, CdLabel, CdVeil. *Feedback:* arm = soft hum; cast = elemental SFX + screen
   micro-flash; cooldown start = veil wipe. *(Spell effect is meta/sim; UI issues the cast request via the
   permitted control path; no balance written client-side.)*
2. **CommanderOrb (Ultimate)** — *purpose:* fire the commander's ultimate/ability. *States:* **idle/charging** =
   ring dim, Orb_CdVeil radial showing charge, portrait slightly desat; **ready** = ring bright + Orb_Glow
   pulsing halo + portrait fully lit (strong "use me" affordance); **hover/focus** = halo intensifies; **pressed/
   cast** = big gold flash + ring spin burst; **disabled** = greyed. *Structure:* glow + gold ring + circular
   portrait + cd/ready veil. *Feedback:* cast = heroic SFX + camera/field flash; then recharge veil restarts.
3. **TargetTelegraph** — *purpose:* show the chosen ground location while a targeted spell is armed. *States:*
   **hidden** (default); **aiming** = visible, follows the finger/cursor ground point, rings rotate; **valid** =
   blue (placeable); **invalid** = tinted red (out of range / blocked); **confirm** = quick bright pulse +
   collapse on release. *Structure:* glow + 3 rings + compass. *Feedback:* purely visual aiming aid; the actual
   placement/confirm is the cast tap on the battlefield. It is **not** a blocking button.

## I · ANIMATION TIMELINE
**OnShow:** identical edge-chrome slide-in to **08 §I**, PLUS:
| t (s) | Element | Action | Dur | Easing |
|---|---|---|---|---|
| 0.16 | SpellRow | slide up + stagger tiles 0.03s | 0.25 | ease-out |
| 0.20 | CommanderOrb | scale 0.85→1 + glow fade-in | 0.25 | back-out |
| 0.34 | CommanderOrb (if ready) | one-shot ready halo pulse | 0.40 | ease-in-out |

**Reactive:**
- Spell arm: tap → selection ring fades in (0.12s) + TargetTelegraph fades/scales in (rings from 0.6→1.0 over
  0.18s) at the aim point.
- While aiming: outer/mid rings counter-rotate (~6s/rev), inner pip breathes ±4% (~1.2s).
- Cast: elemental flash on tile (0.12s) + telegraph quick bright pulse then collapse (0.15s) → CdVeil begins a
  full radial sweep over the spell's cooldown (12s/5s/8s), CdLabel counts down.
- Commander ready: ring brightness +20% + halo breathe (±6%, ~1.6s) continuously until cast; cast = gold burst
  + spin then recharge veil from 0→full over the ult cooldown.
- Cooldown finish: CdVeil vanishes with a quick gold ring-flash; CdLabel hides.

**OnHide:** as 08 (chrome retracts); any active telegraph is force-hidden.

## J · PARTICLE & FX (passive — describe only)
- **Arcane tile cores:** slow drifting motes in each element's color (blue sparks / green life-motes / amber
  dust); faint inner bloom.
- **Commander orb:** soft rotating gold rim-glint + a gentle ready-halo pulse.
- **Telegraph:** rotating ring shimmer + soft projected floor-glow + tiny inward-drifting blue particles toward
  the center pip (reads as "channeling here").
- Shared 08 passives (gold rim-bloom, HP glows) still apply.
- No persistent particles clutter the battlefield center except the transient telegraph while aiming.

## K · EVENT BEHAVIOR
- **OnShow:** same as 08 plus spell/ult readiness subscribed (read-only) and rendered as cooldown veils/labels.
- **OnSpellTap (targeted):** arm → show telegraph → on battlefield release, issue the cast at the ground point →
  start cooldown. **OnSpellTap (instant, Heal):** fire immediately → cooldown. If on cooldown/disabled: deny
  shake.
- **OnCommanderTap:** if ready, fire ultimate → recharge; else pulse "not ready."
- **OnOrder / OnTrain / OnPause / battlefield taps:** exactly as **08 §K** (the spell layer is additive; the unit
  tray, order cluster, pause, and read-only readouts behave identically).
- **Cancel aim:** tapping the armed spell again, or a back/cancel gesture, hides the telegraph without casting.
- **OnHide:** retract chrome; release bindings; hide telegraph.

## L · NEGATIVE RULES (must-never)
- **Never** leave the TargetTelegraph on-screen when no spell is armed; it is transient.
- **Never** make the telegraph a raycast-blocking element that eats the battlefield confirm tap.
- **Never** show "0s" on a ready spell — hide the CdLabel at ready.
- **Never** recolor the telegraph by element — it stays the player's **blue** aiming color (per mockup).
- **Never** let the SpellRow/CommanderOrb overlap the GARRISON/DEFEND/ATTACK cluster or the central battlefield
  zone; stack them cleanly in the lower-right.
- **Never** violate §12: spell/ult casts go through the permitted control path; no client-side balance/effect
  mutation; everything else read-only.
- **Never** drop the shared 08 rules (faction color sides, clear center, unaffordable train state, single-select
  stance group).
- **Never** invent spell counts beyond the **three** slots + **one** commander orb shown.

## M · ACCEPTANCE CRITERIA (objective PASS)
- **≥95% fidelity** to `InMatchSpellHudDesign.png`: all shared 08 chrome present with this mockup's values
  (costs 40/60/50/80/120; right HP 8,750/10,000; gold 150; supply 2/8; army 15/50); a 3-slot SpellRow with
  Lightning "12s" / Heal "5s" / Arrows "8s"; a large gold-ringed CommanderOrb at bottom-right; and the blue
  concentric targeting telegraph over the battlefield — all per §E (±2%).
- **Hierarchy:** §C tree (telegraph above scrims; spell row + orb in SafeArea; veils/labels per spell).
- **Typography:** cooldown labels in elemental tints, tabular; hidden at ready; shared text matches 08.
- **States:** spells arm/cast/cooldown correctly; targeted spells show the telegraph; orb shows charging vs ready;
  telegraph is transient + non-blocking.
- **Clear center + safe area + §12:** inherited from 08 and verified (orb/spell row inside safeArea; telegraph in
  world space, clamped on-screen; only permitted writes).
- **Eye flow:** CommanderOrb (largest, glowing) → SpellRow → ATTACK → unit tray → top readouts.

## N · IMPLEMENTATION CONFIDENCE
**85 / 100.** The additive spell layer is structurally clear and fraction-specified, and the cooldown/telegraph
patterns are standard. The −15: (a) spell/ultimate art + the painted telegraph shader are assets to build;
(b) the cast/aim flow (arm→telegraph→confirm) and what control path the cast uses must be reconciled with the
real spell system within the §12 boundary (the mockup only shows the aiming state); (c) the exact left-HP-bar
placement in this frame (it reads lower-left, beneath the chip row) is slightly ambiguous vs 08's top-spanning
bars and is called out as an anchor decision.

## O · SELF-CHECKLIST
- □ Source PNG read forensically before writing (done).
- □ Sections A–O present, in order, substantive; deltas vs 08 clearly marked.
- □ Numbers captured: costs 40/60/50/80/120; HP 10,000/10,000 & 8,750/10,000; gold 150; supply 2/8; army 15/50;
  spell cooldowns 12s/5s/8s.
- □ Three spell slots + one commander orb (no invented extras).
- □ Telegraph is transient, non-blocking, stays blue, clamped on-screen.
- □ Shared chrome reuses 08 (sides locked, center clear, train-affordability, single-select stance).
- □ §12 respected for casting; everything else read-only.
- □ Fraction layout normalized to 2340×1080; tablet/ultrawide/notch covered.
- □ Hex + materials for arcane tiles, commander orb, telegraph.
- □ Header + Source line in required format.
