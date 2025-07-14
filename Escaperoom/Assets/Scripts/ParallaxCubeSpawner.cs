using UnityEngine;

public class ParallaxCubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public int count = 50;
    public Vector3 areaSize = new Vector3(20, 10, 5);
    public float minZ = 5f;
    public float zStep = 5f;
    public int layers = 3;

    void Start()
    {
        for (int i = 0; i < layers; i++)
        {
            GameObject layer = new GameObject($"Layer_{i + 1}");
            layer.transform.parent = transform;
            float z = minZ + i * zStep;

            for (int j = 0; j < count; j++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-areaSize.x, areaSize.x),
                    Random.Range(-areaSize.y, areaSize.y),
                    z
                );

                GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity, layer.transform);
            }
        }
    }
}
