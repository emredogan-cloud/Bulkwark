# WP-04 REPORT — MAIN MENU / HUB

**Date:** 2026-06-06 · Presentation-only (§12) · Status: **AUTHORED + adversarial-reviewed (PASS-WITH-NITS); PENDING your CI/device.**

- **What was implemented:** `MainMenuScreen : UiScreen` (design `MainMenuDesign.png`) — currency chips (**Gold + Gems**, frozen model; no Energy), BULWARK wordmark, center stack (PLAY/CAMPAIGN/ONLINE BATTLE/CHESTS/STORE), right rail (QUESTS/UNITS/CLAN/LEADERBOARD/SETTINGS), bottom feature bar (DAILY/SPIN/FREE/EVENTS). Currency from `UiStub` (display-only); refreshed in `OnShow`. Built destinations route to their screens (PLAY→ModeSelect, STORE→Store, QUESTS→Quests, SETTINGS→Settings); destinations **not in this build sequence or ADR-gated/deferred** (Campaign/Online→ModeSelect; Chests/Spin→ADR notice; Units/Clan/Leaderboard/Daily/Free/Events→"coming soon") surface a toast — never dead/invented.
- **Files changed:** `MainMenuScreen.cs` (+`.meta`). Also relies on `UiStub`/`UiWidgets` (shared layer).
- **Validation performed:** structural (braces 4/4; meta; §12 clean); batch adversarial review (nav graph traced from hub).
- **Review findings:** loop-closure capture for rail/feature buttons verified correct; currency `OnShow` refresh confirmed.
- **Repairs applied:** none.
- **Remaining risks:** the legacy UiFlow menu sits beneath the shell (covered) — intentional migration state; Units/Clan/Leaderboard/Daily/Spin/Free/Events are out of this sequence (toasts); final hero/wordmark art pending.
- **Stable:** yes (pending CI/device).
