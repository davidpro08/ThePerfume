using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("기본 설정")] public float moveSpeed = 5f;

    [Header("인벤토리 설정")] private bool isOpenInventory = false;
    [SerializeField] public InventoryUIManager inventoryUIManager; // 연결할 인벤토리 UI 관리자
    [SerializeField] private InventoryManager inventoryManager;

    [Header("상호작용 설정")][SerializeField] private float InteractionRange = 1f;
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

    // 마우스로 상호작용
    private void MouseInteration()
    {
        // 마우스 위치 감지 + 오브젝트 감지
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D detection = Physics2D.Raycast(mousePos, Vector2.zero, 0f, interactableLayer);

        if (detection.collider != null)
        {
            // 우선순위 별로 상호작용 오브젝트 상호작용 (작물 > 농지 > 바닥에 있는 아이템)

            // 올바른 도구를 장착했는지 확인 >> 도구가 무슨 종류인지 확인 필요
            ToolData equippedTool = EquippedTool();

            // 작물
            HarvestableCrop detectedCrop = detection.collider.GetComponent<HarvestableCrop>();
            if (TryHarvestCrop(detectedCrop, equippedTool)) return;

            // 감지된 오브젝트들 저자
            Farm detectedFarm = detection.collider.GetComponent<Farm>();
            if (TryInteractiveFarm(detectedFarm, equippedTool)) return;

            PickupItems detectedPickup = detection.collider.GetComponent<PickupItems>();
            if (TryPickup(detectedPickup)) return;
        }

        Debug.Log($"상호작용 없음");
    }

    //키보드로 상호작용
    private void KeyboardInteration()
    {
        Vector2 origin = interactionPoint != null ? interactionPoint.position : transform.position;

        // 주변 상호작용 가능 오브젝트 존재 여부 판단
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, InteractionRange, interactableLayer);

        // 모든 콜라이더 확인
        foreach (Collider2D hitCollider in hitColliders)
        {
            // 거리가 너무 먼 경우
            if (Vector2.Distance(transform.position, hitCollider.transform.position) > InteractionRange)
            {
                continue;
            }

            // 우선순위 별로 상호작용 오브젝트 상호작용 (작물 > 농지 > 바닥에 있는 아이템)

            // 올바른 도구를 장착했는지 확인 >> 도구가 무슨 종류인지 확인 필요
            ToolData equippedTool = EquippedTool();

            // 작물
            HarvestableCrop detectedCrop = hitCollider.GetComponent<HarvestableCrop>();
            if (TryHarvestCrop(detectedCrop, equippedTool)) return;

            // 농지
            Farm detectedFarm = hitCollider.GetComponent<Farm>();
            if (TryInteractiveFarm(detectedFarm, equippedTool)) return;

            // REVIEW: 이거 왜 또 호출하나요? 
            // if (detectedCrop != null && !detectedCrop.CanHarvest())
            // {
            //     _TryHarvestCrop(detectedCrop);
            //     return;
            // }

            PickupItems detectedPickup = hitCollider.GetComponent<PickupItems>();
            if (TryPickup(detectedPickup)) return;
        }

        Debug.Log($"상호작용 없음");
        return;
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
    /// <summary>
    /// 자원을 줍는 코드이다.
    /// </summary>
    /// <param name="pickupItems">수학할 작물 정보를</param>
    /// <returns>성공 여부</returns>
    private bool TryPickup(PickupItems pickupItems)
    {
        if (pickupItems == null)
        {
            Debug.Log($"[{name}] : 주을 아이템 없음");
            return false;
        }

        if (inventoryUIManager == null)
        {
            Debug.Log($"[{name}] : 인벤토리 매니저 없음");
            return false;
        }

        if (pickupItems.itemToGive == null)
        {
            Debug.Log($"[{name}] : 아이템 정보 없음");
            return false;
        }

        bool added = inventoryManager.AddItem(pickupItems.itemToGive, pickupItems.quantityToGive);
        return added;
    }

    /// <summary>
    /// ?
    /// </summary>
    private void TryWatering()
    {
    }

    /// <summary>
    /// 작물을 심는 코드이다.
    /// </summary>
    /// <param name="farm">작물을 심을 농장이다.</param>
    /// <returns>성공 여부</returns>
    private bool TryPlantSeed(Farm farm)
    {
        // 현재 플레이어가 씨앗 아이템을 들고 있는지
        SeedData equippedSeed = EquippedSeed();

        if (!TryPlantSeedExceptionHandling(farm, equippedSeed)) return false;

        // 씨앗 심기
        farm.PlantSeed(equippedSeed);

        // 인벤토리에서 아이템 1개 감소
        inventoryManager.RemoveItem(equippedSeed, 1);

        return true;
    }

    /// <summary>
    /// 작물을 심는 것이 가능한 지 확인하는 코드이다.
    /// </summary>
    /// <param name="farm"></param>
    /// <param name="equippedSeed"></param>
    /// <returns>작물을 심는 것이 가능한가?</returns>
    public bool TryPlantSeedExceptionHandling(Farm farm, SeedData equippedSeed)
    {
        // 화분이 비어있는지 페크
        if (!farm.canPlantSeed())
        {
            if (!farm.isOccupied)
            {
                Debug.Log($"[{name}] : 이미 작물이 심어져 있음");
                return false;
            }

            if (!farm.isWatered)
            {
                Debug.Log($"[{name}] : 화분에 물 줘야함");
                return false;
            }
        }

        if (equippedSeed == null)
        {
            Debug.Log($"[{name}] : 씨앗 아이템 미장착");
            return false; // 씨앗 아이템을 장착하고 있지 않음
        }

        Debug.Log($"[{name}] : 작물 심기 가능");
        return true;
    }

    /// <summary>
    /// 현재 플레이어가 씨앗을 들고 있는 지 확인하는 코드이다.
    /// </summary>
    /// <returns>씨앗 데이터</returns>
    private SeedData EquippedSeed()
    {
        if (inventoryManager == null)
        {
            Debug.Log($"[{name}] : 인벤토리 매니저 없음");
            return null; // 인벤토리 매니저가 없음
        }

        if (inventoryManager.SelectedSlotIndex == -1)
        {
            Debug.Log($"[{name}] : 선택된 슬롯 없음");
            return null; // 선택된 슬롯이 없음
        }

        // 현재 선택된 슬롯의 아이템 가져오기
        ItemSlot selectedSlot = inventoryManager.itemSlots[inventoryManager.SelectedSlotIndex];

        if (selectedSlot.itemData == null)
        {
            Debug.Log($"[{name}] : 선택된 아이템 없음");
            return null; // 선택된 슬롯이 없음
        }

        if (selectedSlot.itemData.itemType != ItemType.Seed)
        {
            Debug.Log($"[{name}] : 선택된 아이템이 씨앗이 아님");
            return null; // 선택된 아이템이 씨앗이 아님
        }

        return selectedSlot.itemData as SeedData; // 씨앗 아이템 들고 있음
    }

    // 농작물 수확 >> 생각해보니깐 수확하는데 잎 직접 딴다고 했던거 같은데
    /// <summary>
    /// 작물을 수확하는 코드이다.
    /// </summary>
    /// <param name="crop">수학할 작물 정보를</param>
    /// <param name="equippedTool">착용한 장비</param>
    /// <returns>성공 여부</returns>
    private bool TryHarvestCrop(HarvestableCrop crop, ToolData equippedTool)
    {
        if (!_TryHarvestCropExceptionHandling(crop, equippedTool)) return false;

        // 수확
        /*
        ItemData harvestedItem = crop.cropData;
        int harvestedQuantity = crop.Quantity;

        if (inventoryManager.AddItem(harvestedItem, harvestedQuantity))
        {
            crop.OnHarvested();
            equippedTool.nowDurability -= equippedTool.useDurability;
            return true;
        }
        else
        {
            //가득 찼음. 수확 불가능?
            return false;
        }
        */

        crop.OnHarvested();
        return true;
    }

    /// <summary>
    /// 작물 수확에 대한 예외 처리 부분이다.
    /// </summary>
    /// <param name="crop">수확할 작물 정보</param>
    /// <param name="equippedTool">착용한 장비</param>
    /// <returns>작물을 수확 가능 여부</returns>
    private bool _TryHarvestCropExceptionHandling(HarvestableCrop crop, ToolData equippedTool)
    {
        // 농작물이 존재하지 않는다면
        if (ReferenceEquals(crop, null))
        {
            Debug.Log($"[{name}] : 농작물 존재하지 않음");
            return false;
        }

        // 농작물이 수확 가능하지 않다면
        if (!crop.CanHarvest())
        {
            Debug.Log($"[{name}] : 농작물 아직 수확 불가능함");
            return false;
        }

        // 작물이 완전히 자랐는지 체크
        if (!crop.CanHarvest())
        {
            Debug.Log($"[{name}] : [{crop}] 아직 다 안 자라났음. [{crop.currentStage}]");
            return false;
        }

        // 도구를 장착하지 않고 있다면
        if (ReferenceEquals(equippedTool, null))
        {
            Debug.Log($"[{name}] : 도구 장착하지 않음");
            return false;
        }

        // 도구를 잘못 장착했다면
        if (equippedTool.toolType != crop.requiredToolType)
        {
            Debug.Log($"[{name}] : 도구 잘못 장착함. " +
                      $"장착한 도구 : [{equippedTool.toolType}]" +
                      $"필요한 도구 : [{crop.requiredToolType}]");
            return false;
        }

        Debug.Log($"[{name}] : 작물 수확 가능");
        return true;
    }

    /// <summary>
    /// 농지를 만났을 때 상호작용하는 부분이다.
    /// </summary>
    /// <param name="farm">찾은 농지</param>
    /// <param name="equippedTool">착용한 장비</param>
    /// <returns>성공 여부</returns>
    private bool TryInteractiveFarm(Farm farm, ToolData equippedTool)
    {
        if (!TryInteractiveFarmExceptionHandling(farm)) return false;

        if (equippedTool == null)
        {
            Debug.Log($"식물 심기");
            if (!TryPlantSeed(farm)) return false;
            return true;
        }

        else
        {
            switch (equippedTool.toolType)
            {
                case ToolType.WateringCan:
                    if (farm.CanWatered())
                    {
                        farm.Watering();
                        equippedTool.nowDurability -= equippedTool.useDurability; // 내구도 감소
                        return true;
                    }
                    else
                    {
                        Debug.Log($"[player] 이미 화분 젖은 상태");
                        return false;
                    }
                    break;
            }
        }

        Debug.Log($"오류 상황");
        return false;
    }

    /// <summary>
    /// 농지 상호작용의 예외 처리 부분이다.
    /// </summary>
    /// <param name="farm">농장</param>
    /// <returns>최소 조건 만족 여부</returns>
    private bool TryInteractiveFarmExceptionHandling(Farm farm)
    {
        // farm이 없다면
        if (farm == null)
        {
            Debug.Log($"[{name}] : 농지가 존재하지 않음");
            return false;
        }

        Debug.Log($"[{name}] : 농지와 상호작용 가능");
        return true;
    }

    /// <summary>
    /// 현재 사용자가 도구를 들고 있는지 확인하는 코드이다.
    /// </summary>
    /// <returns>도구 아이템 정보</returns>
    private ToolData EquippedTool()
    {
        if (inventoryManager == null)
        {
            Debug.Log($"[{name}] : 인벤토리 매니저 연결 안됨");
            return null; // 인벤토리 매니저가 없음
        }

        if (inventoryManager.SelectedSlotIndex == -1)
        {
            Debug.Log($"[{name}] : 선택된 슬롯 없음");
            return null; // 선택된 슬롯이 없음
        }

        // 현재 선택된 슬롯의 아이템 가져오기
        ItemSlot selectedSlot = inventoryManager.itemSlots[inventoryManager.SelectedSlotIndex];

        if (ReferenceEquals(selectedSlot.itemData, null))
        {
            Debug.Log($"[{name}] : 도구 아이템의 정보가 없음");
            return null;
        }

        if (selectedSlot.itemData.itemType != ItemType.Tool)
        {
            Debug.Log($"[{name}] : 들고 있는 아이템의 종류가 도구가 아님");
            return null;
        }

        Debug.Log($"[{name}] : 도구 반환");
        return selectedSlot.itemData as ToolData; // 도규 아이템 들고 있음
    }
}