using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vanilla;
using UnityEngine.UI;
using UnityEngine.Events;


public class IntEvent : UnityEvent<int>{

}

public class TimelineIntro : VanillaTimeline
{
    public UIManager UIManager;

    public VanillaTimelineMotor Motor;

    public IntEvent MilestoneEvent = new IntEvent();

    public override void Start()
    {
        base.Start();

        //Debug.Log("Test");
        Motor = this.GetComponent<VanillaTimelineMotor>();
        //Debug.Log(Motor);
    }

    public void StartMotorMoving()
    {
        MotorMoving(true);
    }

    public void MotorMoving(bool state)
    {
        Debug.Log("Motor set to: " + state);
        Motor.enabled = state;
    }

    public void ChangeChapter(int jumpTo)
    {
        MotorMoving(false);
        float timelineSeconds = 0;
        for (int i = 0; i < jumpTo; i++)
        {
            timelineSeconds += chapters[i].timeInSeconds;
            //Debug.LogFormat("chapter {0} {1} total: {2}", chapters[i].name, chapters[i].timeInSeconds, timelineSeconds);
        }
        currentChapter = jumpTo;
        //Debug.Log("currentChapter " + currentChapter);
        t = timelineSeconds;
        //Debug.Log("t " + t);
        timelineNormal = timelineSeconds / timelineLength;
        ////Debug.Log("timelineNormal " + timelineNormal);
        MotorMoving(true);
    }


    public override void MilestoneReached(int milestone)
    {
        //Debug.Log("============= "+ milestone + " reached ========================== ");
        switch (milestone)
        {
            case 1:

                break;

            case 2: 

                break;

            case 3: 

                break;

            case 4: //4. Fadeout loading after initial downloads
                
                //UIManager.skybox.gameObject.SetActive(true);
                //UIManager.AnalyticsUI.SetActive(false);

                UIManager.colorSkybox.LerpToTargetColor(0, 1, 6);
                //when D/Ls are done, fade out and move on jank free 
                UIManager.LoadingUI_inactive();

                if (!AppManager.Instance.RunningAppFirstTime()) //not the frist run through, show skip buttons
                {
                    UIManager.OnboardingUI_Skip_active();                   
                }
                break;

            case 5: //5. Onboarding / OHS message #1 active
                    //Make onboarding panel visible and 1st OHS message
                UIManager.OnboardingUI_Panel_1_active();
                break;

            case 6: //6.OHS message #1 inactive
                UIManager.OnboardingUI_Panel_1_inactive();
                break;

            case 7: //7. OHS message #2 active
                UIManager.OnboardingUI_Panel_2_active();
                break;

            case 8: //8. OHS message #2 inactive
                UIManager.OnboardingUI_Panel_2_inactive();
                break;

            case 9:  //9. FadeIn OHS Confirm
                UIManager.OnboardingUI_Skip_inactive();
                UIManager.OnboardingUI_Panel_3_active();

                break;

            case 10:  //10. OHS Confirmation active
                     
                //stop for button press/ gaze input
                MotorMoving(false);
                
                break;

            case 11:  //11. OHS Confirmation fadeOut and showbox colour lerp

                //OHS confoirmation inactive after click/gaze has started the motor again
                AppManager.Instance.AppFirstTime = false;
                PlayerPrefs.SetInt("AppFirstTime", 0);
                PlayerPrefs.Save();

                break;

            case 12: //12. Hide onboarding parent / Show Splash Parent
                     //UIManager.OnboardingUI_hidden();
                //Debug.Log("============= The gaze stuff ");
                if (!AppManager.Instance.RunningAppFirstTime()) //not the frist run through, hide onboarding UIs and skip buttons
                {
                    UIManager.ResetOnboardingUIs();
                }
                UIManager.SplashUI_visible();
                break;

            case 13: //13. FadeIn Splash Title
                UIManager.SplashUI_Primary_active();
                break;

            case 14: //14. FadeIn Splash Logo(s)
                UIManager.SplashUI_Secondary_active();
                break;

            case 15: //15. FadeOut Splash Parent
                //Fade out all splah related UIs: title, tag, logos - all at once
                UIManager.SplashUI_inactive();
                //UIManager.skybox.LerpToRandomColour();
                break;

            case 16: //16. Hide Splash / build navigation carousel
                //Hide Splash / build Carousel content / lerp bacground before menu fade in

                UIManager.SplashUI_hidden();

                break;

            case 17: //17. Init Carousel / Fade In Menu/Carousel
                     //Hide Splash / FadeIn Selection Carousel

                UIManager.MenuUI_active();

                AppManager.Instance.SetUpCarouselMenu();

                break;

            default:
                break;
        }

        MilestoneEvent.Invoke(milestone);
    }
}