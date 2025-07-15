using UnityEngine;

public class Farm : MonoBehaviour
{
    [Header("화분 상태")]
    public bool isOccupied = false; //이미 씨앗이 심어져 있는지
    public HarvestableCrop currentCropInstance; // 현재 심어진 작문의 인스턴스
    private SpriteRenderer farmSpriteRenderer;
    public Sprite emptyFarmSprite; // 비어져있는 화분 스프라이트
    public Sprite plantedFarmSprite; // 심여진 화분 스프라이트 (똑같으려나?)

    void Awake()
    {
        farmSpriteRenderer = GetComponent<SpriteRenderer>(); // 초기화
        isOccupied = false;
    }

    public void PlantSeed(SeedData seedData)
    {
        if (isOccupied)
        {
            Debug.Log($"[farm]이미 씨앗이 심어져 있음");
            return; // 이미 씨앗이 심어져 있음
        }
        if (seedData.cropPrefabToGrow == null)
        {
            Debug.Log($"[farm]작물 프리팹 설정 안됨");
            return; // 자라날 작물 프리팹 설정 안됨
        }

        GameObject cropGO = Instantiate(seedData.cropPrefabToGrow, transform.position, Quaternion.identity, transform);
        currentCropInstance = cropGO.GetComponent<HarvestableCrop>();

        if (currentCropInstance != null)
        {// 작물의 초기 상태 = 심은 직후 상태
            currentCropInstance.currentStage = 0;
            Debug.Log($"[farm]{seedData.itemName}씨앗 심어짐");
        }
        else
        {
            Debug.Log($"[farm]작물 프리팹 없음");
            return; // 작물 프리팹이 없음
        }
        isOccupied = true;
    }

    // 화분 비우기
    public void ClearFarm()
    {
        if (currentCropInstance != null)
        {
            currentCropInstance.OnHarvested(); // 작물 수확
            Destroy(currentCropInstance.gameObject);
        }
        isOccupied = false;
        currentCropInstance = null;
    }
}
