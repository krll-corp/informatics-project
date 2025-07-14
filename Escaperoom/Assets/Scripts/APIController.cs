using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

public class APIController : MonoBehaviour
{
    // Singleton instance
    public static APIController Instance { get; private set; }

    public string serverUrl = "http://10.0.20.60:8000";
    //public GameObject helpPanel;
    //public Text helpText;

    // Private fields
    private string sessionHash;

    // --- Serializable classes for JSON ---
    [System.Serializable]
    private class SessionData
    {
        public string hash;
    }

    [System.Serializable]
    private class StateData
    {
        public object state;
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

    private void Start()
    {
        // Check server health and create a new session when the game starts
        StartCoroutine(HealthCheckAndCreateSession());
    }

    // --- Public Methods ---

    /// <summary>
    /// Sends the current game state to the server.
    /// Can be called from other scripts: APIController.Instance.Send(yourState);
    /// </summary>
    /// <param name="gameState">A dictionary or any serializable object representing the game state.</param>
    public void Send(object gameState)
    {
        if (string.IsNullOrEmpty(sessionHash))
        {
            Debug.LogError("Session hash is not available. Cannot send state.");
            return;
        }
        StartCoroutine(SendCoroutine(gameState));
    }

    /// <summary>
    /// Gets the current game state from the server.
    /// </summary>
    /// <param name="callback">Callback to receive the state.</param>
    public void Get(Action<object> callback)
    {
        if (string.IsNullOrEmpty(sessionHash))
        {
            Debug.LogError("Session hash is not available. Cannot get state.");
            callback?.Invoke(null);
            return;
        }
        StartCoroutine(GetCoroutine(callback));
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

    private IEnumerator HealthCheckAndCreateSession()
    {
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

    private IEnumerator SendCoroutine(object gameState)
    {
        string url = $"{serverUrl}/sessions/{sessionHash}";
        
        StateData stateData = new StateData { state = gameState };
        string jsonData = JsonConvert.SerializeObject(stateData);
        
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
        }
    }

    private IEnumerator GetCoroutine(Action<object> callback)
    {
        string url = $"{serverUrl}/sessions/{sessionHash}";

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
            StateData stateData = JsonConvert.DeserializeObject<StateData>(jsonResponse);
            callback?.Invoke(stateData.state);
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