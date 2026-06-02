using System;
using UnityEngine;
using UnityEngine.Scripting;

[Serializable]
public class ProjectileTargeting : AbilityBehaviour {
    [SerializeField, RequiredMember] ProjectileController projectilePrefab;
    ProjectileController projectileInstance;

    [SerializeField, Min(0)] float speed;

    public override void Start(Ability ability, IAbilityTrigger trigger, TargetingManager targetingManager) {
        ProjectileController controller = UnityEngine.Object.Instantiate(projectilePrefab);
        EntityController entity = targetingManager.GetComponent<EntityController>();

        Vector3 velocity = speed * (entity == null ? Vector3.up : entity.GetLookDirection());

        trigger.ConsumeCost(new(ability.GetCaster(), ability.GetCaster(), -velocity));
        controller.Initialize(ability, trigger, velocity);
        controller.transform.position = targetingManager.transform.position + Vector3.up;

        projectileInstance = controller;
    }

    public override void Cancel() {
        if (projectileInstance != null)
            UnityEngine.Object.Destroy(projectileInstance);
    }
}