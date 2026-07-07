using System;
using UnityEngine;

namespace AbilitySystem {
    // maybe merge with Entity?
    public class AnimationEventHandler : MonoBehaviour {

        public event Action OnAttackFrame;
        public event Action OnAnimationFinished;

        void AttackFrameTrigger() => OnAttackFrame?.Invoke();
        void AnimationFinishTrigger() => OnAnimationFinished?.Invoke();
    }
}
