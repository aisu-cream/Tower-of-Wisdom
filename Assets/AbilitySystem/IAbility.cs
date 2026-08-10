using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// IAbility is a data shell of all abilities. <br/>
    /// IAbility does not provide operations to change an ability's state. <br/>
    /// The primary use is to display the abilities as UI.
    /// </summary>
    public interface IAbility {
        string GetName();
        Sprite GetIcon();
        IEntity GetOwner();
        float GetCooldown();
        float GetCurrentCooldownRemaining();
    }
}