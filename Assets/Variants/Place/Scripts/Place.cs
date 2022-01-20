using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Amazon.DynamoDBv2.Model;

using AWS;

using SimpleJSON;

using UnityEngine;

using Unity.RemoteConfig;

public static class Place
{

	public struct userAttributes { }

	public struct appAttributes { }

	public static class Paths
	{

		// The expected remote directory structure is as follows on a per-episode basis:
		
		//		thumbnails/
		//		thumbnails/<episode>
		//		thumbnails/<episode>
		//		thumbnails/<episode>
		//
		//		cache/
		//		cache/<episode>/
		//		cache/<episode>/video<manifest.videoFormat>
		//		cache/<episode>/male0.mp3
		//		cache/<episode>/male1.mp3
		//		cache/<episode>/male2.mp3
		//		cache/<episode>/male3.mp3
		//		cache/<episode>/female0.mp3
		//		cache/<episode>/female1.mp3
		//		cache/<episode>/female2.mp3
		//		cache/<episode>/female3.mp3
		
		public static char Slash => Path.DirectorySeparatorChar;

		public static class Partial
		{

			public static readonly string Cache      = Slash + "cache";
			public static readonly string Thumbnails = Slash + "thumbnails";
			
		}
		
		public static class Local
		{

			private static string _Root       = string.Empty;
			private static string _Cache      = string.Empty;
			private static string _Thumbnails = string.Empty;

			public static string Root       => string.IsNullOrEmpty(_Root) 			? _Root 		= Application.persistentDataPath + Partial.Cache		: _Root;
			public static string Cache      => string.IsNullOrEmpty(_Cache) 		? _Cache 		= Root                           + Partial.Cache 		: _Cache;
			public static string Thumbnails => string.IsNullOrEmpty(_Thumbnails) 	? _Thumbnails	= Root                           + Partial.Thumbnails 	: _Thumbnails;

		}
		
		public static class Remote
		{

			private static string _Root               = string.Empty;
//			private static string _Cache              = string.Empty;
			private static string _Thumbnails         = string.Empty;

			public static string Root       => _Root;
//			public static string Cache      => Root;
			public static string Thumbnails => string.IsNullOrEmpty(_Thumbnails) 	? _Thumbnails 	= Root + Partial.Thumbnails 	: _Thumbnails;

		}

	}
	
	public static bool IsOnline => Application.internetReachability != NetworkReachability.NotReachable;

	private static bool _ThumbnailsFetched = false;
	private static bool _RemoteConfigFetched = false;

	private static bool _CatalogueFetched = false;
	
	public static List<Experience> Catalogue = new List<Experience>(16);
	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static async void Initialize()
	{
		// Wait til we have an internet connection or 5 seconds is up
		
		var timeOutHealth = 10;
		
		while (!IsOnline)
		{
			if (--timeOutHealth > 0)
			{
				Debug.Log("Waiting for internet connection...");

				await Task.Delay(500);
			}
			else
			{
				Debug.LogWarning("Device is offline?");

				break;
			}
		}
		
		// Subscribe remote config fetch handler 

		ConfigManager.SetEnvironmentID("a5e18688-a558-4d50-8420-e71bb16b8469");
		
		ConfigManager.FetchCompleted += ApplyRemoteConfigSettings;
		
		FetchCatalogue();
	}


	public static async void FetchCatalogue()
	{
		_CatalogueFetched = false;
		
		// Fetch thumbnails and wait until it's done.

		FetchThumbnails();
		
		while (!_ThumbnailsFetched) await Task.Delay(500);
		
		// Fetch remote config and wait until it's done.
		
		FetchRemoteConfig();
		
		while (!_RemoteConfigFetched) await Task.Delay(500);

		_CatalogueFetched = true;
	}
	
	private static void FetchThumbnails()
	{
		_ThumbnailsFetched = false;

		Debug.Log($"Beginning thumbnail fetch from\n{Paths.Remote.Thumbnails}\nto\n{Paths.Local.Thumbnails}");
		
		var d = new S3.Download(Paths.Local.Thumbnails,
		                        Paths.Remote.Thumbnails);

		d.onDownloadProgress += (sender,
		                         args) =>
		                        {
			                        _ThumbnailsFetched = args.isDone;
			                        
			                        if (_ThumbnailsFetched) Debug.Log("Thumbnail fetch complete.");
		                        };
	}

	private static void FetchRemoteConfig()
	{
		_RemoteConfigFetched = false;
		
		Debug.LogWarning("Beginning remote config fetch");
		
		ConfigManager.FetchConfigs(userAttributes: new userAttributes(),
		                           appAttributes: new appAttributes());
	}


	private static void ApplyRemoteConfigSettings(ConfigResponse configResponse)
	{
		Debug.LogWarning("Remote config fetch complete!");
		
		Debug.Log($"Remote config request origin [{configResponse.requestOrigin}]");
		
		var json = JSON.Parse(ConfigManager.appConfig.GetJson("manifest"));

		var episodes = json["Episodes"].AsArray;

		Catalogue.Clear();

		foreach (var e in episodes) Catalogue.Add(new Experience(node: e));

		AppManager.Instance.ExperienceList = Catalogue;
		
		_RemoteConfigFetched = true;
	}
	
}