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

        private sealed class Proxy
        {
            public GameObject Go;
            public MeshRenderer Mr;
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
                        UpdateProxy(ents[i], k, pos[i].Value, frac, hp[i].Current);
                        Bounds2D(pos[i].Value, ref minX, ref maxX, ref minY, ref maxY); total++;
                    }
                }
                // ---- Mines ----
                using (var ents = _qMine.ToEntityArray(Allocator.Temp))
                using (var pos = _qMine.ToComponentDataArray<Position>(Allocator.Temp))
                    for (int i = 0; i < ents.Length && total < MaxProxies; i++)
                    {
                        UpdateProxy(ents[i], Kind.Mine, pos[i].Value, 1f, -1f);
                        Bounds2D(pos[i].Value, ref minX, ref maxX, ref minY, ref maxY); total++;
                    }
                // ---- Statues ----
                using (var ents = _qStatue.ToEntityArray(Allocator.Temp))
                using (var st = _qStatue.ToComponentDataArray<StatueState>(Allocator.Temp))
                using (var pos = _qStatue.ToComponentDataArray<Position>(Allocator.Temp))
                    for (int i = 0; i < ents.Length && total < MaxProxies; i++)
                    {
                        float frac = st[i].MaxHealth > 0f ? Mathf.Clamp01(st[i].Health / st[i].MaxHealth) : 1f;
                        UpdateProxy(ents[i], Kind.Statue, pos[i].Value, frac, st[i].Health);
                        Bounds2D(pos[i].Value, ref minX, ref maxX, ref minY, ref maxY); total++;
                    }
            }
            catch (System.Exception e) { Debug.LogError("[PROXY] read error: " + e.Message); }

            CullStale();
            DecayFlashes(dt);
            if (total > 0) ConfigureCamera(minX, maxX, minY, maxY);

            _logTimer += dt;
            if (_logTimer >= 2f) { _logTimer = 0f; Debug.Log($"[PROXY] proxies={_proxies.Count} (rendering {total} entities as primitives)."); }
        }

        private void UpdateProxy(Entity e, Kind kind, float2 p, float hpFrac, float curHp)
        {
            _seen.Add(e);
            if (!_proxies.TryGetValue(e, out Proxy px))
            {
                px = CreateProxy(kind);
                px.LastHp = curHp;
                _proxies[e] = px;
                Debug.Log($"[PROXY] SPAWN {kind} proxy (entity index {e.Index}).");
            }

            // Combat viz: a drop in HP since last frame -> flash white briefly.
            if (curHp >= 0f && curHp < px.LastHp - 0.01f) px.Flash = 0.25f;
            px.LastHp = curHp;

            // Position on the z=0 battlefield plane.
            px.Go.transform.position = new Vector3(p.x, p.y, 0f);

            // Health viz: scale (units/statues shrink as damaged). Mines fixed size.
            float s;
            switch (kind)
            {
                case Kind.Statue: s = 1.6f; px.Go.transform.localScale = new Vector3(1.4f, 1.2f + 1.8f * hpFrac, 1.4f); break;
                case Kind.Mine:   s = 0.9f; px.Go.transform.localScale = new Vector3(s, s, s); break;
                default:          px.Go.transform.localScale = new Vector3(0.7f, 0.5f + 0.9f * hpFrac, 0.7f); break;
            }

            // Color: team/kind base, dimmed by HP, white when flashing (combat hit).
            Color baseCol = kind == Kind.UnitP ? ColP : kind == Kind.UnitAI ? ColAI : kind == Kind.Mine ? ColMine : ColStatue;
            Color c = px.Flash > 0f ? Color.white : Color.Lerp(baseCol * 0.4f, baseCol, hpFrac);
            _mpb.Clear();
            _mpb.SetColor("_BaseColor", c); // URP default material
            _mpb.SetColor("_Color", c);     // built-in fallback
            px.Mr.SetPropertyBlock(_mpb);
        }

        private Material _proxyMat;

        /// <summary>Shared URP material for all proxies. CreatePrimitive's DEFAULT material is the built-in
        /// Standard shader, which renders MAGENTA under URP and ignores the MPB _BaseColor tint — so we must
        /// assign an actual URP shader here for the team colors to show. Per-proxy color is then applied via
        /// the MaterialPropertyBlock (_BaseColor) over this shared material.</summary>
        private Material ProxyMat()
        {
            if (_proxyMat != null) return _proxyMat;
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Unlit/Color");
            _proxyMat = sh != null ? new Material(sh) : null;
            Debug.Log("[PROXY] proxy material shader = " + (sh != null ? sh.name : "NULL (kept default — may render magenta)"));
            return _proxyMat;
        }

        private Proxy CreateProxy(Kind kind)
        {
            PrimitiveType pt = kind == Kind.Mine ? PrimitiveType.Cube
                             : kind == Kind.Statue ? PrimitiveType.Cylinder
                             : PrimitiveType.Capsule;
            var go = GameObject.CreatePrimitive(pt);
            go.name = "proxy_" + kind;
            var col = go.GetComponent<Collider>(); if (col != null) Destroy(col); // debug visual only; no physics
            var mr = go.GetComponent<MeshRenderer>();
            var m = ProxyMat(); if (m != null) mr.sharedMaterial = m; // URP shader so the team-color tint shows (not magenta)
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            return new Proxy { Go = go, Mr = mr, Kind = kind };
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
                if (_proxies.TryGetValue(dead[i], out var px) && px.Go != null) Destroy(px.Go);
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
            if (!_camConfigured) { _camConfigured = true; Debug.Log("[PROXY] camera configured to frame the battlefield (orthographic)."); }
        }
    }
}
