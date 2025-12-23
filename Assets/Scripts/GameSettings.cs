using UnityEngine;

public class GameSettings : MonoBehaviour {

    public int targetFrameRate = 60;

    void OnValidate() {
        Application.targetFrameRate = targetFrameRate;
    }
}