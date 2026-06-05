// BULWARK — AUDIO FRAMEWORK (Production Presentation phase, ADR-5-003 Option A · Priority #3). TEMPORARY/REMOVABLE.
//
// The first complete audio layer: an AudioManager singleton owning a MusicChannel (looping background music,
// no-restart / no-duplicate) and an SfxChannel (one-shot effects). Clips are the curated placeholder OGGs in
// StreamingAssets/bulwark_audio/, loaded at runtime via UnityWebRequestMultimedia (no editor import) — the same
// robust pattern as the sprite loader.
//
// PRESENTATION-ONLY: this layer has NO gameplay dependency and writes NOTHING to the ECS world. It is driven by
// the presentation flow (UiFlow screen changes, uGUI button clicks, and the renderer's READ-ONLY observation of
// hits/deaths). It changes no rule, balance, AI, economy, or canon. Deleting this one file removes audio 100%.
//
// LICENSING: every clip under StreamingAssets/bulwark_audio/ is © ripped (Stick War / Stickman Master) — THROWAWAY
// DEV PLACEHOLDER ONLY, replace with original/licensed audio before any release.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulwark.Bootstrap
{
    /// <summary>Looping background-music channel: switches tracks without restarting the current one or doubling.</summary>
    public sealed class MusicChannel
    {
        private readonly AudioSource _src;
        private string _current;
        public MusicChannel(AudioSource src) { _src = src; _src.loop = true; _src.playOnAwake = false; _src.spatialBlend = 0f; }

        /// <summary>Play <paramref name="clip"/> (keyed by <paramref name="key"/>) on loop. No-op if that key is
        /// already the playing track (prevents restart + duplicate playback). Null clip → stop.</summary>
        public void Play(string key, AudioClip clip)
        {
            if (_current == key && _src.isPlaying) return;
            _current = key;
            if (clip == null) { _src.Stop(); return; }
            _src.clip = clip; _src.loop = true; _src.Play();
        }

        public void Stop() { _current = null; _src.Stop(); }
        public void SetVolume(float v) => _src.volume = Mathf.Clamp01(v);
    }

    /// <summary>One-shot SFX channel.</summary>
    public sealed class SfxChannel
    {
        private readonly AudioSource _src;
        public SfxChannel(AudioSource src) { _src = src; _src.loop = false; _src.playOnAwake = false; _src.spatialBlend = 0f; }
        public void PlayOneShot(AudioClip clip, float vol) { if (clip != null) _src.PlayOneShot(clip, Mathf.Clamp01(vol)); }
        public void SetVolume(float v) => _src.volume = Mathf.Clamp01(v);
    }

    /// <summary>Presentation audio façade: menu/battle music, victory/defeat stingers, and UI/combat SFX.</summary>
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        public bool Ready { get; private set; }

        private static readonly string[] Names =
        {
            "menu_music", "battle_music", "victory", "defeat",
            "sfx_click", "sfx_train", "sfx_hit", "sfx_death",
        };

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private MusicChannel _music;
        private SfxChannel _sfx;
        private bool _victoryFired, _defeatFired; // stingers fire exactly once per match-end
        private float _lastHit = -10f, _lastDeath = -10f; // throttle combat SFX

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("AudioManager");
            Instance = go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
            Debug.Log("[AUDIO] AudioManager booted; loading clips…");
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            var mGo = new GameObject("Music"); mGo.transform.SetParent(transform, false);
            var sGo = new GameObject("Sfx"); sGo.transform.SetParent(transform, false);
            _music = new MusicChannel(mGo.AddComponent<AudioSource>());
            _sfx = new SfxChannel(sGo.AddComponent<AudioSource>());
            _music.SetVolume(0.6f);
            StartCoroutine(LoadAll());
        }

        private IEnumerator LoadAll()
        {
            string baseDir = Application.streamingAssetsPath + "/bulwark_audio/";
            int ok = 0;
            foreach (var n in Names)
            {
                string path = baseDir + n + ".ogg";
                string url = path.Contains("://") ? path : "file://" + path;
                using (var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        var clip = DownloadHandlerAudioClip.GetContent(req);
                        if (clip != null) { _clips[n] = clip; ok++; }
                    }
                    else Debug.LogWarning($"[AUDIO] failed to load {n}: {req.error}");
                }
            }
            Ready = true;
            Debug.Log($"[AUDIO] clips ready: {ok}/{Names.Length}.");
        }

        private AudioClip Clip(string key) => _clips.TryGetValue(key, out var c) ? c : null;

        // ---------------- music (called by UiFlow on screen change) ----------------
        public void PlayMenuMusic() { if (_music != null) _music.Play("menu", Clip("menu_music")); }
        public void PlayBattleMusic() { if (_music != null) _music.Play("battle", Clip("battle_music")); }
        public void StopMusic() { _music?.Stop(); }

        /// <summary>New match starting — allow the stingers to fire again.</summary>
        public void ResetStingers() { _victoryFired = false; _defeatFired = false; }

        /// <summary>Match ended: stop the loop and fire the victory/defeat stinger EXACTLY ONCE.</summary>
        public void PlayEndStinger(bool victory)
        {
            _music?.Stop();
            if (victory) { if (_victoryFired) return; _victoryFired = true; _sfx?.PlayOneShot(Clip("victory"), 1f); }
            else { if (_defeatFired) return; _defeatFired = true; _sfx?.PlayOneShot(Clip("defeat"), 1f); }
        }

        // ---------------- sfx (called by UiFlow / BattleHud / SimProxyRenderer) ----------------
        private bool _muted;
        public bool Muted => _muted;
        /// <summary>Mute/unmute both channels (presentation setting; no gameplay effect).</summary>
        public void ToggleMute()
        {
            _muted = !_muted;
            _music?.SetVolume(_muted ? 0f : 0.6f);
            _sfx?.SetVolume(_muted ? 0f : 1f);
            Debug.Log("[AUDIO] muted=" + _muted);
        }

        public void Click() => _sfx?.PlayOneShot(Clip("sfx_click"), 0.8f);
        public void Train() => _sfx?.PlayOneShot(Clip("sfx_train"), 0.9f);

        public void Hit()
        {
            if (Time.unscaledTime - _lastHit < 0.12f) return; // throttle so massed combat doesn't machine-gun
            _lastHit = Time.unscaledTime; _sfx?.PlayOneShot(Clip("sfx_hit"), 0.5f);
        }

        public void Death()
        {
            if (Time.unscaledTime - _lastDeath < 0.15f) return;
            _lastDeath = Time.unscaledTime; _sfx?.PlayOneShot(Clip("sfx_death"), 0.6f);
        }
    }
}
