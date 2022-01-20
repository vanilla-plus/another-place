using UnityEngine;

using System;

using Vanilla;
using Vanilla.Math;


namespace Vanilla
{
//#if UNITY_EDITOR
//    using UnityEditor;

//    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
//    public class ReadOnlyAttributeDrawer : PropertyDrawer
//    {
//        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
//        {
//            bool wasEnabled = GUI.enabled;
//            GUI.enabled = false;
//            EditorGUI.PropertyField(rect, prop, true);
//            GUI.enabled = wasEnabled;
//        }
//    }
//#endif

//    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
//    public class ReadOnlyAttribute : PropertyAttribute { }

    public class VanillaTimeline : VanillaBehaviour
    {
        [AttributeUsage(AttributeTargets.Field, Inherited = true)]
        public class ReadOnlyAttribute : PropertyAttribute { }



        [Header("[ Vanilla Timeline ]")] public bool allowTimelineToLoop;

        [Tooltip(
            "If this timeline is configured to loop, when we reach either end of the timeline, should we run the opening and closing milestones or isolate them? For example, if you cycled forwards from chapter 3 back to chapter 0, should we invoke milestone 0 after invoking milestone 4? If not ticked, only milestone 4 would be run. This is useful to do in certain cases.")]
        public bool inclusiveMilestoneMode;

        [SerializeField, ReadOnly] private float _t;

        public float t
        {
            get { return _t; }
            set
            {
                //Log("t changed from [{0}] changed to [{1}]", _t, value);

                _t = allowTimelineToLoop ? Mathf.Repeat(value, timelineLength) : Mathf.Clamp(value, 0, timelineLength);


                //Frame(_t - chapters[currentChapter].startTime / chapters[currentChapter].timeInSeconds);
            }
        }

        [SerializeField, ReadOnly] public float timelineLength;

        [SerializeField, ReadOnly]
        [Tooltip("Is the time passed to us positive? i.e. Is the timeline heading forwards or backwards?")]
        public bool timeDirectionIsPositive;

        public enum TimelineState
        {
            Start,
            Middle,
            Finish
        }

        [SerializeField, ReadOnly] public TimelineState timelineState;

        [SerializeField, ReadOnly] private int _currentChapter;

        public int currentChapter
        {
            get { return _currentChapter; }
            set
            {
                if (value != _currentChapter)
                {
                    if (timeDirectionIsPositive)
                    {
                        if (value >= chapters.Length)
                        {
                            if (allowTimelineToLoop)
                            {
                                _currentChapter = 0;

                                MilestoneReached(chapters.Length);

                                if (inclusiveMilestoneMode)
                                {
                                    MilestoneReached(0);
                                }

                                timelineState = TimelineState.Middle;
                            }
                            else
                            {
                                _currentChapter = chapters.Length - 1;

                                MilestoneReached(chapters.Length);

                                timelineState = TimelineState.Finish;
                            }
                        }
                        else
                        {
                            _currentChapter = value;

                            MilestoneReached(value);

                            timelineState = TimelineState.Middle;
                        }
                    }
                    else
                    {
                        if (value <= -1)
                        {
                            if (allowTimelineToLoop)
                            {
                                _currentChapter = chapters.Length - 1;

                                MilestoneReached(0);

                                if (inclusiveMilestoneMode)
                                {
                                    MilestoneReached(chapters.Length);
                                }

                                timelineState = TimelineState.Middle;
                            }
                            else
                            {
                                _currentChapter = 0;

                                MilestoneReached(0);

                                timelineState = TimelineState.Start;
                            }
                        }
                        else
                        {
                            _currentChapter = value;

                            MilestoneReached(value + 1);

                            timelineState = TimelineState.Middle;
                        }
                    }

                    currentChapterName = chapters[_currentChapter].name;

                    //_currentChapter = Mathf.Clamp(value, 0, chapters.Length - 1);

                    //UpdateChapterTimeMultiplier();

                    //Log("Chapter changed to [{0}]", _currentChapter);
                }
            }
        }

        [SerializeField, ReadOnly] private float _chapterNormal;

        public float chapterNormal
        {
            get { return _chapterNormal; }
            set { _chapterNormal = value; }
        }

        [SerializeField, ReadOnly] private float _timelineNormal;

        public float timelineNormal
        {
            get { return _timelineNormal; }
            set { _timelineNormal = value; }
        }

        [ReadOnly] public string currentChapterName;

        [SerializeField] public TimelineChapter[] chapters;

        [Serializable]
        public struct TimelineChapter
        {
            public string name; // What happens during this chapter?

            public float timeInSeconds; // How much time should this chapter take? (inclusive of the timeInSeconds)

            public InterpolationType interpolationType;

            [ReadOnly,HideInInspector] public float startTime; // How much time has elapsed by the start of the chapter?

            [ReadOnly,HideInInspector] public float endTime; // How much time has elapsed by the end of the chapter?
        }

