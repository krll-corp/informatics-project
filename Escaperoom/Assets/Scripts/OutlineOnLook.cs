using System.Collections.Generic;
using UnityEngine;

public class OutlineOnLook : MonoBehaviour
{
    public float angle = 0.97f;
    public float dist = 3f;

    public GameObject[] outlined;

    void Start()
    {
        
    }

    void Update()
    {
        foreach (GameObject go in outlined) {

            Vector3 direction = go.transform.position - transform.position;
            float distance = direction.magnitude;
            Outline outline = go.GetComponent<Outline>();

            if (distance < dist)
            {
                float dot = Vector3.Dot(direction.normalized, transform.forward);
                if (dot > angle) 
                {
                    outline.enabled = true;
                    continue;
                }
            }
            
            outline.enabled = false;
            
            
        }
    }
}
