using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PHUI_Fader : MonoBehaviour
{
    [Space(20)]
    [Header(" - call FadeIn(), FadeOut() to use")]
    [Header(" - set colour, fade in and out times")]
    [Header(" - uses a screen overlay, useful for top level fades")]
    [Header("A basic screen fader")]

    public Color _FadeColour = new Color(0f, 0f, 0f, 0f);
    public float _FadeInTime = 1f;
    public float _FadeOutTime = 1f;

    public Image FaderImage;

    private float TimeBeforeNextFadeCanOccur = -1f;
    private bool IsAlreadyFading = false;
    [SerializeField] private Canvas FadeCanvas;
    private void Awake()
    {
        //Fader = this.GetComponentInChildren<Image>();
        FadeCanvas = this.GetComponentInChildren<Canvas>();
        FaderImage.color = _FadeColour;
    }

    public void FadeIn(float inTime = 0, float optionalWaitTime = 0)
    {
        if (inTime == 0)
            inTime = _FadeInTime;

        StartCoroutine(LerpAlpha(1f, 0f, inTime, _FadeColour, optionalWaitTime));
        //Debug.Log("Fading In over "+ inTime);
    }
    public void FadeOut(float outTime = 0, float optionalWaitTime = 0)
    {
        if (outTime == 0)
            outTime = _FadeOutTime;

//        if (!IsAlreadyFading)
 //           outTime = _FadeOutTime;

        StartCoroutine(LerpAlpha(0f, 1f, outTime, _FadeColour, optionalWaitTime));
        //Debug.Log("Fading Out over " + outTime);
    }

    IEnumerator LerpAlpha(float start, float end, float time, Color _FadeColour, float optionalWaitTime)
    {
        //Debug.Log("Lerping the fader");
        yield return new WaitForSeconds(optionalWaitTime);
        //Debug.Log("waited for " +optionalWaitTime+ " seconds");
        FadeCanvas.gameObject.SetActive(true);
        yield return null;

        float alpha = FaderImage.color.a;
        float t;
        Color newColor;
        for (t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
        {
            newColor = new Color(_FadeColour.r, _FadeColour.g, _FadeColour.b, Mathf.Lerp(start, end, t));
            FaderImage.color = newColor;
            yield return null;
            //Debug.Log("end " + end);
        }
        FaderImage.color = new Color(_FadeColour.r, _FadeColour.g, _FadeColour.b, end);

        //Debug.Log("the actual end "+ end);
        if (end==0)
        {
            FadeCanvas.gameObject.SetActive(false);
        }

    }


    //private void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.H))
    //    {
    //        FadeOut(1);
    //    }
    //    if (Input.GetKeyUp(KeyCode.S))
    //    {
    //        FadeIn(1);
    //    }
    //    if (Input.GetKeyUp(KeyCode.J))
    //    {
    //        FadeOut(1);
    //        FadeIn(1,2);
    //    }
    //}

}
