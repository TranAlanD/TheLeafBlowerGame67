using UnityEngine;

/// <summary>
/// Simple attack animation for meshes without rigged animators.
/// Uses scale/rotation/position tweening to create attack effect.
/// Call TriggerAttack() to play the animation.
/// </summary>
public class SimpleAttackAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of attack animation in seconds")]
    public float attackDuration = 0.5f;

    [Tooltip("How much to scale up during attack")]
    public float attackScaleMultiplier = 1.2f;

    [Tooltip("How much to shake/rotate during attack")]
    public float attackShakeAmount = 10f;

    [Tooltip("Attack animation curve (controls timing)")]
    public AnimationCurve attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [Tooltip("Sound to play when attack starts")]
    public AudioClip attackSound;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isAttacking = false;
    private float attackTime = 0f;
    private AudioSource audioSource;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isAttacking)
        {
            attackTime += Time.deltaTime;
            float progress = Mathf.Clamp01(attackTime / attackDuration);
            float curveValue = attackCurve.Evaluate(progress);

            // Scale pulse
            float scaleAmount = 1f + (curveValue * (attackScaleMultiplier - 1f));
            transform.localScale = originalScale * scaleAmount;

            // Shake/rotate
            float shakeX = Mathf.Sin(progress * Mathf.PI * 4) * attackShakeAmount * (1f - progress);
            float shakeZ = Mathf.Cos(progress * Mathf.PI * 3) * attackShakeAmount * (1f - progress);
            transform.localRotation = originalRotation * Quaternion.Euler(shakeX, 0, shakeZ);

            // End animation
            if (progress >= 1f)
            {
                isAttacking = false;
                transform.localScale = originalScale;
                transform.localRotation = originalRotation;
            }
        }
    }

    /// <summary>
    /// Trigger the attack animation. Call this from MushroomMom.
    /// </summary>
    public void TriggerAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        attackTime = 0f;

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Debug.Log($"<color=red>{gameObject.name} attack animation triggered!</color>");
    }

    /// <summary>
    /// Check if attack animation is currently playing
    /// </summary>
    public bool IsAttacking()
    {
        return isAttacking;
    }
}
