using UnityEngine;

public class PickupItems : MonoBehaviour
{
    [Header("아이템 정보")]
    [SerializeField] public ItemData itemToGive; // 아이템
    [SerializeField] public int quantityToGive = 1; //수량

    void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemToGive.itemIcon;
    }
}
