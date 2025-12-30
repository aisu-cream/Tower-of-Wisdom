# nullable enable

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
        searchFilter.SetLayerMask(searchMask);

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
                IEntity targetEntity = entities[i].GetComponent<IEntity>();

                Vector3 pos = entity.GetVisionPosition();
                Vector3 diff = targetEntity.GetVisionPosition() - pos;

                if (Vector3.Dot(entity.GetFacingDirection(), diff) >= 0 && Physics.Raycast(pos, diff, out RaycastHit hit, entity.targetDetectRadius, entity.confirmMask)) {
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

        [Min(0)] public float jumpStrength = 15f;
        [Min(0)] public float jumpNlagTime = 0.15f;

        private float distanceFromTarget;
        private bool jumpCommitted;
        private float jumpStartTimer = 0;

        private EnemyAI entity;

        public ChaseState(EnemyState stateKey, EnemyAI entity) : base(stateKey) { this.entity = entity; }

        public override void EnterState() {
            distanceFromTarget = 0;
            jumpStartTimer = 0;
        }

        public override void UpdateState() {
            if (!entity.target!.IsAlive()) {
                entity.target = null;
                return;
            }
            
            entity.agent.nextPosition = entity.GetPosition();
            Vector3 pos = entity.target.GetPosition();

            if (Physics.Raycast(pos + 0.1f * Vector3.up, Vector3.down, out RaycastHit hit))
                entity.agent.SetDestination(hit.point);
            else
                entity.agent.SetDestination(pos);

            distanceFromTarget = (pos - entity.GetPosition()).magnitude;
        }

        public override void FixedUpdateState() {
            // recover if agent is on the wrong navmesh surface
            NavMeshHit hit;
            float divergence = Vector3.Distance(entity.agent.nextPosition, entity.GetPosition());

            if (!entity.agent.isStopped && divergence > 0.5f && entity.IsGrounded() && NavMesh.SamplePosition(entity.GetPosition(), out hit, 0.4f, entity.agent.areaMask)) {
                entity.agent.ResetPath();
                entity.agent.Warp(hit.position);
            }

            // move the agent accordingly
            Vector2 dir = Vector2.zero;
            float speed = targetSpeed;

            if (!entity.agent.isOnOffMeshLink) {
                // accelerate only when the agent is not near the target so that it can stop exactly on the target
                if (entity.agent.remainingDistance > entity.GetHorizontalVelocity().magnitude / accelRate + 0.1f) {
                    Vector3 planar = Vector3.ProjectOnPlane(entity.agent.desiredVelocity, Vector3.up);
                    dir = new Vector2(planar.x, planar.z);
                }
            }
            else {
                // perform custom jump logic
                Vector3 dir3D = entity.agent.currentOffMeshLinkData.endPos - entity.GetPosition();
                dir = new Vector2(dir3D.x, dir3D.z);

                Vector3 originalDir3D = entity.agent.currentOffMeshLinkData.endPos - entity.agent.currentOffMeshLinkData.startPos;
                Vector2 originalDir = new Vector2(originalDir3D.x, originalDir3D.z);

                if (Vector2.Dot(dir, originalDir) < 0)
                    dir = Vector2.zero;

                entity.agent.isStopped = true;

                if (entity.IsGrounded()) {
                    if (jumpCommitted) {
                        entity.agent.CompleteOffMeshLink();
                        jumpStartTimer = 0f;
                        entity.agent.isStopped = false;
                        jumpCommitted = false;
                    }
                    else {
                        speed = 0;
                        jumpStartTimer += Time.fixedDeltaTime;

                        if (jumpStartTimer >= jumpNlagTime) {
                            entity.Jump(jumpStrength);
                            jumpCommitted = true;
                        }
                    }
                }
            }

            // move the entity
            if (entity.IsGrounded())
                entity.Move(dir, speed, accelRate, decelRate, 1f);
            else
                entity.Float(dir, speed, accelRate, decelRate, 1f);
        }

        public override void ExitState() {
            entity.agent.ResetPath();
        }

        public override EnemyState GetNextState() {
            if (entity.target == null || !entity.agent.isOnOffMeshLink && distanceFromTarget > entity.targetDetectRadius * 2)
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