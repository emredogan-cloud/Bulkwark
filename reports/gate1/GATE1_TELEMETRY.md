# BULWARK — GATE 1 · Phase C: Gameplay Telemetry

**Date:** 2026-06-06 · **Source:** 10 real matches on the Redmi Note 11R, build `28099ec`. Each match = a fresh
app launch (fresh ECS world) → CLASSIC. The read-only `[GATE1]` logger sampled ECS state every ~3 s (39 samples /
match, 390 total). **No fabrication — every figure is parsed from `runtime/device_validation/gate1/gate1_m1..10.txt`.**
Fields per sample: gold P0/AI, units P0/AI, miners P0/AI, **engaged-in-combat** P0/AI (`AttackState.Target != Null`),
statue-HP P0/AI, outcome.

---

## Aggregate (n = 10 matches)
| Metric (required) | Value | Note |
|---|---|---|
| **Average match duration** | **116.3 s** (115.2–116.9) | extremely tight → deterministic |
| **Victory conditions reached** | **10/10 "Victory"** | **all decided by the `SimAiDriver` test probe** (statue-HP spikes to ≈ −99,900 at ~t115), **not** by two-sided play |
| **Average units spawned (peak alive)** | player **2.4** · AI **1.0** | tiny "armies"; AI fields **exactly 1 unit in 91% of samples** (354/390) — never trains more |
| **Average miners alive (max)** | player **2.1** · AI **0.0** | **AI never trains a single miner (0.0 in 10/10)** |
| **Average gold income** | player gold cycles **5→105** (earns + spends) · **AI gold = exactly 5, all 390 samples** | **AI economy collapsed in 10/10** (`gA == 5` every sample — never earns) |
| **Average combat frequency** | **0 engaged-unit observations / 390 samples (0.0%)** | **no unit ever engaged another unit** — units ignore each other (see below) |
| **Average statue damage** | **bidirectional:** player units chip the AI statue to **avg ~154 / 1000**; the AI's lone advanced unit chips the **player** statue to **~459–472 / 1000 in 9/10** matches (1000 only in the m1 outlier) | both sides march **past** each other to the opposing statue |
| **Stalemate frequency** | **10/10 reach no organic decision** → force-ended by the probe | effectively a 100% non-resolving rate without the scaffold |

## Time-series shape (m2 — typical of 9/10 matches; m1 is the lone outlier)
```
t=3    gP=50 gA=5  uP=0 uA=1  mP=0 mA=0  atk 0/0  statue 1000/1000   (setup; AI already has its 1 unit)
t=63   gP=.. gA=5  uP=2 uA=1  mP=1 mA=0  atk 0/0  ~990/~990          (units cross; BOTH statues start chipping)
t=90   gP=.. gA=5  uP=2 uA=1  mP=1 mA=0  atk 0/0  ~600/~400          (bidirectional, asymmetric)
t=114  gP=26 gA=5  uP=1 uA=1  mP=0 mA=0  atk 0/0  472/162           (player ahead; player miners have died → mP=0)
t=116  ...                                    statue 472/-99886  out=Victory   (PROBE force-end at ~t116)
```
> **Outlier — m1:** AI unit stays back (uA=0 for most of the match), player reaches 6 units, player statue
> untouched (1000), AI statue → 98. Only 1/10 looks like this; the report does NOT generalize from it.

## What the numbers establish (no interpretation beyond the data)
1. **AI economy is dead in 100% of matches** — `gA == 5` in 390/390 samples; AI miners = 0 throughout.
2. **AI never grows its army** — `uA == 1` in 91% of samples (it fields its single starting unit and trains
   nothing more); player peaks at ~2 (m1 outlier: 6).
3. **Unit-vs-unit combat = 0** — `atkP+atkA = 0` across all 390 samples. Both sides' units **march past each other**
   to the opposing statue and chip it (statue damage is a separate path from `AttackState`); they **never fight each
   other**. The core combat loop is not exercised.
4. **Bidirectional but un-contested statue chipping** — the AI's advanced unit drops the **player** statue to
   ~459–472 in **9/10** matches (1000 only in m1); the player's slightly larger force drops the **AI** statue
   further (~154). Pressure exists both ways, but with **no economy growth and no combat** it is not a contest.
5. **Every "Victory" is the probe, not the game** — `sA` spikes to ≈ −99,900 at ~t116 (the `SimAiDriver`
   validation probe), identical timing across all 10 → the match has **no organic win/lose condition**.
6. **First statue contact ~t63** (9/10; ~t78 in m1) — i.e. ~63 s of build-up with no fighting, then a slow
   mutual statue-chip until the probe ends it.

> **Independently corroborated** by the in-build `[SIMPROOF]` logger (`SimDebugOverlay`): at t≈120 it reports
> `GoldAI=5, Miners AI0, Engaged=0, AI=Defend(x1), StatueP=446/1000, fps≈30` — a second, separate logger agreeing
> with the `[GATE1]` figures.

> The Phase-A device-readiness, Phase-B qualitative observations, and Phase-D root causes accompany this data.
> **Telemetry was produced by read-only instrumentation; no gameplay/balance was changed to obtain it.**
