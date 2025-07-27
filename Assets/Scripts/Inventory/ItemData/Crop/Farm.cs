using UnityEngine;

public class Farm : MonoBehaviour, IInteract
{
    [Header("화분 상태")]
    public bool isOccupied = false; //이미 씨앗이 심어져 있는지
    public HarvestableCrop currentCropInstance; // 현재 심어진 작물의 인스턴스
    private SpriteRenderer farmSpriteRenderer;
    public Sprite emptyFarmSprite; // 마른 화분 스프라이트
    public Sprite wateredFarmSprite; // 젖은 화분 스프라이트
    private Collider2D farmCollider; // 수확 때 생기는 오류 수정용 > 작물 다 자라면 farm의 collider 끔
    public bool isWatered = false; // 마른 상태인지 젖은 상태인지 확인

    void Awake()
    {
        farmSpriteRenderer = GetComponent<SpriteRenderer>(); // 초기화
        Debug.Log($"[farm] 현재 isOcuppied = false");
        farmCollider = GetComponent<Collider2D>();
        isOccupied = false;
        isWatered = false;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (farmSpriteRenderer != null)
        {
            farmSpriteRenderer.enabled = true;
            farmSpriteRenderer.sprite = isWatered ? wateredFarmSprite : emptyFarmSprite;
        }
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;

        ItemData itemData = InventoryManager.Instance.EquippedItem();

        SeedData seedData = Caster.CastTo<SeedData>(itemData);
        
        // 씨앗 데이터이면 심을 수 있는 지 확인 
        if (seedData != null)
        {
            Debug.Log($"[farm] [PlantSeed] 현재 isOccupied:{isOccupied}");
            Debug.Log($"[farm] [Watering] 현재 isWatered:{isWatered}");
            PlantSeed(seedData);
            InventoryManager.Instance.RemoveItem(seedData, 1); // 아이템 제거
            return;
        }
        
        ToolData toolData = Caster.CastTo<ToolData>(itemData);
        
        switch (toolData.toolType)
        {
            case ToolType.WateringCan:
                // 물 주는 애니메이션이 이 아래에 들어가야함
                // 여기!
                toolData.nowDurability -= toolData.useDurability; // 내구도 감소
                if (!isWatered)
                {
                    isWatered = true;
                    
                    if (farmSpriteRenderer != null)
                    {
                        farmSpriteRenderer.enabled = true;
                        farmSpriteRenderer.sprite = isWatered ? wateredFarmSprite : emptyFarmSprite;
                    }
                }
                else
                {
                    Debug.Log($"[player] 이미 화분 젖은 상태");
                }

                break;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="seedData"></param>
    /// <returns></returns>
    public void PlantSeed(SeedData seedData)
    {
        // 작물 오브젝트 생성
        GameObject cropGO = Instantiate(seedData.cropPrefabToGrow, transform.position, Quaternion.identity, transform);
        currentCropInstance = cropGO.GetComponent<HarvestableCrop>();
        
        // 작물의 초기 상태 = 심은 직후 상태
        currentCropInstance.parentFarm = this;
        currentCropInstance.currentStage = 0;
        isOccupied = true;
        UpdateSprite();
        if (farmCollider != null)
        {
            farmCollider.enabled = false; // 씨앗 심으면 farm 콜라이더 종료
        }
    }

    /// <summary>
    /// 농지와 상호작용 가능한 지 확인하는 부분이다.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool CanInteract(Player player)
    {
        ItemData itemData = InventoryManager.Instance.EquippedItem();

        SeedData seedData = Caster.CastTo<SeedData>(itemData);
            
        // 씨앗 데이터이면 심을 수 있는 지 확인 
        if (seedData != null)
        {
            if (!TryPlantSeedExceptionHandling(seedData)) return false;
            return true;
        }
        
        ToolData toolData = Caster.CastTo<ToolData>(itemData);
        
        // 도구 데이터는 상황에 따라 다름
        switch (toolData.toolType)
        {
            case ToolType.WateringCan:
                // 물 주는 애니메이션이 이 아래에 들어가야함
                // 여기!
                toolData.nowDurability -= toolData.useDurability; // 내구도 감소
                if (!isOccupied)
                {
                    return true;
                }
                Debug.Log($"[player] 이미 화분 젖은 상태");
                return false;
        }

        Debug.Log($"오류 상황");
        return false;
    }
    
    /// <summary>
    /// 작물을 심는 것이 가능한 지 확인하는 코드이다.
    /// </summary>
    /// <param name="equippedSeed"></param>
    /// <returns>작물을 심는 것이 가능한가?</returns>
    public bool TryPlantSeedExceptionHandling(SeedData equippedSeed)
    {
        if (isOccupied)
        {
            Debug.Log($"[farm]이미 씨앗이 심어져 있음. 현재 isOccupied:{isOccupied}");
            return false; // 이미 씨앗이 심어져 있음
        }
        
        if (!isWatered)
        {
            Debug.Log($"[farm] 화분에 물을 줘야지 씨앗을 심을 수 있음");
            return false;
        }

        if (equippedSeed == null)
        {
            Debug.Log($"[{name}] : 씨앗 아이템 미장착");
            return false; // 씨앗 아이템을 장착하고 있지 않음
        }
        
        // 작물 오브젝트 생성
        HarvestableCrop crop = equippedSeed.cropPrefabToGrow.GetComponent<HarvestableCrop>();

        if (crop == null)
        {// 작물의 초기 상태 = 심은 직후 상태
            Debug.Log($"[{name}] : 작물 프리팹 없음");
            return false; // 씨앗 아이템을 장착하고 있지 않음
        }
        

        Debug.Log($"[{name}] : 작물 심기 가능");
        return true;
    }
    // 화분 비우기
    public void ClearFarm()
    {
        if (currentCropInstance != null)
        {
            //currentCropInstance.OnHarvested(); // 작물 수확
            //Destroy(currentCropInstance.gameObject);
            isOccupied = false;
            isWatered = false;
            Debug.Log($"[farm] [ClearFarm] 현재 isOccupied:{isOccupied}");
            Destroy(currentCropInstance.gameObject);
            currentCropInstance = null;
            UpdateSprite();
            if (farmCollider != null)
            {
                farmCollider.enabled = true; // 작물 수확하고 나서 콜라이더 활성화
            }
        }
    }
}
