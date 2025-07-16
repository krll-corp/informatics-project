using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    private UIDocument _document;

    private Button _hostButton;
    private Button _joinButton;

    private TextField _joinCode;

    public GameObject waitScreen;

    public GameObject HostCodeTextField;
    private TextMeshProUGUI HostCodeText;

    public GameObject WaitTextField;
    private TextMeshProUGUI WaitText;


    private class gameState
    {
        Dictionary<int, bool> players = new Dictionary<int, bool>();
    }


    private void Awake()
    {
        getUi();

        HostCodeText = HostCodeTextField.GetComponent<TextMeshProUGUI>();
        WaitText = WaitTextField.GetComponent<TextMeshProUGUI>();
    }

    private void getUi()
    {
        _document = GetComponent<UIDocument>();


        _joinCode = _document.rootVisualElement.Q("Code") as TextField;


        _hostButton = _document.rootVisualElement.Q("HostButton") as Button;
        _hostButton.RegisterCallback<ClickEvent>(OnPlayerHostClick);

        _joinButton = _document.rootVisualElement.Q("JoinButton") as Button;
        _joinButton.RegisterCallback<ClickEvent>(OnPlayerJoinClick);
    }

    private void OnDisable()
    {
        _hostButton.UnregisterCallback<ClickEvent>(OnPlayerHostClick);
        _joinButton.UnregisterCallback<ClickEvent>(OnPlayerJoinClick);
    }

    private void OnPlayerHostClick(ClickEvent e)
    {
        APIController.playerID = 0;

        StartCoroutine(createSession());
    }

    private void OnPlayerJoinClick(ClickEvent e)
    {
        APIController.Instance.sessionHash = _joinCode.text;

        APIController.playerID = 1;

        _hostButton.UnregisterCallback<ClickEvent>(OnPlayerHostClick);
        _joinButton.UnregisterCallback<ClickEvent>(OnPlayerJoinClick);

        _document.enabled = false;

        WaitText.text = "Joining..";

        waitScreen.SetActive(true);

        // verify code and establish first game state

        // Lobby full check missing here


        APIController.Instance.Send<GameStateP2, GameStateP1>(APIController.gameStateP2, getP1Callback);
    }


    private void getP1Callback(GameStateP1 stateP1)
    {
        if (stateP1 == null)
        {
            //Lobby doesn't exist

            Debug.LogWarning("Failed to retrieve state.");

            StartCoroutine(connectionFailed());

            return;
        }

        // Debug.LogWarning("Lobby full.");
        // StartCoroutine(connectionFailed());
        // return;

        if (stateP1.connected)
        {
            // Player 1 is connected

            StartCoroutine(APIController.Instance.PollGameStateP1());

            WaitText.text = "Connected!";
        }

        else 
        {
            Debug.LogWarning("Player 1 disconected.");

            StartCoroutine(connectionFailed());

            return;
        }
    }


    private IEnumerator connectionFailed()
    {
        HostCodeText.text = "Connection Faied";

        yield return new WaitForSeconds(3);

        APIController.playerID = -1;
        HostCodeText.text = "";
        _document.enabled = true;
        waitScreen.SetActive(false);

        getUi();

        yield break;
    }

    private IEnumerator createSession()
    {
        _hostButton.UnregisterCallback<ClickEvent>(OnPlayerHostClick);
        _joinButton.UnregisterCallback<ClickEvent>(OnPlayerJoinClick);

        _document.enabled = false;

        WaitText.text = "Connecting..";

        waitScreen.SetActive(true);

        yield return APIController.Instance.HealthCheckAndCreateSession();

        if ( string.IsNullOrEmpty(APIController.Instance.sessionHash) )
        {
            yield return connectionFailed();
            yield break ;
        }

       
        // Initiate polling

        StartCoroutine(APIController.Instance.PollGameStateP2());


        // mark as connected
        APIController.gameStateP1.connected = true;


        WaitText.text = "Waiting for second Player..";

        // paste HostCode when connected

        HostCodeText.text = APIController.Instance.sessionHash;

        // detect when second player connects

        // start level
    }

}
