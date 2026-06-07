# WP-13 REPORT — END FLOW (Victory / Defeat)

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS, 1 MED repaired); PENDING your CI/device.**

- **What was implemented:** `EndScreen : UiScreen` (designs `VictoryScreenDesign.png` + `DefeatScreenDesign.png`) — Victory (title + "enemy statue has fallen!" + **display-only** gem reward + match-time + reward-chest placeholder + CONTINUE) / Defeat (title + "your statue has fallen." + RETRY + CONTINUE). The outcome is the **real ECS `MatchState.Outcome`** (read read-only by UiFlow, which delegates to `MatchPresentation.OnMatchDecided` and sets `PendingVictory`) — or a presentation-only Surrender (Defeat). End stinger fires once in `OnShow`. CONTINUE → Main Menu; RETRY → see blocker.
- **Files changed:** `EndScreen.cs` (+`.meta`). (End delegation seam lives in `UiFlow.cs`, see WP-12.)
- **Validation performed:** structural (braces 5/5; meta; §12 clean — outcome consumed read-only; no sim write); batch adversarial review.
- **Review findings (MED):** **RETRY bounced straight back to the end screen** — the ECS `MatchState` persists the outcome, so re-entering a finished battle immediately re-resolves.
- **Repairs applied:** `MatchPresentation.OnRetry` now degrades to a notice + return-to-menu instead of re-entering the finished battle.
- **⚠️ KNOWN BLOCKER (GAMEPLAY, not UI) — documented, not implemented (per binding rules):** a true **rematch/replay** requires the ECS battle **world to reset** (`MatchState`→Ongoing, units/statues respawned). **The sim supports one battle per launch and exposes no reset.** This also means repeated PLAY after a finished match would re-resolve. Resolving it is a gameplay/sim feature (a world-reset / new-match API) that **must not** be added in this UI program. RETRY and replay therefore degrade gracefully until that gameplay feature exists. **Surfaced for the gameplay owners.**
- **Remaining risks:** the victory reward/time are display-only placeholders (reward economy + match-timer surfacing pending); replay blocked as above.
- **Stable:** yes for a single match end (pending CI/device); replay is gameplay-blocked.
