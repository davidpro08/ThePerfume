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

    // PlayerInput으로 입력 받을 수 있도록 수정중
    void OnPickUp(InputValue value)
    {
        Vector2 origin = interactionPoint != null ? interactionPoint.position : transform.position;
        
        // 주변 상호작용 가능 오브젝트 존재 여부 판단
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, InteractionRange, interactableLayer);

        foreach (Collider2D hitCollider in hitColliders)
        {
            PickupItems pickupItems = hitCollider.GetComponent<PickupItems>();
            // 인벤토리에 추가되는 아이템 획득
            if (pickupItems != null)
            {
                Debug.Log($"아이템 발견");
                PerformPickup(pickupItems);
                return;
            }
            // NPC 대화 같은 상호 작용 (추가 예정)
            else
            {
                Debug.Log($"아이템 획득 외 상호작용");
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
}