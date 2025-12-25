using UnityEngine;

// Example script showing how any enemy (mushroom, etc.) can apply knockback to the player
// Call ApplyKnockbackToPlayer() from animation events or collision
// NOTE: These are velocity values, not forces. Player's friction will naturally slow them down.
public class EnemyKnockbackExample : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Total knockback velocity magnitude")]
    public float knockbackVelocity = 15f;
    [Range(0f, 1f)]
    [Tooltip("Upward bias - 0 = horizontal, 1 = very steep upward")]
    public float upwardBias = 0.3f;

    // Call this from an animation event at the impact frame
    public void ApplyKnockbackToPlayer()
    {
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("Player not found!");
            return;
        }

        PlayerMovement player = playerObj.GetComponent<PlayerMovement>();
        if (player == null)
        {
            Debug.LogWarning("Player missing PlayerMovement component!");
            return;
        }

        // Apply knockback - impact at player, knock away from this enemy
        player.ApplyKnockback(playerObj.transform.position, transform.position, knockbackVelocity, upwardBias);
    }

    // Alternative: Apply knockback on collision (for charging enemies, etc.)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // Use collision point as impact location
                Vector3 impactPoint = collision.contacts[0].point;
                player.ApplyKnockback(impactPoint, transform.position, knockbackVelocity, upwardBias);
            }
        }
    }
}
