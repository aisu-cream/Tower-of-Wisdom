using UnityEngine;

[RequireComponent(typeof(TargetingManager))]
public class PlayerAbilityCaster : MonoBehaviour {
    [SerializeField] Ability[] abilities;
    TargetingManager targetingManager;
    IEntity entity;

    void Awake() {
        entity = GetComponent<IEntity>();
        targetingManager = GetComponent<TargetingManager>();
    }

    void Update() {
        for (int i = 0; i < abilities.Length; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                abilities[i].Cast(entity, targetingManager);
        }
    }
}
