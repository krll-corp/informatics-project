using System.Collections;
using UnityEngine;

public class Player2_LoadAnim : MonoBehaviour
{
    public Animator animator;

    public static Player2_LoadAnim instance;

    private void Awake()
    {
        instance = this;

        animator.SetBool("IsStarting", true);
        StartCoroutine(startHelper());
    }

    public void fadeOut()
    {
        animator.SetBool("IsEnding", true);
    }


    private IEnumerator startHelper()
    {
        yield return new WaitForSeconds(3f);
        Player2Move.instance.enabled = true;
    }
}
