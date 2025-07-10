
using UnityEngine;
using System.Collections.Generic;

// 인벤토리 전체 UI를 관리하고 InventoryManager와 연동합니다.
public class InventoryUI : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private InventoryManager inventoryManager; // 인벤토리 데이터 관리자
    [SerializeField] private Transform slotsContainer;      // 슬롯 UI들이 생성될 부모 Transform
    [SerializeField] private GameObject inventorySlotPrefab;  // 슬롯 UI 프리팹

    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    void Start()
    {
        // InventoryManager의 이벤트에 구독하여 인벤토리 변경 시 UpdateUI 함수를 호출하도록 설정
        inventoryManager.onInventoryChangedCallback += UpdateUI;

        // 인벤토리 UI 초기화
        InitializeInventory();
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        inventoryManager.onInventoryChangedCallback -= UpdateUI;
    }

    // 인벤토리 슬롯들을 처음에 한 번 생성합니다.
    private void InitializeInventory()
    {
        // InventoryManager에 정의된 용량(capacity)만큼 슬롯 UI를 생성
        // TODO: 기본 용량만큼 생성하도록 했는데 나중에 하드코딩 해야할 수 있음
        // 그냥 인수 하나 더 둬서 생성 개수 조정하도록 함
        for (int i = 0; i < inventoryManager.DefaultCapacity; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, slotsContainer);
            InventorySlotUI newSlotUI = slotGO.GetComponent<InventorySlotUI>();
            slotUIs.Add(newSlotUI);
        }
        UpdateUI(); // 생성된 슬롯들의 초기 상태 업데이트
    }

    // 인벤토리 UI 전체를 최신 데이터로 새로고침합니다.
    // 이 방식을 사용하려면 흠...
    // List로 저장하지 말고 Array로 저장하는 방식을 사용해야 할 것 같은데
    // 자동 정렬이 되어버림
    private void UpdateUI()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            // 실제 아이템 데이터가 있는 슬롯만 업데이트
            if (i < inventoryManager.itemSlots.Count)
            {
                slotUIs[i].UpdateSlot(inventoryManager.itemSlots[i]);
            }
            else
            {
                // 데이터가 없는 나머지 UI 슬롯은 비워줌
                slotUIs[i].ClearSlot();
            }
        }
    }
}
