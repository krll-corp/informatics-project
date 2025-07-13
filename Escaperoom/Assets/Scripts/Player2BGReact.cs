using MoreMountains.Feedbacks;
using UnityEngine;

public class Player2BGReact : MonoBehaviour
{
    
    public static Player2BGReact instance;

    public MMF_Player feedback;

    void Start()
    {
        instance = this;
    }

       
    public void reactionGood(Vector3 start)
    {
        FloatingCube[] cubes = FindObjectsByType<FloatingCube>(0);
        foreach (var cube in cubes)
        {
            cube.ReactFrom(start, speed: 100f, mode: 1); // adjust speed for visual timing
        }

        feedback.FeedbacksList[1].Active = true;

        feedback.PlayFeedbacks();

        feedback.FeedbacksList[1].Active = false;
    }


    public void reactionBad(Vector3 start)
    {
        FloatingCube[] cubes = FindObjectsByType<FloatingCube>(0);
        foreach (var cube in cubes)
        {
            cube.ReactFrom(start, speed: 100f, mode: 2); // adjust speed for visual timing
        }

        feedback.FeedbacksList[2].Active = true;

        feedback.PlayFeedbacks();

        feedback.FeedbacksList[2].Active = false;
    }
}
