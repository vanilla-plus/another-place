using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DownloadQueueEntry : MonoBehaviour
{ 
    public string chunkedFilePath;
    public long startByteRange;
    public long endByteRange;
    public DateTime downloadStartTime;
    public DateTime downloadEndTime;
    public bool isDownloaded;

    public DownloadQueueEntry(string ChunkedFilePath, long StartByteRange, long EndByteRange)
    {
        this.chunkedFilePath = ChunkedFilePath;
        this.startByteRange = StartByteRange;
        this.endByteRange = EndByteRange;
        this.downloadStartTime = DateTime.Now;
        this.downloadEndTime = DateTime.Now;
        this.isDownloaded = false;
    }
}