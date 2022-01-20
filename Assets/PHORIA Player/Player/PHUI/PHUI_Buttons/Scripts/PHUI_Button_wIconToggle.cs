using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PHUI_Button_wIconToggle : MonoBehaviour
{

    public Image _SourceImage;
    public Sprite _InitialImageToShow;
    public Sprite _SecondaryImageToShow;

    //TODO JHPH make it so this updates in edtor when changed

    void OnEnable()
    {
        //_SourceImage.sprite = _InitialImageToShow; //show initial image in button
    }

    public void SetInitialState()
    {
        _SourceImage.sprite = _InitialImageToShow;
    }
    public void SetSecondaryState()
    {
        _SourceImage.sprite = _SecondaryImageToShow;
    }

    public void ToggleButtonState(int toggleState)
    {
        if (toggleState==0) //true - switch to initial
        {
            SetInitialState();

        }
        else if(toggleState == 1) //false = switch to secondary
        {
            SetSecondaryState();
        }

    }
}
