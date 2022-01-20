using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using RenderHeads.Media.AVProVideo;


public class VideoController : MonoBehaviour
{
    public string VideoTitle; //expereince title for various UIs
    public string ExperiencePath; //expereince file path
    public string VideoGuideGender; //guide gender
    public string VideoGuidanceLevel; //difficulty of meditation
    
    private VideoPlayer VideoPlayerComponent; //the video component to use

    public MediaPlayer mediaPlayer;

    public Material VideoSkyboxMaterial; //the video skybox render texture material

    public UIManager UIManager;

    public List<AudioSource> audioTracks = new List<AudioSource>();


    bool isDone; //completed flag

    public bool VideoReady;

    public bool WaitingForReady;

    IEnumerator WaitCoroutine;

    public enum ContentStates
    {
        Preparing,
        Playing,
        Paused,
        Completed
    }
    private ContentStates currentAppState;

    public int PlayCount=0;

    [SerializeField]
    private GameObject PlayerGazeArea;


    /* **************************************************************************************************************** */
    //Video properties

    public bool IsPlaying
    {
        get { return mediaPlayer.Control.IsPlaying(); }
    }
    public bool IsPaused
    {
        get { return mediaPlayer.Control.IsPaused(); }
    }
    public bool IsPrepared
    {
        get { return mediaPlayer.Control.CanPlay(); }
    }
    public bool IsDone
    {
        get { return isDone; }
    }
    public double CurrentTime
    {
        get {
            float currentTime = mediaPlayer.Control.GetCurrentTimeMs();
            currentTime /= 1000;
            return currentTime; }
    }
    public double /*ulong*/ Duration
    {
        get { return (float)mediaPlayer.Info.GetDurationMs() / 1000; } //(ulong)(VideoPlayerComponent.frameCount / VideoPlayerComponent.frameRate); }
    }
    public double NormalTime
    {
        get { return CurrentTime / Duration; }
    }




