using UnityEngine;

[RequireComponent(typeof(TargetingManager))]
public class ProjectileController : MonoBehaviour {
    [SerializeField] Ability ability;
    TargetingManager targetingManager;

    Ability owner;
    IAbilityTrigger trigger;
    Vector3 velocity;

    public void Initialize(Ability owner, IAbilityTrigger trigger, Vector3 velocity) {
        this.owner = owner;
        this.trigger = trigger;
        this.velocity = velocity;
    }

    void Awake() {
        targetingManager = GetComponent<TargetingManager>();
    }

    // Update is called once per frame
    void Update() {
        transform.Translate(velocity * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other) {
        IEntity target = other.GetComponent<IEntity>();

        trigger.TryExecute(new(ability.GetCaster(), target, target.GetPosition() - transform.position));
        velocity = Vector3.zero;

        ability?.Cast(owner.GetCaster(), targetingManager);
    }
}