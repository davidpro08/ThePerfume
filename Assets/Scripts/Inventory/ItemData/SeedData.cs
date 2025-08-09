
using UnityEngine;

[CreateAssetMenu(fileName = "New Seed", menuName = "Inventory/Seed")]
public class SeedData : ItemData
{
    [Header("씨앗 정보")]
    public CropType growIntoCropType;
    public GameObject cropPrefabToGrow;
    // 씨앗 종류 >> 근데 무슨 작물로 자랄지 연결하려면 cropType이랑 연결해야하는 거 아닌가?
    // 일단 위에 추가해놓음


    private SeedType seedType;
    public SeedData() { }
    public SeedData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    SeedType seedType)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.seedType = seedType;
    }
}