        public virtual void Start()
        {
            AnalyzeTimeline();
        }

        public void AnalyzeTimeline()
        {
            timelineLength = GetTotalChapterTime();

            float l = 0;

            for (int i = 0; i < chapters.Length; i++)
            {
                chapters[i].startTime = l;

                l += chapters[i].timeInSeconds;

                chapters[i].endTime = l;
            }
        }

        public virtual void MilestoneReached(int milestone)
        {
            Log($"Milestone reached! [{milestone}]");
        }

        // When changing the currentChapter value, we can re-use the TimelineState enum in a different context (since it has the same layout) to get around a switch limitation.
        // This is only used when calculating what to do with the currentChapter
        TimelineState ChapterToTimelineState(int incomingChapter)
        {
            return incomingChapter == 0 ? TimelineState.Start :
                incomingChapter == chapters.Length - 1 ? TimelineState.Finish : TimelineState.Middle;
        }

        public void AddTime(float delta)
        {
            if (delta == 0 || chapters.Length == 0)
            {
                return;
            }

            timeDirectionIsPositive = Mathf.Sign(delta) == 1;

            if (timeDirectionIsPositive)
            {
                if (!allowTimelineToLoop && timelineState == TimelineState.Finish)
                {
                    return;
                }
            }
            else
            {
                if (!allowTimelineToLoop && timelineState == TimelineState.Start)
                {
                    return;
                }
            }

            timelineState = TimelineState.Middle;

            // If we're going forward through time this frame...
            if (timeDirectionIsPositive)
            {
                // We could run just one while loop here that does a fair bit of calculatin', but its a bit easier to split it up into two checks.

                // The first here checks only the remaining time of the current chapter relative to t
                if ((t + delta) > chapters[currentChapter].endTime)
                {
                    // Subtract the remaining chapter time from the delta
                    delta -= chapters[currentChapter].endTime - t;

                    // Set t to the end of this chapter
                    t = chapters[currentChapter].endTime;

                    IncrementCurrentChapter();

                    // The second does a slightly less complicated comparison to see if delta is larger than any following chapters.
                    // If so, take the time off delta and add it to t.
                    while (delta > chapters[currentChapter].timeInSeconds)
                    {
                        delta -= chapters[currentChapter].timeInSeconds;

                        t = chapters[currentChapter].endTime;

                        IncrementCurrentChapter();
                    }
                }

                t += delta;
            }
            else
            {
                // Delta is negative here, so we have to be a bit careful when adding/comparing

                // First of all, would this drop below the chapter startTime?
                if ((t + delta) < chapters[currentChapter].startTime)
                {
                    // Subtract the remaining chapter time from the delta
                    delta -= t - chapters[currentChapter].startTime;

                    // Set t to the end of this chapter
                    t = chapters[currentChapter].startTime;

                    DecrementCurrentChapter();

                    // The second does a slightly less complicated comparison to see if delta is larger than any following chapters.
                    // If so, take the time off delta and add it to t.
                    while (delta < -chapters[currentChapter].timeInSeconds)
                    {
                        delta += chapters[currentChapter].timeInSeconds;

                        t = chapters[currentChapter].startTime;

                        DecrementCurrentChapter();
                    }
                }

                t += delta;
            }

            timelineNormal = _t / timelineLength;
            chapterNormal = (t - chapters[currentChapter].startTime) / chapters[currentChapter].timeInSeconds;

            TimelineUpdate();
        }

        public virtual void TimelineUpdate()
        {
            AnalyzeTimeline();
        }

        void IncrementCurrentChapter()
        {
            //currentChapter = allowTimelineToLoop ? currentChapter+1 : Mathf.Clamp(currentChapter+1, 0, chapters.Length-1);
            currentChapter++;
        }

        void DecrementCurrentChapter()
        {
            //currentChapter = allowTimelineToLoop ? currentChapter-1 : Mathf.Clamp(currentChapter - 1, 0, chapters.Length - 1);
            currentChapter--;
        }

        int GetPreviousChapterID()
        {
            return currentChapter <= 0 ? allowTimelineToLoop ? chapters.Length - 1 : 0 : currentChapter - 1;
        }

        int GetNextChapterID()
        {
            return currentChapter >= chapters.Length - 1
                ? allowTimelineToLoop ? 0 : chapters.Length - 1
                : currentChapter + 1;
        }

        //int GetNextChapter() {
        //	if (currentChapter >= chapters.Length - 1) {
        //		if (allowTimelineToLoop) {
        //			return 0;
        //		}
        //	}

        //	return currentChapter + 1;
        //}

        float GetTotalChapterTime()
        {
            float o = 0;

            for (int i = 0; i < chapters.Length; i++)
            {
                o += chapters[i].timeInSeconds;
            }

            return o;
        }

        public float InterpolatedChapterNormal()
        {
            return VanillaMath.Interpolate(chapterNormal, chapters[currentChapter].interpolationType);
        }
    }
}