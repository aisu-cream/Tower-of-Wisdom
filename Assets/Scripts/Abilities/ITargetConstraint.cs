using System;

public interface ITargetConstraint {
    bool Evaluate(IEntity caster, IEntity target);
}

[Serializable]
class ExcludeSelfConstraint : ITargetConstraint {
    public bool Evaluate(IEntity caster, IEntity target) {
        return caster != target;
    }
}