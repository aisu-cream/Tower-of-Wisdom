using UnityEngine;

public class Shadow : MonoBehaviour {

    [Range(0, 1)] public float shadowScaleIntensity = 0.25f;

    private Vector3 originalScale;

    private float z = 0;
    private float ratio = 1;

    void Start() { 
        originalScale = transform.localScale;
    }

    public void MovePosition(float z) {
        if (this.z != z) {
            this.z = z;
            transform.localPosition = Vector2.up * z;
        }
    }

    public float GetZ() {
        return z;
    }

    public void Scale(float ratio) {
        if (this.ratio != ratio) {
            this.ratio = ratio;
            float axisRatio = Mathf.Pow(ratio, shadowScaleIntensity);
            transform.localScale = new Vector3(originalScale.x * axisRatio, originalScale.y * axisRatio, originalScale.z);
        }
    }
}
