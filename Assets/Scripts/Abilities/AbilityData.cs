using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData")]
public class AbilityData : ScriptableObject {

    public string label;

    // Need to work on vfx/sfx pooling

    [SerializeField, Min(0)] float castDelay = 0.1f;
    [SerializeField, Min(0)] float cooldown = 1f;

    [Header("Cast")]
    [SerializeField] ParticleSystem castVfx;
    [SerializeField] AudioClip castSfx;
    [SerializeReference] List<IConstraint> preconditions;
    [SerializeReference] List<IEffect> costs;

    [Header("Active")]
    [SerializeField] ParticleSystem activeVfx;
    [SerializeField] AudioClip activeSfx;

    [Header("Impact")]
    [SerializeField] ParticleSystem impactVfx;
    [SerializeField] AudioClip impactSfx;
    [SerializeReference] List<IConstraint> targetConditions;
    [SerializeReference] List<IEffect> effects;
    
    void OnEnable() {
        if (string.IsNullOrEmpty(label)) label = name;
    }

    public float CastDelay { get { return castDelay; } }
    public float Cooldown { get { return cooldown; } }
    public ParticleSystem CastVfx { get { return castVfx; } }
    public AudioClip CastSfx { get { return castSfx; } }
    public List<IConstraint> Preconditions { get { return preconditions; } }
    public List<IEffect> Costs { get { return costs; } }
    public ParticleSystem ActiveVfx { get { return activeVfx; } }
    public AudioClip ActiveSfx { get { return activeSfx; } }
    public ParticleSystem ImpactVfx { get { return impactVfx; } }
    public AudioClip ImpactSfx { get { return impactSfx; } }
    public List<IConstraint> TargetConditions {  get { return targetConditions; } }
    public List<IEffect> Effects { get { return effects; } }
}
