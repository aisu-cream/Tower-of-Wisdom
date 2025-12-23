#nullable enable

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : Entity<EnemyAI.EnemyState> {

    [Header("Logic Controller")]
    [SerializeField, Min(0)] private float targetDetectRadius = 5f;
    [SerializeField, Min(0)] private int maxSearchConstraint = 10;
    [SerializeField] LayerMask searchMask;
    [SerializeField] LayerMask confirmMask;

    [Header("State Controller")]
    [SerializeField] IdleState idleState;
    [SerializeField] ChaseState chaseState;
    [SerializeField] CombatState combatState;

    private IEntity? target = null!;
    private NavMeshAgent agent = null!;

    private ContactFilter2D searchFilter = new();

    public enum EnemyState {
        Idle,
        Chase,
        Combat
    }

    public EnemyAI() {
        idleState = new IdleState(EnemyState.Idle, this);
        chaseState = new ChaseState(EnemyState.Chase, this);
        combatState = new CombatState(EnemyState.Combat, this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = false;
        agent.updateRotation = false;

        AddState(EnemyState.Idle, idleState);
        AddState(EnemyState.Chase, chaseState);
        AddState(EnemyState.Combat, combatState);

        SetCurrentState(EnemyState.Idle);

        base.Start();
    }

    void OnValidate() {
        searchFilter.SetLayerMask(searchMask);
    }

    [Serializable]
    private class IdleState : MoveState<EnemyAI> {

        private Coroutine searchRoutine = null!;
        private bool isCoroutineRunning = false;

        public IdleState(EnemyState stateKey, EnemyAI entity) : base(stateKey, entity) { }

        public override void EnterState() {
            entity.target = null;
            searchRoutine = entity.StartCoroutine(Search());
        }

        public override void UpdateState() {
            if (!isCoroutineRunning)
                searchRoutine = entity.StartCoroutine(Search());
        }

        public override void FixedUpdateState() {
            Move(Vector2.zero, 0.5f);
        }

        public override EnemyState GetNextState() {
            if (entity.target != null)
                return EnemyState.Chase;
            return stateKey;
        }

        public override void ExitState() {
            entity.StopCoroutine(searchRoutine);
        }

        private IEnumerator Search() {
            isCoroutineRunning = true;

            Collider[] entities = Physics.OverlapSphere(entity.transform.position, entity.targetDetectRadius, entity.searchMask);

            IEntity closestVisibleEntity = null!;
            float minDistance = float.MaxValue;

            for (int i = 0; i < entities.Length && i < entity.maxSearchConstraint; i++) {
                Vector3 diff = entities[i].transform.position - entity.transform.position;
                RaycastHit hit;

                if (Vector3.Dot(entity.GetFacingDirection(), diff) >= 0 && Physics.Raycast(entity.transform.position, diff, out hit, entity.targetDetectRadius, entity.confirmMask)) {
                    float distance = diff.magnitude;

                    if (hit.transform.gameObject == entities[i].transform.gameObject && minDistance > distance) {
                        closestVisibleEntity = entities[i].GetComponent<IEntity>();
                        minDistance = distance;
                    }
                }

                yield return null;
            }

            entity.target = closestVisibleEntity;
            isCoroutineRunning = false;
        }
    }

    [Serializable]
    private class ChaseState : MoveState<EnemyAI> {

        public float stoppingDistance;
        private float distanceFromTarget;

        public ChaseState(EnemyState stateKey, EnemyAI entity) : base(stateKey, entity) { }

        public override void EnterState() {
            distanceFromTarget = 0;
        }

        public override EnemyState GetNextState() {
            if (distanceFromTarget > entity.targetDetectRadius)
                return EnemyState.Idle;
            return stateKey;
        }

        public override void UpdateState() {
            entity.agent.nextPosition = entity.GetPosition();
            entity.agent.SetDestination(entity.target!.GetPosition());
            distanceFromTarget = (entity.target!.GetPosition() - entity.GetPosition()).magnitude;
        }

        public override void FixedUpdateState() {
            Vector2 dir = Vector2.zero;

            if (entity.agent.remainingDistance > entity.GetHorizontalVelocity().magnitude / accelRate + 0.1f)
                dir = Quaternion.AngleAxis(-90, Vector3.right) * Vector3.ProjectOnPlane(entity.agent.desiredVelocity, Vector3.up);

            Move(dir, 1f);
        }

        public override void ExitState() {
            entity.agent.ResetPath();
        }
    }

    [Serializable]
    private class CombatState : MoveState<EnemyAI> { 

        public CombatState(EnemyState stateKey, EnemyAI entity) : base(stateKey, entity) { }

        public override EnemyState GetNextState() {
            return stateKey;
        }

        public override void FixedUpdateState() {
            Move(Vector2.zero, 0.75f);
        }
    }
}
