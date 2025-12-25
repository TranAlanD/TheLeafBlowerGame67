using UnityEngine;
using System.Collections;

public class PoliceCar : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Where the car starts (off-screen)")]
    public Transform startPosition;

    [Tooltip("Where the car stops (visible in scene)")]
    public Transform arrivalPosition;

    [Tooltip("How fast the car drives in")]
    public float driveSpeed = 20f;

    [Header("Audio")]
    [Tooltip("Siren sound to play while driving")]
    public AudioClip sirenSound;

    [Tooltip("Engine/brake sound when stopping")]
    public AudioClip brakeSound;

    [Range(0f, 2f)]
    [Tooltip("Volume for siren (can go above 1 to boost)")]
    public float sirenVolume = 1.5f;

    [Range(0f, 2f)]
    [Tooltip("Volume for brake sound (can go above 1 to boost)")]
    public float brakeVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("3D spatial blend (0=2D everywhere, 1=3D positional)")]
    public float spatialBlend = 0.5f;

    [Tooltip("Max distance to hear the sound (for 3D audio)")]
    public float maxDistance = 100f;

    [Tooltip("Audio priority (0=highest, 256=lowest)")]
    [Range(0, 256)]
    public int priority = 0;

    private AudioSource audioSource;

    void Start()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.maxDistance = maxDistance;
        audioSource.priority = priority;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // Start at the start position if specified
        if (startPosition != null)
        {
            transform.position = startPosition.position;
            transform.rotation = startPosition.rotation;
        }
    }

    public IEnumerator DriveIn()
    {
        if (arrivalPosition == null)
        {
            Debug.LogError("PoliceCar: arrivalPosition not set!");
            yield break;
        }

        Debug.Log("<color=blue>Police car driving in!</color>");

        // Play siren
        if (sirenSound != null && audioSource != null)
        {
            audioSource.clip = sirenSound;
            audioSource.volume = sirenVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Drive to arrival position
        while (Vector3.Distance(transform.position, arrivalPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, arrivalPosition.position, driveSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure final position is exact
        transform.position = arrivalPosition.position;
        transform.rotation = arrivalPosition.rotation;

        // Stop siren and play brake sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (brakeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(brakeSound, brakeVolume);
        }

        Debug.Log("<color=blue>Police car arrived!</color>");
    }
}
