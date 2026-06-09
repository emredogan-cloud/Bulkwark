#!/usr/bin/env python3
"""STICK EMPIRE RISE — PHASE 7 CLEAN META-SCREEN PLATES (L0).

UI-FREE, TEXT-FREE atmospheric background plates for the secondary/meta screens, replacing the baked-UI
mockup backdrops (bd_*). Per-screen mood: moody vertical grade + soft focal glow + faint castle/skyline
silhouette + vignette — NO logos/buttons/text. Output → StreamingAssets/bulwark_ui/plate_<key>.jpg.
"""
import os
from PIL import Image, ImageDraw, ImageFilter

OUT = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"
SS = 2
def lerp(a, b, t): return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))
def hx(h): h = h.lstrip('#'); return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))

# key -> (top, bottom, glow(rgb or None), skyline?)
SCREENS = {
    "store":      ("#2a2016", "#0f0c08", "#d8a23c", False),
    "chestopen":  ("#1a1228", "#0a0810", "#9e6bf0", False),
    "rewardgrant":("#141620", "#0a0b10", "#e8c060", False),
    "leaderboard":("#1e2438", "#0c0e16", "#caa04a", True),
    "profile":    ("#161a26", "#0a0c12", "#caa04a", False),
    "clan":       ("#1c1a24", "#0c0a10", "#c89a4a", True),
    "quests":     ("#2a2418", "#100c08", "#caa04a", False),
    "daily":      ("#14203a", "#0a0e18", "#e8c060", False),
    "tournament": ("#16120e", "#080604", "#e0b050", True),
    "endlessres": ("#1c0e0a", "#0c0604", "#d8452b", False),
    "settings":   ("#16120a", "#0a0806", "#d8902c", False),
    "about":      ("#141620", "#0a0b10", "#caa04a", False),
}

def skyline(d, w, h, col):
    base = int(h * 0.72)
    for i, fx in enumerate([0.08, 0.22, 0.4, 0.58, 0.74, 0.9]):
        tw = int(w * 0.05); th = int(h * (0.10 + (i % 3) * 0.05)); x = int(fx * w)
        d.rectangle([x - tw, base - th, x + tw, base], fill=col)
        for cx in range(x - tw, x + tw, int(w * 0.018)):
            d.rectangle([cx, base - th - int(h * 0.02), cx + int(w * 0.01), base - th], fill=col)

def plate(key, top, bot, glow, sky):
    W, H = 1000, 462; w, h = W * SS, H * SS
    im = Image.new("RGB", (w, h)); px = im.load()
    t0, b0 = hx(top), hx(bot)
    for y in range(h):
        c = lerp(t0, b0, (y / (h - 1)) ** 1.2)
        for x in range(w): px[x, y] = c
    im = im.convert("RGBA"); d = ImageDraw.Draw(im, "RGBA")
    if sky:
        skyline(d, w, h, lerp(b0, (0, 0, 0), 0.4) + (255,))
    if glow:
        g = Image.new("RGBA", (w, h), (0, 0, 0, 0)); gd = ImageDraw.Draw(g)
        gc = hx(glow); gr = int(h * 0.55)
        gd.ellipse([w * 0.5 - gr, h * 0.18 - gr, w * 0.5 + gr, h * 0.18 + gr], fill=gc + (70,))
        im = Image.alpha_composite(im, g.filter(ImageFilter.GaussianBlur(gr * 0.45)))
    # vignette
    v = Image.new("L", (w, h), 0); ImageDraw.Draw(v).ellipse([-w * 0.2, -h * 0.2, w * 1.2, h * 1.2], fill=255)
    v = v.filter(ImageFilter.GaussianBlur(w * 0.11)).point(lambda p: int(255 - (255 - p) * 0.55))
    dark = Image.new("RGBA", (w, h), (0, 0, 0, 255))
    im = Image.composite(im, dark, v)
    im.convert("RGB").resize((W, H), Image.LANCZOS).save(os.path.join(OUT, "plate_" + key + ".jpg"), quality=84)

for k, (t, b, g, s) in SCREENS.items():
    plate(k, t, b, g, s)
print(f"meta plates: {len(SCREENS)}")
print("OK")
