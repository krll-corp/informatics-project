using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Move : MonoBehaviour
{
    [Header("Camera Panning")]
    public float dragSpeed = 0.1f;
    public float smoothTime = 0.1f;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    private InputAction lookAction;
    private InputAction clickAction;
    private InputSystem_Actions playerInput;
    private bool isDragging = false;

    private void OnEnable()
    {
        playerInput = new InputSystem_Actions();
        lookAction = playerInput.Player.Look;
        clickAction = playerInput.UI.Click;

        playerInput.Player.Enable();
        playerInput.UI.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.UI.Disable();
    }

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (clickAction.IsPressed())
        {
            Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
            PanCamera(mouseDelta);
        }

        SmoothMove();
    }

    private void PanCamera(Vector2 delta)
    {
        // Convert mouse movement into world-space movement
        Vector3 move = new Vector3(-delta.x, -delta.y, 0) * dragSpeed;

        // Move along camera's local right and up
        targetPosition += transform.right * move.x + transform.up * move.y;
    }

    private void SmoothMove()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
