using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickTarget : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public enum TargetType { Base, Middle, Top, Perfume, Funnel }
    [SerializeField] TargetType type;
    [Header("기울기 모션")]
    [SerializeField] float tiltAngle = -30f; // 기울일 각도
    [SerializeField] float tiltDuration = 0.25f; // 기울이기/되돌리기 시간
    [SerializeField] float holdDuration = 0.35f; // 기울인 상태 유지 시간
    [SerializeField] int dragSortingOrder = 10;
    Mixture mixture;

    float clickThreshold;
    float pointerDownTime;

    Camera cam;
    SpriteRenderer sr;
    Collider2D col;
    Vector3 originPos;
    Quaternion originRot;
    int originOrder;
    float originZ;
    Vector3 offset;



    void Awake()
    {
        mixture = GetComponentInParent<Mixture>();
        if (mixture == null) Debug.Log("[ClickTarget] 부모에 Mixture 없음");

        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        clickThreshold = Time.time;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Time.time - pointerDownTime <= clickThreshold)
        {
            HandleClick();
        }
    }

    public void HandleClick()
    {
        if (InventoryUIManager.isFullInventoryOpen || (TillUIManager.Instance != null && TillUIManager.Instance.isWarningCanvasOpen)) return;

        switch (type)
        {
            case TargetType.Base:
                // 아이템 소환
                EssenceData baseEssence = currentSelectedEssence();
                if (baseEssence == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(baseEssence, mixture.baseL);
                break;
            case TargetType.Middle:
                // 아이템 소환
                EssenceData middleEssence = currentSelectedEssence();
                if (middleEssence == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(middleEssence, mixture.middleL);
                break;
            case TargetType.Top:
                // 아이템 소환
                EssenceData topEssence = currentSelectedEssence();
                if (topEssence == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(topEssence, mixture.topL);
                break;
            case TargetType.Perfume:
                // 향수가 완성됐는지 확인
                // 아이템 획득
                break;
            case TargetType.Funnel:
                // 베이스,미들,탑 다 들어갔는지 체크
                if (mixture.CanMakePCompleteL())
                {
                    // 빠지는 애니메이션
                    // Destroy();
                }
                break;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (type != TargetType.Base && type != TargetType.Middle && type != TargetType.Top && type != TargetType.Perfume) return;

        originPos = transform.localPosition;
        originRot = transform.localRotation;
        originZ = transform.position.z;

        Vector3 world = ScreenToWorldAtMyZ(eventData.position);
        offset = transform.position - world;

        if (sr != null)
        {
            originOrder = sr.sortingOrder;
            sr.sortingOrder = dragSortingOrder;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        switch (type)
        {
            case TargetType.Base:
            case TargetType.Middle:
            case TargetType.Top:
                // 마우스 따라가기
                Vector3 world = ScreenToWorldAtMyZ(eventData.position);
                Vector3 target = world + offset;
                target.z = originZ;
                transform.position = target;
                break;
            case TargetType.Perfume:
                // 흔들 수 있는지 판단 후(깔떼기가 있는지 없는지)에 마우스 따라가기
                break;
        }
    }

    public void OnEndDrag(PointerEventData evenetData)
    {
        // 위치가 'flowZone' 일 때
        // 따르는 애니메이션
        if (type != TargetType.Base && type != TargetType.Middle && type != TargetType.Top) return;

        if (sr != null) sr.sortingOrder = originOrder;

        bool inZone = isInFlowZone();
        bool canPour = type switch
        {
            TargetType.Base => mixture != null,
            TargetType.Middle => mixture != null,
            TargetType.Top => mixture != null,
            _ => false
        };

        if (inZone && canPour)
        {
            StartCoroutine(TiltRoutine());
        }
        else
        {
            // 일단은 스냅백...
            transform.localPosition = originPos;
            transform.localRotation = originRot;
        }
    }

    Vector3 ScreenToWorldAtMyZ(Vector2 screenPos)
    {
        float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
    }

    public bool isInFlowZone()
    {
        if (col == null) return false;
        var filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = true;

        var results = new List<Collider2D>(8);
        int count = col.Overlap(filter, results);
        for (int i = 0; i < count; i++)
        {
            var c = results[i];
            if (c != null && c.name == "FlowZone") return true;
        }
        return false;
    }

    IEnumerator TiltRoutine()
    {
        Quaternion start = transform.rotation;
        Quaternion tilted = Quaternion.Euler(0f, 0f, tiltAngle) * start;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tiltDuration;
            transform.rotation = Quaternion.Slerp(start, tilted, t);
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        if (mixture != null)
        {
            switch (type)
            {
                case TargetType.Base:
                    // 조건 판단 > 베이스가 없는지
                    if (mixture.CanBePBaseL())
                    {
                        //퍼퓸관에 베이스 넣기
                        mixture.PutBaseInPerfume();
                    }
                    break;
                case TargetType.Middle:
                    // 조건 판단 > 베이스가 이미 있고 미들이 없는지
                    if (mixture.CanBePMiddleL())
                    {
                        //퍼퓸관에 베이스 넣기
                        mixture.PutMiddleInPerfume();
                    }
                    break;
                case TargetType.Top:
                    // 조건 판단 > 베이스, 미들이 이미 있고 탑이 없는지
                    if (mixture.CanBePTopL())
                    {
                        //퍼퓸관에 베이스 넣기
                        mixture.PutTopInPerfume();
                    }
                    break;
            }
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tiltDuration;
            transform.rotation = Quaternion.Slerp(tilted, originRot, t);
            yield return null;
        }

        transform.position = originPos;
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
