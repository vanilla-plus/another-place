using System;

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3.Model;
using Amazon.S3.Transfer;

using SimpleJSON;

using UnityEngine;

using Vanilla.StringFormatting;

using static Place;

using static UnityEngine.Debug;

// Experience/Carousel Tile description:
// The data needed to construct the "experience" or menu/carousel item
// The "experienceRequriedAssets" is a list of file paths from the manifest class that is required for the expereience to be complete
// the experience item/tile code will use that list of file paths to check and if all are there, the experience is complete and can be played

[Serializable]
public class Experience
{

	public JSONNode node;
	

	private const string c_MaleOnboardingFileName   = "male-onboarding.mp3";
	private const string c_MaleBeginnerFileName     = "male-beginner.mp3";
	private const string c_MaleIntermediateFileName = "male-intermediate.mp3";
	private const string c_MaleExpertFileName       = "male-expert.mp3";

	private const string c_FemaleOnboardingFileName   = "female-onboarding.mp3";
	private const string c_FemaleBeginnerFileName     = "female-beginner.mp3";
	private const string c_FemaleIntermediateFileName = "female-intermediate.mp3";
	private const string c_FemaleExpertFileName       = "female-expert.mp3";

	// Apparently we need both of these.
	// EpisodeNameLower is used in assessing remote byte size and can't start with a slash.
	// EpisodeSubpath needs the slash at the start so the download knows where to start.
	private string EpisodeNameLower; 

	private string EpisodeSubpath;

	public string title;
	public string description;
	public string duration;
	public string basePath;
	public string videoFormat;

	public Texture2D thumbnail = new Texture2D(width: 1,
	                                           height: 1);

	public Sprite sprite;
	
	[SerializeField]
	public string LocalPath = string.Empty;
	[SerializeField]
	public string RemotePath = string.Empty;
	[SerializeField]
	public string ThumbnailPath = string.Empty;
	[SerializeField]
	public string VideoPath = string.Empty;
	[SerializeField]
	public string MaleOnboardingPath = string.Empty;
	[SerializeField]
	public string MaleBeginnerPath = string.Empty;
	[SerializeField]
	public string MaleIntermediatePath = string.Empty;
	[SerializeField]
	public string MaleExpertPath = string.Empty;
	[SerializeField]
	public string FemaleOnboardingPath = string.Empty;
	[SerializeField]
	public string FemaleBeginnerPath = string.Empty;
	[SerializeField]
	public string FemaleIntermediatePath = string.Empty;
	[SerializeField]
	public string FemaleExpertPath = string.Empty;

	public ulong  remoteByteSize;
	public string remoteByteSizeString;
	
	public ulong  localByteSize;
	public string localByteSizeString;

	public Action<string> onRemoteByteSizeUpdate;
	public Action<string> onLocalByteSizeUpdate;

	public Action<string> onDownloadRequirementUpdate;

	public Action               onDownloadBegun;
	public Action<ulong, float> onDownloadPacket;
	public Action               onDownloadComplete;

	public Action<bool> onContentAvailabilityChange;

	// S3 downloading
	

	
	private CancellationTokenSource _cancellationTokenSource;
	
	[Header(header: "Current download status")]

	[SerializeField]
	private bool _downloading = false;
	public bool Downloading => _downloading;

	[SerializeField] private bool downloadCompleted = false;
	[SerializeField] private bool downloadCancelled = false;
	[SerializeField] private bool downloadFaulted   = false;
	
	public int    downloadTotalNumberOfFiles               = -1;
	public int    downloadNumberOfFilesDownloaded          = -1;
	public long   downloadTotalBytes                       = -1;
	public long   downloadTransferredBytes                 = -1;
	public string downloadCurrentFile                      = string.Empty;
	public long   downloadTransferredBytesForCurrentFile   = -1;
	public long   downloadTotalNumberOfBytesForCurrentFile = -1;

