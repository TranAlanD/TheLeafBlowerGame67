using UnityEngine;

/// <summary>
/// Handles player picking up and carrying items (like the mushroom guy).
/// Press F to pick up nearby items. Drop item when hit by enemies.
/// Add this to the Player GameObject.
/// </summary>
public class PlayerCarry : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Key to press to pick up items")]
    public KeyCode pickupKey = KeyCode.F;

    [Tooltip("How close player needs to be to pick up")]
    public float pickupRange = 2f;

    [Tooltip("Tag of items that can be picked up")]
    public string pickupableTag = "Pickupable";

    [Header("Carry Settings")]
    [Tooltip("Where the carried item will be positioned (create empty child object)")]
    public Transform carryPoint;

    [Tooltip("Offset from carry point")]
    public Vector3 carryOffset = new Vector3(0, 0.5f, 0.5f);

    [Tooltip("Should carried item rotate with player?")]
    public bool rotateWithPlayer = true;

    [Header("Drop Settings")]
    [Tooltip("Tag of enemies that cause player to drop item when hit")]
    public string enemyTag = "Spider";

    [Tooltip("Force applied to item when dropped")]
    public float dropForce = 3f;

    // Current state
    private PickupableItem carriedItem = null;
    private PickupableItem nearbyItem = null;
    private Rigidbody playerRb;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();

        // Create carry point if not assigned
        if (carryPoint == null)
        {
            GameObject carryPointObj = new GameObject("CarryPoint");
            carryPointObj.transform.SetParent(transform);
            carryPointObj.transform.localPosition = carryOffset;
            carryPoint = carryPointObj.transform;
        }
    }

    void Update()
    {
        // Check for nearby pickupable items
        FindNearbyPickupable();

        // Handle pickup input
        if (Input.GetKeyDown(pickupKey))
        {
            if (carriedItem == null && nearbyItem != null)
            {
                PickupItem(nearbyItem);
            }
            else if (carriedItem != null)
            {
                DropItem(false); // Manual drop (gentle)
            }
        }

        // Update carried item position
        if (carriedItem != null)
        {
            UpdateCarriedItemPosition();
        }
    }

    void FindNearbyPickupable()
    {
        // Find all objects with pickupable tag in range
        GameObject[] pickupables = GameObject.FindGameObjectsWithTag(pickupableTag);
        nearbyItem = null;
        float closestDistance = pickupRange;

        foreach (GameObject obj in pickupables)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance <= closestDistance)
            {
                PickupableItem item = obj.GetComponent<PickupableItem>();
                if (item != null && item.canBePickedUp && !item.isBeingCarried)
                {
                    nearbyItem = item;
                    closestDistance = distance;
                }
            }
        }
    }

    void PickupItem(PickupableItem item)
    {
        carriedItem = item;
        carriedItem.OnPickedUp(transform);

        // Parent to carry point
        carriedItem.transform.SetParent(carryPoint);
        carriedItem.transform.localPosition = Vector3.zero;
        carriedItem.transform.localRotation = Quaternion.identity;

        Debug.LogError($"PICKED UP ITEM: {item.gameObject.name}. CarriedItem is now: {(carriedItem != null ? carriedItem.gameObject.name : "NULL")}");
    }

    void DropItem(bool wasHit)
    {
        if (carriedItem == null) return;

        // Unparent
        carriedItem.transform.SetParent(null);

        // Calculate drop position and velocity
        Vector3 dropPosition = carriedItem.transform.position;
        Vector3 dropVelocity = Vector3.zero;

        if (wasHit)
        {
            // Drop with force when hit
            dropVelocity = -transform.forward * dropForce + Vector3.up * (dropForce * 0.5f);
        }
        else
        {
            // Gentle drop when pressing F
            dropVelocity = transform.forward * 1f;
        }

        // Add player velocity if moving
        if (playerRb != null)
        {
            dropVelocity += playerRb.linearVelocity * 0.5f;
        }

        carriedItem.OnDropped(dropPosition, dropVelocity);
        carriedItem = null;
    }

    void UpdateCarriedItemPosition()
    {
        if (carriedItem == null) return;

        // Keep item at carry point
        carriedItem.transform.position = carryPoint.position;

        if (rotateWithPlayer)
        {
            carriedItem.transform.rotation = carryPoint.rotation;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Drop item if hit by enemy
        if (collision.gameObject.CompareTag(enemyTag) && carriedItem != null)
        {
            Debug.Log("<color=red>Hit by enemy! Dropping item.</color>");
            DropItem(true);
        }
    }

    // Public method for other scripts to check if player is carrying something
    public bool IsCarryingItem()
    {
        return carriedItem != null;
    }

    public PickupableItem GetCarriedItem()
    {
        Debug.LogError($"GetCarriedItem called. Returning: {(carriedItem != null ? carriedItem.gameObject.name : "NULL")}");
        return carriedItem;
    }

    // Draw pickup range in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        if (carryPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(carryPoint.position, 0.2f);
        }
    }
}
