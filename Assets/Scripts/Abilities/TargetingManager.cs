using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    public abstract class TargetingManager : MonoBehaviour {

        public event Action<List<IEntity>> OnTargetFound;
        public void FindTargets() => OnTargetFound?.Invoke(QueryTargets());
        protected abstract List<IEntity> QueryTargets();
    }
}
