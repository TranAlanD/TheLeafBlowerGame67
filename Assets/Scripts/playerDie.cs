using UnityEngine;

public class playerDie : MonoBehaviour
{
    public float fallY = -10f;
    public Transform spawnPoint;

    void Update()
    {
        if (transform.position.y < fallY && spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;
        }
    }
}
