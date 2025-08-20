using System.Collections;
using System.Numerics;
using UnityEngine;
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

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.delta.x;
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp(-deltaX, -rotationStrength, rotationStrength));
        shakeAmount += Mathf.Abs(deltaX) * 0.01f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localRotation = Quaternion.Identity;
        if (shakeAmount >= shakeThreshold)
        {
            mixture.MakingPerfume(mixture.baseData, mixture.middleData, mixture.topData);
            Debug.Log("End making Perfume");
        }
    }

    private void HandleClick()
    {
        if (InventoryUIManager.isFullInventoryOpen || (TillUIManager.Instance != null && TillUIManager.Instance.isWarningCanvasOpen)) return;

        switch (perfumeType)
        {
            case TargetPerfumeType.Perfume:
                // 향수가 완성됐는지 확인
                if (mixture.CanGainPerfume())
                {
                    // 아이템 획득
                    if (mixture.perfumeData == null) return;
                    InventoryManager.Instance.AddItem(mixture.perfumeData, 1);
                    var PCompleteL = mixture.PerfumeL[3].GetComponent<SpriteRenderer>();
                    PCompleteL.sprite = null;

                    // 일단 깔떼기는 아이템 획득하는대로 다시 꽂아둠
                    // 꽂아두는 애니메이션
                    StartCoroutine(RestorePunnelMotion());
                    return;
                }
                break;
            case TargetPerfumeType.Punnel:
                // 베이스,미들,탑 다 들어갔는지 + 깔떼기가 빠졌는지 체크
                if (mixture.CanRemovePunnel())
                {
                    // 빠지는 애니메이션 ( Y축 기준 위로 어느 정도 올라감 )
                    StartCoroutine(RemovePunnelMotion());
                }
                break;
        }
    }

    public IEnumerator RemovePunnelMotion()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = mixture.punnel.transform.localPosition;
        Vector3 targetPos = startPos + Vector3.up * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mixture.punnel.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed);
            yield return null;
        }
        mixture.gameObject.SetActive(false);
    }

    public IEnumerator RestorePunnelMotion()
    {
        mixture.punnel.gameObject.SetActive(true);
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 StartPos = mixture.punnel.transform.localPosition + Vector3.up * 2f;
        Vector3 targetPos = mixture.punnel.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mixture.punnel.transform.localPosition = Vector3.Lerp(StartPos, targetPos, elapsed);
            yield return null;
        }
    }
}
