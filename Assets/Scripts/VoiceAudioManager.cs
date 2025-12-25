using UnityEngine;

/// <summary>
/// Manages voice audio playback with randomized pitch, start position, and effects.
/// Makes looped talking sounds less obvious and more natural.
/// </summary>
public class VoiceAudioManager : MonoBehaviour
{
    [Header("Pitch Randomization")]
    [Tooltip("Minimum pitch shift")]
    [Range(0.5f, 2f)]
    public float minPitch = 0.9f;

    [Tooltip("Maximum pitch shift")]
    [Range(0.5f, 2f)]
    public float maxPitch = 1.1f;

    [Header("Audio Effects")]
    [Tooltip("Add robotic/digital effect using high pass filter")]
    public bool useRoboticEffect = true;

    [Tooltip("High pass filter cutoff frequency (higher = more robotic)")]
    [Range(100f, 5000f)]
    public float highPassCutoff = 800f;

    [Tooltip("Add chorus effect for more digital sound")]
    public bool useChorusEffect = false;

    private AudioSource audioSource;
    private AudioHighPassFilter highPassFilter;
    private AudioChorusFilter chorusFilter;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Add high pass filter for robotic effect
        if (useRoboticEffect)
        {
            highPassFilter = GetComponent<AudioHighPassFilter>();
            if (highPassFilter == null)
            {
                highPassFilter = gameObject.AddComponent<AudioHighPassFilter>();
            }
            highPassFilter.cutoffFrequency = highPassCutoff;
        }

        // Add chorus for digital effect
        if (useChorusEffect)
        {
            chorusFilter = GetComponent<AudioChorusFilter>();
            if (chorusFilter == null)
            {
                chorusFilter = gameObject.AddComponent<AudioChorusFilter>();
            }
            chorusFilter.depth = 0.3f;
            chorusFilter.rate = 0.8f;
        }
    }

    /// <summary>
    /// Play a voice clip with randomized pitch and start position.
    /// Returns the AudioSource so caller can stop it later.
    /// </summary>
    public AudioSource PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (clip == null || audioSource == null) return null;

        // Stop any currently playing audio
        StopVoice();

        // Randomize pitch
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        // Randomize start position (within first 50% of clip to avoid cutting off end)
        float maxStartTime = clip.length * 0.5f;
        float startTime = Random.Range(0f, maxStartTime);
        audioSource.time = startTime;

        // Set clip and play
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true; // Loop so it keeps playing during dialogue
        audioSource.Play();

        return audioSource;
    }

    /// <summary>
    /// Stop the current voice playback and reset pitch
    /// </summary>
    public void StopVoice()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.pitch = 1f; // Reset pitch
        }
    }

    /// <summary>
    /// Check if voice is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}
