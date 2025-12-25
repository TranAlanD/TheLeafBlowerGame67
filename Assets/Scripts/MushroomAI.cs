using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple mushroom enemy that chases and bumps into the player
/// Can be knocked back by player attacks
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MushroomAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform target;
    public Rigidbody rb;

    [Header("Chase Settings")]
    public float detectionRange = 15f;
    public float chaseSpeed = 3f;
    public float attackRange = 2.5f; // Distance to trigger attack animation

    [Header("Melee Attack")]
    public float meleeKnockbackVelocity = 12f;
    [Range(0f, 1f)]
    public float meleeKnockbackUpwardBias = 0.25f;
    public float attackCooldown = 2f;

    [Header("Attack Sound")]
    [Tooltip("Sound to play when attack hits (played in AE_ApplyStatus)")]
    public AudioClip attackSound;

    [Tooltip("Volume for attack sound")]
    [Range(0f, 1f)]
    public float attackSoundVolume = 1f;

    [Header("Knockback Resistance")]
    public float knockbackRecoveryTime = 0.5f; // Time before resuming chase after being knocked back

    private float lastAttackTime = -10f; // Start ready to attack
    private bool isKnockedBack = false;
    private float knockbackEndTime = 0f;
    private AudioSource audioSource;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        if (!rb) rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }

        // Setup agent
        if (agent)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
        }

        // Setup rigidbody for knockback
        if (rb)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Update()
    {
        if (!target || !agent) return;

        // Check if knocked back
        if (isKnockedBack)
        {
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
                agent.enabled = true;
            }
            else
            {
                // Stay disabled during knockback
                return;
            }
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // Only chase if within detection range
        if (distance <= detectionRange)
        {
            // Set destination (only if agent is active and on NavMesh)
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(target.position);
            }

            // Look at player
            Vector3 dirToPlayer = target.position - transform.position;
            dirToPlayer.y = 0;
            if (dirToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }

            // Animation control
            float speed = agent.enabled ? agent.velocity.magnitude : 0f;
            bool isMoving = speed > 0.1f;

            animator.SetBool("IsWalking", isMoving);
            animator.SetFloat("Speed", speed);

            // Trigger attack animation when close
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Debug.Log($"<color=yellow>Mushroom triggering attack at distance {distance:F2}m</color>");
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Stop moving if out of range
            if (agent.enabled && agent.isOnNavMesh && agent.hasPath)
            {
                agent.ResetPath();
            }
            animator.SetBool("IsWalking", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    // Called by animation event - applies knockback to player
    public void AE_ApplyStatus()
    {
        if (!target)
        {
            Debug.LogWarning("AE_ApplyStatus: No target!");
            return;
        }

        float d = Vector3.Distance(transform.position, target.position);
        Debug.Log($"<color=cyan>Mushroom AE_ApplyStatus called at distance {d:F2}m</color>");

        // Check if player is still in attack range when animation event fires
        if (d > attackRange)
        {
            Debug.Log($"<color=yellow>Mushroom attack missed - player out of range ({d:F2}m > {attackRange}m)</color>");
            return;
        }

        Debug.Log($"<color=red>MUSHROOM MELEE HIT at distance {d:F2}m</color>");

        // Play attack sound
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, attackSoundVolume);
        }

        // Apply knockback using player's knockback system
        PlayerMovement playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = target.GetComponentInParent<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            playerMovement.ApplyKnockback(target.position, transform.position, meleeKnockbackVelocity, meleeKnockbackUpwardBias);
            Debug.Log($"<color=green>Mushroom applied knockback to player!</color>");
        }
        else
        {
            Debug.LogWarning("Target is missing PlayerMovement component!");
        }
    }

    /// <summary>
    /// Call this to knock back the mushroom (from player attacks, etc.)
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (rb == null) return;

        // Disable agent temporarily
        if (agent) agent.enabled = false;

        // Apply knockback velocity
        Vector3 knockbackDir = direction.normalized;
        knockbackDir.y = 0.3f; // Add slight upward component
        knockbackDir.Normalize();

        rb.linearVelocity = knockbackDir * force;

        // Set knockback state
        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackRecoveryTime;

        Debug.Log($"<color=orange>Mushroom knocked back!</color>");
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
