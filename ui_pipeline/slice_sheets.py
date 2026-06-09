#!/usr/bin/env python3
"""NATIVE ART-INJECTION: slice the injected AAA atlas sheets into the loaders' EXISTING asset names.
Boxes are from ImageMagick connected-component detection (exact, not eyeballed). Grey bg -> transparent."""
import os
from PIL import Image, ImageDraw

D = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"

def keyed(name):
    """Remove ONLY the connected background grey via border flood-fill — preserves interior art-grey
    (steel weapons, stone statues) that a global colour-key would wrongly eat."""
    im = Image.open(os.path.join(D, name + ".png")).convert("RGBA"); w, h = im.size
    seeds = [(x, 1) for x in range(0, w, 24)] + [(x, h - 2) for x in range(0, w, 24)] \
          + [(1, y) for y in range(0, h, 24)] + [(w - 2, y) for y in range(0, h, 24)]
    for s in seeds:
        r, g, b, a = im.getpixel(s)
        if a != 0 and 72 <= r <= 146 and abs(r - g) < 14 and abs(g - b) < 14:  # only seed on bg-grey
            ImageDraw.floodfill(im, s, (0, 0, 0, 0), thresh=30)
    return im

def cut(im, box, name, do_trim=True):
    x, y, w, h = box; c = im.crop((x, y, x + w, y + h))
    if do_trim:
        bb = c.getbbox()
        if bb: c = c.crop(bb)
    c.save(os.path.join(D, name + ".png"))

# ---- PANELS ----
P = keyed("ui_panels_sheet")
cut(P, (73, 21, 1389, 487), "kit_panel_ornate")
cut(P, (422, 514, 691, 279), "kit_panel_parchment")
cut(P, (71, 794, 1393, 96), "kit_divider")

# ---- BUTTONS ----
B = keyed("ui_buttons_sheet")
for name, box in {"kit_btn_red": (170, 25, 1196, 191), "kit_btn_blue": (170, 224, 1196, 190),
                  "kit_btn_green": (170, 423, 1196, 191), "kit_btn_purple": (170, 623, 1197, 190),
                  "kit_btn_gold": (169, 823, 1197, 189), "kit_btn_dark": (170, 224, 1196, 190)}.items():
    cut(B, box, name)

# ---- ICONS ----
I = keyed("ui_icons_sheet")
for name, box in {"ic_gem": (45, 50, 288, 470), "ic_coin": (359, 123, 346, 368), "ic_crown": (727, 101, 416, 383),
                  "ic_keep": (1168, 90, 302, 446), "ic_attack": (130, 555, 390, 290),
                  "ic_quest": (579, 561, 368, 382), "ic_settings": (1001, 565, 347, 363)}.items():
    cut(I, box, name)

# ---- CHARACTERS ----
C = keyed("ui_characters_sheet")
for name, box in {"cp_head": (69, 88, 150, 140), "cp_head_blue": (69, 88, 150, 140), "cp_head_red": (63, 261, 155, 146),
                  "ce_sword": (777, 55, 141, 576), "ce_pickaxe": (899, 88, 241, 531), "ce_bow": (1054, 46, 232, 572),
                  "ce_spear": (1370, 29, 53, 619), "ce_hat_wizard": (776, 657, 221, 312),
                  "ce_helm_crested": (1020, 642, 211, 323), "ce_satchel": (1252, 679, 237, 290),
                  "cp_limb": (75, 451, 66, 227)}.items():
    cut(C, box, name)
# alias unused launch equipment to sensible AAA stand-ins
import shutil
shutil.copy(os.path.join(D, "ce_helm_crested.png"), os.path.join(D, "ce_helm_iron.png"))  # reuse helm for iron
shutil.copy(os.path.join(D, "ce_hat_wizard.png"), os.path.join(D, "ce_hood.png"))          # nearest head-gear
# ce_shield / ce_cape / ce_banner: keep prior PIL (no AAA equivalent on the sheet)

# ---- STATUES ----
S = keyed("env_statues_sheet")
cut(S, (5, 16, 737, 968), "statue_blue")
cut(S, (770, 19, 724, 965), "statue_red")

# ---- PARALLAX (siege biome): painted bands; sky/ground opaque, others keyed ----
def band(y0, y1, name, ext, key=True):
    im = keyed("env_parallax_siege") if key else Image.open(os.path.join(D, "env_parallax_siege.png")).convert("RGBA")
    w, h = im.size; c = im.crop((int(0.02 * w), y0, int(0.98 * w), y1))
    if ext == ".jpg": c.convert("RGB").save(os.path.join(D, name + ".jpg"), quality=86)
    else: c.save(os.path.join(D, name + ".png"))
band(20, 210, "bf_siege_sky", ".jpg", key=False)
band(215, 470, "bf_siege_horizon", ".png")
band(480, 675, "bf_siege_mid", ".png")
band(675, 1010, "bf_siege_fg", ".png")
band(880, 1010, "bf_siege_ground", ".png", key=False)

print("sliced all 6 sheets into loader asset names.")
print("OK")
