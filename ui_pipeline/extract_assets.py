#!/usr/bin/env python3
"""BULWARK UI asset extraction — slices the /design mockups into real, reusable game sprites.

Source of truth = the /design PNGs. Produces, into Assets/StreamingAssets/bulwark_ui/:
  • bd_<screen>.jpg  — per-screen atmospheric BACKDROP (downscaled mockup; live UI draws on top)
  • kit_*.png        — reusable 9-slice chrome: ornate gold panel + 6 colour gem buttons + gem finial
  • ic_*.png         — icon set (coin / gem / plus / star)
Clean buttons come from the Pause mockup (no leading icon, flourishes only; live labels cover baked text).
Gold/green/purple buttons are luminance-recolours of the blue one (gold rim + flourishes preserved).
9-slice borders are assigned at load time in UiAssets.cs. Re-runnable.
"""
import os, glob
from PIL import Image

SRC = "/home/emre/Downloads/bulwark-clean/design"
OUT = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"
os.makedirs(OUT, exist_ok=True)
for f in glob.glob(os.path.join(OUT, "*.png")) + glob.glob(os.path.join(OUT, "*.jpg")):
    os.remove(f)

SCREENS = {
    "splash":"SplashScreenDesign","loading":"LoadingScreenDesign","login":"LoginAuthDesign",
    "mainmenu":"MainMenuDesign","modeselect":"ModScreenDesign","matchintro":"MatchIntroDesign",
    "battlehud":"BattleHudDesign","spellhud":"InMatchSpellHudDesign","banner":"InMatchBannerDesign",
    "pause":"PauseModalDesign","victory":"VictoryScreenDesign","defeat":"DefeatScreenDesign",
    "campaignresult":"CampaignResultDesign","endlessresult":"EndlessResultDesign","ladderresult":"LadderResultDesign",
    "store":"StoreScreenDesign","spells":"SpellsScreenDesign","skins":"SkinsScreenDesign",
    "chests":"ChestsScreenDesign","chestopen":"ChestOpenResultDesign","units":"UnitsArmyDesign",
    "commander":"CommanderSelectDesign","profile":"ProfileScreenDesign","battlepass":"BattlePassDesign",
    "quests":"QuestsScreenDesign","campaignmap":"CampaignMapDesign","daily":"DailyRewardDesign",
    "luckyspin":"LuckySpinDesign","freerewards":"FreeRewardsDesign","events":"EventsHubDesign",
    "onlinebattle":"OnlineBattleDesign","tournament":"TournamentLadderDesign","leaderboard":"LeaderboardScreenDesign",
    "clan":"ClanScreenDesign","settings":"SettingsScreenDesign","confirm":"ConfirmModalDesign",
    "rewardgrant":"RewardGrantDesign","networkerror":"NetworkErrorDesign",
}

_cache = {}
def src(name):
    if name not in _cache: _cache[name] = Image.open(os.path.join(SRC, name+".png")).convert("RGBA")
    return _cache[name]

def crop_frac(name, box):
    im = src(name); w,h = im.size; x0,y0,x1,y1 = box
    return im.crop((int(x0*w),int(y0*h),int(x1*w),int(y1*h)))

def recolor(im, target, boost=1.45):
    """Recolour the blue body to `target` (luminance-preserving); leave gold rim/flourishes intact.
    Pure-PIL pixel loop (button images are small; no numpy dependency)."""
    im = im.convert("RGBA"); px = im.load(); w,h = im.size; tr,tg,tb = target
    for y in range(h):
        for x in range(w):
            r,g,b,a = px[x,y]
            if b > r+12 and b > g+8:                   # blue-dominant body pixel
                lum = (0.30*r+0.59*g+0.11*b)/255.0
                px[x,y] = (min(255,int(tr*lum*boost)), min(255,int(tg*lum*boost)), min(255,int(tb*lum*boost)), a)
    return im

def clean_button(im):
    """Erase the baked centre label (RESUME/etc.) by stamping a clean body column across the middle;
    keeps the gold rim (top/bottom) + the end flourishes (~18%/82%) so only the text region is replaced."""
    im = im.convert("RGBA"); px = im.load(); w, h = im.size; sx = int(w*0.22)
    for x in range(int(w*0.26), int(w*0.74)):
        for y in range(int(h*0.12), int(h*0.88)):
            px[x, y] = px[sx, y]
    return im

def clean_panel(im):
    """Erase the baked interior (PAUSED title + 3 buttons) → subtle dark obsidian gradient; keep the gold frame."""
    im = im.convert("RGBA"); px = im.load(); w, h = im.size
    x0,x1,y0,y1 = int(w*0.13),int(w*0.87),int(h*0.12),int(h*0.88)
    for y in range(y0, y1):
        t = (y-y0)/max(1,(y1-y0))
        col = (int(20*(1-t)+10*t), int(22*(1-t)+11*t), int(30*(1-t)+15*t), 255)
        for x in range(x0, x1): px[x, y] = col
    return im

# ---- 1) backdrops (JPG q82, 1000w) ----
for key,name in SCREENS.items():
    im = src(name); w,h = im.size
    im.convert("RGB").resize((1000,int(h*1000/w)), Image.LANCZOS).save(os.path.join(OUT,"bd_"+key+".jpg"),quality=82)
print(f"backdrops: {len(SCREENS)}")

# ---- 2) reusable kit (downscaled to ~512w; 9-slice stretches anyway → keeps repo light) ----
def dn(im, w=512):
    return im.resize((w, int(im.height*w/im.width)), Image.LANCZOS) if im.width > w else im
def savek(im, out, w=512): dn(im,w).save(os.path.join(OUT,out+".png"))

panel = clean_panel(crop_frac("PauseModalDesign", (0.28,0.105,0.72,0.95)))   # gold ornate frame, interior cleaned
savek(panel,"kit_panel_ornate")
savek(crop_frac("PauseModalDesign", (0.452,0.0,0.548,0.088)),"kit_finial",128)  # blue gem finial

blue = clean_button(crop_frac("PauseModalDesign", (0.345,0.315,0.655,0.452)))   # blue gem button (text erased)
dark = clean_button(crop_frac("PauseModalDesign", (0.345,0.475,0.655,0.60)))    # dark steel
red  = clean_button(crop_frac("PauseModalDesign", (0.345,0.625,0.655,0.762)))   # oxblood
savek(blue,"kit_btn_blue"); savek(dark,"kit_btn_dark"); savek(red,"kit_btn_red")
savek(recolor(blue,(225,170,55)),"kit_btn_gold")    # gold/orange
savek(recolor(blue,(70,165,60)),"kit_btn_green")    # emerald
savek(recolor(blue,(150,80,200)),"kit_btn_purple")  # amethyst
print("buttons + panel + finial done")

# ---- 3) icons (from MainMenu currency pills + a result star) ----
crop_frac("MainMenuDesign", (0.555,0.025,0.60,0.085)).save(os.path.join(OUT,"ic_coin.png"))   # gold coin
crop_frac("MainMenuDesign", (0.74,0.025,0.785,0.085)).save(os.path.join(OUT,"ic_gem.png"))    # gem
print("icons done")
print("OK")
