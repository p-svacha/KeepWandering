using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Analyzes AudioClips once (cached) to compensate for inconsistent source recordings: trims perceived
/// playback delay caused by leading silence, and computes a per-clip gain that normalizes loudness toward
/// a common target. Purely a runtime playback adjustment - never modifies the underlying asset.
/// </summary>
public static class AudioClipAnalyzer
{
    private struct ClipInfo
    {
        public float LeadingSilence;   // seconds to skip at the start of playback
        public float VolumeMultiplier; // gain to apply so clips land at a consistent perceived loudness
    }

    private const float SILENCE_THRESHOLD = 0.02f;   // sample amplitude below this counts as silence
    private const float MAX_TRIM_FRACTION = 0.5f;    // never trim more than half the clip, as a safety net

    // Loudness normalization: RMS-based, since it tracks perceived loudness far better than peak for
    // short/percussive sounds - a sharp clack can have a high peak but low total energy, and peak
    // normalization alone leaves it sounding quieter than a sustained rustle with a lower peak.
    private const float TARGET_RMS = 0.25f;
    private const float MIN_GAIN = 0.3f;
    private const float MAX_GAIN = 4f;

    private static Dictionary<AudioClip, ClipInfo> Cache = new Dictionary<AudioClip, ClipInfo>();

    public static void Apply(AudioSource source, AudioClip clip, float baseVolume, out float adjustedVolume)
    {
        ClipInfo info = GetInfo(clip);
        source.time = info.LeadingSilence;
        adjustedVolume = baseVolume * info.VolumeMultiplier;
    }

    private static ClipInfo GetInfo(AudioClip clip)
    {
        if (Cache.TryGetValue(clip, out ClipInfo cached)) return cached;

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // Non-silent region: first and last sample points that clear the threshold
        int firstLoudSample = 0;
        int lastLoudSample = samples.Length - 1;
        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > SILENCE_THRESHOLD) { firstLoudSample = i; break; }
        }
        for (int i = samples.Length - 1; i >= 0; i--)
        {
            if (Mathf.Abs(samples[i]) > SILENCE_THRESHOLD) { lastLoudSample = i; break; }
        }

        float leadingSilence = (firstLoudSample / clip.channels) / (float)clip.frequency;
        leadingSilence = Mathf.Min(leadingSilence, clip.length * MAX_TRIM_FRACTION);

        // RMS and peak, computed only over the non-silent region so padding doesn't dilute the reading
        float sumSquares = 0f;
        float peak = 0f;
        int count = 0;
        for (int i = firstLoudSample; i <= lastLoudSample; i++)
        {
            float abs = Mathf.Abs(samples[i]);
            sumSquares += samples[i] * samples[i];
            peak = Mathf.Max(peak, abs);
            count++;
        }
        float rms = count > 0 ? Mathf.Sqrt(sumSquares / count) : 0f;

        float gain = rms > 0.0001f ? TARGET_RMS / rms : 1f;
        gain = Mathf.Clamp(gain, MIN_GAIN, MAX_GAIN);

        // Safety cap: never let the gain push this clip's own peak into clipping/distortion
        if (peak > 0.0001f) gain = Mathf.Min(gain, 0.98f / peak);

        ClipInfo info = new ClipInfo { LeadingSilence = leadingSilence, VolumeMultiplier = gain };
        Cache[clip] = info;
        return info;
    }
}