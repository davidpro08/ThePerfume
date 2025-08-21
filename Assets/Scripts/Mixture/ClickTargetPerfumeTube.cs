using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickTargetPerfumeTube : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    public enum TargetPerfumeType { Perfume, PerfumeShaking, Punnel }
    [SerializeField] TargetPerfumeType perfumeType;
    [SerializeField] float clickThreshold = 0.5f;
    float pointerDownTime;
    Mixture mixture;
    float shakeAmount = 5f;
    float shakeThreshold = 30f;
    float rotationStrength = 15f;

    private UnityEngine.Vector3 originParentPos;
    private UnityEngine.Quaternion originParentRot;

    void Awake()
    {
        mixture = GetComponentInParent<Mixture>();
        if (mixture == null) Debug.LogError("[ClickTargetPerfumeTube] 부모에 Mixture 컴포넌트가 없음");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("PointerDown");
        pointerDownTime = Time.time;

        if (perfumeType == TargetPerfumeType.PerfumeShaking && transform.parent != null)
        {
            originParentPos = transform.parent.position;
            originParentRot = transform.parent.rotation;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Time.time - pointerDownTime <= clickThreshold)
        {
            Debug.Log("PointerUp");
            HandleClick();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (perfumeType != TargetPerfumeType.PerfumeShaking) return;
        if (mixture.punnel.GetComponent<SpriteRenderer>().enabled == true) return;

        UnityEngine.Vector3 worldPos = Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane));

        worldPos.z = 0f;

        if (transform.parent != null)
            transform.parent.position = worldPos;

        float deltaX = eventData.delta.x;
        if (transform.parent != null)
            transform.parent.localRotation = UnityEngine.Quaternion.Euler(0, 0, Mathf.Clamp(-deltaX, -rotationStrength, rotationStrength));
        shakeAmount += Mathf.Abs(deltaX) * 0.01f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (perfumeType != TargetPerfumeType.PerfumeShaking) return;
        if (transform.parent != null)
        {
            transform.parent.position = originParentPos;
            transform.parent.rotation = originParentRot;
        }

        if (mixture.punnel.GetComponent<SpriteRenderer>().enabled == true) return;

        if (shakeAmount >= shakeThreshold)
        {
            mixture.MakingPerfume(mixture.baseData, mixture.middleData, mixture.topData);
            shakeAmount = 0f;
            Debug.Log("End making Perfume");
        }
    }

    private void HandleClick()
    {
        if (InventoryUIManager.isFullInventoryOpen || (TillUIManager.Instance != null && TillUIManager.Instance.isWarningCanvasOpen)) return;

        switch (perfumeType)
        {
            case TargetPerfumeType.Perfume:
            case TargetPerfumeType.PerfumeShaking:
                // 향수가 완성됐는지 확인
                if (mixture.CanGainPerfume())
                {
                    // 아이템 획득
                    if (mixture.perfumeData == null) return;
                    InventoryManager.Instance.AddItem(mixture.perfumeData, 1);
                    var PCompleteL = mixture.PerfumeL[3].GetComponent<SpriteRenderer>();
                    PCompleteL.enabled = false;

                    mixture.baseData = null;
                    mixture.middleData = null;
                    mixture.topData = null;
                    mixture.perfumeData = null;

                    // 일단 깔떼기는 아이템 획득하는대로 다시 꽂아둠
                    // 꽂아두는 애니메이션
                    StartCoroutine(RestorePunnelMotion());
                    return;
                }
                break;
            case TargetPerfumeType.Punnel:
                // 베이스,미들,탑 다 들어갔는지
                if (mixture.CanRemovePunnel())
                {
                    // 빠지는 애니메이션 ( Y축 기준 위로 어느 정도 올라감 )
                    StartCoroutine(RemovePunnelMotion());
                }
                else
                {
                    TillUIManager.Instance.ShowWarningCanvas("cannot make perfume");
                }
                break;
        }
    }

    public IEnumerator RemovePunnelMotion()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        UnityEngine.Vector3 startPos = mixture.punnel.transform.localPosition;
        UnityEngine.Vector3 targetPos = startPos + UnityEngine.Vector3.up * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mixture.punnel.transform.localPosition = UnityEngine.Vector3.Lerp(startPos, targetPos, elapsed);
            yield return null;
        }
        mixture.punnel.GetComponent<SpriteRenderer>().enabled = false;
        shakeAmount = 0f;
    }

    public IEnumerator RestorePunnelMotion()
    {
        mixture.punnel.GetComponent<SpriteRenderer>().enabled = true;
        float duration = 0.5f;
        float elapsed = 0f;
        UnityEngine.Vector3 StartPos = mixture.punnel.transform.localPosition;
        UnityEngine.Vector3 targetPos = mixture.punnel.transform.localPosition - UnityEngine.Vector3.up * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mixture.punnel.transform.localPosition = UnityEngine.Vector3.Lerp(StartPos, targetPos, elapsed);
            yield return null;
        }

        shakeAmount = 0f;
    }
}
