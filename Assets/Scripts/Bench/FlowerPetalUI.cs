using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class FlowerPetalUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private FlowerManager flowerManager;
    private bool isHandled = false; // 꽃잎이 뜯겼는지
    [SerializeField] private float dragHandleThreshold = 60f; // 꽃잎 뜯는 거리? 값
    private RectTransform rectTransform; // 꽃잎 자신 위치
    private RectTransform centerRect; // 중심 위치 (center)
    private Vector2 initialAnchoredPos; // 초기 위치

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(FlowerManager manager, RectTransform center)
    {
        this.flowerManager = manager;
        this.centerRect = center;
        this.isHandled = false;

        initialAnchoredPos = rectTransform.anchoredPosition;
    }

    // 클릭 시 뜯기 로직
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isHandled) return;
        Debug.Log($"{gameObject.name} 꽃잎 클릭");
    }

    // 드래그로 뜯기
    public void OnDrag(PointerEventData eventData)
    {
        if (isHandled) return;


        // =================================================
        // 꽃잎 UI 마우스 커서 따라가기
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector2 localPointPos);
        rectTransform.anchoredPosition = localPointPos;
        // =================================================
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (centerRect == null)
        {
            Debug.LogWarning("centerRect == null");
            return;
        }
        // 드래그 일정 거리? 넘었는지 확인 후 뜯기 완
        float distanceFromCenter = Vector2.Distance(rectTransform.anchoredPosition, centerRect.anchoredPosition);
        Debug.Log($"현재 거리 : {distanceFromCenter}");

        if (distanceFromCenter >= dragHandleThreshold)
        {
            Debug.Log($"뜯겨야하는데");
            HarvestPetal();
        }
        else
        {
            rectTransform.anchoredPosition = initialAnchoredPos;
            Debug.Log("원래 위치로 돌아가기");
        }
    }

    // ==================================================
    // 꽃잎 뜯기
    private void HarvestPetal()
    {
        isHandled = true;
        SoundManager.Instance.PlaySFX(SFXType.FlowerTrim);
        gameObject.SetActive(false);

        if (flowerManager != null)
        {
            Debug.Log($"[FLowerPetalUI] flowerManager != null");
            flowerManager.AddPetalToBowl(this);
        }
        else
        {
            Debug.LogWarning($"[FlowerPetalUI] FlowerManager == null");
        }
        isHandled = false;
    }
    // ==================================================
}
