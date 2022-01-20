using System;

using UnityEngine;
using UnityEngine.Events;

namespace Vanilla {
	public class VanillaBehaviour : MonoBehaviour {
		[Header("[ Vanilla Behaviour ]")]
		[EnumFlags(4)]
		public DebugFlags debugFlags = (DebugFlags)31;

		public static bool applicationIsQuitting;

		protected void Log(string message) {
			#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (debugFlags.HasFlag(DebugFlags.Logs))
				Debug.Log($"[{gameObject.name}] [{Time.frameCount}] {message}");
			#endif
		}

		protected void Warning(string message) {
			#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (debugFlags.HasFlag(DebugFlags.Warnings))
				Debug.LogWarning($"[{gameObject.name}] [{Time.frameCount}] {message}");
			#endif
		}

		protected void Error(string message) {
			#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (debugFlags.HasFlag(DebugFlags.Errors))
				Debug.LogError($"[{gameObject.name}] [{Time.frameCount}] {message}");
			#endif
		}

		public void OnApplicationQuit() {
			applicationIsQuitting = true;
		}
	}
}