using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GazeTimer))]
public class GazeTimerEditor : Editor {
    public GazeTimer script;
    void OnEnable()
    {
        script = (GazeTimer)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Player Action"))
        {
            script.GazeStart();
        }
    }
}

#endif
public class FloatEventNormal : UnityEvent<float> {
}

[RequireComponent(typeof(LineRenderer))]
public class GazeTimer : MonoBehaviour {

    //effects based on timer countdown/count up, deeply coupled to rotating around a circle, and moving a line renderer or particle, which could be broken up.

    public float gazeTime;
    public float cancelGazeTime;

    public int circlePoints;
    public float radians;
    public float timeIncrements;

    public bool gazeActive;

    public float radius;
    public float radiusSecondary;
    public LineRenderer _line;

    public Vector3 renderPoint;

    public Color activeColor;
    public Color inactiveColor;

    public IEnumerator gazeTimerLerp;

    float normal = 0;
    float subNormal = 0;
    int vectorIndex = 0;

    float lastRadian = 0;
    float currentRadian = 0;
    float totalRadian = 0;

    public UnityEvent GazeStarted;
    public UnityEvent GazeFinished;
    public UnityEvent GazeCanceled;
    public UnityEvent TimerStart;
    public UnityEvent TimerFinished;

    public FloatEventNormal NormalEvent;

    public IEnumerator TimerProcess;

    bool queueTimerRunning;
    public bool coolDown;

    public bool active;

    Color newColor;

    public AudioSource audioSource;
    //public AudioPlayback mainAudio;
    public AudioClip[] clips;

    public Transform particleSource;
    public Transform particleSourceSecondary;

    public enum States { Available, GazeActive, Cooldown, Inactive, Cancelled}
    public States state;

    public GameObject GazeParticlesRaySource;
    private RaySource raySource;

    // Use this for initialization
    void Awake()
    {
        raySource = GazeParticlesRaySource.GetComponent<RaySource>();
        SetUpCircle();

        SetUpEvents();
        //Debug.Log("I'm still runnin");
    }

    public void SetUpCircle()
    {

        //Sets up math around circle and line render if visible

        _line = GetComponent<LineRenderer>();

        renderPoint = Vector3.up * radius;

        radians = 360 / circlePoints;

        timeIncrements = gazeTime / circlePoints;

    }

    public void SetUpEvents()
    {

        //Sets up events to be used for other systems

        if (NormalEvent == null) NormalEvent = new FloatEventNormal();
        if (GazeStarted == null) GazeStarted = new UnityEvent();
        if (GazeFinished == null) GazeFinished = new UnityEvent();
        if (GazeCanceled == null) GazeCanceled = new UnityEvent();

        if (TimerStart == null) TimerStart = new UnityEvent();
        if (TimerFinished == null) TimerFinished = new UnityEvent();

        GazeFinished.AddListener(GazeStart);
        NormalEvent.AddListener(LerpReticle);
        NormalEvent.AddListener(LerpAudio);

    }
    private void OnEnable()
    {
        raySource.enabled = true;

    }
    private void OnDisable()
    {
        raySource.enabled = false;

    }
    public virtual void ChangeState(States newState)
    {
        //Debug.Log("Gaze state now "+newState);
        state = newState;
        switch (newState)
        {
            case States.Available:

                break;

            case States.GazeActive:

                //Debug.Log("Gaze now active");
                gazeActive = true; //gaze has now started (stops double gaze)
                GazeStarted.Invoke();
                subNormal = 0; //the fuck is this? -   jesse: haha. oh Sam, don't ever change.

                break;

            case States.Cooldown:

                HandleCoolDown();
                //mainAudio.PlayOneShot(clips[0]);
                Invoke("HandleCoolDown",5f);
                //TimerFinished.AddListener(SetVertexCountToZero);
                gazeActive = false;

                break;
            case States.Inactive:

                break;

        }
    }


    public void GazeStart()
    {
        //Maybe should be handle gaze event
        //Debug.Log("Gaze event received");


        if (coolDown) return; //if not cooling down continue
        if (!active) return; // if currently active continue
        if (!gazeActive) //if a gaze hasn't already started
        {

            ChangeState(States.GazeActive);
            HandleLerp(); //start lerpin

        }

    }

    public void GazeEnd()
    {
        if (coolDown) return; //if not cooling down continue
        if (!active) return; // if currently active continue
        if (gazeActive) //if a gaze hasn't already started
        {

            gazeActive = false; //gaze has ended (meaning it can play)
            HandleLerp();
        }

    }

    public void HandleLerp()
    {
        //Debug.Log("Handle Lerp received");

        if (gazeTimerLerp != null)
        {

            StopCoroutine(gazeTimerLerp); //stops any existing gazes

        }

        gazeTimerLerp = LerpNormal();
        StartCoroutine(gazeTimerLerp);

    }

