using UnityEngine;

public class Pickup : MonoBehaviour
{

    public static Pickup instance;

    void Start()
    {
        instance = this;
    }

}
