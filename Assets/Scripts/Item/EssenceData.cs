
using UnityEngine;

[CreateAssetMenu(fileName = "New Essence", menuName = "Inventory/Essence")]
public class EssenceData : ItemData
{
    // 에센스 종류
    public EssenceType essenceType;

    //따듯한 수치
    public int essenceWarm = 0;

    // 차가움 수치
    public int essenceCool = 0;

    // 진정 수치
    public int essenceRelax = 0;

    // 베이스 노트 가능 유무
    public bool canBaseNote = false;

    // 미들 노트 가능 유무
    public bool canMiddleNote = false;

    // 탑 노트 가능 유무
    public bool canTopNote = false;

    // 에센스 관에서 나올 스프라이트 단계
    public EssenceStage essenceStage;
    // 에센스 고유 색
    public Color color;

    public EssenceData()
    { }
    public EssenceData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    EssenceType essenceType, int essenceWarm, int essenceCool, int essenceRelax,
    bool canBaseNote, bool canMiddleNote, bool canTopNote, EssenceStage essenceStage,
    Color color)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.essenceType = essenceType;
        this.essenceWarm = essenceWarm;
        this.essenceCool = essenceCool;
        this.essenceRelax = essenceRelax;
        this.canBaseNote = canBaseNote;
        this.canMiddleNote = canMiddleNote;
        this.canTopNote = canTopNote;
        this.essenceStage = essenceStage;
        this.color = color;
    }
}