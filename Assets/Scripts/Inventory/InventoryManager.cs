using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// 싱글톤으로 구현하는 건 어떨까?
public class InventoryManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InventoryManager Instance { get; private set; }

    // 인벤토리 일단 리스트로 구현
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    // 인벤토리 변경 시 UI 업데이트 등을 위한 이벤트
    public delegate void OnInventoryChanged();

    // 이벤트 방식으로 구현
    public event OnInventoryChanged onInventoryChangedCallback;

    //슬롯 선택 이벤트
    public delegate void OnSlotSelected(int selectedIndex);
    public event OnSlotSelected onSlotSelectedCallback;
    private int _selectedSlotIndex = -1;

    public int SelectedSlotIndex
    {
        get
        {
            return _selectedSlotIndex;
        }
        private set
        {
            if (_selectedSlotIndex != value)
            {
                _selectedSlotIndex = value;
                onSlotSelectedCallback?.Invoke(_selectedSlotIndex);
            }
        }
    }

    // 용량
    // 기본 용량 = 8
    public int capacity = 8;

    //// 핫바 사이즈
    //// 전체 인벤토리(itemSlots)의 앞부분 N개를 핫바로 간주
    public readonly int hotbarSize = 8;

    void Awake()
    {
        //이미 인스턴스 있으면 자신 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject); // gameManager로 관리하는게 나은가?
        // 인벤토리 슬롯을 초기 용량만큼 미리 생성
        for (int i = 0; i < capacity; i++)
        {
            itemSlots.Add(new ItemSlot());
        }
    }

    private void Start()
    {
        ApplySnapshot(SaveManager.Load().inventory);
    }

    void Update()
    {
        //슬롯 선택 감지
        for (int i = 0; i < hotbarSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                {
                    SelectSlot(i);
                    break;
                }
            }
        }
    }

    public void SelectSlot(int index) // 슬롯 선택 이벤트만 추가해놓음. 업데이트 필요
    {
        if (index >= 0 && index < hotbarSize)
        {
            SelectedSlotIndex = index;
            Debug.Log($"핫바 슬롯 {index + 1}번 선택");
        }
        else
        {
            Debug.Log($"벗어난 슬롯 : {index}");
        }
    }

    // TODO: 로직이 이렇게 간단하면 안되고, 나중에 수정해야 함
    // 1. 같은 아이템이 있으면 어떻게 처리할것인가?
    // 2. 아이템이 앞부터 쌓이는 걸 어떻게 알 것인가?


    public bool AddItem(ItemData itemAdd, int amountOfItems)
    {
        // 쌓일 수 있는 아이템 처리
        // 예를 들어 도구 등은 쌓일 수 없음
        if (itemAdd.isStackable)
        {
            // 만약 쌓일 수 있으면
            foreach (ItemSlot slot in itemSlots)
            {
                if (slot.itemData == itemAdd && slot.quantity < itemAdd.maxStack)
                {
                    // 쌓을 수 있는 남은 공간이 이정도
                    int remainingSpace = itemAdd.maxStack - slot.quantity;

                    // 이 공간에 들어오는 아이템은 
                    // 아이템 개수 만큼 쌓이고
                    // 아이템 개수가 쌓을 수 있는 공간보다 크면
                    // 쌓을 수 있는 공간만큼만 쌓임
                    int amountToMove = Mathf.Min(amountOfItems,
                        remainingSpace);

                    // 이를 적용
                    slot.AddQuantity(amountToMove);

                    amountOfItems -= amountToMove;
                    // 모든 아이템이 들어갔다면
                    if (amountOfItems <= 0)
                    {
                        InventoryChanged();
                        return true; // 모든 아이템 추가 완료
                    }
                }
            }
        }

        // 2. 남은 아이템 또는 스택 불가능한 아이템: 빈 슬롯에 추가
        while (amountOfItems > 0)
        {
            // 빈 공간 찾기
            ItemSlot emptySlot = itemSlots.Find(slot => slot.itemData == null);

            // 빈 공간이 없으면
            if (emptySlot == null)
            {
                // 빈 공간이 없다고 알리기
                Debug.LogError($"ItemSlot {itemAdd.name} has no empty slot");
                return false;
            }

            // 아이템의 최소 값 확인
            int amountToPlace = itemAdd.isStackable ? Mathf.Min(amountOfItems, itemAdd.maxStack) : 1;

            emptySlot.itemData = itemAdd;
            emptySlot.quantity = amountToPlace;
            amountOfItems -= amountToPlace;
        }

        // UI 변경을 위해 이벤트 부르기
        InventoryChanged();
        return true;
    }

    public bool RemoveItem(ItemData itemToRemove, int quantityToRemove)
    {
        // 뒤에서부터 탐색하여 제거 (앞에서부터 제거하면 인덱스 문제 발생 가능성)
        for (int i = itemSlots.Count - 1; i >= 0; i--)
        {
            ItemSlot slot = itemSlots[i];

            // 지울 아이템과 똑같다면
            if (slot.itemData == itemToRemove)
            {
                // 그리고 지울 개수보다 많아야 함
                if (slot.quantity >= quantityToRemove)
                {
                    slot.RemoveQuantity(quantityToRemove);
                    if (slot.quantity == 0)
                    {
                        slot.itemData = null; // 슬롯 비우기
                    }
                    InventoryChanged();
                    return true; // 모든 아이템 제거 완료
                }
                else
                {
                    // 현재 슬롯의 아이템을 모두 제거하고, 남은 수량은다음 슬롯에서 제거
                    quantityToRemove -= slot.quantity;
                    slot.itemData = null;
                    slot.quantity = 0;
                }
            }
        }


        // 만약 여기에 도달한다면 quantityToRemove가 남은 것
        Debug.LogWarning("제거하려는 아이템이 인벤토리에 충분하지않습니다: " + itemToRemove.name + quantityToRemove);
        return false; // 제거하려는 아이템이 부족함
    }

    // 슬록 간 아이템 이동 로직
    public bool TryMoveItem(int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= itemSlots.Count || targetIndex < 0 || targetIndex >= itemSlots.Count)
        {
            Debug.LogWarning("유효하지 않은 슬로 인덱스입니다.");
            Debug.Log($"itemSlots: {itemSlots.Count}");
            Debug.Log($"sourceSlotIndex: {sourceIndex}, targetSlotIndex: {targetIndex}");
            return false;
        }

        ItemSlot sourceSlot = itemSlots[sourceIndex];
        ItemSlot targetSlot = itemSlots[targetIndex];

        //같은 슬록으로 이동 시도
        if (sourceIndex == targetIndex) return false;

        // 소스 슬롯이 비어있으면 이동 불가
        if (sourceSlot.itemData == null) return false;
        // 타겟 슬록이 비어있으면
        if (targetSlot.itemData == null)
        {
            sourceSlot.CopyTo(targetSlot);
            sourceSlot.Clear();
        }

        //타겟 슬롯과 소스 슬록의 아이템이 동일하고 스택이 가능한 경우
        else if (targetSlot.itemData == sourceSlot.itemData && targetSlot.itemData.isStackable)
        {
            int total = targetSlot.quantity + sourceSlot.quantity;
            int maxStack = targetSlot.itemData.maxStack;
            if (total <= maxStack) // 만약에 넘치지 않으면 그대로 합쳐
            {
                targetSlot.quantity = total;
                sourceSlot.Clear();
            }
            else // 오버플로우하면 일부만 합치고 소스로 돌아가
            {
                targetSlot.quantity = maxStack;
                sourceSlot.quantity = total - maxStack;
            }
        }

        //타겟 슬록에 아이템이 있고, 소스 아이템과 다르거나 쌓을 수 없으면 위치 바꾸기
        else
        {
            ItemSlot temp = new ItemSlot();
            sourceSlot.CopyTo(temp);
            targetSlot.CopyTo(sourceSlot);
            temp.CopyTo(targetSlot);
        }
        InventoryChanged();
        return true;
    }

    // 이벤트를 받으면 인벤토리 변경 알림
    public void InventoryChanged()
    {
        if (onInventoryChangedCallback != null)
        {
            onInventoryChangedCallback.Invoke();
        }
        else Debug.LogWarning("Inventory change event was not invoked");

    }

    /// <summary>
    /// 현재 사용자가 아이템 들고 있는지 확인하는 코드이다.
    /// </summary>
    /// <returns>아이템 정보</returns>
    public ItemData EquippedItem()
    {
        if (SelectedSlotIndex == -1)
        {
            //Debug.Log($"[{name}] : 선택된 슬롯 없음");
            return null; // 선택된 슬롯이 없음
        }

        // 현재 선택된 슬롯의 아이템 가져오기
        // 범위 밖 인덱스 오류로 인해 안전장치 추가
        int selectedRealIndex = SelectedSlotIndex;
        if (selectedRealIndex < 0 || selectedRealIndex >= itemSlots.Count)
        {
            Debug.Log($"[{name}] : 선택된 슬롯 인덱스가 유효 범위를 넘어감");
            return null;
        }
        ItemSlot selectedSlot = itemSlots[selectedRealIndex];

        if (ReferenceEquals(selectedSlot.itemData, null))
        {
            //Debug.Log($"[{name}] : 아이템의 정보가 없음");
            return null;
        }

        return selectedSlot.itemData;
    }

    // ================= SaveManager 보조 함수 =================
    public void SaveInventory()
    {
        GameSave save = SaveService.Load();
        save.inventory = CreateSnapshot();
        SaveService.Save(save);
    }

    public void LoadInventory()
    {
        GameSave save = SaveService.Load();
        ApplySnapshot(save.inventory);
    }

    public List<InventoryItemSaveData> CreateSnapshot()
    {
        List<InventoryItemSaveData> snapshot = new List<InventoryItemSaveData>();
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == null || slot.quantity <= 0) continue;

            snapshot.Add(new InventoryItemSaveData
            {
                itemID = slot.itemData.id,
                quantity = slot.quantity
            });

        }
        return snapshot;
    }

    public void ApplySnapshot(List<InventoryItemSaveData> data)
    {
        foreach (var slotData in data)
        {
            var item = ItemDataBase.Instance.ResolveItem(slotData.itemID);
            if (item != null) AddItem(item, slotData.quantity);
        }
    }
}