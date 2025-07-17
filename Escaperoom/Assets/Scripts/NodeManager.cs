using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NodeManager : MonoBehaviour
{
    public GameObject openNode;

    public GameObject mapNode;
    private MMF_Player openMapFeedback;
    private Image mapImage;

    public Sprite mainMap;

    public GameObject blume;
    public GameObject gewehr;
    public GameObject hammer;
    public GameObject saege;

    public GameObject blumeButton;
    public GameObject gewehrButton;
    public GameObject hammerButton;
    public GameObject saegeButton;
    private Button blumeButtonScript;
    private Button gewehrButtonScript;
    private Button hammerButtonScript;
    private Button saegeButtonScript;

    private void Awake()
    {
        openMapFeedback = mapNode.GetComponent<MMF_Player>();
        mapImage = mapNode.GetComponent<Image>();

        blumeButtonScript = blumeButton.GetComponent<Button>();
        gewehrButtonScript = gewehrButton.GetComponent<Button>();
        hammerButtonScript = hammerButton.GetComponent<Button>();
        saegeButtonScript = saegeButton.GetComponent<Button>();

        APIController.gotNewState += playerMapSync;
    }

    private void OnDisable()
    {
        APIController.gotNewState -= playerMapSync;
    }

    public void openMap()
    {
        openNode.SetActive(false);

        mapImage.sprite = mainMap;

        mapNode.SetActive(true);
        openMapFeedback.PlayFeedbacks();
    }

    public void viewBlume()
    {
        blume.SetActive(true);
        gewehr.SetActive(false);
        hammer.SetActive(false);
        saege.SetActive(false);
    }
    public void viewGewehr()
    {
        blume.SetActive(false);
        gewehr.SetActive(true);
        hammer.SetActive(false);
        saege.SetActive(false);
    }
    public void viewHammer()
    {
        blume.SetActive(false);
        gewehr.SetActive(false);
        hammer.SetActive(true);
        saege.SetActive(false);
    }
    public void viewSaege()
    {
        blume.SetActive(false);
        gewehr.SetActive(false);
        hammer.SetActive(false);
        saege.SetActive(true);
    }


    public void hasLevel1Ended()
    {
        if (APIController.gameStateP1.pickup1 && APIController.gameStateP1.pickup2 && APIController.gameStateP1.pickup3 && APIController.gameStateP1.pickup4)
        {
            // Load Next Level
            Player2_LoadAnim.instance.fadeOut();

            StartCoroutine(switchHelper(5));
        }

}
    private IEnumerator switchHelper(int sceneIndex)
    {
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(sceneIndex);
    }


    bool o1 = false;
    bool o2 = false;
    bool o3 = false;
    bool o4 = false;

    int badCount = 0;

    public void playerMapSync()
    {

        if (APIController.gameStateP1.pickup1 && !o1)
        {
            o1 = true;
            // positive feedback
            Player2BGReact.instance.reactionGood(hammerButton.transform.position);

            // disable button 3
            hammerButtonScript.interactable = false;

            hasLevel1Ended();
        }

        if (APIController.gameStateP1.pickup2 && !o2)
        {
            o2 = true;
            // positive feedback
            Player2BGReact.instance.reactionGood(blumeButton.transform.position);

            // disable button 1
            blumeButtonScript.interactable = false;
            hasLevel1Ended();

        }

        if (APIController.gameStateP1.pickup3 && !o3)
        {
            o3 = true;
            // positive feedback
            Player2BGReact.instance.reactionGood(saegeButton.transform.position);

            // disable button 4
            saegeButtonScript.interactable = false;
            hasLevel1Ended();
        }

        if (APIController.gameStateP1.pickup4 && !o4)
        {
            o4 = true;
            // positive feedback
            Player2BGReact.instance.reactionGood(gewehrButton.transform.position);

            // disable button 2
            gewehrButtonScript.interactable = false;
            hasLevel1Ended();
        }

        if (APIController.gameStateP1.wrongTarget > badCount)
        {
            badCount += 1;

            // negative feedback
            Player2BGReact.instance.reactionBad(mapNode.transform.position);
        }
    }
}
