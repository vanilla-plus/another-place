using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

public class Tile_Window : Tile_Element
{

	public RectTransform rect;

	private const float expandDuration = 0.1666f;

	private float expandVel;

	public override void Awake()
	{
		base.Awake();

		tile.onSelectInStart  += ExpandSizeHandler;
		tile.onSelectOutStart += ShrinkSizeHandler;
	}

	private async void ExpandSizeHandler()
	{
		while (tile.selected &&
		       Mathf.Abs(rect.sizeDelta.x - Tile.maxWindowSize) > 0.1f)
		{
			rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: rect.sizeDelta.x,
			                                                 target: Tile.maxWindowSize,
			                                                 currentVelocity: ref expandVel,
			                                                 smoothTime: expandDuration),
			                             y: rect.sizeDelta.y);

			await Task.Yield();
		}
	}


	private async void ShrinkSizeHandler()
	{
		while (!tile.selected &&
		       Mathf.Abs(rect.sizeDelta.x - Tile.minWindowSize) > 0.1f)
		{
			rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: rect.sizeDelta.x,
			                                                 target: Tile.minWindowSize,
			                                                 currentVelocity: ref expandVel,
			                                                 smoothTime: expandDuration),
			                             y: rect.sizeDelta.y);

			await Task.Yield();
		}
	}

}