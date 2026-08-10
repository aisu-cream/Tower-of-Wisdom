using KinematicCharacterController;
using UnityEngine;

enum LookState {
    local,
    world
}

public struct MovementCommand {
    public float MoveAxisForward;
    public float MoveAxisRight;
    public float MaxMoveSpeed;
    public float MoveSharpness;
    public float MoveControlDegree;
}

[RequireComponent(typeof(KinematicCharacterMotor))]
public class EntityController : MonoBehaviour, ICharacterController {

    KinematicCharacterMotor motor;

    Vector3 moveInputVector = Vector3.zero;
    Vector3 lookDirection = Vector3.forward;
    LookState lookState = LookState.local;

    [SerializeField] float gravitationalAcceleration = 45;
    [SerializeField] float maxFallSpeed = 45;

    float maxMoveSpeed;
    float moveSharpness;
    float moveControlDegree;
    float jumpSpeed;

    bool jumpRequested = false;

    Vector3 internalVelocityAdd = Vector3.zero;

    public Vector3 CharacterUp { get { return motor.CharacterUp; } }
    public Vector3 CharacterRight { get { return motor.CharacterRight; } }
    public Vector3 CharacterForward { get { return motor.CharacterForward; } }
    public float Height { get { return motor.Capsule.height; } }

    //#region EightDirectionDefinitions
    //static readonly Vector3 left = Vector3.left;
    //static readonly Vector3 top = Vector3.forward;
    //static readonly Vector3 right = Vector3.right;
    //static readonly Vector3 bottom = Vector3.back;

    //static readonly Vector3 topleft = new Vector3(-1, 0, 1).normalized;
    //static readonly Vector3 topright = new Vector3(1, 0, 1).normalized;
    //static readonly Vector3 bottomright = new Vector3(1, 0, -1).normalized;
    //static readonly Vector3 bottomleft = new Vector3(-1, 0, -1).normalized;
    //#endregion

    void Awake() {
        motor = GetComponent<KinematicCharacterMotor>();
    }

    void OnEnable() {
        motor.CharacterController = this;
    }

    void OnDisable() {
        motor.CharacterController = null;
    }

    public Vector3 GetVelocity() {
        return motor.Velocity;
    }

    public bool IsStableOnGround() {
        return motor.GroundingStatus.IsStableOnGround;
    }

    public void SetMovement(in MovementCommand command) {
        moveInputVector = new Vector3(command.MoveAxisRight, 0, command.MoveAxisForward).normalized;
        maxMoveSpeed = command.MaxMoveSpeed;
        moveSharpness = command.MoveSharpness;
        moveControlDegree = command.MoveControlDegree;
    }

    public void LookAtLocal(in Vector3 localDirection) {
        lookDirection = localDirection;
        lookState = LookState.local;
    }

    public void LookAtWorld(in Vector3 worldDirection) {
        lookDirection = worldDirection;
        lookState = LookState.world;
    }

    public Vector3 GetLocalLookDirection() {
#pragma warning disable CS8524 // switch 식에서 명명되지 않은 열거형 값이 사용되는 입력 형식의 일부 값을 처리하지 않습니다.
        return lookState switch {
            LookState.local => lookDirection,
            LookState.world => ConvertWorldToLocalLookDirection(lookDirection)
        };
#pragma warning restore CS8524 // switch 식에서 명명되지 않은 열거형 값이 사용되는 입력 형식의 일부 값을 처리하지 않습니다.
    }

    Vector3 ConvertWorldToLocalLookDirection(in Vector3 worldLookDirection) {
        return new() {
            x = Vector3.Dot(worldLookDirection, motor.CharacterRight),
            y = Vector3.Dot(worldLookDirection, motor.CharacterUp),
            z = Vector3.Dot(worldLookDirection, motor.CharacterForward)
        };
    }

    public Vector3 GetWorldLookDirection() {
#pragma warning disable CS8524 // switch 식에서 명명되지 않은 열거형 값이 사용되는 입력 형식의 일부 값을 처리하지 않습니다.
        return lookState switch {
            LookState.local => ConvertLocalToWorldLookDirection(lookDirection),
            LookState.world => lookDirection
        };
#pragma warning restore CS8524 // switch 식에서 명명되지 않은 열거형 값이 사용되는 입력 형식의 일부 값을 처리하지 않습니다.
    }

    Vector3 ConvertLocalToWorldLookDirection(in Vector3 localLookDirection) {
        return localLookDirection.x * motor.CharacterRight + 
            localLookDirection.y * motor.CharacterUp + 
            localLookDirection.z * motor.CharacterForward;
    }