       // Callback function to handle events
       public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:


                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                //Debug.Log("First frame ready");
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                //Debug.Log("VideoController : VideoLoopPointReached : Loop Point has been reached... " + IsPlaying);
                VideoLoopPointReached(mediaPlayer);
                break;
        }
        //Debug.Log("Event: " + et.ToString());
    }

    //Video Player Events
    private void OnEnable()
    {
        //VideoPlayerComponent = this.GetComponent<VideoPlayer>();
        ////VideoPlayerComponent.loopPointReached += VideoLoopPointReached; //when video runs to the end
        //VideoPlayerComponent.prepareCompleted += VideoPrepareCompleted; //when video is loaded
        //VideoPlayerComponent.seekCompleted += VideoSeekCompleted;       //when the position of the video has changed via input
        //VideoPlayerComponent.started += VideoStarted;                   //video started 
        //VideoPlayerComponent.errorReceived += VideoErrorReceived;       //for troubleshooting/getting errors
        mediaPlayer.Events.AddListener(OnVideoEvent);

    }

    private void OnDisable()
    {
        ////VideoPlayerComponent.loopPointReached -= VideoLoopPointReached;
        //VideoPlayerComponent.prepareCompleted -= VideoPrepareCompleted;
        //VideoPlayerComponent.seekCompleted -= VideoSeekCompleted;
        //VideoPlayerComponent.started -= VideoStarted;
        //VideoPlayerComponent.errorReceived -= VideoErrorReceived;
        mediaPlayer.Events.RemoveListener(OnVideoEvent);
    }

    /* **************************************************************************************************************** */


    //### these should be changing the state of the videoplayer

    void VideoErrorReceived(VideoPlayer videoPlayer, string message) //for troubleshooting/getting errors
    {
        //Debug.Log("VideoController : VideoErrorReceived : ERROR MESSAGE : " + message);
    }
    
    public virtual void VideoLoopPointReached(MediaPlayer videoPlayer)  //when video runs to the end
    {
        mediaPlayer.Pause();
        UIManager.ResetVideoPlayerUIs();
        UIManager.VideoPlayerUI_visible();

        foreach (AudioSource audio in audioTracks)
        {
            audio.Pause();
        }

        UIManager.VideoPlayerRestartUI_active();

        isDone = true;
        PlayCount++;
    }
    
    void VideoPrepareCompleted(VideoPlayer videoPlayer) //when video is loaded
    {
        //Debug.Log("VideoController : VideoPrepareCompleted : Video prep has been completed.");
        //PlayVideo();
        SetupSkyboxEnvironment(VideoSkyboxMaterial);
        isDone = false;
        VideoReady = true;
    }
    
    void VideoSeekCompleted(VideoPlayer videoPlayer) //when the position of the video has changed via input
    {
        //Debug.Log("VideoController : VideoSeekCompleted : Seeking completed.");
        isDone = false;
    }
    
    void VideoStarted(VideoPlayer videoPlayer) //video started 
    {
        //Debug.Log("VideoController : VideoStarted : Video has started playing.");
    }
    /* **************************************************************************************************************** */


    //Set Video Environment when ready to play
    // ## maybe this should be handled somewhere 

    public void SetupSkyboxEnvironment(Material videoSkyboxMaterial)
    {
        if (videoSkyboxMaterial != null)
        {
            RenderSettings.skybox = videoSkyboxMaterial;
        }
        else
        {
            //Debug.LogError("VideoController : SetupSkyboxEnvironment : No Video Skybox Material provided.");
        }
    }

    //##this whole cycle for play video etc pretty circuitous, actually all good
    //Much of the window dressing should go in the media/play state manager

    //Play video    ## THIS IS ESSENTIALLY SETUP

    public void PlayVideoHandler()
    {
        if (WaitingForReady) StopCoroutine(WaitCoroutine);

        WaitCoroutine = WaitForPrepared();
        StartCoroutine(WaitCoroutine);
    }

    IEnumerator WaitForPrepared()
    {

        AppManager.Instance.ShowLoading();
        WaitingForReady = true;
        while (!VideoReady)
        {
            yield return null;
        }
        PlayVideo();
        AppManager.Instance.HideLoading();
        WaitingForReady = false;

    }

    void TurnOffArt()
    {
        UIManager.ArtAndEffects.gameObject.SetActive(false);
    }

    public virtual void PlayVideo()
    {
        //if (!IsPrepared) return;
        //Debug.LogFormat("PlayVideo 1 : isplaying? {0}  |  isPaused {1}", mediaPlayer.Control.IsPlaying(),mediaPlayer.Control.IsPaused());
        mediaPlayer.Play();
        //Log analytics - video end, taking into account for stopping early / not watching the full thing
        if (AppManagerPlace.Instance.UseAnalytics)
        {
            // if the playcount is more than 1, the video was played all the way through at least once
            if (PlayCount == 0)
                Analytics.StartVideo(VideoTitle, VideoGuidanceLevel, VideoGuideGender);
        }

        //setup gaze audio option first time around
        if (PlayCount == 0)
        {
            PlayCount++;
            Invoke("TurnOffArt",5f);
        }

        //Debug.Log("PlayVideo : PlayCount is now: " + PlayCount);
        //Debug.Log("===== PlayVideo : Status of video  " + mediaPlayer.Control.GetCurrentTimeMs() + " ====================================");
        //Debug.LogFormat("PlayVideo 2 : isplaying? {0}  |  isPaused {1}", mediaPlayer.Control.IsPlaying(), mediaPlayer.Control.IsPaused());

        int audioID = 0;
        //Debug.Log("PlayVideo how many audio tracks? " + audioTracks.Count);
        foreach (AudioSource audio in audioTracks)
        {

            audio.Play();
            audioID++;

        }

    }

    //Pause Video ##These are essentially commands/support for commands
    public virtual void PauseVideo()
    {
        if (!IsPlaying) return;

        //Debug.LogFormat("PauseVideo 1 : isplaying? {0}  |  isPaused {1}", mediaPlayer.Control.IsPlaying(), mediaPlayer.Control.IsPaused());
        mediaPlayer.Pause();
        //Debug.Log("===== PauseVideo : Status of video " + mediaPlayer.Control.GetCurrentTimeMs() + " ====================================");
        //Debug.LogFormat("PauseVideo 2 : isplaying? {0}  |  isPaused {1}", mediaPlayer.Control.IsPlaying(), mediaPlayer.Control.IsPaused());

        foreach (AudioSource audio in audioTracks)
        {
            audio.Pause();
        }
    }

    //Stop video
    public void UnloadVideo()
    {
        UIManager.ArtAndEffects.gameObject.SetActive(true);
        AppManager.Instance.UIManager.colorSkybox.LerpToTargetColor(0, 1, 2);
        //Debug.LogFormat("UnloadVideo 1 : isplaying? {0}  |  isPaused {1}", mediaPlayer.Control.IsPlaying(), mediaPlayer.Control.IsPaused());
        mediaPlayer.Stop();
        mediaPlayer.m_VideoPath = "";
        //Debug.Log("===== UnloadVideo : Status of video " + mediaPlayer.Control.GetCurrentTimeMs() + " ====================================");
        //Debug.LogFormat("UnloadVideo 2 : isplaying? {0}  |  isPaused {1}", mediaPlayer.Control.IsPlaying(), mediaPlayer.Control.IsPaused());
        //VideoPlayerComponent.url = "";
        foreach (AudioSource audio in audioTracks)
        {
            audio.Stop();
        }
    }

    public void ReturnHome()
    {
        UnloadVideo();
        mediaPlayer.Control.CloseVideo();
        ClearAudioTracks();
        //Debug.Log("I'm stopping because someone has requested to return home");

        //Log analytics - video end, taking into account for stopping early / not watching the full thing
        if (AppManagerPlace.Instance.UseAnalytics)
        {
            Debug.Log("ReturnHome #1 : PlayCount is now: " + PlayCount);
            // if the playcount is more than 1, the video was played all the way through at least once
            if (PlayCount > 1)
            {
                Debug.Log("ReturnHome : Exiting video after playing for: " + PlayCount + "times");
                Analytics.EndVideo(false, true, false);
            }
            else
            {
                Debug.Log("ReturnHome : Exiting video early! played for: " + PlayCount + "times");
                Analytics.EndVideo(true, true, false);
            }
        }
        PlayCount = 0;
        //Analytics.currentVideo = null;
        //PlayerGazeArea.SetActive(false);
        AppManager.Instance.ChangeState(AppManager.AppStates.Menu);
    }


    



