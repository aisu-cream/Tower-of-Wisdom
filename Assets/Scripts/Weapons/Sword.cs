#nullable enable

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Sword : Weapon {

    [Min(0)] public float pushPower = 5;
    [SerializeField] GameObject? slashEffect;

    private Animator animator;
    private readonly int slashHash = Animator.StringToHash("Base Layer.Dummy Slash");

    void Awake() {
        animator = GetComponent<Animator>();
    }

    public override void Damage(Collider2D other) {
        if (other.TryGetComponent<IEntity>(out IEntity entity) && entity != holder) {
            entity.TakeDamage(damage);

            Vector2 pushImpulse = entity.GetPosition() - holder.GetPosition();
            pushImpulse = pushImpulse.normalized * pushPower;

            entity.GetPushed(pushImpulse, 0);
            holder.GetPushed(-pushImpulse / 1.5f, 0);
        }
    }

    public override void Wield() {
        animator.Play(slashHash, 0, 0);
    }

    public void CreateSlashEffect() {

        GameObject? effect = Instantiate(slashEffect, transform.position + transform.up * 0.75f, transform.rotation);
        effect?.transform.SetParent(transform);
    }

    public override void EndAction() {
        animator.Play("New State");
    }
}