using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BenchInventoryUIManager : MonoBehaviour
{
    public static BenchInventoryUIManager Instance { get; private set; }
    [Header("필수연결")]
    [SerializeField] private InventoryManager inventoryManager; // 데이터 소스
    [SerializeField] private GameObject inventorySlotUIPrefab;
    [Header("인벤토리 설정")]
    [SerializeField] private GameObject benchInventoryPanel; // 인벤토리 패널
    [SerializeField] private Transform benchSlotContainer; // 인벤토리 슬롯들의 부모
    [SerializeField] private Button cancleButton; // 취소 버튼

    [Header("인벤토리 경고창UI")]
    [SerializeField] private GameObject warningCanvas;
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField] private Button warningOkButton;

    [Header("인벤토리 수량 조절창UI")]
    [SerializeField] private GameObject quantityCanvas;
    [SerializeField] private Image quantityItemImage;
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private TextMeshProUGUI ItemQuantityText;
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private Button plusButtion;
    [SerializeField] private Button minusButtion;
    [SerializeField] private Button quantityOkButton;
    [SerializeField] private Button quantityNoButton;

    [Header("아이템 생성 설정")]
    [SerializeField] private Transform itemSpawnTray; // 아이템이 올라갈 철재 쟁반
    // 쟁반 위에 올려놓을 범위인데 임시로 넣어버렸습니다. 스프라이트 오면 수정 필요합니다~
    [SerializeField] private float spawnRadius = 1.5f;
    // 트레이 위에 생성된 아이템 리스트
    private List<GameObject> spawnedItemOnTray = new List<GameObject>();

    private List<InventorySlotUI> allSlotUI = new List<InventorySlotUI>();
    private int currentlySelectedIndex = -1; // 선택된 슬롯 없음
    private ItemData selectedItemData; // 현재 선택된 아이템 데이터
    private int selectedItemQuantity; // 현재 선택된 아이템 전체 수량 (슬라이더의 최대값이 될 것임)


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

        if (benchInventoryPanel != null) benchInventoryPanel.SetActive(false);
        if (warningCanvas != null) warningCanvas.SetActive(false);
        if (quantityCanvas != null) quantityCanvas.SetActive(false);

        if (cancleButton != null)
        {
            cancleButton.onClick.RemoveAllListeners();
            cancleButton.onClick.AddListener(OnInventoryPannelCancleButton);
        }
        if (warningOkButton != null)
        {
            warningOkButton.onClick.RemoveAllListeners();
            warningOkButton.onClick.AddListener(OnWarningCanvasOkButton);
        }
        if (quantityOkButton != null)
        {
            quantityOkButton.onClick.RemoveAllListeners();
            quantityOkButton.onClick.AddListener(OnQuantityCanvasOKButton);
        }
        if (quantityNoButton != null)
        {
            quantityNoButton.onClick.RemoveAllListeners();
            quantityNoButton.onClick.AddListener(OnQuantityCanvasNOButton);
        }
    }

    void Start()
    {
        Debug.Log($"start 호출");
        IntializeAllInventorySlots();
        Debug.Log($"IntializeAllIventorySlot 완료");
        UpdateAllUIs();
        Debug.Log($"UpdateAllUIs 완료");

        ResetSelection();
    }

    void OnDestroy()
    {
        if (inventoryManager != null) inventoryManager.onInventoryChangedCallback -= UpdateAllUIs;
        if (warningOkButton != null) quantityOkButton.onClick.RemoveAllListeners();
        if (quantityOkButton != null) quantityOkButton.onClick.RemoveAllListeners();
        if (quantityNoButton != null) quantityNoButton.onClick.RemoveAllListeners();
        if (cancleButton != null) cancleButton.onClick.RemoveAllListeners();
    }

    private void IntializeAllInventorySlots()
    {
        Debug.Log($"IntializeAllInventorySlots 시작");
        // 기본 Slot 초기화
        foreach (Transform child in benchSlotContainer)
        {
            Destroy(child.gameObject);
        }
        allSlotUI.Clear();

        for (int i = 0; i < inventoryManager.capacity; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotUIPrefab, benchSlotContainer);
            BenchInventorySlotUI inventorySlot = slotGO.GetComponent<BenchInventorySlotUI>();

            if (inventorySlot != null)
            {
                inventorySlot.slotIndex = i;
                inventorySlot.SetBenchManager(this);
                allSlotUI.Add(inventorySlot);
            }
        }
        Debug.Log($"총 {allSlotUI.Count}개의 슬롯 생성 완");
    }

    private void UpdateAllUIs()
    {
        for (int i = 0; i < allSlotUI.Count; i++)
        {
            if (i < inventoryManager.itemSlots.Count)
            {
                allSlotUI[i].UpdateSlotUI(inventoryManager.itemSlots[i]);
            }
            else
            {
                allSlotUI[i].UpdateSlotUI(new ItemSlot());
            }
        }
    }

    public void ResetSelection()
    {
        if (currentlySelectedIndex != -1)
        {
            allSlotUI[currentlySelectedIndex].SetSelected(false);
        }
        currentlySelectedIndex = -1;
        Debug.Log($"슬롯 초기화 완료");
    }

    public void OpenInventory()
    {
        if (benchInventoryPanel == null)
        {
            Debug.Log("benchInventoryPanel 연결 안됨");
        }
        benchInventoryPanel.SetActive(true);
        UpdateAllUIs();
    }

    public void OnSlotClicked(int index)
    {
        Debug.Log($"{index} 슬롯 선택됨");

        // 이전 슬롯 해제, 새 슬롯 선택
        if (currentlySelectedIndex != -1 && currentlySelectedIndex < allSlotUI.Count)
        {
            allSlotUI[currentlySelectedIndex].SetSelected(false);
        }
        currentlySelectedIndex = index;
        allSlotUI[currentlySelectedIndex].SetSelected(true);

        // 아이템 존재 여부
        if (index >= 0 && index < inventoryManager.itemSlots.Count)
        {
            ItemSlot clickedSlot = inventoryManager.itemSlots[index];
            if (clickedSlot.itemData != null)
            {
                selectedItemData = clickedSlot.itemData;
                selectedItemQuantity = clickedSlot.quantity;

                // 아이템 확인 후 canvas 출력
                if (selectedItemData.itemType == ItemType.Crop)
                {
                    ShowQuantitySelectionCanvas();
                }
                else
                {
                    ShowWarningCanvas("작물 아이템만 선택 가능합니다.");
                }
            }
            else
            {
                ResetSelection();
            }
        }
        else
        {
            ResetSelection();
        }
    }

    // 인벤토리 창 닫기
    private void OnInventoryPannelCancleButton()
    {
        CloseAllUI(true);
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
            ResetSelection();
        }
    }

    public void ShowQuantitySelectionCanvas()
    {
        if (quantityCanvas != null)
        {
            quantityCanvas.SetActive(true);

            if (quantityItemImage != null && selectedItemData != null && selectedItemData.itemIcon != null)
            {
                quantityItemImage.sprite = selectedItemData.itemIcon;
                quantityItemImage.enabled = true;
            }
            else if (quantityItemImage != null)
            {
                quantityItemImage.enabled = false;
            }

            ItemNameText.text = selectedItemData.itemName;
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = selectedItemQuantity;
            quantitySlider.value = 1;

            // 이벤트 리스너 추가, 중복 추가 방지
            quantitySlider.onValueChanged.RemoveAllListeners();
            quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);

            // 버튼 리스너 추가
            plusButtion.onClick.RemoveAllListeners();
            plusButtion.onClick.AddListener(OnQuantityPlus);
            minusButtion.onClick.RemoveAllListeners();
            minusButtion.onClick.AddListener(OnQuantityMinus);

            UpdateQuantityText((int)quantitySlider.value);
        }
    }

    // 슬라이더 값 변경 시 호출
    public void OnQuantitySliderChanged(float value)
    {
        UpdateQuantityText((int)value);
    }

    // 수량 + 버튼
    public void OnQuantityPlus()
    {
        if (quantitySlider.value < quantitySlider.maxValue)
        {
            quantitySlider.value++;
        }
    }

    // 수량 - 버튼
    public void OnQuantityMinus()
    {
        if (quantitySlider.value > quantitySlider.minValue)
        {
            quantitySlider.value--;
        }
    }

    // 수량 텍스트 업데이트
    private void UpdateQuantityText(int currentQuantity)
    {
        ItemQuantityText.text = $"{currentQuantity}/{selectedItemQuantity}";
    }

    // 수량 조절창 확인 버튼
    public void OnQuantityCanvasOKButton()
    {
        int chosenQuantity = (int)quantitySlider.value;
        SpawnItemOnTray(selectedItemData, chosenQuantity);
        CloseAllUI(true);
    }

    // 수량 조절창 취소 버튼
    public void OnQuantityCanvasNOButton()
    {
        CloseAllUI(false);
    }

    public void CloseAllUI(bool mainInventoryClose)
    {
        if (warningCanvas != null) warningCanvas.SetActive(false);
        if (quantityCanvas != null) quantityCanvas.SetActive(false);

        if (mainInventoryClose && benchInventoryPanel != null) benchInventoryPanel.SetActive(false);

        ResetSelection();
    }

    // ============================ 시스템 관련 코드 =====================================
    // tray에 아이템 랜덤 생성
    private void SpawnItemOnTray(ItemData itemToSpawn, int count)
    {
        Debug.Log($"SpawnItemOnTray. 아이템: {itemToSpawn?.name}, 수량: {count}");
        CropData cropData = itemToSpawn as CropData;
        if (itemToSpawn == null || cropData.itemPrefab == null || itemSpawnTray == null)
        {
            if (itemToSpawn == null) Debug.Log("itemToSpawn=null");
            if (cropData == null) Debug.Log($"CropData 형변환 실패");
            if (cropData != null && cropData.itemPrefab == null) Debug.Log($"CropData.itemPrefab==null");
            if (itemSpawnTray == null) Debug.Log($"itemSpawnTray==null");
            return;
        }
        Vector3 trayCenter = itemSpawnTray.position;

        for (int i = 0; i < count; i++)
        {
            Debug.Log($"아이템 {i + 1}개 생성 중");
            Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPostion = trayCenter + new Vector3(randomPos.x, randomPos.y, 0);
            GameObject spawndItem = Instantiate(cropData.itemPrefab, spawnPostion, Quaternion.identity);

            if (spawndItem == null)
            {
                Debug.Log($"Instantiate 실패");
                continue;
            }

            spawndItem.transform.SetParent(itemSpawnTray);
            spawnedItemOnTray.Add(spawndItem);
            Debug.Log($"{spawndItem.name} 생성 및 리스트 추가 끝");
        }
        Debug.Log($"총 {spawnedItemOnTray.Count}개 아이템이 트레이 위에 올라감");
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
