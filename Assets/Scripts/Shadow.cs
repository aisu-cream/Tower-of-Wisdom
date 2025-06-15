using UnityEngine;

public class Shadow : MonoBehaviour {

    private float z = 0;

    public void MovePosition(float z) {
        this.z = z;
        transform.localPosition = Vector2.up * z;
    }

    public float GetZ() {
        return z;
    }
}
