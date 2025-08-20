using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Sword : Weapon {

    [Min(0)] public float pushPower = 5;

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

    public override float Wield() {
        animator.Play(slashHash, 0, 0);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return Time.time + stateInfo.length / stateInfo.speed;
    }

    public override void EndAction() {
        animator.Play("New State");
    }
}