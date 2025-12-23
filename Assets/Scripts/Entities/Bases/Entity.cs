using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public abstract class Entity<Estate> : FiniteStateMachine<Estate>, IEntity where Estate : Enum {

    [Header("Stats")]
    [SerializeField] private float initialHealth;
    protected float health;

    [Header("Environmental")]
    [Min(0)] public float gravity = 49f;
    [Min(0)] public float maxFallSpeed = 30f;
    [Range(0, 90)] public float maxSlopeAngle = 45f;

    [Header("Physics")]
    protected Rigidbody rb;
    protected CapsuleCollider col;

    [Header("Movement")]
    private Vector3 moveDir;
    private Vector3 facingDir;
    private Vector3 gravDir = Vector3.down;
    private RaycastHit surfaceHit;
    private float ignoreGroundUntil;
    protected bool isGrounded;

    [Header("Combat")]
    protected Weapon weapon;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        health = initialHealth;
        moveDir = Vector3.zero;
        facingDir = Vector3.forward;
        ignoreGroundUntil = -1;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        // check ground
        isGrounded = false;

        if (Time.time >= ignoreGroundUntil && OnStandableSurface())
            isGrounded = true;

        if (!isGrounded) {
            // gravity
            Vector3 gravAccel = gravity * gravDir;
            AddForce(gravAccel, ForceMode.Acceleration);

            // drag
            float dragConst = gravity * rb.mass / (maxFallSpeed * maxFallSpeed);
            AddForce(GetVelocity().y * Mathf.Abs(GetVelocity().y) * dragConst * gravDir, ForceMode.Force);
        }
    }

    protected bool OnStandableSurface() {
        float skinEps = 0.001f;
        return (Physics.SphereCast(transform.position + col.radius * Vector3.up, col.radius - 0.05f, Vector3.down, out surfaceHit, 0.25f)) &&
               (Vector3.Dot(GetVelocity(), surfaceHit.normal) <= skinEps && Vector3.Angle(surfaceHit.normal, Vector3.up) <= maxSlopeAngle + skinEps);
    }

    protected void DisableGroundCheckFor(float duration) {
        ignoreGroundUntil = Time.time + duration;
    }

    void OnValidate() {
        if (initialHealth < health)
            health = initialHealth;
    }

    public virtual float GetHealth() {
        return health;
    }

    public virtual Vector3 GetPosition() {
        return transform.position;
    }

    public virtual Vector3 GetHorizontalPosition() {
        return new Vector3(transform.position.x, 0, transform.position.z);
    }

    public virtual Vector3 GetVelocity() {
        return rb.linearVelocity;
    }

    public virtual Vector3 GetHorizontalVelocity() {
        return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    public virtual Vector3 GetDesiredDirection() {
        return moveDir;
    }

    public virtual Vector3 GetFacingDirection() {
        return facingDir;
    }

    public virtual bool IsGrounded() {
        return isGrounded;
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

    public Weapon GetWeapon() {
        return weapon;
    }

    public virtual void AddForce(Vector3 force, ForceMode forceMode) {
        rb.AddForce(force, forceMode);
    }

    // Common use case is adding Transition to State
    public virtual void GetPushed(Vector3 impulse) { 
        AddForce(impulse, ForceMode.Impulse);
    }

    [Serializable]
    protected abstract class MoveState<T> : BaseState<Estate> where T : Entity<Estate> {

        public float targetSpeed = 5;
        public float accelRate = 6;
        public float decelRate = 6;

        protected T entity;

        public MoveState(Estate stateKey, T entity) : base(stateKey) {
            this.entity = entity;
        }

        // accelerate player towards the target velocity
        protected void Move(Vector2 dir, float lerpAmount) {
            // rotate the dir vector to xz plane
            entity.moveDir = new Vector3(dir.x, 0, dir.y);

            // change the facing dir
            if (entity.moveDir.magnitude != 0)
                entity.facingDir = entity.moveDir;

            entity.moveDir.Normalize();

            Vector3 angledDir;
            Vector3 currentVel;

            // angle the move direction based on the angle of the currently standing surface
            if (entity.isGrounded) {
                angledDir = Vector3.ProjectOnPlane(entity.moveDir, entity.surfaceHit.normal).normalized;
                currentVel = Vector3.ProjectOnPlane(entity.GetVelocity(), entity.surfaceHit.normal);
            }
            else {
                angledDir = entity.moveDir;
                currentVel = Vector3.ProjectOnPlane(entity.GetVelocity(), Vector3.up);
            }

            // calculate the force needed
            Vector3 targetVel = Vector3.Lerp(currentVel, angledDir * targetSpeed, lerpAmount);
            Vector3 moveForce = targetVel - currentVel;
            moveForce *= Vector3.Dot(angledDir, currentVel) > 0 ? accelRate : decelRate;

            // apply the force
            entity.rb.AddForce(moveForce, ForceMode.Force);
        }

        protected void Move(float x, float z, float lerpAmount) {
            Move(new Vector2(x, z), lerpAmount);
        }
    }

    protected abstract class FloatState<T> : BaseState<Estate> where T : Entity<Estate> {

        protected T entity;

        public FloatState(Estate stateKey, T entity) : base(stateKey) {
            this.entity = entity;
        }

        /** 
         * maintain the velocity if current velocity and the desired direction to move are the same. Otherwise, decrease the velocity towards the self movable speed limit
         * in general, use the entity's walk speed, accel rate, and decel rate as the three corresponding inputs
         */
        protected void Float(Vector2 dir, float selfMovableSpeedLimit, float accelRate, float decelRate, float lerpAmount) {
            // rotate the dir vector to xz plane
            entity.moveDir = new Vector3(dir.x, 0, dir.y);
            entity.moveDir.Normalize();

            // change the facing dir
            if (entity.moveDir.magnitude != 0)
                entity.facingDir = entity.moveDir;

            // check current velocity and direction of input
            Vector3 currVelocity = entity.GetHorizontalVelocity();
            float currSpeed = currVelocity.magnitude;
            bool movingInIdenticalDir = Vector3.Dot(currVelocity, entity.moveDir) > 0;

            // calculate the target velocity
            Vector3 targetVel = entity.moveDir;
            targetVel *= (currSpeed <= selfMovableSpeedLimit || !movingInIdenticalDir) ? selfMovableSpeedLimit : CalculateElipticSpeed(Vector3.Angle(currVelocity, entity.moveDir), currSpeed, selfMovableSpeedLimit);
            targetVel = Vector3.Lerp(currVelocity, targetVel, lerpAmount);

            // calculate the force needed to reach the desired velocity
            Vector3 moveForce = targetVel - currVelocity;
            moveForce *= movingInIdenticalDir ? accelRate : decelRate;

            // apply the force
            entity.rb.AddForce(moveForce, ForceMode.Force);
        }

        protected void Float(float x, float z, float selfMovableSpeedLimit, float accelRate, float decelRate, float lerpAmount) {
            Float(new Vector2(x, z), selfMovableSpeedLimit, accelRate, decelRate, lerpAmount);
        }

        private float CalculateElipticSpeed(float deltaAngle, float currSpeed, float selfMovableSpeedLimit) {
            float walkSpeed = selfMovableSpeedLimit;
            return walkSpeed * currSpeed / Mathf.Sqrt(Mathf.Pow(currSpeed * Mathf.Sin(deltaAngle), 2) + Mathf.Pow(walkSpeed * Mathf.Cos(deltaAngle), 2));
        }
    }
}
