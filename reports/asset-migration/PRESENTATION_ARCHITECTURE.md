# BULWARK — Presentation Architecture (Phase C)

**Date:** 2026-06-05 · Goal: the app must **feel like a real game** — Splash → Main Menu → Mode Select → Match →
Victory/Defeat. Presentation-layer only (no ECS/sim/balance/AI/economy/GATE-1 change). Placeholder assets are ©
ripped (replace before ship). Inputs: `BULWARK_ASSET_MAPPING.md`, `ASSET_GAP_ANALYSIS.md`.

## 0. Architecture model
- **One persistent bootstrap scene** drives flow via an additive screen stack. Two implementation options:
  - **(Chosen) Single MainScene + a `UiFlow` controller** that shows/hides uGUI Canvas panels (one root Canvas,
    child panels per screen) and starts/stops the ECS battle. Lowest risk: keeps the existing MainScene + ECS
    bootstrap; no multi-scene loading complexity; the battle is already in MainScene.
  - (Alt) Separate scenes per screen (Splash/Menu/Battle) loaded additively — cleaner long-term but heavier to
    author via YAML now. Deferred.
- **Battle visuals** come from the existing `SimProxyRenderer` seam upgraded to sprites (see Phase E). **Menus/HUD**
  come from a new uGUI layer (`UI_RECREATION_PLAN.md`). The IMGUI debug overlays remain available behind a debug
  toggle but are hidden by default once the uGUI HUD exists.
- **State machine:** `Splash → (Loading) → MainMenu → ModeSelect → [Classic|Tournament|Endless] → Battle(HUD) →
  Victory|Defeat → MainMenu`. Settings/Profile are modal overlays reachable from MainMenu.

## 1. Screen-by-screen
For each: **Asset source · Modifications · Prefab hierarchy · Scene/flow placement · Implementation path.**

### Splash
- **Asset:** full-screen bg (SW `Backgrounds/BackgroundLoading` or SM `Img_Bg_Loading_Low`) + **BULWARK wordmark**
  (BUILD placeholder text — **no Stick War logo**). Optional: SW menu-hero Spine later.
- **Modifications:** crop/letterbox to safe area; replace any source branding.
- **Prefab:** `Splash` panel = `Image(bg)` + `Text(BULWARK)` + auto-advance timer.
- **Flow:** first panel; 1.5 s → Loading/MainMenu.
- **Path:** `UiFlow.ShowSplash()` → coroutine → `ShowMainMenu()`.

### Loading
- **Asset:** loading bg + SW `Menus/loading-circle` spinner.
- **Mods:** rotate spinner; progress text.
- **Prefab:** `Loading` panel = `Image(bg)` + `Image(spinner, rotating)` + `Text(%)`.
- **Flow:** shown while the ECS world/bootstrap initializes before a match.
- **Path:** gate on `World/MatchState` ready (read-only) then reveal Battle HUD.

### Main Menu
- **Asset:** menu bg + SW `Menus` titlebar/banner + SW/SM **buttons**; Roboto font; gold/gem icons (SW `UI/Icons`).
- **Mods:** 9-slice buttons; re-label (Play / Modes / Profile / Settings).
- **Prefab:** `MainMenu` = bg `Image` + `Title` + vertical `Button` group (Play→ModeSelect, Profile, Settings) +
  top-bar currency display (gold/gem icon + TMP).
- **Flow:** hub.
- **Path:** buttons wired to `UiFlow` transitions.

### Mode Select (Classic / Tournament / Endless)
- **Asset:** SW `Menus` panels/banners as mode cards; mode icons (SW `UI/Icons`).
- **Mods:** 3 cards with title + art + Play.
- **Prefab:** `ModeSelect` = back button + horizontal `LayoutGroup` of 3 `ModeCard` prefabs (Image+Title+Button).
- **Flow:** from MainMenu; any card → start a Battle (all three currently launch the same ECS match; mode rules are
  gameplay — **not** added here, presentation-only labels).
- **Path:** `UiFlow.StartMatch(mode)` → hide menu Canvas → start battle (the ECS sim already runs in MainScene).

