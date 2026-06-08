# 00 — CONTEXT RECOVERY
**Project:** Stick Empire Rise (working repo: `Bulkwark` / internal codename **BULWARK**)
**Assignment:** UI/UX Consolidation & Visual Reconstruction (analysis & planning only — no code/asset/commit changes)
**Date:** 2026-06-08

---

## 1. What this project is

A **Unity 6000.0.75f1** DOTS/ECS **mobile RTS‑lite** in the *Stick War: Legacy* lineage: two factions — **Iron Pact (blue)** vs **Ashen Horde (red)** — fight a lane/siege battle whose win condition is **destroy the enemy statue**. The shipping front‑end is a **code‑built uGUI** layer (no prefabs, no UXML) running on a screen‑stack shell (`UiRouter` / `UiScreen`), **landscape only**, `CanvasScaler` 2340×1080 match‑height, with a `SafeAreaFitter`. 38 screens are implemented.

A hard architectural rule governs the whole UI: **§12 control boundary** — the UI reads ECS state read‑only and may only ever write three things (`Training.EnqueueTrain`, `MoveDestination`, `Time.timeScale`). No gameplay/ECS/economy/AI/balance logic lives in the UI layer. This assignment **inherits and preserves §12 absolutely**.

## 2. Current visual architecture (as built)

```
UiRouter (screen stack, CanvasScaler 2340×1080, SafeAreaFitter)
  └─ UiScreen (per screen)
        Backdrop (real /design art, runtime-loaded)  ← UiAssets
        Scrim (0.45 black)                            ← suppresses baked logos
        Atmosphere (vignette / glow / gradients)      ← UiTex procedural
        Chrome (ornate panel, gem buttons, cards)     ← UiAssets kit + UiTex fallback
        Content (labels, lists, bars, icons)          ← live data via UiStub/ECS read-only
```

- **UiTex** — procedural `Texture2D`/sprite generator (gradients, vignette, 9‑slice frame, diamond, disc). Original placeholder look.
- **UiTheme / UiWidgets / UiFx** — palette, reusable builders (`OrnateFrame`, `GemButton`, `Card`, `CurrencyChip`, `TabBar`, `Backdrop`…), and unscaled‑time effects.
- **UiAssets** (recent) — runtime loader (UnityWebRequest → Sprite, device‑safe, bypasses the editor import pipeline) for **real sprites sliced from `/design`** into `Assets/StreamingAssets/bulwark_ui/`: 38 per‑screen backdrops, an ornate gold 9‑slice panel, six colour gem buttons, a gem finial, coin/gem icons. `UiWidgets` prefers these and falls back to `UiTex`.
- **PlaceholderAssets** — the older runtime loader for `Assets/StreamingAssets/bulwark/*.png` (16 generic placeholder sprites).

## 3. The `/design` corpus (the "source of truth" art)

38 PNGs in `/design/` (~1536–1824 px, RGBA), one per screen, produced as **high‑fidelity mockups**. Forensic inspection of all 38 (this assignment, Phase 2) establishes the central problem:

- **The mockups are rendered in a realistic, cinematic AAA dark‑fantasy style** (Raid/Diablo/Clash‑adjacent) — **not** the stick‑figure identity the product is supposed to have.
- **Stick figures appear in only ~3–4 screens** (MainMenu hero trio, ModeSelect cards, Skins hero/skins, a borderline Spells mage). Every other screen depicts **realistic armored humans, orcs/ogres, dragons, undead, and a lion** where characters appear.
- The mockup logo literally reads **"STICK EMPIRE / RISE"** (MainMenu), which is the origin of the new official name.

`/design` is **git‑ignored** (92 MB) and flagged in `docs/adr/ADR-0-001-environment-and-ip-blockers.md` and `.gitignore` as **ripped/placeholder IP** to be replaced before release.

## 4. What was done most recently (UI reconstruction, now paused for this assignment)

