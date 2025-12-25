using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;

    public float sprintSpeed = 8f;
    public float walkSpeed = 4f;
    public float jumpForce = 5f;

    public float friction = 10f;
    public float gravity = 3f;

    public float externalSpeedMultiplier = 1f;
    public Vector3 externalVelocity = Vector3.zero;

    private Rigidbody rigidBody;
    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded = false;
    private bool isSprinting = true;
    private bool isJumping = false;
    private bool hasLeftGround = false;
    private bool sprintMode = true;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horzMove = Input.GetAxisRaw("Horizontal");
        float vertMove = Input.GetAxisRaw("Vertical");
        moveDirection = (transform.forward * vertMove + transform.right * horzMove).normalized;

        animator.SetFloat("x", horzMove, 0.1f, Time.deltaTime);
        animator.SetFloat("y", vertMove, 0.1f, Time.deltaTime);

        bool isMoving = moveDirection.magnitude > 0f;
        animator.SetBool("idle", !isMoving);
        animator.SetBool("isSprinting", isSprinting && isMoving);

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
            hasLeftGround = false;
            animator.SetBool("isJumping", true);
            isGrounded = false;
        }

        if (isJumping && !hasLeftGround && !isGrounded)
            hasLeftGround = true;

        if (isJumping && hasLeftGround && isGrounded)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }

        isSprinting = Input.GetKey(KeyCode.LeftShift);
        sprintMode = isSprinting;
    }

    void FixedUpdate()
    {
        Vector3 inputVelocity = moveDirection * MoveSpeed() / 10f * externalSpeedMultiplier;
        float currentY = rigidBody.linearVelocity.y;
        Vector3 horizontalVelocity = new Vector3(inputVelocity.x + externalVelocity.x, 0, inputVelocity.z + externalVelocity.z);

        // If there's external vertical velocity, use it instead of currentY (like rocket jump)
        // Otherwise use currentY (normal gravity/jumping)
        float verticalVelocity = (Mathf.Abs(externalVelocity.y) > 0.1f) ? externalVelocity.y : currentY;

        rigidBody.linearVelocity = new Vector3(
            horizontalVelocity.x * (100 - friction) / 100f,
            verticalVelocity,
            horizontalVelocity.z * (100 - friction) / 100f
        );

        rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, 5f * Time.fixedDeltaTime);
    }

    // Enemy knockback - calculates direction and applies force via externalVelocity
    public void ApplyKnockback(Vector3 impactPoint, Vector3 enemyPosition, float knockbackVelocity, float upwardBias = 0.3f)
    {
        if (rigidBody == null)
        {
            Debug.LogError("ApplyKnockback: rigidBody is NULL!");
            return;
        }

        // Calculate direction from enemy to impact point
        Vector3 knockbackDir = (impactPoint - enemyPosition).normalized;

        // Add upward bias to launch player up and away
        knockbackDir.y += upwardBias;
        knockbackDir.Normalize();

        // Apply velocity in that direction using externalVelocity system
        Vector3 finalVelocity = knockbackDir * knockbackVelocity;
        externalVelocity = finalVelocity;

        Debug.Log($"<color=orange>Knockback: Enemy at {enemyPosition}, Impact at {impactPoint}, Dir: {knockbackDir}, Velocity: {finalVelocity}</color>");
    }

    // Simple knockback - directly applies force vector
    public void ApplyKnockback(Vector3 force)
    {
        externalVelocity += force;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            isGrounded = true;
    }

    public float MoveSpeed()
    {
        float baseSpeed = sprintMode ? sprintSpeed : walkSpeed;
        return baseSpeed * externalSpeedMultiplier;
    }
}
