using System.Collections;
using System.Data;
using UnityEngine;

public class InventorySaveManager
{
    private static Coroutine pendingSave;
    public static void SaveInventory(GameSave save, InventoryManager inventoryManager, MonoBehaviour context, bool immediate = false)
    {
        if (immediate)
        {
            if (pendingSave != null)
            {
                context.StopCoroutine(pendingSave);
                pendingSave = null;
            }

            ApplyInventoryData(save, inventoryManager);
            SaveManager.Save(save);
            return;
        }

        if (pendingSave != null)
        {
            context.StopCoroutine(pendingSave);
        }

        pendingSave = context.StartCoroutine(SaveDelayed(save, inventoryManager));
    }

    private static IEnumerator SaveDelayed(GameSave save, InventoryManager inventoryManager)
    {
        yield return new WaitForSeconds(0.5f);
        ApplyInventoryData(save, inventoryManager);
        SaveManager.Save(save);
        pendingSave = null;
    }

    private static void ApplyInventoryData(GameSave save, InventoryManager inventoryManager)
    {
        save.inventory.Clear();
        for (int i = 0; i < inventoryManager.itemSlots.Count; i++)
        {
            var slot = inventoryManager.itemSlots[i];
            if (!slot.IsEmpty)
            {
                save.inventory.Add(new InventoryItemSaveData
                {
                    itemID = slot.itemData.id,
                    quantity = slot.quantity,
                    location = i
                });
            }
        }
    }

    public static void LoadInventory(GameSave save, InventoryManager inventoryManager)
    {
        foreach (var slot in inventoryManager.itemSlots)
            slot.Clear();

        foreach (var data in save.inventory)
        {
            if (data.location < 0 || data.location >= inventoryManager.itemSlots.Count) continue;

            ItemSlot slot = inventoryManager.itemSlots[data.location];

            ItemData item = SaveManager.Instance.GetItemData(data.itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {data.itemID} not found in SaveManager itemDB");
                continue;
            }

            slot.itemData = item;
            slot.quantity = data.quantity;
        }

        inventoryManager.InventoryChanged();
    }
}
