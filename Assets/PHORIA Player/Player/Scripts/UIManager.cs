using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CurvedUI;

public class UIManager : MonoBehaviour
{
    //public GameObject AnalyticsUI;
    //public GameObject OnboardingUI;
    public GameObject OnboardingUI_Panel_1;
    public GameObject OnboardingUI_Panel_2;
    public GameObject OnboardingUI_Panel_3;
    public GameObject OnboardingUI_Skip;

    public GameObject LoadingUI;

    public GameObject SplashUI;
    public GameObject SplashUI_Primary;
    public GameObject SplashUI_Secondary;

    public GameObject MenuUI;

    public GameObject VideoPlayerUI;
    public GameObject VideoPlayerControlsUI;
    public GameObject VolumeControlsUI;
    public GameObject VideoPlayerRestartUI;
    public GameObject VideoPlayerStopUI;
    public GameObject VoiceObjectUI;

    public GameObject PreExperienceUI;
    
    public CurvedUISettings curvedUI;

    public GameObject Floor;

    public EventSystem EventSystem;
    
    public GameObject FocusHandler;
    public GameObject EnvironmentParticles;
    public EffectSkybox skybox;

    public GameObject GazeTimerObject;
    public GazeTimerEvents GazeTimerParticles;
    public GameObject ArtAndEffects;
    //public static UIManager Instance;

    public List<IEnumerator> HideCouritines = new List<IEnumerator>();
    public int hideIndex;

    public EffectSkybox colorSkybox;

    public void ResetUIs()
    {
        //Switch these to all talk to the same type of default class that handles this, or perhaps just animator
        //OnboardingUI_hidden();
        if (OnboardingUI_Skip.activeInHierarchy) OnboardingUI_Skip_hidden();
        if (OnboardingUI_Panel_1) OnboardingUI_Panel_1_hidden();
        if (OnboardingUI_Panel_2) OnboardingUI_Panel_2_hidden();
        if (OnboardingUI_Panel_3) OnboardingUI_Panel_3_hidden();
        if (LoadingUI.activeInHierarchy) LoadingUI_hidden();
        if (SplashUI.activeInHierarchy) SplashUI_hidden();
        if (SplashUI_Primary.activeInHierarchy) SplashUI_Primary_hidden();
        if (SplashUI_Secondary.activeInHierarchy) SplashUI_Secondary_hidden();
        if (MenuUI.activeInHierarchy) MenuUI_hidden();
        if (VideoPlayerUI.activeInHierarchy) VideoPlayerUI_hidden();
        if (VideoPlayerControlsUI.activeInHierarchy) VideoPlayerControlsUI_hidden();
        if (VolumeControlsUI.activeInHierarchy) VolumeControlUI_hidden();
        if (VideoPlayerRestartUI.activeInHierarchy) VideoPlayerRestartUI_hidden();
        if (VideoPlayerStopUI.activeInHierarchy) VideoPlayerStopUI_hidden();
        if (VoiceObjectUI.activeInHierarchy) VoiceObjectUI_hidden();
        if (PreExperienceUI.activeInHierarchy) PreExperienceUI_hidden();

    }

    public void ResetVideoPlayerUIs()
    {
        if (VideoPlayerUI.activeInHierarchy) VideoPlayerUI_hidden();
        if (VideoPlayerControlsUI.activeInHierarchy) VideoPlayerControlsUI_hidden();
        if (VolumeControlsUI.activeInHierarchy) VolumeControlUI_hidden();
        if (VideoPlayerRestartUI.activeInHierarchy) VideoPlayerRestartUI_hidden();
        if (VideoPlayerStopUI.activeInHierarchy) VideoPlayerStopUI_hidden();
    }

    public void ResetOnboardingUIs()
    {
        if (OnboardingUI_Skip.activeInHierarchy) OnboardingUI_Skip_inactive();
        if (OnboardingUI_Panel_1.activeInHierarchy) OnboardingUI_Panel_1_hidden();
        if (OnboardingUI_Panel_2.activeInHierarchy) OnboardingUI_Panel_2_hidden();
        if (OnboardingUI_Panel_3.activeInHierarchy) OnboardingUI_Panel_3_hidden();
    }


