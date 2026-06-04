# BULWARK — PHASE V1 (Playability) Report

**Date:** 2026-06-04 · **Track:** temporary pre-Phase-5 GATE-1 validation (`PREPHASE5_VISUALIZATION_ROADMAP.md`).
**NOT Phase 5. No roadmap/canon/balance change.** **Device:** Xiaomi Redmi Note 11R, Android 13, arm64-v8a.
**Build:** SimPlayerHud `69ae2b1` → auto-demo `519ddc9` (v0.0.4x), CI GREEN.
**Method:** author → independent review → adversarial audit → repair → CI/CD GREEN → device validation.
**Artifacts (local, gitignored):** `runtime/device_validation/v1*_logcat.txt`, `screen_v1*_*.png`.

---

# ✅ VERDICT: V1 DELIVERED & DEVICE-VALIDATED — first observable combat achieved
The player can **train units, spend gold, watch units move, and observe combat** — all proven on device. A
single trained player Skirmisher advanced across the front and **damaged the enemy statue (1000→674)** while
**units took damage and one was killed** — the first end-to-end mine-less *train → push → fight* slice rendered
live.

---

## 1. What was delivered (`Assets/_Game/Bootstrap/SimPlayerHud.cs` — removable, §12 control layer)
A debug HUD MonoBehaviour that turns button input into the sim DATA writes the existing ECS systems already
consume (like `BattleInput.cs`) — **no new rule, no balance value, no catalog/stat edit.**

| V1 focus deliverable | Status | Evidence |
|---|---|---|
| Gold display | ✅ | HUD "Gold P0=…"; tracked 100→10 on device |
| Unit-production buttons | ✅ | 6 buttons, each `[i] Role — cost g` (Frontline 120 / Miner 50 / Skirmisher 90 / Heavy 160 / Ranged 100 / Caster 130); taps enqueue (`[HUD] TRAIN…`, queue grows) |
| Train-queue display | ✅ | HUD `queue[n]: #idx(wait/secs)` |
| Player-side training | ✅ | `[HUD] AUTO-DEMO train …#2`; **GoldP 100→10** (paid 90), **UnitsP 0→1** (player Skirmisher spawned) |
| First observable combat | ✅ | **StatueAI 1000→674**; `dHealth=-31`, `-71`; **`dAlive=-1` (a kill)** |
| Pause | ✅ (rendered) | `Time.timeScale` toggle button (not stress-tested) |

## 2. The combat-enablement insight (no sim change)
`MovementSystem` moves a unit **only toward `Targeting.Current`**, and `TargetingSystem` only acquires targets
within a unit's ±4 X-bin neighbourhood; spawned units aren't assigned to squads (deferred). So units near their
own base, with the enemy base out of bin-range, **never advance and never fight** (this is exactly why the
earlier scenes were static). The HUD's **ADVANCE / attack-move** issues `MoveDestination{enemyStatuePos,
Active=1}` on Team-0 units — the standard 1.3→1.4 control path (`PossessControl`/`MovementSystem` drive any
unit whose `Active!=0`) — so player units march to the enemy, auto-acquire targets en route, and fight. **Pure
player input; the sim's rules/balance are untouched.**

## 3. On-device evidence (build 519ddc9, auto-demo)
```
[HUD] AUTO-DEMO train cheapest combat unit #2 (gold 100).     ← trains Skirmisher (90g)
GoldP=100 … Units=0   →   GoldP=10 … Units=1(P1/AI0)          ← paid 90, player unit spawned
[HUD] AUTO-DEMO advance (1 player units).  (×8)               ← attack-move to AI statue
… StatueAI=1000/1000 → StatueAI=674/1000 …                   ← player unit DAMAGED the enemy statue
dHealth=-31,1 · dHealth=-71,1 · dAlive=-1                     ← combat damage + a KILL
[PROXY] proxies=6                                             ← blue player + red AI + 2 statues + 2 mines rendered
```
`screen_v1c_24s.png`: a **blue player Skirmisher** (first player unit — team color confirmed) advancing toward
the **red AI unit** between the gray statues. After the hit, `StatueAI` recovered (674→1000) via the
`StatueDamageSystem` trickle/shield mechanic, because the lone Skirmisher died and pressure stopped — expected
with one unit.

## 4. Tooling note (why an auto-demo)
On-device `adb input tap` coordinates **drift** (Android system-bar insets shift Unity's IMGUI layout between
launches), so scripted per-button taps sometimes landed on an **unaffordable** unit (e.g. Heavy 160g), whose
order then stalled the FIFO queue (gold never spent). The HUD buttons themselves are proven (taps enqueue; gold/
costs/queue render correctly). To make first-observable-combat **deterministically reproducible**, a clearly
labeled **debug auto-demo** (timer-triggered, ~4 s train cheapest affordable combat unit, ~12 s advance,
re-issued every 5 s) drives the **same** control writes the buttons do. It is removable and changes no rule.

## 5. Exit-condition assessment — "train units · spend gold · watch units move · observe combat"
- **Train units:** ✅ player Skirmisher trained from a button/auto-demo.
- **Spend gold:** ✅ GoldP 100→10 (paid 90).
- **Watch units move:** ✅ blue player unit advanced across the front to the enemy base (proxies + statue-range arrival).
- **Observe combat:** ✅ enemy statue damaged 1000→674; unit damage `dHealth -31/-71`; a kill `dAlive=-1`.
**All four exit conditions met and device-validated.**

## 6. Honest limitations (for V2 / later — NOT changed here)
- **Economy is the limiter:** 100 starting gold + no income (no miners mining) → the player fields **one** unit
  at a time; deeper, sustained combat (breaking a statue) needs the economy loop (miners → mines → gold). That
  is gameplay/AI tuning for V2 (GATE-1 fun verdict) or a later pass — out of V1 scope.
- **Units don't auto-advance** without an order (no squad assignment / no advance-when-no-target); the HUD's
  attack-move is the current bridge. A real game needs auto-advance or full squad control (later).
- **Overlay `Engaged` metric** reads the retired `AttackState.Target` field (Phase-1 combat uses
  `Targeting.Current`), so it shows 0 even during combat — a cosmetic overlay bug; the statue-HP/health/death
  deltas are the true combat signal. (Fixable in the debug overlay later.)

## 7. Status
- `SimPlayerHud` (HUD + attack-move + debug auto-demo): temporary, removable, read-only-except-permitted-control-writes.
- `SimProxyRenderer` (V0) + `SimDebugOverlay` remain for visualization/observability.
- The debug auto-demo can be turned off (`_autoDemo=false`) or the file deleted to remove V1 entirely.

**STOPPING after this report per instruction. Do NOT start V2. Do NOT start Phase 5. No roadmap/canon/balance change.**
