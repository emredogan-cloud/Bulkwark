# ART BLOCKERS — what stands between "structurally clean" and "commercially shippable"
**Phase 8 fidelity certification — art‑dependency register.** Per the §9/§10 honesty rules: the gaps below are **ART‑PRODUCTION dependent**, not code‑solvable. They are the reason most screens are classified **VISUAL QUALITY BLOCKED BY ART PASS** rather than CERTIFIED. The *implementation* (architecture, layering, cleanliness, §12 isolation) is production‑grade; the *assets* are placeholder‑grade.

---

## The core honest statement

Every visual asset shipped so far is **code‑generated placeholder art** (procedural `UiTex` + PIL silhouettes), produced because this environment has **no image‑generation capability and no editor art pipeline**. They are clean, on‑identity, and structurally correct — but they are **not** the painted "premium mobile RTS" benchmark the certification compares against. A clean gradient is not a matte painting; a PIL stick figure is not authored character art.

## Art blocker register

| # | Asset class | Current (placeholder) | What "premium" requires | Screens affected | Severity to certification |
|---|---|---|---|---|---|
| A1 | **Background plates (L0)** | procedural gradients + silhouette + glow + vignette (`plate_*`) | **authored matte‑painting backdrops** per screen (the `/design` mockups' painted look, UI‑free) | ALL screens | **ART PASS REQUIRED** — caps hero‑screen fidelity ~10–15 pts |
| A2 | **Character art (L1)** | PIL bone‑rig stick figures + silhouette equipment (`cp_*`/`ce_*`) | **authored stick‑hero/unit art** (or SpriteSkin mesh‑deform on the rig) with shading/detail | Splash, MainMenu, ModeSelect, Profile, Commander, results, **battlefield units** | **ART PASS REQUIRED** — the single biggest gap |
| A3 | **Battlefield biome art** | PIL parallax layers (`bf_*`) — flat gradients/silhouettes | **authored painted biome layers** (sky/horizon/mid/ground/fg) | Battle HUD / in‑match | **ART PASS REQUIRED** |
| A4 | **Environment props** | PIL silhouettes (`env_*`) — barracks/banner/scatter | **authored prop art** with material/lighting | in‑match environment | ART PASS REQUIRED (lower priority) |
| A5 | **UI kit ornaments (L2)** | **extracted + cleaned from the ripped mockups** (`kit_*`) — actually decent gold frames/buttons | **authored, licence‑clean** ornate frames/buttons (the current ones are derived from ripped IP — Report 01 §0) | ALL screens | **ART PASS REQUIRED + IP RE‑ORIGINATION** |
| A6 | **Icon set** | minimal (coin/gem) + procedural diamonds/discs | full **authored icon set** (currencies, crests, league emblems, chest/quest/clan icons, leading‑action icons) | Store, Leaderboard, Quests, Chests, HUD, rail | ART PASS REQUIRED |
| A7 | **SDF typography** | styled legacy `Text` (gold gradient + outline, bevel emulation) | **TMP SDF font asset** (Editor Font Asset Creator) + **licensed serif/display TTF** + gold‑bevel/glow material presets | ALL screens (all text) | **TYPOGRAPHY CERTIFICATION PENDING** (Editor‑gated) — caps typography ~4–6 pts/screen |
| A8 | **Hero/key art & logos** | live `Text` wordmark "BULWARK" + procedural frames | **authored logo/wordmark** + hero key art per screen | Splash, MainMenu, Store BP, results | ART PASS REQUIRED + brand/IP clearance (Report 01 §0) |
| A9 | **FX/particles** | procedural discs/diamonds (adequate) | authored sprite‑sheet FX for premium polish | results, statue destruction, casts | ART PASS (lowest priority — current FX are acceptable) |

## Why this is not code‑solvable

Per §9, the path from the current ~75–88 fidelity to ≥95 runs almost entirely through **new authored textures/characters/icons/fonts** (A1–A8). Additional code polishing (spacing/alignment micro‑tweaks) yields **<2 fidelity points** (the plateau definition, §8) because the eye reads "placeholder art," not "misaligned by 4px." Continuing to code‑polish would be **fabricating fidelity gains** — explicitly prohibited.

## Recommended art‑production order (to lift certification)

1. **A7 SDF typography** (Editor pass — fastest, lifts *every* screen's typography category).
2. **A5/A8 licence‑clean UI kit + logo** (resolves the IP exposure from Report 01 §0 *and* lifts material consistency on every screen).
3. **A1 background plates** for the top ~8 screens (biggest hero‑screen uplift).
4. **A2 character art** (Splash/MainMenu heroes + the 5 battlefield units) — the largest single quality jump.
5. **A6 icons**, then **A3 biome art**, then **A4 props / A9 FX**.

## Process note

This is exactly the outcome the Phase‑8 honesty rules anticipate: the engineering foundation is done and validated; **commercial visual readiness is gated on an art‑production pass**, not on more code. Reports 03 (image‑correction prompts) and 08 (CHARACTER_BIBLE) already specify the art to produce; an art team or an image‑generation step fulfils these slots, and the existing code places them with no rewrite.
