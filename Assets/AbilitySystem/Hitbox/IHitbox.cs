using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Hitbox {
    public interface IHitbox {
        /// <summary>
        /// Detects all entities intersecting this hitbox and adds them to the provided collection.
        /// </summary>
        /// <param name="position">The world-space origin from which the hitbox's local offset is applied.</param>
        /// <param name="rotation">The world-space rotation used to transform the hitbox's local offset and orientation.</param>
        /// <param name="results">The collection to which all detected entities are added. Existing contents are preserved.</param>
        void Detect(Vector3 position, Quaternion rotation, ICollection<IEntity> results);
    }
}
