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
