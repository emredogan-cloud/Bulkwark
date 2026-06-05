// BULWARK — SIM PROXY RENDERER (Pre-Phase-5 GATE-1 viz track, PHASE V0). TEMPORARY, REMOVABLE, DEBUG-ONLY.
//
// PURPOSE: make the ECS simulation VISIBLE with primitive proxies, so a real GATE-1 fun verdict becomes
// possible. This is the §12 PRESENTATION layer (a MonoBehaviour) — it READS the ECS world read-only and
// mirrors entities as GameObject primitives. It changes NO gameplay logic or balance and never writes ECS
// state. Deleting this one file removes it 100%.
//
// MAPPING (V0 spec): Player units (Team 0) = BLUE capsules · AI units (Team 1) = RED capsules ·
//                    Mines = YELLOW cubes · Statues = GRAY cylinders.
// Health viz: proxy scales + tints by HP fraction. Spawn viz: proxy appears. Combat viz: white flash on
// damage; proxy destroyed on death. Statue cylinder shrinks/tints as it loses health.
//
// RENDERING CHOICE (entities.graphics evaluated, deferred — see PREPHASE5_VISUALIZATION_ROADMAP.md): uses
// GameObject.CreatePrimitive with the DEFAULT URP material tinted per-proxy via MaterialPropertyBlock
// (_BaseColor) — no runtime Shader.Find (cannot fail to a missing shader) and no per-entity ECS render
// components. Camera/art-light; robust on device.
// SCAFFOLD STATUS: authored here; CI compiles; device run produces the visual evidence.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Sim;
using Bulwark.Data;

