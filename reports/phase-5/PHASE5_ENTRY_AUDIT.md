# BULWARK — Phase 5 Entry Audit (PHASE5_ENTRY_AUDIT.md)

**Date:** 2026-06-05 · **Purpose:** mandatory pre-flight audit before any Phase-5 work, per the Phase-5
authorization. **Method:** re-read `BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` (§13 phase plan + gates), all ADRs
(`docs/adr/ADR-0-001..ADR-5-002`), Phase 0–4 reports, all pre-Phase-5 reports (V0–V2 + GATE1), the Asset
Migration reports, and the Presentation Pass reports. **Companion:** `ADR-5-003`, `PHASE5_PRE_FLIGHT_REPORT.md`.
**Binding rule honored:** no constraint waived; no fabricated PASS; evidence only.

---

## 0. Headline (read first)
**A material contradiction exists and is surfaced here (not waived).** The Phase-5 *objective stated in the
authorization* (character differentiation, battle HUD, audio/VFX/animation frameworks — i.e. **new presentation
features**) is **NOT** the roadmap's Phase 5. Roadmap **Phase 5 = "Soft Launch & Tuning (SCALE-OR-STOP)" —
"tuning only, NO new features,"** validated by **GATE 5 (real D1/D7 retention + blended-D30 LTV ≥ target CPI)**.
Roadmap Phase 5 **cannot legitimately execute here** (no live soft-launch population/telemetry — un-fabricatable;
GATE-5 LTV floor still un-set in ADR-5-002), and the project still carries **GATE 1 (FUN) = FAIL**. Per the
authorization's own rules ("do not waive any constraint," "no fabricated PASS," "stop at the first required gate
and report honestly"), this audit **stops at the entry gate** and asks the owner to resolve the contradiction
(see §13 Stop conditions + ADR-5-003).

## 1. Current project state
- ECS/DOTS RTS-lite, Unity 6 (URP, IL2CPP). **Phases 0–4 COMPLETE** (authored + CI compile/tests/Android APK
  GREEN). The full **mine→train→push→topple loop runs on device** (Redmi Note 11R).
- **Pre-Phase-5 GATE-1 track (V0–V2):** visualization (V0), playability (V1), GATE-1 validation (V2) → **GATE 1 =
  FAIL** (AI economy collapse / not-fun; re-gated after the AI-economy fix and still FAIL — `GATE1_VALIDATION_REPORT.md`).
- **Asset Migration + Presentation + Final Polish passes:** placeholder sprites (Stick War, dev-only), a uGUI
  flow (Splash→Menu→Mode Select→Match→Victory/Defeat), clean menus, battlefield background — device-validated
  (`reports/asset-migration/*`, `reports/prephase5/PREPHASE5_FINAL_POLISH_REPORT.md`). The app now **presents as a
  game**, but the underlying combat fun verdict is unchanged (FAIL).
- **Latest build:** `28eb1ff` CI-GREEN; APK ~39.3 MB.

## 2. Completed roadmap items
| Phase | Roadmap scope | Status | Gate |
|---|---|---|---|
| 0–1 | ECS spike; core combat (economy/train/target/combat/statue/AI, 2 factions) | COMPLETE | **GATE 1 (FUN) = FAIL** (was OPEN; re-gated FAIL) |
| 2 | Tactical depth (terrain, formations, counter matrix, positional, spells, commanders, squad AI) | COMPLETE | GATE 2 (playtest) DEFERRED |
| 3 | Meta-integration hooks (server-owned, cap-clamped upgrades) | COMPLETE (scaffold) | GATE 3 (server economy) DEFERRED |
| 4 | Monetization & live-ops **shell** (battle pass, chests, shop, cosmetics — fair) | COMPLETE | **GATE 4 (fairness) = PASS (static)** |
| 5 | **Soft Launch & Tuning (SCALE-OR-STOP)** | **NOT started (readiness scaffold only)** | GATE 5 DEFERRED / un-evaluable |

## 3. Completed pre-phase (presentation track) items
V0 Visualization ✅ · V1 Playability ✅ · V2 GATE-1 validation ✅ (verdict FAIL) · Asset Migration (discovery→
inventory→mapping→gap→architecture→risk→integration) ✅ · Presentation Pass ✅ · Final Polish ✅. All
presentation-layer, fairness-neutral, removable; **no gameplay/balance/AI/economy/canon change**.

## 4. Remaining Phase 5 scope (roadmap §13 Phase 5)
Per the roadmap (lines 337–344, 372): **5.1** limited-geo store **release** + live **telemetry**; **5.2** RC-tuned
retention/monetization **from real data**; **5.3** balance + perf/stability **hardening**; validated by **GATE 5**
(D1≥~40%, D7≥~18%, blended-D30 LTV ≥ target CPI). **"Tuning only — no new features."** **None of this is
producible in this environment** (no live release, no users, no telemetry; the earlier session delivered a
**readiness scaffold** only — `reports/phase-5/FINAL_REPORT.md` = "READINESS SCAFFOLD — Phase 5 NOT complete").

