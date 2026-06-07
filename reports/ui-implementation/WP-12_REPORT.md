# WP-12 REPORT — MATCH LAYER

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:**
  - `MatchIntroScreen` (design `MatchIntroDesign.png`) — VS framing (mode + Iron Pact vs Ashen + tip); auto-advance/tap → `MatchPresentation.Begin`.
  - `PauseModal` (design `PauseModalDesign.png`) — dim scrim over the frozen battlefield + RESUME/SETTINGS/SURRENDER; pausing toggles `Time.timeScale` (a permitted §12 control the legacy debug HUD already used — no rule change).
  - `MatchPresentation` (bridge) — sequences the shell around the **existing** battle: clears the shell so the battlefield + **existing BattleHud** show, calls `UiFlow.BeginMatchFromShell`, leaves a **safe-area in-match Pause overlay** (so BattleHud is left 100% untouched), and routes the end via the UiFlow delegation.
  - **Landscape Battle HUD:** the existing `BattleHud` already renders landscape via the shared `UiScaling` (WP-00) and is **unedited** (bindings untouched, per the objective); the Pause entry is added as a shell overlay rather than by modifying the HUD.
- **Files changed:** `MatchIntroScreen.cs`, `PauseModal.cs`, `MatchPresentation.cs` (+metas); `UiFlow.cs` (presentation-flow seams only: `Instance`, `BeginMatchFromShell`, `ReturnToMenuFromShell`, end-delegation when `RouterOwnsEntry`). **`BattleHud.cs` NOT edited** (no gameplay logic touched).
- **Validation performed:** structural (braces balanced; meta; §12 clean — only `Time.timeScale`/`PresentationState.InMatch` touched; outcome read by UiFlow read-only); batch adversarial review (full match-flow trace, timeScale coherence, pause overlay lifecycle).
- **Review findings:** defensive — `OnMatchDecided` did not `Clear()` the stack before showing the end screen.
- **Repairs applied:** added `UiRouter.Instance.Clear()` at the top of `OnMatchDecided` (mirrors `Surrender`).
- **Remaining risks:** the in-match Pause is a shell overlay (intentional, to avoid editing BattleHud); deeper landscape HUD re-layout (HP bars to corners) is optional polish — the HUD is functional landscape via the scaler. Match start/end coordinates with the legacy UiFlow orchestrator (kept as the proven sim-control owner).
- **Stable:** yes (pending CI/device).
