using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using RenderHeads.Media.AVProVideo;
using SimpleJSON;
using System.Threading.Tasks;
using Amazon.S3.Transfer;

//using AWS;

using UnityEngine.Serialization;

public static class LoopCounter
{
    public static int buildExperienceList;

}

public delegate void callback<T>(T args);

public class AppManager : MonoBehaviour
{
    public bool initialised = false;

    [Header("=== Debug settings  ========================================")]
    //Show debug logs?
    public bool ShowAppDebugLogs = true;

    [Header("=== Local Content Options ==================================")]
    
    //is it Just Local Content? if true, just assume local content is fine and no downloading is required.
//    public bool JustLocalContent = false;
    // public LocalContentManager LocalContentManager;

    [Header("Local storage folder")]
    [Tooltip("The folder to use inside wherever Unity and the platform OS decide is Application.persistentDataPath. Default is \"assets\".")]
    public const string FolderOnLocalStorageToUse = "assets";

    public string LocalStorage
    {
        // /assets/
        get {
            //Debug.Log("We're get setting baby");
            //Debug.Log("app data path " + Application.persistentDataPath + " Folder on localStorage To use");
            //Debug.Log("returned will be " + Application.persistentDataPath + FolderOnLocalStorageToUse);

            return Path.Combine(path1: Application.persistentDataPath, path2: FolderOnLocalStorageToUse);
        }
    }

    public const string requiredPath = "Required";
    public const string contentPath = "Content";

    //========================================================================================================



    //a list of local and remote entries 
    //remote listing - everything on AWS bucket
    public List<string[]> RemoteFileList = new List<string[]>();



    [Header("=== References ==================================")]

    // the managers
    //public ContentManager ContentManager;
    public VideoController VideoController;
    public TimelineIntro TimelineIntro;
    public GameObject ArtAndEffects;
    public UIManager UIManager;
    public GameObject VideoPlayerComponent;

    public GameObject     CarouselGameObject;
    public Transform     CarouselScrollArea;
    public _PHUI_Carousel CarouselComponent;
    
    public GameObject Selection_TilePrefab;
    public VideoPlayerVolumeControl volumeControlHandler;
    public MediaPlayer mediaPlayer;
    public GameObject videoSphere;
    
    [Header("List of experiences found")]
    //the list of expereinces created from local content, populated during contentManager phase - at app start
    public List<Experience> ExperienceList = new List<Experience>();

    [Header("Skyboxes to use")]
    //list of skybox materials, #1 black for intro/onboarding, #2
    public List<Material> SkyboxMaterials = new List<Material>();


    public enum AppStates
    {
        Loading,
        Onboarding,
        Menu,
        PreExperience,
        Experience
    }
    [SerializeField] protected AppStates currentAppState;

    public UnityEvent LoadingTriggered = new UnityEvent();
    public UnityEvent OnboardingTriggered = new UnityEvent();
//    public UnityEvent MenuTriggered = new UnityEvent();
    public UnityEvent ExperienceTriggered = new UnityEvent();

//    public UnityEvent CarouselBuilt = new UnityEvent();


    public delegate void OnStateChange();
    public event OnStateChange SetStateChanged;

    protected bool MenuFirstTime = true;
    public bool AppFirstTime = true;
    public bool KioskMode = false;
    public bool SkipMenuAndGoIntoPreExperience = false;
    public bool preExperience = false;
    public UnityEvent PrecheckComplete = new UnityEvent();

    public bool UseAnalytics;
    public bool ResetIdentifier = false;
    public string UserIdentifier { get { return SystemInfo.deviceUniqueIdentifier; } }
    public bool IsInternetConnected = false;

    /* END VARS ======================================================================================================================== */

    public static AppManager Instance;
    
    public static bool CheckIfOnline() => Application.internetReachability != NetworkReachability.NotReachable;

    void Awake()
    {
        Instance = this;

        Place.onCatalogueFetched += Initialise;
    }


