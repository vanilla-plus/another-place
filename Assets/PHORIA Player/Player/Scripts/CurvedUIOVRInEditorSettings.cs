using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurvedUI;
public class CurvedUIOVRInEditorSettings : MonoBehaviour
{
    // quick script to set up curvedUI for better editor use.  Should not build out and be editor only
    [Space(20)]
    [Header("  - Set the editor only CurvedUI default input method")]
    [Header("  - Set the editor only transform for the OVRPrefab")]
    [Header("=== CurvedUI OVR In Editor Settings ===")]
    [Header("")]

    public CurvedUISettings curvedUIToUse;
    public Vector3 InEditorOVRPrefabTransform;
    public CurvedUIInputModule.CUIControlMethod InEditorCUIInputMethod; 

    
    void Start()
    {
        #if UNITY_EDITOR
            this.transform.position = InEditorOVRPrefabTransform;
            curvedUIToUse.ControlMethod = InEditorCUIInputMethod;
        #endif
    }

}
