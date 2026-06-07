# WP-11 REPORT — REUSABLE COMPONENTS

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `UiModals` (designs `ConfirmModalDesign.png` 4-in-1 + `RewardGrantDesign.png` + `NetworkErrorDesign.png`) — **Confirm**, **Reward**, **Insufficient** (→ Store), **NetworkError** (Retry/Main Menu) as dim-scrim overlays that float over the current screen (built on the router canvas, not the screen stack, so the screen beneath stays visible but click-blocked). A lightweight **Toast** already lives on `UiRouter.Toast` (foundation). 
- **Files changed:** `UiModals.cs` (+`.meta`). Integrated into `BattlePassScreen` (Insufficient).
- **Validation performed:** structural (braces 12/12; meta; §12 clean); batch adversarial review.
- **Review findings:** `Scaffold(out GameObject, out Transform)` + scrim raycast-block confirmed; modals destroy their root on action.
- **Repairs applied:** none.
- **Note on order:** built alongside the screens that depend on it (Toast/Insufficient are used by earlier screens) — a dependency-driven reorder; the freeze foundation (WP-01 Navigation) groups these utilities in the shell, so this is consistent, not a skip.
- **Remaining risks:** none material (presentation-only). Reward/Confirm/NetworkError are available for wiring as their call sites are built (e.g. real IAP confirm at GATE-3).
- **Stable:** yes (pending CI/device).
