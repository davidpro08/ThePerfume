
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Inventory/Crop")]
public class CropData : ItemData
{
    // 농작물 종류
    public CropType cropType;
    public GameObject itemPrefab; // 손질할 때 필요한 프리팹

    public CropData() { }
    public CropData(int id, string itemName, string itemDescription,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    CropType cropType)
    : base(id, itemName, itemDescription, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.cropType = cropType;
    }
}