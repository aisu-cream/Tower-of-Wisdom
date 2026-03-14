using UnityEngine;

public class TargetingManager : MonoBehaviour {
    TargetingStrategy currentStrategy;

    void Update() {
        if (currentStrategy != null && currentStrategy.IsTargeting)
            currentStrategy.Update();
    }

    public void SetCurrentStrategy(TargetingStrategy strategy) => currentStrategy = strategy;
    public void ClearCurrentStrategy() => currentStrategy = null;
}