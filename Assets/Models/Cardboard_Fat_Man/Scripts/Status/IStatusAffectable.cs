using UnityEngine;

public interface IStatusEffect
{
    // Return false when the effect has finished.
    bool Tick(GameObject host, float dt);
}

public interface IStatusAffectable
{
    void ApplyStatus(IStatusEffect effect);
}
