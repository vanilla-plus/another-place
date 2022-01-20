using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FloatBroadcastEvent : UnityEvent <float> {

}

public class BreathCycle : MonoBehaviour {
    public bool breathingIn;
    public int breathInTime;
    public int breathOutTime;
    public float breathTime;

    public Vector3 topOfBreathScale;
    public Vector3 maxScale;
    public Vector3 minScale;
    public Vector3 targetScale;

    public float breathNormal;

    public Transform debugObjc;

    public FloatBroadcastEvent BreathValueChange = new FloatBroadcastEvent();
    public UnityEvent TopOfBreath = new UnityEvent();


    public IEnumerator BreathRoutine;
    // Start is called before the first frame update
    void Start()
    {
        BreathRoutine = Breath();
        StartCoroutine(BreathRoutine);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}


    IEnumerator Breath()
    {
        //Debug.Log("Started coroutine");
        TopOfBreath.Invoke();

        float rate = 0;
        float i = 0;

        float startPoint = 0;
        float endPoint = 0;

        //Debug.Log(breathInTime);

        //sets paramater values on in and out breath
        if (breathingIn)
        {
            rate = 1f / breathInTime;
            //Debug.Log(rate);
            startPoint = 0;
            endPoint = 1;
            //Debug.Log("I'm breathing in");

        }
        else
        {
            rate = 1f / breathOutTime;
            startPoint = 1;
            endPoint = 0;
            //Debug.Log("I'm breathing out");

        }

        //Debug.Log(i);
        //Debug.Log(rate);

        while (i < 1)
        {
            i += Time.deltaTime * rate;
            breathNormal = Mathf.Lerp(startPoint, endPoint, i * i * (3f - 2f * i));
            BreathValueChange.Invoke(breathNormal);
            //Debug.Log("WERREE LERPING");

            yield return null;

        }

        breathingIn = !breathingIn;
        BreathRoutine = Breath();
        StartCoroutine(BreathRoutine);

        //StartBreath();
    }

    //public void StartBreath()
    //{

    //}

}
