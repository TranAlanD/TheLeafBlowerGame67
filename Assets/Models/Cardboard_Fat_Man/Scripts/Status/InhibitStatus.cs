using UnityEngine;

public class InhibitStatus : IStatusEffect
{
    private readonly float duration; // seconds
    private float t;
    private bool hasLoggedStart = false;

    public InhibitStatus(float duration)
    {
        this.duration = Mathf.Max(0f, duration);
    }

    public bool Tick(GameObject host, float dt)
    {
        if (!hasLoggedStart)
        {
            Debug.Log($"<color=yellow>INHIBIT STATUS ACTIVE - Player cannot use leaf blower for {duration} seconds</color>");
            hasLoggedStart = true;
        }

        t += dt;

        // This status prevents the player from using their leaf blower
        // For now, we'll just track it with a public flag that other systems can check
        var pm = host.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            // Placeholder: In the future, this could disable leaf blower functionality
            // For now, you can check if this status is active by looking at the PlayerStatus component
        }

        bool stillActive = t < duration;

        if (!stillActive)
        {
            Debug.Log("<color=green>Inhibit status expired - Player can use leaf blower again</color>");
        }

        return stillActive;
    }
}
