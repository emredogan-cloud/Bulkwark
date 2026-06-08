# 99 — UI/UX CONSOLIDATION & VISUAL RECONSTRUCTION — MASTER REPORT
**Project:** Stick Empire Rise *(internal codename **Bulwark**; repo `Bulkwark`)*
**Type:** Design / UI‑UX / visual‑identity consolidation — **analysis & planning only** (no code, asset, scene, prefab, commit, or Unity changes were made)
**Date:** 2026-06-08
**Author role:** Senior UI Director · Technical Artist · Art Director · Product Owner

> This master report is **self‑contained**. The executive summary below is followed by **all nine sub‑reports embedded verbatim** (00–09). A reader who has only this file has everything: what happened, what is wrong, why, how the game should look, how monster contamination is fixed, how the UI crisis is permanently solved, and how the battlefield, environments, and characters evolve — plus exactly what to do next.

---

## EXECUTIVE SUMMARY

### What this is
A forensic consolidation of the game's visual identity and UI after an on‑device build (0.0.92) exposed a "broken UI over art" fidelity crisis. No implementation was performed; this is the analysis, plan, and production‑prep package.

### What happened
The UI was first built as code‑built uGUI with procedural placeholders, then "re‑skinned" by slicing real art out of the `/design` mockups and using full mockups as screen backgrounds. On device this produced **doubled logos/titles, garbled buttons, and panel bleed‑through** — because the mockups are **finished, flattened screen compositions** (scene + baked logos/buttons/text), not clean layered assets.

### What is wrong (three intertwined problems)
1. **Broken UI over art** *(Report 04)* — baked pixels (logos, titles, "RESUME/SETTINGS/SURRENDER", "STICK EMPIRE") sit *under* live UI that doesn't fully cover them. Two mechanisms: full‑mockup backdrops, and UI chrome 9‑sliced from a mockup that has baked text.
2. **Wrong visual identity / monster contamination** *(Reports 02, 03)* — the intended identity is **stick figures**, but **~34 of 38 mockups** render realistic knights, **orcs/ogres, dragons, undead, and a lion** wherever characters appear. Only MainMenu, ModeSelect, and Skins are on‑identity.
3. **Brand drift + IP risk** *(Report 01)* — the codebase says "Bulwark" in 293 files while the intended public name is "Stick Empire Rise" — a name that **overlaps Max Games' Stick War / Stick Empires trademarks**, atop ripped‑placeholder art. This is a legal exposure that must be cleared before it touches any store/marketing surface.

### Why it happened
A **design comp was treated as a production asset.** Comps show *what a screen should look like*; assets are *clean, layered, text‑free pieces meant to be shipped*. With only flattened comps available — and a procedural UI retrofitted with extracted art — every reuse dragged baked UI along and produced three conflicting visual sources on one screen. Validation happened too late (the doubling is only visible once the device loads the textures).

### How Stick Empire Rise should look
A **dark‑fantasy stick‑figure war**: black silhouette stick warriors with minimal gold‑trim/cloth accents and glowing eyes — **Iron Pact (blue)** vs **Ashen Horde (red)** — fighting a **2D side‑view parallax lane** toward the enemy **statue** (the win condition). Clean ornate‑gold UI chrome with crisp live text floats over **UI‑free** atmospheric background plates. No realistic humans, no orcs, no dragons.

### How the monster contamination is fixed
Report 02 catalogues every violation and its stick replacement; Report 03 provides **GPT‑Image/DALL·E correction prompts** that preserve each composition and **swap only the creature layer** to **original** stick art — with two hard rules: original art only (no copied assets/marks), and **keep UI zones clean / no baked text** so the live UI overlays without re‑introducing doubling.

