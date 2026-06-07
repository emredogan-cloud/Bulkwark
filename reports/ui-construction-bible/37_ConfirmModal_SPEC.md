# BULWARK — UI CONSTRUCTION SPEC · 37 · Confirm / Toast / Insufficient-Gems / Connection-Lost (4-in-1 modal sheet)
Source: design/ConfirmModalDesign.png · 1536×1024 (≈1.5:1) · Analysis-only forensic spec.

> **This source is a COMPONENT SHEET** — a 2×2 grid presenting FOUR reusable utility components, each with a numbered label: ①Confirm Modal, ②Toast Notification, ③Insufficient Gems Modal, ④Connection Lost Modal. The grid is a documentation layout ONLY; in production each component is an **independent overlay** that floats over the current screen. This spec specifies **all four** sub-components.
> Normalize to 2340×1080. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). All modals are CENTERED over a full-screen dim scrim that BLOCKS input to the screen beneath. FRACTION-BASED sizing; px quoted at 1080-tall height.

---

## A. SCREEN PURPOSE
A **reusable utility-overlay kit** used across the whole game:
- **①Confirm Modal** — generic destructive/spend confirmation ("Spend 150 gems?") with **CONFIRM / CANCEL**. Used by spend actions, Leave Clan, Logout, Reset Settings, surrender, etc.
- **②Toast Notification** — transient non-blocking success/info pill ("Equipped!") that auto-dismisses; does NOT block input.
- **③Insufficient Gems Modal** — blocks a purchase the player can't afford; states the shortfall ("You need 250 more gems…"), shows the deficit amount, and routes to **BUY MORE**.
- **④Connection Lost Modal** — compact network-failure overlay ("Unable to connect to the server…") with a single **RETRY**. (The full-screen connection-lost variant is spec 39; this is the compact modal form.)
All are **server/stub-aware** chrome; they never mutate balances themselves.

