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

    private UnityEngine.Vector3 punnelBasePos;
    private bool isShaking = false;

    void Awake()
    {
        mixture = GetComponentInParent<Mixture>();
        if (mixture == null) Debug.LogError("[ClickTargetPerfumeTube] 부모에 Mixture 컴포넌트가 없음");
        punnelBasePos = mixture.punnel.transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ClickTargetAssence.isPouring) return;

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
        if (ClickTargetAssence.isPouring) return;

        if (Time.time - pointerDownTime <= clickThreshold)
        {
            Debug.Log("PointerUp");
            HandleClick();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ClickTargetAssence.isPouring) return;

        if (perfumeType != TargetPerfumeType.PerfumeShaking) return;
        if (mixture.punnel.GetComponent<SpriteRenderer>().enabled == true) return;
        if (mixture.CanGainPerfume()) return;

        if (!mixture.PerfumeL[3].GetComponent<SpriteRenderer>().enabled)
        {
            if (!mixture.PrepareForShaking()) return;
        }

        UnityEngine.Vector3 worldPos = Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane));

        worldPos.z = 0f;

        if (!isShaking && mixture.perfumeCompleteAni != null)
        {
            mixture.perfumeCompleteAni.SetBool("shake", true);
            isShaking = true;
        }

        if (transform.parent != null)
            transform.parent.position = worldPos;

        float deltaX = eventData.delta.x;
        if (transform.parent != null)
            transform.parent.localRotation = UnityEngine.Quaternion.Euler(0, 0, Mathf.Clamp(-deltaX, -rotationStrength, rotationStrength));
        shakeAmount += Mathf.Abs(deltaX) * 0.01f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ClickTargetAssence.isPouring) return;
        if (mixture.CanGainPerfume()) return;
        if (perfumeType != TargetPerfumeType.PerfumeShaking) return;
        if (mixture.punnel.GetComponent<SpriteRenderer>().enabled == true) return;
        StartCoroutine(EndDragCoroutine());
    }

    private IEnumerator EndDragCoroutine()
    {
        if (transform.parent != null)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            UnityEngine.Vector3 startPos = transform.parent.position;
            UnityEngine.Quaternion startRot = transform.parent.rotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.parent.position = UnityEngine.Vector3.Lerp(startPos, originParentPos, t);
                transform.parent.rotation = UnityEngine.Quaternion.Slerp(startRot, originParentRot, t);
                yield return null;
            }
            transform.parent.position = originParentPos;
            transform.parent.rotation = originParentRot;
        }

        if (mixture.perfumeCompleteAni != null)
        {
            mixture.perfumeCompleteAni.SetBool("shake", false);
            isShaking = false;
        }

        if (shakeAmount >= shakeThreshold)
        {
            mixture.MakingPerfume(mixture.pBaseData, mixture.pMiddleData, mixture.pTopData);
            shakeAmount = 0f;
            Debug.Log("End making Perfume");
        }
    }

    private void HandleClick()
    {
        Debug.Log($"[clickTarget] 클릭 시도: {perfumeType}");
        if (ClickTargetAssence.isPouring)
        {
            Debug.Log($"[clickTarget] 실패: isPouring==true");
            return;
        }

        if (InventoryUIManager.isFullInventoryOpen)
        {
            Debug.Log($"[clickTarget] 실패 인벤토리 열려있음");
            return;
        }

        switch (perfumeType)
        {
            case TargetPerfumeType.Perfume:
            case TargetPerfumeType.PerfumeShaking:
                if (mixture == null)
                {
                    Debug.Log($"[clickTarget] 에러: Mixture 참조 없음");
                    return;
                }
                // 향수가 완성됐는지 확인
                bool canGain = mixture.CanGainPerfume();
                Debug.Log($"[clickTarget] CanGainPerfume: {canGain}");
                if (canGain)
                {
                    Debug.Log($"[clickTarget] perfume/perfumeShaking 선택");
                    // 아이템 획득
                    if (mixture.perfumeData == null)
                    {
                        Debug.Log($"[clickTarget] perfumeData이 null임");
                        return;
                    }
                    InventoryManager.Instance.AddItem(mixture.perfumeData, 1);
                    var PCompleteL = mixture.PerfumeL[3].GetComponent<SpriteRenderer>();
                    PCompleteL.enabled = false;

                    mixture.pBaseData = null;
                    mixture.pMiddleData = null;
                    mixture.pTopData = null;
                    mixture.perfumeData = null;
                    mixture.perfumeIsComplete = false;
                    // 일단 깔떼기는 아이템 획득하는대로 다시 꽂아둠
                    // 꽂아두는 애니메이션
                    StartCoroutine(RestorePunnelMotion());
                    // mixture.SaveNow();
                    return;
                }
                break;
            case TargetPerfumeType.Punnel:
                // 베이스,미들,탑 다 들어갔는지
                if (mixture.CanRemovePunnel())
                {
                    // 빠지는 애니메이션 ( Y축 기준 위로 어느 정도 올라감 )
                    StartCoroutine(RemovePunnelMotion());
                    // mixture.SaveNow();
                }
                else
                {
                    //TillUIManager.Instance.ShowWarningCanvas("cannot make perfume");
                }
                break;
        }
    }

    public IEnumerator RemovePunnelMotion()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        UnityEngine.Vector3 startPos = punnelBasePos;// mixture.punnel.transform.localPosition;
        UnityEngine.Vector3 targetPos = startPos + UnityEngine.Vector3.up * 2f;

        mixture.punnel.transform.localPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mixture.punnel.transform.localPosition = UnityEngine.Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        mixture.punnel.transform.localPosition = targetPos;

        mixture.punnel.GetComponent<SpriteRenderer>().enabled = false;
        shakeAmount = 0f;

        mixture.SaveNow();
    }

    public IEnumerator RestorePunnelMotion()
    {
        mixture.punnel.GetComponent<SpriteRenderer>().enabled = true;
        float duration = 0.5f;
        float elapsed = 0f;
        UnityEngine.Vector3 startPos = punnelBasePos + UnityEngine.Vector3.up * 2f; // mixture.punnel.transform.localPosition;
        UnityEngine.Vector3 targetPos = punnelBasePos; // mixture.punnel.transform.localPosition - UnityEngine.Vector3.up * 2f;

        mixture.punnel.transform.localPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mixture.punnel.transform.localPosition = UnityEngine.Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        mixture.punnel.transform.localPosition = targetPos;

        shakeAmount = 0f;

        mixture.SaveNow();
    }
}
