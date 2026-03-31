using System;
using UnityEngine;
using UnityEngine.Scripting;

[Serializable]
public class ProjectileTargeting : TargetingStrategy {
    [SerializeField, RequiredMember] ProjectileController projectilePrefab;
    ProjectileController projectileInstance;

    [SerializeField, Min(0)] float speed;

    public override void Start(Ability ability, TargetingManager targetingManager) {
        ProjectileController controller = UnityEngine.Object.Instantiate(projectilePrefab);
        EntityController entity = targetingManager.GetComponent<EntityController>();

        Vector3 velocity = speed * (entity == null ? Vector3.up : entity.GetLookDirection());

        controller.Initialize(ability, velocity);
        controller.transform.position = targetingManager.transform.position + Vector3.up;

        projectileInstance = controller;
    }

    public override void Cancel() {
        if (projectileInstance != null)
            UnityEngine.Object.Destroy(projectileInstance);
    }
}