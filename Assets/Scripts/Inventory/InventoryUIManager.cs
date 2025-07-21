
using UnityEngine;
using System.Collections.Generic;

// 핫바와 전체 인벤토리 UI를 모두 관리하는 통합 UI 관리자입니다.
public class InventoryUIManager : MonoBehaviour
{
    [Header("필수 연결")]
    [SerializeField] private InventoryManager inventoryManager; // 데이터 소스
    [SerializeField] private GameObject inventorySlotUIPrefab;  // 슬롯 UI 프리팹

    [Header("핫바 설정")]
    [SerializeField] private Transform hotbarSlotsContainer; // 핫바 슬롯들의 부모

    [Header("전체 인벤토리 설정")]
    [SerializeField] private GameObject fullInventoryPanel;   // 전체 인벤토리 패널
    [SerializeField] private Transform fullInventorySlotsContainer; // 전체 인벤토리 슬롯들의 부모

    // 핫바 리스트
    private List<InventorySlotUI> hotbarSlotUIs = new List<InventorySlotUI>();
    // 전체 인벤토리 리스트
    private List<InventorySlotUI> fullInventorySlotUIs = new List<InventorySlotUI>();

    void Start()
    {
        // 인벤토리 데이터 변경 이벤트에 구독
        inventoryManager.onInventoryChangedCallback += UpdateAllUIs;
        
        // UI 초기화
        InitializeHotbar();
        InitializeFullInventory();
        
        // 업데이트 해주기
        UpdateAllUIs();

        // 시작할 때 전체 인벤토리는 닫아 둠
        fullInventoryPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if(inventoryManager != null)
        {
            // 이벤트 구독 해제
            inventoryManager.onInventoryChangedCallback -= UpdateAllUIs;
        }
    }

    // 핫바 UI를 생성합니다.
    private void InitializeHotbar()
    {
        for (int i = 0; i < inventoryManager.hotbarSize; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotUIPrefab, hotbarSlotsContainer);
            InventorySlotUI invSlot = slotGO.GetComponent<InventorySlotUI>();
            
            if(invSlot != null)
            {
                invSlot.slotIndex = i;
                hotbarSlotUIs.Add(invSlot);
            }
        }
    }

    // 전체 인벤토리 UI를 생성합니다.
    private void InitializeFullInventory()
    {
        for (int i = inventoryManager.hotbarSize; i < inventoryManager.capacity; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotUIPrefab, fullInventorySlotsContainer);
            InventorySlotUI invSlot = slotGO.GetComponent<InventorySlotUI>();

            if (invSlot != null)
            {
                invSlot.slotIndex = i;
                fullInventorySlotUIs.Add(invSlot);
            }
        }
    }

    // 모든 UI(핫바 + 전체 인벤토리)를 새로고침합니다.
    private void UpdateAllUIs()
    {
        // 핫바 UI 업데이트
        for (int i = 0; i < hotbarSlotUIs.Count; i++)
        {
            if (i < inventoryManager.itemSlots.Count)
            {
                hotbarSlotUIs[i].UpdateSlotUI(inventoryManager.itemSlots[i]);
            }
            else
            {
                // 데이터가 없는 슬롯은 비운다
                hotbarSlotUIs[i].UpdateSlotUI(new ItemSlot());
            }
        }

        // 전체 인벤토리 UI 업데이트
        for (int i = 0; i < fullInventorySlotUIs.Count; i++)
        {
            int slotIndex = inventoryManager.hotbarSize + i;
            if (slotIndex < inventoryManager.itemSlots.Count)
            {
                fullInventorySlotUIs[i].UpdateSlotUI(inventoryManager.itemSlots[slotIndex]);
            }
            else
            {
                // 데이터가 없는 슬롯은 비운다
                fullInventorySlotUIs[i].UpdateSlotUI(new ItemSlot());
            }
        }
    }

    // 전체 인벤토리 패널을 켜고 끕니다.
    public void ToggleFullInventory()
    {
        fullInventoryPanel.SetActive(!fullInventoryPanel.activeSelf);
        // 패널을 열때 UI를 강제적으로 한번 더 업데이트
        if(fullInventoryPanel.activeSelf)
        {
            UpdateAllUIs();
        }
    }

}
