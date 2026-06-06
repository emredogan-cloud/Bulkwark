# BULWARK — GATE 1 · Phase E: Final Verdict

**Date:** 2026-06-06 · **Build:** `28099ec` (CI-GREEN) on Redmi Note 11R · **Sample:** 10 real matches, 390
telemetry samples. **Question (GATE 1):** *Is BULWARK currently fun to play?* **Answer with evidence only — no
fabrication, no optimism, no assumptions.**

---

## VERDICT: **FAIL**

BULWARK is **not currently fun**. It is a stable, presentation-complete shell with **no functioning contest
underneath**: the AI never builds an economy or army (frozen at 1 unit), no unit-vs-unit combat ever occurs, and
every match is a deterministic, probe-decided walkover.

## The evidence (each line is measured, not asserted)
| Claim | Evidence (n=10) |
|---|---|
| The AI never builds an economy or army | AI gold **== 5** every sample (390/390); **0 AI miners** (max 0.0/10); AI stuck at **exactly 1 unit (91% of samples)** — never trains more |
| There is no unit-vs-unit combat | **0 engaged-unit observations / 390 samples (0.0%)** — units walk **past** each other to the statues; none ever targets another |
| Stakes exist but aren't a contest | statue damage is **bidirectional** (AI's lone unit chips the player statue to **~459–472 in 9/10**; player chips the AI statue to **~154**) but with no economy growth + no combat there are no swings |
| No organic decision | **10/10 "Victory" by the `SimAiDriver` test probe** (statue HP → ≈ −99,900 at ~t116), identical timing |
| No agency / no variety | match duration **116.3 s ± 0.5**, identical outcome/shape regardless of play (9/10; m1 outlier) |
| Dead time | ~**63 s** before any statue is touched (9/10); battlefield shows **2–3 units** (`baseline_match1.png`); no fighting ever |

## Why this is FAIL (not CONDITIONAL PASS)
A CONDITIONAL PASS would require the *core loop to be fun with caveats*. Here the core loop **does not execute as a
contest at all**: the AI **never builds an economy or grows past 1 unit** (RC-1) so there is no opponent to play
against, **no unit-vs-unit combat** ever happens (RC-3), and there is **no win/lose condition the game itself
produces** (RC-4 — the probe ends every match). The lone AI unit *does* chip the player statue, so "stakes" nominally
exist both ways — but a one-unit, no-economy, no-combat poke is not a contest, so fun cannot be conditionally
credited. This is consistent with the prior verdicts (`GATE1_VALIDATION_REPORT.md` = FAIL) and is now **quantified
across 10 matches**.

## What is NOT the problem (scoped honestly)
- **Presentation is fine** — HUD, sprites, statue-HP bars, audio/VFX/animation all boot and render on device;
  app is stable (10/10 launches, no crashes; PSS ~334 MB). The Production-Presentation phase succeeded.
- The failure is **gameplay/AI/economy**, the exact area the deferred GATE-1 bugs cover.

## Root causes (detail in `GATE1_ROOT_CAUSE_ANALYSIS.md`)
**RC-1 AI economy never starts (CRITICAL)** · **RC-2 miners die/unreplaced (CRITICAL)** · **RC-3 no unit combat
(CRITICAL, downstream)** · RC-4 probe-decided matches (HIGH) · RC-5 deterministic walkover (HIGH) · RC-6 dead
time (MEDIUM). The two structural roots are **RC-1 + RC-2** — the documented deferred bugs.

## Validation of this verdict
Workflow: author → independent review → adversarial audit → device validation (10 matches) → this report. Evidence
is from a **read-only** telemetry logger (no gameplay/balance change) + device screenshots; all figures trace to
`runtime/device_validation/gate1/`. **No runtime evidence was fabricated.** Two independent loggers agree — the
campaign `[GATE1]` logger and the pre-existing in-build `[SIMPROOF]` logger (`SimDebugOverlay`): both report
`GoldAI=5, AI miners 0, Engaged=0, AI=Defend(x1), StatueP=446/1000`. An **adversarial audit re-parsed all 10 raw
logs**, confirmed the decisive figures (AI economy dead, 0/390 combat, 10/10 probe-decided), and **corrected
secondary over-pessimistic claims** (player-statue *is* chipped to ~459–472 in 9/10; AI fields 1 unit, not ~0;
first contact ~63 s, not ~96 s) — now reflected above. The **FAIL verdict was upheld** by the audit.

## Recommendation (not executed — out of scope for this campaign)
Fixing **RC-1 and RC-2** (single AI training authority + opening build order; exclude `MinerTag` from targeting +
a miner-floor maintainer) is the minimum to make a two-sided contest exist; combat (RC-3) should then engage and a
re-run of this exact campaign can re-evaluate. That is a **separately-authorized gameplay effort** — **not started
here**.

---

**STOP — per the campaign:** verdict delivered (**FAIL**). **No gameplay fixed. GATE 2 not started. Roadmap Phase 5
not started.** Evidence presented first, as required.
