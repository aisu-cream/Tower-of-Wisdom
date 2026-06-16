using UnityEngine;

public enum AbilityState {
    startup,
    active,
    inactive
}

namespace AbilitySystem {
    public interface IAbility {
        float GetCooldownRemaining();
        float GetCooldownDuration();
        AbilityState GetState();
        IEntity GetCaster();
        bool CanCast();
        void TryCast();
        // can cancel must be more detailed, i.e. can cancel if this action is performed?
        //bool CanCancel();
        //void TryCancel();
    }
}