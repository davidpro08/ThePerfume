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

    [Header("상호작용 설정")][SerializeField] private float InteractionRange = 1f;
    [SerializeField] private LayerMask interactableLayer; // 상호작용할 아이템 레이어
    [SerializeField] private Transform interactionPoint; // 상호작용 지점

    private Vector2 _moveInput;
    private bool _isSprint;
    private readonly float _runRate = 1.8f; // 걷는 속력과 비교한 달리기 속력비
    private Rigidbody2D _rb;
    private Animator _animator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {

    }
    private void Update()
    {
        if (PauseManager.Instance.IsPlayerMovementBlocked() || isOpenInventory)
        {
            if (_moveInput != Vector2.zero)
            {
                _moveInput = Vector2.zero;
            }
            return;
        }

    }

    void FixedUpdate()
    {
        // 인벤토리 여면 못 움직이게
        if (PauseManager.Instance.IsPlayerMovementBlocked() || isOpenInventory)
        {
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool("isWalking", false);
            return;
        }

        // 입력이 거의 없으면 멈추고 Idle 유지
        if (_moveInput.magnitude <= 0.01f)
        {
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool("isWalking", false);
            return;
        }

        // 움직일 때만 속도 적용
        Vector2 movement = _isSprint ? _moveInput * (moveSpeed * _runRate) : _moveInput * moveSpeed;
        _rb.linearVelocity = movement;
    }

    void OnMove(InputValue value)
    {
        if (PauseManager.Instance.IsPlayerMovementBlocked() || isOpenInventory) return;
        _moveInput = value.Get<Vector2>();
        UpdateAnimator(_moveInput);
    }

    private void UpdateAnimator(Vector2 input)
    {
        float magnitude = input.magnitude;

        if (magnitude <= 0.01f)
        {
            // Idle 상태에서는 isWalking만 false, 방향 값은 그대로 유지
            _animator.SetBool("isWalking", false);
            return;
        }

        // 움직이는 경우에만 파라미터 갱신
        _animator.SetBool("isWalking", true);
        _animator.SetFloat("InputX", input.x);
        _animator.SetFloat("InputY", input.y);
        SetLastInputDirection(input);  // 마지막 방향 갱신
    }

    private void SetLastInputDirection(Vector2 input)
    {
        _animator.SetFloat("LastInputX", input.x);
        _animator.SetFloat("LastInputY", input.y);
    }


    void OnSprint(InputValue value)
    {
        _isSprint = value.Get<float>() > 0.5f;
        Debug.Log("Sprint state: " + _isSprint);
    }

    // 인벤토리 열기
    void OnOpenInventory(InputValue value)
    {
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
    void OnInteract(InputValue value)
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

            IInteract interact = hitCollider.GetComponent<IInteract>();
            interact.Interact(this);

            return;
        }

        Debug.Log($"상호작용 없음");
    }

    // PlayerInput에서 Pause 액션 호출 시 연결
    public void OnPause(InputValue value)
    {
        PauseManager.Instance.TogglePause(true);
    }
}