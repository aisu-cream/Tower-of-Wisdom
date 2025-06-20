using System;
using UnityEngine;

public abstract class Entity<Estate> : FiniteStateMachine<Estate> where Estate : Enum {

    [Header("Physics Components")]
    [SerializeField] protected Shadow shadow;
    [SerializeField] protected CharacterRigidbody character;

    [SerializeField] Collider2D col;
    [SerializeField] Collider2D platformDetectCol;

    protected Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] private float startingHealth;
    protected float health;

    private bool onPlatform = false;
    private float shadowHeight = float.MinValue;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        health = startingHealth;
    }

    private void OnValidate() {
        if (startingHealth < health)
            health = startingHealth;
    }

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float damage) {
        health -= damage;
        // die
    }

    public virtual void GetPushed(Vector2 impulse) { }

    protected abstract class StandableOnPlatform : BaseState<Estate> {

        protected Entity<Estate> entity;

        public StandableOnPlatform(Estate stateKey, Entity<Estate> entity) : base(stateKey) {
            this.entity = entity;
        }

        public override void UpdateState() {
            entity.col.excludeLayers = 0;

            if (entity.character.GetZ() >= 2)
                entity.col.excludeLayers = 192;
            else if (entity.character.GetZ() >= 1)
                entity.col.excludeLayers = 64;

            entity.platformDetectCol.excludeLayers = ~entity.col.excludeLayers;

            // update shadow height wrt the current tallest platform height
            entity.shadow.MovePosition(entity.onPlatform ? entity.shadowHeight : 0);
            entity.shadow.Scale(1 / (1 + entity.character.GetZ() - entity.shadow.GetZ()));
        }

        public override void FixedUpdateState() {
            entity.shadowHeight = float.MinValue;
            entity.onPlatform = false;
        }

        public override void OnTriggerStay2D(Collider2D other) {
            Platform platform = other.GetComponent<Platform>();

            if (platform != null) {
                entity.onPlatform = true;
                if (entity.shadowHeight < platform.Top())
                    entity.shadowHeight = platform.Top();
            }
        }
    }

    [Serializable]
    protected abstract class MoveState : StandableOnPlatform {

        public float targetSpeed = 5;
        public float accelRate = 6;
        public float decelRate = 6;

        protected Vector2 moveDir;
        protected Vector2 moveForce;

        public MoveState(Estate stateKey, Entity<Estate> entity) : base(stateKey, entity) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
        }

        // accelerate player towards the target velocity
        protected void Move(Vector2 dir, float lerpAmount) {
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

    protected abstract class FloatState : StandableOnPlatform {
        
        protected Vector2 moveDir;
        protected Vector2 moveForce;

        public FloatState(Estate stateKey, Entity<Estate> entity) : base(stateKey, entity) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
        }

        /** 
         * maintain the velocity if current velocity and the desired direction to move are the same. Otherwise, decrease the velocity towards the self movable speed limit
         * in general, use the entity's walk speed, accel rate, and decel rate as the three corresponding inputs
         */
        protected void Float(Vector2 dir, float selfMovableSpeedLimit, float accelRate, float decelRate, float lerpAmount) {
            moveDir = dir;
            moveDir.Normalize();

            // calculate the desired velocity to move
            Vector2 targetVel = moveDir;
            bool movingInIdenticalDir = Vector2.Dot(entity.rb.linearVelocity, moveDir) > 0;

            if (entity.rb.linearVelocity.magnitude <= selfMovableSpeedLimit || !movingInIdenticalDir)
                targetVel *= selfMovableSpeedLimit;
            else
                targetVel *= CalculateElipticSpeed(Vector2.Angle(entity.rb.linearVelocity, moveDir), selfMovableSpeedLimit);

            targetVel = Vector2.Lerp(entity.rb.linearVelocity, targetVel, lerpAmount);

            // calculate the force needed to reach the desired velocity
            moveForce = targetVel - entity.rb.linearVelocity;

            if (movingInIdenticalDir)
                moveForce *= accelRate;
            else
                moveForce *= decelRate;

            entity.rb.AddForce(moveForce, ForceMode2D.Force);
        }

        protected void Float(float x, float y, float selfMovableSpeedLimit, float accelRate, float decelRate, float lerpAmount) {
            Float(new Vector2(x, y), selfMovableSpeedLimit, accelRate, decelRate, lerpAmount);
        }

        private float CalculateElipticSpeed(float deltaAngle, float selfMovableSpeedLimit) {
            float walkSpeed = selfMovableSpeedLimit;
            float currSpeed = entity.rb.linearVelocity.magnitude;
            return walkSpeed * currSpeed / Mathf.Sqrt(Mathf.Pow(currSpeed * Mathf.Sin(deltaAngle), 2) + Mathf.Pow(walkSpeed * Mathf.Cos(deltaAngle), 2));
        }
    }
}
