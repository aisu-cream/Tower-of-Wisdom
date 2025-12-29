using UnityEngine;

public class PlayerInputData : MonoBehaviour {

    public KeyCode runCode = KeyCode.LeftShift;
    public KeyCode dashCode = KeyCode.LeftControl;
    public KeyCode jumpCode = KeyCode.Space;
    public KeyCode wieldCode = KeyCode.Mouse0;

    public Vector2 dirInput { get; private set; } = Vector2.zero;

    public bool dashPressed { get; private set; } = false;
    public bool wieldPressed { get; private set; } = false;

    public bool jumpHeld { get; private set; } = false;
    public bool runHeld { get; private set; } = false;

    void Update() {
        dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        dashPressed = Input.GetKeyDown(dashCode);
        wieldPressed = Input.GetKeyDown(wieldCode);

        jumpHeld = Input.GetKey(jumpCode);
        runHeld = Input.GetKey(runCode);
    }
}
