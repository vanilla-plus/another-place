using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoPlayerRestartControl : MonoBehaviour
{

    //public VideoPlayer VideoPlayerComponent; //the video component to use
    public TextMeshProUGUI DescriptionMessage;
    public string RestartMessage;
    public VideoController VideoPlayerController;
    
    public void InitialiseText(string videoTitle)
    {        
        DescriptionMessage.text = RestartMessage.Replace("{VIDEO_TITLE}", videoTitle);
    }
}


