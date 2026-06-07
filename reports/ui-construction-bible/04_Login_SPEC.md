# BULWARK — UI CONSTRUCTION SPEC · 04 · Login / Auth
Source: design/LoginAuthDesign.png · 1672×941 (1.78:1 art; normalized to 2340×1080) · Analysis-only forensic spec.

> ⚠️ **OUT-OF-FLOW / DEPRECATED.** The finalized BULWARK boot is **NO-LOGIN**: Splash → Loading → **Main Menu directly** (Stick War Legacy onboarding simplicity; no account wall). `LoginAuthDesign.png` exists in `/design`, so per the **no-screen-skipped** rule it is fully spec'd here for completeness, **but it is not in the shipped boot chain.** Any implementation agent MUST treat this screen as optional/disabled: it must NOT be inserted between Loading and Main Menu. It may later be repurposed as an *optional* "Link Account" sheet reachable from Settings, but that is out of scope for the boot flow.

---

## A — SCREEN PURPOSE
A guest/social **account gate** offering frictionless "Play as Guest" plus optional social sign-in (Google / Facebook / Apple), with consent + legal acknowledgement.

- **What it is (as designed):** an ornate central parchment-and-gold panel headed **"WELCOME, WARRIOR"** containing a primary **PLAY AS GUEST** button, an **"OR CONTINUE WITH"** divider, three social-login rows (Google, Facebook, Apple), a "your progress is safe and secure" reassurance line, and a **ToS/Privacy consent checkbox**. Corner utilities: **Support** (bottom-left), **Language** (bottom-left), **Account Recovery** (bottom-right).
- **When it WOULD appear (if enabled):** between Loading and Main Menu, or as an optional account-link sheet. **In the finalized flow it does NOT appear.**
- **Emotional state to evoke:** safe, welcoming, prestigious arrival — "you belong here, jump in instantly" (guest is the bright primary; social is secondary).
- **What the player does (if shown):** tap **Play as Guest** to enter instantly, or pick a social provider; tick consent.

---

## B — VISUAL DNA (this screen atop the global baseline)
- **Mood/theme:** a heraldic "audience chamber" — the player is greeted at the gate by an ornate banner-draped gold frame, flanked by the two faction champions (blue knight left, red knight right).
- **Atmosphere:** warm hazy battlefield backdrop (out of focus) framing a crisp foreground panel; a **blue heraldic banner drapes over the panel's left/top shoulder**; an amethyst gem crowns the top of the frame.
- **Visual hierarchy:** (1) gem-crowned ornate frame + "WELCOME, WARRIOR" → (2) **PLAY AS GUEST** (brightest CTA, cobalt) → (3) the three social rows → (4) reassurance + consent → (5) corner utilities.
- **Color psychology:** cobalt Play-as-Guest = Iron Pact / primary action / trust; the social buttons keep each brand's canonical color (Google white, Facebook blue, Apple black) for instant recognition; gold frame = prestige welcome; amethyst gem = premium spark.
- **Material identity:** ornate cast-gold beveled frame with a violet gem finial; near-black panel interior; cloth banner drape; brand-accurate pill buttons with leading brand glyph; small bronze utility icons.
- **Lighting:** focal warm light on the panel; cool rim on the left knight, warm rim on the right; background vignette pushes attention to the panel.
- **Contrast philosophy:** Play-as-Guest is the single brightest interactive object; social rows are calmer; legal text is the quietest.

---

