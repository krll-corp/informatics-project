using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class APIController : MonoBehaviour
{
    public class HashData
    {
        string hash;
    }

    public string server_url = "https://10.0.20.60:8000/";

    void Start()
    {
        StartCoroutine(getHashRequest(server_url));
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
}
