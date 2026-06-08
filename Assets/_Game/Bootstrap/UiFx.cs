// BULWARK — UI FX COMPONENTS (UI Construction Bible · Animation/FX sections). Presentation-only, REMOVABLE.
//
// Reusable code-built uGUI effects the Bible's Section I (Animation Timeline) and Section J (Particle & FX)
// require on most screens: prestige gold gradient titles, CTA/tip-glow pulses, Ken-Burns background push,
// loading-sheen sweeps, drifting embers, smooth count-ups, and idle spins. One tested set so every screen is
// consistent and small. All run on UNSCALED time (menus run at Time.timeScale = 0). NO gameplay/ECS impact (§12).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Vertical colour gradient on a uGUI Graphic mesh (prestige gold titles). top = cap, bottom = base.</summary>
    public sealed class UiGradientText : BaseMeshEffect
    {
        public Color top = UiTheme.GoldHi;
        public Color bottom = UiTheme.Gold;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            var verts = new List<UIVertex>(vh.currentVertCount);
            vh.GetUIVertexStream(verts);
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < verts.Count; i++) { float y = verts[i].position.y; if (y < min) min = y; if (y > max) max = y; }
            float h = Mathf.Max(0.0001f, max - min);
            for (int i = 0; i < verts.Count; i++)
            {
                var v = verts[i];
                float t = (v.position.y - min) / h;
                v.color = Color32.Lerp(bottom, top, t);
                verts[i] = v;
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }
    }

    /// <summary>Oscillates a Graphic's alpha on a sine loop (CTA "tap me" pulse, tip-glow breathe).</summary>
    public sealed class PulseGraphic : MonoBehaviour
    {
        public Graphic target;
        public float min = 0.55f, max = 1.0f, period = 1.6f;
        private void Awake() { if (target == null) target = GetComponent<Graphic>(); }
        private void Update()
        {
            if (target == null) return;
            float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / Mathf.Max(0.01f, period)));
            var c = target.color; c.a = Mathf.Lerp(min, max, t); target.color = c;
        }
    }

    /// <summary>Oscillates localScale on a sine loop (gentle breathing focal elements).</summary>
    public sealed class PulseScale : MonoBehaviour
    {
        public float min = 0.98f, max = 1.02f, period = 2.0f;
        private void Update()
        {
            float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / Mathf.Max(0.01f, period)));
            transform.localScale = Vector3.one * Mathf.Lerp(min, max, t);
        }
    }

    /// <summary>One-shot Ken-Burns: lerp localScale from→to over duration on unscaled time (slow bg push-out).</summary>
    public sealed class KenBurns : MonoBehaviour
    {
        public float from = 1.06f, to = 1.0f, duration = 6f;
        private float _t;
        private void OnEnable() { _t = 0f; transform.localScale = Vector3.one * from; }
        private void Update()
        {
            if (_t >= duration) return;
            _t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_t / duration);
            transform.localScale = Vector3.one * Mathf.Lerp(from, to, k);
        }
    }

    /// <summary>Slides a child RectTransform's anchoredPosition.x left→right on a loop (loading sheen sweep).</summary>
    public sealed class Sheen : MonoBehaviour
    {
        public RectTransform target;
        public float minX, maxX, period = 1.4f;
        private void Update()
        {
            if (target == null) return;
            float t = Mathf.Repeat(Time.unscaledTime / Mathf.Max(0.01f, period), 1f);
            var p = target.anchoredPosition; p.x = Mathf.Lerp(minX, maxX, t); target.anchoredPosition = p;
        }
    }

    /// <summary>Constant rotation (idle loaders / wheel idle). degPerSec negative = clockwise.</summary>
    public sealed class Spin : MonoBehaviour
    {
        public float degPerSec = -90f;
        private void Update() => transform.Rotate(0, 0, degPerSec * Time.unscaledDeltaTime);
    }

    /// <summary>Sparse warm embers drifting upward across the lower frame, recycling. Builds its own motes.</summary>
    public sealed class EmberField : MonoBehaviour
    {
        public int count = 14;
        public Color color = UiTheme.Ember;
        private RectTransform _rt;
        private readonly List<RectTransform> _m = new List<RectTransform>();
        private readonly List<Vector2> _vel = new List<Vector2>();

        private void Start()
        {
            _rt = transform as RectTransform;
            var sp = UiTex.Disc(color, 16);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Ember", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform; rt.SetParent(_rt, false);
                float s = Random.Range(3f, 9f); rt.sizeDelta = new Vector2(s, s);
                var img = go.GetComponent<Image>(); img.sprite = sp; img.raycastTarget = false;
                img.color = new Color(color.r, color.g, color.b, Random.Range(0.15f, 0.5f));
                _m.Add(rt); _vel.Add(new Vector2(Random.Range(-8f, 8f), Random.Range(20f, 55f)));
                Reset(rt, true);
            }
        }

        private void Reset(RectTransform rt, bool anywhere)
        {
            var size = _rt.rect.size;
            float x = Random.Range(-size.x * 0.5f, size.x * 0.5f);
            float y = anywhere ? Random.Range(-size.y * 0.5f, size.y * 0.4f) : -size.y * 0.5f;
            rt.anchoredPosition = new Vector2(x, y);
        }

        private void Update()
        {
            if (_rt == null) return;
            float top = _rt.rect.height * 0.5f, dt = Time.unscaledDeltaTime;
            for (int i = 0; i < _m.Count; i++)
            {
                var rt = _m[i];
                var p = rt.anchoredPosition + _vel[i] * dt;
                p.x += Mathf.Sin(Time.unscaledTime + i) * 6f * dt;
                if (p.y > top) Reset(rt, false); else rt.anchoredPosition = p;
            }
        }
    }

    /// <summary>Swaps a backdrop Image to its real /design-extracted sprite once UiAssets finishes loading
    /// (sprites load async ~1–2 s after boot; this lets even the first screens show the real art when ready).</summary>
    public sealed class BackdropBinder : MonoBehaviour
    {
        private Image _img; private string _key; private bool _done;
        public void Init(Image img, string key) { _img = img; _key = key; }
        private void Update()
        {
            if (_done || _img == null) return;
            var a = UiAssets.Instance; if (a == null || !a.Ready) return;
            var bd = a.Backdrop(_key);
            if (bd != null) { _img.sprite = bd; _img.color = Color.white; _img.preserveAspect = false; }
            _done = true;
        }
    }

    /// <summary>Smoothly tweens an integer display toward a target (loading %, reward counters). Tabular-safe.</summary>
    public sealed class CountUp : MonoBehaviour
    {
        private Text _t;
        private System.Func<int, string> _fmt;
        private float _cur, _target, _speed = 1f;

        public void Bind(Text t, System.Func<int, string> fmt, float startValue = 0f)
        {
            _t = t; _fmt = fmt; _cur = startValue; _target = startValue; Render();
        }
        /// <summary>Tween toward <paramref name="target"/> over roughly <paramref name="seconds"/>.</summary>
        public void To(float target, float seconds = 0.6f)
        {
            _target = target; _speed = Mathf.Abs(target - _cur) / Mathf.Max(0.05f, seconds);
        }
        public void Snap(float v) { _cur = _target = v; Render(); }

        private void Update()
        {
            if (_t == null || Mathf.Approximately(_cur, _target)) return;
            _cur = Mathf.MoveTowards(_cur, _target, _speed * Time.unscaledDeltaTime);
            Render();
        }
        private void Render() { if (_t != null && _fmt != null) _t.text = _fmt(Mathf.RoundToInt(_cur)); }
    }
}
