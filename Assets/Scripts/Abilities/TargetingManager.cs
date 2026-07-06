using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetingManager {
    public abstract List<IEntity> FindTargets(Vector3 basePosition);
}

[Serializable]
public class SphereTargeting : TargetingManager {

    [SerializeField, Min(0)] float radius;

    public override List<IEntity> FindTargets(Vector3 basePosition) {
        List<IEntity> targetList = new();
        Collider[] candidateColliders = Physics.OverlapSphere(basePosition, radius);

        if (candidateColliders == null)
            return targetList;
        
        foreach (Collider candidateCollider in candidateColliders) {
            IEntity entity = candidateCollider.GetComponent<IEntity>();
            if (entity != null)
                targetList.Add(entity);
        }

        return targetList;
    }
}