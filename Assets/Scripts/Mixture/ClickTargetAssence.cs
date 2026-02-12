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

    [SerializeField] float animationDuration = 0.4f;
    [SerializeField] int dragSortingOrder = 15;
    Mixture mixture;

    [SerializeField] float clickThreshold = 0.5f;
    float pointerDownTime;
    public static bool isPouring = false;

    Camera cam;
    SpriteRenderer sr;
    Collider2D col;
    Vector3 originPos;
    Quaternion originRot;
    int originOrder;
    float originZ;
    Vector3 offset;
    private SpriteRenderer essenceSr;
    private int essenceOriginOrder;

    void Awake()
    {
        mixture = GetComponentInParent<Mixture>();
        if (mixture == null) Debug.Log("[ClickTarget] 부모에 Mixture 없음");

        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (liquidTransform != null)
        {
            liquidInitialRotation = liquidTransform.rotation;
            essenceSr = liquidTransform.GetComponent<SpriteRenderer>();
            if (essenceSr != null)
            {
                essenceOriginOrder = essenceSr.sortingOrder;
            }
        }

        mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled = false;
        mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled = false;
        mixture.PerfumeL[2].GetComponent<SpriteRenderer>().enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPouring) return;

        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPouring) return;

        if (Time.time - pointerDownTime <= clickThreshold)
        {
            HandleClick();
        }
    }

    public void HandleClick()
    {
        if (isPouring) return;

        if (InventoryUIManager.isFullInventoryOpen) return;

        switch (essenceType)
        {
            case TargetEssenceType.Base:
                // 아이템 소환
                mixture.baseData = currentSelectedEssence();
                if (mixture.baseData == null)
                {
                    NoticeUIManager.Instance.ShowNoticeCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(mixture.baseData, mixture.baseL);
                SoundManager.Instance.PlaySFX(SFXType.PutEssence);
                break;
            case TargetEssenceType.Middle:
                // 아이템 소환
                mixture.middleData = currentSelectedEssence();
                if (mixture.middleData == null)
                {
                    NoticeUIManager.Instance.ShowNoticeCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(mixture.middleData, mixture.middleL);
                SoundManager.Instance.PlaySFX(SFXType.PutEssence);
                break;
            case TargetEssenceType.Top:
                // 아이템 소환
                mixture.topData = currentSelectedEssence();
                if (mixture.topData == null)
                {
                    NoticeUIManager.Instance.ShowNoticeCanvas("need Essence");
                    return;
                }
                mixture.PlaceEssence(mixture.topData, mixture.topL);
                SoundManager.Instance.PlaySFX(SFXType.PutEssence);
                break;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPouring) return;

        if (essenceType != TargetEssenceType.Base && essenceType != TargetEssenceType.Middle && essenceType != TargetEssenceType.Top) return;

        originPos = transform.position;
        originRot = transform.rotation;
        originZ = transform.position.z;

        Vector3 world = ScreenToWorldAtMyZ(eventData.position);
        offset = transform.position - world;

        if (sr != null)
        {
            originOrder = sr.sortingOrder;
            sr.sortingOrder = dragSortingOrder + 1;
        }
        if (essenceSr != null)
        {
            essenceOriginOrder = essenceSr.sortingOrder;
            essenceSr.sortingOrder = dragSortingOrder;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPouring) return;

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
        if (isPouring) return;
        // 위치가 'flowZone' 일 때
        // 따르는 애니메이션
        if (essenceType != TargetEssenceType.Base && essenceType != TargetEssenceType.Middle && essenceType != TargetEssenceType.Top) return;

        if (sr != null) sr.sortingOrder = originOrder;
        if (essenceSr != null) essenceSr.sortingOrder = essenceOriginOrder;

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
        isPouring = true;
        Quaternion start = transform.rotation;
        Quaternion tilted = Quaternion.Euler(0f, 0f, tiltAngle) * start;

        if (mixture != null)
        {
            // 경고창 호출
            switch (essenceType)
            {
                case TargetEssenceType.Base:
                    if (mixture.baseData == null)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("need Essence");
                        ResetPouringState();
                        yield break;
                    }
                    if (mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled == true && mixture.baseL.GetComponent<SpriteRenderer>().enabled == true)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("Already Base Essence exist in perfume");
                        ResetPouringState();
                        yield break;
                    }
                    break;
                case TargetEssenceType.Middle:
                    if (mixture.middleData == null)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("need Essence");
                        ResetPouringState();
                        yield break;
                    }
                    if (mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled == true && mixture.middleL.GetComponent<SpriteRenderer>().enabled == true)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("Already Middle Essence exist in perfume");
                        ResetPouringState();
                        yield break;
                    }

                    if (!mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("No Base Essence");
                        ResetPouringState();
                        yield break;
                    }
                    break;
                case TargetEssenceType.Top:
                    if (mixture.topData == null)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("need Essence");
                        ResetPouringState();
                        yield break;
                    }
                    if (mixture.PerfumeL[2].GetComponent<SpriteRenderer>().enabled == true && mixture.topL.GetComponent<SpriteRenderer>().enabled == true)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("Already Top Essence exist in perfume");
                        ResetPouringState();
                        yield break;
                    }

                    if (!mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("No Base Essence");
                        ResetPouringState();
                        yield break;
                    }
                    if (!mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled)
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("No Middle Essence");
                        ResetPouringState();
                        yield break;
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

        if (mixture != null)
        {
            bool essencePutSuccess = false;

            switch (essenceType)
            {
                case TargetEssenceType.Base:
                    //Debug.Log("first");
                    if (mixture.CanBePBaseL())
                    {
                        mixture.baseLChild.GetComponent<SpriteRenderer>().color = mixture.baseData.color;
                        mixture.punnelChild.GetComponent<SpriteRenderer>().color = mixture.baseData.color;
                        mixture.baseWaterDropAni.SetTrigger("StartFour");
                        mixture.pBaseLAni.SetTrigger("StartFill");
                        // mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled = true;
                        mixture.punnelAni.SetTrigger("base");
                    }
                    else
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("Already Essence exist in perfume");
                        transform.position = originPos;
                        transform.rotation = originRot;
                        isPouring = false;
                        yield break;
                    }
                    break;
                case TargetEssenceType.Middle:
                    if (mixture.CanBePMiddleL())
                    {
                        mixture.middleLChild.GetComponent<SpriteRenderer>().color = mixture.middleData.color;
                        mixture.punnelChild.GetComponent<SpriteRenderer>().color = mixture.middleData.color;
                        mixture.middleWaterDropAni.SetTrigger("StartFour");
                        mixture.pMiddleLAni.SetTrigger("StartFill");
                        // mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled = true;
                        mixture.punnelAni.SetTrigger("middle");
                    }
                    else
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("Already Essence exist in perfume");
                        ResetPouringState();
                        yield break;
                    }
                    break;
                case TargetEssenceType.Top:
                    if (mixture.CanBePTopL())
                    {
                        mixture.topLChild.GetComponent<SpriteRenderer>().color = mixture.topData.color;
                        mixture.punnelChild.GetComponent<SpriteRenderer>().color = mixture.topData.color;
                        mixture.topWaterDropAni.SetTrigger("StartFour");
                        mixture.pTopLAni.SetTrigger("StartFill");
                        // mixture.PerfumeL[2].GetComponent<SpriteRenderer>().enabled = true;
                        mixture.punnelAni.SetTrigger("top");
                    }
                    else
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("Already Essence exist in perfume");
                        ResetPouringState();
                        yield break;
                    }
                    break;
            }

            switch (essenceType)
            {
                case TargetEssenceType.Base:
                    //Debug.Log("second");
                    // 조건 판단 > 베이스가 없는지
                    if (mixture.PutEssenceInPerfume(mixture.baseData, mixture.PerfumeL[0], mixture.baseL))
                    {
                        //Debug.Log("third");
                        essencePutSuccess = true;
                        mixture.pBaseData = mixture.baseData;
                        mixture.baseData = null;
                        mixture.SaveNow();
                    }

                    break;
                case TargetEssenceType.Middle:
                    // 조건 판단 > 베이스가 이미 있고 미들이 없는지
                    if (mixture.PutEssenceInPerfume(mixture.middleData, mixture.PerfumeL[1], mixture.middleL))
                    {
                        essencePutSuccess = true;
                        mixture.pMiddleData = mixture.middleData;
                        mixture.middleData = null;
                        mixture.SaveNow();
                    }
                    break;
                case TargetEssenceType.Top:
                    // 조건 판단 > 베이스, 미들이 이미 있고 탑이 없는지
                    if (mixture.PutEssenceInPerfume(mixture.topData, mixture.PerfumeL[2], mixture.topL))
                    {
                        essencePutSuccess = true;
                        mixture.pTopData = mixture.topData;
                        mixture.topData = null;
                        mixture.SaveNow();
                    }

                    break;
            }

            if (essencePutSuccess)
            {
                yield return new WaitForSeconds(animationDuration);

                mixture.punnelAni.SetTrigger("end");
                mixture.punnelChild.GetComponent<SpriteRenderer>().sprite = null;
                switch (essenceType)
                {
                    case TargetEssenceType.Base:
                        mixture.baseWaterDropAni.SetTrigger("end");
                        mixture.baseLChild.GetComponent<SpriteRenderer>().sprite = null;
                        mixture.baseL.GetComponent<SpriteRenderer>().enabled = false;
                        mixture.PerfumeL[0].GetComponent<SpriteRenderer>().enabled = true;
                        break;
                    case TargetEssenceType.Middle:
                        mixture.middleWaterDropAni.SetTrigger("end");
                        mixture.middleLChild.GetComponent<SpriteRenderer>().sprite = null;
                        mixture.middleL.GetComponent<SpriteRenderer>().enabled = false;
                        mixture.PerfumeL[1].GetComponent<SpriteRenderer>().enabled = true;
                        break;
                    case TargetEssenceType.Top:
                        mixture.topWaterDropAni.SetTrigger("end");
                        mixture.topLChild.GetComponent<SpriteRenderer>().sprite = null;
                        mixture.topL.GetComponent<SpriteRenderer>().enabled = false;
                        mixture.PerfumeL[2].GetComponent<SpriteRenderer>().enabled = true;
                        break;
                }

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
        mixture.SaveNow();
        isPouring = false;
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

    void ResetPouringState()
    {
        transform.position = originPos;
        transform.rotation = originRot;
        isPouring = false;
    }
}
