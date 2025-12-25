using UnityEngine;

public class SlowStatus : IStatusEffect
{
    private readonly float factor;   // e.g., 0.5f means 50% speed
    private readonly float duration; // seconds
    private float t;
    private bool hasLoggedStart = false;

    public SlowStatus(float factor, float duration)
    {
        this.factor = Mathf.Clamp(factor, 0f, 1f);
        this.duration = Mathf.Max(0f, duration);
    }

    public bool Tick(GameObject host, float dt)
    {
        if (!hasLoggedStart)
        {
            Debug.Log($"<color=yellow>SLOW STATUS ACTIVE - Player movement at {factor * 100}% speed for {duration} seconds</color>");
            hasLoggedStart = true;
        }

        t += dt;

        // Example hookup: a field the player controller reads each frame.
        var pm = host.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            // Keep the smallest multiplier when multiple slows stack
            pm.externalSpeedMultiplier = Mathf.Min(pm.externalSpeedMultiplier, factor);
        }

        bool stillActive = t < duration;

        if (!stillActive)
        {
            Debug.Log("<color=green>Slow status expired - Player movement back to normal</color>");
        }

        return stillActive;
    }
}