    public async void Initialise(JSONArray catalogue)
    {
        if (initialised) return;
        initialised = true;

        ExperienceList = Place.Catalogue;
        
//        IsInternetConnected = false;

//        IsInternetConnected = CheckIfOnline();
//        JustLocalContent    = !IsInternetConnected;

//        await CheckIfOnline(callback: (online) => { JustLocalContent = !online; });

//        Debug.Log($"CheckStartUpConnection Initialise");
        
//        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //ContentManager.Init();
//        Application.targetFrameRate = -1;
//        Debug.Log($"OVRManager.sdkVersion:         " + OVRManager.sdkVersion);
//        Debug.Log($"Target frame rate is +         " + Application.targetFrameRate);
//        Debug.Log($"OVRManager.batteryTemperature: " + OVRManager.batteryTemperature);
//        Debug.Log($"OVRManager.gpuUtilSupported:   " + OVRManager.gpuUtilSupported);

        //turn debug logs on or off
//        UnityEngine.Debug.unityLogger.logEnabled = ShowAppDebugLogs;


//        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.4f;
//        OVRPlugin.cpuLevel = 0;
//        OVRPlugin.gpuLevel = 0;
//        OVRManager.cpuLevel = 0;
//        OVRManager.gpuLevel = 0;
//        Debug.Log($"OVRPlugin.cpuLevel: {OVRPlugin.cpuLevel} / OVRPlugin.gpuLevel {OVRPlugin.gpuLevel}");
//        Debug.Log($"OVRManager.cpuLevel: {OVRManager.cpuLevel} / OVRManager.gpuLevel {OVRManager.gpuLevel}");
        //OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;

        //Clearout any orphaned chunks for all locally stored experiences at start up
        //StartCoroutine(ContentManager.Instance.ClearChunks(LocalStorage));

        
        
        TimelineIntro.gameObject.SetActive(true);
//        Debug.Log($"IsInternetConnected = {IsInternetConnected}, timeline going");

        //setting up events for the precheck - some assets like images and description JSON are mandatory for creating the navigation
        //these events will fire when the remote and local file lists have been created 

        UIManager.Fader.FadeIn(1f);
        CheckForFirstTimeRunThrough();

//        Directory.CreateDirectory(LocalStorage);
//        await ContentManager.BuildFolderLists(LocalStorage);

        //this event will fire when the comparison of the lists and subsequent downloads have finished and onboarding can begin

        //Check to see if this version of the app has run before to allow for skip buttons during the app 
    //}


    //// Start is called before the first frame update
    //void Start()
    //{
//        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        AppStart();
    }


//    public async void BuildExperienceList(JSONArray episodes)
//    {
//        foreach (var e in episodes) ExperienceList.Add(new Experience(node: e));
//
//        foreach (var e in ExperienceList)
//        {
//            S3.Download(Place.Paths.Local.Cache,
//                        "mhoplaceappcontent" + '\\' + e.title);
//        }
//    }

    public void ResetIdentifierCode()
    {
        TimelineIntro.ChangeChapter(1);
        ResetIdentifier = true;
        AppStart();
    }

    /*void StartWithAnalyticsCheck()
    {
        TimelineIntro.MotorMoving(false);
        Debug.Log($"[AppManager : StartWithAnalyticsCheck] _{AppManagerPlace.Instance.UseAnalytics}");
        if (AppManagerPlace.Instance.UseAnalytics)
        {
            
            if (!AppManagerPlace.Instance.CheckForUserUserIdentifier() || ResetIdentifier)
            {
                //Debug.Log("Stoping to check analytics or reset");
                UIManager.AnalyticsUI.SetActive(true);
                if (ResetIdentifier)
                {
                    UIManager.AnalyticsUI.GetComponent<AnalyticsUI>().StartCollection();
                }
            }
            else
            {
                AppStart();
            }

        }
        else
        {
            //Debug.Log("Not running analytics, = false");
            AppStart();
        }

    }*/

    public void AppStart()
    {

        //Debug.Log("Starting the app");

        TimelineIntro.MotorMoving(true);
        
        ShowLoading();
        //Debug.Log("Starting?");
        //set intial starting app state and starting sub states
        SubscribeToStateManager();

        //Log analytics - session start
        //Debug.Log("Calling first analytics- start session");
        if (UseAnalytics /*&& !ResetIdentifier*/)
            Analytics.StartSession(UserIdentifier);

        /*if (ResetIdentifier)
            ChangeState(AppStates.Onboarding);
        else*/
            ChangeState(AppStates.Loading);


        ResetIdentifier = false;
        videoSphere.SetActive(false);
    }

