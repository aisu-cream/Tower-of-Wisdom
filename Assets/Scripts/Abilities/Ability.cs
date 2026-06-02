using UnityEngine;
using ImprovedTimers;

/**
 * The most basic type of ability where you consume cost only once and apply single set of predefined effects to target(s).
 * If more complex abilities are needed, such as an ability that continuously consume cost or an ability that applies different effects based on the target, 
 * use different IAbility implementations.
 * To use this class, one must supply abilityData and abilityBehaviour.
 */
[System.Serializable]
public class Ability : IAbility {

    [SerializeField] AbilityData abilityData;
    [SerializeReference] AbilityBehaviour behaviour;

    TargetingManager targetingManager;

    CountdownTimer castTimer;
    CountdownTimer cooldownTimer;

    IAbilityTrigger trigger;

    IEntity caster;

    public IEntity GetCaster() {
        return caster;
    }

    public void OverrideCaster(IEntity entity) {
        caster = entity;
    }

    public void Cast(IEntity caster, TargetingManager targetingManager) {
        castTimer ??= new(abilityData.CastDelay) { OnTimerStop = Target };
        cooldownTimer ??= new(abilityData.Cooldown);

        trigger ??= new AbilityTrigger(this);

        if (!castTimer.IsFinished || !cooldownTimer.IsFinished) return;
        if (caster == null || targetingManager == null) return;

        foreach (var precondition in abilityData.Preconditions) {
            if (!precondition.Evaluate(caster))
                return;
        }

        this.targetingManager = targetingManager;
        this.caster = caster;
        
        castTimer.Reset(abilityData.CastDelay);
        castTimer.Start();
    }

    void Target() {
        if (targetingManager == null || caster == null || !caster.IsAlive()) 
            return;

        ParticleSystem castVfx = abilityData.CastVfx;
        AudioClip castSfx = abilityData.CastSfx;

        if (castVfx) {
            var castInstance = Object.Instantiate(castVfx, targetingManager.transform.position, Quaternion.identity);
            Object.Destroy(castInstance, castInstance.main.duration);
        }

        if (castSfx) {
            GameObject o = new GameObject("Ability Audio");
            o.transform.position = targetingManager.transform.position;

            AudioSource src = o.AddComponent<AudioSource>();
            src.clip = castSfx;
            src.spatialBlend = 1f;

            src.minDistance = 20;
            src.maxDistance = 100;

            src.Play();
            Object.Destroy(o, src.clip.length);
        }

        behaviour?.Start(this, trigger, targetingManager);

        cooldownTimer.Reset(abilityData.Cooldown);
        cooldownTimer.Start();
    }

    void Execute(EffectContext context) {
        if (context.Target == null) return;

        foreach (var targetCondition in abilityData.TargetConditions) {
            if (!targetCondition.Evaluate(context.Target))
                return;
        }

        foreach (var effect in abilityData.Effects) {
            var runtimeEffect = effect.Clone();
            runtimeEffect.Apply(context);
            context.Target.AlertEffect(runtimeEffect);
        }

        context.Target.AlertThreat(caster);

        ParticleSystem impactVfx = abilityData.ImpactVfx;
        AudioClip impactSfx = abilityData.ImpactSfx;

        if (impactVfx) {
            var castInstance = Object.Instantiate(impactVfx, context.Target.GetPosition(), Quaternion.identity);
            Object.Destroy(castInstance, castInstance.main.duration);
        }

        if (impactSfx) {
            GameObject o = new GameObject("Ability Audio");
            o.transform.position = targetingManager.transform.position;

            AudioSource src = o.AddComponent<AudioSource>();
            src.clip = impactSfx;
            src.spatialBlend = 1f;

            src.minDistance = 20;
            src.maxDistance = 100;

            src.Play();
            Object.Destroy(o, src.clip.length);
        }
    }

    /**
     * A Trigger that allows calling the ability's secure functionalities: consume cost and execute.
     * Consume cost is expected to be called at the start, while the execute function can be called whenever.
     */
    class AbilityTrigger : IAbilityTrigger {
        Ability ability;

        public AbilityTrigger(Ability ability) {
            this.ability = ability;
        }

        public bool CanConsumeCost() {
            foreach (var precondition in ability.abilityData.Preconditions) {
                if (!precondition.Evaluate(ability.caster))
                    return false;
            }

            return true;
        }

        public void ConsumeCost(EffectContext context) {
            foreach (IEffect cost in ability.abilityData.Costs) {
                IEffect costInstance = cost.Clone();
                costInstance.Apply(context);
                ability.GetCaster().AlertEffect(costInstance);
            }
        }

        public void TryExecute(EffectContext context) {
            ability.Execute(context);
        }
    }
}