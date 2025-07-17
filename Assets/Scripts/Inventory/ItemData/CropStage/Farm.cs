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

    void Awake()
    {
        farmSpriteRenderer = GetComponent<SpriteRenderer>(); // 초기화
        Debug.Log($"[farm] 현재 isOcuppied = false");
        farmCollider = GetComponent<Collider2D>();
        isOccupied = false;
    }

    public void PlantSeed(SeedData seedData)
    {
        Debug.Log($"[farm] [PlantSeed] 현재 isOccupied:{isOccupied}");
        if (isOccupied)
        {
            Debug.Log($"[farm]이미 씨앗이 심어져 있음. 현재 isOccupied:{isOccupied}");
            return; // 이미 씨앗이 심어져 있음
        }
        if (seedData.cropPrefabToGrow == null)
        {
            Debug.Log($"[farm]작물 프리팹 설정 안됨");
            return; // 자라날 작물 프리팹 설정 안됨
        }

        // 작물 오브젝트 생성
        GameObject cropGO = Instantiate(seedData.cropPrefabToGrow, transform.position, Quaternion.identity, transform);
        currentCropInstance = cropGO.GetComponent<HarvestableCrop>();

        if (currentCropInstance != null)
        {// 작물의 초기 상태 = 심은 직후 상태
            currentCropInstance.currentStage = 0;
            isOccupied = true;
            if (farmCollider != null)
            {
                farmCollider.enabled = false; // 씨앗 심으면 farm 콜라이더 종료
            }
            Debug.Log($"[farm]{seedData.itemName}씨앗 심어짐");
        }
        else
        {
            Debug.Log($"[farm]작물 프리팹 없음");
            return; // 작물 프리팹이 없음
        }
    }

    public bool canPlantSeed()
    {
        return !isOccupied;
    }

    // 화분 비우기
    public void ClearFarm()
    {
        if (currentCropInstance != null)
        {
            currentCropInstance.OnHarvested(); // 작물 수확
            Destroy(currentCropInstance.gameObject);
            isOccupied = false;
            Debug.Log($"[farm] [ClearFarm] 현재 isOccupied:{isOccupied}");
            currentCropInstance = null;
            if (farmCollider != null)
            {
                farmCollider.enabled = true; // 작물 수확하고 나서 콜라이더 활성화
            }
        }

    }
}
