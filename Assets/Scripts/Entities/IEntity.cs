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

    public static implicit operator EntityLayerMask(int value) => new(value);
    public static implicit operator EntityLayerMask(EntityType type) => new((int) type);
    public static implicit operator int(EntityLayerMask mask) => mask.mask;

    public static EntityLayerMask operator +(EntityLayerMask a, EntityLayerMask b) => new(a.mask | b.mask);
    public static EntityLayerMask operator +(EntityLayerMask a, int b) => new(a.mask | b);
    public static EntityLayerMask operator +(EntityLayerMask a, EntityType b) => new(a.mask | (int) b);
    public static EntityLayerMask operator -(EntityLayerMask a, EntityLayerMask b) => new(a.mask & ~(int) b);
    public static EntityLayerMask operator -(EntityLayerMask a, int b) => new(a.mask & ~b);

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
    float GetInitialHealth();
    bool IsAlive();
    EntityLayerMask GetType();
    Vector3 GetCenterPosition();
    Vector3 GetBottomPosition();
    Vector3 GetClosestPoint(in Vector3 point);
    Vector3 GetVelocity();
    Vector3 GetLookDirection();
    void AlertEffect(IEffect effect);
    void AlertThreat(IEntity threat);
    void TakeDamage(float damage);
    void HealHealth(float amount);
    void ApplyKnockback(in Vector3 impulse);
}