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
        if (GetWeapon() != null && inputData.wieldPressed)
            TransitionToState(PlayerState.Wield);
        base.Update();
    }

    public override void GetPushed(Vector3 impulse) {
        base.GetPushed(impulse);
        TransitionToState(PlayerState.Knockback);
    }

    [Serializable]
    private class IdleState : BaseState<PlayerState> {

        [Min(0)] public float decelRate;
        [Range(0, 1)] public float idleLerpAmount = 0.5f;

        private Player entity;

        public IdleState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void FixedUpdateState() {
            entity.Move(Vector2.zero, 0, 0, decelRate, idleLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (!entity.IsGrounded())
                return PlayerState.Fall;
            else if (entity.inputData.dashPressed)
                return PlayerState.Dash;
            else if (entity.inputData.jumpHeld)
                return PlayerState.Jump;
            else if (entity.inputData.dirInput != Vector2.zero) {
                if (entity.inputData.runHeld)
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
            }
            else
                return stateKey;
        }
    }

    [Serializable]
    private class WalkState : BaseState<PlayerState> {

        [Min(0)] public float targetSpeed = 5;
        [Min(0)] public float accelRate = 12;
        [Min(0)] public float decelRate = 12;

        private Player entity;

        public WalkState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }
        
        public override void FixedUpdateState() {
            entity.Move(entity.inputData.dirInput, targetSpeed, accelRate, decelRate, 1);
        }

        public override PlayerState GetNextState() {
            if (!entity.IsGrounded())
                return PlayerState.Fall;
            else if (entity.inputData.dashPressed)
                return PlayerState.Dash;
            else if (entity.inputData.jumpHeld && entity.CanJump())
                return PlayerState.Jump;
            else if (entity.GetDesiredDirection().magnitude == 0 && entity.GetHorizontalVelocity().magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (entity.inputData.runHeld && entity.GetDesiredDirection().magnitude != 0)
                return PlayerState.Run;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class RunState : BaseState<PlayerState> {

        [Min(0)] public float targetSpeed = 8;
        [Min(0)] public float accelRate = 10;
        [Min(0)] public float decelRate = 12;

        private Player entity;

        public RunState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void FixedUpdateState() {
            entity.Move(entity.inputData.dirInput, targetSpeed, accelRate, decelRate, 1);
        }

        public override PlayerState GetNextState() {
            if (!entity.IsGrounded())
                return PlayerState.Fall;
            else if (entity.inputData.dashPressed)
                return PlayerState.Dash;
            else if (entity.inputData.jumpHeld && entity.CanJump())
                return PlayerState.Jump;
            else if (entity.GetDesiredDirection().magnitude == 0 && entity.GetHorizontalVelocity().magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (!entity.inputData.runHeld && entity.GetDesiredDirection().magnitude != 0)
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class DashState : BaseState<PlayerState> {

        [Min(0)] public float dashSpeed = 30;
        [Min(0)] public float dashDuration = 0.3f;

        [Min(0)] public float decelRate = 9;
        [Range(0, 1)] public float dashLerpAmount = 0.5f;

        public Vector2 defaultDashDirection = new Vector2(1, 0);
        private float dashTime;

        private Player entity;

        public DashState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            Vector3 temp = entity.inputData.dirInput;

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
            entity.Move(entity.inputData.dirInput, entity.runState.targetSpeed, decelRate, decelRate, dashLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (dashTime >= dashDuration) {
                if (!entity.IsGrounded()) {
                    entity.fallState.DisableCoyoteTime();
                    return PlayerState.Fall;
                }
                else if (entity.inputData.runHeld)
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
    private class JumpState : BaseState<PlayerState> {

        [Min(0)] public float jumpStrength = 15;
        [Range(0, 1)] public float jumpLerpAmount = 0.4f;

        private Player entity;

        public JumpState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            entity.Jump(jumpStrength);
        }

        public override void FixedUpdateState() {
            entity.Float(entity.inputData.dirInput, entity.walkState.targetSpeed, entity.walkState.accelRate, entity.walkState.decelRate, jumpLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (entity.inputData.jumpHeld && entity.CanJump())
                EnterState();

            if (entity.IsGrounded()) {
                if (entity.inputData.runHeld)
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
    private class FallState : BaseState<PlayerState> {

        [Min(0)] public float coyoteTime = 0.15f;
        [Range(0, 1)] public float fallLerpAmount = 0.4f;

        private float fallStartTime;
        private bool canEnterCoyoteTime;

        private Player entity;

        public FallState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            fallStartTime = Time.time;
            canEnterCoyoteTime = true;
        }

        public override void FixedUpdateState() {
            entity.Float(entity.inputData.dirInput, entity.walkState.targetSpeed, entity.walkState.accelRate, entity.walkState.decelRate, fallLerpAmount);
        }

        public override PlayerState GetNextState() {
            bool coyotable = canEnterCoyoteTime && Time.time - fallStartTime < coyoteTime;

            if (entity.inputData.jumpHeld && (coyotable || entity.CanJump()))
                return PlayerState.Jump;
            else if (entity.inputData.dashPressed && (entity.IsGrounded() || entity.OnStandableSurface()) && coyotable)
                return PlayerState.Dash;
            else if (entity.IsGrounded()) {
                if (entity.inputData.runHeld)
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
    private class KnockbackState : BaseState<PlayerState> {

        [Min(0)] public float knockbackTime = 0.2f;
        [Min(0)] public float decelRate;
        [Range(0, 1)] public float knockbackLerpAmount = 0.2f;

        private float knockbackStartTime;

        private Player entity;

        public KnockbackState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            knockbackStartTime = Time.time;
        }

        public override void FixedUpdateState() {
            entity.Move(entity.inputData.dirInput, 0, 0, decelRate, knockbackLerpAmount);
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
    private class WieldState : BaseState<PlayerState> {

        [Min(0)] public float duration = 0.5f;
        [Min(0)] public float decelRate = 7;

        private float startTime = -1;

        private Player entity;

        public WieldState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            entity.GetWeapon().Wield();
            startTime = Time.time;
        }

        public override void FixedUpdateState() {
            entity.Move(Vector2.zero, 0, 0, decelRate, 1);
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