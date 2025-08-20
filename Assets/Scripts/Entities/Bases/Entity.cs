using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity<Estate> : FiniteStateMachine<Estate>, IEntity where Estate : Enum {

    [Header("Physics Components")]
    [SerializeField] protected Shadow shadow;
    [SerializeField] protected CharacterRigidbody character;

    [SerializeField] Collider2D col;
    [SerializeField] Collider2D platformDetectCol;

    protected Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] private float startingHealth;
    protected float health;

    [Header("Private Variables")]
    private bool onPlatform = false;
    private float shadowHeight = float.MinValue;
    private Weapon weapon;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        health = startingHealth;
    }

    private void OnValidate() {
        if (startingHealth < health)
            health = startingHealth;
    }

    public virtual float GetHealth() {
        return health;
    }

    public virtual Vector3 GetPosition() {
        return new Vector3(transform.position.x, transform.position.y, character.GetZ());
    }

    public virtual void TakeDamage(float damage) {
        health -= damage;
        if (health <= 0)
            Destroy(gameObject);
    }

    public virtual void HoldWeapon(Weapon weapon) {
        if (weapon == null)
            throw new NotSupportedException();
        weapon.SetHolder(this);
        this.weapon = weapon;
    }

    public void DropWeapon() {
        if (weapon != null) {
            weapon.SetHolder(null);
            weapon = null;
        }
    }

    public virtual void AddForce(Vector2 force, float zForce, ForceMode2D forceMode) {
        rb.AddForce(force, forceMode);
        character.AddForce(zForce, forceMode);
    }

    public void SetWeapon(Weapon weapon) {
        this.weapon = weapon;
    }

    public Weapon GetWeapon() {
        return weapon;
    }

    // Common use case is adding Transition to State
    public virtual void GetPushed(Vector2 impulse, float zImpulse) { 
        AddForce(impulse, zImpulse, ForceMode2D.Impulse);
    }

    private void SetPlatformLayers() {
        col.excludeLayers = 0;

        if (character.GetZ() >= 2)
            col.excludeLayers = 192;
        else if (character.GetZ() >= 1)
            col.excludeLayers = 64;

        platformDetectCol.excludeLayers = ~col.excludeLayers;

        // update shadow height wrt the current tallest platform height
        shadow.MovePosition(onPlatform ? shadowHeight : 0);
        shadow.Scale(1 / (1 + character.GetZ() - shadow.GetZ()));
    }

    private void ResetShadow() {
        shadowHeight = float.MinValue;
        onPlatform = false;
    }

    [Serializable]
    protected abstract class MoveState<T> : BaseState<Estate> where T : Entity<Estate> {

        public float targetSpeed = 5;
        public float accelRate = 6;
        public float decelRate = 6;

        protected T entity;
        protected Vector2 moveDir;
        protected Vector2 moveForce;

        public MoveState(Estate stateKey, T entity) : base(stateKey) {
            this.entity = entity;
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
        }

        public override void UpdateState() {
            entity.SetPlatformLayers();
        }

        public override void FixedUpdateState() {
            entity.ResetShadow();
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
        
        public override void OnTriggerStay2D(Collider2D other) {
            Platform platform = other.GetComponent<Platform>();

            if (platform != null) {
                entity.onPlatform = true;
                if (entity.shadowHeight < platform.Top())
                    entity.shadowHeight = platform.Top();
            }
        }
    }

    protected abstract class FloatState<T> : BaseState<Estate> where T : Entity<Estate> {

        protected T entity;
        protected Vector2 moveDir;
        protected Vector2 moveForce;

        public FloatState(Estate stateKey, T entity) : base(stateKey) {
            this.entity = entity;
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
        }

        public override void UpdateState() {
            entity.SetPlatformLayers();
        }

        public override void FixedUpdateState() {
            entity.ResetShadow();
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
        
        public override void OnTriggerStay2D(Collider2D other) {
            Platform platform = other.GetComponent<Platform>();

            if (platform != null) {
                entity.onPlatform = true;
                if (entity.shadowHeight < platform.Top())
                    entity.shadowHeight = platform.Top();
            }
        }
    }
}
