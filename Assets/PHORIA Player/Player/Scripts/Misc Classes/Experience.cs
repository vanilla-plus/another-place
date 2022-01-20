using System;

using System.IO;

using AWS;

using SimpleJSON;

using UnityEngine;

using static Place;

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

	private string EpisodeSubpath => Paths.Slash + title.ToLower();
	
	public string title;
	public string description;
	public string duration;
	public string basePath;
	public string videoFormat;

	public Texture2D thumbnail = new Texture2D(width: 1,
	                                           height: 1);

	public Sprite sprite;

	[SerializeField] public string LocalPath              = string.Empty;
	[SerializeField] public string RemotePath             = string.Empty;
	[SerializeField] public string ThumbnailPath          = string.Empty;
	[SerializeField] public string VideoPath              = string.Empty;
	[SerializeField] public string MaleOnboardingPath     = string.Empty;
	[SerializeField] public string MaleBeginnerPath       = string.Empty;
	[SerializeField] public string MaleIntermediatePath   = string.Empty;
	[SerializeField] public string MaleExpertPath         = string.Empty;
	[SerializeField] public string FemaleOnboardingPath   = string.Empty;
	[SerializeField] public string FemaleBeginnerPath     = string.Empty;
	[SerializeField] public string FemaleIntermediatePath = string.Empty;
	[SerializeField] public string FemaleExpertPath       = string.Empty;

	public Experience(JSONNode node)
	{
		title       = node["Title"].Value;
		description = node["Description"].Value;
		duration    = node["Duration"].Value;
		basePath    = node["basePath"].Value;
		videoFormat = node["Format"].Value;

		Debug.Log("Experience found! " + title);

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
				
		UpdateThumbnail();
	}

	private void UpdateThumbnail()
	{
		if (!File.Exists(ThumbnailPath))
		{
			Debug.LogError($"Local thumbnail file not found for [{title}]. It was expected at the following path:\n{ThumbnailPath}");

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


	public S3.Download DownloadContent() => new S3.Download(localFolderPath: LocalPath,
	                                                        remoteFolderPath: RemotePath);

}