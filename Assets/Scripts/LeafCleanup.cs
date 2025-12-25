using UnityEngine;

/// <summary>
/// Auto-destroys leaves after a certain time or if they fall off the map.
/// Add this to your leaf prefab to prevent infinite leaves building up.
/// </summary>
public class LeafCleanup : MonoBehaviour
{
    [Tooltip("Destroy leaf after this many seconds (0 = never)")]
    public float lifetime = 30f;

    [Tooltip("Destroy if leaf falls below this Y position")]
    public float killHeight = -50f;

    [Tooltip("Destroy if leaf is this far from origin (0 = never)")]
    public float maxDistance = 0f;

    private float spawnTime;
    private Vector3 originPoint;

    void Start()
    {
        spawnTime = Time.time;
        originPoint = Vector3.zero; // or set to a specific point
    }

    void Update()
    {
        // Lifetime cleanup
        if (lifetime > 0 && Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Fall off map cleanup
        if (transform.position.y < killHeight)
        {
            Destroy(gameObject);
            return;
        }

        // Distance cleanup (optional)
        if (maxDistance > 0 && Vector3.Distance(transform.position, originPoint) > maxDistance)
        {
            Destroy(gameObject);
            return;
        }
    }
}
