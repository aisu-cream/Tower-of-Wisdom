using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IEntity {

    [SerializeField] float maxHealth;
    float health;

    [SerializeField] EntityLayerMask entityMask;
    EntityController controller;

    readonly List<IEffect> activeEffects = new();

    bool dead;

    public void Awake() {
        controller = GetComponent<EntityController>();
        health = maxHealth;
    }

    public float GetHealth() {
        return health;
    }

    EntityLayerMask IEntity.GetType() {
        return entityMask;
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public Vector3 GetLookDirection() {
        throw new System.NotImplementedException();
    }

    public Vector3 GetMoveDirection() {
        throw new System.NotImplementedException();
    }

    public Vector3 GetVelocity() {
        throw new System.NotImplementedException();
    }

    public void ApplyEffect(IEntity caster, Vector3 source, IEffect effect) {
        if (dead) return;
        effect.OnCompleted += RemoveEffect;
        activeEffects.Add(effect);
        effect.Apply(caster, source, this);
    }

    void RemoveEffect(IEffect effect) {
        effect.OnCompleted -= RemoveEffect;
        activeEffects.Remove(effect);
    }

    void Die() {
        Debug.Log(gameObject.name + " died.");

        foreach (var effect in activeEffects) {
            effect.OnCompleted -= RemoveEffect;
            effect.Cancel();
        }

        activeEffects.Clear();
        Destroy(gameObject);

        dead = true;
    }

    public void TakeDamage(float damage) {
        health -= damage;

        if (health > 0)
            Debug.Log(health);
        else
            Die();
    }

    public void HealHealth(float amount) {
        throw new System.NotImplementedException();
    }

    public void LockPosition(float time) {
        throw new System.NotImplementedException();
    }

    public void LockLookDirection(float time) {
        throw new System.NotImplementedException();
    }

    public void LockMoveDirection(float time) {
        throw new System.NotImplementedException();
    }

    public void Slowdown(float percentage, float time) {
        throw new System.NotImplementedException();
    }

    public void Speedup(float percentage, float time) {
        throw new System.NotImplementedException();
    }

    public void Accelerate(float percentage, float time) {
        throw new System.NotImplementedException();
    }

    public void Deccelerate(float percentage, float time) {
        throw new System.NotImplementedException();
    }

    public void ApplyKnockback(Vector3 impulse) {
        throw new System.NotImplementedException();
    }
}
