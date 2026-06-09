#!/usr/bin/env python3
"""STICK EMPIRE RISE — PHASE 6 CHARACTER PARTS + EQUIPMENT (shared-rig overlays).

The master stick rig is built in code (bone Transforms); these sprites are the parts it parents:
  cp_limb   — one reusable limb segment (pivot TOP, set in code) — used for all arm/leg segments
  cp_head   — head disc with glowing eyes
  cp_torso  — torso segment
Equipment overlays (silhouette = the ≤0.3s recognition cue; pivots set per-slot in code):
  ce_sword ce_bow ce_spear ce_pickaxe ce_staff   (WeaponSlot)
  ce_shield                                        (OffhandSlot)
  ce_helm_iron ce_helm_crested ce_hood ce_hat_wizard  (head accessory)
  ce_satchel ce_cape                               (AccessorySlot / CapeAnchor)
Faction tint is applied in code (accents only), so silhouette — not colour — carries identity (grayscale-safe).
Pure code art. Output → StreamingAssets/bulwark_ui/ (cp_*, ce_*).
"""
import os, math
from PIL import Image, ImageDraw

OUT = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"
os.makedirs(OUT, exist_ok=True)
SS = 4
BLACK = (16, 16, 20, 255)
STEEL = (200, 202, 210, 255)
WOOD = (120, 86, 52, 255)
GOLD = (210, 175, 90, 255)
EYE = (255, 244, 200, 255)

def cv(w, h):
    im = Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0)); return im, ImageDraw.Draw(im)
