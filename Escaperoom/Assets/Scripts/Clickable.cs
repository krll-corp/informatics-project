using MoreMountains.Feedbacks;
using Polyperfect.Universal;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public int id;
    public string content;
    public GameObject text;
    private TextMeshProUGUI textField;

    public int targetLocation;
    public Vector3 pickedScale;
    public Vector3 pickedRot;

    public int targetNumber;


    private void Start()
    {
        if (onClickList == OnClickList.ActivateGO)
        {
            textField = text.GetComponent<TextMeshProUGUI>();
        }
    }

    public void OnClick()
    {
        switch(onClickList)
        {
            case OnClickList.ActivateGO:
                ActivateGO();
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
        PlayerMovement.Instance.enabled = false;
        textField.text = content;
        activatedGO.SetActive(true);
    }

    void PickupGO()
    {
        if (Pickup.instance.transform.childCount != 0) 
        {
            return;
        }


        Pickup.instance.transform.localScale = pickedScale;
        Pickup.instance.transform.localEulerAngles = pickedRot;
        gameObject.transform.SetParent(Pickup.instance.transform);
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        gameObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
        gameObject.transform.localScale = new Vector3(1,1,1);
        gameObject.layer = 9;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 9;
        }
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
                delivery.transform.SetParent(null);
            }

            // send negative feedback to player 2
            APIController.gameStateP1.wrongTarget += 1;
        }
        else
        {
            // right target
            delivery.SetActive(false);
            delivery.transform.SetParent(null);

            // send positive feedback to player 2

            switch (target)
            {
                case 0:
                    APIController.gameStateP1.pickup1 = true;
                    break;
                case 1:
                    APIController.gameStateP1.pickup2 = true;
                    break;
                case 2:
                    APIController.gameStateP1.pickup3 = true;
                    break;
                case 3:
                    APIController.gameStateP1.pickup4 = true;
                    break;
            }

            if (APIController.gameStateP1.pickup1 && APIController.gameStateP1.pickup2 && APIController.gameStateP1.pickup3 && APIController.gameStateP1.pickup4)
            {
                // end level

                SceneManager.LoadScene(2);
            }

        }

    }
}
