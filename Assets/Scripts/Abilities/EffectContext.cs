#nullable enable

using UnityEngine;

public struct EffectContext {

    public IEntity Caster { get; }
    public IEntity Target { get; }
    public Vector3 Direction { get; }

    public EffectContext(IEntity caster, IEntity target, Vector3 direction) {
        Caster = caster;
        Target = target;
        Direction = direction.normalized;
    }
}
