using ImprovedTimers;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    /**
     * The most basic type of ability where you consume cost only once and apply single set of predefined effects to target(s).
     * If more complex abilities are needed, such as an ability that continuously consume cost or an ability that applies different effects based on the target, 
     * use different IAbility implementations.
     * To use this class, one must supply abilityData and abilityBehaviour.
     */
    [System.Serializable]
    public abstract class Ability : MonoBehaviour, IAbility {

        // merge these two (data can have targeting info, such as hitbox) - really tho?
        [SerializeField] AbilityData abilityData;
        [SerializeReference] TargetingManager manager;

        AnimationEventHandler handler;
        Animator animator;

        CountdownTimer cooldownTimer;
        AbilityState currentState;
        IEntity caster;

        int animationTriggerHash;

        public Ability(string animationTrigger) {
            animationTriggerHash = Animator.StringToHash(animationTrigger);
            currentState = AbilityState.inactive;
        }

        void Awake() {
            caster = GetComponent<IEntity>();
            handler = GetComponent<AnimationEventHandler>();
            animator = GetComponent<Animator>();
        }

        void Start() {
            cooldownTimer = new CountdownTimer(abilityData.Cooldown);
        }

        void OnDisable() {
            EndAbility();
        }

        void Update() {
            Debug.Log(currentState);
            if (currentState == AbilityState.startup && !CanEnterActiveState())
                currentState = AbilityState.inactive;
        }

        public float GetCooldownRemaining() {
            return cooldownTimer.CurrentTime;
        }

        public float GetCooldownDuration() {
            return abilityData.Cooldown;
        }

        public AbilityState GetState() {
            return currentState;
        }

        public IEntity GetCaster() {
            return caster;
        }

        public bool CanCast() {
            if (GetState() != AbilityState.inactive || !cooldownTimer.IsFinished)
                return false;

            return CanEnterActiveState();
        }

        bool CanEnterActiveState() {
            if (caster == null || handler == null)
                return false;

            foreach (IConstraint precondition in abilityData.Preconditions) {
                if (!precondition.Evaluate(caster))
                    return false;
            }

            return true;
        }

        public void TryCast() {
            if (!CanCast())
                return;

            currentState = AbilityState.startup;
            animator.SetTrigger(animationTriggerHash);

            cooldownTimer.Reset(abilityData.Cooldown);
            cooldownTimer.Start();

            handler.OnAttackFrame += OnAttackFrame;
            handler.OnAnimationFinished += EndAbility;
        }

        public void EndAbility() {
            animator.ResetTrigger(animationTriggerHash);
            currentState = AbilityState.inactive;

            handler.OnAttackFrame -= OnAttackFrame;
            handler.OnAnimationFinished -= EndAbility;
        }

        void OnAttackFrame() {
            if (currentState == AbilityState.startup)
                SpendCost();

            currentState = AbilityState.active;
            List<IEntity> targets = EvaluateValidTargets(manager.FindTargets(caster.GetPosition()));

            foreach (IEntity target in targets)
                ApplyEffects(target);
        }

        List<IEntity> EvaluateValidTargets(List<IEntity> candidateTargets) {
            List<IEntity> targets = new List<IEntity>();

            foreach (IEntity candidate in candidateTargets) {
                if (IsValidCandidate(candidate))
                    targets.Add(candidate);
            }

            return targets;
        }

        bool IsValidCandidate(IEntity candidate) {
            foreach (IConstraint targetCondition in abilityData.TargetConditions) {
                if (!targetCondition.Evaluate(candidate))
                    return false;
            }

            return true;
        }

        void ApplyEffects(IEntity target) {
            foreach (IEffect effect in abilityData.Effects) {
                IEffect effectInstance = effect.Clone();
                effectInstance.Apply(target);
                target.AlertEffect(effectInstance);
            }
        }

        void SpendCost() {
            foreach (IEffect cost in abilityData.Costs) {
                IEffect costInstance = cost.Clone();
                costInstance.Apply(caster);
            }
        }
    }
}