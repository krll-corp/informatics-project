using UnityEngine;

public class NodeTrigger : MonoBehaviour
{
    public void OnNodeClicked()
    {
        Vector3 clickPos = transform.position; // or wherever the node actually is

        Player2BGReact.instance.reactionGood(clickPos);
    }
}
