# WP-05 REPORT — MODE SELECT

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `ModeSelectScreen : UiScreen` (design `ModScreenDesign.png`) — 5 cards **Classic / Campaign / Tournament / Endless / Online** (frozen relabel Missions→Campaign, Multiplayer→Online) + a **Commander** button (→ CommanderSelect). Per the freeze, Classic/Tournament/Endless launch the same skirmish (mode rules deferred) via `MatchPresentation.StartMatch`; Campaign/Online surface "coming soon" (their dedicated flows are out of this sequence). Back → Pop.
- **Files changed:** `ModeSelectScreen.cs` (+`.meta`).
- **Validation performed:** structural (braces 13/13; meta; §12 clean); batch adversarial review (card→handoff traced).
- **Review findings:** loop-capture of `mode`/`playable` verified; match handoff is via the presentation seam only (no gameplay change).
- **Repairs applied:** none.
- **Remaining risks:** mode differentiation (waves/rules) is gameplay-deferred (all playable cards launch the base skirmish); **replay is gameplay-blocked** (see WP-13 — sim supports one battle/launch); per-mode card art pending.
- **Stable:** yes (pending CI/device).
