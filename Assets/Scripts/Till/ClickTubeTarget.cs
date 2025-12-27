using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickTubeTarget : MonoBehaviour, IPointerClickHandler
{
    public enum TargetType { Fuel, Petal, Essence }
    [SerializeField] TargetType type;
    Distiller distiller;

    void Awake()
    {
        distiller = GetComponentInParent<Distiller>();
        if (distiller == null) Debug.LogError("[ClickTubeTarget] 부모에 Distiller 없음");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (eventData.pointerPressRaycast.module is not Physics2DRaycaster) return;
        if (InventoryUIManager.isFullInventoryOpen) return;

        switch (type)
        {
            case TargetType.Fuel:
                distiller.PlaceFuel();
                break;
            case TargetType.Petal:
                MaterialData petal = currentSelectedPetal();
                if (petal == null)
                {
                    NoticeUIManager.Instance.ShowNoticeCanvas("need petal item");
                    return;
                }
                distiller.PlacePetal(petal);
                break;
            case TargetType.Essence:
                distiller.OnEssenceClicked();
                break;
        }
    }

    private MaterialData currentSelectedPetal()
    {
        if (InventoryManager.Instance == null) return null;

        int index = InventoryManager.Instance.SelectedSlotIndex;
        if (index < 0 || index >= InventoryManager.Instance.itemSlots.Count) return null;

        ItemSlot selectedSlot = InventoryManager.Instance.itemSlots[index];
        if (selectedSlot == null || selectedSlot.itemData == null) return null;

        if (selectedSlot.itemData.itemName == "Fuel") return null;

        return selectedSlot.itemData as MaterialData;
    }
}