namespace Bulwark.Bootstrap
{
    /// <summary>Temporary debug primitive renderer for the ECS battle. Auto-spawned; read-only of the sim.</summary>
    public sealed class SimProxyRenderer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("SimProxyRenderer");
            go.AddComponent<SimProxyRenderer>();
            DontDestroyOnLoad(go);
            Debug.Log("[PROXY] SimProxyRenderer booted (AfterSceneLoad).");
        }

        private enum Kind : byte { UnitP = 0, UnitAI = 1, Mine = 2, Statue = 3 }

        // Visual archetype for unit differentiation — derived READ-ONLY from the unit's CombatProfile
        // (DamageType/ArmorClass) + MinerTag. No sim/spawn/balance change; purely picks a placeholder sprite.
        private enum Arch : byte { Skirmisher = 0, Shield = 1, Heavy = 2, Ranged = 3, Caster = 4, Miner = 5 }

        private static Arch ClassifyArch(bool isMiner, DamageType dt, ArmorClass ac)
        {
            if (isMiner) return Arch.Miner;
            if (dt == DamageType.Fire || dt == DamageType.Poison) return Arch.Caster;   // Battlemage / Hexcaster
            if (dt == DamageType.Pierce || dt == DamageType.Blunt) return Arch.Ranged;   // Crossbow / Slinger
            if (ac == ArmorClass.Shielded) return Arch.Shield;                           // Shieldman (frontline)
            if (ac == ArmorClass.Heavy) return Arch.Heavy;                               // Ironclad / Razorbeast / Legionary
            return Arch.Skirmisher;                                                      // Raider / Houndmaster
        }

        private sealed class Proxy
        {
            public GameObject Go;
            public SpriteRenderer Sr;
            public Kind Kind;
            public float LastHp;
            public float Flash;   // seconds of damage-flash remaining
        }

        private readonly Dictionary<Entity, Proxy> _proxies = new Dictionary<Entity, Proxy>();
        private readonly HashSet<Entity> _seen = new HashSet<Entity>();
        private MaterialPropertyBlock _mpb;

        private World _w;
        private EntityManager _em;
        private EntityQuery _qUnit, _qMine, _qStatue;
        private bool _ready;

        private Camera _cam;
        private bool _camConfigured;
        private float _logTimer;
        private SpriteRenderer _bg; // battlefield background (covers the ortho view, behind all proxies)

        private static readonly Color ColP = new Color(0.25f, 0.5f, 1f);
        private static readonly Color ColAI = new Color(1f, 0.3f, 0.25f);
        private static readonly Color ColMine = new Color(1f, 0.85f, 0.1f);
        private static readonly Color ColStatue = new Color(0.72f, 0.72f, 0.74f);

        private const int PlayerTeam = 0, AiTeam = 1, MaxProxies = 1000;

        private bool EnsureQueries()
        {
            World w = World.DefaultGameObjectInjectionWorld;
            if (w == null || !w.IsCreated) { _ready = false; return false; }
            if (!_ready || w != _w)
            {
                _w = w; _em = w.EntityManager;
                _qUnit = _em.CreateEntityQuery(ComponentType.ReadOnly<UnitTag>(), ComponentType.ReadOnly<Position>(), ComponentType.ReadOnly<Team>(), ComponentType.ReadOnly<Health>());
                _qMine = _em.CreateEntityQuery(ComponentType.ReadOnly<MineNode>(), ComponentType.ReadOnly<Position>());
                _qStatue = _em.CreateEntityQuery(ComponentType.ReadOnly<StatueTag>(), ComponentType.ReadOnly<StatueState>(), ComponentType.ReadOnly<Position>());
                _ready = true;
            }
            return true;
        }

        private void Update()
        {
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            float dt = Time.unscaledDeltaTime;
            if (!EnsureQueries()) return;

            _seen.Clear();
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            int total = 0;

            try
            {
                // ---- Units ----
                using (var ents = _qUnit.ToEntityArray(Allocator.Temp))
                using (var pos = _qUnit.ToComponentDataArray<Position>(Allocator.Temp))
                using (var team = _qUnit.ToComponentDataArray<Team>(Allocator.Temp))
                using (var hp = _qUnit.ToComponentDataArray<Health>(Allocator.Temp))
                {
                    for (int i = 0; i < ents.Length && total < MaxProxies; i++)
                    {
                        if (hp[i].Current <= 0f) continue; // dead -> let it be culled (proxy destroyed below)
                        float frac = hp[i].Max > 0f ? Mathf.Clamp01(hp[i].Current / hp[i].Max) : 1f;
                        Kind k = team[i].Id == PlayerTeam ? Kind.UnitP : Kind.UnitAI;
                        // Archetype (read-only): MinerTag + CombatProfile → distinct placeholder sprite per role.
                        bool isMiner = _em.HasComponent<MinerTag>(ents[i]);
                        Arch arch;
                        if (!isMiner && _em.HasComponent<CombatProfile>(ents[i]))
                        {
                            var cp = _em.GetComponentData<CombatProfile>(ents[i]);
                            arch = ClassifyArch(false, cp.DamageType, cp.ArmorClass);
                        }
                        else arch = isMiner ? Arch.Miner : Arch.Skirmisher;
                        UpdateProxy(ents[i], k, arch, pos[i].Value, frac, hp[i].Current);
                        Bounds2D(pos[i].Value, ref minX, ref maxX, ref minY, ref maxY); total++;
                    }
                }
                // ---- Mines ----
                using (var ents = _qMine.ToEntityArray(Allocator.Temp))
                using (var pos = _qMine.ToComponentDataArray<Position>(Allocator.Temp))
                    for (int i = 0; i < ents.Length && total < MaxProxies; i++)
                    {
                        UpdateProxy(ents[i], Kind.Mine, Arch.Skirmisher, pos[i].Value, 1f, -1f);
                        Bounds2D(pos[i].Value, ref minX, ref maxX, ref minY, ref maxY); total++;
                    }
                // ---- Statues ----
                using (var ents = _qStatue.ToEntityArray(Allocator.Temp))
                using (var st = _qStatue.ToComponentDataArray<StatueState>(Allocator.Temp))
                using (var pos = _qStatue.ToComponentDataArray<Position>(Allocator.Temp))
                    for (int i = 0; i < ents.Length && total < MaxProxies; i++)
                    {
                        float frac = st[i].MaxHealth > 0f ? Mathf.Clamp01(st[i].Health / st[i].MaxHealth) : 1f;
                        UpdateProxy(ents[i], Kind.Statue, Arch.Skirmisher, pos[i].Value, frac, st[i].Health);
                        Bounds2D(pos[i].Value, ref minX, ref maxX, ref minY, ref maxY); total++;
                    }
            }
            catch (System.Exception e) { Debug.LogError("[PROXY] read error: " + e.Message); }

            CullStale();
            DecayFlashes(dt);
            if (total > 0) ConfigureCamera(minX, maxX, minY, maxY);

            _logTimer += dt;
            if (_logTimer >= 2f) { _logTimer = 0f; Debug.Log($"[PROXY] proxies={_proxies.Count} (rendering {total} entities as sprites)."); }
        }

        private void UpdateProxy(Entity e, Kind kind, Arch arch, float2 p, float hpFrac, float curHp)
        {
            _seen.Add(e);
            if (!_proxies.TryGetValue(e, out Proxy px))
            {
                px = CreateProxy(kind);
                px.LastHp = curHp;
                _proxies[e] = px;
                Debug.Log($"[PROXY] SPAWN {kind} proxy (entity index {e.Index}).");
            }

            // Combat viz: a drop in HP since last frame -> flash white briefly (+ throttled hit SFX + impact VFX).
            if (curHp >= 0f && curHp < px.LastHp - 0.01f)
            {
                px.Flash = 0.25f;
                if (PresentationState.InMatch)
                {
                    AudioManager.Instance?.Hit();
                    var fxPos = new Vector3(p.x, p.y, 0f);
                    if (kind == Kind.Statue) VfxManager.Instance?.StatueDamage(fxPos);
                    else VfxManager.Instance?.Impact(fxPos);
                }
            }
            px.LastHp = curHp;

            // Position on the z=0 battlefield plane.
            px.Go.transform.position = new Vector3(p.x, p.y, 0f);

            // Sprite: real placeholder art when loaded (StreamingAssets), else a white fallback (never invisible).
            Sprite spr = PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(SpriteKey(kind, arch)) : null;
            if (spr == null) spr = Fallback();
            px.Sr.sprite = spr;

            // Size: normalize sprite to a per-kind target world height; units/miners squash a little with HP.
            float spriteH = spr.bounds.size.y; if (spriteH < 0.01f) spriteH = 1f;
            float targetH = kind == Kind.Statue ? 3.0f : kind == Kind.Mine ? 1.3f : 1.7f; // bigger = more readable
            float baseScale = targetH / spriteH;
            float vSquash = (kind == Kind.Statue || kind == Kind.Mine) ? 1f : (0.7f + 0.3f * hpFrac);
            px.Go.transform.localScale = new Vector3(baseScale, baseScale * vSquash, baseScale);

            // Tint: light team/kind color (faction read), white flash on hit, grey-dim as HP drops.
            Color baseCol = kind == Kind.UnitP ? ColP : kind == Kind.UnitAI ? ColAI : kind == Kind.Mine ? ColMine : ColStatue;
            float tintAmt = kind == Kind.Mine ? 0.15f : kind == Kind.Statue ? 0.25f : 0.4f;
            Color c = px.Flash > 0f ? Color.white
                    : Color.Lerp(Color.Lerp(Color.white, baseCol, tintAmt), Color.gray, (1f - hpFrac) * 0.5f);
            px.Sr.color = c;
            px.Sr.sortingOrder = kind == Kind.Statue ? 0 : kind == Kind.Mine ? 1 : 2; // units in front
        }

        private Sprite _fallback;
        /// <summary>An 8×8 white fallback sprite (created in code) so a proxy renders a colored square before the
        /// StreamingAssets placeholder PNGs finish loading / if a load fails — never invisible.</summary>
        private Sprite Fallback()
        {
            if (_fallback != null) return _fallback;
            var t = new Texture2D(8, 8);
            var cols = new Color[64];
            for (int i = 0; i < 64; i++) cols[i] = Color.white;
            t.SetPixels(cols); t.Apply();
            _fallback = Sprite.Create(t, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 100f);
            return _fallback;
        }

        private static string SpriteKey(Kind k, Arch a)
        {
            if (k == Kind.Mine) return "mine";
            if (k == Kind.Statue) return "statue";
            // Units (either team): pick the placeholder sprite by archetype; team is conveyed by the tint below.
            switch (a)
            {
                case Arch.Miner:  return "miner";
                case Arch.Shield: return "unit_ai";     // Spearton (shield silhouette)
                case Arch.Heavy:  return "u_heavy";     // Giant
                case Arch.Ranged: return "u_ranged";    // Archidon
                case Arch.Caster: return "u_caster";    // Magikill
                default:          return "unit_player"; // Swordwrath (skirmisher/flanker)
            }
        }

        private Proxy CreateProxy(Kind kind)
        {
            var go = new GameObject("proxy_" + kind);
            var sr = go.AddComponent<SpriteRenderer>(); // default Sprites material is URP-compatible
            sr.sprite = Fallback();
            return new Proxy { Go = go, Sr = sr, Kind = kind };
        }

        private void CullStale()
        {
            // Destroy proxies whose entity is gone this frame (death viz). Collect-then-remove (no mutate-while-iterate).
            List<Entity> dead = null;
            foreach (var kv in _proxies)
                if (!_seen.Contains(kv.Key)) { (dead ??= new List<Entity>()).Add(kv.Key); }
            if (dead == null) return;
            for (int i = 0; i < dead.Count; i++)
            {
                if (_proxies.TryGetValue(dead[i], out var px))
                {
                    Vector3 dpos = px.Go != null ? px.Go.transform.position : Vector3.zero;
                    if (px.Go != null) Destroy(px.Go);
                    // Unit death feedback (Match-only) — presentation reaction to a culled unit proxy (SFX + puff VFX).
                    if (PresentationState.InMatch && (px.Kind == Kind.UnitP || px.Kind == Kind.UnitAI))
                    {
                        AudioManager.Instance?.Death();
                        VfxManager.Instance?.DeathPuff(dpos);
                    }
                }
                _proxies.Remove(dead[i]);
                Debug.Log($"[PROXY] DEATH/cull proxy (entity index {dead[i].Index}).");
            }
        }

        private void DecayFlashes(float dt)
        {
            foreach (var kv in _proxies) if (kv.Value.Flash > 0f) kv.Value.Flash -= dt;
        }

        private static void Bounds2D(float2 p, ref float minX, ref float maxX, ref float minY, ref float maxY)
        {
            if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y; if (p.y > maxY) maxY = p.y;
        }

        private void ConfigureCamera(float minX, float maxX, float minY, float maxY)
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null) { var cams = Camera.allCameras; if (cams != null && cams.Length > 0) _cam = cams[0]; }
                if (_cam == null) return;
            }
            float cx = 0.5f * (minX + maxX), cy = 0.5f * (minY + maxY);
            float halfW = Mathf.Max(2f, 0.5f * (maxX - minX)) + 2f;
            float halfH = Mathf.Max(2f, 0.5f * (maxY - minY)) + 2f;
            float aspect = _cam.aspect > 0.01f ? _cam.aspect : (Screen.height > 0 ? (float)Screen.width / Screen.height : 0.5f);
            float size = Mathf.Max(halfH, halfW / Mathf.Max(0.1f, aspect)) * 1.15f;

            _cam.orthographic = true;
            _cam.orthographicSize = Mathf.Max(3f, size);
            _cam.transform.position = new Vector3(cx, cy, -20f);
            _cam.transform.rotation = Quaternion.identity; // look down +Z at the z=0 plane
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.10f, 0.12f, 0.14f);
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 100f;

            // Battlefield background: stretch the placeholder battle bg to cover the ortho view, behind units.
            Sprite bgSpr = PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get("bg_battle") : null;
            if (bgSpr != null)
            {
                if (_bg == null)
                {
                    var bgo = new GameObject("battlefield_bg");
                    _bg = bgo.AddComponent<SpriteRenderer>();
                    _bg.sortingOrder = -100; // behind all unit/mine/statue proxies
                }
                _bg.sprite = bgSpr; _bg.color = Color.white;
                float viewH = _cam.orthographicSize * 2f, viewW = viewH * Mathf.Max(0.1f, aspect);
                float bw = bgSpr.bounds.size.x, bh = bgSpr.bounds.size.y;
                if (bw < 0.01f) bw = 1f; if (bh < 0.01f) bh = 1f;
                _bg.transform.position = new Vector3(cx, cy, 5f); // z>0 = behind the z=0 plane (camera looks +Z)
                _bg.transform.localScale = new Vector3((viewW * 1.1f) / bw, (viewH * 1.1f) / bh, 1f);
            }

            if (!_camConfigured) { _camConfigured = true; Debug.Log("[PROXY] camera configured to frame the battlefield (orthographic)."); }
        }
    }
}
