using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NodeTrigger : MonoBehaviour
{
    public void OnNodeClicked()
    {
        Vector3 clickPos = transform.position; // or wherever the node actually is

        Player2BGReact.instance.reactionGood(clickPos);
    }

    public void switchScene()
    {
        Player2_LoadAnim.instance.fadeOut();

        StartCoroutine(switchHelper());
    }

    private IEnumerator switchHelper()
    {
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(1);
    }
}
