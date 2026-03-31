using System.Collections.Generic;
using UnityEngine;
using ImprovedTimers;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability")]
public class Ability : ScriptableObject {
    public string label;

    [Header("Visuals")]
    [SerializeField] AudioClip castSfx;
    [SerializeField] GameObject castVfx;
    [SerializeField] GameObject runningVfx;

    [Header("Timings")]
    [SerializeField, Min(0)] float castTime = 0.1f;
    [SerializeField, Min(0)] float cooldown = 1f;
    CountdownTimer castTimer;
    CountdownTimer cooldownTimer;

    [Header("Effects")]
    [SerializeReference] List<IPrecondition> preconditions = new();
    [SerializeReference] List<IEffectFactory> castEffects = new();
    [SerializeReference] List<ITargetConstraint> targetConstraints = new();
    [SerializeReference] List<IEffectFactory> executionEffects = new();

    [Header("Targeting")]
    [SerializeReference] TargetingStrategy targetingStrategy;
    TargetingManager targetingManager;

    IEntity caster;

    void OnEnable() {
        if (string.IsNullOrEmpty(label)) label = name;

        castTimer = new CountdownTimer(castTime);
        castTimer.OnTimerStop = Target;

        cooldownTimer = new CountdownTimer(cooldown);
    }

    public void Cast(IEntity caster, TargetingManager targetingManager) {
        if (!cooldownTimer.IsFinished) return;
        if (caster == null || targetingManager == null) return;

        foreach (var precondition in preconditions) {
            if (!precondition.Evaluate(caster))
                return;
        }

        this.targetingManager = targetingManager;
        this.caster = caster;
        
        castTimer.Reset(castTime);
        castTimer.Start();
    }

    public bool CanExecute(IEntity target) {
        foreach (var targetConstraint in targetConstraints) {
            if (!targetConstraint.Evaluate(caster, target))
                return false;
        }

        return true;
    }

    public void Execute(Vector3 source, IEntity target) {
        if (target == null) return;

        foreach (var effect in executionEffects) {
            var runtimeEffect = effect.Create();
            target.ApplyEffect(caster, source, runtimeEffect);
        }

        HandleVFX(target);
    }

    public IEntity GetCaster() {
        return caster;
    }

    void Target() {
        if (targetingManager == null) return;

        if (castSfx)
            HandleSFX();

        foreach (var effect in castEffects) {
            var runtimeEffect = effect.Create();
            runtimeEffect.Apply(caster, caster.GetPosition(), caster);
        }

        targetingStrategy?.Start(this, targetingManager);

        cooldownTimer.Reset(cooldown);
        cooldownTimer.Start();
    }

    void HandleVFX(IEntity target) {
        var targetMb = target as MonoBehaviour;
        if (targetMb == null) return;

        if (castVfx)
            Instantiate(castVfx, targetMb.transform.position, Quaternion.identity);

        if (runningVfx) {
            var runningVfxInstance = Instantiate(runningVfx, targetMb.transform);
            runningVfxInstance.transform.Translate(Vector3.up, Space.World);
            Destroy(runningVfxInstance, 3f);
        }
    }

    void HandleSFX() {
        GameObject o = new GameObject("Ability Audio");
        o.transform.position = targetingManager.transform.position;

        AudioSource src = o.AddComponent<AudioSource>();
        src.clip = castSfx;
        src.spatialBlend = 1f;

        src.minDistance = 20;
        src.maxDistance = 100;

        src.Play();
        Destroy(o, src.clip.length);
    }
}