using ImprovedTimers;
using System;
using UnityEngine;

public interface IEffectFactory {
    IEffect Create();
}

public interface IEffect {
    void Apply(IEntity target);
    void Cancel();
    IEffect Clone();
    event Action<IEffect> OnCompleted;
}

[Serializable]
public class DamageEffect : IEffect {
    [SerializeField] float amount = 10;

    public event Action<IEffect> OnCompleted;

    public void Apply(IEntity target) {
        target.TakeDamage(amount);
        OnCompleted?.Invoke(this);
    }

    public void Cancel() => OnCompleted?.Invoke(this);
    public IEffect Clone() => new DamageEffect() { amount = amount };
}

[Serializable]
public class DamageOverTimeEffect : IEffect {
    [SerializeField] float duration = 5;
    [SerializeField] float tickInterval = 1;
    [SerializeField] float damagePerTick = 5;

    IntervalTimer timer;
    IEntity currentTarget;

    public event Action<IEffect> OnCompleted;

    public void Apply(IEntity target) {
        currentTarget = target;
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