    /*
    ============================================================================================================
    Screen Fading code
    ============================================================================================================
    */
    public PHUI_Fader Fader;

    public void FadeIn(float fadeTime = -1)
    {
        Fader.FadeIn(fadeTime);
    }

    public void FadeOut(float fadeTime = -1)
    {
        Fader.FadeOut(fadeTime);
    }

    /*
    =========================================================================================================
    Onboarding UI code   
    =========================================================================================================
    */
    //public void OnboardingUI_visible()
    //{
    //    PanelUI_SetAnimation(OnboardingUI, "visible");
    //}
    //public void OnboardingUI_hidden()
    //{
    //    PanelUI_SetAnimation(OnboardingUI, "hidden");
    //}
    //public void OnboardingUI_active()
    //{
    //    PanelUI_SetAnimation(OnboardingUI, "active");
    //}
    //public void OnboardingUI_inactive()
    //{
    //    PanelUI_SetAnimation(OnboardingUI, "inactive");
    //}

    public void OnboardingUI_Skip_visible()
    {
        PanelUI_SetAnimation(OnboardingUI_Skip, "visible");
    }
    public void OnboardingUI_Skip_hidden()
    {
        PanelUI_SetAnimation(OnboardingUI_Skip, "hidden");
    }
    public void OnboardingUI_Skip_active()
    {
        PanelUI_SetAnimation(OnboardingUI_Skip, "active");
    }
    public void OnboardingUI_Skip_inactive()
    {
        PanelUI_SetAnimation(OnboardingUI_Skip, "inactive");
    }

