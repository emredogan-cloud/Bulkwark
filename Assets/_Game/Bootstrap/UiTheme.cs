// BULWARK — UI THEME (UI Construction Bible · GLOBAL VISUAL DNA). Presentation-only, REMOVABLE.
//
// The single source of truth for the Bible's exact palette (00 §6 GLOBAL VISUAL DNA + per-screen Section G
// material samples) and typography scale. Every screen inherits these so the whole UI shares one identity:
// near-black obsidian bases; brushed gold / antique bronze chrome; royal/cobalt blue (Iron Pact + CTAs) vs
// ember/oxblood red (Ashen + danger); violet/amethyst (magic/premium/gems); parchment cream detail panels.
// Colours are parsed from the Bible's hex values verbatim. NO gameplay/ECS impact (§12).

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>Exact Bible palette + typography scale (Global Visual DNA). Presentation-only.</summary>
    public static class UiTheme
    {
        private static Color H(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        /// <summary>A colour at a given alpha.</summary>
        public static Color A(Color c, float a) { c.a = a; return c; }

        // ---- Obsidian / charcoal bases (#0a0b0f–#14161e; field samples #151614–#181819, channel #060507–#121318) ----
        public static readonly Color Obsidian   = H("#0a0b0f");
        public static readonly Color Charcoal   = H("#14161e");
        public static readonly Color FieldDark  = H("#181819"); // plaque interior sample
        public static readonly Color FieldTop    = H("#26262b"); // subtle top-lit gradient partner for FieldDark
        public static readonly Color ChannelDark = H("#060507"); // recessed bar channel (empty)
        public static readonly Color ChannelMid  = H("#121318");
        public static readonly Color Vignette    = H("#05060a"); // vignette corner colour

        // ---- Brushed gold / antique bronze chrome ----
        public static readonly Color GoldHi     = H("#f0d27a"); // highlight
        public static readonly Color Gold       = H("#caa04a"); // mid (canonical gold)
        public static readonly Color GoldShadow = H("#6b5320"); // shadow
        public static readonly Color GoldFillHi = H("#ffe9a8"); // progress-fill top highlight
        public static readonly Color GoldFill   = H("#e9c24a"); // progress-fill body
        public static readonly Color GoldFillLo = H("#9a7320"); // progress-fill bottom shadow
        public static readonly Color Parchment  = H("#d9c79a"); // scroll/detail panels
        public static readonly Color ParchGold  = H("#e8d5a8"); // warm parchment-gold text
        public static readonly Color Ember      = H("#ffb04a"); // warm fire/glow core

        // ---- Iron Pact / royal-cobalt blue (also primary CTAs) ----
        public static readonly Color IronBlue   = H("#2b56c8"); // deep cobalt
        public static readonly Color IronBlueHi = H("#4f8bff"); // bright cobalt
        public static readonly Color IronSteel  = H("#2b2a35"); // cool steel-charcoal (cloak/armour)
        public static readonly Color IronBanner = H("#2b3a6a"); // muted royal-blue banner cloth

        // ---- Ashen Horde / ember-oxblood red (also danger/surrender) ----
        public static readonly Color Oxblood    = H("#7a1f1a"); // deep oxblood
        public static readonly Color Ember2     = H("#d8452b"); // bright ember-red
        public static readonly Color AshBanner  = H("#5a1c1a"); // dark blood banner cloth

        // ---- Violet / amethyst (magic / premium / gems) ----
        public static readonly Color Amethyst   = H("#5a2db0"); // deep violet
        public static readonly Color AmethystHi = H("#9e6bf0"); // bright amethyst (gems)

        // ---- Strokes / shadows ----
        public static readonly Color StrokeDark = H("#1a1206"); // thin dark text stroke
        public static readonly Color Shadow     = H("#000000");

        // ---- Panel fills (composited, semi-transparent over art) ----
        public static Color Panel      => A(H("#10111a"), 0.92f);
        public static Color PanelGlass => A(H("#0d0e16"), 0.78f);
        public static Color Scrim      => new Color(0f, 0f, 0f, 0.72f); // modal dim behind sheets

        // ---- Typography (cap-heights from the specs are converted to legacy-Text fontSize ≈ cap×1.34) ----
        // Authored against the 2340×1080 canvas; legacy Text fontSize lives in the same canvas units.
        public const int Display = 150; // hero wordmark (cap ≈110–195)
        public const int H1      = 72;  // screen titles
        public const int H2      = 52;  // section / card titles
        public const int H3      = 40;  // sub-headers
        public const int Body    = 32;  // body / labels
        public const int Small   = 26;  // captions / secondary
        public const int Tiny    = 22;  // micro-labels

        /// <summary>The wide letter-tracking the Bible specifies for UPPERCASE prestige titles (+12–16%):
        /// legacy Text has no tracking, so we space the string. Use sparingly on short UPPERCASE runs.</summary>
        public static string Track(string s, int spaces = 1)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sep = new string(' ', spaces);
            var sb = new System.Text.StringBuilder(s.Length * (1 + spaces));
            for (int i = 0; i < s.Length; i++) { sb.Append(s[i]); if (i < s.Length - 1) sb.Append(sep); }
            return sb.ToString();
        }
    }
}
