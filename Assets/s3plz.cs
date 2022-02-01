using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

//using AWS;

using Vanilla.StringFormatting;

using static UnityEngine.Debug;

public class s3plz : MonoBehaviour
{

	private const string identityPoolId = "ap-southeast-2:b5d45675-818f-4635-86c6-c8f3915280c2";
	private const string bucket         = "mhoplaceappcontent";

	private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.APSoutheast2; //AWSConfigs.RegionEndpoint;

	public static readonly CognitoAWSCredentials credentials = new CognitoAWSCredentials(identityPoolId: identityPoolId,
	                                                                                     region: regionEndpoint);
	private static readonly AmazonS3Client client = new AmazonS3Client(credentials: credentials,
	                                                                   region: regionEndpoint);

	private static readonly TransferUtility transferUtility = new TransferUtility(client);

	public string experienceName = "antarctica";

	public string localPath;
	
	public ulong  remoteByteSize;
	public string remoteByteSizeString;

	[ContextMenu("Get Size")]
	public async void GetSize()
	{
		remoteByteSize = 0;

		var remoteFolderContents = await client.ListObjectsV2Async(request: new ListObjectsV2Request
		                                                                    {
			                                                                    BucketName = bucket,
			                                                                    Prefix     = experienceName
		                                                                    });

		var remoteObjectCount = remoteFolderContents.S3Objects.Count;
		
		Log($"Found [{remoteObjectCount}] remote objects for [{experienceName}]");

		if (remoteObjectCount == 0) return;		
		
		var output = remoteFolderContents.S3Objects.Sum(o => o.Size);

		remoteByteSize = (ulong)output;

		remoteByteSizeString = remoteByteSize.AsDataSize();
		
		Log($"Remote byte size updated for [{experienceName}] - [{remoteByteSizeString}]");
	}
	
	[ContextMenu("Download")]	
	public async void Download()
	{
		localPath = Application.persistentDataPath + $"/s3plz/{experienceName}";
		
		Log("Starting download");

//		transferUtility.DownloadDirectory(bucketName: bucket,
//		                                  s3Directory: experienceName,
//		                                  localDirectory: localPath);

		var t = transferUtility.DownloadDirectoryAsync(bucketName: bucket,
		                                               s3Directory: experienceName,
		                                               localDirectory: localPath);

//		while (!t.IsCompleted)
//		{
//			Log("Waiting...");
//			
//			await Task.Yield();
//		}
//		
//		await transferUtility.DownloadDirectoryAsync(bucketName: bucket,
//		                                             s3Directory: "antarctica",
//		                                             localDirectory: Application.persistentDataPath + "/s3plz");

		Log("And there it is");
	}

}