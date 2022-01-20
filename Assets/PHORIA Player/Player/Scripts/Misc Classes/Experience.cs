using System;

using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AWS;

using JetBrains.Annotations;

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

	public S3.Download download;

	public ulong  remoteByteSize;
	public string remoteByteSizeString;
	
	public ulong  localByteSize;
	public string localByteSizeString;

	public Action<string> onRemoteByteSizeUpdate;
	public Action<string> onLocalByteSizeUpdate;
	
	public Action               onDownloadBegun;
	public Action<ulong, float> onDownloadPacket;
	public Action               onDownloadComplete;

	public Action<bool> onContentAvailabilityChange;

	public Experience(JSONNode node)
	{
		title       = node["Title"].Value;
		description = node["Description"].Value;
		duration    = node["Duration"].Value;
		basePath    = node["basePath"].Value;
		videoFormat = node["Format"].Value;

		EpisodeNameLower = title.ToLower();
		EpisodeSubpath   = Paths.Slash + EpisodeNameLower;
		
		Log("Experience found! " + title);

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
		if (!File.Exists(ThumbnailPath))
		{
			LogError($"Local thumbnail file not found for [{title}]. It was expected at the following path:\n{ThumbnailPath}");

			return;
		}

		var thumbnailBytes = File.ReadAllBytes(ThumbnailPath);

		thumbnail.LoadImage(thumbnailBytes);

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
		localByteSize = Directory.Exists(LocalPath) ? (ulong)GetSizeOfDirectory(new DirectoryInfo(LocalPath)) : 0;

		localByteSizeString = localByteSize.AsDataSize();
		
		Log($"Local byte size updated for [{title}] - [{localByteSizeString}]");

		onLocalByteSizeUpdate?.Invoke(localByteSizeString);
	}


	public long GetSizeOfDirectory(DirectoryInfo input) => input.GetFiles().Sum(file => file.Length) + input.GetDirectories().Sum(GetSizeOfDirectory);

	public string GetDownloadRequirement => (remoteByteSize - localByteSize).AsDataSize();
	
	public bool ContentFullyDownloaded => localByteSize != 0 && localByteSize == remoteByteSize;
	
	public async Task UpdateRemoteByteSize()
	{
		remoteByteSize = 0;
		
		var result = await S3.ListObjectsV2Async(EpisodeNameLower);

		var remoteObjectCount = result.S3Objects.Count;
		
		Log($"Found [{remoteObjectCount}] remote objects for [{EpisodeNameLower}]");

		if (remoteObjectCount == 0) return;		
		
		var output = result.S3Objects.Sum(o => o.Size);

		remoteByteSize = (ulong)output;

		remoteByteSizeString = remoteByteSize.AsDataSize();
		
		Log($"Remote byte size updated for [{title}] - [{remoteByteSizeString}]");
		
		onRemoteByteSizeUpdate?.Invoke(remoteByteSizeString);
	}



	public void DownloadContent()
	{
		download = GetDownload();

		onDownloadBegun?.Invoke();

		download.onDownloadProgress += DownloadProgressHandler;
	}


	private void DownloadProgressHandler(object sender,
	                                     S3.DownloadProgressArgs a)
	{
		onDownloadPacket?.Invoke(arg1: (ulong)a.transferredBytes,
		                         arg2: a.progress);

		if (!a.isDone) return;

		onDownloadComplete?.Invoke();
		
		onContentAvailabilityChange?.Invoke(true);

		download.onDownloadProgress -= DownloadProgressHandler;
	}

	
	private S3.Download GetDownload() => new S3.Download(localFolderPath: LocalPath,
	                                                     remoteFolderPath: RemotePath);


	public void DeleteContent()
	{
		if (!Directory.Exists(path: LocalPath)) return;

		Directory.Delete(path: LocalPath,
		                 recursive: true);
		
		onContentAvailabilityChange?.Invoke(false);
	}

}