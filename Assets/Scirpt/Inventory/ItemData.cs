using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    // 아이템 이름
    public string itemName = "New Item";
    
    // 아이템에 아이콘 할당
    public Sprite itemIcon = null;
    
    // 아이템 설명
    [TextArea(3, 10)]
    public string itemDescription = "Null Description";
    
    // 쌓일 수 있나요?
    public bool isStackable = false;
    
    // 최대 쌓이는 거
    // maxStackSize = 1이면 isStackable = false와 동일한가?
    // 음...
    public int maxStackSize = 99;
    
    // 아이템 종류
    public ItemType itemType = ItemType.Crop; // 아이템 타입
}
