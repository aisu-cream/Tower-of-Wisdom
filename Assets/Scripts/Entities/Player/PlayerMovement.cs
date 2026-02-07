using UnityEngine;

[RequireComponent(typeof(EntityController))]
public class PlayerMovement : MonoBehaviour {

    #region Fields
    [SerializeField] InputReader inputReader;
    EntityController controller;
    StateMachine stateMachine;
    Animator animator;

    [Header("Locomotion Settings")]
    [SerializeField] protected MovementSettings walkSettings = new(5, 8, 8);
    [SerializeField] protected MovementSettings sprintSettings = new(8, 10, 10);

    [Header("Air Settings")]
    [Min(0)] public float jumpStrength = 12.5f;
    [Min(0)] public float coyoteTime = 0.15f;
    [Min(0)] public float jumpBufferTime = 0.1f;
    [Range(0, 1)] public float airLerpAmount = 0.4f;
    #endregion

    void Awake() {
        controller = GetComponent<EntityController>();
        stateMachine = new();
        animator = GetComponent<Animator>();

        var groundState = new GroundState(controller, this);
        var jumpState = new JumpState(controller, this);
        var fallState = new FallState(controller, this);

        At(groundState, fallState, new FuncPredicate(() => !controller.isGrounded));
        At(groundState, jumpState, new FuncPredicate(() => inputReader.jumpHeld));

        At(jumpState, groundState, new FuncPredicate(() => controller.isGrounded));

        At(fallState, groundState, new FuncPredicate(() => controller.isGrounded));
        At(fallState, jumpState, new FuncPredicate(() => inputReader.jumpHeld && fallState.CanEnterCoyoteTime()));

        stateMachine.SetState(fallState);
    }

    void At(IState from, IState to, FuncPredicate condition) => stateMachine.AddTransition(from, to, condition);

    void Any(IState to, FuncPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    void Update() {
        stateMachine.Update();

        animator.SetFloat("speed", controller.GetVelocity().magnitude);
        
        Vector3 lookDir = controller.GetLookDirection();
        animator.SetFloat("lookdir_x", lookDir.x);
        animator.SetFloat("lookdir_y", lookDir.z);

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        if (controller.GetMoveDirection() != Vector3.zero)
            animator.SetBool(inputReader.runHeld ? "isRunning" : "isWalking", true);
    }

    void FixedUpdate() => stateMachine.FixedUpdate();

    class GroundState : BaseState {

        PlayerMovement movement;

        public GroundState(EntityController controller, PlayerMovement movement) : base(controller) => this.movement = movement;

        public override void FixedUpdate() {
            if (movement.inputReader.runHeld)
                controller.Move(movement.inputReader.inputDirection, movement.sprintSettings.targetSpeed, movement.sprintSettings.accel, movement.sprintSettings.decel, 1);
            else
                controller.Move(movement.inputReader.inputDirection, movement.walkSettings.targetSpeed, movement.walkSettings.accel, movement.walkSettings.decel, 1);
        }
    }

    class JumpState : BaseState {

        PlayerMovement movement;

        public JumpState(EntityController controller, PlayerMovement movement) : base(controller) => this.movement = movement;

        public override void OnEnter() {
            float jumpStrength = movement.jumpStrength;
            controller.Jump(jumpStrength);
        }

        public override void FixedUpdate() => controller.Float(movement.inputReader.inputDirection, movement.walkSettings.targetSpeed, movement.walkSettings.accel, movement.walkSettings.decel, movement.airLerpAmount);
    }

    class FallState : BaseState {

        PlayerMovement movement;
        float coyoteTimeCounter = 0;

        public FallState(EntityController controller, PlayerMovement movement) : base(controller) => this.movement = movement;

        public bool CanEnterCoyoteTime() => coyoteTimeCounter + movement.coyoteTime > Time.time;

        public override void OnEnter() => coyoteTimeCounter = Time.time;

        public override void FixedUpdate() => controller.Float(movement.inputReader.inputDirection, movement.walkSettings.targetSpeed, movement.walkSettings.accel, movement.walkSettings.decel, movement.airLerpAmount);
    }
}