using UnityEngine;
using System.Collections.Generic;

public class FlowerManager : MonoBehaviour
{
    // 꽃 프리팹 소환/초기화 (꽃잎 뜯기용 프리팹) >> RoseFlowerPrefab

    // Bowl 관리

    // 경고창 관리 > 비어있는/손질 다 안한 상태로 bowl 클릭 시 경고창

    // 모든 꽃잎이 뜯겠는지 확인

    // 수확 완료 처리 > bowl 클릭하면 아이템 인벤토리 추가 및 Blocking Canvas를 비활성

    public static FlowerManager Instance { get; private set; }

    [SerializeField] private GameObject blockingCanvas;
    [SerializeField] private Transform mainFlowerSpawnPoint;
    [SerializeField] private SpriteRenderer bowlSpriteRenderer;

    // bowl이 차는 단계가 어떻게 표현될지 모르겠었어 그냥 작물성장단계처럼 만들어놨습니다
    [SerializeField] private Sprite[] bowlFillSprites; // Bowl이 차는 단계

    private GameObject currentMainFlower;
    private List<FlowerPetal> allPetalFlower = new List<FlowerPetal>();
    private int collectedPetalCount = 0;
    private bool isHandling = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // 손질 시작 함수 (tray에서 작물 클릭 시 TrayClick에서 호출)
    public void StartHandling(ItemData cropItemData)
    {
        // ========================================================
        // Blocking Canvas 위에 손질 가능 작물 중복 방지
        // 캔버스 위에서 작물 프리팹이 계속 소환되는 오류 수정..? 됐나?
        if (isHandling) return;
        isHandling = true;
        // ========================================================

        if (blockingCanvas != null) blockingCanvas.SetActive(true);

        CropData cropData = cropItemData as CropData;
        if (cropData == null || cropData.itemPrefab == null) return;

        currentMainFlower = Instantiate(cropData.itemPrefab, mainFlowerSpawnPoint.position, Quaternion.identity);

        allPetalFlower.Clear();
        allPetalFlower.AddRange(currentMainFlower.GetComponentsInChildren<FlowerPetal>());
        collectedPetalCount = 0;
        UpdateBowlSprite(); // Bowl 초기화

        // bowl 클릭 감지 + 스크립트 활성화
    }

    public void EndHandling()
    {
        if (blockingCanvas != null) blockingCanvas.SetActive(false);
        if (currentMainFlower != null) Destroy(currentMainFlower);
        currentMainFlower = null;
        allPetalFlower.Clear();

        // =============================================================
        //BenchInventoryUIManager.Instance.RemoveSpawnedItemd(꽃작물아이템, 갯수);
        // =============================================================

        // ========================================================
        // StartHandling에 오류 수정 설명 연장선
        isHandling = false;
        // ========================================================
    }

    public void AddPetalToBowl(FlowerPetal petal)
    {
        collectedPetalCount++;
        UpdateBowlSprite();

        if (collectedPetalCount >= allPetalFlower.Count)
        {
            Debug.Log($"모든 꽃잎 뜯기 완");
            // bowl 클릭
        }
    }

    public void OnBowlClicked()
    {
        if (collectedPetalCount == 0)
        {
            Debug.Log($"bowl 비어있음");
            return;
        }

        if (collectedPetalCount < allPetalFlower.Count)
        {
            Debug.Log($"손질 덜 되었음");
            return;
        }

        // 인벤토리 추가
        // =============================================================
        //InventoryManager.Instance.AddItem(꽃잎 아이템, 갯수);
        // =============================================================
        EndHandling();
    }
    public void UpdateBowlSprite()
    {
        if (bowlSpriteRenderer == null || bowlFillSprites == null || bowlFillSprites.Length == 0) return;

        float fillRatio = (float)collectedPetalCount / allPetalFlower.Count;
        int spriteIndex = Mathf.FloorToInt(fillRatio * (bowlFillSprites.Length - 1));
        bowlSpriteRenderer.sprite = bowlFillSprites[spriteIndex];
    }
}
