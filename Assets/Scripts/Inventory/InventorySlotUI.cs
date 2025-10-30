
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.IO.LowLevel.Unsafe;

// 각 인벤토리 UI 슬롯의 동작을 제어합니다.
public class InventorySlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public int slotIndex;

    public Outline outlineComponent;
    public Color noramlOutlineColor = Color.clear;
    public Color selectedOutlineColor = new Color(1f, 1f, 1f, 0.5f);
    public Vector2 normalOutlineDistance = Vector2.zero;
    public Vector2 selectedOutlineDistance = new Vector2(3f, 3f);

    [SerializeField] private GameObject draggableItemPrefab;
    protected DraggableItem _currentDraggableItem;

    [SerializeField] protected InventoryManager _inventoryManager;

    protected virtual void Awake()
    {
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
        }
        _inventoryManager = InventoryManager.Instance;

        if (draggableItemPrefab != null)
        {
            GameObject go = Instantiate(draggableItemPrefab, transform);
            _currentDraggableItem = go.GetComponent<DraggableItem>();
            if (_currentDraggableItem != null)
            {
                _currentDraggableItem.boundSlot = this;
                _currentDraggableItem.ClearVisuals();
            }
        }
    }
    private void Start()
    {
        // ================================================
        _inventoryManager = InventoryManager.Instance;
        // ================================================

        if (_inventoryManager != null)
        {
            _inventoryManager.onSlotSelectedCallback += OnManagerSlotSelected;
            SetSelected(_inventoryManager.SelectedSlotIndex == slotIndex);
        }
        else
        {
            Debug.LogError("InventoryManager를 찾을 수 없었습니다! (슬롯 선택 기능x)");
            SetSelected(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _inventoryManager.SelectSlot(slotIndex);

        }
    }

    private void OnManagerSlotSelected(int selectedIndex)
    {
        SetSelected(selectedIndex == slotIndex);
    }

    public void SetSelected(bool isSelected)
    {
        if (outlineComponent != null)
        {
            if (isSelected)
            {
                outlineComponent.effectColor = selectedOutlineColor;
                outlineComponent.effectDistance = selectedOutlineDistance;
            }
            else
            {
                outlineComponent.effectColor = noramlOutlineColor;
                outlineComponent.effectDistance = normalOutlineDistance;
            }
        }
    }

    void OnDestroy()
    {
        if (_inventoryManager != null)
        {
            _inventoryManager.onSlotSelectedCallback -= OnManagerSlotSelected;
        }
    }

    // InventoryManager의 데이터에 따라 현재 슬록 UI 업데이트

    public virtual void UpdateSlotUI(ItemSlot slotData)
    {
        if (_currentDraggableItem == null) return;
        if (slotData.itemData != null)
        {
            _currentDraggableItem.Setup(slotData.itemData, slotData.quantity, this);
            _currentDraggableItem.transform.localPosition = Vector3.zero;
        }
        else
        {
            _currentDraggableItem.ClearVisuals();
        }
    }

    // draggableItem이 슬록에 드롭되면 호출
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (droppedItem == null || _inventoryManager == null) return;

        int sourceSlotIndex = droppedItem.boundSlot.slotIndex;
        int targetSlotIndex = this.slotIndex;

        _inventoryManager.TryMoveItem(sourceSlotIndex, targetSlotIndex);
    }
}

//[SerializeField] private Image itemIconImage;
//[SerializeField] private TextMeshProUGUI itemCountText;

//// ItemSlot 데이터로 슬롯의 UI(아이콘, 수량)를 업데이트합니다.
//public void UpdateSlot(ItemSlot slot)
//{
//    // 슬롯에 아이템이 있는지 확인
//    if (slot.itemData != null)
//    {
//        itemIconImage.sprite = slot.itemData.itemIcon;
//        itemIconImage.enabled = true;

//        // 아이템을 쌓을 수 있을 때만 수량 보이기
//        if (slot.itemData.isStackable)
//        {
//            itemCountText.text = slot.quantity.ToString();
//            itemCountText.enabled = true;
//        }
//        else
//        {
//            itemCountText.enabled = false;
//        }
//    }
//    else
//    {
//        // 슬롯이 비어있으면 아이콘과 텍스트를 모두 비활성화
//        ClearSlot();
//    }
//}