using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    // 인벤토리 일단 리스트로 구현했는데, 배열이 더 나을까요?
    // 슬롯에 아이템을 넣어야 해서 이게 적당하지는 않을 것 같은데
    public List<ItemSlot> itemSlots = new
        List<ItemSlot>();

    // 인벤토리 변경 시 UI 업데이트 등을 위한 이벤트
    public delegate void OnInventoryChanged();

    public event OnInventoryChanged onInventoryChangedCallback;

    // 용량
    // 기본 용량 = 8
    // 나중에 추가 용량을 얻으면 capacity를 늘리기
    public int capacity = 8;


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
                if (slot.itemData == itemAdd && slot.quantity < itemAdd.maxStackSize)
                {
                    // 쌓을 수 있는 남은 공간이 이정도
                    int remainingSpace = itemAdd.maxStackSize - slot.quantity;
                    
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
            int amountToPlace = itemAdd.isStackable ? Mathf.Min(amountOfItems, itemAdd.maxStackSize) : 1;
            
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
    
    
    
    // 이벤트를 받으면 인벤토리 변경 알림
    private void InventoryChanged()
    {
        if (onInventoryChangedCallback != null)
        {
            onInventoryChangedCallback.Invoke();
        }
    }
}