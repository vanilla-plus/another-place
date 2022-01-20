using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectShape : MonoBehaviour
{

    public float scaleIncrease;
    public BreathCycle breathCycle;
    public GameObject objectToScale;

    // Start is called before the first frame update
    virtual public void Start()
    {
        breathCycle.BreathValueChange.AddListener(BreathScale);
    }

    public void BreathScale(float normal)
    {
        objectToScale.transform.localScale = new Vector3(normal * scaleIncrease, normal * scaleIncrease, normal * scaleIncrease);
    }

}
