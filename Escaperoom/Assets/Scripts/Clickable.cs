using MoreMountains.Feedbacks;
using Polyperfect.Universal;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public enum OnClickList
    {
        ActivateGO,
        Pickup,
        Deliver
    }
    public OnClickList onClickList = OnClickList.ActivateGO;

    public GameObject activatedGO;
    private APIController apiController;

    private void Awake()
    {
        apiController = FindObjectOfType<APIController>();
    }

    public int targetLocation;

    public int targetNumber;

    public void OnClick()
    {
        switch(onClickList)
        {
            case OnClickList.ActivateGO:
                ActivateGO();
                PlayerMovement.Instance.enabled = false;
                break;

            case OnClickList.Pickup:
                PickupGO();
                break;

            case OnClickList.Deliver:
                Deliver();
                break;
        }
    }


    void ActivateGO()
    {
        activatedGO.SetActive(true);
    }

    void PickupGO()
    {
        if (Pickup.instance.transform.childCount != 0) 
        {
            return;
        }

        gameObject.transform.SetParent(Pickup.instance.transform);
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        gameObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
        gameObject.transform.localScale = new Vector3(1,1,1);
        gameObject.layer = 9;
    }

    void Deliver()
    {
        if (Pickup.instance.transform.childCount != 1)
        {
            return;
        }

        GameObject delivery = Pickup.instance.transform.GetChild(0).gameObject;

        int target = delivery.GetComponent<Clickable>().targetLocation;

        if (target != targetNumber) 
        {
            // wrong target

            if (target >= 4)
            {
                // invalid target (just 4 on level 1)
                delivery.SetActive(false);

                // send negative feedback to player 2

            }

        }
        else
        {
            // right target
            delivery.SetActive(false);

            // send positive feedback to player 2
        }

    }
}
