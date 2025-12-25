using UnityEngine;

/// <summary>
/// Plays legacy Animation clips on non-rigged meshes.
/// Use this instead of SimpleAttackAnimation if you have actual animation clips.
/// </summary>
public class LegacyAttackAnimation : MonoBehaviour
{
    [Tooltip("Animation component (uses legacy Animation, not Animator)")]
    public Animation animationComponent;

    [Tooltip("Name of the attack animation clip to play")]
    public string attackAnimationName = "Attack";

    [Tooltip("Should the animation loop?")]
    public bool loop = false;

    void Start()
    {
        // Auto-find Animation component if not assigned
        if (animationComponent == null)
        {
            animationComponent = GetComponent<Animation>();
        }

        if (animationComponent == null)
        {
            Debug.LogError($"LegacyAttackAnimation: No Animation component found on {gameObject.name}");
        }
        else
        {
            // If attackAnimationName is empty, auto-detect the first animation
            if (string.IsNullOrEmpty(attackAnimationName))
            {
                foreach (AnimationState state in animationComponent)
                {
                    attackAnimationName = state.name;
                    Debug.Log($"<color=cyan>LegacyAttackAnimation: Auto-detected animation '{attackAnimationName}' on {gameObject.name}</color>");
                    break; // Use first animation
                }
            }

            // List all available animation clips for debugging
            Debug.Log($"<color=cyan>Available animations on {gameObject.name}:</color>");
            foreach (AnimationState state in animationComponent)
            {
                Debug.Log($"  - {state.name} {(state.name == attackAnimationName ? "(SELECTED)" : "")}");
            }
        }
    }

    public void TriggerAttack()
    {
        if (animationComponent == null)
        {
            Debug.LogWarning("LegacyAttackAnimation: Cannot play attack - no Animation component!");
            return;
        }

        if (string.IsNullOrEmpty(attackAnimationName))
        {
            Debug.LogWarning("LegacyAttackAnimation: attackAnimationName not set!");
            return;
        }

        // Check if animation exists
        AnimationClip clip = animationComponent.GetClip(attackAnimationName);
        if (clip == null)
        {
            Debug.LogWarning($"LegacyAttackAnimation: Animation '{attackAnimationName}' not found!");
            return;
        }

        // Set looping
        if (animationComponent[attackAnimationName] != null)
        {
            animationComponent[attackAnimationName].wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
        }

        // Play the animation
        animationComponent.Play(attackAnimationName);
        Debug.Log($"<color=green>Playing attack animation: {attackAnimationName}</color>");
    }

    public void StopAttack()
    {
        if (animationComponent != null && !string.IsNullOrEmpty(attackAnimationName))
        {
            animationComponent.Stop(attackAnimationName);
        }
    }
}
