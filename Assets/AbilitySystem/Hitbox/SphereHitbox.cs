using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Hitbox {
    public class SphereHitbox : IHitbox {

        readonly Vector3 localOffset;
        readonly float radius;

        readonly Collider[] hits;

        public SphereHitbox(Vector3 localOffset, float radius) {
            this.localOffset = localOffset;
            this.radius = radius;
            hits = new Collider[16];
        }

        public void Detect(Vector3 position, Quaternion rotation, ICollection<IEntity> results) {
            Vector3 center = position + rotation * localOffset;

            int count = Physics.OverlapSphereNonAlloc(center, radius, hits);

            for (int i = 0; i < count; i++) {
                if (hits[i].TryGetComponent<IEntity>(out var entity)) 
                    results.Add(entity);
            }
        }
    }
}