    public IEnumerator LerpNormal()
    {

        if (gazeActive)
        {

            // starts lerp from 0 to 1

            //Debug.Log("Lerping from zero to one");

            //sets up variables used
            float rate = 1.0f / gazeTime;
            normal = 0;
            vectorIndex = 0;
            
            //sets up head of circle
            renderPoint = Vector3.up * radius;
            Vector3 renderPointSecondary = Vector3.up * radiusSecondary;

            //Sets up line renderer

                //_line.startColor = activeColor;
                //_line.endColor = activeColor;
                //_line.loop = false;
                 

            audioSource.time = 4f;

            PlayClip(clips[0]);


            while (vectorIndex < circlePoints)
            {

                normal += Time.deltaTime * rate; //based on time since last frame

                //done this way to smoothly increment the circle within the time frame
                if (vectorIndex < normal * circlePoints)

                {
                    //not sure if this even gets used?

                    totalRadian = Mathf.Lerp(0, 360, normal);

                    currentRadian = (totalRadian - lastRadian);


                    if (vectorIndex != 0)
                    {
                        renderPoint = RotatePointAroundAxis(renderPoint, radians, this.transform.forward);
                        renderPointSecondary = RotatePointAroundAxis(renderPointSecondary, radians, this.transform.forward);
                    } 
                    //Debug.Log(radians);

                    //sets up next position of vector line
                    //_line.SetVertexCount(vectorIndex + 1);
                    //_line.SetPosition(vectorIndex, renderPoint);

                    //Debug.Log(renderPoint);
                    //Debug.Log(vectorIndex);

                    //this could get added to delegate, a function that gets called
                    if (!particleSource.gameObject.activeSelf) particleSource.gameObject.SetActive(true);
                    particleSource.localPosition = renderPoint;

                    if (!particleSource.gameObject.active) particleSource.gameObject.SetActive(true);
                    particleSourceSecondary.localPosition = renderPointSecondary;

                    vectorIndex++;

                    lastRadian = totalRadian;

                    NormalEvent.Invoke(normal);

                }


                yield return null;


            }

            GazeFinished.Invoke();
            ChangeState(States.Cooldown);
            //Debug.Log("Gaze Finished");

            //_line.loop = true;



        }

        else

        {
            //Debug.Log("LERP DOWN");
            GazeCanceled.Invoke();
            ChangeState(States.Available);
            //Debug.Log("Gaze cancelled");

            _line.loop = false;

            float rate = 1.0f / cancelGazeTime;

            float peakVector = vectorIndex;
            float peakNormal = normal;

            while (subNormal < 1f)
            {
                subNormal += Time.deltaTime * rate; //based on time since last frame
                //Debug.Log(normal);
                normal = (1 - subNormal) * peakNormal;
                //Debug.Log("normal is " + normal);
                //NormalEvent.Invoke(subNormal);

                totalRadian = Mathf.Lerp(0, 360, normal);
                currentRadian = (totalRadian - lastRadian);
                renderPoint = RotatePointAroundAxis(renderPoint, currentRadian, this.transform.position);
                lastRadian = totalRadian;
                NormalEvent.Invoke(normal);

                //Debug.Log("last radian is" + lastRadian);

                vectorIndex = Mathf.RoundToInt((1 - subNormal) * peakVector);
                //if (vectorIndex < 0) vectorIndex = 0;

                //_line.SetVertexCount(vectorIndex);

                yield return null;

            }

            //audioSource.Stop();

        }
    }

    public Vector3 RotatePointAroundAxis(Vector3 point, float angle, Vector3 axis)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        //Debug.Log(q);
        return q * point; //Note: q must be first (point * q wouldn't compile)

    }

    public void HandleCoolDown()
    {
        coolDown = !coolDown;
    }

    public void HandleTimer(float time, bool countUp)
    {

        //Debug.Log("Handle timer");


        if (TimerProcess != null)
        {

            StopCoroutine(TimerProcess);

        }

        TimerProcess = RunTimer(time, countUp);
        StartCoroutine(TimerProcess);

        queueTimerRunning = true;
        //Debug.Log("Timer starts");
    }

    public void LerpAudio(float normal)
    {
        audioSource.volume = Mathf.Lerp(0, 1, normal);
    }

    public void LerpReticle(float normal)
    {

        newColor = Color.Lerp(inactiveColor, activeColor, normal);

        _line.startColor = newColor;
        _line.endColor = newColor;


    }

    public IEnumerator RunTimer(float time, bool countUp)
    {
        TimerStart.Invoke();
        float rate = 1 / time;
        float i = 0;
        if (!countUp) i = 1;

        if (countUp)
        {
            while (i < 1)
            {

                i += Time.deltaTime * rate;
                NormalEvent.Invoke(i);

                yield return null;

            }
        }
        else
        {
            while (i > 0)
            {

                i -= Time.deltaTime * rate;
                NormalEvent.Invoke(i);

                yield return null;

            }
        }

        //Debug.Log("Timer Finished");

        TimerFinished.Invoke();
        TimerFinished.RemoveAllListeners();
        TimerStart.RemoveAllListeners();
        //NormalEvent.RemoveAllListeners();

    }

    public void SetVertexCountToZero()
    {
        _line.SetVertexCount(0);

    }

    public void PlayClip(AudioClip clip)
    {

        audioSource.clip = clip;
        audioSource.Play();

    }

    //split gaze start and gaze finish to make tidier

}

//stop and start environmental particles on complete or cancel
//stop interacttion particle on cancel
//influence of gravity reduced based on 'size' of position vector (so closer to circle less influence
//switch everything over to the state manager
//sub emitter


