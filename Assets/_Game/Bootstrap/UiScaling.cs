// BULWARK — UI SCALING (UI Implementation · WP-00 Landscape Foundation). Presentation-only, REMOVABLE.
//
// Single source of truth for the LANDSCAPE CanvasScaler configuration, so every uGUI canvas (the UiFlow menu
// canvas, the BattleHud, and every WP-01+ screen built on the UiRouter shell) scales identically. This codifies
// the UI Production Freeze decision D1 (reports/ui-production/02_UI_FINAL_DECISIONS.md): the app is landscape;
// reference 2340x1080 (~19.5:9, the device's 1080x2408 inverted); matchWidthOrHeight = 1.0 (match HEIGHT — the
// stable/constrained axis in landscape). It replaces the prior per-canvas portrait constants (1080x2400, 0.5).
//
// NO gameplay/ECS/balance/AI/economy impact: this only configures uGUI CanvasScalers (presentation §12).
// Deleting this file and reverting the two call sites (UiFlow.BuildCanvas, BattleHud.BuildCanvas) fully restores
// the prior behavior.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Landscape UI scaling constants + a CanvasScaler configurator (WP-00 foundation).</summary>
    public static class UiScaling
    {
        /// <summary>Landscape reference width in px (~19.5:9 with <see cref="ReferenceHeight"/>).</summary>
        public const float ReferenceWidth = 2340f;

        /// <summary>Landscape reference height in px (the 1080 short edge the art was tuned against).</summary>
        public const float ReferenceHeight = 1080f;

        /// <summary>Match HEIGHT (1.0): in landscape, height is the stable axis across devices.</summary>
        public const float MatchWidthOrHeight = 1.0f;

        /// <summary>The landscape reference resolution as a Vector2.</summary>
        public static Vector2 ReferenceResolution => new Vector2(ReferenceWidth, ReferenceHeight);

        /// <summary>Configure a <see cref="CanvasScaler"/> for the frozen landscape target
        /// (ScaleWithScreenSize, 2340x1080, match height). Safe to call on any canvas's scaler.</summary>
        public static void Configure(CanvasScaler scaler)
        {
            if (scaler == null) return;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
        }
    }
}
