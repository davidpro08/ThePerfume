using UnityEngine;
using UnityEngine.EventSystems;

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

        // 빈 슬롯일 경우
        if (transform.childCount == 0)
        {
            sourceSlot.CopyTo(targetSlot); //새로운 슬롯에 데이터 복사
            sourceSlot.Clear(); //이전 슬롯에서 삭제
            //UI 오브젝트의 부모 변경
            droppedItem.transform.SetParent(transform);

            droppedItem.boundSlot = targetSlot;
            droppedItem.parentAfterDrag = transform; //부모가 현재 슬롯으로 변경
        }
        else //.. 아이템이 이미 있는 슬롯에 드롭했을때
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
                else //overflow
                {
                    existingSlot.quantity = maxStack;
                    existingItem.quantity = maxStack;
                    existingItem.RefreshCount();

                    int leftover = total - maxStack;
                    sourceSlot.quantity = leftover;
                    droppedItem.quantity = leftover;
                    droppedItem.RefreshCount();

                }
            }
            else // 다른 아이템이면 위치바꾸기
            {
                //데이터 바꾸기
                ItemSlot tmpSlot = new ItemSlot();
                sourceSlot.CopyTo(tmpSlot);
                existingSlot.CopyTo(sourceSlot);
                tmpSlot.CopyTo(existingSlot);
                
                //오브젝트 바꾸기
                existingItem.transform.SetParent(originalParent);
                existingItem.parentAfterDrag = originalParent;
                existingItem.boundSlot = sourceSlot;
                existingItem.transform.localPosition = Vector3.zero;

                droppedItem.transform.SetParent(transform);
                droppedItem.parentAfterDrag = transform;
                droppedItem.boundSlot = existingSlot;
                droppedItem.transform.localPosition = Vector3.zero;
            }
        }
        inventory.InventoryChanged();
    }
}