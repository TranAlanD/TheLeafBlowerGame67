using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages level completion. Enables teleporter or disables blocking wall when objective is complete.
/// Connect this to the Bonfire's onMushroomDelivered event.
/// </summary>
public class LevelGoal : MonoBehaviour
{
    [Header("Level Completion Actions")]
    [Tooltip("GameObject to enable when level complete (teleporter/portal)")]
    public GameObject objectToEnable;

    [Tooltip("GameObject to disable when level complete (blocking wall)")]
    public GameObject objectToDisable;

    [Header("Scene Transition")]
    [Tooltip("Automatically load next scene after delay?")]
    public bool autoLoadNextScene = false;

    [Tooltip("Name of next scene to load")]
    public string nextSceneName = "";

    [Tooltip("Delay before loading next scene (seconds)")]
    public float sceneLoadDelay = 3f;

    [Header("Visual Effects")]
    [Tooltip("Particle effect to spawn at completion")]
    public GameObject completionEffect;

    [Tooltip("Audio clip to play on completion")]
    public AudioClip completionSound;

    private bool levelComplete = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Call this method when the objective is complete (connect to Bonfire event)
    /// </summary>
    public void OnObjectiveComplete()
    {
        if (levelComplete) return;

        levelComplete = true;

        Debug.Log("<color=cyan>LEVEL COMPLETE!</color>");

        // Enable teleporter/portal
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            Debug.Log($"Enabled: {objectToEnable.name}");
        }

        // Disable blocking wall
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
            Debug.Log($"Disabled: {objectToDisable.name}");
        }

        // Spawn completion effect
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }

        // Play completion sound
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }

        // Load next scene if enabled
        if (autoLoadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            Invoke(nameof(LoadNextScene), sceneLoadDelay);
        }
    }

    void LoadNextScene()
    {
        Debug.Log($"Loading next scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Public method to check if level is complete
    /// </summary>
    public bool IsLevelComplete()
    {
        return levelComplete;
    }
}
