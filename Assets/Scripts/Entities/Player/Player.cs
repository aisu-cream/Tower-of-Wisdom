using System;
using Unity.Cinemachine;
using UnityEngine;

public class Player : Entity<Player.PlayerState> {

    [Header("Input Controller")]
    [SerializeField] PlayerInputData inputData;

    [Header("State Controller")]
    [SerializeField] WalkState walkState;
    [SerializeField] RunState runState;
    [SerializeField] DashState dashState;
    [SerializeField] JumpState jumpState;
    [SerializeField] FallState fallState;
    [SerializeField] KnockbackState knockbackState;
    [SerializeField] WieldState wieldState;

    private CinemachinePositionComposer cinemachinePositionComposer;

    [SerializeField] Weapon _debugPurposeWeaponHold;

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
        cinemachinePositionComposer = FindAnyObjectByType<CinemachinePositionComposer>();

        AddState(PlayerState.Idle, new IdleState(PlayerState.Idle, this));
        AddState(PlayerState.Walk, walkState);
        AddState(PlayerState.Run, runState);
        AddState(PlayerState.Dash, dashState);
        AddState(PlayerState.Jump, jumpState);
        AddState(PlayerState.Fall, fallState);
        AddState(PlayerState.Knockback, knockbackState);
        AddState(PlayerState.Wield, wieldState);

        SetCurrentState(PlayerState.Idle);

        HoldWeapon(_debugPurposeWeaponHold);

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

    public override void GetPushed(Vector2 impulse, float zImpulse) {
        base.GetPushed(impulse, zImpulse);
        TransitionToState(PlayerState.Knockback);
    }

    private class IdleState : BaseState<PlayerState> {

        private Player entity;

        public IdleState(PlayerState stateKey, Player entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            entity.rb.linearVelocity = Vector3.zero;
        }

        public override PlayerState GetNextState() {
            if (!entity.character.OnGround())
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
            base.FixedUpdateState();
            Move(entity.GetInput(), 1);
        }

        public override PlayerState GetNextState() {
            if (!entity.character.OnGround())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(entity.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(entity.inputData.jumpCode))
                return PlayerState.Jump;
            else if (moveDir.magnitude == 0 && entity.rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && Input.GetKey(entity.inputData.runCode))
                return PlayerState.Run;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class RunState : MoveState<Player> {

        public RunState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Move(entity.GetInput(), 1);
        }

        public override PlayerState GetNextState() {
            if (!entity.character.OnGround())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(entity.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(entity.inputData.jumpCode))
                return PlayerState.Jump;
            else if (moveDir.magnitude == 0 && entity.rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && !Input.GetKey(entity.inputData.runCode))
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
            Vector2 temp = entity.GetInput();

            if (temp.magnitude == 0)
                temp = defaultDashDirection;

            temp.Normalize();
            temp = temp * dashSpeed - entity.rb.linearVelocity;

            entity.rb.AddForce(temp, ForceMode2D.Impulse);
            dashTime = 0;
        }

        public override void UpdateState() {
            base.UpdateState();
            dashTime += Time.deltaTime;
        }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Move(entity.GetInput(), dashRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (dashTime >= dashDuration) {
                if (!entity.character.OnGround()) {
                    entity.fallState.DisableCoyoteTime();
                    return PlayerState.Fall;
                }
                else if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else 
                    return PlayerState.Walk;
            }

            return stateKey;
        }
    }

    [Serializable]
    private class JumpState : FloatState<Player> {

        [Min(0)] public float jumpStrength = 15;
        [Min(0)] public float jumpBufferTime = 0.1f;
        [Range(0, 1)] public float jumpRunLerpAmount = 0.6f;
        private float bufferInputTime;
        private float orignialCinemachineDampingY;

        public JumpState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            float jumpImpulse = jumpStrength;

            if (entity.character.GetVelocity() < 0)
                jumpImpulse -= entity.character.GetVelocity();

            entity.character.AddForce(jumpImpulse, ForceMode2D.Impulse);

            orignialCinemachineDampingY = entity.cinemachinePositionComposer.Damping.y;
            entity.cinemachinePositionComposer.Damping.y = 3;

            bufferInputTime = -jumpBufferTime;
        }

        public override void ExitState() {
            entity.cinemachinePositionComposer.Damping.y = orignialCinemachineDampingY;
        }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Float(entity.GetInput(), entity.walkState.targetSpeed, entity.walkState.accelRate, entity.walkState.decelRate, jumpRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(entity.inputData.jumpCode))
                bufferInputTime = Time.time;

            if (entity.character.OnGround()) {
                if (Time.time - bufferInputTime <= jumpBufferTime)
                    EnterState();
                else if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
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
        private float bufferInputTime;
        private bool canEnterCoyoteTime = true;

        public FallState(PlayerState stateKey, Player entity) : base(stateKey, entity) { }

        public override void EnterState() {
            fallStartTime = Time.time;
        }

        public override void ExitState() {
            canEnterCoyoteTime = true;
        }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Float(entity.GetInput(), entity.walkState.targetSpeed, entity.walkState.accelRate, entity.walkState.decelRate, fallRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            bool coyotable = canEnterCoyoteTime && Time.time - fallStartTime < coyoteTime;

            if (Input.GetKeyDown(entity.inputData.jumpCode)) {
                if (coyotable) return PlayerState.Jump;
                bufferInputTime = Time.time;
            }

            if (Input.GetKeyDown(entity.inputData.dashCode) && coyotable)
                return PlayerState.Dash;
            else if (entity.character.OnGround()) {
                if (Time.time - bufferInputTime <= jumpBufferTime)
                    return PlayerState.Jump;
                else if (Input.GetKey(entity.inputData.runCode))
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
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
            base.FixedUpdateState();
            Move(entity.GetInput(), knockbackLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (Time.time - knockbackStartTime <= knockbackTime) { 
                if (moveDir.magnitude == 0 && entity.rb.linearVelocity.magnitude == 0)
                    return PlayerState.Idle;
                else
                    return PlayerState.Walk;
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
            base.FixedUpdateState(); 
            Move(Vector2.zero, 1);
        }

        public override void ExitState() {
            entity.GetWeapon().EndAction();
        }

        public override PlayerState GetNextState() {
            if (startTime + duration <= Time.time) { 
                if (moveDir.magnitude == 0 && entity.rb.linearVelocity.magnitude == 0)
                    return PlayerState.Idle;
                else
                    return PlayerState.Walk;
            }
            return stateKey;
        }
    }
}