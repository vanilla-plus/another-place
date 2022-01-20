using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceOptionsHandler : MonoBehaviour
{

    public string ExperienceAudioBasePath;
    public bool femaleSelected;
    public int difficulty;

    public bool genderSet;
    public bool difficultySet;

    public string audioPath;
    public string onboardingPath;
    public string voiceTrackLabel;

    public GameObject BeginButton;


    //TODO needs a sort of 'experience prep' class somewhere, that loads video based on carousel, then sets audio based on this

    // Start is called before the first frame update
    void Start()
    {
        AllowExperienceToBegin(genderSet, difficultySet);
    }

    public void SetVoiceGender(bool femaleSelected)
    {

        this.femaleSelected = femaleSelected;
        genderSet = true;
        if (femaleSelected)
        {
            AudioManagerSimple.i.PlayOneShotInt(12);
        } else
        {
            AudioManagerSimple.i.PlayOneShotInt(14);

        }
        AllowExperienceToBegin(genderSet, difficultySet);
    }

    public void SetDifficulty(int difficulty)
    {

        this.difficulty = difficulty;
        difficultySet = true;

        switch (difficulty)
        {
            case 1:
                AudioManagerSimple.i.PlayOneShotInt(11);
                ;
                break;

            case 2:
                AudioManagerSimple.i.PlayOneShotInt(13);

                break;
            case 3:
                AudioManagerSimple.i.PlayOneShotInt(10);

                break;
        }
        AllowExperienceToBegin(genderSet, difficultySet);
    }



    public void SetupAudio()
    {


        if (genderSet && difficultySet)
        {
            string audioSelection = "";
            string onboardingSelection = "";
            if (femaleSelected)
            {
                audioSelection += "female";
                onboardingSelection += "female-onboarding";
                AppManager.Instance.VideoController.VideoGuideGender = "Female";
            }
            else
            {
                audioSelection += "male";
                onboardingSelection += "male-onboarding";
                AppManager.Instance.VideoController.VideoGuideGender = "Male";
            }

            switch (difficulty)
            {
                case 1:
                    audioSelection += "-beginner";
                    AppManager.Instance.VideoController.VideoGuidanceLevel = "Beginner";
                    break;
                case 2:
                    audioSelection += "-intermediate";
                    AppManager.Instance.VideoController.VideoGuidanceLevel = "Intermediate";
                    break;
                case 3:
                    audioSelection += "-expert";
                    AppManager.Instance.VideoController.VideoGuidanceLevel = "Expert";
                    break;
            }

            audioSelection = AppManager.Instance.LocalStorage + ExperienceAudioBasePath + audioSelection + ".mp3";
            //Debug.Log("SetupAudio : audioSelection: " + audioSelection);

            onboardingSelection = AppManager.Instance.LocalStorage + ExperienceAudioBasePath + onboardingSelection + ".mp3";
            //Debug.Log("SetupAudio : onboardingSelection: " + onboardingSelection);

            AppManager.Instance.SetUpAudioForExperience(audioSelection, voiceTrackLabel, true, false);
            AppManager.Instance.SetUpAudioForExperience(onboardingSelection, "Onboarding", false, false, true); //this was true before kiosk
            //at the moment handling this by checking state, which pushes this forward, so will maybe return to it when not
            //fried as a coconut.
            //AppManager.Instance.SetUpAudioForExperience(onboardingSelection, "Onboarding");


        }

    }

    //added as validation to only show begin button if selections are made.
    void AllowExperienceToBegin(bool genderSet, bool difficultySet)
    {
        if (genderSet && difficultySet)
        {
            BeginButton.SetActive(true);
        }
        else
        {
            BeginButton.SetActive(false);
        }
    }

    public void BeginExperience()
    {
        AppManager.Instance.ChangeState(AppManager.AppStates.PreExperience);
        AppManager.Instance.UIManager.VoiceObjectUI_hidden();
    }


}
