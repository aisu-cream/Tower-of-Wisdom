using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour {

    public float speed = 1.0f;
    public float distance = 10;
    public Vector3 direction = Vector3.up;
    private float t = 0;
    private Vector3 initialPos;

    private Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate() {
        float t_freq = distance / speed;
        rb.MovePosition(initialPos + direction * (distance * (1 + -Mathf.Cos(Mathf.PI * t / t_freq))));
        t += Time.fixedDeltaTime;
    }
}
