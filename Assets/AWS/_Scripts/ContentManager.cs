/*using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using SimpleDiskUtils; //for getting used and free local storage - multi-platform

using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Runtime;
using System.Threading.Tasks;

// put to dos for this code here:
//TODO JHPH - OVERALL - #1 add redundancy, for pulling down all things for s3, if it can't do it in 3 attempts, broadcast event that says its offline, local content only
//TODO JHPH - OVERALL - #2 use download list to add file and know how many there are so redunacy/retry can occur, perform a count for a progress bar and fire event when finished download is done/list is empty
//TODO JHPH - OVERALL - #3 add structure.JSON support see notes below

public class IntBroadcastEvent : UnityEvent<int>
{

}

public class InformationBroadcastEvent : UnityEvent<List<ChunkedFile>, string>
{

}


public class ContentManager : MonoBehaviour
{

    [Header("AWS Remote storage specific")]
    public string _AWSRemoteBucketName;
    public string _AWSIndentityPoolID;
    public string CognitoIdentityRegion = RegionEndpoint.APSoutheast2.SystemName;
    private RegionEndpoint _AWSCognitoIdentityRegion
    {
        get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
    }
    public string S3Region = RegionEndpoint.APSoutheast2.SystemName;
    private RegionEndpoint _AWSS3Region
    {
        get { return RegionEndpoint.GetBySystemName(S3Region); }
    }

    [Header("Multipart download specific")]
    [Tooltip("The file size limit * in bytes * for breaking the download up in to parts (\"chunks\") and treated as a multipart download.  Default value is 20mb (20971520).")]
    public long LargeFileSizeLimit = 10485760; //20971520;

    [Tooltip("When getting a large file, this is the number of smaller file chunks to bring down in one batch before getting the next. Default value is 10 in a batch.")]
    public int DownloadFileCountInBatch;


    //list to add to from to do above #2
    //batchDownloadList
    public List<string[]> DownloadList = new List<string[]>();

    //chunks of a file in the downloadlist
    //public List<List<ChunkedFile>> ChunkedFileLists

    public List<ChunkedFile> ChunkedFileList = new List<ChunkedFile>();
    public List<List<ChunkedFile>> ChunkedFileListList = new List<List<ChunkedFile>>();

    public float ChunkRequestDelay = 1.5f;

    public List<IEnumerator> WriteByteFunctions = new List<IEnumerator>();


    [Header("Events")]

    //app will stop at the begining (timeline stop) to D/L important. When initial required asset download has completed, tell the app to proceed
    public UnityEvent OnDownloadListEntryComplete = new UnityEvent();
    public UnityEvent OnNewDownloadListEntry = new UnityEvent();


    public UnityEvent OnRequiredAssetsDownloadComplete = new UnityEvent();
    //added the event
    public UnityEvent NoMoreConcurrentDownloads = new UnityEvent();


    public InformationBroadcastEvent OnMultipartDownloadFinished = new InformationBroadcastEvent();

    public int downloadIndex;
    public List<UnityEvent> downloadCompleteWithId;
    public int concurrentDownloads;

    public UnityEvent RemoteListCreated = new UnityEvent();

    //Keeps track of which list is currently being written too
    public int chunkedFileListIndex = 0;

    //handles chunks getting written and stitched async
    public List<int> downloadedChunks = new List<int>();
    List<long> currentDownloadSizes = new List<long>();

    public bool firstChunk = false;
    //long chunkFileSize = 0;
    public int chunkIndex = 0;
    public int maximumMemoryStreams = 20;
    public int memoryStreamIndex = 0;

    //handles writing queue
    public bool writing = false;
    public int totalWriting = 0;

    public int TotalRequested = 0;
    public bool LastRequestSent = false;
    public List<string> RequestReceivedList = new List<string>();


    public List<string> subPaths = new List<string>();

    public bool IsDownloading = false;
    private IEnumerator DownloadingChunkTimeOutCheck;
    
    private static ContentManager _instance;
    public static ContentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("ERROR: ContentManager could not initialise.");
            }
            return _instance;
        }
    }

    private AmazonS3Client _s3Client;
    public AmazonS3Client S3Client
    {
        get
        {
            if (_s3Client == null)
            {
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(_AWSIndentityPoolID, _AWSCognitoIdentityRegion);
                _s3Client = new AmazonS3Client(credentials, _AWSS3Region);
            }
            return _s3Client;
        }

    }

#if UNITY_ANDROID
    public void UsedOnlyForAOTCodeGeneration()
    {
        //Bug reported on github https://github.com/aws/aws-sdk-net/issues/477
        //IL2CPP restrictions: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
        //Inspired workaround: https://docs.unity3d.com/ScriptReference/AndroidJavaObject.Get.html

        AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
        int valueString = jo.Get<int>("what");
        //string stringValue = jo.Get<string>("what");
    }
#endif

    public void DestroyMe()
    {


        Resources.UnloadUnusedAssets();
        StopAllCoroutines();
        GC.Collect();
        Destroy(gameObject);
    }

    private void Awake()
    //public void Init()
    {
        //Debug.Log("ContentManager : Init running");

        _instance = this;

        //This code is needed to get AWS to work - don't ask me what is does! ha :)
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        //DownloadingChunkTimeOutCheck = null;
    }

    public IEnumerator AreWeConnected()
    {
        bool didCheck = false;
        var request = new ListObjectsRequest()
        {
            BucketName = _AWSRemoteBucketName
        };
        S3Client.ListObjectsAsync(request, (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                Debug.Log($"ContentManager : AreWeConnected :  {responseObject}");
                AppManager.Instance.IsInternetConnected = true;
                didCheck = true;
            }
            else
            {
                AppManager.Instance.IsInternetConnected = false;
                didCheck = true;
            }
        });
        for (int i = 0; i < 10f; i++)
        {
            yield return new WaitForSeconds(1f);
            if (didCheck)
            {
                Debug.Log($"ContentManager : AreWeConnected :  didCheck {didCheck}, stopping after {i} secs");
                yield break;
            }
        }
        Debug.Log($"ContentManager : AreWeConnected :  didCheck {didCheck} couldn't connect in 10s");
        yield return null;
    }

    public void GetRemoteFileList(List<string[]> populateThisList)
    {
        //Debug.Log("ContentManager : GetRemoteFileList : ==== Getting REMOTE Listing from: " + _AWSRemoteBucketName);

        //TODO JHPH - add redeundancy here for no S3 connection, kinda started already     
        //no of attempts to failed, break at 3;
        int failedAttempts = 0;

        var request = new ListObjectsRequest()
        {
            BucketName = _AWSRemoteBucketName
        };
        S3Client.ListObjectsAsync(request, (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                //Debug.Log("ContentManager : GetRemoteFileList :  Object count: " + responseObject.Response.S3Objects.Count);
                int i = 1;
                responseObject.Response.S3Objects.ForEach((s3obj) =>
                {

                    //Debug.LogFormat("ContentManager : GetRemoteFileList :  " + i + ">> {0} : {1}", s3obj.Key, s3obj.Size.ToString());
                    populateThisList.Add(new string[2] { s3obj.Key, s3obj.Size.ToString() });
                    //Debug.Log("Remote file path added is " + populateThisList[i]);
                    i++;
                });
                //Debug.Log("ContentManager : GetRemoteFileList :  FINISHED remote Listing.");


                RemoteListCreated.Invoke();
                //Debug.Log("ContentManager : GetRemoteFileList : RemoteListCreated Invoked.");
            }
            else
            {
                failedAttempts++;
                // Debug.LogAssertion("ContentManager : GetRemoteFileList :  ERROR >> Attempt " + failedAttempts + " -  Couldnt download from: " + _AWSRemoteBucketName);
                // Debug.LogAssertion("ContentManager : GetRemoteFileList :  ERROR >> \n" + responseObject.Exception + "\n");

                //if (failedAttempts >= 3)
                //{
                //    break;
                //}
            }
        });
    }

    //contact S3 and get the file form the path in the current bucket in global "_RemoteStorage"
    //TODO add a event to download manager to tell the local list to repopulate when all downloads have finished
    public void DownloadItemFromPath(string path, GameObject initiatorGO = null)
    {

        //Debug.Log("<color=purple>DownloadItemFromPath : ###### - download function started at: " + System.DateTime.Now + "</color>");
        //Debug.Log("DownloadItemFromPath : ?? - AWS path: " + path);

        string pathToUse = Path.Combine(AppManager.Instance.LocalStorage, path);
        long remoteFileSize = FindPathSizeInList(path, AppManager.Instance.RemoteFileList);

        // if the path is a file, not a folder
        if (EntryTypeIsFile(path))
        {
            if (remoteFileSize == 0)
            {
                return;
            }

            //Debug.Log("DownloadItemFromPath : LargeFileSizeLimit chunk is : " + LargeFileSizeLimit);
            DownloadMultiPartFile(path, pathToUse, LargeFileSizeLimit, initiatorGO);


            //Debug.Log("Downloading with multi part file");
        }
        else
        {
            //Debug.Log("DownloadItemFromPath : ?? - Creating new folder at: " + Path.Combine(AppManager.Instance.LocalStorage, path));
            Directory.CreateDirectory(Path.Combine(AppManager.Instance.LocalStorage, path));
            if (Directory.Exists(Path.Combine(AppManager.Instance.LocalStorage, path)))
            {
                Debug.Log("DownloadItemFromPath : OK - Confirmed, new folder at: " + Path.Combine(AppManager.Instance.LocalStorage, path));
                downloadComplete.Invoke();
            }
            CheckConcurrentDownloads();
        }
        //return isFinished;
    }

    public List<string> GetMissingFilesInPath(string path, string ExperiencePath)
    {

        //string localRootPath = AppManager.Instance.LocalStorage  + path;
        //Debug.Log("Eyy the local root path is " + localRootPath);
        List<string> missingFiles = new List<string>();

        foreach (string s in subPaths)
        {
            if (!File.Exists(path + ExperiencePath + s))
            {
                missingFiles.Add(ExperiencePath + s);
                //Debug.Log("File not found: " + ExperiencePath + s);
            }
        }
        //Debug.Log($"[ContentManager : GetMissingFilesInPath] {path} {ExperiencePath} Missing experience file count: { missingFiles.Count}");
        return missingFiles;
    }

    public int downloadQueueIndex = 0;

    public List<string> downloadFilePathQueue = new List<string>();
    public GameObject initiatorLocal;
    public bool lastDownload = false;

    public void DownloadFiles(List<string> filePaths, GameObject initiator)
    {
        //Debug.Log("files in filepath queue " + downloadFilePathQueue.Count);
        initiatorLocal = initiator;
        downloadFilePathQueue = filePaths;
        BeginNextDownloadInQueue();
        //1) Call download
        //2) Get access to it's download complete event
        //3) When it's event gets fired, start next download and repeat from step 2 until subPaths.length files have been downloaded
        //4) CheckConcurrentDownloads
    }

    public UnityEvent downloadQueueComplete = new UnityEvent();

    public void BeginNextDownloadInQueue()
    {
        //Debug.Log("I am starting next download in queue");
        //Debug.Log("files in filepath queue  " + downloadFilePathQueue.Count);
        //Debug.Log("download queue index is " + downloadQueueIndex + "and subpath queue is " + subPaths.Count);
        downloadComplete.RemoveAllListeners();
        DownloadMultiPartFile(downloadFilePathQueue[downloadQueueIndex], Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[downloadQueueIndex]), LargeFileSizeLimit, initiatorLocal);
        //Debug.Log("Look at me, I'm " + downloadQueueIndex + "and I'm downloading this file: " + Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[downloadQueueIndex]));
        if (downloadQueueIndex < downloadFilePathQueue.Count - 1)
        {
            downloadComplete.AddListener(BeginNextDownloadInQueue);
            //Debug.Log("This isn't  the last download so we're not saying that it is, and we're triggering a download and subscribing ourself to the end of it");

        }

        if (downloadQueueIndex == downloadFilePathQueue.Count - 1)
        {

            //HERE this one does fire
            //Debug.Log("Apparently this is the last download to be triggered - does it happen before stiching");
            lastDownload = true;
        }
        downloadQueueIndex++;
    }

    //  initiatorGO is optional, only required if when need to update a GO wil the download progress
    private void DownloadSingleFile(string path, string pathToUse, GameObject initiatorGO = null)
    {

        bool isFinished = false;
        int failedAttempts = 0;

        //set up request object
        GetObjectRequest request = new GetObjectRequest
        {
            BucketName = _AWSRemoteBucketName,
            Key = path
        };

        // conatct S3 and download a single *NON CHUNKED* file
        S3Client.GetObjectAsync(request, (responseObject) =>
        {

            //Debug.Log("DownloadItemFromPath : ?? - Downloading new file to: " + pathToUse);
            //Debug.Log("<color=blue>DownloadItemFromPath : ###### - download started at: " + System.DateTime.Now + "</color>");

            var response = responseObject.Response;
            if (response.ResponseStream != null)
            {
                byte[] data = null;
                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        var buffer = new byte[1024];
                        var bytesRead = default(int);
                        int counter = 0;
                        while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memory.Write(buffer, 0, bytesRead);


                            counter++;


                        }
                        data = memory.GetBuffer();
                    }
                }
                // Create a new stream to write to the file
                BinaryWriter Writer = new BinaryWriter(File.Create(pathToUse));
                // Writer raw data                
                Writer.Write(data);
                Writer.Close();

                if (File.Exists(pathToUse))
                {
                    // Debug.Log("DownloadSingleFile : OK - Confirmed, new file downloaded");
                    isFinished = true;

                }
            }
            else

            {
                failedAttempts++;
                //Debug.LogError("DownloadSingleFile : ERROR >> Attempt " + failedAttempts + " -  Couldn't download " + path);
                //Debug.LogError("DownloadSingleFile : ERROR >> \n" + responseObject.Exception + "\n");

                isFinished = false;
            }
        });
    }



    int downloadsStarted = 0;

    // Function available that will download a file in multiple partes
    //  initiatorGO is optional, only required if when need to update a GO wil the download progress
    private void DownloadMultiPartFile(string path, string pathToUse, long chunkLength, GameObject initiatorGO = null)
    {
        IsDownloading = true;
        downloadCompleteWithId.Add(new UnityEvent());
        downloadIndex++;
        concurrentDownloads++;
        downloadsStarted++;

        //for missing chunks if there is any
        LastRequestSent = false;
        TotalRequested = 0;
        RequestReceivedList.Clear();
        RequestReceivedList.TrimExcess();

        //Debug.Log("Downloads started is " + downloadsStarted);
        //Debug.Log("Currently running downloads is " + concurrentDownloads);
        //Debug.Log("DownloadMultiPartFile : starting a multi Download and the index is " + downloadIndex);

        //it's only counting 3 here, but its definitely running through six

        //tell the downloadManager when a D/L is complete

        long remoteFileSize = FindPathSizeInList(path, AppManager.Instance.RemoteFileList);
        int numberOfChunks = (int)Math.Ceiling((double)remoteFileSize / chunkLength);

        //Debug.Log("Created a new event and DL index is  " + downloadIndex);
        //Debug.Log("DownloadMultiPartFile : Chunking into " + numberOfChunks + " parts. The file: " + path + " is " + remoteFileSize + "b, was chunked by groups of " + LargeFileSizeLimit + "b");
        //Debug.Log("Downloaded " + downloadedChunks + " of " + numberOfChunks + " file chunks.");

        //List of chunked files to be written and then stitched
        List<ChunkedFile> ChunkedFileListInstance = new List<ChunkedFile>();
        //they were stitching before, may have broken that in my poking around - 


        //Added to a list of lists, so we can run many lists concurrently
        ChunkedFileListList.Add(ChunkedFileListInstance);

        currentDownloadSizes.Add(0);


        //Index of current list, which is passed to async routines so each routine writes to the right list

        chunkedFileListIndex++;
        //an event to fire when multipart downloads are finished

        //this subscribes it to that event, and i am pretty sure only gets called once per download
        OnMultipartDownloadFinished.AddListener((List<ChunkedFile> l, string s) => { StartCoroutine(ReassembleMultipartFiles(l, s)); });
        for (int i = 1; i < (numberOfChunks + 1); i++)
        {

            long startBytes = ((i * chunkLength) - chunkLength);
            long endbytes = remoteFileSize;

            if (i != (numberOfChunks))
            {
                endbytes = (i * chunkLength) - 1;
            }

            //work out new path/filename to use for chunks
            string oldFilename = Path.GetFileName(pathToUse);

            string filenameWOext = Path.GetFileNameWithoutExtension(pathToUse);

            string newFilename = pathToUse.Replace(oldFilename, filenameWOext + "_DL-PART_" + i + Path.GetExtension(oldFilename));
            //if chunk is already there, don't add it for downloads
            //Debug.Log("Is the chunk for  " + newFilename + "  already there? " + File.Exists(newFilename));
            ChunkedFileListList[chunkedFileListIndex - 1].Add(new ChunkedFile(newFilename, startBytes, endbytes, File.Exists(newFilename)));
            if (!File.Exists(newFilename))
            {
                RequestReceivedList.Add(newFilename);
            }
            //Debug.LogFormat("Making Chunk {0}  |  name: {1}  -  start: {2}  -  end: {3} ", i, newFilename, startBytes, endbytes);

        }

        //when the first chunk finishes downloading, change state to currently downloading and set this to false
        firstChunk = true;
        downloadedChunks.Add(0);
        //Debug.Log("First chunks chunkedFileListIndex = "+ chunkedFileListIndex);

        StartCoroutine(RequestChunk(chunkedFileListIndex, path, pathToUse, firstChunk, numberOfChunks, remoteFileSize, initiatorGO));

    }
    

    private IEnumerator DownloadChunkTimeOutCounter(GameObject initiatorGO)
    {
        //ok so - 
        Debug.Log("DownloadingChunkTimeOutCheck : Waiting for a chunk");
        yield return new WaitForSeconds(180f);

        Debug.Log("DownloadingChunkTimeOutCheck : chunk timer is up, now what to do?");
        IsDownloading = false;
        lastDownload = true;
        Debug.Log($"DownloadingChunkTimeOutCheck : cleaning up DLs. lastDownload is {lastDownload}");
        if (initiatorGO != null)
        {
            Debug.Log($"initiatorGO.name = {initiatorGO.name}");
            initiatorGO.GetComponent<Selection_Tile>().SetContentState(Selection_Tile.ContentStates.needToDownload);
            initiatorGO.GetComponent<Selection_Tile>().SetDownloaderState(Selection_Tile.DownloadStates.readyToDownload);
            initiatorGO.GetComponent<Selection_Tile>().IsExperienceReadyToPlay();
        }
        concurrentDownloads = 0;
        Debug.Log($"DownloadingChunkTimeOutCheck : cleaning up to restyart. CleanUpDownloads()");
        CleanUpDownloads();
        yield return new WaitForSeconds(2f);
        Debug.Log($"DownloadingChunkTimeOutCheck : stopping important coroutines");
        StopAllCoroutines();
    }



    bool allChunksDownloaded;
    public IEnumerator loopedRequest;
    public List<IEnumerator> LoopedRequests = new List<IEnumerator>();
    bool firstLoopCompleted;

    //This requests the chunk, which is already asyncrounous, but as it is a coroutine, can handle the response async as well - this lets us space out writing bytes etc
    IEnumerator RequestChunk(int chunkedFileListIndexInstance, string path, string pathToUse, bool firstChunk, int numberOfChunks, long remoteFileSize, GameObject initiatorGO = null)
    {

        //Debug.Log($"Chunk requested chunkedFileListIndex: {chunkedFileListIndex} chunkedFileListIndexInstance: {chunkedFileListIndexInstance} and downloadIndex {downloadIndex} and ChunkedFileListList[chunkedFileListIndexInstance - 1].Count {ChunkedFileListList[chunkedFileListIndexInstance - 1].Count}");
        foreach (ChunkedFile chunk in ChunkedFileListList[chunkedFileListIndexInstance - 1])
        {
            //Debug.Log("chunk.chunkedFilePath: " + chunk.chunkedFilePath + ", numberOfChunks:" + numberOfChunks + ", pathToUse: " + pathToUse);
            
            //TODO JHPH timeout for downloads, inprogress
            //if (DownloadingChunkTimeOutCheck == null)
            //{
            //    Debug.Log("DownloadingChunkTimeOutCheck : Starting for " + chunk.chunkedFilePath);
            //    DownloadingChunkTimeOutCheck = DownloadChunkTimeOutCounter(initiatorGO);
            //    StartCoroutine(DownloadingChunkTimeOutCheck);
            //}
            //else
            //{
            //    StopCoroutine(DownloadingChunkTimeOutCheck);
            //    Debug.Log("DownloadingChunkTimeOutCheck : Stoping for a new one");
            //    DownloadingChunkTimeOutCheck = null;
            //    DownloadingChunkTimeOutCheck = DownloadChunkTimeOutCounter(initiatorGO);
            //    Debug.Log("DownloadingChunkTimeOutCheck : Starting for " + chunk.chunkedFilePath);
            //    StartCoroutine(DownloadingChunkTimeOutCheck);
            //}
            //if (IsDownloading)
            //{
            if (File.Exists(chunk.chunkedFilePath) && numberOfChunks != 1)
            {
                downloadedChunks[chunkedFileListIndexInstance - 1]++;
                //Debug.Log($"{chunk.chunkedFilePath} exists,  no need to request it again.");
                //get the size of the chunk in bytes and then add to the downloaded size
                long chunkFileSize = chunk.endByteRange - chunk.startByteRange;
                currentDownloadSizes[chunkedFileListIndexInstance - 1] += chunkFileSize;
            }
            else if ((File.Exists(chunk.chunkedFilePath) && numberOfChunks == 1) || !File.Exists(chunk.chunkedFilePath))
            {

                if (File.Exists(chunk.chunkedFilePath) || (File.Exists(chunk.chunkedFilePath) && numberOfChunks == 1))
                {
                    //Debug.Log("chunk.chunkedFilePath: " + chunk.chunkedFilePath + " is there" + ", numberOfChunks:" + numberOfChunks);
                    //Debug.Log("i should be doing something here - deleting single chunk to go through the process again");
                    DeleteOnStorage(chunk.chunkedFilePath);
                }
                else
                {
                    //Debug.Log($"chunk.chunkedFilePath: " + chunk.chunkedFilePath + " is not there - requesting" + ", numberOfChunks:" + numberOfChunks);
                }

                //Debug.Log("chunk.chunkedFilePath: " + chunk.chunkedFilePath + " is not there - requesting");
                //This lets us control how many requests have gone out at any given time
                bool notified = false;
                //Debug.Log("memoryStreamIndex: " + memoryStreamIndex + "  -  maximumMemoryStreams: " + maximumMemoryStreams);

                while (memoryStreamIndex > maximumMemoryStreams)
                {
                    if (!notified)
                    {
                        notified = true;
                    }
                    yield return null;
                }



                memoryStreamIndex++;

                //yield return new WaitForSeconds(1);

                string chunkName = chunk.chunkedFilePath;
                long startRange = chunk.startByteRange;
                long endRange = chunk.endByteRange;
                bool skipChunk = chunk.isDownloaded;

                //Debug.LogFormat("DOWNLOADING - Chunk  |  name: {0}  -  start: {1}  -  end: {2} ", chunkName, startRange, endRange);


                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _AWSRemoteBucketName,
                    Key = path,
                    ByteRange = new ByteRange(startRange, endRange)
                };

                chunk.downloadStartTime = DateTime.Now;
                // conatct S3 and download a single *NON CHUNKED* file
                //Debug.Log("About To send off a request for " + chunkName);
                //a request has been recieved, but not sure yet if it will write to disk

                //if index is less than max, pad it a few seconds this way the first few responses are coming in at once
                //we've had issues with the first few response not writing to disk or go missing entirely
                //after we reach max it will behave as normal
                if (memoryStreamIndex < maximumMemoryStreams && numberOfChunks > 1)
                {
                    yield return new WaitForSeconds(1);
                    //Debug.Log("But waiting for 2 s because the streams are less than " + maximumMemoryStreams + " : " + memoryStreamIndex);
                }

                S3Client.GetObjectAsync(request, (responseObject) =>
                {
                    //Debug.Log("Downloading new file to: " + chunkName);

                    var response = responseObject.Response;

                    //Debug.Log(chunkName + "###_ <color=blue>DownloadMultiPartFile : ###### - download started at: " + System.DateTime.Now + "</color>");

                    if (response.ResponseStream != null)
                    {
                        //when the response comes back the writing is done asyncronously also, this means we can control how it write to memory to avoid too much getting written at the same time
                        IEnumerator WriteByteInstance = WriteBytes(chunkedFileListIndexInstance, response, pathToUse, chunkName, chunk, numberOfChunks, remoteFileSize, initiatorGO, startRange, endRange);
                        StartCoroutine(WriteByteInstance);

                        response = null;
                        request = null;

                        //Debug.Log($"{chunkName} ###_ I've receieved a response for {chunkName}");
                    }

                    //if (response.HttpStatusCode == System.Net.HttpStatusCode.PartialContent)
                    //{
                    //    Debug.Log($"{chunkName} ###_ HttpStatusCode was for PartialContent {chunkName}");
                    //}
                    //else
                    //{
                    //    //failedAttempts++;
                    //    Debug.Log(chunkName + "###_ code was NOT for PartialContent " + chunkName);
                    //    Debug.LogError(chunkName + "###_ DownloadMultiPartFile : ERROR >> -  Couldn't download " + path);
                    //    Debug.LogError(chunkName + "###_ DownloadMultiPartFile : ERROR >> \n" + responseObject.Exception + "\n");
                    //    //isFinished = false;

                    //}

                    //Debug.Log(chunkName + "###_ still inside request for " + chunkName);

                });

                //Debug.Log(chunkName + "###_ still inside foreach loop number " + (chunkedFileListIndexInstance - 1));

            }

            //Debug.Log("RequestChunk - downloadedChunks[chunkedFileListIndexInstance - 1] = " + downloadedChunks[chunkedFileListIndexInstance - 1]);
            yield return null;
        }
        //Debug.Log("RequestChunk method finished, downloadedChunks[chunkedFileListIndexInstance - 1] = " + downloadedChunks[chunkedFileListIndexInstance - 1]);
        yield return null;

    }

    bool loopRequestStarted;
    byte[] buffer = new byte[1024];

    //Writes a list of bytes to a file
    public IEnumerator WriteBytes(int chunkedFileListIndexInstance, GetObjectResponse response, string pathToUse, string chunkName, ChunkedFile chunk, int numberOfChunks, long remoteFileSize, GameObject initiatorGO, long startRange, long endRange)
    {
        //Debug.Log("Bytes writing requested" + chunkedFileListIndexInstance + " and " + downloadIndex);

        byte[] data = null;

        using (StreamReader reader = new StreamReader(response.ResponseStream))
        {

            using (MemoryStream memory = new MemoryStream())
            {
                bool notificationWriting = false;

                //this means there's never too many streams getting written at once
                while (totalWriting > 0)
                {
                    if (!notificationWriting)
                    {
                        //Debug.Log("Max total writing already!");
                        notificationWriting = true;
                    }

                    yield return null;
                }

                writing = true;
                totalWriting++;
                //Debug.Log("Another one starts writing, bringing total to " + totalWriting);
                var bytesRead = default(int);
                int counter = 0;


                while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Debug.Log(counter);
                    memory.Write(buffer, 0, bytesRead);
                    counter++;
                    if (counter > 100)
                    {
                        yield return null;
                        counter = 0;
                        //Debug.Log("Counter ticks over");
                    }

                }
                //Debug.Log("bytes read while loop done");
                yield return null;


                data = memory.ToArray();
                //memory.Dispose();
                yield return null;

                //memory.FlushAsync();
                memory.Close(); //TODO JHPH this was commented out originally
                reader.Close(); //TODO JHPH this was originally dispose

            }
        }

        using (BinaryWriter Writer = new BinaryWriter(File.Create(chunkName)))
        {
            yield return null;

            Writer.Write(data);
            data = null;

            yield return null;

            Writer.Flush();

            yield return null;

            Writer.Close();

        }
        // Writer raw data   

        yield return null;


        writing = false;
        memoryStreamIndex--;
        totalWriting--;

        System.GC.Collect();


        if (File.Exists(chunkName))
        {

            if (initiatorGO != null)
            {

                if (firstChunk && lastDownload) // if its the first chunk downloaded, then we finally have progress numbers to see, change state to show progress UI
                {
                    initiatorGO.GetComponent<Selection_Tile>().SetContentState(Selection_Tile.ContentStates.currentlyDownloading);
                    firstChunk = false;
                }

            }

            //Debug.Log("DownloadMultiPartFile : OK - Confirmed, new file downloaded to: " + chunkName);
            //isFinished = true;
            //Debug.Log("<color=blue>DownloadMultiPartFile : ###### - download ended at: " + System.DateTime.Now + "</color>");

            chunk.downloadEndTime = DateTime.Now;

            //what's the download duration
            //TimeSpan timeDiff = chunk.downloadEndTime.Subtract(chunk.downloadStartTime);
            //string res = String.Format("{0}:{1}:{2}", timeDiff.Hours, timeDiff.Minutes, timeDiff.Seconds);

            chunk.isDownloaded = true;
            RequestReceivedList.Remove(chunkName);
            //Debug.Log($"[ContentManager : WriteBytes] chunkedFileListIndexInstance - 1 = {chunkedFileListIndexInstance - 1}");
            downloadedChunks[chunkedFileListIndexInstance - 1]++;
            //Debug.Log("Number of chunks is " + numberOfChunks + " and downloaded chunks is " + downloadedChunks[chunkedFileListIndexInstance - 1]);
            //if there is an download initiator, update the values

            if (initiatorGO != null && lastDownload)
            {

                //get the size of the chunk in bytes and then add to the downloaded size
                long chunkFileSize = endRange - startRange;
                currentDownloadSizes[chunkedFileListIndexInstance - 1] += chunkFileSize;

                float DLProgValue = (float)currentDownloadSizes[chunkedFileListIndexInstance - 1] / (float)remoteFileSize;
                string DLCurSize = ContentManager.Instance.ConvertBytesToReadableValue(currentDownloadSizes[chunkedFileListIndexInstance - 1]);
                initiatorGO.GetComponent<Selection_Tile>().SelectionIsDownloading = true;
                initiatorGO.GetComponent<Selection_Tile>().SetDownloadProgressValue(DLProgValue);
                //initiatorGO.GetComponent<Selection_Tile>().SetDownloadTextValue(DLCurSize);
                //Debug.Log("=== Download progress percentage : " + String.Format("{0:0}", (DLProgValue) * 100) + "%");
                //Debug.Log("=== Download progress currentDownloadSize : " + DLCurSize);


            }

            if (downloadedChunks[chunkedFileListIndexInstance - 1] >= numberOfChunks || LastRequestSent)
            {

                //Debug.Log($"Running check on gate DLCHUNKS {downloadedChunks[chunkedFileListIndexInstance - 1]}  >=  {numberOfChunks}");
                //Debug.Log($"Calling reassemble with download index of {downloadIndex}");

                //above 0 means there's missing chunks and we should request again
                if (RequestReceivedList.Count > 0)
                {
                    foreach (string RequestReceived in RequestReceivedList)
                    {
                        Debug.Log($"###_ we're done but I'm still missing this chunk {RequestReceived} of {downloadedChunks[chunkedFileListIndexInstance - 1]} downloadedChunks");
                        downloadedChunks[chunkedFileListIndexInstance - 1]--;
                        yield return null;
                    }

                    //if (chunkedFileListIndex <= 0) chunkedFileListIndex = 1;
                    //Debug.Log($"ChunkedFileListIndex given is {chunkedFileListIndex}");
                    yield return null;
                    //Debug.Log($"AppManager.Instance.LocalStorage given is {AppManager.Instance.LocalStorage}");
                    //Debug.Log($"downloadFilePathQueue.Count given is {downloadFilePathQueue.Count}");                     
                    //Debug.Log($"downloadQueueIndex given is {downloadQueueIndex}");
                    //Debug.Log($"downloadFilePathQueue[downloadQueueIndex] given is {downloadFilePathQueue[downloadQueueIndex]}");
                    //Debug.Log($"Path.Combine given is {Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[downloadQueueIndex])}");
                    //Debug.Log($"pathToUse given is {pathToUse}");
                    //Debug.Log($"numberOfChunks given is {numberOfChunks}");
                    //Debug.Log($"remoteFileSize given is {remoteFileSize}");
                    //Debug.Log($"initiatorGO given is {initiatorGO}");
                    //Debug.Log($"chunkedFilePath given is {chunk.chunkedFilePath}");

                    StartCoroutine(RequestChunk(chunkedFileListIndexInstance, Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[downloadQueueIndex]), pathToUse, false, numberOfChunks, remoteFileSize, initiatorGO));
                }
                else  // good to go
                {
                    //StopCoroutine("RerequestMissingChunks");
                    if (initiatorGO != null && lastDownload)
                    {
                        //Debug.Log("Changing the state to Selection_Tile.ContentStates.processDownload");
                        initiatorGO.GetComponent<Selection_Tile>().SetContentState(Selection_Tile.ContentStates.processDownload);
                        initiatorGO.GetComponent<Selection_Tile>().SelectionIsDownloading = false;
                    }
                    //Debug.LogFormat("Reassembling {0} in {1}", ChunkedFileListList[chunkedFileListIndexInstance - 1], pathToUse);
                    StartCoroutine(ReassembleMultipartFiles(ChunkedFileListList[chunkedFileListIndexInstance - 1], pathToUse));
                }
            }
            else
            {
                //Debug.Log("stoping re-request");
                //StopCoroutine("RerequestMissingChunks");
                //Debug.Log("Starting re-request in else");
                //Debug.Log("chunkedFileListIndexInstance: " + chunkedFileListIndexInstance);
                //Debug.Log("downloadQueueIndex: " + downloadQueueIndex);
                //Debug.Log("Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[downloadQueueIndex-1]): " + Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[(downloadQueueIndex - 1)]));
                //Debug.Log("pathToUse: " + pathToUse);
                //Debug.Log("false: ");
                //Debug.Log("numberOfChunks: " + numberOfChunks);
                //Debug.Log("remoteFileSize: " + remoteFileSize);
                //Debug.Log("initiatorGO: " + initiatorGO.name);
                //StartCoroutine(RerequestMissingChunks(chunkedFileListIndexInstance, Path.Combine(AppManager.Instance.LocalStorage, downloadFilePathQueue[(downloadQueueIndex-1)]), pathToUse, false, numberOfChunks, remoteFileSize, initiatorGO));
                //still going?
                //or stuck?

                //Debug.Log($"{chunkName} ###_ I'm in the else: Number of chunks is {numberOfChunks} and downloaded chunks is {downloadedChunks[chunkedFileListIndexInstance - 1]}");

            }

        }
        else
        {
            //Debug.Log(chunkName + "###_ file doesn't exist after writing?");
        }

    }

    public IEnumerator RerequestMissingChunks(int chunkedFileListIndexInstance, string path, string pathToUse, bool firstChunk, int numberOfChunks, long remoteFileSize, GameObject initiatorGO = null)
    {
        //Debug.Log("In the re-request co-routine, waiting for 30");
        yield return new WaitForSeconds(30f);
        //Debug.Log("Starting re-request");
        StartCoroutine(RequestChunk(chunkedFileListIndexInstance, path, pathToUse, false, numberOfChunks, remoteFileSize, initiatorGO));

    }

    int downloadsFinished = 0;
    //Asyncronously stitches, using the job system 
    public IEnumerator ReassembleMultipartFiles(List<ChunkedFile> chunkedFileList, string reassembleAtThisPath)
    {
        //Debug.Log("Now we're INSIDE reassemble with download index of " + downloadIndex);
        //foreach (ChunkedFile downloadedChunk in chunkedFileList)
        //{
        //Debug.Log("ReassembleMultipartFiles : Starting Task for " + downloadedChunk.chunkedFilePath);
        Task StitchTask = Task.Run(() => { AsyncStitchFile(chunkedFileList, reassembleAtThisPath); });

        while (!StitchTask.IsCompleted) // JHPH TODO while not complete and size is not the same as expected 
        {
            yield return null;
        }
        //Debug.Log("IsCompleted : " + (StitchTask.IsCompleted ? " it's complete" : "Didn't complete"));
        //Debug.Log("IsCanceled : " + (StitchTask.IsCanceled ? " it's canceled" : "Didn't cancel"));
        // Debug.Log("IsFaulted : " + (StitchTask.IsFaulted ? " it's faulted" : "Didn't fault"));
        if (StitchTask.IsFaulted)
        {
            // Debug.Log("StitchTask Exception : " + StitchTask.Exception);
        }
        StitchTask.Dispose();
        //}

        //Debug.Log("AsyncStitchFile : checking to see if [" + reassembleAtThisPath + "] exists");
        //if (File.Exists(reassembleAtThisPath))
        //{

        //    foreach (ChunkedFile downloadedChunk in chunkedFileList)
        //    {
        //        Debug.Log("DELETING chunk - " + downloadedChunk.chunkedFilePath);

        //        File.Delete(downloadedChunk.chunkedFilePath);
        //    }

        //    Debug.Log("Clearing chunked list");

        //}

        CleanUpDownloads();

    }
    public void CleanUpDownloads()
    {
        //Debug.Log("CleanUpDownloads : this after the event wher DL index is " + downloadIndex);
        downloadCompleteWithId[downloadIndex - 1].Invoke();

        //HERE - this does not seem to fire as lastDownload is not rue when stiching happens?
        //Debug.Log("CleanUpDownloads : i'm hoping this next block will fire because lastDownload is " + lastDownload);
        if (lastDownload)
        {
            //Debug.Log("CleanUpDownloads : last download is called so we're saying last download");
            downloadQueueComplete.Invoke();
            //set queIndex to be 0
            downloadQueueIndex = 0;
            lastDownload = false;
        }
        downloadComplete.Invoke();
        downloadsFinished++;
        //Debug.Log("CleanUpDownloads : Downloads finished " + downloadsFinished);
        if (concurrentDownloads > 0) concurrentDownloads--;
        //Debug.Log("CleanUpDownloads : Download index is " + downloadIndex);
        CheckConcurrentDownloads();


        System.GC.Collect();

        //Debug.Log("ReassembleMultipartFiles : Garbage collection called : System.GC.Collect()");
    }
    public UnityEvent downloadComplete = new UnityEvent();
    public void CheckConcurrentDownloads()
    {
        //Debug.Log("Currently running downloads is " + concurrentDownloads);
        if (concurrentDownloads == 0)
        {

            //Debug.Log("[ContentManager : CheckConcurrentDownloads] Clearing and trimming all lists.");

            DownloadList.Clear();
            DownloadList.TrimExcess();

            ChunkedFileList.Clear();
            ChunkedFileList.TrimExcess();

            foreach (List<ChunkedFile> cf in ChunkedFileListList)
            {
                cf.Clear();
                cf.TrimExcess();
            }

            ChunkedFileListList.Clear();
            ChunkedFileListList.TrimExcess();

            WriteByteFunctions.Clear();
            WriteByteFunctions.TrimExcess();

            downloadIndex = 0;
            downloadedChunks.Clear();
            downloadedChunks.TrimExcess();

            chunkedFileListIndex = 0;
            currentDownloadSizes.Clear();
            currentDownloadSizes.TrimExcess();

            allChunksDownloaded = false;

            downloadCompleteWithId.Clear();
            downloadCompleteWithId.TrimExcess();
            
            //for missing chunks if there is any
            LastRequestSent = false;
            TotalRequested = 0;
            RequestReceivedList.Clear();
            RequestReceivedList.TrimExcess();

            //TODO JHPH missing chunk fix
            //StopCoroutine(DownloadingChunkTimeOutCheck);
            //Debug.Log("DownloadingChunkTimeOutCheck : Stoping beacuse is done or connection lost");
            //DownloadingChunkTimeOutCheck = null;

            memoryStreamIndex = 0;

            NoMoreConcurrentDownloads.Invoke();
            IsDownloading = false;
            //Debug.Log("CheckConcurrentDownloads : No more front loading - Currently running downloads is " + concurrentDownloads);
        }
    }


    void AsyncVideoFileAppend(ChunkedFile downloadedChunk, string reassembleAtThisPath)
    {
        //Debug.Log("AsyncStitchFile : starting method, setting up file stream for file "+ reassembleAtThisPath);
        using (var fileStream = new FileStream(reassembleAtThisPath, FileMode.Append, FileAccess.Write, FileShare.None))
        using (var bw = new BinaryWriter(fileStream))
        {
            //Debug.Log("AsyncStitchFile : in the binary writer : fileStream: " + fileStream);
            //when finished construct back together

            //Debug.Log("chunkedFilePath - " + downloadedChunk.chunkedFilePath + " | downloadStartTime - " + downloadedChunk.downloadStartTime + " | downloadEndTime - " + downloadedChunk.downloadEndTime + " | isDownloaded - " + downloadedChunk.isDownloaded);

            byte[] bytes = File.ReadAllBytes(downloadedChunk.chunkedFilePath);

            bw.Write(bytes);
            bw.Close();
            fileStream.Close();
            bytes = null;
        }

    }

    public void AsyncStitchFile(List<ChunkedFile> chunkedFileList, string reassembleAtThisPath)
    {
        //Debug.Log("AsyncStitchFile : starting method, setting up filestream");
        using (var fileStream = new FileStream(reassembleAtThisPath, FileMode.Append, FileAccess.Write, FileShare.None))
        using (var bw = new BinaryWriter(fileStream))
        {
            //Debug.Log("AsyncStitchFile : in the binary writer : fileStream: "+ fileStream);
            //when finished construct back together
            foreach (ChunkedFile downloadedChunk in chunkedFileList)
            {

                //Debug.Log("chunkedFilePath - " + downloadedChunk.chunkedFilePath + " | downloadStartTime - " + downloadedChunk.downloadStartTime + " | downloadEndTime - " + downloadedChunk.downloadEndTime + " | isDownloaded - " + downloadedChunk.isDownloaded);

                byte[] bytes = File.ReadAllBytes(downloadedChunk.chunkedFilePath);

                bw.Write(bytes);
                bytes = null;

                bw.Flush();
            }

            //Debug.Log("AsyncStitchFile : bw.Close();");
            bw.Close();
            //Debug.Log("AsyncStitchFile : bw.Dispose();");
            bw.Dispose();

            //FileAttributes stitchedFile = new FileAttributes(); 
            //Debug.Log("AsyncStitchFile : checking to see if [" + reassembleAtThisPath + "] exists");
            if (File.Exists(reassembleAtThisPath))
            {
                // Debug.Log("Deleting chunks");
                foreach (ChunkedFile downloadedChunk in chunkedFileList)
                {
                    //Debug.Log("DELETING chunk - " + downloadedChunk.chunkedFilePath);

                    File.Delete(downloadedChunk.chunkedFilePath);
                }

                //Debug.Log("Clearing chunked list");
                chunkedFileList.Clear();
                chunkedFileList.TrimExcess();

            }
        }

    }

    void AddToDownloadQueue(string path, string pathToUse)
    {
        DownloadList.Add(new string[3] { "0", path, pathToUse });

        OnNewDownloadListEntry.Invoke();

    }

    //downloadCompleteWithId[downloadIndex - 1].AddListener(DownloadListManager);
    void DownloadFromList()
    {
        //Debug.Log("DOWNLOAD COMPLETE! - index: " + downloadIndex);
    }

    public long FindFreeStorage()
    {
        long freeSpace = 0;
        //Debug.Log($"Avail: {DiskUtils.CheckAvailableSpace()} Busy: {DiskUtils.CheckBusySpace()} Total: {DiskUtils.CheckTotalSpace()}");

        // free space in MB converted to bytes and then minus a buffer of 100mb so normal OS operation can still occur
        freeSpace = ((long)DiskUtils.CheckAvailableSpace() * 1048576) - 104857600;  //104857600 is 100 mb in bytes
        //Debug.Log($"FindFreeStorage : freespace {freeSpace}");
        return freeSpace;
    }

    public long FindLocalSizeOfExperience(string path)
    {
        long downloadedVideo = GetSizeOnStorageAlt(path + "/video");
        long downloadedAudio = GetSizeOnStorageAlt(path + "/audio");
        //Debug.Log($"Path to get the size of: {path}  |  Audio: {downloadedAudio}  |  Video: {downloadedVideo}");
        return downloadedVideo + downloadedAudio;
    }

    public long FindPathSizeInList(String pathToFind, List<string[]> listToSearch)
    {
        long returnValue = 0;
        foreach (var item in listToSearch)
        {
            //Debug.Log($"COMPARING: item[0]: {item[0]}   |   pathToFind: {pathToFind}");
            if (pathToFind == item[0])
            {
                //Debug.Log($"FOUND: pathToFind: {pathToFind} in item[0]: {item[0]}  with a size of {item[1]}");
                returnValue = (long)Convert.ToDouble(item[1]);
            }
        }
        //Debug.Log($"FILE size = {returnValue}");
        return returnValue;
    }

    public long FindSizeOfList(List<string> FilesToFind, List<string[]> listToSearch, string expPath)
    {
        long returnValue = 0;
        foreach (var item in listToSearch)
        {

            foreach (var findItem in FilesToFind)
            {
                //if (pathToFind == item[0])
                //Debug.Log($"COMPARING: item[0]: {item[0]}   |   pathToFind: {expPath + findItem}");
                if (item[0].Contains(expPath + findItem))
                {
                    //Debug.Log($"FOUND: pathToFind: {expPath + findItem} in item[0]: {item[0]}  with a size of {item[1]}");
                    returnValue += (long)Convert.ToDouble(item[1]);
                }
            }
        }
        //Debug.Log($"FILE size = {returnValue}");
        return returnValue;
    }

    public string ConvertBytesToReadableValue(long bytesValue)
    {
        //conver a byte valkue to a human readable string
        double returnValue = 0;

        string sizeAbbr = "";
        if (bytesValue >= 1073741824)
        {
            sizeAbbr = "GB";
            returnValue = (((bytesValue / 1024f) / 1024f) / 1024f);
            returnValue = Math.Round(returnValue, 2);
        }
        else
        {
            sizeAbbr = "MB";
            returnValue = ((bytesValue / 1024f) / 1024f);
            returnValue = Math.Round(returnValue, 0);
        }

        //Debug.Log($"returnValue size = {returnValue} {convertTo}");

        return returnValue.ToString() + " " + sizeAbbr;
    }


    //For //Debug
    public string PrintListingToString(List<string[]> listToPrint) //take a file/directory list and stringify it with some formatting to list out like a tree structure
    {
        string tempListingText = "";
        int i = 1;
        foreach (var item in listToPrint)
        {
            if (EntryTypeIsFile(item[0]))
                tempListingText += i + ":     F__ " + item[0] + " - " + item[1];
            else
                tempListingText += i + ": D>> " + item[0] + " - " + item[1];

            tempListingText += "\n";
            i++;
        }
        tempListingText += "\n " + (i - 1) + " Files/folders found.";
        return tempListingText;
    }

    public bool EntryTypeIsFile(string entryType) //true == file, false == folder
    {
        if (entryType.EndsWith("/"))
            return false;
        else
            return true;
    }

    public long GetSizeOnStorage(DirectoryInfo directoryToGet) //gets the size of the path be it folder or file

    {

        //Debug.Log("GetSizeOnStorage : " + directoryToGet);

        long size = 0;
        // Add file sizes.
        FileInfo[] fis = directoryToGet.GetFiles();
        foreach (FileInfo fi in fis)
        {
            size += fi.Length;
        }
        // Add subdirectory sizes.
        DirectoryInfo[] dis = directoryToGet.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            size += GetSizeOnStorage(di);
        }
        return size;
    }

    public long GetSizeOnStorageAlt(String path) //ALT way - gets the size of the path be it folder or file
    {
        //Debug.Log("GetSizeOnStorageAlt : " + path);

        long size = 0;
        string[] entries = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories);
        Array.Sort(entries);

        foreach (var item in entries)
        {

            if (Directory.Exists(item))
            {
                DirectoryInfo dir = new DirectoryInfo(item);
                size += 0;
            }
            else if (File.Exists(item))
            {
                FileInfo file = new FileInfo(item);
                size += file.Length;
            }
        }

        return size;
    }

    public void DeleteOnStorage(string path) //deletes all files in a path (inc root) or just some from a list of extention wildcards
    {
        //Debug.Log("DeleteOnStorage : We're in - is there a file called? : " + path);
        if (File.Exists(path))
        {
            //Debug.Log("DeleteOnStorage : Deleting this file : " + path);
            File.Delete(path);
        }
    }

    public IEnumerator ClearChunks(string path)
    //Clears orphaned chunks for all experiences inside a path
    //Get a list of the files inside the folder that contains "_DL-PART_" as part of the filename.
    //Will then delete a file per frame until complete not to overwhelm app
    {
        //folder search
        if (Directory.Exists(path))
        {
            string[] allfiles = Directory.GetFiles(path, "*_DL-PART_*.*", SearchOption.AllDirectories);
            //Debug.Log("Checking for orphaned chunks... found: " + allfiles.Length);
            yield return null;
            if (allfiles.Length > 0)
            {
                //iterate over list if any chunks are found
                foreach (string partlyDownloadedFile in allfiles)
                {
                    //Debug.Log("partlyDownloadedFile:  " + partlyDownloadedFile);
                    DeleteOnStorage(partlyDownloadedFile);
                    yield return null;
                }
                //Debug.Log("Cleared " + allfiles.Length + " orphan chunks");
            }
            allfiles = null;
        }
        else
        {
            //Debug.Log("can't clear chunks from path:  " + path + " doesn't exist");
        }
        yield return null;
    }
}
*/