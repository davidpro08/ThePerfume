
using System;

[Serializable]
public class ItemSlot
{
    // 슬롯 당 담을 데이터
    public ItemData itemData;
    
    // 아이탬 개수
    public int quantity;

    // 생성자
    public ItemSlot(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }

    // 빈 슬롯을 위한 생성자
    public ItemSlot()
    {
        itemData = null;
        quantity = 0;
    }

    // 개수가 늘어나거나
    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    // 개수가 줄어듬
    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
    }
}