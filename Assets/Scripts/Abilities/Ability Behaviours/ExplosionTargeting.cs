using UnityEngine;

public class ExplosionTargeting : AbilityBehaviour {
    [SerializeField] float radius = 5f;

    public override void Start(Ability ability, IAbilityTrigger trigger, TargetingManager targetingManager) {
        trigger.ConsumeCost(new(ability.GetCaster(), ability.GetCaster(), Vector3.zero));

        Vector3 source = targetingManager.transform.position;

        Collider[] candidates = Physics.OverlapSphere(source, radius, LayerMask.GetMask("Entity"));

        foreach (Collider candidate in candidates) {
            IEntity entity = candidate.GetComponent<IEntity>();
            trigger.TryExecute(new(ability.GetCaster(), entity, entity.GetPosition() - source));
        }
    }
}