    public void SubscribeToStateManager()
    {
        //Debug.Log("Subscribing?");
        LoadingTriggered.AddListener(LoadingTriggeredEvent);
        OnboardingTriggered.AddListener(OnboardingTriggeredEvent);
//        MenuTriggered.AddListener(MenuTriggeredEvent);
        ExperienceTriggered.AddListener(ExperienceTriggeredEvent);

    }


    void CheckForFirstTimeRunThrough()
    {
        //Check to see if this version of the app has run before.  
        //We want the user to see the onboarding and other areas of the app fully the first time.  after they have seen it, it's cool if they 
        //skip it when returning later.  
        if (PlayerPrefs.HasKey("AppFirstTime"))
        {
            //if the playerpref "AppFirstTime" exists, we can assume it's already been run, but we still set/convert 
            //the int value to the bool "AppFirstTime" just in case
            //Debug.Log("This is not our first rodeo " + PlayerPrefs.GetInt("AppFirstTime"));
            //Debug.Log("PlayerPrefs.GetInt(AppFirstTime)" + PlayerPrefs.GetInt("AppFirstTime"));
            AppFirstTime = PlayerPrefs.GetInt("AppFirstTime") != 0;
        }
        else
        {
            // First start because pref does not exist, create pref and set it to 1/true, because we are running first time
            Debug.Log($"[AppManager : CheckForFirstTimeRunThrough] _{PlayerPrefs.GetInt("AppFirstTime")}");
            AppFirstTime = true;
            PlayerPrefs.SetInt(key: "AppFirstTime", value: 1);
            PlayerPrefs.Save();
        }
    }

    /*public bool CheckForUserUserIdentifier()
    {
        //Check to see if user has entered their code.  
        //if so skip to start of app otherwise go through the collection process  
        if (PlayerPrefs.HasKey("UserIdentifier"))
        {
            //if the playerpref "AppFirstTime" exists, we can assume it's already been run, but we still set/convert 
            //the int value to the bool "AppFirstTime" just in case
            
            UserIdentifier = PlayerPrefs.GetString("UserIdentifier");
            UIManager.AnalyticsUI.GetComponent<AnalyticsUI>().UserIdentifierLabel.text = UserIdentifier;
            Debug.Log($"[AppManager : CheckForUserUserIdentifier] _{UserIdentifier}");
            return true;
        }
        else
        {
            // First start because pref does not exist, create pref and set it to 1/true, because we are running first time
            //Debug.Log("There is no user identifier.");
            return false;
        }
    }*/

    public bool RunningAppFirstTime()
    {
        return AppFirstTime;
    }


    public void SetupPlatformEnvironment()
    {
        ShowLoading();
        UIManager.Floor_active();
        UIManager.MenuUI_active();
        HideLoading();
    }

    //###replace These, just use loading UI functions
    public void ShowLoading()
    {
        UIManager.LoadingUI_active();
    }


    public void HideLoading()
    {
        UIManager.LoadingUI_inactive();
    }


