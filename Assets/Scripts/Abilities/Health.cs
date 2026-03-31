using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IAffectable {
    [SerializeField] float initialHealth = 100;
    float health;

    readonly List<IEffect> activeEffects = new();

    EntityController controller;
    Rigidbody rb;

    bool dead;

    void Awake() {
        controller = GetComponent<EntityController>();
        rb = GetComponent<Rigidbody>();
        health = initialHealth;
    }

    void OnValidate(){
        if (initialHealth < health)
            health = initialHealth;        
    }

    public float GetHealth() {
        return health;
    }

    public float GetInitialHealth() {
        return initialHealth;
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

    public void ApplyEffect(GameObject caster, GameObject source, IEffect effect) {
        if (dead) return;
        effect.OnCompleted += RemoveEffect;
        activeEffects.Add(effect);
        //effect.Apply(caster, source, GetComponent<IEntity>());
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
}
