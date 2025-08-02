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
    private List<FlowerPetalUI> allPetalFlower = new List<FlowerPetalUI>();
    private int collectedPetalCount = 0;
    private bool isHandling = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // 손질 시작 함수 (tray에서 작물 클릭 시 TrayClick에서 호출)
    public void StartHandling(ItemData cropItemData, GameObject clickedTrayItem = null)
    {
        // ========================================================
        // Blocking Canvas 위에 손질 가능 작물 중복 방지
        // 캔버스 위에서 작물 프리팹이 계속 소환되는 오류 수정..? 됐나?
        if (isHandling) return;
        isHandling = true;
        // ========================================================

        if (blockingCanvas != null) blockingCanvas.SetActive(true);

        CropData cropData = cropItemData as CropData;
        if (cropData == null || cropData.itemPrefabOnUI == null) return;

        currentMainFlower = Instantiate(cropData.itemPrefabOnUI, mainFlowerSpawnPoint);
        currentMainFlower.transform.localPosition = Vector3.zero;

        RectTransform centerRect = currentMainFlower.transform.Find("center")?.GetComponent<RectTransform>();
        if (centerRect == null)
        {
            Debug.LogWarning("꽃 중심 오브젝트 없음");
            return;
        }

        allPetalFlower.Clear();
        allPetalFlower.AddRange(currentMainFlower.GetComponentsInChildren<FlowerPetalUI>());

        foreach (FlowerPetalUI petal in allPetalFlower)
        {
            petal.Initialize(this, centerRect);
            petal.gameObject.SetActive(true);
        }

        collectedPetalCount = 0;
        UpdateBowlSprite();
    }

    // 손질 종료 후
    // FlowPetalUI 스크립트에서 호출 + 꽃잎 뜯길 때마다 bowl에 갯수 추가
    public void AddPetalToBowl(FlowerPetalUI petal)
    {
        Debug.Log("FlowerManager 입장~");
        collectedPetalCount++;
        UpdateBowlSprite();

        Debug.Log($"collectedPetalCount:{collectedPetalCount}, allPetalFlower.Count:{allPetalFlower.Count}");
        if (collectedPetalCount >= allPetalFlower.Count)
        {
            Debug.Log($"모든 꽃잎 뜯기 완");
            if (blockingCanvas != null) blockingCanvas.SetActive(false);
            if (currentMainFlower != null)
            {
                Destroy(currentMainFlower);
                currentMainFlower = null;
                allPetalFlower.Clear();
            }

            // ===============================
            // tray 위의 clone 파괴
            //
            // ===============================
        }
    }

    public void OnBowlClicked()
    {
        // tray 위에 프리팹이 남았는지 확인
        // ============================================================
        // 
        // ============================================================

        // 인벤토리 추가 + 제거 (테스트용 수정 필요)
        // 꽃잎 아이템 연결을 위해서 cropItem에 추가를 하든지 해야함
        // =============================================================
        if (InventoryManager.Instance != null)
        {
            // Clone으로 뽑아냈던 인벤토리 내의 아이템 삭제
            // BenchInventoryUIManager.Instance.RemoveSpawnedItemd(꽃작물 아이템, 갯수);
            //
            // 새로운 아이템 추가
            // if (꽃잎 아이템 != null){
            //     InventoryManager.Instance.AddItem(꽃잎 아이템, collectedPetalCount);
            // }
            // else {
            //     Debug.LogError("꽃잎 아이템 할당 안됨");
            // }
        }
        else
        {
            Debug.LogError("InventoryManager.Instance==null");
        }
        // =============================================================

        // ========================================================
        // StartHandling에 오류 수정 설명 연장선
        isHandling = false;
        // ========================================================        
    }
    public void UpdateBowlSprite()
    {
        if (bowlSpriteRenderer == null || bowlFillSprites == null || bowlFillSprites.Length == 0) return;

        float fillRatio = (float)collectedPetalCount / allPetalFlower.Count;
        int spriteIndex = Mathf.FloorToInt(fillRatio * (bowlFillSprites.Length - 1));
        bowlSpriteRenderer.sprite = bowlFillSprites[spriteIndex];
    }
}
