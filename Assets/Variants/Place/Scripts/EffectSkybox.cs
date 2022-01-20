using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class  NormalBroadcastEvent : UnityEvent<float> {

}

[System.Serializable]
public class ColorPair 
{

    public Color edgeColor;
    public Color bodyColor;


}



public class EffectSkybox : MonoBehaviour
{

    public ColorPair[] colorPairs;

    public ColorPair active;
    public ColorPair inactive;

    public Renderer renderer; 
    public Material skyboxMaterial;
    public Material skyboxInstance;


    public BreathCycle breathCycle;


    ColorPair newTopPair;
    ColorPair newBottomPair;

    public ColorPair lastTopColour;
    public ColorPair lastBottomColour;

    NormalBroadcastEvent normalChange = new NormalBroadcastEvent();
    public UnityEvent LerpStarted = new UnityEvent();
    public UnityEvent LerpFinished = new UnityEvent();


    public bool lerping;

    public IEnumerator NormalLerp;

    private void Start()

    { 
        skyboxInstance = skyboxMaterial;
        RenderSettings.skybox = skyboxInstance;
        SetToBlack();
        //affectedEffect.SetFloat("_Transparency", 0);
        //skyboxInstance.color = colorPairs[0].bodyColor;

        //SetToLastColour();

    }


    //// Start is called before the first frame update
    ////void Start()
    ////{
    ////    //breathCycle.BreathValueChange.AddListener(ColourShift);

    ////}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    //private void OnDisable()
    //{
        
    //}

    public void KickOffColourShift()
    {

        //PrepForColourChange();
        //breathCycle.BreathValueChange.AddListener(ShiftToRandomColor);
        //breathCycle.TopOfBreath.AddListener(PrepForColourChange);

    }

    void ColourShift(float normal)
    {
        //Start to finish
        skyboxInstance.SetColor("Color_899D7B6C", Color.Lerp(colorPairs[0].edgeColor, colorPairs[0].bodyColor, normal));

        skyboxInstance.SetFloat("Vector1_85B536BE", Mathf.Lerp(-.5f, .5f, (Mathf.PingPong(Time.deltaTime, 1))));

        skyboxInstance.SetColor("Color_666364E1", Color.Lerp(colorPairs[1].edgeColor, colorPairs[1].bodyColor, normal/2));

    }
    public void SetToLastColour()
    {
        skyboxInstance.SetColor("_SkyColor1", lastTopColour.edgeColor);
        skyboxInstance.SetColor("_SunColor", lastTopColour.bodyColor);
        skyboxInstance.SetColor("_SkyColor2", lastBottomColour.bodyColor);
        skyboxInstance.SetColor("_SkyColor3", lastBottomColour.edgeColor);
    }


    void PrepForColourChange(bool random)
    {

        lastTopColour.edgeColor = skyboxInstance.GetColor("_SkyColor1");
        lastTopColour.bodyColor = skyboxInstance.GetColor("_SunColor");

        lastBottomColour.bodyColor = skyboxInstance.GetColor("_SkyColor2");
        lastBottomColour.edgeColor = skyboxInstance.GetColor("_SkyColor3");

        if (!random) return;

        newTopPair = colorPairs[Random.Range(0, colorPairs.Length)];
        newBottomPair = colorPairs[Random.Range(0, colorPairs.Length)];

        while (newBottomPair == newTopPair)
        {
            newBottomPair = colorPairs[Random.Range(0, colorPairs.Length)];

        }

    }



    void ShiftToTargetColour(float normal)
    {

        skyboxInstance.SetColor("_SkyColor1", Color.Lerp(lastTopColour.edgeColor, newTopPair.edgeColor, normal));
        skyboxInstance.SetColor("_SunColor", Color.Lerp(lastTopColour.bodyColor, newTopPair.bodyColor, normal));
        skyboxInstance.SetColor("_SkyColor2", Color.Lerp(lastBottomColour.bodyColor, newTopPair.bodyColor, normal));
        skyboxInstance.SetColor("_SkyColor3", Color.Lerp(lastBottomColour.edgeColor, newTopPair.edgeColor, normal));

    }

    //[ContextMenu("StartLerp")]
    //public void LerpShaderToZero()
    //{
    //    StartCoroutine(Lerp(2,true));
    //}

    [ContextMenu("LerpToRandomColour")]
    public void LerpToRandomColour(int time)
    {

        if(lerping)StopCoroutine(NormalLerp);
        normalChange.RemoveAllListeners();
        PrepForColourChange(true);
        NormalLerp = Lerp(8, false);
        StartCoroutine(NormalLerp);
        normalChange.AddListener(ShiftToTargetColour);

    }

    public void LerpToBlack(int time)
    {
        if (lerping) StopCoroutine(NormalLerp);
        normalChange.RemoveAllListeners();
        PrepForColourChange(false);
        newTopPair = inactive;
        newBottomPair = inactive;
        NormalLerp = Lerp(time, false);
        StartCoroutine(NormalLerp);
        normalChange.AddListener(ShiftToTargetColour);
    }


    public void LerpToTargetColor(int topPair, int bottomPair, int time)
    {
        if (lerping) StopCoroutine(NormalLerp);
        normalChange.RemoveAllListeners();
        PrepForColourChange(false);
        newTopPair = colorPairs[topPair];
        newBottomPair = colorPairs[bottomPair];
        NormalLerp = Lerp(time, false);
        StartCoroutine(NormalLerp);
        normalChange.AddListener(ShiftToTargetColour);
    }

    public void LerpTransparency(float time, bool down)
    {

        normalChange.RemoveAllListeners();
        if (lerping) StopCoroutine(NormalLerp);
        NormalLerp = Lerp(time, down);
        StartCoroutine(NormalLerp);
        normalChange.AddListener(ChangeTransparency);

    }

    public void SetToBlack()
    {
        skyboxInstance.SetColor("_SkyColor1", inactive.edgeColor);
        skyboxInstance.SetColor("_SunColor", inactive.bodyColor);
        skyboxInstance.SetColor("_SkyColor2", inactive.bodyColor);
        skyboxInstance.SetColor("_SkyColor3", inactive.edgeColor);
    }

    void ChangeTransparency(float normal)
    {
        //affectedEffect.SetFloat("_Transparency", normal);
        //skyboxInstance.color = Color.Lerp(colorPairs[0].edgeColor, colorPairs[0].bodyColor, normal);
    }

    public IEnumerator Lerp (float rate, bool down)
    {
        //Debug.Log("I've been requested to lerp by something");
        lerping = true;
        LerpStarted.Invoke();

        float normal = 0;

        while (normal <= 1)
        {

            normal += Time.deltaTime / rate;
            //Debug.Log(normal);

            if (down)
            {

                normalChange.Invoke(1 - normal);

            } else
            {

                normalChange.Invoke(normal);

            }
            yield return null;

        }

        lerping = false;
        LerpFinished.Invoke();
        normalChange.RemoveAllListeners();
        LerpStarted.RemoveAllListeners();
        LerpFinished.RemoveAllListeners();


    }
}
