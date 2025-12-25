using UnityEngine;

public class CameraTumble : MonoBehaviour
{
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.angularVelocity = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f)
        );
    }
}
