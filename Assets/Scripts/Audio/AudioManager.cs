using UnityEngine;


public class AudioManager : MonoBehaviour {

    //Fields
    [Header("--- Audio Sources ---")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("--- Audio Clips ---")]
    public AudioClip backgroundMusic;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip deathSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // Play background music at the start
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Update is called once per frame
    void Update() {

    }
}
