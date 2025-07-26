using UnityEngine;

public class PickupItems : MonoBehaviour, IInteract
{
    [Header("아이템 정보")]
    [SerializeField] public ItemData itemToGive; // 아이템
    [SerializeField] public int quantityToGive = 1; //수량

    void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemToGive.itemIcon;
    }
    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;
        
        // 아이템 추가
        InventoryManager.Instance.AddItem(itemToGive, quantityToGive);
    }

    public bool CanInteract(Player player)
    {
        if (itemToGive == null)
        {
            Debug.Log($"[{name}] : 아이템 정보 없음");
            return false;
        }

        return true;
    }
}
