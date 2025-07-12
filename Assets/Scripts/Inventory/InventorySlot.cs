using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.U2D;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    //슬롯 선택 임시 확인 기능 > 아웃라인 두껍게 보임
    public UnityEngine.UI.Outline outlineComponent;
    public Color normalOutlineColor = Color.clear;
    public Color selectedOutlineColor = new Color(1f, 1f, 1f, 0.5f);
    public UnityEngine.Vector2 normalOutlineDistance = UnityEngine.Vector2.zero;
    public UnityEngine.Vector2 selectedOutlineDistance = new UnityEngine.Vector2(3f, 3f);

    public DraggableItem currentDraggableItem;
    private InventoryManager _inventoryManager;

    void Awake()
    {
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<UnityEngine.UI.Outline>();
        }
    }
    void Start()
    {
        _inventoryManager = FindAnyObjectByType<InventoryManager>();
        if (_inventoryManager != null)
        {
            _inventoryManager.onSlotSelectedCallback += OnManagerSlotSelected;
            SetSelected(_inventoryManager.SelectedSlotIndex == slotIndex);
        }
        else
        {
            //InventoryManager 발견 실패
            SetSelected(false);
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
                outlineComponent.effectColor = normalOutlineColor;
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

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (droppedItem == null) return;

        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
        if (inventory == null) return;

        ItemSlot targetSlot = inventory.itemSlots[slotIndex];
        ItemSlot sourceSlot = droppedItem.boundSlot;

        Transform originalParent = droppedItem.parentAfterDrag;

        //�ݾ� ��������� :: 아이템 존재하지 않음
        if (transform.childCount == 0)
        {
            sourceSlot.CopyTo(targetSlot); //���ο� ���Ͽ� ������ ����
            sourceSlot.Clear(); //���� ���Կ��� ����
            //UI ���̴� �� �̵�
            droppedItem.transform.SetParent(transform);

            droppedItem.boundSlot = targetSlot;
            droppedItem.parentAfterDrag = transform; //������ ���̰� ����
        }
        else //.. ������� ������ ���� �ִ� ������ ������ ������ �ҷ����� ������Ʈ ::이미 아이템 존재
        {
            DraggableItem existingItem = transform.GetChild(0).GetComponent<DraggableItem>();
            ItemSlot existingSlot = existingItem.boundSlot;

            // 기존 아이템과 같은 아이템, 쌓을 수 있는 아이템
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
            // 기존 아이템과 다른 아이템, 쌓을 수 없는 아이템
            else // �ٸ� �������̸� ��ġ�ٲٱ�
            {
                //���� �ٲٱ�
                ItemSlot tmpSlot = new ItemSlot();
                sourceSlot.CopyTo(tmpSlot);
                existingSlot.CopyTo(sourceSlot);
                tmpSlot.CopyTo(existingSlot);
                //������ �ٲٱ�
                existingItem.transform.SetParent(originalParent);
                existingItem.parentAfterDrag = originalParent;
                existingItem.boundSlot = sourceSlot;

                droppedItem.transform.SetParent(transform);
                droppedItem.parentAfterDrag = transform;
                droppedItem.boundSlot = existingSlot;
            }
        }
        inventory.InventoryChanged();
    }
}