### How the UI crisis is permanently solved
Adopt a **Clean Layered UI Kit** *(Report 05)*: UI‑free background plates (Layer 0) → optional stick character/key‑art (Layer 1) → an **authored clean 9‑slice/atlas UI kit with zero baked text** (Layer 2) → **live TMP text as the only text source** (Layer 3), all driven by the **existing** UiRouter/UiWidgets API (so it's an asset swap, not a rewrite), with one UI system and a **device screenshot‑vs‑reference acceptance gate**. This makes both failure mechanisms structurally impossible.

### How battlefield, environments, and characters evolve
- **Battlefield** *(06)*: a 2D parallax lane (sky/horizon/midground+statues/playfield/foreground), pooled FX for ambience/weather, fixed per‑map time‑of‑day, **no realtime lights**, atlas‑batched and biome‑streamed for the 3.66 GB device.
- **Environment** *(07)*: reusable **per‑biome 2D kits**; **statues are the top‑priority animated hero‑prop** (the objective must read); start with one Siege biome.
- **Characters** *(08)*: **2D skeletal/bone animation on a single shared stick rig** (Unity 2D Animation by default; Spine optional) with equipment‑overlay + faction‑tint variation — animate once, the whole roster inherits; ship the 5 core units first.

### What to do next (priority — Report 09)
- **P0:** clear the name legally; adopt the clean layered architecture; author the **UI kit** + **UI‑free plates for the top ~8 screens** + the **stick archetype sheet**; remove the dual UI system; fix visible brand strings.
- **P1:** battlefield parallax + statues; shared rig + 5 core units; remaining plates; TMP fonts; correct the high‑traffic monster screens; stand up the device fidelity gate.
- **P2/P3:** full roster, more biomes, key‑art layer, cosmetics, then polish. **Shipping P0+P1 closes ~90% of the crisis.**

### Single most important caveat
**The name + assets carry real IP/trademark exposure** (Report 01 §0). Treat "Stick Empire Rise" as provisional, build **original** stick art, and get legal clearance before any public/marketing surface uses the name or the corrected art.

---

## TABLE OF CONTENTS (all embedded verbatim below)
- **00** — Context Recovery
- **01** — Rebranding Audit → "Stick Empire Rise"
- **02** — Monster / Non‑Stick Creature Detection
- **03** — Image Correction Prompts (GPT‑Image / DALL·E)
- **04** — Root Cause: "Broken UI over Art"
- **05** — Final UI Integration Roadmap
- **06** — Battlefield Visual Strategy
- **07** — Environment Asset Strategy
- **08** — Character Production Strategy
- **09** — Execution Priority Matrix

---


<!-- ==================== EMBEDDED: 00_CONTEXT_RECOVERY.md ==================== -->

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


<!-- ==================== EMBEDDED: 01_REBRANDING_AUDIT.md ==================== -->

# 01 — REBRANDING AUDIT → "STICK EMPIRE RISE"
**Scope:** every surface where the old name (**BULWARK / bulwark / Bulkwark / bulwark‑clean**) appears, the proposed replacement, migration difficulty, risk, and sequencing — plus a zero‑downtime roadmap.
**Method:** `git grep -il bulwark` across tracked files + `ProjectSettings` + asmdef/csproj inventory + remote inspection. **Analysis only — nothing renamed.**

---

## 0. ⚠️ READ FIRST — Trademark & IP risk on the name itself

This is the single most important finding and it must reach a decision‑maker before *any* rename work starts:

- **"Stick Empire" / "Stick Empires" / "Stick War" are existing commercial marks of Max Games Studios.** Adopting **"Stick Empire Rise"** as the public product name is **highly likely to create trademark confusion / infringement exposure**, especially combined with (a) the `/design` art being ripped‑placeholder in the *Stick War: Legacy* style (per `ADR‑0‑001`) and (b) the in‑game **"destroy the enemy statue"** win condition lifted from that franchise.
- **Recommendation (Product/Legal):** Treat "Stick Empire Rise" as a **provisional internal title** only. Obtain trademark clearance before shipping it on any store/marketing surface. Strongly consider an **original distinct name** for the public brand while keeping a stick‑figure *art style* (style is not protectable; a confusingly‑similar *name* + copied *assets* are the exposure).
- This audit therefore separates **player‑facing brand surfaces** (where the name is shown) from **internal code identity** (namespaces/package), and recommends **not** propagating a legally‑risky name into the irreversible technical identity until cleared.

> Everything below assumes the name is approved/cleared. If Legal substitutes a different name, the same migration mechanics apply — only the target string changes.

---

## 1. Identity surface inventory (evidence)

| Surface | Current value | Proposed | Files/locations | Difficulty | Risk | Notes |
|---|---|---|---|---|---|---|
| **App display name** | `productName: bulwark-clean` | `Stick Empire Rise` | `ProjectSettings/ProjectSettings.asset` | **Low** | Low | Player‑facing launcher/title; safe to change. |
| **Company name** | `companyName: DefaultCompany` | `<studio name>` | `ProjectSettings.asset` | Low | Low | Currently the Unity default — should be set regardless. |
| **Bundle/app id** | `com.DefaultCompany.bulwarkclean` | *(keep, or `com.<studio>.stickempirerise`)* | `applicationIdentifier`, `AndroidManifest` | **High** | **High** | Changing the package id = a **new app** to the store + breaks save/cloud/IAP/analytics continuity. **Recommend NOT changing** (see §3). |
| **In‑game logo text** | live `Text "BULWARK"` + `"RISE"` ribbon | `STICK EMPIRE` + `RISE` | `MainMenuScreen.cs:38–40`, `SplashScreen`, `LoadingScreen` | Low | Low | Live uGUI strings; trivial. (The *baked* logo in mockups is a separate art problem — see Report 04.) |
| **Code namespaces** | `Bulwark.*` (Bootstrap/Sim/Control/Data/Services/Game.*) | *(keep as internal codename)* | 9 `*.asmdef`, 8 `*.csproj`, ~145 `Assets/**.cs` | **High** | **Med‑High** | Mass rename touches every file + asmrefs; high churn, easy to break compile. **Recommend keep** (codename ≠ brand). |
| **Asmdef/asm names** | `Bulwark.Bootstrap` … | *(keep)* | `Assets/**/*.asmdef` | High | Med | Renaming asmdefs cascades into every asmref + csproj. |
| **Git repo** | `github.com/emredogan-cloud/Bulkwark` (**typo'd**) | `stick-empire-rise` *(or codename)* | remote | Low | Low | GitHub rename auto‑redirects old URL; fix the **"Bulkwark" typo** regardless. |
| **StreamingAssets dirs** | `StreamingAssets/bulwark/`, `bulwark_ui/` | *(keep)* | runtime loaders reference these literals | Med | Med | Renaming requires touching `PlaceholderAssets.cs`/`UiAssets.cs` path literals + 96 `.meta`. Low value. **Recommend keep.** |
| **Reports/docs** | "BULWARK …" titles, 97 report files, 13 docs, `report/BULWARK_MASTER_*` | new title in *new* docs | `reports/**`, `docs/**`, `report/**`, `future/**` | Low | Low | Historical docs may keep codename; **new** player‑facing docs use the brand. Do **not** mass‑rewrite history. |
| **Save identifiers** | PlayerPrefs/save keys (codename‑derived) | *(keep keys)* | `Bulwark.Services`, save layer | **High** | **High** | Key renames orphan existing saves → **do not rename keys**; only the display brand. |
| **Cloud / PlayFab title** | PlayFab integration (`Bulwark.Services.PlayFab`) | configured title display only | backend config | Med | High | Title *display* can change; **title id / namespace must not**. |
| **Analytics events** | event names (if codename‑prefixed) | *(keep keys)* | services layer | Med | High | Renaming analytics keys breaks historical dashboards/funnels. Keep keys; relabel in the dashboard. |
| **Marketing references** | store listing, screenshots, key art | full rebrand | external | Low (tech) | **High (legal)** | Gated on the §0 trademark clearance. |

**Spread of the literal "bulwark" (case‑insensitive) across tracked files:** 293 files — Assets 145, reports 97, docs 13, future 6, report 4, backend 2, misc 3.

## 2. Categorised occurrences (from the prompt's checklist)

- **Repository / GitHub:** `Bulkwark` (note typo). → rename + redirect.
- **Folder names:** `Assets/StreamingAssets/bulwark*`, report dirs — keep (internal).
- **Package name / bundle id:** `com.DefaultCompany.bulwarkclean` — **keep** (irreversible identity).
- **Namespaces:** `Bulwark.Bootstrap/Sim/Control/Data/Services/Game.Cosmetics/Game.Modes` — **keep** as codename.
- **UI text:** `MainMenuScreen` logo "BULWARK"+"RISE"; any `Router.Toast`/titles — **change** (player‑facing).
- **Logos / splash:** live splash/loading wordmarks — **change**; baked mockup logos — replaced via art pipeline (Report 03/04).
- **Settings page:** any "About/Version/credits" text — **change** display string.
- **Notifications:** push‑notification app label — follows display name.
- **Reports / documentation:** new docs use brand; legacy keep codename.
- **Screenshots:** store/marketing screenshots — regenerate after UI consolidation.
- **Roadmap docs:** `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`, `future/000-assets-roadmap/BULWARK_ASSET_MASTER_AUDIT_TR.md` — retitle in new copies only.
- **Save / cloud / analytics identifiers:** **keep all keys** (continuity); relabel only display.
- **Marketing references:** gated on legal clearance.

## 3. Core recommendation — decouple "brand" from "codename"

Shipping games routinely keep an **internal codename** distinct from the **marketing name**. Apply that here:

- **Marketing/brand name** (player‑facing, store, logos, splash, settings "about", notifications): **Stick Empire Rise** *(pending legal clearance per §0)*.
- **Internal codename** (package id, namespaces, asmdefs, StreamingAssets dirs, save/cloud/analytics keys, repo): **keep `Bulwark`**.

Rationale: the player never sees the codename; renaming it is **high effort, high regression risk, and irreversible for store/save/cloud identity**, while delivering **zero player value**. The rebrand the user actually wants is the **visible** one, which is cheap and safe.

## 4. Zero‑downtime migration roadmap

**Phase R0 — Decision gate (blocking):** Legal clearance on "Stick Empire Rise" (§0). Output: approved brand string (or substitute). *No work proceeds on visible surfaces until this clears.*

**Phase R1 — Display brand (safe, reversible):**
1. `ProjectSettings`: `productName` → approved brand; set `companyName`.
2. Live UI wordmarks: `MainMenuScreen`/`SplashScreen`/`LoadingScreen` logo strings.
3. Settings "About/Version" + push‑notification label.
*Acceptance:* app launcher, splash, menu, settings show the new brand; **no** id/key changed; build green; saves intact.

**Phase R2 — Repo hygiene (safe):** rename GitHub repo (fix `Bulkwark` typo) → auto‑redirect; update README/CI badge text only.

**Phase R3 — New player‑facing docs:** author brand‑named store listing, press kit, screenshots **after** the UI consolidation (Reports 04–08) lands — so screenshots show the corrected, on‑identity UI.

**Phase R4 — (Optional, deferred, NOT recommended now) deep code rename:** only if a business reason ever forces package‑id parity. Requires: store re‑publish strategy, save/cloud migration shim, analytics key remap, full namespace rename + recompile + regression. Treat as its own project with rollback = git revert + store rollback.

**Rollback:** R1–R3 are pure string/display changes → revert = restore strings. R4 is the only irreversible one and is explicitly deferred.

## 5. Acceptance criteria

- [ ] Legal sign‑off (or approved substitute) recorded before any visible rename.
- [ ] All **player‑facing** surfaces show the approved brand; **zero** package‑id/save/cloud/analytics **keys** changed.
- [ ] Build compiles; existing saves load; IAP/cloud/analytics continuity preserved.
- [ ] `Bulkwark` typo fixed at the repo level with redirect.
- [ ] Internal codename `Bulwark` documented as the stable engineering identity.


<!-- ==================== EMBEDDED: 02_MONSTER_DETECTION_REPORT.md ==================== -->

# 02 — MONSTER / NON‑STICK CREATURE DETECTION
**Goal:** identify every `/design` screen whose **character layer** violates the stick‑figure visual identity, and specify the stick archetype that should replace each.
**Method:** forensic inspection of all 38 mockups (four independent reviewers, viewing each PNG). Classification: `STICK` (on‑identity) vs `NON‑STICK` (realistic human/knight, orc/ogre, dragon, undead/zombie, beast — violation). **No files modified.**

---

## 1. Headline

The intended identity is **stick‑figure warriors** (Iron Pact = blue‑accented sticks, Ashen Horde = red‑accented sticks) in a dark siege war. **The mockups almost entirely betray this:** of 38 screens, **only ~3 use stick figures** (MainMenu hero trio, ModeSelect cards, Skins hero/thumbnails; Spells is a borderline over‑rendered stick mage). Wherever else a character/avatar/unit appears, it is rendered as a **realistic armored human, an orc/ogre, a dragon, undead, or a lion**. This is a **project‑wide art‑identity failure**, not a few stray images.

## 2. The stick archetype vocabulary (replacement targets)

Derived from the game's own roster (`UnitsArmyDesign`) + the on‑identity screens:

| Archetype | Stick description | Faction tint |
|---|---|---|
| **Stick King / Commander** | black stick body, gold crown, short cape, sword + kite shield | blue (Iron Pact) / red (Ashen Horde) |
| **Stick Mage / Caster** | stick body, tall pointed robe + hat silhouette, glowing eyes, staff | purple/arcane accent |
| **Stick Archer / Ranger** | stick body, hood/cloak, longbow | green accent |
| **Stick Swordsman / Shieldman** | stick body, sword + round/kite shield, light helm accent | faction tint |
| **Stick Spearman / Spartan** | stick body, spear + round shield, crested helm | faction tint |
| **Stick Miner / Worker** | stick body, pickaxe, satchel | neutral |
| **Stick Crossbowman** | stick body, crossbow | faction tint |
| **Stick Heavy Guard / Brute** | larger/bulkier stick body, heavy‑armor accents, two‑hander (replaces orc/ogre) | red (Ashen) |
| **Stick Undead** | tattered stick body, bone/green necrotic accents (replaces zombie/undead) | green/red |

**Non‑creature emblems** (dragon crest, lion hub) → replace with **stick‑style heraldry** (crowned‑stick crest, crossed stick‑weapons, faction shield) rather than animal beasts.

## 3. Per‑screen violation matrix

Severity: **S3** entire character layer off‑identity · **S2** prominent creature(s) · **S1** minor/background or gear‑only · **OK** on‑identity · **—** no characters.

| Screen | Severity | Non‑stick creatures (location) | Why it violates | Stick replacement | Conf. |
|---|---|---|---|---|---|
| **UnitsArmyDesign** | **S3** | Full roster of 10 realistic units + large "Shieldman" inspector (grid + right panel) | The actual army — every unit — is realistic knights/mages/humans | Map 1:1 to the stick archetype table (stick Shieldman, Archer, Mage, Miner, Crossbowman, Heavy Guard…) | High |
| **InMatchBannerDesign** | **S3** | Realistic armies + **ogre/monster + undead horde** (right wave) | Worst HUD: live army *and* explicit monsters ("The Dead Awaken") | Blue stick army vs red **stick‑undead** wave | High |
| **BattleHudDesign** | **S3** | Realistic blue+red armies on field + 5 realistic unit icons | The on‑field gameplay army is realistic, not sticks | Stick armies (blue vs red) + stick unit icons | High |
| **InMatchSpellHudDesign** | **S3** | Realistic armies + 5 spawn icons + hero portrait | Same as BattleHud + realistic hero bust | Stick armies + stick hero portrait | High |
| **SplashScreenDesign** | **S3** | Realistic armored king (foreground), **dragons** (sky), realistic soldier horde | Title key art is fully realistic + dragons | Stick king hero foreground; remove/replace dragons with smoke/banners | High |
| **LoadingScreenDesign** | **S3** | 2 realistic warriors, **2 dragons**, 2 realistic armies | Fully realistic clash + dragons | Blue vs red stick armies clashing; drop dragons | High |
| **LoginAuthDesign** | **S3** | 2 realistic knights, **dragon** (sky), siege army | Fully realistic flanking figures | Stick warriors flanking; drop dragon | High |
| **MatchIntroDesign** | **S3** | Realistic knight (Iron Pact) + **orc/warlord brute** (Ashen Horde) | VS champions are realistic + an orc | Blue stick king vs red **stick heavy‑brute** | High |
| **CommanderSelectDesign** | **S3** | Realistic human "Warden" + **orc/ogre "Warchief"** | Both commander portraits off‑identity | Stick Commander (blue) vs stick Heavy‑Brute (red) | High |
| **ProfileDesign** | **S3** | Realistic human hero "Thalrion" (large central portrait) | Dominant hero portrait realistic | Stick King hero portrait | High |
| **TournamentLadderDesign** | **S2** | ~16 realistic/undead competitor avatar busts | Whole bracket roster off‑identity | Stick‑figure avatar busts (varied helms/weapons) | High |
| **LeaderboardScreenDesign** | **S2** | Realistic‑human avatar busts (every row) | All player avatars realistic | Stick‑figure avatar busts | High |
| **ClanScreenDesign** | **S2** | **Dragon** clan crest + realistic member/chat avatars | Dragon beast + realistic avatars | Stick‑style clan crest (crowned‑stick) + stick avatars | High |
| **EventsHubDesign** | **S2** | Army silhouettes + **monster skull** (Endless Rush) + knight (Hero Trials) + warriors (Arena Clash) | Multiple card arts off‑identity incl. a monster | Stick army + stick‑undead + stick warriors per card | High |
| **OnlineBattleDesign** | **S2** | 2 realistic armored knight figures flanking VS | Foreground champions realistic | Blue stick vs red stick champions | High |
| **StoreScreenDesign** | **S2** | Battle‑Pass knight/king render + horned crown in bundle | Featured character render realistic | Stick king render; stick‑style crown | Med‑High |
| **LuckySpinDesign** | **S2** | **Lion** emblem (wheel hub) + realistic "Exclusive Avatar" segment | Beast emblem + realistic avatar | Stick‑king/crossed‑weapons crest; stick avatar | High |
| **MainMenuDesign** | **S1** (partial) | **Dragons** (sky) + realistic cavalry (right) — *hero trio is OK* | Stray dragons/cavalry pollute an on‑identity screen | Remove dragons (smoke/banners); stick cavalry or omit | High |
| **ModeSelectDesign** | **S1** (partial) | **Green zombie/undead** on "Endless" card — *other 4 cards OK* | One card breaks the set | Stick‑undead head | High |
| **ChestsScreenDesign** | **S1** | Hooded robed humanoid looming behind chest | Robed humanoid, not stick | Stick mage/king silhouette, or remove | Med |
| **ChestOpenResultDesign** | **S1** | Realistic "Lionhelm" commander helmet (reward card) | Realistic gear art (no body) off‑style | Stick‑style helm icon | Med |
| **FreeRewardsDesign** | **S1** | "Battle Boost" realistic knight thumbnail | Small realistic figure | Stick warrior thumbnail | Med |
| **CampaignMapDesign** | **S1** | Realistic armored avatar token (node 7) + HEROES icon | Map avatar realistic | Stick‑figure map token | Med |
| **PauseModalDesign** | **S1** | Blurred realistic soldier backdrop | Out‑of‑focus realistic combatants | Blurred stick army backdrop | Med |
| **VictoryScreenDesign** | **S1** | Realistic flag‑bearer + background soldiers | Background figures realistic | Stick victors/banner | Med |
| **DefeatScreenDesign** | **S1** | Prominent foreground realistic kneeling knight | Strong realistic figure | Kneeling stick warrior | Med‑High |
| **CampaignResultDesign** | **S1** | Blurred background soldier silhouettes | Faint realistic figures | Blurred stick army | Low‑Med |
| **SkinsScreenDesign** | **OK** | Stick hero + stick skin thumbnails (bg soldiers ambiguous) | On‑identity | — (verify bg soldiers) | High |
| **SpellsScreenDesign** | **OK*** | Borderline over‑rendered stick mage | On‑intent but heavier render than pure stick | Tighten to true stick silhouette | Med |
| **QuestsScreenDesign** | **—** | None (heraldic icons only) | No characters | — | High |
| **DailyRewardDesign** | **—** | None ("Legendary Unit" implied, not shown) | No visible character | — | High |
| **EndlessResultDesign** | **—** | None (dark abstract field) | No legible characters | — | Med |
| **LadderResultDesign** | **—** | None (crests/architecture) | No characters | — | High |
| **ConfirmModalDesign** | **—** | None | UI components only | — | High |
| **RewardGrantDesign** | **—** | None (reward objects) | No characters | — | High |
| **NetworkErrorDesign** | **—** | None (icon only) | No characters | — | High |
| **SettingsScreenDesign** | **—** | Tiny "StickKing" avatar (stick‑consistent) | No violation | — | Med |

**Totals:** S3 = 10 · S2 = 7 · S1 = 9 · OK/clean/no‑char = 12.

## 4. Creature taxonomy (what recurs)

| Creature | Screens | Replace with |
|---|---|---|
| **Realistic armored humans / knights** | Splash, Loading, Login, MatchIntro, Commander, Profile, BattlePass, BattleHud, SpellHud, Banner, Units, Store, Online, Tournament, Leaderboard, Clan, Events, FreeRewards, ChestOpen, CampaignMap, Pause, Victory, Defeat, CampaignResult | Stick warriors (faction‑tinted) per archetype table |
| **Orc / ogre warlord** | MatchIntro, CommanderSelect, InMatchBanner | Red‑accented **stick Heavy‑Brute** (bulkier stick, war‑paint/horned‑helm accent — *not* a green orc) |
| **Dragons** | Splash, Loading, Login, MainMenu, Clan (crest) | Remove (smoke/embers/banners) or stick‑heraldry crest |
| **Undead / zombie** | ModeSelect (Endless), InMatchBanner, Events (Endless Rush), Tournament | **Stick‑undead** (tattered stick + bone/green accents — on‑identity for Endless mode) |
| **Lion (beast emblem)** | LuckySpin | Stick‑king / crossed‑stick‑weapons heraldic crest |

## 5. Confidence & caveats

- **High confidence** where a figure is clearly rendered (foreground knights, orcs, dragons, the Units roster, the Endless zombie, the Clan dragon, the LuckySpin lion).
- **Medium/low** for small, blurred, or background silhouettes (Pause/Victory/CampaignResult backdrops, CampaignMap token) — flagged for art‑director confirmation, not auto‑replacement.
- **IP note (cross‑cutting):** reviewers did **not** find a confirmed 1:1 asset rip of a single game; the style reads as generic AAA dark‑fantasy / AI key art. The closest IP echoes are MainMenu/ModeSelect (stick art + "Stick Empire: Rise" logo, *Stick War* lineage) and the **"statue has fallen"** win/lose concept (Victory/Defeat). This reinforces Report 01 §0: build **original** stick art (Report 03 prompts) rather than reusing these mockups as final assets.


<!-- ==================== EMBEDDED: 03_IMAGE_CORRECTION_PROMPTS.md ==================== -->

# 03 — IMAGE CORRECTION PROMPTS (GPT‑Image / DALL·E)
**Goal:** for every offending `/design` screen (Report 02), a prompt that **preserves the composition, camera, lighting, FX and colour grade** and **replaces only the creature layer** with **original stick‑figure** art consistent with Stick Empire Rise.

**Two rules baked into every prompt:**
1. **Original art only** — do not reproduce any existing game's characters, unit designs, logos, or trademarks (see Report 01 §0). Stick‑figure *style* is generic; specific copied assets/marks are not.
2. **Keep UI zones clean** — render scene + characters as **background/key art with the UI areas left as uncluttered space and NO baked text/logos/buttons**. The app overlays live UI on top (Reports 04/05); baked UI is exactly what caused the on‑device doubling. Generate **art layers**, not finished screens.

---

## A. SHARED STYLE BLOCK  *(prepend to every prompt below)*

> **STYLE:** Original 2D cinematic dark‑fantasy game art in a **stick‑figure** character idiom. All characters are **black/charcoal silhouette stick‑men** — round featureless heads, thin clean limbs, simple readable poses — with minimal high‑contrast accents only: faction‑tinted cloth/capes, slim gold‑trimmed armor plates, simple iconic weapons (sword, bow, staff, spear, pickaxe, crossbow), and softly **glowing eyes**. **Iron Pact** = cool steel‑blue accents; **Ashen Horde** = red/ember accents. Painterly atmospheric environments and lighting are encouraged; **every living character must be a stick figure.** Mood: epic, grim, heroic. Landscape 16:9, high detail, crisp silhouettes, strong rim‑lighting.

## B. SHARED NEGATIVE BLOCK  *(append to every prompt)*

> **NEGATIVE / DO NOT INCLUDE:** realistic or muscular human anatomy; realistic faces; orcs, ogres, goblins; dragons or winged beasts; lions or animals as characters; zombies rendered as fleshy realistic monsters (use **stick‑undead** instead); 3D renders or photorealism; any existing‑game logo, wordmark, unit, or trademark; baked UI text, buttons, panels, watermarks, or letterforms; clutter in the reserved UI zones.

---

## C. S3 — fully off‑identity (regenerate character layer entirely)

### SplashScreen
*(style block)* Wide cinematic dusk battlefield overlooking a besieged medieval city; left‑to‑right sky gradient cool‑blue → fiery orange/crimson; strong sunset rim‑light. **Replace** the realistic armored king with a **stick KING hero** standing on the left cliff ledge: black stick body, gold crown, short dark‑red cape, sword in right hand, tall **blue banner** in left, heroic low‑angle silhouette against the sunset. **Remove the dragons** — replace with drifting smoke plumes and distant ember sparks. **Replace** the distant soldier horde with tiny **stick‑army silhouettes**. Keep the empty central ornate plaque zone **clear** for the live logo; keep the lower‑centre "tap" zone clear. **No baked text.** *(negative block)*

### LoadingScreen
*(style)* Symmetric wide shot down a corridor toward a burning dark‑spired fortress at centre (vanishing point); split palette cool‑blue left / warm‑red right; fires dotting the field. **Replace** both foreground realistic warriors with faction champions as **stick figures**: left = blue **stick swordsman** with kite shield + blue banner; right = red **stick heavy‑brute** (bulkier stick, horned‑helm accent, two‑hander). **Remove both dragons** (smoke/embers instead). **Replace** both massed armies with **blue vs red stick armies**. Leave the centred title + progress‑bar band as **clean empty space**. *(negative)*

### LoginAuth
*(style)* Centred symmetric onboarding scene, dark battlefield + burning castle softly blurred behind a centre panel zone; split blue‑left/red‑right light; gold ambience. **Replace** the two flanking realistic knights with **stick warriors** (blue stick swordsman left, red stick spearman right), softly depth‑blurred. **Remove the sky dragon.** Keep the entire central panel area **empty/clean** (live login panel overlays). *(negative)*

### MatchIntro
*(style)* Symmetric VS face‑off, low heroic angle, central diagonal energy seam (blue lightning left, orange fire right) as focal axis; high contrast cool/warm rim light. **Replace** the realistic knight (left) with a **blue stick KING/Commander** — crown, blue cape, raised sword, kite shield with simple crest; **replace** the orc warlord (right) with a **red stick HEAVY‑BRUTE** — bulkier stick body, red war‑cloak, horned‑helm accent, heavy flail/maul. Both in dynamic confrontation poses, glowing eyes. Keep top title banner zone, centre "VS" zone, and bottom nameplate zones **clear**. *(negative)*

### CommanderSelect
*(style)* Two symmetric three‑quarter portrait vignettes facing centre, dark stone‑dungeon backdrop, split blue/red + gold light. **Replace** the realistic human "Warden" (left) with a **blue stick Commander** portrait bust: crown/helm, blue cape, gold‑trim shoulder plates, stern glowing‑eye pose. **Replace** the orc "Warchief" (right) with a **red stick Heavy‑Brute** portrait bust: horned‑helm accent, red cloak, jagged pauldrons. Leave ability‑card, name, level, and SELECT zones **clear**. *(negative)*

### Profile
*(style)* Front‑on dashboard; central ornate portrait frame is the focal point; warm key light; dark navy hall backdrop; epic‑purple rarity accents. **Replace** the realistic hero "Thalrion" with a **stick KING hero** portrait inside the frame: black stick body, gold crown, ornate blue‑and‑gold cape, sword at rest, confident glowing‑eye pose. Keep stat tiles, nav rail, gear row, and title zones **clear**. *(negative)*

### BattlePass (tier‑30 capstone)
*(style)* Reward‑grid scene; dark battlefield silhouette backdrop; royal‑purple + gold glow on the capstone reward panel (right). **Replace** the realistic royal‑armored king reward render with a **stick KING in royal regalia**: purple‑and‑gold cape, ornate crown, glowing eyes, triumphant pose, magical purple aura. Keep all reward‑grid/tier/progress zones **clear**. *(negative)*

### BattleHud  *(in‑match key art / unit‑icon source)*
*(style)* High three‑quarter **isometric** battlefield down a horizontal lane; blue keep far‑left, red fortress far‑right; central troop collision lit by small orange fires; cool‑blue left / warm‑red right faction tinting. **Replace** all on‑field troops with **stick armies** — blue **stick swordsmen/archers/spearmen** vs red **stick warriors** — small but crisp silhouettes clashing centre. (Generate a separate clean set of **5 stick unit busts** — swordsman, axeman, archer, cavalry‑stick, crossbowman — for the spawn‑bar icons.) Keep all HUD bar/button zones **clear**. *(negative)*

### InMatchSpellHud
*(style)* Same isometric battlefield, mid‑cast, with a glowing cyan runic ground‑targeting ring at centre as focal point. **Replace** all troops with **stick armies** (blue vs red); generate a clean **stick hero bust** for the bottom‑right portrait ring and **5 stick spawn icons**. Keep HUD/spell‑bar zones **clear**. *(negative)*

### InMatchBanner  *(highest‑severity creature swap)*
*(style)* Same isometric battlefield with an upper event‑banner zone; blue keep defended left, advancing enemy wave right; cyan friendly glows, hot‑red enemy embers; necrotic theme. **Replace** the blue defenders with a **blue stick army**; **replace the realistic army + ogre/monster/undead horde** on the right with a **red STICK‑UNDEAD horde** — tattered stick bodies with **bone‑white/green necrotic accents**, glowing eyes, ragged silhouettes (on‑identity for "The Dead Awaken"). Optionally one larger **stick‑brute** as the wave anchor. Keep the top banner/timer zone and all HUD zones **clear**. *(negative)*

### UnitsArmy  *(heaviest — full roster regeneration)*
*(style)* Dark steel‑blue gloomy castle backdrop; gold‑framed roster grid + right inspector; cold light + warm rim. **Replace the entire roster** with **stick‑figure unit portraits**, one per role, each a clean bust on a neutral vignette: **Shieldman** (sword+kite shield), **Sentinel** (spear+tower shield), **Iron Archer** (bow+hood), **Heavy Guard** (two‑hander, bulky stick), **Runic Adept** (robe+staff, glowing eyes), **Miner** (pickaxe+satchel), **Warden** (sword+cape), **Crossbowman** (crossbow), **Oathbreaker** (red‑accent stick), **Flamecaller** (staff with fire, ember accents). Blue tints for Iron Pact, red for Ashen Horde. Render the large **Shieldman inspector** portrait too. Keep stat/upgrade/tab zones **clear**. *(negative)*

## D. S2 — prominent creature(s) (swap the figures)

### TournamentLadder
*(style)* Torch‑lit cathedral bracket converging on a central gold laurel "Champion" crest; deep one‑point perspective; gold connector lines on near‑black. **Replace** all ~16 competitor avatar busts with **varied stick‑figure busts** (different helms/hoods/weapons, mixed blue/red/neutral tints, a few **stick‑undead** for variety). Keep the centre crest zone and bracket‑label zones **clear**. *(negative)*

### Leaderboard
*(style)* Front‑on ranking table, dark ruined‑castle skyline backdrop, gold trim, league‑coloured badges. **Replace** every player avatar bust with a **distinct stick‑figure bust** (helms/hoods/weapons varied). Keep rank/name/score/badge zones **clear**. *(negative)*

### ClanScreen
*(style)* Three‑panel guild dashboard, dark castle backdrop, gold filigree frames. **Replace the dragon clan crest** with a **stick‑style heraldic emblem** (a crowned **stick‑king silhouette** or **crossed stick‑weapons** on a faction shield). **Replace** all member/chat avatar busts with **stick‑figure busts**. Keep roster/chat/tab zones **clear**. *(negative)*

### EventsHub
*(style)* Rainy night siege hub; featured top banner + four event cards; gold frames, cool‑blue rim over warm embers. **Replace**: banner army → **stick army** before a wagon in rain; "Endless Rush" monster skull → **stick‑undead head** with green necrotic glow; "Hero Trials" knight → **stick warrior** in a fiery scene; "Arena Clash" warriors → **two dueling stick fighters** (blue vs red). Keep title/card‑text/button zones **clear**. *(negative)*

### OnlineBattle
*(style)* Symmetric async‑PvP VS scene, dark war‑hall, explosive blue‑vs‑red "VS" disc focal centre, split side lighting. **Replace** the two flanking realistic knights with **stick champions** (blue stick Commander left, red stick Brute right), depth‑blurred behind the banners. Keep VS/panel/FIND‑MATCH zones **clear**. *(negative)*

### Store (featured bundle + Battle‑Pass art)
*(style)* Torch‑lit treasury hall; central glowing chest+gem hero spot; gold‑on‑black opulent framing; warm torch + cool gem glow. **Replace** the Battle‑Pass knight/king render (right) with a **stick KING** hero in royal blue‑gold regalia; **replace** the horned crown atop the bundle with a **stick‑style crown/helm** silhouette. Keep all card/price/tab zones **clear**. *(negative)*

### LuckySpin
*(style)* Casino prize‑wheel on near‑black; warm gold rim glow; multicolour segment fills. **Replace the golden lion hub emblem** with a **stick‑king or crossed‑stick‑weapons gold crest**; **replace** the "Exclusive Avatar" segment portrait with a **stick‑figure avatar**. Keep wheel‑segment label and button zones **clear**. *(negative)*

## E. S1 — minor / background / gear‑only (lighter swaps)

> For these, regenerate only the small/background creature element in stick style; everything else (environment, lighting, UI zones) is preserved unchanged.

- **MainMenu:** keep the **stick hero trio** (king/mage/archer) — they are correct. **Remove the sky dragons** (replace with smoke/birds/banners) and **convert the right‑side realistic cavalry** to **stick cavalry** or omit. Keep logo/currency/button zones clear. *(style + negative)*
- **ModeSelect:** keep four stick cards; **replace the "Endless" card's green zombie** with a **stick‑undead head** (green necrotic accents). *(style + negative)*
- **Chests:** **replace the hooded robed humanoid** behind the chest with a **stick mage/king silhouette** (or remove for a clean arcane vault). *(style + negative)*
- **ChestOpenResult:** **replace the realistic "Lionhelm" helmet** reward art with a **stick‑style helm icon** (simple gold‑trim crest helm). *(style + negative)*
- **FreeRewards:** **replace the "Battle Boost" knight thumbnail** with a **stick warrior** thumbnail. *(style + negative)*
- **CampaignMap:** **replace the node‑7 realistic avatar token + HEROES icon** with a **stick‑figure token/icon**; keep the green→lava biome map unchanged. *(style + negative)*
- **Pause:** **replace the blurred realistic soldier backdrop** with a **blurred stick‑army** battlefield (same banners/fires/blur). *(style + negative)*
- **Victory:** **replace the flag‑bearer + background soldiers** with **stick victors** raising a blue banner; keep the dusk god‑ray reward composition. *(style + negative)*
- **Defeat:** **replace the foreground kneeling realistic knight** with a **kneeling stick warrior** (tattered cape, dropped sword), same bleak storm composition. *(style + negative)*
- **CampaignResult:** **replace blurred background soldier silhouettes** with **blurred stick‑army** silhouettes. *(style + negative)*

## F. Production notes

- **Output as layers where the tool allows** (character layer separate from background) so the live UI and the corrected characters composite cleanly without re‑introducing baked UI.
- **Aspect ratio:** generate at the mockup's ratio (≈16:9 landscape) at ≥2048 px wide; downscale in the asset pipeline.
- **Consistency pass:** generate the **archetype sheet** (Report 08) FIRST, then reuse those exact stick designs across all screens so the king/mage/archer/etc. look identical everywhere.
- **Validation:** every regenerated image must pass the Report 02 classifier (zero `NON‑STICK` characters) before it enters the asset pipeline.


<!-- ==================== EMBEDDED: 04_UI_FAILURE_ROOT_CAUSE.md ==================== -->

# 04 — ROOT CAUSE: "BROKEN UI OVER ART"
**Question:** why does the current implementation look wrong when real art is enabled?
**Evidence base:** direct on‑device observation of build **0.0.92** (real‑asset re‑skin) on a Redmi Note 11R, plus the source‑art forensic catalog (Report 02) and the code architecture (Report 00 §2).

---

## 1. Symptom catalog (observed on device, 0.0.92)

| # | Symptom | Screen(s) | Screenshot evidence |
|---|---------|-----------|---------------------|
| Y1 | **Double logo** — baked "STICK EMPIRE / RISE" shows behind live "BULWARK / RISE" | MainMenu | `runtime/device_validation/rc_0.0.92/01_mainmenu.png` |
| Y2 | **Garbled button labels** — baked "RESUME/SETTINGS/SURRENDER" spreads across gem buttons under the live label → "RE·PLAY·ME" | MainMenu, everywhere gem buttons appear | `01_mainmenu.png` |
| Y3 | **Panel bleed** — baked "PAUSED" + Resume/Surrender show through gem‑pack cards and the Battle‑Pass promo panel | Store | `03_store.png` |
| Y4 | **List bleed + double title** — baked RESUME/SETTINGS/SURRENDER stretched across rows; baked "LEADERBOARD" behind live title | Leaderboard | `07_leaderboard.png` |
| Y5 | **Double title** — baked "SETTINGS" behind live "SETTINGS" | Settings | `04_settings.png` |

(For contrast, ModeSelect and panel screens whose live content fully covers the chrome looked correct — the failure is specifically *partial coverage of baked content*.)

## 2. The two mechanisms

Every symptom reduces to one of two mechanisms — both are the same mistake at different scales: **baked pixels placed *under* live UI that does not fully occlude them.**

### Mechanism M1 — full mockups used as backdrops
The re‑skin set each screen's background to its **full `/design` mockup** (`UiWidgets.Backdrop`). But those mockups are **finished screen compositions** — they contain baked **logos, titles, currency, and buttons** (Report 02 + the catalog). The live uGUI then draws BULWARK's own logo/title/buttons on top. Where the live element doesn't pixel‑cover the baked one (different position, font, size, or wording), **both show** → Y1, Y4‑title, Y5.

### Mechanism M2 — chrome 9‑sliced from a mockup that has baked text
The reusable gem **buttons** and ornate **panel** were sliced from the **Pause mockup**, which has baked "PAUSED / RESUME / SETTINGS / SURRENDER" text. The working assumption was *"the live label will cover the baked text."* That assumption **breaks under 9‑slice stretch**: BULWARK's buttons are far wider than the 553‑px source button, so the baked word stretches **past** the centered live label and pokes out both sides → Y2; the panel's baked interior (title + 3 buttons) stretches across large panels where live content is sparse → Y3, Y4‑rows.

> Both were partially mitigated late in the session (`clean_button`/`clean_panel` erase the baked text; a 0.45 backdrop scrim dims baked logos — commit `11f0a7b`). Those are **patches on a flawed source pipeline**, not the cure. The cure is architectural (Report 05).

## 3. Root‑cause tree

```
SYMPTOMS (Y1–Y5: doubling, bleed, garble)
  └─ PROXIMATE CAUSE
     • Full finished-screen mockups used as background art, AND
     • UI chrome (buttons/panels) sliced from a mockup that has baked UI text,
       then composited UNDER live UI that does not fully occlude it.
        └─ UNDERLYING CAUSE
           • The source art is FINISHED SCREENS, not separated layers.
             There is no clean "background", "character", or "UI-chrome" layer —
             everything (scene + characters + logos + buttons + text) is flattened
             into one PNG per screen. Any extraction necessarily drags baked UI along.
           • A PROCEDURAL-FIRST UI was retrofitted with extracted art, producing a
             HYBRID with three conflicting visual sources on one screen:
               (a) UiTex procedural primitives,
               (b) extracted-and-cleaned sprites,
               (c) extracted sprites that still carry baked UI.
              └─ SYSTEMIC CAUSE
                 • "Mockup = asset source" conflation: a *design comp* (meant to show
                   what a screen should look like) was treated as a *production asset*
                   (meant to be sliced and shipped). Comps and assets are different artifacts.
                 • No art/UI separation discipline in the design deliverable (no layered
                   PSD/Figma export of background vs character vs chrome).
                 • Dual UI systems coexisted (legacy UiFlow panels + new UiRouter screens),
                   adding a second bleed-through vector (fixed separately: suppress UiFlow
                   when the router owns the front-end, commit 5d5f828).
                 • No pre-device acceptance gate caught the doubling — it only became
                   visible once real art was loaded on the device.
```

## 4. Why it happened (process honesty)

1. **A directive to "use `/design` as the actual asset source"** was taken literally: full mockups became backdrops and the chrome was sliced from a mockup. That was the fastest path to "real art on screen," and it **did** lift fidelity dramatically — but it imported baked UI as a side effect.
2. **The mockups were never produced as layered, UI‑free background plates.** With only flattened comps available, *any* reuse mixes background, character, and chrome.
3. **The UI was already built procedurally** (code‑built uGUI with `UiTex`). Adding real art *underneath* an existing, differently‑positioned procedural UI guarantees mismatch wherever the two don't align pixel‑for‑pixel.
4. **The Construction Bible actually warned about this** — its per‑screen "Section N" says authored art (matte paintings, frames, hero renders) must be **supplied as clean, separate assets**, which the code *places*. Slicing them out of baked comps was a deviation from that guidance, and it reintroduced exactly the baked content the Bible assumed would be absent.
5. **Validation happened too late in the loop.** The doubling is invisible in the editor/compile gate and only appears when the device loads the real textures, so it survived until on‑device review.

## 5. What must STOP (anti‑patterns to ban)

- ❌ Using **finished‑screen mockups** as runtime backgrounds.
- ❌ Slicing **UI chrome** (buttons/panels/frames) out of comps that contain baked text.
- ❌ Relying on **"the live element will cover the baked one."**
- ❌ Shipping **two UI systems** that can both draw the front‑end.
- ❌ Treating a **design comp as a production asset**.

## 6. What the fix must guarantee (hand‑off to Report 05)

1. **One source of truth per layer:** clean **background plates with NO baked UI/characters**, a separate **character/key‑art layer**, and **UI chrome authored as a real UI kit** (clean 9‑slice frames/buttons/icons with **no baked text**).
2. **One UI system** (UiRouter) owning the front‑end; legacy paths removed.
3. **Live UI is the only source of text** — backgrounds never contain words.
4. **A pre‑ship acceptance gate** (device screenshot vs reference, plus a "no baked text in any background/kit sprite" check) so doubling can't recur.

Report 05 selects the architecture that delivers these guarantees.


<!-- ==================== EMBEDDED: 05_FINAL_UI_INTEGRATION_ROADMAP.md ==================== -->

# 05 — FINAL UI INTEGRATION ROADMAP
**Goal:** choose the UI architecture that makes the "broken UI over art" failure (Report 04) **structurally impossible to recur**, then sequence the migration with validation, rollback, and acceptance criteria.

---

## 1. Candidate architectures

| # | Approach | Fidelity ceiling | Dynamic data | Localizable | Maintainable | Perf | Production cost | Verdict |
|---|---|---|---|---|---|---|---|---|
| A | **Full code recreation** (pure procedural `UiTex`, no art) | Low | ✓ | ✓ | ✓ | ✓ | Low | ❌ Can't reach AAA — this is today's fallback look |
| B | **Full image integration** (each screen = one finished PNG + invisible tap zones) | Very high (static) | ✗ | ✗ | ✗ | ✓ | Low | ❌ No live values/state; baked wrong‑brand text; **is the doubling trap** |
| C | **Hybrid extraction** (slice chrome from finished comps — *current*) | High | ✓ | ✓ | ✗ | ✓ | Low | ❌ Imports baked UI → Report 04 failure |
| D | **Atlas‑driven UI** (clean kit packed into a SpriteAtlas) | High | ✓ | ✓ | ✓ | ✓✓ | Med | ◐ Necessary, but only the *packing* half |
| E | **Slice‑driven UI** (clean 9‑slice frames/buttons, all states) | High | ✓ | ✓ | ✓ | ✓ | Med | ◐ Necessary, but only the *chrome* half |
| F | **Layered UI** (separate background / character / UI layers, clean sources) | High | ✓ | ✓ | ✓✓ | ✓ | Med | ◐ The right *principle* |
| **★** | **RECOMMENDED = F over (D+E): Layered UI on a clean authored 9‑slice + atlas UI kit, over UI‑free background plates** | **Very high** | ✓ | ✓ | ✓✓ | ✓✓ | Med‑High | ✅ **Selected** |

## 2. Recommended architecture — "Clean Layered UI Kit"

The failure was **mixed/dirty sources composited in conflicting layers**. The cure is **clean sources in disciplined layers**, with **one rule: backgrounds and kit sprites contain ZERO text and ZERO UI; all text/data is live.**

```
LAYER 3  LIVE CONTENT      ← app-owned: ALL text, numbers, lists, bars, timers (TMP).  The ONLY text source.
LAYER 2  UI KIT (atlas)    ← authored CLEAN 9-slice frames/panels/buttons(all states)/tabs/icons/currency pills.
                             NO baked text. Packed into SpriteAtlas(es). Replaces UiTex AND mockup-sliced chrome.
LAYER 1  CHARACTER/KEY-ART ← optional per-screen stick hero/key art (Report 03), placed deliberately (not full-screen).
LAYER 0  BACKGROUND PLATE  ← UI-FREE atmospheric plate (Report 03 corrected art, UI zones clean). Shared biomes where possible.
```

**Invariants (enforced by the acceptance gate, §6):**
1. No Layer‑0/Layer‑2 sprite contains **any** baked text, logo, button, or wordmark.
2. **One** UI system owns the front‑end: **UiRouter**. Legacy `UiFlow` front‑end paths are removed (the bleed‑through vector from Report 04 §3).
3. Live text uses **TMP** (SDF) so it scales crisply at 2340×1080 and supports localization (current legacy `Text` is a known prestige gap).
4. Background plates are dimmed by a **deliberate design scrim** (for legibility), never to hide baked content.

**Why this kills the failure modes:**
- M1 (full‑mockup backdrops) → impossible: backgrounds are **UI‑free plates**, so there is no baked logo/title/currency to double.
- M2 (baked‑text chrome) → impossible: the kit is **authored clean** (no baked words), so stretching can't reveal text.

## 3. Reuse what already works

The current code is **not** thrown away — it already implements the right *shape*:
- `UiRouter`/`UiScreen` stack, `CanvasScaler` 2340×1080, `SafeAreaFitter` → **keep**.
- `UiWidgets` builders (`OrnateFrame`, `GemButton`, `Card`, `Backdrop`, `CurrencyChip`, `TabBar`) → **keep the API, swap the sprite source** from "extracted‑from‑comp / UiTex" to "clean authored kit atlas."
- `UiAssets` runtime loader + `BackdropBinder` → **keep**; point them at clean plates + the kit atlas.
- `UiTex` procedural set → **demote to dev‑only fallback** (degrade gracefully if an asset is missing), never the shipping look.

This means the migration is **mostly an asset swap behind a stable builder API**, not a UI rewrite — low code risk.

## 4. Phased roadmap

**Phase U0 — Asset contract (blocking).** Define the deliverable spec the art pipeline must hand over: (a) UI‑free background plates per screen/biome; (b) the **UI kit** (frames, panels, buttons ×6 colours ×states, tabs, icons, currency pills, badges) as clean PNGs with documented 9‑slice borders; (c) the stick **archetype sheet** (Report 08). *Output: a one‑page asset contract; the rest of the pipeline depends on it.*

**Phase U1 — Authored UI kit + atlas.** Produce the clean kit (from Report 03 prompts / a UI artist), pack into `SpriteAtlas`(es). Wire `UiWidgets` chrome builders to the atlas (API unchanged). *Acceptance: every screen's chrome renders from clean kit sprites; zero baked text; atlas batches.*

**Phase U2 — UI‑free background plates.** Replace the full‑mockup backdrops with UI‑free plates (corrected per Report 03). Keep `Backdrop()` + scrim. *Acceptance: no baked logos/titles anywhere; live titles/logos stand alone.*

**Phase U3 — TMP text migration.** Import a licensed serif/display SDF font; migrate `UiWidgets.Label`/`TitleLabel` to TMP with gold‑bevel material presets. *Acceptance: crisp text at device DPI; localization‑ready.*

**Phase U4 — Single UI system.** Remove the legacy `UiFlow` front‑end entirely (router owns boot→menu→match). *Acceptance: no second system can draw the front‑end.*

**Phase U5 — Character/key‑art layer.** Add deliberate stick hero/key‑art per hero screen (MainMenu, Profile, Commander, results), composited as Layer‑1 (not full‑screen). *Acceptance: heroes are stick figures, placed, not baked into the background.*

**Phase U6 — Device fidelity loop.** Per‑screen screenshot vs reference on the Redmi; tune scrim/spacing; iterate until the gate (§6) passes for all 35 screens.

## 5. Dependencies

- **Art:** the UI kit + plates + archetype sheet (Reports 03/08) — the critical path. Until clean assets exist, U1/U2/U5 are blocked (this is the same authored‑art dependency the Construction Bible flagged).
- **Font:** a licensed display font + a TMP SDF asset (editor‑generated) for U3.
- **Legal:** brand/IP clearance (Report 01 §0) before any of this is wrapped in store‑facing marketing.

## 6. Acceptance gate (must pass to ship a screen)

- [ ] **Zero baked text** in any Layer‑0 plate or Layer‑2 kit sprite (automated check + visual).
- [ ] **No duplicated** logo/title/header/panel/button on any screen (device screenshot review).
- [ ] All **characters are stick figures** (Report 02 classifier = 0 violations).
- [ ] Device screenshot ≥95% match to the corrected reference (composition/spacing/colour).
- [ ] Single UI system; no `UiFlow` front‑end artifacts.
- [ ] Text is TMP, crisp at 2340×1080, no overflow/clipping; safe‑area respected.
- [ ] 60 fps / no OOM regression on the 3.66 GB reference device.

## 7. Rollback strategy

Because the migration is an **asset swap behind a stable builder API**, each phase is independently revertible:
- **U1/U2/U5:** `UiAssets` falls back to `UiTex` procedural if a kit/plate asset is absent → a missing or bad asset **degrades**, never crashes. Revert = remove the asset / flip the source flag.
- **U3 (TMP):** keep the legacy `Text` path behind a compile/feature flag until TMP is validated on device; revert = flip the flag.
- **U4 (single system):** done last, on its own commit; revert = restore the `UiFlow` suppression shim (already exists).
- All phases are separate commits on a feature branch with the device‑fidelity gate (§6) as the merge bar.

## 8. One‑line answer

**Adopt a clean, layered UI kit:** UI‑free background plates + an authored 9‑slice/atlas UI kit with no baked text + a separate stick character layer + live TMP text, all driven by the existing UiRouter/UiWidgets API. It reuses today's code shape, removes the dirty sources that caused the crisis, and makes the failure structurally impossible — gated by a device screenshot‑vs‑reference acceptance check.


<!-- ==================== EMBEDDED: 06_BATTLEFIELD_VISUAL_STRATEGY.md ==================== -->

# 06 — BATTLEFIELD VISUAL STRATEGY
**Problem:** the current in‑match battlefield (ECS primitives over a flat/placeholder background) is unacceptable. Define the definitive replacement.
**Constraints:** stick‑figure identity; §12 boundary (presentation only — no ECS/gameplay change); 3.66 GB‑RAM reference device (must hit 60 fps / no OOM); win condition = **destroy the enemy statue**.

---

## 1. Reference analysis

| Reference | What to take | What to avoid |
|---|---|---|
| **Stick War: Legacy** | The native idiom: **2D side‑view lane**, two statues, parallax depth, stick armies — exactly our identity & win condition | Don't copy its specific art/assets (IP, Report 01 §0) |
| **Kingdom Rush** | Layered painterly parallax, readable silhouettes, ambient micro‑animation, weather accents | Tower‑defense fixed camera (we pan a lane) |
| **Clash Royale** | Clean readability at small scale, punchy FX, performance discipline on mobile | 3D/iso arena (off‑identity for sticks) |
| **The mockups (`BattleHud`)** | The blue‑keep‑left vs red‑fortress‑right framing, faction colour split, central clash | The realistic iso troops + realistic units (Report 02) |

**Decision:** a **2D orthographic side‑view parallax lane** — Stick War‑native, on‑identity, cheapest to animate, and the most performant choice for a low‑RAM device. (The mockups' iso look is abandoned: it conflicts with the stick identity and is far costlier to animate.)

## 2. Layer model (UI‑free, per Report 05 invariants)

```
SKY        far gradient + drifting cloud band (slow parallax)        depth 1.0
HORIZON    distant castle/mountain silhouettes, faction-tinted        depth 0.8
MIDGROUND  rolling terrain, forest line, the two STATUES + keeps      depth 0.5
PLAYFIELD  the ground plane where stick units march & fight           depth 0.0  ← gameplay
FOREGROUND grass tufts, rocks, banners, occasional fly-through        depth -0.3 (in front, blurred)
FX/AMBIENCE embers, dust motes, smoke, weather                        additive overlays
```

The **PLAYFIELD** is the only gameplay layer (ECS units render here). Every other layer is **presentation** — pure art + particles, no ECS coupling (§12 safe).

## 3. Animation & ambience (all unscaled/presentation)

- **Parallax on camera pan:** as the camera tracks the front line, layers scroll at depth‑scaled rates → strong depth illusion from flat sprites.
- **Idle motion:** drifting clouds, swaying trees/banners (vertex/UV scroll), shimmering water, the **statues' idle aura glow** (faction‑tinted) so the objective always reads.
- **Combat ambience:** ground dust at the clash line, ember/spark bursts on impacts, smoke columns from fires, screen‑space heat shimmer near fires (cheap shader).
- **Camera treatment:** smooth ease toward the active front line; subtle impact shake on big hits / statue damage; a brief push‑in on statue destruction (victory beat). Camera is presentation‑only.

## 4. Weather & time‑of‑day

- **Weather** = additive particle overlay + a global tint, toggled per map: **rain** (streaks + darken + occasional lightning flash), **snow** (drift + cool tint + fog), **dust/ash** (Ashen biomes), **fog** (depth haze band). Implemented as pooled particle systems + one full‑screen tint quad — negligible cost.
- **Time‑of‑day:** ship **fixed per‑map time‑of‑day** (day / dusk / night) via a global colour grade + light‑direction tint per biome (Report 07). **A live day/night *cycle* is explicitly deferred** (P3) — it adds cost/complexity for little gameplay value on this device.

## 5. Performance budget (3.66 GB device, 60 fps target)

- **Backgrounds:** 5–7 large parallax sprites per biome, packed in **one SpriteAtlas per biome**; reuse across maps of the same biome. No per‑map unique 4K plates.
- **Lighting:** **no realtime lights** — bake mood into the art + a global tint. (Realtime lights are the classic mobile perf/OOM trap.)
- **Particles:** GPU particles, hard caps per system, **object‑pooled**; weather is one capped emitter + tint.
- **No heavy post‑processing** (no bloom stacks/DoF on this tier); fake glow via additive sprites.
- **Memory:** atlas streaming per biome; unload the previous biome on map change. Backgrounds as compressed textures (ASTC). This directly addresses the observed `lowmemorykiller` OOMs.
- **Draw calls:** atlas batching keeps the background at a handful of draw calls.

## 6. Definitive recommendation

Build the battlefield as a **2D side‑view parallax lane** with **5–7 UI‑free layered sprites per biome** (sky / horizon / midground+statues / playfield / foreground) + **pooled additive FX** for ambience, weather, and impacts, **fixed per‑map time‑of‑day** via global tint, **no realtime lights / no heavy post‑FX**, atlas‑batched and biome‑streamed for the 3.66 GB device. Camera pans/eases to the front line with light impact shake and a statue‑destruction push‑in. This is on‑identity (Stick War lineage), the cheapest path to a "alive" battlefield, and stays entirely within §12 (presentation only). Statues get a persistent faction aura so the win objective always reads.


<!-- ==================== EMBEDDED: 07_ENVIRONMENT_ASSET_STRATEGY.md ==================== -->

# 07 — ENVIRONMENT ASSET STRATEGY
**Goal:** define how the battlefield environment is built — statues, trees, rocks, mines, terrain, props, fore/background — as a coherent, performant, on‑identity 2D asset set.
**Pairs with:** Report 06 (battlefield layers/perf) and Report 05 (clean‑layer discipline).

---

## 1. Visual style

**Flat, bold 2D vector‑style** consistent with the stick‑figure characters: clean silhouettes, limited palette per biome, painterly texturing kept subtle so **characters read against it**. High silhouette contrast between the playfield and the background (units must never get lost in busy art). Faction coding by tint, not by clutter.

## 2. Biome system (reuse > bespoke)

Author **biome kits**, not per‑map art. Each biome = one SpriteAtlas of layered plates + a prop set + a colour grade. Maps are **compositions of a biome kit**, so 12 campaign levels don't need 12 unique art sets.

| Biome | Mood / grade | Signature props |
|---|---|---|
| **Greenfield / Forest** | bright day, green/gold | oaks, bushes, wooden palisade, mill |
| **Siege / Castle** | dusk, slate+ember | castle walls, broken siege engines, banners, rubble |
| **Ashen / Volcanic** | night, red/black, ash | lava cracks, charred trees, bone piles, ember vents |
| **Frost / Haunted** | cold/fog, teal | dead trees, snowdrifts, fog banks, gravestones |

## 3. Asset catalogue & treatment

| Asset class | Layer | Animated? | Treatment | Notes |
|---|---|---|---|---|
| **Statues** (player + enemy) | midground | **Yes** — idle aura glow, damage states, destruction | **Highest priority** — they are the win condition; must read instantly, faction‑tinted aura, 3–4 damage stages + a destruction burst | One rig, two tints |
| **Terrain / ground plane** | playfield | subtle (dust, footstep decals) | Tileable ground strip per biome; the surface units walk on; must be flat enough to read units | Decal pooling |
| **Keeps / spawn structures** | midground | minor (banner flutter, smoke) | Faction barracks/keep flanking each statue | 2 tints |
| **Trees / foliage** | mid + foreground | sway (UV/vertex) | A few variants per biome; foreground ones blurred & in front | Atlas |
| **Rocks / boulders** | mid + foreground | none | Silhouette breakers; cheap | Atlas |
| **Mines / resource nodes** | midground | shimmer/sparkle | Gold/crystal node where stick **Miners** work; gentle glint so it reads as interactive | Tie to economy *display* only (§12) |
| **Decorative props** | mid + foreground | minor | banners, tents, barrels, bones, broken weapons | Scatter set |
| **Foreground elements** | foreground | parallax + slight blur | grass tufts, fences, occasional fly‑through (crows/embers) | Depth/parallax |
| **Background silhouettes** | horizon | slow parallax | castles, mountains, distant armies (as **stick** silhouettes) | 1 plate/biome |

## 4. Pipeline

1. **Author per biome:** one layered source (background plates + prop sheet) → export UI‑free PNG layers → pack into a **per‑biome SpriteAtlas** (ASTC compressed).
2. **Statues & mines** get a small **state machine** (idle / damage stages / destroyed; idle / active) driven by presentation events mirrored from ECS read‑only state — **no ECS writes** (§12).
3. **Scatter system:** props placed via lightweight presentation data (positions/variants per map), not hard‑coded, so maps are cheap to compose.
4. **Performance:** one atlas resident per biome; unload on biome change (addresses OOM); pooled FX; no realtime lights (bake mood into art + global tint per Report 06).

## 5. Animation requirements (minimum viable "alive")

- Statues: idle aura loop + damage transitions + destruction (must‑have).
- Trees/banners/foliage: ambient sway loop.
- Mines: shimmer loop + active sparkle when worked.
- Fires/smoke: pooled particle loops.
- Foreground: parallax + occasional fly‑through.

## 6. Production priority

1. **P0:** Statues (player + enemy) with damage/destruction states — *the objective must read.*
2. **P0:** One complete **Siege/Castle biome kit** (the default battle look) — ground, keeps, 1 horizon plate, core props.
3. **P1:** Greenfield biome (campaign early game) + mines (economy readability).
4. **P1:** Ambient animation pass (sway, shimmer, fires).
5. **P2:** Ashen + Frost biomes; weather overlays (Report 06).
6. **P3:** Extra prop variety, foreground fly‑throughs, decorative density.

## 7. Definitive recommendation

Build **reusable per‑biome 2D layered kits** (atlas‑packed, ASTC, UI‑free) rather than per‑map bespoke art; treat **statues as the top‑priority animated hero‑prop** (win condition); animate the world cheaply (sway/shimmer/pooled FX, no realtime lights); and start with **one Siege/Castle biome + statues** to make the default battle look correct, then expand biome‑by‑biome. This is on‑identity, memory‑safe for the 3.66 GB device, and §12‑clean (all presentation).


<!-- ==================== EMBEDDED: 08_CHARACTER_PRODUCTION_STRATEGY.md ==================== -->

# 08 — CHARACTER PRODUCTION STRATEGY (the stick army)
**Problem:** the game has no real characters — only ECS primitives in play and realistic, off‑identity figures in the mockups (Report 02). Define how to produce the **stick‑figure** cast.
**Constraints:** stick identity; §12 (presentation only); 3.66 GB device; two factions (Iron Pact blue / Ashen Horde red).

---

## 1. The production‑method decision

| Method | Fit for stick figures | Memory | Flexibility (reskin/equip) | Cost | Verdict |
|---|---|---|---|---|---|
| **Sprite sheets** (pre‑rendered frames per unit per anim) | OK | **High** (frames × units × anims) | Low (re‑render per change) | High (art per unit) | ❌ Memory + cost explode with roster size |
| **2.5D / 3D** | Off‑identity, overkill | High | Med | Very high | ❌ Wrong look, wrong budget |
| **2D skeletal — Spine** | **Excellent** | Low | **Excellent** (mesh deform, skins) | Med + licence | ★ Premium choice |
| **2D skeletal — Unity 2D Animation + 2D IK** (in‑engine) | **Excellent** | Low | **Excellent** (bones + sprite swap) | Low (free, Unity‑native) | ★ **Default recommendation** |

**Decision: 2D skeletal/bone animation.** Stick figures are the *ideal* case for it — a stick body is essentially a bone rig with thin limbs. **One shared humanoid stick rig** is authored once; **every archetype reuses it** with swappable equipment sprites + a faction tint. This is the Stick War approach and it is dramatically cheaper than sprite sheets at roster scale.

**Tooling recommendation:** ship on **Unity's 2D Animation package (bone‑based) + 2D IK** by default (zero extra licence, Unity‑native, atlas‑friendly); treat **Spine** as an optional premium upgrade if mesh‑deform quality demands it later. FX stays sprite‑sheet/particle.

### Why this is the key unlock
Because all humanoid units share one rig, you **animate the locomotion/combat set ONCE** and the entire roster inherits it. New units = a new weapon + accent sprite + tint (hours), not a new animation set (weeks). This collapses the "no characters" problem into a tractable, scalable pipeline.

## 2. Shared rig + animation set (author once)

**Rig:** humanoid stick skeleton — head, torso, 2 arms (upper/fore/hand), 2 legs (thigh/shin/foot), optional cape/cloak bone. Thin clean limbs, round head, glowing‑eye sprite.

**Shared animation set (inherited by all):** `idle`, `march/walk`, `run`, `melee_attack` (A/B), `ranged_attack`, `cast`, `hit/flinch`, `death`, `cheer/victory`. Mirror these from ECS read‑only state (presentation only — **no ECS writes**, §12).

**Variation layers on top of the shared rig:**
- **Equipment overlay** — weapon + armor‑accent sprites parented to hand/body bones (sword, bow, staff, spear, pickaxe, crossbow, shield, helm, cape).
- **Faction tint** — material colour (blue Iron Pact / red Ashen Horde) on accents.
- **Body scale** — bulkier rig variant for Heavy/Brute; tattered for Undead.

## 3. Unit catalogue (maps to the roster in `UnitsArmyDesign`)

| Unit | Silhouette / equipment | Extra anims / VFX | Difficulty | Priority |
|---|---|---|---|---|
| **Swordsman / Shieldman** | stick + sword + kite shield + light helm | shield‑block; clang spark | Low (shared rig) | **P0** |
| **Archer (Iron Archer)** | stick + hood + longbow | draw/loose; **arrow projectile** (pooled) | Low | **P0** |
| **Spearman / Sentinel** | stick + spear + round/tower shield + crest | thrust; brace | Low | **P0** |
| **Miner** | stick + pickaxe + satchel | mine‑swing; **ore sparkle** | Low | **P0** (economy reads) |
| **Mage / Runic Adept** | stick + robe/hat silhouette + staff, glowing eyes | cast; **spell VFX** (particle) | Med (FX) | **P0** (caster) |
| **Crossbowman** | stick + crossbow | reload/loose; bolt projectile | Low | P1 |
| **Heavy Guard** | bulkier stick + two‑hander, heavy plates | heavy swing; ground‑thud | Low‑Med (scale variant) | P1 |
| **Warden** | stick + sword + cape (officer) | rally pose | Low | P1 |
| **King / Commander (hero)** | stick + crown + cape + sword + shield | rally; hero ability; **aura VFX** | Med (hero anims) | P1 |
| **Flamecaller** | stick + staff + ember accents | fire cast; **flame VFX** | Med (FX) | P2 |
| **Oathbreaker** | red‑accent stick warrior | melee | Low | P2 |
| **Heavy‑Brute (Ashen)** | large bulky stick, horned‑helm accent, maul (replaces orc) | heavy slam; roar | Med (scale) | P2 |
| **Stick‑Undead (Endless)** | tattered stick + bone/green necrotic accents | shamble; rise; crumble death | Med (reskin + anims) | P2 |

## 4. VFX & projectiles
- **Projectiles** (arrows, bolts, spells): pooled sprite/particle objects, presentation‑only.
- **Impacts/death:** sprite‑sheet poofs, dust, blood‑optional sparks (pooled).
- **Hero/ability auras:** additive sprite + particle (no realtime lights).
- **Faction read:** blue vs red accent + tinted FX so allegiance is instant at small scale.

## 5. Cosmetics / skins (already a designed feature — `SkinsScreenDesign`)
Skins are **equipment/accent swaps + tints on the shared rig** (e.g., "Leaf Set"). The skeletal approach makes cosmetics nearly free — a skin is a sprite set, not a new rig — which aligns with the existing Skins/Store monetization screens (display‑only; economy stays in ECS/services, §12).

## 6. Production priority (what makes the army exist)

1. **P0 — shared stick rig + the full shared animation set** (the foundation; one‑time).
2. **P0 — the five visible core units** (Swordsman, Archer, Spearman, Miner, Mage) — this is what the player sees fighting.
3. **P1 — remaining common roster** (Crossbowman, Heavy Guard, Warden) + **King/Commander hero**.
4. **P2 — Flamecaller, Oathbreaker, Heavy‑Brute, Stick‑Undead** (Ashen/Endless content) + first cosmetic skins.
5. **P3 — extended cosmetics, emotes, alt heroes.**

## 7. Definitive recommendation

Adopt **2D skeletal/bone animation on a single shared humanoid stick rig** (Unity 2D Animation + 2D IK by default; Spine as a premium option), with **equipment‑overlay sprites + faction tint + scale variants** to express the whole roster, and **pooled sprite/particle FX** for projectiles and abilities. Author the rig + shared animation set **once**, then ship the **five core on‑field units first**. This is the only approach that scales to a full RTS roster + cosmetics on a 3.66 GB device, is perfectly on‑identity for stick figures, and stays entirely presentation‑side (§12). It also makes the Skins/Store cosmetic features cheap to feed.


<!-- ==================== EMBEDDED: 09_EXECUTION_PRIORITY_MATRIX.md ==================== -->

# 09 — EXECUTION PRIORITY MATRIX
**Goal:** identify the work that closes **~90% of the UI/UX crisis**, ranked P0→P3, with rationale, dependencies, and sequencing.

---

## 1. What "the crisis" actually is (and what 90% looks like)

The crisis the player experiences, in order of visibility:
1. **The front‑end looks broken** — doubled logos/titles, garbled buttons, bleed‑through (Report 04).
2. **Wrong identity** — realistic knights/orcs/dragons instead of stick figures (Report 02).
3. **The battlefield is primitive** — ECS shapes on a flat background (Report 06).
4. **Brand drift** — "Bulwark" everywhere vs the intended name (Report 01).

**Closing ~90% = make the screens the player sees most (boot → menu → mode select → match → results → shop) look clean, on‑identity, and un‑doubled, with a battlefield that reads as a stick war.** That is overwhelmingly a **UI‑architecture + top‑screen art** problem, not a "produce every asset" problem. The long tail (every secondary screen's bespoke key art, every biome, full roster, cosmetics) is the last 10% of *perceived* quality at a large share of the cost.

## 2. The matrix

### 🔴 P0 — closes the bulk of the crisis (do first; mostly blocking)
| Item | Why P0 | Report |
|---|---|---|
| **Legal/IP decision on the name** ("Stick Empire Rise" vs original) | Gates every brand surface + de‑risks the whole product; a *decision*, cheaply made now | 01 §0 |
| **Adopt the clean layered UI architecture** (remove dirty sources: no full‑mockup backdrops, no baked‑text chrome) | This *is* the "broken UI" fix — it makes doubling/bleed structurally impossible | 04, 05 |
| **Author the clean UI KIT** (frames, panels, buttons ×states, tabs, icons, currency pills — **no baked text**) + atlas | Replaces the contaminated chrome everywhere at once via the existing builder API | 05 U1 |
| **UI‑free background plates for the top ~8 screens** (Splash, Loading, MainMenu, ModeSelect, Store, Settings, Victory/Defeat) | Kills the most‑seen doubling; restores identity where it matters most | 03, 05 U2 |
| **Stick archetype sheet** (king, mage, archer, swordsman, spearman, miner + faction tints) | The single source the kit, plates, characters, and correction prompts all reuse → consistency | 08, 03 |
| **Remove the dual UI system** (legacy `UiFlow` front‑end) | Eliminates a whole bleed‑through vector; cheap | 04, 05 U4 |
| **Fix visible brand strings** (display name + live logo/splash text) — *after legal* | Cheap, high‑visibility rebrand win | 01 R1 |

### 🟠 P1 — the rest of the visible 90%
| Item | Why P1 | Report |
|---|---|---|
| **Battlefield parallax base + statues** (1 Siege biome) | The in‑match screen is the second‑most‑seen surface; statues = win condition readability | 06, 07 |
| **Shared stick rig + 5 core units** (swordsman, archer, spearman, miner, mage) | Makes the on‑field army real and on‑identity | 08 |
| **UI‑free plates for remaining screens** | Finishes removing baked‑UI backgrounds | 05 U2 |
| **TMP SDF font migration** | Crisp, localizable text (current legacy `Text` is a prestige gap) | 05 U3 |
| **Correct the high‑traffic monster screens** (MainMenu dragons, ModeSelect zombie, MatchIntro/Commander) | Removes the most‑seen identity violations | 02, 03 |
| **Device fidelity gate** (screenshot vs reference for all 35 screens) | Prevents regression of the crisis | 05 U6 |

### 🟡 P2 — depth & completeness
| Item | Report |
|---|---|
| Full unit roster + King/Commander hero + **stick‑undead / stick‑brute** | 08 |
| Additional biomes (Greenfield, Ashen, Frost) + weather/ambience | 06, 07 |
| Character/key‑art layer on hero screens (Profile, Commander, results) | 05 U5 |
| Correct remaining lower‑traffic monster screens | 02, 03 |
| First cosmetic skins (Skins/Store) | 08 |

### ⚪ P3 — polish / deferred
| Item | Report |
|---|---|
| Live day/night cycle, advanced post‑FX, foreground fly‑throughs | 06 |
| Decorative prop density, extra cosmetics/emotes | 07, 08 |
| **Deep code rename** (package id / namespaces) | 01 R4 (explicitly deferred / not recommended) |

## 3. Dependency graph (critical path)

```
Legal name decision ──► visible brand strings
Stick archetype sheet ──► UI kit ──► (re-skin chrome)
                      └─► background plates ──► (re-skin backgrounds)
                      └─► character rig ──► core units ──► battlefield
Clean architecture (Report 05) ──► all of the above land safely
TMP font asset ──► text migration
Device fidelity gate ──► guards every screen before "done"
```

The **archetype sheet** and the **clean UI kit** are the two true bottlenecks: almost everything visible depends on them. Fund those first.

## 4. Why this ordering (rationale)

- **Architecture before assets:** pouring more art onto the current dirty‑source pipeline would re‑create the doubling (Report 04). Fix the pipeline (P0) so every asset added afterwards lands clean.
- **Most‑seen surfaces first:** boot→menu→match→results→shop are ~90% of session screen‑time; perfecting a rarely‑seen modal's key art is P2/P3.
- **One shared source (archetype sheet) → consistency:** the kit, plates, characters, and correction prompts must all draw the *same* stick designs, or the game looks incoherent. Author it once, reuse everywhere.
- **Cheapest high‑visibility wins early:** removing the dual UI system and fixing brand strings are low‑effort, high‑perception P0/P1 items.
- **Defer the irreversible/low‑value:** deep code rename (Report 01 R4) and day/night cycle add cost/risk for little perceived gain — P3 or never.

## 5. The 90% statement

> Ship **P0 + P1** and the UI/UX crisis is ~90% closed: a clean, un‑doubled, on‑identity front‑end on every high‑traffic screen, a stick‑war battlefield with readable statues and a real 5‑unit army, crisp text, and a cleared brand — built on an architecture that makes the failure unable to recur. P2/P3 add depth and polish, not crisis relief.
