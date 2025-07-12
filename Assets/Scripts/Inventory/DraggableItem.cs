using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    public TextMeshProUGUI countText;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public ItemData item;
    [HideInInspector] public int quantity = 1;

    [HideInInspector] public ItemSlot boundSlot;

    public void InitializeItem(ItemData newItem)
    {
        item = newItem;
        image.sprite = newItem.itemIcon;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = quantity.ToString();
        bool textActive = quantity > 1;
        countText.gameObject.SetActive(textActive);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        countText.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
        countText.raycastTarget = true;
    }

    public void BindToSlot(ItemSlot slot)
    {
        boundSlot = slot;
        item = slot.itemData;
        quantity = slot.quantity;
        image.sprite = item.itemIcon;
        RefreshCount();
    }

}
