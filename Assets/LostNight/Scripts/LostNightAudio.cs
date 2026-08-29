using System.Collections;
using UnityEngine;

namespace LostNight
{
    public sealed class LostNightAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioSource musicSource;
        private AudioClip select;
        private AudioClip discover;
        private AudioClip confirm;
        private AudioClip success;
        private AudioClip failure;
        private AudioClip backgroundMusic;

        public void Initialize()
        {
            if (source == null) source = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.volume = 1f;
            if (musicSource == null || musicSource == source) musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false; musicSource.loop = true; musicSource.volume = 0f;
            select = Tone("Select", 620f, .07f, .18f);
            discover = Chime("Discover", 740f, 1110f, .22f, .3f);
            confirm = Tone("Confirm", 430f, .12f, .24f);
            success = Chime("Success", 523f, 784f, .45f, .34f);
            failure = Chime("Failure", 210f, 145f, .42f, .28f);
            backgroundMusic = CreateStationAmbience();
            musicSource.clip = backgroundMusic;
        }

        private void Awake()
        {
            if (select == null) Initialize();
        }

        public void PlaySelect() => source.PlayOneShot(select);
        public void PlayDiscover() => source.PlayOneShot(discover);
        public void PlayConfirm() => source.PlayOneShot(confirm);
        public void PlaySuccess() => source.PlayOneShot(success);
        public void PlayFailure() => source.PlayOneShot(failure);
        public void Unlock()
        {
            if (FindAnyObjectByType<AudioListener>() == null)
            {
                var listenerTarget = Camera.main != null ? Camera.main.gameObject : gameObject;
                listenerTarget.AddComponent<AudioListener>();
            }
            AudioListener.pause = false;
            PlayConfirm();
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
                StartCoroutine(FadeMusicIn());
            }
        }
        public void SetVolume(float value) => AudioListener.volume = Mathf.Clamp01(value);

        private IEnumerator FadeMusicIn()
        {
            const float targetVolume = .22f;
            for (var elapsed = 0f; elapsed < 1.5f; elapsed += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / 1.5f);
                yield return null;
            }
            musicSource.volume = targetVolume;
        }

        private static AudioClip CreateStationAmbience()
        {
            const int sampleRate = 44100; const float duration = 4f;
            var samples = Mathf.CeilToInt(duration * sampleRate); var data = new float[samples];
            uint noiseState = 19770623; var filteredNoise = 0f;
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                noiseState = noiseState * 1664525u + 1013904223u;
                var noise = ((noiseState >> 8) / 8388607.5f - 1f);
                filteredNoise = Mathf.Lerp(filteredNoise, noise, .018f);
                var drone = Mathf.Sin(2f * Mathf.PI * 55f * t) * .09f
                    + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * .035f;
                var bellTime = t - 1.15f;
                var bellEnvelope = bellTime >= 0f ? Mathf.Exp(-bellTime * 2.8f) : 0f;
                var bell = Mathf.Sin(2f * Mathf.PI * 392f * bellTime) * bellEnvelope * .055f;
                data[i] = drone + filteredNoise * .045f + bell;
            }
            const int crossfadeSamples = 4096;
            for (var i = 0; i < crossfadeSamples; i++)
            {
                var blend = i / (float)crossfadeSamples; var endIndex = samples - crossfadeSamples + i;
                data[endIndex] = Mathf.Lerp(data[endIndex], data[i], blend);
            }
            var clip = AudioClip.Create("Last Train Station Ambience", samples, 1, sampleRate, false);
            clip.SetData(data, 0); return clip;
        }

        private static AudioClip Tone(string name, float frequency, float duration, float amplitude)
        {
            const int sampleRate = 44100; var samples = Mathf.CeilToInt(duration * sampleRate); var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate; var envelope = Mathf.Pow(1f - i / (float)samples, 2f);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * amplitude;
            }
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false); clip.SetData(data, 0); return clip;
        }

        private static AudioClip Chime(string name, float startFrequency, float endFrequency, float duration, float amplitude)
        {
            const int sampleRate = 44100; var samples = Mathf.CeilToInt(duration * sampleRate); var data = new float[samples];
            var phase = 0f;
            for (var i = 0; i < samples; i++)
            {
                var normalized = i / (float)samples; var frequency = Mathf.Lerp(startFrequency, endFrequency, normalized);
                phase += 2f * Mathf.PI * frequency / sampleRate;
                var envelope = Mathf.Sin(Mathf.PI * normalized) * (1f - normalized * .45f);
                data[i] = (Mathf.Sin(phase) + Mathf.Sin(phase * 2.01f) * .18f) * envelope * amplitude;
            }
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false); clip.SetData(data, 0); return clip;
        }
    }
}
