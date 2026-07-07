using AbilitySystem;
using UnityEngine;

public class PlayerAbilityCaster : MonoBehaviour {
    [SerializeField] Ability[] abilities;
    IEntity entity;

    void Awake() {
        entity = GetComponent<IEntity>();
    }

    void Update() {
        //for (int i = 0; i < abilities.Length; i++) {
        //    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
        //        abilities[i].Cast(entity, targetingManager);
        //}
    }
}
