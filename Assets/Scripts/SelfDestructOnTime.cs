using UnityEngine;

public class SelfDestructOnTime : MonoBehaviour {

    public float time;

    void Start() {
        Destroy(gameObject, time);
    }
}
