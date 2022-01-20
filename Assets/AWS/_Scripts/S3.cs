using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace AWS
{
    public static class S3
    {
        public struct DownloadProgressArgs
        {
            public long transferredBytes;
            public long totalBytes;
            public float progress;
            public bool isDone;

            public DownloadProgressArgs(long transferredBytes, long totalBytes, float progress, bool isDone)
            {
                this.transferredBytes = transferredBytes;
                this.totalBytes = totalBytes;
                this.progress = progress;
                this.isDone = isDone;
            }
        }

        public class Download
        {
            public delegate void DownloadProgressHandler(object sender, DownloadProgressArgs downloadProgressArgs);
            public event DownloadProgressHandler onDownloadProgress;
            
            private CancellationTokenSource cancellationTokenSource;

            public Download(string localFolderPath, string remoteFolderPath)
            {
                cancellationTokenSource = S3.DownloadDirectory(localFolderPath, remoteFolderPath, (object sender, DownloadDirectoryProgressArgs e) =>
                {
                    Update(sender, e.TransferredBytes, e.TotalBytes);
                });
            }

            public Download(string localFilePath, string remoteFolderPath, string remoteFileName)
            {
                cancellationTokenSource = S3.DownloadFile(localFilePath, Path.Combine(remoteFolderPath, remoteFileName), (object sender, WriteObjectProgressArgs e) =>
                {
                    Update(sender, e.TransferredBytes, e.TotalBytes);
                });
            }

            private void Update(object sender, long transferredBytes, long totalBytes)
            {
                onDownloadProgress.Invoke(this, new DownloadProgressArgs(transferredBytes, totalBytes, (float)transferredBytes / (float)totalBytes, transferredBytes >= totalBytes));
            }

            public void Cancel()
            {
                cancellationTokenSource.Cancel();
            }
        }

        private const string bucket = "mhoplaceappcontent";
        private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.APSoutheast2;
        private static readonly AmazonS3Client client = new AmazonS3Client(Cognito.credentials, regionEndpoint);
        private static readonly TransferUtility transferUtility = new TransferUtility(client);
        
        public static CancellationTokenSource DownloadFile(string localFilePath, string key, EventHandler<WriteObjectProgressArgs> callback)
        {
            TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest
            {
                FilePath = localFilePath,
                BucketName = bucket,
                Key = key
            };
            request.WriteObjectProgressEvent += callback;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            transferUtility.DownloadAsync(request, cancellationTokenSource.Token);

            return cancellationTokenSource;
        }

        public static CancellationTokenSource DownloadDirectory(string localFolderPath, string key, EventHandler<DownloadDirectoryProgressArgs> callback)
        {
            TransferUtilityDownloadDirectoryRequest request = new TransferUtilityDownloadDirectoryRequest
            {
                LocalDirectory = localFolderPath,
                BucketName = bucket,
                S3Directory = key
            };
            request.DownloadedDirectoryProgressEvent += callback;
            
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            transferUtility.DownloadDirectoryAsync(request, cancellationTokenSource.Token);
            
            return cancellationTokenSource;
        }

        public static async Task<GetObjectMetadataResponse> GetObjectMetadataAsync(string key)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest
            {
                BucketName = bucket,
                Key = key
            };

            return await client.GetObjectMetadataAsync(request);
        }

        public static async Task<ListObjectsV2Response> ListObjectsV2Async(string key)
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = bucket,
                Prefix = key
            };

            return await client.ListObjectsV2Async(request);
        }
    }
}
