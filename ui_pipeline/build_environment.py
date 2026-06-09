#!/usr/bin/env python3
"""STICK EMPIRE RISE — PHASE 5 ENVIRONMENT PROP SILHOUETTES.

Dark, transparent prop/structure silhouettes that BattlefieldEnvironment.cs places + tints per biome
(barracks/keeps/watchtower + banner; biome scatter props). Statue cracks/smoke/auras + resource glints are
procedural (UiTex) in code. Pure code art, no external assets. Output → StreamingAssets/bulwark_ui/ (env_*).
"""
import os
from PIL import Image, ImageDraw, ImageFilter

OUT = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"
os.makedirs(OUT, exist_ok=True)
SS = 3
DARK = (18, 18, 22, 255)
ACCENT = (210, 175, 90, 255)  # gold trim slot (tinted/used in code)

def canvas(w, h):
    im = Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0)); return im, ImageDraw.Draw(im)

def save(im, name, w):
    im = im.resize((w, int(im.height * w / im.width)), Image.LANCZOS)
    im.save(os.path.join(OUT, "env_" + name + ".png"))

def barracks():
    im, d = canvas(220, 160); W, H = im.size
    d.rectangle([W*0.12, H*0.40, W*0.88, H], fill=DARK)                       # body
    d.polygon([(W*0.06, H*0.42), (W*0.94, H*0.42), (W*0.5, H*0.10)], fill=DARK)  # roof
    d.rectangle([W*0.70, H*0.16, W*0.80, H*0.42], fill=DARK)                  # chimney
    d.rectangle([W*0.42, H*0.62, W*0.58, H], fill=(8, 8, 10, 255))            # door
    save(im, "barracks", 256)

def watchtower():
    im, d = canvas(120, 220); W, H = im.size
    d.rectangle([W*0.28, H*0.18, W*0.72, H], fill=DARK)                       # shaft
    d.rectangle([W*0.18, H*0.10, W*0.82, H*0.22], fill=DARK)                  # battlement
    for x in range(int(W*0.18), int(W*0.82), int(W*0.13)):
        d.rectangle([x, H*0.05, x + int(W*0.06), H*0.12], fill=DARK)          # crenels
    d.polygon([(W*0.30, H*0.40), (W*0.50, H*0.30), (W*0.50, H*0.46)], fill=(8, 8, 10, 255))  # window slit
    save(im, "watchtower", 160)

def banner():  # pole + cloth (cloth = top region, tinted by faction in code via a 2nd sprite slot)
    im, d = canvas(80, 200); W, H = im.size
    d.rectangle([W*0.46, H*0.04, W*0.54, H], fill=DARK)                       # pole
    d.polygon([(W*0.54, H*0.08), (W*0.95, H*0.12), (W*0.86, H*0.40), (W*0.54, H*0.36)], fill=ACCENT)  # cloth (tinted)
    save(im, "banner", 96)

def prop(name, draw_fn, w=140, h=110):
    im, d = canvas(w, h); draw_fn(d, im.size); save(im, "prop_" + name, 160)

def rock(d, s):
    W, H = s; d.ellipse([W*0.10, H*0.30, W*0.92, H*1.0], fill=(70, 70, 78, 255)); d.ellipse([W*0.30, H*0.16, W*0.78, H*0.62], fill=(92, 92, 100, 255))
def spear(d, s):
    W, H = s; d.line([(W*0.5, H*0.05), (W*0.42, H)], fill=DARK, width=int(8*SS)); d.polygon([(W*0.5, 0), (W*0.40, H*0.16), (W*0.60, H*0.16)], fill=(170, 170, 178, 255))
def debris(d, s):
    W, H = s
    for (x, y, r) in [(0.25, 0.7, 0.16), (0.55, 0.82, 0.2), (0.78, 0.72, 0.13), (0.42, 0.6, 0.1)]:
        d.polygon([(W*x, H*(y-r)), (W*(x+r), H*y), (W*x, H*(y+r*0.6)), (W*(x-r), H*y)], fill=(60, 58, 56, 255))
def grave(d, s):
    W, H = s; d.rectangle([W*0.36, H*0.20, W*0.64, H], fill=(96, 96, 102, 255)); d.ellipse([W*0.36, H*0.06, W*0.64, H*0.34], fill=(96, 96, 102, 255)); d.rectangle([W*0.46, H*0.18, W*0.54, H*0.40], fill=(60,60,64,255)); d.rectangle([W*0.40, H*0.26, W*0.60, H*0.33], fill=(60,60,64,255))
def bone(d, s):
    W, H = s; d.ellipse([W*0.30, H*0.20, W*0.70, H*0.62], fill=(206, 200, 186, 255)); d.ellipse([W*0.40, H*0.34, W*0.50, H*0.46], fill=(40,40,44,255)); d.ellipse([W*0.55, H*0.34, W*0.65, H*0.46], fill=(40,40,44,255)); d.line([(W*0.5,H*0.6),(W*0.5,H*0.95)], fill=(206,200,186,255), width=int(7*SS))
def cart(d, s):
    W, H = s; d.rectangle([W*0.20, H*0.30, W*0.80, H*0.62], fill=(70, 52, 34, 255)); d.ellipse([W*0.22, H*0.60, W*0.44, H*0.98], fill=DARK); d.ellipse([W*0.58, H*0.60, W*0.80, H*0.98], fill=DARK)
def log_(d, s):
    W, H = s; d.rectangle([W*0.08, H*0.45, W*0.92, H*0.80], fill=(74, 56, 38, 255)); d.ellipse([W*0.80, H*0.45, W*0.98, H*0.80], fill=(96, 74, 50, 255))

def crack():
    im, d = canvas(160, 200); W, H = im.size
    cx, cy = W*0.5, H*0.35
    seeds = [15, 55, 95, 140, 200, 250, 300, 335]
    for a in seeds:
        import math
        x, y = cx, cy; ang = math.radians(a)
        pts = [(x, y)]
        for step in range(6):
            ln = (H*0.10) * (0.7 + ((a*7 + step*31) % 10)/10.0)
            ang += math.radians(((a*13 + step*17) % 60) - 30)
            x += math.cos(ang)*ln; y += math.sin(ang)*ln + H*0.04
            pts.append((x, y))
        d.line(pts, fill=(8, 8, 10, 230), width=int(3*SS))
    save(im, "crack", 192)

barracks(); watchtower(); banner(); crack()
for n, f in [("rock", rock), ("spear", spear), ("debris", debris), ("grave", grave), ("bone", bone), ("cart", cart), ("log", log_)]:
    prop(n, f)
print("env props: barracks, watchtower, banner + rock/spear/debris/grave/bone/cart/log")
print("OK")
