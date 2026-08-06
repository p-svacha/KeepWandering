using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
    [Header("Settings")]
    public const int OneShotPoolSize = 12;
    public float MasterVolume { get; private set; } // 0-1
    public float MusicVolume { get; private set; } // 0-1
    public float SfxVolume { get; private set; } // 0-1

    [Header("Music")]
    public AudioClip[] AmbientTracks;
    public float MusicCrossfadeDuration = 2f;
    public const float MUSIC_VOLUME_MODIFIER = 0.7f;

    // Music
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private bool musicSourceAActive = true;
    private int currentTrackIndex = -1;
    private Coroutine crossfadeCoroutine;

    // One-shot pool
    private AudioSource[] oneShotPool;
    private int nextOneShotIndex;

    // Continuous sounds
    private Dictionary<string, ContinuousSound> ContinuousSounds = new Dictionary<string, ContinuousSound>();

    public static bool IsMuted => Instance.MasterVolume <= 0f;

    void Awake()
    {
        InitMusicSources();
        InitOneShotPool();

        MasterVolume = 1f;
        MusicVolume = MUSIC_VOLUME_MODIFIER;
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

        UpdateContinuousSounds();
    }

    private void UpdateContinuousSounds()
    {
        foreach (ContinuousSound cs in ContinuousSounds.Values)
        {
            if (cs.Source == null) continue;

            cs.FadeEnvelope = Mathf.MoveTowards(cs.FadeEnvelope, cs.TargetEnvelope, cs.FadeSpeed * Time.deltaTime);
            cs.Source.volume = cs.FadeEnvelope * cs.Intensity * cs.BaseVolume * SfxVolume * MasterVolume;

            // Fully faded out and not fading back in - stop the source so it's not silently ticking along.
            if (cs.TargetEnvelope <= 0f && cs.FadeEnvelope <= 0.0001f && cs.Source.isPlaying)
            {
                cs.Source.Stop();
            }
        }
    }

    // ==================== ONE-SHOT SOUNDS ====================

    public static void PlayStandardButtonClick()
    {
        PlaySound("Click_04", volume: 0.5f, pitch: 1f, pitchVariance: 0.1f);
    }

    /// <summary>
    /// Play a sound effect once. Supports overlapping.
    /// </summary>
    public static void PlaySound(string name, float volume = 1f, float pitch = 1f, float pitchVariance = 0f)
    {
        // Load AudioClip
        AudioClip clip = ResourceManager.LoadAudioClip($"Audio/SFX/{name}");

        // Debug.Log($"PlaySound: {clip?.name} (vol={volume}, pitch={pitch})");
        if (Instance == null || clip == null || IsMuted) return;

        AudioSource source = Instance.GetNextOneShotSource();
        source.clip = clip;
        source.volume = volume * Instance.SfxVolume * Instance.MasterVolume;
        source.pitch = pitch + Random.Range(-pitchVariance, pitchVariance);
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
        float targetVolume = GetTargetMusicVolume();

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

    private float GetTargetMusicVolume()
    {
        return MusicVolume * MasterVolume * MUSIC_VOLUME_MODIFIER;
    }

    /// <summary>
    /// Crossfade that pauses the outgoing source instead of stopping it.
    /// Used when switching TO a special track so ambient can be resumed.
    /// </summary>
    private IEnumerator CrossfadeWithPause(AudioSource fadeOut, AudioSource fadeIn, float duration)
    {
        float timer = 0f;
        float startVolumeOut = fadeOut.volume;
        float targetVolume = GetTargetMusicVolume();

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
        float targetVolume = GetTargetMusicVolume();

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
        float targetVolume = GetTargetMusicVolume();
        AudioSource active = musicSourceAActive ? musicSourceA : musicSourceB;
        if (active.isPlaying && crossfadeCoroutine == null)
        {
            active.volume = targetVolume;
        }
    }

    #region Continuous SFX

    private class ContinuousSound
    {
        public AudioSource Source;
        public float FadeEnvelope;    // 0-1, eased toward TargetEnvelope over time
        public float TargetEnvelope;  // 0 (fading out / stopped) or 1 (fading in / playing)
        public float FadeSpeed;       // envelope units per second, derived from the requested fade duration
        public float Intensity = 1f;  // 0-1, external multiplier (e.g. tied to drag speed); untouched by fades
        public float BaseVolume = 1f;
    }

    /// <summary>
    /// Starts (or ensures playing) a looping continuous sound, fading its volume in over fadeInDuration.
    /// Safe to call repeatedly while already playing/fading - it will not restart the clip or reset progress,
    /// it just (re)targets the fade envelope toward fully audible.
    /// </summary>
    public static void StartContinuousSound(string clipName, float fadeInDuration, float baseVolume = 1f)
    {
        if (Instance == null || IsMuted) return;

        ContinuousSound cs = Instance.GetOrCreateContinuousSound(clipName);
        cs.Intensity = 1f; // default to fully audible; callers that want to drive this (e.g. drag speed) override it right after
        cs.BaseVolume = baseVolume;
        cs.TargetEnvelope = 1f;
        cs.FadeSpeed = 1f / Mathf.Max(fadeInDuration, 0.0001f);

        if (!cs.Source.isPlaying)
        {
            cs.Source.clip = ResourceManager.LoadAudioClip($"Audio/SFX/{clipName}");
            cs.Source.loop = true;
            cs.Source.volume = 0f;
            cs.FadeEnvelope = 0f;
            cs.Source.Play();
        }
    }

    /// <summary>
    /// Fades a continuous sound's volume out over fadeOutDuration, stopping playback once fully faded.
    /// Safe to call even if the sound isn't currently playing.
    /// </summary>
    public static void StopContinuousSound(string clipName, float fadeOutDuration)
    {
        if (Instance == null) return;
        if (!Instance.ContinuousSounds.TryGetValue(clipName, out ContinuousSound cs)) return;

        cs.TargetEnvelope = 0f;
        cs.FadeSpeed = 1f / Mathf.Max(fadeOutDuration, 0.0001f);
    }

    /// <summary>
    /// Sets the intensity multiplier (0-1) of an already-started continuous sound - e.g. to tie its volume
    /// to drag speed. Independent of the fade envelope: a sound at intensity 0 is still "playing" internally,
    /// just silent, and becomes audible again the instant intensity rises without needing to be restarted.
    /// </summary>
    public static void SetContinuousSoundIntensity(string clipName, float intensity01)
    {
        if (Instance == null) return;
        if (!Instance.ContinuousSounds.TryGetValue(clipName, out ContinuousSound cs)) return;

        cs.Intensity = Mathf.Clamp01(intensity01);
    }

    private ContinuousSound GetOrCreateContinuousSound(string clipName)
    {
        if (ContinuousSounds.TryGetValue(clipName, out ContinuousSound existing)) return existing;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;

        ContinuousSound cs = new ContinuousSound { Source = source };
        ContinuousSounds.Add(clipName, cs);
        return cs;
    }

    #endregion

}