using UnityEngine;
using System.Collections;

/// <summary>
/// Baby mushroom dialogue when player approaches and picks up.
/// Shows "are you here to save me?" then "yay" when picked up.
/// </summary>
public class BabyMushroomDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [Tooltip("Lines shown when player first gets close")]
    public string[] approachDialogue = {
        "are you here to save me?"
    };

    [Tooltip("Lines shown when player picks up baby")]
    public string[] pickupDialogue = {
        "yay"
    };

    [Tooltip("How long each line stays on screen")]
    public float dialogueDuration = 2f;

    [Tooltip("Range to detect player")]
    public float detectionRange = 5f;

    [Header("Voice Sound")]
    [Tooltip("Talking sound when baby speaks")]
    public AudioClip babyTalkingSound;

    [Tooltip("Volume for talking sound")]
    [Range(0f, 1f)]
    public float talkingSoundVolume = 0.5f;

    private bool hasShownApproachDialogue = false;
    private bool hasShownPickupDialogue = false;
    private AudioSource audioSource;
    private PickupableItem pickupableItem;
    private Transform playerTransform;
    private VoiceAudioManager voiceManager;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get voice manager (must be manually added)
        voiceManager = GetComponent<VoiceAudioManager>();
        if (voiceManager == null)
        {
            Debug.LogWarning("VoiceAudioManager not found on baby mushroom! Add it manually for voice effects. Using basic audio instead.");
        }

        pickupableItem = GetComponent<PickupableItem>();

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null || pickupableItem == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Show approach dialogue when player gets close
        if (!hasShownApproachDialogue && !pickupableItem.isBeingCarried && distance <= detectionRange)
        {
            StartCoroutine(ShowApproachDialogue());
            hasShownApproachDialogue = true;
        }

        // Show pickup dialogue when picked up
        if (!hasShownPickupDialogue && pickupableItem.isBeingCarried)
        {
            StartCoroutine(ShowPickupDialogue());
            hasShownPickupDialogue = true;
        }
    }

    IEnumerator ShowApproachDialogue()
    {
        foreach (string line in approachDialogue)
        {
            Debug.Log($"<color=green>Baby: {line}</color>");
            DialogueUI.ShowText($"[Baby]: {line}");

            if (babyTalkingSound != null)
            {
                if (voiceManager != null)
                {
                    voiceManager.PlayVoice(babyTalkingSound, talkingSoundVolume);
                }
                else if (audioSource != null)
                {
                    // Fallback without voice manager
                    audioSource.PlayOneShot(babyTalkingSound, talkingSoundVolume);
                }
            }

            yield return new WaitForSeconds(dialogueDuration);

            // Stop voice when line ends
            if (voiceManager != null)
            {
                voiceManager.StopVoice();
            }
        }
        DialogueUI.Hide();
    }

    IEnumerator ShowPickupDialogue()
    {
        foreach (string line in pickupDialogue)
        {
            Debug.Log($"<color=green>Baby: {line}</color>");
            DialogueUI.ShowText($"[Baby]: {line}");

            if (babyTalkingSound != null)
            {
                if (voiceManager != null)
                {
                    voiceManager.PlayVoice(babyTalkingSound, talkingSoundVolume);
                }
                else if (audioSource != null)
                {
                    // Fallback without voice manager
                    audioSource.PlayOneShot(babyTalkingSound, talkingSoundVolume);
                }
            }

            yield return new WaitForSeconds(dialogueDuration);

            // Stop voice when line ends
            if (voiceManager != null)
            {
                voiceManager.StopVoice();
            }
        }
        DialogueUI.Hide();
    }
}
