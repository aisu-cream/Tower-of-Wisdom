using UnityEngine;

[System.Serializable]
public struct MovementSettings {
    [Min(0)] public float targetSpeed;
    [Min(0)] public float accel;
    [Min(0)] public float decel;

    public MovementSettings(float targetSpeed, float accel, float decel) {
        this.targetSpeed = targetSpeed;
        this.accel = accel;
        this.decel = decel;
    }
}

public class EntityController : MonoBehaviour {

    #region Fields
    Rigidbody rb;
    CapsuleCollider col;
    EntityMover mover;

    public bool isGrounded { get; private set; }
    float groundCheckIgnoreTimer;
    [SerializeField] LayerMask groundLayer = 1 << 3;
    Vector3 surfaceNormal;
    Vector3 surfaceVelocity;

    float speedModifier = 1f;
    float lerpModifier = 1f;

    [Min(0)] public float gravity = 49f;
    [Min(0)] public float maxFallSpeed = 30f;
    [Range(0, 90)] public float slopeLimit = 45f;
    [Min(0)] public float stickSpeed = 5;
    [Min(0)] public float stepOffset = 0.3f;
    float stickIgnoreCounter;
    #endregion

    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        mover = new(rb);

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate() {
        isGrounded = CheckGround();

        // apply gravity
        if (!isGrounded) {
            Vector3 vel = rb.linearVelocity;
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);                         // gravity
            rb.AddForce(DragConst() * vel.y * Mathf.Abs(vel.y) * Vector3.down, ForceMode.Force); // drag
        }

        // apply ground movement correction
        else {
            // check front for wall
            RaycastHit wallHit;
            Vector3 direction = GetIntendedMoveDirection();
            float check = 0.25f;

            if (Physics.SphereCast(rb.position + Vector3.up * col.radius, col.radius - 0.05f, direction, out wallHit, Mathf.Infinity, groundLayer, QueryTriggerInteraction.Ignore) 
                && wallHit.distance <= check) {
                // slope edge case adjustment
                float skin = 0.001f;

                if (Vector3.Angle(wallHit.normal, Vector3.up) <= slopeLimit + skin)
                    surfaceNormal = wallHit.normal;

                // step stair
                else {
                    RaycastHit stepHit;
                    Vector3 stepPoint = wallHit.point;
                    stepPoint.y = rb.position.y;
                    stepPoint += Vector3.up * stepOffset + direction * 0.01f;

                    if (Physics.Raycast(stepPoint, Vector3.down, out stepHit, stepOffset, groundLayer, QueryTriggerInteraction.Ignore)) {
                        float normalAngle = Vector3.Angle(stepHit.normal, Vector3.up);

                        if (Mathf.Abs(normalAngle) <= slopeLimit + skin) {
                            Vector3 pos = rb.position;
                            pos.y = stepHit.point.y;
                            rb.MovePosition(pos);

                            DisableGroundCheckFor(0.06f);
                            DisableStickingFor(0.06f);
                        }
                    }
                }
            }

            if (stickIgnoreCounter > 0)
                stickIgnoreCounter -= Time.fixedDeltaTime;
            else
                ApplyStickForce();
        }
    }

    bool CheckGround() {
        if (groundCheckIgnoreTimer > 0) {
            groundCheckIgnoreTimer -= Time.fixedDeltaTime;
            return this.isGrounded;
        }

        bool isGrounded = false;
        surfaceNormal = Vector3.up;
        surfaceVelocity = Vector3.zero;

        Vector3 vel = rb.linearVelocity;
        RaycastHit groundHit;
        
        if (Physics.SphereCast(rb.position + (col.radius + 0.1f) * Vector3.up, col.radius, Vector3.down, out groundHit, Mathf.Infinity, groundLayer, QueryTriggerInteraction.Ignore)) {
            Vector3 tempSurfaceNormal = groundHit.normal;
            float surfaceAngle = Vector3.Angle(tempSurfaceNormal, Vector3.up);
            float skin = 0.001f;

            if (groundHit.distance <= 0.2f && surfaceAngle <= slopeLimit + skin) {
                if (groundHit.rigidbody != null)
                    surfaceVelocity = groundHit.rigidbody.linearVelocity;

                if (Vector3.Dot(vel - surfaceVelocity, tempSurfaceNormal) <= skin) {
                    isGrounded = true;
                    surfaceNormal = tempSurfaceNormal;
                }
            }
        }

        // extra ground check for small drops
        if (!isGrounded) {
            Vector3 origin = rb.position + Vector3.up * 0.1f;
            RaycastHit extraHit;

            if (Physics.Raycast(origin, Vector3.down, out extraHit, Mathf.Infinity, groundLayer, QueryTriggerInteraction.Ignore)) {
                Vector3 tempSurfaceNormal = extraHit.normal;
                float surfaceAngle = Vector3.Angle(tempSurfaceNormal, Vector3.up);
                float skin = 0.001f;

                if (extraHit.distance <= 0.2f && surfaceAngle <= slopeLimit + skin) {
                    if (groundHit.rigidbody != null)
                        surfaceVelocity = groundHit.rigidbody.linearVelocity;

                    if (Vector3.Dot(vel - surfaceVelocity, tempSurfaceNormal) <= skin) {
                        isGrounded = true;
                        surfaceNormal = tempSurfaceNormal;
                    }
                }
            }
        }

        return isGrounded;
    }

    void ApplyStickForce() {
        float vertVel = Vector3.Dot(rb.linearVelocity - surfaceVelocity, surfaceNormal);
        if (vertVel > -stickSpeed)
            rb.AddForce(-(stickSpeed + vertVel) * surfaceNormal, ForceMode.VelocityChange);
    }

    Vector3 GetIntendedMoveDirection() {
        Vector3 direction = mover.GetMoveDirection();

        if (Vector3.Dot(direction, rb.linearVelocity - surfaceVelocity) < -0.001f)
            direction = Vector3.zero;

        return direction;
    }

    void DisableGroundCheckFor(float time) => groundCheckIgnoreTimer = time;

    void DisableStickingFor(float time) => stickIgnoreCounter = time;

    float DragConst() => gravity * rb.mass / (maxFallSpeed * maxFallSpeed);

    public void Move(Vector3 dir, float targetSpeed, float accelRate, float decelRate, float lerpAmount) 
        => mover.Move(dir, targetSpeed, accelRate, decelRate, lerpAmount, (isGrounded ? surfaceNormal : Vector3.up));

    public void Float(Vector3 dir, float targetSpeed, float accelRate, float decelRate, float lerpAmount) 
        => mover.Float(dir, targetSpeed, accelRate, decelRate, lerpAmount);

    public void Jump(float jumpStrength) {
        float jumpImpulse = jumpStrength;
        float vertVel = (rb.linearVelocity - surfaceVelocity).y;

        if (vertVel < 0)
            jumpImpulse -= vertVel;

        mover.Jump(jumpImpulse);
        isGrounded = false;
        DisableGroundCheckFor(0.05f);
    }

    public Vector3 GetLookDirection() => mover.GetLookDirection();

    public Vector3 GetVelocity() => rb.linearVelocity;

    public Vector3 GetMoveDirection() => mover.GetMoveDirection();
}
