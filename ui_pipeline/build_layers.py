#!/usr/bin/env python3
"""BULWARK / Stick Empire Rise — Phase-3 LAYER ASSET BUILDER.

Generates the CLEAN, UI-FREE, TEXT-FREE assets the 4-layer architecture (Report 05) needs and that did
not exist before:
  • L0 plate_<screen>.jpg  — atmospheric background plates (sky gradient + silhouette horizon + ground +
                             vignette), per-screen mood. NO text, NO UI, NO baked logos.
  • L1 char_<archetype>.png — stick-figure characters from the archetype sheet (Report 08): black stick
                             silhouettes + gold/faction accents + weapon + glowing eyes. Transparent.
  • L1 army_<faction>.png   — a row of small stick soldiers (battlefield/backdrop dressing).
All anti-aliased (drawn at SS× then LANCZOS-downscaled). Pure code art — no external/ripped assets, no IP.
Output → Assets/StreamingAssets/bulwark_ui/.  Re-runnable.
"""
import os, math
from PIL import Image, ImageDraw, ImageFilter

OUT = "/home/emre/Downloads/bulwark-clean/Assets/StreamingAssets/bulwark_ui"
os.makedirs(OUT, exist_ok=True)
SS = 3  # supersample factor for anti-aliasing

GOLD=(202,160,74); GOLDHI=(240,210,122); BLUE=(43,86,200); BLUEHI=(79,139,255)
RED=(122,31,26); EMBER=(216,69,43); PURPLE=(90,45,176); PURPLEHI=(158,107,240)
GREEN=(40,120,55); PARCH=(217,199,154); BLACK=(14,14,18); EYE=(255,244,200)

def lerp(a,b,t): return tuple(int(a[i]+(b[i]-a[i])*t) for i in range(3))

def vgrad(w,h,top,bot):
    im=Image.new("RGB",(w,h)); px=im.load()
    for y in range(h):
        c=lerp(top,bot,y/max(1,h-1))
        for x in range(w): px[x,y]=c
    return im

def vignette(im,strength=0.55):
    w,h=im.size; v=Image.new("L",(w,h),0); d=ImageDraw.Draw(v)
    d.ellipse([-w*0.25,-h*0.25,w*1.25,h*1.25],fill=255)
    v=v.filter(ImageFilter.GaussianBlur(w*0.12))
    dark=Image.new("RGB",(w,h),(0,0,0));
    return Image.composite(im,dark,v.point(lambda p:int(255-(255-p)*strength)))

