using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System.Reflection;

public static class Analytics
{
    public enum Type { Session, Download, Video };

    [DynamoDBTable("Place")][Serializable]
    public class Analytic
    {
        public string UserID;
        public string UnixStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks.ToString();

        public string SessionID;
        public string SessionStartTimestamp = "---";
        public string SessionEndTimestamp = "RUNNING";
        public string SessionDuration = "RUNNING";

        public string DownloadName = "---";
        public string DownloadStartTimestamp = "---";
        public string DownloadEndTimestamp = "---";
        public string DownloadStartProgress = "---";
        public string DownloadEndProgress = "---";
        public string DownloadTime = "---";
        public string DownloadCanceled = "---";

        public string VideoWatchOrder = "---";
        public string VideoName = "---";
        public string VideoDifficulty = "---";
        public string VideoGender = "---";
        public string VideoStartTimestamp = "---";
        public string VideoEndTimestamp = "---";
        public string VideoPlayTime = "---";
        public string VideoExitedEarly = "---";

        public string DevicePlatform = "v" + Application.version + "_" + Application.platform.ToString() + "_" + SystemInfo.operatingSystem;

        public Analytic() { }

        public Analytic(Type type)
        {
            switch (type)
            {
                case Type.Session:
                    break;
                case Type.Download:
                    DownloadEndTimestamp = "DOWNLOADING";
                    DownloadEndProgress = "DOWNLOADING";
                    DownloadTime = "DOWNLOADING";
                    DownloadCanceled = "DOWNLOADING";
                    break;
                case Type.Video:
                    VideoEndTimestamp = "UNEXPECTED END";
                    VideoPlayTime = "UNEXPECTED END";
                    VideoExitedEarly = "UNEXPECTED END";
                    break;
            }
        }

        public Analytic(string csv)
        {
            string[] values = csv.Split(',');

            if (values.Length == 22)
            {
                UserID = values[0];
                UnixStamp = values[1];

                SessionID = values[2];
                SessionStartTimestamp = values[3];
                SessionEndTimestamp = values[4];
                SessionDuration = values[5];

                DownloadName = values[6];
                DownloadStartTimestamp = values[7];
                DownloadEndTimestamp = values[8];
                DownloadStartProgress = values[9];
                DownloadEndProgress = values[10];
                DownloadTime = values[11];
                DownloadCanceled = values[12];

                VideoWatchOrder = values[13];
                VideoName = values[14];
                VideoDifficulty = values[15];
                VideoGender = values[16];
                VideoStartTimestamp = values[17];
                VideoEndTimestamp = values[18];
                VideoPlayTime = values[19];
                VideoExitedEarly = values[20];

                DevicePlatform = values[21];
            }
            else Debug.LogWarning($"Analytics value length {values.Length} doesn't match required length 22.");
        }

        public override string ToString()
        {
            return $"{UserID},{UnixStamp},{SessionID},{SessionStartTimestamp},{SessionEndTimestamp},{SessionDuration},{DownloadName},{DownloadStartTimestamp},{DownloadEndTimestamp},{DownloadStartProgress},{DownloadEndProgress},{DownloadTime},{DownloadCanceled},{VideoWatchOrder},{VideoName},{VideoDifficulty},{VideoGender},{VideoStartTimestamp},{VideoEndTimestamp},{VideoPlayTime},{VideoExitedEarly},{DevicePlatform}";
        }

        public Dictionary<string, AttributeValue> ToDictionary()
        {
            Dictionary<string, AttributeValue> keyValuePairs = new Dictionary<string, AttributeValue>();

            foreach(PropertyInfo property in GetType().GetProperties())
            {
                keyValuePairs.Add(property.Name, new AttributeValue(property.GetValue(this, null).ToString()));
            }

            return keyValuePairs;
        }
    }

    const string tableName = "Place";

    static string UserID;
    static string SessionID;
    static int VideoWatchOrder = 0;

