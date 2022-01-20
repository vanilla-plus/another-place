﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManagerPlace : AppManager
{

    [Header("=== Kiosk Mode Options ==================================")]
    public GameObject PreExperienceSkipButton;
    public GameObject PreExperienceKioskRestartButton;
    public GameObject PreExperienceGoBackButton;

    
    //setup pre experience button combos before showing the UI
    void PreExperienceUI_JustGoback()
    {
        PreExperienceSkipButton.SetActive(false);
        PreExperienceKioskRestartButton.SetActive(false);
        PreExperienceGoBackButton.SetActive(true);
    }
    void PreExperienceUI_JustSkipGoBack()
    {
        PreExperienceSkipButton.SetActive(true);
        PreExperienceKioskRestartButton.SetActive(false);
        PreExperienceGoBackButton.SetActive(true);
    }
    public void PreExperienceUI_JustRestart()
    {
        PreExperienceSkipButton.SetActive(false);
        PreExperienceKioskRestartButton.SetActive(true);
        PreExperienceGoBackButton.SetActive(false);
    }


    public void GazeGateToOnboarding()
    {
        //Debug.Log("running GazeGateToOnboarding");

        PreExperienceUI_JustGoback();
        UIManager.PreExperienceUI_active();

        UIManager.OnboardingUI_Panel_3_visible();
        UIManager.FocusHandler.SetActive(true);
        UIManager.GazeTimerParticles.GazeFinished.AddListener(StartOnboarding);
    }

    void GazeGateToExperience()
    {
        //Debug.Log("running GazeGateToExperience");
        UIManager.FocusHandler.SetActive(true);
        UIManager.OnboardingUI_Panel_3_visible();
        UIManager.GazeTimerParticles.GazeFinished.AddListener(PlayVideoFromReticle);
        UIManager.GazeTimerParticles.GazeFinished.AddListener(UIManager.OnboardingUI_Panel_3_inactive);
        UIManager.PreExperienceUI_hidden();
        PreExperienceUI_JustGoback();
        UIManager.PreExperienceUI_active();
        preExperience = false;
    }

    public void StartOnboarding()
    {

        //Debug.Log("running StartOnboarding");
        UIManager.FocusHandler.SetActive(false);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(StartOnboarding);
        UIManager.OnboardingUI_Panel_3_inactive();

        //time length before gaze gate appears
        Invoke("GazeGateToExperience", 47f);
        AudioManagerSimple.i.LowerMenuMusic();
        onboarding.Play();
        PreExperienceUI_JustSkipGoBack();
        UIManager.PreExperienceUI_active();
        //Debug.Log("Im PRE EXPERIENCE AND IM getting called - in the inherited one from place only - no, that's impossible! I'm not derived from you!!");

    }

    public void PlayVideoFromReticle()
    {
        UIManager.Fader.FadeOut(1.5f,2f);
        //ChangeState(AppStates.Experience);
        preExperience = false;
        AppManager.Instance.UIManager.colorSkybox.LerpFinished.AddListener(ChangeStateExperience);

        UIManager.FocusHandler.SetActive(false);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(StartOnboarding);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(PlayVideoFromReticle);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(UIManager.OnboardingUI_Panel_3_inactive);
        UIManager.GazeTimerParticles.GazeStarted.RemoveListener(UIManager.PreExperienceUI_inactive);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(UIManager.PreExperienceUI_hidden);
        UIManager.GazeTimerParticles.GazeCanceled.RemoveListener(UIManager.PreExperienceUI_visible);

        AudioManagerSimple.i.FadeMenuMusic();
        onboarding.Stop();

        UIManager.PreExperienceUI_inactive();

        //Cancel thise invoked methods
        CancelInvoke("GazeGateToOnboarding");
        CancelInvoke("StartOnboarding");
        CancelInvoke("GazeGateToExperience");
        UIManager.Floor_inactive();
        AppManager.Instance.UIManager.colorSkybox.LerpToBlack(4);
    }

    public override void ChangeState(AppStates newState)
    {

        base.ChangeState(newState);

        //Debug.Log("Im PRE EXPERIENCE AND IM getting called");

        switch (currentAppState)
        {

            case AppStates.PreExperience:
                //Debug.Log("In AppManagerPlace preExperience state");
                preExperience = true;
                UIManager.GazeTimerParticles.GazeStarted.AddListener(UIManager.PreExperienceUI_inactive);
                UIManager.GazeTimerParticles.GazeFinished.AddListener(UIManager.PreExperienceUI_hidden);
                UIManager.GazeTimerParticles.GazeCanceled.AddListener(UIManager.PreExperienceUI_visible);
                //UIManager.GazeTimerParticles.GazeFinished.RemoveAllListeners();
                //Debug.Log("KioskMode: " + KioskMode);

                if (SkipMenuAndGoIntoPreExperience)
                {
                    AudioManagerSimple.i.PlayMenuMusic();
                    UIManager.Floor_active();
                    UIManager.EnvironmentParticles.SetActive(true);
                    SkipMenuAndGoIntoPreExperience = false;
                }


                if (KioskMode)
                {
                    //kiosk mode - add an initial gaze gate before going into onboarding 
                    Invoke("GazeGateToOnboarding", 1f);
                }
                else
                {
                    //No kiosk mode - just run through onboarding normally 
                    Invoke("StartOnboarding", 1f);
                    
                }
                
                //UIManager.colorSkybox.LerpToRandomColour();

                break;
        }

        //Debug.Log("Im at the end of the inherited switch");

    }

    public override void LoadingTriggeredEvent()
    {
        //Debug.Log("Loading triggered event in base class triggered");
    }

    public override void OnboardingTriggeredEvent()
    {
        //Debug.Log("Onboarding triggered event in base class triggered");

    }

    public override void MenuTriggeredEvent()
    {
        //Debug.Log("MenuTriggeredEvent triggered event in base class triggered");

    }

    public override void ExperienceTriggeredEvent()
    {

        UIManager.EnvironmentParticles.SetActive(false);
        AudioManagerSimple.i.PlayOneShotInt(9);

        //Debug.Log("ExperienceTriggeredEvent event in inherited class triggered");

    }

    public void ReturnHomeFromOnboarding()
    {
        //if onboarding audio is playing, cut it off 
        if (onboarding.isPlaying)
        {
            onboarding.Stop();
        }

        UIManager.PreExperienceUI_inactive();
        UIManager.FocusHandler.SetActive(false);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(StartOnboarding);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(PlayVideoFromReticle);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(UIManager.OnboardingUI_Panel_3_inactive);
        UIManager.GazeTimerParticles.GazeStarted.RemoveListener(UIManager.PreExperienceUI_inactive);
        UIManager.GazeTimerParticles.GazeFinished.RemoveListener(UIManager.PreExperienceUI_hidden);
        UIManager.GazeTimerParticles.GazeCanceled.RemoveListener(UIManager.PreExperienceUI_visible);

        CancelInvoke("GazeGateToOnboarding");
        CancelInvoke("StartOnboarding");
        CancelInvoke("GazeGateToExperience");

        //if the men u music is not at max (i.e. menuMin for in VO onboarding), make it normal menu vol
        //Debug.LogFormat("AudioManagerSimple.i.menuMusic.volume | AudioManagerSimple.i.menuMax", AudioManagerSimple.i.menuMusic.volume, AudioManagerSimple.i.menuMax);
        if (AudioManagerSimple.i.menuMusic.volume != AudioManagerSimple.i.menuMax)
        {

            AudioManagerSimple.i.RaiseMenuMusic();
        }
        preExperience = true;
        ChangeState(AppStates.Menu);

    }

}
