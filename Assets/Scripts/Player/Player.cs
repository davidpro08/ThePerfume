using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("기본 설정")]
    public float moveSpeed = 5f;

    [Header("인벤토리 설정")]
    [SerializeField] public InventoryUIManager inventoryUIManager; // 연결할 인벤토리 UI 관리자
    [SerializeField] private InventoryManager inventoryManager;

    [Header("상호작용 설정")]
    [SerializeField] private float InteractionRange = 1f;
    [SerializeField] private LayerMask interactableLayer; // 상호작용할 아이템 레이어
    [SerializeField] private Transform interactionPoint; // 상호작용 지점

    private Vector2 moveInput;
    private bool isRunning;
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

        if (moveInput.magnitude > 0.01f)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }

    // 인벤토리 열기
    void OnInteract(InputValue value)
    {
        if (inventoryUIManager != null)
        {
            inventoryUIManager.ToggleFullInventory();
        }
        else
        {
            Debug.LogWarning("InventoryUIManager가 Player 스크립트에 연결되지 않았습니다.");
        }
    }

    void FixedUpdate()
    {
        Vector2 movement = isRunning ? moveInput * moveSpeed * runRate : moveInput * moveSpeed;
        rb.linearVelocity = movement;
    }
    void Update()
    {
        isRunning = Keyboard.current.leftShiftKey.isPressed;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Vector2 origin = (interactionPoint != null) ? (Vector2)interactionPoint.position : (Vector2)transform.position;
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

    private void PerformPickup(PickupItems pickupItems)
    {
        if (inventoryUIManager == null) return;
        if (pickupItems.itemToGive == null) return;

        bool added = inventoryManager.AddItem(pickupItems.itemToGive, pickupItems.quantityToGive);

    }
}