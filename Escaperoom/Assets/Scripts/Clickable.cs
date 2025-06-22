using Polyperfect.Universal;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public enum OnClickList
    {
        ActivateGO,
        GetHelp
    }
    public OnClickList onClickList = OnClickList.ActivateGO;

    public GameObject activatedGO;
    private APIController apiController;

    private void Awake()
    {
        apiController = FindObjectOfType<APIController>();
    }

    public void OnClick()
    {
        switch(onClickList)
        {
            case OnClickList.ActivateGO:
                ActivateGO();
                PlayerMovement.Instance.enabled = false;
                break;
            case OnClickList.GetHelp:
                if (apiController != null)
                {
                    apiController.GetHelp();
                    PlayerMovement.Instance.enabled = false;
                }
                break;
        }
    }


    void ActivateGO()
    {
        activatedGO.SetActive(true);
    }
}
