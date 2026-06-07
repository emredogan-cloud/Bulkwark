# BULWARK — UI CONSTRUCTION SPEC · 39 · Network Error (full-screen Connection Lost)
Source: design/NetworkErrorDesign.png · 1536×1024 (≈1.5:1) · Analysis-only forensic spec.

> Normalize to 2340×1080. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). This is the FULL-SCREEN connection-lost overlay: a centered gold-frame panel on a dim blocking scrim over the (frozen, dimmed) current screen — distinct from the compact Connection-Lost modal in spec 37 ④. FRACTION-BASED sizing; px quoted at 1080-tall height.

---

## A. SCREEN PURPOSE
The **full-screen "CONNECTION LOST"** overlay shown on a hard network failure (lost socket, failed required fetch, server unreachable). It (a) blocks all interaction with the dimmed screen beneath, (b) explains the failure and lists **possible causes** (unstable internet / weak signal / server temporarily unavailable), and (c) offers two recoveries: **RETRY** (re-attempt the failed operation / reconnect) and **MAIN MENU** (bail out to the hub). The background visibly shows a dimmed Main-Menu-like screen (left rail Quests/Heroes/Armory/Battle Pass/Store, right Events/Arena/Battle, top currencies) — confirming this floats over the live screen rather than replacing it. Server/connection state only; mutates nothing.

