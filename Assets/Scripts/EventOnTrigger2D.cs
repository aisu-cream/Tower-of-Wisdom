using UnityEngine;
using UnityEngine.Events;

public class EventOnTrigger2D : MonoBehaviour {

    [SerializeField] UnityEvent<Collider2D> onTriggerEnter2D;
    [SerializeField] UnityEvent<Collider2D> onTriggerStay2D;
    [SerializeField] UnityEvent<Collider2D> onTriggerExit2D;

    void OnTriggerEnter2D(Collider2D other) {
        onTriggerEnter2D.Invoke(other);
    }

    void OnTriggerStay2D(Collider2D other) {
        onTriggerStay2D.Invoke(other);
    }

    void OnTriggerExit2D(Collider2D other) {
        onTriggerExit2D.Invoke(other);
    }
}
