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

    [Header("경고창UI")]
    [SerializeField] private GameObject warningCanvas;
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField] private Button warningOkButton;

    [Header("수량 조절창UI")]
    [SerializeField] private GameObject quantityCanvas;
    [SerializeField] private Image quantityItemImage;
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private TextMeshProUGUI ItemQuantityText;
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private Button plusButtion;
    [SerializeField] private Button minusButtion;
    [SerializeField] private Button quantityOkButton;
    [SerializeField] private Button quantityNoButton;

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
            enabled = false;
            return;
        }
        Debug.Log($"InventoryManager 참조 성공");

        inventoryManager.onInventoryChangedCallback += UpdateAllUIs;
        Debug.Log($"이벤트 구독 완");

        if (benchInventoryPanel != null)
        {
            benchInventoryPanel.SetActive(true);
            Debug.Log($"benchInventoryPanel 활성화 설정 완");
        }

        if (warningCanvas != null) warningCanvas.SetActive(false);
        if (quantityCanvas != null) quantityCanvas.SetActive(false);

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
            InventorySlotUI inventorySlot = slotGO.GetComponent<InventorySlotUI>();

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
}