    public virtual void ChangeState(AppStates newState)
    {
        //Debug.Log("New state is " + newState);

        currentAppState = newState;

        switch (currentAppState)
        {
            case AppStates.Loading:
                LoadingTriggered.Invoke();

                TimelineIntro.MotorMoving(false);
                SetUpContent();

                break;
            case AppStates.Onboarding:
                OnboardingTriggered.Invoke();
                
                TimelineIntro.MotorMoving(true);
                QuitUI.i.SetState(1);
                break;

            case AppStates.Menu:
//                MenuTriggered.Invoke();
//                Application.targetFrameRate = -1;
                UIManager.ResetUIs();
                if (!UIManager.ArtAndEffects.gameObject.activeInHierarchy)
                    UIManager.ArtAndEffects.gameObject.SetActive(true);

                AppManager.Instance.UIManager.colorSkybox.LerpToTargetColor(topPair: 0, bottomPair: 1, time: 2);
                videoSphere.SetActive(false);

                //Debug.Log("In the menu");
                UIManager.GetComponent<OculusInputListener>().StopListening();
                UIManager.GetComponent<OculusInputListener>().enabled = false;
                //Debug.Log("In the menu");
                TimelineIntro.gameObject.SetActive(false);
                UIManager.MenuUI_active();
                QuitUI.i.SetState(1);
                if (MenuFirstTime)
                {
                    //Debug.Log("MenuFirstTime: " + MenuFirstTime);
                    MenuFirstTime = false;
                    UIManager.Floor_active();
                }
                else
                {
                    UIManager.EnvironmentParticles.SetActive(true);
                    AudioManagerSimple.i.PlayMenuMusic();
                    //Debug.Log("MenuFirstTime: " + MenuFirstTime);
                    if (!preExperience)
                    {
                        UIManager.Floor_active();
                        preExperience = false;
                    }
                }

                //SetupPlatformEnvironment();

                break;


            case AppStates.PreExperience:
                
                break;

            case AppStates.Experience:
                ExperienceTriggered.Invoke();
                Application.targetFrameRate = 60;
                UIManager.GetComponent<OculusInputListener>().enabled = true;
                UIManager.GetComponent<OculusInputListener>().StartListening();
                
                VideoController.PlayVideo();
                videoSphere.SetActive(true);
                UIManager.Fader.FadeIn(inTime: 0.5f, optionalWaitTime: 1f);
                QuitUI.i.SetState(0);
                break;

        }

    }


    //Subscribe to the state manager, and can be inherited to trigger content specific stuff


    public virtual void LoadingTriggeredEvent()
    {
        //Debug.Log("Loading triggered event in base class triggered");
    }

    public virtual void OnboardingTriggeredEvent()
    {
        //Debug.Log("Onboarding triggered event in base class triggered");

    }

    public virtual void MenuTriggeredEvent()
    {
        //Debug.Log("MenuTriggeredEvent triggered event in base class triggered");

    }

    public virtual void ExperienceTriggeredEvent()
    {
        //Debug.Log("ExperienceTriggeredEvent event in base class triggered");

    }

    public void TurnOnSphere()
    {
        videoSphere.SetActive(true);
        AppManager.Instance.UIManager.colorSkybox.LerpFinished.RemoveListener(AppManager.Instance.TurnOnSphere);
    }

    //## this should go to UI Manager - but probably inherited, as specific to swinburne 


    public void ChangeStateExperience()
    {
        ChangeState(AppStates.Experience);
        AppManager.Instance.UIManager.colorSkybox.LerpFinished.AddListener(ChangeStateExperience);
    }

    public void ChangeStateMenu()
    {
        ChangeState(AppStates.Menu);

    }

    public async Task SetUpCarouselMenu()
    {
        foreach (var e in ExperienceList)
        {
            var newTile = Instantiate(original: Selection_TilePrefab,
                                      parent: CarouselScrollArea,
                                      worldPositionStays: false).GetComponent<Selection_Tile>();

            CarouselComponent.tiles.Add(newTile);
            
            await newTile.Populate(e);
        }
        
        UIManager.curvedUI.AddEffectToChildren();
        
        CarouselComponent.InitialiseCarousel();
        
        ChangeState(AppManager.AppStates.Menu);
    }

//    public void InitialiseCarouselOnCarouselBuilt()
//    {
//        CarouselComponent.InitialiseCarousel();
        
//        ChangeState(AppManager.AppStates.Menu);
        
//        CarouselBuilt.RemoveListener(InitialiseCarouselOnCarouselBuilt);
//    }

