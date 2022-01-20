using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeTimerEvents : GazeTimer
{
    public ParticleHandler environmentalParticle;
    public ParticleHandler interactionParticle;
    public GameObject particleCollider;
    public GameObject secondGravity;

    // Start is called before the first frame update
    void Start()
    {
        base.GazeStarted.AddListener(GazeStartedEvent);
        base.GazeCanceled.AddListener(GazeCanceledEvent);
        base.GazeFinished.AddListener(GazeFinishedEvent);

    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    //public override void ChangeState(States newState)
    //{
    //    base.ChangeState(newState);

    //    //content specific events
    //    switch (newState)
    //    {
    //        case States.Available:

    //            break;

    //        case States.GazeActive:

    //            break;

    //        case States.Cooldown:

    //            break;

    //        case States.Inactive:

    //            break;

    //    }
    //} 

    public void GazeStartedEvent()
    {
        environmentalParticle.DrawIn();
        interactionParticle.SetGravity(0);
        interactionParticle.SetRotation(.02f);
        interactionParticle.SetColor(0);
        environmentalParticle.SetGravity(2);

    }

    public void GazeFinishedEvent()
    {
        interactionParticle.SetGravity(0.02f);
        interactionParticle.SetRotation(0);
        interactionParticle.ExpellAfterTime(.5f);
        


        //environmentalParticle.SetGravity(.1f);
        //TODO handover to system that adds another gravity

        environmentalParticle.Expell();

        interactionParticle.FadeOut(1);
        //environmentalParticle.NeutraliseGravity();
        //environmentalParticle.Reset();
        particleCollider.SetActive(false);

        base.GazeFinished.RemoveAllListeners();
        base.GazeFinished.AddListener(GazeFinishedEvent);


    }

    public void GazeCanceledEvent()
    {
        environmentalParticle.Expell();
        interactionParticle.SetColor(1);

        interactionParticle.SetGravity(.02f);
        interactionParticle.SetRotation(0);
        interactionParticle.ExpellAfterTime(0.5f);
        //interactionParticle.FadeOut(1);
      
    }

}
