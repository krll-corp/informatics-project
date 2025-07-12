using UnityEngine;

public class NodeTrigger : MonoBehaviour
{
    public void OnNodeClicked()
    {
        Vector3 clickPos = transform.position; // or wherever the node actually is

        FloatingCube[] cubes = FindObjectsByType<FloatingCube>(0);
        foreach (var cube in cubes)
        {
            cube.ReactFrom(clickPos, speed: 100f); // adjust speed for visual timing
        }
    }
}
