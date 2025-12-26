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

    void OnDrawGizmos() {
        // draw navmesh agent path
        if (!agent || !agent.hasPath)
            return;

        Gizmos.color = Color.green;

        Vector3[] corners = agent.path.corners;

        for (int i = 0; i < corners.Length - 1; i++) {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.08f);
        }
    }

    void OnValidate() {
        searchFilter.SetLayerMask(searchMask);
    }

    [Serializable]
    private class IdleState : BaseState<EnemyState> {

        [Min(0)] public float decelRate = 6;

        private Coroutine searchRoutine = null!;
        private bool isCoroutineRunning = false;

        private EnemyAI entity;

        public IdleState(EnemyState stateKey, EnemyAI entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            entity.target = null;
            searchRoutine = entity.StartCoroutine(Search());
        }

        public override void UpdateState() {
            if (!isCoroutineRunning)
                searchRoutine = entity.StartCoroutine(Search());
        }

        public override void FixedUpdateState() {
            entity.Move(Vector2.zero, 0, 0, decelRate, 0.5f);
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
    private class ChaseState : BaseState<EnemyState> {

        [Min(0)] public float targetSpeed = 6;
        [Min(0)] public float accelRate = 10;
        [Min(0)] public float decelRate = 10;

        public float jumpStrength = 15f;
        [Range(0, 1)] public float airLerpAmount = 0.4f;

        private float distanceFromTarget;

        private float lastY = 0f;
        private float verticalStagnationTimer = 0f;

        private EnemyAI entity;

        public ChaseState(EnemyState stateKey, EnemyAI entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            distanceFromTarget = 0;
            lastY = entity.GetPosition().y;
            verticalStagnationTimer = 0f;
        }

        public override void UpdateState() {
            entity.agent.nextPosition = entity.GetPosition();
            entity.agent.SetDestination(entity.target!.GetPosition());
            distanceFromTarget = (entity.target!.GetPosition() - entity.GetPosition()).magnitude;
        }

        public override void FixedUpdateState() {
            // compensate if agent is on the wrong navmesh surface
            float currentY = entity.GetPosition().y;
            float deltaY = Mathf.Abs(currentY - lastY);

            bool targetAbove = entity.target!.GetPosition().y - currentY > 0.5f;
            bool chasing = entity.agent.hasPath;

            if (targetAbove && chasing) {
                if (verticalStagnationTimer >= 0.2f)
                    RecoverFromNavigationFailure();
                verticalStagnationTimer = (deltaY < 0.02f) ? verticalStagnationTimer + Time.fixedDeltaTime : 0f;                
            }
            else {
                verticalStagnationTimer = 0f;
            }

            lastY = currentY;
            
            // move the agent accordingly
            Vector2 dir = Vector2.zero;

            if (!entity.agent.isOnOffMeshLink) {
                // accelerate only when the agent is not near the target so that it can stop exactly on the target
                if (entity.agent.remainingDistance > entity.GetHorizontalVelocity().magnitude / accelRate + 0.1f) {
                    Vector3 planar = Vector3.ProjectOnPlane(entity.agent.desiredVelocity, Vector3.up);
                    dir = new Vector2(planar.x, planar.z);
                }
            }
            else {
                Vector3 dir3D = entity.agent.currentOffMeshLinkData.endPos - entity.GetPosition();
                dir = new Vector2(dir3D.x, dir3D.z);
                if (entity.IsGrounded() && (dir3D.y > 0 || dir.magnitude > 4f))
                    entity.Jump(jumpStrength);
            }

            if (entity.IsGrounded())
                entity.Move(dir, targetSpeed, accelRate, decelRate, 1f);
            else
                entity.Float(dir, targetSpeed, accelRate, decelRate, airLerpAmount);
        }

        private void RecoverFromNavigationFailure() {
            entity.agent.ResetPath();

            Vector3 pos = entity.GetPosition();
            NavMeshHit hit;

            if (NavMesh.SamplePosition(pos, out hit, 2.0f, entity.agent.areaMask))
                entity.agent.Warp(hit.position);
        }

        public override void ExitState() {
            entity.agent.ResetPath();
        }

        public override EnemyState GetNextState() {
            if (!entity.agent.isOnOffMeshLink && distanceFromTarget > entity.targetDetectRadius * 2)
                return EnemyState.Idle;
            return stateKey;
        }
    }

    [Serializable]
    private class CombatState : BaseState<EnemyState> {

        private EnemyAI entity;

        public CombatState(EnemyState stateKey, EnemyAI entity) : base(stateKey) { this.entity = entity; }

        public override EnemyState GetNextState() {
            return stateKey;
        }
    }
}