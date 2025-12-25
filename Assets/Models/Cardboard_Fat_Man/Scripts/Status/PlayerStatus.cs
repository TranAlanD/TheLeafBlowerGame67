using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, IStatusAffectable
{
    private readonly List<IStatusEffect> effects = new();

    public void ApplyStatus(IStatusEffect effect)
    {
        if (effect != null) effects.Add(effect);
    }

    void Update()
    {
        // Reset before effects tick; each effect can clamp it lower.
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.externalSpeedMultiplier = 1f;

        for (int i = effects.Count - 1; i >= 0; i--)
            if (!effects[i].Tick(gameObject, Time.deltaTime))
                effects.RemoveAt(i);
    }
}