## C — SCREEN DECOMPOSITION (full node tree)
```
LoginScreen (UiScreen root, CanvasGroup)   ⚠️ DISABLED in boot flow
└── Root_SafeArea (RectTransform, SafeAreaFitter)
    ├── BG_Layer (full-bleed, IGNORES safe area)
    │   ├── KeyArt_Base            (Image: hazy battlefield w/ flanking knights baked)
    │   ├── Vignette_Overlay       (Image: radial dark vignette)
    │   └── Grain_Overlay          (Image: faint grain)                 [FX]
    ├── Panel_Group                (central ornate auth card)
    │   ├── BannerDrape            (Image: blue heraldic cloth over top-left shoulder)
    │   ├── Panel_Frame            (Image: ornate cast-gold beveled frame, 9-slice)
    │   ├── Panel_Field            (Image: near-black interior fill)
    │   ├── GemFinial_Top          (Image: amethyst gem + gold setting, top-center of frame)
    │   ├── Title_Welcome          (Text(TMP): "WELCOME, WARRIOR")
    │   ├── Btn_PlayGuest          (Button: cobalt, leading person icon + "PLAY AS GUEST")
    │   │   ├── GuestIcon          (Image: person/silhouette glyph)
    │   │   └── GuestLabel         (Text(TMP))
    │   ├── Divider_OrContinue     (row)
    │   │   ├── Divider_LineLeft   (Image: thin gold rule)
    │   │   ├── Divider_Label      (Text(TMP): "OR CONTINUE WITH")
    │   │   └── Divider_LineRight  (Image: thin gold rule)
    │   ├── Btn_Google             (Button: white pill)
    │   │   ├── GoogleIcon         (Image: G mark)
    │   │   └── GoogleLabel        (Text(TMP): "Continue with Google")
    │   ├── Btn_Facebook           (Button: Facebook-blue pill)
    │   │   ├── FacebookIcon       (Image: f mark)
    │   │   └── FacebookLabel      (Text(TMP): "Continue with Facebook")
    │   ├── Btn_Apple              (Button: black pill)
    │   │   ├── AppleIcon          (Image:  mark)
    │   │   └── AppleLabel         (Text(TMP): "Continue with Apple")
    │   ├── Reassurance_Row
    │   │   ├── ShieldIcon         (Image: small gold shield/lock)
    │   │   └── Reassurance_Label  (Text(TMP): "Your progress is safe and secure")
    │   └── Consent_Row
    │       ├── Consent_Checkbox   (Toggle: checked in mock)
    │       └── Consent_Label      (Text(TMP): "I have read and agree to the Terms of Service and Privacy Policy")
    │                                (with "Terms of Service" + "Privacy Policy" as link runs)
    ├── Util_SupportBtn            (Button: headset icon + "SUPPORT", bottom-left)
    ├── Util_LanguageBtn           (Button: globe icon + "LANGUAGE", bottom-left)
    └── Util_AccountRecoveryBtn    (Button: document icon + "ACCOUNT RECOVERY", bottom-right)
```
> Flanking knights are baked into `KeyArt_Base`. The `BannerDrape` is drawn as a foreground cloth element over the panel's shoulder.

---

