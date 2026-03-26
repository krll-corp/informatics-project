using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class APIController : MonoBehaviour
{
    public static GameStateP1 gameStateP1 = new GameStateP1();
    public static GameStateP2 gameStateP2 = new GameStateP2();

    public static event Action gotNewState;

    public static int playerID = -1;

    private bool isPolling = false;
    private bool isReading = false;


    // Singleton instance
    public static APIController Instance { get; private set; }

    public string serverUrl = "http://10.0.20.60:8000";
    //public GameObject helpPanel;
    //public Text helpText;
    
    public string sessionHash;

    // Private fields

    // --- Serializable classes for JSON ---
    [System.Serializable]
    private class SessionData
    {
        public string hash;
    }

    [System.Serializable]
    private class StateData<T>
    {
        public T state;
    }

    [System.Serializable]
    private class HelpResponse
    {
        public string answer;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        if (playerID != -1)
        {
            switch (playerID) 
            {
                case 0:                    
                    gameStateP1.connected = false;

                    Send<GameStateP1, GameStateP2>(gameStateP1, null);
                    break;

                case 1:
                    gameStateP2.connected = false;

                    Send<GameStateP2, GameStateP1>(gameStateP2, null);
                    break;
            }
        }
    }

    // --- Public Methods ---

    /// <summary>
    /// Sends the current game state to the server.
    /// Can be called from other scripts: APIController.Instance.Send(yourState);
    /// </summary>
    /// <param name="gameState">A dictionary or any serializable object representing the game state.</param>
    public void Send<In, Out>(In gameState, Action<Out> callback) where In : class where Out : class
    {
        if (string.IsNullOrEmpty(sessionHash))
        {
            Debug.LogError("Session hash is not available. Cannot send state.");
            return;
        }
        StartCoroutine(SendCoroutine(gameState, callback));
    }

    /// <summary>
    /// Gets the current game state from the server.
    /// </summary>
    /// <param name="callback">Callback to receive the state.</param>
    public void Get<T>(Action<T> callback, int pID = -1) where T : class
    {
        if (string.IsNullOrEmpty(sessionHash))
        {
            Debug.LogError("Session hash is not available. Cannot get state.");
            callback?.Invoke(null);
            return;
        }
        StartCoroutine(GetCoroutine(callback, pID));
    }

    /// <summary>
    /// Requests help from the server based on the current state.
    /// </summary>
    //public void GetHelp()
    //{
    //    if (string.IsNullOrEmpty(sessionHash))
    //    {
    //        Debug.LogError("Session hash is not available. Cannot get help.");
    //        return;
    //    }
    //    StartCoroutine(GetHelpCoroutine());
    //}

    // --- Coroutines for Web Requests ---



    public IEnumerator HealthCheckAndCreateSession()
    {
        Debug.Log(serverUrl);

        // First, check if the server is healthy
        UnityWebRequest healthRequest = UnityWebRequest.Get($"{serverUrl}/health");
        yield return healthRequest.SendWebRequest();

        if (healthRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Health Check Error: {healthRequest.error}");
            yield break; // Stop if the server is not running
        }
        
        Debug.Log("Server is healthy: " + healthRequest.downloadHandler.text);

        // If healthy, create a new session
        UnityWebRequest sessionRequest = UnityWebRequest.PostWwwForm($"{serverUrl}/sessions", "");

        yield return sessionRequest.SendWebRequest();

        if (sessionRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Create Session Error: {sessionRequest.error}");
        }
        else
        {
            string jsonResponse = sessionRequest.downloadHandler.text;
            Debug.Log("Session created: " + jsonResponse);
            SessionData data = JsonConvert.DeserializeObject<SessionData>(jsonResponse);
            sessionHash = data.hash;
            Debug.Log("Session Hash: " + sessionHash);
        }
    }

    private IEnumerator SendCoroutine<Out, In>(Out gameState, Action<In> callback) where Out : class where In : class
    {

        string url = $"{serverUrl}/sessions/{sessionHash}/{playerID}";
        
        StateData<Out> stateDataOut = new StateData<Out> { state = gameState };
        string jsonData = JsonConvert.SerializeObject(stateDataOut);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Send State Error: {request.error} | Response Code: {request.responseCode}");
        }
        else
        {
            Debug.Log("State sent successfully. Response: " + request.downloadHandler.text);

            // got other gamestate

            string jsonResponse = request.downloadHandler.text;
            Debug.Log("State received successfully. Response: " + jsonResponse);

            try
            {
                StateData<In> stateDataIn = JsonConvert.DeserializeObject<StateData<In>>(jsonResponse);
                callback?.Invoke(stateDataIn.state);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize GameState: {ex.Message}");
                callback?.Invoke(null);
            }
        }
    }

    private IEnumerator GetCoroutine<T>(Action<T> callback, int pID = -1) where T : class
    {
        if (pID == -1) 
        {
            pID = 1 - playerID;
        }

        string url = $"{serverUrl}/sessions/{sessionHash}/{pID}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Get State Error: {request.error} | Response Code: {request.responseCode}");
            callback?.Invoke(null);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("State received successfully. Response: " + jsonResponse);

            try
            {
                StateData<T> stateData = JsonConvert.DeserializeObject<StateData<T>>(jsonResponse);
                callback?.Invoke(stateData.state);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize GameState: {ex.Message}");
                callback?.Invoke(null);
            }
        }
    }


    // used by player 2 to get the state of player 1
    public IEnumerator PollGameStateP1()
    {
        if (isPolling) 
        { 
            yield break;
        }

        isPolling = true;

        while (true)
        {
            isReading = true;

            Send<GameStateP2, GameStateP1>(gameStateP2, state =>
            {
                gameStateP1 = state;
                isReading = false;
            });

            while (isReading)
                yield return null;

            gotNewState?.Invoke();

            yield return new WaitForSeconds(2f);
        }
    }

    public IEnumerator PollGameStateP2()
    {
        if (isPolling)
        {
            yield break;
        }

        isPolling = true;

        while (true)
        {
            isReading = true;

            Send<GameStateP1, GameStateP2>(gameStateP1, state =>
            {
                gameStateP2 = state;
                isReading = false;
            });


            while (isReading)
                yield return null;

            gotNewState?.Invoke();

            yield return new WaitForSeconds(2f);
        }
    }



    //private IEnumerator GetHelpCoroutine() //ai stone
    //{
    //    string url = $"{serverUrl}/sessions/{sessionHash}/help";

    //    UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""); 
    //    request.downloadHandler = new DownloadHandlerBuffer();

    //    yield return request.SendWebRequest();

    //    if (request.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError($"Get Help Error: {request.error} | Response Code: {request.responseCode}");
    //        if (helpText != null)
    //        {
    //            helpText.text = $"Error: {request.error}";
    //        }
    //    }
    //    else
    //    {
    //        string jsonResponse = request.downloadHandler.text;
    //        Debug.Log("AI Help Received: " + jsonResponse);

    //        HelpResponse helpResponse = JsonConvert.DeserializeObject<HelpResponse>(jsonResponse);

    //        if (helpText != null)
    //        {
    //            helpText.text = helpResponse.answer;
    //        }
    //    }

    //    if (helpPanel != null)
    //    {
    //        helpPanel.SetActive(true);
    //    }
    //}
}