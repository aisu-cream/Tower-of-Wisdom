using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    public class AbilityData : ScriptableObject {
        public string label;

        [SerializeField, Min(0)] float cooldown;

        [Header("Cast")]
        [SerializeField] List<IConstraint> preconditions;
        [SerializeField] List<IEffect> costs;

        [Header("Impact")]
        [SerializeField] List<IConstraint> targetConditions;
        [SerializeField] List<IEffect> effects;

        void OnEnable() {
            if (string.IsNullOrEmpty(label)) label = name;
        }

        public float Cooldown { get { return cooldown; } }
        public List<IConstraint> Preconditions { get { return preconditions; } }
        public List<IEffect> Costs { get { return costs; } }
        public List<IConstraint> TargetConditions { get { return targetConditions; } }
        public List<IEffect> Effects { get { return effects; } }
    }
}
