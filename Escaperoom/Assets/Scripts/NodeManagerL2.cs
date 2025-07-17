using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NodeManagerL2 : MonoBehaviour
{

    public GameObject passwordFields;
    private Animator passwordAnim;

    public List<GameObject> pwdInputs = new List<GameObject> (new GameObject[4]);
    private List<TMP_InputField> pwdInputFields = new List<TMP_InputField>(new TMP_InputField[4]);


    public GameObject database;
    private MMF_Player databaseFeedback;
    public GameObject fingerTable;
    public GameObject adressTable;

    public GameObject adressInput;
    private TMP_InputField adressInputField;

    private bool unlocked = false;
    private bool pwdOpen = false;

    private string correctCode = "5973";
    private string correctAdress = "Hermannstrasse 81";

    private int trys = 3;

    private void Start()
    {
        passwordAnim = passwordFields.GetComponent<Animator>();

        for (int i = 0; i < 4; i++)
        {
            pwdInputFields[i] = pwdInputs[i].GetComponent<TMP_InputField>();
        }

        databaseFeedback = database.GetComponent<MMF_Player>();
        adressInputField = adressInput.GetComponent<TMP_InputField>();
    }

    private void firstUnlock()
    {
        Player2BGReact.instance.reactionBad(gameObject.transform.position);

        // reveal code fields
        passwordFields.SetActive(true);
    }

    private void checkPassword()
    {
        if (pwdOpen && !unlocked)
        {
            string code = "";
            foreach (TMP_InputField i in pwdInputFields)
            {
                code += i.text;
            }

            if (code == correctCode)
            {
                unlocked = true;
            }
        }
    }

    private IEnumerator closePwdFields()
    {
        passwordAnim.SetTrigger("playClose");

        foreach (TMP_InputField i in pwdInputFields)
        {
            i.text = "";
        }

        yield return new WaitForSeconds(1.5f);
        passwordFields.SetActive(false);

        database.SetActive(true);
        databaseFeedback.PlayFeedbacks();
    }


    public void dbToggleFingerView()
    {
        if (unlocked)
        {
            fingerTable.SetActive(true);
            adressTable.SetActive(false);
        }
    }
    public void dbToggleAdressView()
    {
        if (unlocked)
        {
            fingerTable.SetActive(false);
            adressTable.SetActive(true);
        }
    }

    private IEnumerator switchHelper(int sceneIndex)
    {
        Player2_LoadAnim.instance.fadeOut();

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(sceneIndex);
    }

    public void dbButtonPressed()
    {
        if (!unlocked && !pwdOpen)
        {
            pwdOpen = true;
            firstUnlock();
        }
        else if (!unlocked && pwdOpen)
        {
            checkPassword();

            if (unlocked)
            {
                Player2BGReact.instance.reactionGood(gameObject.transform.position);

                // next stage
                // Debug.Log("unlocked");

                APIController.gameStateP2.unlocked = true;

                StartCoroutine(closePwdFields());

            }
            else
            {
                Player2BGReact.instance.reactionBad(gameObject.transform.position);
            }
        }
        else
        {
            if (adressInputField.text.ToLower() == correctAdress.ToLower())
            {
                // Level finished

                APIController.gameStateP2.finished = true;

                Player2BGReact.instance.reactionGood(gameObject.transform.position);

                StartCoroutine(switchHelper(6));

            }
            else
            {
                Player2BGReact.instance.reactionBad(gameObject.transform.position);

                trys -= 1;
            }

            if (trys == 0)
            {
                // Game over (reload level)
            }
        }
    }
}
