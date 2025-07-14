
using UnityEngine;

[CreateAssetMenu(fileName = "New Seed", menuName = "Inventory/Item")]
public class SeedData : ItemData
{
    // 씨앗 종류
    public SeedType seedType;

    public SeedData() { }
    public SeedData(int id, string itemName, string itemDescription,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    SeedType seedType)
    : base(id, itemName, itemDescription, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.seedType = seedType;
    }
}