    static Analytic session = null;
    static DateTime sessionStartTime;

    public static Analytic currentDownload = null;
    static DateTime currentDownloadStartTime;

    public static Analytic currentVideo = null;
    static DateTime currentVideoStartTime;

    static string pingTargetIP = "8.8.8.8";
    static int pingTimeoutMilliseconds = 3000;

    static string backlogPath = Path.Combine(Application.persistentDataPath, "backlog.bin");

    //Time & Date formatting
    // mm/dd or dd/mm or something else?? change this string to choose a format
    static string SpecifiedDateFormat = "MM/dd/yyyy HH:mm:ss";

    static Analytics()
    {
        if (!File.Exists(backlogPath)) File.Create(backlogPath);

        _ = CheckIfOnline();
    }

    static public void StartSession(string userID)
    {
        if (session == null)
        {
            sessionStartTime = DateTime.Now;

            UserID = userID;
            SessionID = (sessionStartTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks.ToString();

            session = new Analytic(0);
            session.UserID = UserID;
            session.SessionID = SessionID;

            //format date to specified format
            session.SessionStartTimestamp = sessionStartTime.ToString(SpecifiedDateFormat);

            _ = TrySendAnalytic(session);
        }
        else
            Debug.LogWarning("[Analytics : StartSession] The session has already been started.");
    }

    static public async Task EndSession(bool isForceQuit = true)
    {
        if (session != null)
        {
            DateTime currentTime = DateTime.Now;

            //format date to specified format
            session.SessionEndTimestamp = DateTime.Now.ToString(SpecifiedDateFormat);
            
            //format time to only show ms to 2 places
            TimeSpan formattedDuration = (currentTime - sessionStartTime);
            session.SessionDuration = formattedDuration.Hours.ToString("D2") + ":" + formattedDuration.Minutes.ToString("D2") 
                                        + ":" + formattedDuration.Seconds.ToString("D2") + "." + formattedDuration.Milliseconds.ToString("D1");

            if (isForceQuit) WriteAnalytic(session);
            else TrySendAnalytic(session);
        }
        else
            Debug.LogWarning("[Analytics : EndSession] The session was never started. Make sure to call StartSession at the start of your app.");
    }

    static public void ContinueSession()
    {
        if (session != null)
        {
            session.SessionEndTimestamp = "RUNNING";
            session.SessionDuration = "RUNNING";

            _ = TrySendAnalytic(session);
        }
        else
            Debug.LogWarning("[Analytics : ContinueSession] The session was never started. Make sure to call StartSession at the start of your app.");
    }

    static public void StartDownload(string downloadName, string downloadProgress)
    {
        if (currentDownload == null)
        {
            currentDownloadStartTime = DateTime.Now;

            currentDownload = new Analytic(Type.Download);
            currentDownload.UserID = UserID;
            currentDownload.SessionID = SessionID;
            currentDownload.SessionStartTimestamp = session.SessionStartTimestamp;
            currentDownload.DownloadName = downloadName;
            currentDownload.DownloadStartTimestamp = currentDownloadStartTime.ToString(SpecifiedDateFormat);
            currentDownload.DownloadStartProgress = downloadProgress;

            _ = TrySendAnalytic(currentDownload);
        }
        else
            Debug.LogWarning("[Analytics : StartDownload] There is already a download in progress.");
    }

    static public void EndDownload(bool downloadCanceled, string downloadProgress)
    {
        if (currentDownload != null)
        {
            DateTime currentTime = DateTime.Now;

            currentDownload.DownloadEndTimestamp = currentTime.ToString(SpecifiedDateFormat);

            //format time to only show ms to 2 places
            TimeSpan formattedDuration = (currentTime - currentDownloadStartTime);
            currentDownload.DownloadTime = formattedDuration.Hours.ToString("D2") + ":" + formattedDuration.Minutes.ToString("D2")
                                        + ":" + formattedDuration.Seconds.ToString("D2") + "." + formattedDuration.Milliseconds.ToString("D1");

            currentDownload.DownloadEndProgress = downloadProgress;
            currentDownload.DownloadCanceled = downloadCanceled ? "Yes" : "No";

            Analytic temporaryCurrentDownloadDuplicate = new Analytic(currentDownload.ToString()); // Needed to prevent Async / value nulling bugs.
            currentDownload = null;

            _ = TrySendAnalytic(temporaryCurrentDownloadDuplicate);
        }
        else
            Debug.LogWarning("[Analytics : EndDownload] A download was never started. Make sure to call StartDownload at the start of a download.");
    }

    static public void StartVideo(string videoName, string videoDifficulty, string videoGender)
    {
        if (currentVideo == null)
        {
            VideoWatchOrder++;
            currentVideoStartTime = DateTime.Now;

            currentVideo = new Analytic(Type.Session);
            currentVideo.UserID = UserID;
            currentVideo.SessionID = SessionID;
            currentVideo.SessionStartTimestamp = session.SessionStartTimestamp;
            currentVideo.VideoWatchOrder = VideoWatchOrder.ToString();
            currentVideo.VideoName = videoName;
            //format date to specified format
            currentVideo.VideoStartTimestamp = currentVideoStartTime.ToString(SpecifiedDateFormat);
            currentVideo.VideoDifficulty = videoDifficulty;
            currentVideo.VideoGender = videoGender;

            _ = TrySendAnalytic(currentVideo);
        }
        else
            Debug.LogWarning("[Analytics : StartVideo] There is already a video in progress.");
    }

    static public void EndVideo(bool videoInterupted, bool nullVideo = false, bool skipOnlineCheck=true)
    {
        if (currentVideo != null)
        {
            DateTime currentTime = DateTime.Now;

            //format date to specified format
            currentVideo.VideoEndTimestamp = currentTime.ToString(SpecifiedDateFormat);

            //format time to only show ms to 2 places
            TimeSpan formattedDuration = (currentTime - currentVideoStartTime);
            currentVideo.VideoPlayTime = formattedDuration.Hours.ToString("D2") + ":" + formattedDuration.Minutes.ToString("D2")
                                        + ":" + formattedDuration.Seconds.ToString("D2") + "." + formattedDuration.Milliseconds.ToString("D1");

            currentVideo.VideoExitedEarly = videoInterupted ? "Yes" : "No";

            Analytic temporaryCurrentVideoDuplicate = new Analytic(currentVideo.ToString());
            currentVideo = null;

            _ = TrySendAnalytic(temporaryCurrentVideoDuplicate);
        }
        else
            Debug.LogWarning("[Analytics : EndVideo] There is no video in progress. Make sure to call StartVideo when you start a video.");
    }

    static bool CheckIfOnline(callback<bool> callback = null)
    {
        bool onlineCheck = AppManager.CheckIfOnline();
//        await onlineCheck;

        if (onlineCheck) SendBacklog();

        return onlineCheck;
    }

    static bool TrySendAnalytic(Analytic analytic)
    {
        var onlineCheck = CheckIfOnline();
        
//        await onlineCheck;

        if (onlineCheck) SendAnalytic(analytic);
        else WriteAnalytic(analytic);

        return onlineCheck;
    }

    static void SendAnalytic(Analytic analytic)
    {
        _ = AWS.DynamoDBv2.PutItemAsync(tableName, analytic.ToDictionary());
    }

    static void WriteAnalytic(Analytic analytic)
    {   
        File.AppendAllLines(backlogPath, new[] { analytic.ToString() });
    }

    static void SendBacklog()
    {
        string[] lines = File.ReadAllLines(backlogPath);

        foreach(string line in lines)
        {
            Analytic analytic = new Analytic(line);
            SendAnalytic(analytic);
        }
        
        File.WriteAllBytes(backlogPath, new byte[] { });
    }
}
