using UnityEngine;
using UnityEngine.AI;

public class SpiderAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    public Transform target;

    [Header("Range Settings")]
    public float sightRange = 15f;
    public float rangedAttackRange = 8f;      // Stop at this distance to shoot
    public float meleeAttackRange = 2.2f;     // Close range melee attacks
    public float preferredRangedMin = 6f;     // Preferred minimum distance for ranged
    public float preferredRangedMax = 10f;    // Preferred maximum distance for ranged
    public float preferredMeleeMin = 1.8f;
    public float preferredMeleeMax = 3.5f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.25f;
    [Range(0, 1)] public float attack2Chance = 0.35f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float meleeKnockbackVelocity = 15f;
    [Range(0f, 1f)]
    public float meleeKnockbackUpwardBias = 0.3f;

    [Header("Look At Settings")]
    public bool alwaysLookAtPlayer = true;
    public float lookAtSpeed = 5f;

    [Header("Movement Settings")]
    [Tooltip("Stationary spiders don't move, only rotate and shoot (for turrets/ledge spiders)")]
    public bool isStationary = false;

    float lastAttackTime;
    bool hasLineOfSight;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        // Disable NavMeshAgent for stationary spiders
        if (isStationary && agent != null)
        {
            agent.enabled = false;
        }
    }

    void Update()
    {
        if (!target) return;

        float d = Vector3.Distance(transform.position, target.position);

        // Make spider look at player
        if (alwaysLookAtPlayer && d <= sightRange)
        {
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0; // Keep spider upright

            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
            }
        }

        // Check line of sight
        hasLineOfSight = CheckLineOfSight();

        // Determine if we should be in ranged or melee mode
        bool inRangedMode = d > meleeAttackRange && d <= rangedAttackRange && hasLineOfSight;
        bool inMeleeMode = d <= meleeAttackRange;

        // Calculate backpedal state
        bool backInRanged = inRangedMode && d < preferredRangedMin * 0.7f;
        bool backInMelee = inMeleeMode && d < 0.5f;
        bool back = backInRanged || backInMelee;

        // Only do movement logic if not stationary
        if (!isStationary && agent != null && agent.enabled)
        {
            // Set stopping distance based on mode
            if (inRangedMode)
            {
                // Stop at preferred ranged distance (middle of preferred range)
                agent.stoppingDistance = (preferredRangedMin + preferredRangedMax) * 0.5f;
            }
            else
            {
                agent.stoppingDistance = Mathf.Clamp(preferredMeleeMin * 0.8f, 0.1f, meleeAttackRange - 0.1f);
            }

            // Only move towards player if within sight range
            if (d <= sightRange)
            {
                agent.SetDestination(target.position);
            }

            animator.SetBool("Backpedal", back);
        }

        // Attack logic - can attack when in range and not backing up
        // Stationary spiders can always attack if in range (no backpedal)
        bool canAttack = isStationary ? true : !back;
        if (Time.time >= lastAttackTime + attackCooldown && canAttack)
        {
            // Ranged attack: shoot projectile when in ranged zone
            if (inRangedMode)
            {
                Debug.Log($"Spider triggering RANGED attack at distance {d:F2}m");
                animator.SetInteger("AttackIndex", Random.value < attack2Chance ? 1 : 0);
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
                // Projectile will be spawned by AE_ApplyStatus animation event
            }
            // Melee attack: no projectile when close
            else if (inMeleeMode)
            {
                Debug.Log($"Spider triggering MELEE attack at distance {d:F2}m");
                animator.SetInteger("AttackIndex", Random.value < attack2Chance ? 1 : 0);
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
                // Knockback will be applied by AE_ApplyStatus animation event
            }
        }
    }

    bool CheckLineOfSight()
    {
        if (!target) return false;

        Vector3 dirToTarget = target.position - transform.position;
        float distToTarget = dirToTarget.magnitude;

        // Only check line of sight within sight range
        if (distToTarget > sightRange) return false;

        // Raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToTarget.normalized, out hit, distToTarget))
        {
            // We have line of sight if we hit the player
            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        return false;
    }

    // Hook these to animation events (optional):
    public void AE_LockMovement() { agent.isStopped = true; }
    public void AE_UnlockMovement() { agent.isStopped = false; }

    void ApplyMeleeKnockback()
    {
        if (!target) return;

        float d = Vector3.Distance(transform.position, target.position);

        // Check if player is still in melee range when attack lands
        if (d > meleeAttackRange)
        {
            Debug.Log($"<color=yellow>Melee attack missed - player out of range ({d:F2}m > {meleeAttackRange}m)</color>");
            return;
        }

        Debug.Log($"<color=red>MELEE ATTACK HIT at distance {d:F2}m</color>");

        // Apply slow status effect
        var affectable = target.GetComponentInParent<IStatusAffectable>();
        if (affectable != null)
        {
            affectable.ApplyStatus(new SlowStatus(0.5f, 1.5f));
            Debug.Log("Applied slow status");
        }

        // Apply knockback using player's knockback system
        var playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Simple: impact at player position, knock away from spider position
            playerMovement.ApplyKnockback(target.position, transform.position, meleeKnockbackVelocity, meleeKnockbackUpwardBias);
        }
        else
        {
            Debug.LogWarning("Target is missing PlayerMovement component!");
        }
    }

    void ShootProjectile()
    {
        if (!target || !projectilePrefab)
        {
            Debug.LogWarning($"Cannot shoot: target={target}, projectilePrefab={projectilePrefab}");
            return;
        }

        // Determine spawn position
        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;

        // Calculate direction to target
        Vector3 dirToTarget = (target.position + Vector3.up * 1f) - spawnPos;

        // Instantiate and initialize projectile
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Spawned projectile at {spawnPos}, toward {target.position}");

        SpiderProjectile projScript = proj.GetComponent<SpiderProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(dirToTarget);
        }
        else
        {
            Debug.LogError("SpiderProjectile prefab is missing SpiderProjectile script!");
        }
    }

    // Called by animation events - determines ranged vs melee based on distance
    public void AE_ApplyStatus()
    {
        if (!target)
        {
            Debug.LogWarning("AE_ApplyStatus: No target!");
            return;
        }

        float d = Vector3.Distance(transform.position, target.position);
        Debug.Log($"AE_ApplyStatus called at distance {d:F2}m, hasLineOfSight={hasLineOfSight}, projectilePrefab={(projectilePrefab != null ? "assigned" : "NULL")}");

        // If in ranged mode and has line of sight, shoot projectile
        if (d > meleeAttackRange && d <= rangedAttackRange && hasLineOfSight && projectilePrefab)
        {
            // Determine spawn position
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;

            // Calculate direction to target
            Vector3 dirToTarget = (target.position + Vector3.up * 1f) - spawnPos;

            // Instantiate and initialize projectile
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Debug.Log($"Spawned projectile at {spawnPos}, direction: {dirToTarget.normalized}");

            SpiderProjectile projScript = proj.GetComponent<SpiderProjectile>();
            if (projScript != null)
            {
                projScript.Initialize(dirToTarget);
            }
            else
            {
                Debug.LogError("SpiderProjectile prefab is missing SpiderProjectile script!");
            }
        }
        // Otherwise if in melee range, apply melee status and knockback
        else if (d <= meleeAttackRange)
        {
            ApplyMeleeKnockback();
        }
        else
        {
            Debug.LogWarning($"AE_ApplyStatus: Not in valid range or missing requirements. Distance={d:F2}, MeleeRange={meleeAttackRange}, RangedRange={rangedAttackRange}");
        }
    }
}
