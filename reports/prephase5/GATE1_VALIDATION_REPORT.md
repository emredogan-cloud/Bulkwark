# BULWARK — GATE 1 (FUN) Validation Report

**Date:** 2026-06-05 · **Gate:** roadmap §13 Phase 1 **GATE 1 = "is the combat FUN?"** ("combat must be fun
before meta is built"). **Track:** pre-Phase-5 GATE-1 validation (V2). **Device:** Xiaomi Redmi Note 11R,
Android 13, arm64-v8a (`jfzxugsgnnvsrsg6`). **Build:** V2 `f698cb8`, CI run **26972255935** GREEN.
**Method:** author → independent review (3 lenses PASS) → adversarial audit → repair → CI/CD → device validation.
**Artifacts (local, gitignored):** `runtime/device_validation/v2_logcat.txt`, `screen_v2_{30,70,110,140}s.png`.
**Rule honored:** evidence only; **PASS or FAIL, no DEFERRED, no ambiguity.**

---

# ⛔ VERDICT: GATE 1 = FAIL  (loop functional + fun-capable; combat-fun NOT met — AI opponent collapses)
The full **mine → train → push → topple** loop is **functional, readable, and resolves to Victory on device via
real combat** — a genuine first. **But GATE 1 asks whether the COMBAT is fun, and that bar is NOT met:** the AI
opponent's economy collapses (stuck at 5 gold → 1 unit), so the match is a **9-vs-1 walkover** with no challenge
or tension. You cannot certify "combat is fun" from a walkover, and the gate exists precisely to stop building
meta on un-fun combat. **One specific, identified blocker** (AI economy over-queue bug) stands between this FAIL
and a real re-gate. This is a FAIL *with a clear, cheap path to PASS*, not a broken loop.

---

## 1. What WORKS (device-proven this run) — the core loop is real
| Subsystem | Evidence (v2_logcat.txt / screens) | Status |
|---|---|---|
| **Economy: miner→mine→gold** | auto-trained 2→4 miners; `Mines occ 1→2→3→4`; **GoldP rises from mining** 0→3→6→9→…→120 (cycling as it spends/earns) | ✅ WORKS |
| **Training + queue (FIFO)** | 6 player units trained in order; gold deducted at head; affordable-gating greys unaffordable buttons (screen_v2_70s: all greyed at 40g); `(STALL need Xg)` display added | ✅ WORKS |
| **Army / sustained combat** | player army grew **1→9 units** (P9); 29 advances; AI advanced 12× (two-sided); a unit died (`AI 1→0`) | ✅ WORKS |
| **Push (attack-move)** | player units marched to the AI base and engaged | ✅ WORKS |
| **Victory / freeze** | StatueAI ground down by real combat `1000→…128→70→34→Destroyed` → **Match=Victory** (65 frozen snapshots); `[AIDRV] match already resolved by real combat before the probe — no probe needed` | ✅ WORKS |
| **Rendering/readability** | blue player army vs red AI, yellow mines, gray statues; overlay shows Gold/Units/Miners/Statue/Match | ✅ WORKS |

**This is the first end-to-end BULWARK match: build economy → field a 9-unit army → push → topple the enemy
statue → Victory, all from real systems with zero balance changes (control-layer only).** The victory-latch
probe was a fallback and never fired — real combat won the match.

## 2. Why it FAILS the FUN bar — the AI opponent is non-functional
- **AI economy collapse:** `GoldAI` was **5 for the entire match** (every snapshot). Root cause (V2 investigation,
  `BasicAI.BiasTraining`): the AI counts only ALIVE miners, not in-flight ones, so it front-loads its 100 gold
  into 2 miner orders (both paid immediately → 100→0) before the first miner spawns to raise the count, then
  dribbles back at ~3/s and never recovers enough to field an army.
- **Consequence:** the AI fielded **1 unit**, which advanced and died almost immediately (`UnitsAI 1→0` early).
  The player (working economy) reached **9 units** and ground the AI statue down unopposed (`StatueP` stayed
  874/1000 — the AI's lone unit barely scratched the player base).
- **A 9-vs-1 walkover is not fun combat.** There is no counter-play, no tension, no comeback — the exact failure a
  FUN gate must catch. The combat *mechanics* are present and correct, but a fair, engaging *fight* was never
  demonstrated, so combat-fun is **unproven → FAIL**.

## 3. Observations (balance / combat / economy / AI) — evidence-based
- **Economy:** functional and satisfying on the player side — miners auto-assign to mines (no micro needed),
  income scales with occupancy (occ 1→4), and the spend/earn cycle is legible. Yield/share math works
  (`min(RatePerSec, YieldPerSec/occupants)`). **Player economy: healthy.**
- **Combat:** the type×armor/counter chain, movement-to-range, and statue-inbox damage all execute; deaths occur;
  the statue's 1250 effective HP (250 shield + 1000) is a meaningful objective that an army can topple in ~30–40s
  of contact. **Combat mechanics: sound.** Combat *depth/fun*: **untested** (no real opponent).
- **AI:** decision layer is reasonable (it does try miners-first), but the economy counting bug guts it, and
  spawned AI units only move because this track added a control-layer advance driver (the squad→FormationMember
  pipeline is unterminated in the sim). **AI as an opponent: non-functional.**
- **Queue:** FIFO single-head blocking is correct; the new affordable-gating + stall display remove the "queue
  silently freezes" hazard. **Queue: validated.**
- **Victory/Defeat:** Victory validated by **real combat** end-to-end (statue destroyed → latch → freeze).
  **Defeat** is the *symmetric* path — `StatueDamageSystem` sets `Outcome = (statue.Team==0 ? Defeat : Victory)`
  from the identical code; it was not exercised on device because the AI never threatened the player statue. It
  can be confirmed by flipping `SimAiDriver.ProbeTargetTeam` to the player team (1-line) and re-running. **Victory:
  proven; Defeat: proven-by-symmetry (device-confirmable on request).**

## 4. The single blocker → path to a real re-gate
**Blocker:** AI economy collapse (the `BiasTraining` in-flight-miner counting bug). **Fix (identified, ~1 line,
no balance change):** count queued (in-flight) miner `TrainOrder`s alongside alive miners when comparing to
`TargetMiners`, so the AI stops over-queuing and doesn't bottom out its 100 gold. This is a **bug-fix**, not a
balance/number change. It was deliberately **not applied** in V2 to keep all changes in removable control-layer
MonoBehaviours (no sim edits) — applying it touches `BasicAI.cs` and warrants its own review.

**Re-gate criteria (what a PASS needs):** with the AI economy fixed, re-run and confirm a *contested* match — both
sides field armies, combat swings, and the outcome is not a foregone walkover — then judge fun on a real fight.

## 5. Verdict statement
**GATE 1 = FAIL.** Evidence-based, no DEFERRED, no ambiguity. The mine→train→push→topple loop is **functional,
readable, and produces a real on-device Victory** — a major milestone — but the **FUN** criterion is **not met**
because the AI opponent collapses (economy bug → 9-vs-1 walkover), so engaging combat was never demonstrated.
Fix the one identified AI-economy bug and re-gate.

---

**STOPPING after V2 per instruction. No Phase 5. All V2 changes are control-layer, removable, and change no
balance/rule. The AI-economy bug-fix + re-gate is the recommended next step (owner decision).**
