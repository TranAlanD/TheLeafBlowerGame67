using UnityEngine;

/// <summary>
/// Helper script to find and reference bone transforms.
/// Attach to your character, click the button in Inspector to find bones.
/// </summary>
public class FindBone : MonoBehaviour
{
    [Header("Search")]
    public string boneNameToFind = "RightHand";

    [Header("Result")]
    public Transform foundBone;

    [ContextMenu("Find Bone")]
    public void SearchForBone()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.name.ToLower().Contains(boneNameToFind.ToLower()))
            {
                foundBone = child;
                Debug.Log($"Found bone: {child.name} at path: {GetTransformPath(child)}");
            }
        }

        if (foundBone == null)
        {
            Debug.LogWarning($"Could not find bone containing '{boneNameToFind}'");
        }
    }

    string GetTransformPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null && t.parent != transform)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    [ContextMenu("List All Bones")]
    public void ListAllBones()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        Debug.Log($"=== All bones in {gameObject.name} ===");
        foreach (Transform child in allChildren)
        {
            Debug.Log($"{GetTransformPath(child)}");
        }
    }
}
