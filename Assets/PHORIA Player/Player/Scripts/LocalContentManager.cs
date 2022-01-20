/*using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;
using UnityEngine.Events;

public class LocalContentManager : MonoBehaviour
{
    [Header("=== Local Content Options ===")]
    //TODO JHPH - eventually read these from the structure.JSON to make it custom per bucket #3
    [Tooltip("Exclude these from initial download - A list of file extentions to skip for the remote / local comparison on start up. Saves large assets coming down if not needed")]
    public List<string> ExcludeTheseExtentions = new List<string>();
    *//* { ".mkv",".mp4",".wav",".mp3" }; *//*

    [Header("=== Local Content Events ====")]
    public UnityEvent LocalListCreated = new UnityEvent();

    public long TotalBytesRequired = 0;

    private bool NeedToFrontLoad = false;
    private int FrontloadHowMany = 0;
    private int Frontloaded = 0;

    public List<string> FrontLoadTheseAssets = new List<string>();

    *//* END VARS ======================================================================================================================== *//*


    //grab files and folders in local persistent storage "_LocalStorage" and map it out.  Used to compare against remote list for changes
    public IEnumerator GetLocalFileList(string localStorage, List<string[]> populateThisList, bool invokeEvent = true)
    {
        //The list of paths and filesizes to return
        Debug.Log("LocalContentManager : GetLocalFileList : ==== Getting LOCAL Listing from: " + localStorage + " =====");
        string[] entries = Directory.GetFileSystemEntries(localStorage, "*", SearchOption.AllDirectories);
        Debug.Log("Entire string for filepath is " + Application.persistentDataPath + localStorage + " with entries =" +entries);
        Array.Sort(entries);

        foreach (var item in entries)
        {

            if (Directory.Exists(item))
            {
                string formattedPath = item.Replace(localStorage, "").Replace("\\", "/") + "/";
                //Debug.Log("CompareRemoteAndLocalAssets : local directory is " + formattedPath);
                populateThisList.Add(new string[2] { formattedPath, "0" });
            }
            else if (File.Exists(item))
            {
                FileInfo file = new FileInfo(item);
                string formattedFile = item.Replace(localStorage, "").Replace("\\", "/");

                //Debug.Log("CompareRemoteAndLocalAssets : local file is " + formattedFile);
                populateThisList.Add(new string[2] { formattedFile, file.Length.ToString() });

            }
            yield return null;
        }
        if (invokeEvent) LocalListCreated.Invoke();
        Debug.Log("LocalContentManager : GetLocalFileList : LocalListCreated Invoked.");
    }

    //public IEnumerator GetEntries()
    //{
    //    foreach(string entry in Directory.)
    //}

    //TODO JHPH - make it wildcards are optional
    // compare the 2 generated lists on path and size and use a list of wildcards if provided
    public IEnumerator CompareRemoteAndLocalFileLists(List<string[]> remoteList, List<string[]> localList)
    {
        //Debug.Log("LocalContentManager : CompareRemoteAndLocalFileLists : Starting comparison");

        *//* Comparison logic
         
            for every item in the remote list
                test for file/folder path and size in the local list
                    if found, its a match, flag it matched and do nothing
                    else not found, flag it not matched and more on 

                after the found/not found:
                test the non matched ones against a list of excluded extentions (wildcards) 
                    if theres no match to exclusion wildcards, it's not needed yet (probably a large video or audio file) so do nothing
                    else, download/create it locally 
        
            
                Go to the next item in the remote list and do it again. 
        *//*

        FrontLoadTheseAssets.Clear();
        FrontLoadTheseAssets.TrimExcess();

        for (int r = 0; r < remoteList.Count; r++)
        //foreach (var remoteSourceItem in remoteList)
        {
            //Debug.Log("========== Comparing REMOTE List item: " + remoteSourceItem[0] + " =============================\n ");

            bool matched = false;

            for (int i = 0; i < localList.Count; i++)
            {
                string[] localCompareItem = localList[i];
                if (localCompareItem[0] == remoteList[r][0]) //remoteSourceItem[0])
                {
                    //if (localCompareItem[1] == remoteSourceItem[1])
                    //{

                    //Debug.Log("CompareRemoteAndLocalAssets : OK >> Match on name "+ localCompareItem[1]);
                    matched = true;
                    break;
                    //}
                    //else
                    //{
                    //    //Debug.Log("CompareRemoteAndLocalAssets : OK >> Match on name");
                    //    //Debug.LogFormat("CompareRemoteAndLocalAssets : ERROR >> path matches but file sizes don't: {0} - {1} / {2}", localCompareItem[0], localCompareItem[1], remoteSourceItem[1]);
                    //}
                }
                else
                {
                    //Debug.Log("CompareRemoteAndLocalAssets : ERROR >> paths do not match");
                }
            }
            if (!matched)
            {
                bool skipped = false;

                //Debug.Log("CompareRemoteAndLocalAssets : ?? >> No match, checking for exclusions for " + remoteSourceItem[0]);
                for (int j = 0; j < ExcludeTheseExtentions.Count; j++)
                {
                    string wildcard = ExcludeTheseExtentions[j];
                    //TODO JHPH - C# definitely has better ways to check extentions, find and use it
                    if (remoteList[r][0].EndsWith(wildcard))
                    {
                        //Debug.Log("CompareRemoteAndLocalAssets : OK >> Exclusion Wildcard Found [" + wildcard + "] on path " + remoteSourceItem[0]);
                        skipped = true;
                        break;
                    }
                }
                if (!skipped)
                {
                    //Debug.Log("CompareRemoteAndLocalAssets : OK >> <color=green>========================================================================================</color>");
                    //Debug.Log("CompareRemoteAndLocalAssets : OK >> <color=green>Commencing local creation or download of #" + FrontloadHowMany + " - " + remoteList[r][0] + "</color>");
                    //Debug.Log("CompareRemoteAndLocalAssets : OK >> ========================================================================================");


                    //download the file from AWS
                    //Debug.Log("CompareRemoteAndLocalAssets : waiting for download " + remoteList[r][0]);
                    NeedToFrontLoad = true;

                    //Debug.Log("CompareRemoteAndLocalAssets : adding " + remoteList[r][0]);
                    FrontLoadTheseAssets.Add((remoteList[r][0]));
                    //ContentManager.Instance.DownloadItemFromPath(remoteList[r][0]);
                    //Debug.Log("Adding front loading event listener for down loader.");
                    //ContentManager.Instance.NoMoreConcurrentDownloads.AddListener(AppManager.Instance.TimelineIntro.StartMotorMoving);
                    TotalBytesRequired += TotalBytesRequired + long.Parse(remoteList[r][1]);
                    yield return null;
                    FrontloadHowMany++;
                }
                else
                {
                    //Debug.Log("CompareRemoteAndLocalAssets : OK >> Not a required item. Don't need to re-create: " + remoteList[r][0]);
                }

            }
            else
            {
                //Debug.Log("CompareRemoteAndLocalAssets : OK >> Already matched on path and size. Don't need to re-create: " + remoteList[r][0]);
            }

            yield return null;

        }
        if (NeedToFrontLoad)
        {
            AppManager.Instance.TimelineIntro.MotorMoving(false);
            AWS.Instance.downloadComplete.AddListener(FrontloadFile);
            //Debug.Log("FrontLoadTheseAssets[Frontloaded]: " + FrontLoadTheseAssets[Frontloaded]);
            AWS.Instance.DownloadItemFromPath(FrontLoadTheseAssets[Frontloaded]);
        }

        //Debug.Log("Finished requesting front loads.");
    }

    private void FrontloadFile()
    {

        //Debug.Log(FrontloadHowMany + " - A file has been completed: #" + Frontloaded + ": " + FrontLoadTheseAssets[Frontloaded]);
        Frontloaded++;
        if (Frontloaded < FrontloadHowMany)
            AWS.Instance.DownloadItemFromPath(FrontLoadTheseAssets[Frontloaded]);
        else
        {
            Debug.Log("That's all she wrote on " + Frontloaded);
            AWS.Instance.downloadComplete.RemoveListener(FrontloadFile);
            FrontLoadTheseAssets.Clear();
            FrontLoadTheseAssets.TrimExcess();
            StartCoroutine(RepopulateLocalFileList());
        }
    }

    public IEnumerator RepopulateLocalFileList()
    {
        AppManager.Instance.LocalFileList.Clear();
        yield return GetLocalFileList(AppManager.Instance.LocalStorage, AppManager.Instance.LocalFileList, false);
        AppManager.Instance.TimelineIntro.StartMotorMoving();
    }

    //tempporary
    public IEnumerator SpaceDownLoadRequest(int noOfFiles)
    {
        float timeToWait = (float)noOfFiles * 2.2f;
        //Debug.Log("SpaceDownLoadRequest : Frontloading for "+ timeToWait);
        yield return new WaitForSeconds(timeToWait);
        AppManager.Instance.TimelineIntro.MotorMoving(true);
        //Debug.Log("SpaceDownLoadRequest : Frontloading waited for exactly " + timeToWait);

    }
}
*/