using System;
using UnityEngine;

namespace LostNight
{
    public sealed class LostNightAudio : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip select;
        private AudioClip discover;
        private AudioClip confirm;
        private AudioClip success;
        private AudioClip failure;

        public void Initialize()
        {
            source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.volume = 1f;
            select = Tone("Select", 620f, .07f, .18f);
            discover = Chime("Discover", 740f, 1110f, .22f, .3f);
            confirm = Tone("Confirm", 430f, .12f, .24f);
            success = Chime("Success", 523f, 784f, .45f, .34f);
            failure = Chime("Failure", 210f, 145f, .42f, .28f);
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
        }
        public void SetVolume(float value) => AudioListener.volume = Mathf.Clamp01(value);

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
