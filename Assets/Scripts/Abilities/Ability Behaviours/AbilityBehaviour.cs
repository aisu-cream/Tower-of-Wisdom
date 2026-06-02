public abstract class AbilityBehaviour {
    protected Ability ability;
    protected IAbilityTrigger trigger;
    protected TargetingManager targetingManager;
    protected bool isTargeting = false;

    public bool IsTargeting => isTargeting;

    public abstract void Start(Ability ability, IAbilityTrigger trigger, TargetingManager targetingManager);
    public virtual void Update() { }
    public virtual void Cancel() { }
}
