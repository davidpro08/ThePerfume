using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC 상태에 따른 초상화 변경을 관리하는 컴포넌트
/// </summary>
public class NpcPortraitManager : MonoBehaviour
{
    [Header("초상화 설정")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Npc npc;
    
    [Header("상태별 초상화")]
    [SerializeField] private Sprite defaultPortrait;
    [SerializeField] private Sprite happyPortrait;
    [SerializeField] private Sprite sadPortrait;
    [SerializeField] private Sprite angryPortrait;
    [SerializeField] private Sprite surprisedPortrait;
    [SerializeField] private Sprite confusedPortrait;
    [SerializeField] private Sprite excitedPortrait;
    [SerializeField] private Sprite calmPortrait;
    [SerializeField] private Sprite nervousPortrait;
    [SerializeField] private Sprite friendlyPortrait;
    [SerializeField] private Sprite hostilePortrait;

    private NpcState currentState = NpcState.Default;

    void Start()
    {
        if (portraitImage == null)
        {
            portraitImage = GetComponent<Image>();
        }

        if (npc == null)
        {
            npc = GetComponent<Npc>();
        }

        // 기본 초상화 설정
        UpdatePortrait(NpcState.Default);
    }

    /// <summary>
    /// NPC 상태 변경 시 초상화 업데이트
    /// </summary>
    /// <param name="newState">새로운 NPC 상태</param>
    public void UpdatePortrait(NpcState newState)
    {
        if (portraitImage == null) return;

        currentState = newState;
        Sprite newPortrait = GetPortraitForState(newState);
        
        if (newPortrait != null)
        {
            portraitImage.sprite = newPortrait;
            Debug.Log($"NPC 초상화를 {newState} 상태로 변경했습니다.");
        }
        else
        {
            Debug.LogWarning($"{newState} 상태에 대한 초상화가 설정되지 않았습니다. 기본 초상화를 사용합니다.");
            portraitImage.sprite = defaultPortrait;
        }
    }

    /// <summary>
    /// 상태에 따른 초상화 반환
    /// </summary>
    /// <param name="state">NPC 상태</param>
    /// <returns>해당 상태의 초상화 스프라이트</returns>
    private Sprite GetPortraitForState(NpcState state)
    {
        switch (state)
        {
            case NpcState.Default:
                return defaultPortrait;
            case NpcState.Happy:
                return happyPortrait;
            case NpcState.Sad:
                return sadPortrait;
            case NpcState.Angry:
                return angryPortrait;
            case NpcState.Surprised:
                return surprisedPortrait;
            case NpcState.Confused:
                return confusedPortrait;
            case NpcState.Excited:
                return excitedPortrait;
            case NpcState.Calm:
                return calmPortrait;
            case NpcState.Nervous:
                return nervousPortrait;
            case NpcState.Friendly:
                return friendlyPortrait;
            case NpcState.Hostile:
                return hostilePortrait;
            default:
                return defaultPortrait;
        }
    }

    /// <summary>
    /// 현재 NPC 상태 반환
    /// </summary>
    /// <returns>현재 NPC 상태</returns>
    public NpcState GetCurrentState()
    {
        return currentState;
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
    /// NPC 컴포넌트 설정
    /// </summary>
    /// <param name="npcComponent">NPC 컴포넌트</param>
    public void SetNpc(Npc npcComponent)
    {
        npc = npcComponent;
    }
}
