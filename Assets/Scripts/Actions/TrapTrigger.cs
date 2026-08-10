using UnityEngine;
using UnityEngine.Events;

public class TrapTrigger : MonoBehaviour {

    [Header("Detection Settings")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = false;

    [Header("The Action")]
    // This creates a slot in the Inspector to drag-and-drop ANY functionality
    public UnityEvent onTrapActivated;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other) {
        if (hasTriggered) return;

        if (other.CompareTag(targetTag)) {
            ActivateTrap();
        }
    }

    private void ActivateTrap() {
        Debug.Log($"Trap on {gameObject.name} activated!");

        // This runs whatever functions you've added in the Inspector
        onTrapActivated?.Invoke();

        if (triggerOnlyOnce) {
            hasTriggered = true;
        }
    }
}