using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class OutlineOnLook : MonoBehaviour
{
    public float angle = 0.97f;
    public float dist = 3f;


    public GameObject[] outlined;

    private Dictionary<Outline, float> possibleSelects = new Dictionary<Outline, float>();

    public GameObject selected;

    public static OutlineOnLook Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        possibleSelects.Clear();
        foreach (GameObject go in outlined) {

            if (! go.activeSelf)
            {
                continue;
            }

            Vector3 direction = go.GetComponent<Renderer>().bounds.center - transform.position;
            float distance = direction.magnitude;
            Outline outline = go.GetComponent<Outline>();

            if (distance < dist)
            {
                float dot = Vector3.Dot(direction.normalized, transform.forward);
                if (dot > angle) 
                {
                    possibleSelects[outline] = dot;
                }
            }
            
            outline.enabled = false;            
        }

        float best = 0;
        Outline best_o = null;
        foreach (Outline o in  possibleSelects.Keys)
        {
            if (possibleSelects[o] > best)
            {
                best = possibleSelects[o];
                best_o = o;
            }
        }

        if (best_o is null)
        {
            selected = null;
            return;
        }

        best_o.enabled = true;
        selected = best_o.gameObject;
    }
}
