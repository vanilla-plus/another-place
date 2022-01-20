using System;
using System.Text.RegularExpressions;

namespace Easy.Utilities
{
    public class VersionSettings : ScriptableSingleton<VersionSettings>
    {
        public bool updateVersionOnBuild = false;
        public bool incrementPlatformBundleNumber = false;
        public int year = 0, month = 0, day = 0, build = 0;

        public void Increment()
        {
            DateTime dateTime = DateTime.UtcNow;

            if (dateTime.Year == year && dateTime.Month == month && dateTime.Day == day) build++;
            else build = 1;

            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
        }

        public override string ToString()
        {
            return $"{year}.{month}.{day}.{build}";
        }
    }
}
