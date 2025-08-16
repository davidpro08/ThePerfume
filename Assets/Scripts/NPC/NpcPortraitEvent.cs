using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// NPC 초상화 변경 이벤트를 위한 클래스
/// </summary>
[System.Serializable]
public class NpcPortraitChangeEvent : UnityEvent<string, NpcState, Sprite> { }

/// <summary>
/// NPC 초상화 이벤트 매니저 (싱글톤)
/// </summary>
public class NpcPortraitEventManager : MonoBehaviour
{
    public static NpcPortraitEventManager Instance { get; private set; }

    [Header("이벤트")]
    public NpcPortraitChangeEvent OnPortraitChanged = new NpcPortraitChangeEvent();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 초상화 변경 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="npcId">NPC ID</param>
    /// <param name="newState">새로운 상태</param>
    /// <param name="newPortrait">새로운 초상화</param>
    public void TriggerPortraitChange(string npcId, NpcState newState, Sprite newPortrait)
    {
        OnPortraitChanged.Invoke(npcId, newState, newPortrait);
        Debug.Log($"초상화 변경 이벤트 발생: NPC {npcId} -> {newState} 상태");
    }

    /// <summary>
    /// 특정 NPC의 초상화 변경 이벤트를 구독합니다.
    /// </summary>
    /// <param name="npcId">구독할 NPC ID</param>
    /// <param name="callback">콜백 함수</callback>
    public void SubscribeToNpcPortraitChange(string npcId, UnityAction<string, NpcState, Sprite> callback)
    {
        OnPortraitChanged.AddListener((id, state, portrait) =>
        {
            if (id == npcId)
            {
                callback(id, state, portrait);
            }
        });
    }

    /// <summary>
    /// 초상화 변경 이벤트 구독을 해제합니다.
    /// </summary>
    /// <param name="callback">해제할 콜백 함수</callback>
    public void UnsubscribeFromPortraitChange(UnityAction<string, NpcState, Sprite> callback)
    {
        OnPortraitChanged.RemoveListener(callback);
    }
}
