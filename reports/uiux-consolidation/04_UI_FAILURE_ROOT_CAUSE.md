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
