using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class APIController : MonoBehaviour
{
    public string server_url = "https://10.0.20.60:8000/";

    void Start()
    {
        StartCoroutine(getRequest(server_url));
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
}
