using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Inventory/Material")]
public class MaterialData : ItemData
{
    public GameObject itemPrefab; // 아이템 소환 프리팹
    public EssenceData essenceData; // 에센스 데이터

    public MaterialData() { }
    public MaterialData(int id, string itemName, string itemDescription,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    GameObject itemPrefab, EssenceData essenceData)
    : base(id, itemName, itemDescription, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.itemPrefab = itemPrefab;
        this.essenceData = essenceData;
    }
}
