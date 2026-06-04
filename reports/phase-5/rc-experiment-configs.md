# BULWARK — Phase 5.2 RemoteConfig Experiment-Config Templates (values only; arms TBD)

**Status:** **TEMPLATES — no live experiment, no chosen values.** Defines the **canon-tunable knobs** the
soft-launch A/B tests would adjust; **every control/variant value is TBD by Live-ops/Product from telemetry**
(none invented here — §15.6). **No new RC keys, no new systems** (§13 Phase 5 "no new features"; §E "values
only, no new systems"). Delivered server-side via the **Phase-3 `EconomyResolver` (RC→SO→literal)** pipeline;
assignment is **server-side A/B** (§12, no client-random).

## 1. Guardrails (binding on EVERY experiment arm)
- **Inviolable, never an experiment variable:** no P2W / no sellable power; upgrade **caps** fixed; chest
  **odds disclosed** (an arm may not hide/alter disclosed odds away from what the UI shows); **readability** /
  clarity-mode on; **server authority** over currency; ads **opt-in only** (no interstitial arm).
- **Within canon ranges only** (e.g. `WeekendModifierDef` multipliers ≤3×; gem earn/spend per §9; §10 price
  bands fixed — only presentation/featured rotation is tunable, not price-as-power).
- **4 currencies only** (Gold/Silver/Gems/PassXP); no new currency arm.

## 2. Knob templates (canon surface → tunable value; arms = TBD)
| Knob (canon surface) | What it tunes | Control | Arm A | Arm B | Guardrail |
|---|---|---|---|---|---|
| First-clear gem curve (`EconomyConfig`, §9 `max(floor, base−dec×plays)`) | early-game gem pacing | default (SO) | TBD | TBD | diminishing; earned-only |
| Reward pacing — Silver/PassXP (`EconomyConfig`) | progression feel | default | TBD | TBD | no power; capped progression intact |
| Battle-pass `passXpPerTier` (`BattlePassDef`) | pass grindiness | 1000 (S0 data) | TBD | TBD | earn-by-play; cosmetics/currency only |
| Chest open timers (`ChestDef.openMinutes`) | free-loop pacing | per-tier (SO) | TBD | TBD | gems skip = convenience only |
| Rewarded-ad gems/watch + daily cap (`RewardedAdService`) | opt-in ad lever | default | TBD | TBD | **opt-in only**; respectful cap |
| Login-streak rungs (`LoginStreakLadder`) | retention cadence | default | TBD | TBD | gameplay-safe; capped loop |
| Shop featured rotation (`ShopService.SetFeatured`) | weekly merchandising | default | TBD | TBD | no FOMO/dark pattern; prices §10-fixed |
| Weekend modifier multipliers (`WeekendModifierDef`) | weekend reward boost | 1× | TBD (≤3×) | TBD (≤3×) | Silver/PassXP only; existing-rule allow-list |

*(Exact RC key strings map to the existing Phase-3/Phase-4 config surfaces above; no new key is introduced.
Values shown as "default" are the already-authored canon/SO values; arms are LP-set from telemetry.)*

## 3. Experiment design (method)
Server-side A/B per §12: stable cohort assignment, one variable per experiment where feasible, pre-registered
hypothesis + primary metric + guardrail (see `tuning-log.md`), minimum cohort/duration per **ADR-5-002**.

## 4. Status / DEFERRED
Templates + guardrails defined; **no experiment configured or run, no value chosen** — requires a live soft
launch + telemetry + RC backend (DEFERRED; GATE 1 OPEN / GATE 2 DEFERRED / GATE 3 DEFERRED; GATE 5 DEFERRED).
No value is invented (§15.6).

**Inherited validation debt (preserved, NOT closed):** GATE 1 OPEN · GATE 2 DEFERRED · GATE 3 DEFERRED ·
GATE 4 PASS (static) · GATE 5 DEFERRED.
