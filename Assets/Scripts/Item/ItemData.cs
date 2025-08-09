using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    // 아이템 고유 키
    public int id;
    
    // 아이템 이름
    public string itemName = "New Item";
    
    // 아이템 설명
    [FormerlySerializedAs("itemDescription")] [TextArea(3, 10)]
    public string description = "Null Description";
    
    // 아이템에 아이콘 할당
    public Sprite itemIcon = null;
    
    // 아이템 종류
    public ItemType itemType = ItemType.Crop; // 아이템 타입
    
    // 쌓일 수 있나요?
    public bool isStackable = false;
    
    // 최대 쌓이는 거
    // maxStackSize = 1이면 isStackable = false와 동일한가?
    public int maxStack = 99;

    // 현재 쌓여있는 거
    // 근데 이걸 저장할 필요 없이 나중에 Quantity로 아이템 개수를 저장하기 때문에
    // 이걸 여기에 쓰기에는 애매함
    public int nowStack = 1;
    
    // 거래 가능 여부
    public bool isTradable = false;
    
    // 구매 가격
    public int buyPrice = 0;
    
    // 판매 가격
    public int sellPrice = 0;

    // 인벤토리 슬롯에서 너무 작게 보이면 키기
    public bool scaleUpUI = false;

    public ItemData()
    {
        
    }
    
    public ItemData(int id, string itemName, string description, Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack, int nowStack, bool isTradable, int buyPrice, int sellPrice)
    {
        this.id = id;
        this.itemName = itemName;
        this.description = description;
        this.itemIcon = itemIcon;
        this.itemType = itemType;
        this.isStackable = isStackable;
        this.maxStack = maxStack;
        this.nowStack = nowStack;
        this.isTradable = isTradable;
        this.buyPrice = buyPrice;
        this.sellPrice = sellPrice;
    }
}
