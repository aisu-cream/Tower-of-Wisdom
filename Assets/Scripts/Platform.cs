using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Platform : MonoBehaviour {

    private enum Location {
        Upper,
        Lower
    }

    [SerializeField] Location mode;
    public float z;
    public Platform opposite;
    public Collider2D col { get; private set; }

    void Start() {
        col = GetComponent<Collider2D>();
    }

    public bool IsUpper() {
        return mode == Location.Upper;
    }

    public bool IsLower() {
        return mode == Location.Lower;
    }
}
