using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Amazon.S3.Transfer;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Vanilla.StringFormatting;

[Serializable]
public class ULongEvent : UnityEvent<ulong> { }

[Serializable]
public class FloatEvent : UnityEvent<float> { }

[Serializable]
public class BoolEvent : UnityEvent<bool> { }

public class Selection_Tile : MonoBehaviour
{

//    private Experience _Experience;

    [Header("Experience")]
    [SerializeField]
    public Experience experience;
//    {
//        get => _Experience;
//        set
//        {
//            _Experience = value;
//            
//            Debug.Log($"Selection tile populated with Experience [{value.experienceTitle}]");
//        
//            SelectionHeading.text     = value.experienceTitle;
//            SelectionDuration.text    = value.experienceDuration;
//            SelectionDescription.text = value.experienceDescription;
//        
//            SetThumbnail();
//        }
//    }

    [Header("Experience")]
    public  RectTransform rectTrans;
    private Button        _button;
    
    [Header("Selection UI")]
    public TextMeshProUGUI SelectionHeading;
    public TextMeshProUGUI SelectionDuration;
    public TextMeshProUGUI SelectionDescription;
    public Image SelectionThumbnail;

    [Header("Notification UI")]
    public GameObject NotificationPanel;
    public TextMeshProUGUI NotificationText;
    public Image NotificationIcon;
    public Sprite[] NotificationIconList;
    public GameObject RemovalPanel;

    [Header("Action Panel")]
    public GameObject ActionPanel;
    public Animator   ActionPanelAnimator;

    public                   TextMeshProUGUI       FileSizeText;
    public                   TextMeshProUGUI       DownloadProgressText;
    public                   float                 downloadProgress, clampedDownloadProgress;
    [SerializeField] private float                 InitialProgressValue;
    public                   PHUI_Progress_Spinner DownloadProgressSpinner;

    [Header("Download UI")]
    public TextMeshProUGUI DownloadInstruction;
    public GameObject DownloadButton;
    public TextMeshProUGUI DownloadDisclaimer;
    public TextMeshProUGUI DownloadStorageMessage;
    public TextMeshProUGUI DownloadBusyMessage;
    public TextMeshProUGUI DownloadNoConnectionMessage;

//    private string SelectionBasePath;
//    public bool SelectionIsReady;
    public bool SelectionIsDownloading;
    public Animator TileAnimator;
    public string TotalDownloadFileSize;
    
    private _PHUI_Carousel ParentCarousel;

    int lastTone = 2;

    [Header("Remote")]
    [Header("File Management")]

//    [SerializeField]
//    private string _remoteContentPath;
//    public string remoteContentPath => _remoteContentPath;

    [SerializeField]
    public ContentManager.Folder localContentFolder;
    
    [SerializeField]
    public ContentManager.Folder remoteContentFolder;

    [SerializeField]
    private ulong _remoteFolderSize;
    public ulong remoteFolderSize
    {
        get => _remoteFolderSize;
        set
        {
            _remoteFolderSize = value;
            
            onRemoteFolderSizeUpdate.Invoke(_remoteFolderSize);
        }
    }

    public FloatEvent onRemoteFolderSizeUpdate = new FloatEvent();

    public bool remoteFileExists;

//    [Header("Local")]

//    [SerializeField]
//    private string _localContentPath;
//    public string localContentPath => _localContentPath;

    public ulong LocalFolderSize => localContentFolder.GetBytes();

//    [SerializeField]
//    private ulong _localFolderSize;
//    public ulong localFolderSize
//    {
//        get => _localFolderSize;
//        set
//        {
//            _localFolderSize = value;
//            
//            onLocalFolderSizeUpdate.Invoke(_localFolderSize);
//        }
//    }
//
//    public FloatEvent onLocalFolderSizeUpdate = new FloatEvent();

    [Header("Download")]
    [SerializeField]
    private ulong _downloadBytesRemaining = 0;
    public ulong downloadBytesRemaining
    {
        get => _downloadBytesRemaining;
        set
        {
            _downloadBytesRemaining = value;
            
            onDownloadBytesUpdate.Invoke(_downloadBytesRemaining);
            onDownloadPercentUpdate.Invoke((float)_downloadBytesRemaining / _remoteFolderSize);
        }
    }

    public UnityEvent onDownloadBegun    = new UnityEvent();
    public UnityEvent onDownloadComplete = new UnityEvent();
        
    public ULongEvent onDownloadTotalUpdate   = new ULongEvent();
    public FloatEvent onDownloadPercentUpdate = new FloatEvent();
    public ULongEvent onDownloadBytesUpdate   = new ULongEvent();

    public enum TileStates
    {
        idle,
        active,
        inactive,
        selected
    }

