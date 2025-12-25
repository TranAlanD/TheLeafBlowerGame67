using UnityEngine;

/// <summary>
/// Spawns leaves periodically from a pile. Place this on an empty GameObject where you want a leaf pile.
/// The leaves will be tagged as "Blowable" so the leaf blower can affect them.
/// </summary>
public class LeafPile : MonoBehaviour
{
    [Header("Leaf Prefab")]
    [Tooltip("The leaf prefab to spawn (should have Rigidbody and Blowable tag)")]
    public GameObject leafPrefab;

    [Header("Spawn Settings")]
    [Tooltip("How many leaves to spawn at start")]
    public int initialLeafCount = 20;

    [Tooltip("Spawn leaves continuously")]
    public bool continuousSpawn = true;

    [Tooltip("Time between spawning new leaves (seconds)")]
    public float spawnInterval = 2f;

    [Tooltip("How many leaves to spawn each interval")]
    public int leavesPerSpawn = 1;

    [Tooltip("Maximum number of leaves that can exist at once (0 = unlimited)")]
    public int maxLeaves = 50;

    [Header("Spawn Area")]
    [Tooltip("Radius around this object where leaves can spawn")]
    public float spawnRadius = 1f;

    [Tooltip("Initial upward velocity when leaves spawn")]
    public float spawnForce = 2f;

    [Header("Leaf Physics")]
    [Tooltip("Mass of spawned leaves")]
    public float leafMass = 0.1f;

    [Tooltip("Drag of spawned leaves (higher = slower)")]
    public float leafDrag = 1f;

    [Tooltip("Angular drag (rotation damping)")]
    public float leafAngularDrag = 0.5f;

    private float spawnTimer = 0f;
    private int currentLeafCount = 0;

    void Start()
    {
        if (leafPrefab == null)
        {
            Debug.LogError("LeafPile: No leaf prefab assigned!");
            return;
        }

        // Spawn initial pile
        SpawnLeaves(initialLeafCount);
    }

    void Update()
    {
        if (!continuousSpawn || leafPrefab == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            // Check if we're under the max leaf limit
            if (maxLeaves == 0 || currentLeafCount < maxLeaves)
            {
                SpawnLeaves(leavesPerSpawn);
            }
        }
    }

    void SpawnLeaves(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Random position within spawn radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            // Spawn leaf
            GameObject leaf = Instantiate(leafPrefab, spawnPos, Random.rotation);

            // Configure rigidbody
            Rigidbody rb = leaf.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = leafMass;
                rb.linearDamping = leafDrag;
                rb.angularDamping = leafAngularDrag;

                // Add initial upward force with some randomness
                Vector3 force = new Vector3(
                    Random.Range(-1f, 1f),
                    spawnForce,
                    Random.Range(-1f, 1f)
                );
                rb.AddForce(force, ForceMode.Impulse);

                // Add random spin
                rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            }

            // Ensure it has the Blowable tag
            if (!leaf.CompareTag("Blowable"))
            {
                leaf.tag = "Blowable";
            }

            // Track leaf count
            LeafTracker tracker = leaf.AddComponent<LeafTracker>();
            tracker.pile = this;
            currentLeafCount++;
        }
    }

    // Called when a leaf is destroyed
    public void OnLeafDestroyed()
    {
        currentLeafCount--;
    }

    // Visualize spawn area in editor
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 1f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

/// <summary>
/// Helper component to track when leaves are destroyed
/// </summary>
public class LeafTracker : MonoBehaviour
{
    [HideInInspector]
    public LeafPile pile;

    void OnDestroy()
    {
        if (pile != null)
        {
            pile.OnLeafDestroyed();
        }
    }
}
