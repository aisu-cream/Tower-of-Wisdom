using ImprovedTimers;
using System;

public class DamageOverTimeEffect : IEffect {

    float duration;
    float tickInterval;
    float damagePerTick;

    IntervalTimer timer;
    IEntity currentTarget;

    public event Action<IEffect> OnCompleted;

    public DamageOverTimeEffect(float duration, float tickInterval, float damagePerTick) {
        this.duration = duration;
        this.tickInterval = tickInterval;
        this.damagePerTick = damagePerTick;
    }

    public void Apply(IEntity target) {
        currentTarget = target;
        timer = new IntervalTimer(duration, tickInterval) {
            OnInterval = OnInterval,
            OnTimerStop = CleanUp
        };
        timer.Start();
    }

    void OnInterval() {
        currentTarget?.TakeDamage(damagePerTick);
    }

    void CleanUp() {
        timer = null;
        currentTarget = null;
        OnCompleted?.Invoke(this);
    }

    public void Cancel() {
        timer?.Stop();
    }
}