### Classic / Tournament / Endless (mode framing)
- **Asset:** reuse Battle HUD; a mode banner.
- **Mods:** label only (no rule change — gameplay is deferred/unchanged).
- **Flow:** thin wrappers over the same Battle.

### Profile / Settings (modal overlays)
- **Asset:** SW `Menus` panel; AOW `Sliders`/`Toggles` (volume, sfx, music); commander portrait (AOW Generals).
- **Mods:** 9-slice panel; slider/toggle setup.
- **Prefab:** `SettingsModal` = dim `Image` + panel + sliders/toggles + Close; `ProfileModal` = panel + portrait +
  stats text (read-only).
- **Flow:** modal from MainMenu.
- **Path:** toggle audio volume on the AudioSource (presentation-only).

### Battle HUD
- **Asset:** SM **bars** (statue HP, gold/build), SW/SM **buttons** (train, advance, pause), SW **icons** (gold,
  unit roles), Roboto. Replaces the IMGUI `SimPlayerHud`/`SimDebugOverlay` (kept behind a debug toggle).
- **Mods:** wire to the **same control writes** the IMGUI HUD uses (read-only display + `Training.EnqueueTrain` /
  `MoveDestination` — no new gameplay).
- **Prefab:** `BattleHUD` = top bar (gold icon+TMP, both statue-HP bars, match timer) + bottom bar (per-role train
  buttons, Advance, Pause) + a units/queue readout.
- **Flow:** active during the match (atop the sprite battlefield).
- **Path:** a `BattleHudController` MonoBehaviour reads the ECS world read-only (like `SimDebugOverlay`) and routes
  button clicks through the existing control helpers (like `SimPlayerHud`). **No sim/balance change.**

### Victory / Defeat
- **Asset:** SW `Backgrounds/BackgroundVictory` / `BackgroundDefeat`; SW/SM **win/lose music** stingers; SW
  banner + button.
- **Mods:** re-theme; "Victory"/"Defeat" TMP.
- **Prefab:** `EndScreen` = dim + bg + banner(`Victory`/`Defeat`) + summary text (read from `MatchState`/stats,
  read-only) + buttons (Rematch→StartMatch, Menu→MainMenu).
- **Flow:** shown when `MatchState.Outcome != Ongoing` (the renderer/HUD already reads this).
- **Path:** `BattleHudController` watches `MatchState`; on decided → `UiFlow.ShowEnd(outcome)` + play stinger.

## 2. Scene hierarchy (target MainScene)
```
MainScene
├── Camera (orthographic battlefield cam — existing, SimProxyRenderer-configured)
├── BattleEnvironment (SubScene holder — existing; optional parallax bg quad added behind z=0)
├── Bootstrap (BattleBootstrap — existing, builds the ECS world)
├── [NEW] SpriteRenderRoot (parent for SimProxyRenderer's sprite proxies)  ← Phase E
└── [NEW] UI
    └── Canvas (Screen Space - Overlay, CanvasScaler 1080x2400 ref, safe-area)
        ├── Splash / Loading / MainMenu / ModeSelect / Settings / Profile (panels)
        ├── BattleHUD (panel)
        └── EndScreen (panel)
    └── EventSystem
    └── AudioRoot (AudioSource: music + sfx)  ← Phase E
```

## 3. Implementation path (summary; detail in Phase E)
1. Sprite battlefield via `SimProxyRenderer` (units/statue/mine sprites). 2. uGUI Canvas + `UiFlow` + screens.
3. `BattleHudController` (textured HUD over the same control writes). 4. Audio hooks. 5. Hide IMGUI behind a debug
toggle. **No change to any ECS system, balance, AI, economy, or the deferred GATE-1 bugs.**

## 4. Risks / constraints
- uGUI authored via YAML/prefab without the editor is fiddly (Canvas/RectTransform/refs) — start minimal, validate
  on device early. Sprite import needs correct `.meta` (Sprite type, pivot, PPU). Mode rules are gameplay → not
  implemented (labels only). All assets placeholder/ripped → replace before ship. Keep everything removable.