## D — UNITY HIERARCHY SPECIFICATION
| Node | Parent | Order | Unity type | Anchor | Pivot | Alignment | Safe-area | Responsive |
|---|---|---|---|---|---|---|---|---|
| LoginScreen | Canvas | 0 | RectTransform + CanvasGroup | stretch-all | 0.5,0.5 | fill | n/a | fills canvas |
| Root_SafeArea | LoginScreen | 0 | RectTransform + SafeAreaFitter | stretch-all | 0.5,0.5 | fill | **insets** | holds panel + utils |
| BG_Layer | LoginScreen | 0 (behind) | RectTransform | stretch-all | 0.5,0.5 | fill | **IGNORES** | full-bleed |
| KeyArt_Base | BG_Layer | 0 | Image | stretch-all | 0.5,0.5 | fill | ignores | scale-to-cover |
| Vignette/Grain | BG_Layer | 1–2 | Image | stretch-all | 0.5,0.5 | fill | ignores | full-bleed |
| Panel_Group | Root_SafeArea | 0 | RectTransform | center | 0.5,0.5 | center | inside safe | scales w/ H; capped max width |
| BannerDrape | Panel_Group | 0 | Image | top-left (overhang) | 0.5,1 | upper-left | — | pinned to panel shoulder |
| Panel_Frame | Panel_Group | 1 | Image (9-slice) | stretch-all | 0.5,0.5 | center | — | 9-slice |
| Panel_Field | Panel_Group | 2 (behind frame edge) | Image | stretch-all (inset) | 0.5,0.5 | center | — | fills interior |
| GemFinial_Top | Panel_Group | 3 | Image | top-center (overhang) | 0.5,0.5 | center | — | pinned top edge |
| Content_VLayout | Panel_Field | 0 | VerticalLayoutGroup + padding | stretch-top | 0.5,1 | center | — | stacks rows |
| Title_Welcome | Content_VLayout | 0 | Text(TMP) | top-center | 0.5,0.5 | center | — | — |
| Btn_PlayGuest | Content_VLayout | 1 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full panel-width |
| Divider_OrContinue | Content_VLayout | 2 | RectTransform + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Btn_Google | Content_VLayout | 3 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Btn_Facebook | Content_VLayout | 4 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Btn_Apple | Content_VLayout | 5 | Button + HorizontalLayout | stretch-x | 0.5,0.5 | center | — | full width |
| Reassurance_Row | Content_VLayout | 6 | HorizontalLayout | center | 0.5,0.5 | center | — | — |
| Consent_Row | Content_VLayout | 7 | HorizontalLayout | top-left | 0,0.5 | left | — | — |
| Util_SupportBtn | Root_SafeArea | 1 | Button + vertical icon+label | bottom-left | 0,0 | center | inside safe | pinned bottom-left |
| Util_LanguageBtn | Root_SafeArea | 2 | Button + vertical icon+label | bottom-left | 0,0 | center | inside safe | right of Support |
| Util_AccountRecoveryBtn | Root_SafeArea | 3 | Button + vertical icon+label | bottom-right | 1,0 | center | inside safe | pinned bottom-right |

---

## E — LAYOUT MATHEMATICS (normalized to 2340×1080)
**Background:** scale-to-cover; bleeds under cutout. Flanking knights baked.

**Central panel.** Forensic bounds (normalized; source panel gold bbox fx 0.250–0.749, fy 0.032–0.947):
- **Panel outer:** width ≈ **0.42 × 2340 ≈ 980 px**, height ≈ **0.85 × 1080 ≈ 918 px** (a tall portrait card), centered horizontally; **top edge fy ≈ 0.04**, bottom edge fy ≈ 0.89 (the card nearly spans the full height).
- **Frame thickness:** ≈ 0.016 × 1080 ≈ 17 px beveled gold, with larger ornate corners (≈ 30 px) and a top-center gem finial overhanging ≈ 0.03 × 1080 ≈ 32 px above the top edge.
- **BannerDrape:** blue cloth draped over the top-left shoulder, overhanging the frame by ≈ 0.04 W to the left and ≈ 0.05 H above; it tucks behind the gem finial.
- **Content padding:** interior content inset ≈ 0.035 × panel-width left/right; rows stacked with ≈ 0.018 H vertical gaps.

**Row metrics (within the panel, normalized to screen):**
- **Title "WELCOME, WARRIOR":** centered, baseline fy ≈ 0.30, cap height ≈ 0.034 × 1080 ≈ 37 px.
- **Btn_PlayGuest (cobalt):** full content-width pill; vertical center fy ≈ 0.375; cobalt band spans fx ≈ 0.401–0.608 (≈ 0.21 W, ≈ 485 px); height ≈ 0.075 × 1080 ≈ 81 px (the tallest/primary button). Leading person icon at the left inset.
- **Divider "OR CONTINUE WITH":** centered, fy ≈ 0.47; gold rules flank the centered label.
- **Btn_Google (white):** full-width pill, vertical center fy ≈ 0.55 (white band fx 0.372–0.625); height ≈ 0.063 × 1080 ≈ 68 px.
- **Btn_Facebook (blue):** fy ≈ 0.62 (band fx 0.374–0.625); same height.
- **Btn_Apple (black):** fy ≈ 0.71; same height. (Social buttons share width fx ≈ 0.372–0.625 ≈ 0.253 W ≈ 592 px — slightly wider than the guest pill, with leading brand glyph + centered label.)
- **Reassurance_Row:** centered, fy ≈ 0.80, small gold shield + caption ≈ 0.020 × 1080 ≈ 22 px.
- **Consent_Row:** left-aligned within panel, fy ≈ 0.855; checkbox ≈ 0.026 × 1080 ≈ 28 px square (checked) + wrapping label with link runs.

