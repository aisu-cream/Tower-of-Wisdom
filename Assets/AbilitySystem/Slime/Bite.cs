using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    using Hitbox;

    public class Bite : Ability {

        readonly Timeline timeline;
        readonly HitboxSet biteHitbox = new();
        readonly List<IEntity> detectedEntities = new();

        public Bite(IEntity owner, EntityController controller) : base("Bite", null, owner, controller, 3) {
            SphereHitbox hitbox = new(Vector3.zero, 0.75f);

            MovementCommand stopCommand = new() {
                MoveSharpness = 10,
                MoveControlDegree = 1
            };

            List<TimelineEvent> events = new() {
                new(0f,    () => GetController().SetMovement(stopCommand)),
                new(5/8f,  () => SetState(AbilityState.active)),
                new(5/8f,  () => biteHitbox.Enable(hitbox)),
                new(5/8f,  () => GetController().SetMovement(CreateApproachCommand())),
                new(6/8f,  () => GetController().SetMovement(stopCommand)),
                new(7/8f,  () => biteHitbox.Disable(hitbox)),
                new(7/8f,  () => SetState(AbilityState.recovery)),
                new(10/8f, () => SetState(AbilityState.inactive))
            };

            timeline = new(events);   
        }

        MovementCommand CreateApproachCommand() {
            return new() {
                MoveAxisForward = GetController().GetLocalLookDirection().z,
                MoveAxisRight = GetController().GetLocalLookDirection().x,
                MaxMoveSpeed = 15,
                MoveSharpness = 5,
                MoveControlDegree = 1
            };
        }

        public override bool CanExecute() {
            return GetCurrentCooldownRemaining() <= 0 && GetState() == AbilityState.inactive;
        }

        public override void Execute() {
            if (!CanExecute())
                return;

            SetState(AbilityState.anticipation);
            StartCooldown();
        }

        public override void Tick() {
            if (GetState() == AbilityState.inactive)
                return;

            timeline.Tick(Time.deltaTime);

            detectedEntities.Clear();
            biteHitbox.Detect(GetOwner().GetCenterPosition(), Quaternion.identity, detectedEntities);

            Debug.Log(GetOwner().GetCenterPosition());

            foreach (IEntity entity in detectedEntities) {
                if (entity == GetOwner()) continue;
                entity.TakeDamage(5);
                entity.AlertThreat(GetOwner());
            }

            if (GetState() == AbilityState.inactive) {
                timeline.Reset();
                biteHitbox.Reset();
            }
        }

        public override void Cancel() {
            biteHitbox.Reset();
            timeline.Reset();
            SetState(AbilityState.inactive);
        }
    }
}
