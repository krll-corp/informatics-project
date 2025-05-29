using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float rayDisctance = 2f;

    LayerMask layerMask;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        layerMask = LayerMask.GetMask("Clickable");
    }
    private void OnEnable()
    {
        inputActions.UI.Enable();

        inputActions.UI.Click.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();

        inputActions.UI.Click.performed -= OnClickPerformed;

    }

    void OnClickPerformed(InputAction.CallbackContext context)
    {

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, rayDisctance, layerMask))

        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow, duration: 1);
            Debug.Log("Did Hit");

            hit.transform.gameObject.GetComponent<Clickable>().OnClick();
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white, duration: 1);
            Debug.Log("Did not Hit");
        }

    }
}
