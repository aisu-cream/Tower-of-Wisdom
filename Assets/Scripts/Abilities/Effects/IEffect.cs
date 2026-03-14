using ImprovedTimers;
using System;
using UnityEngine;

public interface IEffectFactory {
    IEffect Create();
}

public interface IEffect {
    void Apply(TargetingManager caster, IAffectable target);
    void Cancel();
    event Action<IEffect> OnCompleted;
}

[Serializable]
public class DamageEffectFactory : IEffectFactory {
    public float amount = 10;

    public IEffect Create() {
        return new DamageEffect { amount = amount };
    }

    struct DamageEffect : IEffect {
        public float amount;

        public event Action<IEffect> OnCompleted;

        public void Apply(TargetingManager caster, IAffectable target) {
            target.TakeDamage(amount);
            OnCompleted?.Invoke(this);
        }

        public void Cancel() { 
            OnCompleted?.Invoke(this);
        }
    }
}

[Serializable]
public class KnockbackEffectFactory : IEffectFactory {
    public float impulseMagnitude = 1;

    public IEffect Create() {
        return new KnockbackEffect { impulseMagnitude = impulseMagnitude };
    }

    struct KnockbackEffect : IEffect {
        public float impulseMagnitude;
        private static readonly Vector3 defaultDir = Vector3.forward;

        public event Action<IEffect> OnCompleted;

        public void Apply(TargetingManager caster, IAffectable target) {
            if (target == null) return;
            if (caster != null) target.Push(defaultDir * impulseMagnitude);

            MonoBehaviour targetMb = target as MonoBehaviour;
            Vector3 dir = targetMb.transform.position - caster.transform.position;

            target.Push(dir * impulseMagnitude);
            OnCompleted?.Invoke(this);
        }

        public void Cancel() {
            OnCompleted?.Invoke(this);
        }
    }
}

[Serializable]
public class DamageOverTimeEffectFactory : IEffectFactory {
    public float duration = 5f;
    public float tickInterval = 1f;
    public float damagePerTick = 5f;

    public IEffect Create() {
        return new DamageOverTimeEffect {
            duration = duration,
            tickInterval = tickInterval,
            damagePerTick = damagePerTick
        };
    }

    struct DamageOverTimeEffect : IEffect {
        public float duration;
        public float tickInterval;
        public float damagePerTick;

        IntervalTimer timer;
        IAffectable currentTarget;

        public event Action<IEffect> OnCompleted;

        public void Apply(TargetingManager caster, IAffectable target) {
            currentTarget = target;
            timer = new IntervalTimer(duration, tickInterval);
            timer.OnInterval = OnInterval;
            timer.OnTimerStop = CleanUp;
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

        public void Cancel() => timer?.Stop();
    }
}