using System.Collections.Generic;
using UnityEngine;

public class BenchInventoryUIManager : MonoBehaviour
{
    [Header("필수연결")]
    private InventoryManager inventoryManager;
    [SerializeField] private GameObject inventorySlotUIPrefab;
    [Header("인벤토리 설정")]
    [SerializeField] private GameObject benchInventoryPanel; // 인벤토리 패널
    [SerializeField] private Transform benchSlotContainer; // 인벤토리 슬롯들의 부모

    private List<InventorySlotUI> allSlotUI = new List<InventorySlotUI>();

    void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            enabled = false;
            return;
        }
        Debug.Log($"InventoryManager 참조 성공");

        inventoryManager.onInventoryChangedCallback += UpdateAllUIs;
        Debug.Log($"이벤트 구독 완");

        if (benchInventoryPanel != null)
        {
            benchInventoryPanel.SetActive(true);
            Debug.Log($"benchInventoryPanel 활성화 설정 완");
        }
    }

    void Start()
    {
        Debug.Log($"start 호출");
        IntializeAllInventorySlots();
        Debug.Log($"IntializeAllIventorySlot 완료");
        UpdateAllUIs();
        Debug.Log($"UpdateAllUIs 완료");
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.onInventoryChangedCallback -= UpdateAllUIs;
        }
    }

    private void IntializeAllInventorySlots()
    {
        Debug.Log($"IntializeAllInventorySlots 시작");
        // 기본 Slot 초기화
        foreach (Transform child in benchSlotContainer)
        {
            Destroy(child.gameObject);
        }
        allSlotUI.Clear();

        for (int i = 0; i < inventoryManager.capacity; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotUIPrefab, benchSlotContainer);
            InventorySlotUI inventorySlot = slotGO.GetComponent<InventorySlotUI>();

            if (inventorySlot != null)
            {
                inventorySlot.slotIndex = i;
                allSlotUI.Add(inventorySlot);
            }
        }
        Debug.Log($"총 {allSlotUI.Count}개의 슬롯 생성 완");
    }

    private void UpdateAllUIs()
    {
        for (int i = 0; i < allSlotUI.Count; i++)
        {
            if (i < inventoryManager.itemSlots.Count)
            {
                allSlotUI[i].UpdateSlotUI(inventoryManager.itemSlots[i]);
            }
            else
            {
                allSlotUI[i].UpdateSlotUI(new ItemSlot());
            }
        }
    }
}