	public Experience(JSONNode node)
	{
		this.node        = node;
		
		title       = node[aKey: "Title"].Value;
		description = node[aKey: "Description"].Value;
		duration    = node[aKey: "Duration"].Value;
		basePath    = node[aKey: "basePath"].Value;
		videoFormat = node[aKey: "Format"].Value;

		EpisodeNameLower = title.ToLower();
		EpisodeSubpath   = Paths.Slash + EpisodeNameLower;
		
		Log(message: "Experience found! " + title);

		LocalPath              = Paths.Local.Root       + EpisodeSubpath;
		RemotePath             = Paths.Remote.Root      + EpisodeSubpath;
		ThumbnailPath          = Paths.Local.Thumbnails + EpisodeSubpath + ".jpg";
		VideoPath              = LocalPath              + Paths.Slash    + "video" + videoFormat;
		MaleOnboardingPath     = LocalPath              + Paths.Slash    + c_MaleOnboardingFileName;
		MaleBeginnerPath       = LocalPath              + Paths.Slash    + c_MaleBeginnerFileName;
		MaleIntermediatePath   = LocalPath              + Paths.Slash    + c_MaleIntermediateFileName;
		MaleExpertPath         = LocalPath              + Paths.Slash    + c_MaleExpertFileName;
		FemaleOnboardingPath   = LocalPath              + Paths.Slash    + c_FemaleOnboardingFileName;
		FemaleBeginnerPath     = LocalPath              + Paths.Slash    + c_FemaleBeginnerFileName;
		FemaleIntermediatePath = LocalPath              + Paths.Slash    + c_FemaleIntermediateFileName;
		FemaleExpertPath       = LocalPath              + Paths.Slash    + c_FemaleExpertFileName;

		LoadThumbnail();

		UpdateLocalByteSize();
		
//		UpdateRemoteByteSize();

//		Log(GetDownloadRequirement);
	}


	private void LoadThumbnail()
	{
		if (!File.Exists(path: ThumbnailPath))
		{
			LogError(message: $"Local thumbnail file not found for [{title}]. It was expected at the following path:\n{ThumbnailPath}");

			return;
		}

		var thumbnailBytes = File.ReadAllBytes(path: ThumbnailPath);

		thumbnail.LoadImage(data: thumbnailBytes);

		sprite = Sprite.Create(texture: thumbnail,
		                       rect: new Rect(x: 0,
		                                      y: 0,
		                                      width: thumbnail.width,
		                                      height: thumbnail.height),
		                       pivot: new Vector2(x: 0.5f,
		                                          y: 0.5f),
		                       pixelsPerUnit: 100F,
		                       extrude: 0,
		                       meshType: SpriteMeshType.FullRect);
	}


	public void UpdateLocalByteSize()
	{
		localByteSize = Directory.Exists(path: LocalPath) ? (ulong)GetSizeOfDirectory(input: new DirectoryInfo(path: LocalPath)) : 0;

		localByteSizeString = localByteSize.AsDataSize();
		
		Log(message: $"Local byte size updated for [{title}] - [{localByteSizeString}]");

		onLocalByteSizeUpdate?.Invoke(obj: localByteSizeString);
	}


	public long GetSizeOfDirectory(DirectoryInfo input) => input.GetFiles().Sum(selector: file => file.Length) + input.GetDirectories().Sum(selector: GetSizeOfDirectory);

	public string GetDownloadRequirement => (remoteByteSize - localByteSize).AsDataSize();
	
	public bool ContentFullyDownloaded => localByteSize != 0 && localByteSize == remoteByteSize;
	
	public async Task UpdateRemoteByteSize()
	{
		remoteByteSize = 0;

		//		var result = await S3.ListObjectsV2Async(key: EpisodeNameLower);

		var result = await S3.client.ListObjectsV2Async(new ListObjectsV2Request
		                                                {
			                                                BucketName = S3.c_S3BucketName,
			                                                Prefix     = EpisodeNameLower
		                                                });

		var remoteObjectCount = result.S3Objects.Count;
		
		Log(message: $"Found [{remoteObjectCount}] remote objects for [{EpisodeNameLower}]");

		if (remoteObjectCount == 0) return;		
		
		var output = result.S3Objects.Sum(selector: o => o.Size);

		remoteByteSize = (ulong)output;

		remoteByteSizeString = remoteByteSize.AsDataSize();
		
		Log(message: $"Remote byte size updated for [{title}] - [{remoteByteSizeString}]");
		
		onRemoteByteSizeUpdate?.Invoke(obj: remoteByteSizeString);
	}


