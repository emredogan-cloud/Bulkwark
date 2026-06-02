# BULWARK — Phase 4 Master Execution Prompt: Monetization & Live-ops Shell

**Mission.** Add the **ethical, cosmetic-led** monetization and retention shell: outfit-class cosmetics (gameplay-safe), Battle Pass S0, a transparent shop + IAP + opt-in rewarded ads, disclosed-odds chests + gem rules, and daily/weekly/weekend retention loops — then pass a **fairness audit (GATE 4)**.
**Scope.** Roadmap §13 **Phase 4 (4.1–4.5)** + **GATE 4** only.
**Inputs (read first).** Roadmap §6 (outfit-class cosmetic framework + cosmetic-safety rule + clarity mode), §8 (chest architecture — disclosed odds, timers, no power, dupe protection), §9 (gem economy + "gems CANNOT"), §10 (IAP/value ladder/ethical rules), §13 Phase 4, §3 GATE 3; `report/PRODUCTION_DECISION_LOG.md` §1 (CUT: loot boxes, interstitials, energy, P2W, paid random boxes).

> **UNIVERSAL PREAMBLE (binding).** Roadmap = law; decision log = binding; canon docs immutable. **No phase skipping** (GATE 3 PASS). **No unauthorized features.** **No hidden design or MONETIZATION changes** — monetization is fixed by §8/§9/§10; pricing/fairness changes need an ADR you cannot self-approve. **No canon drift.** **Inviolable constraints — especially FAIRNESS/NO-P2W and READABILITY (clarity mode).** §15 CUT list is a HARD prohibition (no loot boxes/gacha-for-power, no interstitials, no energy gates, no sellable power, no paid random boxes without disclosed odds + dupe protection). **Stop on ambiguity → ADR. ADR for deviations (inviolable constraints non-overridable). Evidence-first. Stop at GATE 4.**

## A. Context
This is where ethics are enforced in code. The original's opaque chests + interstitials are **[CUT]** (decision log §1). BULWARK monetizes via cosmetics + battle pass + transparent IAP + opt-in rewarded ads, with chests as an **earned, disclosed-odds, cosmetic-only** loop. Cosmetics may **never** affect power or readability (§6). Currency stays server-authoritative (Phase 3).

## B. Objectives (1:1 with roadmap §13 Phase 4)
1. **4.1** Cosmetic system: outfit-class tiers (palette/material/VFX over **locked silhouettes**) + **ranked clarity mode**.
2. **4.2** **Battle Pass S0**: dual track (free+premium), earnable by play.
3. **4.3** Shop + **transparent IAP** (value ladder) + **opt-in rewarded ads** (no interstitials).
4. **4.4** **Chests** (disclosed odds, timers, slots, dupe protection, **no power**) + gem rules per §9.
5. **4.5** Retention loops: daily/weekly quests, login streak, **basic weekend modifiers** (not the full event engine — that is Phase 7).

## C. Dependencies
- **Entry:** Phase 3 GATE 3 = PASS (server-authoritative economy live).

## D. Files expected to exist
- Phase-3 server-authoritative wallet + capped progression + modes + RC/analytics; prior FINAL_REPORTs (gates PASS).

## E. Files to create
- `Assets/_Game/Cosmetics/{OutfitClass,ClarityMode}.cs` + `Assets/_Game/Data/Cosmetics/` (skins as recolor/VFX over locked silhouettes).
- `Assets/_Services/Monetization/{BattlePass,Shop,IAP,RewardedAds}.cs` (server-validated entitlements).
- `Assets/_Services/Rewards/{Chests,GemRules}.cs` (disclosed-odds tables, timers, dupe→shard conversion).
- `Assets/_Services/LiveOps/{Quests,LoginStreak,WeekendModifiers}.cs`.
- `reports/phase-4/FINAL_REPORT.md`.

## F. Tasks
1. Implement the outfit-class cosmetic system honoring the §6 safety rule (no silhouette/size/hitbox/timing/read change); implement ranked **clarity mode** (opponents render read-safe) (4.1).
2. Implement Battle Pass S0 (50 tiers, dual track, earn-by-play, premium upgrade) with server-owned progress (4.2).
3. Implement the shop + transparent IAP value ladder ($0.99→$99.99, shown value) + **opt-in rewarded ads** (no interstitials); first-purchase offer (4.3).
4. Implement chests with **disclosed odds**, open timers + slot cap, **cosmetic+currency only (no power)**, and **duplicate→shard** protection; implement the §9 gem rules (earned/stored/spent-on-convenience; enforce the "gems CANNOT" prohibitions) (4.4).
5. Implement daily/weekly quests, login streak, and basic weekend modifiers (data-driven) (4.5).

## G. Validation gates
- Cosmetic-safety automated check: every cosmetic preserves silhouette/size/hitbox/animation-timing; clarity mode active in ranked. Pass entitlements are server-validated. Shop/IAP shows transparent value; ads are opt-in only. Chests show odds; contain **no power**; dupes convert. Gems cannot buy power/uncapped upgrades/ranked advantage.
- **GATE 4 (FAIRNESS AUDIT):** **zero P2W; readability intact; disclosed odds present.** Any monetization touching power or readability is a FAIL. (roadmap §13 GATE 4)

## H. Deliverables
Cosmetic + battle-pass + shop/IAP/ads + ethical chests + gem rules + retention loops, all gameplay-safe + `reports/phase-4/FINAL_REPORT.md` with the fairness-audit result.

## I. Risks
- Dark-pattern / P2W creep (mitigate: the gate + CUT list are hard). Readability break via cosmetics (mitigate: silhouette-lock + clarity mode + automated check). Pass grindiness. Mitigate via §8/§9/§10 canon.

## J. Forbidden actions
- **No loot boxes / gacha-for-power, no interstitials, no energy gates, no sellable power, no paid random boxes without disclosed odds + dupe protection** (CUT list). No cosmetic that alters silhouette/size/hitbox/timing/read. No pricing/fairness changes without an ADR. No new units/spells/maps. No clans/ranked-seasons/event-engine (Phase 7). No canon edits.

## K. Exit criteria & stop conditions
- **Exit:** all 4.x checks PASS **and** the fairness audit (GATE 4) confirms zero P2W + readability intact + disclosed odds.
- **Stop conditions:** STOP + Final Report at GATE 4; STOP and **reject** immediately if any monetization touches power or breaks readability; STOP on ambiguity with ADR.

## Escalation rules
- Any fairness/monetization question → **Live-ops/Product** within §9/§10 rules (§16); power-adjacent proposals are auto-rejected (inviolable). Readability concerns → Game Director.

## L. Mandatory final report (`reports/phase-4/FINAL_REPORT.md` + print)
```
# BULWARK — Phase 4 Final Report
## 1. Phase Summary
## 2. Work Completed     (4.1–4.5 — Done/Partial/Blocked — evidence + §ref)
## 3. Files Created
## 4. Files Modified      (no canon docs)
## 5. Validation Results  (cosmetic-safety check, entitlement server-validation, ads opt-in, chest odds disclosed + no power, gem prohibitions, FAIRNESS AUDIT result)
## 6. Known Issues
## 7. Risks
## 8. ADRs Raised
## 9. Recommendations
## 10. Gate Status        (GATE 4 fairness audit: PASS/FAIL — zero P2W & readability intact?; Authorization to proceed to Phase 5: GRANTED/WITHHELD)
```
