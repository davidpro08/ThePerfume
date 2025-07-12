using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // 기본 속도
    private float runSpeed = 7f; // 달리기 속도
    private bool isRunning = false; // 달리기 여부 (LeftShift)

    private Vector2 moveInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 movement = isRunning ? runSpeed * moveInput : moveSpeed * moveInput;
        rb.linearVelocity = movement;
    }

    void Update()
    {
        isRunning = Keyboard.current.leftShiftKey.isPressed;
    }
}