    //###Media Manager
    //videoName is an optional parameter, used for setting the vid title in some UIs
//    public IEnumerator SetUpVideoForExperience(string path, string basepath, string videoName = null)
//    {
//        var combinedPath = LocalStorage + Path.DirectorySeparatorChar + contentPath + Path.DirectorySeparatorChar + path;
//        
//        //Debug.Log("AppManager : Trying to play a video at: " + path);
////        var combinedPath = Path.Combine(path1: LocalStorage, path2: path); /*'file:/' + _VideoSource;*/
//        
//        Debug.LogError(path);
//        Debug.LogError(combinedPath);
//        
//		yield return null;
//        
//        if (File.Exists(combinedPath))
//        {
//            //RenderSettings.skybox = AppManager.Instance.SkyboxMaterials[1];
//            UIManager.MenuUI_inactive();
//            yield return null;
//            //VideoController.LoadVideoURLFromPath(combinedPath);
//
//            //Debug.Log("AppManager : OpenVideoFromFile: " + MediaPlayer.FileLocation.AbsolutePathOrURL + " - " + combinedPath);
//            mediaPlayer.OpenVideoFromFile(location: MediaPlayer.FileLocation.AbsolutePathOrURL, path: combinedPath, autoPlay: false);
//            yield return null;
//            //Debug.Log("m_Video:" + mediaPlayer.m_VideoPath + " - VideoOpened: " + mediaPlayer.VideoOpened);
//
//            //set the title if there is one
//            if (videoName != null)
//            {
//                VideoController.VideoTitle = videoName;
//                UIManager.VideoPlayerRestartUI.GetComponent<VideoPlayerRestartControl>().InitialiseText(VideoController.VideoTitle);
//            }
//            VideoController.ExperiencePath = basepath;
//        }
//        else
//        {
//            Debug.Log("AppManager : No vid to play, put one in");
//        }
//        yield return null;
//        
//    }


    public void SetUpVideoForExperience(Selection_Tile tile,
                                        string videoFilePath)
    {
        UIManager.MenuUI_inactive();

        mediaPlayer.OpenVideoFromFile(location: MediaPlayer.FileLocation.AbsolutePathOrURL,
                                      path: videoFilePath,
                                      autoPlay: false);

        VideoController.VideoTitle = tile.experience.title;

        UIManager.VideoPlayerRestartUI.GetComponent<VideoPlayerRestartControl>().InitialiseText(VideoController.VideoTitle);
    }


    bool loadingAudio;

//    public void SetUpAudioForExperience(string path, string label, bool volumeControl, bool playOnSetup, bool isForPreExperience = false)
//    {
//
//        //gets string to path for audio to add to player
//        string combinedPath = Path.Combine(path1: LocalStorage + VideoController.ExperiencePath + "audio/", path2: path); /*'file:/' + _VideoSource;*/
//        //Debug.Log("SetUpAudioForExperience : combinedPath : " + combinedPath);
//        //Debug.Log("SetUpAudioForExperience : path : " + path);
//        //Debug.Log("SetUpAudioForExperience : LocalStorage : " + LocalStorage);
//
//        if (File.Exists(combinedPath))
//        {
//            AudioSource audioSource = VideoPlayerComponent.AddComponent<AudioSource>();
//
//            //loadingAudio = false;
//            StartCoroutine(SetUpAudioForPlay(filePath: combinedPath, audio: audioSource, label: label, volumeControl: volumeControl, playOnSetup: playOnSetup, isForPreExperience: isForPreExperience));
//            //send video info, title & duration details
//        }
//        else
//        {
//            //Debug.Log("AppManager : No vid to play, put one in");
//        }
//
//    }

    public void SetUpAudioForExperience(string path, string label, bool volumeControl, bool playOnSetup, bool isForPreExperience = false)
    {

        
        
        //gets string to path for audio to add to player
        string combinedPath = Path.Combine(path1: LocalStorage + VideoController.ExperiencePath + "audio/", path2: path); /*'file:/' + _VideoSource;*/
        //Debug.Log("SetUpAudioForExperience : combinedPath : " + combinedPath);
        //Debug.Log("SetUpAudioForExperience : path : " + path);
        //Debug.Log("SetUpAudioForExperience : LocalStorage : " + LocalStorage);

        if (File.Exists(combinedPath))
        {
            AudioSource audioSource = VideoPlayerComponent.AddComponent<AudioSource>();

            //loadingAudio = false;
            StartCoroutine(SetUpAudioForPlay(filePath: combinedPath, audio: audioSource, label: label, volumeControl: volumeControl, playOnSetup: playOnSetup, isForPreExperience: isForPreExperience));
            //send video info, title & duration details
        }
        else
        {
            //Debug.Log("AppManager : No vid to play, put one in");
        }

    }

    public AudioSource onboarding;

