using UnityEngine;

[RequireComponent(typeof(TargetingManager))]
public class ProjectileController : MonoBehaviour {
    [SerializeField] Ability ability;
    TargetingManager targetingManager;

    Ability owner;
    Vector3 velocity;

    public void Initialize(Ability owner, Vector3 velocity) {
        this.owner = owner;
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

        if (owner != null && owner.CanExecute(target)) {
            owner.Execute(transform.position, target);
            velocity = Vector3.zero;

            if (ability != null) {
                ability.Cast(owner.GetCaster(), targetingManager);
            }
        }
    }
}