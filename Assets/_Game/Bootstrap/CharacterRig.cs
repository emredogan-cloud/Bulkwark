// STICK EMPIRE RISE — PHASE 6 SHARED CHARACTER RIG (presentation-only, §12, REMOVABLE).
//
// THE definitive character production standard: ONE master stick skeleton (code-built bone Transform hierarchy
// per the Phase-6 spec), shared by every archetype; equipment is modular overlays on bone slots; the animation
// vocabulary is procedural and shared. Animation MIRRORS ECS state (driven externally by SimProxyRenderer from
// read-only ECS reads) — it never leads or writes the sim (§12). Faction identity is silhouette-first (weapon/
// head profile) + an accent tint, so it survives grayscale. LOD scales update cost. No frame sheets, no unique
// rigs, no external deps. (Unity 2D Animation SpriteSkin/IK would wrap THIS same skeleton for mesh-deform via the
// editor Skinning module — an authoring upgrade; the architecture/standard is established here.)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulwark.Bootstrap
{
    /// <summary>Loads + caches the shared character part/equipment sprites once (with per-slot pivots).</summary>
    public sealed class CharacterAssets : MonoBehaviour
    {
        public static CharacterAssets Instance { get; private set; }
        public bool Ready { get; private set; }
        private readonly Dictionary<string, Sprite> _s = new Dictionary<string, Sprite>(24);

        // key -> pivot (so limbs hang from the joint, equipment rotates about its grip, head-gear sits on the head)
        private static readonly (string key, float px, float py)[] Parts =
        {
            ("cp_limb", 0.5f, 1.0f), ("cp_head", 0.5f, 0.5f), ("cp_torso", 0.5f, 0.0f),
            ("ce_sword", 0.5f, 0.92f), ("ce_bow", 0.5f, 0.5f), ("ce_spear", 0.5f, 0.62f),
            ("ce_pickaxe", 0.5f, 0.92f), ("ce_staff", 0.5f, 0.85f), ("ce_shield", 0.5f, 0.5f),
            ("ce_helm_iron", 0.5f, 0.18f), ("ce_helm_crested", 0.5f, 0.30f), ("ce_hood", 0.5f, 0.3f),
            ("ce_hat_wizard", 0.5f, 0.10f), ("ce_satchel", 0.5f, 0.5f), ("ce_cape", 0.5f, 1.0f),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("CharacterAssets"); Instance = go.AddComponent<CharacterAssets>(); DontDestroyOnLoad(go);
        }
        private void Awake() { if (Instance == null) Instance = this; StartCoroutine(Load()); }
        private IEnumerator Load()
        {
            string dir = Application.streamingAssetsPath + "/bulwark_ui/";
            foreach (var p in Parts)
            {
                string url = dir + p.key + ".png"; if (!url.Contains("://")) url = "file://" + url;
                using (var r = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return r.SendWebRequest();
                    if (r.result == UnityWebRequest.Result.Success)
                    {
                        var t = DownloadHandlerTexture.GetContent(r); t.wrapMode = TextureWrapMode.Clamp;
                        _s[p.key] = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(p.px, p.py), 100f);
                    }
                }
            }
            Ready = true; Debug.Log($"[RIG] character sprites ready: {_s.Count}/{Parts.Length}.");
        }
        public Sprite Get(string k) => _s.TryGetValue(k, out var s) ? s : null;
    }

    /// <summary>The shared 2D bone rig. Build(archetype, blue) once; SetState() each frame mirrors ECS. §12.</summary>
    public sealed class CharacterRig : MonoBehaviour
    {
        public enum Arch { Skirmisher = 0, Shield = 1, Heavy = 2, Ranged = 3, Caster = 4, Miner = 5 }
        public enum St { Idle, Walk, Attack, Cast, Mine, Hit, Death, Celebrate }

        // ---- master skeleton (exact hierarchy from the Phase-6 spec) ----
        private Transform _root, _ground, _torso, _head, _lArm, _lFore, _lHand, _rArm, _rFore, _rHand;
        private Transform _capeAnchor, _weaponSlot, _offhandSlot, _accessorySlot, _fxAnchor;
        private Transform _lLeg, _lShin, _lFoot, _rLeg, _rShin, _rFoot;
        private SpriteRenderer _torsoSr, _headSr, _uaL, _faL, _uaR, _faR, _ulL, _slL, _ulR, _slR, _cape;

        private Arch _arch; private bool _blue; private bool _built;
        private St _state = St.Idle, _prev = St.Idle; private float _t, _attackCd;
        private int _lod; private int _sortBase;
        private static readonly Color BlueAcc = new Color(0.30f, 0.55f, 1f), RedAcc = new Color(1f, 0.38f, 0.28f);

        private Sprite P(string k) => CharacterAssets.Instance != null ? CharacterAssets.Instance.Get(k) : null;

        /// <summary>Set archetype/faction/sort before assets are ready; Build() runs lazily once they load.</summary>
        public void Configure(Arch a, bool blue, int sortBase) { _arch = a; _blue = blue; _sortBase = sortBase; }
        public bool Built => _built;

        public void Build(Arch arch, bool blue, int sortBase)
        {
            _arch = arch; _blue = blue; _sortBase = sortBase;
            if (CharacterAssets.Instance == null || !CharacterAssets.Instance.Ready) return; // retry next frame
            _root = NewBone("Root", transform, 0, 0);
            _ground = NewBone("GroundAnchor", _root, 0, 0);
            _torso = NewBone("Torso", _root, 0, 0.42f);
            _head = NewBone("Head", _torso, 0, 0.46f);
            NewBone("EyeFXAnchor", _head, 0, 0);
            _lArm = NewBone("LeftArm", _torso, -0.06f, 0.40f); _lFore = NewBone("LeftForearm", _lArm, 0, -0.18f); _lHand = NewBone("LeftHand", _lFore, 0, -0.16f);
            _rArm = NewBone("RightArm", _torso, 0.06f, 0.40f); _rFore = NewBone("RightForearm", _rArm, 0, -0.18f); _rHand = NewBone("RightHand", _rFore, 0, -0.16f);
            _capeAnchor = NewBone("CapeAnchor", _torso, 0, 0.40f);
            _weaponSlot = NewBone("WeaponSlot", _rHand, 0, 0); _offhandSlot = NewBone("OffhandSlot", _lHand, 0, 0);
            _accessorySlot = NewBone("AccessorySlot", _torso, 0.05f, 0.04f); _fxAnchor = NewBone("FXAnchor", _torso, 0, 0.20f);
            _lLeg = NewBone("LeftLeg", _root, -0.07f, 0.42f); _lShin = NewBone("LeftShin", _lLeg, 0, -0.21f); _lFoot = NewBone("LeftFoot", _lShin, 0, -0.20f);
            _rLeg = NewBone("RightLeg", _root, 0.07f, 0.42f); _rShin = NewBone("RightShin", _rLeg, 0, -0.21f); _rFoot = NewBone("RightFoot", _rShin, 0, -0.20f);

            // limb visuals (cp_limb stretched along each segment; pivot top so it hangs from the joint)
            _torsoSr = Limb(_torso, "cp_torso", 0.44f, 6);
            _headSr = Part(_head, "cp_head", 0.34f, 8);
            _uaL = Limb(_lArm, "cp_limb", 0.20f, 4); _faL = Limb(_lFore, "cp_limb", 0.17f, 4);
            _uaR = Limb(_rArm, "cp_limb", 0.20f, 7); _faR = Limb(_rFore, "cp_limb", 0.17f, 7);
            _ulL = Limb(_lLeg, "cp_limb", 0.22f, 4); _slL = Limb(_lShin, "cp_limb", 0.20f, 4);
            _ulR = Limb(_rLeg, "cp_limb", 0.22f, 5); _slR = Limb(_rShin, "cp_limb", 0.20f, 5);

            EquipFor(arch);
            _built = true;
        }

        private void EquipFor(Arch a)
        {
            // cape (capes for hero-ish frontline; tinted by faction). Accent = faction colour.
            Color acc = _blue ? BlueAcc : RedAcc;
            switch (a)
            {
                case Arch.Shield:
                    Part(_weaponSlot, "ce_sword", 0.30f, 9); Part(_offhandSlot, "ce_shield", 0.34f, 9, acc); Part(_head, "ce_helm_iron", 0.40f, 9); break;
                case Arch.Ranged:
                    Part(_offhandSlot, "ce_bow", 0.40f, 9); Part(_head, "ce_hood", 0.46f, 9, acc); break;
                case Arch.Heavy:   // spearman-style reach in the launch set
                    Part(_weaponSlot, "ce_spear", 0.60f, 9); Part(_head, "ce_helm_crested", 0.44f, 9, acc); break;
                case Arch.Miner:
                    Part(_weaponSlot, "ce_pickaxe", 0.42f, 9); Part(_accessorySlot, "ce_satchel", 0.26f, 6); break;
                case Arch.Caster:
                    Part(_weaponSlot, "ce_staff", 0.55f, 9, acc); Part(_head, "ce_hat_wizard", 0.46f, 9, acc); break;
                default:           // Skirmisher: sword only
                    Part(_weaponSlot, "ce_sword", 0.30f, 9); Part(_head, "ce_helm_iron", 0.36f, 8); break;
            }
            // faction cape for frontline/hero archetypes (silhouette + accent)
            if (a == Arch.Shield || a == Arch.Heavy) { _cape = Part(_capeAnchor, "ce_cape", 0.5f, 3, acc); }
        }

        // ---- per-frame procedural animation (mirrors externally-set state) ----
        public void SetState(St s) { if (s != _state) { _prev = _state; _state = s; _t = 0f; } }
        public St CurrentState => _state;
        public void SetLod(int lod) { _lod = lod; }
        public void Flinch() { SetState(St.Hit); }
        public void PlayDeath() { SetState(St.Death); }
        public bool DeathDone => _state == St.Death && _t > 1.2f;

        private void Update()
        {
            if (!_built) { if (CharacterAssets.Instance != null && CharacterAssets.Instance.Ready) Build(_arch, _blue, _sortBase); return; }
            if (_lod >= 3 && _state != St.Death && (Time.frameCount & 3) != 0) return; // LOD3: idle approximation, sparse
            if (_lod == 2 && (Time.frameCount & 1) != 0 && _state != St.Death) return; // LOD2: half-rate
            float dt = Time.unscaledDeltaTime; _t += dt;
            float tt = Time.unscaledTime;
            switch (_state)
            {
                case St.Idle: Idle(tt); break;
                case St.Walk: Walk(tt); break;
                case St.Attack: Attack(); break;
                case St.Cast: Cast(); break;
                case St.Mine: Mine(); break;
                case St.Hit: Hit(); break;
                case St.Death: Death(); break;
                case St.Celebrate: Celebrate(tt); break;
            }
        }

        private void Z(Transform b, float deg) { if (b != null) b.localRotation = Quaternion.Euler(0, 0, deg); }
        private void Idle(float t)
        {
            float br = Mathf.Sin(t * 1.6f) * 2f;
            Z(_torso, br * 0.5f); Z(_lArm, 8 + br); Z(_rArm, -8 - br); Z(_lLeg, 2); Z(_rLeg, -2); Z(_lFore, -4); Z(_rFore, -4);
            if (_torso) _torso.localPosition = new Vector3(0, 0.42f + Mathf.Sin(t * 1.6f) * 0.008f, 0);
        }
        private void Walk(float t)
        {
            float s = Mathf.Sin(t * 9f), c = Mathf.Cos(t * 9f);
            Z(_lLeg, s * 26f); Z(_rLeg, -s * 26f); Z(_lShin, -Mathf.Max(0, s) * 22f); Z(_rShin, -Mathf.Max(0, -s) * 22f);
            Z(_lArm, -s * 22f + 6); Z(_rArm, s * 22f - 6); Z(_lFore, -10); Z(_rFore, -10);
            Z(_torso, c * 2f); if (_torso) _torso.localPosition = new Vector3(0, 0.42f + Mathf.Abs(s) * 0.02f, 0);
        }
        private void Attack()
        {
            float p = Mathf.Clamp01(_t / 0.35f);                  // quick forward swing then settle
            float sw = Mathf.Sin(p * Mathf.PI) * 95f;
            Z(_rArm, -10 - sw); Z(_rFore, -10 + sw * 0.3f); Z(_lArm, 14); Z(_torso, sw * 0.08f);
            Z(_lLeg, 6); Z(_rLeg, -6);
            if (_t > 0.45f) SetState(St.Idle);
        }
        private void Cast()
        {
            float p = Mathf.Clamp01(_t / 0.6f);
            Z(_rArm, -40 - 30 * Mathf.Sin(p * Mathf.PI)); Z(_lArm, 30); Z(_torso, -3);
            if (_fxAnchor && _t < 0.1f) { var g = CharFx(_fxAnchor, _blue ? BlueAcc : new Color(0.7f, 0.5f, 1f)); }
            if (_t > 0.7f) SetState(St.Idle);
        }
        private void Mine()
        {
            float s = Mathf.Sin(Time.unscaledTime * 6f);
            Z(_rArm, -30 - Mathf.Max(0, s) * 70f); Z(_rFore, -10); Z(_lArm, 18); Z(_torso, Mathf.Max(0, s) * 6f);
        }
        private void Hit()
        {
            float p = Mathf.Clamp01(_t / 0.25f);
            Z(_torso, -14 * (1 - p)); Z(_head, 8 * (1 - p)); Z(_lArm, 18); Z(_rArm, -18);
            if (_t > 0.25f) SetState(St.Idle);
        }
        private void Death()
        {
            // collapse within 1.2s (readable, no ragdoll chaos): fall back + fade
            float p = Mathf.Clamp01(_t / 1.0f);
            if (_root) _root.localRotation = Quaternion.Euler(0, 0, -85f * p);
            float a = 1f - Mathf.Clamp01((_t - 0.6f) / 0.6f);
            SetAlpha(a);
            if (_t > 1.25f) Destroy(gameObject); // pooled cleanup: collapse complete, remove the proxy
        }
        private void Celebrate(float t)
        {
            float s = Mathf.Abs(Mathf.Sin(t * 4f));
            Z(_lArm, 120 + s * 20f); Z(_rArm, -120 - s * 20f); Z(_torso, 0);
            if (_torso) _torso.localPosition = new Vector3(0, 0.42f + s * 0.05f, 0);
        }

        // ---- helpers ----
        private Transform NewBone(string n, Transform parent, float x, float y)
        {
            var go = new GameObject(n); var tr = go.transform; tr.SetParent(parent, false); tr.localPosition = new Vector3(x, y, 0); return tr;
        }
        private SpriteRenderer Limb(Transform bone, string key, float len, int order)
        {
            var go = new GameObject("v"); go.transform.SetParent(bone, false);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = P(key); sr.sortingOrder = _sortBase + order; sr.color = Color.white;
            // pivot top → stretch down the bone's -Y (the child joint sits at local (0,-len))
            float h = sr.sprite != null ? sr.sprite.bounds.size.y : 1f; if (h < 0.01f) h = 1f;
            float w = sr.sprite != null ? sr.sprite.bounds.size.x : 1f; if (w < 0.01f) w = 1f;
            go.transform.localScale = new Vector3((len * 0.22f) / w, len / h, 1f);
            go.transform.localRotation = Quaternion.identity;
            return sr;
        }
        private SpriteRenderer Part(Transform bone, string key, float size, int order, Color? accent = null)
        {
            var s = P(key); if (s == null) return null;
            var go = new GameObject(key); go.transform.SetParent(bone, false);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = s; sr.sortingOrder = _sortBase + order;
            sr.color = accent.HasValue ? accent.Value : Color.white;
            float h = s.bounds.size.y; if (h < 0.01f) h = 1f;
            go.transform.localScale = Vector3.one * (size / h);
            return sr;
        }
        private SpriteRenderer CharFx(Transform bone, Color c)
        {
            var go = new GameObject("castfx"); go.transform.SetParent(bone, false);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = UiTex.Disc(c, 24); sr.sortingOrder = _sortBase + 11; sr.color = new Color(c.r, c.g, c.b, 0.7f);
            go.transform.localScale = Vector3.one * 0.4f; go.AddComponent<DebrisBit>().fade = true;
            return sr;
        }
        private readonly List<SpriteRenderer> _all = new List<SpriteRenderer>(20);
        private void SetAlpha(float a)
        {
            if (_all.Count == 0) GetComponentsInChildren(true, _all);
            foreach (var sr in _all) if (sr != null) { var c = sr.color; c.a = a; sr.color = c; }
        }
    }
}