//Replay Video, reste frame/play
    public void RestartVideo()
    {
        if (!IsPrepared) return;
        AppManager.Instance.ShowLoading();
        PauseVideo();
        
        foreach (AudioSource audio in audioTracks)
        {
            if (audio.mute == true) 
            //for when the guidance has been muted if the user wants to extend the practice longer after a first time run through 
            {
                audio.mute = false;
            }
			audio.time = 0f;
            audio.Stop();
        }
        mediaPlayer.Control.Seek(0);
        PlayVideo();
        AppManager.Instance.HideLoading();
        AppManager.Instance.UIManager.colorSkybox.LerpToTargetColor(2, 3, 3);
        AppManager.Instance.UIManager.colorSkybox.LerpFinished.AddListener(AppManager.Instance.TurnOnSphere);

    }

    //set the position of the video
    public void SeekVideo(float videoPosition)
    {
        //if (!VideoPlayerComponent.canSetTime) return; //some vid types won't let you change position LAME
        //if (!IsPrepared) return;
        //videoPosition = Mathf.Clamp(videoPosition, 0, 1);
        //VideoPlayerComponent.time = videoPosition * Duration;
    }

    //Unload, reset component to start (to restart vid)


    //set video volume
    public void SetVideoVolume(int trackID, float volumeLevel)
    {
        if (audioTracks[trackID].mute == true) audioTracks[trackID].mute = false;
        audioTracks[trackID].volume = volumeLevel;

        //VideoPlayerComponent.SetDirectAudioVolume(0, volumeLevel);
    }

    public void SetPlayerVolume(float volumeLevel)
    {

        mediaPlayer.Control.SetVolume(volumeLevel);
    }

    //Get video volume
    public float GetVideoVolume(int trackID)
    {
        return audioTracks[trackID].volume;
        //return VideoPlayerComponent.GetDirectAudioVolume(0);
    }

    //Get vid title for use in the player UIs
    public void SetVideoTitle(string videoTitle)
    {
        VideoTitle = videoTitle;
    }
    //Get vid title for use in the player UIs
    public string GetVideoTitle()
    {
        return VideoTitle;
    }

    //Load Video local URL - ####This could go into the much vaunted media manager 
    public void LoadVideoURLFromPath(string path)
    {
        //string hasLoadedPathAlready = path; //has it been loaded already? No point loading it again it's already in memory
        //if (VideoPlayerComponent.url == hasLoadedPathAlready) return;

        if (File.Exists(path))
        {
            //Debug.Log("VideoController : SetVideoURLFromPath : Found. New video path set to " + path);
            VideoPlayerComponent.url = path;
            VideoPlayerComponent.Prepare();
        }
        else
        {
            //Debug.Log("VideoController : SetVideoURLFromPath : No video found at " +path);
        }
    }

    public void ClearAudioTracks()
    {
        //Debug.Log("I'm supposed clearing audio sources from video");
        //Debug.Log("Audio Track adding hunt stage 3 -  Something has told me to clear the audio tracks");

        AppManager.Instance.volumeControlHandler.ClearAudioTrackSliders();

        Destroy(AppManager.Instance.onboarding);
        int trackClearIndex = 0;

        foreach (AudioSource audioTrack in audioTracks)
        {
            //Debug.Log("Destroying index trackClearIndex: "+ trackClearIndex+" and track: " + audioTrack.name);
            Destroy(audioTrack.GetComponent<AudioSource>());
            Destroy(audioTrack);

            trackClearIndex++;

        }
        if (audioTracks.Count> 0) audioTracks.RemoveRange(1, trackClearIndex-1);
        //Debug.Log("At the end of this process i had a track clear index of " + trackClearIndex);


        audioTracks.Clear();

    }

}
