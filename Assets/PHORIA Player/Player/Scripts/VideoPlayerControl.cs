using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoPlayerControl : MonoBehaviour
{
    //public VideoPlayer VideoPlayerComponent; //the video component to use
    public TextMeshProUGUI StatusMessage;
    public TextMeshProUGUI DescriptionMessage;
    public PHUI_Progress_Bar VideoProgressBar;
    public GameObject PlayPauseButton;

    private bool PlayPauseButtonState;
    public VideoController VideoPlayerController;

    public bool ShowDurationTiming;
    private float videoDuration;
    private float videoPosition;
    private string currentPosition;
    private string duration;

    // Start is called before the first frame update
    void Start()
    {
        //VideoPlayerController = VideoPlayerComponent.GetComponent<VideoController>();
        PlayPauseButtonState = false;
        InitPlayer();
    }

    public void InitPlayer()
    {
        //Debug.Log("InitPlayer ENABLED!");
        DescriptionMessage.text = VideoPlayerController.VideoTitle;
        PlayPauseButtonIconToggle();
        videoDuration = Mathf.Floor((float)VideoPlayerController.Duration);
    }

    public void PlayPauseButtonToggle()
    {
        if (VideoPlayerController.IsPlaying)
        {
            VideoPlayerController.PauseVideo();
        }
        else if (VideoPlayerController.IsPaused)
        {
            VideoPlayerController.PlayVideo();
        }
        PlayPauseButtonIconToggle();
    }

    public void PlayPauseButtonIconToggle()
    {
        if (VideoPlayerController.IsPlaying)
        {
            PlayPauseButton.GetComponent<PHUI_Button_wIconToggle>().ToggleButtonState(0);
            StatusMessage.text = "Now playing";
        }
        else if (VideoPlayerController.IsPaused)
        {
            PlayPauseButton.GetComponent<PHUI_Button_wIconToggle>().ToggleButtonState(1);
            StatusMessage.text = "Paused, press play to continue";
        }
    }

    public void StopVideoButton()
    {
        //it's actullay pausing - -stoping playback will unload the video so we should only do this from the "Are you sure" UI
        //see the onclick event in the editor for the UI to loadin
        VideoPlayerController.PauseVideo();
        PlayPauseButton.GetComponent<PHUI_Button_wIconToggle>().ToggleButtonState(1);
    }

    string GetTimingText()
    {
        videoPosition = Mathf.Floor((float)VideoPlayerController.CurrentTime);
        
        //Debug.Log(videoPosition + " | " + videoDuration);
        //string currentPosition = Mathf.Floor(videoPosition / 3600).ToString("0") + ":"; //no hours
        currentPosition = Mathf.Floor(videoPosition / 60).ToString("00") + ":";
        currentPosition += Mathf.Floor(videoPosition % 60).ToString("00");

        //string duration = Mathf.Floor(videoDuration / 3600).ToString("0") + ":"; //no hours
        duration = Mathf.Floor(videoDuration / 60).ToString("00") + ":";
        duration += Mathf.Floor(videoDuration % 60).ToString("00");

        string output = "";

        if (ShowDurationTiming)
        {
            output = currentPosition + " / " + duration;
        }
        else
        {
            output = currentPosition;
        }

        return output;
    }
    float GetTimingProgress()
    {
        //Debug.Log((float)VideoPlayerController.NormalTime);
        return (float)VideoPlayerController.NormalTime;
    }


    public void UpdateVideoProgressUI()
    {
        VideoProgressBar.SetProgressText(GetTimingText());
        VideoProgressBar.SetProgressValue(GetTimingProgress());
    }

    void OnEnable()
    {
        InitPlayer();
        PlayPauseButtonIconToggle();
        justOneUpdate = true;

    }

    // Update is called once per frame
    private float time = 0.0f;
    private bool justOneUpdate = true;

    void Update()
    {
        if (VideoPlayerController.IsPrepared && (VideoPlayerController.IsPlaying || VideoPlayerController.IsPaused))
        {
            time += Time.deltaTime;
 
            if (time >= 0.5f) {
                time = 0.0f;
                if (VideoPlayerController.IsPlaying)
                {
                    UpdateVideoProgressUI();
                }
                else if (VideoPlayerController.IsPaused)
                {
                    if (justOneUpdate)
                    {
                        UpdateVideoProgressUI();
                        justOneUpdate = false;
                    }

                }
            }
        }
    }
}