using UnityEngine;

public class EntityMover {

    #region Fields
    private Rigidbody rb;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 lookDirection = top;

    private static readonly Vector3 left;
    private static readonly Vector3 topleft;
    private static readonly Vector3 top;
    private static readonly Vector3 topright;
    private static readonly Vector3 right;
    private static readonly Vector3 bottomright;
    private static readonly Vector3 bottom;
    private static readonly Vector3 bottomleft;
    #endregion

    static EntityMover() {
        left = Vector3.left;
        top = Vector3.forward;
        right = Vector3.right;
        bottom = Vector3.back;

        topleft = new Vector3(-1, 0, 1).normalized;
        topright = new Vector3(1, 0, 1).normalized;
        bottomright = new Vector3(1, 0, -1).normalized;
        bottomleft = new Vector3(-1, 0, -1).normalized;
    }

    public EntityMover(Rigidbody rb) => this.rb = rb;

    /**
     * Accelerate entity towards the target velocity = target speed * dir.
     * The force applied to the entity is perpendicular to the surface normal.
     */
    public void Move(Vector3 dir, float targetSpeed, float accelRate, float decelRate, float lerpAmount, Vector3 surfaceNormal) {
        // normalize move direction and set look direction
        moveDirection = dir.normalized;
        ReevaluateLookDirection();

        // check direction to move to
        Vector3 currentVel = Vector3.ProjectOnPlane(rb.linearVelocity, surfaceNormal);
        Vector3 surfaceDir = Vector3.ProjectOnPlane(moveDirection, surfaceNormal);

        // calculate the force needed
        Vector3 targetVel = Vector3.Lerp(currentVel, surfaceDir * targetSpeed, lerpAmount);
        Vector3 moveForce = targetVel - currentVel;
        moveForce *= Vector3.Dot(surfaceDir, currentVel) > 0 ? accelRate : decelRate;

        // apply the force
        rb.AddForce(moveForce, ForceMode.Force);
    }

    /** 
     * Maintain the velocity if current velocity and the desired direction to move are the same.
     * Otherwise, decrease the velocity towards the target velocity.
     * In general, use the entity's walk speed, accel rate, and decel rate as the three corresponding inputs.
     */
    public void Float(Vector3 dir, float targetSpeed, float accelRate, float decelRate, float lerpAmount) {
        // rotate the dir vector to xz plane
        moveDirection = dir.normalized;
        ReevaluateLookDirection();

        // check current velocity and direction of input
        Vector3 currVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        float currSpeed = currVelocity.magnitude;
        bool movingInIdenticalDir = Vector3.Dot(currVelocity, moveDirection * targetSpeed) > 0;

        // calculate the target velocity
        Vector3 targetVel = moveDirection;
        targetVel *= (currSpeed <= targetSpeed || !movingInIdenticalDir) ? targetSpeed : CalculateElipticSpeed(Vector3.Angle(currVelocity, moveDirection), currSpeed, targetSpeed);
        targetVel = Vector3.Lerp(currVelocity, targetVel, lerpAmount);

        // calculate the force needed to reach the desired velocity
        Vector3 moveForce = targetVel - currVelocity;
        moveForce *= movingInIdenticalDir ? accelRate : decelRate;

        // apply the force
        rb.AddForce(moveForce, ForceMode.Force);
    }

    private float CalculateElipticSpeed(float deltaAngle, float currSpeed, float selfMovableSpeedLimit) {
        float walkSpeed = selfMovableSpeedLimit;
        return walkSpeed * currSpeed / Mathf.Sqrt(Mathf.Pow(currSpeed * Mathf.Sin(deltaAngle), 2) + Mathf.Pow(walkSpeed * Mathf.Cos(deltaAngle), 2));
    }

    private void ReevaluateLookDirection() {
        if (moveDirection.sqrMagnitude != 0) {
            float angle = Vector3.SignedAngle(left, moveDirection, Vector3.down) + 180; // conventional mathematics angle from 0 to 360
            int eightDirections = (int)(((angle + 22.5f) % 360f) / 45f);

            switch (eightDirections) {
                case 0:
                    lookDirection = right;
                    break;
                case 1:
                    lookDirection = topright;
                    break;
                case 2:
                    lookDirection = top;
                    break;
                case 3:
                    lookDirection = topleft;
                    break;
                case 4:
                    lookDirection = left;
                    break;
                case 5:
                    lookDirection = bottomleft;
                    break;
                case 6:
                    lookDirection = bottom;
                    break;
                case 7:
                    lookDirection = bottomright;
                    break;
                case 8:
                    lookDirection = right;
                    break;
                default:
                    break;
            }
        }
    }

    public Vector3 GetMoveDirection() => moveDirection;

    public Vector3 GetLookDirection() => lookDirection;

    public void Jump(float jumpStrength) => rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
}
