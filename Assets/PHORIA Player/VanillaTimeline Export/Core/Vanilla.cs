using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Vanilla
{
    // ---------------------------------------------------------------------------------------------------------------/
    // Enums
    // ---------------------------------------------------------------------------------------------------------------/
    
    [Flags]
    public enum DebugFlags {
        Logs = 1,
        Warnings = 2,
        Errors = 4,
        Gizmos = 8,
    }
    
    // ---------------------------------------------------------------------------------------------------------------/
    // Custom Events
    // ---------------------------------------------------------------------------------------------------------------/
    
    public class IntEvent : UnityEvent<int> {}
}