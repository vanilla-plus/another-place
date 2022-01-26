#undef DEBUG

using System.Threading.Tasks;

using UnityEngine;

using static UnityEngine.Debug;

public class Tile_Window : Tile_Element
{

	public RectTransform rect;

	private const float expandDuration = 0.1666f;

	private float expandVel;

	public override void Awake()
	{
		base.Awake();

		tile.onSelect   += ExpandSizeHandler;
		tile.onDeselect += ShrinkSizeHandler;

		// As suspected, the Mathf.Approx check in SelectIn is accidentally guarding this from firing.
		// It's actually pretty easily fixed, you just need a new Action for "onClick", normals be damned.
		// Reconnect this callback to that new Action instead.
		
	}

	private async void ExpandSizeHandler()
	{
		#if DEBUG
		Log($"[{tile.experience.title}] Expand Start");
		#endif
		
		while (tile.selected &&
		       Mathf.Abs(rect.sizeDelta.x - tile.maxWindowSize) > 0.1f)
		{
			#if DEBUG
			Log($"[{tile.experience.title}] Expand Frame");
			#endif

			rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: rect.sizeDelta.x,
			                                                 target: tile.maxWindowSize,
			                                                 currentVelocity: ref expandVel,
			                                                 smoothTime: expandDuration),
			                             y: rect.sizeDelta.y);

			await Task.Yield();
		}
		
		#if DEBUG
		if (!tile.selected) LogWarning($"[{tile.experience.title}] Expand Interrupted! This tile was de-selected?");

		Log($"[{tile.experience.title}] Expand End");
		#endif
	}
	
	private async void ShrinkSizeHandler()
	{
		#if DEBUG
		Log($"[{tile.experience.title}] Shrink Start");
		#endif

		while (!tile.selected &&
		       Mathf.Abs(rect.sizeDelta.x - tile.minWindowSize) > 0.1f)
		{
			#if DEBUG
			Log($"[{tile.experience.title}] Shrink Frame");
			#endif

			rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: rect.sizeDelta.x,
			                                                 target: tile.minWindowSize,
			                                                 currentVelocity: ref expandVel,
			                                                 smoothTime: expandDuration),
			                             y: rect.sizeDelta.y);

			await Task.Yield();
		}

		#if DEBUG
		if (tile.selected) LogWarning($"[{tile.experience.title}] Shrink Interrupted! This tile was re-selected?");

		Log($"[{tile.experience.title}] Shrink End");
		#endif
	}

}