using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Easy.Utilities
{
    public class Version : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (VersionSettings.instance.updateVersionOnBuild)
            {
                VersionSettings.instance.Increment();
                PlayerSettings.bundleVersion = VersionSettings.instance.ToString();

                if (VersionSettings.instance.incrementPlatformBundleNumber) IncrementBuildNumber(report.summary.platform);

                EditorUtility.SetDirty(VersionSettings.instance);
                AssetDatabase.SaveAssets();
            }
        }

        private void IncrementBuildNumber(BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.Android:
                    PlayerSettings.Android.bundleVersionCode++;
                    break;

                default:
                    throw new System.NotImplementedException(platform.ToString());
            }
        }
    }
}