    public IEnumerator/*<WWW>*/ SetUpAudioForPlay(string filePath, AudioSource audio, string label, bool volumeControl, bool playOnSetup, bool isForPreExperience = false)
    {
        while (loadingAudio)
        {
            yield return null;
        }

        loadingAudio = true;
        //gets audio from filepath
        //WWW audioLoader = new WWW("file://" + filePath);
        WWW audioLoader;
        //Debug.Log("SetUpAudioForPlay : About to start audioloading i guess");

        #if UNITY_EDITOR
            audioLoader = new WWW(filePath);

            //Debug.Log("SetUpAudioForPlay : now the file is " + audioLoader.progress);

        #endif

        #if UNITY_ANDROID
                audioLoader = new WWW("file://" + filePath);
        #endif

        while (!audioLoader.isDone)
        {
            //Debug.Log("I'm writing because audioloader is not done" + audioLoader);
            yield return null;
            //}
        }
        //    
        yield return audioLoader;
        //Debug.Log("SetUpAudioForPlay : Now audioloader exists " + audioLoader.progress);

        //Debug.Log(audioLoader.)

        AudioClip aClip = audioLoader.GetAudioClip(threeD: true, stream: true, audioType: AudioType.MPEG);
        aClip.name = label;
        audio.clip = aClip;
        audio.playOnAwake = false;
        //adds to possible 'array' of audio tracks (so can get fed a few)

        //using this to differentiate between audio setup and following action, definitely needs to be rethought

        if (volumeControl)
        {
            //Debug.Log("SetUpAudioForPlay : I'm getting added to the audio listing - Jesse: here is where I think it's going wrong... EDIT no it was actually OK");
            VideoController.audioTracks.Add(audio); 
            volumeControlHandler.AddAudioTrackToControl(audioSource: audio, label: label);

        }
        if (isForPreExperience)
        {
            onboarding = audio;
        }
        if (playOnSetup)
        {
            Debug.Log("SetUpAudioForPlay : Im just doing this once so will probably get replaced at some stage");
            audio.Play();
        }

        //PlayExperience();

        //TODO need to have some sort of event that handles when all audio for play requests are completed, then something fires play

        loadingAudio = false;
    }

    //## local content (downloads/file structure how that effects UI probably)

//    public IEnumerator BuildExperienceList()
//    {
//        //BuildExperienceManifestList();
//
//        //ExperienceList.Clear();
//
//        //string AppPersistentDataPath = Application.persistentDataPath + FolderOnLocalStorageToUse;
//
//        //Debug.Log("Combined path should be " + Path.Combine(Application.persistentDataPath, FolderOnLocalStorageToUse));
//        //Debug.Log("Basic data path is " + Application.persistentDataPath);
//
//        //Debug.Log("App persistent data path going to tile is " + LocalStorage);
//        //Debug.Log(AppPersistentDataPath + " | LocalTopLevelFolders count " + LocalTopLevelFolders.Length);
//
//        foreach (KeyValuePair<string, ContentManager.Folder> folder in ContentManager.remoteDirectory.GetRelativeFolder(requiredPath).subFolders)
//        {
//            //Debug.Log("base path going to tile is " + basePath);
//
//            //locate & readJSON using simpleJSON - thanks Phill!
//            //readin JSON
//
//            var manifestFile = folder.Value.GetFile("manifest.json");
//
//            if (manifestFile == null)
//            {
//                Debug.LogWarning($"No manifest found for [{folder.Value.name}]!");
//                
//                continue;
//            }
//            
//            var combinedPath = Path.Combine(path1: LocalStorage,
//                                            path2: manifestFile.GetAbsolutePath());
//
//            var fileContent = File.ReadAllText(combinedPath);
//
//            var parsedDescription = JSON.Parse(fileContent);
//            
//            //THIS IS THE CURRENTLY BROKEN LINE!!! Is the path correct? Is the file being found?
////            var parsedDescription = JSON.Parse(File.ReadAllText(Path.Combine(LocalStorage, folder.Value.GetRelativeFile("", "manifest.json").GetAbsolutePath())));
//
//            //assign title
//            //assign duration
//            //assign desc
//            var title       = parsedDescription["Title"].Value;
//            var description = parsedDescription["Description"].Value;
//            var duration    = parsedDescription["Duration"].Value;
//            var videoFormat    = parsedDescription["Format"].Value;
//
//            bool isReady = false;
//            //is there a video?
//            //sexpand this logic to suit all media files, vid and audio
//            if (ContentManager.GetMissingFileKeys(folder.Value.GetAbsolutePath()).Count < 1) isReady = true; 
//
//            yield return null;
//
//            //Make new experience object
//            //Add objkect to list
//            ExperienceList.Add(item: new Experience(title: title,
//                                                    description: description,
//                                                    duration: duration,
//                                                    basePath: folder.Key,
//                                                    format: videoFormat));
//        }
//
//        //build and insert tiles into carousel
//        StartCoroutine(BuildCarouselList());
//    }


