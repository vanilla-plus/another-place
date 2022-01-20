using UnityEngine;

using Vanilla;

namespace Vanilla
{
    public class VanillaTimelineMotor : VanillaBehaviour
    {

        public float timeMultiplier = 1.0f;

        //[HideInInspector]
        public VanillaTimeline timeline;

        void OnEnable()
        {
            timeline = GetComponent<VanillaTimeline>();
        }

        void Update()
        {
            if (timeline)
            {
                timeline.AddTime(Time.deltaTime * timeMultiplier);
            }
        }
    }
}