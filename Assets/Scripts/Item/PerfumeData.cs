
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Perfume", menuName = "Inventory/Perfume")]
public class PerfumeData : ItemData
{
    // 농작물 종류
    public PerfumeType perfumeType;

    // 따듯함 수치
    public float perfumeWarm = 0f;

    // 차가움 수치
    public float perfumeCool = 0f;

    //진정 수치
    public float perfumeRelax = 0f;

    public Color color;

    public PerfumeData() { }
    public PerfumeData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    PerfumeType perfumeType, float perfumeWarm, float perfumeCool, float perfumeRelax
    , Color color)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.perfumeType = perfumeType;
        this.perfumeWarm = perfumeWarm;
        this.perfumeCool = perfumeCool;
        this.perfumeRelax = perfumeRelax;
        this.color = color;
    }
}