    public enum ContentStates
    {
        readyToPlay,
        needToDownload,
        preparingDownload,
        currentlyDownloading,
        processDownload,
        settings_confirmRemoval,
        settings_removingExperience,
        settings_experienceRemoved
    }

    public enum DownloadStates
    {
        hideAll,
        downloaderBusy,
        storageFull,
        readyToDownload,
        noConnection
    }


    //move the progress logic into here from download manager    

//    public string folderName;
//    public string videoPath =>
//        Path.Combine(Experience.basePath,
//                     "video/video-oculus",
//                     Experience.videoFormat);

    

    public TileStates currentTileState;

    private TileStates previousTileState;

    public ContentStates currentContentState;

    public DownloadStates currentDownloaderState;

    public AWS.S3.Download download;
    
    // Magic string <-> hashes
    
    private static readonly int             HighlightEnter             = Animator.StringToHash("highlightEnter");
    private static readonly int             HighlightExit              = Animator.StringToHash("highlightExit");
    private static readonly int             ReadyToPlay                = Animator.StringToHash("readyToPlay");
    private static readonly int             NeedToDownload             = Animator.StringToHash("needToDownload");
    private static readonly int             PreparingDownload          = Animator.StringToHash("preparingDownload");
    private static readonly int             CurrentlyDownloading       = Animator.StringToHash("currentlyDownloading");
    private static readonly int             ProcessDownload            = Animator.StringToHash("processDownload");
    private static readonly int             SettingsConfirmRemoval     = Animator.StringToHash("settings_confirmRemoval");
    private static readonly int             SettingsRemovingExperience = Animator.StringToHash("settings_removingExperience");
    private static readonly int             SettingsExperienceRemoved  = Animator.StringToHash("settings_experienceRemoved");

    private static char _slash => Path.DirectorySeparatorChar;

    // Static subpaths

    private static string _localRootSubpath => Application.persistentDataPath;
    
    private static readonly string _assetsSubpath = _slash + "assets";
    
    private static readonly string _contentSubpath  = _slash + "Content";
    private static readonly string _requiredSubpath = _slash + "Required";

    private static readonly string _audioSubpath = _slash + "audio";
    private static readonly string _videoSubpath = _slash + "video";

    private static readonly string _thumbnailSubpath = _slash + "thumbnail" + c_FileExt_JPG;

    private static readonly string _manifestSubpath = _slash + "manifest" + c_FileExt_JSON;

    private string _videoFileSubpath => _slash + "video-oculus" + experience.videoFormat;

    // Dynamic subpaths

    private string _episodeSubpath => _slash + experience.basePath;

    // Static paths

    // Dynamic paths

    private string _localRequiredPath => _localRootSubpath + _assetsSubpath + _requiredSubpath;
    private string _localContentPath  => _localRootSubpath + _assetsSubpath + _contentSubpath;

    private string _localEpisodePath => _localContentPath + _episodeSubpath;
    
    private string _localManifestPath  => _localRequiredPath + _episodeSubpath + _manifestSubpath;
    private string _localThumbnailPath => _localRequiredPath + _episodeSubpath + _thumbnailSubpath;
    
    private string _localVideoPath => _localEpisodePath + _videoSubpath;
    private string _localAudioPath => _localEpisodePath + _audioSubpath;

    private string _localVideoFilePath => _localVideoPath + _videoFileSubpath;
    
    private string _relativeContentPath => _contentSubpath + _episodeSubpath;

    // File Extensions - Text

    private const string c_FileExt_JSON = ".json";
    
    // File Extensions - Image

    private const string c_FileExt_JPG  = ".jpg";
    private const string c_FileExt_PNG  = ".png";
    
    // File Extensions - Video

    private const string c_FileExt_MKV  = ".mkv";
    private const string c_FileExt_MP4  = ".mp4";



