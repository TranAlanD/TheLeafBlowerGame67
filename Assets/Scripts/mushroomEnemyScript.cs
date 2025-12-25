using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MushroomEnemy : MonoBehaviour
{
    public FPS_Cursor fps_cursor;
    public Transform player;
    public Animator animator;
    public GameObject hitEffectPrefab;

    public float detectionRadius = 12f;
    public float chargeSpeed = 5f;
    public float chargeCooldown = 2f;
    public float chargeDuration = 1.5f;
    public float wanderSpeed = 2f;
    public float wanderTimeMin = 1f;
    public float wanderTimeMax = 3f;
    public float rayDistance = 2f;

    public float knockbackForce = 7f;
    public float knockbackUpward = 0.3f;

    public AudioClip hitSound;
    public float hitVolume = 1f;

    private Rigidbody rb;
    private float lastChargeTime;
    private Vector3 wanderDirection;
    private float wanderTimer;

    private bool isCharging = false;
    private float chargeEndTime;
    private Vector3 chargeDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        lastChargeTime = -chargeCooldown;
        PickNewWanderDirection();
    }

    void FixedUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 movement = Vector3.zero;

        if (!isCharging && distanceToPlayer <= detectionRadius &&
            Time.time >= lastChargeTime + chargeCooldown &&
            IsGroundAhead((player.position - transform.position).normalized))
        {
            isCharging = true;
            chargeEndTime = Time.time + chargeDuration;
            lastChargeTime = Time.time;

            chargeDirection = (player.position - transform.position).normalized;
            chargeDirection.y = 0;

            animator.SetTrigger("Attack");
        }

        if (isCharging)
        {
            if (chargeDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(chargeDirection);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }

            rb.linearVelocity = new Vector3(
                chargeDirection.x * chargeSpeed,
                rb.linearVelocity.y,
                chargeDirection.z * chargeSpeed
            );

            if (Time.time >= chargeEndTime || !IsGroundAhead(chargeDirection))
                isCharging = false;
        }
        else
        {
            wanderTimer -= Time.fixedDeltaTime;

            if (wanderTimer <= 0 || !IsGroundAhead(wanderDirection) || !IsPathClear(wanderDirection))
                PickNewWanderDirection();

            movement = wanderDirection * wanderSpeed;

            if (movement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.fixedDeltaTime));
            }

            if (IsPathClear(movement))
                rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            else
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsWalking", !isCharging && speed > 0.1f);
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
        wanderTimer = Random.Range(wanderTimeMin, wanderTimeMax);
    }

    bool IsGroundAhead(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f + direction * 0.5f;
        return Physics.Raycast(origin, Vector3.down, rayDistance);
    }

    bool IsPathClear(Vector3 direction)
    {
        if (direction.magnitude < 0.1f) return true;
        return !Physics.Raycast(transform.position + Vector3.up * 0.5f, direction.normalized, 0.5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (player != null && collision.gameObject == player.gameObject)
        {
            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);

            PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                if (fps_cursor != null)
                    fps_cursor.isShaking = true;

                Vector3 horizontalDir = (player.position - transform.position);
                horizontalDir.y = 0;
                horizontalDir.Normalize();

                Vector3 knockVector = horizontalDir * knockbackForce;
                knockVector.y = knockbackUpward * knockbackForce;

                playerScript.ApplyKnockback(knockVector);
            }

            Vector3 attackDir = (player.position - transform.position).normalized;
            Vector3 effectPosition = transform.position + attackDir * 0.5f;
            Quaternion effectRotation = Quaternion.LookRotation(attackDir);

            GameObject effect = Instantiate(hitEffectPrefab, effectPosition, effectRotation);
            Destroy(effect, 1.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + wanderDirection * 0.5f,
                        transform.position + wanderDirection * 0.5f + Vector3.down * rayDistance);
    }

    public void AE_ApplyStatus()
    {
        Debug.Log("AE_ApplyStatus received (FBX hidden event).");
    }
}
