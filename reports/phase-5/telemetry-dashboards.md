# BULWARK — Phase 5.1 Telemetry & Dashboards (READINESS PLAN — no live data)

**Status:** **DEFINITIONS / SPECS ONLY.** No soft launch has occurred; **no metric in this document is
measured** — every value cell is a placeholder to be filled from live telemetry at 5.1. **No fabricated data.**
**Authority:** roadmap §12 (one consolidated analytics pipeline; consent/UMP; minimize PII; never log
save-state), §13 Phase 5, §10 KPIs; `PHASE_5_MASTER_EXECUTION_PROMPT.md` §F. **Builds on the EXISTING Phase-3
§3.5 taxonomy** (`Assets/_Services/Analytics/Events.cs` → `AnalyticsEvents` + `GameAnalytics`) — **no new
system; no new code authored here** (instrumentation is applied at 5.1, owner/runtime).

## 1. Soft-launch funnel (definition)
```
install → first_open → FTUE_complete → first_battle → battle_win →
  D1_return → first_session_len → first_purchase(opt) → D7_return → D30_return
```
Each step is a conversion node; the dashboard reports step count + step→step conversion %. **(All %s TBD — live.)**

## 2. Event taxonomy — reuse + the additive extensions to instrument at 5.1
**Already in `AnalyticsEvents` (Phase 3, reuse as-is):** `app_start`, `battle_start`, `battle_end`,
`level_clear`, `draft_complete`, `upgrade_purchase`, `commander_level`, `currency_grant`, `currency_spend`,
`endless_wave`, `ladder_submit`, `rc_activated`, `ad_optin`.

**To ADD when instrumenting 5.1 (additive to the SAME pipeline — names defined here, code not authored, §E
"no new code files"):** `session_start` / `session_end` (len bucket), `ftue_step` (index), `ftue_complete`,
`d1_return` / `d7_return` / `d30_return` (server-derived from trusted time), `iap_purchase` (productId, price
band — never a balance, §12), `pass_tier_claim`, `chest_open` (tier), `rewarded_ad_complete`. **Privacy
(INVIOLABLE, §12):** consent-gated (UMP), minimized non-PII params, **never** a wallet/profile/save payload;
currency events carry delta + currency name + reason code only.

## 3. Dashboards (specs — columns only, no data)
| Dashboard | Key panels (all values TBD — live) | Source events |
|---|---|---|
| **Retention** | D1 / D7 / D30 by cohort + geo | `session_start`, `d*_return` |
| **Funnel** | install→…→first_purchase conversion % | §1 funnel events |
| **Monetization / LTV** | ARPDAU, conversion %, blended **D30 LTV**, by-product revenue | `iap_purchase`, `currency_*`, `pass_tier_claim` |
| **Engagement** | sessions/DAU, avg session len, battles/DAU, mode mix | `session_*`, `battle_*`, `endless_wave`, `ladder_submit` |
| **Stability/Perf** | crash-free %, ANR %, frame-time p50/p95 by device tier | crash/perf SDK (5.3) |
| **Ads (opt-in)** | rewarded views/DAU, completion %, gems/DAU from ads | `ad_optin`, `rewarded_ad_complete` |

## 4. GATE-5 metric definitions (formula; thresholds live in ADR-5-002)
- **D1 / D7 / D30 retention** = returning cohort members / cohort size at day N (trusted-time, server-derived).
- **Blended D30 LTV** = total revenue attributable to a D0 cohort by D30 / cohort size (model + window pinned by **ADR-5-002**).
- **GATE-5 SCALE rule** = `D1 ≥ floor_D1 AND D7 ≥ floor_D7 AND blendedD30LTV ≥ targetCPI` → recommend SCALE; else STOP/RESCOPE.
  The floor values (`floor_D1`, `floor_D7`, `targetCPI`) are **owner/LP-set in ADR-5-002** (currently TBD —
  STOP-blocking). **No value is assumed here.**

## 5. Status / DEFERRED
| Item | Status |
|---|---|
| Funnel + dashboard **definitions** | **Done (plan)** |
| Event-taxonomy **extension names** | **Defined** (code DEFERRED to 5.1 instrumentation) |
| Live event integrity verification | **DEFERRED** (needs a live build + analytics provider) |
| Any measured retention/LTV/funnel number | **DEFERRED — not produced; never fabricated** |

**No live telemetry exists. GATE-5 metrics are unmeasured and remain DEFERRED until a real limited-geo soft
launch on a playable build (GATE 1) with a live backend (GATE 3) + ADR-5-002 values.**

**Inherited validation debt (preserved, NOT closed):** GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED ·
GATE 4 PASS (static) · GATE 5 DEFERRED (un-evaluable without live data + ADR-5-002).
