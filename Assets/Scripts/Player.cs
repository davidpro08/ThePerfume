using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("기본 설정")]
    public float moveSpeed = 5f;
    
    [Header("인벤토리 설정")]
    public InventoryUIManager inventoryUIManager; // 연결할 인벤토리 UI 관리자

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //void OnMove(InputValue value)
    //{
    //    moveInput = value.Get<Vector2>();
    //}

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
        Vector2 movement = moveInput * moveSpeed;
        rb.linearVelocity = movement;
    }
    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);
        if(context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }
}