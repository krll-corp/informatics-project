using Polyperfect.Universal;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public enum OnClickList
    {
        ActivateGO
    }
    public OnClickList onClickList = OnClickList.ActivateGO;

    public GameObject activatedGO;

    public void OnClick()
    {
        switch(onClickList)
        {
            case OnClickList.ActivateGO:
                ActivateGO();
                PlayerMovement.Instance.enabled = false;
                break;
        }
    }


    void ActivateGO()
    {
        activatedGO.SetActive(true);
    }
}
