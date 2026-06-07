#!/usr/bin/env python3
"""BULWARK UI compile-check harness.

Compiles the Bulwark.Bootstrap assembly with Unity's bundled Roslyn (csc.dll),
using the reference set + defines from Bulwark.Bootstrap.csproj but GLOBBING the
live Bootstrap source dir (so newly-added screen .cs files are included even
before Unity regenerates the csproj). Output DLL is discarded; we only want
diagnostics. Exit code 0 = clean compile.
"""
import os, re, subprocess, sys, glob, tempfile

ROOT = "/home/emre/Downloads/bulwark-clean"
CSPROJ = os.path.join(ROOT, "Bulwark.Bootstrap.csproj")
UNITY = "/home/emre/Unity/Hub/Editor/6000.0.75f1/Editor/Data"
DOTNET = os.path.join(UNITY, "NetCoreRuntime", "dotnet")
CSC = os.path.join(UNITY, "DotNetSdkRoslyn", "csc.dll")
SRC_DIR = os.path.join(ROOT, "Assets/_Game/Bootstrap")

def main():
    txt = open(CSPROJ, encoding="utf-8").read()
    refs = re.findall(r"<HintPath>(.*?)</HintPath>", txt)
    m = re.search(r"<DefineConstants>(.*?)</DefineConstants>", txt, re.S)
    defines = m.group(1).strip() if m else ""

    # Sibling Bulwark assemblies are ProjectReferences (not HintPaths). Use the
    # precompiled DLLs Unity already produced. Exclude the Bootstrap assembly
    # itself (we are recompiling its sources) to avoid duplicate definitions.
    for dll in sorted(glob.glob(os.path.join(ROOT, "Library/ScriptAssemblies", "*.dll"))):
        base = os.path.basename(dll)
        if base == "Bulwark.Bootstrap.dll":
            continue
        refs.append(dll)

    sources = sorted(glob.glob(os.path.join(SRC_DIR, "**", "*.cs"), recursive=True))
    if not sources:
        print("ERROR: no sources found", file=sys.stderr); return 2

    out_dll = os.path.join(tempfile.gettempdir(), "Bulwark.Bootstrap.check.dll")
    rsp_lines = [
        "-target:library", "-nostdlib+", "-noconfig", "-unsafe-",
        "-langversion:9.0", "-nowarn:0169,0414,0649,0067,0162,0168,1701,1702",
        "-out:\"%s\"" % out_dll,
        "-define:" + defines,
    ]
    for r in refs:
        rsp_lines.append("-r:\"%s\"" % r)
    for s in sources:
        rsp_lines.append("\"%s\"" % s)

    rsp = os.path.join(tempfile.gettempdir(), "bulwark_bootstrap.rsp")
    open(rsp, "w", encoding="utf-8").write("\n".join(rsp_lines))

    proc = subprocess.run([DOTNET, CSC, "@" + rsp], capture_output=True, text=True)
    out = proc.stdout + proc.stderr
    # Only show error/warning lines (Roslyn prints CS#### codes).
    diags = [ln for ln in out.splitlines() if re.search(r": (error|warning) CS\d+", ln)]
    errors = [ln for ln in diags if ": error CS" in ln]
    for ln in diags:
        print(ln)
    print("---")
    print("SOURCES: %d   ERRORS: %d   WARNINGS: %d" % (len(sources), len(errors), len(diags) - len(errors)))
    return 1 if errors else 0

if __name__ == "__main__":
    sys.exit(main())
