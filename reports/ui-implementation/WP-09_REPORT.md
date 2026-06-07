# WP-09 REPORT — COMMANDER SELECT

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `CommanderSelectScreen : UiScreen` (design `CommanderSelectDesign.png`) — Iron Pact **WARDEN** vs Ashen **WARCHIEF**, each with active/passive ability cards, title, commander level, and **SELECT** (records the display-only choice + Pop). Mirrors canon `CommanderDef`×2 via `UiStub`. Reached from Mode Select (pre-battle).
- **Files changed:** `CommanderSelectScreen.cs` (+`.meta`); `UiStub.cs` (added `SelectedCommander`).
- **Validation performed:** structural (braces 5/5; meta; §12 clean); batch adversarial review.
- **Review findings:** none specific; `BuildCommander` closure capture (`n`) correct.
- **Repairs applied:** none.
- **Remaining risks:** real `CommanderDef`/`ProgressionService` binding (levels, ≤15% power budget) = **GATE-3**; portrait art pending.
- **Stable:** yes (pending CI/device).
