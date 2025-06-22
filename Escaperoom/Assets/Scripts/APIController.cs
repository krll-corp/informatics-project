using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;

public class APIController : MonoBehaviour
{
    public class HashData
    {
        string hash;
    }

    public string server_url = "https://10.0.20.60:8000/";
    public GameObject helpPanel;
    public Text helpText;

    void Start()
    {
        checkS(server_url);
        StartCoroutine(getHashRequest(server_url));
    }

    IEnumerator checkS(string server_url){
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

    IEnumerator getRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.responseCode != 200)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            Debug.Log("Response Code: " + uwr.responseCode);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
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

            HashData data = JsonConvert.DeserializeObject<HashData>(uwr.downloadHandler.text);
            
        }
    }

    public void GetHelp()
    {
        StartCoroutine(getHelpRequest());
    }

    IEnumerator getHelpRequest()
    {
        // secret adjustments
        string endpoint = "help";

        UnityWebRequest uwr = UnityWebRequest.Get(server_url + endpoint);
        yield return uwr.SendWebRequest();

        if (uwr.responseCode != 200)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            Debug.Log("Response Code: " + uwr.responseCode);
            if (helpText != null) {
                helpText.text = "Error: " + uwr.error;
            }
        }
        else
        {
            Debug.Log("AIR help: " + uwr.downloadHandler.text);
            if (helpText != null) {
                helpText.text = uwr.downloadHandler.text;
            }
        }

        if (helpPanel != null) {
            helpPanel.SetActive(true);
        }
    }
}