1. Built all 38 screens as code‑built uGUI per the **UI Construction Bible**.
2. Extracted real sprites from `/design` → re‑skinned the shared chrome + per‑screen backdrops (commits `df0bcab`, `c0588a9`, `11f0a7b`).
3. **Device‑validated on a physical Redmi Note 11R** (Android 14, 3.66 GB RAM). Real art renders on device (`[UIART] 48/48 sprites loaded`, cold boot ~0.8 s).
4. **Resolved an install regression** (see below) and ran a first on‑device fidelity pass that exposed the "broken‑UI‑over‑art" defects this assignment must root‑cause (Phase 4).

## 5. Known problems / prior failures (evidence‑based)

| # | Problem | Evidence |
|---|---------|----------|
| P‑A | **Visual identity mismatch** — realistic art vs intended stick figures | Phase 2 catalog: ~34/38 mockups are realistic/creature art |
| P‑B | **Monster contamination** — dragons, orcs, undead, a lion in mockups | Splash/Loading/Login/MainMenu dragons; MatchIntro/Commander/Banner orcs; ModeSelect/Banner/Events/Tournament undead; LuckySpin lion; ClanScreen dragon crest |
| P‑C | **"Broken UI over art"** — baked mockup UI/logos bleed behind live UI; duplicated headers; stretched baked text garble | On‑device 0.0.92: MainMenu "STICK EMPIRE" behind live "BULWARK"; Leaderboard shows baked RESUME/SETTINGS/SURRENDER; gem buttons garbled |
| P‑D | **Placeholder contamination** — ripped art used as the literal asset source | `/design` + `StreamingAssets/bulwark/` are ripped placeholders per ADR‑0‑001 |
| P‑E | **Brand drift** — codebase says "Bulwark", product is now "Stick Empire Rise" | 293 tracked files contain "bulwark"; `productName: bulwark-clean`; repo typo'd `Bulkwark` |
| P‑F | **Battlefield/character art is primitive** — gameplay uses ECS primitives, no real characters | No authored unit/character art exists in‑repo; mockups' realistic units are unusable (off‑identity + IP) |
| P‑G | **Low‑RAM device instability** — OOM kills on 3.66 GB device | logcat `lowmemorykiller` across the validation run |

### Resolved this session (documented for completeness)
- **Install regression** `INSTALL_FAILED_USER_RESTRICTED`: root cause was `verifier_verify_adb_installs=1` (Play Protect verification of adb installs), **not** a permanent MIUI block. Repaired (reversible) via `settings put global verifier_verify_adb_installs 0`; two clean installs since.

## 6. Documents recovered

- `reports/ui-construction-bible/*` (00 context recovery, 01 design inventory, 02–39 per‑screen forensic specs, **99 UI Construction Bible**).
- `FINAL_IMPLEMENTATION_REPORT.md`, `FINAL_DEVICE_VALIDATION_REPORT.md`.
- `reports/asset-migration/*`, `reports/ui-production/*`, `reports/production-presentation/*`, `reports/ui-blueprint/*`, `reports/mode-blueprint/*`.
- `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`, `report/NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`.
- `docs/adr/ADR-0-001-environment-and-ip-blockers.md` (IP/placeholder policy).
- `future/000-assets-roadmap/BULWARK_ASSET_MASTER_AUDIT_TR.md` (asset strategy, Turkish).

## 7. Objectives of THIS assignment

A **design/UI‑UX/visual‑identity consolidation** — analysis, forensic review, planning, and production preparation **only**. Concretely, to deliver under `reports/uiux-consolidation/`:

1. **00** context recovery (this file).
2. **01** full rebranding audit to *Stick Empire Rise* + zero‑downtime migration roadmap.
3. **02** monster/non‑stick‑creature detection across `/design`.
4. **03** GPT‑Image/DALL·E correction prompts (replace creature layer, keep composition) — targeting **original** stick art.
5. **04** root‑cause analysis of the "broken UI over art" failure.
6. **05** the definitive UI‑integration architecture so the fidelity crisis can't recur.
7. **06 / 07** battlefield visual strategy + environment asset strategy.
8. **08** character production strategy (the stick army).
9. **09** execution priority matrix (P0–P3).
10. **99** self‑contained master report embedding all of the above verbatim.

**Non‑negotiable constraints:** no gameplay/ECS/AI/economy/balance/backend/save/matchmaking changes; no code, asset, scene, prefab, or Unity edits; no commits/pushes; evidence over assertion; recover context before analysis.