    //## streamline start into relevant functions
    private void Start()
    {
//        localContentFolder = ContentManager.localDirectory.GetRelativeFolder(_episodeSubpath);
        
//        if (localContentFolder == null)
//        {
//            Debug.LogError("Local content directory not found:\n" +ContentManager.localDirectory.GetRelativeFolder(_episodeSubpath));
            
//            return;
//        }
        
//        remoteContentFolder = ContentManager.remoteContentDirectory.GetRelativeFolder(_episodeSubpath);
        
//        if (remoteContentFolder == null)
//        {
//            Debug.LogError("Remote content directory not found:\n" +ContentManager.remoteContentDirectory.GetRelativeFolder(_episodeSubpath));
            
//            return;
//        }
        
        Debug.LogWarning("Selection_Tile Start");
        
//        rectTrans = (RectTransform)transform;

//        TileAnimator = GetComponent<Animator>();

//        ActionPanelAnimator = ActionPanel.GetComponent<Animator>();
        
        _button   = GetComponent<Button>();
        
        Shader.EnableKeyword("UNITY_UI_CLIP_RECT");
//        ParentCarousel = GetComponentInParent<_PHUI_Carousel>();
//        if (NotificationPanel) NotificationPanel.SetActive(false);
        
//        SelectionIsDownloading = false;
        //DownloadingAnimator = ActionPanel.GetComponent<Animator>();

        SetTileState(TileStates.idle);
        //Debug.Log("HEY YOU! The video path is: " + videoPath + " and the other one is: "+SelectionBasePath);
        if (AppManager.Instance.JustLocalContent) return;

        //Debug.Log(" ===== " + Experience.experienceTitle + " ===== START ====== SelectionIsReady  " + SelectionIsReady);
//        ResetFileSizeValues();

//        UpdateRemoteContentFileSize();

//        localFileSize = localContentFolder.GetBytes();
//        remo
        
//        onLocalFileSizeUpdate.Invoke(localContentFolder.GetBytes());
//        onRemoteFileSizeUpdate.Invoke(remoteContentFolder.GetBytes());
        
//        UpdateLocalContentFileSize();

//        UpdateLocalFolderSize();
        UpdateRemoteFolderSize();
        UpdateDownloadSize();
        
//        IsExperienceReadyToPlay();
    }

//    public void UpdateLocalFolderSize() => localFolderSize = localContentFolder.GetBytes();
    public void UpdateRemoteFolderSize() => remoteFolderSize = remoteContentFolder.GetBytes();

    
    public void Populate(Experience exp,
                         _PHUI_Carousel carousel)
    {
        experience     = exp;
        ParentCarousel = carousel;

        Debug.Log($"Selection tile populated with Experience [{exp.title}]");

        SelectionHeading.text     = exp.title;
        SelectionDuration.text    = exp.duration;
        SelectionDescription.text = exp.description;

        carousel.tiles.Add(this);

        SelectionThumbnail.sprite = exp.sprite;
    }


    private void Update()
    {
        if (SelectionIsDownloading) UpdateDownloadUI();

        // DEBUG
        if (Input.GetKeyDown(KeyCode.O) && download != null) download.Cancel();
    }

//    void ResetFileSizeValues()
//    {
//        Debug.LogWarning("ResetFileSizeValues");
        
//        if (!AppManager.Instance.initialised) AppManager.Instance.Initialise();
//
//        ContentManager.Folder remoteFolder = ContentManager.remoteDirectory.GetRelativeFolder($"Content{Path.DirectorySeparatorChar}{Experience.experienceBasePath}");
//        if (remoteFolder != null) remoteFileSize = remoteFolder.GetBytes();

//        Debug.Log($"remote file size is apparently [{remoteFileSize}]");

//        ContentManager.Folder localFolder = ContentManager.localDirectory.GetRelativeFolder($"Content{Path.DirectorySeparatorChar}{Experience.experienceBasePath}");
//        if (localFolder != null) localFileSize = localFolder.GetBytes();

//        Debug.Log($"remote file size is apparently [{localFileSize}]");

//        downloadFileSize = remoteFileSize - localFileSize;
//        
//        if (downloadFileSize > 0)
//        {
//            string sizeAbbr = "";
//            if (downloadFileSize >= 1073741824) sizeAbbr = "GB"; else sizeAbbr = "MB";
//
//            SetTotalFileDownloadSize(downloadFileSize);
//            //SetDownloadTextValue("0.00 " + sizeAbbr);
//            SetDownloadProgressValue(downloadProgress);
//
//            //Debug.Log($"remoteFileSize: {remoteFileSize} | localFileSize: {localFileSize}");
//            decimal percentageDone = (decimal)localFileSize / (decimal)remoteFileSize;
//            downloadProgress = (float)percentageDone;
//            InitialProgressValue = downloadProgress;
//            
//            //Debug.Log($"remoteFileSize: {remoteFileSize} | localFileSize: {localFileSize}| downloadFileSize: {downloadFileSize} percentageDone: {percentageDone}");
//            if ((float)percentageDone >= 0.01f)
//            {
//                NotificationPanel.SetActive(true);
//                SetNotifcationPanelValues(String.Format("{0:0}", percentageDone * 100) + "%", 2);
//            }
//        }
//    }


