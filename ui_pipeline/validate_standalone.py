#!/usr/bin/env python3
"""Run the Linux standalone on :1, screenshot, and drive it via XTEST clicks — Unity-runtime validation
when device install is blocked. Usage: validate_standalone.py <step>  where step picks a nav sequence.
"""
import os, sys, time, subprocess
os.environ['DISPLAY'] = ':1'
from Xlib import display, X
from Xlib.ext import xtest

EXE = "/tmp/p3build/linux/BULWARK.x86_64"
OUT = "/home/emre/Downloads/bulwark-clean/runtime/device_validation/rc_p4_linux"
os.makedirs(OUT, exist_ok=True)
W, H = 1280, 600
d = display.Display(); root = d.screen().root

def find_win(timeout=40):
    def walk(win):
        try: nm = win.get_wm_name()
        except Exception: nm = None
        if nm and ('BULWARK' in nm or 'bulwark' in nm or 'Bulwark' in nm): return win
        try:
            for c in win.query_tree().children:
                r = walk(c)
                if r: return r
        except Exception: pass
        return None
    for _ in range(timeout):
        w = walk(root)
        if w: return w
        time.sleep(1)
    return None

def win_origin(win):
    try:
        tc = win.translate_coords(root, 0, 0)
        return (-tc.x, -tc.y)
    except Exception:
        return (0, 0)

def shot(name):
    subprocess.run(['import', '-window', 'root', '-display', ':1', f'{OUT}/{name}.png'], check=False)
    sz = os.path.getsize(f'{OUT}/{name}.png') if os.path.exists(f'{OUT}/{name}.png') else 0
    print(f'  shot {name}: {sz}b')

def focus(win):
    try:
        win.configure(stack_mode=X.Above); win.set_input_focus(X.RevertToParent, X.CurrentTime); d.sync()
    except Exception as e: print('  focus err', e)

def click(win, ox, oy, gw, gh, fx, fy):
    x = int(ox + fx * gw); y = int(oy + fy * gh)
    root.warp_pointer(x, y); d.sync(); time.sleep(0.12)
    xtest.fake_input(d, X.MotionNotify, x=x, y=y); d.sync(); time.sleep(0.08)
    xtest.fake_input(d, X.ButtonPress, 1, root=root); d.sync(); time.sleep(0.08)
    xtest.fake_input(d, X.ButtonRelease, 1, root=root); d.sync(); time.sleep(0.18)
    print(f'  click ({fx:.2f},{fy:.2f}) -> {x},{y}', flush=True)

def main():
    # AUTO-MATCH capture: env-gated hook in the build auto-enters a Classic match (~6s after boot), so no
    # input simulation is needed (Unity's Linux input ignores synthetic X events). We just screenshot.
    env = dict(os.environ)
    env['BULWARK_AUTOMATCH'] = '1'
    env['BULWARK_BIOME'] = os.environ.get('BIOME', 'grass')
    env['BULWARK_SHOWSCREEN'] = os.environ.get('SHOW', '')  # if set, open that meta screen instead of a match
    proc = subprocess.Popen([EXE, '-screen-width', str(W), '-screen-height', str(H),
                             '-screen-fullscreen', '0', '-force-glcore', '-logFile', '/tmp/brun4.log'], env=env)
    print('launched pid', proc.pid, 'biome', env['BULWARK_BIOME'], flush=True)
    win = find_win()
    if not win: print('WINDOW NOT FOUND', flush=True)
    time.sleep(8);  shot('20_menu')            # menu before auto-match
    time.sleep(7);  shot('21_battlefield')     # auto-match fires ~11-12s -> battlefield
    time.sleep(3);  shot('22_battlefield')
    time.sleep(3);  shot('23_battlefield')
    proc.terminate(); time.sleep(1)
    try: proc.kill()
    except Exception: pass
    print('done', flush=True)

if __name__ == '__main__':
    main()
