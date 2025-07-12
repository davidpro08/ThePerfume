using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Subsystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (droppedItem == null) return;

        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
        if (inventory == null) return;

        ItemSlot targetSlot = inventory.itemSlots[slotIndex];
        ItemSlot sourceSlot = droppedItem.boundSlot;

        Transform originalParent = droppedItem.parentAfterDrag;

        //반약 비어있으면
        if (transform.childCount == 0)
        {
            sourceSlot.CopyTo(targetSlot); //새로운 슬록에 데이터 복제
            sourceSlot.Clear(); //원래 슬롯에서 빼기
            droppedItem.boundSlot = targetSlot;
            droppedItem.parentAfterDrag = transform; //앞으로 보이게 저장
        }
        else //.. 비어있지 않으면 원래 있던 아이템 슬롯의 정보를 불러오고 업데이트
        {
            DraggableItem existingItem = transform.GetChild(0).GetComponent<DraggableItem>();
            ItemSlot existingSlot = existingItem.boundSlot;

            if (existingItem.item == droppedItem.item && existingItem.item.isStackable)
            {
                int total = existingItem.quantity + droppedItem.quantity;
                int maxStack = existingItem.item.maxStackSize;

                if (total <= maxStack)
                {
                    existingSlot.quantity = total;
                    existingItem.quantity = total;
                    existingItem.RefreshCount();

                    sourceSlot.Clear();
                    GameObject.Destroy(droppedItem.gameObject);
                }
                else
                {
                    existingSlot.quantity = maxStack;
                    existingItem.quantity = maxStack;

                    int leftover = total - maxStack;
                    sourceSlot.quantity = leftover;
                    droppedItem.quantity = leftover;
                    droppedItem.RefreshCount();

                }
            }
            else // 다른 아이템이면 위치바꾸기
            {
                //슬롯 바꾸기
                ItemSlot tmpSlot = new ItemSlot();
                sourceSlot.CopyTo(tmpSlot);
                existingSlot.CopyTo(targetSlot);
                tmpSlot.CopyTo(targetSlot);
                //데이터 바꾸기
                existingItem.transform.SetParent(originalParent);
                existingItem.parentAfterDrag = originalParent;
                existingItem.boundSlot = sourceSlot;

                droppedItem.transform.SetParent(transform);
                droppedItem.parentAfterDrag = transform;
                droppedItem.boundSlot = targetSlot;
            }
        }
    }
}
