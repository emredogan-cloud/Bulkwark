# BULWARK — Phase 5.2 Retention/Monetization Tuning Log (TEMPLATE — no entries yet)

**Status:** **METHODOLOGY + EMPTY LOG.** No live telemetry → **zero tuning changes recorded** (none can be
justified without data). **No fabricated results.** **Authority:** roadmap §13 Phase 5 ("Tuning only — no new
features"), §12 (RC: server-owned values; **server-side A/B**, no client-random), §9/§10 (the tunable
knobs + fairness limits); `PHASE_5_MASTER_EXECUTION_PROMPT.md` §F. Tuning is **RC value changes only** — no
app updates, no new systems, no code (uses the Phase-3 `EconomyResolver` RC→SO→literal pipeline).

## 1. Method (binding)
1. Every change is a **RemoteConfig value** delivered server-side; assignment is **server-side A/B** (§12).
2. Each entry states a **hypothesis**, the **metric watched**, and a **guardrail** that must not regress.
3. **Inviolable guardrails (never traded for a metric):** fairness/no-P2W, capped upgrades, disclosed odds,
   readability, server authority. A tuning change may **never** introduce power, raise a cap, or alter odds
   away from disclosure (§9/§10/§15). Ad cadence stays **opt-in only** (no interstitials).
4. Values stay within canon ranges (e.g. weekend multipliers ≤3× per `WeekendModifierDef`; gem earn/spend
   per §9). No new RC key/system is created — only canon-tunable values are adjusted.

## 2. Candidate knobs (what 5.2 would A/B — see `rc-experiment-configs.md`)
First-clear gem diminishing curve · reward pacing (Silver/PassXP) · battle-pass `passXpPerTier` ·
chest open timers · opt-in rewarded-ad gems-per-watch + daily cap · shop value-ladder presentation
(price points are §10-fixed; only presentation/featured rotation is tunable) · login-streak rungs.
**All exact arms TBD by LP from telemetry.**

## 3. Tuning log (chronological — EMPTY)
| Date | RC key (canon-tunable) | Control → Variant | Hypothesis | Metric watched | Guardrail (must hold) | Cohort/result |
|---|---|---|---|---|---|---|
| — | — | — | — | — | — | **none — DEFERRED (no live data)** |

## 4. Status / DEFERRED
Method defined; **no tuning performed** — requires a live limited-geo soft launch + real telemetry (DEFERRED;
blocked by GATE 1 OPEN / GATE 2 DEFERRED / GATE 3 DEFERRED; GATE 5 DEFERRED). No RC change is recorded or
recommended without data (§15.8).

**Inherited validation debt (preserved, NOT closed):** GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED ·
GATE 4 PASS (static) · GATE 5 DEFERRED.
