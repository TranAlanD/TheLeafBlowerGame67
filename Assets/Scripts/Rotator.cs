using UnityEngine;

public class Rotator : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }
    public RotationAxis axis = RotationAxis.Y;
    public float rotationSpeed = 100f;
    public float moveForce = 3f;

    private void Update()
    {
        Vector3 rotationVector = axis switch
        {
            RotationAxis.X => Vector3.right,
            RotationAxis.Y => Vector3.up,
            RotationAxis.Z => Vector3.forward,
            _ => Vector3.up
        };

        transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float radius = 0.5f;
                Vector3 tangentialDir = axis switch
                {
                    RotationAxis.X => transform.up,
                    RotationAxis.Y => transform.right,
                    RotationAxis.Z => transform.up,
                    _ => transform.right
                };
                Vector3 tangentialVelocity = tangentialDir * rotationSpeed * Mathf.Deg2Rad * radius;

                Vector3 newVel = new Vector3(tangentialVelocity.x, rb.linearVelocity.y, tangentialVelocity.z);
                rb.linearVelocity = newVel;
            }
        }
    }
}
