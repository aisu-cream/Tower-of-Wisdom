using UnityEngine;

public class HUDVisibilityController : MonoBehaviour
{
    [SerializeField] private GameObject hudContainer;
    [SerializeField] private GameObject pauseMenuContainer;
    [SerializeField] private GameObject inventoryContainer;

    private void Update()
    {
        bool menuOpen = false;

        if (pauseMenuContainer != null && pauseMenuContainer.activeSelf)
        {
            menuOpen = true;
        }

        if (inventoryContainer != null && inventoryContainer.activeSelf)
        {
            menuOpen = true;
        }

        if (hudContainer != null)
        {
            hudContainer.SetActive(!menuOpen);
        }
    }
}