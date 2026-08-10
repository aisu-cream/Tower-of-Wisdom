using UnityEngine;

public class InputReader : MonoBehaviour {

    [SerializeField] KeyCode runCode = KeyCode.LeftShift;
    [SerializeField] KeyCode dashCode = KeyCode.LeftControl;
    [SerializeField] KeyCode jumpCode = KeyCode.Space;
    [SerializeField] KeyCode wieldCode = KeyCode.Mouse0;

    public Vector3 InputDirection { get; private set; } = Vector2.zero;

    public bool DashPressed { get; private set; } = false;
    public bool WieldPressed { get; private set; } = false;

    public bool JumpHeld { get; private set; } = false;
    public bool RunHeld { get; private set; } = false;

    void Update() {
        InputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        DashPressed = Input.GetKeyDown(dashCode);
        WieldPressed = Input.GetKeyDown(wieldCode);

        JumpHeld = Input.GetKeyDown(jumpCode);
        RunHeld = Input.GetKey(runCode);
    }
}
