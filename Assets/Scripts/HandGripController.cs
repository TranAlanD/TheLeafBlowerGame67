using UnityEngine;

/// <summary>
/// Positions a leaf blower at the RH_Grip_Point with offset.
/// RH_Grip_Point should be a child of the character at a fixed local position.
/// IK will make the arm reach to this static point. No animation needed on grip point.
/// </summary>
public class HandGripController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The grip point - should be a child of character at fixed local position (IK target)")]
    public Transform gripPoint;

    [Tooltip("The leaf blower prefab to instantiate")]
    public GameObject leafBlowerPrefab;

    [Header("IK Settings")]
    [Range(0f, 1f)]
    [Tooltip("IK blend weight - 0 = use animation, 1 = reach to grip point")]
    public float ikWeight = 1f;

    [Header("Offsets")]
    [Tooltip("Position offset for leaf blower from grip point")]
    public Vector3 leafBlowerPositionOffset = Vector3.zero;

    [Tooltip("Rotation offset for leaf blower from grip point")]
    public Vector3 leafBlowerRotationOffset = Vector3.zero;

    private Transform leafBlowerInstance;

#if UNITY_EDITOR || UNITY_2019_3_OR_NEWER
    // Reference to IK constraint if using Animation Rigging
    private UnityEngine.Animations.Rigging.TwoBoneIKConstraint ikConstraint;
#endif

    void Start()
    {
        // Instantiate leaf blower if prefab is assigned
        if (leafBlowerPrefab != null && gripPoint != null)
        {
            GameObject instance = Instantiate(leafBlowerPrefab, gripPoint);
            leafBlowerInstance = instance.transform;
            Debug.Log("Leaf blower instantiated as child of grip point");
        }

#if UNITY_EDITOR || UNITY_2019_3_OR_NEWER
        // Try to find IK constraint in children
        ikConstraint = GetComponentInChildren<UnityEngine.Animations.Rigging.TwoBoneIKConstraint>();
        if (ikConstraint == null)
        {
            Debug.LogWarning("HandGripController: No TwoBoneIKConstraint found in children. IK weight control won't work.");
        }
#endif
    }

    void LateUpdate()
    {
        if (gripPoint == null)
        {
            Debug.LogWarning("HandGripController: Missing grip point reference!");
            return;
        }

        // Update IK weight if using Animation Rigging
#if UNITY_EDITOR || UNITY_2019_3_OR_NEWER
        if (ikConstraint != null)
        {
            ikConstraint.weight = ikWeight;
        }
#endif

        // Update leaf blower offset every frame so Inspector changes are applied
        if (leafBlowerInstance != null)
        {
            leafBlowerInstance.localPosition = leafBlowerPositionOffset;
            leafBlowerInstance.localRotation = Quaternion.Euler(leafBlowerRotationOffset);
        }
    }

    void OnDrawGizmos()
    {
        if (gripPoint == null) return;

        // Draw grip point
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gripPoint.position, 0.05f);


        // Draw leaf blower position (preview using offset)
        if (gripPoint != null && leafBlowerPrefab != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 finalPos = gripPoint.TransformPoint(leafBlowerPositionOffset);
            Gizmos.DrawWireSphere(finalPos, 0.05f);
        }
    }
}
