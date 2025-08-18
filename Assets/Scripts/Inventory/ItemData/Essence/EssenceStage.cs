using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EssenceStages", menuName = "Stage/EssenceStage")]
public class EssenceStage : ScriptableObject
{
    [Header("진행 단계 설정")]
    public List<Sprite> progressStage; // 각 진행 단계 스프라이트 설정
    public int progressration = 10; // 각 단계 별 걸리는 시간
    public int totalStage => progressStage.Count; // 총 단계 개수
    public int fullProgressIndex => progressStage.Count - 1; // 다 진행된 단계 (마지막 단계)

    void OnValidate()
    {
        if (progressStage.Count == 0)
        {
            Debug.Log($"{name} 해당 진행 스프라이트 설정 안됨");
        }
    }
}
