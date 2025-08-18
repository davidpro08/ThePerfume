using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fuel", menuName = "Inventory/Fuel")]
public class FuelData : ItemData
{
    public List<GameObject> itemPrefabs; // 아이템 소환 프리팹 (슬롯별 고정)

    public FuelData() { }
    public FuelData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    List<GameObject> itemPrefabs)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.itemPrefabs = itemPrefabs;
    }
}