    public void Jump(float jumpSpeed) {
        jumpRequested = true;
        this.jumpSpeed = jumpSpeed;
    }

    public void AddVelocity(in Vector3 velocity) {
        internalVelocityAdd += velocity;
    }

    public Vector3 ClosestPoint(in Vector3 point) {
        return motor.Capsule.ClosestPoint(point);
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        return true;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        if (motor.GroundingStatus.IsStableOnGround)
            MoveGround(ref currentVelocity, deltaTime);
        else {
            MoveAir(ref currentVelocity, deltaTime);

            float dragConst = gravitationalAcceleration / (maxFallSpeed * maxFallSpeed);
            float verticalVelocity = Vector3.Dot(currentVelocity, motor.CharacterUp);

            currentVelocity += (Mathf.Sign(verticalVelocity) * dragConst * verticalVelocity * verticalVelocity - gravitationalAcceleration) * deltaTime * motor.CharacterUp;
        }

        if (jumpRequested && motor.GroundingStatus.IsStableOnGround) {
            Jump(ref currentVelocity);
            jumpRequested = false;
        }

        if (internalVelocityAdd.sqrMagnitude > 0) {
            currentVelocity += internalVelocityAdd;
            internalVelocityAdd = Vector3.zero;
        }
    }

    void MoveGround(ref Vector3 currentVelocity, float deltaTime) {
        float currentVelocityMagnitude = currentVelocity.magnitude;
        Vector3 effectiveSurfaceNormal = motor.GroundingStatus.GroundNormal;

        // reorient current velocity
        currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveSurfaceNormal) * currentVelocityMagnitude;

        // compute target velocity
        Vector3 reorientedInput = motor.GetDirectionTangentToSurface(moveInputVector, effectiveSurfaceNormal);
        Vector3 targetVelocity = Vector3.Lerp(currentVelocity, reorientedInput * maxMoveSpeed, moveControlDegree);

        // smooth movement Velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-moveSharpness * deltaTime));
    }

    void MoveAir(ref Vector3 currentVelocity, float deltaTime) {
        Vector3 up = motor.CharacterUp;

        // reorient current velocity
        Vector3 verticalVelocity = Vector3.Project(currentVelocity, up);
        Vector3 horizontalVelocity = currentVelocity - verticalVelocity;
        float horizontalVelocityMagnitude = horizontalVelocity.magnitude;

        // compute target velocity
        Vector3 reorientedInput = Vector3.ProjectOnPlane(moveInputVector, up).normalized;
        
        // calculate the post-processed target velocity
        Vector3 targetVelocity = reorientedInput;

        if (horizontalVelocityMagnitude <= maxMoveSpeed || Vector3.Dot(horizontalVelocity, targetVelocity) <= 0)
            targetVelocity *= maxMoveSpeed;
        else
            targetVelocity *= CalculateElipticSpeed(Vector3.Angle(horizontalVelocity, targetVelocity), horizontalVelocityMagnitude, maxMoveSpeed);

        targetVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, moveControlDegree);

        // Smooth the velocity
        currentVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, 1 - Mathf.Exp(-moveSharpness * deltaTime)) + verticalVelocity;
    }

    float CalculateElipticSpeed(float deltaAngle, float currSpeed, float selfMovableSpeedLimit) {
        float walkSpeed = selfMovableSpeedLimit;
        return walkSpeed * currSpeed / Mathf.Sqrt(Mathf.Pow(currSpeed * Mathf.Sin(deltaAngle), 2) + Mathf.Pow(walkSpeed * Mathf.Cos(deltaAngle), 2));
    }

    void Jump(ref Vector3 currentVelocity) {
        currentVelocity += (motor.CharacterUp * jumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
        motor.ForceUnground(0.07f);
    }

    public void BeforeCharacterUpdate(float deltaTime) { }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public void AfterCharacterUpdate(float deltaTime) { }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    //static Vector3 GetEightDirectionalLook(ref Vector3 look) {
    //    float angle = Vector3.SignedAngle(left, look, Vector3.down) + 180; // conventional mathematics angle from 0 to 360
    //    int eightDirectionIndex = (int)(((angle + 22.5f) % 360f) / 45f);

    //    return eightDirectionIndex switch {
    //        0 => right,
    //        1 => topright,
    //        2 => top,
    //        3 => topleft,
    //        4 => left,
    //        5 => bottomleft,
    //        6 => bottom,
    //        7 => bottomright,
    //        8 => right,
    //        _ => top,
    //    };
    //}

    public Vector3 GetLook() => lookDirection;
}
