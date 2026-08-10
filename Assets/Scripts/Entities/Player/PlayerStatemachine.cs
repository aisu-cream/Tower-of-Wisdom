using UnityEngine;

struct MovementSettings {

}

[RequireComponent(typeof(EntityController)), RequireComponent(typeof(IEntity)), RequireComponent(typeof(Animator))]
public class PlayerStatemachine : MonoBehaviour {

    #region Fields
    [SerializeField] InputReader inputReader;
    EntityController controller;
    StateMachine stateMachine;
    Animator animator;

    IEntity entity;

    static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    static readonly int LookdirYHash = Animator.StringToHash("lookdir_y");
    static readonly int LookdirXHash = Animator.StringToHash("lookdir_x");
    static readonly int SpeedHash = Animator.StringToHash("speed");

    [Header("Locomotion Settings")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float walkSharpness = 8f;

    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float sprintSharpness = 8f;

    [Header("Air Settings")]
    [Min(0)] public float jumpBufferTime = 0.1f;
    [Range(0, 1)] public float airControlDegree = 0.4f;
    #endregion

    void Awake() {
        entity = GetComponent<IEntity>();
        controller = GetComponent<EntityController>();
        animator = GetComponent<Animator>();
        stateMachine = new();

        GroundState groundState = new(this);
        JumpState jumpState = new(this);
        FallState fallState = new(this);

        stateMachine.AddTransition(groundState, fallState, new FuncPredicate(() => !controller.IsStableOnGround()));
        stateMachine.AddTransition(groundState, jumpState, new FuncPredicate(() => inputReader.JumpHeld));

        stateMachine.AddTransition(jumpState, groundState, new FuncPredicate(() => controller.IsStableOnGround()));

        stateMachine.AddTransition(fallState, groundState, new FuncPredicate(() => controller.IsStableOnGround()));
        stateMachine.AddTransition(fallState, jumpState, new FuncPredicate(()   => inputReader.JumpHeld && fallState.CanEnterCoyoteTime()));

        stateMachine.SetState(fallState);
    }

    void Update() {
        stateMachine.Update();

        Vector3 inputDir = inputReader.InputDirection;

        animator.SetFloat(SpeedHash, controller.GetVelocity().magnitude);

        if (inputDir != Vector3.zero) {
            controller.LookAtLocal(inputDir);
            animator.SetFloat(LookdirXHash, inputDir.x);
            animator.SetFloat(LookdirYHash, inputDir.z);
        }
        
        animator.SetBool(IsWalkingHash, false);
        animator.SetBool(IsRunningHash, false);

        if (inputDir != Vector3.zero)
            animator.SetBool(inputReader.RunHeld ? "isRunning" : "isWalking", true);
    }

    void FixedUpdate() => stateMachine.FixedUpdate();

    class GroundState : IState {

        readonly PlayerStatemachine super;

        public GroundState(PlayerStatemachine super) {
            this.super = super;
        }

        public void FixedUpdate() {
            InputReader reader = super.inputReader;

            MovementCommand command = new() {
                MoveAxisForward = reader.InputDirection.z,
                MoveAxisRight = reader.InputDirection.x,
                MaxMoveSpeed = reader.RunHeld ? super.sprintSpeed : super.walkSpeed,
                MoveSharpness = reader.RunHeld ? super.sprintSharpness : super.walkSharpness,
                MoveControlDegree = 1
            };

            super.controller.SetMovement(command);
        }
    }

    class JumpState : IState {

        readonly PlayerStatemachine super;

        [Min(0)] public float jumpSpeed = 10f;

        public JumpState(PlayerStatemachine super) {
            this.super = super;
        }

        public void OnEnter() {
            super.controller.Jump(jumpSpeed);
        }

        public void FixedUpdate() {
            InputReader reader = super.inputReader;

            MovementCommand command = new() {
                MoveAxisForward = reader.InputDirection.z,
                MoveAxisRight = reader.InputDirection.x,
                MaxMoveSpeed = super.walkSpeed,
                MoveSharpness = super.walkSharpness,
                MoveControlDegree = super.airControlDegree
            };

            super.controller.SetMovement(command);
        }
    }

    class FallState : IState {

        readonly PlayerStatemachine super;

        [Min(0)] public float coyoteTime = 0.15f;

        float coyoteTimeCounter = 0;

        public FallState(PlayerStatemachine super) {
            this.super = super;
        }

        public bool CanEnterCoyoteTime() => coyoteTimeCounter + coyoteTime > Time.time;

        public void OnEnter() {
            coyoteTimeCounter = Time.time;
        }

        public void FixedUpdate() {
            InputReader reader = super.inputReader;

            MovementCommand command = new() {
                MoveAxisForward = reader.InputDirection.z,
                MoveAxisRight = reader.InputDirection.x,
                MaxMoveSpeed = super.walkSpeed,
                MoveSharpness = super.walkSharpness,
                MoveControlDegree = super.airControlDegree
            };

            super.controller.SetMovement(command);
        }
    }
}