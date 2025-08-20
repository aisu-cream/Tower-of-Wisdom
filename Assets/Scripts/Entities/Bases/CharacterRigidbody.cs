using UnityEngine;

// class that handles fake z axis of character (entity)
public class CharacterRigidbody : MonoBehaviour {

    [SerializeField] Rigidbody2D mainBody;
    [SerializeField] Shadow shadow;

    [Min(0)] public float gravity = 49f;
    [Min(0)] public float maxFallSpeed = 25;

    private float z = 0;
    private float vel = 0;

    private float lastFixedTime = -1;

    void Update() {
        transform.localPosition = Vector2.up * GetZ();
    }

    void FixedUpdate() {        
        AddForce(-Mathf.Sign(vel) * vel * vel * DragConst(), ForceMode2D.Force);
        AddForce(-gravity * mainBody.mass, ForceMode2D.Force);
    }

    /// <summary>
    /// Finalizes and returns z position.
    /// Avoid using the field outside FixedUpdate. Rather, use this method (generally, in Update or LateUpdate).
    /// </summary>
    /// <returns>The corrected internal z position</returns>
    public float GetZ() {
        if (OnGround()) {
            z = shadow.GetZ();
            vel = 0;
        }

        return z;
    }

    public float GetVelocity() {
        return vel;
    }

    private float DragConst() { 
        return gravity * mainBody.mass / (maxFallSpeed * maxFallSpeed);
    }

    public bool OnGround() {
        return GetVelocity() <= 0 && z <= shadow.GetZ();
    }

    // Does the same thing as the rigidbody addforce
    // Takes ForceMode2D rather than custom ForceMode for code conciseness
    public void AddForce(float force, ForceMode2D mode) {
        if (mode == ForceMode2D.Force)
            force *= Time.fixedDeltaTime;

        float deltaVel = force / mainBody.mass;
        vel += deltaVel;

        if (lastFixedTime == Time.fixedTime)
            z += deltaVel * Time.fixedDeltaTime;
        else {
            lastFixedTime = Time.fixedTime;
            z += vel * Time.fixedDeltaTime;
        }
    }
}
