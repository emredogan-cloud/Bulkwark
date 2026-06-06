# BULWARK — GATE 1 · Phase B: Playtest Observations

**Date:** 2026-06-06 · **Matches observed:** 10 (build `28099ec`, Redmi Note 11R). Observation only — **nothing
fixed.** Each match = fresh launch → CLASSIC, watched end-to-end via screenshots + the `[GATE1]` telemetry +
flow logcat. Quantitative backing is in `GATE1_TELEMETRY.md`.

> Note on control: the build's in-match driver is the V2 auto-demo (it trains player miners → cheapest combat →
> advances). The Battle HUD's manual buttons exist and are tappable, but matches were observed under the standard
> auto-demo so all 10 runs are comparable. The findings below hold regardless of who issues the player orders,
> because the **AI opponent is the broken side**.

---

## Observed behavior (consistent across all 10 matches)
| Aspect | Observation |
|---|---|
| **Economy (player)** | Works. Gold cycles 5→~105 as miners earn and training spends. Player reaches ~2 miners + a few combat units. |
| **Economy (AI)** | **Dead.** AI gold sits at **exactly 5 the entire match, every match** (390/390 samples). The AI trains **no miners (0/10)** → no income → **never grows past its 1 starting unit.** |
| **Miner behavior** | Player miners reach ~2 then **decay to 0** by end in 9/10 matches (they leave/die and are not replaced). AI never has a miner at all. |
| **AI decisions** | The AI fields **exactly 1 unit (91% of samples)** in a "Defend" stance; `SimAiDriver` advances it and it **does chip the player statue** — but the AI takes **no economic action and never builds an army**, so it cannot scale or contest. |
| **Combat pacing** | **There is no unit-vs-unit combat.** `AttackState.Target` is null for every unit in every sample (0/390). The two small forces **walk past each other** to the opposing statue; they never trade blows. |
| **Time-to-first-contact** | First statue damage at **~t63 s** (9/10; ~t78 in m1) — i.e. ~63 s of build-up with no fighting, then a slow mutual statue-chip until the probe ends it. Unit-vs-unit combat: **never.** |
| **Statue pressure** | **Bidirectional but un-contested:** the player statue is chipped to **~459–472** in 9/10 (1000 only in m1); the AI statue is chipped further to **~154**. Both sides poke the opposing statue, but with no army growth and no combat there are no real swings or comebacks. |
| **Stalemates / deadlocks** | The match **never reaches an organic decision** — it is force-ended by the `SimAiDriver` probe at ~t115 in 10/10. Without that scaffold it would not resolve as a contest. |
| **Player agency** | Minimal/none in practice: with no opponent, no choice (which unit, when to push) is punished or rewarded — outcomes are identical (10/10 Victory, ~116 s) regardless. |
| **Frustration moments** | The ~63 s of empty waiting before anything touches a statue; a battlefield that is visibly **near-empty** (2–3 tiny units) the whole time; no fighting to watch at any point. |
| **Waiting periods** | Dominant. The match is mostly idle build-up against a non-acting opponent, then a few seconds of unopposed statue damage. |

## Major gameplay issues recorded (not fixed)
- **ISSUE-1 — AI economy never starts** (gold == 5, 0 AI miners, 10/10). The AI never builds an economy and so
  never grows past its 1 starting unit — it cannot scale or contest.
- **ISSUE-2 — Miners die/leave and aren't replaced** (player miners decay to 0 by end in 9/10; AI has none).
- **ISSUE-3 — No unit-vs-unit combat ever happens** (0/390 samples engaged). Units walk past each other to the
  statues — the core loop's payoff (units fighting) is absent.
- **ISSUE-4 — Matches are decided by a test probe, not by play** (10/10 probe-Victory at ~t116, identical timing).
- **ISSUE-5 — Deterministic, agency-free walkover** (9/10 near-identical; no decision changes the result).
- **ISSUE-6 — Long dead time** (~63 s before any statue contact; 2–3 units on screen; no fighting at any point).

These map 1:1 to the three deferred GATE-1 bugs (BasicAI vs SquadAI economy conflict; miner targeting death; miner
replacement) plus their downstream effects. Root causes + classification: `GATE1_ROOT_CAUSE_ANALYSIS.md`.

**No changes were made. Nothing was fixed. This is observation only.**