    public void UpdateRemoteContentFileSize()
    {
        Debug.LogWarning("Selection_Tile UpdateRemoteContentFileSize");

//        _remoteContentPath = $"Content{Path.DirectorySeparatorChar}{Experience.basePath}";
        
//        var remoteFolder       = ContentManager.remoteDirectory.GetRelativeFolder(remoteContentPath);

//        var remoteContentPath = _contentSubpath + _slash + _episodeSubpath;

//        var remoteFolder = remoteDirectory.GetRelativeFolder(_relativeContentPath);

        remoteFolderSize = remoteContentFolder.GetBytes();

        Debug.Log($"Remote file size successfully updated - [{remoteFolderSize}]");

        remoteFileExists = remoteFolderSize != 0;

        if (!remoteFileExists)
        {
            Debug.LogError($"The episode {experience.title} video file could not be found.");
        }
    }


//    public void UpdateLocalContentFileSize()
//    {
//        Debug.LogWarning("Selection_Tile UpdateLocalContentFileSize");
//
////        _localContentPath = $"Content{Path.DirectorySeparatorChar}{Experience.basePath}";
//        
//        if (localFolder == null)
//        {
//            Debug.LogWarning("Target local content directory not found:\n" +_relativeContentPath);
//
//            localFileSize = 0;
//        }
//        else
//        {
//            localFileSize = localFolder.GetBytes();
//        }
//        
//        Debug.Log($"Local file size successfully updated - [{localFileSize}]");
//    }


    public void UpdateDownloadSize()
    {
        Debug.LogWarning("Selection_Tile UpdateDownloadSize");

//        downloadBytesRemaining = remoteFolderSize - localFolderSize;
        downloadBytesRemaining = remoteFolderSize - LocalFolderSize;

//        onDownloadTotalUpdate.Invoke(downloadFileSize);
        
        if (downloadBytesRemaining > 0)
        {
            string sizeAbbr                              = "";
            if (downloadBytesRemaining >= 1073741824) sizeAbbr = "GB"; else sizeAbbr = "MB";

            SetTotalFileDownloadSize((long)downloadBytesRemaining);
            //SetDownloadTextValue("0.00 " + sizeAbbr);
            SetDownloadProgressValue(downloadProgress);

            //Debug.Log($"remoteFileSize: {remoteFileSize} | localFileSize: {localFileSize}");
            decimal percentageDone = (decimal)LocalFolderSize / (decimal)remoteFolderSize;
            downloadProgress     = (float)percentageDone;
            InitialProgressValue = downloadProgress;
            
            //Debug.Log($"remoteFileSize: {remoteFileSize} | localFileSize: {localFileSize}| downloadFileSize: {downloadFileSize} percentageDone: {percentageDone}");
            if ((float)percentageDone >= 0.01f)
            {
                NotificationPanel.SetActive(true);
                SetNotifcationPanelValues(String.Format("{0:0}", percentageDone * 100) + "%", 2);
            }
        }
    }


