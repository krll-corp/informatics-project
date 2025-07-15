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
        StartCoroutine(createSession());
    }

    private void OnPlayerJoinClick(ClickEvent e)
    {
        APIController.Instance.sessionHash = _joinCode.text;

        _hostButton.UnregisterCallback<ClickEvent>(OnPlayerHostClick);
        _joinButton.UnregisterCallback<ClickEvent>(OnPlayerJoinClick);

        _document.enabled = false;

        WaitText.text = "Joining..";

        waitScreen.SetActive(true);

        // verify code and establish first game state

        APIController.Instance.Get(getCallback);
    }


    private void getCallback(GameState state)
    {
        if (state == null)
        {
            Debug.LogWarning("Failed to retrieve state.");

            StartCoroutine(connectionFailed());

            return;
        }
        if(state.player1 && state.player2)
        {
            Debug.LogWarning("Lobby full.");

            StartCoroutine(connectionFailed());

            return;
        }

        WaitText.text = "Connected!";

        APIController.playerID = 1;

        // start level if player 1(id=0) is connected
    }


    private IEnumerator connectionFailed()
    {
        HostCodeText.text = "Connection Faied";

        yield return new WaitForSeconds(3);

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

        Debug.Log(APIController.Instance.sessionHash);

        if ( string.IsNullOrEmpty(APIController.Instance.sessionHash) )
        {
            yield return connectionFailed();
            yield break ;
        }

        GameState state = new GameState();
        state.player1 = true;

        APIController.Instance.Send(state);

        WaitText.text = "Waiting for second Player..";

        APIController.playerID = 0;

        // paste HostCode when connected

        HostCodeText.text = APIController.Instance.sessionHash;

        // detect when second player connects

        // start level
    }

}
