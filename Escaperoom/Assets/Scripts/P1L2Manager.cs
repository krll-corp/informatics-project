using UnityEngine;
using UnityEngine.SceneManagement;

public class P1L2Manager : MonoBehaviour
{
    public GameObject codes;
    public GameObject fingerId;


    private bool unlocked;
    
    void Start()
    {
        APIController.gotNewState += onNewState;
        OutlineOnLook.Instance.enabled = false;
    }

    private void OnDisable()
    {
        APIController.gotNewState -= onNewState;

    }


    void onNewState()
    {
        if(APIController.gameStateP2.unlocked && !unlocked)
        {
            unlocked = true;

            codes.SetActive(false);

            // Bücher aktivieren

            fingerId.SetActive(true);
            OutlineOnLook.Instance.enabled = true;
        }

        if (APIController.gameStateP2.finished)
        {
            SceneManager.LoadScene(3);
        }
    }

}
