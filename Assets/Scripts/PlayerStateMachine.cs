using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateMachine : FiniteStateMachine<PlayerStateMachine.PlayerState> {

    [SerializeField] PlayerInputData inputData;
    [SerializeField] WalkState walkState;
    [SerializeField] RunState runState;
    [SerializeField] DashState dashState;
    [SerializeField] JumpState jumpState;

    private Rigidbody2D rb;
    private CircleCollider2D col;
    
    public enum PlayerState {
        Idle,
        Walk,
        Run,
        Dash,
        Jump
    }

    public PlayerStateMachine() {
        walkState = new WalkState(PlayerState.Walk, this);
        runState = new RunState(PlayerState.Run, this);
        dashState = new DashState(PlayerState.Dash, this);
        jumpState = new JumpState(PlayerState.Jump, this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        AddState(PlayerState.Idle, new IdleState(PlayerState.Idle, this));
        AddState(PlayerState.Walk, walkState);
        AddState(PlayerState.Run, runState);
        AddState(PlayerState.Dash, dashState);
        AddState(PlayerState.Jump, jumpState);

        SetCurrentState(PlayerState.Idle);

        base.Start();
    }

    private class IdleState : BaseState<PlayerState> {

        private PlayerStateMachine machine;

        public IdleState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey) { 
            this.machine = machine;
        }

        public override void EnterState() {
            machine.rb.linearVelocity = Vector3.zero;
        }

        public override void UpdateState() { }

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
        protected PlayerStateMachine machine;

        public Movable(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, BaseState<PlayerState>.UpdateMode.FixedUpdate) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
            this.machine = machine;
        }

        protected void Move(float lerpAmount)
        {
            // accelerate player to the target velocity
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
        public WalkState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) { }
        
        public override void UpdateState() {
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
        public RunState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) { }

        public override void UpdateState()
        {
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

        public DashState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) { }

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
            Move(dashRunLerpAmount);
            dashTime += Time.fixedDeltaTime;
        }

        public override PlayerState GetNextState() {
            if (dashTime > dashDuration)
            {
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
        public float gravity = -9.8f;
        public LayerMask platformLayer;

        private Vector2 gravityForce;
        private Vector2 landingPos;

        private bool landed;

        public JumpState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) {
            gravityForce = new Vector2(0, gravity);
        }

        public override void EnterState() {
            Vector2 jumpImpulse = new Vector2(0, jumpStrength);

            if (machine.rb.linearVelocity.y < 0)
                jumpImpulse.y -= machine.rb.linearVelocity.y;

            machine.rb.AddForce(jumpImpulse, ForceMode2D.Impulse);
            landingPos = machine.transform.position;

            landed = false;
        }

        public override void UpdateState() {
            landingPos = machine.transform.position;

            Move(1);

            float moveRadius = machine.col.radius + machine.rb.linearVelocity.magnitude * Time.fixedDeltaTime;
            Collider2D hit = Physics2D.OverlapCircle(landingPos, moveRadius, platformLayer);

            if (hit != null) {
                Platform platform = hit.GetComponent<Platform>();
                if (platform != null && Mathf.Approximately(platform.lowerZ, landingPos.y) && platform.upperZ <= machine.transform.position.y)
                    landingPos.y = platform.upperZ;
            }

            gravityForce.y = gravity;
            machine.rb.AddForce(gravityForce, ForceMode2D.Force);

            if (machine.rb.position.y < landingPos.y) {
                Vector2 playerPos = machine.rb.position;
                playerPos.y = landingPos.y;
                machine.rb.position = playerPos;
                landed = true;
            }
        }

        public override PlayerState GetNextState() {
            if (landed)
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }
}