using UnityEngine;

public class MouseLook2D : MonoBehaviour {

    // The object that will look at the mouse position
    [SerializeField] Transform target;

    // Update is called once per frame
    void Update() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        LookAtY(mouseWorldPos);
    }

    // Rotates target so that its up vector faces the worldPos
    private void LookAtY(Vector3 worldPos) {
        Vector3 dir = worldPos - target.position;
        float angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        target.rotation = Quaternion.Euler(0, 0, angle);
    }
}
