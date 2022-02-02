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

using UnityEngine.Serialization;

using Vanilla.StringFormatting;

using static UnityEngine.Debug;

[Serializable]
public class ULongEvent : UnityEvent<ulong> { }

[Serializable]
public class FloatEvent : UnityEvent<float> { }

[Serializable]
public class BoolEvent : UnityEvent<bool> { }

public class Selection_Tile : MonoBehaviour
{

//    private Experience _Experience;

    [Header("Components")]
    public Button button;

    [HideInInspector]
    public RectTransform rectTrans;

    [Header("Experience Payload")]
    [SerializeField]
    public Experience experience;

    [Header("Experience UI")]
    public TextMeshProUGUI experienceHeadingText;
    public TextMeshProUGUI experienceDurationText;
    public TextMeshProUGUI experienceDescriptionText;
    public Image experienceThumbnailImage;

    [Header("Download Status UI")]
    public GameObject downloadStatusPanel;
    public TextMeshProUGUI downloadStatusText;
    public Image downloadStatusIcon;

    public Sprite downloadRequiredSprite;
    public Sprite downloadCompleteSprite;
    
//    public GameObject RemovalPanel;
    public Button     deleteContentButton;

    [Header("Action Panel")]
//    public GameObject ActionPanel;
    public Animator   actionPanelAnimator;

    public                   TextMeshProUGUI FileSizeText;
    public                   TextMeshProUGUI DownloadProgressBytesText;
    public                   Text DownloadProgressPercentText;
    public                   float           downloadProgress, clampedDownloadProgress;
    [SerializeField] private float           InitialProgressValue;
//    public                   PHUI_Progress_Spinner DownloadProgressSpinner;

    [FormerlySerializedAs("DownloadInstruction")]
    [Header("Download UI")]
    public Button DownloadButton;
    public TextMeshProUGUI downloadInstructionText;
    [FormerlySerializedAs("DownloadDisclaimer")]
    public TextMeshProUGUI downloadDisclaimerText;
    [FormerlySerializedAs("DownloadStorageMessage")]
    public TextMeshProUGUI downloadStorageMessageText;
    [FormerlySerializedAs("DownloadBusyMessage")]
    public TextMeshProUGUI downloadBusyMessageText;
    [FormerlySerializedAs("DownloadNoConnectionMessage")]
    public TextMeshProUGUI downloadNoConnectionText;

//    private string SelectionBasePath;
//    public bool SelectionIsReady;
    public bool SelectionIsDownloading;
    public Animator TileAnimator;
    public string TotalDownloadFileSize;
    
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
//        processDownload,
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
    
    private static readonly int             c_HighlightEnter             = Animator.StringToHash("highlightEnter");
    private static readonly int             c_HighlightExit              = Animator.StringToHash("highlightExit");
    
    private static readonly int             c_ReadyToPlay                = Animator.StringToHash("readyToPlay");
    private static readonly int             c_NeedToDownload             = Animator.StringToHash("needToDownload");
    private static readonly int             c_PreparingDownload          = Animator.StringToHash("preparingDownload");
    private static readonly int             c_CurrentlyDownloading       = Animator.StringToHash("currentlyDownloading");
    private static readonly int             c_ProcessDownload            = Animator.StringToHash("processDownload");
    private static readonly int             c_SettingsConfirmRemoval     = Animator.StringToHash("settings_confirmRemoval");
    private static readonly int             c_SettingsRemovingExperience = Animator.StringToHash("settings_removingExperience");
    private static readonly int             c_SettingsExperienceRemoved  = Animator.StringToHash("settings_experienceRemoved");

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
//    private void Start()
//    {
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
        
//        Debug.LogWarning("Selection_Tile Start");
        
//        rectTrans = (RectTransform)transform;

//        TileAnimator = GetComponent<Animator>();

//        ActionPanelAnimator = ActionPanel.GetComponent<Animator>();
        
//        _button   = GetComponent<Button>();
        
//        Shader.EnableKeyword("UNITY_UI_CLIP_RECT");
//        ParentCarousel = GetComponentInParent<_PHUI_Carousel>();
//        if (NotificationPanel) NotificationPanel.SetActive(false);
        
//        SelectionIsDownloading = false;
        //DownloadingAnimator = ActionPanel.GetComponent<Animator>();

//        SetTileState(TileStates.idle);
        //Debug.Log("HEY YOU! The video path is: " + videoPath + " and the other one is: "+SelectionBasePath);

//        if (!Place.IsOnline) return;

