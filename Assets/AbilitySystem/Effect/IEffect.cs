using System;

public interface IEffect {
    void Apply(IEntity target);
    void Cancel();
    event Action<IEffect> OnCompleted;
}
