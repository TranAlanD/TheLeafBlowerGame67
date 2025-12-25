using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.forward;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public float phaseOffset = 0f;

    private Vector3 startPos;
    private Vector3 lastPos;

    void Start()
    {
        startPos = transform.position;
        lastPos = startPos;
    }

    void Update()
    {
        float offset = Mathf.Sin((Time.time + phaseOffset) * moveSpeed) * moveDistance;
        Vector3 newPos = startPos + moveDirection.normalized * offset;
        Vector3 deltaMovement = newPos - lastPos;

        MovePlayers(deltaMovement);

        transform.position = newPos;
        lastPos = newPos;
    }

    void MovePlayers(Vector3 delta)
    {
        Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale / 2f + Vector3.up * 0.1f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.transform.position += delta;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
