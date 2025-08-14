using UnityEngine;

[CreateAssetMenu(fileName = "New Installation", menuName = "Inventory/installation")]
public class InstallationData : ItemData
{
    public GameObject itemPrefab; // 아이템 소환 프리팹

    public InstallationData() { }
    public InstallationData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    GameObject itemPrefab)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.itemPrefab = itemPrefab;
    }
}
