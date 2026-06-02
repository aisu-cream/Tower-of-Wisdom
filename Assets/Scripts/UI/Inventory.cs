using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject inventoryContainer;

    public static bool InventoryOpen { get; private set; }

    private void Start()
    {
        SetInventoryOpen(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (PauseMenu.IsPaused) return;
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        SetInventoryOpen(!InventoryOpen);
    }

    public void OpenInventory()
    {
        SetInventoryOpen(true);
    }

    public void CloseInventory()
    {
        SetInventoryOpen(false);
    }

    private void SetInventoryOpen(bool open)
    {
        InventoryOpen = open;
        inventoryContainer.SetActive(open);
    }
    public void CloseInventoryButton()
    {
        CloseInventory();
    }
}