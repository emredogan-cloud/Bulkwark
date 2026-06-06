// BULWARK — REUSABLE MODALS (UI Implementation · WP-11). Presentation-only, REMOVABLE.
//
// The reusable modal overlays (design/ConfirmModalDesign.png 4-in-1 sheet + RewardGrantDesign + NetworkError):
// Confirm, Reward, Insufficient-currency, Network-error. Built directly on the UiRouter canvas as dim-scrim
// overlays that FLOAT over the current screen (they do NOT use the screen stack, so the screen beneath stays
// visible but is click-blocked by the scrim). A simple toast already lives on UiRouter.Toast. Presentation-only;
// NO ECS/gameplay/backend.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-11 reusable modal overlays. Presentation-only.</summary>
    public static class UiModals
    {
        private static Transform Root => UiRouter.Instance.transform;

        // Build a dim-scrim modal root + a centered panel. Returns (overlayRoot, panelTransform).
        private static void Scaffold(float w, float h, out GameObject root, out Transform panel)
        {
            root = new GameObject("Modal", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rt = (RectTransform)root.transform;
            rt.SetParent(Root, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var scrim = root.GetComponent<Image>(); scrim.color = new Color(0f, 0f, 0f, 0.6f); scrim.raycastTarget = true; // block underlying
            panel = UiWidgets.Panel(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h), UiWidgets.PanelCol).transform;
        }

        /// <summary>Yes/No confirm dialog.</summary>
        public static void Confirm(string message, UnityAction onConfirm, string confirmLabel = "CONFIRM")
        {
            Scaffold(900, 480, out var root, out var p);
            UiWidgets.LabelAt(p, "CONFIRM", 44, new Vector2(0.5f, 0.80f), new Vector2(820, 70), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(p, message, 30, new Vector2(0.5f, 0.54f), new Vector2(800, 130), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(p, confirmLabel, new Vector2(0.30f, 0.17f), new Vector2(0.30f, 0.17f), Vector2.zero, new Vector2(340, 96), UiWidgets.IronBlue, () => { Object.Destroy(root); onConfirm?.Invoke(); }, 32);
            UiWidgets.Button(p, "CANCEL", new Vector2(0.70f, 0.17f), new Vector2(0.70f, 0.17f), Vector2.zero, new Vector2(340, 96), UiWidgets.Grey, () => Object.Destroy(root), 32);
        }

        /// <summary>"You received" reward popup.</summary>
        public static void Reward(string rewardLine, UnityAction onCollect = null)
        {
            Scaffold(900, 560, out var root, out var p);
            UiWidgets.LabelAt(p, "REWARD!", 52, new Vector2(0.5f, 0.80f), new Vector2(820, 80), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(p, rewardLine, 34, new Vector2(0.5f, 0.52f), new Vector2(800, 130), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(p, "COLLECT", new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(400, 100), UiWidgets.IronBlue, () => { Object.Destroy(root); onCollect?.Invoke(); }, 34);
        }

        /// <summary>Insufficient-gems prompt with a "Buy More" → Store route.</summary>
        public static void Insufficient(int needed, UnityAction onBuy = null)
        {
            Scaffold(900, 520, out var root, out var p);
            UiWidgets.LabelAt(p, "INSUFFICIENT GEMS", 44, new Vector2(0.5f, 0.78f), new Vector2(820, 70), TextAnchor.MiddleCenter, UiWidgets.Gem);
            UiWidgets.LabelAt(p, "You need " + Mathf.Max(0, needed) + " more gems to complete this purchase.", 28, new Vector2(0.5f, 0.52f), new Vector2(820, 120), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(p, "BUY MORE", new Vector2(0.30f, 0.16f), new Vector2(0.30f, 0.16f), Vector2.zero, new Vector2(340, 96), UiWidgets.Gold, () => { Object.Destroy(root); if (onBuy != null) onBuy(); else UiRouter.Instance.Show<StoreScreen>(); }, 32);
            UiWidgets.Button(p, "CANCEL", new Vector2(0.70f, 0.16f), new Vector2(0.70f, 0.16f), Vector2.zero, new Vector2(340, 96), UiWidgets.Grey, () => Object.Destroy(root), 32);
        }

        /// <summary>Connection-lost modal with Retry / Main Menu.</summary>
        public static void NetworkError(UnityAction onRetry)
        {
            Scaffold(980, 540, out var root, out var p);
            UiWidgets.LabelAt(p, "CONNECTION LOST", 46, new Vector2(0.5f, 0.78f), new Vector2(900, 70), TextAnchor.MiddleCenter, UiWidgets.AshRed);
            UiWidgets.LabelAt(p, "The connection to the server was lost.\nPlease check your network and try again.", 28, new Vector2(0.5f, 0.50f), new Vector2(880, 150), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(p, "RETRY", new Vector2(0.30f, 0.15f), new Vector2(0.30f, 0.15f), Vector2.zero, new Vector2(340, 96), UiWidgets.IronBlue, () => { Object.Destroy(root); onRetry?.Invoke(); }, 32);
            UiWidgets.Button(p, "MAIN MENU", new Vector2(0.70f, 0.15f), new Vector2(0.70f, 0.15f), Vector2.zero, new Vector2(340, 96), UiWidgets.Grey, () => { Object.Destroy(root); UiRouter.Instance.Replace<MainMenuScreen>(); }, 32);
        }
    }
}
