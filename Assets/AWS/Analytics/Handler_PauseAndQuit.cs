using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handler_PauseAndQuit : MonoBehaviour
{
    private void OnApplicationFocus(bool focus)
    {

        if (focus)
        {
            AudioListener.volume = 1f;
            Analytics.ContinueSession();
        }
        else
        {
            AudioListener.volume = 0f;
            if (AppManagerPlace.Instance.UseAnalytics)
            {
                //Debug.Log("[Handler_PauseAndQuit : OnApplicationFocus] : Lost Focus, PlayCount: " + AppManager.Instance.VideoController.PlayCount);
                // if the playcount is more than 1, the video was played all the way through at least once
                if (AppManager.Instance.VideoController.PlayCount > 1)
                {
                    //Debug.Log("[Handler_PauseAndQuit : OnApplicationFocus] : Lost Focus, Exiting video after at least one playthrough, PlayCount: " + AppManager.Instance.VideoController.PlayCount + "times");
                    Analytics.EndVideo(false);
                }
                else
                {
                    //Debug.Log("[Handler_PauseAndQuit : OnApplicationFocus] : Lost Focus, Exiting video early after playing for PlayCount: " + AppManager.Instance.VideoController.PlayCount + "times");
                    Analytics.EndVideo(true);
                }
            }
            Analytics.EndSession();
        }

        Debug.Log($"[Handler_PauseAndQuit : OnApplicationFocus] _{focus}");
    }

    //private void OnApplicationPause(bool pause)
    //{
    //    Debug.Log($"[Handler_PauseAndQuit : OnApplicationPause] {pause}");
    //
    //    if (pause)
    //    {
    //        Analytics.EndSession();
    //    }
    //    else
    //    {
    //        Analytics.ContinueSession();
    //    }
    //}
    //
    private void OnApplicationQuit()
    {
        AudioListener.volume = 0f;

        if (AppManagerPlace.Instance.UseAnalytics)
        {
            //Debug.Log("[Handler_PauseAndQuit : OnApplicationQuit] : Lost Focus, PlayCount: " + AppManager.Instance.VideoController.PlayCount);
            // if the playcount is more than 1, the video was played all the way through at least once
            if (AppManager.Instance.VideoController.PlayCount > 1)
            {
                //Debug.Log("[Handler_PauseAndQuit : OnApplicationQuit] : Lost Focus, Exiting video after at least one playthrough, PlayCount: " + AppManager.Instance.VideoController.PlayCount + "times");
                Analytics.EndVideo(false);
            }
            else
            {
                //Debug.Log("[Handler_PauseAndQuit : OnApplicationQuit] : Lost Focus, Exiting video early after playing for PlayCount: " + AppManager.Instance.VideoController.PlayCount + "times");
                Analytics.EndVideo(true);
            }
        }
        Analytics.EndSession();

        Debug.Log($"[Handler_PauseAndQuit : OnApplicationQuit] _#{AppManager.Instance.VideoController.PlayCount}");
    }
}