def castle_silhouette(d,w,h,base_y,col,scale=1.0):
    # crenellated wall + a few towers as dark polygons
    wall_h=int(h*0.16*scale); top=base_y-wall_h
    d.rectangle([0,top,w,base_y],fill=col)
    bw=int(w*0.04)
    for x in range(0,w,bw*2):
        d.rectangle([x,top-int(wall_h*0.4),x+bw,top],fill=col)
    for tx in (int(w*0.18),int(w*0.5),int(w*0.82)):
        tw=int(w*0.08*scale); th=int(wall_h*1.8)
        d.rectangle([tx-tw//2,base_y-th,tx+tw//2,base_y],fill=col)
        d.polygon([(tx-tw//2-6,base_y-th),(tx+tw//2+6,base_y-th),(tx,base_y-th-int(th*0.4))],fill=col)

def mountains(d,w,h,base_y,col):
    pts=[(0,base_y)]; import_x=0
    peaks=[(0.0,0.0),(0.2,0.5),(0.4,0.2),(0.6,0.6),(0.8,0.3),(1.0,0.55)]
    for fx,fh in peaks: pts.append((int(fx*w),base_y-int(h*0.22*fh)))
    pts.append((w,base_y)); d.polygon(pts,fill=col)

def plate(name, sky_top, sky_bot, horizon="castle", horizon_col=None, ground_col=None, vig=0.5, glow=None):
    W,H=1280,592; w,h=W*SS,H*SS
    im=vgrad(w,h,sky_top,sky_bot).convert("RGB")
    d=ImageDraw.Draw(im,"RGBA")
    if glow:
        gx,gy,gr,gc=glow
        g=Image.new("RGBA",(w,h),(0,0,0,0)); gd=ImageDraw.Draw(g)
        gd.ellipse([gx-gr,gy-gr,gx+gr,gy+gr],fill=gc); g=g.filter(ImageFilter.GaussianBlur(gr*0.4))
        im=Image.alpha_composite(im.convert("RGBA"),g).convert("RGB"); d=ImageDraw.Draw(im,"RGBA")
    base_y=int(h*0.78)
    hc=horizon_col or lerp(sky_bot,BLACK,0.5)
    if horizon=="castle": castle_silhouette(d,w,h,base_y,hc)
    elif horizon=="mountains": mountains(d,w,h,base_y,hc)
    elif horizon=="both": mountains(d,w,h,base_y,lerp(hc,BLACK,0.3)); castle_silhouette(d,w,h,int(h*0.80),hc,0.7)
    gc=ground_col or lerp(sky_bot,BLACK,0.7)
    d.rectangle([0,base_y,w,h],fill=gc)
    im=im.resize((W,H),Image.LANCZOS)
    im=vignette(im,vig)
    im.save(os.path.join(OUT,"plate_"+name+".jpg"),quality=84)

# ---------- L0 plates (8 core screens) ----------
plate("splash",     (28,34,60),(96,52,40), "both",  glow=(int(1280*SS*0.5),int(592*SS*0.62),int(280*SS),(255,150,70,90)), vig=0.6)
plate("loading",    (24,30,54),(120,60,42), "castle",glow=(int(1280*SS*0.5),int(592*SS*0.5),int(240*SS),(255,140,60,110)), vig=0.55)
plate("mainmenu",   (70,120,190),(150,170,150), "castle", horizon_col=(60,70,80), ground_col=(70,90,55), vig=0.35,
      glow=(int(1280*SS*0.25),int(0),int(360*SS),(255,235,170,80)))   # bright day kingdom
plate("modeselect", (40,34,46),(70,46,40), "castle", vig=0.6, glow=(int(1280*SS*0.5),int(592*SS*0.7),int(300*SS),(216,69,43,70)))
plate("campaignmap",(36,52,40),(90,50,34), "mountains", horizon_col=(30,46,34), ground_col=(46,40,26), vig=0.45)  # green->amber
plate("battlehud",  (40,46,64),(60,52,44), "both", horizon_col=(34,40,52), ground_col=(54,52,40), vig=0.4)
plate("victory",    (60,70,120),(190,120,60), "both", horizon_col=(48,44,60), ground_col=(70,58,40), vig=0.4,
      glow=(int(1280*SS*0.5),int(592*SS*0.35),int(340*SS),(255,210,120,120)))   # dawn/triumph
plate("defeat",     (30,32,42),(54,40,40), "both", horizon_col=(28,28,34), ground_col=(34,30,30), vig=0.62)   # bleak storm
print("plates: 8")

# ---------- L1 stick-figure characters ----------
def stick(name, accent, weapon=None, crown=False, hat=False, hood=False, cape=None, shield=False,
          big=False, kneel=False, cheer=False, robe=False):
    W,H=420,720; w,h=W*SS,H*SS
    im=Image.new("RGBA",(w,h),(0,0,0,0)); d=ImageDraw.Draw(im)
    cx=w//2; lw=int(26*SS*(1.25 if big else 1.0))
    head_r=int(46*SS*(1.2 if big else 1.0)); head_y=int(h*0.16)
    hip=int(h*0.58); foot=int(h*0.93); shoulder=int(h*0.30)
    def L(p1,p2,width=lw,fill=BLACK): d.line([p1,p2],fill=fill,width=width)
    # cape (behind)
    if cape: d.polygon([(cx-head_r,shoulder),(cx+head_r,shoulder),(cx+int(head_r*1.6),hip+int(h*0.06)),(cx-int(head_r*1.6),hip+int(h*0.06))],fill=cape)
    # robe (mage) instead of legs
    if robe:
        d.polygon([(cx-int(head_r*0.7),shoulder+int(h*0.02)),(cx+int(head_r*0.7),shoulder+int(h*0.02)),(cx+int(head_r*1.7),foot),(cx-int(head_r*1.7),foot)],fill=BLACK)
    else:
        # legs
        if kneel:
            L((cx,hip),(cx-int(head_r*0.4),foot)); L((cx,hip),(cx+int(head_r*1.4),hip+int(h*0.04))); L((cx+int(head_r*1.4),hip+int(h*0.04)),(cx+int(head_r*1.4),foot))
        else:
            L((cx,hip),(cx-int(head_r*0.9),foot)); L((cx,hip),(cx+int(head_r*0.9),foot))
        # torso
        L((cx,shoulder),(cx,hip))
    if not robe: L((cx,shoulder),(cx,hip))
    # arms
    if cheer:
        L((cx,shoulder),(cx-int(head_r*1.5),shoulder-int(h*0.12))); L((cx,shoulder),(cx+int(head_r*1.5),shoulder-int(h*0.12)))
    elif weapon=="bow":
        L((cx,shoulder),(cx-int(head_r*1.3),shoulder+int(h*0.04))); L((cx,shoulder),(cx+int(head_r*1.1),shoulder+int(h*0.10)))
    else:
        L((cx,shoulder),(cx-int(head_r*1.2),shoulder+int(h*0.12))); L((cx,shoulder),(cx+int(head_r*1.2),shoulder+int(h*0.10)))
    # head
    d.ellipse([cx-head_r,head_y-head_r,cx+head_r,head_y+head_r],fill=BLACK)
    # glowing eyes
    er=int(7*SS); d.ellipse([cx-int(head_r*0.5)-er,head_y-er,cx-int(head_r*0.5)+er,head_y+er],fill=EYE)
    d.ellipse([cx+int(head_r*0.5)-er,head_y-er,cx+int(head_r*0.5)+er,head_y+er],fill=EYE)
    # hood / hat / crown
    if hood: d.arc([cx-head_r-8,head_y-head_r-8,cx+head_r+8,head_y+head_r+8],180,360,fill=accent,width=int(18*SS))
    if hat: d.polygon([(cx-head_r,head_y-int(head_r*0.4)),(cx+head_r,head_y-int(head_r*0.4)),(cx,head_y-int(head_r*2.6))],fill=accent)
    if crown:
        cy=head_y-head_r
        d.polygon([(cx-head_r,cy),(cx+head_r,cy),(cx+head_r,cy-int(head_r*0.3)),(cx+int(head_r*0.5),cy-int(head_r*0.9)),(cx,cy-int(head_r*0.4)),(cx-int(head_r*0.5),cy-int(head_r*0.9)),(cx-head_r,cy-int(head_r*0.3))],fill=GOLD)
    # weapons + shield (accent)
    ay=shoulder+int(h*0.10); ax=cx+int(head_r*1.2)
    if weapon=="sword":
        d.line([(ax,ay),(ax+int(head_r*0.4),ay-int(h*0.22))],fill=lerp(GOLDHI,(220,220,230),0.6),width=int(14*SS))
        d.line([(ax-int(head_r*0.4),ay-int(h*0.05)),(ax+int(head_r*0.8),ay-int(h*0.05))],fill=GOLD,width=int(10*SS))
    elif weapon=="staff":
        ex=cx-int(head_r*1.3); ey=shoulder-int(h*0.16)
        d.line([(cx-int(head_r*1.3),shoulder+int(h*0.04)),(ex,ey)],fill=lerp(GOLD,BLACK,0.3),width=int(13*SS))
        orb=int(26*SS); d.ellipse([ex-orb,ey-orb,ex+orb,ey+orb],fill=accent)
        g=Image.new("RGBA",(w,h),(0,0,0,0)); ImageDraw.Draw(g).ellipse([ex-orb*2,ey-orb*2,ex+orb*2,ey+orb*2],fill=accent+(120,));
        im=Image.alpha_composite(g.filter(ImageFilter.GaussianBlur(orb)),im)  # glow behind
        d=ImageDraw.Draw(im)
    elif weapon=="bow":
        bx=cx-int(head_r*1.5); d.arc([bx-int(head_r*0.9),shoulder-int(h*0.06),bx+int(head_r*0.9),shoulder+int(h*0.18)],300,60,fill=accent,width=int(12*SS))
        d.line([(bx+int(head_r*0.4),shoulder-int(h*0.03)),(bx+int(head_r*0.4),shoulder+int(h*0.15))],fill=lerp(PARCH,BLACK,0.2),width=int(5*SS))
    elif weapon=="spear":
        d.line([(ax,ay-int(h*0.2)),(ax,ay+int(h*0.22))],fill=lerp(GOLD,(200,200,210),0.5),width=int(11*SS))
        d.polygon([(ax,ay-int(h*0.27)),(ax-int(head_r*0.3),ay-int(h*0.19)),(ax+int(head_r*0.3),ay-int(h*0.19))],fill=(220,220,230))
    if shield:
        sx=cx-int(head_r*1.25); sy=shoulder+int(h*0.16)
        d.ellipse([sx-int(head_r*0.8),sy-int(head_r*1.0),sx+int(head_r*0.8),sy+int(head_r*1.0)],fill=accent,outline=GOLD,width=int(8*SS))
    # light rim so the black silhouette reads on dark backgrounds (Splash/Defeat/etc.)
    a=im.split()[3]; rim=a.filter(ImageFilter.MaxFilter(2*SS+1))
    rimimg=Image.new("RGBA",im.size,(0,0,0,0)); rimimg.paste((232,212,150,205),mask=rim)
    im=Image.alpha_composite(rimimg,im)
    im=im.resize((W,H),Image.LANCZOS)
    im.save(os.path.join(OUT,"char_"+name+".png"))

stick("king",      BLUE,   weapon="sword", crown=True, cape=RED,    shield=True)
stick("mage",      PURPLEHI,weapon="staff", hat=True,  robe=True)
stick("archer",    GREEN,  weapon="bow",   hood=True)
stick("sword_blue",BLUE,   weapon="sword", shield=True)
stick("sword_red", RED,    weapon="sword", shield=True)
stick("spear_blue",BLUE,   weapon="spear")
stick("spear_red", RED,    weapon="spear")
stick("brute_red", RED,    weapon="sword", big=True, cape=RED)
stick("kneel",     BLUE,   weapon=None,    cape=RED,  kneel=True)
stick("cheer",     BLUE,   weapon="sword", cape=RED,  cheer=True)
print("characters: 10")

# ---------- L1 army silhouette strips ----------
def army(name, accent, n=9):
    W,H=1100,260; w,h=W*SS,H*SS
    im=Image.new("RGBA",(w,h),(0,0,0,0)); d=ImageDraw.Draw(im)
    for i in range(n):
        cx=int((i+0.5)/n*w)+ (37*i%23)*SS; base=h-int(20*SS)
        head=int(h*0.22); r=int(28*SS); lwl=int(16*SS)
        d.line([(cx,head),(cx,int(h*0.62))],fill=BLACK,width=lwl)
        d.line([(cx,int(h*0.62)),(cx-int(r*0.7),base)],fill=BLACK,width=lwl); d.line([(cx,int(h*0.62)),(cx+int(r*0.7),base)],fill=BLACK,width=lwl)
        d.line([(cx,int(h*0.34)),(cx+int(r*1.0),int(h*0.30))],fill=BLACK,width=lwl)
        d.ellipse([cx-r,head-r,cx+r,head+r],fill=BLACK)
        # spear + accent banner tip
        d.line([(cx+int(r*1.0),int(h*0.30)),(cx+int(r*1.0),int(h*0.05))],fill=lerp(accent,BLACK,0.2),width=int(8*SS))
    im=im.resize((W,H),Image.LANCZOS)
    im.save(os.path.join(OUT,"army_"+name+".png"))
army("blue",BLUE); army("red",RED)
print("armies: 2")
print("OK")
