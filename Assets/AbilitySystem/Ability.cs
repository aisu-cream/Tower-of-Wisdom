using ImprovedTimers;
using UnityEngine;

namespace AbilitySystem {

    /// <summary>
    /// The lifecycle indicator of subclasses of the Ability class.
    /// </summary>
    public enum AbilityState {
        inactive,
        anticipation,
        active,
        recovery
    }

    /// <summary>
    /// A blueprint of abilities with fixed lifecycle (e.g. basic melee attack, shooting a bullet).
    /// </summary>
    // next idea: Channel ability that stops only when input or resource ends (so indefinite active state).
    public abstract class Ability : IAbility {

        readonly string name;
        readonly Sprite icon;
        readonly IEntity owner;
        readonly EntityController controller;
        readonly float cooldown;

        readonly CountdownTimer cooldownTimer;
        AbilityState currentState;

        public Ability(string name, Sprite icon, IEntity owner, EntityController controller, float cooldown) {
            this.name = name;
            this.icon = icon;
            this.owner = owner;
            this.controller = controller;
            this.cooldown = cooldown;
            cooldownTimer = new CountdownTimer(cooldown);
        }

        public virtual string GetName() => name;
        public virtual Sprite GetIcon() => icon;
        public virtual IEntity GetOwner() => owner;
        public virtual EntityController GetController() => controller;
        public virtual float GetCooldown() => cooldown;
        public virtual float GetCurrentCooldownRemaining() => cooldownTimer.CurrentTime;
        protected virtual void StartCooldown() => cooldownTimer.Start();
        public virtual AbilityState GetState() => currentState;
        protected virtual void SetState(AbilityState state) => currentState = state;

        /// <summary>
        /// Checks if the owner can currently execute the ability.
        /// </summary>
        /// <returns>
        ///     true, if the owner can execute the ability, <br/>
        ///     false, otherwise
        /// </returns>
        public abstract bool CanExecute();

        /// <summary>
        /// Begins the lifecycle of the ability and turns the current state to anticipation state. <br/>
        /// The method will fail If the ability cannot be executed or if the execution is not finished.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Makes the ability's lifecycle proceed by 1 frame. <br/>
        /// The method will properly operate only after Execute method is called. <br/>
        /// Cycles the ability in the order of the anticipation, active, and recovery states.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Immediately cancel the lifecycle of the ability.
        /// </summary>
        public abstract void Cancel();
    }
}