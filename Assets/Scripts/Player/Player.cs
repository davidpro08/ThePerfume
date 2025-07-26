using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("기본 설정")] public float moveSpeed = 5f;

    [Header("인벤토리 설정")] private bool isOpenInventory = false;
    [SerializeField] public InventoryUIManager inventoryUIManager; // 연결할 인벤토리 UI 관리자
    [SerializeField] private InventoryManager inventoryManager;

    [Header("상호작용 설정")] [SerializeField] private float InteractionRange = 1f;
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


    }

    private void Start()
    {
        // inventoryManager = InventoryManager.Instance;
        // if (inventoryManager == null)
        // {
        //     // DontDestoryOnLoad로 인해 지정해준게 빠져버려서 수정해봄
        //     //inventoryManager = FindAnyObjectByType<InventoryManager>();
        //     inventoryManager = InventoryManager.Instance;
        //     Debug.Log($"[Player] 인벤토리 매니저 연결 완료");
        // }

        // 연결이 제대로 됐는지 확인
        if (inventoryManager == null)
        {
            Debug.Log($"[Player] InventoryManger 미발견");
            StartCoroutine(WaitForInventoryManager());
        }
        else
        {
            inventoryManager = InventoryManager.Instance;
        }
    }

    private IEnumerator WaitForInventoryManager()
    {
        while (InventoryManager.Instance == null)
        {
            yield return null;
        }

        inventoryManager = InventoryManager.Instance;
        Debug.Log($"InventoryManager 참조 완료");
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
            // 플레이어 위치와 감지된 오브젝트 사이의 거리 계산
            float distaceToTarget = Vector2.Distance(transform.position, detection.collider.transform.position);
            if (distaceToTarget > InteractionRange)
            {
                return;
            }

            if (detection.collider.CompareTag("bench"))
            {
                SceneChanger sceneChanger = detection.collider.GetComponent<SceneChanger>();
                if (sceneChanger != null)
                {
                    sceneChanger.MoveToScene();
                    return;
                }
            }
            
            // 상호작용 부분
            IInteract interact = detection.collider.GetComponent<IInteract>();
            interact.Interact(this);
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
            // 작업대
            SceneChanger sceneChanger = hitCollider.GetComponent<SceneChanger>();
            if (sceneChanger != null)
            {
                if (hitCollider.CompareTag("bench"))
                {
                    sceneChanger.MoveToScene();
                    return;
                }
            }
            // 작물
            IInteract interact = hitCollider.GetComponent<IInteract>();
            interact.Interact(this);
            
            return;
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
}
    
    