## B. VISUAL DNA (inherits GLOBAL DNA)
- Dark heroic high-fantasy; a **large near-black panel** (#0c0e14→#14161e) inside a **brushed gold/antique bronze ornate frame** with corner filigree and a **blue/gold gem finial** centered on the top edge.
- Behind it, the **dimmed previous screen** (Main-Menu chrome) is visible through/around the scrim — strongly darkened, desaturated, unblurred-to-lightly-blurred.
- **Serif gold-bevel UPPERCASE title** "CONNECTION LOST" (warm gold, not amber-warning here — larger/heroic, this is the dedicated full-screen treatment).
- A **distressed metal shield** with **wifi bars + a red crack/lightning bolt** is the hero glyph (left of the body text) — signals a broken connection. **Ember/oxblood red** = the crack accent. **Royal/cobalt blue** = the primary **RETRY** CTA (brightest). **MAIN MENU** is a gold/dark outline secondary.
- "Possible causes" is a small bullet list with line-icons (wifi / signal-bars / globe). Low-key field → focal panel + shield; gold rim-light on frame.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
NetworkErrorOverlay (UiScreen overlay root, CanvasGroup) — pushed ON TOP of current screen
└─ Scrim (Image, full-screen, semi-transparent black, raycastTarget=TRUE ← blocks input)
   ├─ (the previous screen renders beneath, dimmed)
   └─ ErrorPanel (Image, ornate gold frame, centered, landscape rectangle)
      ├─ TopGemFinial (Image — blue/gold gem on top edge)
      ├─ CornerFlourish_TL / TR / BL / BR (Image x4)
      ├─ Title_CONNECTION_LOST ("CONNECTION LOST", serif gold)
      ├─ LeftCluster
      │  └─ ShieldGlyph (Image — metal shield + wifi bars + red crack)
      ├─ RightCluster
      │  ├─ Body_Msg (Text "The connection to the server was lost. Please check your network and try again.")
      │  ├─ Lbl_PossibleCauses ("Possible causes:")
      │  └─ CausesList (VerticalLayoutGroup)
      │     ├─ Cause_Wifi   (Icon_Wifi   + "Unstable internet connection")
      │     ├─ Cause_Signal (Icon_Bars   + "Network signal is weak")
      │     └─ Cause_Server (Icon_Globe  + "Server temporarily unavailable")
      └─ ButtonRow
         ├─ Btn_RETRY (blue, primary; refresh icon + "RETRY")
         └─ Btn_MAIN_MENU (gold/dark outline, secondary; home icon + "MAIN MENU")
```

## D. UNITY HIERARCHY SPEC (per node)
- **NetworkErrorOverlay** — parent: UiRouter canvas, pushed as an OVERLAY above the current screen (does NOT replace it; the dimmed screen stays rendered beneath). Empty `RectTransform` + `CanvasGroup`. Stretch-all. **Highest sorting order** (it must sit above every other overlay too, since connection loss is global).
- **Scrim** — parent overlay. Anchor stretch-all (full-bleed, ignores safe area → dims notch). `Image` solid black @ ~60% alpha. **`raycastTarget=true`** → blocks all input to the screen beneath. Scrim tap ignored (must press a button) — `cancelOnScrim=false`.
- **ErrorPanel** — parent Scrim. Anchor center (0.5,0.5) pivot 0.5,0.5. `Image` 9-slice ornate gold frame; **landscape rectangle** (wider than tall, see E). Center clamped inside safe area. `raycastTarget=true`.
- **TopGemFinial** — parent ErrorPanel, anchor top-center (0.5,1) pivot 0.5,0.5 (overhangs). `Image`, raycast off.
- **CornerFlourish_TL/TR/BL/BR** — parent ErrorPanel, anchored to each corner, raycast off.
- **Title_CONNECTION_LOST** — `Text` serif gold, anchor top-center pivot 0.5,1, alignment center, near panel top under finial.
- **LeftCluster** — parent ErrorPanel, anchor center-left, pivot 0,0.5. Holds the ShieldGlyph (the hero art, left half of the body region).
  - **ShieldGlyph** — `Image` distressed metal shield with wifi arcs + a red crack/lightning; `preserveAspect`; focal glow.
- **RightCluster** — parent ErrorPanel, anchor center-right, pivot 1,0.5. `VerticalLayoutGroup` left-aligned: Body_Msg, Lbl_PossibleCauses, CausesList.
  - **Body_Msg** — `Text`, left-aligned, wrapping (2 lines).
  - **Lbl_PossibleCauses** — `Text` small label (gold), with a thin flanking gold rule as in source.
  - **CausesList** — `VerticalLayoutGroup` (spacing ~8); each **Cause_X** = `HorizontalLayoutGroup`: line-icon `Image` + `Text`.
- **ButtonRow** — parent ErrorPanel, anchor bottom-center pivot 0.5,0. `HorizontalLayoutGroup` (spacing ~28): Btn_RETRY then Btn_MAIN_MENU (a small ornamental separator dot may sit between, decorative).
  - **Btn_RETRY (primary blue)** — `Button` cobalt gradient + refresh `Icon` + label "RETRY"; brightest; min height 72 px.
  - **Btn_MAIN_MENU (secondary)** — `Button` dark/gold outline + home `Icon` + label "MAIN MENU"; same height.
- **Responsive:** panel centered & size-clamped — on ultrawide it stays fixed size (do NOT stretch); on small/tall screens clamp ≤92% width / ≤80% height. Left/right clusters keep their split; on very narrow (rare in landscape) stack shield above text as a fallback. Scrim full-bleed; the dimmed underlying screen + panel center stay inside safe area.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim:** full 2340×1080, alpha ~0.60 (the underlying Main-Menu chrome shows through, darkened).
- **ErrorPanel:** ≈ 0.50 W × 0.58 H (~1170×626 px), **landscape rectangle**, centered. Frame border ~26 px 9-slice. Gem finial overhang ~30 px above top.
- **Title_CONNECTION_LOST:** baseline ~0.16 from panel top, centered.
- **Body region split (below title, ~0.30–0.72 of panel height):**
  - **LeftCluster / ShieldGlyph:** left ~0.40 of panel width; shield Ø/height ~0.34 of panel height (~210 px), vertically centered in the body region.
  - **RightCluster:** right ~0.55 of panel width (x≈0.42–0.97). Body_Msg (2 lines) at top; "Possible causes:" label below with flanking rule; CausesList of 3 rows, each ~32 px tall, icon Ø ~26 px + text.
- **ButtonRow:** centered ~0.86 from panel top; two buttons each ≈ 0.40 of panel width × 72 px, ~28 px gap; RETRY left (blue), MAIN MENU right (outline). Optional center separator dot.
- **Tablet 4:3 / ultrawide:** fixed panel size, re-centered, never stretched (the underlying screen reflows on its own). **Notch:** clamp panel center within safe area; scrim full-bleed.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for title; Roboto-Medium SDF for body/causes/buttons. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "CONNECTION LOST" | heroic prestige serif | Black | UPPER | +5% | 1.0 | gold bevel + bloom + dark stroke + drop-shadow | ~64 | #f0d27a / stroke #3a2c0e |
| Body "The connection to the server was lost. Please check your network and try again." | body | Regular | Sentence | +1% | 1.25 | none | ~28 | #d9d2c2 |
| "Possible causes:" | label | Medium | Sentence | +2% | 1.0 | gold, flanked by thin rule | ~24 | #cdb474 |
| Cause items (Unstable internet connection / Network signal is weak / Server temporarily unavailable) | list | Regular | Sentence | 0 | 1.15 | none (line-icon left) | ~24 | #c9bfa6 |
| RETRY label | primary CTA | Bold | UPPER | +4% | 1.0 | white on blue + shadow | ~32 | #ffffff |
| MAIN MENU label | secondary CTA | Bold | UPPER | +3% | 1.0 | gold/cream on outline + shadow | ~30 | #e9dcc0 |

## G. MATERIALS
- **Frame:** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin, worn edges, engraved corner filigree; strong gold rim-light; **TopGemFinial** = faceted blue/gold gem with bloom.
- **Panel fill:** obsidian #0c0e14→#14161e gradient, low reflectivity, soft inner shadow at the frame; faint vignette.
- **Underlying screen (through scrim):** the live screen (Main-Menu chrome here) rendered then darkened ~60% + slightly desaturated; optional light blur. It is NON-interactive while this overlay is up.
- **ShieldGlyph:** distressed/worn **steel shield** (#cdd3da hi, #8a929c mid, #3a3f47 sh) bearing **wifi arcs**; a jagged **red crack / lightning bolt** (#d8452b→#7a1f1a) splits it with a faint danger glow + small embers at the fracture; subtle rust at the edges.
- **Cause line-icons:** thin gold/cream line-art — wifi waves, signal bars, globe; muted (#cdb474).
- **Btn_RETRY:** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft outer glow (brightest); refresh/circular-arrow icon in white.
- **Btn_MAIN_MENU:** dark stone capsule with a gold beveled rim + gold/cream label + home icon; no glow (secondary).

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **Scrim:** static input-blocker over the dimmed screen; tap ignored (`cancelOnScrim=false`) — must choose RETRY or MAIN MENU. Fades with the overlay.
- **Btn_RETRY (primary blue):** idle cobalt gloss + steady glow; hover brighter + glow grows (pointer); pressed darken + scale 0.96 + inset; **in-flight** = disabled + the refresh icon spins + small spinner (while re-attempting). On retry success → dismiss + resume; on failure → re-enable, keep the panel (optionally update body to reflect repeated failure). ≥88px touch; retry SFX.
- **Btn_MAIN_MENU (secondary outline):** idle dark/gold outline; hover rim brightens + faint fill; pressed scale 0.96; routes to the hub (tears down the failed flow). Soft "back" SFX.
- **CausesList / ShieldGlyph:** non-interactive display.
- **Feedback:** primary RETRY always visually dominant over MAIN MENU; both ≥88px touch; pressed ≤80 ms scale.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms Scrim α 0→0.60 over 150 ms (ease-out), underlying screen dims/desaturates with it. 70 ms ErrorPanel scale 0.92→1.0 + α 0→1 over 220 ms (ease-out-back, overshoot ≤4%). 150 ms ShieldGlyph entry: scale 0.85→1.0 + α over 200 ms, then a **single shake** (±5px horizontal, 200 ms) and the red crack glow flares once. 200 ms Title "CONNECTION LOST" pop (scale 0.9→1.0 + bloom, 180 ms). 260 ms RightCluster text fades/raises in (+12px→0, 180 ms); causes stagger 40 ms apart. 360 ms ButtonRow fades/raises in (+16px→0, 180 ms); RETRY begins its glow pulse.
- **Retry pressed:** RETRY scale 0.96 (80 ms) → disabled + icon spin (continuous ~0.8 s/rev) while the network call runs; on success → panel scale 1→0.96 + α 1→0 over 160 ms (ease-in) + scrim α →0 (180 ms), underlying screen restores, pop overlay; on failure → stop spin, re-enable, brief red flash on the shield crack (160 ms).
- **Main Menu pressed:** MAIN MENU scale 0.96 → panel/scrim fade out (160/180 ms) → route to hub.
- **Idle loops:** RETRY rim glow (±10%, 1.6 s); faint ember flicker at the shield crack (subtle). 
- **Easing:** ease-out-back entries, ease-in exits.

## J. PARTICLE & FX
- **ShieldGlyph crack:** faint red danger glow + a few small embers/sparks at the fracture (very low rate) + the one-shot flare on show and on retry-failure. No looping violent jitter — one entry shake only.
- **TopGemFinial / corner flourishes:** one-shot glint on show + steady faint bloom.
- **Btn_RETRY:** pulsing cobalt rim glow; spinning refresh icon while retrying. Keep FX restrained — this is a reassuring recovery screen, not an alarm strobe.

## K. EVENT BEHAVIOR
- **OnShow(context):** triggered by the network layer on a hard failure; capture the failed operation/route so RETRY can re-attempt it; render; block input globally.
- **OnRetry:** disable + spin → re-attempt the captured operation (reconnect socket / re-fire the failed request); on success → dismiss + resume the original flow; on failure → re-enable + keep the panel (optionally increment a retry counter / adjust copy); never fabricate success.
- **OnMainMenu:** abandon the failed flow → route to the Main-Menu hub (clean teardown of any in-progress match/meta call); dismiss the overlay.
- **OnBackKey:** map to MAIN MENU (safe bail) — never silently dismiss without resolving.
- **Auto-recover (optional):** if the network layer reconnects on its own while the panel is up, auto-invoke the success path (dismiss + resume) so the player isn't stranded.
- **Relationship to spec 37④:** use THIS full-screen variant for hard/global connection loss with two recoveries; use the compact 37④ modal for a single-RETRY in-context failure. Don't show both at once.

## L. NEGATIVE RULES
- Do NOT replace the underlying screen — it stays rendered (dimmed) beneath; this is an overlay so RETRY can resume exactly where the player was.
- Do NOT allow scrim-tap dismissal or any close without RETRY/MAIN MENU (BackKey → Main Menu).
- Do NOT fake a successful reconnect; RETRY must actually re-attempt and only dismiss on real success.
- Do NOT mutate any game/server state from this screen; it is recovery chrome only.
- Do NOT stretch the panel/shield on ultrawide; fixed size, re-centered; center inside safe area; scrim full-bleed.
- Do NOT make MAIN MENU brighter than RETRY (primary must dominate).
- Do NOT add real brand text/stick figures; keep palette within DNA (red crack accent, cobalt RETRY).
- Do NOT over-animate the shield (one entry shake + subtle ember flicker only — reassuring, not alarming).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel serif title well — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Full-screen ~60% blocking scrim over the dimmed/desaturated previous screen (Main-Menu chrome visible beneath); centered landscape gold-frame panel with top gem finial + four corner flourishes.
2. Title "CONNECTION LOST" (large serif gold) centered near the top.
3. Left: distressed metal shield with wifi bars + red crack/lightning (hero glyph) with focal glow.
4. Right: body "The connection to the server was lost. Please check your network and try again." + "Possible causes:" + three line-icon causes — "Unstable internet connection", "Network signal is weak", "Server temporarily unavailable".
5. Bottom: RETRY (blue primary, refresh icon, brightest) + MAIN MENU (gold/dark outline secondary, home icon); RETRY spins/disables in-flight and only dismisses on real reconnect; MAIN MENU routes to hub.
6. No scrim-tap close; back key = Main Menu; nothing mutated.
7. Colors within DNA hex ranges; panel sized ~0.50W×0.58H centered & clamped; safe-area; scrim full-bleed; animations per Section I.

## N. IMPLEMENTATION CONFIDENCE
**94/100.** High: layout (shield-left / text-right / two buttons), copy, causes, and recovery flow are unambiguous; the overlay-over-dimmed-screen pattern is clear from the visible Main-Menu chrome. Risks: bespoke cracked-shield/wifi + cause line-icon + frame artwork (-3); gold-bevel serif needs TMP SDF (-2); robust "re-attempt the exact failed operation" wiring is engineering beyond the visual (-1).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Full-screen overlay = blocking scrim Image[raycast] over the dimmed live screen + centered panel (distinguished from compact 37④).
- [x] Fraction-based sizing → 2340×1080; centered & clamped; safe-area; scrim full-bleed.
- [x] Exact strings recorded (CONNECTION LOST, body, Possible causes + 3 items, RETRY, MAIN MENU).
- [x] Typography + hex; materials (cracked shield) with hex/finish; states (RETRY in-flight spinner); animation; FX; events (OnRetry/OnMainMenu/OnBack/auto-recover); negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.
