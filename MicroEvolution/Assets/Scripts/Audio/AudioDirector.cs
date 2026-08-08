using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Audio
{
    /// <summary>
    /// Lightweight procedural SFX using AudioClip.Create — no external audio assets.
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        AudioSource _sfx;
        AudioSource _ambience;
        bool _muted;

        void Awake()
        {
            Instance = this;
            _muted = SaveSystem.LoadMeta().muted;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _ambience = gameObject.AddComponent<AudioSource>();
            _ambience.loop = true;
            _ambience.volume = 0.12f;
            _ambience.clip = BuildTone(72f, 2f, 0.15f, true);
            if (!_muted) _ambience.Play();

            GameEvents.AteFood += () => PlayBlip(660f, 0.06f, 0.2f);
            GameEvents.PlayerHurt += _ => PlayBlip(140f, 0.12f, 0.28f);
            GameEvents.Evolve += () => PlayBlip(880f, 0.18f, 0.25f);
            GameEvents.KillPulse += () => PlayBlip(220f, 0.1f, 0.22f);
        }

        public void SetMuted(bool muted)
        {
            _muted = muted;
            SaveSystem.SetMuted(muted);
            if (_muted) _ambience.Pause();
            else if (!_ambience.isPlaying) _ambience.Play();
        }

        public bool Muted => _muted;

        public void PlayUi() => PlayBlip(520f, 0.05f, 0.15f);

        void PlayBlip(float freq, float duration, float volume)
        {
            if (_muted) return;
            _sfx.PlayOneShot(BuildTone(freq, duration, volume, false), volume);
        }

        static AudioClip BuildTone(float freq, float duration, float volume, bool soft)
        {
            var sampleRate = 44100;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var env = soft
                    ? 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 0.5f)
                    : Mathf.Clamp01(1f - t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * volume;
                if (!soft)
                    data[i] += Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t) * env * volume * 0.25f;
            }

            var clip = AudioClip.Create($"tone-{freq}", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
