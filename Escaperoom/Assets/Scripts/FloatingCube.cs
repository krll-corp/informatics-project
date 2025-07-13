using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class FloatingCube : MonoBehaviour
{
    public float amplitude = 0.2f;
    public float speed = 1f;
    private Vector3 startPos;
    private float offset;

    [Header("Feel Feedback")]
    public MMF_Player feedback;

    void Start()
    {
        startPos = transform.position;
        offset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed + offset) * amplitude;
        transform.position = startPos + new Vector3(0, y, 0);
    }

    public void React()
    {
        if (feedback != null)
        {
            feedback.PlayFeedbacks();
        }
    }

    public void ReactFrom(Vector3 origin, float speed = 10f, int mode = 1)
    {
        float distance = Vector3.Distance(new Vector3(transform.position.x, transform.position.y, 0), new Vector3(origin.x, origin.y, 0));
        float delay = distance / speed; // seconds per unit distance

        StartCoroutine(PlayFeedbackAfterDelay(delay, mode));
    }

    private IEnumerator PlayFeedbackAfterDelay(float delay, int mode)
    {
        yield return new WaitForSeconds(delay);
        if (feedback != null)
        {

            feedback.FeedbacksList[mode].Active = true;

            feedback.PlayFeedbacks();

            feedback.FeedbacksList[mode].Active = false;
        }
    }
}
