
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem.Composites;

// 각 인벤토리 UI 슬롯의 동작을 제어합니다.
public class BenchInventorySlotUI : InventorySlotUI
{
    // public int slotIndex;

    // public Outline outlineComponent;
    // public Color noramlOutlineColor = Color.clear;
    // public Color selectedOutlineColor = new Color(1f, 1f, 1f, 0.5f);
    // public Vector2 normalOutlineDistance = Vector2.zero;
    // public Vector2 selectedOutlineDistance = new Vector2(3f, 3f);

    [SerializeField] private GameObject draggableItemPrefab;
    private DraggableItem _currentDraggableItem;

    [SerializeField] private InventoryManager _inventoryManager;

    // Bench용 변수
    private BenchInventoryUIManager _benchUIManger;
    private Button _slotButton;

    private void Awake()
    {
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
        }
        //_inventoryManager = InventoryManager.Instance;

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

        //bench용 클릭을 위한 버튼 컴포넌트 추가/클릭 리스터 등록
        _slotButton = GetComponent<Button>();
        if (_slotButton == null)
        {
            _slotButton = gameObject.AddComponent<Button>();
        }
        _slotButton.onClick.RemoveAllListeners();
        _slotButton.onClick.AddListener(OnSlotClicked);
    }
    private void Start()
    {
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

    private void OnManagerSlotSelected(int selectedIndex)
    {
        SetSelected(selectedIndex == slotIndex);
    }

    // public void SetSelected(bool isSelected)
    // {
    //     if (outlineComponent != null)
    //     {
    //         if (isSelected)
    //         {
    //             outlineComponent.effectColor = selectedOutlineColor;
    //             outlineComponent.effectDistance = selectedOutlineDistance;
    //         }
    //         else
    //         {
    //             outlineComponent.effectColor = noramlOutlineColor;
    //             outlineComponent.effectDistance = normalOutlineDistance;
    //         }
    //     }
    // }

    void OnDestroy()
    {
        if (_inventoryManager != null)
        {
            _inventoryManager.onSlotSelectedCallback -= OnManagerSlotSelected;
        }
        if (_slotButton != null)
        {
            _slotButton.onClick.RemoveAllListeners();
        }
    }

    // InventoryManager의 데이터에 따라 현재 슬록 UI 업데이트
    // public void UpdateSlotUI(ItemSlot slotData)
    // {
    //     if (_currentDraggableItem == null) return;
    //     if (slotData.itemData != null)
    //     {
    //         _currentDraggableItem.Setup(slotData.itemData, slotData.quantity, this);
    //         _currentDraggableItem.transform.localPosition = Vector3.zero;
    //     }
    //     else
    //     {
    //         _currentDraggableItem.ClearVisuals();
    //     }
    //     if (_slotButton != null)
    //     {
    //         _slotButton.interactable = (slotData.itemData != null);
    //     }
    // }

    // // draggableItem이 슬록에 드롭되면 호출
    // public void OnDrop(PointerEventData eventData)
    // {
    //     DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
    //     if (droppedItem == null || _inventoryManager == null) return;

    //     int sourceSlotIndex = droppedItem.boundSlot.slotIndex;
    //     int targetSlotIndex = this.slotIndex;

    //     _inventoryManager.TryMoveItem(sourceSlotIndex, targetSlotIndex);
    // }

    private void OnSlotClicked()
    {
        if (_benchUIManger != null)
        {
            _benchUIManger.OnSlotClicked(slotIndex);
        }
    }

    public void SetBenchManager(BenchInventoryUIManager manager)
    {
        _benchUIManger = manager;
    }
}
