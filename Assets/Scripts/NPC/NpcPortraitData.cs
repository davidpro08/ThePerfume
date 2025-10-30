using UnityEngine;

/// <summary>
/// NPC 초상화 데이터를 관리하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New NpcPortraitData", menuName = "NPC/Portrait Data")]
public class NpcPortraitData : ScriptableObject
{
    [Header("NPC 정보")]
    public string npcId;
    public string npcName;
    
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

    /// <summary>
    /// 특정 상태에 대한 초상화를 반환합니다.
    /// </summary>
    /// <param name="state">NPC 상태</param>
    /// <returns>해당 상태의 초상화 스프라이트</returns>
    public Sprite GetPortrait(NpcState state)
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
}
