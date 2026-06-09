# VISUAL CERTIFICATION — per‑screen outcomes
**Phase 8 fidelity certification.** Benchmark = **commercial premium mobile RTS** (not "minimum acceptable"). Scores in `FIDELITY_MATRIX.csv`. Evidence in `runtime/device_validation/rc_p3..p8_linux/`. **No screen reaches the ≥95 CERTIFIED bar** — the honest reasons are below.

**Legend:** `CERT‑TDP` = CERTIFIED — TYPOGRAPHY + DEVICE VALIDATION PENDING (art adequate for the screen's purpose; gated only on SDF typography + on‑device sign‑off). `ART‑BLOCKED` = VISUAL QUALITY BLOCKED BY ART PASS (placeholder art caps fidelity; not code‑solvable). `FAIL` = unresolved code‑solvable S1.

---

## Outcome summary (34 screens)
- **CERTIFIED (≥95, clean):** **0** — honest: nothing is at commercial quality yet because the art is placeholder and typography isn't SDF.
- **CERT‑TDP:** 11 — the clean **data/utility/modal** screens (Leaderboard, Settings, Display, About, Tournament, Daily, Endless, Reward, FreeRewards, Confirm, NetworkError). Their backdrops are simple by purpose, so the procedural plate is acceptable; they're gated only on SDF typography + device.
- **ART‑BLOCKED:** 22 — every **hero/character/product** screen (Splash, MainMenu, ModeSelect, CampaignMap, Battlefield, Victory/Defeat, Store + tabs, Profile, Clan, Units, Commander, Events, BattlePass, Online, LuckySpin, Login). They *need* painted plates / character art / product renders; the procedural placeholders cap them at 70–83.
- **FAIL (code‑solvable S1):** 1 — **Quests** (row‑text clipping). Fix specified below; honestly left unfixed rather than claimed.

## Key per‑screen notes (defects beyond the universal art/typography gap)

| Screen | Status | Score | Notable defect(s) |
|---|---|---|---|
| Splash | ART‑BLOCKED | 79 | hero (king) is a PIL stick + async‑load‑timing weak on the brief screen; logo is live Text not authored mark (A2/A8) |
| Loading | ART‑BLOCKED | 79 | army strip is PIL silhouettes (A2) |
| MainMenu | ART‑BLOCKED | 83 | strongest hero screen, but trio + kingdom are placeholder (A1/A2) |
| ModeSelect | ART‑BLOCKED | 82 | card art is procedural gradients (A1) |
| CampaignMap | ART‑BLOCKED | 77 | map is procedural terrain washes, not painted (A1) |
| **Battlefield/HUD** | ART‑BLOCKED | 74 | units are PIL bone‑rig; biome layers flat (A2/A3) — **the biggest art gap** |
| Victory/Defeat | ART‑BLOCKED | 83 | kneel/cheer stick + procedural plate (A1/A2) |
| Store | ART‑BLOCKED | 79 | needs product/BP renders; cards clean but art‑thin (A1/A6/A8) |
| Skins | ART‑BLOCKED (S1) | 70 | **S1: skin preview is an empty green box — no character rendered** (needs the rigged character in the preview + skin art — A2) |
| Leaderboard | CERT‑TDP | 88 | clean, information‑dominant; only SDF typography + device pending |
| Profile | ART‑BLOCKED | 79 | needs a real hero portrait (A2); otherwise clean |
| Clan | ART‑BLOCKED | 77 | dragon‑crest→stick‑crest + member avatars are art (A2/A6); chat/members are stub seams (correct per §11) |
| **Quests** | **FAIL** | 72 | **S1 (code‑solvable): row description text is clipped/overlapped by the icon disc + panel edge; "DAILY QUESTS" title sits at the bottom (S2).** Fix: inset the row‑text RectTransform to start right of the icon and clamp within the panel; move the title to the header band. *Honestly left unfixed pending a build cycle — not claimed.* |
| Daily/Tournament/Endless | CERT‑TDP | 82–83 | clean; gated on typography + device |
| Settings/Display/About | CERT‑TDP | 85–87 | cleanest utility screens; single titles, organized panels; only SDF typography + device pending |
| Reward/Confirm/NetworkError | CERT‑TDP | 84 | clean modals; gated on typography + device |

## Universal gaps (apply to every screen)
- **Typography (A7):** styled legacy `Text`, not TMP SDF → **TYPOGRAPHY CERTIFICATION PENDING** everywhere (Editor‑gated; −4–6 pts each).
- **Device (§12 device):** MIUI "Install via USB" re‑lock blocks on‑device capture → **DEVICE VALIDATION PENDING** everywhere (validated on Linux‑standalone Unity runtime instead).
- **Art (A1–A8):** procedural/PIL placeholders, not authored premium art → caps fidelity; the dominant reason for ART‑BLOCKED.

## What WOULD lift these to CERTIFIED
Per `ART_BLOCKERS.md`: (1) SDF typography editor pass → +4–6 every screen → most CERT‑TDP screens approach ≥90; (2) licence‑clean kit + logo; (3) painted plates → hero screens +10–15; (4) character art → battlefield/hero screens +10–15. Code polish alone yields <2 pts/screen (plateau) — except the one Quests S1.