    public async void IsExperienceReadyToPlay()
    {
//        // The madness ends here - Lucas
//        
//        return;

        Debug.LogWarning("IsExperienceReadyToPlay");

        if (LocalFolderSize >= remoteFolderSize)
        {
            Debug.Log($"Experience [{experience.title}] is downloaded and playable!");
            
//            SetIsReady(true);
            SetContentState(ContentStates.readyToPlay);
            return;
        }

        //Debug.Log($"OVRPlugin.cpuLevel: {OVRPlugin.cpuLevel} / OVRPlugin.gpuLevel {OVRPlugin.gpuLevel}");
        //Debug.Log($"OVRManager.cpuLevel: {OVRManager.cpuLevel} / OVRManager.gpuLevel {OVRManager.gpuLevel}");
        //only check the current tile
        //if (currentTileState == TileStates.active || currentTileState == TileStates.selected)
        //{
        //Debug.Log($"Experience path is {AppManager.Instance.LocalStorage + Experience.experienceBasePath}");
        //Debug.Log("in function IsExperienceReadyToPlay and missing file count is "+ ContentManager.Instance.GetMissingFilesInPath(AppManager.Instance.LocalStorage,Experience.experienceBasePath).Count);
        //set Downloader state to hide all messages
        SetDownloaderState(DownloadStates.hideAll);

        //Debug.Log("videoPath: " + pathToUse + " DOES NOT exist and SelectionIsDownloading is " + SelectionIsDownloading);

        if (SelectionIsDownloading)
        {
            //Debug.Log("ContentStates.currentlyDownloading - SelectionIsDownloading : is true");
            SetContentState(ContentStates.currentlyDownloading);
        }
        else
        {

            if (currentContentState == ContentStates.preparingDownload)
            {
                //Debug.Log("ContentStates.preparingDownload - SelectionIsDownloading : is false - preparing Download");
                SetContentState(ContentStates.preparingDownload);
            }
            else if (currentContentState == ContentStates.processDownload)
            {
                //Debug.Log("ContentStates.processDownload - SelectionIsDownloading : is false - process Download");
                SetContentState(ContentStates.processDownload);
            }
            else
            {
                //Debug.Log("ContentStates.needToDownload - SelectionIsDownloading : is false - need to download");
                SetContentState(ContentStates.needToDownload);

                //if the device has internet connectivity
                //Debug.Log("In Tile IsInternetConnected: " + AppManager.Instance.IsInternetConnected);
//                Task<bool> onlineCheck = AppManager.CheckIfOnline();
//                await onlineCheck;

                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    //if there's nothing currently downloading, check space and show the button!
                    if (!SelectionIsDownloading)
                    {
//                        ResetFileSizeValues();
                        // is there enough storage free locally to download?
                        
                        var currentAvailableStorage = (ulong)ContentManager.GetAvailableDiscSpace();
                        
                        //Debug.Log("currentAvailableStorage: " + currentAvailableStorage);
                        if (currentAvailableStorage > downloadBytesRemaining)// is free storage is larger than the download... all good, let's go
                        {
                            SetDownloaderState(DownloadStates.readyToDownload);
                            //Debug.Log($"Enough space for this download of {downloadFileSize}, {currentAvailableStorage} > {downloadFileSize}");
                        }
                        else
                        {
                            SetDownloaderState(DownloadStates.storageFull);
                            DownloadStorageMessage.SetText("");
                            //Debug.Log($"NOT enough space for this download of {downloadFileSize}!!! {currentAvailableStorage} < {downloadFileSize}");

                            string storageNeeded    = remoteFolderSize.AsDataSize();
                            string storageAvailable = currentAvailableStorage.AsDataSize();
                            
                            string storageMessage   = "There is not enough available storage on this<BR>device to download this experience.<size=28><BR><BR>Required: " + storageNeeded + "<BR><BR>Available: " + storageAvailable;
                            storageMessage += "<BR><BR></size><size=32>Please free up the required ammount of storage<BR>on your device and try again.</size>";

                            DownloadStorageMessage.SetText(storageMessage);
                            //Debug.Log("DownloadStorageMessage: " + DownloadStorageMessage.text);
                        }
                    }
                    else //another tile is downloading so show a busy message
                    {
                        SetDownloaderState(DownloadStates.downloaderBusy);
                    }
                }
                else // no interwebs connection
                {
                    SetDownloaderState(DownloadStates.noConnection);
                }
            }
        }
        //}

    }

    public void SetDownloaderState(DownloadStates downloadState)
    {
        currentDownloaderState = downloadState;

        FileSizeText.gameObject.SetActive(false);
        DownloadInstruction.gameObject.SetActive(false);
        DownloadButton.gameObject.SetActive(false);
        DownloadDisclaimer.gameObject.SetActive(false);
        DownloadStorageMessage.gameObject.SetActive(false);
        DownloadBusyMessage.gameObject.SetActive(false);
        DownloadNoConnectionMessage.gameObject.SetActive(false);

        switch (downloadState)
        {
            case DownloadStates.hideAll:

                break;
            case DownloadStates.downloaderBusy:
                DownloadBusyMessage.gameObject.SetActive(true);
                break;

            case DownloadStates.storageFull:
                DownloadStorageMessage.gameObject.SetActive(true);
                break;

            case DownloadStates.readyToDownload:
                FileSizeText.gameObject.SetActive(true);
                DownloadInstruction.gameObject.SetActive(true);
                DownloadButton.gameObject.SetActive(true);
                DownloadDisclaimer.gameObject.SetActive(true);
                break;
            case DownloadStates.noConnection:
                DownloadNoConnectionMessage.gameObject.SetActive(true);
                break;
        }

    }


    // possibly this will talk to the media manager, and maybe that will be state based as well... ie preparing/ready/etc

    //sets video up so can be played later
//    public void SetUpVideo() => StartCoroutine(AppManager.Instance.SetUpVideoForExperience(videoPath, Experience.basePath, Experience.title));

    public void SetUpVideo() => AppManager.Instance.SetUpVideoForExperience(tile: this,
                                                                            videoFilePath: _localVideoFilePath);

