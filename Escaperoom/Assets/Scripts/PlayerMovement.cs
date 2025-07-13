using Polyperfect.Universal;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float sprintMult = 1.5f;
    public float maxVelocityChange = 10f; // For AddForce method if you prefer acceleration
    public float jumpForce = 5f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float lookSensitivity = 0.1f;
    public float verticalLookLimit = 80f;

    [Header("Ground Check")]
    public Transform groundCheckPoint; // An empty GameObject at the player's feet
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; // Set this to your ground layer in the Inspector

    private Rigidbody _rigidbody;
    private InputSystem_Actions inputActions;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _cameraPitch = 0f;
    private bool _isGrounded;
    private float _trueSprintMult = 1f;

    public static PlayerMovement Instance;

    // Choose one movement style
    public enum MovementStyle
    {
        SetVelocity,
        AddForce
    }
    public MovementStyle movementStyle = MovementStyle.SetVelocity;


    private void Awake()
    {
        Instance = this;

        _rigidbody = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();


        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
            Debug.LogWarning("Player Camera not assigned. Attempting to use Main Camera. For best results, assign it manually.");
        }

        if (groundCheckPoint == null)
        {
            // Create a default ground check point if none is assigned
            GameObject gcp = new GameObject("GroundCheckPoint");
            gcp.transform.SetParent(transform);
            gcp.transform.localPosition = new Vector3(0, -GetComponent<CapsuleCollider>().height / 2 + 0.01f, 0); // Adjust based on collider
            groundCheckPoint = gcp.transform;
            Debug.LogWarning("GroundCheckPoint not assigned. Created a default one. Adjust its position if needed.");
        }


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.Look.performed += OnLookPerformed;
        inputActions.Player.Look.canceled += OnLookCanceled;

        inputActions.Player.Jump.performed += OnJumpPerformed;

        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();

        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.Look.performed -= OnLookPerformed;
        inputActions.Player.Look.canceled -= OnLookCanceled;

        inputActions.Player.Jump.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        HandleLooking();
    }

    private void FixedUpdate()
    {
        PerformGroundCheck();
        HandleMovement();
    }

    // --- Input Action Callbacks ---
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        _trueSprintMult = sprintMult;
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        _trueSprintMult = 1f;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        _lookInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // --- Core Logic ---

    private void PerformGroundCheck()
    {
        if (groundCheckPoint != null)
        {
            _isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void HandleMovement()
    {

        // Calculate movement direction based on player's forward and right vectors
        Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        moveDirection.Normalize(); // Ensure consistent speed if moving diagonally

        if (movementStyle == MovementStyle.SetVelocity)
        {
            // --- Method 1: Setting Velocity Directly ---
            // Preserves current vertical velocity (gravity)
            Vector3 targetVelocity = moveDirection * (moveSpeed * _trueSprintMult);
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
        }
        else if (movementStyle == MovementStyle.AddForce)
        {
            // --- Method 2: Adding Force (more physics-based acceleration) ---
            // Calculate how much to accelerate to reach target velocity
            Vector3 targetVelocity = moveDirection * (moveSpeed * _trueSprintMult);
            Vector3 velocityChange = (targetVelocity - new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z)); // Ignore Y for velocity change calculation
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0; // We only want to affect horizontal movement force

            if (_isGrounded) // Apply more force when grounded for responsiveness
            {
                _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            else // Less air control
            {
                _rigidbody.AddForce(velocityChange * 0.5f, ForceMode.VelocityChange); // Example: 50% air control
            }
        }
    }

    private void HandleLooking()
    {
        if (playerCamera == null) return;

        // Horizontal rotation (yaw)
        float mouseX = _lookInput.x * lookSensitivity;

        Quaternion deltaRotation = Quaternion.Euler(Vector3.up * mouseX);
        _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);


        // Vertical rotation (pitch)
        float mouseY = _lookInput.y * lookSensitivity;
        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -verticalLookLimit, verticalLookLimit);

        playerCamera.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
    }

    // Optional: Draw gizmo for ground check visualization in editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}