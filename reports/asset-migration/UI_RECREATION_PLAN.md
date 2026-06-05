# BULWARK — UI Recreation Plan (Phase C.5)

**Date:** 2026-06-05 · Unity **uGUI** Canvas architecture for the presentation pass. **Goal: professional
*temporary* presentation, NOT a pixel-perfect Stick War clone** — clean, readable, mobile-correct, original
BULWARK layout/branding using placeholder sprites. Input: `PRESENTATION_ARCHITECTURE.md`. TMP = TextMeshPro.

## 1. Global Canvas + scaling (mobile-first)
- **Root Canvas:** `Screen Space - Overlay`, `pixelPerfect=false`, sorting on top of the battlefield.
- **CanvasScaler:** `Scale With Screen Size`, **reference 1080×2400 (portrait)**, `matchWidthOrHeight = 0.5`
  (balanced), so layouts scale across phones/tablets. (Device is 1080×2408 → ~1:1.)
- **Safe area:** a `SafeAreaFitter` MonoBehaviour sets a `SafeArea` RectTransform from `Screen.safeArea` each
  enable (notch/nav-bar correct on the Redmi + others); all screens parent under `SafeArea`.
- **EventSystem:** standalone + touch input module (uGUI handles touch as pointer — tappable on device).
- **Typography:** **Roboto (Apache-2.0)** as the one TMP SDF font (Regular + Medium); sizes: Title 64, H1 44,
  Body 30, Button 32, Numeric/HUD 28 (all in reference px, scaler-driven). Outline/shadow for battlefield legibility.
- **Color/identity:** BULWARK palette — Iron Pact steel/cobalt, Ashen ember/oxblood — applied as TMP/Image tints
  (faction-color readability is canon-locked §6); placeholder art tinted toward these, not Stick War's palette.

## 2. Reusable widget prefabs (build once, reuse)
| Prefab | Composition | Notes |
|---|---|---|
| `BtnPrimary` | `Image`(9-slice button) + TMP label + `Button` | states via sprite swap (normal/pressed/disabled); 9-slice borders set at import |
| `BtnIcon` | `Image`(round button) + `Image`(icon) + `Button` | HUD action buttons (train/advance/pause) |
| `Panel` | `Image`(9-slice panel) + `VerticalLayoutGroup` + padding | modal/menu container |
| `Card` | `Image`(frame) + `Image`(art) + TMP title + `Button` | mode cards, unit cards |
| `StatBar` | `Image`(bg) + `Image`(fill, type=Filled Horizontal) + TMP | statue/unit HP, build progress (SM bars) |
| `CurrencyChip` | `Image`(icon) + TMP | gold/gem top-bar readout |
| `Modal` | full-screen dim `Image`(α0.6) + `Panel` + Close `BtnIcon` | Settings/Profile |
| `Toast` | `Panel` + TMP (auto-fade) | transient messages |

## 3. Per-screen canvas trees (concise)
```
Splash:    [bg Image][BULWARK TMP title][tagline TMP]        (auto-advance 1.5s; fade-out)
Loading:   [bg Image][spinner Image (rotate)][progress TMP]
MainMenu:  [bg Image]
           ├─ TopBar (HorizontalLayout): [CurrencyChip gold][CurrencyChip gem][Settings BtnIcon]
           ├─ Title (BULWARK)
           └─ MenuButtons (VerticalLayout): [Play BtnPrimary][Profile BtnPrimary][Settings BtnPrimary]
ModeSelect:[bg][Back BtnIcon][Title "Choose Mode"]
           └─ Cards (HorizontalLayout): [Card Classic][Card Tournament][Card Endless]
Settings:  Modal → Panel(VerticalLayout): [Music StatBar+slider][SFX slider][toggles][Close]
Profile:   Modal → Panel: [commander portrait][faction crest][stats TMP (read-only)][Close]
BattleHUD: (non-blocking, anchored)
           ├─ TopBar: [gold CurrencyChip][IronPact StatBar (statue HP)][Ashen StatBar][timer TMP][Pause BtnIcon]
           ├─ BottomBar (HorizontalLayout): [Train BtnIcon ×roster][Advance BtnPrimary]
           └─ QueueStrip: [queue chips]   (mirrors SimPlayerHud info, textured)
EndScreen: Modal dim → [bg Victory/Defeat][Banner TMP][summary TMP][Rematch BtnPrimary][Menu BtnPrimary]
```

## 4. Animations & transitions (lightweight, no DOTween dependency)
- **Screen transitions:** CanvasGroup alpha fade (0↔1, ~0.2 s) via a small coroutine tweener; optional slide for
  modals (anchoredPosition). No third-party tween lib (keep deps minimal / Android-safe).
- **Button feedback:** uGUI `Button` color/sprite transition (pressed sprite) + optional scale punch.
- **Spinner:** continuous `Z` rotation. **HUD bars:** lerp `fillAmount` toward the read-only target each frame.
- **Victory/Defeat:** banner scale-in + stinger SFX.
- **No Spine UI animation this pass** (deferred); chest-open/menu-hero animation is future.

## 5. Layout groups & responsiveness
- Use `HorizontalLayoutGroup`/`VerticalLayoutGroup` + `LayoutElement` + `ContentSizeFitter` so bars/cards/buttons
  reflow across aspect ratios. Anchor HUD bars to screen edges (top/bottom), menus centered. Avoid absolute pixel
  positions except backgrounds. Buttons ≥ 96 px tap targets (mobile ergonomics).

## 6. Authoring approach (no Unity editor available → YAML/prefab + code)
- Build Canvas + widget prefabs as Unity prefab/scene YAML, **or** construct the Canvas hierarchy **in code** at
  runtime (a `UiFlow` MonoBehaviour that builds panels from loaded sprites/TMP) — **code-built UI is lower-risk
  here** (no fragile prefab YAML refs; mirrors how `SimPlayerHud` already builds UI in code, but uGUI instead of
  IMGUI). Sprites loaded from `Assets/_Game/Art/_Placeholder/` (Resources) by name. This is the recommended Phase-E
  path: a code-driven uGUI builder for reliability, refined to prefabs later.

## 7. Explicitly NOT doing (scope guard)
- Not cloning Stick War screens pixel-for-pixel; not implementing mode *rules* (gameplay, deferred); not Spine UI;
  not final branding/cosmetics; not shipping ripped assets. Keep the whole UI layer removable + clearly placeholder.
