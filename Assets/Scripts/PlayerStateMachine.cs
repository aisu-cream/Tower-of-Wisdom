using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateMachine : FiniteStateMachine<PlayerStateMachine.PlayerState> {

    [SerializeField] WalkState walkState = new WalkState(PlayerState.Walk);
    [SerializeField] RunState runState = new RunState(PlayerState.Run);
    [SerializeField] DashState dashState = new DashState(PlayerState.Dash);

    private static Rigidbody2D rb;

    public enum PlayerState {
        Idle,
        Walk,
        Run,
        Dash
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        rb = GetComponent<Rigidbody2D>();

        AddState(PlayerState.Idle, new IdleState(PlayerState.Idle));
        AddState(PlayerState.Walk, walkState);
        AddState(PlayerState.Run, runState);
        AddState(PlayerState.Dash, dashState);

        SetCurrentState(PlayerState.Idle);

        base.Start();
    }

    private class IdleState : BaseState<PlayerState> {
        public IdleState(PlayerState stateKey) : base(stateKey) { }

        public override void EnterState() {
            rb.linearVelocity = Vector3.zero;
        }

        public override void UpdateState() { }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(KeyCode.Space))
                return PlayerState.Dash;
            else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                if (Input.GetKey(KeyCode.LeftShift))
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

        public float targetSpeed = 5;
        public float accelRate = 6;
        public float decelRate = 6;

        protected Vector2 moveDir;
        protected Vector2 moveForce;
        
        public WalkState(PlayerState stateKey) : base(stateKey, UpdateMode.FixedUpdate) {
            moveDir = Vector2.zero;
            moveForce = Vector2.zero;
        }
        
        public override void UpdateState() {
            Move(1);
        }

        protected void Move(float lerpAmount) {
            // accelerate player to the target velocity
            moveDir.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveDir.Normalize();

            float horTargetSpeed = Mathf.Lerp(rb.linearVelocity.x, moveDir.x * targetSpeed, lerpAmount);
            float vertTargetSpeed = Mathf.Lerp(rb.linearVelocity.y, moveDir.y * targetSpeed, lerpAmount);

            float horForce = horTargetSpeed - rb.linearVelocity.x;
            float vertForce = vertTargetSpeed - rb.linearVelocity.y;

            if (Vector2.Dot(moveDir, rb.linearVelocity) > 0) {
                horForce *= accelRate;
                vertForce *= accelRate;
            }
            else {
                horForce *= decelRate;
                vertForce *= decelRate;
            }

            moveForce.Set(horForce, vertForce);
            rb.AddForce(moveForce, ForceMode2D.Force);
        }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(KeyCode.Space))
                return PlayerState.Dash;
            else if (moveDir.magnitude == 0 && rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && Input.GetKey(KeyCode.LeftShift))
                return PlayerState.Run;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class RunState : WalkState {
        public RunState(PlayerState stateKey) : base(stateKey) { }

        public override PlayerState GetNextState() {
            if (Input.GetKeyDown(KeyCode.Space))
                return PlayerState.Dash;
            else if (moveDir.magnitude == 0 && rb.linearVelocity.magnitude <= 0.1f)
                return PlayerState.Idle;
            else if (moveDir.magnitude != 0 && !Input.GetKey(KeyCode.LeftShift))
                return PlayerState.Walk;
            else
                return stateKey;
        }
    }

    [Serializable]
    private class DashState : WalkState
    {
        public float dashSpeed = 30;
        public float dashDuration = 0.3f;
        [Range(0, 1)] public float dashRunLerpAmount = 0.5f;
        public Vector2 defaultDashDirection = new Vector2(1, 0);
        private float dashTime;

        public DashState(PlayerState stateKey) : base(stateKey) { }

        public override void EnterState() {
            Vector2 temp = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (temp.magnitude == 0)
                temp = defaultDashDirection;

            temp = temp.normalized;
            temp = temp * dashSpeed - rb.linearVelocity;

            rb.AddForce(temp, ForceMode2D.Impulse);
            dashTime = 0;
        }

        public override void UpdateState() {
            Move(dashRunLerpAmount);
            dashTime += Time.fixedDeltaTime;
        }

        public override PlayerState GetNextState() {
            if (dashTime > dashDuration) {
                if (!Input.GetKey(KeyCode.LeftShift))
                    return PlayerState.Run;
                else 
                    return PlayerState.Walk;
            }

            return stateKey;
        }
    }
}