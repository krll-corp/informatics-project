using Polyperfect.Universal;
using UnityEngine;
using UnityEngine.InputSystem;

public class CloseUI : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Cancel.performed += OnExitPerformed;
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();
        inputActions.UI.Cancel.performed -= OnExitPerformed;
    }

    void OnExitPerformed(InputAction.CallbackContext context)
    {
        gameObject.SetActive(false);
        PlayerMovement.Instance.enabled = true;
    }

}
