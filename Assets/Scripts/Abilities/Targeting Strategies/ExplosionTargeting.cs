using UnityEngine;

public class ExplosionTargeting : TargetingStrategy {
    [SerializeField] float radius = 5f;

    public override void Start(Ability ability, TargetingManager targetingManager) {
        Vector3 source = targetingManager.transform.position;

        Collider[] candidates = Physics.OverlapSphere(source, radius, LayerMask.GetMask("Entity"));

        foreach (Collider candidate in candidates) {
            IEntity entity = candidate.GetComponent<IEntity>();
            if (entity == null || !ability.CanExecute(entity)) continue;

            Vector3 displacement = entity.GetPosition() - source;

            ability.Execute(source, entity);
        }
    }
}