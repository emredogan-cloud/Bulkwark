# BULWARK — UI CONSTRUCTION SPEC · 36 · Settings
Source: design/SettingsScreenDesign.png · 1915×821 (≈2.33:1) · Analysis-only forensic spec.

> Normalize to 2340×1080 (≈19.5:9). CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). FRACTION-BASED layout; px quoted at 1080-tall height. Full-bleed BG under cutout; interactive content inside `Screen.safeArea`.

---

## A. SCREEN PURPOSE
The **System / Settings** screen. A **left tab rail** (General · Audio · Graphics · Controls · Account · Language · Support) selects content panels in the body. The **General** tab (shown) surfaces four grouped panels — **AUDIO** (Music/Sound Effects/Voice sliders + per-channel mute), **GRAPHICS** (Quality/Resolution/Frame Rate segmented selectors + Shadows/Bloom toggles), **ACCOUNT** (avatar/name/level/XP + Change Name + Link Account), **OTHER** (Vibration/Push Notifications/Battery Saver toggles + a faction emblem) — plus a bottom action bar **LOGOUT · PRIVACY POLICY · RESET SETTINGS**. **IMPLEMENTATION REALITY: only the mute toggle is functional today; every other control is a placeholder** (wire visually, no-op/stub the rest — see A/L). Reached from the Main-Menu rail and from the in-match Pause modal. No gameplay/ECS interaction.

