using System;
using UnityEngine;

public class PlayerRelativeMovement : FiniteStateMachine<PlayerRelativeMovement.PlayerState> {

    [SerializeField] Shadow shadow;
    [SerializeField] CharacterRigidbody character;

    [SerializeField] PlayerInputData inputData;
    [SerializeField] WalkState walkState;
    [SerializeField] RunState runState;
    [SerializeField] DashState dashState;
    [SerializeField] JumpState jumpState;

    private CircleCollider2D col;
    private Rigidbody2D rb;

    public enum PlayerState {
        Idle,
        Walk,
        Run,
        Dash,
        Jump
    }

    public PlayerRelativeMovement() {
        walkState = new WalkState(PlayerState.Walk, this);
        runState = new RunState(PlayerState.Run, this);
        dashState = new DashState(PlayerState.Dash, this);
        jumpState = new JumpState(PlayerState.Jump, this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        AddState(PlayerState.Idle, new IdleState(PlayerState.Idle, this));
        AddState(PlayerState.Walk, walkState);
        AddState(PlayerState.Run, runState);
        AddState(PlayerState.Dash, dashState);
        AddState(PlayerState.Jump, jumpState);

        SetCurrentState(PlayerState.Idle);

        base.Start();
    }

    private class IdleState : BaseState<PlayerState> {

        private PlayerRelativeMovement machine;

        public IdleState(PlayerState stateKey, PlayerRelativeMovement machine) : base(stateKey) { 
            this.machine = machine;
        }

        public override void EnterState() {
            machine.rb.linearVelocity = Vector3.zero;
        }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(machine.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(machine.inputData.jumpCode))
                return PlayerState.Jump;
            else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                if (Input.GetKey(machine.inputData.runCode))
                    return PlayerState.Run;
                else
                    return PlayerState.Walk;
            }
            else
                return stateKey;
        }
    }

    [Serializable]
    private abstract class Movable : BaseState<PlayerState> {

        public float targetSpeed = 5;
        public float accelRate = 6;
        public float decelRate = 6;

        protected Vector2 moveDir;
        protected Vector2 moveForce;
        protected PlayerRelativeMovement machine;

        private Collider2D platformHit;

        public Movable(PlayerState stateKey, PlayerRelativeMovement machine) : base(stateKey) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
            this.machine = machine;
        }

        public override void LateUpdateState() {
            // ignore platforms lower than the player
            machine.col.excludeLayers = 0;

            if (machine.character.GetZ() >= 1)
                machine.col.excludeLayers += 64;

            // detect platform
            Collider2D hit = Physics2D.OverlapCircle(machine.transform.position, machine.col.radius - 0.01f, machine.col.excludeLayers);

            if (hit) {
                if (!hit.Equals(platformHit)) {
                    Platform platform = hit.GetComponent<Platform>();
                    machine.shadow.MovePosition(platform.Top());
                    platformHit = hit;
                }
            }
            else {
                machine.shadow.MovePosition(0);
                platformHit = null;
            }

            
        }

        protected void Move(float lerpAmount) {
            // accelerate player towards the target velocity
            moveDir.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveDir.Normalize();

            float horTargetSpeed = Mathf.Lerp(machine.rb.linearVelocity.x, moveDir.x * targetSpeed, lerpAmount);
            float vertTargetSpeed = Mathf.Lerp(machine.rb.linearVelocity.y, moveDir.y * targetSpeed, lerpAmount);

            float horForce = horTargetSpeed - machine.rb.linearVelocity.x;
            float vertForce = vertTargetSpeed - machine.rb.linearVelocity.y;

            if (Vector2.Dot(moveDir, machine.rb.linearVelocity) > 0) {
                horForce *= accelRate;
                vertForce *= accelRate;
            }
            else {
                horForce *= decelRate;
                vertForce *= decelRate;
            }

            moveForce.Set(horForce, vertForce);
            machine.rb.AddForce(moveForce, ForceMode2D.Force);
        }
    }

    [Serializable]
    private class WalkState : Movable {
        
        public WalkState(PlayerState stateKey, PlayerRelativeMovement machine) : base(stateKey, machine) { }
        
        public override void FixedUpdateState() {
            Move(1);
        }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(machine.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(machine.inputData.jumpCode))
                return PlayerState.Jump;
            else if (moveDir.magnitude == 0 && machine.rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && Input.GetKey(KeyCode.LeftShift))
                return PlayerState.Run;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class RunState : Movable {

        public RunState(PlayerState stateKey, PlayerRelativeMovement machine) : base(stateKey, machine) { }

        public override void FixedUpdateState() {
            Move(1);
        }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(machine.inputData.dashCode))
                return PlayerState.Dash;
            else if (Input.GetKeyDown(machine.inputData.jumpCode))
                return PlayerState.Jump;
            else if (moveDir.magnitude == 0 && machine.rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && !Input.GetKey(KeyCode.LeftShift))
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class DashState : Movable {

        public float dashSpeed = 30;
        public float dashDuration = 0.3f;
        [Range(0, 1)] public float dashRunLerpAmount = 0.5f;
        public Vector2 defaultDashDirection = new Vector2(1, 0);
        private float dashTime;

        public DashState(PlayerState stateKey, PlayerRelativeMovement machine) : base(stateKey, machine) { }

        public override void EnterState() {
            Vector2 temp = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (temp.magnitude == 0)
                temp = defaultDashDirection;

            temp = temp.normalized;
            temp = temp * dashSpeed - machine.rb.linearVelocity;

            machine.rb.AddForce(temp, ForceMode2D.Impulse);
            dashTime = 0;
        }

        public override void UpdateState() {
            base.UpdateState();
            dashTime += Time.deltaTime;
        }

        public override void FixedUpdateState() {
            Move(dashRunLerpAmount);
        }

        public override PlayerState GetNextState() {
            if (dashTime > dashDuration) {
                if (Input.GetKeyDown(machine.inputData.jumpCode))
                    return PlayerState.Jump;
                else if (!Input.GetKey(machine.inputData.runCode))
                    return PlayerState.Run;
                else 
                    return PlayerState.Walk;
            }

            return stateKey;
        }
    }

    [Serializable]
    private class JumpState : Movable {

        public float jumpStrength = 15;

        public JumpState(PlayerState stateKey, PlayerRelativeMovement machine) : base(stateKey, machine) { }

        public override void EnterState() {
            float jumpImpulse = jumpStrength;

            if (machine.character.GetVelocity() < 0)
                jumpImpulse -= machine.character.GetVelocity();

            machine.character.AddForce(jumpImpulse, CharacterRigidbody.ForceModeZ.Impulse);
        }

        public override void FixedUpdateState() {
            Move(1);
        }

        public override PlayerState GetNextState() {
            if (machine.character.OnGround())
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }
}