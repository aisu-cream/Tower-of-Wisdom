using System.Collections;
using UnityEngine;

// class that handles fake z axis of character (entity)
public class CharacterRigidbody : MonoBehaviour {

    public enum ForceModeZ {
        Force,
        Impulse
    }

    [SerializeField] Rigidbody2D mainBody;
    [SerializeField] Shadow shadow;

    public float gravity = -49f;

    private float z = 0;
    private float vel = 0;

    private bool onGround = false;

    void Update() {
        transform.localPosition = Vector2.up * GetZ();
    }

    void FixedUpdate() {
        AddForce(gravity * mainBody.mass, ForceModeZ.Force);
    }

    public float GetZ() {
        onGround = false;

        if (vel <= 0 && z <= shadow.GetZ()) {
            z = shadow.GetZ();
            vel = 0;
            onGround = true;
        }

        return z;
    }

    public float GetVelocity() {
        return vel;
    }

    public bool OnGround() {
        return onGround;
    }

    public void AddForce(float force, ForceModeZ mode) {
        if (mode == ForceModeZ.Force)
            force *= Time.fixedDeltaTime;
        vel += force / mainBody.mass;
        z += vel * Time.fixedDeltaTime;
    }
}
