using UnityEngine;

public abstract class TargetingStrategy {
    protected Ability ability;
    protected TargetingManager targetingManager;
    protected bool isTargeting = false;

    public bool IsTargeting => isTargeting;

    public abstract void Start(Ability ability, TargetingManager targetingManager);
    public virtual void Update() { }
    public virtual void Cancel() { }
}
