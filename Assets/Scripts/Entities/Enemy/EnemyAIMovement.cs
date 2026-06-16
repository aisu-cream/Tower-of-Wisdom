using UnityEngine;
using UnityEngine.AI;
using AbilitySystem;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(EntityController))]
public class EnemyAIMovement : MonoBehaviour {

    #region Fields
    EntityController controller;
    StateMachine stateMachine;
    NavMeshAgent agent;
    Animator animator;

    IEntity self;
    IAbility ability;
    IEntity target = null;
    ContactFilter2D searchFilter = new();

    [Header("Locomotion Settings")]
    [SerializeField] protected MovementSettings idleSettings = new(4, 8, 8);
    [SerializeField] protected MovementSettings chaseSettings = new(6, 6, 6);

    [Header("Air Settings")]
    [Min(0)] public float jumpStrength = 15f;
    [Min(0)] public float jumpNlagTime = 0.15f;
    bool jumpCommitted;
    float jumpStartTimer = 0;

    [Header("Logic Controller")]
    [SerializeField, Min(0)] float targetDetectRadius = 5f;
    [SerializeField, Min(0)] int maxSearchConstraint = 3;
    [SerializeField] LayerMask searchMask;
    [SerializeField] LayerMask confirmMask;
    float distanceFromTarget;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        self = GetComponent<IEntity>();
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<EntityController>();
        animator = GetComponent<Animator>();
        ability = GetComponent<IAbility>();

        stateMachine = new();

        agent.updatePosition = false;
        agent.updateRotation = false;
        
        searchFilter.SetLayerMask(searchMask);
    }

    void Start() {
        var idleState = new IdleState(controller, self, this);
        var chaseState = new ChaseState(controller, self, this);
        var attackState = new AttackState(controller, self, this);

        At(idleState, chaseState, new FuncPredicate(() => target != null));
        At(chaseState, idleState, new FuncPredicate(() => !agent.isOnOffMeshLink && distanceFromTarget > targetDetectRadius));
        At(chaseState, attackState, new FuncPredicate(() => ability.CanCast() && distanceFromTarget <= 1));
        At(attackState, chaseState, new FuncPredicate(() => ability.GetState() == AbilityState.inactive && distanceFromTarget <= targetDetectRadius));
        At(attackState, idleState, new FuncPredicate(() => ability.GetState() == AbilityState.inactive && distanceFromTarget > targetDetectRadius));

        stateMachine.SetState(idleState);
    }

    void Update() {
        stateMachine.Update();
    }

    void FixedUpdate() {
        stateMachine.FixedUpdate();
    }

    void At(IState from, IState to, FuncPredicate condition) => stateMachine.AddTransition(from, to, condition);

    void Any(IState to, FuncPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    void OnValidate() => searchFilter.SetLayerMask(searchMask);

    class IdleState : BaseState {

        EnemyAIMovement movement;
        bool targetFound;

        public IdleState(EntityController controller, IEntity self, EnemyAIMovement movement) : base(controller, self) { 
            this.movement = movement; 
        }

        public override void OnEnter() {
            movement.target = null;
            targetFound = false;
        }

        public override void FixedUpdate() {
            controller.Move(Vector2.zero, 0, 0, movement.idleSettings.decel, 0.5f);

            if (targetFound)
                return;

            Collider[] entities = Physics.OverlapSphere(movement.transform.position, movement.targetDetectRadius, movement.searchMask);

            IEntity closestVisibleEntity = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < entities.Length && i < movement.maxSearchConstraint; i++) {
                IEntity targetEntity = entities[i].GetComponent<IEntity>();

                Vector3 pos = self.GetPosition();
                Vector3 diff = targetEntity.GetPosition() - pos;

                if (Vector3.Dot(self.GetLookDirection(), diff) >= 0 && Physics.Raycast(pos, diff, out RaycastHit hit, movement.targetDetectRadius, movement.confirmMask)) {
                    float distance = diff.magnitude;

                    if (hit.transform.gameObject == entities[i].transform.gameObject && minDistance > distance) {
                        closestVisibleEntity = entities[i].GetComponent<IEntity>();
                        targetFound = true;
                        minDistance = distance;
                    }
                }
            }

            movement.target = closestVisibleEntity;
        }
    }

    class ChaseState : BaseState {

        EnemyAIMovement movement;

        public ChaseState(EntityController controller, IEntity self, EnemyAIMovement movement) : base(controller, self) { this.movement = movement; }

        public override void OnEnter() {
            movement.jumpStartTimer = 0;
            movement.animator.SetBool("Walk", true);
        }

        public override void Update() {
            if (!movement.target.IsAlive()) {
                movement.target = null;
                return;
            }

            movement.agent.nextPosition = movement.transform.position;
            Vector3 pos = movement.target.GetPosition();

            if (Physics.Raycast(pos + 0.1f * Vector3.up, Vector3.down, out RaycastHit hit))
                movement.agent.SetDestination(hit.point);
            else
                movement.agent.SetDestination(pos);

            movement.distanceFromTarget = (pos - self.GetPosition()).magnitude;
        }

        public override void FixedUpdate() {
            // recover if agent is on the wrong navmesh surface
            float divergence = Vector3.Distance(movement.agent.nextPosition, self.GetPosition());

            if (divergence > 0.2f && controller.isGrounded) {
                movement.agent.updatePosition = false;
                movement.agent.Warp(self.GetPosition());
            }

            // prevent entity from moving too much
            Vector3 dir = Vector3.zero;
            float speed = movement.chaseSettings.targetSpeed;

            if (!movement.agent.isOnOffMeshLink) {
                // accelerate only when the agent is not near the target so that it can stop exactly on the target
                if (movement.agent.remainingDistance > controller.GetHorizontalVelocity().magnitude / movement.chaseSettings.accel + 0.1f)
                    dir = Vector3.ProjectOnPlane(movement.agent.desiredVelocity, Vector3.up);
            }

            // perform custom jump logic
            else if (controller.isGrounded) {
                if (movement.jumpCommitted) {
                    movement.agent.CompleteOffMeshLink();
                    movement.jumpStartTimer = 0f;
                    movement.jumpCommitted = false;
                }
                else {
                    dir = movement.agent.currentOffMeshLinkData.endPos - self.GetPosition();
                    Vector3 originalDir = movement.agent.currentOffMeshLinkData.endPos - movement.agent.currentOffMeshLinkData.startPos;

                    if (Vector2.Dot(dir, originalDir) < 0)
                        dir = Vector3.zero;

                    speed = 0;
                    movement.jumpStartTimer += Time.fixedDeltaTime;

                    if (movement.jumpStartTimer >= movement.jumpNlagTime) {
                        controller.Jump(movement.jumpStrength);
                        movement.jumpCommitted = true;
                    }
                }
            }

            // move the entity
            if (controller.isGrounded)
                controller.Move(dir, speed, movement.chaseSettings.accel, movement.chaseSettings.decel, 1f);
            else
                controller.Float(dir, speed, movement.chaseSettings.accel, movement.chaseSettings.decel, 1f);
        }

        public override void OnExit() {
            movement.agent.ResetPath();
            movement.animator.SetBool("Walk", false);
        }
    }

    class AttackState : BaseState {
        
        EnemyAIMovement movement;

        public AttackState(EntityController controller, IEntity self, EnemyAIMovement movement) : base(controller, self) => this.movement = movement;

        public override void OnEnter() => movement.ability.TryCast();
        public override void FixedUpdate() {
            controller.Move(Vector3.zero, 0, 8, 8, 1);
        }
    }
}