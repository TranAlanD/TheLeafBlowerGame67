using UnityEngine;

/// <summary>
/// Blue mushroom trail markers that glow and can be blown by leaf blower.
/// Help guide player back up the cave from bottom to top.
/// Make these "Blowable" tagged objects to work with existing leaf blower.
/// </summary>
public class BlueMushroom : MonoBehaviour
{
    [Header("Visual Effects")]
    [Tooltip("Light component for glow")]
    public Light mushroomLight;

    [Tooltip("Glow color")]
    public Color glowColor = new Color(0.3f, 0.6f, 1f); // Blue

    [Tooltip("Glow intensity")]
    public float glowIntensity = 2f;

    [Tooltip("Pulse speed (0 = no pulse)")]
    public float pulseSpeed = 1f;

    [Tooltip("Pulse amount (how much brighter/dimmer)")]
    public float pulseAmount = 0.5f;

    [Header("Blowable Settings")]
    [Tooltip("Is this mushroom affected by leaf blower?")]
    public bool canBeBlown = true;

    [Tooltip("Mass of mushroom (affects how easily it's blown)")]
    public float mass = 1f;

    private float baseIntensity;
    private Material material;

    void Start()
    {
        // Set up light if present
        if (mushroomLight == null)
        {
            mushroomLight = GetComponentInChildren<Light>();
        }

        if (mushroomLight != null)
        {
            mushroomLight.color = glowColor;
            mushroomLight.intensity = glowIntensity;
            baseIntensity = glowIntensity;
        }

        // Set up material emission if present
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            material = renderer.material;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", glowColor * glowIntensity);
            }
        }

        // Ensure has Rigidbody if can be blown
        if (canBeBlown)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.mass = mass;
            rb.linearDamping = 2f; // So it doesn't fly too far
            rb.angularDamping = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevent falling through ground
        }

        // Ensure has collider
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 1f;
        }

        // Tag as Blowable if not already
        if (canBeBlown && !CompareTag("Blowable"))
        {
            Debug.LogWarning($"{gameObject.name} is canBeBlown but not tagged 'Blowable' - consider adding tag");
        }
    }

    void Update()
    {
        // Pulse glow
        if (pulseSpeed > 0 && mushroomLight != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            mushroomLight.intensity = baseIntensity + pulse;
        }

        // Update emission if material exists
        if (material != null && material.HasProperty("_EmissionColor"))
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            material.SetColor("_EmissionColor", glowColor * (glowIntensity + pulse));
        }
    }
}