## 5. Technical debt inherited from previous phases
- `entities.graphics` deferred → sprites via a removable presentation MonoBehaviour (not ECS rendering).
- Squad→`FormationMember` pipeline unterminated → units move only via control-layer drivers (V1/V2 scaffolds).
- Debug scaffolds (`SimDebugOverlay`/`SimPlayerHud`/`SimProxyRenderer`/`SimAiDriver`/`UiFlow`/`PlaceholderAssets`)
  stand in for production render/UI/input; removable.
- No production UI/animation/audio/VFX; placeholder art is **© ripped, dev-only** (replace before any release).
- GATE 2/3 DEFERRED; no live BaaS; ADR-5-002 GATE-5 LTV floor still **PROPOSED (no values set)**.

## 6. Deferred GATE-1 issues (DOCUMENTED — remain deferred; NOT to be fixed unless the roadmap requires)
1. **BasicAI vs SquadAI conflict** — two AI training sources starve the AI economy.
2. **Miner targeting death** — `TargetingSystem` doesn't exclude `MinerTag`; miners walk into combat and die.
3. **Miner replacement** — no system maintains a live miner floor against attrition.
**Net: GATE 1 (FUN) = FAIL.** (Full detail: `reports/asset-migration/PROJECT_STATE_ANALYSIS.md` §10;
`reports/prephase5/GATE1_VALIDATION_REPORT.md`.) Per the roadmap, these block "fun" and thus the fun-gated
downstream phases.

## 7. Asset readiness
**Placeholder only.** 13 curated Stick War PNGs (units/miner/mine/statue/backgrounds/UI) in `StreamingAssets/`,
runtime-loaded; **© ripped, dev-only, must be replaced before release** (legal blocker). No production/licensed
art, no faction crests/branding, no per-archetype unit art (units share one team sprite). 2D-Spine-vs-2.5D-3D
ADR remains an unresolved human gate. **Readiness: prototype-placeholder, NOT production.**

## 8. UI readiness
A code-built **uGUI flow** exists (Splash/Menu/Mode Select/Match/Victory/Defeat) using placeholder sprites + the
built-in font; menus/end are clean (debug overlays gated to Match). **No textured battle HUD** (in-match control
is still IMGUI debug); no Profile/Settings; no 9-slice; no branded UI; no TMP/licensed font. **Readiness:
functional placeholder front-end, NOT production UI.**

## 9. Animation readiness
**None.** All battlefield visuals are **static sprites**; no skeletal/flipbook animation. Spine rigs exist in the
extracted sets but are **deferred** (paid `spine-unity` runtime; version-split 4.1/3.8/2.x; integration not done).
**Readiness: zero (static only).**

## 10. Audio readiness
**None integrated.** No `AudioSource`, no music/SFX in the build. Curated audio exists in the extracted sets
(© ripped, dev-only) but is **not imported** (size/curation deferred). **Readiness: zero.**

## 11. Risks
- **Mislabeling risk (high):** doing presentation/feature work under the "Phase 5" label would (a) falsely imply
  roadmap-Phase-5 (soft-launch/scale) progress, and (b) violate roadmap Phase 5's "**no new features**" rule.
- **Constraint-waiver risk (high):** proceeding into meta/scale phases while **GATE 1 (FUN) = FAIL** violates the
  roadmap's binding "combat must be fun before meta is built" + GATE 1 "if not → kill/pivot."
- **Fabrication risk (high):** GATE 5 / soft-launch metrics cannot be produced here; any "PASS" would be fabricated
  (explicitly forbidden).
- **IP/legal risk (high):** placeholder art is ripped; not shippable.
- **Scaffold-debt risk (medium):** the presentation stack is debug scaffolding, not production systems.

## 12. Contradictions (REQUIRED — surfaced, not waived)
1. **Objective vs roadmap Phase 5:** stated objective = NEW presentation features; roadmap Phase 5 = "tuning only,
   no new features." Direct contradiction. **The requested work is NOT roadmap Phase 5.**
2. **Executability:** roadmap Phase 5 + GATE 5 require a real soft launch + live D1/D7/D30/LTV — **un-producible /
   un-fabricatable** here. (Already concluded: `PHASE5_CONFLICT_REPORT.md`, `reports/phase-5/FINAL_REPORT.md`.)
3. **GATE 5 floor unset:** ADR-5-002 (LTV floor) is **PROPOSED, no values** → GATE 5 is un-evaluable by
   definition until the owner/LP set it.
4. **GATE 1 unresolved:** GATE 1 (FUN) = **FAIL**; the roadmap fun-gates downstream meta/scale phases. Building
   further on un-fun combat contradicts the roadmap (and "do not waive any constraint").

## 13. Stop conditions (this audit triggers one)
The authorization says **"Stop at the first required gate and report honestly"** and **"do not waive any
constraint."** The **first required gate is the Phase-5 ENTRY decision**, and it is **blocked by the contradictions
in §12** (objective ≠ roadmap Phase 5; roadmap Phase 5 un-executable/un-fabricatable; GATE 1 FAIL unwaived).
**Therefore: STOP at the entry gate.** Resolution requires an **owner decision** (see `ADR-5-003` + the conflict
prompt): re-scope the requested work as a **Production-Presentation phase** (distinct from roadmap Phase 5,
fairness-neutral, executable) **and/or** address **GATE 1 (FUN)** first, **and/or** accept that roadmap Phase 5
(soft launch) remains **telemetry-blocked (readiness only)**. **No constraint is waived; no PASS is fabricated.**
