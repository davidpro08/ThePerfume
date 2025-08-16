using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI에서 NPC 초상화를 표시하는 컴포넌트
/// </summary>
public class NpcPortraitDisplay : MonoBehaviour
{
    [Header("초상화 표시 설정")]
    [SerializeField] private string targetNpcId; // 표시할 NPC ID
    [SerializeField] private Image portraitImage; // 초상화를 표시할 이미지 컴포넌트
    
    [Header("기본 초상화")]
    [SerializeField] private Sprite defaultPortrait; // 기본 초상화 (NPC 데이터가 없을 때 사용)

    void Start()
    {
        if (portraitImage == null)
        {
            portraitImage = GetComponent<Image>();
        }

        // 기본 초상화 설정
        if (portraitImage != null && defaultPortrait != null)
        {
            portraitImage.sprite = defaultPortrait;
        }

        // 초상화 변경 이벤트 구독
        if (NpcPortraitEventManager.Instance != null)
        {
            NpcPortraitEventManager.Instance.SubscribeToNpcPortraitChange(targetNpcId, OnPortraitChanged);
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (NpcPortraitEventManager.Instance != null)
        {
            NpcPortraitEventManager.Instance.UnsubscribeFromPortraitChange(OnPortraitChanged);
        }
    }

    /// <summary>
    /// 초상화 변경 이벤트 콜백
    /// </summary>
    private void OnPortraitChanged(string npcId, NpcState newState, Sprite newPortrait)
    {
        if (npcId == targetNpcId && portraitImage != null && newPortrait != null)
        {
            portraitImage.sprite = newPortrait;
            Debug.Log($"UI 초상화 업데이트: NPC {npcId} -> {newState} 상태");
        }
    }

    /// <summary>
    /// 표시할 NPC ID 설정
    /// </summary>
    /// <param name="npcId">NPC ID</param>
    public void SetTargetNpcId(string npcId)
    {
        // 기존 구독 해제
        if (NpcPortraitEventManager.Instance != null)
        {
            NpcPortraitEventManager.Instance.UnsubscribeFromPortraitChange(OnPortraitChanged);
        }

        targetNpcId = npcId;

        // 새로운 NPC에 구독
        if (NpcPortraitEventManager.Instance != null)
        {
            NpcPortraitEventManager.Instance.SubscribeToNpcPortraitChange(targetNpcId, OnPortraitChanged);
        }
    }

    /// <summary>
    /// 초상화 이미지 컴포넌트 설정
    /// </summary>
    /// <param name="image">초상화 이미지 컴포넌트</param>
    public void SetPortraitImage(Image image)
    {
        portraitImage = image;
    }

    /// <summary>
    /// 기본 초상화 설정
    /// </summary>
    /// <param name="portrait">기본 초상화 스프라이트</param>
    public void SetDefaultPortrait(Sprite portrait)
    {
        defaultPortrait = portrait;
        if (portraitImage != null)
        {
            portraitImage.sprite = defaultPortrait;
        }
    }
}
