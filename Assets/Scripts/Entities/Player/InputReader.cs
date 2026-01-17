using UnityEngine;

public class InputReader : MonoBehaviour {

    [SerializeField] KeyCode runCode = KeyCode.LeftShift;
    [SerializeField] KeyCode dashCode = KeyCode.LeftControl;
    [SerializeField] KeyCode jumpCode = KeyCode.Space;
    [SerializeField] KeyCode wieldCode = KeyCode.Mouse0;

    public Vector3 inputDirection { get; private set; } = Vector2.zero;

    public bool dashPressed { get; private set; } = false;
    public bool wieldPressed { get; private set; } = false;

    public bool jumpHeld { get; private set; } = false;
    public bool runHeld { get; private set; } = false;

    void Update() {
        inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        dashPressed = Input.GetKeyDown(dashCode);
        wieldPressed = Input.GetKeyDown(wieldCode);

        jumpHeld = Input.GetKeyDown(jumpCode);
        runHeld = Input.GetKey(runCode);
    }
}
