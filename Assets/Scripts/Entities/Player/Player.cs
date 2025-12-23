using System;
using UnityEngine;

public class Player : Entity<Player.PlayerState> {

    [Header("Input Controller")]
    [SerializeField] PlayerInputData inputData;

    [Header("State Controller")]
    [SerializeField] IdleState idleState;
    [SerializeField] WalkState walkState;
    [SerializeField] RunState runState;
    [SerializeField] DashState dashState;
    [SerializeField] JumpState jumpState;
    [SerializeField] FallState fallState;
    [SerializeField] KnockbackState knockbackState;
    [SerializeField] WieldState wieldState;

    public enum PlayerState {
        Idle,
        Walk,
        Run,
        Dash,
        Jump,
        Fall,
        Knockback,
        Wield
    }

    public Player() {
        idleState = new IdleState(PlayerState.Idle, this);
        walkState = new WalkState(PlayerState.Walk, this);
        runState = new RunState(PlayerState.Run, this);
        dashState = new DashState(PlayerState.Dash, this);
        jumpState = new JumpState(PlayerState.Jump, this);
        fallState = new FallState(PlayerState.Fall, this);
        knockbackState = new KnockbackState(PlayerState.Knockback, this);
        wieldState = new WieldState(PlayerState.Wield, this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        AddState(PlayerState.Idle, idleState);
        AddState(PlayerState.Walk, walkState);
        AddState(PlayerState.Run, runState);
        AddState(PlayerState.Dash, dashState);
        AddState(PlayerState.Jump, jumpState);
        AddState(PlayerState.Fall, fallState);
        AddState(PlayerState.Knockback, knockbackState);
        AddState(PlayerState.Wield, wieldState);
        
        SetCurrentState(PlayerState.Idle);

        base.Start();
    }

    protected override void Update() {
        if (GetWeapon() != null && Input.GetKeyDown(inputData.wieldCode))
            TransitionToState(PlayerState.Wield);
        base.Update();
    }

    private Vector2 GetInput() {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    public override void GetPushed(Vector3 impulse) {
        base.GetPushed(impulse);
        TransitionToState(PlayerState.Knockback);
    }

    [Serializable]
    private class IdleState : MoveState<Player> {

        public IdleState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void FixedUpdateState() {
            Move(Vector2.zero, 0.5f);
        }

        public override PlayerState GetNextState() {
            if (!entity.IsGrounded())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(entity.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(entity.inputData.jumpCode))
                return PlayerState.Jump;
            else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
            }
            else
                return stateKey;
        }
    }

    [Serializable]
    private class WalkState : MoveState<Player> {
        
        public WalkState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }
        
        public override void FixedUpdateState() {
            Move(entity.GetInput(), 1);
        }

        public override PlayerState GetNextState() {
            if (!entity.IsGrounded())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(entity.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(entity.inputData.jumpCode))
                return PlayerState.Jump;
            else if (entity.GetDesiredDirection().magnitude == 0 && entity.GetHorizontalVelocity().magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (entity.GetDesiredDirection().magnitude != 0 && Input.GetKey(entity.inputData.runCode))
                return PlayerState.Run;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class RunState : MoveState<Player> {

        public RunState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void FixedUpdateState() {
            Move(entity.GetInput(), 1);
        }

        public override PlayerState GetNextState() {
            if (!entity.IsGrounded())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(entity.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(entity.inputData.jumpCode))
                return PlayerState.Jump;
            else if (entity.GetDesiredDirection().magnitude == 0 && entity.GetHorizontalVelocity().magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (entity.GetDesiredDirection().magnitude != 0 && !Input.GetKey(entity.inputData.runCode))
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class DashState : MoveState<Player> {

        [Min(0)] public float dashSpeed = 30;
        [Min(0)] public float dashDuration = 0.3f;
        [Range(0, 1)] public float dashRunLerpAmount = 0.5f;
        public Vector2 defaultDashDirection = new Vector2(1, 0);
        private float dashTime;

        public DashState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            Vector3 temp = entity.GetInput();

            if (temp.magnitude == 0)
                temp = defaultDashDirection;

            temp = Quaternion.AngleAxis(90, Vector3.right) * temp;
            temp = temp.normalized * dashSpeed - Vector3.ProjectOnPlane(entity.GetVelocity(), Vector3.up);

            entity.AddForce(temp, ForceMode.Impulse);
            dashTime = 0;
        }

        public override void UpdateState() {
            dashTime += Time.deltaTime;
        }

        public override void FixedUpdateState() {
            Move(entity.GetInput(), dashRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (dashTime >= dashDuration) {
                if (!entity.IsGrounded()) {
                    entity.fallState.DisableCoyoteTime();
                    return PlayerState.Fall;
                }
                else if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else if (entity.GetDesiredDirection().magnitude != 0 || entity.GetHorizontalVelocity().magnitude > 0.1f)
                    return PlayerState.Walk;
                else
                    return PlayerState.Idle;
            }

            return stateKey;
        }
    }

    [Serializable]
    private class JumpState : FloatState<Player> {

        [Min(0)] public float jumpStrength = 15;
        [Range(0, 1)] public float jumpRunLerpAmount = 0.6f;

        public JumpState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            float jumpImpulse = jumpStrength;

            if (entity.GetVelocity().y < 0)
                jumpImpulse -= entity.GetVelocity().y;

            entity.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);

            entity.DisableGroundCheckFor(0.05f);
            entity.isGrounded = false;
        }

        public override void FixedUpdateState() {
            Float(entity.GetInput(), entity.walkState.targetSpeed, entity.walkState.accelRate, entity.walkState.decelRate, jumpRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (entity.IsGrounded()) {
                if (Input.GetKey(entity.inputData.jumpCode))
                    EnterState();
                else if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else if (entity.GetDesiredDirection().magnitude != 0 || entity.GetHorizontalVelocity().magnitude > 0.1f)
                    return PlayerState.Walk;
                else
                    return PlayerState.Idle;
            }
            
            return stateKey;
        }
    }

    [Serializable]
    private class FallState : FloatState<Player> {

        [Min(0)] public float coyoteTime = 0.1f;
        [Min(0)] public float jumpBufferTime = 0.1f;
        [Range(0, 1)] public float fallRunLerpAmount = 0.6f;

        private float fallStartTime;
        private bool canEnterCoyoteTime;
        private bool canEnterJumpCoyoteTime;

        public FallState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            fallStartTime = Time.time;
            canEnterCoyoteTime = true;
        }

        public override void FixedUpdateState() {
            Float(entity.GetInput(), entity.walkState.targetSpeed, entity.walkState.accelRate, entity.walkState.decelRate, fallRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            bool coyotable = canEnterCoyoteTime && Time.time - fallStartTime < coyoteTime;

            if (Input.GetKeyDown(entity.inputData.jumpCode) && coyotable)
                return PlayerState.Jump;
            else if (Input.GetKeyDown(entity.inputData.dashCode) && (entity.IsGrounded() || entity.OnStandableSurface()) && coyotable)
                return PlayerState.Dash;
            else if (entity.IsGrounded()) {
                if (Input.GetKeyDown(entity.inputData.jumpCode))
                    return PlayerState.Jump;
                else if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else if (entity.GetDesiredDirection().magnitude != 0 || entity.GetHorizontalVelocity().magnitude > 0.1f)
                    return PlayerState.Walk;
                else
                    return PlayerState.Idle;
            }
            else
                return stateKey;
        }

        public void DisableCoyoteTime() {
            canEnterCoyoteTime = false;
        }
    }

    [Serializable]
    private class KnockbackState : MoveState<Player> {

        [Min(0)] public float knockbackTime = 0.2f;
        public float knockbackLerpAmount = 0.2f;
        private float knockbackStartTime;

        public KnockbackState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            knockbackStartTime = Time.time;
        }

        public override void FixedUpdateState() {
            Move(entity.GetInput(), knockbackLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (Time.time - knockbackStartTime <= knockbackTime) { 
                if (entity.GetDesiredDirection().magnitude != 0 || entity.GetHorizontalVelocity().magnitude > 0.1f)
                    return PlayerState.Walk;
                else
                    return PlayerState.Idle;
            }
            return stateKey;
        }
    }

    [Serializable]
    private class WieldState : MoveState<Player> {

        public float duration = 0.5f;
        private float startTime = -1;

        public WieldState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            entity.GetWeapon().Wield();
            startTime = Time.time;
        }

        public override void FixedUpdateState() {
            Move(Vector2.zero, 1);
        }

        public override void ExitState() {
            entity.GetWeapon().EndAction();
        }

        public override PlayerState GetNextState() {
            if (startTime + duration <= Time.time) { 
                if (entity.GetDesiredDirection().magnitude != 0 || entity.GetHorizontalVelocity().magnitude > 0.1f)
                    return PlayerState.Walk;
                else
                    return PlayerState.Idle;
            }
            return stateKey;
        }
    }
}