def save(im, name, w):
    im.resize((w, int(im.height * w / im.width)), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

# ---- body parts ----
def limb():
    im, d = cv(28, 120); W, H = im.size
    d.rounded_rectangle([W*0.30, 0, W*0.70, H], radius=W*0.3, fill=BLACK)  # pivot TOP (set in code)
    save(im, "cp_limb", 28)
def head():
    im, d = cv(90, 90); W, H = im.size
    d.ellipse([W*0.06, H*0.06, W*0.94, H*0.94], fill=BLACK)
    er = W*0.07; d.ellipse([W*0.34-er, H*0.46-er, W*0.34+er, H*0.46+er], fill=EYE); d.ellipse([W*0.62-er, H*0.46-er, W*0.62+er, H*0.46+er], fill=EYE)
    save(im, "cp_head", 64)
def torso():
    im, d = cv(40, 110); W, H = im.size
    d.rounded_rectangle([W*0.28, 0, W*0.72, H], radius=W*0.25, fill=BLACK)
    save(im, "cp_torso", 32)

# ---- equipment (silhouettes) ----
def sword():
    im, d = cv(40, 150); W, H = im.size
    d.polygon([(W*0.5, 0), (W*0.62, H*0.10), (W*0.58, H*0.68), (W*0.42, H*0.68), (W*0.38, H*0.10)], fill=STEEL)  # blade
    d.rectangle([W*0.20, H*0.66, W*0.80, H*0.74], fill=GOLD)  # guard
    d.rectangle([W*0.44, H*0.74, W*0.56, H*0.95], fill=WOOD)  # grip (pivot near here)
    save(im, "ce_sword", 40)
def bow():
    im, d = cv(70, 150); W, H = im.size
    d.arc([W*0.10, 0, W*1.4, H], 110, 250, fill=WOOD, width=int(7*SS))
    d.line([(W*0.46, H*0.06), (W*0.46, H*0.94)], fill=(230, 225, 210, 220), width=int(2*SS))  # string
    save(im, "ce_bow", 56)
def spear():
    im, d = cv(28, 230); W, H = im.size
    d.rectangle([W*0.42, H*0.10, W*0.58, H], fill=WOOD)  # shaft
    d.polygon([(W*0.5, 0), (W*0.30, H*0.12), (W*0.70, H*0.12)], fill=STEEL)  # tip
    save(im, "ce_spear", 24)
def pickaxe():
    im, d = cv(90, 150); W, H = im.size
    d.rectangle([W*0.46, H*0.10, W*0.56, H], fill=WOOD)  # handle
    d.arc([W*0.05, H*0.02, W*0.95, H*0.40], 200, 340, fill=(90, 92, 98, 255), width=int(9*SS))  # pick head
    save(im, "ce_pickaxe", 64)
def staff():
    im, d = cv(46, 200); W, H = im.size
    d.rectangle([W*0.44, H*0.18, W*0.56, H], fill=(70, 50, 90, 255))  # shaft
    orb = W*0.34; d.ellipse([W*0.5-orb, H*0.16-orb, W*0.5+orb, H*0.16+orb], fill=(158, 107, 240, 255))  # crystal
    save(im, "ce_staff", 40)
def shield():
    im, d = cv(90, 110); W, H = im.size
    d.polygon([(W*0.5, 0), (W*0.95, H*0.18), (W*0.85, H*0.78), (W*0.5, H), (W*0.15, H*0.78), (W*0.05, H*0.18)], fill=(110, 112, 120, 255))
    d.ellipse([W*0.40, H*0.42, W*0.60, H*0.62], fill=GOLD)  # boss
    save(im, "ce_shield", 64)
def helm_iron():
    im, d = cv(96, 70); W, H = im.size
    d.pieslice([0, 0, W, H*1.6], 180, 360, fill=(120, 122, 130, 255))
    d.rectangle([W*0.44, H*0.45, W*0.56, H], fill=(120, 122, 130, 255))  # nasal
    save(im, "ce_helm_iron", 64)
def helm_crested():
    im, d = cv(96, 110); W, H = im.size
    d.pieslice([0, H*0.36, W, H*1.4], 180, 360, fill=(120, 122, 130, 255))
    d.polygon([(W*0.5, 0), (W*0.42, H*0.42), (W*0.58, H*0.42)], fill=(190, 60, 50, 255))  # red crest
    d.rectangle([W*0.46, H*0.40, W*0.54, H*0.56], fill=(190, 60, 50, 255))
    save(im, "ce_helm_crested", 64)
def hood():
    im, d = cv(100, 90); W, H = im.size
    d.pieslice([0, 0, W, H*1.7], 180, 360, fill=(40, 70, 40, 255))
    d.polygon([(W*0.06, H*0.5), (W*0.5, H*0.5), (W*0.3, H)], fill=(40, 70, 40, 255))  # drape
    save(im, "ce_hood", 72)
def hat_wizard():
    im, d = cv(90, 130); W, H = im.size
    d.polygon([(W*0.5, 0), (W*0.84, H*0.78), (W*0.16, H*0.78)], fill=(90, 60, 150, 255))  # cone
    d.ellipse([W*0.06, H*0.74, W*0.94, H*0.98], fill=(70, 46, 120, 255))  # brim
    save(im, "ce_hat_wizard", 64)
def satchel():
    im, d = cv(70, 70); W, H = im.size
    d.rounded_rectangle([W*0.18, H*0.30, W*0.82, H*0.92], radius=W*0.12, fill=(96, 66, 38, 255))
    d.arc([W*0.18, H*0.04, W*0.82, H*0.7], 180, 360, fill=(70, 48, 28, 255), width=int(4*SS))  # strap
    save(im, "ce_satchel", 48)
def cape():
    im, d = cv(80, 130); W, H = im.size
    d.polygon([(W*0.30, 0), (W*0.70, 0), (W*0.92, H), (W*0.08, H)], fill=(150, 40, 36, 255))  # cloth (tinted in code)
    save(im, "ce_cape", 56)

for f in [limb, head, torso, sword, bow, spear, pickaxe, staff, shield, helm_iron, helm_crested, hood, hat_wizard, satchel, cape]:
    f()
print("character parts + equipment: cp_limb/head/torso + 12 equipment")
print("OK")