        //Debug.Log(" ===== " + Experience.experienceTitle + " ===== START ====== SelectionIsReady  " + SelectionIsReady);
//        ResetFileSizeValues();

//        UpdateRemoteContentFileSize();

//        localFileSize = localContentFolder.GetBytes();
//        remo
        
//        onLocalFileSizeUpdate.Invoke(localContentFolder.GetBytes());
//        onRemoteFileSizeUpdate.Invoke(remoteContentFolder.GetBytes());
        
//        UpdateLocalContentFileSize();

//        UpdateLocalFolderSize();
//        UpdateRemoteFolderSize();
//        UpdateDownloadSize();
        
//        IsExperienceReadyToPlay();
//    }

//    public void UpdateLocalFolderSize() => localFolderSize = localContentFolder.GetBytes();
//    public void UpdateRemoteFolderSize() => remoteFolderSize = remoteContentFolder.GetBytes();

    



    private void Update()
    {
//        if (SelectionIsDownloading) UpdateDownloadUI();

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
        LogWarning("Selection_Tile UpdateRemoteContentFileSize");

//        _remoteContentPath = $"Content{Path.DirectorySeparatorChar}{Experience.basePath}";
        
//        var remoteFolder       = ContentManager.remoteDirectory.GetRelativeFolder(remoteContentPath);

//        var remoteContentPath = _contentSubpath + _slash + _episodeSubpath;

//        var remoteFolder = remoteDirectory.GetRelativeFolder(_relativeContentPath);

        remoteFolderSize = remoteContentFolder.GetBytes();

        Log($"Remote file size successfully updated - [{remoteFolderSize}]");

        remoteFileExists = remoteFolderSize != 0;

        if (!remoteFileExists)
        {
            LogError($"The episode {experience.title} video file could not be found.");
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


//    public void UpdateDownloadSize()
//    {
//        LogWarning("Selection_Tile UpdateDownloadSize");
//
////        downloadBytesRemaining = remoteFolderSize - localFolderSize;
//        downloadBytesRemaining = remoteFolderSize - LocalFolderSize;
//
////        onDownloadTotalUpdate.Invoke(downloadFileSize);
//        
//        if (downloadBytesRemaining > 0)
//        {
//            string sizeAbbr                              = "";
//            if (downloadBytesRemaining >= 1073741824) sizeAbbr = "GB"; else sizeAbbr = "MB";
//
//            SetTotalFileDownloadSize((long)downloadBytesRemaining);
//            //SetDownloadTextValue("0.00 " + sizeAbbr);
//            SetDownloadProgressValue(downloadProgress);
//
//            //Debug.Log($"remoteFileSize: {remoteFileSize} | localFileSize: {localFileSize}");
//            decimal percentageDone = (decimal)LocalFolderSize / (decimal)remoteFolderSize;
//            downloadProgress     = (float)percentageDone;
//            InitialProgressValue = downloadProgress;
//            
//            //Debug.Log($"remoteFileSize: {remoteFileSize} | localFileSize: {localFileSize}| downloadFileSize: {downloadFileSize} percentageDone: {percentageDone}");
//            if ((float)percentageDone >= 0.01f)
//            {
//                downloadStatusPanel.SetActive(true);
////                SetNotificationPanelValues(String.Format("{0:0}", percentageDone * 100) + "%", 2);
//            }
//        }
//    }


    public async void IsExperienceReadyToPlay()
    {
//        // The madness ends here - Lucas
//        
        return;

        LogWarning("IsExperienceReadyToPlay");


        if (LocalFolderSize >= remoteFolderSize)
        {
            Log($"Experience [{experience.title}] is downloaded and playable!");
            
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
//            else if (currentContentState == ContentStates.processDownload)
//            {
//                //Debug.Log("ContentStates.processDownload - SelectionIsDownloading : is false - process Download");
//                SetContentState(ContentStates.processDownload);
//            }
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
                            downloadStorageMessageText.SetText("");
                            //Debug.Log($"NOT enough space for this download of {downloadFileSize}!!! {currentAvailableStorage} < {downloadFileSize}");

                            string storageNeeded    = remoteFolderSize.AsDataSize();
                            string storageAvailable = currentAvailableStorage.AsDataSize();
                            
                            string storageMessage   = "There is not enough available storage on this<BR>device to download this experience.<size=28><BR><BR>Required: " + storageNeeded + "<BR><BR>Available: " + storageAvailable;
                            storageMessage += "<BR><BR></size><size=32>Please free up the required ammount of storage<BR>on your device and try again.</size>";

                            downloadStorageMessageText.SetText(storageMessage);
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
        downloadInstructionText.gameObject.SetActive(false);
        DownloadButton.gameObject.SetActive(false);
        downloadDisclaimerText.gameObject.SetActive(false);
        downloadStorageMessageText.gameObject.SetActive(false);
        downloadBusyMessageText.gameObject.SetActive(false);
        downloadNoConnectionText.gameObject.SetActive(false);

        switch (downloadState)
        {
            case DownloadStates.hideAll:

                break;
            case DownloadStates.downloaderBusy:
                downloadBusyMessageText.gameObject.SetActive(true);
                break;

            case DownloadStates.storageFull:
                downloadStorageMessageText.gameObject.SetActive(true);
                break;

            case DownloadStates.readyToDownload:
                FileSizeText.gameObject.SetActive(true);
                downloadInstructionText.gameObject.SetActive(true);
                DownloadButton.gameObject.SetActive(true);
                downloadDisclaimerText.gameObject.SetActive(true);
                break;
            case DownloadStates.noConnection:
                downloadNoConnectionText.gameObject.SetActive(true);
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
        actionPanelAnimator.SetTrigger("readyToPlay");
//        SetIsReady(true);
        downloadStatusPanel.SetActive(true);
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

        Log($"Downloadin'!\nLocalPath [{_localEpisodePath}]\nRemotePath [{_relativeContentPath}]");


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

//    private void UpdateDownloadUI()
//    {
//        SetNotificationPanelValues(String.Format("{0:0}", clampedDownloadProgress * 100) + "%", 2);
        
//        DownloadProgressText.text = $"{LocalFolderSize.AsDataSize()} of {_remoteFolderSize.AsDataSize()}";
        
//        DownloadProgressSpinner.SetProgressValue(downloadProgress);
//    }
    
//    private void SetNotificationPanelValues(string textToUse, int iconToUseFromArray)
//    {
//        downloadStatusText.fontSize = 38f;
//        
//        switch (iconToUseFromArray)
//        {
//            case 0:
//                downloadStatusIcon.gameObject.SetActive(false);
//                break;
//            case 1:
//                downloadStatusIcon.gameObject.SetActive(true);
//                break;
//            case 2:
//                downloadStatusText.fontSize = 60f;
//                downloadStatusIcon.gameObject.SetActive(true);
//                break;
//
//            default:
//                break;
//        }
//        downloadStatusIcon.sprite = NotificationIconList[iconToUseFromArray];
//        downloadStatusText.text = textToUse;
//    }

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
                button.interactable = false;
                break;
        }

    }

    public void SetContentState(ContentStates contentState)
    {
        currentContentState = contentState;
        
        Log("SelectionTile : SetContentState : " + currentContentState);

        switch (contentState)
        {
            case ContentStates.readyToPlay:
                actionPanelAnimator.SetTrigger(c_ReadyToPlay);
                downloadStatusPanel.SetActive(true);
//                SetNotificationPanelValues("Ready", 1);
//                RemovalPanel.SetActive(true);
                break;

            case ContentStates.needToDownload:
                actionPanelAnimator.SetTrigger(c_NeedToDownload);
                downloadStatusPanel.SetActive(false);
//                SetNotificationPanelValues("", 0);
//                RemovalPanel.SetActive(false);
                break;

//            case ContentStates.preparingDownload:
//                ActionPanelAnimator.SetTrigger(PreparingDownload);
//                downloadStatusPanel.SetActive(true);
//                SetNotificationPanelValues("Preparing...", 0);
//                break;

            case ContentStates.currentlyDownloading:
                actionPanelAnimator.SetTrigger(c_CurrentlyDownloading);
                downloadStatusPanel.SetActive(true);
//                SetNotificationPanelValues("", 2);
                break;

//            case ContentStates.processDownload:
//                ActionPanelAnimator.SetTrigger(ProcessDownload);
//                downloadStatusPanel.SetActive(true);
////                SetNotificationPanelValues("Processing...", 0);
//                break;

            case ContentStates.settings_confirmRemoval:
                actionPanelAnimator.SetTrigger(c_SettingsConfirmRemoval);
//                RemovalPanel.SetActive(false);
                break;

            case ContentStates.settings_removingExperience:
                actionPanelAnimator.SetTrigger(c_SettingsRemovingExperience);
//                RemovalPanel.SetActive(false);
                break;

            case ContentStates.settings_experienceRemoved:
                actionPanelAnimator.SetTrigger(c_SettingsExperienceRemoved);
//                RemovalPanel.SetActive(false);
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

//    public void SetDownloadProgressValue(float progressValue)
//    {
//        downloadProgress = progressValue;
//        if (downloadProgress < InitialProgressValue) downloadProgress = InitialProgressValue;
//
//        downloadProgress = Mathf.Clamp(downloadProgress, 0.01f, 0.99f);
////        DownloadProgressSpinner.SetProgressValue(downloadProgress);
////        SetNotificationPanelValues(String.Format("{0:0}", downloadProgress * 100) + "%", 2);
//
//        DownloadProgressText.text = FileSizeToString((long)(Convert.ToInt64(remoteFolderSize) * downloadProgress)) + " of " + FileSizeToString((long)remoteFolderSize);
//    }


    public void SelectTile()
    {

        if (currentTileState == TileStates.idle) // if tile is in an idle state 
        {
            _PHUI_Carousel.i.AddNewHighlightTile(this);
            //Debug.Log("From the tile : i'm number #" + ParentCarousel.GetCarouselItemNumber(this.gameObject));
            _PHUI_Carousel.i.SetCarouselItemIndex(_PHUI_Carousel.i.GetCarouselItemNumber(gameObject));
        }
    }

    public void SetIdleTileHighlight(bool onEnter)
    {

        if (currentTileState == TileStates.idle) // if tile is in an idle state 
        {
            if (onEnter)
            {
                //Debug.Log("SetIdleTileHighlight onEnter = enter ");
                TileAnimator.SetTrigger(id: c_HighlightEnter);
            }
            else
            {
                //Debug.Log("SetIdleTileHighlight onEnter = exit ");
                TileAnimator.SetTrigger(id: c_HighlightExit);
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
        
        button.interactable = false;

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
        
        button.interactable = true;

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
        
        actionPanelAnimator.SetTrigger(c_SettingsConfirmRemoval);
//        RemovalPanel.SetActive(false);
    }

    public void StartDeleteGracePeriod()
    {
        //Debug.Log("SelectionTile : StartDeleteGracePeriod : kicking it off,  waiting 10 seconds");
//        SetContentState(ContentStates.settings_removingExperience);
        
        actionPanelAnimator.SetTrigger(c_SettingsRemovingExperience);
//        RemovalPanel.SetActive(false);

        Invoke(nameof(WaitForDeleteGracePeriod), 3f);
    }

    void WaitForDeleteGracePeriod()
    {
        //Debug.Log("SelectionTile : WaitForDeleteGracePeriod : Too late, deleting");
//        SetContentState(ContentStates.settings_experienceRemoved);

        actionPanelAnimator.SetTrigger(c_SettingsExperienceRemoved);
//        RemovalPanel.SetActive(false);
        
        DeleteContent();
    }

    public void CancelDeleteExperience()
    {
        //Debug.Log("SelectionTile : CancelDeleteExperience : cancelled, just made it!");
        CancelInvoke(nameof(WaitForDeleteGracePeriod));
        StartCoroutine(WaitAndResetTile(0.25f));

        if (AppManagerPlace.Instance.UseAnalytics) Analytics.EndDownload(true, GetDownloadPercentString());
    }




    IEnumerator WaitAndResetTile(float waitTime)
    {
        //Debug.Log("SelectionTile : WaitAndResetTile : waiting before the state change");
        yield return new WaitForSeconds(waitTime);
        
//        UpdateDownloadSize();
        
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


    // -------------------------------------------------------------------------------------------------------------------------- Sanity Restored //

    public async Task Populate(Experience exp)
    {
        LogWarning($"Tile [{experience.title}]\tPopulate");

        // Not sure, possibly vital functionality from old version of the app
        
        Shader.EnableKeyword("UNITY_UI_CLIP_RECT");
        
        SetTileState(TileStates.idle);
        
        // Populate all the UI references for the experience payload class
        
        experience                      = exp;
        experienceHeadingText.text      = exp.title;
        experienceDurationText.text     = exp.duration;
        experienceDescriptionText.text  = exp.description;
        experienceThumbnailImage.sprite = exp.sprite;

        // Connect all our important UI update events
        
//        exp.onContentAvailabilityChange += ContentAvailabilityChangeHandler;
        exp.onDownloadRequirementUpdate += DownloadRequirementUpdateHandler;
        exp.onDownloadBegun             += DownloadBegunHandler;
        exp.onDownloadPacket            += DownloadPacketHandler;
        exp.onDownloadComplete          += DownloadCompleteHandler;

        // This one already fired so we'll just populate him manually here.

        DownloadRequirementUpdateHandler(exp.remoteByteSizeString);
        
        // Ask the experience to fetch its remote size from S3

//        await exp.UpdateRemoteByteSize();
        
        // Ask the experience to update and emit if its locally available on device storage
        
        exp.UpdateContentAvailability();
    }


    [ContextMenu("Make Available")]
    public void MakeAvailable() => ContentAvailabilityChangeHandler(true);


    [ContextMenu("Make Unavailable")]
    public void MakeUnAvailable() => ContentAvailabilityChangeHandler(false);
    
    public void ContentAvailabilityChangeHandler(bool available)
    {
        LogWarning($"Tile [{experience.title}]\tContentAvailabilityChangeHandler [{available}]");
        
        if (available)
        {
            LogWarning($"AnimDebug for [{experience.title}] SetTrigger[{c_ReadyToPlay}]");

            actionPanelAnimator.SetTrigger(id: c_ReadyToPlay);
            downloadStatusIcon.sprite   = downloadCompleteSprite;
            downloadStatusText.fontSize = 38f;
            downloadStatusText.text     = "Ready";
            deleteContentButton.gameObject.SetActive(true);

        }
        else
        {
            LogWarning($"AnimDebug for [{experience.title}] SetTrigger[{c_NeedToDownload}]");
            
            actionPanelAnimator.SetTrigger(id: c_NeedToDownload);
            downloadStatusIcon.sprite   = downloadRequiredSprite;
            downloadStatusText.fontSize = 60f;
            downloadStatusText.text     = string.Empty;
            deleteContentButton.gameObject.SetActive(false);
            
//            FileSizeText.gameObject.SetActive(true);
//            downloadInstructionText.gameObject.SetActive(true);
//            DownloadButton.gameObject.SetActive(true);
//            downloadDisclaimerText.gameObject.SetActive(true);
        }
        
    }


    private void DownloadRequirementUpdateHandler(string sizeString)
    {
        Log($"Tile [{experience.title}]\tDownload Requirement Update [{sizeString}]");

        FileSizeText.text = "TOTAL SIZE: " + sizeString;
    }


    private void DownloadBegunHandler()
    {
        Log($"Tile [{experience.title}]\tDownload Begun");

        downloadStatusText.text = "0%";
        
        actionPanelAnimator.SetTrigger(c_CurrentlyDownloading);
    }

    private void DownloadPacketHandler(ulong transferred, float progress)
    {
        var transferredString = transferred.AsDataSize();

        var progressPercent = Mathf.FloorToInt(progress * 100);
        
        Log($"Tile [{experience.title}]\tDownload Progress [{transferredString}] [{progressPercent}%]");
        
        downloadStatusText.text = $"{progressPercent}%";

        DownloadProgressBytesText.text   = $"{transferredString} / {experience.remoteByteSizeString}";
        DownloadProgressPercentText.text = progressPercent.ToString();
    }


    private void DownloadCompleteHandler()
    {
        Log($"Tile [{experience.title}]\tDownload Complete");
        
        downloadStatusText.text = "100%";
        
        actionPanelAnimator.SetTrigger(c_ReadyToPlay);
    }


    public void DownloadContent() => experience.Download();
    
    public void DeleteContent() => experience.DeleteContent();

}
