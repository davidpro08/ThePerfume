using System;
using UnityEngine;

public class HarvestableCrop : MonoBehaviour, IInteract
{
    [Header("농작물 정보")]
    public CropType cropType; // 해당 농작물 종류
    public CropData cropData; // 수확 시 얻을 아이템 데이터
    public int Quantity = 1; // 수확 시 얻을 아이템 수량
    public GameObject collectableItemPrefab; // 바닥에 스폰될 프리팹

    [Header("수확 도구")]
    public ToolType requiredToolType;

    [Header("성장 시스템")]
    public CropStage cropStage; // 성장 단계 데이터
    private SpriteRenderer spriteRenderer; // 작물 스프라이트 변경
    private float timer = 0f; // 현재 단계에서 경과한 시간
    public int currentStage = 0; // 현재 성장 단계

    public bool isFullGrowth => currentStage == cropStage.fullGrowthIndex; // 완전히 자랐는지 여부
    public Farm parentFarm;

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

    // 성장 단계에 맞게 스프라이트 업데이트
    private void UpdateSprite()
    {
        if (spriteRenderer != null && cropStage != null && currentStage < cropStage.growthStage.Count)
        {
            spriteRenderer.sprite = cropStage.growthStage[currentStage];
        }
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;
        
        if(OnHarvested()) Reset();
    }
    
    /// <summary>
    /// 수확을 실행하고 복제품을 떨어뜨립니다.
    /// </summary>
    public bool OnHarvested()
    {
        // Debug.Log($"{name} 수확");
        // Debug.Log($"[HarvestableCrop] collectableItemPrefab 상태: {(collectableItemPrefab != null ? collectableItemPrefab.name : "NULL")}");
        // Debug.Log($"[HarvestableCrop] cropData 상태: {(cropData != null ? cropData.itemName : "NULL")}");

        if (ReferenceEquals(collectableItemPrefab, null) && ReferenceEquals(cropData, null))
        {
            Debug.Log($"{name} : 작물 데이터가 없거나 소환할 프리팹이 없음");
            return false;
        }
        
        Debug.Log($"{name} 수확 시도");
        // 아이템 스폰
        GameObject spawnItem = Instantiate(collectableItemPrefab, transform.position, Quaternion.identity);
        ColletableItem collectable = spawnItem.GetComponent<ColletableItem>();
        
        if (collectable != null)
        {
            // 아이템 초기화
            collectable.Initialize(cropData, Quantity);
        }
        else
        {
            Debug.Log($"{collectableItemPrefab.name}에 collectableItem.cs 없음");
        }

        return true;
    }

    /// <summary>
    /// 수확 후 화분 상태를 초기화합니다.
    /// </summary>
    public void Reset()
    {
        if (parentFarm != null)
        {
            parentFarm.ClearFarm();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 작물 수확에 대한 예외 처리 부분이다.
    /// </summary>

    public bool CanInteract(Player player)
    {
        // 농작물이 존재하지 않는다면
        if (ReferenceEquals(cropData, null))
        {
            Debug.Log($"[{name}] : 농작물 존재하지 않음");
            return false;
        }

        // 작물이 완전히 자랐는지 체크
        if (!isFullGrowth)
        {
            Debug.Log($"[{name}] : [{cropData.name}] 아직 다 안 자라났음. [{currentStage}]");
            return false;
        }

        ToolData toolData = InventoryManager.Instance.EquippedTool();
        
        // 도구를 장착하지 않고 있다면
        if (ReferenceEquals(toolData, null))
        {
            Debug.Log($"[{name}] : 도구 장착하지 않음");
            return false;
        }

        // 도구를 잘못 장착했다면
        if (toolData.toolType != requiredToolType)
        {
            Debug.Log($"[{name}] : 도구 잘못 장착함. " +
                      $"장착한 도구 : [{toolData.toolType}]" +
                      $"필요한 도구 : [{requiredToolType}]");
            return false;
        }

        Debug.Log($"[{name}] : 작물 수확 가능");
        return true;
    }
}
