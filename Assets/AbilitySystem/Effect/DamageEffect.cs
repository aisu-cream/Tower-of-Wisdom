using System;

public class DamageEffect : IEffect {

    float amount;
    public event Action<IEffect> OnCompleted;

    public DamageEffect(float amount) {
        this.amount = amount;
    }

    public void Apply(IEntity target) {
        target.TakeDamage(amount);
        OnCompleted?.Invoke(this);
    }

    public void Cancel() {
        OnCompleted?.Invoke(this);
    }
}
