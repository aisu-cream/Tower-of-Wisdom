using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityController))]
public class Entity : MonoBehaviour, IEntity {

    [SerializeField] float maxHealth;
    float health;
    bool dead;

    [SerializeField] EntityLayerMask entityMask;
    EntityController controller;

    readonly List<IEffect> activeEffects = new();

    IEntity currentThreat;

    void Awake() {
        controller = GetComponent<EntityController>();
        health = maxHealth;
    }

    public float GetHealth() {
        return health;
    }

    public bool IsAlive() {
        return health > 0;
    }

    EntityLayerMask IEntity.GetType() {
        return entityMask;
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public Vector3 GetLookDirection() {
        return controller.GetLookDirection();
    }

    public Vector3 GetMoveDirection() {
        return controller.GetMoveDirection();
    }

    public Vector3 GetVelocity() {
        return controller.GetVelocity();
    }

    public void AlertEffect(IEffect effect) {
        if (!IsAlive()) return;
        effect.OnCompleted += RemoveEffect;
        activeEffects.Add(effect);
    }

    public void AlertThreat(IEntity entity) {
        currentThreat = entity;
    }

    void RemoveEffect(IEffect effect) {
        effect.OnCompleted -= RemoveEffect;
        activeEffects.Remove(effect);
    }

    void Die() {
        dead = true;
        Debug.Log(gameObject.name + " died.");

        foreach (var effect in activeEffects) {
            effect.OnCompleted -= RemoveEffect;
            effect.Cancel();
        }

        activeEffects.Clear();
        Destroy(gameObject);
    }

    public void TakeDamage(float damage) {
        health -= damage;

        if (IsAlive())
            Debug.Log(health);
        else if (!dead)
            Die();
    }

    public void HealHealth(float amount) {
        if (!IsAlive()) return;
        health = Mathf.Min(maxHealth, health + amount);
    }

    public void ApplyKnockback(Vector3 impulse) {
        throw new System.NotImplementedException();
    }
}
