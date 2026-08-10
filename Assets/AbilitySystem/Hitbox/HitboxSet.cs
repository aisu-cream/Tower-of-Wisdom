
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Hitbox {
    public class HitboxSet {

        readonly HashSet<IHitbox> activeHitboxes;
        readonly HashSet<IEntity> detectedEntities;
        readonly List<IEntity> detectionBuffer;

        public HitboxSet() {
            activeHitboxes = new HashSet<IHitbox>();
            detectedEntities = new HashSet<IEntity>();
            detectionBuffer = new List<IEntity>();
        }

        public void Enable(IHitbox hitbox) {
            if (hitbox == null)
                throw new ArgumentNullException(nameof(hitbox));

            activeHitboxes.Add(hitbox);
        }

        public void Disable(IHitbox hitbox) {
            activeHitboxes.Remove(hitbox);
        }

        public void DisableAll() {
            activeHitboxes.Clear();
        }

        public void Reset() {
            detectedEntities.Clear();
        }

        public void Detect(Vector3 position, Quaternion rotation, ICollection<IEntity> results) {
            foreach (IHitbox hitbox in activeHitboxes) {
                detectionBuffer.Clear();
                hitbox.Detect(position, rotation, detectionBuffer);

                foreach (IEntity entity in detectionBuffer) {
                    if (!detectedEntities.Contains(entity)) {
                        detectedEntities.Add(entity);
                        results.Add(entity);
                    }
                }
            }
        }
    }
}
