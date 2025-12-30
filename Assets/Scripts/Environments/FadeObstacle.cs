using System.Collections;
using UnityEngine;

public class FadeObstacle : MonoBehaviour {

    public float targetFade = 0.2f;
    public float fadeDuration = 0.25f;
    private float originalFade;

    private Material mat = null!;
    private Renderer rend;
    private Coroutine currCoroutine;

    private float lastFaded = float.MaxValue;
    private bool fadingOut = false;

    private static readonly int FadeID = Shader.PropertyToID("_Fade");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        originalFade = mat.GetFloat(FadeID);
    }

    void Update() {
        if (Time.time - 0.1f > lastFaded) {
            StartFade(originalFade);
            fadingOut = false;
            lastFaded = float.MaxValue;
        }
    }

    public void FadeOut() {
        if (!fadingOut)
            StartFade(targetFade);
        fadingOut = true;
        lastFaded = Time.time;
    }

    private void StartFade(float targetAlpha) {
        if (currCoroutine != null)
            StopCoroutine(currCoroutine);
        currCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
    }

    private IEnumerator FadeCoroutine(float targetFade) {
        float startFade = mat.GetFloat(FadeID);
        float t = 0;

        while (t < 1) {
            t += Time.deltaTime / fadeDuration;
            float a = Mathf.Lerp(startFade, targetFade, t);
            mat.SetFloat(FadeID, a);
            yield return null;
        }

        mat.SetFloat(FadeID, targetFade);
        currCoroutine = null;
    }
}
