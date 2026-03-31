using UnityEngine;

[System.Flags]
public enum EntityType {
    None   = 0,
    Player = 1 << 0,
    Enemy  = 1 << 1,
}

[System.Serializable]
public readonly struct EntityLayerMask {
    readonly int mask;

    public EntityLayerMask(int value) => mask = value;

    public static implicit operator EntityLayerMask(int value) => new EntityLayerMask(value);
    public static implicit operator EntityLayerMask(EntityType type) => new EntityLayerMask((int) type);
    public static implicit operator int(EntityLayerMask mask) => mask.mask;

    public static EntityLayerMask operator +(EntityLayerMask a, EntityLayerMask b) => new EntityLayerMask(a.mask | b.mask);
    public static EntityLayerMask operator +(EntityLayerMask a, int b) => new EntityLayerMask(a.mask | b);
    public static EntityLayerMask operator +(EntityLayerMask a, EntityType b) => new EntityLayerMask(a.mask | (int) b);
    public static EntityLayerMask operator -(EntityLayerMask a, EntityLayerMask b) => new EntityLayerMask(a.mask & ~(int) b);
    public static EntityLayerMask operator -(EntityLayerMask a, int b) => new EntityLayerMask(a.mask & ~b);

    public bool Contains(EntityType type) {
        return (mask & (int) type) != 0;
    }

    public bool ContainsAll(EntityType type) {
        int iType = (int)type;
        return (mask & iType) == iType;
    }
}

public interface IEntity {
    float GetHealth();
    EntityLayerMask GetType();
    Vector3 GetPosition();
    Vector3 GetLookDirection();
    Vector3 GetMoveDirection();
    Vector3 GetVelocity();
    void ApplyEffect(IEntity caster, Vector3 source, IEffect effect);
    void TakeDamage(float damage);
    void HealHealth(float amount);
    void LockPosition(float time);
    void LockLookDirection(float time);
    void LockMoveDirection(float time);
    void Slowdown(float percentage, float time);
    void Speedup(float percentage, float time);
    void Accelerate(float percentage, float time);
    void Deccelerate(float percentage, float time);
    void ApplyKnockback(Vector3 impulse);
}