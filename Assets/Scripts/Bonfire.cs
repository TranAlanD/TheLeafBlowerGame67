using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The bonfire where player delivers the mushroom to win.
/// Press F near bonfire while carrying mushroom to deliver it.
/// </summary>
public class Bonfire : MonoBehaviour
{
    [Header("Delivery Settings")]
    [Tooltip("Key to press to deliver item")]
    public KeyCode deliverKey = KeyCode.F;

    [Tooltip("How close player needs to be to deliver")]
    public float deliveryRange = 3f;

    [Tooltip("Tag of the item that must be delivered (mushroom)")]
    public string requiredItemTag = "Pickupable";

    [Header("Visual Feedback")]
    [Tooltip("Particle system to play when item is delivered")]
    public ParticleSystem deliveryEffect;

    [Tooltip("Light component to enable when item delivered")]
    public Light bonfireLight;

    [Header("Win Condition")]
    [Tooltip("Event triggered when mushroom is delivered")]
    public UnityEvent onMushroomDelivered;

    private bool mushroomDelivered = false;
    private bool playerInRange = false;
    private PlayerCarry playerCarry = null;

    void Update()
    {
        if (mushroomDelivered) return;

        // Check if player is in range and has mushroom
        if (playerInRange && playerCarry != null && Input.GetKeyDown(deliverKey))
        {
            PickupableItem carriedItem = playerCarry.GetCarriedItem();

            if (carriedItem != null && carriedItem.CompareTag(requiredItemTag))
            {
                DeliverMushroom(carriedItem);
            }
        }
    }

    void DeliverMushroom(PickupableItem mushroom)
    {
        mushroomDelivered = true;

        // Notify the mushroom it was delivered
        mushroom.OnDelivered();

        // Place mushroom in bonfire (you can destroy it or keep it for visuals)
        mushroom.transform.SetParent(transform);
        mushroom.transform.localPosition = Vector3.zero;
        mushroom.transform.localRotation = Quaternion.identity;

        // Visual effects
        if (deliveryEffect != null)
        {
            deliveryEffect.Play();
        }

        if (bonfireLight != null)
        {
            bonfireLight.enabled = true;
            bonfireLight.intensity = 3f;
            bonfireLight.color = Color.green;
        }

        // Trigger win event
        onMushroomDelivered?.Invoke();

        Debug.Log("<color=green>MUSHROOM DELIVERED! Level Complete!</color>");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerCarry = other.GetComponent<PlayerCarry>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerCarry = null;
        }
    }

    // Draw delivery range in editor
    void OnDrawGizmos()
    {
        Gizmos.color = mushroomDelivered ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deliveryRange);
    }
}
