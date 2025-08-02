
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem.Composites;
using System.Net.Http.Headers;

// 각 인벤토리 UI 슬롯의 동작을 제어합니다.
public class BenchInventorySlotUI : InventorySlotUI
{
    // Bench용 변수
    private BenchInventoryUIManager _benchUIManger;
    private Button _slotButton;

    protected override void Awake()
    {
        base.Awake();

        _slotButton = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        _slotButton.onClick.RemoveAllListeners();
        _slotButton.onClick.AddListener(OnSlotClicked);
    }

    // private void Start()
    // {
    //     if (_inventoryManager != null)
    //     {
    //         _inventoryManager.onSlotSelectedCallback += OnManagerSlotSelected;
    //         SetSelected(_inventoryManager.SelectedSlotIndex == slotIndex);
    //     }
    //     else
    //     {
    //         Debug.LogError("InventoryManager를 찾을 수 없었습니다! (슬롯 선택 기능x)");
    //         SetSelected(false);
    //     }
    // }

    private void OnManagerSlotSelected(int selectedIndex)
    {
        SetSelected(selectedIndex == slotIndex);
    }

    public override void UpdateSlotUI(ItemSlot slotData)
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
        if (_slotButton != null)
        {
            _slotButton.interactable = (slotData.itemData != null);
        }
    }

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
