using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Hitbox {
    public class BoxHitbox : IHitbox {

        Vector3 halfExtents;
        Vector3 localOffset;
        Quaternion orientation;

        readonly Collider[] hits;

        public BoxHitbox(Vector3 halfExtents, Vector3 localOffset, Quaternion orientation) {
            this.halfExtents = halfExtents;
            this.localOffset = localOffset;
            this.orientation = orientation;
            hits = new Collider[16];
        }

        public BoxHitbox(Vector3 halfExtents, Vector3 localOffset) : this(halfExtents, localOffset, Quaternion.identity) { }

        public void Detect(Vector3 position, Quaternion rotation, ICollection<IEntity> results) {
            Quaternion rotatedOrientation = rotation * orientation;
            Vector3 center = position + rotation * localOffset;

            int count = Physics.OverlapBoxNonAlloc(center, halfExtents, hits, rotatedOrientation);

            for (int i = 0; i < count; i++) {
                if (hits[i].TryGetComponent<IEntity>(out IEntity entity))
                    results.Add(entity);
            }
        }
    }
}
