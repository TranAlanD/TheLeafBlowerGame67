using UnityEngine;

/// <summary>
/// Makes an object pickupable by the player (like the mushroom guy).
/// When picked up, disables physics and gameplay but keeps visuals.
/// </summary>
public class PickupableItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Can this item be picked up right now?")]
    public bool canBePickedUp = true;

    [Tooltip("Prompt text when player is nearby (for UI)")]
    public string pickupPrompt = "Press F to pick up";

    [Header("Components to Disable When Held")]
    [Tooltip("Disable these components when picked up (AI, NavMeshAgent, etc.)")]
    public MonoBehaviour[] componentsToDisable;

    [HideInInspector]
    public bool isBeingCarried = false;

    private Rigidbody rb;
    private Collider[] colliders;
    private Vector3 originalScale;
    private bool wasKinematic;
    private bool hadGravity;
    private InteractionPrompt interactionPrompt;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        originalScale = transform.localScale;

        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            hadGravity = rb.useGravity;
        }

        // Add interaction prompt if not present
        interactionPrompt = GetComponent<InteractionPrompt>();
        if (interactionPrompt == null && canBePickedUp)
        {
            interactionPrompt = gameObject.AddComponent<InteractionPrompt>();
            interactionPrompt.promptText = "F";
            interactionPrompt.showRange = 3f;
        }
    }

    /// <summary>
    /// Called when player picks up this item
    /// </summary>
    public void OnPickedUp(Transform carrier)
    {
        isBeingCarried = true;

        // Disable physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable colliders (so it doesn't collide with player or enemies)
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Disable gameplay components (AI, NavMeshAgent, etc.)
        foreach (MonoBehaviour component in componentsToDisable)
        {
            if (component != null)
                component.enabled = false;
        }

        // Hide interaction prompt while being carried
        if (interactionPrompt != null)
        {
            interactionPrompt.SetVisible(false);
        }

        Debug.Log($"<color=green>Picked up {gameObject.name}</color>");
    }

    /// <summary>
    /// Called when player drops this item
    /// </summary>
    public void OnDropped(Vector3 dropPosition, Vector3 dropVelocity)
    {
        isBeingCarried = false;

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = hadGravity;
            rb.linearVelocity = dropVelocity;
        }

        // Re-enable colliders
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        // Re-enable gameplay components
        foreach (MonoBehaviour component in componentsToDisable)
        {
            if (component != null)
                component.enabled = true;
        }

        // Show interaction prompt again
        if (interactionPrompt != null && canBePickedUp)
        {
            interactionPrompt.SetVisible(true);
        }

        // Set position
        transform.position = dropPosition;

        Debug.Log($"<color=yellow>Dropped {gameObject.name}</color>");
    }

    /// <summary>
    /// Called when item is delivered to goal (bonfire)
    /// </summary>
    public void OnDelivered()
    {
        Debug.Log($"<color=cyan>{gameObject.name} delivered to bonfire!</color>");
        // You can add effects here (particles, sound, etc.)
    }
}
