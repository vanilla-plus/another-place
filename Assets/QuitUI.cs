using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitUI : MonoBehaviour
{
    public static QuitUI instance;
    [SerializeField] GameObject self, initial, confirmation, quitting;
    int state = 0;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            instance.SetState(0);
        }
        else
        {
            Debug.LogError("[QuitUI : Start] An instance of QuitUI already exists. Destroying 'this' now.");
            Destroy(this);
        }
    }

    public void SetState(int state)
    {
        if (state < 0 || state > 3)
        {
            Debug.LogError($"[QuitUI : SetState] {state} is not a valid state. Setting state to 0.");
            state = 0;
        }

        this.state = state;

        if (state == 0) instance.self.SetActive(false);
        else
        {
            instance.self.SetActive(true);
            instance.initial.SetActive(state == 1);
            instance.confirmation.SetActive(state == 2);
            instance.quitting.SetActive(state == 3);
        }
    }

    public async void Quit()
    {
        SetState(3);
        Debug.Log("EndSession Start");
        System.DateTime startTime = System.DateTime.Now;
        await Analytics.EndSession(false);
        Debug.Log("EndSession Complete " + (System.DateTime.Now - startTime).TotalMilliseconds.ToString());
        Application.Quit();
    }
}
