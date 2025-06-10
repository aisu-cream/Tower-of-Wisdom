using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerStateMachine : FiniteStateMachine<PlayerStateMachine.PlayerState> {

    [SerializeField] GameObject sprite;

    [SerializeField] PlayerInputData inputData;
    [SerializeField] WalkState walkState;
    [SerializeField] RunState runState;
    [SerializeField] DashState dashState;
    [SerializeField] JumpState jumpState;

    private CircleCollider2D col;
    private Rigidbody2D rb;
    private float z;

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
        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        z = 0;

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

        protected float posZ = 0;

        public Movable(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
            this.machine = machine;
        }

        public override void LateUpdateState() {
            machine.sprite.transform.localPosition = Vector3.up * posZ;
        }

        protected void Move(float lerpAmount) {
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

            Debug.Log(machine.rb.linearVelocity);
        }
    }

    [Serializable]
    private class WalkState : Movable {
        public WalkState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) { }
        
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

        public RunState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) { }

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

        public override void FixedUpdateState() {
            Move(dashRunLerpAmount);
            dashTime += Time.fixedDeltaTime;
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

        private enum ForceModeZ {
            Force,
            Impulse
        }

        public float jumpStrength = 15;
        public float gravity = -9.8f;

        [SerializeField] LayerMask platformLayer;

        private bool onGround;
        private float velZ = 0;

        public JumpState(PlayerState stateKey, PlayerStateMachine machine) : base(stateKey, machine) { }

        public override void EnterState() {
            float jumpImpulse = jumpStrength;

            if (velZ < 0)
                jumpImpulse -= velZ;

            AddForce(jumpImpulse, ForceModeZ.Impulse);
            onGround = false;
        }

        public override void FixedUpdateState() {
            Move(1);

            float moveRadius = machine.col.radius + machine.rb.linearVelocity.magnitude * Time.fixedDeltaTime + 0.1f;
            RaycastHit2D hit = Physics2D.CircleCast(machine.transform.position, moveRadius, Vector2.zero, 0, platformLayer);

            if (hit) {
                bool isMovingToward = Vector2.Dot(hit.normal, machine.rb.linearVelocity) < 0;
                bool isStationaryAndMovingToward = machine.rb.linearVelocity.magnitude == 0 && Vector2.Dot(hit.normal, moveDir) < 0;

                if (isMovingToward || isStationaryAndMovingToward) {
                    Platform platform = hit.transform.gameObject.GetComponent<Platform>();

                    if (platform != null && platform.IsLower() && Mathf.Approximately(platform.z, machine.z)) {
                        Physics2D.IgnoreCollision(machine.col, platform.col);

                        posZ = posZ + machine.z;
                        machine.z = platform.opposite.z;
                        posZ = posZ - machine.z;

                        Vector2 pos = machine.transform.position;
                        pos.y += platform.opposite.z - platform.z;
                        machine.rb.MovePosition(pos);
                    }
                }
            }

            AddForce(gravity, ForceModeZ.Force);
            UpdatePositionZ();
        }

        private void AddForce(float force, ForceModeZ mode) {
            if (mode == ForceModeZ.Force)
                velZ += force * Time.fixedDeltaTime;
            else
                velZ += force;
        }

        private void UpdatePositionZ() {
            float deltaZ = velZ * Time.fixedDeltaTime;

            if (velZ <= 0 && posZ + deltaZ < 0) {
                deltaZ = -posZ;
                onGround = true;
            }

            posZ += deltaZ;
        }

        public override void ExitState() {
            velZ = 0;
        }

        public override void OnTriggerExit2D(Collider2D other) {
            Platform platform = other.GetComponent<Platform>();

            if (platform != null && platform.IsUpper() && Mathf.Approximately(platform.z, machine.z)) {
                posZ = posZ + machine.z;
                machine.z = platform.opposite.z;
                posZ = posZ - machine.z;

                Vector2 pos = machine.transform.position;
                pos.y += platform.opposite.z - platform.z;
                machine.rb.MovePosition(pos);

                Physics2D.IgnoreCollision(platform.opposite.col, machine.col, false);
            }
        }

        public override PlayerState GetNextState() {
            if (onGround)
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }
}