## B. VISUAL DNA (inherits GLOBAL DNA)
- Dark heroic high-fantasy; each modal is a **near-black panel** (#0c0e14→#14161e) inside a **brushed gold/antique bronze ornate frame** with **corner filigree flourishes** and a small **gold/blue gem finial** centered on the top edge.
- **Serif gold-bevel UPPERCASE titles**; the **primary CTA is royal/cobalt blue** and brightest; CANCEL is a ghost/outline secondary.
- Insufficient/Connection modals add a small **crest/shield emblem** above the title (amber/bronze) and an **amber-gold title** (warning tone) rather than pure gold.
- Toast is the odd one out: a **horizontal rounded bar** with a **green success glow** (#3fd07a) and a green check medallion — no scrim, no buttons.
- **Violet/amethyst** = the gem icon in ③. Low-key; focal glow on the CTA and the gem finial; gentle vignette implied by the scrim.

## C. SCREEN DECOMPOSITION (ASCII node trees — one sub-tree per component)
```
=== Shared overlay wrapper (used by ①③④; NOT ②) ===
ModalOverlay (UiScreen overlay root, CanvasGroup) — pushed ON TOP of current screen
└─ Scrim (Image, full-screen, semi-transparent black, raycastTarget=TRUE  ← blocks input)
   └─ ModalPanel (Image, ornate gold frame, centered)
      ├─ TopGemFinial (Image — gold/blue gem on top edge)
      ├─ CornerFlourish_TL / TR / BL / BR (Image x4)
      └─ <component-specific content>

=== ① Confirm Modal ===
ModalOverlay
└─ Scrim
   └─ ModalPanel (Confirm)
      ├─ TopGemFinial · CornerFlourish x4
      ├─ Title_CONFIRM (Text "CONFIRM")
      ├─ Body_Prompt (Text "Spend 150 [gem] gems?")   ← inline violet gem glyph between "150" and "gems"
      └─ ButtonRow
         ├─ Btn_CONFIRM (blue, primary)
         └─ Btn_CANCEL (outline, secondary)

=== ② Toast Notification (NO scrim, non-blocking) ===
ToastRoot (overlay, CanvasGroup, raycastTarget=FALSE on root)
└─ ToastBar (Image — rounded gold-rimmed pill, green glow)
   ├─ Icon_CheckMedallion (green ✓ circle)
   └─ Toast_Text (Text "Equipped!")

=== ③ Insufficient Gems Modal ===
ModalOverlay
└─ Scrim
   └─ ModalPanel (InsufficientGems)
      ├─ TopGemFinial · CornerFlourish x4
      ├─ Crest_Shield (amber/bronze shield emblem, top-center)
      ├─ Title_INSUFFICIENT_GEMS (Text "INSUFFICIENT GEMS", amber-gold)
      ├─ Body_Need (Text "You need 250 more gems to complete this purchase.")
      ├─ DeficitCluster
      │  ├─ Icon_Gem (large violet gem)
      │  └─ Val_Deficit (Text "100")
      ├─ Body_Hint (Text "Purchase more gems to continue.")
      └─ Btn_BUY_MORE (blue, primary)

=== ④ Connection Lost Modal (compact) ===
ModalOverlay
└─ Scrim
   └─ ModalPanel (ConnectionLost)
      ├─ TopGemFinial · CornerFlourish x4
      ├─ Crest_Shield (amber/bronze shield emblem, top-center)
      ├─ Title_CONNECTION_LOST (Text "CONNECTION LOST", amber-gold)
      ├─ Body_Msg (Text "Unable to connect to the server.\nPlease check your connection and try again.")
      ├─ Icon_WifiError (wifi bars + red ✕)
      └─ Btn_RETRY (blue, primary)
```

## D. UNITY HIERARCHY SPEC (per node)
**Shared (①③④):**
- **ModalOverlay** — parent: UiRouter canvas, pushed as an OVERLAY above the current screen (does NOT replace it). Empty `RectTransform` + `CanvasGroup`. Stretch-all. High sorting order.
- **Scrim** — parent ModalOverlay. Anchor stretch-all (full-bleed, ignores safe area so it dims the notch too). `Image` solid black @ ~55–65% alpha. **`raycastTarget = true`** → blocks all input to the screen beneath. Tapping the scrim = Cancel/Dismiss for ① (configurable; ③④ may require explicit button).
- **ModalPanel** — parent Scrim. Anchor center (0.5,0.5) pivot 0.5,0.5. `Image` 9-slice ornate gold frame; sized per E. Sits inside safe area (clamp center so the panel never hides under a notch). `raycastTarget=true`.
- **TopGemFinial** — parent ModalPanel. Anchor top-center (0.5,1) pivot 0.5,0.5 (overhangs the top edge). `Image`, raycast off.
- **CornerFlourish_TL/TR/BL/BR** — parent ModalPanel, anchored to each corner (0,1)/(1,1)/(0,0)/(1,0) with matching pivots. `Image`, raycast off.
- **Title_X** — `Text` serif, top-center under finial/crest, alignment center.
- **Body_X** — `Text`, center-aligned, wrapping, under title.
- **ButtonRow** (① only) — `HorizontalLayoutGroup` (spacing ~24), bottom-center of panel; two equal buttons.
- **Btn_CONFIRM / Btn_BUY_MORE / Btn_RETRY (primary blue)** — `Button` + label `Text`; cobalt gradient; min height 64 px; the brightest element.
- **Btn_CANCEL (secondary)** — `Button` + label; transparent fill with gold/grey outline; same height as Confirm.

**② Toast (independent, non-blocking):**
- **ToastRoot** — parent UiRouter canvas (overlay layer, above content but a toast can coexist with input). `CanvasGroup`; **root `raycastTarget=false`** so it never blocks. Anchor: top-center or above-center band (see E). Pivot 0.5,1 (slides from top) — or bottom-center per app convention; source shows a free-floating pill (use top-center default).
- **ToastBar** — `Image` rounded pill (gold rim, dark fill, green glow `Image` behind). `HorizontalLayoutGroup`: Icon_CheckMedallion + Toast_Text.
- **Icon_CheckMedallion** — `Image` green circle + check.
- **Toast_Text** — `Text`, left-aligned after the icon.

**Component-specific:**
- **Crest_Shield (③④)** — `Image` amber/bronze shield, anchor top-center, pivot 0.5,1 (above title), raycast off.
- **DeficitCluster (③)** — `HorizontalLayoutGroup` center: large gem `Image` + deficit `Text` "100".
- **Icon_WifiError (④)** — `Image` (wifi arcs + red ✕), centered between body and button.
- **Inline gem glyph (①)** — in "Spend 150 gems?", the violet gem appears between the number and word; implement via a TMP inline sprite or a small `Image` laid into the text flow (sprite asset in the TMP sprite sheet).

**Responsive:** modals are centered & size-clamped — on ultrawide they stay the same size (do NOT stretch); on small screens clamp to ≤90% width / ≤85% height. Scrim always full-bleed. Toast width hugs its content (min/max clamp). All panels keep their center inside safe area.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim:** full 2340×1080 (bleeds under cutout), alpha ~0.58.
- **① Confirm panel:** ≈ 0.30 W × 0.40 H (~700×432 px), centered. Frame border ~24 px. Title baseline ~0.20 from panel top; body ~0.45; ButtonRow centered ~0.78 from top. Each button ≈ 0.42 of panel width × 64 px; gap ~24 px. Gem finial overhang ~28 px above top edge.
- **② Toast bar:** height ≈ 80 px; width hugs content, clamp ~360–640 px (~0.15–0.27 W). Check medallion Ø ~48 px left; text padding ~20 px. Default anchor: top-center, ~0.12 from top (slides down to ~0.10). Green glow halo extends ~16 px beyond the bar.
- **③ Insufficient Gems panel:** ≈ 0.33 W × 0.52 H (~770×562 px), centered (taller). Crest shield Ø ~80 px overlapping top frame. Title ~0.20 from top; Body_Need (2 lines) ~0.36; DeficitCluster centered ~0.54 (gem Ø ~96 px + value); Body_Hint ~0.70; Btn_BUY_MORE ~0.85, ≈ 0.55 W × 64 px.
- **④ Connection Lost panel:** ≈ 0.33 W × 0.50 H (~770×540 px), centered. Crest shield top. Title ~0.20; Body_Msg (2 lines) ~0.38; Icon_WifiError centered ~0.58 (Ø ~96 px); Btn_RETRY ~0.84, ≈ 0.48 W × 64 px.
- **Documentation-grid note:** in the SOURCE sheet the four sit in a 2×2 at roughly: ① top-left, ② top-right, ③ bottom-left, ④ bottom-right, each ~0.40 W of the sheet with the numbered caption above-left. Reproduce sizes/contents, NOT the grid placement, in production (each is centered & solo).
- **Tablet/ultrawide:** fixed panel sizes, re-centered; never stretch. **Notch:** clamp panel center within safe area; scrim full-bleed.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for titles; Roboto-Medium SDF for body/buttons; TMP inline sprite for the gem glyph. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| ① Title "CONFIRM" | prestige serif | Heavy | UPPER | +6% | 1.0 | gold bevel + bloom + dark stroke | ~48 | #f0d27a / stroke #3a2c0e |
| ① Body "Spend 150 gems?" | prompt | Regular | Sentence | 0 | 1.15 | none (inline violet gem glyph) | ~28 | #d9d2c2 |
| ① CONFIRM label | primary CTA | Bold | UPPER/Title | +3% | 1.0 | white on blue + shadow | ~28 | #ffffff |
| ① CANCEL label | secondary | Bold | UPPER/Title | +3% | 1.0 | gold/grey on outline | ~28 | #cdbf9e |
| ② Toast text "Equipped!" | success | Bold | Title | +1% | 1.0 | faint green glow | ~30 | #eafff0 |
| ③ Title "INSUFFICIENT GEMS" | warning serif | Heavy | UPPER | +5% | 1.05 | amber-gold bevel + bloom + stroke | ~44 | #f0b24a / stroke #3a2607 |
| ③ Body "You need 250 more gems to complete this purchase." | body | Regular | Sentence | 0 | 1.2 | none | ~26 | #d9d2c2 |
| ③ Deficit value "100" | data | Black | — | 0 | 1.0 | violet-gold glow + shadow | ~40 | #e9dcc0 |
| ③ Hint "Purchase more gems to continue." | hint | Regular | Sentence | 0 | 1.15 | none | ~24 | #b8b0a0 |
| ③ BUY MORE label | primary CTA | Bold | UPPER | +3% | 1.0 | white on blue + shadow | ~28 | #ffffff |
| ④ Title "CONNECTION LOST" | warning serif | Heavy | UPPER | +5% | 1.05 | amber-gold bevel + bloom + stroke | ~44 | #f0b24a / stroke #3a2607 |
| ④ Body "Unable to connect to the server. Please check your connection and try again." | body | Regular | Sentence | 0 | 1.2 | none | ~26 | #d9d2c2 |
| ④ RETRY label | primary CTA | Bold | UPPER | +3% | 1.0 | white on blue + shadow | ~28 | #ffffff |

## G. MATERIALS
- **Frames (①③④):** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin, worn edges, engraved corner filigree; gold rim-light; **TopGemFinial** = faceted blue/gold gem with bloom.
- **Panel fills:** obsidian #0c0e14→#14161e gradient, low reflectivity, soft inner shadow at frame; faint vignette.
- **Crest shields (③④):** amber/bronze cast shield (#caa04a hi, #8a6a28 mid, #4a3410 sh) with a small exclamation/heraldic glyph and a warm glow — signals "attention/warning".
- **Primary blue buttons (CONFIRM/BUY MORE/RETRY):** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft outer glow (brightest object).
- **CANCEL (outline):** transparent dark fill with a gold/grey beveled rim, no glow.
- **② Toast:** rounded pill, dark fill (#10131a) with a **gold hairline rim** and a **green emissive glow** (#3fd07a) bleeding outward; **check medallion** = green disc (#2e8a52→#3fd07a) with white ✓ and faint bloom.
- **③ Gem icon:** faceted violet amethyst (#9e6bf0 core, #5a2db0 shadow, white specular) with bloom.
- **④ WifiError icon:** steel/grey wifi arcs with a bright **red ✕** (#d8452b) overlay + faint danger glow.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **Scrim:** static input-blocker; tap = Cancel for ① (if `cancelOnScrim`), ignored for ③④ (must use a button) — configurable per call. Fades with the modal.
- **Primary blue buttons (CONFIRM/BUY MORE/RETRY):** idle cobalt gloss+glow; hover brighter+glow grows; pressed darken+scale 0.96+inset; disabled desaturated grey @50% (e.g., RETRY briefly disabled while a retry is in flight → spinner). Feedback: confirm SFX.
- **CANCEL (secondary outline):** idle ghost; hover rim brightens + faint fill; pressed scale 0.96; gives a soft "back" SFX.
- **② Toast:** no interactive states (non-blocking, auto-dismiss); optional tap-to-dismiss-early.
- **RETRY in-flight:** show a small rotating spinner inside the button + disable until the network call resolves; on failure keep the modal, on success close it.
- **Feedback:** all buttons ≥88px touch; pressed ≤80 ms scale; primary always visually dominant over secondary.

## I. ANIMATION TIMELINE
- **Modal show (①③④):** 0 ms Scrim α 0→0.58 over 140 ms (ease-out). 60 ms ModalPanel scale 0.92→1.0 + α 0→1 over 200 ms (ease-out-back, overshoot ≤4%) — a "slide+pop" (optionally also +20px→0 vertical). 160 ms TopGemFinial + corner flourishes glint (one-shot sparkle). 180 ms primary CTA glow pulse begins (loop). For ③ the deficit gem does a soft bloom pulse at 200 ms; for ④ the wifi ✕ does a single shake (±4px, 180 ms).
- **Modal dismiss (OnConfirm/OnCancel/OnRetry-success):** ModalPanel scale 1→0.95 + α 1→0 over 140 ms (ease-in); Scrim α 0.58→0 over 160 ms; then pop overlay.
- **② Toast in:** α 0→1 + slide from top −24px→0 + scale 0.96→1 over 200 ms (ease-out-back); **hold ~1.6 s**; **out:** α 1→0 + slide +12px + scale 1→0.98 over 220 ms (ease-in). Green glow pulses once on entry. Total lifetime ~2.0 s (configurable).
- **Easing:** ease-out-back for entries (gentle), ease-in for exits.

## J. PARTICLE & FX
- **TopGemFinial / corner flourishes:** one-shot glint on show + steady faint bloom.
- **Primary CTA:** subtle pulsing rim glow (±10%, 1.6 s loop).
- **③ Gem:** soft amethyst bloom + occasional sparkle.
- **④ WifiError ✕:** faint red danger glow + the single entry shake (no looping jitter — keep calm).
- **② Toast:** green success glow flare on entry, then steady soft glow during hold; subtle outward shimmer. Keep all FX restrained — these are functional overlays.

## K. EVENT BEHAVIOR
- **①Confirm — OnShow(prompt, confirmLabel, cancelLabel, cost, cancelOnScrim):** render the prompt (with inline gem glyph if a gem cost is supplied). **OnConfirm:** invoke the caller's confirm callback (e.g., spend, leave clan, logout, reset) — the actual mutation is server/stub-validated by the caller, NOT by the modal — then dismiss. **OnCancel / OnScrim(if enabled) / OnBackKey:** dismiss with no action.
- **②Toast — OnToast(message, [icon], [duration]):** spawn, play in→hold→out, auto-destroy; never blocks input; multiple toasts queue/stack.
- **③Insufficient Gems — OnShow(needed, deficit):** display the shortfall text and deficit amount. **OnBuyMore:** route to the Store (gem packs); dismiss this modal. **OnCancel/OnScrim/OnBackKey:** dismiss (purchase aborted). Never auto-deducts.
- **④Connection Lost (compact) — OnShow(message):** display. **OnRetry:** disable button + spinner → re-attempt the failed network call; on success dismiss and resume; on failure keep the modal (optionally update message). **OnBackKey:** configurable (usually blocked until resolved, or routes to Main Menu via the full-screen variant spec 39).
- **General:** these overlays are **stateless/reusable** — created on demand, parameterized by the caller, and popped on resolution. They never persist game state.

## L. NEGATIVE RULES
- Do NOT let any modal mutate a currency/balance itself — the modal only invokes the caller's server/stub-validated callback; ③ never deducts, it routes to Store.
- Do NOT make the scrim transparent or non-blocking for ①③④ (input MUST be blocked beneath them); conversely the **Toast MUST NOT block input** (root raycast off).
- Do NOT reproduce the 2×2 documentation grid in production — each component is a solo, centered overlay.
- Do NOT stretch panels on ultrawide; keep fixed sizes, re-centered; keep centers inside safe area.
- Do NOT make CANCEL/secondary brighter than the primary blue CTA.
- Do NOT auto-dismiss ①③④ (they require a choice); do NOT make the Toast persistent (it must auto-expire).
- Do NOT add real brand text/stick figures; keep palette within DNA; amber title tone reserved for warning modals (③④).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render gold-bevel serif titles or the inline gem sprite well — **flag the TMP SDF upgrade** (and TMP sprite atlas for the gem glyph); don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. All four components specified & buildable as independent overlays; the doc-grid is not reproduced in production.
2. ①: gold-frame panel, "CONFIRM" title, "Spend 150 [violet gem] gems?" body, CONFIRM (blue, primary) + CANCEL (outline) — Confirm is the brightest element; scrim blocks input.
3. ②: rounded green-glow pill with check medallion + "Equipped!"; non-blocking; auto-dismisses (~2 s) with in/out anim.
4. ③: shield crest + amber "INSUFFICIENT GEMS" + "You need 250 more gems to complete this purchase." + big violet gem with "100" + "Purchase more gems to continue." + BUY MORE (blue) → routes to Store; no auto-deduct.
5. ④: shield crest + amber "CONNECTION LOST" + "Unable to connect to the server. Please check your connection and try again." + wifi-with-red-✕ icon + RETRY (blue) with in-flight spinner.
6. Every modal has the top gem finial + four corner flourishes; centered & size-clamped; scrim full-bleed at ~55–65% alpha.
7. Colors within DNA hex ranges; titles serif gold/amber; primary CTA cobalt brightest; animations per Section I.
8. No modal mutates a balance; toast never blocks; ①③④ always block.

## N. IMPLEMENTATION CONFIDENCE
**95/100.** Very high: simple, well-bounded reusable overlays; all text, structure, button roles, and the blocking/non-blocking distinction are unambiguous. Risks: bespoke frame/crest/finial/icon artwork + the TMP inline gem sprite (-3); gold/amber-bevel serif needs TMP SDF (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] ALL FOUR sub-components specified, each with its own ASCII sub-tree (per the 4-in-1 sheet rule).
- [x] Modal pattern = full-screen scrim Image[raycast block] + centered panel; Toast = non-blocking exception.
- [x] Fraction-based sizing → 2340×1080; centered & clamped; safe-area; scrim full-bleed.
- [x] Exact strings/numbers recorded (Confirm/Spend 150/Equipped!/Insufficient Gems/250/100/Connection Lost/Retry/Buy More).
- [x] Typography + hex; materials with hex/finish; states; animation (modal slide+scrim, toast in/out); events (OnConfirm/OnCancel/OnDismiss/OnToast/OnRetry); negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.
