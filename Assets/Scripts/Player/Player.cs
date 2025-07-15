using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("기본 설정")]
    public float moveSpeed = 5f;

    [Header("인벤토리 설정")]
    private bool isOpenInventory = false;
    [SerializeField] public InventoryUIManager inventoryUIManager; // 연결할 인벤토리 UI 관리자
    [SerializeField] private InventoryManager inventoryManager;

    [Header("상호작용 설정")]
    [SerializeField] private float InteractionRange = 1f;
    [SerializeField] private LayerMask interactableLayer; // 상호작용할 아이템 레이어
    [SerializeField] private Transform interactionPoint; // 상호작용 지점

    [Header("일시정지 설정")] private bool isPaused = false;

    private Vector2 moveInput;
    private bool isSprint;
    private float runRate = 1.8f; // 걷는 속력과 비교한 달리기 속력비
    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (inventoryManager == null)
        {
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        }
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetBool("isWalking", moveInput.magnitude > 0.01f);

        // 확인 코드가 너무 길어지는데
        if (moveInput.magnitude > 0.01f)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }

    void OnSprint(InputValue value)
    {
        isSprint = value.Get<float>() > 0.5f;
        Debug.Log("Sprint state: " + isSprint);
    }

    // 인벤토리 열기
    void OnInteract(InputValue value)
    {
        // 멈춰 있으면 작동 안해야 함
        if (isPaused) return;

        isOpenInventory = !isOpenInventory;

        if (inventoryUIManager != null)
        {
            inventoryUIManager.ToggleFullInventory();
        }
        else
        {
            Debug.LogWarning("InventoryUIManager가 Player 스크립트에 연결되지 않았습니다.");
        }
    }

    // 아이템/NPC 등 상호작용
    // PlayerInput으로 입력 받을 수 있도록 수정중 >> 기획서에 맞춰서 키보드/마우스 상호작용으로 나눔
    void OnPickUp(InputValue value)
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            MouseInteration();
        }
        else if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            KeyboardInteration();
        }
    }

    // 마우스로 상호작용
    private void MouseInteration()
    {
        // 마우스 위치 감지 + 오브젝트 감지
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D detection = Physics2D.Raycast(mousePos, Vector2.zero, 0f, interactableLayer);

        if (detection.collider != null)
        {
            PickupItems pickupItems = detection.collider.GetComponent<PickupItems>();
            // 인벤토리에 추가되는 아이템 획득 (임시)
            if (pickupItems != null)
            {
                if (Vector2.Distance(transform.position, detection.collider.transform.position) <= InteractionRange)
                {
                    Debug.Log($"[player]아이템 발견");
                    PerformPickup(pickupItems);
                    return;
                }
                else
                {
                    Debug.Log($"[player]아이템이 너무 멀리 있음");
                    return;
                }
            }

            // Farm과 상호 작용
            Farm farm = detection.collider.GetComponent<Farm>();
            if (farm != null)
            {
                TryPlantSeed(farm); // 씨앗 심기
                return;
            }
            else
            {
                HarvestableCrop crop = detection.collider.GetComponent<HarvestableCrop>();
                if (crop != null)
                {
                    TryHarvestCrop(crop); // 농작물 수확
                    return;
                }

                else
                {
                    // 그 외 상호작용
                }
            }

        }
        Debug.Log($"상호작용 없음");
    }

    //키보드로 상호작용
    private void KeyboardInteration()
    {
        Vector2 origin = interactionPoint != null ? interactionPoint.position : transform.position;

        // 주변 상호작용 가능 오브젝트 존재 여부 판단
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, InteractionRange, interactableLayer);

        foreach (Collider2D hitCollider in hitColliders)
        {
            PickupItems pickupItems = hitCollider.GetComponent<PickupItems>();
            // 인벤토리에 추가되는 아이템 획득 (임시)
            if (pickupItems != null)
            {
                Debug.Log($"[player]아이템 발견");
                PerformPickup(pickupItems);
                return;
            }

            // farm 상호작용
            Farm farm = hitCollider.GetComponent<Farm>();
            if (farm != null)
            {
                TryPlantSeed(farm); // 작물 심기
                return;
            }
            else
            {
                HarvestableCrop crop = hitCollider.GetComponent<HarvestableCrop>();
                if (crop != null)
                {
                    TryHarvestCrop(crop);
                    return;
                }
                else
                {
                    // 그 외 상호작용
                }
            }
        }
        Debug.Log($"상호작용 없음");
    }

    // ESC 누르면 멈추도록 하기
    void OnPause(InputValue value)
    {
        isPaused = !isPaused;

        // 시간이 흐른다라고 해서.. 그러면 이렇게 구현하면 안 될 것 같긴 한데
        if (isPaused)
        {
            Debug.Log("일시 정지");
            Time.timeScale = 0f; // 게임 정지
            Time.fixedDeltaTime = 0f; // 물리 정지
        }
        else
        {
            Debug.Log("일시 정지 해제");
            Time.timeScale = 1f; // 재개
            Time.fixedDeltaTime = 0.02f;
        }
    }

    void FixedUpdate()
    {
        // 멈춰 있으면 작동 안해야 함
        // 추가로 인벤토리 열려도 못 움직이도록 하기
        if (isPaused || isOpenInventory)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
        else
        {
            Vector2 movement = isSprint ? moveInput * (moveSpeed * runRate) : moveInput * moveSpeed;
            rb.linearVelocity = movement;
        }
    }

    private void PerformPickup(PickupItems pickupItems)
    {
        if (inventoryUIManager == null) return;
        if (pickupItems.itemToGive == null) return;

        bool added = inventoryManager.AddItem(pickupItems.itemToGive, pickupItems.quantityToGive);
    }

    private void TryPlantSeed(Farm farm)
    {
        // 농지가 비어있는지 페크
        if (!farm.isOccupied)
        {
            Debug.Log($"[player]이미 작물이 심어져 있음");
            return; // 이미 심어져있음
        }

        // 현재 플레이어가 씨앗 아이템을 들고 있는지
        SeedData equippedSeed = EquippedSeed();
        if (equippedSeed == null)
        {
            Debug.Log($"[player]씨앗 아이템 미장착");
            return; // 씨앗 아이템을 장착하고 있지 않음
        }

        // 씨앗 심기
        farm.PlantSeed(equippedSeed);

        // 인벤토리에서 아이템 1개 감소
        inventoryManager.RemoveItem(equippedSeed, 1);
    }

    // 현재 플레이어가 씨앗 아이템을 들고 있는지
    private SeedData EquippedSeed()
    {
        if (inventoryManager == null || inventoryManager.SelectedSlotIndex == -1)
        {
            Debug.Log($"[player]인벤코리 메니저가 없거나 선택된 슬롯이 없음");
            return null; // 인벤토리 매니저가 없거나 선택된 슬롯이 없음
        }
        // 현재 선택된 슬롯의 아이템 가져오기
        ItemSlot selectedSlot = inventoryManager.itemSlots[inventoryManager.SelectedSlotIndex];

        if (selectedSlot.itemData != null && selectedSlot.itemData.itemType == ItemType.Seed)
        {
            Debug.Log($"[player]씨앗 아이템 들고 있음");
            return selectedSlot.itemData as SeedData; // 씨앗 아이템 들고 있음
        }
        return null; // 씨앗 아이템 안 들고 있음
    }

    // 농작물 수확
    private void TryHarvestCrop(HarvestableCrop crop)
    {
        // 작물이 완전히 자랐는지 체크
        if (!crop.isFullGrowth)
        {
            return; // 아직 다 안 자라남
        }

        // 올바른 도구를 장착했는지 확인 >> 도구가 무슨 종류인지 확인 필요
        ToolData equippedTool = EquippedTool();
        if (equippedTool == null)
        {
            return; // 도구 장착하지 않음
        }
        if (equippedTool.toolType != crop.requiredToolType)
        {
            return; // 잘못된 도구 장착
        }

        // 수확
        ItemData harvestedItem = crop.itemType;
        int harvestedQuantity = crop.Quantity;

        if (inventoryManager.AddItem(harvestedItem, harvestedQuantity))
        {
            crop.OnHarvested();
            // 도구 내구도 감소 로직 추가해야함
            // equippedTool.nowDurability -= 10;
            // 이런 식으로 하지 않을까
        }
        else
        {
            //가득 찼음. 수확 불가능?
            return;
        }
    }

    // 현재 플레이어가 도구를 들고 있는지
    private ToolData EquippedTool()
    {
        if (inventoryManager == null || inventoryManager.SelectedSlotIndex == -1)
        {
            return null; // 인벤토리 매니저가 없거나 선택된 슬롯이 없음
        }
        // 현재 선택된 슬롯의 아이템 가져오기
        ItemSlot selectedSlot = inventoryManager.itemSlots[inventoryManager.SelectedSlotIndex];

        if (selectedSlot.itemData != null && selectedSlot.itemData.itemType == ItemType.Tool)
        {
            return selectedSlot.itemData as ToolData; // 도규 아이템 들고 있음
        }
        return null; // 도구앗 아이템 안 들고 있음
    }
}