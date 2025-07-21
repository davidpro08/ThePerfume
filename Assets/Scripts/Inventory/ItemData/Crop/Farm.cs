using UnityEngine;

public class Farm : MonoBehaviour
{
    [Header("화분 상태")]
    public bool isOccupied = false; //이미 씨앗이 심어져 있는지
    public HarvestableCrop currentCropInstance; // 현재 심어진 작문의 인스턴스
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

    public bool PlantSeed(SeedData seedData)
    {
        Debug.Log($"[farm] [PlantSeed] 현재 isOccupied:{isOccupied}");
        Debug.Log($"[farm] [Watering] 현재 isWatered:{isWatered}");
        if (isOccupied)
        {
            Debug.Log($"[farm]이미 씨앗이 심어져 있음. 현재 isOccupied:{isOccupied}");
            return false; // 이미 씨앗이 심어져 있음
        }
        if (seedData.cropPrefabToGrow == null)
        {
            Debug.Log($"[farm]작물 프리팹 설정 안됨");
            return false; // 자라날 작물 프리팹 설정 안됨
        }
        if (!isWatered)
        {
            Debug.Log($"[farm] 화분에 물을 줘야지 씨앗을 심을 수 있음");
            return false;
        }

        // 작물 오브젝트 생성
        GameObject cropGO = Instantiate(seedData.cropPrefabToGrow, transform.position, Quaternion.identity, transform);
        currentCropInstance = cropGO.GetComponent<HarvestableCrop>();

        if (currentCropInstance != null)
        {// 작물의 초기 상태 = 심은 직후 상태
            currentCropInstance.currentStage = 0;
            isOccupied = true;
            UpdateSprite();
            if (farmCollider != null)
            {
                farmCollider.enabled = false; // 씨앗 심으면 farm 콜라이더 종료
            }
            Debug.Log($"[farm]{seedData.itemName}씨앗 심어짐");
        }
        else
        {
            Debug.Log($"[farm]작물 프리팹 없음");
            Destroy(cropGO);
            return false; // 작물 프리팹이 없음
        }
        return true;
    }

    public bool canPlantSeed()
    {
        return !isOccupied;
    }

    public bool CanWatered()
    {
        return !isWatered;
    }

    // 물 주기
    public void Watering()
    {
        if (isWatered)
        {
            Debug.Log($"[farm] 이미 젖은 화분");
            return;
        }
        isWatered = true;
        UpdateSprite();
        Debug.Log($"[farm] 화분에 물 줌");
    }
    // 화분 비우기
    public void ClearFarm()
    {
        if (currentCropInstance != null)
        {
            currentCropInstance.OnHarvested(); // 작물 수확
            Destroy(currentCropInstance.gameObject);
            isOccupied = false;
            isWatered = false;
            Debug.Log($"[farm] [ClearFarm] 현재 isOccupied:{isOccupied}");
            currentCropInstance = null;
            UpdateSprite();
            if (farmCollider != null)
            {
                farmCollider.enabled = true; // 작물 수확하고 나서 콜라이더 활성화
            }
        }
    }

    private void UpdateSprite()
    {
        if (farmSpriteRenderer != null)
        {
            farmSpriteRenderer.enabled = true;
            farmSpriteRenderer.sprite = isWatered ? wateredFarmSprite : emptyFarmSprite;
        }
    }
}
