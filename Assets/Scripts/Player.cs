using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

// will be replaced with Entity soon
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity<Player.PlayerState> {

    [Header("input controller")]
    [SerializeField] PlayerInputData inputData;

    [Header("state controller")]
    [SerializeField] WalkState walkState;
    [SerializeField] RunState runState;
    [SerializeField] DashState dashState;
    [SerializeField] JumpState jumpState;
    [SerializeField] FallState fallState;

    private CinemachinePositionComposer cinemachinePositionComposer;

    public enum PlayerState {
        Idle,
        Walk,
        Run,
        Dash,
        Jump,
        Fall
    }

    public Player() {
        walkState = new WalkState(PlayerState.Walk, this);
        runState = new RunState(PlayerState.Run, this);
        dashState = new DashState(PlayerState.Dash, this);
        jumpState = new JumpState(PlayerState.Jump, this);
        fallState = new FallState(PlayerState.Fall, this);
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

        SetCurrentState(PlayerState.Idle);

        base.Start();
    }

    private class IdleState : BaseState<PlayerState> {

        private Player player;

        public IdleState(PlayerState stateKey, Player player) : base(stateKey) { 
            this.player = player;
        }

        public override void EnterState() {
            player.rb.linearVelocity = Vector3.zero;
        }

        public override PlayerState GetNextState() {
            if (!player.character.OnGround())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(player.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(player.inputData.jumpCode))
                return PlayerState.Jump;
            else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                if (Input.GetKey(player.inputData.runCode))
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
            }
            else
                return stateKey;
        }
    }

    private abstract class InputMoveState : MoveState {

        protected Player player;

        public InputMoveState(PlayerState stateKey, Player player) : base(stateKey, player) { this.player = player; }

        public void Move(float lerpAmount) {
            Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), lerpAmount);
        }
    }

    private abstract class InputFloatState : FloatState {

        protected Player player;

        public InputFloatState(PlayerState stateKey, Player player) : base(stateKey, player) { this.player = player; }

        public void Float(float selfMovableSpeedLimit, float accelRate, float decelRate, float lerpAmount) {
            Float(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), selfMovableSpeedLimit, accelRate, decelRate, lerpAmount);
        }
    }

    [Serializable]
    private class WalkState : InputMoveState {
        
        public WalkState(PlayerState stateKey, Player player) : base(stateKey, player) { }
        
        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Move(1);
        }

        public override PlayerState GetNextState() {
            if (!player.character.OnGround())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(player.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(player.inputData.jumpCode))
                return PlayerState.Jump;
            else if (moveDir.magnitude == 0 && player.rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && Input.GetKey(player.inputData.runCode))
                return PlayerState.Run;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class RunState : InputMoveState {

        public RunState(PlayerState stateKey, Player player) : base(stateKey, player) { }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Move(1);
        }

        public override PlayerState GetNextState() {
            if (!player.character.OnGround())
                return PlayerState.Fall;
            else if (Input.GetKeyDown(player.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(player.inputData.jumpCode))
                return PlayerState.Jump;
            else if (moveDir.magnitude == 0 && player.rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && !Input.GetKey(player.inputData.runCode))
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class DashState : InputMoveState {

        public float dashSpeed = 30;
        public float dashDuration = 0.3f;
        [Range(0, 1)] public float dashRunLerpAmount = 0.5f;
        public Vector2 defaultDashDirection = new Vector2(1, 0);
        private float dashTime;

        public DashState(PlayerState stateKey, Player player) : base(stateKey, player) { }

        public override void EnterState() {
            Vector2 temp = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (temp.magnitude == 0)
                temp = defaultDashDirection;

            temp.Normalize();
            temp = temp * dashSpeed - player.rb.linearVelocity;

            player.rb.AddForce(temp, ForceMode2D.Impulse);
            dashTime = 0;
        }

        public override void UpdateState() {
            base.UpdateState();
            dashTime += Time.deltaTime;
        }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Move(dashRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (dashTime > dashDuration) {
                if (!player.character.OnGround()) {
                    player.fallState.DisableCoyoteTime();
                    return PlayerState.Fall;
                }
                else if (Input.GetKey(player.inputData.runCode))
                    return PlayerState.Run;
                else 
                    return PlayerState.Walk;
            }

            return stateKey;
        }
    }

    [Serializable]
    private class JumpState : InputFloatState {

        [Min(0)] public float jumpStrength = 15;
        [Min(0)] public float jumpBufferTime = 0.1f;
        [Range(0, 1)] public float jumpRunLerpAmount = 0.6f;
        private float bufferInputTime;
        private float orignialCinemachineDampingY;

        public JumpState(PlayerState stateKey, Player player) : base(stateKey, player) { }

        public override void EnterState() {
            float jumpImpulse = jumpStrength;

            if (player.character.GetVelocity() < 0)
                jumpImpulse -= player.character.GetVelocity();

            player.character.AddForce(jumpImpulse, CharacterRigidbody.ForceModeZ.Impulse);

            orignialCinemachineDampingY = player.cinemachinePositionComposer.Damping.y;
            player.cinemachinePositionComposer.Damping.y = 3;
        }

        public override void UpdateState() {
            base.UpdateState();
            if (Input.GetKeyDown(player.inputData.jumpCode))
                bufferInputTime = Time.time;
        }

        public override void ExitState() {
            player.cinemachinePositionComposer.Damping.y = orignialCinemachineDampingY;
        }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Float(player.walkState.targetSpeed, player.walkState.accelRate, player.walkState.decelRate, jumpRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (player.character.OnGround()) {
                if (Time.time - bufferInputTime <= jumpBufferTime)
                    EnterState();
                else if (Input.GetKey(player.inputData.runCode))
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
            }
            
            return stateKey;
        }
    }

    [Serializable]
    private class FallState : InputFloatState {

        [Min(0)] public float coyoteTime = 0.1f;
        [Min(0)] public float jumpBufferTime = 0.1f;
        [Range(0, 1)] public float fallRunLerpAmount = 0.6f;
        private float fallStartTime;
        private float bufferInputTime;
        private bool canEnterCoyoteTime = true;

        public FallState(PlayerState stateKey, Player player) : base(stateKey, player) { }

        public override void EnterState() {
            fallStartTime = Time.time;
        }

        public override void ExitState() {
            canEnterCoyoteTime = true;
        }

        public override void UpdateState() {
            base.UpdateState();
            if (canEnterCoyoteTime && Time.time - fallStartTime <= coyoteTime && Input.GetKeyDown(player.inputData.jumpCode))
                player.TransitionToState(PlayerState.Jump);
            if (Input.GetKeyDown(player.inputData.jumpCode))
                bufferInputTime = Time.time;
        }

        public override void FixedUpdateState() {
            base.FixedUpdateState();
            Float(player.walkState.targetSpeed, player.walkState.accelRate, player.walkState.decelRate, fallRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (player.character.OnGround()) {
                if (Time.time - bufferInputTime <= jumpBufferTime)
                    return PlayerState.Jump;
                else if (Input.GetKey(player.inputData.runCode))
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
}