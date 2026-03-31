public class SelfTargeting : TargetingStrategy {
    public override void Start(Ability ability, TargetingManager targetingManager) {
        this.ability = ability;
        this.targetingManager = targetingManager;

        var target = ability.GetCaster();
        if (target != null)
            ability.Execute(target.GetPosition(), target);
    }
}
