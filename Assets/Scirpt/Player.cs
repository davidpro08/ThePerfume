using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
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

    // TODO: 인벤토리 열기
    void OnInteract(InputValue value)
    {
        Debug.Log("Interact");
    }

    void FixedUpdate()
    {
        Vector2 movement = moveInput * moveSpeed;
        rb.linearVelocity = movement;
    }
}