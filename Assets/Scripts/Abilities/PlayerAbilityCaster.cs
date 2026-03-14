using UnityEngine;

[RequireComponent(typeof(TargetingManager))]
public class PlayerAbilityCaster : MonoBehaviour {
    [SerializeField] Ability[] abilities;
    [SerializeField] TargetingManager targetingManager;

    void Update() {
        for (int i = 0; i < abilities.Length; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                abilities[i].Target(targetingManager);
        }
    }
}
