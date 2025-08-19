using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickTarget : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
{
    public enum TargetType { Base, Middle, Top, Perfume, Funnel }
    [SerializeField] TargetType type;
    Mixture mixture;

    void Awake()
    {
        mixture = GetComponent<Mixture>();
        if (mixture == null) Debug.Log("[ClickTarget] 부모에 Mixture 없음");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (eventData.pointerPressRaycast.module is not Physics2DRaycaster) return;
        if (InventoryUIManager.isFullInventoryOpen || (TillUIManager.Instance != null && TillUIManager.Instance.isWarningCanvasOpen)) return;

        switch (type)
        {
            case TargetType.Base:
            // 아이템 소환
            case TargetType.Middle:
            // 아이템 소환
            case TargetType.Top:
                // 아이템 소환
                EssenceData essence = currentSelectedEssence();
                if (essence == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(essence);
                break;
            case TargetType.Perfume:
                // 향수가 완성됐는지 확인
                // 아이템 획득
                break;
            case TargetType.Funnel:
                // 베이스,미들,탑 다 들어갔는지 체크
                break;
        }
    }

    public void OnDrag(PointerEventData evenetData)
    {
        switch (type)
        {
            case TargetType.Base:
            case TargetType.Middle:
            case TargetType.Top:
                // 마우스 따라가기
                break;
            case TargetType.Perfume:
                // 흔들 수 있는지 판단 후(깔떼기가 있는지 없는지)에 마우스 따라가기
                break;
        }
    }

    public void OnEndDrag(PointerEventData evenetData)
    {
        switch (type)
        {
            case TargetType.Base:
                // 위치가 '퍼퓸' 일 때
                // 조건 판단 > 베이스가 없는지
                // 퍼퓸관에 베이스 넣기
                break;
            case TargetType.Middle:
                // 위치가 '퍼퓸' 일 때
                // 조건 판단 > 베이스가 이미 있고 미들이 없는지
                // 퍼퓸관에 미들 넣기
                break;
            case TargetType.Top:
                // 위치가 '퍼퓸' 일 때
                // 조건 판단 > 베이스, 미들이 이미 있고 탑이 없는지
                // 퍼퓸관에 탑 넣기
                break;
        }
    }

    private EssenceData currentSelectedEssence()
    {
        if (InventoryManager.Instance == null) return null;

        int index = InventoryManager.Instance.SelectedSlotIndex;
        if (index < 0 || index >= InventoryManager.Instance.itemSlots.Count) return null;

        ItemSlot selectedSlot = InventoryManager.Instance.itemSlots[index];
        if (selectedSlot == null || selectedSlot.itemData == null) return null;

        return selectedSlot.itemData as EssenceData;
    }
}
