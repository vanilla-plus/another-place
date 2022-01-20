using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySource : MonoBehaviour
{
    public Ray ray;
    public RayTarget currentTarget;
    int layerMask = 1 << 9;
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        //layerMask = ~layerMask;
    }
    //TODO - turn off raysource when not in use (maybe attach to gazehandler)
    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");

            if (hit.transform.gameObject.GetComponent<RayTarget>()) {

                currentTarget = hit.transform.gameObject.GetComponent<RayTarget>();

                //Debug.Log("Did Hit raytarget");

                if (!currentTarget.gazeTimer.gazeActive)
                {


                    currentTarget.gazeTimer.GazeStart();

                }

            } else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                //Debug.Log("Did not Hit");
                if (currentTarget != null && currentTarget.gazeTimer.gazeActive)
                {

                    currentTarget.gazeTimer.GazeEnd();

                }
            }

        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            //Debug.Log("Did not Hit");
            if (currentTarget != null && currentTarget.gazeTimer.gazeActive)
            {

                currentTarget.gazeTimer.GazeEnd();

            }
        }
    }
}
