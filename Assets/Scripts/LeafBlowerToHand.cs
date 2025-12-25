using UnityEngine;

public class LeafBlowerToHand : MonoBehaviour
{
    public Animator animator;  // Assign your character’s Animator here
    public HumanBodyBones bone = HumanBodyBones.RightHand;

    private bool attached = false;

    void LateUpdate()
    {
        if (!attached)
        {
            Transform hand = animator.GetBoneTransform(bone);
            transform.SetParent(hand, false);
            transform.localPosition = new Vector3(-0.1f, 0f, 0f);
            transform.localRotation = Quaternion.Euler(0, 0, -90f);
            transform.localScale = Vector3.one * 0.5f;

            attached = true;
        }
    }

}
