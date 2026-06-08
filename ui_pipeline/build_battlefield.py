#!/usr/bin/env python3
"""STICK EMPIRE RISE — PHASE 4 BATTLEFIELD PARALLAX LAYER ASSETS.

Generates, per biome, the UI-free 2D side-view parallax layers (Report 06 / Phase-4 matrix):
  bf_<biome>_sky.jpg       L1 SKY      — wide gradient + sun/moon
  bf_<biome>_horizon.png   L2 HORIZON  — distant silhouettes (castles/mountains/crags), transparent above
  bf_<biome>_mid.png       L3 MIDGROUND— hills / forest line / structures, transparent above
  bf_<biome>_ground.png    L4 GROUND   — the playfield ground strip (units walk on this)
  bf_<biome>_fg.png        L5 FOREGROUND— near props (grass/rocks), bottom-anchored, transparent above
Layers are WIDE (2400px) so the camera can pan/parallax without revealing edges. No realtime lights — mood is
painted in (Phase-4 atmospheric rule). Pure code art, no external/ripped assets. Output → StreamingAssets/bulwark_ui/.
"""
import os
from PIL import Image, ImageDraw, ImageFilter

OUT = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"
os.makedirs(OUT, exist_ok=True)
SS = 2
W = 2400  # wide enough for camera pan + parallax

def lerp(a, b, t): return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))

def vgrad(w, h, top, bot):
    im = Image.new("RGB", (w, h)); px = im.load()
    for y in range(h):
        c = lerp(top, bot, y / max(1, h - 1))
        for x in range(w): px[x, y] = c
    return im

# Biome palettes: (sky_top, sky_bot, horizon_col, mid_col, ground_top, ground_bot, prop_col, sun, sun_col)
BIOMES = {
    "grass":   ((96,150,210),(176,200,170),(70,86,80),(54,84,46),(86,116,54),(54,78,38),(40,58,30),(0.30,0.18,210),(255,240,180)),
    "ash":     ((40,26,30),(110,52,38),(46,30,34),(40,24,24),(54,34,30),(30,18,18),(24,14,14),(0.62,0.30,170),(255,140,60)),
    "snow":    ((150,178,205),(210,224,232),(120,140,160),(150,168,184),(196,210,220),(150,168,182),(120,140,156),(0.40,0.20,150),(255,250,240)),
    "volcanic":((44,22,20),(150,52,28),(40,22,20),(34,18,16),(60,30,22),(28,14,12),(70,26,16),(0.50,0.34,150),(255,120,40)),
    "dead":    ((58,62,66),(120,118,110),(56,58,60),(46,48,50),(64,64,60),(40,40,40),(34,34,34),(0.45,0.22,120),(200,200,190)),
}

def jag(d, w, h, base_y, col, amp, n, seed):
    pts = [(0, h)]
    for i in range(n + 1):
        x = int(i / n * w)
        hh = int(amp * (0.4 + 0.6 * (((i * 131 + seed * 71) % 100) / 100.0)))
        pts.append((x, base_y - hh))
    pts.append((w, h)); d.polygon(pts, fill=col)

def crenel(d, w, h, base_y, col, towers, seed):
    wall_h = int(h * 0.5)
    d.rectangle([0, base_y, w, base_y + wall_h], fill=col)
    bw = int(w * 0.018)
    for x in range(0, w, bw * 2): d.rectangle([x, base_y - int(wall_h * 0.18), x + bw, base_y], fill=col)
    for t in range(towers):
        tx = int((t + 0.5) / towers * w); tw = int(w * 0.03)
        th = int(wall_h * (1.4 + 0.5 * (((t * 53 + seed) % 10) / 10.0)))
        d.rectangle([tx - tw, base_y - th, tx + tw, base_y], fill=col)
        d.polygon([(tx - tw - 4, base_y - th), (tx + tw + 4, base_y - th), (tx, base_y - th - int(th * 0.35))], fill=col)

for name, (skt, skb, hzc, mdc, grt, grb, prc, (sunx, suny, sunr), sunc) in BIOMES.items():
    # L1 SKY (jpg) + sun glow
    h = 600; im = vgrad(W, h, skt, skb)
    g = Image.new("RGBA", (W, h), (0, 0, 0, 0)); gd = ImageDraw.Draw(g)
    sx, sy, sr = int(sunx * W), int(suny * h), int(sunr * h)
    gd.ellipse([sx - sr, sy - sr, sx + sr, sy + sr], fill=sunc + (160,))
    g = g.filter(ImageFilter.GaussianBlur(sr * 0.5))
    im = Image.alpha_composite(im.convert("RGBA"), g).convert("RGB")
    im.save(os.path.join(OUT, f"bf_{name}_sky.jpg"), quality=85)

    # L2 HORIZON (png, transparent above the silhouette)
    h = 420; im = Image.new("RGBA", (W, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    if name in ("grass", "ash"): crenel(d, W, h, int(h * 0.45), hzc + (255,), 5, 7)
    else: jag(d, W, h, int(h * 0.55), hzc + (255,), int(h * 0.45), 14, 3)
    im = im.filter(ImageFilter.GaussianBlur(1.5))  # distance haze
    im.save(os.path.join(OUT, f"bf_{name}_horizon.png"))

    # L3 MIDGROUND (png) — hills/forest line + a keep silhouette each side
    h = 460; im = Image.new("RGBA", (W, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    jag(d, W, h, int(h * 0.62), mdc + (255,), int(h * 0.34), 20, 11)
    for kx in (int(W * 0.10), int(W * 0.90)):  # flanking keeps
        kw = int(W * 0.035); kh = int(h * 0.5)
        d.rectangle([kx - kw, h - kh, kx + kw, h], fill=lerp(mdc, (0, 0, 0), 0.25) + (255,))
        d.polygon([(kx - kw - 6, h - kh), (kx + kw + 6, h - kh), (kx, h - kh - int(kh * 0.3))], fill=lerp(mdc, (0, 0, 0), 0.25) + (255,))
    im.save(os.path.join(OUT, f"bf_{name}_mid.png"))

    # L4 GROUND (png) — playfield ground strip (opaque)
    h = 240; im = vgrad(W, h, grt, grb).convert("RGBA")
    d = ImageDraw.Draw(im, "RGBA")
    for i in range(60):  # speckle texture
        x = (i * 197) % W; y = (i * 89) % h
        d.ellipse([x, y, x + 6, y + 4], fill=lerp(grt, grb, 0.5) + (60,))
    im.save(os.path.join(OUT, f"bf_{name}_ground.png"))

    # L5 FOREGROUND (png) — near props (rocks/tufts), bottom-anchored, slightly blurred
    h = 300; im = Image.new("RGBA", (W, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for i in range(26):
        x = int((i + 0.5) / 26 * W) + (i * 53 % 40); base = h
        if name == "grass":
            for b in range(5): d.line([(x + b * 5, base), (x + b * 5 - 6, base - 60 - (i * 7 % 30))], fill=prc + (255,), width=4)
        else:
            rw = 30 + i * 3 % 40; rh = 26 + i * 5 % 30
            d.ellipse([x - rw, base - rh, x + rw, base + rh], fill=prc + (255,))
    im = im.filter(ImageFilter.GaussianBlur(2.5))  # near-DoF blur
    im.save(os.path.join(OUT, f"bf_{name}_fg.png"))
    print(f"biome {name}: sky+horizon+mid+ground+fg")
print("OK")
