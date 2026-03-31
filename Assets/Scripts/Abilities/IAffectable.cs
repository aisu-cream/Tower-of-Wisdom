using UnityEngine;

public interface IAffectable {
    void TakeDamage(float amount);
    void Push(Vector3 impulse);
    void ApplyEffect(GameObject caster, GameObject source, IEffect effect);
}
