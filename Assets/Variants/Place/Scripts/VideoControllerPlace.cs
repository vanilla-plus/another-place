using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using RenderHeads.Media.AVProVideo;


public class VideoControllerPlace : VideoController
{
     
    public void CloseKioskUI()
    {
        if (AppManager.Instance.KioskMode)
        {
            UIManager.PreExperienceUI_inactive();
        }
    }

    public override void VideoLoopPointReached(MediaPlayer videoPlayer)  //when video runs to the end
    {
        UIManager.Fader.FadeOut(0.01f);
        base.VideoLoopPointReached(videoPlayer);
        //if (AppManager.Instance.KioskMode)
        //{
            //mediaPlayer.Control.Rewind();
            GameObject mutedSlider = UIManager.VolumeControlsUI.GetComponent<VideoPlayerVolumeControl>().audioSourceSliders[0];
            mutedSlider.GetComponent<PHUI_Volume_Slider>().SetSliderValue(0f);
            audioTracks[0].mute = true;
            //AppManager.Instance.GetComponent<AppManagerPlace>().PreExperienceUI_JustRestart();
            //UIManager.PreExperienceUI_active();

            float jumpPoint;
            //if vid is longer than 3mins - take the jump point as the duration - 3mins (60000ms) 
            //otherwise, go back a 1/3 of the duration and use that to start loop
            if (mediaPlayer.Info.GetDurationMs() < 180000)
            {
                jumpPoint = mediaPlayer.Info.GetDurationMs();
                jumpPoint = jumpPoint / 3; // 1/3 of duration
                //Debug.Log("Jump Back point is 1/3 of " + (mediaPlayer.Info.GetDurationMs() / 1000) + " | "+ (jumpPoint / 1000));
            }
            else
            {
                jumpPoint = mediaPlayer.Info.GetDurationMs() - 180000f; //3mins
                //Debug.Log("Jump Back point is 3mins less of " + (mediaPlayer.Info.GetDurationMs() / 1000) + " | " + ((mediaPlayer.Info.GetDurationMs() / 1000) - (jumpPoint / 1000)));
            }
            mediaPlayer.Pause();
            mediaPlayer.Control.Seek(jumpPoint);
            audioTracks[0].time = (jumpPoint / 1000);
            PlayVideo();

        //}
        UIManager.Fader.FadeIn(3f,2f);
    }

    public override void PlayVideo()
    {
        base.PlayVideo();
        if (AppManager.Instance.KioskMode)
        {
            AppManager.Instance.GetComponent<AppManagerPlace>().PreExperienceUI_JustRestart();
            UIManager.PreExperienceUI_hidden();
        }
     
    }

    public override void PauseVideo()
    {
        //Debug.Log("PLace overide : UIManager.VideoPlayerStopUI.activeSelf " + UIManager.VideoPlayerStopUI.activeSelf);
        //Debug.Log("PLace overide : AppManager.Instance.KioskMode " + AppManager.Instance.KioskMode);
        base.PauseVideo();
        if (AppManager.Instance.KioskMode)
        { 
            AppManager.Instance.GetComponent<AppManagerPlace>().PreExperienceUI_JustRestart();
            UIManager.PreExperienceUI_active();
        }
    }

    public void ReturnToPreExperience()
    {
        AppManager.Instance.UIManager.colorSkybox.LerpToTargetColor(0, 1, 1);
        UIManager.ResetVideoPlayerUIs();
        UnloadVideo();
        mediaPlayer.Control.Rewind();
        //Debug.Log("I'm stopping because someone has requested to restart at the pre-experience step");
        AppManager.Instance.SkipMenuAndGoIntoPreExperience = true;
        AppManager.Instance.ChangeState(AppManager.AppStates.PreExperience);
    }

}
