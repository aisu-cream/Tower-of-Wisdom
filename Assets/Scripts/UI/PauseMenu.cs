using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenuContainer;

    public static bool IsPaused { get; private set; }

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void OpenMenu()
    {
        SetPaused(true);
    }

    public void CloseMenu()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        PauseMenuContainer.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void ResumeButton()
    {
        CloseMenu();
    }

    public void MainMenuButton()
    {
        SetPaused(false);
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}