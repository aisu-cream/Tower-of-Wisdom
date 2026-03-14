using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IAffectable {
    float health = 100;
    readonly List<IEffect> activeEffects = new();

    private EntityController controller;
    private Rigidbody rb;

    void Awake() {
        controller = GetComponent<EntityController>();
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(float amount) {
        health -= amount;

        if (health > 0)
            Debug.Log(health);
        else
            Die();
    }

    public void Push(Vector3 impulse) {
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    public void ApplyEffect(TargetingManager caster, IEffect effect) {
        effect.OnCompleted += RemoveEffect;
        activeEffects.Add(effect);
        effect.Apply(caster, this);
    }

    void RemoveEffect(IEffect effect) {
        effect.OnCompleted -= RemoveEffect;
        activeEffects.Remove(effect);
    }

    void Die() {
        Debug.Log(gameObject + " died.");

        foreach (var effect in activeEffects) {
            effect.OnCompleted -= RemoveEffect;
            effect.Cancel();
        }

        activeEffects.Clear();
        Destroy(gameObject);
    }
}
