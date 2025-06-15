using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Platform : MonoBehaviour {

    [SerializeField] GameObject lower;
    [SerializeField] GameObject upper;

    // return fake z axis of the top of the platform
    public float Top() {
        return lower.transform.localPosition.y + upper.transform.localScale.y + lower.transform.localScale.y / 2f;
    }

    // return fake z axis of the bottom of the platform
    public float Bottom() {
        return lower.transform.localPosition.y + upper.transform.localScale.y - lower.transform.localScale.y / 2f;
    }
}