//    public string PathToLocalVideoFile => 
    
    public void PlayVideo()
    {

        //Goes straight to video

        AppManager.Instance.UIManager.colorSkybox.LerpToBlack(2);
        AppManager.Instance.UIManager.colorSkybox.LerpFinished.AddListener(AppManager.Instance.ChangeStateExperience);
        AppManager.Instance.UIManager.MenuUI_hidden();
        //AudioManagerSimple.i.PlayOneShotInt(8);

    }

    public void TriggerAudioSelection()
    {
        //Debug.Log("PLAY BUTTON - selection tile : TriggerAudioSelection is being called in Selection tile for audio in the folder: " + SelectionBasePath + "audio/");
        //Turn on audio selection set up
        AppManager.Instance.UIManager.VoiceObjectUI_active();
        AppManager.Instance.UIManager.VoiceObjectUI.gameObject.GetComponentInChildren<VoiceOptionsHandler>().ExperienceAudioBasePath = experience.title + "audio/";

    }

    public void ExperienceReadyAndDownloaded()
    {
        //Debug.Log("Download is done");
        ActionPanelAnimator.SetTrigger("readyToPlay");
//        SetIsReady(true);
        NotificationPanel.SetActive(true);
        SetContentState(ContentStates.readyToPlay);
        //Debug.Log("Play button should be shown");

    }

    public void DownloadExperience()
    {
        SelectionIsDownloading = true;

        

//        var localPath = Path.Combine(AppManager.Instance.LocalStorage,
//                                     AppManager.contentPath,
//                                     experience.basePath);

//        var remotePath = Path.Combine(AppManager.contentPath,
//                                      experience.basePath);

        Debug.Log($"Downloadin'!\nLocalPath [{_localEpisodePath}]\nRemotePath [{_relativeContentPath}]");


        download = new AWS.S3.Download(_localEpisodePath,
                                       _relativeContentPath);
        
//        download.onDownloadProgress += DownloadProgressCallback;

        onDownloadBegun.Invoke();

//        download.onDownloadProgress += DownloadUpdate;
        
        AudioManagerSimple.i.PlayOneShotInt(5);
        
        if (AppManager.Instance.UseAnalytics) Analytics.StartDownload(experience.title, GetDownloadPercentString());
        
        IsExperienceReadyToPlay();
    }

    private void DownloadProgressCallback(object sender, AWS.S3.DownloadProgressArgs downloadProgressArgs)
    {
//        remoteFileSize = downloadProgressArgs.totalBytes;
//        _localFolderSize = _downloadBytesRemaining = (ulong)downloadProgressArgs.transferredBytes;
        
//        _downloadBytesRemaining        = (ulong)downloadProgressArgs.transferredBytes;
        
        downloadProgress        = downloadProgressArgs.progress;
        clampedDownloadProgress = Mathf.Clamp(downloadProgress, 0, 0.99f);

        if (downloadProgressArgs.isDone)
        {
            if (AppManager.Instance.UseAnalytics) Analytics.EndDownload(false, GetDownloadPercentString());
//            SetIsReady(true);
            SetContentState(ContentStates.readyToPlay);
            SelectionIsDownloading = false;
        }
    }


//    private void DownloadUpdate(object sender,
//                                AWS.S3.DownloadProgressArgs args)
//    {
//        onDownloadBytesUpdate.Invoke(args.transferredBytes);
//        onDownloadPercentUpdate.Invoke(args.progress);
//        download.onDownloadProgress += 
//    }

    private void OnApplicationQuit()
    {
        if (download != null) download.Cancel();
    }

//    private void UpdateDownloadUI()
//    {
//        SetNotifcationPanelValues(String.Format("{0:0}", clampedDownloadProgress * 100) + "%", 2);
//        DownloadProgressText.text = $"{FileSizeToString(_localFolderSize)} of {FileSizeToString(remoteFolderSize)}";
//        DownloadProgressSpinner.SetProgressValue(downloadProgress);
//    }

    private void UpdateDownloadUI()
    {
        SetNotifcationPanelValues(String.Format("{0:0}", clampedDownloadProgress * 100) + "%", 2);
        
        DownloadProgressText.text = $"{LocalFolderSize.AsDataSize()} of {_remoteFolderSize.AsDataSize()}";
        
//        DownloadProgressSpinner.SetProgressValue(downloadProgress);
    }
    
    private void SetNotifcationPanelValues(string textToUse, int iconToUseFromArray)
    {
        NotificationText.fontSize = 38f;
        
        switch (iconToUseFromArray)
        {
            case 0:
                NotificationIcon.gameObject.SetActive(false);
                break;
            case 1:
                NotificationIcon.gameObject.SetActive(true);
                break;
            case 2:
                NotificationText.fontSize = 60f;
                NotificationIcon.gameObject.SetActive(true);
                break;

            default:
                break;
        }
        NotificationIcon.sprite = NotificationIconList[iconToUseFromArray];
        NotificationText.text = textToUse;
    }

//    public void SetHeadingText(string experienceTitle)
//    {
//        SelectionHeading.text = experienceTitle;
//
//    }
//
//    public void SetDurationText(string experienceDuration)
//    {
//        SelectionDuration.text = experienceDuration;
//
//    }
//
//    public void SetDescriptionText(string experienceDescription)
//    {
//        SelectionDescription.text = experienceDescription;
//    }