    public void OnboardingUI_Panel_1_visible()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_1,  "visible");
    }
    public void OnboardingUI_Panel_1_hidden()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_1, "hidden");
    }
    public void OnboardingUI_Panel_1_active()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_1, "active");
    }
    public void OnboardingUI_Panel_1_inactive()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_1, "inactive");
    }


    public void OnboardingUI_Panel_2_visible()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_2, "visible");
    }
    public void OnboardingUI_Panel_2_hidden()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_2, "hidden");
    }
    public void OnboardingUI_Panel_2_active()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_2, "active");
    }
    public void OnboardingUI_Panel_2_inactive()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_2, "inactive");
    }


    public void OnboardingUI_Panel_3_visible()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_3, "visible");
    }
    public void OnboardingUI_Panel_3_hidden()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_3, "hidden");
    }
    public void OnboardingUI_Panel_3_active()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_3, "active");
    }
    public void OnboardingUI_Panel_3_inactive()
    {
        PanelUI_SetAnimation(OnboardingUI_Panel_3, "inactive");
    }
      /* ========================================================================================================= */

    /*
    =========================================================================================================
    Loading UI code   
    =========================================================================================================
    */
    public void LoadingUI_visible()
    {
        PanelUI_SetAnimation(LoadingUI, "visible");
    }
    public void LoadingUI_hidden()
    {
        PanelUI_SetAnimation(LoadingUI, "hidden");
    }
    public void LoadingUI_active()
    {
        PanelUI_SetAnimation(LoadingUI, "active");
    }
    public void LoadingUI_inactive()
    {
        PanelUI_SetAnimation(LoadingUI, "inactive");
    }

    /* ========================================================================================================= */

    /*
    =========================================================================================================
    Splash UI code   
    =========================================================================================================
    */
    public void SplashUI_visible()
    {
        PanelUI_SetAnimation(SplashUI, "visible");
    }
    public void SplashUI_hidden()
    {
        PanelUI_SetAnimation(SplashUI, "hidden");
    }
    public void SplashUI_active()
    {
        PanelUI_SetAnimation(SplashUI, "active");
    }
    public void SplashUI_inactive()
    {
        PanelUI_SetAnimation(SplashUI, "inactive");
    }


    public void SplashUI_Primary_visible()
    {
        PanelUI_SetAnimation(SplashUI_Primary, "visible");
    }
    public void SplashUI_Primary_hidden()
    {
        PanelUI_SetAnimation(SplashUI_Primary, "hidden");
    }
    public void SplashUI_Primary_active()
    {
        PanelUI_SetAnimation(SplashUI_Primary, "active");
    }
    public void SplashUI_Primary_inactive()
    {
        PanelUI_SetAnimation(SplashUI_Primary, "inactive");
    }


    public void SplashUI_Secondary_visible()
    {
        PanelUI_SetAnimation(SplashUI_Secondary, "visible");
    }
    public void SplashUI_Secondary_hidden()
    {
        PanelUI_SetAnimation(SplashUI_Secondary, "hidden");
    }
    public void SplashUI_Secondary_active()
    {
        PanelUI_SetAnimation(SplashUI_Secondary, "active");
    }
    public void SplashUI_Secondary_inactive()
    {
        PanelUI_SetAnimation(SplashUI_Secondary, "inactive");
    }
    /* ========================================================================================================= */


    /*
     =========================================================================================================
     Navigation UI code   
     =========================================================================================================
     */
    public void MenuUI_visible()
    {
        PanelUI_SetAnimation(MenuUI, "visible");
    }
    public void MenuUI_hidden()
    {
        PanelUI_SetAnimation(MenuUI, "hidden");
    }
    public void MenuUI_active()
    {
        PanelUI_SetAnimation(MenuUI, "active");
    }
    public void MenuUI_inactive()
    {
        PanelUI_SetAnimation(MenuUI, "inactive");
    }
    /* ========================================================================================================= */


    /*
    =========================================================================================================
    Video Player UI code   
    =========================================================================================================
    */
    public void VideoPlayerUI_visible()
    {
        PanelUI_SetAnimation(VideoPlayerUI, "visible");
    }
    public void VideoPlayerUI_hidden()
    {
        PanelUI_SetAnimation(VideoPlayerUI, "hidden");
    }
    public void VideoPlayerUI_active()
    {
        PanelUI_SetAnimation(VideoPlayerUI, "active");
    }
    public void VideoPlayerUI_inactive()
    {
        PanelUI_SetAnimation(VideoPlayerUI, "inactive");
    }
    /* ========================================================================================================= */


    /*
    =========================================================================================================
    Video Player Controls UI code   
    =========================================================================================================
    */
    public void VideoPlayerControlsUI_visible()
    { 
        PanelUI_SetAnimation(VideoPlayerControlsUI, "visible");
    }
    public void VideoPlayerControlsUI_hidden()
    {
        PanelUI_SetAnimation(VideoPlayerControlsUI, "hidden");
    }
    public void VideoPlayerControlsUI_active()
    {
        PanelUI_SetAnimation(VideoPlayerControlsUI, "active");
    }
    public void VideoPlayerControlsUI_inactive()
    {
        PanelUI_SetAnimation(VideoPlayerControlsUI, "inactive");
    }


    /* ========================================================================================================= */

    /*
    =========================================================================================================
    Volume Control UI code   
    =========================================================================================================
    */
    public void VolumeControlUI_visible()
    {
        PanelUI_SetAnimation(VolumeControlsUI, "visible");
    }
    public void VolumeControlUI_hidden()
    {
        PanelUI_SetAnimation(VolumeControlsUI, "hidden");
    }
    public void VolumeControlUI_active()
    {
        PanelUI_SetAnimation(VolumeControlsUI, "active");
    }
    public void VolumeControlUI_inactive()
    {
        PanelUI_SetAnimation(VolumeControlsUI, "inactive");
    }

    /* ========================================================================================================= */

    /*
    =========================================================================================================
    Voice Object UI code   
    =========================================================================================================
    */
    public void VoiceObjectUI_visible()
    {
        PanelUI_SetAnimation(VoiceObjectUI, "visible");
    }
    public void VoiceObjectUI_hidden()
    {
        PanelUI_SetAnimation(VoiceObjectUI, "hidden");
    }
    public void VoiceObjectUI_active()
    {
        PanelUI_SetAnimation(VoiceObjectUI, "active");
    }
    public void VoiceObjectUI_inactive()
    {
        PanelUI_SetAnimation(VoiceObjectUI, "inactive");
    }


    /* ========================================================================================================= */

    /*
    =========================================================================================================
    Video Player Restart UI code   
    =========================================================================================================
    */
    public void VideoPlayerRestartUI_visible()
    {
        PanelUI_SetAnimation(VideoPlayerRestartUI, "visible");
    }
    public void VideoPlayerRestartUI_hidden()
    {
        PanelUI_SetAnimation(VideoPlayerRestartUI, "hidden");
    }
    public void VideoPlayerRestartUI_active()
    {
        PanelUI_SetAnimation(VideoPlayerRestartUI, "active");
    }
    public void VideoPlayerRestartUI_inactive()
    {
        PanelUI_SetAnimation(VideoPlayerRestartUI, "inactive");
    }
    /* ========================================================================================================= */

    /*
    =========================================================================================================
    Video Player Stop UI code   
    =========================================================================================================
    */
    public void VideoPlayerStopUI_visible()
    {
        PanelUI_SetAnimation(VideoPlayerStopUI, "visible");
    }
    public void VideoPlayerStopUI_hidden()
    {
        PanelUI_SetAnimation(VideoPlayerStopUI, "hidden");
    }
    public void VideoPlayerStopUI_active()
    {
        PanelUI_SetAnimation(VideoPlayerStopUI, "active");
    }
    public void VideoPlayerStopUI_inactive()
    {
        PanelUI_SetAnimation(VideoPlayerStopUI, "inactive");
    }
    /* ========================================================================================================= */

    /*
    =========================================================================================================
    PreExperience UI code   
    =========================================================================================================
    */
    public void PreExperienceUI_visible()
    {
        PanelUI_SetAnimation(PreExperienceUI, "visible");
    }
    public void PreExperienceUI_hidden()
    {
        PanelUI_SetAnimation(PreExperienceUI, "hidden");
    }
    public void PreExperienceUI_active()
    {
        PanelUI_SetAnimation(PreExperienceUI, "active");
    }
    public void PreExperienceUI_inactive()
    {
        PanelUI_SetAnimation(PreExperienceUI, "inactive");
    }


    /* ========================================================================================================= */

    /*
    =========================================================================================================
    Floor code   
    =========================================================================================================
    */
    public void Floor_active()
    {
        Floor.GetComponent<Animator>().SetTrigger("active");
    }
    public void Floor_inactive()
    {
        Floor.GetComponent<Animator>().SetTrigger("inactive");
    }
    /* ========================================================================================================= */


    /*
    =========================================================================================================
    Panel animation code   
    =========================================================================================================
    */


    public void PanelUI_SetAnimation(GameObject parentPanel, string trigger)
    {
        //Debug.Log("Animating [" + trigger + "] for Panel " + parentPanel.name);

        //Get the poarent GO and find the element with the animation component 


        //could the object know it's own child and animator - get child/component etc can be a bit heavy
        GameObject childUI = parentPanel.transform.GetChild(0).gameObject;

        Animator thisAnimator = childUI.GetComponent<Animator>();

        //Debug.Log("Targeting animator " + childUI.name);

        parentPanel.SetActive(true);
        switch (trigger)
        {
            case "visible":
                thisAnimator.SetTrigger(trigger);
                break;

            case "hidden":
                thisAnimator.SetTrigger(trigger);
                parentPanel.SetActive(false);
                break;

            case "active":
                thisAnimator.SetTrigger(trigger);
                break;

            case "inactive":
                thisAnimator.SetTrigger(trigger);
                IEnumerator newHide = HideAfterTime(parentPanel);
                HideCouritines.Add(newHide);
                StartCoroutine(HideCouritines[hideIndex]);
                hideIndex++;
                break;
        }
        //EventSystem.SetSelectedGameObject(parentPanel);
    }

   
    //wait for animation time to end and say when its done with a bool
    public IEnumerator HideAfterTime(GameObject toHide)
    {
        //Debug.Log("A timer has begun to close " + toHide);
        // Waits "time" seconds
        yield return new WaitForSeconds(0.5f);
        toHide.SetActive(false);
        //Debug.Log("Timer complete, turning off " + toHide);
    }

}
