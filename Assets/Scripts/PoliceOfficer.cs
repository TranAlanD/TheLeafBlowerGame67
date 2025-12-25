using UnityEngine;
using System.Collections;

public class PoliceOfficer : MonoBehaviour
{
    [Header("Voice Settings")]
    public AudioClip voiceClip;
    [Range(0f, 1f)]
    public float voiceVolume = 0.5f;

    [Header("Movement")]
    public Transform startPosition; // Hidden position
    public Transform scenePosition; // Where cop stands during scene
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    private VoiceAudioManager voiceManager;
    private AudioSource audioSource;

    void Start()
    {
        // Get or create audio components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        voiceManager = GetComponent<VoiceAudioManager>();

        // Start at hidden position if specified
        if (startPosition != null)
        {
            transform.position = startPosition.position;
            transform.rotation = startPosition.rotation;
        }
    }

    public IEnumerator EnterScene()
    {
        if (scenePosition == null)
        {
            Debug.LogError("PoliceOfficer: scenePosition not set!");
            yield break;
        }

        // Move to scene position
        while (Vector3.Distance(transform.position, scenePosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, scenePosition.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = scenePosition.position;
    }

    public IEnumerator ExitScene()
    {
        if (startPosition == null)
        {
            Debug.LogError("PoliceOfficer: startPosition not set!");
            yield break;
        }

        // Move back to start position
        while (Vector3.Distance(transform.position, startPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPosition.position;
    }

    public IEnumerator LookAt(Transform target, float duration = 0.5f)
    {
        if (target == null) yield break;

        Quaternion startRotation = transform.rotation;
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Keep rotation horizontal only

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    public void PlayVoice()
    {
        if (voiceClip == null) return;

        if (voiceManager != null)
        {
            voiceManager.PlayVoice(voiceClip, voiceVolume);
        }
        else if (audioSource != null)
        {
            audioSource.PlayOneShot(voiceClip, voiceVolume);
        }
    }

    public void StopVoice()
    {
        if (voiceManager != null)
        {
            voiceManager.StopVoice();
        }
        else if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
