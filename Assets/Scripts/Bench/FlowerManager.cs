using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

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

    [Header("Bowl 설정")]
    [SerializeField] private Transform bowlTransform;
    [SerializeField] private SpriteRenderer bowlSpriteRenderer;
    [SerializeField] private float bowlPetalSpawnRadius = 0.5f;
    [SerializeField] private float petalStackZOffsest = 0.01f;

    private GameObject currentMainFlower;
    private List<FlowerPetalUI> allPetalFlower = new List<FlowerPetalUI>();
    private int totalPetalInBowl = 0;
    private int collectedPetalCount = 0;
    private CropData currentCropItemData;
    public bool isHandling = false;
    public GameObject _TrayClick;
    public bool blockingCanvasOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        isHandling = false;
    }

    // 손질 시작 함수 (tray에서 작물 클릭 시 TrayClick에서 호출)
    public void StartHandling(ItemData cropItemData, ItemOnTrayClick clickedTrayItem = null)
    {
        Debug.Log("start StartHandling()");
        // ========================================================
        // Blocking Canvas 위에 손질 가능 작물 중복 방지
        // 캔버스 위에서 작물 프리팹이 계속 소환되는 오류 수정..? 됐나?
        if (isHandling && clickedTrayItem == null) return;
        isHandling = true;
        // ========================================================
        Debug.Log("doing StartHandling()");


        ItemOnTrayClick itemOnTrayClick = clickedTrayItem;
        if (itemOnTrayClick != null)
        {
            Debug.Log($"콜라이더 비활성화:{itemOnTrayClick.gameObject.name}");
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        if (blockingCanvas != null)
        {
            blockingCanvas.SetActive(true);
            blockingCanvasOpen = true;
        }

        this.currentCropItemData = cropItemData as CropData;

        currentMainFlower = Instantiate(currentCropItemData.itemPrefabOnUI, mainFlowerSpawnPoint);
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
    }

    // 손질 종료 후
    // FlowPetalUI 스크립트에서 호출 + 꽃잎 뜯길 때마다 bowl에 갯수 추가
    public void AddPetalToBowl(FlowerPetalUI petal)
    {
        Debug.Log("FlowerManager 입장~");
        collectedPetalCount++;
        totalPetalInBowl++;

        // 여기에 애니메이션 넣어도 되고, UpdateBowlSprite에 애니메이션 넣어도 되고
        UpdateBowlSprite();


        Debug.Log($"collectedPetalCount:{collectedPetalCount}, allPetalFlower.Count:{allPetalFlower.Count}");
        if (collectedPetalCount >= allPetalFlower.Count)
        {
            Debug.Log($"모든 꽃잎 뜯기 완");
            if (blockingCanvas != null)
            {
                blockingCanvas.SetActive(false);
                blockingCanvasOpen = false;
            }
            if (currentMainFlower != null)
            {
                Destroy(currentMainFlower);
                currentMainFlower = null;
                allPetalFlower.Clear();
            }

            // ===============================

            isHandling = false;
            // ===============================
        }
    }

    public void OnBowlClicked()
    {
        Debug.Log("OnBowlClicked called");
        // tray 위에 프리팹이 남았는지 확인
        // ============================================================
        if (totalPetalInBowl == 0)
        {
            return;
        }
        // 근데 생각해보니깐 tray랑 bowl 위에 아무것도 없어야지 나갈 수 있도록 하면
        // 굳이 이걸 확인할 필요가 있나
        // ============================================================

        // 인벤토리 추가 + 제거 (테스트용 수정 필요)
        // 꽃잎 아이템 연결을 위해서 cropItem에 추가를 하든지 해야함
        // =============================================================
        if (InventoryManager.Instance != null)
        {
            // Clone으로 뽑아냈던 인벤토리 내의 아이템 삭제
            ItemData harvestedPetalItemData = null;
            if (currentCropItemData != null && currentCropItemData.petal != null)
            {
                harvestedPetalItemData = currentCropItemData.petal;
            }

            // 새로운 아이템 추가
            if (harvestedPetalItemData != null)
            {
                InventoryManager.Instance.AddItem(harvestedPetalItemData, totalPetalInBowl);
                totalPetalInBowl = 0;
                if (bowlTransform != null)
                {
                    foreach (Transform child in bowlTransform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            else
            {
                Debug.LogError("꽃잎 아이템 할당 안됨");
            }
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
        Sprite petalSpriteToUse = null;

        if (currentCropItemData != null && currentCropItemData.petal != null && currentCropItemData.petal.itemIcon != null)
        {
            petalSpriteToUse = currentCropItemData.petal.itemIcon;
        }
        else
        {
            Debug.LogWarning("CropData or Petal == null");
            return;
        }


        if (petalSpriteToUse != null && bowlTransform != null)
        {
            GameObject newSmallPetal = new GameObject($"SmallPeata_{totalPetalInBowl}");

            SpriteRenderer sr = newSmallPetal.AddComponent<SpriteRenderer>();
            sr.sprite = petalSpriteToUse;
            sr.sortingLayerName = bowlSpriteRenderer.sortingLayerName;
            sr.sortingOrder = bowlSpriteRenderer.sortingOrder + totalPetalInBowl;

            Vector3 bowlCneter = bowlTransform.position;
            Vector2 randomOffset = Random.insideUnitCircle * bowlPetalSpawnRadius;
            Vector3 spawnPos = new Vector3(bowlCneter.x + randomOffset.x, bowlCneter.y + randomOffset.y, bowlCneter.z + (collectedPetalCount * petalStackZOffsest));
            newSmallPetal.transform.position = spawnPos;
            newSmallPetal.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            newSmallPetal.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f);
            newSmallPetal.transform.SetParent(bowlTransform);
        }
        else
        {
            Debug.LogWarning("smallPetalSprite or bowlTranform == null");
        }
    }

    public bool IsExitable()
    {
        return totalPetalInBowl <= 0;
    }
}
