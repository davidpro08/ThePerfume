using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickTargetAssence : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum TargetEssenceType { Base, Middle, Top }
    [SerializeField] TargetEssenceType essenceType;
    [SerializeField] private Transform liquidTransform;
    private Quaternion liquidInitialRotation;
    [Header("기울기 모션")]
    [SerializeField] float tiltAngle = -30f; // 기울일 각도
    [SerializeField] float tiltDuration = 0.25f; // 기울이기/되돌리기 시간
    [SerializeField] float holdDuration = 0.35f; // 기울인 상태 유지 시간
    [SerializeField] int dragSortingOrder = 10;
    Mixture mixture;

    [SerializeField] float clickThreshold = 0.5f;
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

        if (liquidTransform != null) liquidInitialRotation = liquidTransform.rotation;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("PointerDown");
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Time.time - pointerDownTime <= clickThreshold)
        {
            Debug.Log("PointerUp");
            HandleClick();
        }
    }

    public void HandleClick()
    {
        if (InventoryUIManager.isFullInventoryOpen || (TillUIManager.Instance != null && TillUIManager.Instance.isWarningCanvasOpen)) return;

        switch (essenceType)
        {
            case TargetEssenceType.Base:
                // 아이템 소환
                mixture.baseData = currentSelectedEssence();
                if (mixture.baseData == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(mixture.baseData, mixture.baseL);
                break;
            case TargetEssenceType.Middle:
                // 아이템 소환
                mixture.middleData = currentSelectedEssence();
                if (mixture.middleData == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(mixture.middleData, mixture.middleL);
                break;
            case TargetEssenceType.Top:
                // 아이템 소환
                mixture.topData = currentSelectedEssence();
                if (mixture.topData == null)
                {
                    TillUIManager.Instance.ShowWarningCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(mixture.topData, mixture.topL);
                break;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (essenceType != TargetEssenceType.Base && essenceType != TargetEssenceType.Middle && essenceType != TargetEssenceType.Top) return;

        originPos = transform.position;
        originRot = transform.rotation;
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
        switch (essenceType)
        {
            case TargetEssenceType.Base:
            case TargetEssenceType.Middle:
            case TargetEssenceType.Top:
                // 마우스 따라가기
                Vector3 world = ScreenToWorldAtMyZ(eventData.position);
                Vector3 target = world + offset;
                target.z = originZ;
                transform.position = target;
                break;
        }
    }

    public void OnEndDrag(PointerEventData evenetData)
    {
        // 위치가 'flowZone' 일 때
        // 따르는 애니메이션
        if (essenceType != TargetEssenceType.Base && essenceType != TargetEssenceType.Middle && essenceType != TargetEssenceType.Top) return;

        if (sr != null) sr.sortingOrder = originOrder;

        bool inZone = isInFlowZone();
        bool canPour = essenceType switch
        {
            TargetEssenceType.Base => mixture != null,
            TargetEssenceType.Middle => mixture != null,
            TargetEssenceType.Top => mixture != null,
            _ => false
        };

        if (inZone && canPour)
        {
            StartCoroutine(TiltRoutine());
        }
        else
        {
            // 일단은 스냅백...
            transform.position = originPos;
            transform.rotation = originRot;
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

        if (mixture != null)
        {
            // 경고창 호출
            switch (essenceType)
            {
                case TargetEssenceType.Base:
                    if (mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled == true && mixture.baseL.GetComponent<SpriteRenderer>().enabled == true)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("Already Base Essence exist in perfume");
                        yield return null;
                    }
                    break;
                case TargetEssenceType.Middle:
                    if (mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled == true && mixture.middleL.GetComponent<SpriteRenderer>().enabled == true)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("Already Middle Essence exist in perfume");
                        yield return null;
                    }

                    if (!mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("No Base Essence");
                        yield return null;
                    }
                    break;
                case TargetEssenceType.Top:
                    if (mixture.PerfumeL[2].GetComponent<SpriteRenderer>().enabled == true && mixture.topL.GetComponent<SpriteRenderer>().enabled == true)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("Already Top Essence exist in perfume");
                        yield return null;
                    }

                    if (!mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("No Base Essence");
                        yield return null;
                    }
                    if (!mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("No Middle Essence");
                        yield return null;
                    }
                    break;
            }
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tiltDuration;
            transform.rotation = Quaternion.Slerp(start, tilted, t);

            // if (liquidTransform != null) liquidTransform.rotation = liquidInitialRotation;
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        if (mixture != null)
        {
            switch (essenceType)
            {
                case TargetEssenceType.Base:
                    // 조건 판단 > 베이스가 없는지
                    if (mixture.CanBePBaseL())
                    {
                        //퍼퓸관에 베이스 넣기
                        if (mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled == true)
                        {
                            TillUIManager.Instance.ShowWarningCanvas("Already Essence exist in perfume");
                            yield return null;
                        }
                        else if (mixture.PutEssenceInPerfume(mixture.baseData, mixture.PerfumeL[0], mixture.baseL))
                        {
                            Debug.Log("Put Base in Perfume");
                            mixture.pBaseData = mixture.baseData;
                            mixture.baseData = null;
                            mixture.SaveNow();
                        }
                    }
                    break;
                case TargetEssenceType.Middle:
                    // 조건 판단 > 베이스가 이미 있고 미들이 없는지
                    if (mixture.CanBePMiddleL())
                    {
                        //퍼퓸관에 미들 넣기
                        if (mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled == true)
                        {
                            TillUIManager.Instance.ShowWarningCanvas("Already Essence exist in perfume");
                            yield return null;
                        }
                        else if (mixture.PutEssenceInPerfume(mixture.middleData, mixture.PerfumeL[1], mixture.middleL))
                        {
                            mixture.pMiddleData = mixture.middleData;
                            mixture.middleData = null;
                            Debug.Log($"[{mixture.pMiddleData}, {mixture.middleData}]");
                            mixture.SaveNow();
                        }
                    }
                    break;
                case TargetEssenceType.Top:
                    // 조건 판단 > 베이스, 미들이 이미 있고 탑이 없는지
                    if (mixture.CanBePTopL())
                    {
                        if (mixture.PerfumeL[2].GetComponent<SpriteRenderer>().enabled == true)
                        {
                            TillUIManager.Instance.ShowWarningCanvas("Already Essence exist in perfume");
                            yield return null;
                        }
                        //퍼퓸관에 탑 넣기
                        else if (mixture.PutEssenceInPerfume(mixture.topData, mixture.PerfumeL[2], mixture.topL))
                        {
                            mixture.pTopData = mixture.topData;
                            mixture.topData = null;
                            Debug.Log($"[{mixture.pTopData}, {mixture.topData}]");
                            mixture.SaveNow();
                        }
                    }
                    break;
            }
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tiltDuration;
            transform.rotation = Quaternion.Slerp(tilted, originRot, t);

            if (liquidTransform != null) liquidTransform.rotation = Quaternion.Slerp(tilted, originRot, t);
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
