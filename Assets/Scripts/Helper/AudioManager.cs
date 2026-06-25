using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Settings")]
        public const int OneShotPoolSize = 12;
        public float MasterVolume { get; private set; } // 0-1
        public float MusicVolume { get; private set; } // 0-1
        public float SfxVolume { get; private set; } // 0-1

        [Header("Music")]
        public AudioClip[] AmbientTracks;
        public float MusicCrossfadeDuration = 2f;

        // Music
        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private bool musicSourceAActive = true;
        private int currentTrackIndex = -1;
        private Coroutine crossfadeCoroutine;

        // One-shot pool
        private AudioSource[] oneShotPool;
        private int nextOneShotIndex;

        public static bool IsMuted => Instance.MasterVolume <= 0f;

        void Awake()
        {
            InitMusicSources();
            InitOneShotPool();

            MasterVolume = 1f;
            MusicVolume = 1f;
            SfxVolume = 1f;
        }

        private void InitMusicSources()
        {
            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceA.loop = false;
            musicSourceA.playOnAwake = false;
            musicSourceA.volume = 0f;

            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceB.loop = false;
            musicSourceB.playOnAwake = false;
            musicSourceB.volume = 0f;
        }

        private void InitOneShotPool()
        {
            oneShotPool = new AudioSource[OneShotPoolSize];
            for (int i = 0; i < OneShotPoolSize; i++)
            {
                oneShotPool[i] = gameObject.AddComponent<AudioSource>();
                oneShotPool[i].playOnAwake = false;
            }
        }

        void Update()
        {
            // Auto-advance to next track
            AudioSource activeMusic = musicSourceAActive ? musicSourceA : musicSourceB;
            if (activeMusic.clip != null && !activeMusic.isPlaying && crossfadeCoroutine == null)
            {
                PlayNextTrack();
            }
        }

        // ==================== ONE-SHOT SOUNDS ====================

        /// <summary>
        /// Play a sound effect once. Supports overlapping.
        /// </summary>
        public static void PlaySound(string name, float volume = 1f, float pitch = 1f, bool applySpeedModifier = false)
        {
            // Load AudioClip
            AudioClip clip = ResourceManager.LoadAudioClip($"Audio/SFX/{name}");

            // Debug.Log($"PlaySound: {clip?.name} (vol={volume}, pitch={pitch})");
            if (Instance == null || clip == null || IsMuted) return;

            AudioSource source = Instance.GetNextOneShotSource();
            source.clip = clip;
            source.volume = volume * Instance.SfxVolume * Instance.MasterVolume;
            source.pitch = pitch;
            source.Play();
        }

        private AudioSource GetNextOneShotSource()
        {
            // Find a free source first
            for (int i = 0; i < OneShotPoolSize; i++)
            {
                int index = (nextOneShotIndex + i) % OneShotPoolSize;
                if (!oneShotPool[index].isPlaying)
                {
                    nextOneShotIndex = (index + 1) % OneShotPoolSize;
                    return oneShotPool[index];
                }
            }

            // All busy steal the next one in rotation
            AudioSource stolen = oneShotPool[nextOneShotIndex];
            stolen.Stop();
            nextOneShotIndex = (nextOneShotIndex + 1) % OneShotPoolSize;
            return stolen;
        }

        #region Music

        /// <summary>
        /// Start playing ambient music, cycling through AmbientTracks.
        /// </summary>
        public static void StartMusic()
        {
            if (Instance == null || Instance.AmbientTracks.Length == 0) return;

            Instance.PlayNextTrack();
        }

        /// <summary>
        /// Stop music with a fade out.
        /// </summary>
        public static void StopMusic(float fadeTime = 1f)
        {
            if (Instance == null) return;
            if (Instance.crossfadeCoroutine != null) Instance.StopCoroutine(Instance.crossfadeCoroutine);
            Instance.crossfadeCoroutine = Instance.StartCoroutine(Instance.FadeOut(fadeTime));
        }

        private void PlayNextTrack()
        {
            if (AmbientTracks.Length == 0) return;

            currentTrackIndex = (currentTrackIndex + 1) % AmbientTracks.Length;
            AudioClip nextClip = AmbientTracks[currentTrackIndex];

            AudioSource fadeIn = musicSourceAActive ? musicSourceB : musicSourceA;
            AudioSource fadeOut = musicSourceAActive ? musicSourceA : musicSourceB;
            musicSourceAActive = !musicSourceAActive;

            fadeIn.clip = nextClip;
            fadeIn.loop = false;
            fadeIn.Play();

            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = StartCoroutine(Crossfade(fadeOut, fadeIn, MusicCrossfadeDuration));
        }

        /// <summary>
        /// Standard crossfade: stops the outgoing source when done.
        /// </summary>
        private IEnumerator Crossfade(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float timer = 0f;
            float startVolumeOut = fadeOut.volume;
            float targetVolume = MusicVolume * MasterVolume;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);

                // Make sure it's muted if muted
                if (IsMuted)
                {
                    fadeOut.volume = 0;
                    fadeIn.volume = 0;
                }

                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;
            crossfadeCoroutine = null;

            RefreshMusicVolume();
        }

        /// <summary>
        /// Crossfade that pauses the outgoing source instead of stopping it.
        /// Used when switching TO a special track so ambient can be resumed.
        /// </summary>
        private IEnumerator CrossfadeWithPause(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float timer = 0f;
            float startVolumeOut = fadeOut.volume;
            float targetVolume = MusicVolume * MasterVolume;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            fadeOut.Pause(); // Pause, not stop: preserves playback position
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;
            crossfadeCoroutine = null;
        }

        /// <summary>
        /// Crossfade that fully stops the outgoing source.
        /// Used when switching FROM a special track back to ambient.
        /// </summary>
        private IEnumerator CrossfadeAndStop(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float timer = 0f;
            float startVolumeOut = fadeOut.volume;
            float targetVolume = MusicVolume * MasterVolume;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;
            crossfadeCoroutine = null;
        }

        private IEnumerator FadeOut(float duration)
        {
            AudioSource active = musicSourceAActive ? musicSourceA : musicSourceB;
            float startVolume = active.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                active.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            active.Stop();
            active.volume = 0f;
            crossfadeCoroutine = null;
        }

        #endregion

        // ==================== GLOBAL CONTROLS ====================

        public static void SetMasterVolume(float volume)
        {
            Instance.MasterVolume = Mathf.Clamp01(volume);
            Instance.RefreshMusicVolume();
        }

        public static void SetMusicVolume(float volume)
        {
            Instance.MusicVolume = Mathf.Clamp01(volume);
            Instance.RefreshMusicVolume();
        }

        public static void SetSfxVolume(float volume)
        {
            Instance.SfxVolume = Mathf.Clamp01(volume);
        }

        private void RefreshMusicVolume()
        {
            float targetVolume = MusicVolume * MasterVolume;
            AudioSource active = musicSourceAActive ? musicSourceA : musicSourceB;
            if (active.isPlaying && crossfadeCoroutine == null)
            {
                active.volume = targetVolume;
            }
        }
    }
}