public class SelfTargeting : TargetingStrategy {
    public override void Start(Ability ability, TargetingManager targetingManager) {
        this.ability = ability;
        this.targetingManager = targetingManager;

        if (targetingManager.transform.TryGetComponent<IAffectable>(out var target))
            ability.Execute(targetingManager, target);
    }
}
