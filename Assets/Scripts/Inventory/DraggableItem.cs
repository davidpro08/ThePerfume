using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    public TextMeshProUGUI countText;

    // 드롭 실패하면 돌아갈 위치 (드래그전 위치)
    [HideInInspector] public Transform parentAfterDrag;

    // draggableItem이 현재 표기하는 아이템 데이터 (UI표현용)
    [HideInInspector] public ItemData currentItemData;
    [HideInInspector] public int currentQuantity;

    // draggableItem이 어느 슬록에 바인딩되어있는지 참조
    [HideInInspector] public InventorySlotUI boundSlot;

    // draggableItem UI 초기화
    public void Setup(ItemData item, int quantity, InventorySlotUI boundSlotUI)
    {
        currentItemData = item;
        currentQuantity = quantity;
        boundSlot = boundSlotUI;
        
        if (item != null)
        {
            image.sprite = item.itemIcon;
            image.preserveAspect = true;
            image.enabled = true;
            RefreshCount();

            image.rectTransform.localScale = Vector3.one;

            if (item.scaleUpUI)
            {
                image.rectTransform.localScale = new Vector3(1.4f, 1.4f, 1f);
            }
        }
        else
        {
            ClearVisuals();
        }
    }

    public void RefreshCount()
    {
        if (currentItemData != null && currentItemData.isStackable)
        {
            countText.text = currentQuantity.ToString();
            bool textActive = currentQuantity > 1; // 개수가 1이하면 개수 안보임
            countText.gameObject.SetActive(textActive);
        }
        else
        {
            countText.gameObject.SetActive(false);
        }
    }

    // 슬록이 비면 아이템 UI 비활성화
    public void ClearVisuals()
    {
        image.enabled = false;
        countText.gameObject.SetActive(false);
        currentItemData = null;
        currentQuantity = 0;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("Begin Drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        countText.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("Dragging");
        transform.position = eventData.position;
        //transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("End Drag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
        if(countText != null) countText.raycastTarget = true;
    }

}