	public void UpdateContentAvailability() => onContentAvailabilityChange?.Invoke(obj: ContentFullyDownloaded);

	public void UpdateDownloadRequirementText() => onDownloadRequirementUpdate?.Invoke(obj: GetDownloadRequirement);


	public async Task Download()
	{
		if (_downloading)
		{
			LogWarning(message: $"[{title}] is already downloading.");
			
			return;
		}

		_downloading = true;

		downloadCompleted = downloadCancelled = downloadFaulted = false;

		Log($"[{title}] content download begun.");

		onDownloadBegun?.Invoke();

		var request = new TransferUtilityDownloadDirectoryRequest
		              {
			              LocalDirectory = LocalPath,
			              BucketName     = S3.c_S3BucketName,
			              S3Directory    = RemotePath
		              };
		
		// Reset all the download metrics
		
		downloadTotalNumberOfFiles               = -1;
		downloadNumberOfFilesDownloaded          = -1;
		downloadTotalBytes                       = -1;
		downloadTransferredBytes                 = -1;
		downloadTransferredBytesForCurrentFile   = -1;
		downloadTotalNumberOfBytesForCurrentFile = -1;
		
		request.DownloadedDirectoryProgressEvent += OnDownloadDirectoryProgress;

		_cancellationTokenSource = new CancellationTokenSource();

		var t = S3.transferUtility.DownloadDirectoryAsync(request: request,
		                                                  cancellationToken: _cancellationTokenSource.Token);

		// The download takes a few frames to start.
		// We can wait for the first OnDownloadDirectoryProgress to come through until then.

		while (!(downloadCancelled || downloadFaulted) && downloadTransferredBytes == -1) await Task.Yield();
		
		while (!(downloadCompleted || downloadCancelled || downloadFaulted))
		{
			downloadCancelled = t.IsCanceled;
			downloadCompleted = t.IsCompleted;
			downloadFaulted   = t.IsFaulted;

			onDownloadPacket?.Invoke(arg1: (ulong)downloadTransferredBytes,
			                         arg2: (float)downloadTransferredBytes / downloadTotalBytes);

			await Task.Yield();
		}

		request.DownloadedDirectoryProgressEvent -= OnDownloadDirectoryProgress;

		_downloading                             =  false;

		var downloadSuccessful = downloadTransferredBytes == downloadTotalBytes;
		
		if (downloadSuccessful)
		{
			Log($"[{title}] content download complete.");

			onDownloadComplete?.Invoke();
		}

		onContentAvailabilityChange?.Invoke(downloadSuccessful);
	}
	
	private void OnDownloadDirectoryProgress(object s,
	                                         DownloadDirectoryProgressArgs a)
	{
		downloadTotalNumberOfFiles               = a.TotalNumberOfFiles;
		downloadNumberOfFilesDownloaded          = a.NumberOfFilesDownloaded;
		downloadTotalBytes                       = a.TotalBytes;
		downloadTransferredBytes                 = a.TransferredBytes;
		downloadCurrentFile                      = a.CurrentFile;
		downloadTransferredBytesForCurrentFile   = a.TransferredBytesForCurrentFile;
		downloadTotalNumberOfBytesForCurrentFile = a.TotalNumberOfBytesForCurrentFile;
	}
	
	public void CancelDownload()
	{
		if (!_downloading) return;

		_cancellationTokenSource.Cancel();

		_downloading = false;

		_cancellationTokenSource = null;
	}


	public async Task DeleteContent()
	{
		if (!Directory.Exists(path: LocalPath)) return;

		Directory.Delete(path: LocalPath,
		                 recursive: true);
		
		UpdateLocalByteSize();
		
		UpdateDownloadRequirementText();

		// If we don't let the method take a breather here, we seem to get performance spikes on one of the following frames.
		// If we remove this delay and turn the method back into a standard void, the next frames deltaTime is 0.333f (the projects maximum).
		// This may only be possible due to Editor overhead or a Windows-environment specific issue, but it can't hurt to give the delete
		// operation some breathing room anyway.
		
		await Task.Delay(250);
		
		onContentAvailabilityChange?.Invoke(obj: false);

	}

}