## B. VISUAL DNA (screen-specific, inherits GLOBAL DNA)
- Dark heroic high-fantasy; near-black field over a faint **torch-lit dungeon/armory** backdrop (warm braziers, dark stone), heavily vignetted.
- **Brushed gold/antique bronze ornate title plate** "SETTINGS" centered top with **crown + wing/laurel** ornaments; gold frames around each grouped panel.
- **Left tab rail** = dark vertical bronze-framed strip; selected tab = warm gold highlight bar; others muted.
- **Violet/amethyst** = the gem currency (top-right) + a faint magical accent. **Royal blue** = the ACCOUNT action buttons (Change Name / Link Account) and selected segmented options. **Ember red** = the **LOGOUT** danger button. **Green** (#3fd07a) accents on ON toggles. **Gold** = section headers, slider fills, selected segment.
- Panel headers are small gold serif caps centered with flanking flourishes. Low-key lighting; warm focal glow from the background braziers; gold rim-light on frames.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
SettingsScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ BackgroundLayer (full-bleed)
   │  ├─ BG_Dungeon (Image — torch-lit armory)
   │  └─ BG_Vignette (Image)
   ├─ TopBar
   │  ├─ BackButton (gold square + left chevron)
   │  ├─ TitleOrnamentLeft (crown/wing flourish)
   │  ├─ Title_SETTINGS (Text serif gold)
   │  ├─ TitleOrnamentRight (flourish)
   │  └─ CurrencyCluster
   │     ├─ GemChip (Icon_Gem(violet) + "1746" + "+")
   │     └─ GoldChip (Icon_Coin + "48570" + "+")
   ├─ BodyRow
   │  ├─ TabRail (left vertical)
   │  │  ├─ Tab_General (selected)  (icon + "General")
   │  │  ├─ Tab_Audio
   │  │  ├─ Tab_Graphics
   │  │  ├─ Tab_Controls
   │  │  ├─ Tab_Account
   │  │  ├─ Tab_Language
   │  │  └─ Tab_Support
   │  └─ ContentArea (General tab content)
   │     ├─ Panel_AUDIO
   │     │  ├─ Header_AUDIO
   │     │  ├─ Row_Music    (Icon_Speaker + "Music"         + Slider[100%] + "100%" + MuteToggle)
   │     │  ├─ Row_SFX      (Icon_Speaker + "Sound Effects" + Slider[100%] + "100%" + MuteToggle)
   │     │  └─ Row_Voice    (Icon_Speaker + "Voice"         + Slider[80%]  + "80%"  + MuteToggle)
   │     ├─ Panel_GRAPHICS
   │     │  ├─ Header_GRAPHICS
   │     │  ├─ Row_Quality    ("Quality"    + Seg[LOW|MEDIUM|HIGH|ULTRA*])
   │     │  ├─ Row_Resolution ("Resolution" + Seg[50%|75%|100%*])
   │     │  ├─ Row_FrameRate  ("Frame Rate" + Seg[30 FPS|60 FPS*|120 FPS])
   │     │  ├─ Row_Shadows    ("Shadows"    + Toggle[ON])
   │     │  └─ Row_Bloom      ("Bloom"      + Toggle[ON])
   │     ├─ Panel_ACCOUNT
   │     │  ├─ Header_ACCOUNT
   │     │  ├─ AccountIdentity (Avatar + "StickKing" + "Level 45" + XPBar["25600"])
   │     │  ├─ Btn_ChangeName (blue)
   │     │  └─ Btn_LinkAccount (blue)
   │     ├─ Panel_OTHER
   │     │  ├─ Header_OTHER
   │     │  ├─ Row_Vibration         ("Vibration" + Toggle[ON])
   │     │  ├─ Row_PushNotifications  ("Push Notifications" + Toggle[ON])
   │     │  ├─ Row_BatterySaver       ("Battery Saver Mode" + Toggle[ON])
   │     │  └─ FactionEmblem (crossed-swords shield art)
   │     └─ BottomActionBar
   │        ├─ Btn_LOGOUT (red)
   │        ├─ Btn_PRIVACY_POLICY
   │        └─ Btn_RESET_SETTINGS
```

## D. UNITY HIERARCHY SPEC (per node)
- **SettingsScreen** — parent UiRouter canvas. Empty `RectTransform` + `CanvasGroup`. Stretch-all. Full screen (NOT a modal; though it can be pushed over Pause).
- **SafeAreaRoot** — `SafeAreaFitter`, stretch-all. Children: Background, TopBar, BodyRow.
- **BackgroundLayer** (BG_Dungeon, BG_Vignette) — parent SettingsScreen (outside safe area). Stretch-all `Image`, raycast off.
- **TopBar** — parent SafeAreaRoot. Anchor top-stretch, pivot 0.5,1, fixed height (E). BackButton top-left; Title+ornaments top-center; CurrencyCluster top-right `HorizontalLayoutGroup`.
- **BackButton** — `Button`, (0,1) pivot 0,1, ≥88×88.
- **Title_SETTINGS** — `Text` serif gold, top-center, alignment center; ornaments anchored to its measured edges.
- **CurrencyCluster** — `HorizontalLayoutGroup` (spacing ~16), (1,1) pivot 1,1. GemChip + GoldChip; each = capsule `Image` + icon + value `Text` + "+" `Button` (opens Store).
- **BodyRow** — parent SafeAreaRoot. Stretch below TopBar (min (0,0) max (1, topBarBottomFrac)) pivot 0.5. Two regions: TabRail (left, fixed fractional width) and ContentArea (fills rest).
- **TabRail** — anchor left-stretch (0,0)-(0,1) pivot 0,0.5, fixed width (E). `Image` dark bronze-framed strip. `VerticalLayoutGroup` (spacing ~6) of 7 tabs; `ToggleGroup` (exclusive). Each **Tab_X** = `Toggle` with child `Image` (icon) + `Text` (label), left-aligned; selected shows a gold highlight bar `Image`.
- **ContentArea** — anchor: fills right of rail (min (railWidthFrac,0) max (1,1)) pivot 0.5. Uses a **2×2 grid of grouped panels** + a bottom bar. Layout via anchored panels (not a single LayoutGroup) because the four panels differ in height. Child order: Panel_AUDIO (top-left), Panel_GRAPHICS (top-right), Panel_ACCOUNT (bottom-left), Panel_OTHER (bottom-right), BottomActionBar (very bottom, spanning).
- **Panel_AUDIO / GRAPHICS / ACCOUNT / OTHER** — each `Image` gold-framed sub-panel with a centered **Header_X** `Text` (gold caps + flourishes) at top and a `VerticalLayoutGroup` of rows below.
- **Audio Row_X** — `HorizontalLayoutGroup`: Icon_Speaker `Image` | label `Text` (fixed width) | **Slider** (flexible) | percent `Text` (fixed) | **MuteToggle** (speaker `Toggle`). The **Slider** = uGUI `Slider` (0–100) with gold fill + round gold knob.
- **Graphics Row_Quality/Resolution/FrameRate** — `HorizontalLayoutGroup`: label `Text` | **SegmentedControl** = a `ToggleGroup` of N option `Toggle`s in a horizontal capsule (selected option = gold fill, others ghost). Quality has 4 options (ULTRA selected), Resolution 3 (100% selected), Frame Rate 3 (60 FPS selected).
- **Graphics Row_Shadows/Bloom** — `HorizontalLayoutGroup`: label `Text` | **Toggle** (pill ON/OFF, ON = green/gold knob right).
- **Panel_ACCOUNT → AccountIdentity** — `HorizontalLayoutGroup`/composite: circular Avatar `Image` (faction ring) | vertical block: name `Text` "StickKing", "Level 45" `Text`, **XPBar** = `Slider`/`Image` fill labeled "25600". Below identity: **Btn_ChangeName** and **Btn_LinkAccount** (blue `Button`s, stacked or side-by-side).
- **Panel_OTHER → Row_X** — `HorizontalLayoutGroup`: label `Text` | **Toggle** (ON). **FactionEmblem** = decorative `Image` (crossed-swords shield) anchored in the panel's right area; `raycastTarget=false`.
- **BottomActionBar** — parent ContentArea (or SettingsScreen body), anchor bottom-stretch pivot 0.5,0, fixed height. `HorizontalLayoutGroup`: **Btn_LOGOUT** (red), **Btn_PRIVACY_POLICY**, **Btn_RESET_SETTINGS** — left-weighted/grouped as in source (Logout leftmost).
- **Responsive:** match-height keeps row/slider heights stable. On ultrawide, the 2×2 grid keeps fractions; extra width pads between panels. Tab rail keeps min width. Notch handled by SafeAreaRoot; BG full-bleed.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar height:** ≈ 0.105 (~113 px). BackButton ≈ 88×88 left-inset ~1.4%. Title center; ornaments ±(title half-width + ~50 px). CurrencyCluster right-inset ~1.4%; each chip ~210 px incl. "+".
- **BodyRow:** y from ~0.105 to ~0.99.
- **TabRail width:** ≈ 0.165 of canvas width (~386 px), x≈[0.012, 0.177]. 7 tabs equally spaced over the rail height; each tab row height ≈ 0.115 of rail (~108 px), icon ~36px left + label. Selected highlight bar spans the tab width.
- **ContentArea:** x≈[0.19, 0.99], the 2×2 panel grid:
  - Column split inside content: left panels ≈ 0.46 of content width, right panels ≈ 0.46, gutter ~0.04 (Account/Audio narrower-left as in source; Graphics/Other on the right).
  - Row split: top panels (AUDIO, GRAPHICS) occupy ~0.46 of body height; bottom panels (ACCOUNT, OTHER) ~0.40; BottomActionBar ~0.12; small gutters ~0.02.
  - **Panel_AUDIO** top-left ≈ x[0.19,0.55] y[top]. **Panel_GRAPHICS** top-right ≈ x[0.57,0.99]. **Panel_ACCOUNT** bottom-left. **Panel_OTHER** bottom-right.
- **Audio row metrics:** row height ≈ 64 px; gap ~12. Speaker icon Ø ~32; label width ~0.28 of panel; slider track width ~0.40 of panel, knob Ø ~28; percent text width ~0.10; mute speaker toggle ~40×40 right.
- **Graphics row metrics:** row height ≈ 64 px. Segmented control: each option ~70–90 px wide in a shared capsule ~0.55 of panel width; selected option gold-filled. Toggle pill ~64×32.
- **Account panel:** Avatar Ø ~96 px; XP bar height ~16 px width ~0.7 of panel; ChangeName/LinkAccount buttons height ~56 px each.
- **Other panel:** three toggle rows height ~56 px; FactionEmblem ~140×140 right-area, decorative.
- **BottomActionBar:** height ≈ 0.10 of body (~104 px); three buttons ~0.20 width each, height ~64 px; LOGOUT leftmost (red), then PRIVACY POLICY, RESET SETTINGS; gap ~16.
- **Tablet 4:3 / ultrawide:** keep the 2×2 grid fractions; clamp content max-width ~0.97·2340 on ultrawide. **Notch:** inside SafeAreaRoot; BG bleeds.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF for title/headers; Roboto-Medium SDF for body/labels/values. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title "SETTINGS" | prestige serif | Heavy | UPPER | +6% | 1.0 | gold bevel + bloom + dark stroke | ~60 | #f0d27a / stroke #3a2c0e |
| Gem value "1746" | data | Bold | — | +1% | 1.0 | violet-tinged shadow | ~28 | #d8cdf0 |
| Gold value "48570" | data | Bold | — | +1% | 1.0 | shadow | ~28 | #f0d27a |
| Tab labels (General…Support) | menu sans | Bold | Title | +2% | 1.0 | sel gold-glow / unsel grey | ~28 | sel #f4dd8c / unsel #9a8a6a |
| Panel headers (AUDIO/GRAPHICS/ACCOUNT/OTHER) | section serif caps | Bold | UPPER | +6% | 1.0 | gold + flourishes | ~30 | #f0d27a |
| Audio labels (Music/Sound Effects/Voice) | label | Medium | Title | +1% | 1.0 | none | ~26 | #d9d2c2 |
| Percent values (100%/80%) | data | Bold | — | 0 | 1.0 | shadow | ~24 | #f0d27a |
| Graphics labels (Quality/Resolution/Frame Rate/Shadows/Bloom) | label | Medium | Title | +1% | 1.0 | none | ~26 | #d9d2c2 |
| Segmented options (LOW/MEDIUM/HIGH/ULTRA, 50%/75%/100%, 30/60/120 FPS) | option | Bold | UPPER | +2% | 1.0 | sel dark-on-gold / unsel grey | ~22 | sel #2a2010 / unsel #b8b0a0 |
| Toggle ON/OFF text (if shown) | state | Bold | UPPER | +2% | 1.0 | ON green / OFF grey | ~20 | ON #3fd07a / OFF #8c8270 |
| Account name "StickKing" | name | Bold | Title | +1% | 1.0 | shadow | ~30 | #f3ead2 |
| "Level 45" | meta | Medium | Title | +1% | 1.0 | none | ~24 | #c9bfa6 |
| XP value "25600" | data | Medium | — | 0 | 1.0 | none | ~22 | #d9d2c2 |
| Change Name / Link Account labels | CTA | Bold | Title | +2% | 1.0 | white on blue + shadow | ~24 | #ffffff |
| Other labels (Vibration/Push Notifications/Battery Saver Mode) | label | Medium | Title | +1% | 1.0 | none | ~26 | #d9d2c2 |
| LOGOUT label | danger CTA | Bold | UPPER | +3% | 1.0 | white on red + shadow | ~26 | #ffffff |
| PRIVACY POLICY / RESET SETTINGS labels | CTA | Bold | UPPER | +3% | 1.0 | gold/cream on dark | ~24 | #e9dcc0 |

## G. MATERIALS
- **Title plate / frames:** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin, worn edges, engraved filigree; crown/wing ornaments polished gold with bloom.
- **Panel fills:** obsidian #0c0e14→#14161e gradient, low reflectivity, inner shadow at frame edge.
- **Tab rail:** darker bronze-framed strip; **selected highlight** = warm gold gradient bar (#caa04a→#f0d27a) with soft glow; tab icons gold line-art.
- **Sliders:** track = recessed dark groove (#1a1c24) with a **gold fill** (#caa04a→#f0d27a) up to value; **knob** = round gold cabochon with specular + faint glow; mute speaker icon gold (lit) / grey (muted).
- **Segmented controls:** dark capsule with thin gold rim; **selected option** = gold gradient fill with dark text; unselected = transparent with grey text; thin separators.
- **Toggles (Shadows/Bloom/Vibration/Push/Battery):** pill track dark when OFF / **green-gold when ON** (#2e8a52→#3fd07a tint), gold knob; slight glow when ON.
- **Account avatar:** portrait sprite in beveled gold ring (faction-tinted); **XP bar** = gold fill in a dark groove.
- **Blue buttons (Change Name/Link Account):** royal cobalt #2b56c8→#4f8bff gloss + inner highlight + soft glow.
- **FactionEmblem:** crossed-swords over a steel/blue shield, cast-metal relief, faint focal glow; decorative only.
- **LOGOUT:** oxblood/ember red gradient (#7a1f1a→#d8452b) gloss, danger glow. **PRIVACY/RESET:** dark stone capsules with gold rim + gold/cream label; RESET may carry a small refresh glyph.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
> **Functional-status note:** Per implementation reality, ONLY the **per-channel mute toggle** is live today; **all other controls render their states but no-op/stub** until wired. Treat every below as visually complete, behaviorally stubbed except the mute toggle.
- **BackButton:** idle gold chevron; hover +brightness+glow; pressed scale 0.94; SFX click.
- **Currency "+" buttons:** idle gold "+"; hover glow; pressed 0.96 → (stub) would open Store.
- **Tab rail tabs (Toggle group):** selected = gold highlight bar + bright label + glow; idle = no bar + muted label; hover = label brightens + faint bar; pressed = 0.97; disabled = 40%. Switching swaps ContentArea (General shown; other tabs reveal their own panels — stubbed today).
- **Sliders (Music/SFX/Voice):** idle gold fill + knob; hover knob +glow; **drag** = knob follows, fill updates, percent label live-updates; released = settle; disabled = desaturated. (Stub: value changes are visual only until audio mixer is wired.)
- **MuteToggle (FUNCTIONAL):** unmuted = lit gold speaker (sound waves); muted = grey speaker with slash; toggling **actually mutes that channel** (the one live behavior). Pressed scale 0.94; subtle SFX.
- **Segmented controls (Quality/Resolution/Frame Rate):** selected option gold-filled (dark text); others ghost; hover (unselected) brightens; pressed 0.96 → moves selection (stub: stored but not applied yet). ULTRA / 100% / 60 FPS are the seeded selections.
- **Toggles (Shadows/Bloom/Vibration/Push/Battery):** ON = knob-right + green-gold track + glow; OFF = knob-left + dark track; pressed = knob slides 120 ms; disabled = desaturated. (Stub today.)
- **Account buttons (Change Name/Link Account):** idle blue gloss; hover brighter+glow; pressed darken+0.96; disabled desaturated. (Stub → would open rename/link flow.)
- **BottomActionBar:** **LOGOUT (danger)** idle red, hover brighter red+glow, pressed darken+0.96 → triggers Confirm modal (spec 37) [stub action]. **PRIVACY POLICY** → opens URL/legal view (stub). **RESET SETTINGS** → Confirm modal then reset-to-defaults (stub). All ≥88px touch.
- **Feedback:** hover only on pointer platforms; pressed ≤80 ms scale; toggles/sliders give immediate visual response even when behaviorally stubbed.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms CanvasGroup α 0→1 over 180 ms. 40 ms title plate scale 0.985→1.0 over 200 ms (gentle back). 80 ms TabRail slide −20px→0 + α over 200 ms; tabs stagger 25 ms apart. 140–460 ms the four panels fade/scale-in (0.98→1) staggered 60 ms apart (AUDIO→GRAPHICS→ACCOUNT→OTHER). 260 ms BottomActionBar fades/raises in (180 ms). 220 ms FactionEmblem soft bloom one-shot.
- **Tab switch:** old content α 1→0 + slide −10px (120 ms); new content α 0→1 + slide +10px→0 (160 ms); selected highlight bar glides vertically to the new tab (140 ms ease-out).
- **Slider drag:** fill/percent track the knob in real time; release knob settle (80 ms, ease-out).
- **Toggle flip:** knob slides + track color crossfade over 120 ms (ease-out); ON adds a brief glow pop.
- **Segment select:** gold fill slides to the new option (120 ms ease-out).
- **LOGOUT/RESET pressed:** Confirm modal slide-in (per spec 37 I).
- **OnHide:** CanvasGroup α 1→0 over 140 ms; panels slight scale-down.
- **Easing:** ease-out enters, ease-in exits, gentle back on title/panel pops.

## J. PARTICLE & FX
- **Title ornaments / FactionEmblem:** steady focal bloom + occasional single sparkle.
- **Background braziers:** subtle warm flicker (low-amplitude brightness loop) + faint drifting embers (very low rate) — atmospheric, never over the controls.
- **Selected tab highlight & ON toggles:** subtle pulsing glow (±10%, 1.6 s).
- **Slider knobs:** faint glow on hover/drag only. Keep all FX low-key; control legibility first.

## K. EVENT BEHAVIOR
- **OnShow:** load current settings (audio levels, mute flags, graphics tier, toggles) from local prefs; bind account identity (name/level/XP) from profile; default tab = General.
- **OnTabChanged(General/Audio/Graphics/Controls/Account/Language/Support):** swap ContentArea to that tab's panels (today only General is fully populated; others stubbed/empty-state).
- **OnSliderChanged(channel,value):** update the percent label + (when wired) the audio mixer; persist to prefs. (Stub today — visual only.)
- **OnMuteToggled(channel) [FUNCTIONAL]:** mute/unmute that audio channel immediately; persist flag.
- **OnSegmentChanged / OnToggleChanged:** store the new value (stub: apply graphics/system change when wired); persist.
- **OnChangeName:** open rename flow (server-validated uniqueness). **OnLinkAccount:** open account-link (social/platform). [stubs]
- **OnLogout:** Confirm modal (spec 37) → on confirm, sign out / return to boot. [stub action]
- **OnPrivacyPolicy:** open legal/privacy view (URL or in-app). **OnResetSettings:** Confirm modal → reset prefs to defaults → refresh controls. [stubs]
- **OnCurrencyPlus:** open Store. **OnBack:** persist + pop screen → caller (Main Menu or Pause).

## L. NEGATIVE RULES
- **Do NOT imply non-mute controls are live** — only the per-channel mute toggle functions today; render the rest but stub/no-op (this is a known reality, surface it in code comments/empty-states; do not fake applied effects).
- Do NOT skip Confirm before **LOGOUT** and **RESET SETTINGS** (destructive).
- Do NOT mutate any server balance; account actions are server-validated; the client never changes XP/level.
- Do NOT replace this with a modal; it's a full screen (it may be pushed over Pause).
- Do NOT add real brand text or stick figures ("StickKing" is the seed username, keep it; no stick-figure art). Keep palette within DNA.
- Do NOT stretch panels/avatar on ultrawide — pad gutters and re-center.
- Do NOT make background braziers/embers distract from the controls.
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel serif title/headers well — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Title "SETTINGS" + crown/wing ornaments centered top; back chevron top-left; gem "1746" + gold "48570" chips top-right.
2. Left tab rail with all seven tabs (General selected, gold highlight): General/Audio/Graphics/Controls/Account/Language/Support, each with an icon.
3. AUDIO panel: Music (100%), Sound Effects (100%), Voice (80%) each with speaker icon + slider + percent + mute toggle; fills match values.
4. GRAPHICS panel: Quality segmented (ULTRA selected) / Resolution (100% selected) / Frame Rate (60 FPS selected) / Shadows (ON) / Bloom (ON).
5. ACCOUNT panel: avatar + "StickKing" + "Level 45" + XP bar "25600" + Change Name (blue) + Link Account (blue).
6. OTHER panel: Vibration (ON), Push Notifications (ON), Battery Saver Mode (ON) + the faction shield emblem.
7. Bottom bar: LOGOUT (red), PRIVACY POLICY, RESET SETTINGS; Logout & Reset trigger Confirm.
8. **Only the mute toggle is functional**; all other controls render correct states but are stubbed (verifiable: dragging a slider/flipping a graphics toggle changes visuals but not real settings yet).
9. Colors within DNA hex ranges; fraction-based + match-height stable; safe-area; BG full-bleed; animations per Section I.

## N. IMPLEMENTATION CONFIDENCE
**93/100.** High: rail+2×2 grid, slider/segment/toggle taxonomy, exact seeded values, action bar all read clearly. Risks: bespoke gold ornament/emblem/icon artwork (-3); gold-bevel serif needs TMP SDF (-2); the "only mute is functional" constraint means careful stubbing that's easy to over-implement (-1); exact XP-bar fill ratio for "25600" is interpretive (-1).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Left tab rail specified; "only mute toggle functional" reality flagged in A, H, K, L, M.
- [x] Fraction-based layout → 2340×1080; match-height; safe-area; full-bleed BG.
- [x] Exact strings/values recorded (tabs, audio %s, graphics selections, account name/level/XP, toggles, action labels, currencies).
- [x] Sliders/toggles/tabs/segmented components with states; typography + hex; materials with hex/finish.
- [x] Animation, FX, events, negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.
