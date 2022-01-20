using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsUseExample : MonoBehaviour
{
    #region UI
    public InputField InpUserID;
    public InputField InpVideoName;
    public InputField InpVideoDifficulty;
    public InputField InpVideoGender;

    public Button BtnStartSession;
    public Button BtnStartVideo;
    public Button BtnStopVideo;
    public Button BtnInteruptVideo;
    #endregion

    public void StartSession()
    {
        //----- MAKE SURE YOU HAVE AN OBJECT IN YOUR SCENE WITH THE AWS UNITY INITIALIZER ATTACHED PRIOR TO CALLING THIS ----- EXAMPLE CODE BELOW -----//
        //UnityInitializer.AttachToGameObject(gameObject);

        //----- MAKE SURE YOU HAVE CONFIGURED AWS' HTTP CLIENT TO UNITY WEB REQUEST PRIOR TO CALLING THIS ----- EXAMPLE CODE BELOW -----//
        //AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        Analytics.StartSession(InpUserID.text);

        #region UI
        BtnStartSession.gameObject.SetActive(false);
        BtnStartVideo.gameObject.SetActive(true);
        BtnStopVideo.gameObject.SetActive(false);
        BtnInteruptVideo.gameObject.SetActive(false);
        #endregion
    }

    public void StartVideo()
    {
        Analytics.StartVideo(InpVideoName.text, InpVideoDifficulty.text, InpVideoGender.text);

        #region UI
        BtnStartSession.gameObject.SetActive(false);
        BtnStartVideo.gameObject.SetActive(false);
        BtnStopVideo.gameObject.SetActive(true);
        BtnInteruptVideo.gameObject.SetActive(true);
        #endregion
    }

    public void StopVideo(bool interupted)
    {
        Analytics.EndVideo(interupted);

        #region UI
        BtnStartSession.gameObject.SetActive(false);
        BtnStartVideo.gameObject.SetActive(true);
        BtnStopVideo.gameObject.SetActive(false);
        BtnInteruptVideo.gameObject.SetActive(false);
        #endregion
    }

    private void OnApplicationQuit()
    {
        Analytics.EndSession();
    }
}
