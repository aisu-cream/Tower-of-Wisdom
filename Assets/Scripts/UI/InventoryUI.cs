using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform slotGrid;
    [SerializeField] private GameObject slotPrefab;

    [Header("Temporary")]
    [SerializeField] private int testSlotCount;

    private readonly List<GameObject> spawnedSlots = new List<GameObject>();

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        ClearSlots();

        int slotCount = GetSlotCount();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotGrid);
            slot.name = $"InventorySlot_{i}";
            spawnedSlots.Add(slot);
        }
    }

    private int GetSlotCount()
    {
        // TEMP — replace this later with real inventory count
        testSlotCount = 10;
        return testSlotCount;
    }

    public void ClearSlots()
    {
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot);
        }

        spawnedSlots.Clear();
    }
}