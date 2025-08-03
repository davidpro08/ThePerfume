using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BenchInventoryUIManager : InventoryUIManager
{
    public static BenchInventoryUIManager Instance { get; private set; }
    [Header("인벤토리 경고창UI")]
    [SerializeField] private GameObject warningCanvas;
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField] private Button warningOkButton;



    [Header("아이템 생성 설정")]
    [SerializeField] private Transform itemSpawnTray; // 아이템이 올라갈 철재 쟁반
    [SerializeField] private List<Transform> transformPos;
    // 트레이 위에 생성된 아이템 리스트
    private List<GameObject> spawnedItemOnTray = new List<GameObject>();

    void Awake()
    {
        Debug.Log($"Awake 실행");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.Log($"[BenchInventoryUIManager] InventoryManager 존재 안 함.");
            enabled = false;
            return;
        }
        Debug.Log($"InventoryManager 참조 성공");

        inventoryManager.onInventoryChangedCallback += UpdateAllUIs;
        Debug.Log($"이벤트 구독 완");

        if (fullInventoryPanel != null) fullInventoryPanel.SetActive(false);
        if (warningCanvas != null) warningCanvas.SetActive(false);
        // if (quantityCanvas != null) quantityCanvas.SetActive(false);

        if (warningOkButton != null)
        {
            warningOkButton.onClick.RemoveAllListeners();
            warningOkButton.onClick.AddListener(OnWarningCanvasOkButton);
        }
    }

    void Start()
    {
        inventoryManager.onInventoryChangedCallback += UpdateAllUIs;
        InitializeHotbar();
        InitializeFullInventory();
        UpdateAllUIs();
    }

    void OnDestroy()
    {
        if (inventoryManager != null) inventoryManager.onInventoryChangedCallback -= UpdateAllUIs;
        if (warningOkButton != null) warningOkButton.onClick.RemoveAllListeners();
    }

    // 경고창 표시
    private void ShowWarningCanvas(string message)
    {
        if (warningCanvas != null)
        {
            warningMessageText.text = message;
            warningCanvas.SetActive(true);
        }
    }
    // 경고창 끄기
    public void OnWarningCanvasOkButton()
    {
        if (warningCanvas != null)
        {
            warningCanvas.SetActive(false);
            //ResetSelection();
        }
    }

    // ============================ 시스템 관련 코드 =====================================
    // tray에 아이템 랜덤 생성
    public void SpawnItemOnTray(ItemData itemToSpawn, int count)
    {
        Debug.Log($"SpawnItemOnTray. 아이템: {itemToSpawn?.name}, 수량: {count}");
        CropData cropData = itemToSpawn as CropData;
        if (itemToSpawn == null || cropData == null || cropData.itemPrefab == null || itemSpawnTray == null)
        {
            if (itemToSpawn == null) Debug.Log("itemToSpawn=null");
            if (cropData == null) Debug.Log($"CropData 형변환 실패");
            if (cropData.itemPrefab == null) Debug.Log($"CropData.itemPrefab==null");
            if (itemSpawnTray == null) Debug.Log($"itemSpawnTray==null");
            return;
        }

        int spawnd = 0;

        foreach (Transform spawnPoint in transformPos)
        {
            if (spawnPoint.childCount == 0)
            {
                GameObject spawndItem = Instantiate(cropData.itemOnTray, spawnPoint.position, Quaternion.identity);
                if (spawndItem == null)
                {
                    Debug.Log($"Instantiate 실패");
                    continue;
                }

                spawndItem.transform.SetParent(spawnPoint);
                spawnedItemOnTray.Add(spawndItem);

                Debug.Log($"{spawndItem.name} {spawnd + 1}개 생성");
                spawnd++;
                InventoryManager.Instance.RemoveItem(itemToSpawn, 1);

                if (spawnd >= count) break;
            }
        }
        if (spawnd < count)
        {
            Debug.Log("트레이 빈칸 부족");
        }
    }

    // Tray 아이템 삭제
    public void RemoveSpawnedItemd(GameObject itemToRemove)
    {
        if (spawnedItemOnTray.Contains(itemToRemove))
        {
            spawnedItemOnTray.Remove(itemToRemove);
            Destroy(itemToRemove);
        }
    }

    // 아이템 존재 여부 확인
    public bool HasSpawnedItemOnTray()
    {
        // 리스트가 비어있으면 false
        return spawnedItemOnTray != null && spawnedItemOnTray.Count > 0;
    }
}
