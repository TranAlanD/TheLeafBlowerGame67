using UnityEngine;

/// <summary>
/// Positions the leaf blower relative to the RH_Grip_Point.
/// Set up IK constraint on the character's right hand to target RH_Grip_Point,
/// then this script will position the leaf blower at that same point with an offset.
/// </summary>
public class LeafBlowerHolder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RH_Grip_Point transform that the hand IK targets")]
    public Transform gripPoint;

    [Tooltip("The leaf blower GameObject to position")]
    public Transform leafBlower;

    [Header("Offset Settings")]
    [Tooltip("Position offset from grip point in local space")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("Rotation offset from grip point in euler angles")]
    public Vector3 rotationOffset = Vector3.zero;

    void LateUpdate()
    {
        if (gripPoint == null || leafBlower == null)
        {
            Debug.LogWarning("LeafBlowerHolder: Missing grip point or leaf blower reference!");
            return;
        }

        // Position leaf blower at grip point with offset
        leafBlower.position = gripPoint.position + gripPoint.TransformDirection(positionOffset);

        // Rotate leaf blower to match grip point with offset
        leafBlower.rotation = gripPoint.rotation * Quaternion.Euler(rotationOffset);
    }

    // Helper: Visualize the grip point and offset in editor
    void OnDrawGizmos()
    {
        if (gripPoint == null) return;

        // Draw grip point
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gripPoint.position, 0.05f);

        // Draw final leaf blower position
        if (leafBlower != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 finalPos = gripPoint.position + gripPoint.TransformDirection(positionOffset);
            Gizmos.DrawWireSphere(finalPos, 0.05f);
            Gizmos.DrawLine(gripPoint.position, finalPos);
        }
    }
}
