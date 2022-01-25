using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Amazon.DynamoDBv2.Model;

using AWS;

using SimpleJSON;

using UnityEngine;

using Unity.RemoteConfig;

using static UnityEngine.Debug;

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

			private static string _Cache;
			private static string _Thumbnails;

			public static string Cache      => _Cache;
			public static string Thumbnails => _Thumbnails;


			static Partial()
			{
				_Cache      = Slash + "cache";
				_Thumbnails = Slash + "thumbnails";
			}

		}

		public static class Local
		{

			private static string _Root;
			private static string _Thumbnails;

			public static string Root       => _Root;
			public static string Thumbnails => _Thumbnails;


			static Local()
			{
				_Root       = Application.persistentDataPath + Partial.Cache;
				_Thumbnails = Root                           + Partial.Thumbnails;
			}

		}

		public static class Remote
		{

			private static string _Root;
			private static string _Thumbnails;

			public static string Root       => _Root;
			public static string Thumbnails => _Thumbnails;


			static Remote()
			{
				_Root       = string.Empty;
				_Thumbnails = Root + Partial.Thumbnails;
			}

		}

	}
	
	public static bool IsOnline => Application.internetReachability != NetworkReachability.NotReachable;

	private static bool _ThumbnailsFetched = false;
	private static bool _RemoteConfigFetched = false;

	private static bool _CatalogueFetched = false;
	
	public static List<Experience> Catalogue = new List<Experience>(16);

	public static Action onCatalogueFetched;

	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static async void PreSceneInitialize()
	{

		await WaitForInternetConnection(timeOutHealth: 10,
		                                waitInMilliseconds: 500);
		
		InitializeUnityEnvironment();

		InitializeOvrEnvironment();
		
		InitializeRemoteConfig();
		
		await InitializeContentEnvironment();
		
		FetchCatalogue();
	}


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static async void PostSceneInitialize()
	{
		// Wait for the fetch to complete, or 10 seconds to elapse.
		
		for (var i = 10;
		     i > 0;
		     i--)
			if (!_CatalogueFetched)
				await Task.Delay(1000);

		// Hopefully the Catalogue Fetch has completed by now... because we need to ask each Experience to fetch its remote size.

		foreach (var e in Catalogue) await e.UpdateRemoteByteSize();

		LogError("Here are the download requirements for each experience");
		
//		foreach (var e in Catalogue) Log(e.GetDownloadRequirement);

//		Catalogue[1].DownloadContent();
		
//		AppManager.Instance.Initialise();

//		Menu.i.Initialize();
	}


	private static async Task WaitForInternetConnection(int timeOutHealth = 10,
	                                                    int waitInMilliseconds = 500)
	{
		for (;
			timeOutHealth > 0;
			timeOutHealth--)
		{
			if (IsOnline) return;

			Log("Waiting for internet connection...");

			await Task.Delay(waitInMilliseconds);
		}

		LogWarning("Device is offline?");
	}


	private static void InitializeUnityEnvironment()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		
		Application.targetFrameRate = -1;
		
		UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.4f;
	}

	private static void InitializeOvrEnvironment()
	{
		OVRPlugin.cpuLevel                                  = 0;
		OVRPlugin.gpuLevel                                  = 0;
		OVRManager.cpuLevel                                 = 0;
		OVRManager.gpuLevel                                 = 0;
		
		Log($"OVRManager.sdkVersion:         " + OVRManager.sdkVersion);
		Log($"Target frame rate is +         " + Application.targetFrameRate);
		Log($"OVRManager.batteryTemperature: " + OVRManager.batteryTemperature);
		Log($"OVRManager.gpuUtilSupported:   " + OVRManager.gpuUtilSupported);
		
		Log($"OVRPlugin.cpuLevel: {OVRPlugin.cpuLevel} / OVRPlugin.gpuLevel {OVRPlugin.gpuLevel}");
		Log($"OVRManager.cpuLevel: {OVRManager.cpuLevel} / OVRManager.gpuLevel {OVRManager.gpuLevel}");
	}
	
	private static void InitializeRemoteConfig() => ConfigManager.FetchCompleted += ApplyRemoteConfigSettings;

	private static async Task InitializeContentEnvironment()
	{
//		await ContentManager.BuildFolderLists();
		
//		ContentManager.BuildRootFolder();
		
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

		Log($"Beginning thumbnail fetch from\n{Paths.Remote.Thumbnails}\nto\n{Paths.Local.Thumbnails}");
		
		var d = new S3.Download(Paths.Local.Thumbnails,
		                        Paths.Remote.Thumbnails);

		d.onDownloadProgress += (sender,
		                         args) =>
		                        {
			                        _ThumbnailsFetched = args.isDone;
			                        
			                        if (_ThumbnailsFetched) Log("Thumbnail fetch complete.");
		                        };
	}

	private static void FetchRemoteConfig()
	{
		_RemoteConfigFetched = false;
		
		LogWarning("Beginning remote config fetch");
		
		ConfigManager.FetchConfigs(userAttributes: new userAttributes(),
		                           appAttributes: new appAttributes());
	}


	private static void ApplyRemoteConfigSettings(ConfigResponse configResponse)
	{
		LogWarning("Remote config fetch complete!");
		
		Log($"Remote config request origin [{configResponse.requestOrigin}]");
		
		var json = JSON.Parse(ConfigManager.appConfig.GetJson("manifest"));

		var episodes = json["Episodes"].AsArray;

		Catalogue.Clear();

		foreach (var e in episodes) Catalogue.Add(new Experience(node: e));

		onCatalogueFetched?.Invoke();
		
		_RemoteConfigFetched = true;
	}
	
}