using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CurvedUI
{
    /// <summary>
    /// This class contains code that controls the visuals (only!) of the laser pointer.
    /// </summary>
    public class CurvedUILaserBeam : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        Transform LaserBeamTransform;
        [SerializeField]
        Transform LaserBeamDot;
        [SerializeField]
        bool CollideWithMyLayerOnly = false;
        [SerializeField]
        bool hideWhenNotAimingAtCanvas = false;
        
        //jesse stuff
        public float hideLength = 0f;
        public float length;

        [SerializeField]
        bool respectCanvasGroupSettings = true;
        [SerializeField]
        private bool isCanvasInteractable;
        [SerializeField]
        private bool isCanvasBlocking;

        private CanvasGroup canvasGroupSettings;

        #pragma warning restore 0649

        // Update is called once per frame
        protected void Update()
        {

            //get direction of the controller
            Ray myRay = new Ray(this.transform.position, this.transform.forward);


            //make laser beam hit stuff it points at.
            if(LaserBeamTransform && LaserBeamDot) {
                //change the laser's length depending on where it hits
                length = 5;


                //create layerMaskwe're going to use for raycasting
                int myLayerMask = -1;
                if (CollideWithMyLayerOnly)
                {
                    //lm with my own layer only.
                    myLayerMask = 1 << this.gameObject.layer;
                }


                RaycastHit hit;
                if (Physics.Raycast(myRay, out hit, length, myLayerMask))
                {
                    length = Vector3.Distance(hit.point, this.transform.position);

                    //Find if we hit a canvas
                    CurvedUISettings cuiSettings = hit.collider.GetComponentInParent<CurvedUISettings>();
                    
                    
                    //jesse - if canvas group settings interactable and blocks raycasts are off, don't show the beam 
                    if (respectCanvasGroupSettings)
                    { 
                        //if we care, get the vales for the canvas group
                        canvasGroupSettings = hit.collider.GetComponentInParent<CanvasGroup>();
                        if (canvasGroupSettings != null)
                        {
                            isCanvasInteractable = canvasGroupSettings.interactable;
                            isCanvasBlocking = canvasGroupSettings.blocksRaycasts;
                        }
                    }
                    if (cuiSettings != null)
                    {
                        // jesse - we are respecting canvas group and there is a canvas group exists and we do not want to block rays
                        if (respectCanvasGroupSettings && canvasGroupSettings != null && !isCanvasBlocking)
                        {
                            length = hideLength;
                        }
                        else 
                        //jesse - else we do the normal pointer pointing at stuff
                        {
                            //find if there are any canvas objects we're pointing at. we only want transforms with graphics to block the pointer. (that are drawn by canvas => depth not -1)
                            int selectablesUnderPointer = cuiSettings.GetObjectsUnderPointer().FindAll(x => x != null && x.GetComponent<Graphic>() != null && x.GetComponent<Graphic>().depth != -1).Count;

                            length = selectablesUnderPointer == 0 ? 5 : Vector3.Distance(hit.point, this.transform.position);
                        }
                    }
                    else if (hideWhenNotAimingAtCanvas)
                        length = hideLength;
                }
                else if (hideWhenNotAimingAtCanvas)
                    length = hideLength;

                if (length == hideLength)
                    LaserBeamDot.gameObject.SetActive(false);
                else
                    LaserBeamDot.gameObject.SetActive(true);

                //set the length of the beam
                LaserBeamTransform.localScale = LaserBeamTransform.localScale.ModifyZ(length);

            }
           

        }
    }
}
