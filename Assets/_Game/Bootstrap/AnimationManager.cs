// BULWARK — ANIMATION FRAMEWORK (Production Presentation phase, ADR-5-003 Option A · Priority #5). REMOVABLE.
//
// Skeletal (Spine) animation is DEFERRED (paid spine-unity runtime, version-split sets — ANIMATION_DISCOVERY §1)
// and there are NO baked unit frame-sheets, so this layer animates the existing STATIC archetype sprites
// PROCEDURALLY via transform: idle breathe, walk bounce + lean, hit recoil, and a death fall/fade — no new assets,
// no paid runtime.
//
// PRESENTATION-ONLY: NO gameplay dependency, ZERO ECS writes. Each per-proxy SpriteAnimator runs in LateUpdate and
// modulates the renderer's per-frame BASE transform (which SimProxyRenderer resets every Update, so there is no
// accumulation). State is inferred READ-ONLY from what the renderer already observes (proxy moved? recently hit?
// culled?). It changes no rule, balance, AI, economy, or canon. Deleting this one file removes animation 100%
// (SimProxyRenderer guards every call with AnimationManager.Instance?. / px.Anim?.).

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>Procedural per-proxy sprite animator. Lives on a unit proxy GameObject; presentation-only.</summary>
    public sealed class SpriteAnimator : MonoBehaviour
    {
        // tuning (constants — presentation feel only)
        private const float MoveThreshold = 0.20f; // world units/sec above which the proxy is "walking"
        private const float WalkHz = 2.2f, IdleHz = 0.6f;
        private const float WalkBob = 0.12f, IdleBob = 0.035f;
        private const float WalkSquash = 0.06f, IdleBreathe = 0.025f;
        private const float LeanDeg = 6f;
        private const float HitDur = 0.2f, HitPunch = 0.25f;
        private const float DeathDur = 0.45f;

        private SpriteRenderer _sr;
        private Vector3 _lastBase;
        private bool _inited;
        private float _phase, _hitTimer;

        private bool _dying;
        private float _deathT0;
        private Vector3 _deathPos, _deathScale;
        private Color _deathColor;

        public void Init(SpriteRenderer sr) { _sr = sr; }

        /// <summary>Recent-damage reaction (a brief recoil punch). Called read-only by the renderer on HP-drop.</summary>
        public void NotifyHit() { if (!_dying) _hitTimer = HitDur; }

        /// <summary>Begin the death tween, then self-destruct. The renderer has already removed this proxy from its
        /// live set; the entity is gone in the sim — this only animates the orphaned visual.</summary>
        public void PlayDeath()
        {
            if (_dying) return;
            _dying = true;
            _deathT0 = Time.unscaledTime;
            _deathPos = transform.position;
            _deathScale = transform.localScale;
            _deathColor = _sr != null ? _sr.color : Color.white;
        }

        private void LateUpdate()
        {
            if (_dying) { TickDeath(); return; }

            Vector3 baseP = transform.position;       // the renderer's value this frame
            Vector3 baseS = transform.localScale;
            if (!_inited) { _lastBase = baseP; _inited = true; }

            float dt = Time.deltaTime;
            float speed = dt > 1e-4f ? (baseP - _lastBase).magnitude / dt : 0f;
            _lastBase = baseP;
            bool moving = speed > MoveThreshold;

            _phase += (moving ? WalkHz : IdleHz) * dt;
            float wave = Mathf.Sin(_phase * 2f * Mathf.PI);

            float bob = wave * (moving ? WalkBob : IdleBob);
            if (moving && bob < 0f) bob = -bob; // walking hops up (no sinking below the feet)

            float breathe = 1f + (moving ? Mathf.Abs(wave) * WalkSquash : wave * IdleBreathe);
            float recoil = 1f;
            if (_hitTimer > 0f) { _hitTimer -= dt; recoil = 1f + (_hitTimer / HitDur) * HitPunch; }
            float m = breathe * recoil;
            float lean = moving ? wave * LeanDeg : 0f;

            transform.position = new Vector3(baseP.x, baseP.y + bob, baseP.z);
            transform.localScale = new Vector3(baseS.x * m, baseS.y * m, baseS.z);
            transform.rotation = Quaternion.Euler(0f, 0f, lean);
        }

        private void TickDeath()
        {
            float t = (Time.unscaledTime - _deathT0) / DeathDur;
            if (t >= 1f) { Destroy(gameObject); return; }
            transform.position = new Vector3(_deathPos.x, _deathPos.y - 0.25f * t, _deathPos.z);
            transform.localScale = _deathScale * (1f - 0.7f * t);
            transform.rotation = Quaternion.Euler(0f, 0f, -80f * t); // topple
            if (_sr != null) { var c = _deathColor; c.a = _deathColor.a * (1f - t); _sr.color = c; }
        }
    }

    /// <summary>Factory/owner for the procedural sprite-animation layer. Presentation-only; no ECS writes.</summary>
    public sealed class AnimationManager : MonoBehaviour
    {
        public static AnimationManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("AnimationManager");
            Instance = go.AddComponent<AnimationManager>();
            DontDestroyOnLoad(go);
            Debug.Log("[ANIM] AnimationManager booted (procedural sprite animation).");
        }

        private void Awake() { if (Instance == null) Instance = this; }

        /// <summary>Attach a procedural animator to a unit proxy and return it (the renderer stores the ref).</summary>
        public SpriteAnimator Attach(GameObject proxy, SpriteRenderer sr)
        {
            if (proxy == null) return null;
            var a = proxy.AddComponent<SpriteAnimator>();
            a.Init(sr);
            return a;
        }
    }
}
