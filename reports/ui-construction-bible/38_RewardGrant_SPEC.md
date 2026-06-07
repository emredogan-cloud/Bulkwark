# BULWARK — UI CONSTRUCTION SPEC · 38 · Reward Grant
Source: design/RewardGrantDesign.png · 1536×1024 (≈1.5:1) · Analysis-only forensic spec.

> Normalize to 2340×1080. CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). This is a CENTERED modal overlay floating over the current screen on a dim scrim that BLOCKS input. FRACTION-BASED sizing; px quoted at 1080-tall height.

---

## A. SCREEN PURPOSE
The generic **"REWARD!" grant popup** — a reusable celebratory overlay shown whenever the player is awarded items (quest/daily/season/league/event/clan rewards, etc.). It announces the grant ("You have received the following:"), displays one or more **reward icons with amounts** inside a radiant burst, and ends with a single **COLLECT** action that acknowledges the grant and dismisses. Source shows two rewards: a **violet gem cluster "+40"** and a **silver coin stack "+500"**. The grant itself is **server-authoritative** (already applied or applied on COLLECT-ack server-side); this overlay is presentation + acknowledgement only. Floats over any screen.

## B. VISUAL DNA (inherits GLOBAL DNA)
- Dark heroic high-fantasy; a **near-black panel** (#0c0e14→#16131e, faintly warm) inside a **brushed gold/antique bronze ornate frame** with corner filigree and a small **blue/gold gem finial** centered on the top edge.
- Background (behind the panel/scrim) is a dim **battlefield at dusk with faction banners** and scattered braziers, heavily darkened so the panel pops.
- **Serif gold-bevel UPPERCASE title** "REWARD!" — large, celebratory, with strong bloom.
- The reward icons sit in a **warm radiant gold burst** (god-ray sunburst) — the focal celebration. **Violet/amethyst** gems + **silver** coins are the two reward materials shown.
- **Royal/cobalt blue COLLECT** CTA at the bottom — the brightest interactive element. A thin gold divider separates the rewards from the button. Low-key field → luminous focal burst.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
RewardGrantOverlay (UiScreen overlay root, CanvasGroup) — pushed ON TOP of current screen
└─ Scrim (Image, full-screen, semi-transparent black, raycastTarget=TRUE ← blocks input)
   ├─ BG_Ambience (Image — dim dusk battlefield + banners; optional, behind panel, above scrim)
   └─ RewardPanel (Image, ornate gold frame, centered, ~square)
      ├─ TopGemFinial (Image — blue/gold gem on top edge)
      ├─ CornerFlourish_TL / TR / BL / BR (Image x4)
      ├─ Title_REWARD ("REWARD!", serif gold)
      ├─ Subtitle_Received ("You have received the following:")
      ├─ RewardBurst (Image — radiant gold sunburst behind the items)
      ├─ RewardRow (HorizontalLayoutGroup)
      │  ├─ RewardItem_Gems
      │  │  ├─ Icon_GemCluster (violet crystals)
      │  │  └─ AmountChip_Gems (Text "+40" on dark capsule)
      │  └─ RewardItem_Coins
      │     ├─ Icon_CoinStack (silver star-stamped coins)
      │     └─ AmountChip_Coins (Text "+500" on dark capsule)
      ├─ Divider (Image — thin gold rule)
      └─ Btn_COLLECT (blue, primary)
```

## D. UNITY HIERARCHY SPEC (per node)
- **RewardGrantOverlay** — parent: UiRouter canvas, pushed as an OVERLAY above the current screen (does NOT replace it). Empty `RectTransform` + `CanvasGroup`. Stretch-all. High sorting order.
- **Scrim** — parent overlay. Anchor stretch-all (full-bleed, ignores safe area to dim the notch). `Image` solid black @ ~58% alpha. **`raycastTarget=true`** → blocks input beneath. Scrim tap is **ignored** here (must press COLLECT) — `cancelOnScrim=false`.
- **BG_Ambience** — parent Scrim (child, drawn above the flat scrim but behind the panel). Anchor stretch-all, `Image` dim battlefield, raycast off. Optional; if omitted, the flat scrim alone is fine.
- **RewardPanel** — parent Scrim. Anchor center (0.5,0.5) pivot 0.5,0.5. `Image` 9-slice ornate gold frame, ~square (see E). Center clamped inside safe area. `raycastTarget=true`.
- **TopGemFinial** — parent RewardPanel, anchor top-center (0.5,1) pivot 0.5,0.5 (overhangs). `Image`, raycast off.
- **CornerFlourish_TL/TR/BL/BR** — parent RewardPanel, anchored to each corner, raycast off.
- **Title_REWARD** — `Text` serif gold, anchor top-center pivot 0.5,1, alignment center, near panel top under finial.
- **Subtitle_Received** — `Text`, center-aligned, under title.
- **RewardBurst** — `Image` radiant sunburst, anchor center-upper (behind the reward row), pivot 0.5,0.5, raycast off, additive/screen-blend look. Drawn BEFORE RewardRow (earlier sibling).
- **RewardRow** — `HorizontalLayoutGroup` (spacing ~64, center-aligned), anchor center pivot 0.5,0.5. Holds N **RewardItem** children (here 2; layout must support 1–4 gracefully).
- **RewardItem_X** — `VerticalLayoutGroup` (center): big icon `Image` (`preserveAspect`) over an **AmountChip** (`Image` dark capsule + `Text` "+N", gold).
- **Icon_GemCluster** — `Image` violet amethyst crystal cluster, focal bloom. **Icon_CoinStack** — `Image` silver star-stamped coin stack.
- **AmountChip_X** — `Image` rounded dark capsule + centered `Text` "+40"/"+500" gold.
- **Divider** — `Image` thin gold rule, anchor horizontal-center below the reward row, raycast off.
- **Btn_COLLECT** — parent RewardPanel, anchor bottom-center pivot 0.5,0, `Button` cobalt gradient + label `Text` "COLLECT"; the brightest element; min height 72 px.
- **Responsive:** panel is centered & size-clamped — on ultrawide it stays fixed size (do NOT stretch); on small screens clamp ≤88% width / ≤90% height. RewardRow auto-centers for 1–4 items (wrap to a 2×2 if >4 — note as extension). Scrim full-bleed; panel center inside safe area.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **Scrim:** full 2340×1080, alpha ~0.58. BG_Ambience same bounds, darkened.
- **RewardPanel:** ≈ 0.345 W × 0.62 H (~807×670 px), **near-square**, centered. Frame border ~26 px 9-slice. Gem finial overhang ~30 px above top.
- **Title_REWARD:** baseline ~0.16 from panel top; large.
- **Subtitle_Received:** ~0.27 from top.
- **RewardBurst:** centered on the reward row at ~0.50 panel height, radius ~0.42 of panel width (extends slightly behind both icons).
- **RewardRow:** vertical center ~0.50 from top; two items with ~64 px gap; each item column ~0.30 of panel width. Icon Ø ~0.24 of panel width (~190 px); AmountChip below: ~0.20 W × 56 px.
- **Divider:** ~0.74 from top, width ~0.72 of panel, ~2 px tall.
- **Btn_COLLECT:** centered ~0.87 from top, ≈ 0.62 of panel width × 72 px.
- **Item count adaptivity:** 1 item → centered; 2 → as shown; 3 → equal thirds; 4 → tighter gaps or 2×2 grid; keep icons ≥0.18 W. Burst radius scales to span the row.
- **Tablet/ultrawide:** fixed panel size, re-centered, never stretched. **Notch:** clamp panel center within safe area; scrim/ambience full-bleed.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for title; Roboto-Medium SDF for subtitle/amounts/button. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "REWARD!" | celebratory prestige serif | Black | UPPER | +5% | 1.0 | heavy gold bevel + strong bloom + dark stroke + drop-shadow | ~72 | #f2d885 / stroke #3a2c0e |
| Subtitle "You have received the following:" | announce | Regular | Sentence | +1% | 1.1 | faint shadow | ~28 | #d9d2c2 |
| Amount "+40" (gems) | reward data | Black | — | 0 | 1.0 | gold + soft glow + shadow on capsule | ~36 | #f0d27a |
| Amount "+500" (coins) | reward data | Black | — | 0 | 1.0 | gold + soft glow + shadow on capsule | ~36 | #f0d27a |
| COLLECT label | primary CTA | Bold | UPPER | +4% | 1.0 | white on blue + shadow | ~32 | #ffffff |

## G. MATERIALS
- **Frame:** brushed gold/antique bronze (base #8a6a28, hi #f2d885, sh #5a3f12), satin, worn edges, engraved corner filigree; strong gold rim-light; **TopGemFinial** = faceted blue/gold gem with bloom.
- **Panel fill:** obsidian #0c0e14→#16131e (faintly warm) gradient, low reflectivity, soft inner shadow; subtle vignette toward corners.
- **BG_Ambience:** dim dusk battlefield, deep-blue faction banners at the sides, scattered warm braziers; heavily darkened/blurred.
- **RewardBurst:** warm **radiant gold sunburst / god-rays** (#ffdca0 core → #caa04a → transparent), additive/screen blend, soft bloom — the celebration light source behind the loot.
- **Icon_GemCluster:** faceted **violet amethyst** crystals (#a06ff2 core, #5a2db0 shadow, white specular), inner glow + bloom + sparkle.
- **Icon_CoinStack:** **silver/pewter** coins (#e6ebf1 hi, #aab2bb mid, #6c747d sh) with a **star** stamp on the face, metallic specular, faint glow.
- **AmountChips:** rounded near-black capsules (#10131a) with a thin gold rim; gold "+N" text.
- **Divider:** thin gold gradient rule (#caa04a→#f0d27a→fade).
- **Btn_COLLECT:** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft outer glow (brightest element).

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **Scrim:** static input-blocker; tap ignored (`cancelOnScrim=false`) — player must COLLECT. Fades with the overlay.
- **Btn_COLLECT (primary blue):** idle cobalt gloss + steady glow; hover brighter + glow grows (pointer platforms); pressed darken + scale 0.96 + inset; disabled (briefly, during the collect-ack round-trip) desaturated + small spinner. The only interactive element; ≥88px touch; satisfying confirm SFX + reward chime.
- **Reward icons / amount chips:** non-interactive display (no states), but they animate on entry (see I).
- **Feedback:** COLLECT press → reward chime, optional coin/gem fly-to-wallet micro-FX toward the top-right currency HUD of the underlying screen, then dismiss.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms Scrim α 0→0.58 over 150 ms (ease-out); BG_Ambience α 0→1 with it. 70 ms RewardPanel scale 0.90→1.0 + α 0→1 over 220 ms (ease-out-back, overshoot ≤5%). 160 ms RewardBurst scale 0.7→1.0 + α 0→1 over 260 ms with a slow continuous rotation begun (god-rays). 220 ms Title "REWARD!" pop: scale 0.85→1.0 + bloom flare over 200 ms. 300 ms each RewardItem staggers in (80 ms apart): scale 0.6→1.0 (ease-out-back) + α, with a sparkle burst on arrival; AmountChip counts up 0→value over ~400 ms (ease-out) [+40 / +500]. 520 ms Divider wipes in (left→right, 180 ms). 560 ms COLLECT fades/raises in (+16px→0, 180 ms) and begins its glow pulse.
- **OnCollect:** COLLECT scale 0.96 (80 ms); optional fly-to-wallet streaks (coins/gems arc to the top-right HUD over ~400 ms); then RewardPanel scale 1→0.96 + α 1→0 over 160 ms (ease-in), burst fades, Scrim α →0 over 180 ms; pop overlay.
- **Idle loops:** RewardBurst slow rotation (~12 s/rev) + gentle pulse (±6%, 2.4 s); COLLECT rim glow (±10%, 1.6 s); occasional sparkle on the gem cluster.
- **Easing:** ease-out-back entries (celebratory), ease-in exit.

## J. PARTICLE & FX
- **RewardBurst:** rotating god-ray sunburst + drifting gold dust motes (low rate) + a one-shot radial sparkle flare when items land.
- **Icon_GemCluster:** amethyst bloom + intermittent sparkle (1–2 motes).
- **Icon_CoinStack:** metallic glint sweep + tiny coin-shine sparkles.
- **TopGemFinial / corner flourishes:** one-shot glint on show + faint steady bloom.
- **COLLECT:** pulsing rim glow; on press, optional coin/gem fly-to-wallet streak particles toward the currency HUD. Celebratory but not seizure-y — bloom and sparkle, no harsh strobing.

## K. EVENT BEHAVIOR
- **OnShow(rewards[]):** receive a server-authoritative list of `{type, icon, amount}` (here gems+40, coins+500); build RewardItems dynamically (1–4); play the entry timeline; the grant is recorded server-side (either already applied, or applied on COLLECT-ack — caller decides; the overlay does NOT mint currency).
- **OnCollect:** disable button + spinner → send COLLECT/ack to the server-auth meta service → on success, play fly-to-wallet, refresh the underlying screen's wallet, dismiss the overlay; on failure, re-enable + surface NetworkError (spec 39) and keep the grant pending.
- **OnBackKey:** treated as COLLECT (acknowledge) — never silently discard a reward; if a round-trip is required, route through OnCollect.
- **Reusability:** parameterized & stateless; spawned on demand by any reward source; popped on acknowledgement.

## L. NEGATIVE RULES
- Do NOT let the client mint/add currency — amounts are display; the actual grant is server-authoritative (applied/acked server-side). The fly-to-wallet is cosmetic; the real wallet refresh comes from the server.
- Do NOT allow dismissal without acknowledgement — no scrim-tap close, no silent discard; COLLECT (or BackKey→Collect) only.
- Do NOT replace the underlying screen; this floats over it on a blocking scrim.
- Do NOT stretch the panel/icons on ultrawide; fixed size, re-centered; center inside safe area.
- Do NOT exceed 4 inline items without switching to a grid (keeps icons legible); do NOT shrink icons below ~0.18 panel width.
- Do NOT add real brand text/stick figures; keep palette within DNA (violet gems, silver coins, gold burst, cobalt CTA).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel "REWARD!" convincingly — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Centered near-square gold-framed panel with top gem finial + four corner flourishes, on a ~58% blocking scrim over a dim battlefield ambience.
2. Title "REWARD!" (large serif gold, strong bloom) + subtitle "You have received the following:".
3. Two reward items inside a radiant gold burst: violet gem cluster with "+40" and silver star-stamped coin stack with "+500", each on a dark amount chip.
4. Thin gold divider, then a single cobalt **COLLECT** button (brightest element, ≥72px tall, centered).
5. Layout supports 1–4 items centered; icons ≥0.18 panel width; amounts count up on entry.
6. COLLECT acknowledges (server-acked) and dismisses; no scrim-tap close; back key = collect.
7. Colors within DNA hex ranges; animations per Section I (panel pop, burst rotation, item stagger, count-up, collect fly-to-wallet); client never mints currency.

## N. IMPLEMENTATION CONFIDENCE
**95/100.** Very high: a single, well-bounded reusable popup; all text, structure, two reward types, and the COLLECT flow are unambiguous. Risks: bespoke gem-cluster/coin-stack/sunburst/frame artwork + sparkle FX (-3); gold-bevel "REWARD!" needs TMP SDF (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Modal pattern = full-screen blocking scrim Image[raycast] + centered panel.
- [x] Fraction-based sizing → 2340×1080; centered & clamped; safe-area; scrim full-bleed; adaptive 1–4 items.
- [x] Exact strings/numbers recorded (REWARD!, subtitle, +40 gems, +500 coins, COLLECT).
- [x] Typography + hex; materials (gem/coin/burst) with hex/finish; states; animation (count-up, burst, collect); FX; events (OnShow/OnCollect); negative rules (no minting, no silent dismiss).
- [x] No code/assets/scenes; analysis-only; invented nothing.
