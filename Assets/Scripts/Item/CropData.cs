
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Inventory/Crop")]
public class CropData : ItemData
{
    // 농작물 종류
    public CropType cropType;
    public GameObject growPrefab; // 작물 성장 프리팹
    public GameObject itemOnTray; // 손질할 때 필요한 프리팹 (트레이 위에 올라감)
    public GameObject itemPrefabOnUI; // 손질할 때 필요한 프리팹
    public ItemData petal; // 꽃잎 프리팹
    

    public CropData() { }
    public CropData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    CropType cropType, GameObject itemPrefab, GameObject itemPrefabOnUI,
    ItemData petal, GameObject itemOnTray)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.cropType = cropType;
        this.growPrefab = itemPrefab;
        this.itemPrefabOnUI = itemPrefabOnUI;
        this.petal = petal;
        this.itemOnTray = itemOnTray;
    }
}