    public void BuildCarouselList()
    {

        
//        CarouselBuilt.Invoke();
        
//        CarouselBuilt.RemoveAllListeners();
    }


    //### For remote content manager
//    void SetUpContent()
//    {
//        //Debug.Log("AppManager : Awake : LocalStorage = [" + LocalStorage + "]");
//
//        //check for the set local storage path and create it if not there
//        if (!Directory.Exists(LocalStorage))
//        {
//            Directory.CreateDirectory(LocalStorage);
//            //Debug.Log("AppManager : Awake() : ERROR >> Couldn't find local storage, creating: " + LocalStorage);
//        }
//        else
//        {
//            //Debug.Log("AppManager : Awake() : OK >> Found Local storage folder. Nice. - " + LocalStorage);
//        }
//
//        //If using remote content, get the file list
//        if (Place.IsOnline)
//        {
//            if (ContentManager.GetMissingFileKeys(requiredPath).Count > 0)
//            {
//                AWS.S3.Download download = new AWS.S3.Download(localFolderPath: Path.Combine(path1: LocalStorage, path2: requiredPath), remoteFolderPath: requiredPath);
//                download.onDownloadProgress += (object sender, AWS.S3.DownloadProgressArgs downloadProgressArgs) =>
//                {
//                    if (downloadProgressArgs.isDone) ChangeState(AppStates.Onboarding);
//                };
//            }
//            else ChangeState(AppStates.Onboarding);
//        }
//    }

    void SetUpContent() => ChangeState(AppStates.Onboarding);

    /*void PreparePrecheck()
    {
        ////Debug.LogFormat("AppManager : PreparePrecheck : List counts - {0} | {1} ", RemoteFileList.Count, LocalFileList.Count);

        if ((!JustLocalContent) && (RemoteFileList.Count != 0))
        {
            Debug.Log("AppManager : PreparePrecheck : 1st condition, remote and local content and need to compare. Number of remote files: "+ RemoteFileList.Count);
            StartCoroutine(LocalContentManager.CompareRemoteAndLocalFileLists(RemoteFileList, LocalFileList));
            //stop timeline
            PrecheckComplete.Invoke();
            Debug.Log("AppManager : PreparePrecheck : (1) PrecheckComplete Invoked.");
        }
        else if (JustLocalContent && (LocalFileList.Count != 0))
        {
            Debug.Log("AppManager : PreparePrecheck : 2nd condition, just local content... all good. Found files: "+ LocalFileList.Count);

            PrecheckComplete.Invoke();
            Debug.Log("AppManager : PreparePrecheck : (2) PrecheckComplete Invoked.");
        }
        else if (JustLocalContent && (IsInternetConnected==false))
        {
            Debug.Log("AppManager : PreparePrecheck : 3nd condition, just local content... not internet. Found files: " + LocalFileList.Count);

            PrecheckComplete.Invoke();
        }
        else
        {
            Debug.Log("AppManager : PreparePrecheck : 4rd conition, Waiting on Local and Remote content... both need to be completed.");
        }
    }

    void ReadyToStartOnboarding()
    {
        ChangeState(AppStates.Onboarding);
        LocalContentManager.LocalListCreated.RemoveAllListeners();
        AWS.RemoteListCreated.RemoveAllListeners();
        PrecheckComplete.RemoveAllListeners();
		ChangeState(AppStates.Onboarding);
    }*/

    public void DownloadExperience(string path, GameObject initiator)
    {

        Debug.Log("AppManager : Trying to download a video at: " + path);
        //AWS.Instance.DownloadItemFromPath(path, initiator);
        //currentDownload = Path.Combine(ContentManager.Instance.FolderOnLocalStorageToUse,path);
    }
}