using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CurvedUI;

public class OculusInputListener : MonoBehaviour
{
    public bool settingsAvailable;
    public bool settingsTriggered;

    public bool settingsCoolDownBegun = false;
    public float CoolDownTime;
    public CurvedUILaserBeam LaserPointer;
    public float LaserLength;

    public UnityEvent SettingsSelected;
    public UnityEvent SettingsCanceled;

   

    // Start is called before the first frame update
    void Start()
    {
        // some quick set up, just in case
        settingsTriggered = false;
        settingsCoolDownBegun = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.GetAxis("Cancel"));
        if (settingsAvailable)
        {
            ListenForSettingsChange();
        }
    }

    public void ToggleControls()
    {
        settingsTriggered = !settingsTriggered;
    }


    public void StartListening()
    {
        settingsAvailable = true;
        //Debug.Log("OculusInputListener : StartListening Called, settingsAvailable=" + settingsAvailable);
        //Debug.Log("OculusInputListener : StartListening Called, settingsTriggered=" + settingsTriggered);
    }

    public void StopListening()
    {
        //Debug.Log("OculusInputListener : StopListening Called, settingsAvailable=" + settingsAvailable);
        //Debug.Log("OculusInputListener : StopListening Called, settingsTriggered=" + settingsTriggered);
        settingsCoolDownBegun = false;
        CancelInvoke("CloseControls");
        settingsAvailable = false;
        settingsTriggered = false;
        settingsCoolDownBegun = false;

    }

    void SetSettingsAvailability(bool isAvailable)
    {
        settingsAvailable = isAvailable;
    }

    public void ToggleSettingsAvailability()
    {
        settingsAvailable = !settingsAvailable;
        //Debug.Log("ToggleCalled");
    }

    //public void CloseSettings()
    //{
    //    settingsTriggered = false;
    //    SettingsCanceled.Invoke();
    //    settingsAvailable = false;
    //    Invoke("ToggleSettingsAvailability", 1f);
    //    //AppManager.Instance.UIManager.laserPointer.SetActive(false);
    //}

    public void OpenControls()
    {
        settingsTriggered = true;
        settingsCoolDownBegun = false;
        SettingsSelected.Invoke();
        //Debug.Log("now in Open Controls");
        //Invoke("ToggleSettingsAvailability", 0.5f);
        //AppManager.Instance.UIManager.laserPointer.SetActive(false);
    }

    public void CloseControls()
    {
        settingsTriggered = false;
        settingsCoolDownBegun = false;
        SettingsCanceled.Invoke();
        //Debug.Log("now in Close Controls");
        //Invoke("ToggleSettingsAvailability", 0.5f);
        //AppManager.Instance.UIManager.laserPointer.SetActive(false);
    }



    void ListenForSettingsChange()
    {
        //this is in to debug the laser, not needed generally
        //If trigger and laser = 5 >>> toggle panel
        if (((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
                && (LaserPointer.length == 5 || LaserPointer.length == LaserPointer.hideLength))
                    || (Input.GetKeyUp(KeyCode.C)))
        {
            //Debug.Log("OculusInputListener _qwe : toggle panel - triggers pressed and LaserLength is 5, [" + LaserPointer.length + "]");

            //Setting available and not yet triggered, cancel all closing/cool down stuff and open the controls
            if (!settingsTriggered)
            {
                OpenControls();
            }
            //settings not available and setting have been triggered
            else
            {
                CloseControls();
            }

        }

        //if panel is triggered and laser = 5 >>> invoke cool down
        else if (settingsTriggered && (LaserPointer.length == 5 || LaserPointer.length == LaserPointer.hideLength) && !settingsCoolDownBegun)
        {
            //Debug.Log("OculusInputListener _asd : invoke cool down - settingsTriggered = [" + settingsTriggered + "] and LaserLength is 5, [" + UIManager.instance.LaserBeam.length + "]");
            settingsCoolDownBegun = true;
            //Debug.Log("COOL DOWN, done waiting for 5 seconds. cool");
            Invoke("CloseControls", CoolDownTime);
        }

        //if panel is toggled and laser < 5 >>> cancel cool down
        else if (settingsTriggered && (LaserPointer.length < 5 && LaserPointer.length > LaserPointer.hideLength))
        {
            //Debug.Log("OculusInputListener _zxc : cancel cool down - settingsTriggered = [" + settingsTriggered + "] and LaserLength is [" + UIManager.instance.LaserBeam.length + "]");
            settingsCoolDownBegun = false;
            CancelInvoke("CloseControls");
            //Debug.Log("COOL DOWN, CANCELLED");
        }
        
    }


}