//    public void SetThumbnail()
//    {
//        var path = AppManager.Instance.LocalStorage + Path.DirectorySeparatorChar + "Required" + Path.DirectorySeparatorChar + experience.basePath + _thumbnailSubpath;
//        
//        if (File.Exists(path))
//        {
//            //Debug.Log("Selection_Tile : SetThumbnail : OK >> Setting thumbnail: " + SelectionThumbnail);
//            var thumbnailBytes = File.ReadAllBytes(path);
//            var texture        = new Texture2D(1, 1);
//            
//            texture.LoadImage(thumbnailBytes);
//            
//            var thumbnailSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100F, 0, SpriteMeshType.FullRect);
//            
//            SelectionThumbnail.GetComponent<Image>().sprite = thumbnailSprite;
//        }
//        else
//        {
//            Debug.Log("Selection_Tile : SetThumbnail : ERROR >> couldn't find file: " + SelectionThumbnail);
//        }
//    }


    public void SetTileState(TileStates tileState)
    {
        previousTileState = currentTileState;
        currentTileState = tileState;
        //Debug.Log("SelectionTile : SetTileState : " + currentTileState);
        switch (tileState)
        {
            case TileStates.idle:
                TileAnimator.SetTrigger("idle");
                break;
            case TileStates.active:
                TileAnimator.SetTrigger("active");
                currentTileState = TileStates.selected;
                //AppManager.Instance.UIManager.skybox.LerpToRandomColour();

                AudioManagerSimple.i.PlayOneShotInt(lastTone); // make this random
                lastTone++;
                if (lastTone > 4) lastTone = 2;

                break;
            case TileStates.inactive:
                TileAnimator.SetTrigger("inactive");
                currentTileState = TileStates.idle;
                break;
            case TileStates.selected:
                TileAnimator.SetTrigger("selected");
                _button.interactable = false;
                break;
        }

    }

    public void SetContentState(ContentStates contentState)
    {
        currentContentState = contentState;
        
        Debug.Log("SelectionTile : SetContentState : " + currentContentState);

        switch (contentState)
        {
            case ContentStates.readyToPlay:
                ActionPanelAnimator.SetTrigger(ReadyToPlay);
                NotificationPanel.SetActive(true);
                SetNotifcationPanelValues("Ready", 1);
                RemovalPanel.SetActive(true);
                break;

            case ContentStates.needToDownload:
                ActionPanelAnimator.SetTrigger(NeedToDownload);
                NotificationPanel.SetActive(false);
                SetNotifcationPanelValues("", 0);
                RemovalPanel.SetActive(false);
                break;

            case ContentStates.preparingDownload:
                ActionPanelAnimator.SetTrigger(PreparingDownload);
                NotificationPanel.SetActive(true);
                SetNotifcationPanelValues("Preparing...", 0);
                break;

            case ContentStates.currentlyDownloading:
                ActionPanelAnimator.SetTrigger(CurrentlyDownloading);
                NotificationPanel.SetActive(true);
                SetNotifcationPanelValues("", 2);
                break;

            case ContentStates.processDownload:
                ActionPanelAnimator.SetTrigger(ProcessDownload);
                NotificationPanel.SetActive(true);
                SetNotifcationPanelValues("Processing...", 0);
                break;

            case ContentStates.settings_confirmRemoval:
                ActionPanelAnimator.SetTrigger(SettingsConfirmRemoval);
                RemovalPanel.SetActive(false);
                break;

            case ContentStates.settings_removingExperience:
                ActionPanelAnimator.SetTrigger(SettingsRemovingExperience);
                RemovalPanel.SetActive(false);
                break;

            case ContentStates.settings_experienceRemoved:
                ActionPanelAnimator.SetTrigger(SettingsExperienceRemoved);
                RemovalPanel.SetActive(false);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(contentState),
                                                      contentState,
                                                      null);
        }

    }

//    public void SetIsReady(bool experienceIsReady)
//    {
//        //Debug.Log("SetIsReady : " + experienceIsReady);
////        SelectionIsReady = experienceIsReady;
//    }


    public void SetTotalFileDownloadSize(long byteValue)
    {
//        string DLValue    = FileSizeToString(byteValue);
//        string TotalValue = FileSizeToString(remoteFolderSize);
        
//        var DLValue    = byteValue.AsDataSize();
//        var TotalValue = remoteFolderSize.AsDataSize();
//
//        //DownloadInstruction.SetText("Download this experience");
//        FileSizeText.text = "Total download: " + DLValue;
//
//        if (DLValue != TotalValue)
//        {
//            DownloadInstruction.SetText("Resume this download");
//
//            FileSizeText.text = String.Format("{0:0}",
//                                              downloadProgress * 100) +
//                                "% complete. Already downloaded ";
//
//            FileSizeText.text += FileSizeToString(remoteFolderSize - byteValue) + " of " + TotalValue;
//        }
    }


