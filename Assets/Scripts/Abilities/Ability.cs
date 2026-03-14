using System;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability")]
public class Ability : ScriptableObject {
    public string label;

    [SerializeField] AudioClip castSfx;
    [SerializeField] GameObject castVfx;
    [SerializeField] GameObject runningVfx;

    [Header("Effects")]
    [SerializeReference] List<IEffectFactory> effects = new();

    [Header("Targeting")]
    [SerializeReference] TargetingStrategy targetingStrategy;

    void OnEnable() {
        if (string.IsNullOrEmpty(label)) label = name;
    }

    public void Target(TargetingManager targetingManager) {
        if (castSfx)
            HandleSFX(targetingManager);

        if (targetingStrategy != null)
            targetingStrategy.Start(this, targetingManager);
    }

    public void Execute(TargetingManager caster, IAffectable target) {
        HandleVFX(target);

        foreach (var effect in effects) {
            var runtimeEffect = effect.Create();
            target.ApplyEffect(caster, runtimeEffect);
        }
    }

    void HandleVFX(IAffectable target) {
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

    void HandleSFX(TargetingManager targetingManager) {
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