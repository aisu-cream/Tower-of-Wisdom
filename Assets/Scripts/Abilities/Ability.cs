using UnityEngine;
using AbilitySystem;
using ImprovedTimers;
using System.Collections.Generic;

/**
 * The most basic type of ability where you consume cost only once and apply single set of predefined effects to target(s).
 * If more complex abilities are needed, such as an ability that continuously consume cost or an ability that applies different effects based on the target, 
 * use different IAbility implementations.
 * To use this class, one must supply abilityData and abilityBehaviour.
 */
[System.Serializable]
public abstract class Ability : IAbility {

    [SerializeField] AbilityData abilityData;

    TargetingManager manager;
    Animator animator;
    CountdownTimer cooldownTimer;

    AbilityState currentState;
    IEntity caster;

    public Ability() {
        cooldownTimer = new CountdownTimer(abilityData.Cooldown);
        currentState = AbilityState.inactive;
        manager.OnTargetFound += OnTargetFound;
    }

    public void Awake(IEntity caster, TargetingManager manager, Animator animator) {
        this.caster = caster;
        this.manager = manager;
        this.animator = animator;
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

    public virtual void Update() {
        if (currentState == AbilityState.startup) {
            if (!CanCast())
                currentState = AbilityState.inactive;
        }
    }

    public virtual bool CanCast() {
        if (caster == null || manager == null || currentState != AbilityState.inactive || !cooldownTimer.IsFinished)
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
        cooldownTimer.Reset(abilityData.Cooldown);
        cooldownTimer.Start();

        PlayAbilityAnimation();
    }

    protected abstract void PlayAbilityAnimation();

    protected virtual void OnTargetFound(List<IEntity> candidateTargets) {
        if (currentState == AbilityState.startup)
            SpendCost();

        currentState = AbilityState.active;
        List<IEntity> targets = GetValidTargets(candidateTargets);

        foreach (IEntity target in targets)
            ApplyEffects(target);
    }

    protected List<IEntity> GetValidTargets(List<IEntity> candidateTargets) {
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

    protected void ApplyEffects(IEntity target) {
        foreach (IEffect effect in abilityData.Effects) {
            IEffect effectInstance = effect.Clone();
            effectInstance.Apply(target);
            target.AlertEffect(effectInstance);
        }
    }

    protected void SpendCost() {
        foreach (IEffect cost in abilityData.Costs) {
            IEffect costInstance = cost.Clone();
            costInstance.Apply(caster);
        }
    }
}