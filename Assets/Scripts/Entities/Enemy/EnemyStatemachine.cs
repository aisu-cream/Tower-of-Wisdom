using AbilitySystem;
using System;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(EntityController)), RequireComponent(typeof(Animator)), RequireComponent(typeof(IEntity))]
public class EnemyStatemachine : MonoBehaviour {

    #region Fields
    StateMachine stateMachine;

    IEntity entity;
    EntityController controller;
    NavMeshAgent agent;
    Animator animator;

    [SerializeField, Min(0)] float searchRadius = 5f;
    [SerializeField] LayerMask searchMask;
    [SerializeField] LayerMask occlusionMask;

    [SerializeField] ChaseState chaseState;

    IEntity target = null;
    #endregion

    EnemyStatemachine() {
        chaseState = new(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        stateMachine = new();

        entity = GetComponent<IEntity>();
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<EntityController>();
        animator = GetComponent<Animator>();
        
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Start() {
        Bite bite = new(entity, controller);

        IdleState idleState = new(this);
        AbilityUseState biteState = new(this, bite);

        stateMachine.AddTransition(idleState, chaseState, new FuncPredicate(() => target != null));
        stateMachine.AddTransition(chaseState, idleState, new FuncPredicate(() => target == null));
        stateMachine.AddTransition(chaseState, biteState, new FuncPredicate(() => target != null && bite.CanExecute() && Vector3.Distance(target.GetClosestPoint(entity.GetCenterPosition()), entity.GetCenterPosition()) <= 2));
        stateMachine.AddTransition(biteState, chaseState, new FuncPredicate(() => target != null && bite.GetState() == AbilityState.inactive));
        stateMachine.AddTransition(biteState, idleState,  new FuncPredicate(() => target == null && bite.GetState() == AbilityState.inactive));

        stateMachine.SetState(idleState);
    }

    void Update() {
        stateMachine.Update();
    }

    void FixedUpdate() {
        stateMachine.FixedUpdate();
    }

    [Serializable]
    class IdleState : IState {

        readonly EnemyStatemachine super;
        readonly Collider[] hits = new Collider[10];

        bool targetFound;

        public IdleState(EnemyStatemachine super) { 
            this.super = super;
        }

        public void OnEnter() {
            super.target = null;
            targetFound = false;
        }

        public void Update() {
            MovementCommand command = new() {
                MoveAxisForward = 0,
                MoveAxisRight = 0,
                MaxMoveSpeed = 0,
                MoveSharpness = 6,
                MoveControlDegree = 1
            };

            super.controller.SetMovement(command);
        }

        public void FixedUpdate() {
            if (targetFound)
                return;

            int count = Physics.OverlapSphereNonAlloc(super.transform.position, super.searchRadius, hits, super.searchMask);

            IEntity closestFoundEntity = null;
            float minDistance = float.MaxValue;

            Vector3 center = super.entity.GetCenterPosition();
            Vector3 sightVector = super.entity.GetLookDirection();

            for (int i = 0; i < count; i++) {
                IEntity candidate = hits[i].GetComponent<IEntity>();

                // check if the candidate is a valid target
                if (candidate == null || !candidate.IsAlive() || candidate == super.entity) 
                    continue;

                Vector3 displacement = candidate.GetCenterPosition() - center;

                // check if the candidate is in sight
                if (Vector3.Dot(sightVector, displacement) < 0) 
                    continue;

                float distance = displacement.magnitude;

                // check if the candidate is not occluded and is closer than the current closest one
                if (minDistance > distance && !Physics.Raycast(center, displacement, distance, super.occlusionMask)) {
                    closestFoundEntity = candidate;
                    minDistance = distance;
                    targetFound = true;
                }
            }

            super.target = closestFoundEntity;
        }
    }

    [Serializable]
    class ChaseState : IState {

        readonly EnemyStatemachine super;

        [Min(0), SerializeField] float lookaheadTime = 2f;
        [Min(0), SerializeField] float outOfSightGracePeriod = 3f;

        Vector3 targetLastPosition;
        Vector3 targetLastVelocity;

        // timer indicating the period it takes until the target becomes completely out of sight
        float gracePeriodTimer;
        
        static readonly int WalkHash = Animator.StringToHash("Walk");

        public ChaseState(EnemyStatemachine super) { 
            this.super = super;
        }

        public void OnEnter() {
            gracePeriodTimer = outOfSightGracePeriod;
            targetLastPosition = super.target.GetBottomPosition();
            super.agent.speed = 5;
            super.animator.SetBool(WalkHash, true);
        }

        public void Update() {
            if (!super.target.IsAlive()) {
                super.target = null;
                return;
            }

            Vector3 pos = super.entity.GetCenterPosition();
            Vector3 targetPos = super.target.GetCenterPosition();
            Vector3 targetBottomPos = super.target.GetBottomPosition();
            Vector3 diff = targetPos - pos;

            // update internal target position and velocity if the target is still in sight
            if (gracePeriodTimer > 0) {
                targetLastPosition = targetBottomPos;
                targetLastVelocity = super.target.GetVelocity();
                gracePeriodTimer -= Time.deltaTime;
            }

            // reset the timer if the target is visible
            if (!Physics.Raycast(pos, diff, diff.magnitude, super.occlusionMask))
                gracePeriodTimer = outOfSightGracePeriod;

            // predict where the target must be after lookahead time of time
            Vector3 targetPredictedPos = targetLastPosition + lookaheadTime * targetLastVelocity;
            bool succeed = super.agent.SetDestination(targetPredictedPos);

            if (!succeed)
                super.agent.SetDestination(targetLastPosition);

            super.controller.LookAtWorld(targetLastPosition - pos);
        }

        public void FixedUpdate() {
            super.agent.nextPosition = super.transform.position;
            Vector3 desiredVel = super.agent.desiredVelocity;

            // assign movement to the controller
            MovementCommand command = new() {
                MoveAxisForward = Vector3.Dot(desiredVel, super.controller.CharacterForward),
                MoveAxisRight = Vector3.Dot(desiredVel, super.controller.CharacterRight),
                MaxMoveSpeed = desiredVel.magnitude,
                MoveSharpness = 6,
                MoveControlDegree = 1
            };

            super.controller.SetMovement(command);
        }

        public void OnExit() {
            super.agent.ResetPath();
            super.animator.SetBool("Walk", false);
        }
    }

    class AbilityUseState : IState {

        readonly int AttackHash = Animator.StringToHash("Attack");
        readonly EnemyStatemachine super;
        readonly Ability ability;

        public AbilityUseState(EnemyStatemachine super, Ability ability) {
            this.super = super;
            this.ability = ability;
        }

        public void OnEnter() {
            ability.Execute();
            super.animator.SetBool(AttackHash, true);
        }

        public void Update() {
            ability.Tick();
            if (super.target == null || !super.target.IsAlive()) super.target = null;
        }

        public void OnExit() {
            super.animator.SetBool(AttackHash, false);
        }
    }
}