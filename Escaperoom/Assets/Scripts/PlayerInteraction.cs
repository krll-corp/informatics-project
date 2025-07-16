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

        inputActions.UI.Click.started += OnClickPerformed;
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();

        inputActions.UI.Click.started -= OnClickPerformed;

    }

    void OnClickPerformed(InputAction.CallbackContext context)
    {

        GameObject sel = OutlineOnLook.Instance.selected;

        if ( sel is not null )
        {
            sel.GetComponent<Clickable>().OnClick();
        }
    }
}
