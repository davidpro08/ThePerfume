using System;
using UnityEngine;

public class HarvestableCrop : MonoBehaviour
{
    [Header("농작물 정보")]
    public CropType cropType; // 해당 농작물 종류
    public CropData cropData; // 수확 시 얻을 아이템 데이터
    public int Quantity = 1; // 수확 시 얻을 아이템 수량

    [Header("수확 도구")]
    public ToolType requiredToolType;

    [Header("성장 시스템")]
    public CropStage cropStage; // 성장 단계 데이터
    private SpriteRenderer spriteRenderer; // 작물 스프라이트 변경
    private float timer = 0f; // 현재 단계에서 경과한 시간
    public int currentStage = 0; // 현재 성장 단계

    public bool isFullGrowth => currentStage == cropStage.fullGrowthIndex; // 완전히 자랐는지 여부

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return; // spriteRenderer 컴포넌트 없음
        if (cropStage == null) return; // cropStage 연결 안됨
        UpdateSprite();
    }

    void Update()
    {
        if (!isFullGrowth)
        {
            timer += Time.deltaTime;

            // 해당 단계 끝
            if (timer >= cropStage.growDuration)
            {
                timer -= cropStage.growDuration; // 남은 시간 처리
                currentStage++;

                Debug.Log($"1단계 더 자라남");

                if (currentStage >= cropStage.totalStage)
                {
                    currentStage = cropStage.fullGrowthIndex; // 마지막 단계 고정
                    Debug.Log($"{cropType}이 모두 자라났음");
                }
                UpdateSprite();
            }
        }
    }

    public bool CanHarvest() // 수확 가능한지 여부
    {
        return isFullGrowth;
    }

    public CropData GetHarvestedItem() // 얻을 cropdata 호출
    {
        return cropData;
    }

    public int GetHarvestedQuantity() // 얻을 작물 양 호출
    {
        return Quantity;
    }

    // 성장 단계에 맞게 스프라이트 업데이트
    private void UpdateSprite()
    {
        if (spriteRenderer != null && cropStage != null && currentStage < cropStage.growthStage.Count)
        {
            spriteRenderer.sprite = cropStage.growthStage[currentStage];
        }
    }

    // 수확 후 화분 상태 초기화 (player 상호작용에 들어갈 예정)
    public void OnHarvested()
    {
        Debug.Log("${cropType} 수확");
    }
}
