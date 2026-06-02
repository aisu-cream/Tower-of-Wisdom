using ImprovedTimers;
using System;
using UnityEngine;

public interface IEffectFactory {
    IEffect Create();
}

public interface IEffect {
    void Apply(EffectContext context);
    void Cancel();
    IEffect Clone();
    event Action<IEffect> OnCompleted;
}

[Serializable]
public class DamageEffect : IEffect {
    [SerializeField] float amount = 10;

    public event Action<IEffect> OnCompleted;

    public void Apply(EffectContext context) {
        context.Target.TakeDamage(amount);
        OnCompleted?.Invoke(this);
    }

    public void Cancel() => OnCompleted?.Invoke(this);
    public IEffect Clone() => new DamageEffect() { amount = amount };
}

[Serializable]
public class KnockbackEffect : IEffect {
    [SerializeField] float impulseMagnitude = 1;
    public event Action<IEffect> OnCompleted;

    public void Apply(EffectContext context) {
        Vector3 dir = context.Direction;
        context.Target.ApplyKnockback(dir * impulseMagnitude);
        OnCompleted?.Invoke(this);
    }

    public void Cancel() => OnCompleted?.Invoke(this);
    public IEffect Clone() => new KnockbackEffect() { impulseMagnitude = impulseMagnitude };
}

[Serializable]
public class DamageOverTimeEffect : IEffect {
    [SerializeField] float duration = 5;
    [SerializeField] float tickInterval = 1;
    [SerializeField] float damagePerTick = 5;

    IntervalTimer timer;
    IEntity currentTarget;

    public event Action<IEffect> OnCompleted;

    public void Apply(EffectContext context) {
        currentTarget = context.Target;
        timer = new IntervalTimer(duration, tickInterval) {
            OnInterval = OnInterval,
            OnTimerStop = CleanUp
        };
        timer.Start();
    }

    void OnInterval() => currentTarget?.TakeDamage(damagePerTick);

    void CleanUp() {
        timer = null;
        currentTarget = null;
        OnCompleted?.Invoke(this);
    }

    public void Cancel() => timer?.Stop();
    public IEffect Clone() => new DamageOverTimeEffect() { duration = duration };
}