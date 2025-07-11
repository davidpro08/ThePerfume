
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 각 인벤토리 UI 슬롯의 동작을 제어합니다.
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountText;

    // ItemSlot 데이터로 슬롯의 UI(아이콘, 수량)를 업데이트합니다.
    public void UpdateSlot(ItemSlot slot)
    {
        // 슬롯에 아이템이 있는지 확인
        if (slot.itemData != null)
        {
            itemIconImage.sprite = slot.itemData.itemIcon;
            itemIconImage.enabled = true;

            // 아이템을 쌓을 수 있을 때만 수량 보이기
            if (slot.itemData.isStackable)
            {
                itemCountText.text = slot.quantity.ToString();
                itemCountText.enabled = true;
            }
            else
            {
                itemCountText.enabled = false;
            }
        }
        else
        {
            // 슬롯이 비어있으면 아이콘과 텍스트를 모두 비활성화
            ClearSlot();
        }
    }

    // 슬롯 UI를 깨끗하게 비웁니다.
    public void ClearSlot()
    {
        itemIconImage.enabled = false;
        itemCountText.enabled = false;
    }

}
