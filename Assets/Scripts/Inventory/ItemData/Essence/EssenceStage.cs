using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EssenceStages", menuName = "Stage/EssenceStage")]
public class EssenceStage : ScriptableObject
{
    [Header("진행 단계 설정")]
    public List<Sprite> growthStage; // 각 성장 단계 스프라이트 설정
    public int growDuration = 10; // 각 단계 별 걸리는 시간
    public int totalStage => growthStage.Count; // 총 단계 개수
    public int fullGrowthIndex => growthStage.Count - 1; // 다 자란 단계 (마지막 단계)

    void OnValidate()
    {
        if (growthStage.Count == 0)
        {
            Debug.Log($"{name} 해당 성장 스프라이트 설정 안됨");
        }
    }
}
