using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private InputSystem_Actions inputActions;
    private Vector2 inputMove;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();

        // Movement
        inputActions.Player.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => inputMove = Vector2.zero;

        // Jump
        inputActions.Player.Jump.performed += ctx => jumpRequested = true;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void FixedUpdate()
    {
        Move();
        if (jumpRequested && IsGrounded())
        {
            Debug.Log("Jump");
            Jump();
        }
        jumpRequested = false; // Reset after processing
    }

    private void Move()
    {
        Vector3 direction = new Vector3(inputMove.x, 0f, inputMove.y);
        Vector3 worldDirection = transform.TransformDirection(direction);
        Vector3 velocity = new Vector3(worldDirection.x * moveSpeed, rb.linearVelocity.y, worldDirection.z * moveSpeed);
        rb.linearVelocity = velocity;
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        bool rayc = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 1.1f, groundMask);
        Debug.Log(rayc);
        return rayc;
    }
}
