using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class AOETargeting : AbilityBehaviour {
    [SerializeField] GameObject aoePrefab;
    public float aoeRadius = 5f;
    public LayerMask groundLayerMask = 1;

    GameObject previewInstance;

    public override void Start(Ability ability, IAbilityTrigger trigger, TargetingManager targetingManager) {
        this.ability = ability;
        this.trigger = trigger;
        this.targetingManager = targetingManager;
        isTargeting = true;

        if (previewInstance != null)
            Object.Destroy(previewInstance);

        targetingManager.SetCurrentStrategy(this);

        if (aoePrefab != null)
            previewInstance = Object.Instantiate(aoePrefab, Vector3.up * 0.1f, Quaternion.identity);
    }

    public override void Update() {
        if (!isTargeting || previewInstance == null) return;

        previewInstance.transform.position = GetMouseWorldPosition() + Vector3.up * 0.1f;

        if (Input.GetKeyDown(KeyCode.R))
            OnClick(previewInstance.transform.position);
    }

    Vector3 GetMouseWorldPosition() {
        if (Camera.main == null) return Vector3.zero;
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out var hit, 100f, groundLayerMask) ? hit.point : Vector3.zero;
    }

    public override void Cancel() {
        isTargeting = false;
        targetingManager.ClearCurrentStrategy();

        if (previewInstance != null)
            UnityEngine.Object.Destroy(previewInstance);
    }

    void OnClick(Vector3 position) {
        if (isTargeting) {
            var targets = Physics.OverlapSphere(position, aoeRadius)
                .Select(c => c.GetComponent<IEntity>())
                .OfType<IEntity>();

            if (trigger.CanConsumeCost()) {
                trigger.ConsumeCost(new(ability.GetCaster(), ability.GetCaster(), Vector3.zero));
                foreach (var target in targets)
                    trigger.TryExecute(new(ability.GetCaster(), target, target.GetPosition() - position));
            }

            Cancel();
        }
    }
}