**Corner utilities (outside the panel, on the background):**
- **Support:** bottom-left, icon center fx ≈ 0.045, fy ≈ 0.90; vertical icon-over-label, label ≈ 16 px.
- **Language:** to the right of Support, fx ≈ 0.135, fy ≈ 0.90.
- **Account Recovery:** bottom-right, fx ≈ 0.955, fy ≈ 0.90.
Each utility ≈ 0.05 × 1080 ≈ 54 px icon + small caption beneath.

**16:9 tablet:** panel keeps its proportional width (cap max ≈ 1040 px so it doesn't dominate on near-square); height-anchored; utilities pinned to safe corners. Background crops width.

**Ultrawide:** panel stays centered at fixed width; more background revealed; utilities pinned to safe-area corners (move inward with the safe inset).

**Notch behavior:** background bleeds under the cutout; panel is centered (clear of a side-notch); utilities sit inside `Root_SafeArea` so a side-notch pushes them inward rather than under the cutout.

---

## F — TYPOGRAPHY SPECIFICATION
- **Title "WELCOME, WARRIOR":** serif display, UPPERCASE, tracking +6%, gold bevel #f0d27a→#caa04a, soft glow + thin dark stroke; cap height ≈ **37 px**.
- **GuestLabel "PLAY AS GUEST":** semi-condensed bold sans **or** light serif, UPPERCASE, tracking +4%, near-white **#f4f6ff** with a thin dark shadow; cap height ≈ **30 px** (largest button label). Left-padded after the person icon, optically centered.
- **Divider_Label "OR CONTINUE WITH":** small caps, tracking +16%, muted parchment **#b9a877**, ≈ **18 px**.
- **Social labels "Continue with Google/Facebook/Apple":** clean medium sans (brand-style), sentence case, ≈ **24 px**. Colors: Google label dark **#3c4043** on white; Facebook label white **#ffffff** on blue; Apple label white **#ffffff** on black.
- **Reassurance "Your progress is safe and secure":** light sans, sentence case, muted gold/parchment **#c9b888**, ≈ **20 px**.
- **Consent label:** light sans, ≈ **18 px**, parchment grey **#b7ad95**; the runs **"Terms of Service"** and **"Privacy Policy"** are link-styled (cobalt **#4f8bff**, slightly brighter, underlined).
- **Utility captions (SUPPORT / LANGUAGE / ACCOUNT RECOVERY):** small caps, tracking +8%, muted gold ≈ **16 px**.
- **Hierarchy:** Title 37 > Guest 30 > social 24 > reassurance 20 > consent/divider/util 16–18.

---

## G — MATERIAL SPECIFICATION
- **Panel_Frame:** cast gold #f0d27a/#caa04a/#6b5320, beveled, ornate corners; top gem setting in gold.
- **GemFinial_Top:** faceted **amethyst** crystal #9e6bf0 core → #5a2db0 deep facets, inner glow + specular highlight, gold claw setting.
- **Panel_Field:** near-black **#1a1f29–#212630** (sampled interior #212630) with a subtle top-lit gradient and inner shadow from the frame.
- **BannerDrape:** royal/cobalt blue cloth #2b56c8→#1a347a with stitched gold trim and soft folds; faint cloth specular.
- **Btn_PlayGuest:** cobalt body #2b56c8→#1c3f9e with a brighter top sheen, gold/steel beveled rim, inner glow; the brightest button.
- **Btn_Google:** white #ffffff→#ececec, subtle grey 1-px border, soft drop shadow; multi-color G glyph.
- **Btn_Facebook:** Facebook blue #1877f2→#1057de, white f glyph.
- **Btn_Apple:** near-black #1c1c1e→#000000, subtle edge highlight, white  glyph.
- **ShieldIcon:** small gold shield/lock #caa04a.
- **Utility icons:** antique bronze #b08a3e line icons on transparent.
- **Vignette:** radial multiply → #06070b corners ≈ 55%.

---

## H — COMPONENT SPECIFICATION
**Btn_PlayGuest (primary CTA, cobalt):**
- Purpose: enter as guest instantly.
- States — Idle: cobalt with top sheen + soft glow; Hover: brighten +10%, glow widens; Pressed: dip −8% + scale 0.98 + brief inner flash; Disabled: desaturate to slate, glow off; Selected: n/a.
- Structure: leading person icon + centered UPPERCASE label on a beveled cobalt pill.

**Btn_Google / Btn_Facebook / Btn_Apple (social pills):**
- Purpose: OAuth with the respective provider.
- States — Idle: brand color; Hover: subtle elevation + +6% brightness; Pressed: scale 0.98 + slight dim; Disabled: 50% opacity; Selected: n/a.
- Structure: leading brand glyph (left-aligned) + label (centered or left-of-center per brand guidelines); brand-accurate colors must be preserved exactly.

**Consent_Checkbox (Toggle):**
- Purpose: gate sign-in on legal acknowledgement.
- States — Unchecked: empty gold-bordered box; Checked (mock state): gold/cobalt fill + check glyph; Hover: border brighten; Disabled: muted.
- Behavior: social/guest sign-in disabled until checked (per typical compliance; mock shows it checked).

**Util buttons (Support / Language / Account Recovery):**
- States — Idle: bronze icon + caption; Hover: icon brighten + caption glow; Pressed: scale 0.95.

---

## I — ANIMATION TIMELINE (entrance)
- **t=0.00:** CanvasGroup 0; panel scale 0.94, offset +20 px down.
- **t=0.00 → 0.45 s:** background fade in + slight Ken-Burns.
- **t=0.20 → 0.70 s:** Panel_Group scales/slides to 1.0 [ease-out-back]; gem finial catches a specular flash; banner drape settles with a tiny cloth ripple.
- **t=0.55 → 0.80 s:** Title fades/bevels in with a gold sweep.
- **t=0.70 → 1.10 s:** buttons cascade in top→bottom (Guest → divider → Google → Facebook → Apple), each fade+rise over ≈0.12 s, staggered ≈0.06 s [ease-out]; Guest gets an extra glow pulse to mark it primary.
- **t=1.10 → 1.30 s:** reassurance + consent fade in.
- **t=0.30 → 0.60 s (parallel):** corner utilities fade in.
- **Exit:** reverse-ish — panel scale 0.96 + fade 1→0 over 0.30 s.

---

## J — PARTICLE & FX BEHAVIOR (passive)
- **Gem sparkle:** the amethyst finial emits occasional tiny sparkles + a slow inner-glow breathe.
- **Guest button glow breathe:** the cobalt CTA's outer glow gently pulses to mark it primary.
- **Banner sway:** subtle cloth sway on the blue drape (≈ 4 s).
- **Background:** faint god-ray drift + grain; vignette steady.
- **Frame glint:** a slow specular glint travels the gold frame top every ≈ 6 s.
> No gameplay particles; all ambient.

---

## K — EVENT BEHAVIOR (player experience)
- **OnShow (if ever enabled):** entrance (I); FX (J); focus none (no keyboard).
- **OnPlayGuest:** if consent checked → enter; create/reuse a guest session (server-authoritative, stubbed) → route to Main Menu. If consent unchecked → nudge the checkbox (shake + highlight).
- **OnSocial(provider):** if consent checked → launch provider OAuth (stubbed) → on success route to Main Menu; on cancel/fail → return to this screen with a toast.
- **OnConsentToggle:** enable/disable sign-in buttons.
- **OnLink taps (ToS/Privacy):** open the respective document (web view / overlay).
- **OnSupport / OnLanguage / OnAccountRecovery:** open the respective utility flow.
- **OnHide:** stop FX; release art.
- **⚠️ Boot-flow note:** in the shipped flow none of the above fires because the screen is never pushed.

---

## L — NEGATIVE RULES (MUST NEVER)
- **MUST NEVER insert this screen between Loading and Main Menu** (no-login flow). If implemented, it is opt-in from Settings only.
- MUST NOT alter the brand colors/glyphs of Google/Facebook/Apple buttons (compliance + recognition).
- MUST NOT use default Unity Button/Toggle visuals or default font.
- MUST NOT demote "Play as Guest" below the social buttons in prominence — it is the brightest, largest, topmost action.
- MUST NOT omit the consent checkbox or the ToS/Privacy link runs.
- MUST NOT drop the gem finial, banner drape, vignette, or gem sparkle.
- MUST NOT let the panel exceed the safe area or the corner utilities sit under the cutout.
- MUST NOT swap the serif title to a sans.
- MUST NOT invent extra providers or fields (no email/password row — not in the mock).

---

## M — ACCEPTANCE CRITERIA
- **Fidelity ≥95%** vs LoginAuthDesign.png at 2340×1080: gem-crowned banner-draped gold panel, "WELCOME, WARRIOR", cobalt Play-as-Guest, OR-divider, three brand-accurate social pills, reassurance + consent, three corner utilities.
- **Out-of-flow respected:** the screen is NOT in the boot chain; verifiable by routing Loading → Main Menu with no auth stop.
- **Hierarchy preserved:** Guest brightest/largest; social calmer; legal quietest.
- **Brand integrity:** Google white / Facebook blue / Apple black exact.
- **Typography:** serif title; correct size ladder.
- **Safe area:** panel + utilities inside safe; background bleeds.
- **Animation:** entrance cascade + gem sparkle + guest glow present.
- **Affordance:** consent gates sign-in; unchecked nudges.

---

## N — IMPLEMENTATION CONFIDENCE
**88/100.** Layout/components are standard and measurable, and the cascade animation is easy. −12 because (a) it is **deprecated/out-of-flow**, so its real value is uncertain and an implementer must resist wiring it into boot; (b) brand-compliant social buttons + the ornate gem/banner panel art must be supplied; (c) live OAuth is stubbed/server-authoritative, so only the presentation is in scope here.

---

## O — SELF-CHECKLIST
- □ **Screen flagged DISABLED / not pushed in boot flow** (Loading → Main Menu verified to skip it).
- □ KeyArt_Base scale-to-cover w/ flanking knights; vignette + grain; bleeds under cutout.
- □ Central panel fx0.25–0.75, fy0.04–0.89, ornate gold frame + amethyst gem finial + blue banner drape.
- □ Title "WELCOME, WARRIOR" serif UPPERCASE gold, fy≈0.30.
- □ Btn_PlayGuest cobalt, fy≈0.375, largest/brightest, person icon + label.
- □ "OR CONTINUE WITH" divider with gold rules, fy≈0.47.
- □ Google(white)/Facebook(blue)/Apple(black) pills, brand-exact, fy≈0.55/0.62/0.71.
- □ Reassurance row (shield + caption) fy≈0.80; consent checkbox + ToS/Privacy links fy≈0.855.
- □ Corner utilities: Support + Language (bottom-left), Account Recovery (bottom-right), inside safe area.
- □ Entrance cascade + gem sparkle + guest glow breathe.
- □ Consent gates sign-in; unchecked → nudge.
- □ SafeAreaFitter on Root_SafeArea; BG_Layer ignores safe area.
- □ No default styling; no brand-color changes; no serif→sans swap; no email/password invented.
