using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    // 인벤토리 일단 리스트로 구현했는데, 배열이 더 나을까요?
    public List<Item> items = new List<Item>();
    
    
    public void AddItem(Item item)
    {
        items.Add(item);
        Debug.Log($"Added {item.itemName} to inventory.");
    }

    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log($"Removed {item.itemName} from inventory.");
        }
        else
        {
            Debug.Log($"{item.itemName} not found in inventory.");
        }
    }
}