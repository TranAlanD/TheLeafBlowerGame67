using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(Animator))]
public class SpiderMotor : MonoBehaviour
{
    public NavMeshAgent agent;
    public Rigidbody rb;
    public Animator animator;

    [Header("Tuning")]
    public float maxSpeed = 3.5f;
    public float turnSpeed = 720f;
    public float animatorSpeedLerp = 10f;

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponent<Animator>();

        // NavMeshAgent should plan but not move the transform directly.
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;

        // Rigidbody drives movement and rotation.
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // Freeze tipping, allow Y rotation.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (!agent.isActiveAndEnabled) return;

        // Move toward agent�s next position.
        Vector3 toNext = agent.nextPosition - rb.position;
        toNext.y = 0f;

        float step = maxSpeed * Time.fixedDeltaTime;
        Vector3 move = Vector3.ClampMagnitude(toNext, step);
        rb.MovePosition(rb.position + move);

        // Rotate toward desired velocity, or toward target when near stop.
        Vector3 vel = agent.desiredVelocity;
        vel.y = 0f;

        // Fallback: if nearly stopped, still face steering target / player.
        if (vel.sqrMagnitude < 0.001f && agent.hasPath)
        {
            Vector3 dirToTarget = agent.steeringTarget - rb.position;
            dirToTarget.y = 0f;
            if (dirToTarget.sqrMagnitude > 0.001f)
                vel = dirToTarget;
        }

        if (vel.sqrMagnitude > 0.001f)
        {
            Quaternion want = Quaternion.LookRotation(vel.normalized);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, want, turnSpeed * Time.fixedDeltaTime));
        }

        // Keep NavMeshAgent synced to Rigidbody.
        agent.nextPosition = rb.position;
    }

    private void Update()
    {
        // Animator speed parameter (for Idle <-> Walk blend)
        float planarSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
        float target = Mathf.Max(planarSpeed, agent.desiredVelocity.magnitude);

        float current = animator.GetFloat("Speed");
        float blended = Mathf.Lerp(current, target, Time.deltaTime * animatorSpeedLerp);
        animator.SetFloat("Speed", blended);
    }
}
