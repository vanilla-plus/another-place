using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitUI : MonoBehaviour
{
    public static QuitUI i;
    [SerializeField]
    public GameObject self, initial, confirmation, quitting;

    [SerializeField]
    private int _state = -1;
    public int state
    {
        get => _state;
        set
        {
            _state = Mathf.Clamp(value: value,
                                 min: 0,
                                 max: 3);
            
            self.SetActive(_state         != 0);
            initial.SetActive(_state      == 1);
            confirmation.SetActive(_state == 2);
            quitting.SetActive(_state     == 3);
            
//            if (_state == 0) self.SetActive(false);
//            else
//            {
//                instance.self.SetActive(true);
//                instance.initial.SetActive(_state      == 1);
//                instance.confirmation.SetActive(_state == 2);
//                instance.quitting.SetActive(_state     == 3);
//            }
        }
    }


    private void Awake()
    {
        i = this;

        state = 0;
    }


    public void SetState(int newState) => state = newState;

//    public void SetState(int newState)
//    {
//        
//        
//        if (newState == 0) i.self.SetActive(false);
//        else
//        {
//            i.self.SetActive(true);
//            i.initial.SetActive(newState == 1);
//            i.confirmation.SetActive(newState == 2);
//            i.quitting.SetActive(newState == 3);
//        }
//    }

    public async void Quit()
    {
        state = 3;
        Debug.Log("EndSession Start");
        System.DateTime startTime = System.DateTime.Now;
        await Analytics.EndSession(false);
        Debug.Log("EndSession Complete " + (System.DateTime.Now - startTime).TotalMilliseconds);
        Application.Quit();
    }
}
