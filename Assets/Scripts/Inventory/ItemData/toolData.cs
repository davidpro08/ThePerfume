
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Item")]
public class ToolData : ItemData
{
    // 도구 종류
    public ToolType toolType;

    //최대 내구도
    public int maxDurability = 0;

    //현재 내구도
    public int nowDurability = 0;

    public ToolData() { }
    public ToolData(int id, string itemName, string itemDescription,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    ToolType toolType, int maxDurability, int nowDurability)
    : base(id, itemName, itemDescription, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.toolType = toolType;
        this.maxDurability = maxDurability;
        this.nowDurability = nowDurability;
    }
}