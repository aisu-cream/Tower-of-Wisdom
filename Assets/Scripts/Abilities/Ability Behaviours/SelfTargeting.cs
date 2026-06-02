[System.Serializable]
public class SelfTargeting : AbilityBehaviour {
    public override void Start(Ability ability, IAbilityTrigger trigger, TargetingManager targetingManager) {
        this.ability = ability;
        this.trigger = trigger;
        this.targetingManager = targetingManager;

        var target = ability.GetCaster();

        if (target != null) {
            EffectContext context = new(target, target, UnityEngine.Vector3.zero);
            trigger.ConsumeCost(context);
            trigger.TryExecute(context);
        }
    }
}
