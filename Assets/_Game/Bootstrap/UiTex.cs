// BULWARK — UI PROCEDURAL TEXTURES (UI Construction Bible implementation). Presentation-only, REMOVABLE.
//
// The Construction Bible's Material/FX sections call for elements no placeholder PNG provides: radial vignettes,
// warm additive glows (god-rays / fire / tip-glow), vertical gold gradient fills, ornate beveled gold frames,
// and finial ornaments. Rather than ship flat solid colours (a Negative-Rule violation on most screens), this
// generates those primitives as small cached Sprites at runtime (RGBA32, bilinear) — so code-built uGUI can hit
// the Material specs at ≥95% structural fidelity while final authored art is still pending (per each spec's
// Section N). All sprites are cached by key (generated once, reused everywhere). NO gameplay/ECS impact (§12).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>Runtime-generated cached uGUI sprites (gradients, glows, frames, finials). Presentation-only.</summary>
    public static class UiTex
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>(64);

        private static Sprite Make(string key, Texture2D tex, Vector4 border = default, float ppu = 100f)
        {
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply(false, false);
            var rect = new Rect(0, 0, tex.width, tex.height);
            var sp = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect, border);
            _cache[key] = sp;
            return sp;
        }

        private static string K(string p, params object[] a) { var s = p; foreach (var x in a) s += "_" + x; return s; }

        /// <summary>A 4×4 solid-colour sprite.</summary>
        public static Sprite Solid(Color c)
        {
            string k = K("solid", Hx(c));
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = c; t.SetPixels(px);
            return Make(k, t);
        }

        /// <summary>A vertical gradient (top → bottom), 1×h. Use as a stretched Image sprite.</summary>
        public static Sprite VGradient(Color top, Color bottom, int h = 64)
        {
            string k = K("vg", Hx(top), Hx(bottom), h);
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(1, h, TextureFormat.RGBA32, false);
            var px = new Color[h];
            for (int y = 0; y < h; y++) px[y] = Color.Lerp(bottom, top, y / (float)(h - 1));
            t.SetPixels(px);
            return Make(k, t);
        }

        /// <summary>A horizontal gradient (left → right), w×1.</summary>
        public static Sprite HGradient(Color left, Color right, int w = 64)
        {
            string k = K("hg", Hx(left), Hx(right), w);
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(w, 1, TextureFormat.RGBA32, false);
            var px = new Color[w];
            for (int x = 0; x < w; x++) px[x] = Color.Lerp(left, right, x / (float)(w - 1));
            t.SetPixels(px);
            return Make(k, t);
        }

        /// <summary>A radial sprite: <paramref name="inner"/> at centre → <paramref name="outer"/> at the edge
        /// (alpha interpolated too). Vignette = (transparent inner, dark outer); Glow = (bright inner, transparent
        /// outer). <paramref name="power"/> shapes the falloff (1=linear, &gt;1 concentrates toward centre).</summary>
        public static Sprite Radial(Color inner, Color outer, int size = 128, float power = 1.6f)
        {
            string k = K("rad", Hx(inner), Hx(outer), size, power.ToString("0.0"));
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float c = (size - 1) * 0.5f, maxD = c;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c, dy = y - c;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / maxD);
                    px[y * size + x] = Color.Lerp(inner, outer, Mathf.Pow(d, power));
                }
            t.SetPixels(px);
            return Make(k, t);
        }

        /// <summary>A 9-sliceable beveled frame border: gold molding ring with a transparent centre, so content
        /// shows through. <paramref name="border"/> px is the molding thickness (becomes the 9-slice border).</summary>
        public static Sprite Frame(Color hi, Color mid, Color shadow, int size = 64, int border = 12)
        {
            string k = K("frame", Hx(hi), Hx(mid), Hx(shadow), size, border);
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    Color col;
                    if (edge >= border) col = clear;                       // interior = transparent
                    else
                    {
                        float f = edge / (float)Mathf.Max(1, border - 1);  // 0 at outer rim → 1 at inner rim
                        // Beveled molding: bright outer rim → mid → dark inner rim (cast-gold relief).
                        col = f < 0.5f ? Color.Lerp(hi, mid, f * 2f) : Color.Lerp(mid, shadow, (f - 0.5f) * 2f);
                    }
                    px[y * size + x] = col;
                }
            t.SetPixels(px);
            return Make(k, t, new Vector4(border, border, border, border));
        }

        /// <summary>A filled diamond/lozenge finial ornament (gold), transparent background.</summary>
        public static Sprite Diamond(Color c, int size = 48)
        {
            string k = K("dia", Hx(c), size);
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float h = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float m = Mathf.Abs(x - h) / h + Mathf.Abs(y - h) / h; // |x|+|y| ≤ 1 = diamond
                    px[y * size + x] = m <= 1f ? c : new Color(0, 0, 0, 0);
                }
            t.SetPixels(px);
            return Make(k, t);
        }

        /// <summary>A filled disc (soft-edged) sprite — for star/jewel bosses, dots, bullets.</summary>
        public static Sprite Disc(Color c, int size = 48)
        {
            string k = K("disc", Hx(c), size);
            if (_cache.TryGetValue(k, out var s)) return s;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float cc = (size - 1) * 0.5f, r = cc - 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cc) * (x - cc) + (y - cc) * (y - cc));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * size + x] = new Color(c.r, c.g, c.b, c.a * a);
                }
            t.SetPixels(px);
            return Make(k, t);
        }

        private static string Hx(Color c) =>
            ((int)(c.r * 255) << 24 | (int)(c.g * 255) << 16 | (int)(c.b * 255) << 8 | (int)(c.a * 255)).ToString("x8");
    }
}