//    public void SetDownloadTextValue(string progressText)
//    {
//        //if (DownloadProgressValue < InitialProgressValue) DownloadProgressValue = InitialProgressValue;
//        DownloadProgressText.text = progressText + " of " + FileSizeToString(remoteFileSize);
//    }

    public void SetDownloadProgressValue(float progressValue)
    {
        downloadProgress = progressValue;
        if (downloadProgress < InitialProgressValue) downloadProgress = InitialProgressValue;

        downloadProgress = Mathf.Clamp(downloadProgress, 0.01f, 0.99f);
//        DownloadProgressSpinner.SetProgressValue(downloadProgress);
        SetNotifcationPanelValues(String.Format("{0:0}", downloadProgress * 100) + "%", 2);

        DownloadProgressText.text = FileSizeToString((long)(Convert.ToInt64(remoteFolderSize) * downloadProgress)) + " of " + FileSizeToString((long)remoteFolderSize);
    }


    public void SelectTile()
    {

        if (currentTileState == TileStates.idle) // if tile is in an idle state 
        {
            ParentCarousel.AddNewHighlightTile(this);
            //Debug.Log("From the tile : i'm number #" + ParentCarousel.GetCarouselItemNumber(this.gameObject));
            ParentCarousel.SetCarouselItemIndex(ParentCarousel.GetCarouselItemNumber(gameObject));
        }
    }

    public void SetIdleTileHighlight(bool onEnter)
    {

        if (currentTileState == TileStates.idle) // if tile is in an idle state 
        {
            if (onEnter)
            {
                //Debug.Log("SetIdleTileHighlight onEnter = enter ");
                TileAnimator.SetTrigger(id: HighlightEnter);
            }
            else
            {
                //Debug.Log("SetIdleTileHighlight onEnter = exit ");
                TileAnimator.SetTrigger(id: HighlightExit);
            }
        }
    }



    public void SetTileActive()
    {

        if (currentTileState == TileStates.idle)
        {
            SetTileState(TileStates.active);
            //TileAnimator.SetBool("Open", true);

        }
        else
        {
            SetTileState(TileStates.selected);
        }
        
        _button.interactable = false;

    }

    public void SetTileInactive()
    {
        if (currentTileState == TileStates.selected)
        {
            SetTileState(TileStates.inactive);
            //TileAnimator.SetBool("Open", false);

        }
        else
        {
            SetTileState(TileStates.idle);
        }
        
        _button.interactable = true;

    }

//    public void ToggleTileState()
//    {
//        if (currentTileState == TileStates.active)
//        {
//            SetTileState(TileStates.inactive);
//        }
//        else
//        {
//            SetTileState(TileStates.active);
//        }
//    }

    public void ConfirmDeleteExperience()
    {
        //Debug.Log("SelectionTile : ConfirmDeleteExperience : starting the deletion confirmation process");
//        SetContentState(ContentStates.settings_confirmRemoval);
        
        ActionPanelAnimator.SetTrigger(SettingsConfirmRemoval);
        RemovalPanel.SetActive(false);
    }

    public void StartDeleteGracePeriod()
    {
        //Debug.Log("SelectionTile : StartDeleteGracePeriod : kicking it off,  waiting 10 seconds");
        SetContentState(ContentStates.settings_removingExperience);
        Invoke("WaitForDeleteGracePeriod", 3f);
    }

    void WaitForDeleteGracePeriod()
    {
        //Debug.Log("SelectionTile : WaitForDeleteGracePeriod : Too late, deleting");
        SetContentState(ContentStates.settings_experienceRemoved);
        DeleteExperience();
    }

    public void CancelDeleteExperience()
    {
        //Debug.Log("SelectionTile : CancelDeleteExperience : cancelled, just made it!");
        CancelInvoke("WaitForDeleteGracePeriod");
        StartCoroutine(WaitAndResetTile(0.25f));

        if (AppManagerPlace.Instance.UseAnalytics) Analytics.EndDownload(true, GetDownloadPercentString());
    }

    void DeleteExperience()
    {
        var targetPath = Path.Combine(AppManager.Instance.LocalStorage,
                                      AppManager.contentPath,
                                      experience.basePath);

        Debug.Log($"I'm going to destory the world and this directory {targetPath}");

        Directory.Delete(targetPath,
                         true);

//        localFileSize = 0;
        
//        ContentManager.BuildFolderLists(AppManager.Instance.LocalStorage);
        
//        StartCoroutine(WaitAndResetTile(5f));
        
        UpdateDownloadSize();

    }

    IEnumerator WaitAndResetTile(float waitTime)
    {
        //Debug.Log("SelectionTile : WaitAndResetTile : waiting before the state change");
        yield return new WaitForSeconds(waitTime);
        
        UpdateDownloadSize();
        
//        ResetFileSizeValues();
        IsExperienceReadyToPlay();
    }

    string GetDownloadPercentString()
    {
        return (((float)LocalFolderSize / (float)remoteFolderSize) * 100).ToString("F0") + "%";
    }

    string FileSizeToString(long fileSize)
    {
        if (fileSize >= 1073741824) return $"{(((fileSize / 1024f) / 1024f) / 1024f).ToString("F2")}GB";
        else return $"{((fileSize / 1024f) / 1024f).ToString("F0")}MB";
    }




}
