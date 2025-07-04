using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;

public class APIController : MonoBehaviour
{
    Dictionary<string, int> cur_state = new Dictionary<string, int>();
    public class HashData
    {
        public string hash;
    }

    public string server_url = "https://10.0.20.60:8000/";
    public GameObject helpPanel;
    public Text helpText;

    private HashData data;

    void Start()
    {
        checkS(server_url);
        StartCoroutine(getHashRequest(server_url));
    }

    IEnumerator checkS(string server_url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(server_url + "health");
        yield return uwr.SendWebRequest();
        if (uwr.responseCode != 200)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            Debug.Log("Response Code: " + uwr.responseCode);
        }
        else
        {
            Debug.Log("server running: " + uwr.downloadHandler.text);
        }
    }

    IEnumerator getHashRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri + "getHash");
        yield return uwr.SendWebRequest();

        if (uwr.responseCode != 200)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            Debug.Log("Response Code: " + uwr.responseCode);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);

            data = JsonConvert.DeserializeObject<HashData>(uwr.downloadHandler.text);
        }
    }

    IEnumerator SendStateLoop(){
    while (true)
    {
        yield return new WaitForSeconds(2);
        if (data != null && !string.IsNullOrEmpty(data.hash))
        {
            cur_state["example_key"] = UnityEngine.Random.Range(0, 100); //for now
            
            StartCoroutine(SendStateRequest(server_url, cur_state));
        }
    }
}

IEnumerator SendStateRequest(string uri, Dictionary<string, int> state)
{
    string endpoint = $"{data.hash}";
    string fullUrl = uri + endpoint;
    string jsonData = JsonConvert.SerializeObject(state);

    UnityWebRequest uwr = new UnityWebRequest(fullUrl, "POST");
    byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
    uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
    uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    uwr.SetRequestHeader("Content-Type", "application/json");

    yield return uwr.SendWebRequest();

    if (uwr.responseCode == 200)
    {
        Debug.Log("State sent successfully.");
    }
    else
    {
        Debug.Log("Error While Sending State: " + uwr.error);
        Debug.Log("Response Code: " + uwr.responseCode);
    }
}



    public void GetHelp()
    {
        StartCoroutine(getHelpRequest());
    }

    IEnumerator getHelpRequest()
    {
        // secret adjustments
        string endpoint = $"{data.hash}/help";

        UnityWebRequest uwr = UnityWebRequest.Get(server_url + endpoint);
        yield return uwr.SendWebRequest();

        if (uwr.responseCode != 200)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            Debug.Log("Response Code: " + uwr.responseCode);
            if (helpText != null)
            {
                helpText.text = "Error: " + uwr.error;
            }
        }
        else
        {
            Debug.Log("AIR help: " + uwr.downloadHandler.text);
            if (helpText != null)
            {
                helpText.text = uwr.downloadHandler.text;
            }
        }

        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
        }
    }
}
