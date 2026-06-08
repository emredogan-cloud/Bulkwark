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
