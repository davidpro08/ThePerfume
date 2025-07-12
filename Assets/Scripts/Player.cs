using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("기본 설정")]
    public float moveSpeed = 5f;

    [Header("인벤토리 설정")]
    public InventoryUIManager inventoryUIManager; // 연결할 인벤토리 UI 관리자

    private Vector2 moveInput;
    private bool isRunning;
    private float runRate = 1.8f; // 걷는 속력과 비교한 달리기 속력비
    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
    }
}