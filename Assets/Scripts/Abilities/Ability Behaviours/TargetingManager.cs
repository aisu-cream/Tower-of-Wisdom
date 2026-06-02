using UnityEngine;

public class TargetingManager : MonoBehaviour {
    AbilityBehaviour currentStrategy;

    void Update() {
        if (currentStrategy != null && currentStrategy.IsTargeting)
            currentStrategy.Update();
    }

    public void SetCurrentStrategy(AbilityBehaviour strategy) => currentStrategy = strategy;
    public void ClearCurrentStrategy() => currentStrategy = null;
}