using UnityEngine;

public interface IAffectable {
    void TakeDamage(float amount);
    void Push(Vector3 impulse);
    void ApplyEffect(TargetingManager targetingManager, IEffect effect);
}
