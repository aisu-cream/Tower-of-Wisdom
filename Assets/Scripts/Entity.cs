using System;
using UnityEngine;

public abstract class Entity<Estate> : FiniteStateMachine<Estate> where Estate : Enum {

    [Header("Physics Components")]
    [SerializeField] Shadow shadow;
    [SerializeField] CharacterRigidbody character;

    [SerializeField] Collider2D col;
    [SerializeField] Collider2D platformDetectCol;

    private Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] private float startingHealth;
    protected float health;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        health = startingHealth;
    }

    public float GetHealth() {
        return health;
    }

    public abstract void GetPushed(Vector2 impulse);

    [Serializable]
    protected abstract class Movable : BaseState<Estate> {

        public float targetSpeed = 5;
        public float accelRate = 6;
        public float decelRate = 6;

        protected Vector2 moveDir;
        protected Vector2 moveForce;

        protected Entity<Estate> entity;

        private static bool onPlatform = false;
        private static float shadowHeight = float.MinValue;

        public Movable(Estate stateKey, Entity<Estate> entity) : base(stateKey) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
            this.entity = entity;
        }

        public override void UpdateState() {
            entity.col.excludeLayers = 0;

            if (entity.character.GetZ() >= 1)
                entity.col.excludeLayers = 64;

            entity.platformDetectCol.excludeLayers = ~entity.col.excludeLayers;

            // update shadow height wrt the current tallest platform height
            entity.shadow.MovePosition(onPlatform ? shadowHeight : 0);
        }

        public override void FixedUpdateState() {
            shadowHeight = float.MinValue;
            onPlatform = false;
        }

        public override void OnTriggerStay2D(Collider2D other) {
            Platform platform = other.GetComponent<Platform>();

            if (platform != null) {
                onPlatform = true;
                if (shadowHeight < platform.Top())
                    shadowHeight = platform.Top();
            }
        }

        protected void Move(Vector2 dir, float lerpAmount) {
            // accelerate player towards the target velocity
            moveDir = dir;
            moveDir.Normalize();

            Vector2 targetVel = Vector2.Lerp(entity.rb.linearVelocity, moveDir * targetSpeed, lerpAmount);

            moveForce = targetVel - entity.rb.linearVelocity;
            moveForce *= Vector2.Dot(moveDir, entity.rb.linearVelocity) > 0 ? accelRate : decelRate;

            entity.rb.AddForce(moveForce, ForceMode2D.Force);
        }

        protected void Move(float x, float y, float lerpAmount) {
            Move(new Vector2(x, y), lerpAmount);
        }
    }

    protected abstract class KnockbackState : Movable {
        public KnockbackState(Estate stateKey, Entity<Estate> entity) : base(stateKey, entity) { }
    }
}
