// BULWARK — PRESENTATION STATE (Pre-Phase-5 final polish). Presentation-only; no gameplay effect.
//
// A tiny shared flag so the uGUI front-end (UiFlow) can hide the IMGUI DEBUG overlays
// (SimDebugOverlay + SimPlayerHud) while a menu/end screen is up — they otherwise draw on TOP of uGUI
// (OnGUI always renders above Canvas). The debug systems are NOT removed (observability preserved); only
// their on-screen visibility is gated. Default = NOT suppressed, so a debug-only run (no UiFlow present)
// still shows the overlays exactly as before.

namespace Bulwark.Bootstrap
{
    /// <summary>Shared visibility gate for the IMGUI debug overlays (presentation-only).</summary>
    public static class PresentationState
    {
        /// <summary>When true, SimDebugOverlay/SimPlayerHud skip their OnGUI draw. With the textured uGUI
        /// BattleHud in place, UiFlow keeps this true whenever the front-end is active (the IMGUI debug
        /// overlays are replaced on-screen; observability remains via logcat). Default false = shown, so a
        /// debug-only run without UiFlow still shows them.</summary>
        public static bool SuppressDebugHud;

        /// <summary>True only while the uGUI Match screen is active — gates the textured BattleHud (gold,
        /// statue-HP bars, troop-production buttons). Set by UiFlow. Default false.</summary>
        public static bool InMatch;
    }
}
