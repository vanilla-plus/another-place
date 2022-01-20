using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineIntroPlace : MonoBehaviour
{

    public TimelineIntro timeline;

    public void Start()
    {
        timeline.MilestoneEvent.AddListener(MilestoneReached);
    }

    public void MilestoneReached(int milestone)
    {
        //Debug.Log("Milestone inherited");
        switch (milestone)
        {
            case 1:
                //Debug.Log("The switch is runnin!");
                AudioManagerSimple.i.PlayOneShotInt(15);

                break;
            case 4:
                AudioManagerSimple.i.PlayOneShotInt(15);
                AudioManagerSimple.i.PlayOneShotInt(1);
                AudioManagerSimple.i.PlayMenuMusic();
                //Debug.Log("EnvironmentParticles on");
                timeline.UIManager.EnvironmentParticles.SetActive(true);

                break;
            case 10:
                //Debug.Log("GazeTimerObject on");
                timeline.UIManager.GazeTimerObject.SetActive(true);
                timeline.UIManager.GazeTimerParticles.GazeFinished.AddListener(timeline.StartMotorMoving);

                timeline.Motor.enabled = false;
                //timeline.UIManager.skybox.LerpToRandomColour();

                break;

            case 11:
                timeline.UIManager.OnboardingUI_Panel_3_inactive();
                timeline.UIManager.GazeTimerObject.SetActive(false);
                //timeline.UIManager.skybox.LerpToRandomColour();
                timeline.UIManager.GazeTimerParticles.GazeFinished.RemoveListener(timeline.StartMotorMoving);
                break;

            case 12:
                //stoping it here too incase the skip button was pressed on the gaze screen
                timeline.UIManager.GazeTimerObject.SetActive(false);
                break;


        }
    }

}
