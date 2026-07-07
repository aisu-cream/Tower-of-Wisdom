using TMPro;
using UnityEngine;

public class InventoryCategoryController : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private TMP_Text categoryTitle;

    private string[] categories = { "Weapons", "Armor", "Consumables" };
    private int currentIndex = 0;

    private void Start()
    {
        UpdateCategory();
    }

    public void NextCategory()
    {
        currentIndex = (currentIndex + 1) % categories.Length;
        UpdateCategory();
    }

    public void PreviousCategory()
    {
        currentIndex = (currentIndex - 1 + categories.Length) % categories.Length;
        UpdateCategory();
    }

    private void UpdateCategory()
    {
        string currentCategory = categories[currentIndex];

        categoryTitle.text = currentCategory;

        // TEMP: just refresh slots
        inventoryUI.RefreshUI();
    }
}