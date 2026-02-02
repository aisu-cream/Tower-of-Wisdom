# nullable enable

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(EntityController))]
public class EnemyAIMovement : MonoBehaviour {

    //#region Fields
    //EntityController controller;
    //StateMachine stateMachine;
    //NavMeshAgent agent;

    //IEntity? target = null!;
    //ContactFilter2D searchFilter = new();

    //[Header("Locomotion Settings")]
    //[SerializeField] protected MovementSettings idleSettings = new(4, 8, 8);
    //[SerializeField] protected MovementSettings chaseSettings = new(6, 6, 6);

    //[Header("Air Settings")]
    //[Min(0)] public float jumpStrength = 15f;
    //[Min(0)] public float jumpNlagTime = 0.15f;
    //bool jumpCommitted;
    //float jumpStartTimer = 0;

    //[Header("Logic Controller")]
    //[SerializeField, Min(0)] float targetDetectRadius = 5f;
    //[SerializeField, Min(0)] int maxSearchConstraint = 10;
    //[SerializeField] LayerMask searchMask;
    //[SerializeField] LayerMask confirmMask;
    //float distanceFromTarget;
    //#endregion

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Awake() {
    //    agent = GetComponent<NavMeshAgent>();
    //    controller = GetComponent<EntityController>();
    //    stateMachine = new();

    //    agent.updatePosition = false;
    //    agent.updateRotation = false;

    //    var idleState = new IdleState(controller, this);
    //    var chaseState = new ChaseState(controller, this);
    //    var combatState = new CombatState(controller, this);

    //    At(idleState, combatState, new FuncPredicate(() => target != null));

    //    At(chaseState, idleState, new FuncPredicate(() => target == null || !agent.isOnOffMeshLink && distanceFromTarget > targetDetectRadius * 2));
    //    At(chaseState, combatState, new FuncPredicate(() => null));

    //    At(combatState, idleState, new FuncPredicate(() => null));
    //    At(combatState, chaseState, new FuncPredicate(() => null));

    //    stateMachine.SetState(idleState);
    //    searchFilter.SetLayerMask(searchMask);
    //}

    //void At(IState from, IState to, FuncPredicate condition) => stateMachine.AddTransition(from, to, condition);

    //void Any(IState to, FuncPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    //void OnValidate() => searchFilter.SetLayerMask(searchMask);
    
    //class IdleState : BaseState {

    //    EnemyAIMovement movement;
    //    Coroutine searchRoutine = null!;

    //    public IdleState(EntityController controller, EnemyAIMovement movement) : base(controller) { this.movement = movement; }

    //    public override void OnEnter() {
    //        entity.target = null;
    //        searchRoutine = movement.StartCoroutine(Search());
    //    }

    //    public override void FixedUpdate() => controller.Move(Vector2.zero, 0, 0, decelRate, 0.5f);

    //    public override void OnExit() => entity.StopCoroutine(searchRoutine);

    //    IEnumerator Search() {
    //        Collider[] entities = Physics.OverlapSphere(entity.transform.position, entity.targetDetectRadius, entity.searchMask);

    //        IEntity closestVisibleEntity = null!;
    //        float minDistance = float.MaxValue;

    //        for (int i = 0; i < entities.Length && i < entity.maxSearchConstraint; i++) {
    //            IEntity targetEntity = entities[i].GetComponent<IEntity>();

    //            Vector3 pos = entity.GetVisionPosition();
    //            Vector3 diff = targetEntity.GetVisionPosition() - pos;

    //            if (Vector3.Dot(entity.GetFacingDirection(), diff) >= 0 && Physics.Raycast(pos, diff, out RaycastHit hit, entity.targetDetectRadius, entity.confirmMask)) {
    //                float distance = diff.magnitude;

    //                if (hit.transform.gameObject == entities[i].transform.gameObject && minDistance > distance) {
    //                    closestVisibleEntity = entities[i].GetComponent<IEntity>();
    //                    minDistance = distance;
    //                }
    //            }

    //            yield return null;
    //        }

    //        entity.target = closestVisibleEntity;
    //        searchRoutine = movement.StartCoroutine(Search());
    //    }
    //}

    //class ChaseState : BaseState {

    //    EnemyAIMovement movement;

    //    public ChaseState(EntityController controller, EnemyAIMovement movement) : base(stateKey) { this.movement = movement; }

    //    public override void OnEnter() {
    //        distanceFromTarget = 0;
    //        jumpStartTimer = 0;
    //    }

    //    public override void Update() {
    //        if (!movement.target!.IsAlive()) {
    //            movement.target = null;
    //            return;
    //        }

    //        movement.agent.nextPosition = movement.transform.position;
    //        Vector3 pos = movement.target.transform.position;

    //        if (Physics.Raycast(pos + 0.1f * Vector3.up, Vector3.down, out RaycastHit hit))
    //            entity.agent.SetDestination(hit.point);
    //        else
    //            entity.agent.SetDestination(pos);

    //        distanceFromTarget = (pos - entity.GetPosition()).magnitude;
    //    }

    //    public override void FixedUpdate() {
    //        // recover if agent is on the wrong navmesh surface
    //        NavMeshHit hit;
    //        float divergence = Vector3.Distance(entity.agent.nextPosition, entity.GetPosition());

    //        if (!entity.agent.isStopped && divergence > 0.5f && entity.IsGrounded() && NavMesh.SamplePosition(entity.GetPosition(), out hit, 0.4f, entity.agent.areaMask)) {
    //            entity.agent.ResetPath();
    //            entity.agent.Warp(hit.position);
    //        }

    //        // move the agent accordingly
    //        Vector2 dir = Vector2.zero;
    //        float speed = targetSpeed;

    //        if (!movement.agent.isOnOffMeshLink) {
    //            // accelerate only when the agent is not near the target so that it can stop exactly on the target
    //            if (entity.agent.remainingDistance > entity.GetHorizontalVelocity().magnitude / accelRate + 0.1f) {
    //                Vector3 planar = Vector3.ProjectOnPlane(entity.agent.desiredVelocity, Vector3.up);
    //                dir = new Vector2(planar.x, planar.z);
    //            }
    //        }
    //        else {
    //            // perform custom jump logic
    //            Vector3 dir3D = entity.agent.currentOffMeshLinkData.endPos - entity.GetPosition();
    //            dir = new Vector2(dir3D.x, dir3D.z);

    //            Vector3 originalDir3D = entity.agent.currentOffMeshLinkData.endPos - entity.agent.currentOffMeshLinkData.startPos;
    //            Vector2 originalDir = new Vector2(originalDir3D.x, originalDir3D.z);

    //            if (Vector2.Dot(dir, originalDir) < 0)
    //                dir = Vector2.zero;

    //            entity.agent.isStopped = true;

    //            if (entity.IsGrounded()) {
    //                if (jumpCommitted) {
    //                    entity.agent.CompleteOffMeshLink();
    //                    jumpStartTimer = 0f;
    //                    entity.agent.isStopped = false;
    //                    jumpCommitted = false;
    //                }
    //                else {
    //                    speed = 0;
    //                    jumpStartTimer += Time.fixedDeltaTime;

    //                    if (jumpStartTimer >= jumpNlagTime) {
    //                        entity.Jump(jumpStrength);
    //                        jumpCommitted = true;
    //                    }
    //                }
    //            }
    //        }

    //        // move the entity
    //        if (entity.IsGrounded())
    //            entity.Move(dir, speed, accelRate, decelRate, 1f);
    //        else
    //            entity.Float(dir, speed, accelRate, decelRate, 1f);
    //    }

    //    public override void OnExit() => entity.agent.ResetPath();
    //}

    //private class CombatState : BaseState {

    //    private EnemyAI entity;

    //    public CombatState(EnemyState stateKey, EnemyAI entity) : base(stateKey) => this.entity = entity;
    //}
}