using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventArray : MonoBehaviour
{

    public UnityEvent[] AnimationEvents;
    
    public void TriggerAinmationEvent(int eventIndex)
    {
        AnimationEvents[eventIndex].Invoke();
    }

}
