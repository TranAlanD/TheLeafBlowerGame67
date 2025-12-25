using UnityEngine;

public class SpiderProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;
    public float lifetime = 5f;
    public float slowDuration = 3f;
    [Range(0f, 1f)]
    public float slowFactor = 0.5f;  // 0.5 = 50% speed

    private Vector3 direction;
    private float spawnTime;

    public void Initialize(Vector3 targetDirection)
    {
        direction = targetDirection.normalized;
        spawnTime = Time.time;

        // Orient the projectile in the direction it's traveling
        transform.forward = direction;
    }

    void Update()
    {
        // Move projectile forward
        transform.position += direction * speed * Time.deltaTime;

        // Destroy after lifetime expires
        if (Time.time >= spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile hit: {other.name} (tag: {other.tag})");

        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            var affectable = other.GetComponentInParent<IStatusAffectable>();
            Debug.Log($"Player hit! IStatusAffectable found: {affectable != null}");

            if (affectable != null)
            {
                // Apply the slow status effect
                affectable.ApplyStatus(new SlowStatus(slowFactor, slowDuration));
                Debug.Log($"<color=cyan>Applied SlowStatus to player ({slowFactor * 100}% speed for {slowDuration} seconds)</color>");
            }
            else
            {
                Debug.LogWarning("Player is missing PlayerStatus component (IStatusAffectable)!");
            }

            // Destroy projectile on impact
            Destroy(gameObject);
        }
        // Optionally destroy on hitting environment
        else if (!other.isTrigger && !other.CompareTag("Spider"))
        {
            Debug.Log($"Projectile destroyed on collision with {other.name}");
            Destroy(gameObject);
        }
    }
}
